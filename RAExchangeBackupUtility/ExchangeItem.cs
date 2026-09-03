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



namespace ExchangeBackupUtility
{
    using AngleSharp.Css;
    using AvePoint.RA.Common;
    using AvePoint.RA.CommonUtil;
    using ExchangeBackupUtility.Graph;
    using ExchangeUtility;
    using Microsoft.Exchange.WebServices.Data;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    using GraphChangeStatus = ExchangeUtility.Graph.ChangeStatus;
    using SystemTask = System.Threading.Tasks.Task;

    public class ExchangeItem : ExchangeObjectBase, IExchangeItem
    {
        private static RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        internal ExchangeService service = null;
        //ExchangeServiceBinding serviceBinding = null;
        internal Item currentItem = null;

        #region Properties

        public string ItemName { get; private set; }
        public string ItemId { get; private set; }
        public string ItemType { get; private set; }
        public string ItemPath { get; set; }
        public string RetentionLabel { get; set; }
        public string SensitivityLabel { get => GetSensitivityLabel(); }

        public bool IsNew => this.currentItem.IsNew;
        public bool IsUnmodified => this.currentItem.IsUnmodified;

        public string DisplayCc => currentItem.DisplayCc;
        public int Importance => (int)currentItem.Importance;

        public IExchangePolicyTag PolicyTag => throw new NotImplementedException();

        public string ItemInternalPath { get; set; }
        public string ParentFolderId { get; private set; }
        public string ParentFolderDisplayName { get; private set; }
        public string ExchangeId { get; private set; }
        public DateTime Modified { get; private set; }
        public DateTime Created { get; private set; }
        public string ModifiedBy { get; private set; }
        public int ItemSize { get; private set; }
        public ChangeStatus ChangeStatus { get; private set; }
        //为了提升效率，并且不能修改Item 对象，防止无法update，所以需要获取到ExchangeItem对象后单独赋值，public set
        public string DisplayTo { get;  set; }
        public List<string> ToRecipients { get; set; }

        public string Sender { get; private set; } = string.Empty;
        public string SenderDisplayName { get; private set; } = string.Empty;
        public string SenderEmailAddress { get; private set; } = string.Empty;
        public string Category { get; private set; }
        public DateTime SendDateUTC { get; private set; }
        public bool HasAttach { get; private set; }

        public int AttachmentCount { get { return Attachments.Count; } }

        //public bool IsDraft { get; private set; }

        public List<string> AttachmentNames { get { return Attachments.Select(a => a.Name).ToList(); } }

        //使用此属性的时候，代码会额外从exchange 服务器发起一次request，所以非必须的时候需要注意
        //PS 如果有其他额外操作，需要对EWS API 的attachment 进行一次封装，变成自己的对象进行操作
        private AttachmentCollection _attachments;
        internal AttachmentCollection Attachments
        {
            get
            {
                if (_attachments == null)
                {
                    if (currentItem != null && currentItem.HasAttachments)
                    {
                        Item itemAttachment = Item.Bind(service, currentItem.Id, new PropertySet(BasePropertySet.IdOnly, ItemSchema.Attachments, ItemSchema.HasAttachments)).GetAwaiter().GetResult();
                        _attachments = itemAttachment.Attachments;
                    }
                    else
                    {
                        _attachments = Activator.CreateInstance(typeof(AttachmentCollection), true) as AttachmentCollection;
                    }
                }
                return _attachments;
            }
            set
            {
                _attachments = value;
            }
        }

        GraphChangeStatus IExchangeItem.ChangeStatus => Enum.Parse<GraphChangeStatus>(this.ChangeStatus.ToString());

        public DateTime Received { get; private set; }

        public int FailedCount { get; set; }

        public bool IsDraft => currentItem?.IsDraft ?? false;

        public bool IsRead { get; set; }

        long IExchangeItem.ItemSize => this.ItemSize;

        public string MessageId { get; set; }

        public int ParentNameEnumerator { get; set; }

        public string SendDateUtcString => SendDateUTC.ToString("yyyy/MM/dd HH:mm:ss");

        int IExchangeItem.AttachmentCount => this.Attachments.Count;

        #endregion


        public bool TryGetProperty(ExtendedPropertyDefinition idDefinition, out string value)
        {
            return currentItem.TryGetProperty(idDefinition, out value);
        }

