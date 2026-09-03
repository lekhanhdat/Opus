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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using Microsoft.Identity.Client;
//using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ExchangeUtility.AppTokenAuthObject;

namespace ExchangeUtility
{
    public class SAApptokenManager : AppTokenManager
    {
        private static IRALogger logger = RALogger.GetInstance(typeof(AppTokenManager));

        public override EXOTokenItem AcquireToken(AppTokenAuthObject authObj)
        {
            EXOTokenItem result = null;
            try
            {
                result = GetTokenFromAOS(authObj);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get token from aos with service account. Error:{e.ToString()}");
                throw;
            }
            //no longer get token locally           
            //if (result == null || string.IsNullOrWhiteSpace(result.AccessToken))
            //{
            //    result = GetTokenFromLocal(authObj);
            //}
            return result;
        }

        private EXOTokenItem GetTokenFromAOS(AppTokenAuthObject authObj)
        {
            logger.Info("Start to get token with MSAL from AOS for service account.");
            var saAppTokenAuthObj = authObj as ServiceAccout2AppTokenAuthObject;
            TokenParam tokenParam = new TokenParam()
            {
                CustomerId = authObj.tenantGroupId,
                SpTokenType = SharePointTokenType.IDCRL,
                TenantId = authObj.tenantId,
                AppType = authObj.appType,
                TokenMethod = TokenMethod.MSAL,
                Identity = saAppTokenAuthObj.UserName,
                Resource = saAppTokenAuthObj.ResourceUrl,
                ClientId = saAppTokenAuthObj.clientId
                // SiteUrl = authObj.siteUrl
            };
            AvePoint.GCommon.Utility.AosTokenResult aosToken = AvePoint.Common.Portal.PortalUtil.GetTokenByAOSNewSDKForEXO(tokenParam);
            return new EXOTokenItem(aosToken.AccessToken, "Bearer", aosToken.ExpiresOn);
        }
    }
}
