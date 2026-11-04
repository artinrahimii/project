# T0186 — شواهد اجرا، شکست CI و اصلاح دفتر گراف

## Snapshot معتبر
- Commit: `ea0edc3db0addf1f46c38663119b5ab33d8c3fe5`، شاخه main، پیام V 3.0.874.
- Git tree: `910a6c42f7edfed88a231dde7bd6676df3333de6`.
- Graph blob از tree همان commit استخراج و کامل دریافت شد؛ JSON معتبر با ۲۳۰ task و ۴۲۲ edge.
- وضعیت snapshot: ۱۸۴ done، ۲ deferred_final، ۱ in_progress، ۴۳ not_started.

## CI V 3.0.874
- Workflow quality run `41816319735`, job `126527849431`.
- ۳۷۱ آزمون Python: موفق.
- `python -m compileall -q tools tests`: موفق.
- `python tools/graph_guard.py "step three/project-graph.md"`: ناموفق با `FAILED_ATTEMPT_COUNT_MISMATCH: T0186: summary=1, history=0`.
- علت: guard تعداد attemptهایی را می‌شمارد که `result == failed`؛ دو تلاش ثبت‌شدهٔ T0186 هر دو `partial` هستند. بنابراین مقدار صحیح `failed_attempt_count` صفر است.
- Container run `41816319760`: موفق.
- Android job `126527849590`، Windows job `126527967724` و HostLifecycle job `126527852877` هنگام ثبت این مدخل هنوز در حال اجرا بودند.

## اصلاح A003
- `execution_summary.failed_attempt_count` به صفر تغییر کرد تا با تعداد history entryهای دارای `result=failed` منطبق باشد.
- شکست graph guard و اقدام اصلاحی در history ثبت شد؛ status وظیفه همچنان in_progress است.
- بررسی مجدد validator باید روی commit بعدی main انجام شود.
