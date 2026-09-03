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
//using AvePoint.GCommon;
//using AvePoint.GCommon.Utility.Cryptography;
//using AvePoint.PhysicalCore.API;
//using AvePoint.PhysicalCore.Contract;
//using AvePoint.PhysicalCore.Util;
//using AvePoint.StorageOptimization.Schedule.Common;
//using AvePoint.Wrapper.Common;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Web;
//using System.Web.Script.Serialization;
//using System.Xml;

namespace RAFileSystem.SharePoint.EnforceRuleAction.LeaveStub
{
    public class OnPremSPLeaveStubRelatedRecordsUtility// : IDisposable
    {
        //private AveLogger logger = AveLogger.GetInstance(typeof(RelatedRecordsUtility));
        ////protected IAveListItem CurrentItem;
        //private Guid relatedColumnId = new Guid("b40273fb-26d2-40e8-9a34-dd20bc9ca1d7");
        //private const string relatedColumnInternalName = "RecordsRelated";
        //private const string columnHeader = "<div class=\"ExternalClass702E44874C854B099A61564528AE0439\">{0}</div>";
        //private const string columnStrcuture = "<p>​<a rel =\"{0}\" href=\"{1}\">{2}</a>​</p>";
        //private const string categoryHeader = "<p style=\"font-weight:600\">{0}</p>";
        //private AveObjectModelFactory aveObjectModelFactory;
        //private ScheduleConfiguration config = null;

        //public RelatedRecordsUtility(ScheduleConfiguration config)
        //{
        //    this.config = config;
        //}

        ////public List<RMRelatedItemInfo> GetRelatedProperties(IAveListItem item)
        ////{

        ////    try
        ////    {

        ////        if (CurrentItem.FieldValues.ContainsKey(relatedColumnInternalName) && CurrentItem[relatedColumnInternalName] != null)
        ////        {
        ////            var sourceUrlValue = CurrentItem[relatedColumnInternalName].ToString();
        ////            List<RMRelatedItemInfo> infos = new List<RMRelatedItemInfo>();
        ////            XmlDocument xmlDoc = new XmlDocument();
        ////            // sourceUrlValue = HttpUtility.UrlDecode(sourceUrlValue);//??
        ////            sourceUrlValue = sourceUrlValue.Replace("&#58;", ":");
        ////            xmlDoc.LoadXml(sourceUrlValue);
        ////            foreach (var ele in xmlDoc.GetElementsByTagName("a"))
        ////            {
        ////                XmlElement element = ele as XmlElement;
        ////                var relatedObjString = element.GetAttribute("rel");
        ////                relatedObjString = HttpUtility.UrlDecode(relatedObjString);
        ////                JavaScriptSerializer jss = new JavaScriptSerializer();
        ////                RMRelatedItemInfo relatedObj = jss.Deserialize<RMRelatedItemInfo>(relatedObjString);
        ////                var relatedItemUrl = HttpUtility.UrlDecode(element.GetAttribute("href"));
        ////                //string url = string.Empty;
        ////                //if (!element.GetAttribute("href").StartsWith(relatedObj.SiteUrl))//parmDic["SiteUrl"]))
        ////                //{
        ////                //    var webServerRelativeUrl = currentWeb.ServerRelativeUrl;
        ////                //    url = element.GetAttribute("href").Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
        ////                //    url = relatedObj.SiteUrl + "/" + url;
        ////                //}
        ////                relatedObj.url = relatedItemUrl;
        ////                infos.Add(relatedObj);

        ////            }
        ////            return infos;
        ////        }
        ////    }
        ////    catch (Exception e)
        ////    {
        ////        logger.Warn("Get related records failed {0}:{1}", CurrentItem["FileRef"].ToString(), e.ToString());
        ////        throw;
        ////    }
        ////    return null;
        ////}

        //public List<RMRelatedItemInfo> GetRelatedProperties(IAveListItem item)
        //{
        //    try
        //    {
        //        if (item != null && item.FieldValues != null && item.FieldValues.ContainsKey(relatedColumnInternalName) && item[relatedColumnInternalName] != null)
        //        {
        //            var sourceUrlValue = item[relatedColumnInternalName].ToString();
        //            return GetSPRelatedProperties(sourceUrlValue);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Warn("Get related records failed {0}:{1}", item["FileRef"].ToString(), e.ToString());
        //    }
        //    return new List<RMRelatedItemInfo>();
        //}

