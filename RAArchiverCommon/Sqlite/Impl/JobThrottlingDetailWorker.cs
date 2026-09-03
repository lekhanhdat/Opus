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
using AvePoint.RA.Contract.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAArchiverCommon.Sqlite.Impl
{
    public class JobThrottlingDetailWorker : BaseThrottlingDetailWorker
    {
        private const string DB_NAME = "JobThrottlingStatistic.rpt";



        public JobThrottlingDetailWorker()
        {
            _dbName = DB_NAME;
            _dbdirPath = GetDbFolderPath();
            _dbFilePath = GetDbPath();
            blobUri = GetDbBlobUri();
        }

        private string GetDbBlobUri()
        {
            return string.Join("/", TenantLocalValue.LogonGroupId, CACHE_FOLDER_NAME, currentTime.Year.ToString(), currentTime.Month.ToString(), _dbName);
        }

        private string GetDbFolderPath()
        {
            return Path.Combine(CACHE_FODER_PATH, CACHE_FOLDER_NAME, currentTime.Year.ToString(), currentTime.Month.ToString());
        }

        private string GetDbPath()
        {
            return Path.Combine(GetDbFolderPath(), _dbName);
        }


        public override void UploadDatabase()
        {
            UploadDatabase(blobUri, _dbFilePath);
        }

        public override string GetLastestDataBase()
        {
            return GetLastestDataBase(blobUri, _dbFilePath);
        }
    }
}
