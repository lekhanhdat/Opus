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
    public interface IRMPhysicalRecordSettingsService
    {
        string SaveColumn(Guid locationUID, string columnName, bool columnRequired = true);
        Task<RMPRTreeNode> LoadPhysicalRecordSettingAsync(Guid locationUID);

        RAReturnMessage SaveTerm(RMPRSaveTermDto termDto);

        Task<RAReturnMessage> SaveRecordOwnerAsync(RMPRSaveRecordOwnerDto recordOwnerDto);

        string GetProfileId(Guid locationUid);

        int InheritParentSetting(Guid locationUid);
        RAReturnMessage RunPhysicalDisposalJob(int locationId, JobRunBy jobRunBy);
        string RunPhysicalDisposalScheduleJob(string profileId ,JobRunBy jobRunBy);
        RAReturnMessage RealRunPhysicallDisposalScheduleJob(string param, JobRunBy JobRunType);
        void RunPhysicalDisposalScheduleJob(string profileId);
        void RunPhysicalTimerJob(JobRunBy jobRunBy);
        Task<RAReturnMessage> SyncADUsersAsync(List<ToUserInfo> users);
        void CheckIsTopLevelSetting(string locationDirPath, out bool isTopLevelLocation, out Guid topLevelLocationUniqueId, out List<string> locationIds);
        Task<RAReturnMessage> RunPhysicalRecordsDisposalJobAsync(int locationId, JobRunBy jobRunBy, bool skipRemoveContentAndDestroyAction);
        string RealRunPhysicalRecordsDisposalJob( string jobRunByUser, JobRunBy jobRunBy, string param);
        string RealRunPhysicalRecordsForApprovalDisposalJob(string jobRunByUser, JobRunBy jobRunBy);
    }
}
