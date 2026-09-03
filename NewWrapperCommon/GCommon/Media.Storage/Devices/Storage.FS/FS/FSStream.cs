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



namespace AvePoint.Media.Storage.FS
{
    #region using directives
    using AvePoint.Media.Storage.Util;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    #endregion

    class FSStream : XStream
    {
        //private BufferedStream innerStream;
        private Stream innerStream;
        public Stream InnerStream
        {
            set
            {
                this.innerStream = value;
            }
            get
            {
                return innerStream;
            }
        }

        public FSStream(FileStream innerStream, StorageInfo info, AbstractXSystem sys, Boolean useBuffer)
            : base(sys)
        {
            if (useBuffer)
            {

                this.innerStream = new BufferedStream(innerStream);
            }
            else
            {
                this.innerStream = innerStream;
            }
            this.URI.SdType = 0;
            this.URI.SysId = System.SystemID;
            this.URI.SInfo = info.Clone();
        }
        private StorageLogger logger = StorageLogger.GetInstance(typeof(FSStream));
        private string fileFullName;

        public FSStream(StorageInfo info, AbstractXSystem parentSys, FileMode fileMode)
            : base(parentSys)
        {
            string directoryPath = PathUtil.CombinePath(parentSys.SystemLocation, info.HighName);
            fileFullName = PathUtil.CombinePath(directoryPath, info.LowName);
            //FileStream innerStream = null;
            switch (fileMode)
            {
                case FileMode.Open:
                    try
                    {
                        innerStream = new FileStream(fileFullName, FileMode.Open, FileAccess.Read, FileShare.Read, info.BufferSize, info.FileOptions);
                    }
                    catch (Exception ex)
                    {
                        if ((System as FSSystem).ReadFailover)
                        {
                            logger.Error("OpenStream Failed in [{0}], begin do failover. Error:{1}", System.SystemLocation, ex);
                            int index = 0;
                            foreach (string readFailoverLocation in (System as FSSystem).ReadFailoverLocations)
                            {
                                directoryPath = PathUtil.CombinePath(readFailoverLocation, info.HighName);
                                fileFullName = PathUtil.CombinePath(directoryPath, info.LowName);
                                try
                                {
                                    index++;
                                    innerStream = new FileStream(fileFullName, FileMode.Open, FileAccess.Read, FileShare.Read, info.BufferSize, info.FileOptions);
                                    break;
                                }
                                catch (Exception e)
                                {
                                    logger.Error("OpenStream Failed in Failover Location [{0}], begin do failover next. Error:{1}", readFailoverLocation, e);
                                    if (index == (System as FSSystem).ReadFailoverLocations.Count)
                                    {
                                        this.logger.Error("Opened the data failed, path: {0}.", fileFullName);
                                        throw;
                                    }
                                }
                            }
                        }
                        else
                        {
                            this.logger.Error("Opened the data failed, path: [{0}].", fileFullName);
                            throw;
                        }
                    }
                    break;
                case FileMode.Append:
                    innerStream = new FileStream(fileFullName, FileMode.Open, info.FileAccess == 0 ? FileAccess.Write : info.FileAccess, FileShare.Read, info.BufferSize, info.FileOptions);
                    break;
                case FileMode.OpenOrCreate://VDB uses this switch case, please contact VDB developers before changing this piece of code
                    try
                    {
                        innerStream = new FileStream(fileFullName, FileMode.OpenOrCreate, info.FileAccess == 0 ? FileAccess.Write : info.FileAccess, FileShare.Read, info.BufferSize, info.FileOptions);
                    }
                    catch (FileNotFoundException ex)
                    {
                        throw new CatchedToDoMoreExcetion(ex.Message, ex);
                    }
                    catch (DirectoryNotFoundException ex)
                    {
                        throw new CatchedToDoMoreExcetion(ex.Message, ex);
                    }
                    break;
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.Truncate:
                    try
                    {
                        innerStream = new FileStream(fileFullName, fileMode, info.FileAccess == 0 ? FileAccess.Write : info.FileAccess, FileShare.Read, info.BufferSize, info.FileOptions);
                    }
                    catch (FileNotFoundException ex)
                    {
                        throw new CatchedToDoMoreExcetion(ex.Message, ex);
                    }
                    catch (DirectoryNotFoundException ex)
                    {
                        throw new CatchedToDoMoreExcetion(ex.Message, ex);
                    }
                    break;
                default:
                    throw new NotSupportedException("Unknown File Mode Type.");
            }
            //BufferedStream bufferStream = new BufferedStream(innerStream, info.BufferSize);
            //this.innerStream = bufferStream;
            this.System = parentSys;
            this.URI.SdType = 0;
            this.URI.SysId = System.SystemID;
            this.URI.SInfo = info.Clone();
        }

        protected override void Dispose(bool disposing)
        {
            innerStream.Dispose();
        }


        public override bool CanRead
        {
            get { return innerStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return innerStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return innerStream.CanWrite; }
        }

        public override void Flush()
        {
            innerStream.Flush(); ;
        }

        public override long Length
        {
            get { return innerStream.Length; }
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
            long startTicks = DateTime.UtcNow.Ticks;
            int readLen = 0;
            try
            {
                readLen = innerStream.Read(buffer, offset, count);
                System.IncreaseTotalReadTicks(DateTime.UtcNow.Ticks - startTicks);
                System.IncreaseTotalReadBytes(readLen);
            }
            catch (Exception e)
            {
                //EventIds.Storage.ReadFailedEventMessage readFailedEventMessage = new EventIds.Storage.ReadFailedEventMessage(PathUtil.CombinePath(System.SystemLocation, this.URI.SInfo.HighName),ContextValues.Storage.StorageType.FileSystem, e);
                //this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.NetShare, readFailedEventMessage);
                logger.Error("Read file {0} failed. Error: {1}", PathUtil.CombinePath(System.SystemLocation, this.URI.SInfo.HighPlusLowName), e);
                throw;
            }
            return readLen;
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            return innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            long startTicks = DateTime.UtcNow.Ticks;
            try
            {
                innerStream.Write(buffer, offset, count);
                System.IncreaseTotalWriteTicks(DateTime.UtcNow.Ticks - startTicks);
                System.IncreaseTotalWriteBytes(count);
            }
            catch (Exception e)
            {
                logger.Error("Write file {0} failed. Error: {1}", PathUtil.CombinePath(System.SystemLocation, this.URI.SInfo.HighPlusLowName), e);
                throw;
            }
        }

        public override void FlushFileBuffers()
        {
            if (innerStream != null && innerStream is FileStream)
            {
                var stream = innerStream as FileStream;
                if (!Win32API.FlushBuffers(stream.SafeFileHandle))
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    throw new System.ComponentModel.Win32Exception(errorCode, 
                        $"An error occurred while calling Win32 flush file Buffers api for {stream.Name}");
                }
            }
        }

        public override void Close()
        {
            if (innerStream != null)
            {
                innerStream.Close();
                innerStream = null;
            }
        }

        public override XURIResult GetURI()
        {
            return this.URI;
        }
    }
}
