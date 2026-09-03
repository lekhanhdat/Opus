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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Security;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.Message;
using AvePoint.RA.Contract.Security;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Security
{
    public class SecurityService : ISecurityService
    {

        public RMAccessToken GenerateToken(RMLoginInfo loginInfo, double sessionTimeOutMinute)
        {
            //var digitalSignatureHelper = digitalSignatureHelperFactory.Create(RecordsConstants.RECORDS_APPLICATION_NAME);
            var accessToken = new RMAccessToken
            {
                Email = loginInfo.Email,
                TenantGroupId = loginInfo.TenantGroupId,
                Type = loginInfo.Type,
                AccessToken = loginInfo.AccessToken,
                ExpiredTime = DateTime.UtcNow.AddMinutes(sessionTimeOutMinute).Ticks.ToString(CultureInfo.InvariantCulture)
            };

            //var signature = digitalSignatureHelper.SignData(accessToken.ToString());
            //accessToken.Signature = signature;
            return accessToken;
        }

        public RMAccessToken RefreshToken(RMAccessToken accessToken, double sessionTimeOutMinute)
        {
            //var portalDigitalSignatureHelper = digitalSignatureHelperFactory.Create(RecordsConstants.RECORDS_APPLICATION_NAME);
            accessToken.ExpiredTime = DateTime.UtcNow.AddMinutes(sessionTimeOutMinute).Ticks.ToString(CultureInfo.InvariantCulture);
            //accessToken.Signature = portalDigitalSignatureHelper.SignData(accessToken.ToString());
            return accessToken;
        }
    }
}
