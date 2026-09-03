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
using System;
using System.IO;
using System.Text;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.GranularRestore.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.FileTransfer;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Contract.Server.GranularRestore.Object;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.Media.Common;
using AvePoint.Media.Service.DomainModel;
using System.Collections.Generic;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
using AvePoint.Media.Service.ArchiverBackup.Restore;
using AvePoint.GCommon.Contract.Media.TCPRequest;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.GCommon.Contract.CloudServiceCommon;
using AvePoint.ObjectModel.Common;
using System.Web;
using System.Globalization;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.Cryptography;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Archiver.Media;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.SharePoint.RestoreJob.Restore.Content;
using RestoreType = AvePoint.GCommon.Contract.Server.Common.BackupDataSearch.RestoreType;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Common;
using System.Linq;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.GCommon.Contract.Server.ControlPanel.SecurityInformationManager;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Common.Report;
using RecordsHotfixMaintenanceService;
using AvePoint.GCommon.Contract.GranularBackup.Object;
using BackupLevel = AvePoint.GCommon.Contract.GranularBackup.Object.BackupLevel;
using Aspose.Email.PersonalInfo;
using ItemDependencyOption = AvePoint.GCommon.Contract.Server.GranularRestore.Object.ItemDependencyOption;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.RACommonUtility.Encryption;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Utility.Cryptography.Encryption.KeyVault;
using AvePoint.Common.Portal;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.RMWeb.Setting;
using System.Threading.Tasks;
using AvePoint.RA.Common.Util;
using RAArchiverCommon;
using PnP.Framework.Diagnostics;
using Aspose.Pdf.Operators;
using AvePoint.RA.RACommonUtility.Telemetry;
using RAExportCommon;
using Cloud.Sdk.EDiscovery.Services;
using Media.Service.ArchiverBackup.Restore;
using AvePoint.Media.Core.IO;
using AvePoint.RA.Contract.Common;
using static AvePoint.GCommon.Utility.I18N.ContextValues.Configuration;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.SharePoint.RestoreJob;
using log4net;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.CommonFilter;
using System.Diagnostics;
using AvePoint.RA.Contract.RMWeb;

namespace AvePoint.Item.Restore
{
    public abstract class AbstractAveItemRestore
    {
        protected static readonly AveLogger mLog = AveLogger.GetInstance(typeof(AbstractAveItemRestore));
        protected JobType mJobType;

        
        protected string JobId = string.Empty;
        protected ISettingProfilesDao SettingProfileDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();
        protected IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        protected RMAesEncryptorWrapper AesEncryptorWrapper => new();
        protected IArchiverSiteMasterIndexService ArchiverSiteMasterIndexService => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexService>();
        protected IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        protected IRMArchiverSettingDao ArchiverSettingDao => PlatformWindsorManager.GetService<IRMArchiverSettingDao>();
        protected IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();
        protected IRMRemoteNodeService RemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();

        protected IRMGlobalKeyValueDao RMGlobalKeyValueDao => PlatformWindsorManager.GetService<IRMGlobalKeyValueDao>();

