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

namespace AvePoint.GCommon.GraphAPI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class ODataSpecialCharactersConverter
    {
        private static Dictionary<string, string> specialCharDic
        {
            get
            {
                var dics = new Dictionary<string, string>();
                dics.Add("&", "%26");
                dics.Add("'", "''");
                dics.Add("+", "%2b");
                dics.Add("#", "%23");
                return dics;
            }
        }

        public static string ConvertToS(string queryString)
        {
            specialCharDic.Keys.ToList().ForEach(key =>
            {
                if (queryString.Contains(key))
                {
                    queryString = queryString.Replace(key, specialCharDic[key]);
                }
            });
            return queryString;
        }

        public static string ConvertMailForSDK(string queryString)
        {
            return queryString.Replace("'", "''");
        }
    }

    public static class SupportConfigTabs
    {
        public static HashSet<string> Tabs =>
            new HashSet<string>{
                BuiltInTabTeamAppsId.Planner,
                BuiltInTabTeamAppsId.WebSite,
                BuiltInTabTeamAppsId.Word,
                BuiltInTabTeamAppsId.PDF,
                BuiltInTabTeamAppsId.PowerPoint,
                BuiltInTabTeamAppsId.Excel,
                BuiltInTabTeamAppsId.Word_V1,
                BuiltInTabTeamAppsId.PowerPoint_V1,
                BuiltInTabTeamAppsId.Excel_V1,
                BuiltInTabTeamAppsId.DocumentLibrary,
                BuiltInTabTeamAppsId.Lists,
                BuiltInTabTeamAppsId.PowerBI,
                BuiltInTabTeamAppsId.PowerBI_V1//Power BI
            };

        public static bool Contains(string teamAppId)
        {
            if (string.IsNullOrEmpty(teamAppId)) return false;
            else return Tabs.Contains(teamAppId);
        }
    }

    public static class BuiltInTabTeamAppsId
    {
        public static readonly String TeamTabAppUrl = "https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/{0}";

        public const string Word_V1 = "d7958adf-f419-46fa-941b-1b946497ef84";
        public const string Excel_V1 = "1c256a65-83a6-4b5c-9ccf-78f8afb6f1e8";
        public const string PowerPoint_V1 = "3e0a4fec-499b-4138-8e7c-71a9d88a62ed";
        public const string PowerBI_V1 = "1c4340de-2a85-40e5-8eb0-4f295368978b";

        public const string Word = "com.microsoft.teamspace.tab.file.staticviewer.word";
        public const string Excel = "com.microsoft.teamspace.tab.file.staticviewer.excel";
        public const string PowerPoint = "com.microsoft.teamspace.tab.file.staticviewer.powerpoint";
        public const string PDF = "com.microsoft.teamspace.tab.file.staticviewer.pdf";
        public const string Wiki = "com.microsoft.teamspace.tab.wiki";
        public const string DocumentLibrary = "com.microsoft.teamspace.tab.files.sharepoint";
        public const string PowerBI = "com.microsoft.teamspace.tab.powerbi";
        public const string WebSite = "com.microsoft.teamspace.tab.web";
        public const string Planner = "com.microsoft.teamspace.tab.planner";
        public const string Stream = "com.microsoftstream.embed.skypeteamstab";
        public const string Forms = "81fef3a6-72aa-4648-a763-de824aeafb7d";
        public const string SPPageList = "2a527703-1f6f-4559-a332-d8a7d288cd88";
        public const string OneNote = "0d820ecd-def2-4297-adad-78056cde7c78";
        public const string BroadcastQnA = "7a0c1d53-f647-4d76-ab2c-fbc0d73c8bb0";
        public const string Lists = "26bc2873-6023-480c-a11b-76b66605ce8c";
        public static bool IsFileTab(this Tab tab)
        {
            var appId = tab?.TeamsApp?.Id;
            return IsFileTab(appId);
        }

        public static bool IsOfficeTab(string teamAppId)
        {
            return string.Equals(teamAppId, Word, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(teamAppId, Excel, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(teamAppId, PowerPoint, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(teamAppId, PDF, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(teamAppId, Word_V1, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(teamAppId, Excel_V1, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(teamAppId, PowerPoint_V1, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsFileTab(string teamAppId)
        {
            return IsOfficeTab(teamAppId) ||
                string.Equals(teamAppId, Lists, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsPowerBI(string teamAppId)
        {
            return string.Equals(teamAppId, PowerBI, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(teamAppId, PowerBI_V1, StringComparison.OrdinalIgnoreCase);
        }
    }
}