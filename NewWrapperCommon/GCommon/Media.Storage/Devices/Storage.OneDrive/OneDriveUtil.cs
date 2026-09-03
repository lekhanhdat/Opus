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

using System.Diagnostics.CodeAnalysis;
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.SkyDrive.SkyDriveConstant.#.cctor()", MessageId = "wl")]
namespace AvePoint.Media.Storage.OneDrive
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Text.RegularExpressions;
    using System.Diagnostics;
    #endregion

    class OneDriveUtil
    {
        public static OneDriveStorageInfo Convert2CAStorStorageInfo(string storageInfo)
        {
            OneDriveStorageInfo info = new OneDriveStorageInfo();
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

        public static string Convert2StorageInfo(OneDriveStorageInfo info)
        {
            return string.Format("<StorageInfo metaId=\"{0}\" contentId=\"{1}\"/>", info.MetaId, info.ContentId);
        }

        public static string ParseRootFolderId(string jsonStr, string fileName)
        {
            string result = null;
            List<string> folderIdlist = new List<string>();
            List<string> folderName = new List<string>();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            try
            {
                MatchCollection ms1 = Regex.Matches(jsonStr, "\"id\": \"folder[^\"]+");
                foreach (Match m in ms1)
                {
                    folderIdlist.Add(Parse1(m.Groups[0].Value));
                }
                MatchCollection ms2 = Regex.Matches(jsonStr, "\"name\": \"[^\"]+");
                for (int i = 0; i < ms2.Count; i++)
                {
                    if (i % 2 == 1)
                    {
                        folderName.Add(Parse2(ms2[i].Groups[0].Value));
                        if (folderName.Contains(fileName))
                        {
                            result = folderIdlist[(i - 1) / 2];
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.Message);
                return null;
            }
            return result;
        }

        private static string Parse1(string str)
        {
            Match m = Regex.Match(str, "folder[^\"]+");
            return m.Groups[0].Value;
        }

        private static string Parse2(string str)
        {
            string[] strs = str.Split(':');
            return strs[1].Trim().Trim('\"');
        }

        public static string GetNewRootFolderId(string jsonStr)
        {
            Match m = Regex.Match(jsonStr, "\"id\": \"folder[^\"]+");
            return Parse1(m.Groups[0].Value);
        }

    }

    class OneDriveStorageInfo
    {
        public string MetaId { get; set; }
        public string ContentId { get; set; }
    }

    class OneDriveConstant
    {
        public static readonly string FILE_ID_SEPARATOR = "__";
        public static readonly string META_ID_HEADER = "__SkyDriveMetaID__";
        public static readonly string RESPONSE_TYPE = "code";
        public static readonly string SCOPES = "wl.offline_access%20wl.skydrive%20wl.skydrive_update"; //Obtaining user consent http://msdn.microsoft.com/en-us/library/live/hh826540.aspx
    }

}
