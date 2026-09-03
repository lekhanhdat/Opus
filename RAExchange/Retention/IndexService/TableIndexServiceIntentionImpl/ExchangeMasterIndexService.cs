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
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.Media.Core.Index;
    #endregion

    public class ExchangeMasterIndexService
        : ExchangeTableIndexServiceBase
        , IExchangeMasterIndexService
    {
        //public IIndexProcessor<ExchangeIndexProcessorParameter> IndexProcessor = new IndexProcessor<ExchangeIndexProcessorParameter>();

        static readonly String updateSiteMasterIndexPruneState = "update " + IndexConstants.TableNameExchangeSiteMaster
           + " set COL_MODIFY_DATA = @COL_MODIFY_DATA where COL_JOB_ID = @COL_JOB_ID";

        public void InsertSiteMasterIndex(GroupMasterIndex siteMaster)
        {
            this.IndexProcessor.Insert<GroupMasterIndex>(siteMaster);
        }

        public void UpdateSiteMasterIndex(GroupMasterIndex siteMaster)
        {
            var parameters = new Dictionary<string, object>();
            parameters["@COL_MODIFY_DATA"] = siteMaster.ModifyData;
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
    }
}