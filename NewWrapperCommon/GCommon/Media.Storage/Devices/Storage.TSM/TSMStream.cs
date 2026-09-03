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

namespace AvePoint.Media.Storage.TSM
{
    #region using directives
    using System;
    using System.IO;
    using System.Threading;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Utility.I18N;
    using AvePoint.Media.Storage.Util;
    #endregion

    #region CodeReview
    [AveCodeReview(
    "2012/2/22",
    "rongbiao.sun@avepoint.com",
    "dapeng.zhang@avepoint.com",
    new string[] { },
    null,
    true)]
    #endregion
    class TSMStream : XStream
    {
        TSMClient client;
        TSMSession session;
        FileMode fileMode;
        TSMNodeInfo nodeInfo;
        StorageLogger logger;
        Mutex mutex;
        object mutexLocker = new object();
        /// <summary>
        /// TSM Stream Constructor.
        /// </summary>
        /// <param name="client">The current communication TSMClient.</param>
        /// <param name="session">The current communication session with TSM</param>
        /// <param name="info">The infomation of storage</param>
        /// <param name="nodeInfo">The infomation of TSM node.</param>
        /// <param name="fileMode">The open mode.</param>
        /// <param name="sys">Which system belong to.</param>
        public TSMStream(TSMClient client, TSMSession session, StorageInfo info, TSMNodeInfo nodeInfo, FileMode fileMode, TSMSystem sys, Mutex mutex)
            : base(sys)
        {
            this.client = client;
            this.Info = info;
            this.fileMode = fileMode;
            this.nodeInfo = nodeInfo;
            this.session = session;
            this.URI.SdType = 2;
            this.URI.SysId = this.System.SystemID;
            this.URI.SInfo = info.Clone();
            this.mutex = mutex;
            logger = StorageLogger.GetInstance(this.GetType());
        }

        /// <summary>
        /// Starts a restore or retrieve operation.
        /// </summary>
        /// <param name="info">The infomation of Storage.</param>
        public override void BeginRead(StorageInfo info)
        {
            if (this.session.State == 0)
            {
                try
                {
                    var tsmInfo = TSMUtil.FormateTsmNode(info);
                    this.client.BeginRead(this.session, tsmInfo.HighName, tsmInfo.LowName, info.Offset, info.Length);
                }
                catch (Exception e)
                {
                    ReleaseMutex();
                    logger.Error("Opened the data failed, path: {0}.", PathUtil.CombinePath(this.Info.HighName, this.Info.LowName));
                    logger.Error(e.Message, e);
                    throw;
                }
            }
            else
            {
                logger.Error("BeginRead failed ,session state is :" + this.session.State);
            }
        }

        /// <summary>
        /// Ends a read stream that obtains data from storage.
        /// </summary>
        public override void EndRead()
        {
            try
            {
                if (this.session.State == 4)
                {
                    this.client.EndRead(this.session);
                }
                else
                {
                    logger.Error("EndRead failed ,session state is :" + this.session.State);
                }
            }
            catch (Exception e)
            {
                logger.Error("End read error, details {0}.", e);
                ReleaseMutex();
                throw;
            }
        }

        /// <summary>
        /// Initialized a stream depending on the type of file mode.
        /// </summary>
        public void InitStream()
        {
            if (this.mutex != null)
            {
                this.mutex.WaitOne();
            }
            try
            {
                switch (this.fileMode)
                {
                    case FileMode.Create:
                    case FileMode.CreateNew:
                    case FileMode.Append:
                    case FileMode.OpenOrCreate:
                    case FileMode.Truncate:
                        this.client.BeginWrite(this.session, this.Info.HighName, this.Info.LowName);
                        break;
                    case FileMode.Open:
                        break;
                    default:
                        throw new Exception("Unsupported mode:" + this.fileMode);
                }
            }
            catch (Exception e)
            {
                logger.Error("Init stream error: {0}.", e);
                ReleaseMutex();
                throw;
            }
        }

        /// <summary>
        /// Commit data to the server.
        /// </summary>
        /// <param name="closeParent">Whether or not to close the parent stream.</param>
        /// <returns>The result indicating success or not.</returns>
        public override StorageResult Commit(bool closeParent)
        {
            return Commit();
        }

