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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Model.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Common;
using AvePoint.RA.DB.Dao;
using Newtonsoft.Json;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Service.Services.Discovery.Office365.Converter;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365;
using AvePoint.GCommon.Utility;

namespace AvePoint.RA.Service.Services.Discovery.Office365
{
    public class RMDiscoveryOffice365BaiscInfoQueryService : IRMDiscoveryOffice365BasicInfoQueryService
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365BaiscInfoQueryService));

        private readonly IRMDiscoveryOffice365TenantDao _o365TenantInfoDao = new RMDiscoveryOffice365TenantDao();

        private readonly IRMDiscoveryOffice365FileExtensionDao _fileExtensionDao = new RMDiscoveryOffice365FileExtensionDao();

        private readonly IRMDiscoveryOffice365WithoutInDateDao _withoutInDateDao = new RMDiscoveryOffice365WithoutInDateDao();

        private readonly IRMDiscoveryOffice365SizeRangeDao _sizeRangeDao = new RMDiscoveryOffice365SizeRangeDao();

        private readonly IRMDiscoveryOffice365RuleInfoDao _ruleInfoDao = new RMDiscoveryOffice365RuleInfoDao();

        private readonly IRMDiscoveryOffice365DataQueryDao _dataQueryDao = new RMDiscoveryOffice365DataQueryDao();

        private readonly IRMDiscoveryConfigurationDao _configDao = new RMDiscoveryConfigurationDao();

        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public async Task<List<RMDiscoveryOffice365TenantDataInfo>> GetO365TenantInfoesAsync()
        {
            try
            {
                return (await _o365TenantInfoDao.GetAllAsync()).ConvertAll(item => new RMDiscoveryOffice365TenantDataInfo
                {
                    Id = item.Id,
                    UniqueId = item.UniqueId,
                    Name = item.Name,
                    AdminUrl = item.AdminUrl
                });
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get o365 tenant infoes. Error: {e}");
                return new();
            }
        }

        public async Task<List<RMDiscoveryFileExtensionDataInfo>> GetFileExtensionsAsync(Guid o365TenantId)
        {
            try
            {
                var defaultLimit = 100;

                var limitSetting = GetFileExtensionInternalLimit();

                int limit = limitSetting.Enabled ? limitSetting.Limit : defaultLimit;

                _logger.Info($"The discovery file extension limit is {limit}.");

                var schemaName = SecurityUtils.SanitizeSQLSchemaName(RMDiscoveryDBManager.GetOffice365SchemaName(o365TenantId));
                var sql = $@"SELECT TOP {limit} fileType.Id AS Id, fileType.Name AS Name, SUM(data.FileTotalSize) AS FileTotalSize 
                    FROM [{schemaName}].[RMBasicInactiveData] AS data 
                    JOIN [{schemaName}].[RMFileExtensions] AS fileType 
                    ON data.FileExtension = fileType.Id 
                    GROUP BY fileType.Id, fileType.Name
                    ORDER BY FileTotalSize DESC";
                var dataList = await _dataQueryDao.GetDataListAsync<RMDiscoveryFileExtensionDataInfo>(sql);
                dataList.ForEach(item =>
                {
                    item.Name = I18NEntity.GetString(item.Name);
                    item.RealName = item.Name;
                });
                return dataList;
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

        public async Task<RMDiscoverySummaryStatisticalDataInfo> GetSummaryStaticalDataInfoAsync(Guid o365TenantId)
        {
            try
            {
                var res = await _dataQueryDao.GetAggregateTotalDataListAsync(o365TenantId);
                return new()
                {
                    FileTotalSize = res.Sum(item => item.FileTotalSize),
                    FileSumCount = res.Sum(item => item.FileSumCount),
                    MaxFileAge = res.Max(item => item.MaxFileAge),
                    TotalVersionSize = res.Sum(item => item.TotalVersionSize),
                    PHLVolume = res.Sum(item => item.PHLVolume),
                    DuplicateFileTotalSize = res.Sum(item => item.DuplicateFileTotalSize),
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
                var rotConfig = await _configDao.GetAsync<RMDiscoveryOffice365RotDefinition>(RMDiscoveryConfigurationType.Office365ROTDefinition);
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
