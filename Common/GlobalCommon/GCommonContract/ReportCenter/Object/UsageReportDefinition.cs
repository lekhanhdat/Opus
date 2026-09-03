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
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UsageReportDefinition : BaseCollectorDefinition
    {
        [DataMember]
        public ExportReportDto ExportReportDto { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public DateTime EndTime { get; set; }

        [DataMember]
        public AuditReportType ExportReportType { get; set; }

        [DataMember]
        public APIUserFilterCondition UserFilter { get; set; }

        [DataMember]
        public List<SiteActivityRankingType> SiteActivityRankingTypes { get; set; }

        [DataMember]
        public List<UsageReportType> UsageReportTypes { get; set; }

        [DataMember]
        public double Offset { get; set; }

        [DataMember]
        public string PlanName { get; set; }

        [DataMember]
        public bool ZipFileToSP { get; set; }

        [DataMember]
        public string AzureBlobConnString { get; set; }

        [DataMember]
        public string DefaultStorageConnString { get; set; }

    }
}
