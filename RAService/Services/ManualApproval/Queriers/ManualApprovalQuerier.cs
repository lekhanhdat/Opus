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

using AngleSharp.Common;
using Aspose.Pdf.Operators;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.CustomizeConnector.Enums;
using AvePoint.RA.Contract.CustomizeConnector.I18ns;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ManualApproval.Enums;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.SharePoint.CustomIndexMetadata;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Workflow;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using CommonModel.Utils;
using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using Microsoft.Azure.Cosmos;
using Microsoft.Graph;
using Newtonsoft.Json;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.ControlPlus;
using AvePoint.RA.Contract.GoogleOne;
using Util;

namespace AvePoint.RA.Service.Services.ManualApproval.Queriers
{
    public class ManualApprovalQuerier
    {
        
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ManualApprovalQuerier));

        private static IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private static IUserService UserService => PlatformWindsorManager.GetService<IUserService>();

        public static ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService<ITaxonomyService>();

        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private static IRMCache Cache => PlatformWindsorManager.GetService<IRMCache>();

        private static IRMCustomizeConnectorContentSourceDao CustomizeConnectorContentSourceDao => PlatformWindsorManager.GetService<IRMCustomizeConnectorContentSourceDao>();

        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private static IOneDriveSettingDao OneDriveSettingDao => PlatformWindsorManager.GetService<IOneDriveSettingDao>();

        private static IEXOSettingDao EXOSettingDao => PlatformWindsorManager.GetService<IEXOSettingDao>();

        private static IRMGoogleSettingDao RMGoogleSettingDao => PlatformWindsorManager.GetService<IRMGoogleSettingDao>();


        private static ManualApprovalRecordRepository Repository => new();

        private static readonly Dictionary<ManualApprovalFilterOptions, IFilter> FilterCollection =
            new Dictionary<ManualApprovalFilterOptions, IFilter>();

        private static readonly Dictionary<ManualApprovalOrderOptions, ISorter> SorterCollection =
            new Dictionary<ManualApprovalOrderOptions, ISorter>();

        private static readonly Dictionary<ManualApprovalDefaultOptions, IDefaultValue> DefaultValueCollection =
            new Dictionary<ManualApprovalDefaultOptions, IDefaultValue>();

        private static readonly List<ManualApprovalOrderOptions> CustomOrderOptions = new List<ManualApprovalOrderOptions>
        {
             ManualApprovalOrderOptions.CustomText,
             ManualApprovalOrderOptions.CustomYesOrNo,
             ManualApprovalOrderOptions.CustomDateTime,
             ManualApprovalOrderOptions.CustomNumber,
        };

        static ManualApprovalQuerier()
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

        public static async Task<ManualApprovalPaginateResult> CosmosDBQueryAsync(ManualApprovalQueryDefinition queryDefinition, string timeZoneId = "", bool isDaylight = false, string timeFormat = "")
        {
            var repository = Repository;
            await PrePermissionValidateAsync(queryDefinition);
            var filterExpresions = await BuildCosmosDBFilterAsync(queryDefinition.Filters);
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

            var contentSourceInfoes = await Cache.TryGetAsync(IRMCache.Keys.ManualApprovalQuerier_GetAllSimpleInfoes, async () =>
            {
                return (await CustomizeConnectorContentSourceDao.GetAllSimpleInfoes(CustomizeConnectorOrigin.BuildIn, CustomizeConnectorOrigin.ExternalCustomize)).ToDictionary(item => item.Flag, item => I18NEntity.GetString(item.Name));
            });                

            var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();
            if (queryDefinition.FromGControl) generalSetting.TimeZoneId = GeneralSettingService.ConvertBrowserTimeZoneToWindows(TenantLocalValue.TimezoneId);
            var items = await explorerQueryResult.Items.ConvertAllAsync<ManualApprovalRecord, ManualApprovalItem> (item => ConvertAsync(item, generalSetting, contentSourceInfoes, timeZoneId, isDaylight, timeFormat));
            
            result.Continuation = explorerQueryResult.Continuation;
            result.Items = items;
            return result;
        }

        public static async Task<ManualApprovalPaginateResult> CosmosDBFolderViewQueryAsync(ManualApprovalQueryDefinition queryDefinition, string timeZoneId, bool isDaylight)
        {
            Expression<Func<ManualApprovalRecord, bool>> folderPredicate = null;
            var repository = Repository;
            var sorterDefinitions = BuildCosmosDBSorter(queryDefinition);
            var queryItemDefinitions = await BuildFolderViewQueryDefinition(queryDefinition.FromGControl, queryDefinition.IsJpmc);
            var queryExpresions = await BuildCosmosDBFilterAsync(queryItemDefinitions);
            if (queryDefinition.IsEnableFolderView)
            {
                if (!queryDefinition.FolderInfos.Any())
                {
                    Expression<Func<ManualApprovalRecord, bool>> listPredicates = root => root.ManualSiteUrl == queryDefinition.ManualSiteUrl && root.NodeType == (int)NodeType.List
                    && root.IsManualSynced && root.ManualArchiveStatus != (int)ActionStatus.Archiverd && root.RecordStatus != (int)RMRecordStatus.Hidden && root.RecordStatus != (int)RMRecordStatus.RMDeleted;
                    queryExpresions = [listPredicates];
                    sorterDefinitions = [
                        new ManualApprovalExplorerOrderDefinition() { IsDesc = false, OrderKeySelector = root => root.ManualCollectionTime },
                    ];
                }
                else
                { 
                    var folderInfo = queryDefinition.FolderInfos.Last();
                    Expression<Func<ManualApprovalRecord, bool>> parentPredicates = root => root.ParentId == new Guid(folderInfo.NodeId);
                    queryExpresions.Add(parentPredicates);
                    folderPredicate = root => root.ManualSiteUrl == queryDefinition.ManualSiteUrl && root.NodeType == (int)NodeType.Folder && root.ParentId == new Guid(folderInfo.NodeId)
                    && root.IsManualSynced && root.ManualArchiveStatus != (int)ActionStatus.Archiverd && root.RecordStatus != (int)RMRecordStatus.Hidden && root.RecordStatus != (int)RMRecordStatus.RMDeleted;
                    sorterDefinitions = [
                        new ManualApprovalExplorerOrderDefinition() { IsDesc = false, OrderKeySelector = root => root.NodeType },
                    ];
                }
            }

            var folderViewFilters = await BuildCosmosDBFilterAsync(queryDefinition.Filters);
            var explorerQueryDefinition = new ManualApprovalExplorerQueryDefinition
            {
                PageSize = queryDefinition.PageSize,
                Continuation = queryDefinition.Continuation,
                Predicates = queryExpresions,
                OrderDefinitions = sorterDefinitions
            };

            var result = new ManualApprovalPaginateResult();

            if (queryDefinition.NeedCalculationCount)
            {
                var count = await repository.CountFolderViewAsync(explorerQueryDefinition, folderPredicate, folderViewFilters);
                result.Count = count;
            }

            var explorerQueryResult = await repository.QueryFolderViewItemsWithPaginationAsync(explorerQueryDefinition, folderPredicate, folderViewFilters);

            var contentSourceInfoes = await Cache.TryGetAsync(IRMCache.Keys.ManualApprovalQuerier_GetAllSimpleInfoes, async () =>
            {
                return (await CustomizeConnectorContentSourceDao.GetAllSimpleInfoes(CustomizeConnectorOrigin.BuildIn, CustomizeConnectorOrigin.ExternalCustomize)).ToDictionary(item => item.Flag, item => I18NEntity.GetString(item.Name));
            });

            var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();
            var items = await explorerQueryResult.Items.ConvertAllAsync(item => ConvertAsync(item, generalSetting, contentSourceInfoes, timeZoneId, isDaylight, queryDefinition.TimeFormat));

            result.Continuation = explorerQueryResult.Continuation;
            result.Items = items;
            return result;
        }

        public static async Task<PaginateQueryManualApprovalExplorerResult> CosmosDBFolderViewQueryAsync(ManualApprovalQueryDefinition queryDefinition, ManualApprovalRecordRepository repository)
        {
            Expression<Func<ManualApprovalRecord, bool>> folderPredicate = null;
            var sorterDefinitions = BuildCosmosDBSorter(queryDefinition);
            var queryItemDefinitions = await BuildFolderViewQueryDefinition();
            var queryExpresions = await BuildCosmosDBFilterAsync(queryItemDefinitions);
            if (queryDefinition.IsEnableFolderView)
            {
                if (!queryDefinition.FolderInfos.Any())
                {
                    Expression<Func<ManualApprovalRecord, bool>> listPredicates = root => root.ManualSiteUrl == queryDefinition.ManualSiteUrl && root.NodeType == (int)NodeType.List;
                    queryExpresions = [listPredicates];
                    sorterDefinitions = [
                        new ManualApprovalExplorerOrderDefinition() { IsDesc = false, OrderKeySelector = root => root.ManualCollectionTime },
                    ];
                }
                else
                {
                    var folderInfo = queryDefinition.FolderInfos.Last();
                    Expression<Func<ManualApprovalRecord, bool>> parentPredicates = root => root.ParentId == new Guid(folderInfo.NodeId);
                    queryExpresions.Add(parentPredicates);
                    folderPredicate = root => root.ManualSiteUrl == queryDefinition.ManualSiteUrl && root.NodeType == (int)NodeType.Folder && root.ParentId == new Guid(folderInfo.NodeId);
                    sorterDefinitions = [
                        new ManualApprovalExplorerOrderDefinition() { IsDesc = false, OrderKeySelector = root => root.NodeType },
                    ];
                }
            }

            var folderViewFilters = await BuildCosmosDBFilterAsync(queryDefinition.Filters);
            var explorerQueryDefinition = new ManualApprovalExplorerQueryDefinition
            {
                PageSize = queryDefinition.PageSize,
                Continuation = queryDefinition.Continuation,
                Predicates = queryExpresions,
                OrderDefinitions = sorterDefinitions
            };

            var explorerQueryResult = await repository.QueryFolderViewItemsWithPaginationAsync(explorerQueryDefinition, folderPredicate, folderViewFilters);
            return explorerQueryResult;
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

        public static async Task<int> CountAsync(ManualApprovalQueryDefinition queryDefinition)
        {
            var repository = Repository;
            await PrePermissionValidateAsync(queryDefinition);
            var filterExpresions = await BuildCosmosDBFilterAsync(queryDefinition.Filters);
            var explorerQueryDefinition = new ManualApprovalExplorerQueryDefinition
            {
                PageSize = queryDefinition.PageSize,
                Continuation = queryDefinition.Continuation,
                Predicates = filterExpresions,
            };
            var count = await repository.CountAsync(explorerQueryDefinition);
            return count;
        }     

        private static async Task<List<ManualApprovalFilterDefinition>> BuildFolderViewQueryDefinition(bool fromGControl = false, bool isJPMCMyhub = false)
        {
            var queryItemDefinitions = new List<ManualApprovalFilterDefinition>();
            if(!isJPMCMyhub)
            {
                queryItemDefinitions.Add(new()
                {
                    FilterOption = fromGControl ? ManualApprovalFilterOptions.GControlApprovalStatus : ManualApprovalFilterOptions.ApprovalStatus,
                    Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.WaitingApprove })
                });
                queryItemDefinitions.Add(new()
                {
                    FilterOption = ManualApprovalFilterOptions.ExtendTime,
                    Value = "false"
                });
            }

            if (string.IsNullOrEmpty(TenantLocalValue.LogonUserId))
            {
                return queryItemDefinitions;
            }
            var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(Contract.RoleAssignments.RMPermissionMasks.ManualReviewAdmin);
            if (isAdmin)
            {
                return queryItemDefinitions;
            }

            if (fromGControl)
            {
                if (TenantLocalValue.RequesterType == RequesterTypeEnum.OpusControlPlus)
                {
                    return queryItemDefinitions;
                }
                var googleReviewerFilter = new ManualApprovalFilterDefinition()
                {
                    FilterOption = ManualApprovalFilterOptions.GControlReviewer,
                    Value = "[]"
                };
                queryItemDefinitions.Add(googleReviewerFilter);
                var googleUserHasPermissionIntIds = UserService.GetUserWithRemovedAndGroupIds(TenantLocalValue.LogonUserId);
                googleReviewerFilter.Value = JsonConvert.SerializeObject(googleUserHasPermissionIntIds);
                return queryItemDefinitions;
            }

            var reviewerFilter = new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.Reviewer,
                Value = "[]"
            };
            queryItemDefinitions.Add(reviewerFilter);

            var userHasPermissionIntIds = UserService.GetUserWithRemovedAndGroupIds(TenantLocalValue.LogonUserId);
            reviewerFilter.Value = JsonConvert.SerializeObject(userHasPermissionIntIds);
            return queryItemDefinitions;
        }

        private static async System.Threading.Tasks.Task PrePermissionValidateAsync(ManualApprovalQueryDefinition queryDefinition)
        {
            if (string.IsNullOrEmpty(TenantLocalValue.LogonUserId))
            {
                return;
            }
            if (queryDefinition.FromGControl)
            {
                if (TenantLocalValue.RequesterType == RequesterTypeEnum.OpusControlPlus)
                {
                    return;
                }
                var googleReviewerFilter = new ManualApprovalFilterDefinition()
                {
                    FilterOption = ManualApprovalFilterOptions.GControlReviewer,
                    Value = "[]"
                };
                queryDefinition.Filters.Add(googleReviewerFilter);
                var googleUserHasPermissionIntIds = UserService.GetUserWithRemovedAndGroupIds(TenantLocalValue.LogonUserId);
                googleReviewerFilter.Value = JsonConvert.SerializeObject(googleUserHasPermissionIntIds);
                return;
            }
            var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(Contract.RoleAssignments.RMPermissionMasks.ManualReviewAdmin);
            if (isAdmin)
            {
                return;
            }

            var reviewerFilter = new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.Reviewer,
                Value = "[]"
            };
            queryDefinition.Filters.Add(reviewerFilter);

            var userHasPermissionIntIds = UserService.GetUserWithRemovedAndGroupIds(TenantLocalValue.LogonUserId);
            reviewerFilter.Value = JsonConvert.SerializeObject(userHasPermissionIntIds);
        }

        private static async Task<List<Expression<Func<ManualApprovalRecord, bool>>>> BuildCosmosDBFilterAsync(List<ManualApprovalFilterDefinition> filters)
        {
            var result = new List<Expression<Func<ManualApprovalRecord, bool>>>();
            foreach (var filterDefinition in filters)
            {
                var filterOption = filterDefinition.FilterOption;
                var filter = FilterCollection[filterOption];
                if (!string.IsNullOrEmpty(filterDefinition.CustomColumnId))
                {
                    var customFilter = filter as ICustomFilter;
                    var customExpression = await customFilter.GetCosmosDBFilterExpressionAsync(filterDefinition.CustomColumnId, filterDefinition.Value);
                    result.Add(customExpression);
                    continue;
                }
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
                if (!string.IsNullOrEmpty(queryDefinition.CustomColumnId))
                {
                    var customSorter = sorter as ICustomSorter;
                    var customExpression = customSorter.GetCosmosDBOrderExpression(queryDefinition.CustomColumnId);
                    result.Add(new ManualApprovalExplorerOrderDefinition
                    {
                        OrderKeySelector = customExpression,
                        IsDesc = queryDefinition.IsDesc
                    });
                    return result;
                }

                var expression = sorter.GetCosmosDBOrderExpression();
                result.Add(new ManualApprovalExplorerOrderDefinition
                {
                    OrderKeySelector = expression,
                    IsDesc = queryDefinition.IsDesc
                });
            }

            #region 

            //if (queryDefinition.OrderBy == ManualApprovalOrderOptions.None)
            //{
            //    var approvalStatusSorter = SorterCollection[ManualApprovalOrderOptions.ApprovalStatus];
            //    var approvalStatusExpression = approvalStatusSorter.GetCosmosDBOrderExpression();
            //    result.Add(new ManualApprovalExplorerOrderDefinition
            //    {
            //        OrderKeySelector = approvalStatusExpression,
            //        IsDesc = false,
            //    });
            //}

            //if (queryDefinition.OrderBy != ManualApprovalOrderOptions.CollectioinTime)
            //{
            //    var collectionTimeSorter = SorterCollection[ManualApprovalOrderOptions.CollectioinTime];
            //    var collectionTimeExpression = collectionTimeSorter.GetCosmosDBOrderExpression();
            //    result.Add(new ManualApprovalExplorerOrderDefinition
            //    {
            //        OrderKeySelector = collectionTimeExpression,
            //        IsDesc = true
            //    });
            //}

            #endregion

            if (queryDefinition.OrderBy == ManualApprovalOrderOptions.None)
            {
                var collectionTimeSorter = SorterCollection[ManualApprovalOrderOptions.CollectioinTime];
                var collectionTimeExpression = collectionTimeSorter.GetCosmosDBOrderExpression();
                result.Add(new ManualApprovalExplorerOrderDefinition
                {
                    OrderKeySelector = collectionTimeExpression,
                    IsDesc = true
                });
            }

            return result;
        }

        private async static Task<ManualApprovalItem> ConvertAsync(ManualApprovalRecord record, GeneralSettingModel gls, Dictionary<int, string> contentSourceInfoes, string timeZoneId, bool isDaylight, string timeFormat)
        {

            async Task<List<string>> GetUsersDisplayNames(int[] userIntIds, string aadId)
            {
                if (record.IsGControlRecord && aadId.IsNotNullOrEmpty())
                {
                    return await GetUserDisplayNameAsyncForGControl(aadId, record.GControlManualReviewers);
                }
                if(record.IsFsControlRecordJPMC && userIntIds.IsNotNullOrEmpty())
                {
                    return await GetUsersDisplayAsyncForFsControl(userIntIds);
                }
                if (userIntIds == null || userIntIds.Length == 0)
                {
                    return new List<string>();
                }
                var users = await AccountDao.GetUserWithRemovedByIds(userIntIds.ToHashSet().ToList());
                users = users.DistinctBy(item => item.UserPrincipalName).ToList();
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
                return user?.DisplayName ?? string.Empty;
            }

            async Task<List<string>> GetUserDisplayNameAsyncForGControl(string aadId, int[] reviewerIds)
            {
                var userName = (await AccountDao.GetUserByAADIdAsync(aadId))?.DisplayName ?? string.Empty;
                List<string> displayNames = [];
                if (reviewerIds.IsNotNullOrEmpty())
                {
                    var users = await AccountDao.GetUserWithRemovedByIds(reviewerIds.ToHashSet().ToList());
                    users = users.DistinctBy(item => item.UserPrincipalName).ToList();
                    displayNames = users.ConvertAll(item => item.DisplayName);
                }
                return userName.IsNotNullOrEmpty() ? [..displayNames, userName] : displayNames;
            }

            async Task<List<string>> GetUsersDisplayAsyncForFsControl(int[] userIntIds)
            {
                if (userIntIds == null || userIntIds.Length == 0)
                {
                    return new List<string>();
                }
                var users = await AccountDao.GetUserByIdsAsync(userIntIds.ToHashSet().ToList());
                users = users.DistinctBy(item => item.UserPrincipalName).ToList();
                var displayNames = users.ConvertAll(item => item.DisplayName);
                return displayNames;
            }

            async Task<Dictionary<string, CustomColumn>> HandleCustomColumns(Dictionary<string, CustomColumn> customColumns, GeneralSettingModel gls)
            {
                if(customColumns != null && customColumns.Any())
                {
                    foreach (var customColumn in customColumns.ToList())
                    {
                        var value = customColumn.Value;
                        if (value?.Date is DateTime date && date > DateTime.MinValue)
                        {
                            var dateUtc = await GeneralSettingService.ConvertDateTimeToUtcAsync(date);
                            Logger.Info($"Convert custom column date time, source date time: {dateUtc.ISOFormat()}");
                            value.Date = GeneralSettingService.ConvertTiksToDateTime(gls, date.Ticks, true).DataTime;
                            customColumns[customColumn.Key] = value;
                        }
                    }
                }
                return customColumns;
            }

            List<int> sourceFlagClassification = new List<int>() { (int)SourceFlag.OneDrive, (int)SourceFlag.Google, (int)SourceFlag.Exchange };
            bool IsEnableClassificationByOpus(string containerId, int sourceFlag)
            {
                if (!Guid.TryParse(containerId, out var guidContainerId)) return true;
                if (!sourceFlagClassification.Contains(sourceFlag)) return true;
                return sourceFlag switch
                {
                    (int)SourceFlag.OneDrive => IsODEnableClassification(guidContainerId),
                    (int)SourceFlag.Exchange => IsEXOEnableClassification(guidContainerId),
                    (int)SourceFlag.Google => IsGoogleEnableClassification(guidContainerId),
                };
            }

            bool IsGoogleEnableClassification(Guid containerId)
            {
                var setting = RMGoogleSettingDao.GetSettingInfoByScope(containerId, containerId, Guid.Empty);
                return !(setting?.IsNullClassificationSetting ?? false);
            }

            bool IsEXOEnableClassification(Guid containerId)
            {
                var setting = EXOSettingDao.GetSettingInfoByScope(containerId, Guid.Empty, containerId);
                return !(setting?.IsNullClassificationSetting ?? false);
            }

            bool IsODEnableClassification(Guid containerId)
            {
                var setting = OneDriveSettingDao.GetSettingInfoByScope(containerId, Guid.Empty, containerId);
                return !(setting?.IsNullClassificationSetting ?? false);
            }

            return new ManualApprovalItem
            {
                Id = record.Id,
                RecordsId = record.RecordsId,
                SourceFlag = record.SourceFlag,
                SourceName = contentSourceInfoes.ContainsKey(record.SourceFlag) ? contentSourceInfoes[record.SourceFlag] : I18NEntity.GetString("RM_CP_Connector"),
                SourceIcon = BuildInContentSourceI18Ns.SourceFlagIcons.ContainsKey((SourceFlag)record.SourceFlag) ? BuildInContentSourceI18Ns.SourceFlagIcons[(SourceFlag)record.SourceFlag] : "fia-connecter",
                NodeType = record.NodeType,
                LeafName = record.LeafName,
                FileExtension = I18NEntity.GetString(record.ExtensionForFile),
                NodeId = record.NodeId,
                RuleId = record.RuleId,
                RuleName = record.ManualRuleName,
                RuleCriteria = record.ManualRuleCriteria,
                RuleDisposalClass = record.ManualRuleDisposalClass,
                ReviewerDisplayNames = await GetUsersDisplayNames(record.ManualReviewer, record.GControlCurrentApproverId),
                EscalateFromDisplayName = await GetUserDisplayNameAsync(record.ManualEscalateFrom),
                FullPath = record.ExtensionForFile == "RM_RDM_RecordDetails_DataType_SPItem" ? WebUtil.GetListItemRealPath(record.ManualFullPath) : record.ManualFullPath,
                FullPathRealLocation = GetFullPathRealLocation(record),
                FolderPath = record.ManualFolderPath,
                ApprovedByDisplayName = await GetUserDisplayNameAsync(record.ManualApprovedBy),
                ApprovedStatus = record.IsGControlRecord ? record.GControlManualApprovedStatus : record.ManualApprovedStatus,
                InternalApprovedStatus = record.IsGControlRecord ? record.GControlManualInternalApprovedStatus : record.ManualInternalApprovedStatus,
                EscalatedComment = record.ManualEscalatedComment,
                ExtendTime = record.ManualExtendTime > 0
                    ? (string.IsNullOrEmpty(timeZoneId)
                        ? GeneralSettingService.ConvertTiksToDateTime(gls, record.ManualExtendTime, true).SimplifyFormatTime
                        : GeneralSettingService.ConvertTiksToDateTime(gls, record.ManualExtendTime, true, Convert.ToInt32(timeZoneId), isDaylight, timeFormat).SimplifyFormatTime)
                    : "0",
                ExtendTicks = record.ManualExtendTime > 0 ? record.ManualExtendTime : 0,
                ExtendComment = record.ManualExtendComment,
                CreatedBy = record.CreatedBy,
                ModifiedBy = record.ModifiedBy,
                ModifiedTime = record.ManualModifiedTime > 0
                ? (string.IsNullOrEmpty(timeZoneId)
                        ? GeneralSettingService.ConvertTiksToDateTime(gls, record.ManualModifiedTime, true).SimplifyFormatTime
                        : GeneralSettingService.ConvertTiksToDateTime(gls, record.ManualModifiedTime, true, Convert.ToInt32(timeZoneId), isDaylight, timeFormat).SimplifyFormatTime)
                        : string.Empty,
                ModifiedTicks = record.ManualModifiedTime > 0 ? record.ManualModifiedTime : 0,
                CollectionTime = string.IsNullOrEmpty(timeZoneId)
                    ? GeneralSettingService.ConvertTiksToDateTime(gls, record.ManualCollectionTime, true).SimplifyFormatTime
                    : GeneralSettingService.ConvertTiksToDateTime(gls, record.ManualCollectionTime, true, Convert.ToInt32(timeZoneId), isDaylight, timeFormat).SimplifyFormatTime,
                CollectionDateTime = string.IsNullOrEmpty(timeZoneId)
                    ? GeneralSettingService.ConvertTiksToDateTime(gls, record.ManualCollectionTime, true).DataTime
                    : GeneralSettingService.ConvertTiksToDateTime(gls, record.ManualCollectionTime, true, Convert.ToInt32(timeZoneId), isDaylight, timeFormat).DataTime,
                CollectionTicks = record.ManualCollectionTime > 0 ? record.ManualCollectionTime : 0,
                IsRelatedRecords = record.ManualIsRelatedRecords,
                RelatedRecordsAction = record.ManualRelatedRecordsAction,
                ExtendCount = record.ManualExtendCount,
                EmailNotificationCount = record.ManualEmailNotificationCount,
                EmailNotificationLastTime = record.ManualEmailNotificationLastTime,
                NeedEmailNotification = record.ManualNeedEmailNotification,
                RetentionStatus = record.ManualRetentionStatus,
                RelatedRecords = string.IsNullOrEmpty(record.ManualRelatedRecords) ?
                                  new List<ReportRelatedRecords>() :
                                  SerializerHelper.DeserializeFromXmlString<List<ReportRelatedRecords>>(record.ManualRelatedRecords),
                ManualAudit = SerializerHelper.SerializeByJsonSerializer(await GetManualReviewInfoAsync(record, timeZoneId, isDaylight, timeFormat)),
                ManualApprovalComment = record.ManualApprovalComment,
                QuickReason = record.QuickReason,
                SiteUrl = record.ManualSiteUrl,
                SiteUrlId = record.ScopeId.ToString(),
                ManualLastReasonForRejection = record.ManualLastReasonForRejection ?? string.Empty,
                ManualLastExtendType = record.ManualLastExtendType,
                ManualLastCustomeExtendDate = record.ManualLastCustomeExtendDate,
                ManualLastApproveRejectComment = record.ManualLastApproveRejectComment,
                ManualLastReviewedBy = record.ManualLastReviewedBy,
                ManuaLastlReviewTime = record.ManualLastlReviewTime > 0
                ? (string.IsNullOrEmpty(timeZoneId)
                        ? GeneralSettingService.ConvertTiksToDateTime(gls, record.ManualLastlReviewTime, true).SimplifyFormatTime
                        : GeneralSettingService.ConvertTiksToDateTime(gls, record.ManualLastlReviewTime, true, Convert.ToInt32(timeZoneId), isDaylight, timeFormat).SimplifyFormatTime)
                        : string.Empty,
                ManualLastReviewTicks = record.ManualLastlReviewTime > 0 ? record.ManualLastlReviewTime : 0,
                TermName = record.TermName,
                TermId = record.TermId.ToString(),
                CustomColumnDic = await HandleCustomColumns(record.CustomColumnDic, gls),
                ManualDisposalDueDate = record.ManualDisposalDueDate > 0
                        ? GeneralSettingService.ConvertTiksToDateTime(gls, record.ManualDisposalDueDate, true).SimplifyFormatTime
                        : string.Empty,
                WebViewLink = record.WebViewLink,
                EnableClassificationByOpus = IsEnableClassificationByOpus(record.ContainerId, record.SourceFlag),
                ManualApprovedStatus = record.ManualApprovedStatus
            };
        }

        private static async Task<ManualReviewInfo> GetManualReviewInfoAsync(Record info, string timeZoneId, bool isDaylight, string timeFormat)
        {
            ManualReviewInfo mri = new ManualReviewInfo
            {
                ReviewAudits = new List<ReviewAudits>()
            };

            try
            {
                if (!string.IsNullOrEmpty(info.ManualAudits))
                {
                    mri.ReviewAudits = SerializerHelper.DeserializeFromXmlString<List<ReviewAudits>>(info.ManualAudits);
                }

                var reviewerIntIds = info.ManualReviewer ?? new int[0];

                var accounts = await AccountDao.GetUserWithRemovedByIds(reviewerIntIds.ToList());

                var displayNames = accounts.Select(item => item.DisplayName);

                if (info.IsGControlRecord)
                {  
                    var gControlReviewerName = (await AccountDao.GetUserByAADIdAsync(info.GControlCurrentApproverId))?.DisplayName ?? "";
                    var gControlReviewerIds =  info.GControlManualReviewers ?? new int[0];
                    var delegateReviewerNames = (await AccountDao.GetUserWithRemovedByIds(gControlReviewerIds.ToList())).Select(item => item.DisplayName);

                    displayNames = [..delegateReviewerNames,gControlReviewerName];
                }

                mri.RecordOwner = string.Join("; ", displayNames);

                if (mri.ReviewAudits != null && mri.ReviewAudits.Count > 0)
                {
                    mri.ReviewAudits = mri.ReviewAudits.OrderByDescending(a => long.Parse(a.ReviewTime)).ToList();
                    GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
                    foreach (var item in mri.ReviewAudits)
                    {
                        if (!string.IsNullOrEmpty(item.ReviewTime))
                        {
                            item.ReviewTimeTicks = long.Parse(item.ReviewTime) > 0 ? long.Parse(item.ReviewTime) : 0;
                            item.ReviewTime = string.IsNullOrEmpty(timeZoneId)
                                ? GeneralSettingService.ConvertTiksToDateTime(gls, long.Parse(item.ReviewTime), true).SimplifyFormatTime
                                : GeneralSettingService.ConvertTiksToDateTime(gls, long.Parse(item.ReviewTime), true, Convert.ToInt32(timeZoneId), isDaylight, timeFormat).SimplifyFormatTime;
                        }
                        switch (item.Action)
                        {
                            case "RM_JS_MA_ApproveStatus_Approved":
                                item.Action = "RM_MA_Approve";
                                break;
                            case "RM_JS_MA_ApproveStatus_Rejected":
                                item.Action = "RM_MA_Reject";
                                break;
                            case "RM_JS_MA_ApproveStatus_Extend":
                                item.Action = "RM_MA_Extend";
                                break;
                            case "RM_JS_MA_ApproveStatus_Escalated":
                                item.Action = "RM_MA_Escalate";
                                break;
                            case "RM_JS_MA_ApproveStatus_Reassigned":
                                item.Action = "RM_MA_Reassign";
                                break;
                        }
                        item.Action = I18NEntity.GetString(item.Action);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while GetManualReviewInfo, ERROR:{0}", ex.ToString());
            }
            return mri;
        }

        private static string GetFullPathRealLocation(Record item)
        {
            if(item.ExtensionForFile == "RM_RDM_RecordDetails_DataType_SPItem" && item.ManualRetentionStatus == 0)
            {
                return WebUtil.GetListItemRealPath(item.ManualFullPath);
            }
            if((item.SourceFlag == (int)SourceFlag.SharePoint || item.SourceFlag == (int)SourceFlag.OneDrive) && item.ManualRetentionStatus == 0)
            {
                var fileExtentionsConfig = TenantService.GetFileExtentionsConfig();
                try
                {
                    if (item.ManualFullPath.Contains("Root/PRM/RecordsExplorer", StringComparison.CurrentCulture) || fileExtentionsConfig == null)
                    {
                        return item.ManualFullPath;
                    }

                    if (item.ManualFullPath.IndexOf(".") > -1 && fileExtentionsConfig.EnableExclusion)
                    {
                        var fileExtension = item.ManualFullPath[(item.ManualFullPath.LastIndexOf(".") + 1)..];
                        var isNeedExclude = fileExtentionsConfig.FileExtensions.Contains(fileExtension.ToLowerInvariant());
                        return isNeedExclude ? item.ManualFullPath : $"{item.ManualFullPath}?web = 1";
                    }                    
                }
                catch (Exception e)
                {
                    Logger.Error($"Get record full path real location failed error : {e}.");
                    return item.ManualFullPath;
                }
            }

            return "";
        }
    }
}
