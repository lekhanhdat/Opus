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
using AvePoint.Archiver.Media;
using AvePoint.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using Media.Service.ArchiverBackup.Index;
using RAArchiverCommon;
using Storage;

namespace RAArchiverMaintenance.Deduplication
{
    public class ArchiverDedupIndexDBUpgrader
    {
        private IRALogger logger = new RALogger(typeof(ArchiverDedupIndexDBUpgrader));

        private IXSystem indexLogicalDevice;
        private IVolumeGenerator volumeGenerator = new VolumeGeneratorFactory().GetVolumeGenerator(ProductModule.ArchiverBackup);
        private string indexVolume;
        private CacheSettingDto cacheSetting;
        private HashSet<string> changedSubIndexes = new HashSet<string>();

        private IIndexDatabaseSynchronizer IndexSynchronizer = PlatformWindsorManager.GetService<IIndexDatabaseSynchronizer>();
        //主Index IIndexProcessor,用来操作本地Download的Index
        public IIndexProcessor<ArchiverIndexProcessorParameter> IndexMainProcessor;
        //临时Index，用来存储要De-dup的数据,IIndexProcessor,用来操作本地Download的Index
        public IIndexProcessor<ArchiverDedupIndexProcessorParameter> DedupIndexProcessor;

        private Dictionary<string, IIndexProcessor<ArchiverIndexProcessorParameter>> SubIndexProcessors = new Dictionary<string, IIndexProcessor<ArchiverIndexProcessorParameter>>(); // key is archiver sub job id


        private ICacheService CacheManager = PlatformWindsorManager.GetService<ICacheService>();
        private IStorageDeviceManager StorageDeviceManager = PlatformWindsorManager.GetService<IStorageDeviceManager>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();

        public ArchiverDedupIndexDBUpgrader()
        {
            Init();
        }

        private void Init()
        {
            MediaConfigInfo.CommonConfigInfo = MediaServiceFactory.CreateCommonConfigInfo();
            MediaConfigInfo.ArchiverConfigInfo = MediaServiceFactory.CreateArchiverConfigInfo();
            logger.Info($"Begin opening IndexLogicalDevice.");
            var indexStroage = StorageDeviceService.GetIndexDeviceForMigrationJob();
            if (indexStroage == null)
            {
                throw new Exception("Cannot find index Storage Device.");
            }
            this.cacheSetting = GetCacheSetting();
            var indexLogicalDeviceDto = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(indexStroage);
            this.indexLogicalDevice = this.StorageDeviceManager.Open(indexLogicalDeviceDto.GetXRIS(PhysicalDeviceUsage.Index));
            this.CacheManager.Open(this.cacheSetting, false, true);
            logger.Info($"Opened indexLogicalDevice successfully.");

        }
        
        public bool Upgrade(string siteCollectionUrl)
        {
            bool success = true;
            logger.Error($"Upgrade index for {siteCollectionUrl}");
            try
            {
                this.indexVolume = this.volumeGenerator.GenerateIndexVolume(new VolumeParameter() { SiteCollectionUrl = siteCollectionUrl, FarmName = "" });
                this.OpenMainIndex();
                this.OpenDedupFileIndex();

                RealUpgrade();
            }
            catch (Exception ex)
            {
                logger.Error($"Upgrade failed for {siteCollectionUrl}, {ex}");
                success = false;
            }
            finally
            {
                changedSubIndexes.Clear();
                CloseIndexProcessors();
            }
            
            return success;
        }

