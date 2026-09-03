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
 *  Copyright c 2017-2026 AvePoint Inc. All Rights Reserved.
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using AngleSharp.Common;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cloud;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Discovery.Model.Rule.Condition;
using AvePoint.RA.Contract.Discovery.Model.Rule.Criteria;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Records.Core.Utilities.Extensions;
using Cloud.Sdk.Data.Dal;
using Cloud.Sdk.LAL.PlatformSS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using GCommonRoleConfiguration = AvePoint.GCommon.Utility.Cloud.GCommonRoleConfiguration;
using TenantLocalValue = AvePoint.RA.Contract.Tenant.TenantLocalValue;

namespace AvePoint.RA.Service.Services.DalServices
{
    public class DALDataOptimizationService
    {
        private const int DuplicateObjectIdBatchSize = 100;
        private const string MicrosoftItemQueryUri = "api/v1/microsoft-items";
        private const string MicrosoftItemSelect = "DalObjectId,ObjectIntId,CapturedTime,CloudTenantId,SourceSystem,IsDeleted,WorkspaceId,ItemId,CreatedTime,LastModifiedTime,CreatedBy,LastModifiedBy,ExtendedAttributes,SiteId,ItemType,Name,Path,Extension,Size";

        private readonly RALogger _logger = RALogger.GetInstance(typeof(DALDataOptimizationService));
        private readonly ICloudSdkLALPlatformSSClientFactory _lalPlatformSSClientFactory;
        private readonly LALPlatformSSDGApiClient _dalClient;
        private readonly LALPlatformSSDGApiClient _dalQueryClient;
        private readonly RMDiscoveryOptimizeDataSettingDto _dataOptimizeSettingDto;
        private readonly string _office365TenantId;
        private readonly bool _checkVersionRule;
        private readonly SourceFlag _sourceFlag;

        private List<RMDiscoveryOffice365FileExtension> _fileExtensions;
        private bool _needDoQuery;
        private string _dataQueryFilters = string.Empty;

        private readonly IRMDiscoveryOffice365FileExtensionDao _FileExtensionDao = new RMDiscoveryOffice365FileExtensionDao();

        public DALDataOptimizationService(RMDiscoveryOptimizeDataSettingDto setting, bool checkVersionRule, SourceFlag sourceFlag, bool isProcessDuplicateDatas)
        {
            _lalPlatformSSClientFactory = AosApiUtility.LALPlatformSSClientFactory;
            _dataOptimizeSettingDto = setting;
            _office365TenantId = setting.O365TenantId;
            _checkVersionRule = checkVersionRule;
            _sourceFlag = sourceFlag;

            var customerTenantId = !string.IsNullOrWhiteSpace(TenantLocalValue.LogonGroupId)
                ? TenantLocalValue.LogonGroupId
                : _office365TenantId;
            _dalClient = TenantClient(customerTenantId);
            _dalQueryClient = DalQueryClient(customerTenantId);

            if (isProcessDuplicateDatas)
            {
                InitProcessDuplicateDatasSettingsAsync().GetAwaiter().GetResult();
            }
            else
            {
                InitSettingsAsync().GetAwaiter().GetResult();
            }

            _logger.Info($"DAL optimization setting is :{JsonConvert.SerializeObject(setting)}");
        }

