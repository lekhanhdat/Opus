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

        //domain group在fba环境下存储的格式为c:0+.w|Sid,通过ConvertDomainGroupSidToAccount将Sid转化成account
        public static AveUserInfo ConvertDomainGroupSidToAccount(AveUserInfo userInfo, AveObjectModelFactory modelFactory)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Common.AveUserUtility.ConvertDomainGroupSidToAccount"))
            {
                if (userInfo.DomainGroup)
                {
                    if (userInfo.Login.IndexOf('|') > 0)
                    {
                        string temp = userInfo.Login.Substring(userInfo.Login.IndexOf('|') + 1);
                        if (AveDirectoryServiceUtility.IsStringSid(temp))
                        {
                            temp = AveDirectoryServiceUtility.GetAccountFromSid(temp, modelFactory);
                            if (!string.IsNullOrEmpty(temp))
                            {
                                userInfo.Login = userInfo.Login.Substring(0, userInfo.Login.IndexOf('|') + 1) + temp;
                            }
                        }
                    }
                }
            }
            return userInfo;
        }

        public static List<AveUserInfo> ConvertDomainGroupSidToAccount(List<AveUserInfo> userInfos, AveObjectModelFactory modelFactory)
        {
            if (userInfos.Count == 0)
            {
                return userInfos;
            }
            List<AveUserInfo> results = new List<AveUserInfo>(userInfos.Count);
            foreach (var userInfo in userInfos)
            {
                results.Add(ConvertDomainGroupSidToAccount(userInfo, modelFactory));
            }
            return results;
        }
    }
}
