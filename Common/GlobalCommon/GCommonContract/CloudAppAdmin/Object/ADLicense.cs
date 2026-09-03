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
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ADLicense
    {
        [DataMember]
        public string SkuId { get; set; }

        [DataMember]
        public string ObjectId { get; set; }

        [DataMember]
        public string SkuPartNumber { get; set; }

        [DataMember]
        public string SkuDisplayName { get; set; }

        [DataMember]
        public List<ADServicePlan> ServicePlans { get; set; }

        [DataMember]
        public LicenseUnitsDetail PrepaidUnits { get; set; }

        [DataMember]
        public int? ConsumedUnits { get; set; }

        [DataMember]
        public ExpireTime ExpireTime { get; set; }

        public string CapabilityStatus { get; set; }
        [DataMember]
        public List<string> ScheduleIds { get; set; }

    }
    public class AssignUserLicenseInfo
    {
        [DataMember]
        public string SkuId { get; set; }
        [DataMember]
        public string FailedUserObjectId { get; set; }
       
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ADServicePlan
    {
        [DataMember]
        public string ServicePlanName { get; set; }

        [DataMember]
        public string ServicePlanDisplayName { get; set; }

        [DataMember]
        public string ServicePlanId { get; set; }

        [DataMember]
        public string CapabilityStatus { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LicenseUnitsDetail
    {
        [DataMember]
        public int? Enabled { get; set; }

        [DataMember]
        public int? Suspended { get; set; }

        [DataMember]
        public int? Warning { get; set; }
    }
}