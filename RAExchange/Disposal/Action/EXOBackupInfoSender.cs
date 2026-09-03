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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Media.Service.ArchiverBackup.Backup;
using AvePoint.Media.Service.ArchiverBackup.Restore;
using AvePoint.RA.Common.GraphApi.Mail;
using AvePoint.RA.RAExchange.Discover;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using DocAveOnline.WebApi.Contracts;
using ExchangeBackupUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NodeType = AvePoint.GCommon.Contract.Tree.Object.NodeType;
namespace AvePoint.RA.RAExchange.Disposal.Action
{
    public class EXOBackupInfoSender
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
        public EXOBackupInfoSender(IArchiverBackupDataWriter filesender)
        {
            FileSender = filesender;
            BackupStream = new WrapperBackupStreamV1(new EXOArchiverFileSender(FileSender));
            XmlDocument doc = new XmlDocument();
            fileHeaderXml = doc.CreateElement("FileHeader");
        }
        private XmlElement WriteFileHeader(Hashtable attributes, string innerXml)
        {
            try
            {
                if (!string.IsNullOrEmpty(innerXml))
                {
                    this.fileHeaderXml.InnerXml = innerXml;
                }
                foreach (object key in attributes.Keys)
                {
                    fileHeaderXml.SetAttribute(key.ToString(), Convert.ToString(attributes[key]));
                }
                BackupStream.WriteHead(fileHeaderXml.OuterXml);
                return (XmlElement)fileHeaderXml.CloneNode(true);
            }
            finally
            {
                attributes.Clear();
            }
        }
        public long BackupTail()
        {
            FileSender.HandleTail("");
            return 0;
        }

