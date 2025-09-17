// Core/DeviceRegistry.cs
using Device.Abstractions;
using Device.Devices;
using Device.Transport;
using System.Collections.Generic;
using System.Diagnostics;

namespace Device.Core
{
    /// <summary>
    /// 장치 정의(Descriptor) -> 실제 인스턴스 생성 팩토리
    /// </summary>
    public static class DeviceRegistry
    {
        // TODO: 추후 삭제
        public static IDevice Create(DeviceDescriptor decorator)
        {
            //// Transport 생성
            //ITransport transport = CreateTransport(decorator.TransportType, decorator.TransportParam);

            //// 장치 타입에 따라 분기(모델명/ID 규칙으로도 가능)
            //if (decorator.Model.StartsWith("PRN"))
            //    return new PrinterDevice(decorator, transport);
            //if (decorator.Model.StartsWith("QR"))
            //    return new QrScannerDevice(decorator, transport);

            throw new System.NotSupportedException($"Unknown model: {decorator.Model}");
        }

        public static IDevice Create(DeviceDescriptor d, ITransport t) => d.Model.ToUpper() switch
        {
            "PRINT" => new DevicePrinter(d, t),
            "QR" => new DeviceQrScanner(d, t),
            "IDSCANNER" => new DeviceIdScanner(d, t),
            //"SCL-ABC" => new ScaleDevice(d, t),
            _ => new DevicePrinter(d, t) // 기본 or throw
        };

        private static ITransport CreateTransport(string name)
        {
            return null;
            // TODO : 추후 Transport 선택 로직 구현
            /*
            // 예: "COM3@115200"
            var parts = name.Split('@');
            var port = parts[0];
            var baud = parts.Length > 1 ? int.Parse(parts[1]) : 115200;
            return new SerialPortTransport(port, baud);
            */
        }

        private static IProtocol CreateProtocol(string name)
        {
            return null;
            // TODO : 추후 프로토콜 선택 로직 구현
            /*
            switch (name?.Trim().ToLowerInvariant())
            {
                case "ascii":
                case "asciiline":
                    // 개행(라인) 단위 ASCII 프로토콜
                    return new SimpleCrcProtocol(new AsciiLineFramer());

                case "simplecrc":
                case "crc":
                    // [0xAA][LEN(2,BE)][PAYLOAD][CRC16(2,LE)]
                    return new SimpleCrcProtocol(new BinaryCrcFramer());

                case "raw":
                case "noheader":
                case "none":
                    // 헤더/체크섬 없는 원시 송수신(응답 처리는 장치 코드에서 옵션 조정)
                    return new RawNoHeaderProtocol();

                default:
                    throw new NotSupportedException(
                        $"Unknown protocol: '{name}'. Use one of: AsciiLine, SimpleCRC, StxLenXor, Raw");
            }
            */
        }

    }
}
