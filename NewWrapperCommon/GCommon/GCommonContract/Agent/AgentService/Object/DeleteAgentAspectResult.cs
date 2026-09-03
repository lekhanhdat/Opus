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
using AvePoint.GCommon.Contract.AveModuleContract;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.AgentService.Object {

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DeleteAgentAspectResult {

        [DataMember]
        public ServiceDto Agent { get; set; }

        [DataMember]
        public Dictionary<ServiceGroupDto, List<DeleteAgentAspectResult>> GroupList { get; set; }

        /// <summary>
        /// 需要各模块封装
        /// 取值(e.g. ModuleContract.DocAvePlatform.ControlPanel.AgentGroup)
        /// </summary>
        [DataMember]
        public AveModule Module { get; set; }

        /// <summary>
        /// 需要各模块封装
        /// </summary>
        [DataMember]
        public OperationFlag OperationFlag { get; set; }

        /// <summary>
        /// 相关消息
        /// </summary>
        [DataMember]
        public string Message { get; set; }
    }

    public enum OperationFlag {
        SUCCESS,
        FAIL,
        NULL,
        EXCEPTION,
    }
}
