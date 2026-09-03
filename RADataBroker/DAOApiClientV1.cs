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
using AvePoint.GCommon.Contract.Server.Common.ExportReport.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.CustomizeConnector.Enums;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RuleManagement;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RADataBroker.Common;
using Cloud.Sdk.Dao;
using Cloud.Sdk.Dao.Services;
using Cloud.Sdk.Data.Dao;
using DocAveOnline.WebApi.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IJobMonitorService = AvePoint.RA.Contract.RMWeb.IJobMonitorService;

namespace AvePoint.RA.RADataBroker
{
    public class DAOAPIClientV1
    {
        protected static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(DAOAPIClientV1));
        private DocAveOnlineApiClient onlineApiClient = null;
        private bool _initClientAfterUpgradeOpus = false;
        #region interface 

        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private ISettingProfilesDao SettingProfileDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();
        private IRecordsRuleManagement RecordsRuleManagement => PlatformWindsorManager.GetService<IRecordsRuleManagement>();

        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private IRMCache Cache => PlatformWindsorManager.GetService<IRMCache>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();

        #endregion
        public DAOAPIClientV1(bool initClientAfterUpgradeOpus = false)
        {
            if (RMGlobalConfiguration.EnvSetting.IsGCPEnvironment)
            {
                logger.Info("GCP environment skip to init DAO API Client.");
                return;
            }

            _initClientAfterUpgradeOpus = initClientAfterUpgradeOpus;
            if (_initClientAfterUpgradeOpus || !TenantService.IsNewOpusTenant())
            {
                onlineApiClient = DAOClientCache.GetDAOApiClient(TenantLocalValue.LogonGroupId);
            }
        }

        public string StartDownloadArchivedContent(string siteUrl, string pathMd5, string jobId, string fileUrl, string archiverIndex)
        {
            string restoreJobId = string.Empty;
            try
            {
                //TODO support end user restore in records
                if (TenantService.IsNewOpusTenant())
                {
                    return restoreJobId;
                }
                else
                {
                    Cloud.Sdk.Data.Dao.ArchivedContentRestoreConfig config = new Cloud.Sdk.Data.Dao.ArchivedContentRestoreConfig()
                    {
                        SiteUrl = siteUrl,
                        ArchivedContentInfos = new List<Cloud.Sdk.Data.Dao.ArchivedContentInfo>()
                        {
                             new Cloud.Sdk.Data.Dao.ArchivedContentInfo()
                             {
                                 BackUpJobId = jobId,
                                 PathMD5 = pathMd5,
                                 FileUrl = fileUrl,
                                 ExtensionString = archiverIndex
                             }
                        }
                    };
                    string storageStr = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECORDS_HISTORY_STORAGE_CONNECTION_STRING_FULL];
                    config.RestoreStorage = CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(Encoding.UTF8.GetBytes(storageStr));
                    RetryUtility.RetryWhen(() =>
                    {
                        var task1 = Task.Run(() => { return onlineApiClient.ArchiverService.DownloadArchivedContent(config); });
                        var result = task1.GetAwaiter().GetResult();
                        if (result != null && result.Jobs != null && result.Jobs.Count > 0 && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
                        {
                            restoreJobId = !string.IsNullOrWhiteSpace(result.Jobs[0].Id) ? result.Jobs[0].Id : string.Empty;
                        }
                        else
                        {
                            logger.Warn($"Error occurred while starting download archived content job. BackUpJobId:{jobId} Error Code:{result?.ErrorCode} Error Message:{result?.ErrorMessage} Job:{result?.Jobs?.FirstOrDefault()?.Id}");

                        }
                        return restoreJobId;
                    }, ShouldRetrAPIErrorMessage, 3);
                }
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while starting download archived content job. BackUpJobId:{jobId} Error:{e.ToString()}");
            }
            return restoreJobId;
        }

