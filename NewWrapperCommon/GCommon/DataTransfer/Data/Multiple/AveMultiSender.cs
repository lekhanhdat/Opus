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
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Transfer.Data.Multiple.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AvePoint.GCommon.Transfer.Data.Multiple
{
    public class AveMultiSender:IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveMultiSender));

        private IDataSender dataSender;
        private String tempFolder;
        private AveTaskHierarchyScheduler scheduler;
        private Byte[] innerBuffer;
        private AveMultiTaskTree taskTree;
        private AveMultiTaskThreadPool pool;
        private Thread senderThread;
        private AutoResetEvent sendFinish;
        private StringBuilder errorString;
        private Queue taskSyncQueue;

        public AveMultiSender(IDataSender sender, string temparyFolder, int threadCount)
        {
            this.sendFinish = new AutoResetEvent(false);
            this.taskTree = new AveMultiTaskTree(threadCount * 2);
            this.pool = new AveMultiTaskThreadPool(threadCount);
            this.scheduler = new AveTaskHierarchyScheduler(taskTree, pool);
            this.taskSyncQueue = Queue.Synchronized(new Queue(threadCount * 2));
            this.dataSender = sender;
            this.tempFolder = temparyFolder;
            this.innerBuffer = new Byte[65536];
            this.errorString = new StringBuilder();
        }

        public void Start()
        {
            this.senderThread = new Thread(SendThread);
            this.senderThread.IsBackground = true;
            this.senderThread.Name = Thread.CurrentThread.Name + "_MultiSender";
            this.senderThread.Start();
        }

        public void AddTask(AveMultiSendTask task)
        {
            CheckError();

            InitDataSender(task);

            this.taskSyncQueue.Enqueue(task);
            this.scheduler.AddTask(task);
        }

        public void Finish()
        {
            this.scheduler.Finish();
        }

        public void Wait()
        {
            this.scheduler.Wait();

            this.sendFinish.WaitOne();
        }

        private AveMultiTask EnsureNextWithMinStatus(TaskStatus status)
        {
            AveMultiTask current;
            while (true)
            {
                if (this.taskSyncQueue.Count > 0)
                {
                    current = this.taskSyncQueue.Dequeue() as AveMultiTask;
                    break;
                }
                else if (this.taskTree.Finished)
                {
                    return null;
                }
                else
                {
                    Thread.Sleep(20);
                }
            }

            while (current.Status < status)
            {
                Thread.Sleep(50);
            }

            return current;
        }

        private void CheckError()
        {
            if (errorString.Length > 0)
            {
                throw new Exception("Exception occurred in multiple sender logic: " + errorString.ToString());
            }
        }

        private void SendThread()
        {
            try
            {
                AveMultiTask task = null;
                while ((task = this.EnsureNextWithMinStatus(TaskStatus.ProcessEnd)) != null)
                {
                    try
                    {
                        using (var sendStream = (task as AveMultiSendTask).DataSender as SegmentedDataTransfer)
                        {
                            while (true)
                            {
                                var head = sendStream.GetNextFileHeadComplete();

                                if (string.IsNullOrEmpty(head)) break;

                                this.dataSender.WriteHead(head);

                                int readLen = 0;
                                while ((readLen = sendStream.ReadBytes(innerBuffer, 0, innerBuffer.Length)) > 0)
                                {
                                    this.dataSender.WriteData(innerBuffer, 0, readLen);
                                }

                                var tail = sendStream.GetFileTail();
                                this.dataSender.WriteTail(tail);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(task.GetType().Name, ex);
                    }
                }

                logger.Info("Send data thread finished normally.");
            }
            catch (Exception ex)
            {
                logger.Error("Error in Send thread. {0}", ex.ToString());
                errorString.AppendLine(ex.ToString());
            }
            finally
            {
                this.sendFinish.Set();
            }
        }

        public void InitDataSender(AveMultiSendTask task)
        {
            CheckError();

            if (task.DataSender == null)
            {
                lock (task)
                {
                    if (task.DataSender == null)
                    {
                        task.DataSender = new SegmentedDataTransfer(this.tempFolder);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (this.sendFinish != null)
            {
                this.sendFinish.Close();
            }
        }
    }
}
