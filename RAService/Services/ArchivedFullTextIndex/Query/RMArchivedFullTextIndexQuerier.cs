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
using Aspose.Email.Tools.Search;
using Aspose.Pdf;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao.ArchivedFullTextIndex.Impl;
using AvePoint.RA.DB.Dao.ArchivedFullTextIndex;
using Cloud.Sdk.Data.EDiscovery;
using Cloud.Sdk.EDiscovery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Model.ArchivedFullTextIndex;
using ICSharpCode.SharpZipLib.Core;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.DB.Dao;
using AvePoint.Media.Service.ArchiverBackup;

namespace AvePoint.RA.Service.Services.ArchivedFullTextIndex.Query
{
    public class RMArchivedFullTextIndexQuerier
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMArchivedFullTextIndexQuerier));

        private readonly IGeneralSettingService _generalSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

        private readonly IRMArchivedFullTextIndexCategoryDao _categoryDao = new RMArchivedFullTextIndexCategoryDao();

        private readonly IRMRestoreSiteMappingDao _restoreSiteMappingDao = PlatformWindsorManager.GetService<IRMRestoreSiteMappingDao>();
        private readonly IArchiverSiteMasterIndexDao _archiverSiteMasterIndexDao = PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        private readonly IKeyValueService _keyValueService = PlatformWindsorManager.GetService<IKeyValueService>();

        private readonly EDiscoveryApiClient _apiClient;

        private readonly BackupDataSearchContract _parameter;

        public RMArchivedFullTextIndexQuerier(BackupDataSearchContract parameter)
        {
            _apiClient = AosApiUtility.GetEDiscoveryApiClient();
            _parameter = parameter;
        }

        public async Task<ArchiverRestoreResult> QueryAsync(string positionJson, int pageSize, int categoryId)
        {
            try
            {
                var res = new ArchiverRestoreResult();
                var needQuerySites = ResolveSearchableSiteUrls(_parameter.SearchNode.SiteUrl, out var isBlacklistMode);
                if (needQuerySites.Count == 0)
                {
                    return new()
                    {
                        RestoreSerchNodes = []
                    };
                }

                var gls = await _generalSettingService.GetGeneralSettingAsync();
                var dataList = new List<ArchiverRestoreSerchResult>();
                Position position = string.IsNullOrWhiteSpace(positionJson) ? null : JsonConvert.DeserializeObject<Position>(positionJson);

                var (hasNextCategory, categoryInfo) = categoryId != int.MaxValue ? await _categoryDao.TryGetByIdAsync(categoryId) : await TryGetNextCategoryInfoAsync(categoryId);
                if(!hasNextCategory)
                {
                    _logger.Info($"No category found with site [{_parameter.SearchNode.SiteUrl}] category [{categoryId}].");
                    return new()
                    {
                        RestoreSerchNodes = []
                    };
                }

                do
                {
                    var currentPageSize = pageSize - dataList.Count;
                    var searchInfo = BuildSearchInfo(position, currentPageSize, categoryInfo, needQuerySites, isBlacklistMode);
                    var searchResult = await _apiClient.IndexService.SearchAsync(searchInfo);
                    if (searchResult == null)
                    {
                        _logger.Warn($"query e-discovery index data failed. Paramter: [{JsonConvert.SerializeObject(_parameter)}] Position: [{positionJson}] PageSize: [{pageSize}].");
                        break;
                    }
                    else if (!searchResult.Successful)
                    {
                        _logger.Warn($"query e-discovery index data failed. Error code: [{searchResult.ErrorCode}].");
                        break;
                    }

                    foreach (var item in searchResult.Result)
                    {
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

                if(position == null && dataList.Count == pageSize)
                {
                    (hasNextCategory, categoryInfo) = await TryGetNextCategoryInfoAsync(categoryInfo.Id);
                    res.HasNext = hasNextCategory;
                }
                if(hasNextCategory)
                {
                    res.CategoryId = categoryInfo.Id;
                }

                while (res.HasNext && hasNextCategory)
                {
                    var searchResult = await _apiClient.IndexService.SearchAsync(BuildSearchInfo(position, 1, categoryInfo, needQuerySites, isBlacklistMode));
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
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query data in full text index. Paramter: [{JsonConvert.SerializeObject(_parameter)}] Position: [{positionJson}] PageSize: [{pageSize}]. Error: {e}");
                return new();
            }
        }

        private SearchInfo BuildSearchInfo(Position position, int pageSize, RMArchivedDataFullTextIndexCategory categoryInfo, List<string> needQuerySites, bool isBlacklistMode)
        {
            var fieldQuerys = new List<FieldQuery>();
            var nameFieldQuerys = new List<FieldQuery>();
            var contentFieldQuerys = new List<FieldQuery>();

            if (_parameter.FilterPolicy.Level == GCommon.Contract.CommonFilter.PolicyLevel.Document)
            {
                fieldQuerys.Add(new FieldQuery()
                {
                    Field = new Field
                    {
                        Name = "nodeLevel",
                        Value = ((int)_parameter.FilterPolicy.Level).ToString(),
                        FieldType = FieldType.Int | FieldType.NeedIndex
                    },
                    Operator = FilterOperator.And
                });
            }

            var filterPolicy = _parameter.FilterPolicy;
            if (!string.IsNullOrWhiteSpace(filterPolicy.CreateStartTime) && !string.IsNullOrWhiteSpace(filterPolicy.CreateEndTime))
            {
                fieldQuerys.Add(new FieldRangeQuery()
                {
                    Min = Convert.ToDateTime(filterPolicy.CreateStartTime).Ticks,
                    Max = Convert.ToDateTime(filterPolicy.CreateEndTime).Ticks,
                    MinInclusive = true,
                    MaxInclusive = true,
                    Field = new Field
                    {
                        Name = "createdTime",
                        FieldType = FieldType.Long | FieldType.NeedIndex
                    },
                    Operator = FilterOperator.And
                });
            }

            if (!string.IsNullOrWhiteSpace(filterPolicy.ModifiedStartTime) && !string.IsNullOrWhiteSpace(filterPolicy.ModifiedEndTime))
            {
                fieldQuerys.Add(new FieldRangeQuery()
                {
                    Min = Convert.ToDateTime(filterPolicy.ModifiedStartTime).Ticks,
                    Max = Convert.ToDateTime(filterPolicy.ModifiedEndTime).Ticks,
                    MinInclusive = true,
                    MaxInclusive = true,
                    Field = new Field
                    {
                        Name = "modifiedTime",
                        FieldType = FieldType.Long | FieldType.NeedIndex
                    },
                    Operator = FilterOperator.And
                });
            }

            if (!string.IsNullOrWhiteSpace(filterPolicy.ArchivedStartTime) && !string.IsNullOrWhiteSpace(filterPolicy.ArchivedEndTime))
            {
                fieldQuerys.Add(new FieldRangeQuery()
                {
                    Min = Convert.ToDateTime(filterPolicy.ArchivedStartTime).Ticks,
                    Max = Convert.ToDateTime(filterPolicy.ArchivedEndTime).Ticks,
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

            if (!string.IsNullOrWhiteSpace(filterPolicy.FilterName))
            {
                var searchValue = filterPolicy.FilterName;
                var LikeSearchValue = filterPolicy.FilterName.Trim();
                if (LikeSearchValue.IndexOf(' ') < 0)
                {
                    LikeSearchValue = "*" + LikeSearchValue;
                    LikeSearchValue += "*";
                }
                nameFieldQuerys.Add(new()
                {
                    Field = new Field
                    {
                        Name = "name",
                        Value = $"{searchValue}",
                        FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                    },
                    Operator = FilterOperator.Or
                });
                if (LikeSearchValue.StartsWith("*") && LikeSearchValue.EndsWith("*"))
                {
                    nameFieldQuerys.Add(new()
                    {
                        Field = new Field
                        {
                            Name = "name",
                            Value = $"{LikeSearchValue}",
                            FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                        },
                        Operator = FilterOperator.Or
                    });
                }
            }

            if (!string.IsNullOrWhiteSpace(filterPolicy.FilterContent))
            {
                var searchValue = filterPolicy.FilterContent.Trim();
                var LikeSearchValue = filterPolicy.FilterContent.Trim();
                if (LikeSearchValue.IndexOf(' ') < 0)
                {
                    LikeSearchValue = "*" + LikeSearchValue;
                    LikeSearchValue += "*";
                }

                contentFieldQuerys.Add(new()
                {
                    Field = new Field
                    {
                        Name = "content",
                        Value = $"{searchValue}",
                        FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                    },
                    Operator = FilterOperator.Or
                });
                if(LikeSearchValue.StartsWith("*") && LikeSearchValue.EndsWith("*"))
                {
                    contentFieldQuerys.Add(new()
                    {
                        Field = new Field
                        {
                            Name = "content",
                            Value = $"{LikeSearchValue}",
                            FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                        },
                        Operator = FilterOperator.Or
                    });
                }
            }

            if (!string.IsNullOrWhiteSpace(filterPolicy.FilterMetadataInfo))
            {
                var searchValue = filterPolicy.FilterMetadataInfo.Trim();
                var LikeSearchValue = filterPolicy.FilterMetadataInfo.Trim();
                if (LikeSearchValue.IndexOf(' ') < 0)
                {
                    LikeSearchValue = "*" + LikeSearchValue;
                    LikeSearchValue += "*";
                }

                contentFieldQuerys.Add(new()
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
                    contentFieldQuerys.Add(new()
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
            }

            var res = new SearchInfo()
            {
                Category = categoryInfo.Name,
                Filter = [],
                MaxCount = pageSize,
                Position = position,
            };

            if (fieldQuerys.Count > 0)
            {
                res.Filter.Add(new QueryGroup()
                {
                    QueryFields = fieldQuerys,
                    Operator = FilterOperator.And
                });
            }

            if (nameFieldQuerys.Count > 0)
            {
                res.Filter.Add(new QueryGroup()
                {
                    QueryFields = nameFieldQuerys,
                    Operator = FilterOperator.And
                });
            }

            if (contentFieldQuerys.Count > 0)
            {
                res.Filter.Add(new QueryGroup()
                {
                    QueryFields = contentFieldQuerys,
                    Operator = FilterOperator.And
                });
            }

            var siteFieldQuerys = new List<FieldQuery>();
            foreach (var siteUrl in needQuerySites)
            {
                if (!isBlacklistMode)
                {
                    _logger.Info($"Site in whitelist: {siteUrl}");
                }
                siteFieldQuerys.Add(new FieldQuery()
                {
                    Field = new Field
                    {
                        Name = "siteUrlMd5",
                        Value = siteUrl.ToLower().ToMD5HashCode(),
                        FieldType = FieldType.String
                    },
                    Operator = FilterOperator.Or
                });
            }
            if (siteFieldQuerys.Count > 0)
            {
                res.Filter.Add(new QueryGroup()
                {
                    QueryFields = siteFieldQuerys,
                    Operator = FilterOperator.And
                });
            }
            return res;
        }

        private List<string> ResolveSearchableSiteUrls(string requestedSiteUrl, out bool isBlacklistMode)
        {
            isBlacklistMode = _keyValueService.IsSCBlackListForEdiscovery();

            if (!string.IsNullOrWhiteSpace(requestedSiteUrl))
            {
                var normalizedRequested = NormalizeSiteUrlOriginal(requestedSiteUrl);
                if (isBlacklistMode)
                {
                    if (_restoreSiteMappingDao.ExistBlacklistInSiteUrls([normalizedRequested]))
                    {
                        _logger.Info($"Requested site [{requestedSiteUrl}] is in blacklist. Skip query.");
                        return [];
                    }
                }
                else if (!_restoreSiteMappingDao.ExistWhitelistInSiteUrls([normalizedRequested]))
                {
                    _logger.Info($"Requested site [{requestedSiteUrl}] is not in whitelist. Skip query.");
                    return [];
                }

                return [normalizedRequested];
            }

            if (!isBlacklistMode)
            {
                var whitelist = _restoreSiteMappingDao.GetAllWhitelist()
                    .Select(w => NormalizeSiteUrlOriginal(w.SourceSiteUrl))
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                _logger.Info($"Whitelist mode site count: {whitelist.Count}.");
                return whitelist;
            }

            var blacklist = _restoreSiteMappingDao.GetAllBlacklist()
                .Select(b => NormalizeSiteUrl(b.SourceSiteUrl))
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var allSites = _archiverSiteMasterIndexDao.GetAllBackupSiteCollectionDistinctUrl()
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var allowedSites = allSites
                .Where(url => !blacklist.Contains(NormalizeSiteUrl(url)))
                .Select(NormalizeSiteUrlOriginal)
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            _logger.Info($"Blacklist count: {blacklist.Count}. Allowed site count: {allowedSites.Count}.");

            return allowedSites;
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
            var startMonth = string.IsNullOrWhiteSpace(_parameter.FilterPolicy.ArchivedStartTime) ? 0 : int.Parse(Convert.ToDateTime(_parameter.FilterPolicy.ArchivedStartTime).ToString("yyyyMM"));
            var endMonth = string.IsNullOrWhiteSpace(_parameter.FilterPolicy.ArchivedEndTime) ? int.MaxValue : int.Parse(Convert.ToDateTime(_parameter.FilterPolicy.ArchivedEndTime).ToString("yyyyMM"));
            if (string.IsNullOrWhiteSpace(_parameter.SearchNode.SiteUrl))
            {
                return await _categoryDao.TryGetNextAvaliableCategoryAsync(categoryId, startMonth, endMonth);
            }
            return await _categoryDao.TryGetSiteNextAvaliableCategoryAsync(_parameter.SearchNode.SiteUrl, categoryId, startMonth, endMonth);
        }
    }
}