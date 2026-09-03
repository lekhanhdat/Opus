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
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.RA.Contract.RMWeb.JobMonitor;
    using AvePoint.RA.Contract.RMWeb.CP;
    using AvePoint.RA.Contract.RMWeb;
    using AvePoint.RA.Contract.Services;
    using AvePoint.RA.I18N.Core;
    using Newtonsoft.Json;
    using AvePoint.RA.Contract.JobMonitor;
    using System.Linq.Expressions;
    using System.Linq;
    using System.Threading.Tasks;
    using RAFileSystem.FileSystem.FileSystem.Backup.CoreIndex.CoreIndexCommon;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Utility;
    using DocAveOnline.WebApi.Contracts;

    #endregion using directives

    [AveCodeReview(
    "2012/8/2",
    "dwxue@avepoint.com",
    "yjhuo@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_EH_5 },
    "ADO-44845",
    true)]

    public class ArchiverSiteMasterIndexService
        : ArchiverTableIndexServiceBase
        , IArchiverSiteMasterIndexService
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly static object MLock = new object();
        public void InsertSiteMaster(ArchiveIndexInfo siteMasterIndex)
        {
            this.IndexProcessor.Insert(siteMasterIndex);
        }
        public void InitIndexProcesser(ArchiverIndexService _indexService)
        {
            this.IndexProcessor = _indexService.IndexProcessor;
        }
        public ArchiveIndexInfo GetSiteMasterByJobId(string jobId)
        {
            var parameters = new Dictionary<String, Object>();
            parameters.Add("@COL_JOB_ID", jobId);
            var sql = "select * from " + IndexConstants.TableNameArchiveIndexInfo + " where COL_JOB_ID = @COL_JOB_ID";
            var indexList = this.IndexProcessor.ExecuteQuery<ArchiveIndexInfo>(sql, parameters);
            if (indexList.Count == 0)
            {
                logger.Warn($"can not find any master info by job id:{jobId}");
                return new ArchiveIndexInfo();
            }
            return indexList[0];
        }

        public void DeleteSiteMasterByJobId(string jobId)
        {
            var parameters = new Dictionary<String, Object>();
            parameters["@jobId"] = jobId;
            var deleteBodyTable = "delete from " + SecurityUtils.SanitizeSQLSchemaName(IndexConstants.TableNameArchiveIndexInfo) + " where COL_JOB_ID = @jobId";
            this.IndexProcessor.Execute(deleteBodyTable, parameters);
        }
    }
}