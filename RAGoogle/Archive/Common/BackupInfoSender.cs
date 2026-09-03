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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Media.Object.Common;
using AvePoint.Media.Service.ArchiverBackup.Backup;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using Newtonsoft.Json;
using RAGoogle.Models;
using RAGoogle.Models.GoogleObjectModel;
using RAGoogle.RecordsDisposal.Action.ExportOnly;
using System.Collections;
using System.Reflection;
using System.Xml;

namespace RAGoogle.Archive
{
    class BackupInfoSender
    {
        private static readonly AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private Hashtable fileHeaderAttribute = new Hashtable();

        /// <summary>
        /// For item multi-threads backup, return parent info
        /// </summary>
        public Hashtable FileHeaderAttribute
        {
            get
            {
                return fileHeaderAttribute;
            }
            internal set
            {
                fileHeaderAttribute = value;
            }
        }

        /// <summary>
        /// Item Level use this property
        /// </summary>
        public IAveBackupStream BackupStream { get; set; }

        /// <summary>
        /// Site ，Subsite use this property
        /// </summary>
        public IArchiverBackupDataWriter FileSender { get; set; }

        /// <summary>
        /// send header message of a backup item
        /// </summary>
        private XmlElement fileHeaderXml;


        public BackupInfoSender(IArchiverBackupDataWriter filesender)
        {
            FileSender = filesender;
            BackupStream = new WrapperBackupStreamV1(new ArchiverFileSender(FileSender));
            XmlDocument doc = new XmlDocument();
            fileHeaderXml = doc.CreateElement("FileHeader");
        }
        public XmlElement BackupHeader(string fullpath)
        {
            return WriteFileHeader(fileHeaderAttribute, GetProperties(fullpath));
        }
        public void BackupDriveHeader(ArchiveApproveReport entity, DriveProxy drive)
        {
            AddBackupFileHeaderAttribute(GDriveKeyWord.Name, entity.LeafName);
            AddBackupFileHeaderAttribute(GDriveKeyWord.Path, entity.FullPath);
            AddBackupFileHeaderAttribute(GDriveKeyWord.NodeType, ((int)GDriveDataType.SharedDrive).ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.ParentId, entity.ParentId);
            AddBackupFileHeaderAttribute(GDriveKeyWord.ParentIds, string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.FileType, string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.LabelIds, string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.MimeType, string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.Size, string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.CreatedTime, entity.DateTimeCreated.ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.ModifiedTime, entity.LastModifiedTime.ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.CreatedBy, entity.CreatedBy);
            AddBackupFileHeaderAttribute(GDriveKeyWord.ModifiedBy, entity.ModifiedBy);
            AddBackupFileHeaderAttribute(GDriveKeyWord.VersionNumber, string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.DataType, ((int)GDriveDataType.SharedDrive).ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.DriveId, drive.Id);
            AddBackupFileHeaderAttribute(GDriveKeyWord.DriveName, drive.Name);
            AddBackupFileHeaderAttribute(GDriveKeyWord.ItemId, drive.Id ?? string.Empty);
        }
        public void BackupMyDriveHeader(ArchiveApproveReport entity, string driveId, string driveName, string itemId)
        {
            AddBackupFileHeaderAttribute(GDriveKeyWord.Name, entity.LeafName);
            AddBackupFileHeaderAttribute(GDriveKeyWord.Path, entity.FullPath);
            AddBackupFileHeaderAttribute(GDriveKeyWord.NodeType, ((int)GDriveDataType.MyDrive).ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.ParentId, string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.ParentIds, string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.FileType, string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.LabelIds, string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.MimeType, string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.Size, string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.CreatedTime, entity.DateTimeCreated.ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.ModifiedTime, entity.LastModifiedTime.ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.CreatedBy, entity.CreatedBy);
            AddBackupFileHeaderAttribute(GDriveKeyWord.ModifiedBy, entity.ModifiedBy);
            AddBackupFileHeaderAttribute(GDriveKeyWord.VersionNumber, string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.DataType, ((int)GDriveDataType.MyDrive).ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.DriveId, driveId);
            AddBackupFileHeaderAttribute(GDriveKeyWord.DriveName, driveName);
            AddBackupFileHeaderAttribute(GDriveKeyWord.ItemId, itemId);
        }
        public void BackupGoogleFolderHeader(GoogleItemData item, string ruleName, string nodeType)
        {
            AddBackupFileHeaderAttribute(GDriveKeyWord.Name, item.Name);
            AddBackupFileHeaderAttribute(GDriveKeyWord.Path, item.RelativePath);
            AddBackupFileHeaderAttribute(GDriveKeyWord.NodeType, ((int)GDriveDataType.Folder).ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.ParentId, item.ParentId);
            AddBackupFileHeaderAttribute(GDriveKeyWord.ParentIds, item.ParentIds);
            AddBackupFileHeaderAttribute(GDriveKeyWord.FileType, item.Level.ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.LabelIds, string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.MimeType, "application/vnd.google-apps.folder");
            AddBackupFileHeaderAttribute(GDriveKeyWord.Size, string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.CreatedTime, item.CreatedTime.ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.ModifiedTime, item.ModifiedTime.ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.CreatedBy, item.CreatedBy);
            AddBackupFileHeaderAttribute(GDriveKeyWord.ModifiedBy, item.ModifiedBy);
            AddBackupFileHeaderAttribute(GDriveKeyWord.VersionNumber, string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.DataType, ((int)GDriveDataType.Folder).ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.DriveId, item.DriveId ?? string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.ItemId, item.Id ?? string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.RuleName, ruleName ?? string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.Permissions, JsonConvert.SerializeObject(item.Permissions));
            AddBackupFileHeaderAttribute(GDriveKeyWord.Permissions, item.CreatedBy);
            AddBackupFileHeaderAttribute(GDriveKeyWord.DriveName, item.DriveName ?? string.Empty);
        }
        public void BackupGoogleFileHeader(GoogleItemData item, DownloadedFileInfo fileInfo, string ruleName)
        {
            AddBackupFileHeaderAttribute(GDriveKeyWord.Name, item.Name);
            AddBackupFileHeaderAttribute(GDriveKeyWord.Path, item.RelativePath);
            AddBackupFileHeaderAttribute(GDriveKeyWord.NodeType, ((int)GDriveDataType.File).ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.ParentId, item.ParentId);
            AddBackupFileHeaderAttribute(GDriveKeyWord.ParentIds, item.ParentIds);
            AddBackupFileHeaderAttribute(GDriveKeyWord.FileType, item.Level.ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.LabelIds, string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.MimeType, item.MimeType);
            AddBackupFileHeaderAttribute(GDriveKeyWord.Size, fileInfo.Size.ToString() ?? "0");
            AddBackupFileHeaderAttribute(GDriveKeyWord.CreatedTime, item.CreatedTime.Ticks.ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.ModifiedTime, item.ModifiedTime.Ticks.ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.CreatedBy, item.CreatedBy);
            AddBackupFileHeaderAttribute(GDriveKeyWord.ModifiedBy, item.ModifiedBy);
            AddBackupFileHeaderAttribute(GDriveKeyWord.VersionNumber, string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.DataType, ((int)GDriveDataType.File).ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.DriveId, item.DriveId ?? string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.ItemId, item.Id ?? string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.RuleName, ruleName ?? string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.Permissions, JsonConvert.SerializeObject(item.Permissions));
            AddBackupFileHeaderAttribute(GDriveKeyWord.DriveName, item.DriveName ?? string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.MemberEmail, item.MemberEmail ?? string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.FileExtension, item.FileExtension ?? string.Empty);
        }
        public void BackupGoogleFileVersionHeader(GoogleItemData item, DownloadedFileInfo fileInfo, string ruleName, string version)
        {
            AddBackupFileHeaderAttribute(GDriveKeyWord.Name, item.Name);
            AddBackupFileHeaderAttribute(GDriveKeyWord.Path, item.RelativePath);
            AddBackupFileHeaderAttribute(GDriveKeyWord.NodeType, ((int)GDriveDataType.FileVersion).ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.ParentId, item.ParentId);
            AddBackupFileHeaderAttribute(GDriveKeyWord.ParentIds, item.ParentIds);
            AddBackupFileHeaderAttribute(GDriveKeyWord.FileType, item.Level.ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.LabelIds, string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.MimeType, item.MimeType);
            AddBackupFileHeaderAttribute(GDriveKeyWord.Size, fileInfo.Size.ToString() ?? "0");
            AddBackupFileHeaderAttribute(GDriveKeyWord.CreatedTime, item.CreatedTime.Ticks.ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.ModifiedTime, fileInfo.ModifiedTime.Ticks.ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.CreatedBy, item.CreatedBy);
            AddBackupFileHeaderAttribute(GDriveKeyWord.ModifiedBy, item.ModifiedBy);
            AddBackupFileHeaderAttribute(GDriveKeyWord.VersionNumber, version);
            AddBackupFileHeaderAttribute(GDriveKeyWord.DataType, ((int)GDriveDataType.FileVersion).ToString());
            AddBackupFileHeaderAttribute(GDriveKeyWord.DriveId, item.DriveId ?? string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.ItemId, item.Id ?? string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.RuleName, ruleName ?? string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.Permissions, JsonConvert.SerializeObject(item.Permissions));
            AddBackupFileHeaderAttribute(GDriveKeyWord.DriveName, item.DriveName ?? string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.MemberEmail, item.MemberEmail ?? string.Empty);
            AddBackupFileHeaderAttribute(GDriveKeyWord.FileExtension, item.FileExtension ?? string.Empty);
        }

