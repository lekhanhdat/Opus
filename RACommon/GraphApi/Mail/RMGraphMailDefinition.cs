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
using Amazon.Runtime.Internal.Transform;
using Aspose.Email;
using AvePoint.GCommon.GraphAPI;
using AvePoint.RA.Common.Global.Utils;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.GraphApi.Mail
{
    public class RMGraphMailDefinition
    {
        [JsonProperty("message")]
        public RMGraphMailMessage Message { get; set; }

        [JsonProperty("saveToSentItems")]
        public bool SaveToSentItems { get; set; }
    }

    public class RMGraphMailMessage
    {
        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("body")]
        public RMGraphMailBody Body { get; set; }

        [JsonProperty("toRecipients")]
        public List<RMGraphMailReciver> ToRecipients { get; set; } = new();

        [JsonProperty("ccRecipients")]
        public List<RMGraphMailReciver> CcRecipients { get; set; } = new();

    }

    public class RMGraphMailBody
    {
        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }

    public class RMGraphMailReciver
    {
        [JsonProperty("emailAddress")]
        public RMGraphMailAddress MailAddress { get; set; }
    }

    public class RMGraphMailAddress
    {
        [JsonProperty("address")]
        public string Address { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class RMGraphGroupSiteUrl
    {
        [JsonProperty("webUrl")]
        public string Url { get; set; }
    }

    public class RMGroup
    {
        [JsonProperty("objectId")]
        public string ObjectId { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("classification")]
        public string Classification { get; set; }

        [JsonProperty("createdDateTime")]
        public string CreatedDateTime { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Include)]
        public string Description
        {
            get { return string.IsNullOrEmpty(this.mDescription) ? null : this.mDescription; }
            set { this.mDescription = value; }
        }

        [JsonIgnore]
        private string mDescription;

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("groupTypes")]
        public string[] GroupTypes { get; set; }

        [JsonProperty("mail")]
        public string Mail { get; set; }

        [JsonProperty("mailEnabled")]
        public object MailEnabled { get; set; }

        [JsonProperty("mailNickname")]
        public string MailNickname { get; set; }

        [JsonProperty("onPremisesLastSyncDateTime")]
        public object OnPremisesLastSyncDateTime { get; set; }

        [JsonProperty("onPremisesSecurityIdentifier")]
        public string OnPremisesSecurityIdentifier { get; set; }

        [JsonProperty("onPremisesSyncEnabled")]
        public object OnPremisesSyncEnabled { get; set; }

        [JsonProperty("proxyAddresses")]
        public string[] ProxyAddresses { get; set; }

        [JsonProperty("renewedDateTime")]
        public string RenewedDateTime { get; set; }

        [JsonProperty("resourceProvisioningOptions")]
        public string[] ResourceProvisioningOptions { get; set; }

        [JsonProperty("securityEnabled")]
        public object SecurityEnabled { get; set; }

        [JsonProperty("visibility")]
        public string Visibility { get; set; }

        [JsonProperty("creationOptions")]
        public string[] CreationOptions { get; set; }
        [JsonProperty(PropertyName = "extension_fe2174665583431c953114ff7268b7b3_Education_ObjectType")]
        public string EducationObjectType { get; set; }
    }
    public class BaseObj
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }
        [JsonProperty("@odata.nextLink")]
        public string OdataNextLink { get; set; }
    }
    public class ListGroupsObj: BaseObj
    {
        [JsonProperty("value")]
        public RMGroup[] Value { get; set; }
    }
    public class ListGroupOwnersObj : BaseObj
    {
        [JsonProperty("value")]
        public RMGroupOwner[] Value { get; set; }
    }
    public class RMGroupOwner
    {
        [JsonProperty("mail")]
        public string Mail { get; set; }
    }
    public class ListGroupsConversationsObj:BaseObj
    {
        [JsonProperty("value")]
        public GroupConversation[] Value { get; set; }
    }
    public class ListConversationsThreadObj : BaseObj
    {
        [JsonProperty("value")]
        public ConversationThread[] Value { get; set; }
    }
    public class ListThreadPostObj : BaseObj
    {
        [JsonProperty("value")]
        public ThreadPost[] Value { get; set; }
    }
    public class ListPostAttachmentObj : BaseObj
    {
        [JsonProperty("value")]
        public PostAttachment[] Value { get; set; }
    }
    public class ListCalendarEventObj : BaseObj
    {
        [JsonProperty("value")]
        public GroupCalendarEvent[] Value { get; set; }
    }
    public class GroupCalendarEvent
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("createdDateTime")]
        public string CreatedDateTime { get; set; }
        [JsonProperty("lastModifiedDateTime")]
        public string LastModifiedDateTime { get; set; }
        [JsonProperty("changeKey")]
        public string ChangeKey { get; set; }
        [JsonProperty("categories")]
        public string[] Categories { get; set; }
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }
        [JsonProperty("originalStartTimeZone")]
        public string OriginalStartTimeZone { get; set; }
        [JsonProperty("originalEndTimeZone")]
        public string OriginalEndTimeZone { get; set; }
        [JsonProperty("iCalUId")]
        public string ICalUId { get; set; }
        [JsonProperty("uid")]
        public string Uid { get; set; }
        [JsonProperty("reminderMinutesBeforeStart")]
        public string ReminderMinutesBeforeStart { get; set; }
        [JsonProperty("isReminderOn")]
        public bool IsReminderOn { get; set; }
        [JsonProperty("hasAttachments")]
        public bool HasAttachments { get; set; }
        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonProperty("bodyPreview")]
        public string BodyPreview { get; set; }
        [JsonProperty("importance")]
        public string Importance { get; set; }
        [JsonProperty("sensitivity")]
        public string Sensitivity { get; set; }
        [JsonProperty("isAllDay")]
        public bool IsAllDay { get; set; }
        [JsonProperty("isCancelled")]
        public bool IsCancelled { get; set; }
        [JsonProperty("isOrganizer")]
        public bool IsOrganizer { get; set; }
        [JsonProperty("responseRequested")]
        public bool ResponseRequested { get; set; }
        [JsonProperty("seriesMasterId")]
        public string SeriesMasterId { get; set; }
        [JsonProperty("showAs")]
        public string ShowAs { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("isOnlineMeeting")]
        public bool IsOnlineMeeting { get; set; }
        [JsonProperty("allowNewTimeProposals")]
        public bool AllowNewTimeProposals { get; set; }
        [JsonProperty("isDraft")]
        public bool IsDraft { get; set; }
        [JsonProperty("body")]
        public RMGraphMailBody Body { get; set; }
        [JsonProperty("start")]
        public CalendarDateTime Start { get; set; }

        [JsonProperty("end")]
        public CalendarDateTime End { get; set; }
        [JsonProperty("location")]
        public EventLocationPreview Location { get; set; }

        [JsonProperty("locations")]
        public EventLocation[] Locations { get; set; }
        [JsonProperty("organizer")]
        public RMGraphMailReciver Organizer { get; set; }
        [JsonProperty("attendees")]
        public RMGraphMailReciver[] Attendees { get; set; }
        [JsonProperty("recurrence")]
        public RecurrenceInfo Recurrence { get; set; }
        public string CalendarId { get; set; }
        public long TotalBodySize { get; set; }
        public int LegacyFreeBusyStatus { get; set; }
    }
    public class RecurrenceInfo
    {
        [JsonProperty("pattern")]
        public PatternInfo Pattern { get; set; }
        [JsonProperty("range")]
        public RangeInfo Range { get; set; }

    }
    public class RangeInfo
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("startDate")]
        public string StartDate { get; set; }
        [JsonProperty("endDate")]
        public string EndDate { get; set; }
        [JsonProperty("recurrenceTimeZone")]
        public string RecurrenceTimeZone { get; set; }
        [JsonProperty("numberOfOccurrences")]
        public int NumberOfOccurrences { get; set; }

    }
    public class PatternInfo
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("interval")]
        public int Interval { get; set; }
        [JsonProperty("month")]
        public int Month { get; set; }
        [JsonProperty("dayOfMonth")]
        public int DayOfMonth { get; set; }
        [JsonProperty("daysOfWeek")]
        public string[] DaysOfWeek { get; set; }
        [JsonProperty("firstDayOfWeek")]
        public string FirstDayOfWeek { get; set; }
        [JsonProperty("index")]
        public string Index { get; set; }

    }
    public class EventLocationPreview
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("locationType")]
        public string LocationType { get; set; }
        [JsonProperty("uniqueId")]
        public string UniqueId { get; set; }
        [JsonProperty("uniqueIdType")]
        public string UniqueIdType { get; set; }
    }
    public class EventLocation
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("locationUri")]
        public string LocationUri { get; set; }
        [JsonProperty("locationType")]
        public string LocationType { get; set; }
        [JsonProperty("uniqueId")]
        public string UniqueId { get; set; }
        [JsonProperty("uniqueIdType")]
        public string UniqueIdType { get; set; }
        [JsonProperty("address")]
        public EventLocationAddress Address { get; set; }
        [JsonProperty("coordinates")]
        public EventLocationCoordinates Coordinates { get; set; }
        [JsonProperty("organizer")]
        public EventLocationCoordinates Organizer { get; set; }

    }
    public class EventLocationAddress
    {
        [JsonProperty("street")]
        public string Street { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("countryOrRegion")]
        public string CountryOrRegion { get; set; }
        //[JsonProperty("postalCode")]
        //public long? PostalCode { get; set; }
    }
    public class EventLocationCoordinates
    {
        [JsonProperty("latitude")]
        public string Latitude { get; set; }
        [JsonProperty("longitude")]
        public string Longitude { get; set; }
    }
    public class CalendarDateTime
    {
        [JsonProperty("dateTime")]
        public string DateTime { get; set; }
        [JsonProperty("timeZone")]
        public string TimeZone { get; set; }
    }
    public class PostAttachment
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("lastModifiedDateTime")]
        public string LastModifiedDateTime { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("contentType")]
        public string ContentType { get; set; }
        [JsonProperty("size")]
        public long Size { get; set; }
        [JsonProperty("isInline")]
        public bool IsInline { get; set; }
        [JsonProperty("contentId")]
        public string ContentId { get; set; }
        [JsonProperty("contentLocation")]
        public string ContentLocation { get; set; }
        [JsonProperty("contentBytes")]
        public string ContentBytes { get; set; }
        public string ParentItemId { get; set; }
        public Dictionary<string, string> GetProperties()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("Id", this.Id);
            result.Add("LastModifiedDateTime", this.LastModifiedDateTime);
            result.Add("Name", this.Name);
            result.Add("ContentType", this.ContentType);
            result.Add("Size",this.Size.ToString());
            result.Add("IsInline", this.IsInline.ToString());
            result.Add("ContentId", this.ContentId);
            result.Add("ContentLocation", this.ContentLocation);
            result.Add("ParentItemId", this.ParentItemId);
            return result;
        }
    }
    public class GroupCalendar
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("color")]
        public string Color { get; set; }
        [JsonProperty("hexColor")]
        public string HexColor { get; set; }
        [JsonProperty("isDefaultCalendar")]
        public bool IsDefaultCalendar { get; set; }
        [JsonProperty("changeKey")]
        public string ChangeKey { get; set; }
        [JsonProperty("canShare")]
        public bool CanShare { get; set; }
        [JsonProperty("canViewPrivateItems")]
        public bool CanViewPrivateItems { get; set; }
        [JsonProperty("canEdit")]
        public bool CanEdit { get; set; }
        [JsonProperty("allowedOnlineMeetingProviders")]
        public string[] AllowedOnlineMeetingProviders { get; set; }
        [JsonProperty("defaultOnlineMeetingProvider")]
        public string DefaultOnlineMeetingProvider { get; set; }
        [JsonProperty("isTallyingResponses")]
        public bool IsTallyingResponses { get; set; }
        [JsonProperty("isRemovable")]
        public bool IsRemovable { get; set; }
        [JsonProperty("owner")]
        public RMGraphMailAddress Owner { get; set; }
    }
    public class ThreadPost
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("createdDateTime")]
        public string CreatedDateTime { get; set; }

        [JsonProperty("lastModifiedDateTime")]
        public string LastModifiedDateTime { get; set; }

        [JsonProperty("changeKey")]
        public string ChangeKey { get; set; }

        [JsonProperty("categories")]
        public string[] Categories { get; set; }

        [JsonProperty("receivedDateTime")]
        public string ReceivedDateTime { get; set; }

        [JsonProperty("hasAttachments")]
        public bool HasAttachments { get; set; }

        [JsonProperty("body")]
        public RMGraphMailBody Body { get; set; }

        [JsonProperty("from")]
        public RMGraphMailReciver From { get; set; }

        [JsonProperty("sender")]
        public RMGraphMailReciver Sender { get; set; }
        public string ConversationId { get; set; }
        public string ThreadId { get; set; }
        public long TotalBodySize { get; set; }
        public string Topic { get; set; }
        public MailPriority Importance { get; set; }
        public Dictionary<string, string> GetProperties()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("Id", this.Id);
            result.Add("CreatedDateTime", this.CreatedDateTime);
            result.Add("LastModifiedDateTime", this.LastModifiedDateTime);
            result.Add("ChangeKey", this.ChangeKey);
            result.Add("Categories",SerializerHelper.SerializeByJsonConvert(this.Categories));
            result.Add("ReceivedDateTime", this.ReceivedDateTime);
            result.Add("HasAttachments", this.HasAttachments.ToString());
            result.Add("From", SerializerHelper.SerializeByJsonConvert(this.From));
            result.Add("Sender", SerializerHelper.SerializeByJsonConvert(this.Sender));
            result.Add("ConversationId", this.ConversationId);
            result.Add("ThreadId", this.ThreadId);
            result.Add("Importance", this.Importance.ToString());
            return result;
        }

    }
    public class GroupConversation
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("hasAttachments")]
        public bool HasAttachments { get; set; }

        [JsonProperty("lastDeliveredDateTime")]
        public string LastDeliveredDateTime { get; set; }

        [JsonProperty("uniqueSenders")]
        public string[] UniqueSenders { get; set; }
        [JsonProperty("preview")]
        public string Preview { get; set; }
        public long TotalBodySize { get; set; }
    }
    public class ConversationThread: GroupConversation
    {
        [JsonProperty("isLocked")]
        public bool IsLocked { get; set; }
    }
    public class ListUsersObj
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        [JsonProperty("value")]
        public GraphUser[] Value { get; set; }
    }
    public class ItemInfo
    {
        [JsonProperty("fields")]
        public ItemFields Fields { get; set; }
    }
    public class ItemFields
    {
        [JsonProperty("_FileArchiveStatus")]
        public string FileArchiveStatus { get; set; }
    }
}
