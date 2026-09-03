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
using AvePoint.Common.FilterEngine.ObjectInfos;
using AvePoint.GCommon.Contract.Tree;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.Records.Core.Utilities.Extensions;
using Newtonsoft.Json;
using RAGoogle.Archive.Wrapper;
using RAGoogle.Models;
using RAGoogle.RecordsDisposal.Action.ExportOnly;
using RAGoogle.Util;
using System.Collections;

namespace RAGoogle.Extension
{
    public static class DataItemExtension
    {
        public static Record ConvertToRecord(this GoogleItemData item, GoogleDriveTreeNodeDto selectedNode, Record? existRecord = null, RMRecordStatus? status = RMRecordStatus.Active)
        {
            try
            {
                if (existRecord == null)
                {
                    existRecord = new Record
                    {
                        RecordStatus = (int)(status ?? RMRecordStatus.Active),
                        CreateDate = Convert.ToInt32(item.CreatedTime.ToString("yyyyMMdd")),
                        TimeCreated = item.CreatedTime.Ticks,
                        RecordsId = null,
                    };
                }
                if (item.ParentId.Equals("root") || item.ParentId.Equals(selectedNode.ObjectId))
                {
                    // if the first level, parent id is node id
                    existRecord.ParentId = new Guid(selectedNode.ID);
                }
                else
                {
                    existRecord.ParentId = $"{item.DriveId}/{item.ParentId}".ToMd5();
                }
                existRecord.Id = item.UniqueId;
                existRecord.ScopeId = new Guid(selectedNode.ID);
                existRecord.AveSiteId = item.DriveId;
                existRecord.ContainerId = selectedNode.ContainerId;
                existRecord.SourceFlag = (int)SourceFlag.Google;
                existRecord.NodeType = (int)item.Level;
                existRecord.NodeId = item.UniqueId;
                existRecord.CreatedBy = item.CreatedBy;
                existRecord.ModifiedBy = item.ModifiedBy;
                existRecord.TimeModified = item.ModifiedTime.Ticks;
                existRecord.DirPath = item.RelativePath;
                existRecord.FullPath = item.Path;
                existRecord.LeafName = item.Name;
                existRecord.ExtensionForFile = item.FileExtension;
                existRecord.CollectTime = DateTime.UtcNow.Ticks;
                existRecord.MetaInfo = JsonConvert.SerializeObject(item.MetaInfo);
                existRecord.WebViewLink = item.WebViewLink;

                if (existRecord.RecordStatus == (int)RMRecordStatus.ManualPreSync)
                {
                    existRecord.RecordStatus = (int)RMRecordStatus.Active;
                }
                existRecord.RecordStatus = (int)(status ?? RMRecordStatus.Active);
                return existRecord;
            }
            catch (Exception ex)
            {
                throw new Exception($"Convert GoogleItemData to Record failed. {ex.Message}");
            }
        }

        public static GoogleItemInfo ConvertToInfo(this GoogleItemData item)
        {
            return new()
            {
                Title = item.Name,
                Name = item.Name,
                Created = item.CreatedTime,
                Modified = item.ModifiedTime,
                Size = item.Size ?? 0,
                Path = item.Path,
                Id = item.Id,
                CreateByEmail = item.CreatedBy,
                ModifiedByEmail = item.ModifiedByEmail,
                LabelInfos = AssignLabelInfos(item.MetaInfo.Labels),
                ModifiedByUserDisplayName = item.ModifiedBy
            };
        }

        public static Hashtable AssignLabelInfos(List<LabelMetaInfo> labelInfos)
        {
            Hashtable infos = new Hashtable();
            foreach (LabelMetaInfo labelInfo in labelInfos)
            {
                Dictionary<string, List<string>> dics = new();
                foreach (var field in labelInfo.FieldInfos)
                {
                    if (field.Values == null)
                    {
                        continue;
                    }
                    string fieldType = field.ValueType switch
                    {
                        FieldValueType.text or
                        FieldValueType.user or
                        FieldValueType.selection => "text",
                        FieldValueType.integer => "number",
                        FieldValueType.dateString => "datetime",
                        _ => throw new Exception("Invalid field type")
                    };
                    if (dics.ContainsKey($"{fieldType}/{field.Title}"))
                    {
                        dics[$"{fieldType}/{field.Title}"].AddRange(field.Values);
                    }
                    else
                    {
                        dics.Add($"{fieldType}/{field.Title}", field.Values);
                    }
                }
                infos.Add(labelInfo.Title, dics);
            }
            return infos;
        }

        #region Generate job detail
        public static JMArchiverActionJobDetails GenerateDisposalActionJobDetail(this GoogleItemData item, string action, string ruleName, string comment, ActionTab actionTab = ActionTab.Action)
        {
            var detail = new JMArchiverActionJobDetails();
            detail.SourceLocation = item.RelativePath;
            detail.Size = item.Size.ToString();
            detail.FileSize = item.Size ?? 0;
            detail.RuleName = ruleName;
            detail.Level = item.Level == RMNodeLevel.GoogleFolder ? I18NResource.ObjectLevelFolder : I18NResource.ObjectLevelFile;
            detail.ActionTab = (int)actionTab;
            detail.Action = action;
            detail.FinishTime = DateTime.UtcNow.Ticks;
            detail.Comment = comment;
            detail.DestinationLocation = item.DestinationPath;

            return detail;
        }
        
