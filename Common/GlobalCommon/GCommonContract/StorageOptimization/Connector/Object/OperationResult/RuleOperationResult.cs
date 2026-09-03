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
using AvePoint.GCommon.Contract.Media.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Connector.Object.OperationResult
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RuleOperationResult : ConnectorOperationResult
    {
        public RuleOperationResult()
            : base(false, null)
        {

        }

        public RuleOperationResult(bool hasError, OperationResultException exception)
            : base(hasError, exception)
        {

        }

        public static readonly RuleOperationResult Empty = new RuleOperationResult();

        private static RuleOperationResult paramErrorResult = null;
        public static RuleOperationResult GetParamErrorResult()
        {
            if (paramErrorResult == null)
            {
                paramErrorResult = new RuleOperationResult(true, new OperationResultException(OperationResultError.ParameterError));
            }
            return paramErrorResult;
        }

        [DataMember]
        public List<ConnectorInfoDto> ConnectorInfoDtos { get; set; }

        [DataMember]
        public List<string> ManagedPaths { get; set; }

        [DataMember]
        public Dictionary<string, PhysicalDeviceCheckResult> PhysicalDeviceCheckResult { get; set; }


        [DataMember]
        public List<ListSettingOperateResult> SaveResults { get; set; }

        [DataMember]
        public int[] SupportedTemplates { get; set; }
    }
}
