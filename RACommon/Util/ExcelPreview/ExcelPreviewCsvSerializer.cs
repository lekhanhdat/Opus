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
using System.Linq;
using System.Text;

namespace AvePoint.RA.Common.Util.ExcelPreview
{
    internal static class ExcelPreviewCsvSerializer
    {
        public static string Serialize(ExcelPreviewSheetData sheetData, int maxChars)
        {
            if (sheetData == null)
            {
                throw new ArgumentNullException(nameof(sheetData));
            }

            if (sheetData.Header == null || sheetData.Header.Length == 0)
            {
                return string.Empty;
            }

            var headerWidth = sheetData.Header.Length;
            var builder = new StringBuilder();
            AppendRow(builder, sheetData.Header, headerWidth);

            foreach (var row in sheetData.Rows ?? Enumerable.Empty<string[]>())
            {
                var rowText = BuildRow(row, headerWidth) + "\r\n";
                if (builder.Length + rowText.Length > maxChars)
                {
                    break;
                }

                builder.Append(rowText);
            }

            return builder.ToString();
        }

        private static void AppendRow(StringBuilder builder, string[] row, int width)
        {
            builder.Append(BuildRow(row, width));
            builder.Append("\r\n");
        }

        private static string BuildRow(string[] row, int width)
        {
            if (width <= 0)
            {
                return string.Empty;
            }

            var escapedValues = new string[width];
            for (var i = 0; i < width; i++)
            {
                escapedValues[i] = Escape(row != null && i < row.Length ? row[i] : string.Empty);
            }

            return string.Join(",", escapedValues);
        }

        private static string Escape(string value)
        {
            var safeValue = value ?? string.Empty;
            if (safeValue.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
            {
                return safeValue;
            }

            return "\"" + safeValue.Replace("\"", "\"\"") + "\"";
        }
    }
}
