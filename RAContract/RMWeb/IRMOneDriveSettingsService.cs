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
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.LocationManagement;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IRMOneDriveSettingsService
    {
        Task<RMSPTreeNode> LoadNodeSettingAsync(RMSPTreeNode sNode);
        System.Threading.Tasks.Task LoadSettingIconAsync(List<RMSPSampleTreeNode> nodes);
        Task<RAReturnMessage> AddTermSettingAsync(RMSPTreeNode node);
        Task<RAReturnMessage> AddLocationOwnersAsync(RMSPTreeNode node);
        Task<RAReturnMessage> AddEnableRecordsManagementSettingAsync(RMSPTreeNode settingNode);
        Task<RAReturnMessage> InheritParentSettingAsync(RMSPTreeNode node);
        RMSPTreeNode GetSiteCollectionNode(RMSPTreeNode node);
        bool CheckParentNodeDisable(RMSPTreeNode settingNode, string SPObjectId, bool isCheckSelfNode = true);
        Task<string> RealRunDataSyncJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param);
        RAReturnMessage RunDataSyncJob(AvePoint.RA.Contract.Object.RMSPTreeNode selectedTree, JobRunBy jobRunBy);
        RAReturnMessage RunOneDriveDataSyncScheduleJob(JobRunBy jobRunBy);
        Task<string> RealRunOneDriveDataSyncScheduleJobAsync(JobRunBy jobRunBy, string jobRunByUser = null);
        Task<RAReturnMessage> AddIsShowUniqueIdSettingAsync(RMSPTreeNode settingNode);
        Task<RAReturnMessage> AddOneDriveGeneralSettingAsync(RMSPTreeNode settingNode);
        RAReturnMessage RunRecordsDisposalJob(AvePoint.RA.Contract.Object.RMSPTreeNode selectedTree, JobRunBy jobRunBy);
        Task<string> RealRunRecordsDisposalJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param);
    }
}
