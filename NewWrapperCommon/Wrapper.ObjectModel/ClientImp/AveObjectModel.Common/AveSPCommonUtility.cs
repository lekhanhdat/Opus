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


using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AvePoint.ObjectModel.Common
{
    public class AveSPCommonUtility
    {
        public static string ConvertMultiColumnValueToString(List<string> subColumnValues, bool bAddLeadingTailingDelimiter, bool bPreserveEmpty)
        {
            bool flag = false;
            StringBuilder builder = new StringBuilder(0xff);
            for (int i = 0; i < subColumnValues.Count; i++)
            {
                string str = subColumnValues[i];
                if (!string.IsNullOrEmpty(str))
                {
                    str = str.Replace(";", ";;");
                }
                if (!string.IsNullOrEmpty(str))
                {
                    flag = true;
                }
                if (bAddLeadingTailingDelimiter || (i != 0))
                {
                    builder.Append(";#");
                }
                builder.Append(str);
            }
            if (!flag && !bPreserveEmpty)
            {
                return string.Empty;
            }
            if (bAddLeadingTailingDelimiter)
            {
                builder.Append(";#");
            }
            return builder.ToString();
        }

        public static bool IsGuid(string strId)
        {
            if (string.IsNullOrEmpty(strId))
            {
                return false;
            }
            strId = strId.Trim();
            if (strId.Length < 0x20)
            {
                return false;
            }
            if (strId.Contains("x") || strId.Contains("X"))
            {
                strId = strId.Replace(" ", "");
                return Regex.IsMatch(strId, @"^\{0[x|X][a-fA-F\d]{8},(0[x|X][a-fA-F\d]{4},){2}\{(0[x|X][a-fA-F\d]{2},){7}0[x|X][a-fA-F\d]{2}\}\}$", RegexOptions.Compiled);
            }
            return Regex.IsMatch(strId, @"^([a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}|\([a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}\)|\{[a-fA-F\d]{8}-([a-fA-F\d]{4}-){3}[a-fA-F\d]{12}\}|[a-fA-F\d]{32})$", RegexOptions.Compiled);
        }

        public static string GetTenantAdminSiteUrl(AveBPOSAccountInfo adminAccount,string siteUrl)
        {
            using (AveAzurePowerShellRequest azureShell = new AveAzurePowerShellRequest(adminAccount))
            {
                return AveUrlUtility.GetTenantAdminSiteUrl(azureShell);
            }
        }
    }
}
