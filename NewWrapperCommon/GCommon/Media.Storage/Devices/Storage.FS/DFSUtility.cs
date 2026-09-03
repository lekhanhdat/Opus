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
using System.Runtime.InteropServices;
using System.Text;
using AvePoint.GCommon;
using AvePoint.Media.Storage.Util;

namespace AvePoint.Media.Storage.FS
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    unsafe internal struct DFS_INFO_3
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        private string entryPath;
        [MarshalAs(UnmanagedType.LPWStr)]
        private string comment;
        private UInt32 state;
        private UInt32 numberOfStorages;
        private IntPtr storages;

        public List<DFS_STORAGE_INFO> GetDFSTargets()
        {
            List<DFS_STORAGE_INFO> rets = new List<DFS_STORAGE_INFO>();
            Int64 ptr = storages.ToInt64();
            for (int i = 0; i < numberOfStorages; i++)
            {
                unsafe
                {

                    DFS_STORAGE_INFO info = (DFS_STORAGE_INFO)Marshal.PtrToStructure((IntPtr)ptr, typeof(DFS_STORAGE_INFO));
                    rets.Add(info);
                    ptr += Marshal.SizeOf(typeof(DFS_STORAGE_INFO));
                }
            }
            return rets;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    unsafe internal struct DFS_INFO_200
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        private string ftDfsName;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    unsafe internal struct DFS_INFO_300
    {
        private UInt32 flags;
        [MarshalAs(UnmanagedType.LPWStr)]
        private string dfsName;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    unsafe internal struct DFS_STORAGE_INFO
    {
        private UInt64 state;
        public UInt64 State { get { return state; } }
        [MarshalAs(UnmanagedType.LPWStr)]
        private string serverName;
        public string ServerName { get { return serverName; } }
        [MarshalAs(UnmanagedType.LPWStr)]
        private string shareName;
        public string ShareName { get { return shareName; } }
    }

    enum DFSENUMLEVEL
    {
        EnumDomain = 200,
        EnumServer = 300,
        EnumRoot = 3
    }

    class DFSUtility
    {
        private const int ERROR_SUCCESS = 0;
        private const int ERROR_NO_MORE_ITEMS = 259;
        private static StorageLogger logger = StorageLogger.GetInstance(typeof(DFSUtility));

        [DllImport("Netapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int NetDfsEnum([MarshalAs(UnmanagedType.LPWStr)]string dfsName, int level, int prefMaxLen, out IntPtr buffer, [MarshalAs(UnmanagedType.I4)]out int entriesRead, [MarshalAs(UnmanagedType.I4)]ref int resumeHandle);
        public static List<DFS_STORAGE_INFO> EnumDFS(string dfsName, DFSENUMLEVEL level)
        {
            List<DFS_STORAGE_INFO> rets = new List<DFS_STORAGE_INFO>();
            unsafe
            {
                IntPtr oBufferRef;
                int oEntriesRead;
                int ioResumeHandle = 0;
                int ret;
                ret = NetDfsEnum(dfsName, (int)level, -1, out oBufferRef, out oEntriesRead, ref ioResumeHandle);
                while (ret == ERROR_SUCCESS)
                {
                    switch (level)
                    {
                        case DFSENUMLEVEL.EnumRoot:
                            Int64 ptr = oBufferRef.ToInt64();
                            for (int i = 0; i < oEntriesRead; i++)
                            {
                                DFS_INFO_3 di = (DFS_INFO_3)Marshal.PtrToStructure((IntPtr)ptr, typeof(DFS_INFO_3));
                                rets.AddRange(di.GetDFSTargets());
                                ptr += Marshal.SizeOf(typeof(DFS_INFO_3));
                            }
                            break;
                        case DFSENUMLEVEL.EnumServer:
                            break;
                        case DFSENUMLEVEL.EnumDomain:
                            break;
                    }
                    ret = NetDfsEnum(dfsName, (int)level, -1, out oBufferRef, out oEntriesRead, ref ioResumeHandle);
                }
                if (ret != ERROR_NO_MORE_ITEMS)
                {
                    logger.Error("Enum DFS Targets Error, Error Code :{0} ", ret);
                }
            }
            return rets;
        }
    }
}
