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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.ReportCenter.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Storage.Entity;

namespace AvePoint.GCommon.Contract.Server.Common.ExportLocation.Object
{
    [KnownType(typeof(RCExportLocationDto))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExportLocationDto
    {
        [DataMember]
        public string Id { set; get; }
        [DataMember]
        public string Name { set; get; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string Path { get; set; }
        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string MediaStorageXri { get; set; }
        [DataMember]
        public string RelativePath { get; set; }

        [DataMember]
        public FarmDto Farm { get; set; }

        /// <summary>
        /// 区分各个模块的Export location类型
        /// </summary>
        [DataMember]
        public PlanCategory Category { get; set; }

        /// <summary>
        /// 每个模块所用的空间
        /// </summary>
        [DataMember]
        public long CategoryUseSpace { get; set; }

        /// <summary>
        /// 其他应用空间
        /// </summary>
        [DataMember]
        public long OtherUseSpace { get; set; }

        /// <summary>
        /// 剩余空间
        /// </summary>
        [DataMember]
        public long RemainingSpace { get; set; }

        /// <summary>
        /// online使用，将job report上传到azure上
        /// </summary>
        [DataMember]
        public PhysicalDeviceDto StorageDevice { get; set; }

    }

    /// <summary>
    /// 为Job Monitor的Report Export Location使用
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReportExportLocationDto : ExportLocationDto, IProfileContent, IComparable<ReportExportLocationDto>
    {
        [DataMember]
        public bool IsUseNetShare { get; set; }
        [DataMember]
        public long CreateTime { get; set; }
        [DataMember]
        public long UpdateTime { get; set; }

        public int CompareTo(ReportExportLocationDto other)
        {
            return this.UpdateTime.CompareTo(other.UpdateTime);
        }
    }

    [DataContract]
    public enum ReportExportLocationStatus
    {
        [EnumMember]
        Default = 0,
        [EnumMember]
        NoDefault = 1
    }
}
