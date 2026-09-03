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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Encryption;
using AvePoint.RA.Service.Services.Extensions;
using Cloud.sdk.Data.Opus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AvePoint.GCommon.Utility.I18N.ContextValues.Configuration;

namespace AvePoint.RA.Service.Services.Common
{
    public class KeyValueService : RMServiceBase, IKeyValueService
    {
        private RALogger logger = RALogger.GetInstance(typeof(KeyValueService));
        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private RMAesEncryptorWrapper AesEncryptorWrapper => new();
        private static string echoPassword = "******";

        public bool HasUpgradeVEOV3()
        {
            var result = false;
            var key = KeyNameCollection.HasUpgradeVEOV3;
            var setting = KeyValueDao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }
        public bool IsSCBlackListForEdiscovery()
        {
            try
            {
                var entity = KeyValueDao.GetValueByKey(KeyNameCollection.IsSCBlackListForEdiscovery);
                if (entity != null && bool.TryParse(entity.Value, out bool value))
                {
                    return value;
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get IsSCBlackListForEdiscovery, error : {e.ToString()}");
            }
            return false;
        }

        public bool ForceFilterSiteCollectionInMemory()
        {
            try
            {
                var entity = KeyValueDao.GetValueByKey("forceFilterSiteCollectionInMemory");
                if (entity != null && bool.TryParse(entity.Value, out bool value))
                {
                    return value;
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get forceFilterSiteCollectionInMemory, error : {e.ToString()}");
            }
            return false;
        }

        public int ForceFilterInMemoryPageSize()
        {
            try
            {
                var entity = KeyValueDao.GetValueByKey("ForceFilterInMemoryPageSize");
                if (entity != null && int.TryParse(entity.Value, out int value))
                {
                    return value;
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get ForceFilterInMemoryPageSize, error : {e.ToString()}");
            }
            return 500;
        }

        public async Task<TenantGlobalSetting> GetAllAsync()
        {
            try
            {
                var entities = await KeyValueDao.GetAllAsync();
                var dic = new Dictionary<string, object>();
                foreach(var entity in entities)
                {
                    if(bool.TryParse(entity.Value, out var boolValue))
                    {
                        dic[entity.Key] = boolValue;
                        continue;
                    }
                    else if(long.TryParse(entity.Value, out var longValue))
                    {
                        dic[entity.Key] = longValue;
                        continue;
                    }
                    else if(Guid.TryParse(entity.Value, out var guidValue))
                    {
                        dic[entity.Key] = guidValue;
                        continue;
                    }
                    try
                    {
                        if (entity.Key == "ArchiverExtendSetting")
                        {
                            var tempSetting= JsonConvert.DeserializeObject<ArchiverExtendSettingDto>(entity.Value);
                            string tempPassword = GetPasswordFromConnetionString(tempSetting.CGDatabaseConnection);
                            tempSetting.CGDatabaseConnection=tempSetting.CGDatabaseConnection.Replace(tempPassword, echoPassword);
                            dic[entity.Key]=tempSetting;
                        }
                        else
                        {
                            dic[entity.Key] = JsonConvert.DeserializeObject<object>(entity.Value);
                        }
                    }
                    catch
                    {
                        dic[entity.Key] = entity.Value;
                    }
                }

                var json = JsonConvert.SerializeObject(dic);
                return JsonConvert.DeserializeObject<TenantGlobalSetting>(json);
            }
            catch(Exception e)
            {
                logger.Error($"An error occurred while get all async. Error: {e}");
                return null;
            }
        }

        public int GetConvertFolderItemToDBLimitCount()
        {
            try
            {
                var entity = KeyValueDao.Find(o => o.Key.Equals("ConvertFolderItemToDBLimitCount"));
                var convertFolderItemToDBLimit = entity?.Conver2Dto();
                if (convertFolderItemToDBLimit != null && int.TryParse(convertFolderItemToDBLimit.Value, out int value) && value > 0)
                {
                    logger.Info($"Set AdaptiveSpoItemStorage.MEMORY_TEIM_CACHE_LIMIE_COUNT to {value} from convertFolderItemToDBLimit");
                    return value;
                }
            }
            catch (Exception e)
            {
                logger.Warn($"Fail check key of convertFolderItemToDBLimit,ex:{e}");
            }
            return 2500000; //250w, can be set from outside for test
        }

        public async Task<bool> UpdateAsync(TenantGlobalSetting tenantGlobalSetting)
        {
            try
            {
                var existTenantGlobalSettings = await GetAllAsync();
                EncryptDBPassword(tenantGlobalSetting);
                EncryptDBPassword(existTenantGlobalSettings);
                var json = JsonConvert.SerializeObject(tenantGlobalSetting);
                var dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                var entities = dic.ToDictionary(item => item.Key, item => JsonConvert.SerializeObject(item.Value));
                var existJson = JsonConvert.SerializeObject(existTenantGlobalSettings);
                var existDic = JsonConvert.DeserializeObject<Dictionary<string, object>>(existJson);
                var existEntities = existDic.ToDictionary(item => item.Key, item => JsonConvert.SerializeObject(item.Value));
                var deleteEntityKeys = existEntities.Keys.Where(key => !entities.ContainsKey(key));
                logger.Info($"Delete tenant setting key [{string.Join(", ", deleteEntityKeys)}]");
                return await KeyValueDao.UpdateAsync(entities, deleteEntityKeys);
            }
            catch(Exception e)
            {
                logger.Error($"An error occurred while update async. Error: {e}");
                return false;
            }

        }
        private void EncryptDBPassword(TenantGlobalSetting tenantGlobalSetting)
        {
            if (tenantGlobalSetting.ArchiverExtendSetting != null && !string.IsNullOrEmpty(tenantGlobalSetting.ArchiverExtendSetting.CGDatabaseConnection))
            {
                var tempSetting = KeyValueDao.GetValueByKey("ArchiverExtendSetting");
                if (tempSetting != null)
                {
                    string tempPassword= GetPasswordFromConnetionString(tenantGlobalSetting.ArchiverExtendSetting.CGDatabaseConnection);
                    if (tempPassword == echoPassword)
                    {
                        logger.Info("not change password,no need encrypt");
                        var extendSettingDto= JsonConvert.DeserializeObject<ArchiverExtendSettingDto>(tempSetting.Value);
                        string oldPassword = GetPasswordFromConnetionString(extendSettingDto.CGDatabaseConnection);
                        tenantGlobalSetting.ArchiverExtendSetting.CGDatabaseConnection = tenantGlobalSetting.ArchiverExtendSetting.CGDatabaseConnection.Replace(tempPassword, oldPassword);
                    }
                    else
                    {
                        logger.Info("is changging password,encrypt it");
                        string notEncryptPassword = GetPasswordFromConnetionString(tenantGlobalSetting.ArchiverExtendSetting.CGDatabaseConnection);
                        string encryptPassword = AesEncryptorWrapper.Encrypt(notEncryptPassword);
                        tenantGlobalSetting.ArchiverExtendSetting.CGDatabaseConnection = tenantGlobalSetting.ArchiverExtendSetting.CGDatabaseConnection.Replace(notEncryptPassword, encryptPassword);
                    }
                }
                else
                {
                    logger.Info("ArchiverExtendSetting is null,encrypt db password");
                    string notEncryptPassword = GetPasswordFromConnetionString(tenantGlobalSetting.ArchiverExtendSetting.CGDatabaseConnection);
                    string encryptPassword = AesEncryptorWrapper.Encrypt(notEncryptPassword);
                    tenantGlobalSetting.ArchiverExtendSetting.CGDatabaseConnection=tenantGlobalSetting.ArchiverExtendSetting.CGDatabaseConnection.Replace(notEncryptPassword,encryptPassword);
                }
            }
        }
        private string GetPasswordFromConnetionString(string connectionString)
        {
            var tempDatabaseConnectionSplit = connectionString.Split(';');
            string passwordString = "Password=";
            foreach (string splitString in tempDatabaseConnectionSplit)
            {
                if (splitString.StartsWith(passwordString,StringComparison.OrdinalIgnoreCase))
                {
                    return splitString.Substring(passwordString.Length);
                }
            }
            return string.Empty;
        }
        public RMNameValueDto Get(string key)
        {
            try
            {
                var entity = KeyValueDao.Find(o => o.Key.Equals(key));
                return entity != null ? entity.Conver2Dto() : null;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get Key value, key: {key}, error : {e.ToString()}");
            }

            return null;
        }


        public long GetOOPRestoreJobZipSizeLimit()
        {
            try
            {
                string value = KeyValueDao.GetValueByKey("OOPRestoreJobZipSizeLimit")?.Value;
                if(long.TryParse(value, out long result))
                {
                    return result;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception e)
            {
                logger.Error($@"fail get oop restore job zip size limt,ex:{e}");
                return -1;
            }
        }

        public RMNameValueDto Get(string name, RMNameValueType type)
        {
            var key = $"{name}{RMNameValueDto.Seprator}{type}";
            return Get(key);
        }

        public async Task<bool> SaveAsync(RMNameValueDto dto)
        {
            try
            {
                var entity = dto.Conver2Entity();
                return await KeyValueDao.SaveOrUpdateAsync(entity);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while save key and value, key : {dto.Name}{RMNameValueDto.Seprator}{dto.Type}. error: {e.ToString()}");
            }

            return false;
        }

        public bool Delete(string key)
        {
            try
            {
                return KeyValueDao.DeleteByKey(key);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while delete key-value, key : {key}. error: {e.ToString()}");
            }
            return false;
        }
        public bool IsEnableSoftDeleteSetting()
        {
            var key = KeyValueDao.GetValueByKey("EnableSoftDelete");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }

        public bool IsEnableCGScan()
        {
            try
            {
                RMKeyValue setting = KeyValueDao.GetValueByKey("ArchiverExtendSetting");
                ArchiverExtendSettingDto archiverExtendSetting = setting == null ? null : JsonConvert.DeserializeObject<ArchiverExtendSettingDto>(setting.Value);
                return archiverExtendSetting?.IsCGDiscovery == true;
            }
            catch(Exception e)
            {
                logger.Error($"Fail get is enable cg scan,ex:{e}");
                throw;
            } 
        }

        public ArchiveJobSplitLimit GetArchiveJobSplitLimit()
        {
            try
            {
                RMKeyValue setting = KeyValueDao.GetValueByKey("ArchiveJobSplitLimit");
                ArchiveJobSplitLimit archiveJobSplitLimit = setting == null ? null : JsonConvert.DeserializeObject<ArchiveJobSplitLimit>(setting.Value);
                return archiveJobSplitLimit;
            }
            catch (Exception e)
            {
                logger.Error($"Fail GetArchiveJobSplitLimit,ex:{e}");
                throw;
            }
        }
    }
}
