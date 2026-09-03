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

namespace AvePoint.GCommon.Contract.UsageCalculation.Object
{
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.GUI;
    using AvePoint.GCommon.Contract.Storage.Entity;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ThroughputUsageItem
    {
        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public long SizeIn { get; set; }

        [DataMember]
        public long SizeOut { get; set; }

        [DataMember]
        public ThroughputType ThroughputType { get; set; }

        [DataMember]
        public string TenantGroupId { get; set; }

        [DataMember]
        public PhysicalDeviceType DeviceType { get; set; }

        [DataMember]
        public string DeviceName { get; set; }

        [DataMember]
        public JobRetentionState RetentionState { get; set; }

        [DataMember]
        public GUIModuleType Module { get; set; }
    }
}
