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
                header.Type = (AveSharePointType)(char.Parse(xEl.GetAttribute("type").ToUpper()));
            }
            if (xEl.HasAttribute("path"))
            {
                header.Path = xEl.GetAttribute("path");
            }
            if (xEl.HasAttribute("personalSiteUserName"))
            {
                header.PersonalSiteUserName = xEl.GetAttribute("personalSiteUserName");
            }
            if (xEl.HasAttribute("nodeGuid"))
            {
                //用于过滤IB中删除的数据
                header.UniqueId = xEl.GetAttribute("nodeGuid");
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
            if (xEl.HasAttribute("listBaseType"))
            {
                header.ListBaseType = Convert.ToInt32(xEl.GetAttribute("listBaseType"));
            }
            header.WebApp = xEl.GetAttribute("webApp");
            if (xEl.HasAttribute("isCurrentVersion"))
            {
                header.IsCurrentVersion = Boolean.Parse(xEl.GetAttribute("isCurrentVersion"));
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

        public static FileTail ToFileTail(string tailXml)
        {
            XmlDocument xmlTailDoc = new XmlDocument();
            FileTail fileTail = new FileTail();
            fileTail.Attributes = new List<string>();
            xmlTailDoc.LoadXml(tailXml);
            XmlElement rootEl = xmlTailDoc.DocumentElement;
            if (rootEl.HasAttribute("length"))
            {
                fileTail.Length = long.Parse(rootEl.GetAttribute("length"));
            }
            if (rootEl.HasAttribute("CRC32"))
            {
                fileTail.Crc32 = long.Parse(rootEl.GetAttribute("CRC32"));
            }
            else
            {
                fileTail.Crc32 = null;
            }
            if (rootEl.HasAttribute("failed"))
            {
                fileTail.IsFailed = bool.Parse(rootEl.GetAttribute("failed"));
            }
            else
            {
                fileTail.IsFailed = false;
            }
            XmlNodeList attList = rootEl.GetElementsByTagName("Attribute");
            foreach (XmlElement attEl in attList)
            {
                fileTail.Attributes.Add(attEl.InnerText);
            }
            var objectBase = rootEl.GetElementsByTagName("ObjectBase");
            if (objectBase.Count > 0)
            {
                foreach (XmlAttribute node in objectBase[0].Attributes)
                {
                    switch (node.Name)
                    {
                        case "Created":
                            fileTail.CreatedTime = Convert.ToInt64(node.InnerText);
                            break;
                        case "Author":
                            fileTail.CreatedBy = node.InnerText;
                            break;
                        case "Editor":
                            fileTail.ModifiedBy = node.InnerText;
                            break;
                        case "ContentSize":
                            fileTail.ContentSize = Convert.ToInt64(node.InnerText);
                            break;
                        default:
                            break;
                    }
                }
            }
            var list = rootEl.GetElementsByTagName("IsSystemFile");
            if (list.Count > 0)
            {
                fileTail.IsSystemFile = Convert.ToBoolean(list[0].InnerText);
            }
            var crcList = rootEl.GetElementsByTagName("CRC64");
            if (crcList != null && crcList.Count > 0)
            {
                fileTail.Crc64 = crcList[0].InnerText;
            }
            fileTail.ExtraInfo = rootEl.GetAttribute("extraInfo");
            var fullTextindexInfo = rootEl.GetElementsByTagName("FullTextindexInfo");
            foreach (XmlElement attEl in fullTextindexInfo)
            {
                fileTail.DetailInfoAttributes = attEl.OuterXml;
            }
            return fileTail;
        }

        public static string ToString(FileHeader fileheader)
        {
            XmlDocument mDoc = new XmlDocument();
            XmlElement xEl = mDoc.CreateElement("FileHeader");
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
            xEl.SetAttribute("isCurrentVersion", fileheader.IsCurrentVersion.ToString());
            xEl.SetAttribute("isChecked", fileheader.IsChecked.ToString());
            xEl.SetAttribute("isSelected", fileheader.IsSelect.ToString());
            xEl.SetAttribute("parentIsSelected", fileheader.ParentIsSelect.ToString());
            xEl.SetAttribute("listBaseType", fileheader.ListBaseType.ToString());
            xEl.SetAttribute("isFailed", fileheader.IsFailed.ToString());
            xEl.SetAttribute("isAppData", fileheader.IsAppData.ToString());
            xEl.SetAttribute("appDataName", fileheader.AppDataName);
            xEl.SetAttribute("Id", fileheader.Id);
            xEl.SetAttribute("StorageId", fileheader.StorageId);
            xEl.SetAttribute("BackUpJobId", fileheader.BackUpJobId);
            xEl.SetAttribute("ItemPathMD5", fileheader.ItemPathMD5);
            xEl.SetAttribute("ArchiveTime", fileheader.ArchiveTime.ToString());
            xEl.SetAttribute("nodeGuid", fileheader.UniqueId);
            if (!string.IsNullOrEmpty(fileheader.StubType))
            {
                xEl.SetAttribute("stubType", fileheader.StubType);
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