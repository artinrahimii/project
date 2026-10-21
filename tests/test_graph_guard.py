import json
import sys
import unittest
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from tools.graph_guard import extract_graph, validate_graph

def graph_fixture():
    return {
        "source": {"task_count": 2},
        "nodes": [
            {"id":"T0001","status":"not_started","depends_on":[],"actual_output_paths":[],"evidence":[],"execution_history":[],"execution_summary":{"attempt_count":0,"failed_attempt_count":0,"tests_passed":[]}},
            {"id":"T0002","status":"not_started","depends_on":["T0001"],"actual_output_paths":[],"evidence":[],"execution_history":[],"execution_summary":{"attempt_count":0,"failed_attempt_count":0,"tests_passed":[]}},
        ],
        "edges": [{"from":"T0001","to":"T0002","type":"blocks","dependency_classification":"unreviewed","validation_status":"unreviewed","rationale":"","evidence":[]}],
    }

class ExtractTests(unittest.TestCase):
    def test_extracts_fenced_json(self):
        g = graph_fixture(); fence = chr(96) * 3
        self.assertEqual(extract_graph("# graph\n"+fence+"json\n"+json.dumps(g)+"\n"+fence), g)
    def test_missing_graph_raises(self):
        with self.assertRaises(ValueError): extract_graph("# no graph")

