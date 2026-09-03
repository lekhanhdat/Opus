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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Common.Utils.ProtoBuf;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.SharePoint.Discover;
using RAFileSystem.FileSystem.FileSystem.Backup.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.Common
{
    public class ArchiverCommonStaticMethod
    {
    }
    public static class ArchiverTypeConvert
    {
        public static LogicalDeviceDto ConvertStorageDeviceDtoToLogicalDeviceDto(AvePoint.GCommon.Contract.Storage.Entity.StorageDeviceDto storageDevice)
        {
            var physical = new PhysicalDeviceDto()
            {
                Id = storageDevice.Id,
                ConnectionString = storageDevice.ConnectionString,
                ModifyTime = storageDevice.ModifyTime,
                Type = storageDevice.Type,
                IsSystemStorage = storageDevice.Id == RecordsConstants.AVEPOINT_DEFAULT_STORAGEID || storageDevice.IsSystemStorage
            };

            var logical = new LogicalDeviceDto();
            logical.Name = storageDevice.Name;
            logical.Id = storageDevice.Id;
            logical.PhysicalDrives = new List<PhysicalDeviceDto>
            {
                physical
            };
            return logical;
        }
    }
    public static class ClassCodeCommonStaticMethod
    {
        private static AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static void GenerateRetentionTimeCacheKeyAndSetEndTime(FileSystemRecordDto record, ClassCodeInfoDto dto)
        {
            var key = GenerateRetentionTimeCacheKey(dto.CountryCode, dto.RetentionType, dto.TermId.ToString());
            logger.Info($"GenerateRetentionTimeCacheKeyAndSetEndTime get retention unit key:{key}");
            lock (FSJobCache.Instance.RuleUnitClassCodeCacheLock)
            {

                if (!FSJobCache.Instance.RuleUnitClassCodeCache.ContainsKey(key))
                {
                    var unitString = HybridApiClient.Instance.GetRetentionUnit(dto.CountryCode, dto.RetentionType, dto.TermId.ToString());
                    if (!string.IsNullOrEmpty(unitString))
                    {
                        var unitDto = SerializerHelper.DeserializeByDataContractSerializer<OlderThanTimeDtoForAgent>(unitString);
                        logger.Info($"GenerateRetentionTimeCacheKeyAndSetEndTime get retention unit from api success,CountryCode:{dto?.CountryCode},RetentionType:{dto?.RetentionType},TermId:{dto?.TermId},unitDto.policyNumber:{unitDto?.Number},unitDto.policyType:{unitDto?.PolicyValueUnit}");
                        FSJobCache.Instance.RuleUnitClassCodeCache.Add(key, unitDto);
                    }
                    else
                    {
                        FSJobCache.Instance.RuleUnitClassCodeCache.Add(key, null);
                        logger.Warn($"GenerateRetentionTimeCacheKeyAndSetEndTime get retention unit from api failed,CountryCode:{dto?.CountryCode},RetentionType:{dto?.RetentionType},TermId:{dto?.TermId}");
                    }
                }
            }
            if (FSJobCache.Instance.RuleUnitClassCodeCache.ContainsKey(key))
            {
                OlderThanTimeDtoForAgent tempDto = FSJobCache.Instance.RuleUnitClassCodeCache[key];
                if (tempDto != null)
                {
                    logger.Info($"GenerateRetentionTimeCacheKeyAndSetEndTime get retention unit from cache success,CountryCode:{dto?.CountryCode},RetentionType:{dto?.RetentionType},TermId:{dto?.TermId},tempDto.policyNumber:{tempDto?.Number},tempDto.policyType:{tempDto?.PolicyValueUnit}");
                    record.EndTime = dto.RetentionType == (int)RetentionScheduleType.Event ? FileSystemContractHelper.CalculateEndTime(dto.StartDate, tempDto.PolicyValueUnit, tempDto.Number) : FileSystemContractHelper.CalculateEndTime(record.TimeLastModified, tempDto.PolicyValueUnit, tempDto.Number);
                    record.PolicyValueNumber = tempDto.Number;
                    record.PolicyValueUnit = tempDto.PolicyValueUnit;
                }
                else
                {
                    record.EndTime = 0;//rule change or file change caused not has fit endtime
                }
            }
            else
            {
                record.EndTime = 0;//rule change or file change caused not has fit endtime
            }
        }
        private static string GenerateRetentionTimeCacheKey(string countryCode, int retentionType, string termId)
        {
            return $"{countryCode}_{retentionType}_{termId}";
        }

        public static bool IsRuleModified(Guid termId, long collectionTime)
        {
            List<Rule> rules;
            bool ruleHasModifed = false;
            if (termId != Guid.Empty && FSJobCache.Instance.TermRuleMapping.TryGetValue(termId, out rules))
            {
                foreach (var rule in rules)
                {
                    if (collectionTime < rule.ModifyTime)
                    {
                        ruleHasModifed = true;
                        break;
                    }
                }
            }
            return ruleHasModifed;
        }
    }
}
