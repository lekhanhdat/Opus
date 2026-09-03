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
namespace AvePoint.Wrapper.Common
{
    using AvePoint.GCommon;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    /// <summary>
    /// 传入的stream，只负责使用，不负责释放，在哪构造的，就在哪释放
    /// </summary>
    class AveMemoryMetadataReader:IAveMetadataReader
    {

        protected bool IsAllMetadataLoaded = false;
        private StreamReader internalReader;
        private XmlDocument mDoc=new XmlDocument();
        private XmlElement mRootElement;
        public AveMemoryMetadataReader(Stream stream)
        {
            IsAllMetadataLoaded = false;
            internalReader = new StreamReader(stream, Encoding.UTF8);
        }

        public AveMemoryMetadataReader(StreamReader streamReader)
        {
            internalReader = streamReader;
        }
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
            if (!IsAllMetadataLoaded)
            {
               
               IsAllMetadataLoaded = true;
                mDoc.Load(internalReader);
                mRootElement = (XmlElement)mDoc.FirstChild;
                if (mRootElement == null || string.Compare(mRootElement.Name, AveWrapperConstants.ROOT_ELEMENT, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    mRootElement = null;
                }
            }
        }
        #endregion

        public void Dispose()
        {
            mDoc.RemoveAll();
            IsAllMetadataLoaded = false;
            //throw new NotImplementedException();
        }
    }
}
