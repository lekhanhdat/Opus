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

namespace ExchangeCommonWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class EventEntity : OutlookItemEntity
    {
        [DataMember]
        public string TransactionId { get; set; }

        [DataMember]
        public string OriginalStartTimeZone { get; set; }

        [DataMember]
        public string OriginalEndTimeZone { get; set; }

        [DataMember]
        public string ICalUId { get; set; }

        [DataMember]
        public int? ReminderMinutesBeforeStart { get; set; }

        [DataMember]
        public bool? IsReminderOn { get; set; }

        [DataMember]
        public bool? HasAttachments { get; set; }

        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public string BodyPreview { get; set; }

        [DataMember]
        public string Importance { get; set; }

        [DataMember]
        public string Sensitivity { get; set; }

        [DataMember]
        public bool? IsAllDay { get; set; }

        [DataMember]
        public bool? IsCancelled { get; set; }

        [DataMember]
        public bool? IsOrganizer { get; set; }

        [DataMember]
        public bool? ResponseRequested { get; set; }

        [DataMember]
        public string SeriesMasterId { get; set; }

        [DataMember]
        public string ShowAs { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string WebLink { get; set; }

        [DataMember]
        public string OnlineMeetingUrl { get; set; }

        [DataMember]
        public bool? IsOnlineMeeting { get; set; }

        [DataMember]
        public string OnlineMeetingProvider { get; set; }

        [DataMember]
        public bool? AllowNewTimeProposals { get; set; }

        [DataMember]
        public bool? IsDraft { get; set; }

        [DataMember]
        public bool? HideAttendees { get; set; }

        [DataMember]
        public string Recurrence { get; set; }

        [DataMember]
        public object ResponseStatus { get; set; }

        [DataMember]
        public Body Body { get; set; }

        [DataMember]
        public DateTimeTimeZoneEntity Start { get; set; }

        [DataMember]
        public DateTimeTimeZoneEntity End { get; set; }

        [DataMember]
        public object Location { get; set; }

        [DataMember]
        public List<object> Locations { get; set; }

        [DataMember]
        public List<object> Attendees { get; set; }

        [DataMember]
        public object Organizer { get; set; }

        [DataMember]
        public object OnlineMeeting { get; set; }

        [DataMember]
        public DateTimeOffset? OriginalStart { get; set; }

        [DataMember]
        public object Calendar { get; set; }
    }
}