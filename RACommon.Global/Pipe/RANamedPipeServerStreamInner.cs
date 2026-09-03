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

namespace AvePoint.RA.Common.Pipe
{
    public class RANamedPipeServerStreamInner : IDisposable
    {

        private readonly Guid Uid = Guid.NewGuid();

        private readonly NamedPipeServerStream PipeServerStream;

        private UnicodeEncoding StreamEncoding;

        private static readonly JsonSerializerSettings DefaultSerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public RANamedPipeServerStreamInner(string pipeName, PipeDirection direction, int numberOfServerInstances, Action<RANamedPipeServerStreamInner> clientOnConnect, Action<RANamedPipeServerStreamInner> endConnectCallBack)
        {
            PipeServerStream = new NamedPipeServerStream(pipeName, direction, numberOfServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            PipeServerStream.BeginWaitForConnection((asyncResult) =>
            {
                PipeServerStream.EndWaitForConnection(asyncResult);
                StreamEncoding = new UnicodeEncoding();
                clientOnConnect?.Invoke(this);
                endConnectCallBack(this);
            }, null);
        }

        public bool SendMessage(object value)
        {
            return SendMessage(value, DefaultSerializerSettings);
        }

        public bool SendMessage(object value, JsonSerializerSettings serializerSettings)
        {
            if (value == null || serializerSettings == null)
            {
                return false;
            }
            var valueSerialized = JsonConvert.SerializeObject(value, serializerSettings);
            byte[] outBuffer = StreamEncoding.GetBytes(valueSerialized);
            int len = outBuffer.Length;
            var header = BitConverter.GetBytes(len);
            PipeServerStream.Write(header, 0, 4);
            PipeServerStream.Write(outBuffer, 0, len);
            PipeServerStream.Flush();
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
            var message = ReadMsg();
            return JsonConvert.DeserializeObject<T>(message, serializerSettings);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if(obj is RANamedPipeServerStreamInner)
            {
                var o = obj as RANamedPipeServerStreamInner;
                return o.GetHashCode() == GetHashCode();
            }
            return object.ReferenceEquals(obj, this);  //base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Uid.GetHashCode();
        }

        private string ReadMsg()
        {
            var header = new byte[4];
            int bRead = 0;
            while (bRead < header.Length)
            {
                int rd = PipeServerStream.Read(header, bRead, header.Length - bRead);
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
                int rd = PipeServerStream.Read(inBuffer, bRead, len - bRead);
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
            PipeServerStream?.Dispose();
        }
    }
}
