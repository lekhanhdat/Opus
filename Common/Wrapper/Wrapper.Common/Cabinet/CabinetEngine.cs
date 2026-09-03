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
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;

    public abstract class CabinetEngine : IDisposable
    {
        private byte[] buf;
        private IDictionary<string, int> cabNumbers;
        private Stream cabStream;
        internal const string CabStreamName = "%%CAB%%";
        private NativeMethods.ERF erf = new NativeMethods.ERF();
        private GCHandle erfHandle;
        private Stream fileStream;
        private string nextCabinetName;
        private EventHandler<CabinetProgressEventArgs> progress;
        private CabinetProgressEventArgs progressData;
        private HandleManager<Stream> streamHandles = new HandleManager<Stream>();
        private bool suppressProgressEvents;

        public event EventHandler<CabinetProgressEventArgs> Progress
        {
            add
            {
                EventHandler<CabinetProgressEventArgs> handler2;
                EventHandler<CabinetProgressEventArgs> progress = this.progress;
                do
                {
                    handler2 = progress;
                    EventHandler<CabinetProgressEventArgs> handler3 = (EventHandler<CabinetProgressEventArgs>) Delegate.Combine(handler2, value);
                    progress = Interlocked.CompareExchange<EventHandler<CabinetProgressEventArgs>>(ref this.progress, handler3, handler2);
                }
                while (progress != handler2);
            }
            remove
            {
                EventHandler<CabinetProgressEventArgs> handler2;
                EventHandler<CabinetProgressEventArgs> progress = this.progress;
                do
                {
                    handler2 = progress;
                    EventHandler<CabinetProgressEventArgs> handler3 = (EventHandler<CabinetProgressEventArgs>) Delegate.Remove(handler2, value);
                    progress = Interlocked.CompareExchange<EventHandler<CabinetProgressEventArgs>>(ref this.progress, handler3, handler2);
                }
                while (progress != handler2);
            }
        }

        internal CabinetEngine()
        {
            this.erfHandle = GCHandle.Alloc(this.erf, GCHandleType.Pinned);
            this.progressData = new CabinetProgressEventArgs();
            this.cabNumbers = new Dictionary<string, int>(1);
            this.buf = new byte[0x8000];
        }

        internal IntPtr CabAllocMem(int byteCount)
        {
            return Marshal.AllocHGlobal((IntPtr) byteCount);
        }

        internal int CabCloseStream(int streamHandle)
        {
            int num;
            return this.CabCloseStreamEx(streamHandle, out num, IntPtr.Zero);
        }

        internal virtual int CabCloseStreamEx(int streamHandle, out int err, IntPtr pv)
        {
            this.streamHandles.FreeHandle(streamHandle);
            err = 0;
            return 0;
        }

        internal void CabFreeMem(IntPtr memPointer)
        {
            Marshal.FreeHGlobal(memPointer);
        }

        internal int CabOpenStream(string path, int openFlags, int shareMode)
        {
            int num;
            return this.CabOpenStreamEx(path, openFlags, shareMode, out num, IntPtr.Zero);
        }

        internal virtual int CabOpenStreamEx(string path, int openFlags, int shareMode, out int err, IntPtr pv)
        {
            path = path.Trim();
            Stream cabStream = this.cabStream;
            this.cabStream = new DuplicateStream(cabStream);
            int num = this.streamHandles.AllocHandle(cabStream);
            err = 0;
            return num;
        }

        internal int CabReadStream(int streamHandle, IntPtr memory, int cb)
        {
            int num;
            return this.CabReadStreamEx(streamHandle, memory, cb, out num, IntPtr.Zero);
        }

        internal virtual int CabReadStreamEx(int streamHandle, IntPtr memory, int cb, out int err, IntPtr pv)
        {
            Stream stream = this.streamHandles[streamHandle];
            int count = cb;
            if (count > this.buf.Length)
            {
                this.buf = new byte[count];
            }
            count = stream.Read(this.buf, 0, count);
            Marshal.Copy(this.buf, 0, memory, count);
            err = 0;
            return count;
        }

        internal int CabSeekStream(int streamHandle, int offset, int seekOrigin)
        {
            int num;
            return this.CabSeekStreamEx(streamHandle, offset, seekOrigin, out num, IntPtr.Zero);
        }

        internal virtual int CabSeekStreamEx(int streamHandle, int offset, int seekOrigin, out int err, IntPtr pv)
        {
            Stream stream = this.streamHandles[streamHandle];
            offset = (int) stream.Seek((long) offset, (SeekOrigin) seekOrigin);
            err = 0;
            return offset;
        }

        internal int CabWriteStream(int streamHandle, IntPtr memory, int cb)
        {
            int num;
            return this.CabWriteStreamEx(streamHandle, memory, cb, out num, IntPtr.Zero);
        }

        internal virtual int CabWriteStreamEx(int streamHandle, IntPtr memory, int cb, out int err, IntPtr pv)
        {
            Stream stream = this.streamHandles[streamHandle];
            int length = cb;
            if (length > this.buf.Length)
            {
                this.buf = new byte[length];
            }
            Marshal.Copy(memory, this.buf, 0, length);
            stream.Write(this.buf, 0, length);
            err = 0;
            return cb;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.cabStream != null)
                {
                    this.cabStream.Close();
                    this.cabStream = null;
                }
                if (this.fileStream != null)
                {
                    this.fileStream.Close();
                    this.fileStream = null;
                }
            }
            if (this.erfHandle.IsAllocated)
            {
                this.erfHandle.Free();
            }
        }

        ~CabinetEngine()
        {
            this.Dispose(false);
        }

        internal void OnProgress()
        {
            if (!this.suppressProgressEvents && (this.progress != null))
            {
                CabinetProgressEventArgs e = new CabinetProgressEventArgs(this.progressData.ProgressType, this.progressData.CurrentFileName, (this.progressData.CurrentFileNumber >= 0) ? this.progressData.CurrentFileNumber : 0, this.progressData.TotalFiles, this.progressData.CurrentFileBytesProcessed, this.progressData.CurrentFileTotalBytes, this.progressData.CurrentFolderNumber, this.progressData.CurrentFolderBytesProcessed, this.progressData.CurrentFolderTotalBytes, this.progressData.CurrentCabinetName, this.progressData.CurrentCabinetNumber, this.progressData.TotalCabinets, this.progressData.CurrentCabinetBytesProcessed, this.progressData.CurrentCabinetTotalBytes, this.progressData.FileBytesProcessed, this.progressData.TotalFileBytes);
                this.progress(this, e);
            }
        }

        internal IDictionary<string, int> CabNumbers
        {
            get
            {
                return this.cabNumbers;
            }
        }

        internal Stream CabStream
        {
            get
            {
                return this.cabStream;
            }
            set
            {
                this.cabStream = value;
            }
        }

        internal NativeMethods.ERF Erf
        {
            get
            {
                return this.erf;
            }
        }

        internal GCHandle ErfHandle
        {
            get
            {
                return this.erfHandle;
            }
        }

        internal Stream FileStream
        {
            get
            {
                return this.fileStream;
            }
            set
            {
                this.fileStream = value;
            }
        }

        internal string NextCabinetName
        {
            get
            {
                return this.nextCabinetName;
            }
            set
            {
                this.nextCabinetName = value;
            }
        }

        internal CabinetProgressEventArgs ProgressData
        {
            get
            {
                return this.progressData;
            }
            set
            {
                this.progressData = value;
            }
        }

        internal HandleManager<Stream> StreamHandles
        {
            get
            {
                return this.streamHandles;
            }
        }

        internal bool SuppressProgressEvents
        {
            get
            {
                return this.suppressProgressEvents;
            }
            set
            {
                this.suppressProgressEvents = value;
            }
        }
    }
}

