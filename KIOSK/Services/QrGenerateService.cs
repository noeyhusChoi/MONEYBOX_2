﻿using QRCoder;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KIOSK.Services
{
    public interface IQrGenerateService
    {
        public ImageSource ImageFromBytes();
    }

    public class QrGenerateService : IQrGenerateService
    {
        public readonly byte[] PngBytes;

        public QrGenerateService()
        {
            var payload = "https://maps.app.goo.gl/ufNcfJPe7212gtzs8";
            using var qrGen = new QRCodeGenerator();
            using var qrData = qrGen.CreateQrCode(payload, QRCodeGenerator.ECCLevel.H);
            var png = new PngByteQRCode(qrData);
            PngBytes = png.GetGraphic(20); // 20 = pixels per module (크기 조절)
        }

        public ImageSource ImageFromBytes()
        {
            using var ms = new MemoryStream(PngBytes);
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.StreamSource = ms;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
    }
}
