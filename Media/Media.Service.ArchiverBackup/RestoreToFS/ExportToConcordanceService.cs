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
///********************************************************************
// *
// *  PROPRIETARY and CONFIDENTIAL
// *
// *  This file is licensed from, and is a trade secret of:
// *
// *                   AvePoint, Inc.
// *                   Harborside Financial Center
// *                   9th Fl.   Plaza Ten
// *                   Jersey City, NJ 07311
// *                   United States of America
// *                   Telephone: +1-800-661-6588
// *                   WWW: www.avepoint.com
// *
// *  Refer to your License Agreement for restrictions on use,
// *  duplication, or disclosure.
// *
// *  RESTRICTED RIGHTS LEGEND
// *
// *  Use, duplication, or disclosure by the Government is
// *  subject to restrictions as set forth in subdivision
// *  (c)(1)(ii) of the Rights in Technical Data and Computer
// *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
// *  FAR 52.227-19 (C) (June 1987).
// *
// *  Copyright © 2013-2015 AvePoint® Inc. All Rights Reserved. 
// *
// *  Unpublished - All rights reserved under the copyright laws of the United States.
// *  $Revision:  $
// *  $Author:  $        
// *  $Date:  $
// */



//namespace AvePoint.Media.Service.ArchiverBackup
//{
//    #region using directives

//    using System;
//    using System.Collections.Generic;
//    using System.IO;
//    using System.Linq;
//    using System.Reflection;
//    using System.Text;
//    using AvePoint.GCommon;
//    using AvePoint.GCommon.Contract.CodeReview;
//    using AvePoint.GCommon.Contract.Media.Object;
//    using AvePoint.GCommon.Contract.Server.Job;
//    using AvePoint.GCommon.Contract.Server.Job.Object;
//    using AvePoint.GCommon.Contract.Storage.Entity;
//    using AvePoint.GCommon.Media.StorageService;
//    using AvePoint.Media.Common;
//    using Merged18NResources.MediaServiceArchiverBackup;
//    using AvePoint.Media.Service.DomainModel;
//    using AvePoint.Media.Storage;
//    using AvePoint.Media.Storage.Util;

//    #endregion using directives

//    #region CodeReview

//    [AveCodeReview(
//    "2012/6/20",
//    "dwxue@avepoint.com",
//    "yjhuo@avepoint.com",
//    new string[] { },
//    null,
//    true)]

//    #endregion CodeReview

//    public class ExportToConcordanceService
//        : RestoreToFSServiceBase<ArchiverExportJob>
//        , IRestoreToFSService
//    {
//        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
//        const Int64 KB = 1024;
//        ArchiverExportJob exportJob;
//        IXSystem indexDevice;
//        IExportService exportService;
//        JobStatusInfo jobStatusInfo;
//        Int32 totalItemNumber;
//        Int32 exportItemNumber;
//        RestoreJobPolicy restoreJobPolicy;
//        Int64 totalSize;
//        String errorMessage;

//        public IRestoreToFSReportService ReportService { get; set; }

//        public IDataReader<ArchiverRestoreJob> DataReader { get; set; }

//        public IIndexService<ArchiverIndexServiceOpenParameter> IndexService { get; set; }

//        public IArchiverRestoreIndexService RestoreIndexService { get; set; }

//        public IEncryptionInfoManager EncryptionInfoManager { get; set; }

//        public IRestoreJobRunningPolicyChecker RestoreJobRunningPolicyChecker { get; set; }

//        public IJobProgressUpdater JobProgressUpdater { get; set; }

