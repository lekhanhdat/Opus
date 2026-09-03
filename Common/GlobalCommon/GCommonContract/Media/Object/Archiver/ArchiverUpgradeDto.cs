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




namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Storage.Entity;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverUpgradeDto
    {
        [DataMember]
        public String JobId { set; get; }

        [DataMember]
        public String PlanId { set; get; }

        [DataMember]
        public Int32 JobType { set; get; }

        [DataMember]
        public List<ArchiverUpgradeSubDto> ImportInfos { set; get; }

        [DataMember]
        public LogicalDeviceDto OldIndexLogicalDevice { set; get; }

        [DataMember]
        public Dictionary<String, LogicalDeviceDto> NewIndexLogicalDevices { set; get; }

        [DataMember]
        public List<StoragePolicyDto> DataStoragePolicies { set; get; }

        /// <summary>
        /// key 是D5中的job id，value是这个job用到的storage policy
        /// </summary>
        [DataMember]
        public Dictionary<String, List<StoragePolicyDto>> JobStoragePolicies { set; get; }

        [DataMember]
        public PlatformType PlatformType { set; get; }

        [DataMember]
        public ProductVersion ProductVersion { set; get; }

        [DataMember]
        public List<String> FailedSiteCollections { get; set; }

        public override String ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("Archiver Upgrade DTO: ");
            stringBuilder.AppendFormat("Job Id: {0}, ", this.JobId);
            stringBuilder.AppendFormat("Job Type: {0}, ", this.JobType);
            stringBuilder.AppendFormat("Old Index Logical Device: {0}", this.OldIndexLogicalDevice);
            return stringBuilder.ToString();
        }
    }
}
