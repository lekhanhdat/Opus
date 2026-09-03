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
using AvePoint.Common;
using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using Media.Service.ArchiverBackup.Index;
using Media.Service.DomainModel.Index.ExchangeIndexes;
using Merged18NResources.MediaServiceArchiverBackup;
using RAArchiverCommon;
using Storage;
using JobMonitor = AvePoint.RA.Contract.RMWeb.JobMonitor;

namespace RAGoogle.Archive.Media;

public class GDriveArchiveMergeIndexJobHandler
{
    #region IndexDBSQLs
    static readonly string IndexDBSQL_DeleteAgents = "DELETE FROM " + IndexConstants.TableNameGDriveAgent + " WHERE COL_JOB_ID like @COL_JOB_ID";
    static readonly string IndexDBSQL_DeleteContainers = "DELETE FROM " + IndexConstants.TableNameGDriveContainer + " WHERE COL_JOB_ID like @COL_JOB_ID";
    static readonly string IndexDBSQL_DeleteItems = "DELETE FROM " + IndexConstants.TableNameGDriveItem + " WHERE COL_JOB_ID like @COL_JOB_ID";
    static readonly string IndexDBSQL_DeleteSiteMasters = "DELETE FROM " + IndexConstants.TableNameGDriveMaster + " WHERE COL_JOB_ID like @COL_JOB_ID";
    static readonly string IndexDBSQL_DeleteJobInfoes = "DELETE FROM " + IndexConstants.TableNameArchiveJobInfo + " WHERE COL_JOB_ID like @COL_JOB_ID";


    static readonly string IndexDBSQL_SelectAgents = "SELECT * FROM " + IndexConstants.TableNameGDriveAgent + " LIMIT @OFFSET, @LENGTH";
    static readonly string IndexDBSQL_SelectContainers = "SELECT * FROM " + IndexConstants.TableNameGDriveContainer + " LIMIT @OFFSET, @LENGTH";
    static readonly string IndexDBSQL_SelectItems = "SELECT * FROM " + IndexConstants.TableNameGDriveItem + " LIMIT @OFFSET, @LENGTH";
    static readonly string IndexDBSQL_SelectSiteMasters = "SELECT * FROM " + IndexConstants.TableNameGDriveMaster + " LIMIT @OFFSET, @LENGTH";
    static readonly string IndexDBSQL_SelectJobInfoes = "SELECT * FROM " + IndexConstants.TableNameArchiveJobInfo + " WHERE COL_JOB_ID IS NOT NULL LIMIT @OFFSET, @LENGTH";

    static readonly string IndexDBSQL_SelectAgentsCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameGDriveAgent;
    static readonly string IndexDBSQL_SelectContainersCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameGDriveContainer;
    static readonly string IndexDBSQL_SelectItemsCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameGDriveItem;
    static readonly string IndexDBSQL_SelectSiteMastersCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameGDriveMaster;
    static readonly string IndexDBSQL_SelectJobInfoesCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameArchiveJobInfo;
    static readonly string SelectTableNameArchiverItemFileCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameGDriveItem + " WHERE COL_TYPE = '20'";
    static readonly string SelectTableNameArchiverItemVersionCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameGDriveItem + " WHERE COL_TYPE = '21'";
    #endregion

    private IRALogger logger = RALogger.GetInstance(typeof(GDriveArchiveMergeIndexJobHandler));

    private IIndexProcessor<GDriveArchiverIndexProcessorParameter> IndexMainProcessor = new IndexProcessor<GDriveArchiverIndexProcessorParameter>();

    private IIndexProcessor<GDriveArchiverIndexProcessorParameter> IndexMapProcessor = new IndexProcessor<GDriveArchiverIndexProcessorParameter>();

    private IIndexDatabaseSynchronizer IndexSynchronizer = new IndexDatabaseSynchronizer();

    private static readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
    private static readonly IArchiverIndexSubInfoDao _archiverIndexSubInfoDao = PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
    private static readonly IArchiverSiteMasterIndexDao _archiverSiteMasterIndexDao = PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
    private static readonly IRMRemoteNodeDao _remoteNodeDao = PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
    private static readonly IRMArchiveGDriveInfoDao _archiveGDriveInfoDao = PlatformWindsorManager.GetService<IRMArchiveGDriveInfoDao>();


