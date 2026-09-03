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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.I18N.Core;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Common.Utility;
using Cloud.Sdk.Data.Aos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAos = Cloud.Sdk.Data.AosModern;


namespace AvePoint.RA.Common.Util
{
    public static class MultiAppUtil
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(MultiAppUtil));
        public static List<BposInfo> GetBposInfoCollection(AveBPOSAccountInfo bposInfo, string siteUrl)
        {
            try
            {
                logger.Info("Start to get app profiles from AOS");
                var appProfiles = RMAosApiClient.GetHasADPermissionProfiles(TenantLocalValue.LogonGroupId).Where(item => item.TenantId == bposInfo.TenantId)
                    .ConvertAll(p => RMAosApiClient.ConvertToAppProfile(p))
                    .ToList();

                List<BposInfo> infos = appProfiles.ConvertAll(p => GetBopsInfoFromAppProfile(siteUrl, bposInfo, p));
                return infos;
            }
            catch
            {
                logger.Error(I18NEntity.GetString("RM_APP_AppProfileNotAvailable"));
                return new List<BposInfo>();
            }
        }

        private static BposInfo GetBopsInfoFromAppProfile(string siteUrl, AveBPOSAccountInfo bposInfo, AppProfile profile)
        {
            BposInfo newInfo = new BposInfo();
            newInfo.SiteUrl = siteUrl;
            newInfo.AppType = bposInfo.AppType;
            newInfo.TenantGroupId = bposInfo.TenantGroupId;
            newInfo.ConnectionType = GCommon.Contract.CentralAdmin.Object.BposConnectionType.AppToken;
            newInfo.UserAccountInfo = new BposUserAccountInfo()
            {
                AppId = profile.Id,
                AppClientId = profile.AppClientId,
                AppCertSecret = profile.AppCertSecret,
                //AppCertContent = profile.AppCertContent,
                AADEnvironment = profile.AADEnvironment,
                AppCertSecretContent = profile.AppCertSecretContent,
                AdminUrl = profile.AdminUrl,
                TenantId = bposInfo.TenantId
            };
            return newInfo;
        }

        public static AveObjectModelFactory CreateAveObjectModelFactory(string siteUrl, AveBPOSAccountInfo accountInfo, AveContextKind contextKind, bool useSpecialApp = false)
        {
            var factory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, accountInfo, contextKind);
            if (!useSpecialApp)
            {
                List<BposInfo> appprofiles = GetBposInfoCollection(accountInfo, siteUrl);
                if (!appprofiles.Any())
                {
                    return factory;
                }
                AveAppProfileUtility.Init(new Guid(accountInfo.TenantId), appprofiles);
            }
            return factory;
        }
    }
}
