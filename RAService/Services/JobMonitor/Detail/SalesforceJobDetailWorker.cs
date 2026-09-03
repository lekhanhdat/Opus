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
using System.Collections.Generic;
using System.Linq;
using AvePoint.RA.Common;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;

namespace AvePoint.RA.Service.Services.JobMonitor.Detail;

public class SalesforceJobDetailWorker : AbstractJobDetailWorker
{
    private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();

    public override void InsertData(IEnumerable<JMJobDetails> jobDetails, BaseJobDto jobInfo)
    {
        InitCreateTableSQLString();
        string reportFilePath = NeedCreateTable(jobInfo);
        JobDetailDao.SaveDataIntoTable(reportFilePath, jobDetails, this.INSERT_DATA_SQL);
    }
    
    public void InitCreateTableSQLString()
    {
        TABLE_NAME = JobMonitorConstants.JOBDETAIL;
        CREATE_TABLE_SQL = string.Format(JobMonitorConstants.CREATE_TABLE_SalesforceDiscovery, TABLE_NAME);
        INSERT_DATA_SQL = string.Format(JobMonitorConstants.INSERT_DATA_SalesforceDiscovery, TABLE_NAME);
    }

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
        if (JobServiceUtility.IsSubJob(jobInfo.Id))
        {
            var nextSubJob = SubJobDao.IsFinalSubJob(jobInfo.Id,jobInfo.Id.Split('_')[0]).GetAwaiter().GetResult();
            if(nextSubJob)
            {
                return null;
            }
        }
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
    public override void InitGetDataSQLString(int PageSize, int StartPage, string conditionFilter)
    {
        if (string.IsNullOrEmpty(conditionFilter))
        {
            SELECT_DATA_SQL = string.Format(JobMonitorConstants.SELECT_DATA_FROM_TABLE_ORDERBY_CONDITONSTR, TABLE_NAME, nameof(JMSalesforceDiscoveryJob.ObjectName), PageSize, (StartPage - 1) * PageSize) ;
            SELECT_DETAIL_COUNT_SQL = string.Format(JobMonitorConstants.SELECT_DETAIL_COUNT_SQL, TABLE_NAME);
        }
        else
        {
            SELECT_DATA_SQL = string.Format(JobMonitorConstants.SELECT_DATA_ON_CONDITION_FROM_TABLE_ORDERBY_CONDITONSTR, TABLE_NAME, conditionFilter,nameof(JMSalesforceDiscoveryJob.ObjectName), PageSize, (StartPage - 1) * PageSize) ;
            SELECT_DETAIL_COUNT_SQL = string.Format(JobMonitorConstants.SELECT_DETAIL_COUNT_ON_CONDITION_SQL, TABLE_NAME, conditionFilter);
        }
    }
}