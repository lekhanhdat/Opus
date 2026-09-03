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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.ManualApproval.Model;
using Cloud.Sdk.Data.MyHub;
using RAGoogle.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ManualApproval.AuditHandler
{
    public class RMManualApprovalAfterAuditHandler : IAsyncAuditAfterHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMManualApprovalAfterAuditHandler));

        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private IAccountDao AccountDao = PlatformWindsorManager.GetService<IAccountDao>();

        private IAccountWrapperService AccountWrapperService => PlatformWindsorManager.GetService<IAccountWrapperService>();

        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo auditInfo, AuditModule module, AuditAction action, Contract.RMWeb.Audit.AuditCategory category, object[] args, object returnValue)
        {
            if (action == AuditAction.ManualApprovalSettingTimer)
            {
                RunJob(auditInfo, returnValue, args);
                return auditInfo;
            }
            else if (action == AuditAction.ManualApprovalConfigSetting)
            {
                return auditInfo;
            }
            else if (action == AuditAction.RunManualApproveOrReject)
            {
                auditInfo.Object = returnValue.ToString();
                var manualActionInfos = SerializerHelper.DeserializeByJsonSerializer<ManualApprovalJobParam>(args[0].ToString());
                if (manualActionInfos.IsFromMyhub)
                {
                    logger.Info("Add bulk action job audit to myhub.");
                    var userName = TenantLocalValue.LogonUserEmail ?? (await AccountDao.GetUserByUserIdAsync(TenantLocalValue.LogonUserId))?.UserPrincipalName;
                    var account = AccountWrapperService.GetAccount(TenantLocalValue.LogonGroupId, userName);
                    await BuildMyhubAudit(returnValue.ToString(), AuditActionType.OpusRunBulkActionJob, account);
                }
                return auditInfo;
            }
            else if (action == AuditAction.RunFolderViewActionJob)
            {
                logger.Info("Add folder view action job audit to myhub.");
                auditInfo.Object = returnValue.ToString();
                var userName = TenantLocalValue.LogonUserEmail ?? (await AccountDao.GetUserByUserIdAsync(TenantLocalValue.LogonUserId))?.UserPrincipalName;
                var account = AccountWrapperService.GetAccount(TenantLocalValue.LogonGroupId, userName);
                await BuildMyhubAudit(returnValue.ToString(), AuditActionType.OpusRunFolderViewActionJob, account);
                return auditInfo;
            }
            else if (action == AuditAction.RunExportHistoryJob
                || action == AuditAction.RunExportRecordsForReviewJob
                || action == AuditAction.RunImportUnderReviewJob)
            {
                auditInfo.Object = returnValue.ToString();

                var historyOption = SerializerHelper.DeserializeByJsonSerializer<ManualApprovalHistoryOption>(args[0]?.ToString());
                if (action == AuditAction.RunExportHistoryJob && !string.IsNullOrEmpty(historyOption.FullPath))
                {
                    auditInfo.Object = historyOption.DisplayName;
                }
                return auditInfo;
            } else if (auditInfo.Action == null && action == AuditAction.RunManualApproveOrReject) {
                auditInfo.Action = action;
            }

                var actionResult = returnValue as ManualApprovalActionResult;
            auditInfo.Status = (int)(actionResult.CompletedStatus == ActionCompletedStatus.Failed ? AuditStatus.Failed : AuditStatus.Successful);
            var effectItems = actionResult.EffectItems;
            var effectFullPaths = effectItems.Select(item => item.EffectItemFullPath);
            auditInfo.Object = effectFullPaths.Count() == 1 ? effectFullPaths.ToList()[0] : string.Join("; ", effectFullPaths);

            switch (action)
            {
                case AuditAction.MarkToApproved:
                    Approved(auditInfo, actionResult, args);
                    break;
                case AuditAction.MarkToRejected:
                    Rejected(auditInfo, actionResult, args);
                    break;
                case AuditAction.EscalateTo:
                    Escalate(auditInfo, args);
                    break;
                case AuditAction.ReassignTo:
                    Reassign(auditInfo, args);
                    break;
                case AuditAction.MarkToExtend:
                    await ExtendAsync(auditInfo, args);
                    break;
                case AuditAction.ChangeAction:
                    ChangeDisposalAction(auditInfo, actionResult, args);
                    break;
                case AuditAction.MarkToPause:
                    Pause(auditInfo, actionResult, args);
                    break;
                case AuditAction.MarkToResume:
                    Resume(auditInfo, actionResult, args);
                    break;
            }
            
            return auditInfo;
        }

        private void Approved(RMAuditInfo info, ManualApprovalActionResult actionResult, object[] args)
        {
            var effectItems = actionResult.EffectItems;

            var auditItems = effectItems.ConvertAll(item =>
            {
                return new AuditItem
                {
                    OldValue = $"RM_JS_MA_ApproveStatus_{((SOApproveDBStatus)item.OldValue)}",
                };
            });

            auditItems = auditItems.GroupBy(item => item.OldValue).Select(group => group.FirstOrDefault()).ToList();
            auditItems.FirstOrDefault().TargetSetting = "RM_RC_Audit_ManualApproveStatus";
            auditItems.FirstOrDefault().NewValue = $"RM_JS_MA_ApproveStatus_{SOApproveDBStatus.Approved}";
            info.ModifyContent.AddRange(auditItems);

            var manualApprovalActionParams = args[0] as ManualApprovalActionParams;
            if (!string.IsNullOrEmpty(manualApprovalActionParams.ApprovalComment))
            {
                var auditApprovalComment = new AuditItem
                {
                    TargetSetting = "RM_MA_ApprovalComment_ApproveWhy",
                    NewValue = manualApprovalActionParams.ApprovalComment.ToString(),
                };
                info.ModifyContent.Add(auditApprovalComment);
            }
            var isFromMyhub = bool.Parse(args[1].ToString());
            if (isFromMyhub)
            {
                logger.Info("Add approve audit to myhub.");
                var userName = TenantLocalValue.LogonUserEmail ?? (AccountDao.GetUserByUserIdAsync(TenantLocalValue.LogonUserId).GetAwaiter().GetResult())?.UserPrincipalName;
                var account = AccountWrapperService.GetAccount(TenantLocalValue.LogonGroupId, userName);
                BuildMyhubAudit(info.Object, AuditActionType.OpusApprove, account).GetAwaiter().GetResult();
            }
        }

        private void Rejected(RMAuditInfo info, ManualApprovalActionResult actionResult, object[] args)
        {
            var effectItems = actionResult.EffectItems;

            var auditItems = effectItems.ConvertAll(item =>
            {
                return new AuditItem
                {
                    OldValue = $"RM_JS_MA_ApproveStatus_{((SOApproveDBStatus)item.OldValue)}",
                };
            });

            auditItems = auditItems.GroupBy(item => item.OldValue).Select(group => group.FirstOrDefault()).ToList();
            auditItems.FirstOrDefault().TargetSetting = "RM_RC_Audit_ManualApproveStatus";
            auditItems.FirstOrDefault().NewValue = $"RM_JS_MA_ApproveStatus_{SOApproveDBStatus.Rejected}";
            info.ModifyContent.AddRange(auditItems);

            var extendType = effectItems.First().ExtendType;
            var customeExtendDate = effectItems.First().ExtendTime;
            var extendTime = extendType switch
            {
                ManualApprovalExtendType.After1Month => DateTime.UtcNow.AddMonths(1),
                ManualApprovalExtendType.After3Month => DateTime.UtcNow.AddMonths(3),
                ManualApprovalExtendType.After6Month => DateTime.UtcNow.AddMonths(6),
                ManualApprovalExtendType.After1Year => DateTime.UtcNow.AddYears(1),
                _ => customeExtendDate,
            };

            var extendSimplifyFormatTime = GeneralSettingService.ConvertTiksToDateTimeAsync(extendTime.Ticks, true);

            var manualApprovalActionParams = args[0] as ManualApprovalActionParams;
            if (!string.IsNullOrEmpty(manualApprovalActionParams.QuickReason))
            {
                var auditLastReason = new AuditItem
                {
                    TargetSetting = "RM_MA_LastReasonforRejection",
                    NewValue = manualApprovalActionParams.QuickReason.ToString(),
                };
                info.ModifyContent.Add(auditLastReason);
            }
            if (!string.IsNullOrEmpty(manualApprovalActionParams.ApprovalComment))
            {
                var auditApprovalComment = new AuditItem
                {
                    TargetSetting = "RM_MA_ApprovalComment_RejectWhy",
                    NewValue = manualApprovalActionParams.ApprovalComment.ToString(),
                };
                info.ModifyContent.Add(auditApprovalComment);
            }

            var auditExtendTime = new AuditItem
            {
                TargetSetting = "RM_JS_MA_ApproveStatus_ExtendTime",
                NewValue = extendSimplifyFormatTime.Result.SimplifyFormatTime.ToString(),
            };
            info.ModifyContent.Add(auditExtendTime);
            var isFromMyhub = bool.Parse(args[1].ToString());
            if (isFromMyhub)
            {
                var userName = TenantLocalValue.LogonUserEmail ?? (AccountDao.GetUserByUserIdAsync(TenantLocalValue.LogonUserId).GetAwaiter().GetResult())?.UserPrincipalName;
                var account = AccountWrapperService.GetAccount(TenantLocalValue.LogonGroupId, userName);
                BuildMyhubAudit(info.Object, AuditActionType.OpusReject, account).GetAwaiter().GetResult();
            }
        }

        private void Escalate(RMAuditInfo info, object[] args)
        {
            var definition = args[0] as ManualAprovalEscalateDefinition;
            var emails = definition.ToUsers.Select(item => item.UserPrincipalName);
            var actionAudit = new AuditItem
            {
                TargetSetting = "RM_JS_MA_EscalateToUsers",
                NewValue = emails.Count() == 1 ? emails.ToList()[0] : string.Join("; ", emails)
            };
            info.ModifyContent.Add(actionAudit);

            var sendAudit = new AuditItem
            {
                TargetSetting = "RM_JS_MA_IsSendEmail",
                NewValue = definition.NeedSendEmail ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
            };
            info.ModifyContent.Add(sendAudit);

            var commentAudit = new AuditItem
            {
                TargetSetting = "RM_JS_MA_Comment",
                NewValue = definition.Comment
            };
            info.ModifyContent.Add(commentAudit);
        }

        private void Reassign(RMAuditInfo info, object[] args)
        {
            var definition = args[0] as ManualAprovalEscalateDefinition;
            var emails = definition.ToUsers.Select(item => item.UserPrincipalName);
            var actionAudit = new AuditItem
            {
                TargetSetting = "RM_JS_MA_ReassignToUsers",
                NewValue = emails.Count() == 1 ? emails.ToList()[0] : string.Join("; ", emails)
            };
            info.ModifyContent.Add(actionAudit);

            var sendAudit = new AuditItem
            {
                TargetSetting = "RM_JS_MA_IsSendEmail",
                NewValue = definition.NeedSendEmail ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
            };
            info.ModifyContent.Add(sendAudit);

            var commentAudit = new AuditItem
            {
                TargetSetting = "RM_JS_MA_Comment",
                NewValue = definition.Comment
            };
            info.ModifyContent.Add(commentAudit);
        }

        private async System.Threading.Tasks.Task ExtendAsync(RMAuditInfo info, object[] args)
        {
            var definition = args[0] as ManualApprovalExtendDefinition;

            var extendDateAudit = new AuditItem
            {
                TargetSetting = "RM_RC_Audit_ManualApproveExtend",
                NewValue = await GetExtendValueAsync(definition)
            };
            info.ModifyContent.Add(extendDateAudit);

            var commentAudit = new AuditItem
            {
                TargetSetting = "RM_JS_MA_Comment",
                NewValue = definition.Comment
            };
            info.ModifyContent.Add(commentAudit);
        }

        private void ChangeDisposalAction(RMAuditInfo info, ManualApprovalActionResult actionResult, object[] args)
        {
            var definition = args[0] as ManualApprovalRelatedRecordsDisposalDefinition;
            var auditItem = new AuditItem
            {
                TargetSetting = "RM_RC_Audit_WhetherDeleteRelatedRecord",
                NewValue = definition.DisposalAction == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both ?
                    "RM_JS_RDM_RelatedRecordsAction_Both" : "RM_JS_RDM_RelatedRecordsAction_None",
                OldValue = definition.DisposalAction == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both ?
                    "RM_JS_RDM_RelatedRecordsAction_None" : "RM_JS_RDM_RelatedRecordsAction_Both"
            };
            info.ModifyContent.Add(auditItem);
        }

        private void ResetManualWorkflow(RMAuditInfo info)
        {
            var commentAudit = new AuditItem
            {
                TargetSetting = "RM_RC_Audit_Action_ResetManualWorkflow"
            };
            info.ModifyContent.Add(commentAudit);
        }

        private void RunJob(RMAuditInfo info, object returnValue, object[] args)
        {
            info.Object = returnValue.ToString();
            if ((int)args[0] == (int)JobRunBy.Schedule)
            {
                info.UserName = "RM_TS_RunSchedule";
            }
        }

        private async Task<string> GetExtendValueAsync(ManualApprovalExtendDefinition definiton)
        {
            var now = DateTime.UtcNow;
            switch (definiton.ExtendType)
            {
                case ManualApprovalExtendType.After1Month:
                    return (await GeneralSettingService.ConvertTiksToDateTimeAsync(now.AddMonths(1).Ticks, true)).SimplifyFormatTime;
                case ManualApprovalExtendType.After3Month:
                    return (await GeneralSettingService.ConvertTiksToDateTimeAsync(now.AddMonths(3).Ticks, true)).SimplifyFormatTime;
                case ManualApprovalExtendType.After6Month:
                    return (await GeneralSettingService.ConvertTiksToDateTimeAsync(now.AddMonths(6).Ticks, true)).SimplifyFormatTime;
                case ManualApprovalExtendType.After1Year:
                    return (await GeneralSettingService.ConvertTiksToDateTimeAsync(now.AddYears(1).Ticks, true)).SimplifyFormatTime;
                case ManualApprovalExtendType.Custom:
                    return (GeneralSettingService.ConvertTiksToUTCDateTime(await GeneralSettingService.GetGeneralSettingAsync(), definiton.CustomeExtendDate.Ticks, true)).SimplifyFormatTime;
            }

            return "";
        }

        private async Task BuildMyhubAudit(string objectName, AuditActionType action, AADAccount userInfo)
        {
            try
            {
                var myhubClient = AosApiUtility.GetMyhubClient(TenantLocalValue.LogonGroupId);
                logger.Info("Get myhub client success.");
                var userType = userInfo?.InviteType == AccountType.Group ? AuditUserType.Office365Group : AuditUserType.User;
                var auditModel = new AuditModel()
                {
                    Category = Cloud.Sdk.Data.MyHub.AuditCategory.OpusTask,
                    InstanceType = InstanceType.OpusCommonTask,
                    ActionTime = DateTime.UtcNow,
                    Action = action,
                    InstanceName = objectName,
                    Processor = new AuditUserModel(userInfo?.Id, userInfo?.UserPrincipalName, userInfo?.DisplayName, userInfo?.Mail)
                };
                auditModel.Processor.AuditUserType = userType;
                await myhubClient.AuditService.AddAuditAsync(auditModel);
                logger.Info("add myhub audit success.");
            }
            catch(Exception e)
            {
                logger.Error($"add myhub audit failed, error : {e}.");
                throw;
            }
        }

        private void Pause(RMAuditInfo info, ManualApprovalActionResult actionResult, object[] args)
        {
            //var effectItems = actionResult.EffectItems;

            //var auditItems = effectItems.ConvertAll(item =>
            //{
            //    int value = (int)item.OldValue;
            //    string str = "";
            //    if (value == 1) {
            //        str = $"RM_FS_JpmcAuditPause";
            //    }
            //    else {
            //        str = $"RM_FS_JpmcAuditResume";
            //    }
            //    return new AuditItem
            //    {
            //        OldValue = str,
            //    };
            //});

            //auditItems.FirstOrDefault().TargetSetting = "RM_RC_Audit_ManualPauseStatus";
            //auditItems.FirstOrDefault().NewValue = $"RM_FS_JpmcAuditPause";
            //info.ModifyContent.AddRange(auditItems);

            logger.Info("Add Pause audit to myhub.");
            var userName = TenantLocalValue.LogonUserEmail ?? (AccountDao.GetUserByUserIdAsync(TenantLocalValue.LogonUserId).GetAwaiter().GetResult())?.UserPrincipalName;
            var account = AccountWrapperService.GetAccount(TenantLocalValue.LogonGroupId, userName);
            BuildMyhubAudit(info.Object, AuditActionType.OpusApprove, account).GetAwaiter().GetResult();
        }

        private void Resume(RMAuditInfo info, ManualApprovalActionResult actionResult, object[] args)
        {
            //var effectItems = actionResult.EffectItems;

            //var auditItems = effectItems.ConvertAll(item =>
            //{
            //    int value = (int)item.OldValue;
            //    string str = "";
            //    if (value == 1)
            //    {
            //        str = $"RM_FS_JpmcAuditPause";
            //    }
            //    else
            //    {
            //        str = $"RM_FS_JpmcAuditResume";
            //    }
            //    return new AuditItem
            //    {
            //        OldValue = str,
            //    };
            //});

            //auditItems.FirstOrDefault().TargetSetting = "RM_RC_Audit_ManualPauseStatus";
            //auditItems.FirstOrDefault().NewValue = $"RM_FS_JpmcAuditResume";
            //info.ModifyContent.AddRange(auditItems);

            logger.Info("Add Resume audit to myhub.");
            var userName = TenantLocalValue.LogonUserEmail ?? (AccountDao.GetUserByUserIdAsync(TenantLocalValue.LogonUserId).GetAwaiter().GetResult())?.UserPrincipalName;
            var account = AccountWrapperService.GetAccount(TenantLocalValue.LogonGroupId, userName);
            BuildMyhubAudit(info.Object, AuditActionType.OpusApprove, account).GetAwaiter().GetResult();
        }

    }
}
