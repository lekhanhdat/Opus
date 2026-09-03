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
using AvePoint.GCommon.Contract.Common;
using System.Runtime.Serialization;

namespace AvePoint.GCommon.Contract.ReportCenter.AuditReport
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReportCenterAuditDataType
    {
        [EnumMember]
        AuditData = 1,
        [EnumMember]
        AuditReport = 2,
        [EnumMember]
        AuditReportFilter = 4,
        [EnumMember]
        MgtApiReport = 8,
    }

    [Flags]
    public enum ComplianceReportTitleType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Url = 1,
        [EnumMember]
        ItemType = 2,
        [EnumMember]
        UserLoginName = 4,
        [EnumMember]
        UserDisplayName = 8,
        [EnumMember]
        EventAction = 16,
        [EnumMember]
        Title = 32,
        [EnumMember]
        Time = 64,
        [EnumMember]
        Detail = 128,
        [EnumMember]
        OutCome = 256,
        [EnumMember]
        Browser = 512
    }

    [Flags]
    public enum ManageApiReportTitleType
    {
        [EnumMember] None = 0,
        [EnumMember] Date = 1,
        [EnumMember] IP = 2,
        [EnumMember] UserName = 4,
        [EnumMember] DisPlayName = 8,
        [EnumMember] Activity = 16,
        [EnumMember] Item = 32,
        [EnumMember] Url = 64,
        [EnumMember] Detail = 128,
        [EnumMember] DataSource = 256,
        [EnumMember] OperationSystem = 512,
        [EnumMember] AdminActionDetail = 1024,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReportCenterAuditResultType
    {
        [EnumMember]
        Normal = 0,
        [EnumMember]
        DataCountTooLarge = 1,
        [EnumMember]
        ReportCacheExpired = -1,
        [EnumMember]
        ProfileDeleted = -2,
        [EnumMember]
        ProfileChanged = -3,
    }
}
