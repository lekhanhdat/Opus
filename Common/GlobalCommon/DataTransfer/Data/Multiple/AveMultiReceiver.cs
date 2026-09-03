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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AvePoint.GCommon.Transfer.Data.Multiple
{
    public class AveMultiReceiver:IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveMultiReceiver));

        public delegate AveMultiReceiveTask FileHeadReceivedEvent(string fileHead);

        private IDataReceiver dataReceiver;
        private string tempFolder;
        private AveMultiTaskTree taskTree;
        private Thread receiverThread;
        public AveTaskHierarchyScheduler scheduler;
        private AveMultiTaskThreadPool pool;
        private Byte[] innerBuffer;
        private bool closeReceiver;
        private int headerCounter;

        public AveMultiReceiver(IDataReceiver receiver, string temparyFolder, int threadCount, bool closeReceiver = true)
        {
            this.taskTree = new AveMultiTaskTree(threadCount * 2);
            this.pool = new AveMultiTaskThreadPool(threadCount);

            this.dataReceiver = receiver;
            this.tempFolder = temparyFolder;
            this.scheduler = new AveTaskHierarchyScheduler(taskTree, pool);
            this.innerBuffer = new Byte[65536];
            this.closeReceiver = closeReceiver;
        }
       
        public AveMultiReceiver(int threadCount, bool closeReceiver = true)
        {
            this.taskTree = new AveMultiTaskTree(threadCount * 2);
            this.pool = new AveMultiTaskThreadPool(threadCount);
            this.scheduler = new AveTaskHierarchyScheduler(taskTree, pool);
            this.innerBuffer = new Byte[65536];
            this.closeReceiver = closeReceiver;
        }

        public FileHeadReceivedEvent OnFileHeadReceived { get; set; }

        public Action<int> OnTaskPoolFull { get; set; }

        public Action OnDataReceiveEnd { get; set; }

        public Action<string> OnDataReceiveException { get; set; }

        public void Start()
        {
            if (OnFileHeadReceived == null)
            {
                throw new Exception("The callback OnFileHeadReceived should be set.");
            }
            StartReceiverThread();
        }

        public void Wait()
        {
            this.scheduler.Wait();
        }

        private void StartReceiverThread()
        {
            this.receiverThread = new Thread(ReceiverThread);
            this.receiverThread.IsBackground = true;
            this.receiverThread.Name = Thread.CurrentThread.Name + "_MultiReceiver";
            this.receiverThread.Start();
        }

        public SegmentedDataTransfer GetSegmentReceiver(string head)
        {
            var segmentReceiver = new SegmentedDataTransfer(tempFolder);
            WriteFile(segmentReceiver, head);
            return segmentReceiver;
        }

        public void WriteFile(SegmentedDataTransfer transfer, string head)
        {
            transfer.WriteHead(head);

            int readLen = 0;
            while ((readLen = this.dataReceiver.ReadBytes(innerBuffer, 0, innerBuffer.Length)) > 0)
            {
                transfer.WriteData(innerBuffer, 0, readLen);
            }

            var tail = this.dataReceiver.GetFileTail();
            transfer.WriteTail(tail);
        }

        private void ReceiverThread()
        {
            try
            {
                while (true)
                {
                    var head = this.dataReceiver.GetNextFileHead();
                    if (string.IsNullOrEmpty(head))
                    {
                        if (OnDataReceiveEnd != null)
                            OnDataReceiveEnd();

                        this.scheduler.Finish();
                        break;
                    }

                    var task = OnFileHeadReceived(head);

                    task.DataReceiver = GetSegmentReceiver(head);

                    this.scheduler.AddTask(task, OnTaskPoolFull);

                    this.headerCounter++;
                }

                logger.Info("Receiver thread finished. Header count:{0}", this.headerCounter);
            }
            catch (Exception ex)
            {
                logger.Error("Critical error in multiple receiver thread. {0}", ex.ToString());

                this.scheduler.Stop(ex.Message);

                if (OnDataReceiveException != null)
                    OnDataReceiveException(ex.ToString());
            }
            finally
            {
                if (this.closeReceiver)
                {
                    this.dataReceiver.Close();
                }
            }
        }

        public void Dispose()
        {
            if (scheduler != null)
            {
                scheduler.Dispose();
            }
            if (pool != null)
            {
                pool.Dispose();
            }
        }
    }
}
