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


using AvePoint.GCommon.Contract.ReportCenter.AuditReport;

namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.GCommon.Contract.ReportCenter.AuditReport;

    [AveCodeReviewAttribute("2012/04/22", "DL_DEV_19@avepoint.com", "Zhiwei.Liu@avepoint.com",
    new string[] 
    {
        CodeReviewConstants.CHECK_LIST_ID_HC_2,
    },
    "ADO-25936", true)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AuditReportChart : BaseChart
    {
        [DataMember]
        public AuditReportChartType Type { get; set; }
        [DataMember]
        public ScopeProfile ReportProfile { get; set; }
        [DataMember]
        public List<AuditDataInfo> AuditDatas { get; set; }

        [DataMember]
        public List<ContentTypeInfo> ContentTypeInfos { get; set; }

        /// <summary>
        /// for view report data
        /// </summary>
        [DataMember]
        public SortCondition SortCondition { get; set; }
        /// <summary>
        /// 用于ViewData时过滤条件或者GetFilterContent传递filter values
        /// </summary>
        [DataMember]
        public List<FilterCondition> FilterConditions { set; get; }
        [DataMember]
        public List<SortCondition> SortConditions { get; set; }

        [DataMember]
        public PageInfo PageInfo { get; set; }

        //for report tree
        [DataMember]
        public SPTreeNodeDto Node { get; set; }

        //for view report
        [DataMember]
        public Dictionary<string, int> NodeDataCount { get; set; }

        //for view report
        [DataMember]
        public string JobId { get; set; }

        //for view report
        [DataMember]
        public RCCollectorJobDto ReportJob { get; set; }

        //for AuditReportChartType.GetReportData
        [DataMember]
        public string FileName { get; set; }
        [DataMember]
        public long ReportLength { get; set; }
        [DataMember]
        public long Offset { get; set; }
        [DataMember]
        public int BufferSize { get; set; }
        [DataMember]
        public byte[] ReportData { get; set; }
        [DataMember]
        public AuditReportType ReportType { get; set; }

        /// <summary>
        /// to be deleted
        /// </summary>
        [DataMember]
        public List<RCCollectorJobDto> Jobs { get; set; }

        /// <summary>
        /// 0 success
        /// 1 too much data
        /// </summary>
        public ReportCenterAuditResultType ResultType { get; set; }

        /// <summary>
        /// for Schedule IsRCRunNowSchdule
        /// </summary>
        public bool IsRunNow { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AuditReportChartType
    {
        [EnumMember]
        RunReport = 0,
        [EnumMember]
        RunReportNew = 1,
        [EnumMember]
        DownloadReport = 2,
        [EnumMember]
        DownloadReportNew = 3,
        [EnumMember]
        DownloadReportNewInThread = 4,
        [EnumMember]
        BrowseReportTree = 5,
        [EnumMember]
        ViewData = 6,
        [EnumMember]
        ViewDataNew = 7,
        [EnumMember]
        GetFilterContent = 8,
        [EnumMember]
        GetFilterContentNew = 9,

        //删除report job时删除对应的cache
        [EnumMember]
        DeleteReportCache = 10,
        [EnumMember]
        ViewItemLife = 11,
        [EnumMember]
        ViewListAccess = 12,
        [EnumMember]
        ViewListDeletion = 13,
        [EnumMember]
        ViewSiteAccess = 14,
        [EnumMember]
        ViewUserLife = 15,
        [EnumMember]
        ViewMetaDataChange = 16,
        [EnumMember]
        ViewContentTypeChange = 17,
        [EnumMember]
        ViewUserPermission = 18,
        [EnumMember]
        ViewSiteUserPermission = 19,
        [EnumMember]
        ViewSiteCollectionUserPermission = 20,
        [EnumMember]
        ViewListUserPermission = 21,
        [EnumMember]
        ViewCustomized = 22,

        //获取profile同时获取profile对应的最新的一次jobId
        [EnumMember]
        GetProfile = 23,
        [EnumMember]
        UpdateProfile = 24,
        //export now获取临时report数据
        [EnumMember]
        GetReportData = 25,
        //export now执行完删除临时report
        [EnumMember]
        DeleteTmpReport = 26,
        //export now获取临时report长度
        [EnumMember]
        GetReportLength = 27,

        [EnumMember]
        RunReportNewAndExport = 28,

        [EnumMember]
        GetInstancePlan = 29,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AuditReportType
    {
        [EnumMember]
        Pdf,
        [EnumMember]
        Csv,
        [EnumMember]
        Xlsx,
        [EnumMember]
        Xls
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AuditReportSortType
    {
        [DataMember]
        public static string Time { get { return "occurred"; } }
        [DataMember]
        public static string User { get { return "userName"; } }
        [DataMember]
        public static string ItemType { get { return "itemTypeName"; } }
        [DataMember]
        public static string Action { get { return "eventTypeName"; } }
        [DataMember]
        public static string Url { get { return "url"; } }
        [DataMember]
        public static string WebUrl { get { return "webUrl"; } }
        [DataMember]
        public static string SiteUrl { get { return "siteUrl"; } }
        [DataMember]
        public static string ListUrl { get { return "listUrl"; } }
        [DataMember]
        public static string Title { get { return "title"; } }
        /// <summary>
        /// only for contentTypeChangeReport
        /// </summary>
        [DataMember]
        public static string ContentTypeId { get { return "contentTypeId"; } }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AuditReportFilterType
    {
        /// <summary>
        /// only used for run report web level 
        /// </summary>
        [DataMember]
        public static string WebUrl { get { return "webUrl"; } }

        [DataMember]
        public static string ListId { get { return "listId"; } }

        [DataMember]
        public static string User { get { return "userName"; } }

        [DataMember]
        public static string ItemTypeName { get { return "itemTypeName"; } }

        [DataMember]
        public static string ItemType { get { return "itemType"; } }

        [DataMember]
        public static string EventTypeName { get { return "eventTypeName"; } }

        [DataMember]
        public static string Event { get { return "event"; } }

        [DataMember]
        public static string Url { get { return "url"; } }

        [DataMember]
        public static string ListUrl { get { return "listUrl"; } }

        [DataMember]
        public static string SiteUrl { get { return "siteUrl"; } }

        //[DataMember]
        //public static string ItemId { get { return "itemId"; } }

        [DataMember]
        public static string EventData { get { return "eventData"; } }

        [DataMember]
        public static string UserDisplayName { get { return "displayName"; } }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ContentTypeChangeFilterType
    {
        [DataMember]
        public static string ContentTypeIdGroup { get { return "contentTypeIdGroup"; } }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AuditTimeRangeType
    {
        [EnumMember]
        Duration = 0,
        [EnumMember]
        StartEnd = 1,
        [EnumMember]
        OlderThan = 2,
        [EnumMember]
        DateBefore = 3,
        [EnumMember]
        Last = 4,
    }

    /// <summary>
    /// 该类只使用Url SubUrl 和IsAnd属性
    /// </summary>
    public class AuditReportUrlFilter : SearchFilter
    {
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public string SubUrl { get; set; }
    }
}