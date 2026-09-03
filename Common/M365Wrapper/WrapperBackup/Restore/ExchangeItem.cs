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

namespace ExchangeRestoreUtility
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using ExchangeUtility.Graph;
    using Microsoft.Exchange.WebServices.Data;

    using EwsTask = Microsoft.Exchange.WebServices.Data.Task;


    #endregion

    public class ExchangeItem : ExchangeObjectBase
    {
        #region Properties
        private const int CONTENT_BUFFER_Size = 64 * 1024;
        private byte[] contentBuffer = new byte[CONTENT_BUFFER_Size];
        private ExchangeService service = null;
        private Item currentItem = null;
        private bool largeFile;

        //public static Folder parentFolder = null;
        public string ItemName { get; private set; }

        public string ItemId { get; private set; }

        public string ItemType { get; private set; }

        //public string ItemPath { get; private set; }
        public string ParentFolderId { get; private set; }

        public string ExchangeId { get; private set; }

        public DateTime Modified { get; private set; }

        public int ItemSize { get; private set; }

        public int MessageFlag { get; private set; }

        #endregion

        public ExchangeItem(ExchangeFolder parentExchangeFolder)
            : base(parentExchangeFolder.AuthObject)
        {
            this.service = CloneExchangeService(parentExchangeFolder.Service, -1);
            this.ParentFolderId = parentExchangeFolder.ParentFolderId.UniqueId;
        }

        //internal ExchangeItem(Item item)
        //{
        //    service = item.Service;
        //    parentFolderId = item.ParentFolderId.UniqueId;
        //    currentItem = item;
        //    GenerateItemInfo(item);
        //}

        public void Open(string targetItemId)
        {
            this.currentItem = Item.Bind(service, targetItemId).ExecuteAsyncTask();
            GenerateItemInfo(this.currentItem);
        }

        public void ReInitService()
        {
            this.service = CloneExchangeService(this.service, -1);
        }

        public void UpdateItemIdField(string targetItemId, string sourceItemId)
        {
            if (string.IsNullOrEmpty(targetItemId)) return;

            string exchangeId = ExchangeConstants.ConvertItemId(sourceItemId);
            int retryTimes = 0;
            var def = new ExtendedPropertyDefinition(new Guid("0006200A-0000-0000-C000-000000000046"), 0xF555, MapiPropertyType.String);
            while (retryTimes < 5)
            {
                try
                {
                    currentItem = Item.Bind(service, targetItemId).ExecuteAsyncTask();
                    currentItem.SetExtendedProperty(def, sourceItemId);
                    if (currentItem is Appointment)
                    {
                        Appointment currentAppointment = currentItem as Appointment;
                        currentAppointment.Update(ConflictResolutionMode.AlwaysOverwrite, SendInvitationsOrCancellationsMode.SendToNone);
                    }
                    else
                    {
                        currentItem.Update(ConflictResolutionMode.AlwaysOverwrite);
                    }
                    break;
                }
                catch (Exception ex)
                {
                    logger.Warn("An error occurred while Set item {0} ExtendedPropery. Try times: {2}, Message : {1}.", exchangeId, ex.ToString(), retryTimes);
                    Thread.Sleep(1000);
                }
                retryTimes++;
            }
        }

        public string ImportItem(Stream content)
        {
            string newItemId = string.Empty;

            var uploadItems = new UploadItemParameter[]
            {
                new UploadItemParameter
                {
                    CreateAction =  CreateAction.CreateNew,
                    // DataStream = content,
                    // DataSize = GetDataSize(content),
                    IsAssociated = false,
                    ParentFolderId = new FolderId(ParentFolderId)
                },
            };

            int retryTimes = 0;
            while (retryTimes < 10)
            {
                try
                {
                    newItemId = InternalImportItem(uploadItems);
                    if (!string.IsNullOrEmpty(newItemId))
                    {
                        break;
                    }
                    logger.Error("Cannot import the item. Retry times: {0}.", retryTimes);
                    Thread.Sleep(10000);
                }
                catch (EWSNeverRetryException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.Error("Cannot import the item. retry times: {1}. Reason: {0}", ex.ToString(), retryTimes);
                    Thread.Sleep(30000);
                }
                retryTimes++;
            }

            return newItemId;
        }

        private long? GetDataSize(Stream stream)
        {
            try
            {
                return stream.Length;
            }
            catch (NotSupportedException ex)
            {
                logger.Warn("Failed to get content stream length, stream type: {0}, error: {1}", stream.GetType().FullName, ex);
                return null;
            }
        }

        private string InternalImportItem(UploadItemParameter[] uploadItems)
        {
            string newItemId = string.Empty;
            for (int i = 0; i < 5; i++)
            {
                //reset
                //Array.ForEach(uploadItems, itemArg => itemArg.DataStream.Seek(0, SeekOrigin.Begin));
                var items = service.ImportItems(uploadItems).ExecuteAsyncTask();
                var item = items[0];
                if (item.Result == ServiceResult.Success)
                {
                    newItemId = item.ItemId.UniqueId;
                    break;
                }
                else
                {
                    logger.Warn("Cannot import item, error code: {0} ,reason : {1}, Retry times : {2}.", item.ErrorCode, item.ErrorMessage, i);
                    ThrowIfNeverRetryError(item);
                    Thread.Sleep(500 * (i + 1));
                }
            }
            return newItemId;
        }

        private static void ThrowIfNeverRetryError(UploadItemsResponse item)
        {
            if (item.ErrorCode.IsNeverRetryError())
            {
                throw new EWSNeverRetryException(item.ErrorMessage, item.ErrorCode);
            }
        }

        public void CreateItem(byte[] content, string type, string subject, string id)
        {
            try
            {
                Item targetItem = null;
                string tempItemType = FindNearestItemType(type);
                switch (tempItemType.ToUpper(CultureInfo.InvariantCulture))
                {
                    case "IPM.APPOINTMENT":
                        targetItem = new Appointment(service);
                        break;

                    case "IPM.CONTACT":
                        targetItem = new Contact(service);
                        break;

                    case "IPM.TASK":
                        targetItem = new EwsTask(service);
                        break;

                    case "IPM.POST":
                        targetItem = new PostItem(service);
                        break;

                    case "IPM.DOCUMENT":
                    case "IPM.ACTIVITY":
                    case "IPM.STICKYNOTE":
                    case "IPM.NOTE":
                    default:
                        targetItem = new EmailMessage(service);
                        ExtendedPropertyDefinition sentFlag = new ExtendedPropertyDefinition(0x0E07, MapiPropertyType.Integer);
                        targetItem.SetExtendedProperty(sentFlag, this.MessageFlag);
                        break;
                }
                targetItem.Subject = subject;
                targetItem.ItemClass = tempItemType;
                ExtendedPropertyDefinition exchangeIdExtendedProperty = new ExtendedPropertyDefinition(DefaultExtendedPropertySet.PublicStrings, "ExchangeId", MapiPropertyType.String);
                targetItem.SetExtendedProperty(exchangeIdExtendedProperty, id);
                targetItem.MimeContent = new MimeContent();
                targetItem.MimeContent.Content = content;

                targetItem.Save(this.ParentFolderId);
                this.ItemId = targetItem.Id.ToString();
                this.ItemType = targetItem.ItemClass;
                this.ItemName = targetItem.Subject;
            }
            catch (Exception e)
            {
                logger.Error(string.Format("An error occurred while create item, Message: {0}", e.ToString()));
                throw;
            }
        }

        private string FindNearestItemType(string itemType)
        {
            logger.Debug("Item original type: " + itemType);
            List<string> types = new List<string>();
            types.Add("IPM.POST");
            types.Add("IPM.NOTE");
            types.Add("IPM.DOCUMENT");
            types.Add("IPM.APPOINTMENT");
            types.Add("IPM.CONTACT");
            types.Add("IPM.TASK");
            types.Add("IPM.ACTIVITY");
            types.Add("IPM.STICKYNOTE");
            string type = itemType.ToUpper(CultureInfo.InvariantCulture);
            if (type.StartsWith("IPM.Schedule.Meeting", StringComparison.OrdinalIgnoreCase))
                type = "IPM.APPOINTMENT";
            if (types.Contains(type))
            {
                logger.Debug("Item final type: " + type);
                return type;
            }
            else
            {
                if (type.StartsWith("REPORT.", StringComparison.OrdinalIgnoreCase))
                {
                    type = type.Substring(7);
                }
                while (true)
                {
                    int dotIndex = type.LastIndexOf('.');
                    if (dotIndex >= 0)
                    {
                        type = type.Substring(0, dotIndex);
                    }
                    if (type.StartsWith("IPM.InfoPathForm", StringComparison.OrdinalIgnoreCase))
                    {
                        type = "IPM.Document";
                        break;
                    }
                    if (type.Equals("IPM", StringComparison.OrdinalIgnoreCase) || dotIndex < 0)
                    {
                        type = string.Empty;
                        break;
                    }
                    if (types.Contains(type))
                    {
                        break;
                    }
                }
                if (string.IsNullOrEmpty(type))
                    type = "IPM.POST";
                logger.Debug("Item final type: " + type);
                return type;
            }
        }

        private string GetExtension(string name)
        {
            string result = name;
            int index = name.LastIndexOf('.');
            if (index > 0)
            {
                result = name.Substring(index);
            }
            return result;
        }

        private void GenerateItemInfo(Item item)
        {
            try
            {
                this.ItemName = item.Subject;
                this.ItemId = item.Id.ToString();
                this.ItemType = item.ItemClass;
                this.Modified = item.LastModifiedTime;
                this.ItemSize = item.Size;
                this.MessageFlag = item.MessageFlag();
                this.ExchangeId = ExchangeConstants.ConvertItemId(ItemId);
            }
            catch (Exception e)
            {
                logger.Warn("Generate item info, reason: {0}", e.ToString());
            }
        }

        private static bool ValidateRedirectionUrlCallback(String RedirectionUrl)
        {
            return true;
        }
    }
}