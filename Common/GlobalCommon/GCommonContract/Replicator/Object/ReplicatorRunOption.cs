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

namespace AvePoint.GCommon.Contract.Replicator.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorRunSetting
    {
        [DataMember]
        public ReplicatorRunOption RunOption { get; set; }

        [DataMember]
        public ReplicatorRunLevel RunLevel { get; set; }

        [DataMember]
        public bool ReplicateModifications { get; set; }

        [DataMember]
        public bool ReplicateDeletions { get; set; }

        [DataMember]
        public bool UseSpecialRefTime { get; set; }

        [DataMember]
        public int SpecialTimeNumber { get; set; }

        [DataMember]
        public TimeUnit SpecialTimeUnit { get; set; }

        [DataMember]
        public string TimeZoneId { get; set; }

        /// <summary>
        /// null if StartTime is Now.
        /// </summary>
        [DataMember]
        public DateTime? StartTime { get; set; }

        [DataMember]
        public string Description { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReplicatorRunOption : int
    {
        [EnumMember]
        Unknown = 0,

        [EnumMember]
        TestRun = 1,

        [EnumMember]
        Run = 2
    }

    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class ReplicatorRunJobMessage
    //{
    //    [DataMember]
    //    public string PlanId { get; set; }
    //    [DataMember]
    //    public ReplicatorRunSetting Settings { get; set; }
    //}
}
