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



namespace AvePoint.Media.Storage.CAStor
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Net.Sockets;
    using System.IO;
    using AvePoint.GCommon;
    using System.Reflection;
    using System.Threading;
    using System.Net;
    using AvePoint.Media.Storage.Util;
    using Scsp;
    using AvePoint.GCommon.Utility.I18N;
    using AvePoint.Media.Storage.Util;
    using System.Diagnostics;
    #endregion

    class CAStorStream : XStream
    {
        FileMode streamMode;
        Stream innerStream;
        CAStorClient client;
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        string objectId;
        public string LastMetaId { get; set; }
        //public string NextMetaId { get; set; }
        const string Temp_file_Location = @"..\temp\dell_temp";
        string tmpFilePath;
        const long MaxCacheStreamLength = 1024 * 1024 * 8;
        ScspResponse response;

        public CAStorStream(CAStorClient client, StorageInfo info, string lastMetaId, AbstractXSystem sys)
            : base(sys)
        {
            this.client = client;
            this.Info = info;
            this.URI.SdType = 1;
            this.URI.SysId = System.SystemID;
            this.URI.SInfo = info.Clone();
            this.LastMetaId = lastMetaId;
            if (!Directory.Exists(Temp_file_Location))
            {
                Directory.CreateDirectory(Temp_file_Location);
            }
            tmpFilePath = PathUtil.CombinePath(Temp_file_Location, Guid.NewGuid().ToString());
        }

        public CAStorStream(CAStorClient client, StorageInfo info, string lastMetaId, AbstractXSystem sys, Stream stream)
            : base(sys)
        {
            this.client = client;
            this.Info = info;
            this.URI.SdType = 1;
            this.URI.SysId = System.SystemID;
            this.URI.SInfo = info.Clone();
            this.LastMetaId = lastMetaId;
            this.innerStream = stream;
            this.streamMode = FileMode.Create;
        }

        public void InitStream(FileMode mode)
        {
            this.streamMode = mode;
            switch (mode)
            {
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.Append:
                case FileMode.OpenOrCreate:
                case FileMode.Truncate:
                    InitWriteStream(this.Info);
                    break;
                case FileMode.Open:
                    InitReadStream(this.Info);
                    break;
                default:
                    throw new Exception("Unsupported access type.");
            }
        }

        private void InitWriteStream(StorageInfo storageInfo)
        {
            CloseStream();
            if (storageInfo.Length > MaxCacheStreamLength)
            {
                this.innerStream = new FileStream(tmpFilePath, FileMode.OpenOrCreate);
            }
            else
            {
                this.innerStream = new MemoryStream();
            }
            if (this.innerStream == null)
            {
                throw new Exception("InitWriteStream(), innerStream is null.");
            }
        }

        public void InitReadStream(StorageInfo info)
        {
            try
            {
                long startTicks = DateTime.UtcNow.Ticks;
                CloseStream();
                if (this.Info.DataType == DataBlockType.MetaData)
                {
                    if (info.SkipNum > 0)
                    {
                        for (int i = 0; i < info.SkipNum; i++)
                        {
                            //info.ObjectId = client.GetNextMetaId(info);
                            info.ObjectId = (string)client.Invoke("GetNextMetaId", new object[] { info });
                        }
                        info.SkipNum = 0;
                        //this.NextMetaId = (string)client.Invoke("GetNextMetaId", new object[] { info });
                    }
                    else
                    {
                        //this.NextMetaId = client.GetNextMetaId(info);
                        //this.NextMetaId = (string)client.Invoke("GetNextMetaId", new object[] { info });
                    }
                }
                //this.response = client.InitReadStream(info, this.tmpFilePath);
                this.response = (ScspResponse)client.Invoke("InitReadStream", new object[] { info, this.tmpFilePath });
                this.innerStream = this.response.ResponseStream;
                if (this.innerStream == null)
                {
                    throw new Exception("InitReadStream(), innerStream is null.");
                }
                timeTotalRead += DateTime.UtcNow.Ticks - startTicks;

                //if (logger.IsDebugEnabled)
                //{
                //    logger.Debug("Open Object Time : " + (timeTotalRead / 10000.0F) + " S");
                //}
            }
            catch (Exception e)
            {
                this.logger.Error("Opened the object failed, ID: {0}.", info.ObjectId);
                logger.Error("Read data with primary node failed:" + e.Message, e);
                throw;
            }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
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
                throw new NotSupportedException();
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
                        long fileSize = (long)client.Invoke("GetFileSize", new object[] { this.Info });
                        StorageInfo storageInfo = new StorageInfo();
                        if (this.Info.Offset + this.ReadLength < fileSize)
                        {
                            logger.Info("reach stream end when file still has content, file size:{0}, offset:{1}, length:{2},readLen:{3}", fileSize, this.Info.Offset, this.Info.Length, this.ReadLength);
                            storageInfo.Offset = this.Info.Offset + this.ReadLength;
                            storageInfo.ObjectId = this.Info.ObjectId;
                            storageInfo.Length = this.Info.Length - this.ReadLength;
                            Thread.Sleep(1000);
                            this.InitReadStream(storageInfo);
                            return this.Read(buffer, offset, count);

                        }
                       
                        if (this.Info.DataType == DataBlockType.MetaData)
                        {
                            string id = (string)client.Invoke("GetNextMetaId", new object[] { this.Info });
                            logger.Info("reach the end of a meta file, we will change to a new meta file, id={0}", id);
                            storageInfo.Offset = 62 + 4096;
                            storageInfo.ObjectId = id;
                            this.Info.ObjectId = id;
                            storageInfo.Length = this.Info.Length - this.ReadLength;
                            this.InitReadStream(storageInfo);
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
                //EventIds.Storage.ReadFailedEventMessage readFailedEventMessage = new EventIds.Storage.ReadFailedEventMessage(this.URI.SInfo.ObjectId, ContextValues.Storage.StorageType.DELLDXStorage, e);
                //this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.DELL_DX_Storage, readFailedEventMessage);

                if (System != null)
                {
                    if (innerStream != null)
                    {
                        innerStream.Close();
                    }
                    innerStream = null;
                    innerStream = (System.OpenStream(Info, FileMode.Open) as CAStorStream).innerStream;
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
                //EventIds.Storage.WriteFailedEventMessage writeFailedEventMessage = new EventIds.Storage.WriteFailedEventMessage(this.URI.SInfo.ObjectId, ContextValues.Storage.StorageType.DELLDXStorage, e);
                //this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.DELL_DX_Storage, writeFailedEventMessage);

                logger.Error("Write file {0} failed, error message: {1},{2}", this.URI.SInfo.ObjectId, e.Message, e);
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
                        //this.ojectId = client.EndWriteStream(innerStream, this.Info);
                        if (innerStream == null)
                        {
                            throw new Exception("innerStream is null.");
                        }
                        this.objectId = (string)client.Invoke("EndWriteStream", new object[] { innerStream, this.Info });
                        if (this.Info.DataType == DataBlockType.MetaData)
                        {
                            Dictionary<string, string> headers = new Dictionary<string, string>();
                            headers.Add(CAStorConstants.META_ID_HEADER, this.objectId);
                            if (!string.IsNullOrEmpty(this.LastMetaId))
                            {
                                //if (!client.UpdateObjectMeta(this.LastMetaId, headers))
                                if (!(bool)client.Invoke("UpdateObjectMeta", new object[] { this.LastMetaId, headers }))
                                {
                                    throw new Exception("update meta info failed");
                                }
                            }
                            this.LastMetaId = this.objectId;
                        }
                        else
                        {
                            this.LastMetaId = null;
                        }
                        break;
                    case FileMode.Open:
                    case FileMode.Append:
                    case FileMode.Truncate:
                        break;
                    default:
                        throw new Exception("Unsupported access type.");
                }
                CAStorStorageInfo casInfo = new CAStorStorageInfo();
                casInfo.ContentId = this.objectId;
                rs.StorageInfo = CAStorUtil.Convert2StorageInfo(casInfo);
                //if (!string.IsNullOrEmpty(this.Info.ExtraStorageInfo))
                //{
                //    this.System.DeleteFile(new StorageInfo() { ObjectId = this.Info.ObjectId});
                //}
                return rs;
            }
            catch (Exception e)
            {
                EventIds.Storage.WriteFailedEventMessage writeFailedEventMessage = new EventIds.Storage.WriteFailedEventMessage(this.URI.SInfo.ObjectId, ContextValues.Storage.StorageType.DELLDXStorage, e);
                this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.DELL_DX_Storage, writeFailedEventMessage);

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

            if (File.Exists(tmpFilePath))
            {
                File.Delete(tmpFilePath);
            }

            try
            {
                if (sizeTotalWrite > 0)
                {
                    if (logger.IsDebugEnabled)
                    {
                        logger.Debug("Close Reading Stream, Object Id: " + Info.ObjectId+
                            "Close Reading Stream, TimeTotalRead:" + timeTotalRead+
                            "Close Reading Stream, SizeTotalRead:" + sizeTotalRead+
                            "Close Reading Stream, Speed:" + ((sizeTotalRead * 0.1 / 1024.0F / 1024.0F) / (timeTotalRead * 0.1 / 10000.0F)) + "M/S");
                    }
                }
                if (sizeTotalRead > 0)
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
                if (this.response != null)
                {
                    this.response.Close();
                    this.response = null;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("close stream error:" + ex.Message);
            }
        }
        public override XURIResult GetURI()
        {
            CAStorStorageInfo info = new CAStorStorageInfo();
            info.ContentId = this.objectId;
            info.MetaId = this.objectId;
            this.URI.SInfo.LowName = this.objectId;
            this.URI.SInfo.ExtraStorageInfo = CAStorUtil.Convert2StorageInfo(info);
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
    }
}
