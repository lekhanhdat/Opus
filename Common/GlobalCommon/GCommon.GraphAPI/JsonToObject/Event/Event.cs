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

namespace AvePoint.GCommon.GraphAPI
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Event : OutlookItem
    {
        public Event() => ODataType = "microsoft.graph.event";

        /// <summary>
        /// Gets or sets transaction id.
        /// A custom identifier specified by a client app for the server to avoid redundant POST operations in case of client retries to create the same event. This is useful when low network connectivity causes the client to time out before receiving a response from the server for the client's prior create-event request. After you set transactionId when creating an event, you cannot change transactionId in a subsequent update. This property is only returned in a response payload if an app has set it. Optional.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "transactionId", Required = Required.Default)]
        public string TransactionId { get; set; }

        /// <summary>
        /// Gets or sets original start time zone.
        /// The start time zone that was set when the event was created. A value of tzone://Microsoft/Custom indicates that a legacy custom time zone was set in desktop Outlook.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "originalStartTimeZone", Required = Required.Default)]
        public string OriginalStartTimeZone { get; set; }

        /// <summary>
        /// Gets or sets original end time zone.
        /// The end time zone that was set when the event was created. A value of tzone://Microsoft/Custom indicates that a legacy custom time zone was set in desktop Outlook.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "originalEndTimeZone", Required = Required.Default)]
        public string OriginalEndTimeZone { get; set; }

        /// <summary>
        /// Gets or sets i cal uid.
        /// A unique identifier for an event across calendars. This ID is different for each occurrence in a recurring series. Read-only.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "iCalUId", Required = Required.Default)]
        public string ICalUId { get; set; }

        /// <summary>
        /// Gets or sets reminder minutes before start.
        /// The number of minutes before the event start time that the reminder alert occurs.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "reminderMinutesBeforeStart", Required = Required.Default)]
        public int? ReminderMinutesBeforeStart { get; set; }

        /// <summary>
        /// Gets or sets is reminder on.
        /// Set to true if an alert is set to remind the user of the event.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "isReminderOn", Required = Required.Default)]
        public bool? IsReminderOn { get; set; }

        /// <summary>
        /// Gets or sets has attachments.
        /// Set to true if the event has attachments.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "hasAttachments", Required = Required.Default)]
        public bool? HasAttachments { get; set; }

        /// <summary>
        /// Gets or sets subject.
        /// The text of the event's subject line.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "subject", Required = Required.Default)]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets body preview.
        /// The preview of the message associated with the event. It is in text format.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "bodyPreview", Required = Required.Default)]
        public string BodyPreview { get; set; }

        /// <summary>
        /// Gets or sets importance.
        /// The importance of the event. The possible values are: low, normal, high.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "importance", Required = Required.Default)]
        public string Importance { get; set; }

        /// <summary>
        /// Gets or sets sensitivity.
        /// The possible values are: normal, personal, private, confidential.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "sensitivity", Required = Required.Default)]
        public string Sensitivity { get; set; }

        /// <summary>
        /// Gets or sets is all day.
        /// Set to true if the event lasts all day.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "isAllDay", Required = Required.Default)]
        public bool? IsAllDay { get; set; }

        /// <summary>
        /// Gets or sets is cancelled.
        /// Set to true if the event has been canceled.isCancelled
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "isCancelled", Required = Required.Default)]
        public bool? IsCancelled { get; set; }

        /// <summary>
        /// Gets or sets is organizer.
        /// Set to true if the calendar owner (specified by the owner property of the calendar) is the organizer of the event (specified by the organizer property of the event). This also applies if a delegate organized the event on behalf of the owner.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "isOrganizer", Required = Required.Default)]
        public bool? IsOrganizer { get; set; }

        /// <summary>
        /// Gets or sets response requested.
        /// Default is true, which represents the organizer would like an invitee to send a response to the event.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "responseRequested", Required = Required.Default)]
        public bool? ResponseRequested { get; set; }

        /// <summary>
        /// Gets or sets series master id.
        /// The ID for the recurring series master item, if this event is part of a recurring series.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "seriesMasterId", Required = Required.Default)]
        public string SeriesMasterId { get; set; }

        /// <summary>
        /// Gets or sets show as.
        /// The status to show. The possible values are: free, tentative, busy, oof, workingElsewhere, unknown.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "showAs", Required = Required.Default)]
        public string ShowAs { get; set; }

        /// <summary>
        /// Gets or sets type.
        /// The event type. The possible values are: singleInstance, occurrence, exception, seriesMaster. Read-only.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "type", Required = Required.Default)]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets web link.
        /// The URL to open the event in Outlook on the web.Outlook on the web opens the event in the browser if you are signed in to your mailbox. Otherwise, Outlook on the web prompts you to sign in.This URL cannot be accessed from within an iFrame.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "webLink", Required = Required.Default)]
        public string WebLink { get; set; }

        /// <summary>
        /// Gets or sets online meeting url.
        /// A URL for an online meeting. The property is set only when an organizer specifies an event as an online meeting such as a Skype meeting. Read-only.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "onlineMeetingUrl", Required = Required.Default)]
        public string OnlineMeetingUrl { get; set; }

        /// <summary>
        /// Gets or sets is online meeting.
        /// True if this event has online meeting information, false otherwise. Default is false. Optional.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "isOnlineMeeting", Required = Required.Default)]
        public bool? IsOnlineMeeting { get; set; }

        /// <summary>
        /// Gets or sets online meeting provider.
        /// Represents the online meeting service provider. The possible values are teamsForBusiness, skypeForBusiness, and skypeForConsumer. Optional.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "onlineMeetingProvider", Required = Required.Default)]
        public string OnlineMeetingProvider { get; set; }

        /// <summary>
        /// Gets or sets allow new time proposals.
        /// True if the meeting organizer allows invitees to propose a new time when responding, false otherwise. Optional. Default is true.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "allowNewTimeProposals", Required = Required.Default)]
        public bool? AllowNewTimeProposals { get; set; }

        /// <summary>
        /// Gets or sets is draft.
        /// Set to true if the user has updated the meeting in Outlook but has not sent the updates to attendees. Set to false if all changes have been sent, or if the event is an appointment without any attendees.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "isDraft", Required = Required.Default)]
        public bool? IsDraft { get; set; }

        /// <summary>
        /// Gets or sets hide attendees.
        /// When set to true, each attendee only sees themselves in the meeting request and meeting Tracking list. Default is false.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "hideAttendees", Required = Required.Default)]
        public bool? HideAttendees { get; set; }

        /// <summary>
        /// Gets or sets recurrence.
        /// The recurrence pattern for the event.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "recurrence", Required = Required.Default)]
        public string Recurrence { get; set; }

        /// <summary>
        /// Gets or sets response status.
        /// Indicates the type of response sent in response to an event message.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "responseStatus", Required = Required.Default)]
        public object ResponseStatus { get; set; }

        /// <summary>
        /// Gets or sets body.
        /// The body of the message associated with the event. It can be in HTML or text format.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "body", Required = Required.Default)]
        public CMBody Body { get; set; }

        /// <summary>
        /// Gets or sets start.
        /// The date, time, and time zone that the event starts. By default, the start time is in UTC.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "start", Required = Required.Default)]
        public DateTimeTimeZone Start { get; set; }

        /// <summary>
        /// Gets or sets end.
        /// The date, time, and time zone that the event ends. By default, the end time is in UTC.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "end", Required = Required.Default)]
        public DateTimeTimeZone End { get; set; }

        /// <summary>
        /// Gets or sets location.
        /// The location of the event.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "location", Required = Required.Default)]
        public object Location { get; set; }

        /// <summary>
        /// Gets or sets locations.
        /// The locations where the event is held or attended from. The location and locations properties always correspond with each other. If you update the location property, any prior locations in the locations collection would be removed and replaced by the new location value.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "locations", Required = Required.Default)]
        public List<object> Locations { get; set; }

        /// <summary>
        /// Gets or sets attendees.
        /// The collection of attendees for the event.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "attendees", Required = Required.Default)]
        public List<object> Attendees { get; set; }

        /// <summary>
        /// Gets or sets organizer.
        /// The organizer of the event.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "organizer", Required = Required.Default)]
        public object Organizer { get; set; }

        /// <summary>
        /// Gets or sets online meeting.
        /// Details for an attendee to join the meeting online. Read-only.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "onlineMeeting", Required = Required.Default)]
        public object OnlineMeeting { get; set; }

        /// <summary>
        /// Gets or sets original start.
        /// The Timestamp type represents date and time information using ISO 8601 format and is always in UTC time. For example, midnight UTC on Jan 1, 2014 is 2014-01-01T00:00:00Z
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "originalStart", Required = Required.Default)]
        public DateTimeOffset? OriginalStart { get; set; }

        /// <summary>
        /// Gets or sets calendar.
        /// The calendar that contains the event. Navigation property. Read-only.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "calendar", Required = Required.Default)]
        public object Calendar { get; set; }
    }
}