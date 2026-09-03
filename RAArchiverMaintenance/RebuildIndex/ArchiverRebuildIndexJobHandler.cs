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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.DBLocker;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.ArchiverCommon;
using Microsoft.SharePoint.Client;
using RAArchiverCommon;
using Storage;
using System;
using ArchiverSiteMasterIndex = AvePoint.Media.Service.DomainModel.ArchiverSiteMasterIndex;
using JobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus;
using JobType = AvePoint.RA.Contract.JobMonitor.JobType;
using Path = System.IO.Path;

namespace RAArchiverMaintenance.RebuildIndex
{
    public class ArchiverRebuildIndexJobHandler
    {
        private static IRALogger mLog = new RALogger(typeof(ArchiverRebuildIndexJobHandler));
        private string SubJobId = string.Empty;
        private JobStatus mJobStatus = JobStatus.Finished;
        private int currentJobProgress = 1;
        private bool SiteHasMergedNode { get; set; }
        private bool HasCompleteNode { get; set; }
        private bool HasErrorNode { get; set; }
        private bool HasStop { get; set; }

        private Random random = new Random();
        private int indexLimit = ServiceConstants.MergeIndexLimit;
        private CacheSettingDto cacheSetting;
        private IXSystem indexLogicalDevice;
        private IVolumeGenerator volumeGenerator = new VolumeGeneratorFactory().GetVolumeGenerator(ProductModule.ArchiverBackup);

        private IRMReportManager ReportManager = ReportMangerFactory.Instance.ReportManager;
        private IStorageDeviceManager DeviceManager = PlatformWindsorManager.GetService<IStorageDeviceManager>();
        private ICacheService CacheManager = PlatformWindsorManager.GetService<ICacheService>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();

        public IIndexDatabaseSynchronizer IndexSynchronizer = PlatformWindsorManager.GetService<IIndexDatabaseSynchronizer>();
        public IIndexProcessor<ArchiverIndexProcessorParameter> MainIndexProcessor = new IndexProcessor<ArchiverIndexProcessorParameter>();
        public IIndexProcessor<ArchiverIndexProcessorParameter> SubIndexProcessor = PlatformWindsorManager.GetService<IIndexProcessor<ArchiverIndexProcessorParameter>>();

        

        private class IndexDbQuerySQL
        {
            public static readonly string GetArchiverSiteMasters = "SELECT * FROM " + IndexConstants.TableNameArchiveSiteMaster;
            public static readonly string GetArchiverSiteMasterIDsByJobId = "SELECT * FROM " + IndexConstants.TableNameArchiveSiteMaster + " WHERE COL_JOB_ID = @JobId";
            public static readonly string GetArchiverJobInfoes = "SELECT * FROM " + IndexConstants.TableNameArchiveJobInfo + " WHERE COL_JOB_ID IS NOT NULL";
            public static readonly string GetArchiverJobInfoIDsByJobId = "SELECT COL_GUID FROM " + IndexConstants.TableNameArchiveJobInfo + " WHERE COL_JOB_ID = @JobId";
            public static readonly string GetArchiverHeadCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameArchiveHead;
            public static readonly string GetArchiverBodyCount = "SELECT COUNT(*) FROM " + IndexConstants.TableNameArchiveBody;
            public static readonly string GetArchiverHeads = "SELECT * FROM " + IndexConstants.TableNameArchiveHead + " LIMIT @OFFSET, @LENGTH";
            public static readonly string GetArchiverBodys = "SELECT * FROM " + IndexConstants.TableNameArchiveBody + " LIMIT @OFFSET, @LENGTH";
            public static readonly string GetArchiverHeadCountByJobId = "SELECT COUNT(*) FROM " + IndexConstants.TableNameArchiveHead + " WHERE COL_JOBID = @JobId";
            public static readonly string GetArchiverBodyCountByJobId = "SELECT COUNT(*) FROM " + IndexConstants.TableNameArchiveBody + " WHERE COL_JOBID = @JobId";
            public static readonly string GetArchiverHeadIDsByJobId = "SELECT COL_ID FROM " + IndexConstants.TableNameArchiveHead + " WHERE COL_JOBID = @JobId LIMIT @OFFSET, @LENGTH";
            public static readonly string GetArchiverBodyIDsByJobId = "SELECT COL_ID FROM " + IndexConstants.TableNameArchiveBody + " WHERE COL_JOBID = @JobId LIMIT @OFFSET, @LENGTH";
        }