    private IJobProgressUpdater JobProgressUpdater => PlatformWindsorManager.GetService<IJobProgressUpdater>();
    private ICacheService CacheManager = new CacheService() { IndexCacheRetentionManager = new IndexCacheRetentionManager() };

    static readonly int indexLimit = ServiceConstants.MergeIndexLimit;

    public IStorageDeviceManager DeviceManager => PlatformWindsorManager.GetService<IStorageDeviceManager>();
    private IMArchiverJobManagementService ArchiverManagementService => PlatformWindsorManager.GetService<IMArchiverJobManagementService>();

    IXSystem indexLogicalDevice;
    GDriveMergeIndexSubJobInfo mergeIndexInfo;
    List<String> needMergeIndexsName;
    bool hasMerged = false;
    Int32 weight = default(Int32);
    Int32 insertTotalTimes = default(Int32);
    Int32 insertRealTimes = default(Int32);
    Int32 overallRatio = default(Int32);
    Dictionary<string, int> tablesCount = new Dictionary<string, int>();
    JobStatusInfo jobStatusInfo = new JobStatusInfo();

    public void PerformMergeIndex(GDriveMergeIndexJobInfo job, GDriveBackupRequest request)
    {
        job.CacheLocation.Extension = new CacheSettingExtension { Path = new List<PathMap>() };
        DiskInfoDto disk = new()
        {
            Path = BackgroundSettings.GetInstance().ArchiveCache,
            Type = DeviceType.LocalPath,
            Password = null,
            UserName = string.Empty,
            Usage = null
        };
        job.CacheLocation.Extension.Path.Add(new PathMap() { DiskInfo = disk });
        job.CacheLocation.LimitFreeSpace = 1024 * 1024 * 1024;
        var subJobInfo = new GDriveMergeIndexSubJobInfo(job, request);
        subJobInfo.JobDto.Id = request.JobId;
        MergeIndexInternal(subJobInfo);
    }

    private void MergeIndexInternal(GDriveMergeIndexSubJobInfo mergeInfo)
    {
        MergeIndexState mergeIndexState = MergeIndexState.Succeed;
        var jobStatus = JobMonitor.JobStatus.Finished;
        try
        {
            this.Open(mergeInfo);
            this.MergeIndex();
            this.UploadIndexToRealSystem(mergeInfo);

        }
        catch (Exception e)
        {
            mergeIndexState = MergeIndexState.Failed;
            this.logger.Error($"Merge Index Service Merge Index Error, {e}");
            throw;
        }
        finally
        {
            UpdateJobStatusAndControlTable(mergeIndexState, jobStatus);
            this.Close();
        }
    }
    private void UpdateJobStatusAndControlTable(MergeIndexState mergeIndexState, JobMonitor.JobStatus jobStatus)
    {
        ArchiverSiteInfoDto siteInfo = new ArchiverSiteInfoDto();
        this.mergeIndexInfo.MergeIndexJobsState.ForEach(item =>
        {
            try
            {
                this.jobStatusInfo.State = (int)jobStatus;
                if (mergeIndexState.Equals(MergeIndexState.Succeed))
                {
                    item.IsSuccessful = true;

                }
                else
                {
                    item.IsSuccessful = false;

                }
                ArchiverManagementService.UpdateGDriveMergeIndexStateAsync(item.JobId, siteInfo, mergeIndexState).Wait();
            }
            catch (Exception ex)
            {
                this.jobStatusInfo.State = (int)JobMonitor.JobStatus.Failed;
                this.logger.Error(MediaServiceArchiverBackupResource.MergeIndexServiceUpdateJobStatusAndControlTableError, ex.ToString());
                throw;
            }
        });
    }

    private void Close()
    {
        try
        {
            if (this.needMergeIndexsName != null && this.needMergeIndexsName.Count > 0)
            {
                this.needMergeIndexsName.ForEach(item =>
                {
                    if (this.CacheManager.CacheSystem.FileExists(new StorageInfo(this.mergeIndexInfo.IndexVolume, item)))
                        this.CacheManager.CacheSystem.DeleteFile(new StorageInfo(this.mergeIndexInfo.IndexVolume, item));
                });
            }
            else
            {
                this.logger.Warn("There is no index files need to be deleted.");
            }
        }
        catch (Exception e)
        {
            this.logger.Warn("An error occurred while delete index.details:{0}.", e.Message.ToString());
        }
        if (this.DeviceManager != null)
        {
            this.DeviceManager.Close(this.indexLogicalDevice);
        }
    }

