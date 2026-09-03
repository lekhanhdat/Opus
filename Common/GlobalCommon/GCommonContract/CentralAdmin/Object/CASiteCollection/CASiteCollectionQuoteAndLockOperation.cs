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




using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASiteCollectionQuoteAndLockOperation:CAOperation
    {
        [DataMember]
        public string SiteCollectionUrl { get; set; }
        [DataMember]
        public List<SiteCollectionQuotaTemplate> Templates { get; set; }
        [DataMember]
        public SiteCollectionQuotaTemplate NewOrSelectedTemplate { get; set; }
        [DataMember]
        public string SiteCollectionOwner { get; set; }
        [DataMember]
        public LockStatus LockStatus { get; set; }
        [DataMember]
        public bool IsIndividualQouta { get; set; }
        [DataMember]
        public QuotaOperation OperationType { get; set; }
        [DataMember]
        public string AdditionLockInformation { get; set; }
        [DataMember]
        public string CurrentStorageUsed { get; set; }
        [DataMember]
        public string CurrentUsageToday { get; set; }
        [DataMember]
        public string AverageUsage { get; set; }
        [DataMember]
        public string SandboxedSolutionQuota { get; set; }
        
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteCollectionQuotaTemplate
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public long MaxSiteStorage { get; set; }
        [DataMember]
        public bool MaxSiteStorageEnabled { get; set; }
        [DataMember]
        public long WarningStorage { get; set; }
        [DataMember]
        public bool WarningStorageEnabled { get; set; }
        [DataMember]
        public double MaxUsagePerDay { get; set; }
        [DataMember]
        public double WarningUsagePerDay { get; set; }
        [DataMember]
        public bool WarningUsagePerDayEnabled { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum LockStatus
    {
        [EnumMember]
        NotLocked,
        [EnumMember]
        AddContentPrevented,
        [EnumMember]
        ReadOnly,
        [EnumMember]
        NoAccess
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum QuotaOperation
    {
        [EnumMember]
        CreateNewTemplate,
        [EnumMember]
        DeleteTeamplate,
        [EnumMember]
        ConfigLockAndQuota,
        [EnumMember]
        EditTemplate
    }
}
