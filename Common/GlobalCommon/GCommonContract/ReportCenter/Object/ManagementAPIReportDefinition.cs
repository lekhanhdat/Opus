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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.ReportCenter.AuditReport.MgtApiReport;
using AvePoint.GCommon.Contract.Server.Common.ExportReport.Object;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.ReportCenter.AuditReport;
using AvePoint.GCommon.Contract.CentralAdmin.Object;

namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ManagementAPIReportDefinition : BaseCollectorDefinition
    {
        /// <summary>
        /// RunReport or DownloadReportNew or RunReportAndExport
        /// </summary>
        [DataMember]
        public ManagementAPIReportChartType DefinitionType { get; set; }

        [DataMember]
        public ExportReportDto ExportReportDto { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public DateTime EndTime { get; set; }

        [DataMember]
        public AuditReportType ReportType { get; set; }

        [DataMember]
        public string CustomReportFileName { get; set; }

        [DataMember]
        public APIActionFilterCondition ActionFilter { get; set; }

        [DataMember]
        public APIUrlFilterCondition UrlFilter { get; set; }

        [DataMember]
        public APIUserFilterCondition UserFilter { get; set; }

        [DataMember]
        public List<O365ActivityType> ProductTypes { get; set; }

        /// <summary>
        /// Azure AD 中的 Office 365 Group filter
        /// </summary>
        [DataMember]
        public List<O365GroupType> O365GroupTypes { get; set; }

        /// <summary>
        /// SharePoint 站点分类
        /// </summary>
        [DataMember]
        public List<SharePointOnlineSitesType> SharePointSiteTypes { get; set; }

        /// <summary>
        /// 为了支持 Group 导出 each site
        /// </summary>
        [DataMember]
        public Dictionary<string, BposInfo> GroupTeamSites { get; set; }

        [DataMember]
        public TimeSpan TimeOffset { get; set; }

        [DataMember]
        public double Offset { get; set; }

        [DataMember]
        public string PlanName { get; set; }

        [DataMember]
        public bool IsDBOverSize { get; set; }

        [DataMember]
        public bool ZipFileToSP { get; set; }

        [DataMember]
        public string MgtApiConnString { get; set; }

        [DataMember]
        public ManageApiReportTitleType ReportDataTitlesType { get; set; }

        [DataMember]
        public bool CustomExportReportColumn { get; set; }

        [DataMember]
        public bool BreakInheritance { get; set; }

        [DataMember]
        public bool SpecificTenant { get; set; }

        [DataMember]
        public List<string> Tenants { get; set; }
    }
}
