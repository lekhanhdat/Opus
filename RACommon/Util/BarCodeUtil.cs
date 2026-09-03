/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using System;
using ZXing;
using System.IO;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using SkiaSharp;
using SKImage = SkiaSharp.SKImage;
using ZXing.SkiaSharp.Rendering;
using System.Linq;
using ZXing.SkiaSharp;
using AvePoint.GCommon.Contract.ContentManager.Object;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.CodeAnalysis.Text;
using System.Drawing;
using ZXing.OneD;
using ZXing.Common;
using DocumentFormat.OpenXml.Spreadsheet;
using AvePoint.RA.Contract.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.RegularExpressions;
using ZXing.QrCode.Internal;
using ZXing.PDF417.Internal;

namespace AvePoint.RA.Common.Util
{
    public class BarcodeUtil
    {
        protected static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(BarcodeUtil));

        private readonly BarcodeWriter mBarcodeWriter;

        private static readonly IRMCache Cache = PlatformWindsorManager.GetService<IRMCache>();

        private const string BARCODE_STANDARD_KEY = "Barcode_Standard";

        public BarcodeUtil()
        {
            string fontName = "noto";
            var font = SKTypeface.FromFamilyName(fontName, SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
            if (font == null)
            {
                throw new Exception($"Can not get the font. Family Name: [{fontName}]");
            }
            else
            {
                logger.Info($"Successfully get font and the family name is [{font.FamilyName}]");
            }
            var barcodeFormatString = Cache.GetAsync<string>(BARCODE_STANDARD_KEY).GetAwaiter().GetResult();
            var barcodeFormat = BarcodeFormat.CODE_128;
            if (!string.IsNullOrEmpty(barcodeFormatString))
            {
                barcodeFormat = barcodeFormatString == "1" ? BarcodeFormat.CODE_39 : BarcodeFormat.CODE_128;
            }
            mBarcodeWriter = new ZXing.SkiaSharp.BarcodeWriter()
            {
                Format = barcodeFormat,
                Renderer = new SKBitmapRenderer()
                {
                    TextFont = font,
                    TextSize = 14f
                }
            };
            if(barcodeFormatString == "0")
            {
                mBarcodeWriter.Options.Hints.Add(EncodeHintType.CODE128_FORCE_CODESET_B, true);
            }
            mBarcodeWriter.Options.PureBarcode = false;
        }

        public string GetBarcodeImgBase64Str(string barcodeValue)
        {
            string imgBase64Str = string.Empty;
            if (!string.IsNullOrEmpty(barcodeValue))
            {
                using (var ms = GetBarcodeStream(barcodeValue))
                {
                    imgBase64Str = Convert.ToBase64String(ms.ToArray());
                }
            }
            return imgBase64Str;
        }

        private MemoryStream GetBarcodeStream(string barcodeValue)
        {
            var img = GetBarcodeImg(barcodeValue);
            var skdata = img.Encode(SKEncodedImageFormat.Png, 100);
            var ms = new MemoryStream();
            skdata.SaveTo(ms);
            return ms;
        }

        public MemoryStream GetBarcodeStream(string barcodeValue, ref BarCodeImageInfo bi)
        {
            var ms = GetBarcodeStream(barcodeValue);
            var img = SKImage.FromEncodedData(ms.ToArray());
            //var img = GetBarcodeImg(barcodeValue, BarcodeFormat.CODE_128);
            bi.Width = img.Width;
            bi.Height = img.Height;
            double resolution = img.Height * 0.96;
            bi.HR = resolution;
            bi.VR = resolution;
            return ms;
        }

        public bool PreCheckBarcodeInfo(string barcodeValue)
        {
            if (mBarcodeWriter.Format == BarcodeFormat.CODE_39)
            {
                var pattern = @"^[A-Z0-9\-\. \$\/\+\%\ ]+$";
                var isValid = Regex.IsMatch(barcodeValue, pattern);
                if (!isValid)
                {
                    return false;
                }
            }
            return true;
        }

        private SKBitmap GetBarcodeImg(string barcodeValue)
        {
            var matrix = mBarcodeWriter.Encode(barcodeValue);
            var skBitmapRender = (SKBitmapRenderer)mBarcodeWriter.Renderer;
            var image = Render(matrix, mBarcodeWriter.Format, barcodeValue, mBarcodeWriter.Options, skBitmapRender.TextFont, skBitmapRender.TextSize);
            return image;
        }

        private static unsafe SKBitmap Render(BitMatrix matrix, BarcodeFormat format, string content, EncodingOptions options, SKTypeface font, float fontSize)
        {
            var textSize = (int)fontSize + 4;

            int width = matrix.Width;
            int height = matrix.Height;
            SKTypeface sKTypeface = font;
            int num = 0;
            bool flag = sKTypeface != null && (options == null || !options.PureBarcode) && !string.IsNullOrEmpty(content) && (format == BarcodeFormat.CODE_39 || format == BarcodeFormat.CODE_93 || format == BarcodeFormat.CODE_128 || format == BarcodeFormat.EAN_13 || format == BarcodeFormat.EAN_8 || format == BarcodeFormat.CODABAR || format == BarcodeFormat.ITF || format == BarcodeFormat.UPC_A || format == BarcodeFormat.UPC_E || format == BarcodeFormat.MSI || format == BarcodeFormat.PLESSEY);
            if (options != null)
            {
                if (options.Width > width)
                {
                    width = options.Width;
                }

                if (options.Height > height)
                {
                    height = options.Height;
                }
            }

            int num2 = width / matrix.Width;
            int num3 = height / matrix.Height;
            SKBitmap sKBitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            uint* ptr = (uint*)sKBitmap.GetPixels().ToPointer();
            uint num4 = (uint)SKColors.Black;
            uint num5 = (uint)SKColors.White;
            int num6 = (int)((textSize < 1f) ? 10f : textSize);
            num = ((flag && height + 10 > num6) ? num6 : 0);
            for (int i = 0; i < matrix.Height - num; i++)
            {
                for (int j = 0; j < num3; j++)
                {
                    for (int k = 0; k < matrix.Width; k++)
                    {
                        uint num7 = (matrix[k, i] ? num4 : num5);
                        for (int l = 0; l < num2; l++)
                        {
                            uint* num8 = ptr;
                            ptr = num8 + 1;
                            *num8 = num7;
                        }
                    }

                    for (int m = num2 * matrix.Width; m < width; m++)
                    {
                        uint* num9 = ptr;
                        ptr = num9 + 1;
                        *num9 = num5;
                    }
                }
            }

            for (int n = num3 * matrix.Height; n < height; n++)
            {
                for (int num10 = 0; num10 < width; num10++)
                {
                    uint* num11 = ptr;
                    ptr = num11 + 1;
                    *num11 = num5;
                }
            }

            if (flag && num > 0)
            {
                for (int num12 = height - num; num12 < height; num12++)
                {
                    for (int num13 = 0; num13 < width; num13++)
                    {
                        uint* num14 = ptr;
                        ptr = num14 + 1;
                        *num14 = num5;
                    }
                }
            }

            if (num > 0)
            {
                using (SKCanvas sKCanvas = new SKCanvas(sKBitmap))
                {
                    using SKPaint sKPaint = new SKPaint();
                    sKPaint.IsAntialias = true;
                    sKPaint.Color = SKColors.Black;
                    sKPaint.Typeface = sKTypeface;
                    sKPaint.TextSize = ((fontSize < 1f) ? 10f : fontSize);

                    float num15 = sKPaint.MeasureText(content);
                    float num16 = ((float)(num2 * matrix.Width) - num15) / 2f;
                    int num17 = height - 1 - (textSize - (int)fontSize);
                    num16 = ((num16 < 0f) ? 0f : num16);
                    sKCanvas.DrawText(content, num16, num17, sKPaint);

                    sKCanvas.Flush();
                    return sKBitmap;
                }
            }

            return sKBitmap;
        }

        public static BarCodeImageInfo GetImageInfo(byte[] data)
        {
            var img = SKImage.FromEncodedData(data);
            return new BarCodeImageInfo
            {
                Width = img.Width,
                Height = img.Height
            };
        }
    }
}
