using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using System.Collections.Generic;

namespace AvePoint.RA.Service.Services.JobMonitor.Detail
{
    public class MainDataSyncCommonDataJobDetailWorker : AbstractJobDetailWorker
    {
        public override IEnumerable<JMJobDetails> GetData(int PageSize, int StartPage, ref int totalCount, string conditionFilter, BaseJobDto jobInfo)
        {
            string reportFilePath = DownloadReports(jobInfo);
            TABLE_NAME = JobMonitorConstants.JOBDETAIL;
            InitGetDataSQLString(PageSize, StartPage, conditionFilter);
            totalCount = base.GetCountForDetail(reportFilePath, base.SELECT_DETAIL_COUNT_SQL, jobInfo);
            return GetData(PageSize, StartPage, conditionFilter, jobInfo);
        }

        public override IEnumerable<JMJobDetails> GetData(int PageSize, int StartPage, string conditionFilter, BaseJobDto jobInfo)
        {
            IEnumerable<JMJobDetails> result = null;
            string reportFilePath = DownloadReports(jobInfo);
            TABLE_NAME = JobMonitorConstants.JOBDETAIL;
            InitGetDataSQLString(PageSize, StartPage, conditionFilter);
            bool isRPTExist = CheckFileExist(reportFilePath);
            bool isTableInRPTExist = JobDetailDao.IsExistTable(reportFilePath, TABLE_NAME);
            if (!isRPTExist && !isTableInRPTExist)
            {
                logger.Debug("about {0} database exist:{1},table exist{2}", jobInfo.Id, isRPTExist, isTableInRPTExist);
                return result;
            }
            result = JobDetailDao.GetData(reportFilePath, base.SELECT_DATA_SQL, jobInfo);
            return result;
        }

        public override void InsertData(IEnumerable<JMJobDetails> jobDetails, BaseJobDto jobInfo)
        {
            InitCreateTableSQLString();
            string reportFilePath = NeedCreateTable(jobInfo);
            JobDetailDao.SaveDataIntoTable(reportFilePath, jobDetails, this.INSERT_DATA_SQL);
        }
        public void InitCreateTableSQLString()
        {
            TABLE_NAME = JobMonitorConstants.JOBDETAIL;
            CREATE_TABLE_SQL = string.Format(JobMonitorConstants.CREATE_TABLE_MainSyncCommonData, TABLE_NAME);
            INSERT_DATA_SQL = string.Format(JobMonitorConstants.INSERT_DATA_MainSyncCommonData, TABLE_NAME);
        }
    }
}
