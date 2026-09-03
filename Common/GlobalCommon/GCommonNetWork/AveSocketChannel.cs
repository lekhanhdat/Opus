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
using System.IO;
using System.Net.Sockets;

namespace AvePoint.GCommon.Network
{
    internal class AveSocketChannel : IAveNetworkChannel
    {
        private Socket socket;
        private Stream socketStream;

        private DateTime lastWriteSucceedTime = DateTime.MinValue;
        private DateTime currentWriteStartTime = DateTime.MinValue;
        private long totalWriteBytes = 0;
        private long totalWriteTime = 0;
        private DateTime lastReadSucceedTime = DateTime.MinValue;
        private DateTime currentReadStartTime = DateTime.MinValue;
        private long totalReadBytes = 0;
        private long totalReadTime = 0;

        public AveSocketChannel(Socket socket, Stream socketStream)
        {
            this.socket = socket;
            this.socketStream = socketStream;
        }

        #region IAveNetworkChannel Members

        public void Write(byte[] data, int offset, int len)
        {
            AveNetworkSpeedPerformanceCounter.Begin(AveNetworkSpeedPerformanceCounterCatalogs.SocketWriteCatalog);
            try
            {
                currentWriteStartTime = DateTime.Now;
                SendReceiveUtility.SafeSend(socketStream, data, offset, len);
                lastWriteSucceedTime = DateTime.Now;
            }
            catch (Exception e)
            {
                AveNetworkTrace.TraceError("An error occurred while writing to socket stream. LastWriteSucceedTime:{0} CurrentWriteStartTime:{1} Now:{2} Exception:{3}", lastWriteSucceedTime.ToString(), currentWriteStartTime.ToString(), DateTime.Now.ToString(), e.ToString());
                throw;
            }
            totalWriteTime += (DateTime.Now.Ticks - currentWriteStartTime.Ticks);
            totalWriteBytes += len;
            AveNetworkSpeedPerformanceCounter.End(AveNetworkSpeedPerformanceCounterCatalogs.SocketWriteCatalog, len);
        }

        public int Read(byte[] data, int offset, int len)
        {
            AveNetworkSpeedPerformanceCounter.Begin(AveNetworkSpeedPerformanceCounterCatalogs.SocketReadCatalog);
            int readLen = 0;
            try
            {
                currentReadStartTime = DateTime.Now;
                readLen = socketStream.Read(data, offset, len);
                lastReadSucceedTime = DateTime.Now;
            }
            catch (Exception e)
            {
                AveNetworkTrace.TraceError("An error occurred while reading from socket stream. LastReadSucceedTime:{0} CurrentReadStartTime:{1} Now:{2} Exception:{3}", lastReadSucceedTime.ToString(), currentReadStartTime.ToString(), DateTime.Now.ToString(), e.ToString());
                throw;
            }
            totalReadTime += (DateTime.Now.Ticks - currentReadStartTime.Ticks);
            totalReadBytes += readLen;
            AveNetworkSpeedPerformanceCounter.End(AveNetworkSpeedPerformanceCounterCatalogs.SocketReadCatalog, readLen);
            return readLen;
        }

        public void Shutdown(ShutDownOptions option)
        {
            if (socket != null)
            {
                if (option == ShutDownOptions.Both)
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                if (option == ShutDownOptions.Send)
                {
                    socket.Shutdown(SocketShutdown.Send);
                }
                if (option == ShutDownOptions.Receive)
                {
                    socket.Shutdown(SocketShutdown.Receive);
                }
            }
        }

        public void Close()
        {
            if (socketStream != null)
            {
                socketStream.Close();
                socketStream = null;
            }
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
        }

        public void ReplaceSocket(Socket socket, Stream socketStream)
        {
            this.socket = socket;
            this.socketStream = socketStream;
        }

        public bool BackToByte(long offset)
        {
            if (totalWriteBytes == offset)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public long TotalBytesReceived { get { return totalReadBytes; } }

        public long TotalReadTime { get { return totalReadTime; } }

        public long TotalBytesSent { get { return totalWriteBytes; } }

        public long TotalWriteTime { get { return totalWriteTime; } }

        public int Available { get { return socket.Available; } }

        #endregion
    }

}
