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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.SharePoint.Discover.Base;
using RAArchiverCommon.DestructionCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.Wrapper.Common;
using AvePoint.RA.SharePoint.SPObjDiscover.Models;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.Spreadsheet;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.Hybrid.ClientLibrary.SDK.Services;
using BaseJobDto = AvePoint.RA.Contract.JobMonitor.BaseJobDto;
using AvePoint.RA.Contract.Object.ArchiverMigration;
using AvePoint.RA.Contract.Tenant;
using AvePoint.GCommon.Utility;
using DocumentFormat.OpenXml.Office2010.Excel;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.SharePoint.Object;
using AvePoint.Item.Restore;
using PnP.Framework.Extensions;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMReport;
using System.IO;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Action;
using AvePoint.RA.RACommonUtility.Lcoker;
using AvePoint.RA.Contract.Monitor;
using AvePoint.RA.SharePoint.RestoreReport.Worker;
using AvePoint.RA.SharePoint.RestoreReport.Statistic;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using JobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus;

namespace AvePoint.RA.SharePoint.Discover
{
    public class RestoreReportProcessor : RMReportProcessor  //CreationAndDestroyedFileReportProcessor
    {

        private RMCreationJobMessage msg = null;
        private DateTime startUtcTime;
        private DateTime endUtcTime;
        private StatisticRestoreJobDetailsExecutor statisticRestoreJobDetailsExecutor;
        private JobContext jobContext;
        private readonly IJobMonitorDao JobMonitorDao = PlatformWindsorManager.GetService<IJobMonitorDao>();
        private readonly IRMReportService ReportService = PlatformWindsorManager.GetService<IRMReportService>();
        public RestoreReportProcessor(RMCreationJobMessage msg)
            : base(msg.JobID, (int)JobType.RestoreReport, false)
        {
            this.msg = msg;
            this.msg.EndTime = this.msg.EndTime.AddDays(1);//包含当天
            var globalTimeZone = GeneralSettingConfig.FindSystemTimeZoneById(this.msg.GlobalTimeZoneId.Replace("_", " "));
            startUtcTime = TimeZoneInfo.ConvertTimeToUtc(this.msg.StartTime, globalTimeZone);
            endUtcTime = TimeZoneInfo.ConvertTimeToUtc(this.msg.EndTime, globalTimeZone);
            jobContext = JobContext.GetInstance(msg.JobID, msg.JobType);
            jobContext.ReportManager.StartUpdateJobProgress();
            statisticRestoreJobDetailsExecutor = new StatisticRestoreJobDetailsExecutor(startUtcTime, endUtcTime, msg.ProfileId, jobContext);

            mLog.Info($"RestoreReportProcessor msg:{SerializerHelper.SerializeByJsonConvert(msg, true)}");
        }




        public override async Task RunReportJobAsync()
        {
            try
            {
                statisticRestoreJobDetailsExecutor.StatictisRestoreJobDetails();
                StartScheduledExport();
            }
            catch (Exception ex)
            {
                mLog.Error($"Fail RunReportJobAsync, ex:{ex}");
            }
        }
        private void StartScheduledExport()
        {
            RMProfileDto profile = ReportService.GetProfileByIdAsync(msg.ProfileId).GetAwaiter().GetResult();
            if (profile?.ScheduleId != null)
            {
                var jobIdReal = msg.JobID?.Split('_')[0];
                var job = JobMonitorDao.GetJobById(jobIdReal);
                if (job.Status == (int)AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Finished || job.Status == (int)AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.FinishWithException)
                {
                    var exportModel = new ExportReportCommonModel
                    {
                        ReportJobType = ((int)profile.Type).ToString(),
                        ReportJobId = jobIdReal,
                        ProfileName = profile.ProfileName,
                        ProfileId = profile.Id.ToString(),
                    };
                    var reportParameters = SerializerHelper.SerializeByJsonConvert(exportModel);
                    ReportService.RunExportReportJob(reportParameters);
                }
            }
        }





        protected override CAMLManager InitCamlQuery(IAveFieldCollection listFields, IAveTaxonomyField taxonomyField, List<Guid> termIds)
        {
            throw new NotImplementedException();
        }

        protected override CAMLManager InitUnclassificationCamlQuery(IAveFieldCollection listFields, IAveWeb web, IAveList list, RMReportExtension reportExt)
        {
            throw new NotImplementedException();
        }

        protected override int ProcessItems(IAveWeb web, IAveList list, List<RMDiscoverItem> items)
        {
            throw new NotImplementedException();
        }
    }
}
