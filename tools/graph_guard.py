#!/usr/bin/env python3
"""Audit project-graph.md and reject unsupported task-completion claims."""
from __future__ import annotations
import argparse, json, re, sys
from collections import defaultdict, deque
from dataclasses import dataclass
from pathlib import Path

NODE_STATUSES = {"not_started", "in_progress", "done", "blocked"}
EDGE_CLASSES = {"hard_dependency", "contract_or_interface_dependency", "sequencing_only", "unreviewed"}
EDGE_STATES = {"unreviewed", "validated", "disputed"}

@dataclass(frozen=True)
class Finding:
    severity: str
    code: str
    message: str
    def __str__(self):
        return f"{self.severity} {self.code}: {self.message}"

def extract_graph(markdown: str) -> dict:
    """Find a fenced JSON object containing node and edge arrays."""
    for match in re.finditer(r"\x60{3}json\s*([\s\S]*?)\x60{3}", markdown, re.IGNORECASE):
        try: graph = json.loads(match.group(1))
        except json.JSONDecodeError: continue
        if isinstance(graph, dict) and isinstance(graph.get("nodes"), list) and isinstance(graph.get("edges"), list):
            return graph
    raise ValueError("No fenced JSON graph with both 'nodes' and 'edges' was found.")

def _list(value):
    return value if isinstance(value, list) else []

def _repo_path(root: Path, value):
    if not isinstance(value, str) or not value.strip(): return None
    path = Path(value)
    if path.is_absolute() or ".." in path.parts: return None
    return root / path

