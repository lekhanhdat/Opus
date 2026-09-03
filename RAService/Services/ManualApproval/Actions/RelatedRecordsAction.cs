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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Service.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ManualApproval.Actions
{
    public class RelatedRecordsAction
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RelatedRecordsAction));

        private static IArchiverTableDao ArchiverTableDao => PlatformWindsorManager.GetService<IArchiverTableDao>();

        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private static readonly string ConnectString = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];

        private readonly AzureTableConnectContract ConnectContract;

        private readonly ManualApprovalRecordRepository Repository;

        private readonly RMAccount ActionAccount;

        private readonly bool _needSyncArchiverTable;

        private readonly bool _hasFSLiscense;

        private readonly bool _hasLSPLiscense;

        public RelatedRecordsAction(ManualApprovalRecordRepository repository)
        {
            _needSyncArchiverTable = !TenantService.IsNewOpusTenant();
            Repository = repository;
            if (_needSyncArchiverTable)
            {
                ConnectContract = new DAOAPIClientV1().GetArchiverDataBaseConfigAsync().Result;
            }
                var accountId = TenantLocalValue.LogonUserId;
            ActionAccount = AccountDao.Find(item => item.UserId == accountId && item.IsRemoved == 0);

            _hasFSLiscense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.FileSystem);
            _hasLSPLiscense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.SharePointOnPrem);
        }

        public async Task<ManualApprovalActionResult> ChangeDisposalAction(ManualApprovalRelatedRecordsDisposalDefinition definition)
        {
            try
            {
                var result = new ManualApprovalActionResult
                {
                    CompletedStatus = ActionCompletedStatus.Succeed
                };
                var items = await Repository.QueryItemsAsync(item => definition.ItemIds.Contains(item.Id));

                if (!_hasFSLiscense && items.Any(item => item.SourceFlag == (int)SourceFlag.FileSystem))
                {
                    result.CompletedStatus = ActionCompletedStatus.Failed;
                    return result;
                }

                if (!_hasLSPLiscense && items.Any(item => item.SourceFlag == (int)SourceFlag.SharePointOnPrem))
                {
                    result.CompletedStatus = ActionCompletedStatus.Failed;
                    return result;
                }

                await items.ForEachAsync(async item =>
                {
                    var itemActionResult = await ChangeItemDisposalActionAsync(item, definition.DisposalAction);
                    result.EffectItems.Add(itemActionResult);
                });

                await Repository.UpsertItemsAsync(items);
                return result;
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while change disposal action for items: [{string.Join(", ", definition.ItemIds)}]. Error: {e}");
                return new ManualApprovalActionResult
                {
                    CompletedStatus = ActionCompletedStatus.Failed,
                    Message = e.Message
                };
            }
        }

        private async Task<ManualApprovalItemActionResult> ChangeItemDisposalActionAsync(ManualApprovalRecord item, RelatedRecordOption disposalAction)
        {
            try
            {
                item.ManualRelatedRecordsAction = (int)disposalAction;
                if (_needSyncArchiverTable || (SourceFlag)item.SourceFlag == SourceFlag.SharePointOnPrem)
                {
                    switch ((SourceFlag)item.SourceFlag)
                    {
                        case SourceFlag.SharePoint:
                            await ArchiverTableDao.UpdateItemDisposalActionAsync(ConnectContract, TenantLocalValue.LogonGroupId, item.ManualPartitionKey, item.NodeId, disposalAction);
                            break;
                        case SourceFlag.SharePointOnPrem:
                            await ArchiverTableDao.UpdateItemDisposalActionAsync(ConnectString, TenantLocalValue.LogonGroupId, item.ManualPartitionKey, item.NodeId, disposalAction);
                            break;
                        case SourceFlag.FileSystem:
                            await ArchiverTableDao.UpdateItemDisposalActionForFSAsync(ConnectString, TenantLocalValue.LogonGroupId, item.ManualPartitionKey, item.ManualRowKey, disposalAction);
                            break;
                        case SourceFlag.Physical:
                            item.DeleteRelatedRecords = (int)disposalAction;
                            break;
                    }
                }else{
                    item.DeleteRelatedRecords = (int)disposalAction;
                }

                this.RebuildAudits(item);

                return new ManualApprovalItemActionResult
                {
                    IsSucceed = true,
                    EffectItemFullPath = item.SourceFlag >= 1000 ? item.LeafName : item.ManualFullPath
                };
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while change item: [{item.Id}] diposal action to [{disposalAction}].");
                return new ManualApprovalItemActionResult
                {
                    IsSucceed = false,
                    Message = e.Message,
                    OldValue = item.ManualRelatedRecordsAction,
                    EffectItemFullPath = item.SourceFlag >= 1000 ? item.LeafName : item.ManualFullPath
                };
            }
        }

        private void RebuildAudits(ManualApprovalRecord item)
        {
            var audits = new List<ReviewAudits>();
            if (!string.IsNullOrEmpty(item.ManualAudits))
            {
                audits = SerializerHelper.DeserializeFromXmlString<List<ReviewAudits>>(item.ManualAudits);
            }
            audits.Add(new ReviewAudits
            {
                ReviewTime = DateTime.UtcNow.Ticks.ToString(),
                ReviewBy = ActionAccount.DisplayName,
                Action = "RM_MA_ChangeAction",
            }) ;
            item.ManualAudits = SerializerHelper.SerializeToXmlString(audits);
        }
    }
}