        protected ArchiverRestoreRequest AssembleRestoreMessage(string subjobId, SPTreeNodeDto TreeRoot, RestoreSettingAndTree mRestore, bool isPreview = false, bool initializeRestoredFileContext = true)
        {
            ArchiverRestoreRequest message = new ArchiverRestoreRequest();
            //message.Action = ArchiverAction.ARCHIVER_RESTORE_JOB_REQUEST;
            //message.ArchiverRestoreJobRequest = new ArchiverRestoreJobRequest();
            ArchiverSiteMasterIndexContract siteinfo = new ArchiverSiteMasterIndexContract();
            this.GetIndexInfo(TreeRoot, siteinfo);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            if (!isPreview && mRestore.Setting.RestoreTypeSelect == RestoreType.ToSPOLocation)
            {
                var result = ExplorerService.CheckSPUrl4Job(mRestore.Setting.SPOLibOrFolderPath, null);
                if (result == null)
                {
                    mLog.Error($"The SPO location {mRestore.Setting.SPOLibOrFolderPath} is not accessible.");
                    throw new Exception("RM_JS_Rule_SPDestUrlError");
                }

                mRestore.Setting.DestDto = result;
                mRestore.IsRestoreToSPOLocation = true;
            }

            var allSiteMasterIndexes = ArchiverSiteMasterIndexService.GetSiteCollectionWithSubInfos(siteinfo);
            if (!this.ValidateDataForRestore(allSiteMasterIndexes))
            {
                throw new AveException("The Archiver data has already been deleted by the specified Archiver Retention rules.");
            }
            sw.Stop();
            mLog.Info($"linkRestoreReport AssembleRestoreMessage GetSiteCollectionWithSubInfos cost time:{sw.ElapsedMilliseconds}");
            ArchiverRestoreRequest request = this.BuildRestoreRequest(allSiteMasterIndexes, TreeRoot, mRestore, subjobId);
            Stopwatch sw2 = new Stopwatch();
            sw2.Start();
            request.JobId = subjobId;
            request.JobType = mJobType;
            request.CacheLocation = GenerateCacheSettings();
            mLog.Info("Build archiver restore request, is searched result ? {0}", request.IsSearchTree);
            message = request;
            SOArchiverJobInfoStatistics.Instance.InitInstance(subjobId, siteinfo.SiteURL, mJobType, siteinfo.SiteId);
            SOArchiverJobInfoStatistics.Instance.KeepDataOption = -2;
            SOArchiverJobInfoStatistics.Instance.IsDeleteOnlyActionOrRestore = true;
            var volumeGenerator = new VolumeGeneratorFactory().GetVolumeGenerator(ProductModule.ArchiverBackup);
            var IndexVolume = volumeGenerator.GenerateIndexVolume(new VolumeParameter(request));
            var archiveSetting = ArchiverSettingDao.LoadSiteArchiverSettingByUrl(siteinfo.SiteURL);
            sw2.Stop();
            mLog.Info($"linkRestoreReport AssembleRestoreMessage LoadSiteArchiverSettingByUrl cost time:{sw2.ElapsedMilliseconds}");
            Stopwatch sw3 = new Stopwatch();
            sw3.Start();
            if (initializeRestoredFileContext)
            {
                RecordRestoredFile.InitContext(subjobId+"_"+DateTime.UtcNow.Ticks.ToString()+".db", siteinfo?.SiteId, IndexVolume, mRestore.Setting.RestoreTypeSelect == RestoreType.OutOfPlace, SerializerHelper.SerializeByDataContractSerializer(mRestore.Setting), archiveSetting?.CleanRestoredOption, IsArchiverBackupOutputStreamFileLevel() == 0, IsUseSqliteToSaveRestoredDatas());
            }
            sw3.Stop();
            mLog.Info($"linkRestoreReport RecordRestoredFile.InitContext cost time:{sw3.ElapsedMilliseconds}");
            return message;
        }

        public abstract System.Threading.Tasks.Task RunNowAsync();

        private bool IsUseSqliteToSaveRestoredDatas()
        {
            var key = RMKeyValueDao.GetValueByKey("IsUseSqliteToSaveRestoredDatas");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }

        private int IsArchiverBackupOutputStreamFileLevel()
        {
            var key = RMKeyValueDao.GetValueByKey(RMKeyValuesConstants.ArchiverBackupOutputStreamLevel);//0 filelevel,4096 datablock
            int.TryParse(key?.Value, out int result);
            return result;
        }

        protected bool ValidateDataForRestore(List<ArchiverSiteMasterIndexContract> indexWithSubInfos)
        {
            foreach (ArchiverSiteMasterIndexContract index in indexWithSubInfos)
            {
                if (!index.SubInfo.IsNullOrEmpty())
                {
                    return true;
                }
            }
            return false;
        }

