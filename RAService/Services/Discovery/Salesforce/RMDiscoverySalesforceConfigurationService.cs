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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Salesforce;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Salesforce;
using AvePoint.RA.DB.Dao.Discovery.Salesforce;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Salesforce;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Lcoker;
using AvePoint.RA.Service.Services.Discovery.Salesforce.Audit;
using AvePoint.RA.Service.Services.Discovery.Salesforce.Configuration;
using AvePoint.RA.Service.Services.Discovery.Salesforce.Configuration.Checker;
using AvePoint.RA.Service.Services.Discovery.Salesforce.License;
using AvePoint.RA.Service.Services.Discovery.Salesforce.Report;
using AvePoint.RA.Service.Services.Discovery.Salesforce.Work.Preparer;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.Services.Discovery.Salesforce
{
    [AsyncAudit]
    public class RMDiscoverySalesforceConfigurationService : IRMDiscoverySalesforceConfigurationService
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoverySalesforceConfigurationService));
        
        private readonly IRMDiscoveryConfigurationDao _configurationDao = new RMDiscoveryConfigurationDao();

        private readonly IRMTenantDiscoveryDBInfoDao _tenantInfoDao = new RMTenantDiscoveryDBInfoDao();
        
        private readonly IRMDiscoverySalesforceJobDao _jobDao = new RMDiscoverySalesforceJobDao();
        
        private readonly IRMDiscoverySalesforceWithoutInDateDao _withoutInDateDao = new RMDiscoverySalesforceWithoutInDateDao();

        private readonly IRMDiscoverySalesforceSizeRangeDao _sizeRangeDao = new RMDiscoverySalesforceSizeRangeDao();
        
        public async Task<RMDiscoverySalesforceConfigurationInfo> GetConfigurationInfoAsync()
        {
            try
            {
                if (!await _tenantInfoDao.IsInitTenantDiscoveryDBInfoAsync() || !await RMDiscoveryDBManager.CheckSalesforceTablesExistsAsync())
                {
                    _logger.Info("The tenant has not been initialized");
                    return new ()
                    {
                        SizeRangeInfoes = RMDiscoverySalesforceDefaultConfigurationInfo.DEFAULT_SIZE_RANGE_INFOES,
                        DateRangeInfoes = RMDiscoverySalesforceDefaultConfigurationInfo.DEFAULT_DATE_RANGE_INFOES,
                    };
                }

                var scopeInfo = await _configurationDao.GetAsync<RMDiscoverySalesforceScopeInfo>(RMDiscoveryConfigurationType.SalesforceNewlyScope);
                var listSizeRangeInfo = (await _sizeRangeDao.GetAllAsync()).ConvertAll<RMDiscoverySizeRangeDataInfo>(
                    item => new()
                    {
                        Id = item.Id,
                        Name = item.DisplayName,
                        GenerateEqual = item.GenerateEqual,
                        LessThan = item.LessThan,
                        Order = item.Order
                    });
                var listWithoutInDateInfo =
                    (await _withoutInDateDao.GetAllAsync()).ConvertAll<RMDiscoveryWithoutInDateDataInfo>(item => new()
                    {
                        Id = item.Id,
                        Unit = item.UnitType == RMDiscoveryWithoutInUnitType.Month ? item.Unit : item.Unit * 12,
                        UnitType = RMDiscoveryWithoutInUnitType.Month,
                        Order = item.Order
                    });
                
                var result = RMDiscoverySalesforceConfigurationAssembler.Instance
                    .AddScopeInfo(scopeInfo)
                    .AddSizeRangeInfo(listSizeRangeInfo)
                    .AddWithoutInDateInfo(listWithoutInDateInfo)
                    .Assemble();
                
                return result;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get configuration info. Error: {e}");
                return new ()
                {
                    SizeRangeInfoes = RMDiscoverySalesforceDefaultConfigurationInfo.DEFAULT_SIZE_RANGE_INFOES,
                    DateRangeInfoes = RMDiscoverySalesforceDefaultConfigurationInfo.DEFAULT_DATE_RANGE_INFOES,
                };
            }
        }
        
        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.DiscoveryConfiguration, Action = AuditAction.SaveDiscoveryConfiguration, IAsyncBeforeHandler = typeof(RMSalesforceDiscoveryConfigurationBeforeAuditHandler), IAsyncAfterHandler = typeof(RMSalesforceDiscoveryConfigurationAfterAuditHandler))]
        public async Task<RAReturnMessage> AddOrUpdateConfigurationInfoAsync(RMDiscoverySalesforceConfigurationInfo configurationInfo)
        {
            try
            {
                var resultMessage = new RAReturnMessage();
                
                var licenseType = await RMDiscoverySalesforceLicenseHelper.GetLicenseTypeAsync();
                if (licenseType == Cloud.Sdk.Data.AosModern.LicenseType.Trial)
                {
                    return new()
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = I18NEntity.GetString("Trial license not support process salesforce discovery job."),
                    };
                }
                var checker = new RMDiscoverySalesforceConfigurationNewlyChecker(configurationInfo);
                var (isPassed, message) = await checker.CheckAsync();
                if (!isPassed)
                {
                    _logger.Warn($"Newly Security check failed.");
                    resultMessage.MessageType = RAMessageType.Failed;
                    resultMessage.ErrorMessage = I18NEntity.GetString(message);
                    return resultMessage;
                }
                await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryConfiguration, TimeSpan.FromMinutes(15)))
                {
                    _logger.Info($"Start add or update configuration salesforce info.");

                    await RMDiscoveryDBManager.InitSalesforceDatabaseAsync();
                    
                    if (!await RMDiscoverySalesforceLicenseHelper.IsMeetLimitAsync())
                    {
                        resultMessage.MessageType = RAMessageType.Failed;
                        resultMessage.ErrorMessage = I18NEntity.GetString("RM_FA_License_JobLimit");
                        return resultMessage;
                    }
                    
                    _logger.Info($"Current tenant discovery database has been init.");

                    var (has, jobInfo) = await _jobDao.TryGetProcessingMainJobAsync();
                    if (has)
                    {
                        _logger.Warn($"Has processing main job [{jobInfo.Id}], prohibit add or update configuration info.");
                        resultMessage.MessageType = RAMessageType.Failed;
                        resultMessage.ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_RunJobFailed");
                        return resultMessage;
                    }
                    
                    using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
                    using var transaction = efContext.Database.BeginTransaction();
                    try
                    {
                        await AddOrUpdateConfigurations(configurationInfo, efContext);
                        await AddOrUpdateSizeRangesAsync(configurationInfo, efContext);
                        await AddOrUpdateDateRangesAsync(configurationInfo, efContext);
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }

                    _logger.Info($"Finished add or update salesforce configuration info.");

                    var preparer = new RMDiscoverySalesforceJobNewlyPreparer(configurationInfo.ScopeInfo.Organizations.Select(org => org.Id).ToList());
                    var (success, errorMessage) = await preparer.PrepareAsync();

                    _logger.Info($"Prepare salesforce discovery job is [{success}].");

                    resultMessage.MessageType = success ? RAMessageType.Successful : RAMessageType.Failed;
                    resultMessage.ErrorMessage = errorMessage;
                    return resultMessage;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while add or update configuration info async. Error: {e}");
                return new()
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_RunJobFailed"),
                };
            }
        }

        public async Task<string> DownloadDiscoveryJobReporAsync()
        {
            try
            {
                var (has, jobInfo) = await _jobDao.TryGetLatestMainJobAsync();
                var reportManager = new RMDiscoverySalesforceJobReportManager();
                return await reportManager.GenerateReportAsync(jobInfo);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while download discovery job report. Error: {e}");
                return string.Empty;
            }
        }

        private async Task AddOrUpdateDateRangesAsync(RMDiscoverySalesforceConfigurationInfo configurationInfo, RMDiscoveryDBEFContext efContext)
        {
            var withoutInDateInfoes = configurationInfo.DateRangeInfoes.OrderBy(item => item.Unit).ToList();
            for (int i = 0; i < withoutInDateInfoes.Count; i++)
            {
                withoutInDateInfoes[i].Order = i;
            }
            var willAddOrUpdateWithoutInDateInfoes = withoutInDateInfoes.ConvertAll(item => new RMDiscoverySalesforceWithoutInDate
            {
                Unit = item.Unit,
                UnitType = item.UnitType,
                Order = item.Order,
            });
            await _withoutInDateDao.DeleteAllInfoAsync(efContext);
            await _withoutInDateDao.AddOrUpdateAsync(efContext, willAddOrUpdateWithoutInDateInfoes);
        }

        private async Task AddOrUpdateSizeRangesAsync(RMDiscoverySalesforceConfigurationInfo configurationInfo, RMDiscoveryDBEFContext efContext)
        {
            var sizeRangeInfoes = configurationInfo.SizeRangeInfoes;
            sizeRangeInfoes.Add(new RMDiscoverySizeRangeDataInfo
            {
                GenerateEqual = sizeRangeInfoes.Count <= 0 ? 0 : sizeRangeInfoes[sizeRangeInfoes.Count - 1].LessThan,
                LessThan = int.MaxValue,
                Order = sizeRangeInfoes.Count + 1
            });
            sizeRangeInfoes = sizeRangeInfoes.OrderBy(item => item.GenerateEqual).ToList();
            for (int i = 0; i < sizeRangeInfoes.Count; i++)
            {
                var sizeRangeInfo = sizeRangeInfoes[i];
                sizeRangeInfo.Order = i;
                if (sizeRangeInfo.Order == 0)
                {
                    sizeRangeInfo.Name = "<" + sizeRangeInfo.LessThan.ToString() + " MB";
                    continue;
                }
                sizeRangeInfo.Name = ">=" + sizeRangeInfo.GenerateEqual.ToString() + " MB";
            }

            var willAddOrUpdateSizeRangeInfoes = sizeRangeInfoes.ConvertAll(item => new RMDiscoverySalesforceSizeRange
            {
                GenerateEqual = item.GenerateEqual,
                LessThan = item.LessThan,
                Order = item.Order,
                DisplayName = item.Name
            });
            await _sizeRangeDao.DeleteAllDataAsync(efContext);
            await _sizeRangeDao.AddOrUpdateAsync(efContext, willAddOrUpdateSizeRangeInfoes);
        }

        private async Task AddOrUpdateConfigurations(RMDiscoverySalesforceConfigurationInfo configurationInfo, RMDiscoveryDBEFContext efContext)
        {
            var willAddOrUpdateConfigurations = new Dictionary<RMDiscoveryConfigurationType, RMDiscoveryConfiguration>
            {
                {
                    RMDiscoveryConfigurationType.SalesforceNewlyScope, new(){
                        ConfigurationType = RMDiscoveryConfigurationType.SalesforceNewlyScope,
                        ValueJson = JsonConvert.SerializeObject(configurationInfo.ScopeInfo.UpdateRunningUser()),
                        CreateTime = DateTime.UtcNow.Ticks,
                        ModifiedTime = DateTime.UtcNow.Ticks,
                    }
                }
            };
            await _configurationDao.AddOrUpdateAsync(efContext, willAddOrUpdateConfigurations.Values.ToArray());
        }
    }
}
