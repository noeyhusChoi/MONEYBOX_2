using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KIOSK.Modules.Features.Environment.ViewModel
{
    public partial class CassetteModel : ObservableObject
    {
        [ObservableProperty] private int id;
        [ObservableProperty] private string deviceId;
        [ObservableProperty] private int slot;
        [ObservableProperty] private string currency;
        [ObservableProperty] private int value;
        [ObservableProperty] private int count;
    }

    public partial class EnvironmentCassetteSettingViewModel : ObservableObject
    {
        // DataGrid에 바인딩되는 카세트 리스트
        public ObservableCollection<CassetteModel> AllCassettes { get; set; }

        // ComboBox에 바인딩되는 통화 리스트
        public ObservableCollection<string> Currencies { get; set; } = new ObservableCollection<string> { "KRW", "USD", "JPY", "CNY" };

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        private int _nextId = 100;

        public EnvironmentCassetteSettingViewModel()
        {
            LoadInitialData();

        }

        // 초기 데이터 로드 (실제 환경에서는 DB에서 로드)
        private void LoadInitialData()
        {
            AllCassettes = new ObservableCollection<CassetteModel>
            {
                new CassetteModel { Id = 1, DeviceId = "HCDM1", Slot = 1, Currency = "KRW", Value = 50000, Count = 100 },
                new CassetteModel { Id = 2, DeviceId = "HCDM1", Slot = 2, Currency = "KRW", Value = 10000, Count = 500 },
                new CassetteModel { Id = 3, DeviceId = "HCDM2", Slot = 1, Currency = "USD", Value = 100, Count = 50 },
                new CassetteModel { Id = 4, DeviceId = "HCDM2", Slot = 2, Currency = "USD", Value = 50, Count = 20 },
                new CassetteModel { Id = 5, DeviceId = "CoinDisp", Slot = 1, Currency = "JPY", Value = 1000, Count = 10 }
            };
        }

        // --- ICommand Methods ---

        // 기기별 슬롯 추가 로직
        private void AddSlotToDevice(object deviceIdObject)
        {
            if (!(deviceIdObject is string deviceId)) return;

            // 해당 기기의 현재 최대 슬롯 번호를 찾습니다.
            var currentSlots = AllCassettes
                .Where(c => c.DeviceId == deviceId)
                .Select(c => c.Slot)
                .DefaultIfEmpty(0);

            int newSlotNumber = currentSlots.Max() + 1;

            if (newSlotNumber > 4)
            {
                StatusMessage = $"{deviceId}에 더 이상 슬롯을 추가할 수 없습니다 (최대 4개).";
                return;
            }

            // 새 모델 인스턴스를 생성하고 ObservableCollection에 추가 (UI 자동 업데이트)
            AllCassettes.Add(new CassetteModel
            {
                Id = _nextId++,
                DeviceId = deviceId,
                Slot = newSlotNumber,
                Currency = string.Empty,
                Value = 0,
                Count = 0
            });

            StatusMessage = $"{deviceId}에 슬롯 {newSlotNumber}가 추가되었습니다. 저장하세요.";
        }

        // 슬롯 삭제 로직
        private void DeleteSlot(object item)
        {
            if (item is CassetteModel cassette &&
                MessageBox.Show($"{cassette.DeviceId}의 슬롯 {cassette.Slot}을 삭제하시겠습니까?", "삭제 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                AllCassettes.Remove(cassette);
                StatusMessage = "슬롯이 삭제되었습니다. 저장하세요.";
            }
        }

        // 전체 저장 로직 (유효성 검사 및 데이터 처리)
        private void SaveAll(object obj)
        {
            if (!ValidateData())
            {
                // ValidateData 내부에서 StatusMessage 설정
                return;
            }

            // 1. 여기서 실제 데이터베이스나 파일 저장 로직을 호출합니다.

            // 2. 저장 후 상태 메시지 업데이트
            StatusMessage = $"성공적으로 {AllCassettes.Count}개의 설정을 저장했습니다. (DB Write)";

            // (추가: 저장 후 로직 필요 시 구현)
        }

        // 데이터 유효성 검사 (예시)
        private bool ValidateData()
        {
            // 예시: 슬롯 번호 중복 및 값 누락 검사
            var validationErrors = AllCassettes
                .GroupBy(c => new { c.DeviceId, c.Slot })
                .Where(g => g.Count() > 1 || string.IsNullOrWhiteSpace(g.Key.DeviceId) || g.All(c => c.Value <= 0 || string.IsNullOrWhiteSpace(c.Currency)));

            if (validationErrors.Any())
            {
                StatusMessage = "오류: 중복된 슬롯 번호나 누락된 통화/권종 값이 있습니다. 확인해주세요.";
                return false;
            }
            return true;
        }

        // INotifyPropertyChanged 구현
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
