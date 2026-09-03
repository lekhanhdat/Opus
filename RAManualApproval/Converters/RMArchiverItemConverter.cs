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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.Records.Core.Utilities.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
namespace RAManualApproval.Converters
{
    public class RMArchiverItemConverter
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RMArchiverItemConverter));

        private const string ARCHIVER_XML_NODE_METADATA = "MetaData";

        private const string ARCHIVER_XML_NODE_NAME = "Name";

        private const string ARCHIVER_XML_NODE_VALUE = "Value";

        private const string SP_FIELD_Title_NAME = "title";

        private const string SP_FIELD_CONTENTTYPE_NAME = "content type";

        private const string SP_FIELD_MODIFIEDBY_NAME = "editor";

        private const string SP_FIELD_CREATEDBY_NAME = "author";

        private const string SP_FIELD_CREATED_TIME = "created";

        private const string FS_XML_NODE_PROPERTY = "Property";

        private const string FS_FIELD_CREATEDBY_NAME = "CreatedBy";

        private const string FS_FIELD_MODIFIEDBY_NAME = "ModifiedBy";

     
        private static IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();
 
        public static ManualExportReportInfo ConvertToReportInfo(RMManualArchiverSharePointOnlineTableEntity entity, bool isRetention = false)
        {
            try
            {
                var aspd = JsonConvert.DeserializeObject<ArchiverSharePointDto>(entity.JsonMeta);
                var objectLevel = GetObjectLevel(entity);
                var leafName = objectLevel == RMReportObjectLevel.SiteCollection || objectLevel == RMReportObjectLevel.Site ? aspd.SiteTitle : aspd.LeafName;
                var info = new ManualExportReportInfo()
                {
                    PartKey = entity.PartitionKey,
                    LeafName = leafName,
                    SiteGroupID = aspd.SiteGroupId,
                    SiteID = aspd.SiteId,
                    RegistedSiteId = aspd.RegistedSiteId,
                    WebID = aspd.WebId,
                    ListID = aspd.ListId,
                    NodeID = entity.NodeID,
                    ParentID = entity.ParentID,
                    RowKey = entity.RowKey,
                    SiteUrl = aspd.SiteUrl,
                    ArchiveLevel = entity.ArchiveLevel,
                    Level = entity.CacheNodeType,
                    RuleID = entity.RuleID.ToString(),
                    ScanJobId = entity.ScanJobID,
                    ScopeID = entity.ScopeID.ToString(),
                    Status = (SOApproveDBStatus)entity.Status,
                    UIVersion = entity.UIVersion,
                    ObjectLevel = objectLevel,
                    JsonMeta = entity.JsonMeta,
                    DeleteRelatedRecords = entity.DeleteRelatedRecords,
                    HasRelatedDocument = entity.HasRelatedDocument,
                    RelatedRecordInfo = entity.RelatedRecordInfo,
                    RetentionStatus = isRetention ? 1 : entity.SourceFlag == (int)SourceFlag.LifecycleRetention ? 1 : 0,
                    ModifiedTime = aspd.LastModifiedTime,
                };

                if (!string.IsNullOrEmpty(aspd.Metadata))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(aspd.Metadata);
                    XmlNode root = doc.SelectSingleNode(ARCHIVER_XML_NODE_METADATA);
                    foreach (XmlNode node in root.ChildNodes)
                    {
                        string fieldName = node.Attributes[ARCHIVER_XML_NODE_NAME].Value;
                        string fieldValue = node.Attributes[ARCHIVER_XML_NODE_VALUE].Value;
                        switch (fieldName)
                        {
                            case SP_FIELD_CONTENTTYPE_NAME:
                                info.ContentType = fieldValue;
                                break;
                            case SP_FIELD_CREATEDBY_NAME:
                                info.CreatedBy = fieldValue;
                                break;
                            case SP_FIELD_MODIFIEDBY_NAME:
                                info.ModifiedBy = fieldValue;
                                break;
                            case SP_FIELD_Title_NAME:
                                if (entity.ArchiveLevel == (int)AvePoint.RA.Contract.SPNodeArchiverLevel.Item)
                                {
                                    info.LeafName = fieldValue;
                                }
                                break;
                            case SP_FIELD_CREATED_TIME:
                                try
                                {
                                    info.CreatedTime = Convert.ToDateTime(fieldValue).Ticks;
                                }
                                catch (Exception e)
                                {
                                    Logger.Error($"An error occurred while convert sharepoint item create time. Error: {e}");
                                }

                                break;
                        }
                    }
                }
                info.Path = GetFullPath(aspd, info.ObjectLevel);
                info.ServerRelativeUrl = aspd.Path;
                info.FolderPath = GetManualFolderPath(info, GetFullPath(aspd, info.ObjectLevel));
                info.SiteUrl = aspd.SiteUrl;
                if (aspd.ArchivedTime > DateTime.MinValue)
                {
                    info.ArchivedTime = aspd.ArchivedTime.Ticks;
                }

                return info;
            }
            catch(Exception e) 
            {
                Logger.Error($"An error occurred while convert entity [{entity.PartitionKey}] [{entity.RowKey}]. Error: {e}");
                return null;            
            }
        }
        
        private static string GetManualFolderPath(ManualExportReportInfo item, string manualFullPath)
        {
            string folderPath = string.Empty;
            try 
            {
                if (item.ObjectLevel == RMReportObjectLevel.Item || item.ObjectLevel == RMReportObjectLevel.Document || item.ObjectLevel == RMReportObjectLevel.Folder)
                {
                    folderPath = manualFullPath.Replace("\\", "/").Replace(item.SiteUrl, "").Replace(item.LeafName, "");
                    folderPath = folderPath.Substring(0, folderPath.Length - 1);
                }
              
            }
             catch (Exception ex) 
            {
                Logger.Error($"RMArchiverItemConverter-GetManualFolderPath Error Exception:{ex}");
                throw;
            }
            return folderPath;
        }

        public static ManualExportReportInfo ConvertToReportInfo(RMManualArchiverSharePointOnPremiseTableEntity entity)
        {
            var aspd = JsonConvert.DeserializeObject<OnPremiseArchiverSharePointDto>(entity.JsonMeta);
            var objectLevel = RMReportObjectLevel.Item;
            var leafName = aspd.LeafName;
            var info = new ManualExportReportInfo()
            {
                PartKey = entity.PartitionKey,
                LeafName = leafName,
                SiteGroupID = aspd.SiteGroupId,
                SiteID = new Guid(aspd.SiteId),
                RegistedSiteId = new Guid(aspd.RegistedSiteId),
                WebID = aspd.WebId,
                ListID = aspd.ListId,
                NodeID = entity.NodeID,
                ParentID = entity.ParentID,
                RowKey = entity.RowKey,
                SiteUrl = aspd.SiteUrl,
                ArchiveLevel = entity.ArchiveLevel,
                Level = entity.CacheNodeType,
                RuleID = entity.RuleID.ToString(),
                ScanJobId = entity.ScanJobID,
                ScopeID = entity.ScopeID.ToString(),
                Status = (SOApproveDBStatus)entity.Status,
                UIVersion = entity.UIVersion,
                ObjectLevel = objectLevel,
                JsonMeta = entity.JsonMeta,
                ModifiedTime = aspd.LastModifiedTime,
            };
            if (!string.IsNullOrEmpty(aspd.Metadata))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(aspd.Metadata);
                XmlNode root = doc.SelectSingleNode(ARCHIVER_XML_NODE_METADATA);
                foreach (XmlNode node in root.ChildNodes)
                {
                    string fieldName = node.Attributes[ARCHIVER_XML_NODE_NAME].Value;
                    string fieldValue = node.Attributes[ARCHIVER_XML_NODE_VALUE].Value;
                    switch (fieldName)
                    {
                        case SP_FIELD_CONTENTTYPE_NAME:
                            info.ContentType = fieldValue;
                            break;
                        case SP_FIELD_CREATEDBY_NAME:
                            info.CreatedBy = fieldValue;
                            break;
                        case SP_FIELD_MODIFIEDBY_NAME:
                            info.ModifiedBy = fieldValue;
                            break;
                        case SP_FIELD_CREATED_TIME:
                            try
                            {
                                info.CreatedTime = Convert.ToDateTime(fieldValue).Ticks;
                            }
                            catch (Exception e)
                            {
                                Logger.Error($"An error occurred while convert sharepoint item create time. Error: {e}");
                            }

                            break;
                    }
                }
            }
            info.Path = GetFullPathForSPOnPrem(aspd);
            info.ServerRelativeUrl = aspd.Path;
            info.RelatedRecordInfo = entity.RelatedRecordInfo;
            info.HasRelatedDocument = entity.HasRelatedDocument;
            info.DeleteRelatedRecords = entity.DeleteRelatedRecords;
            return info;
        }
        public static ManualExportReportInfo ConvertToReportInfo(FileSystemTableEntity entity)
        {
            var info = new ManualExportReportInfo()
            {
                PartKey = entity.PartitionKey,
                RowKey = entity.RowKey,
                LeafName = entity.LowName,
                NodeID = new Guid(entity.RowKey),//文件路径md5
                ParentID = entity.ParentID, //parent folder md5
                Level = entity.NodeLevel,
                RuleID = entity.RuleId.ToString(),
                ScopeID = entity.CurrentSettingId != Guid.Empty ? entity.CurrentSettingId.ToString() : entity.ScopeID.ToString(),
                Status = (SOApproveDBStatus)entity.Status,
                ObjectLevel = RMReportObjectLevel.FSFile,
                ArchivedTime = entity.AchiveTime.Ticks,
                ExportToRECO = entity.MovedToApprovalTable,
                DeleteRelatedRecords = entity.DisposalAction ? 1 : 0,
                RelatedRecordInfo = entity.RelatedRecordInfo,
                //Path = GetFSItemFullPath(entity.HighName, entity.LowName)
                Path = entity.FullPath,
                CreatedTime = entity.CreateTime.Ticks,
                ModifiedTime = entity.LastModifiedTme.Ticks,
                InternalStatus = (SOApproveDBStatus)entity.InternalStatus,
                ManualApprovalBy = entity.ApprovalBy,
                ManualEscalateFrom = entity.ManualEscalateFrom,
                DestroyedTime = entity.DestroyedTime,
                RecordStatus = (RMRecordStatus)entity.RecordStatus,
                SiteID = entity.ConnectionId,
            };

            if (!string.IsNullOrEmpty(entity.Property))
            {
                //处理扩展属性逻辑
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(entity.Property);
                XmlNode root = doc.SelectSingleNode(FS_XML_NODE_PROPERTY);
                foreach (XmlNode node in root.ChildNodes)
                {
                    string fieldName = node.Attributes[ARCHIVER_XML_NODE_NAME].Value;
                    string fieldValue = node.Attributes[ARCHIVER_XML_NODE_VALUE]?.Value;
                    switch (fieldName)
                    {
                        case FS_FIELD_CREATEDBY_NAME:
                            info.CreatedBy = fieldValue;
                            break;
                        case FS_FIELD_MODIFIEDBY_NAME:
                            info.ModifiedBy = fieldValue;
                            break;
                    }
                }
            }
            return info;
        }
        public static ManualExportReportInfo ConvertToReportInfo(RMManualArchiverFileSystemTableEntity entity)
        {
            var info = new ManualExportReportInfo()
            {
                PartKey = entity.PartitionKey,
                RowKey = entity.RowKey,
                LeafName = entity.LowName,
                NodeID = new Guid(entity.RowKey),//文件路径md5
                ParentID = entity.ParentID, //parent folder md5
                Level = entity.NodeLevel,
                RuleID = entity.RuleId.ToString(),
                ScopeID = entity.CurrentSettingId != Guid.Empty ? entity.CurrentSettingId.ToString() : entity.ScopeID.ToString(),
                Status = (SOApproveDBStatus)entity.Status,
                ObjectLevel = RMReportObjectLevel.FSFile,
                ArchivedTime = entity.AchiveTime.Ticks,
                ExportToRECO = entity.MovedToApprovalTable,
                DeleteRelatedRecords = entity.DisposalAction ? 1 : 0,
                RelatedRecordInfo = entity.RelatedRecordInfo,
                //Path = GetFSItemFullPath(entity.HighName, entity.LowName)
                Path = entity.FullPath,
                CreatedTime = entity.CreateTime.Ticks,
                ModifiedTime = entity.LastModifiedTme.Ticks,
            };

            if (!string.IsNullOrEmpty(entity.Property))
            {
                //处理扩展属性逻辑
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(entity.Property);
                XmlNode root = doc.SelectSingleNode(FS_XML_NODE_PROPERTY);
                foreach (XmlNode node in root.ChildNodes)
                {
                    string fieldName = node.Attributes[ARCHIVER_XML_NODE_NAME].Value;
                    string fieldValue = node.Attributes[ARCHIVER_XML_NODE_VALUE]?.Value;
                    switch (fieldName)
                    {
                        case FS_FIELD_CREATEDBY_NAME:
                            info.CreatedBy = fieldValue;
                            break;
                        case FS_FIELD_MODIFIEDBY_NAME:
                            info.ModifiedBy = fieldValue;
                            break;
                    }
                }
            }
            return info;
        }

        public static ManualExportReportInfo ConvertToReportInfo(RMManualArchiverExchangeTableEntity entity)
        {
            var aspd = JsonConvert.DeserializeObject<ArchiverExchangeOnlineDto>(entity.JsonMeta);
            var info = new ManualExportReportInfo()
            {
                PartKey = entity.PartitionKey,
                LeafName = aspd.Title,
                SiteGroupID = string.IsNullOrEmpty(entity.MailBoxGroupID) ? Guid.Empty : new Guid(entity.MailBoxGroupID),
                SiteID = Guid.Empty,
                RegistedSiteId = Guid.Empty,
                WebID = Guid.Empty,
                ListID = Guid.Empty,
                UnMD5NodeId = entity.NodeID,
                NodeID = entity.NodeID.ToMd5(),
                ParentID = entity.ParentID.ToMd5(),
                RowKey = entity.RowKey,
                SiteUrl = GetMailBoxUrl(entity.FullPath),
                ArchiveLevel = entity.ArchiveLevel,
                ArchivedTime = entity.ArchivedTime,
                Level = entity.CacheNodeType,
                RuleID = entity.RuleID.ToString(),
                ScanJobId = entity.ScanJobID,
                Status = (SOApproveDBStatus)entity.Status,
                ObjectLevel = RMReportObjectLevel.ExchangeOnlineItem,
                JsonMeta = entity.JsonMeta,
                DeleteRelatedRecords = entity.DeleteRelatedRecords,
                HasRelatedDocument = entity.HasRelatedDocument,
                RelatedRecordInfo = entity.RelatedRecordInfo,
                //MailBoxID = string.IsNullOrEmpty(entity.MailBoxID) ? Guid.Empty : new Guid(entity.MailBoxID),
                CreatedBy = aspd.SendFrom,
                ModifiedBy = aspd.ModifiedBy,
                CreatedTime = string.IsNullOrWhiteSpace(aspd.Created) ? 0 : long.Parse(aspd.Created),
                ModifiedTime = entity.LastModifiedTime,
            };
            var mailboxId = entity.MailBoxID;
            if (mailboxId.IndexOf("(Archive)") != -1)
            {
                mailboxId = mailboxId.Substring(0, mailboxId.IndexOf("(Archive)"));
            }
            info.MailBoxID = string.IsNullOrEmpty(mailboxId) ? Guid.Empty : new Guid(mailboxId);
            info.Path = entity.FullPath;
            info.ServerRelativeUrl = string.Empty;
            return info;
        }

        public static ManualExportReportInfo ConvertToReportInfo(Record entity)
        {
            var info = new ManualExportReportInfo()
            {
                LeafName = entity.LeafName,
                LocationID = entity.LocationId,
                RuleID = entity.RuleId.ToString(),
                ScopeID = entity.ScopeId.ToString(),
                Status = (SOApproveDBStatus)entity.DisposalStatus,
                ObjectLevel = GetObjectLevelForPhysical((RMNodeType)entity.NodeType),
                NodeID = entity.Id,
                ArchivedTime = entity.DestroyedTime,
                CreatedBy = entity.CreatedBy,
                ModifiedBy = entity.ModifiedBy,
                Path = $"{ExplorerService.GetPhysicalObjectFullPath(entity.Id)}/{entity.LeafName}",
                ExportToRECO = entity.ExportToRECO,
                RecordStatus = (RMRecordStatus)entity.RecordStatus,
                HasRelatedDocument = entity.RelatedRecordsCount,
                DeleteRelatedRecords = entity.DeleteRelatedRecords,
                RelatedRecordInfo = entity.RelatedRecords,
                ModifiedTime = entity.ManualModifiedTime,
            };
            return info;
        }

        private static RMReportObjectLevel GetObjectLevel(RMManualArchiverSharePointOnlineTableEntity entity)
        {
            RMReportObjectLevel level = RMReportObjectLevel.Item;
            if (entity.CacheNodeType == (int)CacheNodeType.SiteCollection)
            {
                level = RMReportObjectLevel.SiteCollection;
            }
            else if ((entity.CacheNodeType >= (int)CacheNodeType.Web) && (entity.CacheNodeType < (int)CacheNodeType.List))
            {
                level = RMReportObjectLevel.Site;
            }
            else if (entity.CacheNodeType == (int)CacheNodeType.List)
            {
                level = RMReportObjectLevel.List;
            }
            else if ((entity.CacheNodeType > (int)CacheNodeType.List) && (entity.CacheNodeType < (int)CacheNodeType.Item))
            {
                level = RMReportObjectLevel.Folder;
            }
            else if (entity.CacheNodeType == (int)CacheNodeType.Item || entity.CacheNodeType == (int)CacheNodeType.ItemVersion)
            {
                level = RMReportObjectLevel.Item;
            }
            else if (entity.CacheNodeType == (int)CacheNodeType.Attachment)
            {
                level = RMReportObjectLevel.Attachment;
            }

            return level;
        }

        private static string GetFullPath(ArchiverSharePointDto info, RMReportObjectLevel objectLevel)
        {
            string fullPathUrl = string.Empty;
            try
            {
                if (info.Path.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                    || info.Path.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)
                    || objectLevel == RMReportObjectLevel.SiteCollection)
                {
                    fullPathUrl = info.Path;
                }
                else
                {
                    var level = objectLevel;

                    if (level == RMReportObjectLevel.Site || level == RMReportObjectLevel.Folder || level == RMReportObjectLevel.List || level == RMReportObjectLevel.Item)
                    {
                        fullPathUrl = WebUtil.MakeFullUrl(info.SiteUrl, info.Path);
                    }

                    else if (level == RMReportObjectLevel.Attachment)
                    {
                        try
                        {
                            string baseUrl = info.SiteUrl.Length > 8 ? info.SiteUrl.IndexOf('/', 8) > 0 ? info.SiteUrl.Substring(0, info.SiteUrl.IndexOf('/', 8)) : info.SiteUrl : string.Empty;
                            int indexRealName = info.LeafName.IndexOf(':');
                            int id = 0;
                            string realName = string.Empty;
                            string listServerRelatedUrl = string.Empty;
                            id = Convert.ToInt32(info.LeafName.Substring(0, info.LeafName.IndexOfAny(new char[] { '_', '.' })));
                            realName = info.LeafName.Substring(indexRealName + 1);
                            string list = "Lists/";
                            int listUrlLength = info.Path.IndexOf(list, StringComparison.OrdinalIgnoreCase) + list.Length;
                            string listUrl = info.Path.Substring(0, listUrlLength);
                            string subUrl = info.Path.Substring(listUrlLength);
                            //sites/gaoxinqu/Lists/Tasks/aaa/bbb/ccc\5_.000  -> subfold's item attachment
                            int index = subUrl.IndexOf('/');
                            if (index == -1)
                            {
                                listServerRelatedUrl = (listUrl + subUrl.Substring(0, subUrl.IndexOf('\\'))).TrimStart('/');
                            }
                            else
                            {
                                //sites/gaoxinqu/Lists/Tasks\1_.000  -> rootfold's attachment
                                listServerRelatedUrl = (listUrl + subUrl.Substring(0, index)).TrimStart('/');
                            }
                            fullPathUrl = baseUrl + @"/" + listServerRelatedUrl + @"/Attachments/" + id + @"/" + realName;
                        }
                        catch (Exception ex)
                        {
                            Logger.Warn("Error in Get Attachment Full Url" + ex.ToString());
                            fullPathUrl = info.Path;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(fullPathUrl))
                {
                    fullPathUrl = fullPathUrl.Replace("\\", "/");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("get full path error, node id:{0}, url:{1} error:{2}", info?.NodeID, info?.SiteUrl, ex.ToString());
            }

            return fullPathUrl;
        }

        private static string GetFullPathForSPOnPrem(OnPremiseArchiverSharePointDto info)
        {
            string fullPathUrl = "";
            if (info.Path.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) || info.Path.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
            {
                fullPathUrl = info.Path;
            }
            else
            {
                fullPathUrl = WebUtil.MakeFullUrl(info.SiteUrl, info.Path);
            }
            return fullPathUrl;
        }

        private static string GetMailBoxUrl(string mailItemUrl)
        {
            if (!string.IsNullOrEmpty(mailItemUrl))
            {
                return mailItemUrl.Split('\\').ToList()[0];
            }
            return "";
        }

        private static RMReportObjectLevel GetObjectLevelForPhysical(RMNodeType rmNodeType)
        {
            RMReportObjectLevel rmReportLevel;
            switch (rmNodeType)
            {
                case RMNodeType.PhyBox:
                    rmReportLevel = RMReportObjectLevel.PhysicalBox;
                    break;
                case RMNodeType.PhyFile:
                    rmReportLevel = RMReportObjectLevel.PhysicalFile;
                    break;
                default:
                    rmReportLevel = (RMReportObjectLevel)0;
                    break;
            }
            return rmReportLevel;
        }
    }
}