        internal ExchangeItem(Item item, ChangeType changeType, ExchangeFolder parentFolder)
            : base(parentFolder.AuthObject)
        {
            this.service = item.Service;
            this.currentItem = item;
            GenerateItemInfo(item, changeType, parentFolder);
        }

        //private void InitializeServiceBinding()
        //{
        //    var xAnchorMailbox = string.Empty;
        //    if (service.HttpHeaders.ContainsKey(ExchangeConstants.IMPERSONATION_HEADER_NAME))
        //    {
        //        xAnchorMailbox = service.HttpHeaders[ExchangeConstants.IMPERSONATION_HEADER_NAME];
        //    }
        //    serviceBinding = CreateExchangeServiceBinding(xAnchorMailbox);
        //    serviceBinding.Url = service.Url.ToString();

        //    if (service.ImpersonatedUserId != null)
        //    {
        //        serviceBinding.ExchangeImpersonation = new ExchangeImpersonationType();
        //        serviceBinding.ExchangeImpersonation.ConnectingSID = new ConnectingSIDType() { Item = service.ImpersonatedUserId.Id, ItemElementName = ItemChoiceType.SmtpAddress };
        //    }
        //}
        public byte[] GetItemStreamBytes()
        {
            PropertySet propertySet = new PropertySet(BasePropertySet.IdOnly, ItemSchema.MimeContent);
            Item exoItem = Item.Bind(service, new ItemId(this.ItemId), propertySet).GetAwaiter().GetResult();
            return exoItem.MimeContent.Content;
        }
        private string GetSensitivityLabel()
        {
            string res = "";
            if(currentItem?.ExtendedProperties != null)
            {
                ExtendedProperty labProperty = currentItem.ExtendedProperties.FirstOrDefault(property => property?.PropertyDefinition?.Name == "msip_labels");
                if(labProperty != null)
                {
                    string msipLabelValue = labProperty.Value?.ToString();
                    res = GetSensitivityLabelFromMsipStr(msipLabelValue);
                }
            }
            return res;
        }

        private static string GetSensitivityLabelFromMsipStr(string msipStr)
        {   //msipStr 格式为 MSIP_Label_label的id_Enabled=True;MSIP_Label_1cdafc73-e03e-4ad8-8861-74f2fc66cfc7_SiteId= site的UUID ;MSIP_Label_label的id_SetDate=2024-08-07T02:06:43.292Z;MSIP_Label_label的id_Name=Project - Falcon;MSIP_Label_label的id_ContentBits=0;MSIP_Label_label的id_Method=Privileged;
            string res = "";
            if (!string.IsNullOrWhiteSpace(msipStr))
            {
                string[] strArr = msipStr.Split(';');
                foreach (string str in strArr)
                {
                    int index = str.IndexOf('=');
                    if (index > -1)
                    {
                        string key = str.Substring(0, index);
                        string value = str.Substring(index + 1);
                        if (key.Contains("Name"))
                        {
                            res = value;
                        }
                    }
                    string[] keyValue = str.Split('=');

                }
            }
            return res;
        }

        public void ReInitService(string mailboxAddress)
        {
            Uri serviceUrl = service.Url;
            service = CreateExchangeService();
            service.UseDefaultCredentials = false;
            service.Url = serviceUrl;
            service.ImpersonatedUserId = new ImpersonatedUserId(ConnectingIdType.SmtpAddress, mailboxAddress);
        }

        //private string InternalExportItem(string folderPath)
        //{
        //    ExportItemsType exportItem = new ExportItemsType();
        //    exportItem.ItemIds = new ItemIdType[1];
        //    exportItem.ItemIds[0] = new ItemIdType();
        //    exportItem.ItemIds[0].Id = ItemId;
        //    string filePath = string.Empty;

        //    DateTime startTime = DateTime.Now;
        //    DateTime deadline = startTime.AddMinutes(5);
        //    int retryTimes = 0;

