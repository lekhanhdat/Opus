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
namespace AvePoint.GCommon.Contract.CloudAppAdmin.Object
{
    using AvePoint.GCommon.Contract.Common;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ADMailboxObj
    {
        [DataMember]
        public string Guid { get; set; }
        //[DataMember]
        //public string Name { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string Alias { get; set; }
        [DataMember]
        public string PrimarySmtpAddress { get; set; }
        [DataMember]
        public string UserPrincipalName { get; set; }
        [DataMember]
        public List<string> GrantSendOnBehalfTo { get; set; }
        [DataMember]
        public List<ADEmailAddress> EmailAddresses { get; set; }
        [DataMember]
        public string SharingPolicy { get; set; }
        [DataMember]
        public string RetentionPolicy { get; set; }
        [DataMember]
        public string RoleAssignmentPolicy { get; set; }
        [DataMember]
        public string AddressBookPolicy { get; set; }
        [DataMember]
        public bool? LitigationHoldEnabled { get; set; }
        [DataMember]
        public string ArchiveStatus { get; set; }
        [DataMember]
        public string ForwardingAddress { get; set; }
        [DataMember]
        public int MaxSendSize { get; set; }
        [DataMember]
        public int MaxReceiveSize { get; set; }
        [DataMember]
        public List<string> AcceptMessagesOnlyFrom { get; set; }
        [DataMember]
        public List<string> RejectMessagesFrom { get; set; }
        [DataMember]
        public int RecipientLimits { get; set; }
        [DataMember]
        public DateTime? WhenChangedUTC { get; set; }
        [DataMember]
        public DateTime? WhenCreatedUTC { get; set; }
        [DataMember]
        public DateTime? WhenSoftDeleted { get; set; }
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
        /// <summary>
        /// CASMailBoxProperties
        /// </summary
        [DataMember]
        public bool? ActiveSyncEnabled { get; set; }
        [DataMember]
        public bool? ImapEnabled { get; set; }
        [DataMember]
        public bool? MAPIEnabled { get; set; }
        [DataMember]
        public bool? OWAEnabled { get; set; }
        [DataMember]
        public bool? OWAforDevicesEnabled { get; set; }
        [DataMember]
        public bool? PopEnabled { get; set; }
        /// <summary>
        /// Get-Userporperties
        /// </summary>
        [DataMember]
        public string Initials { get; set; }
        [DataMember]
        public string ExternalDirectoryObjectId { get; set; }
        [DataMember]
        public bool? UMEnabled { get; set; }

        public ADMailboxObj()
        {
            EmailAddresses = new List<ADEmailAddress>();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ADEmailAddress
    {
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string EmailAddress { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ADRecipientType
    {
        [EnumMember]
        None,
        [EnumMember]
        UserMailbox,
    }
}