        //private List<RMRelatedItemInfo> GetSPRelatedProperties(string recordValue)
        //{
        //    try
        //    {
        //        List<RMRelatedItemInfo> infos = new List<RMRelatedItemInfo>();
        //        XmlDocument xmlDoc = new XmlDocument();
        //        //+加号的文件(Folder)，HttpUtility.UrlDecode方法会把+加号变成空格，所以调用这个方法之前需要把+加号转成%2b.
        //        recordValue = recordValue.Replace("&#58;", ":").Replace("+", "%2b");
        //        xmlDoc.LoadXml(recordValue);
        //        foreach (var ele in xmlDoc.GetElementsByTagName("a"))
        //        {
        //            XmlElement element = ele as XmlElement;
        //            var relatedObjString = element.GetAttribute("rel");
        //            relatedObjString = HttpUtility.UrlDecode(relatedObjString);
        //            JavaScriptSerializer jss = new JavaScriptSerializer();
        //            RMRelatedItemInfo relatedObj = jss.Deserialize<RMRelatedItemInfo>(relatedObjString);
        //            var relatedItemUrl = HttpUtility.UrlDecode(element.GetAttribute("href"));
        //            //relatedObj.url = relatedItemUrl;
        //            infos.Add(relatedObj);

        //        }
        //        return infos;
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Warn(string.Format("Get related records failed, reason : {0}", e.ToString()));
        //        throw;
        //    }
        //}

        //public static List<RMRelatedItemInfo> GetRelatedProperties(string recordsRelatedValue)
        //{
        //    List<RMRelatedItemInfo> infos = new List<RMRelatedItemInfo>();
        //    if (!string.IsNullOrEmpty(recordsRelatedValue))
        //    {
        //        var sourceUrlValue = recordsRelatedValue;
        //        XmlDocument xmlDoc = new XmlDocument();
        //        sourceUrlValue = sourceUrlValue.Replace("&#58;", ":");
        //        xmlDoc.LoadXml(sourceUrlValue);
        //        if (xmlDoc.GetElementsByTagName("a").Count > 0)
        //        {
        //            foreach (var ele in xmlDoc.GetElementsByTagName("a"))
        //            {
        //                XmlElement element = ele as XmlElement;
        //                var relatedObjString = element.GetAttribute("rel");
        //                relatedObjString = HttpUtility.UrlDecode(relatedObjString);
        //                JavaScriptSerializer jss = new JavaScriptSerializer();
        //                RMRelatedItemInfo relatedObj = jss.Deserialize<RMRelatedItemInfo>(relatedObjString);
        //                var relatedItemUrl = HttpUtility.UrlDecode(element.GetAttribute("href"));
        //                string url = relatedItemUrl;
        //                relatedObj.url = relatedItemUrl;
        //                relatedObj.url = url;
        //                infos.Add(relatedObj);
        //            }
        //        }
        //        else if (xmlDoc.GetElementsByTagName("RMRelatedItemInfo").Count > 0)
        //        {
        //            infos = GCommon.Utility.SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(sourceUrlValue);
        //        }
        //    }
        //    return infos;
        //}

