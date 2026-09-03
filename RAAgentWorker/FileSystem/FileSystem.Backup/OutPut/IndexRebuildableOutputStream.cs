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
using System.Xml;

using AvePoint.GCommon.Utility;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Storage;

namespace AvePoint.Media.Core.IO.Output
{
    public class IndexRebuildableOutputStream : FilterGeneralOutputStream
    {
        private IndexBase mBasicIndex;
        public IndexRebuildableOutputStream(IGeneralOutputStream innerOutput)
            : base(innerOutput)
        {
        }
        public override StorageResult Close()
        {
            return InnerOutputStream.Close();
        }

        public override void EndItem(IndexBase basicIndex)
        {
            InnerOutputStream.EndItem(basicIndex);
            mBasicIndex = basicIndex;
        }

        public override void Open()
        {
            InnerOutputStream.Open();
        }

        public override void WriteContentData(byte[] data, int offset, int count)
        {
            InnerOutputStream.WriteContentData(data, offset, count);
        }

        public override void WriteHeaderXml(string headerXml)
        {
            InnerOutputStream.WriteHeaderXml(headerXml);
        }

        public override void WriteMetaData(byte[] data, int offset, int count)
        {
            InnerOutputStream.WriteMetaData(data, offset, count);
        }

        public override void WriteTailXml(string tailXml)
        {
            if (this.mBasicIndex != null)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(tailXml);
                xmlDoc.DocumentElement.SetAttribute("indexString", SerializerHelper.SerializeToBase64String(this.mBasicIndex));
                tailXml = xmlDoc.DocumentElement.OuterXml;
            }
            InnerOutputStream.WriteTailXml(tailXml);
        }
    }
}
