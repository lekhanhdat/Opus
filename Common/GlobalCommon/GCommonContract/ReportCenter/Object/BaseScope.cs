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
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;
    using AvePoint.GCommon.Contract.AccountManager.Object;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.GCommon.Contract.ReportCenter.AdminReport.Object;
    using AvePoint.GCommon.Contract.ReportCenter.AuditReport.MgtApiReport;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.SharePointBrowser.Object;
    using AvePoint.GCommon.Contract.Tree.Object;

    /// <summary>
    /// The base of scope
    /// </summary>
    [KnownType(typeof(SPUserScope))]
    [KnownType(typeof(SPTreeScope))]
    [KnownType(typeof(TimeScope))]
    [KnownType(typeof(AdminReportScope))]
    [KnownType(typeof(ScheduleExportReportScope))]
    [KnownType(typeof(ExportReportSettingsScope))]
    [KnownType(typeof(UsageReportScope))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BaseScope
    {
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdminReportScope : BaseScope
    {
        [DataMember]
        public ScheduleExportReportScope ScheduleExport { get; set; }
        [DataMember]
        public RCScheduleDto ExportSchedule { set; get; }
        [DataMember]
        public ReportCondition AdminReportCondition { set; get; }
        [DataMember]
        public string LastModifiedUser { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SPTreeScope : BaseScope
    {
        /// <summary>
        /// SPTreeNodeDto:表示选中节点到根节点组成的最小Tree
        /// NodeList:表示所有选中的节点
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<SPTreeNodeDto> NodeList { get; set; }

        /// <summary>
        /// 保存Profile使用，保存Tree当前的展开状态
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public SPTreeNodeDto EntireTree { get; set; }

        /// <summary>
        /// 选中的最小tree;
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public SPTreeNodeDto SelectedNodeTree { get; set; }

        //RC Server使用，key=>FarmId
        public Dictionary<string, List<SPTreeNodeDto>> FarmNodeMapping { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AuditControllerScope : BaseScope
    {
        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Category { get; set; }
        
        [DataMember]
        public string EmailId { get; set; }

        [DataMember]
        public string FilterId { get; set; }

        [DataMember]
        public ApplyRuleType ApplyRuleType { get; set; }

        /// <summary>
        /// 对应delete data in sharepoint  older than N days 
        /// </summary>
        [DataMember]
        public bool DeleteSPData { get; set; }
        [DataMember]
        public int DaysDeleteData { get; set; }

        [DataMember]
        public bool RetrieveViewItem { get; set; }

        [DataMember]
        public bool RetrieveDeletedSiteCollection { get; set; }

        [DataMember]
        public DateTime LastModifyTime { get; set; }
        [DataMember]
        public string LastModifyUser { get; set; }

        [DataMember]
        public bool MatchIp { get; set; }

        [DataMember]
        public RCScheduleDto ApplySchedule { get; set; }

        [DataMember]
        public RCScheduleDto RetrieveSchedule { get; set; }

        /// <summary>
        /// for apply
        /// </summary>
        [DataMember]
        public List<FilterPolicy> ApplyRuleFilters { get; set; }

        /// <summary>
        /// for apply
        /// </summary>
        [DataMember]
        public Dictionary<PolicyLevel, String> ApplyRuleFilterExpressions { get; set; }

        /// <summary>
        /// for retrieve
        /// </summary>
        [DataMember]
        public List<FilterPolicy> Filters { get; set; }

        /// <summary>
        /// for retrieve
        /// </summary>
        [DataMember]
        public Dictionary<PolicyLevel, String> FilterExpressions { get; set; }
  
        [Obsolete("use Domains")]
        [DataMember]
        public string UserName { get; set; }
        [Obsolete("use Domains")]
        [DataMember]
        public string Password { get; set; }
        //用于访问ad的用户名和密码
        [DataMember]
        public List<DomainDto> Domains { get; set; }

        /// <summary>
        /// 在其他Job中与Auditor Job相同的节点,Auditor Job中使用
        /// </summary>
        [DataMember]
        public List<SPTreeNodeDto> ConflictNodes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public AuditControllerPlanType PlanType { get; set; }

        /// <summary>
        /// 为了使得audit retrieve job 更快获取数据
        /// </summary>
        [DataMember]
        public bool NotRetrieveLowLevel { get; set; }

        /// <summary>
        /// 直接将可用的 audit db 传给 agent.
        /// </summary>
        [DataMember]
        public AuditDatabaseDto AuditDataBase { get; set; }

        [DataMember]
        public string ConnectString { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public AnonymousSettingDto AnonymousSetting { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AuditReportScope : BaseScope
    {
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string FilterId { get; set; }
        /// <summary>
        /// TimeRangeType为Duration时需要设置调用方当前时间到StartTime用来获取调用方的当前时间以及时区
        /// </summary>
        [DataMember]
        public AuditTimeRangeType TimeRangeType { get; set; }
        //for AuditReportTimeRangeType.Duration
        [DataMember]
        public RCTimeUnit TimeUnit { get; set; }
        /// <summary>
        /// 0 = this 
        /// 1 = last 1
        /// 2 = last 2
        /// 依此类推
        /// 
        /// </summary>
        [DataMember]
        public int DurationValue { get; set; }

        //AuditReportTimeRangeType.Duration这个值用于获取调用方的时区
        //导出report时使用这个获取调用方时区
        [DataMember]
        public TimeSpan TimeOffset { get; set; }

        //记录当地的时区
        [DataMember]
        public double Offset { get; set; }

        /// <summary>
        ///  AuditReportTimeRangeType.StartEnd为开始时间
        ///  
        /// </summary>
        [DataMember]
        public DateTimeOffset StartTime { get; set; }
        [DataMember]
        public DateTimeOffset EndTime { get; set; }

        //6.3新添加属性，记录start time 、end time是否支持夏令时
        [DataMember]
        public bool StartTimeDst { get; set; }
        [DataMember]
        public bool EndTimeDst { get; set; }

        [DataMember]
        public AuditReportChartType ReportDataType { get; set; }

        [DataMember]
        public ComplianceReportTitleType ReportDataTitlesType { get; set; }

        [DataMember]
        public ComplianceReportTitleType ReportDataTitlesType4RunNow { get; set; }

        [DataMember]
        public bool CustomExportReportColumn { get; set; }

        [DataMember]
        public bool CustomExportReportColumnRunNow { get; set; }

        [DataMember]
        public List<AuditReportUrlFilter> UrlFilters { get; set; }

        [DataMember]
        public UrlFilterCondition UrlFilter { get; set; }

        [Obsolete]
        [DataMember]
        public bool DownloadReport { get; set; }

        [DataMember]
        public AuditReportType ReportType { get; set; }

        [DataMember]
        public ReportEmailScope EmailScope { get; set; }

        [Obsolete("使用EmailScope代替")]
        [DataMember]
        public RCEmailNotificationDto EmailNotification { get; set; }

        [DataMember]
        public int AuditEvents { get; set; }

        [DataMember]
        public List<AuditItemType> Types { get; set; }

        [Obsolete]
        [DataMember]
        public bool ItemMatchExactly { get; set; }

        [Obsolete]
        [DataMember]
        public string Item { get; set; }

        /// <summary>
        /// schedule export report时代表下载的路径
        /// </summary>
        [DataMember]
        public string DownloadLocationId { get; set; }

        /// <summary>
        /// schedule export report时 download report name
        /// </summary>
        [DataMember]
        public string ScheduleReportName { get; set; }

        /// <summary>
        /// run now schedule download Location
        /// </summary>
        [DataMember]
        public string ExportReportDownloadLocationId { get; set; }

        /// <summary>
        /// run now  download report name
        /// </summary>
        [DataMember]
        public string RunNowReportName { get; set; }

        //标记是否是Run now export 
        [DataMember]
        public bool IsReportChartType { get; set; }
        /// <summary>
        /// run now  download Report Type
        /// </summary>
        [DataMember]
        public AuditReportType RunNowReportType { get; set; }

        [DataMember]
        public RCScheduleDto JobSchedule { get; set; }

        [DataMember]
        public string StartTimeTimeZoneId { get; set; }

        [DataMember]
        public string EndTimeTimeZoneId { get; set; }

        [DataMember]
        public AuditReportChartType ReportChartType { get; set; }


        [DataMember]
        public string LastModifyUser { get; set; }

        [DataMember]
        public bool ZipFileUploadToSP { get; set; }

        //标记是否是Run Now Export
        [DataMember]
        public bool ZipFileForExportNow { get; set; }

        [DataMember]
        public bool ExportEachSiteNewSite { get; set; }

        [DataMember]
        public bool ExportEachSiteNewSiteNow { get; set; }

        public override string ToString()
        {
            return string.Format("AuditReportScope[DownloadReport {0}, reportType {1}, DownloadLocationId {2}]",
                DownloadReport, ReportType, DownloadLocationId);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AuditPruningScope : BaseScope
    {
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public AuditTimeRangeType TimeRangeType { get; set; }
        //for AuditPruningTimeRangeType.OrderThan
        [DataMember]
        public int OrderThanValue { get; set; }
        [DataMember]
        public RCTimeUnit TimeUnit { get; set; }
        //for AuditReportTimeRangeType.StartEnd
        [DataMember]
        public DateTimeOffset StartTime { get; set; }
        [DataMember]
        public DateTimeOffset EndTime { get; set; }

        /// <summary>
        /// GUI显示时用，在查询Profile时把这个值设置上
        /// </summary>
        [DataMember]
        public string LastPruningTime { get; set; }
        [DataMember]
        public string LastModifyUser { get; set; }

        [DataMember]
        public int AuditEvents { get; set; }
        [DataMember]
        public PruningOption PruningOption { get; set; }
        [DataMember]
        public AuditDataQueryInfo PruningInfo { get; set; }
        [DataMember]
        public bool SaveData { get; set; }
        [DataMember]
        public bool CompressData { get; set; }

        [DataMember]
        public string DownloadLocationId { get; set; }

        [DataMember]
        public RCScheduleDto JobSchedule { get; set; }

        public override string ToString()
        {
            return string.Format("AuditPruningScope[PruningOption {0}, AuditDataPruningInfo {1}, DownloadLocationId {2}, SaveData {3}]",
                PruningOption, PruningInfo, DownloadLocationId, SaveData);
        }
    }
    
    [DataContract(Namespace =ContractConstants.Namespace)]
    public class UsageReportScope : BaseScope
    {
        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public AuditTimeRangeType TimeRangeType { get; set; }

        [DataMember]
        public RCTimeUnit TimeUnit { get; set; }
        /// <summary>
        /// 0 = this 
        /// 1 = last 1
        /// 2 = last 2
        /// 依此类推
        /// 
        /// </summary>
        [DataMember]
        public int DurationValue { get; set; }

        [DataMember]
        public double Offset { get; set; }

        [DataMember]
        public DateTimeOffset StartTime { get; set; }

        [DataMember]
        public DateTimeOffset EndTime { get; set; }

        [DataMember]
        public string StartTimeTimeZoneId { get; set; }

        [DataMember]
        public string EndTimeTimeZoneId { get; set; }

        [DataMember]
        public bool StartTimeDst { get; set; }
        [DataMember]
        public bool EndTimeDst { get; set; }

        [DataMember]
        public AuditReportType ExportReportType { get; set; }

        [DataMember]
        public ReportEmailScope EmailScope { get; set; }

        //sites,pages, users, items,lists
        [DataMember]
        public int SiteActivityRankingTypes { get; set; }

        [DataMember]
        public string ExportLocationId { get; set; }

        [DataMember]
        public RCScheduleDto JobSchedule { get; set; }

        [DataMember]
        public UsageReportChartType ReportChartType { get; set; }

        [DataMember]
        public bool ZipFileUploadToSP { get; set; }

        // site activity ranking, active user, download ranking, site visitor
        [DataMember]
        public int ReportTypes { get; set; }

        [DataMember]
        public DateTime LastModifyTime { get; set; }

        [DataMember]
        public string LastModifyUser { get; set; }

        [DataMember]
        public APIUserFilterCondition UserFilter { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExportReportSettingsScope : BaseScope
    {
        [DataMember]
        public  DateTime StartTime;
        [DataMember]
        public  DateTime EndTime;
        [DataMember]
        public ExportReportType ExportReportType;
        [DataMember]
        public TimeSpan TimeSpan;
        [DataMember]
        public ReportType ReportType;
        [DataMember]
        public string FileName;
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SPUserScope : BaseScope
    {
        //已删除，不再使用
        [DataMember]
        public bool IncludeAllUsers { get; set; }
        [DataMember]
        public SPUserType UserType { get; set; }
        [DataMember]
        public List<UserDetail> Users { get; set; }

        /// <summary>
        /// 因为已经有UserType指定include或者是exclude了，所以IncludeUsers和ExcludeUsers不需要了，请使用Users
        /// </summary>
        [Obsolete]
        [DataMember]
        public List<UserDetail> IncludeUsers { get; set; }
        [Obsolete]
        [DataMember]
        public List<UserDetail> ExcludeUsers { get; set; }
        //是否包含匿名用户
        [DataMember]
        public bool IsIncludeAnonymousUsers { get; set; }

        public override string ToString()
        {
            StringBuilder buider = new StringBuilder();
            buider.Append(string.Format("UserType:{0}", UserType.ToString()));
            if (IncludeUsers != null && IncludeUsers.Count > 0)
            {
                List<string> names = new List<string>();
                foreach (var item in IncludeUsers)
                {
                    names.Add(item.SPLoginName.ToLower());
                }
                names.Sort();
                buider.Append(string.Format("IncludeUsers:{0}", string.Join("|", names.ToArray())));
            }
            if (ExcludeUsers != null && ExcludeUsers.Count > 0)
            {
                List<string> names = new List<string>();
                foreach (var item in ExcludeUsers)
                {
                    names.Add(item.SPLoginName.ToLower());
                }
                names.Sort();
                buider.Append(string.Format("ExcludeUsers:{0}", string.Join("|", names.ToArray())));
            }
            return buider.ToString();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ScheduleExportReportScope : BaseScope
    {
        [DataMember]
        public RCScheduleDto Schedule { get; set; }

        [DataMember]
        public string Format { get; set; }

        [DataMember]
        public string LocationId { get; set; }

        [DataMember]
        public int ExportServiceType { get; set; }

        [DataMember]
        public int ExportCategory { get; set; }

        [DataMember]
        public BaseChart Chart { get; set; }

        [DataMember]
        public AuditTimeRangeType TimeRangeType { get; set; }

        [DataMember]
        public RCTimeUnit TimeUnit { get; set; }

        [DataMember]
        public int DurationValue { get; set; }

        [DataMember]
        public TimeSpan Offset { get; set; }

        [DataMember]
        public DateTimeOffset StartTime { get; set; }

        [DataMember]
        public DateTimeOffset EndTime { get; set; }

        [DataMember]
        public ReportEmailScope ReportEmailScope { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReportEmailScope
    {
        [DataMember]
        public bool IsSendEmail { get; set; }

        [DataMember]
        public string NotificationId { get; set; }

        [DataMember]
        public bool IsAttachReport { get; set; }

        [DataMember]
        public int ReportSize { get; set; }

        public override string ToString()
        {
            return string.Format("IsSendEmail:{0} NotificationId:{1} IsAttachReport:{2} ReportSize:{3}", IsSendEmail, NotificationId, IsAttachReport, ReportSize);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCTimeRange
    {
        /// <summary>
        /// 0 = this 
        /// 1 = last 1
        /// 2 = last 2
        /// 依此类推
        /// </summary>
        [DataMember]
        public int DurationValue { get; set; }

        [DataMember]
        public TimeRangeType TimeRangeType { get; set; }

        [DataMember]
        public RCTimeUnit TimeUnit { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// TimeRangeType = Duration 时调用方的Local当前时间
        /// </summary>
        [DataMember]
        public DateTime LocalNow { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RCTimeUnit
    {
        [EnumMember]
        Second,
        [EnumMember]
        Minute,
        [EnumMember]
        Hour,
        [EnumMember]
        Day,
        [EnumMember]
        Week,
        [EnumMember]
        Month
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum TimeRuleIntervalType
    {
        [EnumMember]
        Day = 0,

        [EnumMember]
        Week = 1,

        [EnumMember]
        Month = 2,

        [EnumMember]
        Year = 3
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DateType
    {
        [EnumMember]
        CreatedDate = 0,

        [EnumMember]
        LastModifiedDate = 1,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RuleType
    {
        [EnumMember]
        SizeRule = 0,

        [EnumMember]
        TimeRule = 1,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum TimeRangeType
    {
        [EnumMember]
        Undefined = 0,
        [EnumMember]
        Duration = 1,
        [EnumMember]
        StartEnd = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ObjectType
    {
        [EnumMember]
        Attachment = 0,

        [EnumMember]
        Document = 1,

        [EnumMember]
        DocumentVersion = 2,

        [EnumMember]
        Item = 4,

        [EnumMember]
        ItemVersion = 8
    }
}
