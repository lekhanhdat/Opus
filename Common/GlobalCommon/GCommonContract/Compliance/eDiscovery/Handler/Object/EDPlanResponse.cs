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
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Handler.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EDPlanResponse : EDiscoveryResponse
    {
        /// <summary>
        /// LoadAll操作返回的结果
        /// </summary>
        [DataMember]
        public List<EDPlanDto> Plans { get; set; }


        /// <summary>
        /// LoadById返回的结果
        /// </summary>
        [DataMember]
        public EDPlanDto Plan { get; set; }


        /// <summary>
        /// Save返回的结果
        /// </summary>
        [DataMember]
        public SaveResultEnum SaveResult { get; set; }

        /// <summary>
        /// 执行Run Now的返回结果
        /// key为plan的id
        /// value为是否通知成功
        /// </summary>
        [DataMember]
        public Dictionary<string, bool> RunNowResults { get; set; }


        /// <summary>
        /// 删除返回的结果
        /// key为plan的id
        /// value为是否删除成功
        /// </summary>
        [DataMember]
        public Dictionary<string, bool> DeleteResults { get; set; }


        [DataContract(Namespace = ContractConstants.Namespace)]
        public enum SaveResultEnum
        {
            [EnumMember]
            Failed = 0,
            [EnumMember]
            Successful = 1,
            [EnumMember]
            HasExisted = 2
        }


        [DataContract(Namespace = ContractConstants.Namespace)]
        public enum StateEnum
        {
            [EnumMember]
            None = 0,
            [EnumMember]
            Exception = 1,
            [EnumMember]
            IllegalInput = 2,
            [EnumMember]
            IllegalSchedule = 3
        }

    }
}
