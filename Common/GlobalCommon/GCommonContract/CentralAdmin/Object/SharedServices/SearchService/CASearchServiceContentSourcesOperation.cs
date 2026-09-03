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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object.SharedServices.SearchService
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASearchServiceContentSourcesOperation : CAOperation
    {
        [DataMember]
        public string ServiceApplicationId { get; set; }

        [DataMember]
        public List<CASearchServiceContentSource> ContentSources { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASearchServiceContentSource
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public ContentSourceType ContentSourceType { get; set; }

        [DataMember]
        public ContentSourceStatus CurrentStatus { get; set; }

        [DataMember]
        public string CurrentCrawlDuration { set; get; }

        [DataMember]
        public string LastCrawType { set; get; }

        [DataMember]
        public string LastCrawlBegan { set; get; }

        [DataMember]
        public string LastCrawlDuration { set; get; }

        [DataMember]
        public string LastCrawCompleted { set; get; }

        [DataMember]
        public string NextFullCrawl { set; get; }

        [DataMember]
        public string NextIncrementalCrawl { set; get; }

        [DataMember]
        public CrawlSettings ContentSourceCrawlSettings { set; get; }

        [DataMember]
        public CrawlSchedule FullCrawlSchedule { set; get; }

        [DataMember]
        public CrawlSchedule IncrementalCrawlSchedule { set; get; }

        [DataMember]
        public ContentSourcePriority ContentSourcePriority { set; get; }

        [DataMember]
        public bool StartFullCrawl { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ContentSourceType
    {
        [EnumMember]
        Business = 0,
        [EnumMember]
        O12Business = 1,
        [EnumMember]
        CustomRepository = 2,
        [EnumMember]
        Custom = 3,
        [EnumMember]
        Exchange = 4,
        [EnumMember]
        File = 5,
        [EnumMember]
        LotusNotes = 6,
        [EnumMember]
        SharePoint = 7,
        [EnumMember]
        Web = 8
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ContentSourceStatus
    {
        // Summary:
        //     The content source is not being crawled.
        [EnumMember]
        Idle = 0,
        //
        // Summary:
        //     The crawler is executing a full crawl on the content source.
        [EnumMember]
        CrawlingFull = 1,
        //
        // Summary:
        //     The content source's crawl is paused.
        [EnumMember]
        Paused = 2,
        //
        [EnumMember]
        Throttled = 3,
        //
        // Summary:
        //     The crawler is recovering from a crawl of the content source.
        [EnumMember]
        Recovering = 4,
        //
        // Summary:
        //     The crawler is shutting down the content source's crawl.
        [EnumMember]
        ShuttingDown = 5,
        //
        // Summary:
        //     The crawler is executing an incremental crawl on the content source.
        [EnumMember]
        CrawlingIncremental = 6,
        //
        // Summary:
        //     The crawler is processing notifications for the content source.
        [EnumMember]
        ProcessingNotifications = 7,
        //
        // Summary:
        //     The content source's crawl is stopping.
        [EnumMember]
        CrawlStopping = 8,
        //
        // Summary:
        //     The content source's crawl is being paused.
        [EnumMember]
        CrawlPausing = 9,
        //
        [EnumMember]
        CrawlResuming = 10,
        //
        [EnumMember]
        CrawlStarting = 11,
        //
        [EnumMember]
        CrawlCompleting = 12,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ContentSourcePriority
    {
        [EnumMember]
        Normal,
        [EnumMember]
        High,
    }

    #region Schedule Setting
    [KnownType(typeof(DailyCrawlSchedule))]
    [KnownType(typeof(WeeklyCrawlSchedule))]
    [KnownType(typeof(MonthlyCrawlSchedule))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CrawlSchedule
    {
        [DataMember]
        public ScheduleType ScheduleType { set; get; }

        [DataMember]
        public int StartHour { set; get; }

        [DataMember]
        public bool NeedRepeat { set; get; }

        [DataMember]
        public int RepeatInterval { set; get; }

        [DataMember]
        public int RepeatDuration { set; get; }

        //[DataMember]
        //public string ScheduleDescription { set; get; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScheduleType
    {
        [EnumMember]
        DailySchedule,

        [EnumMember]
        WeeklySchedule,

        [EnumMember]
        MonthlySchedule,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DailyCrawlSchedule : CrawlSchedule
    {
        [DataMember]
        public int DaysInterval { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class WeeklyCrawlSchedule : CrawlSchedule
    {
        [DataMember]
        public int WeeksInterval { set; get; }

        [DataMember]
        public int DaysOfWeek { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MonthlyCrawlSchedule : CrawlSchedule
    {
        [DataMember]
        public int MonthsInterval { set; get; }

        [DataMember]
        public int DayOfMonth { set; get; }

        [DataMember]
        public int MonthsOfYear { set; get; }
    }

    [Flags]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DaysOfWeek
    {
        // Summary:
        //     Specifies Sunday as the week day for the crawl schedule.
        [EnumMember]
        Sunday = 1,
        //
        // Summary:
        //     Specifies Monday as the day of the week for the crawl schedule.
        [EnumMember]
        Monday = 2,
        //
        // Summary:
        //     Specifies Tuesday as the day of the week for the crawl schedule.
        [EnumMember]
        Tuesday = 4,
        //
        // Summary:
        //     Specifies Wednesday as the day of the week for the crawl schedule.
        [EnumMember]
        Wednesday = 8,
        //
        // Summary:
        //     Specifies Thursday as the day of the week for the crawl schedule.
        [EnumMember]
        Thursday = 16,
        //
        // Summary:
        //     Specifies Friday as the day of the week for the crawl schedule.
        [EnumMember]
        Friday = 32,
        //
        // Summary:
        //     Specifies Saturday as the day of the week for the crawl schedule.
        [EnumMember]
        Saturday = 64,
    }

    [Flags]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum MonthsOfYear
    {
        // Summary:
        //     Specifies January as the month for the crawl schedule.
        [EnumMember]
        January = 1 << 0,
        //
        // Summary:
        //     Specifies February as the month for the crawl schedule.
        [EnumMember]
        February = 1 << 1,
        //
        // Summary:
        //     Specifies March as the month for the crawl schedule.
        [EnumMember]
        March = 1 << 2,
        //
        // Summary:
        //     Specifies April as the month for the crawl schedule.
        [EnumMember]
        April = 1 << 3,
        //
        // Summary:
        //     Specifies May as the month for the crawl schedule.
        [EnumMember]
        May = 1 << 4,
        //
        // Summary:
        //     Specifies June as the month for the crawl schedule.
        [EnumMember]
        June = 1 << 5,
        //
        // Summary:
        //     Specifies July as the month for the crawl schedule.
        [EnumMember]
        July = 1 << 6,
        //
        // Summary:
        //     Specifies August as the month for the crawl schedule.
        [EnumMember]
        August = 1 << 7,
        //
        // Summary:
        //     Specifies September as the month for the crawl schedule.
        [EnumMember]
        September = 1 << 8,
        //
        // Summary:
        //     Specifies October as the month for the crawl schedule.
        [EnumMember]
        October = 1 << 9,
        //
        // Summary:
        //     Specifies November as the month for the crawl schedule.
        [EnumMember]
        November = 1 << 10,
        //
        // Summary:
        //     Specifies December as the month for the crawl schedule.
        [EnumMember]
        December = 1 << 11,
    }
    #endregion

    #region Content Source Type
    [KnownType(typeof(SharePointSitesCrawlSettings))]
    [KnownType(typeof(WebSitesCrawlSettings))]
    [KnownType(typeof(FileSharesCrawlSettings))]
    [KnownType(typeof(ExchangePublicFoldersCrawlSettings))]
    [KnownType(typeof(LineOfBusinessDataCrawlSettings))]
    [KnownType(typeof(CustomRepositoryCrawlSettings))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CrawlSettings
    {

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SharePointSitesCrawlSettings : CrawlSettings
    {
        [DataMember]
        public List<string> StartAddresses { set; get; }

        [DataMember]
        public SharePointCrawlType CrawlType { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class WebSitesCrawlSettings : CrawlSettings
    {
        [DataMember]
        public List<string> StartAddresses { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FileSharesCrawlSettings : CrawlSettings
    {
        [DataMember]
        public List<string> StartAddresses { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExchangePublicFoldersCrawlSettings : CrawlSettings
    {
        [DataMember]
        public List<string> StartAddresses { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LineOfBusinessDataCrawlSettings : CrawlSettings
    {

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CustomRepositoryCrawlSettings : CrawlSettings
    {

    }

    public enum SharePointCrawlType
    {
        CrawlEverythingUnderHostName,

        OnlyCrawlSiteCollection,
    }

    public enum FileSharesCrawlType
    {
        CrawlFolderAndSubFolders,

        OnlyCrawlFolder,
    }

    public enum WebSiteCrawlType
    {
        OnlyCrawlWithinServer,

        OnlyCrawlFirstPage,

        Custom,
    }

    public enum ExchangePublicFoldersCrawlType
    {
        CrawlFolderAndSubFolders,

        OnlyCrawlFolder,
    }

    #endregion
}
