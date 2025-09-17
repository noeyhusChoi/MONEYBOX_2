using Pr22.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.NewFolder
{
    public interface IOcrService
    {
        Task<OcrOutcome> RunAsync(Page page, OcrMode mode, CancellationToken ct);
    }

    public sealed class OcrService : IOcrService
    {
        private readonly IOcrProvider _mrz;
        private readonly IOcrProvider _ext;

        public OcrService(IOcrProvider mrz, IOcrProvider ext)
        {
            _mrz = mrz;
            _ext = ext;
        }

        public async Task<OcrOutcome> RunAsync(Page page, OcrMode mode, CancellationToken ct)
        {
            switch (mode)
            {
                case OcrMode.MrzOnly:
                    return await _mrz.RunAsync(page, ct);

                case OcrMode.ExternalOnly:
                    return await _ext.RunAsync(page, ct);

                case OcrMode.Auto:
                default:
                    // 1) MRZ 시도
                    Debug.WriteLine("MRZ OCR Process");
                    var mrz = await _mrz.RunAsync(page, ct);
                    if (mrz.Success) return mrz;
                    // 2) 실패 시 외부 OCR로 폴백
                    Debug.WriteLine("External OCR Process");
                    return await _ext.RunAsync(page, ct);
            }
        }
    }
}