        // itemUniqueId maps to ObjectId in DB
        public async Task<bool> TagAsArchivedAsync(string itemUniqueId)
        {
            if (string.IsNullOrWhiteSpace(itemUniqueId))
            {
                return false;
            }

            if (!long.TryParse(itemUniqueId, out var itemId))
            {
                _logger.Warn($"[{nameof(TagAsArchivedAsync)}] DAL item id is invalid:{itemUniqueId}.");
                return false;
            }

            var odataUrl = BuildMicrosoftItemQuery($"ItemId eq {itemId}", 1);
            var dataJson = await GetByODataUrlWithRetryAsync(odataUrl, _office365TenantId);

            var rootObj = JsonConvert.DeserializeObject<ExpandoObject>(dataJson);
            var valueList = rootObj?.TryGet("data") as List<object>;
            if (valueList == null || valueList.Count == 0)
            {
                _logger.Info($"[{nameof(TagAsArchivedAsync)}] No DAL item found for ObjectId:{itemUniqueId}.");
                return false;
            }

            var record = valueList.FirstOrDefault() as ExpandoObject;
            if (record == null)
            {
                _logger.Info($"[{nameof(TagAsArchivedAsync)}] DAL item payload invalid for ObjectId:{itemUniqueId}.");
                return false;
            }

            var dict = (IDictionary<string, object>)record;
            dict[RMDiscoveryBuildInRule.ARCHVIED_COLUMN_NAME] = 1;

            var archivedTagColumn = $"tags_{RMDiscoveryBuildInRule.ARCHVIED_UNIQUE_ID.ToString().ToLowerInvariant().Replace("-", string.Empty)}";
            dict[archivedTagColumn] = 1;

            var ingestModel = new IngestionModel
            {
                IngestionDataType = IngestionDataType.MicrosoftItem,
                Data = new List<object> { record }
            };

            var result = await _dalClient.IngestionService.IngestAsync(ingestModel);
            var succeeded = result?.Summary?.Succeeded ?? 0;
            var failed = result?.Summary?.Failed ?? 0;

            if (succeeded > 0 && failed == 0)
            {
                return true;
            }


            _logger.Error($"[{nameof(TagAsArchivedAsync)}] DAL ingest failed for ObjectId:{itemUniqueId}. Succeeded:{succeeded}, Failed:{failed}.");

            return false;
        }

        public async Task<IEnumerable<string>> GetAllWebsAsync(string siteId)
        {
            return Enumerable.Empty<string>();
        }

        public async Task<IEnumerable<string>> GetAllListsAsync(string siteId, string webId)
        {
            return Enumerable.Empty<string>();
        }

