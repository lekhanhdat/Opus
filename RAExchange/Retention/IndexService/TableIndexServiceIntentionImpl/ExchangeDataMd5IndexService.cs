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

namespace RAExchangeRetention
{
    using System;
    using System.Collections.Generic;

    using AvePoint.Media.Service.DomainModel;
    using AvePoint.RA.CommonUtil;

    internal class ExchangeDataMd5IndexService
        : ExchangeTableIndexServiceBase,
         IExchangeDataMd5IndexService
    {
        private RALogger logger = RALogger.GetInstance(typeof(ExchangeDataMd5IndexService));

        public void InsertDataMd5Index(GroupDataMd5Index dataMd5)
        {
            this.IndexProcessor.Insert<GroupDataMd5Index>(dataMd5);
        }

        public void UpdateDataMd5Index(GroupDataMd5Index dataMd5)
        {
            String updateDataMd5IndexPruneState = "update " + IndexConstants.TableNameExchangeDataMd5
                + " set COL_DATA_MD5 = @COL_DATA_MD5,COL_DATA_OBJECT_ID = @COL_DATA_OBJECT_ID"
                + " where COL_JOB_ID = @COL_JOB_ID And COL_DATA_NAME = @COL_DATA_NAME ";
            var parameters = new Dictionary<string, object>();
            parameters["@COL_DATA_MD5"] = dataMd5.DataMd5;
            parameters["@COL_DATA_OBJECT_ID"] = dataMd5.DataObjectId;
            parameters["@COL_JOB_ID"] = dataMd5.JobId;
            parameters["@COL_DATA_NAME"] = dataMd5.DataName;
            this.IndexProcessor.Execute(updateDataMd5IndexPruneState, parameters);
        }

        public void DeleteDataMd5IndexByJobId(string jobId)
        {
            var removeCommand = "delete from " + IndexConstants.TableNameExchangeDataMd5 + " where COL_JOB_ID = @COL_JOB_ID";
            var parameters = new Dictionary<string, object>();
            parameters.Add("@COL_JOB_ID", jobId);
            this.IndexProcessor.Execute(removeCommand, parameters);
        }

        public List<GroupDataMd5Index> GetDataMd5(string jobId)
        {
            var indexList = new List<GroupDataMd5Index>();
            var parameters = new Dictionary<string, object>();
            var sql = "select * from " + IndexConstants.TableNameExchangeDataMd5;
            if (!string.IsNullOrEmpty(jobId))
            {
                parameters.Add("@COL_JOB_ID", jobId);
                sql = sql + " where COL_JOB_ID = @COL_JOB_ID";
            }
            indexList = this.IndexProcessor.ExecuteQuery<GroupDataMd5Index>(sql, parameters);
            return indexList;
        }

        private const string DataMd5TableName = "tb_datamd5_index";

        private const string CreateDataMd5TableScript = @"CREATE TABLE [tb_datamd5_index](
                                          [COL_PLAN_ID] VARCHAR(256),
                                          [COL_CYCLE_ID] VARCHAR(256),
                                          [COL_JOB_ID] VARCHAR(256),
                                          [COL_DATA_NAME] VARCHAR(32672),
                                          [COL_DATA_OBJECT_ID] VARCHAR(32672),
                                          [COL_DATA_MD5] CHAR(32)
                                );";

        public void CreateTableDataMd5()
        {
            var parameters = new Dictionary<String, Object>();
            var sqlCheckTable = string.Format("SELECT count(*) FROM sqlite_master WHERE type='table' and name = '{0}'; ", DataMd5TableName);
            object count = this.IndexProcessor.ExecuteScalar(sqlCheckTable, parameters);
            if (Convert.ToInt64(count) != 1)
                this.IndexProcessor.Execute(CreateDataMd5TableScript, parameters);
        }

        public GroupDataMd5Index GetCurrentDataMd5(string jobId, string dataName, string dataObjectId)
        {
            var index = default(GroupDataMd5Index);
            var parameters = new Dictionary<string, object>();
            parameters.Add("@COL_JOB_ID", jobId);
            string tempSql = string.Empty;
            if (!string.IsNullOrEmpty(dataObjectId) && CheckColumnExist("COL_DATA_OBJECT_ID", "VARCHAR(32672)"))
            {
                parameters.Add("@COL_DATA_OBJECT_ID", dataObjectId);
                tempSql = "COL_DATA_OBJECT_ID = @COL_DATA_OBJECT_ID ";
            }
            else
            {
                parameters.Add("@COL_DATA_NAME", dataName);
                tempSql = "COL_DATA_NAME = @COL_DATA_NAME ";
            }
            var sql = "select * from " + IndexConstants.TableNameExchangeDataMd5
                  + " where COL_JOB_ID = @COL_JOB_ID and " + tempSql;
            var indexList = this.IndexProcessor.ExecuteQuery<GroupDataMd5Index>(sql, parameters);
            if (indexList.Count > 0)
                index = indexList[0];
            return index;
        }

        public void AddColumn(string columnName, string declaredType)
        {
            if (!CheckColumnExist(columnName, declaredType))
            {
                var parameters = new Dictionary<String, Object>();
                var sql = string.Format("ALTER TABLE tb_datamd5_index ADD [{0}] {1}", columnName, declaredType);//COL_DATA_OBJECT_ID VARCHAR(32672)
                this.IndexProcessor.Execute(sql, parameters);
            }
        }

        private bool CheckColumnExist(string columnName, string declaredType)
        {
            var parameters = new Dictionary<String, Object>();
            var sql = "select sql from sqlite_master where tbl_name='tb_datamd5_index' and type='table'";
            var createSql = this.IndexProcessor.ExecuteScalar(sql, parameters);
            return createSql.ToString().Contains(string.Format("[{0}] {1}", columnName, declaredType));
        }
    }
}