        public XmlElement BackupEXOItemHeader(ThreadPost exoItem)
        {
            //fileHeaderAttribute.Add(EXOCommonWord.Name, exoItem.ItemName ==null?"":exoItem.ItemName.TrimEnd((char)0x12));
            fileHeaderAttribute.Add(EXOCommonWord.ParentFullPath, exoItem.ConversationId);
            fileHeaderAttribute.Add(EXOCommonWord.NodeType, (int)NodeType.EOItem);
            fileHeaderAttribute.Add(EXOCommonWord.DataType, (int)ExchangeDataType.Post);
            //fileHeaderAttribute.Add(EXOCommonWord.DisplayTo, exoItem.);
            fileHeaderAttribute.Add(EXOCommonWord.Sender, exoItem.Sender.MailAddress.Address);
            fileHeaderAttribute.Add(EXOCommonWord.Category, exoItem.Categories);
            //fileHeaderAttribute.Add(EXOCommonWord.SendDate, exoItem..Ticks);
            fileHeaderAttribute.Add(EXOCommonWord.HasAttach, (exoItem.HasAttachments || (exoItem.Body!=null && exoItem.Body.Content.Contains("data-imagetype"))));
            fileHeaderAttribute.Add(EXOCommonWord.Path, exoItem.Id);
            return WriteFileHeader(fileHeaderAttribute, string.Empty);
        }
        public XmlElement BackupEXOEventHeader(GroupCalendarEvent calendarEvent)
        {
            fileHeaderAttribute.Add(EXOCommonWord.Name, calendarEvent.Subject);
            fileHeaderAttribute.Add(EXOCommonWord.ParentFullPath, calendarEvent.CalendarId);
            fileHeaderAttribute.Add(EXOCommonWord.NodeType, (int)NodeType.EOCalendarEvent);
            fileHeaderAttribute.Add(EXOCommonWord.DataType, (int)ExchangeDataType.CalendarEvent);
            //fileHeaderAttribute.Add(EXOCommonWord.DisplayTo, exoItem.);
            //fileHeaderAttribute.Add(EXOCommonWord.Sender, calendarEvent.Sender.MailAddress.Address);
            //fileHeaderAttribute.Add(EXOCommonWord.Category, exoItem.Categories);
            //fileHeaderAttribute.Add(EXOCommonWord.SendDate, exoItem..Ticks);
            fileHeaderAttribute.Add(EXOCommonWord.HasAttach, calendarEvent.HasAttachments);
            fileHeaderAttribute.Add(EXOCommonWord.Path, calendarEvent.Id);
            return WriteFileHeader(fileHeaderAttribute, string.Empty);
        }
        public XmlElement BackupEXOItemAttachmentHeader(PostAttachment attachment)
        {
            fileHeaderAttribute.Add(EXOCommonWord.Name, attachment.Name);
            fileHeaderAttribute.Add(EXOCommonWord.ParentFullPath, attachment.ParentItemId);
            fileHeaderAttribute.Add(EXOCommonWord.NodeType, (int)NodeType.EOItemAttachment);
            fileHeaderAttribute.Add(EXOCommonWord.DataType, (int)ExchangeDataType.Attachment);
            //fileHeaderAttribute.Add(EXOCommonWord.DisplayTo, attachment.d);
            //fileHeaderAttribute.Add(EXOCommonWord.Sender, attachment.Sender.MailAddress.Address);
            //fileHeaderAttribute.Add(EXOCommonWord.Category, exoItem.Categories);
            //fileHeaderAttribute.Add(EXOCommonWord.SendDate, exoItem..Ticks);
            //fileHeaderAttribute.Add(EXOCommonWord.HasAttach, exoItem.HasAttachments);
            fileHeaderAttribute.Add(EXOCommonWord.Path, attachment.Id);
            return WriteFileHeader(fileHeaderAttribute, string.Empty);
        }
        public XmlElement BackupEXOConversationHeader(GroupConversation conversation)
        {
            fileHeaderAttribute.Add(EXOCommonWord.Name, conversation.Topic);
            fileHeaderAttribute.Add(EXOCommonWord.ParentFullPath, conversation.Id);
            fileHeaderAttribute.Add(EXOCommonWord.NodeType, (int)NodeType.EOMailFolder);
            fileHeaderAttribute.Add(EXOCommonWord.DataType, (int)ExchangeDataType.Folder);
            fileHeaderAttribute.Add(EXOCommonWord.Path, conversation.Id);
            return WriteFileHeader(fileHeaderAttribute, string.Empty);
        }
        public XmlElement BackupEXOCalendarHeader(GroupCalendar calendar)
        {
            fileHeaderAttribute.Add(EXOCommonWord.Name, calendar.Name);
            fileHeaderAttribute.Add(EXOCommonWord.ParentFullPath, calendar.Id);
            fileHeaderAttribute.Add(EXOCommonWord.NodeType, (int)NodeType.EOCalendarFolder);
            fileHeaderAttribute.Add(EXOCommonWord.DataType, (int)ExchangeDataType.Calendar);
            fileHeaderAttribute.Add(EXOCommonWord.Path, calendar.Id);
            return WriteFileHeader(fileHeaderAttribute, string.Empty);
        }
        public XmlElement BackupEXOMailBoxHeader(ExchangeOnlineTreeNodeDto exoMailBox)
        {
            fileHeaderAttribute.Add(EXOCommonWord.Name, exoMailBox.Name);
            fileHeaderAttribute.Add(EXOCommonWord.ParentFullPath, "");
            fileHeaderAttribute.Add(EXOCommonWord.NodeType, (int)NodeType.EOMailBox);
            fileHeaderAttribute.Add(EXOCommonWord.DataType, (int)ExchangeDataType.Mailbox);
            fileHeaderAttribute.Add(EXOCommonWord.Path, exoMailBox.Name);
            return WriteFileHeader(fileHeaderAttribute, string.Empty);
        }
        private string GetParentFullPath(string InternalPath)
        {
            string parentFullPath = string.Empty;
            parentFullPath = !InternalPath.Contains((char)0x12) ?InternalPath :InternalPath.Remove(InternalPath.LastIndexOf((char)0x12));
            return parentFullPath;
        }
        protected virtual int GetObjectType(string type)
        {
            return type.ToLowerInvariant() switch
            {
                "mailbox" => (int)NodeType.EOMailBox,
                "ipf.note" => (int)NodeType.EOMailFolder,
                "ipf.appointment" => (int)NodeType.EOCalendarFolder,
                "ipf.contact" => (int)NodeType.EOContactsFolder,
                "ipf.task" => (int)NodeType.EOTasksFolder,
                "ipf.journal" => (int)NodeType.EOJournalFolder,
                "ipf.stickynote" => (int)NodeType.EONotesFolder,
                "ipf.note.outlookhomepage" => (int)NodeType.EORSSFeedsFolder,
                "ipf.note.infopathform" => (int)NodeType.EOInfoPathsFolder,
                _ => (int)NodeType.EOMailFolder,
            };
        }
    }
}
