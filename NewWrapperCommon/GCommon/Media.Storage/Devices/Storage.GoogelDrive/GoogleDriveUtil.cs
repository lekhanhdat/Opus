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

namespace AvePoint.Media.Storage.GoogleDrive
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Diagnostics;
    using System.Text.RegularExpressions; 
    #endregion

    class GoogleDriveUtil
    {
        public static GoogleDriveStorageInfo Convert2CAStorStorageInfo(string storageInfo)
        {
            GoogleDriveStorageInfo info = new GoogleDriveStorageInfo();
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

        public static string Convert2StorageInfo(GoogleDriveStorageInfo info)
        {
            return string.Format("<StorageInfo metaId=\"{0}\" contentId=\"{1}\"/>", info.MetaId, info.ContentId);
        }

        private static string Parse1(string str)
        {
            string[] tempStrs = str.Split(':');
            return tempStrs[1].Substring(2);
        }

        public static string GetNewRootFolderId(string jsonStr)
        {
            Match match = Regex.Match(jsonStr, "\"id\": \"[^\"]+");
            string[] tempStrs = match.Groups[0].Value.Split(':');
            return tempStrs[1].Substring(2);
        }

    }

    class GoogleDriveStorageInfo
    {
        public string MetaId { get; set; }
        public string ContentId { get; set; }
    }

    class GoogleDriveConstant
    {
        public static readonly string FILE_ID_SEPARATOR = "__";
        public static readonly string META_ID_HEADER = "__GoogleDriveMetaID__";
        public static readonly string RedirectDomain = "urn:ietf:wg:oauth:2.0:oob";
    }
}
