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
using AvePoint.Opus.RelatedRecords.Contract;
using Microsoft.Office.Server.Search.Query;
using Microsoft.SharePoint;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace AvePoint.Opus.RelatedRecords.Utilities
{
    internal class RelatedAppRequestProcessor
    {
        public static object SaveOpusApiInfo(OpusAPIInfo apiInfo)
        {
            Logger.LogInfo($"Save Opus API info request: {SerializerHelper.SerializeByJsonSerializer(apiInfo)}");

            OpusApiTokenService.SaveOpusApiInfoAndValidateToken(apiInfo);

            return new
            {
                success = true,
                tokenCached = true,
                expiresAtUtc = OpusApiTokenService.GetTokenExpiresAtUtc().ToString("o")
            };
        }

        public static SearchPageResult QueryRecords(SearchCondition condition)
        {
            Logger.LogInfo($"Query records: {SerializerHelper.SerializeByJsonSerializer(condition)}");
            List<SearchResult> datas = new List<SearchResult>();
            var result = new SearchPageResult();
            try
            {
                if (condition.SearchScope != 1 && condition.SearchScope != 2)
                {
                    Logger.LogError($"Invalid search scope: {condition.SearchScope}");
                    return result;
                }
                if (condition.SearchScope != 2)
                {
                    return QuerySPRecords(condition, datas, result);
                }
                else
                {
                    return QueryPhysicalRecords(condition, datas, result);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Query records failed: {ex}");
            }

            return result;
        }

        private static SearchPageResult QuerySPRecords(SearchCondition condition, List<SearchResult> datas, SearchPageResult result)
        {
            using (var site = new SPSite(condition.SiteUrl))
            using (var web = site.OpenWeb(condition.WebId))
            using (var keywordQuery = new KeywordQuery(web))
            {
                string queryDlcDcId = (condition.QueryText ?? string.Empty).Replace("-", " ");
                keywordQuery.QueryText = $"((IsDocument=True AND (FileName:{condition.QueryText} OR DlcDocId=\"{queryDlcDcId}\")) OR (ContentTypeId:0x0100* AND Title:{condition.QueryText}))";
                keywordQuery.RowLimit = 10;
                keywordQuery.StartRow = 10 * condition.PageIndex;
                keywordQuery.TrimDuplicates = false;
                keywordQuery.SelectProperties.Add("UniqueId");
                keywordQuery.SelectProperties.Add("ListItemID");
                keywordQuery.SelectProperties.Add("AuthorOWSUSER");
                keywordQuery.SelectProperties.Add("EditorOWSUSER");
                keywordQuery.SelectProperties.Add("Filename");
                keywordQuery.SelectProperties.Add("Title");
                keywordQuery.SelectProperties.Add("Path");
                keywordQuery.SelectProperties.Add("FileExtension");
                keywordQuery.SelectProperties.Add("SPSiteUrl");
                keywordQuery.SelectProperties.Add("IsDocument");
                keywordQuery.SelectProperties.Add("SiteID");
                keywordQuery.SelectProperties.Add("WebId");
                keywordQuery.SelectProperties.Add("ListID");
                keywordQuery.SelectProperties.Add("DlcDocId");

                SearchExecutor searchExecutor = new SearchExecutor();
                var resultTableCollection = searchExecutor.ExecuteQuery(keywordQuery);
                var resultTable = resultTableCollection.Filter("TableType", KnownTableTypes.RelevantResults).FirstOrDefault();
                if (resultTable != null)
                {
                    DataTable dataTable = resultTable.Table;
                    result.TotalPage = (int)(Math.Ceiling(resultTable.TotalRows / 10m));
                    foreach (DataRow row in dataTable.Rows)
                    {
                        datas.Add(new SearchResult()
                        {
                            SPSiteUrl = GetStringValue(row, "SPSiteUrl"),
                            SiteId = GetStringValue(row, "SiteID").ToLower(),
                            WebId = GetStringValue(row, "WebId").ToLower(),
                            ListId = GetStringValue(row, "ListID").ToLower(),
                            ListItemID = GetIntValue(row, "ListItemID"),
                            FileName = GetStringValue(row, "FileName"),
                            Title = GetStringValue(row, "Title"),
                            Path = GetStringValue(row, "Path"),
                            UniqueId = Guid.TryParse(GetStringValue(row, "UniqueId"), out var uid) ? uid.ToString() : "",
                            IsDocument = GetBoolValue(row, "IsDocument"),
                            FileExtension = GetStringValue(row, "FileExtension"),
                            DocumentId = GetStringValue(row, "DlcDocId")
                        });
                    }
                }

                result.Datas = datas;
            }

            return result;
        }

        private static SearchPageResult QueryPhysicalRecords(SearchCondition condition, List<SearchResult> datas, SearchPageResult result)
        {
            const int pageSize = 10;
            if(condition.PageSize == 0)
            {
                condition.PageSize = pageSize;
            }
            var requestBody = SerializerHelper.SerializeByJsonSerializer(new
            {
                PageIndex = condition.TokenIndex,
                PageSize = condition.PageSize,
                Value = condition.QueryText
            });

            var responseText = OpusApiTokenService.CallExternalApi("POST", "/API/AppActionsForSPS/SearchRecords", requestBody);
            var responseJson = ParseApiResponseToObject(responseText);

            var items = responseJson["Datas"] as JArray;
            if (items != null)
            {
                foreach (var item in items.OfType<JObject>())
                {
                    var uniqueId = item.Value<string>("Id")
                        ?? item.Value<string>("NodeId")
                        ?? string.Empty;
                    var fullPath = item.Value<string>("FullPath") ?? item.Value<string>("DirPath") ?? string.Empty;
                    var fileName = item.Value<string>("LeafName") ?? string.Empty;

                    datas.Add(new SearchResult
                    {
                        SPSiteUrl = fullPath,
                        SiteId = (item.Value<string>("AveSiteId") ?? Guid.Empty.ToString()).ToLower(),
                        WebId = (item.Value<string>("WebId") ?? Guid.Empty.ToString()).ToLower(),
                        ListId = (item.Value<string>("ListId") ?? Guid.Empty.ToString()).ToLower(),
                        ListItemID = 0,
                        FileName = fileName,
                        Title = fileName,
                        Path = fullPath,
                        UniqueId = Guid.TryParse(uniqueId, out var uid) ? uid.ToString() : uniqueId,
                        IsDocument = false,
                        FileExtension = item.Value<string>("ExtensionForFile") ?? string.Empty,
                        NodeType = item.Value<string>("NodeType") ?? string.Empty,
                        DocumentId = item.Value<string>("RecordsId") ?? string.Empty
                    });
                }
            }

            var pagingInfo = responseJson["PagingInfo"] as JObject;
            var total = pagingInfo?.Value<int?>("Total") ?? datas.Count;
            result.TokenIndex = pagingInfo?.Value<string>("PageIndex") ?? string.Empty;
            result.HasNextPage = pagingInfo?.Value<bool?>("HasNextPage") ?? false;
            result.TotalPage = total <= 0 ? 0 : (int)Math.Ceiling(total / (decimal)pageSize);
            result.Datas = datas;

            return result;
        }

        private static JObject ParseApiResponseToObject(string responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText))
            {
                return new JObject();
            }

            var token = JToken.Parse(responseText);
            if (token.Type == JTokenType.String)
            {
                var innerJson = token.Value<string>();
                if (string.IsNullOrWhiteSpace(innerJson))
                {
                    return new JObject();
                }

                token = JToken.Parse(innerJson);
            }

            if (token is JObject obj)
            {
                return obj;
            }

            throw new JsonException("Expected JSON object in API response.");
        }

        public static bool CheckItemHasEditPermission(ListItemInfo itemInfo)
        {
            bool result = false;
            Logger.LogInfo($"Check record permission: {SerializerHelper.SerializeByJsonSerializer(itemInfo)}");
            try
            {
                var currentUser = HttpContext.Current.User;
                string username = currentUser.Identity.Name?.Split('|').LastOrDefault();
                Logger.LogInfo($"Check record permission for {currentUser.Identity.Name}");
                SPSecurity.RunWithElevatedPrivileges(() =>
                {
                    using (var site = new SPSite(itemInfo.SiteUrl))
                    using (var web = site.OpenWeb(itemInfo.WebId))
                    {
                        var list = web.Lists[itemInfo.ListId];
                        SPListItem item = null;
                        try
                        {
                            item = list.GetItemById(itemInfo.ListItemId);
                        }
                        catch (ArgumentException aex)
                        {
                            Logger.LogInfo($"Item {itemInfo.ListItemId} not found in list {itemInfo.ListId}, treat as success. {aex.Message}");
                            result = true;
                            return;
                        }
                        catch (SPException spex)
                        {
                            if (spex.Message.IndexOf("does not exist", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                Logger.LogInfo($"Item {itemInfo.ListItemId} not found in list {itemInfo.ListId}, treat as success. {spex.Message}");
                                result = true;
                                return;
                            }
                            throw;
                        }

                        if (item != null)
                        {
                            SPUser spUser = web.EnsureUser(username);
                            result = item.DoesUserHavePermissions(spUser, SPBasePermissions.EditListItems);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.LogError($"Check record permission: {ex}");
            }

            return result;
        }

        public static object TryAddRecord(ListItemInfo itemInfo)
        {
            Logger.LogInfo($"TryAddRecord request: {SerializerHelper.SerializeByJsonSerializer(itemInfo)}");
            try
            {
                if (itemInfo == null)
                {
                    return new { success = false, error = "itemInfo is null." };
                }

                var payload = BuildTryAddRecordRequestBody(itemInfo);
                var responseText = OpusApiTokenService.CallExternalApi("POST", "/API/AppActionsForSPS/TryAddRecord", payload);
                var responseJson = ParseApiResponseToObject(responseText);

                bool isSuccess = responseJson.Value<bool?>("Success")
                    ?? responseJson.Value<bool?>("success")
                    ?? false;

                if (!isSuccess)
                {
                    var error = responseJson.Value<string>("Message")
                        ?? responseJson.Value<string>("message")
                        ?? "TryAddRecord failed.";
                    return new { success = false, error, detail = responseJson };
                }

                return new { success = true, data = responseJson };
            }
            catch (Exception ex)
            {
                Logger.LogError($"TryAddRecord failed: {ex}");
                return new { success = false, error = ex.Message };
            }
        }

        private static string BuildTryAddRecordRequestBody(ListItemInfo itemInfo)
        {
            using (var site = new SPSite(itemInfo.SiteUrl))
            using (var web = site.OpenWeb(itemInfo.WebId))
            {
                var list = web.Lists[itemInfo.ListId];
                var item = list.GetItemById(itemInfo.ListItemId);
                var nodeId = new Guid(item["UniqueId"].ToString());
                var scopeId = web.Site.ID;
                var aveSiteId = site.ID;
                var recId = HashCodeHelper.StringHash(scopeId.ToString().ToLowerInvariant() + nodeId.ToString().ToLowerInvariant());
                var fileRef = item["FileRef"]?.ToString() ?? string.Empty;
                var fileDirRef = item["FileDirRef"]?.ToString() ?? string.Empty;
                var fullPath = BuildFullPath(site.Url, fileRef);
                var leafName = list.BaseType == SPBaseType.DocumentLibrary
                    ? item["FileLeafRef"]?.ToString() ?? item["Title"]?.ToString() ?? string.Empty
                    : item["Title"]?.ToString() ?? string.Empty;
                var extensionForFile = list.BaseType == SPBaseType.DocumentLibrary
                    ? GetItemExtension(leafName)
                    : "RM_RDM_RecordDetails_DataType_SPItem";
                string recordsId = string.Empty;

                if (item.Fields.ContainsField("_dlc_DocId"))
                {
                    recordsId = item["_dlc_DocId"]?.ToString();
                }
                else if (item.Fields.ContainsField("RevIMUniqueID"))
                {
                    recordsId = item["RevIMUniqueID"]?.ToString();
                }


                var requestBody = SerializerHelper.SerializeByJsonSerializer(new
                {
                    PersistAfterConvert = true,
                    Input = new
                    {
                        Id = recId,
                        ScopeId = scopeId,
                        NodeId = nodeId,
                        DirPath = fileDirRef,
                        FullPath = fullPath,
                        RecordsId = recordsId,
                        NodeType = 500,  // 500 stands for File. This API will only be invoked for items of type File sourced from SharePoint On-Prem.,
                        AveSiteId = aveSiteId,
                        WebId = web.ID,
                        ListId = list.ID,
                        ItemId = nodeId,
                        FolderId = item.GetGuidFieldValue("ParentUniqueId"),
                        LeafName = leafName,
                        UniqueId = nodeId,
                        CollectionTime = DateTime.UtcNow.Ticks,
                        TimeCreated = item.GetUTCDateWithTimeZone("Created"),
                        TimeLastModified = item.GetUTCDateWithTimeZone("Modified"),
                        TermId = Guid.Empty,
                        TermName = string.Empty,
                        DisposalDueDate = string.Empty,
                        DeclareAsRecord = item.IsBlockEditAndDeleteRecord(),
                        CreatedBy = item.GetSingleUserFieldValue("Author"),
                        ModifiedBy = item.GetSingleUserFieldValue("Editor"),
                        ExtensionForFile = extensionForFile,
                        MetaInfo = JsonConvert.SerializeObject(new
                        {
                            FileSize = Convert.ToInt64(item.GetFieldValue("File_x0020_Size", "0"))
                        }),
                        RelatedRecords = string.Empty,  // TODO
                        RelatedRecordsCount = 0,    // TODO
                        ItemRowId = item.ID,
                        RuleId = Guid.Empty,
                        RuleLevel = 0,
                        RecordStatus = 1, // (int)RMRecordStatus.Active,
                        ApproveUsers = item.GetFieldValue("HPRM_RecordNumber")
                    }
                });

                return requestBody;
            }
        }

        private static string BuildFullPath(string siteUrl, string serverRelativePath)
        {
            if (string.IsNullOrWhiteSpace(serverRelativePath))
            {
                return siteUrl ?? string.Empty;
            }

            if (Uri.TryCreate(serverRelativePath, UriKind.Absolute, out var absolute))
            {
                return absolute.ToString();
            }

            if (Uri.TryCreate(siteUrl, UriKind.Absolute, out var siteUri)
                && Uri.TryCreate(siteUri, serverRelativePath, out var combined))
            {
                return combined.ToString();
            }

            return serverRelativePath;
        }

        private static string GetItemExtension(string objectName)
        {
            var ext = Path.GetExtension(objectName ?? string.Empty);
            if (string.IsNullOrWhiteSpace(ext))
            {
                return "RM_RDM_RecordDetails_DataType_FileNull";
            }

            return ext.StartsWith(".") ? ext.Substring(1) : ext;
        }


        private static string GetStringValue(DataRow row, string columnName, string defaultValue = "")
        {
            if (!row.Table.Columns.Contains(columnName) || row.IsNull(columnName))
                return defaultValue;

            return row[columnName].ToString();
        }

        private static int GetIntValue(DataRow row, string columnName, int defaultValue = 0)
        {
            if (!row.Table.Columns.Contains(columnName) || row.IsNull(columnName))
                return defaultValue;

            if (int.TryParse(row[columnName].ToString(), out int result))
                return result;

            return defaultValue;
        }

        private static bool GetBoolValue(DataRow row, string columnName, bool defaultValue = false)
        {
            if (!row.Table.Columns.Contains(columnName) || row.IsNull(columnName))
                return defaultValue;

            var value = row[columnName].ToString().ToLower();
            return value == "true" || value == "1" || value == "yes";
        }

        private static DateTime? GetNullableDateTimeValue(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row.IsNull(columnName))
                return null;

            if (DateTime.TryParse(row[columnName].ToString(), out DateTime result))
                return result;

            return null;
        }
    }
}
