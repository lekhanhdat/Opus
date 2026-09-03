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
using AvePoint.Common.Module.JobMonitor.Entities;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Action;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.Server.Common.ExportReport.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExportReportDto : IProfileContent
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public long CreateTime { get; set; }
        [DataMember]
        public long UpdateTime { get; set; }
        [DataMember]
        public ExportReportType ReportType { get; set; }
        [DataMember]
        public PhysicalDeviceDto PhysicalDevice { get; set; }
        [DataMember]
        public SPTreeNodeDto SPTreeNode { get; set; }
        [DataMember]
        public string SPDocumentLibraryName { get; set; }
        [DataMember]
        public bool IsEachSite { get; set; }
        [DataMember]
        public bool IsForceCreate { get; set; }

        [DataMember]
        public int Type { get; set; }
        
        [DataMember]
        public bool IsSystemStorage { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExportJobReportData
    {
        [DataMember]
        public List<string> JobIds { get; set; }
        [DataMember]
        public ReportFileType ReportFileType { get; set; }
        [DataMember]
        public string TimeZoneId { get; set; }
        [DataMember]
        public TimeZoneType ZoneType { get; set; }
        [DataMember]
        public List<string> ColumnKeys { get; set; }
        [DataMember]
        public string ExportLocationProfileId { get; set; }
        [DataMember]
        public string CurrentJobId { get; set; }
        [DataMember]
        public bool IsIncludeSuccessfulJob { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExportReportData
    {
        [DataMember]
        public ReportType ReportType { get; set; }
        [DataMember]
        public string SubJobId { get; set; }
        [DataMember]
        public string ExportLocationProfileId { get; set; }
        [DataMember]
        public string ReportFilePath { get; set; }
        [DataMember]
        public string FileName { get; set; }
        [DataMember]
        public SPTreeNodeDto SPNode { get; set; }
        [DataMember]
        public ExportReportDto Profile { get; set; }
        [DataMember]
        public ServiceDto Agent { get; set; }
        [DataMember]
        public String TenantGroupId { get; set; }
        [DataMember]
        public String TenantGroupOwner { get; set; }
        [DataMember]
        public String TenantUser { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExportReportJobDto : BaseJobDto
    {
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string ReportLocationProfileName { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string ReportLocationProfileId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExportReportResult
    {
        [DataMember]
        public bool IsSuccessful { get; set; }
        [DataMember]
        public List<PropertyItem> PropertyItems { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ExportReportType
    {
        [EnumMember]
        Storage = 0,
        [EnumMember]
        SharePoint = 1
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReportType
    {
        [EnumMember]
        JobReport = 0,
        [EnumMember]
        CAReport = 1,
        [EnumMember]
        RCReport = 2,
    }
}
