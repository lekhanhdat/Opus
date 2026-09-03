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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.GCommon.Network;
using AvePoint.Wrapper.Resource;

namespace AvePoint.Wrapper.Common
{
    public abstract class WrapperRestoreStreamBaseV2 : IAveRestoreStream
    {
        protected static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        internal AveInternalRestoreStream internalStream;
        private bool mIsInternalStreamInited;
      
        private byte[] mDataBuffer = new byte[AveDataBlock.DATA_BLOCK_DATA_LEN];
        protected IInputStreamWrapper stream;


        public WrapperRestoreStreamBaseV2(IInputStreamWrapper stream)
        {
            this.stream = stream;
        }

        public IAveMetadataReader MetadataReader { get; set; }

        public virtual byte[] DataBuffer
        {
            get { return mDataBuffer; }
        }

        public virtual long ContentLength
        {
            get { return internalStream.ContentLength; }
        }

        public virtual long CurrentNodeTransferedSize
        {
            get { return Length; }
        }

        public virtual long Length
        {
            get
            {
                long length = 0L;
                try
                {
                    length = ContentLength;
                    length += internalStream.MetadataLength;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, WrapperCommonResource.AWCGetStreamLengthError, e.ToString());
                }
                return length;
            }
        }

        public virtual long MetadataLength
        {
            get { return internalStream.MetadataLength; }
        }

        public virtual void Reset()
        {
            if (MetadataReader != null)
            {
                MetadataReader.Dispose();
                MetadataReader = null;
            }
            mIsInternalStreamInited = false;
        }

        #region Use FileReceiver
        public virtual string ReadHead()
        {
            Reset();
            return stream.GetNextFileHead();
        }

        public virtual string ReadTail()
        {
            return stream.GetFileTail();
        }
        #endregion

        #region MetaData
        public virtual AveMetadata ReadMetadata()
        {
            InitRootElement();
            return MetadataReader.ReadMetadata();
        }

        public virtual AveMetadata TryReadMetadata(AveMetadataType metadataName)
        {
            InitRootElement();
            return MetadataReader.TryReadMetadata(metadataName);
        }

        public virtual List<AveMetadata> TryReadMetadataList(AveMetadataType metadataName)
        {
            InitRootElement();
            return MetadataReader.TryReadMetadataList(metadataName);
        }

        protected virtual void InitRootElement()
        {
            if (!mIsInternalStreamInited)
            {
                InitInternalRestoreStream();
                mIsInternalStreamInited = true;
                InitMetadataReader();
                if (MetadataReader == null)
                {
                    throw new Exception("Metadata reader is null.");
                }
            }
        }

        protected abstract void InitMetadataReader();

        protected abstract void InitInternalRestoreStream();

        #endregion

        #region Content
        public virtual int ReadContent(byte[] buffer, int offset, int length)
        {
            if (ContentLength == 0)
            {
                return 0;
            }
            return internalStream.ReadContent(buffer, offset, length);
        }
        #endregion

        public void Dispose()
        {
            if (MetadataReader != null)
            {
                MetadataReader.Dispose();
                MetadataReader = null;
            }
            if (internalStream != null)
            {
                internalStream.Dispose();
                internalStream = null;
            }
        }
    }
}
