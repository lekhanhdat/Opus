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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Utility;
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
//using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.RACommonUtility.SharePointOnPrem;
using AvePoint.RA.RADataBroker;
using AvePoint.Records.Core.Utilities.Extensions;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using ArgumentCheck = AvePoint.Wrapper.Common.ArgumentCheck;

namespace AvePoint.RA.RACommonUtility
{
    public class RelatedRecordsUtility : IDisposable
    {
        private static RALogger logger = RALogger.GetInstance(typeof(RelatedRecordsUtility));
        #region RelatedItem 与SP 相关的column 对应的属性
        private const string relatedColumnInternalName = "RecordsRelated";
        private const string columnHeader = "<div class=\"ExternalClass702E44874C854B099A61564528AE0439\">{0}</div>";
        private const string categoryHeader = "<p style=\"font-weight:600\">{0}</p>";
        private const string columnStrcuture = "<p style=\"margin: 0px 0px 5px 0px;\">​<a rel =\"{0}\" href=\"{1}\" style=\"{3}\">{2}</a>​</p>";
        private const string physicalRelatedStyle = "color: black;text-decoration: none;";
        private Guid relatedColumnId = new Guid("b40273fb-26d2-40e8-9a34-dd20bc9ca1d7");
        #endregion

        private List<RemoteSiteCollection> mRemoteSiteCollectionCache = null;

        [ThreadStatic]
        private static Dictionary<Guid, CachedSiteContext> siteContextDic;

        private bool BatchOperation = false; //是否批量操作
        private readonly object mlock = new object();

        //目前此工具类放开Cosmos DB 的限制，以后直接使用即可
        //TODO 稍后会去掉让外围更新DB 的逻辑，放到方法内部维护
        private IExplorerDao mExplorerDao;
        public IExplorerDao ExplorerDao
        {
            get
            {
                if (mExplorerDao == null)
                {
                    mExplorerDao = new ExplorerDao();
                }
                return mExplorerDao;
            }
        }

        private IAveORecords Record;
        private string accesstoken { get; set; }
        private string userSid { get; set; }
        private string siteUrl { get; set; }
        private ClientContext currentContext { get; set; }
        //private string currentItemUrl { get; set; }
        private Guid currentListId { get; set; }
        private int currentItemId { get; set; }
        private List currentList { get; set; }
        private ListItem currentItem { get; set; }
        private Web currentWeb { get; set; }
        private Site currentSite { get; set; }
        public string folderUrl { get; set; }

        //#region 后台应用特有的属性
        //private Record currentRecord = null;
        //#endregion

        #region Structure
        public RelatedRecordsUtility()
        {
            siteContextDic = new Dictionary<Guid, CachedSiteContext>();
        }
        /// <summary>
        /// 为SP以及SP APP端直接调用提供的构造函数
        /// </summary>
        /// <param name="hosturl"></param>
        /// <param name="accesstoken"></param>
        /// <param name="listId"></param>
        /// <param name="itemId"></param>
        public RelatedRecordsUtility(string hosturl, string accesstoken, Guid listId, int itemId, string userSid = "")
        {
            using (new PerformanceScope("RelatedRecordsController--RelatedRecordsUtility--ctor"))
            {
                this.accesstoken = accesstoken;
                this.userSid = userSid;
                siteUrl = hosturl;
                currentContext = CommonClientContext.GetClientContextWithAccessToken(hosturl, accesstoken);
                currentSite = currentContext.Site;
                currentContext.Load(currentSite, s => s.Url, s => s.Id);
                currentContext.ExecuteQuery();
                if (currentSite.Url.Equals(hosturl))
                {
                    currentWeb = currentSite.RootWeb;
                }
                else
                {
                    currentWeb = currentContext.Web;
                }
                currentContext.Load(currentWeb);
                currentContext.ExecuteQuery();
                var user = currentWeb.CurrentUser;
                currentContext.Load(user);
                currentContext.ExecuteQuery();
                logger.Info("Current User is {0}{1}", siteUrl, user.Id);//Replace user name to ID
                currentListId = listId;
                currentItemId = itemId;
                GetCurrentItem();
            }
        }
        /// <summary>
        /// 提供给后台对象的构造函数，实例化此对象
        /// </summary>
        /// <param name="record">Cosmos DB 中的Record 对象</param>
        public RelatedRecordsUtility(Record record, bool batchOperation = false)
        {
            //currentRecord = record;
            this.BatchOperation = batchOperation;
            //if (!batchOperation)
            //{
            siteContextDic = new Dictionary<Guid, CachedSiteContext>();
            //}
            if (record.SourceFlag == (int)SourceFlag.SharePoint)
            {
                InitSharePointInfo(record.AveSiteId, record.WebId, record.ListId, record.ItemRowId);
            }
        }

