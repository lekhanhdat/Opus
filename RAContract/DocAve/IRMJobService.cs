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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.DocAve
{
    public interface IRMJobService
    {
        Task<RAReturnMessage> RunNowAsync(RMSPTreeNode tree, JobRunBy jobRunBy);

        System.Threading.Tasks.Task RunDisposalJobNowAsync(List<RMSPTreeNode> nodes);
        RAReturnMessage RunDeclaredOnly(RMSPTreeNode tree, JobRunBy jobRunBy);

        Task<RAReturnMessage> RunEXONowAsync(RMEXOTreeNode tree, JobRunBy jobRunBy);

        Task<RAReturnMessage> OldOpusTenantRunPhysicalJobNowAsync(int locationID, JobRunBy jobRunBy, bool skipRemoveContentAndDestroyAction);

        Task<RAReturnMessage> NewOpusTenantRunPhysicalJobNowAsync(int locationID, JobRunBy jobRunBy, bool skipRemoveContentAndDestroyAction);

        Task<RAReturnMessage> RunOneDriveNowAsync(RMSPTreeNode tree, JobRunBy jobRunBy);

        Task<string> RealRunDeclareOnlyJobAsync(JobRunBy jobRunBy, string jobRunByUser, RMSPTreeNode runJobNode);

        bool CheckIsRemoteSite(RMSPTreeNode tree);

        bool CheckIsRemoteTeamsExisting(RMSPTreeNode tree);

        bool CheckIsOneDriveNode(RMSPTreeNode tree);
        bool CheckIsTeamsNode(RMSPTreeNode tree);

        bool CheckEXONodeMoved(RMEXOTreeNode tree);
        int GetTenantMainJobCount();

        bool IsFSConnectionDeleted(AvePoint.RA.Contract.Object.RMFSTreeNode treeNode);

        bool RunDisposalInRecords();
        List<RuleNodeContract> BuildBreakTreeNode(RMSPTreeNode tree);

        Task<Dictionary<Guid, List<Guid>>> AssembleTermRuleMappingAsync(AvePoint.RA.Contract.Explorer.SourceFlag sourceFlag);
    }
}