        //    while (true)
        //    {
        //        if (DateTime.Now.Ticks >= deadline.Ticks)
        //        {
        //            break;
        //        }
        //        ExportItemsResponseType exportItemResponse = serviceBinding.ExportItems(exportItem);
        //        ExportItemsResponseMessageType responseMessage = (ExportItemsResponseMessageType)exportItemResponse.ResponseMessages.Items[0];
        //        Byte[] messageBytes = null;
        //        if (responseMessage.ResponseClass == ResponseClassType.Success)
        //        {
        //            messageBytes = responseMessage.Data;
        //            filePath = Path.Combine(folderPath, Guid.NewGuid().ToString() + ".fts");
        //            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
        //            {
        //                fileStream.Write(messageBytes, 0, messageBytes.Length);
        //            }
        //            break;
        //        }
        //        else
        //        {
        //            Thread.Sleep(10000);
        //            logger.Warn("Cannot export item: {0}, reason: {1} TryTimes: {2}.", ItemId, responseMessage.MessageText, retryTimes);
        //            retryTimes++;
        //        }
        //    }
        //    return filePath;
        //}

        ///// <summary>
        /////推荐使用该方法download item，能最大限制保存item的content和metadata
        ///// </summary>
        //public string ExportItem(string folderPath)
        //{
        //    string filePath = string.Empty;
        //    int retryTimes = 0;
        //    InitializeServiceBinding();
        //    while (retryTimes < 6)
        //    {
        //        try
        //        {
        //            filePath = InternalExportItem(folderPath);
        //            break;
        //        }
        //        catch (SoapException ex)
        //        {
        //            logger.Error("Cannot export the item, item id : {1}, reason : {0} Retry times : {2}", ex.ToString(), ItemId, retryTimes);
        //            Thread.Sleep(120000);
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("Cannot export the item, item id : {1}, reason : {0} Retry times : {2}", ex.ToString(), ItemId, retryTimes);
        //            Thread.Sleep(60000);
        //        }
        //        retryTimes++;
        //    }
        //    return filePath;
        //}

        public void UpdateItemIdField(Guid propertySetId, int id, string value)
        {

            int retryTimes = 0;
            var def = new ExtendedPropertyDefinition(propertySetId, id, MapiPropertyType.String);
            while (retryTimes < 5)
            {
                retryTimes++;
                try
                {
                    //var service = CloneExchangeService(this.service, 5);
                    //service.UpdateItem(currentItem, this.ParentFolderId, ConflictResolutionMode.AlwaysOverwrite, MessageDisposition.SaveOnly, SendInvitationsOrCancellationsMode.SendToNone);
                    currentItem.SetExtendedProperty(def, value);
                    if (currentItem is Appointment)
                    {
                        Appointment currentAppointment = currentItem as Appointment;
                        currentAppointment.Update(ConflictResolutionMode.AlwaysOverwrite, SendInvitationsOrCancellationsMode.SendToNone).GetAwaiter().GetResult();
                    }
                    else
                    {
                        currentItem.Update(ConflictResolutionMode.AlwaysOverwrite).GetAwaiter().GetResult();
                    }
                    break;
                }
                catch (Exception ex)
                {

                    logger.Warn($"An error occurred while Set item {currentItem.Subject} ExtendedPropery, item id : {currentItem.Id}. Try times: {retryTimes}, Message : {ex.ToString()}.");
                    if (retryTimes >= 5)
                    {
                        throw ex;
                    }
                    Thread.Sleep(1000);
                }
            }
        }
        public void UpdateIdtemIdField(Guid propertySetId, string name, string value)
        {

            int retryTimes = 0;
            var def = new ExtendedPropertyDefinition(propertySetId, name, MapiPropertyType.String);
            while (retryTimes < 5)
            {
                try
                {
                    currentItem.SetExtendedProperty(def, value);
                    if (currentItem is Appointment)
                    {
                        Appointment currentAppointment = currentItem as Appointment;
                        currentAppointment.Update(ConflictResolutionMode.AlwaysOverwrite, SendInvitationsOrCancellationsMode.SendToNone).GetAwaiter().GetResult();
                    }
                    else
                    {
                        currentItem.Update(ConflictResolutionMode.AlwaysOverwrite).GetAwaiter().GetResult();
                    }
                    break;
                }
                catch (Exception ex)
                {
                    logger.Warn("An error occurred while Set item {0} ExtendedPropery. Try times: {2}, Message : {1}.", currentItem.Id, ex.ToString(), retryTimes);
                    Thread.Sleep(1000);
                }
                retryTimes++;
            }
        }
        public void RemoveItemIdField(Guid propertySetId, int id)
        {

            int retryTimes = 0;
            var def = new ExtendedPropertyDefinition(propertySetId, id, MapiPropertyType.String);
            while (retryTimes < 5)
            {
                try
                {
                    currentItem.RemoveExtendedProperty(def);
                    if (currentItem is Appointment)
                    {
                        Appointment currentAppointment = currentItem as Appointment;
                        currentAppointment.Update(ConflictResolutionMode.AlwaysOverwrite, SendInvitationsOrCancellationsMode.SendToNone).GetAwaiter().GetResult();
                    }
                    else
                    {
                        currentItem.Update(ConflictResolutionMode.AlwaysOverwrite).GetAwaiter().GetResult();
                    }
                    break;
                }
                catch (Exception ex)
                {
                    logger.Warn("An error occurred while Set item {0} ExtendedPropery. Try times: {2}, Message : {1}.", currentItem.Id, ex.ToString(), retryTimes);
                    Thread.Sleep(1000);
                }
                retryTimes++;
            }
        }

