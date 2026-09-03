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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.SharePoint.Client.Microfeed;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMJobSizeAndCountStatisticsDao : BaseDao<RMJobSizeAndCountStatistics>, IRMJobSizeAndCountStatisticsDao
    {
        public async Task<List<RMJobSizeAndCountStatistics>> GetAllDeleteJobStatisticsAsync(bool isTrailLicence = false)
        {
            long lastYearTicks = DateTime.UtcNow.AddYears(-1).Ticks;
            if (isTrailLicence)
            {
                return await base.FindListAsync(a => (a.JobType == (int)JobType.RecordsDisposal || a.JobType == (int)JobType.OneDriveRecordsDisposal || a.JobType == (int)JobType.RMArchiverBackup || a.JobType == (int)JobType.RMEndUserArchiverBackup || a.JobType == (int)JobType.SpecifySitesArchiverBackup || a.JobType == (int)JobType.DiscoverOptimization || a.JobType == (int)JobType.ApprovalProcessArchive || a.JobType == (int)JobType.TeamsArchiverBackup || a.JobType == (int)JobType.SpecifyTeamsArchiverBackup || a.JobType == (int)JobType.TeamsRecordsDisposal || a.JobType == (int)JobType.ArchiverByHSMXml || a.JobType == (int)JobType.CleanUpDuplicateDatas) && a.LicenceType == (int)Cloud.Sdk.Data.AosModern.LicenseType.Trial && a.Status == (int)StatisticsStatus.Using && a.StatisticsTime >= lastYearTicks);
            }
            else
            {
                return await base.FindListAsync(a => (a.JobType == (int)JobType.RecordsDisposal || a.JobType == (int)JobType.OneDriveRecordsDisposal || a.JobType == (int)JobType.RMArchiverBackup || a.JobType == (int)JobType.RMEndUserArchiverBackup || a.JobType == (int)JobType.SpecifySitesArchiverBackup || a.JobType == (int)JobType.DiscoverOptimization || a.JobType == (int)JobType.ApprovalProcessArchive || a.JobType == (int)JobType.TeamsArchiverBackup || a.JobType == (int)JobType.SpecifyTeamsArchiverBackup || a.JobType == (int)JobType.TeamsRecordsDisposal|| a.JobType == (int)JobType.ArchiverByHSMXml || a.JobType == (int)JobType.CleanUpDuplicateDatas) && a.LicenceType == (int)Cloud.Sdk.Data.AosModern.LicenseType.Enterprise && a.Status == (int)StatisticsStatus.Using && a.StatisticsTime >= lastYearTicks);
            }
        }

        public async Task<List<RMJobSizeAndCountStatistics>> GetAOSPDeleteJobStatisticsAsync()
        {
            long lastYearTicks = DateTime.UtcNow.AddYears(-1).Ticks;
            return await base.FindListAsync(a => (a.JobType == (int)JobType.DiscoveryAOSPOptimization) && a.StatisticsTime >= lastYearTicks);
        }

        public async Task<List<RMJobSizeAndCountStatistics>> GetAllJobStatisticsAsync()
        {
            using var context = GetNewContext();
            return await context.RMJobSizeAndCountStatistics.ToListAsync();
        }

        public async Task<List<RMJobSizeAndCountStatistics>> GetAllRestoreJobStatisticsAsync(bool isTrailLicence = false)
        {
            if (isTrailLicence)
            {
                return await base.FindListAsync(a => 
                    (
                        a.JobType == (int)JobType.ArchiverRestore 
                        || a.JobType == (int)JobType.ArchiverOutPlaceRestore 
                        || a.JobType == (int)JobType.StubOopRestore 
                        || a.JobType == (int)JobType.TeamsArchiverRestore 
                        || a.JobType == (int)JobType.TeamsOutPlaceRestore
                        || a.JobType == (int)JobType.MailBoxArchiverRestore
                        || a.JobType == (int)JobType.ArchiverOutPlaceRestore
                    ) 
                    && a.LicenceType == (int)Cloud.Sdk.Data.AosModern.LicenseType.Trial && a.Status == (int)StatisticsStatus.Using);
            }
            else
            {
                return await base.FindListAsync(a => 
                    (
                        a.JobType == (int)JobType.ArchiverRestore 
                        || a.JobType == (int)JobType.ArchiverOutPlaceRestore 
                        || a.JobType == (int)JobType.StubOopRestore 
                        || a.JobType == (int)JobType.TeamsArchiverRestore
                        || a.JobType == (int)JobType.TeamsOutPlaceRestore
                        || a.JobType == (int)JobType.MailBoxArchiverRestore 
                        || a.JobType == (int)JobType.ArchiverOutPlaceRestore
                    ) 
                    && a.LicenceType == (int)Cloud.Sdk.Data.AosModern.LicenseType.Enterprise && a.Status == (int)StatisticsStatus.Using);
            }
        }        
        
        public async Task<List<RMJobSizeAndCountStatistics>> GetAOSPRestoreJobStatisticsAsync()
        {
            return await base.FindListAsync(a => a.JobType == (int)JobType.AOSPRestore && a.Status == (int)StatisticsStatus.Using);
        }

        public async Task<RMJobSizeAndCountStatistics> GetJobStatisticsByJobTypeAndKeepDataOptionAsync(JobType jobType, int keepDataOption,bool isTrailLicence = false)
        {
            List<RMJobSizeAndCountStatistics> result = null;
            if (isTrailLicence)
            {
                result = await base.FindListAsync(a => a.JobType == (int)jobType && a.KeepDataOption == keepDataOption && a.LicenceType == (int)Cloud.Sdk.Data.AosModern.LicenseType.Trial);
            }
            else
            {
                result = await base.FindListAsync(a => (a.JobType == (int)jobType && a.KeepDataOption == keepDataOption) && a.LicenceType == (int)Cloud.Sdk.Data.AosModern.LicenseType.Enterprise);
            }
            if (result != null)
            {
                return result.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public async Task<List<RMJobSizeAndCountStatistics>> GetJobStatisticsByJobTypeAsync(JobType jobType)
        {
            return await base.FindListAsync(a => a.JobType == (int)jobType);
        }
                
        public async Task<List<RMJobSizeAndCountStatistics>> GetJobStatisticsByMainJobIdAsync(JobType jobType, string mainJobId)
        {
            return await base.FindListAsync(a => a.JobType == (int)jobType && a.JobId.StartsWith(mainJobId));
        }

        public async Task AddJobStatisticsAsync(JobType jobType, int keepDataOption, long totalSize,string jobId, string SiteId, bool isTrailLicence = false)
        {

            ExecuteWithRetry(context =>
            {
                string sql = $"INSERT INTO [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMJobSizeAndCountStatistics (Id,JobName,JobType,KeepDataOption,Size,EndUserJobCount,Extend,LicenceType,JobId,StatisticsTime,Status, SiteId) VALUES(@Id, @JobName,@JobType,@KeepDataOption,@TotalSize,0,null,@LicenceType,@JobId,@StatisticsTime,@Status,@SiteId)";
                var paras = new SqlParameter[]
                {
                        new SqlParameter("TotalSize", totalSize),
                        new SqlParameter("JobType", jobType),
                        new SqlParameter("KeepDataOption", keepDataOption),
                        new SqlParameter("LicenceType", isTrailLicence?(int)Cloud.Sdk.Data.AosModern.LicenseType.Trial:(int)Cloud.Sdk.Data.AosModern.LicenseType.Enterprise),
                        new SqlParameter("Id", Guid.NewGuid()),
                        new SqlParameter("JobName", jobType.ToString()),
                        new SqlParameter("JobId", jobId),
                        new SqlParameter("StatisticsTime",DateTime.UtcNow.Ticks),
                        new SqlParameter("Status",(int)StatisticsStatus.Using),
                        new SqlParameter("SiteId",SiteId)
                };
                context.Database.ExecuteSqlCommand(sql, paras);
            });
        }
        public async Task UpdateJobStatisticsStatusAsync()
        {
            ExecuteWithRetry(context =>
            {
                string sql = $"Update [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMJobSizeAndCountStatistics set Status=@Status Where Status <> @Status and JobType in (@Type1,@Type2,@Type3,@Type4,@Type5,@Type6,@Type7,@Type8,@Type9,@Type10,@Type11,@Type12,@Type13,@Type14,@Type15,@Type16,@Type17,@Type18,@Type19,@Type20,@Type21,@Type22) and StatisticsTime>0";
                var paras = new SqlParameter[]
                {
                     new SqlParameter("Status", (int)StatisticsStatus.Abandoned),
                     new SqlParameter("Type1", (int)JobType.ArchiverOutPlaceRestore),
                     new SqlParameter("Type2", (int)JobType.DiscoverOptimization),
                     new SqlParameter("Type3", (int)JobType.RecordsDisposal),
                     new SqlParameter("Type4", (int)JobType.OneDriveRecordsDisposal),
                     new SqlParameter("Type5", (int)JobType.ArchiverRestore),
                     new SqlParameter("Type6", (int)JobType.RMArchiverBackup),//TODO Cyrus SpecifySitesArchiverBackup
                     new SqlParameter("Type7", (int)JobType.StubOopRestore),
                     new SqlParameter("Type8", (int)JobType.ApprovalProcessArchive),
                     new SqlParameter("Type9", (int)JobType.TeamsArchiverBackup),
                     new SqlParameter("Type10", (int)JobType.TeamsRecordsDisposal),
                     new SqlParameter("Type11", (int)JobType.TeamsArchiverRestore),
                     new SqlParameter("Type12", (int)JobType.DiscoveryAOSPOptimization),
                     new SqlParameter("Type13", (int)JobType.AOSPRestore),
                     new SqlParameter("Type14", (int)JobType.MailBoxArchiverRestore),
                     new SqlParameter("Type15", (int)JobType.SpecifyTeamsArchiverBackup),
                     new SqlParameter("Type16", (int)JobType.RMEndUserArchiverBackup),
                     new SqlParameter("Type17", (int)JobType.ArchiverByHSMXml),
                     new SqlParameter("Type18",(int)JobType.CleanUpDuplicateDatas),
                     new SqlParameter("Type19", (int)JobType.ArchiverToSpoRestore),
                     new SqlParameter("Type20", (int)JobType.TeamsOutPlaceRestore),
                     new SqlParameter("Type21", (int)JobType.StubArchiverRestore),
                     new SqlParameter("Type22", (int)JobType.M365InPlaceArchiverRestore)
                };
                context.Database.ExecuteSqlCommand(sql, paras);
            });
        }
        public async Task UpdateRestoreJobStatisticsStatusAsync()
        {
            ExecuteWithRetry(context =>
            {
                string sql = $"Update [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMJobSizeAndCountStatistics set Status=@Status Where Status <> @Status and JobType in (@Type1,@Type2,@Type3,@Type4,@Type5,@Type6,@Type7,@Type8,@Type9) and StatisticsTime>0";
                var paras = new SqlParameter[]
                {
                     new SqlParameter("Status", (int)StatisticsStatus.Abandoned),
                     new SqlParameter("Type1", (int)JobType.ArchiverOutPlaceRestore),
                     new SqlParameter("Type2", (int)JobType.ArchiverRestore),
                     new SqlParameter("Type3", (int)JobType.StubOopRestore),
                     new SqlParameter("Type4", (int)JobType.TeamsArchiverRestore),
                     new SqlParameter("Type5", (int)JobType.MailBoxArchiverRestore),
                     new SqlParameter("Type6", (int)JobType.ArchiverToSpoRestore),
                     new SqlParameter("Type7", (int)JobType.TeamsOutPlaceRestore),
                     new SqlParameter("Type8", (int)JobType.StubArchiverRestore),
                     new SqlParameter("Type9", (int)JobType.M365InPlaceArchiverRestore),
                };
                context.Database.ExecuteSqlCommand(sql, paras);
            });
        }

        public async Task UpdateAOSPRestoreJobStatisticsStatusAsync()
        {
            ExecuteWithRetry(context =>
            {
                string sql = $"Update [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMJobSizeAndCountStatistics set Status=@Status Where Status <> @Status and JobType=@Type1 and StatisticsTime>0";
                var paras = new SqlParameter[]
                {
                     new SqlParameter("Status", (int)StatisticsStatus.Abandoned),
                     new SqlParameter("Type1", (int)JobType.AOSPRestore),
                };
                context.Database.ExecuteSqlCommand(sql, paras);
            });
        }

        public async Task UpdateJobStatisticsAsync(JobType jobType, int keepDataOption, long totalSize, bool isTrailLicence = false)
        {

            ExecuteWithRetry(context =>
            {
                string sql = $"Update [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMJobSizeAndCountStatistics set Size=Size+@TotalSize Where JobType = @JobType and KeepDataOption=@KeepDataOption and LicenceType = @LicenceType" +
                $" IF @@ROWCOUNT = 0 BEGIN INSERT INTO [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMJobSizeAndCountStatistics VALUES(@Id, @JobName,@JobType,@KeepDataOption,@TotalSize,0,null,@LicenceType) END";
                var paras = new SqlParameter[]
                {
                        new SqlParameter("TotalSize", totalSize),
                        new SqlParameter("JobType", jobType),
                        new SqlParameter("KeepDataOption", keepDataOption),
                        new SqlParameter("LicenceType", isTrailLicence?(int)Cloud.Sdk.Data.AosModern.LicenseType.Trial:(int)Cloud.Sdk.Data.AosModern.LicenseType.Enterprise),
                        new SqlParameter("Id", Guid.NewGuid()),
                        new SqlParameter("JobName", jobType.ToString()),
                };
                context.Database.ExecuteSqlCommand(sql, paras);
            });
        }
    }
    public enum StatisticsStatus
    {
        None = 0,
        Using = 1,
        Abandoned = 2
    }
}
