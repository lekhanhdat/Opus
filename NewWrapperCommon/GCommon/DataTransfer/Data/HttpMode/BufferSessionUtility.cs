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
using AvePoint.GCommon.Network;
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Transfer.HttpMode;
using AvePoint.GCommon.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace AvePoint.GCommon.Transfer.Data.HttpMode
{
    public class BufferSessionUtility : IDisposable
    {
        private AutoResetEvent waitEvent = new AutoResetEvent(false);
        private bool stopTransferThread = false;
        private FileCycleStream fileCycleStream;
        private static AveLogger logger = AveLogger.GetInstance(typeof(BufferSessionUtility));
        internal FileCycleStream FileCycleStream
        {
            get { return fileCycleStream; }
            set { this.fileCycleStream = value; }
        }
        private Stream dataStream;
        public Stream DataStream
        {
            get { return dataStream; }
            set { this.dataStream = value; }
        }

        private bool isInput;
        public bool IsInput
        {
            get { return isInput; }
            set { isInput = value; }
        }

        private AveThreadWrapper transferThread;
        public AveThreadWrapper TransferThread
        {
            get { return this.transferThread; }
            set { this.transferThread = value; }
        }

        private int readSerialNumber = 0;
        public int ReadSerialNumber
        {
            get { return readSerialNumber; }
            set { readSerialNumber = value; }
        }

        private int needReadLength = 0;
        private int NeedReadLength
        {
            get { return needReadLength; }
            set { this.needReadLength = value; }
        }

        private int reSendOffSet = 0;

        private int putSerialNumber;
        public int PutSerialNumber
        {
            get { return putSerialNumber; }
            set { this.putSerialNumber = value; }
        }

        private bool CanBeReUsed = true;

        private string mBaseSessionId = string.Empty;

        private CacheBuffer mCacheSendBuffer;//cache data for sender

        internal BufferSessionUtility(string sessionId, FileCycleStream fileCycleStream, Stream dataStream, bool input, int length = 0)
        {
            this.mBaseSessionId = sessionId;
            this.FileCycleStream = fileCycleStream;
            this.DataStream = dataStream;
            this.IsInput = input;
            if (length > 0)
            {
                mCacheSendBuffer = new CacheBuffer(fileCycleStream.CacheFilePath, length);
                NeedReadLength = length;
            }
            MutiThransferUtilityCache.ExcuteThread(mBaseSessionId, isInput, DataTransferThread);
            //this.TransferThread = AveThreadUtility.StartThread(DataTransferThread, "TransferThreadInner", string.Empty);
        }

        public void RestartSessionStream(Stream dataStream)
        {
            ResetStream();
            this.DataStream = dataStream;
            //this.TransferThread = AveThreadUtility.StartThread(DataTransferThread, "TransferThreadInner", string.Empty);
            MutiThransferUtilityCache.ExcuteThread(mBaseSessionId, isInput, DataTransferThread);
            //DataTransferThread();
        }

        public bool StopTransfer()
        {
            stopTransferThread = true;
            bool value =  MutiThransferUtilityCache.ForceStopThread(mBaseSessionId, isInput);
            //AveThreadUtility.SafeStopThread(transferThread, 2 * 1000, string.Empty);
            //return !transferThread.IsAlive && CanBeReUsed;
            return value && CanBeReUsed;

        }

        private void ResetStream()
        {
            stopTransferThread = false;
            //AveThreadUtility.SafeStopThread(transferThread, 2 * 1000, string.Empty);
            if (this.DataStream != null)
            {
                DataStream.Dispose();
                this.DataStream = null;
            }
            this.TransferThread = null;

        }

        public void WaitUntilProcessFinish()
        {
            waitEvent.WaitOne();
            CanBeReUsed = true;
        }

        private void DataTransferThread()
        {
            CanBeReUsed = false;
            if (IsInput)
            {
                InPutData();
            }
            else
            {
                OutPutData();
            }

        }

        private void InPutData()
        {
            try
            {
                while (!stopTransferThread)
                {
                    byte[] buffer = new byte[64 * 1024];
                    int readLength = 0;
                    while ((readLength = DataStream.Read(buffer, 0, 64 * 1024)) != 0)
                    {
                        FileCycleStream.SafeWrite(buffer, 0, readLength);
                        putSerialNumber += readLength;
                    }
                    buffer = null;
                    break;
                }
            }
            catch (Exception e)
            {
                logger.Error("get data from network and put into file cycle stream failed, exception:{0}", e.ToString());
            }
            finally
            {
                waitEvent.Set();
            }
        }

        private void OutPutData()
        {
            try
            {
                while (!stopTransferThread)
                {
                    int copyLength = mCacheSendBuffer.CopyDateToStream(DataStream, reSendOffSet);
                    ReadSerialNumber = copyLength + reSendOffSet;
                    byte[] buffer = new byte[64 * 1024];
                    while (ReadSerialNumber < NeedReadLength)
                    {
                        int readLength = (64 * 1024 >= NeedReadLength - ReadSerialNumber) ? NeedReadLength - ReadSerialNumber : 64 * 1024;
                        readLength = fileCycleStream.SafeRead(buffer, 0, readLength);
                        mCacheSendBuffer.CopyDataToCache(buffer, 0, readLength);
                        DataStream.Write(buffer, 0, readLength);
                        ReadSerialNumber += readLength;
                    }
                    DataStream.Flush();
                    buffer = null;
                    //CacheSendBuffer = null;
                    break;
                }
            }
            catch (Exception e)
            {
                logger.Error("put file cycle stream into network failed, exception:{0}", e.ToString());
            }
            finally
            {
                CanBeReUsed = true;
                waitEvent.Set();
            }
        }

        public void ResetQueueData(int serialNumber)
        {
            reSendOffSet = serialNumber;
        }

        public void Dispose()
        {
            if (waitEvent != null)
            {
                waitEvent.Close();
                waitEvent = null;
            }
            if (mCacheSendBuffer != null)
            {
                mCacheSendBuffer.Dispose();
            }
            if (dataStream != null)
            {
                dataStream.Dispose();
                dataStream = null;
            }
        }
    }

    public class BufferSessionUtilityCache
    {
        public string SessionId = string.Empty;

        public Dictionary<BufferSessionUtilityKey, BufferSessionUtility> BufferUtilityDic = new Dictionary<BufferSessionUtilityKey, BufferSessionUtility>();

        public BufferSessionUtilityCache(string sessionId)
        {
            this.SessionId = sessionId;
        }

        internal BufferSessionUtility AddOrUpdateBufferUtility(Stream dataStream, FileCycleStream fileCycleStream, string sessionId, bool isInput, int length = 0)
        {
            BufferSessionUtility utility = null;
            BufferSessionUtilityKey key = new BufferSessionUtilityKey() { SessionId = sessionId, IsInput = isInput };
            foreach (BufferSessionUtilityKey keyInDic in BufferUtilityDic.Keys)
            {
                if (key.Equals(keyInDic))
                {
                    utility = BufferUtilityDic[keyInDic];
                    break;
                }
            }

            if (utility == null)
            {
                utility = new BufferSessionUtility(SessionId, fileCycleStream, dataStream, key.IsInput, length);
                BufferUtilityDic[key] = utility;
            }
            else
            {
                utility.RestartSessionStream(dataStream);
            }
            return utility;
        }

        internal BufferSessionUtility GetBufferUtility(string sessionId, bool isInput)
        {
            BufferSessionUtilityKey key = new BufferSessionUtilityKey() { SessionId = sessionId, IsInput = isInput };
            BufferSessionUtility utility = null;
            foreach (BufferSessionUtilityKey keyInDic in BufferUtilityDic.Keys)
            {
                if (key.Equals(keyInDic))
                {
                    utility = BufferUtilityDic[keyInDic];
                    break;
                }
            }
            return utility;
        }

        internal void ClearBufferUtility()
        {
            foreach (BufferSessionUtility utility in BufferUtilityDic.Values)
            {
                utility.Dispose();
            }
            BufferUtilityDic.Clear();
        }
    }

    public class GlobalBufferSessionUtilityCache
    {
        public static Dictionary<string, BufferSessionUtilityCache> BufferSessionGlobalCache = new Dictionary<string, BufferSessionUtilityCache>();

        public static BufferSessionUtility AddOrUpdateBufferUtility(string sessionId, Stream dataStream, FileCycleStream fileCycleStream, string subSessionId, bool isInput, int length = 0)
        {
            lock (BufferSessionGlobalCache)
            {
                BufferSessionUtilityCache utilityCache = BufferSessionGlobalCache[sessionId];
                BufferSessionUtilityKey key = new BufferSessionUtilityKey() { SessionId = subSessionId, IsInput = isInput };
                foreach (BufferSessionUtilityKey keyInDic in utilityCache.BufferUtilityDic.Keys)
                {
                    if ((!key.Equals(keyInDic)) && (keyInDic.IsInput == isInput))
                    {
                        utilityCache.BufferUtilityDic[keyInDic].Dispose();
                    }
                }
                return BufferSessionGlobalCache[sessionId].AddOrUpdateBufferUtility(dataStream, fileCycleStream, subSessionId, isInput, length);

            }
        }

        public static BufferSessionUtility GetBufferUtility(string sessionId, bool isInput)
        {
            lock (BufferSessionGlobalCache)
            {
                BufferSessionUtilityKey key = new BufferSessionUtilityKey() { SessionId = sessionId, IsInput = isInput };
                BufferSessionUtility utility = null;
                foreach (BufferSessionUtilityCache cache in BufferSessionGlobalCache.Values)
                {
                    foreach (BufferSessionUtilityKey keyInDic in cache.BufferUtilityDic.Keys)
                    {
                        if (key.Equals(keyInDic))
                        {
                            utility = cache.BufferUtilityDic[keyInDic];
                            break;
                        }
                    }
                    if (utility != null)
                    {
                        break;
                    }
                }
                return utility;
            }
        }

        public static bool InitGlobalBufferSessionWithId(string sessionId)
        {
            lock (BufferSessionGlobalCache)
            {
                if (!BufferSessionGlobalCache.ContainsKey(sessionId))
                {
                    BufferSessionGlobalCache.Add(sessionId, new BufferSessionUtilityCache(sessionId));
                    return true;
                }
                return false;
            }
        }

        public static bool CheckGlobalBufferSessionIdExist(string sessionId)
        {
            lock (BufferSessionGlobalCache)
            {
                return BufferSessionGlobalCache.ContainsKey(sessionId);
            }
        }

        public static void ClearSession(string sessionId)
        {
            lock (BufferSessionGlobalCache)
            {
                if (BufferSessionGlobalCache.ContainsKey(sessionId))
                {
                    BufferSessionGlobalCache[sessionId].ClearBufferUtility();
                    BufferSessionGlobalCache.Remove(sessionId);
                }
            }
        }
    }

    public class BufferSessionUtilityKey
    {
        public string SessionId;
        public bool IsInput;

        public bool Equals(BufferSessionUtilityKey bufferUtility)
        {
            return SessionId.Equals(bufferUtility.SessionId, StringComparison.OrdinalIgnoreCase) && IsInput.Equals(bufferUtility.IsInput);
        }

    }

    public enum ReadBufferStatus
    {
        Exception,
        Ok,
        Finish,
        KeepAlive,

    }
}
