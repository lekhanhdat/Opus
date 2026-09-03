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
using System.Reflection;
using System.Threading;
using AvePoint.I18N;

namespace AvePoint.GCommon.FileTransfer
{
    internal class ResponseReader
    {
        private AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private BlockSender dataBlockSender;
        private IFileSenderResponseWorker readerWorker;
        private Thread readerThread;

        public ResponseReader(BlockSender blockSender, IFileSenderResponseWorker responseWorker)
        {
            dataBlockSender = blockSender;
            readerWorker = responseWorker;
        }

        public void WaitingForReaderCompleted(int timeout)
        {
            readerThread.Join(timeout);
        }

        public void Start(string jobId = "")
        {
            string currentThreadId = Thread.CurrentThread.Name;
            if (string.IsNullOrEmpty(currentThreadId))
            {
                currentThreadId = Thread.CurrentThread.ManagedThreadId.ToString();
            }
            readerThread = new Thread(new ParameterizedThreadStart(Run));
            readerThread.Name = currentThreadId + "_Reader";
            readerThread.IsBackground = true;
            readerThread.Start(string.IsNullOrEmpty(jobId) ? null : jobId);
        }

        private void Run(object o = null)
        {
            try
            {
                if (o != null)
                {
                    AveLogger.SetThreadJobId(o as string);
                }
                while (true)
                {
                    if (dataBlockSender.Available > 8191)
                    {
                        dataBlockSender.Pause();
                    }
                    else
                    {
                        dataBlockSender.Resume();
                    }
                    string message = dataBlockSender.ReceiveMessage();
                    if (message.Equals("<KeepAlive />", StringComparison.OrdinalIgnoreCase)) continue;
                    try
                    {
                        readerWorker.SaveXmlHeader(message);
                    }
                    catch (Exception e)
                    {
                        //logger.Error("An error occurred while processing xml header. Header:{0} Exception details:{1}", message, e.ToString());
                        logger.Error(CommonResources.ResponseReaderRunErrorOccurredWhenSaveXml, message, e.ToString());
                    }
                    if (message.Equals("End", StringComparison.OrdinalIgnoreCase))
                    {
                        //logger.Debug("Response reader exit.");
                        dataBlockSender.SendMessage("Confirm end");
                        logger.Info(CommonResources.ResponseReaderRunExit);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                //logger.Error("An error occurred in response reader thread. Exception details:{0}", e.ToString());
                logger.Error(CommonResources.ResponseReaderRunErrorOccurredInRunThread, e.ToString());
            }
        }
    }
}