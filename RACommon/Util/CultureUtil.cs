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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.GCommon;

namespace AvePoint.RA.Common.Util
{
    public class CultureUtil
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(CultureUtil));
        private static readonly List<string> cultures = new List<string> { "en-US", "ja-JP", "zh-CN", "ko-KR", "fr-FR", "fr-CA" };
        private static readonly List<string> validCultures = new List<string> { "en", "en-US", "ja", "ja-JP" ,"zh" , "zh-CN" ,"ko" ,"ko-KR", "fr", "fr-FR", "fr-CA" };

        public static string GetDefaultCulture()
        {
            return cultures[0];
        }

        public static void SetCulture(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                if (!validCultures.Any(c => c.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    //不是有效的culture则用默认的
                    logger.Info($"Invalid Culture name, use default culture name {name}.");
                    name = GetDefaultCulture();
                }
                //CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("en-US"); 也可以这种方式赋值 给所有线程设置默认的Culture了 仅.Net 4.5以上
                var ci = System.Globalization.CultureInfo.CreateSpecificCulture(name);
                Thread.CurrentThread.CurrentUICulture = ci;
                Thread.CurrentThread.CurrentCulture = ci;
            }
        }
    }
}
