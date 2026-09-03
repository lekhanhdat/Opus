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
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Xml;

    public class AveMetadataV2: AveMetadata,IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private bool isDisposed;
        private XmlDocument mDoc = new XmlDocument();
        public AveMetadataV2(XmlElement xmlElement) : base(xmlElement)
        {
            //throw new NotSupportedException();
        }

        public AveMetadataV2(string tempDataPath, AveMetadataType type):base()
        {
            isDisposed = false;
            MetadataPath = tempDataPath;
            mMetadataType = type;
        }

        public string MetadataPath { get; set; }

        [Obsolete("will be removed later")]
        public override void GetMetadata(object value)
        {
            throw new NotSupportedException();
        }

        public override T GetMetadata<T>()
        {
            CheckDispose();
            return (T)AveXmlSerializer.Deserialize(MetadataPath, typeof(T));
        }

        public override object GetMetadataObject()
        {
            CheckDispose();
            return AveXmlSerializer.Deserialize(MetadataPath);
        }

        /// <summary>
        /// todo:need to improve it later
        /// </summary>
        /// <param name="dictionary"></param>
        public override void GetMetadata(IDictionary dictionary)
        {
            IDictionary tempDic = AveXmlSerializer.Deserialize(MetadataPath) as IDictionary;
            foreach (var key in tempDic.Keys)
            {
                dictionary.Add(key, tempDic[key]);
            }
            tempDic.Clear();
        }

        private void CheckDispose()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException("AveMetdataV2:"+MetadataPath);
            }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                try
                {
                    mXmlElement = null;
                    mDoc.RemoveAll();
                    if (File.Exists(MetadataPath))
                    {
                        File.Delete(MetadataPath);
                    }
                    isDisposed = true;
                }
                catch (Exception e)
                {
                    log.Error("An error occurred while dispose AveMetadataV2.MetadataPath:{0},Error:{1}", MetadataPath, e);
                }
            }
        }

        [Obsolete("will be removed later")]
        public override XmlElement XmlElement
        {

            get
            {
                if (mXmlElement == null)
                {
                    
                    mDoc.Load(MetadataPath);
                    mXmlElement = mDoc.DocumentElement;
                }
                return mXmlElement;
            }
        }

    }
}
