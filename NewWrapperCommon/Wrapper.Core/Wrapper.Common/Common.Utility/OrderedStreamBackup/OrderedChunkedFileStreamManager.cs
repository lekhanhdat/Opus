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
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common.Common.Utility;

namespace AvePoint.Wrapper.Common
{
    public class OrderedChunkedFileStreamManager : IOrderedChunkedStreamManager,IDisposable
    {
        protected static AveLogger sLog = AveLogger.GetInstance(typeof(OrderedChunkedFileStreamManager));
        private readonly BlockingDictionary<AveOrderedCoordinatedStream> mCachedStreams;
        protected const int defaultThreshold = 100;
        private Semaphore mBackupingSemaphore;
        public OrderedChunkedFileStreamManager()
        {
            mCachedStreams = new BlockingDictionary<AveOrderedCoordinatedStream>(defaultThreshold);
            mBackupingSemaphore = new Semaphore(defaultThreshold, defaultThreshold);
        }

        public OrderedChunkedFileStreamManager(int threshold)
        {
            mCachedStreams = new BlockingDictionary<AveOrderedCoordinatedStream>(threshold);
            mBackupingSemaphore = new Semaphore(defaultThreshold, defaultThreshold);
        }

        public Stream GetEmptyStream(int streamOrder)
        {
            AveOrderedCoordinatedStream emptyStream = new AveOrderedCoordinatedStream();
            emptyStream.ItemStreamOrder = streamOrder;
            mBackupingSemaphore.WaitOne();
            return emptyStream;
        }

        public void ReturnEmptyStream(Stream stream)
        {
            AveOrderedCoordinatedStream fs = stream as AveOrderedCoordinatedStream;
            try
            {
                stream.Flush();
                mCachedStreams.Add(fs.ItemStreamOrder, fs);
            }
            catch (Exception e)
            {
                sLog.Error("error occurred when flush stream: exception message: {0}, stack trace: {1}", e.Message, e.StackTrace);
                throw;
            }
        }

        public Stream GetFilledStream(int streamOrder)
        {
            AveOrderedCoordinatedStream fs = mCachedStreams[streamOrder];
            if (fs != null)
            {
                mCachedStreams.Remove(streamOrder);
                mBackupingSemaphore.Release();
                fs.Position = 0;
            }
            return fs;
        }

        public void ReturnFilledStream(Stream stream)
        {
            AveOrderedCoordinatedStream fs = stream as AveOrderedCoordinatedStream;
            try
            {
                fs.Dispose();
            }
            catch (Exception e)
            {
                sLog.Error("error occurred when delete file: exception message: {0}, stack trace: {1}", e.Message, e.StackTrace);
                throw;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return mCachedStreams.IsEmpty;
            }
        }

        public int Count
        {
            get
            {
                return mCachedStreams.Count;
            }
        }

        public void Close()
        {
            mCachedStreams.Close();
        }
        public void Dispose()
        {
            mCachedStreams.Dispose();
        }
    }
}