        public void RemoveItemIdField(Guid propertySetId, string name)
        {

            int retryTimes = 0;
            var def = new ExtendedPropertyDefinition(propertySetId, name, MapiPropertyType.String);
            while (retryTimes < 5)
            {
                try
                {
                    currentItem.RemoveExtendedProperty(def);
                    if (currentItem is Appointment)
                    {
                        Appointment currentAppointment = currentItem as Appointment;
                        currentAppointment.Update(ConflictResolutionMode.AlwaysOverwrite, SendInvitationsOrCancellationsMode.SendToNone).GetAwaiter().GetResult();
                    }
                    else
                    {
                        currentItem.Update(ConflictResolutionMode.AlwaysOverwrite).GetAwaiter().GetResult();
                    }
                    break;
                }
                catch (Exception ex)
                {
                    logger.Warn("An error occurred while Set item {0} ExtendedPropery. Try times: {2}, Message : {1}.", currentItem.Id, ex.ToString(), retryTimes);
                    Thread.Sleep(1000);
                }
                retryTimes++;
            }
        }

        public void SetExtendProperties(List<Tuple<ExtendedPropertyDefinition, string>> properties)
        {
            foreach(var propertyInfo in properties)
            {
                currentItem.SetExtendedProperty(propertyInfo.Item1, propertyInfo.Item2);
            }
            int retryTimes = 0;
            while (retryTimes < 5)
            {
                try
                {
                    if (currentItem is Appointment)
                    {
                        Appointment currentAppointment = currentItem as Appointment;
                        currentAppointment.Update(ConflictResolutionMode.AlwaysOverwrite, SendInvitationsOrCancellationsMode.SendToNone).GetAwaiter().GetResult();
                    }
                    else
                    {
                        currentItem.Update(ConflictResolutionMode.AlwaysOverwrite).GetAwaiter().GetResult();
                    }
                    break;
                }
                catch (Exception ex)
                {
                    logger.Warn("An error occurred while Set item {0} SetExtendProperties. Try times: {2}, Message : {1}.", currentItem.Id, ex.ToString(), retryTimes);
                    Thread.Sleep(1000);
                }
                retryTimes++;
            }
        }

        public Dictionary<PropertyDefinitionBase, string> LoadExtendProperties(params PropertyDefinitionBase[] definitions)
        {
            var properties = new Dictionary<PropertyDefinitionBase, string>();
            try
            {
                var service = CloneExchangeService(this.service, 5);
                PropertySet set = new PropertySet(BasePropertySet.FirstClassProperties, definitions);
                Item tempItem = Item.Bind(service, this.currentItem.Id, set).GetAwaiter().GetResult();
                foreach (var defination in definitions)
                {
                    string value;
                    tempItem.TryGetProperty(defination, out value);
                    properties[defination] = value;
                }
            }
            catch(Exception ex)
            {
                logger.Error(string.Format("Error in load item properties, reason : {0}", ex.ToString()));
            }
            return properties;
        }

        public bool TryGetExtendProperties(PropertyDefinitionBase defination, out string value)
        {
            return this.currentItem.TryGetProperty(defination, out value);
        }

