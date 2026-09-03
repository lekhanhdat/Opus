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

namespace AvePoint.GCommon.Contract.MediaManagement.Object
{
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RetentionMessageBase : AveMessage
    {
        [DataMember]
        public String RetentionJobId { get; set; }

        [DataMember]
        public Int32 RetentionJobWeight { get; set; }

        [DataMember]
        public Int32 RetentionJobType { get; set; }

        [DataMember]
        public String UserAddress { get; set; }

        [DataMember]
        public String PlanName { get; set; }

        [DataMember]
        public String PlanId { get; set; }

        [DataMember]
        public String CycleId { get; set; }
        
        [DataMember]
        public String BatchFilePath { get; set; }

        [DataMember]
        public Dictionary<String, String> StorageInfoMap { get; set; }

        [DataMember]
        public CacheSettingDto Cache { get; set; }

        [DataMember]
        public LogicalDeviceDto LogicalDevice { get; set; }

        [DataMember]
        public LogicalDeviceDto DestinationDevice { get; set; }
        [DataMember]
        public RetentionType OperationType { get; set; }
    }
}