        public RelatedRecordsUtility(string siteUrl, Guid webId, Guid listId, int itemId)
        {
            siteContextDic = new Dictionary<Guid, CachedSiteContext>();
            currentContext = this.GetSiteContext(siteUrl);
            currentSite = currentContext.Site;
            currentContext.Load(currentSite, s => s.Url, s => s.Id, s => s.RootWeb);
            currentContext.ExecuteQuery();
            if (currentSite.RootWeb.Id.Equals(webId))
            {
                currentWeb = currentSite.RootWeb;
            }
            else
            {
                currentWeb = currentSite.OpenWebById(webId);
            }
            currentContext.Load(currentWeb);
            currentContext.ExecuteQuery();
            currentListId = listId;
            currentItemId = itemId;
            GetCurrentItem();
        }
        #endregion
        private void InitSharePointInfo(string aveId, Guid webId, Guid listId, int itemId)
        {
            currentContext = this.GetSiteContext(new Guid(aveId)); //this.InitContext(new Guid(aveId));
            currentSite = currentContext.Site;
            currentContext.Load(currentSite, s => s.Url, s => s.Id, s => s.RootWeb);
            currentContext.ExecuteQuery();
            if (currentSite.RootWeb.Id.Equals(webId))
            {
                currentWeb = currentSite.RootWeb;
            }
            else
            {
                //currentWeb = currentContext.Web;
                currentWeb = currentSite.OpenWebById(webId);
            }
            currentContext.Load(currentWeb);
            currentContext.ExecuteQuery();
            currentListId = listId;
            currentItemId = itemId;
            GetCurrentItem();
        }
        public ListItem GetCurrentItem(Guid? listId = null, int? itemId = null)
        {
            if (listId != null && itemId != null)
            {
                currentListId = (Guid)listId;
                currentItemId = (int)itemId;
            }

            currentContext.Load(currentWeb, w => w.Lists, w => w.Url, w => w.ServerRelativeUrl);//remove Load Fields
            currentContext.ExecuteQuery();
            currentList = currentWeb.Lists.AsQueryable().Where(l => l.Id.Equals(currentListId)).FirstOrDefault();
            currentContext.Load(currentList);
            currentContext.ExecuteQuery();
            #region debug code

            //AddRelatedColumn();
            //AddRelatedColumnTolist();
            #endregion
            currentItem = currentList?.GetItemById(currentItemId);
            currentContext.Load(currentItem);
            currentContext.ExecuteQuery();
            try
            {
                folderUrl = currentItem?.FieldValues["FileDirRef"].ToString();
                var webServerRelativeUrl = currentWeb.ServerRelativeUrl;
                folderUrl = folderUrl?.Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
                folderUrl = currentWeb.Url + "/" + folderUrl;
            }
            catch (Exception e)
            {
                logger.Warn("Get navigation url failed {0}", e.ToString());
            }
            return currentItem;
        }
        public string GetCurrentItemName()
        {
            try
            {
                return currentItem.FieldValues["FileLeafRef"].ToString();
            }
            catch (Exception)
            {
                return currentItem.DisplayName;
            }
        }
        public bool CheckCurrentListEnableApp()
        {
            try
            {
                Field metadataField = currentList.Fields.GetById(relatedColumnId);
                currentContext.Load(metadataField);
                currentContext.ExecuteQueryWithIncrementalRetry(3, 1);
                return true;
            }
            catch (Exception ex)
            {
                logger.Info("List not enable app {0}", ex.ToString());
            }
            return false;
        }
        public string GetTenantId()
        {
            Func<string> getObj = () =>
            {
                string result = null;
                var p = currentContext.Web.AllProperties;
                currentContext.Load(p);
                currentContext.ExecuteQuery();
                if (p.FieldValues.ContainsKey("RelatedId"))
                {
                    logger.Info("get related id from {0}", siteUrl);
                    result = p["RelatedId"]?.ToString();
                }
                return result;
            };
            return getObj();

        }
        private List<UpdateRelatedRecordParams> CloneList(IEnumerable<UpdateRelatedRecordParams> src)
        {
            List<UpdateRelatedRecordParams> result = new List<UpdateRelatedRecordParams>();
            foreach (UpdateRelatedRecordParams srcString in src)
            {
                result.Add(srcString);
            }
            return result;
        }
        /// <summary>
        /// 用于Rebuild Relatedship Tool， 批量操作
        /// 基本操作顺序：1. remove related： 删除关联文件的related info中当前文件的关联内容; 然后删除当前文件的related info 中关联文件的内容。
        ///2. add related： 在关联文件中添加当前文件的related info；然后在当前文件中添加关联文件的related info。 
        ///3.如果是SP ，更新SP 中的属性
        ///4.返回一对一的RelateInfos
        /// </summary>
        /// <param name="infos">需要更新的目的端 items 信息</param>
        /// <param name="allRecords">所以参与related 文件的cosmos db 对象,可以在当前Class 中提供获取方法，此处可以调整外围是否传递List<Record> 对象</param>
        public Dictionary<UpdateRelatedRecordParams, string> UpdateRelatedPropertiesForTool(Record sourceRecord, List<RMRelatedItemInfo> infos, List<Record> allRecords)
        {
            Dictionary<UpdateRelatedRecordParams, string> result = new Dictionary<UpdateRelatedRecordParams, string>();
            string sourceUpdateValue = sourceRecord.RelatedRecords ?? string.Empty;
            //原端对象当前的RelatedInfos 集合，会随着下面的foreach 处理增加或者删除
            List<RMRelatedItemInfo> sourceObjRelatedInfoCollection = GetRelatedProperties(sourceRecord);
            //将原端对象，生成RMRelatedInfo，方便各个关联record 进行更新等操作
            RMRelatedItemInfo sourceRelatedInfo = GenerateRMRelatedItemInfo(sourceRecord);
            bool hasFailedRecords = false;
            foreach (RMRelatedItemInfo related in infos)
            {
                var destRecord = allRecords.Find(r => r.NodeId == related.id);
                //如果DB 继续跟SP 不一致，此处应该通过SP 获取
                var destRelatedInfos = GetRelatedProperties(destRecord);
                UpdateRelatedRecordParams param = new UpdateRelatedRecordParams()
                {
                    SourceInfo = sourceRelatedInfo,
                    DesInfo = related,
                    SourceRelatedInfos = sourceObjRelatedInfoCollection,
                    DestRelatedInfos = destRelatedInfos
                };
                bool updateFailed = false;
                List<string> disableAppFile = new List<string>();
                List<RMRelatedItemInfo> destResult = UpdateDestinationRelatedRecord(true, ref updateFailed, disableAppFile, ref param);
                if (!updateFailed)
                {
                    this.UpdateRecordRelatedInfo(param.DesInfo.id, destResult);
                }
                else
                {
                    logger.Warn("Update related column failed, no need to update explorer");
                }
                sourceObjRelatedInfoCollection = param.SourceRelatedInfos;
                logger.Info("update related record column and explorer db successfully. src {0}, related {1}", sourceRecord.RecordsId, related.SourceFlag == (int)SourceFlag.Physical ? related.recId : related.ItemUrl);
                if (related.SourceFlag == (int)SourceFlag.Physical || sourceRelatedInfo.SourceFlag == (int)SourceFlag.Physical)
                {
                    ArgumentCheck.CheckNotNull(destRecord);
                    logger.Info("Append phy src id {0},  dest id {1}", sourceRecord?.Id, destRecord?.Id);
                    param.DesInfo.name = destRecord?.LeafName;
                    param.DesInfo.recId = destRecord?.RecordsId;
                    param.SourceInfo.name = sourceRecord?.LeafName;
                }

                if (updateFailed)
                {
                    hasFailedRecords = true;
                    if (disableAppFile.Count == 0)
                    {
                        result.Add(param, "Failed to update Related Records");
                    }
                    else
                    {
                        result.Add(param, "The related records column doesn't exist in related record's SP list.");
                    }
                }
                else
                {
                    result.Add(param, "");
                }
            }

            if (hasFailedRecords)
            {
                logger.Info("related record has failed result, no need to update source record {0}", sourceRecord.RecordsId);
                return result;
            }

            #region 获取source 的最终 relatedInfos， 如果source 是SP ，更新SP item 属性
            try
            {
                //理论上currentRecord 的sourceflag 是准的，添加 0 判断只为了代码健壮,０是SP老数据，老数据中可能没有存
                if (sourceRecord.SourceFlag == (int)SourceFlag.SharePoint || sourceRecord.SourceFlag == (int)SourceFlag.All)
                {
                    var sourceFinalRelatedValue = ConvertToSPColumnValueString(sourceObjRelatedInfoCollection);
                    if (!sourceUpdateValue.Equals(sourceFinalRelatedValue, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!sourceFinalRelatedValue.Contains("href"))
                        {
                            sourceFinalRelatedValue = string.Empty;
                        }
                        IAveListItem aveListItem = this.GetRelatedItem(sourceRecord);
                        if (aveListItem == null)
                        {
                            throw new Exception("Query source item failed.");
                        }
                        if (!aveListItem.FieldValues.ContainsKey(relatedColumnInternalName))
                        {
                            logger.Warn("Source record {0} does not contains related column.", sourceRecord.RecordsId);
                            List<UpdateRelatedRecordParams> keys = CloneList(result.Keys);
                            foreach (UpdateRelatedRecordParams key in keys)
                            {
                                result[key] = "The related records column doesn't exist in source record's SP list.";
                            }
                            return result;
                        }
                        this.UpdateSPItemRelatedProperties(aveListItem, sourceObjRelatedInfoCollection);

                    }
                }
                this.UpdateRecordRelatedInfo(sourceRecord.NodeId, sourceObjRelatedInfoCollection);
            }
            catch (Exception se)
            {
                logger.Warn("submit related records failed {0}:{1}:{2}", currentItem["FileRef"].ToString(), sourceUpdateValue, se.ToString());
                List<UpdateRelatedRecordParams> keys = CloneList(result.Keys);
                foreach (UpdateRelatedRecordParams key in keys)
                {
                    result[key] = string.Format("Update source record related column failed. {0}", se.Message);
                }
            }
            #endregion
            return result;
        }
        /// <summary>
        /// 更新Related文件的时候，需要即更新source ，又更新destination。 此方法提供更新source 和destination 的功能
        /// 基本操作顺序：1. remove related： 删除关联文件的related info中当前文件的关联内容; 然后删除当前文件的related info 中关联文件的内容。
        ///2. add related： 在关联文件中添加当前文件的related info；然后在当前文件中添加关联文件的related info。 
        ///3.如果是SP ，更新SP 中的属性
        ///4.返回当前文件更新后的RelateInfos
        /// </summary>
        /// <param name="infos">需要更新的目的端 items 信息</param>
        /// /// <param name="allRecords">所以参与related 文件的cosmos db 对象,可以在当前Class 中提供获取方法，此处可以调整外围是否传递List<Record> 对象</param>
        public List<RMRelatedItemInfo> UpdateRelatedPropertiesForExplorer(Record sourceRecord, List<RMRelatedItemInfo> infos, List<Record> allRecords)
        {
            var result = new List<RMRelatedItemInfo>();
            var sourceUpdateValue = sourceRecord.RelatedRecords ?? string.Empty;
            //原端对象当前的RelatedInfos 集合，会随着下面的foreach 处理增加或者删除
            var sourceObjRelatedInfoCollection = GetRelatedProperties(sourceRecord);
            //将原端对象，生成RMRelatedInfo，方便各个关联record 进行更新等操作
            var sourceRelatedInfo = GenerateRMRelatedItemInfo(sourceRecord);
            bool hasFailedRecords = false;
            List<string> disableAppFile = new List<string>();
            foreach (var desInfo in infos)
            {
                var destRecord = allRecords.Find(r => r.NodeId == desInfo.id);
                //如果DB 继续跟SP 不一致，此处应该通过SP 获取
                var destRelatedInfos = GetRelatedProperties(destRecord);
                var param = new UpdateRelatedRecordParams()
                {
                    SourceInfo = sourceRelatedInfo,
                    DesInfo = desInfo,
                    SourceRelatedInfos = sourceObjRelatedInfoCollection,
                    DestRelatedInfos = destRelatedInfos
                };
                var destResult = UpdateDestinationRelatedRecord(true, ref hasFailedRecords, disableAppFile, ref param);
                if (!hasFailedRecords)
                {
                    this.UpdateRecordRelatedInfo(param.DesInfo.id, destResult);
                }
                else
                {
                    logger.Warn("Update related column failed, no need to update explorer");
                }
                sourceObjRelatedInfoCollection = param.SourceRelatedInfos;
            }

            if (hasFailedRecords)
            {
                if (disableAppFile.Count == 0)
                {
                    throw new Exception("Related Records has failed Records");
                }
                else
                {
                    throw new RelatedRecordsAppDisableExcetion(string.Format(I18N.Core.I18NEntity.GetString("RM_Explorer_Related_RelatedRecordsDisableApp"), string.Join(", ", disableAppFile)));
                }
            }

            #region 获取source 的最终 relatedInfos， 如果source 是SP ，更新SP item 属性
            try
            {
                result = sourceObjRelatedInfoCollection;
                //理论上currentRecord 的sourceflag 是准的，添加 0 判断只为了代码健壮,０是SP老数据，老数据中可能没有存
                if (sourceRecord.SourceFlag == (int)SourceFlag.SharePoint || sourceRecord.SourceFlag == (int)SourceFlag.All)
                {
                    var sourceFinalRelatedValue = ConvertToSPColumnValueString(sourceObjRelatedInfoCollection);
                    if (!sourceUpdateValue.Equals(sourceFinalRelatedValue, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!sourceFinalRelatedValue.Contains("href"))
                        {
                            sourceFinalRelatedValue = string.Empty;
                        }
                        var aveListItem = this.GetRelatedItem(sourceRecord);
                        this.UpdateSPItemRelatedProperties(aveListItem, sourceObjRelatedInfoCollection);
                    }
                }
                else if (sourceRecord.SourceFlag == (int)SourceFlag.SharePointOnPrem)
                {
                    var sourceFinalRelatedValue = ConvertToSPColumnValueString(sourceObjRelatedInfoCollection);
                    if (!sourceUpdateValue.Equals(sourceFinalRelatedValue, StringComparison.OrdinalIgnoreCase))
                    {
                        SharePointOnPremClient.UpdateSPItemRelatedProperties(
                            sourceRelatedInfo.SiteUrl,
                            Guid.Empty,
                            sourceRelatedInfo.WebId,
                            sourceRelatedInfo.WebUrl,
                            sourceRelatedInfo.ListId,
                            sourceRelatedInfo.DocLibRowId,
                            sourceRelatedInfo.name,
                            sourceFinalRelatedValue).GetAwaiter().GetResult();
                    }
                }
                this.UpdateRecordRelatedInfo(sourceRecord.NodeId, sourceObjRelatedInfoCollection);
            }
            catch (Exception se)
            {
                logger.Warn("submit related records failed {0}:{1}:{2}", currentItem["FileRef"].ToString(), sourceUpdateValue, se.ToString());
                throw;
            }
            #endregion
            return result;
        }
        public (string, List<RMRelatedItemInfo>, List<RMRelatedItemInfo>) UpdateRelatedPropertiesForApp(List<RMRelatedItemInfo> infos, List<RelatedItemSubmitInfo> deletedItemInfos, bool isSpfxApp = false)
        {
            var result = new List<RMRelatedItemInfo>();
            string sourceUrlValue = currentItem[relatedColumnInternalName] != null ? currentItem[relatedColumnInternalName].ToString() : string.Empty;
            var sourceUpdateValue = HttpUtility.UrlDecode(sourceUrlValue);
            sourceUpdateValue = sourceUpdateValue.Replace("&#58;", ":");
            var orignalInfos = GetRelatedPropertiesBySPColumnValue(sourceUrlValue);
            List<RMRelatedItemInfo> sourceRelatedInfos = GetRelatedPropertiesBySPColumnValue(sourceUrlValue);
            if (CheckIsRecord())
            {
                throw new Exception($"Current item is declared record {currentItem.DisplayName}");
            }
            var sourceRelatedInfo = GenerateRelatedItemInfoForSP();
            bool hasFailedRecords = false;
            List<string> disableAppFile = new List<string>();
            foreach (var desInfo in infos)
            {
                if (string.Equals(desInfo?.ItemUrl, sourceRelatedInfo?.ItemUrl, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Can't relate to oneself");
                }
            }

            foreach (var desInfo in infos)
            {
                List<RMRelatedItemInfo> destRelatedInfos = new List<RMRelatedItemInfo>();
                var destListItem = GetRelatedItem(desInfo);
                if (destListItem != null)
                {
                    destRelatedInfos = GetRelatedProperties(destListItem);
                }
                var param = new UpdateRelatedRecordParams()
                {
                    SourceInfo = sourceRelatedInfo,
                    DesInfo = desInfo,
                    SourceRelatedInfos = sourceRelatedInfos,
                    DestRelatedInfos = destRelatedInfos
                };
                var destResult = UpdateDestinationRelatedRecord(isSpfxApp, ref hasFailedRecords, disableAppFile, ref param);
                //为了代码健壮,此处添加强赋值逻辑，理论上如果sourceRelatedInfos 是集合对象，此处就可能不需要赋值，但是如果是空，就可能需要有赋值行为。
                sourceRelatedInfos = param.SourceRelatedInfos;
            }

            if (hasFailedRecords)
            {
                if (disableAppFile.Count == 0)
                {
                    throw new Exception("Related Records has failed Records");
                }
                else
                {
                    throw new RelatedRecordsAppDisableExcetion(string.Format(I18N.Core.I18NEntity.GetString("RM_Explorer_Related_RelatedRecordsDisableApp"), string.Join(", ", disableAppFile)));
                }
            }

            #region 获取source 的最终 relatedInfos， 如果source 是SP ，更新SP item 属性
            try
            {
                if(deletedItemInfos != null && deletedItemInfos.Count > 0)
                {
                    sourceRelatedInfos = sourceRelatedInfos.Where(r => !deletedItemInfos.Any(d => d.UniqueId == r.id)).ToList();
                }
                result = sourceRelatedInfos;
                //理论上currentRecord 的sourceflag 是准的，添加 0 判断只为了代码健壮,０是SP老数据，老数据中可能没有存
                var sourceFinalRelatedValue = ConvertToSPColumnValueString(sourceRelatedInfos);
                if (!sourceUpdateValue.Equals(sourceFinalRelatedValue, StringComparison.OrdinalIgnoreCase))
                {
                    if (!sourceFinalRelatedValue.Contains("href"))
                    {
                        sourceFinalRelatedValue = string.Empty;
                    }
                    UpdateSPItemRelatedProperties(currentContext, currentItem, sourceFinalRelatedValue);
                }
            }
            catch (Exception se)
            {
                logger.Warn("submit related records failed {0}:{1}:{2}", currentItem["FileRef"].ToString(), sourceUpdateValue, se.ToString());
                throw;
            }
            #endregion
            var displayName = string.Empty;
            if ((currentItem.FieldValues["FileLeafRef"] as string).EndsWith("_.000", StringComparison.OrdinalIgnoreCase))
            {
                displayName = currentItem.FieldValues["Title"] as string;
                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = "";
                }
            }
            else
            {
                displayName = currentItem.FieldValues["FileLeafRef"].ToString();
            }
            return (displayName, orignalInfos, result);//for auditor
        }
        private List<RMRelatedItemInfo> UpdateDestinationRelatedRecord(bool forRECOExplorer, ref bool hasFailedRecords, List<string> disableAppFile, ref UpdateRelatedRecordParams param)
        {
            var result = new List<RMRelatedItemInfo>();
            if (param.DesInfo.SourceFlag == (int)SourceFlag.SharePointOnPrem && param.DesInfo.DeclareAsRecord)
            {
                logger.Info($"Skip updating related records for declared SharePoint On-Prem item. ItemId:{param.DesInfo.DocLibRowId}, UniqueId:{param.DesInfo.id}");
                return param.DestRelatedInfos;
            }

            AddOrRemoveRelatedInfos(ref param);
            result = param.DestRelatedInfos;
            #region update sp value
            //0是SP老数据，老数据中可能没有存
            if (param.DesInfo.SourceFlag == (int)SourceFlag.SharePoint || param.DesInfo.SourceFlag == (int)SourceFlag.All)
            {
                //Update destination if type is SP
                ClientContext destContext = null;
                Web destWeb = null;
                List destList = null;
                ListItem destItem = null;
                try
                {
                    try
                    {
                        if (forRECOExplorer)
                        {
                            if (string.IsNullOrEmpty(param.DesInfo.AveId))
                            {
                                destContext = this.GetSiteContext(param.DesInfo.SiteUrl);
                            }
                            else
                            {
                                destContext = this.GetSiteContext(new Guid(param.DesInfo.AveId));// this.InitContext(new Guid(param.DesInfo.AveId));
                            }
                        }
                        else
                        {
                            destContext = CommonClientContext.GetClientContextWithAccessToken(param.DesInfo.WebUrl, accesstoken);
                        }
                        destWeb = destContext.Site.OpenWebById(param.DesInfo.WebId);
                        destContext.Load(destWeb, w => w.Lists, w => w.Url, w => w.Id, w => w.ServerRelativeUrl);
                        destList = destWeb.Lists.GetById(param.DesInfo.ListId);
                        destContext.Load(destList);
                        destContext.ExecuteQuery();
                        logger.Info("SPRelated get list successfully. ListId:{0}", param?.DesInfo?.ListId);
                        try
                        {
                            Field metadataField = destList.Fields.GetById(relatedColumnId);
                            destContext.Load(metadataField);
                            destContext.ExecuteQuery();
                            logger.Info("SPRelated load metadata successfully.");
                        }
                        catch (WebException ex)
                        {
                            hasFailedRecords = true;
                            logger.Error("load related column error {0}", ex);
                            var response = ex.Response as HttpWebResponse;
                            // Check if request was throttled - http status code 429
                            // Check is request failed due to server unavailable - http status code 503
                            if (response != null && (response.StatusCode == (HttpStatusCode)429 || response.StatusCode == (HttpStatusCode)503))
                            {
                                // 429 or 503, not sure if the related column exists or not
                            }
                            else
                            {
                                disableAppFile.Add(param.DesInfo.name);
                            }
                            throw ex;
                        }
                        catch (Exception)
                        {
                            hasFailedRecords = true;
                            disableAppFile.Add(param.DesInfo.name);
                            throw;
                        }

                        destItem = destList.GetItemById(param.DesInfo.DocLibRowId);
                        destContext.Load(destItem);
                        destContext.ExecuteQuery();
                        logger.Info("SPRelated get item successfully. ItemId:{0}", param?.DesInfo?.DocLibRowId);
                    }
                    catch (Exception exp)
                    {
                        hasFailedRecords = true;
                        logger.Warn($"Error in get SharePoint item, ave id {param.DesInfo.AveId}, web id : {param.DesInfo.WebId}, list id: {param.DesInfo.ListId}, item id : {param.DesInfo.DocLibRowId}. reason : {exp.ToString()}");
                        return result;
                    }

                    var urlValue = destItem[relatedColumnInternalName] != null ? destItem[relatedColumnInternalName].ToString() : string.Empty;
                    var updateValue = ConvertToSPColumnValueString(param.DestRelatedInfos);
                    if (!urlValue.Equals(updateValue, StringComparison.OrdinalIgnoreCase))
                    {
                        UpdateSPItemRelatedProperties(destContext, destItem, updateValue);
                        logger.Info("SPRelated update item successfully. ItemId:{0}", param?.DesInfo?.DocLibRowId);
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("add related record failed {0}:{1}", param.DesInfo.url, ex.ToString());
                    hasFailedRecords = true;
                }
                finally
                {
                    if (!forRECOExplorer)
                    {
                        destContext?.Dispose();
                    }
                }
            }

            if (param.DesInfo.SourceFlag == (int)SourceFlag.SharePointOnPrem)
            {
                var updateValue = ConvertToSPColumnValueString(param.DestRelatedInfos);
                SharePointOnPremClient.UpdateSPItemRelatedProperties(
                    param.DesInfo.SiteUrl,
                    param.DesInfo.SiteId,
                    param.DesInfo.WebId,
                    param.DesInfo.WebUrl,
                    param.DesInfo.ListId,
                    param.DesInfo.DocLibRowId,
                    param.DesInfo.name,
                    updateValue
                ).GetAwaiter().GetResult();
            }
            #endregion
            return result;
        }
        /// <summary>
        /// 更新关联Item 的RelatedColumn,用于文件Move 等操作引起的Related 属性变化。此方法负责更新，不负责增加和删除
        /// </summary>
        /// <param name="relatedItemInfoBeforeMove">Move之前文件的RelatedItemInfo， 通过这个对象可以获取到关联的Item</param>
        /// <param name="siteUrlBeforeMove">Move之前文件的site url</param>
        /// <param name="itemUrlBeforeMove">Move之前文件的item url, 支持server related url  和 full url</param>
        /// 通过site url 和item url ，能从关联文件中找到与原文件的关联记录，进而可以更新这条信息
        /// <param name="relatedInfoAfterMove">原文件被move到目的端后，目的端文件的属性，用于更新关联文件</param>
        /// <param name="relatedItemAccountInfo">关联文件站点对应的注册信息，用于连接关联文件站点，进而更新关联文件</param>
        public void UpdateRelateColumnValue(RMRelatedItemInfo relatedItemInfoBeforeMove, string siteUrlBeforeMove, string itemUrlBeforeMove, Guid itemId, RMRelatedItemInfo relatedInfoAfterMove)
        {
            var relatedItem = GetRelatedItem(relatedItemInfoBeforeMove);
            var relatedProperties = GetRelatedProperties(relatedItem);
            //Find the right RMRelatedItemInfo, remove the old one, and the new one
            //此处可能需要额外在加一个Id 相同的判断，防止ID 改变的case。
            if (relatedProperties == null)
            {
                relatedProperties = new List<RMRelatedItemInfo>();
            }
            relatedProperties.RemoveAll(r => siteUrlBeforeMove.Equals(r.SiteUrl, StringComparison.OrdinalIgnoreCase) &&
            (itemUrlBeforeMove.Equals(r.ItemUrl, StringComparison.OrdinalIgnoreCase)
            || itemUrlBeforeMove.Equals(r.url, StringComparison.OrdinalIgnoreCase)
            || itemId == r.id));
            relatedProperties.Add(relatedInfoAfterMove);
            UpdateSPItemRelatedProperties(relatedItem, relatedProperties);
            this.UpdateRecordRelatedInfo(relatedItemInfoBeforeMove.id, relatedProperties);
        }
        /// <summary>
        /// 更新关联Item 的RelatedColumn,用于文件Move 等操作引起的Related 属性变化。此方法负责更新，不负责增加和删除
        /// </summary>
        /// <param name="relatedItemInfoBeforeMove">Move之前文件的RelatedItemInfo， 通过这个对象可以获取到关联的Item</param>
        /// <param name="siteUrlBeforeMove">Move之前文件的site url</param>
        /// <param name="itemUrlBeforeMove">Move之前文件的item url, 支持server related url  和 full url</param>
        /// 通过site url 和item url ，能从关联文件中找到与原文件的关联记录，进而可以更新这条信息
        /// <param name="relatedInfoAfterMove">原文件被move到目的端后，目的端文件的属性，用于更新关联文件</param>
        /// <param name="relatedItemAccountInfo">关联文件站点对应的注册信息，用于连接关联文件站点，进而更新关联文件</param>
        /// <param name="allRecords">所有需要操作的Record 信息，用来提升性能，后期可以调整成方法内重新获取</param>
        public void UpdateRelateColumnValuePhysical(RMRelatedItemInfo relatedItemInfoBeforeMove, string siteUrlBeforeMove, string itemUrlBeforeMove, Guid itemId, RMRelatedItemInfo relatedInfoAfterMove, List<Record> allRecords)
        {
            var relatedProperties = GetRelatedPropertiesByDB(allRecords.Where(r => r.Id == relatedItemInfoBeforeMove.id).First().RelatedRecords);
            //Find the right RMRelatedItemInfo, remove the old one, and the new one
            relatedProperties.RemoveAll(r => siteUrlBeforeMove.Equals(r.SiteUrl, StringComparison.OrdinalIgnoreCase) &&
            (itemUrlBeforeMove.Equals(r.ItemUrl, StringComparison.OrdinalIgnoreCase) ||
            itemUrlBeforeMove.Equals(r.url, StringComparison.OrdinalIgnoreCase)
            || itemId == r.id));
            relatedProperties.Add(relatedInfoAfterMove);
            this.UpdateRecordRelatedInfo(relatedItemInfoBeforeMove.id, relatedProperties);
        }

        /// <summary>
        /// 更新关联Item 的RelatedColumn 
        /// </summary>
        /// <param name="relatedItemInfoBeforeMove">Move之前文件的RelatedItemInfo， 通过这个对象可以获取到关联的Item</param>
        /// <param name="siteUrlBeforeMove">Move之前文件的site url</param>
        /// <param name="itemUrlBeforeMove">Move之前文件的item url, 支持server related url  和 full url</param>
        /// 通过site url 和item url ，能从关联文件中找到与原文件的关联记录，进而可以更新这条信息
        /// <param name="relatedInfoAfterMove">原文件被move到目的端后，目的端文件的属性，用于更新关联文件</param>
        /// <param name="relatedItemAccountInfo">关联文件站点对应的注册信息，用于连接关联文件站点，进而更新关联文件</param>
        public void UpdateSPRelatedSPColumnValue(RMRelatedItemInfo relatedItemInfoBeforeMove, string siteUrlBeforeMove, string itemUrlBeforeMove, RMRelatedItemInfo relatedInfoAfterMove, string relatedItemAccountInfo)
        {
            var relatedItem = GetRelatedItem(relatedItemInfoBeforeMove);
            if (relatedItem == null)
            {
                return;
            }
            var relatedProperties = GetRelatedProperties(relatedItem);
            //Find the right RMRelatedItemInfo, remove the old one, and the new one
            relatedProperties.RemoveAll(r => r.SourceFlag == (int)SourceFlag.SharePoint && r.SiteUrl.Equals(siteUrlBeforeMove, StringComparison.OrdinalIgnoreCase) && (r.ItemUrl.Equals(itemUrlBeforeMove, StringComparison.OrdinalIgnoreCase) || r.url.Equals(itemUrlBeforeMove, StringComparison.OrdinalIgnoreCase)));
            relatedProperties.Add(relatedInfoAfterMove);
            UpdateSPItemRelatedProperties(relatedItem, relatedProperties);
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

        private void UpdateSPItemRelatedProperties(ClientContext ctx, ListItem item, string relatedValue)
        {
            item[relatedColumnInternalName] = relatedValue;
            item.Update();
            ctx.ExecuteQuery();
        }
        public void UpdateSPItemRelatedProperties(IAveListItem item, List<RMRelatedItemInfo> relatedItemInfos)
        {
            if (item != null)
            {
                try
                {
                    var columnValue = ConvertToSPColumnValueString(relatedItemInfos);
                    item[relatedColumnInternalName] = columnValue;
                    item.SystemUpdate();
                }
                catch (Exception ex)
                {
                    logger.Warn(string.Format("Error in update realted properties for item : {0}, reason : {1}", item["FileRef"].ToString(), ex.ToString()));
                    throw;
                }
            }
        }

        public void UpdateSPRelatedPhysicalColumnValue(RMRelatedItemInfo relatedItemInfoBeforeMove, string siteUrlBeforeMove, string itemUrlBeforeMove, RMRelatedItemInfo relatedInfoAfterMove)
        {
            Record record = ExplorerDao.ReadById(Guid.Empty, relatedItemInfoBeforeMove.id);
            List<RMRelatedItemInfo> relatedItemInfos = RelatedRecordsUtility.GetRelatedProperties(record.RelatedRecords);
            relatedItemInfos.RemoveAll(r => (r.SourceFlag == (int)SourceFlag.SharePoint || r.SourceFlag == (int)SourceFlag.All) && r.SiteUrl.Equals(siteUrlBeforeMove, StringComparison.OrdinalIgnoreCase) && (r.ItemUrl.Equals(itemUrlBeforeMove, StringComparison.OrdinalIgnoreCase) || r.url.Equals(itemUrlBeforeMove, StringComparison.OrdinalIgnoreCase)));
            relatedItemInfos.Add(relatedInfoAfterMove);
            record.RelatedRecords = RelatedRecordsUtility.SerializeRelatedProperties(relatedItemInfos);
            record.RelatedRecordsCount = relatedItemInfos.Count;
            ExplorerDao.UpdatePhysicalRecord(record, true);
        }

        public RMRelatedItemInfo GetRelatedItemInfo(RelatedItemSubmitInfo submitInfo)
        {
            try
            {
                IAveSite site = this.GetIAveSite(submitInfo.SiteUrl);
                var web = site.OpenWeb(submitInfo.WebId);
                var webServerRelativeUrl = web.ServerRelativeUrl;
                var webUrl = web.Url;
                IAveListItem item = web.GetListItem(string.Empty, submitInfo.ListId, submitInfo.ListItemId);

                RMRelatedItemInfo itemInfo = new RMRelatedItemInfo();
                itemInfo.SourceFlag = (int)SourceFlag.SharePoint;
                itemInfo.NeedDelete = submitInfo.NeedDelete;

                if (item.FieldValues.ContainsKey("FileRef"))
                {
                    string str = item.FieldValues["FileRef"].ToString();
                    string relatedUrl = str.Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
                    itemInfo.url = webUrl + "/" + relatedUrl;
                    itemInfo.WebUrl = webUrl;
                    itemInfo.ItemUrl = str;
                }
                if ((item.FieldValues["FSObjType"] as string).Equals(((int)FileSystemObjectType.File).ToString()))
                {
                    if ((item.FieldValues["FileLeafRef"] as string).EndsWith("_.000", StringComparison.OrdinalIgnoreCase))
                    {
                        itemInfo.name = item.FieldValues["Title"] as string;
                        if (string.IsNullOrEmpty(itemInfo.name))
                        {
                            itemInfo.name = GetSpecialListItemName(item);
                        }
                    }
                    else
                    {
                        itemInfo.name = item.FieldValues["FileLeafRef"].ToString();
                    }
                }
                else
                {
                    itemInfo.name = item.FieldValues["FileLeafRef"].ToString();
                }

                itemInfo.id = new Guid(item["UniqueId"].ToString());
                itemInfo.DocLibRowId = item.ID;

                itemInfo.WebUrl = item.ParentList.ParentWeb.Url;
                itemInfo.ListId = item.ParentList.ID;
                itemInfo.WebId = item.ParentList.ParentWeb.ID;
                itemInfo.SiteId = item.ParentList.ParentWeb.Site.ID;
                itemInfo.SiteUrl = item.ParentList.ParentWeb.Site.Url;
                itemInfo.WebServerRelativeUrl = item.ParentList.ParentWeb.ServerRelativeUrl;
                itemInfo.ListUrl = item.ParentList.RootFolder.ServerRelativeUrl;
                itemInfo.level = item.ParentList.BaseType == AveBaseType.GenericList ? SORelativeDataArchiverNodeLevel.Item : SORelativeDataArchiverNodeLevel.Document;


                if (item.FieldValues.TryGetValue("FileDirRef", out object value))
                {
                    string fileDirRef = value.ToString();
                    var parentFolder = web.GetFolder(fileDirRef);
                    itemInfo.FolderId = parentFolder.UniqueId;
                    itemInfo.FolderUrl = parentFolder.ServerRelativeUrl;
                    itemInfo.ParentFolderIsRootFolder = item.ParentList.RootFolder.UniqueId.Equals(parentFolder.UniqueId);
                }

                return itemInfo;
            }
            catch(ServerException se)
            {
                if(se.Message.Contains("Item does not exist"))
                {
                    logger.Warn($"Related item does not exist. Id:{submitInfo.ListItemId},Error:{se}");
                    return null;
                }
            }
            catch (Exception e)
            {
                logger.Warn($"Error occurred while gettting related item. Id:{submitInfo.ListItemId},Error:{e}");
            }
            return null;
        }

        public static string GetSpecialListItemName(IAveListItem item)
        {
            var itemName = "";
            if (AveListTemplateType.Links == item.ParentList.BaseTemplate)
            {
                FieldUrlValue filedUrlValue = item.FieldValues["URL"] as FieldUrlValue;
                itemName = filedUrlValue.Url;
            }
            return itemName;
        }

        public IAveListItem GetRelatedItem(RMRelatedItemInfo relatedInfo)
        {
            IAveListItem item = null;
            IAveSite site = null;
            //目前功能由于多人维护，并且操作很多，比如Move ，update 等操作，无法保证生产的RelatedInfo 是否完整。所以需要做代码健壮处理，来实例化site。 此处后期可以添加通过siteUrl 实例化site 的逻辑
            try
            {
                if (string.IsNullOrEmpty(relatedInfo.AveId))
                {
                    site = this.GetIAveSite(relatedInfo.SiteUrl);
                }
                else
                {
                    site = this.GetIAveSite(new Guid(relatedInfo.AveId));
                }
                var web = site.OpenWeb(relatedInfo.WebId);
                item = web.GetListItem(relatedInfo.ItemUrl, relatedInfo.ListId, relatedInfo.id);
            }
            catch (Exception e)
            {
                logger.Warn($"Error occurred while gettting related item. Id:{relatedInfo.id} ,name:{relatedInfo.name},url:{relatedInfo.url},Error:{e.ToString()}");
            }
            return item;
        }

        private IAveListItem GetRelatedItem(Record record)
        {
            IAveListItem item = null;
            if (record.SourceFlag == (int)SourceFlag.SharePoint)
            {
                try
                {
                    IAveSite site = this.GetIAveSite(new Guid(record.AveSiteId));
                    var web = site.OpenWeb(record.WebId);
                    var list = web.GetList(record.ListId);
                    item = list.GetItemByUniqueId(record.NodeId);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error in get SharePoint item : {record.DirPath}, reason : {ex.ToString()}.");
                }
            }
            return item;
        }
        private IAveSite GetIAveSite(string siteUrl)
        {
            var siteInfo = this.GetSiteNode(siteUrl);
            if (siteInfo == null) { throw new Exception(string.Format("Site : {0} does not be registed in DocAve.", siteUrl)); }
            var bposInfo = CommonPoolUserUtil.GetBPOSInfo(siteInfo);
            var aveObjectModelFactory = MultiAppUtil.CreateAveObjectModelFactory(siteUrl, bposInfo, AveContextKind.ClientObjectModel);
            var aveSite = aveObjectModelFactory.CreateSite(siteUrl);
            Record = aveObjectModelFactory.CreateRecords();
            return aveSite;
        }
        private IAveSite GetIAveSite(Guid aveSiteId)
        {
            var siteInfo = this.GetSiteNode(aveSiteId);
            if (siteInfo == null) { throw new Exception(string.Format("Site : {0} does not be registed in DocAve.", aveSiteId)); }
            var bposInfo = CommonPoolUserUtil.GetBPOSInfo(siteInfo);
            var aveObjectModelFactory = MultiAppUtil.CreateAveObjectModelFactory(siteInfo.url, bposInfo, AveContextKind.ClientObjectModel);
            var aveSite = aveObjectModelFactory.CreateSite(siteInfo.url);
            Record = aveObjectModelFactory.CreateRecords();
            return aveSite;
        }

        #region 后台对象和db 对象，以及GUI 对象之间的 Convert 方法
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
                    rel = string.Format(columnStrcuture, rel, relatedItemInfo.url, relatedItemInfo.recId, physicalRelatedStyle);
                    physicalBuilder.Append(rel);
                }
                else
                {
                    rel = string.Format(columnStrcuture, rel, relatedItemInfo.url, relatedItemInfo.name, string.Empty);
                    electronicBuilder.Append(rel);
                }
                //logger.Info("Debug: related infos href:{0}", relatedItemInfo.url);
            }

