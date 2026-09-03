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
    using AvePoint.Media.Service.DomainModel;
    using global::Media.Service.ArchiverBackup.Index.IndexService.TableIndexServiceIntention;
    #endregion

    public class GDrvieMasterIndexService
        : GDriveArchiverTableIndexServiceBase, IGDriveMasterIndexService
    {

        static readonly String updateSiteMasterIndexPruneState = "update " + IndexConstants.TableNameGDriveMaster
           + " set COL_MODIFY_DATA = @COL_MODIFY_DATA where COL_JOB_ID = @COL_JOB_ID";

        public void InsertSiteMasterIndex(GDriveMasterIndex siteMaster)
        {
            this.IndexProcessor.Insert<GDriveMasterIndex>(siteMaster);
        }

        public void UpdateSiteMasterIndex(GDriveMasterIndex siteMaster)
        {
            var parameters = new Dictionary<string, object>();
            parameters["@COL_MODIFY_DATA"] = siteMaster.BackupTime;
            parameters["@COL_JOB_ID"] = siteMaster.JobId;
            this.IndexProcessor.Execute(updateSiteMasterIndexPruneState, parameters);
        }

        public Boolean HasSpecifyJobInfo(String jobId)
        {
            var sql = "select * from " + IndexConstants.TableNameExchangeSiteMaster + " where COL_JOB_ID = @COL_JOB_ID";
            var parameters = new Dictionary<String, object>();
            parameters["@COL_JOB_ID"] = jobId;
            var bodyResult = this.IndexProcessor.ExecuteScalar(sql, parameters);
            return Convert.IsDBNull(bodyResult) ? false : true;
        }

        public void DeleteSiteMasterIndexByJobId(String jobId)
        {
            var removeCommand = "delete from " + IndexConstants.TableNameExchangeSiteMaster + " where COL_JOB_ID = @COL_JOB_ID";
            var parameters = new Dictionary<string, object>();
            parameters.Add("@COL_JOB_ID", jobId);
            this.IndexProcessor.Execute(removeCommand, parameters);
        }

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
    }
}