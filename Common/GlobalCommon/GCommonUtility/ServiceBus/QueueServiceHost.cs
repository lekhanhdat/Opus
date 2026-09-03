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
namespace AvePoint.GCommon.Utility.ServiceBus
{
    using AvePoint.GCommon.Contract.AvePointService;
    using AvePoint.GCommon.Utility.AvePointService;
    using AvePoint.GCommon.Utility.Cloud;
    using AvePoint.RA.Common.Global.Utils;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal class QueueServiceHost : IServiceBusHost, IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(QueueServiceHost));

        private AutoResetEvent signal = new AutoResetEvent(false);
        private AutoResetEvent stopSignal = new AutoResetEvent(false);
        private Dictionary<int, Task> workingTasks = new Dictionary<int, Task>();

        private QueueClient client = null;
        private QueueDescription queueDescription = null;

        private Action<AresRemoteEventProperty> callback = null;

        private SbConnectionDto connectionDto = null;

        private int threadMaxCount = -1;
        private int currentFreeThreadCount = -1;
        private bool keepRunning = true;

        public QueueServiceHost(int threadMaxCount, string dataCenter, Action<AresRemoteEventProperty> callBack)
        {
            this.callback += callBack;
            this.threadMaxCount = threadMaxCount;
            this.currentFreeThreadCount = threadMaxCount;
            CreateQueueClient(dataCenter);
        }

        public void StartHost()
        {
            AveThreadUtility.StartThread(ScheduleThreadAndReceiveMessage, "ReceiveMessageThread", "");
            logger.Info("Successfully host the queue receiver service.");
        }

        public void ScheduleThreadAndReceiveMessage()
        {
            try
            {
                while (keepRunning)
                {
                    if (currentFreeThreadCount > 0)
                    {
                        logger.Info("Receiver begin receive batch, Thread id: {0}.", Thread.CurrentThread.ManagedThreadId);
                        var messages = client.ReceiveBatchAsync(currentFreeThreadCount, TimeSpan.FromHours(1)).GetAwaiter().GetResult();
                        logger.Info("Receive batch message successfully, message count: {0}.", messages.Count());
                        foreach (BrokeredMessage message in messages)
                        {
                            try
                            {
                                CancellationTokenSource tokenSource = new CancellationTokenSource();
                                Timer timer = new Timer(CancelWorkingTask, tokenSource, 10 * 60 * 1000, 10 * 60 * 1000);
                                var singleTask = Task.Factory.StartNew(HandleEventMessage, message, tokenSource.Token);
                                Interlocked.Decrement(ref currentFreeThreadCount);
                                lock (workingTasks)
                                {
                                    workingTasks.Add(singleTask.Id, singleTask);
                                }
                                singleTask.ContinueWith(task => ContinuationAction(singleTask.Id, timer), TaskContinuationOptions.ExecuteSynchronously);
                            }
                            catch (Exception ex)
                            {
                                logger.Error("Handle message error. {0}. {1}.", message.MessageId, ex.ToString());
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
                if (!keepRunning && workingTasks.Count == 0)
                {
                    logger.Info("Receiver thread stopped. Thread id: {0}.", Thread.CurrentThread.ManagedThreadId);
                    stopSignal.Set();
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while processing the event message, error: {0}.", ex.ToString());
                Thread.Sleep(1000);
            }
        }

        private void HandleEventMessage(Object obj)
        {
            BrokeredMessage message = obj as BrokeredMessage;
            string receivedMessage = message.GetBody<string>();
            AresRemoteEventProperty eventProperty = JsonConvert.DeserializeObject<AresRemoteEventProperty>(receivedMessage);
            message.Complete();
            callback(eventProperty);
        }

        private void ContinuationAction(int taskId, Timer timer)
        {
            timer.Dispose();
            Interlocked.Increment(ref currentFreeThreadCount);
            lock (workingTasks)
            {
                var singleTask = workingTasks[taskId];
                singleTask.Dispose();
                workingTasks.Remove(taskId);
                if (!keepRunning && workingTasks.Count == 0)
                {
                    stopSignal.Set();
                }
            }
        }

        private void CreateQueueClient(string dataCenter)
        {
            var queuePath = AresUtility.MakeServiceBusPath(dataCenter, Module.RP);

            queueDescription = new QueueDescription(queuePath);
            queueDescription.MaxSizeInMegabytes = 5120; //5G
            queueDescription.DefaultMessageTimeToLive = TimeSpan.FromMinutes(45); //45min
            //EnableDeadLetteringOnMessageExpiration: 获取或设置一个值，用于指示当消息过期时，此队列是否支持死信
            ExecuteSilently(() =>
                {
                    var manager = NamespaceManager.CreateFromConnectionString(GCommonRoleConfiguration.SbConnectionInfo);
                    if (!manager.QueueExists(queueDescription.Path))
                    {
                        queueDescription = manager.CreateQueue(queueDescription);
                    }
                    else
                    {
                        queueDescription = manager.GetQueue(queueDescription.Path);
                    }
                }, 50);

            client = QueueClient.CreateFromConnectionString(GCommonRoleConfiguration.SbConnectionInfo, queueDescription.Path, ReceiveMode.PeekLock);
            logger.Info("Successfully create queue client. queue path: {0}, queue connection info: {1}", queueDescription.Path, GCommonRoleConfiguration.SbConnectionInfo);
        }

        private void ExecuteSilently(Action action, int retryTimes)
        {
            for (int i = 0; i < retryTimes; i++)
            {
                try
                {
                    action();
                    return;
                }
                catch
                {
                    if (i == retryTimes - 1)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(5 * 1000);
                    }
                }
            }
        }

        public void SafeStopHost()
        {
            keepRunning = false;
            signal.Set();
            stopSignal.WaitOne();
        }

        public void CancelWorkingTask(Object obj)
        {
            CancellationTokenSource tokenSource = obj as CancellationTokenSource;
            tokenSource.Cancel();
        }

        public void Dispose()
        {
            if(signal != null)
            {
                signal.Dispose();
            }
            if(stopSignal != null)
            {
                ArgumentCheck.NotNull(signal, nameof(signal));
                signal.Dispose();
            }
        }
    }
}
