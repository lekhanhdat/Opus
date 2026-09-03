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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.ArchivedFullTextIndex;
using AvePoint.RA.DB.Dao.ArchivedFullTextIndex.Impl;
using AvePoint.RA.DB.Model.ArchivedFullTextIndex;
using AvePoint.RA.Service.Services.Common;
using Cloud.Sdk.Data.EDiscovery;
using Cloud.Sdk.EDiscovery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ArchivedFullTextIndex.Query
{
    public class RMArchivedFullTextIndexSimpleQuerier
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMArchivedFullTextIndexSimpleQuerier));

        private readonly IGeneralSettingService _generalSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

        private readonly IRMArchivedFullTextIndexCategoryDao _categoryDao = new RMArchivedFullTextIndexCategoryDao();

        private readonly IRMRestoreSiteMappingDao _restoreSiteMappingDao = PlatformWindsorManager.GetService<IRMRestoreSiteMappingDao>();

        private readonly IKeyValueService _keyValueService = PlatformWindsorManager.GetService<IKeyValueService>();

        private readonly EDiscoveryApiClient _apiClient;

        private readonly ArchiverRestoreSimpleSearchQueryParameter _parameter;

        public RMArchivedFullTextIndexSimpleQuerier(ArchiverRestoreSimpleSearchQueryParameter parameter)
        {
            _apiClient = AosApiUtility.GetEDiscoveryApiClient();
            _parameter = parameter;
        }

        public async Task<ArchiverRestoreResult> QueryAsync()
        {
            ArchiverRestoreResult res = new ArchiverRestoreResult() { RestoreSerchNodes = [] };
            try
            {
                SiteFilterContext siteFilter = ResolveSiteFilter();
                _logger.Info($"Full text index {(siteFilter.IsBlacklistMode ? "blacklist" : "whitelist")} mode; site count={siteFilter.Sites.Count}.");
                if (!siteFilter.IsBlacklistMode && siteFilter.Sites.Count == 0)
                {
                    return res;
                }
                GeneralSettingModel gls = await _generalSettingService.GetGeneralSettingAsync();
                Position position = string.IsNullOrWhiteSpace(_parameter.ContinuationToken) ? null : JsonConvert.DeserializeObject<Position>(_parameter.ContinuationToken);
                int pageSize = _parameter.PageSize;
                int categoryId = _parameter.CategoryId;
                do
                {
                    ArchiverRestoreResult tempRes = await RealQueryAsync(siteFilter, gls, position, pageSize, categoryId);
                    MergeResult(res, tempRes);
                    position = string.IsNullOrWhiteSpace(tempRes.ContinuationToken) ? null : JsonConvert.DeserializeObject<Position>(tempRes.ContinuationToken);
                    categoryId = tempRes.CategoryId;
                } while (res.RestoreSerchNodes.Count() < pageSize && res.HasNext);
                return res;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while simple query data in full text index. Paramter: [{JsonConvert.SerializeObject(_parameter)}]. Error: {e}");
                return new();
            }
        }

        private void MergeResult(ArchiverRestoreResult target, ArchiverRestoreResult source)
        {
            target.ContinuationToken = source.ContinuationToken;
            target.HasNext = source.HasNext;
            target.CategoryId = source.CategoryId;
            target.PageSize = source.PageSize;
            target.Failed = source.Failed;
            if(target.RestoreSerchNodes == null)
            {
                target.RestoreSerchNodes = [];
            }
            target.RestoreSerchNodes.AddRange(source?.RestoreSerchNodes ?? []);
            target.OrderBy = source.OrderBy;
            target.Message = source.Message;
            target.OpenIndexDbTimeoutInMs = source.OpenIndexDbTimeoutInMs;
            target.SearchMode = source.SearchMode;
            target.archiverRestoreSimpleSearchQueryParameter = source.archiverRestoreSimpleSearchQueryParameter;
            target.IsDesc = source.IsDesc;
            target.SearchValue = source.SearchValue;
        }

        private async Task<ArchiverRestoreResult> RealQueryAsync(SiteFilterContext siteFilter, GeneralSettingModel gls, Position position, int pageSize, int categoryId)
        {
            var res = new ArchiverRestoreResult();
            var dataList = new List<ArchiverRestoreSerchResult>();
            var (hasNextCategory, categoryInfo) = categoryId != int.MaxValue ? await _categoryDao.TryGetByIdAsync(categoryId) : await TryGetNextCategoryInfoAsync(categoryId);
            if (!hasNextCategory)
            {
                _logger.Info($"Simple search no category found category [{categoryId}].");
                return new()
                {
                    RestoreSerchNodes = []
                };
            }
            bool forceFilterSiteCollectionInMemory = _keyValueService.ForceFilterSiteCollectionInMemory();
            int forceFilterInMemoryPageSize = _keyValueService.ForceFilterInMemoryPageSize();
            do
            {
                int currentPageSize = pageSize - dataList.Count;
                SearchInfo searchInfo = null;
                SearchResult searchResult = null;

                if (!forceFilterSiteCollectionInMemory)
                {
                    searchInfo = BuildSearchInfo(position, currentPageSize, categoryInfo, siteFilter);
                    searchResult = await _apiClient.IndexService.SearchAsync(searchInfo);
                }
                if((searchResult != null && !searchResult.Successful && siteFilter?.Sites != null && siteFilter.Sites.Any())
                    || forceFilterSiteCollectionInMemory)
                {
                    forceFilterSiteCollectionInMemory = true;
                    currentPageSize = forceFilterInMemoryPageSize;
                    _logger.Info($"full text index forceFilterSiteCollectionInMemory. Error code: [{searchResult?.ErrorCode}], filterCount :{siteFilter.Sites.Count()}, will retry search by not filter and {currentPageSize} page size.");
                    searchInfo = BuildSearchInfo(position, currentPageSize, categoryInfo, null);
                    searchResult = await _apiClient.IndexService.SearchAsync(searchInfo);
                }

                if (searchResult == null)
                {
                    _logger.Warn($"Simple query e-discovery index data failed. Paramter: [{JsonConvert.SerializeObject(_parameter)}].");
                    break;
                }
                else if (!searchResult.Successful)
                {
                    _logger.Warn($"Simple query e-discovery index data failed. Error code: [{searchResult.ErrorCode}].");
                    break;
                }


                foreach (var item in searchResult.Result)
                {
                    if (siteFilter?.Sites != null && siteFilter.Sites.Any())
                    {
                        var siteUrl = NormalizeSiteUrl(item["siteUrl"].ToString());
                        bool existInFulter = siteFilter.Sites.Any(site => formatSiteUrl(site).Equals(formatSiteUrl(siteUrl)));
                        if(existInFulter == siteFilter.IsBlacklistMode)
                        {
                            continue;
                        }
                    }

                    dataList.Add(new()
                    {
                        ObjectName = item["name"].ToString(),
                        FullPath = item["fullPath"].ToString(),
                        Location = item["fullPath"].ToString(),
                        ParentPathMd5 = item["parentPathMd5"].ToString(),
                        PathMd5 = item["pathMd5"].ToString(),
                        ModifiedBy = item["editor"].ToString(),
                        TreeNode = item["treeNode"].ToString(),
                        ArchivedTime = _generalSettingService.ConvertTiksToDateTime(gls, Convert.ToInt64(item["archiverTime"].ToString()), true).SimplifyFormatTime,
                        CreatedDate = _generalSettingService.ConvertTiksToDateTime(gls, Convert.ToInt64(item["createdTime"].ToString()), true).SimplifyFormatTime,
                        LastModifiedTime = _generalSettingService.ConvertTiksToDateTime(gls, Convert.ToInt64(item["modifiedTime"].ToString()), true).SimplifyFormatTime,
                        CreatedDateTicks = Convert.ToInt64(item["archiverTime"].ToString()).ToString(),
                        SitePath = item["siteUrl"].ToString(),
                        ContentLenth = Convert.ToInt64(item["fileSize"].ToString()),
                    });
                }
                position = (searchResult.Result == null || searchResult.Result.Count == 0) ? null : searchResult.Position;
                if ((dataList.Count < pageSize && position == null) || currentPageSize < searchResult.Result.Count)
                {
                    (hasNextCategory, categoryInfo) = await TryGetNextCategoryInfoAsync(categoryInfo.Id);
                }
            } while (dataList.Count < pageSize && (position != null || hasNextCategory));

            res.ContinuationToken = JsonConvert.SerializeObject(position);
            res.HasNext = position != null && dataList.Count == pageSize;
            res.RestoreSerchNodes = dataList;

            if (position == null && dataList.Count == pageSize)
            {
                (hasNextCategory, categoryInfo) = await TryGetNextCategoryInfoAsync(categoryInfo.Id);
                res.HasNext = hasNextCategory;
            }
            if (hasNextCategory)
            {
                res.CategoryId = categoryInfo.Id;
            }

            while (res.HasNext && hasNextCategory)
            {
                var searchResult = await _apiClient.IndexService.SearchAsync(BuildSearchInfo(position, 1, categoryInfo, siteFilter));
                position = (searchResult.Result == null || searchResult.Result.Count == 0) ? null : searchResult.Position;
                if (searchResult.Result.Count < 1 && position == null)
                {
                    (hasNextCategory, categoryInfo) = await TryGetNextCategoryInfoAsync(categoryInfo.Id);
                    res.HasNext = hasNextCategory;
                }
                else
                {
                    res.HasNext = dataList.Count > 0;
                    break;
                }
            }

            return res;
        }

        private string formatSiteUrl(string siteUrl)
        {
            if (string.IsNullOrWhiteSpace(siteUrl))
            {
                return "";
            }
            while (siteUrl.EndsWith("/") || siteUrl.EndsWith("\\") || siteUrl.EndsWith(" "))
            {
                siteUrl = siteUrl.Substring(0, siteUrl.Length - 1);
            }
            return siteUrl.ToLowerInvariant();
        }


        private SearchInfo BuildSearchInfo(Position position, int pageSize, RMArchivedDataFullTextIndexCategory categoryInfo, SiteFilterContext siteFilter)
        {
            var basicFieldQuerys = new List<FieldQuery>();
            var fieldQuerys = new List<FieldQuery>();

            basicFieldQuerys.Add(new FieldQuery()
            {
                Field = new Field
                {
                    Name = "nodeLevel",
                    Value = 64.ToString(),
                    FieldType = FieldType.Int | FieldType.NeedIndex
                },
                Operator = FilterOperator.And
            });

            if (!string.IsNullOrWhiteSpace(_parameter.ArchivedStartTime) && !string.IsNullOrWhiteSpace(_parameter.ArchivedEndTime))
            {
                basicFieldQuerys.Add(new FieldRangeQuery()
                {
                    Min = Convert.ToDateTime(_parameter.ArchivedStartTime).Ticks,
                    Max = Convert.ToDateTime(_parameter.ArchivedEndTime).Ticks,
                    MinInclusive = true,
                    MaxInclusive = true,
                    Field = new Field
                    {
                        Name = "archiverTime",
                        FieldType = FieldType.Long | FieldType.NeedIndex
                    },
                    Operator = FilterOperator.And
                });
            }

            var searchValue = _parameter.Keyword.Trim();
            var LikeSearchValue = _parameter.Keyword.Trim();
            if (LikeSearchValue.IndexOf(' ') < 0)
            {
                LikeSearchValue = "*" + LikeSearchValue;
                LikeSearchValue += "*";
            }

            fieldQuerys.Add(new()
            {
                Field = new Field
                {
                    Name = "name",
                    Value = $"{searchValue}",
                    FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                },
                Operator = FilterOperator.Or
            });

            fieldQuerys.Add(new()
            {
                Field = new Field
                {
                    Name = "content",
                    Value = $"{searchValue}",
                    FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                },
                Operator = FilterOperator.Or
            });

            fieldQuerys.Add(new()
            {
                Field = new Field
                {
                    Name = "metadataInfo",
                    Value = $"{searchValue}",
                    FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                },
                Operator = FilterOperator.Or
            });

            if (LikeSearchValue.StartsWith("*") && LikeSearchValue.EndsWith("*"))
            {
                fieldQuerys.Add(new()
                {
                    Field = new Field
                    {
                        Name = "name",
                        Value = $"{LikeSearchValue}",
                        FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                    },
                    Operator = FilterOperator.Or
                });

                fieldQuerys.Add(new()
                {
                    Field = new Field
                    {
                        Name = "content",
                        Value = $"{LikeSearchValue}",
                        FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                    },
                    Operator = FilterOperator.Or
                });

                fieldQuerys.Add(new()
                {
                    Field = new Field
                    {
                        Name = "metadataInfo",
                        Value = $"{LikeSearchValue}",
                        FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                    },
                    Operator = FilterOperator.Or
                });
            }

            var filterGroups = new List<QueryGroup>
            {
                new QueryGroup
                {
                    QueryFields = basicFieldQuerys,
                    Operator = FilterOperator.And
                },
                new QueryGroup
                {
                    QueryFields = fieldQuerys,
                    Operator = FilterOperator.And
                }
            };            

            if (siteFilter?.Sites != null && siteFilter.Sites.Count > 0)
            {
                _logger.Info($"Simple search site is blackList mode: {siteFilter.IsBlacklistMode}, ");
                List<FieldQuery> siteFilterQueries = [];
                foreach (var sc in siteFilter.Sites)
                {
                    siteFilterQueries.Add(new FieldQuery()
                    {
                        Field = new Field
                        {
                            Name = "siteUrl",
                            Value = sc,
                            FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                        },
                        Operator = FilterOperator.Or
                    });
                }
                filterGroups.Add(new QueryGroup
                {
                    QueryFields = siteFilterQueries,
                    Operator = siteFilter.IsBlacklistMode ? FilterOperator.Or : FilterOperator.And
                });
            }

            return new()
            {
                Category = categoryInfo.Name,
                Filter = filterGroups,
                MaxCount = pageSize,
                Position = position,
            };
        }

        private SiteFilterContext ResolveSiteFilter()
        {
            var context = new SiteFilterContext
            {
                IsBlacklistMode = _keyValueService.IsSCBlackListForEdiscovery()
            };

            if (!context.IsBlacklistMode)
            {
                var whitelist = _restoreSiteMappingDao.GetAllWhitelist()
                    .Select(w => NormalizeSiteUrlOriginal(w.SourceSiteUrl))
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .GroupBy(NormalizeSiteUrl)
                    .Select(g => g.First())
                    .ToList();

                context.Sites.AddRange(whitelist);
                return context;
            }

            var blacklist = _restoreSiteMappingDao.GetAllBlacklist()
                .Select(b => NormalizeSiteUrlOriginal(b.SourceSiteUrl))
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .GroupBy(NormalizeSiteUrl)
                .Select(g => g.First())
                .ToList();

            context.Sites.AddRange(blacklist);
            return context;
        }

        private static string NormalizeSiteUrl(string url)
        {
            return string.IsNullOrWhiteSpace(url)
                ? string.Empty
                : url.Trim().TrimEnd('/').ToLowerInvariant();
        }

        private static string NormalizeSiteUrlOriginal(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }

            return url.Trim();
        }

        private async Task<(bool has, RMArchivedDataFullTextIndexCategory categoryInfo)> TryGetNextCategoryInfoAsync(int categoryId)
        {
            var startMonth = string.IsNullOrWhiteSpace(_parameter.ArchivedStartTime) ? 0 : int.Parse(Convert.ToDateTime(_parameter.ArchivedStartTime).ToString("yyyyMM"));
            var endMonth = string.IsNullOrWhiteSpace(_parameter.ArchivedEndTime) ? int.MaxValue : int.Parse(Convert.ToDateTime(_parameter.ArchivedEndTime).ToString("yyyyMM"));
            return await _categoryDao.TryGetNextAvaliableCategoryAsync(categoryId, startMonth, endMonth);
        }

        private class SiteFilterContext
        {
            public bool IsBlacklistMode { get; set; }

            public List<string> Sites { get; } = [];
        }
    }
}
