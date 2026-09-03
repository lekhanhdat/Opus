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
using AvePoint.RA.Contract.Services;
using AvePoint.Records.Core.Utilities.Extensions;
using HSMAzureCommon;

namespace RAGoogle.Extension
{
    public static class GoogleUserExtension
    {
        private static readonly IRALogger logger = RALogger.GetInstance(typeof(GoogleUserExtension));
        public static bool IsExternalUser(this string email, IList<string> domians)
        {
            string domain = email?.GetDomainFromMail();
            if (!domians.Any(d => d.Eq(domain)))
            {
                return true;
            }
            return false;
        }

        public static string GetDomainFromMail(this string str)
        {
            string domain = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(str) || !str.Contains('@'))
                {
                    return string.Empty;
                }
                else
                {
                    domain = str.GetStringByLastKey("@");
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"[GetDomainFromMail] failed, name:{str}, exception:{ex}");
            }
            return domain;
        }

        public static string GetStringByLastKey(this string str, string key)
        {
            try
            {
                int index = str.LastIdxOf(key);
                if (index > -1)
                {
                    string result = str[(index + key.Length)..];
                    return result;
                }
            }
            catch (Exception e)
            {
                logger.Error($"[GetStringByLastKey] Exception: {e}");
            }
            return str;
        }
    }
}
