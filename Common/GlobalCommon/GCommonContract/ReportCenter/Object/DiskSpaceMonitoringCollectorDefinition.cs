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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DiskSpaceMonitoringCollectorDefinition : BaseCollectorDefinition
    {
        [DataMember]
        public List<DiskSpaceLogicalDeviceDefinition> LogicalDevices { set; get; }

        [DataMember]
        public ServiceDto Media { set; get; }

        [DataMember]
        public override int BaseReportType
        {
            get
            {
                return (int)ReportType.DiskSpaceMonitoring;
            }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DiskSpaceLogicalDeviceDefinition
    {
        [DataMember]
        public State State { set; get; }
        [DataMember]
        public string Comment { set; get; }
        [DataMember]
        public string Id { set; get; }
        [DataMember]
        public string Name { set; get; }
        [DataMember]
        public List<ServiceDto> Media { set; get; }
        [DataMember]
        public List<DiskSpacePhysicalDeviceDefinition> PhysicalDevice { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DiskSpacePhysicalDeviceDefinition
    {
        [DataMember]
        public string Id { set; get; }
        [DataMember]
        public string Name { set; get; }
        [DataMember]
        public int Position { set; get; }
        [DataMember]
        public string Path { set; get; }
        [DataMember]
        public long TotalSize { set; get; }
        [DataMember]
        public long UsedSpace { set; get; }
        [DataMember]
        public long DocAveDataSize { set; get; }
        [DataMember]
        public long ModifyTime { set; get; }
        [DataMember]
        public string ConnectionString { set; get; }
        [DataMember]
        public List<DiskSpacePlanDefinition> Plans { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DiskSpacePlanDefinition
    {
        [DataMember]
        public string Id { set; get; }
        [DataMember]
        public string Name { set; get; }
        [DataMember]
        public int Type { set; get; }
        [DataMember]
        public long DataSize { set; get; }
        [DataMember]
        public string FarmName { set; get; }
        [DataMember]
        public List<JobIdAndCycleId> JobIdAndCycleIds { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobIdAndCycleId
    {
        [DataMember]
        public string JobId { set; get; }
        [DataMember]
        public string CycleId { set; get; }
    }
}
