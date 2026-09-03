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
using System.Reflection;
using RABox.Extensions;
using Util;

namespace RABox.DataWorker
{
    public class DataConsumer : DataWorkerBase
    {
        private static readonly ILogger logger = LoggerFactory.Get(MethodBase.GetCurrentMethod().DeclaringType);

        public virtual async Task ConsumeContainer(BoxFolderProxy container)
        {
            //var response = await this.Service.ProcessContainer(container);
            //if (response?.DataObject != null && response.Result == OperationResult.Success)
            //{
            //    if (response?.DataObject is DataContainerCompleted)
            //    {
            //        this.containerQueue.Complete();
            //        logger.Info("Consumer container queue complete.");
            //        return;
            //    }

            //    await this.containerQueue.WriteAsync(container);
            //}
        }

        public virtual void Process((DataQueue<BoxFolderProxy>, DataQueue<BoxFileProxy>) produceDataQueue)
        {
            logger.Info("Start consumer process.");
            if (cancellationToken.IsCancellationRequested)
            {
                logger.Info("Stop process consumer.");
                return;
            }
            this.allTasks.Add(Task.Run(async () => await ConsumeContainer(produceDataQueue.Item1), this.cancellationToken));
            this.allTasks.Add(Task.Run(async () => await ConsumeItems(produceDataQueue.Item2), this.cancellationToken));
        }

        public virtual async Task ConsumeContainer(DataQueue<BoxFolderProxy> produceContainerQueue)
        {
            logger.Info("Begin to consume containers.");

            try
            {
                do
                {
                    var dataTask = await produceContainerQueue.ReadAsync();
                    if (dataTask == null)
                    {
                        //await ConsumeContainer(new DataContainerCompleted());
                        break;
                    }

                    if (dataTask is BoxFolderProxy)
                    {
                        await ConsumeContainer(dataTask);
                    }
                }
                while (!this.cancellationToken.IsCancellationRequested);
            }
            catch (Exception e)
            {
                logger.Error($"Failed to consume containers, Error: {e}");
                this.containerQueue.Complete();
                logger.Info("Complete container queue because consume container failed.");
            }
            finally
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        logger.Error($"Job is stoped try to complete consume containers queue.");
                        this.containerQueue.Complete();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to complete consume containers queue, Error: {ex}");
                }
            }

            logger.Info("Finish to consume containers.");
        }

        public virtual async Task ConsumeItems(DataQueue<BoxFileProxy> produceItemQueue)
        {
            logger.Info("Begin to consume items.");

            try
            {
                await produceItemQueue.ToIEnumerable().ParallelExecute(async data =>
                {
                    if (this.cancellationToken.IsCancellationRequested)
                        return;
                    try
                    {

                        //await Service.ProcessItem(dataItem.Item1, dataItem.Item2, batchSize);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"ConsumeItems error {e}");
                    }
                }, maxThreadCount, this.cancellationToken);

            }
            catch (Exception e)
            {
                logger.Error($"Failed to consume items, Error: {e}");
            }
            finally
            {
                this.itemQueue.Complete();
            }
            logger.Info("Consumer item queue complete");
        }

        public override Task<bool> Validate()
        {
            throw new NotImplementedException();
        }
    }
}
