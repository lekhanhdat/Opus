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
    using AvePoint.GCommon.Contract.Storage.Entity;
    #endregion
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EndUserArchiverViewNodeDto
    {
        [DataMember]
        public EndUserArchiverNodeType NodeType { set; get; } // Enum 
        [DataMember]
        public String Name { set; get; }
        [DataMember]
        public String Url { get; set; }
        [DataMember]
        public Boolean IsHold { set; get; }
        [DataMember]
        public Int64 FinalDisposition { set; get; }
        [DataMember]
        public String NodeMd5Value { set; get; }
        [DataMember]
        public String Attribute { set; get; }
        [DataMember]
        public Int64 ArchiveTime { get; set; }
        [DataMember]
        public Boolean HasNextPage { get; set; }
        [DataMember]
        public SelectAllStatus SelectAllStatus { get; set; }
        [DataMember]
        public EndUserArchiverViewNodeDto ParentNode { set; get; }
        [DataMember]
        public List<EndUserArchiverViewNodeDto> ChildNodes { set; get; }
        [DataMember]
        public String TimeZoneId { get; set; }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("End User Archiver View Node DTO: ");
            stringBuilder.AppendFormat("Node Type: {0}, ", this.NodeType);
            stringBuilder.AppendFormat("Name: {0}, ", this.Name);
            stringBuilder.AppendFormat("Url: {0}, ", this.Url);
            stringBuilder.AppendFormat("Select All Status: {0}", this.SelectAllStatus);
            stringBuilder.AppendFormat("Time Zone ID: {0}", this.TimeZoneId);
            return stringBuilder.ToString();
        }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EndUserArchiverNodeType
    {
        [EnumMember]
        Root = -2,
        [EnumMember]
        Farm = -1,
        [EnumMember]
        WebApplication = 2,
        [EnumMember]
        SiteCollection = 100,
        [EnumMember]
        Site = 200,
        [EnumMember]
        Lists = 201,
        [EnumMember]
        Sites = 202,
        [EnumMember]
        List = 300,
        [EnumMember]
        Library = 301,
        [EnumMember]
        Folder = 400,
        [EnumMember]
        Folders = 401,
        [EnumMember]
        RootFolder = 402, //list rootfolder & web rootfolder
        [EnumMember]
        Item = 500,
        [EnumMember]
        Items = 501,
        [EnumMember]
        Document = 600,
        [EnumMember]
        Attachment = 601
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SelectAllStatus
    {
        [EnumMember]
        Undefined = -1,
        [EnumMember]
        Checked = 1,
        [EnumMember]
        Unchecked = 0
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EndUserArchiverViewInfo
    {
        [DataMember]
        public LogicalDeviceDto IndexDevice { get; set; }
        [DataMember]
        public String PathMD5 { get; set; }
        [DataMember]
        public String FarmName { get; set; }
        [DataMember]
        public String WebAppUrl { get; set; }
        [DataMember]
        public String SiteUrl { get; set; }
        [DataMember]
        public Int32 OffSet { get; set; }
        [DataMember]
        public Int32 Length { get; set; }
        [DataMember]
        public Boolean NeedNodeMap { get; set; }

        public override String ToString()
        {
            return String.Format("End User Archiver View Info: Site Url: {0}, Need Node Map: {1}, Index Device: {2}",
                this.SiteUrl,
                this.NeedNodeMap,
                this.IndexDevice);
        }
    }
}