        private void RealUpgrade()
        {
            var duplicateFiles = QueryAllDuplicateFileIndexes();
            logger.Info($"Total duplicate files {duplicateFiles.Count}");

            List<ArchiverBodyIndex> changedFiles = new List<ArchiverBodyIndex>();
            foreach (var item in duplicateFiles)
            {
                var dedupExtInfo = GetDedupExtensionInfo(item);
                if(dedupExtInfo == null)
                {
                    logger.Warn($"No dedup extension info: {item.Id}");
                    continue;
                }
                if (!string.IsNullOrEmpty(dedupExtInfo.DedupSourceFileJobId))
                {
                    logger.Warn($"Already upgraded: {item.Id}");
                    continue;
                }

                dedupExtInfo.DedupSourceFileJobId = item.CycleId;
                dedupExtInfo.DedupSourceFileFlag = item.PruneTime;
                dedupExtInfo.DuplicateFileNumber = item.ContentDataFilePrefixNumber;
                dedupExtInfo.DuplicateFileStorageInfo = item.SubRetention;

                item.CycleId = string.Empty;
                item.PruneTime = 0;
                item.ContentDataFilePrefixNumber = 0;
                item.SubRetention = string.Empty;
                item.DedupExtension = SerializerHelper.SerializeByDataContractJsonSerializer(dedupExtInfo);

                UpdateFileDedupInfoToMainIndexDB(IndexMainProcessor, item);
                UpdateFileInDedupIndexDB(item);

                changedFiles.Add(item);
            }

            if(changedFiles.Count > 0)
            {
                UpdateDuplicateFilesInSubIndexDB(changedFiles);

                UploadChangedSubIndexToAzure();

                UploadMainIndexToAzure();

                UploadDedupIndexToAzure();
            }
        }

        private void UpdateDuplicateFilesInSubIndexDB(List<ArchiverBodyIndex> indexes)
        {
            foreach (var group in indexes.GroupBy(i => i.JobId))
            {
                var archiverSubJobId = group.Key;
                this.changedSubIndexes.Add(archiverSubJobId);

                logger.Info($"Update duplicate files in sub index db. ArchiverSubJobId: {archiverSubJobId}. IDs: {string.Join(",", indexes.Select(i => i.Id))}");
                var subIndexProcessor = GetSubIndexProcessor(archiverSubJobId);
                foreach (var dupFileInfo in group)
                {
                    UpdateFileDedupInfoToSubIndexDB(subIndexProcessor, dupFileInfo);
                }
            }
        }

        private DedupExtensionInfo? GetDedupExtensionInfo(ArchiverBodyIndex item)
        {
            if (string.IsNullOrEmpty(item.DedupExtension))
            {
                return null;
            }
            try
            {
                return SerializerHelper.DeserializeByDataContractJsonSerializer<DedupExtensionInfo>(item.DedupExtension);
            }
            catch (Exception ex)
            {
                logger.Error($"Deserialize dedup extension failed: {ex}");
            }
            return null;
        }

