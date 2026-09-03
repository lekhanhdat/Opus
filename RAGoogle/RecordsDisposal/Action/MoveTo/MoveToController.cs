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
using AvePoint.GCommon.Contract.Tree;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.ArchiverCommon;
using Newtonsoft.Json;
using RAGoogle.Archive.Common;
using RAGoogle.Common;
using RAGoogle.Extension;
using RAGoogle.Models;
using RAGoogle.RecordsDisposal.Action.Archive.Data;
using RAGoogle.Services;
using RAGoogle.Util;
using File = Google.Apis.Drive.v3.Data.File;

namespace RAGoogle.RecordsDisposal.Action.MoveTo
{
    internal class MoveToController(GoogleConfiguration configuration) : BaseBackupController(configuration)
    {
        #region properties

        private IRALogger _logger = RALogger.GetInstance(typeof(MoveToController));

        private GoogleDriveTreeNodeDto _driveLevelNode;

        private DestinationLocationInfo _destinationLocationObj;

        private IExplorerDao _explorerDao { get; set; }
        public IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new AvePoint.RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                }
                return _explorerDao;
            }
        }
        #endregion
        public override async Task ProcessArchiveReport(ArchiveApproveReport item, BackupNodeParameters nodeParameters)
        {
            mArchiveItem = item;
            if (item.JsonMeta.IsNotNullOrEmpty())
            {
                mGoogleItem = JsonConvert.DeserializeObject<GoogleItemData>(item.JsonMeta) ?? new();
            }
            if (mGoogleItem != null)
            {
                await Process(mGoogleItem);
            }
        }
        public override async Task Process(GoogleItemData item)
        {
            _logger.Info("Start move Google content to new location. ItemName: {0}", item.Name);
            if (item.Level != RMNodeLevel.GoogleFile)
            {
                _logger.Info("Ignore item because node level is different from google file. ItemName: {0}", item.Name);
                return;
            }
            try
            {
                using CheckJobStopScope jScope = new();
                _destinationLocationObj = mConfiguration.CurrentRule.GoogleDriveRule
                    .MoveToRecordCenterAndDelareSetting
                    .DestinationLocation;
                _logger.Info($"Start moving Google item name: {item.Name} from {mConfiguration.SelectedNode.Name} to {_destinationLocationObj.GoogleTreeNode.Name}");
                if (item.Level != AvePoint.RA.Contract.RMWeb.Tree.Base.RMNodeLevel.GoogleFile)
                {
                    _logger.Info("Only execute move action with document level. Skip this item: {0}", item.Name);
                }
                _driveLevelNode = GetDriveLevel(_destinationLocationObj.GoogleTreeNode);

                var destinationId = _destinationLocationObj.DestinationId;

                var reportCenter = mConfiguration.ReportCenter;
                var allowedToMove = ValidateDestinationBeforeMove(item);

                if (!allowedToMove) return;

                if (!reportCenter.CheckPermissionForDestinationDrive(_driveLevelNode.ObjectId))
                {
                    var (isNeedAssignPermission, email) = await CheckDestinationDrivePermissionAsync(mConfiguration.SelectedNode, _driveLevelNode);
                    reportCenter.AssignPermissionForDestinationDrive(isNeedAssignPermission, email, _driveLevelNode.ObjectId);
                }

                using GoogleDriveService googleService = await GetGoogleDriveServiceAsync(mConfiguration.SelectedNode, _driveLevelNode);
                mConfiguration.RecordManager.TryGetRecordValue(item.UniqueId, 0, out Record existRecord);
                var movedFile = await googleService.MoveToNewFolder(item.Id, item.ParentId, destinationId);
                var newDirPath = await GetMovedFileRealDirPath(googleService, movedFile);
                UpdateRecordDb(_driveLevelNode, movedFile, item, existRecord, newDirPath);

                item.AddToOtherSummaryReportsByGoogleItem(mConfiguration.ActionApproveReports, JobDetailsStatus.Successful, mConfiguration.CurrentRule.Name, string.Empty,  I18NResource.MoveAction);
            }
            catch (JobStopException)
            {
                _logger.Warn("The move action job has stopped.");
                throw new JobStopException("The move action job has stopped.");
            }
            catch (Exception ex)
            {
                string message = I18NResource.MoveItemFailed;
                if (ex.Message.Contains(I18NResource.InvalidUserPermission))
                {
                    message = I18NResource.InvalidUserPermission;
                }
                _logger.Info("Error occurred while processing move item '{0}' to new location. Inner exception: {1}", item.Name, ex.ToString());
                item.AddToOtherSummaryReportsByGoogleItem(mConfiguration.ActionApproveReports, JobDetailsStatus.Failed, mConfiguration.CurrentRule.Name, message,  I18NResource.MoveAction);
            }
        }

        private bool ValidateDestinationBeforeMove(GoogleItemData item)
        {
            item.DestinationPath = GetParentFolderPath(item.RelativePath);
            if (item.ParentId.EqualsIgnoreCase(_destinationLocationObj.DestinationId))
            {
                item.AddToOtherSummaryReportsByGoogleItem(mConfiguration.ActionApproveReports, JobDetailsStatus.Skipped, mConfiguration.CurrentRule.Name, I18NResource.MoveToSameDestination,  I18NResource.MoveAction);
                return false;
            }
            if (!item.TenantId.EqualsIgnoreCase(_driveLevelNode.TenantId))
            {
                item.AddToOtherSummaryReportsByGoogleItem(mConfiguration.ActionApproveReports, JobDetailsStatus.Skipped, mConfiguration.CurrentRule.Name, I18NResource.MoveToDifferentTenant,  I18NResource.MoveAction);
                return false;
            }
            return true;
        }

        private void UpdateRecordDb(GoogleDriveTreeNodeDto driveLevelRuleNode, File movedFile, GoogleItemData item, Record record, string dirPath)
        {
            try
            {
                item.DestinationPath = GetParentFolderPath(dirPath);
                var enableRecordSetting = (EnableRecordManagementSetting)mConfiguration.GoogleSetting.EnableRecordManagement;
                if (enableRecordSetting == EnableRecordManagementSetting.Enable)
                {
                    if (record != null)
                    {
                        var newRecord = mConfiguration.RecordManager.CreateRecordByCurrentNode(driveLevelRuleNode, movedFile, record, dirPath, item.TenantId, item.MemberEmail);
                        var (rule, term) = CalculateMatchedPotentialRule(item, driveLevelRuleNode, mConfiguration.GoogleSetting);
                        newRecord.TermId = term?.UniqueId ?? Guid.Empty;
                        newRecord.RuleId = rule?.Id != null ? new Guid(rule.Id) : Guid.Empty;
                        newRecord.TermName = term?.Name;
                        mConfiguration.RecordManager.AddNewRecord(newRecord);

                        if (newRecord.Id != item.UniqueId)
                        {
                            ExplorerDao.Delete(record.CreateDate, item.UniqueId);
                        }

                        return;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    _logger.Info("Record disable", item.Name);
                    mConfiguration.RecordManager.Delete(record);
                }
            }
            catch (Exception ex)
            {
                _logger.Info("Error occurred while processing update record to new location. Inner exception: {1}", item.Name, ex.ToString());
            }
        }

        private (Rule? rule, RMTerm? term) CalculateMatchedPotentialRule(GoogleItemData item, GoogleDriveTreeNodeDto selectedNode, RMGoogleSetting setting)
        {
            return mConfiguration.RuleManager.CalculateMatchedPotentialRule(mConfiguration.AppProfile, item, selectedNode, setting);
        }

        private void CopyRecordProperties(Record sourceRecord, Record destRecord)
        {
            destRecord.CollectTime = DateTime.UtcNow.Ticks;
            destRecord.CreateDate = sourceRecord.CreateDate;
            destRecord.DeclaredBy = sourceRecord.DeclaredBy;
            destRecord.DestroyedTime = sourceRecord.DestroyedTime;
            destRecord.DisposalDueDate = sourceRecord.DisposalDueDate;
            destRecord.ExtensionForFile = sourceRecord.ExtensionForFile;
            destRecord.Extsion1 = sourceRecord.Extsion1;
            destRecord.HoldBy = sourceRecord.HoldBy;
            destRecord.HoldId = sourceRecord.HoldId;
            destRecord.HoldReleaseTime = sourceRecord.HoldReleaseTime;
            destRecord.HoldStatus = sourceRecord.HoldStatus;
            destRecord.AppendHolds_Array = sourceRecord.AppendHolds_Array;
            destRecord.HoldByUsers = sourceRecord.HoldByUsers;
            destRecord.HoldUntilTimes = sourceRecord.HoldUntilTimes;
            destRecord.MetaInfo = sourceRecord.MetaInfo;
            destRecord.ModifiedBy = sourceRecord.ModifiedBy;
            destRecord.CreatedBy = sourceRecord.CreatedBy;
            destRecord.NodeType = sourceRecord.NodeType;
            destRecord.PredictTermId = sourceRecord.PredictTermId;
            destRecord.PredictTime = sourceRecord.PredictTime;
            destRecord.MLUnderReview = sourceRecord.MLUnderReview;
            destRecord.MLClassificationType = sourceRecord.MLClassificationType;
            destRecord.MLReviewer = sourceRecord.MLReviewer;
            destRecord.MLApprovalStatus = sourceRecord.MLApprovalStatus;
            destRecord.MLEscalateFrom = sourceRecord.MLEscalateFrom;
            destRecord.MLEscalatedComment = sourceRecord.MLEscalatedComment;
            destRecord.TrainingScope = sourceRecord.TrainingScope;
            destRecord.TrainingTermId = sourceRecord.TrainingTermId;
            destRecord.TrainingAddType = sourceRecord.TrainingAddType;
            destRecord.TrainingModelId = sourceRecord.TrainingModelId;
            destRecord.PredictTermScore = sourceRecord.PredictTermScore;

            destRecord.RecordOwner = sourceRecord.RecordOwner;
            destRecord.RecordsId = sourceRecord.RecordsId;
            destRecord.RecordStatus = sourceRecord.RecordStatus;
            destRecord.RelatedRecords = sourceRecord.RelatedRecords;
            destRecord.RelatedRecordsCount = sourceRecord.RelatedRecordsCount;
            destRecord.RuleId = sourceRecord.RuleId;
            destRecord.RuleLevel = sourceRecord.RuleLevel;
            destRecord.LeafName = sourceRecord.LeafName;
            destRecord.LeafName_Array = sourceRecord.LeafName_Array;
            destRecord.SourceFlag = sourceRecord.SourceFlag;
        }

        private async Task<string> GetDestinationUrlForMyDrive(GoogleDriveTreeNodeDto destTreeNode, GoogleDriveTreeNodeDto? selectedTreeNode = null)
        {
            if (selectedTreeNode == null)
            {
                if (destTreeNode.Level == NodeLevel.GoogleFolder)
                {
                    return destTreeNode.ID;
                }
                using var googleService = await GetGoogleDriveServiceAsync(destTreeNode);
                var rootFolderMyDrive = await googleService.GetRootFolderMyDriveAsync();
                return rootFolderMyDrive.Id;
            }

            var driveLevelNode = GetDriveLevel(destTreeNode);

            if (driveLevelNode.FullPath == selectedTreeNode.FullPath)
            {
                return await GetDestinationUrlForMyDrive(destTreeNode);
            }
            throw new NotSupportedException("Do not support move file between My Drive node");
        }

        private GoogleDriveTreeNodeDto GetDriveLevel(GoogleDriveTreeNodeDto treeNodeDto)
        {
            while (treeNodeDto is { Level: not (NodeLevel.GoogleMyDrive or NodeLevel.GoogleSharedDrive) })
            {
                treeNodeDto = treeNodeDto.Parent;
            }
            return treeNodeDto;
        }

        private async Task<string> GetMovedFileRealDirPath(GoogleDriveService service, File file)
        {
            string workspace = _driveLevelNode.Name;

            string fullPath = file.Name;
            while (file.Parents is { Count: > 0 })
            {
                string parentId = file.Parents[0];
                var parentFile = await service.GetFileByIdAsync(parentId);
                if (parentFile.Parents.IsNotNullOrEmpty())
                {
                    fullPath = parentFile.Name + "/" + fullPath;
                }
                file = parentFile;
            }

            if (fullPath.LastIndexOf('/') >= 0)
            {
                fullPath = $"{workspace}/" + fullPath;
            }
            else
            {
                fullPath = workspace;
            }

            return fullPath;
        }

        private string GetParentFolderPath(string path)
        {
            int lastSlashIndex = path.LastIndexOf("/") == -1
                ? path.LastIndexOf("\\")
                : path.LastIndexOf("/");

            if (lastSlashIndex > 0)
            {
                return path.Substring(0, lastSlashIndex);
            }

            return path;
        }
    }
}
