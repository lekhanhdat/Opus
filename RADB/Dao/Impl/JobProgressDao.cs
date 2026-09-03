using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class JobProgressDao : BaseDao<RMJobProgress>, IJobProgressDao
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(JobProgressDao));

        public async Task<int> GetJobProgressCountAsync(string conditionFilter, BaseJobDto jobInfo)
        {
            using var perfScope = new PerformanceScope("GetJobProgressCount");
            using var context = GetNewContext();
            string sql = string.Format("SELECT COUNT(*) FROM {0} WHERE {1}",
                    $"{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.{SecurityUtils.SanitizeSQLParameterName("RMJobProgresses")}",
                    string.IsNullOrEmpty(conditionFilter) ? "SubJobID LIKE @SubJobID" : conditionFilter + " AND SubJobID LIKE @SubJobID");
            int result = 0;
            try
            {
                List<SqlParameter> parameters = [];
                parameters.Add(new SqlParameter("@SubJobID", $"{jobInfo.Id}%"));
                if (jobInfo.AddValues != null)
                {
                    foreach (var item in jobInfo.AddValues)
                    {
                        parameters.Add(new SqlParameter(item.Key, item.Value));
                    }
                }
                result = await context.Database.SqlQuery<int>(sql, [.. parameters]).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error($"GetJobProgressCountAsync failed, jobInfo: {jobInfo?.Id}, exception: {ex}");
            }
            return result;
        }

        public async Task<IEnumerable<RMJobProgress>> GetJobProgressesAsync(int pageSize, int pageNumber, string conditionFilter, BaseJobDto jobInfo)
        {
            using var perfScope = new PerformanceScope("GetJobProgresses");
            using var context = GetNewContext();
            string orderBy = jobInfo.IsGettingProgress ? $@"CASE WHEN ProgressStatus IN ({string.Join(", ", JobServiceUtility.LowerProgressStatuses)}) THEN 1 ELSE 0 END, StartTime DESC" : "SubJobID";
            string sql = string.Format("SELECT * FROM {0} WHERE {1} ORDER BY {2} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
                    $"{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.{SecurityUtils.SanitizeSQLParameterName("RMJobProgresses")}",
                    string.IsNullOrEmpty(conditionFilter) ? "SubJobID LIKE @SubJobID" : "(" + conditionFilter.Trim() + ") AND SubJobID LIKE @SubJobID",
                    orderBy);
            IEnumerable<RMJobProgress> result = [];
            try
            {
                List<SqlParameter> parameters = [];
                parameters.Add(new SqlParameter("@SubJobID", $"{jobInfo.Id}%"));
                parameters.Add(new SqlParameter("@Offset", (pageNumber - 1) * pageSize));
                parameters.Add(new SqlParameter("@PageSize", pageSize));
                if (jobInfo.AddValues is not null)
                {
                    foreach (var item in jobInfo.AddValues)
                    {
                        parameters.Add(new SqlParameter(item.Key, item.Value));
                    }
                }
                result = await context.JobProgresses.SqlQuery(sql, [.. parameters]).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error($"GetJobProgressesAsync failed, jobInfo: {jobInfo?.Id}, exception: {ex}");
            }
            return result;
        }

        public async Task<RMJobProgress> GetJobProgressBySubJobIdAsync(string subJobId)
        {
            using var perfScope = new PerformanceScope("GetJobProgressBySubJobId");
            using var context = GetNewContext();
            RMJobProgress result = null;
            try
            {
                result = await context.JobProgresses.FindAsync(subJobId);
            }
            catch (Exception ex)
            {
                _logger.Error($"GetJobProgressBySubJobIdAsync failed, subJobId: {subJobId}, exception: {ex}");
            }
            return result;
        }

        public async IAsyncEnumerable<IEnumerable<RMJobProgress>> GetJobProgressesByMainJobIdAsync(string mainJobId)
        {
            using var perfScope = new PerformanceScope("GetJobProgressesByMainJobId");
            using var context = GetNewContext();
            int take = 1000, offset = 0;
            while (true)
            {
                var jobProgresses = await context.JobProgresses
                    .Where(jp => jp.SubJobID.StartsWith(mainJobId))
                    .OrderBy(jp => jp.SubJobID)
                    .Skip(offset)
                    .Take(take)
                    .ToListAsync();
                if (jobProgresses.Count == 0)
                {
                    break;
                }
                yield return jobProgresses;
                offset += take;
            }
        }

        public async Task<bool> AddJobProgressAsync(RMJobProgress jobProgress)
        {
            using var perfScope = new PerformanceScope("AddJobProgress");
            bool result = true;
            try
            {
                await ExecuteWithRetryAsync(async context =>
                {
                    context.JobProgresses.Add(jobProgress);
                    return await context.SaveChangesAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"AddJobProgressAsync failed, jobProgress: {jobProgress?.SubJobID}, exception: {ex}");
                result = false;
            }
            return result;
        }

        public async Task<bool> UpdateJobProgressAsync(RMJobProgress jobProgress)
        {
            using var perfScope = new PerformanceScope("UpdateJobProgress");
            bool result = true;
            try
            {
                await ExecuteWithRetryAsync(async context =>
                {
                    context.Entry(jobProgress).State = EntityState.Modified;
                    return await context.SaveChangesAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"UpdateJobProgressAsync failed, jobProgress: {jobProgress?.SubJobID}, exception: {ex}");
                result = false;
            }
            return result;
        }

        public async Task<int> ClearJobProgressesByJobIdAsync(string mainJobId)
        {
            using var perfScope = new PerformanceScope("ClearJobProgressesByJobId");
            int limitDeleteCount = 5000, timeout = 600;
            int totalDeleted = 0, deletedRows = 0;
            try
            {
                do
                {
                    deletedRows = await ExecuteWithRetryAsync(async context =>
                    {
                        context.Database.CommandTimeout = timeout;
                        string sql = string.Format("DELETE TOP ({0}) FROM {1} WHERE SubJobID LIKE @MainJobID",
                            limitDeleteCount,
                            $"{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.{SecurityUtils.SanitizeSQLParameterName("RMJobProgresses")}");
                        return await context.Database.ExecuteSqlCommandAsync(sql, new SqlParameter("@MainJobID", $"{mainJobId}%"));
                    });
                    totalDeleted += deletedRows;
                }
                while (deletedRows > 0);
            }
            catch (Exception ex)
            {
                _logger.Error($"ClearJobProgressesByJobIdAsync failed, mainJobId: {mainJobId}, exception: {ex}");
            }
            return totalDeleted;
        }

        public async Task<int> UpdateRemainingSubJobStatusAsync(string mainJobId, HashSet<int> originalStatuses, int newStatus)
        {
            using var perfScope = new PerformanceScope("UpdateRemainingSubJobStatus");
            int limitUpdateCount = 5000, timeout = 600;
            int totalUpdated = 0, updatedRows = 0;
            try
            {
                var statusInClause = DatabaseUtility.BuildInClause(originalStatuses, out var parameters);
                parameters.AddRange([
                    new("@NewStatus", newStatus),
                    new("@NewProgressStatus", (int)JobReportUtility.ConvertJobStatusToProgressStatus((JobStatus)newStatus)),
                    new("@MainJobID", $"{mainJobId}%"),
                ]);
                var progressStatusInClause = DatabaseUtility.BuildInClause(originalStatuses.Select(s => (int)JobReportUtility.ConvertJobStatusToProgressStatus((JobStatus)s)), out var progressStatusParameters);
                parameters.AddRange(progressStatusParameters);
                do
                {
                    updatedRows = await ExecuteWithRetryAsync(async context =>
                    {
                        context.Database.CommandTimeout = timeout;
                        string sql = string.Format("UPDATE TOP ({0}) {1} SET Status = @NewStatus, ProgressStatus = @NewProgressStatus WHERE SubJobID LIKE @MainJobID AND (Status IN {2} OR ProgressStatus IN {3})",
                            limitUpdateCount,
                            $"{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.{SecurityUtils.SanitizeSQLParameterName("RMJobProgresses")}",
                            statusInClause,
                            progressStatusInClause);
                        return await context.Database.ExecuteSqlCommandAsync(sql, parameters.ToArray());
                    });
                    totalUpdated += updatedRows;
                }
                while (updatedRows > 0);
            }
            catch (Exception ex)
            {
                _logger.Error($"UpdateRemainingSubJobStatusAsync failed, mainJobId: {mainJobId}, exception: {ex}");
            }
            return totalUpdated;
        }

        public async Task<bool> BatchAddJobProgressesBySubJobsAsync(IEnumerable<RMSubJob> subJobs)
        {
            using var perfScope = new PerformanceScope("BatchAddJobProgressesBySubJobs");
            bool result = true;
            try
            {
                _logger.Info($"BatchAddJobProgressesBySubJobsAsync start, subJobs count: {subJobs?.Count()}");
                using (new PerformanceScope("BatchAddJobProgressesBySubJobs"))
                {
                    var tableName = GetFullTableName();
                    using var table = ConvertToDataTable(subJobs);
                    table.TableName = tableName;
                    await BatchAddAsync(table, tableName);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"BatchAddJobProgressesBySubJobsAsync failed, exception: {ex}");
                result = false;
            }
            return result;
        }

        private string GetFullTableName()
        {
            return $"[{SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}].[RMJobProgresses]";
        }

        private DataTable ConvertToDataTable(IEnumerable<RMSubJob> subJobs)
        {
            var table = new DataTable();
            table.Columns.Add("SubJobID", typeof(String));
            table.Columns.Add("Status", typeof(Int32));
            table.Columns.Add("ProgressStatus", typeof(Int32));
            table.Columns.Add("Scope", typeof(String));
            table.Columns.Add("Comment", typeof(String));
            foreach (var subJob in subJobs)
            {
                var row = table.NewRow();
                row["SubJobID"] = subJob.Id;
                row["Status"] = subJob.Status;
                row["ProgressStatus"] = (int)JobReportUtility.ConvertJobStatusToProgressStatus((JobStatus)subJob.Status);
                row["Scope"] = subJob.String1;
                row["Comment"] = string.Empty;
                table.Rows.Add(row);
            }
            return table;
        }
    }
}