        protected static CacheSettingDto GenerateCacheSettings()
        {
            DiskInfoDto disk = new DiskInfoDto()
            {
                Path = Path.Combine(RecordsEnv.AppDomainRootFolder, "ArchiverCache", ItemRestoreConfig.CACHE_DATA_FOLDER_NAME),
                Type = DeviceType.LocalPath,
                Password = null,
                UserName = string.Empty,
                Usage = null
            };
            return new CacheSettingDto
            {
                Extension = new CacheSettingExtension { Path = new List<PathMap>() { new PathMap() { DiskInfo = disk } } },
                LimitFreeSpace = 1024 * 1024 * 1024,//1 GB
            };
        }

        private ArchiverRestoreRequest BuildRestoreRequest(List<ArchiverSiteMasterIndexContract> indexes, SPTreeNodeDto TreeRoot, RestoreSettingAndTree mRestore, string subjobId)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var index = indexes.FirstOrDefault();
            var indexDeviceDto = StorageDeviceService.GetIndexDevice();
            if (indexDeviceDto == null)
            {
                throw new Exception("index device not exist");
            }
            ArchiverRestoreRequest request = new ArchiverRestoreRequest();
            request.RestoreOption = mRestore.Setting.RestoreOption;
            request.NotificationUsers = mRestore.Setting.NotificationUsers;
            request.UseBackupResourceQuota = true; //对应的Resource Quota 值在API 已经去掉所以赋默认值 SAAS-36778
            if (mRestore.BackUpJobId != null && !mRestore.BackUpJobId.Contains('_'))
            {
                mLog.Warn($"this job id is not valid,id:{mRestore.BackUpJobId}");
                mRestore.BackUpJobId = null;
            }
            request.ArchiveJobId = string.IsNullOrEmpty(mRestore.BackUpJobId) ? index?.JobId : mRestore.BackUpJobId;

            request.RestoreJobId = subjobId;
            //request.PlanId = plan.Id;
            request.RestoreVersionsOption = mRestore.Setting.RestoreVersionOption;
            request.KeepVersionsNumber = mRestore.Setting.KeepVersionsNumber;
            request.FarmName = string.Empty;
            request.SiteUrl = index?.SiteURL;
            request.ArchiveTime = index.ArchiverTime;
            request.IndexLogicalDevice = RAStorageUtil.ConvertStorageDeviceDtoToLogicalDeviceDto(indexDeviceDto);
            request.DataLogicalDeviceList = new List<LogicalDeviceDto>();
            GetAllStorageLogicalDevices(indexes)
                .ForEach(ldId => request.DataLogicalDeviceList.Add(RAStorageUtil.ConvertStorageDeviceDtoToLogicalDeviceDto(StorageDeviceService.GetStorageDeviceById(ldId, needDecryptSecert: true))));
            request.LoadTreeOption = string.IsNullOrEmpty(mRestore.BackUpJobId) ? ArchiverLoadTreeOption.SiteCollectionMode : ArchiverLoadTreeOption.JobMode;
            List<RestoreSecurityInfoWrapper> restoreSecurityInfos = GetRestoreSecurityInfoList(indexes);
            if (restoreSecurityInfos.Count > 0)
            {
                request.RestoreSecurityInfos = restoreSecurityInfos;
            }

            request.TreeRoot = TreeRoot;
            sw.Stop();
            mLog.Info($"linkRestoreReport BuildRestoreRequest init cost time:{sw.ElapsedMilliseconds}");
            //SetBposAppProfile(request.TreeRoot, plan.SOPlanExtension);
            //if (request.TreeRoot.Level == NodeLevel.Farm)
            //{
            //    request.IsSearchTree = plan.TreeContents[0].Tree.NodeExtension.IsAdvancedSearchResult;
            //}
            //else if (request.TreeRoot.Level == NodeLevel.Root)
            //{
            //    request.IsSearchTree = request.TreeRoot.Children[0].NodeExtension.IsAdvancedSearchResult;
            //}
            //TreeAccessMapping(request.TreeRoot); 

