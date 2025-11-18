using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.Models
{
    public sealed class GtfTaxRefundModel
    {
        public Guid SessionId { get; } = Guid.NewGuid();

        // 공통 정보 (initial)
        public string? Edi { get; set; }
        public string? KioskNo { get; set; }
        public string? KioskType { get; set; }
        public string? RefundLimitAmt { get; set; }

        // 신분증 정보 (inquirySlipList 요청/응답)
        public string? Name { get; set; }
        public string? PassportNo { get; set; }
        public string? NationalityCode { get; set; }
        public string? Birthday { get; set; }
        public string? PassportExpirdate { get; set; }
        public string? GenderCode { get; set; }
        public string? InputWayCode { get; set; }

        public string? PassportSerialNo { get; set; }   // 응답에서 받는 값

        // QR로 등록/검증된 슬립 리스트 (registerSlip 응답)
        public List<GtfSlipItem> SlipItems { get; } = new();

        // 환불 종류/방식 + 결과
        public string? RefundTypeCode { get; set; }      // 환불유형: 카드, 알리페이, 위챗 등의 코드
        public string? RefundWayCode { get; set; }       // 환불수단 코드

        public string? RefundNo { get; set; }            // 최종 refund_no
        public string? TotalRefundAmt { get; set; }      // 총 환불금액(필요시)
        public string? TotalDepositAmt { get; set; }     // 입금형이면 deposit_amt

        public DateTime StartedAt { get; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }

    public sealed class GtfSlipItem
    {
        // 서버에서 검증한 QR 슬립 한 건
        public string? QrDataType { get; set; }
        public string? QrData { get; set; }             // 원본 QR 스트링

        // registerSlip / possibility / refund 등에서 얻은 값들
        public string? BuySerialNo { get; set; }
        public string? NumberOfSlip { get; set; }
        public string? SellDate { get; set; }
        public string? SellTime { get; set; }
        public string? TotalBuyAmt { get; set; }
        public string? TotalRefundAmt { get; set; }
        public string? Qty { get; set; }
        public string? TotalTaxAmt { get; set; }
        public string? SlipStatusCode { get; set; }
        public string? HotelRefundYn { get; set; }
        public string? MediRefundYn { get; set; }

        public string? Rc { get; set; }      // 마지막 응답 코드들
        public string? Rm { get; set; }
    }
}