//        public override void Open(ArchiverExportJob restoreJob)
//        {
//            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreToFSServiceOpenBegin);
//            this.exportJob = restoreJob;
//            this.ReportService.PrepareForReport(this.exportJob);
//            this.indexDevice = this.StorageDeviceManager.Open(this.exportJob.IndexLogicalDevice.GetXRIS(PhysicalDeviceUsage.Index));
//            var indexOpenParam = new ArchiverIndexServiceOpenParameter(this.exportJob, this.indexDevice);
//            this.IndexService.Open(indexOpenParam);
//            var tempRestoreJob = new ArchiverRestoreJob(this.exportJob);
//            this.DataReader.Open(tempRestoreJob);
//            var encryptionInfoDic = this.EncryptionInfoManager.PutEncryptionInfos(this.exportJob.RestoreSecurityInfos);
//            DataReader.SetEncryptionInfos(encryptionInfoDic);
//            var exportProvider = new ExportServiceProvider();
//            this.exportService = exportProvider.Create(ExportFormat.Concordance);
//            var exportServiceInfo = new ExportServiceInfo { ExportDevice = this.exportJob.DestinationFSDevice, JobId = this.exportJob.ParentJobId };
//            this.exportService.Open(exportServiceInfo);
//            this.jobStatusInfo = new JobStatusInfo() { Id = this.exportJob.JobId, MainJobId = this.exportJob.ParentJobId, Type = 28, State = 1 };
//            this.JobProgressUpdater.UpdateJobProgress(this.jobStatusInfo, 100, 1);
//            this.totalItemNumber = this.exportJob.ExportItemList.Count;
//            this.restoreJobPolicy = new RestoreJobPolicy(exportJob);
//            this.RestoreJobRunningPolicyChecker.SetPolicy(restoreJobPolicy);
//            this.restoreJobPolicy.JobStatus = JobStatus.Stopping;
//        }

//        public override void DoRestore()
//        {
//            var folderName = this.GenerateFolderName();
//            var fileName = String.Empty;
//            foreach (ExportItemInfo info in this.exportJob.ExportItemList)
//            {
//                if (this.RestoreJobRunningPolicyChecker.CheckPolicy(this.restoreJobPolicy))
//                {
//                    throw new JobNeedStopException();
//                }
//                this.logger.Debug(MediaServiceArchiverBackupResource.ExportToConcordanceServiceDoRestorePathMD5, info.PathMD5);
//                var index = this.RestoreIndexService.GetCurrentIndex(info.PathMD5, info.SubJobId);
//                index.IsRestoreToFS = true;
//                try
//                {
//                    this.DataReader.GetNextItem(index);
//                    fileName = this.GenerateFileName(index);
//                    var metaData = this.GenerateMetaData(info, index);
//                    var exportInfo = new ExportInfo { FolderName = folderName, FileName = fileName };
//                    if (index.Type.EqualsIgnoreCase("I"))
//                        this.exportService.Export(metaData, exportInfo);
//                    else
//                    {
//                        metaData.FileName = fileName;
//                        this.exportService.Export(DataReader.Input.ReadContent, metaData, exportInfo);
//                    }
//                    this.logger.Info(MediaServiceArchiverBackupResource.ExportToConcordanceServiceDoRestoreFinished, fileName);
//                }
//                catch (Exception e)
//                {
//                    this.logger.Error(MediaServiceArchiverBackupResource.ExportToConcordanceServiceDoRestoreError, fileName, e.ToString());
//                }
//                this.totalSize += index.ContentLength;
//                var detail = new ItemDetailMessage { Name = fileName, Type = index.Type, ContentLength = index.ContentLength };
//                this.ReportService.SendDetailReport(detail);
//                this.JobProgressUpdater.UpdateJobProgress(this.jobStatusInfo, this.totalItemNumber, ++this.exportItemNumber);
//            }
//        }

//        String GenerateFolderName()
//        {
//            var temp = this.exportJob.SiteUrl.Replace("://", "#");
//            var siteUrl = temp.Replace("/", "#").Replace(":", "#");
//            var folderName = Path.Combine(this.exportJob.ParentJobId, siteUrl);
//            return folderName;
//        }