class ValidateTests(unittest.TestCase):
    def test_valid_graph(self):
        self.assertEqual(validate_graph(graph_fixture()), [])
    def test_duplicate_ids(self):
        g=graph_fixture(); g["nodes"][1]["id"]="T0001"
        self.assertIn("DUPLICATE_NODE_ID", {x.code for x in validate_graph(g)})
    def test_dangling_edge(self):
        g=graph_fixture(); g["edges"][0]["to"]="T9999"
        self.assertIn("DANGLING_EDGE", {x.code for x in validate_graph(g)})
    def test_done_requires_output_evidence_tests_and_success(self):
        g=graph_fixture(); g["nodes"][0]["status"]="done"
        codes={x.code for x in validate_graph(g)}
        self.assertTrue({"DONE_WITHOUT_OUTPUT","DONE_WITHOUT_EVIDENCE","DONE_WITHOUT_PASSED_TESTS","DONE_WITHOUT_SUCCESSFUL_ATTEMPT"}.issubset(codes))
    def test_validated_edge_requires_rationale_and_evidence(self):
        g=graph_fixture(); g["edges"][0]["validation_status"]="validated"
        self.assertIn("VALIDATED_EDGE_WITHOUT_EVIDENCE", {x.code for x in validate_graph(g)})
    def test_unmapped_dependency_is_warning_and_preserved(self):
        g=graph_fixture(); g["nodes"][0]["depends_on"]=["external prerequisite text"]
        findings=validate_graph(g)
        self.assertIn("UNMAPPED_DEPENDENCY", {x.code for x in findings})
        self.assertEqual(g["nodes"][0]["depends_on"],["external prerequisite text"])
    def test_cycle_detected(self):
        g=graph_fixture(); g["edges"].append({"from":"T0002","to":"T0001","type":"blocks","dependency_classification":"unreviewed","validation_status":"unreviewed","rationale":"","evidence":[]})
        self.assertIn("DEPENDENCY_CYCLE", {x.code for x in validate_graph(g)})
    def test_done_passes_when_actual_outputs_and_evidence_exist(self):
        import tempfile
        g=graph_fixture(); node=g["nodes"][0]
        node.update({"status":"done", "actual_output_paths":["output.txt"], "acceptance":"Output and evidence are present.",
                     "evidence":["report.md", "tests/test_graph_guard.py"],
                     "execution_history":[{"result":"succeeded","tests":["unit tests passed"]}],
                     "execution_summary":{"attempt_count":1,"failed_attempt_count":0,"tests_passed":["unit tests passed"]}})
        with tempfile.TemporaryDirectory() as td:
            root=Path(td); (root/"output.txt").write_text("real output"); (root/"report.md").write_text("evidence")
            (root/"tests").mkdir(); (root/"tests"/"test_graph_guard.py").write_text("test source")
            node["evidence"].append("tests/test_graph_guard.py")
            self.assertEqual(validate_graph(g, root), [])
    def test_done_reports_missing_actual_file(self):
        import tempfile
        g=graph_fixture(); node=g["nodes"][0]
        node.update({"status":"done", "actual_output_paths":["missing.txt"], "evidence":["report.md"],
                     "execution_history":[{"result":"succeeded"}],
                     "execution_summary":{"attempt_count":1,"failed_attempt_count":0,"tests_passed":["unit tests passed"]}})
        with tempfile.TemporaryDirectory() as td:
            (Path(td)/"report.md").write_text("evidence")
            self.assertIn("MISSING_OUTPUT_FILE", {x.code for x in validate_graph(g, Path(td))})


    def test_done_requires_acceptance_criteria(self):
        g=graph_fixture(); node=g["nodes"][0]
        node.update({"status":"done","actual_output_paths":["output.txt"],"evidence":["report.md","tests/test_output.py"],
                     "execution_history":[{"result":"succeeded"}],
                     "execution_summary":{"attempt_count":1,"failed_attempt_count":0,"tests_passed":["unit tests passed"]}})
        node.pop("acceptance",None)
        self.assertIn("DONE_WITHOUT_ACCEPTANCE_CRITERIA", {x.code for x in validate_graph(g)})
    def test_done_rejects_placeholder_implementation_marker(self):
        import tempfile
        g=graph_fixture(); node=g["nodes"][0]
        node.update({"status":"done","actual_output_paths":["src/engine.py"],"acceptance":"Engine performs real work.",
                     "evidence":["report.md","tests/test_engine.py"],
                     "execution_history":[{"result":"succeeded"}],
                     "execution_summary":{"attempt_count":1,"failed_attempt_count":0,"tests_passed":["unit tests passed"]}})
        with tempfile.TemporaryDirectory() as td:
            root=Path(td); (root/"src").mkdir(); (root/"tests").mkdir()
            (root/"src"/"engine.py").write_text("def infer():\\n    raise NotImplementedError()\\n")
            (root/"report.md").write_text("reviewed evidence")
            (root/"tests"/"test_engine.py").write_text("def test_engine(): pass")
            codes={x.code for x in validate_graph(g, root)}
            self.assertIn("DONE_WITH_PLACEHOLDER_IMPLEMENTATION", codes)

    def test_done_accepts_code_without_known_placeholder_markers(self):
        import tempfile
        g=graph_fixture(); node=g["nodes"][0]
        node.update({"status":"done","actual_output_paths":["src/engine.py"],"acceptance":"Engine returns computed output.",
                     "evidence":["report.md","tests/test_engine.py"],
                     "execution_history":[{"result":"succeeded"}],
                     "execution_summary":{"attempt_count":1,"failed_attempt_count":0,"tests_passed":["unit tests passed"]}})
        with tempfile.TemporaryDirectory() as td:
            root=Path(td); (root/"src").mkdir(); (root/"tests").mkdir()
            (root/"src"/"engine.py").write_text("def infer(value):\\n    return value.strip().upper()\\n")
            (root/"report.md").write_text("reviewed evidence")
            (root/"tests"/"test_engine.py").write_text("def test_engine(): assert True")
            self.assertEqual(validate_graph(g, root), [])

    def test_done_requires_test_source_evidence(self):
        g=graph_fixture(); node=g["nodes"][0]
        node.update({"status":"done","actual_output_paths":["output.txt"],"acceptance":"Output is valid.",
                     "evidence":["report.md"],"execution_history":[{"result":"succeeded"}],
                     "execution_summary":{"attempt_count":1,"failed_attempt_count":0,"tests_passed":["unit tests passed"]}})
        self.assertIn("DONE_WITHOUT_TEST_FILE_EVIDENCE", {x.code for x in validate_graph(g)})

if __name__ == "__main__": unittest.main()