            if (mRestore.IsSearchAllRestore && mRestore.Setting.SerchContract?.FilterPolicy != null && mRestore.Setting.SerchContract?.FilterPolicy.Level == PolicyLevel.SiteCollection)
            {
                mLog.Info("no need to process search for site collection level in a single site collection restore job");
                mRestore.IsSearchAllRestore = false;
            }

            if (mJobType == JobType.SimulateRestore || mJobType == JobType.PreviewRestore)
            {
                mLog.Info("simulate restore not need storage device ");
            }
            else if (mRestore.Setting.RestoreTypeSelect == RestoreType.InPlace || mRestore.Setting.RestoreTypeSelect == RestoreType.StubOop 
                || mJobType == JobType.AOSPRestore || mRestore.Setting.RestoreTypeSelect == RestoreType.ToSPOLocation
                || mRestore.Setting.RestoreTypeSelect == RestoreType.ArchivedStubs || mRestore.Setting.RestoreTypeSelect == RestoreType.M365InPlaceArchivedFiles)
            {
                // the tree will not really be accurate for select all search result restore
                if (mRestore.IsSearchAllRestore)
                {
                    LogInfoForSelectAllRestore(mRestore.Setting.SerchContract.FilterPolicy);
                }
                else
                {
                    mLog.Info("In place restore job, sort item level node by major and minor version.");
                    //OrderingItemByVersion(request.TreeRoot);
                    mLog.Debug("Tree after sorted:");
                    mLog.Debug(request.TreeRoot.TextNode("---", true));
                }
            }
            else
            {
                if (mRestore.IsSearchAllRestore) LogInfoForSelectAllRestore(mRestore.Setting.SerchContract.FilterPolicy);
                StorageDeviceDto storage = null;
                if (string.IsNullOrEmpty(mRestore.ConnectionString))
                {
                    storage = StorageDeviceService.GetStorageDeviceById(mRestore.Setting.StorageDeviceDto.Id, needDecryptSecert: true);
                }
                else
                {
                    //recenter export
                    storage = new StorageDeviceDto();
                    storage.ConnectionString = mRestore.ConnectionString;
                    storage.Type = (int)StorageDeviceType.CloudAzure;
                }
                if (storage != null)
                {
                    Stopwatch sw2 = new Stopwatch();
                    sw2.Start();
                    var storagePolicy = ConvertStorageDeviceDtoToPhysicalDeviceDto(storage);
                    if (storagePolicy != null)
                    {
                        request.DestinationFSDevice = storagePolicy;
                        mLog.Info("Set outplace physical device: {0}.", storagePolicy.Name);
                    }
                    else if (storagePolicy == null)
                    {
                        mLog.Info("It's restore to RA storage string.");
                    }
                    var zipPassword = GeneratePassword(13, true, false, true, true);
                    request.ZipFilePassword = zipPassword;
                    var encryptPassword = AesEncryptorWrapper.Encrypt(zipPassword);
                    DownloadDataInfoDao.CreateZipPasswordInfo(new RA.DB.Model.RMDownloadDataInfo() { Name = encryptPassword, JobId = subjobId, FileDownloadTime = DateTime.UtcNow.Ticks, DownloadType = DownloadContentType.ZipPasswordInfo });
                    sw2.Stop();
                    mLog.Info($"linkRestoreReport CreateZipPasswordInfo cost time:{sw2.ElapsedMilliseconds}");
                }
                else
                {
                    mLog.Info("Can not find outplace storage policy: {0}.", mRestore.Setting.StorageDeviceDto.Id);
                }


            }
            request.IsEndUserRequest = mRestore.IsOpusArchivedDownloadJob;
            request.IsRecenterExport = mRestore.IsRecenterExport;
            //request.EndUserRequestItems = plan.SOPlanExtension == null ? null : plan.SOPlanExtension.EndUserRequestItems;
            //var endUserRestoreSettings = GetEndUserRestoreSetting();
            //request.IsEndUserRestoreAccessTier = endUserRestoreSettings == null ? false : endUserRestoreSettings.IsRestoreArchivedTier;
            //request.EndUserRestoreToFSStorageString = plan.SOPlanExtension.RestoreStorageString;
            //request.IntegrationModule = plan.SOPlanExtension.IntegrationModule;
            request.IsSearchAllRestore = mRestore.IsSearchAllRestore;
            if (mRestore.IsSearchAllRestore && mRestore.Setting.RestoreVersionOption == RestoreDocumentVersionsOption.SpecifyVersions
                && mRestore.Setting.KeepVersionsNumber == 1)
            {
                mRestore.Setting.SerchContract.FilterPolicy.SkipDocVersion = true;
            }
            request.SearchContract = mRestore.Setting.SerchContract;
            return request;
        }

