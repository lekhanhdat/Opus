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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object.ArchiverMigration;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus;
using ArchiverJobStatus = AvePoint.Common.JobState;
using AvePoint.GCommon.Utility;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class ArchiverJobDao : BaseDao<RMArchiverJob>, IArchiverJobDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(ArchiverJobDao));

        public RMArchiverJob GetJobByID(string id)
        {
            using (var context = GetNewContext())
            {
                return context.ArchiverJobs.AsQueryable().Where(j => j.Id == id).FirstOrDefault();
            }
        }

        public List<RMArchiverJob> GetJobByRECOJobID(string recoJobId)
        {
            using (var context = GetNewContext())
            {
                return context.ArchiverJobs.AsQueryable().Where(j => j.RECOJobId == recoJobId).OrderBy(j => j.StartTime).ToList();
            }
        }

        private List<string> GetAllExistArchiverJobId()
        {
            using (var context = GetNewContext())
            {
                return context.ArchiverJobs.AsQueryable().Where(j => j.DAOMigrated == null || !j.DAOMigrated.Value).Select(j => j.Id).ToList();
            }
        }

        public void UpdateJob(RMArchiverJob job)
        {
            using (var context = GetNewContext())
            {
                var jobdb = context.ArchiverJobs.AsQueryable().Where(j => j.Id == job.Id).FirstOrDefault();
                if (jobdb == null)
                {
                    context.ArchiverJobs.Add(job);
                    context.SaveChanges();
                   //Create(job);
                }
                else
                {
                    jobdb.Id = job.Id;
                    jobdb.JobCategory = job.JobCategory;
                    jobdb.JobType = job.JobType;
                    jobdb.Order = job.Order;
                    jobdb.PlanId = job.PlanId;
                    jobdb.Progress = job.Progress;
                    jobdb.RECOJobId = job.RECOJobId;
                    jobdb.Scope = job.Scope;
                    jobdb.StartTime = job.StartTime;
                    jobdb.EndTime = job.EndTime;
                    jobdb.StatusFromDAOL = job.StatusFromDAOL;
                    jobdb.UserName = job.UserName;
                    ApplyCurrentValues(context, jobdb);
                }
            }
        }


        public async Task<int> ClearOldArchiverJobsAsync()
        {
            var sql = $@"DELETE FROM [{SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}].RMArchiverJobs";
            using (var context = GetNewContext())
            {
                return await context.Database.ExecuteSqlCommandAsync(sql);
            }
        }

        public async Task BulkMigrateJobsAsync(IEnumerable<ArchiverMigrationJobDto> jobs)
        {
            if (jobs.Count() == 0)
            {
                return;
            }

            var lfAllArchiverJobs = GetAllExistArchiverJobId();
            logger.Debug("Total migrate archiver jobs: {0}", jobs.Count());
            using (new PerformanceScope("Batch migrate archiver jobs"))
            {
                var tableName = GetFullTableName();
                using (var table = ConvertToDataTable(jobs, lfAllArchiverJobs))
                {
                    table.TableName = tableName;
                    await BatchAddAsync(table, tableName);
                }
            }
        }

        private string GetFullTableName()
        {
            return $"[{SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}].[RMArchiverJobs]";
        }

        private DataTable ConvertToDataTable(IEnumerable<ArchiverMigrationJobDto> items, List<string> lfAllArchiverJobs)
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(String));
            table.Columns.Add("RECOJobId", typeof(String));
            table.Columns.Add("JobType", typeof(Int32));
            table.Columns.Add("JobCategory", typeof(Int32));
            table.Columns.Add("PlanId", typeof(String));
            table.Columns.Add("Order", typeof(Int32));
            table.Columns.Add("Progress", typeof(Int32));
            table.Columns.Add("Scope", typeof(String));
            table.Columns.Add("UserName", typeof(String));
            table.Columns.Add("StatusFromDAOL", typeof(Int32));
            table.Columns.Add("StartTime", typeof(Int64));
            table.Columns.Add("EndTime", typeof(Int64));
            table.Columns.Add("Comment", typeof(String));
            table.Columns.Add("DAOMigrated", typeof(bool));

            Regex regex = new("^AR|^PAR|^EAR");

            foreach (var item in items)
            {
                if (lfAllArchiverJobs.Any(j => j == item.Id))
                {
                    logger.Info($"{item.Id} exist, skip insert RMArchiverJobs");
                    continue;
                }
                var jobType = item.JobType switch
                {
                    (int)JobType.ArchiverScan => (int)JobType.MigrationArchiverScan,
                    (int)JobType.ArchiverBackup => (int)JobType.MigrationArchiverBackup,
                    _ => item.JobType
                };
                var row = table.NewRow();
                var isDaJob = JobTypeConstants.MigrationDisposalJobTypes.Contains(item.JobType);
                row["Id"] = item.Id;
                row["RECOJobId"] = isDaJob ? regex.Replace(item.Id, "DA").Replace("S", "").Replace("A0", "") : item.Id;
                row["JobType"] = jobType;
                row["JobCategory"] = item.JobCategory;
                row["PlanId"] = item.PlanId;
                row["Order"] = (isDaJob && item.Id.Contains("A0")) ? 2 : 1;
                row["Progress"] = item.Progress;
                row["Scope"] = item.ScopeId;
                row["UserName"] = item.UserName;
                row["StatusFromDAOL"] = ConvertJobStatus((JobStatus)item.Status);
                row["StartTime"] = item.StartTime;
                row["EndTime"] = item.EndTime;
                row["Comment"] = item.Comment;
                row["DAOMigrated"] = true;
                table.Rows.Add(row);
            }

            return table;
        }
        private static ArchiverJobStatus ConvertJobStatus(JobStatus state)
        {
            return state switch
            {
                JobStatus.None => ArchiverJobStatus.None,
                JobStatus.Wait => ArchiverJobStatus.Waiting,
                JobStatus.InProgress => ArchiverJobStatus.InProgress,
                JobStatus.Finished => ArchiverJobStatus.Finished,
                JobStatus.Failed => ArchiverJobStatus.Failed,
                JobStatus.FinishWithException => ArchiverJobStatus.FinishedException,
                JobStatus.Stopped => ArchiverJobStatus.Stopped,
                JobStatus.Skipped => ArchiverJobStatus.Skiped,
                JobStatus.Stopping => ArchiverJobStatus.Stopping,
                JobStatus.Calculating => ArchiverJobStatus.Started,
                JobStatus.Pending => ArchiverJobStatus.Pending,
                _ => ArchiverJobStatus.None,
            };
        }

        public async Task<int> UpdateMigratedJobsInfoAsync()
        {
            var sql = 
$@"UPDATE [{SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}].RMJobMonitors 
SET Status=sj.StatusFromDAOL, EndTime=sj.EndTime, Comment=sj.Comment
FROM [{SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}].RMJobMonitors AS mj
JOIN [{SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}].[RMArchiverJobs] AS sj ON sj.RECOJobId=mj.Id
WHERE sj.[Order]=2";

            using (var context = GetNewContext())
            {
                return await context.Database.ExecuteSqlCommandAsync(sql);
            }
        }

        public async Task<int> DeleteMigratedArchiverJobsAsync()
        {
            var sql = $"DELETE FROM [{SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}].RMArchiverJobs WHERE DAOMigrated=1";

            using (var context = GetNewContext())
            {
                return await context.Database.ExecuteSqlCommandAsync(sql);
            }
        }
        public async Task<int> DeleteMigratedMainJobsAsync()
        {
            var sql = $"DELETE FROM [{SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}].RMJobMonitors WHERE DAOMigrated=1";

            using (var context = GetNewContext())
            {
                return await context.Database.ExecuteSqlCommandAsync(sql);
            }
        }
    }
}
