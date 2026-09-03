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
using AvePoint.Api.Contract;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.SharePoint.RestoreJob.Restore.Content;
using Cloud.Sdk.Data.Cop.SMP;
using DocAveOnline.WebApi.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeaveStubType = AvePoint.GCommon.Contract.StorageOptimization.Object.LeaveStubType;
using RestoreObjectLevel = AvePoint.GCommon.Contract.StorageOptimization.Object.RestoreObjectLevel;

namespace AvePoint.RA.Service.Services.Settings.AuditHandler
{
    public class ArchiverJobAfterAuditHandler : IAfterAuditHandler
    {
        private static readonly AveLogger mLog = AveLogger.GetInstance(typeof(ArchiverJobAfterAuditHandler));
        private IRMMiscProfileDao StubSettingDao => PlatformWindsorManager.GetService<IRMMiscProfileDao>();

        private IStorageDeviceService _StorageDeviceService;
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService(ref _StorageDeviceService);

        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            info = new RMAuditInfo();
            info.Module = (AuditModule)model;
            info.Action = (AuditAction)action;
            info.Category = (AuditCategory)category;
            if (returnValue.GetType() == typeof(JobResult) && (action == (int)AuditAction.RunArchiverRestoreJob || action == (int)AuditAction.RunArchiverOutPlaceRestoreJob || action == (int)AuditAction.RunTeamsArchiverRestoreJob || action == (int)AuditAction.RunArchiverRestoreGoogleDriveJob))
            {
                var configInfo = args.FirstOrDefault();
                JobResult res = (JobResult)returnValue;
                info.Object = res?.Jobs == null ? string.Empty : string.Join("; ", res.Jobs?.Select(job => job.Id));
                info.Status = res?.ErrorCode == ErrorCode.none ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                info.UserName = (configInfo is EndUserRestoreConfig) ? (configInfo as EndUserRestoreConfig).Office365User : (configInfo as ExportArchivedContentConfig).Office365UserMail;
                TryApplyPublicApiAuditUser(args, info);
            }
            else if (action == (int)AuditAction.RunArchiverRestoreJob || action == (int)AuditAction.RunArchiverOutPlaceRestoreJob || action == (int)AuditAction.RunTeamsArchiverRestoreJob)
            {
                info.Object = returnValue.ToString();
                AddAdminResotreAudit(args, info);
                TryApplyPublicApiAuditUser(args, info);
            }
            else if (action == (int)AuditAction.SimulateRunArchiverRestoreJob)
            {
                RAReturnMessage returnMsg = returnValue as RAReturnMessage;
                info.Object = returnMsg?.Extension ?? string.Empty;
                info.Status = returnMsg?.MessageType == RAMessageType.Successful
                    ? (int)AuditStatus.Successful
                    : (int)AuditStatus.Failed;
            }
            else if (action == (int)AuditAction.RunODPreScanJob
                  || action == (int)AuditAction.RunSOPreScanJob
                  || action == (int)AuditAction.RunTeamsPreScanJob)
            {
                RAReturnMessage returnMsg = returnValue as RAReturnMessage;
                info.Object = returnMsg?.Extension ?? string.Empty;
                info.Status = returnMsg?.MessageType == RAMessageType.Successful
                    ? (int)AuditStatus.Successful
                    : (int)AuditStatus.Failed;
            }
            else if (action == (int)AuditAction.RunArchiverBackupJob || action == (int)AuditAction.RunArchiverRestoreJob || action == (int)AuditAction.RunArchiverRestoreGoogleDriveJob
                || action == (int)AuditAction.RunArchiverRetentionJob || action == (int)AuditAction.RunVeoMergeJob
                || action == (int)AuditAction.RunSOPreScanJob || action == (int)AuditAction.RunODPreScanJob || action == (int)AuditAction.RunTeamsPreScanJob
                || action == (int)AuditAction.RunArchiverFullTextIndexJob || action == (int)AuditAction.RunArchiverDeleteRestoredDataJob
                || action == (int)AuditAction.RunArchiverDedupJob || action == (int)AuditAction.ExportIndex || action == (int)AuditAction.ApprovalProcessConfig
                || action == (int)AuditAction.RunDeleteOrphanDatasJob
                || action == (int)AuditAction.RunEndUserArchiverBackupJob
                || action == (int)AuditAction.RunTeamsUpgradeJob || action == (int)AuditAction.RunTeamsConflictJob || action == (int)AuditAction.RunJobMonitorArchiveJob
                || action == (int)AuditAction.ImportExternalArchivedData || action == (int)AuditAction.RunStubDisposalJob 
                || action == (int)AuditAction.RunDeleteArchivedSiteCollectionJob
                )
            {
                info.Object = returnValue.ToString();
            }
            else if (action == (int)AuditAction.RunMoveIndexJob)
            {
                info.Object = returnValue.ToString();
                var jobInfo = SerializerHelper.DeserializeByDataContractSerializer<RMArchiverMoveIndexInfo>(args[2].ToString());
                mLog.Info($"SourceDeviceId {jobInfo.SrcIndexDeviceId} , DestinationDeviceId {jobInfo.DestIndexDeviceId}");
                var currentIndex = StorageDeviceService.GetIndexDevice();
                if (currentIndex == null)
                {
                    return info;
                }
                var srcDeviceName = currentIndex.Name;
                var destDeviceName = jobInfo.DestIndexDeviceId;
                if (!string.Equals(jobInfo.SrcIndexDeviceId, currentIndex.Id))
                {
                    mLog.Warn($"The source index device id {jobInfo.SrcIndexDeviceId} is different from current index device id {currentIndex.Id}");
                    var srcDevice = StorageDeviceService.GetStorageDeviceById(jobInfo.SrcIndexDeviceId);
                    srcDeviceName = srcDevice?.Name ?? srcDeviceName;
                }

                var destDevice = StorageDeviceService.GetStorageDeviceById(jobInfo.DestIndexDeviceId);
                if (destDevice != null)
                {
                    destDeviceName = destDevice.Name;
                }

                info.ModifyContent =
                [
                    new AuditItem()
                    {
                        Id = Guid.NewGuid(),
                        TargetSetting = "RM_JS_JMD_Grid_SrcStorageName",
                        NewValue = srcDeviceName,
                    },
                    new AuditItem()
                    {
                        Id = Guid.NewGuid(),
                        TargetSetting = "RM_JS_JMD_Grid_DesStorageName",
                        NewValue = destDeviceName
                    },
                ];

            }
            else if (action == (int)AuditAction.RunConvertStubJob)
            {
                ConvertStubDto jobInfo = SerializerHelper.DeserializeByDataContractSerializer<ConvertStubDto>(args[2].ToString());
                info.Object = returnValue.ToString();
                var targetStub = StubSettingDao.Load(jobInfo.StubTemplateId.ToString());
                var stubTypeI18Nstr = jobInfo.StubType switch
                {
                    LeaveStubType.Aspx => "RM_AR_CP_Stub_Type_Aspx",
                    LeaveStubType.Txt => "RM_AR_CP_Stub_Type_Txt",
                    LeaveStubType.Html => "RM_AR_CP_Stub_Type_Html",
                    LeaveStubType.Link => "RM_AR_CP_Stub_Type_RestoreLink",
                    _ => "Unknown Stub Type"
                };
                info.ModifyContent =
                [
                    new AuditItem()
                    {
                        Id = Guid.NewGuid(),
                        TargetSetting = "RM_RC_Audit_ConvertStub_OriginalStubType",
                        NewValue = stubTypeI18Nstr,
                    },
                    new AuditItem()
                    {
                        Id = Guid.NewGuid(),
                        TargetSetting = "RM_RC_Audit_ConvertStub_TargetStubName",
                        NewValue = targetStub.Name
                    },
                ];
            }
            else if (action == (int)AuditAction.RunDeclaredRecordsMigrationJob)
            {
                var jobInfo = SerializerHelper.DeserializeByDataContractSerializer<DeclaredRecordsMigrationDto>(args[2].ToString());
                info.Object = returnValue.ToString();
                info.ModifyContent =
                [
                    new AuditItem()
                    {
                        Id = Guid.NewGuid(),
                        TargetSetting = "RM_RC_Audit_DeclaredRecordsMigration_RecordsLabel",
                        NewValue = jobInfo.RecordsLabel,
                    },
                ];
            }
            else if (action == (int)AuditAction.ApprovalProcessConfig)
            {
                if (string.IsNullOrEmpty(returnValue.ToString()))
                {
                    info.NotNeedRecordAudit = true;
                }
            }
            else if (action == (int)AuditAction.RunDeleteArchivedSiteCollectionJob)
            {
                var siteNodeInfo = SerializerHelper.DeserializeByDataContractSerializer<SiteCollectionNodesInfo>(args[2].ToString());
                info.Object = returnValue.ToString();
                info.ModifyContent =
                [
                    new AuditItem()
                    {
                        Id = Guid.NewGuid(),
                        TargetSetting = "RM_JS_RC_ActionAudit_ObjType_SiteCollection",
                        NewValue = siteNodeInfo.SiteUrl,
                    },
                ];
            }