            //var noneInfo = string.Format("<p>{0}</p>", I18NEntity.GetString("RM_SS_RelatedRecords_Data_None"));
            var electronicInfo = electronicBuilder.Length > 0 ?
                string.Format(categoryHeader, I18NEntity.GetString("RM_SS_RelatedRecords_Type_Electronic") + ":") + electronicBuilder.ToString()
                : string.Empty;

            var physicalInfo = physicalBuilder.Length > 0 ?
                string.Format(categoryHeader, I18NEntity.GetString("RM_SS_RelatedRecords_Type_Physical") + ":") + physicalBuilder.ToString()
                : string.Empty;
            relatedInfo = string.Format(columnHeader, electronicInfo + physicalInfo);
            return relatedInfo;
        }

        /// <summary>
        /// 将SP Column value 转换成List,SP column value 有特殊属性，所以需要解析XML<RMRelatedItemInfo>
        /// </summary>
        /// <param name="relatedColumnValue"></param>
        /// <returns>如果没有related column value，则返回空集合，不返回null</returns>
        public List<RMRelatedItemInfo> GetRelatedPropertiesBySPColumnValue(string relatedColumnValue)
        {
            List<RMRelatedItemInfo> infos = new List<RMRelatedItemInfo>();
            if (!string.IsNullOrEmpty(relatedColumnValue))
            {
                var columnValue = relatedColumnValue;

                XmlDocument xmlDoc = new XmlDocument();
                //columnValue = HttpUtility.UrlDecode(columnValue); error: "+"->" "
                columnValue = columnValue.Replace("&#58;", ":");
                columnValue = columnValue.Replace("&", "&amp;").Replace("amp;amp;","amp;");
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
        public List<RMRelatedItemInfo> GetRelatedPropertiesByDB(string relatedValueInDB)
        {
            if (!string.IsNullOrEmpty(relatedValueInDB))
            {
                //DB 中的对象，直接反序列化即可
                List<RMRelatedItemInfo> infos = SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(relatedValueInDB);
                return infos;
            }
            return new List<RMRelatedItemInfo>();
        }
        public List<RMRelatedItemInfo> GetRelatedProperties(IAveListItem currentItem)
        {
            try
            {
                if (currentItem != null && currentItem.FieldValues != null && currentItem.FieldValues.ContainsKey(relatedColumnInternalName) && currentItem[relatedColumnInternalName] != null)
                {
                    var sourceUrlValue = currentItem[relatedColumnInternalName].ToString();
                    return GetRelatedPropertiesBySPColumnValue(sourceUrlValue);
                }
            }
            catch (Exception e)
            {
                ArgumentCheck.CheckNotNull(currentItem);
                logger.Warn("Get related records failed {0}:{1}", currentItem["FileRef"].ToString(), e.ToString());
                throw;
            }
            return null;
        }

        public static List<RMRelatedItemInfo> GetRelatedProperties(string recordsRelatedValue)
        {
            List<RMRelatedItemInfo> infos = new List<RMRelatedItemInfo>();
            if (!string.IsNullOrEmpty(recordsRelatedValue))
            {
                var sourceUrlValue = recordsRelatedValue;
                XmlDocument xmlDoc = new XmlDocument();
                sourceUrlValue = sourceUrlValue.Replace("&#58;", ":");
                xmlDoc.LoadXml(sourceUrlValue);
                if (xmlDoc.GetElementsByTagName("a").Count > 0)
                {
                    foreach (var ele in xmlDoc.GetElementsByTagName("a"))
                    {
                        XmlElement element = ele as XmlElement;
                        var relatedObjString = element.GetAttribute("rel");
                        relatedObjString = HttpUtility.UrlDecode(relatedObjString);
                        RMRelatedItemInfo relatedObj = SerializerHelper.DeserializeByJsonSerializer<RMRelatedItemInfo>(relatedObjString);
                        var relatedItemUrl = HttpUtility.UrlDecode(element.GetAttribute("href"));
                        string url = relatedItemUrl;
                        relatedObj.url = relatedItemUrl;
                        relatedObj.url = url;
                        infos.Add(relatedObj);
                    }
                }
                else if (xmlDoc.GetElementsByTagName("RMRelatedItemInfo").Count > 0)
                {
                    infos = GCommon.Utility.SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(sourceUrlValue);
                }
            }
            return infos;
        }

        public static string GetRelatedString(List<RMRelatedItemInfo> relatedItemInfos)
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
                    rel = string.Format(columnStrcuture, rel, relatedItemInfo.url, relatedItemInfo.recId, physicalRelatedStyle);
                    physicalBuilder.Append(rel);
                }
                else
                {
                    rel = string.Format(columnStrcuture, rel, relatedItemInfo.url, relatedItemInfo.name, string.Empty);
                    electronicBuilder.Append(rel);
                }
                //logger.Info("Debug: related infos href:{0}", relatedItemInfo.url);
            }

