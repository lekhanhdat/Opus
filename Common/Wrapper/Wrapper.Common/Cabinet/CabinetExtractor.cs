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
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    public class CabinetExtractor : CabinetEngine
    {
        private ICabinetExtractStreamContext context;
        private NativeMethods.FDI.PFNALLOC fdiAllocMemHandler;
        private NativeMethods.FDI.PFNCLOSE fdiCloseStreamHandler;
        private NativeMethods.FDI.PFNFREE fdiFreeMemHandler;
        private NativeMethods.FDI.Handle fdiHandle;
        private NativeMethods.FDI.PFNOPEN fdiOpenStreamHandler;
        private NativeMethods.FDI.PFNREAD fdiReadStreamHandler;
        private NativeMethods.FDI.PFNSEEK fdiSeekStreamHandler;
        private NativeMethods.FDI.PFNWRITE fdiWriteStreamHandler;
        private IList<CabinetFileInfo> fileList;
        private Predicate<string> filter;
        private int folderId;
        private readonly object innerLock = new object();

        //[SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        public CabinetExtractor()
        {
            this.fdiAllocMemHandler = new NativeMethods.FDI.PFNALLOC(this.CabAllocMem);
            this.fdiFreeMemHandler = new NativeMethods.FDI.PFNFREE(this.CabFreeMem);
            this.fdiOpenStreamHandler = new NativeMethods.FDI.PFNOPEN(this.CabOpenStream);
            this.fdiReadStreamHandler = new NativeMethods.FDI.PFNREAD(this.CabReadStream);
            this.fdiWriteStreamHandler = new NativeMethods.FDI.PFNWRITE(this.CabWriteStream);
            this.fdiCloseStreamHandler = new NativeMethods.FDI.PFNCLOSE(this.CabCloseStream);
            this.fdiSeekStreamHandler = new NativeMethods.FDI.PFNSEEK(this.CabSeekStream);
            this.fdiHandle = NativeMethods.FDI.Create(this.fdiAllocMemHandler, this.fdiFreeMemHandler, this.fdiOpenStreamHandler, this.fdiReadStreamHandler, this.fdiWriteStreamHandler, this.fdiCloseStreamHandler, this.fdiSeekStreamHandler, 1, base.ErfHandle.AddrOfPinnedObject());
            if (base.Erf.Error)
            {
                int oper = base.Erf.Oper;
                int type = base.Erf.Type;
                base.ErfHandle.Free();
                throw new CabinetException(oper, type, CabinetException.GetErrorMessage(oper, type, true));
            }
        }

        internal override int CabCloseStreamEx(int streamHandle, out int err, IntPtr pv)
        {
            Stream stream = DuplicateStream.OriginalStream(base.StreamHandles[streamHandle]);
            if (stream == DuplicateStream.OriginalStream(base.CabStream))
            {
                if ((this.fileList == null) && (this.folderId >= 0))
                {
                    base.ProgressData.ProgressType = CabinetProgressType.FinishFolder;
                    base.OnProgress();
                }
                if (this.folderId != -3)
                {
                    base.ProgressData.ProgressType = CabinetProgressType.FinishCab;
                    base.OnProgress();
                }
                this.context.CloseCabinetReadStream(base.ProgressData.CurrentCabinetNumber, base.ProgressData.CurrentCabinetName, stream);
                base.ProgressData.CurrentCabinetName = base.NextCabinetName;
                base.ProgressData.CurrentCabinetBytesProcessed = base.ProgressData.CurrentCabinetTotalBytes = 0L;
                base.CabStream = null;
            }
            return base.CabCloseStreamEx(streamHandle, out err, pv);
        }

        private int CabExtractCloseFile(NativeMethods.FDI.NOTIFICATION notification)
        {
            DateTime time;
            Stream stream = base.StreamHandles[notification.hf];
            base.StreamHandles.FreeHandle(notification.hf);
            string fileName = GetFileName(notification);
            FileAttributes normal = ((FileAttributes) notification.attribs) & (FileAttributes.Archive | FileAttributes.System | FileAttributes.Hidden | FileAttributes.ReadOnly);
            if (normal == 0)
            {
                normal = FileAttributes.Normal;
            }
            NativeMethods.FDI.CabDateAndTimeToDateTime(notification.date, notification.time, out time);
            stream.Flush();
            this.context.CloseFileWriteStream(fileName, stream, normal, time);
            base.FileStream = null;
            long num = base.ProgressData.CurrentFileTotalBytes - base.ProgressData.CurrentFileBytesProcessed;
            CabinetProgressEventArgs progressData = base.ProgressData;
            progressData.CurrentFileBytesProcessed += num;
            CabinetProgressEventArgs args2 = base.ProgressData;
            args2.FileBytesProcessed += num;
            base.ProgressData.ProgressType = CabinetProgressType.FinishFile;
            base.OnProgress();
            base.ProgressData.CurrentFileName = null;
            return 1;
        }

        private int CabExtractCopyFile(NativeMethods.FDI.NOTIFICATION notification)
        {
            if (notification.iFolder != this.folderId)
            {
                if (notification.iFolder != -3)
                {
                    if (this.folderId != -1)
                    {
                        base.ProgressData.ProgressType = CabinetProgressType.FinishFolder;
                        base.OnProgress();
                        CabinetProgressEventArgs progressData = base.ProgressData;
                        progressData.CurrentFolderNumber++;
                    }
                    base.ProgressData.ProgressType = CabinetProgressType.StartFolder;
                    base.OnProgress();
                }
                this.folderId = notification.iFolder;
            }
            string fileName = GetFileName(notification);
            if ((this.filter == null) || this.filter(fileName))
            {
                DateTime time;
                CabinetProgressEventArgs args2 = base.ProgressData;
                args2.CurrentFileNumber++;
                base.ProgressData.CurrentFileName = fileName;
                base.ProgressData.CurrentFileBytesProcessed = 0L;
                base.ProgressData.CurrentFileTotalBytes = notification.cb;
                base.ProgressData.ProgressType = CabinetProgressType.StartFile;
                base.OnProgress();
                NativeMethods.FDI.CabDateAndTimeToDateTime(notification.date, notification.time, out time);
                Stream stream = this.context.OpenFileWriteStream(fileName, (long) notification.cb, time);
                if (stream != null)
                {
                    base.FileStream = stream;
                    return base.StreamHandles.AllocHandle(stream);
                }
                CabinetProgressEventArgs args3 = base.ProgressData;
                args3.FileBytesProcessed += notification.cb;
                base.ProgressData.ProgressType = CabinetProgressType.FinishFile;
                base.OnProgress();
                base.ProgressData.CurrentFileName = null;
            }
            return 0;
        }

        private int CabExtractNotify(NativeMethods.FDI.NOTIFICATIONTYPE notificationType, NativeMethods.FDI.NOTIFICATION notification)
        {
            switch (notificationType)
            {
                case NativeMethods.FDI.NOTIFICATIONTYPE.CABINET_INFO:
                    if ((base.NextCabinetName == null) || !base.NextCabinetName.StartsWith("?", StringComparison.Ordinal))
                    {
                        string str = Marshal.PtrToStringAnsi(notification.psz1);
                        base.NextCabinetName = (str.Length != 0) ? str : null;
                        break;
                    }
                    base.NextCabinetName = base.NextCabinetName.Substring(1);
                    break;

                case NativeMethods.FDI.NOTIFICATIONTYPE.COPY_FILE:
                    return this.CabExtractCopyFile(notification);

                case NativeMethods.FDI.NOTIFICATIONTYPE.CLOSE_FILE_INFO:
                    return this.CabExtractCloseFile(notification);

                case NativeMethods.FDI.NOTIFICATIONTYPE.NEXT_CABINET:
                {
                    string str2 = Marshal.PtrToStringAnsi(notification.psz1);
                    base.CabNumbers[str2] = notification.iCabinet;
                    base.NextCabinetName = "?" + base.NextCabinetName;
                    return 0;
                }
                default:
                    return 0;
            }
            return 0;
        }

        private int CabListNotify(NativeMethods.FDI.NOTIFICATIONTYPE notificationType, NativeMethods.FDI.NOTIFICATION notification)
        {
            switch (notificationType)
            {
                case NativeMethods.FDI.NOTIFICATIONTYPE.CABINET_INFO:
                {
                    string str = Marshal.PtrToStringAnsi(notification.psz1);
                    base.NextCabinetName = (str.Length != 0) ? str : null;
                    return 0;
                }
                case NativeMethods.FDI.NOTIFICATIONTYPE.PARTIAL_FILE:
                    if (this.fileList.Count > 0)
                    {
                        this.fileList[this.fileList.Count - 1].EndCabinetNumber = base.ProgressData.CurrentCabinetNumber;
                    }
                    return 0;

                case NativeMethods.FDI.NOTIFICATIONTYPE.COPY_FILE:
                {
                    string fileName = GetFileName(notification);
                    if (((this.filter == null) || this.filter(fileName)) && (this.fileList != null))
                    {
                        DateTime time;
                        FileAttributes normal = ((FileAttributes) notification.attribs) & (FileAttributes.Archive | FileAttributes.System | FileAttributes.Hidden | FileAttributes.ReadOnly);
                        if (normal == 0)
                        {
                            normal = FileAttributes.Normal;
                        }
                        NativeMethods.FDI.CabDateAndTimeToDateTime(notification.date, notification.time, out time);
                        long cb = notification.cb;
                        CabinetFileInfo item = new CabinetFileInfo(Path.GetFileName(fileName), Path.GetDirectoryName(fileName), notification.iFolder, notification.iCabinet, notification.iCabinet, normal, time, cb);
                        this.fileList.Add(item);
                        base.ProgressData.CurrentFileNumber = this.fileList.Count - 1;
                        CabinetProgressEventArgs args1 = base.ProgressData;
                        args1.FileBytesProcessed += notification.cb;
                    }
                    CabinetProgressEventArgs progressData = base.ProgressData;
                    progressData.TotalFiles++;
                    CabinetProgressEventArgs args3 = base.ProgressData;
                    args3.TotalFileBytes += notification.cb;
                    return 0;
                }
            }
            return 0;
        }

        internal override int CabOpenStreamEx(string path, int openFlags, int shareMode, out int err, IntPtr pv)
        {
            if (base.CabNumbers.ContainsKey(path))
            {
                if (base.CabStream == null)
                {
                    int cabinetNumber = base.CabNumbers[path];
                    Stream stream = this.context.OpenCabinetReadStream(cabinetNumber, path);
                    if (stream == null)
                    {
                        throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, "Cabinet {0} not provided.", new object[] { cabinetNumber }));
                    }
                    base.ProgressData.CurrentCabinetName = path;
                    base.ProgressData.CurrentCabinetNumber = cabinetNumber;
                    if (base.ProgressData.TotalCabinets <= base.ProgressData.CurrentCabinetNumber)
                    {
                        int num2 = base.ProgressData.CurrentCabinetNumber + 1;
                        base.ProgressData.TotalCabinets = (short) num2;
                    }
                    base.ProgressData.CurrentCabinetTotalBytes = stream.Length;
                    base.ProgressData.CurrentCabinetBytesProcessed = 0L;
                    if (this.folderId != -3)
                    {
                        base.ProgressData.ProgressType = CabinetProgressType.StartCab;
                        base.OnProgress();
                    }
                    base.CabStream = stream;                                 
                }
                path = "%%CAB%%";
            }
            return base.CabOpenStreamEx(path, openFlags, shareMode, out err, pv);
        }

        internal override int CabReadStreamEx(int streamHandle, IntPtr memory, int cb, out int err, IntPtr pv)
        {
            int num = base.CabReadStreamEx(streamHandle, memory, cb, out err, pv);
            if (((err == 0) && (base.CabStream != null)) && (this.fileList == null))
            {
                Stream stream = base.StreamHandles[streamHandle];
                if (DuplicateStream.OriginalStream(stream) == DuplicateStream.OriginalStream(base.CabStream))
                {
                    CabinetProgressEventArgs progressData = base.ProgressData;
                    progressData.CurrentCabinetBytesProcessed += cb;
                    if (base.ProgressData.CurrentCabinetBytesProcessed > base.ProgressData.CurrentCabinetTotalBytes)
                    {
                        base.ProgressData.CurrentCabinetBytesProcessed = base.ProgressData.CurrentCabinetTotalBytes;
                    }
                }
            }
            return num;
        }

        internal override int CabWriteStreamEx(int streamHandle, IntPtr memory, int cb, out int err, IntPtr pv)
        {
            int num = base.CabWriteStreamEx(streamHandle, memory, cb, out err, pv);
            if ((num > 0) && (err == 0))
            {
                CabinetProgressEventArgs progressData = base.ProgressData;
                progressData.CurrentFileBytesProcessed += cb;
                CabinetProgressEventArgs args2 = base.ProgressData;
                args2.FileBytesProcessed += cb;
                base.ProgressData.ProgressType = CabinetProgressType.PartialFile;
                base.OnProgress();
            }
            return num;
        }

        private void CheckError()
        {
            if (base.Erf.Error)
            {
                throw new CabinetException(base.Erf.Oper, base.Erf.Type, CabinetException.GetErrorMessage(base.Erf.Oper, base.Erf.Type, true));
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && (this.fdiHandle != null))
                {
                    this.fdiHandle.Dispose();
                    this.fdiHandle = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public Stream Extract(Stream stream, string path)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            BasicExtractStreamContext streamContext = new BasicExtractStreamContext(stream);
            this.Extract(streamContext, false, delegate (string match) {
                return string.Compare(match, path, true, CultureInfo.InvariantCulture) == 0;
            });
            Stream fileStream = streamContext.FileStream;
            if (fileStream != null)
            {
                fileStream.Position = 0L;
            }
            return fileStream;
        }

        //[SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        public void Extract(ICabinetExtractStreamContext streamContext, bool chain, Predicate<string> fileFilter)
        {
            if (streamContext == null)
            {
                throw new ArgumentNullException("streamContext");
            }
            lock (this.innerLock)
            {
                try
                {
                    IList<CabinetFileInfo> list = null;
                    bool suppressProgressEvents = base.SuppressProgressEvents;
                    try
                    {
                        base.SuppressProgressEvents = true;
                        list = this.GetFileInfo(streamContext, chain, fileFilter);
                    }
                    finally
                    {
                        base.SuppressProgressEvents = suppressProgressEvents;
                    }
                    base.ProgressData = new CabinetProgressEventArgs();
                    if (list != null)
                    {
                        base.ProgressData.TotalFiles = list.Count;
                        for (int j = 0; j < list.Count; j++)
                        {
                            CabinetProgressEventArgs progressData = base.ProgressData;
                            progressData.TotalFileBytes += list[j].Length;
                            if (list[j].EndCabinetNumber >= base.ProgressData.TotalCabinets)
                            {
                                int num2 = list[j].EndCabinetNumber + 1;
                                base.ProgressData.TotalCabinets = (short) num2;
                            }
                        }
                    }
                    this.context = streamContext;
                    this.fileList = null;
                    base.NextCabinetName = string.Empty;
                    this.folderId = -1;
                    base.ProgressData.CurrentFileNumber = -1;
                    for (short i = 0; (chain || (i == 0)) && (base.NextCabinetName != null); i = (short) (i + 1))
                    {
                        base.Erf.Clear();
                        base.CabNumbers[base.NextCabinetName] = i;
                        NativeMethods.FDI.Copy(this.fdiHandle, base.NextCabinetName, string.Empty, 0, new NativeMethods.FDI.PFNNOTIFY(this.CabExtractNotify), IntPtr.Zero, IntPtr.Zero);
                        this.CheckError();
                    }
                }
                finally
                {
                    if (base.CabStream != null)
                    {
                        this.context.CloseCabinetReadStream(base.ProgressData.CurrentCabinetNumber, base.ProgressData.CurrentCabinetName, base.CabStream);
                        base.CabStream = null;
                    }
                    if (base.FileStream != null)
                    {
                        this.context.CloseFileWriteStream(base.ProgressData.CurrentFileName, base.FileStream, FileAttributes.Normal, DateTime.Now);
                        base.FileStream = null;
                    }
                    this.context = null;
                }
            }
        }

        //[SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        public long FindCabinetOffset(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            lock (this.innerLock)
            {
                long num4 = 4L;
                long length = stream.Length;
                for (long i = 0L; i < length; i += num4)
                {
                    short num;
                    int num2;
                    int num3;
                    stream.Seek(i, SeekOrigin.Begin);
                    if (this.IsCabinet(stream, out num, out num2, out num3))
                    {
                        return i;
                    }
                }
                return -1L;
            }
        }

        public IList<CabinetFileInfo> GetFileInfo(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            return this.GetFileInfo(new BasicExtractStreamContext(stream), false, null);
        }

        //[SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        public IList<CabinetFileInfo> GetFileInfo(ICabinetExtractStreamContext streamContext, bool chain, Predicate<string> fileFilter)
        {
            if (streamContext == null)
            {
                throw new ArgumentNullException("streamContext");
            }
            lock (this.innerLock)
            {
                this.context = streamContext;
                this.filter = fileFilter;
                base.NextCabinetName = string.Empty;
                this.fileList = new List<CabinetFileInfo>();
                for (short i = 0; (chain || (i == 0)) && (base.NextCabinetName != null); i = (short) (i + 1))
                {
                    base.Erf.Clear();
                    base.CabNumbers[base.NextCabinetName] = i;
                    NativeMethods.FDI.Copy(this.fdiHandle, base.NextCabinetName, string.Empty, 0, new NativeMethods.FDI.PFNNOTIFY(this.CabListNotify), IntPtr.Zero, IntPtr.Zero);
                    this.CheckError();
                }
                IList<CabinetFileInfo> fileList = this.fileList;
                this.fileList = null;
                return fileList;
            }
        }

        private static string GetFileName(NativeMethods.FDI.NOTIFICATION notification)
        {
            Encoding encoding = ((notification.attribs & 0x80) != 0) ? Encoding.UTF8 : Encoding.Default;
            int ofs = 0;
            while (Marshal.ReadByte(notification.psz1, ofs) != 0)
            {
                ofs++;
            }
            byte[] destination = new byte[ofs];
            Marshal.Copy(notification.psz1, destination, 0, ofs);
            return encoding.GetString(destination);
        }

        public IList<string> GetFiles(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            return this.GetFiles(new BasicExtractStreamContext(stream), false, null);
        }

        //[SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        public IList<string> GetFiles(ICabinetExtractStreamContext streamContext, bool chain, Predicate<string> fileFilter)
        {
            if (streamContext == null)
            {
                throw new ArgumentNullException("streamContext");
            }
            IList<CabinetFileInfo> list = this.GetFileInfo(streamContext, chain, fileFilter);
            IList<string> list2 = new List<string>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                list2.Add(list[i].Name);
            }
            return list2;
        }

        //[SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        public bool IsCabinet(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            lock (this.innerLock)
            {
                short num;
                int num2;
                int num3;
                return this.IsCabinet(stream, out num, out num2, out num3);
            }
        }

        private bool IsCabinet(Stream cabStream, out short id, out int cabFolderCount, out int fileCount)
        {
            bool flag2;
            int hf = base.StreamHandles.AllocHandle(cabStream);
            try
            {
                NativeMethods.FDI.CABINFO cabinfo;
                base.Erf.Clear();
                bool flag = 0 != NativeMethods.FDI.IsCabinet(this.fdiHandle, hf, out cabinfo);
                if (base.Erf.Error)
                {
                    if (base.Erf.Oper != 3)
                    {
                        throw new CabinetException(base.Erf.Oper, base.Erf.Type, CabinetException.GetErrorMessage(base.Erf.Oper, base.Erf.Type, true));
                    }
                    flag = false;
                }
                id = cabinfo.setID;
                cabFolderCount = cabinfo.cFolders;
                fileCount = cabinfo.cFiles;
                flag2 = flag;
            }
            finally
            {
                base.StreamHandles.FreeHandle(hf);
            }
            return flag2;
        }
    }
}

