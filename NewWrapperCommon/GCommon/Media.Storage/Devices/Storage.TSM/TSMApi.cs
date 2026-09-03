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

namespace AvePoint.Media.Storage.TSM
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.InteropServices;
    using AvePoint.Media.Storage.Util;
    #endregion

    class TSMApi
    {
        const string tsmDllName = TSMConst.tsmApiFullName;

        #region Extern Methods

        [DllImport("kernel32")]
        public static extern IntPtr LoadLibrary(string throwEx);

        [DllImport(tsmDllName, EntryPoint = "getInstance")]
        public static extern IntPtr GetInstance(ThrowException throwEx);

        [DllImport(tsmDllName, EntryPoint = "releaseInstance")]
        public static extern void ReleaseInstance(IntPtr instance);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="dsmiDir">A fully-qualified directory path that contains a message file onUNIX or Linux. 
        ///                       It also specifies the dsmtca and the dsm.sys directories.</param>
        /// <param name="dsmiLog">The fully-qualified path of the error log directory.</param>
        /// <param name="logName">The file name for an error log if the application does not use dsierror.log.</param>
        /// <param name="configFile">The fully-qualified name of the client options file.</param>
        /// <param name="fileSpace"></param>
        /// <param name="capacity"></param>
        /// <param name="occupancy"></param>
        /// <param name="sizeEstimate"></param>
        /// <returns></returns>
        //[DllImport(tsmDllName, EntryPoint = "setUp")]
        //public static extern void SetUp(IntPtr instance, uint handle, string dsmiDir, string dsmiLog, string logName, string configFile, string fileSpace, long capacity, long occupancy, long sizeEstimate);

        [DllImport(tsmDllName, EntryPoint = "cleanUp")]
        public static extern string CleanUp(IntPtr instance);

        [DllImport(tsmDllName, EntryPoint = "openSession")]
        public static extern uint OpenSession(IntPtr instance, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string configFile, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string fileSpace, long capacity, long occupancy, long sizeEstimate, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string password);

        [DllImport(tsmDllName, EntryPoint = "closeSession")]
        public static extern void CloseSession(IntPtr instance, uint handle);

        [DllImport(tsmDllName, EntryPoint = "beginWrite")]
        public static extern void BeginWrite(IntPtr instance, uint handle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string hl, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string ll, DSMObjType objType);

        [DllImport(tsmDllName, EntryPoint = "write")]
        public static extern void Write(IntPtr instance, uint handle, byte[] buffer, int offset, int len);

        [DllImport(tsmDllName, EntryPoint = "endWrite")]
        public static extern void EndWrite(IntPtr instance, uint handle);

        [DllImport(tsmDllName, EntryPoint = "beginRead")]
        public static extern void BeginRead(IntPtr instance, uint handle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string hl, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string ll, long offset, long partlen);

        [DllImport(tsmDllName, EntryPoint = "read")]
        public static extern int Read(IntPtr instance, uint handle, byte[] buffer, int offset, int len);

        [DllImport(tsmDllName, EntryPoint = "endRead")]
        public static extern void EndRead(IntPtr instance, uint handle);

        [DllImport(tsmDllName, EntryPoint = "logEvent")]
        public static extern void LogEvent(IntPtr instance, uint handle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string message, DSMLogLevel level, DSMLogType type);

        [return: MarshalAs(UnmanagedType.I1)]
        [DllImport(tsmDllName, EntryPoint = "isOpen")]
        public static extern bool IsOpen(IntPtr instance);

        //[return: MarshalAs(UnmanagedType.I1)]
        //[DllImport(tsmDllName, EntryPoint = "isSessionOpen")]
        //public static extern bool IsSessionOpen(IntPtr instance);

        [DllImport(tsmDllName, EntryPoint = "deleteObjects")]
        public static extern long DeleteObjects(IntPtr instance, uint handle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string hl, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string ll, DSMObjType objType);

        [DllImport(tsmDllName, EntryPoint = "verifyMC")]
        public static extern void VerifyMC(IntPtr instance, uint handle, string mc);

        [return: MarshalAs(UnmanagedType.I1)]
        [DllImport(tsmDllName, EntryPoint = "checkObject", ThrowOnUnmappableChar=false)]
        public static extern bool CheckObject(IntPtr instance, uint handle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))] string highName, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))] string lowName, DSMObjType objType);

        [DllImport(tsmDllName, EntryPoint = "verifyUserDeletePermission")]
        public static extern int VerifyUserDeletePermission(IntPtr instance, uint handle);

        //for Multiple node
        [DllImport(tsmDllName, EntryPoint = "setUp")]
        public static extern void SetUp(IntPtr instance, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string dsmiDir, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string dsmiLog, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string logName, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string configFile);

        //[DllImport(tsmDllName, EntryPoint = "setUpNode")]
        //public static extern void SetUpNode(IntPtr instance, string configFile, string fileSpace, long capacity, long occupancy, long sizeEstimate);

        [return : MarshalAs(UnmanagedType.U4)]
        [DllImport(tsmDllName, EntryPoint = "getLength")]
        public static extern uint GetLength(IntPtr instance, uint handle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string hl, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string ll);

        [DllImport(tsmDllName, EntryPoint = "getObjectNameSize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetObjectNameSize(IntPtr dsm, uint handle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string hl, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string ll);

        //LOAD HLName
        [DllImport(tsmDllName, EntryPoint = "getObjectNameSizeForTool", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getObjectNameSizeForTool(IntPtr dsm, uint handle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string hl, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string ll);

        [DllImport(tsmDllName, EntryPoint = "getObjectNames", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetObjectNames(IntPtr dsm, uint handle, StringBuilder objName, int length);

        [DllImport(tsmDllName, EntryPoint = "listItems", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToDSMObjectList))]
        public static extern DSMObjectItem[] ListItems(IntPtr dsm, uint handle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string hl, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string ll, DSMObjType objType, DSMObjState objState);

        [DllImport(tsmDllName, EntryPoint = "getObjectNameSizeWithDate", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetObjectNameSizeWithDate(IntPtr dsm, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string hl, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string ll, int year, int mon, int day);

        [DllImport(tsmDllName, EntryPoint = "getObjectNameSizeWithDate", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetObjectNameSizeWithDate(IntPtr dsm, uint handle, string hl, string ll, int year, int mon, int day);
        #endregion
    }

    enum DSMObjType
    {
        DSM_FILE = 0,//DSM_OBJ_FILE
        DSM_DIRECTORY = 1,//DSM_OBJ_DIRECTORY
        DSM_ANY = 2//DSM_OBJ_WILDCARD
    }

    enum DSMObjState
    {
        DSM_STATE_ACTIVE = 0,//DSM_ACTIVE
        DSM_STATE_INACTIVE = 1,//DSM_INACTIVE
        DSM_STATE_ANY=2//DSM_ANY_MATCH
    }

    

    enum DSMLogLevel
    {
        DSM_LOG_LEVEL_INFO,
        DSM_LOG_LEVEL_WARN,
        DSM_LOG_LEVEL_ERROR,
        DSM_LOG_LEVEL_SEVERE
    };

    enum DSMLogType
    {
        DSM_LOG_TYPE_SERVER,
        DSM_LOG_TYPE_LOCAL,
        DSM_LOG_TYPE_BOTH
    };

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate void ThrowException([MarshalAs(UnmanagedType.LPStr)]string str);
}
