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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.FileSystem.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.ExplorerSync.Utils
{
    public class IDGenerator
    {
        public static Guid GetRecordId(Guid scopeId, Guid nodeId)
        {
            return (scopeId.ToString().ToLowerInvariant() + nodeId.ToString().ToLowerInvariant()).ToMd5();
        }
    }
    public class RuleHelper
    {
        public static bool CheckMoveRule(Rule rs)
        {
            var result = false;
            if (rs != null && rs.KeepDataOption == (int)KeepDataStatus.Delete && rs.spMoveOption != null && rs.spMoveOption.MoveDestination != null)
            {
                result = true;
            }
            return result;
        }

        public static bool IsRemoveRule(Rule tempRule, int sourceFlag)
        {
            var result = false;
            var action = -1;
            if ((int)SourceFlag.SharePoint == sourceFlag || (int)SourceFlag.SharePointOnPrem == sourceFlag)
            {
                action = (int)GetOperationTypeForSP(tempRule);
            }
            if (action == 0 || action == 2 || action == 8
                || action == 10 || action == 16 || action == 18 || action == 24 || action == 26)
            {
                result = true;
            }
            return result;
        }

        public static int GetOperationTypeForSP(Rule rule)
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
                if (rule.RelatedRecordOption == AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both && rule.DeleteRecords && !((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords) | (int)RMContentDisposalAction.DeclaredRecords | (int)RMContentDisposalAction.Archive;
                }
                //勾选三个的情况：
                //1.Leave Stub + Include Related + Include Declare
                //2.Leave Stub + Include Related + Archive
                //3.Leave Stub + Include Declare + Archive
                else if (rule.RelatedRecordOption == AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both && rule.DeleteRecords)
                {
                    return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords) | (int)RMContentDisposalAction.DeclaredRecords;
                }
                else if (rule.RelatedRecordOption == AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both && !((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords) | (int)RMContentDisposalAction.Archive;
                }
                else if (rule.DeleteRecords && !((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    return ((int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.DeclaredRecords) | (int)RMContentDisposalAction.Archive;
                }
                //勾选两个的情况：
                //1.Leave Stub + Include Related
                //2.Leave Stub + Include Declare
                //3.Leave Stub + Archive
                else if (rule.RelatedRecordOption == AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both)
                {
                    return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords;
                }
                else if (rule.DeleteRecords)
                {
                    return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.DeclaredRecords;
                }
                else if (!((keepDataOption & (int)KeepDataStatus.NotBackup) == (int)KeepDataStatus.NotBackup))
                {
                    return (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.Archive;
                }
                //只勾选Leave Stub
                return (int)RMContentDisposalAction.LeaveStub;
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
    }
}