        //public RMRelatedItemInfo GenerateRMRelatedItemInfo(IAveListItem currentItem)
        //{
        //    RMRelatedItemInfo info = new RMRelatedItemInfo();
        //    info.DocLibRowId = currentItem.ID;
        //    var folder = currentItem.ParentList.ParentWeb.GetFolder(currentItem.FieldValues["FileDirRef"].ToString());
        //    if (folder != null && folder.Properties.ContainsKey("vti_etag") &&
        //            folder.Properties["vti_etag"] != null)
        //    {
        //        string tagString = folder.Properties["vti_etag"].ToString().Trim('"').Split(',')[0];
        //        info.FolderId = string.IsNullOrEmpty(tagString) ? Guid.Empty : new Guid(tagString);
        //        info.ParentFolderIsRootFolder = currentItem.ParentList.RootFolder.UniqueId == folder.UniqueId;
        //        info.FolderUrl = folder.ServerRelativeUrl;
        //    }
        //    info.id = currentItem.UniqueId;
        //    string displayName = string.Empty;
        //    if ((currentItem.FieldValues["FSObjType"] as string).Equals((0).ToString()))
        //    {
        //        if ((currentItem.FieldValues["FileLeafRef"] as string).EndsWith("_.000", StringComparison.OrdinalIgnoreCase))
        //        {
        //            displayName = currentItem.FieldValues["Title"] as string;
        //            if (string.IsNullOrEmpty(displayName))
        //            {
        //                displayName = "";
        //            }
        //        }
        //        else
        //        {
        //            displayName = currentItem.FieldValues["FileLeafRef"].ToString();
        //        }
        //    }
        //    else
        //    {
        //        displayName = currentItem.FieldValues["FileLeafRef"].ToString();
        //    }
        //    info.name = displayName;
        //    info.WebId = currentItem.ParentList.ParentWeb.ID;
        //    info.WebUrl = currentItem.ParentList.ParentWeb.Url;
        //    info.SiteId = currentItem.ParentList.ParentWeb.Site.ID;
        //    info.SiteUrl = currentItem.ParentList.ParentWeb.Site.Url;
        //    info.level = currentItem.ParentList.BaseType == AveBaseType.DocumentLibrary ? SOEndUserArchiverNodeLevel.Document : SOEndUserArchiverNodeLevel.Item;
        //    info.ListId = currentItem.ParentList.ID;
        //    info.WebServerRelativeUrl = currentItem.ParentList.ParentWeb.ServerRelativeUrl;
        //    info.ListUrl = currentItem.ParentList.RootFolder.ServerRelativeUrl;
        //    info.ItemUrl = currentItem.FieldValues["FileRef"].ToString();
        //    //info.url = info.SiteUrl + "/" + info.ItemUrl.Substring(currentItem.ParentList.ParentWeb.ServerRelativeUrl.TrimEnd('/').Length + 1);
        //    info.url = new Uri(info.SiteUrl).Scheme + @"://" + new Uri(info.SiteUrl).Authority + info.ItemUrl;
        //    info.SourceFlag = (int)SourceFlag.SharePoint;
        //    return info;
        //}

        ///// <summary>
        ///// 更新关联Item 的RelatedColumn 
        ///// </summary>
        ///// <param name="relatedItemInfoBeforeMove">Move之前文件的RelatedItemInfo， 通过这个对象可以获取到关联的Item</param>
        ///// <param name="siteUrlBeforeMove">Move之前文件的site url</param>
        ///// <param name="itemUrlBeforeMove">Move之前文件的item url, 支持server related url  和 full url</param>
        ///// 通过site url 和item url ，能从关联文件中找到与原文件的关联记录，进而可以更新这条信息
        ///// <param name="relatedInfoAfterMove">原文件被move到目的端后，目的端文件的属性，用于更新关联文件</param>
        ///// <param name="relatedItemAccountInfo">关联文件站点对应的注册信息，用于连接关联文件站点，进而更新关联文件</param>
        //public void UpdateSPRelatedSPColumnValue(RMRelatedItemInfo relatedItemInfoBeforeMove, string siteUrlBeforeMove, string itemUrlBeforeMove, RMRelatedItemInfo relatedInfoAfterMove, string relatedItemAccountInfo)
        //{
        //    var relatedItem = GetRelatedItem(relatedItemInfoBeforeMove, relatedItemAccountInfo);
        //    if (relatedItem == null)
        //    {
        //        return;
        //    }
        //    var relatedProperties = GetRelatedProperties(relatedItem);
        //    //Find the right RMRelatedItemInfo, remove the old one, and the new one
        //    relatedProperties.RemoveAll(r => r.SourceFlag == (int)SourceFlag.SharePoint && r.SiteUrl.Equals(siteUrlBeforeMove, StringComparison.OrdinalIgnoreCase) && (r.ItemUrl.Equals(itemUrlBeforeMove, StringComparison.OrdinalIgnoreCase) || r.url.Equals(itemUrlBeforeMove, StringComparison.OrdinalIgnoreCase)));
        //    relatedProperties.Add(relatedInfoAfterMove);
        //    UpdateSPRelatedProperties(relatedItem, relatedProperties);
        //}

