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
using System.Text;
using System.Security.Principal;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Common
{
    public class AveUserUtility
    {
        public static SecurityIdentifier AccountNameToSid(string accName)
        {
            NTAccount account = new NTAccount(accName);
            return (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));
        }

        public static string EnsureAccountName(string account, AveObjectModelFactory modelFactory)
        {
            if (account.IndexOf('|') > 0)
            {
                account = account.Substring(account.IndexOf('|') + 1);
            }
            if (AveDirectoryServiceUtility.IsStringSid(account))
            {
                account = AveDirectoryServiceUtility.GetAccountFromSid(account, modelFactory);
            }
            return account;
        }

        public static bool IsSystemAccount(string loginName)
        {
            string noPrifixUserName = UserLoginNamePrefix.RemoveLoginNamePrifix(loginName);
            if (noPrifixUserName.StartsWith("rolemanager|spo-grid-all-users")
                || noPrifixUserName.StartsWith("ylo001\\_spocrawler")
                || noPrifixUserName.StartsWith("ylo001\\_spocrwl")
                || noPrifixUserName.StartsWith("ylo001\\_spofrm")
                || loginName.StartsWith("C:0%.c|system|", StringComparison.OrdinalIgnoreCase)) //For SAAS-32379, skip the system account: C:0%.c|system|farmId)
            {
                return true;
            }

            string lowerUserName = noPrifixUserName.ToLower(System.Globalization.CultureInfo.CurrentCulture).Trim();
            switch (lowerUserName)
            {
                case "true":
                case "windows":
                case "sharepoint\\system":
                case "nt authority\authenticated users":
                case "ylo001\\_spocachefull":
                case "ylo001\\_spocacheread":
				case "nt service\\spsearch":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 通过MFA scan的SC，会多出user:i:0i.t|00000003-0000-0ff1-ce00-000000000000|app@sharepoint
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public static bool IsSPAppUser(string loginName)
        {
            if (loginName.StartsWith("i:0i.t|", StringComparison.OrdinalIgnoreCase) &&
                loginName.EndsWith("|app@sharepoint", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }
    }
}
