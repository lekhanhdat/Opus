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


using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;

namespace AvePoint.GCommon.Contract.Gateway.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PlanSummaryDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public PlanCategory Category { get; set; }

        /// <summary>
        /// Category下plan数量
        /// </summary>
        [DataMember]
        public int PlanCount { get; set; }

        /// <summary>
        /// Category下plan运行过的已经完成的job的平均时间
        /// 小于0表示没运行过Job
        /// Unit：s
        /// </summary>
        [DataMember]
        public int AvgJobTime { get; set; }

        /// <summary>
        /// Category下plan运行过的已经完成的job的最长Job时间
        /// 小于0表示没运行过Job
        /// Unit：s
        /// </summary>
        [DataMember]
        public int MaxJobTime { get; set; }

        /// <summary>
        /// 所在Data Center service id
        /// </summary>
        [DataMember]
        public int ServiceId { get; set; }
    }
}