        public ArchiverRebuildIndexJobHandler(string jobId, JobType jobType)
        {
            SubJobId = jobId;
            ReportMangerFactory.Instance.Init(jobId, jobType, true);
            try
            {
                MediaConfigInfo.CommonConfigInfo = MediaServiceFactory.CreateCommonConfigInfo();
                MediaConfigInfo.ArchiverConfigInfo = MediaServiceFactory.CreateArchiverConfigInfo();
            }
            catch (Exception e)
            {
                mLog.Error($"Create Archiver Rebuild Index Service Failed. {e}");
                throw;
            }
        }

        public async Task RunAsync()
        {
            mLog.Info("Begin Rebuild Index Job.");
            try
            {
                ReportManager.StartUpdateJobProgress();
                

                InitIndexDevice();
                UpdateProgress(10, 20);

                List<RebuildIndexSiteData> rebuildIndexData = GetRebuildIndexJobData();

                int baseJobProgress = this.currentJobProgress;
                int processedSiteNum = 0;
                foreach (var siteData in rebuildIndexData)
                {
                    processedSiteNum++;
                    if (siteData ==  null)
                    {
                        mLog.Warn("Site data of Rebuild Index cannot be null.");
                        continue;
                    }
                    mLog.Info($"Start rebuild index for sitecollection: {siteData.SiteCollectionURL}");
                    if(string.IsNullOrWhiteSpace(siteData.SiteCollectionURL))
                    {
                        continue;
                    }

                    var siteId = ArchiverSiteMasterIndexDao.GetSiteIdByUrl(siteData.SiteCollectionURL);
                    var lockResult = await SampleDBLocker.TryGet4IndexDBUpdater(siteData.SiteCollectionURL, siteId, SubJobId);
                    if(lockResult.Item1)
                    {
                        using var dbLocker = lockResult.Item2;
                        RebuildIndex(siteData.SiteCollectionURL, siteData.SubIndexIDs, (baseJobProgress + 80 * processedSiteNum / rebuildIndexData.Count));
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Error($"Rebuid Index Failed. {e}.");
                mJobStatus = JobStatus.Failed;
            }
            finally 
            {
                if(mJobStatus == JobStatus.Finished)
                {
                    if (HasErrorNode && HasCompleteNode)
                    {
                        mJobStatus = JobStatus.FinishWithException;
                    }
                    else if (HasErrorNode)
                    {
                        mJobStatus = JobStatus.Failed;
                    }
                }
                
                ReportManager.SetJobFinished(mJobStatus);
                mLog.Info("Finish Rebuild Index Job.");
            }
        }

        private List<RebuildIndexSiteData> GetRebuildIndexJobData()
        {
            IRMSubJobDao SubJobDao = new RMSubJobDao();
            RMSubJob subJobWithContext = SubJobDao.GetSubJob(SubJobId, true);
            mLog.Info($"Rebuild index data : {subJobWithContext.JobContext?.Settings}");
            return SerializerHelper.DeserializeByJsonConvert<List<RebuildIndexSiteData>>(subJobWithContext.JobContext?.Settings);
        }

        private void InitIndexDevice()
        {
            var indexStroage = StorageDeviceService.GetIndexDevice();
            if (indexStroage == null)
            {
                mLog.Error("Cannot find index Storage Device.");
                mJobStatus = JobStatus.Skipped;
                return;
            }
            var indexLogicalDeviceDto = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(indexStroage);
            this.indexLogicalDevice = this.DeviceManager.Open(indexLogicalDeviceDto.GetXRIS(PhysicalDeviceUsage.Index));
            this.indexLogicalDevice.Open();

            this.cacheSetting = GetCacheSetting();
            CacheManager.Open(this.cacheSetting, this.indexLogicalDevice.IsDirectSystem);
        }

        private void OpenIndexDB(string indexVolume, string indexDBname)
        {
            var openParam = new ArchiverIndexServiceOpenParameter()
            {
                IndexDatabaseName = indexDBname,
                IndexVolume = indexVolume,
                IndexLogicalDeviceSystem = this.indexLogicalDevice,
                IndexCacheDeviceSystem = this.CacheManager.CacheSystem,
                CacheSetting = this.cacheSetting,
            };
            IndexSynchronizer.Initialize(openParam);
            this.InitIndexProcessor(openParam);
        }

        private void InitIndexProcessor(ArchiverIndexServiceOpenParameter openParam)
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
                    indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Cached, SecurityUtils.SafeCombinePath(realIndexDevice.SystemLocation, SecurityUtils.SafeCombinePath(openParam.IndexVolume, openParam.IndexDatabaseName)));
            }
            else
            {
                mLog.Info("the index file:{0}", logicalStorageInfo.ToString());
                indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Nonexistent, SecurityUtils.SafeCombinePath(realIndexDevice.SystemLocation, SecurityUtils.SafeCombinePath(openParam.IndexVolume, openParam.IndexDatabaseName)));
                if (openParam.IndexDatabaseName.Equals(ServiceConstants.IndexDBName))
                {
                    FileInfo finfo = new FileInfo(indexDownLoadInfo.IndexFullPath);
                    if (finfo.Exists)
                    {

                        mLog.Warn("The main index cache file exists but storage does not exist current main index {0} Create time{1} and delete cache index.", indexDownLoadInfo.IndexFullPath, finfo.CreationTimeUtc.ToString());
                        finfo.Delete();
                        mLog.Info($"Success delete cache index file:{indexDownLoadInfo.IndexFullPath}.");
                    }
                }
            }
            realIndexDevice.OpenDirectory(XConvert.FromNames(openParam.IndexVolume, string.Empty), FileMode.Create);
            IdentityManager.IdentityMode = IdentityMode.Process;
            ArchiverIndexProcessorParameter param = new ArchiverIndexProcessorParameter(IdentityManager.IdentityContent);
            param.DownLoadResult = indexDownLoadInfo;
            param.IndexWorkingSystem = openParam.IndexLogicalDeviceSystem;
            if (openParam.IndexDatabaseName.Equals(ServiceConstants.IndexDBName))
            {
                param.IsNeedCheckIntegrity = true;
                this.MainIndexProcessor.Open(param);
            }
            else
                this.SubIndexProcessor.Open(param);
        }


        private void RebuildIndex(string siteUrl, List<string> subIndexIDs, int upToJobProgress)
        {
            var indexVolume = this.volumeGenerator.GenerateIndexVolume(
                new VolumeParameter() { FarmName = String.Empty, SiteCollectionUrl = siteUrl });
            try
            {
                this.OpenIndexDB(indexVolume, ServiceConstants.IndexDBName);    //open main index
                var allIndexFiles = this.indexLogicalDevice.ListFiles(new StorageInfo(indexVolume, string.Empty));
                mLog.Info($"Total files in the index vlolume : {allIndexFiles.Count}");

                int baseJobProgress = this.currentJobProgress;
                int increaseJobProgress = upToJobProgress - baseJobProgress;
                int processSubIndexNum = 0;
                foreach (var subIndexId in subIndexIDs)
                {
                    processSubIndexNum++;
                    mLog.Info($"Start merge index for : {subIndexId}");
                    string subIndexDbName = $"{subIndexId}_{ServiceConstants.IndexDBName}";
                    if (!allIndexFiles.Any(i => i.Name.Equals(subIndexDbName, StringComparison.OrdinalIgnoreCase)))
                    {
                        mLog.Warn($"Sub index db not exists : {subIndexDbName}");
                        continue;
                    }

                    try
                    {
                        this.OpenIndexDB(indexVolume, subIndexDbName);  //Open sub index db

                        this.MergeIndex(subIndexId);
                    }
                    catch (Exception ex)
                    {
                        mLog.Error($"Merge index failed for sub index : {subIndexId}. {ex}");
                        this.HasErrorNode = true;
                    }
                    finally
                    {
                        this.SubIndexProcessor?.Close();
                    }

                    UpdateProgress(baseJobProgress + increaseJobProgress * processSubIndexNum / subIndexIDs.Count);
                    mLog.Info($"Finish merge index for : {subIndexId}");
                }
            }
            catch (Exception e)
            {
                mLog.Error($"Merge index failed for site : {siteUrl}. {e}");
                this.HasErrorNode = true;
            }
            finally
            {
                this.MainIndexProcessor?.Close();

                if (SiteHasMergedNode)
                {
                    var mainIndexDbInfo = new IndexDatabaseInfo(ServiceConstants.IndexDBName, null);
                    this.IndexSynchronizer.Upload(mainIndexDbInfo);
                }

                UpdateProgress(upToJobProgress);
            }
            
        }

        private void MergeIndex(string subIndexDbId)
        {
            this.InsertIntoJobInfoTable(subIndexDbId);
            this.InsertIntoSiteMasterIndex(subIndexDbId);
            this.InsertIntoHeadTable(subIndexDbId);
            this.InsertIntoBodyTable(subIndexDbId);
        }

        private void InsertIntoSiteMasterIndex(string subIndexDbId)
        {
            List<string> existsItems = this.MainIndexProcessor.ExecuteQueryForOneColume<string>(
                IndexDbQuerySQL.GetArchiverSiteMasterIDsByJobId, 
                new Dictionary<string, object>() { { "@JobId", subIndexDbId } });
            List<ArchiverSiteMasterIndex> items = this.SubIndexProcessor.ExecuteQuery<ArchiverSiteMasterIndex>(IndexDbQuerySQL.GetArchiverSiteMasters, null);
            mLog.Info($"Total site masters in sub index: {items.Count}");
            if (existsItems.Count > 0)
            {
                mLog.Info($"Exists site masters in main index: {existsItems.Count}");
                items = items.Where(i => !existsItems.Contains(i.ID)).ToList();
                mLog.Info($"Final site masters of sub index: {items.Count}");
            }

            if (items.Count > 0)
            {
                this.MainIndexProcessor.Insert(items);
                SiteHasMergedNode = true;
                HasCompleteNode = true;
            }
        }

        private void InsertIntoJobInfoTable(string subIndexDbId)
        {
            List<string> existsItems = this.MainIndexProcessor.ExecuteQueryForOneColume<string>(
                IndexDbQuerySQL.GetArchiverJobInfoIDsByJobId,
                new Dictionary<string, object>() { { "@JobId", subIndexDbId } });
            List<ArchiverJobInfoIndex> items = this.SubIndexProcessor.ExecuteQuery<ArchiverJobInfoIndex>(IndexDbQuerySQL.GetArchiverJobInfoes, null);
            mLog.Info($"Total job infoes in sub index: {items.Count}");
            if (existsItems.Count > 0)
            {
                mLog.Info($"Exists job infoes in main index: {existsItems.Count}");
                items = items.Where(i => !existsItems.Contains(i.Guid)).ToList();
                mLog.Info($"Final job infoes of sub index: {items.Count}");
            }

            if(items.Count > 0)
            {
                this.MainIndexProcessor.Insert(items);
                SiteHasMergedNode = true;
                HasCompleteNode = true;
            }
        }

        private void InsertIntoBodyTable(string subIndexDbId)
        {
            int count = Convert.ToInt32(this.SubIndexProcessor.ExecuteScalar(IndexDbQuerySQL.GetArchiverBodyCount, null));
            if (count == 0)
            {
                mLog.Warn($"No any body items. JobId: {subIndexDbId}");
                return;
            }

            var existsIDs = GetExistsIDsOfBodyOrHeadByJobId(false, subIndexDbId);
            if (existsIDs.Count > 0)
            {
                mLog.Info($"Exists bodys in main index: {existsIDs.Count}");
            }

            int offset = 0;
            Dictionary<string, object> param = new Dictionary<string, object>();
            do
            {
                param["@OFFSET"] = offset;
                param["@LENGTH"] = indexLimit;

                List<ArchiverBodyIndex> indexes = this.SubIndexProcessor.ExecuteQuery<ArchiverBodyIndex>(IndexDbQuerySQL.GetArchiverBodys, param);
                mLog.Info($"Total bodys in sub index: {indexes.Count}");
                if (existsIDs.Count > 0)
                {
                    indexes = indexes.Where(i => !existsIDs.Contains(i.Id)).ToList();
                    mLog.Info($"Final bodys of sub index: {indexes.Count}");
                }

                if (indexes.Count > 0)
                {
                    this.MainIndexProcessor.Insert(indexes);
                    SiteHasMergedNode = true;
                    HasCompleteNode = true;
                    SendJobDetails(indexes);
                }

                offset = offset + indexLimit;
            } while (offset < count);
        }

        private void InsertIntoHeadTable(string subIndexDbId)
        {
            int count = Convert.ToInt32(this.SubIndexProcessor.ExecuteScalar(IndexDbQuerySQL.GetArchiverHeadCount, null));
            if (count == 0)
            {
                mLog.Warn($"No any head items. JobId: {subIndexDbId}");
                return;
            }

            var existsIDs = GetExistsIDsOfBodyOrHeadByJobId(true, subIndexDbId);
            if(existsIDs.Count > 0)
            {
                mLog.Info($"Exists heads in main index: {existsIDs.Count}");
            }

            int offset = 0;
            Dictionary<string, object> param = new Dictionary<string, object>();
            do
            {
                param["@OFFSET"] = offset;
                param["@LENGTH"] = indexLimit;

                List<ArchiverHeadIndex> indexes = this.SubIndexProcessor.ExecuteQuery<ArchiverHeadIndex>(IndexDbQuerySQL.GetArchiverHeads, param);
                mLog.Info($"Total heads in sub index: {indexes.Count}");
                if (existsIDs.Count > 0)
                {
                    indexes = indexes.Where(i => !existsIDs.Contains(i.Id)).ToList();
                    mLog.Info($"Final heads of sub index: {indexes.Count}");
                }

                if(indexes.Count > 0)
                {
                    this.MainIndexProcessor.Insert(indexes);
                    SiteHasMergedNode = true;
                    HasCompleteNode = true;
                }

                offset = offset + indexLimit;
            } while (offset < count);
        }

        private HashSet<string> GetExistsIDsOfBodyOrHeadByJobId(bool isQueryHead, string subIndexDbId)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            param["@JobId"] = subIndexDbId;

            int count = isQueryHead
                ? Convert.ToInt32(this.SubIndexProcessor.ExecuteScalar(IndexDbQuerySQL.GetArchiverHeadCountByJobId, param))
                : Convert.ToInt32(this.SubIndexProcessor.ExecuteScalar(IndexDbQuerySQL.GetArchiverBodyCountByJobId, param));
            int offset = 0;
            var existsIDs = new HashSet<string>();
            do
            {
                param["@OFFSET"] = offset;
                param["@LENGTH"] = indexLimit;
                List<string> existsItems = this.MainIndexProcessor.ExecuteQueryForOneColume<string>(
                    isQueryHead ? IndexDbQuerySQL.GetArchiverHeadIDsByJobId : IndexDbQuerySQL.GetArchiverBodyIDsByJobId,
                    param);
                
                if(existsItems.Count > 0)
                {
                    existsItems.ForEach(i => existsIDs.Add(i));
                }
                else
                {
                    break;
                }
                offset = offset + indexLimit;
            } while (offset < count);

            return existsIDs;
        }


        private CacheSettingDto GetCacheSetting()
        {
            var archiveTemp = BackgroundSettings.GetInstance().ArchiveTemp;
            if (!System.IO.Directory.Exists(archiveTemp))
            {
                System.IO.Directory.CreateDirectory(archiveTemp);
            }

            CacheSettingDto cache = new CacheSettingDto() {
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

        private void SendJobDetails(IEnumerable<ArchiverBasicIndex> indexes)
        {
            var jobDetails = indexes.Select(i => new JMArchiverRebuildIndexJobDetails()
            {
                SiteUrl = i.SitePath,
                ObjectUrl = i.Url,
                ObjectType = i.Type,
                JobId = i.JobId,
                Status = JobDetailsStatus.Successful,
            });
            ReportManager.BatchSendJobDetail(jobDetails);
        }

        private void UpdateProgress(int min, int max)
        {
            UpdateProgress(random.Next(min, max));
        }
        private void UpdateProgress(int num)
        {
            this.currentJobProgress = Math.Max(num, this.currentJobProgress);
            ReportManager.SetProgress(this.currentJobProgress);
        }
    }
}
