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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.CommonFilter;
using System.Runtime.Serialization;

namespace AvePoint.GCommon.Contract.StorageOptimization.Object
{
    /// <summary>
    /// 此类继承FilterPolicy，用于manager和GUI之间，manager会将SOFilterPolicy转化为FilterPolicy发给Agent
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SOFilterPolicy : FilterPolicy
    {
        /// <summary>
        /// 用于表示filter policy的and or
        /// </summary>
        [DataMember]
        public bool IsAnd { get; set; }

        /// <summary>
        /// 用于处理时间类型的filter policy的时间回显
        /// </summary>
        [DataMember]
        public DisplayDateTime BeginTime { get; set; }

        /// <summary>
        /// 用于处理时间类型的filter policy的时间回显
        /// </summary>
        [DataMember]
        public DisplayDateTime EndTime { get; set; }
    }

    /// <summary>
    /// For display filter policy time, using by GUI and Manager.
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DisplayDateTime
    {
        [DataMember]
        public string StartTime { get; set; }

        [DataMember]
        public string TimeZoneId { get; set; }

        [DataMember]
        public bool IsDayLightSaving { get; set; }

    }
}
