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

namespace AvePoint.Metadata;

using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

public delegate (Stream Stream, long Size) StreamProviderDelegate();
public class AveReusableStreamProvider : IDisposable
{
    public static Stopwatch Timer = new Stopwatch();
    public static long Times = 0;
    private const int MemoryStreamLimit = 50 * 1024 * 1024;
    protected CoordinatedStream InnerStream { get; set; }
    protected StreamProviderDelegate StreamProvider { get; set; }
    protected bool IsNewStream;
    public AveReusableStreamProvider(StreamProviderDelegate? streamProvider)
    {
        ArgumentNullException.ThrowIfNull(streamProvider);
        StreamProvider = streamProvider;
        InnerStream = null;
        IsNewStream = true;
    }

    public AveReusableStreamProvider(Stream? stream, bool closeOriginalStreamAfterClone = false)
    {
        ArgumentNullException.ThrowIfNull(stream);

        Timer.Start();
        if (IsReusableStream(stream))
        {
            InnerStream = stream as CoordinatedStream;
            IsNewStream = false;
        }
        else
        {
            InnerStream = new CoordinatedStream("ReusableStream", 0, true, MemoryStreamLimit);
            stream.CopyTo(InnerStream);
            if (closeOriginalStreamAfterClone)
            {
                stream.Close();
            }
            IsNewStream = true;
        }
        Timer.Stop();
        Times++;
    }

    public (Stream Content, Int64 Size) GetStream()
    {
        if (InnerStream == null)
        {
            return StreamProvider.Invoke();
        }
        InnerStream.Position = 0;
        return (InnerStream, InnerStream.Length);
    }

    private static bool IsReusableStream(Stream stream)
    {
        return stream is CoordinatedStream && (stream as CoordinatedStream).IsExplictlyClose;
    }

    public void ReleaseStream(Stream stream)
    {
        if (!IsReusableStream(stream))
        {
            stream.Dispose();
        }
    }

    public void Dispose()
    {
        if (IsNewStream)
        {
            InnerStream?.ExplictlyClose();
        }
        InnerStream = null;
        StreamProvider = null;
    }
}