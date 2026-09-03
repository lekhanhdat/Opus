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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Action
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RelationJobGroupDto
    {
        /// <summary>
        /// 前台选择的要删除的Job.
        /// </summary>
        [DataMember]
        public BaseJobDto Job { get; set; }

        /// <summary>
        /// 与选择删除的Job相关联的Job(s).
        /// </summary>
        [DataMember]
        public List<BaseJobDto> DependencyJobs { get; set; }

        /// <summary>
        /// 是否是被Hold的.
        /// </summary>
        [Obsolete("建议使用枚举型代替")]
        [DataMember]
        public bool IsHolded { get; set; }

        /// <summary>
        /// job的各种状态集.
        /// </summary>
        [DataMember]
        public Dictionary<StatusType, List<string>> StatusDictionary { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum StatusType
    {
        /// <summary>
        /// 正常.
        /// </summary>
        [EnumMember]
        Normal = 0,

        /// <summary>
        /// 是否是被Hold.
        /// </summary>
        [EnumMember]
        Holded = 1,

        /// <summary>
        /// 是否正在使用.
        /// </summary>
        [EnumMember]
        Using = 1 << 1,
    }
}
