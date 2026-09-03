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
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Web.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.Media.StorageApi;
using Storage;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.Office2010.Excel;
using Cloud.Sdk.Data.Dao;
using StorageDeviceType = AvePoint.GCommon.Contract.Storage.Entity.StorageDeviceType;
using AvePoint.RA.DB.Dao;
using AvePoint.Common.Portal;
using AvePoint.RA.Service.SharePointSetting;
using System.Text.RegularExpressions;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Web.Common.WIF;
using System.Threading.Tasks;
using AvePoint.RA.RADataBroker;
using System.Linq;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using DocumentFormat.OpenXml.Wordprocessing;
using Aspose.Pdf.Operators;
using Storage.Util;
using System.Net.Http;
using AvePoint.GCommon.Utility;
using System.Net;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.RACommonUtility.Common;

namespace AvePoint.RA.Web.Controllers.PhysicalDevice
{
    [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser, RMSOPermissionMasks.RuleManagementEnduser, preferred: false)]
    public class StorageDeviceController : BaseApiController
    {

        private IStorageDeviceService _StorageDeviceService;
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService(ref _StorageDeviceService);
        
        private IRMArchiverSettingsService _RMArchiverSettingsService;
        private IRMArchiverSettingsService RMArchiverSettingsService => PlatformWindsorManager.GetService(ref _RMArchiverSettingsService);
        private ITenantService _TenantService;
        private ITenantService TenantService => PlatformWindsorManager.GetService(ref _TenantService);

        private IGeneralSettingService _GeneralSettingService;
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService(ref _GeneralSettingService);
        private IRMKeyValueDao _RMKeyValueDao;
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService(ref _RMKeyValueDao);
        private IExportSettingService _ExportSettingService;
        private IExportSettingService ExportSettingService => PlatformWindsorManager.GetService(ref _ExportSettingService);


        [HttpGet]
        [RMApiAuthorize(RMSOPermissionMasks.RuleManagementAdmin, RMPermissionExtensionMasks.GoogleAdmin )]
        public async Task<RAReturnMessage> CheckAzureRegion(string accessPoint, string accountName, string storageDeviceId)
        {
            return await StorageDeviceService.CheckAzureRegion(accessPoint, accountName, storageDeviceId);
        }


        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.RuleManagementAdmin, RMPermissionExtensionMasks.GoogleAdmin )]
        public async Task<RAReturnMessage> CreateOrEditStorageDevice([FromBody] StorageDeviceUIDto dto)
        {
            if (ValidateStorageInfo(dto) != (int)CreateOrEditStatus.Success)
            {
                var msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = "ParameterIsIncorrect" };
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
                    var msg = new RAReturnMessage()
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = I18NEntity.GetString("RM_AR_Storage_Account_ErrorMessage")
                    };
                    return msg;
                }
                if (!ValidateAzureContainerName(dto.mCurrentXRI.Params["containername"]))
                {
                    var msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_AR_Storage_ContainerName_ErrorMessage") };
                    return msg;
                }
            }
            if (!dto.IsSystemStorage && dto.mCurrentXRI.Params["advanced"] == "true")
            {
                var paramString = dto.mCurrentXRI.Params["extendedparameters"];
                List<string> tempParaList = paramString.Split("\n").ToList();
                if(dto.Type == (int)StorageDeviceType.Google)
                {
                    if (!ValidateGoogleAdvanceExtendedParameters(tempParaList, dto.mCurrentXRI.Params["containername"]))
                    {
                        var msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_AR_Storage_ExtendedParameters_ErrorMessage") };
                        return msg;
                    }
                }
                else
                {
                    if (!ValidateAdvanceExtendedparameters(tempParaList))
                    {
                        var msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_AR_Storage_ExtendedParameters_ErrorMessage") };
                        return msg;
                    }
                }
            }
            StorageDeviceDto mDto = ConvertUIDto2PhysicalDeviceDto(dto);
            return await StorageDeviceService.ValidateAndCreateStorageDeviceAsync(mDto, EntityObjectPermissionType.FullPermission);
        }
        [HttpPost]
        public async Task<StorageDeviceResult> GetAllActiveStorage([FromBody] StorageDeviceResult sdr)
        {
            if (sdr.StorageDeviceUIDtosList == null)
            {
                sdr.StorageDeviceUIDtosList = new List<StorageDeviceUIDto>();
            }
            StorageDeviceResult result = new StorageDeviceResult();
            result = await StorageDeviceService.GetAllStorageDeviceByIsOldRecordAsync(RMConstants.STORAGE_NEW_DATA_TYPE, sdr);
            //if (dtos != null)
            //{
            //    foreach (var d in dtos)
            //    {
            //        result.StorageDeviceUIDtosList.Add(StorageDeviceConvert.ConvertStorageDeviceDtoToUIDto(d));
            //    }
            //}
            return result;
        }
        
        [HttpGet]
        public async Task<StorageInfoExportSetting> GetSftpAndAzureStorageInfos()
        {
            return await ExportSettingService.GetStorageInfoInExportSettingsAsync();
        }
        [HttpGet]
        public async Task<StorageInfoExportSetting> GetGoogleStoragLocationInfos()
        {
            return await ExportSettingService.GetGoogleStorageInfoInExportSettingsAsync();
        }

        [HttpGet]
        [RMApiAuthorize(RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterExport, RMPermissionExtensionMasks.GoogleAdmin, PermissionJoinType.Any)]
        public async Task<List<StorageDeviceUIDto>> GetAllActiveStorage()
        {
            List<StorageDeviceUIDto> result = new List<StorageDeviceUIDto>();
            result = await StorageDeviceService.GetAllStorageDeviceNotPagedAsync();
            return result;
        }
        [HttpPost]
        public async Task<StorageDeviceUIDto> GetStorageDeviceById([FromBody] string Id)
        {
            RAReturnMessage result = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            await StorageDeviceService.CheckCanDeleteStorageAsync(new List<string>() { Id }, result);
            var dto = StorageDeviceService.GetStorageDeviceById(Id, true);
            if (result.MessageType == RAMessageType.Failed)
            {
                dto.IsUsingDevice = true;
            }
            return StorageDeviceConvert.ConvertStorageDeviceDtoToUIDto(dto);
        }
        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.RuleManagementAdmin, RMPermissionExtensionMasks.GoogleAdmin )] //RMPermissionMasks.RuleManagementAdmin, 
        public Task<RAReturnMessage> DeleteStorageDevices([FromBody] List<string> Ids)
        {
            return StorageDeviceService.DeleteStorageDevicesAsync(Ids);
        }
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementAdmin, RMSOPermissionMasks.RuleManagementAdmin)]//RMPermissionMasks.RuleManagementAdmin,
        public async Task<RAReturnMessage> SetIndexDevice([FromBody] string Id)
        {
            RAReturnMessage result = new RAReturnMessage() { MessageType = RAMessageType.Successful, ErrorMessage = string.Empty };
            var storageDto = StorageDeviceService.GetStorageDeviceById(Id);
            if (storageDto != null)
            {
                var currentIndex = StorageDeviceService.GetIndexDevice();
                if (currentIndex != null && TenantService.IsNewOpusTenant())
                {   
                    var jobReturnMessage = RMArchiverSettingsService.RunArchiverMoveIndexJob(JobRunBy.Control, TenantLocalValue.LogonUserEmail, currentIndex.Id, Id);
                    result.Extension = jobReturnMessage.Extension;
                }
                else
                {
                    result = await StorageDeviceService.SetUsingDeviceByIdAsync(storageDto.Id, SettingProfilesType.IndexDevice, storageDto.Name);
                }
            }
            else
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = "The storage device is not exist";
            }
            return result;
        }

        [HttpPost]
        public Task<DevicesResult> GetStorageDevices([FromBody] bool isFilter)
        {   
            return StorageDeviceService.GetStorageIdAndNameAsync(isFilter); 
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser, RMSOPermissionMasks.RuleManagementEnduser, preferred: false)]
        public async Task<SecurityProfileResult> GetEncryptionProfileNames()
        {
            return await GeneralSettingService.SaveUsingSecurityProfileAsync();
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ContentRepositoyAdmin, RMSOPermissionMasks.ContentRepositoyAdmin)]
        public async Task<RAReturnMessage> RunExportIndexJob()
        {
            try
            {
                return await RMArchiverSettingsService.RunExportIndexJob();
            }
            catch (Exception e)
            {
                Logger.Error("Failed to run job. Error:{1}", e.ToString());
                throw;
            }
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ContentRepositoyAdmin, RMSOPermissionMasks.ContentRepositoyAdmin)]
        public void CopyIndexPassword()
        {
            RMArchiverSettingsService.CopyPasswordAudit();
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
                    builder.Params.Add(dic.Key, tempDto.mCurrentXRI.Params.Where(a => a.Key == dic.Key).Select(a=>a.Value).FirstOrDefault());
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
            bool result= false;
            List<string> boolList= new List<string>() { "true","false"};
            List<string> authmethodList = new List<string>() { "netuse", "logonuser", "netuse_deleteold" };
            List<string> locatortypeList = new List<string>() { "static", "proxy"};
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
                    vimType = (int)StorageDeviceType.CloudAmazon;
                    break;
                case "s3compatible_vim":
                    vimType = (int)StorageDeviceType.S3Compatible;
                    break;
                case "wasabi_vim":
                    vimType = (int)StorageDeviceType.Wasabi;
                    break;
                case "box_vim":
                    vimType = (int)StorageDeviceType.Box;
                    break;
                case "dropbox_vim":
                    vimType = (int)StorageDeviceType.Dropbox;
                    break;
                case "ftp_vim":
                    vimType = (int)StorageDeviceType.FTP;
                    break;
                case "netapp_alta_vault_vim":
                    vimType = (int)StorageDeviceType.NetApp_Alta_Vault;
                    break;
                case "rackspace_vim":
                    vimType = (int)StorageDeviceType.CloudRackspace;
                    break;
                case "sftp_vim":
                    vimType = (int)StorageDeviceType.SFTP;
                    break;
                case "azure_vim":
                    vimType = (int)StorageDeviceType.CloudAzure;
                    break;
                case "google_vim":
                    vimType = (int)StorageDeviceType.Google;
                    break;
                default:
                    vimType = (int)StorageDeviceType.None;
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
                            if (rtRule.KeepValue < 91 && rtRule.RetentionDataTimeType!=KeepDateType.ModifiedTime)
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
                if(vimType == (int)StorageDeviceType.CloudAmazon && dto.mCurrentXRI.Params["region"] == "customized" 
                    && (!bool.Parse(dto.mCurrentXRI.Params["advanced"]) || string.IsNullOrEmpty(dto.mCurrentXRI.Params["extendedparameters"]) || !dto.mCurrentXRI.Params["extendedparameters"].Contains("CustomizedRegion=", StringComparison.OrdinalIgnoreCase)))
                {
                    return (int)RAFailedType.ParameterIsIncorrect;
                }

                if (string.IsNullOrEmpty(dto.Name) || (bool.Parse(dto.mCurrentXRI.Params["advanced"]) && string.IsNullOrEmpty(dto.mCurrentXRI.Params["extendedparameters"])))
                {
                    return (int)RAFailedType.ParameterIsIncorrect;
                }
            }
            return status;
        }
        private bool IsEnableSoftDeleteSetting()
        {
            var key = RMKeyValueDao.GetValueByKey("EnableSoftDelete");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }
    }
}