        public async IAsyncEnumerable<List<RMDiscoveryFileDataInfo>> GetAllFilesAsync(string siteId, string webId, string listId, List<string> tagRuleIds = null, int pageSize = 1000)
        {
            if (_needDoQuery)
            {
                string pageToken = null;
                do
                {
                    var filter = $"SiteId eq '{siteId}'{_dataQueryFilters}";
                    var odataUrl = BuildMicrosoftItemQuery(filter, pageSize, "ItemId", pageToken);

                    string dataJson;
                    try
                    {
                        _logger.Info($"Query DAL files. SiteId:{siteId}, WebId:{webId}, ListId:{listId}, HasPageToken:{!string.IsNullOrEmpty(pageToken)}, Query:{odataUrl}.");
                        dataJson = await GetByODataUrlWithRetryAsync(odataUrl, _office365TenantId);
                    }
                    catch
                    {
                        _logger.Error($"Get AllFiles failed: {odataUrl}");
                        throw;
                    }

                    var results = JsonConvert.DeserializeObject<ExpandoObject>(dataJson);
                    var dataList = results.TryGet("data") as List<object>;
                    pageToken = results.TryGet("nextPageToken")?.ToString();
                    var hasMore = results.TryGet<bool>("hasMore") ?? false;
                    _logger.Info($"DAL files query returned {dataList?.Count ?? 0} records. SiteId:{siteId}, WebId:{webId}, ListId:{listId}.");
                    List<RMDiscoveryFileDataInfo> files = new List<RMDiscoveryFileDataInfo>();
                    if (dataList != null && dataList.Count > 0)
                    {
                        foreach (var item in dataList)
                        {
                            var dataObj = item as ExpandoObject;
                            var fileData = new RMDiscoveryFileDataInfo
                            {
                                Id = GetValue(dataObj, "dalObjectId"),
                                Name = GetValue(dataObj, "name"),
                                SiteUrl = GetValue(dataObj, "path"),
                                FullUrl = GetValue(dataObj, "path"),
                                SiteId = GetValue(dataObj, "siteId"),
                                ItemId = (int)GetValue<long>(dataObj, "objectIntId"),
                                ItemUniqueId = GetValue(dataObj, "itemId"),
                                FileExtension = GetValue(dataObj, "extension"),
                                FileSize = GetValue<long>(dataObj, "size"),
                                CreatedTime = GetValue<DateTime>(dataObj, "createdTime"),
                                ModifiedTime = GetValue<DateTime>(dataObj, "lastModifiedTime"),
                            };

                            if (_checkVersionRule)
                            {
                                var versionObjs = dataObj.TryGet("Versions") as List<object>;
                                if (versionObjs != null && versionObjs.Count > 0)
                                {
                                    fileData.Versions = new List<RMDiscoveryFileVersionDataInfo>();
                                    foreach (var versionObj in versionObjs)
                                    {
                                        var versionData = versionObj as ExpandoObject;
                                        fileData.Versions.Add(new RMDiscoveryFileVersionDataInfo
                                        {
                                            Version = GetValue(versionData, "Version"),
                                            VersionSize = GetValue<long>(versionData, "VersionSize"),
                                            CreatedTime = GetValue<DateTime>(versionData, "CreatedTime"),
                                            ModifiedTime = GetValue<DateTime>(versionData, "ModifiedTime"),
                                        });
                                    }
                                }
                            }

                            if (tagRuleIds != null && tagRuleIds.Any())
                            {
                                fileData.Tags = [];
                                foreach (var tagId in tagRuleIds)
                                {
                                    fileData.Tags[tagId] = dataObj.TryGet(tagId) ?? 1;
                                }
                            }

                            files.Add(fileData);
                        }
                    }

                    yield return files;

                    if (!hasMore || string.IsNullOrEmpty(pageToken))
                    {
                        break;
                    }
                } while (true);
            }
            else
            {
                yield return new List<RMDiscoveryFileDataInfo>();
            }
        }

        public async IAsyncEnumerable<List<RMDiscoveryFileDataInfo>> GetAllDuplicateFilesAsync(string siteId, string webId, string listId, List<string> ObjectIds, int pageSize = 1000)
        {
            if (!_needDoQuery || ObjectIds == null || ObjectIds.Count == 0)
            {
                yield return new List<RMDiscoveryFileDataInfo>();
                yield break;
            }

            yield return new List<RMDiscoveryFileDataInfo>();
            yield break;
        }

        private LALPlatformSSDGApiClient TenantClient(string tenantId)
        {
            var gatewayUrl = GCommonRoleConfiguration.DAL_GATEWAY_API_URL;

            var client = _lalPlatformSSClientFactory.CreateLALPlatformSSDGClient(gatewayUrl, tenantId);
            _logger.Info($"Created DAL ingestion client. BaseUrl:{gatewayUrl}, TenantId:{tenantId}.");
            return client;
        }

        private LALPlatformSSDGApiClient DalQueryClient(string tenantId)
        {
            var client = _lalPlatformSSClientFactory.CreateLALPlatformSSDGClient(GCommonRoleConfiguration.DAL_GATEWAY_API_URL, tenantId);
            _logger.Info($"Created DAL query client. BaseUrl:{GCommonRoleConfiguration.DAL_GATEWAY_API_URL}, TenantId:{tenantId}.");
            return client;
        }

        private static string NormalizeBaseUrl(string url)
        {
            var normalizedUrl = url?.Trim();
            return string.IsNullOrEmpty(normalizedUrl) || normalizedUrl.EndsWith("/", StringComparison.Ordinal)
                ? normalizedUrl
                : normalizedUrl + "/";
        }

