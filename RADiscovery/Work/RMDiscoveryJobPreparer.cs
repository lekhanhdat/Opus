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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.RACommonUtility.Lcoker;
using Cloud.Sdk.Data.Core;
using Cloud.Sdk.Data.IE;
using Cloud.Sdk.IE;
using DataOrchestration.Tag.Sdk.Service.CloudRecords.Contract;
using Newtonsoft.Json;
using RADiscovery.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RADiscovery.Work
{
    public class RMDiscoveryJobPreparer : RMDiscoveryWorker
    {

        private readonly IRMSyncNodeDao _syncNodeDao = new RMSyncNodeDao();

        public async Task<bool> PrepareAsync(RMDiscoveryScopeInfo scopeInfo, bool hasRuleChange)
        {
            await using (await RMRedisLockHanlder.LockAsync(RMRedisLockKey.DiscoveryJob, TimeSpan.FromHours(1)))
            {
                using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
                var job = await efContext.MainJobs.FirstOrDefaultAsync(
                    item => item.Status == RMDiscoveryJobStatus.Preparing ||
                    item.Status == RMDiscoveryJobStatus.Running ||
                    item.Status == RMDiscoveryJobStatus.Pending);
                if (job != null)
                {
                    _logger.Warn($"there is already job [{job.Id}] being executed.");
                    return false;
                }

                if(hasRuleChange)
                {
                    await RegisteTagsAsync();
                }

                var containerCount = 0;
                var siteCount = 0;

                if (scopeInfo.ScopeType == RMDiscoveryScopeType.All)
                {
                    containerCount = await _syncNodeDao.CountContainerAsync();
                    siteCount = await _syncNodeDao.CountSiteAsync();
                }
                else if (scopeInfo.ScopeType == RMDiscoveryScopeType.SpecifyContainer)
                {
                    var existsContainerIds = new List<Guid>();

                    if(hasRuleChange)
                    {
                        var o365TenantIds = await efContext.O365TenantInfoes.Select(item => item.UniqueId).ToListAsync();
                        foreach(var o365TenantId in o365TenantIds)
                        {
                            using var tenantContext = await RMDiscoveryDBManager.GetEFContextAsync(o365TenantId);
                            var containerIds = await tenantContext.ContainerInfoes.Select(item => item.OpusId).ToListAsync();
                            existsContainerIds.AddRange(containerIds);
                        }
                    }

                    existsContainerIds.AddRange(scopeInfo.SpecifyContainerIds);
                    var existsContainerSet = existsContainerIds.ToHashSet();
                    containerCount = existsContainerSet.Count;
                    siteCount = await _syncNodeDao.CountSiteAsync(existsContainerSet);
                }

                job = new RMDiscoveryMainJob
                {
                    Id = Guid.NewGuid(),
                    StartTime = DateTime.UtcNow.Ticks,
                    ContainersCount = containerCount,
                    SitesCount = siteCount,
                    HasRuleChange = hasRuleChange,
                    Status = RMDiscoveryJobStatus.Preparing,
                };

                efContext.MainJobs.Add(job);
                var effectCount = await efContext.SaveChangesAsync();
                return effectCount > 0;
            }
        }

        public async Task<bool> RegisteTagsAsync()
        {
            var allTags = await _ieApiClient.TagRuleService.GetAllAsync(DataType.SPDocument, CallerType.CloudRecords);
            var allTagUniqueIds = allTags.Select(item => item.Id).ToHashSet();

            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var enabledRules = await efContext.RuleInfoes.Where(item => item.IsEnable).ToListAsync();
            var enabledRuleUniqueIds = enabledRules.Select(item => item.UniqueId).ToHashSet();

            var needDeleteTags = allTagUniqueIds.Except(enabledRuleUniqueIds);
            var needAddTags = enabledRuleUniqueIds.Except(allTagUniqueIds).ConvertAll(uniqueId =>
            {
                var rule = enabledRules.First(item => item.UniqueId == uniqueId);
                var definition = new
                {
                    Kind = rule.DefinitionKind,
                    Method = rule.AnalyseMethod,
                    Category = rule.Category,
                    CriteriaInfoes = rule.CriteriaInfoesJson,
                };
                return new TagRuleModel
                {
                    Id = rule.UniqueId,
                    Name = rule.Name,
                    Definition = JsonConvert.SerializeObject(definition),
                    Product = CallerType.CloudRecords,
                    Type = DataType.SPDocument
                };
            }).ToList();
            var needUpdateTags = allTagUniqueIds.Intersect(enabledRuleUniqueIds).ConvertAll(uniqueId =>
            {
                var rule = enabledRules.First(item => item.UniqueId == uniqueId);
                var definition = new
                {
                    Kind = rule.DefinitionKind,
                    Method = rule.AnalyseMethod,
                    Category = rule.Category,
                    CriteriaInfoes = rule.CriteriaInfoesJson,
                };
                return new TagRuleModel
                {
                    Id = rule.UniqueId,
                    Name = rule.Name,
                    Definition = JsonConvert.SerializeObject(definition),
                    Product = CallerType.CloudRecords,
                    Type = DataType.SPDocument
                };
            });

            await needDeleteTags.ToAsyncEnumerable().ForEachAwaitAsync(async item =>
            {
                await _ieApiClient.TagRuleService.DeleteAsync(item);
            });
            await _ieApiClient.TagRuleService.AddBatchAsync(needAddTags);
            await needUpdateTags.ToAsyncEnumerable().ForEachAwaitAsync(async item =>
            {
                await _ieApiClient.TagRuleService.UpdateAsync(item);
            });

            return true;
        }
    }
}
