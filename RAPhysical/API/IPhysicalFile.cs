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
using AvePoint.RA.DB.Explorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.API
{

    public interface IPhysicalFile : IPhysicalFields, IDisposable
    {
        string Name { get; set; }
        string Description { get; set; }
        string RecordId { get; set; }
        string DirPath { get; }
        string CreateBy { get; set; }
        string ModifiedBy { get; set; }
        long CreateTimeTicks { get; set; }
        long ModifiedTimeTicks { get; set; }
        bool HoldStatus { get; set; }
        long HoldReleaseTime { get; set; }
        string HoldBy { get; set; }
        string HoldId { get; set; }
        int HoldType { get; set; }
        string HoldByUsers { get; set; }
        string HoldUntilTimes { get; set; }
        string[] AppendHolds_Array { get; set; }
        Guid Id { get; set; }
        Guid BoxId { get; set; }
        Guid LocationId { get; set; }
        Guid TermId { get; set; }
        bool IsUnderLocation { get; }
        int TemplateId { get; set; }
        long DisposalDueDate { get; set; }
        long PreviousDisposalDueDate { get; set; }
        string RelatedRecords { get; set; }
        int RelatedRecordsCount { get; set; }
        int DeleteRelatedRecords { get; set; }
        object Barcode { get; }
        int ScopePermissionId { get; set; }
        #region physical Disposal 
        Guid RuleId { get; set; }
        int DisposalStatus { get; set; }
        int ManualApprovedStatus { get; set; }
        int ManualArchiveStatus { get; set; }
        int RecordStatus { get; set; }
        long DisposalActionTime { get; set; }
        bool ExportToManual { get; set; }
        #endregion 
        IPhysicalLocation ParentLocation { get; }
        IPhysicalBox ParentBox { get; }
        List<IPhysicalRecord> Records { get; }
        List<IPhysicalRecord> GetRecords(Expression<Func<Record, bool>> expression);
        Guid ParentId { get; set; }
        List<Guid> Ancestors { get; set; }
        long ManualExtendTime { get; set; }
        void AddPhysicalRecord(IPhysicalRecord physicalRecord);
        Task DeleteAsync();
        void Update(bool forceUpdate = false, bool isModifyPermissionId = false, bool isUpdateManualProperties = false);
    }
}
