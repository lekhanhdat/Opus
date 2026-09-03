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
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.Common;

namespace AvePoint.Wrapper.Common
{
    public abstract class WrapperRestoreStreamBase : IAveRestoreStream, IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        internal AveInternalRestoreStream internalStream;
        private XmlDocument mDoc;
        private XmlElement mRootElement;
        private bool mIsReadMeatadata;
        private byte[] mDataBuffer = new byte[AveDataBlock.DATA_BLOCK_DATA_LEN];
        protected IInputStreamWrapper stream;


        public WrapperRestoreStreamBase(IInputStreamWrapper stream)
        {
            this.stream = stream;
            mDoc = new XmlDocument();
        }

        public virtual byte[] DataBuffer
        {
            get { return mDataBuffer; }
        }

        public virtual long ContentLength
        {
            get { return internalStream.ContentLength;  }
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
                    log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetStreamLengthError, e.ToString());
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
            mDoc.RemoveAll();
            mIsReadMeatadata = false;
        }

        #region Use FileReceiver
        public virtual string ReadHead()
        {
            throw new NotImplementedException();
        }

        public virtual string ReadTail()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region MetaData
        public virtual AveMetadata ReadMetadata()
        {
            InitRootElement();
            if (mRootElement == null)
            {
                return null;
            }
            XmlElement xmlElement = (XmlElement)mRootElement.FirstChild;
            if (xmlElement == null)
            {
                return null;
            }
            mRootElement.RemoveChild(xmlElement);
            return new AveMetadata(xmlElement);
        }

        public virtual AveMetadata TryReadMetadata(AveMetadataType metadataName)
        {
            InitRootElement();
            if (mRootElement == null)
            {
                return null;
            }
            XmlNodeList nodeList = mRootElement.SelectNodes("/Data/Field[@name='" + metadataName.ToString() + "']");
            if (nodeList == null || nodeList.Count == 0)
            {
                return null;
            }
            XmlElement xmlElement = (XmlElement)nodeList[0];
            //mRootElement.RemoveChild(xmlElement);
            return new AveMetadata(xmlElement);
        }

        public virtual List<AveMetadata> TryReadMetadataList(AveMetadataType metadataName)
        {
            InitRootElement();
            if (mRootElement == null)
            {
                return null;
            }
            XmlNodeList nodeList = mRootElement.SelectNodes("/Data/Field[@name='" + metadataName.ToString() + "']");
            if (nodeList == null || nodeList.Count == 0)
            {
                return null;
            }
            List<AveMetadata> list = new List<AveMetadata>();
            foreach (XmlElement xe in nodeList.OfType<XmlElement>())
            {
                AveMetadata metadata = new AveMetadata(xe);
                list.Add(metadata);
            }
            return list;
        }
        protected virtual void InitRootElement()
        {
            if (!mIsReadMeatadata)
            {
                InitInternalRestoreStream();

                mIsReadMeatadata = true;
                var streamReader = new StreamReader(internalStream, Encoding.UTF8);
                mDoc.Load(streamReader);
                mRootElement = (XmlElement)mDoc.FirstChild;
                if (mRootElement == null || string.Compare(mRootElement.Name, AveWrapperConstants.ROOT_ELEMENT, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    mRootElement = null;
                }
            }
        }

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
            if (internalStream != null)
            {
                internalStream.Dispose();
                internalStream = null;
            }
        }
    }
}