        //通过removed 和UpdateSuccessful 两个属性，判断是否remove 了propery 并且成功的update。 返回&& 关系，供外围逻辑处理后续操作
        public bool RemoveExtendProperties(List<ExtendedPropertyDefinition> properties)
        {
            bool removed = false;
            var service = CloneExchangeService(this.service, -1);
            PropertySet set = new PropertySet(BasePropertySet.FirstClassProperties, properties);
            Item tempItem = Item.Bind(service, this.currentItem.Id, set).GetAwaiter().GetResult();
            foreach (var propertyInfo in properties)
            {
                var isRemove = tempItem.RemoveExtendedProperty(propertyInfo);
                if (isRemove)
                {
                    removed = true;
                }
            }
            bool updateSuccessFul = false;
            int retryTimes = 0;
            while (retryTimes < 5)
            {
                try
                {
                    if (tempItem is Appointment)
                    {
                        Appointment currentAppointment = tempItem as Appointment;
                        currentAppointment.Update(ConflictResolutionMode.AlwaysOverwrite, SendInvitationsOrCancellationsMode.SendToNone).GetAwaiter().GetResult();
                    }
                    else
                    {
                        tempItem.Update(ConflictResolutionMode.AlwaysOverwrite).GetAwaiter().GetResult();
                    }
                    updateSuccessFul = true;
                    break;
                }
                catch (Exception ex)
                {
                    logger.Warn("An error occurred while Set item {0} SetExtendProperties. Try times: {2}, Message : {1}.", tempItem.Id, ex.ToString(), retryTimes);
                    Thread.Sleep(1000);
                }
                retryTimes++;
            }
            return removed && updateSuccessFul;
        }

        #region Assembly Exchange Properties
        private void GenerateItemInfo(Item item, ChangeType changeType, ExchangeFolder parentFolder)
        {
            try
            {
                //this.IsDraft = item.IsDraft;
                this.ParentFolderId = item.ParentFolderId.ToString();
                this.ChangeStatus = changeType.ToChangeStatus();
                this.ItemName = GetItemName(item);
                this.ItemId = item.Id.ToString();
                this.ItemType = item.ItemClass;
                this.Modified = item.LastModifiedTime.ToUniversalTime();
                this.Created = item.DateTimeCreated.ToUniversalTime();
                this.ModifiedBy = item.LastModifiedName;
                this.ItemSize = item.Size;
                this.DisplayTo = GetToRecipients(item);
                this.HasAttach = item.HasAttachments;
                this.SendDateUTC = item.DateTimeSent.ToUniversalTime();
                this.Category = string.Join(";", item.Categories.ToArray());
                var retentionId = item.PolicyTag!= null ? item.PolicyTag.RetentionId : Guid.Empty; ;
                this.RetentionLabel = parentFolder.LabelIdNameMapping.ContainsKey(retentionId) ? parentFolder.LabelIdNameMapping[retentionId] : string.Empty;
                GetSenderValue(item);
                this.ExchangeId = ExchangeConstants.ConvertItemId(this.ItemId);
                //此处逻辑如果item 有att，并且att 的count>0, 表示当前Item对象加载过att， 我们就给Attachments 对象赋值，否则需要在使用attachments 的时候去加载
                if (item.HasAttachments && item.Attachments.Count > 0)
                {
                    this.Attachments = item.Attachments;
                }
            }
            catch (Exception e)
            {
                logger.Warn("Generate item info, reason: {0}", e.ToString());
            }
        }
        private string GetToRecipients(Item item)
        {
            var message = item as EmailMessage;
            if (message != null && message.ToRecipients != null && message.ToRecipients.Count > 0)
            {
                return string.Join("; ", message.ToRecipients.Select(address => address.ToFormatString()));
            }
            return item.DisplayTo ?? string.Empty;
        }

        private static string GetItemName(Item item)
        {
            var itemName = item.Subject;
            if (string.IsNullOrEmpty(itemName))     //SAAS-10111
            {
                var contact = item as Contact;
                if (contact != null)
                {
                    Microsoft.Exchange.WebServices.Data.EmailAddress address;
                    if (contact.EmailAddresses.TryGetValue(EmailAddressKey.EmailAddress1, out address))
                    {
                        itemName = address.Address;
                    }
                }
            }
            return itemName;
        }

        private void GetSenderValue(Item item)
        {
            var message = item as EmailMessage;
            if (message != null && message.Sender != null)
            {
                this.Sender = message.Sender.ToFormatString();
                this.SenderDisplayName = message.Sender.Name;
                this.SenderEmailAddress = message.Sender.Address;
                //return message.Sender.ToFormatString();
            }
            //return string.Empty;
        }

