using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xaml;

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

    // LocalizationService: 런타임/디자인타임 공용 로직
    public class LocalizationService : ILocalizationService
    {
        private readonly ResourceDictionary _langDictionary = new();
        private readonly string _basePath = "Assets/LANGUAGE"; // 리소스 폴더
        private readonly Dictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);

        public event EventHandler? LanguageChanged;

        public IReadOnlyList<CultureInfo> SupportedCultures { get; } = new[]
        {
            new CultureInfo("ko-KR"),
            new CultureInfo("en-US"),
            new CultureInfo("ja-JP"),
            new CultureInfo("zh-CN"),
            new CultureInfo("zh-TW"),
        };

        public CultureInfo CurrentCulture { get; private set; } = CultureInfo.GetCultureInfo("en-US");

        // 기본 생성자: en-US
        public LocalizationService() : this(CultureInfo.GetCultureInfo("en-US")) { }

        // 새 생성자: 특정 문화로 초기 로드 (디자인타임에서 사용)
        public LocalizationService(CultureInfo initialCulture)
        {
            CurrentCulture = initialCulture ?? CultureInfo.GetCultureInfo("en-US");
            LoadForCulture(CurrentCulture);
        }

        public void SetCulture(CultureInfo culture)
        {
            if (culture == null) throw new ArgumentNullException(nameof(culture));
            if (culture.Name == CurrentCulture.Name) return;

            CurrentCulture = culture;
            LoadForCulture(culture);
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }

        public string? GetString(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;

            if (_cache.TryGetValue(key, out var cached)) return cached;

            // ResourceDictionary 안전 조회
            string? value = null;
            if (_langDictionary.Contains(key))
            {
                value = _langDictionary[key] as string;
            }
            // 추가 폴백: App.Resources에도 있으면 사용
            else
            {
                var app = Application.Current;
                if (app != null)
                {
                    foreach (var rd in app.Resources.MergedDictionaries.Reverse())
                    {
                        if (rd.Contains(key))
                        {
                            value = rd[key] as string;
                            break;
                        }
                    }
                }
            }

            if (value != null) _cache[key] = value;
            return value;
        }

        private void LoadForCulture(CultureInfo culture)
        {
            _cache.Clear();

            // 후보 파일들 (culture.Name 우선, 두글자 코드, 기본)
            var candidates = new List<string>
            {
                Path.Combine(_basePath, $"StringResources.{culture.Name}.xaml"),
                //Path.Combine(_basePath, $"StringResources.{culture.TwoLetterISOLanguageName}.xaml"),
                //Path.Combine(_basePath, "StringResources.xaml")
            };

            var rd = new ResourceDictionary();
            // 디자인타임 환경에서 GetExecutingAssembly가 디자이너 호스트와 다를 수 있으므로
            // 명시적으로 현재 타입의 Assembly 사용
            var assemblyName = typeof(LocalizationService).Assembly.GetName().Name ?? System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            foreach (var c in candidates.Distinct())
            {
                try
                {
                    var uri = new Uri($"pack://application:,,,/{assemblyName};component/{c}", UriKind.Absolute);
                    rd.MergedDictionaries.Add(new ResourceDictionary { Source = uri });
                    Debug.WriteLine($"[Localization] Loaded resource candidate: {c}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Localization] failed to load '{c}' -> {ex.GetType().Name}: {ex.Message}");
                    // not found -> skip
                }
            }

            _langDictionary.MergedDictionaries.Clear();
            foreach (var d in rd.MergedDictionaries)
                _langDictionary.MergedDictionaries.Add(d);

            // App 전역 리소스가 존재하면 교체(하지만 디자인타임에 Application.Current는 null일 수 있음)
            var app = Application.Current;
            if (app != null)
            {
                var appRDs = app.Resources.MergedDictionaries;

                // 기존의 StringResources.* 만 제거
                var oldLocals = appRDs
                    .Where(x => x.Source != null &&
                                x.Source.OriginalString.IndexOf($"{_basePath}/StringResources.", StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
                foreach (var o in oldLocals) appRDs.Remove(o);

                // 추가
                foreach (var d in _langDictionary.MergedDictionaries)
                    appRDs.Add(d);
            }
            else
            {
                Debug.WriteLine("[Localization] Application.Current is null -> skipping App.Resources merge (design-time).");
            }
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
            // 초기 알림(디자이너에서 바인딩이 바로 반영되도록)
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
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

            // 디자인타임 초기화: 루트 요소의 d:Language 읽어서 해당 culture로 LocalizationService 생성
            if (!LocalizationProvider.Instance.IsInitialized && DesignerHelper.IsInDesignTool())
            {
                try
                {
                    var rootProvider = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
                    CultureInfo? designCulture = null;

                    if (rootProvider?.RootObject is FrameworkElement root)
                    {
                        // 즉시 초기화(현재 값)
                        var xmlLang = root.Language;
                        if (xmlLang != null && !string.IsNullOrEmpty(xmlLang.IetfLanguageTag))
                        {
                            try
                            {
                                designCulture = new CultureInfo(xmlLang.IetfLanguageTag);
                                Debug.WriteLine($"[LocExtension] Design language detected: {xmlLang.IetfLanguageTag}");
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"[LocExtension] invalid design language '{xmlLang.IetfLanguageTag}': {ex.Message}");
                            }
                        }

                        // 디자인타임 동안에 d:Language 값이 바뀌면 재초기화되도록 변경 감지기 추가
                        try
                        {
                            var dpd = DependencyPropertyDescriptor.FromProperty(FrameworkElement.LanguageProperty, typeof(FrameworkElement));
                            if (dpd != null)
                            {
                                // 중복 등록을 방지하기 위해 간단한 토큰을 이용해 이미 등록되었는지 확인
                                const string tokenKey = "___Loc_Ext_LangHandler_Registered";
                                if (!(root.Tag is string t && t.Contains(tokenKey)))
                                {
                                    EventHandler handler = (_, __) =>
                                    {
                                        try
                                        {
                                            var newLang = root.Language;
                                            if (newLang != null && !string.IsNullOrEmpty(newLang.IetfLanguageTag))
                                            {
                                                Debug.WriteLine($"[LocExtension] Design language changed -> {newLang.IetfLanguageTag}");
                                                var newCulture = new CultureInfo(newLang.IetfLanguageTag);
                                                // 재초기화: LocalizationProvider.Instance.Attach 를 통해 새로운 서비스로 교체
                                                LocalizationProvider.Initialize(new LocalizationService(newCulture));
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine($"[LocExtension] error handling design language change: {ex}");
                                        }
                                    };

                                    dpd.AddValueChanged(root, handler);

                                    // 안전한 메모리 관리: root가 언로드되면 제거
                                    RoutedEventHandler unloaded = null!;
                                    unloaded = (_, __) =>
                                    {
                                        try
                                        {
                                            dpd.RemoveValueChanged(root, handler);
                                            root.Unloaded -= unloaded;
                                        }
                                        catch { }
                                    };
                                    root.Unloaded += unloaded;

                                    // 짧은 토큰 표식 (디자이너 전용이므로 간단하게 Tag를 재사용)
                                    root.Tag = (root.Tag?.ToString() ?? string.Empty) + tokenKey;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[LocExtension] failed to attach language change listener: {ex}");
                        }
                    }

                    // 폴백: CurrentUICulture 또는 ko-KR (원하면 수동 설정)
                    var initial = designCulture ?? CultureInfo.CurrentUICulture;
                    LocalizationProvider.Initialize(new LocalizationService(initial));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[LocExtension] design init failed: {ex}");
                }
            }

            // 리소스 인덱서에 바인딩
            var binding = new Binding($"[{Key}]")
            {
                Source = LocalizationProvider.Instance,
                Mode = BindingMode.OneWay
            };

            return binding.ProvideValue(serviceProvider);
        }
    }

    internal static class DesignerHelper
    {
        private static bool? _cached;
        public static bool IsInDesignTool()
        {
            if (_cached.HasValue) return _cached.Value;
            var d = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
            _cached = d;
            return d;
        }
    }
}
