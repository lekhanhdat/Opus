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
    public class TopologyChart : BaseChart
    {
        [DataMember]
        public List<DTopologyService> Services { set; get; }
        [DataMember]
        public List<DNetworkConnection> NetworkConnection { set; get; }
        [DataMember]
        public DTopologyType Type { set; get; }

    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DTopologyService
    {
        [DataMember]
        public ServiceType ServiceType { get; set; }
        [DataMember]
        public string Name { set; get; }
        [DataMember]
        public int Status { set; get; }
        [DataMember]
        public long LastStartedTime { set; get; }
        [DataMember]
        public long OutOfServiceDuration { set; get; }
        [DataMember]
        public List<DTopologyServer> Servers { set; get; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DTopologyServer
    {
        [DataMember]
        public long LastStartTime { set; get; }
        [DataMember]
        public long LastStopTime { set; get; }
        [DataMember]
        public string Name { set; get; }
        [DataMember]
        public string Status { set; get; }
        [DataMember]
        public int ServiceStatus { set; get; }
        [DataMember]
        public string OS { set; get; }
        [DataMember]
        public string SystemType { set; get; }
        [DataMember]
        public string Processor { set; get; }
        [DataMember]
        public string Memery { set; get; }
        [DataMember]
        public string MemeryUsage { set; get; }
        [DataMember]
        public string CpuUsage { set; get; }
        [DataMember]
        public string NetworkUsage { set; get; }
        [DataMember]
        public string TotalLocalStorage { set; get; }
        [DataMember]
        public string LocalStorageUsage { set; get; }
        [DataMember]
        public long BytesReceivedPerSecond { set; get; }
        [DataMember]

        public long BytesSentPerSecond { set; get; }

        public override string ToString()
        {
            string str = "Name:{0}, Status:{1}, ServiceStatus:{2}, OS:{3},SystemType:{4}, Processor: {5}, Memery:{6}, "
                          + "MemeryUsage:{7}, CpuUsage:{8}, NetworkUsage:{9}, TotalLocalStorage:{10}, LocalStorageUsage:{11}, "
                          + "BytesReceivedPerSecond:{12}, BytesSentPerSecond:{13}";
            string result = string.Format(str, Name, Status, ServiceStatus, OS, SystemType, Processor, Memery, MemeryUsage, CpuUsage, NetworkUsage, TotalLocalStorage, LocalStorageUsage, BytesReceivedPerSecond, BytesSentPerSecond);
            return result;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DNetworkConnection
    {
        [DataMember]
        public string Source { set; get; }
        [DataMember]
        public string SourceIP { set; get; }
        [DataMember]
        public ServiceDto SourceServiceDto { set; get; }
        [DataMember]
        public string Destination { set; get; }
        [DataMember]
        public string DestinationIP { set; get; }
        [DataMember]
        public long NetworkLatency { set; get; }
        [DataMember]
        public long InBytes { set; get; }
        [DataMember]
        public long OutBytes { set; get; }

        public override string ToString()
        {
            return "source:" + Source + "," + "destination" + Destination;
        }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DTopologyType
    {
        [EnumMember]
        Service,
        [EnumMember]
        Network,
        [EnumMember]
        SystemInfo
    }
}
