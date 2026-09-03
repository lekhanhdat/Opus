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

using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AvePoint.GCommon.GraphAPI
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TeamObj: EntityBase
    {
        [JsonProperty(PropertyName = "Id")]
        public string GroupId
        {
            get;
            set;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "isArchived", Required = Newtonsoft.Json.Required.Default)]
        public bool? IsArchived { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "webUrl", Required = Newtonsoft.Json.Required.Default)]
        public string WebUrl { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "memberSettings", Required = Newtonsoft.Json.Required.Default)]
        public TeamMemberSettings MemberSettings
        {
            get;
            set;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "guestSettings", Required = Newtonsoft.Json.Required.Default)]
        public TeamGuestSettings GuestSettings
        {
            get;
            set;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "messagingSettings", Required = Newtonsoft.Json.Required.Default)]
        public TeamMessagingSettings MessagingSettings
        {
            get;
            set;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "funSettings", Required = Newtonsoft.Json.Required.Default)]
        public TeamFunSettings FunSettings
        {
            get;
            set;
        }
    }
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TeamMemberSettings: EntityBase
    {
        public bool? AllowCreateUpdateChannels
        {
            get;
            set;
        }

        public bool? AllowDeleteChannels
        {
            get;
            set;
        }

        public bool? AllowAddRemoveApps
        {
            get;
            set;
        }

        public bool? AllowCreateUpdateRemoveTabs
        {
            get;
            set;
        }

        public bool? AllowCreateUpdateRemoveConnectors
        {
            get;
            set;
        }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TeamGuestSettings: EntityBase
    {
        public bool? AllowCreateUpdateChannels
        {
            get;
            set;
        }

        public bool? AllowDeleteChannels
        {
            get;
            set;
        }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TeamMessagingSettings: EntityBase
    {
        public bool? AllowUserEditMessages
        {
            get;
            set;
        }

        public bool? AllowUserDeleteMessages
        {
            get;
            set;
        }

        public bool? AllowOwnerDeleteMessages
        {
            get;
            set;
        }

        public bool? AllowTeamMentions
        {
            get;
            set;
        }

        public bool? AllowChannelMentions
        {
            get;
            set;
        }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TeamFunSettings: EntityBase
    {
        public bool? AllowGiphy
        {
            get;
            set;
        }

        //[JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "giphyContentRating", Required = Newtonsoft.Json.Required.Default)]
        public GiphyRatingType? GiphyContentRating
        {
            get;
            set;
        }

        public bool? AllowStickersAndMemes
        {
            get;
            set;
        }

        public bool? AllowCustomMemes
        {
            get;
            set;
        }
    }

    [JsonConverter(typeof(EnumConverter))]
    public enum GiphyRatingType
    {
        strict,
        moderate,
        //unknownFutureValue
    }
}