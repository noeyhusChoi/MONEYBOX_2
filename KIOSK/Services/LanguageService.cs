using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Localization
{
    public interface ILocalizationService
    {
        CultureInfo CurrentCulture { get; }
        IReadOnlyList<CultureInfo> SupportedCultures { get; }

        void SetCulture(CultureInfo culture);
        string? GetString(string key);

        event EventHandler? LanguageChanged;
    }

    // TODO: 하드코딩 부분 변경 필요, 또는 중앙 서비스로 분리
    public class LocalizationService : ILocalizationService
    {
        private readonly ResourceDictionary _langDictionary = new();
        private readonly string _basePath = "Assets/LANGUAGE"; // 폴더명
        private readonly Dictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);

        public event EventHandler? LanguageChanged;

        public IReadOnlyList<CultureInfo> SupportedCultures { get; } = new[]
        {
            // TODO: 동적으로 지원할 언어 추가
            new CultureInfo("ko-KR"),
            new CultureInfo("en-US"),
            new CultureInfo("ja-JP"),
            new CultureInfo("zh-CN"),
            new CultureInfo("zh-TW"),
        };

        public CultureInfo CurrentCulture { get; private set; } = CultureInfo.GetCultureInfo("en-US");

        public LocalizationService()
        {
            // 초기 로드
            LoadForCulture(CurrentCulture);
        }

        public void SetCulture(CultureInfo culture)
        {
            if (culture.Name == CurrentCulture.Name) return;
            CurrentCulture = culture;
            LoadForCulture(culture);
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }

        public string? GetString(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            if (_cache.TryGetValue(key, out var cached)) return cached;

            var value = _langDictionary[key] as string;
            if (value != null) _cache[key] = value;
            return value;
        }

        private void LoadForCulture(CultureInfo culture)
        {
            _cache.Clear();

            // 파일명 규칙: StringResources.{culture}.xaml
            var candidates = new List<string>
            {
                Path.Combine(_basePath, $"StringResources.{culture.Name}.xaml"),
                //Path.Combine(_basePath, $"StringResources.{culture.TwoLetterISOLanguageName}.xaml"),
                //Path.Combine(_basePath, "StringResources.ko-KR.xaml") // 기본 폴백
            };

            // pack URI 사용(디자인타임/런타임 모두 안정적)
            var rd = new ResourceDictionary();
            foreach (var c in candidates.Distinct())
            {
                try
                {
                    var uri = new Uri(
                        $"pack://application:,,,/{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name};component/{c}",
                        UriKind.Absolute);
                    rd.MergedDictionaries.Add(new ResourceDictionary { Source = uri });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Localization] failed to load '{c}' -> {ex.GetType().Name}: {ex.Message}");
                    // not found → skip
                }
            }

            _langDictionary.MergedDictionaries.Clear();
            foreach (var d in rd.MergedDictionaries)
                _langDictionary.MergedDictionaries.Add(d);

            // App 전역 리소스 갱신 (기존 언어 사전 제거 → 신규 추가)
            var appRDs = Application.Current.Resources.MergedDictionaries;

            // 제거 기준을 StringResources로 일치시킵니다.
            var oldLocals = appRDs
                .Where(x => x.Source != null &&
                            x.Source.OriginalString.Contains($"{_basePath}/StringResources.", StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (var o in oldLocals) appRDs.Remove(o);

            foreach (var d in _langDictionary.MergedDictionaries)
                appRDs.Add(d);
        }
    }

    // MarkupExtension이 바인딩할 “단일 소스”
    public sealed class LocalizationProvider : INotifyPropertyChanged
    {
        private static LocalizationProvider? _instance;
        private ILocalizationService? _svc;

        public static LocalizationProvider Instance => _instance ??= new LocalizationProvider();
        public bool IsInitialized => _svc != null;

        private LocalizationProvider() { }

        public static void Initialize(ILocalizationService svc)
        {
            Instance.Attach(svc);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string this[string key] => _svc?.GetString(key) ?? $"[{key}]";

        private void Attach(ILocalizationService svc)
        {
            _svc = svc;
            _svc.LanguageChanged += (_, __) =>
            {
                // 인덱서 변경 알림 (Item[])
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            };
        }
    }

    [MarkupExtensionReturnType(typeof(BindingExpression))]
    public class LocExtension : MarkupExtension
    {
        public string Key { get; set; } = string.Empty;

        public LocExtension() { }
        public LocExtension(string key) => Key = key;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            Debug.WriteLine($"[LocExtension] ProvideValue for key='{Key}', ProviderInitialized={LocalizationProvider.Instance.IsInitialized}");

            // 디자이너에서 아직 Initialize 안됐으면, 임시 서비스로 스스로 초기화
            if (!LocalizationProvider.Instance.IsInitialized && DesignerHelper.IsInDesignTool())
            {
                // _basePath 규칙을 그대로 사용하는 LocalizationService를 직접 생성
                LocalizationProvider.Initialize(new LocalizationService());
                // 디자이너에서 d:Language를 지정해두면 CurrentUICulture가 반영
                // 필요시 Culture를 강제로 지정하는 로직도 추가
            }

            // 디자인타임에서 d:Language 반영
            // (디자이너일 때도 바인딩을 씌워서 문자열 미리보기)
            var binding = new Binding($"[{Key}]")
            {
                Source = LocalizationProvider.Instance,
                Mode = BindingMode.OneWay
            };

            var value = binding.ProvideValue(serviceProvider);

            // 디자이너 모드에서 즉시 문자열 보이도록 (선택 사항)
            if (DesignerHelper.IsInDesignTool())
            {
                // nothing; WPF 디자이너가 바인딩도 렌더합니다.
            }

            return value;
        }
    }

    internal static class DesignerHelper
    {
        private static bool? _cached;
        public static bool IsInDesignTool()
        {
            if (_cached.HasValue) return _cached.Value;
            var d = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject());
            _cached = d;
            return d;
        }
    }

}
