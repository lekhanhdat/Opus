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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object.Compare;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SOSourceFlag = AvePoint.RA.SharePoint.ArchiverCommon.SOSourceFlag;

namespace AvePoint.RA.Service.Services.RuleManagement.AuditHandler
{
    public class RuleAuditUtil
    {
        private static IStorageDeviceService _storageDeviceService;

        private static IStorageDeviceService StorageDeviceService
        {
            get
            {
                if (_storageDeviceService == null)
                {
                    _storageDeviceService = PlatformWindsorManager.GetService<IStorageDeviceService>();
                }
                return _storageDeviceService;
            }
        }

        public static Dictionary<int, string> exportType = new Dictionary<int, string>()
        {
            {-1, "RM_JS_RDM_CreateRule_ExportType_None"},
            { 0, "RM_JS_RDM_CreateRule_ExportType_Autonomy"},
            { 1, "RM_JS_RDM_CreateRule_ExportType_Concordance"},
            { 2, "RM_JS_RDM_CreateRule_ExportType_EDRM"},
            { 3, "RM_JS_RDM_CreateRule_ExportType_VEO"},
            { 4, "RM_JS_RDM_CreateRule_ExportType_NAA"},
            { 5, "RM_JS_RDM_CreateRule_ExportType_NARA"}
        };
        public static string GetExportInfo(SOExportInfo info)
        {
            StringBuilder sb = new StringBuilder();
            if (info != null)
            {
                sb.Append(info.exportSPDataOption == ExportSPDataOption.ExportBeforeArchive ? "RM_JS_RDM_CreateRule_Options_ExportBefore " : "RM_JS_RDM_NoExportAction ");
                sb.Append("<br>");
                sb.Append("RM_RDM_CreateRule_Title_ExportType ");
                sb.Append("<br>");
                sb.Append(exportType[(int)info.exportType] + " ");
            }
            else
            {
                sb.Append("RM_JS_RDM_NoExportAction ");
            }
            return sb.ToString();
        }