        private static string BuildMicrosoftItemQuery(string filter, int top, string orderBy = null, string pageToken = null)
        {
            var query = $"{MicrosoftItemQueryUri}?$select={Uri.EscapeDataString(MicrosoftItemSelect)}&$filter={Uri.EscapeDataString(filter)}&$top={top}";
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                query += $"&$orderby={Uri.EscapeDataString(orderBy)}";
            }
            if (!string.IsNullOrWhiteSpace(pageToken))
            {
                query += $"&$pageToken={Uri.EscapeDataString(pageToken)}";
            }
            return query;
        }

        private static string BuildMicrosoftItemAggregateQuery(string filter, string groupBy)
        {
            return $"{MicrosoftItemQueryUri}?$filter={Uri.EscapeDataString(filter)}"
                + $"&$groupby={Uri.EscapeDataString(groupBy)}"
                + $"&$aggregate={Uri.EscapeDataString("count() as Count")}";
        }

        private string BuildPlanRuleFilter(RMDiscoveryRuleDefinition rule)
        {
            if (rule?.CriteriaInfoes == null || rule.CriteriaInfoes.Count == 0)
            {
                return null;
            }

            var filters = rule.CriteriaInfoes
                .OrderBy(c => c.Order)
                .Select(BuildPlanCriterionFilter)
                .Where(filter => !string.IsNullOrWhiteSpace(filter))
                .ToList();

            return filters.Count == 0 ? null : $"({string.Join(" and ", filters)})";
        }

        private string BuildPlanCriterionFilter(RMDiscoveryRuleCriteriaInfo criterion)
        {
            if (criterion?.ConditionInfo == null)
            {
                return null;
            }

            if (criterion.CriteriaType == (int)RMDiscoveryDocumentCriteriaType.DocumentSize
                || criterion.CriteriaType == (int)RMDiscoveryVersionCriteriaType.DocumentSize)
            {
                return BuildSizeCriterionFilter(criterion);
            }

            if (criterion.CriteriaType == (int)RMDiscoveryDocumentCriteriaType.Name)
            {
                return BuildNameCriterionFilter(criterion);
            }

            if (criterion.CriteriaType == (int)RMDiscoveryDocumentCriteriaType.CreatedTime)
            {
                return BuildDateTimeCriterionFilter(criterion, "CreatedTime");
            }

            if (criterion.CriteriaType == (int)RMDiscoveryDocumentCriteriaType.ModifiedTime
                || criterion.CriteriaType == (int)RMDiscoveryVersionCriteriaType.ModifiedTime)
            {
                return BuildDateTimeCriterionFilter(criterion, "LastModifiedTime");
            }

            if (criterion.CriteriaType == (int)RMDiscoveryDocumentCriteriaType.DocumentType
                || criterion.CriteriaType == (int)RMDiscoveryVersionCriteriaType.DocumentType)
            {
                return BuildDocumentTypeCriterionFilter(criterion);
            }

            _logger.Warn($"Unsupported DAL Plan Profile criterion. CriteriaType:{criterion.CriteriaType}, Category:{criterion.ConditionInfo?.Category}.");
            return null;
        }

        private string BuildSizeCriterionFilter(RMDiscoveryRuleCriteriaInfo criterion)
        {
            if (string.IsNullOrWhiteSpace(criterion.ConditionInfo?.Value))
            {
                return null;
            }

            try
            {
                var value = JsonConvert.DeserializeObject<RMDiscoveryFileSizeConditionValue>(criterion.ConditionInfo.Value);
                if (value == null) return null;
                var sizeInBytes = ConvertFileSizeToBytes(value);
                var operation = ((RMDiscoveryFileSizeConditionType)criterion.ConditionInfo.Logic) switch
                {
                    RMDiscoveryFileSizeConditionType.LessThanEquals => "le",
                    RMDiscoveryFileSizeConditionType.GreaterThanEquals => "ge",
                    RMDiscoveryFileSizeConditionType.LessThan => "lt",
                    RMDiscoveryFileSizeConditionType.GreaterThan => "gt",
                    RMDiscoveryFileSizeConditionType.Equals => "eq",
                    _ => null,
                };

                return operation == null ? null : $"Size {operation} {sizeInBytes}";
            }
            catch (Exception ex)
            {
                _logger.Warn($"Failed to build Size criterion filter. Value:{criterion.ConditionInfo.Value}, error:{ex}");
                return null;
            }
        }

