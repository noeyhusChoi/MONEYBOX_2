using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KIOSK.ViewModels
{
    public class CurrencyNoteItem
    {
        public int Denomination { get; set; }
        public string Label => Denomination.ToString();
        public ImageSource Image { get; set; }
        public string FilePath { get; set; } // 필요시
    }

    public partial class ExchangeDepositViewModel : ObservableObject, IStepMain, IStepNext, IStepError
    {
        public Func<Task>? OnStepMain { get; set; }
        public Func<Task>? OnStepPrevious { get; set; }
        public Func<bool?, Task>? OnStepNext { get; set; }
        public Action<Exception>? OnStepError { get; set; }

        [ObservableProperty]
        private string gifPath;

        [ObservableProperty]
        private ObservableCollection<CurrencyNoteItem> currencyNotes;

        private string[] _supportedExt = new[] { ".png", ".jpg"};

        [ObservableProperty]
        private string selectedCurrency;

        // 에셋 폴더 (실제 경로에 맞춰 수정)
        private readonly string _assetsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Image", "Denomination");

        // 최대 로드 개수
        private readonly int _maxCount = 7;

        public ExchangeDepositViewModel()
        {
            // TODO: 사용 가능 화폐 단위 모델 참조 형식으로 변경 필요 ( 시스템 설정에서 사용 가능 화폐 단위 )
            // TODO: 현재 선택 화폐 참조 형식으로 변경 필요 ( 유저 선택 화폐 )
            // TODO: 이미지 추출 유틸리티로 추후 이동
            currencyNotes = new();
            // 기본값 설정 (필요시)
            SelectedCurrency = "USD";
            LoadDenomination(SelectedCurrency);

            // TODO: 선택 화폐에 따라 GIF 영상 변경
            gifPath = "pack://application:,,,/Assets/Gif/IDScan_ID.gif"; //Temp
        }


        private void LoadDenomination(string currencyCode)
        {
            //currencyNotes.Clear();
            if (string.IsNullOrWhiteSpace(currencyCode) || !Directory.Exists(_assetsDir)) return;

            // 모든 파일을 찾아서 "PREFIX_"로 시작하는 것만 필터
            var files = Directory.GetFiles(_assetsDir)
                .Where(f => _supportedExt.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                .Where(f => Path.GetFileName(f).StartsWith(currencyCode + "_", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            // 파싱: 파일명에서 숫자(denomination) 추출 (ex USD_100.png -> 100)
            var list = files
                .Select(f =>
                {
                    var name = Path.GetFileNameWithoutExtension(f); // USD_100
                    var m = Regex.Match(name, @"^.+[_\-](\d+)$");   // 뒷부분 숫자 캡처
                    int denom = 0;
                    if (m.Success) int.TryParse(m.Groups[1].Value, out denom);
                    return new { File = f, Denom = denom };
                })
                // 숫자 기반 정렬(숫자 없으면 뒤로)
                .OrderBy(x => x.Denom == 0 ? int.MaxValue : x.Denom)
                .ThenBy(x => x.File)
                .Take(_maxCount)
                .ToArray();

            foreach (var item in list)
            {
                BitmapImage bmp = null;
                try
                {
                    bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(item.File, UriKind.Absolute);
                    bmp.DecodePixelWidth = 240; // 필요하면 조정: 메모리 절약
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();
                }
                catch
                {
                    bmp = null; // placeholder 처리가능
                }

                CurrencyNotes.Add(new CurrencyNoteItem
                {
                    Denomination = item.Denom,
                    Image = bmp,
                    FilePath = item.File
                });
            }
        }
    }
}
