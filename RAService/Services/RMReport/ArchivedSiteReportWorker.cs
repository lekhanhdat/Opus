using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.JobMonitor;
using System.Collections.Generic;

namespace AvePoint.RA.Service.Services.RMReport
{
    public class ArchivedSiteReportWorker : AbstractReportWorker
    {
        public ArchivedSiteReportWorker()
        {
            InitCreateTableSQLString();
        }

        public override void SaveReportJobDatas(IEnumerable<BaseReport> reports, BaseJobDto jobInfo)
        {
            InitCreateTableSQLString();
            var path = NeedCreateTable(jobInfo);
            ReportCenterDao.SaveReportJobDatas(path, reports, INSERT_DATA_SQL);
        }

        public override IEnumerable<BaseReport> GetReportJobDatas(int pageSize, int startPage, ref int totalCount,
            string conditionFilter, BaseJobDto jobInfo, string sortKey = null, bool isAscending = true)
        {
            InitCreateTableSQLString();
            var path = DownloadReports(jobInfo);
            InitGetDataSQLString(pageSize, startPage, conditionFilter, sortKey, isAscending);
            var result = ReportCenterDao.GetReportJobDatas(path, SELECT_DATA_SQL, jobInfo);
            totalCount = GetCountForDetail(conditionFilter, jobInfo);
            return result;
        }

        public override ReportFilter GetReportJobFilterData(BaseJobDto jobInfo)
        {
            return new ReportFilter { Filters = new Dictionary<ReportFilterType, List<ReportFilterData>>() };
        }

        private void InitCreateTableSQLString()
        {
            TABLE_NAME = ReportConstants.ReportDETAIL;
            CREATE_TABLE_SQL = string.Format(ReportConstants.CREATE_ARCHIVED_SITE_REPORT_TABLE, TABLE_NAME);
            INSERT_DATA_SQL = string.Format(ReportConstants.INSERT_ARCHIVED_SITE_REPORT, TABLE_NAME);
        }
    }
}