        #endregion

        #region Get Properties for filter
        public Dictionary<string, string> GetProperties()
        {
            switch (this.ItemType)
            {
                case "IPM.Note":
                    return GetMessageProperties();
                case "IPM.Task":
                    return GetTaskProperties();
                case "IPM.Post":
                    return GetPostProperties();
                case "IPM.Appointment":
                    return GetEventProperties();
                case "IPM.Activity":
                    return GetJournalProperties();
                case "IPM.StickyNote":
                    return GetNoteProperties();
                case "IPM.Contact":
                    return GetContactProperties();
                case "IPM.Document":
                    return GetDocumentProperties();
                case "IPM.DistList":
                    return GetDistListProperties();
                default:
                    if (ItemType.StartsWith("IPM.Document"))
                        return GetDocumentProperties();
                    return new Dictionary<string, string>();
            }
        }

        private Dictionary<string, string> GetMessageProperties()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            EmailMessage message = this.currentItem as EmailMessage;
            result.Add("Subject", message.Subject);
            result.Add("Received", message.DateTimeReceived == DateTime.MinValue?"": message.DateTimeReceived.ToUniversalTime().ToString());
            result.Add("From", message.From == null ? string.Empty : message.From.Address);
            result.Add("To", message.DisplayTo);
            result.Add("Size", message.Size.ToString());
            //result.Add("Categories", message.Categories.ToString());
            //result.Add("Mention", message.Categories.ToString());
            result.Add("Conversation", message.ConversationTopic?.ToString());
            result.Add("Created", message.DateTimeCreated == DateTime.MinValue ? "" : message.DateTimeCreated.ToUniversalTime().ToString());
            result.Add("Due Date", message.Flag?.DueDate == DateTime.MinValue ? "" : message.Flag?.DueDate.ToUniversalTime().ToString());
            result.Add("Flag Completed Date", message.Flag?.CompleteDate == DateTime.MinValue ? "" : message.Flag?.CompleteDate.ToUniversalTime().ToString());
            result.Add("Flag Status", message.Flag?.FlagStatus.ToString());
            result.Add("Importance", message.Importance.ToString());
            result.Add("Received Representing Name", message.ReceivedRepresenting?.Name.ToString());
            result.Add("Recipient Name", message.ReceivedRepresenting?.Name.ToString());
            result.Add("Sensitivity", message.Sensitivity.ToString());
            result.Add("Sent", message.DateTimeSent == DateTime.MinValue ? "" : message.DateTimeSent.ToUniversalTime().ToString());
            result.Add("Start Date", message.Flag?.StartDate == DateTime.MinValue ? "" : message.Flag?.StartDate.ToUniversalTime().ToString());
            result.Add("Cc", message.DisplayCc);
            result.Add("Email Account", message.ReceivedRepresenting?.Address.ToString());
            try
            {
                foreach (var prop in message.ExtendedProperties)
                {
                    try
                    {
                        string propName = prop.PropertyDefinition.Name;
                        if (!string.IsNullOrEmpty(propName))
                        {
                            result.Add(propName, prop.Value.ToString());
                        }
                        else
                        {
                            logger.Warn($"the extended prop has no prop name,value:{prop.Value.ToString()}");
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error($"get extended prop failed,prop:{prop.PropertyDefinition?.Name},error:{e}");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error ($"get extended prop failed,error:{e.ToString()}");
            }
            return result;
        }

        private Dictionary<string, string> GetTaskProperties()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            Task task = this.currentItem as Task;
            result.Add("Subject", task.Subject);
            result.Add("IPM.Task.StartDate", task.StartDate.HasValue ? task.StartDate.Value.ToUniversalTime().ToString() : string.Empty);
            result.Add("IPM.Task.DueDate", task.DueDate.HasValue ? task.DueDate.Value.ToUniversalTime().ToString() : string.Empty);
            result.Add("Status", task.Status.ToString());
            result.Add("Priority", task.Importance.ToString());
            result.Add("Size", task.Size.ToString());
            result.Add("CreatedBy", task.Owner);
            return result;
        }

        private Dictionary<string, string> GetPostProperties()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            PostItem postItem = this.currentItem as PostItem;
            result.Add("Conversation", postItem.ConversationTopic);
            result.Add("PostedOn", postItem.PostedTime.ToUniversalTime().ToString());
            result.Add("PostedTo", GetParentFolderName(ParentFolderId));
            result.Add("Size", postItem.Size.ToString());
            return result;
        }

