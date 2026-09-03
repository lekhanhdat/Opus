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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.FileSystemRegister.JPMC;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.Records.Core.Utilities.Extensions;
using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RACloudFS.FSImportJob
{
    public class FSImportSettingProcessorJPMC : FSImportSettingProcessorBase
    {
        private static readonly string[] ExpectedDateFormats = {
            "MM/dd/yyyy", "M/d/yyyy", "MM/d/yyyy", "M/dd/yyyy", "yyyy-MM-dd",
            "M/d/yyyy h:mm:ss tt", "MM/dd/yyyy hh:mm:ss tt", "MM/dd/yyyy h:mm:ss tt", "M/dd/yyyy h:mm:ss tt",
            "M/d/yyyy H:mm:ss", "M/d/yyyy HH:mm:ss",
            "MM/dd/yyyy H:mm:ss", "MM/dd/yyyy HH:mm:ss",
            "M/dd/yyyy H:mm:ss",
            "yyyy-MM-dd H:mm:ss", "yyyy-MM-dd HH:mm:ss",
        };

        private Dictionary<string, List<ClassCodeCascadeDataDto>> cachedCascadeData = new Dictionary<string, List<ClassCodeCascadeDataDto>>();

        public FSImportSettingProcessorJPMC(RMImportSPSettingJobMessage jobMsg)
        {
            logger = RALogger.GetInstance(typeof(FSImportSettingProcessorJPMC));
            ReportMangerFactory.Instance.Init(jobMsg.JobID, jobMsg.JobType);
            Result = new JobResult();
            try
            {
                FilePath = JobReportUtility.GetImportJobCSVFile(jobMsg.CSVPath);
            }
            catch (Exception e)
            {
                logger.Error("Cannot download file, error:{0}", e.ToString());
                throw;
            }
            DeactiveUNCPath = FileSystemSettingDao.GetAllDeactiveId();
            ReportManager.IncreaseBase(10);
            ReportManager.StartUpdateJobProgress();
        }

        public override async Task RunAsync()
        {
            try
            {
                Dictionary<Guid, List<Tuple<Guid, string[]>>> fileDatas = GetFileDatas(FilePath);
                Dictionary<Guid, List<FSImportSettingJPMCObject>> importSettingDic = GetImportSetting(fileDatas);
                var groupMappingObjectSetting = await ValidateUNCPathsAsync(importSettingDic);

                if (groupMappingObjectSetting.Count == 0 || groupMappingObjectSetting.All(group => group.Value.Count == 0))
                    throw new Exception("RM_FS_ImportJob_NoAvailableConnection");

                foreach(var group in groupMappingObjectSetting)
                {
                    var settingObjects = group.Value;
                    logger.Info($"Start processing connection group [{group.Key}] with {settingObjects.Count} valid settings.");
                    ReportManager.IncreaseBase(settingObjects.Count);
                    foreach (var settingObj in settingObjects)
                    {
                        await AddCustomSettingAsync(settingObj, group.Key);
                        ReportManager.Increase(1);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"Import job failed with exception: {e}");
                Result.HasFailed = true;
                if (e.Message == "RM_FS_ImportJob_NoAvailableConnection")
                {
                    commomErrorMessage = e.Message;
                }
            }
            finally
            {
                try
                {
                    File.Delete(FilePath);
                }
                catch (Exception e)
                {
                    logger.Error($"Failed to delete csv file: {FilePath}, error: {e}");
                }
                var (status, comment) = CalculateJobStatusAndComment();
                ReportManager.SetJobFinished(status, comment);
            }
        }

        private (JobStatus, string) CalculateJobStatusAndComment()
        {
            var status = (Result.HasSuccessful, Result.HasFailed) switch
            {
                (true, true) => JobStatus.FinishWithException,
                (false, true) => JobStatus.Failed,
                (true, false) => JobStatus.Finished,
                _ => JobStatus.None
            };

            string jobComment = (status == JobStatus.FinishWithException || status == JobStatus.Failed) ? commomErrorMessage : string.Empty;

            return (status, jobComment);
        }

        private async Task<Dictionary<Guid, List<FSImportSettingJPMCObject>>> ValidateUNCPathsAsync(Dictionary<Guid, List<FSImportSettingJPMCObject>> groupMappingObjectSetting)
        {
            Dictionary<Guid, List<FSImportSettingJPMCObject>> validNodeSettings = new Dictionary<Guid, List<FSImportSettingJPMCObject>>();

            foreach (var group in groupMappingObjectSetting)
            {
                var connectionGroup = FSConnectionGroupDao.GetGroupById(group.Key);
                try
                {
                    var connectionSettings = group.Value;
                    var scopeIdUNCPathDic = GetUNCPathDic(connectionSettings);
                    var agentIds = FSConnectionGroupWithAgentMemebershipDao.GetAgentIdByGroupId(connectionGroup.Id);
                    var resultList = await FileSystemBrowserService.ValidateUNCPathsAsync(scopeIdUNCPathDic, connectionGroup.AccessConnectionType, agentIds);
                    List<FSImportSettingJPMCObject> targetSettings = new List<FSImportSettingJPMCObject>();
                    var failedList = scopeIdUNCPathDic.AsQueryable().Where(s => !resultList.Contains(s.Key)).Select(s => s.Value).ToList();
                    var successList = scopeIdUNCPathDic.AsQueryable().Where(s => resultList.Contains(s.Key)).Select(s => s.Key).ToList();
                    if (failedList.Count > 0)
                    {
                        foreach (var failed in failedList)
                        {
                            AddReportDetail(new JMImportSPSettingDetail()
                            {
                                ObjectName = failed.Substring(failed.LastIndexOf(@"\") + 1),
                                Url = failed,
                                Status = JobDetailsStatus.Failed,
                                Comment = "RM_FS_ImportJob_PathMsg",
                            });
                        }
                    }
                    if (successList.Count > 0)
                    {
                        foreach (var settingObject in connectionSettings)
                        {
                            if (successList.Contains(settingObject.scopeId) && !settingObject.UNCPath.Contains("/"))
                            {
                                targetSettings.Add(settingObject);
                            }
                            else if (settingObject.UNCPath.Contains("/"))
                            {
                                AddReportDetail(new JMImportSPSettingDetail()
                                {
                                    ObjectName = settingObject.ConnectionName,
                                    Url = settingObject.UNCPath,
                                    Status = JobDetailsStatus.Failed,
                                    Comment = "RM_FS_ImportJob_PathMsg",
                                });
                            }
                        }
                        if (!validNodeSettings.ContainsKey(group.Key))
                        {
                            validNodeSettings[group.Key] = targetSettings;
                        }
                    }
                    continue;
                }
                catch (Exception e)
                {
                    logger.Error($"Error validating UNC paths for connection group [{group.Key}]: {e}");
                    AddReportDetail(new JMImportSPSettingDetail()
                    {
                        ObjectName = connectionGroup.Name,
                        Status = JobDetailsStatus.Failed,
                        Comment = "RM_TS_SS_Summary",
                    });
                    continue;
                }
            }

            return validNodeSettings;
        }

        private Dictionary<Guid, string> GetUNCPathDic(List<FSImportSettingJPMCObject> settingObjects)
        {
            Dictionary<Guid, string> scopeIdUNCPathDic = new Dictionary<Guid, string>();
            foreach (var settingObj in settingObjects)
            {
                if (!scopeIdUNCPathDic.ContainsKey(settingObj.scopeId))
                {
                    scopeIdUNCPathDic.Add(settingObj.scopeId, settingObj.UNCPath);
                }
            }
            return scopeIdUNCPathDic;
        }

        private async Task AddCustomSettingAsync(FSImportSettingJPMCObject settingObj, Guid connectionGroupId)
        {
            JMImportSPSettingDetail detail = new JMImportSPSettingDetail()
            {
                ObjectName = settingObj.ConnectionName,
                Url = settingObj.UNCPath,
            };
            try
            {
                if(ExplorerService.HasJPMCConnectionRecord(settingObj.ConnectionId.ToString()))
                {
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = "RM_FS_ImportJob_Connection_ExistProcessed";
                    logger.Warn($"Connection [{settingObj.ConnectionName}] already has processed data sync job before, skipping import.");
                    return;
                }

                if (!settingObj.EnableIL)
                {
                    await SaveDisabledILSettingAsync(settingObj, connectionGroupId, detail);
                    return;
                }

                var termGroupId = GetTermGroupId(settingObj.TermGroup);
                var termSet = GetTermSet(termGroupId, settingObj.TermSet);

                RMTerm scopeTerm = new RMTerm();
                if (TermScopeCache.ContainsKey(settingObj.TermScopePath))
                {
                    scopeTerm.Id = TermScopeCache.GetValue(settingObj.TermScopePath).ID;
                    scopeTerm.UniqueId = TermScopeCache.GetValue(settingObj.TermScopePath).UniqueId;
                    scopeTerm.Name = TermScopeCache.GetValue(settingObj.TermScopePath).Name;
                }
                else
                {
                    scopeTerm = GetScopeTerm(termSet, settingObj.TermScopeRelativePath);
                    if (scopeTerm != null)
                    {
                        TermScopeCache.Add(settingObj.TermScopePath, new TermCache { ID = scopeTerm.Id, UniqueId = scopeTerm.UniqueId, Name = scopeTerm.Name });
                    }
                }

                int classCodeParentId = scopeTerm == null ? termSet.Id : scopeTerm.Id;
                RMTerm classCodeTerm = new RMTerm();
                string classCodeCacheKey = classCodeParentId + settingObj.ClassCode;
                if (DefaulTermCache.ContainsKey(classCodeCacheKey))
                {
                    classCodeTerm.Id = DefaulTermCache.GetValue(classCodeCacheKey).ID;
                    classCodeTerm.UniqueId = DefaulTermCache.GetValue(classCodeCacheKey).UniqueId;
                    classCodeTerm.Name = DefaulTermCache.GetValue(classCodeCacheKey).Name;
                }
                else
                {
                    classCodeTerm = GetDefaultTerm(termSet, classCodeParentId, settingObj.ClassCode, scopeTerm == null);
                    if (classCodeTerm != null)
                    {
                        DefaulTermCache.Add(classCodeCacheKey, new TermCache { ID = classCodeTerm.Id, UniqueId = classCodeTerm.UniqueId, Name = classCodeTerm.Name });
                    }
                }

                var inheritSetting = LoadGroupSetting(connectionGroupId);
                if (inheritSetting == null)
                {
                    detail.ObjectName = settingObj.ConnectionName;
                    detail.Url = settingObj.UNCPath;
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = "RM_FS_Import_InheritFailed";
                    logger.Warn($"Group [{connectionGroupId}] has not setting to inherit. Name: {settingObj.ConnectionName}");
                    return;
                }
                if (!IsSameTermGroup(inheritSetting.TermSetId, termSet))
                {
                    Result.HasFailed = true;
                    detail.ObjectName = settingObj.UNCPath.Substring(settingObj.UNCPath.LastIndexOf(@"\") + 1);
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = "RM_JS_BCM_ImportFSSetting_DifferentTermGroup";
                    logger.Error($"Current term group is not same with inherit setting term group. URL: {settingObj.UNCPath}");
                    return;
                }

                var approvalType = settingObj.ApprovalType;
                var workflowDef = VerifyManualWorkflow(settingObj);
                var userInfos = await VerifyManualRecordOwnerAsync(settingObj);
                var curNode = CreateNodeForJPMC(inheritSetting, settingObj, termSet, scopeTerm, classCodeTerm, workflowDef, approvalType, userInfos);
                detail.ObjectName = curNode.Name;
                await FileSystemSettingsService.SaveFSNodeSettingAsync(curNode);
                detail.Status = JobDetailsStatus.Successful;
            }
            catch (Exception e)
            {
                Result.HasFailed = true;
                detail.Status = JobDetailsStatus.Failed;
                detail.ObjectName = settingObj.ConnectionName;
                detail.Url = settingObj.UNCPath;
                detail.Comment = e.Message;
                logger.Error($"Import Custom Setting Error:{e}");
            }
            finally
            {
                AddReportDetail(detail);
                logger.Info($"Finish processing {settingObj.FullUrl}");
            }
        }

        private RMFSTreeNode CreateNodeForJPMC(RMFileSystemSetting inheritSetting,
            FSImportSettingJPMCObject settingObj, RMTermSet termSet, RMTerm scopeTerm, RMTerm classCodeTerm,
            RMWorkflowDefinition workflow, int approvalType, List<ToUserInfo> userInfos)
        {
            var spObjectId = settingObj.ConnectionId;
            RMFSTreeNode inheritNode = SerializerHelper.DeserializeByDataContractSerializer<RMFSTreeNode>(inheritSetting.NodeInfo);
            var title = settingObj.ConnectionName;
            var curNode = ConstructTreeNodeForJPMC(inheritNode, settingObj, title, NodeLevel.Site, spObjectId, termSet, scopeTerm, classCodeTerm, workflow, approvalType, userInfos, inheritSetting.WorkflowReferenceId);
            CreateParentNode(inheritNode, ref curNode);
            if (curNode.RecordOwner == null)
            {
                curNode.RecordOwner = new List<ToUserInfo>();
            }
            SetDoclevelSettingForJPMC(ref curNode, settingObj, termSet, scopeTerm, classCodeTerm, workflow, approvalType, userInfos, settingObj.IsSendEmail);
            return curNode;
        }

        private RMFSTreeNode CreateNodeForJPMCDisabledIL(RMFileSystemSetting inheritSetting, FSImportSettingJPMCObject settingObj)
        {
            RMFSTreeNode inheritNode = SerializerHelper.DeserializeByDataContractSerializer<RMFSTreeNode>(inheritSetting.NodeInfo);
            RMFSTreeNode curNode = new RMFSTreeNode();
            #region inherited properties
            curNode.ConnGroupId = inheritNode.ConnGroupId;
            curNode.EnableRelatedRecords = inheritNode.EnableRelatedRecords;
            curNode.isEnableClassification = inheritNode.isEnableClassification;
            curNode.TermNameOfContainer = inheritNode.TermNameOfContainer;
            curNode.TermIdOfContainer = inheritNode.TermIdOfContainer;
            curNode.EMailToRecordOwner = inheritNode.EMailToRecordOwner;
            curNode.ApprovalType = inheritNode.ApprovalType;
            curNode.WorkflowReferenceId = inheritSetting.WorkflowReferenceId;
            curNode.RecordOwner = inheritNode.RecordOwner ?? new List<ToUserInfo>();
            #endregion
            curNode.Level = (int)NodeLevel.SiteCollection;
            curNode.Id = settingObj.ConnectionId;
            curNode.Name = settingObj.ConnectionName;
            curNode.FullPath = settingObj.UNCPath;
            curNode.IsActive = true;
            curNode.EnableRecordManagement = (int)EnableRecordManagementSetting.Disable;

            CreateParentNode(inheritNode, ref curNode);
            return curNode;
        }

        private async Task SaveDisabledILSettingAsync(FSImportSettingJPMCObject settingObj, Guid connectionGroupId, JMImportSPSettingDetail detail)
        {
            var ownSetting = FileSystemSettingDao.LoadFSSetting(settingObj.scopeId, connectionGroupId);
            if (ownSetting != null)
            {
                ownSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.Disable;
                if (!string.IsNullOrEmpty(ownSetting.NodeInfo))
                {
                    var existingNode = SerializerHelper.DeserializeByDataContractSerializer<RMFSTreeNode>(ownSetting.NodeInfo);
                    existingNode.EnableRecordManagement = (int)EnableRecordManagementSetting.Disable;
                    ownSetting.NodeInfo = SerializerHelper.SerializeByDataContractSerializer(existingNode);
                }
                await FileSystemSettingDao.AddOrUpdateFSSettingAsync(ownSetting);
                logger.Info($"Updated EnableRecordManagement to disabled for existing setting: [{settingObj.UNCPath}]");
            }
            else
            {
                var inheritSetting = LoadGroupSetting(connectionGroupId);
                if (inheritSetting == null)
                {
                    detail.ObjectName = settingObj.ConnectionName;
                    detail.Url = settingObj.UNCPath;
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = "RM_FS_Import_InheritFailed";
                    logger.Info("Current Node inherit setting is null");
                    return;
                }
                var curNode = CreateNodeForJPMCDisabledIL(inheritSetting, settingObj);
                await FileSystemSettingsService.SaveFSNodeSettingAsync(curNode);
                logger.Info($"Created new setting with disabled IL for: [{settingObj.UNCPath}]");
            }

            detail.Status = JobDetailsStatus.Successful;
            logger.Info($"Finish processing {settingObj.UNCPath} with disabled IL");
        }

        private RMFSTreeNode ConstructTreeNodeForJPMC(RMFSTreeNode inheritNode, FSImportSettingJPMCObject settingObj, string title,
            NodeLevel level, Guid spObjectId, RMTermSet termSet, RMTerm scopeTerm, RMTerm classCodeTerm,
            RMWorkflowDefinition workflow, int approvalType, List<ToUserInfo> userInfos, string inheritWorkflowId)
        {
            RMFSTreeNode currentNode = new RMFSTreeNode();
            #region inherited properties
            currentNode.ConnGroupId = inheritNode.ConnGroupId;
            currentNode.EnableRelatedRecords = inheritNode.EnableRelatedRecords;
            currentNode.isEnableClassification = inheritNode.isEnableClassification;
            currentNode.TermNameOfContainer = inheritNode.TermNameOfContainer;
            currentNode.TermIdOfContainer = inheritNode.TermIdOfContainer;
            currentNode.EMailToRecordOwner = inheritNode.EMailToRecordOwner;
            currentNode.ApprovalType = inheritNode.ApprovalType;
            currentNode.WorkflowReferenceId = inheritWorkflowId;
            currentNode.RecordOwner = inheritNode.RecordOwner;
            #endregion

            currentNode.FullPath = settingObj.UNCPath;
            currentNode.Level = (int)level;
            currentNode.Id = spObjectId;
            currentNode.Name = title;
            currentNode.IsActive = true;

            SetDoclevelSettingForJPMC(ref currentNode, settingObj, termSet, scopeTerm, classCodeTerm, workflow, approvalType, userInfos, settingObj.IsSendEmail);
            return currentNode;
        }

        private void SetDoclevelSettingForJPMC(ref RMFSTreeNode node, FSImportSettingJPMCObject settingObj, RMTermSet termSet, RMTerm scopeTerm, RMTerm classCodeTerm, RMWorkflowDefinition workflow, int approvalType, List<ToUserInfo> userInfos, bool isSendEmail)
        {
            node.Name = settingObj.UNCPath.Substring(settingObj.UNCPath.LastIndexOf(@"\") + 1);
            node.Id = settingObj.scopeId;
            node.FullPath = settingObj.UNCPath;
            node.ApplyExistDocument = settingObj.ApplyExistDocument;
            node.TermSetId = termSet.UniqueId;
            node.TermSetName = termSet.Name;
            node.TermId = scopeTerm != null ? scopeTerm.UniqueId : Guid.Empty;
            node.TermName = scopeTerm != null ? scopeTerm.Name : string.Empty;
            node.DeployTermMethod = (int)DeployTermMethod.UseDefaultTerm;
            node.DefaultTermId = classCodeTerm != null ? classCodeTerm.UniqueId : Guid.Empty;
            node.DefaultTermName = classCodeTerm != null ? classCodeTerm.Name : settingObj.ClassCode;
            node.EnableRecordManagement = settingObj.EnableIL ? 1 : 2;
            node.IsAllowUserDownloadRCCReport = settingObj.AllowDownloadRCCReport;
            node.ApplyExistType = settingObj.EffectScope != (int)EffectScopeType.None ? (int)ApplyExistingTermType.OverWrite : (int)ApplyExistingTermType.SkipAndKeep;
            node.ClassCode = new FSClassCodeDto
            {
                ClassCodeId = settingObj.ClassCode,
                CountryCode = settingObj.CountryCode,
                RetentionType = settingObj.RetentionType,
                RetentionDate = settingObj.StartDateTicks > 0 ? settingObj.StartDateTicks.ToString() : string.Empty,
                ApplyExistDocuments =  settingObj.ApplyExistDocument
            };
            if (approvalType != 3)
            {
                node.ApprovalType = approvalType;
                if (approvalType == 1 && workflow.ReferenceId != Guid.Empty)
                {
                    node.WorkflowReferenceId = workflow.ReferenceId.ToString();
                    node.EMailToRecordOwner = isSendEmail;
                }
                else if (approvalType == 2 && userInfos.Count != 0)
                {
                    node.RecordOwner = userInfos;
                    node.EMailToRecordOwner = isSendEmail;
                }
            }
        }

        private static bool ParseEffectScopeType(string effectScope)
        {
            return effectScope.ToLowerInvariant() switch
            {
                "apply to the selected node itself and all its child nodes" => true,
                "only apply to selected node itself" => false,
                _ => false
            };
        }

        private Dictionary<Guid, List<Tuple<Guid,string[]>>> GetFileDatas(string path)
        {
            Dictionary<Guid, List<Tuple<Guid, string[]>>> groupAndRowDataMapping = new Dictionary<Guid, List<Tuple<Guid, string[]>>>();
            Dictionary<string, List<string[]>> sheets = new();
            int sheetIndex = 1;
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    sheets = ExcelUtil.ReadExcelForFSJPMC(fs);
                }
            }
            catch (OpenXmlPackageException e)
            {
                logger.Error(e.Message, e);
                if (e.ToString().Contains("Invalid Hyperlink") || e.ToString().Contains("Invalid URI"))
                {
                    using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        UriFixer.FixInvalidUri(fs, brokenUri => UriFixer.FixUri(brokenUri));
                    }
                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        sheets = ExcelUtil.ReadExcelForFSJPMC(fs);
                    }
                }
            }

            if (!sheets.Any())
            {
                return new Dictionary<Guid, List<Tuple<Guid, string[]>>>();
            }

            foreach (var sheet in sheets)
            {
                var groupName = sheet.Value[0][1];
                var connGroup = FSConnectionGroupDao.Find(g => g.Name.Equals(groupName));
                if (string.IsNullOrWhiteSpace(groupName) || connGroup == null)
                {
                    AddReportDetail(new JMImportSPSettingDetail()
                    {
                        ObjectName = groupName,
                        Url = groupName,
                        Status = JobDetailsStatus.Failed,
                        Comment = "RM_FS_ImportJob_InvalidConnectionGroup",
                    });
                    continue;
                }
                var connectionGroupId = connGroup.Id;

                for (int index = 2; index < sheet.Value.Count; index++)
                {
                    var row = sheet.Value[index];
                    if (IsRowEmpty(row))
                    {
                        continue;
                    }

                    string connectionName = row.ElementAtOrDefault(0) ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(connectionName))
                    {
                        logger.Warn($"Sheet [{sheetIndex}] row [{index + 1}] skipped: Connection Name is empty.");
                        continue;
                    }

                    var connection = FSConnectionDao.GetConnectionByName(connectionName);
                    if (connection == null)
                    {
                        AddReportDetail(new JMImportSPSettingDetail()
                        {
                            ObjectName = connectionName,
                            Url = connectionName,
                            Status = JobDetailsStatus.Failed,
                            Comment = string.Format(I18NEntity.GetString("RM_FS_ImportJob_ConnectMsg"), connectionName),
                        });
                        continue;
                    }

                    if (!groupAndRowDataMapping.ContainsKey(connectionGroupId))
                    {
                        groupAndRowDataMapping[connectionGroupId] = new List<Tuple<Guid, string[]>>();
                    }
                    groupAndRowDataMapping[connectionGroupId].Add(new Tuple<Guid, string[]>(connection.Id, row));
                }
                sheetIndex++;
            }

            Dictionary<Guid, List<Tuple<Guid, string[]>>> validatedDataList = new Dictionary<Guid, List<Tuple<Guid, string[]>>>();
            int groupIndex = 1;
            foreach (var group in groupAndRowDataMapping)
            {
                int rowIndex = 3;
                validatedDataList[group.Key] = group.Value.Select(groupData =>
                {
                    string[] validated = ValidateFormatForJPMC(groupData.Item2, groupIndex, rowIndex);
                    rowIndex++;
                    if (validated == null) return null;
                    return new Tuple<Guid, string[]>(groupData.Item1, validated);
                }).Where(validRow => validRow != null).ToList();
                groupIndex++;
            }
            return validatedDataList;
        }

        /// <summary>
        /// Columns: [0] Connection Name, [1] Full UNC Path, [2] Enable IL, [3] Allow Download RCC Report,
        ///          [4] Class Code Scope, [5] Class Code, [6] Country Code, [7] Retention Type,
        ///          [8] Start Date, [9] Effect Scope, [10] Manual Approval Type,
        ///          [11] Reviewer/Process Name, [12] Send Email Notification
        /// </summary>
        private string[] ValidateFormatForJPMC(string[] dataRow, int sheetIndex, int rowIndex)
        {
            if (IsRowEmpty(dataRow)) return null;
            var row = dataRow;

            var connectionName = row.ElementAtOrDefault(0) ?? string.Empty;
            var connectionUNCPath = row.ElementAtOrDefault(1) ?? string.Empty;

            // [1] Full UNC Path
            if (string.IsNullOrEmpty(row[1]) || row[1].Contains("\t"))
            {
                logger.Warn($"Row [{rowIndex}] rejected: Path is empty or invalid.");
                ReportValidationError(connectionName, connectionUNCPath, "RM_FS_ImportJob_PathFormatMsg", sheetIndex, rowIndex, 2);
                Result.HasFailed = true;
                return null;
            }
            string uncUrl = row[1];

            // [2] Whether Enable Record Management
            if (string.IsNullOrEmpty(row[2])) { row[2] = "false"; }
            else if (!bool.TryParse(row[2], out _))
            {
                logger.Warn($"Row [{rowIndex}] rejected: Enable Record Management has invalid value [{row[2]}].");
                ReportValidationError(row[1], uncUrl, "RM_FS_ImportJob_EnableILFormatMsg", sheetIndex, rowIndex, 3);
                Result.HasFailed = true;
                return null;
            }

            if (!GetBoolColumnValue(row[2]))
            {
                return row;
            }

            // [3] Whether Allow Information Owner To Download RCC Report
            if (string.IsNullOrEmpty(row[3])) { row[3] = "false"; }
            else if (!bool.TryParse(row[3], out _))
            {
                logger.Warn($"Row [{rowIndex}] rejected: Allow Download RCC Report has invalid value [{row[3]}].");
                ReportValidationError(row[1], uncUrl, "RM_FS_ImportJob_AllowDownloadRCCFormatMsg", sheetIndex, rowIndex, 4);
                Result.HasFailed = true;
                return null;
            }

            // [4] Class Code Scope
            if (string.IsNullOrEmpty(row[4]) || row[4].Contains("\t") || !row[4].Contains('|'))
            {
                logger.Warn($"Row [{rowIndex}] rejected: Class Code Scope is empty or has invalid format [{row[4]}].");
                ReportValidationError(row[1], uncUrl, "RM_FS_ImportJob_ClassCodeScopeFormatMsg", sheetIndex, rowIndex, 5);
                Result.HasFailed = true;
                return null;
            }

            // [5] Class Code
            if (string.IsNullOrEmpty(row[5]) || row[5].Contains("\t"))
            {
                logger.Warn($"Row [{rowIndex}] rejected: Class Code is empty or invalid.");
                ReportValidationError(row[1], uncUrl, "RM_FS_ImportJob_ClassCodeFormatMsg", sheetIndex, rowIndex, 6);
                Result.HasFailed = true;
                return null;
            }

            // [6] Country Code
            if (string.IsNullOrEmpty(row[6]) || row[6].Contains("\t"))
            {
                logger.Warn($"Row [{rowIndex}] rejected: Country Code is empty or invalid.");
                ReportValidationError(row[1], uncUrl, "RM_FS_ImportJob_CountryCodeFormatMsg", sheetIndex, rowIndex, 7);
                Result.HasFailed = true;
                return null;
            }

            if (!ValidateClassCodeRule(row, uncUrl, sheetIndex, rowIndex))
            {
                Result.HasFailed = true;
                return null;
            }

            // [7] Retention Type
            if (!ValidateRetentionType(row, uncUrl, sheetIndex, rowIndex, true))
            {
                Result.HasFailed = true;
                return null;
            }

            // [8] Start Date
            if (!ValidateStartDate(row, uncUrl, sheetIndex, rowIndex, true))
            {
                Result.HasFailed = true;
                return null;
            }

            // [9] Effect Scope
            if (string.IsNullOrEmpty(row[9]))
            {
                row[9] = string.Empty;
            }
            else if (row[9].Contains("\t"))
            {
                logger.Warn($"Row [{rowIndex}] rejected: Effect Scope has invalid value [{row[9]}].");
                ReportValidationError(row[1], uncUrl, "RM_FS_ImportJob_EffectScopeFormatMsg", sheetIndex, rowIndex, 10);
                Result.HasFailed = true;
                return null;
            }

            // [10] Manual Approval Type
            if (string.IsNullOrEmpty(row[10])) { row[10] = "0"; }
            else
            {
                switch (row[10].ToLowerInvariant())
                {
                    case NoManualSetting: row[10] = "0"; break;
                    case WorkflowProcess: row[10] = "1"; break;
                    case RecordOwner: row[10] = "2"; break;
                    default:
                        logger.Warn($"Row [{rowIndex}] rejected: Manual Approval Type has unknown value [{row[10]}].");
                        ReportValidationError(row[1], uncUrl, "RM_FS_ImportJob_ApprovalTypeFormatMsg", sheetIndex, rowIndex, 11);
                        Result.HasFailed = true;
                        return null;
                }
            }

            // [11] Record Reviewer/Process Name
            if (int.Parse(row[10]) > 0)
            {
                string reviewerName = row.ElementAtOrDefault(11);
                if (string.IsNullOrEmpty(reviewerName))
                {
                    logger.Warn($"Row [{rowIndex}] rejected: Reviewer/Process Name is required for approval type [{row[10]}].");
                    ReportValidationError(row[1], uncUrl, "RM_FS_ImportJob_ReviewerNameFormatMsg", sheetIndex, rowIndex, 12);
                    Result.HasFailed = true;
                    return null;
                }
            }

            // [12] Send Email Notification
            if (row.Length > 12)
            {
                if (string.IsNullOrEmpty(row[12])) { row[12] = "false"; }
                else if (!bool.TryParse(row[12], out _))
                {
                    logger.Warn($"Row [{rowIndex}] rejected: Send Email Notification has invalid value [{row[12]}].");
                    ReportValidationError(row[1], uncUrl, "RM_FS_ImportJob_SendEmailFormatMsg", sheetIndex, rowIndex, 13);
                    Result.HasFailed = true;
                    return null;
                }
            }

            return row;
        }

        private bool ValidateRetentionType(string[] row, string uncUrl, int index, int count, bool currentFlag)
        {
            if (string.IsNullOrWhiteSpace(row[7]))
            {
                row[7] = "Flat";
            }
            var retentionTypeLower = row[7].Trim().ToLowerInvariant();
            if (retentionTypeLower != "flat" && retentionTypeLower != "event")
            {
                ReportValidationError(row[1], uncUrl, "RM_FS_ImportJob_RetentionTypeFormatMsg", index, count, 8);
                return false;
            }
            return currentFlag;
        }

        private bool ValidateStartDate(string[] row, string uncUrl, int index, int count, bool currentFlag)
        {
            var retentionTypeLower = (row[7] ?? string.Empty).Trim().ToLowerInvariant();
            if (retentionTypeLower == "flat")
            {
                row[8] = string.Empty;
                return currentFlag;
            }
            string startDateRaw = (row[8] ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(startDateRaw))
            {
                logger.Error($"Start date is required for event retention type. Path: [{row[1]}]");
                ReportValidationError(row[1], uncUrl, "RM_FS_ImportJob_StartDateFormatMsg", index, count, 9);
                return false;
            }
            if (!TryParseAnyDate(startDateRaw, out DateTime _))
            {
                logger.Error($"Start date [{row[8]}] has unrecognized format for path: [{row[1]}]");
                ReportValidationError(row[1], uncUrl, "RM_FS_ImportJob_StartDateFormatMsg", index, count, 9);
                return false;
            }
            return currentFlag;
        }

        private bool ValidateClassCodeRule(string[] row, string uncUrl, int index, int count)
        {
            try
            {
                var classCode = row[5].Trim();
                var countryCode = row[6].Trim();
                var scopeParts = row[4].Split(PathSeparator);
                if (scopeParts.Length < 2)
                {
                    logger.Error($"Invalid class code scope format in column 4: [{row[4]}]");
                    ReportValidationError(row[1], uncUrl, "RM_FS_ImportJob_ClassCodeScopeFormatMsg", index, count, 5);
                    return false;
                }

                Guid termGroupId;
                RMTermSet termSet;
                try
                {
                    termGroupId = GetTermGroupId(scopeParts[0]);
                    termSet = GetTermSet(termGroupId, scopeParts[1]);
                }
                catch (Exception ex)
                {
                    logger.Error($"Class code scope not found: [{row[4]}]. Error: {ex}");
                    ReportValidationError(row[1], uncUrl, "RM_FS_ImportJob_ClassCodeScopeNotFoundMsg", index, count, 5);
                    return false;
                }

                string termSetKey = termSet.UniqueId.ToString();
                if (!cachedCascadeData.ContainsKey(termSetKey))
                {
                    var data = TaxonomyService.GetClassCodeCascadeDataAsync(new CurrentSettingsInfo { TermSetId = termSetKey }).GetAwaiter().GetResult();
                    cachedCascadeData[termSetKey] = data;
                }

                var cascadeData = cachedCascadeData[termSetKey];
                var matchedClassCode = cascadeData.FirstOrDefault(c => string.Equals(c.ClassCode, classCode, StringComparison.OrdinalIgnoreCase));
                if (matchedClassCode == null)
                {
                    logger.Error($"Class code not found: [{classCode}] in term set: [{termSet.Name}]");
                    ReportValidationError(row[1], uncUrl, "RM_FS_ImportJob_ClassCodeRuleNotFoundMsg", index, count, 6);
                    return false;
                }

                if (matchedClassCode.CountryCode == null || string.IsNullOrEmpty(countryCode))
                {
                    logger.Error($"Country code [{countryCode}] is empty or class code has no valid country codes");
                    ReportValidationError(row[1], uncUrl, "RM_FS_ImportJob_CountryCodeRuleNotFoundMsg", index, count, 7);
                    return false;
                }

                string matchedCountryCode = matchedClassCode.CountryCode.FirstOrDefault(cc => string.Equals(cc, countryCode, StringComparison.OrdinalIgnoreCase));
                if (matchedCountryCode == null)
                {
                    logger.Error($"Country code [{countryCode}] is not valid for class code [{classCode}]");
                    ReportValidationError(row[1], uncUrl, "RM_FS_ImportJob_CountryCodeRuleNotFoundMsg", index, count, 7);
                    return false;
                }

                row[5] = matchedClassCode.ClassCode;
                row[6] = matchedCountryCode;
                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to validate class code rule. Error: {ex}");
                ReportValidationError(row[1], uncUrl, "RM_FS_ImportJob_ClassCodeRuleNotFoundMsg", index, count, 6);
                return false;
            }
        }

        private Dictionary<Guid, List<FSImportSettingJPMCObject>> GetImportSetting(Dictionary<Guid, List<Tuple<Guid, string[]>>> groupMappingDataRow)
        {
            Dictionary<Guid, List<FSImportSettingJPMCObject>> groupMappingObjectSetting = new Dictionary<Guid, List<FSImportSettingJPMCObject>>();
            foreach (var group in groupMappingDataRow)
            {
                if (!groupMappingObjectSetting.ContainsKey(group.Key))
                {
                    groupMappingObjectSetting.Add(group.Key, new List<FSImportSettingJPMCObject>());
                }

                HashSet<string> uncPathList = new HashSet<string>();
                foreach (var groupData in group.Value)
                {
                    var (_, dataRow) = groupData;
                    var setting = ConvertToFSSettingObjectForJPMC(groupData);
                    string uncPath = setting.UNCPath;
                    if (!uncPathList.Contains(uncPath))
                    {
                        uncPathList.Add(uncPath);
                        groupMappingObjectSetting[group.Key].Add(setting);
                        continue;
                    }
                    AddReportDetail(new JMImportSPSettingDetail()
                    {
                        ObjectName = dataRow[0],
                        Url = uncPath,
                        Status = JobDetailsStatus.Skipped,
                        Comment = string.Format(I18NEntity.GetString("RM_FS_ImportJob_PathMsg"), dataRow[1]),
                    });
                }
            }
            return groupMappingObjectSetting;
        }

        /// <summary>
        /// Columns: [0] Connection Name, [1] Full UNC Path, [2] Enable IL, [3] Allow Download RCC Report,
        ///          [4] Class Code Scope, [5] Class Code, [6] Country Code, [7] Retention Type,
        ///          [8] Start Date, [9] Effect Scope, [10] Approval Type, [11] Reviewer/Process,
        ///          [12] Send Email Notification
        /// </summary>
        private FSImportSettingJPMCObject ConvertToFSSettingObjectForJPMC(Tuple<Guid, string[]> groupData)
        {
            var (connectionId, data) = groupData;
            var connectionName = data[0] ?? string.Empty;
            string rawPath = (data[1] ?? string.Empty).Trim();
            string orginPath = @"\" + rawPath.Replace(@"\\", @"\");
            Guid scopeId = connectionId;
            bool enableIL = GetBoolColumnValue(data.ElementAtOrDefault(2));
            if (!enableIL)
            {
                return new FSImportSettingJPMCObject()
                {
                    ConnectionId = connectionId,
                    ConnectionName = connectionName,
                    UNCPath = orginPath,
                    scopeId = scopeId,
                    EnableIL = false,
                    SettingLevelJPMC = SettingLevelJPMC.None,
                };
            }

            var retentionType = ParseRetentionType(data[7]);
            FSImportSettingJPMCObject obj = new FSImportSettingJPMCObject()
            {
                ConnectionId = connectionId,
                ConnectionName = connectionName,
                UNCPath = orginPath,
                scopeId = scopeId,
                EnableIL = true,
                AllowDownloadRCCReport = GetBoolColumnValue(data.ElementAtOrDefault(3)),
                TermScopePath = data[4],
                ClassCode = data[5].Trim(),
                CountryCode = data[6].Trim(),
                RetentionType = retentionType,
                StartDateTicks = ParseStartDateTicks(data[8], retentionType),
                ApplyExistDocument = ParseEffectScopeType(data.ElementAtOrDefault(9) ?? string.Empty),
                EffectScope = string.IsNullOrWhiteSpace(data.ElementAtOrDefault(9) ?? string.Empty) ? (int)EffectScopeType.None
                    : (int)EffectScopeType.AllUnderSelectedNode,
                IsOverwrite = true,
                ApprovalType = int.TryParse(data[10], out int parsedApproval) ? parsedApproval : 0,
                WorkflowName = data.ElementAtOrDefault(11) ?? string.Empty,
                IsSendEmail = GetBoolColumnValue(data.ElementAtOrDefault(12)),
                SettingLevelJPMC = SettingLevelJPMC.None,
            };

            string[] names = obj.TermScopePath.Split(PathSeparator);
            obj.TermGroup = names[0];
            obj.TermSet = names.Length > 1 ? names[1] : string.Empty;
            if (names.Length > 2)
            {
                obj.TermScopeRelativePath = obj.TermScopePath.Substring(obj.TermScopePath.IndexOf(obj.TermSet) + obj.TermSet.Length + 1);
            }
            return obj;
        }

        private RetentionScheduleType ParseRetentionType(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Trim().Equals("flat", StringComparison.OrdinalIgnoreCase))
            {
                return RetentionScheduleType.Flat;
            }
            return value.Trim().Equals("event", StringComparison.OrdinalIgnoreCase)
                ? RetentionScheduleType.Event
                : RetentionScheduleType.Flat;
        }

        private long ParseStartDateTicks(string value, RetentionScheduleType retentionType)
        {
            if (retentionType == RetentionScheduleType.Flat || string.IsNullOrEmpty(value))
            {
                return 0;
            }
            return TryParseAnyDate(value.Trim(), out DateTime parsedDate) ? parsedDate.Ticks : 0;
        }

        private bool TryParseAnyDate(string input, out DateTime result)
        {
            if (DateTime.TryParseExact(input, ExpectedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return true;
            }
            if (double.TryParse(input, out double serialDate))
            {
                try
                {
                    result = DateTime.FromOADate(serialDate);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        }

        private void AddReportDetail(JMImportSPSettingDetail detail)
        {
            ReportManager.SendJobDetail(detail);
            if(detail.Status == JobDetailsStatus.Failed)
            {
                Result.HasFailed = true;
                return;
            }
            Result.HasSuccessful = true;
        }

        protected override bool IsConnectionRootPath(string parentFullPath, FSConnection connection)
        {
            string normalizedParent = @"\" + parentFullPath.Replace(@"\\", @"\").TrimStart('\\');
            string normalizedConn = @"\" + connection.UNCPath.Replace(@"\\", @"\").TrimStart('\\');
            return normalizedConn.Equals(normalizedParent, StringComparison.OrdinalIgnoreCase);
        }
    }

    public class FSImportSettingJPMCObject : IFSImportSettingBase
    {
        #region csv column
        public string ConnectionName { get; set; }
        public string UNCPath { get; set; }
        public string TermScopePath { get; set; }
        public string DefaultTermPath { get; set; }
        public int EffectScope { get; set; }
        public bool ApplyExistDocument { get; set; }
        public bool IsOverwrite { get; set; }
        public string WorkflowName { get; set; }
        public int ApprovalType { get; set; }
        public bool IsSendEmail { get; set; }
        #endregion

        #region JPMC class code columns
        public string ClassCode { get; set; }
        public string CountryCode { get; set; }
        public RetentionScheduleType RetentionType { get; set; }
        public long StartDateTicks { get; set; }
        public bool EnableIL { get; set; }
        public bool AllowDownloadRCCReport { get; set; }
        #endregion

        #region computed properties
        public Guid ConnectionId { get; set; }
        public Guid scopeId { get; set; }
        public string TermGroup { get; set; }
        public string TermSet { get; set; }
        public string TermScopeRelativePath { get; set; }
        public SettingLevelJPMC SettingLevelJPMC { get; set; }
        public string FullUrl { get; set; }
        #endregion
    }

    public enum SettingLevelJPMC
    {
        None = 0,
        SiteCollection = 1,
        RootWeb = 2,
        SubWeb = 3,
        List = 4,
        Folder = 5
    }

    public enum EffectScopeType
    {
        None = 0,
        OnlySelectedNode = 1,
        AllUnderSelectedNode = 2
    }
}