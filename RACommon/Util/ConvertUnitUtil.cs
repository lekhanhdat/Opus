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
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Util
{
    public class ConvertUnitUtil
    {
        private static RALogger logger = RALogger.GetInstance(typeof(ConvertUnitUtil));

        public static string ConvertToKB(string item)
        {
            try
            {
                double result;
                int pos = 0;
                while (pos < item.Length && (char.IsDigit(item[pos]) || item[pos] == '.'))
                {
                    pos++;
                }
                double value = double.Parse(item.Substring(0, pos));
                string unit = item.Substring(pos).ToUpper().Trim();
                switch (unit)
                {
                    case "KB":
                        result = value;
                        break;
                    case "MB":
                        result = value * 1024;
                        break;
                    case "GB":
                        result = value * 1024 * 1024;
                        break;
                    case "TB":
                        result = value * 1024 * 1024 * 1024;
                        break;
                    case "PB":
                        result = value * 1024 * 1024 * 1024 * 1024;
                        break;
                    case "EB":
                        result = value * 1024 * 1024 * 1024 * 1024 * 1024;
                        break;
                    case "ZB":
                        result = value * 1024 * 1024 * 1024 * 1024 * 1024 * 1024;
                        break;
                    case "B":
                        result = value / 1024 / 1024;
                        break;
                    case "BYTES":
                        result = value / 1024;
                        break;
                    default:
                        logger.Error("Invalid unit");
                        return item;
                }
                result = Math.Round(result, 2);
                return result.ToString();
            }
            catch(Exception e)
            {
                logger.Error($"Convert unit occur error: {e}");
                return item;
            }
        }

        public static string FormatBytes(long bytes)
        {
            if (bytes == 0)
                return "0 B";

            string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB" };

            int order = (int)Math.Floor(Math.Log(bytes, 1024));
            double num = bytes / Math.Pow(1024, order);

            return $"{num:0.##} {sizes[order]}";
        }
    }
}
