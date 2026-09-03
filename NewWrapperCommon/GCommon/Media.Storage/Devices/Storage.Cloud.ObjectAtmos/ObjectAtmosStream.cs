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
using AvePoint.GCommon;
using System.Reflection;
using System.Net;
using AvePoint.Media.Storage.Util;
using System.Threading;
using System.Diagnostics;

namespace AvePoint.Media.Storage.Cloud.ObjectAtmos
{
    class ObjectAtmosStream : XStream
    {
        FileMode streamMode;
        Stream innerStream;
        ObjectAtmosClient client;
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public string objectId { get; set; }
        HttpWebRequest request;
        public HttpWebResponse response { set; get; }
        long fileSize;
        delegate T RetryDelegate<T>();

        public ObjectAtmosStream(ObjectAtmosClient client, StorageInfo info, HttpWebRequest request, AbstractXSystem sys)
            : base(sys)
        {
            this.client = client;
            this.Info = info;
            this.URI.SdType = 1;
            this.URI.SysId = System.SystemID;
            this.URI.SInfo = info.Clone();
            this.request = request;
            this.fileSize = -1;
        }

        public ObjectAtmosStream(ObjectAtmosClient client, StorageInfo info, HttpWebResponse response, AbstractXSystem sys)
            : base(sys)
        {
            this.client = client;
            this.Info = info;
            this.URI.SdType = 1;
            this.URI.SysId = System.SystemID;
            this.response = response;
            this.URI.SInfo = info.Clone();
            this.fileSize = -1;
        }

        public void InitStream(FileMode mode)
        {
            this.streamMode = mode;
            CloseStream();
            switch (mode)
            {
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.Append:
                case FileMode.OpenOrCreate:
                case FileMode.Truncate:
                    Stream reqStream = client.GetRequestStream(request);
                    this.innerStream = new BufferedStream(reqStream, 64 * 1024);
                    break;
                case FileMode.Open:
                    CloseStream();
                this.innerStream = this.response.GetResponseStream();
                    break;
                default:
                    throw new Exception("Unsupported access type.");
            }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { return this.Info.Length; }
        }

        public override long Position
        {
            get
            {
                return this.ReadLength;
            }
            set
            {
                CloseStream();
                this.Info.Offset = value;
                this.innerStream = this.client.OpenObject(this.Info).GetResponseStream();
                logger.Info("open stream for set position, ID is {0}, offset is {1}", Info.ObjectId, this.Info.Offset);
            }
        }

        private long timeTotalRead;
        private long sizeTotalRead;

