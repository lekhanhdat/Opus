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
using System.ComponentModel;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.UsageCalculation.Object;

namespace AvePoint.GCommon.Contract.Gateway.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class StorageUsageDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public StorageType StorageType { get; set; }

        /// <summary>
        /// Unit:byte
        /// </summary>
        [DataMember]
        public long CurrentSize { get; set; }

        /// <summary>
        /// 所在Data Center service id
        /// </summary>
        [DataMember]
        public int ServiceId { get; set; }

        [DataMember]
        public string DataCenterName { get; set; }

        [DataMember]
        public List<UserInformationDto> UserInformations { get; set; }

        [DataMember]
        public List<TenantUsageDBInfo> TenantUsageDBInfos { get; set; } 
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum StorageType
    {
        [Description("")]
        [EnumMember]
        None = 0,

        [Description("Amazon")]
        [EnumMember]
        Amazon = 1,

        [Description("Azure")]
        [EnumMember]
        Azure = 2,
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UserInformationDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string TenantName { get; set; }

        [DataMember]
        public string Region { get; set; }

        [DataMember]
        public string TenantId { get; set; }

        [DataMember]
        public int LicenseType { get; set; }

        [DataMember]
        public DateTime ExpirationTime { get; set; }
    }
   [DataContract(Namespace = ContractConstants.Namespace)]
   public class TenantUsageDBInfo
   {
       [DataMember]
       public string TenantId{get;set;}
       
       [DataMember]
       public long DBSize{get;set;}
       
       [DataMember]
       public UsageStorageType DBType{get;set;}

       [DataMember]
       public DateTime CreateTime { get; set; }
   }
}