def validate_graph(graph: dict, repo_root: Path | None = None) -> list[Finding]:
    """Check graph integrity; for 'done' tasks, require real files and evidence."""
    out = []
    nodes, edges = graph.get("nodes"), graph.get("edges")
    if not isinstance(nodes, list) or not isinstance(edges, list):
        return [Finding("ERROR", "GRAPH_SHAPE", "'nodes' and 'edges' must be arrays.")]
    ids = [n.get("id") for n in nodes if isinstance(n, dict) and isinstance(n.get("id"), str)]
    known = set(ids)
    if len(ids) != len(nodes): out.append(Finding("ERROR", "NODE_ID_MISSING", "Every node must have a string id."))
    if len(ids) != len(known): out.append(Finding("ERROR", "DUPLICATE_NODE_ID", "Task IDs are not unique."))
    src = graph.get("source")
    declared = src.get("task_count") if isinstance(src, dict) else None
    if declared is not None and declared != len(nodes):
        out.append(Finding("ERROR", "TASK_COUNT_MISMATCH", f"source.task_count={declared}; nodes={len(nodes)}."))

    for n in nodes:
        if not isinstance(n, dict) or not isinstance(n.get("id"), str): continue
        tid, status = n["id"], n.get("status")
        if status not in NODE_STATUSES: out.append(Finding("ERROR", "INVALID_NODE_STATUS", f"{tid}: {status!r}."))
        summary = n.get("execution_summary") if isinstance(n.get("execution_summary"), dict) else {}
        history = _list(n.get("execution_history"))
        count = summary.get("attempt_count")
        if isinstance(count, int) and count != len(history):
            out.append(Finding("ERROR", "ATTEMPT_COUNT_MISMATCH", f"{tid}: summary={count}, history={len(history)}."))
        failed = sum(isinstance(h, dict) and h.get("result") == "failed" for h in history)
        failed_count = summary.get("failed_attempt_count")
        if isinstance(failed_count, int) and failed_count != failed:
            out.append(Finding("ERROR", "FAILED_ATTEMPT_COUNT_MISMATCH", f"{tid}: summary={failed_count}, history={failed}."))
        deps = n.get("depends_on")
        if deps is not None and not isinstance(deps, list): out.append(Finding("ERROR", "INVALID_DEPENDS_ON", f"{tid}: depends_on must be an array."))
        for dep in _list(deps):
            if isinstance(dep, str) and dep not in known:
                out.append(Finding("WARNING", "UNMAPPED_DEPENDENCY", f"{tid}: {dep!r} is not a task ID; preserved as metadata."))
        if status == "done":
            paths, evidence = _list(n.get("actual_output_paths")), _list(n.get("evidence"))
            tests = _list(summary.get("tests_passed"))
            succeeded = [h for h in history if isinstance(h, dict) and h.get("result") == "succeeded"]
            if not paths: out.append(Finding("ERROR", "DONE_WITHOUT_OUTPUT", f"{tid} is done but has no actual_output_paths."))
            if not evidence: out.append(Finding("ERROR", "DONE_WITHOUT_EVIDENCE", f"{tid} is done but has no evidence."))
            if not tests: out.append(Finding("ERROR", "DONE_WITHOUT_PASSED_TESTS", f"{tid} has no recorded passing tests."))
            if not succeeded: out.append(Finding("ERROR", "DONE_WITHOUT_SUCCESSFUL_ATTEMPT", f"{tid} has no succeeded execution-history entry."))
            if repo_root is not None:
                for item in paths:
                    p = _repo_path(repo_root, item)
                    if p is None: out.append(Finding("ERROR", "UNSAFE_OUTPUT_PATH", f"{tid}: invalid output path {item!r}."))
                    elif not p.is_file(): out.append(Finding("ERROR", "MISSING_OUTPUT_FILE", f"{tid}: output file not found: {item!r}."))
                for item in evidence:
                    if not isinstance(item, str) or not item.strip():
                        out.append(Finding("ERROR", "INVALID_EVIDENCE", f"{tid}: evidence must be non-empty text.")); continue
                    if "/" in item or "\\" in item:
                        p = _repo_path(repo_root, item)
                        if p is None or not p.is_file(): out.append(Finding("ERROR", "MISSING_EVIDENCE_FILE", f"{tid}: evidence file not found: {item!r}."))

    adj, indegree = defaultdict(list), {tid: 0 for tid in known}
    for i, e in enumerate(edges):
        if not isinstance(e, dict): out.append(Finding("ERROR", "INVALID_EDGE", f"Edge {i} is not an object.")); continue
        a, b = e.get("from"), e.get("to")
        if a not in known or b not in known:
            out.append(Finding("ERROR", "DANGLING_EDGE", f"Edge {a!r} -> {b!r} has an unknown endpoint.")); continue
        if e.get("dependency_classification") not in EDGE_CLASSES:
            out.append(Finding("ERROR", "INVALID_EDGE_CLASSIFICATION", f"{a} -> {b}: unsupported class."))
        state = e.get("validation_status")
        if state not in EDGE_STATES: out.append(Finding("ERROR", "INVALID_EDGE_VALIDATION_STATUS", f"{a} -> {b}: unsupported validation state."))
        if state == "validated" and (not isinstance(e.get("rationale"), str) or not e["rationale"].strip() or not _list(e.get("evidence"))):
            out.append(Finding("ERROR", "VALIDATED_EDGE_WITHOUT_EVIDENCE", f"{a} -> {b} is validated without rationale and evidence."))
        adj[a].append(b); indegree[b] += 1
    queue = deque(k for k, d in indegree.items() if d == 0); visited = 0
    while queue:
        current = queue.popleft(); visited += 1
        for target in adj[current]:
            indegree[target] -= 1
            if indegree[target] == 0: queue.append(target)
    if visited != len(known): out.append(Finding("ERROR", "DEPENDENCY_CYCLE", "Dependency edges contain a cycle."))
    return out

def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("graph", type=Path, help="Markdown file containing graph JSON")
    parser.add_argument("--repo-root", type=Path, help="Also check actual output/evidence file existence")
    parser.add_argument("--strict", action="store_true", help="Treat warnings as failures")
    args = parser.parse_args(argv)
    try: graph = extract_graph(args.graph.read_text(encoding="utf-8"))
    except (OSError, UnicodeError, ValueError) as exc:
        print(f"ERROR GRAPH_READ: {exc}", file=sys.stderr); return 2
    findings = validate_graph(graph, args.repo_root.resolve() if args.repo_root else None)
    print(f"Graph guard: {len(graph['nodes'])} tasks, {len(graph['edges'])} edges.")
    for f in findings: print(f)
    errors = sum(f.severity == "ERROR" for f in findings)
    warnings = sum(f.severity == "WARNING" for f in findings)
    print(f"Result: {errors} error(s), {warnings} warning(s).")
    return 1 if errors or (args.strict and warnings) else 0

if __name__ == "__main__": raise SystemExit(main())
