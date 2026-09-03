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
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao;
using System;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using Microsoft365.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CloudAos = Cloud.Sdk.Data.AosModern;
using AvePoint.Wrapper.Common;

namespace AvePoint.RA.RAExchange.Common
{
    public class EXOPoolUserUtil
    {
        private static IRMAppProfileDao RMAppProfileDao = null;
        private static readonly IRMRemoteNodeService RemoteNodeService = PlatformWindsorManager.GetService<IRMRemoteNodeService>();
        private static RALogger logger = RALogger.GetInstance(typeof(EXOPoolUserUtil));
        public static async Task<AveBPOSAccountInfo> GetBPOSInfoAsync(RemoteSiteCollection site)
        {
            logger.Info("No1 available app profile. Use Service Account that bound with site.");
            AveBPOSAccountInfo accountInfo = GetConnectionSAInfo(site);
            accountInfo.TokenProvider = accountInfo.Convert2TokenProvider();
            accountInfo.ExsitAppProfile = false;

            return accountInfo;
        }
        public static Wrapper.Common.AveBPOSAccountInfo GetConnectionSAInfo(RemoteSiteCollection site)
        {
            string username = string.Empty;
            string password = string.Empty;
            string domain = string.Empty;
            string siteAdminUrl = string.IsNullOrEmpty(site.AdminUrl) ? WebUtil.GetSPAdminUrl(site.url, site.TenantId) : site.AdminUrl;
            bool hasPoolUser = false;

            if (!hasPoolUser)
            {
                if (string.IsNullOrEmpty(site.username))
                {
                    username = RMAosApiClient.GetServiceAccountsByTenantIdWithPassword(TenantLocalValue.LogonGroupId, site.TenantId).FirstOrDefault()?.UserName;
                }
                else
                {
                    username = site.username;
                }
                domain = ".".Equals(site.domain) ? string.Empty : site.domain;
                password = RMAosApiClient.GetServiceAccountPassword(TenantLocalValue.LogonGroupId, username);
            }

            return new Wrapper.Common.AveBPOSAccountInfo()
            {
                Domain = domain,
                UserName = username,
                Password = password?.ToSecureString(),
                AdminUrl = siteAdminUrl,
                ConnectionType = Wrapper.Common.BposConnectionType.ServiceAccount,
                TenantGroupId = TenantLocalValue.LogonGroupId,
                AADEnvironment = (AveAzureEnvironment)site.AADEnvironment,
                TenantId = site.TenantId,
            };
        }
        private static RMAppProfileInfo Convert2RMAppProfileInfo(RMAosAuthenticationProfile aosAuthenticationProfile)
        {
            return new RMAppProfileInfo()
            {
                AppClientId = new Guid(aosAuthenticationProfile.AppClientId),
                TenantId = new Guid(aosAuthenticationProfile.TenantId),
                UsedTimes = 0,
                AppType = aosAuthenticationProfile.AppType,
            };
        }
    }
}