        //public void UpdateSPRelatedPhysicalColumnValue(RMRelatedItemInfo relatedItemInfoBeforeMove, string siteUrlBeforeMove, string itemUrlBeforeMove, RMRelatedItemInfo relatedInfoAfterMove)
        //{
        //    PhysicalCore.CosmosDB.Record record = PhysicalConfiguration.PhysicalConfigurationForExplore.ExplorerDAO.ReadById(Guid.Empty, relatedItemInfoBeforeMove.id);
        //    List<RMRelatedItemInfo> relatedItemInfos = RelatedRecordsUtility.GetRelatedProperties(record.RelatedRecords);
        //    relatedItemInfos.RemoveAll(r => r.SiteUrl.Equals(siteUrlBeforeMove, StringComparison.OrdinalIgnoreCase) && (r.ItemUrl.Equals(itemUrlBeforeMove, StringComparison.OrdinalIgnoreCase) || r.url.Equals(itemUrlBeforeMove, StringComparison.OrdinalIgnoreCase)));
        //    relatedItemInfos.Add(relatedInfoAfterMove);
        //    record.RelatedRecords = RelatedRecordsUtility.SerializeRelatedProperties(relatedItemInfos);
        //    record.RelatedRecordsCount = relatedItemInfos.Count;
        //    PhysicalConfiguration.PhysicalConfigurationForExplore.ExplorerDAO.UpdatePhysicalRecord(record, true);
        //}

        ///// <summary>
        ///// 移除关联Item 的RelatedColumn 
        ///// </summary>
        ///// <param name="relatedItemInfoBeforeMove">Move之前文件的RelatedItemInfo， 通过这个对象可以获取到关联的Item</param>
        ///// <param name="siteUrlBeforeMove">Move之前文件的site url</param>
        ///// <param name="itemUrlBeforeMove">Move之前文件的item url, 支持server related url  和 full url</param>
        ///// 通过site url 和item url ，能从关联文件中找到与原文件的关联记录，进而可以更新这条信息
        ///// <param name="relatedItemAccountInfo">关联文件站点对应的注册信息，用于连接关联文件站点，进而更新关联文件</param>
        //public void RemoveRelateColumnValue(RMRelatedItemInfo relatedItemInfoBeforeMove, IAveSite site, string itemUrlBeforeMove, Guid itemId, string relatedItemAccountInfo)
        //{
        //    string siteUrlBeforeMove = site.Url;
        //    if (relatedItemInfoBeforeMove.SourceFlag == (int)SourceFlag.All || relatedItemInfoBeforeMove.SourceFlag == (int)SourceFlag.SharePoint)
        //    {
        //        var relatedItem = GetRelatedItem(relatedItemInfoBeforeMove, relatedItemAccountInfo);
        //        if (relatedItem == null)
        //        {
        //            return;
        //        }
        //        var relatedProperties = GetRelatedProperties(relatedItem);
        //        //Find the right RMRelatedItemInfo, remove it.
        //        //考虑到老数据可能出现siteid 或者siteurl为空的case，此处添加兼容逻辑。
        //        relatedProperties.RemoveAll(r => (r.SiteId == site.ID || (r.SiteUrl != null && r.SiteUrl.Equals(siteUrlBeforeMove, StringComparison.OrdinalIgnoreCase)))
        //                                    &&
        //                                    (r.ItemUrl.Equals(itemUrlBeforeMove, StringComparison.OrdinalIgnoreCase)
        //                                    || r.url.Equals(itemUrlBeforeMove, StringComparison.OrdinalIgnoreCase)
        //                                    || r.id == itemId));
        //        UpdateSPRelatedProperties(relatedItem, relatedProperties);
        //        //DAO 目前对于SP ，只更新了SP 没更新DB ，稍后需要加回来下面逻辑
        //        var id = ScheduleConfiguration.GetRecordId(relatedItemInfoBeforeMove.SiteId, relatedItemInfoBeforeMove.id);
        //        PhysicalCore.CosmosDB.Record record = PhysicalConfiguration.PhysicalConfigurationForExplore.ExplorerDAO.ReadById(relatedItemInfoBeforeMove.SiteId, id);
        //        record.RelatedRecords = RelatedRecordsUtility.SerializeRelatedProperties(relatedProperties);
        //        record.RelatedRecordsCount = relatedProperties.Count;
        //        PhysicalConfiguration.PhysicalConfigurationForExplore.ExplorerDAO.UpdatePhysicalRecord(record, false);
        //    }
        //    else if (relatedItemInfoBeforeMove.SourceFlag == (int)SourceFlag.Physical)
        //    {
        //        PhysicalCore.CosmosDB.Record record = PhysicalConfiguration.PhysicalConfigurationForExplore.ExplorerDAO.ReadById(relatedItemInfoBeforeMove.SiteId, relatedItemInfoBeforeMove.id);
        //        var relatedProperties = RelatedRecordsUtility.DeserializeRelatedProperties(record.RelatedRecords);
        //        relatedProperties.RemoveAll(r => r.SiteUrl.Equals(siteUrlBeforeMove, StringComparison.OrdinalIgnoreCase)
        //                                         && (r.ItemUrl.Equals(itemUrlBeforeMove, StringComparison.OrdinalIgnoreCase)
        //                                             || r.url.Equals(itemUrlBeforeMove, StringComparison.OrdinalIgnoreCase)
        //                                             || r.id == itemId));
        //        record.RelatedRecordsCount = relatedProperties.Count;
        //        record.RelatedRecords = RelatedRecordsUtility.SerializeRelatedProperties(relatedProperties);
        //        PhysicalConfiguration.PhysicalConfigurationForExplore.ExplorerDAO.UpdatePhysicalRecord(record, false);
        //    }
        //}