//        ConcordanceMetaData GenerateMetaData(ExportItemInfo info, ArchiverBasicIndex index)
//        {
//            var createdTime = new DateTime(info.CreateTime);
//            var metaDataInfo = new HashSet<MetaDataItemInfo>();
//            var metaData = new ConcordanceMetaData { CreatedBy = info.CreateBy, CreatedTime = createdTime.ToString(), ContentSize = index.ContentLength };
//            var timeZoneInfo = this.GetTimeZoneInfo(index.Attributes);
//            var userDefined = index.Attributes.Split(ServiceConstants.ExtraChar);
//            foreach (String column in userDefined)
//            {
//                var seperatorIndex = column.IndexOf(ServiceConstants.Delimiter);
//                if (seperatorIndex > 0 && seperatorIndex + 1 != column.Length)
//                {
//                    var columnName = column.Remove(seperatorIndex);
//                    var columnValue = column.Substring(seperatorIndex + 1);
//                    var localTime = new DateTime();
//                    var isDateTime = DateTime.TryParse(columnValue, out localTime);
//                    if (columnName.EqualsIgnoreCase("Title"))
//                        metaData.Title = columnValue;
//                    else if (isDateTime)
//                    {
//                        localTime = TimeZoneInfo.ConvertTimeFromUtc(localTime, timeZoneInfo);
//                        metaDataInfo.Add(new MetaDataItemInfo(columnName, localTime.ToString(), typeof(DateTime)));
//                    }
//                    else
//                        metaDataInfo.Add(new MetaDataItemInfo(columnName, columnValue, typeof(String)));
//                }
//            }
//            metaDataInfo.Add(new MetaDataItemInfo("Location", info.FullPath, typeof(String)));
//            metaData.MetadataInfo = metaDataInfo;
//            return metaData;
//        }

//        TimeZoneInfo GetTimeZoneInfo(String attributes)
//        {
//            var timeZoneInfo = TimeZoneInfo.Local;
//            if (attributes != null && attributes.Contains("TimeZoneID"))
//            {
//                var timeZoneId = attributes.Substring(attributes.IndexOfIgnoreCase("TimeZoneID") + 11);
//                timeZoneId = timeZoneId.Remove(timeZoneId.IndexOf(ServiceConstants.ExtraChar));
//                timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
//            }
//            return timeZoneInfo;
//        }

//        String GenerateFileName(ArchiverBasicIndex index)
//        {
//            var fileName = index.Name;
//            var flag = index.Name.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);
//            if (index.Type.EqualsIgnoreCase("A"))
//                fileName = index.Name.Substring(flag + 1);
//            else if (index.Type.EqualsIgnoreCase("D") || index.Type.EqualsIgnoreCase("V"))
//            {
//                var name = index.ItemName.Remove(index.ItemName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase));
//                var extension = index.ItemName.Substring(index.ItemName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase));
//                fileName = flag > 0 ? name + '_' + index.Name.Substring(flag + 1) + extension : index.Name;
//            }
//            return fileName;
//        }

//        public override void ProcessException(Exception e)
//        {
//            e = e.InnerException ?? e;
//            if (this.jobStatusInfo != null)
//            {
//                if (e is JobNeedStopException)
//                {
//                    this.restoreJobPolicy.JobStatus = JobStatus.Stopped;
//                    this.jobStatusInfo.State = 4;
//                    this.RestoreJobRunningPolicyChecker.SetPolicy(this.restoreJobPolicy);
//                }
//                else
//                    this.jobStatusInfo.State = 7;
//            }
//            this.errorMessage = e.ToString();
//            this.logger.Error(MediaServiceArchiverBackupResource.ExportToConcordanceServiceDoRestoreProcessExceptionError, e.ToString());
//        }

//        public override void Dispose()
//        {
//            if (this.IndexService != null)
//                this.IndexService.Close();
//            this.StorageDeviceManager.Close(this.indexDevice);
//            this.DataReader.Close();
//            this.exportService.Close();
//            this.jobStatusInfo.Progress = 100;
//            this.jobStatusInfo.State = this.jobStatusInfo.State != 1 ? this.jobStatusInfo.State : 2;
//            var summary = new JobSummaryMessage { JobStatus = this.jobStatusInfo.State, TotalSize = this.totalSize / 1024, ErrorMessage = this.errorMessage };
//            this.ReportService.SendJobSummary(summary);
//            this.JobProgressUpdater.UpdateJobProgress(this.jobStatusInfo, 100, 100, true);
//        }
//    }
//}