        private void LogInfoForSelectAllRestore(ArchiverRestoreFilter searchCondition)
        {
            mLog.Info("Select all search result restore");
            mLog.Debug($"ObjectLevel: {searchCondition.Level}\n" +
                $"FilterName: {searchCondition.FilterName}\n" +
                $"ArchivedTime: {searchCondition.ArchivedStartTime} - {searchCondition.ArchivedEndTime}\n" +
                $"CreatedTime: {searchCondition.CreateStartTime} - {searchCondition.ArchivedEndTime}\n" +
                $"ModifiedTime: {searchCondition.ModifiedStartTime} - {searchCondition.ModifiedEndTime}\n" +
                $"DeleteType: {searchCondition.FilterDeleteType}\n" +
                $"MainJobId: {searchCondition.MainJobId}");
        }

        public List<RestoreSecurityInfoWrapper> GetRestoreSecurityInfoList(List<ArchiverSiteMasterIndexContract> indexes)
        {
            List<RestoreSecurityInfoWrapper> restoreSecurityInfos = new List<RestoreSecurityInfoWrapper>();
            indexes.ForEach(index => index.SubInfo.ForEach(item =>
            {
                if (item.DataEncryptionInfo != null)
                {
                    var wrapper = new RestoreSecurityInfoWrapper() { BackupJobId = item.JobId };
                    string key = item.DataEncryptionInfo.ProfileGuid + item.DataEncryptionInfo.ProtectionGuid;

                    wrapper.SecurityInfo = InternalUnWrapperInfoForRestoreJob(item.DataEncryptionInfo);
                    mLog.Debug("jobId is {0}, Whether find the job encrypted key or not : {1}.", item.JobId, wrapper.SecurityInfo != null);
                    if (wrapper.SecurityInfo != null)
                    {
                        restoreSecurityInfos.Add(wrapper);
                    }
                }
            }));
            return restoreSecurityInfos;
        }
        private DataEncryptionInfo AssemblyDataEncryptionInfo(DataEncryptionProfile profile)
        {
            if (profile.CurrentProtectionAlgorithm != null)
            {
                DataEncryptionInfo info = new DataEncryptionInfo();
                //info.Checksum = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1).ComputeHash(result);
                info.EncryptionType = profile.CurrentProtectionAlgorithm.AlgorithmType;
                info.ProfileGuid = profile.Guid;
                //info.PromptMessage = profile.PromptMessage;
                //info.ProtectionAlgorithmType = profile.CurrentProtectionAlgorithm.Type;
                info.ProtectionGuid = profile.CurrentProtectionAlgorithm.Guid;
                info.ProfileName = profile.Name;
                return info;
            }
            return null;
        }
        private DataEncryptionInfoWrapper InternalUnWrapperInfoForRestoreJob(DataEncryptionInfo info, bool forDA = false)
        {
            if (info != null)
            {
                // 三种情况
                //1.只有一个profileGuid
                //var encryptionInfo = SettingProfileDao.Load();
                var encryptionInfo = SettingProfileDao.LoadById(new Guid(info.ProfileGuid));
                if (encryptionInfo == null)
                {
                    mLog.Error("encryptionInfo not exit");
                    throw new Exception();
                }
                DataEncryptionProfile profile = SerializerHelper.DeserializeByDataContractSerializer<DataEncryptionProfile>(encryptionInfo.Settings);
                if (string.IsNullOrEmpty(info.ProtectionGuid))
                {
                    info.ProtectionGuid = info.ProfileGuid;
                }

                string protectionGuid = string.IsNullOrEmpty(info.ProtectionGuid) ? info.ProfileGuid : info.ProtectionGuid;

                //DataEncryptionProfile profile = DataProfileManager.GetDataEncryptionProfileById(info.ProfileGuid, protectionGuid);
                if (profile != null)
                {
                    if (profile.CurrentProtectionAlgorithm != null && profile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.TenantMasterKeyEncryptionService)
                    {
                        DataEncryptionInfoWrapper wrapper = new DataEncryptionInfoWrapper();
                        DataEncryptionInfo infoFromDB = AssemblyDataEncryptionInfo(profile);
                        infoFromDB.EncryptedDynamicKey = info.EncryptedDynamicKey;
                        infoFromDB.ProfileGuid = info.ProfileGuid;
                        infoFromDB.ProtectionGuid = info.ProtectionGuid;
                        if (infoFromDB != null)
                        {
                            wrapper.EncryptionInfo = infoFromDB;

                            try
                            {
                                wrapper.DynamicKey = TryUnWraperDynamicKey(infoFromDB, profile);
                            }
                            catch (Exception e)
                            {
                                mLog.Error(e.Message, e);
                            }
                            return wrapper;
                        }
                    }
                    else
                    {
                        //2.没有EncryptedDynamickey 说明是老数据，只要返回当前Security profile 内部的key值就可以了
                        if (info.EncryptedDynamicKey == null || info.EncryptedDynamicKey.Length == 0)
                        {
                            return ConvertToWrapper(profile);
                        }
                        else
                        {
                            //3.所有数据都有，需要解密并返回值
                            DataEncryptionInfoWrapper wrapper = new DataEncryptionInfoWrapper();
                            DataEncryptionInfo infoFromDB = AssemblyDataEncryptionInfo(profile);
                            infoFromDB.EncryptedDynamicKey = info.EncryptedDynamicKey;
                            infoFromDB.ProfileGuid = info.ProfileGuid;
                            infoFromDB.ProtectionGuid = info.ProtectionGuid;
                            if (infoFromDB != null)
                            {
                                wrapper.EncryptionInfo = infoFromDB;

                                try
                                {
                                    if (forDA)
                                    {
                                        wrapper.DynamicKey = TryUnWraperDynamicKeyForDA(infoFromDB, profile);
                                    }
                                    else
                                    {
                                        wrapper.DynamicKey = TryUnWraperDynamicKey(infoFromDB, profile);
                                    }
                                }
                                catch (Exception e)
                                {
                                    mLog.Error(e.Message, e);
                                }
                                return wrapper;
                            }
                        }
                    }
                }

            }
            return null;
        }
        private DataEncryptionInfoWrapper ConvertToWrapper(DataEncryptionProfile profile)
        {
            if (profile != null)
            {
                var wrapper = AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement.DataEncryptionUtil.CreateDataEncryptionInfoWrapper(profile);
                if (wrapper != null)
                {
                    DataEncryptionInfoWrapper result = new DataEncryptionInfoWrapper();
                    result.DynamicKey = wrapper.DynamicKey;
                    result.EncryptionInfo = wrapper.EncryptionInfo;
                    return result;
                }
            }
            return null;
        }
        private string TryUnWraperDynamicKeyForDA(DataEncryptionInfo info, DataEncryptionProfile profile)
        {
            if (profile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.KeyVault)
            {
                var aosProfile = profile.CurrentProtectionAlgorithm.AOSProfile ?? PortalUtil.GetSecurityProfileById(profile.CurrentProtectionAlgorithm.AosSecurityProfileId);
                var provider = new KeyVaultServiceProvider(aosProfile);


                return AvePoint.GCommon.Utility.Cryptography.CspCommunicationWrapper.WrapKeyToBase64StringByDefault(provider.DecryptBinary(info.EncryptedDynamicKey));
            }
            else if (profile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.TenantMasterKeyEncryptionService)
            {
                return Convert.ToBase64String(AesEncryptorWrapper.Decrypt(info.EncryptedDynamicKey));
            }
            else
            {
                IEncryption encryption = EncryptionFactory.GetEncryption(EncryptionAlgorithm.AES_ENCRYPTION, profile.CurrentProtectionAlgorithm.ProtectionKey);
                return AvePoint.GCommon.Utility.Cryptography.CspCommunicationWrapper.WrapKeyToBase64StringByDefault(encryption.DecryptBinary(info.EncryptedDynamicKey));
            }
        }

