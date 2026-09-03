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
using AvePoint.Common;

namespace AvePoint.Wrapper.Common
{
    public class AveCoordinatedStream : Stream, IDisposable
    {
        public readonly static int StreamSizeLimit = 64 * 1024;
        private Stream innerStream;
        private string mFileName = string.Empty;
        private bool explictlyClose = false;

        /// <summary>
        /// 调用此类时，需要注意当Stream大于64K时，需要手动将Stream翻译，否则会遗留临时文件
        /// </summary>
        /// <param name="fileName"></param>
        public AveCoordinatedStream()
        {
            innerStream = new MemoryStream(StreamSizeLimit);
        }

        public AveCoordinatedStream(bool explictlyClose)
            : this()
        {
            this.explictlyClose = explictlyClose;
        }
        private string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(this.mFileName))
                {
                    this.mFileName = Guid.NewGuid().ToString();
                }
                return this.mFileName;
            }
        }

        public override bool CanRead
        {
            get
            {
                return innerStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return innerStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return innerStream.CanWrite;
            }
        }

        public override void Flush()
        {
            innerStream.Flush();
        }

        public override long Length
        {
            get
            {
                return innerStream.Length;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            innerStream.SetLength(value);
        }

        public override long Position
        {
            get
            {
                return innerStream.Position;
            }
            set
            {
                innerStream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return innerStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
            }
            if (innerStream == null)
            {
                innerStream = new MemoryStream(StreamSizeLimit);
            }
            if (innerStream.GetType().Equals(typeof(MemoryStream)))
            {
                if (innerStream.Position + count < StreamSizeLimit)
                {
                    innerStream.Write(buffer, offset, count);
                    return;
                }
                string filePath = AveWrapperConstants.WrapperTempFolder.TrimEnd('\\') + "\\" + this.FileName;
                FileStream largeContent = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                if (innerStream.Position > 0)
                {
                    byte[] tmpBuffer = (innerStream as MemoryStream).GetBuffer();
                    largeContent.Write(tmpBuffer, 0, (int)innerStream.Position);
                }
                innerStream = largeContent;
            }
            innerStream.Write(buffer, offset, count);
        }

        new public void Dispose()
        {
            Dispose(true);
        }

        public bool IsEndOfStream
        {
            get { return (this.Position == this.Length); }
        }

        protected override void Dispose(bool disposing)
        {
            if (!explictlyClose)
            {
                ExplictlyClose();
            }
        }

        public void ExplictlyClose()
        {
            if (innerStream != null)
            {
                innerStream.Dispose();
                innerStream = null;
                CleanStreamFile();
            }
            base.Dispose(true);
        }

        private void CleanStreamFile()
        {
            string filePath = AveWrapperConstants.WrapperTempFolder.TrimEnd('\\') + "\\" + this.FileName;
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}
