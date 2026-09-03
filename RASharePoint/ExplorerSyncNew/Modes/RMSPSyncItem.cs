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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.SharePoint.Common;
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
using AvePoint.RA.Contract.RMRelatedRecord;
using System.Xml;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Explorer;
using Newtonsoft.Json;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Contract.SharePoint.CustomIndexMetadata;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using Cloud.Sdk.Telemetry.Data.Alita;

namespace AvePoint.RA.SharePoint.ExplorerSync.Modes
{
    public class RMSPSyncItem
    {
        protected static readonly RALogger logger = RALogger.GetInstance(typeof(RMSPSyncItem));
        private IRMManualApproveDao mManualApproveDao;

        protected IRMManualApproveDao ManualApproveDao
        {
            get
            {
                if (mManualApproveDao == null)
                {
                    mManualApproveDao = (IRMManualApproveDao)PlatformWindsorManager.GetService(typeof(IRMManualApproveDao));
                }
                return mManualApproveDao;
            }
        }
        protected IAveTimeZone mSPWebTimeZone { get; set; }

        public RMSPSyncItem(RMSPExplorerSiteLevelCache cache)
        {
            siteCahce = cache;
        }

        public RMSPSyncItem(RMSPExplorerSiteLevelCache cache, List<RMCustomIndexMetadata> customIndexMetadatas, List<RMCustomMetadataColumn> customMetadataColumns)
        {
            siteCahce = cache;
            CustomIndexMetadatas = customIndexMetadatas;
            CustomMetadataColumns = customMetadataColumns;
        }

        public void InitTimeZone(IAveTimeZone timeZone)
        {
            mSPWebTimeZone = timeZone;
        }
        private RMSPExplorerSiteLevelCache siteCahce;

        protected List<RMCustomIndexMetadata> CustomIndexMetadatas;

        protected List<RMCustomMetadataColumn> CustomMetadataColumns;

        public virtual Record AssembleRecord(AveDiscoverSite discoverObj, SyncItemRuleInfo itemRule)
        {
            var aveSite = discoverObj.Site;
            var siteId = discoverObj.SiteID.ToString();
            var rootWeb = discoverObj.GetRootWeb().AveWeb;
            var recId = IDGenerator.GetRecordId(discoverObj.SiteID, discoverObj.SiteID);
            return new Record()
            {
                Id = recId,
                RecordsId = string.Empty,
                LeafName = rootWeb.Title,
                FullPath = rootWeb.Url,
                ScopeId = siteCahce.SPSiteId,
                DirPath = rootWeb.Url,
                NodeId = discoverObj.SiteID,
                AveSiteId = siteCahce.AveSiteId,
                CollectTime = DateTime.UtcNow.Ticks,
                TimeCreated = rootWeb.Created.Ticks,
                CreateDate = int.Parse(rootWeb.Created.ToString("yyyyMMdd")),
                NodeType = (int)NodeLevel.SiteCollection,
                TermId = itemRule?.TermInfo?.UniqueId ?? Guid.Empty,
                TermName = itemRule?.TermInfo?.Name,
                SourceFlag = (int)SourceFlag.SharePoint,
                DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(itemRule.DisposalAction),
                RuleId = itemRule.Rule != null ? new Guid(itemRule.Rule.Id) : Guid.Empty,
                RuleLevel = itemRule.Rule != null ? (int)itemRule.Rule.PolicyLevel : 0,
                HoldStatus = false,
                RecordStatus = (int)RMRecordStatus.Active,
            };
        }