        //public void RemoveRelateColumnValue(RMRelatedItemInfo relatedItemInfoBeforeMove, Guid physicalObjectId)
        //{
        //    if (relatedItemInfoBeforeMove.SourceFlag == (int)SourceFlag.All || relatedItemInfoBeforeMove.SourceFlag == (int)SourceFlag.SharePoint)
        //    {
        //        var relatedItem = GetRelatedItem(relatedItemInfoBeforeMove, "");
        //        if (relatedItem == null)
        //        {
        //            return;
        //        }
        //        //1.Remove SP Object Related Column Info.
        //        var relatedProperties = GetRelatedProperties(relatedItem);
        //        relatedProperties.RemoveAll(r => r.id == physicalObjectId);
        //        UpdateSPRelatedProperties(relatedItem, relatedProperties);
        //        //2.Remove SP Explore Related Info.
        //        var id = ScheduleConfiguration.GetRecordId(relatedItemInfoBeforeMove.SiteId, relatedItemInfoBeforeMove.id);
        //        PhysicalCore.CosmosDB.Record record = PhysicalConfiguration.PhysicalConfigurationForExplore.ExplorerDAO.ReadById(relatedItemInfoBeforeMove.SiteId, id);
        //        record.RelatedRecords = RelatedRecordsUtility.SerializeRelatedProperties(relatedProperties);
        //        record.RelatedRecordsCount = relatedProperties.Count;
        //        PhysicalConfiguration.PhysicalConfigurationForExplore.ExplorerDAO.UpdatePhysicalRecord(record, false);
        //    }
        //    else if (relatedItemInfoBeforeMove.SourceFlag == (int)SourceFlag.Physical)
        //    {
        //        PhysicalCore.CosmosDB.Record record = PhysicalConfiguration.PhysicalConfigurationForExplore.ExplorerDAO.ReadById(relatedItemInfoBeforeMove.SiteId, relatedItemInfoBeforeMove.id);
        //        var relatedProperties = RelatedRecordsUtility.DeserializeRelatedProperties(record.RelatedRecords);
        //        relatedProperties.RemoveAll(r => r.id == physicalObjectId);
        //        record.RelatedRecordsCount = relatedProperties.Count;
        //        record.RelatedRecords = RelatedRecordsUtility.SerializeRelatedProperties(relatedProperties);
        //        PhysicalConfiguration.PhysicalConfigurationForExplore.ExplorerDAO.UpdatePhysicalRecord(record, false);
        //    }
        //}

