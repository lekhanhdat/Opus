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
using System.Runtime.Serialization;

namespace AvePoint.Wrapper.Common
{
    [DataContract]
    public class AveComplianceTagInfo
    {
        [DataMember]
        public string ComplianceTagValue { get; set; }
        [DataMember]
        public string EncryptionRMSTemplateId { get; set; }
        [DataMember]
        public string TagName { get; set; }
        [DataMember]
        public Guid TagId { get; set; }
        [DataMember]
        public int TagDuration { get; set; }
        [DataMember]
        public bool SuperLock { get; set; }
        [DataMember]
        public string SharingCapabilities { get; set; }
        [DataMember]
        public string ReviewerEmail { get; set; }
        [DataMember]
        public bool RequireSenderAuthenticationEnabled { get; set; }
        [DataMember]
        public string Notes { get; set; }
        [DataMember]
        public bool IsEventTag { get; set; }
        [DataMember]
        public bool HasRetentionAction { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public bool ContainsSiteLabel { get; set; }
        [DataMember]
        public bool BlockEdit { get; set; }
        [DataMember]
        public bool BlockDelete { get; set; }
        [DataMember]
        public bool AutoDelete { get; set; }
        [DataMember]
        public string AllowAccessFromUnmanagedDevice { get; set; }
        [DataMember]
        public string AccessType { get; set; }
        [DataMember]
        public bool AcceptMessagesOnlyFromSendersOrMembers { get; set; }
        [DataMember]
        public string TagRetentionBasedOn { get; set; }
    }
}
