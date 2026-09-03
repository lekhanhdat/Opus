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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using Cloud.Sdk.Telemetry.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IRecordsHistoryService
    {
        void AddRecordsHistory(List<Guid> currentIds, string historyAction, string comment = "");
        void AddRecordsHistoryWithUser(List<Guid> currentIds, string historyAction, string logonUser, string comment);
        Task<List<RecordHistory>> GetRecordsHistoryAsync(string historyInfo, Guid recordsId, bool isControlPlus = false);
        void CloneMoveHistoryRecords(Guid sourceId, Guid destId);

        Task<List<PhysicalAudit>> GetPhysicalRecordActionAuditsAsync(Guid recordId);

        System.Threading.Tasks.Task AddPhysicalRecordActionAuditAsync(PhysicalActionType actionType, Guid recordId, PhysicalObjectDto newObject, bool isNew, PhysicalObjectDto oldObject = null);

        void AddPhysicalHoldActionAudit(Dictionary<Guid, string> records, HoldSettingDto holdDto, string holdName, AuditAction actionType);

        void AddPhysicalRelatedActionAudit(Guid id, string relateRecords, List<string> addRecords);

        void AddPhysicalCommonHoldActionAudit(Guid id, PhysicalActionType actionType);

        System.Threading.Tasks.Task AddPhysicalPermissionAudtisAsync(ScopePermissionDto dto);

        PhysicalRecordActionAudit BuildPhysicalLoanAudit(Guid id, Dictionary<string, CustomColumn> customColumnDic, string currHeldBy);

        PhysicalRecordActionAudit BuildPhysicalReturnLoanAudit(Guid id, Dictionary<string, CustomColumn> customColumnDic);

        PhysicalRecordActionAudit BuildPhysicalActionAuditForJob(Guid id, PhysicalActionType actionType, bool isNew, JobRunBy jobRunBy = JobRunBy.Control, string originalPath = "", string destinationPath = "");

        PhysicalRecordActionAudit BuildPhysicalReclassifyAudit(Guid id, string orignalTermPath, string currentTermPath);

        void AddPhysicalAudit(List<PhysicalRecordActionAudit> physicalAudits);

        void AddRecordReturnLoanHistory(List<RecordReturnLoanDataHistory> entries);

        Task<PhysicalReturnHistoryResponse> GetReturnLoanHistory(ReturnLoanHistoryParam param, int limit = -1);
        void AddMoveData(List<PhysicalRecordMoveData> moveData);
        Task<PickListMoveResultDto> GetMoveData(PickListMoveParam param, int limit = -1);
        Task<PhysicalRecordMoveData> BuildPhysicalMoveDataAsync(dynamic item, int status, string comment,string destinationPath, Guid destinationLocationId, string homeLocation);
    }
}