        public override int Read(byte[] buffer, int offset, int count)
        {
            long startTicks = DateTime.UtcNow.Ticks;
            int readLen = 0;
            try
            {
                while (readLen < count)
                {
                    int tempLen = innerStream.Read(buffer, offset + readLen, count - readLen);
                    readLen += tempLen;
                    if (readLen >= count || tempLen <= 0)
                    {
                        break;
                    }
                }

                if (readLen > 0)
                {
                    this.ReadLength += readLen;
                }
                else
                {
                    if (this.ReadLength < this.Info.Length)
                    {
                        if (fileSize < 0)
                        {
                            fileSize = client.GetObjectInfo(this.Info).FileSize ;
                        }
                        StorageInfo storageInfo = new StorageInfo();
                        if (this.Info.Offset + this.ReadLength < fileSize)
                        {
                            logger.Info("reach stream end when file still has content, file size:{0}, offset:{1}, length:{2},readLen:{3}", fileSize, this.Info.Offset, this.Info.Length, this.ReadLength);
                            storageInfo.Offset = this.Info.Offset + this.ReadLength;
                            storageInfo.ObjectId = this.Info.ObjectId;
                            storageInfo.Length = this.Info.Length - this.ReadLength;
                            Thread.Sleep(1000);
                            this.innerStream = (this.System.OpenStream(storageInfo, streamMode) as ObjectAtmosStream).innerStream;
                            return this.Read(buffer, offset, count);

                        }
                    }
                }

                timeTotalRead += DateTime.UtcNow.Ticks - startTicks;
                sizeTotalRead += readLen;

                System.IncreaseTotalReadTicks(DateTime.UtcNow.Ticks - startTicks);
                System.IncreaseTotalReadBytes(readLen);
            }
            catch (IDNullException ex)
            {
                logger.Error(ex.Message, ex);
                throw;
            }
            catch (IOException e)
            {
                logger.Error("Read file {0} failed, error message: {1},{2}", this.URI.SInfo.ObjectId, e.Message, e);
                if (System != null)
                {
                    if (innerStream != null)
                    {
                        innerStream.Close();
                    }
                    innerStream = null;
                    (this.System as ObjectAtmosSystem).RemoveDeadHost(this.client.Endpoint);
                    innerStream = (System.OpenStream(Info, FileMode.Open) as ObjectAtmosStream).innerStream;
                    if (innerStream != null)
                    {
                        logger.Debug("innerStream is not null.");
                        return this.Read(buffer, offset, count);
                    }
                    else
                    {
                        logger.Debug("innerStream is null.");
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
            return readLen;
        }

        private long timeTotalWrite;
        private long sizeTotalWrite;

        public override void Write(byte[] buffer, int offset, int count)
        {
            long startTicks = DateTime.UtcNow.Ticks;
            try
            {
                this.innerStream.Write(buffer, offset, count);
                timeTotalWrite += DateTime.UtcNow.Ticks - startTicks;
                sizeTotalWrite += count;
                System.IncreaseTotalWriteTicks(DateTime.UtcNow.Ticks - startTicks);
                System.IncreaseTotalWriteBytes(count);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw;
            }
        }

        public override StorageResult Commit(bool closeParent)
        {
            try
            {
                StorageResult rs = new StorageResult();

                switch (this.streamMode)
                {
                    case FileMode.Create:
                    case FileMode.CreateNew:
                    case FileMode.OpenOrCreate:
                        if (this.innerStream != null)
                        {
                            this.innerStream.Close();
                        }
                        this.objectId = client.EndWriteStream(this.request, this.Info);
                        break;
                    case FileMode.Open:
                    case FileMode.Append:
                    case FileMode.Truncate:
                        break;
                    default:
                        throw new Exception("Unsupported access type.");
                }
                ObjectAtmosStorageInfo casInfo = new ObjectAtmosStorageInfo();
                casInfo.ContentId = this.objectId;
                rs.StorageInfo = ObjectAtmosUtil.Convert2StorageInfo(casInfo);
                return rs;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw;
            }
            finally
            {
                if (innerStream != null)
                {
                    innerStream.Close();
                    innerStream = null;
                }
            }
        }

        public override void Flush()
        {
        }

        public override void Close()
        {
            base.Close();
            CloseStream();

            try
            {
                if (sizeTotalRead > 0)
                {
                    if (logger.IsDebugEnabled)
                    {
                        logger.Debug("Close Reading Stream, Object Id: " + Info.ObjectId+
                            "Close Reading Stream, TimeTotalRead:" + timeTotalRead+
                            "Close Reading Stream, SizeTotalRead:" + sizeTotalRead+
                            "Close Reading Stream, Speed:" + ((sizeTotalRead * 0.1 / 1024.0F / 1024.0F) / (timeTotalRead * 0.1 / 10000.0F)) + "M/S");
                    }
                }
                if (sizeTotalWrite > 0)
                {
                    if (logger.IsDebugEnabled)
                    {
                        logger.Debug("Close Writing Stream, Object Id: " + Info.ObjectId+
                            "Close Writing Stream, TimeTotalWrite:" + timeTotalWrite+
                            "Close Writing Stream, SizeTotalWrite:" + sizeTotalWrite+
                            "Close Writing Stream, Speed:" + ((sizeTotalWrite * 0.1 / 1024.0F / 1024.0F) / (timeTotalWrite * 0.1 / 10000.0F)) + "M/S");
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.Message);
            }
        }

        private void CloseStream()
        {
            try
            {
                if (this.innerStream != null)
                {
                    this.innerStream.Close();
                    this.innerStream = null;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("close stream error:" + ex.Message);
            }
        }
        public override XURIResult GetURI()
        {
            ObjectAtmosStorageInfo info = new ObjectAtmosStorageInfo();
            info.ContentId = this.objectId;
            info.MetaId = this.objectId;
            this.URI.SInfo.LowName = this.objectId;
            this.URI.SInfo.ExtraStorageInfo = ObjectAtmosUtil.Convert2StorageInfo(info);
            return this.URI;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (this.response != null)
            {
                this.response.Close();
                this.response = null;
            }
            if (this.request != null)
            {
                this.request.Abort();
                this.request = null;
            }
            base.Dispose(disposing);
        }
    }
}
