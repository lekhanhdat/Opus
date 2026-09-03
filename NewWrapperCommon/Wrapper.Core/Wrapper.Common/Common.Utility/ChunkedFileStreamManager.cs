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
using System.Text;
using System.IO;
using System.Threading;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common.Common.Utility;

namespace AvePoint.Wrapper.Common
{
    public class ChunkedFileStreamManager : IBlockedStreamManager, IDisposable
    {
        private static AveLogger sLog = AveLogger.GetInstance(typeof(ChunkedFileStreamManager));
        private readonly BlockingQueue<AveCoordinatedStream> mCachedStreams;
        private const int defaultThreshold = 100;

        public ChunkedFileStreamManager()
        {
            mCachedStreams = new BlockingQueue<AveCoordinatedStream>(defaultThreshold);
        }

        public ChunkedFileStreamManager(int threshold)
        {
            mCachedStreams = new BlockingQueue<AveCoordinatedStream>(threshold);
        }

        public Stream GetEmptyStream()
        {
            return new AveCoordinatedStream();
        }

        public void ReturnEmptyStream(Stream stream)
        {
            AveCoordinatedStream fs = stream as AveCoordinatedStream;
            try
            {
                stream.Flush();
                mCachedStreams.Enqueue(fs);
            }
            catch (Exception e)
            {
                sLog.Error("error occurred when flush stream: exception message: {0}, stack trace: {1}", e.Message, e.StackTrace);
                throw;
            }
        }

        public Stream GetFilledStream()
        {
            AveCoordinatedStream fs = mCachedStreams.Dequeue();
            if (fs != null)
            {
                fs.Position = 0;
            }
            return fs;
        }

        public void ReturnFilledStream(Stream stream)
        {
            AveCoordinatedStream fs = stream as AveCoordinatedStream;
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
