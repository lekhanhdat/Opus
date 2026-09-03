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
namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Diagnostics;
    using System.IO;
    public class AveReusableStreamProvider : IDisposable
    {
        public static Stopwatch Timer = new Stopwatch();
        public static long Times = 0;
        private const int MemoryStreamLimit = 20 * 1024 * 1024;
        protected AveCoordinatedStream InnerStream;
        protected bool IsNewStream;
        public AveReusableStreamProvider(Stream stream, bool closeOriginalStreamAfterClone = false)
        {
            Timer.Start();
            if (IsReusableStream(stream))
            {
                InnerStream = stream as AveCoordinatedStream;
                IsNewStream = false;
            }
            else
            {
                InnerStream = new AveCoordinatedStream("ReusableStream", 0, true, MemoryStreamLimit);
                AveIOHelper.Copy(stream, InnerStream);
                if (closeOriginalStreamAfterClone)
                {
                    stream.Close();
                }
                IsNewStream = true;
            }
            Timer.Stop();
            Times++;
        }

        public Stream GetStream()
        {
            InnerStream.Position = 0;
            return InnerStream;
        }

        private static bool IsReusableStream(Stream stream)
        {
            return stream is AveCoordinatedStream && (stream as AveCoordinatedStream).IsExplictlyClose;
        }

        public void Dispose()
        {
            if (IsNewStream)
            {
                InnerStream.ExplictlyClose();
            }
            InnerStream = null;
        }
    }
}
