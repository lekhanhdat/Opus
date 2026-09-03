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
using System.Globalization;
using System.Threading;

namespace RASPAppWeb
{
    public class I18nUtil
    {
        private static CultureInfo DefaultCulture = new CultureInfo("en-US");
        private static readonly RALogger logger = RALogger.GetInstance(typeof(I18nUtil));
        private static Dictionary<string, CultureInfo> LanguageMapping = new Dictionary<string, CultureInfo>
        {
            ["en-US"] = new CultureInfo("en-US"),
            ["ja-JP"] = new CultureInfo("ja-JP"),
            ["fr-FR"] = new CultureInfo("fr-FR"),
            ["zh-CN"] = new CultureInfo("zh-CN"),
            ["ko-KR"] = new CultureInfo("ko-KR"),
            ["fr-CA"] = new CultureInfo("fr-CA"),
        };

        public static void SetLanguage(string lang)
        {
            try
            {
                if (lang != null && LanguageMapping.ContainsKey(lang))
                {
                    var ci = LanguageMapping[lang];
                    logger.Info($"Set culture {ci.Name}");
                    //RASPAppWeb.Resources.RelatedRecords.Culture = ci;
                    //Thread.CurrentThread.CurrentUICulture = LanguageMapping[lang];
                    Thread.CurrentThread.CurrentUICulture = ci;
                    Thread.CurrentThread.CurrentCulture = ci;
                }
                else
                {
                    Thread.CurrentThread.CurrentUICulture = DefaultCulture;
                }
            }
            catch (Exception e)
            {
                logger.Info($"Set culture language failed {e.ToString()}");
            }
        }
    }
}