        //public void AddRelateColumnValue(RMRelatedItemInfo sourceRMRelatedItemInfo, IPhysicalFile desFile)
        //{
        //    if (sourceRMRelatedItemInfo.SourceFlag == (int)SourceFlag.All || sourceRMRelatedItemInfo.SourceFlag == (int)SourceFlag.SharePoint)
        //    {
        //        var relatedItem = GetRelatedItem(sourceRMRelatedItemInfo, "");
        //        if (relatedItem == null)
        //        {
        //            return;
        //        }
        //        var sourceObjectRelatedProperties = GetRelatedProperties(relatedItem);
        //        var desRelatedProperties = RelatedRecordsUtility.DeserializeRelatedProperties(desFile.RelatedRecordInfo);
        //        if (!desRelatedProperties.Exists(x => x.id == sourceRMRelatedItemInfo.id))
        //        {
        //            //1.目的端Folder添加源端Folder关联的RelatedInfo.
        //            desRelatedProperties.Add(sourceRMRelatedItemInfo);
        //            desFile.RelatedRecordsCount = desRelatedProperties.Count;
        //            desFile.RelatedRecordInfo = RelatedRecordsUtility.SerializeRelatedProperties(desRelatedProperties);
        //            //2.源端Related SP Object添加目的端Folder RelatedInfo.
        //            sourceObjectRelatedProperties.Add(new RMRelatedItemInfo()
        //            {
        //                id = desFile.Id,
        //                recId = desFile.RecordId,
        //                SourceFlag = (int)SourceFlag.Physical,
        //                //PhyBox = 9300,PhyFile = 9400,PhyRecord = 9500,
        //                NodeType = 9400
        //            }
        //            );
        //            UpdateSPRelatedProperties(relatedItem, sourceObjectRelatedProperties);
        //            //3.源端Related SP Object添加目的端Folder ExploreDB Info.
        //            var id = ScheduleConfiguration.GetRecordId(sourceRMRelatedItemInfo.SiteId, sourceRMRelatedItemInfo.id);
        //            PhysicalCore.CosmosDB.Record record = PhysicalConfiguration.PhysicalConfigurationForExplore.ExplorerDAO.ReadById(sourceRMRelatedItemInfo.SiteId, id);
        //            if (record != null)
        //            {
        //                record.RelatedRecords = RelatedRecordsUtility.SerializeRelatedProperties(sourceObjectRelatedProperties);
        //                record.RelatedRecordsCount = sourceObjectRelatedProperties.Count;
        //                PhysicalConfiguration.PhysicalConfigurationForExplore.ExplorerDAO.UpdatePhysicalRecord(record, false);
        //            }
        //        }
        //    }
        //    else if (sourceRMRelatedItemInfo.SourceFlag == (int)SourceFlag.Physical)
        //    {
        //        var desRelatedProperties = RelatedRecordsUtility.DeserializeRelatedProperties(desFile.RelatedRecordInfo);
        //        if (!desRelatedProperties.Exists(x => x.id == sourceRMRelatedItemInfo.id))
        //        {
        //            //1.目的端Folder添加源端Folder关联的RelatedInfo.
        //            desRelatedProperties.Add(sourceRMRelatedItemInfo);
        //            desFile.RelatedRecordsCount = desRelatedProperties.Count;
        //            desFile.RelatedRecordInfo = RelatedRecordsUtility.SerializeRelatedProperties(desRelatedProperties);
        //            //2.源端Folder关联的Related数据需要添加目的端Folder RelatedInfo.
        //            PhysicalCore.CosmosDB.Record record = PhysicalConfiguration.PhysicalConfigurationForExplore.ExplorerDAO.ReadById(sourceRMRelatedItemInfo.SiteId, sourceRMRelatedItemInfo.id);
        //            var relatedProperties = RelatedRecordsUtility.DeserializeRelatedProperties(record.RelatedRecords);
        //            relatedProperties.Add(new RMRelatedItemInfo()
        //            {
        //                id = desFile.Id,
        //                recId = desFile.RecordId,
        //                SourceFlag = (int)SourceFlag.Physical,
        //                //PhyBox = 9300,PhyFile = 9400,PhyRecord = 9500,
        //                NodeType = 9400
        //            }
        //            );
        //            record.RelatedRecordsCount = relatedProperties.Count;
        //            record.RelatedRecords = RelatedRecordsUtility.SerializeRelatedProperties(relatedProperties);
        //            PhysicalConfiguration.PhysicalConfigurationForExplore.ExplorerDAO.UpdatePhysicalRecord(record, false);
        //        }
        //    }
        //}

