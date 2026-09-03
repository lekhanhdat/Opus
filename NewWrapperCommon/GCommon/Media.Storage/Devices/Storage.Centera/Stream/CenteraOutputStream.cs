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

    class CenteraOutputStream : XStream
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        FPStream innerStream;
        public string LastClipId { get; set; }

        private FPClip clip;
        private FPTag tag;
        private UInt64 retentionDays;

        public CenteraOutputStream(CenteraClient client, StorageInfo info, AbstractXSystem sys)
            : base(sys)
        {
            this.Info = info;
            clip = client.CreateClip(info.HighName, (sys as CenteraSystem).CASLevel);
            this.retentionDays = client.RetentionDays;
            clip.SetRetentionPeriod(this.retentionDays * 24 * 60 * 60);
            tag = clip.CreateTag(client.CheckName(info.LowName));
            tag.UpdateTagMeta(FPTag.ORIGINAL_NAME, info.LowName);
            innerStream = new FPOutputStream(tag, this.Info.Length);
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
                EventIds.Storage.WriteFailedEventMessage writeFailedEventMessage = new EventIds.Storage.WriteFailedEventMessage(this.Info.ClipId, ContextValues.Storage.StorageType.EMCCentera, e);
                this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.EMC_Centera, writeFailedEventMessage);
                logger.Error("Centera Wrote Failed : " + e.Message, e);
                throw;
            }
        }

        #region
        public override bool CanRead
        {
            get { return false; }
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

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {

        }
        #endregion

        string currentClipID;
        string currentTagName;

        public override StorageResult Commit(bool closeParent)
        {
            try
            {
                StorageResult sr = new StorageResult();
                innerStream.Commit();
                if (innerStream != null)
                {
                    innerStream.Close();
                    innerStream = null;
                }
                //logger.Info("tag close :    " + tag.Name);
                currentTagName = tag.Name;
                tag.Close();

                if ((tag.Sequence >= FPClip.MAX_TAG_NUMBER && this.Info.DataType == DataBlockType.MetaData) || closeParent || (System as CenteraSystem).CASLevel == CASLevel.SingleBlob) //for one clip one more tag
                {
                    //if ((System as CenteraSystem).CASLevel != CASLevel.SingleBlob)
                    //{
                    //    if (!string.IsNullOrEmpty(this.LastClipId))
                    //    {
                    //        if (!this.clip.UpdateClipMeta(FPClip.PREVIOUS_CLIP_ID, LastClipId))
                    //        {
                    //            throw new Exception("update meta info failed");
                    //        }
                    //    }
                    //}
                    string clipId = clip.Write();
                    //clip.Close();
                    //clip = null;
                    logger.Info("New Clip ID : " + clipId);
                    sr.StorageInfo = StorageInfoUtil.ClipId2StorageInfo(clipId);
                    currentClipID = clipId;

                    //this.pool.RemoveClip(this.Info.HighName);
                    this.LastClipId = clipId;
                    sr.NeedCommit = true;
                }
                tag = null;
                if (!string.IsNullOrEmpty(this.Info.ExtraStorageInfo) && this.Info.IsDeleteOldVersion && this.retentionDays == 0)
                {
                    this.System.DeleteFile(new StorageInfo() { ClipId = this.Info.ClipId });
                }
                return sr;
            }
            catch (Exception e)
            {
                EventIds.Storage.WriteFailedEventMessage writeFailedEventMessage = new EventIds.Storage.WriteFailedEventMessage(this.Info.ClipId, ContextValues.Storage.StorageType.EMCCentera, e);
                this.logger.Log(EventSources.DocAveStorageAPIService, EventCategorys.DocAveStorageAPIService.EMC_Centera, writeFailedEventMessage);

                logger.Error(e.Message, e);
                throw;
            }
        }

        public override StorageResult Commit()
        {
            return Commit(true);
        }

        public override void Close()
        {

        }

        public override XURIResult GetURI()
        {
            this.URI.SdType = 3;
            this.URI.SysId = System.SystemID;
            logger.Info("tag return :    " + currentTagName);
            this.URI.SInfo = new StorageInfo(currentClipID, this.Info.LowName);
            this.URI.SInfo.ExtraStorageInfo = StorageInfoUtil.ClipId2StorageInfo(currentClipID);
            return this.URI;
        }
    }
}
