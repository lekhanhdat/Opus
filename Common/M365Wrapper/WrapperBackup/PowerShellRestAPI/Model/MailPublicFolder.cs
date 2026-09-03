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
using ExchangeBackupUtility.Graph.PowerShellRestAPI;
using Newtonsoft.Json;

namespace ExchangeUtility.Graph.PowerShellRestAPI
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class MailPublicFolder : PSBaseObject
    {
        [JsonProperty("Alias")]
        public string Alias { get; set; }

        #region CustomAttribute
        [JsonProperty("CustomAttribute1")]
        public string CustomAttribute1 { get; set; }

        [JsonProperty("CustomAttribute10")]
        public string CustomAttribute10 { get; set; }

        [JsonProperty("CustomAttribute11")]
        public string CustomAttribute11 { get; set; }

        [JsonProperty("CustomAttribute12")]
        public string CustomAttribute12 { get; set; }

        [JsonProperty("CustomAttribute13")]
        public string CustomAttribute13 { get; set; }

        [JsonProperty("CustomAttribute14")]
        public string CustomAttribute14 { get; set; }

        [JsonProperty("CustomAttribute15")]
        public string CustomAttribute15 { get; set; }

        [JsonProperty("CustomAttribute2")]
        public string CustomAttribute2 { get; set; }

        [JsonProperty("CustomAttribute3")]
        public string CustomAttribute3 { get; set; }

        [JsonProperty("CustomAttribute4")]
        public string CustomAttribute4 { get; set; }

        [JsonProperty("CustomAttribute5")]
        public string CustomAttribute5 { get; set; }

        [JsonProperty("CustomAttribute6")]
        public string CustomAttribute6 { get; set; }

        [JsonProperty("CustomAttribute7")]
        public string CustomAttribute7 { get; set; }

        [JsonProperty("CustomAttribute8")]
        public string CustomAttribute8 { get; set; }

        [JsonProperty("CustomAttribute9")]
        public string CustomAttribute9 { get; set; }

        #endregion

        #region DeliveryOptions
        [JsonProperty("GrantSendOnBehalfTo")]
        public string[] GrantSendOnBehalfTo { get; set; }

        [JsonProperty("ForwardingAddress")]
        public string ForwardingAddress { get; set; }

        [JsonProperty("DeliverToMailboxAndForward")]
        public bool DeliverToMailboxAndForward { get; set; }
        #endregion

        #region MailFlowSettings
        [JsonProperty("MaxSendSize")]
        public string MaxSendSize { get; set; }

        [JsonProperty("MaxReceiveSize")]
        public string MaxReceiveSize { get; set; }

        [JsonProperty("AcceptMessagesOnlyFrom")]
        public string[] AcceptMessagesOnlyFrom { get; set; }

        [JsonProperty("AcceptMessagesOnlyFromDLMembers")]
        public string[] AcceptMessagesOnlyFromDLMembers { get; set; }

        [JsonProperty("RequireSenderAuthenticationEnabled")]
        public bool RequireSenderAuthenticationEnabled { get; set; }

        [JsonProperty("RejectMessagesFrom")]
        public string[] RejectMessagesFrom { get; set; }

        [JsonProperty("RejectMessagesFromDLMembers")]
        public string[] RejectMessagesFromDLMembers { get; set; }
        #endregion 

        [JsonProperty("DisplayName")]
        public string DisplayName { get; set; }

        [JsonProperty("EmailAddresses")]
        public string[] EmailAddresses { get; set; }

        [JsonProperty("HiddenFromAddressListsEnabled")]
        public bool HiddenFromAddressListsEnabled { get; set; }

        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("PrimarySmtpAddress")]
        public string PrimarySmtpAddress { get; set; }
    }
}
