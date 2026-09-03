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

namespace AvePoint.Media.Storage.Cloud.Common
{
    #region using directives
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;

    #endregion
    class HttpDownloadStream : CloudStream
    {
        private StorageLogger logger = StorageLogger.GetInstance(typeof(HttpDownloadStream));
        public StorageLogger Logger { get { return logger; } }
        public HttpWebResponse Response { get; set; }
        public long InnerLength { get; protected set; }
        long totalReadLength = 0;
        public long NowOffset { set; get; }
        public long NowLenght { set; get; }


        public override long Length
        {
            get
            {
                return this.InnerLength;
            }
        }

        public override bool CanRead
        {
            get
            {
                return InnerStream.CanRead;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return InnerStream.CanWrite;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public override Int64 Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    this.Position = offset;
                    break;
                case SeekOrigin.Current:
                    this.Position = this.Position + offset;
                    break;
                case SeekOrigin.End:
                    this.Position = this.System.OpenFile(this.Info).FileSize - offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("origin");
            }
            return this.Position;
        }

        public override long Position
        {
            get
            {
                return this.ReadLength + this.Info.Offset;
            }
            set
            {
                this.Close();
                this.Info.Offset = value;
                this.Info.Length = this.InnerLength - value;
                this.InnerStream = this.System.OpenStream(this.Info, FileMode.Open).InnerStream;
                Logger.Debug("Open stream for set position, offset is {0}", this.Info.Offset);
            }
        }

        public override void Flush()
        {
            InnerStream.Flush();
        }

        public HttpDownloadStream(HttpWebResponse response)
        {
            if (HttpStatusCode.OK == response.StatusCode || HttpStatusCode.PartialContent == response.StatusCode)
            {
                Stream respStream = response.GetResponseStream();
                InnerStream = new BufferedStream(respStream, 64 * 1024);
            }
            else
            {
                throw new Exception(string.Format("Open Http Down Stream Error, Error Code : {0}", response.StatusCode));
            }
            this.Response = response;
            this.InnerLength = response.ContentLength;
            if (this.InnerLength == -1)
            {
                string meta = response.Headers.Get("x-emc-meta");
                if (!string.IsNullOrEmpty(meta))
                {
                    string[] metaBuffer = meta.Split(',');
                    foreach (string value in metaBuffer)
                    {
                        if (value.Contains("size"))
                        {
                            this.InnerLength = long.Parse(value.Substring(value.IndexOf('=') + 1));
                            break;
                        }
                    }
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long startTicks = DateTime.UtcNow.Ticks;
            StorageInfo tempInfo = Info.Clone();
            int readLen = 0;
            try
            {
                while (readLen < count)
                {
                    int tempLen = InnerStream.Read(buffer, offset + readLen, count - readLen);
                    readLen += tempLen;
                    totalReadLength += tempLen;
                    if (readLen >= count || (tempLen <= 0 && totalReadLength >= Info.Length))
                    {
                        Info.CurrentRetryCount = 0;
                        break;
                    }
                    if (tempLen <= 0 && totalReadLength < Info.Length)
                    {
                        throw new Exception("The result of InnerStream.read is 0, either server read exception or info.Length is inexactly, info.length: " + Info.Length);
                    }
                }
                System.IncreaseTotalReadTicks(DateTime.UtcNow.Ticks - startTicks);
                System.IncreaseTotalReadBytes(readLen);
            }
            catch (Exception e)
            {
                Logger.Warn("Error Occurred during reading content (" + "R" + "ETRING".ToLower(CultureInfo.InvariantCulture) + " : " + Info.CurrentRetryCount + " ):" + e.Message, e);
                if (Info.CurrentRetryCount < MaxRetryCount)
                {
                    if (InnerStream != null)
                    {
                        InnerStream.Close();
                    }
                    InnerStream = null;
                    tempInfo.Offset += totalReadLength;
                    tempInfo.Length -= totalReadLength;
                    InnerStream = (System.OpenStream(tempInfo, FileMode.Open) as HttpDownloadStream).InnerStream;
                    return Read(buffer, offset + readLen, count - readLen) + readLen;
                }
                else
                {
                    SetEventTaskInfo(System);
                    Logger.Error("Read file {0} failed, error message: {1},{2}", this.HttpWebRequest.RequestUri.AbsoluteUri.ToString(), e.Message, e);
                    throw;
                }
            }
            this.ReadLength += readLen;
            return readLen;
        }

        public override void ClosedUnmoral()
        {
            if (Response != null)
            {
                Response.Close();
            }
            if (InnerStream != null)
            {
                InnerStream.Close();
            }
        }

        public override void Close()
        {
            this.ReadLength = 0;
            if (Response != null)
            {
                Response.Close();
            }
            if (InnerStream != null)
            {
                InnerStream.Close();
            }
        }
    }
}
