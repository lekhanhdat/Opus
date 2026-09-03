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
    //public class ChunkedFileStreamManager : IBlockedStreamManager
    //{
    //    protected static AveLogger sLog = AveLogger.GetInstance(typeof(ChunkedFileStreamManager));
    //    protected readonly BlockingQueue<AveCoordinatedStream> mCachedStreams;
    //    private const int defaultThreshold = 100;

    //    public ChunkedFileStreamManager()
    //    {
    //        mCachedStreams = new BlockingQueue<AveCoordinatedStream>(defaultThreshold);
    //    }

    //    public ChunkedFileStreamManager(int threshold)
    //    {
    //        mCachedStreams = new BlockingQueue<AveCoordinatedStream>(threshold);
    //    }

    //    public virtual Stream GetEmptyStream()
    //    {
    //        return GetEmptyStream(0);
    //    }

    //    public virtual Stream GetEmptyStream(long order)
    //    {
    //        return new AveCoordinatedStream("CFM", order);
    //    }

    //    public virtual void ReturnEmptyStream(Stream stream)
    //    {
    //        AveCoordinatedStream fs = stream as AveCoordinatedStream;
    //        try
    //        {
    //            stream.Flush();
    //            mCachedStreams.Enqueue(fs);
    //        }
    //        catch (Exception e)
    //        {
    //            sLog.Error("error occurred when flush stream: exception message: {0}, stack trace: {1}", e.Message, e.StackTrace);
    //            throw;
    //        }
    //    }

    //    public virtual Stream GetFilledStream()
    //    {            
    //        AveCoordinatedStream fs = mCachedStreams.Dequeue();
    //        if (fs != null)
    //        {
    //            fs.Position = 0;
    //        }
    //        return fs;                       
    //    }

    //    public void ReturnFilledStream(Stream stream)
    //    {
    //        AveCoordinatedStream fs = stream as AveCoordinatedStream;
    //        try
    //        {
    //            fs.Dispose();
    //        }
    //        catch (Exception e)
    //        {
    //            sLog.Error("error occurred when delete file: exception message: {0}, stack trace: {1}", e.Message, e.StackTrace);
    //            throw;
    //        }
    //    }

    //    public bool IsEmpty
    //    {
    //        get
    //        {
    //            return mCachedStreams.IsEmpty;
    //        }
    //    }

    //    public int Count
    //    {
    //        get
    //        {
    //            return mCachedStreams.Count;
    //        }
    //    }

    //    public bool ClearCache()
    //    {
    //        return mCachedStreams.Clear();
    //    }

    //    public void Close()
    //    {
    //        mCachedStreams.Close();
    //    }
    //}

    //public class ChunkedFileStreamManagerWithOrder : ChunkedFileStreamManager
    //{
    //    //private long currentReadOrder;
    //    private long currentOrder;
    //    private readonly Dictionary<long, ManualResetEvent> resetEvents; 

    //    public ChunkedFileStreamManagerWithOrder()
    //        : base()
    //    {
    //        //this.currentReadOrder = -1;
    //        this.currentOrder = 0;
    //        this.resetEvents = new Dictionary<long, ManualResetEvent>();
    //    }

    //    public ChunkedFileStreamManagerWithOrder(int threshold)
    //        : base(threshold)
    //    {
    //        //this.currentReadOrder = -1;
    //        this.currentOrder = 0;
    //        this.resetEvents = new Dictionary<long, ManualResetEvent>();
    //    }

    //    public override Stream GetEmptyStream()
    //    {
    //        throw new NotImplementedException();
    //        //return GetEmptyStream(Interlocked.Add(ref currentReadOrder, 1));
    //    }

    //    private ManualResetEvent GetResetEventWithOrder(long eventOrder)
    //    {
    //        ManualResetEvent resetEvent;
    //        lock (resetEvents)
    //        {
    //            if (!resetEvents.TryGetValue(eventOrder, out resetEvent))
    //            {
    //                resetEvent = new ManualResetEvent(false);
    //                resetEvents[eventOrder] = resetEvent;
    //            }
    //        }

    //        return resetEvent;
    //    }

    //    private void ReleaseResetEventWithOrder(long eventOrder)
    //    {
    //        lock (resetEvents)
    //        {
    //            ManualResetEvent resetEvent;
    //            if (resetEvents.TryGetValue(eventOrder, out resetEvent))
    //            {
    //                resetEvent.Dispose();
    //                resetEvents.Remove(eventOrder);
    //            }
    //        }
    //    }

    //    public override void ReturnEmptyStream(Stream stream)
    //    {
    //        AveCoordinatedStream fs = stream as AveCoordinatedStream;
    //        try
    //        {
    //            stream.Flush();

    //            if (fs.Order != Interlocked.Read(ref currentOrder))
    //            {
    //                var resetEvent = GetResetEventWithOrder(fs.Order);
    //                resetEvent.WaitOne();
    //            }

    //            mCachedStreams.Enqueue(fs);

    //            ReleaseResetEventWithOrder(fs.Order);

    //            Interlocked.Add(ref currentOrder, 1L);

    //            var nextEvent = GetResetEventWithOrder(currentOrder);
    //            nextEvent.Set();
    //        }
    //        catch (Exception e)
    //        {
    //            sLog.Error("error occurred when flush stream: exception message: {0}, stack trace: {1}", e.Message, e.StackTrace);
    //            throw;
    //        }
    //    }
    //}
}
