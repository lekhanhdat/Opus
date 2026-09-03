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
using AvePoint.GCommon.Utility.I18N;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Util
{
    public class SizeConvertUtil
    {
        public static string I18NDataSize(string size)
        {
            if (I18NUtility.curCulture == "fr-FR" || I18NUtility.curCulture == "fr-CA")//TODO Cyrus
            {
                return size.Replace(".", ",");
            }
            return size;
        }
        /// <summary>
        /// 默认保留小数点后两位
        /// </summary>
        /// <param name="size">单位为Byte</param>
        /// <returns></returns>
        public static string GetDataSizeToView(long size)
        {
            if (size < 1024)
            {
                return I18NDataSize(string.Format("{0}{1}", size, I18NEntity.GetString("Bytes")));
            }
            else if (size >= 1024 && size < 1024 * 1024)
            {
                return I18NDataSize(string.Format("{0:F}{1}", size / 1024.0, I18NEntity.GetString("KB")));
            }
            else if (size >= 1024 * 1024 && size < 1024 * 1024 * 1024)
            {
                return I18NDataSize(string.Format("{0:F}{1}", size / (1024 * 1024.0), I18NEntity.GetString("MB")));
            }
            else if (size >= 1024 * 1024 * 1024 && size < 1024L * 1024 * 1024 * 1024)
            {
                return I18NDataSize(string.Format("{0:F}{1}", size / (1024 * 1024 * 1024.0), I18NEntity.GetString("GB")));
            }
            else
            {
                return I18NDataSize(string.Format("{0:F}{1}", size / (1024L * 1024 * 1024 * 1024.0), I18NEntity.GetString("TB")));
            }
        }
    }
}
