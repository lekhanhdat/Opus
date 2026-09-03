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
using Amazon.Runtime.Internal.Util;
using AvePoint.Api.Service.Implement;
using AvePoint.Common.RemoteNode.Impl;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Api.Contract;
using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Api.Web.Controllers;
using AvePoint.RA.Api.Web.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.RMWeb.Dashboard;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.Service.Services.Archiver;
using AvePoint.RA.Service.Services.Archiver.Restore;
using AvePoint.RA.Service.Services.Dashboard;
using AvePoint.RA.Service.Services.StorageDevice;
using AvePoint.RA.SharePoint.Archiver.Scan.Base;
using Cloud.Sdk.Core;
using DocAveOnline.WebApi.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvePoint.Api.Web.ApiControllers
{
    [Route("api/archiverapi/[action]")]
    //[Authorize]
    [ApiController]
    public class ArchiversApiController : RAWebApiBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(ArchiversApiController));

        private Service.Interface.IArchiverService ArchiverService { get { return new ArchiverService(); } }

        private AvePoint.Common.Api.Services.IArchiverService ControlArchiverService => PlatformWindsorManager.GetService<Common.Api.Services.IArchiverService>();

        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();

        private  ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private IRestoreSearchService _RestoreSearchService;
        private IRestoreSearchService RestoreSearchService => PlatformWindsorManager.GetService(ref _RestoreSearchService);
        //private IRMArchiverSettingsService _RMArchiverSettingsService => PlatformWindsorManager.GetService<IRMArchiverSettingsService>();
        private IRMArchiverSettingsService _RMArchiverSettingsService { get { return new RMArchiverSettingsService(); } }

        private IDashboardService _DashboardService { get { return new DashboardService(); } }

        /// <summary>
        /// advance search 
        /// </summary>
        /// <param name="searchCondition">search condition</param>
        /// <returns>return result if search success</returns>
        /// <response code="500">An error occured.</response>
        /// <response code="200">return result if search success.</response>
        /// <response code="401">Authorize header failed.</response>
        [HttpPost]
        [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridInernalScope)]
        public Task<SearchResult> AdvanceSearch([FromBody]AdvanceSearchCondition searchCondition)
        {
            return ArchiverService.AdvanceSearchAsync(searchCondition);
        }

        [HttpPost]
        public Task<ArchiverRestoreResult> AOSPAdvanceSearch([FromBody] AdvanceSearchCondition searchCondition)
        {
            return ArchiverService.AOSPAdvanceSearchAsync(searchCondition);
        }

        [HttpPost]
        public async Task<List<RMDiscoveryReturnMessage>> AOSPBatchRestoreSiteCollection([FromBody] AOSPRestoreInfo info)
        {
            string infoString = SerializerHelper.SerializeByJsonConvert(info);
            logger.Info($"start AOSPBatchRestoreSiteCollection with site collection urls, params :{infoString}");

            List<RMDiscoveryReturnMessage> result = new List<RMDiscoveryReturnMessage>();
            foreach (var siteUrl in info.SiteUrls)
            {
                AOSPRestoreInfo tempInfo = SerializerHelper.DeserializeByJsonConvert<AOSPRestoreInfo>(infoString);
                NormalizeSpecifyUserSettings(tempInfo);
                string siteCollectionUrl = siteUrl;
                RMDiscoveryReturnMessage returnMessage = new();
                try
                {
                    ArchiverRestoreResult searchResult = await ArchiverService.AOSPAdvanceSearchAsync(new AdvanceSearchCondition()
                    {
                        Scope = siteCollectionUrl,
                        ModuleType = DocAveOnline.WebApi.Contracts.ModuleType.SharePointOnline,
                        IsAOSPSearch = true,
                        Page = 1,
                        Size = 10,
                        Keyword = "",
                        PolicyLevel = (int)PolicyLevel.SiteCollection
                    });

                    if (searchResult == null || searchResult.RestoreSerchNodes == null || searchResult.RestoreSerchNodes.Count == 0)
                    {
                        logger.Warn($"no restore node found for site collection url:{siteCollectionUrl},skip it");
                        returnMessage.MessageType = RAMessageType.Failed;
                        returnMessage.ErrorMessage = $"No restore node found for site collection url:{siteCollectionUrl}";
                    }
                    else
                    {
                        tempInfo.NodeObjects = searchResult.RestoreSerchNodes;
                        tempInfo.DataSource = 1;
                        returnMessage = InternalAOSPRestore(tempInfo);
                    }
                }
                catch(Exception ex)
                {
                    logger.Error($"something went wrong when AOSPBatchRestoreSiteCollection for site collection url:{siteCollectionUrl},error:{ex}");
                    returnMessage.MessageType = RAMessageType.Failed;
                    returnMessage.ErrorMessage = I18NEntity.GetString("RM_RS_SaveRestoreSettingError");
                }
                returnMessage.SiteCollectionUrl = siteCollectionUrl;
                result.Add(returnMessage);
            }
            logger.Info($"finish AOSPBatchRestoreSiteCollection,json:{SerializerHelper.SerializeByJsonConvert(result)}");
            return result;
        }


        [HttpPost]
        public RMDiscoveryReturnMessage SaveRestoreSettingAndRun([FromBody]AOSPRestoreInfo info)
        {
            return InternalAOSPRestore(info);
        }

        private RMDiscoveryReturnMessage InternalAOSPRestore(AOSPRestoreInfo info)
        {
            NormalizeSpecifyUserSettings(info);
            info.IsEndUserJob = false;
            var tempRestoreType = GCommon.Contract.StorageOptimization.Object.RestoreType.AOPSOop;
            var result = new RMDiscoveryReturnMessage();
            try
            {
                TenantService.CheckAndUpdateAOSPTenantAsync().GetAwaiter().GetResult();
                if (info.DataSource == (int)RestoreDataSource.M365)
                {
                    logger.Info("this restore data source is M365");
                    foreach (var tempInfo in BuildAOSPRestoreInfos(info))
                    {
                        var tempResult = RestoreSearchService.SaveAndRunRestoreJob(tempInfo, tempRestoreType);
                        if (string.IsNullOrEmpty(tempResult.Extension))
                        {
                            return new RMDiscoveryReturnMessage() { MessageType = tempResult.MessageType, ErrorMessage = tempResult.ErrorMessage };
                        }
                        result.JobId = tempResult.Extension;
                        result.MessageType = RAMessageType.Successful;
                    }
                }
                logger.Info($"finish run restore job");
            }
            catch (Exception e)
            {
                logger.Error($"something went wrong when save restore setting,error :{e}");
                return new RMDiscoveryReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_SaveRestoreSettingError") };
            }
            return result;
        }

        private List<RestoreInfo> BuildAOSPRestoreInfos(AOSPRestoreInfo info)
        {
            List<RestoreInfo> restoreInfos = new List<RestoreInfo>();
            Dictionary<string, List<ArchiverRestoreSerchResult>> siteWithObject = new Dictionary<string, List<ArchiverRestoreSerchResult>>();
            logger.Info("start generate restore setting nodes");
            var needRestoreObjects = info.NodeObjects;
            foreach (ArchiverRestoreSerchResult obj in needRestoreObjects)
            {
                if (siteWithObject.ContainsKey(obj.SitePath))
                {
                    siteWithObject[obj.SitePath].Add(obj);
                }
                else
                {
                    siteWithObject.Add(obj.SitePath, new List<ArchiverRestoreSerchResult>());
                    siteWithObject[obj.SitePath].Add(obj);
                    logger.Info($"siteWithObject not containe key:{obj.SitePath},add it");
                }
            }
            logger.Info($"finish ganerate siteWithObject,count:{siteWithObject.Count}");
            foreach (var tempKeyValue in siteWithObject)
            {
                logger.Info($"this site with object info is :key:{tempKeyValue.Key},value count:{tempKeyValue.Value?.Count}");
                var tempRestoreInfo = Clone(info);
                tempRestoreInfo.NodeObjects.Clear();
                tempRestoreInfo.NodeObjects = tempKeyValue.Value;
                restoreInfos.Add(tempRestoreInfo);
            }
            return restoreInfos;
        }

        [HttpPost]
        public async Task<RMDiscoveryReturnMessage> AddOrUpdateStorageDevice([FromBody] StorageDeviceUIDto dto)
        {
            if(ValidateStorageInfo(dto) != (int)CreateOrEditStatus.Success)
            {
                var msg = new RMDiscoveryReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = "ParameterIsIncorrect" };
                return msg;//(int)RAFailedType.ParameterIsIncorrect;
            }
            if (dto.UseCompression == false || dto.CompressionSpeed != 5)
            {
                dto.UseCompression = true;
                dto.CompressionSpeed = 5;
            }
            if (dto.mCurrentXRI.VIM == "azure_vim")
            {
                if (!StorageDeviceUtility.ValidateAzureAccessPoint(dto.mCurrentXRI.Params["accesspoint"]))
                {
                    var msg = new RMDiscoveryReturnMessage()
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = I18NEntity.GetString("RM_AR_Storage_Account_ErrorMessage")
                    };
                    return msg;
                }
                if (!ValidateAzureContainerName(dto.mCurrentXRI.Params["containername"]))
                {
                    var msg = new RMDiscoveryReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_AR_Storage_ContainerName_ErrorMessage") };
                    return msg;
                }
            }
            if (!dto.IsSystemStorage && dto.mCurrentXRI.Params["advanced"] == "true")
            {
                var paramString = dto.mCurrentXRI.Params["extendedparameters"];
                List<string> tempParaList = paramString.Split("\n").ToList();
                if (dto.Type == (int)GCommon.Contract.Storage.Entity.StorageDeviceType.Google)
                {
                    if (!ValidateGoogleAdvanceExtendedParameters(tempParaList, dto.mCurrentXRI.Params["containername"]))
                    {
                        var msg = new RMDiscoveryReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_AR_Storage_ExtendedParameters_ErrorMessage") };
                        return msg;
                    }
                }
                else
                {
                    if (!ValidateAdvanceExtendedparameters(tempParaList))
                    {
                        var msg = new RMDiscoveryReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_AR_Storage_ExtendedParameters_ErrorMessage") };
                        return msg;
                    }
                }
            }
            StorageDeviceDto mDto = ConvertUIDto2PhysicalDeviceDto(dto);
            var result = await StorageDeviceService.ValidateAndCreateStorageDeviceAsync(mDto, EntityObjectPermissionType.FullPermission);
            return new RMDiscoveryReturnMessage() { MessageType = result.MessageType, ErrorMessage = result.ErrorMessage };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridInernalScope)]
        public Task<JobResult> RunArchiverEndUserRestoreJob([FromBody]EndUserRestoreConfig config)
        {
            //Task<JobResult> RunEndUserRestoreJob(EndUserRestoreConfig config);
            return ControlArchiverService.RunArchiverEndUserRestoreJobAsync(config);
        }
        [HttpPost]
        [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridInernalScope)]
        public JobResult RunExportSearchResultJob([FromBody] EndUserRestoreConfig config)
        {
            //Task<JobResult> RunEndUserRestoreJob(EndUserRestoreConfig config);
            var result = ControlArchiverService.RunExportSearchResultJob(config);
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridInernalScope)]
        public async Task<StubParseResult> ParseStubString([FromBody]ParseStubParameters parseStubParameters)
        {
            var result = await ControlArchiverService.ParseStubStringAsync(parseStubParameters.StubString, parseStubParameters.Office365UserID, parseStubParameters.Office365TenantId);
            return result;
        }

        /// <summary>
        /// [Opus]
        /// start a job to restore archived content to azure storage
        /// </summary>
        /// <param name="archivedContentInfo"></param>
        /// <returns></returns>
        [HttpPost]
        [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridInernalScope)]
        public JobResult DownloadArchivedContent(ArchivedContentRestoreConfig archivedContentInfo)
        {
            //Task<JobResult> RunEndUserRestoreJob(EndUserRestoreConfig config);
            var result = ControlArchiverService.RunArchiverContentDownloadJob(archivedContentInfo);
            return result;
        }

        /// <summary>
        /// [Cloud Archiver]
        /// start a job to restore archived content to azure storage
        /// </summary>
        /// <param name="archivedContentInfo"></param>
        /// <returns></returns>
        [HttpPost]
        [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridInernalScope)]
        public JobResult RunArchivedContentExportJob(ExportArchivedContentConfig archivedContentInfo)
        {
            var result = ControlArchiverService.RunArchivedContentExportJob(archivedContentInfo);
            return result;
        }

        /// <summary>
        /// start a job to restore archived content to azure storage
        /// </summary>
        /// <param name="jobInfo"></param>
        /// <returns></returns>
        [HttpPost]
        [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridInernalScope)]
        public async Task<ExportedDataResult> RequestExportedDataSASByJobInfoAsync(ExportJobInfo jobInfo)
        {
            //Task<JobResult> RunEndUserRestoreJob(EndUserRestoreConfig config);
            var result = await ArchiverService.GetExportedDataSASByJobInfoAsync(jobInfo);
            return result;
        }

        /// <summary>
        /// get recenter end user setting
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridInernalScope)]
        public Task<EndUserRestoreSettingResult> GetEndUserSetting()
        {
            return ControlArchiverService.GetEndUserSettingAsync();
        }
        [HttpGet]
        [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridInernalScope)]
        public Task<bool> OpusStorageOptimizationEnabled()
        {
            return ArchiverService.OpusStorageOptimizationEnabled();
        }

        [HttpGet]
        public Task<int> GetTenantJobQueueCount()
        {
            return ArchiverService.GetTenantJobQueueCount();
        }

        [HttpPost]
        [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridInernalScope)]
        public Task<Byte[]> GetPhoto([FromBody] Microsoft365User microsoft365User)
        {
            return ArchiverService.GetPhotoAsync(microsoft365User);
        }

        [HttpPost]
        [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridInernalScope)]
        public Task<List<Microsoft365Group>> GetTeams([FromBody] Microsoft365User microsoft365User)
        {
            return ArchiverService.GetTeamsAsync(microsoft365User);
        }

        [HttpPost]
        [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridInernalScope)]
        public Task<List<Microsoft365Group>> GetGroups([FromBody] Microsoft365User microsoft365User)
        {
            return ArchiverService.GetGroupsAsync(microsoft365User);
        }
        [HttpPost]
        public Task<bool> ClearLicenseUsage()
        {
            return ArchiverService.ClearLicenseUsageAsync();
        }
        [HttpPost]
        [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridInernalScope)]
        public Task<List<string>> SearchAllStub([FromBody] Microsoft365User microsoft365User)
        {
            return ArchiverService.GetAllStubSearchResultAsync(microsoft365User);
        }
        [HttpPost]
        public Task<Stream> GetStubPreviewStream([FromBody]PreviewDataParam param)
        {
            var result = ArchiverService.GetStubPreviewStreamAsync(param);
            return result;
        }


        /// <summary>
        /// full text advance search 
        /// </summary>
        /// <param name="searchCondition">search condition</param>
        /// <returns>return result if search success</returns>
        /// <response code="500">An error occured.</response>
        /// <response code="200">return result if search success.</response>
        /// <response code="401">Authorize header failed.</response>
        [HttpPost]
        [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridInernalScope)]
        public Task<SearchResult> AdvanceFullTextSearch([FromBody] AdvanceSearchCondition searchCondition)
        {
            return ArchiverService.AdvanceFullTextAsync(searchCondition);
        }

        [HttpGet]
        [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridInernalScope)]
        public bool EnableFullTextIndex()
        {
            return RestoreSearchService.IsEnableFullTextIndexSearch();
        }

        [HttpGet]
        public async Task<bool> CheckWhiteListForGroupsTeams(string group)
        {
            return await RestoreSearchService.CheckWhiteListForGroupsTeamsAsync(group);
        }
        [HttpGet]
        public async Task<bool> CheckWhiteListForSharePointSite(string scUrl)
        {
            return await RestoreSearchService.CheckWhiteListForSharePointSiteAsync(scUrl);
        }


        [HttpPost]
        public string RunSpecifySitesArchiverBackup([FromBody] List<string> siteUrls)
        {
            var result = _RMArchiverSettingsService.RunSpecifySitesArchiverBackupJob(siteUrls);
            return result.Extension;
        }

        [HttpPost]
        public RMEndUserArchiveReturnMessage RunEndUserStorageOptimizationJob([FromBody] EndUserArchiveRequestParam request)
        {
            return _RMArchiverSettingsService.RunEndUserArchiverBackupJob(request);
        }

        [HttpPost]
        public string RunSpecifyTeamsArchiverBackup([FromBody] List<string> teamIdList)
        {
            var result = _RMArchiverSettingsService.RunSpecifyTeamsArchiverBackupJob(teamIdList);
            return result.Extension;
        }

        private RestoreInfo Clone(AOSPRestoreInfo aRestoreInfo)
        {
            var serialized = JsonConvert.SerializeObject(aRestoreInfo);
            var res = JsonConvert.DeserializeObject<RestoreInfo>(serialized);
            res.IsSpecifyUser = aRestoreInfo.IsSpecifyUser;
            res.SpecifyUserList = aRestoreInfo.SpecifyUserList ?? new List<ToExportUserInfo>();
            if (!res.IsSpecifyUser)
            {
                res.SpecifyUserList.Clear();
            }
            if (aRestoreInfo.RestoreOption == RestoreOption.OverWrite)
            {
                res.RestoreAPPOption = RestoreOption.OverWrite;
            }
            else
            {
                res.RestoreAPPOption = RestoreOption.NotOverWrite;
            }
            return res;
        }

        private void NormalizeSpecifyUserSettings(AOSPRestoreInfo info)
        {
            if (info == null)
            {
                return;
            }

            info.SpecifyUserList ??= new List<ToExportUserInfo>();
            if (!info.IsSpecifyUser)
            {
                info.SpecifyUserList.Clear();
            }
        }

        private StorageDeviceDto ConvertUIDto2PhysicalDeviceDto(StorageDeviceUIDto mUIDto)
        {
            StorageDeviceDto mDto = new StorageDeviceDto();
            mDto.Id = mUIDto.Id;
            mDto.Type = mUIDto.Type;
            mDto.Name = mUIDto.Name;
            mDto.Description = mUIDto.Description;
            if (mUIDto.Extension != null)
            {
                mDto.Extension = new StorageDeviceExtension()
                {
                    //AccountProfile = mUIDto.Extension.AccountProfile,
                    //SystemProfile = mUIDto.Extension.SystemProfile,
                    TotalSpace = mUIDto.Extension.TotalSpace,
                    UsedSpace = mUIDto.Extension.UsedSpace
                };
            }
            mDto.ArchiveRetentionRules = mUIDto.ArchiveRetentionRules;
            mDto.StorageDeviceSpace = mUIDto.StorageDeviceSpace;
            mDto.SpaceType = mUIDto.SpaceType;
            mDto.UseSpace = mUIDto.UseSpace;
            mDto.mCurrentXRI = mUIDto.mCurrentXRI;
            //mDto.LastArchivedTime = mUIDto.LastArchivedTime;
            //mDto.LastModifiedTime = mUIDto.LastModifiedTime;
            mDto.Schedule = mUIDto.Schedule;
            mDto.UseCompression = mUIDto.UseCompression;
            mDto.UseEncryption = mUIDto.UseEncryption;
            mDto.CompressionSpeed = mUIDto.CompressionSpeed;
            mDto.EncryptionProfileId = mUIDto.EncryptionProfileId;
            //XRI mCurrentXRI = GetAllStorageTypeXRI()[mUIDto.mCurrentXRI.VIM];

            //foreach (var dic in mUIDto.mCurrentXRI.Params)
            //{
            //    mCurrentXRI.Params[dic.Key] = dic.Value;
            //}

            //mDto.ConnectionString = mCurrentXRI.ToString();
            var builder = new ConnectionBuilder();
            builder.StorageName = mUIDto.mCurrentXRI.VIM;
            if (!mUIDto.IsSystemStorage && mUIDto.mCurrentXRI.VIM == "google_vim" && !mUIDto.mCurrentXRI.Params["secret"].Equals(new Guid().ToString()))
            {
                mUIDto.mCurrentXRI.Params["secret"] = mUIDto.mCurrentXRI.Params["secret"].Replace("\\n", "\n");
            }
            foreach (var dic in mUIDto.mCurrentXRI.Params)
            {
                if (dic.Key == "secret" && dic.Value.Equals(new Guid().ToString()))
                {
                    var tempDto = StorageDeviceService.GetStorageDeviceById(mDto.Id);
                    builder.Params.Add(dic.Key, tempDto.mCurrentXRI.Params.Where(a => a.Key == dic.Key).Select(a => a.Value).FirstOrDefault());
                }
                else
                {
                    builder.Params.Add(dic.Key, dic.Value);
                }
            }
            if (!builder.Params.ContainsKey(XRIParameterKeys.CREATE_IF_NOT_EXISTS))
            {
                builder.Params.Add(XRIParameterKeys.CREATE_IF_NOT_EXISTS, "true");
            }
            mDto.ConnectionString = builder.ToString();
            //mDto.LanguageType = GetCultureInfo(); //I18NUtility.curCulture;
            mDto.IsSystemStorage = mUIDto.IsSystemStorage;
            mDto.DAOMigrated = mUIDto.DAOMigrated ?? false;
            mDto.DAOStoragePolicyId = mUIDto.DAOStoragePolicyId;
            mDto.DAOLogicalDeviceId = mUIDto.DAOLogicalDeviceId;
            mDto.DAOPhysicalDeviceId = mUIDto.DAOPhysicalDeviceId;
            return mDto;
        }

        private bool ValidateAzureContainerName(string containerName)
        {
            Regex reg = new Regex("(?=^.{3,63}$)(?!.*--)(?!.*[A-Z])^[^-][0-9a-z-]*[^-]$");
            return reg.IsMatch(containerName);
        }

        private bool ValidateGoogleAdvanceExtendedParameters(List<string> extendedParams, string bucketName)
        {
            bool result = false;
            List<string> predefinedAclList = new List<string> { "authenticatedread", "private", "projectprivate", "publicread", "publicreadwrite " };
            List<string> predefinedDefaultObjectAclList = new List<string> { "authenticatedread", "bucketownerfullcontrol", "bucketownerread", "private", "projectprivate", "publicread" };
            List<string> projectionList = new List<string> { "full", "noacl" };
            foreach (string para in extendedParams)
            {
                var temp = para.Split("=");
                if (temp.Length != 2)
                {
                    return false;
                }
                string value = temp[1].ToLower();
                string key = temp[0].ToLower();
                switch (key)
                {
                    case "projection":
                        result = projectionList.Contains(value);
                        break;
                    case "predefinedacl":
                        result = predefinedAclList.Contains(value);
                        break;
                    case "predefineddefaultobjectacl":
                        result = predefinedDefaultObjectAclList.Contains(value);
                        break;
                    case "prefix":
                        result = true;
                        break;
                    default:
                        result = false;
                        break;
                }
            }
            return result;
        }

        private bool ValidateAdvanceExtendedparameters(List<string> extendedParams)
        {
            bool result = false;
            List<string> boolList = new List<string>() { "true", "false" };
            List<string> authmethodList = new List<string>() { "netuse", "logonuser", "netuse_deleteold" };
            List<string> locatortypeList = new List<string>() { "static", "proxy" };
            List<string> failovermodeList = new List<string>() { "read", "readwrite", "off" };
            List<string> customizedmodeList = new List<string>() { "close", "supportall", "docaveonly", "customizedonly" };
            foreach (string para in extendedParams)
            {
                var temp = para.Split("=");
                if (temp.Length != 2)
                {
                    return false;
                }
                string value = temp[1].ToLower();
                string key = temp[0].ToLower();
                Regex reg;
                switch (key)
                {
                    case "retryinterval":
                    case "retrycount":
                    case "remotehosttimeout":
                    case "secondarynamespacetimeout":
                        reg = new Regex("^[0-9]+$");
                        result = reg.IsMatch(value);
                        break;
                    case "customizedmetadata":
                        reg = new Regex("^\\{(\\[[^,]+,[^,]+\\],)*\\[[^,]+,[^,]+\\]\\}$");
                        result = reg.IsMatch(value);
                        break;
                    case "filespace":
                        reg = new Regex(".+");
                        result = reg.IsMatch(value);
                        break;
                    case "blocklength":
                        reg = new Regex("^[1-9]$|^[1-5][0-9]$|^6[0-4]$");
                        result = reg.IsMatch(value);
                        break;
                    case "signatureversion":
                    case "customizedregion":
                        reg = new Regex("^.*$");
                        result = reg.IsMatch(value);
                        break;
                    case "isretry":
                    case "cacheremotehost":
                    case "cachesecondarynamespace":
                    case "singlesession":
                    case "longpathenabled":
                    case "flushdns":
                    case "snaplockenabled":
                    case "enablessl":
                        if (boolList.Contains(value))
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                        break;
                    case "readonly":
                        if (value == "true")
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                        break;

                    case "authmethod":
                        if (authmethodList.Contains(value))
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                        break;
                    case "locatortype":
                        if (locatortypeList.Contains(value))
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                        break;
                    case "failovermode":
                        if (failovermodeList.Contains(value))
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                        break;
                    case "customizedmode":
                        if (customizedmodeList.Contains(value))
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                        break;
                    default:
                        result = false;
                        break;
                }
                if (result == false)
                {
                    return result;
                }
            }
            return result;
        }

        private int ValidateStorageInfo(StorageDeviceUIDto dto)
        {
            int status = (int)CreateOrEditStatus.Success;
            int vimType;
            switch (dto.mCurrentXRI.VIM)
            {
                case "amazon_vim":
                    vimType = (int)GCommon.Contract.Storage.Entity.StorageDeviceType.CloudAmazon;
                    break;
                case "s3compatible_vim":
                    vimType = (int)GCommon.Contract.Storage.Entity.StorageDeviceType.S3Compatible;
                    break;
                case "box_vim":
                    vimType = (int)GCommon.Contract.Storage.Entity.StorageDeviceType.Box;
                    break;
                case "dropbox_vim":
                    vimType = (int)GCommon.Contract.Storage.Entity.StorageDeviceType.Dropbox;
                    break;
                case "ftp_vim":
                    vimType = (int)GCommon.Contract.Storage.Entity.StorageDeviceType.FTP;
                    break;
                case "netapp_alta_vault_vim":
                    vimType = (int)GCommon.Contract.Storage.Entity.StorageDeviceType.NetApp_Alta_Vault;
                    break;
                case "rackspace_vim":
                    vimType = (int)GCommon.Contract.Storage.Entity.StorageDeviceType.CloudRackspace;
                    break;
                case "sftp_vim":
                    vimType = (int)GCommon.Contract.Storage.Entity.StorageDeviceType.SFTP;
                    break;
                case "azure_vim":
                    vimType = (int)GCommon.Contract.Storage.Entity.StorageDeviceType.CloudAzure;
                    break;
                case "google_vim":
                    vimType = (int)GCommon.Contract.Storage.Entity.StorageDeviceType.Google;
                    break;
                default:
                    vimType = (int)GCommon.Contract.Storage.Entity.StorageDeviceType.None;
                    break;
            }
            if (dto.Type != vimType)
            {
                status = (int)RAFailedType.ParameterIsIncorrect;
                return status;
            }
            var archiveRetentionRuleIndex = -1;
            foreach (var rtRule in dto.ArchiveRetentionRules)
            {
                archiveRetentionRuleIndex++;
                if (rtRule.SetupDataRetention)
                {
                    if (dto.IsSystemStorage && archiveRetentionRuleIndex == 0 && !StorageDeviceService.IsDisableRetentionPeriodLimitation())
                    {
                        if (rtRule.ArchiveDateUnit == DateUnit.Day)
                        {
                            if (rtRule.KeepValue < 91 && rtRule.RetentionDataTimeType != KeepDateType.ModifiedTime)
                            {
                                return (int)RAFailedType.ParameterIsIncorrect;
                            }
                        }
                        else if (rtRule.ArchiveDateUnit == DateUnit.Week)
                        {
                            if (rtRule.KeepValue < 13 && rtRule.RetentionDataTimeType != KeepDateType.ModifiedTime)
                            {
                                return (int)RAFailedType.ParameterIsIncorrect;
                            }
                        }
                        else if (rtRule.ArchiveDateUnit == DateUnit.Month)
                        {
                            if (rtRule.KeepValue < 4 && rtRule.RetentionDataTimeType != KeepDateType.ModifiedTime)
                            {
                                return (int)RAFailedType.ParameterIsIncorrect;
                            }
                        }
                    }
                    if ((rtRule.KeepValue > 0 && rtRule.KeepValue <= int.MaxValue) && (rtRule.DeleteTheData ^ rtRule.IsMove ^ rtRule.IsMarkDataTier))
                    {
                        status = (int)CreateOrEditStatus.Success;
                    }
                    else
                    {
                        status = (int)RAFailedType.ParameterIsIncorrect;
                        return status;
                    }
                    if (rtRule.IsMove)
                    {
                        if (StorageDeviceService.GetStorageDeviceById(rtRule.MoveDeviceId) == null)
                        {
                            status = (int)RAFailedType.ParameterIsIncorrect;
                            return status;
                        }
                    }
                    if (rtRule.IsSoftDelete && !IsEnableSoftDeleteSetting())
                    {
                        status = (int)RAFailedType.ParameterIsIncorrect;
                        return status;
                    }
                }
            }
            if (dto.IsSystemStorage)
            {
                if (string.IsNullOrEmpty(dto.Name))
                {
                    status = (int)RAFailedType.ParameterIsIncorrect;
                    return status;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(dto.Name) || (bool.Parse(dto.mCurrentXRI.Params["advanced"]) && string.IsNullOrEmpty(dto.mCurrentXRI.Params["extendedparameters"])))
                {
                    status = (int)RAFailedType.ParameterIsIncorrect;
                    return status;
                }
            }
            return status;
        }

        [HttpGet]
        public async Task<SOSummaryTotalDataDetails> GetSummaryArchiverSiteTotalInfo(string o365TenantId, string siteId)
        {
            return await _DashboardService.GetSOTotalDataInfos(o365TenantId, siteId);
        }

        [HttpGet]
        public async Task<SOSummaryTotalDataDetails> GetSummaryArchiverTenantTotalInfo(string o365TenantId)
        {
            return await _DashboardService.GetSOTotalDataInfosByTenant(o365TenantId);
        }

        [HttpGet]
        public async Task<SOSummaryTotalDataDetails> GetSummaryArchiverTotalDetails()
        {
            try
            {
                return await SODashboardQuerier.GetSOTotalDataDetailsAsync();
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get archived data size, Error: {e}");
            }
            return new SOSummaryTotalDataDetails();
        }

        private bool IsEnableSoftDeleteSetting()
        {
            var key = RMKeyValueDao.GetValueByKey("EnableSoftDelete");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }
    }
}