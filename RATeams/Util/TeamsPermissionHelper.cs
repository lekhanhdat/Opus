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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;

namespace RATeams
{
    public static class TeamsPermissionHelper
    {
        private static IRMKeyValueDao s_RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        //Using for filter out in job monitor with Teams job type
        private static List<JobType> s_AllowedTeamsJobTypes = new List<JobType>()
        {
            JobType.TeamsActionAuditReport,
            JobType.TeamsArchiverBackup,
            JobType.SpecifyTeamsArchiverBackup,
            JobType.TeamsArchiverRestore,
            JobType.TeamsOutPlaceRestore,
            JobType.TeamsArchiverRetention,
            JobType.TeamsBCSTermUsageReport,
            JobType.TeamsCreateAndDestroyedFileReport,
            JobType.TeamsDataSynchronisation,
            JobType.TeamsDataSynchronisationSchedule,
            JobType.TeamsEnforceRetention,
            JobType.TeamsItemsFilesDueDisposalReport,
            JobType.TeamsOrphanedTermUsageReport,
            JobType.TeamsRecordsDisposal,
            JobType.TeamsRestoreReport,
            JobType.TeamsRetiredTermUsageReport,
            JobType.TeamsScheduleSetting,
            JobType.TeamsUniqueIDSettingFullSchedule,
            JobType.TeamsUniqueIDSettingIncrementalSchedule,
            JobType.ApplyTeamsSettings,
            JobType.ImportTeamsSetting,
            JobType.ExportTeamsSetting,
            JobType.MailBoxArchiverRestore,
        };

        public static bool HasUpgradeTeamsFeature()
        {
            return s_RMKeyValueDao.HasUpgradeTeams();
        }

        public static List<JobType> FilterAllowedTeamsJobTypes(List<JobType> jobTypes)
        {
            if (HasUpgradeTeamsFeature() || jobTypes.Count == 0)
            {
                return jobTypes;
            }

            return jobTypes.Where(j => !s_AllowedTeamsJobTypes.Contains(j)).ToList();
        }
    }
}
