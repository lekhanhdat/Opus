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
using AvePoint.GCommon.Contract.SharePointBrowser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Office365AppToken.Object
{
    public class CreateAppResult
    {
        public bool IsSuccess { get; set; }
        public string AppProfileId { get; set; }
        public string AppId { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class GetTenantIdResult
    {
        public string TenantId { get; set; }
        public AzureRegions AzureRegion { get; set; }
        public GetTenantIdResultType ResultType { get; set; }
    }

    public enum GetTenantIdResultType
    {
        success = 0,
        UsernameIsNull = 1,
        NoAvalibleAgent = 2,
        GetResponseException = 3,
        Exception = 4,
        NoTenantId = 5
    }
    
    public class ResultEunmMessage
    {
        public BrowserResultEnum ResultEnum { get; set; }
        public string ErrorMessage { get; set; }
    }

    public enum BrowserResultEnum
    {
        Success,
        UnKnown,
        NoSiteCollection,
        UnAuthorized,
        PasswordExpired,
        PasswordNotMatch,
        BadUrl,
        TimeOut,
        HostnameCannotResolved,
        WebApplicationNotFound,
        DotNet45Required,
        UserNameInvalid, //online cannont get tenantid by username
        DifferentTenant,// online app profile和Account取到的TenantId不同
        CommonError, //control 操作异常时
        AppCertificateError,//app certificate mismatch
        NoAvailableAgent,
        GetResponseException
    }
}
