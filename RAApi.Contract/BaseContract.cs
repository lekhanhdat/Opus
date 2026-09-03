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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Api.Contract
{
    [DataContract]
    public class BaseContract
    {
        [DataMember]
        public ErrorCode ErrorCode { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }
    }

    [DataContract]
    public enum ErrorCode
    {
        [EnumMember]
        none = 0,
        #region cloud Archive
        //cannot find archive history in backup data.
        [EnumMember]
        NoArchiveHistory = 1,
        [EnumMember]
        AdvanceSearchError = 2,
        //parse stub string error.
        [EnumMember]
        ParseError = 3,
        //for onedrive site, the user id(Office365) not match restore user id(Office365).
        [EnumMember]
        UserPermissionError = 4,
        //Office365TenantId not match
        [EnumMember]
        TenantIDMismatchError = 5,
        //Site collection not found
        [EnumMember]
        SCNotExistOrAccessDenied = 6,
        [EnumMember]
        AuthenticationNotFound = 7,
        //Site collection Locked
        [EnumMember]
        SiteLockedError = 8,
        //Site collection read only
        [EnumMember]
        SiteReadOnlyError = 9,
        [EnumMember]
        GroupNotFound = 10,
        [EnumMember]
        RemoveFromAos = 11,
        #region For Permission Check
        //permission check: site owner permission check failed
        [EnumMember]
        InsufficientPrivileges4SiteOwner = 20,
        //permission check: user don't have stub open permission(stub exist) or sitecollection viewitems permission(stub not exist)
        [EnumMember]
        InsufficientPrivileges4StubView = 21,
        //the user not in Teams or Group owners group
        [EnumMember]
        UserNotInOwnerGroup = 22,
        [EnumMember]
        UserNotInOwnerOrMemberGroup = 23,
        [EnumMember]
        UserNotInOwnerOrSpecificGroup = 24,
        //Archive End User Restore总开关关闭
        [EnumMember]
        DAODoesNotAllowUserRestoreAndExportTotalError = 25,
        //Archiver End User Restore各个Module开关关闭
        [EnumMember]
        DAODoesNotAllowUserRestoreAndExportServiceError = 26,
        [EnumMember]
        ExportSizeLimitReached = 27,
        [EnumMember]
        SiteTypeNotSupport =29,
        [EnumMember]
        UserNotInOwnerOrMemberOrVisitorGroup = 31,
        [EnumMember]
        UserNotLicenseUseFullIndexSearch = 32,
        [EnumMember]
        DAODoesNotAllowUserStubOopError = 33,
        StubNameNotMatch = 34,
        [EnumMember]
        ActiveAppProfileNotFound = 35,
        #endregion
        #endregion

        #region JobMonitor
        [EnumMember]
        GetJobListError = 100,
        [EnumMember]
        JobIdIsNull = 101,
        [EnumMember]
        RevIMKeyIsNull = 102,
        #endregion

        #region
        [EnumMember]
        UnExpectedException = 500,
        [EnumMember]
        NotFound = 501,
        #endregion
    }
}
