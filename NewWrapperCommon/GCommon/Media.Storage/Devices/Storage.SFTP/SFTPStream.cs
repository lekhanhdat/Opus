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




using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.SFTP.SFTPStream.#InitWriteStream(AvePoint.Media.Storage.StorageInfo,System.IO.FileMode)", MessageId = "sftp")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.SFTP.SFTPStream.#InitReadStream(AvePoint.Media.Storage.StorageInfo)", MessageId = "sftp")]
namespace AvePoint.Media.Storage.SFTP
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
    using AvePoint.GCommon.Utility.I18N;
    using AvePoint.Media.Storage.Util;
    using AvePoint.GCommon.Contract.CodeReview;
    using System.Diagnostics;
    #endregion

    #region CodeReview
    #endregion

    class SFTPStream : XStream
    {
        SFTPClient sftpClient;
        FileMode streamMode;
        Stream input;
        Stream output;
        StorageLogger logger = new StorageLogger(MethodBase.GetCurrentMethod().DeclaringType);
        byte[] readBuffer;
        int readBufferPosition;
        int readBufferLength;

        public SFTPStream(SFTPClient sftpClient, StorageInfo info, AbstractXSystem sys) : base(sys)
        {
            this.sftpClient = sftpClient;
            this.Info = info;
            this.URI.SdType = 1;
            this.URI.SysId = System.SystemID;
            this.URI.SInfo = info.Clone();
        }

        public void InitStream(FileMode mode)
        {
            streamMode = mode;
            switch (mode)
            {
                case FileMode.CreateNew:
                case FileMode.Create:
                case FileMode.OpenOrCreate:
                    InitWriteStream(this.Info, mode);
                    break;
                case FileMode.Open:
                    InitReadStream(this.Info);
                    break;
                case FileMode.Append:
                    InitWriteStream(this.Info, mode);
                    break;
                case FileMode.Truncate:
                default:
                    throw new Exception("Unsupported access type.");
            }
        }

        public void InitReadStream(StorageInfo info)
        {
            try
            {
                logger.Info("open stream for file, path:" + PathUtil.CombinePath(info.HighName, info.LowName));
                if (!sftpClient.CheckFileExist(PathUtil.CombinePath(info.HighName, info.LowName)))
                {
                    throw new Exception("storage node can not be found " + this.Info);
                }
                CloseInputStream();
                input = sftpClient.GetDownloadStream(PathUtil.CombinePath(info.HighName, info.LowName));
                if (input == null)
                {
                    logger.Error(string.Format("Init sftp download stream error {0}", this.Info));
                    throw new Exception("Init sftp download stream error ");
                }
                readBuffer = new byte[4 * 1024 * 1024];
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw;
            }
        }

        public void InitWriteStream(StorageInfo info, FileMode mode)
        {
            try
            {
                logger.Info("create stream for file, path:" + PathUtil.CombinePath(info.HighName, info.LowName));
                if (!sftpClient.CheckFileExist(PathUtil.CombinePath(info.HighName, info.LowName)))
                {
                    if (!sftpClient.CheckDirectory(info.HighName))
                    {
                        sftpClient.MakeDirectory(info.HighName);
                    }
                }
                else
                {
                    sftpClient.DeleteFile(info.HighPlusLowName);
                }
                CloseOutputStream();
                output = this.sftpClient.GetUploadStream(PathUtil.CombinePath(info.HighName, info.LowName), mode);
                if (output == null)
                {
                    logger.Error(string.Format("Init sftp upload stream error {0}", this.Info));
                    throw new Exception("Init sftp upload stream error ");
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw;
            }
        }

        public override bool CanRead
        {
            get { return input.CanRead; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return output.CanWrite; }
        }

        public override void Flush()
        {
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
                throw new NotImplementedException();
            }
        }

        public int Read0(byte[] buffer, int offset, int count)
        {
            long startTicks = DateTime.UtcNow.Ticks;
            int readLen = 0;
            try
            {
                readLen = input.Read(buffer, offset, count);
                System.IncreaseTotalReadTicks(DateTime.UtcNow.Ticks - startTicks);
                System.IncreaseTotalReadBytes(readLen);

                if (readLen >= 0)
                {
                    this.ReadLength += readLen;
                }
            }
            catch (Exception e)
            {
                if (e.InnerException != null && e.InnerException is SocketException)
                {
                    //SocketException se = e.InnerException as SocketException;
                    //if (se.SocketErrorCode == SocketError.ConnectionReset) //unplug network throw ConnectionReset exception                  
                    //EventIds.Storage.ReadFailedEventMessage readFailedEventMessage = new EventIds.Storage.ReadFailedEventMessage(PathUtil.CombinePath(System.SystemLocation, this.URI.SInfo.HighName), ContextValues.Storage.StorageType.FTP, e);                   
                    //this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.FTP, readFailedEventMessage);
                }
                logger.Error("Read file {0} failed, error message: {1},{2}", PathUtil.CombinePath(System.SystemLocation, this.URI.SInfo.HighPlusLowName), e.Message, e);
                throw;
            }
            return readLen;
        }

        bool eof = false;
        public override int Read(byte[] buffer, int offset, int count)
        {
            long startTicks = DateTime.UtcNow.Ticks;
            int readLen = 0;
            try
            {
                if (eof)
                {
                    return 0;
                }
                if (count >= readBuffer.Length)
                {
                    readLen = input.Read(buffer, offset, count);
                }
                else
                {
                    if (readBufferLength >= count)
                    {
                        Array.Copy(readBuffer, readBufferPosition, buffer, offset, count);
                        readBufferPosition = readBufferPosition + count;
                        readBufferLength = readBufferLength - count;
                        readLen = count;
                    }
                    else
                    {
                        Array.Copy(readBuffer, readBufferPosition, buffer, offset, readBufferLength);
                        int remainSize = count - readBufferLength;
                        offset = offset + readBufferLength;
                        readLen = readLen + readBufferLength;

                        readBufferLength = input.Read(readBuffer, 0, readBuffer.Length);
                        readBufferPosition = 0;

                        if (readBufferLength >= remainSize)
                        {
                            Array.Copy(readBuffer, readBufferPosition, buffer, offset, remainSize);
                            readBufferPosition = readBufferPosition + remainSize;
                            readBufferLength = readBufferLength - remainSize;
                            readLen = readLen + remainSize;
                        }
                        else
                        {
                            Array.Copy(readBuffer, readBufferPosition, buffer, offset, readBufferLength);
                            readBufferPosition = readBufferPosition + readBufferLength;
                            readLen = readLen + readBufferLength;
                            readBufferLength = 0;
                            eof = true;
                        }

                    }
                }

                if (readLen >= 0)
                {
                    this.ReadLength += readLen;
                }

                System.IncreaseTotalReadTicks(DateTime.UtcNow.Ticks - startTicks);
                System.IncreaseTotalReadBytes(readLen);
            }
            catch (Exception e)
            {
                if (e.InnerException != null && e.InnerException is SocketException)
                {
                    //SocketException se = e.InnerException as SocketException;
                    //if (se.SocketErrorCode == SocketError.ConnectionReset) //unplug network throw ConnectionReset exception                  
                    //EventIds.Storage.ReadFailedEventMessage readFailedEventMessage = new EventIds.Storage.ReadFailedEventMessage(PathUtil.CombinePath(System.SystemLocation, this.URI.SInfo.HighName), ContextValues.Storage.StorageType.FTP, e);                   
                    //this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.FTP, readFailedEventMessage);
                }
                logger.Error("Read file {0} failed, error message: {1},{2}", PathUtil.CombinePath(System.SystemLocation, this.URI.SInfo.HighPlusLowName), e.Message, e);
                throw;
            }
            return readLen;
        }

        private void FullFill()
        {

        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                lock (output)
                {
                    long startTicks = DateTime.UtcNow.Ticks;
                    output.Write(buffer, offset, count);
                    System.IncreaseTotalWriteTicks(DateTime.UtcNow.Ticks - startTicks);
                    System.IncreaseTotalWriteBytes(count);
                }
            }
            catch (Exception e)
            {
                if (e.InnerException != null && e.InnerException is SocketException)
                {
                    //SocketException se = e.InnerException as SocketException;
                    //if (se.SocketErrorCode == SocketError.ConnectionReset) //unplug network throw ConnectionReset exception
                    //EventIds.Storage.WriteFailedEventMessage writeFailedEventMessage = new EventIds.Storage.WriteFailedEventMessage(PathUtil.CombinePath(System.SystemLocation, this.URI.SInfo.HighName),ContextValues.Storage.StorageType.FTP, e);                    
                    //this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.FTP, writeFailedEventMessage);
                }
                logger.Error("Write file {0} failed, error message: {1},{2}", PathUtil.CombinePath(System.SystemLocation, this.URI.SInfo.HighPlusLowName), e.Message, e);
                throw;
            }
        }

        public void CloseOutputStream()
        {
            if (output != null)
            {
                output.Close();
                output = null;
            }
        }

        public void CloseInputStream()
        {
            try
            {
                if (input != null)
                {
                    input.Close();
                    input = null;
                }
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.Message);
            }
        }

        public override void Close()
        {
            switch (streamMode)
            {
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.OpenOrCreate:
                    CloseOutputStream();
                    break;
                case FileMode.Open:
                    CloseInputStream();
                    break;
                case FileMode.Append:
                case FileMode.Truncate:
                default:
                    throw new Exception("Unsupported access type.");
            }
        }

        public override XURIResult GetURI()
        {
            return this.URI;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
    }
}
