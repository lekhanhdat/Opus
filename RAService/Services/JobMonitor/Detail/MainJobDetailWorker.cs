using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.JobMonitor.Detail
{
    public class MainJobDetailWorker : AbstractJobDetailWorker
    {
        public override IEnumerable<JMJobDetails> GetData(int PageSize, int StartPage, ref int totalCount, string conditionFilter, BaseJobDto jobInfo)
        {
            if (jobInfo.NeedQueryFromUploadLocation)
            {
                totalCount = _jobProgressDao.GetJobProgressCountAsync(conditionFilter, jobInfo).ExecuteAsyncTask();
            }
            else
            {
                string reportFilePath = DownloadReports(jobInfo);
                TABLE_NAME = JobMonitorConstants.JOBDETAIL;
                InitGetDataSQLString(PageSize, StartPage, conditionFilter);
                totalCount = base.GetCountForDetail(reportFilePath, base.SELECT_DETAIL_COUNT_SQL, jobInfo);
            }
            return GetData(PageSize, StartPage, conditionFilter, jobInfo);
        }

        public override IEnumerable<JMJobDetails> GetData(int PageSize, int StartPage, string conditionFilter, BaseJobDto jobInfo)
        {
            IEnumerable<JMJobDetails> result = null;
            if (jobInfo.NeedQueryFromUploadLocation)
            {
                result = _jobProgressDao.GetJobProgressesAsync(PageSize, StartPage, conditionFilter, jobInfo).ExecuteAsyncTask()
                    .Select(jp => jobInfo.IsGettingProgress ? ConvertUtil.ConvertToProgressJobDetails(jp) : ConvertUtil.ConvertToMainJobDetails(jp));
            }
            else
            {
                string reportFilePath = DownloadReports(jobInfo);
                TABLE_NAME = JobMonitorConstants.JOBDETAIL;
                InitGetDataSQLString(PageSize, StartPage, conditionFilter);
                bool isRPTExist = CheckFileExist(reportFilePath);
                bool isTableInRPTExist = JobDetailDao.IsExistTable(reportFilePath, TABLE_NAME);
                if (!isRPTExist || !isTableInRPTExist)
                {
                    logger.Debug("about {0} database exist:{1},table exist{2}", jobInfo.Id, isRPTExist, isTableInRPTExist);
                    return result;
                }
                result = JobDetailDao.GetData(reportFilePath, base.SELECT_DATA_SQL, jobInfo);
            }
            return result;
        }

        public override void InitGetDataSQLString(int PageSize, int StartPage, string conditionFilter)
        {
            var orderByColumn = "SubJobID";
            if (string.IsNullOrEmpty(conditionFilter))
            {
                SELECT_DATA_SQL = string.Format(JobMonitorConstants.SELECT_DATA_FROM_TABLE_ORDERBY_CONDITONSTR, TABLE_NAME, orderByColumn, PageSize, (StartPage - 1) * PageSize);
                SELECT_DETAIL_COUNT_SQL = string.Format(JobMonitorConstants.SELECT_DETAIL_COUNT_SQL, TABLE_NAME);
            }
            else
            {
                SELECT_DATA_SQL = string.Format(JobMonitorConstants.SELECT_DATA_ON_CONDITION_FROM_TABLE_ORDERBY_CONDITONSTR, TABLE_NAME, conditionFilter, orderByColumn, PageSize, (StartPage - 1) * PageSize);
                SELECT_DETAIL_COUNT_SQL = string.Format(JobMonitorConstants.SELECT_DETAIL_COUNT_ON_CONDITION_SQL, TABLE_NAME, conditionFilter);
            }
        }

        public override void InsertData(IEnumerable<JMJobDetails> jobDetails, BaseJobDto jobInfo)
        {
            // UPSERT
            var details = jobDetails.Where(item => item is JMArchiverJobProgressDetails);
            if (details != null && details.Count() > 0)
            {
                InitCreateTableSQLString();
                var reportFilePath = base.NeedCreateTable(jobInfo);
                JobDetailDao.SaveDataIntoTable(reportFilePath, details, this.INSERT_DATA_SQL);
            }
        }

        public void InitCreateTableSQLString()
        {
            TABLE_NAME = JobMonitorConstants.JOBDETAIL;
            CREATE_TABLE_SQL = string.Format(JobMonitorConstants.CREATE_TABLE_MainJobDetails, TABLE_NAME);
            INSERT_DATA_SQL = string.Format(JobMonitorConstants.UPSERT_DATA_MainJobDetails, TABLE_NAME);
        }
    }
}
