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



using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.ReportCenter.AdminReport.Object;

namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    [KnownType(typeof(AuditorDefinition))]
    [KnownType(typeof(AdminReportCollectorDefinition))]
    [KnownType(typeof(AuditReportDefinition))]
    [KnownType(typeof(AuditPruningDefinition))]
    [KnownType(typeof(DocAveAuditorDefinition))]
    [KnownType(typeof(StorageTrendsCollectorDefinition))]
    [KnownType(typeof(ManagementAPIReportDefinition))]
    [KnownType(typeof(UsageReportDefinition))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BaseCollectorDefinition
    {
        [DataMember]
        public BaseScope Scope { get; set; }

        //Schedule收集时使用
        [DataMember]
        public virtual int BaseReportType { get; set; }

        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public string UserId { set; get; }

        /// <summary>
        /// DocAve current display language
        /// </summary>
        [DataMember]
        public string CultureName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public AnonymousSettingDto AnonymousSetting { get; set; }
    }
}
