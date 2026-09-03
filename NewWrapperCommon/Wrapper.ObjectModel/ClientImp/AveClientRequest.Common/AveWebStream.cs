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


using System.IO;
using System;

namespace AveClientRequest.Common
{
    public class AveWebStream : Stream
    {        
        private Stream m_Stream = null;
        private long m_Position = 0;
        private long m_Origin = 0;
        private DataMonitor m_DataMonitor;

        public AveWebStream(Stream stream, DataMonitor dataMonitor)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            this.m_Stream = stream;
            this.m_Position = 0;
            this.m_Origin = 0;
            this.m_DataMonitor = dataMonitor;
        }

        public override bool CanRead
        {
            get { return this.m_Stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return this.m_Stream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return this.m_Stream.CanWrite; }
        }

        public override void Flush()
        {
            this.m_Stream.Flush();
        }

        public override long Length
        {
            get 
            {
                return this.m_Stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return this.m_Position;
            }
            set
            {
                this.m_Stream.Position = value;
                this.m_Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesCount = this.m_Stream.Read(buffer, offset, count);
            m_Position += count;
            m_DataMonitor.ByteReceive += count;
            return bytesCount;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.m_Stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.m_Stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.m_Stream.Write(buffer, offset, count);
            m_Position += count;
            m_DataMonitor.ByteSend += count;
        }

        public override void Close()
        {
            this.m_Stream.Close();
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            this.m_Stream.Dispose();
            base.Dispose(disposing);
        }

        public override bool Equals(object obj)
        {
            return this.m_Stream.Equals(obj) || base.Equals(obj);
        }
    }
    public class DataMonitor
    {
        public long ByteSend;
        public long ByteReceive;
        public long ByteLastSend;    //for calculating bytes sended during one request
        public long ByteLastReceive;    //for calculating bytes received during one request
        public DataMonitor()
        {
            ByteSend = ByteReceive = 0;
            ByteLastSend = ByteLastReceive = 0;
        }

        public void RecordStream()
        {
            this.ByteLastReceive = this.ByteReceive;
            this.ByteLastSend = this.ByteSend;
        }
    }
}
