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
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using AvePoint.Media.Storage.Util;
    using AvePoint.GCommon;
    using System.Security.Principal;
    using AvePoint.GCommon.Utility.I18N;
    #endregion

    class AlphaFSStream : XStream
    {
        //private BufferedStream innerStream;
        FileStream innerStream;
        public FSClientOpenParam Param { get; set; }

        public AlphaFSStream(Alphaleonis.Win32.Filesystem.FileInfo alFileInfo, StorageInfo info, AbstractXSystem sys, FSClientOpenParam param)
            : base(sys)
        {

            this.innerStream = alFileInfo.Open(FileMode.OpenOrCreate);
            this.Param = param;
            this.URI.SdType = 0;
            this.URI.SysId = System.SystemID;
            this.URI.SInfo = info.Clone();
        }
        private StorageLogger logger = StorageLogger.GetInstance(typeof(AlphaFSStream));
        private string fileFullName;

        public AlphaFSStream(StorageInfo info, AbstractXSystem parentSys, FSClientOpenParam param, FileMode fileMode)
            : base(parentSys)
        {
            this.Param = param;
            string directoryPath = PathUtil.CombinePath(param.SystemLocation, info.HighName);

            fileFullName = PathUtil.CombinePath(directoryPath, info.LowName);
            Alphaleonis.Win32.Filesystem.FileInfo aFileInfo = new Alphaleonis.Win32.Filesystem.FileInfo(fileFullName);
            //FileStream innerStream = null;
            switch (fileMode)
            {
                case FileMode.Open:
                    try
                    {
                        innerStream = aFileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
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
                                    innerStream = aFileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
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
                            this.logger.Error("Opened the data failed, path: {0}.", fileFullName);
                            throw;
                        }
                    }
                    break;
                case FileMode.Append:
                    innerStream = aFileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                    break;
                case FileMode.OpenOrCreate://VDB uses this switch case, please contact VDB developers before changing this piece of code
                    try
                    {
                        innerStream = aFileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
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
                        innerStream = aFileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
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
            innerStream.Flush();
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
                EventIds.Storage.ReadFailedEventMessage readfailedEventMessage = new EventIds.Storage.ReadFailedEventMessage(PathUtil.CombinePath(System.SystemLocation, this.URI.SInfo.HighName), ContextValues.Storage.StorageType.FileSystem, e);
                this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.NetShare, readfailedEventMessage);
                logger.Error(e.Message, e);
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
                EventIds.Storage.WriteFailedEventMessage writeFailedEventMessage = new EventIds.Storage.WriteFailedEventMessage(PathUtil.CombinePath(System.SystemLocation, this.URI.SInfo.HighName), ContextValues.Storage.StorageType.FileSystem, e);
                this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.NetShare, writeFailedEventMessage);
                logger.Error(e.Message, e);
                throw;
            }
        }

        public override void Close()
        {
            if (innerStream != null)
            {
                //var fs = (innerStream as FileStream).GetAccessControl();

                //string sidString = AccountUtil.LookupSidByAccountName("10.2.26.111", "dlqa3", "yswang");
                //var i = new SecurityIdentifier(sidString);

                ////var ntAccount = new NTAccount((System as FSSystem).SystemDomain, (System as FSSystem).SystemUserName);

                //fs.SetOwner(i);
                ////fs.seto
                //(innerStream as FileStream).SetAccessControl(fs);

                innerStream.Close();
            }
        }

        public override XURIResult GetURI()
        {
            return this.URI;
        }
    }
}
