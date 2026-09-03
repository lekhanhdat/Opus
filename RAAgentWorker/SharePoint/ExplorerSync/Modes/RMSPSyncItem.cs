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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.SharePoint.ExplorerSync.Cache;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections;
using System.Collections.Generic;
using AvePoint.RA.SharePoint.Extension;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AvePoint.RA.Contract.Explorer;
using Newtonsoft.Json;
using AvePoint.RA.CommonUtil;
using RAFileSystem.SharePoint.ExplorerSync.Cache;
using RAFileSystem.Utils;
using AvePoint.GCommon;
using RAFileSystem.SharePoint.Common;
using System.Xml;
using System.Web;
using AvePoint.RA.Common.Global.Utils;

namespace AvePoint.RA.SharePoint.ExplorerSync.Modes
{
    public class RMSPSyncItem
    {
        protected static readonly AveLogger logger = AveLogger.GetInstance(typeof(RMSPSyncItem));

        protected IAveTimeZone mSPWebTimeZone { get; set; }

        public RMSPSyncItem(RMSPExplorerSiteLevelCache cache)
        {
            siteCahce = cache;
        }

        public void InitTimeZone(IAveTimeZone timeZone)
        {
            mSPWebTimeZone = timeZone;
        }
        private RMSPExplorerSiteLevelCache siteCahce;

        public RecordDto AssembleRecord(AveDiscoverSite discoverObj, SyncItemRuleInfo itemRule)
        {
            var aveSite = discoverObj.Site;
            var siteId = discoverObj.SiteID.ToString();
            var rootWeb = discoverObj.GetRootWeb().AveWeb;
            var recId = AvePoint.RA.SharePoint.ExplorerSync.Utils.IDGenerator.GetRecordId(discoverObj.SiteID, discoverObj.SiteID);
            return new RecordDto()
            {
                Id = recId,
                RecordsId = string.Empty,
                LeafName = rootWeb.Title,
                FullPath = rootWeb.Url,
                ScopeId = siteCahce.SPSiteId,
                DirPath = rootWeb.Url,
                NodeId = discoverObj.SiteID,
                AveSiteId = siteCahce.AveSiteId,
                CollectionTime = DateTime.UtcNow.Ticks,
                TimeCreated = rootWeb.Created.Ticks,
                CreateDate = int.Parse(rootWeb.Created.ToString("yyyyMMdd")),
                NodeType = (int)NodeLevel.SiteCollection,
                TermId = itemRule.TermInfo.UniqueId,
                TermName = itemRule.TermInfo.Name,
                SourceFlag = (int)SourceFlag.SharePointOnPrem,
                DisposalDueDate = itemRule.DisposalAction,
                RuleId = itemRule.Rule != null ? new Guid(itemRule.Rule.Id) : Guid.Empty,
                RuleLevel = itemRule.Rule != null ? (int)itemRule.Rule.PolicyLevel : 0,
                HoldStatus = false,
                RecordStatus = (int)RMRecordStatus.Active,
                SortTicks = Snowflake.Instance().GetTicks()
            };
        }

        public RecordDto AssembleRecord(AveDiscoverWeb discoverObj, SyncItemRuleInfo itemRule)
        {
            var aveWeb = discoverObj.AveWeb;
            var recId = AvePoint.RA.SharePoint.ExplorerSync.Utils.IDGenerator.GetRecordId(siteCahce.SPSiteId, aveWeb.ID);
            return new RecordDto()
            {
                Id = recId,
                RecordsId = string.Empty,
                LeafName = discoverObj.Title,
                FullPath = aveWeb.Url,
                ScopeId = siteCahce.SPSiteId,
                DirPath = aveWeb.Url,
                NodeId = aveWeb.ID,
                WebId = aveWeb.ID,
                AveSiteId = siteCahce.AveSiteId,
                ListId = Guid.Empty,
                ItemId = Guid.Empty,
                CollectionTime = DateTime.UtcNow.Ticks,
                TimeCreated = aveWeb.Created.ToUniversalTime().Ticks,
                CreateDate = int.Parse(aveWeb.Created.ToString("yyyyMMdd")),
                NodeType = (int)NodeLevel.Site,
                TermId = itemRule.TermInfo.UniqueId,
                TermName = itemRule.TermInfo.Name,
                SourceFlag = (int)SourceFlag.SharePointOnPrem,
                DisposalDueDate = itemRule.DisposalAction,
                RuleId = itemRule.Rule != null ? new Guid(itemRule.Rule.Id) : Guid.Empty,
                RuleLevel = itemRule.Rule != null ? (int)itemRule.Rule.PolicyLevel : 0,
                HoldStatus = false,
                RecordStatus = (int)RMRecordStatus.Active,
                SortTicks = Snowflake.Instance().GetTicks()
            };
        }