        private Dictionary<string, string> GetEventProperties()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            Appointment appointment = this.currentItem as Appointment;
            result.Add("Subject", appointment.Subject);
            result.Add("IPM.Appointment.EventDate", appointment.Start.ToUniversalTime().ToString());
            result.Add("IPM.Appointment.End", appointment.End.ToUniversalTime().ToString());
            return result;
        }

        private Dictionary<string, string> GetJournalProperties()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("Subject", this.currentItem.Subject);
            string entry;
            DateTime start;
            Guid guid = new Guid("0006200A-0000-0000-C000-000000000046");
            ExtendedPropertyDefinition entryTypeDefinition = new ExtendedPropertyDefinition(guid, 0x8700, MapiPropertyType.String);
            ExtendedPropertyDefinition startDefinition = new ExtendedPropertyDefinition(guid, 0x8706, MapiPropertyType.SystemTime);
            PropertySet set = new PropertySet(BasePropertySet.FirstClassProperties, entryTypeDefinition, startDefinition);
            Item tempItem = Item.Bind(this.service, this.currentItem.Id, set).GetAwaiter().GetResult();
            tempItem.TryGetProperty(entryTypeDefinition, out entry);
            tempItem.TryGetProperty(startDefinition, out start);
            result.Add("EntryType", entry.Replace(" ", ""));
            result.Add("IPM.Activity.Start", start.ToUniversalTime().ToString());
            return result;
        }

        private Dictionary<string, string> GetNoteProperties()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("Name", this.currentItem.Subject);
            result.Add("Created", this.currentItem.DateTimeCreated.ToUniversalTime().ToString());
            result.Add("Modified", this.currentItem.LastModifiedTime.ToUniversalTime().ToString());
            string createdBy = string.Empty;
            Guid guid = new Guid("00062008-0000-0000-C000-000000000046");
            ExtendedPropertyDefinition createdByDefinition = new ExtendedPropertyDefinition(guid, 0x8580, MapiPropertyType.String);
            PropertySet set = new PropertySet(BasePropertySet.FirstClassProperties, createdByDefinition);
            Item tempItem = Item.Bind(this.service, this.currentItem.Id, set).GetAwaiter().GetResult();
            tempItem.TryGetProperty(createdByDefinition, out createdBy);
            if (string.IsNullOrEmpty(createdBy))
                createdBy = this.currentItem.LastModifiedName;
            result.Add("CreatedBy", createdBy);
            result.Add("ModifiedBy", this.currentItem.LastModifiedName);
            return result;
        }

        private Dictionary<string, string> GetContactProperties()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            Contact contact = this.currentItem as Contact;
            result.Add("FullName", contact.DisplayName);
            result.Add("LastName", contact.Surname);
            result.Add("FirstName", contact.GivenName);
            result.Add("Modified", contact.LastModifiedTime.ToUniversalTime().ToString());
            return result;
        }

        private Dictionary<string, string> GetDocumentProperties()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("Name", this.currentItem.Subject);
            result.Add("Created", this.currentItem.DateTimeCreated.ToUniversalTime().ToString());
            result.Add("Size", this.currentItem.Size.ToString());
            return result;
        }

        private Dictionary<string, string> GetDistListProperties()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            return result;
        }

        private string GetParentFolderName(FolderId folderId)
        {
            Folder folder = Folder.Bind(this.service, folderId).GetAwaiter().GetResult();
            return folder.DisplayName;
        }
        #endregion

        public override bool Equals(object obj)
        {
            var other = obj as ExchangeItem;
            if (other == null) return false;
            return string.Equals(this.ItemId, other.ItemId);
        }

        public override int GetHashCode()
        {
            return this.ItemId?.GetHashCode() ?? 0;
        }

        public bool TryGetExtendProperty(ExtendProperty property, out string value)
        {
            var def = property switch
            {
                ExtendProperty.Term => new ExtendedPropertyDefinition(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, MapiPropertyType.String)
            };
            return currentItem.TryGetProperty(def, out value);
        }

        public void SetExtendProperty(string definition, string value)
        {
            throw new NotImplementedException();
        }

        #region Label
        public void TagLabel(Guid labelId)
        {
            try
            {
                currentItem.PolicyTag = new PolicyTag();
                currentItem.PolicyTag.RetentionId = labelId;
                currentItem.PolicyTag.IsExplicit = true;
                //currentItem.Update(ConflictResolutionMode.AlwaysOverwrite);
            }
            catch (Exception ex)
            {
                logger.Error("Tag label failed.Exception:" + ex.ToString());
                throw;
            }
        }

        public void RemoveLabel()
        {
            try
            {
                currentItem.PolicyTag = null;
                //currentItem.Update(ConflictResolutionMode.AlwaysOverwrite);
            }
            catch (Exception ex)
            {
                logger.Error("Remove label failed. Exception: " + ex.ToString());
                throw;
            }
        }

        public bool CanUpdateLabel(List<Guid> labelIds)
        {
            var result = false;
            try
            {
                if (currentItem.PolicyTag == null || labelIds.Contains(currentItem.PolicyTag.RetentionId))
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Get label failed. Exception: " + ex.ToString());
                throw;
            }
            return result;
        }

        public bool IsLabelExist()
        {
            var result = false;
            try
            {
                result = currentItem.PolicyTag != null;
            }
            catch (Exception ex)
            {
                logger.Error("Get label failed. Exception: " + ex.ToString());
                throw;
            }
            return result;
        }
        public Guid ApplyedLabelId()
        {
            try
            {
                if (currentItem.PolicyTag != null) 
                {
                    return currentItem.PolicyTag.RetentionId;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Get label Id failed. Exception: " + ex.ToString());
                throw;
            }
            return Guid.Empty;
        }
        #endregion

        public async System.Threading.Tasks.Task<bool> DeleteAsync(bool isHardDelete = false)
        {
            try
            {
                await this.currentItem.Delete(isHardDelete ? DeleteMode.HardDelete : DeleteMode.MoveToDeletedItems);
            }
            catch (Exception ex)
            {
                logger.Error("Delete item failed. Exception: " + ex.ToString());
                return false;
            }
            return true;
        }

        public async System.Threading.Tasks.Task<bool> MoveAsync(string targetFolderId)
        {
            try
            {
                FolderId folderId = new FolderId(targetFolderId);
                await this.currentItem.Move(folderId);
            }
            catch (Exception ex)
            {
                logger.Error("Move item failed. Exception: " + ex.ToString());
                return false;
            }
            return true;
        }

        public async System.Threading.Tasks.Task<bool> SetExtendedPropertyAsync(ExtendedPropertyDefinition prop, object value)
        {
            try
            {
                this.currentItem.SetExtendedProperty(prop, value);
                await this.currentItem.Update(ConflictResolutionMode.AlwaysOverwrite);
            }
            catch (Exception ex)
            {
                logger.Error($"Set extended property failed for item {this.currentItem.Id}. Exception: {ex.ToString()}");
                return false;
            }
            return true;
        }

        public async System.Threading.Tasks.Task<Stream> GetMimeContentAsync()
        {
            try
            {
                await this.currentItem.Load(new PropertySet(ItemSchema.MimeContent));
                return new MemoryStream(this.currentItem.MimeContent.Content);
            }
            catch (Exception ex)
            {
                logger.Error($"Get mime content failed for item {this.currentItem.Id}. Exception: {ex.ToString()}");
                return Stream.Null;
            }
        }

        public async System.Threading.Tasks.Task<bool> SetRetentionLabelAsync(Guid labelId)
        {
            using (var performance0 = new PerformanceScope("ExchangeTagController.TagLabel", "", true))
            {
                try
                {
                    this.currentItem.PolicyTag = new PolicyTag();
                    this.currentItem.PolicyTag.RetentionId = labelId;
                    this.currentItem.PolicyTag.IsExplicit = true;
                    await this.currentItem.Update(ConflictResolutionMode.AlwaysOverwrite);
                }
                catch (Exception ex)
                {
                    logger.Error("Tag label failed.Exception:" + ex.ToString());
                    return false;
                }
            }
            return true;
        }

        public void RemovePolicyTag()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string?> GetExtendedProperties()
        {
            throw new NotImplementedException();
        }
    }

}
