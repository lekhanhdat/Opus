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
using System.Collections.Generic;
using System.Threading;
using AvePoint.GCommon;
using AvePoint.Media.Service.ArchiverBackup.Backup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common.MultiThread;
using AvePoint.RA.Common;

namespace AvePoint.RA.SharePoint.Archiver
{
    internal class TransferBackupDataTask : TransferDataTask<MultiBackupTask>
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(TransferBackupDataTask));
        private readonly IArchiverBackupDataWriter archiverBackupDataWriter;

        public TransferBackupDataTask(IArchiverBackupDataWriter archiverBackupDataWriter, int maxQueueNumber)
            : base(maxQueueNumber)
        {
            this.archiverBackupDataWriter = archiverBackupDataWriter;
        }

        protected override void ProcessTask(MultiBackupTask task)
        {
            ArchiverBackupStreamWriter writer = null;
            using (LogPerformance lp = new($"TransferBackupDataTask.ProcessTask.GetWriter"))
            {
                writer = task.GetWriter();
            }
            try
            {
                using (LogPerformance lp = new($"TransferBackupDataTask.ProcessTask.WriteToAnotherWriter"))
                {
                    writer.WriteToAnotherWriter(archiverBackupDataWriter);
                }
            }
            catch (Exception e)
            {
                logger.Error("Process Task failed error :{0}", e);
                throw new BackupDataStoreException(e.Message);
            }
        }

        public override void Process()
        {
            while (true)
            {
                var task = Peek();
                if (task != null)
                {
                    try
                    {
                        currentTask = task;
                        ProcessTask(task);
                    }
                    catch (BackupDataStoreException e)
                    {
                        if (exception == null)
                        {
                            exception = e;
                        }
                        throw;
                    }
                    finally
                    {
                        task.Dispose();
                        RemoveFirstOne();
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private MultiBackupTask Peek()
        {
            lock (tasks)
            {
                if (tasks.Count > 0)
                {
                    return tasks.Peek();
                }
            }

            return default(MultiBackupTask);
        }

        private void RemoveFirstOne()
        {
            lock (tasks)
            {
                tasks.Dequeue();
            }
            transferedEvent.Set();
        }

        public override void CompleteTask()
        {

        }

        public override void CompleteTask(Exception ex)
        {
            logger.Error("transfer data for {0} failed:{1}", currentTask.NodeParameters.Node.FullPath, ex);

            if (exception == null && ex is BackupDataStoreException)
            {
                exception = ex;
            }
        }

        public void RaiseException()
        {
            if (exception != null)
            {
                throw exception;
            }
        }

        protected override void Close()
        {
            base.Close();
            if (exception != null)
            {
                throw exception;
            }
        }
    }
}