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
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao.ArchivedFullTextIndex;
using AvePoint.RA.DB.Dao.ArchivedFullTextIndex.Impl;
using Cloud.Sdk.Data.EDiscovery;
using Cloud.Sdk.EDiscovery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvanceSearchCondition = DocAveOnline.WebApi.Contracts.AdvanceSearchCondition;
using AdvanceSearchResult = DocAveOnline.WebApi.Contracts.AdvanceSearchResult;
using SearchResult = DocAveOnline.WebApi.Contracts.SearchResult;
using AvePoint.Media.Service.ArchiverBackup;

namespace AvePoint.RA.Service.Services.ArchivedFullTextIndex.Query;

public class RMArchivedFullTextIndexReCenterQuerierV1
{
    private readonly RALogger _logger = RALogger.GetInstance(typeof(RMArchivedFullTextIndexReCenterQuerierV1));

    private readonly IGeneralSettingService _generalSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

    private readonly IRMArchivedFullTextIndexDao _archivedFullTextIndexDao = new RMArchivedFullTextIndexDao();

    private readonly EDiscoveryApiClient _apiClient;

    private readonly AdvanceSearchCondition _searchCondition;

    public RMArchivedFullTextIndexReCenterQuerierV1(AdvanceSearchCondition searchCondition)
    {
        _apiClient = AosApiUtility.GetEDiscoveryApiClient();
        _searchCondition = searchCondition;
    }

    public async Task<SearchResult> QueryAsync(string positionJson, int pageSize)
    {
        SearchResult res = new SearchResult();
        try
        {
            var gls = await _generalSettingService.GetGeneralSettingAsync();
            List<AdvanceSearchResult> datas = new List<AdvanceSearchResult>();
            Position position = string.IsNullOrWhiteSpace(positionJson) ? null : JsonConvert.DeserializeObject<Position>(positionJson);

            do
            {
                var currentPageSize = pageSize - datas.Count;
                var searchInfo = await BuildSearchInfo(position, currentPageSize);
                var searchResult = await _apiClient.IndexService.SearchAsync(searchInfo);
                if (searchResult == null)
                {
                    _logger.Warn($"(Recenter)query e-discovery index data failed. Paramter: [{JsonConvert.SerializeObject(_searchCondition)}] Position: [{positionJson}] PageSize: [{pageSize}].");
                    break;
                }
                else if (!searchResult.Successful)
                {
                    _logger.Warn($"(Recenter)query e-discovery index data failed. Error code: [{searchResult.ErrorCode}].");
                    break;
                }

                foreach (var item in searchResult.Result)
                {
                    var fullPath = item["fullPath"].ToString();
                    if (item.TryGetValue("friendlyFullPath", out object value))
                    {
                        fullPath = value.ToString();
                    }
                    datas.Add(new()
                    {
                        Name = item["name"].ToString(),
                        FullPath = fullPath,
                        AbsolutePath = fullPath,
                        PathMD5 = item["pathMd5"].ToString(),
                        ModifiedBy = item["editor"].ToString(),
                        CreateTime = Convert.ToInt64(item["createdTime"].ToString()),
                        ModifiedTime = Convert.ToInt64(item["modifiedTime"].ToString()),
                        ArchiveTime = Convert.ToInt64(item["archiverTime"].ToString()),
                        ContentLenth = Convert.ToInt64(item["fileSize"].ToString()),
                        IsArchiveTier = item.TryGetValue("accessTierType", out object accessTierType) && Convert.ToInt64(accessTierType) == (int)Storage.AccessTierType.Archive,
                    });
                }
                position = (searchResult.Result == null || searchResult.Result.Count == 0) ? null : searchResult.Position;
            } while (datas.Count < pageSize && position != null);

            res.ContinuationToken = JsonConvert.SerializeObject(position);
            res.HasNext = position != null && datas.Count == pageSize;
            res.AdvanceSearchResults = datas;
            return res;
        }
        catch (Exception e)
        {
            _logger.Error($"(Recenter)An error occurred while query data in full text index. Paramter: [{JsonConvert.SerializeObject(_searchCondition)}] Position: [{positionJson}] PageSize: [{pageSize}]. Error: {e}");
            return new();
        }
    }

