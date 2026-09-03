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




using System.Globalization;

namespace AvePoint.GCommon.Network
{
    #region using directives
    using System;
    using System.Net.Sockets;
    using System.IO;
    using AvePoint.GCommon.Utility;
    #endregion

    internal class AveSocketChannel : IAveNetworkChannel
    {
        private Socket socket;
        private Stream socketStream;

        private DateTime lastWriteSucceedTime = DateTime.MinValue;
        private DateTime currentWriteStartTime = DateTime.MinValue;
        private DateTime lastReadSucceedTime = DateTime.MinValue;
        private DateTime currentReadStartTime = DateTime.MinValue;

        public AveSocketChannel(Socket socket, Stream socketStream)
        {
            this.socket = socket;
            this.socketStream = socketStream;
        }

        #region IAveNetworkChannel Members

        public void Write(byte[] data, int offset, int len)
        {
            AveSpeedPerformanceCounter.Begin(AveSpeedPerformanceCounterCatalogs.SocketWriteCatalog);
            try
            {
                currentWriteStartTime = DateTime.Now;
                SendReceiveUtility.SafeSend(socketStream, data, offset, len);
                lastWriteSucceedTime = DateTime.Now;
            }
            catch (Exception e)
            {
                AveNetworkTrace.TraceError("An error occurred while writing to socket stream. LastWriteSucceedTime:{0} CurrentWriteStartTime:{1} Now:{2} Exception:{3}", lastWriteSucceedTime.ToString(CultureInfo.InvariantCulture), currentWriteStartTime.ToString(CultureInfo.InvariantCulture), DateTime.Now.ToString(CultureInfo.InvariantCulture), e.ToString());
                throw;
            }
            TotalWriteTime += (DateTime.Now.Ticks - currentWriteStartTime.Ticks);
            TotalBytesSent += len;
            AveSpeedPerformanceCounter.End(AveSpeedPerformanceCounterCatalogs.SocketWriteCatalog, len);
        }

        public int Read(byte[] data, int offset, int len, bool mustGet)
        {
            AveSpeedPerformanceCounter.Begin(AveSpeedPerformanceCounterCatalogs.SocketReadCatalog);
            int readLen;
            try
            {
                currentReadStartTime = DateTime.Now;
                readLen = socketStream.Read(data, offset, len);
                if (readLen <= 0)
                {
                    if (mustGet) throw new ReadEmptyDataFromSocketException();
                }
                lastReadSucceedTime = DateTime.Now;
            }
            catch (Exception e)
            {
                AveNetworkTrace.TraceError("An error occurred while reading from socket stream. LastReadSucceedTime:{0} CurrentReadStartTime:{1} Now:{2} Exception:{3}", lastReadSucceedTime.ToString(CultureInfo.InvariantCulture), currentReadStartTime.ToString(CultureInfo.InvariantCulture), DateTime.Now.ToString(CultureInfo.InvariantCulture), e.ToString());
                throw;
            }
            TotalReadTime += (DateTime.Now.Ticks - currentReadStartTime.Ticks);
            TotalBytesReceived += readLen;
            AveSpeedPerformanceCounter.End(AveSpeedPerformanceCounterCatalogs.SocketReadCatalog, readLen);
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

        // ReSharper disable ParameterHidesMember
        public void ReplaceSocket(Socket socket, Stream socketStream)
        // ReSharper restore ParameterHidesMember
        {
            this.socket = socket;
            this.socketStream = socketStream;
        }

        public bool BackToByte(long offset)
        {
            throw new NotImplementedException();
        }

        public Int64 TotalBytesReceived { get; private set; }
        public Int64 TotalReadTime { get; private set; }
        public Int64 TotalBytesSent { get; private set; }
        public Int64 TotalWriteTime { get; private set; }
        public Int32 Available { get { return socket.Available; } }

        #endregion
    }
}