        /// <summary>
        /// Commit data to the server.
        /// </summary>
        /// <returns>The result indicating success or not.</returns>
        public override StorageResult Commit()
        {
            var rs = new StorageResult();
            try
            {
                switch (this.fileMode)
                {
                    case FileMode.Create:
                    case FileMode.CreateNew:
                    case FileMode.Append:
                    case FileMode.OpenOrCreate:
                    case FileMode.Truncate:
                        this.client.EndWrite(this.session);
                        break;
                    case FileMode.Open:
                        break;
                    default:
                        throw new Exception("Unsupported access type.");
                }
            }
            catch (Exception e)
            {
                ReleaseMutex();
                var writeFailedEventMessage = new EventIds.Storage.WriteFailedEventMessage(PathUtil.CombinePath(this.System.SystemLocation, this.URI.SInfo.HighName), ContextValues.Storage.StorageType.TSM, e);
                logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.TSM, writeFailedEventMessage);
                logger.Error(e.Message, e);
                throw;
            }
            return rs;
        }

        /// <summary>
        /// Send all data in stream to the server.
        /// </summary>
        public override void Flush()
        {
            //tsm not use
        }

        /// <summary>
        /// 调用者使用完之后，必须调用此方法关闭此流。
        /// </summary>
        public override void Close()
        {
            try
            {
                if (this.session.State != 0)
                {
                    if (this.session.State == 4)
                    {
                        EndRead();
                    }
                    if (this.session.State == 2)
                    {
                        this.client.EndWrite(this.session);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("close error:" + e.Message, e);
                throw;
            }
            finally
            {
                base.Close();
                ReleaseMutex();
            }
        }

        private void ReleaseMutex()
        {
            lock (this.mutexLocker)
            {
                if (this.mutex != null)
                {
                    this.mutex.ReleaseMutex();
                }
            }
        }

        public override Int64 Length
        {
            get
            {
                return this.Info.Length;
            }
        }

        public override Int64 Position
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

        /// <summary>
        /// Read a byte stream of data from server and places it in the caller’s buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset of started reading.</param>
        /// <param name="count">The total count of read.</param>
        /// <returns>The number of bytes read.</returns>
        public override Int32 Read(byte[] buffer, Int32 offset, Int32 count)
        {
            var readLength = 0;
            try
            {
                if (this.session.State != 4)
                {
                    BeginRead(this.Info);
                }
                var startTicks = DateTime.UtcNow.Ticks;
                readLength = this.client.Read(this.session, buffer, offset, count);
                this.System.IncreaseTotalReadTicks(DateTime.UtcNow.Ticks - startTicks);
                this.System.IncreaseTotalReadBytes(readLength);

                if (readLength >= 0)
                {
                    this.ReadLength += readLength;
                }
            }
            catch (Exception e)
            {
                ReleaseMutex();
                var readFailedEventMessage = new EventIds.Storage.ReadFailedEventMessage(PathUtil.CombinePath(this.System.SystemLocation, this.URI.SInfo.HighName), ContextValues.Storage.StorageType.TSM, e);
                logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.TSM, readFailedEventMessage);
                logger.Error(e.Message, e);
                throw;
            }
            return readLength;
        }

        /// <summary>
        /// Send a byte stream of data to Server through a buffer.
        /// </summary>
        /// <param name="buffer">The buffer of byte stream.</param>
        /// <param name="offset">The offset of started writing.</param>
        /// <param name="count">The total count to write.</param>
        public override void Write(byte[] buffer, Int32 offset, Int32 count)
        {
            try
            {
                if (this.session.State != 2)
                {
                    this.client.BeginWrite(this.session, this.Info.HighName, this.Info.LowName);
                }
                var startTicks = DateTime.UtcNow.Ticks;
                this.client.Write(this.session, buffer, offset, count);
                this.System.IncreaseTotalWriteTicks(DateTime.UtcNow.Ticks - startTicks);
                this.System.IncreaseTotalWriteBytes(count);
            }
            catch (Exception e)
            {
                ReleaseMutex();
                var writeFailedEventMessage = new EventIds.Storage.WriteFailedEventMessage(PathUtil.CombinePath(this.System.SystemLocation, this.URI.SInfo.HighName), ContextValues.Storage.StorageType.TSM, e);
                logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.TSM, writeFailedEventMessage);
                logger.Error(e.Message, e);
                throw;
            }
        }

        public override XURIResult GetURI()
        {
            return this.URI;
        }

        #region -- Member of XStream --

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override Int64 Seek(Int64 offset, System.IO.SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(Int64 value)
        {
            throw new NotSupportedException();
        }

        #endregion

    }
}
