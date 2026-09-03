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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.RACommonUtility.GlobalLocker;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.UniqueId
{
    public class UniqueIdGenerator
    {

        protected static readonly RALogger logger = RALogger.GetInstance(typeof(UniqueIdGenerator));

        private static readonly IRMLockDao RecordLock = PlatformWindsorManager.GetService<IRMLockDao>();
        /// <summary>
        /// Generate UniqueId for Electric
        /// </summary>
        /// <param name="setting">Unique id setting which configed on Page</param>
        /// <returns></returns>
        public static async Task<string> GenerateUniqueIdAsync(RMUniqueIdSetting setting)
        {
            var recordsGlobalId = await FormateCurrentIdAsync(setting);
            return recordsGlobalId;
        }

        /// <summary>
        /// Generate UniqueId for Physical
        /// </summary>
        /// <param name="templateId">Template id for the physical object</param>
        /// <param name="prefix">prefix for the unique id setting in template</param>
        /// <param name="digit">digit setting in template</param>
        /// <returns></returns>
        public static async Task<string> GenerateUniqueIdAsync(string templateId, string prefix, int digit)
        {
            var result = string.Empty;
            try
            {
                string currentFormat = "{0}-{1}";
                string lockerKey = TenantLocalValue.LogonGroupId;
                result = string.Format(currentFormat, prefix, FormatNumber(await RMGlobalLocker.GetIdAsync(lockerKey + "|" + templateId), digit, true));
            }
            catch (Exception e)
            {
                logger.Info("Failed to formate currentId : " + e.ToString());
                throw;
            }
            return result;
        }

        public static async Task<string> GenerateCustomUniqueIdAsync(List<string> templateIds, string currentTemplateId,string prefix, int digit)
        {
            var result = string.Empty;
            try
            {
                string currentFormat = "{0}-{1}";
                string lockerKey = TenantLocalValue.LogonGroupId;
                long recordId = 0;
                var customGlobalLockerKey = "GLOBAL_CUSTOM_TEMPLATE_LOCKER";
                var customGlobalLocker = RecordLock.GetLockerRecord(lockerKey + "|" + customGlobalLockerKey);
                if(customGlobalLocker == null)
                {
                    templateIds = templateIds.ConvertAll(templateId =>
                    {
                        templateId = lockerKey + "|" + templateId;
                        return templateId;
                    });
                    var maxLockRecord = RecordLock.GetMaxLockerRecord(templateIds);
                    if (maxLockRecord != null)
                    {
                        recordId = maxLockRecord.RecordId;
                    }
                    recordId = await RMGlobalLocker.GetAndSetIdAsync(lockerKey + "|" + customGlobalLockerKey, recordId + 1);
                    logger.Info($"Add global custom locker to db, record id is : {recordId}");
                }
                else
                {
                    recordId = await RMGlobalLocker.GetIdAsync(lockerKey + "|" + customGlobalLockerKey);
                }
            
                result = string.Format(currentFormat, prefix, FormatNumber(recordId, digit, true));
            }
            catch (Exception e)
            {
                logger.Error("Failed to formate currentId : " + e.ToString());
                throw;
            }
            return result;
        }

        private static async Task<string> FormateCurrentIdAsync(RMUniqueIdSetting setting)
        {
            var result = string.Empty;
            try
            {
                string templateId = string.Empty;//Electric unique id do not have templateid, so we use templatedId = string.Empty
                string currentFormat = "{0}-{1}";
                string lockerKey = TenantLocalValue.LogonGroupId;
                if (setting != null)
                {
                    result = string.IsNullOrEmpty(setting.Prefix) ? FormatNumber(await RMGlobalLocker.GetIdAsync(lockerKey)) : string.Format(currentFormat, setting.Prefix, FormatNumber(await RMGlobalLocker.GetIdAsync(lockerKey)));
                }
                else
                {
                    result = string.Format(currentFormat, UniqueIdConfig.DefaultPrefix, FormatNumber(await RMGlobalLocker.GetIdAsync(lockerKey)));
                }
            }
            catch (Exception e)
            {
                logger.Info("Failed to formate currentId : " + e.ToString());
                throw;
            }
            return result;
        }

        /// <summary>
        /// 如果number 小于digit 位数，则补齐； 如果大于，则需要判断是否要throw， 不throw 直接返回
        /// </summary>
        /// <param name="number">需要Format 的值</param>
        /// <param name="digit">设置的默认的位数， default = 10位</param>
        /// <param name="throwIfOverLength">如果超过位数的长度，是否抛出异常，默认不抛</param>
        /// <returns></returns>
        private static string FormatNumber(long number, int digit = 10, bool throwIfOverLength = false)
        {
            var result = string.Empty;
            try
            {
                if (number < (Math.Pow(10, digit)))
                {
                    result = number.ToString().PadLeft(digit, '0');
                }
                else if (throwIfOverLength)
                {
                    throw new Exception("Over the digit number");
                }
                else
                {
                    result = number.ToString();
                }
            }
            catch (Exception e)
            {
                logger.Info(string.Format("Failed to formate number {0} : {1}", number, e.ToString()));
                throw;
            }
            return result;
        }
    }
}