        private string TryUnWraperDynamicKey(DataEncryptionInfo info, DataEncryptionProfile profile)
        {
            if (profile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.KeyVault)
            {
                var aosProfile = profile.CurrentProtectionAlgorithm.AOSProfile ?? PortalUtil.GetSecurityProfileById(profile.CurrentProtectionAlgorithm.AosSecurityProfileId);
                var provider = new KeyVaultServiceProvider(aosProfile);
                return AvePoint.GCommon.Utility.Cryptography.CspCommunicationWrapper.WrapKeyToBase64String(provider.DecryptBinary(info.EncryptedDynamicKey));
            }
            else if (profile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.TenantMasterKeyEncryptionService)
            {
                return AvePoint.GCommon.Utility.Cryptography.CspCommunicationWrapper.WrapKeyToBase64String(AesEncryptorWrapper.Decrypt(info.EncryptedDynamicKey));
            }
            else
            {
                IEncryption encryption = EncryptionFactory.GetEncryption(EncryptionAlgorithm.AES_ENCRYPTION, profile.CurrentProtectionAlgorithm.ProtectionKey);
                return AvePoint.GCommon.Utility.Cryptography.CspCommunicationWrapper.WrapKeyToBase64String(encryption.DecryptBinary(info.EncryptedDynamicKey));
            }
        }

