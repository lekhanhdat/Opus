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
    using System.Runtime.Serialization;
    using System.Text;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Tree.Object;
    #endregion

    #region Known Type Media Archiver
    [KnownType(typeof(MediaArchiverFileHeader))]
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FileHeader
    {
        [DataMember]
        public AveSharePointType Type { get; set; }

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
        public Boolean IsCurrentVersion { get; set; }
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
        public String TypeAsString
        {
            get { return ((char)Type).ToString(); }
        }
        [DataMember]
        public long ArchiveTime { get; set; }

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
    }
}