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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.GlobalLocker;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.UniqueId
{
    public class UniqueIdUtil
    {
        private RALogger logger = RALogger.GetInstance(typeof(UniqueIdUtil));
        private IUniqueIdSettingDao mUniqueIdSettingDao { get; set; }
        public IUniqueIdSettingDao UniqueIdSettingDao
        {
            get
            {
                if (mUniqueIdSettingDao == null)
                {
                    mUniqueIdSettingDao = (IUniqueIdSettingDao)PlatformWindsorManager.GetService(typeof(IUniqueIdSettingDao));
                }
                return mUniqueIdSettingDao;
            }
        }

        private RMUniqueIdSetting mRMUniqueIdSetting;
        private List<long> mUniqueIdList;
        private int mIndex = 0;
        private readonly object mLock = new object();

        public UniqueIdUtil(string tenantId, long range)
        {
            using (new PerformanceScope("UniqueIdUtil--GetIdRange"))
            {
                mRMUniqueIdSetting = UniqueIdSettingDao.LoadingUniqueIdSetting();
                mUniqueIdList = GenerateUniqueIdListAsync(tenantId, range).Result;
            }
        }

        public UniqueIdUtil()
        {

        }

        public UniqueIdUtil(string tenantId, long range, UniqueIdType uniqueIdType)
        {
            using (new PerformanceScope("UniqueIdUtil--GetIdRange"))
            {
                mRMUniqueIdSetting = UniqueIdSettingDao.LoadingUniqueIdSetting(uniqueIdType);
                var lockKey = tenantId + uniqueIdType.ToString();
                mUniqueIdList = GenerateUniqueIdListAsync(lockKey, range).Result;
            }
        }

        public RMUniqueIdSetting GetFSUniqueIdSetting()
        {
            return UniqueIdSettingDao.LoadingUniqueIdSetting(UniqueIdType.FileSystem);
        }

        public List<long> GetFSUniqueIdList(string tenantId, long range)
        {
            var lockKey = tenantId + UniqueIdType.FileSystem.ToString();
            return GenerateUniqueIdListAsync(lockKey, range).Result;
        }

        public FileSystemUniqueIdDto GetFileSystemUniqueSetting()
        {
            var setting = UniqueIdSettingDao.LoadingUniqueIdSetting(UniqueIdType.FileSystem);
            if(setting != null)
            {
                return new FileSystemUniqueIdDto
                {
                    Id = setting.Id,
                    IsActived = setting.IsActived,
                    IsStored = setting.OverrideSPPrefix,
                    Name = setting.Name,
                    Prefix = setting.Prefix,
                    UniqueIdType = (int)setting.UniqueIdType
                };
            }

            return null;
        }

        public string GenerateUniqueId()
        {
            try
            {
                lock (mLock)
                {
                    return FormateCurrentId(mRMUniqueIdSetting, mUniqueIdList[mIndex++]);
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while generating unique id. Error:{e.ToString()}");
                throw;
            }
        }

        private async Task<List<long>> GenerateUniqueIdListAsync(string tenantId, long range)
        {
            if (range == 0)
            {
                return new List<long>();
            }
            List<long> tempList = new List<long>();
            var ids = await RMGlobalLocker.GetIdRangeAsync(tenantId, range);
            for (long i = ids.Item1; i < ids.Item2; i++)
            {
                tempList.Add(i + 1);
            }
            return tempList;
        }

        private string FormateCurrentId(RMUniqueIdSetting setting, long number)
        {
            var result = string.Empty;
            try
            {
                string templateId = string.Empty;//Electric unique id do not have templateid, so we use templatedId = string.Empty
                string currentFormat = "{0}-{1}";
                if (setting != null)
                {
                    result = string.IsNullOrEmpty(setting.Prefix) ? FormatNumber(number) : string.Format(currentFormat, setting.Prefix, FormatNumber(number));
                }
                else
                {
                    result = string.Format(currentFormat, UniqueIdConfig.DefaultPrefix, FormatNumber(number));
                }
            }
            catch (Exception e)
            {
                logger.Info("Failed to formate currentId : " + e.ToString());
                throw;
            }
            return result;
        }

        private string FormatNumber(long number, int digit = 10, bool throwIfOverLength = false)
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