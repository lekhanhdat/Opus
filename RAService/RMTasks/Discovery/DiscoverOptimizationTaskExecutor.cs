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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.GCommon.Contract.StorageOptimization.Connector;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.GCommon.Contract.Server.Common;
using Microsoft.SharePoint.News.DataModel;
using RAExportCommon;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.DB.AzureTable;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Service.Services.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.GCommon.Contract.Tree.Object;
using Microsoft.Graph;
using AvePoint.RA.SharePoint.Archiver.Common.DiscoverUtil;
using AvePoint.RA.Contract.CloudService;
using System.Globalization;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.RA.RACommonUtility.Lcoker;
using AvePoint.RA.Service.JobMonitor;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Core.Discovery.DBManager;

namespace AvePoint.RA.Service.RMTasks.Discovery
{
    public class DiscoverOptimizationTaskExecutor : ITaskExecutor
    {
        private RALogger mLogger = RALogger.GetInstance(typeof(DiscoverOptimizationTaskExecutor));
        private readonly IRMDiscoveryOffice365OptimizationSettingsInfoDao _optimizationSettingsInfoDao = new RMDiscoveryOffice365OptimizationSettingsInfoDao();
        private readonly IRMDiscoveryAOSPOptimizationSettingsInfoDao _aospOptimizationSettingsInfoDao = new RMDiscoveryAOSPOptimizationSettingsInfoDao();
        private readonly IRMDiscoveryOffice365TenantDao _o365TenantInfoDao = new RMDiscoveryOffice365TenantDao();
        private readonly IRMDiscoveryAOSPTenantDao _aospTenantInfoDao = new RMDiscoveryAOSPTenantDao();
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        public ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        public IRMTenantDiscoveryDBInfoDao TenantDiscoveryDBInfoDao = new RMTenantDiscoveryDBInfoDao();
        // Cache missing discovery table names we have already logged to avoid log spam every schedule interval.
        private static readonly HashSet<string> s_missingDiscoveryTablesLogged = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public async Task ExecutorAsync(TaskBase task)
        {
            try
            {
                var tInfos = TenantService.GetAllTenantInfo();
                var discoveryTInfoes = await TenantDiscoveryDBInfoDao.GetAllAvaliableAsync();
                foreach (var tInfo in tInfos)
                {
                    if (discoveryTInfoes.All(x => !x.Id.Equals(tInfo.TenantId, StringComparison.OrdinalIgnoreCase)))
                    {
                        mLogger.Warn($"Tenant {tInfo.TenantId} doesn't have available discovery DB, skip add optimization job.");
                        continue;
                    }

                    await TenantUtil.RunUnderTenantAsync(tInfo.TenantId, tInfo.RegisterEmail, async () =>
                    {
                        if (tInfo.Status == 0)
                        {
                            await AddOptimizationJobToJobqueue();
                        }
                        await AddAOSPOptimizationJobToJobqueue();
                    });
                }
            }
            catch (Exception e)
            {
                mLogger.Error($"something went wrong when add optimization subjob ,error:{e.ToString()}");
            }
        }

        private async Task AddOptimizationJobToJobqueue()
        {
            try
            {
                var pendingJobQueueMessage = JobQueueService.GetMessagesCount(TenantLocalValue.LogonGroupId, JobType.DiscoveryOptimizationCalculate);
                var runningCalculateCount = JobMonitorService.GetRunningJobsCount(JobType.DiscoveryOptimizationCalculate);
                mLogger.Info($"AddOptimizationJobToJobqueue,pendingJobQueueMessage count:{pendingJobQueueMessage},runningCalculateCount:{runningCalculateCount}");
                if (pendingJobQueueMessage + runningCalculateCount > 0)
                {
                    mLogger.Warn($"Has running discovery optimization calculate job. can't start optimization job.");
                    return;
                }
                var allO365TenantInfos = await _o365TenantInfoDao.GetAllAsync();
                foreach (var o365Info in allO365TenantInfos)
                {
                    mLogger.Info($"Process O365Tenant ID {o365Info.UniqueId} Name {o365Info.Name}.");
                    if (!await RMDiscoveryDBManager.CheckOffice365OptimizationSettingsTableExistsAsync(o365Info.UniqueId))
                    {
                        mLogger.Info($"O365Tenant ID {o365Info.UniqueId} Name {o365Info.Name} O365 optimization settings table not exists, skip add optimization job.");
                        return;    
                    } 
                    var settingInfos = await _optimizationSettingsInfoDao.GetNeedRunJobSettingAsync(DateTime.UtcNow.Ticks, o365Info.UniqueId);
                    foreach (var settingInfo in settingInfos)
                    {
                        bool needAddToJobQueue = NeedAddToJobQueue(settingInfo.Setting);
                        if (needAddToJobQueue)
                        {
                            RMDiscoverOptimizationJobInfo jobParaInfo = new RMDiscoverOptimizationJobInfo();
                            jobParaInfo.o365Info = o365Info;
                            jobParaInfo.settingInfo = settingInfo;
                            JobQueueDto jqDto = new JobQueueDto()
                            {
                                JobType = JobType.DiscoverOptimization,
                                //JobRunType = jobRunBy,
                                TenantGroupId = TenantLocalValue.LogonGroupId,
                                JobRunByUser = "RM_TS_RunSchedule",
                                JobRunType = JobRunBy.Schedule,
                                Parameters = SerializerHelper.SerializeByDataContractSerializer(jobParaInfo),
                            };
                            var id = JobQueueService.AddToDBJobQueue(jqDto);
                            await _optimizationSettingsInfoDao.UpdateIsHandleAsync(settingInfo.SettingId, true, o365Info.UniqueId);
                        }
                        else
                        {
                            mLogger.Warn("there is the same setting in job queue,so skip add the job to job queue");
                        }
                    }
                }
            }
            catch(Exception e)
            {
                mLogger.Error($"Add optimization job to job queue failed, error : {e}");
            }
        }

