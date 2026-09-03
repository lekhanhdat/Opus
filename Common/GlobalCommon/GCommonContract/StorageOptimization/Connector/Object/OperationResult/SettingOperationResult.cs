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
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Connector.Object.OperationResult
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SettingOperationResult : ConnectorOperationResult
    {
        public SettingOperationResult()
            : base(false, null)
        {

        }

        public SettingOperationResult(bool hasError, OperationResultException exception)
            : base(hasError, exception)
        {

        }

        public static readonly SettingOperationResult Empty = new SettingOperationResult();

        private static SettingOperationResult paramErrorResult = null;
        public static SettingOperationResult GetParamErrorResult()
        {
            if (paramErrorResult == null)
            {
                paramErrorResult = new SettingOperationResult(true, new OperationResultException(OperationResultError.ParameterError));
            }
            return paramErrorResult;
        }

        [DataMember]
        public ProfileDto Profile { get; set; }

        [DataMember]
        public List<ProfileDto> Profiles { get; set; }

        [DataMember]
        public Dictionary<ProfileType, List<ProfileDto>> ProfilesByType { get; set; }

        [DataMember]
        public List<string> noUseMappingIds { get; set; }
    }
}