            //var noneInfo = string.Format("<p>{0}</p>", I18NEntity.GetString("RM_SS_RelatedRecords_Data_None"));
            var electronicInfo = electronicBuilder.Length > 0 ?
                string.Format(categoryHeader, I18NEntity.GetString("RM_SS_RelatedRecords_Type_Electronic") + ":") + electronicBuilder.ToString()
                : string.Empty;

            var physicalInfo = physicalBuilder.Length > 0 ?
                string.Format(categoryHeader, I18NEntity.GetString("RM_SS_RelatedRecords_Type_Physical") + ":") + physicalBuilder.ToString()
                : string.Empty;
            relatedInfo = string.Format(columnHeader, electronicInfo + physicalInfo);
            return relatedInfo;
        }

        public List<RMRelatedItemInfo> GetRelatedProperties(Record record)
        {
            List<RMRelatedItemInfo> infos = null;
            if (record.SourceFlag == (int)SourceFlag.SharePoint || record.SourceFlag == (int)SourceFlag.All)
            {
                //SP 数据目前很多case 无法保证SP 中属性，跟DB 属性同步，需要跑完DS job才可以，为了保证功能正常，此处从SP 中取。
                //如果能保证各个功能SP 与DB value 一致，则可以直接从DB 获取即可
                var aveListItem = GetRelatedItem(record);
                if (aveListItem != null)
                {
                    infos = GetRelatedProperties(aveListItem);
                }
                else
                {
                    //SP 没获取到ListItem ，可能文件已经不存在，这时候只能依赖于DB
                    infos = GetRelatedPropertiesByDB(record.RelatedRecords);
                }
            }
            else if (record.SourceFlag == (int)SourceFlag.SharePointOnPrem)
            {
                var listItem = SharePointOnPremClient.GetSPOnPremiseItem(new Guid(record.AveSiteId), record.WebId, record.ListId, record.NodeId).GetAwaiter().GetResult();
                if (listItem != null)
                {
                    infos = GetRelatedPropertiesBySPColumnValue(listItem.RelatedRecordsInfo);
                }
                else
                {
                    infos = GetRelatedPropertiesByDB(record.RelatedRecords);
                }
            }
            else
            {
                infos = GetRelatedPropertiesByDB(record.RelatedRecords);
            }
            return infos;
        }
        public string GetLogonName()
        {
            var user = currentWeb.CurrentUser;
            currentContext.Load(user);
            currentContext.ExecuteQuery();
            return user.LoginName;
        }
        //目前方法用于SP App
        public List<RMRelatedItemInfo> GetRelatedProperties(Guid? listId = null, int? itemId = null, bool includePhysicalRecord = false)
        {
            if (listId != null && itemId != null)
            {
                GetCurrentItem(listId, itemId);
            }
            try
            {
                if (currentItem.FieldValues.ContainsKey(relatedColumnInternalName) && currentItem[relatedColumnInternalName] != null)
                {
                    var sourceUrlValue = currentItem[relatedColumnInternalName].ToString();
                    List<RMRelatedItemInfo> infos = new List<RMRelatedItemInfo>();
                    XmlDocument xmlDoc = new XmlDocument();
                    // sourceUrlValue = HttpUtility.UrlDecode(sourceUrlValue);//??
                    sourceUrlValue = sourceUrlValue.Replace("&#58;", ":");
                    xmlDoc.LoadXml(sourceUrlValue);
                    foreach (var ele in xmlDoc.GetElementsByTagName("a"))
                    {
                        XmlElement element = ele as XmlElement;
                        var relatedObjString = element.GetAttribute("rel");
                        relatedObjString = HttpUtility.UrlDecode(relatedObjString);
                        RMRelatedItemInfo relatedObj = SerializerHelper.DeserializeByJsonSerializer<RMRelatedItemInfo>(relatedObjString);
                        if (relatedObj.SourceFlag == (int)SourceFlag.SharePoint || relatedObj.SourceFlag == (int)SourceFlag.All)
                        {
                            var relatedItemUrl = HttpUtility.UrlDecode(element.GetAttribute("href"));
                            //string url = string.Empty;
                            //if (!element.GetAttribute("href").StartsWith(relatedObj.SiteUrl))//parmDic["SiteUrl"]))
                            //{
                            //    var webServerRelativeUrl = currentWeb.ServerRelativeUrl;
                            //    url = element.GetAttribute("href").Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
                            //    url = relatedObj.SiteUrl + "/" + url;
                            //}

                            //relatedObj.url = relatedItemUrl;
                            StringBuilder urlHostStringBuilder = new StringBuilder(512);
                            var siteUri = new Uri(relatedObj.SiteUrl);
                            urlHostStringBuilder.Append("https:");
                            urlHostStringBuilder.Append("//");
                            urlHostStringBuilder.Append(siteUri.Host);
                            relatedObj.url = urlHostStringBuilder.ToString() + relatedItemUrl;
                        }

                        if (includePhysicalRecord || relatedObj.SourceFlag != (int)SourceFlag.Physical)
                        {
                            infos.Add(relatedObj);
                        }
                    }

                    //foreach (var ele in xmlDoc.GetElementsByTagName("span"))
                    //{
                    //    XmlElement element = ele as XmlElement;
                    //    var relatedObjString = element.GetAttribute("data-rel");
                    //    relatedObjString = HttpUtility.UrlDecode(relatedObjString);
                    //    JavaScriptSerializer jss = new JavaScriptSerializer();
                    //    RMRelatedItemInfo relatedObj = jss.Deserialize<RMRelatedItemInfo>(relatedObjString);
                    //    if (includePhysicalRecord)
                    //    {

                    //    }
                    //    else
                    //    {
                    //        continue;
                    //    }
                    //    infos.Add(relatedObj);

                    //}
                    return infos;
                }
            }
            catch (Exception e)
            {
                logger.Warn("Get related records failed {0}:{1}", currentItem["FileRef"].ToString(), e.ToString());
                throw;
            }
            return null;
        }

        public RMRelatedItemInfo GenerateRMRelatedItemInfo(ListItem item, List list, Web web, ClientContext destContext)
        {
            RMRelatedItemInfo info = new RMRelatedItemInfo();
            info.DocLibRowId = item.Id;
            var folder = web.GetFolderByServerRelativeUrl(item.FieldValues["FileDirRef"].ToString());
            destContext.Load(folder, f => f.ServerRelativeUrl, f => f.Properties);
            destContext.Load(list, l => l.RootFolder, l => l.BaseType);
            var destSite = destContext.Site;
            destContext.Load(destSite, s => s.Url, s => s.Id);
            destContext.ExecuteQuery();
            if (folder.Properties.FieldValues.ContainsKey("vti_etag") &&
                     folder.Properties["vti_etag"] != null)
            {
                string tagString = folder.Properties["vti_etag"].ToString().Trim('"').Split(',')[0];
                info.FolderId = string.IsNullOrEmpty(tagString) ? Guid.Empty : new Guid(tagString);
            }
            //info.FolderId = new Guid(folder.ListItemAllFields["UniqueId"].ToString());//confirm....
            info.id = new Guid(item.FieldValues["UniqueId"].ToString());
            string displayName = string.Empty;
            if ((item.FieldValues["FSObjType"] as string).Equals(((int)FileSystemObjectType.File).ToString()))
            {
                if ((item.FieldValues["FileLeafRef"] as string).EndsWith("_.000", StringComparison.OrdinalIgnoreCase))
                {
                    displayName = item.FieldValues["Title"] as string;
                    if (string.IsNullOrEmpty(displayName))
                    {
                        displayName = "";
                    }
                }
                else
                {
                    displayName = item.FieldValues["FileLeafRef"].ToString();
                }
            }
            else
            {
                displayName = item.FieldValues["FileLeafRef"].ToString();
            }
            info.name = displayName;
            info.ParentFolderIsRootFolder = list.RootFolder.ServerRelativeUrl.Equals(folder.ServerRelativeUrl);//confirm
            info.WebId = web.Id;
            info.WebUrl = web.Url;
            info.SiteId = destContext.Site.Id;
            info.SiteUrl = destContext.Site.Url;
            info.level = list.BaseType.Equals(BaseType.DocumentLibrary) ? SORelativeDataArchiverNodeLevel.Document : SORelativeDataArchiverNodeLevel.Item;
            info.ListId = list.Id;
            info.WebServerRelativeUrl = web.ServerRelativeUrl;
            info.ListUrl = list.RootFolder.ServerRelativeUrl;
            info.FolderUrl = folder.ServerRelativeUrl;
            info.ItemUrl = item.FieldValues["FileRef"].ToString();
            //info.url = info.SiteUrl + "/" + info.ItemUrl.Substring(web.ServerRelativeUrl.TrimEnd('/').Length + 1);
            if (info.level == SORelativeDataArchiverNodeLevel.Item)
            {
                info.url = WebUtil.GetListItemRealPath(info.SiteUrl, list.RootFolder.ServerRelativeUrl, info.ItemUrl);
            }
            else
            {
                info.url = new Uri(info.SiteUrl).Scheme + @"://" + new Uri(info.SiteUrl).Authority + info.ItemUrl;
            }
            return info;
        }

        /// <summary>
        /// 此方法为后台调用的方法，作用是将当前Record 对象，转换成RMRelatedItemInfo， 用来更新到关联文件的时候进行使用
        /// </summary>
        /// <returns></returns>
        public RMRelatedItemInfo GenerateRMRelatedItemInfo(Record record)
        {
            RMRelatedItemInfo info = new RMRelatedItemInfo();
            if (record.SourceFlag == (int)SourceFlag.SharePoint || record.SourceFlag == (int)SourceFlag.All)
            {
                var aveListItem = GetRelatedItem(record);
                if (aveListItem != null)
                {
                    info = this.GenerateRMRelatedItemInfo(aveListItem);
                    info.AveId = record.AveSiteId;
                }
                else
                {
                    logger.Error($"An error occur when generate related info for sp, reason : item is null.");
                }
            }
            else if (record.SourceFlag == (int)SourceFlag.SharePointOnPrem)
            {
                var listItem = SharePointOnPremClient.GetSPOnPremiseItem(new Guid(record.AveSiteId), record.WebId, record.ListId, record.NodeId).GetAwaiter().GetResult();
                if (listItem != null)
                {
                    info = this.GenerateRMRelatedItemInfo(listItem);
                    info.AveId = record.AveSiteId;
                }
                else
                {
                    logger.Error($"An error occur when generate related info for sp on premise, reason : item is null.");
                }
            }
            else if (record.SourceFlag == (int)SourceFlag.FileSystem)
            {
                info = this.GenerateRelatedItemInfoForFS(record);
            }
            else if (record.SourceFlag == (int)SourceFlag.Physical)
            {
                info = this.GenerateRelatedItemInfoForPhysical(record);
            }
            return info;
        }
        public RMRelatedItemInfo GenerateRMRelatedItemInfo(IAveListItem currentItem)
        {
            RMRelatedItemInfo info = new RMRelatedItemInfo();
            info.SourceFlag = (int)SourceFlag.SharePoint;
            info.DocLibRowId = currentItem.ID;
            var folder = currentItem.ParentList.ParentWeb.GetFolder(currentItem.FieldValues["FileDirRef"].ToString());
            if (folder != null && folder.Properties.ContainsKey("vti_etag") &&
                    folder.Properties["vti_etag"] != null)
            {
                string tagString = folder.Properties["vti_etag"].ToString().Trim('"').Split(',')[0];
                info.FolderId = string.IsNullOrEmpty(tagString) ? Guid.Empty : new Guid(tagString);
                info.ParentFolderIsRootFolder = currentItem.ParentList.RootFolder.UniqueId == folder.UniqueId;
                info.FolderUrl = folder.ServerRelativeUrl;
            }
            info.id = currentItem.UniqueId;
            string displayName = string.Empty;
            if ((currentItem.FieldValues["FSObjType"] as string).Equals((0).ToString()))
            {
                if ((currentItem.FieldValues["FileLeafRef"] as string).EndsWith("_.000", StringComparison.OrdinalIgnoreCase))
                {
                    displayName = currentItem.FieldValues["Title"] as string;
                    if (string.IsNullOrEmpty(displayName))
                    {
                        displayName = "";
                    }
                }
                else
                {
                    displayName = currentItem.FieldValues["FileLeafRef"].ToString();
                }
            }
            else
            {
                displayName = currentItem.FieldValues["FileLeafRef"].ToString();
            }
            info.name = displayName;
            info.WebId = currentItem.ParentList.ParentWeb.ID;
            info.WebUrl = currentItem.ParentList.ParentWeb.Url;
            info.SiteId = currentItem.ParentList.ParentWeb.Site.ID;
            info.SiteUrl = currentItem.ParentList.ParentWeb.Site.Url;
            info.level = currentItem.ParentList.BaseType == AveBaseType.DocumentLibrary ? SORelativeDataArchiverNodeLevel.Document : SORelativeDataArchiverNodeLevel.Item;
            info.ListId = currentItem.ParentList.ID;
            info.WebServerRelativeUrl = currentItem.ParentList.ParentWeb.ServerRelativeUrl;
            info.ListUrl = currentItem.ParentList.RootFolder.ServerRelativeUrl;
            info.ItemUrl = currentItem.FieldValues["FileRef"].ToString();

            if (info.level == SORelativeDataArchiverNodeLevel.Item)
            {
                info.url = WebUtil.GetListItemRealPath(info.SiteUrl, currentItem.ParentList.RootFolder.ServerRelativeUrl, info.ItemUrl);
            }
            else
            {
                info.url = new Uri(info.SiteUrl).Scheme + @"://" + new Uri(info.SiteUrl).Authority + info.ItemUrl;
            }
            return info;
        }

        public RMRelatedItemInfo GenerateRMRelatedItemInfo(SharePointOnPremQuererResult result)
        {
            RMRelatedItemInfo info = new RMRelatedItemInfo();
            info.id = result.UniqueId;
            info.SourceFlag = (int)SourceFlag.SharePointOnPrem;
            info.DeclareAsRecord = result.DeclareAsRecord;
            info.DocLibRowId = result.Id;

            info.FolderId = result.FolderId;
            info.ParentFolderIsRootFolder = result.ParentFolderIsRootFolder;
            info.FolderUrl = result.FolderUrl;
            info.name = result.Name;
            info.WebId = result.WebId;
            info.WebUrl = result.WebUrl;
            info.SiteId = result.SiteId;
            info.SiteUrl = result.SiteUrl;
            info.level = (SORelativeDataArchiverNodeLevel)result.Level;
            info.ListId = result.ListId;
            info.WebServerRelativeUrl = result.WebServerRelativeUrl;
            info.ListUrl = result.ListUrl;
            info.ItemUrl = result.ItemUrl;

            if (info.level == SORelativeDataArchiverNodeLevel.Item)
            {
                info.url = WebUtil.GetListItemRealPath(info.SiteUrl, info.ListUrl, info.ItemUrl);
            }
            else
            {
                info.url = new Uri(info.SiteUrl).Scheme + @"://" + new Uri(info.SiteUrl).Authority + info.ItemUrl;
            }
            return info;
        }

        private IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();
        private RMRelatedItemInfo GenerateRelatedItemInfoForPhysical(Record record)
        {
            RMRelatedItemInfo info = new RMRelatedItemInfo();
            info.id = record.Id;
            // info.name = record.LeafName;
            info.recId = record.LeafName;
            //info.url = record.DirPath;
            info.url = ExplorerService.GetPhysicalObjectFullPath(record.Id);
            info.SourceFlag = (int)SourceFlag.Physical;
            info.NodeType = record.NodeType;
            return info;
        }

        private RMRelatedItemInfo GenerateRelatedItemInfoForFS(Record record)
        {
            RMRelatedItemInfo info = new RMRelatedItemInfo();
            info.id = record.Id;
            info.name = record.LeafName;
            info.recId = record.RecordsId;
            info.url = record.DirPath;
            info.AveId = record.AveSiteId;
            info.SiteId = record.ScopeId;
            info.SourceFlag = (int)SourceFlag.FileSystem;
            info.NodeType = record.NodeType;
            info.FolderId = record.FolderId;
            return info;
        }
        //目前方法用于SP App，从SP 中直接获取属性, 可以进一步修改，通过将SP 属性封装成对象，当做参数传递到方法中，进而将方法公用
        private RMRelatedItemInfo GenerateRelatedItemInfoForSP()
        {
            RMRelatedItemInfo info = new RMRelatedItemInfo();
            info.SourceFlag = (int)SourceFlag.SharePoint;
            info.NodeType = (int)GCommon.Contract.Tree.Object.NodeLevel.Item;
            info.DocLibRowId = currentItemId;
            var folder = currentWeb.GetFolderByServerRelativeUrl(currentItem.FieldValues["FileDirRef"].ToString());
            currentContext.Load(folder, f => f.ServerRelativeUrl, f => f.Properties);
            currentContext.Load(currentList, l => l.RootFolder, l => l.BaseType);
            currentContext.ExecuteQuery();
            if (folder.Properties.FieldValues.ContainsKey("vti_etag") &&
                     folder.Properties["vti_etag"] != null)
            {
                string tagString = folder.Properties["vti_etag"].ToString().Trim('"').Split(',')[0];
                info.FolderId = string.IsNullOrEmpty(tagString) ? Guid.Empty : new Guid(tagString);
            }
            //info.FolderId = new Guid(folder.ListItemAllFields["UniqueId"].ToString());//confirm....
            info.id = new Guid(currentItem.FieldValues["UniqueId"].ToString());
            string displayName = string.Empty;
            if ((currentItem.FieldValues["FSObjType"] as string).Equals(((int)FileSystemObjectType.File).ToString()))
            {
                if ((currentItem.FieldValues["FileLeafRef"] as string).EndsWith("_.000", StringComparison.OrdinalIgnoreCase))
                {
                    displayName = currentItem.FieldValues["Title"] as string;
                    if (string.IsNullOrEmpty(displayName))
                    {
                        displayName = "";
                    }
                }
                else
                {
                    displayName = currentItem.FieldValues["FileLeafRef"].ToString();
                }
            }
            else
            {
                displayName = currentItem.FieldValues["FileLeafRef"].ToString();
            }
            info.name = displayName;
            info.ParentFolderIsRootFolder = currentList.RootFolder.ServerRelativeUrl.Equals(folder.ServerRelativeUrl);//confirm
            info.WebId = currentWeb.Id;
            info.WebUrl = currentWeb.Url;
            info.SiteId = currentSite.Id;
            info.SiteUrl = currentSite.Url;
            info.level = currentList.BaseType.Equals(BaseType.DocumentLibrary) ? SORelativeDataArchiverNodeLevel.Document : SORelativeDataArchiverNodeLevel.Item;
            info.ListId = currentList.Id;
            info.WebServerRelativeUrl = currentWeb.ServerRelativeUrl;
            info.ListUrl = currentList.RootFolder.ServerRelativeUrl;
            info.FolderUrl = folder.ServerRelativeUrl;
            info.ItemUrl = currentItem.FieldValues["FileRef"].ToString();
            //info.url = info.SiteUrl + "/" + info.ItemUrl.Substring(currentWeb.ServerRelativeUrl.TrimEnd('/').Length + 1);
            if (info.level == SORelativeDataArchiverNodeLevel.Item)
            {
                info.url = WebUtil.GetListItemRealPath(info.SiteUrl, currentList.RootFolder.ServerRelativeUrl, info.ItemUrl);
            }
            else
            {
                info.url = new Uri(info.SiteUrl).Scheme + @"://" + new Uri(info.SiteUrl).Authority + info.ItemUrl;
            }
            return info;
        }
        #endregion
        #region original token method
        //[Obsolete("使用时建议自行调试，目前没人调用")]
        //public void RemoveRelatedProperties(RMRelatedItemInfo info, ListItem listItem)//remove destination
        //{
        //    var needRemoveHref = listItem["FileRef"].ToString();
        //    if ((listItem.FieldValues["FileLeafRef"] as string).EndsWith("_.000", StringComparison.OrdinalIgnoreCase))
        //    {
        //        var displayName = listItem.FieldValues["Title"] as string;
        //        if (string.IsNullOrEmpty(displayName))
        //        {
        //            displayName = "";
        //        }
        //        needRemoveHref = listItem.FieldValues["FileDirRef"].ToString() + "/" + displayName;
        //    }
        //    var relatedWebURL = info.WebUrl;
        //    var relatedItemUrl = info.url;
        //    ClientContext context = null;
        //    #region
        //    try
        //    {
        //        context = this.GetClientContextWithAccessToken(relatedWebURL, accesstoken);
        //        var destWeb = context.Web;
        //        context.Load(destWeb, w => w.Lists, w => w.Url, w => w.Id, w => w.ServerRelativeUrl);
        //        context.ExecuteQuery();
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Info("Verify dest context failed {0}:{1} retry with Usersid {2}", relatedWebURL, e.ToString(), userSid);
        //        var targetUri = new Uri(relatedWebURL);
        //        var destaccessToken = TokenHelper.GetS2SAccessTokenWithSid(targetUri, userSid);
        //        context = this.GetClientContextWithAccessToken(relatedWebURL, destaccessToken);
        //    }
        //    #endregion
        //    try
        //    {
        //        using (context)
        //        {
        //            var relatedWeb = context.Web;
        //            context.Load(relatedWeb, w => w.Lists);
        //            var relatedList = context.Web.Lists.GetById(info.ListId);
        //            context.Load(relatedList);
        //            var relatedItem = relatedList.GetItemById(info.DocLibRowId);
        //            context.Load(relatedItem);
        //            context.ExecuteQuery();
        //            var relatedColumnValue = relatedItem[relatedColumnInternalName].ToString();//debug later
        //                                                                                       //relatedColumnValue = HttpUtility.UrlDecode(relatedColumnValue);
        //            XmlDocument reDoc = new XmlDocument();
        //            reDoc.LoadXml(relatedColumnValue);
        //            XmlElement reXe = null;
        //            foreach (var reNode in reDoc.GetElementsByTagName("a"))
        //            {
        //                reXe = reNode as XmlElement;
        //                var relatedObjString = reXe.GetAttribute("rel");
        //                relatedObjString = HttpUtility.UrlDecode(relatedObjString);
        //                JavaScriptSerializer jss = new JavaScriptSerializer();
        //                RMRelatedItemInfo relatedObj = jss.Deserialize<RMRelatedItemInfo>(relatedObjString);
        //                var itemUrl = relatedObj.FolderUrl + "/" + relatedObj.name;
        //                //var href = HttpUtility.UrlDecode(reXe.GetAttribute("href").ToString());
        //                needRemoveHref = HttpUtility.UrlDecode(needRemoveHref);
        //                if (itemUrl.Equals(needRemoveHref) || itemUrl.EndsWith(needRemoveHref))//to do special leter
        //                {
        //                    break;
        //                }
        //                else
        //                {
        //                    reXe = null;
        //                }
        //            }
        //            if (reXe != null)
        //            {
        //                var root = reXe.ParentNode.ParentNode;
        //                var parent = reXe.ParentNode;
        //                parent.RemoveChild(reXe);
        //                root.RemoveChild(parent);
        //                if (relatedList.ForceCheckout)
        //                {
        //                    context.Load(relatedItem.File);
        //                    context.ExecuteQuery();
        //                    if (relatedItem.File.CheckOutType == CheckOutType.None)
        //                    {
        //                        relatedItem.File.CheckOut();
        //                        context.ExecuteQuery();
        //                    }
        //                }
        //                if (!reDoc.InnerXml.Contains("href"))
        //                {
        //                    relatedItem[relatedColumnInternalName] = string.Empty;
        //                }
        //                else
        //                {
        //                    relatedItem[relatedColumnInternalName] = reDoc.InnerXml;
        //                }
        //                relatedItem.Update();
        //                if (relatedList.ForceCheckout)
        //                {
        //                    if (relatedItem.File.CheckOutType == CheckOutType.None)
        //                    {
        //                        relatedItem.File.CheckIn("Update Related Record", CheckinType.MajorCheckIn);
        //                    }
        //                }
        //                //relatedItem.SystemUpdate();//confirm later
        //                context.ExecuteQuery();
        //            }
        //        }
        //    }
        //    catch (Exception ee)
        //    {
        //        if (!ee.Message.Contains("File Not Found"))
        //        {
        //            throw new Exception("Remove Related Failed");
        //        }
        //    }
        //    //use infomation in column ,remove destination relationship.
        //}
        #endregion
        [Obsolete("使用时建议自行调试，目前没人调用")]
        public string RemoveRelatedPropertiesForExplorer(RMRelatedItemInfo info, ListItem listItem)//remove destination
        {
            string result = string.Empty;
            var needRemoveHref = listItem["FileRef"].ToString();
            if ((listItem.FieldValues["FileLeafRef"] as string).EndsWith("_.000", StringComparison.OrdinalIgnoreCase))
            {
                var displayName = listItem.FieldValues["Title"] as string;
                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = "";
                }
                needRemoveHref = listItem.FieldValues["FileDirRef"].ToString() + "/" + displayName;
            }
            //var relatedWebURL = info.WebUrl;
            //var relatedItemUrl = info.url;
            using (ClientContext context = this.InitContext(new Guid(info.AveId)))
            {
                var relatedWeb = context.Site.OpenWebById(info.WebId);
                context.Load(relatedWeb, w => w.Lists);
                var relatedList = relatedWeb.Lists.GetById(info.ListId);
                context.Load(relatedList);
                var relatedItem = relatedList.GetItemById(info.DocLibRowId);
                context.Load(relatedItem);
                context.ExecuteQuery();
                //if ((relatedItem.FieldValues["FileLeafRef"] as string).EndsWith("_.000", StringComparison.OrdinalIgnoreCase))
                //{
                //    var displayName = relatedItem.FieldValues["Title"] as string;
                //    if (string.IsNullOrEmpty(displayName))
                //    {
                //        displayName = "";
                //    }
                //    info.url = relatedItem.FieldValues["FileDirRef"].ToString() + "/" + displayName;
                //}
                //var relatedColumnValue = relatedItem[relatedColumnInternalName].ToString();//debug later
                var relatedColumnValue = relatedItem.FieldValues[relatedColumnInternalName] as string;
                if (!string.IsNullOrEmpty(relatedColumnValue))
                {
                    relatedColumnValue = HttpUtility.UrlDecode(relatedColumnValue);
                    XmlDocument reDoc = new XmlDocument();
                    reDoc.LoadXml(relatedColumnValue);
                    XmlElement reXe = null;
                    foreach (var reNode in reDoc.GetElementsByTagName("a"))
                    {
                        reXe = reNode as XmlElement;
                        var relatedObjString = reXe.GetAttribute("rel");
                        relatedObjString = HttpUtility.UrlDecode(relatedObjString);
                        RMRelatedItemInfo relatedObj = SerializerHelper.DeserializeByJsonSerializer<RMRelatedItemInfo>(relatedObjString);
                        var itemUrl = relatedObj.FolderUrl + "/" + relatedObj.name;
                        //var href = HttpUtility.UrlDecode(reXe.GetAttribute("href").ToString()); // DispForm?id
                        needRemoveHref = HttpUtility.UrlDecode(needRemoveHref);
                        if (itemUrl.Equals(needRemoveHref) || itemUrl.EndsWith(needRemoveHref))//to do special leter
                        {
                            break;
                        }
                        else
                        {
                            reXe = null;
                        }
                    }
                    if (reXe != null)
                    {
                        var root = reXe.ParentNode.ParentNode;
                        var parent = reXe.ParentNode;
                        parent.RemoveChild(reXe);
                        root.RemoveChild(parent);
                        if (!reDoc.InnerXml.Contains("href"))
                        {
                            relatedItem[relatedColumnInternalName] = string.Empty;
                        }
                        else
                        {
                            relatedItem[relatedColumnInternalName] = reDoc.InnerXml;
                            result = reDoc.InnerXml;
                        }
                        //relatedItem.SystemUpdate();//confirm later
                        relatedItem.Update();
                        context.ExecuteQuery();
                    }
                }
            }
            //use infomation in column ,remove destination relationship.
            return result;
        }

        public void RemoveRelatedProperty(Guid recordId)
        {
            var record = ExplorerDao.GetRecordByIds(new List<Guid>() { recordId }).FirstOrDefault();
            if (record == null) return;
            this.RemoveRelatedProperty(record);
        }
        public void RemoveRelatedProperty(Record record)
        {
            if (string.IsNullOrEmpty(record.RelatedRecords)) return;
            var relatedInfo = GenerateRMRelatedItemInfo(record);
            var relatedProperties = GetRelatedPropertiesByDB(record.RelatedRecords);
            relatedProperties.ForEach(r => r.NeedDelete = true);
            var ids = relatedProperties.Select(d =>
            {
                Guid selectResult;
                if (d.SourceFlag == (int)SourceFlag.All || d.SourceFlag == (int)SourceFlag.SharePoint)
                {
                    selectResult = IDGenerator.GetRecordId(d.SiteId, d.id);
                }
                else
                {
                    selectResult = d.id;
                }
                return selectResult;
            }).ToList();
            ids.Add(record.Id);
            var allRecords = ExplorerDao.GetRecordByIds(ids);
            this.UpdateRelatedPropertiesForExplorer(record, relatedProperties, allRecords);
        }

        public void RemoveRelatedPropertyForListItem(IAveListItem item)
        {
            var relatedProperties = GetRelatedProperties(item);
            if (relatedProperties == null)
            {
                logger.Info("Current item:{0} relatedProperties is null.", item.Url);
                return;
            }
            relatedProperties.ForEach(r => r.NeedDelete = true);
            var ids = relatedProperties.Select(d =>
            {
                Guid selectResult;
                if (d.SourceFlag == (int)SourceFlag.All || d.SourceFlag == (int)SourceFlag.SharePoint)
                {
                    selectResult = IDGenerator.GetRecordId(d.SiteId, d.id);
                }
                else
                {
                    selectResult = d.id;
                }
                return selectResult;
            }).ToList();
            var currentId = IDGenerator.GetRecordId(item.ParentList.ParentWeb.Site.ID, item.UniqueId);
            ids.Add(currentId);
            var allRecords = ExplorerDao.GetRecordByIds(ids);
            var currentRecord = allRecords.Find(r => r.Id == currentId);
            this.UpdateRelatedPropertiesForExplorer(currentRecord, relatedProperties, allRecords);
        }

        /// <summary>
        /// 移除关联Item 的RelatedColumn 
        /// </summary>
        /// <param name="relatedItemInfoBeforeMove">Move之前文件的RelatedItemInfo， 通过这个对象可以获取到关联的Item</param>
        /// <param name="siteUrlBeforeMove">Move之前文件的site url</param>
        /// <param name="itemUrlBeforeMove">Move之前文件的item url, 支持server related url  和 full url</param>
        /// 通过site url 和item url ，能从关联文件中找到与原文件的关联记录，进而可以更新这条信息
        /// <param name="relatedItemAccountInfo">关联文件站点对应的注册信息，用于连接关联文件站点，进而更新关联文件</param>
        public void RemoveRelateColumnValue(RMRelatedItemInfo relatedItemInfoBeforeMove, IAveSite site, string itemUrlBeforeMove, Guid itemId, string relatedItemAccountInfo)
        {
            string siteUrlBeforeMove = site.Url;
            if (relatedItemInfoBeforeMove.SourceFlag == (int)SourceFlag.All || relatedItemInfoBeforeMove.SourceFlag == (int)SourceFlag.SharePoint)
            {
                var relatedItem = GetRelatedItem(relatedItemInfoBeforeMove);
                if (relatedItem == null)
                {
                    return;
                }
                var relatedProperties = GetRelatedProperties(relatedItem);
                //Find the right RMRelatedItemInfo, remove it.
                //考虑到老数据可能出现siteid 或者siteurl为空的case，此处添加兼容逻辑。
                relatedProperties.RemoveAll(r => (r.SiteId == site.ID || (r.SiteUrl != null && r.SiteUrl.Equals(siteUrlBeforeMove, StringComparison.OrdinalIgnoreCase)))
                                            &&
                                            (r.ItemUrl.Equals(itemUrlBeforeMove, StringComparison.OrdinalIgnoreCase)
                                            || r.url.Equals(itemUrlBeforeMove, StringComparison.OrdinalIgnoreCase)
                                            || r.id == itemId));
                UpdateSPItemRelatedProperties(relatedItem, relatedProperties);
                //DAO 目前对于SP ，只更新了SP 没更新DB ，稍后需要加回来下面逻辑
                var id = IDGenerator.GetRecordId(relatedItemInfoBeforeMove.SiteId, relatedItemInfoBeforeMove.id);
                Record record = ExplorerDao.ReadById(relatedItemInfoBeforeMove.SiteId, id);
                if (record != null)
                {
                    record.RelatedRecords = RelatedRecordsUtility.SerializeRelatedProperties(relatedProperties);
                    record.RelatedRecordsCount = relatedProperties.Count;
                    ExplorerDao.UpdatePhysicalRecord(record, false);
                }
            }
            else if (relatedItemInfoBeforeMove.SourceFlag == (int)SourceFlag.Physical)
            {
                Record record = ExplorerDao.ReadById(relatedItemInfoBeforeMove.SiteId, relatedItemInfoBeforeMove.id);
                var relatedProperties = RelatedRecordsUtility.DeserializeRelatedProperties(record.RelatedRecords);
                relatedProperties.RemoveAll(r => r.SiteUrl.Equals(siteUrlBeforeMove, StringComparison.OrdinalIgnoreCase)
                                                 && (r.ItemUrl.Equals(itemUrlBeforeMove, StringComparison.OrdinalIgnoreCase)
                                                     || r.url.Equals(itemUrlBeforeMove, StringComparison.OrdinalIgnoreCase)
                                                     || r.id == itemId));
                record.RelatedRecordsCount = relatedProperties.Count;
                record.RelatedRecords = RelatedRecordsUtility.SerializeRelatedProperties(relatedProperties);
                ExplorerDao.UpdatePhysicalRecord(record, false);
            }
        }


        public void RemoveRelateColumnValue(RMRelatedItemInfo relatedItemInfoBeforeMove, Guid physicalObjectId)
        {
            if (relatedItemInfoBeforeMove.SourceFlag == (int)SourceFlag.All || relatedItemInfoBeforeMove.SourceFlag == (int)SourceFlag.SharePoint)
            {
                var relatedItem = GetRelatedItem(relatedItemInfoBeforeMove);
                if (relatedItem == null)
                {
                    return;
                }
                //1.Remove SP Object Related Column Info.
                var relatedProperties = GetRelatedProperties(relatedItem);
                relatedProperties.RemoveAll(r => r.id == physicalObjectId);
                UpdateSPRelatedProperties(relatedItem, relatedProperties);
                //2.Remove SP Explore Related Info.
                var id = IDGenerator.GetRecordId(relatedItemInfoBeforeMove.SiteId, relatedItemInfoBeforeMove.id);
                var record = ExplorerDao.ReadById(relatedItemInfoBeforeMove.SiteId, id);
                record.RelatedRecords = SerializeRelatedProperties(relatedProperties);
                record.RelatedRecordsCount = relatedProperties.Count;
                ExplorerDao.UpdatePhysicalRecord(record, false);
            }
            else if (relatedItemInfoBeforeMove.SourceFlag == (int)SourceFlag.Physical)
            {
                var record = ExplorerDao.ReadById(relatedItemInfoBeforeMove.SiteId, relatedItemInfoBeforeMove.id);
                var relatedProperties = DeserializeRelatedProperties(record.RelatedRecords);
                relatedProperties.RemoveAll(r => r.id == physicalObjectId);
                record.RelatedRecordsCount = relatedProperties.Count;
                record.RelatedRecords = SerializeRelatedProperties(relatedProperties);
                ExplorerDao.UpdatePhysicalRecord(record, false);
            }
            else if (relatedItemInfoBeforeMove.SourceFlag == (int)SourceFlag.SharePointOnPrem)
            {
                logger.Info($"remove related column for sponprem,id:{relatedItemInfoBeforeMove.id}");
                SharePointOnPremQuererResult listItem = null;
                try
                {
                    listItem = SharePointOnPremClient.GetSPOnPremiseItem(new Guid(relatedItemInfoBeforeMove.AveId), relatedItemInfoBeforeMove.WebId, relatedItemInfoBeforeMove.ListId, relatedItemInfoBeforeMove.id).GetAwaiter().GetResult();

                }
                catch (Exception ex)
                {
                    logger.Error($"remove related data failed,idL{relatedItemInfoBeforeMove.id},error:{ex}");
                }
                if (listItem == null)
                {
                    return;
                }
                var relatedProperties = GetRelatedPropertiesBySPColumnValue(listItem.RelatedRecordsInfo);
                relatedProperties.RemoveAll(r => r.id == physicalObjectId);
                UpdateSPOnpremRelatedProperties(relatedProperties, relatedItemInfoBeforeMove.SiteUrl, relatedItemInfoBeforeMove.SiteId, relatedItemInfoBeforeMove.WebId, relatedItemInfoBeforeMove.WebUrl, relatedItemInfoBeforeMove.ListId, relatedItemInfoBeforeMove.DocLibRowId, relatedItemInfoBeforeMove.name);
                var id = IDGenerator.GetRecordId(relatedItemInfoBeforeMove.SiteId, relatedItemInfoBeforeMove.id);
                var record = ExplorerDao.ReadById(relatedItemInfoBeforeMove.SiteId, id);
                record.RelatedRecords = SerializeRelatedProperties(relatedProperties);
                record.RelatedRecordsCount = relatedProperties.Count;
                ExplorerDao.UpdatePhysicalRecord(record, true);
                logger.Info($"finish remove related column for sponprem,id:{relatedItemInfoBeforeMove.id}");
            }
        }
        private void UpdateSPOnpremRelatedProperties(List<RMRelatedItemInfo> relatedInfo,string siteUrl,Guid siteId,Guid webId,string webUrl,Guid listId,int docLibRowId,string name)
        {
            var updateValue = ConvertToSPColumnValueString(relatedInfo);
            SharePointOnPremClient.UpdateSPItemRelatedProperties(
                siteUrl,
                siteId,
                webId,
                webUrl,
                listId,
                docLibRowId,
                name,
                updateValue
            ).GetAwaiter().GetResult();
        }
        private void UpdateSPRelatedProperties(IAveListItem item, List<RMRelatedItemInfo> relatedItemInfos)
        {
            if (item != null)
            {
                try
                {
                    var columnValue = ConvertRMRelatedItemInfosToColumnValueString(relatedItemInfos);
                    if (CheckIsRecord(item))
                    {
                        logger.Info("current file is Declare Status and will be Undo declare it.File:{0}", item.UniqueId);
                        Record.UndeclareItemAsRecord(item);
                        item[relatedColumnInternalName] = columnValue;
                        item.SystemUpdate();
                        Record.DeclareItemAsRecord(item);
                        logger.Info("Replace RecordsRelated Declare File Successful.File:{0}", item.UniqueId);
                    }
                    else
                    {
                        item[relatedColumnInternalName] = columnValue;
                        item.SystemUpdate();
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(string.Format("Error in update realted properties for item : {0}, reason : {1}", item["FileRef"].ToString(), ex.ToString()));
                }
            }
        }
        public string ConvertRMRelatedItemInfosToColumnValueString(List<RMRelatedItemInfo> relatedItemInfos)
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
                    rel = string.Format(columnStrcuture, rel, relatedItemInfo.url, relatedItemInfo.recId, physicalRelatedStyle);
                    physicalBuilder.Append(rel);
                }
                else
                {
                    rel = string.Format(columnStrcuture, rel, relatedItemInfo.url, relatedItemInfo.name, string.Empty);
                    electronicBuilder.Append(rel);
                }
            }
            //var noneInfo = string.Format("<p>{0}</p>", I18NEntity.GetString("RM_SS_RelatedRecords_Data_None"));
            var electronicInfo = electronicBuilder.Length > 0 ?
                string.Format(categoryHeader, "Electronic:") + electronicBuilder.ToString()
                : string.Empty;
            var physicalInfo = physicalBuilder.Length > 0 ?
                string.Format(categoryHeader, "Physical:") + physicalBuilder.ToString()
                : string.Empty;
            relatedInfo = string.Format(columnHeader, electronicInfo + physicalInfo);
            return relatedInfo;
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

        public void Dispose()
        {
            try
            {
                using (currentContext)
                { }
                lock (siteContextDic)
                {
                    if (siteContextDic.Count > 0 && !BatchOperation)
                    {
                        foreach (CachedSiteContext site in siteContextDic.Values)
                        {
                            try
                            {
                                site.ClientContext.Dispose();
                            }
                            catch (Exception ex)
                            {
                                logger.Warn("client dispose error {0}", ex.ToString());
                            }
                        }
                    }
                    siteContextDic.Clear();
                }
            }
            catch (Exception ex)
            {
                logger.Warn("client dispose error {0}", ex.ToString());
            }
        }
        /// <summary>
        /// 批量操作释放清除ClientContext缓存
        /// </summary>
        public static void ClearContextCache()
        {
            try
            {
                lock (siteContextDic)
                {
                    if (siteContextDic.Count > 0)
                    {
                        foreach (CachedSiteContext site in siteContextDic.Values)
                        {
                            try
                            {
                                site.ClientContext.Dispose();
                            }
                            catch (Exception ex)
                            {
                                logger.Warn("client dispose error {0}", ex.ToString());
                            }
                        }
                    }
                    siteContextDic.Clear();
                }
            }
            catch (Exception ex)
            {
                logger.Warn("client dispose error {0}", ex.ToString());
            }
        }
        public bool CheckIsRecord()
        {
            bool isRecord = false;
            int result = 0;
            try
            {
                //object obj = item[new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")];
                object obj = currentItem.FieldValues["_vti_ItemHoldRecordStatus"];
                if (obj != null && !int.TryParse(obj.ToString(), out result)) result = 0;
            }
            catch (ArgumentException ex)
            {
                result = 0;
            }
            catch (Exception e)
            {
                isRecord = false;
            }
            isRecord = IsBlockEditAndDeleteRecord(result);
            return isRecord;
        }

        public bool CheckIsRecord(IAveListItem item)
        {
            bool isRecord = false;
            int result = 0;
            try
            {
                //object obj = item[new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")];
                object obj = item.FieldValues["_vti_ItemHoldRecordStatus"];
                if (obj != null && !int.TryParse(obj.ToString(), out result)) result = 0;
            }
            catch (ArgumentException ex)
            {
                result = 0;
            }
            catch (Exception e)
            {
                isRecord = false;
            }
            isRecord = IsBlockEditAndDeleteRecord(result);
            return isRecord;
        }
        public bool IsBlockEditAndDeleteRecord(int holdAndRecordStatus)
        {
            return ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.RecordMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.EditBlockedMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.DeleteBlockedMask) != 0L);
        }
        internal enum HoldAndRecordStatusMask
        {
            EditBlockedMask = 1, //只要不允许编辑, 这位值就为1, 包括Hold 和 Block edit and delete
            RecordMask = 0x10, //Record 文件，这位值 就是1 ， 包含Block edit and delete， block delete
            DeleteBlockedMask = 0x100,//只要不允许删除，这位值就为1, 包括 Hold， block edit and delete， block delete
            HoldMask = 0x1000, //Hold 文件，这位值就是 1， 
        }

        public ClientContext GetClientContextWithAccessToken(string targetUrl, string accessToken)
        {
            ClientContext clientContext = new ClientContext(targetUrl);

            //clientContext.AuthenticationMode = ClientAuthenticationMode.Anonymous;
            //clientContext.FormDigestHandlingEnabled = false;
            clientContext.ExecutingWebRequest +=
                delegate (object oSender, WebRequestEventArgs webRequestEventArgs)
                {
                    webRequestEventArgs.WebRequestExecutor.RequestHeaders["Authorization"] =
                        "Bearer " + accessToken;
                };

            return clientContext;
        }
        /// <summary>
        /// 建议使用带缓存的方法:GetSiteContext 获取Client Context
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        private ClientContext InitContext(Guid siteId)
        {
            var siteCollection = GetSiteNode(siteId);//from cache
            CommonClientContext clientContext = new CommonClientContext();
            ClientContext context = clientContext.InitClientContext(siteCollection);
            currentSite = context.Site;
            return context;
        }

        private ClientContext InitContext(string siteUrl)
        {
            var siteCollection = GetSiteNode(siteUrl);//from cache
            CommonClientContext clientContext = new CommonClientContext();
            ClientContext context = clientContext.InitClientContext(siteCollection);
            currentSite = context.Site;
            return context;
        }
        //建议使用此方法GetContext，此方法会有缓存，提升性能
        private ClientContext GetSiteContext(Guid siteId)
        {
            lock (siteContextDic)
            {
                if (siteContextDic.ContainsKey(siteId) && siteContextDic[siteId].InitTime < DateTime.Now.AddHours(6))
                {
                    return siteContextDic[siteId].ClientContext;
                }
                else
                {
                    ClientContext context = this.InitContext(siteId);
                    siteContextDic[siteId] = new CachedSiteContext() { AveSiteId = siteId, ClientContext = context, InitTime = DateTime.Now };
                    return context;
                }
            }
        }

        //建议使用此方法GetContext，此方法会有缓存，提升性能
        private ClientContext GetSiteContext(string siteUrl)
        {
            var internalId = siteUrl.ToMd5();
            lock (siteContextDic)
            {
                if (siteContextDic.ContainsKey(internalId) && siteContextDic[internalId].InitTime < DateTime.Now.AddHours(6))
                {
                    return siteContextDic[internalId].ClientContext;
                }
                else
                {
                    ClientContext context = this.InitContext(siteUrl);
                    siteContextDic[internalId] = new CachedSiteContext() { AveSiteId = internalId, ClientContext = context, InitTime = DateTime.Now };
                    return context;
                }
            }
        }

        public RemoteSiteCollection GetSiteNode(Guid aveSiteId)
        {
            List<string> aveIds = new List<string>();
            aveIds.Add(aveSiteId.ToString());
            //return mDocAveClient.GetRemoteSiteCollectionsByIdList(aveIds).FirstOrDefault();
            return RABrowserClient.GetRemoteSiteCollectionsByIdList(aveIds).FirstOrDefault();
        }

        private List<RemoteSiteCollection> GetAllRemoteNodeSites()
        {
            return MemoryCacheUtility.Get(
                "AllSitesCache",
                TimeSpan.FromHours(1),
                () => RABrowserClient.GetAuthorisedRemoteSiteCollectionsByUser()
            );
        }
        public RemoteSiteCollection GetSiteNode(string siteUrl)
        {
            lock (mlock)
            {
                bool cacheRefreshed = false;
                if (mRemoteSiteCollectionCache != null)
                {
                    var siteInfo = mRemoteSiteCollectionCache.FirstOrDefault(a => a.url.Equals(siteUrl, StringComparison.OrdinalIgnoreCase));
                    if (siteInfo != null)
                    {
                        return siteInfo;
                    }
                }

                var allSites = GetAllRemoteNodeSites();
                if(allSites != mRemoteSiteCollectionCache)
                {
                    logger.Info($"All sites cache refreshed");
                    cacheRefreshed = true;
                    mRemoteSiteCollectionCache = allSites;
                }

                if (cacheRefreshed)
                {
                    return mRemoteSiteCollectionCache.FirstOrDefault(a => a.url.Equals(siteUrl, StringComparison.OrdinalIgnoreCase));
                }

                return null;
            }

        }

        //Private this method later
        public void UpdateRecordRelatedInfo(Guid id, List<RMRelatedItemInfo> updateResult)
        {
            ExplorerDao.UpdateAll(r => r.NodeId == id, rec =>
            {
                if (updateResult == null || updateResult.Count == 0)
                {
                    rec.RelatedRecordsCount = 0;
                    rec.RelatedRecords = string.Empty;
                }
                else
                {
                    rec.RelatedRecordsCount = updateResult.Count;
                    rec.RelatedRecords = SerializerHelper.SerializeToXmlString(updateResult);
                }
            });
        }

        public static string SerializeRelatedProperties(List<RMRelatedItemInfo> relatedItemInfos)
        {
            return relatedItemInfos.Count == 0 ? string.Empty : GCommon.Utility.SerializerHelper.SerializeToXmlString<List<RMRelatedItemInfo>>(relatedItemInfos);
        }

        public static List<RMRelatedItemInfo> DeserializeRelatedProperties(string relatedItemInfos)
        {
            return string.IsNullOrEmpty(relatedItemInfos) ? new List<RMRelatedItemInfo>() : GCommon.Utility.SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(relatedItemInfos);
        }
    }

    /// <summary>
    /// 缓存Site的Context， 有过期时间
    /// </summary>
    public class CachedSiteContext
    {
        internal Guid AveSiteId { set; get; }
        internal ClientContext ClientContext { set; get; }
        internal DateTime InitTime { set; get; }
    }
}
