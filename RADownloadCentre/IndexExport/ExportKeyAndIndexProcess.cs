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
using AvePoint.Common.Portal;
using AvePoint.Cryptography;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography.Encryption.KeyVault;
using AvePoint.Media.Common;
using AvePoint.Media.Core.Index;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.RACommonUtility.Encryption;
using AvePoint.RA.SharePoint.Common;
using DocumentFormat.OpenXml.Drawing;
using RADownloadCentre.IndexExport;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace RADownloadCenter.IndexExport
{
    public class ExportKeyAndIndexProcess
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(ExportKeyAndIndexProcess));
        private readonly IDownloadDataInfoDao DownloadDataInfoDao = PlatformWindsorManager.GetService<IDownloadDataInfoDao>();

        private string _jobId = string.Empty;

        private JobType _jobType;

        private readonly string _customPassword;

        private FileInfo? fileInfo;
        private string _folderPath;
        private string _exportDBName = "Export.db";
        private string _indexDeviceName = "UsingIndexDevice";
        private string _storageAveId = "6a040c17-af8a-4f1f-96c1-7ceb2e23b1f3";

        private IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        private ISettingProfilesDao SettingProfileDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();
        private IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao => PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private IIndexDatabaseScriptGenerator scriptGenerator = MediaServiceLocator.Discover<IIndexDatabaseScriptGenerator>();
        private ICommonSiteMasterIndexDao CommonSiteMasterIndexDao => PlatformWindsorManager.GetService<ICommonSiteMasterIndexDao>();

        private RMAesEncryptorWrapper AesEncryptorWrapper => new();
        private readonly RMRetryer _retryer = RMRetryerBuilder.CreateBuilder().Build();

        private RMAesEncryptorWrapper CustomAesEncryptorWrapper;

        private Dictionary<string, SettingProfiles> _encryptionInfoCache = new Dictionary<string, SettingProfiles>();

        public ExportKeyAndIndexProcess(string jobId, JobType jobType, string customPassword)
        {
            _jobId = jobId;
            _jobType = jobType;
            _customPassword = customPassword;
            Initialize();
        }

        private void Initialize()
        {
            CustomAesEncryptorWrapper = new(GenerateAesKey());
            GenerateAndUploadFileManager.Init(_jobId, _jobType);
            _folderPath = JobReportUtility.GetDownloadReportDetailTempleFolder(new BaseJobDto()
            {
                JobType = (int)_jobType,
                Id = _jobId,
            });
            if (!Directory.Exists(_folderPath))
            {
                Directory.CreateDirectory(_folderPath);
            }
        }

        public async Task RunAsync()
        {
            var reportProfile = DownloadDataInfoDao.GetDownloadDataInfosByStatus(new List<int>() { (int)DownloadContentJobStatus.Wait })
                                   .FirstOrDefault(item => item.JobId == _jobId);
            try
            {
                if (reportProfile == null)
                {
                    GenerateAndUploadFileManager.HasFailed = true;
                    _logger.Error($"Can not find report download info!");
                    return;
                }
                reportProfile.JobStatus = (int)DownloadContentJobStatus.InProgress;

                await DownloadDataInfoDao.UpdateAsync(reportProfile);
                await GenerateExportDB();
                await UploadBlobAsync();
                if (fileInfo != null)
                {
                    reportProfile.FileSize = fileInfo.Length;
                }

                reportProfile.BlobSasUri = await DownloadCenterUtility.GenerateSasUri();
                _logger.Info("Upload blob success!");


                reportProfile.JobStatus = (int)DownloadContentJobStatus.Finished;

                DownloadDataInfoDao.UpdateDownloadInfo(reportProfile);
            }
            catch (Exception e)
            {
                reportProfile!.JobStatus = (int)DownloadContentJobStatus.Failed;
                await DownloadDataInfoDao.UpdateAsync(reportProfile);
                GenerateAndUploadFileManager.HasFailed = true;
                GenerateAndUploadFileManager.JobComment = e.Message;
                _logger.Error($"Generate And Upload File failed! Error : {e}");
            }
            finally
            {
                GenerateAndUploadFileManager.SendJobDetail();
                GenerateAndUploadFileManager.SetJobFinished();
                if (GenerateAndUploadFileManager.HasFailed && !GenerateAndUploadFileManager.HasSucceed)
                {
                    if (reportProfile != null)
                    {
                        DownloadDataInfoDao.BatchDelete(new List<RMDownloadDataInfo> { reportProfile });
                    }
                }
            }
        }

        private async Task GenerateExportDB()
        {
            var settingProfiles = LoadSettingProfiles();
            var siteMasterIndexs = LoadAllSiteMasterIndexs();
            var indexSubInfoes = LoadAllIndexSubInfoes(out var storageIds);
            var commonSiteMasterIndexInfoes = LoadAllCommonSiteMasterIndex();

            var subJobSourceFlagMapping = indexSubInfoes.GroupBy(k => k.JobId[..k.JobId.LastIndexOf('_')], v => v.SourceFlag).ToDictionary(g => g.Key, g => g.First());

            storageIds.Add(settingProfiles[_indexDeviceName]);
            var storageDevices = LoadDeviceInfos(storageIds);

            //Insert data to sql lite
            var dbHelper = new IndexDatabaseHelper();
            try
            {
                //dbHelper.Open(string.Format("Data Source = {0}", SecurityUtils.SafeCombinePath(_folderPath, _exportDBName)));
                dbHelper.Open(SecurityUtils.SafeCombinePath(_folderPath, _exportDBName), _customPassword);
                //create table for export db
                var initialDatabaseScript = this.scriptGenerator.GenerateInitialScript("ExportIndexProcessorParameter");
                dbHelper.ExecuteNonQuery(initialDatabaseScript, default(Dictionary<String, Object>));
                //export table
                dbHelper.ExecuteNonQuery(ConvertExportDto.ConvertSettingProfiles(settingProfiles));
                dbHelper.ExecuteNonQuery(ConvertExportDto.ConvertSiteMasterIndexes(siteMasterIndexs, subJobSourceFlagMapping));
                dbHelper.ExecuteNonQuery(ConvertExportDto.ConvertIndexSubInfoes(indexSubInfoes));
                dbHelper.ExecuteNonQuery(ConvertExportDto.ConvertStorageDeviceInfoes(storageDevices));
                dbHelper.ExecuteNonQuery(ConvertExportDto.ConvertCommonSiteMasterIndex(commonSiteMasterIndexInfoes));
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred when export table ,error:{e}");
                throw;
            }
            finally
            {
                dbHelper.Close();
            }
        }

        private async Task UploadBlobAsync()
        {
            AvePoint.GCommon.ZipUtil.ZipFolder(_folderPath, _folderPath + ".zip", _customPassword, Encoding.UTF8);
            var customId = TenantLocalValue.LogonGroupId;
            var blobName = SecurityUtils.SafeCombinePath(customId, _jobId + ".zip");//Path.Combine(customId, JobId + ".zip");
            try
            {
                await _retryer.RetryAsync(() =>
                {
                    blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, _folderPath + ".zip");
                    _logger.Info($"Upload key and index export success");
                    return Task.CompletedTask;
                });
            }
            catch (Exception e)
            {
                _logger.Error($"Upload key and index export failed,error is :{e}");
                throw;
            }

            _logger.Info($"finish to upload blob name:{blobName}");
            fileInfo = new FileInfo(_folderPath + ".zip");
        }

        private Dictionary<string, string> LoadSettingProfiles()
        {
            Dictionary<string, string> settingProfiles = new();

            try
            {
                string? key = SettingProfileDao.LoadByType(SettingProfilesType.DBSEEMasterKey)?.Settings;
                if (string.IsNullOrEmpty(key))
                {
                    key = SettingProfileDao.GetDBSEEMasterKey(AesEncryptorWrapper.Encrypt(string.Format("aes256:{0}", new NetworkCredential("", KeyGenerateProviderFactory.CreateProvider().GenerateVisibleKeyString(20)).Password)));
                }
                var reEncryptedDBSEEMasterKey = CustomAesEncryptorWrapper.Encrypt(AesEncryptorWrapper.Decrypt(key));
                settingProfiles[SettingProfilesType.DBSEEMasterKey.ToString()] = reEncryptedDBSEEMasterKey;

                var indexDto = new SettingProfileDto()
                {
                    Type = (int)SettingProfilesType.IndexDevice,
                    Name = _indexDeviceName
                };
                var indexSetting = SettingProfileDao.Load(indexDto);
                if (indexSetting != null)
                {
                    settingProfiles[indexSetting.Name] = indexSetting.Settings;
                }
                else
                {
                    _logger.Warn($"UsingIndexDevive not found in SettingProfile");
                    throw new Exception("IndexDevive not found");
                }

                return settingProfiles;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred when loading Setting Profiles ,error:{e}");
                throw;
            }
        }

        private List<ArchiverSiteMasterIndex> LoadAllSiteMasterIndexs()
        {
            return ArchiverSiteMasterIndexDao.GetAllSiteMastersInfo();
        }

        private List<AvePoint.RA.DB.Model.CommonSiteMasterIndex> LoadAllCommonSiteMasterIndex()
        {
            return CommonSiteMasterIndexDao.GetAllCommonSiteMasterIndexes();
        }

        private List<ArchiverIndexSubInfoContract> LoadAllIndexSubInfoes(out HashSet<string> storageIds)
        {
            List<ArchiverIndexSubInfoContract> result = new List<ArchiverIndexSubInfoContract>();
            storageIds = new HashSet<string>();
            try
            {
                var SubInfos = ArchiverIndexSubInfoDao.GetAllSubInfos();
                if (SubInfos != null && SubInfos.Count > 0)
                {
                    foreach (ArchiverIndexSubInfo domain in SubInfos)
                    {
                        var subInfo = ConvertToDto(domain);                        
                        subInfo.DataEncryptionType = subInfo.DataEncryptionInfo == null ? (int)EncryptionAlgorithm.BLOWFISH_ENCRYPTION : subInfo.DataEncryptionInfo.EncryptionType;
                        subInfo.DataEncryptionDynamicKey = subInfo.DataEncryptionInfo == null ? null : ReEncryptDataEncryptionInfo(subInfo.DataEncryptionInfo);
                        //subInfo.ArchiverSubInfoExtension = null;
                        //subInfo.DataEncryptionInfo = null;

                        result.Add(subInfo);
                        storageIds.Add(domain.CurrentStorageId);
                        if (domain.CurrentStorageId != domain.StorageId)
                        {
                            storageIds.Add(domain.StorageId);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred when loading all indexSubInfoes ,error:{e}");
                throw;
            }
            return result;
        }

        private List<StorageDeviceDto> LoadDeviceInfos(HashSet<string> storageIds)
        {
            try
            {
                List<StorageDeviceDto> storageDevices = new List<StorageDeviceDto>();
                storageIds.ForEach(s => {
                    var storage = StorageDeviceService.GetStorageDeviceById(s);
                    if (storage != null && !storage.Id.Equals(_storageAveId, StringComparison.OrdinalIgnoreCase) && !storage.IsSystemStorage)
                    {
                        storage.ConnectionString = CustomAesEncryptorWrapper.Encrypt(storage.ConnectionString);
                        storageDevices.Add(storage);
                    }
                });
                return storageDevices;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred when loading device infos ,error:{e}");
                throw;
            }
        }

        private ArchiverIndexSubInfoContract ConvertToDto(ArchiverIndexSubInfo domain)
        {
            if (domain == null)
            {
                return null;
            }
            ArchiverIndexSubInfoContract info = new ArchiverIndexSubInfoContract();
            info.Id = domain.Id;
            info.JobId = domain.SubSubJobId;
            info.RetentionTime = domain.RetentionTime;
            info.RetentionTimeSpanSeconds = domain.KeepTime;
            info.StorageInfo = domain.StorageId;
            info.CurrentStorageId = domain.CurrentStorageId;
            info.MediaDataSize = domain.MediaDataSize;
            info.AgentDataSize = domain.AgentDataSize;
            info.SourceFlag = domain.SourceFlag;
            info.DataFlag = domain.DataFlag;
            info.SubJobId = domain.SubJobId;
            if (domain.Extension != null && domain.Extension != string.Empty)
            {
                info.ArchiverSubInfoExtension = SerializerHelper.DeserializeByDataContractSerializer<ArchiverSubInfoExtension>(domain.Extension);
                if (info.ArchiverSubInfoExtension != null)
                {
                    info.DataEncryptionInfo = info.ArchiverSubInfoExtension.DataEncryptionInfo;
                }
            }
            return info;
        }

        private byte[] ReEncryptDataEncryptionInfo(DataEncryptionInfo info, bool forDA = false)
        {
            if (info != null)
            {
                if (!_encryptionInfoCache.TryGetValue(info.ProfileGuid, out var encryptionInfo))
                {
                    encryptionInfo = SettingProfileDao.LoadById(new Guid(info.ProfileGuid));
                    if (encryptionInfo == null)
                    {
                        _logger.Error("encryptionInfo not exist");
                        throw new ArgumentNullException("encryptionInfo");
                    }
                    _encryptionInfoCache[info.ProfileGuid] = encryptionInfo;
                }

                DataEncryptionProfile profile = SerializerHelper.DeserializeByDataContractSerializer<DataEncryptionProfile>(encryptionInfo.Settings);
                if (string.IsNullOrEmpty(info.ProtectionGuid))
                {
                    info.ProtectionGuid = info.ProfileGuid;
                }

                if (profile != null)
                {
                    if (profile.CurrentProtectionAlgorithm != null && profile.CurrentProtectionAlgorithm.Type == ProtectionAlgorithmType.TenantMasterKeyEncryptionService)
                    {
                        DataEncryptionInfo infoFromDB = AssemblyDataEncryptionInfo(profile);
                        infoFromDB.EncryptedDynamicKey = info.EncryptedDynamicKey;
                        infoFromDB.ProfileGuid = info.ProfileGuid;
                        infoFromDB.ProtectionGuid = info.ProtectionGuid;
                        if (infoFromDB != null)
                        {
                            try
                            {
                                var planText = TryUnWraperDynamicKey(infoFromDB, profile);
                                return CustomAesEncryptorWrapper.Encrypt(Encoding.UTF8.GetBytes(planText));
                            }
                            catch (Exception e)
                            {
                                _logger.Error(e.Message, e);
                            }
                        }
                    }
                    else
                    {
                        //2.没有EncryptedDynamickey 说明是老数据，只要返回当前Security profile 内部的key值就可以了
                        if (info.EncryptedDynamicKey != null && info.EncryptedDynamicKey.Length != 0)
                        {
                            //3.所有数据都有，需要解密并返回值
                            DataEncryptionInfo infoFromDB = AssemblyDataEncryptionInfo(profile);
                            infoFromDB.EncryptedDynamicKey = info.EncryptedDynamicKey;
                            infoFromDB.ProfileGuid = info.ProfileGuid;
                            infoFromDB.ProtectionGuid = info.ProtectionGuid;
                            if (infoFromDB != null)
                            {
                                try
                                {
                                    var planText = string.Empty;

                                    if (forDA)
                                    {
                                        planText = TryUnWraperDynamicKeyForDA(infoFromDB, profile);
                                        return CustomAesEncryptorWrapper.Encrypt(Encoding.UTF8.GetBytes(planText));
                                    }
                                    else
                                    {
                                        planText = TryUnWraperDynamicKey(infoFromDB, profile);
                                        return CustomAesEncryptorWrapper.Encrypt(Encoding.UTF8.GetBytes(planText));
                                    }
                                }
                                catch (Exception e)
                                {
                                    _logger.Error(e.Message, e);
                                }
                            }
                        }
                    }
                }
            }
            return null;
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

        private byte[] GenerateAesKey()
        {
            try
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    return sha256.ComputeHash(Encoding.UTF8.GetBytes(_customPassword));
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred when generating AesKey [{_customPassword}] ,error:{e}");
                throw;
            }
        }
    }
}
