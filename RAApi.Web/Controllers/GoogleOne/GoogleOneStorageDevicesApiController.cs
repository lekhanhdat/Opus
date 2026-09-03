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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.ControlPanel;
using AvePoint.RA.Service.Services.StorageDevice;
using Cloud.sdk.Data.Opus.GoogleOne.Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne
{
    [Route("api/googleone/storagedevices")]
    public class GoogleOneStorageDevicesApiController : GoogleOneApiBaseController
    {
        private IExportSettingService ExportSettingService => PlatformWindsorManager.GetService<IExportSettingService>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        [HttpGet("google/locations")]
        public async Task<StorageInfoExportSetting> GetStorageLocations()
        {
            return await ExportSettingService.GetGoogleStorageInfoInExportSettingsAsync();
        }

        [HttpPost("save")]
        public async Task<string> CreateOrEditStorageDevice([FromBody] StorageDeviceUIDto dto)
        {
            if (ValidateStorageInfo(dto) != (int)CreateOrEditStatus.Success)
            {
                return I18NEntity.GetString("RM_JS_ArchiverMigration_Storage_Unsupported");
            }
            if (dto.UseCompression == false || dto.CompressionSpeed != 5)
            {
                dto.UseCompression = true;
                dto.CompressionSpeed = 5;
            }
            if (dto.mCurrentXRI.VIM == "azure_vim")
            {
                if (!ValidateAzureContainerName(dto.mCurrentXRI.Params["containername"]))
                {
                    return I18NEntity.GetString("RM_AR_Storage_ContainerName_ErrorMessage");
                }
            }
            if (!dto.IsSystemStorage && dto.mCurrentXRI.Params["advanced"] == "true")
            {
                var paramString = dto.mCurrentXRI.Params["extendedparameters"];
                List<string> tempParaList = paramString.Split("\n").ToList();
                if (dto.Type == (int)StorageDeviceType.Google)
                {
                    if (!ValidateGoogleAdvanceExtendedParameters(tempParaList, dto.mCurrentXRI.Params["containername"]))
                    {
                        return I18NEntity.GetString("RM_AR_Storage_ExtendedParameters_ErrorMessage");
                    }
                }
                else
                {
                    if (!ValidateAdvanceExtendedparameters(tempParaList))
                    {
                        return I18NEntity.GetString("RM_AR_Storage_ExtendedParameters_ErrorMessage");
                    }
                }
            }
            StorageDeviceDto mDto = ConvertUIDto2PhysicalDeviceDto(dto);
            try
            {
                GOReturnMessage goReturnMessage =  await StorageDeviceService.ValidateAndCreateStorageDeviceAsyncForGoogleOne(mDto, EntityObjectPermissionType.FullPermission);
                if (goReturnMessage.MessageType == RAMessageType.Failed)
                {
                    switch(goReturnMessage.ErrorMessage)
                    {
                        case var msg when msg == I18NEntity.GetString("RM_AR_Stub_Name_ErrorMessage"):
                            return I18NEntity.GetString("RM_AR_Stub_Name_ErrorMessage");
                        case var msg when msg == I18NEntity.GetString("RM_AR_Storage_ExtendedParameters_ErrorMessage"):
                            return I18NEntity.GetString("RM_AR_Storage_ExtendedParameters_ErrorMessage");
                        case var msg when msg == I18NEntity.GetString("RM_AR_Storage_Private_ID_Incorrect"):
                            return I18NEntity.GetString("RM_AR_Storage_Private_ID_Incorrect");
                        case var msg when msg == I18NEntity.GetString("RM_AR_Storage_Account_ErrorMessage"):
                            return I18NEntity.GetString("RM_AR_Storage_Account_ErrorMessage");
                        default:
                            return I18NEntity.GetString("RM_AR_Storage_Unknow_ErrorMessage");
                    }
                }
                   
                return JsonConvert.SerializeObject(goReturnMessage);
            }
            catch (Exception)
            {
                return I18NEntity.GetString("RM_AR_Storage_Unknow_ErrorMessage");
            }

        }
       


        [HttpPost("google/storages")]
        public async Task<string> GetAllActiveStorage([FromBody] StorageDeviceResult sdr)
        {
            if (sdr.StorageDeviceUIDtosList == null)
            {
                sdr.StorageDeviceUIDtosList = new List<StorageDeviceUIDto>();
            }
            StorageDeviceResult result = new StorageDeviceResult();
            result = await StorageDeviceService.GetAllStorageDeviceByIsOldRecordAsync(RMConstants.STORAGE_NEW_DATA_TYPE, sdr);
           
            return JsonConvert.SerializeObject(result);
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
        private bool IsEnableSoftDeleteSetting()
        {
            var key = RMKeyValueDao.GetValueByKey("EnableSoftDelete");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }
    }

}
