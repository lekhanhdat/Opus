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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.Records.Core.Utilities.Extensions;
using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RACloudFS.FSImportJob
{
    public class FSImportSettingProcessor : FSImportSettingProcessorBase
    {
        public FSImportSettingProcessor(RMImportSPSettingJobMessage jobMsg)
        {
            logger = RALogger.GetInstance(typeof(FSImportSettingProcessor));
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
            JobStatus status = JobStatus.None;
            try
            {
                Dictionary<string, List<string[]>> fileDatas = GetFileDatas(FilePath);
                List<List<FSImportSettingObject>> settingList = GetImportSetting(fileDatas);
                var validateResult = await ValidateUNCPathsAsync(settingList);
                if (validateResult.Count != 0)
                {
                    foreach (var settingObjects in validateResult)
                    {
                        var connection = FSConnectionDao.GetConnectionByName(settingObjects[0].ConnectionName);
                        if (isContainsIllegalCharacters)
                        {
                            Result.HasFailed = true;
                            commomErrorMessage = illegalCharactersErrorMessage;
                        }
                        else
                        {
                            ReportManager.IncreaseBase(settingObjects.Count);
                            foreach (var settingObj in settingObjects)
                            {
                                await AddCustomSettingAsync(settingObj, connection);
                                ReportManager.Increase(1);
                            }
                        }
                    }
                }
                else
                {
                    Result.HasFailed = true;
                }
            }
            catch (Exception e)
            {
                Result.HasFailed = true;
                logger.Error($"ImportCustomSetting Error:{e}");
            }
            finally
            {
                status = Result.HasFailed
                    ? Result.HasSuccessful ? JobStatus.FinishWithException : JobStatus.Failed
                    : JobStatus.Finished;
                string jobComment = (status == JobStatus.FinishWithException || status == JobStatus.Failed)
                    ? commomErrorMessage
                    : string.Empty;
                ReportManager.SetJobFinished(status, jobComment);
                try
                {
                    System.IO.File.Delete(FilePath);
                }
                catch (Exception e)
                {
                    logger.Warn($"Delete csv error:{e}");
                }
                if (status != JobStatus.Failed && status != JobStatus.Stopped)
                    await MultiGeoDataCenterService.RunMainDCSyncCommonDataJob(JobRunBy.Control);
            }
        }

        private async Task<List<List<FSImportSettingObject>>> ValidateUNCPathsAsync(List<List<FSImportSettingObject>> settingObjectList)
        {
            List<List<FSImportSettingObject>> canImportSettings = new List<List<FSImportSettingObject>>();
            foreach (var settingObjects in settingObjectList)
            {
                try
                {
                    var scopeIdUNCPathDic = GetUNCPathDic(settingObjects);
                    var connection = FSConnectionDao.GetConnectionByName(settingObjects[0].ConnectionName);
                    var accessConnectionType = FSConnectionGroupDao.GetTypeByGroupId(connection.GroupId);
                    var agentIds = FSConnectionGroupWithAgentMemebershipDao.GetAgentIdByGroupId(connection.GroupId);
                    var resultList = await FileSystemBrowserService.ValidateUNCPathsAsync(scopeIdUNCPathDic, accessConnectionType, agentIds);
                    List<FSImportSettingObject> canImportSetting = new List<FSImportSettingObject>();
                    var failedList = scopeIdUNCPathDic.AsQueryable().Where(s => !resultList.Contains(s.Key)).Select(s => s.Value).ToList();
                    var successList = scopeIdUNCPathDic.AsQueryable().Where(s => resultList.Contains(s.Key)).Select(s => s.Key).ToList();
                    if (failedList.Count > 0)
                    {
                        foreach (var failed in failedList)
                        {
                            JMImportSPSettingDetail detail = new JMImportSPSettingDetail()
                            {
                                ObjectName = failed.Substring(failed.LastIndexOf(@"\") + 1),
                                Url = failed,
                                Status = JobDetailsStatus.Failed,
                                Comment = "RM_FS_ImportJob_UNCPathMsg",
                            };
                            ReportManager.SendJobDetail(detail);
                            Result.HasFailed = true;
                        }
                    }
                    if (successList.Count > 0)
                    {
                        foreach (var settingObject in settingObjects)
                        {
                            if (successList.Contains(settingObject.scopeId) && !settingObject.UNCPath.Contains("/"))
                            {
                                canImportSetting.Add(settingObject);
                            }
                            else if (settingObject.UNCPath.Contains("/"))
                            {
                                JMImportSPSettingDetail detail = new JMImportSPSettingDetail()
                                {
                                    ObjectName = settingObject.UNCPath.Substring(settingObject.UNCPath.LastIndexOf(@"\") + 1),
                                    Url = settingObject.UNCPath,
                                    Status = JobDetailsStatus.Failed,
                                    Comment = "RM_FS_ImportJob_UNCPathMsg",
                                };
                                ReportManager.SendJobDetail(detail);
                                Result.HasFailed = true;
                            }
                        }
                        canImportSettings.Add(canImportSetting);
                    }
                    continue;
                }
                catch (Exception e)
                {
                    logger.Error($"ValidateUNCPaths failed, error:{e}");
                    JMImportSPSettingDetail detail = new JMImportSPSettingDetail()
                    {
                        ObjectName = settingObjects[0].ConnectionName,
                        Status = JobDetailsStatus.Failed,
                        Comment = "RM_FS_ImportJob_UNCPathMsg",
                    };
                    Result.HasFailed = true;
                    ReportManager.SendJobDetail(detail);
                    continue;
                }
            }
            return canImportSettings;
        }

        private Dictionary<Guid, string> GetUNCPathDic(List<FSImportSettingObject> settingObjects)
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

        private async Task AddCustomSettingAsync(FSImportSettingObject settingObj, FSConnection connection)
        {
            JMImportSPSettingDetail detail = new JMImportSPSettingDetail()
            {
                Url = settingObj.UNCPath,
            };
            try
            {
                var termGroupId = GetTermGroupId(settingObj.TermGroup);
                var termSet = GetTermSet(termGroupId, settingObj.TermSet);
                RMTerm scopeTerm = new RMTerm();
                RMTerm defaultTerm = new RMTerm();
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
                int parentId = scopeTerm == null ? termSet.Id : scopeTerm.Id;
                string defaultCacheKey = parentId + settingObj.DefaultTermPath;
                if (DefaulTermCache.ContainsKey(defaultCacheKey))
                {
                    defaultTerm.UniqueId = DefaulTermCache.GetValue(defaultCacheKey).UniqueId;
                    defaultTerm.Name = DefaulTermCache.GetValue(defaultCacheKey).Name;
                }
                else
                {
                    defaultTerm = GetDefaultTerm(termSet, parentId, settingObj.DefaultTermPath, scopeTerm == null);
                    if (defaultTerm != null)
                    {
                        DefaulTermCache.Add(defaultCacheKey, new TermCache { ID = defaultTerm.Id, UniqueId = defaultTerm.UniqueId, Name = defaultTerm.Name });
                    }
                }
                var inheritSetting = LoadInheritSeting(settingObj.UNCPath, connection);
                if (inheritSetting == null)
                {
                    Result.HasFailed = true;
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = "RM_FS_Import_InheritFailed";
                    logger.Info("Current Node inherit setting is null");
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
                var groupSetting = LoadGroupSetting(connection);
                var curNode = CreateNode(connection, inheritSetting, groupSetting, settingObj, termSet, scopeTerm, defaultTerm, workflowDef, approvalType, userInfos);
                detail.ObjectName = curNode.Name;
                if (DeactiveUNCPath.Contains(settingObj.UNCPath) || DeactiveUNCPath.Any(d => settingObj.UNCPath.Contains(d + @"\")))
                {
                    Result.HasFailed = true;
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = "RM_FS_DisposalDeactiveFolder_JobFailed";
                    logger.Info("Current Node is Deactive");
                    return;
                }
                await ValidateTermPermissionAsync(TenantLocalValue.LogonUserId, termSet, settingObj.TermGroup, termGroupId, SourceFlag.FileSystem);
                await FileSystemSettingsService.SaveFSNodeSettingAsync(curNode);
                detail.Status = JobDetailsStatus.Successful;
                Result.HasSuccessful = true;
                logger.Info($"Finish processing {settingObj.FullUrl}");
            }
            catch (Exception e)
            {
                Result.HasFailed = true;
                detail.Status = JobDetailsStatus.Failed;
                detail.ObjectName = settingObj.UNCPath.Substring(settingObj.UNCPath.LastIndexOf(@"\") + 1);
                detail.Comment = e.Message;
                logger.Error($"Import Custom Setting Error:{e}");
            }
            finally
            {
                ReportManager.SendJobDetail(detail);
            }
        }

        private RMFSTreeNode CreateNode(FSConnection connection, RMFileSystemSetting inheritSetting, RMFileSystemSetting groupSetting,
            FSImportSettingObject settingObj, RMTermSet termSet, RMTerm scopeTerm, RMTerm defaultTerm,
            RMWorkflowDefinition workflow, int approvalType, List<ToUserInfo> userInfos)
        {
            var spObjectId = settingObj.UNCPath.ToLower().ToMd5();
            RMFSTreeNode inheritNode = SerializerHelper.DeserializeByDataContractSerializer<RMFSTreeNode>(inheritSetting.NodeInfo);
            var title = settingObj.UNCPath.Substring(settingObj.UNCPath.LastIndexOf('\\') + 1);
            var curNode = ConstructTreeNode(inheritNode, settingObj, title, NodeLevel.FSFolder, spObjectId, termSet, scopeTerm, defaultTerm, workflow, approvalType, userInfos, inheritSetting.WorkflowReferenceId);
            CreateParentNodes(inheritNode, groupSetting, connection, ref curNode);
            if (curNode.RecordOwner == null)
            {
                curNode.RecordOwner = new List<ToUserInfo>();
            }
            SetDoclevelSetting(ref curNode, termSet, scopeTerm, defaultTerm, settingObj, workflow, approvalType, userInfos, settingObj.IsSendEmail);
            return curNode;
        }

        private RMFSTreeNode ConstructTreeNode(RMFSTreeNode inheritNode, FSImportSettingObject settingObj, string title,
            NodeLevel level, Guid spObjectId, RMTermSet termSet, RMTerm scopeTerm, RMTerm defaultTerm,
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

            SetDoclevelSetting(ref currentNode, termSet, scopeTerm, defaultTerm, settingObj, workflow, approvalType, userInfos, settingObj.IsSendEmail);
            return currentNode;
        }

        private void SetDoclevelSetting(ref RMFSTreeNode node, RMTermSet termSet, RMTerm scopeTerm, RMTerm defaultTerm,
            FSImportSettingObject settingObj, RMWorkflowDefinition workflow, int approvalType, List<ToUserInfo> userInfos, bool isSendEmail)
        {
            node.Name = settingObj.UNCPath.Substring(settingObj.UNCPath.LastIndexOf(@"\") + 1);
            node.Id = settingObj.scopeId;
            node.TermSetId = termSet.UniqueId;
            node.TermSetName = termSet.Name;
            node.TermId = scopeTerm != null ? scopeTerm.UniqueId : Guid.Empty;
            node.TermName = scopeTerm != null ? scopeTerm.Name : string.Empty;
            node.DefaultTermId = defaultTerm.UniqueId;
            node.DefaultTermName = defaultTerm.Name;
            node.DeployTermMethod = (int)DeployTermMethod.UseDefaultTerm;
            node.NeedCheckDefaultValue = settingObj.ApplyExisting;
            node.FullPath = settingObj.UNCPath;
            if (settingObj.ApplyExisting)
            {
                node.ApplyExistType = settingObj.IsOverwrite ? (int)ApplyExistingTermType.OverWrite : (int)ApplyExistingTermType.SkipAndKeep;
            }
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

        private Dictionary<string, List<string[]>> GetFileDatas(string path)
        {
            Dictionary<string, List<string[]>> dataList = new Dictionary<string, List<string[]>>();
            Dictionary<string, List<string[]>> sheets = new();
            int index = 1;
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    sheets = ExcelUtil.ReadExcelForFS(fs);
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
                        sheets = ExcelUtil.ReadExcelForFS(fs);
                    }
                }
            }

            if (!sheets.Any())
            {
                return new Dictionary<string, List<string[]>>();
            }

            foreach (var sheet in sheets)
            {
                var connection = FSConnectionDao.GetConnectionByName(sheet.Value[0][1]);
                if (connection == null)
                {
                    JMImportSPSettingDetail detail = new JMImportSPSettingDetail()
                    {
                        ObjectName = sheet.Value[0][1],
                        Status = JobDetailsStatus.Failed,
                        Comment = string.Format(I18NEntity.GetString("RM_FS_ImportJob_ConnectMsg"), index),
                    };
                    Result.HasFailed = true;
                    ReportManager.SendJobDetail(detail);
                    index++;
                    continue;
                }
                if (dataList.Keys.Contains(connection.UNCPath))
                {
                    JMImportSPSettingDetail detail = new JMImportSPSettingDetail()
                    {
                        ObjectName = sheet.Value[0][1],
                        Status = JobDetailsStatus.Skipped,
                        Comment = string.Format(I18NEntity.GetString("RM_FS_ImportJob_ConnectionExitsMsg"), index),
                    };
                    ReportManager.SendJobDetail(detail);
                    index++;
                    continue;
                }
                List<string[]> canCovertItems = ValidateFormat(sheet.Value, index, connection);
                if (canCovertItems.Count != 0)
                {
                    dataList.Add(connection.UNCPath, canCovertItems);
                }
                index++;
            }
            return dataList;
        }

        private List<string[]> ValidateFormat(List<string[]> list, int index, FSConnection connection)
        {
            List<string[]> successList = new List<string[]>();
            for (int count = 0; count < list.Count; count++)
            {
                bool flag = true;
                if (count == 0) { successList.Add(list[count]); continue; }
                if (count == 1) { continue; }
                if (string.IsNullOrEmpty(list[count][0]) && string.IsNullOrEmpty(list[count][1]) && string.IsNullOrEmpty(list[count][2]) && string.IsNullOrEmpty(list[count][3]) && string.IsNullOrEmpty(list[count][4]))
                {
                    continue;
                }
                if (string.IsNullOrEmpty(list[count][0]) || list[count][0].Contains("\t"))
                {
                    JMImportSPSettingDetail detail = new JMImportSPSettingDetail()
                    {
                        ObjectName = connection.Name,
                        Url = connection.UNCPath,
                        Status = JobDetailsStatus.Failed,
                        Comment = string.Format(I18NEntity.GetString("RM_FS_ImportJob_UNCPathFormatMsg"), index, count + 1, 1),
                    };
                    flag = false;
                    ReportManager.SendJobDetail(detail);
                    continue;
                }
                if (string.IsNullOrEmpty(list[count][1]) || list[count][1].Contains("\t") || !list[count][1].Contains('|'))
                {
                    JMImportSPSettingDetail detail = new JMImportSPSettingDetail()
                    {
                        ObjectName = list[count][0],
                        Url = connection.UNCPath + "\\" + list[count][0],
                        Status = JobDetailsStatus.Failed,
                        Comment = string.Format(I18NEntity.GetString("RM_FS_ImportJob_TermScopeFormatMsg"), index, count + 1, 2),
                    };
                    flag = false;
                    ReportManager.SendJobDetail(detail);
                }
                if (string.IsNullOrEmpty(list[count][2]) || list[count][2].Contains("\t"))
                {
                    JMImportSPSettingDetail detail = new JMImportSPSettingDetail()
                    {
                        ObjectName = list[count][0],
                        Url = connection.UNCPath + "\\" + list[count][0],
                        Status = JobDetailsStatus.Failed,
                        Comment = string.Format(I18NEntity.GetString("RM_JS_BCM_ImportSetting_NotSupportManual"), index, count + 1, 3),
                    };
                    flag = false;
                    ReportManager.SendJobDetail(detail);
                }
                if (string.IsNullOrEmpty(list[count][3])) { list[count][3] = "false"; }
                else if (!bool.TryParse(list[count][3], out _))
                {
                    JMImportSPSettingDetail detail = new JMImportSPSettingDetail()
                    {
                        ObjectName = list[count][0],
                        Url = connection.UNCPath + "\\" + list[count][0],
                        Status = JobDetailsStatus.Failed,
                        Comment = string.Format(I18NEntity.GetString("RM_FS_ImportJob_ApplyExistFormatMsg"), index, count + 1, 4),
                    };
                    flag = false;
                    ReportManager.SendJobDetail(detail);
                }
                if (string.IsNullOrEmpty(list[count][4])) { list[count][4] = "false"; }
                else if (!bool.TryParse(list[count][4], out _))
                {
                    JMImportSPSettingDetail detail = new JMImportSPSettingDetail()
                    {
                        ObjectName = list[count][0],
                        Url = connection.UNCPath + "\\" + list[count][0],
                        Status = JobDetailsStatus.Failed,
                        Comment = string.Format(I18NEntity.GetString("RM_FS_ImportJob_OverExistFormatMsg"), index, count + 1, 5),
                    };
                    flag = false;
                    ReportManager.SendJobDetail(detail);
                }
                if (string.IsNullOrEmpty(list[count][5])) { list[count][5] = "0"; }
                else
                {
                    switch (list[count][5].ToLowerInvariant())
                    {
                        case NoManualSetting: list[count][5] = "0"; break;
                        case WorkflowProcess: list[count][5] = "1"; break;
                        case RecordOwner: list[count][5] = "2"; break;
                        default: list[count][5] = "0"; break;
                    }
                }
                if (string.IsNullOrEmpty(list[count][7])) { list[count][7] = "false"; }
                else if (!bool.TryParse(list[count][7], out _))
                {
                    JMImportSPSettingDetail detail = new JMImportSPSettingDetail()
                    {
                        ObjectName = list[count][0],
                        Url = connection.UNCPath + "\\" + list[count][0],
                        Status = JobDetailsStatus.Failed,
                        Comment = string.Format(I18NEntity.GetString("RM_FS_ImportJob_OverExistFormatMsg"), index, count + 1, 7),
                    };
                    flag = false;
                    ReportManager.SendJobDetail(detail);
                }
                if (flag) { successList.Add(list[count]); }
                else { Result.HasFailed = true; }
            }
            return successList;
        }

        private List<List<FSImportSettingObject>> GetImportSetting(Dictionary<string, List<string[]>> dataList)
        {
            List<List<FSImportSettingObject>> settingObjects = new List<List<FSImportSettingObject>>();
            foreach (var datas in dataList)
            {
                List<FSImportSettingObject> settingList = new List<FSImportSettingObject>();
                List<string> uncPathList = new List<string>();
                string connectionName = datas.Value[0][1];
                for (int index = 1; index < datas.Value.Count; index++)
                {
                    var setting = ConvertToFSSettingObject(datas.Value[index], datas.Key, connectionName);
                    string uncPath = setting.UNCPath;
                    if (!uncPathList.Contains(uncPath))
                    {
                        uncPathList.Add(uncPath);
                        settingList.Add(setting);
                    }
                    else
                    {
                        string objectName = datas.Value[index][0];
                        JMImportSPSettingDetail detail = new JMImportSPSettingDetail()
                        {
                            ObjectName = objectName,
                            Url = uncPath,
                            Status = JobDetailsStatus.Skipped,
                            Comment = string.Format(I18NEntity.GetString("RM_FS_ImportJob_UNCPathExitsMsg"), objectName),
                        };
                        ReportManager.SendJobDetail(detail);
                    }
                }
                if (settingList.Count != 0)
                {
                    settingObjects.Add(settingList);
                }
            }
            return settingObjects;
        }

        private FSImportSettingObject ConvertToFSSettingObject(string[] data, string connectionUNCPath, string connectionName)
        {
            string uncPath = connectionUNCPath + @"\" + data[0];
            string orginPath = @"\" + uncPath.Replace(@"\\", @"\");
            Guid scopeId = orginPath.ToLowerInvariant().ToMd5();
            FSImportSettingObject obj = new FSImportSettingObject()
            {
                ConnectionName = connectionName,
                UNCPath = orginPath,
                TermScopePath = data[1],
                DefaultTermPath = data[2],
                ApplyExisting = GetBoolColumnValue(data.ElementAtOrDefault(3)),
                IsOverwrite = GetBoolColumnValue(data.ElementAtOrDefault(4)),
                scopeId = scopeId,
                ApprovalType = int.Parse(data[5]),
                WorkflowName = data[6],
                IsSendEmail = GetBoolColumnValue(data.ElementAtOrDefault(7)),
                SettingLevel = SettingLevel.None,
            };
            string[] names = obj.TermScopePath.Split(PathSeparator);
            obj.TermGroup = names[0];
            obj.TermSet = names[1];
            if (names.Length > 2)
            {
                obj.TermScopeRelativePath = obj.TermScopePath.Substring(obj.TermScopePath.IndexOf(obj.TermSet) + obj.TermSet.Length + 1);
            }
            return obj;
        }

        private async Task ValidateTermPermissionAsync(string userId, RMTermSet termSet, string termGroupName, Guid termGroupId, SourceFlag sourceFlag)
        {
            var filterOption = new FilterTermObjOption
            {
                NeedCheckPermission = true,
                FilterByContentSource = true,
                ExcludeBuiltIn = true,
                ForPhysicalView = true,
                SourceFlag = sourceFlag,
                ContainerId = null,
            };
            var permissionTermGroup = await DoesUserHasPermisionToTermAsync(userId, SecurityTermLevel.TermGroup, [termGroupId], filterOption);
            if (!permissionTermGroup)
            {
                logger.Error($"Current user does not have permission for term group. Name:[{termGroupName}]");
                throw new Exception("RM_JS_BCM_ImportSetting_NoTermGroupPermission");
            }
            var permissionTermSet = await DoesUserHasPermisionToTermAsync(userId, SecurityTermLevel.TermSet, [termSet.UniqueId], filterOption);
            if (!permissionTermSet)
            {
                logger.Error($"Current user does not have permission for term set. Name:[{termSet.Name}]");
                throw new Exception("RM_JS_BCM_ImportSetting_NoTermSetPermission");
            }
        }

        private async Task<bool> DoesUserHasPermisionToTermAsync(string userId, SecurityTermLevel level, List<Guid> termObjIds, FilterTermObjOption filterOption)
        {
            var hasPermission = false;
            try
            {
                if (termObjIds != null && termObjIds.Count > 0)
                {
                    var userAndGroupIds = await UserSerive.GetUserAndGroupUserIdsAsync(userId);
                    QuerySecurityTermObjDto dto = new QuerySecurityTermObjDto
                    {
                        Level = level,
                        UserAndGroupIds = userAndGroupIds,
                        FilterByContentSource = filterOption.FilterByContentSource,
                        ExcludeBuiltIn = filterOption.ExcludeBuiltIn,
                        ContainerId = filterOption.ContainerId,
                        SourceFlag = filterOption.SourceFlag,
                        ForPhysicalView = filterOption.ForPhysicalView,
                    };
                    hasPermission = RMSecurityGroupDao.DoesUserHasPermisionToTerm(termObjIds, dto);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error checking term permission for user, termObjId:{string.Join(";", termObjIds)}, level:{level}, message:{ex}");
            }
            return hasPermission;
        }
    }

    public class FSImportSettingObject : IFSImportSettingBase
    {
        #region csv column
        public string ConnectionName { get; set; }
        public string UNCPath { get; set; }
        public string TermScopePath { get; set; }
        public string DefaultTermPath { get; set; }
        public bool ApplyExisting { get; set; }
        public bool IsOverwrite { get; set; }
        public string WorkflowName { get; set; }
        public int ApprovalType { get; set; }
        public bool IsSendEmail { get; set; }
        #endregion

        #region computed properties
        public Guid scopeId { get; set; }
        public string TermGroup { get; set; }
        public string TermSet { get; set; }
        public string TermScopeRelativePath { get; set; }
        public SettingLevel SettingLevel { get; set; }
        public string FullUrl { get; set; }
        public Guid ConnectionId { get; set; }
        #endregion
    }

    public enum SettingLevel
    {
        None = 0,
        SiteCollection = 1,
        RootWeb = 2,
        SubWeb = 3,
        List = 4,
        Folder = 5
    }

    public enum ApplyExistingTermType
    {
        None = 0,
        OverWrite = 1,
        SkipAndKeep = 2
    }
}