        public RecordDto AssembleRecord(AveDiscoverList discoverObj, SyncItemRuleInfo itemRule)
        {
            var aveList = discoverObj.GetListObject();
            var recId = AvePoint.RA.SharePoint.ExplorerSync.Utils.IDGenerator.GetRecordId(siteCahce.SPSiteId, aveList.ID);
            return new RecordDto()
            {
                Id = recId,
                RecordsId = string.Empty,
                LeafName = discoverObj.Title,
                FullPath = discoverObj.RootFolderUrl,
                ScopeId = siteCahce.SPSiteId,
                DirPath = discoverObj.RootFolderUrl,
                NodeId = aveList.ID,
                AveSiteId = siteCahce.AveSiteId,
                WebId = aveList.ParentWeb.ID,
                ListId = aveList.ID,
                ItemId = Guid.Empty,
                CollectionTime = DateTime.UtcNow.Ticks,
                TimeCreated = aveList.Created.ToUniversalTime().Ticks,
                NodeType = (int)NodeLevel.List,
                TermId = itemRule.TermInfo.UniqueId,
                TermName = itemRule.TermInfo.Name,
                SourceFlag = (int)SourceFlag.SharePointOnPrem,
                DisposalDueDate = itemRule.DisposalAction,
                RuleId = itemRule.Rule != null ? new Guid(itemRule.Rule.Id) : Guid.Empty,
                RuleLevel = itemRule.Rule != null ? (int)itemRule.Rule.PolicyLevel : 0,
                HoldStatus = false,
                RecordStatus = (int)RMRecordStatus.Active,
                SortTicks = Snowflake.Instance().GetTicks()
            };
        }

        public RecordDto AssembleRecord(AveDiscoverFolder discoverObj, Guid parentId, SyncItemRuleInfo itemRule)
        {
            var aveFolder = discoverObj.AveFolder;
            if (aveFolder.Properties == null)
            {
                logger.Warn("get folder occured error, folder is :{0}", discoverObj.FullUrl.LogBase64());
                return null;
            }
            var recId = AvePoint.RA.SharePoint.ExplorerSync.Utils.IDGenerator.GetRecordId(siteCahce.SPSiteId, discoverObj.DocID);
            RecordDto rec = new RecordDto()
            {
                Id = recId,
                RecordsId = string.Empty,
                LeafName = discoverObj.LeafName,
                FullPath = discoverObj.FullUrl,
                ScopeId = siteCahce.SPSiteId,
                DirPath = discoverObj.FullUrl,
                NodeId = discoverObj.DocID,
                AveSiteId = siteCahce.AveSiteId,
                WebId = aveFolder.ParentWeb.ID,
                ListId = aveFolder.ParentListId,
                FolderId = discoverObj.ParentID != Guid.Empty ? discoverObj.ParentID : parentId,
                ItemId = Guid.Empty,
                CollectionTime = DateTime.UtcNow.Ticks,
                TimeCreated = ((DateTime)aveFolder.Properties["vti_timecreated"]).ToUniversalTime().Ticks,
                NodeType = (int)NodeLevel.Folder,
                TermId = itemRule.TermInfo.UniqueId,
                TermName = itemRule.TermInfo.Name,
                SourceFlag = (int)SourceFlag.SharePointOnPrem,
                DisposalDueDate =itemRule.DisposalAction,
                RuleId = itemRule.Rule != null ? new Guid(itemRule.Rule.Id) : Guid.Empty,
                RuleLevel = itemRule.Rule != null ? (int)itemRule.Rule.PolicyLevel : 0,
                HoldStatus = false,
                RecordStatus = (int)RMRecordStatus.Active,
                SortTicks = Snowflake.Instance().GetTicks()
            };
            if(aveFolder.Item != null)
            {
                rec.ApproveUsers = aveFolder.Item.FieldValues.ContainsKey("HPRM_RecordNumber") ? aveFolder.Item.FieldValues["HPRM_RecordNumber"]?.ToString() : string.Empty;
            }
            return rec;
        }
        public RecordDto AssembleRecord(IAveFolder aveFolder, Guid parentId, SyncItemRuleInfo itemRule)
        {
            var recId = AvePoint.RA.SharePoint.ExplorerSync.Utils.IDGenerator.GetRecordId(siteCahce.SPSiteId, aveFolder.UniqueId);
            RecordDto rec = new RecordDto()
            {
                Id = recId,
                RecordsId = string.Empty,
                LeafName = aveFolder.Name,
                FullPath = aveFolder.ServerRelativeUrl,
                ScopeId = siteCahce.SPSiteId,
                DirPath = aveFolder.ServerRelativeUrl,
                NodeId = aveFolder.UniqueId,
                AveSiteId = siteCahce.AveSiteId,
                WebId = aveFolder.ParentWeb.ID,
                ListId = aveFolder.ParentListId,
                FolderId = parentId,
                ItemId = Guid.Empty,
                CollectionTime = DateTime.UtcNow.Ticks,
                TimeCreated = ((DateTime)aveFolder.Properties["vti_timecreated"]).ToUniversalTime().Ticks,
                NodeType = (int)NodeLevel.Folder,
                TermId = itemRule.TermInfo.UniqueId,
                TermName = itemRule.TermInfo.Name,
                SourceFlag = (int)SourceFlag.SharePointOnPrem,
                DisposalDueDate = itemRule.DisposalAction,
                RuleId = itemRule.Rule != null ? new Guid(itemRule.Rule.Id) : Guid.Empty,
                RuleLevel = itemRule.Rule != null ? (int)itemRule.Rule.PolicyLevel : 0,
                HoldStatus = false,
                RecordStatus = (int)RMRecordStatus.Active,
                SortTicks = Snowflake.Instance().GetTicks()
            };
            if (aveFolder.Item != null)
            {
                rec.ApproveUsers = aveFolder.Item.FieldValues.ContainsKey("HPRM_RecordNumber") ? aveFolder.Item.FieldValues["HPRM_RecordNumber"]?.ToString() : string.Empty;
            }
            return rec;
        }


