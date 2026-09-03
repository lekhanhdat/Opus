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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Explorer.Model
{
    public static class RecordExtension
    {
        private static AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RecordExtension));
        /// <summary>
        /// check if the there are fields' value changed compared to that in Cosmos DB
        /// </summary>
        /// <param name="rec"></param>
        /// <param name="dbRec">record in DB</param>
        /// <returns></returns>        
        public static bool HaveFieldsValueChanged(this Record rec, Record dbRec)
        {
            // compare uniqueid, add for document ID feature
            return (rec.TimeModified > dbRec.TimeModified || rec.TermId != dbRec.TermId || rec.DeclareAsRecord != dbRec.DeclareAsRecord || rec.LockedByRecordLabel != dbRec.LockedByRecordLabel
                        || dbRec.DisposalDueDate == DueDateUtil.Pending
                        || rec.RuleId != dbRec.RuleId || rec.ModifiedBy != dbRec.ModifiedBy || rec.CreatedBy != dbRec.CreatedBy
                        || rec.RecordsId != dbRec.RecordsId || rec.RecordOwner != dbRec.RecordOwner || rec.SourceFlag != dbRec.SourceFlag || rec.DirPath != dbRec.DirPath || rec.RecordStatus != dbRec.RecordStatus
                        || rec.PredictTermId != dbRec.PredictTermId || rec.MLClassificationType != dbRec.MLClassificationType 
                        || rec.MLUnderReview != dbRec.MLUnderReview || rec.MLApprovalStatus != dbRec.MLApprovalStatus 
                        || rec.TrainingModelId != dbRec.TrainingModelId
                        );  //add comparing record owner
        }

        public static bool HaveFieldsValueChangedMLManual(this Record rec, Record dbRec)
        {
            return (rec.DirPath != dbRec.DirPath || rec.SourceFlag != dbRec.SourceFlag //|| rec.RecordsId != dbRec.RecordsId
                    || rec.ModifiedBy != dbRec.ModifiedBy || rec.CreatedBy != dbRec.CreatedBy
                    ||  rec.PredictTermId != dbRec.PredictTermId || rec.MLClassificationType != dbRec.MLClassificationType
                    || rec.MLUnderReview != dbRec.MLUnderReview || rec.MLApprovalStatus != dbRec.MLApprovalStatus
                    || rec.TrainingModelId != dbRec.TrainingModelId
                    );  
        }
        public static void CopyFrom(this Record dest, Record source)
        {
            dest.TimeModified = source.TimeModified;
            dest.TermId = source.TermId;
            dest.TermName = source.TermName;
            dest.DisposalDueDate = source.DisposalDueDate;
            dest.PreviosDisposalDueDate = source.PreviosDisposalDueDate;
            dest.LeafName = source.LeafName;
            dest.RuleId = source.RuleId;
            dest.FolderId = source.FolderId;
            dest.RecordOwner = source.RecordOwner;
            dest.DirPath = source.DirPath;
            dest.MetaInfo = source.MetaInfo;
            dest.CustomColumnDic = source.CustomColumnDic;
            dest.RuleLevel = source.RuleLevel;
            dest.RelatedRecords = source.RelatedRecords;
            dest.RelatedRecordsCount = source.RelatedRecordsCount;
            dest.CollectTime = source.CollectTime;
            dest.RecordsId = source.RecordsId;
            dest.DeclareAsRecord = source.DeclareAsRecord;
            dest.LockedByRecordLabel = source.LockedByRecordLabel;
            dest.ModifiedBy = source.ModifiedBy;
            dest.CreatedBy = source.CreatedBy;
            dest.ExtensionForFile = source.ExtensionForFile;
            dest.ExternalId = source.ExternalId;
            dest.EmailAddress = source.EmailAddress;
            dest.SendTo = source.SendTo;
            dest.ContainerId = source.ContainerId;
            dest.LeafName_Array = source.LeafName_Array;
            dest.ModifiedBy_Lower = source.ModifiedBy_Lower;
            dest.CreatedBy_Lower = source.CreatedBy_Lower;
            dest.DeclaredBy_Lower = source.DeclaredBy_Lower;
            dest.ModifiedBy_Array = source.ModifiedBy_Array;
            dest.CreatedBy_Array = source.CreatedBy_Array;
            dest.DeclaredBy_Array = source.DeclaredBy_Array;
            dest.RecordOwner_Array = source.RecordOwner_Array;
            dest.SourceFlag = source.SourceFlag;
            dest.RecordStatus = source.RecordStatus;
            dest.TrainingScope = source.TrainingScope;
            dest.TrainingTermId = source.TrainingTermId;
            dest.TrainingAddType = source.TrainingAddType;
            dest.MLApprovalStatus = source.MLApprovalStatus;
            dest.WebViewLink = source.WebViewLink;
        }
        /// <summary>
        /// 转换MetaInfo到CustomColumnDic， 以存储新的数据结构
        /// </summary>
        /// <param name="rec"></param>
        public static void AppendCustomColumns(this Record rec)
        {
            if (rec != null)
            {
                if (rec.CreateDate == 0 && rec.TimeCreated > 0)
                {
                    DateTime date = new DateTime(rec.TimeCreated, DateTimeKind.Utc);
                    rec.CreateDate = int.Parse(date.ToString("yyyyMMdd"));
                }
                rec.RecordsId_Array = rec.RecordsId.ExplorerAnalyzeUniqueId();
                rec.LeafName_Array = rec.LeafName.ExplorerAnalyzeBuiltInColumn();
                rec.DeclaredBy_Lower = rec.DeclaredBy?.ToLower();
                rec.CreatedBy_Lower = rec.CreatedBy?.ToLower();
                rec.ModifiedBy_Lower = rec.ModifiedBy?.ToLower();
                rec.ModifiedBy_Array = rec.ModifiedBy.ExplorerAnalyzeBuiltInColumn();
                rec.CreatedBy_Array = rec.CreatedBy.ExplorerAnalyzeBuiltInColumn();
                rec.DeclaredBy_Array = rec.DeclaredBy.ExplorerAnalyzeBuiltInColumn();
                rec.RecordOwner_Array = rec.RecordOwner.ExplorerSearchSplit();
            }
            if (rec != null && rec.SourceFlag == 4 && rec.MetaInfo != null && (rec.CustomColumnDic == null || rec.CustomColumnDic.Count == 0))
            {
                //Convert metainfo string to List<CustomColumn>
                try
                {
                    Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(rec.MetaInfo);
                    if (dic.Count > 0)
                    {
                        Dictionary<string, CustomColumn> result = new Dictionary<string, CustomColumn>();
                        foreach (string key in dic.Keys)
                        {
                            CustomColumn customColumn = new CustomColumn();
                            string content = dic[key];
                            if (string.IsNullOrEmpty(content))
                            {
                                continue;
                            }
                            if (content.Contains("\":\""))
                            {
                                if (content.StartsWith("["))
                                {
                                    if (content.Contains("\"UserPrincipalName\":\"") && content.Contains("\"InviteType\":"))
                                    {
                                        try
                                        {
                                            List<AOSUserDto> users = JsonConvert.DeserializeObject<List<AOSUserDto>>(content);
                                            users.ForEach(a => a.UserPrincipalName = a.UserPrincipalName?.ToLower());
                                            customColumn.Users = users;
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.Warn($@"fail des users,ex:{ex}");
                                        }
                                    }
                                    else if (content.Contains("\"Name\":") && content.Contains("\"Value\":"))
                                    {
                                        try
                                        {
                                            List<ChoiceColumnValue> choices = JsonConvert.DeserializeObject<List<ChoiceColumnValue>>(content);
                                            customColumn.MultiChoice = choices;
                                        }
                                        catch (Exception ex) 
                                        {
                                            logger.Warn($@"fail des choices,ex:{ex}");
                                        }
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        customColumn = JsonConvert.DeserializeObject<CustomColumn>(content);
                                    }
                                    catch
                                    {
                                        customColumn.Value = content;
                                        customColumn.Number = GetNumber(content);
                                        customColumn.Value_Array = content.ExplorerAnalyzeBuiltInColumn();
                                    }
                                }
                            }
                            else
                            {
                                customColumn.Value = content;
                                customColumn.Number = GetNumber(content);
                                customColumn.Value_Array = content.ExplorerAnalyzeBuiltInColumn();
                            }

                            //only convert time to utc in upgrade tool
                            //if (customColumn.TimeZoneId != null && customColumn.Date != null && customColumn.Date != DateTime.MinValue)
                            //{
                            //    customColumn.Date = Common.Util.DateTimeUtil.ConvertTimeToUtcDate(customColumn.Date, customColumn.TimeZoneId, customColumn.IsSetDayLight);
                            //}
                            if (key == DefaultColumnIDs.Classification && customColumn.Name != null && rec.TermName == null)
                            {
                                rec.TermName = customColumn.Name;
                            }
                            if (customColumn.Date != default && customColumn.TimeZoneId != null)
                            {
                                try
                                {
                                    customColumn.Date = Common.Util.DateTimeUtil.ConvertTimeToUtcDate(customColumn.Date, customColumn.TimeZoneId, customColumn.IsSetDayLight);
                                }
                                catch(Exception e)
                                {
                                    logger.Error($"error occured when AppendCustomColumns,error:{e}");
                                }
                            }
                            result.Add(key, customColumn);
                        }
                        rec.MetaInfo = null;
                        rec.CustomColumnDic = result;
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(@$"fail append custom colmns,ex:{ex}");
                }
            }
        }

        public static Record AppendCustomerColumns4FsJPMCRecord(this Record rec)
        {
            if (rec == null || rec.SourceFlag != (int)SourceFlag.FileSystem) return rec;
            if (rec.CreateDate == 0 && rec.TimeCreated > 0)
            {
                var dt = new DateTime(rec.TimeCreated, DateTimeKind.Utc);
                rec.CreateDate = dt.Year * 10000 + dt.Month * 100 + dt.Day;
            }
            rec.IsFsControlRecordJPMC = true;
            rec.RecordsId_Array = rec.RecordsId.ExplorerAnalyzeUniqueId();
            rec.LeafName_Array = rec.LeafName.ExplorerAnalyzeBuiltInColumn();
            rec.DeclaredBy_Lower = rec.DeclaredBy?.ToLowerInvariant();
            rec.CreatedBy_Lower = rec.CreatedBy?.ToLowerInvariant();
            rec.ModifiedBy_Lower = rec.ModifiedBy?.ToLowerInvariant();
            rec.ModifiedBy_Array = rec.ModifiedBy.ExplorerAnalyzeBuiltInColumn();
            rec.CreatedBy_Array = rec.CreatedBy.ExplorerAnalyzeBuiltInColumn();
            rec.DeclaredBy_Array = rec.DeclaredBy.ExplorerAnalyzeBuiltInColumn();
            rec.RecordOwner_Array = rec.RecordOwner.ExplorerSearchSplit();
            return rec;
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

        /// <summary>
        /// 转换CustomColumnDic到MetaInfo， 使旧逻辑可以兼容新的数据结构
        /// </summary>
        /// <param name="rec"></param>
        public static void AppendMetaInfoForOldLogic(this Record rec)
        {
            if (rec != null && rec.SourceFlag == 4 && rec.CustomColumnDic != null && rec.CustomColumnDic.Count > 0)
            {
                Dictionary<string, string> metaInfo = new Dictionary<string, string>();
                foreach (string key in rec.CustomColumnDic.Keys)
                {
                    CustomColumn column = rec.CustomColumnDic[key];
                    if (column.Date != default && column.TimeZoneId != null)
                    {
                        try
                        {
                            column.Date = Common.Util.DateTimeUtil.ConvertTimeFromUtc(column.Date.Ticks, column.TimeZoneId, column.IsSetDayLight);
                        }
                        catch(Exception e)
                        {
                            logger.Error($"error occured when AppendMetaInfoForOldLogic,error:{e}");
                        }
                    }
                    if (column.MultiChoice != null && column.MultiChoice.Count > 0)
                    {
                        string value = JsonConvert.SerializeObject(column.MultiChoice);
                        metaInfo.Add(key, value);
                    }
                    else if (column.Users != null && column.Users.Count > 0)
                    {
                        if (column.Users[0] != null)
                        {
                            string value = JsonConvert.SerializeObject(column.Users);
                            metaInfo.Add(key, value);
                        }
                    }
                    else if (column.Name == null && column.Id == null && column.Date == default && column.TimeZoneId == null)
                    {
                        metaInfo.Add(key, column.Value);
                    }
                    else
                    {
                        column.Number = default;
                        string value = JsonConvert.SerializeObject(column);
                        metaInfo.Add(key, value);
                    }
                }
                rec.CustomColumnDic = null;
                rec.MetaInfo = JsonConvert.SerializeObject(metaInfo);
            }
        }

        /// <summary>
        /// upgrade personal hold by data for physical data for old data. 
        /// if there exists data with old loaned by id, then will update it with new id, otherwise, will create a new one with loanedBy.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="loanedBy"></param>
        public static void UpgradePersonalHoldData(this Record record, AOSUserDto loanedBy)
        {
            var newUsers = new List<AOSUserDto> { loanedBy };
            string oldKey = DefaultColumnIDs.LoanedBy_Old;
            var newLoanedBy = new CustomColumn();
            if (record.CustomColumnDic.ContainsKey(oldKey))
            {
                var oldLoanedBy = record.CustomColumnDic[oldKey];
                newLoanedBy.Users = oldLoanedBy.Users ?? newUsers;
                record.CustomColumnDic.Remove(oldKey);
            }
            else
            {
                newLoanedBy.Users = newUsers;
            }

            //newLoanedBy.Users.ForEach(user => user.DisplayName = user.DisplayName.ToLower()); //convert to lower case
            record.CustomColumnDic[DefaultColumnIDs.LoanedBy] = newLoanedBy;
        }

        /// <summary>
        /// update personal hold by data for physical
        /// </summary>
        /// <param name="record"></param>
        /// <param name="loanedBy"></param>
        public static void UpdatePersonalHoldData(this Record record, AOSUserDto loanedBy)
        {
            if (record.CustomColumnDic != null)
            {
                if (!record.CustomColumnDic.ContainsKey(DefaultColumnIDs.LoanedBy))
                {
                    record.LoanPickStatus = (int)PickStatusType.Pendding;
                }
                else
                {
                    if (string.IsNullOrEmpty(record.CustomColumnDic[DefaultColumnIDs.LoanedBy]?.Users.FirstOrDefault()?.UserPrincipalName) && string.IsNullOrEmpty(loanedBy.UserPrincipalName))
                    {
                        //AOS Unregistered Users
                        if (record.CustomColumnDic[DefaultColumnIDs.LoanedBy]?.Users.FirstOrDefault()?.DisplayName != loanedBy.DisplayName)
                        {
                            record.LoanPickStatus = (int)PickStatusType.Pendding;
                        }
                    }
                    else if (record.CustomColumnDic[DefaultColumnIDs.LoanedBy]?.Users.FirstOrDefault()?.UserPrincipalName != loanedBy.UserPrincipalName)
                    {
                        record.LoanPickStatus = (int)PickStatusType.Pendding;
                    }
                }
                record.CustomColumnDic[DefaultColumnIDs.LoanedBy] = new CustomColumn { Users = new List<AOSUserDto> { loanedBy } };
            }
        }

        /// <summary>
        /// remove personal hold data
        /// </summary>
        /// <param name="record"></param>
        public static void RemovePersonalHoldData(this Record record)
        {
            if (record.CustomColumnDic != null && record.CustomColumnDic.ContainsKey(DefaultColumnIDs.LoanedBy))
            {
                record.CustomColumnDic.Remove(DefaultColumnIDs.LoanedBy);
            }
            record.LoanPickStatus = (int)PickStatusType.Pendding;
        }

        /// <summary>
        /// get personal hold data
        /// </summary>
        /// <param name="record"></param>
        public static CustomColumn GetPersonalHoldData(this Record record)
        {
            if (record.CustomColumnDic != null && record.CustomColumnDic.ContainsKey(DefaultColumnIDs.LoanedBy))
            {
                return record.CustomColumnDic[DefaultColumnIDs.LoanedBy];
            }
            else return null;
        }

        public static string GetPhysicalLocationFullPathByAncestors(this Record record, string locationPath, IExplorerDao ExplorerDao)
        {
            if (record.Ancestors == null || record.Ancestors.Count == 1) return locationPath;
            Guid[] ancestors = new Guid[record.Ancestors.Count - 1]; 
            record.Ancestors.CopyTo(1, ancestors, 0, record.Ancestors.Count - 1);//first one is location id,  do not need it
            var path = new StringBuilder(locationPath);
            var queryResult = ExplorerDao.QueryAll(o => Enumerable.Contains(ancestors, o.Id) && o.RecordStatus != (int)RMRecordStatus.RMDeleted);
            if (queryResult == null)
            {
                return locationPath;
            }
            var dic = queryResult.Select(o => new { o.Id, o.LeafName }).ToDictionary(o => o.Id);
            foreach(var r in ancestors)
            {
                if (dic.TryGetValue(r, out var ancestorRecord) && !string.IsNullOrEmpty(ancestorRecord.LeafName))
                {
                    path.Append($"/{ancestorRecord.LeafName}");
                }
            }

            return path.ToString();
        }

        public static string GetScopeIdPath(this Record record)
        {
            if (record.Ancestors != null && record.Ancestors.Count > 1)
            {
                List<Guid> parentIds = new List<Guid>();
                parentIds.AddRange(record.Ancestors);
                parentIds.RemoveAt(0);
                var parentIdPath = string.Join("/", parentIds);
                return parentIdPath + "/" + record.Id.ToString();
            }
            return record.Id.ToString();
        }

        /// <summary>
        /// used for Physical record, include self id
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static List<Guid> GetPhysicalAncestorsIndludeSelf(this Record record)
        {
            var result = new List<Guid>() {};
            if (record.Ancestors != null && record.Ancestors.Count > 0)
            {
                result.AddRange(record.Ancestors);
                result.Add(record.Id);
                return result;
            }

            result.Add(record.LocationId);
            if (record.BoxId != Guid.Empty)
            {
                result.Add(record.BoxId);
            }

            if (record.FileId != Guid.Empty)
            {
                result.Add(record.FileId);
            }
            result.Add(record.Id);

            return result;
        }

        public static bool IsUnderContainer(this Record record)
        {
            if (record.NodeType == (int)RA.Contract.RMWeb.Tree.Base.RMNodeType.PhyBox && BoxUnderContainer(record))
            {
                return true;
            }
            else if (record.NodeType == (int)RA.Contract.RMWeb.Tree.Base.RMNodeType.PhyFile && FolderUnderContainer(record))
            {
                return true;
            }
            return false;
        }

        private static bool BoxUnderContainer(DB.Explorer.Model.Record box)
        {
            if (box.Ancestors != null && box.Ancestors.Count > 0 && box.ParentId != box.LocationId)
            {
                return true;
            }
            return false;
        }

        private static bool FolderUnderContainer(DB.Explorer.Model.Record folder)
        {
            if (folder.Ancestors != null && folder.Ancestors.Count > 1)
            {
                if (folder.ParentId == folder.LocationId || folder.Ancestors[1] == folder.BoxId)
                {
                    //folder under location or location/box
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public static void KeepOldManualColumn(this Record record, Record dbRecord)
        {
            if (dbRecord != null)
            {
                record.IsManualSynced = dbRecord.IsManualSynced;
                record.ManualActionTime = dbRecord.ManualActionTime;
                record.ManualApprovedBy = dbRecord.ManualApprovedBy;
                record.ManualEscalatedComment = dbRecord.ManualEscalatedComment;
                record.ManualApprovedStatus = dbRecord.ManualApprovedStatus;
                record.ManualInternalApprovedStatus = dbRecord.ManualInternalApprovedStatus;
                record.ManualArchiveStatus = dbRecord.ManualArchiveStatus;
                record.ManualFullPath = dbRecord.ManualFullPath;
                record.ManualFolderPath = dbRecord.ManualFolderPath;
                record.ManualSiteUrl = dbRecord.ManualSiteUrl;
                record.ManualEscalateFrom = dbRecord.ManualEscalateFrom;
                record.ManualExtendTime = dbRecord.ManualExtendTime;
                record.ManualExtendComment = dbRecord.ManualExtendComment;
                record.ManualCollectionTime = dbRecord.ManualCollectionTime;
                record.ManualAudits = dbRecord.ManualAudits;
                record.ManualArchivedTime = dbRecord.ManualArchivedTime;
                record.ManualModifiedTime = dbRecord.ManualModifiedTime;
                record.ManualPartitionKey = dbRecord.ManualPartitionKey;
                record.ManualRowKey = dbRecord.ManualRowKey;
                record.ManualRuleName = dbRecord.ManualRuleName;
                record.ManualRuleCriteria = dbRecord.ManualRuleCriteria;
                record.ManualRuleDisposalClass = dbRecord.ManualRuleDisposalClass;
                record.ManualVersion = dbRecord.ManualVersion;
                record.ManualReviewer = dbRecord.ManualReviewer;
                record.ManualRelatedRecordsAction = dbRecord.ManualRelatedRecordsAction;
                record.ManualRelatedRecords = dbRecord.ManualRelatedRecords;
                record.ManualIsRelatedRecords = dbRecord.ManualIsRelatedRecords;
                record.ManualWorkflowInstanceId = dbRecord.ManualWorkflowInstanceId;
                record.ManualWorkflowDefinitionId = dbRecord.ManualWorkflowDefinitionId;
                record.ManualWorkflowStepId = dbRecord.ManualWorkflowStepId;
                record.hasDuplicate = dbRecord.hasDuplicate;
                record.ManualExtendCount = dbRecord.ManualExtendCount;
                record.ManualEmailNotificationCount = dbRecord.ManualEmailNotificationCount;
                record.ManualEmailNotificationLastTime = dbRecord.ManualEmailNotificationLastTime;
                record.ManualNeedEmailNotification = dbRecord.ManualNeedEmailNotification;
                record.ManualIsAutoReassigned = dbRecord.ManualIsAutoReassigned;
                if (!string.IsNullOrWhiteSpace(dbRecord.ExtensionForFile))
                {
                    record.ExtensionForFile = dbRecord.ExtensionForFile;
                }

                record.HoldByUsers = dbRecord.HoldByUsers;
                record.HoldBy = dbRecord.HoldBy;
                record.HoldId = dbRecord.HoldId;
                record.HoldReleaseTime = dbRecord.HoldReleaseTime;
                record.HoldStatus = dbRecord.HoldStatus;
                record.HoldType = dbRecord.HoldType;
                record.HoldUntilTimes = dbRecord.HoldUntilTimes;
                record.AppendHolds_Array = dbRecord.AppendHolds_Array;
                record.LoanPickStatus = dbRecord.LoanPickStatus;
                record.DestructionPickStatus = dbRecord.DestructionPickStatus;
                record.TrainingScope = dbRecord.TrainingScope;
                record.TrainingTermId = dbRecord.TrainingTermId;
                record.TrainingAddType = dbRecord.TrainingAddType;
                record.ManualLastApproveRejectComment = dbRecord.ManualLastApproveRejectComment;
                record.ManualLastReviewedBy = dbRecord.ManualLastReviewedBy;
                record.ManualLastlReviewTime = dbRecord.ManualLastlReviewTime;
                record.ManualDisposalDueDate = dbRecord.ManualDisposalDueDate;
            }
        }

        public static void KeepTermInfo(this Record record, Record dbRecord)
        {
            if (dbRecord != null)
            {
                record.TermId = dbRecord.TermId;
                record.TermName = dbRecord.TermName;
            }
        }

        public static void KeepMachineLearningPredictInfo(this Record record, Record dbRecord)
        {
            if (dbRecord != null && new int[] { (int)SourceFlag.SharePoint, (int)SourceFlag.Teams }.Contains(dbRecord.SourceFlag))
            {
                record.PredictTermId = dbRecord.PredictTermId;
                record.PredictTermScore = dbRecord.PredictTermScore;
                record.PredictTime = dbRecord.PredictTime;
                record.MLUnderReview = dbRecord.MLUnderReview;
                record.MLClassificationType = dbRecord.MLClassificationType;
                record.MLReviewer = dbRecord.MLReviewer;
                record.MLApprovalStatus = dbRecord.MLApprovalStatus;
                record.MLEscalateFrom = dbRecord.MLEscalateFrom;
                record.MLEscalatedComment = dbRecord.MLEscalatedComment;
                record.TrainingScope = dbRecord.TrainingScope;
                record.TrainingTermId = dbRecord.TrainingTermId;
                record.TrainingAddType = dbRecord.TrainingAddType;
                record.TrainingModelId = dbRecord.TrainingModelId;
            }
        }


        public static bool CheckExistAndTagDuplicateManual(this Record record) 
        {
            if (record != null) 
            {
                record.hasDuplicate =  record.RecordStatus == (int)RMRecordStatus.ManualPreSync && record.CreateDate == 0;
            }
            return record != null && (record.RecordStatus == (int)RMRecordStatus.Active || record.RecordStatus == (int)RMRecordStatus.RMDeleted || record.RecordStatus == (int)RMRecordStatus.ManualPreSync || record.RecordStatus == (int)RMRecordStatus.TrainingManualSync);
        }

        public static void AddSyncFailedMetaInfo(this Record record)
        {
            if (record != null)
            {
                var metaInfo = JsonConvert.DeserializeObject<RecordMetaInfo>(record.MetaInfo);
                metaInfo.IsSyncFailed = true;
                record.MetaInfo = JsonConvert.SerializeObject(metaInfo);
            }
        }
        public static void RemoveSyncFailedMetaInfo(this Record record)
        {
            if (record != null && !string.IsNullOrEmpty(record.MetaInfo))
            {
                var metaInfo = JsonConvert.DeserializeObject<RecordMetaInfo>(record.MetaInfo);
                metaInfo.IsSyncFailed = false;
                record.MetaInfo = JsonConvert.SerializeObject(metaInfo);
            }
        }

        public static void AppendMetaInfoForMovedData(this Record rec)
        {
            if (rec != null)
            {
                if (!string.IsNullOrWhiteSpace(rec.MetaInfo))
                {
                    var metaInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<RecordMetaInfo>(rec.MetaInfo);
                    metaInfo.DataStatus = (int)DataStatus.Moved;
                    rec.MetaInfo = JsonConvert.SerializeObject(metaInfo);
                }
                else
                {
                    RecordMetaInfo metaInfo = new RecordMetaInfo()
                    {
                        DataStatus = (int)DataStatus.Moved
                    };
                    rec.MetaInfo = JsonConvert.SerializeObject(metaInfo);
                }
            }
        }

        public static void RemoveManualFields(this Record r, bool isRemoveRule = true)
        {
            if (r != null)
            {
                r.IsManualSynced = false;
                r.ManualActionTime = 0;
                r.ManualApprovedBy = 0;
                r.ManualApprovedStatus = 0;
                r.ManualArchivedTime = 0;
                r.ManualArchiveStatus = 0;
                r.ManualAudits = string.Empty;
                r.ManualCollectionTime = 0;
                r.ManualEmailNotificationCount = 0;
                r.ManualEmailNotificationLastTime = 0;
                r.ManualEscalatedComment = string.Empty;
                r.ManualEscalateFrom = 0;
                r.ManualExtendComment = string.Empty;
                r.ManualExtendCount = 0;
                r.ManualExtendTime = 0;
                r.ManualFullPath = string.Empty;
                r.ManualSiteUrl = string.Empty;
                r.ManualFolderPath = string.Empty;
                r.ManualInternalApprovedStatus = 0;
                r.ManualIsAutoReassigned = false;
                r.ManualIsRelatedRecords = false;
                r.ManualNeedEmailNotification = false;
                r.ManualPartitionKey = string.Empty;
                r.ManualRelatedRecords = string.Empty;
                r.ManualRelatedRecordsAction = 0;
                r.ManualRetentionStatus = 0;
                r.ManualReviewer = null;
                r.ManualRowKey = string.Empty;
                r.ManualRuleCriteria = string.Empty;
                r.ManualVersion = string.Empty;
                r.ManualWorkflowDefinitionId = Guid.Empty;
                r.ManualWorkflowInstanceId = Guid.Empty;
                r.ManualWorkflowStepId = Guid.Empty;
                r.ManualModifiedTime = 0;
                r.ManualLastApproveRejectComment = string.Empty;
                r.ManualLastReviewedBy = string.Empty;
                r.ManualLastlReviewTime = 0;
                r.ManualModifiedTime = 0;
                r.IsInheritedTerm = false;

                if (isRemoveRule)
                {
                    r.ManualRuleDisposalClass = string.Empty;
                    r.ManualRuleName = string.Empty;
                    r.RuleId = Guid.Empty;
                    r.ManualDisposalDueDate = 0;
                }
            }
        }

        public static Record MergeRecords(this Record record, Record dbRecord)
        {
            Type type1 = record.GetType();
            Type type2 = dbRecord.GetType();

            PropertyInfo[] properties1 = type1.GetProperties();
            foreach (var property in properties1)
            {
                string name = property.Name;
                object value = property.GetValue(record);

                PropertyInfo propToSet = type2.GetProperty(name);
                if (propToSet == null || !property.CanWrite) continue;

                if(value == null || (value.GetType() == typeof(Guid) && (Guid)value == Guid.Empty))
                {
                    continue;
                }
                propToSet.SetValue(dbRecord, value);
            }

            return dbRecord;
        }
    }

    public static class StringExtension
    {
        #region Explorer search split

        static string[] stopWords = new string[] { "a", "an", "are", "as", "at", "be", "by", "for", "in", "is", "it", "of", "on", "or", "the", "to", "was", "will", "with", "the" };
        static char[] seperator = new char[] { ' ', '<', '{', '>', ' ', ',', '_', '|', '"', '\'', '/', '\\', ':', ';', '(', ')', '-', '\n', '\t', '}', '[', ']', '=', '+', '~', '&', '@' };

        public static string[] SplitBySpace(this string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            return key.Split(' ');
        }

        public static string[] SplitSearchKey(this string name)
        {
            if (name == null || name == string.Empty)
            {
                return null;
            }
            string[] terms = name.Split(seperator).ToArray();
            List<string> temp = new List<string>();
            foreach (string t in terms)
            {
                if (string.IsNullOrEmpty(t))
                {
                    continue;
                }
                if (t.Contains('.'))
                {
                    //在拆分Search Key里这部分不重复添加double数据
                    string[] subterms = t.Split('.');
                    foreach (string sub in subterms)
                    {
                        string lowerSub = sub.ToLower();
                        if (!stopWords.Contains(lowerSub))
                        {
                            temp.Add(lowerSub);
                        }
                    }
                }
                else
                {
                    string lowerT = t.ToLower();
                    if (!stopWords.Contains(lowerT))
                    {
                        temp.Add(lowerT);
                    }
                }
            }
            string[] result = temp.Distinct().ToArray();
            if (result.Length > 0)
            {
                return result;
            }
            return new string[] { name.ToLower() };
        }

        /// <summary>
        /// 解析一般Column， 在第一位添加一个小写的原文本
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string[] ExplorerSearchSplit(this string name)
        {
            if (name == null || name == string.Empty)
            {
                return null;
            }
            string[] terms = name.Split(seperator).ToArray();
            List<string> temp = new List<string>();
            foreach (string t in terms)
            {
                if (string.IsNullOrEmpty(t))
                {
                    continue;
                }
                if (t.Contains('.'))
                {
                    double output = 0.0;
                    if (double.TryParse(t, out output))
                    {
                        temp.Add(t);
                    }
                    string[] subterms = t.Split('.');
                    foreach (string sub in subterms)
                    {
                        string lowerSub = sub.ToLower();
                        if (!stopWords.Contains(lowerSub))
                        {
                            temp.Add(lowerSub);
                        }
                    }
                }
                else
                {
                    string lowerT = t.ToLower();
                    if (!stopWords.Contains(lowerT))
                    {
                        temp.Add(lowerT);
                    }
                }
            }
            temp.Insert(0, name.ToLower()); //插入一条原文
            string[] result = temp.Distinct().ToArray();
            if (result.Length > 0)
            {
                return result;
            }
            return new string[] { name.ToLower() };
        }
        /// <summary>
        /// Unique ID, 解析suffix的数字， 在数组的第一位再加一条小写的
        /// 原文本
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public static string[] ExplorerAnalyzeUniqueId(this string uniqueId)
        {
            string[] terms = ExplorerSearchSplit(uniqueId);
            if (terms != null && terms.Length > 0)
            {
                List<string> termsList = terms.ToList();
                string suffix = termsList.Last();
                long number = 0;
                if (long.TryParse(suffix, out number))
                {
                    if (number.ToString() != suffix)
                    {
                        termsList.Add(number.ToString());
                    }
                }
                if (termsList.Count > 1)
                {
                    termsList.Insert(0, uniqueId.ToLower());  //Unique Id加入本身， 实现完全匹配
                }
                return termsList.Distinct().ToArray();
            }
            return terms;
        }

        /// <summary>
        /// 解析Name等， 在数组的第一位加一条小写的原文本
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string[] ExplorerAnalyzeBuiltInColumn(this string name)
        {
            if (name == null || name == string.Empty)
            {
                return null;
            }
            string[] terms = name.Split(seperator).ToArray();
            List<string> temp = new List<string>();
            foreach (string t in terms)
            {
                if (string.IsNullOrEmpty(t))
                {
                    continue;
                }
                if (t.Contains('.'))
                {
                    double output = 0.0;
                    if (double.TryParse(t, out output))
                    {
                        temp.Add(t);
                    }
                    string[] subterms = t.Split('.');
                    foreach (string sub in subterms)
                    {
                        string lowerSub = sub.ToLower();
                        if (!stopWords.Contains(lowerSub))
                        {
                            temp.Add(lowerSub);
                        }
                    }
                }
                else
                {
                    string lowerT = t.ToLower();
                    if (!stopWords.Contains(lowerT))
                    {
                        temp.Add(lowerT);
                    }
                }
            }
            temp.Insert(0, name.ToLower());
            string[] result = temp.Distinct().ToArray();
            if (result.Length > 0)
            {
                return result;
            }
            return new string[] { name.ToLower() };
        }
        #endregion
    }
}
