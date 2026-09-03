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
namespace DataExportCore
{
    public static class SizeUtil
    {
        public enum SizeUnit : long
        {
            Byte = 1,
            KB = 1024,
            MB = 1048576,
            GB = 1073741824,
            TB = 1099511627776,
        }

        public static (double Size, SizeUnit Unit) AutoFitSizeUnit(long size)
        {
            double tempSize = size * 1.0;
            if (tempSize < (long)SizeUnit.KB)
            {
                return (size, SizeUnit.Byte);
            }
            if (tempSize < (long)SizeUnit.MB)
            {
                return (tempSize / (long)SizeUnit.KB, SizeUnit.KB);
            }
            if (tempSize < (long)SizeUnit.GB)
            {
                return (tempSize / (long)SizeUnit.MB, SizeUnit.MB);
            }
            if (tempSize < (long)SizeUnit.TB)
            {
                return (tempSize / (long)SizeUnit.GB, SizeUnit.GB);
            }
            return (tempSize / (long)SizeUnit.TB, SizeUnit.TB);
        }

        public static double ToKBSize(this long size)
        {
            double tempSize = size * 1.0;
            return Math.Round(tempSize / (long)SizeUnit.KB, 2);
        }
    }
}
