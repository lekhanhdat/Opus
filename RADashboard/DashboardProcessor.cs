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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Dao;
using RADashboard.Collectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Dao.Impl;
using RATeams;
using System.Threading;

namespace RADashboard
{
    public class DashboardProcessor
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(DashboardProcessor));

        private static readonly List<DashboardCollector> DashboardCollectors = new List<DashboardCollector>();

        private static readonly ITenantService TenantService = PlatformWindsorManager.GetService<ITenantService>();

        private static readonly IDashboardDataUsageOfDateDao DashboardDataUsageOfDateDao = PlatformWindsorManager.GetService<IDashboardDataUsageOfDateDao>();

        private static readonly IRMKeyValueDao RMKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private static readonly IExplorerDao ExplorerDao = new ExplorerDao(true);

        private const string DASHBOARD_SYNC_CHANGE_INFO = "DASHBOARD_SYNC_CHANGE_INFO";

        private static bool NeedCollectTermWithRule = false;

        private static bool NeedCollectSources = false;

        private static long LastCosmosDBItemCount = 0;

        private static long LastCosmosDBTimeStamp = 0;

        static DashboardProcessor()
        {
            try
            {
                CheckIfNeedProcessSourceAndTerm();
                if (NeedCollectSources)
                {
                    var collectorType = typeof(DashboardCollector);
                    var assembly = Assembly.GetAssembly(collectorType);
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsAbstract || type.IsInterface) continue;
                        if (type.BaseType?.Name == collectorType.Name)
                        {
                            var instance = Activator.CreateInstance(type) as DashboardCollector;
                            if (CheckLicense(instance.Flag))
                            {
                                if (instance.Flag == SourceFlag.Teams && !TeamsPermissionHelper.HasUpgradeTeamsFeature())
                                {
                                    Logger.Info($"Has not enable {instance.Flag} feature so skip.");
                                    continue;
                                };
                                DashboardCollectors.Add(instance);
                            }
                        }
                    }

                    Logger.Info($"Successful initialize dashboard collectors.");
                }
                else
                {
                    Logger.Info($"No need to initialize dashboard collectors. NeedCollectSources : [{NeedCollectSources}]");
                }
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while initialize dashboard collectors. Error: {e}");
            }
        }

        private static bool CheckLicense(SourceFlag sourceFlag)
        {
            if(sourceFlag == SourceFlag.FileSystem)
            {
                return TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.FileSystem);
            }
            else if(sourceFlag == SourceFlag.SharePointOnPrem)
            {
                return TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.SharePointOnPrem);
            }
            else if (sourceFlag == SourceFlag.AzureFileShare)
            {
                return TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.AzureFiles);
            }
            else if (sourceFlag == SourceFlag.Box)
            {
                return TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, PaidForModule.Box);
            }
            else if (sourceFlag == SourceFlag.Google)
            {
                return TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, PaidForModule.Google);
            }
            else if (sourceFlag == SourceFlag.GGControl)
            {
                return TenantService.HasInitGControlPlatForm().GetAwaiter().GetResult();
            }

            if (sourceFlag == SourceFlag.SharePoint || sourceFlag == SourceFlag.OneDrive || sourceFlag == SourceFlag.Exchange || sourceFlag == SourceFlag.Teams)
            {
                return TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, PaidForProduct.OpusIL);
            }
            return true;
        }

        private static void CheckIfNeedProcessSourceAndTerm()
        {
            var dashboradSyncInfo = RMKeyValueDao.GetValueByKey(DASHBOARD_SYNC_CHANGE_INFO);

            if(dashboradSyncInfo == null){
                NeedCollectTermWithRule = true;
                NeedCollectSources = true;
                LastCosmosDBItemCount = ExplorerDao.QueryCount("SELECT VALUE COUNT(1) FROM c");
                var topCosmosDBItem = ExplorerDao.GetFirstOrDefaultByOrderDesc(record => record.TimeStamp != 0, record => record.TimeStamp);
                LastCosmosDBTimeStamp = topCosmosDBItem.TimeStamp;
                return;
            }

            var dashboardSyncChangeLogger = SerializerHelper.DeserializeByDataContractSerializer<DashboardSyncChangeLogger>(dashboradSyncInfo.Value);
            LastCosmosDBTimeStamp = dashboardSyncChangeLogger.LastCosmosDBTimeStamp;
            NeedCollectTermWithRule = dashboardSyncChangeLogger.HasRuleAppliedTermChange;
            LastCosmosDBItemCount = dashboardSyncChangeLogger.LastCosmosDBItemCount;

            var currentTopCosmosDBItem = ExplorerDao.GetFirstOrDefaultByOrderDesc(record => record.TimeStamp != 0, record => record.TimeStamp);
            if(currentTopCosmosDBItem == null)
            {
                NeedCollectSources = false;
                return;
            }

            if(LastCosmosDBTimeStamp != currentTopCosmosDBItem.TimeStamp)
            {
                NeedCollectSources = true;
                if(LastCosmosDBTimeStamp == 0)
                {
                    LastCosmosDBItemCount = ExplorerDao.QueryCount("SELECT VALUE COUNT(1) FROM c");
                }
                LastCosmosDBTimeStamp = currentTopCosmosDBItem.TimeStamp;
                return;
            }

            var currentTotalCount = ExplorerDao.QueryCount("SELECT VALUE COUNT(1) FROM c");

            if(LastCosmosDBItemCount != currentTotalCount)
            {
                NeedCollectSources = true;
                LastCosmosDBItemCount = currentTotalCount;
            }
        }

        public static async Task RunAsync(string jobId, JobRunBy jobRunBy)
        {
            try
            {
                var flags = DashboardCollectors.Select(item => item.Flag);
                DashboardCollectorJobManager.Init(jobId, flags, NeedCollectTermWithRule);
                Logger.Info($"Successful init collector job manager.");

                if (TenantService.IsNewOpusTenant() && jobRunBy == JobRunBy.ChangeTab)
                {
                    await SODashboardCollector.Collect();
                    DashboardCollectorJobManager.SetOnlySOJobFinish();
                    return;
                }

                
                Logger.Info($"The [{string.Join(",", flags)}] need run dashboard collect job.");


                DashboardCollectorCache.Init();
                Logger.Info($"Successful init collctor cache.");

                IndependentDashboardCollector.Collect(NeedCollectTermWithRule);

                await DashboardCollectors.ForEachAsync(collector => collector.CollectAsync());

                await DashboardDataUsageOfDateDao.RemoveAllAsync(SourceFlag.None);
                DashboardDataUsageOfDateDao.Create(new AvePoint.RA.DB.Model.RMDashboardDataUsageOfDate
                {
                    Id = Guid.NewGuid().ToString(),
                    SourceFlag = (int)SourceFlag.None,
                    Created = 0,
                    Destroyed = 0,
                    WaitingApproved = 0,
                    Date = DateTime.UtcNow.Ticks
                });

                var dashboardSyncChangeLogger = new DashboardSyncChangeLogger
                {
                    LastCosmosDBTimeStamp = LastCosmosDBTimeStamp,
                    LastCosmosDBItemCount = LastCosmosDBItemCount,
                };

                await RMKeyValueDao.SaveOrUpdateAsync(
                        new()
                        {
                            Key = DASHBOARD_SYNC_CHANGE_INFO,
                            Value = SerializerHelper.SerializeByDataContractSerializer(dashboardSyncChangeLogger),
                        }
                    );

                DashboardCollectorJobManager.SetJobFinish();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while running dashboard job. Error: {e}");
                DashboardCollectorJobManager.SetJobFailed(e.Message);
            }
        }
    }
}
