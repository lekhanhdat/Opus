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

namespace AutoInstallationCommon.ActiveDirectory
{
    public enum NameType
    {
        UPN = 0,
        Classic = 1,
        SingleName = 2
    }

    public class ActiveDirectoryNameInfo
    {
        public string UserName { get; set; }
        public string Domain { get; set; }
        public NameType Type { get; set; }

        public static ActiveDirectoryNameInfo AnalyzeName(string name)
        {
            string[] domainAndName = null;
            var nameInfo = new ActiveDirectoryNameInfo();

            if (name.Contains("\\"))
            {
                domainAndName = name.Split('\\');
                nameInfo.Type = NameType.Classic;
                nameInfo.Domain = domainAndName[0].ToLowerInvariant();
                nameInfo.UserName = domainAndName[1];
            }
            else if (name.Contains("@")) //用户名中不存在@,组名中可能存在,带@的组名，推荐使用domain\group形式
            {
                var result = new string[2];
                var lastAt = name.LastIndexOf('@');
                nameInfo.UserName = name.Substring(0, lastAt);
                nameInfo.Domain = name.Substring(lastAt + 1).ToLowerInvariant();
                nameInfo.Type = NameType.UPN;
            }
            else
            {
                nameInfo.Type = NameType.SingleName;
                nameInfo.UserName = name;
            }

            return nameInfo;
        }
    }
}