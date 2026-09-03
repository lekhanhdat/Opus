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
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Transfer.Data.Multiple.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AvePoint.GCommon.Transfer.Data.HttpMode
{
    class FileCycleStreamCacheUtility
    {
        public static Dictionary<string, FileCycleStream> GlobalCache = new Dictionary<string, FileCycleStream>();

        public static SessionStatus InitCycleSessionStream(string sessionId)
        {
            lock (GlobalCache)
            {
                if (GlobalCache.ContainsKey(sessionId))
                {
                    return SessionStatus.IsInUse;
                }
                else
                {
                    FileCycleStream fileCycleStream = new FileCycleStream(DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.FileCycleStreamSize * 1024 * 1024, (DataTransferGlobalConfig.DataTransferConfiguration.DataConfig.FileCycleStreamSize * 1024 * 1024) / 10);
                    GlobalCache.Add(sessionId, fileCycleStream);
                    return SessionStatus.InitedOK;
                }
            }
        }

        public static SessionStatus CheckCycleSessionStreamExist(string sessionId)
        {
            lock (GlobalCache)
            {
                if (!GlobalCache.ContainsKey(sessionId))
                {
                    return SessionStatus.NonExist;
                }
                else
                {
                    return SessionStatus.IsReady;
                }
            }
        }

        public static int ClearFileCycleStringSession(string sessionId)
        {
            FileCycleStream stream = null;
            lock (GlobalCache)
            {
                if (GlobalCache.ContainsKey(sessionId))
                {
                    stream = GlobalCache[sessionId];
                }
                GlobalCache.Remove(sessionId);
            }
            if (stream != null)
            {
                stream.Dispose();
            }
            return 0;
        }

        public static FileCycleStream GetFileCycleStream(string sessionId)
        {
            lock (GlobalCache)
            {
                return GlobalCache[sessionId];
            }
        }
    }

    class TransferMutiThreadUtility
    {
        private AveMultiTaskThread mInputThread;
        private AveMultiTaskThread mOutPutThread;
        private string mSessionId;

        public TransferMutiThreadUtility(string sessionId)
        {
            this.mSessionId = sessionId;
        }

        public AveMultiTaskThread InputThread
        {
            get
            {
                if (mInputThread == null)
                {
                    mInputThread = new AveMultiTaskThread(string.Format("{0}_MutiSender", mSessionId));
                }
                return mInputThread;
            }
            set
            {
                mInputThread = value;
            }
        }

        public AveMultiTaskThread OutputThread
        {
            get
            {
                if (mOutPutThread == null)
                {
                    mOutPutThread = new AveMultiTaskThread(string.Format("{0}_MutiReceiver", mSessionId));
                }
                return mOutPutThread;
            }
            set { mOutPutThread = value; }
        }

        public void ExcuteInputTask(Action sendAction)
        {
            while (!InputThread.IsAvailable())
            {
                Thread.Sleep(1000);
            }
            InputThread.ExecuteTask(sendAction);
        }

        public void ExcuteOutputTask(Action receiveAction)
        {
            while (!OutputThread.IsAvailable())
            {
                Thread.Sleep(1000);
            }
            OutputThread.ExecuteTask(receiveAction);
        }

        public void ClearResource()
        {
            if (this.mInputThread != null)
            {
                this.mInputThread.Stop();
            }
            if (this.mOutPutThread != null)
            {
                this.mOutPutThread.Stop();
            }
        }

        public bool ForceDisposeInput()
        {
            if (mInputThread != null)
            {
                if (mInputThread.ForceDispose())
                {
                    mInputThread = null;
                    return true;
                }
                return false;
            }
            return true;
        }

        public bool ForceDisposeOutput()
        {
            if (mOutPutThread != null)
            {
                if (mOutPutThread.ForceDispose())
                {
                    mOutPutThread = null;
                    return true;
                }
                return false;
            }
            return true;
        }
    }

    public class MutiThransferUtilityCache
    {
        private static Dictionary<string, TransferMutiThreadUtility> cacheList = new Dictionary<string, TransferMutiThreadUtility>();

        public static void ExcuteThread(string sessionId, bool isInput, Action sendReceiveAction)
        {
            lock (cacheList)
            {
                if (!cacheList.ContainsKey(sessionId))
                {
                    cacheList[sessionId] = new TransferMutiThreadUtility(sessionId);
                }
                if (isInput)
                {
                    cacheList[sessionId].ExcuteInputTask(sendReceiveAction);
                }
                else
                {
                    cacheList[sessionId].ExcuteOutputTask(sendReceiveAction);
                }
            }
        }

        public static bool ForceStopThread(string sessionId, bool isInput)
        {
            lock (cacheList)
            {
                if (!cacheList.ContainsKey(sessionId))
                {
                    cacheList[sessionId] = new TransferMutiThreadUtility(sessionId);
                }
                if (isInput)
                {
                    return cacheList[sessionId].ForceDisposeInput();
                }
                else
                {
                    return cacheList[sessionId].ForceDisposeOutput();
                }
            }
        }

        public static void Clear(string sessionId)
        {
            TransferMutiThreadUtility utility = null;
            lock (cacheList)
            {
                if (cacheList.ContainsKey(sessionId))
                {
                    utility = cacheList[sessionId];
                    cacheList.Remove(sessionId);
                }
                if (utility != null)
                {
                    utility.ClearResource();
                }
            }
            
        }
    }

}