        private string GeneratePassword(int intLength, bool booNumber, bool booSign, bool booSmallword, bool booBigword)
        {
            //定义
            int intResultRound = 0;
            string strB = "";
            while (intResultRound < intLength)
            {
                //生成随机数A，表示生成类型
                //1=数字，2=符号，3=小写字母，4=大写字母
                int intA = SecurityUtils.GetRandomNumber(1, 5);
                //如果随机数A=1，则运行生成数字
                //生成随机数A，范围在0-10
                //把随机数A，转成字符
                //生成完，位数+1，字符串累加，结束本次循环
                if (intA == 1 && booNumber)
                {
                    intA = SecurityUtils.GetRandomNumber(0, 10);
                    strB = intA.ToString(CultureInfo.InvariantCulture) + strB;
                    intResultRound = intResultRound + 1;
                    continue;
                }
                //如果随机数A=2，则运行生成符号
                //生成随机数A，表示生成值域
                //1：33-47值域，2：58-64值域，3：91-96值域，4：123-126值域
                if (intA == 2 && booSign)
                {
                    intA = SecurityUtils.GetRandomNumber(1, 5);

                    //如果A=1
                    //生成随机数A，33-47的Ascii码
                    //把随机数A，转成字符
                    //生成完，位数+1，字符串累加，结束本次循环
                    if (intA == 1)
                    {
                        intA = SecurityUtils.GetRandomNumber(33, 48);
                        strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }

                    //如果A=2
                    //生成随机数A，58-64的Ascii码
                    //把随机数A，转成字符
                    //生成完，位数+1，字符串累加，结束本次循环
                    if (intA == 2)
                    {
                        intA = SecurityUtils.GetRandomNumber(58, 65);
                        strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }

                    //如果A=3
                    //生成随机数A，91-96的Ascii码
                    //把随机数A，转成字符
                    //生成完，位数+1，字符串累加，结束本次循环
                    if (intA == 3)
                    {
                        intA = SecurityUtils.GetRandomNumber(91, 97);
                        strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }

                    //如果A=4
                    //生成随机数A，123-126的Ascii码
                    //把随机数A，转成字符
                    //生成完，位数+1，字符串累加，结束本次循环
                    if (intA == 4)
                    {
                        intA = SecurityUtils.GetRandomNumber(123, 127);
                        strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                        intResultRound = intResultRound + 1;
                        continue;
                    }
                }
                //如果随机数A=3，则运行生成小写字母
                //生成随机数A，范围在97-122
                //把随机数A，转成字符
                //生成完，位数+1，字符串累加，结束本次循环
                if (intA == 3 && booSmallword)
                {
                    intA = SecurityUtils.GetRandomNumber(97, 123);
                    strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                    intResultRound = intResultRound + 1;
                    continue;
                }

                //如果随机数A=4，则运行生成大写字母
                //生成随机数A，范围在65-90
                //把随机数A，转成字符
                //生成完，位数+1，字符串累加，结束本次循环
                if (intA == 4 && booBigword)
                {
                    intA = SecurityUtils.GetRandomNumber(65, 89);
                    strB = ((char)intA).ToString(CultureInfo.InvariantCulture) + strB;
                    intResultRound = intResultRound + 1;
                }
            }
            return strB;
        }

