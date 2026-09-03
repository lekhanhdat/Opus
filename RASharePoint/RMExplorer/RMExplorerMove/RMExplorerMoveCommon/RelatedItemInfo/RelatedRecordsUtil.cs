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

using AvePoint.GCommon;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using ArgumentCheck = AvePoint.Wrapper.Common.ArgumentCheck;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    [Obsolete("use RelatedRecordsUtility now")]
    public class RelatedRecordsUtil// : IDisposable
    {
        private AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RelatedRecordsUtil));
        //protected IAveListItem CurrentItem;
        private const string relatedColumnInternalName = "RecordsRelated";
        private const string columnHeader = "<div class=\"ExternalClass702E44874C854B099A61564528AE0439\">{0}</div>";
        private const string categoryHeader = "<p style=\"font-weight:600\">{0}</p>";
        private const string columnStrcuture = "<p>​<a rel =\"{0}\" href=\"{1}\">{2}</a>​</p>";

        public RelatedRecordsUtil()
        {
        }

        public List<RMRelatedItemInfo> GetRelatedProperties(IAveListItem item)
        {
            try
            {
                if (item != null && item.FieldValues != null && item.FieldValues.ContainsKey(relatedColumnInternalName) && item[relatedColumnInternalName] != null)
                {
                    var sourceUrlValue = item[relatedColumnInternalName].ToString();
                    return GetRelatedProperties(sourceUrlValue);
                }
            }
            catch (Exception e)
            {
                ArgumentCheck.CheckNotNull(item);
                logger.Warn("Get related records failed {0}:{1}", item["FileRef"].ToString(), e.ToString());
                throw;
            }
            return null;
        }

        public List<RMRelatedItemInfo> GetRelatedProperties(string recordValue)
        {
            try
            {
                List<RMRelatedItemInfo> infos = new List<RMRelatedItemInfo>();
                XmlDocument xmlDoc = new XmlDocument();
                recordValue = recordValue.Replace("&#58;", ":").Replace("+", "%2b");
                xmlDoc.LoadXml(recordValue);
                foreach (var ele in xmlDoc.GetElementsByTagName("a"))
                {
                    XmlElement element = ele as XmlElement;
                    var relatedObjString = element.GetAttribute("rel");
                    relatedObjString = HttpUtility.UrlDecode(relatedObjString);
                    RMRelatedItemInfo relatedObj = SerializerHelper.DeserializeByJsonSerializer<RMRelatedItemInfo>(relatedObjString);
                    var relatedItemUrl = HttpUtility.UrlDecode(element.GetAttribute("href"));
                    //relatedObj.url = relatedItemUrl;
                    infos.Add(relatedObj);

                }
                return infos;
            }
            catch (Exception e)
            {
                logger.Warn(string.Format("Get related records failed, reason : {0}", e.ToString()));
                throw;
            }
            return null;
        }

        public RMRelatedItemInfo GenerateRMRelatedItemInfo(IAveListItem currentItem)
        {
            RMRelatedItemInfo info = new RMRelatedItemInfo();
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
            //info.url = info.SiteUrl + "/" + info.ItemUrl.Substring(currentItem.ParentList.ParentWeb.ServerRelativeUrl.TrimEnd('/').Length + 1);
            info.url = new Uri(info.SiteUrl).Scheme + @"://" + new Uri(info.SiteUrl).Authority + info.ItemUrl;
            return info;
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
        public async System.Threading.Tasks.Task UpdateRelateColumnValueAsync(RMRelatedItemInfo relatedItemInfoBeforeMove, string siteUrlBeforeMove, string itemUrlBeforeMove, RMRelatedItemInfo relatedInfoAfterMove, string relatedItemAccountInfo)
        {
            var relatedItem = await GetRelatedItemAsync(relatedItemInfoBeforeMove, relatedItemAccountInfo);
            var relatedProperties = GetRelatedProperties(relatedItem);
            //Find the right RMRelatedItemInfo, remove the old one, and the new one
            relatedProperties.RemoveAll(r => siteUrlBeforeMove.Equals(r.SiteUrl, StringComparison.OrdinalIgnoreCase)
            && (itemUrlBeforeMove.Equals(r.ItemUrl, StringComparison.OrdinalIgnoreCase) || itemUrlBeforeMove.Equals(r.url, StringComparison.OrdinalIgnoreCase)));
            relatedProperties.Add(relatedInfoAfterMove);
            UpdateRelatedProperties(relatedItem, relatedProperties);
            var dbUtil = new RMExplorerMoveDBUtil();
            dbUtil.UpdateRecordRelatedInfo(relatedItemInfoBeforeMove.id, relatedProperties);
        }

        public void UpdateRelateColumnValuePhysical(RMRelatedItemInfo relatedItemInfoBeforeMove, string siteUrlBeforeMove, string itemUrlBeforeMove, RMRelatedItemInfo relatedInfoAfterMove, string relatedItemAccountInfo)
        {
            var dbUtil = new RMExplorerMoveDBUtil();
            var relatedProperties = ConvertDBValueToRelatedProperties(dbUtil.GetRecord4Phy(relatedItemInfoBeforeMove.id).RelatedRecords);
            //Find the right RMRelatedItemInfo, remove the old one, and the new one
            relatedProperties.RemoveAll(r => siteUrlBeforeMove.Equals(r.SiteUrl, StringComparison.OrdinalIgnoreCase)
            && (itemUrlBeforeMove.Equals(r.ItemUrl, StringComparison.OrdinalIgnoreCase) || itemUrlBeforeMove.Equals(r.url, StringComparison.OrdinalIgnoreCase)));
            relatedProperties.Add(relatedInfoAfterMove);
            dbUtil.UpdateRecordRelatedInfo(relatedItemInfoBeforeMove.id, relatedProperties);
        }

        public List<RMRelatedItemInfo> ConvertDBValueToRelatedProperties(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                List<RMRelatedItemInfo> infos = GCommon.Utility.SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(value);
                return infos;
            }
            return null;
        }

        /// <summary>
        /// 移除关联Item 的RelatedColumn 
        /// </summary>
        /// <param name="relatedItemInfoBeforeMove">Move之前文件的RelatedItemInfo， 通过这个对象可以获取到关联的Item</param>
        /// <param name="siteUrlBeforeMove">Move之前文件的site url</param>
        /// <param name="itemUrlBeforeMove">Move之前文件的item url, 支持server related url  和 full url</param>
        /// 通过site url 和item url ，能从关联文件中找到与原文件的关联记录，进而可以更新这条信息
        /// <param name="relatedItemAccountInfo">关联文件站点对应的注册信息，用于连接关联文件站点，进而更新关联文件</param>
        public async System.Threading.Tasks.Task RemoveRelateColumnValueAsync(RMRelatedItemInfo relatedItemInfoBeforeMove, string siteUrlBeforeMove, string itemUrlBeforeMove, string relatedItemAccountInfo)
        {
            var relatedItem = await GetRelatedItemAsync(relatedItemInfoBeforeMove, relatedItemAccountInfo);
            var relatedProperties = GetRelatedProperties(relatedItem);
            //Find the right RMRelatedItemInfo, remove it.
            relatedProperties.RemoveAll(r => r.SiteUrl.Equals(siteUrlBeforeMove, StringComparison.OrdinalIgnoreCase) && (r.ItemUrl.Equals(itemUrlBeforeMove, StringComparison.OrdinalIgnoreCase) || r.url.Equals(itemUrlBeforeMove, StringComparison.OrdinalIgnoreCase)));
            UpdateRelatedProperties(relatedItem, relatedProperties);
        }

        public string ConvertRMRelatedItemInfosToColumnValueString(List<RMRelatedItemInfo> relatedItemInfos)
        {
            string relatedInfo = string.Empty;
            if (relatedItemInfos == null)
            {
                return relatedInfo;
            }
            StringBuilder electronicBuilder = new StringBuilder();
            StringBuilder physicalBuilder = new StringBuilder();
            foreach (var relatedItemInfo in relatedItemInfos)
            {
                string rel = SerializerHelper.SerializeByJsonConvert(relatedItemInfo);
                rel = HttpUtility.HtmlEncode(rel);
                rel = rel.TrimStart('[').TrimEnd(']');
                if (relatedItemInfo.SourceFlag == (int)SourceFlag.Physical)
                {
                    rel = string.Format(columnStrcuture, rel, relatedItemInfo.url, relatedItemInfo.recId);
                    physicalBuilder.Append(rel);
                }
                else
                {
                    rel = string.Format(columnStrcuture, rel, relatedItemInfo.url, relatedItemInfo.name);
                    electronicBuilder.Append(rel);
                }
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

        private async Task<IAveSite> GetIAveSiteAsync(string siteUrl)
        {
            
            var siteInfo = RegistedSiteCache.CreateInstance().GetAccountInfoBySiteUrl(siteUrl);
            if (siteInfo == null) { throw new Exception(string.Format("Site : {0} does not be registed in DocAve.", siteUrl)); }
            var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(siteInfo);
            var aveObjectModelFactory = MultiAppUtil.CreateAveObjectModelFactory(siteUrl, bposInfo, AveContextKind.ClientObjectModel);
            var aveSite = aveObjectModelFactory.CreateSite(siteUrl);
            return aveSite;
        }

        private async Task<IAveListItem> GetRelatedItemAsync(RMRelatedItemInfo relatedInfo, string desAccountInfo)
        {
            IAveListItem item = null;
            IAveSite site = await GetIAveSiteAsync(relatedInfo.SiteUrl);
            var web = site.OpenWeb(relatedInfo.WebId);
            item = web.GetListItem(relatedInfo.ItemUrl, relatedInfo.ListId, relatedInfo.id);
            return item;
        }

        private void UpdateRelatedProperties(IAveListItem item, List<RMRelatedItemInfo> relatedItemInfos)
        {
            if (item != null)
            {
                try
                {
                    var columnValue = ConvertRMRelatedItemInfosToColumnValueString(relatedItemInfos);
                    item[relatedColumnInternalName] = columnValue;
                    item.SystemUpdate();
                }
                catch(Exception ex)
                {
                    logger.Warn(string.Format("Error in update realted properties for item : {0}, reason : {1}", item["FileRef"].ToString(), ex.ToString()));
                    throw;
                }
            }
        }


        //public void Dispose()
        //{
        //    throw new NotImplementedException();
        //}
    }
}