        private async Task AddAOSPOptimizationJobToJobqueue()
        {
            try
            {
                var runningCalculateCount = JobMonitorService.GetRunningJobsCount(JobType.DiscoveryAOSPOptimizationCalculate);
                if (runningCalculateCount > 0)
                {
                    mLogger.Warn($"Has running discovery optimization calculate job. can't start optimization job.");
                    return;
                }
                if(!await RMDiscoveryDBManager.CheckAOSPTenantInfoTableExistsAsync())
                {
                    mLogger.Info($"AOSP tenant info table not exists, skip add AOSP optimization job.");
                    return;
                }
                var allTenantInfos = await _aospTenantInfoDao.GetAllAsync();
                foreach (var tenantInfo in allTenantInfos)
                {
                    mLogger.Info($"Process O365Tenant ID {tenantInfo.UniqueId} Name {tenantInfo.Name}.");
                    if (!await RMDiscoveryDBManager.CheckAOSPOptimizationSettingsTableExistsAsync(tenantInfo.UniqueId))
                    {
                        mLogger.Info($"O365Tenant ID {tenantInfo.UniqueId} Name {tenantInfo.Name} AOSP optimization settings table not exists, skip add optimization job.");
                        return;
                    }
                    var settingInfos = await _aospOptimizationSettingsInfoDao.GetNeedRunJobSettingAsync(DateTime.UtcNow.Ticks, tenantInfo.UniqueId);
                    foreach (var settingInfo in settingInfos)
                    {
                        bool needAddToJobQueue = NeedAddAOSPJobToJobQueue(settingInfo.Setting);
                        if (needAddToJobQueue)
                        {
                            RMDiscoverAOSPOptimizationJobInfo jobParaInfo = new()
                            {
                                o365Info = tenantInfo,
                                settingInfo = settingInfo
                            };
                            JobQueueDto jqDto = new()
                            {
                                JobType = JobType.DiscoveryAOSPOptimization,
                                //JobRunType = jobRunBy,
                                TenantGroupId = TenantLocalValue.LogonGroupId,
                                JobRunByUser = "RM_TS_RunSchedule",
                                JobRunType = JobRunBy.Schedule,
                                Parameters = SerializerHelper.SerializeByDataContractSerializer(jobParaInfo),
                            };
                            var id = JobQueueService.AddToDBJobQueue(jqDto);
                            await _aospOptimizationSettingsInfoDao.UpdateIsHandleAsync(settingInfo.SettingId, true, tenantInfo.UniqueId);
                        }
                        else
                        {
                            mLogger.Warn("there is the same setting in job queue,so skip add the job to job queue");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Error($"Add AOSP optimization job to job queue failed, error : {e}");
            }
        }
        private bool NeedAddToJobQueue(string setting)
        {
            try
            {
                mLogger.Info("start check optimization job need add to job queue");
                var jobQueueDtoList = JobQueueService.GetDBJobMessage();
                foreach (var jbDto in jobQueueDtoList)
                {
                    if (jbDto.JobType == JobType.DiscoverOptimization)
                    {
                        RMDiscoverOptimizationJobInfo jobParaInfo = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoverOptimizationJobInfo>(jbDto.Parameters);
                        if (jobParaInfo.settingInfo.Setting.Equals(setting, StringComparison.OrdinalIgnoreCase))
                        {
                            mLogger.Info($"finish check optimization job need add to job queue,result is false");
                            return false;
                        }
                    }
                }
                mLogger.Info($"finish check optimization job need add to job queue,result is true");
                return true;
            }
            catch (Exception ex)
            {
                mLogger.Error($"check discover optimization job need add to job queue failed,error:{ex.ToString()}");
                return true;
            }
        }

        private bool NeedAddAOSPJobToJobQueue(string setting)
        {
            try
            {
                mLogger.Info("start check optimization job need add to job queue");
                var jobQueueDtoList = JobQueueService.GetDBJobMessage();
                foreach (var jbDto in jobQueueDtoList)
                {
                    if (jbDto.JobType == JobType.DiscoveryAOSPOptimization)
                    {
                        RMDiscoverAOSPOptimizationJobInfo jobParaInfo = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoverAOSPOptimizationJobInfo>(jbDto.Parameters);
                        if (jobParaInfo.settingInfo.Setting.Equals(setting, StringComparison.OrdinalIgnoreCase))
                        {
                            mLogger.Info($"finish check optimization job need add to job queue,result is false");
                            return false;
                        }
                    }
                }
                mLogger.Info($"finish check optimization job need add to job queue,result is true");
                return true;
            }
            catch (Exception ex)
            {
                mLogger.Error($"check discover optimization job need add to job queue failed,error:{ex.ToString()}");
                return true;
            }
        }
    }
}
