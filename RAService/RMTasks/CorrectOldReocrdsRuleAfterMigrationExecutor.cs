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

using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RuleManagement;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.TenantUpgrade;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.Tenant.Upgrade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Common.Util;
using AvePoint.RA.RADataBroker;
using AvePoint.Media.Service;
using Microsoft365.SharePoint.Rest;
using Cloud.Sdk.Aos;
using AvePoint.RA.Contract.RMWeb.Account;

namespace AvePoint.RA.Service.RMTasks
{

    internal class CorrectOldReocrdsRuleAfterMigrationExecutor : ITaskExecutor
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(CorrectOldReocrdsRuleAfterMigrationExecutor));
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IRecordsRuleManagement RecordsRuleManagement => PlatformWindsorManager.GetService<IRecordsRuleManagement>();
        private IRMStorageDeviceInfoDao RMStorageDeviceInfoDao => PlatformWindsorManager.GetService<IRMStorageDeviceInfoDao>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IRMRuleDao RMRuleDao => PlatformWindsorManager.GetService<IRMRuleDao>();
        private IRMMiscProfileDao RMMiscProfileDao => PlatformWindsorManager.GetService<IRMMiscProfileDao>();

        public System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {
                Logger.Info("Start to Correct Old ReocrdsRule.");
                DoCorrectOldReocrdsRule();
                Logger.Info("Finish to Correct Old ReocrdsRule.");
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while Correct Old ReocrdsRule. ERROR:{0}", e.ToString());
            }
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public void DoCorrectOldReocrdsRule()
        {
            var tenants = TenantService.GetAllAvailableTenantInfo().ToDictionary(item => item.TenantId, item => item.RegisterEmail);
            foreach (var tenant in tenants)
            {
                TenantUtil.RunUnderTenant(tenant.Key, tenant.Value, () =>
                {
                    try
                    {
                        IEnumerable<RMRule> recordsRules = null;
                        if (NeedUpgrade(tenant.Key, out recordsRules))
                        {
                            Logger.Info($"Start Correct Old ReocrdsRule ,Tenant: {tenant.Key}");
                            RunAsync(recordsRules, tenant.Key);
                            Logger.Info($"Correct Old ReocrdsRule success,Tenant: {tenant.Key}");
                        }
                        else
                        {
                            Logger.Info($"Skip Correct Old ReocrdsRule, Because no need upgrade,Tenant: {tenant.Key} ");
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"An error occurred while Correct Old ReocrdsRule in tenant: {tenant.Key}. Error:{e}");
                    }
                });
            }

        }

        public bool NeedUpgrade(String tenantId, out IEnumerable<RMRule> recordsRules)
        {
            recordsRules = null;
            if (RMTenantUpgradeHelper.IsNeedUpgrade(tenantId, RMUpgradeFeature.CorrectOldReocrdsRule))
            {
                var jobs = JobMonitorService.GetJobsByJobType(JobType.CloudArchiverMigration);
                // JobStatus 有 processing的，状态不改动，返回false；下一次升级
                if (jobs.Any(job => job.Status == (int)JobStatus.InProgress))
                {
                    return false;
                }
                // JobStatus 有 finish 且无processing的，需要升级，执行下面判断逻辑，
                else if (jobs.Any(job => job.Status == (int)JobStatus.Finished))
                {
                    //判断是否有 ModelType == (int)RuleModel.None 的Rule的Tenant
                    IEnumerable<RMRule> allRules = RMRuleDao.GetRulesWithoutRemovedAsync().GetAwaiter().GetResult();
                    recordsRules = allRules?.Where(r => r.ModelType == (int)RuleModel.None || r.ModelType == (int)RuleModel.Records);
                    //tenant内没有符合条件的rule，跳过
                    if (recordsRules == null || recordsRules.Count() == 0)
                    {
                        RMTenantUpgradeHelper.SetToUpgrading(tenantId);
                        RMTenantUpgradeHelper.SetToFinish(tenantId, RMUpgradeFeature.CorrectOldReocrdsRule, RMUpgradeStatus.Success);
                        return false;
                    }
                    return true;
                }
                else
                {
                    RMTenantUpgradeHelper.SetToUpgrading(tenantId);
                    RMTenantUpgradeHelper.SetToFinish(tenantId, RMUpgradeFeature.CorrectOldReocrdsRule, RMUpgradeStatus.Success);
                    return false;
                }
            }
            return false;
        }


        public void RunAsync(IEnumerable<RMRule> recordsRules, String tenantId)
        {
            try
            {
                List<String> profileIdList = RMMiscProfileDao.LoadAllRecordsRules().Select(profile => profile.Id).ToList();
                profileIdList = profileIdList.Where(profileId => recordsRules.Any(rule => rule.RuleId.ToString().Equals(profileId, StringComparison.OrdinalIgnoreCase))).ToList();
                DeleteRMMiscProfileInIdListAsync(profileIdList).GetAwaiter().GetResult();

                List<RMStorageDeviceInfo> storageDeviceInfoList = RMStorageDeviceInfoDao.FindAll();
                var ruleStoragePolicyIDs = GetAllRuleStoragePolicyIDsAsync().GetAwaiter().GetResult();

                RMTenantUpgradeHelper.SetToUpgrading(tenantId);
                foreach (var recordsRule in recordsRules)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(recordsRule.Extension))
                        {
                            Logger.Error($"Records Rule extension is empty: {recordsRule.Id} - {recordsRule.RuleId}");
                            continue;
                        }
                        Logger.Info($"Start Correct Old ReocrdsRule success,Rule:{recordsRule.Id}");
                        var rule = SerializerHelper.DeserializeByDataContractJsonSerializer<Rule>(recordsRule.Extension);
                        UpdateStoragePolicy(rule, storageDeviceInfoList, ruleStoragePolicyIDs);
                        recordsRule.Extension = SerializerHelper.SerializeByDataContractJsonSerializer(rule);
                        RecordsRuleManagement.CreateRecordsRule(rule, false, true == recordsRule.DAOMigrated);
                        Logger.Info($"Correct Old ReocrdsRule success, Recorded in RMMiscProfiles,Rule:{recordsRule.Id}");
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"An error occurred while Correct Old ReocrdsRule in Rule:{recordsRule.Id}");
                        throw;
                    }
                }
                RMRuleDao.BatchUpdate(recordsRules.ToList());
                Logger.Info($"Rule in the list {String.Join(",", recordsRules.Select(r => r.RuleId))} is updated to RMRules success");
                RMTenantUpgradeHelper.SetToFinish(tenantId, RMUpgradeFeature.CorrectOldReocrdsRule, RMUpgradeStatus.Success);
            }
            catch
            {
                Logger.Error($"An error occurred while Correct Old ReocrdsRule, fail update Rules in the list {String.Join(",", recordsRules.Select(r => r.RuleId))}");
                RMTenantUpgradeHelper.SetToFinish(tenantId, RMUpgradeFeature.CorrectOldReocrdsRule, RMUpgradeStatus.Failed);
                throw;
            }
        }

        private async Task DeleteRMMiscProfileInIdListAsync(List<String> profileIdList)
        {
            await DatabaseUtility.BatchOperationAsync(
                profileIdList,
                async (batchIDs) =>
                {
                    try
                    {
                        await RMMiscProfileDao.BatchDeleteAsync(profileIdList);
                        Logger.Info($"Remove Id in ${String.Join(",", profileIdList)} of MiscProfile Success");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Remove Id in ${String.Join(",", profileIdList)} of MiscProfile Fail,error:{ex}");
                        throw;
                    }
                },
                50);
        }


        private void UpdateStoragePolicy(Rule soRule, List<RMStorageDeviceInfo> storageDeviceInfoList, Dictionary<string, string> ruleStoragePolicyIDs)
        {
            var ruleStorageId = soRule.StoragePolicyId;
            if (string.IsNullOrEmpty(ruleStorageId))
            {
                Logger.Info($"Records Rule's StoragePolicyId is null: {soRule.Id}");
                return;
            }
            if (Guid.TryParse(ruleStorageId, out var tempGuid) && storageDeviceInfoList.Any(d => d.Id == tempGuid))
            {
                Logger.Info($"Records Rule's StoragePolicyId is right: {soRule.Id}");
                return;
            }

            if (Guid.Empty.ToString().Equals(ruleStorageId, StringComparison.OrdinalIgnoreCase))
            {
                Logger.Warn($"Records Rule's StoragePolicyId is empty: {soRule.Id}");
                if (!ruleStoragePolicyIDs.TryGetValue(soRule.Id.ToLower(), out ruleStorageId))
                {
                    Logger.Warn($"Records Rule's StoragePolicyId not found: {soRule.Id}|{TenantLocalValue.LogonGroupId}");
                }
                else
                {
                    Logger.Info($"Records Rule's StoragePolicyId: {soRule.Id}|{ruleStorageId}");
                }
            }

            var storages = storageDeviceInfoList
                .Where(device => device.DAOStoragePolicyId == ruleStorageId)
                .OrderBy(device => device.Name);
            var storageDevice = storages.FirstOrDefault(device => device.Status == 0);
            if (storageDevice == null)
            {
                Logger.Warn($"Records Rule not found avaliable device: {soRule.Id}|{TenantLocalValue.LogonGroupId}");
                if (storages.FirstOrDefault() != null)
                {
                    storageDevice = storages.FirstOrDefault();
                }

                if (storageDevice == null)
                {
                    Logger.Error($"Records Rule not found device. wil try by name: {soRule.Id}|{soRule.StoragePolicyName}|{TenantLocalValue.LogonGroupId}");

                    storageDevice = storageDeviceInfoList
                        .Where(device => device.Name.Equals(soRule.StoragePolicyName, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(d => d.Status)
                        .FirstOrDefault();

                    if (storageDevice == null)
                    {
                        Logger.Error($"Records Rule not found device: {soRule.Id}|{TenantLocalValue.LogonGroupId}");
                        return;
                    }
                }
            }

            UpdateStoragePolicyForSingleRule(soRule, storageDevice);
            UpdateStoragePolicyForSingleRule(soRule.OneDriveRule, storageDevice);
            UpdateStoragePolicyForSingleRule(soRule.PhysicalRule, storageDevice);
        }

        private void UpdateStoragePolicyForSingleRule(Rule soRule, RMStorageDeviceInfo storageDevice)
        {
            if (soRule != null && storageDevice != null)
            {
                soRule.StoragePolicyId = storageDevice.Id.ToString();
                Logger.Info($"modify StoragePolicyId from {soRule.StoragePolicyId} to {storageDevice.Id}");
                soRule.StoragePolicyName = storageDevice.Name;
            }
        }

        private Task<T> GetArchiverMigrationDataAsync<T>(
            DAOAPIClientV1 daoClient,
            Func<Cloud.Sdk.Dao.Services.IArchiverMigrationService, Task<Cloud.Sdk.Data.Dao.ArchiverMigrationData>> action,
            bool isDataContract = false)
        {
            return daoClient.GetArchiverMigrationDataAsync<T>(action, isDataContract);
        }

        private async Task<List<Cloud.Sdk.Data.Dao.SORule>> GetAllRulesAsync(DAOAPIClientV1 daoClient, int offset, int fetchRows)
        {
            return await GetArchiverMigrationDataAsync<List<Cloud.Sdk.Data.Dao.SORule>>(daoClient, (service) =>
            {
                return service.GetAllRules(new Cloud.Sdk.Data.Dao.FetchDataInfo()
                {
                    Offset = offset,
                    FetchSize = fetchRows
                });
            }, true);
        }
        private async Task<Dictionary<string, string>> GetAllRuleStoragePolicyIDsAsync()
        {
            Dictionary<string, string> ruleStoragePolicyIDs = new Dictionary<string, string>();
            var daoClient = new DAOAPIClientV1(true);
            List<Cloud.Sdk.Data.Dao.SORule> archiverRules = null;
            int count = 0;
            int fetchSize = 20;
            int offset = 0;
            do
            {
                archiverRules = await GetAllRulesAsync(daoClient, offset, fetchSize);
                count = archiverRules?.Count ?? 0;
                if (archiverRules != null)
                {
                    foreach (var rule in archiverRules)
                    {
                        ruleStoragePolicyIDs[rule.Id.ToLower()] = rule.StoragePolicyId;
                    }
                    offset += count;
                }
                
            } while (count >= fetchSize);

            return ruleStoragePolicyIDs;
        }
    }
}