            return info;
        }

        private void AddAdminResotreAudit(object[] args, RMAuditInfo info)
        {
            JobType jobType = (JobType)args[3];
            var validJobTypes = new List<JobType>
            {
                JobType.ArchiverRestore,
                JobType.ArchiverOutPlaceRestore,
                JobType.ArchiverToSpoRestore,
                JobType.StubArchiverRestore,
                JobType.M365InPlaceArchiverRestore,

                // Advanced
                JobType.StubArchiverRestore,
                JobType.M365InPlaceArchiverRestore,

                // Teams
                JobType.TeamsArchiverRestore,
                JobType.TeamsOutPlaceRestore,
                JobType.MailBoxArchiverRestore,
            };

            if (!validJobTypes.Contains(jobType))
            {
                mLog.Warn($"Invalid job type {jobType} for admin restore audit. Skipping audit content addition.");
                return;
            }

            JobRunBy jobRunBy = (JobRunBy)args[0];
            string jobRunByUser = (string)args[1];
            string param = (string)args[2];
            JobPriority jobPriority = (JobPriority)args[4];
            RestoreSettingAndTree restoreSettingAndTree = SerializerHelper.DeserializeByDataContractSerializer<RestoreSettingAndTree>(param);
            RestoreInfo restoreSetting = restoreSettingAndTree.Setting;
            info.ModifyContent = new List<AuditItem>();

            if (restoreSetting.RestoreTypeSelect == RestoreType.ToSPOLocation)
            {
                AuditItem restoreVersions = new AuditItem()
                {
                    Id = Guid.NewGuid(),
                    TargetSetting = "RM_RestoreCenter_RestoreToSPOLibraryOrFolder",
                };
                info.ModifyContent.Add(restoreVersions);
                restoreVersions.NewValue = restoreSetting.DestDto.FullPath;
            }
            else if (restoreSetting.RestoreTypeSelect == RestoreType.ArchivedStubs)
            {
                AuditItem restoreVersions = new AuditItem()
                {
                    Id = Guid.NewGuid(),
                    TargetSetting = "RM_JS_JM_JobType_StubArchiverRestore",
                };
                info.ModifyContent.Add(restoreVersions);
                restoreVersions.NewValue = restoreSetting.DestDto.FullPath;
            }
            else if (restoreSetting.RestoreTypeSelect == RestoreType.M365InPlaceArchivedFiles)
            {
                AuditItem restoreVersions = new AuditItem()
                {
                    Id = Guid.NewGuid(),
                    TargetSetting = "RM_JS_JM_JobType_M365InPlaceArchiverRestore",
                };
                info.ModifyContent.Add(restoreVersions);
                restoreVersions.NewValue = restoreSetting.DestDto.FullPath;
            }

            if (restoreSetting.RestoreTypeSelect == RestoreType.ArchivedStubs || restoreSetting.RestoreTypeSelect == RestoreType.M365InPlaceArchivedFiles)
            {
                info.ModifyContent.Add(new()
                {
                    Id = Guid.NewGuid(),
                    TargetSetting = "RM_JS_JM_RestoreCenter_RestoreScope",
                    NewValue = restoreSetting.RestoreScope switch
                    {
                        GCommon.Contract.StorageOptimization.Object.RestoreScope.IncludeChildrenContainersAndFolders => "RM_JS_JM_RestoreCenter_IncludeChildren",
                        GCommon.Contract.StorageOptimization.Object.RestoreScope.SelectedLocationOnly => "RM_JS_JM_RestoreCenter_SelectedLocationOnly",
                    }
                });
            }

            bool shouldShowDocumentVersion = jobType != JobType.MailBoxArchiverRestore 
                && (restoreSetting.RestoreTypeSelect == RestoreType.OutOfPlace 
                    || restoreSetting.RestoreTypeSelect == RestoreType.InPlace 
                    || restoreSetting.RestoreTypeSelect == RestoreType.ToSPOLocation
                    || restoreSetting.RestoreTypeSelect == RestoreType.ArchivedStubs
                    );

            if (shouldShowDocumentVersion)
            {
                AuditItem restoreVersions = new AuditItem()
                {
                    Id = Guid.NewGuid(),
                    TargetSetting = "RM_RestoreCenter_RestoreVersionTitle",
                };
                info.ModifyContent.Add(restoreVersions);
                if (restoreSetting.RestoreVersionOption == RestoreDocumentVersionsOption.AllVersions)
                {
                    restoreVersions.NewValue = "RM_RestoreCenter_RestoreKeepAllVersion";
                }
                else
                {
                    restoreVersions.NewValue = restoreSetting.KeepVersionsNumber.ToString();
                }
            }

            var isTeamsInplace = jobType == JobType.TeamsArchiverRestore && restoreSetting.RestoreTypeSelect == RestoreType.InPlace;

            if (isTeamsInplace)
            {
                AuditItem restorePosts = new AuditItem()
                {
                    Id = Guid.NewGuid(),
                    TargetSetting = "RM_RestoreCenter_Conversation",
                };
                info.ModifyContent.Add(restorePosts);
                if (restoreSetting.RestoreConversationType == RestoreConversationType.Skip || restoreSetting.IsSkipRestoreConversation)
                {
                    restorePosts.NewValue = "RM_RestoreCenter_Skip_RestoreConversation";
                }
                else if (restoreSetting.RestoreConversationType == RestoreConversationType.Html)
                {
                    restorePosts.NewValue = "RM_RestoreCenter_ConversationAsHtml";
                }
                else if (restoreSetting.RestoreConversationType == RestoreConversationType.Original)
                {
                    restorePosts.NewValue = "RM_RestoreCenter_ConversationInPlace_Delegate";
                }
            }

            var isToSPORestore = jobType == JobType.ArchiverRestore || jobType == JobType.ArchiverToSpoRestore || isTeamsInplace || jobType == JobType.StubArchiverRestore;
            var isToStorageRestore = jobType == JobType.ArchiverOutPlaceRestore || jobType == JobType.MailBoxArchiverRestore || jobType == JobType.TeamsOutPlaceRestore;

            if (isToSPORestore)
            {
                AuditItem conflictOption = new AuditItem()
                {
                    Id = Guid.NewGuid(),
                    TargetSetting = "RM_BCM_Audit_NameConflictOption",
                };
                info.ModifyContent.Add(conflictOption);
                switch (restoreSetting.RestoreOption)
                {
                    case GCommon.Contract.StorageOptimization.Object.RestoreOption.OverWrite:
                        conflictOption.NewValue = "RM_TM_Excel_Overwrite";
                        break;
                    case GCommon.Contract.StorageOptimization.Object.RestoreOption.NotOverWrite:
                        conflictOption.NewValue = "RM_TM_Excel_Skip";
                        break;
                    case GCommon.Contract.StorageOptimization.Object.RestoreOption.Append:
                        conflictOption.NewValue = "ExchangeOnline.Service_eeae3ec9-8bb0-4d81-a063-ab2c6d8a23ae";
                        break;
                    default:
                        conflictOption.NewValue = "";
                        break;
                }

                AuditItem appConflictOption = new AuditItem()
                {
                    Id = Guid.NewGuid(),
                    TargetSetting = "StorageOptimization.Gui_B2838B85-02A7-4D8D-8B89-2A138EE3589B",
                };
                info.ModifyContent.Add(appConflictOption);
                switch (restoreSetting.RestoreAPPOption)
                {
                    case GCommon.Contract.StorageOptimization.Object.RestoreOption.OverWrite:
                        appConflictOption.NewValue = "RM_TM_Excel_Overwrite";
                        break;
                    case GCommon.Contract.StorageOptimization.Object.RestoreOption.NotOverWrite:
                        appConflictOption.NewValue = "RM_TM_Excel_Skip";
                        break;
                    case GCommon.Contract.StorageOptimization.Object.RestoreOption.Append:
                        appConflictOption.NewValue = "ExchangeOnline.Service_eeae3ec9-8bb0-4d81-a063-ab2c6d8a23ae";
                        break;
                    default:
                        appConflictOption.NewValue = "";
                        break;
                }

                info.ModifyContent.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        TargetSetting = "RM_AR_SPS_Options_Workflow",
                        NewValue = restoreSetting.IncludeWorkflowDefinition ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                });