        public static JMArchiverActionJobDetails GenerateExportDisposalActionJobDetail(this GoogleItemData item, string action, string ruleName, string comment)
        {
            var detail = new JMArchiverActionJobDetails();
            detail.SourceLocation = item.RelativePath;
            detail.Size = item.Size.ToString();
            detail.FileSize = item.Size ?? 0;
            detail.RuleName = ruleName;
            detail.Level = ConvertItemLevelToString(item.Level);
            detail.ActionTab = (int)ActionTab.Export;
            detail.Action = action;
            detail.FinishTime = DateTime.UtcNow.Ticks;
            detail.Comment = comment;

            return detail;
        }


        private static string ConvertItemLevelToString(RMNodeLevel node)
        {
            switch (node)
            {
                case RMNodeLevel.GoogleFolder:
                    return I18NResource.ObjectLevelFolder;
                case RMNodeLevel.ItemVersion:
                    return I18NResource.ObjectLevelGoogleDriveFileVersion;
                default:
                    return I18NResource.ObjectLevelFile;
            }
            
        }
        public static JMArchiverActionJobDetails GenerateMoveToActionJobDetail(this GoogleItemData item, string action, string ruleName, string comment)
        {
            var detail = new JMArchiverActionJobDetails();
            detail.SourceLocation = item.RelativePath;
            detail.DestinationLocation = item.DestinationPath;
            detail.Size = item.Size.ToString();
            detail.FileSize = item.Size ?? 0;
            detail.RuleName = ruleName;
            detail.Level = item.Level == RMNodeLevel.GoogleFolder ? I18NResource.ObjectLevelFolder : I18NResource.ObjectLevelFile;
            detail.ActionTab = (int)ActionTab.Action;
            detail.Action = action;
            detail.FinishTime = DateTime.UtcNow.Ticks;
            detail.Comment = comment;

            return detail;
        }

        public static JMGoogleJobDetails GenerateApplySettingJobDetail(this GoogleItemData item, string labelName, string action, JobDetailsStatus status, string message)
        {
            JMGoogleJobDetails detail = new JMGoogleJobDetails();
            detail.ObjectName = item.Name;
            detail.FullPath = item.RelativePath;
            detail.FileSize = item.Size.ToString();
            detail.ItemType = I18NResource.ObjectLevelFile;
            detail.Action = action;
            detail.Status = status;
            detail.Comment = message;
            detail.Classification = labelName;
            return detail;
        }

        public static JMCreateAndDestroyedFileReportJobDetail GenerateCreateAndDestroyedReportJobDetail(this GoogleItemData item, string comments = "")
        {
            JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
            detail.ObjectLevel = item.Level == RMNodeLevel.GoogleFolder ? I18NResource.ObjectLevelFolder : I18NResource.ObjectLevelFile;
            detail.Title = item.Name;
            detail.URL = item.RelativePath;
            detail.Comment = comments;
            return detail;
        }

        public static JMReportJobDetails GenerateReportJobDetail(this GoogleItemData item, string comment = "")
        {
            return new JMReportJobDetails()
            {
                Type = item.Level == RMNodeLevel.GoogleFile ? I18NResource.ObjectLevelFile : I18NResource.ObjectLevelFolder,
                TitleOrName = item.Name,
                Url = item.RelativePath,
                Comment = comment
            };
        }
        #endregion
        public static GoogleItemMetaInfo ConvertToMetaInfo(this GoogleItemData item, List<LabelMetaInfo> labels)
        {
            return new GoogleItemMetaInfo
            {
                DriveName = item.DriveName,
                DriveId = item.DriveId,
                DocId = item.Id,
                Labels = labels,
                FileSize = item.Size ?? 0,
                TenantId = item.TenantId,
                ModifiedByEmail = item.ModifiedByEmail,
            };
        }

        public static SyncFailureItemEntity GenerateFailureItemEntity(this GoogleItemData item, string scopeId, string jobId)
        {
            var entity = new SyncFailureItemEntity(scopeId, item.UniqueId.ToString())
            {
                DataSource = (int)SourceFlag.Google,
                FullPath = item.RelativePath,
                ParentId = item.ParentId.ToString(),
                NodeId = item.UniqueId.ToString(),
                DocId = item.Id,
                IsDirectory = item.Level == RMNodeLevel.GoogleFolder,
                JobId = jobId,
            };

            return entity;
        }
        public static DownloadedFileInfo ToDownloadedFileInfo(this GoogleItemData item)
        {
            return new DownloadedFileInfo
            {
                Id = item.Id,
                FormattedFileVersionName = item.Name,
                ModifiedTime = item.ModifiedTime,
                DriveName = item.DriveName,
                ParentId = item.ParentId,
                ParentIds = item.ParentIds,
                FileExtension = item.FileExtension,
                MimeType = item.MimeType,
                Path = item.Path,
                CreatedBy = item.CreatedBy,
                CreatedTime = item.CreatedTime,
                RelativePath = item.RelativePath,
                Labels = item.LableIds ?? new List<string>(),
                Size = item.Size,
                VersionId = string.Empty,
                VersionName = string.Empty,
                FolderName = item.Name,
                MemberEmail = item.MemberEmail,
                ModifiedBy = item.ModifiedBy,
                Description = item.Description,
                Permissions = item.Permissions,
            };
        }
    }
}
