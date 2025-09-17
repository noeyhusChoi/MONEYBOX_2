using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.NewFolder
{
    public enum OcrMode { MrzOnly, ExternalOnly, Auto }

    public sealed class OcrOptions
    {
        public string BaseDir { get; init; } = @"C:\Users\niaci\OneDrive\Dokumen\MoneyBox\SourceCode\MPOS_V2\Money24h\Bin\OCR";
        public string InputDir => Path.Combine(BaseDir, "input");
        public string ResultTypeDir => Path.Combine(BaseDir, "resultType");
        public string ResultDir => Path.Combine(BaseDir, "result");

        // 대기/타임아웃
        public TimeSpan ResultTimeout { get; init; } = TimeSpan.FromSeconds(10);
        public TimeSpan PollInterval { get; init; } = TimeSpan.FromMilliseconds(200);
    }

    public sealed class ExternalOcrFilePath
    {
        public string SessionId { get; init; } = "0001"; // 거래번호, 반드시 4자리
        public string InfraImagePath { get; init; } = default!;     // 분석 이미지 (infra)
        public string WhiteImagePath { get; init; } = default!;     // 분석 이미지 (white)
        public string TriggerPath { get; init; } = default!;        // 분석 시작 트리거 파일
        public string TypeJsonPath { get; init; } = default!;       // 신분증 타입 분석 결과 Json
        public string ResultJsonPath { get; init; } = default!;     // 신분증 내용 분석 결과 Json
    }

    public sealed class OcrOutcome
    {
        public bool Success { get; init; }
        public string? DocumentType { get; init; }  // 예: "KOR_ID", "Passport", ...
        public Dictionary<string, string>? Fields { get; init; } // 외부 OCR key-value
        public string? RawTypeJson { get; init; }
        public string? RawResultJson { get; init; }
        public string Source { get; init; } = "";   // "MRZ" or "External"
        public string? Error { get; init; }
    }

    public sealed class ExternalTypeJson
    {
        public string type { get; set; } = "";
    }

    public sealed class ExternalResultJson
    {
        public string type { get; set; } = "";
        public string id { get; set; } = "";
        public string id_confidence { get; set; } = "";
        public string name { get; set; } = "";
        public string name_confidence { get; set; } = "";
        public string address { get; set; } = "";
        public string address_confidence { get; set; } = "";
        public string nation { get; set; } = "";
        public string nation_confidence { get; set; } = "";
        public string comment { get; set; } = "";
        public bool rotate_image { get; set; }
        public bool need_save_original { get; set; }
    }

}
