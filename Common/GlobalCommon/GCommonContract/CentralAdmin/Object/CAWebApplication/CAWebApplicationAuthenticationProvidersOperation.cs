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





namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAWebApplicationAuthenticationProvidersOperation : CAOperation
    {
        [DataMember]
        public String WebAppUrl { get; set; }

        //由于sharepoint中一个web app可以Extend多个zone,但是公共代码默认的只有一个zone
        //修改后
        [DataMember]
        public List<AuthenticationUrlZoneIisSettingInfo> ZonesIisSettings { get; set; }

    }
    public class AuthenticationUrlZoneIisSettingInfo
    {
        /// <summary>
        /// For GUI
        /// </summary>
        public string SharePointZoneStr { get; set; }

        [DataMember]
        public SharePointUrlZone Zone { get; set; }

        [DataMember]
        public SharePointAuthenticationMode AuthenticationMode { get; set; }

        [DataMember]
        public Boolean IsAllowAnonymous { get; set; }

        //更改名称
        [DataMember]
        public Boolean IsEnableWindowsAuthentication { get; set; }

        [DataMember]
        public Boolean IsDisableKerberos { get; set; }

        [DataMember]
        public Boolean IsUseIntegratedWindowsAuthentication { get; set; }

        [DataMember]
        public Boolean IsUseBasicAuthentication { get; set; }

        // 新增数据
        [DataMember]
        public Boolean IsEnableFormsBasedAuthentication { get; set; }

        //新增数据
        [DataMember]
        public Boolean IsTrustedIdentityProvider { get; set; }

        //新增数据
        [DataMember]
        public List<String> TrustedIdentityProviders { get; set; }

        //新增数据
        [DataMember]
        public string SelectedTrustedProvider { get; set; }

        //新增数据
        [DataMember]
        public Boolean IsDefaultSignInPage { get; set; }

        //新增数据
        [DataMember]
        public String CustomSignInPageURL { get; set; }

        [DataMember]
        public Boolean IsEnableClientIntegration { get; set; }

        [DataMember]
        public Boolean IsClientObjectModelRequiresUseRemoteAPIsPermission { get; set; }

        [DataMember]
        public String RoleManager { get; set; }

        //新增数据
        [DataMember]
        public String ASPNETMembershipProviderName { get; set; }

        //新增数据
        [DataMember]
        public String ASPNETRolemanagerName { get; set; }

        [DataMember]
        public String MembershipProvider { get; set; }

        [DataMember]
        public Boolean IsUsableAuthenticationTypeWindows { get; set; }

        [DataMember]
        public Boolean IsUsableAuthenticationTypeForms { get; set; }

        [DataMember]
        public Boolean IsUsableAuthenticationTypeWebSingleSignOn { get; set; }

    }
}
