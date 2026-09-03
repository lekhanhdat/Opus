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
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Media.Object.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GDriveFileHeader
    {
        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public Int32 NodeType { get; set; }

        [DataMember]
        public GDriveDataType DataType { get; set; }

        [DataMember]
        public String ParentId { get; set; }
        [DataMember]
        public String ParentIds { get; set; }
        /// <summary>
        /// 0 means normal data, 1 means deleted data
        /// </summary>
        [DataMember]
        public Int32 BackupType { get; set; }

        [DataMember]
        public long CreatedTime { get; set; }

        [DataMember]
        public long ModifiedTime { get; set; }

        [DataMember]
        public string CreatedBy { get; set; }

        [DataMember]
        public string DriveId { get; set; }

        [DataMember]
        public string DriveName { get; set; }

        [DataMember]
        public string Path { get; set; }
        public string ParentName { get; set; }
        public int ChildCount { get; set; }
        public string MemberEmail { get; private set; }
        public string ItemId { get; set; }
        public string VersionNumberStr { get; set; }
        public GDriveFileHeader() { }
        public GDriveFileHeader(String fileHeaderXml)
        {
            var docment = new XmlDocument();
            docment.LoadXml(fileHeaderXml);
            var rootElement = docment.DocumentElement;
            if (rootElement.HasAttribute(GDriveKeyWord.Name))
            {
                this.Name = rootElement.GetAttribute(GDriveKeyWord.Name);
            }
            if (rootElement.HasAttribute(GDriveKeyWord.ParentIds))
            {
                this.ParentIds = rootElement.GetAttribute(GDriveKeyWord.ParentIds);
            }
            if (rootElement.HasAttribute(GDriveKeyWord.ParentId))
            {
                this.ParentId = rootElement.GetAttribute(GDriveKeyWord.ParentId);
            }
            if (rootElement.HasAttribute(GDriveKeyWord.NodeType))
            {
                this.NodeType = int.Parse(rootElement.GetAttribute(GDriveKeyWord.NodeType));
            }
            if (rootElement.HasAttribute(GDriveKeyWord.DataType))
            {
                this.DataType = (GDriveDataType)int.Parse(rootElement.GetAttribute(GDriveKeyWord.DataType));
            }
            if (rootElement.HasAttribute(GDriveKeyWord.Path))
            {
                this.Path = rootElement.GetAttribute(GDriveKeyWord.Path);
            }
            if (rootElement.HasAttribute(GDriveKeyWord.CreatedTime))
            {
                this.CreatedTime = ParseToTicks(rootElement.GetAttribute(GDriveKeyWord.CreatedTime));
            }
            if (rootElement.HasAttribute(GDriveKeyWord.ModifiedTime))
            {
                this.ModifiedTime = ParseToTicks(rootElement.GetAttribute(GDriveKeyWord.ModifiedTime));
            }
            if (rootElement.HasAttribute(GDriveKeyWord.CreatedBy))
            {
                this.CreatedBy = rootElement.GetAttribute(GDriveKeyWord.CreatedBy);
            }
            if (rootElement.HasAttribute(GDriveKeyWord.DriveId))
            {
                this.DriveId = rootElement.GetAttribute(GDriveKeyWord.DriveId);
            }
            if (rootElement.HasAttribute(GDriveKeyWord.DriveName))
            {
                this.DriveName = rootElement.GetAttribute(GDriveKeyWord.DriveName);
            }
            if (rootElement.HasAttribute(GDriveKeyWord.MemberEmail))
            {
                this.MemberEmail = rootElement.GetAttribute(GDriveKeyWord.MemberEmail);
            }
            if (rootElement.HasAttribute(GDriveKeyWord.ItemId))
            {
                this.ItemId = rootElement.GetAttribute(GDriveKeyWord.ItemId);
            }
            if (rootElement.HasAttribute(GDriveKeyWord.VersionNumber))
            {
                this.VersionNumberStr = rootElement.GetAttribute(GDriveKeyWord.VersionNumber);
            }
            docment.RemoveAll();
        }
        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("GDrive File Header: ");
            stringBuilder.AppendFormat("Name: {0}, ", this.Name);
            stringBuilder.AppendFormat("ParentIds: {0}, ", this.ParentIds);
            stringBuilder.AppendFormat("DataType: {0}, ", this.DataType);
            stringBuilder.AppendFormat("BackupType: {0}, ", this.BackupType);
            stringBuilder.AppendFormat("NodeType: {0}, ", this.NodeType);
            return stringBuilder.ToString();
        }

        private long ParseToTicks(string xmlInput)
        {
            if (!string.IsNullOrWhiteSpace(xmlInput))
            {
                long ticksValue;
                if (long.TryParse(xmlInput, out ticksValue))
                {
                    return ticksValue;
                }
                else
                {
                    DateTime dt;
                    if (DateTime.TryParse(xmlInput, out dt))
                    {
                        return dt.Ticks;
                    }
                }
            }

            return DateTime.UtcNow.Ticks;
        }
    }
}
