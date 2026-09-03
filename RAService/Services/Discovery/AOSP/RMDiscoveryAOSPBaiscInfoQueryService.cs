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
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.Contract.Discovery.Model.Query.AOSP;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using Newtonsoft.Json;
using AvePoint.RA.Contract.Discovery.Model.Configuration.AOSP;
using AvePoint.RA.Service.Services.Discovery.Office365.Converter;
using AvePoint.GCommon.Utility;

namespace AvePoint.RA.Service.Services.Discovery.AOSP
{
    public class RMDiscoveryAOSPBaiscInfoQueryService : IRMDiscoveryAOSPBasicInfoQueryService
    {

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryAOSPBaiscInfoQueryService));

        private readonly IRMDiscoveryAOSPTenantDao _o365TenantInfoDao = new RMDiscoveryAOSPTenantDao();

        private readonly IRMDiscoveryAOSPFileExtensionDao _fileExtensionDao = new RMDiscoveryAOSPFileExtensionDao();

        private readonly IRMDiscoveryAOSPWithoutInDateDao _withoutInDateDao = new RMDiscoveryAOSPWithoutInDateDao();

        private readonly IRMDiscoveryAOSPSizeRangeDao _sizeRangeDao = new RMDiscoveryAOSPSizeRangeDao();

        private readonly IRMDiscoveryAOSPRuleInfoDao _ruleInfoDao = new RMDiscoveryAOSPRuleInfoDao();

        private readonly IRMDiscoveryAOSPDataQueryDao _dataQueryDao = new RMDiscoveryAOSPDataQueryDao();

        private readonly IRMDiscoveryAOSPConfigurationDao _configDao = new RMDiscoveryAOSPConfigurationDao();

        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public async Task<List<RMDiscoveryAOSPTenantDataInfo>> GetO365TenantInfoesAsync()
        {
            try
            {
                return (await _o365TenantInfoDao.GetAllAsync()).ConvertAll(item => new RMDiscoveryAOSPTenantDataInfo
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

        public async Task<List<RMDiscoveryFileExtensionDataInfo>> GetFileExtensionsAsync(string o365TenantId)
        {
            try
            {
                var limit = GetFileExtensionInternalLimit(); 
                if (limit.Enabled)
                {
                    var schemaName = SecurityUtils.SanitizeSQLSchemaName(RMDiscoveryDBManager.GetAOSPSchemaName(new Guid(o365TenantId)));
                    var sql = $@"SELECT fileType.Id AS Id, fileType.Name AS Name, SUM(data.FileTotalSize) AS FileTotalSize 
FROM [{schemaName}].[RMBasicInactiveData] AS data 
JOIN [{schemaName}].[RMFileExtensions] AS fileType 
ON data.FileExtension = fileType.Id 
GROUP BY fileType.Id, fileType.Name";
                    var dataList = await _dataQueryDao.GetDataListAsync<RMDiscoveryFileExtensionDataInfo>(sql);
                    dataList.ForEach(item => item.Name = I18NEntity.GetString(item.Name));
                    return dataList.OrderByDescending(item => item.FileTotalSize)
                        .Take(limit.Limit)
                        .ToList();
                }
                return (await _fileExtensionDao.GetAllAsync(new Guid(o365TenantId))).ConvertAll(item => new RMDiscoveryFileExtensionDataInfo
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

        public async Task<List<RMDiscoveryWithoutInDateDataInfo>> GetWithoutInDateListAsync(string o365TenantId)
        {
            try
            {
                return (await _withoutInDateDao.GetAllAsync(o365TenantId))
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

        public async Task<List<RMDiscoverySizeRangeDataInfo>> GetSizeRangeListAsync(string o365TenantId)
        {
            try
            {
                return (await _sizeRangeDao.GetAllAsync(o365TenantId))
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

        public async Task<List<RMDiscoveryTableColumnInfo>> GetInactiveTableColumnsAsync(string o365TenantId)
        {
            try
            {
                var rules = await _ruleInfoDao.GetRuleInfoesAsync(true, o365TenantId, RMDiscoveryRuleDefinitionKind.Inactive);
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

        public async Task<bool> GetRotEnableAsync(string o365TenantId)
        {
            try
            {
                var rotConfig = await _configDao.GetByO365TenantIdAsync<RMDiscoveryAOSPRotDefinition>(RMDiscoveryConfigurationType.AOSPROTDefinition, o365TenantId);
                return rotConfig.Enable;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get rot rule data info. Error: {e}");
                return false;
            }
        }

        public async Task<RMDiscoveryRotRuleDataInfo> GetRotRuleDataInfoAsync(string o365TenantId)
        {
            try
            {
                var rules = await _ruleInfoDao.GetRuleInfoesAsync(true, o365TenantId, RMDiscoveryRuleDefinitionKind.ROT);
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
