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
using PluralizationService.Core;
using System.Collections.Concurrent;
using System.Reflection;
using Util;

namespace RABox.DataWorker
{
    public abstract class DataWorkerBase : DisposableObject
    {
        private readonly ILogger logger = LoggerFactory.Get(MethodBase.GetCurrentMethod().DeclaringType);

        protected CancellationToken cancellationToken;

        protected DataQueue<BoxFolderProxy> containerQueue;
        protected DataQueue<BoxFileProxy> itemQueue;
        protected ConcurrentBag<Task> allTasks;
        protected Int32 maxThreadCount;


        public DataWorkerBase Config(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            return this;
        }

        public DataWorkerBase Config(ReportCenter reportCenter)
        {
            return this;
        }

        public virtual (DataQueue<BoxFolderProxy>, DataQueue<BoxFileProxy>) CreateDataQueue(Int32 containerCapacity, Int32 itemCapacity)
        {
            this.containerQueue = new DataQueue<BoxFolderProxy>(containerCapacity, this.cancellationToken);
            this.itemQueue = new DataQueue<BoxFileProxy>(itemCapacity, this.cancellationToken);

            return (this.containerQueue, this.itemQueue);
        }

        public abstract Task<Boolean> Validate();
        public virtual void Completed()
        {
            try
            {
                if (!this.allTasks.Any())
                {
                    logger.Error($"No task need be complete.");
                    return;
                }
                Task.WaitAll(this.allTasks.ToArray(), this.cancellationToken);
            }
            catch (Exception e)
            {
                logger.Error($"Failed to completed the worker, Error: {e}");
            }
        }

    }
}
