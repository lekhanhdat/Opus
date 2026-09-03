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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.CSD;
using AvePoint.RA.Contract.CSD.Service;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.Authentication;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Services.StorageDevice;
using AvePoint.RA.Service.Services.Tenant;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using AvePoint.RA.Web.Models.ControlPanel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AvePoint.RA.Web.Common.Filters.GoogleDriveFilter;
using CP = AvePoint.RA.Contract.RMWeb.CP;

namespace AvePoint.RA.Web.Controllers.ControlPanel
{
    [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, preferred: false)]
    public class CPApiController : BaseApiController
    {


        private IAuthenticationManagerService _AuthenticationManagerService;
        private IAuthenticationManagerService AuthenticationManagerService => PlatformWindsorManager.GetService(ref _AuthenticationManagerService);
        private IAccountManagerService _AccountManagerService;
        private IAccountManagerService AccountManagerService => PlatformWindsorManager.GetService(ref _AccountManagerService);
        private IGeneralSettingService _GeneralSettingService;
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService(ref _GeneralSettingService);
        private IExportSettingService _ExportSettingService;
        private IExportSettingService ExportSettingService => PlatformWindsorManager.GetService(ref _ExportSettingService);
        private IExportDataEncryptionSettingService _ExportDataEncryptionSettingService;
        private IExportDataEncryptionSettingService ExportDataEncryptionSettingService => PlatformWindsorManager.GetService(ref _ExportDataEncryptionSettingService);
        private IEmailTemplateService _EmailTemplateService;
        private IEmailTemplateService EmailTemplateService => PlatformWindsorManager.GetService(ref _EmailTemplateService);
        private IManualProcessManagementService _ManualProcessManagementService;
        private IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService(ref _ManualProcessManagementService);
        private IUserService _UserService;
        private IUserService UserService => PlatformWindsorManager.GetService(ref _UserService);
        private IPermissionManagementService _PermissionManagementService;
        private IPermissionManagementService PermissionManagementService => PlatformWindsorManager.GetService(ref _PermissionManagementService);
        private ISecurityGroupManagementService _SecurityGroupManagementService;
        private ISecurityGroupManagementService SecurityGroupManagementService => PlatformWindsorManager.GetService(ref _SecurityGroupManagementService);
        private ISPSettingTreeService _mSPSettingTreeService;
        private ISPSettingTreeService mSPSettingTreeService => PlatformWindsorManager.GetService(ref _mSPSettingTreeService);
        private IRMSecurityContainerService _SecurityContainerService;
        private IRMSecurityContainerService SecurityContainerService => PlatformWindsorManager.GetService(ref _SecurityContainerService);
        private ITaxonomyService _TaxonomyService;
        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService(ref _TaxonomyService);
        private IRuleContainerService _RuleContainerService;
        private IRuleContainerService RuleContainerService => PlatformWindsorManager.GetService(ref _RuleContainerService);
        private ICSDCommonService _CSDCommonService;
        private ICSDCommonService CSDCommonService => PlatformWindsorManager.GetService(ref _CSDCommonService);
        private IRMArchiverSettingsService _ArchiverSettingsService;
        private IRMArchiverSettingsService ArchiverSettingsService => PlatformWindsorManager.GetService(ref _ArchiverSettingsService);
        private ISettingProfileService _SettingProfileService;
        private ISettingProfileService SettingProfileService => PlatformWindsorManager.GetService(ref _SettingProfileService);


        #region Global Setting

        //no longer load storage and export location from dao
        //[HttpGet]
        //public string GetStorageSettings()
        //{
        //    GlobalStorageSetting gssDataTemp = new GlobalStorageSetting();
        //    //need to init these properties to ensure page looks normally  
        //    gssDataTemp.CurrentExportLocation = new ExportLocation();
        //    //gssDataTemp.CurrentProcessingPool = new ProcessingPool();
        //    gssDataTemp.CurrentSecurityProfile = new SecurityProfile();
        //    gssDataTemp.CurrentStoragePolicy = new StoragePolicy();
        //    gssDataTemp.AllStoragePolicy = new List<StoragePolicy>();
        //    gssDataTemp.AllExportLocation = new List<ExportLocation>()
        //    {
        //        new ExportLocation() { ID = Guid.Empty.ToString(), Name = I18NEntity.GetString("RM_JS_Rule_ObjectLevel_None") }
        //    };
        //    gssDataTemp.AllSecurityProfile = new List<SecurityProfile>();
        //    //gssDataTemp.AllProcessingPool = new List<ProcessingPool>();
        //    try
        //    {

        //        //这里我们需要先从DocAve load数据，如果发现DocAve里的一些信息被remove了，reset信息的显示并在前台给出相应提示

        //        #region Get MetaData From DocAve
        //        SORulesAndSettings docAveMetaData = null;
        //        try
        //        {
        //            docAveMetaData = GlobalSettingService.LoadMetaData();
        //        }
        //        catch (Exception ex)
        //        {
        //            gssDataTemp.GSSExceptionType = GSSExceptionType.DocAveConnFailed;
        //            gssDataTemp.ExceptionMsg = string.Format(I18NEntity.GetString("RM_JS_Common_FromDocaveMsg"), ex.Message);
        //            return JsonConvert.SerializeObject(gssDataTemp);
        //        }
        //        foreach (var item in docAveMetaData.StoragePolicies)
        //        {
        //            gssDataTemp.AllStoragePolicy.Add(new StoragePolicy()
        //            {
        //                ID = item.Id,
        //                Name = item.Name
        //            });
        //        }
        //        try
        //        {
        //            foreach (var item in GlobalSettingService.GetAllExportLocation())
        //            {
        //                //if (item.ReportType == GCommon.Contract.Server.Common.ExportReport.Object.ExportReportType.SharePoint)
        //                //{
        //                //    continue;
        //                //}
        //                gssDataTemp.AllExportLocation.Add(new ExportLocation()
        //                {
        //                    ID = item.Id,
        //                    Name = item.Name
        //                });
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.Warn("Get Global Storage Setting Export Location  Failed.{0}", ex.Message);
        //        }

        //        foreach (var item in docAveMetaData.DataEncryptionProfiles)
        //        {
        //            gssDataTemp.AllSecurityProfile.Add(new SecurityProfile()
        //            {
        //                ID = item.Guid,
        //                Name = item.Name,
        //                IsDefault = item.IsDefault
        //            });
        //        }

        //        //foreach (var item in docAveMetaData.ProcessingPools)
        //        //{
        //        //    gssDataTemp.AllProcessingPool.Add(new ProcessingPool()
        //        //    {
        //        //        ID = item.Id,
        //        //        Name = item.Name
        //        //    });
        //        //}
        //        #endregion

        //        #region Get MetaData From RA

        //        GlobalStorageSetting gssData = GlobalSettingService.LoadGlobalSettingInfoFromRA();

        //        if (gssData.CurrentExportLocation != null)
        //        {
        //            if (gssDataTemp.AllExportLocation.Where(item => item.ID == gssData.CurrentExportLocation.ID).Count() != 0)
        //            {
        //                gssDataTemp.CurrentExportLocation = gssData.CurrentExportLocation;
        //            }
        //            else
        //            {
        //                gssDataTemp.CurExportLocationRemoved = true;
        //            }
        //        }
        //        //if (gssData.CurrentProcessingPool != null)
        //        //{
        //        //    if (gssDataTemp.AllProcessingPool.Where(item => item.ID == gssData.CurrentProcessingPool.ID).Count() != 0)
        //        //    {
        //        //        gssDataTemp.CurrentProcessingPool = gssData.CurrentProcessingPool;
        //        //    }
        //        //    else
        //        //    {
        //        //        gssDataTemp.CurProcessingPoolRemoved = true;
        //        //    }
        //        //}

        //        if (gssData.CurrentStoragePolicy != null)
        //        {
        //            if (gssData.CurrentStoragePolicy.ID == null || gssData.CurrentStoragePolicy.ID == Guid.Empty.ToString() || string.IsNullOrEmpty(gssData.CurrentStoragePolicy.ID))
        //            {
        //                gssDataTemp.CurrentStoragePolicy = gssData.CurrentStoragePolicy;
        //            }
        //            else
        //            {
        //                if (gssDataTemp.AllStoragePolicy.Where(item => item.ID == gssData.CurrentStoragePolicy.ID).Count() != 0)
        //                {
        //                    gssDataTemp.CurrentStoragePolicy = gssData.CurrentStoragePolicy;
        //                }
        //                else
        //                {
        //                    gssDataTemp.CurStoragePolicyRemoved = true;
        //                }
        //            }
        //        }

        //        if (gssData.CurrentSecurityProfile != null && gssData.CurrentSecurityProfile.Name != "")
        //        {
        //            if (gssDataTemp.AllSecurityProfile.Where(item => item.ID == gssData.CurrentSecurityProfile.ID).Count() != 0)
        //            {
        //                gssDataTemp.CurrentSecurityProfile = gssData.CurrentSecurityProfile;
        //            }
        //            else
        //            {
        //                gssDataTemp.CurSecurityProfileRemoved = true;
        //            }
        //        }
        //        gssDataTemp.UseCompression = gssData.UseCompression;
        //        gssDataTemp.UseEncryption = gssData.UseEncryption;
        //        gssDataTemp.CompressionSpeed = gssData.CompressionSpeed;
        //        gssDataTemp.CompressionMethod = gssData.CompressionMethod;
        //        gssDataTemp.EncryptionMethod = gssData.EncryptionMethod;
        //        #endregion


        //        return JsonConvert.SerializeObject(gssDataTemp);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error("Get Global Storage Setting  Failed.{0}", ex.Message);
        //        return JsonConvert.SerializeObject(gssDataTemp);
        //    }
        //}

        //[RACodeReview("Allen Yin")]
        //[HttpPost]
        //public string StorageSettings([FromBody]GlobalStorageSetting newGssDataTemp)
        //{
        //    RAReturnMessage message = new RAReturnMessage() { MessageType = RAMessageType.Successful };
        //    lock (upGlobalSettingsLocker)
        //    {
        //        //bool saveGssDataSucess = true;
        //        try
        //        {
        //            if (newGssDataTemp?.CurrentExportLocation?.ID != null)
        //            {
        //                var exportLocationDic = GlobalSettingService.GetExportLocationTypes();
        //                Guid exportLocationId = Guid.Empty;
        //                if (Guid.TryParse(newGssDataTemp.CurrentExportLocation.ID.ToString(), out exportLocationId))
        //                {
        //                    if (exportLocationDic.ContainsKey(exportLocationId) && exportLocationDic[exportLocationId] == 1)
        //                    {
        //                        message.MessageType = RAMessageType.Failed;
        //                        message.ErrorMessage = I18NEntity.GetString("RM_JS_CP_GSS_FTPExportLocationNotSupported");
        //                        return JsonConvert.SerializeObject(message);
        //                    }
        //                }
        //            }
        //            ArgumentCheck.NotNull(newGssDataTemp, nameof(newGssDataTemp));
        //            newGssDataTemp.Id = 1;
        //            GlobalSettingService.SaveOrUpdate(newGssDataTemp);
        //            return JsonConvert.SerializeObject(message);
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.Error("Save Global Storage Setting Failed.{0}", ex.Message);
        //            //saveGssDataSucess = false;
        //            message.MessageType = RAMessageType.Failed;
        //            message.ErrorMessage = I18NEntity.GetString("RM_JS_CP_GSS_UsedItemRemoved");
        //            return JsonConvert.SerializeObject(message);
        //        }
        //    }

        //}

        #endregion

        #region Authentication Manager

        [RACodeReview("Allen Yin")]
        [HttpPost]
        public List<RMAuthenticationDto> GetAuthentication()
        {
            return AuthenticationManagerService.GetAuthenticationModes(false, true, false);
        }

        [RACodeReview("Allen Yin")]
        [HttpPost]
        public bool EnableAuthentication([FromBody]int id)
        {
            return AuthenticationManagerService.EnableAuthenticationMode(id);
        }

        [RACodeReview("Allen Yin")]
        [HttpPost]
        public bool DisableAuthentication([FromBody]int id)
        {
            return AuthenticationManagerService.DisableAuthenticationMode(id);
        }

        [RACodeReview("Allen Yin")]
        [HttpPost]
        public bool SetDefaultAuthentication([FromBody]int id)
        {
            return AuthenticationManagerService.SetDefaultAuthenticationMode(id);
        }

        [RACodeReview("Allen Yin")]
        [HttpPost]
        public RMOperatingDomainError AddADDomain([FromBody]RMDomainDto infor)
        {
            RMOperatingDomainError errorType;
            AuthenticationManagerService.AddADDomain(infor, out errorType);
            return errorType;
        }

        [RACodeReview("Allen Yin")]
        [HttpPost]
        public bool DelDomain([FromBody]List<int> domainIds)
        {
            return AuthenticationManagerService.DeleteADDomain(domainIds);
        }

        [RACodeReview("Allen Yin", comment: "方法名字不太贴切，实际上是切换enable disable状态")]
        [HttpPost]
        public bool EnableDomain([FromBody]ControlPanelModule domainIds)
        {
            return AuthenticationManagerService.UpdateADDomainStatus(domainIds.DomainIds, domainIds.Enable);
        }

        [RACodeReview("Allen Yin")]
        [HttpPost]
        public bool UpdateDomain([FromBody]RMDomainDto infor)
        {
            return AuthenticationManagerService.UpdateADDomainUserInfo(infor.Id, infor.UserName, infor.Password);
        }

        #endregion

        #region Account Manager

        /// <summary>
        /// 测试用, Web API
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public string GetAdminSecurityToken(string userName, string password)
        {
            string token = null;
            try
            {
                token = AccountManagerService.GetAdminSecurityToken(password);
            }
            catch (Exception e)
            {
                Logger.Warn($"An error has occurred when Get Sec Token, message:{e.Message}");
            }
            return token;
        }

        [HttpPost]
		[RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public Task<UserQueryResult> QueryUsers([FromBody] UserQueryParams queryDto)
        {
            //UserQueryResult result = null;
            //if (queryDto == null)
            //{
            //    queryDto = new UserQueryParams { PageIndex = 1, PageSize = 10 };
            //    result = UserService.QueryUsers(queryDto);
            //}
            //return result;
            return UserService.QueryUsersAsync(queryDto);
        }

        [HttpGet]
		[RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<string> GetUserPermissionScopes(string id)
        {
            var userPermissions = await SecurityGroupManagementService.GetUserScopePermissionsAsync(id);
            return JsonConvert.SerializeObject(userPermissions); 
        }

        [HttpGet]
        public async Task<List<string>> GetViewedLocationPaths(string id)
        {
    
            var userAndGroupIds = await UserService.GetUserAndGroupIdsAsync(id);
            return PermissionManagementService.GetlocationPathsCanBeViewed(userAndGroupIds);
        }
        #endregion

        #region General Setting

        [RACodeReview("Allen Yin")]
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin, RMDiscoveryPermissionMasks.AccessAll, RMDiscoverySalesforcePermissionMask.AccessAll, RMDiscoveryGoogleROTPermissionMask.AccessAll, RMDiscoveryFileSystemPermissionMask.AccessAll)]
        public async Task<GeneralSettingJsModel> GetGeneralSetting()
        {
            GeneralSettingJsModel generalSettingJsModel = new GeneralSettingJsModel();
            generalSettingJsModel.GeneralSettingModel = await GeneralSettingService.GetGeneralSettingAsync();
            return generalSettingJsModel;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin, RMDiscoveryPermissionMasks.AccessAll, RMDiscoverySalesforcePermissionMask.AccessAll, RMDiscoveryGoogleROTPermissionMask.AccessAll, RMDiscoveryFileSystemPermissionMask.AccessAll)]
        public async Task<bool> CheckEmailSenderDefinition([FromBody] GeneralSettingModel generalSettingModel)
        {
            return await GeneralSettingService.CheckEmailSenderDefinition(generalSettingModel.EmailSenderDefinition);
        }

        [RACodeReview("Allen Yin")]
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin, RMDiscoveryPermissionMasks.AccessAll, RMDiscoverySalesforcePermissionMask.AccessAll, RMDiscoveryGoogleROTPermissionMask.AccessAll, RMDiscoveryFileSystemPermissionMask.AccessAll)]
        public async Task<object> SaveOrUpdateGeneralSetting([FromBody]GeneralSettingModel generalSettingModel)
        {
            if (!await GeneralSettingService.CheckEmailSenderDefinition(generalSettingModel.EmailSenderDefinition))
            {
                return false;
            }

            if (await GeneralSettingService.SaveOrUpdateGeneralSettingAsync(generalSettingModel))
            {
                TimeSettingModel tsm = await GeneralSettingService.GetTimeSettingModelAsync(TenantLocalValue.LogonGroupId);
                return tsm;
            }
            else
            {
                return false;
            }
        }

        [HttpGet]
        public HttpResponseMessage DownloadPhysicalSolution()
        {
            try
            {
                var solutionFilename = "EndUserSolution.zip";
                var solutionFolderPath = Path.Combine(WebUtil.GetInstallPath(), "EndUserSolution");
                var solutionFilePath = Path.Combine(solutionFolderPath, solutionFilename);

                string downloadFilename = Path.GetFileName(solutionFilename);
                var stream = new FileStream(solutionFilePath, FileMode.Open);
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(stream);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = downloadFilename
                };
                return response;
            }
            catch
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }
        }
        #endregion

        #region Export Setting

        [HttpPost]
        //[FileDownloadFilter]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public ActionResult DownSavedloadFile()
        {
            try
            {
                string filename;
                var stream = ExportSettingService.DownloadConfigureFileToStream(out filename);
                stream.Position = 0;
                return File(stream, GetContentType(filename), filename);
			}
            catch
            {
				return new StatusCodeResult((int)HttpStatusCode.NoContent);
			}
        }

        [HttpPost]
        //[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        //[FileDownloadFilter]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public ActionResult DownSavedloadNaaFile()
        {
            try
            {
                string filename;
                var stream = ExportSettingService.DownloadNAAConfigureFileToStream(out filename);
                stream.Position = 0;
                return File(stream, GetContentType(filename), filename);
			}
            catch
            {
				return new StatusCodeResult((int)HttpStatusCode.NoContent);
			}
        }
        [HttpPost]
        //[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        //[FileDownloadFilter]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public ActionResult DownSavedloadNaraFile()
        {
            try
            {
                string filename;
                var stream = ExportSettingService.DownloadNARAConfigureFileToStream(out filename);
                stream.Position = 0;
                return File(stream, GetContentType(filename), filename);
			}
            catch
            {
				return new StatusCodeResult((int)HttpStatusCode.NoContent);
			}
        }
        [HttpPost]
        //[FileDownloadFilter]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public ActionResult DownloadSavedDedupSettingFile()
        {
            try
            {
                string filename;
                var stream = ArchiverSettingsService.DownloadDedupSettingsFileToStream(out filename);
                stream.Position = 0;
                return File(stream, GetContentType(filename), filename);
            }
            catch
            {
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }
        }
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public CP.ExportSetting GetSavedFile()
        {
            double outSize = 0;
            bool isActive = false;
            var fileName = ExportSettingService.GetSavedFileName(out outSize, out isActive);
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }
            CP.ExportSetting es = new CP.ExportSetting();
            es.FileName = fileName;
            es.FileSize = outSize.ToString("f2");
            es.IsActive = isActive;
            return es;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public Task<CP.ExportSettingEx> GetSavedFileInfos()
        {
            return ExportSettingService.GetSavedFileInfosAsync();
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        [ValidateOnlyGoogleLicenseFilter]
        public Dictionary<string, string> GetSavedDedupTemplate()
        {
            return ArchiverSettingsService.GetSavedDedupFileInfo();
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<JsonResult> UpdateDedupSettingFile()
        {
            try
            {
                if (!SettingProfileService.IsEnableArchiverDeduplication())
                {
                    return new JsonResult(new { success = false, message = "Update failed!", details = "De-duplication not enabled" });
                }

                var file = Request.Form.Files["fileUp"];
                string fileName = null;
                Stream fileStream = null;

                if (file != null)
                {
                    fileName = Path.GetFileName(file.FileName);
                    var ext = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
                    WebUtil.CheckFileExtension(ext, new List<FileExtension> { FileExtension.XLSX });
                    WebUtil.CheckFileSize(file.Length, 5);
                    fileStream = file.OpenReadStream();
                }

                var uploadResult = await ArchiverSettingsService.UpdateDedupSettingFile(fileName, fileStream);
                
                //这样判断是不合理的，这里处理更细一些，一个成功一个没有成功，应该是exception不是erro
                if (uploadResult)
                {
                    return new JsonResult(new { success = true, message = "Update success!" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Update failed!" });
                }
            }
            catch (Exception e)
            {
                this.Logger.Error("Update file error, {0}", e.ToString());
                return new JsonResult(new { success = false, message = "Update failed!", details = e.Message });
            }
        }


        //该方法目前没有引用
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public bool DeleteSavedFile()
        {
            return !string.IsNullOrEmpty(ExportSettingService.DeleteConfigureFileName());
        }
        [HttpPost]
        //[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        //[FileDownloadFilter]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public IActionResult DownloadTemplateZip()
        {
            var filePath = ExportSettingService.DownloadTemplateZip("VEO Configuration Files.zip");
            return DownloadTemplate(filePath);
        }


        /// <summary>
        /// Support VEO Vers-3
        /// </summary>
        /// <returns></returns>
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public IActionResult DownloadVEOV3TemplateZip()
        {
            var filePath = ExportSettingService.DownloadTemplateZip("VEO V3 Configuration Files.zip");
            return DownloadTemplate(filePath);
        }

        [HttpPost]
        //[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        //[FileDownloadFilter]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public IActionResult DownloadNAATemplateZip()
        {
            var filePath = ExportSettingService.DownloadTemplateZip("NAA Configuration File.zip");
            return DownloadTemplate(filePath);
        }

        [HttpPost]
        //[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        //[FileDownloadFilter]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public IActionResult DownloadNARATemplateZip()
        {
            var filePath = ExportSettingService.DownloadTemplateZip("NARA Configuration File.zip");
            return DownloadTemplate(filePath);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public IActionResult DownloadDedupTemplate()
        {
            if (!SettingProfileService.IsEnableArchiverDeduplication())
            {
                return new StatusCodeResult((int)HttpStatusCode.Forbidden);
            }
            var filePath = ArchiverSettingsService.DownloadDedupTemplate();
            return DownloadTemplate(filePath);
        }

        private IActionResult DownloadTemplate(string filePath) 
        {
            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                stream.CopyTo(memoryStream);
            }
            memoryStream.Position = 0;
            return File(memoryStream, GetContentType(filePath), Path.GetFileName(filePath));
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
        //该方法目前没有引用
        //[HttpGet]
        //public object ExportSettignsOnlyChangeActived(string isActived)
        //{
        //    var active = bool.Parse(isActived);
        //    ExportSettingService.ExportSettignsOnlyChangeActived(active);
        //    return new { success = true, message = I18NEntity.GetString("RM_ES_SaveScuessfully") };
        //}
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public RAReturnMessage GetCurrentAesKey()
        {
            return ExportDataEncryptionSettingService.GetCurrentAesKey();
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public RAReturnMessage GenerateAesKey()
        {
            return ExportDataEncryptionSettingService.GenerateAesKey();
        }
      
        #endregion

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<EmailTemplatesInfo> GetAllEmailTemplate([FromBody] GetAllEmailTemplateDto getAllTemplateDto)
        {
            EmailTemplateService.InitDefaultData();
			return await EmailTemplateService.GetAllTemplateDatas(getAllTemplateDto);
        }

		[HttpPost]
		[RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser)]
		public List<EmailTemplateDto> GetAllCustomEmailTemplates()
		{
			return EmailTemplateService.GetAllCustomEmailTemplates();
		}

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public EmailTemplateDto GetAllEmailTemplateById(int id, bool isCopy)
        {
            var template = EmailTemplateService.GetEmailTemplateById(id);
            if (isCopy && template.Type != (int)EmailTemplateType.RecordsForReview)
            {
                return null;
            }
            if (isCopy)
            {
                template.UniqueId = Guid.NewGuid();
                template.Name = template.Name + " - " + I18NEntity.GetString("RM_JS_Common_Copy");
                template.IsCustomTemplate = true;
            }
            return template;
        }

		[HttpPost]
		[RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin)]
		public EmailTemplateDto GetCustomDefaultEmailTemplate(EmailTemplateInternalType type)
		{
			return EmailTemplateService.GetCustomDefaultEmailTemplate(type);
		}

		[HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        [ValidEmailTemplateParamenterFilter("ValidateTemplateLength")]
        public async Task<string> EditEamilTemplate([FromBody] EmailTemplateDto eamil)
        {
            return await RouteMultiGeoApiActionAsync(
               eamil,
               MultiGeoOperationType.EditEmailTemplate,
               request =>
               {
                   var result = EmailTemplateService.UpdateEmailTemplate(request);
                   return Task.FromResult(result);
               },
               _ => "-2");
        }

		[HttpPost]
		[RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin)]
		[ValidEmailTemplateParamenterFilter("ValidateTemplateLength")]
        public async Task<string> CreateEmailTemplate([FromBody] EmailTemplateDto eamil)
        {
            return await RouteMultiGeoApiActionAsync(
                eamil,
                MultiGeoOperationType.CreateEmailTemplate,
                request =>
                {
                    var result = EmailTemplateService.CreateEmailTemplate(request);
                    return Task.FromResult(result);
                },
                _ => "-2");
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin)]
        [ValidEmailTemplateParamenterFilter("ValidateTemplateGuid")]
        public async Task<string> DeleteEmailTemplate([FromBody] Guid uniqueId)
        {
            return await RouteMultiGeoApiActionAsync(
                 uniqueId,
                 MultiGeoOperationType.DeleteEmailTemplate,
                 request =>
                 {
                     var result = EmailTemplateService.DeleteEmailTemplate(request);
                     return Task.FromResult(result);
                 },
                 _ => "-2");
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        [ValidEmailTemplateParamenterFilter("ValidateUploadImage")]
        public async Task<EmailImageDto> UploadImage([FromForm] IFormFile fileUp, [FromForm] string templateId)
        {
            var fileType = fileUp.FileName[(fileUp.FileName.LastIndexOf('.') + 1)..];
            Logger.Info("Email image file,file name :{0}", fileUp.FileName);
            return await EmailTemplateService.UploadImage(fileUp.OpenReadStream(), templateId, fileType);
        }

        #region Manual Approval Process

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser)]
        [ValidWorkflowParameterFilter()]
        public Task<RAReturnMessage> SaveManualProcess([FromBody] WorkflowDefinitionDto dto)
        {
            return RouteMultiGeoApiActionAsync(
                dto,
                MultiGeoOperationType.SaveManualProcess,
                async request =>
                {
                    var result = await ManualProcessManagementService.SaveAsync(request);
                    return result;
                },
                (request, _) =>
                {
                    ManualProcessManagementService.PrepareManualProcessReplicaRequest(request);
                    return Task.CompletedTask;
                },
                _ => new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage")
                });
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser)]
        public WorkflowDefinitionDto LoadManualProcess(Guid id)
        {
            return ManualProcessManagementService.LoadProcess(id);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser)]
        public Task<RAReturnMessage> DeleteManualProcess([FromBody]Guid id)
        {
            return RouteMultiGeoApiActionAsync(
                id,
                MultiGeoOperationType.DeleteManualProcess,
                ManualProcessManagementService.DeleteProcessAsync,
                _ => new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_Multi_Geo_Update_Common_ErrorMessage")
                });
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser)]
        public Task<QueryProcessesResultDto> GetManualProcesses([FromBody] ProcessQueryDto dto)
        {
            return ManualProcessManagementService.GetProcessesAsync(dto);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser)]
        public bool IsUpgradeWorkflowVersion([FromBody] WorkflowDefinitionDto dto)
        {
            return ManualProcessManagementService.IsUpgradeVerion(dto);
        }
        #endregion

        #region Permission Scope Synchronisation Job
        [HttpPost]
        public bool RunPSSJob([FromBody] string syncNoeJobId)
        {
            var jobId = SecurityContainerService.RunScheduleJob(JobRunBy.Control, syncNoeJobId);
            return !string.IsNullOrEmpty(jobId);
        }
        #endregion

        #region Security Group Management
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<RAReturnMessage> CreateGroup([FromBody] SecurityGroupDto group)
        {
            var syncUsersResult = await SecurityGroupManagementService.SyncADUsersAsync(group.Users);
            if (syncUsersResult.MessageType != RAMessageType.Successful)
            {
                return syncUsersResult;
            }
            return await SecurityGroupManagementService.CreateGroupAsync(group);
        }


        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public  Task<RAReturnMessage> ValidateGroup([FromBody] ValidateSecurityGroupDto vGroup)
        {
            return SecurityGroupManagementService.ValidateGroupTermAndRuleAsync(vGroup);
        }


        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<RAReturnMessage> EditGroup([FromBody] SecurityGroupDto group)
        {
            var syncUserResult = await SecurityGroupManagementService.SyncADUsersAsync(group.Users);
            if (syncUserResult.MessageType != RAMessageType.Successful)
            {
                return syncUserResult;
            }
            return await SecurityGroupManagementService.EditGroupAsync(group);
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<string> LoadGroups()
        {
            var groups = await SecurityGroupManagementService.GetGroupsAsync();
            return JsonConvert.SerializeObject(groups);
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public string LoadAssignContainerIds()
        {
            var assignContainerIds = SecurityGroupManagementService.GetAllAssignContainerIds();
            return JsonConvert.SerializeObject(assignContainerIds);
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<string> LoadGroup(int id)
        {
            var group = await SecurityGroupManagementService.GetGroupAsync(id);
            if (group.SetTermPermissionMethod == TermPermissionMethod.SpecifyScope)
            {
                group.TermTreeNodeInfo = TaxonomyService.BuildSecurityTermTree(group.TermTreeNodeInfo, id);
            }
            else if (group.SetTermPermissionMethod == TermPermissionMethod.All)
            {
                var treeNodeInfo = SecurityGroupManagementService.GetSecurityTermRootNode();
                treeNodeInfo.IsChecked = true;
                group.TermTreeNodeInfo = TaxonomyService.BuildSecurityTermTree(treeNodeInfo, id);
            }
            else if (group.SetTermPermissionMethod == TermPermissionMethod.None)
            {
                group.TermTreeNodeInfo = null;
            }

            if (group.SetRulePermissionMethod == RulePermissionMethod.SpecifyScope)
            {
                group.RuleTreeNodeInfo = TaxonomyService.BuildSecurityRuleTree(group.RuleTreeNodeInfo, id);
            }
            else if (group.SetRulePermissionMethod == RulePermissionMethod.All)
            {
                var treeNodeInfo = SecurityGroupManagementService.GetSecurityRuleRootNode();
                treeNodeInfo.IsChecked = true;
                group.RuleTreeNodeInfo = TaxonomyService.BuildSecurityRuleTree(treeNodeInfo, id);
            }
            else if (group.SetRulePermissionMethod == RulePermissionMethod.None)
            {
                group.RuleTreeNodeInfo = null;
            }

            if (group.Id != (int)BuiltInGroupId.Admin && ((RMSOPermissionMasks)group.SOPermissionMasks).UserHasThisPermission(RMSOPermissionMasks.RestoreCenterSearch))
            {
                group.SecurityGroupControlType = SecurityGroupControlType.FunctionModule;
                if (((RMSOPermissionMasks)group.SOPermissionMasks).UserHasThisPermission(RMSOPermissionMasks.RestoreCenterFullControl))
                {
                    group.FunctionSubPermission = FunctionSubPermission.RestoreCenterFullControl;
                }
                else if (((RMSOPermissionMasks)group.SOPermissionMasks).UserHasThisPermission(RMSOPermissionMasks.RestoreCenterExport))
                {
                    group.FunctionSubPermission = FunctionSubPermission.RestoreCenterExport;
                }
                else if (((RMSOPermissionMasks)group.SOPermissionMasks).UserHasThisPermission(RMSOPermissionMasks.RestoreCenterSearch))
                {
                    group.FunctionSubPermission = FunctionSubPermission.RestoreCenterSearch;
                }
            }

            return JsonConvert.SerializeObject(group);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public Task<bool> DeleteGroup([FromBody]int id)
        {
            return SecurityGroupManagementService.DeleteGroupAsync(id);
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<RAReturnMessage> LoadContainers(SourceFlag source)
        {
            var message = new RAReturnMessage();
            try
            {
                var containers = new List<SecurityContainerDto>();
                if (source == SourceFlag.SharePoint)
                {
                    var farmNode = mSPSettingTreeService.LoadFarmSampleTree()[0];
                    if (farmNode != null && !farmNode.Id.Equals(Guid.Empty))
                    {
                        List<RMSPSampleTreeNode> children = new List<RMSPSampleTreeNode>();
                        children = await mSPSettingTreeService.BrowseSampleTreeAsync(farmNode, false);
                        foreach (var item in children)
                        {
                            containers.Add(new SecurityContainerDto
                            {
                                Id = item.Id,
                                Name = item.Name
                            });
                        }
                    }
                }
                else if (source == SourceFlag.Exchange)
                {
                    var exoRootNode = mSPSettingTreeService.LoadExchangeRoot()[0];
                    if (exoRootNode != null && !exoRootNode.Id.Equals(System.Guid.Empty))
                    {
                        List<RMSampleEXOTreeNode> children = new List<RMSampleEXOTreeNode>();
                        children = (await mSPSettingTreeService.BrowseSampleExchangeTreeAsync(exoRootNode)).OrderBy(a => a.Name).ToList();
                        foreach (var item in children)
                        {
                            containers.Add(new SecurityContainerDto
                            {
                                Id = item.Id,
                                Name = item.Name
                            });
                        }
                    }
                }
                message.Extension = JsonConvert.SerializeObject(containers);
            }
            catch (Exception e)
            {
                Logger.Error($"An error while load containers, message:{e}");
                message.MessageType = RAMessageType.Failed;
            }
            return message;
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<string> LoadGroupsAndContainers()
        {
            var groups = await SecurityGroupManagementService.GetGroupsAsync();
            var result = new GroupsAndContainers
            {
                GroupItems = groups,
                SPContainerItems = await SecurityGroupManagementService.GetContainersAsync(SourceFlag.SharePoint),
                EXOContainerItems = await SecurityGroupManagementService.GetContainersAsync(SourceFlag.Exchange),
                OneDriveContainerItems = await SecurityGroupManagementService.GetContainersAsync(SourceFlag.OneDrive),
                TeamsContainerItems = await SecurityGroupManagementService.GetContainersAsync(SourceFlag.Teams),
                PhysicalLocationItems = await SecurityGroupManagementService.GetContainersAsync(SourceFlag.Physical),
            };
            return JsonConvert.SerializeObject(result);
        }


        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public string LoadTermObjData([FromBody] QueryTermObjDto dto)
        {
            return TaxonomyService.GetTermTreeForSecurityGroup(dto);
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<string> GetTermSettings(string userId, [BindRequired] SecurityTermLevel level, Guid parentId)
        {
            var termPermissionInfo = await SecurityGroupManagementService.GetSecurityTermObjInfoAsync(new QuerySecurityTermObjDto
            {
                UserId = userId,
                Level = level,
                ParentId = parentId
            });
            return JsonConvert.SerializeObject(termPermissionInfo); 
        }


        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public string LoadRuleObjData([FromBody] QueryRuleObjDto dto)
        {
            return RuleContainerService.GetRuleTreeForSecurityGroup(dto);
        }

        #endregion

        #region CSD Feature
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin)]
        public async Task<CSDPageInfo<CSDApiKeyDto>> GetCSDKeys([FromBody] CSDPageInfo<CSDApiKeyDto> info)
        {
            (info.Data,info.TotalCount) = await CSDCommonService.GetApiKeysAsync(info.PageIndex, info.PageSize);
            return info;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin)]
        public async Task<string> AddCSDKey([FromBody] CSDApiKeyDto dto)
        {
            try
            {
                if (!ValidApiKeyInfo(dto, out var msg))
                {
                    return msg;
                }
                if (await CSDCommonService.AddApiKeyAsync(dto.Name, dto.Expired.Value, dto.OperatorLoginName))
                {
                    return "true";
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Add key: {0} failed. {1}", dto.Name, ex.ToString());
            }

            return "false";
        }

        [HttpPost]
        public async Task<string> EditCSDKey([FromBody] CSDApiKeyDto dto)
        {
            try
            {
                if (!ValidApiKeyInfo(dto, out var msg))
                {
                    return msg;
                }
                if (await CSDCommonService.EditApiKeyAsync(dto.Id, dto.Name, dto.Expired.Value, dto.OperatorLoginName))
                {
                    return "true";
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Edit key: {0} failed. {1}", dto.Name, ex.ToString());
            }

            return "false";
        }

        [HttpPost]
        public string RemoveCSDKey([FromBody] List<CSDApiKeyDto> keys)
        {
            try
            {
                if (CSDCommonService.RemoveApiKeys(keys.Select(k => k.Id)))
                {
                    return "true";
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Remove key: [{0}] failed. {1}", string.Join(", ", keys.Select(k => k.Name)), ex.ToString());
            }

            return "false";
        }

        private bool ValidApiKeyInfo(CSDApiKeyDto keyInfo, out string message)
        {
            message = null;
            if (string.IsNullOrEmpty(keyInfo.Name?.Trim()))
            {
                message = "emptyKeyName";
            }
            else if (keyInfo.Name.Length > 255)
            {
                message = "tooLongKeyName";
            }
            else if (keyInfo.Expired == null)
            {
                message = "emptyKeyExpiredTime";
            }
            else if (CSDCommonService.ExistsKeyName(keyInfo.Id, keyInfo.Name))
            {
                message = "duplicateKeyName";
            }
            else if (string.IsNullOrWhiteSpace(keyInfo.OperatorLoginName?.Trim()))
            {
                message = "emptyKeyOperator";
            }
            else if (keyInfo.OperatorLoginName.Length > 255)
            {
                message = "tooLongKeyOperator";
            }
            else
            {
                return true;
            }

            return false;
        }
        #endregion

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin | RMPermissionMasks.ManageHold, RMPermissionExtensionMasks.ManageHoldEndUser, PermissionJoinType.Any, PermissionJoinType.Any)]
        public async Task<AddUserPageInfo> SearchUsersByPermissionScope([FromQuery] string keyword)
        {
            var users = await SecurityGroupManagementService.SearchUsersByPermissionScopeAsync(keyword);
            return new AddUserPageInfo
            {
                Users = users,
                StatusMsg = users.Count > 0
                    ? string.Format(I18NEntity.GetString("RM_CP_AM_AddUser_UsersCount"), users.Count)
                    : I18NEntity.GetString("RM_CP_AM_AddUser_NoUserFound")
            };
        }
    }
}