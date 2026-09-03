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
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;

namespace AvePoint.Media.Storage.Box
{
    class BoxUtil
    {
        public static string Convert2StorageInfo(BoxStorageInfo info)
        {
            return string.Format("<StorageInfo metaId=\"{0}\" contentId=\"{1}\"/>", info.MetaId, info.ContentId);
        }

        public static BoxStorageInfo Convert2CAStorStorageInfo(string storageInfo)
        {
            BoxStorageInfo info = new BoxStorageInfo();
            if (!string.IsNullOrEmpty(storageInfo))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(storageInfo);
                XmlElement node = (XmlElement)xmlDoc.SelectSingleNode("StorageInfo");
                info.MetaId = node.GetAttribute("metaId");
                info.ContentId = node.GetAttribute("contentId");
            }
            return info;
        }

        public static string ParseSpaceField(string jsonString, string pattern)
        {
            Match m = Regex.Match(jsonString, pattern);
            if (!m.Success)
            {
                throw new Exception("Match space field failed.");
            }
            string[] tempStrs = m.Groups[0].Value.Split(':');
            return tempStrs[1];
        }
    }
    
    class BoxStorageInfo
    {
        public string MetaId { get; set; }
        public string ContentId { get; set; }
    }

    class BoxConstants
    {
        public static readonly string META_ID_HEADER = "__BoxMetaID__";

        public static readonly string HttpMethod_PUT = "PUT";
        public static readonly string HttpMethod_GET = "GET";
        public static readonly string HttpMethod_DELETE = "DELETE";
        public static readonly string HttpMethod_POST = "POST";
    }
}
