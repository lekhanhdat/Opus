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
using AvePoint.RA.I18N.Core;

namespace RAGoogle.Util
{
    public class I18NResource
    {
        #region information
        public static string ObjectLevelDrive = "RM_JS_SPS_TabLabel_Google";

        public static string ObjectLevelGoogleDrive = "RM_JS_Common_ReportType_GoogleDrive";

        public static string ObjectLevelFolder = "RM_JS_Rule_ObjectLevel_GoogleFolder";

        public static string ObjectLevelFile = "RM_JS_Rule_ObjectLevel_GoogleFile";

        public static string ObjectLevelGoogleDriveFileVersion = "RM_JS_Rule_ObjectLevel_GoogleDriveFileVersion";

        public static string ObjectLevelItemVersion = "RM_JS_Rule_ObjectLevel_ItemVersion";

        public static string WaitingForDisposal = "RM_JM_FSFileWaitingForApproval";

        public static string DeleteItemSuccessfully = "StorageOptimization.Service_A390D0EF-DA20-40D2-AF63-EB267BB96BE3";

        public static string NotYetDueDate = "RM_MA_NotYetDueDate";

        public static string FileOnHold = "StorageOptimization_EXOExploreHoldFile";

        public static string ReportSkipOnHold = "RM_FS_ReportSkip_OnHold";

        public static string DeleteItemFailed = "StorageOptimization_SOARCOMArchiverReportDtoAddDeletionCommonsItem";

        public static string MoveItemFailed = "RM_GoogleRule_MoveItemFailed";

        public static string RuleIsNotAvailable = "StorageOptimization_SOARSOArchiverRuleIsNotAvailable";

        public static string NoMatchedLabel = "RM_JM_JD_NoMatchedLabel"; // not support

        public static string ManualChooseLabel = "RM_JS_SPS_Label_AutoClassification_NoDefaultValue";

        public static string LabelAlreadyApplied = "RM_JM_JD_LabelAlreadyApplied"; // not support

        public static string LabelNoPermission = "RM_JM_JD_LabelNoPermission";

        public static string NoMatchedRule = "RM_MA_UpdateNoRule";

        public static string NewMatchedRule = "RM_MA_UpdateNewRule";

        public static string ItemHaveApprovalStatusIsNotNone = "RM_MA_ItemHaveApprovalStatus";

        public static string ManualSelectLabelOption = "RM_JM_JD_ManualSelectLabel";

        public static string DisableRecordsManagement = "RM_JS_BCM_ExportSetting_DisableState";

        public static string DefaultGoogleTermSet = "RM_TM_DefaultGoogleTermSet";
        public static string ExportItemFailed = "RM_GoogleRule_ExportItemFailed";

        public static string LabelLimitApplied = "RM_JM_JD_LabelLimitApplied";

        public static string UnsupportFile = "RM_JM_JD_UnsupportFile";

        #endregion

        #region job
        public static string GoogleApplySettings = "RM_JS_SPS_Apply_Google_Settings";
        #endregion

        #region action

        public static string RemoveAndDestroyAction = "RM_JMD_PD_DisposalAction_Dispose";
        public static string SkipAppliedLabel = "RM_JS_SPS_ApplySkipLabel";
        public static string MoveAction = "RM_JMD_PD_DisposalAction_Move";
        public static string ExportAction = "RM_JMD_PD_DisposalAction_Export";
        public const string ApplyAutoPopulate = "RM_JS_JMD_Action_SetAutoClassification";
        public const string ApplyDefault = "RM_SS_ApplyExist";
        public const string ApplyViaSmartTerm = "RM_JS_JMD_Action_SetAIClassification";
        public const string ApplySmartTermViaManual = "RM_JS_JMD_Action_SkipAIManualApproval";
        #endregion


        #region Exception

        public static string NoRecordOwner = "RM_MA_NoRecordOwner";

        public static string NotFoundSiteOwner = "RM_MA_NotFound_SiteOwner";

        public static string UnexpectedException = "RM_MA_Unexpected";

        public static string RuleIsDeleted = "RM_RDM_Rule_RuleIsDeleted";


        public static string NoWorkflow = "RM_MA_WF_NoWorkflow";

        public static string NoSupportSiteOwner = $"RM_MA_NoSupport_SiteOwner{I18NEntity.Separator}{"RM_JS_SPS_TabLabel_GoogleDrive"}";

        public static string LabelInvalidException = "RM_JM_JD_LabelInvalidError";

        public static string LabelInvalidOverwritePermissionException = "RM_JM_JD_LabelInvalidOverwritePermissionError";

        public static string InvalidUserPermission = "RM_JM_JD_UserActionInvalidPermission";

        public static string NotFoundDrive = "RM_JM_JD_NotFound_Drive";

        public static string MoveToSameDestination = "RM_JM_JD_MoveToSameDestination_Drive";
        public static string MoveToDifferentTenant = "RM_JM_JD_MoveToDifferentTenant_Drive";
        public static string UnexpectedError = "RM_JM_Details_Failed_UnexpectedError";
        public static string UnexpectedExtractFileContent = "RM_JM_Details_Failed_ExtractFileContentFaile";

        #endregion
    }
}
