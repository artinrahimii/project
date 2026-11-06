import tempfile
import unittest
from pathlib import Path

from validate_coverage_artifacts import (
    parse_cobertura,
    parse_critical_log,
    parse_junit_directories,
    validate_html_report,
)


class CoverageArtifactValidationTests(unittest.TestCase):
    def test_cobertura_report_exposes_real_line_counters(self):
        with tempfile.TemporaryDirectory() as temporary:
            report = Path(temporary) / "coverage.xml"
            report.write_text(
                '<coverage lines-valid="10" lines-covered="7" line-rate="0.7"/>',
                encoding="utf-8",
            )
            result = parse_cobertura(report)
            self.assertEqual(result["lines_valid"], 10)
            self.assertEqual(result["lines_covered"], 7)
            self.assertEqual(result["line_coverage_percent"], 70.0)

    def test_cobertura_without_executable_lines_is_rejected(self):
        with tempfile.TemporaryDirectory() as temporary:
            report = Path(temporary) / "coverage.xml"
            report.write_text(
                '<coverage lines-valid="0" lines-covered="0" line-rate="0"/>',
                encoding="utf-8",
            )
            with self.assertRaises(ValueError):
                parse_cobertura(report)

    def test_html_coverage_report_must_be_complete_html(self):
        with tempfile.TemporaryDirectory() as temporary:
            report = Path(temporary) / "index.html"
            report.write_text("<html><body>" + ("coverage " * 30) + "</body></html>", encoding="utf-8")
            self.assertGreater(validate_html_report(report)["size_bytes"], 100)
            report.write_text("not html", encoding="utf-8")
            with self.assertRaises(ValueError):
                validate_html_report(report)

    def test_junit_summary_counts_passed_skipped_and_failure_states(self):
        with tempfile.TemporaryDirectory() as temporary:
            results = Path(temporary) / "results"
            results.mkdir()
            (results / "TEST-suite.xml").write_text(
                '<testsuite><testcase name="pass"/>'
                '<testcase name="skip"><skipped/></testcase></testsuite>',
                encoding="utf-8",
            )
            summary = parse_junit_directories([results])
            self.assertEqual(summary["tests"], 2)
            self.assertEqual(summary["passed"], 1)
            self.assertEqual(summary["skipped"], 1)
            self.assertEqual(summary["failures"], 0)

    def test_junit_failure_is_not_summarized_as_success(self):
        with tempfile.TemporaryDirectory() as temporary:
            results = Path(temporary) / "results"
            results.mkdir()
            (results / "TEST-suite.xml").write_text(
                '<testsuite><testcase name="bad"><failure/></testcase></testsuite>',
                encoding="utf-8",
            )
            with self.assertRaises(ValueError):
                parse_junit_directories([results])

    def test_critical_log_requires_an_explicit_pass_marker(self):
        with tempfile.TemporaryDirectory() as temporary:
            log = Path(temporary) / "critical.log"
            log.write_text("PASS: lifecycle suite\n", encoding="utf-8")
            self.assertEqual(parse_critical_log(log)["pass_markers"], 1)
            log.write_text("completed without a pass marker\n", encoding="utf-8")
            with self.assertRaises(ValueError):
                parse_critical_log(log)


if __name__ == "__main__":
    unittest.main()