        private string BuildNameCriterionFilter(RMDiscoveryRuleCriteriaInfo criterion)
        {
            if (string.IsNullOrWhiteSpace(criterion.ConditionInfo?.Value))
            {
                return null;
            }

            try
            {
                if (criterion.ConditionInfo.Category == RMDiscoveryConditionCategory.Text)
                {
                    var textVal = EscapeODataString(criterion.ConditionInfo.Value);
                    return ((RMDiscoveryTextConditionType)criterion.ConditionInfo.Logic) switch
                    {
                        RMDiscoveryTextConditionType.Contains => $"contains(Name, '{textVal}')",
                        RMDiscoveryTextConditionType.DoesNotContain => $"not contains(Name, '{textVal}')",
                        RMDiscoveryTextConditionType.Equals => $"Name eq '{textVal}'",
                        RMDiscoveryTextConditionType.DoesNotEqual => $"Name ne '{textVal}'",
                        _ => $"contains(Name, '{textVal}')"
                    };
                }

                List<string> rawValues = null;
                var trimmed = criterion.ConditionInfo.Value.Trim();
                if (trimmed.StartsWith("["))
                {
                    rawValues = JsonConvert.DeserializeObject<List<string>>(trimmed);
                }
                else
                {
                    rawValues = new List<string> { trimmed };
                }

                if (rawValues == null || rawValues.Count == 0)
                {
                    return null;
                }

                var itemFilters = new List<string>();
                foreach (var raw in rawValues)
                {
                    if (string.IsNullOrWhiteSpace(raw)) continue;
                    var pat = raw.Trim();
                    if (pat.StartsWith("*") && pat.EndsWith("*") && pat.Length > 2)
                    {
                        var inner = EscapeODataString(pat.Substring(1, pat.Length - 2));
                        itemFilters.Add($"contains(Name, '{inner}')");
                    }
                    else if (pat.StartsWith("*") && pat.Length > 1)
                    {
                        var inner = EscapeODataString(pat.Substring(1));
                        itemFilters.Add($"endswith(Name, '{inner}')");
                    }
                    else if (pat.EndsWith("*") && pat.Length > 1)
                    {
                        var inner = EscapeODataString(pat.Substring(0, pat.Length - 1));
                        itemFilters.Add($"startswith(Name, '{inner}')");
                    }
                    else
                    {
                        var inner = EscapeODataString(pat);
                        if (criterion.ConditionInfo.Logic == (int)RMDiscoveryArrayConditionType.In ||
                            criterion.ConditionInfo.Logic == (int)RMDiscoveryArrayConditionType.NotIn)
                        {
                            itemFilters.Add($"Name eq '{inner}'");
                        }
                        else
                        {
                            itemFilters.Add($"contains(Name, '{inner}')");
                        }
                    }
                }

                if (itemFilters.Count == 0) return null;

                var combined = itemFilters.Count == 1 ? itemFilters[0] : $"({string.Join(" or ", itemFilters)})";

                if (criterion.ConditionInfo.Logic == (int)RMDiscoveryArrayConditionType.TextNotMatchIn ||
                    criterion.ConditionInfo.Logic == (int)RMDiscoveryArrayConditionType.NotIn)
                {
                    return $"not ({combined})";
                }

                return combined;
            }
            catch (Exception ex)
            {
                _logger.Warn($"Failed to build Name criterion filter. Value:{criterion.ConditionInfo.Value}, error:{ex}");
                return null;
            }
        }

