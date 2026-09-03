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
using AvePoint.Contract.ExportLocation;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Agent.SharePointBrowser.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Hybrid.Utility.Util;
using AvePoint.Media.Storage;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Common.Utils.ProtoBuf;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json;
using RAFileSystem.FileSystem.Common;
using RAFileSystem.FileSystem.DataSync.Utils;
using RAFileSystem.Utils;
using RAFileSystemCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace AvePoint.RA.FileSystem.Collect
{
    /// <summary>
    /// check rules and store result (to cache)
    /// </summary>
    internal class FSObjectAnalyzer
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Guid scopeId { get; set; }

        public FSObjectAnalyzer()
        {
            scopeId = FSJobCache.Instance.RootPath.ToLowerInvariant().ToMd5();
        }
        public FileSystemRecordDto Analyze(Stub stub, int classificationLevel = (int)NodeLevel.FSFile)
        {
            using (new AgentPerformanceScope("FSObjectAnalyzer.AnalyzeStub", addToStatistics: true))
            {
                if (stub.Type == Stub.StubType.ConnectionGroups || stub.Type == Stub.StubType.ConnectionGroup)
                {
                    var top2LevelRecord = GenerateTop2LevelRecord(stub);
                    if (JobContext.Current.EnableFSHighPerformanceMode && stub.DBRecord != null)
                    {
                        top2LevelRecord.TimeCreated1 = stub.DBRecord.TimeCreated1;
                    }
                    return top2LevelRecord;
                }
                logger.Debug("Start to analyze record: {0}", stub.SelfId);
                FileSystemRecordDto record = new FileSystemRecordDto();
                record.BulkImportEnabled = JobContext.Current.BulkImportEnabled;
                record.BulkSize = JobContext.Current.BulkSize;
                record.RecordStatus = 1;
                record.Depth = stub.Depth;
                AssembleBasicInfo(record, stub);
                if (stub.Type == Stub.StubType.File)
                {
                    AssembleTerm(record, stub, stub.DBRecord);
                    AssembleRule(record, stub, stub.DBRecord);
                    if (stub.DBRecord != null)
                    {
                        record.CreateDate = stub.DBRecord.CreateDate;
                        record.hasDuplicated = stub.DBRecord.RecordStatus == (int)RMRecordStatus.ManualPreSync && stub.DBRecord.CreateDate == 0;
                    }
                }
                else if(stub.Type == Stub.StubType.Folder && classificationLevel == (int)NodeLevel.FSFolder)
                {
                    AssembleTermForFolder(record, stub);
                }
                AssembleManuallyChangedProperties(record, stub, stub.DBRecord);
                return record;
            }
        }


        public void AssembleDBRecords(IEnumerable<Stub> stubs)
        {
            var pairs = stubs.ToDictionary(p => p.SelfId);
            var recordStr = JobContext.Current.ApiClient.GetRecords(new List<Guid>(pairs.Keys));
            if (!string.IsNullOrWhiteSpace(recordStr))
            {
                var recordsPairs = JsonConvert.DeserializeObject<List<FileSystemRecordDto>>(recordStr).ToDictionary(r => r.NodeId);
                foreach (var p in pairs)
                {
                    if (recordsPairs.ContainsKey(p.Key))
                    {
                        p.Value.DBRecord = recordsPairs[p.Key];
                    }
                }
            }
        }
        //private FileSystemObjectDto QueryExistingRecord(FileSystemObjectDto record)
        //{
        //    FileSystemObjectDto dbRecord = new FileSystemObjectDto(); ;
        //    //using (new AgentPerformanceScope("Analyze---QueryExistingRecord"))
        //    {
        //       // dbRecord = dBProcessor.QueryExistingRecords(new List<Guid>() { record.ItemId });
        //    }
        //    return dbRecord;
        //}

        private void AssembleManuallyChangedProperties(FileSystemRecordDto record, Stub stub, FileSystemRecordDto dbRecord)
        {
            using (new AgentPerformanceScope("FSObjectAnalyzer.AssembleFileBasicInfo", addToStatistics: true))
            {
                if (dbRecord != null)
                {
                    record.DeclareAsRecord = dbRecord.DeclareAsRecord;
                    record.DeclaredBy = dbRecord.DeclaredBy;
                    record.HoldStatus = dbRecord.HoldStatus;
                    record.HoldReleaseTime = dbRecord.HoldReleaseTime;
                    record.HoldBy = dbRecord.HoldBy;
                    record.HoldId = dbRecord.HoldId;
                    record.HoldType = dbRecord.HoldType;
                    //AssembleRule处理RecordOwner
                    //record.RecordOwner = dbRecord.RecordOwner;
                    record.RecordsId = dbRecord.RecordsId;
                    record.RecordHistory = dbRecord.RecordHistory;
                    record.RelatedRecords = dbRecord.RelatedRecords;
                    record.RelatedRecordsCount = dbRecord.RelatedRecordsCount;

                    record.IsManualSynced = dbRecord.IsManualSynced;
                    record.ManualActionTime = dbRecord.ManualActionTime;
                    record.ManualApprovedBy = dbRecord.ManualApprovedBy;
                    record.ManualEscalatedComment = dbRecord.ManualEscalatedComment;
                    record.ManualApprovedStatus = dbRecord.ManualApprovedStatus;
                    record.ManualArchiveStatus = dbRecord.ManualArchiveStatus;
                    record.ManualInternalApprovedStatus = dbRecord.ManualInternalApprovedStatus;
                    record.ManualFullPath = dbRecord.ManualFullPath;
                    record.ManualEscalateFrom = dbRecord.ManualEscalateFrom;
                    record.ManualExtendTime = dbRecord.ManualExtendTime;
                    record.ManualExtendComment = dbRecord.ManualExtendComment;
                    record.ManualCollectionTime = dbRecord.ManualCollectionTime;
                    record.ManualAudits = dbRecord.ManualAudits;
                    record.ManualArchivedTime = dbRecord.ManualArchivedTime;
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
                    record.ManualExtendCount = dbRecord.ManualExtendCount;
                    record.ManualEmailNotificationCount = dbRecord.ManualEmailNotificationCount;
                    record.ManualEmailNotificationLastTime = dbRecord.ManualEmailNotificationLastTime;
                    record.ManualNeedEmailNotification = dbRecord.ManualNeedEmailNotification;
                    record.ManualIsAutoReassigned = dbRecord.ManualIsAutoReassigned;

                    record.HoldByUsers = dbRecord.HoldByUsers;
                    record.HoldUntilTimes = dbRecord.HoldUntilTimes;
                    record.AppendHolds_Array = dbRecord.AppendHolds_Array;
                    if (!string.IsNullOrEmpty(dbRecord.ClassCode) && FSJobCache.Instance.EnableJPMC)
                    {
                        logger.Info($"AssembleManuallyChangedProperties current itme has added in the cosmos,current id:{record.NodeId},class code:{dbRecord.ClassCode}");
                        if (dbRecord.NodeType == (int)NodeLevel.FSFolder)
                        {
                            lock (FSJobCache.Instance.ContainerLevelClassCodeCacheLock)
                            {
                                if (!FSJobCache.Instance.ContainerLevelClassCodeCache.ContainsKey(dbRecord.NodeId))
                                {
                                    FSJobCache.Instance.ContainerLevelClassCodeCache.Add(dbRecord.NodeId, new ClassCodeInfoDto()
                                    {
                                        ClassCode = dbRecord.ClassCode,
                                        CountryCode = dbRecord.CountryCode,
                                        RetentionType = dbRecord.RetentionType,
                                        TermId = dbRecord.TermId,
                                        StartDate = dbRecord.StartDate,
                                        EndTime = dbRecord.EndTime,
                                        PolicyValueNumber = dbRecord.PolicyValueNumber,
                                        PolicyValueUnit = dbRecord.PolicyValueUnit,
                                        CollectionTime = dbRecord.CollectionTime,
                                    });
                                }
                            }
                        }
                        bool ruleHasModifed = ClassCodeCommonStaticMethod.IsRuleModified(dbRecord.TermId, dbRecord.CollectionTime);
                        if (!string.IsNullOrEmpty(dbRecord.ClassCode) && (ruleHasModifed || dbRecord.TimeLastModified < record.TimeLastModified))
                        {
                            logger.Info($"rule or file modified time changed,will reset endtime,current id:{record.NodeId},endtime:{record.EndTime},ruleHasModifed:{ruleHasModifed},dbRecord.TimeLastModified < record.TimeLastModified:{dbRecord.TimeLastModified < record.TimeLastModified}");
                            ClassCodeCommonStaticMethod.GenerateRetentionTimeCacheKeyAndSetEndTime(record, new ClassCodeInfoDto() { 
                                ClassCode = dbRecord.ClassCode,
                                CountryCode = dbRecord.CountryCode,
                                RetentionType = dbRecord.RetentionType,
                                StartDate = dbRecord.StartDate,
                                TermId = dbRecord.TermId,
                            });
                        }
                        else if (FSJobCache.Instance.classCodeInfoDtoOnNode != null && !string.IsNullOrEmpty(FSJobCache.Instance.classCodeInfoDtoOnNode.ClassCode) && FSJobCache.Instance.CurrentNodeIsEnableRecordManagement)
                        {
                            record.EndTime = dbRecord.EndTime;
                            record.PolicyValueUnit = dbRecord.PolicyValueUnit;
                            record.PolicyValueNumber = dbRecord.PolicyValueNumber;
                        }
                        else
                        {
                            record.EndTime = 0;//need to set 0 for can not get the class code setting
                            record.PolicyValueUnit = dbRecord.PolicyValueUnit;
                            record.PolicyValueNumber = dbRecord.PolicyValueNumber;
                        }
                        record.ClassCode = dbRecord.ClassCode;
                        record.CountryCode = dbRecord.CountryCode;
                        record.RetentionType = dbRecord.RetentionType;
                        record.TermId = dbRecord.TermId;
                        record.StartDate = dbRecord.StartDate;
                        record.TermName = dbRecord.ClassCode;
                    }
                }
                else
                {
                    record.RecordsId = "";
                    record.RelatedRecords = "";
                    if (FSJobCache.Instance.EnableJPMC && FSJobCache.Instance.ConnectionPath == record.FullPath)
                    {
                        logger.Info($"this is connection node and has setting,node id:{record.NodeId}，FSJobCache.Instance.classCodeInfoDtoOnNode != null：{FSJobCache.Instance.classCodeInfoDtoOnNode != null}");
                        if (FSJobCache.Instance.classCodeInfoDtoOnNode != null && !string.IsNullOrEmpty(FSJobCache.Instance.classCodeInfoDtoOnNode.ClassCode))
                        {
                            AssembleRecord(record);
                        }
                    }
                    else if (FSJobCache.Instance.ContainerLevelClassCodeCache.ContainsKey(record.ParentId) && FSJobCache.Instance.EnableJPMC)
                    {
                        logger.Info($"AssembleManuallyChangedProperties ContainerLevelClassCodeCache contains this item parent id:{record.ParentId},current id:{record.NodeId}");
                        var classCodeDto = FSJobCache.Instance.ContainerLevelClassCodeCache[record.ParentId];
                        record.CountryCode = classCodeDto.CountryCode;
                        record.ClassCode = classCodeDto.ClassCode;
                        record.RetentionType = classCodeDto.RetentionType;
                        record.StartDate = classCodeDto.StartDate;
                        record.TermId = classCodeDto.TermId;
                        record.TermName = classCodeDto.ClassCode;
                        record.PolicyValueNumber = classCodeDto.PolicyValueNumber;
                        record.PolicyValueUnit = classCodeDto.PolicyValueUnit;
                        if (FSJobCache.Instance.classCodeInfoDtoOnNode != null && !string.IsNullOrEmpty(FSJobCache.Instance.classCodeInfoDtoOnNode.ClassCode))
                        {
                            logger.Info($"AssembleManuallyChangedProperties ContainerLevelClassCodeCache ApplyExistDocuments:{FSJobCache.Instance.classCodeInfoDtoOnNode.ApplyExistDocuments},current id:{record.NodeId},classCodeDto.EnableRecordManagement:{classCodeDto.EnableRecordManagement}");
                            if (FSJobCache.Instance.CurrentNodeIsEnableRecordManagement)
                            {
                                bool ruleHasModifed = ClassCodeCommonStaticMethod.IsRuleModified(record.TermId, classCodeDto.CollectionTime);
                                if (FSJobCache.Instance.ConnectionPath == record.DirPath)
                                {
                                    record.EndTime = FSJobCache.Instance.classCodeInfoDtoOnNode.RetentionType == (int)RetentionScheduleType.Event ? FileSystemContractHelper.CalculateEndTime(FSJobCache.Instance.classCodeInfoDtoOnNode.StartDate, FSJobCache.Instance.classCodeInfoDtoOnNode.PolicyValueUnit, FSJobCache.Instance.classCodeInfoDtoOnNode.PolicyValueNumber) : FileSystemContractHelper.CalculateEndTime(record.TimeLastModified, FSJobCache.Instance.classCodeInfoDtoOnNode.PolicyValueUnit, FSJobCache.Instance.classCodeInfoDtoOnNode.PolicyValueNumber);
                                }
                                else if (ruleHasModifed)
                                {
                                    ClassCodeCommonStaticMethod.GenerateRetentionTimeCacheKeyAndSetEndTime(record, classCodeDto);
                                    logger.Info($"rule not changed1,will not reset endtime,current id:{record.NodeId},endtime:{record.EndTime}");
                                }
                                else
                                {
                                    record.EndTime = classCodeDto.RetentionType == (int)RetentionScheduleType.Event ? FileSystemContractHelper.CalculateEndTime(classCodeDto.StartDate, classCodeDto.PolicyValueUnit, classCodeDto.PolicyValueNumber) : FileSystemContractHelper.CalculateEndTime(record.TimeLastModified, classCodeDto.PolicyValueUnit, classCodeDto.PolicyValueNumber);
                                }
                            }
                        }
                        else
                        {
                            logger.Info($"AssembleManuallyChangedProperties not use the parent date to calculate endtime:{record.ParentId},current id:{record.NodeId}");
                            record.EndTime = 0; //need to set 0 for can not get the class code setting
                        }
                    }
                    else if(FSJobCache.Instance.EnableJPMC)
                    {
                        var parentResult = JobContext.Current.ApiClient.GetRecords(new List<Guid>() { record.ParentId });
                        if (!string.IsNullOrEmpty(parentResult))
                        {
                            try
                            {
                                List<FileSystemRecordDto> fileSystemRecordDtos = JsonConvert.DeserializeObject<List<FileSystemRecordDto>>(parentResult);
                                if (fileSystemRecordDtos != null && fileSystemRecordDtos.Count > 0)
                                {
                                    var tempResult = fileSystemRecordDtos.FirstOrDefault();
                                    lock (FSJobCache.Instance.ContainerLevelClassCodeCacheLock)
                                    {
                                        if (!FSJobCache.Instance.ContainerLevelClassCodeCache.ContainsKey(tempResult.NodeId))
                                        {
                                            FSJobCache.Instance.ContainerLevelClassCodeCache.Add(tempResult.NodeId, new ClassCodeInfoDto()
                                            {
                                                ClassCode = tempResult.ClassCode,
                                                CountryCode = tempResult.CountryCode,
                                                RetentionType = tempResult.RetentionType,
                                                TermId = tempResult.TermId,
                                                StartDate = tempResult.StartDate,
                                                EndTime = tempResult.EndTime,
                                                PolicyValueNumber = tempResult.PolicyValueNumber,
                                                PolicyValueUnit = tempResult.PolicyValueUnit,
                                                CollectionTime = tempResult.CollectionTime,
                                            });
                                        }
                                    }

                                }
                            }
                            catch (Exception e)
                            {
                                logger.Error($"deserialize to List<FileSystemRecordDto> failed,error:{e}");
                            }
                            if (FSJobCache.Instance.ContainerLevelClassCodeCache.ContainsKey(record.ParentId))
                            {
                                var classCodeDto = FSJobCache.Instance.ContainerLevelClassCodeCache[record.ParentId];
                                record.CountryCode = classCodeDto.CountryCode;
                                record.ClassCode = classCodeDto.ClassCode;
                                record.RetentionType = classCodeDto.RetentionType;
                                record.StartDate = classCodeDto.StartDate;
                                record.TermId = classCodeDto.TermId;
                                record.TermName = classCodeDto.ClassCode;
                                record.PolicyValueNumber = classCodeDto.PolicyValueNumber;
                                record.PolicyValueUnit = classCodeDto.PolicyValueUnit;
                                if (FSJobCache.Instance.classCodeInfoDtoOnNode != null && !string.IsNullOrEmpty(FSJobCache.Instance.classCodeInfoDtoOnNode.ClassCode))
                                {
                                    logger.Info($"AssembleManuallyChangedProperties ContainerLevelClassCodeCache ApplyExistDocuments1:{FSJobCache.Instance.classCodeInfoDtoOnNode.ApplyExistDocuments},current id:{record.NodeId},classCodeDto.EnableRecordManagement:{classCodeDto.EnableRecordManagement}");
                                    if (FSJobCache.Instance.CurrentNodeIsEnableRecordManagement)
                                    {
                                        bool ruleHasModifed = ClassCodeCommonStaticMethod.IsRuleModified(record.TermId, classCodeDto.CollectionTime);
                                        if (FSJobCache.Instance.ConnectionPath == record.DirPath)
                                        {
                                            record.EndTime = FSJobCache.Instance.classCodeInfoDtoOnNode.RetentionType == (int)RetentionScheduleType.Event ? FileSystemContractHelper.CalculateEndTime(FSJobCache.Instance.classCodeInfoDtoOnNode.StartDate, FSJobCache.Instance.classCodeInfoDtoOnNode.PolicyValueUnit, FSJobCache.Instance.classCodeInfoDtoOnNode.PolicyValueNumber) : FileSystemContractHelper.CalculateEndTime(record.TimeLastModified, FSJobCache.Instance.classCodeInfoDtoOnNode.PolicyValueUnit, FSJobCache.Instance.classCodeInfoDtoOnNode.PolicyValueNumber);
                                        }
                                        else if (ruleHasModifed)
                                        {
                                            ClassCodeCommonStaticMethod.GenerateRetentionTimeCacheKeyAndSetEndTime(record, classCodeDto);
                                            logger.Info($"rule has changed2,will reset endtime,current id:{record.NodeId},endtime:{record.EndTime}");
                                        }
                                        else
                                        {
                                            record.EndTime = classCodeDto.RetentionType == (int)RetentionScheduleType.Event ? FileSystemContractHelper.CalculateEndTime(classCodeDto.StartDate, classCodeDto.PolicyValueUnit, classCodeDto.PolicyValueNumber) : FileSystemContractHelper.CalculateEndTime(record.TimeLastModified, classCodeDto.PolicyValueUnit, classCodeDto.PolicyValueNumber);
                                        }
                                    }
                                }
                                else
                                {
                                    logger.Info($"AssembleManuallyChangedProperties2 not use the parent date to calculate endtime:{record.ParentId},current id:{record.NodeId}");
                                    record.EndTime = 0; //need to set 0 for can not get the class code setting
                                }
                            }

                        }
                        else if (FSJobCache.Instance.classCodeInfoDtoOnNode != null && !string.IsNullOrEmpty(FSJobCache.Instance.classCodeInfoDtoOnNode.ClassCode))
                        {
                            AssembleRecord(record);
                        }
                        logger.Info($"AssembleManuallyChangedProperties ContainerLevelClassCodeCache not contains this item parent id:{record.ParentId},current id:{record.NodeId},FSJobCache.Instance.classCodeInfoDtoOnNode not null?:{FSJobCache.Instance.classCodeInfoDtoOnNode != null},classCodeInfoDtoOnNode setting class code is null?:{FSJobCache.Instance.classCodeInfoDtoOnNode?.ClassCode},parent container items:{FSJobCache.Instance.ContainerLevelClassCodeCache?.Count},parentResult from cosmos:{parentResult}");
                    }
                }
            }
        }

        private void AssembleRecord(FileSystemRecordDto record)
        {
            logger.Info($"AssembleManuallyChangedProperties ContainerLevelClassCodeCache not contains this item parent id:{record.ParentId},current id:{record.NodeId},will use the node setting");
            record.CountryCode = FSJobCache.Instance.classCodeInfoDtoOnNode.CountryCode;
            record.ClassCode = FSJobCache.Instance.classCodeInfoDtoOnNode.ClassCode;
            record.RetentionType = FSJobCache.Instance.classCodeInfoDtoOnNode.RetentionType;
            record.StartDate = FSJobCache.Instance.classCodeInfoDtoOnNode.StartDate;
            record.TermId = FSJobCache.Instance.classCodeInfoDtoOnNode.TermId;
            record.TermName = FSJobCache.Instance.classCodeInfoDtoOnNode.ClassCode;
            record.PolicyValueNumber = FSJobCache.Instance.classCodeInfoDtoOnNode.PolicyValueNumber;
            record.PolicyValueUnit = FSJobCache.Instance.classCodeInfoDtoOnNode.PolicyValueUnit;
            if (FSJobCache.Instance.CurrentNodeIsEnableRecordManagement)
            {
                record.EndTime = FSJobCache.Instance.classCodeInfoDtoOnNode.RetentionType == (int)RetentionScheduleType.Event ? FileSystemContractHelper.CalculateEndTime(FSJobCache.Instance.classCodeInfoDtoOnNode.StartDate, FSJobCache.Instance.classCodeInfoDtoOnNode.PolicyValueUnit, FSJobCache.Instance.classCodeInfoDtoOnNode.PolicyValueNumber) : FileSystemContractHelper.CalculateEndTime(record.TimeLastModified, FSJobCache.Instance.classCodeInfoDtoOnNode.PolicyValueUnit, FSJobCache.Instance.classCodeInfoDtoOnNode.PolicyValueNumber);
            }
        }
        private void AssembleBasicInfo(FileSystemRecordDto record, Stub stub)
        {
            if (stub.Type == Stub.StubType.File)
            {
                using (new AgentPerformanceScope("FSObjectAnalyzer.AssembleFileBasicInfo", addToStatistics: true))
                {
                    AssembleFileBasicInfo(record, stub);
                }
            }
            else if (stub.Type == Stub.StubType.Folder)
            {
                using (new AgentPerformanceScope("FSObjectAnalyzer.AssembleFolderBasicInfo", addToStatistics: true))
                {
                    AssembleFolderBasicInfo(record, stub);
                }
            }
        }
        private void AssembleFolderBasicInfo(FileSystemRecordDto record, Stub stub)
        {
            try
            {
                XDirectoryInfoEx xObj = new XDirectoryInfoEx(stub.MediaObj);
                record.AveSiteId = FSJobCache.Instance.AveConnectionId.ToString();
                //record.CollectionTime = DateTime.UtcNow.Ticks;
                if (xObj.Owner.Contains('\\'))
                {
                    var splitCreateBy = xObj.Owner.Split('\\');
                    var createByName = splitCreateBy[1];
                    if (createByName.Any(char.IsUpper) && createByName.Any(char.IsLower))
                    {
                        record.CreatedBy = xObj.Owner;
                    }
                    else
                    {
                        record.CreatedBy = string.Join("\\", splitCreateBy[0], createByName.ToLower());
                    }
                }
                else
                {
                    record.CreatedBy = xObj.Owner;
                }
                record.DirPath = Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(stub.FullPath);
                record.FolderId = stub.ParentId;
                record.FullPath = stub.FullPath;
                record.ItemId = stub.SelfId;
                record.ItemRowId = -1;
                record.LeafName = xObj.Name;
                record.ListId = Guid.Empty;
                record.NodeId = stub.SelfId;
                record.NodeType = (int)NodeLevel.FSFolder;
                record.ScopeId = scopeId;
                record.SourceFlag = (int)SourceFlag.FileSystem;
                record.TimeCreated1 = xObj.CreationTimeUtc;
                record.TimeLastModified = xObj.LastWriteTimeUtc.Ticks;
                record.ParentId = stub.ParentId;
                record.SortTicks = Snowflake.Instance().GetTicks();
                RecordMetaInfo metaInfo = new RecordMetaInfo
                {
                    FileSize = xObj.Length,
                    LocalFullPath = xObj.LocalFullPath,
                    LastModifiedTime = xObj.LastWriteTimeUtc.Ticks,
                    CreatedTime = xObj.CreationTimeUtc.Ticks,
                };
                record.FileSize = xObj.Length;
                record.MetaInfo = JsonConvert.SerializeObject(metaInfo);
                record.JPMCFSFileSize = stub.MediaObj.Length;
                record.JPMCFSFileCount = stub.MediaObj.TotalFileCount;
            }
            catch (Exception ex)
            {
                logger.Error("Failed to assemble the basic info of the folder. Exception:{0}", ex.ToString());
                throw;
            }
        }
        private void AssembleFileBasicInfo(FileSystemRecordDto record, Stub stub)
        {
            try
            {
                XFileInfoEx xObj = new XFileInfoEx(stub.MediaObj);
                record.ScopeId = FSJobCache.Instance.RootPath.ToLowerInvariant().ToMd5();
                record.AveSiteId = FSJobCache.Instance.AveConnectionId.ToString();
                // record.CreateDate = DateTime.UtcNow.Ticks;
                if (xObj.Owner.Contains('\\'))
                {
                    var splitCreateBy = xObj.Owner.Split('\\');
                    var createByName = splitCreateBy[1];
                    if (createByName.Any(char.IsUpper) && createByName.Any(char.IsLower))
                    {
                        record.CreatedBy = xObj.Owner;
                    }
                    else
                    {
                        record.CreatedBy = string.Join("\\", splitCreateBy[0], createByName.ToLower());
                    }
                }
                else 
                {
                    record.CreatedBy = xObj.Owner;
                }
                record.DirPath = Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(stub.FullPath);
                record.ExtensionForFile = Alphaleonis.Win32.Filesystem.Path.GetExtension(xObj.FileFullPath).TrimStart(new char[] { '.' });
                record.FolderId = stub.ParentId;
                record.FullPath = stub.FullPath;
                record.ItemId = stub.SelfId;
                record.LeafName = xObj.Name;
                record.NodeId = stub.SelfId;
                record.NodeType = (int)NodeLevel.FSFile;
                record.SourceFlag = (int)SourceFlag.FileSystem;
                record.TimeCreated1 = xObj.CreationTimeUtc;
                record.TimeLastModified = xObj.LastWriteTimeUtc.Ticks;
                record.SortTicks = Snowflake.Instance().GetTicks();
                string fileTypeName = FileTypeDescriptionResolver.Resolve(record.ExtensionForFile);
                RecordMetaInfo metaInfo = new RecordMetaInfo
                {
                    FileSize = xObj.FileSize,
                    LastAccessTime = xObj.LastAccessTimeUtc.Ticks,
                    Owner = xObj.Owner,
                    LocalFullPath = xObj.FileFullPath,
                    CreatedTime = xObj.CreationTimeUtc.Ticks,
                    LastModifiedTime = xObj.LastWriteTimeUtc.Ticks,
                    FileTypeName = fileTypeName,
                };
                record.FileSize = xObj.FileSize;
                record.ParentId = stub.ParentId;
                record.MetaInfo = JsonConvert.SerializeObject(metaInfo);
                record.JPMCFSFileSize = stub.MediaObj.Length;
                record.JPMCFSFileCount = 1;
            }
            catch (Exception ex)
            {
                logger.Error("Failed to assemble the basic info of the file/folder. Exception:{0}", ex.ToString());
                throw;
            }
        }

        private void AssembleTermForFolder(FileSystemRecordDto record, Stub stub)
        {

            using (new AgentPerformanceScope("FSObjectAnalyzer.AssembleTermForFolder", addToStatistics: true))
            {
                
                FSSettingDto setting;
                if (FSJobCache.Instance.ScopeSettingCache.TryGetValue(stub.ScopeSettingId, out setting))
                {
                    record.FSSettingDto = setting;
                    if(setting.NeedCheckDefaultValue) //ApplyExistingTermType.OverWrite or Skip all use default term
                    {
                        var termid = setting.DefaultTermId; 
                        if (FSJobCache.Instance.Terms.TryGetValue(termid, out var temp))
                        {
                            //var tempTerm = FSJobCache.Instance.Terms[termid];
                            //var termInvalid = false;
                            //if (tempTerm == null || tempTerm.IsDeprecated || tempTerm.IsRemoved)
                            //{
                            //    termInvalid = true;
                            //}
                            //if (tempTerm.TermExpirationFrom != 0 || tempTerm.TermExpirationTo != 0)
                            //{
                            //    if (DateTime.UtcNow.Ticks < tempTerm.TermExpirationFrom || (tempTerm.TermExpirationTo != 0 && DateTime.UtcNow.Ticks > tempTerm.TermExpirationTo))
                            //    {
                            //        termInvalid = true;
                            //    }
                            //}
                            //if (termInvalid)
                            //{
                            //    logger.Warn("Term is invalid [{0}].", termid);
                            //    throw new Exception("RM_FS_DisposalDetail_TermIsInvalid" + I18NEntity.Separator + tempTerm.Name);
                            //}
                            var termName = temp.Name;
                            record.TermId = termid;
                            record.TermName = termName;
                        }
                        else
                        {
                            logger.Warn("Cannot find the term with id [{0}] from the cache.", termid);
                        }
                    }
                    else
                    {
                        record.TermId = stub.TermId4Folder;
                        record.TermName = stub.TermName4Folder;
                    }
                }
                else
                {
                    record.TermId = stub.TermId4Folder;
                    record.TermName = stub.TermName4Folder;
                    logger.Warn("There is no term setting for the ID:{0}", stub.ScopeSettingId);
                }
                CheckTermExpired(record.TermId);
            }
        }

        private void CheckTermExpired(Guid termid)
        {
            if (FSJobCache.Instance.Terms.TryGetValue(termid, out var tempTerm))
            {
                var termInvalid = false;
                if (tempTerm == null || tempTerm.IsDeprecated || tempTerm.IsRemoved)
                {
                    termInvalid = true;
                }
                if (tempTerm.TermExpirationFrom != 0 || tempTerm.TermExpirationTo != 0)
                {
                    if (DateTime.UtcNow.Ticks < tempTerm.TermExpirationFrom || (tempTerm.TermExpirationTo != 0 && DateTime.UtcNow.Ticks > tempTerm.TermExpirationTo))
                    {
                        termInvalid = true;
                    }
                }
                if (termInvalid)
                {
                    logger.Warn("Term is invalid [{0}].", termid);
                    throw new Exception("RM_FS_DisposalDetail_TermIsInvalid" + I18NEntity.Separator + tempTerm.Name);
                }              
            }
        }

        private void AssembleTerm(FileSystemRecordDto record, Stub stub, FileSystemRecordDto dbRecord)
        {
            using (new AgentPerformanceScope("FSObjectAnalyzer.AssembleTerm", addToStatistics: true))
            {
                var jobType = FSJobCache.Instance.JobController.JobType;
                var termConflictOption = FSJobCache.Instance.JobController.TermConflictOption;
                record.TermId = Guid.Empty;
                record.TermName = string.Empty;
                bool needRecomputeTerm = false;
                if (jobType == FSJobType.UserFullJob || jobType == FSJobType.IncrementalJob)
                {
                    if (dbRecord == null 
                        || dbRecord.TermId == null 
                        || dbRecord.TermId == Guid.Empty 
                        || termConflictOption == TermConflictOption.Overwrite)
                    {
                        logger.Debug($"Current JobType [{jobType.ToString()}] and DBRecord of file [{stub.SelfId}] is null or term id is null that needRecomputeTerm.");
                        needRecomputeTerm = true;
                    }
                }
                if (jobType == FSJobType.RematchRuleFullJob)
                {
                    if ((record.TimeCreated1 > FSJobCache.Instance.JobController.IncrementalStartTime
                        || record.TimeLastModified > FSJobCache.Instance.JobController.IncrementalStartTime.Ticks))
                    {
                        logger.Debug($"Current JobType [{jobType.ToString()}] and file [{stub.SelfId}] is modified that needRecomputeTerm.");
                        needRecomputeTerm = true;
                    }

                    if (dbRecord != null)
                    {
                        if (FSJobCache.Instance.ChangedTermIds != null && dbRecord.TermId != null && dbRecord.TermId != Guid.Empty && FSJobCache.Instance.ChangedTermIds.Contains(dbRecord.TermId))
                        {
                            logger.Debug($"Current change term container DB change term id [{dbRecord.TermId}], file [{stub.SelfId}].");
                            if (termConflictOption == TermConflictOption.Overwrite)
                            {
                                logger.Debug($"Current job TermConflictOption is Overwrite and needRecomputeTerm.File [{stub.SelfId}].");
                                needRecomputeTerm = true;
                            }
                            else
                            {
                                logger.Debug($"Current job TermConflictOption is [{termConflictOption.ToString()}] and don't needRecomputeTerm.File [{stub.SelfId}].");
                            }
                        }
                    }
                }
                if (needRecomputeTerm)
                {
                    Guid termid = Guid.Empty;
                    string termName = string.Empty;
                    FSSettingDto setting;
                    if (FSJobCache.Instance.ScopeSettingCache.TryGetValue(stub.ScopeSettingId, out setting))
                    {
                        termid = setting.DefaultTermId;
                        //AutoClassification
                        if (stub.Type == Stub.StubType.File && setting.DeployTermMethod == (int)DeployTermMethod.UseAutoClassification)
                        {
                            List<Rule> autoRule = FSJobCache.Instance.AutoRuleCollections[stub.ScopeSettingId];
                            DisposalRuleEngine ruleEngine = new DisposalRuleEngine(autoRule);
                            AvePoint.Common.FilterEngine.ObjectInfoBase filterObj = ObjectConverter.ConvertXObject2FilterObject(new XFileInfoEx(stub.MediaObj), FSJobCache.Instance.RootPath);
                            Rule matchedRule = ruleEngine.MatchRule(filterObj);
                            string key = string.Empty;
                            if (matchedRule != null)
                            {
                                key = stub.ScopeSettingId.ToString() + "_" + matchedRule.FSRule.Id;
                            }
                            else
                            {
                                key = stub.ScopeSettingId.ToString() + "_" + Guid.Empty.ToString();
                            }
                            termid = FSJobCache.Instance.AutoRuleIdTermIdMapping[key];
                        }

                        if (FSJobCache.Instance.Terms.TryGetValue(termid, out var tempTerm))
                        {
                            var termInvalid = false;
                            if (tempTerm == null || tempTerm.IsDeprecated || tempTerm.IsRemoved)
                            {
                                termInvalid = true;
                            }
                            if (tempTerm.TermExpirationFrom != 0 || tempTerm.TermExpirationTo != 0)
                            {
                                if (DateTime.UtcNow.Ticks < tempTerm.TermExpirationFrom || (tempTerm.TermExpirationTo != 0 && DateTime.UtcNow.Ticks > tempTerm.TermExpirationTo))
                                {
                                    termInvalid = true;
                                }
                            }
                            if (termInvalid)
                            {
                                logger.Warn("Term is invalid [{0}].", termid);
                                throw new Exception("RM_FS_DisposalDetail_TermIsInvalid" + I18NEntity.Separator + tempTerm.Name);
                            }
                            termName = tempTerm.Name;
                        }
                        else
                        {
                            logger.Warn("Cannot find the term with id [{0}] from the cache.", termid);
                        }
                    }
                    else
                    {
                        logger.Warn("There is no term setting for the ID:{0}", stub.ScopeSettingId);
                    }
                    record.TermId = termid == Guid.Empty && dbRecord != null && dbRecord.TermId != Guid.Empty ? dbRecord.TermId : termid;
                    record.TermName = string.IsNullOrWhiteSpace(termName) && dbRecord != null && !string.IsNullOrWhiteSpace(dbRecord.TermName) ? dbRecord.TermName : termName;
                    if(dbRecord == null || dbRecord.TermId != termid || !dbRecord.TermName.Equals(termName, StringComparison.OrdinalIgnoreCase))
                    {
                        record.HasTermChanged = true;
                    }
                }
                else
                {
                    record.TermId = dbRecord == null ? Guid.Empty : dbRecord.TermId;
                    record.TermName = dbRecord == null ? string.Empty : dbRecord.TermName;
                }
            }
        }
        private void AssembleRule(FileSystemRecordDto record, Stub stub, FileSystemRecordDto dbRecord)
        {
            using (new AgentPerformanceScope("FSObjectAnalyzer.AssembleRule", addToStatistics: true))
            {
                if (stub.Type == Stub.StubType.Folder)
                {
                    return;
                }
                record.RuleId = Guid.Empty;
                record.RuleLevel = 0;
                record.DisposalDueDate = "";
                Tuple<Rule, TimeSpan> result = MatchRule(record, stub.MediaObj);
                if (result != null)
                {
                    if (dbRecord != null && dbRecord.RuleId != null && !string.IsNullOrEmpty(result.Item1.Id) && new Guid(result.Item1.Id) != dbRecord.RuleId)
                    {
                        //FS数据换rule，重置Records Owner
                        record.RecordOwner = string.Empty;
                        record.HasRuleChanged = true;
                    }
                    record.RuleId = string.IsNullOrEmpty(result.Item1.Id) ? Guid.Empty : new Guid(result.Item1.Id);
                    record.RuleLevel = (int)result.Item1.PolicyLevel;
                    record.RuleName = result.Item1.Name;
                    record.DisposalDueDate = result.Item2 == default(TimeSpan) ? "RDM_RecordsExporer_Status_NextJob" : DateTime.UtcNow.Add(result.Item2).Ticks.ToString();
                    record.PreviosDisposalDueDate = result.Item2 == default(TimeSpan) ? "RDM_RecordsExporer_Status_NextJob" : DateTime.UtcNow.Add(result.Item2).Ticks.ToString();
                    //Hold状态Record重新计算Due Date;
                    if (dbRecord != null && dbRecord.HoldStatus && IsRemoveRule(result.Item1))
                    {
                        if (record.DisposalDueDate == "RDM_RecordsExporer_Status_NextJob")
                        {
                            record.DisposalDueDate = dbRecord.HoldReleaseTime.ToString();
                            record.PreviosDisposalDueDate = "RDM_RecordsExporer_Status_NextJob"; //dbRecord.HoldReleaseTime.ToString();
                        }
                        else
                        {
                            if (DateTime.UtcNow.Add(result.Item2).Ticks > dbRecord.HoldReleaseTime)
                            {
                                record.DisposalDueDate = DateTime.UtcNow.Add(result.Item2).Ticks.ToString();
                                record.PreviosDisposalDueDate = DateTime.UtcNow.Add(result.Item2).Ticks.ToString();
                            }
                            else
                            {
                                record.DisposalDueDate = dbRecord.HoldReleaseTime.ToString();
                                record.PreviosDisposalDueDate = dbRecord.HoldReleaseTime.ToString();
                            }
                        }
                    }
                }
                else
                {
                    //FS数据不符合rule，重置Records Owner
                    record.RecordOwner = string.Empty;
                }
            }
        }

        private bool IsRemoveRule(Rule rule)
        {
            if (rule.FSRule != null && rule.FSRule.spMoveOption != null && rule.FSRule.spMoveOption.MoveDestination != null)
            {
                return false;
            }
            return true;
        }
        private Tuple<Rule, TimeSpan> MatchRule(FileSystemRecordDto record, StorageInfo mediaObj)
        {
            Tuple<Rule, TimeSpan> matchedRule = default(Tuple<Rule, TimeSpan>);
            List<Rule> rules;
            if (FSJobCache.Instance.TermRuleMapping.TryGetValue(record.TermId, out rules))
            {
                var filteredRules = RuleUtil.FilterMoveRules(rules, record.DirPath).Where(x => x.FSRule != null).ToList();
                if (filteredRules.Count > 0)
                {
                    DisposalRuleEngine engine = new DisposalRuleEngine(filteredRules);
                    AvePoint.Common.FilterEngine.ObjectInfoBase filterObject = ObjectConverter.ConvertXObject2FilterObject(new XFileInfoEx(mediaObj), FSJobCache.Instance.RootPath);
                    matchedRule = engine.MatchPotentialRule(filterObject, true);
                }
                else
                {
                    logger.Debug($"Current Term[{record.TermId}] doesn't have FS rule so skip check rule.FSPath:{record.FullPath.LogBase64()}.");
                }
            }
            return matchedRule;
        }
        private FileSystemRecordDto GenerateTop2LevelRecord(Stub stub)
        {
            return new FileSystemRecordDto()
            {
                AveSiteId = Guid.Empty.ToString(),
                //CollectionTime = DateTime.UtcNow.Ticks,
                CreatedBy = string.Empty,
                DeclareAsRecord = false,
                DeclaredBy = string.Empty,
                DirPath = stub.FullPath,
                DisposalDueDate = "",
                ExtensionForFile = string.Empty,
                Extsion1 = string.Empty,
                FolderId = stub.SelfId,
                FullPath = stub.FullPath,
                HoldStatus = false,
                ItemId = stub.SelfId,
                ItemRowId = -1,
                LeafName = stub.FullPath,
                ListId = Guid.Empty,
                MetaInfo = string.Empty,
                NodeId = stub.SelfId,
                NodeType = stub.Type == Stub.StubType.ConnectionGroups ? (int)NodeLevel.FSConnectionGroups : (int)NodeLevel.FSConnectionGroup,
                ParentId = stub.ParentId,
                RecordHistory = string.Empty,
                RecordOwner = string.Empty,
                RecordsId = string.Empty,
                RelatedRecords = string.Empty,
                RelatedRecordsCount = 0,
                RuleId = Guid.Empty,
                ScopeId = Guid.Empty,
                SourceFlag = (int)SourceFlag.FileSystem,
                TermId = Guid.Empty,
                TermName = string.Empty,
                TimeCreated1 = DateTime.UtcNow,
                TimeLastModified = Convert.ToInt64(DateTime.UtcNow.Ticks),
                WebId = Guid.Empty,
                RecordStatus = 1,
                BulkImportEnabled = JobContext.Current.BulkImportEnabled,
                BulkSize = JobContext.Current.BulkSize,
                FSJobType = FSJobCache.Instance.JobController.JobType
            };
        }
    }
}
