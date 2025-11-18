using KIOSK.API.GTF.KIOSK.API.Gtf;
using KIOSK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.Services
{
    public interface IGtfTaxRefundService
    {
        GtfTaxRefundModel Current { get; }

        void Reset();

        void ApplyInitialResponse(InitialRequestDto req, InitialResponseDto resp);
        void ApplyInquirySlipList(InquirySlipListRequestDto req, InquirySlipListResponseDto resp);
        void AddOrUpdateSlip(RegisterSlipRequestDto req, RegisterSlipResponseDto resp);

        // 최종 환불 타입별 적용
        void ApplyCardRefund(CardRefundRequestDto req, CardRefundResponseDto resp);
        void ApplyAlipayRefund(AlipayRefundRequestDto req, AlipayRefundResponseDto resp);
        void ApplyWechatRefund(WechatRefundRequestDto req, WechatRefundResponseDto resp);
        void ApplyDepositAmt(DepositAmtRequestDto req, DepositAmtResponseDto resp);

        //GtfTransactionEntity ToEntity(); // DB에 저장할 엔티티로 변환
    }

    class GtfTaxRefundService : IGtfTaxRefundService
    {
        public GtfTaxRefundModel Current { get; private set; } = new();

        public void Reset()
        {
            Current = new GtfTaxRefundModel();
        }

        public void ApplyInitialResponse(InitialRequestDto req, InitialResponseDto resp)
        {
            Current.Edi = req.Edi;
            Current.KioskNo = resp.KioskNo;
            Current.KioskType = resp.KioskType;
            Current.RefundLimitAmt = resp.RefundLimitAmt;
        }

        public void ApplyInquirySlipList(InquirySlipListRequestDto req, InquirySlipListResponseDto resp)
        {
            Current.Name = req.Name;
            Current.PassportNo = req.PassportNo;
            Current.NationalityCode = req.NationalityCode;
            Current.Birthday = req.Birthday;
            Current.PassportExpirdate = req.PassportExpirdate;
            Current.GenderCode = req.GenderCode;
            Current.InputWayCode = req.InputWayCode;

            Current.PassportSerialNo = resp.PassportSerialNo;
        }

        public void AddOrUpdateSlip(RegisterSlipRequestDto req, RegisterSlipResponseDto resp)
        {
            // 여권 일련번호는 세션 헤더 수준에서 유지
            if (!string.IsNullOrEmpty(resp.PassportSerialNo))
                Current.PassportSerialNo = resp.PassportSerialNo;

            // rows 값과 list 개수 간단 검증
            if (!string.IsNullOrWhiteSpace(resp.Rows)
                && int.TryParse(resp.Rows, out var rows)
                && rows != resp.List.Count)
            {
                // 필요하면 로그 남기기
                // _logger.Warn($"rows({rows}) != list.Count({resp.List.Count})");
            }

            // resp.List에 들어있는 전표들을 Current.SlipItems에 반영
            foreach (var item in resp.List)
            {
                // 어떤 걸 기준으로 "같은 전표"로 볼지 키를 정해야 함
                // 여기서는 buy_serial_no 기준으로 upsert
                var slip = Current.SlipItems
                                  .FirstOrDefault(x => x.BuySerialNo == item.BuySerialNo);

                if (slip is null)
                {
                    slip = new GtfSlipItem
                    {
                        QrDataType = req.QrDataType,
                        QrData = req.QrData
                    };
                    Current.SlipItems.Add(slip);
                }

                // 응답 필드 매핑
                slip.BuySerialNo = item.BuySerialNo;
                slip.SellDate = item.SellDate;
                slip.SellTime = item.SellTime;
                slip.TotalBuyAmt = item.TotalBuyAmt;
                slip.TotalRefundAmt = item.TotalRefundAmt;
                slip.Qty = item.Qty;
                slip.TotalTaxAmt = item.TotalTaxAmt;
                slip.SlipStatusCode = item.SlipStatusCode;
                slip.HotelRefundYn = item.HotelRefundYn;
                slip.MediRefundYn = item.MediRefundYn;

                // 공통 헤더(rc, rm)는 각 슬립에도 같이 달아두면 나중에 디버깅 편함
                slip.Rc = resp.Rc;
                slip.Rm = resp.Rm;
            }
        }


        public void ApplyCardRefund(CardRefundRequestDto req, CardRefundResponseDto resp)
        {
            Current.RefundTypeCode = req.RefundTypeCode;
            Current.RefundWayCode = req.RefundWayCode;
            Current.RefundNo = resp.RefundNo;
        }

        public void ApplyAlipayRefund(AlipayRefundRequestDto req, AlipayRefundResponseDto resp)
        {
            Current.RefundTypeCode = req.RefundTypeCode;
            Current.RefundWayCode = req.RefundWayCode;
            Current.RefundNo = resp.RefundNo;
            // 필요하다면 list_1, list_2, list_3도 별도 테이블/JSON 저장
        }

        public void ApplyWechatRefund(WechatRefundRequestDto req, WechatRefundResponseDto resp)
        {
            Current.RefundTypeCode = req.RefundTypeCode;
            Current.RefundWayCode = req.RefundWayCode;
            Current.RefundNo = resp.RefundNo;
            Current.TotalRefundAmt = resp.TotalWechatRefundAmt;
        }

        public void ApplyDepositAmt(DepositAmtRequestDto req, DepositAmtResponseDto resp)
        {
            Current.TotalDepositAmt = resp.DepositAmt;
        }
    }
}
