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




namespace RAGoogle.Restore.Content
{
    #region using directives
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Tree.Object;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;
    #endregion

    #region Known Type Media Archiver
    [KnownType(typeof(MediaArchiverFileHeader))]
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FileHeader
    {
        [DataMember]
        public int Type { get; set; }

        [DataMember]
        public String Path { get; set; }

        [DataMember]
        public BackupType BackupType { get; set; }

        [DataMember]
        public Int64 Sequence { get; set; }

        [DataMember]
        public String WebApp { get; set; }

        [DataMember]
        public String HeaderExtraAttribute { get; set; }

        [DataMember]
        public Int32 VersionFlag { get; set; }
        [DataMember]
        public Boolean IsSelect { get; set; }
        [DataMember]
        public Boolean ParentIsSelect { get; set; }

        [DataMember]
        public PropertyState Property { get; set; }

        [DataMember]
        public SecurityState Security { get; set; }

        [DataMember]
        public Int32 ListType { get; set; }

        [DataMember]
        public Int64 DataMode { get; set; }

        [DataMember]
        public String EncryptionInfo { get; set; }

        [DataMember]
        public List<Int64> ItemOffests { get; set; }

        [DataMember]
        public List<Int64> ItemLengths { get; set; }

        /// <summary>
        /// Site Id/Web Id/List Id/item.document.folder unique id,add on 2018/10/8
        /// </summary>
        [DataMember]
        public string UniqueId { get; set; }

        /// <summary>
        /// This field is used to mark whether the current node is selected as at site-level you do not need to restore if the current node is not checked
        /// </summary>
        [DataMember]
        public Boolean IsChecked { get; set; }

        /// <summary>
        /// This field is used to uniquely identify an item or a folder in BPOS
        /// </summary>
        [DataMember]
        public String ExtensionInfo { get; set; }

        /// <summary>
        /// This field is used to identify specified list type
        /// </summary>
        [DataMember]
        public Int32 ListBaseType { get; set; }

        [DataMember]
        public Boolean IsFailed { get; set; }

        /// <summary>
        /// This field is used to identify granular App data
        /// </summary>
        [DataMember]
        public Boolean IsAppData { get; set; }

        [DataMember]
        public String AppDataName { get; set; }

        [DataMember]
        public String PersonalSiteUserName { get; set; }
        public String StubType { get; set; }
        [DataMember]
        public String Id { get; set; } //COL_ID
        [DataMember]
        public String StorageId { get; set; } //COL_STORAGEPOLICYID
        [DataMember]
        public String BackUpJobId { get; set; } //COL_JOBID
        public String ItemPathMD5 { get; set; } //COL_PATH_MD5
        [DataMember]
        public String DriveId { get; set; } //COL_DRIVEID
        [DataMember]
        public String DriveName { get; set; } //COL_DRIVENAME
        [DataMember]
        public String VersionNumber { get; set; }
        [DataMember]
        public String ParentId { get; set; }
        public String TypeAsString
        {
            get { return ((char)Type).ToString(); }
        }
        [DataMember]
        public long ArchiveTime { get; set; }
        [DataMember]
        public string Name { get; internal set; }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("File Header: ");
            stringBuilder.AppendFormat("Type: {0}, ", this.Type);
            stringBuilder.AppendFormat("Path: {0}, ", this.Path);
            stringBuilder.AppendFormat("Backup Type: {0}, ", this.BackupType);
            stringBuilder.AppendFormat("Header Extra Attribute: {0}", this.HeaderExtraAttribute);
            stringBuilder.AppendFormat("Property: {0}, ", this.Property);
            stringBuilder.AppendFormat("Security: {0}, ", this.Security);
            stringBuilder.AppendFormat("Is Checked: {0}, ", this.IsChecked);
            stringBuilder.AppendFormat("Is Failed: {0}", this.IsFailed);
            stringBuilder.AppendFormat("IsAppData: {0}", this.IsAppData);
            stringBuilder.AppendFormat("PersonalSiteUserName: {0}", this.PersonalSiteUserName);
            stringBuilder.AppendFormat("Is select: {0}, ", this.IsSelect);
            stringBuilder.AppendFormat("Parent is select: {0}, ", this.ParentIsSelect);
            return stringBuilder.ToString();
        }
        public static string ToXmlString(FileHeader fileheader)
        {
            XmlDocument mDoc = new XmlDocument();
            XmlElement xEl = mDoc.CreateElement("FileHeader");
            xEl.SetAttribute("type", fileheader.Type.ToString());
            xEl.SetAttribute("path", fileheader.Path);
            xEl.SetAttribute("name", fileheader.Name);
            xEl.SetAttribute("sequence", fileheader.Sequence.ToString());
            xEl.SetAttribute("versionFlag", fileheader.VersionFlag.ToString());
            xEl.SetAttribute("webApp", fileheader.WebApp);
            xEl.SetAttribute("HeaderExtraAttribute", fileheader.HeaderExtraAttribute);
            xEl.SetAttribute("property", fileheader.Property == AvePoint.GCommon.Contract.Tree.Object.PropertyState.Checked ? "true" : "false");
            xEl.SetAttribute("security", fileheader.Security == AvePoint.GCommon.Contract.Tree.Object.SecurityState.Checked ? "true" : "false");
            xEl.SetAttribute("listType", fileheader.ListType.ToString());
            xEl.SetAttribute("dataMode", fileheader.DataMode.ToString());
            xEl.SetAttribute("encryptionInfo", fileheader.EncryptionInfo);
            xEl.SetAttribute("extensionInfo", fileheader.ExtensionInfo);
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
            xEl.SetAttribute("driveId", fileheader.DriveId);
            xEl.SetAttribute("driveName", fileheader.DriveName);
            xEl.SetAttribute("version", fileheader.VersionNumber);
            xEl.SetAttribute("parentId", fileheader.ParentId);
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