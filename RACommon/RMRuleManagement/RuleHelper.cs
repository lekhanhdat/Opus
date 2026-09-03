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

using Amazon.Runtime.Internal.Transform;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Common.RMRuleManagement
{
    public class RuleHelper
    {

        #region old logic disposal action dics, New option Try not to use this
        private static Dictionary<int, List<string>> oldLogicFSDisposalActionDic = new Dictionary<int, List<string>>
        {
            {0, new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove"} },
            {1,new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndKeep"} },
            {2,new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_RDM_CreateRule_Options_LeaveStub_FS"} },
            {3, new List<string>(){"RM_JS_RDM_CreateRule_Options_MoveRecord_FS"} },
            {4, new List<string>(){"RM_JS_RDM_CreateRule_Options_MoveRecord_FS" ,"RM_JS_RDM_CreateRule_Options_Move_DeclareRecord"} },
            {5,new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove","RM_RDM_CreateRule_BackupBeforeDestroying"} },
            {7,new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_RDM_CreateRule_BackupBeforeDestroying" ,"RM_JS_RDM_CreateRule_Options_LeaveStub_FS"} },
            {8, new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeRelatedRecord"} },
            {10,new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeRelatedRecord" ,"RM_JS_RDM_CreateRule_Options_LeaveStub_FS"} },
            {13,new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeRelatedRecord" ,"RM_RDM_CreateRule_BackupBeforeDestroying"} },
            {15, new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeRelatedRecord" ,"RM_RDM_CreateRule_BackupBeforeDestroying" ,"RM_JS_RDM_CreateRule_Options_LeaveStub_FS"} },
            {16, new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeDeclaredFile"} },
            {18,new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeDeclaredFile" ,"RM_JS_RDM_CreateRule_Options_LeaveStub_FS"} },
            {21,new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeDeclaredFile" ,"RM_RDM_CreateRule_BackupBeforeDestroying"} },
            {23, new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeDeclaredFile" ,"RM_RDM_CreateRule_BackupBeforeDestroying" ,"RM_JS_RDM_CreateRule_Options_LeaveStub_FS"} },
            {24,new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeRelatedRecord" ,"RM_JS_Rule_Detail_IncludeDeclaredFile"} },
            {26, new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeRelatedRecord" ,"RM_JS_Rule_Detail_IncludeDeclaredFile" ,"RM_JS_RDM_CreateRule_Options_LeaveStub_FS"} },
            {29, new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeRelatedRecord" ,"RM_JS_Rule_Detail_IncludeDeclaredFile" ,"RM_RDM_CreateRule_BackupBeforeDestroying"} },
            {31,new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeRelatedRecord" ,"RM_JS_Rule_Detail_IncludeDeclaredFile" ,"RM_RDM_CreateRule_BackupBeforeDestroying" ,"RM_JS_RDM_CreateRule_Options_LeaveStub_FS"} },
            {64,new List<string>(){ "RM_JS_RDM_CreateRule_Options_ExportOnly" } },
            {99,new List<string>(){ "RM_JS_RDM_CreateRule_Options_None"} },
        };

        private static Dictionary<int, List<string>> oldLogicSPDisposalActionDic = new Dictionary<int, List<string>>()
        {
            { 0, new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" } },
            { 1,  new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndKeep"}},
            { 2,  new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" , "RM_JS_RDM_CreateRule_Options_LeaveStub"}},
            { 3, new List<string>(){ "RM_JS_RDM_CreateRule_Options_MoveRecord"}},
            { 4,  new List<string>(){"RM_JS_RDM_CreateRule_Options_MoveRecord", "RM_JS_RDM_CreateRule_Options_Move_DeclareRecord"}},
            { 5, new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" , "RM_RDM_CreateRule_BackupBeforeDestroying"}},
            { 7,  new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" , "RM_RDM_CreateRule_BackupBeforeDestroying", "RM_JS_RDM_CreateRule_Options_LeaveStub"}},
            { 8,  new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" , "RM_JS_Rule_Detail_IncludeRelatedRecord"}},
            { 10, new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove", "RM_JS_Rule_Detail_IncludeRelatedRecord" ,"RM_JS_RDM_CreateRule_Options_LeaveStub"}},
            { 11,  new List<string>(){"RM_JS_RDM_CreateRule_Options_MoveRecord" , "RM_JS_BCM_Rule_Move_IsRemoveEmail"}},
            { 13,  new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" , "RM_JS_Rule_Detail_IncludeRelatedRecord","RM_RDM_CreateRule_BackupBeforeDestroying"}},
            { 15,  new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove", "RM_JS_Rule_Detail_IncludeRelatedRecord" ,"RM_RDM_CreateRule_BackupBeforeDestroying" ,"RM_JS_RDM_CreateRule_Options_LeaveStub"}},
            { 16,  new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" , "RM_JS_Rule_Detail_IncludeDeclaredFile"}},
            { 18,  new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeDeclaredFile" ,"RM_JS_RDM_CreateRule_Options_LeaveStub"}},
            { 19,  new List<string>(){"RM_JS_RDM_CreateRule_Options_MoveRecord" ,"RM_JS_BCM_Rule_Move_IsReclassify"}},
            { 20,  new List<string>(){"RM_JS_RDM_CreateRule_Options_MoveRecord" , "RM_JS_RDM_CreateRule_Options_Move_DeclareRecord", "RM_JS_BCM_Rule_Move_IsReclassify"}},
            { 21,  new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeDeclaredFile" ,"RM_RDM_CreateRule_BackupBeforeDestroying"}},
            { 23,  new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeDeclaredFile" ,"RM_RDM_CreateRule_BackupBeforeDestroying" ,"RM_JS_RDM_CreateRule_Options_LeaveStub"}},
            { 24,  new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeRelatedRecord" ,"RM_JS_Rule_Detail_IncludeDeclaredFile"}},
            { 25, new List<string>(){ "RM_RDM_CreateRule_ArchiveToAzureBlobStorage"}},
            { 26,  new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeRelatedRecord" ,"RM_JS_Rule_Detail_IncludeDeclaredFile" ,"RM_JS_RDM_CreateRule_Options_LeaveStub"}},
            { 27, new List<string>(){ "RM_JS_RDM_CreateRule_Options_MoveRecord" ,"RM_JS_BCM_Rule_Move_IsRemoveEmail" ,"RM_JS_BCM_Rule_Move_IsReclassify"}},
            { 28,  new List<string>(){ "RM_RDM_CreateRule_ArchiveToAzureBlobStorage", "RM_JS_RDM_CreateRule_Options_LeaveStub"}},
            { 29, new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeRelatedRecord" ,"RM_JS_Rule_Detail_IncludeDeclaredFile" ,"RM_RDM_CreateRule_BackupBeforeDestroying"}},
            { 31, new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeRelatedRecord" ,"RM_JS_Rule_Detail_IncludeDeclaredFile" ,"RM_RDM_CreateRule_BackupBeforeDestroying" ,"RM_JS_RDM_CreateRule_Options_LeaveStub"}},
            { 40,  new List<string>(){"RM_JS_RDM_CreateRule_Options_MoveRecord", "RM_JS_RDM_CreateRule_Options_Move_DeclareRecord", "RM_JS_RDM_CreateRule_Options_Move_AllVersions"}},
            { 41,  new List<string>(){"RM_JS_RDM_CreateRule_Options_MoveRecord" ,"RM_JS_RDM_CreateRule_Options_Move_AllVersions"}},
            { 42,  new List<string>(){"RM_JS_RDM_CreateRule_Options_MoveRecord" , "RM_JS_RDM_CreateRule_Options_Move_DeclareRecord" , "RM_JS_RDM_CreateRule_Options_Move_FolderStructure", "RM_JS_RDM_CreateRule_Options_Move_AllVersions"}},
            { 43,  new List<string>(){"RM_JS_RDM_CreateRule_Options_MoveRecord" , "RM_JS_RDM_CreateRule_Options_Move_DeclareRecord" , "RM_JS_RDM_CreateRule_Options_Move_FolderStructure"}},
            { 44,  new List<string>(){"RM_JS_RDM_CreateRule_Options_MoveRecord" , "RM_JS_RDM_CreateRule_Options_Move_FolderStructure", "RM_JS_RDM_CreateRule_Options_Move_AllVersions"}},
            { 45,  new List<string>(){"RM_JS_RDM_CreateRule_Options_MoveRecord" , "RM_JS_RDM_CreateRule_Options_Move_FolderStructure"}},
            { 64,  new List<string>(){"RM_JS_RDM_CreateRule_Options_ExportOnly"}},
            { 130, new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_RDM_CreateRule_Options_LeaveStub" ,"RM_JS_RDM_CreateRule_Options_DeclareStub" }},
            { 135, new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove","RM_RDM_CreateRule_BackupBeforeDestroying" ,"RM_JS_RDM_CreateRule_Options_LeaveStub","RM_JS_RDM_CreateRule_Options_DeclareStub" }},
            { 138, new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove","RM_JS_Rule_Detail_IncludeRelatedRecord" ,"RM_JS_RDM_CreateRule_Options_LeaveStub" ,"RM_JS_RDM_CreateRule_Options_DeclareStub" }},
            { 143, new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove","RM_JS_Rule_Detail_IncludeRelatedRecord","RM_RDM_CreateRule_BackupBeforeDestroying" ,"RM_JS_RDM_CreateRule_Options_LeaveStub" ,"RM_JS_RDM_CreateRule_Options_DeclareStub" }},
            { 146, new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove","RM_JS_Rule_Detail_IncludeDeclaredFile" ,"RM_JS_RDM_CreateRule_Options_LeaveStub" ,"RM_JS_RDM_CreateRule_Options_DeclareStub" }},
            { 151, new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove","RM_JS_Rule_Detail_IncludeDeclaredFile","RM_RDM_CreateRule_BackupBeforeDestroying" ,"RM_JS_RDM_CreateRule_Options_LeaveStub","RM_JS_RDM_CreateRule_Options_DeclareStub" }},
            { 154, new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove","RM_JS_Rule_Detail_IncludeRelatedRecord","RM_JS_Rule_Detail_IncludeDeclaredFile" ,"RM_JS_RDM_CreateRule_Options_LeaveStub" ,"RM_JS_RDM_CreateRule_Options_DeclareStub" }},
            { 156, new List<string>(){"RM_RDM_CreateRule_ArchiveToAzureBlobStorage","RM_JS_RDM_CreateRule_Options_LeaveStub" ,"RM_JS_RDM_CreateRule_Options_DeclareStub" }},
            { 159, new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove","RM_JS_Rule_Detail_IncludeRelatedRecord" ,"RM_JS_Rule_Detail_IncludeDeclaredFile" ,"RM_RDM_CreateRule_BackupBeforeDestroying" ,"RM_JS_RDM_CreateRule_Options_LeaveStub" ,"RM_JS_RDM_CreateRule_Options_DeclareStub" }},
            { 99,  new List<string>(){"RM_JS_RDM_CreateRule_Options_None"}},
            { 4096,  new List<string>(){"RM_JS_RDM_CreateRule_Options_BackupAndRemove"}},
            { 8192,  new List<string>(){"RM_JS_RDM_CreateRule_Options_BackupAndRemove","RM_JS_RDM_CreateRule_Options_LeaveStub"}},
        };

        private static Dictionary<int, List<string>> oldLogicGoogleDisposalActionDic = new Dictionary<int, List<string>>
        {
            {0, new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove"} },
            {99,new List<string>(){ "RM_JS_RDM_CreateRule_Options_None"} },
        };

        private static Dictionary<int, List<string>> oldLogicPhysicalDisposalActionDic = new Dictionary<int, List<string>>()
        {
            {0, new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove"} },
            {1,new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndKeep"} },
            {2, new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_RDM_CreateRule_Options_LeaveStub"} },
            {3, new List<string>(){"RM_JS_RDM_CreateRule_Options_MoveLocation"} },
            {99, new List<string>(){"RM_JS_RDM_CreateRule_Options_None"} },
            {4, new List<string>(){"RM_JS_RDM_CreateRule_Options_MoveRecord" ,"RM_JS_RDM_CreateRule_Options_Move_DeclareRecord"} },
            {8,new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeRelatedRecord"} },
            {10,new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeRelatedRecord" ,       "RM_JS_RDM_CreateRule_Options_LeaveStub"} },
            {16,new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeDeclaredFile"} },
            {18,new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeDeclaredFile" ,       "RM_JS_RDM_CreateRule_Options_LeaveStub"} },
            {24,new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeRelatedRecord" ,       "RM_JS_Rule_Detail_IncludeDeclaredFile"} },
            {26,new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeRelatedRecord" ,       "RM_JS_Rule_Detail_IncludeDeclaredFile" ,"RM_JS_RDM_CreateRule_Options_LeaveStub"} },
            {32,new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_RDM_CreateRule_DestroyEmptyBox"} },
            {40, new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeRelatedRecord" ,       "RM_RDM_CreateRule_DestroyEmptyBox"} },
            {64, new List<string>(){"RM_JS_RDM_CreateRule_Options_ExportOnly"} },
        };

        private static Dictionary<int, List<string>> oldLogicBoxDisposalActionDic = new Dictionary<int, List<string>>
        {
            {0, new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove"} },
            {1,new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndKeep"} },
            {2, new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeRelatedRecord"} },
            {3, new List<string>(){"RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeDeclaredFile"} },
            {4,new List<string>(){ "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove" ,"RM_JS_Rule_Detail_IncludeRelatedRecord" ,"RM_JS_Rule_Detail_IncludeDeclaredFile" } },
            {5,new List<string>(){ "RM_JS_RDM_CreateRule_Options_ExportOnly" } },
            {99,new List<string>(){ "RM_JS_RDM_CreateRule_Options_None"} },
        };
        #endregion

        private static Dictionary<int, string> newLogicDisposalActionI18nMap = new Dictionary<int, string>()
        {
            #region main option
            {(int)RMContentDisposalAction.NewLogicArchvie, "RM_RDM_CreateRule_ArchiveToAzureBlobStorage"},
            {(int)RMContentDisposalAction.ArchiverOnly, "RM_JS_RDM_CreateRule_Options_Backup" },
            {(int)RMContentDisposalAction.TriggerMicrosoft365ArchivingData, "RM_JS_RDM_CreateRule_Options_StoreInM365Archive" },
            
            #endregion

            #region sub option
            {(int)RMContentDisposalAction.NewDeclaredRecords, "RM_JS_Rule_Detail_IncludeDeclaredFile"},
            {(int)RMContentDisposalAction.IsEnableRemoveRetentionLabel, "RM_RDM_CreateRule_Options_IncludeRetentionLabels"},
            {(int)RMContentDisposalAction.ArchiveOnlyLastestVersion, "RM_JS_Rule_ArchiveVersionAndDestroyFile" },
            #endregion
        };

        private static HashSet<int> newLogicMainOptionSet = new HashSet<int>
        {

            (int)RMContentDisposalAction.NewLogicArchvie,
            (int)RMContentDisposalAction.ArchiverOnly,
            (int)RMContentDisposalAction.TriggerMicrosoft365ArchivingData
        };

        public static HashSet<int> newLogicKeepDataStatusList = new HashSet<int>()
        {
            (int)KeepDataStatus.IsEnableRemoveRetentionLabel,
            (int)KeepDataStatus.ArchiverOnly,
            (int)KeepDataStatus.ArchiveOnlyLastestVersion
        };

        #region old logic, In the old logic, equal is used to check option in many places, so it is difficult to expand when different permutations and combinations are added and compared, please refer to new logic in under
        public static RMContentDisposalAction GetOperationType(Rule rule)
        {
            if (rule == null)
            {
                return RMContentDisposalAction.None;
            }
            int keepDataOption = GetOldLogicKeepDataStatusOption(rule.KeepDataOption);
            if (rule.ExportInfo != null && rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive && rule.ExportType != GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None)
            {
                return RMContentDisposalAction.ExportOnly;
            }
            else if ((keepDataOption & (int)KeepDataStatus.LinkToDocument) == (int)KeepDataStatus.LinkToDocument)
            {
                return RMContentDisposalAction.LeaveStub;
            }
            else if ((keepDataOption & (int)KeepDataStatus.Keep) == (int)KeepDataStatus.Keep)
            {
                return RMContentDisposalAction.KeepData;
            }
            else if (keepDataOption == (int)KeepDataStatus.Delete && rule.MoveToRecordCenterAndDelareSetting != null && rule.MoveToRecordCenterAndDelareSetting.DestinationLocation != null)
            {
                //DelaredRecord为false时，是Declare，否则不是Declare，这样设计是为了升级兼容老数据
                if (!rule.MoveToRecordCenterAndDelareSetting.DelaredRecord)
                {
                    return RMContentDisposalAction.MoveDeclare;
                }
                return RMContentDisposalAction.Move;
            }
            else if (keepDataOption == (int)KeepDataStatus.Archive || keepDataOption == (int)KeepDataStatus.ArchiveAndLeaveStub)
            {
                if ((keepDataOption & (int)KeepDataStatus.ArchiveAndLeaveStub) == (int)KeepDataStatus.ArchiveAndLeaveStub)
                {
                    return RMContentDisposalAction.ArchiveToStorageAndLeaveStub;
                }
                else
                {
                    return RMContentDisposalAction.ArchiveToStorage;
                }
            }
            //else if (keepDataOption == (int)KeepDataStatus.Delete && rule.spMoveOption != null && !rule.spMoveOption.MoveDestination.NotDeclareMovedData)
            //{
            //    return RMContentDisposalAction.MoveDeclare;
            //}
            //else if (keepDataOption == (int)KeepDataStatus.Delete && rule.spMoveOption != null && rule.spMoveOption.MoveDestination.NotDeclareMovedData)
            //{
            //    return RMContentDisposalAction.Move;
            //}
            else
            {
                return RMContentDisposalAction.Remove;
            }
        }

        public static bool IsRemoveRule(Rule tempRule, int sourceFlag)
        {
            var result = false;
            var action = -1;
            if ((int)SourceFlag.SharePoint == sourceFlag || (int)SourceFlag.OneDrive == sourceFlag || (int)SourceFlag.Teams == sourceFlag)
            {
                action = (int)GetOperationTypeForSP(tempRule, GetOldLogicKeepDataStatusOption(tempRule.KeepDataOption));
                if (action == 0 || action == 2 || action == 5 || action == 7 || action == 8
                || action == 10 || action == 13 || action == 15 || action == 16 || action == 18
                || action == 21 || action == 23 || action == 24 || action == 25 || action == 26
                || action == 28 || action == 29 || action == 31)
                {
                    result = true;
                }
            }
            if ((int)SourceFlag.Exchange == sourceFlag)
            {
                action = (int)GetOperationTypeForEXO(tempRule.EXORule);
                if (action == 0)
                {
                    result = true;
                }
            }
            //if (action == 0 || action == 2 || action == 8
            //    || action == 10 || action == 16 || action == 18 || action == 24 || action == 25 || action == 26 ||action == 28)
            //{
            //    result = true;
            //}
            return result;
        }

        private static int GetOperationTypeForSP(Rule rule, int keepDataOption)//old logic
        {
            if (rule == null)
            {
                return (int)RMContentDisposalAction.None;
            }
            if (rule.ExportInfo != null && rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive && rule.ExportType != GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None)
            {
                return (int)RMContentDisposalAction.ExportOnly;
            }
            else if ((keepDataOption & (int)KeepDataStatus.LinkToDocument) == (int)KeepDataStatus.LinkToDocument)
            {
                //四个都勾选-------Leave Stub + Include Related + Include Declare + Archive
                if (rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both && rule.DeleteRecords && !((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    if (rule.DeclareLinkFile)
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords) | (int)RMContentDisposalAction.DeclaredRecords | (int)RMContentDisposalAction.Archive | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords) | (int)RMContentDisposalAction.DeclaredRecords | (int)RMContentDisposalAction.Archive;
                    }
                }
                //勾选三个的情况：
                //1.Leave Stub + Include Related + Include Declare
                //2.Leave Stub + Include Related + Archive
                //3.Leave Stub + Include Declare + Archive
                else if (rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both && rule.DeleteRecords)
                {
                    if (rule.DeclareLinkFile)
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords) | (int)RMContentDisposalAction.DeclaredRecords | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords) | (int)RMContentDisposalAction.DeclaredRecords;
                    }
                }
                else if (rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both && !((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    if (rule.DeclareLinkFile)
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords) | (int)RMContentDisposalAction.Archive | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords) | (int)RMContentDisposalAction.Archive;
                    }
                }
                else if (rule.DeleteRecords && !((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    if (rule.DeclareLinkFile)
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.DeclaredRecords) | (int)RMContentDisposalAction.Archive | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.DeclaredRecords) | (int)RMContentDisposalAction.Archive;
                    }
                }
                //勾选两个的情况：
                //1.Leave Stub + Include Related
                //2.Leave Stub + Include Declare
                //3.Leave Stub + Archive
                else if (rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both)
                {
                    if (rule.DeclareLinkFile)
                    {
                        return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords;
                    }
                }
                else if (rule.DeleteRecords)
                {
                    if (rule.DeclareLinkFile)
                    {
                        return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.DeclaredRecords | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.DeclaredRecords;
                    }
                }
                else if (!((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    if (rule.DeclareLinkFile)
                    {
                        return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.Archive | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.Archive;
                    }
                }
                //只勾选Leave Stub
                if (rule.DeclareLinkFile)
                {
                    return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.DeclareLinkFile;
                }
                else
                {
                    return (int)RMContentDisposalAction.LeaveStub;
                }
            }
            else if ((keepDataOption & (int)KeepDataStatus.Keep) == (int)KeepDataStatus.Keep)
            {
                return (int)RMContentDisposalAction.KeepData;
            }
            else if ((keepDataOption & (int)KeepDataStatus.TriggerMicrosoft365Archiving) == (int)KeepDataStatus.TriggerMicrosoft365Archiving)
            {
                return (int)RMContentDisposalAction.TriggerMicrosoft365ArchivingData;
            }
            else if (keepDataOption == (int)KeepDataStatus.Delete && rule.MoveToRecordCenterAndDelareSetting != null && rule.MoveToRecordCenterAndDelareSetting.DestinationLocation != null)
            {
                //DelaredRecord为false时，是Declare，否则不是Declare，这样设计是为了升级兼容老数据
                if (!rule.MoveToRecordCenterAndDelareSetting.DelaredRecord)
                {
                    if (rule.spMoveOption.MoveDestination.KeepSourceClassification)
                    {
                        return (int)RMContentDisposalAction.MoveDeclareWithKeepClassfication;
                    }
                    else if (rule.spMoveOption.MoveDestination.KeepFolderStructure)
                    {
                        if (rule.spMoveOption.MoveDestination.IsMoveVersions)
                        {
                            return (int)RMContentDisposalAction.MoveDeclareStructureWithAllVersions;
                        }
                        else
                        {
                            return (int)RMContentDisposalAction.MoveDeclareWithStructure;
                        }
                    }
                    else if (rule.spMoveOption.MoveDestination.IsMoveVersions)
                    {
                        return (int)RMContentDisposalAction.MoveDeclareWithAllVersions;
                    }
                    else
                    {
                        return (int)RMContentDisposalAction.MoveDeclare;
                    }
                }
                else
                {
                    if (rule.spMoveOption.MoveDestination.KeepSourceClassification)
                    {
                        return (int)RMContentDisposalAction.MoveWithKeepClassfication;
                    }
                    else if (rule.spMoveOption.MoveDestination.KeepFolderStructure)
                    {
                        if (rule.spMoveOption.MoveDestination.IsMoveVersions)
                        {
                            return (int)RMContentDisposalAction.MoveStructureWithAllVersions;
                        }
                        else
                        {
                            return (int)RMContentDisposalAction.MoveWithStructure;
                        }
                    }
                    else if (rule.spMoveOption.MoveDestination.IsMoveVersions)
                    {
                        return (int)RMContentDisposalAction.MoveWithAllVersions;
                    }
                    else
                    {
                        return (int)RMContentDisposalAction.Move;
                    }
                }
            }
            else if (keepDataOption == (int)KeepDataStatus.Delete && rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null && !rule.spMoveOption.MoveDestination.NotDeclareMovedData)
            {
                if (rule.spMoveOption.MoveDestination.KeepSourceClassification)
                {
                    return (int)RMContentDisposalAction.MoveDeclareWithKeepClassfication;
                }
                else if (rule.spMoveOption.MoveDestination.KeepFolderStructure)
                {
                    if (rule.spMoveOption.MoveDestination.IsMoveVersions)
                    {
                        return (int)RMContentDisposalAction.MoveDeclareStructureWithAllVersions;
                    }
                    else
                    {
                        return (int)RMContentDisposalAction.MoveDeclareWithStructure;
                    }
                }
                else if (rule.spMoveOption.MoveDestination.IsMoveVersions)
                {
                    return (int)RMContentDisposalAction.MoveDeclareWithAllVersions;
                }
                else
                {
                    return (int)RMContentDisposalAction.MoveDeclare;
                }
            }
            else if (keepDataOption == (int)KeepDataStatus.Delete && rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null && rule.spMoveOption.MoveDestination.NotDeclareMovedData)
            {
                if (rule.spMoveOption.MoveDestination.KeepSourceClassification)
                {
                    return (int)RMContentDisposalAction.MoveWithKeepClassfication;
                }
                else if (rule.spMoveOption.MoveDestination.KeepFolderStructure)
                {
                    if (rule.spMoveOption.MoveDestination.IsMoveVersions)
                    {
                        return (int)RMContentDisposalAction.MoveStructureWithAllVersions;
                    }
                    else
                    {
                        return (int)RMContentDisposalAction.MoveWithStructure;
                    }
                }
                else if (rule.spMoveOption.MoveDestination.IsMoveVersions)
                {
                    return (int)RMContentDisposalAction.MoveWithAllVersions;
                }
                else
                {
                    return (int)RMContentDisposalAction.Move;
                }
            }
            else if (keepDataOption == (int)KeepDataStatus.Archive || keepDataOption == (int)KeepDataStatus.ArchiveAndLeaveStub)
            {
                if ((keepDataOption & (int)KeepDataStatus.ArchiveAndLeaveStub) == (int)KeepDataStatus.ArchiveAndLeaveStub)
                {
                    if (rule.DeclareLinkFile)
                    {
                        return (int)RMContentDisposalAction.ArchiveToStorageAndLeaveStub | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return (int)RMContentDisposalAction.ArchiveToStorageAndLeaveStub;
                    }
                }
                else
                {
                    return (int)RMContentDisposalAction.ArchiveToStorage;
                }
            }
            else if (keepDataOption == (int)KeepDataStatus.ArchiveBackupAndRemove || keepDataOption == (int)KeepDataStatus.ArchiveBackupAndRemoveLeaveStub)
            {
                if (keepDataOption == (int)KeepDataStatus.ArchiveBackupAndRemoveLeaveStub)
                {
                    return (int)RMContentDisposalAction.ArchiveBackupAndRemoveLeaveStub;
                }
                else
                {
                    return (int)RMContentDisposalAction.BackupAndRemove;
                }
            }
            else
            {
                //三者都勾选 Include Related + Include Declare + Archive
                if (rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both && rule.DeleteRecords && !((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    return (int)RMContentDisposalAction.RelatedRecords | (int)RMContentDisposalAction.DeclaredRecords | (int)RMContentDisposalAction.Archive;
                }
                //勾选两个的情况：
                //1.Include Related + Include Declare
                //2.Include Related + Archive
                //3.Include Declare + Archive
                else if (rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both && rule.DeleteRecords)
                {
                    return (int)RMContentDisposalAction.RelatedRecords | (int)RMContentDisposalAction.DeclaredRecords;
                }
                else if (rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both && !((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    return (int)RMContentDisposalAction.RelatedRecords | (int)RMContentDisposalAction.Archive;
                }
                else if (rule.DeleteRecords && !((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    return (int)RMContentDisposalAction.DeclaredRecords | (int)RMContentDisposalAction.Archive;
                }
                //勾选一个的情况：
                //1.Include Related 
                //2.Include Declare
                //3.Archive
                else if (rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both)
                {
                    return (int)RMContentDisposalAction.RelatedRecords;
                }
                else if (rule.DeleteRecords)
                {
                    return (int)RMContentDisposalAction.DeclaredRecords;
                }
                else if (!((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    return (int)RMContentDisposalAction.Archive;
                }
                //什么都不勾
                return (int)RMContentDisposalAction.Remove;
            }
        }

        

        private static int GetOperationTypeForOneDrive(Rule rule, int keepDataOption)
        {
            if (rule == null)
            {
                return (int)RMContentDisposalAction.None;
            }
            if (rule.ExportInfo != null && rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive && rule.ExportType != GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None)
            {
                return (int)RMContentDisposalAction.ExportOnly;
            }
            else if ((keepDataOption & (int)KeepDataStatus.LinkToDocument) == (int)KeepDataStatus.LinkToDocument)
            {
                //四个都勾选-------Leave Stub + Include Related + Include Declare + Archive
                if (rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both && rule.DeleteRecords && !((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    if (rule.DeclareLinkFile)
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords) | (int)RMContentDisposalAction.DeclaredRecords | (int)RMContentDisposalAction.Archive | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords) | (int)RMContentDisposalAction.DeclaredRecords | (int)RMContentDisposalAction.Archive;
                    }
                }
                //勾选三个的情况：
                //1.Leave Stub + Include Related + Include Declare
                //2.Leave Stub + Include Related + Archive
                //3.Leave Stub + Include Declare + Archive
                else if (rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both && rule.DeleteRecords)
                {
                    if (rule.DeclareLinkFile)
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords) | (int)RMContentDisposalAction.DeclaredRecords | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords) | (int)RMContentDisposalAction.DeclaredRecords;
                    }
                }
                else if (rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both && !((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    if (rule.DeclareLinkFile)
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords) | (int)RMContentDisposalAction.Archive | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords) | (int)RMContentDisposalAction.Archive;
                    }
                }
                else if (rule.DeleteRecords && !((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    if (rule.DeclareLinkFile)
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.DeclaredRecords) | (int)RMContentDisposalAction.Archive | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.DeclaredRecords) | (int)RMContentDisposalAction.Archive;
                    }
                }
                //勾选两个的情况：
                //1.Leave Stub + Include Related
                //2.Leave Stub + Include Declare
                //3.Leave Stub + Archive
                else if (rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both)
                {
                    if (rule.DeclareLinkFile)
                    {
                        return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords;
                    }
                }
                else if (rule.DeleteRecords)
                {
                    if (rule.DeclareLinkFile)
                    {
                        return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.DeclaredRecords | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.DeclaredRecords;
                    }
                }
                else if (!((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    if (rule.DeclareLinkFile)
                    {
                        return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.Archive | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.Archive;
                    }
                }
                //只勾选Leave Stub
                if (rule.DeclareLinkFile)
                {
                    return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.DeclareLinkFile;
                }
                else
                {
                    return (int)RMContentDisposalAction.LeaveStub;
                }
            }
            else if ((keepDataOption & (int)KeepDataStatus.Keep) == (int)KeepDataStatus.Keep)
            {
                return (int)RMContentDisposalAction.KeepData;
            }
            else if (keepDataOption == (int)KeepDataStatus.Delete && rule.MoveToRecordCenterAndDelareSetting != null && rule.MoveToRecordCenterAndDelareSetting.DestinationLocation != null)
            {
                //DelaredRecord为false时，是Declare，否则不是Declare，这样设计是为了升级兼容老数据
                if (!rule.MoveToRecordCenterAndDelareSetting.DelaredRecord)
                {
                    if (rule.spMoveOption.MoveDestination.KeepSourceClassification)
                    {
                        return (int)RMContentDisposalAction.MoveDeclareWithKeepClassfication;
                    }
                    else if (rule.spMoveOption.MoveDestination.KeepFolderStructure)
                    {
                        if (rule.spMoveOption.MoveDestination.IsMoveVersions)
                        {
                            return (int)RMContentDisposalAction.MoveDeclareStructureWithAllVersions;
                        }
                        else
                        {
                            return (int)RMContentDisposalAction.MoveDeclareWithStructure;
                        }
                    }
                    else if (rule.spMoveOption.MoveDestination.IsMoveVersions)
                    {
                        return (int)RMContentDisposalAction.MoveDeclareWithAllVersions;
                    }
                    else
                    {
                        return (int)RMContentDisposalAction.MoveDeclare;
                    }
                }
                else
                {
                    if (rule.spMoveOption.MoveDestination.KeepSourceClassification)
                    {
                        return (int)RMContentDisposalAction.MoveWithKeepClassfication;
                    }
                    else if (rule.spMoveOption.MoveDestination.KeepFolderStructure)
                    {
                        if (rule.spMoveOption.MoveDestination.IsMoveVersions)
                        {
                            return (int)RMContentDisposalAction.MoveStructureWithAllVersions;
                        }
                        else
                        {
                            return (int)RMContentDisposalAction.MoveWithStructure;
                        }
                    }
                    else if (rule.spMoveOption.MoveDestination.IsMoveVersions)
                    {
                        return (int)RMContentDisposalAction.MoveWithAllVersions;
                    }
                    else
                    {
                        return (int)RMContentDisposalAction.Move;
                    }
                }
            }
            else if (keepDataOption == (int)KeepDataStatus.Delete && rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null && !rule.spMoveOption.MoveDestination.NotDeclareMovedData)
            {
                if (rule.spMoveOption.MoveDestination.KeepSourceClassification)
                {
                    return (int)RMContentDisposalAction.MoveDeclareWithKeepClassfication;
                }
                else if (rule.spMoveOption.MoveDestination.KeepFolderStructure)
                {
                    if (rule.spMoveOption.MoveDestination.IsMoveVersions)
                    {
                        return (int)RMContentDisposalAction.MoveDeclareStructureWithAllVersions;
                    }
                    else
                    {
                        return (int)RMContentDisposalAction.MoveDeclareWithStructure;
                    }
                }
                else if (rule.spMoveOption.MoveDestination.IsMoveVersions)
                {
                    return (int)RMContentDisposalAction.MoveDeclareWithAllVersions;
                }
                else
                {
                    return (int)RMContentDisposalAction.MoveDeclare;
                }
            }
            else if (keepDataOption == (int)KeepDataStatus.Delete && rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null && rule.spMoveOption.MoveDestination.NotDeclareMovedData)
            {
                if (rule.spMoveOption.MoveDestination.KeepSourceClassification)
                {
                    return (int)RMContentDisposalAction.MoveWithKeepClassfication;
                }
                else if (rule.spMoveOption.MoveDestination.KeepFolderStructure)
                {
                    if (rule.spMoveOption.MoveDestination.IsMoveVersions)
                    {
                        return (int)RMContentDisposalAction.MoveStructureWithAllVersions;
                    }
                    else
                    {
                        return (int)RMContentDisposalAction.MoveWithStructure;
                    }
                }
                else if (rule.spMoveOption.MoveDestination.IsMoveVersions)
                {
                    return (int)RMContentDisposalAction.MoveWithAllVersions;
                }
                else
                {
                    return (int)RMContentDisposalAction.Move;
                }
            }
            else if (keepDataOption == (int)KeepDataStatus.Archive || keepDataOption == (int)KeepDataStatus.ArchiveAndLeaveStub)
            {
                if ((keepDataOption & (int)KeepDataStatus.ArchiveAndLeaveStub) == (int)KeepDataStatus.ArchiveAndLeaveStub)
                {
                    if (rule.DeclareLinkFile)
                    {
                        return (int)RMContentDisposalAction.ArchiveToStorageAndLeaveStub | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return (int)RMContentDisposalAction.ArchiveToStorageAndLeaveStub;
                    }
                }
                else
                {
                    return (int)RMContentDisposalAction.ArchiveToStorage;
                }
            }
            else
            {
                //三者都勾选 Include Related + Include Declare + Archive
                if (rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both && rule.DeleteRecords && !((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    return (int)RMContentDisposalAction.RelatedRecords | (int)RMContentDisposalAction.DeclaredRecords | (int)RMContentDisposalAction.Archive;
                }
                //勾选两个的情况：
                //1.Include Related + Include Declare
                //2.Include Related + Archive
                //3.Include Declare + Archive
                else if (rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both && rule.DeleteRecords)
                {
                    return (int)RMContentDisposalAction.RelatedRecords | (int)RMContentDisposalAction.DeclaredRecords;
                }
                else if (rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both && !((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    return (int)RMContentDisposalAction.RelatedRecords | (int)RMContentDisposalAction.Archive;
                }
                else if (rule.DeleteRecords && !((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    return (int)RMContentDisposalAction.DeclaredRecords | (int)RMContentDisposalAction.Archive;
                }
                //勾选一个的情况：
                //1.Include Related 
                //2.Include Declare
                //3.Archive
                else if (rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both)
                {
                    return (int)RMContentDisposalAction.RelatedRecords;
                }
                else if (rule.DeleteRecords)
                {
                    return (int)RMContentDisposalAction.DeclaredRecords;
                }
                else if (!((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    return (int)RMContentDisposalAction.Archive;
                }
                //什么都不勾
                return (int)RMContentDisposalAction.Remove;
            }
        }

        public static int GetOperationTypeForEXO(Rule rule)
        {
            if (rule == null)
            {
                return (int)RMContentDisposalAction.None;
            }
            int keepDataOption = rule.KeepDataOption;
            if (rule.ExportInfo != null && rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive && rule.ExportType != GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None)
            {
                return (int)RMContentDisposalAction.ExportOnly;
            }
            else if ((keepDataOption & (int)KeepDataStatus.Keep) == (int)KeepDataStatus.Keep)
            {
                return (int)RMContentDisposalAction.KeepData;
            }
            else if (keepDataOption == (int)KeepDataStatus.Delete && rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null)
            {
                var status =(int) RMContentDisposalAction.Move;
                if (rule.spMoveOption.MoveDestination.DeleteSourceItem)
                {
                    status = status | (int)RMContentDisposalAction.MoveWithDeleteSource;
                }
                if (rule.spMoveOption.MoveDestination.KeepSourceClassification)
                {
                    status = status | (int)RMContentDisposalAction.MoveWithKeepClassfication;
                }
               return status;
            }
            else
            {
                return (int)RMContentDisposalAction.Remove;
            }
        }
        //TO DO FS
        public static int GetOperationTypeForFS(Rule rule)
        {
            if (rule == null)
            {
                return (int)RMContentDisposalAction.None;
            }
            int keepDataOption = rule.KeepDataOption;
            if (keepDataOption == (int)KeepDataStatus.LinkToDocument)
            {
                return (int)RMContentDisposalAction.LeaveStub;
            }
            else if((keepDataOption & (int)KeepDataStatus.Archive) == (int)KeepDataStatus.Archive)
            {
                return (int)RMContentDisposalAction.NewLogicArchvie;
            }
            else if (keepDataOption != (int)KeepDataStatus.Delete && keepDataOption != (int)KeepDataStatus.Remove && keepDataOption != (int)KeepDataStatus.Vault)
            {
                return (int)RMContentDisposalAction.KeepData;
            }
            else if (rule.spMoveOption != null)
            {
                return (int)RMContentDisposalAction.Move;
            }
            else
            {
                return (int)RMContentDisposalAction.Remove;
            }
        }

        public static int GetOperationTypeForAzureFile(Rule rule)
        {
            if (rule == null)
            {
                return (int)RMContentDisposalAction.None;
            }
            int keepDataOption = rule.KeepDataOption;
            if (keepDataOption == (int)KeepDataStatus.LinkToDocument)
            {
                return (int)RMContentDisposalAction.LeaveStub;
            }
            else if (keepDataOption != (int)KeepDataStatus.Delete && keepDataOption != (int)KeepDataStatus.Remove && keepDataOption != (int)KeepDataStatus.Vault)
            {
                return (int)RMContentDisposalAction.KeepData;
            }
            else if (rule.spMoveOption != null)
            {
                return (int)RMContentDisposalAction.Move;
            }
            else
            {
                return (int)RMContentDisposalAction.Remove;
            }
        }

        public static int GetOperationTypeForBox(Rule rule)
        {
            if (rule == null)
            {
                return (int)RMContentDisposalAction.None;
            }
            int keepDataOption = rule.KeepDataOption;
            if (keepDataOption == (int)KeepDataStatus.LinkToDocument)
            {
                return (int)RMContentDisposalAction.LeaveStub;
            }
            //else if (keepDataOption != (int)KeepDataStatus.Delete && keepDataOption != (int)KeepDataStatus.Remove && keepDataOption != (int)KeepDataStatus.Vault)
            //{
            //    return (int)RMContentDisposalAction.KeepData;
            //}
            else if (rule.spMoveOption != null)
            {
                return (int)RMContentDisposalAction.Move;
            }
            else
            {
                return (int)RMContentDisposalAction.Remove;
            }
        }

        public static int GetOperationTypeForConnector(Rule rule)
        {
            if (rule == null)
            {
                return (int)RMContentDisposalAction.None;
            }
            int keepDataOption = rule.KeepDataOption;
            if (keepDataOption == (int)KeepDataStatus.LinkToDocument)
            {
                return (int)RMContentDisposalAction.LeaveStub;
            }
            //else if (keepDataOption != (int)KeepDataStatus.Delete && keepDataOption != (int)KeepDataStatus.Remove && keepDataOption != (int)KeepDataStatus.Vault)
            //{
            //    return (int)RMContentDisposalAction.KeepData;
            //}
            else if (rule.spMoveOption != null)
            {
                return (int)RMContentDisposalAction.Move;
            }
            else
            {
                return (int)RMContentDisposalAction.Remove;
            }
        }

        public static int GetOperationTypeForSPLocal(Rule rule)
        {
            if (rule == null)
            {
                return (int)RMContentDisposalAction.None;
            }
            int keepDataOption = rule.KeepDataOption;
            if (rule.ExportInfo != null && rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive && rule.ExportType != AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None)
            {
                return (int)RMContentDisposalAction.ExportOnly;
            }
            else if ((keepDataOption & (int)KeepDataStatus.LinkToDocument) == (int)KeepDataStatus.LinkToDocument)
            {
                //四个都勾选-------Leave Stub + Include Related + Include Declare + Archive
                if (rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both && rule.DeleteRecords && !((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    if (rule.DeclareLinkFile)
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords) | (int)RMContentDisposalAction.DeclaredRecords | (int)RMContentDisposalAction.Archive | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords) | (int)RMContentDisposalAction.DeclaredRecords | (int)RMContentDisposalAction.Archive;
                    }
                }
                //勾选三个的情况：
                //1.Leave Stub + Include Related + Include Declare
                //2.Leave Stub + Include Related + Archive
                //3.Leave Stub + Include Declare + Archive
                else if (rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both && rule.DeleteRecords)
                {
                    if (rule.DeclareLinkFile)
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords) | (int)RMContentDisposalAction.DeclaredRecords | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords) | (int)RMContentDisposalAction.DeclaredRecords;
                    }
                }
                else if (rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both && !((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    if (rule.DeclareLinkFile)
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords) | (int)RMContentDisposalAction.Archive | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords) | (int)RMContentDisposalAction.Archive;
                    }
                }
                else if (rule.DeleteRecords && !((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    if (rule.DeclareLinkFile)
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.DeclaredRecords) | (int)RMContentDisposalAction.Archive | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.DeclaredRecords) | (int)RMContentDisposalAction.Archive;
                    }
                }
                //勾选两个的情况：
                //1.Leave Stub + Include Related
                //2.Leave Stub + Include Declare
                //3.Leave Stub + Archive
                else if (rule.RelatedRecordOption == GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both)
                {
                    if (rule.DeclareLinkFile)
                    {
                        return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords;
                    }
                }
                else if (rule.DeleteRecords)
                {
                    if (rule.DeclareLinkFile)
                    {
                        return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.DeclaredRecords | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.DeclaredRecords;
                    }
                }
                else if (!((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    if (rule.DeclareLinkFile)
                    {
                        return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.Archive | (int)RMContentDisposalAction.DeclareLinkFile;
                    }
                    else
                    {
                        return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.Archive;
                    }
                }
                //只勾选Leave Stub
                if (rule.DeclareLinkFile)
                {
                    return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.DeclareLinkFile;
                }
                else
                {
                    return (int)RMContentDisposalAction.LeaveStub;
                }
            }
            else if ((keepDataOption & (int)KeepDataStatus.Keep) == (int)KeepDataStatus.Keep)
            {
                return (int)RMContentDisposalAction.KeepData;
            }
            else if (keepDataOption == (int)KeepDataStatus.Delete && rule.MoveToRecordCenterAndDelareSetting != null && rule.MoveToRecordCenterAndDelareSetting.DestinationLocation != null)
            {
                //DelaredRecord为false时，是Declare，否则不是Declare，这样设计是为了升级兼容老数据
                if (!rule.MoveToRecordCenterAndDelareSetting.DelaredRecord)
                {
                    return (int)RMContentDisposalAction.MoveDeclare;
                }
                return (int)RMContentDisposalAction.Move;
            }
            else if (keepDataOption == (int)KeepDataStatus.Delete && rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null && !rule.spMoveOption.MoveDestination.NotDeclareMovedData)
            {
                return (int)RMContentDisposalAction.MoveDeclare;
            }
            else if (keepDataOption == (int)KeepDataStatus.Delete && rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null && rule.spMoveOption.MoveDestination.NotDeclareMovedData)
            {
                return (int)RMContentDisposalAction.Move;
            }
            else
            {
                //三者都勾选 Include Related + Include Declare + Archive
                if (rule.RelatedRecordOption == AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both && rule.DeleteRecords && !((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    return (int)RMContentDisposalAction.RelatedRecords | (int)RMContentDisposalAction.DeclaredRecords | (int)RMContentDisposalAction.Archive;
                }
                //勾选两个的情况：
                //1.Include Related + Include Declare
                //2.Include Related + Archive
                //3.Include Declare + Archive
                else if (rule.RelatedRecordOption == AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both && rule.DeleteRecords)
                {
                    return (int)RMContentDisposalAction.RelatedRecords | (int)RMContentDisposalAction.DeclaredRecords;
                }
                else if (rule.RelatedRecordOption == AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both && !((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    return (int)RMContentDisposalAction.RelatedRecords | (int)RMContentDisposalAction.Archive;
                }
                else if (rule.DeleteRecords && !((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    return (int)RMContentDisposalAction.DeclaredRecords | (int)RMContentDisposalAction.Archive;
                }
                //勾选一个的情况：
                //1.Include Related 
                //2.Include Declare
                //3.Archive
                else if (rule.RelatedRecordOption == AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both)
                {
                    return (int)RMContentDisposalAction.RelatedRecords;
                }
                else if (rule.DeleteRecords)
                {
                    return (int)RMContentDisposalAction.DeclaredRecords;
                }
                else if (!((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    return (int)RMContentDisposalAction.Archive;
                }
                //什么都不勾
                return (int)RMContentDisposalAction.Remove;
            }
        }
        public static int GetOperationTypeForGoogleDrive(Rule rule)
        {
            if (rule == null)
            {
                return (int)RMContentDisposalAction.None;
            }
            else if (rule.spMoveOption != null)
            {
                return (int)RMContentDisposalAction.Move;
            }
            else if (rule.ExportInfo != null && rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive && rule.ExportType != GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None)
            {
                return (int)RMContentDisposalAction.ExportOnly;
            }
            if (rule.KeepDataOption == (int)KeepDataOption.Archive)
            {
                return (int)RMContentDisposalAction.ArchiveToStorage;
            }
            else
            {
                return (int)RMContentDisposalAction.Remove;
            }

        }

        public static RMContentDisposalAction GetOperationTypeForPhysical(Rule rule)
        {
            if (rule == null)
            {
                return RMContentDisposalAction.None;
            }
            if (rule.IsCalculationDisposalDate)
            {
                return RMContentDisposalAction.CalculationDisposalDate;
            }
            int keepDataOption = rule.KeepDataOption;
            if (keepDataOption == (int)KeepDataStatus.Delete && rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null)
            {
                return RMContentDisposalAction.Move;
            }
            else
            {
                return RMContentDisposalAction.Remove;
            }
        }

        private static string ConvertDisposalActionToString(RMContentDisposalAction disposalAction, bool isPhy = false)
        {
            var removeAction = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ArchiveAndRemove");
            var archiveAction = I18NEntity.GetString("RM_RDM_CreateRule_ArchiveToAzureBlobStorage");
            var moveAction = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_MoveRecord");
            var moveLocation = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_MoveLocation");
            var appendMoveDeclareRecords = "; " + I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_Move_DeclareRecord");
            var appendMoveClassify = "; " + I18NEntity.GetString("RM_JS_BCM_Rule_Move_IsReclassify");
            var appendMoveAllVersion = "; " + I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_Move_AllVersions");
            var appendMoveFolder = "; " + I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_Move_FolderStructure");
            var appendMoveDeleteSource = "; " + I18NEntity.GetString("RM_JS_BCM_Rule_Move_IsRemoveEmail");
            var appendRelatedRecord = "; " + I18NEntity.GetString("RM_JS_Rule_Detail_IncludeRelatedRecord");
            var appendDeclaredRecords = "; " + I18NEntity.GetString("RM_JS_Rule_Detail_IncludeDeclaredFile");
            var appendLeaveStub = "; " + I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_LeaveStub");
            var appendDeleteParentBox = "; " + I18NEntity.GetString("RM_RDM_CreateRule_DestroyEmptyBox");
            var appendArchive = "; " + I18NEntity.GetString("RM_RDM_CreateRule_BackupBeforeDestroying");
            var appendMakeStubImmutable = "; " + I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_DeclareStub");
            var exportOnlyAction = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ExportOnly");
            var calculateDisposalDate = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_CalculateDisposalDate");
            switch (disposalAction)
            {
                case RMContentDisposalAction.Remove://Nothing Select
                    return I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ArchiveAndRemove");
                case RMContentDisposalAction.KeepData:
                    return I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_ArchiveAndKeep");
                case RMContentDisposalAction.None:
                    return I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_None");
                case RMContentDisposalAction.LeaveStub://Only Include LeaveStub
                    return removeAction + appendLeaveStub;
                case RMContentDisposalAction.Archive://Only Include Archive
                    return removeAction + appendArchive;
                case RMContentDisposalAction.RelatedRecords://Only Include Related
                    return removeAction + appendRelatedRecord;
                case RMContentDisposalAction.DeclaredRecords://Only Include Declare
                    return removeAction + appendDeclaredRecords;
                case RMContentDisposalAction.RelatedRecords | RMContentDisposalAction.LeaveStub://Leave Stub + Include Related
                    return removeAction + appendRelatedRecord + appendLeaveStub;
                case RMContentDisposalAction.RelatedRecords | RMContentDisposalAction.Archive://Include Related + Archive
                    return removeAction + appendRelatedRecord + appendArchive;
                case RMContentDisposalAction.DeclaredRecords | RMContentDisposalAction.Archive://Include Declare + Archive
                    return removeAction + appendDeclaredRecords + appendArchive;
                case RMContentDisposalAction.DeclaredRecords | RMContentDisposalAction.LeaveStub://Leave Stub + Include Declare
                    return removeAction + appendDeclaredRecords + appendLeaveStub;
                case RMContentDisposalAction.Archive | RMContentDisposalAction.LeaveStub://Leave Stub + Archive
                    return removeAction + appendArchive + appendLeaveStub;
                case RMContentDisposalAction.RelatedRecords | RMContentDisposalAction.DeclaredRecords://Include Related + Include Declare
                    return removeAction + appendRelatedRecord + appendDeclaredRecords;
                case RMContentDisposalAction.RelatedRecords | RMContentDisposalAction.DeclaredRecords | RMContentDisposalAction.Archive://Include Related + Include Declare + Archive
                    return removeAction + appendRelatedRecord + appendDeclaredRecords + appendArchive;
                case RMContentDisposalAction.RelatedRecords | RMContentDisposalAction.LeaveStub | RMContentDisposalAction.Archive://Leave Stub + Include Related + Archive
                    return removeAction + appendRelatedRecord + appendLeaveStub + appendArchive;
                case RMContentDisposalAction.DeclaredRecords | RMContentDisposalAction.LeaveStub | RMContentDisposalAction.Archive://Leave Stub + Include Declare + Archive
                    return removeAction + appendDeclaredRecords + appendLeaveStub + appendArchive;
                case RMContentDisposalAction.RelatedRecords | RMContentDisposalAction.DeclaredRecords | RMContentDisposalAction.LeaveStub://Leave Stub + Include Related + Include Declare
                    return removeAction + appendRelatedRecord + appendDeclaredRecords + appendLeaveStub;
                case RMContentDisposalAction.RelatedRecords | RMContentDisposalAction.DeclaredRecords | RMContentDisposalAction.Archive | RMContentDisposalAction.LeaveStub://Leave Stub + Include Related + Include Declare + Archive
                    return removeAction + appendRelatedRecord + appendDeclaredRecords + appendArchive + appendLeaveStub;
                case RMContentDisposalAction.DeleteParentBox://32
                    return removeAction + appendDeleteParentBox;
                case RMContentDisposalAction.DeleteParentBox | RMContentDisposalAction.RelatedRecords://40
                    return removeAction + appendRelatedRecord + appendDeleteParentBox;
                case RMContentDisposalAction.ExportOnly:
                    return exportOnlyAction;
                case RMContentDisposalAction.ArchiveToStorage:
                    return archiveAction;
                case RMContentDisposalAction.ArchiveToStorageAndLeaveStub:
                    return archiveAction + appendLeaveStub;
                //move
                case RMContentDisposalAction.Move:
                    return isPhy ? moveLocation : moveAction;
                case RMContentDisposalAction.MoveDeclare:
                    return moveAction + appendMoveDeclareRecords;
                case RMContentDisposalAction.MoveWithAllVersions:
                    return moveAction + appendMoveAllVersion;
                case RMContentDisposalAction.MoveWithDeleteSource:
                    return moveAction + appendMoveDeleteSource;
                case RMContentDisposalAction.MoveWithStructure:
                    return moveAction + appendMoveFolder;
                case RMContentDisposalAction.MoveWithKeepClassfication:
                    return moveAction + appendMoveClassify;
                case RMContentDisposalAction.MoveDeclareWithKeepClassfication:
                    return moveAction + appendMoveDeclareRecords + appendMoveClassify;
                case RMContentDisposalAction.MoveDeclareWithStructure:
                    return moveAction + appendMoveDeclareRecords + appendMoveFolder;
                case RMContentDisposalAction.MoveStructureWithAllVersions:
                    return moveAction + appendMoveFolder + appendMoveAllVersion;
                case RMContentDisposalAction.MoveDeclareStructureWithAllVersions:
                    return moveAction + appendMoveDeclareRecords + appendMoveFolder + appendMoveAllVersion;
                case RMContentDisposalAction.Remove_Declared_LeaveStub_MakeStubImmutable:
                    return removeAction + appendDeclaredRecords + appendLeaveStub + appendMakeStubImmutable;
                case RMContentDisposalAction.CalculationDisposalDate:
                    return calculateDisposalDate;
                default:
                    return "";
            }
        }

        public static bool CheckIsWillDeleteDataAction(Rule rule)
        {
            if(rule.ExportInfo?.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)
            {
                return false;
            }
            HashSet<KeepDataOption> notDeleteMainActions = [KeepDataOption.TagContent, KeepDataOption.Keep, KeepDataOption.ArchiverOnly];
            return !notDeleteMainActions.Any(objectRuleAction => (rule.KeepDataOption & (int)objectRuleAction) == (int)objectRuleAction);
        }

        public static bool CheckMoveRule(Rule rs)
        {
            var result = false;
            int keepDataOption = GetOldLogicKeepDataStatusOption(rs.KeepDataOption);
            if (rs != null && keepDataOption == (int)KeepDataStatus.Delete && rs.spMoveOption != null && rs.spMoveOption.MoveDestination != null)
            {
                result = true;
            }
            return result;
        }

        public static bool CheckArchiveOnlyRule(int ruleKeepDataOption)
        {
            if((ruleKeepDataOption & (int)KeepDataStatus.ArchiverOnly) == (int)KeepDataStatus.ArchiverOnly)
            {
                return true;
            }
            return false;
        }

        public static bool CheckArchiveOnlyRule(Rule rs)
        {
            return CheckArchiveOnlyRule(rs.KeepDataOption);
        }

        #endregion

        #region the part is new logic, please use bit operation to check if an option is used, This expands to avoid using a lot of if else
        public static int GetOperationTypeForOneDrive(Rule rule)
        {
            if (rule == null)
            {
                return (int)RMContentDisposalAction.None;
            }

            rule = CopyRule(rule);

            int res = 0;
            int keepDataOption = rule.KeepDataOption;

            if ((keepDataOption & (int)KeepDataStatus.IsEnableRemoveRetentionLabel) == (int)KeepDataStatus.IsEnableRemoveRetentionLabel) //unable check is which main option
            {
                res |= (int)RMContentDisposalAction.IsEnableRemoveRetentionLabel;
            }

            if( (keepDataOption & (int)KeepDataStatus.ArchiverOnly) == (int)KeepDataStatus.ArchiverOnly)
            {
                res |= (int)RMContentDisposalAction.ArchiverOnly;
                if ((keepDataOption & (int)KeepDataStatus.ArchiveOnlyLastestVersion) == (int)KeepDataStatus.ArchiveOnlyLastestVersion)
                {
                    res |= (int)RMContentDisposalAction.ArchiveOnlyLastestVersion;

                }
            }
            else if ((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup)
            {

            }
            else if ((keepDataOption & (int)KeepDataStatus.Archive) == (int)KeepDataStatus.Archive
                || (keepDataOption & (int)KeepDataStatus.ArchiveAndLeaveStub) == (int)KeepDataStatus.ArchiveAndLeaveStub)
            {
                if (rule.DeleteRecords)
                {
                    res |= (int)RMContentDisposalAction.NewDeclaredRecords;
                    rule.DeleteRecords = false;
                }
            }
            else
            {

            }

            int oldLogicRes = GetOperationTypeForOneDrive(rule, GetOldLogicKeepDataStatusOption(keepDataOption));
            if ((res & oldLogicRes) != 0)
            {
                throw new Exception(@$"OD new wrapper res conflict whih old logic res, new wrapper res :{res}, old logic res :{oldLogicRes}");
            }
            res |= oldLogicRes;
            return res;
        }

        public static string ConvertDisposalActionToString(int disposalAction, bool isPhy = false)
        {
            string res = string.Empty;

            foreach (int action in newLogicDisposalActionI18nMap.Keys)
            {
                if ((disposalAction & action) == action)
                {
                    res += I18NEntity.GetString(newLogicDisposalActionI18nMap[action]);
                }
            }

            string oldLogicRes = ConvertDisposalActionToString((RMContentDisposalAction)GetOldLogicDisposalAction(disposalAction), isPhy);
            if (!string.IsNullOrWhiteSpace(oldLogicRes))
            {
                if (!string.IsNullOrWhiteSpace(res) && !string.IsNullOrWhiteSpace(oldLogicRes))
                {
                    oldLogicRes += "; ";
                }
                res = oldLogicRes + res;
            }
            return res;
        }

        public static bool DisposalActionUseNewLogicMainOption(int DisposalAction)
        {
            foreach(int action in newLogicMainOptionSet)
            {
                if ((DisposalAction & action) == action)
                {
                    return true;
                }
            }
            return false;
        }

        public static int GetOldLogicDisposalAction(int DisposalAction)
        {
            foreach (int action in newLogicDisposalActionI18nMap.Keys)
            {
                if ((DisposalAction & action) == action)
                {
                    DisposalAction -= action;
                }
            }
            return DisposalAction;
        }

        public static int GetOldLogicKeepDataStatusOption(int keepDataStatus)
        {
            foreach (int status in newLogicKeepDataStatusList)
            {
                if ((keepDataStatus & status) == status)
                {
                    keepDataStatus -= status;
                }
            }
            return keepDataStatus;
        }

        public static int GetOperationTypeForSP(Rule rule)
        {
            if (rule == null)
            {
                return (int)RMContentDisposalAction.None;
            }
            rule = CopyRule(rule);

            int res = 0;
            int keepDataOption = rule.KeepDataOption;

            if ((keepDataOption & (int)KeepDataStatus.IsEnableRemoveRetentionLabel) == (int)KeepDataStatus.IsEnableRemoveRetentionLabel)// unable judge is which main option
            {
                res |= (int)RMContentDisposalAction.IsEnableRemoveRetentionLabel;
            }

            if ((keepDataOption & (int)KeepDataStatus.ArchiverOnly) == (int)KeepDataStatus.ArchiverOnly)
            {
                res |= (int)RMContentDisposalAction.ArchiverOnly;
                if ((keepDataOption & (int)KeepDataStatus.ArchiveOnlyLastestVersion) == (int)KeepDataStatus.ArchiveOnlyLastestVersion)
                {
                    res |= (int)RMContentDisposalAction.ArchiveOnlyLastestVersion;

                }
            }
            else if ((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup)
            {

            }
            else if ((keepDataOption & (int)KeepDataStatus.Archive) == (int)KeepDataStatus.Archive
                || (keepDataOption & (int)KeepDataStatus.ArchiveAndLeaveStub) == (int)KeepDataStatus.ArchiveAndLeaveStub)
            {
                if (rule.DeleteRecords)
                {
                    res |= (int)RMContentDisposalAction.NewDeclaredRecords;
                    rule.DeleteRecords = false;
                }
            }
            else
            {

            }

            int oldLogicRes = GetOperationTypeForSP(rule, GetOldLogicKeepDataStatusOption(keepDataOption));
            if ((res & oldLogicRes) != 0)
            {
                throw new Exception(@$"SP new wrapper res conflict whih old logic res, new wrapper res :{res}, old logic res :{oldLogicRes}");
            }
            res |= oldLogicRes;
            return res;
        }

        public static int GetOperationTypeForTeams(Rule rule)
        {
            if (rule == null)
            {
                return (int)RMContentDisposalAction.None;
            }

            int res = 0;
            int keepDataOption = rule.KeepDataOption;

            if ((keepDataOption & (int)KeepDataStatus.IsEnableRemoveRetentionLabel) == (int)KeepDataStatus.IsEnableRemoveRetentionLabel)// unable judge is which main option
            {
                res |= (int)RMContentDisposalAction.IsEnableRemoveRetentionLabel;
            }

            int oldLogicRes = GetOperationTypeForSP(rule, GetOldLogicKeepDataStatusOption(keepDataOption));
            if ((res & oldLogicRes) != 0)
            {
                throw new Exception(@$"Teams new wrapper res conflict whih old logic res, new wrapper res :{res}, old logic res :{oldLogicRes}");
            }
            res |= oldLogicRes;
            return res;
        }

        private static Rule CopyRule(Rule rule)
        {
            string ruleJson = SerializerHelper.SerializeByJsonConvert(rule);
            return SerializerHelper.DeserializeByJsonConvert<Rule>(ruleJson);
        }

        public static List<string> ParseDisposalActionListForFS(int DisposalAction)
        {
            List<string> res = new List<string>();
            if (oldLogicFSDisposalActionDic.ContainsKey(GetOldLogicDisposalAction(DisposalAction)) && !DisposalActionUseNewLogicMainOption(DisposalAction))
            {
                res.AddRange(oldLogicFSDisposalActionDic[GetOldLogicDisposalAction(DisposalAction)]);
            }

            foreach (int action in newLogicDisposalActionI18nMap.Keys)
            {
                if ((DisposalAction & action) == action)
                {
                    res.Add(newLogicDisposalActionI18nMap[action]);
                }
            }
            return res;
        }



        public static List<string> ParseDisposalActionListForPhysical(int DisposalAction)
        {
            List<string> res = new List<string>();
            if (oldLogicPhysicalDisposalActionDic.ContainsKey(GetOldLogicDisposalAction(DisposalAction)))
            {
                res.AddRange(oldLogicPhysicalDisposalActionDic[GetOldLogicDisposalAction(DisposalAction)]);
            }

            foreach (int action in newLogicDisposalActionI18nMap.Keys)
            {
                if ((DisposalAction & action) == action)
                {
                    res.Add(newLogicDisposalActionI18nMap[action]);
                }
            }
            return res;
        }



        public static List<string> ParseDisposalActionListForBox(int DisposalAction)
        {
            List<string> res = new List<string>();
            if (oldLogicBoxDisposalActionDic.ContainsKey(GetOldLogicDisposalAction(DisposalAction)))
            {
                res.AddRange(oldLogicBoxDisposalActionDic[GetOldLogicDisposalAction(DisposalAction)]);
            }

            foreach (int action in newLogicDisposalActionI18nMap.Keys)
            {
                if ((DisposalAction & action) == action)
                {
                    res.Add(newLogicDisposalActionI18nMap[action]);
                }
            }
            return res;
        }



        public static List<string> ParseDisposalActionListForGoogle(int DisposalAction)
        {
            List<string> res = new List<string>();
            if (oldLogicGoogleDisposalActionDic.ContainsKey(GetOldLogicDisposalAction(DisposalAction)))
            {
                res.AddRange(oldLogicGoogleDisposalActionDic[GetOldLogicDisposalAction(DisposalAction)]);
            }

            foreach (int action in newLogicDisposalActionI18nMap.Keys)
            {
                if ((DisposalAction & action) == action)
                {
                    res.Add(newLogicDisposalActionI18nMap[action]);
                }
            }
            return res;
        }

        public static List<string> ParseDisposalActionListForSP(int DisposalAction, SourceFlag sourceFlag)
        {
            List<string> res = new List<string>();
            if (oldLogicSPDisposalActionDic.ContainsKey(GetOldLogicDisposalAction(DisposalAction)) && !DisposalActionUseNewLogicMainOption(DisposalAction))
            {
                res.AddRange(oldLogicSPDisposalActionDic[GetOldLogicDisposalAction(DisposalAction)]);
            }

            foreach (int action in newLogicDisposalActionI18nMap.Keys)
            {
                if ((DisposalAction & action) == action)
                {
                    res.Add(newLogicDisposalActionI18nMap[action]);
                }
            }
            if(sourceFlag == SourceFlag.OneDrive && res.Contains("RM_JS_RDM_CreateRule_Options_ArchiveAndKeep"))
            {
                int index = res.IndexOf("RM_JS_RDM_CreateRule_Options_ArchiveAndKeep");
                res[index] = "RM_JS_RDM_CreateRule_Options_ArchiveAndKeep_TagContent";
            }
            return res;
        }

        public static bool IsAllowedDisposalAction(int disposalAction)
        {
            var allowedActions = new HashSet<int>
            {
                (int)RMContentDisposalAction.Move,
                (int)RMContentDisposalAction.MoveDeclare,
                (int)RMContentDisposalAction.KeepData,
                (int)RMContentDisposalAction.ExportOnly,
                (int)RMContentDisposalAction.TriggerMicrosoft365ArchivingData,
                (int)RMContentDisposalAction.MoveDeclareWithKeepClassfication,
                (int)RMContentDisposalAction.MoveWithDeleteSource,
                (int)RMContentDisposalAction.MoveWithKeepClassfication
            };

            int adjustedAction = disposalAction & ~(int)RMContentDisposalAction.IsEnableRemoveRetentionLabel;
            return allowedActions.Contains(adjustedAction);
        }


        #endregion

    }
}
