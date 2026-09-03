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

namespace AvePoint.GCommon.Contract.CloudAppAdmin.Object
{
    using AvePoint.GCommon.Contract.Common;
    using System.Runtime.Serialization;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CAAExportType
    {
        [EnumMember]
        ExportUserList = 1,

        [EnumMember]
        ExportUserGroupManagement = 2,

        [EnumMember]
        ExportUserLicenseManagement = 3,

        [EnumMember]
        ExportUserApplicationManagement = 4,

        [EnumMember]
        ExportUserEmailAccessManagement = 5,

        [EnumMember]
        ExportGroupList = 6,

        [EnumMember]
        ExportGroupMembers = 7,

        [EnumMember]
        ExportGroupLicenseManagement = 8,

        [EnumMember]
        ExportGroupApplicationManagement = 9,

        [EnumMember]
        ExportGroupEmailAccessManagement = 10,

        [EnumMember]
        BatchCreateUserTemplate=11,

        [EnumMember]
        BatchCreateGroupTemplate=12,

        [EnumMember]
        ExportPEReport = 13,

        [EnumMember]
        ExportPEWhatIfReport = 14,

        [EnumMember]
        ExportPEConflictReport = 15,

        [EnumMember]
        BatchInviteUserTemplate = 16,
    }
}