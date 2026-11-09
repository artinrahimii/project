# WPF generated-resource regression failure — 2026-11-10

- Quality gate run: [42704516200](https://github.com/artinrahimii/project/actions/runs/42704516200), commit `V 3.1.131`.
- Result: 489 tests, 1 failure. The failing test was `test_wpf_button_string_content_renders_with_contrasting_foreground`.
- The failure came from comparing the entire freshly generated WPF dictionary byte-for-byte with the tracked file. The generator output included the ContentTemplate, while the tracked XAML still contained the prior Button style without it. The generator itself was not failing.
- A013 updates the tracked resource to include the template and tests both files semantically (valid XML, ContentTemplate present, TextBlock foreground bound to PureWhiteBrush). Actual button-label rendering still requires inspection of the next WPF screenshot.
