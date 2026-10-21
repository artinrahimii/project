# Acceptance and evidence policy

This file defines the first executable acceptance gate for Step Three. It is a project-quality gate, not a claim that the Windows host, Android client, or P2P inference product already exists.

## Task completion gate

A graph task may be marked done only when all of the following are true:

1. Its real output files are listed in actual_output_paths and exist in the repository.
2. Its evidence field points to reviewable evidence.
3. At least one relevant test/check is recorded in execution_summary.tests_passed.
4. execution_history contains a succeeded attempt with the real command/test result; summary attempt counters agree with the history.
5. All graph edges still reference valid task IDs, and a validated edge has a written rationale and evidence.

Run the graph guard locally with Python 3.11 or later:

    python -m unittest discover -s tests -v
    python tools/graph_guard.py "step three/project-graph.md"
    python tools/graph_guard.py "step three/project-graph.md" --repo-root .
    
Warnings such as a free-text prerequisite that does not resolve to a task ID remain visible and are not silently converted into task dependencies. Use --strict when warnings must fail a validation run. Passing guard tests do not prove that application features work; each feature still needs its own implementation-specific acceptance tests. Final Windows, Android, and P2P requirements will be detailed with their dedicated task.

## Task T0001 scope

The deliverable for T0001 is the graph guard, its automated tests, and this acceptance policy. The guard uses only the Python standard library. It checks graph structure and rejects completion claims missing outputs, evidence, passing tests, or a successful recorded attempt.
