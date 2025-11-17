using KIOSK.Models;
using KIOSK.Services;
using System.Diagnostics;

namespace KIOSK.Mocking
{
    public class MockTransactionServiceV2
    {
        TransactionServiceV2 transactionServiceV2 = new();
        public TransactionModelV2 Current => transactionServiceV2.Current;

        public MockTransactionServiceV2()
        {

        }

        public async Task Initialize()
        {
            await transactionServiceV2.UpsertCustomerAsync("0", "홍길동", "KR", "M12341234");

            await transactionServiceV2.UpsertRateAsync(new CurrencyPair("USD", 1234.12m));
            await transactionServiceV2.UpsertPolicyAsync("USD", "KRW", new ExchangePolicy
            {
                FeePercent = 0m,
                FeeFlat = 0m,
                TargetIncrement = 100m,
                RoundingMode = RoundingMode.Down
            });

            await transactionServiceV2.NewAsync("USD", "KRW");
            await transactionServiceV2.AddOrIncrementAsync("USD", 100m, 5);

            HashSet<WithdrawalCassette> mockWithdrawalCassette = new() {
                new WithdrawalCassette(){Capacity = 1000, Count = 100, CurrencyCode = "KRW", Denomination = 50000m, DeviceID = "HCDM1", Slot = 1},
                new WithdrawalCassette(){Capacity = 1000, Count = 100, CurrencyCode = "KRW", Denomination = 10000m, DeviceID = "HCDM1", Slot = 2},
                new WithdrawalCassette(){Capacity = 1000, Count = 100, CurrencyCode = "KRW", Denomination = 5000m, DeviceID = "HCDM1", Slot = 3},
                new WithdrawalCassette(){Capacity = 1000, Count = 100, CurrencyCode = "KRW", Denomination = 1000m, DeviceID = "HCDM1", Slot = 4},
            };

            await transactionServiceV2.PlanPayoutsAsync(mockWithdrawalCassette.ToList());
            
            Dictionary<int, (int, int, int)> res = new Dictionary<int, (int, int, int)>()
            {
                { 1, (2, 0, 0) },
                { 2, (2, 0, 0) },
                { 3, (0, 0, 0) },
                { 4, (3, 0, 0) }
            };

            transactionServiceV2.ApplyDeviceResults("HCDM1", res);
        }
    }
}
