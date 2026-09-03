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
    using Newtonsoft.Json;
    using System;

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class GroupExtraInfo : EntityBase
    {
        /// <summary>
        /// Indicates if people external to the organization can send messages to the group. 
        /// Default value is false.
        /// Backup: App and Delegate; Restore: Delegate
        /// </summary>
        [JsonProperty("allowExternalSenders")]
        public Boolean? AllowExternalSenders { get; set; }

        //[JsonProperty("assignedLabels")]
        //public AssignedLabel[] AssignedLabels { get; set; }

        //[JsonProperty("assignedLicenses")]
        //public readonly AssignedLicense[] AssignedLicenses;//Read-only

        /// <summary>
        /// Indicates if new members added to the group will be auto-subscribed to receive email notifications. 
        /// Default value is false.
        /// /// Backup: App and Delegate; Restore: Delegate
        /// </summary>
        [JsonProperty("autoSubscribeNewMembers")]
        public Boolean? AutoSubscribeNewMembers { get; set; }

        /// <summary>
        /// True if the group is not displayed in certain parts of the Outlook user interface: in the Address Book, in address lists for selecting message recipients, and in the Browse Groups dialog for searching groups; false otherwise. 
        /// Default value is false.
        /// Backup: App and Delegate; Restore: App and Delegate
        /// </summary>
        [JsonProperty("hideFromAddressLists")]
        public Boolean? HideFromAddressLists { get; set; }

        /// <summary>
        /// True if the group is not displayed in Outlook clients, such as Outlook for Windows and Outlook on the web, false otherwise. 
        /// Default value is false.
        /// Backup: App and Delegate; Restore: App and Delegate
        /// </summary>
        [JsonProperty("hideFromOutlookClients")]
        public Boolean? HideFromOutlookClients { get; set; }

        /// <summary>
        /// Indicates whether the signed-in user is subscribed to receive email conversations. 
        /// Default value is true.
        /// Backup: Delegate; Restore: Delegate
        /// </summary>
        [JsonProperty("isSubscribedByMail")]
        public Boolean? isSubscribedByMail { get; set; } //only delegated permissions


        //[JsonProperty("licenseProcessingState")]
        //public readonly string licenseProcessingState; //Read-only

        //[JsonProperty("unseenConversationsCount")]
        //public Int32 unseenConversationsCount { get; set; } //only delegated permissions

        //[JsonProperty("unseenCount")]
        //public Int32 unseenCount { get; set; } //only delegated permissions

        //[JsonProperty("unseenMessagesCount")]
        //public Int32 unseenMessagesCount { get; set; } //only delegated permissions
    }
    //public class AssignedLabel
    //{
    //    [JsonProperty("labelId")]
    //    public string LabelId { get; set; }
    //    [JsonProperty("displayName")]
    //    public string DisplayName { get; set; }
    //}
    //public class AssignedLicense
    //{
    //    [JsonProperty("skuId")]
    //    public string SkuId { get; set; }
    //    [JsonProperty("displayName")]
    //    public string DisplayName { get; set; }
    //}
}