        private List<ArchiverBodyIndex> QueryAllDuplicateFileIndexes()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("QueryAllDuplicateFileIndexes"))
            {
                string sql = $"SELECT * FROM {IndexConstants.TableNameArchiveBody} WHERE COL_EXTENSION_3 > 0";

                return this.IndexMainProcessor.ExecuteQuery<ArchiverBodyIndex>(
                    sql,
                    new Dictionary<string, object>());
            }
        }

        private CacheSettingDto GetCacheSetting()
        {
            var archiveTemp = BackgroundSettings.GetInstance().ArchiveTemp;
            if (!System.IO.Directory.Exists(archiveTemp))
            {
                System.IO.Directory.CreateDirectory(archiveTemp);
            }

            CacheSettingDto cache = new CacheSettingDto()
            {
                Extension = new CacheSettingExtension()
                {
                    Path = new List<PathMap>() {
                        new PathMap() {
                            DiskInfo = new DiskInfoDto() {
                                Path = archiveTemp
                            }
                        }
                    }
                }
            };
            return cache;
        }

        private void OpenMainIndex()
        {
            IndexMainProcessor = new IndexProcessor<ArchiverIndexProcessorParameter>();
            using (AvePerformanceScope pc = new AvePerformanceScope("OpenMainIndex"))
            {
                logger.Info("Begin opening mainindex.");
                var indexServiceOpenParameter = new ArchiverIndexServiceOpenParameter()
                {
                    IndexDatabaseName = ServiceConstants.IndexDBName,
                    //BackupJobId = ,
                    IndexVolume = indexVolume,
                    TreeMode = TreeMode.SiteCollectionMode,
                    IndexLogicalDeviceSystem = this.indexLogicalDevice,
                    IndexCacheDeviceSystem = this.CacheManager.CacheSystem,
                    CacheSetting = this.cacheSetting,
                    //StorageInfo = 
                };
                IndexSynchronizer.Initialize(indexServiceOpenParameter);
                this.InitMainIndexProcessor(indexServiceOpenParameter);
            }
        }

        private void InitMainIndexProcessor(ArchiverIndexServiceOpenParameter openParam)
        {
            var realIndexDevice = this.CacheManager.CacheSystem;
            IndexDatabaseDownLoadResult indexDownLoadInfo;
            var logicalStorageInfo = XConvert.FromNames(openParam.IndexVolume, openParam.IndexDatabaseName, openParam.StorageInfo);
            if (openParam.IndexLogicalDeviceSystem.FileExists(logicalStorageInfo))
            {
                if (MediaConfigInfo.CommonConfigInfo.ForceUseCache)
                {
                    var dbInfo = new IndexDatabaseInfo(openParam);
                    indexDownLoadInfo = this.IndexSynchronizer.Download(dbInfo);
                }
                else
                    indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Cached, Path.Combine(realIndexDevice.SystemLocation, Path.Combine(openParam.IndexVolume, openParam.IndexDatabaseName)));
            }
            else
            {
                indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Nonexistent, Path.Combine(realIndexDevice.SystemLocation, Path.Combine(openParam.IndexVolume, openParam.IndexDatabaseName)));
            }
            realIndexDevice.OpenDirectory(XConvert.FromNames(openParam.IndexVolume, string.Empty), FileMode.Create);
            IdentityManager.IdentityMode = IdentityMode.Process;
            ArchiverIndexProcessorParameter param = new ArchiverIndexProcessorParameter(IdentityManager.IdentityContent)
            {
                DownLoadResult = indexDownLoadInfo,
                IndexWorkingSystem = realIndexDevice,
            };
            if (openParam.IndexDatabaseName.Equals(ServiceConstants.IndexDBName))
            {
                param.IsNeedCheckIntegrity = true;
                this.IndexMainProcessor.Open(param);
            }

            this.logger.Info("Open MainIndex Finished.");
        }

        private void UploadMainIndexToAzure()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Dedup.UploadMainIndexToAzure"))
            {
                logger.Info($"Begin UploadMainIndexToAzure.");
                var mainIndexDBInfo = new IndexDatabaseInfo(ServiceConstants.IndexDBName, null);
                DatabaseUtility.RetryPolicy.ExecuteAction(() =>
                {
                    this.IndexSynchronizer.Upload(mainIndexDBInfo);
                });
                logger.Info($"End UploadMainIndexIndexToAzure.");
            }
        }
        private IIndexProcessor<ArchiverIndexProcessorParameter> GetSubIndexProcessor(string archiverSubJobId)
        {
            if (!SubIndexProcessors.TryGetValue(archiverSubJobId, out var subIndexProcessor))
            {
                subIndexProcessor = OpenSubIndex(archiverSubJobId);
                SubIndexProcessors[archiverSubJobId] = subIndexProcessor;
            }

            return subIndexProcessor;
        }

        private IIndexProcessor<ArchiverIndexProcessorParameter> OpenSubIndex(string archiverSubJobId)
        {
            IIndexProcessor<ArchiverIndexProcessorParameter> subIndexProcessor = null;
            this.logger.Info($"Begin opening SubIndex: {archiverSubJobId}");
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("Dedup.OpenSubIndex"))
                {
                    var indexServiceOpenParameter = new ArchiverIndexServiceOpenParameter()
                    {
                        IndexDatabaseName = $"{archiverSubJobId}_{ServiceConstants.IndexDBName}",
                        //BackupJobId = ,
                        IndexVolume = indexVolume,
                        TreeMode = TreeMode.SiteCollectionMode,
                        IndexLogicalDeviceSystem = this.indexLogicalDevice,
                        IndexCacheDeviceSystem = this.CacheManager.CacheSystem,
                        CacheSetting = this.cacheSetting,
                        //StorageInfo = 
                    };
                    IndexSynchronizer.Initialize(indexServiceOpenParameter);

                    subIndexProcessor = this.InitSubIndexProcessor(indexServiceOpenParameter);
                }

                logger.Info($"Open SubIndex Finished: {archiverSubJobId}");
            }
            catch (Exception ex)
            {
                logger.Error($"Open SubIndex Failed: {archiverSubJobId}. {ex}");
            }
            return subIndexProcessor;
        }

        private IIndexProcessor<ArchiverIndexProcessorParameter> InitSubIndexProcessor(ArchiverIndexServiceOpenParameter openParam)
        {
            var subIndexProcessor = new IndexProcessor<ArchiverIndexProcessorParameter>();
            var realIndexDevice = this.CacheManager.CacheSystem;
            IndexDatabaseDownLoadResult indexDownLoadInfo;
            var logicalStorageInfo = XConvert.FromNames(openParam.IndexVolume, openParam.IndexDatabaseName, openParam.StorageInfo);
            if (openParam.IndexLogicalDeviceSystem.FileExists(logicalStorageInfo))
            {
                if (MediaConfigInfo.CommonConfigInfo.ForceUseCache)
                {
                    var dbInfo = new IndexDatabaseInfo(openParam);
                    indexDownLoadInfo = this.IndexSynchronizer.Download(dbInfo);
                }
                else
                    indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Cached, Path.Combine(realIndexDevice.SystemLocation, Path.Combine(openParam.IndexVolume, openParam.IndexDatabaseName)));
            }
            else
            {
                indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Nonexistent, Path.Combine(realIndexDevice.SystemLocation, Path.Combine(openParam.IndexVolume, openParam.IndexDatabaseName)));
            }
            realIndexDevice.OpenDirectory(XConvert.FromNames(openParam.IndexVolume, string.Empty), FileMode.Create);
            IdentityManager.IdentityMode = IdentityMode.Process;
            ArchiverIndexProcessorParameter param = new ArchiverIndexProcessorParameter(IdentityManager.IdentityContent)
            {
                DownLoadResult = indexDownLoadInfo,
                IndexWorkingSystem = realIndexDevice,
            };
            //param.IsNeedCheckIntegrity = true;
            subIndexProcessor.Open(param);

            return subIndexProcessor;
        }

        private void UploadChangedSubIndexToAzure()
        {
            foreach (var archiverSubJobId in this.changedSubIndexes)
            {
                UploadSubIndexToAzure(archiverSubJobId);
            }
        }

        private void UploadSubIndexToAzure(string archiverSubJobId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Dedup.UploadSubIndexToAzure"))
            {
                var subIdxDbName = $"{archiverSubJobId}_{ServiceConstants.IndexDBName}";
                logger.Info($"Begin UploadSubIndexToAzure: {subIdxDbName}");
                var subIndexDBInfo = new IndexDatabaseInfo(subIdxDbName, null);
                try
                {
                    DatabaseUtility.RetryPolicy.ExecuteAction(() =>
                    {
                        this.IndexSynchronizer.Upload(subIndexDBInfo);
                    });
                    logger.Info($"End UploadSubIndexToAzure: {subIdxDbName}");
                }
                catch (Exception ex)
                {
                    logger.Error($"Error occurred while UploadSubIndexToAzure: {subIdxDbName}. {ex}");
                }
            }
        }

        private void UpdateFileDedupInfoToSubIndexDB(IIndexProcessor<ArchiverIndexProcessorParameter> subIndexProcessor, ArchiverBodyIndex index)
        {
            logger.Info($"Update dedup status to sub index db. Id: {index.Id}.");
            using (AvePerformanceScope pc = new AvePerformanceScope("Dedup.UpdateFileDedupStatusForSubIndexDB"))
            {
                UpdateFileIndexDedupInfo(subIndexProcessor, index);
            }
        }
        private void OpenDedupFileIndex()
        {
            DedupIndexProcessor = new IndexProcessor<ArchiverDedupIndexProcessorParameter>();
            using (AvePerformanceScope pc = new AvePerformanceScope("Dedup.OpenDedupFileIndex"))
            {
                this.logger.Info("Begin opening Dedup File Index.");
                var indexServiceOpenParameter = new ArchiverDedupIndexServiceOpenParameter()
                {
                    IndexDatabaseName = ServiceConstants.DedupIndexDBName,
                    //BackupJobId = ,
                    IndexVolume = indexVolume,
                    TreeMode = TreeMode.SiteCollectionMode,
                    IndexLogicalDeviceSystem = this.indexLogicalDevice,
                    IndexCacheDeviceSystem = this.CacheManager.CacheSystem,
                    CacheSetting = this.cacheSetting,
                    //StorageInfo = 
                };
                //RetentionIndexSynchronizer.Initialize(indexServiceOpenParameter);
                this.InitDedupFileIndexProcessor(indexServiceOpenParameter);
            }
        }

        private void InitDedupFileIndexProcessor(ArchiverDedupIndexServiceOpenParameter openParam)
        {
            var realIndexDevice = this.CacheManager.CacheSystem;
            IndexDatabaseDownLoadResult indexDownLoadInfo;
            var logicalStorageInfo = XConvert.FromNames(openParam.IndexVolume, openParam.IndexDatabaseName, openParam.StorageInfo);

            if (openParam.IndexLogicalDeviceSystem.FileExists(logicalStorageInfo))
            {
                if (MediaConfigInfo.CommonConfigInfo.ForceUseCache)
                {
                    var dbInfo = new IndexDatabaseInfo(openParam);
                    indexDownLoadInfo = this.IndexSynchronizer.Download(dbInfo);
                }
                else
                {
                    indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Cached, Path.Combine(realIndexDevice.SystemLocation, Path.Combine(openParam.IndexVolume, openParam.IndexDatabaseName)));
                }
            }
            else
            {
                indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Nonexistent, Path.Combine(realIndexDevice.SystemLocation, Path.Combine(openParam.IndexVolume, openParam.IndexDatabaseName)));

                //azure不存在 dedup index，本地新创建，如果存在缓存的dedup index，此处会抛错，因此azure不存在时先删除本地cache的dedup index.
                FileInfo finfo = new FileInfo(indexDownLoadInfo.IndexFullPath);
                if (finfo.Exists)
                {
                    this.logger.Info($"The dedup index file exist in media cache and delete it.Path:{indexDownLoadInfo.IndexFullPath}.");
                    try
                    {
                        finfo.Delete();
                    }
                    catch (Exception ex)
                    {
                        this.logger.Error($"Delete dedup index file failed.Path:{indexDownLoadInfo.IndexFullPath}.Error:{ex}.");
                    }
                }
            }

            realIndexDevice.OpenDirectory(XConvert.FromNames(openParam.IndexVolume, string.Empty), FileMode.Create);
            IdentityManager.IdentityMode = IdentityMode.Process;
            var param = new ArchiverDedupIndexProcessorParameter(IdentityManager.IdentityContent)
            {
                DownLoadResult = indexDownLoadInfo,
                IndexWorkingSystem = realIndexDevice,
            };
            //param.IsNeedCheckIntegrity = true;
            this.DedupIndexProcessor.Open(param);
            this.logger.Info("Open DedupFileIndex Finished.");
        }

        private void UploadDedupIndexToAzure()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Dedup.UploadDedupIndexToAzure"))
            {
                logger.Info($"Begin UploadDedupIndexToAzure.");
                var dedupIndexDBInfo = new IndexDatabaseInfo(ServiceConstants.DedupIndexDBName, null);
                DatabaseUtility.RetryPolicy.ExecuteAction(() =>
                {
                    IndexSynchronizer.Upload(dedupIndexDBInfo);
                });
                logger.Info($"End UploadDedupIndexToAzure.");
            }
        }

        private void UpdateFileInDedupIndexDB(ArchiverBodyIndex fileIndex)
        {
            logger.Info($"Updating file in Dedup Index DB. ID: {fileIndex.Id}.");
            using (AvePerformanceScope pc = new AvePerformanceScope("Dedup.UpdateFileInDedupIndexDB"))
            {
                UpdateFileIndexDedupInfo(this.DedupIndexProcessor, fileIndex);
            }
        }

        private void UpdateFileIndexDedupInfo<T>(IIndexProcessor<T> indexDbProcessor, ArchiverBodyIndex fileIndex)
            where T : IndexProcessorParameter
        {
            var param = new Dictionary<string, object>()
            {
                { "@CycleId", fileIndex.CycleId },
                { "@PruneTime", fileIndex.PruneTime },
                { "@ContentDataFilePrefixNumber", fileIndex.ContentDataFilePrefixNumber },
                { "@SubRetention", fileIndex.SubRetention },
                { "@DedupExtension", fileIndex.DedupExtension },
                { "@COL_ID", fileIndex.Id },
            };

            string sql = $@"
UPDATE {IndexConstants.TableNameArchiveBody} SET 
  COL_CYCLEID = @CycleId, 
  COL_PRUNE_TIME = @PruneTime, 
  COL_CONTENT_DATA_FILE_PREFIX_NUMBER = @ContentDataFilePrefixNumber, 
  COL_SUB_RETENTION = @SubRetention, 
  COL_POOL_GUID = @DedupExtension
WHERE COL_ID = @COL_ID ";
            indexDbProcessor.Execute(
                sql,
                param);
        }

        private void UpdateFileDedupInfoToMainIndexDB<T>(IIndexProcessor<T> indexDbProcessor, ArchiverBodyIndex fileIndex)
            where T : IndexProcessorParameter
        {
            logger.Info($"Index is updating to dedup status. ID: {fileIndex.Id}.");
            using (AvePerformanceScope pc = new AvePerformanceScope("Dedup.UpdateFileDedupInfoToMainIndexDB"))
            {
                UpdateFileIndexDedupInfo(indexDbProcessor, fileIndex);
            }
        }

        private void CloseIndexProcessors()
        {
            if (IndexMainProcessor != null)
            {
                try
                {
                    IndexMainProcessor.Close();
                }
                catch (Exception ex)
                {
                    logger.Error($"Close mian index fails. {ex}");
                }
            }
            if (DedupIndexProcessor != null)
            {
                try
                {
                    DedupIndexProcessor.Close();
                }
                catch (Exception ex)
                {
                    logger.Error($"Close dedup index fails. {ex}");
                }
            }

            foreach (var processor in SubIndexProcessors)
            {
                try
                {
                    processor.Value.Close();
                }
                catch (Exception ex)
                {
                    logger.Error($"Close sub index fails, {processor.Key}. {ex}");
                }
            }
            SubIndexProcessors.Clear();
        }

        public void CloseIndexDevice()
        {
            if (StorageDeviceManager != null)
            {
                try
                {
                    StorageDeviceManager.Close(this.indexLogicalDevice);
                }
                catch (Exception ex)
                {
                    logger.Error($"Close index device fails. {ex}");
                }
            }
        }
    }
}