        //public string ConvertRMRelatedItemInfosToColumnValueString(List<RMRelatedItemInfo> relatedItemInfos)
        //{
        //    string relatedInfo = string.Empty;
        //    if (relatedItemInfos == null || relatedItemInfos.Count == 0)
        //    {
        //        return relatedInfo;
        //    }
        //    StringBuilder electronicBuilder = new StringBuilder();
        //    StringBuilder physicalBuilder = new StringBuilder();
        //    foreach (var relatedItemInfo in relatedItemInfos)
        //    {
        //        JavaScriptSerializer jss = new JavaScriptSerializer();
        //        string rel = jss.Serialize(relatedItemInfo);
        //        rel = HttpUtility.HtmlEncode(rel);
        //        rel = rel.TrimStart('[').TrimEnd(']');
        //        if (relatedItemInfo.SourceFlag == (int)SourceFlag.Physical)
        //        {
        //            rel = string.Format(columnStrcuture, rel, relatedItemInfo.url, relatedItemInfo.recId);
        //            physicalBuilder.Append(rel);
        //        }
        //        else
        //        {
        //            rel = string.Format(columnStrcuture, rel, relatedItemInfo.url, relatedItemInfo.name);
        //            electronicBuilder.Append(rel);
        //        }
        //    }
        //    //var noneInfo = string.Format("<p>{0}</p>", I18NEntity.GetString("RM_SS_RelatedRecords_Data_None"));
        //    var electronicInfo = electronicBuilder.Length > 0 ?
        //        string.Format(categoryHeader, "Electronic:") + electronicBuilder.ToString()
        //        : string.Empty;
        //    var physicalInfo = physicalBuilder.Length > 0 ?
        //        string.Format(categoryHeader, "Physical:") + physicalBuilder.ToString()
        //        : string.Empty;
        //    relatedInfo = string.Format(columnHeader, electronicInfo + physicalInfo);
        //    return relatedInfo;
        //}

        ////public string ConvertRMRelatedItemInfosToColumnValueString(List<RMRelatedItemInfo> relatedItemInfos)
        ////{
        ////    string relatedInfo = string.Empty;
        ////    StringBuilder builder = new StringBuilder();
        ////    foreach (var relatedItemInfo in relatedItemInfos)
        ////    {
        ////        JavaScriptSerializer jss = new JavaScriptSerializer();
        ////        string rel = jss.Serialize(relatedItemInfo);
        ////        rel = HttpUtility.HtmlEncode(rel);
        ////        rel = rel.TrimStart('[').TrimEnd(']');
        ////        rel = string.Format(columnStrcuture, rel, relatedItemInfo.url, relatedItemInfo.name);
        ////        builder.Append(rel);
        ////    }
        ////    relatedInfo = string.Format(columnHeader, builder.ToString());
        ////    return relatedInfo;
        ////}

