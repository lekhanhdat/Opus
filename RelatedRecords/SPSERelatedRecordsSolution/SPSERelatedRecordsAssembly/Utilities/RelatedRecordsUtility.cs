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
using Microsoft.SharePoint;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace AvePoint.Opus.RelatedRecords.Utilities
{
    internal class RelatedRecordsUtility
    {
        private const string RelatedRecordsFieldInternalName = "RecordsRelated";
        private const string RelatedRecordsColumnHeader = "<div class=\"ExternalClass702E44874C854B099A61564528AE0439\">{0}</div>";
        private const string RelatedRecordsCategoryHeader = "<p style=\"font-weight:600\">{0}:</p>";
        private const string RelatedRecordsColumnStrcuture = "<p style=\"margin: 0px 0px 5px 0px;\">​<a rel =\"{0}\" href=\"{1}\" style=\"{3}\">{2}</a>​</p>";
        private const string RelatedRecordsColumnStyle = "color: black;text-decoration: none;";

        private static readonly Guid RelatedRecordsColumnId = new Guid("b40273fb-26d2-40e8-9a34-dd20bc9ca1d7");

        private RelatedItemSubmit _relatedItemSubmitInfo = null;

        public RelatedRecordsUtility(RelatedItemSubmit saveInfo)
        {
            this._relatedItemSubmitInfo = saveInfo;
        }

        public RAReturnMessage SubmitRelatedRecords()
        {
            Logger.LogInfo($"Submit related items: {Utilities.SerializerHelper.SerializeByJsonSerializer(_relatedItemSubmitInfo)}");
            var rstMsg = new RAReturnMessage { MessageType = RAMessageType.Successful };

            if (_relatedItemSubmitInfo?.CurrentInfo == null)
            {
                Logger.LogError($"Invalid parameters.");
                return Fail(rstMsg, "Invalid parameters.");
            }

            try
            {
                SPSecurity.RunWithElevatedPrivileges(() =>
                {
                    InnerSubmitRelatedRecords();
                });
            }
            catch (Exception ex)
            {
                rstMsg.MessageType = RAMessageType.Failed;
                rstMsg.ErrorMessage = "Submit related items failed: " + ex.Message;

                Logger.LogError($"Submit related item error: {ex}");
            }

            return rstMsg;
        }

        private void InnerSubmitRelatedRecords()
        {
            using (var site = new SPSite(_relatedItemSubmitInfo.CurrentInfo.SiteUrl))
            using (var web = site.OpenWeb(_relatedItemSubmitInfo.CurrentInfo.WebId))
            {
                var list = web.Lists[_relatedItemSubmitInfo.CurrentInfo.ListId];
                var currentItem = list.GetItemById(_relatedItemSubmitInfo.CurrentInfo.ListItemId);
                if (CheckIsRecord(currentItem))
                {
                    throw new Exception($"Current item is declared record {currentItem.DisplayName}");
                }

                if (!list.Fields.ContainsFieldWithStaticName(RelatedRecordsFieldInternalName))
                {
                    throw new Exception($"Related item's parent list is not contains RelatedColumn. ItemId:{list.Title}");
                }

                var sourceRelatedItems = GetRelatedItems(currentItem);
                var sourceItemInfo = GenerateRelatedItemInfoForSP(currentItem);
                sourceItemInfo.id = new Guid(_relatedItemSubmitInfo.CurrentInfo.RecordId);
                foreach (var desInfo in _relatedItemSubmitInfo.RelatedInfos)
                {
                    if (desInfo.SiteUrl.Equals(_relatedItemSubmitInfo.CurrentInfo.SiteUrl, StringComparison.OrdinalIgnoreCase)
                        && desInfo.WebId.Equals(_relatedItemSubmitInfo.CurrentInfo.WebId)
                        && desInfo.ListId.Equals(_relatedItemSubmitInfo.CurrentInfo.ListId)
                        && desInfo.ListItemId.Equals(_relatedItemSubmitInfo.CurrentInfo.ListItemId))
                    {
                        throw new Exception("Can't relate to oneself");
                    }
                }


                bool hasFailedRecords = false;
                List<string> noRelatedColumnItems = new List<string>();

                try
                {
                    SyncRelatedRecordWithPublicApi(sourceItemInfo, _relatedItemSubmitInfo.RelatedInfos);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Sync related record failed. Source:{sourceItemInfo.id}. {ex}");
                    hasFailedRecords = true;
                }

                if (hasFailedRecords)
                {
                    if (noRelatedColumnItems.Count == 0)
                    {
                        throw new Exception("Related Records has failed Records");
                    }
                    else
                    {
                        throw new Exception(string.Format("The following files cannot be added as related records, because they do not have the Related Records column in SharePoint: {0}.", string.Join(", ", noRelatedColumnItems)));
                    }
                }

                #region 获取source 的最终 relatedInfos， 如果source 是SP ，更新SP item 属性
                // var originColumnValue = GetRelatedColumnValue(currentItem);
                // try
                // {
                //     //理论上currentRecord 的sourceflag 是准的，添加 0 判断只为了代码健壮,０是SP老数据，老数据中可能没有存
                //     var sourceFinalRelatedValue = ConvertToSPColumnValueString(sourceRelatedItems);
                //     if (!originColumnValue.Equals(sourceFinalRelatedValue, StringComparison.OrdinalIgnoreCase))
                //     {
                //         if (!sourceFinalRelatedValue.Contains("href"))
                //         {
                //             sourceFinalRelatedValue = string.Empty;
                //         }
                //         UpdateSPItemRelatedProperties(currentItem, sourceFinalRelatedValue);
                //     }
                // }
                // catch (Exception se)
                // {
                //     Logger.LogWarning($"submit related records failed {currentItem["FileRef"]}:{originColumnValue}:{se}");
                //     throw;
                // }
                #endregion
                var displayName = string.Empty;
                if ((currentItem["FileLeafRef"] as string).EndsWith("_.000", StringComparison.OrdinalIgnoreCase))
                {
                    displayName = currentItem["Title"] as string;
                    if (string.IsNullOrEmpty(displayName))
                    {
                        displayName = "";
                    }
                }
                else
                {
                    displayName = currentItem["FileLeafRef"].ToString();
                }
            }
        }

        private static bool IsPhysicalSubmitInfo(RelatedItemSubmitInfo itemInfo)
        {
            if (itemInfo == null)
            {
                return false;
            }

            return string.IsNullOrWhiteSpace(itemInfo.SiteUrl)
                || (itemInfo.WebId == Guid.Empty && itemInfo.ListId == Guid.Empty && itemInfo.ListItemId <= 0);
        }

        private static List<RelatedItemSubmitInfo> MergePhysicalSubmitInfos(IEnumerable<RelatedItemSubmitInfo> relatedInfos)
        {
            var result = new List<RelatedItemSubmitInfo>();
            if (relatedInfos == null)
            {
                return result;
            }

            var merged = new Dictionary<Guid, RelatedItemSubmitInfo>();
            foreach (var item in relatedInfos)
            {
                // if (!IsPhysicalSubmitInfo(item))
                // {
                //     continue;
                // }

                if (item.UniqueId == Guid.Empty)
                {
                    continue;
                }

                merged[item.UniqueId] = item;
            }

            result.AddRange(merged.Values);
            return result;
        }

        private void SyncRelatedRecordWithPublicApi(RMRelatedItemInfo sourceItemInfo, IEnumerable<RelatedItemSubmitInfo> relatedItemInfos)
        {
            if (sourceItemInfo == null)
            {
                throw new ArgumentNullException(nameof(sourceItemInfo));
            }

            if (relatedItemInfos == null)
            {
                throw new ArgumentNullException(nameof(relatedItemInfos));
            }

            var addIds = relatedItemInfos
                .Where(i => !i.NeedDelete)
                .Select(i => i.UniqueId)
                .Distinct()
                .ToList();

            var deleteIds = relatedItemInfos
                .Where(i => i.NeedDelete)
                .Select(i => i.UniqueId)
                .Distinct()
                .ToList();
            var idNameDict = BuildIdNameDict(sourceItemInfo, relatedItemInfos);

            var requestBody = Utilities.SerializerHelper.SerializeByJsonSerializer(new
            {
                Id = sourceItemInfo.id,
                ReletedIds = addIds.Count == 0 ? null : addIds,
                DeleteReletedIds = deleteIds.Count == 0 ? null : deleteIds,
                IdNameDict = idNameDict.Count == 0 ? null : idNameDict
            });

            var responseText = OpusApiTokenService.CallExternalApi("POST", "/API/AppActionsForSPS/UpdateRelatedRecordsWithSP", requestBody);
            EnsureUpdateRelatedRecordsSucceeded(responseText);
        }


        private static Dictionary<Guid, string> BuildIdNameDict(RMRelatedItemInfo sourceItemInfo, IEnumerable<RelatedItemSubmitInfo> relatedItemInfos)
        {
            var result = new Dictionary<Guid, string>();

            AddIdName(result, sourceItemInfo.id, sourceItemInfo.name);

            foreach (var item in relatedItemInfos)
            {
                if (item == null || item.UniqueId == Guid.Empty)
                {
                    continue;
                }

                AddIdName(result, item.UniqueId, item.Name);
            }

            return result;
        }

        private static void AddIdName(Dictionary<Guid, string> dict, Guid id, string name)
        {
            if (dict == null || id == Guid.Empty)
            {
                return;
            }

            var normalizedName = name ?? string.Empty;
            if (!dict.ContainsKey(id) || string.IsNullOrWhiteSpace(dict[id]))
            {
                dict[id] = normalizedName;
            }
        }

        private static void EnsureUpdateRelatedRecordsSucceeded(string responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText))
            {
                return;
            }

            var responseObj = ParseApiResponseToObject(responseText);
            var resultObj = responseObj["result"] as JObject ?? responseObj["Result"] as JObject;
            if (resultObj == null)
            {
                return;
            }

            var messageTypeToken = resultObj["MessageType"];
            if (IsSuccessfulMessageType(messageTypeToken))
            {
                return;
            }

            var errorMessage = resultObj.Value<string>("ErrorMessage");
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                errorMessage = "UpdateRelatedRecordsWithSP returned failed message type.";
            }

            throw new Exception(errorMessage);
        }

        private static bool IsSuccessfulMessageType(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
            {
                return true;
            }

            if (token.Type == JTokenType.Integer)
            {
                return token.Value<int>() == (int)RAMessageType.Successful;
            }

            var text = token.ToString();
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            if (int.TryParse(text, out var numericType))
            {
                return numericType == (int)RAMessageType.Successful;
            }

            return string.Equals(text, RAMessageType.Successful.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private static JObject ParseApiResponseToObject(string responseText)
        {
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


        private string GetRelatedColumnValue(SPListItem listItem)
        {
            var columnValue = listItem[RelatedRecordsFieldInternalName] as string ?? string.Empty;
            columnValue = HttpUtility.UrlDecode(columnValue);
            columnValue = columnValue.Replace("&#58;", ":");
            return columnValue;
        }

        private RMRelatedItemInfo GenerateRelatedItemInfoForSP(SPListItem currentItem)
        {
            RMRelatedItemInfo info = new RMRelatedItemInfo();
            info.SourceFlag = (int)SourceFlag.SharePointOnPrem;
            info.NodeType = 500; // (int)GCommon.Contract.Tree.Object.NodeLevel.Item;
            info.DocLibRowId = currentItem.ID;
            var folder = currentItem.Web.GetFolder(currentItem["FileDirRef"].ToString());
            info.FolderId = folder.UniqueId;
            info.id = new Guid(currentItem["UniqueId"].ToString());
            string displayName = string.Empty;
            if ((currentItem["FSObjType"] as string).Equals(((int)SPFileSystemObjectType.File).ToString()))
            {
                if ((currentItem["FileLeafRef"] as string).EndsWith("_.000", StringComparison.OrdinalIgnoreCase))
                {
                    displayName = currentItem["Title"] as string;
                    if (string.IsNullOrEmpty(displayName))
                    {
                        displayName = "";
                    }
                }
                else
                {
                    displayName = currentItem["FileLeafRef"].ToString();
                }
            }
            else
            {
                displayName = currentItem["FileLeafRef"].ToString();
            }

            var currentWeb = currentItem.Web;
            var currentSite = currentWeb.Site;
            var currentList = currentItem.ParentList;

            info.name = displayName;
            info.ParentFolderIsRootFolder = currentItem.ParentList.RootFolder.ServerRelativeUrl.Equals(folder.ServerRelativeUrl);//confirm
            info.WebId = currentWeb.ID;
            info.WebUrl = currentWeb.Url;
            info.SiteId = currentSite.ID;
            info.SiteUrl = currentSite.Url;
            info.level = currentList.BaseType.Equals(SPBaseType.DocumentLibrary) ? SOEndUserArchiverNodeLevel.Document : SOEndUserArchiverNodeLevel.Item;
            info.ListId = currentList.ID;
            info.WebServerRelativeUrl = currentWeb.ServerRelativeUrl;
            info.ListUrl = currentList.RootFolder.ServerRelativeUrl;
            info.FolderUrl = folder.ServerRelativeUrl;
            info.ItemUrl = currentItem["FileRef"].ToString();
            //info.url = info.SiteUrl + "/" + info.ItemUrl.Substring(currentWeb.ServerRelativeUrl.TrimEnd('/').Length + 1);
            if (info.level == SOEndUserArchiverNodeLevel.Item)
            {
                info.url = GetListItemRealPath(info.SiteUrl, currentList.RootFolder.ServerRelativeUrl, info.ItemUrl);
            }
            else
            {
                info.url = new Uri(info.SiteUrl).Scheme + @"://" + new Uri(info.SiteUrl).Authority + info.ItemUrl;
            }
            return info;
        }

        private static string GetListItemRealPath(string webUrl, string listServerUrl, string itemPath)
        {
            if (string.IsNullOrEmpty(webUrl))
            {
                throw new ArgumentNullException("webUrl");
            }
            if (string.IsNullOrEmpty(listServerUrl))
            {
                throw new ArgumentNullException("listServerUrl");
            }
            if (string.IsNullOrEmpty(itemPath))
            {
                throw new ArgumentNullException("itemPath");
            }
            string itemName = "";
            if (!itemPath.Contains("/"))
            {
                itemName = itemPath;
            }
            else
            {
                itemName = itemPath.Substring(itemPath.LastIndexOf("/") + 1);
            }
            return MakeFullUrl(webUrl, listServerUrl) + $"/DispForm.aspx?ID={itemName.Split('_')[0]}";
        }
        /// <summary>
        /// for sp make full url
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <param name="strUrl"></param>
        /// <returns></returns>
        private static string MakeFullUrl(string siteUrl, string strUrl)
        {
            if (siteUrl == null || strUrl == null)
            {
                throw new ArgumentNullException("strUrl");
            }
            if (siteUrl == strUrl)
            {
                return siteUrl;
            }
            if (strUrl.StartsWith("http:") || strUrl.StartsWith("https:"))
            {
                return strUrl;
            }
            strUrl = strUrl.Trim();
            StringBuilder stringBuilder = new StringBuilder(512);
            if (strUrl.StartsWith("/"))
            {
                var siteUri = new Uri(siteUrl);
                var protocol = siteUri.Scheme + ":";
                stringBuilder.Append(protocol);
                stringBuilder.Append("//");
                stringBuilder.Append(siteUri.Host);
                if ((StsCompareStrings(protocol, "http:") && siteUri.Port != 80) || (StsCompareStrings(protocol, "https:") && siteUri.Port != 443))
                {
                    stringBuilder.Append(":");
                    stringBuilder.Append(siteUri.Port);
                }
                stringBuilder.Append(strUrl);
            }
            else
            {
                stringBuilder.Append(siteUrl);
                if (strUrl != "")
                {
                    stringBuilder.Append("/");
                    stringBuilder.Append(strUrl);
                }
            }
            if (stringBuilder[stringBuilder.Length - 1] == '/')
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            return stringBuilder.ToString();
        }
        private static bool StsCompareStrings(string str1, string str2)
        {
            System.Globalization.CompareInfo compareInfo = System.Globalization.CultureInfo.InvariantCulture.CompareInfo;
            return 0 == compareInfo.Compare(str1, str2, System.Globalization.CompareOptions.IgnoreCase);
        }

        private bool UpdateRelatedItem(SPListItem destItem, List<string> noRelatedColumnItems, UpdateRelatedRecordParams param)
        {
            var result = true;
            try
            {
                var destList = destItem.ParentList;
                var destWeb = destItem.Web;
                Logger.LogInfo($"SPRelated get list successfully. ListId:{param?.DesInfo?.ListId}");
                if (!destList.Fields.ContainsFieldWithStaticName(RelatedRecordsFieldInternalName))
                {
                    Logger.LogError($"Related item's parent list is not contains RelatedColumn. ItemId:{param?.DesInfo?.DocLibRowId}");
                    noRelatedColumnItems.Add(param.DesInfo.name);
                    result = false;
                }
                else
                {
                    Logger.LogInfo($"SPRelated get item successfully. ItemId:{param.DesInfo.DocLibRowId}");

                    var originColumnValue = GetRelatedColumnValue(destItem);
                    var updateValue = ConvertToSPColumnValueString(param.DestRelatedInfos);
                    if (!originColumnValue.Equals(updateValue, StringComparison.OrdinalIgnoreCase))
                    {
                        UpdateSPItemRelatedProperties(destItem, updateValue);
                        Logger.LogInfo($"SPRelated update item successfully. ItemId:{param?.DesInfo?.DocLibRowId}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"add related record failed: {param.DesInfo.url}. {ex}");
                result = false;
            }

            return result;
        }

        private void UpdateSPItemRelatedProperties(SPListItem item, string relatedValue)
        {
            item[RelatedRecordsFieldInternalName] = relatedValue;
            item.Update();
        }

        /// <summary>
        /// 将List <RMRelatedItemInfo> 对象转换成SP column 的value
        /// </summary>
        /// <param name="relatedItemInfos"></param>
        /// <returns></returns>
        private string ConvertToSPColumnValueString(List<RMRelatedItemInfo> relatedItemInfos)
        {
            string relatedInfo = string.Empty;
            if (relatedItemInfos == null || relatedItemInfos.Count == 0)
            {
                return relatedInfo;
            }
            StringBuilder electronicBuilder = new StringBuilder();
            StringBuilder physicalBuilder = new StringBuilder();
            foreach (var relatedItemInfo in relatedItemInfos)
            {
                string rel = SerializerHelper.SerializeByJsonSerializer(relatedItemInfo);
                rel = HttpUtility.HtmlEncode(rel);
                rel = rel.TrimStart('[').TrimEnd(']');
                if (relatedItemInfo.SourceFlag == (int)SourceFlag.Physical)
                {
                    rel = string.Format(RelatedRecordsColumnStrcuture, rel, relatedItemInfo.url, relatedItemInfo.recId, RelatedRecordsColumnStyle);
                    physicalBuilder.Append(rel);
                }
                else
                {
                    rel = string.Format(RelatedRecordsColumnStrcuture, rel, relatedItemInfo.url, relatedItemInfo.name, string.Empty);
                    electronicBuilder.Append(rel);
                }
                //logger.Info("Debug: related infos href:{0}", relatedItemInfo.url);
            }

            //var noneInfo = string.Format("<p>{0}</p>", I18NEntity.GetString("RM_SS_RelatedRecords_Data_None"));
            var electronicInfo = electronicBuilder.Length > 0 ?
                string.Format(RelatedRecordsCategoryHeader, "Electronic") + electronicBuilder.ToString()
                : string.Empty;

            var physicalInfo = physicalBuilder.Length > 0 ?
                string.Format(RelatedRecordsCategoryHeader, "Physical") + physicalBuilder.ToString()
                : string.Empty;
            relatedInfo = string.Format(RelatedRecordsColumnHeader, electronicInfo + physicalInfo);
            return relatedInfo;
        }

        private void AddOrRemoveRelatedInfos(ref UpdateRelatedRecordParams param)
        {
            if (param.DesInfo.NeedDelete)
            {
                //移除目的端文件中，关于原端的related 信息
                param.DestRelatedInfos = this.RemoveRelatedInfo(param.DestRelatedInfos, param.SourceInfo);
                //从source related info 中移除 desInfo信息
                param.SourceRelatedInfos = this.RemoveRelatedInfo(param.SourceRelatedInfos, param.DesInfo);
            }
            else
            {
                //在目的端related 信息中添加原端 属性
                param.DestRelatedInfos = this.AddRelatedInfo(param.DestRelatedInfos, param.SourceInfo);
                //在原端属性中，添加目的端related 信息
                param.SourceRelatedInfos = this.AddRelatedInfo(param.SourceRelatedInfos, param.DesInfo);
            }
        }
        /// <summary>
        /// 方法提供删除RelatedInfo 的功能
        /// </summary>
        /// <param name="relatedInfos">当前文件所有RelatedInfoCollection</param>
        /// <param name="info">需要Remove 掉的RelatedInfo</param>
        /// <returns>返回Remove 后剩余的RelatedInfoCollection</returns>
        private List<RMRelatedItemInfo> RemoveRelatedInfo(List<RMRelatedItemInfo> relatedInfos, RMRelatedItemInfo info)
        {
            var result = new List<RMRelatedItemInfo>();
            if (relatedInfos != null)
            {
                relatedInfos.RemoveAll(r => r.id == info.id && r.SiteId == info.SiteId);
                result = relatedInfos;
            }
            return result;
        }
        private List<RMRelatedItemInfo> AddRelatedInfo(List<RMRelatedItemInfo> relatedInfos, RMRelatedItemInfo info)
        {
            var result = new List<RMRelatedItemInfo>();
            if (relatedInfos != null)
            {
                var relatedInfo = relatedInfos.Where(r => r.id == info.id && r.SiteId == info.SiteId).FirstOrDefault();
                if (relatedInfo != null)
                {
                    //SP中Rename处理逻辑
                    if (relatedInfo.name != info.name)
                    {
                        relatedInfos.Remove(relatedInfo);
                        relatedInfos.Add(info);
                    }
                }
                else
                {
                    relatedInfos.Add(info);
                }
            }
            else
            {
                relatedInfos = new List<RMRelatedItemInfo>();
                relatedInfos.Add(info);
            }
            result = relatedInfos;
            return result;
        }

        private bool CheckIsRecord(SPListItem currentItem)
        {
            bool isRecord = false;
            if (currentItem.Fields.ContainsField("_vti_ItemHoldRecordStatus"))
            {
                //object obj = item[new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")];
                object obj = currentItem["_vti_ItemHoldRecordStatus"];
                if (int.TryParse(obj?.ToString(), out var result))
                {
                    isRecord = IsBlockEditAndDeleteRecord(result);
                }
            }
            return isRecord;
        }
        public bool IsBlockEditAndDeleteRecord(int holdAndRecordStatus)
        {
            return ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.RecordMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.EditBlockedMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.DeleteBlockedMask) != 0L);
        }

        private string TryGetTitle(SPListItem item)
        {
            if (item == null) return string.Empty;
            if (item.Fields.ContainsField("Title"))
            {
                var v = item["Title"];
                if (v != null) return v.ToString();
            }
            if (item.Fields.ContainsField("FileLeafRef"))
            {
                var v = item["FileLeafRef"];
                if (v != null) return v.ToString();
            }
            return item.Name ?? string.Empty;
        }

        private List<RMRelatedItemInfo> GetRelatedItems(SPListItem item)
        {
            var relatedItems = new List<RMRelatedItemInfo>();
            if (item == null) return relatedItems;

            if (!item.Fields.ContainsField(RelatedRecordsFieldInternalName))
                return relatedItems;

            var relatedColumnValue = item[RelatedRecordsFieldInternalName] as string;
            if (string.IsNullOrEmpty(relatedColumnValue)) return relatedItems;

            return GetRelatedItemsBySPColumnValue(relatedColumnValue);
        }

        /// <summary>
        /// 将SP Column value 转换成List,SP column value 有特殊属性，所以需要解析XML<RMRelatedItemInfo>
        /// </summary>
        /// <param name="relatedColumnValue"></param>
        /// <returns>如果没有related column value，则返回空集合，不返回null</returns>
        private List<RMRelatedItemInfo> GetRelatedItemsBySPColumnValue(string relatedColumnValue)
        {
            relatedColumnValue = HttpUtility.UrlDecode(relatedColumnValue);
            relatedColumnValue = relatedColumnValue.Replace("&#58;", ":");

            List<RMRelatedItemInfo> infos = new List<RMRelatedItemInfo>();
            if (!string.IsNullOrEmpty(relatedColumnValue))
            {
                var columnValue = relatedColumnValue;

                XmlDocument xmlDoc = new XmlDocument();
                //columnValue = HttpUtility.UrlDecode(columnValue); error: "+"->" "
                columnValue = columnValue.Replace("&#58;", ":");
                columnValue = columnValue.Replace("&", "&amp;").Replace("amp;amp;", "amp;");
                xmlDoc.LoadXml(columnValue);
                //每一个Related 的item 真实属性都记录在<a> 标签中
                foreach (var ele in xmlDoc.GetElementsByTagName("a"))
                {
                    XmlElement element = ele as XmlElement;
                    var relatedObjString = element.GetAttribute("rel");
                    relatedObjString = HttpUtility.HtmlDecode(relatedObjString);
                    RMRelatedItemInfo relatedObj = SerializerHelper.DeserializeByJsonSerializer<RMRelatedItemInfo>(relatedObjString);
                    var relatedItemUrl = element.GetAttribute("href");
                    //string url = string.Empty;
                    //if (!element.GetAttribute("href").StartsWith(relatedObj.SiteUrl))//parmDic["SiteUrl"]))
                    //{
                    //    var webServerRelativeUrl = currentWeb.ServerRelativeUrl;
                    //    url = element.GetAttribute("href").Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
                    //    url = relatedObj.SiteUrl + "/" + url;
                    //}
                    //relatedObj.url = relatedItemUrl;
                    infos.Add(relatedObj);
                }
            }
            return infos;
        }


        private RAReturnMessage Fail(RAReturnMessage rst, string msg)
        {
            rst.MessageType = RAMessageType.Failed;
            rst.ErrorMessage = msg;
            return rst;
        }



        class UpdateRelatedRecordParams
        {
            public List<RMRelatedItemInfo> SourceRelatedInfos { get; set; }
            public List<RMRelatedItemInfo> DestRelatedInfos { get; set; }
            public RMRelatedItemInfo DesInfo { get; set; }
            public RMRelatedItemInfo SourceInfo { get; set; }

            public UpdateRelatedRecordParams()
            {
                SourceRelatedInfos = new List<RMRelatedItemInfo>();
                DestRelatedInfos = new List<RMRelatedItemInfo>();
                DesInfo = new RMRelatedItemInfo();
                SourceInfo = new RMRelatedItemInfo();
            }
        }
    }

    public enum SourceFlag
    {
        None = -1,
        All = 0,
        SharePoint = 1,
        FileSystem = 2,
        Exchange = 3,
        Physical = 4,
        SharePointOnPrem = 5,
        OneDrive = 6,
        AzureFileShare = 7,
        Box = 8,
        Google = 9,
        SalesForce = 10,
        Teams = 11,
        Groups = 12,
        LifecycleRetention = 99,
        Connector = 999
    }

    internal enum HoldAndRecordStatusMask
    {
        EditBlockedMask = 1, //只要不允许编辑, 这位值就为1, 包括Hold 和 Block edit and delete
        RecordMask = 0x10, //Record 文件，这位值 就是1 ， 包含Block edit and delete， block delete
        DeleteBlockedMask = 0x100,//只要不允许删除，这位值就为1, 包括 Hold， block edit and delete， block delete
        HoldMask = 0x1000, //Hold 文件，这位值就是 1， 
    }
}