        public RecordDto AssembleRecord(IAveListItem aveItem, Guid parentId, SyncItemRuleInfo itemRule, string recordOwner = null)
        {
            using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.AssembleRecord", addToStatistics: true))
            {
                var recId = AvePoint.RA.SharePoint.ExplorerSync.Utils.IDGenerator.GetRecordId(siteCahce.SPSiteId, aveItem.UniqueId);
                var itemUrl = aveItem.FullPath();
                var itemName = aveItem?.GetObjectName();
                var extension = GetItemExtension(itemName, aveItem);
                var releatedRecordsCount = 0;
                var releatedRecordsInfo = string.Empty;
                if (aveItem.FieldValues.ContainsKey(RA.Common.Global.RcordsBuiltInColumn.RecordsRelated))
                {
                    releatedRecordsInfo = GetReleatedRecordsInfo(aveItem.FieldValues[RA.Common.Global.RcordsBuiltInColumn.RecordsRelated]?.ToString(), ref releatedRecordsCount);
                }
                string recordsId = string.Empty;
                if (aveItem.FieldValues.ContainsKey(RA.Common.Global.SPColumnConstants.DocumentId))
                {
                    recordsId = aveItem.FieldValues[RA.Common.Global.SPColumnConstants.DocumentId]?.ToString();
                }
                else if (aveItem.FieldValues.ContainsKey(RA.Common.Global.RcordsBuiltInColumn.UNIQUEID_NAME))
                {
                    recordsId = aveItem.FieldValues[RA.Common.Global.RcordsBuiltInColumn.UNIQUEID_NAME]?.ToString();
                }
                var recoEntity = new RecordDto()
                {
                    Id = recId,
                    ScopeId = siteCahce.SPSiteId,
                    NodeId = aveItem.UniqueId,
                    DirPath = aveItem.DirPath(),
                    FullPath = itemUrl,
                    RecordsId = recordsId,
                    LeafName = itemName,
                    ExtensionForFile = extension,
                    AveSiteId = siteCahce.AveSiteId,
                    WebId = aveItem.ParentList.ParentWeb.ID,
                    ListId = aveItem.ParentList.ID,
                    ItemId = aveItem.UniqueId,
                    CollectionTime = DateTime.UtcNow.Ticks,
                    TimeCreated = aveItem.FieldValues.ContainsKey(RA.Common.Global.SPColumnConstants.SP_Created) ? aveItem.GetUTCDateWithTimeZone(RA.Common.Global.SPColumnConstants.SP_Created).Ticks : 0,
                    //aveItem.FieldValues.ContainsKey(RA.Common.Global.SPColumnConstants.SP_Created) ? Convert.ToDateTime(aveItem.FieldValues[RA.Common.Global.SPColumnConstants.SP_Created]).Ticks : 0,
                    NodeType = (int)NodeLevel.Item,
                    TermId = itemRule.TermInfo.UniqueId,
                    TermName = itemRule.TermInfo.Name,
                    FolderId = parentId,
                    MetaInfo = GetMetaInfo(aveItem),
                    HoldStatus = false,
                    RelatedRecords = releatedRecordsInfo,
                    RelatedRecordsCount = releatedRecordsCount,
                    SourceFlag = (int)SourceFlag.SharePointOnPrem,
                    CreatedBy = aveItem.GetSingleUserFieldValue(RA.Common.Global.SPColumnConstants.Author),
                    ModifiedBy = aveItem.GetSingleUserFieldValue(RA.Common.Global.SPColumnConstants.Editor),
                    DisposalDueDate = itemRule.DisposalAction,
                    PreviosDisposalDueDate = itemRule.DisposalAction,
                    DeclareAsRecord = aveItem.IsBlockEditAndDeleteRecord(),
                    TimeLastModified = aveItem.FieldValues.ContainsKey(RA.Common.Global.SPColumnConstants.Modified) ? aveItem.GetUTCDateWithTimeZone(RA.Common.Global.SPColumnConstants.Modified).Ticks : 0,
                    //.FieldValues.ContainsKey(RA.Common.Global.SPColumnConstants.Modified) ? Convert.ToDateTime(aveItem.FieldValues[RA.Common.Global.SPColumnConstants.Modified]).Ticks : 0,
                    ItemRowId = aveItem.ID,
                    RuleId = itemRule.Rule != null ? new Guid(itemRule.Rule.Id) : Guid.Empty,
                    RuleLevel = itemRule.Rule != null ? (int)itemRule.Rule.PolicyLevel : 0,
                    RecordStatus = (int)RMRecordStatus.Active,
                    ApproveUsers = aveItem.FieldValues.ContainsKey("HPRM_RecordNumber") ? aveItem.FieldValues["HPRM_RecordNumber"]?.ToString() : string.Empty,   //VEC临时方案
                    SortTicks = Snowflake.Instance().GetTicks()
                };
                //if (itemRule.Rule != null && recordOwner == null)
                //{
                //    var userIds = DataSyncJobCache.GetLastReviewedUserIds(recoEntity.ScopeId, recoEntity.NodeId);
                //    recoEntity.RecordOwner = userIds;
                //}
                //else
                //{
                //    recoEntity.RecordOwner = recordOwner;
                //}
                return recoEntity;
            }

        }

