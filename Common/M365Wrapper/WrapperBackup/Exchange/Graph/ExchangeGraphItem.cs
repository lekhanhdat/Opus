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
#nullable enable
using AvePoint.GCommon.Contract.ReportCenter.Object;
using AvePoint.RA.Common;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using ExchangeUtility.Graph;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Graph.Beta.Models;
using Microsoft365.Graph.Extensions;
using Microsoft365.Graph.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GraphV1Message = Microsoft.Graph.Models.Message;
using GraphV1SingleValueLegacyExtendedProperty = Microsoft.Graph.Models.SingleValueLegacyExtendedProperty;
using SystemTask = System.Threading.Tasks.Task;

namespace ExchangeBackupUtility.Graph;

public class ExchangeGraphItem : BaseExchangeItem, IExchangeItem
{
    public GraphService Service { get; }
    public string MailboxId { get; }

    private readonly MailboxItem item;
    private readonly MailboxFolder folder;

    public ExchangeGraphItem(GraphService service, string mailboxId, MailboxItem item, MailboxFolder folder)
    {
        this.Service = service;
        this.MailboxId = mailboxId;
        this.item = item;
        this.folder = folder;
    }
    public ExchangeGraphItem(GraphService service, string mailboxId, string itemId, string folderId)
    {
        this.Service = service;
        this.MailboxId = mailboxId;
        this.item = GetItemAsync(mailboxId, folderId.ToRestId(), itemId.ToRestId()).ExecuteAsyncTask();
        this.folder = GetFolderAsync(mailboxId, folderId.ToRestId()).ExecuteAsyncTask();
    }

    public string Category => item.Categories.IsNotNullOrEmpty() ? string.Join(",", item.Categories) : String.Empty;

    public ChangeStatus ChangeStatus => item.IsDeleted() ? ChangeStatus.Delete : ChangeStatus.Create;

    public DateTime Created => item.CreatedDateTime?.UtcDateTime ?? DateTime.MinValue;
    public DateTime Received => item.Received() ?? DateTime.MinValue;

    public string DisplayTo => Recipients.Value;

    public string ExchangeId => ExchangeConstants.ConvertItemId(this.ItemId.ToRestId());

    public int FailedCount { get; set; }

    public bool HasAttach => item.HasAttachments();

    public bool IsDraft
    {
        get
        {
            var msgFlag = item.MsgFlag();
            return msgFlag.HasValue && (msgFlag.Value & (int)MsgFlag.MSGFLAG_UNSENT) == (int)MsgFlag.MSGFLAG_UNSENT;
        }
    }

    public bool IsRead { get => item.IsRead(); }

    public string ItemId => item.Id.EnsureIfNotNullOrEmpty().ToEwsId();

    public string ItemInternalPath { get; set; } = null!;

    public string ItemName => item.Subject() ?? String.Empty;

    public string ItemPath { get; set; } = null!;

    public long ItemSize => item.Size ?? 0;

    public string ItemType => item.Type ?? string.Empty;

    public string MessageId { get => item.InternetMessageId(); }

    public DateTime Modified => item.LastModifiedDateTime?.UtcDateTime ?? DateTime.MinValue;

    public string ParentFolderId => folder.Id.ToEwsId().EnsureIfNotNullOrEmpty();

    public string ParentFolderDisplayName => folder.DisplayName.EnsureIfNotNullOrEmpty();

    public int ParentNameEnumerator { get => (int?)folder.WellKnownFolderNameEnum() ?? -1; }

    public DateTime SendDateUTC => item.ClientSubmitTime()?.ToUniversalTime() ?? DateTime.MinValue;

    public string SendDateUtcString => SendDateUTC.ToString("yyyy/MM/dd HH:mm:ss");

    public string Sender => EmailAddressExtension.ToFormatString(item.SenderName(), item.SenderEmail());
    public string ModifiedBy => item.ModifiedBy();
    public string SenderDisplayName => item.SenderName();
    public string SenderEmailAddress => item.SenderEmail();
    public int AttachmentCount => this.HasAttach ? this.Attachments.Value.Count() : 0;
    public string RetentionLabel => item.RetentionLabel() ?? string.Empty;

    public string MailBoxObjectId { get; set; }

    public string DisplayCc => item.DisplayCc() ?? string.Empty;
    public int Importance => item.ImportanceLevel() ?? 0;

