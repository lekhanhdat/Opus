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
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.AgentService.Object
{
    /// <summary>
    /// load balance information
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AveLoadBalanceInfo
    {
        /// <summary>
        /// network interface adpater caption
        /// </summary>
        [DataMember]
        public string NetWorkInterfaceAdapterCaption { get; set; } 
        /// <summary>
        /// the speed of network interface
        /// </summary>
        [DataMember]
        public long NetworkBandWidth { get; set; } 
        /// <summary>
        /// the network upload speed.
        /// </summary>
        [DataMember]
        public long NetworkSentSpeed { get; set; } 
        /// <summary>
        /// the network download speed.
        /// </summary>
        [DataMember]
        public long NetworkReceivedSpeed { get; set; } 
        /// <summary>
        /// CurrentClockSpeed
        /// </summary>
        [DataMember]
        public UInt32 WindowsCPUHz { get; set; } 
        /// <summary>
        /// CPU usage
        /// </summary>
        [DataMember]
        public int CPUUsage { get; set; } 
        /// <summary>
        /// time until 1970.1.1...
        /// </summary>
        [DataMember]
        public long CurrentTime { get; set; } 
        /// <summary>
        /// TotalVisibleMemorySize
        /// </summary>
        [DataMember]
        public long TotalVisibleMemorySize { get; set; } 
        /// <summary>
        /// FreePhysicalMemory
        /// </summary>
        [DataMember]
        public long FreePhysicalMemory { get; set; } 
        /// <summary>
        /// calculated from total visiblememory size and free physical memory
        /// </summary>
        [DataMember]
        public int MemoryUsage { get; set; } 
    }
}
