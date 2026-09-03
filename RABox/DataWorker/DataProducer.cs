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
    public class DataProducer : DataWorkerBase
    {
        private static readonly ILogger logger = LoggerFactory.Get(MethodBase.GetCurrentMethod().DeclaringType);

        public RMBoxService Service { get; protected set; }

        private Int32 batchSize;

        public void Build()
        {
            // Create Service based on the job info
            //var serviceFactory = typedFactory.Get<DataProviderFactory>($"Box");

            Service = new RMBoxService(null);

        }

        public bool ShouldRetry()
        {
            return false;
        }


        public virtual void Process((DataQueue<BoxFolderProxy>, DataQueue<BoxFileProxy>) consumerQueue)
        {
            logger.Info("Start producer process.");
            if (cancellationToken.IsCancellationRequested)
            {
                logger.Info("Stop process producer.");
                return;
            }

            this.allTasks.Add(Task.Run(() => ProduceItems(consumerQueue.Item1), cancellationToken));
            this.allTasks.Add(Task.Run(() => WaitConsumeItems(consumerQueue.Item2), cancellationToken));
            this.allTasks.Add(Task.Run(async () => await ProduceContainer(), cancellationToken));
        }

        public virtual async Task ProduceContainer()
        {
            try
            {
                BoxFolderProxy container = Service.GetContainer(null);
                if (container != null)
                {
                    await containerQueue.WriteAsync(container);

                    await ProduceContainers(container, null);
                }
                else
                {
                    logger.Error("Get top container failed.");
                }
            }
            catch (Exception e)
            {
                logger.Error($"Failed to produce container, Error: {e}");
            }
            finally
            {
                containerQueue.Complete();
            }
            logger.Info("Produce container queue complete");
        }

        public virtual async Task ProduceContainers(BoxFolderProxy parent, Dictionary<String, Object> queryParams)
        {
            var containers = Service.GetContainers(parent, queryParams);

            await foreach (var container in containers)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                await containerQueue.WriteAsync(container);
                await ProduceContainers(container, queryParams);
            }
        }

        public virtual async Task ProduceItems(DataQueue<BoxFolderProxy> consumerContainerQueue)
        {
            Thread.CurrentThread.Name = "ProduceItems";

            try
            {
                await consumerContainerQueue.ToIEnumerable().ParallelExecute(async data =>
                {
                    if (this.cancellationToken.IsCancellationRequested)
                        return;
                    try
                    {
                        var items = Service.GetItems(data);
                        var innerQueue = items.ToEnumerable().PrecacheData(batchSize);

                        foreach (var item in innerQueue)
                        {
                            await itemQueue.WriteAsync(item);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error($"ProduceItems error {e}");
                    }
                }, maxThreadCount, this.cancellationToken);
            }
            catch (Exception e)
            {
                logger.Error($"Failed to produce items, Error: {e}");
            }
            finally
            {
                this.itemQueue.Complete();
            }
            logger.Info("Produce item queue complete");
        }

        private void WaitConsumeItems(DataQueue<BoxFileProxy> consumerItemQueue)
        {
            do
            {
                var containerTask = consumerItemQueue.ReadAsync();
                if (containerTask.Result == null)
                    break;

            }
            while (!cancellationToken.IsCancellationRequested);

            logger.Info("Consume items finished.");
        }

        public override Task<bool> Validate()
        {

            return Task.FromResult(true);
        }
    }
}
