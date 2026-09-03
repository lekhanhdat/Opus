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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.TenantUpgrade;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Services.Tenant.Upgrade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.RMTasks
{
    public class ArchivedSiteInfoTenantIdUpgradeExecutor : ITaskExecutor
    {
        private RALogger Logger = RALogger.GetInstance(typeof(ArchivedSiteInfoTenantIdUpgradeExecutor));

        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private IRMArchiveSiteInfoDao ArchiveSiteInfoDao => PlatformWindsorManager.GetService<IRMArchiveSiteInfoDao>();

        public async Task ExecutorAsync(TaskBase task)
        {
            var tenants = TenantService.GetAllTenantInfo();
            foreach (var tenant in tenants)
            {
                Logger.Info($"Begin to upgrade tenant [{tenant.TenantId}], tenant status [{tenant.Status}].");
                TenantUtil.RunUnderTenant(tenant.TenantId, tenant.RegisterEmail, UpgradeArhivedSiteTenantInfo);
            }
        }

        private void UpgradeArhivedSiteTenantInfo()
        {
            if (RMTenantUpgradeHelper.IsNeedUpgrade(TenantLocalValue.LogonGroupId, RMUpgradeFeature.ArchivedSiteTenantInfo))
            {
                Logger.Info($"Current tenant [{TenantLocalValue.LogonGroupId}] need to upgrade.");
                try
                {
                    var NoO365TenantIdSites = ArchiveSiteInfoDao.GetNoO365TenatIdSitesCount();
                    if (NoO365TenantIdSites > 0)
                    {
                        var result = ArchiveSiteInfoDao.AddO365TenantIdInfo();
                        if (NoO365TenantIdSites - result > 0)
                        {
                            var otherSites = ArchiveSiteInfoDao.GetNoO365TenatIdSites();
                            if (otherSites.Count == 0)
                            {
                                RMTenantUpgradeHelper.SetToFinish(TenantLocalValue.LogonGroupId, RMUpgradeFeature.ArchivedSiteTenantInfo, RMUpgradeStatus.Success);
                                Logger.Info($"Upgrade tenant [{TenantLocalValue.LogonGroupId}] success");
                                return;
                            }

                            var allProfiles = RMAosApiClient.GetAllProfiles(TenantLocalValue.LogonGroupId).DistinctBy(profile => profile.DomainName);
                            var tenantIdDomainMap = allProfiles.ToDictionary(profile => profile.DomainName.ToLowerInvariant(), profile => profile.TenantId);
                            var updateSites = new List<DB.Model.RMArchiveSiteInfo>();
                            foreach (var profileTenantId in tenantIdDomainMap)
                            {
                                try
                                {
                                    var matchingSites = otherSites
                                       .Where(site => site.SiteUrl.StartsWith("https://" + profileTenantId.Key + ".", StringComparison.InvariantCultureIgnoreCase)
                                           || site.SiteUrl.StartsWith("https://" + profileTenantId.Key + "-", StringComparison.InvariantCultureIgnoreCase))
                                       .ToList();

                                    if (matchingSites.Count != 0)
                                    {
                                        foreach (var site in matchingSites)
                                        {
                                            site.O365TenantId = profileTenantId.Value;
                                        }

                                        updateSites.AddRange(matchingSites);
                                    }
                                }
                                catch (Exception e)
                                {
                                    RMTenantUpgradeHelper.SetToFinish(TenantLocalValue.LogonGroupId, RMUpgradeFeature.ArchivedSiteTenantInfo, RMUpgradeStatus.Failed);
                                    Logger.Error($"Upgrade tenant [{TenantLocalValue.LogonGroupId}] failed with domain name [{profileTenantId.Key}]. Error {e}");
                                }
                            }
                            try
                            {
                                if (updateSites.Count > 0)
                                {
                                    var updateResult = ArchiveSiteInfoDao.UpdateO365TenantIdInfo(updateSites);
                                    if (updateResult > 0)
                                    {
                                        RMTenantUpgradeHelper.SetToFinish(TenantLocalValue.LogonGroupId, RMUpgradeFeature.ArchivedSiteTenantInfo, RMUpgradeStatus.Success);
                                        Logger.Info($"Upgrade tenant [{TenantLocalValue.LogonGroupId}] success");
                                    }
                                    else
                                    {
                                        RMTenantUpgradeHelper.SetToFinish(TenantLocalValue.LogonGroupId, RMUpgradeFeature.ArchivedSiteTenantInfo, RMUpgradeStatus.Failed);
                                        Logger.Error($"Upgrade tenant [{TenantLocalValue.LogonGroupId}] failed with profile.");
                                    }
                                }
                                else
                                {
                                    RMTenantUpgradeHelper.SetToFinish(TenantLocalValue.LogonGroupId, RMUpgradeFeature.ArchivedSiteTenantInfo, RMUpgradeStatus.Success);
                                    Logger.Info($"Upgrade tenant [{TenantLocalValue.LogonGroupId}] success");
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Error($"Upgrade tenant [{TenantLocalValue.LogonGroupId}] failed with profile. Error {e}");
                            }
                        }
                        else
                        {
                            RMTenantUpgradeHelper.SetToFinish(TenantLocalValue.LogonGroupId, RMUpgradeFeature.ArchivedSiteTenantInfo, RMUpgradeStatus.Success);
                            Logger.Info($"Upgrade tenant [{TenantLocalValue.LogonGroupId}] Success.");
                        }
                    }
                    else
                    {
                        RMTenantUpgradeHelper.SetToFinish(TenantLocalValue.LogonGroupId, RMUpgradeFeature.ArchivedSiteTenantInfo, RMUpgradeStatus.Success);
                        Logger.Info($"Upgrade tenant [{TenantLocalValue.LogonGroupId}] Success.");
                    }
                }
                catch (Exception e)
                {
                    RMTenantUpgradeHelper.SetToFinish(TenantLocalValue.LogonGroupId, RMUpgradeFeature.ArchivedSiteTenantInfo, RMUpgradeStatus.Failed);
                    Logger.Error($"Upgrade tenant [{TenantLocalValue.LogonGroupId}] failed, error {e}");
                }
            }
        }
    }
}
