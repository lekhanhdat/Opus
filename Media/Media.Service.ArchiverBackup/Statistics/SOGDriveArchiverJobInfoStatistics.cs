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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;

namespace Media.Service.ArchiverBackup.Statistics
{
    public class SOGDriveArchiverJobInfoStatistics
    {
        public long ItemSizeSum;
        public int ItemCount;
        public long ItemSizeSumForVersion;
        public long ItemSizeSumForTelemetry;
        public long ItemCountForTelemetry = 0;
        public long ItemAndVersionCountFotTelemetry;
        public long ItemAndVersionExpireSumTime;
        public long FileCurrentVersionCount;
        public long FileHisVersionCount;
        public long MainJobStartTime;
        private long ControlPlusDeletedNumber = 0;
        public Dictionary<string, long> DriveAndDeleteSize;

        public JobType CurrentJobType;
        public int KeepDataOption;
        public string SubjobId;
        private bool needStatistics = false;

        private Thread executeThread;
        private Exception threadException;
        private ThreadState executeThreadStatus = ThreadState.Unstarted;

        private IRMJobSizeAndCountStatisticsDao _jobSizeAndCountStatisticsDao => PlatformWindsorManager.GetService<IRMJobSizeAndCountStatisticsDao>();

        private IJobDetailService _jobSizeDetailService = (IJobDetailService)PlatformWindsorManager.GetService(typeof(IJobDetailService));
        private IRMGDriveDeletedSizeInfoDao _gDriveDeletedSizeInfoDao => PlatformWindsorManager.GetService<IRMGDriveDeletedSizeInfoDao>();
        private IRMArchiveGDriveInfoDao _archiveGDriveInfoDao => PlatformWindsorManager.GetService<IRMArchiveGDriveInfoDao>();

        private IRMRemoteNodeDao _remoteNodeDao = PlatformWindsorManager.GetService<IRMRemoteNodeDao>();



        private static SOGDriveArchiverJobInfoStatistics instance;
        private static readonly object lockObject = new object();
        private readonly object syncStatisticDetailLockObject = new object();
        public bool IsNeedStatisticsAction;
        public bool IsDeleteOnlyActionOrRestore;
        public bool IsDeleteOnlyVersion;
        public bool IsDeleteArchiveAction;
        public bool IncludeRelatedOrHasBackUp;
        private string realSiteUrl;
        private BaseJobDto baseJobInfo;// = new BaseJobDto() { Id = currentJobId, JobType = (int)jobType };
        private RALogger logger = RALogger.GetInstance(typeof(SOGDriveArchiverJobInfoStatistics));
        public List<JMSOJobSizeStatistics> jobDetailList = new List<JMSOJobSizeStatistics>();
        private bool isTrailLicence = false;
        private SOGDriveArchiverJobInfoStatistics() { }

        public static SOGDriveArchiverJobInfoStatistics Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new SOGDriveArchiverJobInfoStatistics();
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
                    _jobSizeDetailService.SyncJobDetails(commitList, baseJobInfo);
                }
                _jobSizeDetailService.SyncJobDetails(jobDetailList, baseJobInfo);
                _jobSizeDetailService.UploadReportFile(baseJobInfo);
            }
            catch (Exception ex)
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

        public void InitGDriveInstance(string jobId, string driveName, JobType jobtype, string driveId)
        {
            var opusLicense = RMAosApiClient.GetOPUSLicenseInformation();
            
            if (opusLicense?.Extension is Cloud.Sdk.Data.AosModern.CloudRecordsExtension)
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

            realSiteUrl = driveName;
            this.baseJobInfo = new BaseJobDto() { Id = jobId, SiteCollectionUrl = driveName, JobType = (int)JobType.None, JobtypeString = jobtype.ToString(), SiteId = driveId };
            this.CurrentJobType = jobtype;
            this.SubjobId = jobId;
            //StartInsertThreadIfNeedStatistics();
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
        public void AccumulationDeletedNumber()
        {
            _ = Interlocked.Increment(ref ControlPlusDeletedNumber);
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
        private JMSOJobSizeStatistics GenarateSOJobSizeStatistics(long nodeSize, string url, RMDiscoveryOptimizationFileReport fileInfo = null)
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
            return result;
        }

        #region
        public void SaveInfoToGDriveDB()
        {
            try
            {
                logger.Info($"current rule total size is:{this.ItemSizeSum},count:{this.ItemCount}");
                if (needStatistics)
                {
                    if (this.IsDeleteOnlyVersion)
                    {
                        logger.Info($"current rule total version size is:{this.ItemSizeSumForVersion}");
                        _jobSizeAndCountStatisticsDao.AddJobStatisticsAsync(this.CurrentJobType, this.KeepDataOption, this.ItemSizeSumForVersion, this.SubjobId, this.baseJobInfo.SiteId, isTrailLicence).GetAwaiter().GetResult();
                        this.IsDeleteOnlyVersion = false;
                    }
                    if (this.IsDeleteOnlyActionOrRestore)
                    {
                        _jobSizeAndCountStatisticsDao.AddJobStatisticsAsync(this.CurrentJobType, this.KeepDataOption, this.ItemSizeSum, this.SubjobId, this.baseJobInfo.SiteId, isTrailLicence).GetAwaiter().GetResult();
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
                    CreateGDriveDeleteSizeInfo();
                    this.IsDeleteArchiveAction = false;
                }
                this.ItemSizeSum = 0;
                this.ItemCount = 0;
            }
            catch (Exception ex)
            {
                logger.Error($"some thing went wrong when save total size to RMJobSizeAndCountStatistics,error:{ex.ToString()}");
            }
        }
        #endregion

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
                Exception exception = new Exception("Exception accure when insert SOGDriveArchiverJobInfoStatistic", threadException);
                threadException = null;
                throw exception;
            }
        }
        private void CreateGDriveDeleteSizeInfo()
        {
            logger.Info($"current rule deleted total size is:{this.ItemSizeSum},count:{this.ItemCount}");
            var tenantId = _remoteNodeDao.GetTenantIdByObjectId(this.baseJobInfo.SiteId);
            _gDriveDeletedSizeInfoDao.CreateInfo(new RMGDriveDeletedSizeInfo()
            {
                Id = Guid.NewGuid().ToString(),
                CreateTime = DateTime.UtcNow.Ticks,
                DriveId = this.baseJobInfo.SiteId,
                DriveName = realSiteUrl,
                JobId = SubjobId,
                DeletedSize = ItemSizeSum,
                TenantId = tenantId
            });
            _archiveGDriveInfoDao.CreateOrUpdateDeletedInfo(realSiteUrl, this.ItemSizeSum, this.baseJobInfo.SiteId, tenantId, this.ControlPlusDeletedNumber);
        }
    }
}
