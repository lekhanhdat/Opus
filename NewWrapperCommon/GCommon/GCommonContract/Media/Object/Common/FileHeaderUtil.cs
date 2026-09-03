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


namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml;

    #endregion using directives

    public static class FileHeaderUtil
    {
        public static FileHeader ToFileHeader(string fileHeaderXml)
        {
            XmlDocument mDoc = new XmlDocument();
            FileHeader header = new FileHeader();
            mDoc.LoadXml(fileHeaderXml);
            XmlElement xEl = mDoc.DocumentElement;
            if (xEl.HasAttribute("type"))
            {
                header.Type = (AveSharePointType)(char.Parse(xEl.GetAttribute("type").ToUpperInvariant()));
            }
            if (xEl.HasAttribute("path"))
            {
                header.Path = xEl.GetAttribute("path");
            }
            if (xEl.HasAttribute("isAppData"))
            {
                header.IsAppData = Boolean.Parse(xEl.GetAttribute("isAppData"));
            }
            if (xEl.HasAttribute("appDataName"))
            {
                header.AppDataName = xEl.GetAttribute("appDataName");
            }
            if (xEl.HasAttribute("listType"))
            {
                header.ListType = Int32.Parse(xEl.GetAttribute("listType"));
            }
            else
            {
                header.ListType = 0;
            }
            if (xEl.HasAttribute("extensionInfo"))
            {
                header.ExtensionInfo = xEl.GetAttribute("extensionInfo");
            }
            if (xEl.HasAttribute("backupType"))
            {
                header.BackupType = (BackupType)int.Parse(xEl.GetAttribute("backupType"));
            }
            else
            {
                header.BackupType = BackupType.Normal;
            }
            if (header.Type == AveSharePointType.TYPE_SITE && xEl.HasAttribute("compatibilityLevel"))
            {
                header.SPMode = int.Parse(xEl.GetAttribute("compatibilityLevel"));
            }
            else
            {
                header.SPMode = 0;
            }
            if (xEl.HasAttribute("listBaseType"))
            {
                header.ListBaseType = Convert.ToInt32(xEl.GetAttribute("listBaseType"));
            }
            if (xEl.HasAttribute("webApp"))
            {
                header.WebApp = xEl.GetAttribute("webApp");
            }
            if (xEl.HasAttribute("SPVersion"))
            {
                header.SPVersion = Convert.ToInt32(xEl.GetAttribute("SPVersion"));
            }
            if (xEl.HasAttribute("rowId"))
            {
                header.RowId = xEl.GetAttribute("rowId");
            }
            if (xEl.HasAttribute("IsSuccessful")) 
            {
                header.IsSuccessful = Boolean.Parse(xEl.GetAttribute("IsSuccessful"));
            }
            XmlNodeList nodeList = xEl.GetElementsByTagName("HeaderExtraAttribute");
            if (nodeList != null && nodeList.Count > 0)
            {
                header.HeaderExtraAttribute = nodeList[0].OuterXml;
            }
            else
            {
                header.HeaderExtraAttribute = string.Empty;
            }
            return header;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "FullTextindexInfo is unmodifiable as the cause of being referenced.")]
        public static FileTail ToFileTail(string tailXml)
        {
            var xmlTailDoc = new XmlDocument();
            var fileTail = new FileTail();
            fileTail.Attributes = new List<String>();
            xmlTailDoc.LoadXml(tailXml);
            var rootEl = xmlTailDoc.DocumentElement;
            fileTail.Length = long.Parse(rootEl.GetAttribute("length"));
            if (rootEl.HasAttribute("CRC32"))
            {
                fileTail.Crc32 = long.Parse(rootEl.GetAttribute("CRC32"));
            }
            else
            {
                fileTail.Crc32 = null;
            }
            var title = rootEl.GetElementsByTagName("Title");
            if (title.Count > 0)
            {
                fileTail.Title = title[0].InnerText.ToString();
            }
            else
            {
                fileTail.Title = null;
            }
            var PostId = rootEl.GetElementsByTagName("PostId");
            if (PostId.Count > 0)
            {
                fileTail.PostId = PostId[0].InnerText.ToString();
            }
            else
            {
                fileTail.PostId = null;
            }
            var CreateTime = rootEl.GetElementsByTagName("CreateTime");
            if (CreateTime.Count > 0)
            {
                fileTail.CreateTime = long.Parse(CreateTime[0].InnerText);
            }
            else
            {
                fileTail.CreateTime = 0;
            }
            if (rootEl.HasAttribute("failed"))
            {
                fileTail.IsFailed = bool.Parse(rootEl.GetAttribute("failed"));
            }
            else
            {
                fileTail.IsFailed = false;
            }
            var attList = rootEl.GetElementsByTagName("Attribute");
            foreach (XmlElement attEl in attList)
            {
                fileTail.Attributes.Add(attEl.InnerText);
            }
            var list = rootEl.GetElementsByTagName("IsSystemFile");
            if (list.Count > 0)
            {
                fileTail.IsSystemFile = Convert.ToBoolean(list[0].InnerText);
            }
            fileTail.ExtraInfo = rootEl.GetAttribute("extraInfo");
            var fullTextindexInfo = rootEl.GetElementsByTagName("FullTextindexInfo");
            foreach (XmlElement attEl in fullTextindexInfo)
            {
                fileTail.DetailInfoAttributes = attEl.OuterXml;
            }
            return fileTail;
        }

        public static string ToString(FileHeader fileheader, Int64 totalRestoreCount = 0)
        {
            var mDoc = new XmlDocument();
            var xEl = mDoc.CreateElement("FileHeader");
            xEl.SetAttribute("type", fileheader.TypeAsString);
            xEl.SetAttribute("path", fileheader.Path);
            xEl.SetAttribute("sequence", fileheader.Sequence.ToString());
            xEl.SetAttribute("versionFlag", fileheader.VersionFlag.ToString());
            xEl.SetAttribute("webApp", fileheader.WebApp);
            xEl.SetAttribute("HeaderExtraAttribute", fileheader.HeaderExtraAttribute);
            xEl.SetAttribute("property", fileheader.Property == Tree.Object.PropertyState.Checked ? "true" : "false");
            xEl.SetAttribute("security", fileheader.Security == Tree.Object.SecurityState.Checked ? "true" : "false");
            xEl.SetAttribute("listType", fileheader.ListType.ToString());
            xEl.SetAttribute("dataMode", fileheader.DataMode.ToString());
            xEl.SetAttribute("encryptionInfo", fileheader.EncryptionInfo);
            xEl.SetAttribute("extensionInfo", fileheader.ExtensionInfo);
            xEl.SetAttribute("isChecked", fileheader.IsChecked.ToString());
            xEl.SetAttribute("listBaseType", fileheader.ListBaseType.ToString());
            xEl.SetAttribute("isFailed", fileheader.IsFailed.ToString());
            xEl.SetAttribute("isAppData", fileheader.IsAppData.ToString());
            xEl.SetAttribute("appDataName", fileheader.AppDataName);
            if (totalRestoreCount > 0)
            {
                xEl.SetAttribute("totalRestoreCount", totalRestoreCount.ToString());
            }
            if (fileheader.ItemOffests != null && fileheader.ItemOffests.Count > 0)
            {
                XmlElement itemEl;
                for (int i = 0; i < fileheader.ItemOffests.Count; i++)
                {
                    itemEl = mDoc.CreateElement("ItemOffset");
                    itemEl.SetAttribute("offset", fileheader.ItemOffests[i].ToString());
                    itemEl.SetAttribute("length", fileheader.ItemLengths[i].ToString());
                    xEl.AppendChild(itemEl);
                }
            }
            mDoc.RemoveAll();
            return xEl.OuterXml;
        }
    }
}