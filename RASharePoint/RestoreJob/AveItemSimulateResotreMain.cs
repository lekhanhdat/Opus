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
using System;
using System.IO;
using System.Text;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.GranularRestore.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.FileTransfer;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Contract.Server.GranularRestore.Object;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.Media.Common;
using AvePoint.Media.Service.DomainModel;
using System.Collections.Generic;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
using AvePoint.Media.Service.ArchiverBackup.Restore;
using AvePoint.GCommon.Contract.Media.TCPRequest;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.GCommon.Contract.CloudServiceCommon;
using AvePoint.ObjectModel.Common;
using System.Web;
using System.Globalization;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.Cryptography;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Archiver.Media;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.SharePoint.RestoreJob.Restore.Content;
using RestoreType = AvePoint.GCommon.Contract.Server.Common.BackupDataSearch.RestoreType;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Common;
using System.Linq;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.GCommon.Contract.Server.ControlPanel.SecurityInformationManager;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Common.Report;
using RecordsHotfixMaintenanceService;
using AvePoint.GCommon.Contract.GranularBackup.Object;
using BackupLevel = AvePoint.GCommon.Contract.GranularBackup.Object.BackupLevel;
using Aspose.Email.PersonalInfo;
using ItemDependencyOption = AvePoint.GCommon.Contract.Server.GranularRestore.Object.ItemDependencyOption;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.RACommonUtility.Encryption;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Utility.Cryptography.Encryption.KeyVault;
using AvePoint.Common.Portal;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.RMWeb.Setting;
using System.Threading.Tasks;
using AvePoint.RA.Common.Util;
using RAArchiverCommon;
using PnP.Framework.Diagnostics;
using Aspose.Pdf.Operators;
using AvePoint.RA.RACommonUtility.Telemetry;
using RAExportCommon;
using Cloud.Sdk.EDiscovery.Services;
using Media.Service.ArchiverBackup.Restore;
using AvePoint.Media.Core.IO;
using AvePoint.RA.Contract.Common;
using static AvePoint.GCommon.Utility.I18N.ContextValues.Configuration;
using AvePoint.RA.CommonUtil;
using AvePoint.Item.Restore;
using log4net;
using System.Threading;

namespace AvePoint.Item.Restore
{
    public  class AveItemSimulateResotreMain : AbstractAveItemRestore
    {
        private IRMSubJobDao SubJobDao = new RMSubJobDao();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private volatile bool jobEndFlag = false;
        Thread checkThread;
        public AveItemSimulateResotreMain(string jobId, JobType mJobType)
        {
            JobId = jobId;
            this.mJobType = mJobType;
        }
        public override async System.Threading.Tasks.Task RunNowAsync()
        {
            try
            {
                CheckNeedStop();
                SubJobDao.UpdateStatus(JobId, (int)RA.Contract.RMWeb.JobMonitor.JobStatus.InProgress, DateTime.UtcNow.Ticks);
                RMSubJob subJobWithContext = SubJobDao.GetSubJob(JobId, true);
                RestoreSettingAndTree mRestore = SerializerHelper.DeserializeByJsonConvert<RestoreSettingAndTree>(subJobWithContext.JobContext.Settings);
                MediaTCPRequest configForMedia = AssembleRestoreMessage(JobId, mRestore.Tree[0], mRestore);
                IArchiverRestoreService restoreService = MediaServiceFactory.CreateArchiverRestoreService();
                SimulateResotreResult statisticRes = restoreService.HandleSimulateRequest(configForMedia, cancellationTokenSource.Token);
                SubJobDao.UpdateStatus(JobId, (int)RA.Contract.RMWeb.JobMonitor.JobStatus.Finished, DateTime.UtcNow.Ticks, SerializerHelper.SerializeByJsonConvert(statisticRes));
            }
            catch(JobStopException e)
            {
                SubJobDao.UpdateStatus(JobId, (int)RA.Contract.RMWeb.JobMonitor.JobStatus.Stopped, DateTime.UtcNow.Ticks);
                mLog.Error($"Request stop job,e:{e}");
            }
            catch(Exception ex)
            {
                mLog.Error($"Fail run simulate resotre job status,e:{ex}");
                SubJobDao.UpdateStatus(JobId, (int)RA.Contract.RMWeb.JobMonitor.JobStatus.Failed, DateTime.UtcNow.Ticks);
            }
            finally
            {
                lock (this)
                {
                    jobEndFlag = true;
                }
                cancellationTokenSource.Dispose();
            }
        }

        public void CheckNeedStop()
        {
            checkThread = new Thread(DoCheckNeedStop) { IsBackground = true };
            checkThread.Start();
        }

        public void DoCheckNeedStop()
        {
            while(!jobEndFlag && !cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    RMSubJob subJob = SubJobDao.GetSubJob(JobId);
                    if (subJob.Status == (int)RA.Contract.RMWeb.JobMonitor.JobStatus.Stopping || subJob.Status == (int)RA.Contract.RMWeb.JobMonitor.JobStatus.Stopped)
                    {
                        cancellationTokenSource.Cancel();
                        break;
                    }
                }
                catch(Exception e)
                {
                    mLog.Error($"Fail Check stop,e:{e}");
                }
                Thread.Sleep(5000);
            }
        }


    }
}