        //private IAveSite GetIAveSite(string siteUrl)
        //{
        //    var siteInfo = config.GetRemoteNodeInfo(siteUrl);
        //    if (siteInfo == null) { throw new Exception(string.Format("Site does not be registed in AOS.")); }
        //    if (!string.IsNullOrEmpty(siteInfo.Password))
        //    {
        //        //bposInfo.ConvertToAveBPOSAccountInfo() need give WrapKey password but aos get is unwrap.
        //        siteInfo.Password = CspCommunicationWrapper.WrapKeyToBase64String(CryptoUtil.ConvertStringToBytes(siteInfo.Password));
        //    }
        //    AvePoint.GCommon.Contract.CentralAdmin.Object.BposInfo bposInfo = new GCommon.Contract.CentralAdmin.Object.BposInfo()
        //    {
        //        SiteUrl = siteInfo.SiteUrl,
        //        ConnectionType = siteInfo.BposConnectionType,
        //        TenantGroupId = siteInfo.TenantGroupId,

        //        UserAccountInfo = new GCommon.Contract.CentralAdmin.Object.BposUserAccountInfo()
        //        {
        //            Domain = siteInfo.DomainName,
        //            Username = siteInfo.UserName,
        //            Password = siteInfo.Password,
        //            AppClientId = siteInfo.AppClientId,
        //            AppCertSecret = siteInfo.AppCertSecret,
        //            AppCertContent = siteInfo.AppCertContent,
        //            AppCertSecretContent = siteInfo.AppCertSecretContent,
        //            TenantId = siteInfo.TenantId,
        //            AdminUrl = siteInfo.AdminUrl,
        //            AppId = siteInfo.AppId,
        //        },
        //    };
        //    AveBPOSAccountInfo user = bposInfo.ConvertToAveBPOSAccountInfo();
        //    aveObjectModelFactory = AveObjectModelFactory.CreateObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
        //    var aveSite = aveObjectModelFactory.CreateSite(siteUrl);
        //    return aveSite;
        //}

        //private IAveListItem GetRelatedItem(RMRelatedItemInfo relatedInfo, string desAccountInfo)
        //{
        //    IAveListItem item = null;
        //    try
        //    {
        //        IAveSite site = GetIAveSite(relatedInfo.SiteUrl);
        //        var web = site.OpenWeb(relatedInfo.WebId);
        //        item = web.GetListItem(relatedInfo.ItemUrl, relatedInfo.ListId, relatedInfo.id);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Info("Can't get related Item:{0}.Message:{1}.", relatedInfo.url, ex.ToString());
        //    }
        //    return item;
        //}

        //private void UpdateSPRelatedProperties(IAveListItem item, List<RMRelatedItemInfo> relatedItemInfos)
        //{
        //    if (item != null)
        //    {
        //        try
        //        {
        //            var columnValue = ConvertRMRelatedItemInfosToColumnValueString(relatedItemInfos);
        //            if (ScheduleConfiguration.CheckisRecord(item))
        //            {
        //                logger.Info("current file is Declare Status and will be Undo declare it.File Name:{0}", item.Name);
        //                aveObjectModelFactory.CreateRecords().UndeclareItemAsRecord(item);
        //                item[relatedColumnInternalName] = columnValue;
        //                item.SystemUpdate();
        //                aveObjectModelFactory.CreateRecords().DeclareItemAsRecord(item);
        //                logger.Info("Replace RecordsRelated Declare File Successful.File Name:{0}", item.Name);
        //            }
        //            else
        //            {
        //                item[relatedColumnInternalName] = columnValue;
        //                item.SystemUpdate();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Warn(string.Format("Error in update realted properties for item : {0}, reason : {1}", item["FileRef"].ToString(), ex.ToString()));
        //        }
        //    }
        //}


        //public static string SerializeRelatedProperties(List<RMRelatedItemInfo> relatedItemInfos)
        //{
        //    return relatedItemInfos.Count == 0 ? string.Empty : GCommon.Utility.SerializerHelper.SerializeToXmlString<List<RMRelatedItemInfo>>(relatedItemInfos);
        //}

        //public static List<RMRelatedItemInfo> DeserializeRelatedProperties(string relatedItemInfos)
        //{
        //    return string.IsNullOrEmpty(relatedItemInfos) ? new List<RMRelatedItemInfo>() : GCommon.Utility.SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(relatedItemInfos);
        //}

        ////public void Dispose()
        ////{
        ////    throw new NotImplementedException();
        ////}
    }
}

