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
using System.Threading.Tasks;

namespace DataExportCore;

public interface IExportQueue
{
    bool IsFinish { get; }
    DiscoverNode MoveNext();
    Exception Error { set; get; }
    void Enqueue(DiscoverNode entity);
    void Finish();
}

public class ExportQueue : IExportQueue
{
    private readonly Queue<DiscoverNode> mExportQueue;
    private const int MaxDiscoverListCount = 100;
 
    private bool mFinished;
     
    public Exception Error { get; set; }
    
    public int Count { get { return mExportQueue.Count; } }

    public bool IsFinish => mFinished || Error != null;

    public ExportQueue()
    {
        mExportQueue = new Queue<DiscoverNode>();
    }
 
    public void Enqueue(DiscoverNode entity)
    {
        lock (mExportQueue)
        {
            //CheckJobStop

            while (mExportQueue.Count > MaxDiscoverListCount)
            {
                Monitor.Wait(mExportQueue);
            }
            mExportQueue.Enqueue(entity);
            Monitor.Pulse(mExportQueue);
        }
    }

    public DiscoverNode MoveNext()
    {
        DiscoverNode current;
        lock (mExportQueue)
        {
            while (mExportQueue.Count == 0)
            {
                if (Error != null)
                {
                    throw Error;
                }
                if (mFinished)
                {
                    return null;
                }
                Monitor.Wait(mExportQueue);
            }
            current = mExportQueue.Dequeue();
            Monitor.Pulse(mExportQueue);
        }
        return current;
    }
    /// <summary>
    /// Call this method if all datablock are enQueued
    /// </summary>
    public void Finish()
    {
        lock (mExportQueue)
        {
            mFinished = true;
            Monitor.Pulse(mExportQueue);
        }
    }
}

public interface IExportQueue<T>
{
    bool IsFinish { get; }
    T MoveNext();
    Exception Error { set; get; }
    void Enqueue(T entity);
    void Finish();
}

public class ExportQueue<T> : IExportQueue<T>
{
    private readonly Queue<T> mExportQueue;
    private const int MaxDiscoverListCount = 100;

    private bool mFinished;

    public Exception Error { get; set; }

    public int Count { get { return mExportQueue.Count; } }

    public bool IsFinish => mFinished || Error != null;

    public ExportQueue()
    {
        mExportQueue = new Queue<T>();
    }

    public void Enqueue(T entity)
    {
        lock (mExportQueue)
        {
            //CheckJobStop

            while (mExportQueue.Count > MaxDiscoverListCount)
            {
                Monitor.Wait(mExportQueue);
            }
            mExportQueue.Enqueue(entity);
            Monitor.Pulse(mExportQueue);
        }
    }

    public T MoveNext()
    {
        T current;
        lock (mExportQueue)
        {
            while (mExportQueue.Count == 0)
            {
                if (Error != null)
                {
                    throw Error;
                }
                if (mFinished)
                {
                    return default(T);
                }
                Monitor.Wait(mExportQueue);
            }
            current = mExportQueue.Dequeue();
            Monitor.Pulse(mExportQueue);
        }
        return current;
    }
    /// <summary>
    /// Call this method if all datablock are enQueued
    /// </summary>
    public void Finish()
    {
        lock (mExportQueue)
        {
            mFinished = true;
            Monitor.Pulse(mExportQueue);
        }
    }
}