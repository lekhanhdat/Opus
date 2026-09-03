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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Pipe
{
    /// <summary>
    /// Client 端：双向同步管道
    /// </summary>
    public class RANamedPipeClientStream : IDisposable
    {

        #region private property

        private static readonly int DefaultConnectTimeout = 60 * 1000;

        private static readonly string DefaultServerName = ".";

        private static readonly PipeDirection DefaultPipeDirection = PipeDirection.InOut;
        private UnicodeEncoding StreamEncoding;

        private static readonly JsonSerializerSettings DefaultSerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        private static NamedPipeClientStream PipeClientStream;

        #endregion

        #region public property

        public string PipeName { get; set; }

        public string ServerName { get; }

        public PipeDirection Direction { get; }

        public int ConnectTimeout { get; }

        #endregion

        public RANamedPipeClientStream(string pipeName)
            : this(pipeName, DefaultServerName, DefaultConnectTimeout)
        {
        }

        public RANamedPipeClientStream(string pipeName, string serverName)
            : this(pipeName, serverName, DefaultConnectTimeout)
        {

        }

        public RANamedPipeClientStream(string pipeName, string serverName, int connectTimeout)
        {
            PipeName = pipeName;
            ServerName = serverName;
            Direction = DefaultPipeDirection;
            ConnectTimeout = connectTimeout;
            PipeClientStream = new NamedPipeClientStream(ServerName, PipeName, Direction);
            PipeClientStream.Connect(ConnectTimeout);
            StreamEncoding = new UnicodeEncoding();
        }

        public bool SendMessage(object value)
        {
            return SendMessage(value, DefaultSerializerSettings);
        }

        public bool SendMessage(object value, JsonSerializerSettings serializerSettings)
        {
            if(value == null || serializerSettings == null)
            {
                return false;
            }
            var valueSerialized = JsonConvert.SerializeObject(value, serializerSettings);
            byte[] outBuffer = StreamEncoding.GetBytes(valueSerialized);
            int len = outBuffer.Length;
            var header = BitConverter.GetBytes(len);
            PipeClientStream.Write(header, 0, 4);
            PipeClientStream.Write(outBuffer, 0, len);
            PipeClientStream.Flush();
            return true;
        }

        public Task<bool> SendMessageAsync(object value, CancellationToken cancellationToken)
        {
            return SendMessageAsync(value, DefaultSerializerSettings, cancellationToken);
        }

        public async Task<bool> SendMessageAsync(object value, JsonSerializerSettings serializerSettings, CancellationToken cancellationToken)
        {
            if (value == null || serializerSettings == null)
            {
                return false;
            }
            var valueSerialized = JsonConvert.SerializeObject(value, serializerSettings);
            byte[] outBuffer = StreamEncoding.GetBytes(valueSerialized);
            int len = outBuffer.Length;
            var header = BitConverter.GetBytes(len);
            await PipeClientStream.WriteAsync(header, 0, 4, cancellationToken);
            await PipeClientStream.WriteAsync(outBuffer, 0, len, cancellationToken);
            await PipeClientStream.FlushAsync(cancellationToken);
            return true;
        }

        public string ReadMessage()
        {
            return ReadMessage<string>();
        }

        public T ReadMessage<T>()
        {
            return ReadMessage<T>(DefaultSerializerSettings);
        }

        public T ReadMessage<T>(JsonSerializerSettings serializerSettings)
        {
            var message = SafeReadMessage();
            return JsonConvert.DeserializeObject<T>(message, serializerSettings);
        }

        public Task<string> ReadMessageAsync(CancellationToken cancellationToken)
        {
            return ReadMessageAsync<string>(cancellationToken);
        }

        public Task<T> ReadMessageAsync<T>(CancellationToken cancellationToken)
        {
            return ReadMessageAsync<T>(DefaultSerializerSettings, cancellationToken);
        }

        public async Task<T> ReadMessageAsync<T>(JsonSerializerSettings serializerSettings, CancellationToken cancellationToken)
        {
            var message = await SafeReadMessageAsync(cancellationToken);
            return JsonConvert.DeserializeObject<T>(message, serializerSettings);
        }

        private string SafeReadMessage()
        {
            var header = new byte[4];
            int bRead = 0;
            while (bRead < header.Length)
            {
                int rd = PipeClientStream.Read(header, bRead, header.Length - bRead);
                if (rd == -1)
                {
                    throw new IOException("file is unusually small");
                }
                bRead += rd;
            }
            int len = BitConverter.ToInt32(header, 0);
            var inBuffer = new byte[len];

            bRead = 0;
            while (bRead < len)
            {
                int rd = PipeClientStream.Read(inBuffer, bRead, len - bRead);
                if (rd == -1)
                {
                    throw new IOException("file is unusually small");
                }
                bRead += rd;
            }
            return StreamEncoding.GetString(inBuffer);
        }

        private async Task<string> SafeReadMessageAsync(CancellationToken cancellationToken)
        {
            var header = new byte[4];
            int bRead = 0;
            while (bRead < header.Length)
            {
                int rd = await PipeClientStream.ReadAsync(header, bRead, header.Length - bRead, cancellationToken);
                if (rd == -1)
                {
                    throw new IOException("file is unusually small");
                }
                bRead += rd;
            }
            int len = BitConverter.ToInt32(header, 0);
            var inBuffer = new byte[len];

            bRead = 0;
            while (bRead < len)
            {
                int rd = await PipeClientStream.ReadAsync(inBuffer, bRead, len - bRead, cancellationToken);
                if (rd == -1)
                {
                    throw new IOException("file is unusually small");
                }
                bRead += rd;
            }
            return StreamEncoding.GetString(inBuffer);
        }

        public void Dispose()
        {
            PipeClientStream?.Dispose();
        }
    }
}
