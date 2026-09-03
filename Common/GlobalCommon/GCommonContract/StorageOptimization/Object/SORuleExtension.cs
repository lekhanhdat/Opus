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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMRuleManageMent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Contract.StorageOptimization.Object
{
    public static class SORuleExtension
    {
        public static bool IsPhysicalMoveToRule(this Rule rule)
        {
            if (!string.IsNullOrWhiteSpace(rule?.PhysicalRule?.spMoveOption?.MoveDestination?.PhysicalTree?.BoxId)
                || !string.IsNullOrWhiteSpace(rule?.PhysicalRule?.spMoveOption?.MoveDestination?.PhysicalTree?.LocationId))
            {
                return true;
            }
            return false;
        }

        public static bool IsDeleteSiteCollectionToRecycleBin(this Rule rule, int sourceFlag = -1)
        {
            return rule.CanDeleteSiteCollection(sourceFlag) && rule.DeleteSiteCollectionToRecycleBin;
        }

        public static bool IsDeleteSiteCollectionPermanently(this Rule rule, int sourceFlag = -1)
        {
            return rule.CanDeleteSiteCollection(sourceFlag) && !rule.DeleteSiteCollectionToRecycleBin;
        }

        private static bool CanDeleteSiteCollection(this Rule rule, int sourceFlag)
        {
            return sourceFlag == (int)SourceFlag.SharePoint
                && (rule.PolicyLevel == PolicyLevel.SiteCollection || rule.PolicyLevel == PolicyLevel.Teams)
                && rule.IsRuleActionCanDeleteSC();
        }

        public static bool IsRuleActionCanDeleteSC(this Rule rule)
        {
            //string strArchiverActions = "";
            int keepDataOption = rule.KeepDataOption;
            var canDeleteSCAction = false;
            if (rule.PolicyLevel != PolicyLevel.SiteCollection)
            {
                return canDeleteSCAction;
            }
            else
            {
                if ((keepDataOption & (int)KeepDataStatus.ArchiverOnly) == (int)KeepDataStatus.ArchiverOnly)
                {
                    //strArchiverActions = "RM_JS_RDM_CreateRule_Options_Backup";
                }
                else if (rule.ExportInfo != null && rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)
                {
                    //strArchiverActions = "RM_JS_RDM_CreateRule_Options_ExportOnly";
                }
                else if ((keepDataOption & (int)KeepDataStatus.Delete) != (int)KeepDataStatus.Delete
                    && (keepDataOption & (int)KeepDataStatus.Remove) != (int)KeepDataStatus.Remove
                    && (keepDataOption & 128) != (int)KeepDataStatus.LinkToDocument
                    && (keepDataOption & 256) != (int)KeepDataStatus.NotBackup
                    && (keepDataOption & (int)KeepDataStatus.Vault) != (int)KeepDataStatus.Vault
                    && (keepDataOption & (int)KeepDataStatus.Archive) != (int)KeepDataStatus.Archive
                    && (keepDataOption & (int)KeepDataStatus.ArchiveAndLeaveStub) != (int)KeepDataStatus.ArchiveAndLeaveStub
                    && (keepDataOption & (int)KeepDataStatus.ArchiveBackupAndRemove) != (int)KeepDataStatus.ArchiveBackupAndRemove
                    && (keepDataOption & (int)KeepDataStatus.ArchiveBackupAndRemoveLeaveStub) != (int)KeepDataStatus.ArchiveBackupAndRemoveLeaveStub
                    && (keepDataOption & (int)KeepDataStatus.TriggerMicrosoft365Archiving) != (int)KeepDataStatus.TriggerMicrosoft365Archiving)
                {
                    //strArchiverActions = "RM_JS_RDM_CreateRule_Options_ArchiveAndKeep";
                }
                else if (ExcludeOptionUnderMoveAction(keepDataOption) == (int)KeepDataStatus.Delete && rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null)
                {
                    //strArchiverActions = "RM_JS_RDM_CreateRule_Options_MoveRecord";
                }
                else if ((keepDataOption & (int)KeepDataStatus.Archive) == (int)KeepDataStatus.Archive 
                    || (keepDataOption & (int)KeepDataStatus.ArchiveAndLeaveStub) == (int)KeepDataStatus.ArchiveAndLeaveStub)
                {
                    //strArchiverActions = "RM_RDM_CreateRule_ArchiveToAzureBlobStorage";
                }
                else if ((keepDataOption & (int)KeepDataStatus.ArchiveBackupAndRemove) == (int)KeepDataStatus.ArchiveBackupAndRemove 
                    || (keepDataOption & (int)KeepDataStatus.ArchiveBackupAndRemoveLeaveStub) == (int)KeepDataStatus.ArchiveBackupAndRemoveLeaveStub)
                {
                    //strArchiverActions = "RM_JS_RDM_CreateRule_Options_BackupAndRemove";
                    canDeleteSCAction = true;
                }
                else if ((keepDataOption & (int)KeepDataStatus.TriggerMicrosoft365Archiving) == (int)KeepDataStatus.TriggerMicrosoft365Archiving)
                {
                    //strArchiverActions = "RM_JS_RDM_CreateRule_Options_StoreInM365Archive";
                }
                else if (rule.TagContentInfo != null && rule.TagContentInfo.Any()) { }
                else if (keepDataOption == 20) { }
                else
                {
                    //strArchiverActions = "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove";
                    canDeleteSCAction = true;
                }
            }
            return canDeleteSCAction;
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
    }
}
