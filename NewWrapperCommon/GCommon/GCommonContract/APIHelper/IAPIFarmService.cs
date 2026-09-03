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



using System.ServiceModel;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.AveLicense;
using System.Collections.Generic;
using AvePoint.GCommon.Contract.AveModuleContract;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.APIHelper
{
    [ServiceContract]
    public interface IAPIFarmService
    {
        [OperationContract]
        GetFarmDtoResult GetLicensedFarmByAnyAgentType(string farmName, ModuleName licenseModule, List<string> agentTypes);

        [OperationContract]
        GetFarmDtoResult GetLicensedFarmByAllAgentType(string farmName, ModuleName licenseModule, List<string> agentTypes);

        [OperationContract]
        GetFarmDtoResult GetFarmByAnyAgentType(string farmName, List<string> agentTypes);

        [OperationContract]
        GetFarmDtoResult GetFarmByAllAgentType(string farmName, List<string> agentTypes);
        
        [OperationContract]
        List<FarmDto> GetInstalledFarms();

        [OperationContract]
        List<FarmDto> GetAvailableFarms();
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GetFarmDtoResult
    {
        [DataMember]
        public GetFarmDtoResultType Type { get; set; }

        [DataMember]
        public FarmDto FarmDto { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum GetFarmDtoResultType
    {
        [EnumMember]
        Successful,

        [EnumMember]
        NotFound,

        [EnumMember]
        NotLicensed,

        [EnumMember]
        NoAvailableAgent
    }
}
