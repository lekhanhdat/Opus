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
using System.Text;
using System.Xml;

namespace AvePoint.Media.Storage.CAStor
{
    class CAStorUtil
    {
        public static string Convert2StorageInfo(CAStorStorageInfo info)
        {
            return string.Format("<StorageInfo metaId=\"{0}\" contentId=\"{1}\"/>", info.MetaId, info.ContentId);
        }

        public static CAStorStorageInfo Convert2CAStorStorageInfo(string storageInfo)
        {
            CAStorStorageInfo info = new CAStorStorageInfo();
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
    }

     class CAStorStorageInfo
     {
         public string MetaId { get; set; }
         public string ContentId { get; set; }
    }

     enum LocatorType
     {
         None = -1,
         Proxy = 0,
         Static = 1
     }
}
