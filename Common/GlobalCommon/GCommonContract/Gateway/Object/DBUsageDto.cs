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


using System.ComponentModel;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Gateway.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DBUsageDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string DBName { get; set; }

        [DataMember]
        public DBType DBType { get; set; }

        /// <summary>
        /// Unit:byte
        /// </summary>
        [DataMember]
        public long CurrentSize { get; set; }

        /// <summary>
        /// db的大小的上限，没设置为-1
        /// Unit:byte
        /// </summary>
        [DataMember]
        public long MaxSize { get; set; }

        /// <summary>
        /// 所在Data Center service id
        /// </summary>
        [DataMember]
        public int ServiceId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DBType
    {
        [Description("")]
        [EnumMember]
        None = 0,

        [Description("Control DB")]
        [EnumMember]
        ControlDB = 1,

        [Description("Recycle DB")]
        [EnumMember]
        RecycleDB = 2,

        [Description("Replicator DB")]
        [EnumMember]
        ReplicatorDB = 3,

        [Description("Usage DB")]
        [EnumMember]
        UsageDB = 4,

        [Description("Report DB")]
        [EnumMember]
        ReportDB = 5,

        [Description("Auditor DB")]
        [EnumMember]
        AuditorDB = 6,

        [Description("CA PolicyEnforcer DB")]
        [EnumMember]
        PolicyEnforcerDB = 7,
    }
}
