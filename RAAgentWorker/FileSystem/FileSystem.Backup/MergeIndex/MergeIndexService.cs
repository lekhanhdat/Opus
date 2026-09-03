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




namespace AvePoint.Media.Service.ArchiverBackup
{
    #region directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Server.Job.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.Media.Common;
    using AvePoint.Media.Core.Index;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.Common;
    using Storage;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Storage.Util;
    using AvePoint.Media.Service.ArchiverBackup.MergeIndex;
    using AvePoint.RA.Common.Hybrid;
    using AvePoint.RA.Contract.Services;
    using RAFileSystem.FileSystem.Common;
    using RAFileSystem.FileSystem.FileSystem.Backup.CoreIndex.CoreIndexCommon;

    #endregion directives

    public class MergeIndexService
        : MergeIndexServiceBase<List<IMergeIndexSubJobInfo>>
        , IMergeIndexService
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        //private IMArchiverJobManagementService ArchiverManagementService => PlatformWindsorManager.GetService<IMArchiverJobManagementService>();
        //private static readonly IRMArchiveSiteInfoDao ArchiveSiteInfoDao = PlatformWindsorManager.GetService<IRMArchiveSiteInfoDao>();
        //private static readonly IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao = PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
        //private static readonly IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao = PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        //private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        static readonly int indexLimit = ServiceConstants.MergeIndexLimit;
        static readonly string DeleteArchiverHeadIndex = "DELETE FROM " + IndexConstants.TableNameArchiveHead + " WHERE COL_JOBID like @COL_JOBID";
        static readonly string DeleteArchiverBodyIndex = "DELETE FROM " + IndexConstants.TableNameArchiveBody + " WHERE COL_JOBID like @COL_JOBID";
        static readonly string DeleteArchiverJobInfoIndex = "DELETE FROM " + IndexConstants.TableNameArchiveJobInfo + " WHERE COL_JOB_ID like @COL_JOB_ID";
        static readonly string DeleteArchiverSiteMasterIndex = "DELETE FROM " + IndexConstants.TableNameArchiveSiteMaster + " WHERE COL_JOB_ID like @COL_JOB_ID";
        static readonly string SelectTableNameArchiverHead = "SELECT * FROM " + IndexConstants.TableNameArchiveHead + " LIMIT @OFFSET, @LENGTH";
        static readonly string SelectTableNameArchiverBody = "SELECT * FROM " + IndexConstants.TableNameArchiveBody + " LIMIT @OFFSET, @LENGTH";
        static readonly string SelectTableNameArchiverSiteMaster = "SELECT * FROM " + IndexConstants.TableNameArchiveIndexInfo;
        static readonly string SelectTableNameArchiverJobInfo = "SELECT * FROM " + IndexConstants.TableNameArchiveJobInfo + " WHERE COL_JOB_ID IS NOT NULL";
        static readonly string SelectTableNameArchiverHeadCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameArchiveHead;
        static readonly string SelectTableNameArchiverBodyCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameArchiveBody;
        static readonly string SelectTableNameArchiverBodyFileCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameArchiveBody+" WHERE COL_TYPE = 'D' AND COL_NAME NOT LIKE '%:%'";
        static readonly string SelectTableNameArchiverBodyVersionCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameArchiveBody + " WHERE COL_TYPE = 'D' AND COL_NAME LIKE '%:%'";
        JobStatusInfo jobStatusInfo = new JobStatusInfo();
        Int32 insertTotalTimes = default(Int32);
        Int32 insertRealTimes = default(Int32);
        Int32 weight = default(Int32);
        Int32 overallRatio = default(Int32);
        MergeIndexState mergeIndexJobState;
        IXSystem indexLogicalDevice;
        string dbPassword;
        MergeIndexSubJobInfo mergeIndexInfo;
        String errorMessage = String.Empty;
        List<String> needMergeIndexsName;
        bool hasMerged = false;

        public IIndexProcessor<ArchiverIndexProcessorParameter> IndexMainProcessor =new IndexProcessor<ArchiverIndexProcessorParameter>();

        public IIndexProcessor<ArchiverIndexProcessorParameter> IndexMapProcessor = new IndexProcessor<ArchiverIndexProcessorParameter>();

        public IIndexDatabaseSynchronizer IndexSynchronizer = new IndexDatabaseSynchronizer();

        /// <summary>
        /// public IJobProgressUpdater JobProgressUpdater { get; set; }
        /// </summary>

        public IStorageDeviceManager DeviceManager = new StorageDeviceManager();

