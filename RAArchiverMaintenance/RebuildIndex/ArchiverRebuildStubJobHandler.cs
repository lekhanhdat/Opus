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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.ArchiverCommon;
using RAArchiverCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Archiver;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Contract.MediaManagement.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Common.Util;
using Media.Service.DomainModel;
using AvePoint.RA.Contract.RMWeb.Setting;
using RazorEngine.Templating;

namespace RAArchiverMaintenance.RebuildIndex
{
    public class ArchiverRebuildStubJobHandler
    {
        private static IRALogger mLog = new RALogger(typeof(ArchiverRebuildStubJobHandler));
        private string SubJobId = string.Empty;
        private string JobContextSetting = string.Empty;
        private AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Finished;
        private bool HasCompleteNode { get; set; }
        private bool HasErrorNode { get; set; }
        private bool HasStop { get; set; }

        private IRebuildStubService rebuildStubService;
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        private IStubSettingService StubSettingServiceDao = ((IStubSettingService)PlatformWindsorManager.GetService(typeof(IStubSettingService)));

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
        public ArchiverRebuildStubJobHandler(string jobId, JobType jobType)
        {
            SubJobId = jobId;
            ReportMangerFactory.Instance.Init(jobId, jobType, true);
            try
            {
                rebuildStubService = MediaServiceFactory.CreateArchiverRebuildStubService();
                MediaConfigInfo.CommonConfigInfo = MediaServiceFactory.CreateCommonConfigInfo();
                MediaConfigInfo.ArchiverConfigInfo = MediaServiceFactory.CreateArchiverConfigInfo();
            }
            catch (Exception e)
            {
                mLog.Error("Create Archiver Rebuild Stub Service Failed.Failed Message:{0}", e.ToString());
                throw;
            }

        }

        public async Task RunAsync()
        {
            mLog.Info("Begin Rebuild Stub Job.");
            ReportManager.StartUpdateJobProgress();
            IRMSubJobDao SubJobDao = new RMSubJobDao();
            IJobMonitorDao JobMonitorDao = new JobMonitorDao();
            //从子job的Context中获取当前需要处理的节点.
            RMSubJob subJobWithContext = SubJobDao.GetSubJob(SubJobId, true);
            JobContextSetting = subJobWithContext.JobContext?.Settings;
            RebuildStubInfo rebuildStubInfo = SerializerHelper.DeserializeByDataContractSerializer<RebuildStubInfo>(JobContextSetting);

            try
            {
                var indeDevice = StorageDeviceService.GetIndexDevice();
                if (indeDevice == null)
                {
                    mLog.Error("Cannot find inde Device.");
                    ReportManager.SetJobFinished(AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Skipped);
                    return;
                }
                var indexLogical = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(indeDevice);
                List<AvePoint.RA.DB.Model.ArchiverSiteMasterIndex> allSiteMasterIndexs = ArchiverSiteMasterIndexDao.GetAllSiteCollectionNodsInfo();
                var rebuildJobMasterIndex = allSiteMasterIndexs.Where(x => x.JobId.StartsWith(rebuildStubInfo.RebuildJobId)).ToList();
                mLog.Info($"Init Rebuild Stub Job.Index Device Name:{indeDevice.Name}." +
                    $"Rebuild Index Count:{rebuildJobMasterIndex.Count}." +
                    $"Rebuild Job Id:{rebuildStubInfo.RebuildJobId}." +
                    $"Rebuild Stub Template Name:{rebuildStubInfo.StubTemplateName}.");
                var ArchiveTemp = BackgroundSettings.GetInstance().ArchiveTemp;
                if (!System.IO.Directory.Exists(ArchiveTemp))
                {
                    Directory.CreateDirectory(ArchiveTemp);
                }
                CacheSettingDto cacheSettingDto = InitCacheSetting(ArchiveTemp);
                AvePoint.GCommon.Contract.Server.StubSetting.StubSettingDto template = await StubSettingServiceDao.GetStubTemplateByNameAsync(rebuildStubInfo.StubTemplateName);
                foreach (AvePoint.RA.DB.Model.ArchiverSiteMasterIndex index in rebuildJobMasterIndex)
                {
                    try
                    {
                        mLog.Info($"Begin Rebuild Stub subjob:{index.JobId}.");
                        var mArchiverRebuidStubInfo = new ArchiverRebuildStubInfo(index, indexLogical, cacheSettingDto, rebuildStubInfo, template);
                        var retentionService = (IRebuildStubService)PlatformWindsorManager.GetService("AvePoint.Media.Service.ArchiverBackup.ArchiverRebuildStubService", typeof(IRebuildStubService));
                        retentionService.RebuildStub(mArchiverRebuidStubInfo, SendJobReport);
                        mLog.Info($"Finish Rebuild Stub subjob:{index.JobId}.");
                    }
                    catch (Exception ex)
                    {
                        mLog.Error($"Rebuid Stub Failed.Message:{ex}.SiteUrl:{index.SiteURL}.JobId:{index.JobId}.");
                        mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.FinishWithException;
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Error($"Rebuid Stub Failed.Message:{e}.");
                mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Failed;
                ReportManager.SetJobFinished(AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Failed);
            }
            finally 
            {
                ReportManager.SetJobFinished(mJobStatus);
                mLog.Info("Finish Rebuild Stub Job.");
            }
        }

        private CacheSettingDto InitCacheSetting(string path)
        {
            string cachePath = path;
            CacheSettingDto cache = new CacheSettingDto();
            cache.Extension = new CacheSettingExtension();
            cache.Extension.Path = new List<PathMap>();
            cache.Extension.Path.Add(new PathMap() { DiskInfo = new DiskInfoDto() });
            cache.Extension.Path[0].DiskInfo.Path = cachePath;
            return cache;
        }

        private void SendJobReport(JMArchiverRebuildStubJobDetails rententionJobDetails)
        {
            AnalyzeStatus(rententionJobDetails.Status);
            ReportManager.SendJobDetail(rententionJobDetails);
        }

        private void AnalyzeStatus(JobDetailsStatus status)
        {
            if (status == JobDetailsStatus.Successful || status == JobDetailsStatus.Skipped)
            {
                HasCompleteNode = true;
            }
            else if (status == JobDetailsStatus.Failed)
            {
                HasErrorNode = true;
            }
        }
    }
}
