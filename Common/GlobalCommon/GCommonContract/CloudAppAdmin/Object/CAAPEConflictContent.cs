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
    using Common;
    using Server.Common.Profile.Object;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAAPEConflictContent : IProfileContent
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public List<SimpleProfileDto> ProfileContents { get; set; }

        [DataMember]
        public string TenantId { get; set; }

        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public long LastModifiedTime { get; set; }

        [DataMember]
        public List<CAAConflictUserContent> ConflictUserContents { get; set; }

        public CAAPEConflictContent()
        {
            ProfileContents = new List<SimpleProfileDto>();
            ConflictUserContents = new List<CAAConflictUserContent>();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAAConflictUserContent
    {
        [DataMember]
        public SimpleADUser SimpleADUser { get; set; }

        [DataMember]
        public List<CAAPERuleConflictContent> CAAPERuleConflictContents { get; set; }

        public CAAConflictUserContent()
        {
            CAAPERuleConflictContents = new List<CAAPERuleConflictContent>();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAAPERuleConflictContent
    {
        [DataMember]
        public CAAPERuleCategory RuleCategory { get; set; }

        [DataMember]
        public List<CAAPERuleConflictDetailContent> CAAPERuleConflictDetailContents { get; set; }

        public CAAPERuleConflictContent()
        {
            CAAPERuleConflictDetailContents = new List<CAAPERuleConflictDetailContent>();
        }
    }

    public class CAAPERuleConflictDetailContent
    {
        [DataMember]
        //Group License Application Id
        public string SubKey { get; set; }

        [DataMember]
        public string SubDisplayName { get; set; }

        [DataMember]
        public List<SimpleProfileDto> WithinPolicyProfiles { get; set; }

        [DataMember]
        public List<SimpleProfileDto> OutOfPolicyProfiles { get; set; }

        public CAAPERuleConflictDetailContent()
        {
            WithinPolicyProfiles = new List<SimpleProfileDto>();
            OutOfPolicyProfiles = new List<SimpleProfileDto>();
        }
    }
}
