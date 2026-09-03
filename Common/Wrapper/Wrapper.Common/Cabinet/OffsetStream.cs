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


namespace AvePoint.Wrapper.Common
{
    using System;
    using System.IO;

    internal class OffsetStream : Stream
    {
        private long offset;
        private Stream source;

        public OffsetStream(Stream source, long offset)
        {
            if (source == null)
            {
                throw new ArgumentNullException();
            }
            this.source = source;
            this.offset = offset;
            this.source.Seek(offset, SeekOrigin.Current);
        }

        public override void Close()
        {
            this.source.Close();
        }

        public override void Flush()
        {
            this.source.Flush();
        }

        public override int Read(byte[] buffer, int start, int count)
        {
            return this.source.Read(buffer, start, count);
        }

        public override int ReadByte()
        {
            return this.source.ReadByte();
        }

        public override long Seek(long seekOffset, SeekOrigin seekOrigin)
        {
            return (this.source.Seek(seekOffset + ((seekOrigin == SeekOrigin.Begin) ? this.offset : 0L), seekOrigin) - this.offset);
        }

        public override void SetLength(long value)
        {
            this.source.SetLength(value + this.offset);
        }

        public override void Write(byte[] buffer, int start, int count)
        {
            this.source.Write(buffer, start, count);
        }

        public override void WriteByte(byte value)
        {
            this.source.WriteByte(value);
        }

        public override bool CanRead
        {
            get
            {
                return this.source.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return this.source.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.source.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return (this.source.Length - this.offset);
            }
        }

        public override long Position
        {
            get
            {
                return (this.source.Position - this.offset);
            }
            set
            {
                this.source.Position = value + this.offset;
            }
        }
    }
}