        private string BuildDateTimeCriterionFilter(RMDiscoveryRuleCriteriaInfo criterion, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(criterion.ConditionInfo?.Value))
            {
                return null;
            }

            try
            {
                var logic = (RMDiscoveryDateTimeConditionType)criterion.ConditionInfo.Logic;
                if (logic == RMDiscoveryDateTimeConditionType.OlderThan)
                {
                    var olderThanInfo = JsonConvert.DeserializeObject<RMDiscoveryDateConditionOlderThanInfo>(criterion.ConditionInfo.Value);
                    if (olderThanInfo == null) return null;
                    var targetDate = olderThanInfo.UnitType switch
                    {
                        RMDiscoveryDateUnitType.Day => DateTime.UtcNow.AddDays(-olderThanInfo.Unit),
                        RMDiscoveryDateUnitType.Week => DateTime.UtcNow.AddDays(-olderThanInfo.Unit * 7),
                        RMDiscoveryDateUnitType.Month => DateTime.UtcNow.AddMonths(-olderThanInfo.Unit),
                        RMDiscoveryDateUnitType.Year => DateTime.UtcNow.AddYears(-olderThanInfo.Unit),
                        _ => DateTime.UtcNow.AddDays(-olderThanInfo.Unit)
                    };
                    return $"{fieldName} lt {targetDate:yyyy-MM-ddTHH:mm:ssZ}";
                }
                else if (logic == RMDiscoveryDateTimeConditionType.Before)
                {
                    if (DateTime.TryParse(criterion.ConditionInfo.Value, out var beforeDate))
                    {
                        return $"{fieldName} lt {beforeDate.ToUniversalTime():yyyy-MM-ddTHH:mm:ssZ}";
                    }
                }
                else if (logic == RMDiscoveryDateTimeConditionType.FromTo)
                {
                    var fromToInfo = JsonConvert.DeserializeObject<RMDiscoveryDateConditionFromToInfo>(criterion.ConditionInfo.Value);
                    if (fromToInfo != null && DateTime.TryParse(fromToInfo.Value1, out var fromDate) && DateTime.TryParse(fromToInfo.Value2, out var toDate))
                    {
                        return $"({fieldName} ge {fromDate.ToUniversalTime():yyyy-MM-ddTHH:mm:ssZ} and {fieldName} le {toDate.ToUniversalTime():yyyy-MM-ddTHH:mm:ssZ})";
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.Warn($"Failed to build DateTime criterion filter for {fieldName}. Value:{criterion.ConditionInfo.Value}, error:{ex}");
                return null;
            }
        }

        private string BuildDocumentTypeCriterionFilter(RMDiscoveryRuleCriteriaInfo criterion)
        {
            if (string.IsNullOrWhiteSpace(criterion.ConditionInfo?.Value))
            {
                return null;
            }

            try
            {
                if (criterion.ConditionInfo.Category == RMDiscoveryConditionCategory.BooleanLogic &&
                    criterion.ConditionInfo.Logic == (int)RMDiscoveryBooleanConditionType.IsEmpty)
                {
                    if (string.Equals(criterion.ConditionInfo.Value, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        return "(Extension eq '' or Extension eq null)";
                    }
                    return "(Extension ne '' and Extension ne null)";
                }

                List<string> rawValues = null;
                var trimmed = criterion.ConditionInfo.Value.Trim();
                if (trimmed.StartsWith("["))
                {
                    rawValues = JsonConvert.DeserializeObject<List<string>>(trimmed);
                }
                else
                {
                    rawValues = new List<string> { trimmed };
                }

                if (rawValues == null || rawValues.Count == 0)
                {
                    return null;
                }

                var exts = rawValues
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .Select(e => e.Trim().TrimStart('.'))
                    .Where(e => !string.IsNullOrEmpty(e))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (exts.Count == 0) return null;

                var allVariants = exts.Concat(exts.Select(e => "." + e)).Distinct().Select(EscapeODataString).ToList();
                var inClause = $"Extension in ('{string.Join("','", allVariants)}')";

                if (criterion.ConditionInfo.Logic == (int)RMDiscoveryArrayConditionType.NotIn ||
                    criterion.ConditionInfo.Logic == (int)RMDiscoveryArrayConditionType.TextNotMatchIn)
                {
                    return $"not ({inClause})";
                }

                return inClause;
            }
            catch (Exception ex)
            {
                _logger.Warn($"Failed to build Extension criterion filter. Value:{criterion.ConditionInfo.Value}, error:{ex}");
                return null;
            }
        }

        private static string EscapeODataString(string val)
        {
            return val?.Replace("'", "''");
        }

        private static long ConvertFileSizeToBytes(RMDiscoveryFileSizeConditionValue value)
        {
            var multiplier = value.UnitType switch
            {
                RMDiscoveryFileSizeUnitType.KB => 1024L,
                RMDiscoveryFileSizeUnitType.MB => 1024L * 1024,
                RMDiscoveryFileSizeUnitType.GB => 1024L * 1024 * 1024,
                _ => 1L,
            };
            return value.Unit * multiplier;
        }

        private async Task InitSettingsAsync()
        {
            List<string> filters = new List<string>();

            // Filter out deleted items in DAL
            filters.Add("IsDeleted eq false");

            if (_dataOptimizeSettingDto.WithoutDateQueryParameter != null)
            {
                if (_dataOptimizeSettingDto.WithoutDateQueryParameter.From > 0)
                {
                    filters.Add($"{RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} gt {_dataOptimizeSettingDto.WithoutDateQueryParameter.From}");
                }

                if (_dataOptimizeSettingDto.WithoutDateQueryParameter.To < 999)
                {
                    filters.Add($"{RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} le {_dataOptimizeSettingDto.WithoutDateQueryParameter.To}");
                }
            }

            if (_dataOptimizeSettingDto.FileExtensionQueryParameter?.FileExtensions != null
                && _dataOptimizeSettingDto.FileExtensionQueryParameter.FileExtensions.Count > 0)
            {
                _fileExtensions = await _FileExtensionDao.GetAsync(new Guid(_office365TenantId), _dataOptimizeSettingDto.FileExtensionQueryParameter.FileExtensions);
                if (_fileExtensions.Count > 0)
                {
                    List<string> tempNameList = new List<string>();
                    bool hasEmptyExtenstion = false;
                    foreach (var fileExten in _fileExtensions)
                    {
                        if (fileExten.Name == "RM_FA_FileType_Empty")
                        {
                            hasEmptyExtenstion = true;
                        }
                        else
                        {
                            tempNameList.Add(fileExten.Name.EscapeSpecialCharacters());
                        }
                    }

                    string extensionString = $"FileExtension in ('{string.Join("','", tempNameList)}')";
                    if (hasEmptyExtenstion)
                    {
                        extensionString = '(' + extensionString + " or FileExtension eq '')";
                    }

                    filters.Add(extensionString);
                }
            }

            if (_dataOptimizeSettingDto.SizeRangeQueryParameter != null
                && _dataOptimizeSettingDto.SizeRangeQueryParameter.QueryMode != RMDiscoverySizeRangeQueryMode.None
                && _dataOptimizeSettingDto.SizeRangeQueryParameter.SizeRange > 0)
            {
                var rangeId = _dataOptimizeSettingDto.SizeRangeQueryParameter.SizeRange;
                var condition = _dataOptimizeSettingDto.SizeRangeQueryParameter.QueryMode switch
                {
                    RMDiscoverySizeRangeQueryMode.LessThanEqual => $"{RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} le {rangeId}",
                    RMDiscoverySizeRangeQueryMode.GenerateThanEqual => $"{RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} ge {rangeId}",
                    _ => $"{RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {rangeId}"
                };

                filters.Add(condition);
            }

            if (_dataOptimizeSettingDto.ArchiveDataType == (int)ArchiverDataType.Special)
            {
                bool hasDocumentRule = false;
                bool hasVersionRule = false;
                List<RMDiscoveryRuleDefinition> rules = new List<RMDiscoveryRuleDefinition>();
                if (_checkVersionRule && _dataOptimizeSettingDto.InactiveRule != null && _dataOptimizeSettingDto.InactiveRule.Count > 0)
                {
                    hasVersionRule = true;
                    rules.AddRange(_dataOptimizeSettingDto.InactiveRule);
                }

                if (_dataOptimizeSettingDto.ROTRule != null && _dataOptimizeSettingDto.ROTRule.Count > 0)
                {
                    if (_checkVersionRule)
                    {
                        var versionRules = _dataOptimizeSettingDto.ROTRule.Where(r => r.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Version);
                        if (versionRules.Any())
                        {
                            hasVersionRule = true;
                            rules.AddRange(versionRules);
                        }
                    }
                    else
                    {
                        var docRules = _dataOptimizeSettingDto.ROTRule.Where(r => r.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Document);
                        if (docRules.Any())
                        {
                            hasDocumentRule = true;
                            rules.AddRange(docRules);
                        }
                    }
                }

                if ((_checkVersionRule ? hasVersionRule : hasDocumentRule) && rules.Count > 0)
                {
                    var ruleFilters = rules
                        .Select(BuildPlanRuleFilter)
                        .Where(filter => !string.IsNullOrWhiteSpace(filter))
                        .ToList();
                    _needDoQuery = ruleFilters.Count > 0;
                    if (_needDoQuery)
                    {
                        filters.Add(ruleFilters.Count == 1 ? ruleFilters[0] : $"({string.Join(" or ", ruleFilters)})");
                    }
                }
            }
            else
            {
                _needDoQuery = true;
            }

            if (filters.Count > 0)
            {
                _dataQueryFilters = $" and {string.Join(" and ", filters)}";
            }

            _logger.Info($"optimization filter info:{_dataQueryFilters}");
        }

        private Task InitProcessDuplicateDatasSettingsAsync()
        {
            _needDoQuery = true;
            _dataQueryFilters = " and IsDeleted eq false";

            _logger.Info($"optimization filter info:{_dataQueryFilters}");
            return Task.CompletedTask;
        }

        private object GetValueFromIdData(ExpandoObject data, string columnName)
        {
            return (data.TryGet("_id") as ExpandoObject).TryGet(columnName);
        }

        private string GetValue(ExpandoObject data, string key, string defaultValue = null)
        {
            var res = data.TryGet(key);
            return res == null ? defaultValue : res.ToString();
        }

        private T GetValue<T>(ExpandoObject data, string key, T defaultValue = default) where T : struct
        {
            var res = data.TryGet<T>(key);
            return res == null ? defaultValue : res.Value;
        }

        private async Task<string> GetByODataUrlWithRetryAsync(string odataUrl, string office365TenantId)
        {
            _ = office365TenantId;
            try
            {
                return await _dalQueryClient.DGService.GetOData(odataUrl);
            }
            catch (Exception ex)
            {
                _logger.Error($"Query DAL data by OData URL failed and will retry. Error:{ex}");
                int retry = 1;
                const int retryCount = 5;
                while (retry <= retryCount)
                {
                    try
                    {
                        var dataJson = await _dalQueryClient.DGService.GetOData(odataUrl);
                        _logger.Info("Retry success and return data json from DAL.");
                        return dataJson;
                    }
                    catch (Exception retryEx)
                    {
                        _logger.Error($"Query DAL data by OData URL retry failed. Retry:{retry}, Error:{retryEx}");
                        await Task.Delay(5000).ConfigureAwait(false);
                        retry++;
                    }
                }

                _logger.Error("Query DAL data by OData URL failed after retries.");
                throw;
            }
        }
    }
}