    public string ConversationTopic => item.ConversationTopic() ?? string.Empty;

    public string ReceivedRepresentingName => item.ReceivedRepresentingName() ?? string.Empty;
    public string ReceivedRepresentingSmtpAddress => item.ReceivedRepresentingSmtpAddress() ?? string.Empty;

    public string Sensitivity => item.SensitivityLevel() ?? string.Empty;

    public List<string> AttachmentNames => this.HasAttach ? this.Attachments.Value.Select(a => a.Name).ToList() : [];
    public Lazy<IEnumerable<IExchangeAttachment>> Attachments
    {
        get
        {
            return this.HasAttach ? GetAttachments() : new Lazy<IEnumerable<IExchangeAttachment>>(() => []);
        }
    }

    public Lazy<string> Recipients
    {
        get
        {
            return GetToRecipients();
        }
    }

    public bool TryGetExtendProperty(ExtendProperty extendProperty, out string value)
    {
        var definition = extendProperty switch
        {
            ExtendProperty.SensitiveLabel => OutlookExtendedProperties.PidNameMSIPLabels.GetShortString(),
            ExtendProperty.Term => TermColumnInfo.WellKnowTermColumnGuid.ToString()
        };
        if (this.item.SingleValueExtendedProperties.IsNullOrEmpty())
        {
            value = string.Empty;
            return false;
        }
        value = this.item.SingleValueExtendedProperties.FirstOrDefault(p => p.Id.Contains(definition))?.Value ?? string.Empty;
        return !string.IsNullOrEmpty(value);
    }

    public string SensitivityLabel => TryGetSensitiveLabelProperty(out var value) ? value : string.Empty;
    
    public bool IsNew => string.IsNullOrEmpty(this.ItemId);
    public bool IsUnmodified => this.item.LastModifiedDateTime == this.item.CreatedDateTime;

    public IExchangePolicyTag PolicyTag => new ExchangeGraphPolicyTag(this.item);

    public void RemovePolicyTag()
    {
        if (this.item.SingleValueExtendedProperties.IsNullOrEmpty())
        {
            Logger.Info($"Item {ItemId.ToRestId()} has no extended properties, skip removing policy tag.");
            return;
        }
        if (this.PolicyTag.RetentionId != null)
        {
            Logger.Info($"Removing policy tag from item {ItemId.ToRestId()}.");
            // Current support remove retention label for message only
            var tempMessage = new GraphV1Message();
            tempMessage.SingleValueExtendedProperties = new List<GraphV1SingleValueLegacyExtendedProperty>
            {
                new GraphV1SingleValueLegacyExtendedProperty
                {
                    Id = "Binary 0x3019",
                    Value = string.Empty
                }
            };
            Service.Mails.UpdateMessageAsync(MailboxId, ParentFolderId.ToRestId(), ItemId.ToRestId(), tempMessage).ExecuteAsyncTask();
        }
    }

    public void TagLabel(Guid labelId)
    {
        // Check if item already has retention label
        var retentionIdProp = this.item.SingleValueExtendedProperties?.FirstOrDefault(p => p.Id == "Binary 0x3019");
        if (retentionIdProp is null)
        {
            this.AddOrInitSingleValueExtendedProperties("Integer 0x301a", "0"); // Retention period
        }
        this.AddOrInitSingleValueExtendedProperties("Binary 0x3019", labelId.ConvertFromGuidToBase64Id());
    }

    public void RemoveLabel()
    {
        // Check if item already has retention label
        var retentionIdProp = this.item.SingleValueExtendedProperties?.FirstOrDefault(p => p.Id == "Binary 0x3019");
        if (retentionIdProp is not null)
        {
            this.AddOrInitSingleValueExtendedProperties("Binary 0x3019", null);
        }
    }

    public bool CanUpdateLabel(List<Guid> labelIds)
    {
        return item.RetentionLabel == null || labelIds.Contains(item.RetentionLabelId().ConvertFromBase64ToGuidId());
    }

    public bool IsLabelExist()
    {
        return !string.IsNullOrEmpty(item.RetentionLabelId());
    }