    private async Task<SearchInfo> BuildSearchInfo(Position position, int pageSize)
    {
        var siteUrl = NormalizeSiteUrlOriginal(_searchCondition.SiteUrl);
        var siteUrls = string.IsNullOrWhiteSpace(siteUrl) ? [] : new List<string> { siteUrl };

        var basicFieldQuerys = new List<FieldQuery>
            {
                new FieldQuery()
                {
                    Field = new Field
                    {
                        Name = "siteUrlMd5",
                        Value = _searchCondition.SiteUrl.ToLower().ToMD5HashCode(),
                        FieldType = FieldType.String
                    },
                    Operator = FilterOperator.And
                }
            };

        if (_searchCondition.CreatedDateFrom != 0 && _searchCondition.CreatedDateTo != 0)
        {
            basicFieldQuerys.Add(new FieldRangeQuery()
            {
                Min = _searchCondition.CreatedDateFrom,
                Max = _searchCondition.CreatedDateTo,
                MinInclusive = true,
                MaxInclusive = true,
                Field = new Field
                {
                    Name = "createdTime",
                    FieldType = FieldType.Long
                },
                Operator = FilterOperator.And
            });
        }

        var startTicks = _searchCondition.ArchivedDateFrom;
        var endTicks = _searchCondition.ArchivedDateTo;
        (startTicks, endTicks) = await RMArchivedFullTextIndexQueryHelper.GetClampedArchivedTimeRangeV1Async(
            siteUrls,
            startTicks,
            endTicks);
        basicFieldQuerys.Add(new FieldRangeQuery()
        {
            Min = startTicks,
            Max = endTicks,
            MinInclusive = true,
            MaxInclusive = true,
            Field = new Field
            {
                Name = "archiverTime",
                FieldType = FieldType.Long | FieldType.NeedIndex
            },
            Operator = FilterOperator.And
        });

        var nameFieldQuerys = GetFiledQuerys("name", _searchCondition.Name, FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore);
        var contentFieldQuerys = GetFiledQuerys("content", _searchCondition.Content, FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore);
        var metadataFieldQuerys = GetFiledQuerys("metadataInfo", _searchCondition.MetadataInfo, FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore);
        var createdByFieldQuerys = GetFiledQuerys("author", _searchCondition.CreatedBy?.ToLower(), FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore);
        var modifiedByFieldQuerys = GetFiledQuerys("editor", _searchCondition.ModifiedBy?.ToLower(), FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore);
        var folderPathFieldQuerys = GetFiledQuerys("fullPath", _searchCondition.FolderNameOrPath, FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore);
        var keywordsFieldQuerys = GetKeywordsFieldQuerys();

        var queryGroups = new List<QueryGroup>() {
                new()
                {
                    QueryFields = basicFieldQuerys,
                    Operator = FilterOperator.And
                }
            };

        var queryCollections = new List<List<FieldQuery>>
            {
                nameFieldQuerys,
                contentFieldQuerys,
                metadataFieldQuerys,
                createdByFieldQuerys,
                modifiedByFieldQuerys,
                keywordsFieldQuerys,
                folderPathFieldQuerys
            };

        foreach (var queryCollection in queryCollections)
        {
            if (queryCollection.Count > 0)
            {
                queryGroups.Add(new QueryGroup { QueryFields = queryCollection, Operator = FilterOperator.And });
            }
        }

        return new()
        {
            Category = "RestoreFullTextIndex",
            Filter = queryGroups,
            MaxCount = pageSize,
            Position = position,
            DocSeperator = new()
            {
                Field = new Field
                {
                    Name = "archiverTime",
                    FieldType = FieldType.Long | FieldType.NeedIndex
                },
                Type = SeparatorType.Month
            }
        };
    }

