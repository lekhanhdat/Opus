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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.Service.Services.ControlPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ManualApproval.Actions
{
    public class ExtendAction
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ExtendAction));

        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private readonly ManualApprovalRecordRepository Repository;

        private readonly RMAccount ActionAccount;

        private readonly bool _hasFSLiscense;

        private readonly bool _hasLSPLiscense;

        private readonly bool _hasGControlLicense;
        public ExtendAction(ManualApprovalRecordRepository repository)
        {
            Repository = repository;
            var accountId = TenantLocalValue.LogonUserId;
            ActionAccount = AccountDao.Find(item => item.UserId == accountId && item.IsRemoved == 0);

            _hasFSLiscense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.FileSystem);
            _hasLSPLiscense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.SharePointOnPrem);
            _hasGControlLicense = TenantService.HasInitGControlPlatForm().Result;
        }

        public async Task<ManualApprovalActionResult> Restore(List<Guid> itemIds)
        {
            try
            {
                var result = new ManualApprovalActionResult
                {
                    CompletedStatus = ActionCompletedStatus.Succeed
                };

                var items = await Repository.QueryItemsAsync(item => itemIds.Contains(item.Id));

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

                if (!_hasGControlLicense && items.Any(item => item.IsGControlRecord))
                {
                    result.CompletedStatus = ActionCompletedStatus.Failed;
                    return result;
                }

                items.ForEach(item =>
                {
                    var itemActionResult = RestoreItem(item);
                    result.EffectItems.Add(itemActionResult);
                });

                await Repository.UpsertItemsAsync(items);

                return result;
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while restore extended for items: [{string.Join(", ", itemIds)}]. Error: {e}");
                return new ManualApprovalActionResult
                {
                    CompletedStatus = ActionCompletedStatus.Failed,
                    Message = e.Message
                };
            }
        }

        public async Task<ManualApprovalActionResult> Extend(ManualApprovalExtendDefinition definition)
        {
            try
            {
                var extendTime = await CalculationExtendTimeAsync(definition);
                if(extendTime <= DateTime.UtcNow.Ticks)
                {
                    return new ManualApprovalActionResult
                    {
                        CompletedStatus = ActionCompletedStatus.Failed,
                        Message = I18NEntity.GetString("RM_MA_ExtendDisposalTime_Valid_EarlierThanNow")
                    };
                }

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

                items.ForEach(item =>
                {
                    var itemActionResult = ExtendItem(item, extendTime, definition.Comment);
                    result.EffectItems.Add(itemActionResult);
                });

                await Repository.UpsertItemsAsync(items);

                return result;
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while execute extend action for items [{string.Join(", ", definition.ItemIds)}]. Error: {e}");
                return new ManualApprovalActionResult
                {
                    CompletedStatus = ActionCompletedStatus.Failed,
                };
            }
        }

        private ManualApprovalItemActionResult RestoreItem(ManualApprovalRecord item)
        {
            item.ManualExtendTime = 0;
            RebuildAudits(item);
            return new ManualApprovalItemActionResult
            {
                IsSucceed = true,
                //OldValue = item.ManualExtendTime,
                EffectItemFullPath = item.SourceFlag >= 1000 ? item.LeafName : item.ManualFullPath
            };
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
                Action = "RM_JS_MA_ApproveStatus_RestoreExtend"
            });

            item.ManualAudits = SerializerHelper.SerializeToXmlString(audits);
        }
        private ManualApprovalItemActionResult ExtendItem(ManualApprovalRecord item, long extendTime, string comment)
        {
            item.ManualExtendTime = extendTime;
            item.ManualExtendComment = comment;
            item.ManualExtendCount += 1;

            var audits = new List<ReviewAudits>();
            if (!string.IsNullOrEmpty(item.ManualAudits))
            {
                audits = SerializerHelper.DeserializeFromXmlString<List<ReviewAudits>>(item.ManualAudits);
            }
            audits.Add(new ReviewAudits
            {
                ReviewTime = DateTime.UtcNow.Ticks.ToString(),
                ReviewBy = ActionAccount.DisplayName,
                Action = "RM_JS_MA_ApproveStatus_Extend",
                Comment = comment
            });


            item.ManualAudits = SerializerHelper.SerializeToXmlString(audits);

            return new ManualApprovalItemActionResult
            {
                IsSucceed = true,
                OldValue = item.ManualExtendTime,
                EffectItemFullPath = item.SourceFlag >= 1000 ? item.LeafName : item.ManualFullPath
            };
        }

        private static async Task<long> CalculationExtendTimeAsync(ManualApprovalExtendDefinition definition)
        {
            var now = DateTime.UtcNow;
            if(definition.ExtendType == ManualApprovalExtendType.Custom)
            {
                return (await GeneralSettingService.ConvertDateTimeToUtcAsync(definition.CustomeExtendDate)).Ticks;
            }
            else if (definition.ExtendType == ManualApprovalExtendType.After1Month)
            {
                return now.AddMonths(1).Ticks;
            }
            else if(definition.ExtendType == ManualApprovalExtendType.After3Month)
            {
                return now.AddMonths(3).Ticks;
            }
            else if (definition.ExtendType == ManualApprovalExtendType.After6Month)
            {
                return now.AddMonths(6).Ticks;
            }
            else if (definition.ExtendType == ManualApprovalExtendType.After1Year)
            {
                return now.AddYears(1).Ticks;
            }

            return 0;
        }
    }
}