        public virtual Record AssembleRecord(AveDiscoverWeb discoverObj, SyncItemRuleInfo itemRule)
        {
            var aveWeb = discoverObj.AveWeb;
            var recId = IDGenerator.GetRecordId(siteCahce.SPSiteId, aveWeb.ID);
            return new Record()
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
                CollectTime = DateTime.UtcNow.Ticks,
                TimeCreated = aveWeb.Created.ToUniversalTime().Ticks,
                CreateDate = int.Parse(aveWeb.Created.ToString("yyyyMMdd")),
                NodeType = (int)NodeLevel.Site,
                TermId = itemRule?.TermInfo?.UniqueId ?? Guid.Empty,
                TermName = itemRule?.TermInfo?.Name,
                SourceFlag = (int)SourceFlag.SharePoint,
                DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(itemRule.DisposalAction),
                RuleId = itemRule.Rule != null ? new Guid(itemRule.Rule.Id) : Guid.Empty,
                RuleLevel = itemRule.Rule != null ? (int)itemRule.Rule.PolicyLevel : 0,
                HoldStatus = false,
                RecordStatus = (int)RMRecordStatus.Active,
            };
        }

        public virtual Record AssembleRecord(AveDiscoverList discoverObj, SyncItemRuleInfo itemRule)
        {
            var aveList = discoverObj.GetListObject();
            var recId = IDGenerator.GetRecordId(siteCahce.SPSiteId, aveList.ID);
            return new Record()
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
                CollectTime = DateTime.UtcNow.Ticks,
                TimeCreated = aveList.Created.ToUniversalTime().Ticks,
                NodeType = (int)NodeLevel.List,
                TermId = itemRule?.TermInfo?.UniqueId ?? Guid.Empty,
                TermName = itemRule?.TermInfo?.Name,
                SourceFlag = (int)SourceFlag.SharePoint,
                DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(itemRule.DisposalAction),
                RuleId = itemRule.Rule != null ? new Guid(itemRule.Rule.Id) : Guid.Empty,
                RuleLevel = itemRule.Rule != null ? (int)itemRule.Rule.PolicyLevel : 0,
                HoldStatus = false,
                RecordStatus = (int)RMRecordStatus.Active,
            };
        }

        public virtual Record AssembleRecord(AveDiscoverFolder discoverObj, Guid parentId, SyncItemRuleInfo itemRule)
        {
            var aveFolder = discoverObj.AveFolder;
            if (aveFolder.Properties == null)
            {
                logger.Warn("get folder occured error, folder is :{0}", discoverObj.FullUrl);
                return null;
            }
            var recId = IDGenerator.GetRecordId(siteCahce.SPSiteId, discoverObj.DocID);
            Record rec= new Record()
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
                CollectTime = DateTime.UtcNow.Ticks,
                TimeCreated = ((DateTime)aveFolder.Properties["vti_timecreated"]).ToUniversalTime().Ticks,
                NodeType = (int)NodeLevel.Folder,
                TermId = itemRule.TermInfo.UniqueId,
                TermName = itemRule.TermInfo.Name,
                SourceFlag = (int)SourceFlag.SharePoint,
                DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(itemRule.DisposalAction),
                RuleId = itemRule.Rule != null ? new Guid(itemRule.Rule.Id) : Guid.Empty,
                RuleLevel = itemRule.Rule != null ? (int)itemRule.Rule.PolicyLevel : 0,
                HoldStatus = false,
                RecordStatus = (int)RMRecordStatus.Active,
            };
            if(aveFolder.Item != null)
            {
                rec.ApproveUsers = aveFolder.Item.FieldValues.ContainsKey("HPRM_RecordNumber") ? aveFolder.Item.FieldValues["HPRM_RecordNumber"]?.ToString() : string.Empty;
            }
            return rec;
        }
        public virtual Record AssembleRecord(IAveFolder aveFolder, Guid parentId, SyncItemRuleInfo itemRule)
        {
            string recordsId = string.Empty;
            if (aveFolder.Item != null && aveFolder.Item.FieldValues.ContainsKey(SPColumnConstants.DocumentId))
            {
                recordsId = aveFolder.Item.FieldValues[SPColumnConstants.DocumentId]?.ToString();
            }
            else if (aveFolder.Item != null && aveFolder.Item.FieldValues.ContainsKey(RcordsBuiltInColumn.UNIQUEID_NAME))
            {
                recordsId = aveFolder.Item.FieldValues[RcordsBuiltInColumn.UNIQUEID_NAME]?.ToString();
            }
            var recId = IDGenerator.GetRecordId(siteCahce.SPSiteId, aveFolder.UniqueId);
            Record rec = new Record()
            {
                Id = recId,
                RecordsId = recordsId,
                LeafName = aveFolder.Name,
                FullPath = aveFolder.Item?.FullPath(),
                ScopeId = siteCahce.SPSiteId,
                DirPath = aveFolder.ServerRelativeUrl,
                NodeId = aveFolder.UniqueId,
                AveSiteId = siteCahce.AveSiteId,
                WebId = aveFolder.ParentWeb.ID,
                ListId = aveFolder.ParentListId,
                FolderId = parentId,
                ItemId = Guid.Empty,
                CollectTime = DateTime.UtcNow.Ticks,
                TimeCreated = ((DateTime)aveFolder.Properties["vti_timecreated"]).ToUniversalTime().Ticks,
                CreatedBy = aveFolder.Item?.GetSingleUserFieldValue(SPColumnConstants.Author),
                ModifiedBy = aveFolder.Item?.GetSingleUserFieldValue(SPColumnConstants.Editor),
                TimeModified = (aveFolder.Item != null && aveFolder.Item.FieldValues.ContainsKey(SPColumnConstants.Modified)) ? aveFolder.Item.GetUTCDateWithTimeZone(SPColumnConstants.Modified).Ticks : 0,
                NodeType = (int)NodeLevel.Folder,
                TermId = itemRule.TermInfo.UniqueId,
                TermName = itemRule.TermInfo.Name,
                SourceFlag = (int)SourceFlag.SharePoint,
                DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(itemRule.DisposalAction),
                RuleId = itemRule.Rule != null ? new Guid(itemRule.Rule.Id) : Guid.Empty,
                RuleLevel = itemRule.Rule != null ? (int)itemRule.Rule.PolicyLevel : 0,
                HoldStatus = false,
                RecordStatus = (int)RMRecordStatus.Active,
                ItemRowId = aveFolder.Item != null ? aveFolder.Item.ID : 0
            };
            if (aveFolder.Item != null)
            {
                rec.ApproveUsers = aveFolder.Item.FieldValues.ContainsKey("HPRM_RecordNumber") ? aveFolder.Item.FieldValues["HPRM_RecordNumber"]?.ToString() : string.Empty;
            }

            return rec;
        }


        public virtual Record AssembleRecord(IAveListItem aveItem, Guid parentId, SyncItemRuleInfo itemRule, string recordOwner = null)
        {
            using (var performance1 = new PerformanceScope("RMSPExplorerProcessor.AssembleRecord", addToStatistics: true))
            {
                var recId = IDGenerator.GetRecordId(siteCahce.SPSiteId, aveItem.UniqueId);
                var itemUrl = aveItem.FullPath();
                var itemName = aveItem?.GetObjectName();
                var extension = GetItemExtension(itemName, aveItem);
                var releatedRecordsCount = 0;
                var releatedRecordsInfo = string.Empty;
                if (aveItem.FieldValues.ContainsKey(RcordsBuiltInColumn.RecordsRelated))
                {
                    releatedRecordsInfo = GetReleatedRecordsInfo(aveItem.FieldValues[RcordsBuiltInColumn.RecordsRelated]?.ToString(), ref releatedRecordsCount);
                }
                string recordsId = string.Empty;
                if (aveItem.FieldValues.ContainsKey(SPColumnConstants.DocumentId))
                {
                    recordsId = aveItem.FieldValues[SPColumnConstants.DocumentId]?.ToString();
                }
                else if (aveItem.FieldValues.ContainsKey(RcordsBuiltInColumn.UNIQUEID_NAME))
                {
                    recordsId = aveItem.FieldValues[RcordsBuiltInColumn.UNIQUEID_NAME]?.ToString();
                }
                var recoEntity = new Record()
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
                    CollectTime = DateTime.UtcNow.Ticks,
                    TimeCreated = aveItem.FieldValues.ContainsKey(SPColumnConstants.SP_Created) ? aveItem.GetUTCDateWithTimeZone(SPColumnConstants.SP_Created).Ticks : 0,
                    //aveItem.FieldValues.ContainsKey(SPColumnConstants.SP_Created) ? Convert.ToDateTime(aveItem.FieldValues[SPColumnConstants.SP_Created]).Ticks : 0,
                    NodeType = (int)NodeLevel.Item,
                    TermId = itemRule.TermInfo.UniqueId,
                    TermName = itemRule.TermInfo.Name,
                    IsInheritedTerm = itemRule.TermInfo.IsInheritedTerm,
                    //FolderId = aveItem[""].ToString(),
                    MetaInfo = GetMetaInfo(aveItem),
                    HoldStatus = false,
                    RelatedRecords = releatedRecordsInfo,
                    RelatedRecordsCount = releatedRecordsCount,
                    SourceFlag = (int)SourceFlag.SharePoint,
                    CreatedBy = aveItem.GetSingleUserFieldValue(SPColumnConstants.Author),
                    ModifiedBy = aveItem.GetSingleUserFieldValue(SPColumnConstants.Editor),
                    DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(itemRule.DisposalAction),
                    PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(itemRule.DisposalAction),
                    DeclareAsRecord = aveItem.IsBlockEditAndDeleteRecord(),
                    LockedByRecordLabel = IsLockedByRecordLabel(aveItem),
                    TimeModified = aveItem.FieldValues.ContainsKey(SPColumnConstants.Modified) ? aveItem.GetUTCDateWithTimeZone(SPColumnConstants.Modified).Ticks : 0,
                    //aveItem.FieldValues.ContainsKey(SPColumnConstants.Modified) ? Convert.ToDateTime(aveItem.FieldValues[SPColumnConstants.Modified]).Ticks : 0,
                    ItemRowId = aveItem.ID,
                    RuleId = itemRule.Rule != null ? new Guid(itemRule.Rule.Id) : Guid.Empty,
                    RuleLevel = itemRule.Rule != null ? (int)itemRule.Rule.PolicyLevel : 0,
                    RecordStatus = (int)RMRecordStatus.Active,
                    ApproveUsers = aveItem.FieldValues.ContainsKey("HPRM_RecordNumber") ? aveItem.FieldValues["HPRM_RecordNumber"]?.ToString() : string.Empty,   //VEC临时方案
                };

                try
                {
                    var parentUniqueId = aveItem["ParentUniqueId"]?.ToString();
                    recoEntity.FolderId = new Guid(parentUniqueId);
                }
                catch(Exception ex)
                {
                    logger.Error($"An error occured while get parent unique id of item [{aveItem.ID}]. Error: {ex}");
                    recoEntity.FolderId = parentId;
                }

                recoEntity.CustomColumnDic = GetCustomMetadata(aveItem, recoEntity);

                return recoEntity;
            }
        }

        private bool IsLockedByRecordLabel(IAveListItem aveItem)
        {
            if (siteCahce.SiteRetentionLabelCache == null)
            {
                var site = aveItem.Web.Site;
                var availableTags = site.GetAvailableTagsForSite();
                try
                {
                    siteCahce.SiteRetentionLabelCache = availableTags.ToDictionary(_ => _.TagName, StringComparer.OrdinalIgnoreCase);
                }
                catch (Exception e)
                {
                    logger.Info($"init label exception retry {e}");
                    siteCahce.SiteRetentionLabelCache = availableTags.ToDictionary(_ => _.TagName);
                }
            }
            var retentionLabelOfItem = aveItem.GetComplianceInfo();
            if (retentionLabelOfItem != null && retentionLabelOfItem.TagPolicyHold && retentionLabelOfItem.TagPolicyRecord && (siteCahce.SiteRetentionLabelCache?.TryGetValue(retentionLabelOfItem.ComplianceTag, out var tagInfo) ?? false))
            {
                if (tagInfo.BlockDelete && tagInfo.BlockEdit)
                {
                    return true;
                }
            }
            return false;
        }

        protected string GetMetaInfo(IAveListItem aveItem)
        {
            RecordMetaInfo metaInfo = new RecordMetaInfo
            {
                FileSize = aveItem.FieldValues.ContainsKey(SPColumnConstants.File_Size) ? Convert.ToInt64(aveItem.FieldValues[SPColumnConstants.File_Size]) : 0
            };
            return JsonConvert.SerializeObject(metaInfo);
        }

        protected Dictionary<string, CustomColumn> GetCustomMetadata(IAveListItem aveItem, Record record)
        {
            var dic = new Dictionary<string, CustomColumn>();
            foreach (var customIndexMetadata in CustomIndexMetadatas)
            {
                try
                {
                    var columnInfo = CustomMetadataColumns.Where(c => c.UniqueId == customIndexMetadata.TargetColumnId).FirstOrDefault();
                    var sourceColumnName = customIndexMetadata.SourceColumnName;
                    if (sourceColumnName.StartsWith("[", StringComparison.OrdinalIgnoreCase) && sourceColumnName.EndsWith("]", StringComparison.OrdinalIgnoreCase))
                    {
                        sourceColumnName = customIndexMetadata.SourceColumnName.Trim(['[', ']']);
                        if (aveItem.FieldValues.TryGetValue(sourceColumnName, out object internalValue))
                        {
                            var metadataValue = GetValueByType(columnInfo, aveItem, sourceColumnName, internalValue);
                            dic[columnInfo.UniqueId.ToString()] = metadataValue;
                        }
                        else
                        {
                            logger.Warn($"Can not get value by column [{sourceColumnName}]");
                            record.CustomColumnNotExist = true;
                        }
                        continue;
                    }

                    if (aveItem.FieldValues.TryGetValue(sourceColumnName, out object value))
                    {
                        var metadataValue = GetValueByType(columnInfo, aveItem, sourceColumnName, value);
                        dic[columnInfo.UniqueId.ToString()] = metadataValue;
                        continue;
                    }

                    try
                    {
                        var aveItemColumnValue = aveItem[sourceColumnName];
                        var metadataValue = GetValueByType(columnInfo, aveItem, sourceColumnName, aveItemColumnValue);
                        dic[columnInfo.UniqueId.ToString()] = metadataValue;
                    }
                    catch (Exception ex)
                    {
                        logger.Warn($"Can not get value by column [{sourceColumnName}]");
                        record.CustomColumnNotExist = true;
                    }
                }
                catch(Exception e)
                {
                    logger.Error($"Get custom column [{customIndexMetadata.SourceColumnName}] failed, error: {e}");
                    record.CustomColumnNotExist = true;
                }
            }

            return dic;
        }

        private CustomColumn GetValueByType(RMCustomMetadataColumn column, IAveListItem aveItem, string sourceColumnName, object value)
        {
            var customColumn = new CustomColumn();
            if (value == null)
            {
                return customColumn;
            }

            // Special handling for _ComplianceTagWrittenTime
            if (string.Equals(sourceColumnName, "_ComplianceTagWrittenTime", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var dateTime = DateTime.Parse(value.ToString()).ToUniversalTime();
                    var timeColumn = new DateTimeColumnValue() { Date = dateTime, TimeZoneId = "UTC" };
                    customColumn.Value = JsonConvert.SerializeObject(timeColumn);
                    customColumn.Date = dateTime;
                    customColumn.TimeZoneId = "UTC";
                    return customColumn;
                }
                catch (Exception ex)
                {
                    logger.Error($"Error parsing _ComplianceTagWrittenTime: {ex}");
                    return customColumn;
                }
            }

            // Special handling for _dlc_DocId
            if (string.Equals(sourceColumnName, "_dlc_DocId", StringComparison.OrdinalIgnoreCase))
            {
                customColumn.Value = aveItem.FieldValues[sourceColumnName]?.ToString() ?? string.Empty;
                customColumn.Value_Array = customColumn.Value.ExplorerAnalyzeBuiltInColumn();
                return customColumn;
            }

            switch (column.ColumnType)
            {
                case CustomColumnType.SingleText:
                    return GetMetadataCustomColumn(value);
                case CustomColumnType.Number:
                    if (!double.TryParse(value.ToString(), out var numberValue))
                    {
                        throw new Exception("Can not get the Number value");
                    }

                    customColumn.Value = numberValue.ToString();
                    customColumn.Value_Array = value?.ToString().ExplorerAnalyzeBuiltInColumn() ?? [];
                    customColumn.Number = GetNumber(value.ToString());
                    return customColumn;
                case CustomColumnType.YesOrNo:
                    if (!bool.TryParse(value?.ToString(), out var result))
                    {
                        throw new Exception("Can not get the YesOrNo value");
                    }

                    customColumn.Value = value?.ToString() ?? string.Empty;
                    customColumn.YesOrNo = result ? "Yes" : "No";
                    return customColumn;
                case CustomColumnType.DateTime:
                    var dateTime = aveItem.GetCustomUTCDateWithTimeZone(sourceColumnName);
                    if (dateTime == DateTime.MinValue)
                    {
                        throw new Exception("Can not get the DateTime value");
                    }
                    var timeColumn = new DateTimeColumnValue() { Date = dateTime, TimeZoneId = "UTC" };
                    customColumn.Value = JsonConvert.SerializeObject(timeColumn);
                    customColumn.Date = dateTime;
                    customColumn.TimeZoneId = "UTC";
                    return customColumn;
                default:
                    return customColumn;
            }
        }

        private static double GetNumber(string content)
        {
            double result = default(double);
            if (content != null && content.Length < 255)
            {
                if (double.TryParse(content, out result))
                {
                    return result;
                }
            }
            return result;
        }

        private static CustomColumn GetMetadataCustomColumn(object value)
        {
            var valueString = value?.ToString() ?? string.Empty;
            var customColumn = new CustomColumn();
            if (string.IsNullOrEmpty(valueString))
            {
                customColumn.Value = string.Empty;
                customColumn.Value_Array = [];
                return customColumn;
            }

            if(valueString.IndexOf('|') > -1)
            {
                var metadataInfo = valueString.Split('|');
                if (metadataInfo.Length == 2)
                {
                    valueString = metadataInfo[0];
                }
            }
            else if(valueString.StartsWith("i:0#.w|", StringComparison.OrdinalIgnoreCase))
            {
                valueString = valueString.Substring("i:0#.w|".Length);
            }
            else if (valueString.IndexOf(";#") > -1)
            {
                var userValues = valueString.Split(new string[] { ";#" }, StringSplitOptions.None);
                valueString = userValues[1];
            }

            customColumn.Value = valueString;
            customColumn.Value_Array = valueString.ExplorerAnalyzeBuiltInColumn();
            return customColumn;
        }

        protected string GetReleatedRecordsInfo(string value, ref int count)
        {
            string result = string.Empty;
            try
            {
                if (value == null)
                {
                    return result;
                }
                var util = new RelatedRecordsUtility();
                var rProps = util.GetRelatedPropertiesBySPColumnValue(value);
                //var rProps = new List<RMRelatedItemInfo>();
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

        protected string GetItemExtension(string objectName, IAveListItem aveItem)
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

    }
    public class SyncItemRuleInfo
    {
        public RMTermInfo TermInfo { get; set; }
        public Rule Rule { get; set; }
        public string DisposalAction { get; set; } = string.Empty;
    }
}
