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
using System.ServiceModel;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.StorageOptimization.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Common
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMProcessingPoolService
    {
        [OperationContract]
        SOReturnMessage CreateProcessingPool(ProcessingPoolContract contract);

        [OperationContract]
        ProcessingPoolContract GetProcessingPool(string Id);

        [OperationContract]
        SOReturnMessage DeleteProcessingPools(List<ProcessingPoolContract> list);

        [OperationContract]
        SOReturnMessage DeleteProcessingPool(List<ProcessingPoolContract> list);

        [OperationContract]
        List<ProcessingPoolContract> GetAllProcessingPool();

        [OperationContract]
        List<ProcessingPoolContract> GetAllProcessingPoolByModule(ProcessingPoolModule processingPoolModule);

        [OperationContract]
        SOReturnMessage UpdateProcessingPool(ProcessingPoolContract contract);

        [OperationContract]
        IList<FarmDto> GetFarmFromAgent();

        [OperationContract]
        List<ServiceGroupDto> GetAllAgentGroup(string farmId);

        [OperationContract]
        List<ServiceGroupDto> GetAllAgentGroupByModule(string farmId, ProcessingPoolModule module);

        [OperationContract]
        List<ProcessingPoolContract> GetAllProcessingPoolsByFarm(string farmId);

        [OperationContract]
        SOReturnMessage CheckEditProcessingPool(ProcessingPoolContract processingPoolContract);

        [OperationContract]
        bool IsProcessingPoolUsing(ProcessingPoolContract poolContract);
    }
}
