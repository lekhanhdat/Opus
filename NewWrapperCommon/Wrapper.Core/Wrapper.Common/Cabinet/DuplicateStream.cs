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

    internal class DuplicateStream : Stream
    {
        private long position;
        private Stream source;

        public DuplicateStream(Stream source)
        {
            if (source == null)
            {
                throw new ArgumentNullException();
            }
            this.source = OriginalStream(source);
        }

        public override void Close()
        {
            this.source.Close();
        }

        public override void Flush()
        {
            this.source.Flush();
        }

        public static Stream OriginalStream(Stream stream)
        {
            DuplicateStream stream2 = stream as DuplicateStream;
            if (stream2 == null)
            {
                return stream;
            }
            return stream2.Source;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long position = this.source.Position;
            this.source.Position = this.position;
            int num2 = this.source.Read(buffer, offset, count);
            this.position = this.source.Position;
            this.source.Position = position;
            return num2;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long position = 0L;
            if (origin == SeekOrigin.Current)
            {
                position = this.position;
            }
            else if (origin == SeekOrigin.End)
            {
                position = this.Length;
            }
            this.position = position + offset;
            return this.position;
        }

        public override void SetLength(long value)
        {
            this.source.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            long position = this.source.Position;
            this.source.Position = this.position;
            this.source.Write(buffer, offset, count);
            this.position = this.source.Position;
            this.source.Position = position;
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
                return this.source.Length;
            }
        }

        public override long Position
        {
            get
            {
                return this.position;
            }
            set
            {
                this.position = value;
            }
        }

        public Stream Source
        {
            get
            {
                return this.source;
            }
        }
    }
}

