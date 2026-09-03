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
using Amazon.Runtime.Internal.Util;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.GraphApi.Tenant;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.O365Tenant;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.Office.CustomUI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.JobControl.O365Tenant
{
    public class RMO365TenantSubJobController
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMO365TenantSubJobController));

        private const string S_TENANT_USER_SEAT_CACHE_KEY = "TENANT_USER_SEAT";

        private const string S_TENANT_SUB_JOB_CONTROL_CACHE_KEY = "TENANT_SUB_JOB_CONTROL";

        private const string S_CUSTOM_USER_SEATS = "CUSTOM_USER_SEATS";

        private const string S_CUSTOM_O365_TENANT_SUB_JOB_CONTROL = "CUSTOM_O365_TENANT_SUB_JOB_CONTROL";

        private const string S_LAST_UPDATED_MAIN_JOB_ID = "LAST_UPDATED_MAIN_JOB_ID";

        private readonly IRMSubJobDao _subJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();

        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();


        private readonly IRMCache _cache = PlatformWindsorManager.GetService<IRMCache>();

        private readonly IJobInfoUpdater _jobInfoUpdate = (IJobInfoUpdater)PlatformWindsorManager.GetService(typeof(IJobInfoUpdater));

        public async Task RunAsync()
        {
            try
            {
                var hasWaitingJobs = await _subJobDao.HasWaitingSubJobCountAsync(RMO365TenantSubJobControlConstants.CONTROLLED_JOBS.ToArray());
                if (!hasWaitingJobs)
                {
                    return;
                }
                s_logger.Info($"Current tenant [{TenantLocalValue.LogonGroupId}] has waiting sub jobs.");
                var tenantSubscribedInfoes = await GetTenantSubscribedInfoToCache();

                var tenantSubJobControlDefinitions = await GetTenantSubJobControlDefinitions(tenantSubscribedInfoes);

                foreach (var tenantSubscribedInfo in tenantSubscribedInfoes)
                {
                    if (!tenantSubJobControlDefinitions.ContainsKey(tenantSubscribedInfo.Id))
                    {
                        continue;
                    }
                    var maxRunSubJobCount = CalculateSubJobCount(tenantSubscribedInfo.UserSeats, tenantSubJobControlDefinitions[tenantSubscribedInfo.Id]);
                    // var runningCount = _subJobDao.GetRunningAndRunnableSubJobCountAsync(tenantSubscribedInfo.Id, RMO365TenantSubJobControlConstants.CONTROLLED_JOBS.ToArray()).GetAwaiter().GetResult();
                    var runningMainJobsAndSubJobCountDict = await _subJobDao.GetRunningAndRunnableMainJobIdAndSubJobCountAsync(tenantSubscribedInfo.Id, RMO365TenantSubJobControlConstants.CONTROLLED_JOBS.ToArray());
                    var runningCount = runningMainJobsAndSubJobCountDict.Values.Sum();
                    var canRunSubJobCount = maxRunSubJobCount - runningCount;
                    s_logger.Info($"Tenant [{tenantSubscribedInfo.Id}] has user seats [{tenantSubscribedInfo.UserSeats}], max can run sub job count: [{maxRunSubJobCount}], running sub job count: [{runningCount}], can run sub job count: [{canRunSubJobCount}].");
#if DEBUG
                    s_logger.Debug($"Tenant [{tenantSubscribedInfo.Id}] running main jobs and subjobs count: [{string.Join(" ; ", runningMainJobsAndSubJobCountDict.Select(x => $"main job id: [{x.Key}], subjob count: [{x.Value}]"))}]");
#endif
                    if (canRunSubJobCount > 0)
                    {
                        var result = await _subJobDao.UpdateWaitingSubJobToRunnableAsync(tenantSubscribedInfo.Id, maxRunSubJobCount, canRunSubJobCount, runningMainJobsAndSubJobCountDict, RMO365TenantSubJobControlConstants.CONTROLLED_JOBS.ToArray());
                        s_logger.Info($"Update [{tenantSubscribedInfo.Id}] waiting sub jobs [{result}] to runnable.");
                    }
                }
                try
                {
                    List<string> existOffice365TenantIds = tenantSubscribedInfoes.Select(x => x.Id).ToList();
                    List<RMSubJob> dirtySubjobs = _subJobDao.GetDirtyWaitingArchiverSubJob(RMO365TenantSubJobControlConstants.CONTROLLED_JOBS.ToList(), existOffice365TenantIds);
                    foreach (RMSubJob dirtySubjob in dirtySubjobs)
                    {
                        s_logger.Info($"Current subjob:[{dirtySubjob.Id}],Url:[{dirtySubjob.String1}],TenantId:[{dirtySubjob.O365TenantId}] is dirty subjob and need update to failed status.");
                        _jobInfoUpdate.UpdateJobState(dirtySubjob.Id, (int)JobStatus.Failed, "RM_JM_Archive_TenantRemoveFromAOS_ErrorMessage");
                    }
                }
                catch (Exception ex)
                {
                    s_logger.Error($"An error occurred while check dirty Subjobs. Error: {ex}");
                }
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while run sub jobs. Error: {e}");
            }
        }

        public int CalculateSubJobCount(int userSeats, RMO365TenantSubJobControlDefinition jobControlSetting)
        {
            var res = 0;

            var slaCollection = new List<RMO365TenantJobControlSLA>(jobControlSetting.SLACollection);
            slaCollection.Insert(0, new RMO365TenantJobControlSLA(0, 0));

            for (int i = 1; i < slaCollection.Count && userSeats > 0; i++)
            {
                var previousSla = slaCollection[i - 1];
                var currentSla = slaCollection[i];
                var needCalculateUserSeat = currentSla.UserSeats - previousSla.UserSeats;
                if (needCalculateUserSeat > userSeats)
                {
                    needCalculateUserSeat = userSeats;
                }

                var needJobCount = (1.0 * needCalculateUserSeat * jobControlSetting.AverageUserDataSize * 1000 / jobControlSetting.AverageFileSize)
                    / (jobControlSetting.ScanSpeed * 24) / currentSla.Days * 1.0;
                res += (int)Math.Ceiling(needJobCount);

                userSeats -= needCalculateUserSeat;
            }

            res = (int)Math.Ceiling(res * jobControlSetting.Rate);
            res = Math.Max(res, jobControlSetting.MinLimit);

            return Math.Min(res, jobControlSetting.MaxLimit);
        }

        public async Task<RMO365TenantSubscribed> GetTenantSubscribedInfoBy365TenantId(string o365TenantId)
        {
            try
            {
                var tenantManager = new RMGraphTenantManager(o365TenantId);
                var skus = await tenantManager.GetSharePointSubscribedSkusAsync();
                var userSeats = skus.Sum(item => item.PrepaidUnits.Enabled);
                s_logger.Info($"Tenant [{o365TenantId}] user seats [{userSeats}].");
                return new RMO365TenantSubscribed
                {
                    Id = o365TenantId,
                    UserSeats = userSeats,
                };
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while calculate tenant [{o365TenantId}] subscribed skus. Error: {e}");
            }

            return new RMO365TenantSubscribed
            {
                Id = o365TenantId,
                UserSeats = 0,
            };
        }

        public async Task<List<RMO365TenantSubscribed>> GetTenantSubscribedInfoToCache()
        {
            try
            {
                var hasCache = await _cache.ExistAsync(S_TENANT_USER_SEAT_CACHE_KEY);
                if (!hasCache)
                {
                    var o365TenantIds = RMAosApiClient.GetO365TenantIds(TenantLocalValue.LogonGroupId);

                    var o365TenantSubscribed = await o365TenantIds.ConvertAllAsync(async o365TenantId =>
                    {
                        try
                        {
                            var tenantManager = new RMGraphTenantManager(o365TenantId);
                            var skus = await tenantManager.GetSharePointSubscribedSkusAsync();
                            var userSeats = skus.Sum(item => item.PrepaidUnits.Enabled);
                            s_logger.Info($"Tenant [{o365TenantId}] user seats [{userSeats}].");
                            return new RMO365TenantSubscribed
                            {
                                Id = o365TenantId,
                                UserSeats = userSeats,
                            };
                        }
                        catch (Exception e)
                        {
                            s_logger.Error($"An error occurred while calculate tenant [{o365TenantId}] subscribed skus. Error: {e}");
                        }

                        return new RMO365TenantSubscribed
                        {
                            Id = o365TenantId,
                            UserSeats = 0,
                        };
                    });

                    var setting = _keyValueDao.GetValueByKey(S_CUSTOM_USER_SEATS);
                    if (setting != null && !string.IsNullOrWhiteSpace(setting.Value))
                    {
                        if(int.TryParse(setting.Value, out var customUserSeats))
                        {
                            s_logger.Info($"Used custom user seats [{customUserSeats}].");
                            o365TenantSubscribed.ForEach(item => item.UserSeats = customUserSeats);
                        }
                    }

                    await _cache.SetListAsync(S_TENANT_USER_SEAT_CACHE_KEY, o365TenantSubscribed);
                    await _cache.KeyExpiredAsync(S_TENANT_USER_SEAT_CACHE_KEY, 60 * 60 * 24);
                    s_logger.Info("Successful add o365 tenant subscribed to redis.");
                }

                return await _cache.GetListAsync<RMO365TenantSubscribed>(S_TENANT_USER_SEAT_CACHE_KEY);
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while add tenant info to cache. Error: {e}");
                List<RMO365TenantSubscribed> o365TenantSubscribed = null;
                try
                {
                    var o365TenantIds = RMAosApiClient.GetO365TenantIds(TenantLocalValue.LogonGroupId);

                    o365TenantSubscribed = await o365TenantIds.ConvertAllAsync(async o365TenantId =>
                    {
                        try
                        {
                            var tenantManager = new RMGraphTenantManager(o365TenantId);
                            var skus = await tenantManager.GetSharePointSubscribedSkusAsync();
                            var userSeats = skus.Sum(item => item.PrepaidUnits.Enabled);
                            s_logger.Info($"Tenant [{o365TenantId}] user seats [{userSeats}].");
                            return new RMO365TenantSubscribed
                            {
                                Id = o365TenantId,
                                UserSeats = userSeats,
                            };
                        }
                        catch (Exception e)
                        {
                            s_logger.Error($"An error occurred while calculate tenant [{o365TenantId}] subscribed skus. Error: {e}");
                        }

                        return new RMO365TenantSubscribed
                        {
                            Id = o365TenantId,
                            UserSeats = 0,
                        };
                    });
                }
                catch (Exception ex)
                {
                    s_logger.Info($"something error when get RMO365TenantSubscribed from db,error:{ex}");
                }
                if (o365TenantSubscribed != null && o365TenantSubscribed.Count > 0)
                {
                    s_logger.Info($"Can find RMO365TenantSubscribed,return o365TenantSubscribed.Count:{o365TenantSubscribed.Count}.");
                    return o365TenantSubscribed;
                }
                else
                {
                    s_logger.Info("can not find RMO365TenantSubscribed,return new");
                    return new List<RMO365TenantSubscribed>();
                }
            }

        }

        public async Task<List<RMO365TenantSubscribed>> GetAOSPTenantSubscribedInfoToCache(string appProfileId)
        {
            try
            {
                var hasCache = await _cache.ExistAsync(S_TENANT_USER_SEAT_CACHE_KEY);
                if (!hasCache)
                {
                    var o365TenantIds = RMAosApiClient.GetAOSPO365TenantIds(TenantLocalValue.LogonGroupId);

                    var o365TenantSubscribed = await o365TenantIds.ConvertAllAsync(async o365TenantId =>
                    {
                        try
                        {
                            var aospAppProfile = await RMAosApiClient.GetAOSPAuthProfileByAppId(TenantLocalValue.LogonGroupId, appProfileId);
                            var tenantManager = new RMAOSPGraphTenantManager(aospAppProfile);
                            var skus = await tenantManager.GetSharePointSubscribedSkusAsync();
                            var userSeats = skus.Sum(item => item.PrepaidUnits.Enabled);
                            s_logger.Info($"Tenant [{o365TenantId}] user seats [{userSeats}].");
                            return new RMO365TenantSubscribed
                            {
                                Id = o365TenantId,
                                UserSeats = userSeats,
                            };
                        }
                        catch (Exception e)
                        {
                            s_logger.Error($"An error occurred while calculate tenant [{o365TenantId}] subscribed skus. Error: {e}");
                        }

                        return new RMO365TenantSubscribed
                        {
                            Id = o365TenantId,
                            UserSeats = 0,
                        };
                    });

                    var setting = _keyValueDao.GetValueByKey(S_CUSTOM_USER_SEATS);
                    if (setting != null && !string.IsNullOrWhiteSpace(setting.Value))
                    {
                        if (int.TryParse(setting.Value, out var customUserSeats))
                        {
                            s_logger.Info($"Used custom user seats [{customUserSeats}].");
                            o365TenantSubscribed.ForEach(item => item.UserSeats = customUserSeats);
                        }
                    }

                    await _cache.SetListAsync(S_TENANT_USER_SEAT_CACHE_KEY, o365TenantSubscribed);
                    await _cache.KeyExpiredAsync(S_TENANT_USER_SEAT_CACHE_KEY, 60 * 60 * 24);
                    s_logger.Info("Successful add o365 tenant subscribed to redis.");
                }

                return await _cache.GetListAsync<RMO365TenantSubscribed>(S_TENANT_USER_SEAT_CACHE_KEY);
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while add tenant info to cache. Error: {e}");
                try
                {
                    var o365TenantIds = RMAosApiClient.GetAOSPO365TenantIds(TenantLocalValue.LogonGroupId);

                    var o365TenantSubscribed = await o365TenantIds.ConvertAllAsync(async o365TenantId =>
                    {
                        try
                        {
                            var aospAppProfile = await RMAosApiClient.GetAOSPAuthProfileByAppId(TenantLocalValue.LogonGroupId, appProfileId);
                            var tenantManager = new RMAOSPGraphTenantManager(aospAppProfile);
                            var skus = await tenantManager.GetSharePointSubscribedSkusAsync();
                            var userSeats = skus.Sum(item => item.PrepaidUnits.Enabled);
                            s_logger.Info($"Tenant [{o365TenantId}] user seats [{userSeats}].");
                            return new RMO365TenantSubscribed
                            {
                                Id = o365TenantId,
                                UserSeats = userSeats,
                            };
                        }
                        catch (Exception e)
                        {
                            s_logger.Error($"An error occurred while calculate tenant [{o365TenantId}] subscribed skus. Error: {e}");
                        }

                        return new RMO365TenantSubscribed
                        {
                            Id = o365TenantId,
                            UserSeats = 0,
                        };
                    });
                }
                catch (Exception ex)
                {
                    s_logger.Info($"something error when get RMO365TenantSubscribed from db,error:{ex}");
                }
                s_logger.Info("can not find RMO365TenantSubscribed,return new");
                return new List<RMO365TenantSubscribed>();
            }

        }
        private async Task<RMO365TenantSubscribed> GetTenantSubscribedAsync()
        {
            var o365TenantIds = RMAosApiClient.GetO365TenantIds(TenantLocalValue.LogonGroupId);

            var o365TenantSubscribed = await o365TenantIds.ConvertAllAsync(async o365TenantId =>
            {
                try
                {
                    var tenantManager = new RMGraphTenantManager(o365TenantId);
                    var skus = await tenantManager.GetSharePointSubscribedSkusAsync();
                    var userSeats = skus.Sum(item => item.PrepaidUnits.Enabled);
                    s_logger.Info($"Tenant [{o365TenantId}] user seats [{userSeats}].");
                    return new RMO365TenantSubscribed
                    {
                        Id = o365TenantId,
                        UserSeats = userSeats,
                    };
                }
                catch (Exception e)
                {
                    s_logger.Error($"An error occurred while calculate tenant [{o365TenantId}] subscribed skus. Error: {e}");
                }

                return new RMO365TenantSubscribed
                {
                    Id = o365TenantId,
                    UserSeats = 0,
                };
            });

            return new RMO365TenantSubscribed();
        }
        public async Task<Dictionary<string, RMO365TenantSubJobControlDefinition>> GetTenantSubJobControlDefinitions(List<RMO365TenantSubscribed> tenantSubscribedInfoes)
        {
            var hasCache = await _cache.ExistAsync(S_TENANT_SUB_JOB_CONTROL_CACHE_KEY);
            if (!hasCache)
            {
                var definitions = tenantSubscribedInfoes.ConvertAll(item => new RMO365TenantSubJobControlDefinition
                {
                    TenantId = item.Id,
                    SLACollection = new()
                    {
                        new RMO365TenantJobControlSLA(500, 2),
                        new RMO365TenantJobControlSLA(1000, 2),
                        new RMO365TenantJobControlSLA(5000, 5),
                        new RMO365TenantJobControlSLA(10000, 7),
                        new RMO365TenantJobControlSLA(int.MaxValue, 15),
                    }
                });
                var setting = _keyValueDao.GetValueByKey(S_CUSTOM_O365_TENANT_SUB_JOB_CONTROL);
                if (setting != null && !string.IsNullOrWhiteSpace(setting.Value))
                {
                    try
                    {
                        definitions = JsonConvert.DeserializeObject<List<RMO365TenantSubJobControlDefinition>>(setting.Value);
                    }
                    catch (Exception e)
                    {
                        s_logger.Error($"An error occurred while deserialize setting info. Error: {e}");
                    }

                    tenantSubscribedInfoes.ForEach(item =>
                    {
                        if (!definitions.Exists(definition => definition.TenantId.Equals(item.Id, StringComparison.OrdinalIgnoreCase)))
                        {
                            definitions.Add(new RMO365TenantSubJobControlDefinition
                            {
                                TenantId = item.Id,
                                SLACollection = new()
                                {
                                    new RMO365TenantJobControlSLA(500, 2),
                                    new RMO365TenantJobControlSLA(1000, 2),
                                    new RMO365TenantJobControlSLA(5000, 5),
                                    new RMO365TenantJobControlSLA(10000, 7),
                                    new RMO365TenantJobControlSLA(int.MaxValue, 15),
                                }
                            });
                        }
                    });
                }

                s_logger.Info($"The diff o365 tenant job control definition is : {string.Join(" ; ", definitions.Select(d => d.TenantId))}");
                await _cache.SetListAsync(S_TENANT_SUB_JOB_CONTROL_CACHE_KEY, definitions);
                await _cache.KeyExpiredAsync(S_TENANT_SUB_JOB_CONTROL_CACHE_KEY, 60 * 60 * 2);
                s_logger.Info("Successful add o365 tenant job control definition to redis.");
            }

            var jobControlDefinitions = await _cache.GetListAsync<RMO365TenantSubJobControlDefinition>(S_TENANT_SUB_JOB_CONTROL_CACHE_KEY);
            var result = jobControlDefinitions.Where(item => !string.IsNullOrEmpty(item.TenantId)).DistinctBy(item => item.TenantId).ToDictionary(item => item.TenantId, item => item);
            if (result.Count == 0)
            {
                s_logger.Info("No valid o365 tenant job control definition found in redis.");
                await _cache.RemoveAsync(S_TENANT_SUB_JOB_CONTROL_CACHE_KEY);
            }
            return result;
        }
    }
}