    private void UploadIndexToRealSystem(GDriveMergeIndexSubJobInfo info)
    {
        if (this.IndexMapProcessor != null)
        {
            this.IndexMapProcessor.Close();
        }
        if (this.IndexMainProcessor != null)
        {
            this.IndexMainProcessor.Close();
        }
        if (MediaConfigInfo.CommonConfigInfo.ForceUseCache)
        {
            var storageInfo = XConvert.FromNames(info.IndexVolume, ServiceConstants.IndexDBName);
            var dbInfo = new IndexDatabaseInfo(ServiceConstants.IndexDBName, null);
            this.IndexSynchronizer.Upload(dbInfo);
        }
    }

    private void MergeIndex()
    {
        this.needMergeIndexsName = new List<String>();
        if (!hasMerged)
        {
            var openParameter = new GDriveIndexServiceOpenParameter(
                mergeIndexInfo.JobDto.Id,
                this.indexLogicalDevice,
                this.CacheManager.CacheSystem,
                this.mergeIndexInfo.IndexVolume);
            var indexFiles = this.indexLogicalDevice.ListFiles(new StorageInfo(this.mergeIndexInfo.IndexVolume, string.Empty));
            var subJobIndexes = indexFiles.FindAll(index => index.Name.StartsWith(mergeIndexInfo.JobDto.Id, StringComparison.OrdinalIgnoreCase)
                && index.Name.EndsWith(ServiceConstants.IndexDBName, StringComparison.OrdinalIgnoreCase));
            weight = subJobIndexes.Count;
            subJobIndexes.ForEach(index =>
            {
                openParameter.IndexDatabaseName = index.Name;
                this.InitIndexProcessor(openParameter);
                this.insertTotalTimes = this.CalculateInsertCount();
                this.DeleteLastUnfinishedIndex(mergeIndexInfo.JobDto.Id);
                this.InsertIntoMainIndex();

                overallRatio++;
            });
            this.needMergeIndexsName.Add(ServiceConstants.IndexDBName);
            this.needMergeIndexsName.Add(ServiceConstants.IndexDBName + ".properties");
            this.logger.Info(MediaServiceArchiverBackupResource.MergeIndexServiceMergeIndexEnd);
        }
        UpdateArchivedInfo(mergeIndexInfo.DriveName, mergeIndexInfo.DriveId);
    }
    private void UpdateArchivedInfo(string driveName, string driveId)
    {
        var driveIdAndJobIdMapping = _archiverSiteMasterIndexDao.GetAllBackupGDriveDistinctJobIdMappings(new List<string>() { driveId });
        var driveIdlAndSizeMapping = _archiverIndexSubInfoDao.GetAllGoogleArchiverIndexSubInfoByDriveIds(driveIdAndJobIdMapping);
        var tenantId = _remoteNodeDao.GetTenantIdByObjectId(driveId);
        _archiveGDriveInfoDao.UpdateGoogleArchiverInfo(driveName, GetFileCount(), GetFileVersionCount(), tenantId, driveId, driveIdlAndSizeMapping[driveId]);
    }
    private long GetFileCount()
    {
        var result = Convert.ToInt64(this.IndexMainProcessor.ExecuteScalar(SelectTableNameArchiverItemFileCount, null));
        logger.Info($"file count is:{result}");
        return result;
    }
    private long GetFileVersionCount()
    {
        var result = Convert.ToInt64(this.IndexMainProcessor.ExecuteScalar(SelectTableNameArchiverItemVersionCount, null));
        logger.Info($"file version count is:{result}");
        return result;
    }

    private void DeleteLastUnfinishedIndex(string subsubJobId)
    {
        Dictionary<string, object> param = new Dictionary<string, object>();
        param["@COL_JOB_ID"] = subsubJobId;
        //this.IndexMainProcessor.Execute(IndexDBSQL_DeleteAgents, param);
        this.IndexMainProcessor.Execute(IndexDBSQL_DeleteContainers, param);
        this.IndexMainProcessor.Execute(IndexDBSQL_DeleteItems, param);
        this.IndexMainProcessor.Execute(IndexDBSQL_DeleteSiteMasters, param);
        this.IndexMainProcessor.Execute(IndexDBSQL_DeleteJobInfoes, param);
    }