        public override void Open(MergeIndexSubJobInfo info)
        {
            this.mergeIndexInfo = info;
            //需要先MergeJobStatus，确保Agent Merge完的数据能够正确显示Job Info
            this.UpdateJobStatusInfo(this.mergeIndexInfo.JobDto);
            var subSubJobId = info.JobDto.Id;//子子JobID
            //var subJobId = subSubJobId.Substring(0, subSubJobId.LastIndexOf("_"));
            if (HybridApiClient.Instance.CheckCurrentJobHasMerged(subSubJobId))
            {
                logger.Info("Current Merge Index Job already merged.JobID:{0}.", subSubJobId);
                hasMerged = true;
                return;
            }
            this.logger.Info("MergeIndexServiceOpenBegin");
            this.dbPassword = HybridApiClient.Instance.GetDBSEEMasterKey();
            this.indexLogicalDevice = this.DeviceManager.Open(this.mergeIndexInfo.IndexLogicalDevice.GetXRIS(PhysicalDeviceUsage.Index));
            XFactory.InstanceLibrary(this.mergeIndexInfo.CacheSetting.ConvertToLogicalDeviceDto().ToXRIS());
            this.CacheManager.Open(this.mergeIndexInfo.CacheSetting, BackgroundSettings.GetInstance().ArchiveCache, false, true);
            this.logger.Info("MergeIndexServiceOpenOpenIndexLogicalDeviceSuccessfully");
            this.OpenMainIndex(this.mergeIndexInfo);
            this.OpenMapIndex(this.mergeIndexInfo);
        }

        public override void MergeIndex()
        {
            this.needMergeIndexsName = new List<String>();
            if (!hasMerged)
            {
            //    this.mergeIndexInfo.MergeIndexJobsState.ForEach(item =>
            //{
                var openParameter = new ArchiverIndexServiceOpenParameter(this.mergeIndexInfo, this.indexLogicalDevice, this.CacheManager.CacheSystem, this.mergeIndexInfo.IndexVolume, dbPassword);
                var indexFiles = this.indexLogicalDevice.ListFiles(new StorageInfo(this.mergeIndexInfo.IndexVolume, string.Empty));
                var subJobIndexes = indexFiles.FindAll(index => index.Name.StartsWith(this.mergeIndexInfo.JobDto.Id, StringComparison.OrdinalIgnoreCase)
                    && index.Name.EndWithIgnoreCase(ServiceConstants.IndexDBName));
                weight = subJobIndexes.Count;
                subJobIndexes.ForEach(index =>
                {

                    openParameter.IndexDatabaseName = index.Name;
                    //this.InitIndexProcessor(openParameter);
                    this.insertTotalTimes = this.CalculateInsertCount();
                    this.logger.Info($"MergeIndexServiceMergeIndexCount:{this.insertTotalTimes}");
                    //if (item.IsSuccessful)
                    //{
                        this.InsertIntoMainIndex();
                        var propertiesName = index.Name + ".properties";
                        this.needMergeIndexsName.Add(index.Name);
                        this.needMergeIndexsName.Add(propertiesName);
                    //}
                    //else
                    //{
                    //    this.DeleteLastUnfinishedIndex(item);
                    //    this.InsertIntoMainIndex();
                    //}

                overallRatio++;
                });
            //});
                this.needMergeIndexsName.Add(ServiceConstants.IndexDBName);
                this.needMergeIndexsName.Add(ServiceConstants.IndexDBName + ".properties");
                this.logger.Info("MergeIndexServiceMergeIndexEnd");
            }
            //var syncArchivedSiteInfo = RMKeyValueDao.GetValueByKey("SyncArchivedSiteInfo");
            //if (syncArchivedSiteInfo != null)
            //{
            //    bool result;
            //    if (bool.TryParse(syncArchivedSiteInfo.Value, out result) && result)
            //    {
            //        UpdateArchivedInfo(mergeIndexInfo.ConnectionName, mergeIndexInfo.ConnectionId);
            //    }
            //    else
            //    {
            //        logger.Warn($"syncArchivedSiteInfo value is false or syncArchivedSiteInfo value convert failed,syncArchivedSiteInfo Value is:{syncArchivedSiteInfo.Value}");
            //    }
            //}
            //else
            //{
            //    logger.Warn("syncArchivedSiteInfo is null,please check it in db");
            //}
        }

