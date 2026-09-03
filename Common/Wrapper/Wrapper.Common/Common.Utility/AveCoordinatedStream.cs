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
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;

namespace AvePoint.Wrapper.Common
{
    public class AveCoordinatedStream : Stream, IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(AveCoordinatedStream));

        private int StreamSizeLimit = 64 * 1024;
        private Stream innerStream;
        private bool mExplictlyClose = false;
        private readonly string mInternalName;
        private string mFileName = string.Empty;
        private string mTempPath = string.Empty;
        private readonly long order;

        /// <summary>
        /// 调用此类时，需要注意当Stream大于64K时，需要手动将Stream翻译，否则会遗留临时文件
        /// </summary>
        /// <param name="fileName"></param>
        public AveCoordinatedStream(string internalName=null, long order=0, bool explictlyClose = false, int streamSizeLimit = 64 * 1024)
        {
            innerStream = new MemoryStream(StreamSizeLimit);
            this.mExplictlyClose = explictlyClose;
            this.mInternalName = internalName;
            StreamSizeLimit = streamSizeLimit;
            this.order = order;
        }

        public bool IsExplictlyClose
        {
            get
            {
                return mExplictlyClose;
            }
        }

        public long Order
        {
            get { return order; }
        }

        private string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(this.mFileName))
                {
                    this.mFileName = AveLogger.JobId + mInternalName + Guid.NewGuid().ToString();
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

        //bind some object here, ie:report
        public object Attachment
        {
            get;
            set;
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
                mTempPath = GCommon.Utility.SecurityUtils.SafeCombinePath(WrapperConfiguration.TempDirectory, this.FileName);
                //log.Info($"Create File Stream at Path:{mTempPath}");
                FileStream largeContent = new FileStream(mTempPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                if (innerStream.Length > 0 && innerStream.Position >= 0)
                {
                    byte[] tmpBuffer = (innerStream as MemoryStream).GetBuffer();
                    largeContent.Write(tmpBuffer, 0, (int)innerStream.Position);
                }
                innerStream = largeContent;
            }
            innerStream.Write(buffer, offset, count);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public bool IsEndOfStream
        {
            get { return (this.Position == this.Length); }
        }

        protected override void Dispose(bool disposing)
        {
            if (!mExplictlyClose)
            {
                ExplictlyClose();
                mExplictlyClose = true;
            }
        }

        public void ExplictlyCloseAndKeepFile()
        {
            if (innerStream != null)
            {
                innerStream.Dispose();
                innerStream = null;
                //CleanStreamFile();
                log.Warn("The file:{0} is kept for future analysis", mTempPath);
            }
            base.Dispose(true);
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
            if (!string.IsNullOrEmpty(mTempPath) && System.IO.File.Exists(mTempPath))
            {
                //log.Info("Delete temp file:{0}", mTempPath);
                System.IO.File.Delete(mTempPath);
            }
        }

        public override string ToString()
        {
            return mFileName;
        }
    }
}
