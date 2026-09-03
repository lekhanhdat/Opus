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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Backup;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Web;
using Microsoft.Exchange.WebServices.Data;
using System.Security.Cryptography;
using AvePoint.RA.Common.Util;
using ExchangeBackupUtility.Graph;

namespace RAExportCommon
{
    class EXOCustomizeProperty
    {
        #region DateTime Column Summary
        //        语法糖时间——UTC：
        //          1.Local&Office365：list级别，通过Wrapper API获取，Created，LastItemModifiedDate 获取的是UTC时间，并且Kind为UTC.
        //          2.1.Local：Folder，Document 级别，通过index获取Created，Modified，获取的是页面时间，Kind为Unspecified.
        //          2.2.1.Office365，Folder，级别，通过index获取Created，Modified，获取的是页面时间，Kind为Local.
        //          2.2.2.Office365，Document级别,通过index获取Created，Modified，获取的是UTC时间，并且Kind为UTC.
        //        SP时间类型Column——SP页面时间：
        //          1.Local&Office365 Folder，Document级别(List没有Column方法), 通过GetColumnValues获取时间类型的column，获取的是UTC时间，并且Kind为UTC.
        #endregion

        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        internal static string GetEXOFolderPropertyValue(bool ExchangeMetadataAsSource, string defaultValue, string columnName, Folder EXOFolder)
        {
            string value = string.Empty;
            if (ExchangeMetadataAsSource)
            {
                value = GetEXOFolderValueFromExchange(columnName, EXOFolder);
            }
            else
            {
                value = defaultValue;
            }
            return value;
        }

        internal static string GetEXOItemPropertyValue(bool ExchangeMetadataAsSource, string defaultValue, string columnName, Item EXOItem, string jobID, string exportPath, string filePath, string disposalClass)
        {
            string value = string.Empty;
            if (ExchangeMetadataAsSource)
            {
                value = GetEXOItemValueFromExchange(columnName, EXOItem, jobID, exportPath, filePath, disposalClass);
            }
            else
            {
                value = defaultValue;
            }
            return value;
        }

        internal static string GetEXOItemPropertyValue(bool ExchangeMetadataAsSource, string defaultValue, string columnName, IExchangeItem EXOItem, string jobID, string exportPath, string filePath, string disposalClass)
        {
            string value = string.Empty;
            if (ExchangeMetadataAsSource)
            {
                value = GetEXOItemValueFromExchange(columnName, EXOItem, jobID, exportPath, filePath, disposalClass);
            }
            else
            {
                value = defaultValue;
            }
            return value;
        }

        private static string GetEXOFolderValueFromExchange(string columnName, Folder EXOFolder)
        {
            string value = string.Empty;
            switch (columnName)
            {
                case "TimeNow":
                    value = value = DateTime.UtcNow.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                    break;
                case "GUID":
                case "ID":
                    value = EXOFolder.Id.ToString();
                    break;
                case "FilePath":
                    value = string.Empty;
                    break;
                case "Name":
                    value = EXOFolder.DisplayName;
                    break;
                case "UnreadCount":
                    value = EXOFolder.UnreadCount.ToString();
                    break;
                case "TotalCount":
                    value = EXOFolder.TotalCount.ToString();
                    break;
                case "ChildFolderCount":
                    value = EXOFolder.ChildFolderCount.ToString();
                    break;
                case "Size":
                    value = "0 KB";
                    break;
                default:
                    value = string.Empty;
                    break;
            }
            return value;
        }

