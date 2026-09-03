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
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using AvePoint.GCommon.Utility.EmRumtime;

    [EmRuntime()]
    internal static class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        internal class ERF
        {
            private int erfOper;
            private int erfType;
            private int fError;
            internal int Oper
            {
                get
                {
                    return this.erfOper;
                }
                set
                {
                    this.erfOper = value;
                }
            }
            internal int Type
            {
                get
                {
                    return this.erfType;
                }
                set
                {
                    this.erfType = value;
                }
            }
            internal bool Error
            {
                get
                {
                    return (this.fError != 0);
                }
                set
                {
                    this.fError = value ? 1 : 0;
                }
            }
            internal void Clear()
            {
                this.Oper = 0;
                this.Type = 0;
                this.Error = false;
            }
        }

        internal static class FCI
        {
            internal const int CPU_80386 = 1;
            internal const int MAX_CAB_PATH = 0x100;
            internal const int MAX_CABINET_NAME = 0x100;
            internal const int MAX_CHUNK = 0x8000;
            internal const int MAX_DISK = 0x7fffffff;
            internal const int MAX_DISK_NAME = 0x100;
            internal const int MAX_FILENAME = 0x100;
            internal const int MIN_DISK = 0x8000;

            [DllImport("cabinet.dll", EntryPoint="FCIAddFile", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
            internal static extern int AddFile(Handle hfci, string pszSourceFile, IntPtr pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fExecute, PFNGETNEXTCABINET pfnfcignc, PFNSTATUS pfnfcis, PFNGETOPENINFO pfnfcigoi, TCOMP typeCompress);
            [DllImport("cabinet.dll", EntryPoint="FCICreate", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
            internal static extern Handle Create(IntPtr perf, PFNFILEPLACED pfnfcifp, PFNALLOC pfna, PFNFREE pfnf, PFNOPEN pfnopen, PFNREAD pfnread, PFNWRITE pfnwrite, PFNCLOSE pfnclose, PFNSEEK pfnseek, PFNDELETE pfndelete, PFNGETTEMPFILE pfnfcigtf, [MarshalAs(UnmanagedType.LPStruct)] CCAB pccab, IntPtr pv);
            internal static void DateTimeToCabDateAndTime(DateTime dateTime, out short cabDate, out short cabTime)
            {
                long fileTime = dateTime.ToLocalTime().ToFileTime();
                FileTimeToDosDateTime(ref fileTime, out cabDate, out cabTime);
            }

            [return: MarshalAs(UnmanagedType.Bool)]
            [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("cabinet.dll", EntryPoint="FCIDestroy", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
            internal static extern bool Destroy(IntPtr hfci);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("kernel32.dll", SetLastError=true)]
            internal static extern bool FileTimeToDosDateTime(ref long fileTime, out short wFatDate, out short wFatTime);
            [DllImport("cabinet.dll", EntryPoint="FCIFlushCabinet", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
            internal static extern int FlushCabinet(Handle hfci, [MarshalAs(UnmanagedType.Bool)] bool fGetNextCab, PFNGETNEXTCABINET pfnfcignc, PFNSTATUS pfnfcis);
            [DllImport("cabinet.dll", EntryPoint="FCIFlushFolder", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
            internal static extern int FlushFolder(Handle hfci, PFNGETNEXTCABINET pfnfcignc, PFNSTATUS pfnfcis);

            [StructLayout(LayoutKind.Sequential)]
            internal class CCAB
            {
                internal int cb = 0x7fffffff;
                internal int cbFolderThresh = 0x7fffffff;
                internal int cbReserveCFHeader;
                internal int cbReserveCFFolder;
                internal int cbReserveCFData;
                internal int iCab;
                internal int iDisk;
                internal int fFailOnIncompressible;
                internal short setID;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x100)]
                internal string szDisk = string.Empty;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x100)]
                internal string szCab = string.Empty;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x100)]
                internal string szCabPath = string.Empty;
            }

            internal enum ERROR
            {
                NONE,
                OPEN_SRC,
                READ_SRC,
                ALLOC_FAIL,
                TEMP_FILE,
                BAD_COMPR_TYPE,
                CAB_FILE,
                USER_ABORT,
                MCI_FAIL
            }

            internal class Handle : SafeHandle
            {
                internal Handle() : base(IntPtr.Zero, true)
                {
                }

                [SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
                protected override bool ReleaseHandle()
                {
                    return NativeMethods.FCI.Destroy(base.handle);
                }

                public override bool IsInvalid
                {
                    get
                    {
                        return (base.handle == IntPtr.Zero);
                    }
                }
            }

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr PFNALLOC(int cb);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate int PFNCLOSE(int fileHandle, out int err, IntPtr pv);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate int PFNDELETE(string path, out int err, IntPtr pv);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate int PFNFILEPLACED(IntPtr pccab, string path, long fileSize, int continuation, IntPtr pv);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void PFNFREE(IntPtr pv);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate int PFNGETNEXTCABINET(IntPtr pccab, uint cbPrevCab, IntPtr pv);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate int PFNGETOPENINFO(string path, out short date, out short time, out short pattribs, out int err, IntPtr pv);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate int PFNGETTEMPFILE(IntPtr tempNamePtr, int tempNameSize, IntPtr pv);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate int PFNOPEN(string path, int oflag, int pmode, out int err, IntPtr pv);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate int PFNREAD(int fileHandle, IntPtr memory, int cb, out int err, IntPtr pv);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate int PFNSEEK(int fileHandle, int dist, int seekType, out int err, IntPtr pv);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate int PFNSTATUS(NativeMethods.FCI.STATUS typeStatus, uint cb1, uint cb2, IntPtr pv);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate int PFNWRITE(int fileHandle, IntPtr memory, int cb, out int err, IntPtr pv);

            internal enum STATUS : uint
            {
                CABINET = 2,
                FILE = 0,
                FOLDER = 1
            }

            internal enum TCOMP : ushort
            {
                BAD = 15,
                LZX_WINDOW_HI = 0x1500,
                LZX_WINDOW_LO = 0xf00,
                MASK_LZX_WINDOW = 0x1f00,
                MASK_QUANTUM_LEVEL = 240,
                MASK_QUANTUM_MEM = 0x1f00,
                MASK_RESERVED = 0xe000,
                MASK_TYPE = 15,
                QUANTUM_LEVEL_HI = 0x70,
                QUANTUM_LEVEL_LO = 0x10,
                QUANTUM_MEM_HI = 0x1500,
                QUANTUM_MEM_LO = 0xa00,
                SHIFT_LZX_WINDOW = 8,
                SHIFT_QUANTUM_LEVEL = 4,
                SHIFT_QUANTUM_MEM = 8,
                TYPE_LZX = 3,
                TYPE_MSZIP = 1,
                TYPE_NONE = 0,
                TYPE_QUANTUM = 2
            }
        }

        internal static class FDI
        {
            internal const int CPU_80386 = 1;
            internal const int MAX_CAB_PATH = 0x100;
            internal const int MAX_CABINET_NAME = 0x100;
            internal const int MAX_CHUNK = 0x8000;
            internal const int MAX_DISK = 0x7fffffff;
            internal const int MAX_DISK_NAME = 0x100;
            internal const int MAX_FILENAME = 0x100;

            internal static void CabDateAndTimeToDateTime(short cabDate, short cabTime, out DateTime dateTime)
            {
                if ((cabDate == 0) && (cabTime == 0))
                {
                    dateTime = DateTime.MinValue;
                }
                else
                {
                    long num;
                    DosDateTimeToFileTime(cabDate, cabTime, out num);
                    dateTime = DateTime.FromFileTime(num);
                }
            }

            [DllImport("cabinet.dll", EntryPoint="FDICopy", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
            internal static extern int Copy(Handle hfdi, string pszCabinet, string pszCabPath, int flags, PFNNOTIFY pfnfdin, IntPtr pfnfdid, IntPtr pvUser);
            [DllImport("cabinet.dll", EntryPoint="FDICreate", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
            internal static extern Handle Create([MarshalAs(UnmanagedType.FunctionPtr)] PFNALLOC pfnalloc, [MarshalAs(UnmanagedType.FunctionPtr)] PFNFREE pfnfree, PFNOPEN pfnopen, PFNREAD pfnread, PFNWRITE pfnwrite, PFNCLOSE pfnclose, PFNSEEK pfnseek, int cpuType, IntPtr perf);
            [return: MarshalAs(UnmanagedType.Bool)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SuppressUnmanagedCodeSecurity, DllImport("cabinet.dll", EntryPoint="FDIDestroy", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
            internal static extern bool Destroy(IntPtr hfdi);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("kernel32.dll", SetLastError=true)]
            internal static extern bool DosDateTimeToFileTime(short wFatDate, short wFatTime, out long fileTime);
            [DllImport("cabinet.dll", EntryPoint="FDIIsCabinet", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
            internal static extern int IsCabinet(Handle hfdi, IntPtr hf, out CABINFO pfdici);

            [StructLayout(LayoutKind.Sequential)]
            internal struct CABINFO
            {
                internal int cbCabinet;
                internal short cFolders;
                internal short cFiles;
                internal short setID;
                internal short iCabinet;
                internal int fReserve;
                internal int hasprev;
                internal int hasnext;
            }

            internal enum ERROR
            {
                NONE,
                CABINET_NOT_FOUND,
                NOT_A_CABINET,
                UNKNOWN_CABINET_VERSION,
                CORRUPT_CABINET,
                ALLOC_FAIL,
                BAD_COMPR_TYPE,
                MDI_FAIL,
                TARGET_FILE,
                RESERVE_MISMATCH,
                WRONG_CABINET,
                USER_ABORT
            }

            internal class Handle : SafeHandle
            {
                internal Handle() : base(IntPtr.Zero, true)
                {
                }

                protected override bool ReleaseHandle()
                {
                    return NativeMethods.FDI.Destroy(base.handle);
                }

                public override bool IsInvalid
                {
                    get
                    {
                        return (base.handle == IntPtr.Zero);
                    }
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            internal class NOTIFICATION
            {
                internal int cb;
                internal IntPtr psz1;
                internal IntPtr psz2;
                internal IntPtr psz3;
                internal IntPtr pv;
                internal IntPtr hf_ptr;
                internal short date;
                internal short time;
                internal short attribs;
                internal short setID;
                internal short iCabinet;
                internal short iFolder;
                internal int fdie;
                internal int hf
                {
                    get
                    {
                        return (int) this.hf_ptr;
                    }
                }
            }

            internal enum NOTIFICATIONTYPE
            {
                CABINET_INFO,
                PARTIAL_FILE,
                COPY_FILE,
                CLOSE_FILE_INFO,
                NEXT_CABINET,
                ENUMERATE
            }

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr PFNALLOC(int cb);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate int PFNCLOSE(int hf);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void PFNFREE(IntPtr pv);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate int PFNNOTIFY(NativeMethods.FDI.NOTIFICATIONTYPE fdint, NativeMethods.FDI.NOTIFICATION fdin);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate int PFNOPEN(string path, int oflag, int pmode);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate int PFNREAD(int hf, IntPtr pv, int cb);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate int PFNSEEK(int hf, int dist, int seektype);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate int PFNWRITE(int hf, IntPtr pv, int cb);
        }
    }
}

