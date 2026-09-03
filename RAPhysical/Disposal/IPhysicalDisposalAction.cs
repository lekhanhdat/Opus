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
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.RAPhysical.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAContract = AvePoint.GCommon.Contract.StorageOptimization.Object;


namespace AvePoint.RA.RAPhysical.Disposal
{
    public delegate void SendReportHandler(string name, string originPath, string ruleName, PhysicalDisposalActionType actionType, string destinationPath, string ItemType, JobDetailsStatus status, string comment = "");
    public delegate void SendRelatedReportHandler(List<OnPremRelatedResult> relatedResult, string name, string dirPath, string ItemType, JobDetailsStatus status, string comment = "");
    public delegate void RelatedPostAction(IPhysicalFile file, Rule rule, SendReportHandler SendReportHandler);
    public interface IPhysicalDisposalAction
    {
        PhysicalRecordActionAudit DisposalBox(IPhysicalBox box, Rule rule, SendReportHandler SendReportHandler);//How to deside the action.
        PhysicalRecordActionAudit DisposalFile(IPhysicalFile file, Rule rule, SendReportHandler SendReportHandler, bool needRelatedRecord = true, bool includeDeleteBlock = false);
        void DisposalRecord(IPhysicalRecord record, Rule rule, bool needRelatedRecord = true);
        Task MoveFileAsync(IPhysicalFile file, Guid boxId, Guid locationId, string fullPath, string ruleName, DAContract.ConflictOption conflictOption, SendReportHandler SendReportHandler, PhysicalHoldConflictOption physicalHoldConflict);
        Task MoveBoxAsync(IPhysicalBox box, Guid locationId, string ruleName, DAContract.ConflictOption conflictOption, SendReportHandler sendReport, PhysicalHoldConflictOption physicalHoldConflict);
        //void MoveRecord(IPhysicalRecord record, Guid LocationId);
        //void MoveRecord(IPhysicalFile file, IPhysicalRecord record, Guid boxId, Guid LocationId, DAContract.ConflictOption conflictOption, bool needCheckConflictOption, int fileScopePermissionId);
        void PendingBox(IPhysicalBox box, Rule rule, SendReportHandler SendReportHandler);//Need Update to all sub files.
        void PendingFile(IPhysicalFile file, Rule rule, SendReportHandler SendReportHandler);//Need Update all records in the file.
        void PendingRecord(IPhysicalRecord record, Rule rule);
        void EmptyBoxRuleInfo(IPhysicalBox box);
        void EmptyFileRuleInfo(IPhysicalFile file);
        void EmptyRecordRuleInfo(IPhysicalRecord record);

        bool HasMoveFailed();

        bool HasMoveSuccess();
        void CalculateDisposalDateForFolder(IPhysicalFile folder, PhysicalRuleEngine engine, ObjectInfoBase fileFilterObj, SendReportHandler sendReportHandler);
    }
}
