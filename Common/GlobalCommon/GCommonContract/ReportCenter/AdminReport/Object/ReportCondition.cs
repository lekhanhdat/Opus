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
using AvePoint.GCommon.Contract.ReportCenter.Object;
using AvePoint.GCommon.Contract.Server.Common.ExportLocation.Object;
using AvePoint.GCommon.Contract.Server.Common.ExportReport.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.ReportCenter.AdminReport.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdminReportCollectorDefinition : BaseCollectorDefinition
    {
        [DataMember]
        public ReportCondition reportCondition { set; get; }

        [DataMember]
        public override int BaseReportType
        {
            get
            {
                return (int)ReportType.AdminReport;
            }
        }
        [DataMember]
        public bool IsSkipAll { set; get; }

        public override string ToString()
        {
            return reportCondition == null ? "null" : reportCondition.ToString();
        }

    }
    [Flags]
    public enum AdminReportIncludeType
    {
        None = 0,

        IncludeSummary = 1,

        IncludeDetail =2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReportCondition
    {
        [DataMember]
        public bool EmailSet { set; get; }
        [DataMember]
        public RCEmailNotificationScope AdminReportEmailScope { set; get; }

        [DataMember]
        public ScheduleExportReportScope ScheduleExport { set; get; }
        [DataMember]
        public bool IsExportNow { set; get; }

        [DataMember]
        public DownloadInfo DownloadInfo { set; get; }
      
        [DataMember]
        public NodeLevel Level { set; get; }
        [DataMember]
        public List<SPTreeNodeDto> Nodes { set; get; }
        [DataMember]
        public List<SettingCondition> SettingCondition { set; get; }
        [DataMember]
        public AdminReportJobContext JobContext { set; get; }
        [DataMember]
        public Dictionary<NodeLevel, List<SettingCondition>> SettingConditions { get; set; }
        [DataMember]
        public List<NodeLevel> IncludeSummaryReportLevel { get; set; }

        [DataMember]
        public AdminReportIncludeType AdminReportIncludeType { get; set; }
        [DataMember]
        public FilterPolicyInfo Filter { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DownloadInfo
    {
        [Obsolete("因为兼容旧数据所以保留，请使用DownloadLocationEx")]
        [DataMember]
        public ExportLocationDto DownloadLocation { set; get; }

        [DataMember]
        public ExportReportDto DownloadLocationEx { set; get; }
        [DataMember]
        public AdminReportDownloadType DownloadType { set; get; }
        [DataMember]
        public string CustomFileName { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AdminReportDownloadType
    {
        [EnumMember]
        PDF,
        [EnumMember]
        CSV,
        [EnumMember]
        XLS,
        [EnumMember]
        XLSX,
        [EnumMember]
        XML
    }
}