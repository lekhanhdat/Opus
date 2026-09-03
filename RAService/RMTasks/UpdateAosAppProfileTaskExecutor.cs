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
using AvePoint.Hybrid.ClientLibrary.SDK.Services;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.RMTasks
{
    public class UpdateAosAppProfileTaskExecutor : ITaskExecutor
    {
        private IRALogger mLogger = RALogger.GetInstance(typeof(UpdateAosAppProfileTaskExecutor));
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        public IRMAppProfileDao RMAppProfileDao => PlatformWindsorManager.GetService<IRMAppProfileDao>();
        public async System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {
                //RMAppProfileDao = (IRMAppProfileDao)PlatformWindsorManager.GetService(typeof(IRMAppProfileDao));

                var tInfos = TenantService.GetAllAvailableTenantInfo();
                foreach (var tInfo in tInfos)
                {
                    await TenantUtil.RunUnderTenantAsync(tInfo.TenantId, tInfo.RegisterEmail, ExcuteTaskAsync);
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("error occurred while update aos app profile,ERROR:{0}", ex.ToString());
            }
        }

        private async System.Threading.Tasks.Task ExcuteTaskAsync()
        {
            try
            {
                var tenantIds = RMAosApiClient.GetO365TenantIds(TenantLocalValue.LogonGroupId);
                mLogger.Info($"Start to update app profile for tenant:{TenantLocalValue.LogonGroupId} Office 365 Tenant Ids:{string.Join(",", tenantIds)}");
                var appProfiles = RMAosApiClient.GetSPOAuthenticationProfiles(TenantLocalValue.LogonGroupId, tenantIds);
                mLogger.Info($"App profile count:{appProfiles?.Count}");
                if (appProfiles != null && appProfiles.Count > 0)
                {
                    var tenantAppProfiles = appProfiles.GroupBy(a => a.TenantId).ToDictionary(a => a.Key, a => a.ToList());
                    foreach (var kv in tenantAppProfiles)
                    {
                        await RMAppProfileDao.UpdateAppProfilesForTenantAsync(new Guid(kv.Key), kv.Value.ConvertAll(a => Convert2RMAppProfileInfo(a)));
                    }
                }
                var existAppTenantIds = appProfiles != null && appProfiles.Count > 0 ?
                    appProfiles.Select(a => a.TenantId).Distinct().ToList() : new List<string>();
                var nonAppTenantIds = tenantIds.Where(t => !existAppTenantIds.Contains(t)).ToList();
                if (nonAppTenantIds != null && nonAppTenantIds.Count > 0)
                {
                    RMAppProfileDao.RemoveAppProfilesForTenant(nonAppTenantIds.Select(s => new Guid(s)).ToList());
                }

                mLogger.Info($"Update aos app profile finished.");
            }
            catch (Exception e)
            {
                mLogger.Error($"Error occurred while updating aos app profile for tenant:{TenantLocalValue.LogonGroupId} Error:{e.ToString()}");
            }
        }

        private RMAppProfileInfo Convert2RMAppProfileInfo(RMAosAuthenticationProfile aosAuthenticationProfile)
        {
            return new RMAppProfileInfo()
            {
                AppClientId = new Guid(aosAuthenticationProfile.AppClientId),
                TenantId = new Guid(aosAuthenticationProfile.TenantId),
                UsedTimes = 0,
                AppType = aosAuthenticationProfile.AppType
            };
        }
    }
}
