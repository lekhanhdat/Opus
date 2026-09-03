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

    #endregion using directives

    //[KnownType(typeof(ArchiverFullTextIndexSearchRequestResult))]
    //[KnownType(typeof(VaultFullTextIndexSearchRequestResult))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FullTextIndexSearchRequestResult
    {
        [DataMember]
        public FullTextIndexJobType IndexType { get; set; }

        [DataMember]
        public Int32 TotalDocs { get; set; }

        [DataMember]
        public List<SearchRequestResult> SeachResults { get; set; }

        public FullTextIndexSearchRequestResult()
        {
            SeachResults = new List<SearchRequestResult>();
        }

        public override String ToString()
        {
            return String.Format("Index Type: {0}", this.IndexType);
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SearchRequestResult
    {
        [DataMember]
        public String Id { get; set; }

        [DataMember]
        public String FullTextIndexJobId { get; set; }

        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public String SubJobId { get; set; }

        [DataMember]
        public String FarmName { get; set; }

        [DataMember]
        public String SiteUrl { get; set; }
        /// <summary>
        /// webappid, 用于处理一个Site Url同时出现在两个WEbapp下的情况
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public String WebAppId { set; get; }

        [DataMember]
        public String Title { get; set; }

        [DataMember]
        public String Version { get; set; }

        [DataMember]
        public String FullPath { get; set; }

        [DataMember]
        public AveSharePointType SpType { get; set; }

        [DataMember]
        public string FileType { get; set; }

        [DataMember]
        public string ContentType { get; set; }

        [DataMember]
        public String PathMD5 { get; set; }

        [DataMember]
        public String ParentPathMD5 { get; set; }

        [DataMember]
        public Int64 CreateTime { get; set; }

        [DataMember]
        public Int64 CreateTimeUtc { get; set; }

        [DataMember]
        public String CreateBy { get; set; }

        [DataMember]
        public Int64 ModifiedTime { get; set; }

        [DataMember]
        public String ModifiedBy { get; set; }

        [DataMember]
        public Int64 LastModifiedTime { get; set; }

        [DataMember]
        public Int64 LastModifiedTimeUtc { get; set; }

        [DataMember]
        public Int64 ArchiverTime { get; set; }

        [DataMember]
        public Int64 ArchiverTimeUtc { get; set; }

        [DataMember]
        public String ArchiverBy { get; set; }

        [DataMember]
        public Single Score { get; set; }

        [DataMember]
        public String Size { get; set; }

        [DataMember]
        public String TimeZoneInfoId { get; set; }

        [DataMember]
        public String Summary { get; set; }

        [DataMember]
        public String ScopeId { get; set; }

        [DataMember]
        public Dictionary<string, string> ExtraFields { get; set; }

        public override String ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("{");
            buf.Append("Name: ").Append(Name).Append(" ")
               .Append("Id: ").Append(Id).Append(" ")
               .Append("Score: ").Append(Score).Append(" ")
               .Append("FullTextIndexJobId: ").Append(FullTextIndexJobId).Append(" ")
               .Append("BackupJobId: ").Append(SubJobId).Append(" ")
               .Append("TimeZoneInfoId: ").Append(TimeZoneInfoId).Append(" ")
               .Append("Summary: ").Append(Summary);
            buf.Append("}").Append("\n");
            return buf.ToString();
        }

        public string ItemName
        {
            get
            {
                string itemName = this.Name;
                if (this.SpType == AveSharePointType.TYPE_ATTACHMENTS
                    || this.SpType == AveSharePointType.TYPE_DOCUMENT
                    || this.SpType == AveSharePointType.TYPE_LISTITEM
                    || this.SpType == AveSharePointType.TYPE_LISTITEMVERSION
                    || this.SpType == AveSharePointType.TYPE_VERSION)
                {
                    int flag = this.Name.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);
                    if (flag >= 0)
                    {
                        itemName = this.Name.Substring(0, flag);
                    }
                }
                return itemName;
            }
        }

        public float ItemMajorVersion
        {
            get
            {
                float majorVersion = float.MaxValue;
                if (this.SpType == AveSharePointType.TYPE_DOCUMENT
                    || this.SpType == AveSharePointType.TYPE_LISTITEM
                    || this.SpType == AveSharePointType.TYPE_LISTITEMVERSION
                    || this.SpType == AveSharePointType.TYPE_VERSION)
                {
                    int flag = this.Name.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);
                    if (flag >= 0)
                    {
                        string versionStr = this.Name.Substring(flag + 1);
                        String[] version = versionStr.Split('.');
                        if (!float.TryParse(version[0], out majorVersion))
                        {
                            majorVersion = float.MaxValue;
                        }
                    }
                }
                return majorVersion;
            }
        }

        public float ItemMinorVersion
        {
            get
            {
                float minorVersion = float.MaxValue;
                if (this.SpType == AveSharePointType.TYPE_DOCUMENT
                     || this.SpType == AveSharePointType.TYPE_LISTITEM
                     || this.SpType == AveSharePointType.TYPE_LISTITEMVERSION
                     || this.SpType == AveSharePointType.TYPE_VERSION)
                {
                    int flag = this.Name.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);
                    if (flag >= 0)
                    {
                        string versionStr = this.Name.Substring(flag + 1);
                        String[] version = versionStr.Split('.');
                        if (!float.TryParse(version[1], out minorVersion))
                        {
                            minorVersion = float.MaxValue;
                        }
                    }
                }
                return minorVersion;
            }
        }
    }
    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class ArchiverFullTextIndexSearchRequestResult : FullTextIndexSearchRequestResult
    //{
    //    [DataMember]
    //    public List<SearchRequestResult> SeachResults { get; set; }

    //    public ArchiverFullTextIndexSearchRequestResult()
    //    {
    //        SeachResults = new List<SearchRequestResult>();
    //    }
    //}

    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class VaultFullTextIndexSearchRequestResult : FullTextIndexSearchRequestResult
    //{
    //}
}