    private void InsertIntoMainIndex()
    {
        //this.InsertIntoMainIndexTable<GroupAgentIndex>(IndexConstants.TableNameExchangeAgent, IndexDBSQL_SelectAgents, IndexDBSQL_SelectAgentsCount);
        this.InsertIntoMainIndexTable<GoogleContainerIndex>(IndexConstants.TableNameExchangeContainer, IndexDBSQL_SelectContainers, IndexDBSQL_SelectContainersCount);
        this.InsertIntoMainIndexTable<GoogleItemIndex>(IndexConstants.TableNameExchangeItem, IndexDBSQL_SelectItems, IndexDBSQL_SelectItemsCount);
        this.InsertIntoMainIndexTable<GDriveMasterIndex>(IndexConstants.TableNameExchangeSiteMaster, IndexDBSQL_SelectSiteMasters, IndexDBSQL_SelectSiteMastersCount);
        this.InsertIntoMainIndexTable<ArchiverJobInfoIndex>(IndexConstants.TableNameArchiveJobInfo, IndexDBSQL_SelectJobInfoes, IndexDBSQL_SelectJobInfoesCount);
    }

    private int GetTableCount(string selectCountSql)
    {
        if (!tablesCount.TryGetValue(selectCountSql, out var count))
        {
            count = Convert.ToInt32(this.IndexMapProcessor.ExecuteScalar(selectCountSql, null));
            tablesCount[selectCountSql] = count;
        }

        return count;
    }

    private void InsertIntoMainIndexTable<TIndexable>(string tableName, string selectDataSql, string selectCountSql) where TIndexable : IIndexable
    {
        logger.Info($"Start merging {tableName}");
        Dictionary<string, object> param = new Dictionary<string, object>();
        var count = GetTableCount(selectCountSql);
        long number = count / indexLimit;
        long size = count % indexLimit;
        int offset = 0;
        this.logger.Info($"Total records: {count}, page limit: {indexLimit}");

        if (number >= 1)
        {
            for (int i = 0; i < number; i++)
            {
                param["@OFFSET"] = offset;
                param["@LENGTH"] = indexLimit;
                ExecuteInsert<TIndexable>(tableName, selectDataSql, param);
                offset = offset + indexLimit;
            }
        }
        if (size > 0)
        {
            param["@OFFSET"] = offset;
            param["@LENGTH"] = size;
            ExecuteInsert<TIndexable>(tableName, selectDataSql, param);
        }

        logger.Info($"Finish merge {tableName}");
    }

    private void ExecuteInsert<TIndexable>(string tableName, string sql, Dictionary<string, object> param) where TIndexable : IIndexable
    {
        List<TIndexable> indexes = this.IndexMapProcessor.ExecuteQuery<TIndexable>(sql, param);
        if (indexes.Count == 0)
        {
            return;
        }

        this.IndexMainProcessor.Insert(indexes);
        this.insertRealTimes++;

        try
        {
            Int32 tempProgress = (Int32)((this.insertRealTimes * 1.0 / this.insertTotalTimes) * 100);
            Int32 calculationProgress = (1 / this.weight) * 100 * this.overallRatio + (1 / this.weight) * tempProgress;
            jobStatusInfo.Progress = calculationProgress >= 100 ? 99 : calculationProgress;
        }
        catch (Exception e)
        {
            logger.Warn($"Calculation progress error {e.ToString()}");
        }
        this.JobProgressUpdater.UpdateJobProgress(this.jobStatusInfo);
    }

    private int CalculateInsertCount()
    {
        var tableCounts = new List<int>()
        {
            //GetTableCount(IndexDBSQL_SelectAgentsCount),
            GetTableCount(IndexDBSQL_SelectContainersCount),
            GetTableCount(IndexDBSQL_SelectItemsCount),
            GetTableCount(IndexDBSQL_SelectSiteMastersCount),
            GetTableCount(IndexDBSQL_SelectJobInfoesCount),
        };

        int insertTimes = 0;

        foreach (var count in tableCounts)
        {
            insertTimes += count / indexLimit;

            if (count % indexLimit > 0)
            {
                insertTimes++;
            }
        }

        return insertTimes;
    }

