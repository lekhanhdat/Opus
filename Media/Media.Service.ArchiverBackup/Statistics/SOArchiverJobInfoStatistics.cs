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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.Wrapper.Common;
using Castle.Core.Resource;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreadState = System.Threading.ThreadState;

namespace RAArchiverCommon
{
    public class SOArchiverJobInfoStatistics
    {
        public long ItemSizeSum;
        public int ItemCount;
        public long ItemSizeSumForVersion;
        public long ItemSizeSumForTelemetry;
        public long ItemCountForTelemetry=0;
        #region 为了统计item 和 version 从archive到resotre的平均时间统计，不包含attachment
        public long ItemAndVersionCountFotTelemetry;
        public long ItemAndVersionExpireSumTime;
        #endregion
        public long FileCurrentVersionCount;
        public long FileHisVersionCount;
        public long MainJobStartTime;
        public Dictionary<string, long> DriveAndDeleteSize;

        public JobType CurrentJobType;
        public int KeepDataOption;
        public string SubjobId;
        private bool needStatistics = false;

        private Thread executeThread;
        private Exception threadException;
        private ThreadState executeThreadStatus = ThreadState.Unstarted;

        private IRMJobSizeAndCountStatisticsDao mRMJobSizeAndCountStatisticsDao => PlatformWindsorManager.GetService<IRMJobSizeAndCountStatisticsDao>();
        public IJobDetailService jobSizeDetailService = (IJobDetailService) PlatformWindsorManager.GetService(typeof(IJobDetailService));
        private IRMSiteDeletedSizeInfoDao siteDeletedSizeInfoDao=> PlatformWindsorManager.GetService<IRMSiteDeletedSizeInfoDao>();
        private IRMArchiveSiteInfoDao ArchiveSiteInfoDao = PlatformWindsorManager.GetService<IRMArchiveSiteInfoDao>();
        private IRMRemoteNodeDao RemoteNodeDao = PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private IRMSODashboardMonthlySnapshotDao mRMSODashboardMonthlySnapshotDao => PlatformWindsorManager.GetService<IRMSODashboardMonthlySnapshotDao>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private static SOArchiverJobInfoStatistics instance;
        private static readonly object lockObject = new object();
        private readonly object syncStatisticDetailLockObject = new object();
        public bool IsNeedStatisticsAction;
        public bool IsDeleteOnlyActionOrRestore;
        public bool IsDeleteOnlyVersion;
        public bool IsDeleteArchiveAction;
        public bool IsArchiveBy365Action;
        public bool IncludeRelatedOrHasBackUp;
        private string realSiteUrl;
        private BaseJobDto baseJobInfo;// = new BaseJobDto() { Id = currentJobId, JobType = (int)jobType };
        private RALogger logger = RALogger.GetInstance(typeof(SOArchiverJobInfoStatistics));
        public List<JMSOJobSizeStatistics> jobDetailList = new List<JMSOJobSizeStatistics>();
        private bool isTrailLicence = false;
        private SOArchiverJobInfoStatistics() { }