        //public override void MergeIndex()
        //{
        //    this.insertTotalTimes = this.CalculateInsertCount();
        //    this.logger.Info(MediaServiceArchiverBackupResource.MergeIndexServiceMergeIndexCount, this.insertTotalTimes);
        //    this.MergingIndex(this.mergeIndexInfo);
        //    this.logger.Info(MediaServiceArchiverBackupResource.MergeIndexServiceMergeIndexEnd);
        //}

        public override void ProcessException(Exception e)
        {
            e = e.InnerException ?? e;
            this.errorMessage = e.Message;
            this.logger.Error($"MergeIndexServiceProcessExceptionError:{e}");
        }

        public override void UpdateJobStatusAndControlTable(MergeIndexState mergeIndexState, Int32 jobStatus)
        {
            this.mergeIndexJobState = mergeIndexState;
            ArchiverSiteInfoDto siteInfo = new ArchiverSiteInfoDto();
            this.mergeIndexInfo.MergeIndexJobsState.ForEach(item =>
            {
                try
                {
                    this.jobStatusInfo.State = jobStatus;
                    if (mergeIndexState.Equals(MergeIndexState.Succeed))
                    {
                        item.IsSuccessful = true;
                        this.insertTotalTimes = 1; //however we need update job progress to 100%.
                        this.insertRealTimes = this.insertTotalTimes;
                    }
                    else
                    {
                        item.IsSuccessful = false;
                        if (this.insertTotalTimes == 0)
                        {
                            this.insertRealTimes = 0;
                            this.insertTotalTimes = 1;
                        }
                    }
                    //if (!this.mergeIndexInfo.IgnoreUpdateJobState)
                    //{
                        HybridApiClient.Instance.UpdateMergeIndexStateAsync(item.JobId, (int)mergeIndexState);
                    //}
                }
                catch (Exception ex)
                {
                    this.mergeIndexJobState = MergeIndexState.Failed;
                    this.jobStatusInfo.State = 3;   //3 stand for job failed
                    this.logger.Error($"MergeIndexServiceProcessExceptionError:{ex}");
                    throw;
                }
                //this.JobProgressUpdater.UpdateJobProgress(this.jobStatusInfo, this.insertTotalTimes, this.insertRealTimes, true);
            });
        }

        public override void GenerateJobReport()
        {
            //var jobDetailList = new List<JobDetail>();
            //var jobSummaryList = new List<JobSummary>();
            //try
            //{
            //    JobDetail jobDetail = new JobDetail();
            //    jobDetail.Type = ServiceConstants.MergeDetailType;
            //    jobDetail.SrcURL = this.mergeIndexInfo.SiteUrl;
            //    jobDetail.Status = this.mergeIndexJobState.Equals(MergeIndexState.Failed) ? 1 : 0;
            //    jobDetail.Message = this.mergeIndexJobState.Equals(MergeIndexState.Succeed) ?
            //        ServiceConstants.MergeReportSuccessfulMessage : ServiceConstants.MergeReportFailedMessage;
            //    jobDetailList.Add(jobDetail);
            //    jobSummaryList.Add(new JobSummary()
            //    {
            //        Key = "Comments",
            //        Value = this.mergeIndexJobState.Equals(MergeIndexState.Failed) ? String.Format(MediaServiceArchiverBackupResource.MergeIndexServiceFailed, this.errorMessage) : MediaServiceArchiverBackupResource.MergeIndexServiceSuccessful
            //    });
            //    this.logger.Info(MediaServiceArchiverBackupResource.MergeIndexServiceGenerateJobReportFinished, this.mergeIndexInfo.MergeIndexJobsState[0].JobId);
            //}
            //catch (Exception ex)
            //{
            //    this.logger.Error(MediaServiceArchiverBackupResource.MergeIndexServiceGenerateJobReportError, ex.ToString());
            //    throw;
            //}
            //GenerateJobDetailService();
            //SubJobDto subJobInfo = new SubJobDto() { Id = this.mergeIndexInfo.JobDto.Id, ParentId = this.mergeIndexInfo.JobDto.Id.Split('_')[0] };
            //logger.Info("Id:{0} , ParentId:{1}", subJobInfo.Id, subJobInfo.ParentId);
            //if (this.mergeIndexInfo.JobDto.Id.StartsWith("EA", StringComparison.OrdinalIgnoreCase))
            //{
            //    WriteEndUserDetailToTempFile(jobDetailList, subJobInfo);
            //}
            //else
            //{
            //    this.ControlStubs.JobDetailService.UpdateSubJobDetails(jobDetailList, subJobInfo);
            //    this.ControlStubs.JobDetailService.UpdateSubJobSummary(jobSummaryList, subJobInfo);
            //}
        }

