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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Media.Service;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using RAArchiverCommon;
using Google.Apis.Storage.v1;
using AvePoint.Media.Common;

namespace RAArchiverMaintenance
{
    public class ArchiverMoveIndexJobHandler
    {
        private static IRALogger mLog = new RALogger(typeof(ArchiverMoveIndexJobHandler));
        private string SubJobId  = string.Empty;
        private string MainJobId = string.Empty;
        private string JobContextSetting = string.Empty;
        private string JobContextContent = string.Empty;
        private JobType mJobType;

        private IMoveIndexService moveIndexService;
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        private IArchiverDedupInfoDao ArchiverDedupInfoDao => PlatformWindsorManager.GetService<IArchiverDedupInfoDao>();
        private IFSMasterIndexDao FSMasterIndexDao => PlatformWindsorManager.GetService<IFSMasterIndexDao>();
        private ICommonSiteMasterIndexDao TeamsMasterIndexDao => PlatformWindsorManager.GetService<ICommonSiteMasterIndexDao>();
        private IEXOArchiverIndexSubInfoDao ExchangeMasterIndexDao => PlatformWindsorManager.GetService<IEXOArchiverIndexSubInfoDao>();

        private IFSIndexSubInfoDao FSIndexSubInfoDao => PlatformWindsorManager.GetService<IFSIndexSubInfoDao>();

        private IRMReportManager mReportManger;

        private IRMReportManager ReportManager
        {
            get
            {
                if (mReportManger == null)
                {
                    mReportManger = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManger;
            }
        }
        public ArchiverMoveIndexJobHandler(string jobId, JobType jobType)
        {
            SubJobId = jobId;
            mJobType = jobType;
            ReportMangerFactory.Instance.Init(jobId, jobType, true);
            try
            {
                moveIndexService = MediaServiceFactory.CreateArchiverMoveIndexService();
            }
            catch (Exception e)
            {
                mLog.Error("Create Archiver Move Index Service Failed.Failed Message:{0}", e.ToString());
                throw;
            }

        }

        public List<FSMasterIndexContract> GetAllFSIndexInfos()
        {
            return FSMasterIndexDao.GetAllConnectionsInfo();
        }

        public List<string> GetAllTeamsIndexInfos()
        {
            return TeamsMasterIndexDao.GetAllTeamIndexInfoes().Select(t=>t.SiteURL).Distinct().ToList();
        }

        public List<string> GetAllExchangeIndexInfos()
        {
            return ExchangeMasterIndexDao.GetAllArchiverIndexSubInfo().Select(t => t.MailBoxAddress).Distinct().ToList();
        }
        public List<ArchiverSiteMasterIndexContract> GetAllGoogleDriveIndexInfos()
        {
            return ArchiverSiteMasterIndexDao.GetAllBackupGoogleDriveIndexs();
        }
        private List<string> GetAllBackupSiteURLs()
        {
            var allSiteURLs = new HashSet<string>();
            foreach (var url in ArchiverSiteMasterIndexDao.GetAllBackupSiteCollectionDistinctUrl())
            {
                allSiteURLs.Add(url);
            }

            foreach (var url in ArchiverDedupInfoDao.GetAllDedupCollections())
            {
                allSiteURLs.Add(url);
            }

            return allSiteURLs.ToList();
        }

        public async Task PerformArchiverMoveIndexJobAsync()
        {
            ReportManager.StartUpdateJobProgress();
            IRMSubJobDao SubJobDao = new RMSubJobDao();
            IJobMonitorDao JobMonitorDao = new JobMonitorDao();
            //从子job的Context中获取当前需要处理的节点.
            RMSubJob subJobWithContext = SubJobDao.GetSubJob(SubJobId, true);
            MainJobId = subJobWithContext.ParentId;
            JobContextSetting = subJobWithContext.JobContext?.Settings;
            JobContextContent = subJobWithContext.JobContext?.Content;
            RMArchiverMoveIndexInfo moveIndexSettings = SerializerHelper.DeserializeByDataContractSerializer<RMArchiverMoveIndexInfo>(JobContextSetting);
            try
            {
                var srcStorageDevice = StorageDeviceService.GetStorageDeviceById(moveIndexSettings.SrcIndexDeviceId, needDecryptSecert: true);
                var srcLogical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(srcStorageDevice);

                var destStorageDevice = StorageDeviceService.GetStorageDeviceById(moveIndexSettings.DestIndexDeviceId, needDecryptSecert: true);
                ArchiverCommonStaticMethod.VerifyAvePoint(destStorageDevice);
                var destLogical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(destStorageDevice);

                var siteUrls = GetAllBackupSiteURLs();

                ArchiverMoveIndexJobInfo jobInfo = new ArchiverMoveIndexJobInfo();
                jobInfo.DestinationDevice = destLogical;
                jobInfo.FarmName = string.Empty;
                jobInfo.IndexLogicalDevice = srcLogical;
                jobInfo.JobId = MainJobId;
                jobInfo.SubJobId = SubJobId;
                jobInfo.SiteUrls = siteUrls;
                jobInfo.TeamsSiteUrls = GetAllTeamsIndexInfos();
                jobInfo.ExchangeSiteUrls = GetAllExchangeIndexInfos();
                jobInfo.GDriveIndexInfos = GetAllGoogleDriveIndexInfos();
                jobInfo.FSIndexInfos = GetAllFSIndexInfos()
                    .GroupBy(index => index.ConnectionId)
                    .Select(group => group
                      .OrderByDescending(index => index.ArchiverTime).First()).ToList();
                jobInfo.WebApp = string.Empty;
                jobInfo.CacheSetting = new CacheSettingDto { Extension = new CacheSettingExtension { Path = new List<PathMap>() } };
                DiskInfoDto disk = new DiskInfoDto()
                {
                    Path = BackgroundSettings.GetInstance().ArchiveCache,
                    Type = DeviceType.LocalPath,
                    Password = null,
                    UserName = string.Empty,
                    Usage = null
                };
                jobInfo.CacheSetting.Extension.Path.Add(new PathMap() { DiskInfo = disk });
                jobInfo.CacheSetting.LimitFreeSpace = 1024 * 1024 * 1024;//1 GB
                var moveIndexInfo = new ArchiverMoveIndexInfo(jobInfo);
                var result = await moveIndexService.MoveIndexAsync(moveIndexInfo);
            }
            catch (LicenseMismatchOfAvePointStorageException lme)
            {
                mLog.Error("Move Index Failed:{0}", lme.ToString());
                ReportManager.SetJobFinished(AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Failed, lme.Message);
            }
            catch (Exception e)
            {
                mLog.Error("Move Index Failed:{0}", e.ToString());
                ReportManager.SetJobFinished(AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Failed);
            }

        }
    }
}