        public static SOArchiverJobInfoStatistics Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new SOArchiverJobInfoStatistics();
                        }
                    }
                }
                return instance;
            }
        }

        private void RealInsert()
        {
            try
            {
                while (executeThreadStatus == ThreadState.Running)
                {
                    IEnumerable<JMSOJobSizeStatistics> commitList;
                    lock (syncStatisticDetailLockObject)
                    {
                        if (jobDetailList.Count < 500)
                        {
                            Thread.Sleep(500);
                            continue;
                        }
                        else
                        {
                            commitList = jobDetailList.Take(jobDetailList.Count);
                            jobDetailList.RemoveRange(0, jobDetailList.Count);
                        }
                    }
                    jobSizeDetailService.SyncJobDetails(commitList, baseJobInfo);
                }
                jobSizeDetailService.SyncJobDetails(jobDetailList, baseJobInfo);
                jobSizeDetailService.UploadReportFile(baseJobInfo);
            }
            catch(Exception ex)
            {
                logger.Error($@"Fail insert or upload SOArchiverJobInfoStatistic,ex:{ex}");
                threadException = ex;
            }
            finally
            {
                jobDetailList?.Clear();
                executeThreadStatus = ThreadState.Stopped;
            }
        }

        public void InitInstance(string jobId,string siteCollectionUrl, JobType jobtype,string siteId)
        {
            if(jobtype == JobType.AOSPRestore)
            {
                needStatistics = true;
            }
            else
            {
                try
                {
                    var opusLicense = RMAosApiClient.GetOPUSLicenseInformation();
                    logger.Info($"InitInstance the info,opusLicense is null?:{opusLicense == null}");
                    if (opusLicense.Extension is Cloud.Sdk.Data.AosModern.CloudRecordsExtension)
                    {
                        Cloud.Sdk.Data.AosModern.CloudRecordsExtension extension = opusLicense.Extension as Cloud.Sdk.Data.AosModern.CloudRecordsExtension;
                        if (extension.SaleType == Cloud.Sdk.Data.AosModern.SaleType.PrePaidConsumption)
                        {
                            needStatistics = true;
                        }
                        else if (opusLicense.Type == Cloud.Sdk.Data.AosModern.LicenseType.Trial)
                        {
                            isTrailLicence = true;
                            needStatistics = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($@"Fail to get OPUS license information,ex:{ex}");
                    needStatistics = true;
                }
            }
            logger.Info($"InitInstance.JobID:{jobId}.JobType:{jobtype}.SiteID:{siteId}.SiteURL:{siteCollectionUrl}.");
            realSiteUrl = siteCollectionUrl;
            siteCollectionUrl = siteCollectionUrl.Replace('\\', '#').Replace(':', '#').Replace('/', '#').Replace('*','#').Replace('?','#').Replace('\"','#').Replace('<','#').Replace('>', '#').Replace('|', '#');
            this.baseJobInfo = new BaseJobDto() { Id = jobId, SiteCollectionUrl = siteCollectionUrl,JobType = (int)JobType.None,JobtypeString = jobtype.ToString(),SiteId = siteId };
            this.CurrentJobType = jobtype;
            this.SubjobId = jobId;
            StartInsertThreadIfNeedStatistics();
        }

        public void InitAOSPInstance(string jobId, string siteCollectionUrl, JobType jobtype, string siteId)
        {
            needStatistics = true;
            realSiteUrl = siteCollectionUrl;
            siteCollectionUrl = siteCollectionUrl.Replace('\\', '#').Replace(':', '#').Replace('/', '#').Replace('*', '#').Replace('?', '#').Replace('\"', '#').Replace('<', '#').Replace('>', '#').Replace('|', '#');
            this.baseJobInfo = new BaseJobDto() { Id = jobId, SiteCollectionUrl = siteCollectionUrl, JobType = (int)JobType.None, JobtypeString = jobtype.ToString(), SiteId = siteId };
            this.CurrentJobType = jobtype;
            this.SubjobId = jobId;
            StartInsertThreadIfNeedStatistics();
        }

        public void StartInsertThreadIfNeedStatistics()
        {
            if (!needStatistics) 
            {
                return;
            }
            lock (syncStatisticDetailLockObject)
            {
                executeThreadStatus = ThreadState.Running;
            }
            executeThread = new Thread(RealInsert) { IsBackground = true };
            executeThread.Start();
        }

        public void AccumulationItemsSize(long nodeSize, string url)
        {
            ItemSizeSum += nodeSize;
            ItemCount++;
            AddSOJobSizeStatistics(nodeSize, url);
        }
        public void AccumulationItemsSizeForVersion(long nodeSize, string url)
        {
            ItemSizeSumForVersion += nodeSize;
            ItemCount++;
            AddSOJobSizeStatistics(nodeSize, url);
        }
        public void AccumulationArchiveBy365ItemsSize(long nodeSize, string url)
        {
            ItemSizeSum += nodeSize;
            ItemCount++;
        }
        public void AccumulationItemsSize(long nodeSize, string url, RMDiscoveryOptimizationFileReport fileInfo)
        {
            ItemSizeSum += nodeSize;
            ItemCount++;
            AddSOJobSizeStatistics(nodeSize, url, fileInfo);
        }
        private void AddSOJobSizeStatistics(long nodeSize, string url, RMDiscoveryOptimizationFileReport fileInfo = null)
        {
            if (needStatistics)
            {
                lock (syncStatisticDetailLockObject)
                {
                    jobDetailList.Add(GenarateSOJobSizeStatistics(nodeSize, url, fileInfo));
                }
            }
        }
        private JMSOJobSizeStatistics GenarateSOJobSizeStatistics(long nodeSize, string url,RMDiscoveryOptimizationFileReport fileInfo=null)
        {
            JMSOJobSizeStatistics result = new JMSOJobSizeStatistics();
            result.SourceLocation = url;
            result.FinishTime = DateTime.UtcNow.Ticks.ToString();
            result.Size = nodeSize.ToString();
            result.KeepDataOption = this.KeepDataOption;
            result.Action = this.CurrentJobType.ToString();
            if (fileInfo != null)
            {
                result.AuthorID = fileInfo.AuthorID;
                result.AuthorEmail = fileInfo.AuthorEmail;
                result.ModifiedID = fileInfo.ModifiedID;
                result.ModifiedEmail = fileInfo.ModifiedEmail;
                result.CreateTime = fileInfo.CreateTime;
                result.ModifiedTime = fileInfo.ModifiedTime;
                result.VersionCount = fileInfo.VersionCount;
            }
            //result.FinishTime = DateTime.UtcNow.Ticks;
            //result.Status = (JobDetailsStatus)status;
            return result;
        }
        public void SaveInfoToDB()
        {
            try
            {
                bool snapshotIsDeleteOnlyActionOrRestore = this.IsDeleteOnlyActionOrRestore;
                bool snapshotIsDeleteArchiveAction = this.IsDeleteArchiveAction;
                long snapshotItemSizeSum = this.ItemSizeSum;

                logger.Info($"current rule total size is:{this.ItemSizeSum},count:{this.ItemCount}");
                if (needStatistics)
                {
                    if (this.IsDeleteOnlyVersion)
                    {
                        logger.Info($"current rule total version size is:{this.ItemSizeSumForVersion}");
                        mRMJobSizeAndCountStatisticsDao.AddJobStatisticsAsync(this.CurrentJobType, this.KeepDataOption, this.ItemSizeSumForVersion, this.SubjobId, this.baseJobInfo.SiteId, isTrailLicence).GetAwaiter().GetResult();
                        this.IsDeleteOnlyVersion = false;
                    }
                    if (this.IsDeleteOnlyActionOrRestore)
                    {
                        mRMJobSizeAndCountStatisticsDao.AddJobStatisticsAsync(this.CurrentJobType, this.KeepDataOption, this.ItemSizeSum, this.SubjobId, this.baseJobInfo.SiteId, isTrailLicence).GetAwaiter().GetResult();
                        this.IsDeleteOnlyActionOrRestore = false;
                    }
                    WaitInsertThreadExit();
                }
                else
                {
                    logger.Warn("this user licence module is not pre paid ,so not update db");
                }
                if (this.IsDeleteArchiveAction)
                {
                    CreateDeleteSizeInfo();
                    this.IsDeleteArchiveAction = false;
                }
                if (this.IsArchiveBy365Action)
                {
                    CreateArchiveBy365SizeInfo();
                    this.IsArchiveBy365Action = false;
                }

                UpsertMonthlySnapshot(snapshotIsDeleteOnlyActionOrRestore, snapshotIsDeleteArchiveAction, snapshotItemSizeSum);

                this.ItemSizeSum = 0;
                this.ItemCount = 0;
            }
            catch (Exception ex)
            {
                logger.Error($"some thing went wrong when save total size to RMJobSizeAndCountStatistics,error:{ex.ToString()}");
            }
        }

        private void UpsertMonthlySnapshot(bool isDeleteOnlyActionOrRestore, bool isDeleteArchiveAction, long itemSizeSum)
        {
            logger.Info($"[DEBUG] UpsertMonthlySnapshot called. " +
                $"isDeleteOnlyActionOrRestore:{isDeleteOnlyActionOrRestore}, " +
                $"isDeleteArchiveAction:{isDeleteArchiveAction}, " +
                $"itemSizeSum:{itemSizeSum}, " +
                $"JobType:{CurrentJobType}, " +
                $"KeepDataOption:{KeepDataOption}");
            try
            {
                var subJobId = this.SubjobId;
                var subJobTenantId = string.IsNullOrWhiteSpace(subJobId) ? string.Empty : SubJobDao.GetSubJob(subJobId)?.O365TenantId;
                var o365TenantId = RemoteNodeDao.GetRemoteSiteCollectionByUrl(realSiteUrl)?.TenantId
                    ?? (string.IsNullOrWhiteSpace(subJobTenantId) ? null : subJobTenantId)
                    ?? WrapperConfiguration.WrapperConfigurationForBPOS.O365TenantId.ToString();

                var period = DateTime.UtcNow.ToString("yyyyMM");
                bool isOneDrive = realSiteUrl.Contains("-my.sharepoint.com");
                
                // Check if data exists in archived storage to distinguish between Case 2 and Case 3
                bool hasArchivedData = HasArchivedData(subJobId);
                
                bool isRestoreJob = CurrentJobType == JobType.AOSPRestore || KeepDataOption == -2;

                if (isRestoreJob) return;

                long spoArchivedSize = 0, odArchivedSize = 0;
                long spoDestroyedFromArchiveSize = 0, odDestroyedFromArchiveSize = 0;
                long spoDestroyedFromLiveSize = 0, odDestroyedFromLiveSize = 0;

                // Case 1: Archive action
                if (isDeleteArchiveAction && !isDeleteOnlyActionOrRestore)
                {
                    long physicalSize = GetPhysicalSizeFromIndex(subJobId); 
                    long archiveSize = physicalSize > 0 ? physicalSize : itemSizeSum; 

                    if (isOneDrive) odArchivedSize = archiveSize;
                    else spoArchivedSize = archiveSize;
                }
                // Case 2: Destroyed from archived storage
                else if (isDeleteOnlyActionOrRestore && hasArchivedData)
                {
                    if (isOneDrive) odDestroyedFromArchiveSize = itemSizeSum;
                    else spoDestroyedFromArchiveSize = itemSizeSum;
                }
                // Case 3: Destroyed from live storage
                else if (isDeleteOnlyActionOrRestore && !hasArchivedData)
                {
                    if (isOneDrive) odDestroyedFromLiveSize = itemSizeSum;
                    else spoDestroyedFromLiveSize = itemSizeSum;
                }
                else
                {
                    return;
                }

                logger.Info($"UpsertMonthlySnapshot. TenantId:{o365TenantId}, Period:{period}, IsOneDrive:{isOneDrive}, " +
                            $"SpoArchived:{spoArchivedSize}, OdArchived:{odArchivedSize}, " +
                            $"SpoDestroyedArchive:{spoDestroyedFromArchiveSize}, OdDestroyedArchive:{odDestroyedFromArchiveSize}, " +
                            $"SpoDestroyedLive:{spoDestroyedFromLiveSize}, OdDestroyedLive:{odDestroyedFromLiveSize}");

                mRMSODashboardMonthlySnapshotDao.UpsertMonthlySnapshotAsync(
                    o365TenantId, period,
                    spoArchivedSize, odArchivedSize,
                    spoDestroyedFromArchiveSize, odDestroyedFromArchiveSize,
                    spoDestroyedFromLiveSize, odDestroyedFromLiveSize
                ).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.Error($"UpsertMonthlySnapshot failed, error:{ex}");
            }
        }
        private long GetPhysicalSizeFromIndex(string subJobId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(subJobId)) return 0;

                var archiverIndexSubInfoDao = PlatformWindsorManager
                    .GetService<IArchiverIndexSubInfoDao>();

                var subInfos = archiverIndexSubInfoDao.GetSubInfoesBySubJobId(subJobId);

                if (subInfos == null || subInfos.Count == 0)
                {
                    logger.Info($"GetPhysicalSizeFromIndex: no records for SubJobId:{subJobId}");
                    return 0;
                }

                long physicalSize = subInfos.Sum(s => s.MediaDataSize);
                logger.Info($"GetPhysicalSizeFromIndex: SubJobId:{subJobId}, " +
                            $"PhysicalSize:{physicalSize}, Records:{subInfos.Count}");
                return physicalSize;
            }
            catch (Exception ex)
            {
                logger.Error($"GetPhysicalSizeFromIndex failed: SubJobId:{subJobId}, error:{ex}");
                return 0;
            }
        }

        /// <summary>
        /// Check if SubJobId has data in ArchiverIndexSubInfoes table (archived storage)
        /// Only used to determine if delete is from archive or live storage
        /// </summary>
        private bool HasArchivedData(string subJobId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(subJobId))
                {
                    return false;
                }

                var archiverIndexSubInfoDao = PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
                var subInfos = archiverIndexSubInfoDao.GetSubInfoesBySubJobId(subJobId);

                logger.Info($"HasArchivedData for SubJobId:{subJobId}, found {subInfos?.Count ?? 0} records in ArchiverIndexSubInfoes");
                
                return subInfos != null && subInfos.Count > 0;
            }
            catch (Exception ex)
            {
                logger.Error($"HasArchivedData failed for SubJobId:{subJobId}, error:{ex}");
                return false;
            }
        }        
        private void WaitInsertThreadExit()
        {
            lock (syncStatisticDetailLockObject)
            {
                executeThreadStatus = ThreadState.StopRequested;
            }
            while (true)
            {
                lock (syncStatisticDetailLockObject)
                {
                    if (executeThreadStatus == ThreadState.Stopped || executeThreadStatus == ThreadState.Unstarted || executeThread == null || !executeThread.IsAlive)
                    {
                        break;
                    }
                }
                Thread.Sleep(500);
            }
            if (threadException != null) 
            {
                Exception exception = new Exception("Exception accure when insert SOArchiverJobInfoStatistic", threadException);
                threadException = null;
                throw exception;
            }
        }

        private void CreateDeleteSizeInfo()
        {
            logger.Info($"current rule deleted total size is:{this.ItemSizeSum},count:{this.ItemCount}");
            siteDeletedSizeInfoDao.CreateInfo(new RMSiteDeletedSizeInfo()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.UtcNow.Ticks,
                SiteId = this.baseJobInfo.SiteId,
                SiteUrl = realSiteUrl,
                JobId = SubjobId,
                DeletedSize = ItemSizeSum,
                DeletedFile = ItemCount
            });
            var subJobId = this.SubjobId;
            var subJobTenantId = string.IsNullOrWhiteSpace(subJobId) ? string.Empty : SubJobDao.GetSubJob(subJobId)?.O365TenantId;
            var o365TenantId = RemoteNodeDao.GetRemoteSiteCollectionByUrl(realSiteUrl)?.TenantId
                ?? (string.IsNullOrWhiteSpace(subJobTenantId) ? null : subJobTenantId)
                ?? WrapperConfiguration.WrapperConfigurationForBPOS.O365TenantId.ToString();
            logger.Info($"CreateDeleteSizeInfo resolved O365TenantId. SiteId:{this.baseJobInfo.SiteId}, SiteUrl:{realSiteUrl}, SubJobId:{subJobId}, subJobTenantId:{subJobTenantId}, O365TenantId:{o365TenantId}");
            ArchiveSiteInfoDao.CreateOrUpdateDeletedInfo(realSiteUrl, this.ItemSizeSum, this.baseJobInfo.SiteId, o365TenantId, this.ItemCount);
        }
        private void CreateArchiveBy365SizeInfo()
        {
            logger.Info($"current rule archive by 365 total size is:{this.ItemSizeSum},count:{this.ItemCount}");
            var subJobId = this.SubjobId;
            var subJobTenantId = string.IsNullOrWhiteSpace(subJobId) ? string.Empty : SubJobDao.GetSubJob(subJobId)?.O365TenantId;
            var o365TenantId = RemoteNodeDao.GetRemoteSiteCollectionByUrl(realSiteUrl)?.TenantId
                ?? (string.IsNullOrWhiteSpace(subJobTenantId) ? null : subJobTenantId)
                ?? WrapperConfiguration.WrapperConfigurationForBPOS.O365TenantId.ToString();
            logger.Info($"CreateArchiveBy365SizeInfo resolved O365TenantId. SiteId:{this.baseJobInfo.SiteId}, SiteUrl:{realSiteUrl}, SubJobId:{subJobId}, subJobTenantId:{subJobTenantId}, O365TenantId:{o365TenantId}");
            ArchiveSiteInfoDao.CreateOrUpdateArchiveBy365Info(realSiteUrl, this.ItemSizeSum, this.baseJobInfo.SiteId, o365TenantId);
        }
    }
}
