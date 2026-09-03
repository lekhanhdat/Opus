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
using AvePoint.Common;
using AvePoint.GCommon.Contract.ReportCenter.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace RAReportCenter.ClientAuditReport.Utility
{
    internal class SPAuditReportUtility
    {
        private static Dictionary<string, string> BrowserMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<string> KnownAppUserAgents = new List<string>
        {
            "OneNote",
            "msone",
            "MSWAC",
            "OneDrive",
            "OneDriveiOSApp",
            "ODMTA",
            "Microsoft Outlook Social Connector"
        };

        //private static Parser UaParser = Parser.GetDefault();
        public static string GetBrowserType(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
            {
                return "";
            }
            foreach (var item in KnownAppUserAgents)
            {
                if (userAgent.Contains(item))
                {
                    return "";
                }
            }
            lock (BrowserMapping)
            {
                if (BrowserMapping.ContainsKey(userAgent))
                {
                    return BrowserMapping[userAgent];
                }
                else
                {
                    var result = BrowserType.Others;
                    throw new Exception("obsoleted");
                    //var browserCapabilities = new HttpBrowserCapabilities
                    //{
                    //    Capabilities = new Hashtable(180, StringComparer.OrdinalIgnoreCase)
                    //    {
                    //        { string.Empty, userAgent }
                    //    }
                    //};
                    //var capabilitiesFactory = new BrowserCapabilitiesFactory();
                    //capabilitiesFactory.ConfigureBrowserCapabilities(new NameValueCollection(), browserCapabilities);
                    //if (userAgent.IndexOf("Edge", StringComparison.OrdinalIgnoreCase) > 0)
                    //{
                    //    result = BrowserType.Edge;
                    //}
                    //else if (string.Equals(browserCapabilities.Browser, "InternetExplorer", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    result = BrowserType.InternetExplorer;
                    //}
                    //else if (string.Equals(browserCapabilities.Browser, "IE", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    result = BrowserType.InternetExplorer;
                    //}
                    //else if (string.Equals(browserCapabilities.Browser, "Chrome", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    result = BrowserType.Chrome;
                    //}
                    //else if (string.Equals(browserCapabilities.Browser, "Firefox", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    result = BrowserType.Firefox;
                    //}
                    //else if (string.Equals(browserCapabilities.Browser, "Safari", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    result = BrowserType.Safari;
                    //}
                    //else if (string.Equals(browserCapabilities.Browser, "Edge", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    result = BrowserType.Edge;
                    //}
                    BrowserMapping.Add(userAgent, result);
                    return result;
                }
            }
        }

        public static string GetOperateSystem(string agent)
        {
            if (agent.Contains("Windows NT") || agent.Contains("compatible"))
            {
                return OperatSystem.Windows;
            }
            else if (agent.Contains("iPhone") || agent.Contains("iPad") || agent.Contains("iPod") || agent.Contains("iOS"))
            {
                return OperatSystem.IOS;
            }
            else if (agent.Contains("Android"))
            {
                return OperatSystem.Android;
            }
            else if (agent.Contains("Macintosh"))
            {
                return OperatSystem.Mac;
            }
            else
            {
                return OperatSystem.Others;
            }
        }

        //public static string GetBrowser(string userAgent)
        //{
        //    var browser = UaParser.ParseUserAgent(userAgent);
        //    if (KnownBrowsers.Contains(browser.Family))
        //    {
        //        if (string.Equals(browser.Family, "IE", StringComparison.OrdinalIgnoreCase))
        //        {
        //            return BrowserType.InternetExplorer;
        //        }
        //        return browser.Family;
        //    }
        //    else
        //    {
        //        return BrowserType.Others;
        //    }
        //}
        public static string GetAveId(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            var hash = HashCodeHelper.ToMD5HashCode(text.ToLowerInvariant());
            return hash.Replace("-", "").Substring(8, 16);
        }

        #region Convert O365 auditLog to SharePoint.
        public static int ConvertItemType(string itemType)
        {
            if (string.Equals(itemType, O365ItemType.DocumentLibrary.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return (int)ClientAuditObjType.List;
            }
            else if (string.Equals(itemType, O365ItemType.File.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return (int)ClientAuditObjType.Document;
            }
            else if (string.Equals(itemType, O365ItemType.Folder.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return (int)ClientAuditObjType.Folder;
            }
            else if (string.Equals(itemType, O365ItemType.Page.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return (int)ClientAuditObjType.ListItem;
            }
            else if (string.Equals(itemType, O365ItemType.Web.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return (int)ClientAuditObjType.Site;
            }
            else if (string.Equals(itemType, O365ItemType.Site.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return (int)ClientAuditObjType.SiteCollection;
            }
            else if (string.Equals(itemType, O365ItemType.Tenant.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return (int)ClientAuditObjType.SiteCollection;
            }
            else if (string.Equals(itemType, O365ItemType.ListItem.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return (int)ClientAuditObjType.ListItem;
            }
            else if (string.Equals(itemType, O365ItemType.List.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return (int)ClientAuditObjType.List;
            }
            else if (string.Equals(itemType, O365ItemType.Field.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return (int)ClientAuditObjType.List;
            }
            else
            {
                //如果是目前没发现的 itemType，都归类为 Document，以后如果有问题再修改
                return (int)ClientAuditObjType.Document;
            }
            return 0;
        }

        public static int ConvertEventType(string operationStr)
        {
            int resultNum = -1;
            switch (operationStr)
            {
                case "eDiscoverySearchPerformed":
                    resultNum = (int)AuditEventType.Search;
                    break;
                case "GroupAdded":
                    resultNum = (int)AuditEventType.CreateGroup;
                    break;
                case "GroupRemoved":
                    resultNum = (int)AuditEventType.DeleteGroup;
                    break;
                case "UserAddedToGroup":
                case "AddedToGroup":
                case "SiteCollectionAdminAdded":
                    resultNum = (int)AuditEventType.AddGroupMember;
                    break;
                case "UserRemovedFromGroup":
                case "RemovedFromGroup":
                case "SiteCollectionAdminRemoved":
                    resultNum = (int)AuditEventType.DeleteGroupMember;
                    break;
                case "PermissionLevelAdded":
                    resultNum = (int)AuditEventType.CreatePermissionLevel;
                    break;
                case "PermissionLevelRemoved":
                    resultNum = (int)AuditEventType.DeletePermissionLevel;
                    break;
                case "PermissionLevelModified":
                    resultNum = (int)AuditEventType.ChangePermissionLevel;
                    break;
                case "SharingSet":
                    resultNum = (int)AuditEventType.ChangePermission;
                    break;
                case "SharingInheritanceReset":
                    resultNum = (int)AuditEventType.InheritPermissionSetting;
                    break;
                case "SharingInheritanceBroken":
                    resultNum = (int)AuditEventType.BreakPermissionInheritance;
                    break;
                default:
                    if (operationStr.IndexOf("Updated", StringComparison.OrdinalIgnoreCase)>0)
                    {
                        resultNum = (int)AuditEventType.Update;
                    }
                    else if (operationStr.IndexOf("Created", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        resultNum = (int)AuditEventType.Update;
                    }
                    else if (operationStr.IndexOf("Renamed", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        resultNum = (int)AuditEventType.Update;
                    }
                    else if (operationStr.IndexOf("Modified", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        resultNum = (int)AuditEventType.Update;
                    }
                    else if (operationStr.IndexOf("Added", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        resultNum = (int)AuditEventType.Update;
                    }
                    else if (operationStr.IndexOf("Applied", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        resultNum = (int)AuditEventType.Update;
                    }
                    else if (operationStr.IndexOf("Change", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        resultNum = (int)AuditEventType.Update;
                    }
                    else if (operationStr.IndexOf("Accepted", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        resultNum = (int)AuditEventType.Update;
                    }
                    else if (operationStr.IndexOf("Rejected", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        resultNum = (int)AuditEventType.Update;
                    }
                    else if (operationStr.IndexOf("CheckOut", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        resultNum = (int)AuditEventType.CheckOut;
                    }
                    else if (operationStr.IndexOf("CheckIn", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        resultNum = (int)AuditEventType.CheckIn;
                    }
                    else if (operationStr.IndexOf("Viewed", StringComparison.OrdinalIgnoreCase) > 0 || operationStr.IndexOf("Accessed", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        resultNum = (int)AuditEventType.View;
                    }
                    else if (operationStr.IndexOf("Deleted", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        resultNum = (int)AuditEventType.Delete;
                    }
                    else if (operationStr.IndexOf("Restored", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        resultNum = (int)AuditEventType.Undelete;
                    }
                    else if (operationStr.IndexOf("Moved", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        resultNum = (int)AuditEventType.Move;
                    }
                    else if (operationStr.IndexOf("Downloaded", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        resultNum = (int)AuditEventType.Download;
                    }
                    else if (operationStr.IndexOf("Copied", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        resultNum = (int)AuditEventType.Copy;
                    }
                    else if (operationStr.IndexOf("Recycled", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        resultNum = (int)AuditEventType.Delete;
                    }
                    else
                    {
                        resultNum = (int)AuditEventType.Custom;
                    }
                    break;
            }
            return resultNum;
        }
        #endregion

        public static string GetTempFolder(string tenantGroupID,string jobId)
        {
            string tmpFolder;
            string path = Path.Combine(AveEnv.AgentTempFolder, @"AuditReport");
            path = Path.Combine(path, tenantGroupID);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            tmpFolder = Path.Combine(path, jobId);
            if (!Directory.Exists(tmpFolder))
            {
                Directory.CreateDirectory(tmpFolder);
            }
            return tmpFolder;
        }

        public static void GetRangeDate(ref DateTime start, ref DateTime end, AvePoint.RA.Contract.JobMonitor.TimeRangeType tangeType)
        {
            //对于One_Month 这种range,时间范围从月初开始. e.g 当前3月13日，onemonth是3月1日 to now
            DateTime now = DateTime.UtcNow;
            DateTime tmp = new DateTime();
            if (tangeType != AvePoint.RA.Contract.JobMonitor.TimeRangeType.Custom)
            {
                end = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
                tmp = new DateTime(end.Year, end.Month, 1, 0, 0, 0);
            }
            switch (tangeType)
            {
                case AvePoint.RA.Contract.JobMonitor.TimeRangeType.CurrentWeek:
                    //本周  每一周的第一天为周一
                    int addDaysTemp = (int)end.DayOfWeek == 0 ? -6 : -(int)end.DayOfWeek + 1;
                    start = end.AddDays(addDaysTemp).AddHours(-23).AddMinutes(-59).AddSeconds(-59);
                    break;
                case AvePoint.RA.Contract.JobMonitor.TimeRangeType.CurrentMonth:
                    start = tmp;
                    break;
                case AvePoint.RA.Contract.JobMonitor.TimeRangeType.Last3Month:
                    start = tmp.AddMonths(-2);
                    break;
                case AvePoint.RA.Contract.JobMonitor.TimeRangeType.Last6Month:
                    start = tmp.AddMonths(-5);
                    break;
                //case TimeRangeType.Custom:
                //    start = new DateTime(start.Value.Year, start.Value.Month, start.Value.Day, 0, 0, 0);
                //    end = new DateTime(end.Value.Year, end.Value.Month, end.Value.Day, 23, 59, 59);
                //    break;
                default:
                    start = end.AddDays(-5).AddHours(-23).AddMinutes(-59).AddSeconds(-59);
                    break;
            }
            start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
            end = DateTime.SpecifyKind(end, DateTimeKind.Utc);
        }

        #region Get URL AveId
        public static Dictionary<string, Dictionary<string, string>> GetUrlDic(List<string> nodes)
        {
            var uriDic = new Dictionary<string, Dictionary<string, string>>();
            uriDic = GetSiteNodesUrlDic(nodes);
            return uriDic;
        }

        private static Dictionary<string, Dictionary<string, string>> GetSiteNodesUrlDic(List<string> nodes)
        {
            var urlDic = new Dictionary<string, Dictionary<string, string>>();
            var dic = new Dictionary<string, string>();
            foreach (var siteUrl in nodes)
            {
                var siteId = GetAveId(siteUrl);
                if (!dic.ContainsKey(siteId))
                {
                    dic.Add(siteId, siteUrl);
                }
            }
            urlDic.Add("SiteCollection", dic);
            return urlDic;
        }

        #endregion

    }

    public class BrowserType
    {
        public static readonly string Others = "Others";
        public static readonly string Edge = "Edge";
        public static readonly string Firefox = "Firefox";
        public static readonly string Chrome = "Chrome";
        public static readonly string Safari = "Safari";
        public static readonly string InternetExplorer = "Internet Explorer";
    }

    public class OperatSystem
    {
        public static readonly string Others = "Others";
        public static readonly string Windows = "Windows";
        public static readonly string IOS = "iOS";
        public static readonly string Android = "Android";
        public static readonly string Mac = "Mac";
    }

    
}
