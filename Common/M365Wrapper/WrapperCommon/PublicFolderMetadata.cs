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
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ExchangeCommonWrapper
{
    [DataContract]
    public class PublicFolderMetadata
    {
        [DataMember]
        public bool MailEnabled { get; set; }
        [DataMember]
        public string Identity { get; set; }
        [DataMember]
        public General General { get; set; }
        [DataMember]
        public Limits Limits { get; set; }
        [DataMember]
        public GeneralMailProperties GeneralMailProperties { get; set; }
        [DataMember]
        public PFEmailAddress EmailAddress { get; set; }
        [DataMember]
        public DeliveryOptions DeliveryOptions { get; set; }
        [DataMember]
        public MailFlowSettings MailFlowSettings { get; set; }
    }

    [DataContract]
    public class General
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public bool PerUserReadStateEnabled { get; set; }
    }

    [DataContract]
    public class Limits
    {
        [DataMember]
        public string IssueWarningQuota { get; set; }
        [DataMember]
        public string ProhibitPostQuota { get; set; }
        [DataMember]
        public string MaxItemSize { get; set; }
        [DataMember]
        public string RetainDeletedItemsFor { get; set; }
        [DataMember]
        public string AgeLimit { get; set; }
    }
    [DataContract]
    public class GeneralMailProperties
    {
        [DataMember]
        public string Alias { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public bool HiddenFromAddressListsEnabled { get; set; }
        [DataMember]
        public string CustomAttribute1 { get; set; }
        [DataMember]
        public string CustomAttribute2 { get; set; }
        [DataMember]
        public string CustomAttribute3 { get; set; }
        [DataMember]
        public string CustomAttribute4 { get; set; }
        [DataMember]
        public string CustomAttribute5 { get; set; }
        [DataMember]
        public string CustomAttribute6 { get; set; }
        [DataMember]
        public string CustomAttribute7 { get; set; }
        [DataMember]
        public string CustomAttribute8 { get; set; }
        [DataMember]
        public string CustomAttribute9 { get; set; }
        [DataMember]
        public string CustomAttribute10 { get; set; }
        [DataMember]
        public string CustomAttribute11 { get; set; }
        [DataMember]
        public string CustomAttribute12 { get; set; }
        [DataMember]
        public string CustomAttribute13 { get; set; }
        [DataMember]
        public string CustomAttribute14 { get; set; }
        [DataMember]
        public string CustomAttribute15 { get; set; }
    }
    [DataContract]
    public class PFEmailAddress
    {
        [DataMember]
        public string[] EmailAddresses { get; set; }
    }
    [DataContract]
    public class DeliveryOptions
    {
        [DataMember]
        //AccessRights: SendAs
        public string[] Trustees { get; set; }
        [DataMember]
        public string[] GrantSendOnBehalfTo { get; set; }
        [DataMember]
        public string ForwardingAddress { get; set; }
        [DataMember]
        public bool DeliverToMailboxAndForward { get; set; }
    }
    [DataContract]
    public class MailFlowSettings
    {
        [DataMember]
        public string MaxSendSize { get; set; }
        [DataMember]
        public string MaxReceiveSize { get; set; }
        [DataMember]
        public string[] AcceptMessagesOnlyFrom { get; set; }
        [DataMember]
        public string[] AcceptMessagesOnlyFromDLMembers { get; set; }
        [DataMember]
        // not use because automatically copied to the AcceptMessagesOnlyFrom and AcceptMessagesOnlyFromDLMembers properties
        public string[] AcceptMessagesOnlyFromSendersOrMembers { get; set; }
        [DataMember]
        public bool RequireSenderAuthenticationEnabled { get; set; }
        [DataMember]
        public string[] RejectMessagesFrom { get; set; }
        [DataMember]
        public string[] RejectMessagesFromDLMembers { get; set; }
        [DataMember]
        // not use because automatically copied to the RejectMessagesFrom and RejectMessagesFromDLMembers properties
        public string[] RejectMessagesFromSendersOrMembers { get; set; }
    }
    //[DataContract]
    //public class SendAsProperty
    //{
    //    [DataMember]
    //    public string AccessRights { get; set; }
    //    [DataMember]
    //    public string Identity { get; set; }
    //    [DataMember]
    //    public string Trustee { get; set; }
    //}
}