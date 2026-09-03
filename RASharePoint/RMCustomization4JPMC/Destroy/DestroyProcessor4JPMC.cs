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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.RMCustomization4JPMC.Excel;
using AvePoint.Wrapper.Common;
using RAArchiverCommon.DestructionCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMCustomization4JPMC.Destroy
{
    public class DestroyProcessor4JPMC : IDisposable
    {
        private readonly RALogger logger = RALogger.GetInstance(typeof(DestroyProcessor4JPMC));
        private string mSiteID;
        public DestroyProcessor4JPMC(string siteID)
        {
            mSiteID = siteID;
            LoadDestructionCache();
        }
        public void LoadDestructionCache()
        {
            string siteId = mSiteID;
            string filePath = String.Empty;
            using (PerformanceScope scope = new PerformanceScope("DestroyProcessor4JPMC.DownloadCacheFromStorage"))
            {
                filePath = DestructionFactory.GetInstance(siteId.ToString(), string.Empty).DownloadCacheFromStorage(siteId.ToString(), DateTime.MinValue, DateTime.MaxValue);
            }
            var LiteDBWrapper = DestructionCacheLiteDBWrapper.CreateInstance(GetLiteDBPath(siteId.ToString()));

            if (!string.IsNullOrWhiteSpace(filePath) && System.IO.Directory.Exists(filePath))
            {
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(filePath);
                foreach (var file in dir.GetFiles())
                {
                    DestructionUtility destructionUtility = new DestructionUtility(file.FullName);
                    int pageSize = 100;
                    int pageIndex = 0;
                    int readCount;
                    do
                    {
                        var records = destructionUtility.SelectValuesFromDB(pageIndex, pageSize);
                        pageIndex += records.Count;
                        readCount = records.Count;
                        LiteDBWrapper.Insert(records);
                    }
                    while (readCount == 100);
                }
                try
                {
                    System.IO.Directory.Delete(filePath, true);
                }
                catch(Exception e)
                {
                    logger.Error($"Load destruction {e}");
                }
            }
            else
            {
                logger.Warn("Destruction cache file not exist.");
            }
            DestructionFactory.Dispose(siteId.ToString(), string.Empty);
        }

        public long GetTotalCount()
        {
            using AvePerformanceScope pc = new AvePerformanceScope("DestroyProcessor4JPMC.GetTotalCount");
            var siteId = mSiteID;
            var LiteDBWrapper = DestructionCacheLiteDBWrapper.CreateInstance(GetLiteDBPath(siteId.ToString()));
            return LiteDBWrapper.QueryCountByActionType((int)ActionType.DeleteOnly);
        }
        
        public long GetTotalCount(string listId)
        {
            using AvePerformanceScope pc = new AvePerformanceScope("DestroyProcessor4JPMC.GetTotalCountByListId");
            var siteId = mSiteID;
            var LiteDBWrapper = DestructionCacheLiteDBWrapper.CreateInstance(GetLiteDBPath(siteId.ToString()));
            return LiteDBWrapper.QueryCountByActionType((int)ActionType.DeleteOnly, listId);
        }

        private string GetLiteDBPath(string siteId)
        {
            return SecurityUtils.SafeCombinePath(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.REPORT_TEMP_FOLDER], "DestructionLiteDB", siteId);
        }

        public void Dispose()
        {
            var siteId = mSiteID;
            DestructionFactory.Dispose(siteId.ToString(), string.Empty);
            DestructionCacheLiteDBWrapper.CreateInstance(GetLiteDBPath(siteId.ToString())).Dispose();
        }
    }
}