        private static string GetEXOItemValueFromExchange(string columnName, Item EXOItem, string jobID, string exportPath, string filePath, string disposalClass)
        {
            string value = string.Empty;
            try
            {
                switch (columnName)
                {
                    case "TimeNow":
                        value = value = DateTime.UtcNow.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        break;
                    case "GUID":
                    case "ID":
                        value = EXOItem.Id.ToString();
                        break;
                    case "LastModifiedName":
                        value = EXOItem.LastModifiedName;
                        break;
                    case "Title":
                    case "Name":
                    case "Subject":
                        value = EXOItem.Subject ?? string.Empty;
                        break;
                    case "IsNew":
                        value = EXOItem.IsNew.ToString();
                        break;
                    case "IsUnmodified":
                        value = EXOItem.IsUnmodified.ToString();
                        break;
                    case "IsDraft":
                        value = EXOItem.IsDraft.ToString();
                        break;
                    case "DisplayCc":
                    case "SendCc":
                        var messageSendCc = EXOItem as EmailMessage;
                        if (messageSendCc != null && messageSendCc.CcRecipients != null && messageSendCc.CcRecipients.Count > 0)
                        {
                            value = string.Join("; ", messageSendCc.CcRecipients.Select(address => ExchangeUtils.EmailAddressToFormatString(address)));
                        }
                        else
                        {
                            value = EXOItem.DisplayCc;
                        }
                        break;
                    case "DisplayTo":
                    case "SendTo":
                        var messageSendTo = EXOItem as EmailMessage;
                        if (messageSendTo != null && messageSendTo.ToRecipients != null && messageSendTo.ToRecipients.Count > 0)
                        {
                            value = string.Join("; ", messageSendTo.ToRecipients.Select(address => ExchangeUtils.EmailAddressToFormatString(address)));
                        }
                        else
                        {
                            value = EXOItem.DisplayTo;
                        }
                        break;
                    case "Importance":
                        value = EXOItem.Importance.ToString();
                        break;
                    case "Size":
                        value = ExchangeUtils.ConvertByteToKB(EXOItem.Size.ToString()) + " KB";
                        break;
                    case "Modified":
                        value = EXOItem.LastModifiedTime.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        break;
                    case "Created":
                        value = EXOItem.DateTimeCreated.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        break;
                    case "DateTimeSent":
                    case "Send Time":
                        value = EXOItem.DateTimeSent.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        break;
                    case "DateTimeReceived":
                    case "Received Time":
                        value = EXOItem.DateTimeReceived.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        break;
                    case "AttachmentsCount":
                        value = EXOItem.Attachments.Count.ToString();
                        break;
                    case "File Type":
                        value = ExchangeUtils.ConvertMailTypeToDisplayType(EXOItem.ItemClass);
                        break;
                    case "FileContent":
                        PropertySet propertySet = new PropertySet(BasePropertySet.IdOnly, ItemSchema.MimeContent);
                        Item exoItem = Item.Bind(EXOItem.Service, EXOItem.Id, propertySet).GetAwaiter().GetResult();
                        using (Stream tempstream = new MemoryStream(exoItem.MimeContent.Content))
                        {
                            value = string.Format("{0}{1}{0}", "\n", VaultCover.ConverStreamToString(tempstream));
                        }
                        break;
                    case "Sender":
                    case "Created By":
                        #region Sender
                        var sender = string.Empty;
                        var message = EXOItem as EmailMessage;
                        if (message != null && message.Sender != null)
                        {
                            //sender = message.Sender.Name;
                            //sender = message.Sender.Address;
                            sender = ExchangeUtils.EmailAddressToFormatString(message.Sender);
                        }
                        value = sender;
                        break;
                    #endregion
                    case "Checksum":
                        string checkSum = string.Empty;
                        //using (var md5 = MD5.Create())
                        //{
                        //    using (Stream docStream = new FileStream(ExchangeUtils.GetEXOItemLocalMSGFilePath(jobID, EXOItem.Id.ToString(), EXOItem.Service), FileMode.Open, FileAccess.Read))
                        //    {
                        //        var hash = md5.ComputeHash(docStream);
                        //        checkSum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                        //    }
                        //}
                        value = checkSum;
                        break;
                    case "ExportPath":
                        value = exportPath;
                        break;
                    case "FilePath":
                        value = filePath;
                        break;
                    case "Disposal Class":
                    case "Disposal class":
                    case "Disposition Authority":
                        #region disposal class logic.
                        value = disposalClass;
                        break;
                    #endregion
                    #region VEO-V3
                    case "FileSize":
                        var tempPath = ExchangeUtils.GetEXOItemLocalMSGFilePath(jobID, EXOItem.Id.ToString(), EXOItem.Service);
                        value = new FileInfo(tempPath).Length.ToString();
                        break;
                    case "NewGuid":
                        value = Guid.NewGuid().ToString();
                        break;
                    #endregion
                    default:
                        #region EXOItem ExtendedProperties 
                        ExtendedProperty extendedProperties = EXOItem.ExtendedProperties.Where(x => x.PropertyDefinition.Name == columnName).FirstOrDefault();
                        if (extendedProperties == null)
                        {
                            value = string.Empty;
                        }
                        else
                        {
                            value = extendedProperties.Value.ToString();
                        }
                        break;
                        #endregion
                }
            }
            catch (Exception ex)
            {
                mLog.Info("Can't GetEXOItemValueFromExchange.Message:{0}.", ex.ToString());
                value = string.Empty;
            }
            return value;
        }