        public void AddBackupFileHeaderAttribute(string key, string value)
        {
            if (fileHeaderAttribute.ContainsKey(key))
            {
                fileHeaderAttribute[key] = value;
            }
            else
            {
                fileHeaderAttribute.Add(key, value);
            }
        }
        public string GetProperties(string apUrl)
        {
            var doc = new XmlDocument();
            XmlElement headerExtraAttribute = doc.CreateElement("HeaderExtraAttribute");
            headerExtraAttribute.SetAttribute("APUrl", apUrl);
            return headerExtraAttribute.OuterXml;
        }

        private XmlElement WriteFileHeader(Hashtable attributes, string innerXml)
        {
            if (!string.IsNullOrEmpty(innerXml))
            {
                fileHeaderXml.InnerXml = innerXml;
            }
            foreach (object key in attributes.Keys)
            {
                fileHeaderXml.SetAttribute(key.ToString(), attributes[key]?.ToString());
            }
            BackupStream.WriteHead(fileHeaderXml.OuterXml);
            XmlElement ret = (XmlElement)fileHeaderXml.CloneNode(true);
            return ret;
        }


        public long BackupTail(bool successful)
        {
            return BackupTail("", successful);
        }

        public long BackupTail(string tail, bool successful)
        {
            FileSender.HandleTail(GenerateTailWithState(tail, successful));
            return 0;
        }

        private string GenerateTailWithState(string tail, bool successful)
        {
            int index = tail.IndexOf("<BackupDataExtraInfo", StringComparison.OrdinalIgnoreCase);
            string attributes = string.Empty;
            string extraInfo = string.Empty;
            if (index > 0)
            {
                attributes = tail.Substring(0, index);
                extraInfo = tail.Substring(index);
            }
            else
            {
                attributes = tail;
            }
            XmlDocument doc = new XmlDocument();
            XmlElement e = doc.CreateElement("FileTail");
            e.SetAttribute("extraInfo", extraInfo);
            e.InnerXml = attributes;
            if (!successful)
            {
                e.SetAttribute("failed", "true");
            }
            return e.OuterXml;
        }
    }
}