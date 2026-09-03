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
namespace AvePoint.Office365.Api.AIR.IPC
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using ComTypes = System.Runtime.InteropServices.ComTypes;

    internal class ILockBytesOverStream : ILockBytes
    {
        private Stream stream;

        public ILockBytesOverStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (!stream.CanSeek)
            {
                throw new ArgumentException("The passed in stream must be seekable", "stream");
            }
            this.stream = stream;
        }

        public void ReadAt(ulong offset, byte[] buffer, int count, IntPtr pBytesRead)
        {
            int bytesRead = 0;
            if (buffer.Length < count)
            {
                throw new ArgumentException("Requesting more bytes from the stream than will fit in the supplied buffer", "count");
            }

            int bytesToRead = count;
            bytesRead = 0;

            this.stream.Seek((long)offset, SeekOrigin.Begin);

            // Read may return fewer bytes than requested even if there are more bytes available.  We
            // keep reading from the stream until we've gathered the request number, or hit the EOF.
            while (bytesToRead > 0)
            {
                int currentRead = this.stream.Read(buffer, bytesRead, bytesToRead);

                if (currentRead == 0)
                {
                    break;
                }

                bytesToRead -= currentRead;
                bytesRead += currentRead;
            }

            if (IntPtr.Zero != pBytesRead)
            {
                Marshal.WriteInt32(pBytesRead, bytesRead);
            }
        }

        public void WriteAt(ulong offset, byte[] buffer, int count, IntPtr pBytesWritten)
        {
            this.stream.Seek((long)offset, SeekOrigin.Begin);
            this.stream.Write(buffer, 0, count);

            if (IntPtr.Zero != pBytesWritten)
            {
                Marshal.WriteInt32(pBytesWritten, count);
            }
        }

        public void Flush()
        {
            this.stream.Flush();
        }

        public void SetSize(ulong length)
        {
            this.stream.SetLength((long)length);
        }
        
        public void LockRegion(ulong libOffset, ulong cb, int dwLockType)
        {
        }

        public void UnlockRegion(ulong libOffset, ulong cb, int dwLockType)
        {
        }

        public void Stat(out ComTypes.STATSTG pstatstg, STATFLAG grfStatFlag)
        {
            pstatstg = new ComTypes.STATSTG();
            pstatstg.type = (int)STGTY.Stream;
            pstatstg.cbSize = this.stream.Length;
            pstatstg.grfLocksSupported = (int)LOCKTYPE.Exclusive;
        }
    }
}