        private static string GetEXOItemValueFromExchange(string columnName, IExchangeItem EXOItem, string jobID, string exportPath, string filePath, string disposalClass)
        {
            string value = string.Empty;
            try
            {
                switch (columnName)
                {
                    case "TimeNow":
                        value = value = DateTime.UtcNow.ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        break;
                    case "GUID":
                    case "ID":
                        value = EXOItem.ItemId.ToString();
                        break;
                    case "LastModifiedName":
                        value = EXOItem.ModifiedBy;
                        break;
                    case "Title":
                    case "Name":
                    case "Subject":
                        value = EXOItem.ItemName ?? string.Empty;
                        break;
                    case "IsNew":
                        value = EXOItem.IsNew.ToString();
                        break;
                    case "IsUnmodified":
                        value = EXOItem.IsUnmodified.ToString();
                        break;
                    case "IsDraft":
                        value = EXOItem.IsDraft.ToString();
                        break;
                    case "DisplayCc":
                    case "SendCc":
                        if (!string.IsNullOrEmpty(EXOItem.DisplayCc))
                            value = EXOItem.DisplayCc;
                        break;
                    case "DisplayTo":
                    case "SendTo":
                        if (!string.IsNullOrEmpty(EXOItem.DisplayTo))
                            value = EXOItem.DisplayTo;
                        break;
                    case "Importance":
                        value = EXOItem.Importance.ToString();
                        break;
                    case "Size":
                        value = ExchangeUtils.ConvertByteToKB(EXOItem.ItemSize.ToString()) + " KB";
                        break;
                    case "Modified":
                        value = EXOItem.Modified.ToLocalTime().ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        break;
                    case "Created":
                        value = EXOItem.Created.ToLocalTime().ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        break;
                    case "DateTimeSent":
                    case "Send Time":
                        value = EXOItem.SendDateUTC.ToLocalTime().ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        break;
                    case "DateTimeReceived":
                    case "Received Time":
                        value = EXOItem.Received.ToLocalTime().ToString("yyyy'-'MM'-'ddTHH':'mm':'sszzz");
                        break;
                    case "AttachmentsCount":
                        value = EXOItem.AttachmentCount.ToString();
                        break;
                    case "File Type":
                        value = ExchangeUtils.ConvertMailTypeToDisplayType(EXOItem.ItemType);
                        break;
                    case "FileContent":
                        using (var tempstream = new MemoryStream())
                        {
                            EXOItem.GetMimeContentAsync().ExecuteAsyncTask().CopyTo(tempstream);
                            tempstream.Seek(0, SeekOrigin.Begin);
                            value = string.Format("{0}{1}{0}", "\n", VaultCover.ConverStreamToString(tempstream));
                        }
                        break;
                    case "Sender":
                    case "Created By":
                        #region Sender
                        var sender = string.Empty;
                        if (!string.IsNullOrEmpty(EXOItem.Sender))
                        {
                            //sender = message.Sender.Name;
                            //sender = message.Sender.Address;
                            sender = ExchangeUtils.EmailAddressToFormatString(EXOItem.Sender);
                        }
                        value = sender;
                        break;
                    #endregion
                    case "Checksum":
                        string checkSum = string.Empty;
                        //using (var md5 = MD5.Create())
                        //{
                        //    using (Stream docStream = new FileStream(ExchangeUtils.GetEXOItemLocalMSGFilePath(jobID, EXOItem.Id.ToString(), EXOItem.Service), FileMode.Open, FileAccess.Read))
                        //    {
                        //        var hash = md5.ComputeHash(docStream);
                        //        checkSum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                        //    }
                        //}
                        value = checkSum;
                        break;
                    case "ExportPath":
                        value = exportPath;
                        break;
                    case "FilePath":
                        value = filePath;
                        break;
                    case "Disposal Class":
                    case "Disposal class":
                    case "Disposition Authority":
                        #region disposal class logic.
                        value = disposalClass;
                        break;
                    #endregion
                    #region VEO-V3
                    case "FileSize":
                        var tempPath = ExchangeUtils.GetEXOItemLocalMSGFilePath(jobID, EXOItem).ExecuteAsyncTask();
                        value = new FileInfo(tempPath).Length.ToString();
                        break;
                    case "NewGuid":
                        value = Guid.NewGuid().ToString();
                        break;
                    #endregion
                    default:
                        #region EXOItem ExtendedProperties 
                        var extendedProperty = EXOItem.GetProperties()[columnName];
                        if (extendedProperty == null)
                        {
                            value = string.Empty;
                        }
                        else
                        {
                            value = extendedProperty.ToString();
                        }
                        break;
                        #endregion
                }
            }
            catch (Exception ex)
            {
                mLog.Info("Can't GetEXOItemValueFromExchange.Message:{0}.", ex.ToString());
                value = string.Empty;
            }
            return value;
        }
    }
}
