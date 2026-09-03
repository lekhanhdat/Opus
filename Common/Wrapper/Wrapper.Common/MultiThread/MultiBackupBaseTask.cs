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
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.Wrapper.Common.MultiThread
{
    public abstract class MultiBackupBaseTask<TNode, TWriter> : BaseTask where TWriter : IDisposable
    {
        protected readonly TNode nodeParameters;
        protected TWriter writer;
        protected readonly AutoResetEvent backupCompleted;
        protected Exception exception;

        public TNode NodeParameters
        {
            get { return nodeParameters; }
        }

        protected MultiBackupBaseTask(TNode nodeParameters)
        {
            this.nodeParameters = nodeParameters;
            backupCompleted = new AutoResetEvent(false);
        }

        protected abstract System.Threading.Tasks.Task BackupAsync();

        public override void Process()
        {
            try
            {
                BackupAsync().Wait();
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                backupCompleted.Set();
            }
        }

        protected override void Close()
        {
            writer.Dispose();
            backupCompleted.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TWriter GetWriter()
        {
            backupCompleted.WaitOne();

            if (exception != null)
            {
                throw exception;
            }

            return writer;
        }
    }
}