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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.ControlPlus;
using AvePoint.RA.Contract.CustomizeConnector.I18ns;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.ManualApproval.Model;
//using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Service.Services.ManualApproval.Queriers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.MachineLearningManualApproval.Queriers
{
    public class MLManualApprovalQuerier
    {
        private static readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private static IUserService UserService => PlatformWindsorManager.GetService<IUserService>();

        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private static IRMMLTermDao MLTermDao => PlatformWindsorManager.GetService<IRMMLTermDao>();
        private static ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private static IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        private static Dictionary<Guid, MLTermDto> TrainingTermCache = new();

        private static Dictionary<string, string> SiteUrlCache = new();

        private static MLManualApprovalRecordRepository Repository => new();


        private static readonly Dictionary<ManualApprovalFilterOptions, IFilter> FilterCollection = new();

        private static readonly Dictionary<ManualApprovalOrderOptions, ISorter> SorterCollection = new();

        private static readonly Dictionary<ManualApprovalDefaultOptions, IDefaultValue> DefaultValueCollection = new();

        static MLManualApprovalQuerier()
        {
            InitFilterCollection();
            InitSorterCollection();
            InitDefaultValueCollection();
        }

        private static void InitFilterCollection()
        {
            try
            {
                var filterType = typeof(IFilter);
                var assembly = Assembly.GetAssembly(filterType);

                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsInterface) continue;
                    if (type.GetInterfaces().Contains(filterType))
                    {
                        var instance = Activator.CreateInstance(type) as IFilter;
                        FilterCollection.Add(instance.FilterOption, instance);
                    }
                }
                Logger.Info($"Succeed init filter collection.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while init filter collection. Error: {e}");
                throw;
            }
        }

        private static void InitSorterCollection()
        {
            try
            {
                var sorterType = typeof(ISorter);
                var assembly = Assembly.GetAssembly(sorterType);

                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsInterface) continue;
                    if (type.GetInterfaces().Contains(sorterType))
                    {
                        var instance = Activator.CreateInstance(type) as ISorter;
                        SorterCollection.Add(instance.OrderOption, instance);
                    }
                }
                Logger.Info($"Succeed init sorter collection.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while init sorter collection. Error: {e}");
                throw;
            }
        }

        private static void InitDefaultValueCollection()
        {
            try
            {
                var deafultValueType = typeof(IDefaultValue);
                var assembly = Assembly.GetAssembly(deafultValueType);

                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsInterface) continue;
                    if (type.GetInterfaces().Contains(deafultValueType))
                    {
                        var instance = Activator.CreateInstance(type) as IDefaultValue;
                        DefaultValueCollection.Add(instance.DefaultValueOption, instance);
                    }
                }
                Logger.Info($"Succeed init default value collection.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while init default value collection. Error: {e}");
                throw;
            }
        }

        public static async Task<ManualApprovalPaginateResult> CosmosDBQueryAsync(ManualApprovalQueryDefinition queryDefinition)
        {
            var repository = Repository;
            await PrePermissionValidateAsync(queryDefinition);
            var filterExpresions = await BuildCosmosDBFilterAsync(queryDefinition);
            var sorterDefinitions = BuildCosmosDBSorter(queryDefinition);
            var explorerQueryDefinition = new ManualApprovalExplorerQueryDefinition
            {
                PageSize = queryDefinition.PageSize,
                Continuation = queryDefinition.Continuation,
                Predicates = filterExpresions,
                OrderDefinitions = sorterDefinitions
            };

            var result = new ManualApprovalPaginateResult();

            if (queryDefinition.NeedCalculationCount)
            {
                var count = await repository.CountAsync(explorerQueryDefinition);
                result.Count = count;
            }

            var explorerQueryResult = await repository.QueryItemsWithPaginationAsync(explorerQueryDefinition);

            var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();
            if (queryDefinition.FromGControl) generalSetting.TimeZoneId = GeneralSettingService.ConvertBrowserTimeZoneToWindows(TenantLocalValue.TimezoneId);
            var items = await explorerQueryResult.Items.ConvertAllAsync(item => ConvertAsync(item, generalSetting));

            result.Continuation = explorerQueryResult.Continuation;
            result.Items = items;

            return result;
        }

        public static async Task<List<ManualApprovalDefaultOptionDefinition>> GetFilterDefaultOptionsAsync()
        {
            var result = new List<ManualApprovalDefaultOptionDefinition>();

            foreach (var defaultValueEntry in DefaultValueCollection)
            {
                var value = await defaultValueEntry.Value.GetDefaultValueAsync();
                result.Add(new ManualApprovalDefaultOptionDefinition
                {
                    DefaultOption = defaultValueEntry.Key,
                    Value = value
                });
            }

            return result;
        }

        public static async Task<int> Count(ManualApprovalQueryDefinition queryDefinition)
        {
            var repository = Repository;
            await PrePermissionValidateAsync(queryDefinition);
            var filterExpresions = await BuildCosmosDBFilterAsync(queryDefinition);
            var explorerQueryDefinition = new ManualApprovalExplorerQueryDefinition
            {
                PageSize = queryDefinition.PageSize,
                Continuation = queryDefinition.Continuation,
                Predicates = filterExpresions,
            };
            var count = await repository.CountAsync(explorerQueryDefinition);
            return count;
        }

        private static async System.Threading.Tasks.Task PrePermissionValidateAsync(ManualApprovalQueryDefinition queryDefinition)
        {
            var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(Contract.RoleAssignments.RMPermissionMasks.ManualReviewAdmin);
            if (isAdmin || TenantLocalValue.RequesterType == RequesterTypeEnum.OpusControlPlus)
            {
                return;
            }

            var reviewerFilter = new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.MLReviewer,
                Value = "[]"
            };
            queryDefinition.Filters.Add(reviewerFilter);

            var userHasPermissionIntIds = await UserService.GetUserAndGroupIdsAsync(TenantLocalValue.LogonUserId);
            reviewerFilter.Value = JsonConvert.SerializeObject(userHasPermissionIntIds);
        }

        private static async Task<List<Expression<Func<ManualApprovalRecord, bool>>>> BuildCosmosDBFilterAsync(ManualApprovalQueryDefinition queryDefinition)
        {
            var result = new List<Expression<Func<ManualApprovalRecord, bool>>>();
            foreach (var filterDefinition in queryDefinition.Filters)
            {
                var filterOption = filterDefinition.FilterOption;
                var filter = FilterCollection[filterOption];
                var expression = await filter.GetCosmosDBFilterExpressionAsync(filterDefinition.Value);
                result.Add(expression);
            }

            return result;
        }

        private static List<ManualApprovalExplorerOrderDefinition> BuildCosmosDBSorter(ManualApprovalQueryDefinition queryDefinition)
        {
            var result = new List<ManualApprovalExplorerOrderDefinition>();
            if (queryDefinition.OrderBy != ManualApprovalOrderOptions.None)
            {
                var sorter = SorterCollection[queryDefinition.OrderBy];
                var expression = sorter.GetCosmosDBOrderExpression();
                result.Add(new ManualApprovalExplorerOrderDefinition
                {
                    OrderKeySelector = expression,
                    IsDesc = queryDefinition.IsDesc
                });
            }


            if (queryDefinition.OrderBy == ManualApprovalOrderOptions.None)
            {
                var predictCollectionTimeSorter = SorterCollection[ManualApprovalOrderOptions.PredictTime];
                var predictCollectionTimeExpression = predictCollectionTimeSorter.GetCosmosDBOrderExpression();
                result.Add(new ManualApprovalExplorerOrderDefinition
                {
                    OrderKeySelector = predictCollectionTimeExpression,
                    IsDesc = true
                });
            }

            return result;
        }

        private static async Task<ManualApprovalItem> ConvertAsync(ManualApprovalRecord record, GeneralSettingModel gls)
        {

            async Task<List<string>> GetUsersDisplayNamesAsync(int[] userIntIds)
            {
                if (userIntIds == null || userIntIds.Length == 0)
                {
                    return new List<string>();
                }
                var users = (await AccountDao.GetUserByIdsAsync(userIntIds.ToHashSet().ToList()));
                var displayNames = users.ConvertAll(item => item.DisplayName);
                return displayNames;
            }

            async Task<string> GetUserDisplayNameAsync(int userIntId)
            {
                if (userIntId <= 0)
                {
                    return "";
                }

                var user = await AccountDao.GetUserByIdAsync(userIntId);
                return user.DisplayName;
            }

            var preditcTermInfo = GetPredictTermInfo(record.PredictTermId);
            return new ManualApprovalItem
            {
                Id = record.Id,
                RecordsId = record.RecordsId,
                SourceFlag = record.SourceFlag,
                SourceName = I18NEntity.GetString(BuildInContentSourceI18Ns.SourceFlagI18ns[(SourceFlag)record.SourceFlag]),
                SourceIcon = BuildInContentSourceI18Ns.SourceFlagIcons[(SourceFlag)record.SourceFlag],
                NodeType = record.NodeType,
                LeafName = record.LeafName,
                FileExtension = I18NEntity.GetString(record.ExtensionForFile),
                NodeId = record.NodeId,
                ReviewerDisplayNames = await GetUsersDisplayNamesAsync(record.MLReviewer),
                EscalateFromDisplayName = await GetUserDisplayNameAsync(record.MLEscalateFrom),
                FullPath = GetRecordFullPath(record),
                //ApprovedByDisplayName = GetUserDisplayName(record.ManualApprovedBy),
                //ApprovedStatus = record.ManualApprovedStatus,
                EscalatedComment = record.MLEscalatedComment,
                CreatedBy = record.CreatedBy,
                ModifiedBy = record.ModifiedBy,
                CollectionTime = GeneralSettingService.ConvertTiksToDateTime(gls, record.PredictTime, true).SimplifyFormatTime,
                PredictTermName = preditcTermInfo?.Name,
                PredictTermId = record.PredictTermId,
                PredictTermFullPath = preditcTermInfo?.FullPath,
                ContainerId = record.ContainerId,
                CreatedTime = GeneralSettingService.ConvertTiksToDateTime(gls, record.TimeCreated, true).SimplifyFormatTime,
                ModifiedTime = GeneralSettingService.ConvertTiksToDateTime(gls, record.TimeModified, true).SimplifyFormatTime,
            };
        }

        private static string GetRecordFullPath(ManualApprovalRecord record)
        {
            try
            {
                var siteUrl = GetSiteUrl(record.AveSiteId);
                var fullPath = WebUtil.MakeFullUrl(siteUrl, record.DirPath);
                if (record.ExtensionForFile == "RM_RDM_RecordDetails_DataType_SPItem")
                {
                    fullPath = WebUtil.GetListItemRealPath(fullPath);
                }
                return fullPath;
            }
            catch (Exception ex)
            {
                Logger.Warn($"An error while get record full path, record id: {record?.Id} message: {ex}");
                return string.Empty;
            }
        }

        private static string GetSiteUrl(string siteId)
        {
            if (!SiteUrlCache.TryGetValue(siteId, out var siteUrl))
            {
                siteUrl = RemoteNodeDao.GetRemoteSiteCollectionById(siteId)?.url;
                if (!SiteUrlCache.TryAdd(siteId, siteUrl))
                {
                    Logger.Warn($"An error while add site url, site is:{siteId}");
                }
            }
            return siteUrl;
        }

        private static MLTermDto GetPredictTermInfo(Guid termId)
        {
            try
            {
                var term = MLTermDao.GetTrainingTerm(termId);
                if (term != null)
                {
                    term.FullPath = TermDao.GetTermNamesPathByTermId(termId);
                }
                return term;
            }
            catch (Exception ex)
            {
                Logger.Warn($"An error while get predict term info, termId: {termId} message: {ex}");
                return null;
            }
        }
    }
}