        public List<GCommon.Contract.StorageOptimization.Object.SOJob> GetSOJobsByIds(List<string> jobIds)
        {
            List<GCommon.Contract.StorageOptimization.Object.SOJob> jobs = new List<GCommon.Contract.StorageOptimization.Object.SOJob>();
            try
            {
                if (TenantService.IsNewOpusTenant())
                {
                    List<JMItemInfo> jobMonitor = JobMonitorService.GetJobsForRecenterAsync(jobIds).GetAwaiter().GetResult();
                    foreach (var temp in jobMonitor)
                    {
                        SOJob opusJob = new SOJob()
                        {
                            Id = temp.JobId,
                            Progress = temp.Progress,
                            State = (int)temp.Status
                        };
                        jobs.Add(opusJob);
                    };
                    return jobs;
                }
                else
                {
                    RetryUtility.RetryWhen(() =>
                    {
                        var task1 = Task.Run(() => { return onlineApiClient.JobMonitorService.GetJobListByIds(jobIds); });
                        var result = task1.GetAwaiter().GetResult();
                        if (result != null && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
                        {
                            if (result.Jobs != null && result.Jobs.Count > 0)
                            {
                                foreach (var job in result.Jobs)
                                {
                                    jobs.Add(ConvertUtilityNewSDK.ConvertJobDtoToSOJob(job));
                                }
                            }
                        }
                        else
                        {
                            logger.Warn($"Error occurred while getting so jobs. Error Code:{result?.ErrorCode} Error Message:{result?.ErrorMessage}");
                        }
                        return jobs;
                    }, ShouldRetrAPIErrorMessage, 3);
                }
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while getting so jobs. Error:{e.ToString()}");
            }
            return jobs;
        }

        #region run archiver job

        public async Task<AzureTableConnectContract> GetArchiverDataBaseConfigAsync()
        {
            async Task<AzureTableConnectContract> GetArchiverDataBaseConfigInternalAsync()
            {
                GCommon.Contract.StorageOptimization.Object.AzureTableConnectContract result = null;
                try
                {
                    await RetryUtility.RetryWhenAsync(async () =>
                    {
                        var config = await onlineApiClient.ArchiverService.GetArchiverDatabaseConfigInfo();
                        if (config != null)
                        {
                            result = ConvertUtilityNewSDK.ConvertToAzureTableContract(config);
                            if (string.IsNullOrEmpty(result.AccountName) || string.IsNullOrEmpty(result.AccountKey))
                            {
                                logger.Info($"ArchiverDataBaseConfig name or key is empty, we will use managed identity auth.");
                            }
                        }
                        else
                        {
                            logger.Error($"Error occurred while GetArchiverDataBaseConfig. DB is null.");
                        }
                        return result;
                    }, ShouldRetrAPIErrorMessage, 3);
                }
                catch (Exception ex)
                {
                    logger.Error("error occurred while GetArchiverDataBaseConfig ERROR:{0}", ex.ToString());
                    throw;
                }
                return result;
            }

            if (onlineApiClient == null)
            {
                return null;
            }

            return await Cache.TryGetAsync(IRMCache.Keys.DAOAPIClientV1_GetArchiverDataBaseConfig, GetArchiverDataBaseConfigInternalAsync);

        }

        public string RunNow(List<AvePoint.GCommon.Contract.Tree.Object.SPTreeNodeDto> selectedNode, AvePoint.GCommon.Contract.StorageOptimization.Object.SORuleInfoContract ruleInfo, List<AvePoint.GCommon.Contract.StorageOptimization.Object.RuleNodeContract> breakInheriting)
        {
            var key = string.Empty;
            try
            {

                logger.Info("enter run now process.");
                var tree = ConvertUtilityNewSDK.ConvertToAPISPTreeNodeDto(selectedNode);
                var treeNode = tree[0];
                var ruleContract = ConvertUtilityNewSDK.ConvertToAPIRuleInfoContract(ruleInfo);
                var breakNodes = ConvertUtilityNewSDK.ConvertToAPIRuleNodeContactList(breakInheriting);
                logger.Info("begin to run now job:{0}, {1}, {2}", ruleContract == null, breakNodes == null, tree == null);
                Cloud.Sdk.Data.Dao.RunRuleInfo runRuleInfo = new Cloud.Sdk.Data.Dao.RunRuleInfo()
                {
                    BreakInheritingNode = breakNodes,
                    RuleInfo = ruleContract,
                    SelectedNode = treeNode
                };
                RetryUtility.RetryWhen(() =>
                {
                    var task = Task.Run(() => { return onlineApiClient.ArchiverRuleService.RunNow(runRuleInfo); });
                    var result = task.GetAwaiter().GetResult();
                    if (result != null && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
                    {
                        key = result.Value;
                    }
                    else
                    {
                        logger.Error($"Error occurred while RunNow. Error code:{result?.ErrorCode} Error message:{result?.ErrorMessage}");
                        throw new Exception(result?.ErrorMessage);
                    }
                    logger.Info("get result:{0}", result);
                    return key;
                }, ShouldRetrAPIErrorMessage, 3);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunNow ERROR:{0}", ex.ToString());
                throw;
            }
            return key;
        }

        public string RunNowForPhysicalRecords(int locationID, AvePoint.GCommon.Contract.StorageOptimization.Object.SORuleInfoContract ruleInfo)
        {
            var key = string.Empty;
            try
            {
                logger.Info("enter physical run now process.");
                var ruleContract = ConvertUtilityNewSDK.ConvertToAPIRuleInfoContract(ruleInfo);
                Cloud.Sdk.Data.Dao.PhysicalRecordsRunRuleInfo physicalRecordsRunRuleInfo = new Cloud.Sdk.Data.Dao.PhysicalRecordsRunRuleInfo()
                {
                    RuleInfo = ruleContract,
                    LocationId = locationID
                };
                RetryUtility.RetryWhen(() =>
                {
                    var task = Task.Run(() => { return onlineApiClient.ArchiverRuleService.PhysicalRecordsRunNow(physicalRecordsRunRuleInfo); });
                    var result = task.GetAwaiter().GetResult();
                    if (result != null && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
                    {
                        key = result.Value;
                    }
                    else
                    {
                        logger.Error($"Error occurred while RunNow. Error code:{result?.ErrorCode} Error message:{result?.ErrorMessage}");
                        throw new Exception(result?.ErrorMessage);
                    }
                    logger.Info("get result:{0}", result);
                    return key;
                }, ShouldRetrAPIErrorMessage, 3);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunNow ERROR:{0}", ex.ToString());
                throw;
            }
            return key;
        }

        public string RunNowForExchange(List<AvePoint.GCommon.Contract.Tree.Object.ExchangeOnlineTreeNodeDto> selectedNode, AvePoint.GCommon.Contract.StorageOptimization.Object.SORuleInfoContract ruleInfo, List<AvePoint.GCommon.Contract.StorageOptimization.Object.RuleNodeContract> breakInheriting)
        {
            var key = string.Empty;
            try
            {
                logger.Info("enter exo run now process.");
                var tree = ConvertUtilityNewSDK.ConvertToApiExOLTreeNodeDtos(selectedNode);
                var treeNode = tree[0];
                var ruleContract = ConvertUtilityNewSDK.ConvertToAPIRuleInfoContract(ruleInfo);
                var breakNodes = ConvertUtilityNewSDK.ConvertToAPIRuleNodeContactList(breakInheriting);
                logger.Info("begin to run now job:{0}, {1}, {2}", ruleContract == null, breakNodes == null, tree == null);
                Cloud.Sdk.Data.Dao.ExchangeRunRuleInfo runRuleInfo = new Cloud.Sdk.Data.Dao.ExchangeRunRuleInfo()
                {
                    BreakInheritingNode = breakNodes,
                    RuleInfo = ruleContract,
                    SelectedNode = treeNode
                };
                RetryUtility.RetryWhen(() =>
                {
                    var task = Task.Run(() => { return onlineApiClient.ArchiverRuleService.ExchangeArchiveRunNow(runRuleInfo); });
                    var result = task.GetAwaiter().GetResult();
                    if (result != null && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
                    {
                        key = result.Value;
                    }
                    else
                    {
                        logger.Error($"Error occurred while RunNowForExchange. Error code:{result?.ErrorCode} Error message:{result?.ErrorMessage}");
                        throw new Exception(result?.ErrorMessage);
                    }
                    logger.Info("get result:{0}", result);
                    return key;

                }, ShouldRetrAPIErrorMessage, 3);

            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunNow ERROR:{0}", ex.ToString());
                throw;
            }
            return key;
        }

        public int GetArchiverDBAndIndexDeviceSetting()
        {
            int archiveSetting = 1;
            try
            {
                if (TenantService.IsNewOpusTenant())
                {
                    var profile = SettingProfileDao.LoadByType((int)SettingProfilesType.IndexDevice);
                    if (profile != null)
                    {
                        archiveSetting = 0;
                    }
                }
                else
                {
                    RetryUtility.RetryWhen(() =>
                    {
                        var task = Task.Run(() => { return onlineApiClient.ArchiverService.Validate(); });
                        var result = task.GetAwaiter().GetResult();
                        if (result != null && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
                        {
                            return archiveSetting = int.Parse(result.Value.ToString());
                        }
                        else
                        {
                            logger.Error($"Error occurred while GetArchiverDBAndIndexDeviceSetting. Error code:{result?.ErrorCode} Error message:{result?.ErrorMessage}");
                            return 0;
                        }
                    }, ShouldRetrAPIErrorMessage, 3);
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while GetArchiverDBAndIndexDeviceSetting ERROR:{0}", ex.ToString());
                throw;
            }
            return archiveSetting;

        }

        public List<AvePoint.GCommon.Contract.Storage.Entity.StoragePolicyDto> GetAllStoragePolicy()
        {
            List<AvePoint.GCommon.Contract.Storage.Entity.StoragePolicyDto> sps = new List<AvePoint.GCommon.Contract.Storage.Entity.StoragePolicyDto>();
            try
            {
                RetryUtility.RetryWhen(() =>
                {
                    var task = Task.Run(() => { return onlineApiClient.StoragePolicyService.All(); });
                    var result = task.GetAwaiter().GetResult();
                    if (result != null && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
                    {
                        foreach (var policy in result.Values)
                        {
                            sps.Add(ConvertUtilityNewSDK.ConvertToStoragePolicyDto(policy));
                        }

                    }
                    else
                    {
                        logger.Error($"Error occurred while GetAllStoragePolicy. Error code:{result?.ErrorCode} Error message:{result?.ErrorMessage}");
                    }
                    return sps;
                }, ShouldRetrAPIErrorMessage, 3);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while GetAllStoragePolicy ERROR:{0}", ex.ToString());
                throw ex;
            }
            return sps;
        }

        public AvePoint.GCommon.Contract.Storage.Entity.StoragePolicyDto GetStoragePolicyById(string id)
        {
            AvePoint.GCommon.Contract.Storage.Entity.StoragePolicyDto sp = null;
            try
            {
                RetryUtility.RetryWhen(() =>
                {
                    var task = Task.Run(() => { return onlineApiClient.StoragePolicyService.Load(id); });
                    var result = task.GetAwaiter().GetResult();
                    if (result != null && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
                    {
                        sp = ConvertUtilityNewSDK.ConvertToStoragePolicyDto(result);
                    }
                    else
                    {
                        logger.Error($"Error occurred while GetStoragePolicyById. Error code:{result?.ErrorCode} Error message:{result?.ErrorMessage}");
                    }
                    return sp;
                }, ShouldRetrAPIErrorMessage, 3);

            }
            catch (Exception ex)
            {
                logger.Error("error occurred while GetStoragePolicyById ERROR:{0}", ex.ToString());
                throw ex;
            }
            return sp;
        }

        public List<GCommon.Contract.Server.ControlPanel.Cryptography.DataEncryptionProfile> GetAllSecurityProfile()
        {
            List<GCommon.Contract.Server.ControlPanel.Cryptography.DataEncryptionProfile> sps = new List<GCommon.Contract.Server.ControlPanel.Cryptography.DataEncryptionProfile>();
            try
            {
                RetryUtility.RetryWhen(() =>
                {
                    var task = Task.Run(() => { return onlineApiClient.SecurityProfileService.GetAllSecurityProfile(); });
                    var result = task.GetAwaiter().GetResult();
                    if (result != null)
                    {
                        foreach (var profile in result)
                        {
                            sps.Add(ConvertUtilityNewSDK.ConvertToDateEncryption(profile));
                        }
                    }
                    else
                    {
                        logger.Error($"Error occurred while GetAllSecurityProfile. Result is null.");
                    }
                    return sps;
                }, ShouldRetrAPIErrorMessage, 3);

            }
            catch (Exception ex)
            {
                logger.Error("error occurred while GetAllSecurityProfile ERROR:{0}", ex.ToString());
                throw ex;
            }

            return sps;
        }

        #endregion

        #region storage location
        public async Task<List<ExportReportDto>> GetAllExportLocationAsync()
        {
            List<GCommon.Contract.Server.Common.ExportReport.Object.ExportReportDto> locations = new List<GCommon.Contract.Server.Common.ExportReport.Object.ExportReportDto>();
            try
            {
                if (TenantService.IsNewOpusTenant())
                {
                    (await StorageDeviceService.GetAllAsync()).ForEach(s =>
                    {
                        locations.Add(new GCommon.Contract.Server.Common.ExportReport.Object.ExportReportDto()
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Type = s.Type,
                            IsSystemStorage= s.IsSystemStorage
                        });
                    });
                }
                else
                {
                    RetryUtility.RetryWhen(() =>
                    {
                        var task = Task.Run(() => { return onlineApiClient.ExportLoationService.LoadByType(new List<Cloud.Sdk.Data.Dao.ExportReportType>() { Cloud.Sdk.Data.Dao.ExportReportType.Storage }); });
                        var result = task.GetAwaiter().GetResult();
                        if (result != null && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
                        {
                            var exportLocations = result.Values.Where(e => e.StorageType == Cloud.Sdk.Data.Dao.StorageDeviceType.SFTP || e.StorageType == Cloud.Sdk.Data.Dao.StorageDeviceType.CloudAzure).ToList();
                            foreach (var location in exportLocations)
                            {
                                locations.Add(ConvertUtilityNewSDK.ConvertToExportLocation(location));
                            }
                        }
                        else
                        {
                            logger.Error($"Error occurred while GetAllExportLocation. Error code:{result?.ErrorCode} Error message:{result?.ErrorMessage}");
                        }
                        return locations;
                    }, ShouldRetrAPIErrorMessage, 3);
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while GetAllExportLocation ERROR:{0}", ex.ToString());
                throw;
            }
            logger.Info($"Get all export locations from dao. {string.Join(",", locations.Select(l => l.Id))}");
            return locations;
        }

        public async Task<List<ExportReportDto>> GetGoogleExportLocationAsync()
        {
            List<ExportReportDto> locations = new List<ExportReportDto>();
            try
            {
                if (TenantService.IsNewOpusTenant())
                {
                    (await StorageDeviceService.GetAllAsync()).ForEach(s =>
                    {
                        locations.Add(new ExportReportDto()
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Type = s.Type,
                            IsSystemStorage = s.IsSystemStorage
                        });
                    });
                }
                else
                {
                    RetryUtility.RetryWhen(() =>
                    {
                        var task = Task.Run(() => { return onlineApiClient.ExportLoationService.LoadByType(new List<Cloud.Sdk.Data.Dao.ExportReportType>() { Cloud.Sdk.Data.Dao.ExportReportType.Storage }); });
                        var result = task.GetAwaiter().GetResult();
                        if (result != null && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
                        {
                            var exportLocations = result.Values.Where(e => e.StorageType == Cloud.Sdk.Data.Dao.StorageDeviceType.SFTP || e.StorageType == Cloud.Sdk.Data.Dao.StorageDeviceType.CloudAzure).ToList();
                            foreach (var location in exportLocations)
                            {
                                locations.Add(ConvertUtilityNewSDK.ConvertToExportLocation(location));
                            }
                        }
                        else
                        {
                            logger.Error($"Error occurred while GetGoogleExportLocation. Error code:{result?.ErrorCode} Error message:{result?.ErrorMessage}");
                        }
                        return locations;
                    }, ShouldRetrAPIErrorMessage, 3);
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while GetGoogleExportLocation. ERROR:{0}", ex.ToString());
                throw;
            }
            logger.Info($"Get google export locations from dao. {string.Join(",", locations.Select(l => l.Id))}");
            return locations;
        }

        public string GetExportLocationbyId(string id)
        {
            try
            {
                if (TenantService.IsNewOpusTenant())
                {
                    return StorageDeviceService.GetStorageDeviceById(id, needDecryptSecert: true).ConnectionString;
                }

                return RetryUtility.RetryWhen(() =>
                {
                    var task = Task.Run(() => onlineApiClient.ExportLoationService.Load(id));
                    var result = task.GetAwaiter().GetResult();

                    if (result != null && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
                    {
                        return result.ConnectionString;
                    }

                    logger.Error($"Error occurred while GetExportLocationbyId. Id:{id} Error code:{result?.ErrorCode} Error message:{result?.ErrorMessage}");
                    return string.Empty;
                }, ShouldRetrAPIErrorMessage, 3);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while GetExportLocationbyId ERROR:{0}", ex.ToString());
                throw;
            }
        }


        public (string name, string connectionString ) GetExportLocationNameAndConntionbyId(string id)
        {
            try
            {
                if (TenantService.IsNewOpusTenant())
                {
                    var location = StorageDeviceService.GetStorageDeviceById(id, needDecryptSecert: false);
                    return (location.Name, location.ConnectionString);
                }

                return RetryUtility.RetryWhen(() =>
                {
                    var task = Task.Run(() => onlineApiClient.ExportLoationService.Load(id));
                    var result = task.GetAwaiter().GetResult();

                    if (result != null && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
                    {
                        return (result.Name, result.ConnectionString);
                    }

                    logger.Error($"Error occurred while GetExportLocationbyId. Id:{id} Error code:{result?.ErrorCode} Error message:{result?.ErrorMessage}");
                    return (string.Empty, string.Empty);
                }, ShouldRetrAPIErrorMessage, 3);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while GetExportLocationbyId ERROR:{0}", ex.ToString());
                throw;
            }
        }

        public async Task<Dictionary<Guid, int>> GetExportLocationTypesAsync()
        {
            Dictionary<Guid, int> typeDic = new Dictionary<Guid, int>();
            try
            {
                if (TenantService.IsNewOpusTenant())
                {
                    typeDic = (await StorageDeviceService.GetAllAsync()).ToDictionary(s => new Guid(s.Id), k => k.Type);
                }
                else
                {
                    RetryUtility.RetryWhen(() =>
                    {
                        var task = Task.Run(() => { return onlineApiClient.ExportLoationService.LoadByType(new List<Cloud.Sdk.Data.Dao.ExportReportType>() { Cloud.Sdk.Data.Dao.ExportReportType.Storage }); });
                        var result = task.GetAwaiter().GetResult();
                        if (result != null && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
                        {
                            var exportLocations = result.Values.Where(e => e.StorageType == Cloud.Sdk.Data.Dao.StorageDeviceType.FTP || e.StorageType == Cloud.Sdk.Data.Dao.StorageDeviceType.SFTP || e.StorageType == Cloud.Sdk.Data.Dao.StorageDeviceType.CloudAzure).ToList();
                            foreach (var location in exportLocations)
                            {
                                if (!typeDic.ContainsKey(new Guid(location.Id)))
                                {
                                    typeDic.Add(new Guid(location.Id), (int)location.StorageType);
                                }
                            }
                        }
                        else
                        {
                            logger.Error($"Error occurred while GetExportLocationTypes. Error code:{result?.ErrorCode} Error message:{result?.ErrorMessage}");
                        }
                        return typeDic;

                    }, ShouldRetrAPIErrorMessage, 3);
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while GetExportLocationTypesss ERROR:{0}", ex.ToString());
                throw;
            }
            return typeDic;
        }
        #endregion

        #region rules
        public bool CreateRuleInProfile(GCommon.Contract.StorageOptimization.Object.Rule ruleInfo)
        {
            bool created = false;
            try
            {
                if (TenantService.IsNewOpusTenant())
                {
                    return RecordsRuleManagement.CreateRecordsRule(ruleInfo);
                }
                else
                {
                    RetryUtility.RetryWhen(() =>
                    {
                        var apiRule = ConvertUtilityNewSDK.ConvertSORuleToArchiverRule(ruleInfo);
                        var task = Task.Run(() => { return onlineApiClient.ArchiverRuleService.Create(apiRule); });
                        var result = task.GetAwaiter().GetResult();
                        if (result != null && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
                        {
                            created = result.Value;
                        }
                        else
                        {
                            logger.Error($"Error occurred while creating rule. Rule name:{ruleInfo.Name} Error code:{result?.ErrorCode} Error message:{result?.ErrorMessage}");
                            throw new Exception(result?.ErrorMessage);
                        }
                        return created;
                    }, ShouldRetrAPIErrorMessage, 3);
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while create rule api Name:{0}, ERROR:{1}", ruleInfo.Name, ex.ToString());
                throw;
            }
            return created;
        }

        public GCommon.Contract.StorageOptimization.Object.Rule LoadRule(string ruleId)
        {
            GCommon.Contract.StorageOptimization.Object.Rule rule = null;
            try
            {
                if (TenantService.IsNewOpusTenant())
                {
                    return RecordsRuleManagement.LoadRule(ruleId);
                }
                else
                {
                    RetryUtility.RetryWhen(() =>
                    {
                        var task = Task.Run(() => { return onlineApiClient.ArchiverRuleService.Get(ruleId); });
                        var result = task.GetAwaiter().GetResult();
                        if (result != null && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
                        {
                            rule = ConvertUtilityNewSDK.ConvertArchiverRuleToSORule(result.Rule);
                        }
                        else
                        {
                            logger.Error($"Error occurred while LoadRule. Rule id:{ruleId} Error code:{result?.ErrorCode} Error message:{result?.ErrorMessage}");
                            throw new Exception(result?.ErrorMessage);
                        }
                        return rule;
                    }, ShouldRetrAPIErrorMessage, 3);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"error occurred while LoadRule:{ruleId} ERROR:{ex.ToString()}");
                throw;
            }
            return rule;
        }

        public async Task<List<Rule>> GetRACreatedRulesAsync()
        {
            List<GCommon.Contract.StorageOptimization.Object.Rule> rules = new List<GCommon.Contract.StorageOptimization.Object.Rule>();
            try
            {
                if (TenantService.IsNewOpusTenant())
                {
                    return await RecordsRuleManagement.GetAllAsync();
                }
                else
                {
                    RetryUtility.RetryWhen(() =>
                    {
                        var task = Task.Run(() => { return onlineApiClient.ArchiverRuleService.GetArchiverRules(Cloud.Sdk.Data.Dao.ProfileType.ArchiverRuleForRevIM); });
                        var result = task.GetAwaiter().GetResult();
                        if (result != null && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
                        {
                            foreach (var rule in result.Values)
                            {
                                rules.Add(ConvertUtilityNewSDK.ConvertArchiverRuleToSORule(rule));
                            }
                        }
                        else
                        {
                            logger.Error($"Error occurred while GetRACreatedRules. Error code:{result?.ErrorCode} Error message:{result?.ErrorMessage}");
                            throw new Exception(result?.ErrorMessage);
                        }
                        return rules;
                    }, ShouldRetrAPIErrorMessage, 3);
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while GetRACreatedRules ERROR:{0}", ex.ToString());
                throw;
            }

            return rules;
        }

        public async Task<bool> EditRuleAsync(GCommon.Contract.StorageOptimization.Object.Rule ruleInfo)
        {
            bool created = false;
            try
            {
                if (TenantService.IsNewOpusTenant())
                {
                    return await RecordsRuleManagement.EditRecordsRuleAsync(ruleInfo);
                }
                else
                {
                    RetryUtility.RetryWhen(() =>
                    {
                        var apiRule = ConvertUtilityNewSDK.ConvertSORuleToArchiverRule(ruleInfo);
                        var task = Task.Run(() => { return onlineApiClient.ArchiverRuleService.EditRule(apiRule); });
                        var result = task.GetAwaiter().GetResult();
                        if (result != null && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
                        {
                            created = result.Value;
                        }
                        else
                        {
                            logger.Error($"Error occurred while editing rule.Rule name:{ruleInfo.Name} Error code:{result?.ErrorCode} Error message:{result?.ErrorMessage}");
                            throw new Exception(result?.ErrorMessage);
                        }
                        return created;
                    }, ShouldRetrAPIErrorMessage, 3);
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while EditRule ERROR:{0}", ex.ToString());
                throw;
            }
            return created;
        }

        public async Task<bool> BatchDeleteRulesAsync(List<string> rulesIds)
        {
            bool success = false;
            try
            {
                if (TenantService.IsNewOpusTenant())
                {
                    return (await RecordsRuleManagement.BatchDeleteRulesAsync(rulesIds)) > 0;
                }
                else
                {
                    using (new RA.Common.PerformanceScope(string.Format("manage.rule.deletefromdao")))
                    {
                        RetryUtility.RetryWhen(() =>
                        {
                            var task = Task.Run(() => { return onlineApiClient.ArchiverRuleService.Delete(rulesIds); });
                            var result = task.GetAwaiter().GetResult();
                            if (result != null && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
                            {
                                success = result.Value;
                            }
                            else
                            {
                                logger.Error($"Error occurred while deleting rules. Error code:{result?.ErrorCode} Error message:{result?.ErrorMessage}");
                                throw new Exception(result?.ErrorMessage);
                            }
                            return success;
                        }, ShouldRetrAPIErrorMessage, 3);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while BatchDeleteRules ERROR:{0}", ex.ToString());
                throw;
            }
            return success;
        }

        #endregion

        #region Job Monitor
        public List<string> GetJobInQueue(List<string> jobKeys)
        {
            var inQueueJobScopeIds = new List<string>();
            try
            {
                if (!TenantService.IsNewOpusTenant())
                {
                    RetryUtility.RetryWhen(() =>
                {
                    var task = Task.Run(() => { return onlineApiClient.JobMonitorService.ScheduleJobQueues(jobKeys); });
                    var result = task.GetAwaiter().GetResult();
                    if (result != null && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
                    {
                        foreach (var job in result.ScheduleJobQueues)
                        {
                            if (job.State == Cloud.Sdk.Data.Dao.ScheduleJobQueueState.Waiting)
                            {
                                inQueueJobScopeIds.Add(job.Key);
                            }
                        }
                    }
                    else
                    {
                        logger.Error($"Error occurred while get job in queue. Job keys:{string.Join(",", jobKeys)}Error code:{result?.ErrorCode} Error message:{result?.ErrorMessage}");
                    }
                    return inQueueJobScopeIds;
                }, ShouldRetrAPIErrorMessage, 3);
                }

            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while get job in queue. jobKeys:{0}, ERROR:{1}", string.Join(",", jobKeys), ex.ToString());
                throw ex;
            }
            return inQueueJobScopeIds;
        }

        public List<GCommon.Contract.StorageOptimization.Object.SOJob> GetJobByRevIMKey(List<string> jobKeys)
        {
            List<GCommon.Contract.StorageOptimization.Object.SOJob> jobs = new List<GCommon.Contract.StorageOptimization.Object.SOJob>();
            try
            {
                if (TenantService.IsNewOpusTenant())
                {
                    return jobs;
                }
                else
                {
                    RetryUtility.RetryWhen(() =>
                    {
                        var task = Task.Run(() => { return onlineApiClient.JobMonitorService.GetJobByRevIMKey(jobKeys); });
                        var result = task.GetAwaiter().GetResult();
                        if (result != null && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
                        {
                            foreach (var job in result.Values)
                            {
                                jobs.Add(ConvertUtilityNewSDK.ConvertToSOJob(job));
                            }
                        }
                        else
                        {
                            logger.Error($"Error occurred while ValidateJob. Job keys:{string.Join(",", jobKeys)}Error code:{result?.ErrorCode} Error message:{result?.ErrorMessage}");
                            throw new Exception(result?.ErrorMessage);
                        }
                        return jobs;
                    }, ShouldRetrAPIErrorMessage, 3);
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while ValidateJob jobKeys:{0}, ERROR:{1}", string.Join(",", jobKeys), ex.ToString());
                throw;
            }
            return jobs;
        }

        public GCommon.Contract.Server.Common.Monitor.Object.Detail.JobDetailInfos JobDetails(Contract.JobMonitor.ArchiverJobDto jobDto, List<string> jobIds, string searchValue, int skip, int take, int[] states, int[] entityTypeFilters)
        {
            if (entityTypeFilters.Length == 0)
            {
                entityTypeFilters = EntityTypeContainer;
            }
            GCommon.Contract.Server.Common.Monitor.Object.Detail.JobDetailInfos infos = new GCommon.Contract.Server.Common.Monitor.Object.Detail.JobDetailInfos();
            try
            {
                if (TenantService.IsNewOpusTenant())
                {
                    //infos.Values = new List<GCommon.Contract.Server.Common.Monitor.Object.Detail.JobDetailDto>();
                    logger.Error($"Error occurred while getting job details. Can not get new Opus job detais from dao.");
                    return infos;
                }
                else
                {
                    Cloud.Sdk.Data.Dao.JobDetailParam param = new Cloud.Sdk.Data.Dao.JobDetailParam()
                    {
                        Id = jobDto.Id,
                        PlanId = jobDto.PlanId,
                        JobState = (int)AvePoint.Common.JobState.Finished,
                        JobType = jobDto.JobType,
                        JobCategory = jobDto.JobCategory,
                        Skip = skip,
                        Take = take,
                        CommonSearch = searchValue,
                        TimeZoneId = string.Empty,
                        States = states,
                        EntityTypes = entityTypeFilters,
                        ZoneType = Cloud.Sdk.Data.Dao.TimeZoneType.Local
                    };
                    RetryUtility.RetryWhen(() =>
                    {
                        var task = Task.Run(() => { return onlineApiClient.JobMonitorService.JobDetails(param); });
                        var result = task.GetAwaiter().GetResult();
                        if (result != null && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
                        {
                            infos = ConvertUtilityNewSDK.ConvertToJobDetailsInfos(result);
                        }
                        else
                        {
                            logger.Error($"Error occurred while getting job details. JobIds:{string.Join(",", jobIds)} Error code:{result?.ErrorCode} Error message:{result?.ErrorMessage}");
                        }
                        return infos;
                    }, ShouldRetrAPIErrorMessage, 3);
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while JobDetails jobIds:{0}, ERROR:{1}", string.Join(",", jobIds), ex.ToString());
                throw;
            }
            return infos;
        }

        public GCommon.Contract.Server.Common.Monitor.Object.Detail.JobSummaryInfos JobSummary(AvePoint.GCommon.Contract.StorageOptimization.Object.SOJob soJob)
        {
            GCommon.Contract.Server.Common.Monitor.Object.Detail.JobSummaryInfos infos = new GCommon.Contract.Server.Common.Monitor.Object.Detail.JobSummaryInfos();
            try
            {
                if (TenantService.IsNewOpusTenant())
                {
                    return infos;
                }
                else
                {
                    Cloud.Sdk.Data.Dao.JobDetailParam param = new Cloud.Sdk.Data.Dao.JobDetailParam()
                    {
                        Id = soJob.Id,
                        PlanId = soJob.PlanId,
                        JobState = soJob.State,
                        JobType = soJob.Type,
                        JobCategory = soJob.Category,
                        Skip = 0,
                        Take = 0,
                        CommonSearch = string.Empty,
                        TimeZoneId = soJob.TimeZoneId,
                        States = new int[] { },
                        EntityTypes = EntityTypeContainer,
                        ZoneType = Cloud.Sdk.Data.Dao.TimeZoneType.Local
                    };
                    RetryUtility.RetryWhen(() =>
                    {
                        var task = Task.Run(() => { return onlineApiClient.JobMonitorService.JobSummary(param); });
                        var result = task.GetAwaiter().GetResult();
                        if (result != null && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
                        {
                            infos = ConvertUtilityNewSDK.ConvertToJobSummaryInfos(result, soJob);
                        }
                        else
                        {
                            logger.Error($"Error occurred while JobMonitorSummary. JobId:{soJob.Id} Error code:{result?.ErrorCode} Error message:{result?.ErrorMessage}");
                        }
                        return infos;
                    }, ShouldRetrAPIErrorMessage, 3);
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while JobMonitorSummary jobId:{0}, ERROR:{1}", soJob.Id, ex.ToString());
                throw;
            }
            return infos;
        }

        private int[] EntityTypeContainer = new int[] {
            (int)JobReportDetailEntityType.NormalInfo,
            (int)JobReportDetailEntityType.ArchiveDeletion,
            (int)JobReportDetailEntityType.Export,
            (int)JobReportDetailEntityType.RecordManager
        };
        #endregion

        #region ExchangeOnline
        //public string RunNowForExchange(List<AvePoint.GCommon.Contract.Tree.Object.ExchangeOnlineTreeNodeDto> selectedNode, AvePoint.GCommon.Contract.StorageOptimization.Object.SORuleInfoContract ruleInfo, List<AvePoint.GCommon.Contract.StorageOptimization.Object.RuleNodeContract> breakInheriting)
        //{
        //    var key = string.Empty;
        //    try
        //    {
        //        logger.Info("enter run now process.");
        //        Dictionary<string, object> dic = new Dictionary<string, object>();
        //        var tree = ConvertUtilityNewSDK.ConvertToApiExOLTreeNodeDtos(selectedNode);
        //        var treeNode = tree[0];
        //        var ruleContract = ConvertUtilityNewSDK.ConvertToAPIRuleInfoContract(ruleInfo);
        //        var breakNodes = ConvertUtilityNewSDK.ConvertToAPIRuleNodeContactList(breakInheriting);
        //        Cloud.Sdk.Data.Dao.ExchangeRunRuleInfo exchangeRunRuleInfo = new Cloud.Sdk.Data.Dao.ExchangeRunRuleInfo()
        //        {
        //            BreakInheritingNode = breakNodes,
        //            RuleInfo = ruleContract,
        //            SelectedNode = treeNode
        //        };
        //        logger.Info("begin to run now job:{0}, {1}, {2}", ruleContract == null, breakNodes == null, tree == null);
        //        RetryUtility.RetryWhen(() =>
        //        {

        //            var task = Task.Run(() => { return onlineApiClient.ArchiverRuleService.ExchangeArchiveRunNow(exchangeRunRuleInfo); });
        //            var result = task.GetAwaiter().GetResult();
        //            if (result != null && result.ErrorCode == Cloud.Sdk.Data.Dao.ErrorCode.none)
        //            {
        //                key = result.Value.ToString();
        //            }
        //            else
        //            {
        //                logger.Error($"Error occurred while running exo disposal job. Error code:{result?.ErrorCode} Error message:{result?.ErrorMessage}");
        //            }
        //            logger.Info("get result:{0}", result);
        //            key = result.ToString();
        //            return key;
        //        }, ShouldRetrAPIErrorMessage, 3);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("error occurred while RunNow ERROR:{0}", ex.ToString());
        //        throw ex;
        //    }
        //    return key;
        //}
        #endregion

        /// <summary>
        /// 返回结果是True代表当前用户在Cloud Archiver中有BackUp数据/有Records创建过Rule, 否则表示用户虽然有Cloud Archiver License但是没有使用过
        /// </summary>
        /// <returns></returns>
        public Task<bool> CloudArchiverEnabled()
        {
            try
            {
                return RetryUtility.RetryWhenAsync(async () =>
                {
                    return await onlineApiClient.ArchiverService.CloudArchiverEnabled();
                }, ShouldRetrAPIErrorMessage, 3);
            }
            catch (Exception ex)
            {
                logger.Error($"An error while call dao api CloudArchiverEnabled, message: {ex}");
                return Task.FromResult(false);
            }
        }
        public Task<bool> IsArchiverMigrating()
        {
            try
            {
                return RetryUtility.RetryWhenAsync(async () =>
                {
                    return await onlineApiClient.ArchiverMigrationService.IsMigrating();
                }, ShouldRetrAPIErrorMessage, 3);
            }
            catch (Exception ex)
            {
                logger.Error($"An error while call dao api CloudArchiverEnabled, message: {ex}");
                return Task.FromResult(false);
            }
        }
        private bool ShouldRetrAPIErrorMessage(Exception e)
        {
            logger.Info($"retry to access api, {e.ToString()}.");
            return e is TimeoutException
                || e is UnauthorizedAccessException
                || e is TaskCanceledException
                || e.InnerException != null && e.InnerException is TimeoutException;
        }

        public Task<T> ExecuteAsync<T>(Func<DocAveOnlineApiClient, Task<T>> action)
        {
            return RetryUtility.RetryWhenAsync(() =>
            {
                return action(onlineApiClient);
            }, ShouldRetrAPIErrorMessage, 3);
        }

        public Task ExecuteAsync(Func<DocAveOnlineApiClient, Task> action)
        {
            return RetryUtility.RetryWhenAsync(() =>
            {
                return action(onlineApiClient);
            }, ShouldRetrAPIErrorMessage, 3);
        }

        public Task<T> GetArchiverMigrationDataAsync<T>(Func<IArchiverMigrationService, Task<ArchiverMigrationData>> action, bool isDataContract = false)
        {
            return RetryUtility.RetryWhenAsync(async () =>
            {
                var result = await action(onlineApiClient.ArchiverMigrationService);

                if (result != null && result.ErrorCode == ErrorCode.none)
                {
                    return isDataContract 
                        ? SerializerHelper.DeserializeByDataContractSerializer<T>(result.JsonData)
                        : SerializerHelper.DeserializeByJsonConvert<T>(result.JsonData);
                }
                else
                {
                    throw new Exception($"Error occurred while getting data. Code: {result?.ErrorCode}, Message: {result?.ErrorMessage}");
                }

            }, ShouldRetrAPIErrorMessage, 3);
        }
        public async Task<(string,string)> GetExportDataSasByJobInfo(string exportJobId, string office365UserMail, bool isDownload)
        { 
            Cloud.Sdk.Data.Dao.ExportJobInfo exJobInfo = new Cloud.Sdk.Data.Dao.ExportJobInfo();
            exJobInfo.ExportJobId = exportJobId;
            exJobInfo.Office365UserMail = office365UserMail;
            exJobInfo.IsDownload = isDownload;
            var res = await ExecuteAsync((apiClient) =>
            {
                return onlineApiClient.ArchiverService.RequestExportedDataSASByJobInfo(exJobInfo);
            });
            if(res == null)
            {
                throw new NullReferenceException("Can not Get Exported Data Sas From Dao, exportJobInfo is null");
            }
            return (res.DataSASString, res.ZipPassword);
        }
    }
}
