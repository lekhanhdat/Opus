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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Google;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Google;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.Office365.Converter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Google
{
    public class RMDiscoveryGoogleBasicInfoQueryService : IRMDiscoveryGoogleBasicInfoQueryService
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleBasicInfoQueryService));

        private readonly IRMDiscoveryGoogleOrganizationInfoDao _organizationInfoDao = new RMDiscoveryGoogleOrganizationInfoDao();

        private readonly IRMDiscoveryGoogleDataQueryDao _dataQueryDao = new RMDiscoveryGoogleDataQueryDao();

        private readonly IRMDiscoveryGoogleFileExtensionDao _fileExtensionDao = new RMDiscoveryGoogleFileExtensionDao();

        private readonly IRMDiscoveryGoogleWithoutInDateDao _withoutInDateDao = new RMDiscoveryGoogleWithoutInDateDao();

        private readonly IRMDiscoveryGoogleSizeRangeDao _sizeRangeDao = new RMDiscoveryGoogleSizeRangeDao();

        private readonly IRMDiscoveryGoogleRuleInfoDao _ruleInfoDao = new RMDiscoveryGoogleRuleInfoDao();

        private readonly IRMDiscoveryConfigurationDao _configDao = new RMDiscoveryConfigurationDao();

        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public async Task<List<RMDiscoveryGoogleOrganizationDataInfo>> GetOrganizationInfoesAsync()
        {
            try
            {
                return (await _organizationInfoDao.GetAllAsync()).ConvertAll(item => new RMDiscoveryGoogleOrganizationDataInfo
                {
                    Id = item.Id,
                    OrganizationId = item.OrganizationId,
                    Name = item.Name,
                });
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while get organization infoes. Error: {e}");
                return [];
            }
        }

        public async Task<List<RMDiscoveryFileExtensionDataInfo>> GetFileExtensionsAsync(string organizationId)
        {
            try
            {
                var limit = GetFileExtensionInternalLimit();
                if (limit.Enabled)
                {
                    var schemaName = SecurityUtils.SanitizeSQLSchemaName(RMDiscoveryDBManager.GetGoogleSchemaName(organizationId));
                    var sql = $@"SELECT fileType.Id AS Id, fileType.Name AS Name, SUM(data.FileTotalSize) AS FileTotalSize 
FROM [{schemaName}].[RMGoogleBasicInactiveData] AS data 
JOIN [{schemaName}].[RMGoogleFileExtensions] AS fileType 
ON data.FileExtension = fileType.Id 
GROUP BY fileType.Id, fileType.Name";
                    var dataList = await _dataQueryDao.GetDataListAsync<RMDiscoveryFileExtensionDataInfo>(sql);
                    dataList.ForEach(item => item.Name = I18NEntity.GetString(item.Name));
                    return dataList.OrderByDescending(item => item.FileTotalSize)
                        .Take(limit.Limit)
                        .ToList();
                }
                return (await _fileExtensionDao.GetAllAsync(organizationId)).ConvertAll(item => new RMDiscoveryFileExtensionDataInfo
                {
                    Id = item.Id,
                    Name = I18NEntity.GetString(item.Name),
                    RealName = item.Name,
                });
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get file extensions. Error: {e}");
                return new();
            }
        }

        public async Task<List<RMDiscoveryWithoutInDateDataInfo>> GetWithoutInDateListAsync()
        {
            try
            {
                return (await _withoutInDateDao.GetAllAsync())
                    .OrderBy(item => item.Order)
                    .ConvertAll(item => new RMDiscoveryWithoutInDateDataInfo
                    {
                        Id = item.Id,
                        Unit = item.Unit,
                        UnitType = item.UnitType,
                        Order = item.Order
                    })
                    .ToList();
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get without in date list. Error: {e}");
                return new();
            }
        }

        public async Task<List<RMDiscoverySizeRangeDataInfo>> GetSizeRangeListAsync()
        {
            try
            {
                return (await _sizeRangeDao.GetAllAsync())
                    .OrderBy(item => item.Order)
                    .ConvertAll(item => new RMDiscoverySizeRangeDataInfo
                    {
                        Id = item.Id,
                        Name = I18NEntity.GetString(item.DisplayName),
                        LessThan = item.LessThan,
                        GenerateEqual = item.GenerateEqual,
                        Order = item.Order
                    })
                    .ToList();
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get size range list. Error: {e}");
                return new();
            }
        }

        public async Task<List<RMDiscoveryTableColumnInfo>> GetInactiveTableColumnsAsync()
        {
            try
            {
                var rules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.Inactive);
                return rules.OrderBy(item => item.Order)
                    .ConvertAll(item => new RMDiscoveryTableColumnInfo(item.Name, item.ToCustomColumn().Name, item.Id)).ToList();
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get inactive table columns. Error: {e}");
                return new();
            }
        }

        public async Task<RMDiscoverySummaryStatisticalDataInfo> GetSummaryStaticalDataInfoAsync(string organizationId)
        {
            try
            {
                var res = await _dataQueryDao.GetAggregateTotalDataListAsync(organizationId);
                return new()
                {
                    FileTotalSize = res.Sum(item => item.FileTotalSize),
                    FileSumCount = res.Sum(item => item.FileSumCount),
                    MaxFileAge = res.Max(item => item.MaxFileAge),
                    TotalVersionSize = res.Sum(item => item.TotalVersionSize),
                    DuplicateFileTotalSize = -1,
                };
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query statical data info. Error: {e}");
                return new();
            }
        }

        public async Task<RMDiscoveryRotRuleDataInfo> GetRotRuleDataInfoAsync()
        {
            try
            {
                var rules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.ROT);
                rules = rules.Where(item => item.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ToList();
                var res = rules.ConvertAll(item => new RMDiscoveryRotRuleDataInfo() { Id = item.Id, Label = item.Name, Category = item.Category }).ToList();
                return RMDiscoveryRuleTreeConverter.ConvertToFilterItem(res);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get rot rule data info. Error: {e}");
                return new();
            }
        }

        public async Task<List<RMDiscoveryRuleDataInfo>> GetRotRuleInfeosAsync()
        {
            try
            {
                var rules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.ROT);
                rules = rules.Where(item => item.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ToList();
                return rules.ConvertAll(item => new RMDiscoveryRuleDataInfo
                {
                    Id = item.Id,
                    Name = item.Name,
                    AnalyseMethod = item.AnalyseMethod,
                    Category = item.Category,
                });
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get rot rule infoes. Error: {e}");
                return [];
            }
        }

        public async Task<bool> GetRotEnableAsync()
        {
            try
            {
                var rotConfig = await _configDao.GetAsync<RMDiscoveryGoogleRotDefinition>(RMDiscoveryConfigurationType.GoogleROTDefinition);
                return rotConfig.Enable;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get rot rule data info. Error: {e}");
                return false;
            }
        }

        private RMDiscoveryFileExtensionInternalLimit GetFileExtensionInternalLimit()
        {
            try
            {
                var valueJson = _keyValueDao.GetValueByKey("DISCOVERY_FILE_EXTENSION_INTERNAL_LIMIT");
                if (valueJson == null || string.IsNullOrEmpty(valueJson.Value))
                {
                    return new()
                    {
                        Enabled = false
                    };
                }

                return JsonConvert.DeserializeObject<RMDiscoveryFileExtensionInternalLimit>(valueJson.Value);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get file extension internal limit. Error: {e}");
                return new()
                {
                    Enabled = false
                };
            }
        }
    }
}
