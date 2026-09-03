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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.Security;
using AvePoint.RA.Contract.Logon;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface ILoginService
    {
        //Task<(RAReturnMessage, RMIdentity)> Office365LoginAsync(LogOnInfo model);

        System.Threading.Tasks.Task LogOutAsync(RMIdentity identity);

        Task<ClaimsPrincipal> ConvertClaimsPrincipalAsync(RMIdentity identity);

        System.Threading.Tasks.Task UpdateSessionTimeoutSettingAsync(int sessionTimeout);

        Task<(RAReturnMessage, RMIdentity)> SSOLoginAsync(RMLogonInfo logonInfo);
        Task<(RAReturnMessage, RMIdentity)> SSOLoginAsync(RMLogonInfo logonInfo, RMIdentity identity);
        string GetSsoLoginFailedMessage(string failedType);
        bool AcceptLicenseAgreement(SsoSamplerUserInfo userInfo, string ipAddress);
        Task<(RAReturnMessage, RMLoginInfo)> MobileSSOLogin(string state, string token, string accessToken);
        System.Threading.Tasks.Task InitSecurityProfileAsync();
    }
}
