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

    public class CabinetCreator : CabinetEngine
    {
        private CabinetCompressionLevel compressionLevel;
        private ICabinetCreateStreamContext context;
        private bool dontUseTempFiles;
        private NativeMethods.FCI.PFNALLOC fciAllocMemHandler;
        private NativeMethods.FCI.PFNCLOSE fciCloseStreamHandler;
        private NativeMethods.FCI.PFNDELETE fciDeleteFileHandler;
        private NativeMethods.FCI.PFNFILEPLACED fciFilePlacedHandler;
        private NativeMethods.FCI.PFNFREE fciFreeMemHandler;
        private NativeMethods.FCI.PFNGETTEMPFILE fciGetTempFileHandler;
        private NativeMethods.FCI.Handle fciHandle;
        private NativeMethods.FCI.PFNOPEN fciOpenStreamHandler;
        private NativeMethods.FCI.PFNREAD fciReadStreamHandler;
        private NativeMethods.FCI.PFNSEEK fciSeekStreamHandler;
        private NativeMethods.FCI.PFNWRITE fciWriteStreamHandler;
        private FileAttributes fileAttributes;
        private DateTime fileLastWriteTime;
        private object innerLock = new object();
        private int maxCabBytes;
        private const string TempStreamName = "%%TEMP%%";
        private IList<Stream> tempStreams;
        private long totalFolderBytesProcessedInCurrentCab;

        public CabinetCreator()
        {
            this.fciAllocMemHandler = new NativeMethods.FCI.PFNALLOC(this.CabAllocMem);
            this.fciFreeMemHandler = new NativeMethods.FCI.PFNFREE(this.CabFreeMem);
            this.fciOpenStreamHandler = new NativeMethods.FCI.PFNOPEN(this.CabOpenStreamEx);
            this.fciReadStreamHandler = new NativeMethods.FCI.PFNREAD(this.CabReadStreamEx);
            this.fciWriteStreamHandler = new NativeMethods.FCI.PFNWRITE(this.CabWriteStreamEx);
            this.fciCloseStreamHandler = new NativeMethods.FCI.PFNCLOSE(this.CabCloseStreamEx);
            this.fciSeekStreamHandler = new NativeMethods.FCI.PFNSEEK(this.CabSeekStreamEx);
            this.fciFilePlacedHandler = new NativeMethods.FCI.PFNFILEPLACED(this.CabFilePlaced);
            this.fciDeleteFileHandler = new NativeMethods.FCI.PFNDELETE(this.CabDeleteFile);
            this.fciGetTempFileHandler = new NativeMethods.FCI.PFNGETTEMPFILE(this.CabGetTempFile);
            this.tempStreams = new List<Stream>();
            this.compressionLevel = CabinetCompressionLevel.Normal;
        }

        public void AddFile(string name, Stream stream, FileAttributes attributes, DateTime lastWriteTime, bool execute, CabinetCompressionLevel compLevel)
        {
            base.FileStream = stream;
            this.fileAttributes = attributes & (FileAttributes.Archive | FileAttributes.System | FileAttributes.Hidden | FileAttributes.ReadOnly);
            this.fileLastWriteTime = lastWriteTime;
            base.ProgressData.CurrentFileName = name;
            NativeMethods.FCI.TCOMP compressionType = GetCompressionType(compLevel);
            IntPtr zero = IntPtr.Zero;
            try
            {
                Encoding aSCII = Encoding.ASCII;
                if (Encoding.UTF8.GetByteCount(name) > name.Length)
                {
                    aSCII = Encoding.UTF8;
                    this.fileAttributes |= FileAttributes.Normal;
                }
                byte[] bytes = aSCII.GetBytes(name);
                zero = Marshal.AllocHGlobal((int) (bytes.Length + 1));
                Marshal.Copy(bytes, 0, zero, bytes.Length);
                Marshal.WriteByte(zero, bytes.Length, 0);
                base.Erf.Clear();
                NativeMethods.FCI.AddFile(this.fciHandle, string.Empty, zero, execute, new NativeMethods.FCI.PFNGETNEXTCABINET(this.CabGetNextCabinet), new NativeMethods.FCI.PFNSTATUS(this.CabCreateStatus), new NativeMethods.FCI.PFNGETOPENINFO(this.CabGetOpenInfo), compressionType);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
            }
            this.CheckError();
            base.FileStream = null;
            base.ProgressData.CurrentFileName = null;
        }

        internal override int CabCloseStreamEx(int streamHandle, out int err, IntPtr pv)
        {
            Stream stream = DuplicateStream.OriginalStream(base.StreamHandles[streamHandle]);
            if (stream == DuplicateStream.OriginalStream(base.FileStream))
            {
                this.context.CloseFileReadStream(base.ProgressData.CurrentFileName, stream);
                base.FileStream = null;
                long num = base.ProgressData.CurrentFileTotalBytes - base.ProgressData.CurrentFileBytesProcessed;
                CabinetProgressEventArgs progressData = base.ProgressData;
                progressData.CurrentFileBytesProcessed += num;
                CabinetProgressEventArgs args2 = base.ProgressData;
                args2.FileBytesProcessed += num;
                base.ProgressData.ProgressType = CabinetProgressType.FinishFile;
                base.OnProgress();
                base.ProgressData.CurrentFileTotalBytes = 0L;
                base.ProgressData.CurrentFileBytesProcessed = 0L;
                base.ProgressData.CurrentFileName = null;
            }
            else if (stream == DuplicateStream.OriginalStream(base.CabStream))
            {
                if (stream.CanWrite)
                {
                    stream.Flush();
                }
                base.ProgressData.CurrentCabinetBytesProcessed = base.ProgressData.CurrentCabinetTotalBytes;
                base.ProgressData.ProgressType = CabinetProgressType.FinishCab;
                base.OnProgress();
                CabinetProgressEventArgs args3 = base.ProgressData;
                args3.CurrentCabinetNumber++;
                CabinetProgressEventArgs args4 = base.ProgressData;
                args4.TotalCabinets++;
                this.context.CloseCabinetWriteStream(base.ProgressData.CurrentCabinetNumber, base.ProgressData.CurrentCabinetName, stream);
                base.ProgressData.CurrentCabinetName = base.NextCabinetName;
                base.ProgressData.CurrentCabinetBytesProcessed = base.ProgressData.CurrentCabinetTotalBytes = 0L;
                this.totalFolderBytesProcessedInCurrentCab = 0L;
                base.CabStream = null;
            }
            else
            {
                stream.Close();
                this.tempStreams.Remove(stream);
            }
            return base.CabCloseStreamEx(streamHandle, out err, pv);
        }

        private int CabCreateStatus(NativeMethods.FCI.STATUS typeStatus, uint cb1, uint cb2, IntPtr pv)
        {
            switch (typeStatus)
            {
                case NativeMethods.FCI.STATUS.FILE:
                    if ((cb2 > 0) && (base.ProgressData.CurrentFileBytesProcessed < base.ProgressData.CurrentFileTotalBytes))
                    {
                        if ((base.ProgressData.CurrentFileBytesProcessed + cb2) > base.ProgressData.CurrentFileTotalBytes)
                        {
                            cb2 = ((uint) base.ProgressData.CurrentFileTotalBytes) - ((uint) base.ProgressData.CurrentFileBytesProcessed);
                        }
                        CabinetProgressEventArgs progressData = base.ProgressData;
                        progressData.CurrentFileBytesProcessed += cb2;
                        CabinetProgressEventArgs args2 = base.ProgressData;
                        args2.FileBytesProcessed += cb2;
                        base.ProgressData.ProgressType = CabinetProgressType.PartialFile;
                        base.OnProgress();
                    }
                    break;

                case NativeMethods.FCI.STATUS.FOLDER:
                    if (cb1 != 0)
                    {
                        if (base.ProgressData.CurrentFolderTotalBytes > 0L)
                        {
                            base.ProgressData.CurrentFolderBytesProcessed = cb1;
                            base.ProgressData.ProgressType = CabinetProgressType.PartialFolder;
                        }
                        else
                        {
                            base.ProgressData.ProgressType = CabinetProgressType.PartialCab;
                        }
                        break;
                    }
                    base.ProgressData.CurrentFolderBytesProcessed = cb1;
                    base.ProgressData.CurrentFolderTotalBytes = cb2 - this.totalFolderBytesProcessedInCurrentCab;
                    this.totalFolderBytesProcessedInCurrentCab = cb2;
                    base.ProgressData.ProgressType = CabinetProgressType.StartFolder;
                    base.OnProgress();
                    break;

                default:
                    break;
            }          
        
            return 0;
        }

        private int CabDeleteFile(string path, out int err, IntPtr pv)
        {
            try
            {
                if (path != "%%TEMP%%")
                {
                    path = Path.Combine(Path.GetTempPath(), path);
                    File.Delete(path);
                }
            }
            catch (IOException)
            {
            }
            err = 0;
            return 1;
        }

        private int CabFilePlaced(IntPtr pccab, string filePath, long fileSize, int continuation, IntPtr pv)
        {
            return 0;
        }

        private int CabGetNextCabinet(IntPtr pccab, uint prevCabSize, IntPtr pv)
        {
            NativeMethods.FCI.CCAB structure = new NativeMethods.FCI.CCAB();
            Marshal.PtrToStructure(pccab, structure);
            structure.szDisk = string.Empty;
            structure.szCab = this.context.GetCabinetName(structure.iCab);
            base.CabNumbers[structure.szCab] = (short) structure.iCab;
            base.NextCabinetName = structure.szCab;
            Marshal.StructureToPtr(structure, pccab, false);
            return 1;
        }

        private int CabGetOpenInfo(string path, out short date, out short time, out short attribs, out int err, IntPtr pv)
        {
            NativeMethods.FCI.DateTimeToCabDateAndTime(this.fileLastWriteTime, out date, out time);
            attribs = (short) this.fileAttributes;
            Stream fileStream = base.FileStream;
            base.FileStream = new DuplicateStream(fileStream);
            int num = base.StreamHandles.AllocHandle(fileStream);
            err = 0;
            return num;
        }

        private int CabGetTempFile(IntPtr tempNamePtr, int tempNameSize, IntPtr pv)
        {
            string fileName;
            if (this.dontUseTempFiles)
            {
                fileName = "%%TEMP%%";
            }
            else
            {
                fileName = Path.GetFileName(Path.GetTempFileName());
            }
            byte[] bytes = Encoding.ASCII.GetBytes(fileName);
            if (bytes.Length >= tempNameSize)
            {
                return -1;
            }
            Marshal.Copy(bytes, 0, tempNamePtr, bytes.Length);
            Marshal.WriteByte(tempNamePtr, bytes.Length, 0);
            return 1;
        }

        internal override int CabOpenStreamEx(string path, int openFlags, int shareMode, out int err, IntPtr pv)
        {
            if (base.CabNumbers.ContainsKey(path))
            {
                if (base.CabStream == null)
                {
                    int cabinetNumber = base.CabNumbers[path];
                    if (base.ProgressData.CurrentFolderTotalBytes > 0L)
                    {
                        base.ProgressData.CurrentFolderBytesProcessed = base.ProgressData.CurrentFolderTotalBytes;
                        base.ProgressData.ProgressType = CabinetProgressType.FinishFolder;
                        base.OnProgress();
                        base.ProgressData.CurrentFolderBytesProcessed = base.ProgressData.CurrentFolderTotalBytes = 0L;
                    }
                    Stream stream = this.context.OpenCabinetWriteStream(cabinetNumber, path);
                    if (stream == null)
                    {
                        throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, "Cabinet {0} not provided.", new object[] { cabinetNumber }));
                    }
                    base.ProgressData.CurrentCabinetName = path;
                    base.ProgressData.CurrentCabinetTotalBytes = Math.Min(this.totalFolderBytesProcessedInCurrentCab, (long) this.maxCabBytes);
                    base.ProgressData.CurrentCabinetBytesProcessed = 0L;
                    base.ProgressData.ProgressType = CabinetProgressType.StartCab;
                    base.OnProgress();
                    base.CabStream = stream;
                }
                path = "%%CAB%%";
            }
            else
            {
                if (path == "%%TEMP%%")
                {
                    Stream item = new MemoryStream();
                    this.tempStreams.Add(item);
                    int num2 = base.StreamHandles.AllocHandle(item);
                    err = 0;
                    return num2;
                }
                if (path != "%%CAB%%")
                {
                    path = Path.Combine(Path.GetTempPath(), path);
                    Stream stream3 = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
                    this.tempStreams.Add(stream3);
                    stream3 = new DuplicateStream(stream3);
                    int num3 = base.StreamHandles.AllocHandle(stream3);
                    err = 0;
                    return num3;
                }
            }
            return base.CabOpenStreamEx(path, openFlags, shareMode, out err, pv);
        }

        internal override int CabWriteStreamEx(int streamHandle, IntPtr memory, int cb, out int err, IntPtr pv)
        {
            int num = base.CabWriteStreamEx(streamHandle, memory, cb, out err, pv);
            if ((num > 0) && (err == 0))
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

        private void CheckError()
        {
            if (base.Erf.Error)
            {
                throw new CabinetException(base.Erf.Oper, base.Erf.Type, CabinetException.GetErrorMessage(base.Erf.Oper, base.Erf.Type, false));
            }
        }

        public void Create(ICabinetCreateStreamContext streamContext, IList<string> files)
        {
            if (streamContext == null)
            {
                throw new ArgumentNullException("streamContext");
            }
            if (files == null)
            {
                throw new ArgumentNullException("files");
            }
            this.Create(streamContext, new IList<string>[] { files }, 0L, 0L);
        }

        public void Create(ICabinetCreateStreamContext streamContext, IList<string>[] foldersAndFiles, long maxCabSize, long maxFolderSize)
        {
            if (streamContext == null)
            {
                throw new ArgumentNullException("streamContext");
            }
            if (foldersAndFiles == null)
            {
                throw new ArgumentNullException("foldersAndFiles");
            }
            lock (this.innerLock)
            {
                try
                {
                    this.context = streamContext;
                    NativeMethods.FCI.CCAB pccab = new NativeMethods.FCI.CCAB();
                    if ((maxCabSize > 0L) && (maxCabSize < pccab.cb))
                    {
                        pccab.cb = Math.Max(0x8000, (int) maxCabSize);
                    }
                    if ((maxFolderSize > 0L) && (maxFolderSize < pccab.cbFolderThresh))
                    {
                        pccab.cbFolderThresh = (int) maxFolderSize;
                    }
                    this.maxCabBytes = pccab.cb;
                    pccab.szCab = this.context.GetCabinetName(0);
                    if (pccab.szCab == null)
                    {
                        throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, "Cabinet {0} not provided.", new object[] { 0 }));
                    }
                    pccab.setID = (short) new Random().Next(-32768, 0x8000);
                    base.CabNumbers[pccab.szCab] = 0;
                    base.ProgressData.CurrentCabinetName = pccab.szCab;
                    base.ProgressData.TotalCabinets = 1;
                    base.CabStream = null;
                    base.Erf.Clear();
                    this.fciHandle = NativeMethods.FCI.Create(base.ErfHandle.AddrOfPinnedObject(), this.fciFilePlacedHandler, this.fciAllocMemHandler, this.fciFreeMemHandler, this.fciOpenStreamHandler, this.fciReadStreamHandler, this.fciWriteStreamHandler, this.fciCloseStreamHandler, this.fciSeekStreamHandler, this.fciDeleteFileHandler, this.fciGetTempFileHandler, pccab, IntPtr.Zero);
                    this.CheckError();
                    using (this.fciHandle)
                    {
                        for (int i = 0; i < foldersAndFiles.Length; i++)
                        {
                            IList<string> list = foldersAndFiles[i];
                            for (int k = 0; k < list.Count; k++)
                            {
                                FileAttributes attributes;
                                DateTime time;
                                Stream stream = this.context.OpenFileReadStream(list[k], out attributes, out time);
                                if (stream != null)
                                {
                                    CabinetProgressEventArgs progressData = base.ProgressData;
                                    progressData.TotalFileBytes += stream.Length;
                                    CabinetProgressEventArgs args2 = base.ProgressData;
                                    args2.TotalFiles++;
                                    this.context.CloseFileReadStream(list[k], stream);
                                }
                            }
                        }
                        for (int j = 0; j < foldersAndFiles.Length; j++)
                        {
                            IList<string> list2 = foldersAndFiles[j];
                            for (int m = 0; m < list2.Count; m++)
                            {
                                FileAttributes attributes2;
                                DateTime time2;
                                Stream stream2 = this.context.OpenFileReadStream(list2[m], out attributes2, out time2);
                                if (stream2 != null)
                                {
                                    if (base.ProgressData.CurrentFolderTotalBytes > 0L)
                                    {
                                        base.ProgressData.CurrentFolderBytesProcessed = base.ProgressData.CurrentFolderTotalBytes;
                                        base.ProgressData.ProgressType = CabinetProgressType.FinishFolder;
                                        base.OnProgress();
                                        base.ProgressData.CurrentFolderBytesProcessed = 0L;
                                        base.ProgressData.CurrentFolderTotalBytes = 0L;
                                        if ((j != 0) || (m != 0))
                                        {
                                            CabinetProgressEventArgs args3 = base.ProgressData;
                                            args3.CurrentFolderNumber++;
                                        }
                                    }
                                    base.ProgressData.CurrentFileName = list2[m];
                                    if ((j != 0) || (m != 0))
                                    {
                                        CabinetProgressEventArgs args4 = base.ProgressData;
                                        args4.CurrentFileNumber++;
                                    }
                                    base.ProgressData.CurrentFileTotalBytes = stream2.Length;
                                    base.ProgressData.CurrentFileBytesProcessed = 0L;
                                    base.ProgressData.ProgressType = CabinetProgressType.StartFile;
                                    base.OnProgress();
                                    this.AddFile(list2[m], stream2, attributes2, time2, false, this.compressionLevel);
                                }
                            }
                            this.FlushFolder();
                        }
                        this.FlushCabinet();
                    }
                    this.fciHandle = null;
                }
                finally
                {
                    if (base.CabStream != null)
                    {
                        this.context.CloseCabinetWriteStream(base.ProgressData.CurrentCabinetNumber, base.ProgressData.CurrentCabinetName, base.CabStream);
                        base.CabStream = null;
                    }
                    if (base.FileStream != null)
                    {
                        this.context.CloseFileReadStream(base.ProgressData.CurrentFileName, base.FileStream);
                        base.FileStream = null;
                    }
                    this.context = null;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && (this.fciHandle != null))
                {
                    this.fciHandle.Dispose();
                    this.fciHandle = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void FlushCabinet()
        {
            base.Erf.Clear();
            NativeMethods.FCI.FlushCabinet(this.fciHandle, false, new NativeMethods.FCI.PFNGETNEXTCABINET(this.CabGetNextCabinet), new NativeMethods.FCI.PFNSTATUS(this.CabCreateStatus));
            this.CheckError();
        }

        private void FlushFolder()
        {
            base.Erf.Clear();
            NativeMethods.FCI.FlushFolder(this.fciHandle, new NativeMethods.FCI.PFNGETNEXTCABINET(this.CabGetNextCabinet), new NativeMethods.FCI.PFNSTATUS(this.CabCreateStatus));
            this.CheckError();
        }

        private static NativeMethods.FCI.TCOMP GetCompressionType(CabinetCompressionLevel compLevel)
        {
            if (compLevel == CabinetCompressionLevel.MsZip)
            {
                return NativeMethods.FCI.TCOMP.TYPE_MSZIP;
            }
            if (compLevel <= CabinetCompressionLevel.None)
            {
                return NativeMethods.FCI.TCOMP.TYPE_NONE;
            }
            if (compLevel > CabinetCompressionLevel.Max)
            {
                compLevel = CabinetCompressionLevel.Max;
            }
            return (NativeMethods.FCI.TCOMP) ((ushort) (3 | (0xf00 + (((int) (compLevel - 1)) << 8))));
        }

        public CabinetCompressionLevel CompressionLevel
        {
            get
            {
                return this.compressionLevel;
            }
            set
            {
                this.compressionLevel = value;
            }
        }

        public bool UseTempFiles
        {
            get
            {
                return !this.dontUseTempFiles;
            }
            set
            {
                this.dontUseTempFiles = !value;
            }
        }
    }
}

