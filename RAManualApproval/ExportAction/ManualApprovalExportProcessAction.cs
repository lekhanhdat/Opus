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
using RAManualApproval.BulkAction.ManualApprovalBulkActions;
using RAManualApproval.BulkAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.ManualApproval.Enums;
using RAManualApproval.ExportAction.Disposal_extensions;
using RAManualApproval.ExportAction.UnderReview;
using RAManualApproval.ExportAction.RelatedRecords;
using RAManualApproval.ExportAction.WaitingDisposal;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.ManualApproval.Model;

namespace RAManualApproval.ExportAction
{
    public class ManualApprovalExportJobProcessAction
    {

        private static readonly IRMSubJobDao SubJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();

        private static readonly Dictionary<ManualApprovalTab, ManualApprovalExportProcess> manualExportExecutors = new()
        {
            {ManualApprovalTab.UnderReview, new ManualUnderReviewExportProcessor() },
            {ManualApprovalTab.WaitDisposal, new ManualWaitingDisposalExportProcessor() },
            {ManualApprovalTab.Extend, new ManualDisposalExtensionsExportProcessor() },
            {ManualApprovalTab.RelatedRecords, new ManualRelatedRecordsExportProcessor() }
        };

        public static async Task RunAsync(string subJobId, string jobId, int manualApprovalTab)
        {
            var executor = manualExportExecutors[(ManualApprovalTab)manualApprovalTab];
            await executor.RunAsync(subJobId, jobId);
        }
    }

}
