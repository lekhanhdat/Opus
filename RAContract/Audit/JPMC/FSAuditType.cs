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
using System;
using System.ComponentModel;

namespace AvePoint.RA.Contract.Audit.JPMC
{
    public enum FSAuditType
    {
        #region Connection Group & Connection (1 - 100)
        [Description("RM_FS_Monitoring_MoveFile")]
        MoveFile = 1,

        [Description("RM_FS_EditPermission")]
        PermissionChange = 2,

        [Description("RM_RC_Audit_Action_CreateFSGroup")]
        CreateFSGroup = 3,

        [Description("RM_RC_Audit_Action_EditFSGroup")]
        EditFSGroup = 4,

        //[Description("RM_RC_Audit_Action_DeleteFSGroup")]
        //DeleteFSGroup = 5,

        [Description("RM_RC_Audit_Action_CreateFSConnection")]
        CreateFSConnection = 6,

        [Description("RM_RC_Audit_Action_EditFSConnection")]
        EditFSConnection = 7,

        //[Description("RM_RC_Audit_Action_DeleteFSConnection")]
        //DeleteFSConnection = 8,

        //[Description("RM_RC_Audit_Action_FSConnectionCorrelateGroup")]
        //FSConnectionCorrelateGroup = 9,

        //[Description("RM_RC_Audit_Action_FSConnectionValidationTest")]
        //FSConnectionValidationTest = 10,
        #endregion

        #region Configuration (101 - 200)
        //[Description("RM_RC_Audit_Action_FSUniqueIDSetting")]
        //UniqueIdSetting = 101,

        [Description("RM_BCM_Audit_Action_FSDeactiveSetting")]
        FSDeactiveSetting = 102,

        [Description("RM_BCM_Audit_Action_FSActiveSetting")]
        FSActiveSetting = 103,

        [Description("RM_BCM_Audit_Action_ConfigureRecordOwnerSettings4FS")]
        FSEditLocationOwnersSetting = 104,

        [Description("RM_RC_Audit_Action_ConfigureInheritSetting4FS")]
        FSEditInheritSetting = 105,

        [Description("RM_BCM_Audit_Action_ConfigureClassCodeSettings4FS")]
        FSEditDocLevelSettingForJPMC = 106,

        [Description("RM_JS_JM_JobType_ApplyClassCode")]
        ApplyClassCodeSettings4FS = 107,

        [Description("RM_RC_Audit_Action_GeneralSetting4FS")]
        FSEditGeneralSettingForJPMC = 108,
        [Description("RM_JS_JM_JobType_MyhubClassify")]
        MyhubClassify =110,
        //[Description("RM_RC_Audit_Action_FSClassificationSetting")]
        //FSClassificationSetting = 109,
        #endregion

        #region Job (201 - 300)
        [Description("RM_RC_Audit_Action_RunCollectionJob4FS")]
        RunFSCollectionJob = 201,

        [Description("RM_RC_Audit_Action_RunDisposalJob4FS")]
        RunFSDisposalJob = 202,

        //[Description("RM_BCM_Audit_Action_RunFSManageHoldJob")]
        //RunFSManageHoldJob = 203,

        //[Description("RM_BCM_Audit_Action_RunFSReclassicfyJob")]
        //RunFSReclassicfyJob = 204,

        //[Description("RM_RC_Audit_Action_RunApplyClassCodeJob")]
        //RunFSApplyClassCodeJob = 205,

        [Description("RM_RC_Audit_Action_RunFSRestoreJob")]
        RunFSRestoreJob = 206,

        [Description("RM_RC_Audit_Action_ConfigureDisposalJob4FS")]
        ConfigureDisposalJobSchedule4FS = 207,

        //[Description("RM_RC_Audit_Action_RunDisposalJob4FS")]
        //RunEnforceRule = 208,

        [Description("RM_JS_FS_DisposalOnSpecificClassCode")]
        RunEnforceRuleWithClassCode = 209,

        //[Description("RM_BCM_Audit_Action_RunFSReclassicfyJob")]
        //Reclassify = 210,

        //[Description("RM_RC_Audit_Action_FSImportSetting")]
        //ImportSetting = 211,

        //[Description("RM_RC_Audit_Action_FSExportSetting")]
        //ExportSetting = 212,

        //[Description("RM_FS_Audit_Type_RunSyncJob")]
        //RunSyncJob = 213,

        [Description("RM_FS_DownloadRCCReport")]
        DownloadRCCReport = 214,

        [Description("RM_FS_Audit_Action_JpmcDownloadRCCReport")]
        JpmcDownloadRCCReport = 215,

        [Description("RM_FS_Audit_Action_DeleteRCCReport")]
        DeleteRCCReport = 216,

        [Description("RM_FS_Audit_Action_RunFSDashboardJob")]
        RunFSDashboardJob = 217,

        [Description("RM_RC_Audit_Action_MarkToApproved")]
        JpmcAuditApprove = 218,

        [Description("RM_RC_Audit_Action_MarkToRejected")]
        JpmcAuditReject = 219,

        [Description("RM_FS_Audit_Action_PauseApprovalProcess")]
        JpmcAuditPause = 220,

        [Description("RM_FS_Audit_Action_ResumeApprovalProcess")]
        JpmcAuditResume = 221,

        //[Description("RM_RC_Audit_Action_RunExportHistroyJob")]
        //RunExportHistroyJob = 222,

        [Description("RM_FS_Audit_Action_GenerateDisposalHistory")]
        GenerateDisposalHistory = 223,

        [Description("RM_FS_Audit_Action_DownloadDisposalHistory")]
        DownloadDisposalHistory = 224,

        [Description("RM_FS_Audit_Action_DeleteDisposalHistory")]
        DeleteDisposalHistory = 225,

        #endregion
    }

    public enum FSAuditLevel
    {
        [Description("Unknown")]
        Unknown = 0,

        [Description("RM_FS_Register_Tab_ConnectionGroup")]
        ConnectionGroup = 1,
        
        [Description("RM_FS_Register_Tab_Connections")]
        Connection = 2,
        
        [Description("RM_JS_RC_ActionAudit_ObjType_Folder")]
        Folder = 3,

        [Description("RM_JS_RC_ActionAudit_ObjType_File")]
        File = 4
    }

    public enum FSAuditExecutedBy
    {
        Unknown = 0,
        User = 1,
        System = 2
    }
}