    public Guid ApplyedLabelId()
    {
        return item.RetentionLabelId().ConvertFromBase64ToGuidId();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not ExchangeGraphItem other) return false;
        return string.Equals(this.ItemId.ToRestId(), other.ItemId.ToRestId());
    }

    public override int GetHashCode()
    {
        return this.ItemId?.ToRestId().GetHashCode() ?? 0;
    }

    public Dictionary<string, string?> GetExtendedProperties()
    {
        if (this.item.SingleValueExtendedProperties == null)
        {
            return new Dictionary<string, string?>();
        }

        var result = new Dictionary<string, string?>();
        foreach (var property in this.item.SingleValueExtendedProperties)
        {
            if (string.IsNullOrEmpty(property?.Id))
            {
                continue;
            }

            result[property.Id] = property.Value;
        }
        return result;
    }

    public Dictionary<string, string> GetProperties()
    {
        switch (this.ItemType)
        {
            case ExchangeConstants.ItemType.Message:
                return GetMessageProperties();
            // case "IPM.Task":
            //     return GetTaskProperties();
            // case "IPM.Post":
            //     return GetPostProperties();
            // case "IPM.Appointment":
            //     return GetEventProperties();
            // case "IPM.Activity":
            //     return GetJournalProperties();
            // case "IPM.StickyNote":
            //     return GetNoteProperties();
            // case "IPM.Contact":
            //     return GetContactProperties();
            // case "IPM.Document":
            //     return GetDocumentProperties();
            // case "IPM.DistList":
            //     return GetDistListProperties();
            default:
                if (ItemType.StartsWith(ExchangeConstants.ItemType.Document))
                    return GetDocumentProperties();
                return new Dictionary<string, string>();
        }
    }

    private Dictionary<string, string> GetMessageProperties()
    {
        Dictionary<string, string> result = [];
        var message = Service.Users.GetMessageByIdAsync(MailboxId, ItemId.ToRestId()).Result;

        result["Subject"] = message.Subject ?? string.Empty;
        result["Received"] = FormatDateTime(message.ReceivedDateTime);
        result["From"] = message.From?.EmailAddress?.Address ?? string.Empty;
        result["To"] = JoinRecipients(message.ToRecipients);
        result["Size"] = ItemSize.ToString();
        result["Conversation"] = this.ConversationTopic;
        result["Created"] = FormatDateTime(message.CreatedDateTime);
        result["Due Date"] = FormatDateTime(message.Flag?.DueDateTime);
        result["Flag Completed Date"] = FormatDateTime(message.Flag?.CompletedDateTime);
        result["Flag Status"] = message.Flag?.FlagStatus?.ToString() ?? string.Empty;
        result["Importance"] = message.Importance?.ToString() ?? string.Empty;
        result["Received Representing Name"] = this.ReceivedRepresentingName;
        result["Recipient Name"] = this.ReceivedRepresentingName;
        result["Sensitivity"] = this.Sensitivity;
        result["Sent"] = FormatDateTime(message.SentDateTime);
        result["Start Date"] = FormatDateTime(message.Flag?.StartDateTime);
        result["Cc"] = JoinRecipients(message.CcRecipients);
        result["Email Account"] = this.ReceivedRepresentingSmtpAddress;
        result["Type"] = this.ItemType;

        AppendExtendedProperties(message, result);
        return result;
    }

    private static string JoinRecipients(IEnumerable<Recipient> recipients)
    {
        return recipients == null
            ? string.Empty
            : string.Join("; ",
                recipients.Select(r => r.EmailAddress?.Address).Where(address => !string.IsNullOrWhiteSpace(address)));
    }

    private static string FormatDateTime(DateTimeOffset? value)
    {
        return value.HasValue ? value.Value.UtcDateTime.ToString("o") : string.Empty;
    }

    private static string FormatDateTime(DateTimeTimeZone value)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.DateTime))
        {
            return string.Empty;
        }

        return DateTimeOffset.TryParse(value.DateTime, out var parsed)
            ? parsed.ToUniversalTime().ToString("o")
            : string.Empty;
    }

    private void AppendExtendedProperties(Message message, IDictionary<string, string> result)
    {
        if (message.SingleValueExtendedProperties == null)
        {
            return;
        }

        foreach (var property in message.SingleValueExtendedProperties)
        {
            if (string.IsNullOrWhiteSpace(property?.Id) || string.IsNullOrWhiteSpace(property.Value))
            {
                continue;
            }

            result[property.Id] = property.Value;
        }
    }
       
       //  private Dictionary<string, string> GetTaskProperties()
       //  {
       //      Dictionary<string, string> result = new Dictionary<string, string>();
       //      Task task = this.currentItem as Task;
       //      result.Add("Subject", task.Subject);
       //      result.Add("IPM.Task.StartDate", task.StartDate.HasValue ? task.StartDate.Value.ToUniversalTime().ToString() : string.Empty);
       //      result.Add("IPM.Task.DueDate", task.DueDate.HasValue ? task.DueDate.Value.ToUniversalTime().ToString() : string.Empty);
       //      result.Add("Status", task.Status.ToString());
       //      result.Add("Priority", task.Importance.ToString());
       //      result.Add("Size", task.Size.ToString());
       //      result.Add("CreatedBy", task.Owner);
       //      return result;
       //  }
       //
       //  private Dictionary<string, string> GetPostProperties()
       //  {
       //      Dictionary<string, string> result = new Dictionary<string, string>();
       //      PostItem postItem = this.currentItem as PostItem;
       //      result.Add("Conversation", postItem.ConversationTopic);
       //      result.Add("PostedOn", postItem.PostedTime.ToUniversalTime().ToString());
       //      result.Add("PostedTo", GetParentFolderName(ParentFolderId));
       //      result.Add("Size", postItem.Size.ToString());
       //      return result;
       //  }
       //
       //  private Dictionary<string, string> GetEventProperties()
       //  {
       //      Dictionary<string, string> result = new Dictionary<string, string>();
       //      Appointment appointment = this.currentItem as Appointment;
       //      result.Add("Subject", appointment.Subject);
       //      result.Add("IPM.Appointment.EventDate", appointment.Start.ToUniversalTime().ToString());
       //      result.Add("IPM.Appointment.End", appointment.End.ToUniversalTime().ToString());
       //      return result;
       //  }
       //
       //  private Dictionary<string, string> GetJournalProperties()
       //  {
       //      Dictionary<string, string> result = new Dictionary<string, string>();
       //      result.Add("Subject", this.currentItem.Subject);
       //      string entry;
       //      DateTime start;
       //      Guid guid = new Guid("0006200A-0000-0000-C000-000000000046");
       //      ExtendedPropertyDefinition entryTypeDefinition = new ExtendedPropertyDefinition(guid, 0x8700, MapiPropertyType.String);
       //      ExtendedPropertyDefinition startDefinition = new ExtendedPropertyDefinition(guid, 0x8706, MapiPropertyType.SystemTime);
       //      PropertySet set = new PropertySet(BasePropertySet.FirstClassProperties, entryTypeDefinition, startDefinition);
       //      Item tempItem = Item.Bind(this.service, this.currentItem.Id, set).GetAwaiter().GetResult();
       //      tempItem.TryGetProperty(entryTypeDefinition, out entry);
       //      tempItem.TryGetProperty(startDefinition, out start);
       //      result.Add("EntryType", entry.Replace(" ", ""));
       //      result.Add("IPM.Activity.Start", start.ToUniversalTime().ToString());
       //      return result;
       //  }
       //
       private Dictionary<string, string> GetNoteProperties()
       {
           Dictionary<string, string> result = new Dictionary<string, string>();
           result.Add("Name", ItemName);
           result.Add("Created", Created == DateTime.MinValue ? "" : Created.ToUniversalTime().ToString());
           result.Add("Modified", Modified == DateTime.MinValue? "" : Modified.ToUniversalTime().ToString());
           // if (string.IsNullOrEmpty(createdBy))
           //     createdBy = this.currentItem.LastModifiedName;
           // result.Add("CreatedBy", ModifiedBy);
           result.Add("ModifiedBy", ModifiedBy);
           return result;
       }
       
       //  private Dictionary<string, string> GetContactProperties()
       //  {
       //      Dictionary<string, string> result = new Dictionary<string, string>();
       //      Contact contact = this.currentItem as Contact;
       //      result.Add("FullName", contact.DisplayName);
       //      result.Add("LastName", contact.Surname);
       //      result.Add("FirstName", contact.GivenName);
       //      result.Add("Modified", contact.LastModifiedTime.ToUniversalTime().ToString());
       //      return result;
       //  }
       //
       private Dictionary<string, string> GetDocumentProperties()
       {
           Dictionary<string, string> result = new Dictionary<string, string>();
           result.Add("Name", ItemName);
           result.Add("Created", Created == DateTime.MinValue ? "" : Created.ToUniversalTime().ToString());
           result.Add("Size", ItemSize.ToString());
           return result;
       }
       
       //  private Dictionary<string, string> GetDistListProperties()
       //  {
       //      Dictionary<string, string> result = new Dictionary<string, string>();
       //      return result;
       //  }
       //
       //  private string GetParentFolderName(FolderId folderId)
       //  {
       //      Folder folder = Folder.Bind(this.service, folderId).GetAwaiter().GetResult();
       //      return folder.DisplayName;
       //  }

    public bool TryGetSensitiveLabelProperty(out string value)
    {
        if (this.item.SingleValueExtendedProperties.IsNullOrEmpty())
        {
            value = string.Empty;
            return false;
        }
        var sensitiveLabel = this.item.SingleValueExtendedProperties.FirstOrDefault(p => p.Id.Contains("msip_labels"))?.Value ?? string.Empty;
        value = GetSensitivityLabelFromString(sensitiveLabel);
        return !string.IsNullOrEmpty(value);
    }

    public void SetExtendProperty(string definition, string value)
    {
        if (this.item.SingleValueExtendedProperties.IsNullOrEmpty())
        {
            this.item.SingleValueExtendedProperties = [];
        }

        this.item.SingleValueExtendedProperties.Add(new SingleValueLegacyExtendedProperty
        {
            Id = definition,
            Value = value
        });
    }

    private Lazy<IEnumerable<IExchangeAttachment>> GetAttachments()
    {
        var attachments = Service.Mails.GetMessageAttachmentsAsync(MailboxId, ParentFolderId.ToRestId(), ItemId.ToRestId()).ExecuteAsyncTask();

        var exchangeAttachments = attachments?.Value?.Select(a => new ExchangeGraphFileAttachment
        {
            ContentType = a.ContentType,
            Id = a.Id,
            IsInline = a.IsInline ?? false,
            LastModifiedDateTime = a.LastModifiedDateTime ?? DateTimeOffset.MinValue,
            Name = a.Name,
            Size = a.Size ?? 0
        } as IExchangeAttachment).ToList();

        return new Lazy<IEnumerable<IExchangeAttachment>>(() => exchangeAttachments ?? []);
    }
    private Lazy<string> GetToRecipients()
    {
        return new Lazy<string>(() =>
        {
            var message = Service.Mails
                .GetToRecipientsMessageByIdAsync(MailboxId, ItemId.ToRestId())
                .ExecuteAsyncTask();

            if (message?.ToRecipients == null || message.ToRecipients.Count == 0)
                return string.Empty;

            return string.Join("; ",
                message.ToRecipients
                    .Where(r => r.EmailAddress?.Address != null)
                    .Select(r => ExchangeGraphUtil.ToFormatString(r.EmailAddress)));
        });
    }

    public async System.Threading.Tasks.Task<bool> DeleteAsync(bool isHardDelete = false)
    {
        try
        {
            Service.Mails.DeleteMessageAsync(MailboxId, ParentFolderId.ToRestId(), ItemId.ToRestId(), isHardDelete).ExecuteAsyncTask();
        }
        catch (Exception ex)
        {
            Logger.Error($"Delete item {ItemId.ToRestId()} in folder {ParentFolderId.ToRestId()} failed.", ex);
            throw;
        }
        return true;
    }

    public async System.Threading.Tasks.Task<bool> MoveAsync(string targetFolderId)
    {
        try
        {
            Service.Mails.MoveMessageAsync(MailboxId, ParentFolderId.ToRestId(), ItemId.ToRestId(), targetFolderId).ExecuteAsyncTask();
        }
        catch (Exception ex)
        {
            Logger.Error($"Move item {ItemId.ToRestId()} to folder {targetFolderId} failed.", ex);
            return false;
        }
        return true;
    }

    public async System.Threading.Tasks.Task<bool> SetExtendedPropertyAsync(ExtendedPropertyDefinition prop, object value)
    {
        var propId = prop.ToGraphExtendedPropId();
        var valueStr = value.ToString() ?? string.Empty;

        var tempMessage = new GraphV1Message();
        tempMessage.SingleValueExtendedProperties = new List<GraphV1SingleValueLegacyExtendedProperty>
        {
            new GraphV1SingleValueLegacyExtendedProperty
            {
                Id = propId,
                Value = valueStr
            }
        };

        try
        {
            Service.Mails.UpdateMessageAsync(MailboxId, ParentFolderId.ToRestId(), ItemId.ToRestId(), tempMessage).ExecuteAsyncTask();
        }
        catch (Exception ex)
        {
            Logger.Error($"Set extended property {prop} failed for item {ItemId.ToRestId()}", ex);
            return false;
        }

        AddOrInitSingleValueExtendedProperties(propId, valueStr);
        return true;
    }

    public async System.Threading.Tasks.Task<Stream> GetMimeContentAsync()
    {
        return Service.Mails.GetMessageMimeContentAsync(MailboxId, ParentFolderId.ToRestId(), ItemId.ToRestId()).ExecuteAsyncTask() ?? Stream.Null;
    }

    public async System.Threading.Tasks.Task<bool> SetRetentionLabelAsync(Guid labelId)
    {
        switch (this.ItemType)
        {
            case ExchangeConstants.ItemType.Message:
                return await SetRetentionLabelForMessageAsync(labelId);
            // case "IPM.Task":
            //     return GetTaskProperties();
            // case "IPM.Post":
            //     return GetPostProperties();
            // case "IPM.Appointment":
            //     return GetEventProperties();
            // case "IPM.Activity":
            //     return GetJournalProperties();
            // case "IPM.StickyNote":
            //     return GetNoteProperties();
            // case "IPM.Contact":
            //     return GetContactProperties();
            // case "IPM.Document":
            //     return GetDocumentProperties();
            // case "IPM.DistList":
            //     return GetDistListProperties();
            default:
                return false;
        }
    }

    private async System.Threading.Tasks.Task<bool> SetRetentionLabelForMessageAsync(Guid labelId)
    {
        var tempMessage = new GraphV1Message();
        tempMessage.SingleValueExtendedProperties = new List<GraphV1SingleValueLegacyExtendedProperty>
        {
            new GraphV1SingleValueLegacyExtendedProperty
            {
                Id = "Binary 0x3019", // Retention Id
                Value = labelId.ConvertFromGuidToBase64Id()
            },
            new GraphV1SingleValueLegacyExtendedProperty
            {
                Id = "Integer 0x301a", // Retention Period
                Value = "0" // 0 means use the default retention period defined in the label
            }
        };

        try
        {
            Service.Mails.UpdateMessageAsync(MailboxId, ParentFolderId.ToRestId(), ItemId.ToRestId(), tempMessage).ExecuteAsyncTask();
        }
        catch (Exception ex)
        {
            Logger.Error($"Set retention label {labelId} failed for item {ItemId.ToRestId()}", ex);
            throw;
        }

        // TODO: Update policy tag object
        return true;
    }

    private void AddOrInitSingleValueExtendedProperties(string id, string? value)
    {
        if (this.item.SingleValueExtendedProperties.IsNullOrEmpty())
        {
            this.item.SingleValueExtendedProperties = [];
        }
        var existingProp = this.item.SingleValueExtendedProperties.FirstOrDefault(p => p.Id == id);
        if (existingProp != null)
        {
            existingProp.Value = value;
        }
        else
        {
            this.item.SingleValueExtendedProperties.Add(new SingleValueLegacyExtendedProperty
            {
                Id = id,
                Value = value
            });
        }
    }

    public async Task<MailboxItem> GetItemAsync(string mailboxId, string folderId, string itemId)
    {
        try
        {
            return await Service.Mails.ExportImport.GetItemAsync(mailboxId, folderId, itemId.ToRestId());
        }
        catch (Exception ex)
        {
            Logger.Error($"Get mail box item {itemId} failed.", ex);
            return new MailboxItem { Id = itemId.ToRestId() };
        }
    }

    public async Task<MailboxFolder> GetFolderAsync(string mailboxId, string folderId)
    {
        try
        {
            return await Service.Mails.ExportImport.GetFolderByIdAsync(mailboxId, folderId);
        }
        catch (Exception ex)
        {
            Logger.Error($"Get mail box folder {folderId} failed.", ex);
            return new MailboxFolder { Id = folderId };
        }
    }
}