    private static string NormalizeSiteUrlOriginal(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        return url.Trim();
    }

    private List<FieldQuery> GetFiledQuerys(
        string name,
        string value,
        FieldType fieldType)
    {
        var res = new List<FieldQuery>();
        if (string.IsNullOrWhiteSpace(value))
        {
            return res;
        }
        var searchValue = value.Trim();
        res.Add(new()
        {
            Field = new Field
            {
                Name = name,
                Value = $"{searchValue}",
                FieldType = fieldType
            },
            Operator = FilterOperator.Or
        });

        if (searchValue.IndexOf(' ') < 0)
        {
            var likeSearchValue = searchValue;
            likeSearchValue = "*" + likeSearchValue;
            likeSearchValue += "*";
            res.Add(new()
            {
                Field = new Field
                {
                    Name = name,
                    Value = $"{likeSearchValue}",
                    FieldType = fieldType
                },
                Operator = FilterOperator.Or
            });
        }

        return res;
    }

    private List<FieldQuery> GetKeywordsFieldQuerys()
    {
        if (string.IsNullOrWhiteSpace(_searchCondition.Keyword))
        {
            return [];
        }
        var searchValue = _searchCondition.Keyword.Trim();


        var res = new List<FieldQuery>()
            {
                new()
                    {
                        Field = new Field
                        {
                            Name = "name",
                            Value = searchValue,
                            FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                        },
                        Operator = FilterOperator.Or,
                    },
                    new()
                    {
                        Field = new Field
                        {
                            Name = "content",
                            Value = searchValue,
                            FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                        },
                        Operator = FilterOperator.Or,
                    },
                    new()
                    {
                        Field = new Field
                        {
                            Name = "metadataInfo",
                            Value = searchValue,
                            FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                        },
                        Operator = FilterOperator.Or,
                    },
                    new()
                    {
                        Field = new Field
                        {
                            Name = "fullPath",
                            Value = searchValue,
                            FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                        },
                        Operator = FilterOperator.Or
                    },
                    new()
                    {
                        Field = new Field
                        {
                            Name = "author",
                            Value = searchValue.ToLower(),
                            FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                        },
                        Operator = FilterOperator.Or,
                    },
                    new()
                    {
                        Field = new Field
                        {
                            Name = "editor",
                            Value = searchValue.ToLower(),
                            FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                        },
                        Operator = FilterOperator.Or,
                    }
            };
        if (searchValue.IndexOf(' ') < 0)
        {
            var likeSearchValue = searchValue;
            likeSearchValue = "*" + likeSearchValue;
            likeSearchValue += "*";
            res.Add(new()
            {
                Field = new Field
                {
                    Name = "name",
                    Value = likeSearchValue,
                    FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                },
                Operator = FilterOperator.Or,
            });
            res.Add(new()
            {
                Field = new Field
                {
                    Name = "content",
                    Value = likeSearchValue,
                    FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                },
                Operator = FilterOperator.Or,
            });
            res.Add(new()
            {
                Field = new Field
                {
                    Name = "metadataInfo",
                    Value = likeSearchValue,
                    FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                },
                Operator = FilterOperator.Or,
            });
            res.Add(new()
            {
                Field = new Field
                {
                    Name = "author",
                    Value = likeSearchValue.ToLower(),
                    FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                },
                Operator = FilterOperator.Or,
            });
            res.Add(new()
            {
                Field = new Field
                {
                    Name = "editor",
                    Value = likeSearchValue.ToLower(),
                    FieldType = FieldType.String | FieldType.NeedIndex | FieldType.NeedTokenize | FieldType.NeedStore
                },
                Operator = FilterOperator.Or,
            });
        }

        return res;
    }
}