    private void Open(GDriveMergeIndexSubJobInfo info)
    {
        this.mergeIndexInfo = info;
        this.UpdateJobStatusInfo(this.mergeIndexInfo.JobDto);
        this.logger.Info(MediaServiceArchiverBackupResource.MergeIndexServiceOpenBegin);
        this.indexLogicalDevice = this.DeviceManager.Open(this.mergeIndexInfo.IndexLogicalDevice.GetXRIS(PhysicalDeviceUsage.Index));
        this.CacheManager.Open(this.mergeIndexInfo.CacheSetting, false, true);
        this.logger.Info(MediaServiceArchiverBackupResource.MergeIndexServiceOpenOpenIndexLogicalDeviceSuccessfully);
        this.OpenMainIndex(this.mergeIndexInfo);
        this.OpenMapIndex(this.mergeIndexInfo);
    }
    private void UpdateJobStatusInfo(BaseJobDto JobDto)
    {
        this.jobStatusInfo.Id = JobDto.Id;
        this.jobStatusInfo.Type = JobDto.Type;
        this.jobStatusInfo.IsSubJob = true;
        this.jobStatusInfo.Stamp = JobDto.Stamp;
    }

    private void OpenMainIndex(GDriveMergeIndexSubJobInfo jobInfo)
    {
        var openParam = new GDriveIndexServiceOpenParameter()
        {
            IndexDatabaseName = ServiceConstants.IndexDBName,
            IndexVolume = jobInfo.IndexVolume,
            IndexLogicalDeviceSystem = this.indexLogicalDevice,
            IndexCacheDeviceSystem = this.CacheManager.CacheSystem,
            CacheSetting = jobInfo.CacheSetting,
        };
        IndexSynchronizer.Initialize(openParam);
        this.InitIndexProcessor(openParam);
    }
    private void OpenMapIndex(GDriveMergeIndexSubJobInfo jobInfo)
    {
        var openParameter = new GDriveIndexServiceOpenParameter(jobInfo.JobDto.Id, this.indexLogicalDevice, this.CacheManager.CacheSystem, jobInfo.IndexVolume);
        this.InitIndexProcessor(openParameter);
    }

    private void InitIndexProcessor(GDriveIndexServiceOpenParameter openParam)
    {
        var realIndexDevice = MediaConfigInfo.CommonConfigInfo.ForceUseCache ? this.CacheManager.CacheSystem : this.indexLogicalDevice;
        IndexDatabaseDownLoadResult indexDownLoadInfo;
        StorageInfo logicalStorageInfo = XConvert.FromNames(openParam.IndexVolume, openParam.IndexDatabaseName, openParam.StorageInfo);
        if (openParam.IndexLogicalDeviceSystem.FileExists(logicalStorageInfo))
        {
            if (MediaConfigInfo.CommonConfigInfo.ForceUseCache)
            {
                var dbInfo = new IndexDatabaseInfo(openParam);
                indexDownLoadInfo = this.IndexSynchronizer.Download(dbInfo);
            }
            else
            {
                indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Cached, SecurityUtils.SafeCombinePath(realIndexDevice.SystemLocation, SecurityUtils.SafeCombinePath(openParam.IndexVolume, openParam.IndexDatabaseName)));
            }
        }
        else
        {
            logger.Info("the index file:{0}", logicalStorageInfo.ToString());
            indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Nonexistent, SecurityUtils.SafeCombinePath(realIndexDevice.SystemLocation, SecurityUtils.SafeCombinePath(openParam.IndexVolume, openParam.IndexDatabaseName)));
            if (openParam.IndexDatabaseName.Equals(ServiceConstants.IndexDBName))
            {
                FileInfo finfo = new FileInfo(indexDownLoadInfo.IndexFullPath);
                if (finfo.Exists)
                {

                    this.logger.Warn("The main index cache file exists but storage does not exist current main index {0} Create time{1} and delete cache index.", indexDownLoadInfo.IndexFullPath, finfo.CreationTimeUtc.ToString());
                    finfo.Delete();
                    this.logger.Info($"Success delete cache index file:{indexDownLoadInfo.IndexFullPath}.");
                }
            }
        }
        realIndexDevice.OpenDirectory(XConvert.FromNames(openParam.IndexVolume, string.Empty), FileMode.Create);
        IdentityManager.IdentityMode = IdentityMode.Process;
        var param = new GDriveArchiverIndexProcessorParameter(IdentityManager.IdentityContent);
        param.DownLoadResult = indexDownLoadInfo;
        param.IndexWorkingSystem = openParam.IndexLogicalDeviceSystem;
        if (openParam.IndexDatabaseName.Equals(ServiceConstants.IndexDBName))
        {
            param.IsNeedCheckIntegrity = true;
            this.IndexMainProcessor.Open(param);
        }
        else
            this.IndexMapProcessor.Open(param);
    }
}