        private void WriteEndUserDetailToTempFile(List<JobDetail> details, SubJobDto subJobInfo)
        {
            string tempReportFolder = Path.Combine(AveEnv.AgentJobFolder, subJobInfo.Id);
            if (!Directory.Exists(tempReportFolder))
            {
                Directory.CreateDirectory(tempReportFolder);
            }
            string tempReportFile = Path.Combine(tempReportFolder, subJobInfo.Id + "_Report.txt");
            using (StreamWriter sr = new StreamWriter(tempReportFile, true))
            {
                foreach (var detail in details)
                {
                    sr.WriteLine(detail.Type + ";" + detail.SrcURL + ";" + detail.Status.ToString() + ";" + detail.Message);
                }
            }
        }

        private void GenerateJobDetailService()
        {
            //this.ControlStubs.JobDetailService = JobReportServiceFactory.CreateJobDetailService();

        }

        public override void Close()
        {
            this.Dispose();
        }

        protected override void Dispose(Boolean disposing)
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
            base.Dispose(disposing);
        }

        private void UpdateJobStatusInfo(BaseJobDto JobDto)
        {
            this.jobStatusInfo.Id = JobDto.Id;
            this.jobStatusInfo.Type = JobDto.Type;
            this.jobStatusInfo.IsSubJob = true;
            this.jobStatusInfo.Stamp = JobDto.Stamp;
        }

        private int CalculateInsertCount()
        {
            int insertCount = 0;
            insertCount += this.RealCalculateInsertCount();
            return insertCount;
        }

        private int RealCalculateInsertCount()
        {
            //int headTableIndexCount = Convert.ToInt32(this.IndexMapProcessor.ExecuteScalar(SelectTableNameArchiverHeadCount, null));
            int bodyTableIndexCount = Convert.ToInt32(this.IndexMapProcessor.ExecuteScalar(SelectTableNameArchiverBodyCount, null));
            //int headTableInsertCount = headTableIndexCount < indexLimit ? 1 : headTableIndexCount / indexLimit + 1;
            int bodyTableInsertCount = bodyTableIndexCount < indexLimit ? 1 : bodyTableIndexCount / indexLimit + 1;
            return bodyTableInsertCount;//headTableInsertCount + ;
        }

        private void OpenMainIndex(MergeIndexSubJobInfo jobInfo)
        {
            var openParam = new ArchiverIndexServiceOpenParameter()
            {
                IndexDatabaseName = ServiceConstants.IndexDBName,
                IndexVolume = jobInfo.IndexVolume,
                IndexLogicalDeviceSystem = this.indexLogicalDevice,
                IndexCacheDeviceSystem = this.CacheManager.CacheSystem,
                CacheSetting = jobInfo.CacheSetting,
                DBPassWord = this.dbPassword,
            };
            IndexSynchronizer.Initialize(openParam);
            this.InitIndexProcessor(openParam);
        }
        //private void OpenMainIndex(MergeIndexSubJobInfo jobInfo)
        //{
        //    var openParam = new ArchiverIndexServiceOpenParameter()
        //    {
        //        IndexDatabaseName = ServiceConstants.IndexDBName,
        //        IndexVolume = jobInfo.IndexVolume,
        //        IndexLogicalDeviceSystem = this.indexLogicalDevice,
        //    };
        //    this.InitIndexProcessor(openParam);
        //}

        private void OpenMapIndex(MergeIndexSubJobInfo jobInfo)
        {
            //jobInfo.MergeIndexJobsState.ForEach(item =>
            //{
                var openParameter = new ArchiverIndexServiceOpenParameter(jobInfo, this.indexLogicalDevice, this.CacheManager.CacheSystem, jobInfo.IndexVolume, this.dbPassword);
                this.InitIndexProcessor(openParameter);
            //});
        }

        private void InitIndexProcessor(ArchiverIndexServiceOpenParameter openParam)
        {
            var realIndexDevice = this.CacheManager.CacheSystem;// : this.indexLogicalDevice;
            IndexDatabaseDownLoadResult indexDownLoadInfo;
            StorageInfo logicalStorageInfo = XConvert.FromNames(openParam.IndexVolume, openParam.IndexDatabaseName, openParam.StorageInfo);
            if (openParam.IndexLogicalDeviceSystem.FileExists(logicalStorageInfo))
            {
                //if (MediaConfigInfo.CommonConfigInfo.ForceUseCache)
                //{
                    var dbInfo = new IndexDatabaseInfo(openParam);
                    indexDownLoadInfo = this.IndexSynchronizer.Download(dbInfo);
                //}
                //else
                //{
                //    indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Cached, SecurityUtils.SafeCombinePath(realIndexDevice.SystemLocation, SecurityUtils.SafeCombinePath(openParam.IndexVolume, openParam.IndexDatabaseName)));
                //}
            }
            else
            {
                logger.Info("the index file:{0}", logicalStorageInfo.ToString().LogBase64());
                indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Nonexistent, SecurityUtils.SafeCombinePath(realIndexDevice.SystemLocation, SecurityUtils.SafeCombinePath(openParam.IndexVolume, openParam.IndexDatabaseName)));
                if (openParam.IndexDatabaseName.Equals(ServiceConstants.IndexDBName))
                {
                    FileInfo finfo = new FileInfo(indexDownLoadInfo.IndexFullPath);
                    if (finfo.Exists)
                    {

                        this.logger.Warn("The main index cache file exists but storage does not exist current main index {0} Create time{1} and delete cache index.", indexDownLoadInfo.IndexFullPath.LogBase64(), finfo.CreationTimeUtc.ToString());
                        finfo.Delete();
                        this.logger.Info($"Success delete cache index file:{indexDownLoadInfo.IndexFullPath.LogBase64()}.");
                    }
                }
            }
            realIndexDevice.OpenDirectory(XConvert.FromNames(openParam.IndexVolume, string.Empty), FileMode.Create);
            //IdentityManager.IdentityMode = IdentityMode.Process;
            ArchiverIndexProcessorParameter param = new ArchiverIndexProcessorParameter();
            param.DownLoadResult = indexDownLoadInfo;
            param.IndexWorkingSystem = openParam.IndexLogicalDeviceSystem;
            param.DBPassWord = openParam.DBPassWord;
            if (openParam.IndexDatabaseName.Equals(ServiceConstants.IndexDBName))
            {
                param.IsNeedCheckIntegrity = true;
                this.IndexMainProcessor.Open(param);
            }
            else
                this.IndexMapProcessor.Open(param);
        }
        //private void InitIndexProcessor(ArchiverIndexServiceOpenParameter openParam)
        //{
        //    IndexDatabaseDownLoadResult indexDownLoadInfo;
        //    StorageInfo logicalStorageInfo = XConvert.FromNames(openParam.IndexVolume, openParam.IndexDatabaseName, openParam.StorageInfo);
        //    if (openParam.IndexLogicalDeviceSystem.FileExists(logicalStorageInfo))
        //    {
        //        indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Cached, Path.Combine(openParam.IndexLogicalDeviceSystem.SystemLocation, Path.Combine(openParam.IndexVolume, openParam.IndexDatabaseName)));
        //    }
        //    else
        //    {
        //        indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Nonexistent, Path.Combine(openParam.IndexLogicalDeviceSystem.SystemLocation, Path.Combine(openParam.IndexVolume, openParam.IndexDatabaseName)));
        //    }
        //    openParam.IndexLogicalDeviceSystem.OpenDirectory(XConvert.FromNames(openParam.IndexVolume, string.Empty), FileMode.Create);
        //    ArchiverIndexProcessorParameter param = new ArchiverIndexProcessorParameter();
        //    param.DownLoadResult = indexDownLoadInfo;
        //    param.IndexWorkingSystem = openParam.IndexLogicalDeviceSystem;
        //    if (openParam.IndexDatabaseName.Equals(ServiceConstants.IndexDBName))
        //        this.IndexMainProcessor.Open(param);
        //    else
        //        this.IndexMapProcessor.Open(param);
        //}



        private void DeleteLastUnfinishedIndex(MergeIndexJobState jobInfo)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            param["@COL_JOBID"] = jobInfo.JobId;
            param["@COL_JOB_ID"] = jobInfo.JobId;
            this.IndexMainProcessor.Execute(DeleteArchiverHeadIndex, param);
            this.IndexMainProcessor.Execute(DeleteArchiverBodyIndex, param);
            this.IndexMainProcessor.Execute(DeleteArchiverJobInfoIndex, param);
            this.IndexMainProcessor.Execute(DeleteArchiverSiteMasterIndex, param);
        }

        private void InsertIntoMainIndex()
        {
            //this.InsertIntoJobInfoTable();
            this.InsertIntoSiteMasterIndex();
            //this.InsertIntoHeadTable();
            this.InsertIntoBodyTable();
        }

        private void InsertIntoBodyTable()
        {
            this.logger.Info($"MergeIndexServiceInsertIntoMainIndexHeadEnd:{IndexConstants.TableNameArchiveBody.LogBase64()}");
            this.InsertIntoHeadOrBodyIndex(IndexConstants.TableNameArchiveBody, SelectTableNameArchiverBody);
            this.logger.Info($"MergeIndexServiceInsertIntoMainIndexHeadEnd:{IndexConstants.TableNameArchiveBody.LogBase64()}");
        }

        private void InsertIntoHeadTable()
        {
            this.logger.Info($"MergeIndexServiceInsertIntoMainIndexHeadBegin:{IndexConstants.TableNameArchiveHead.LogBase64()}");
            this.InsertIntoHeadOrBodyIndex(IndexConstants.TableNameArchiveHead, SelectTableNameArchiverHead);
            this.logger.Info($"MergeIndexServiceInsertIntoMainIndexHeadEnd:{IndexConstants.TableNameArchiveHead.LogBase64()}");
        }

        private void InsertIntoHeadOrBodyIndex(string tableName, string sql)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            int count = tableName.Equals(IndexConstants.TableNameArchiveHead, StringComparison.OrdinalIgnoreCase) ?
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
        //private void UpdateArchivedInfo(string siteUrl,string siteId)
        //{
        //    var siteUrlAndJobIdMapping = ArchiverSiteMasterIndexDao.GetAllBackupSiteCollectionDistinctJobIdMappings(new List<string>() { siteUrl});
        //    var siteUrlAndSizeMapping = ArchiverIndexSubInfoDao.GetAllArchiverIndexSubInfoBySiteUrls(siteUrlAndJobIdMapping);
        //    ArchiveSiteInfoDao.UpdateArchiverInfo(siteUrl, GetFileCount(), GetFileVersionCount(), siteUrlAndSizeMapping[siteUrl], siteId);
        //}
        private long GetFileCount()
        {
            var result= Convert.ToInt64(this.IndexMainProcessor.ExecuteScalar(SelectTableNameArchiverBodyFileCount, null));
            logger.Info($"file count is:{result}");
            return result;
        }
        private long GetFileVersionCount()
        {
            var result = Convert.ToInt64(this.IndexMainProcessor.ExecuteScalar(SelectTableNameArchiverBodyVersionCount, null));
            logger.Info($"file version count is:{result}");
            return result;
        }
        private void ExecuteInsert(string tableName, string sql, Dictionary<string, object> param)
        {
            if (tableName.Equals(IndexConstants.TableNameArchiveHead,StringComparison.OrdinalIgnoreCase))
            {
                List<ArchiverHeadIndex> indexes = this.IndexMapProcessor.ExecuteQuery<ArchiverHeadIndex>(sql, param);
                this.IndexMainProcessor.Insert(indexes);
            }
            else
            {
                List<ArchiverBodyIndex> indexes = this.IndexMapProcessor.ExecuteQuery<ArchiverBodyIndex>(sql, param);
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
            //this.JobProgressUpdater.UpdateJobProgress(this.jobStatusInfo);
        }

        private void InsertIntoSiteMasterIndex()
        {
            List<ArchiveIndexInfo> indexes = this.IndexMapProcessor.ExecuteQuery<ArchiveIndexInfo>(SelectTableNameArchiverSiteMaster, null);
            this.IndexMainProcessor.Insert(indexes);
        }

        private void InsertIntoJobInfoTable()
        {
            List<ArchiverJobInfoIndex> indexes = this.IndexMapProcessor.ExecuteQuery<ArchiverJobInfoIndex>(SelectTableNameArchiverJobInfo, null);
            this.IndexMainProcessor.Insert(indexes);
        }

        public override void UploadIndexToRealSystem(MergeIndexSubJobInfo info)
        {
            if (this.IndexMapProcessor != null)
            {
                this.IndexMapProcessor.Close();
            }
            if (this.IndexMainProcessor != null)
            {
                this.IndexMainProcessor.Close();
            }
            //if (MediaConfigInfo.CommonConfigInfo.ForceUseCache)
            //{
                var storageInfo = XConvert.FromNames(info.IndexVolume, ServiceConstants.IndexDBName);
                var dbInfo = new IndexDatabaseInfo(ServiceConstants.IndexDBName, null);
                this.IndexSynchronizer.Upload(dbInfo);
            //}
        }
    }
}