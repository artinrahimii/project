# ماتریس پوشش آزمون UPD-S1-019

- خط مبنای دقیق: `235dff60a9f18f45ba4350203941ed6594847aee` — `V 3.1.206`.
- گراف به‌روزرسانی: ۲۴ گره و ۸۰ یال؛ هر ۸۰ یال validated؛ گراف DAG است.
- پیش‌نیازهای مستقل UPD-S1-019 تکمیل‌اند. UPD-S1-011 در وضعیت deferred_final قرار دارد و مجوز downstream آن صریحاً اجرای دامنهٔ خودکار UPD-S1-019 را مجاز می‌کند؛ این امر پذیرش endpoint عمومی را تأیید نمی‌کند.
- شکاف یافت‌شده: Android peer-trust store پیش از این تغییر، unknown peer و revoke را آزمون می‌کرد؛ اما آزمون اختصاصی برای عدم انتقال trust از PeerId قبلی به PeerId حاصل از چرخش کلید میزبان نداشت. آزمون `rotatedHostIdentityDoesNotInheritPriorTrustAndRequiresFreshConfirmation` افزوده شد.
- هیچ کد محصول، wire contract، سیاست اعتماد یا الگوریتم رمزنگاری تغییر نکرده است؛ فقط پوشش آزمون افزوده شده است.

| معیار پذیرش | آزمون‌ها و فایل‌های واقعی |
|---|---|
| مسیر موفق و correlation درخواست/تلاش | Android: Libp2pInferenceRequestTransportTest; RequestViewModelTest؛ Windows: HostRequestResponseDeliveryTests؛ transport روی peer واقعی .NET در CI |
| نبود میزبان، میزبان مشغول و عدم dispatch غیرضروری | AuthenticatedHostDiscoveryTest؛ InferenceAttemptDispatcherTest؛ HostRequestCoordinatorBusyTests؛ HostRequestCoordinatorConcurrencyTests |
| مدل ناسازگار، metadata نامعتبر و backend آماده‌نشده | AutomaticModelSelectionPolicyTest؛ ModelCatalogParserAcceptanceTests؛ HostPeerRuntimeCapabilitiesAcceptanceTests؛ InstalledCatalogModelVerifierAcceptanceTests |
| قطع اتصال، timeout و پاسخ دیرهنگام | InferenceRoundTimerTest؛ InferenceRoundResponseCollectorTest؛ InferenceResponseCorrelationValidatorTest؛ NetworkInferenceRequestFrameAcceptanceTests |
| لغو و retry صریح | RequestViewModelTest؛ Libp2pInferenceRequestTransportTest؛ LlamaCppInferenceEngineCancellationTests؛ HostRequestCoordinatorFailureAcceptanceTests |
| هویت ناشناس/نامعتبر، تغییر هویت کلید و revoke | AndroidPeerTrustStoreTest (شامل آزمون تازهٔ rotated PeerId)؛ PeerTrustStoreAcceptanceTests؛ WindowsPeerTrustStoreAcceptanceTests؛ PeerTransportContractAcceptanceTests |
| replay، دستکاری و response جعلی/نامرتبط | PeerMessageSecurityAcceptanceTests؛ PeerTransportContractAcceptanceTests؛ InferenceResponseCorrelationValidatorTest |
| عدم افشای prompt/response/secret در logهای عادی | SafeDiagnosticLogAcceptanceTests؛ NetworkInferenceRequestFrameAcceptanceTests؛ test_network_payload_logging.py؛ test_sensitive_artifact_scanning.py؛ test_sensitive_buffer_cleanup.py |
| انتخاب deterministic مدل و سقف خروجی | AutomaticModelSelectionPolicyTest؛ HostPeerRuntimeCapabilitiesAcceptanceTests؛ ModelCatalogParserAcceptanceTests |
| نگاشت وضعیت‌ها و خطاها در UI | RequestStatusPresentationTest؛ RequestViewModelTest؛ test_android_request_status_contract.py |

## مرز شواهد

- آزمون Android و Windows در GitHub Actions روی commit اجرایی این وظیفه باید واقعاً اجرا شوند؛ تا ثبت نتیجهٔ دقیق، این تلاش در حال اجرا است.
- CI/loopback/native .NET integration آزمون فیزیکی محصول نیست.
- آزمون دستگاه Android واقعی به Windows نصب‌شده با مدل محلی واقعی، بازگشت پاسخ به همان Room و snapshot نصب/ارتقای درجا همچنان در UPD-S1-020 و UPD-S1-021 است و در این وظیفه ادعای موفقیت نمی‌شود.
