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
using AvePoint.GCommon.Contract.Tree;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.SharePoint.ArchiverCommon;
using Newtonsoft.Json;
using RAGoogle.Common;
using RAGoogle.GoogleObjDiscover;
using RAGoogle.Helper;
using RAGoogle.Models;
using RAGoogle.Models.Contract;
using RAGoogle.RecordsDisposal.Action.Archive.Data;
using RAGoogle.Services;
using RAGoogle.Util;

namespace RAGoogle.RecordsDisposal
{
    internal abstract class BaseBackupController
    {
        protected GoogleConfiguration mConfiguration;
        protected ArchiveApproveReport mArchiveItem { get; set; }
        protected GoogleItemData mGoogleItem { get; set; }
        protected GoogleDriveData mGoogleDriveInfo { get; set; }
        public BaseBackupController(GoogleConfiguration configuration)
        {
            mConfiguration = configuration;
            if (configuration.SelectedNode != null && configuration.SelectedNode.Level is NodeLevel.GoogleMyDrive or NodeLevel.GoogleSharedDrive)
            {
                mGoogleDriveInfo = ConvertHelper.ConvertDtoNodeTreeToData(configuration.SelectedNode, mConfiguration.AppProfile.TenantId);
            }
        }

        public abstract Task Process(GoogleItemData item);
        public abstract Task ProcessArchiveReport(ArchiveApproveReport item, BackupNodeParameters nodeParameters);

        protected void SaveDataToLiteDB(GoogleItemData item, Record? record)
        {
            GoogleDestructionData data = new GoogleDestructionData()
            {
                ScopeId = mConfiguration.SelectedNode.ID,
                ItemName = item.Name,
                Level = (int)item.Level,
                RuleId = mConfiguration.CurrentRule.Id,
                FullPath = item.RelativePath,
                TermId = mConfiguration.CurrentTerm?.Id.ToString() ?? string.Empty,
                DestroyedTime = DateTime.UtcNow.Ticks,
                MetaInfo = GetMetaInfo(item, record)
            };
            GoogleLiteDBWrapper.CreateInstance(GooglePathUtil.GetDisposalRecordDBPath(mConfiguration.JobId)).Insert(new List<GoogleDestructionData>() { data });
            mConfiguration.ReportCenter.IncreaseBaseProgress(1);
        }

        private string GetMetaInfo(GoogleItemData item, Record? record)
        {
            var metaInfo = new GoogleDestructionMetaData()
            {
                ItemId = item.Id,
                ItemName = item.Name,
                Level = (int)item.Level,
                ItemExtension = item.FileExtension,
                TermId = mConfiguration.CurrentTerm?.Id.ToString() ?? string.Empty,
                TermName = mConfiguration.CurrentTerm?.Name ?? string.Empty,
                CreatedBy = item.CreatedBy,
                CreatedTime = item.CreatedTime.Ticks,
                ModifiedBy = item.ModifiedBy,
                ModifiedTime = item.ModifiedTime.Ticks,
                ManualApprovedBy = record?.ManualApprovedBy ?? 0,
                ManualApprovedStatus = record?.ManualApprovedStatus ?? 0,
                ManualInternalApprovedStatus = record?.ManualInternalApprovedStatus ?? 0,
                ManualArchiveStatus = record?.ManualArchiveStatus ?? 0,
                MetaInfo = JsonConvert.SerializeObject(item.MetaInfo)
            };
            return JsonConvert.SerializeObject(metaInfo);
        }

        protected async Task<GoogleDriveService> GetGoogleDriveServiceAsync(GoogleDriveTreeNodeDto selectedNode, GoogleDriveTreeNodeDto ruleNode = null)
        {
            RMGoogleDiscoverBase discoverBase = new(null);
            discoverBase.Init(mConfiguration.AppProfile);
            string driveId = selectedNode.Level is NodeLevel.GoogleSharedDrive ? selectedNode.ObjectId : selectedNode.FullPath;
            if (ruleNode is { Level: NodeLevel.GoogleSharedDrive } && selectedNode.Level == NodeLevel.GoogleSharedDrive)
            {
                return await discoverBase.GetDriveService(driveId, ruleNode.ObjectId);
            }
            return await discoverBase.GetDriveService(driveId);
        }

        protected Task<(bool, string)> CheckDestinationDrivePermissionAsync(GoogleDriveTreeNodeDto selectedNode, GoogleDriveTreeNodeDto ruleNode)
        {
            RMGoogleDiscoverBase discoverBase = new(null);
            discoverBase.Init(mConfiguration.AppProfile);
            return discoverBase.CheckPermissionInDestinationDrive(selectedNode, ruleNode);
        }
    }
}