                info.ModifyContent.Add(new()
                {
                    Id = Guid.NewGuid(),
                    TargetSetting = "StorageOptimization.Gui_9DC59F76-D900-4F54-8ECD-5385AD1C7B8A",
                    NewValue = restoreSetting.IncludeSharingLink ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                });

                info.ModifyContent.Add(new()
                {
                    Id = Guid.NewGuid(),
                    TargetSetting = "RM_AR_RC_Panel_RestoreToLocked",
                    NewValue = restoreSetting.IsSupportLockedSite ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
                });

                bool siteCollectionLevelRestore = restoreSettingAndTree?.Tree?.FirstOrDefault()?.Level == GCommon.Contract.Tree.Object.NodeLevel.SiteCollection
                    && restoreSettingAndTree?.Tree?.FirstOrDefault()?.Children?.Any() != true;

                if (siteCollectionLevelRestore)
                {
                    info.ModifyContent.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        TargetSetting = "RM_RS_CheckSiteAdminOrOwnerIfUserNotExist",
                        NewValue = !restoreSetting.IsSpecifyUser ? "RM_JS_Common_No" : restoreSetting.SpecifyUserList.FirstOrDefault()?.UserPrincipalName
                    });
                }
                else if (isTeamsInplace)
                {
                    info.ModifyContent.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        TargetSetting = "RM_RS_CheckTeamsGroupsAdminOrOwnerIfUserNotExist",
                        NewValue = !restoreSetting.IsSpecifyUser ? "RM_JS_Common_No" : restoreSetting.SpecifyUserList.FirstOrDefault()?.UserPrincipalName
                    });
                }
            }
            else if (isToStorageRestore)
            {
                info.ModifyContent.Add(
                    new()
                    {
                        Id = Guid.NewGuid(),
                        TargetSetting = "RM_AR_RC_Panel_Storage",
                        NewValue = restoreSetting.StorageDeviceDto.Name
                    });

                List<string> notifyUsers = restoreSetting.NotificationUsers.Select(u => u.UserPrincipalName).ToList();
                info.ModifyContent.Add(
                    new()
                    {
                        Id = Guid.NewGuid(),
                        TargetSetting = "RM_AR_RC_Panel_Notification",
                        NewValue = string.Join("; ", notifyUsers)
                    });
            }

            AuditItem priority = new AuditItem()
            {
                Id = Guid.NewGuid(),
                TargetSetting = "RM_JS_JM_Priority",
            };
            info.ModifyContent.Add(priority);
            switch (restoreSetting.JobPriority)
            {
                case JobPriority.Low:
                    priority.NewValue = "RM_JS_JM_Priority_Low";
                    break;
                case JobPriority.Normal:
                    priority.NewValue = "RM_JS_JM_Priority_Normal";
                    break;
                case JobPriority.High:
                    priority.NewValue = "RM_JS_JM_Priority_High";
                    break;
                default:
                    priority.NewValue = "";
                    break;
            }

            if (restoreSettingAndTree.IsSearchAllRestore)
            {
                AuditItem selectAlls = new AuditItem()
                {
                    Id = Guid.NewGuid(),
                    TargetSetting = "RM_AR_RC_Search_Tab_SelectedAllResult",
                    NewValue = "RM_JS_Common_Yes"
                };
                info.ModifyContent.Add(selectAlls);
            }

            if (info.Action == AuditAction.RunArchiverRestoreJob)
            {
                info.Action = restoreSetting.RestoreTypeSelect switch
                {
                    RestoreType.StubOop or RestoreType.InPlace => AuditAction.RunArchiverInPlaceRestoreJob,
                    RestoreType.OutOfPlace => AuditAction.RunArchiverOutPlaceRestoreJob,
                    RestoreType.ToSPOLocation => AuditAction.RunArchiverToSpoRestoreJob,
                    RestoreType.ArchivedStubs => AuditAction.RunStubArchiverRestoreJob,
                    RestoreType.M365InPlaceArchivedFiles => AuditAction.RunM365ArchiverRestoreJob,
                    _ => info.Action,
                };
            }
            else if (jobType == JobType.MailBoxArchiverRestore)
            {
                info.Action = AuditAction.RunTeamsMailboxArchiverOutPlaceRestoreJob;
            }
            else if (info.Action == AuditAction.RunTeamsArchiverRestoreJob)
            {
                info.Action = restoreSetting.RestoreTypeSelect switch
                {
                    RestoreType.InPlace => AuditAction.RunTeamsArchiverInPlaceRestoreJob,
                    RestoreType.OutOfPlace => AuditAction.RunTeamsArchiverOutPlaceRestoreJob,
                    _ => info.Action,
                };
            }
        }

        private static void TryApplyPublicApiAuditUser(object[] args, RMAuditInfo info)
        {
            if (args == null || args.Length < 3 || info == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(args[2]?.ToString()))
            {
                return;
            }

            try
            {
                RestoreSettingAndTree restoreSettingAndTree = SerializerHelper.DeserializeByDataContractSerializer<RestoreSettingAndTree>(args[2].ToString());
                if (!string.IsNullOrWhiteSpace(TenantLocalValue.ClientName)
                    && restoreSettingAndTree?.Setting?.IsPublicRestoreApiRequest == true)
                {
                    info.UserName = TenantLocalValue.ClientName;
                }
            }
            catch (Exception exception)
            {
                mLog.Warn($"Failed to apply public API audit user override. Exception:{exception}");
            }
        }

    }
}