        private PhysicalDeviceDto ConvertStorageDeviceDtoToPhysicalDeviceDto(StorageDeviceDto storageDevice)
        {
            var physical = new PhysicalDeviceDto()
            {
                Id = storageDevice.Id,
                ConnectionString = storageDevice.ConnectionString,
                ModifyTime = storageDevice.ModifyTime,
                Type = storageDevice.Type,
            };
            return physical;
        }

        private HashSet<string> GetAllStorageLogicalDevices(List<ArchiverSiteMasterIndexContract> indexes)
        {
            HashSet<string> logicalDeviceIdList = new HashSet<string>();
            foreach (var index in indexes)
            {
                foreach (var subInfo in index.SubInfo)
                {
                    logicalDeviceIdList.Add(string.IsNullOrEmpty(subInfo.CurrentStorageId) ? subInfo.StorageInfo : subInfo.CurrentStorageId);
                }
            }
            return logicalDeviceIdList;
        }

        private ArchiverSiteMasterIndexContract GetIndexInfo(SPTreeNodeDto node, ArchiverSiteMasterIndexContract index)
        {
            //if (node.Level == NodeLevel.Farm)
            //{
            //    index.FarmId = node.FarmID;
            //    index.FarmName = node.FarmName;
            //}
            //else if (node.Level == NodeLevel.WebApplication && HasSelectedNode(node))
            //{
            //    index.WebId = node.SPObjectId;
            //    index.WebURL = node.Name;
            //}
            //else if (node.Level == NodeLevel.SiteCollection && HasSelectedNode(node))
            //{
            index.SiteId = node.SPObjectId;
            index.SiteURL = node.SitePath;
            //}
            //if (node.Children != null)
            //{
            //    foreach (SPTreeNodeDto child in node.Children)
            //    {
            //        this.GetIndexInfo(child, index);
            //    }
            //}
            return index;
        }
    }
}
