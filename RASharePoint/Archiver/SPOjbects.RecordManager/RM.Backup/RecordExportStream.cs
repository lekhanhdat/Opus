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

using AvePoint.GCommon.FileTransfer;
using AvePoint.GCommon.Network;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace AvePoint.RA.SharePoint.Archiver
{
    public class RMExportStream : IAveBackupStream, IDisposable
    {
        #region - Params -

        protected static byte[] METADATA_HEAD_BUFFER;
        protected static byte[] METADATA_TAIL_BUFFER;
        protected static string TEMP_STREAM_NAME = "ACVRMES";   //Archiver RMExportStream
        protected const int XML_SIZE = 64 * 1024;
        protected IFileSender mFileSender;
        protected Stream mInternalStream;
        protected XmlTextWriter mXmlWriter;

        protected Dictionary<string, object> mDataCache;
        protected long mStreamTransfered;
        protected long mCurrentNodeTransferedSize;
        protected byte[] mDataBuffer;
        protected byte[] mMetaDataHeader;

        protected int mState;
        protected const int STATE_BEGIN = 0;
        protected const int STATE_END = 1;
        protected const int STATE_METATATA = 2;
        protected const int STATE_CONTENT = 3;

        public Dictionary<string, object> DataCache
        {
            get { return mDataCache; }
        }

        public long StreamTransfered
        {
            get { return mStreamTransfered; }
        }

        public long CurrentNodeTransferedSize
        {
            get { return mCurrentNodeTransferedSize; }
        }

        public byte[] DataBuffer
        {
            get { return mDataBuffer; }
        }

        #endregion

        #region - Method -

        public RMExportStream(IFileSender iSender)
        {
            mFileSender = iSender;
            Init();
        }

        private void Init()
        {
            mInternalStream = new AveCoordinatedStream(TEMP_STREAM_NAME);
            mXmlWriter = new XmlTextWriter(mInternalStream, new UTF8Encoding(false));
            METADATA_HEAD_BUFFER = Encoding.UTF8.GetBytes("<Data version=\"" + AveWrapperConstants.CURRENT_VERSION + "\">");
            METADATA_TAIL_BUFFER = Encoding.UTF8.GetBytes("</Data>");
            mState = STATE_END;

            mDataBuffer = new byte[AveDataBlock.DATA_BLOCK_DATA_LEN];
            mDataCache = new Dictionary<string, object>();
            mMetaDataHeader = new byte[AveWrapperConstants.HEADER_SIZE];
        }

        //public virtual void WriteMetadata(string name, IDictionary value)
        //{
        //    WriteMetadata(name, value);
        //}

        public virtual void WriteMetadata(string name, object value)
        {
            if (mState == STATE_BEGIN || mState == STATE_METATATA)
            {
                mState = STATE_METATATA;
                AveXmlSerializer.Serialize(mXmlWriter, name, value);
            }
            else
            {
                throw new AveException("The stream must be initialized first.");
            }
        }

        public virtual void WriteMetadata(AveMetadataType metadataType, object value)
        {
            if (mState == STATE_BEGIN || mState == STATE_METATATA)
            {
                mState = STATE_METATATA;
                AveXmlSerializer.Serialize(mXmlWriter, metadataType.ToString(), value);
            }
            else
            {
                throw new AveException("The stream must be initialized first.");
            }
        }

        public virtual void WriteMetadata(AveMetadataType metadataType, IDictionary value)
        {
            if (mState == STATE_BEGIN || mState == STATE_METATATA)
            {
                mState = STATE_METATATA;
                AveXmlSerializer.Serialize(mXmlWriter, metadataType.ToString(), value);
            }
            else
            {
                throw new AveException("The stream must be initialized first.");
            }
        }

        public virtual void WriteContent(byte[] buffer, int offset, int length)
        {
            if (mState != STATE_CONTENT)
            {
                throw new AveException("The content must be sent after metadata. Please call FlushMetadata first.");
            }
            mFileSender.WriteContentData(buffer, offset, length);
            mCurrentNodeTransferedSize += length;
            mStreamTransfered += length;
        }

        public virtual void BeginWriteMetadata()
        {
            //if (mState != STATE_ALL_BEGIN || mState != STATE_ALL_END)
            //{
            //    throw new AveException("Please clear the stream before do other actions.");
            //}
            mState = STATE_BEGIN;
            mXmlWriter.Flush();
            if (mInternalStream.Length > 1024 * 1024 * 5)
            {
                if (mXmlWriter != null)
                {
                    mXmlWriter.Close();
                }
                if (mInternalStream != null)
                {
                    mInternalStream.Dispose();
                }
                mInternalStream = new AveCoordinatedStream(TEMP_STREAM_NAME);
                mXmlWriter = new XmlTextWriter(mInternalStream, new UTF8Encoding(false));
            }
            mInternalStream.Position = 0;
            mInternalStream.Write(mMetaDataHeader, 0, mMetaDataHeader.Length);
            mInternalStream.Write(METADATA_HEAD_BUFFER, 0, METADATA_HEAD_BUFFER.Length);
        }

        public virtual void EndWriteMetadata()
        {
            if (mState == STATE_END)
            {
                return;
            }
            mState = STATE_END;
            mXmlWriter.Flush();
            mInternalStream.Write(METADATA_TAIL_BUFFER, 0, METADATA_TAIL_BUFFER.Length);
        }

        public virtual void FlushMetadata(long contentLength)
        {
            if (mState == STATE_CONTENT)
            {
                return;
            }
            mState = STATE_CONTENT;
            int length = (int)mInternalStream.Position;

            bool isFirstTime = true;
            int bytesRead = 0;
            byte[] buffer = new byte[XML_SIZE];
            mInternalStream.Position = 0;
            while ((bytesRead = mInternalStream.Read(buffer, 0, XML_SIZE)) > 0)
            {
                if (isFirstTime)
                {
                    AveConvert.ToBigBytes(length - mMetaDataHeader.Length, buffer, 0);
                    if (contentLength < 0)
                    {
                        contentLength = 0;
                    }
                    AveConvert.ToBigBytes((int)contentLength, buffer, 4);
                    isFirstTime = false;
                }
                if (mInternalStream.Position >= length)//没必要再往下读数据了
                {
                    bytesRead -= (int)(mInternalStream.Position - length);
                    mFileSender.WriteData(buffer, 0, bytesRead);
                    break;
                }
                mFileSender.WriteData(buffer, 0, bytesRead);
            }
        }

        public virtual void WriteHead(string headXml)
        {
            using (AvePerformanceScope pcDoc = new AvePerformanceScope("FileSender.WriteHeader"))
            {
                mFileSender.WriteHead(headXml);
            }
            int length = Encoding.UTF8.GetByteCount(headXml);
            mCurrentNodeTransferedSize = length;
            mStreamTransfered += length;
        }

        public virtual long WriteTail(string tailXml)
        {
            return WriteTail(tailXml, true);
        }

        public virtual long WriteTail(string tailXml, bool isOk)
        {
            mStreamTransfered += Encoding.UTF8.GetByteCount(tailXml);
            using (AvePerformanceScope pcDoc = new AvePerformanceScope("FileSender.WriteTail"))
            {
                return mFileSender.WriteTail(tailXml, isOk);
            }
        }

        public virtual void ClearStreamTransfered()
        {
            mStreamTransfered = 0;
            mFileSender.Close("0");
        }

        public virtual void SetStreamTransfered(long value)
        {
            if (value >= 0)
            {
                mStreamTransfered = value;
            }
        }

        public void Dispose()
        {
            if (mXmlWriter.WriteState != WriteState.Closed)
            {
                mXmlWriter.Close();
            }
            if (mInternalStream != null)
            {
                mInternalStream.Dispose();
                mInternalStream = null;
            }
        }

        #endregion
    }
}
