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
using DocumentFormat.OpenXml.Office.CoverPageProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RABox.Util
{
    public class I18NResource
    {
        #region information
        public static string DataTypeBoxFolder = "RM_RDM_RecordDetails_DataType_BoxFolder";

        public static string DataTypeBoxFile = "RM_RDM_RecordDetails_DataType_BoxFile";

        public static string DataTypeBoxUser = "RM_RDM_RecordDetails_DataType_BoxUser";

        public static string ObjectLevelDocument = "RM_JS_Rule_ObjectLevel_Document";

        public static string ObjectLevelBoxFolder = "RM_JS_Rule_ObjectLevel_BoxFolder";

        public static string ObjectLevelBoxFile = "RM_JS_Rule_ObjectLevel_BoxFile";

        public static string BoxAnonymousUser = "RM_JS_BoxAnonymousUser";

        public static string DisposalExtendTime = "RM_RA_Extended";

        public static string WaitingForDisposal = "RM_JM_FSFileWaitingForApproval";

        public static string DeleteItemSuccessfully = "StorageOptimization.Service_A390D0EF-DA20-40D2-AF63-EB267BB96BE3";

        public static string FileOnHold = "StorageOptimization_EXOExploreHoldFile";

        public static string ReportSkipOnHold = "RM_FS_ReportSkip_OnHold";

        #endregion

        #region action

        public static string RemoveAndDestroyAction = "RM_JMD_PD_DisposalAction_Dispose";

        public static string ExplorerChangeTerm = "RM_JS_BCM_Explorer_ChangeTerm";

        public static string ExplorerChangeLabel = "RM_JS_BCM_Explorer_ChangeLabel";

        public static string AuditChangeTerm = "RM_BCM_Audit_Action_ChangeTerm";

        public static string UpdateNewRule = "RM_MA_UpdateNewRule";

        public static string UpdateNoRule = "RM_MA_UpdateNoRule";

        public static string NotYetDueDate = "RM_MA_NotYetDueDate";

        #endregion


        #region Exception

        public static string NoRecordOwner = "RM_MA_NoRecordOwner";

        public static string NotFoundSiteOwner = "RM_MA_NotFound_SiteOwner";

        public static string UnexpectedException = "RM_MA_Unexpected";

        public static string RuleIsDeleted = "RM_RDM_Rule_RuleIsDeleted";

        public static string ChangeTermFailed = "RM_JM_GlobalSearch_ChangeTermFailed";

        public static string AuditChangeTermErrorMessage = "RM_JS_Audit_ChangeTermErrorMessage";

        public static string TermIsInvalid = "RM_FS_DisposalDetail_TermIsInvalid";

        public static string DeleteItemFailed = "StorageOptimization_SOARCOMArchiverReportDtoAddDeletionCommonsItem";

        public static string RuleIsNotAvailable = "StorageOptimization_SOARSOArchiverRuleIsNotAvailable";

        public static string ItemIsLocked = "RM_JS_JM_ItemIsLocked";

        public static string NotReachRetentionExpiration = "RM_JS_JM_NotReachRetentionExpiration";

        public static string ItemUnderLegalHold = "RM_JS_JM_ItemUnderLegalHold";

        public static string ItemNotFound = "RM_Connector_ItemNotFound";

        public static string NeedResetPassword = "RM_JS_JM_NeedResetPassword";

        public static string NeedCompleteEmailConfirmation = "RM_JS_JM_NeedCompleteEmailConfirmation";

        public static string NoWorkflow = "RM_MA_WF_NoWorkflow";
        
        #endregion
    }
}
