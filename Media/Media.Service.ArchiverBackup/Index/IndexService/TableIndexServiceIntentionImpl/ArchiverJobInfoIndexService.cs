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



namespace AvePoint.Media.Service.ArchiverBackup
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.Media.Service.DomainModel;
    #endregion

    public class ArchiverJobInfoIndexService
        : ArchiverTableIndexServiceBase
        , IArchiverJobInfoIndexService
    {
        static readonly string updateJobInfoValue = "update " + IndexConstants.TableNameArchiveJobInfo
                    + " set COL_VALUE = @COL_VALUE where COL_JOB_ID = @COL_JOB_ID and COL_KEY = @COL_KEY ";

        public void UpdateJobInfoIndex(String jobId, String key, String value)
        {
            var sql = "select count(*) from tb_job_info where";
            var parameters = new Dictionary<String, Object>();
            if (!string.IsNullOrEmpty(jobId))
            {
                sql += " COL_JOB_ID = @COL_JOB_ID and ";
                parameters["@COL_JOB_ID"] = jobId;
            }
            sql += " COL_KEY=@COL_KEY";
            parameters["@COL_KEY"] = key;
            Object objInt = this.IndexProcessor.ExecuteScalar(sql, parameters);
            int count;
            if (!int.TryParse(objInt.ToString(), out count))
                count = 0;
            if (count == 0)
            {
                var jobInfo = new ArchiverJobInfoIndex();
                jobInfo.Guid = Guid.NewGuid().ToString();
                jobInfo.JobId = jobId;
                jobInfo.Key = key;
                jobInfo.Value = value;
                this.IndexProcessor.Insert<ArchiverJobInfoIndex>(jobInfo);
            }
            else
            {
                parameters["@COL_VALUE"] = value;
                this.IndexProcessor.Execute(updateJobInfoValue, parameters);
            }
        }

        public List<ArchiverJobInfoIndex> GetJobInfoIndexesByKey(String key)
        {
            var parameters = new Dictionary<String, Object>();
            StringBuilder sb = new StringBuilder();
            sb.Append("select * from ").Append(IndexConstants.TableNameArchiveJobInfo).Append(" where COL_KEY = @KEY");
            parameters.Add("@KEY", key);
            return this.IndexProcessor.ExecuteQuery<ArchiverJobInfoIndex>(sb.ToString(), parameters);
        }
    }
}