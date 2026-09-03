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
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace AvePoint.GCommon.Utility.I18N
{
    public enum EventSources
    {
        Empty,
        DocAveMediaService,
        DocAveControlService,
        DocAveAgentService,
        DocAveReportService,
        DocAvePackageService,
        DocAveToolService,
        DocAveCLIService,
        DocAveAPIService,
        DocAveStorageAPIService,
    }

    public class EventSourcesUtil
    {
        private const string DocAveMediaService = "Media Service";
        private const string DocAveControlService = "Control Service";
        private const string DocAveAgentService = "Agent Service";
        private const string DocAveReportService = "Report Service";
        private const string DocAvePackageService = "Package";
        private const string DocAveToolService = "Tool";
        private const string DocAveCLIService = "CLI";
        private const string DocAveAPIService = "API";
        private const string DocAveStorageAPIService = "Storage API";

        //public static void CreateEventSources()
        //{
        //    DeleteObsoleteEventSources();

        //    string eventLogName = "AvePoint";
        //    if (IsSMSP()) eventLogName = "SMSP";
        //    EnsureEventLogName(eventLogName);
        //    List<string> eventSources = new List<string>();
        //    eventSources.Add(ToEventSourceString(EventSources.DocAveMediaService));
        //    eventSources.Add(ToEventSourceString(EventSources.DocAveControlService));
        //    eventSources.Add(ToEventSourceString(EventSources.DocAveAgentService));
        //    eventSources.Add(ToEventSourceString(EventSources.DocAveReportService));
        //    eventSources.Add(ToEventSourceString(EventSources.DocAvePackageService));
        //    eventSources.Add(ToEventSourceString(EventSources.DocAveToolService));
        //    eventSources.Add(ToEventSourceString(EventSources.DocAveCLIService));
        //    eventSources.Add(ToEventSourceString(EventSources.DocAveAPIService));
        //    eventSources.Add(ToEventSourceString(EventSources.DocAveStorageAPIService));
        //    string categoryDll = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles), Path.Combine(Path.Combine(Path.Combine("DocAve Shared", "DocAve 6"), "bin"), "CommonEventCategory.dll"));
        //    if (!File.Exists(categoryDll))
        //    {
        //        categoryDll = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles), Path.Combine(Path.Combine(Path.Combine("AvePoint Shared", "DocAve 6"), "bin"), "CommonEventCategory.dll"));
        //    }
        //    int categoryCount = 2000;
        //    foreach (string eventSource in eventSources)
        //    {
        //        if (EventLog.SourceExists(eventSource))
        //        {
        //            if (IsEventSourceUnderLogName(eventLogName, eventSource))
        //            {
        //                AveTuple<string, int> categoryInfos = GetCategoryResourceFileAndCount(eventLogName, eventSource);
        //                if (string.Compare(categoryInfos.ItemA, categoryDll, StringComparison.OrdinalIgnoreCase) == 0
        //                    && categoryInfos.ItemB == categoryCount)
        //                {
        //                    continue;
        //                }
        //            }
        //            EventLog.DeleteEventSource(eventSource);
        //        }
        //        EventSourceCreationData eventSourceCreationData = new EventSourceCreationData(eventSource, eventLogName);
        //        if (File.Exists(categoryDll))
        //        {
        //            eventSourceCreationData.MessageResourceFile = categoryDll;
        //            eventSourceCreationData.ParameterResourceFile = categoryDll;
        //            eventSourceCreationData.CategoryResourceFile = categoryDll;
        //            eventSourceCreationData.CategoryCount = categoryCount;
        //        }
        //        eventSourceCreationData.MachineName = ".";

        //        //http://msdn.microsoft.com/en-us/library/2awhba7a.aspx
        //        //If a source has already been mapped to a log and you remap it to a new log, you must restart the computer for the changes to take effect.
        //        EventLog.CreateEventSource(eventSourceCreationData);
        //    }
        //}

        internal static string ToEventSourceString(EventSources eventSource)
        {
            string prefix = "DocAve6 ";
            if (IsSMSP()) prefix = "SMSP7 ";
            switch (eventSource)
            {
                case EventSources.DocAveMediaService:
                    return prefix + DocAveMediaService;
                case EventSources.DocAveControlService:
                    return prefix + DocAveControlService;
                case EventSources.DocAveAgentService:
                    return prefix + DocAveAgentService;
                case EventSources.DocAveReportService:
                    return prefix + DocAveReportService;
                case EventSources.DocAvePackageService:
                    return prefix + DocAvePackageService;
                case EventSources.DocAveToolService:
                    return prefix + DocAveToolService;
                case EventSources.DocAveCLIService:
                    return prefix + DocAveCLIService;
                case EventSources.DocAveAPIService:
                    return prefix + DocAveAPIService;
                case EventSources.DocAveStorageAPIService:
                    return prefix + DocAveStorageAPIService;
                default:
                    return string.Empty;
            }
        }

        private static bool IsSMSP()
        {
            bool isSMSP = false;
            var productType = AppDomain.CurrentDomain.GetData("ProductType");
            if (productType != null)
            {
                if (string.Compare("SMSP", productType.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    isSMSP = true;
                }
                else
                {
                    isSMSP = false;
                }
            }
            else
            {
                //IIS may load assembly from C:\Windows\Microsoft.NET\Framework64\v2.0.50727\Temporary ASP.NET Files\......
                //if (typeof(EventSourcesUtil).Assembly.Location.Contains("SMSP"))
                if (AppDomain.CurrentDomain.BaseDirectory.Contains("SMSP"))
                {
                    isSMSP = true;
                }
                else
                {
                    isSMSP = false;
                }
            }
            return isSMSP;
        }

        //private static void DeleteObsoleteEventSources()
        //{
        //    foreach (string eventSource in ObsoleteEventSources.D60EventSources)
        //    {
        //        if (EventLog.SourceExists(eventSource))
        //        {
        //            EventLog.DeleteEventSource(eventSource);
        //        }
        //    }
        //}
    }

    internal class ObsoleteEventSources
    {
        public static string[] D60EventSources = new string[]{
            "DocAve-Media-Service",
            "DocAve-Control-Service",
            "DocAve-Timer-Service",
            "DocAve-Agent-Service",
            "DocAve-Report-Service",
            "DocAve-Package-Service",
            "DocAve-Tool-Service",
            "DocAve-CLI-Service",
            "DocAve-API-Service",
            "DocAve-Storage",
        };

    }

}