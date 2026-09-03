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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.DB.Dao.Impl;
using Media.Service.ArchiverBackup.Index;
using Media.Service.DomainModel.Index.ExchangeIndexes;
using Merged18NResources.MediaServiceArchiverBackup;
using RAArchiverCommon;
using Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.RAExchange.Disposal.MergeIndex
{
    internal class EXOArchiverMergeIndexJobHandler
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public IIndexProcessor<EXOArchiverIndexProcessorParameter> IndexMainProcessor= new IndexProcessor<EXOArchiverIndexProcessorParameter>();

        public IIndexProcessor<EXOArchiverIndexProcessorParameter> IndexMapProcessor = new IndexProcessor<EXOArchiverIndexProcessorParameter>();

        public IIndexDatabaseSynchronizer IndexSynchronizer= new IndexDatabaseSynchronizer();

        public IJobProgressUpdater JobProgressUpdater=> PlatformWindsorManager.GetService<IJobProgressUpdater>();
        public ICacheService CacheManager = new CacheService() { IndexCacheRetentionManager = new IndexCacheRetentionManager()};

        static readonly int indexLimit = ServiceConstants.MergeIndexLimit;
        static readonly string DeleteArchiverHeadIndex = "DELETE FROM " + IndexConstants.TableNameExchangeContainer + " WHERE COL_JOB_ID like @COL_JOB_ID";
        static readonly string DeleteArchiverBodyIndex = "DELETE FROM " + IndexConstants.TableNameExchangeItem + " WHERE COL_JOB_ID like @COL_JOB_ID";
        static readonly string DeleteArchiverJobInfoIndex = "DELETE FROM " + IndexConstants.TableNameArchiveJobInfo + " WHERE COL_JOB_ID like @COL_JOB_ID";
        static readonly string DeleteArchiverSiteMasterIndex = "DELETE FROM " + IndexConstants.TableNameExchangeSiteMaster + " WHERE COL_JOB_ID like @COL_JOB_ID";
        static readonly string SelectTableNameArchiverHead = "SELECT * FROM " + IndexConstants.TableNameExchangeContainer + " LIMIT @OFFSET, @LENGTH";
        static readonly string SelectTableNameArchiverBody = "SELECT * FROM " + IndexConstants.TableNameExchangeItem + " LIMIT @OFFSET, @LENGTH";
        //static readonly string SelectTableNameArchiverSiteMaster = "SELECT * FROM " + IndexConstants.TableNameArchiveSiteMaster;
        //static readonly string SelectTableNameArchiverJobInfo = "SELECT * FROM " + IndexConstants.TableNameArchiveJobInfo + " WHERE COL_JOB_ID IS NOT NULL";
        static readonly string SelectTableNameArchiverHeadCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameExchangeContainer;
        static readonly string SelectTableNameArchiverBodyCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameExchangeItem;
        //static readonly string SelectTableNameArchiverBodyFileCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameExchangeItem + " WHERE COL_TYPE = 'D' AND COL_NAME NOT LIKE '%:%'";
        //static readonly string SelectTableNameArchiverBodyVersionCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameExchangeItem + " WHERE COL_TYPE = 'D' AND COL_NAME LIKE '%:%'";
        public IStorageDeviceManager DeviceManager => PlatformWindsorManager.GetService<IStorageDeviceManager>();
        IXSystem indexLogicalDevice;
        EXOMergeIndexSubJobInfo mergeIndexInfo;
        List<String> needMergeIndexsName;
        bool hasMerged = false;
        Int32 weight = default(Int32);
        Int32 insertTotalTimes = default(Int32);
        Int32 insertRealTimes = default(Int32);
        Int32 overallRatio = default(Int32);
        JobStatusInfo jobStatusInfo = new JobStatusInfo();
        public void PerformMergeIndexSubJob(EXOMergeIndexJobInfo job, string subJobId,string mailBoxAddress)
        {
            job.CacheLocation = new CacheSettingDto { Extension = new CacheSettingExtension { Path = new List<PathMap>() } };
            DiskInfoDto disk = new DiskInfoDto()
            {
                Path = BackgroundSettings.GetInstance().ArchiveCache,
                Type = DeviceType.LocalPath,
                Password = null,
                UserName = string.Empty,
                Usage = null
            };
            job.CacheLocation.Extension.Path.Add(new PathMap() { DiskInfo = disk });
            job.CacheLocation.LimitFreeSpace = 1024 * 1024 * 1024;//1 GB
            EXOMergeIndexSubJobInfo subJobInfo = new EXOMergeIndexSubJobInfo(job, subJobId, mailBoxAddress);
            subJobInfo.JobDto.Id = subJobId;
            //subJobInfo.IgnoreUpdateJobState = true;
            //need further optimization
            MergeIndexInternal(subJobInfo);
        }

        private void MergeIndexInternal(EXOMergeIndexSubJobInfo mergeInfo)
        {
            MergeIndexState mergeIndexState = MergeIndexState.Succeed;
            Int32 jobStatus = 2;    //2 stand for job succeed
            try
            {
                this.Open(mergeInfo);
                this.MergeIndex();
                this.UploadIndexToRealSystem(mergeInfo);

            }
            catch (Exception e)
            {
                mergeIndexState = MergeIndexState.Failed;
                jobStatus = 3;     //3 stand for job failed
                this.logger.Error($"Merge Index Service Merge Index Error, {e}");
                throw;
            }
            finally
            {
                try
                {
                    //this.UpdateJobStatusAndControlTable(mergeIndexState, jobStatus);
                    //this.GenerateJobReport();
                }
                catch (Exception e)
                {
                    try
                    {
                        Thread.Sleep(5000);
                        //this.UpdateJobStatusAndControlTable(mergeIndexState, jobStatus);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
                finally
                {
                    this.Close();
                }
            }
        }
        public void Close()
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
        public void UploadIndexToRealSystem(EXOMergeIndexSubJobInfo info)
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
        public void MergeIndex()
        {
            this.needMergeIndexsName = new List<String>();
            if (!hasMerged)
            {
                //this.mergeIndexInfo.MergeIndexJobsState.ForEach(item =>
                //{
                    var openParameter = new ExchangeIndexServiceOpenParameter(mergeIndexInfo.JobDto.Id, this.indexLogicalDevice, this.CacheManager.CacheSystem, this.mergeIndexInfo.IndexVolume);
                    var indexFiles = this.indexLogicalDevice.ListFiles(new StorageInfo(this.mergeIndexInfo.IndexVolume, string.Empty));
                    var subJobIndexes = indexFiles.FindAll(index => index.Name.StartsWith(mergeIndexInfo.JobDto.Id, StringComparison.OrdinalIgnoreCase)
                        && index.Name.EndsWith(ServiceConstants.IndexDBName,StringComparison.OrdinalIgnoreCase));
                    weight = subJobIndexes.Count;
                    subJobIndexes.ForEach(index =>
                    {

                        openParameter.IndexDatabaseName = index.Name;
                        this.InitIndexProcessor(openParameter);
                        this.insertTotalTimes = this.CalculateInsertCount();
                        this.logger.Info(MediaServiceArchiverBackupResource.MergeIndexServiceMergeIndexCount, this.insertTotalTimes);
                        //if (item.IsSuccessful)
                        //{
                        //    this.InsertIntoMainIndex();
                        //    var propertiesName = index.Name + ".properties";
                        //    this.needMergeIndexsName.Add(index.Name);
                        //    this.needMergeIndexsName.Add(propertiesName);
                        //}
                        //else
                        //{
                            this.DeleteLastUnfinishedIndex(mergeIndexInfo.JobDto.Id);
                            this.InsertIntoMainIndex();
                        //}

                        overallRatio++;
                    });
                //});
                this.needMergeIndexsName.Add(ServiceConstants.IndexDBName);
                this.needMergeIndexsName.Add(ServiceConstants.IndexDBName + ".properties");
                this.logger.Info(MediaServiceArchiverBackupResource.MergeIndexServiceMergeIndexEnd);
            }
        }
        private void InsertIntoMainIndex()
        {
            this.InsertIntoHeadTable();
            this.InsertIntoBodyTable();
        }

        private void InsertIntoBodyTable()
        {
            this.logger.Info(MediaServiceArchiverBackupResource.MergeIndexServiceInsertIntoMainIndexBodyBegin, IndexConstants.TableNameExchangeItem);
            this.InsertIntoHeadOrBodyIndex(IndexConstants.TableNameExchangeItem, SelectTableNameArchiverBody);
            this.logger.Info(MediaServiceArchiverBackupResource.MergeIndexServiceInsertIntoMainIndexBodyEnd, IndexConstants.TableNameExchangeItem);
        }
        private void DeleteLastUnfinishedIndex(string subsubJobId)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            //["@COL_JOBID"] = subsubJobId;
            param["@COL_JOB_ID"] = subsubJobId;
            this.IndexMainProcessor.Execute(DeleteArchiverHeadIndex, param);
            this.IndexMainProcessor.Execute(DeleteArchiverBodyIndex, param);
            this.IndexMainProcessor.Execute(DeleteArchiverJobInfoIndex, param);
            this.IndexMainProcessor.Execute(DeleteArchiverSiteMasterIndex, param);
        }
        private void InsertIntoHeadTable()
        {
            this.logger.Info(MediaServiceArchiverBackupResource.MergeIndexServiceInsertIntoMainIndexHeadBegin, IndexConstants.TableNameExchangeContainer);
            this.InsertIntoHeadOrBodyIndex(IndexConstants.TableNameExchangeContainer, SelectTableNameArchiverHead);
            this.logger.Info(MediaServiceArchiverBackupResource.MergeIndexServiceInsertIntoMainIndexHeadEnd, IndexConstants.TableNameExchangeContainer);
        }

        private void InsertIntoHeadOrBodyIndex(string tableName, string sql)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            int count = tableName.EqualsIgnoreCase(IndexConstants.TableNameExchangeContainer) ?
                Convert.ToInt32(this.IndexMapProcessor.ExecuteScalar(SelectTableNameArchiverHeadCount, null))
                : Convert.ToInt32(this.IndexMapProcessor.ExecuteScalar(SelectTableNameArchiverBodyCount, null));
            long number = count / indexLimit;
            long size = count % indexLimit;
            int offset = 0;

            if (number >= 1)
            {
                for (int i = 0; i < number; i++)
                {
                    param["@OFFSET"] = offset;
                    param["@LENGTH"] = indexLimit;
                    ExecuteInsert(tableName, sql, param);
                    offset = offset + indexLimit;
                }
            }
            if (size > 0)
            {
                param["@OFFSET"] = offset;
                param["@LENGTH"] = size;
                ExecuteInsert(tableName, sql, param);
            }
            if (number == 0 && size == 0)
            {
                this.insertRealTimes = this.insertTotalTimes;
            }
        }
        private void ExecuteInsert(string tableName, string sql, Dictionary<string, object> param)
        {
            if (tableName.EqualsIgnoreCase(IndexConstants.TableNameExchangeContainer))
            {
                List<ExchangeContainerIndex> indexes = this.IndexMapProcessor.ExecuteQuery<ExchangeContainerIndex>(sql, param);
                this.IndexMainProcessor.Insert(indexes);
            }
            else
            {
                List<ExchangeItemIndex> indexes = this.IndexMapProcessor.ExecuteQuery<ExchangeItemIndex>(sql, param);
                this.IndexMainProcessor.Insert(indexes);
            }
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
            int insertCount = 0;
            insertCount += this.RealCalculateInsertCount();
            return insertCount;
        }

        private int RealCalculateInsertCount()
        {
            int headTableIndexCount = Convert.ToInt32(this.IndexMapProcessor.ExecuteScalar(SelectTableNameArchiverHeadCount, null));
            int bodyTableIndexCount = Convert.ToInt32(this.IndexMapProcessor.ExecuteScalar(SelectTableNameArchiverBodyCount, null));
            int headTableInsertCount = headTableIndexCount < indexLimit ? 1 : headTableIndexCount / indexLimit + 1;
            int bodyTableInsertCount = bodyTableIndexCount < indexLimit ? 1 : bodyTableIndexCount / indexLimit + 1;
            return headTableInsertCount + bodyTableInsertCount;
        }
        public void Open(EXOMergeIndexSubJobInfo info)
        {
            this.mergeIndexInfo = info;
            //需要先MergeJobStatus，确保Agent Merge完的数据能够正确显示Job Info
            //this.UpdateJobStatusInfo(this.mergeIndexInfo.JobDto);
            //var subSubJobId = this.mergeIndexInfo.MergeIndexJobsState[0].JobId;//子子JobID
            //var subJobId = subSubJobId.Substring(0, subSubJobId.LastIndexOf("_"));
            //if (ArchiverManagementService.CheckCurrentJobHasMerged(subSubJobId, IdentityManager.IdentityContent))
            //{
            //    logger.Info("Current Merge Index Job already merged.JobID:{0}.", subSubJobId);
            //    hasMerged = true;
            //    return;
            //}
            this.logger.Info(MediaServiceArchiverBackupResource.MergeIndexServiceOpenBegin);
            this.indexLogicalDevice = this.DeviceManager.Open(this.mergeIndexInfo.IndexLogicalDevice.GetXRIS(PhysicalDeviceUsage.Index));
            this.CacheManager.Open(this.mergeIndexInfo.CacheSetting, false, true);
            this.logger.Info(MediaServiceArchiverBackupResource.MergeIndexServiceOpenOpenIndexLogicalDeviceSuccessfully);
            this.OpenMainIndex(this.mergeIndexInfo);
            this.OpenMapIndex(this.mergeIndexInfo);
        }
        private void OpenMainIndex(EXOMergeIndexSubJobInfo jobInfo)
        {
            var openParam = new ExchangeIndexServiceOpenParameter()
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
        private void OpenMapIndex(EXOMergeIndexSubJobInfo jobInfo)
        {
            //jobInfo.MergeIndexJobsState.ForEach(item =>
            //{
                var openParameter = new ExchangeIndexServiceOpenParameter(jobInfo.JobDto.Id, this.indexLogicalDevice, this.CacheManager.CacheSystem, jobInfo.IndexVolume);
                this.InitIndexProcessor(openParameter);
            //});
        }

        private void InitIndexProcessor(ExchangeIndexServiceOpenParameter openParam)
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
            EXOArchiverIndexProcessorParameter param = new EXOArchiverIndexProcessorParameter(IdentityManager.IdentityContent);
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
}