        private string GetMetaInfo(IAveListItem aveItem)
        {
            RecordMetaInfo metaInfo = new RecordMetaInfo
            {
                FileSize = aveItem.FieldValues.ContainsKey(RA.Common.Global.SPColumnConstants.File_Size) ? Convert.ToInt64(aveItem.FieldValues[RA.Common.Global.SPColumnConstants.File_Size]) : 0
            };
            return JsonConvert.SerializeObject(metaInfo);
        }

        private string GetReleatedRecordsInfo(string value, ref int count)
        {
            string result = string.Empty;
            try
            {
                if (value == null)
                {
                    return result;
                }
                var rProps = GetRelatedPropertiesBySPColumnValue(value);
                if (rProps != null && rProps.Count > 0)
                {
                    count = rProps.Count;
                    result = SerializerHelper.SerializeToXmlString(rProps);
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while get releatedInfo, ERROR:{0}", ex.ToString());
            }

            return result;
        }

        private string GetItemExtension(string objectName, IAveListItem aveItem)
        {
            var result = string.Empty;
            if (aveItem.ParentList.BaseType == AveBaseType.DocumentLibrary)
            {
                var ext = Path.GetExtension(objectName);
                result = ext.IndexOf(".") >= 0 ? ext.Substring(1) : "RM_RDM_RecordDetails_DataType_FileNull";
            }
            else
            {
                result = "RM_RDM_RecordDetails_DataType_SPItem";
            }
            return result;
        }

        private List<RMRelatedItemInfo> GetRelatedPropertiesBySPColumnValue(string relatedColumnValue)
        {
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
                    infos.Add(relatedObj);
                }
            }
            return infos;
        }

    }
    public class SyncItemRuleInfo
    {
        public Contract.Global.Object.RMTermInfo TermInfo { get; set; }
        public Rule Rule { get; set; }
        public string DisposalAction { get; set; } = string.Empty;
    }
}
