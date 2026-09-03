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



namespace AvePoint.Media.Storage.Centera
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.InteropServices;
    using AvePoint.Media.Storage.Util;
    using AvePoint.GCommon;
    using System.Diagnostics;
    #endregion

    class FPConst
    {
        public const string fpDllName = CenteraConst.CENTERA_API_FILE_FULLNAME;
        public const int fpStringBufferSize = 256;
    }

    #region Pool Functions

    class Pool
    {
        #region Import Pool functions
        [DllImport(FPConst.fpDllName)]
        private static extern void FPPool_Close(ulong poolRef);

        [DllImport(FPConst.fpDllName)]
        private static extern int FPPool_GetLastError();

        [DllImport(FPConst.fpDllName)]
        private static extern void FPPool_GetLastErrorInfo(IntPtr ptr);

        [DllImport(FPConst.fpDllName)]
        private static extern void FPPool_SetGlobalOption8([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string optionName, int optionValue);

        [DllImport(FPConst.fpDllName)]
        private static extern void FPPool_SetIntOption8(ulong poolRef, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string optionName, int optionValue);

        [DllImport(FPConst.fpDllName)]
        private static extern int FPPool_GetIntOption8(ulong poolRef, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string optionName);

        [DllImport(FPConst.fpDllName)]
        private static extern void FPPool_RegisterApplication8([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))] string appName, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))] string version);

        [DllImport(FPConst.fpDllName)]
        private static extern ulong FPPool_Open8([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))] string poolAddress);
        [DllImport(FPConst.fpDllName)]
        public static extern void FPPool_GetCapability(ulong inPool, System.String inCapabilityName, System.String inCapabilityAttributeName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] tagName, ref int tagNameLen);
        [DllImport(FPConst.fpDllName, EntryPoint = "_FPPool_GetPoolInfo")]
        private static extern void FPPool_GetPoolInfo(ulong poolRef, ref FPPoolInfo poolInfo);
        #endregion



        #region Wrapper Pool Functions
        public static string GetPoolPermisson(ulong poolRef, string CapabilityName, string inCapabilityAttributeName)
        {
            byte[] outString;
            int bufSize = 0;
            int len = 0;

            do
            {
                bufSize += 256;
                len = bufSize;
                outString = new byte[(int)bufSize];

                FPPool_GetCapability(poolRef, CapabilityName, inCapabilityAttributeName, outString, ref len);
            } while (len > bufSize);

            Pool.ThrowIfError();

            return Encoding.UTF8.GetString(outString, 0, (int)len - 1);
        }
        public static FPPoolInfo GetPoolInfo(ulong poolRef)
        {
            FPPoolInfo poolInfo = new FPPoolInfo();
            poolInfo.PoolInfoVersion = 2;
            FPPool_GetPoolInfo(poolRef, ref poolInfo);
            Pool.ThrowIfError();
            return poolInfo;
        }

        public static void RegisterApplication(string appName, string appVersion)
        {
            FPPool_RegisterApplication8(appName, appVersion);
            ThrowIfError();
        }

        public static void SetGlobalOption(FPOption option, int value)
        {
            FPPool_SetGlobalOption8(option.ToString(), value);
            ThrowIfError();
        }

        public static void SetOption(ulong poolRef, FPOption option, int optionValue)
        {
            FPPool_SetIntOption8(poolRef, option.ToString(), optionValue);
            ThrowIfError();
        }

        public static int GetOption(ulong poolRef, FPOption option)
        {
            int value = FPPool_GetIntOption8(poolRef, option.ToString());
            ThrowIfError();
            return value;
        }

        private static AveLogger logger = AveLogger.GetInstance(typeof(Pool));

        public static ulong Open(string poolAdress)
        {
            try
            {
                string centeraDllPath = PathUtil.CombinePath(ExecutorContext.BinDirectory, @"storage\centera\api\");
                bool rs = Win32API.SetDllDirectory(centeraDllPath);
                if (!rs)
                {
                    logger.Warn("SetDllDirectory Failed. Path:" + centeraDllPath);
                }
            }
            catch (Exception t)
            {
                logger.Error(t.Message, t);
            }
            ulong poolRef = FPPool_Open8(poolAdress);
            ThrowIfError();
            return poolRef;
        }

        public static void Close(ulong poolRef)
        {
            FPPool_Close(poolRef);
            ThrowIfError();
        }

        private static string FilterTraceMessage(string str)
        {
            char[] chars = str.ToCharArray();
            int secret = str.IndexOf("secret", StringComparison.OrdinalIgnoreCase);
            if (secret == -1)
            {
                return str;
            }
            int fppool = str.IndexOf("FPPool", StringComparison.OrdinalIgnoreCase);
            int max = 0;
            try
            {
                while (max < 100)
                {
                    max++;
                    if (secret < fppool)
                    {
                        for (int i = secret + 7; i < fppool; i++)
                        {
                            if (chars[i] == ')' && i + 5 > fppool)
                            {
                                break;
                            }
                            chars[i] = '*';
                        }
                        secret = str.IndexOf("secret", secret + 1, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        fppool = str.IndexOf("FPPool", fppool + 1, StringComparison.OrdinalIgnoreCase);
                        if (fppool == -1)
                        {
                            fppool = str.Length;
                            max = 99;
                        }
                    }
                }
                return new string(chars);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                return str;
            }
        }

        public static void ThrowIfError()
        {
            int errorCode = FPPool_GetLastError();
            string errorInformation = string.Empty;
            Boolean known = false;
            if (0 != errorCode)
            {
                FPErrorInfo err = new FPErrorInfo();
                unsafe
                {
                    IntPtr ptr = (IntPtr)Marshal.AllocHGlobal((int)1024).ToPointer();
                    FPPool_GetLastErrorInfo(ptr);
                    err = (FPErrorInfo)Marshal.PtrToStructure(ptr, typeof(FPErrorInfo));
                    Marshal.FreeHGlobal(ptr);
                }
                err.Trace = FilterTraceMessage(err.Trace);

                if (err.Error == -10153 && err.SystemError == 4)
                {
                    known = true;
                    errorInformation = "MediaStorage_Centera_No_PEA_file_found";
                }
                else if (err.Error == -10153 && err.SystemError == 8)
                {
                    known = true;
                    errorInformation = "MediaStorage_Centera_Authentication_failed";
                }
                else if (err.Error == -10020)
                {
                    known = true;
                    errorInformation = "MediaStorage_Centera_Cannot_connect_to_any_application_pool";
                }
                else if (err.Error == -10204)
                {
                    known = true;
                    errorInformation = "MediaStorage_Centera_Retention_Failed";
                }
                if (known)
                {
                    throw new FPKnownException(err, errorInformation);
                }
                throw new FPException(err);
            }
        }
        #endregion
    }
    #endregion

    #region Clip Functions
    class Clip
    {
        #region Import Clip Functions
        [DllImport(FPConst.fpDllName)]
        private static extern ulong FPClip_RawOpen(ulong poolRef, string clipId, ulong streamRef, long option);

        [DllImport(FPConst.fpDllName)]
        private static extern ulong FPClip_Open(ulong poolRef, [MarshalAs(UnmanagedType.LPStr)]string clipId, int openMode);

        [DllImport(FPConst.fpDllName)]
        private static extern void FPClip_RawRead(ulong clipRef, ulong streamRef);

        [DllImport(FPConst.fpDllName)]
        private static extern ulong FPClip_Create8(ulong poolRef, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string clipName);

        [DllImport(FPConst.fpDllName)]
        private static extern void FPClip_Write(ulong clipRef, [MarshalAs(UnmanagedType.LPStr)] StringBuilder clipID);

        [DllImport(FPConst.fpDllName)]
        private static extern ulong FPClip_GetTopTag(ulong clipRef);

        [DllImport(FPConst.fpDllName)]
        private static extern ulong FPClip_FetchNext(ulong clipRef);

        [DllImport(FPConst.fpDllName)]
        private static extern void FPClip_Close(ulong clipRef);

        [DllImport(FPConst.fpDllName)]
        private static extern void FPClip_Delete(ulong poolRef, [MarshalAs(UnmanagedType.LPStr)]string clipId);

        [DllImport(FPConst.fpDllName)]
        private static extern int FPClip_Exists(ulong poolRef, [MarshalAs(UnmanagedType.LPStr)]string clipId);

        [DllImport(FPConst.fpDllName)]
        private static extern ulong FPClip_GetTotalSize(ulong clipRef);

        [DllImport(FPConst.fpDllName)]
        public static extern void FPClip_SetDescriptionAttribute8(ulong inClip, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))] String inAttrName, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))] String inAttrValue);

        [DllImport(FPConst.fpDllName)]
        public static extern void FPClip_GetDescriptionAttribute8(ulong inClip, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))] String inAttrName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] outAttrValue, ref int ioAttrValueLen);

        [DllImport(FPConst.fpDllName)]
        public static extern void FPClip_SetRetentionPeriod(ulong FPClipRef, ulong inRetentionSecs);

        [DllImport(FPConst.fpDllName)]
        public static extern ulong FPClip_GetRetentionPeriod(ulong FPClipRef);

        #endregion

        #region Wrapper Clip Function

        public static ulong GetTotalSize(ulong clipRef)
        {
            ulong size = FPClip_GetTotalSize(clipRef);
            Pool.ThrowIfError();
            return size;
        }
        public static void SetRetentionPeriod(ulong clipRef, ulong inRetentionSecs = 0)
        {
            FPClip_SetRetentionPeriod(clipRef, inRetentionSecs);
            Pool.ThrowIfError();
        }

        public static ulong GetRetentionPeriod(ulong clipRef)
        {
            var result = FPClip_GetRetentionPeriod(clipRef);
            Pool.ThrowIfError();
            return result;
        }
        public static ulong Create(ulong poolRef, string clipName)
        {
            ulong clipRef = FPClip_Create8(poolRef, clipName);
            Pool.ThrowIfError();
            return clipRef;
        }
        public static ulong RawOpen(ulong poolRef, string clipId, ulong streamRef, long option)
        {
            ulong clipRef = FPClip_RawOpen(poolRef, clipId, streamRef, option);
            Pool.ThrowIfError();
            return clipRef;
        }

        public static ulong Open(ulong poolRef, string clipId, FPClipOpenMode mode)
        {
            ulong clipRef = FPClip_Open(poolRef, clipId, (int)mode);
            Pool.ThrowIfError();
            return clipRef;
        }

        public static void RawRead(ulong clipRef, ulong streamRef)
        {
            FPClip_RawRead(clipRef, streamRef);
            Pool.ThrowIfError();
        }

        public static ulong FetchNext(ulong clipRef)
        {
            ulong next = FPClip_FetchNext(clipRef);
            Pool.ThrowIfError();
            return next;
        }

        public static ulong GetTopTag(ulong clipRef)
        {
            ulong topTagRef = FPClip_GetTopTag(clipRef);
            Pool.ThrowIfError();
            return topTagRef;
        }


        public static string Write(ulong clipRef)
        {
            StringBuilder outputClipId = new StringBuilder(FPConst.fpStringBufferSize);
            FPClip_Write(clipRef, outputClipId);
            Pool.ThrowIfError();
            return outputClipId.ToString();
        }

        public static void Close(ulong clipRef)
        {
            FPClip_Close(clipRef);
            Pool.ThrowIfError();
        }

        public static void Delete(ulong poolRef, string clipId)
        {
            FPClip_Delete(poolRef, clipId);
            Pool.ThrowIfError();
        }

        public static int Exists(ulong poolRef, string clipId)
        {
            int rs = FPClip_Exists(poolRef, clipId);
            Pool.ThrowIfError();
            return rs;
        }

        public static void SetAttribute(ulong clipRef, string key, string value)
        {
            Clip.FPClip_SetDescriptionAttribute8(clipRef, key, value);
            Pool.ThrowIfError();
        }

        public static string GetAttribute(ulong clipRef, string attrKey)
        {
            byte[] outString;
            int bufSize = 0;
            int len = 0;

            do
            {
                bufSize += FPConst.fpStringBufferSize;
                len = bufSize;
                outString = new byte[(int)bufSize];
                Clip.FPClip_GetDescriptionAttribute8(clipRef, attrKey, outString, ref len);
            } while (len > bufSize);
            Pool.ThrowIfError();
            return Encoding.UTF8.GetString(outString, 0, (int)len - 1);
        }

        #endregion
    }
    #endregion
    #region FPQueryExpression Functions
    class FPQueryExpression
    {
        [DllImport(FPConst.fpDllName)]
        private static extern ulong FPQueryExpression_Create();

        [DllImport(FPConst.fpDllName)]
        private static extern void FPQueryExpression_SetStartTime(ulong inRef, ulong inTime);

        [DllImport(FPConst.fpDllName)]
        private static extern void FPQueryExpression_SetEndTime(ulong inRef, ulong inTime);
        [DllImport(FPConst.fpDllName)]
        private static extern ulong FPPoolQuery_Open(ulong inPoolRef, ulong inQueryExpressionRef);
        [DllImport(FPConst.fpDllName)]
        private static extern ulong FPPoolQuery_FetchResult(ulong inPoolQueryRef, ulong inTimeout);
        public static ulong Create()
        {
            ulong FPQueryExpressionRef = FPQueryExpression_Create();
            Pool.ThrowIfError();
            return FPQueryExpressionRef;
        }



    }
    #endregion


    #region Tag Functions
    class Tag
    {
        #region Tag Import Functions
        [DllImport(FPConst.fpDllName)]
        private static extern ulong FPTag_Create8(ulong parentRef, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))]string tagName);

        [DllImport(FPConst.fpDllName)]
        private static extern void FPTag_Close(ulong tagRef);

        [DllImport(FPConst.fpDllName)]
        private static extern void FPTag_Delete(ulong tagRef);

        [DllImport(FPConst.fpDllName)]
        private static extern long FPTag_GetBlobSize(ulong tagRef);

        [DllImport(FPConst.fpDllName)]
        private static extern ulong FPTag_Copy(ulong srcTagRef, ulong destParentTagRef, int option);

        [DllImport(FPConst.fpDllName)]
        private static extern void FPTag_BlobRead(ulong tagRef, ulong streamRef, long option);

        [DllImport(FPConst.fpDllName)]
        private static extern void FPTag_BlobReadPartial(ulong tagRef, ulong streamRef, long offest, long length, long option);

        [DllImport(FPConst.fpDllName)]
        private static extern void FPTag_BlobWrite(ulong tagRef, ulong streamRef, long option);

        [DllImport(FPConst.fpDllName)]
        private static extern void FPTag_BlobWritePartial(ulong tagRef, ulong streamRef, long option, long sequenceID);

        [DllImport(FPConst.fpDllName)]
        private static extern ulong FPTag_GetFirstChild(ulong tagRef);

        [DllImport(FPConst.fpDllName)]
        private static extern ulong FPTag_GetParent(ulong tagRef);

        [DllImport(FPConst.fpDllName)]
        private static extern ulong FPTag_GetSibling(ulong tagRef);

        [DllImport(FPConst.fpDllName)]
        private static extern ulong FPTag_GetPrevSibling(ulong tagRef);

        [DllImport(FPConst.fpDllName)]
        private static extern void FPTag_GetTagName8(ulong tagRef, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] tagName, ref int tagNameLen);

        [DllImport(FPConst.fpDllName)]
        private static extern void FPTag_SetStringAttribute(ulong tagRef, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))] String inAttrName, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))] String inAttrValue);

        [DllImport(FPConst.fpDllName)]
        private static extern void FPTag_GetStringAttribute(ulong tagRef, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))] String inAttrName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] outAttrValue, ref int ioAttrValueLen);
        #endregion

        #region Wrapper Tag Function
        public static void SetAttribute(ulong tagRef, string key, string value)
        {
            Tag.FPTag_SetStringAttribute(tagRef, key, value);
            Pool.ThrowIfError();
        }

        public static string GetAttribute(ulong tagRef, string attrKey)
        {
            byte[] outString;
            int bufSize = 0;
            int len = 0;

            do
            {
                bufSize += FPConst.fpStringBufferSize;
                len = bufSize;
                outString = new byte[(int)bufSize];
                Tag.FPTag_GetStringAttribute(tagRef, attrKey, outString, ref len);
            } while (len > bufSize);
            Pool.ThrowIfError();
            return Encoding.UTF8.GetString(outString, 0, (int)len - 1);
        }
        public static ulong Create(ulong parentTagRef, string tagName)
        {
            ulong tagRef = FPTag_Create8(parentTagRef, tagName);
            Pool.ThrowIfError();
            return tagRef;
        }

        public static void Close(ulong tagRef)
        {
            FPTag_Close(tagRef);
            Pool.ThrowIfError();
        }

        public static void Delete(ulong tagRef)
        {
            FPTag_Delete(tagRef);
            Pool.ThrowIfError();
        }

        public static ulong Copy(ulong srcTagRef, ulong destParentTagRef, int option)
        {
            ulong newTagRef = FPTag_Copy(srcTagRef, destParentTagRef, option);
            Pool.ThrowIfError();
            return newTagRef;
        }

        public static long GetBlobSize(ulong tagRef)
        {
            long blobSize = FPTag_GetBlobSize(tagRef);
            Pool.ThrowIfError();
            return blobSize;
        }

        public static void BlobRead(ulong tagRef, ulong streamRef, long option)
        {
            FPTag_BlobRead(tagRef, streamRef, option);
            Pool.ThrowIfError();
        }

        public static void BlobReadPartial(ulong tagRef, ulong streamRef, long offset, long length, long option)
        {
            FPTag_BlobReadPartial(tagRef, streamRef, offset, length, option);
            Pool.ThrowIfError();
        }

        public static void BlobWrite(ulong tagRef, ulong streamRef, long option)
        {
            FPTag_BlobWrite(tagRef, streamRef, option);
            Pool.ThrowIfError();
        }

        public static void BlobWritePartial(ulong tagRef, ulong streamRef, long option, long sequenceID)
        {
            FPTag_BlobWritePartial(tagRef, streamRef, option, sequenceID);
            Pool.ThrowIfError();
        }

        public static ulong GetFirstChild(ulong tagRef)
        {
            ulong tRef = FPTag_GetFirstChild(tagRef);
            Pool.ThrowIfError();
            return tRef;
        }

        public static ulong GetParent(ulong tagRef)
        {
            ulong tRef = FPTag_GetParent(tagRef);
            Pool.ThrowIfError();
            return tRef;
        }

        public static ulong GetSibling(ulong tagRef)
        {
            ulong tRef = FPTag_GetSibling(tagRef);
            Pool.ThrowIfError();
            return tRef;
        }

        public static ulong GetPrevSibling(ulong tagRef)
        {
            ulong tRef = FPTag_GetPrevSibling(tagRef);
            Pool.ThrowIfError();
            return tRef;
        }
        public static string GetName8(ulong tagRef)
        {

            String outName = "";

            byte[] outString;
            Int32 bufSize = 0;
            Int32 len = 0;

            do
            {
                bufSize += 256;
                len = bufSize;
                outString = new byte[(int)bufSize];

                FPTag_GetTagName8(tagRef, outString, ref len);
            } while (len > bufSize);
            if (len == 0) return outName;
            outName = Encoding.UTF8.GetString(outString, 0, (int)len - 1);
            outString = null;
            return outName;
            ;
        }
        public static string GetName(ulong tagRef)
        {
            byte[] outString;
            int bufSize = 0;
            int len = 0;

            do
            {
                bufSize += FPConst.fpStringBufferSize;
                len = bufSize;
                outString = new byte[(int)bufSize];

                FPTag_GetTagName8(tagRef, outString, ref len);
            } while (len > bufSize);

            Pool.ThrowIfError();

            return Encoding.UTF8.GetString(outString, 0, (int)len - 1);
        }

        #endregion
    }
    #endregion

    #region Stream Fuctions
    class Stream
    {
        #region Import Stream Functions
        [DllImport(FPConst.fpDllName)]
        private static extern ulong FPStream_CreateBufferForInput(IntPtr pBuffer, long bufferLen);

        [DllImport(FPConst.fpDllName)]
        private static extern ulong FPStream_CreateBufferForOutput(IntPtr pBuffer, long bufferLen);

        [DllImport(FPConst.fpDllName, EntryPoint = "FPStream_CreateFileForInput8")]
        private static extern ulong FPStream_CreateFileForInput([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))] string filePath, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))] string permission, long bufferSize);

        [DllImport(FPConst.fpDllName, EntryPoint = "FPStream_CreateFileForOutput8")]
        private static extern ulong FPStream_CreateFileForOutput([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))] string filePath, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))] string permission);

        [DllImport(FPConst.fpDllName, EntryPoint = "FPStream_CreatePartialFileForInput8")]
        private static extern ulong FPStream_CreatePartialFileForInput([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))] string filePath, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))] string perm, long bufferSize, long offset, long length);

        [DllImport(FPConst.fpDllName, EntryPoint = "FPStream_CreatePartialFileForOutput8")]
        private static extern ulong FPStream_CreatePartialFileForOutput([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))] string filePath, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MarshalPtrToUtf8))] string perm, long bufferSize, long offset, long length, long maxFileLength);

        [DllImport(FPConst.fpDllName)]
        private static extern ulong FPStream_CreateTemporaryFile(long bufferSize);

        [DllImport(FPConst.fpDllName)]
        private static extern ulong FPStream_CreateToNull();

        [DllImport(FPConst.fpDllName)]
        private static extern ulong FPStream_CreateToStdio();

        [DllImport(FPConst.fpDllName)]
        private static extern void FPStream_Close(ulong streamRef);

        /// <summary>
        /// Generic Stream : 1. The Data is not completedly in memory; 2. The data is not an application file.
        /// </summary>
        /// <param name="mBuffer"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        [DllImport(FPConst.fpDllName)]
        private static extern ulong FPStream_CreateGenericStream(FPAsyncCallback prepareProc, FPAsyncCallback completeProc, FPAsyncCallback setMarkerProc, FPAsyncCallback resetMarkerProc, FPAsyncCallback closeProc, IntPtr userData);

        [DllImport(FPConst.fpDllName)]
        unsafe private static extern FPStreamInfo* FPStream_GetInfo(ulong streamHeandle);

        [DllImport(FPConst.fpDllName)]
        private static extern IntPtr FPStream_PrepareBuffer(ulong streamHandle);

        [DllImport(FPConst.fpDllName)]
        private static extern IntPtr FPStream_Complete(ulong streamHandle);

        [DllImport(FPConst.fpDllName)]
        private static extern void FPStream_SetMark(ulong streamHandle);

        [DllImport(FPConst.fpDllName)]
        private static extern void FPStream_ResetMark(ulong streamHandle);

        #endregion

        #region Wrapper Stream Functions

        public static ulong CreateBufferForInput(IntPtr mBuffer, long bufferSize)
        {
            ulong streamRef = FPStream_CreateBufferForInput(mBuffer, bufferSize);
            Pool.ThrowIfError();
            return streamRef;
        }

        public static ulong CreateBufferForOutput(IntPtr mBuffer, long bufferSize)
        {
            ulong streamRef = FPStream_CreateBufferForOutput(mBuffer, bufferSize);
            Pool.ThrowIfError();
            return streamRef;
        }

        public static ulong CreateFileForInput(string filePath, FPStreamPerm perm, long bufferSize)
        {
            ulong streamRef = FPStream_CreateFileForInput(filePath, perm.ToString(), bufferSize);
            Pool.ThrowIfError();
            return streamRef;
        }

        public static ulong CreateFileForOutput(string filePath, FPStreamPerm perm)
        {
            ulong streamRef = FPStream_CreateFileForOutput(filePath, perm.ToString());
            Pool.ThrowIfError();
            return streamRef;
        }

        public static ulong CreatePartialFileForInput(string filePath, FPStreamPerm perm, long bufferSize, long offset, long length)
        {
            ulong streamRef = FPStream_CreatePartialFileForInput(filePath, perm.ToString(), bufferSize, offset, length);
            Pool.ThrowIfError();
            return streamRef;
        }

        public static ulong CreatePartialFileForOutput(string filePath, FPStreamPerm perm, long bufferSize, long offset, long length, long maxFileLengh)
        {
            ulong streamRef = FPStream_CreatePartialFileForOutput(filePath, perm.ToString(), bufferSize, offset, length, maxFileLengh);
            Pool.ThrowIfError();
            return streamRef;
        }

        public static ulong CreateTemporaryFile(long bufferSize)
        {
            ulong streamRef = FPStream_CreateTemporaryFile(bufferSize);
            Pool.ThrowIfError();
            return streamRef;
        }

        public static ulong CreateToNull()
        {
            ulong streamRef = FPStream_CreateToNull();
            Pool.ThrowIfError();
            return streamRef;
        }

        public static ulong CreateToStdio()
        {
            ulong streamRef = FPStream_CreateToStdio();
            Pool.ThrowIfError();
            return streamRef;
        }

        public static void Close(ulong streamRef)
        {
            FPStream_Close(streamRef);
            Pool.ThrowIfError();
        }

        //FSStream_CreateGenericStream(FPCallback prepareProc, FPCallback completeProc, FPCallback setMarkerProc, FPCallback resetMarkerProc, FPCallback closeProc, IntPtr userData);
        public static ulong CreateGenericStream(FPAsyncCallback prepareProc, FPAsyncCallback completeProc, FPAsyncCallback setMarkerProc, FPAsyncCallback resetMarkerProc, FPAsyncCallback closeProc, IntPtr userData)
        {
            ulong streamRef = FPStream_CreateGenericStream(prepareProc, completeProc, setMarkerProc, resetMarkerProc, closeProc, userData);
            Pool.ThrowIfError();
            return streamRef;
        }

        unsafe public static FPStreamInfo* GetInfo(ulong streamHandle)
        {
            FPStreamInfo* info;
            info = FPStream_GetInfo(streamHandle);
            Pool.ThrowIfError();
            return info;
        }
        #endregion
    }
    #endregion

}
