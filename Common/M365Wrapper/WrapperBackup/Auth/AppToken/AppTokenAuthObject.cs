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

namespace ExchangeUtility.Graph
{
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using M365.Wrapper.Backup.Auth.Common;
    using System;

    public abstract class AppTokenAuthObject : AuthObject, IAppTokenAuthObject
    {

        #region Input

        public string TenantId;

        public string ResourceUrl { get; private set; }

        public bool IsCustomerApp { get; internal set; }

        public DelegateAppCloudBackupModuleType DelegateAppCloudBackupModuleType { get; set; }

        public bool IsDelegateApp { get; set; }

        #endregion

        #region Result

        protected string accessToken { get; set; }

        #endregion

        internal AppTokenAuthObject(AuthenticationInfo authenticationInfo, string userName, string ewsServiceUrl, ImpersonateUserInfo impersonateUserInfo = null)
           : base(userName, ewsServiceUrl, impersonateUserInfo)
        {
            if (string.IsNullOrEmpty(authenticationInfo?.TenantId)) throw new ArgumentNullException("authenticationInfo.TenantId");
            if (string.IsNullOrEmpty(authenticationInfo?.Resource)) throw new ArgumentNullException("authenticationInfo.Resource");
            TenantId = authenticationInfo?.TenantId;
            ResourceUrl = authenticationInfo?.Resource;
            Environment = authenticationInfo.Environment;
        }

        public abstract string GetAccessToken();

        public override AuthObjectType AuthType => AuthObjectType.AccessToken;

        public abstract TokenPermissionType PermissionType { get; }

    }
}