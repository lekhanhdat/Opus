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
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.GCommon.Contract.Server.Common.Performance.Object
{
    [DataContract(Name = ContractConstants.Namespace)]
    [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Memery is unmodifiable as the cause of being referenced.")]
    public class CommonTopologyServer
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
            string str = "Name:{0}, Status:{1}, ServiceStatus:{2}, OS:{3},SystemType:{4}, Processor: {5}, Memory:{6}, "
                          + "MemoryUsage:{7}, CPUUsage:{8}, NetworkUsage:{9}, TotalLocalStorage:{10}, LocalStorageUsage:{11}, "
                          + "BytesReceivedPerSecond:{12}, BytesSentPerSecond:{13}";
            string result = string.Format(str, Name, Status, ServiceStatus, OS, SystemType, Processor, Memery, MemeryUsage, CpuUsage, NetworkUsage, TotalLocalStorage, LocalStorageUsage, BytesReceivedPerSecond, BytesSentPerSecond);
            return result;
        }
    }
}
