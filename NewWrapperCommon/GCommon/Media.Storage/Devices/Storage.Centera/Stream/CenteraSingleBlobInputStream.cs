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



namespace AvePoint.Media.Storage.Centera
{
    #region using directives
    using System;
    using AvePoint.GCommon;
    using System.Reflection;
    using AvePoint.GCommon.Utility.I18N;
    #endregion

    class CenteraSingleBlobInputStream : XStream
    {
        private AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private FPTag tag;
        private FPClip clip;
        private CenteraClient client;
        private FPInputStream innerStream;
        public CenteraSingleBlobInputStream(StorageInfo info, CenteraClient client, AbstractXSystem sys)
            : base(sys)
        {
            this.client = client;
            this.Info = info;
            InitReadStream(this.Info);
        }

        public void InitReadStream(StorageInfo info)
        {
            this.clip = client.OpenClip(info.ClipId);
            this.tag = this.clip.OpenTag(client.CheckName(info.LowName));
            innerStream = new FPInputStream(this.tag, info.Offset, info.Length);
            innerStream.BeginRead();

        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long startTicks = DateTime.UtcNow.Ticks;
            int readLen = 0;
            try
            {
                while (true)
                {
                    //if (offset > 0)
                    //{
                    //    byte[] tmp = new byte[count];
                    //    readLen = innerStream.Read(tmp, 0, tmp.Length);
                    //    Array.Copy(tmp, 0, buffer, offset, readLen);
                    //}
                    //else
                    //{
                    //    readLen = innerStream.Read(buffer, offset, count);
                    //}
                    readLen = innerStream.Read(buffer, offset, count);
                    if (readLen > 0)
                    {
                        this.ReadLength += readLen;
                        break;
                    }
                    else
                    {
                        if (this.ReadLength < this.Info.Length && this.Info.DataType == DataBlockType.MetaData)
                        {

                            StorageInfo storageInfo = new StorageInfo();
                            storageInfo.Length = this.Info.Length - this.ReadLength;
                            this.ReadLength = 0;
                            storageInfo.ClipId = this.Info.ClipId;
                            storageInfo.LowName = this.tag.GetNextTagName();
                            storageInfo.Offset = 62 + 4096;
                            this.InitReadStream(storageInfo);
                            continue;
                        }
                        break;
                    }
                }
                System.IncreaseTotalReadTicks(DateTime.UtcNow.Ticks - startTicks);
                System.IncreaseTotalReadBytes(readLen);
            }
            catch (Exception e)
            {
                EventIds.Storage.ReadFailedEventMessage readfailedEventMessage = new EventIds.Storage.ReadFailedEventMessage(this.Info.ClipId, ContextValues.Storage.StorageType.EMCCentera, e);
                this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.EMC_Centera, readfailedEventMessage);

                logger.Error(e.Message, e);
                throw;
            }
            return readLen;
        }

        public override void Close()
        {

            if (this.innerStream != null)
            {
                this.innerStream.Close();
                this.innerStream = null;
            }
            if (this.tag != null)
            {
                this.tag.Close();
                this.tag = null;
            }
            if (this.clip != null)
            {
                this.clip.Close();
            }
        }

        #region Not Supported Methods

        public override bool CanRead
        {
            get { return innerStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return innerStream.CanWrite; }
        }

        public override void Flush()
        {
            //throw new InvalidOperationException("");
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {

            throw new NotSupportedException("Not Supported Method.");
        }

        public override StorageResult Commit(bool closeParent)
        {
            throw new NotSupportedException("Not Supported Method.");
        }

        public override StorageResult Commit()
        {
            return Commit(true);
        }

        public override XURIResult GetURI()
        {
            throw new NotSupportedException("Not Supported Method");
        }
        #endregion
    }
}
