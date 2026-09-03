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
using AvePoint.GCommon.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace AvePoint.GCommon.Transfer.HttpMode.Common
{
    public class HttpModePeer : IDisposable
    {
        private static AveLogger logger = new AveLogger(typeof(HttpModePeer), false);

        private bool isSenderMatch = false;
        private bool isReceiverMatch = false;
        private bool isSenderReconnectionMatch = false;
        private bool isReceiverReconnectionMatch = false;
        private long serialNumberForReconnection = 0;//only for reconnection
        private bool clearStream = false;
        private string sessionId = string.Empty;
        private object MemoryStreamLock = new object();
        //bool finishReceiveSuccess = true;//check receive stream finished success

        public HttpModeStream CacheStream = new HttpModeStream();//cache data for transfer

        public HttpModePeer(string sessionId)
        {
            this.sessionId = sessionId;
        }

        public HttpModePeer(string sessionId, bool isSenderMatch, bool isReceiverMatch)
        {
            this.sessionId = sessionId;
            this.isSenderMatch = isSenderMatch;
            this.isReceiverMatch = isReceiverMatch;
        }

        /// <summary>
        /// httpStream Write to memoryStream 
        /// </summary>
        public void WriteByte(byte[] buffer, int offset, int length)
        {
            if (MatchComplete)
            {
                CacheStream.Write(buffer, offset, length);
            }
        }

        public void ReadDataFromStream(Stream tempStream)
        {
            int bufferSize = 64 * 1000;
            byte[] buffer = new byte[bufferSize];
            int readLength = 0;
            //finishReceiveSuccess = true;

            if(CacheStream.IsStopped)
            {
                CacheStream.Reset();
            }

            logger.Info("start to read data from stream with session id:{0}", sessionId);
            try
            {
                while (((readLength = tempStream.Read(buffer, 0, bufferSize)) > 0))
                {
                    CacheStream.Write(buffer, 0, readLength);
                }
            }
            catch (Exception e)
            {
                logger.Error("Read data from stream with session id:{0} failed:{1}", sessionId, e);
                //finishReceiveSuccess = false;
                try
                {
                    if(System.ServiceModel.OperationContext.Current != null &&
                        System.ServiceModel.OperationContext.Current.RequestContext!= null)
                    {
                        System.ServiceModel.OperationContext.Current.RequestContext.Close();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("force close request failed:{0}", ex);
                }

                return;
            }

            logger.Info("read data from stream successful, session id:{0}", sessionId);

            WriteFinish();
        }

        public bool MatchSender
        {
            set { this.isSenderMatch = value; }
            get { return this.isSenderMatch; }
        }

        public bool MatchReconnectionSender
        {
            set { this.isSenderReconnectionMatch = value; }
            get { return this.isSenderReconnectionMatch; }
        }

        public bool MatchReconnectionReceiver
        {
            set { this.isReceiverReconnectionMatch = value; }
            get { return this.isReceiverReconnectionMatch; }
        }

        public bool ClearStream
        {
            get { return this.clearStream; }
            set
            {
                this.clearStream = false;
            }
        }

        public string SessionId
        {
            get { return this.sessionId; }
        }

        public bool MatchReceiver
        {
            set { this.isReceiverMatch = value; }
            get { return this.isReceiverMatch; }
        }


        public long SerialNumberForReconnection
        {
            set { this.serialNumberForReconnection = value; }
            get { return this.serialNumberForReconnection; }
        }

        public bool MatchComplete
        {
            get
            {
                lock (MemoryStreamLock)
                {
                    bool complete = isSenderMatch && isReceiverMatch;
                    return complete;
                }
            }
        }

        public bool MatchReconnectionComplete
        {
            get
            {
                lock (MemoryStreamLock)
                {
                    bool reconnectionComplete = this.isReceiverReconnectionMatch && this.isSenderReconnectionMatch;
                    //if (CacheStream.mStream != null && (!reconnectionComplete))
                    //{
                    //    return false;
                    //}
                    //else
                    //{
                    //    if (reconnectionComplete && CacheStream.mStream == null)
                    //    {
                    //        CacheStream = new HttpModeStream();
                    //    }
                    //    return reconnectionComplete;
                    //}


                    return reconnectionComplete;
                }
            }

        }

        //public bool FinishReceiveSuccess
        //{
        //    get { return finishReceiveSuccess; }
        //}

        //public void ClearUsedStream(bool waitClose)
        //{
        //    lock (MemoryStreamLock)
        //    {
        //        if (CacheStream.ISNewOne)
        //        {
        //            this.isReceiverReconnectionMatch = false;
        //            this.isSenderReconnectionMatch = false;
        //            serialNumberForReconnection = 0;
        //            if (waitClose)
        //            {
        //                while (!CacheStream.IsFinishWrite)
        //                {
        //                    Thread.Sleep(2000);
        //                }
        //            }
        //            CacheStream.Dispose();
        //        }
        //    }
        //}

        public void WriteFinish()
        {
            CacheStream.Flush();
        }

        public void Dispose()
        {
            CacheStream.Dispose();
        }

        /// <summary>
        /// stop read data
        /// </summary>
        internal void StopWriteData(string message)
        {
            if(CacheStream != null)
            {
                CacheStream.Stop(message);
            }
        }
    }

    internal class HttpModePeerCache
    {
        private static Dictionary<string, HttpModePeer> caches = new Dictionary<string, HttpModePeer>(StringComparer.OrdinalIgnoreCase);

        public static HttpModePeer EnsureHttpModePee(string sessionId)
        {
            HttpModePeer peer = null;
            lock (caches)
            {
                if(!caches.TryGetValue(sessionId, out peer))
                {
                    peer = new HttpModePeer(sessionId);
                    caches[sessionId] = peer;
                }
            }
            return peer;
        }

        public static HttpModePeer QueryHttpModePeer(string sessionId)
        {
            HttpModePeer peer = null;
            lock (caches)
            {
                caches.TryGetValue(sessionId, out peer);
            }
            return peer;
        }

        internal static bool RemovePeer(string sessionId)
        {
            var findValue = false;
            lock(caches)
            {
                HttpModePeer peer = null;
                if(caches.TryGetValue(sessionId, out peer))
                {
                    DataTransferLogger.Logger(AveLogLevel.INFO, "start to remove session {0} and release stream.", sessionId);
                    findValue = true;
                    caches.Remove(sessionId);
                    peer.Dispose();
                }
            }

            return findValue;
        }
    }

}
