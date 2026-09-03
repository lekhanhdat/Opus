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
using System;
using AvePoint.GCommon;
using AvePoint.RA.FileSystem.Core;
using RAFileSystem.Disposal;
using RAFileSystem.FileSystem.BaseProcessor;

namespace RAFileSystem.FileSystem.Disposal
{
    internal sealed class FSDisposalProcessorWorker : FSProcessorWorkerBase
    {
        private readonly AveLogger _logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public FSDisposalProcessorWorker(IFSExecutionStrategy strategy)
            : base(strategy)
        {
        }
        
        protected override void AfterExecute()
        {
            RemoveSqlLiteDb();
        }

        private void RemoveSqlLiteDb()
        {
            try
            {
                FileSystemLiteDBWrapper fileSystemSqliteWrapper = FileSystemLiteDBWrapper.CreateInstance(ReportUtil.GetDisposalDueRecordDBPath(JobContext.Current.JobId));
                fileSystemSqliteWrapper.Dispose();
                fileSystemSqliteWrapper.DeleteDBFile();
            }
            catch(Exception e)
            {
                _logger.Warn("An error occurred while deleting db file. Error:" + e.ToString());
            }
        }

        protected override string GetJobStartMessage()
        {
            return "Start FS disposal job.";
        }

        protected override string GetJobFinishMessage()
        {
            return "Finished FS disposal job.";
        }
    }
}

