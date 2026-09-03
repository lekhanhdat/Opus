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



namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.ReportCenter.Common;
    using AvePoint.GCommon.Contract.Server.Audit;

    #endregion
    [KnownType(typeof(DateTimeOffsetPair))]
    [KnownType(typeof(DocAveAuditChart))]
    [KnownType(typeof(DocAveAuditReportType))]
    [KnownType(typeof(AveAuditDataInfo))]
    [KnownType(typeof(AveAuditDataPropertyInfo))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DocAveAuditChart : BaseChart
    {

        [DataMember]
        public ScopeProfile Profile { get; set; }

        [DataMember]
        public List<AveAuditDataDto> DocAveAuditDatas { get; set; }

        [DataMember]
        public string ContinuationToken { get; set; }

        [DataMember]
        public string NextContinuationToken { get; set; }

        [DataMember]
        public Dictionary<AveAuditDataProperty,object> Filters { get; set; }

        [DataMember]
        public DocAveAuditReportType DocAveAuditReportType { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AveAuditDataInfo
    {
        [DataMember]
        public DateTime Occurred { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string ClientIP { get; set; }
        [DataMember]
        public AveAuditorObjectType ObjectType { get; set; }
        [DataMember]
        public string Detail { get; set; }
        [DataMember]
        public AveAuditorActionType Action { get; set; }
        [DataMember]
        public ModuleEnum Module { get; set; }
        [DataMember]
        public AveAuditStatus Status { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DocAveAuditReportType
    {
        [EnumMember]
        Undefined = 0,
        [EnumMember]
        ViewData = 1,
        [EnumMember]
        RunExportReport = 2,
        [EnumMember]
        SCOMData = 3,
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DisplayBy
    {
        [EnumMember]
        Time,
        [EnumMember]
        User,
        [EnumMember]
        Role,
        [EnumMember]
        Module,
        [EnumMember]
        Object,
        [EnumMember]
        Action,
        [EnumMember]
        Status
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RequestTypeForDocAveAudit
    {
        [EnumMember]
        LoadPage,
        [EnumMember]
        NextPage,
        [EnumMember]
        PreviousPage,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AveAuditDataPropertyInfo
    {
        [EnumMember]
        Undefined = 0,
        [EnumMember]
        UserName = 1,
        [EnumMember]
        Action = 2,
        [EnumMember]
        Status = 3,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DocAveAuditItem
    {
        [DataMember]
        public long Time { get; set; }
        [DataMember]
        public string User { get; set; }
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string Role { get; set; }
        [DataMember]
        public AveAction Action { get; set; }
        [DataMember]
        public AveAuditStatus Status { get; set; }
        [DataMember]
        public string Object { get; set; }

        //For Export
        [DataMember]
        public DateTime ExportTime { get; set; }
        [DataMember]
        public string ExportModule { get; set; }
        [DataMember]
        public string ExportAction { get; set; }
        [DataMember]
        public string ExportStatus { get; set; }

        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Content { get; set; }
        [DataMember]
        public string Method { get; set; }

    }
}