        public static string GetAuditorRuleActionString(RMRuleInfos rule, RuleModel onedriveMod = RuleModel.None, SOSourceFlag sourceFlag = SOSourceFlag.None, bool isNewLogicAccount = false)
        {
            string strArchiverActions = "";
            int keepDataOption = rule.RuleKeepDataOption;
            if (rule.RuleLevel == PolicyLevel.FileSysFile)
            {
                if (keepDataOption == (int)KeepDataStatus.Delete && rule.MoveDto != null)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_MoveRecord_FS");
                }
                else if((keepDataOption & (int)KeepDataStatus.Archive) == (int)KeepDataStatus.Archive)
                {
                    strArchiverActions = I18NEntity.GetString("RM_RDM_CreateRule_ArchiveToAzureBlobStorage");
                }
                else
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ArchiveAndRemove");
                    if ((keepDataOption & 128) == (int)KeepDataStatus.LinkToDocument)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_LeaveStub_FS"));
                    }
                }
            }
            else if (rule.RuleLevel == PolicyLevel.AzureFileDocument)
            {
                if (keepDataOption == (int)KeepDataStatus.Delete && rule.MoveDto != null)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_MoveRecord_FS");
                }
                else
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ArchiveAndRemove");
                    if ((keepDataOption & 128) == (int)KeepDataStatus.LinkToDocument)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_LeaveStub_FS"));
                    }
                }
            }
            else if (rule.RuleLevel == PolicyLevel.ExchangeOnlineItem)
            {
                if (rule.ExportInfo != null && rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ExportOnly");
                }
                else if ((keepDataOption & (int)KeepDataStatus.Keep) == (int)KeepDataStatus.Keep)
                {
                    //strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ExchangeArchiveAndKeep");
                    strArchiverActions = "RM_JS_RDM_CreateRule_Options_ArchiveAndKeep";
                }
                else if (ExcludeOptionUnderMoveAction(keepDataOption) == (int)KeepDataStatus.Delete && rule.MoveDto != null)
                {
                    strArchiverActions = "RM_JS_RDM_CreateRule_Options_MoveRecord";
                    if (rule.MoveDto.IsDeleteSourceItem)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_JS_BCM_Rule_Move_IsRemoveEmail ");
                    }

                    if (rule.MoveDto.isKeepClassification)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_JS_BCM_Rule_Move_IsReclassify ");
                    }
                    if (rule.MoveDto.IsMoveToSP)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_JS_BCM_Explorer_ExoMoveToSP_CheckboxTitle ");
                    }
                }
                else
                {
                    strArchiverActions = "RM_JS_RDM_CreateRule_Options_ExchangeArchiveAndRemove";
                }
            }
            else if (rule.RuleLevel == PolicyLevel.PhysicalBox || rule.RuleLevel == PolicyLevel.PhysicalFile)
            {
                if (rule.IsCalculationDisposalDate)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_CalculateDisposalDate");
                }
                else if (keepDataOption == (int)KeepDataStatus.Delete && rule.MoveDto != null)
                {
                    strArchiverActions = "RM_JS_RDM_CreateRule_Options_MoveLocation";
                }
                else
                {
                    strArchiverActions = "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove";
                    if (rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_RDM_CreateRule_DeleteRelatedRecord ");
                    }
                    if (rule.DestroyEmptyBoxOnFolderRule)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_RDM_CreateRule_DestroyEmptyBox ");
                    }
                }
            }
            else if (rule.RuleLevel == PolicyLevel.BoxDocument)
            {
                if (keepDataOption == (int)KeepDataStatus.Delete && rule.MoveDto != null)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_MoveRecord_FS");
                }
                else
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ArchiveAndRemove");
                    if ((keepDataOption & 128) == (int)KeepDataStatus.LinkToDocument)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_LeaveStub_FS"));
                    }
                }
            }
            else if (rule.RuleLevel == PolicyLevel.GoogleDriveDocument)
            {
                if (rule.MoveDto != null)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_MoveRecord");

                }
                else if (rule.ExportInfo is { exportSPDataOption: ExportSPDataOption.ExportWithoutArchive })
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ExportOnly");
                }
                else if ((keepDataOption & (int)KeepDataStatus.Archive) == (int)KeepDataStatus.Archive)
                {
                    strArchiverActions = I18NEntity.GetString("RM_RDM_CreateRule_ArchiveToAzureBlobStorage");
                }
                else
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ArchiveAndRemove");
                }

            }
            else
            {
                if ((keepDataOption & (int)KeepDataStatus.ArchiverOnly) == (int)KeepDataStatus.ArchiverOnly)
                {
                    strArchiverActions = "RM_JS_RDM_CreateRule_Options_Backup";
                    if ((keepDataOption & (int)KeepDataStatus.ArchiveOnlyLastestVersion) == (int)KeepDataStatus.ArchiveOnlyLastestVersion)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_JS_Rule_ArchiveVersionAndDestroyFile ");
                    }
                }
                else if (rule.ExportInfo != null && rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ExportOnly");
                }
                else if (keepDataOption != (int)KeepDataStatus.DeleteOnly &&
                    ExcludeOptionUnderMoveAction(keepDataOption) != (int)KeepDataStatus.Delete && keepDataOption != (int)KeepDataStatus.Remove
                    && (keepDataOption & 128) != (int)KeepDataStatus.LinkToDocument
                    && (keepDataOption & 256) != (int)KeepDataStatus.NotBackup
                    && keepDataOption != (int)KeepDataStatus.Vault
                    && ExcludeOptionUnderMoveAction(keepDataOption) != (int)KeepDataStatus.Archive
                    && ExcludeOptionUnderMoveAction(keepDataOption) != (int)KeepDataStatus.ArchiveAndLeaveStub
                    && (keepDataOption & (int)KeepDataStatus.ArchiveLatestVersion) != (int)KeepDataStatus.ArchiveLatestVersion
                    && (keepDataOption & (int)KeepDataStatus.KeepLatestVersionAndArhiveOthers) != (int)KeepDataStatus.KeepLatestVersionAndArhiveOthers
                    && (keepDataOption & (int)KeepDataStatus.TriggerMicrosoft365Archiving) != (int)KeepDataStatus.TriggerMicrosoft365Archiving
                    && ((rule.ModelType != RuleModel.SOArchiver && onedriveMod != RuleModel.SOArchiver) || (rule.ModelType == RuleModel.SOArchiver
                    && (keepDataOption & (int)KeepDataStatus.TagContent) == (int)KeepDataStatus.TagContent
                    && (keepDataOption & (int)KeepDataStatus.Keep) == (int)KeepDataStatus.Keep))
                    )
                {
                    if (sourceFlag == SOSourceFlag.OneDrive)
                    {
                        strArchiverActions = !DataCenterUtil.Is21V() && isNewLogicAccount ? "RM_JS_RDM_CreateRule_Options_TagOrLock" : "RM_JS_RDM_CreateRule_Options_ArchiveAndKeep_TagContent";
                    }
                    else
                    {
                        strArchiverActions = sourceFlag == SOSourceFlag.SharePoint && !DataCenterUtil.Is21V() && isNewLogicAccount ? "RM_JS_RDM_CreateRule_Options_TagOrLock" : "RM_JS_RDM_CreateRule_Options_ArchiveAndKeep";
                    }

                    if ((keepDataOption & (int)KeepDataStatus.DeclareRecord) == (int)KeepDataStatus.DeclareRecord)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_RDM_CreateRule_Options_DeclareDocumnet ");
                    }
                    if ((keepDataOption & (int)KeepDataStatus.TagContent) == (int)KeepDataStatus.TagContent)
                    {
                        if (rule.TagContentInfo != null && rule.TagContentInfo.Any())
                        {
                            strArchiverActions = BuildTagContentAuditAction(rule, keepDataOption, sourceFlag, isNewLogicAccount);
                        }
                        else
                        {
                            strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_TM_Excel_DoTag ");
                        }
                    }
                }
                else if (ExcludeOptionUnderMoveAction(keepDataOption) == (int)KeepDataStatus.Delete && rule.MoveDto != null)
                {
                    strArchiverActions = "RM_JS_RDM_CreateRule_Options_MoveRecord";
                    if (!rule.MoveDto.NotDeclareMovedData)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, (sourceFlag == SOSourceFlag.SharePoint || sourceFlag == SOSourceFlag.OneDrive) && !DataCenterUtil.Is21V() && isNewLogicAccount ? "RM_JS_RDM_CreateRule_Options_Move_LockByRecordsLabel " : "RM_JS_RDM_CreateRule_Options_Move_DeclareRecord ");
                    }

                    if (rule.MoveDto.isKeepClassification)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_JS_BCM_Rule_Move_IsReclassify ");
                    }

                    if (rule.MoveDto.IsMoveAllVersions)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_JS_RDM_CreateRule_Options_Move_AllVersions ");
                    }

                    if (rule.MoveDto.IsKeepFolderStructure)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_JS_RDM_CreateRule_Options_Move_FolderStructure ");
                    }
                    if ((keepDataOption & (int)KeepDataStatus.IsEnableRemoveRetentionLabel) == (int)KeepDataStatus.IsEnableRemoveRetentionLabel)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_Options_IncludeRetentionLabels"));
                    }
                }
                else if ((keepDataOption & (int)KeepDataStatus.Archive) == (int)KeepDataStatus.Archive
                    || (keepDataOption & (int)KeepDataStatus.ArchiveAndLeaveStub) == (int)KeepDataStatus.ArchiveAndLeaveStub
                    || (keepDataOption & (int)KeepDataStatus.ArchiveBackupAndRemoveLeaveStub) == (int)KeepDataStatus.ArchiveBackupAndRemoveLeaveStub
                    || (keepDataOption & (int)KeepDataStatus.ArchiveLatestVersion) == (int)KeepDataStatus.ArchiveLatestVersion
                    || (keepDataOption & (int)KeepDataStatus.KeepLatestVersionAndArhiveOthers) == (int)KeepDataStatus.KeepLatestVersionAndArhiveOthers
                    ||(keepDataOption & (int)KeepDataStatus.ArchiveBackupAndRemove) == (int)KeepDataStatus.ArchiveBackupAndRemove)
                {
                    if (rule.ModelType == RuleModel.Records)
                    {
                        strArchiverActions = "RM_RDM_CreateRule_ArchiveToAzureBlobStorage";
                    }
                    else
                    {
                        strArchiverActions = "RM_JS_RDM_CreateRule_Options_BackupAndRemove";
                    }
                    if ((keepDataOption & (int)KeepDataStatus.ArchiveAndLeaveStub) == (int)KeepDataStatus.ArchiveAndLeaveStub
                        || (keepDataOption & (int)KeepDataStatus.ArchiveBackupAndRemoveLeaveStub) == (int)KeepDataStatus.ArchiveBackupAndRemoveLeaveStub)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_RDM_CreateRule_LeaveStubOption ");
                        if (rule.DeclareLinkFile)
                        {
                            strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_JS_RDM_CreateRule_Options_DeclareStub ");
                        }
                    }
                    if ((keepDataOption & (int)KeepDataStatus.ArchiveLatestVersion) == (int)KeepDataStatus.ArchiveLatestVersion)
                    {
                        strArchiverActions = string.Format("{0}; {1}{2}", strArchiverActions, "RM_JS_Audit_ArchiveVersionAndDestroyFile ", rule.ArchivedLatestVersion);
                    }
                    if ((keepDataOption & (int)KeepDataStatus.KeepLatestVersionAndArhiveOthers) == (int)KeepDataStatus.KeepLatestVersionAndArhiveOthers)
                    {
                        strArchiverActions = string.Format("{0}; {1}{2}", strArchiverActions, "RM_JS_Audit_KeepVersionAndArchiveOther ", rule.KeepLatestMajorAndMinorVersionAndArchiveOthers);
                    }
                    
                    if (rule.DeleteRecords)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_RDM_CreateRule_Options_IncludeDeclaredFile ");
                    }

                    if (rule.IsDeleteSiteCollectionToRecycleBin())
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_RDM_CreateRule_Options_DeleteSiteCollectionToRecycleBin ");
                    }

                    if (rule.IncludeDeleteRecordLabel)
                    {
                       strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_RDM_CreateRule_RecordsLabelOption ");
                       if (rule.LockRecordBeforeDestroy)
                       {
                           strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_RDM_CreateRule_LockRecordBeforeDestroy ");
                       }
                    }

                    if ((keepDataOption & (int)KeepDataStatus.IsEnableRemoveRetentionLabel) == (int)KeepDataStatus.IsEnableRemoveRetentionLabel)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_Options_IncludeRetentionLabels"));
                    }
                }
                else if ((keepDataOption & (int)KeepDataStatus.DeleteOnly) == (int)KeepDataStatus.DeleteOnly)
                {
                    strArchiverActions = "RM_JS_JM_DataOperation_DeleteOnlyFromSharePoint";
                    if ((keepDataOption & (int)KeepDataStatus.KeepLatestVersion) == (int)KeepDataStatus.KeepLatestVersion)
                    {
                        strArchiverActions = string.Format("{0}; {1}{2}", strArchiverActions, "RM_JS_Audit_KeepLatestVersionAndDestroyOther ", rule.KeepLatestMajorAndMinorVersion);
                    }
                    if (rule.DeleteRecords)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_RDM_CreateRule_Options_IncludeDeclaredFile ");
                    }

                    if (rule.DeleteToRecycleBin)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_RDM_CreateRule_Options_DeleteToRecycleBin ");
                    }

                    if (rule.IncludeDeleteRecordLabel)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_RDM_CreateRule_RecordsLabelOption ");
                        if (rule.LockRecordBeforeDestroy)
                        {
                            strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_RDM_CreateRule_LockRecordBeforeDestroy ");
                        }
                    }

                    if (rule.IsDeleteSiteCollectionToRecycleBin())
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_RDM_CreateRule_Options_DeleteSiteCollectionToRecycleBin ");
                    }

                    if ((keepDataOption & (int)KeepDataStatus.IsEnableRemoveRetentionLabel) == (int)KeepDataStatus.IsEnableRemoveRetentionLabel)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_Options_IncludeRetentionLabels"));
                    }
                }
                else if ((keepDataOption & (int)KeepDataStatus.TriggerMicrosoft365Archiving) == (int)KeepDataStatus.TriggerMicrosoft365Archiving)
                {
                    strArchiverActions = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_StoreInM365Archive");
                }
                else if (rule.TagContentInfo != null && rule.TagContentInfo.Any())
                {
                    strArchiverActions = BuildTagContentAuditAction(rule, keepDataOption, sourceFlag, isNewLogicAccount);
                }
                else
                {
                    strArchiverActions = "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove";
                    if ((keepDataOption & 256) != (int)KeepDataStatus.NotBackup && rule.ModelType != RuleModel.SOArchiver)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_RDM_CreateRule_BackupBeforeDestroying ");
                    }
                    if (rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_RDM_CreateRule_DeleteRelatedRecord ");
                    }
                    if (rule.DeleteRecords)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_RDM_CreateRule_Options_IncludeDeclaredFile ");
                    }
                    if (rule.DeleteToRecycleBin)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_RDM_CreateRule_Options_DeleteToRecycleBin ");
                    }

                    if (rule.IncludeDeleteRecordLabel)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_RDM_CreateRule_RecordsLabelOption ");
                        if (rule.LockRecordBeforeDestroy)
                        {
                            strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_RDM_CreateRule_LockRecordBeforeDestroy ");
                        }
                    }

                    if (rule.IsDeleteSiteCollectionToRecycleBin())
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_RDM_CreateRule_Options_DeleteSiteCollectionToRecycleBin ");
                    }

                    if ((keepDataOption & 128) == (int)KeepDataStatus.LinkToDocument)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_RDM_CreateRule_LeaveStubOption ");
                        if (rule.DeclareLinkFile)
                        {
                            strArchiverActions = string.Format("{0}; {1}", strArchiverActions, "RM_JS_RDM_CreateRule_Options_DeclareStub ");
                        }
                    }
                    if ((keepDataOption & (int)KeepDataStatus.IsEnableRemoveRetentionLabel) == (int)KeepDataStatus.IsEnableRemoveRetentionLabel)
                    {
                        strArchiverActions = string.Format("{0}; {1}", strArchiverActions, I18NEntity.GetString("RM_RDM_CreateRule_Options_IncludeRetentionLabels"));
                    }
                }
            }
            return strArchiverActions;
        }

        private static string BuildTagContentAuditAction(RMRuleInfos rule, int keepDataOption, SOSourceFlag sourceFlag, bool isNewLogicAccount)
        {
            var actionParts = new List<string>
            {
                GetTagOrKeepActionText(sourceFlag, isNewLogicAccount)
            };

            if ((keepDataOption & (int)KeepDataStatus.DeclareRecord) == (int)KeepDataStatus.DeclareRecord)
            {
                actionParts.Add(I18NEntity.GetString("RM_RDM_CreateRule_Options_DeclareDocumnet"));
            }

            actionParts.Add(I18NEntity.GetString(rule.RuleLevel == PolicyLevel.Folder
                ? "RM_RDM_CreateRule_Options_TagFolder"
                : "RM_RDM_CreateRule_Options_TagDocumnet"));

            var tagDetails = new List<string>();

            if (rule.TagContentInfo.Any(t => t.Type == TagContentInfoType.Archived))
            {
                tagDetails.Add(I18NEntity.GetString("RM_RDM_CreateRule_Options_Archived"));
            }

            if (rule.TagContentInfo.Any(t => t.Type == TagContentInfoType.ArchivedBy))
            {
                tagDetails.Add(I18NEntity.GetString("RM_RDM_CreateRule_Options_ArchivedBy"));
            }

            if (rule.TagContentInfo.Any(t => t.Type == TagContentInfoType.ArchivedDate))
            {
                tagDetails.Add(I18NEntity.GetString("RM_RDM_CreateRule_Options_ArchivedTime"));
            }

            var retentionLabelTag = rule.TagContentInfo.FirstOrDefault(t => t.Type == TagContentInfoType.RetentionLabel);
            if (retentionLabelTag != null)
            {
                tagDetails.Add(I18NEntity.GetString("RM_RDM_CreateRule_Options_Label"));

                if (retentionLabelTag.Option == (int)RetentionLabelOptions.GetFromGeneralSetting)
                {
                    tagDetails.Add(string.Format(
                        I18NEntity.GetString("RM_RDM_CreateRule_Options_LabelGetFromSetting"),
                        I18NEntity.GetString("RM_GS_Title")));
                }
                else if (!string.IsNullOrWhiteSpace(retentionLabelTag.Value))
                {
                    tagDetails.Add(retentionLabelTag.Value);
                }
            }

            if (!tagDetails.Any())
            {
                tagDetails.Add(I18NEntity.GetString(rule.RuleLevel == PolicyLevel.Folder
                    ? "RM_TM_Excel_Folder_DoTag"
                    : "RM_TM_Excel_DoTag"));
            }

            actionParts.AddRange(tagDetails);

            return string.Join(";", actionParts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        private static string GetTagOrKeepActionText(SOSourceFlag sourceFlag, bool isNewLogicAccount)
        {
            var actionKey = sourceFlag == SOSourceFlag.OneDrive
                ? (!DataCenterUtil.Is21V() && isNewLogicAccount
                    ? "RM_JS_RDM_CreateRule_Options_TagOrLock"
                    : "RM_JS_RDM_CreateRule_Options_ArchiveAndKeep_TagContent")
                : (sourceFlag == SOSourceFlag.SharePoint && !DataCenterUtil.Is21V() && isNewLogicAccount
                    ? "RM_JS_RDM_CreateRule_Options_TagOrLock"
                    : "RM_JS_RDM_CreateRule_Options_ArchiveAndKeep");

            return I18NEntity.GetString(actionKey);
        }

        public static int ExcludeOptionUnderMoveAction(int keepDataOption)
        {
            if ((keepDataOption & (int)KeepDataStatus.IsEnableRemoveRetentionLabel) == (int)KeepDataStatus.IsEnableRemoveRetentionLabel)
            {
                keepDataOption -= (int)KeepDataStatus.IsEnableRemoveRetentionLabel;
            }
            if ((keepDataOption & (int)KeepDataStatus.TriggerMicrosoft365Archiving) == (int)KeepDataStatus.TriggerMicrosoft365Archiving)
            {
                keepDataOption -= (int)KeepDataStatus.TriggerMicrosoft365Archiving;
            }
            return keepDataOption;
        }

        public static string getEXORuleAuditString(string targetSetting)
        {
            var result = "";
            if (!string.IsNullOrEmpty(targetSetting))
            {
                result = targetSetting.Remove(targetSetting.Length - 4);
            }
            return I18NEntity.GetString(result);
        }

        public static bool IsSystemStorage(string storageId)
        {
            try
            {
                if (storageId.Equals(RecordsConstants.AVEPOINT_DEFAULT_STORAGEID, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                var storageDeviceDto = StorageDeviceService.GetStorageDeviceById(storageId, needDecryptSecert: true);
                return storageDeviceDto != null && storageDeviceDto.IsSystemStorage;
            }
            catch
            {
                return false;
            }
        }
    }
}
