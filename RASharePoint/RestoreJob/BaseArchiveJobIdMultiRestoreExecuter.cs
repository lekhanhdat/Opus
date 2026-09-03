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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RestoreJob
{
    public class BaseArchiveJobIdMultiRestoreExecuter
    {
        private static readonly AveLogger s_logger = AveLogger.GetInstance(typeof(BaseArchiveJobIdMultiRestoreExecuter));

        private IRestoreSearchService _restoreSearchService;
        private IRestoreSearchService RestoreSearchService => PlatformWindsorManager.GetService(ref _restoreSearchService);

        private IRMSubJobDao _rmSubJobDao;

        private IRMSubJobDao RMSubJobDao => PlatformWindsorManager.GetService(ref _rmSubJobDao);


        private BackendBatchRestoreInfo _BackendBatchRestoreInfo = null;

        public BaseArchiveJobIdMultiRestoreExecuter(string jobId)
        {
            s_logger.Info($"Initializing MultiSiteCollectionRestoreExecuter for job {jobId}");
            string paramJson = RMSubJobDao.GetJobContextSettingByJobId(jobId);
            RMSubJobDao.DeleteJobContext(jobId);
            s_logger.Info($"Constructing MultiSiteCollectionRestoreExecuter for job {jobId}, paramJson:{paramJson}");
            _BackendBatchRestoreInfo = SerializerHelper.DeserializeByJsonConvert<BackendBatchRestoreInfo>(paramJson);
        }

        public void Execute() 
        {
            RestoreSearchService.SaveBaseArchiveJobIdMultiRestoreSettingAndRunAsync(_BackendBatchRestoreInfo).GetAwaiter().GetResult();

        }
    }
}
