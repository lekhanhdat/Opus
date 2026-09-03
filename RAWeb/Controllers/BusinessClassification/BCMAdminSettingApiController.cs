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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.Service.Services.ControlPanel;
using AvePoint.RA.Service.Services.RMFileSystemSettings;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.BusinessClassification
{
    [RMApiAuthorize(RMPermissionMasks.ContentRepositoyAdmin, RMSOPermissionMasks.AccessAll, preferred: false)]
    public class BCMAdminSettingApiController : BaseApiController
    {
        #region interface
        public IRMSharePointSettingsService _RMSPSettingsService;
        public IRMSharePointSettingsService RMSPSettingsService => PlatformWindsorManager.GetService(ref _RMSPSettingsService);
        public IRMFileSystemSettingsService _RMFSSettingsService;
        public IRMFileSystemSettingsService RMFSSettingsService => PlatformWindsorManager.GetService(ref _RMFSSettingsService);
        public IUniqueIdSettingService _UniqueIdSettingService;
        public IUniqueIdSettingService UniqueIdSettingService => PlatformWindsorManager.GetService(ref _UniqueIdSettingService);
        public IScheduleService _ScheduleService;
        public IScheduleService ScheduleService => PlatformWindsorManager.GetService(ref _ScheduleService);
        public IRMCollectionDataService _ReportCollectionService;
        public IRMCollectionDataService ReportCollectionService => PlatformWindsorManager.GetService(ref _ReportCollectionService);
        public ILicenseHelperService _LicenseHelperService;
        public ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService(ref _LicenseHelperService);

        public IRMTeamsSettingsService _RMTeamsSettingsService;
        public IRMTeamsSettingsService RMTeamsSettingsService => PlatformWindsorManager.GetService(ref _RMTeamsSettingsService);
        private IRMKeyValueDao _RMKeyValueDao;
        public IRMKeyValueDao RMKeyValueDao => (IRMKeyValueDao)PlatformWindsorManager.GetService(ref _RMKeyValueDao);
        private IFSConnectionDao _FSConnectionDao;
        private IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService(ref _FSConnectionDao);
        private IExplorerService _ExplorerService;
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService(ref _ExplorerService);

        #endregion

        #region Run Job

        [HttpPost]
        public string RunUIJob()
        {
            bool re = false;
            try
            {
                //启动Job前,check是否满足条件
                if (UniqueIdSettingService.ValidUniqueIdSetting())
                {
                    UniqueIdSettingService.RunUniqueIDSettingScheduleJob(JobRunBy.Schedule, Contract.JobMonitor.JobType.UniqueIDSettingIncrementalSchedule);
                    re = true;
                    Logger.Info($"ran UniqueIDSettingIncremental Job");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"failed to run UniqueIDSettingIncremental Job,message:{ex.Message},stackTrace:{ex.StackTrace}");
            }
            return re.ToString();
        }

        [HttpPost]
        public string RunUFJob()
        {
            UniqueIdSettingService.RunUniqueIDSettingScheduleJob(JobRunBy.Schedule, Contract.JobMonitor.JobType.UniqueIDSettingFullSchedule);
            return true.ToString();
        }

        [HttpPost]
        public string RunLocalUIJob()
        {
            bool re = false;
            try
            {
                //启动Job前,check是否满足条件
                if (UniqueIdSettingService.ValidSPOnPremUniqueIdSetting())
                {
                    UniqueIdSettingService.RunUniqueIDSettingScheduleJob(JobRunBy.Schedule, Contract.JobMonitor.JobType.SPOnPremUniqueIDSettingIncrementalSchedule);
                    re = true;
                    Logger.Info($"ran sp-onprem UniqueIDSettingIncremental Job");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"failed to run sp-onprem UniqueIDSettingIncremental Job,message:{ex.Message},stackTrace:{ex.StackTrace}");
            }
            return re.ToString();
        }

        [HttpPost]
        public string RunLocalUFJob()
        {
            UniqueIdSettingService.RunUniqueIDSettingScheduleJob(JobRunBy.Schedule, Contract.JobMonitor.JobType.SPOnPremUniqueIDSettingFullSchedule);
            return true.ToString();
        }

        [HttpPost]
        public string RunCDF()
        {
            ReportCollectionService.RunScheduleJob(JobRunBy.Schedule, Contract.JobMonitor.JobType.CollectionDataFull);
            return true.ToString();
        }

        [HttpPost]
        public string RunCDI()
        {
            ReportCollectionService.RunScheduleJob(JobRunBy.Schedule, Contract.JobMonitor.JobType.CollectionDataIncremental);
            return true.ToString();
        }

        //[HttpGet]
        //public string DirtyData()
        //{
        //    mRMSPSettingsService.CheckDirtyData();
        //    return true.ToString();
        //}

        [HttpPost]
        public string RunTeamsUIJob()
        {
            bool re = false;
            try
            {
                //启动Job前,check是否满足条件
                if (UniqueIdSettingService.ValidTeamsUniqueIdSetting())
                {
                    UniqueIdSettingService.RunTeamsUniqueIDSettingScheduleJob(JobRunBy.Schedule, Contract.JobMonitor.JobType.TeamsUniqueIDSettingIncrementalSchedule);
                    re = true;
                    Logger.Info($"ran UniqueIDSettingIncremental Job");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"failed to run UniqueIDSettingIncremental Job,message:{ex.Message},stackTrace:{ex.StackTrace}");
            }
            return re.ToString();
        }

        [HttpPost]
        public string RunTeamsUFJob()
        {
            UniqueIdSettingService.RunTeamsUniqueIDSettingScheduleJob(JobRunBy.Schedule, Contract.JobMonitor.JobType.TeamsUniqueIDSettingFullSchedule);
            return true.ToString();
        }


        #endregion

        #region Import Settings
        [HttpPost]
        ////[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        ////[FileDownloadFilter]
        [RMApiAuthorize(RMPermissionMasks.SPOEnduser)]
        public IActionResult DownloadTemplate()
        {
            try
            {
                string filepath = "";
                if (LicenseHelperService.IsNewOpus().GetAwaiter().GetResult())
                {
                    filepath = Path.Combine(WebUtil.GetInstallPath(), "Config", "Import Content Sources Settings for SharePoint Online.csv");
                }
                else
                {
                    filepath = Path.Combine(WebUtil.GetInstallPath(), "Config", "OldTemplate", "Import Content Sources Settings for SharePoint Online.csv");
                }
                var memoryStream = new MemoryStream();
                using (var stream = new FileStream(filepath, FileMode.Open,FileAccess.Read))
                {
                    stream.CopyTo(memoryStream);
                }
                memoryStream.Position = 0;
                return File(memoryStream, GetContentType(filepath), Path.GetFileName(filepath));
            }
            catch
            {
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }
        }
        [HttpPost]
        public IActionResult DownloadArchiverImportTemplate()
        {
            try
            {   if (IsUseArchiverImportFile())
                {
                    var filepath = Path.Combine(WebUtil.GetInstallPath(), "Config", "Archiver Import Site Template.csv");

                    var memoryStream = new MemoryStream();
                    using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                    {
                        stream.CopyTo(memoryStream);
                    }
                    memoryStream.Position = 0;
                    return File(memoryStream, GetContentType(filepath), Path.GetFileName(filepath));
                }
                else
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }
            }
            catch
            {
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }
        }
        private bool IsUseArchiverImportFile()
        {
            var key = RMKeyValueDao.GetValueByKey("UseArchiverImportFile");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }
        [HttpPost]
        public IActionResult DownloadTeamsArchiverImportTemplate()
        {
            try
            {
                var filepath = Path.Combine(WebUtil.GetInstallPath(), "Config", "Archiver Import Teams Template.csv");

                var memoryStream = new MemoryStream();
                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    stream.CopyTo(memoryStream);
                }
                memoryStream.Position = 0;
                return File(memoryStream, GetContentType(filepath), Path.GetFileName(filepath));
            }
            catch
            {
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }
        }

        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }

            return contentType;
        }

        [HttpPost]
        public IActionResult DownloadFSTemplate()
        {
            try
            {
                var fileName = "Import Content Sources Settings for File System.xlsx";
                //if (RMKeyValueDao.IsEnableJPMCFileSystemFeature())
                //{
                //    fileName = "Import Content Sources Settings for File System JPMC.xlsx";
                //}
                var filepath = Path.Combine(WebUtil.GetInstallPath(), "Config", fileName);
                var memoryStream = new MemoryStream();
                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    stream.CopyTo(memoryStream);
                }
                memoryStream.Position = 0;
                return File(memoryStream, GetContentType(filepath), Path.GetFileName(filepath));
            }
            catch
            {
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.SPOEnduser)]
        public string ImportSPSetting()
        {
            string jobId = "";
            try
            {
                var file = Request.Form.Files["fileUp"];
                Logger.Info("sharepoint setting import file,file name :{0}", file.FileName);
                CheckFile(file, FileExtension.CSV);
                string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
                DateTime dt = DateTime.Now;
                string fileName = "ImportSharePointSettings_" + dt.Ticks.ToString() + ".csv";
                var blobName = Path.Combine(JobReportUtility.GetTenantIdentity(), JobReportUtility.ImportCSVFile, fileName);
                RAStorageUtil.UploadReportBlob(blobName, file.OpenReadStream());
                Logger.Info("save file success.");
                if (RMSPSettingsService == null)
                {
                    Logger.Error("mRMSPSettingsService null.");
                }
                jobId = RMSPSettingsService.RunImportSPSetting(JobRunBy.Control, extension, blobName);
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred import sharepoint setting data:{0}", ex.ToString());
            }
            return jobId;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.SPOEnduser)]
        public RAReturnMessage ExportSPSetting([FromBody][BindRequired] ExportSettingType type)
        {
            RAReturnMessage message = new();
            try
            {
                Logger.Info("Share point setting export file");
                DateTime dt = DateTime.Now;
                if (RMSPSettingsService == null)
                {
                    Logger.Error("mRMSPSettingsService null.");
                }
                message = RMSPSettingsService.RunExportSPSetting(type, JobRunBy.Control);
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred export share point setting data:{0}", ex.ToString());
            }
            return message;
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.SPOEnduser)]
        public RAReturnMessage ExportSPSOSetting([FromBody][BindRequired] ExportSettingType type)
        {
            RAReturnMessage message = new();
            try
            {
                Logger.Info("Share point setting export file");
                DateTime dt = DateTime.Now;
                if (RMSPSettingsService == null)
                {
                    Logger.Error("mRMSPSettingsService null.");
                }
                message = RMSPSettingsService.RunExportSPSOSetting(type, JobRunBy.Control);
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred export share point setting data:{0}", ex.ToString());
            }
            return message;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ContentRepositoyAdmin)]
        public RAReturnMessage ImportFSSetting()
        {
            RAReturnMessage message = null;
            try
            {
                var file = Request.Form.Files["fileUp"];
                Logger.Info("sharepoint setting import file,file name :{0}", file.FileName);
                CheckFile(file, FileExtension.XLSX);
                string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
                DateTime dt = DateTime.Now;
                string fileName = "ImportFileSystemSettings_" + dt.Ticks.ToString() + ".xlsx";
                var blobName = Path.Combine(JobReportUtility.GetTenantIdentity(), JobReportUtility.ImportCSVFile, fileName);
                RAStorageUtil.UploadReportBlob(blobName, file.OpenReadStream());
                Logger.Info("save file success.");
                if (RMFSSettingsService == null)
                {
                    Logger.Error("mRMFSSettingsService null.");
                }
                message = RMFSSettingsService.RunImportFSSettingJob(JobRunBy.Control, extension, blobName);
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred import file System setting data:{0}", ex.ToString());
            }
            return message;
        }

        #endregion

        #region Export settings

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ContentRepositoyAdmin)]
        public RAReturnMessage ExportFSSetting()
        {
            RAReturnMessage message = null;
            try
            {
                Logger.Info("file systemt setting export file");
                DateTime dt = DateTime.Now;
                if (RMFSSettingsService == null)
                {
                    Logger.Error("mRMFSSettingsService null.");
                }
                message = RMFSSettingsService.RunExportFSSettingJob(JobRunBy.Control);
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred export file System setting data:{0}", ex.ToString());
            }
            return message;
        }
        #endregion
        #region Download RCC report

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.FSAdmin)]
        public RAReturnMessage DownloadRCCReport([FromBody] RCCReportRequest request)
        {
            var result = new RAReturnMessage() { MessageType = RAMessageType.Failed };
            try
            {
                if (!RMKeyValueDao.IsEnableJPMCFileSystemFeature())
                {
                    result.ErrorMessage = "This feature is not supported in Non-JPMC environment.";
                    return result;
                }

                if (request?.Nodes == null || request.Nodes.Count == 0)
                {
                    result.ErrorMessage = "At least one node is required.";
                    return result;
                }

                if (request?.TimeRange == null)
                {
                    result.ErrorMessage = "Time range is required.";
                    return result;
                }

                if (request.TimeRange.PresetType == 0 && (string.IsNullOrEmpty(request.TimeRange.StartDate) || string.IsNullOrEmpty(request.TimeRange.EndDate)))
                {
                    result.ErrorMessage = "Start date and end date are required for custom time range.";
                    return result;
                }

                var node = request.Nodes.FirstOrDefault();
                switch (request.Level)
                {
                    case (int)NodeLevel.SiteCollection:
                        request.ConnectionId = node.Id;
                        break;
                    case (int)NodeLevel.FSFolder:
                        request.ConnectionId = Guid.Parse(ExplorerService.GetFSConnectionIdByItemId(node.Id));
                        break;
                    default:
                        request.ConnectionId = node.Id;
                        break;
                }
                result = RMFSSettingsService.RunDownloadRCCReportJob(request, JobRunBy.Control);
            }
            catch (Exception ex)
            {
                result.ErrorMessage = "Download RCC Report failed.";
                Logger.Error($"Download RCC Report failed. ERROR:{ex}");
            }
            return result;
        }
        #endregion

        #region Unique Id Settings
        [HttpPost]
        public UniqueIdSetting LoadingUniqueIdSetting([FromBody] UniqueIdLoad uniqueIdLoad)
        {
            var result = new UniqueIdSetting();
            if (uniqueIdLoad.SourceFlag == SourceFlag.Teams)
            {
                try
                {
                    result = UniqueIdSettingService.LoadingTeamsUniqueIdSetting();
                }
                catch (Exception e)
                {
                    Logger.Error("failed to loading teams uniqueIdSetting", e.ToString());
                }
                return result;
            }
            
            if(uniqueIdLoad.SourceFlag == SourceFlag.FileSystem)
            {
                try
                {
                    result = UniqueIdSettingService.LoadingFSUniqueIdSetting();
                }
                catch (Exception e)
                {
                    Logger.Error("failed to loading teams uniqueIdSetting", e.ToString());
                }
                return result;
            }

            try
            {
                result = UniqueIdSettingService.LoadingUniqueIdSetting();
            }
            catch (Exception e)
            {
                Logger.Error("failed to loading uniqueIdSetting", e.ToString());
            }

            return result;
        }

        [HttpPost]
        public async Task<RAReturnMessage> UpdateUniqueIdSetting([FromBody] UniqueIdSetting setting)
        {
            return await RouteMultiGeoApiActionAsync(
                setting,
                MultiGeoOperationType.UpdateUniqueIdSetting,
                async request =>
                {
                    var result = new RAReturnMessage();
                    result.MessageType = RAMessageType.Successful;
                    try
                    {
                        if (request.Prefix.Length < 4 || request.Prefix.Length > 12)
                        {
                            result.MessageType = RAMessageType.Failed;
                            return result;
                        }

                        if (request.SourceFlag == SourceFlag.FileSystem)
                        {
                            await UniqueIdSettingService.UpdateFileSystemUniqueIdSettingAsync(request);
                        }
                        else
                        {
                            await UniqueIdSettingService.UpdateUniqueIdSettingAsync(request);

                            if (request.IsActived)
                            {
                                await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.UniqueIDSettingSchedule);
                                await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.SPOnPremUniqueIDSettingSchedule);
                                await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.TeamsUniqueIDSettingSchedule);
                            }
                            else
                            {
                                ScheduleService.DeleteScheduleByType(ScheduleType.UniqueIDSettingSchedule);
                                ScheduleService.DeleteScheduleByType(ScheduleType.SPOnPremUniqueIDSettingSchedule);
                                ScheduleService.DeleteScheduleByType(ScheduleType.TeamsUniqueIDSettingSchedule);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        result.MessageType = RAMessageType.Failed;
                        if (e.Message.Contains("RM_JS_FS_UniqueSetting_SaveFailed"))
                        {
                            result.ErrorMessage = I18NEntity.GetString(e.Message);
                        }

                        Logger.Error("failed to loading uniqueIdSetting", e.ToString());
                    }
                    return result;
                },
                _ => new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage")
                });
        }

        #endregion

        #region Schedule
        [HttpPost]
        [ValidScheduleSettingActionFilter]
        [RMApiAuthorize(RMPermissionMasks.ContentRepositoyAdmin, RMSOPermissionMasks.ContentRepositoyAdmin)]
        public async Task<string> GetScheduleByType([FromBody][BindRequired] ScheduleType type)
        {
            List<ScheduleInfo> schedules = await ScheduleService.GetScheduleByTypeServiceAsync(type);
            return JsonConvert.SerializeObject(schedules);
        }
        #endregion

        #region Private Method
        private void CheckFile(IFormFile file, FileExtension fileExtension)
        {
            string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
            var allowFileExts = fileExtension == FileExtension.CSV ? new List<FileExtension> { FileExtension.CSV } : new List<FileExtension> { FileExtension.XLSX };
            WebUtil.CheckFileExtension(extension, allowFileExts);
            WebUtil.CheckFileHeadCode(file.OpenReadStream(), allowFileExts);
        }

        #endregion
    }
}