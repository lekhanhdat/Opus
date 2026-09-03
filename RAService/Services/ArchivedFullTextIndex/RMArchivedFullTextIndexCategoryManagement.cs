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
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.ArchivedFullTextIndex;
using AvePoint.RA.DB.Dao.ArchivedFullTextIndex.Impl;
using Cloud.Sdk.Data.EDiscovery;
using Cloud.Sdk.EDiscovery;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ArchivedFullTextIndex
{
    public class RMArchivedFullTextIndexCategoryManagement
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMArchivedFullTextIndexCategoryManagement));

        private static long s_maxCategorySize = 1024 * 1024 * 1024 * 100L;

        private readonly IRMArchivedFullTextIndexCategoryDao _categoryDao = new RMArchivedFullTextIndexCategoryDao();

        private readonly EDiscoveryApiClient _apiClient;

        private const string CATEGORY_NAME = "RestoreFullTextIndex";

        static RMArchivedFullTextIndexCategoryManagement()
        {
            try
            {
                var keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
                var setting = keyValueDao.GetValueByKey("ARCHIVE_FULL_TEXT_INDEX_CATEGORY_LIMIT_SIZE");
                if (setting != null && !string.IsNullOrEmpty(setting.Value))
                {
                    if (long.TryParse(setting.Value, out var limitSize))
                    {
                        s_logger.Info($"The archive full text index category limit size [{limitSize}].");
                        s_maxCategorySize = limitSize;
                    }
                }
            }
            catch (Exception ex)
            {
                s_logger.Error("Error occurred while initializing category management. Use default max category size. ", ex);
            }
        }

        public RMArchivedFullTextIndexCategoryManagement()
        {
            if (!RMGlobalConfiguration.EnvSetting.IsGCPEnvironment)
            {
                _apiClient = AosApiUtility.GetEDiscoveryApiClient();
            }
        }

        public async Task SyncCategoryDataSizeAsync()
        {
            try
            {
                var res = await _apiClient.IndexService.CalculateCatalogSizeAsync(new Cloud.Sdk.Data.EDiscovery.SearchInfo()
                {
                    Category = CATEGORY_NAME,
                    Filter = []
                });
                if (!res.Successful)
                {
                    s_logger.Error($"The calculate category total size failed. Skipped sync.");
                    return;
                }
                s_logger.Info($"The total category data size is [{res.Size}].");
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while sync category data size. Error: {e}");
            }
        }

        public async Task<long> GetCategorySizeByArchiverTimeRangeAsync(long minDateTicks, long maxDateTicks)
        {
            try
            {
                s_logger.Info($"Start getting category data size. MinDateTicks=[{minDateTicks}], MaxDateTicks=[{maxDateTicks}]");
                var archiverTimeField = new Field
                {
                    Name = "archiverTime",
                    FieldType = FieldType.Long | FieldType.NeedIndex
                };

                var searchInfo = new SearchInfo
                {
                    Category = CATEGORY_NAME,
                    Filter =
                    [
                        new QueryGroup
                        {
                            Operator = FilterOperator.And,
                            QueryFields =
                            [
                                new FieldRangeQuery
                                {
                                    Min = minDateTicks,
                                    Max = maxDateTicks,
                                    MinInclusive = true,
                                    MaxInclusive = true,
                                    Field = archiverTimeField,
                                    Operator = FilterOperator.And
                                }
                            ]
                        }
                    ],
                    DocSeperator = new()
                    {
                        Field = archiverTimeField,
                        Type = SeparatorType.Month
                    }
                };

                var result = await _apiClient.IndexService.CalculateCatalogSizeAsync(searchInfo);

                if (result?.Successful == true)
                    return result.Size;

                s_logger.Error("Calculate category total size failed. Returning 0.");
                return 0;
            }
            catch (Exception ex)
            {
                s_logger.Error($"Error occurred while syncing category data size. Ex: {ex}");
                return 0;
            }
        }

        public async Task<long> CountAsync()
        {
            return await _categoryDao.CountAsync();
        }
    }
}
