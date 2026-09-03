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
using System.Text;
using System.ServiceModel;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.Common.Farm
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMFarmService
    {
        /// <summary>
        /// 根据farm id查询farm.
        /// </summary>
        /// <param name="farmId"></param>
        /// <returns></returns>
        [OperationContract]
        FarmDto GetFarmByFarmId(string farmId);

        [OperationContract]
        FarmDto GetByFarmName(string farmName);

        [OperationContract]
        List<FarmDto> GetInstalledFarms();

        [OperationContract]
        void UpdateFarm(FarmDto dto);

        [OperationContract]
        List<FarmDto> GetAllFarms();

        [OperationContract]
        List<FarmDto> GetInstalledFarmsWithinRemoteFarm();

        List<FarmDto> GetAvailableFarms();

        List<FarmDto> GetAvailableFarmsByType(FarmType type);

        List<FarmDto> GetAvailableFarmsByAgentType(string agentType);

        [OperationContract]
        string CreateFarm(FarmDto farm);

        [OperationContract]
        string CreateRemoteFarm();
        
        [OperationContract]
        FarmDto GetFarmByHost(string host);

        [OperationContract]
        [Obsolete]
        FarmDto GetFarmByType(FarmType type);

        [OperationContract]
        FarmDto GetFarmByAgentName(string agentName);

        int GetSPVersion(string farmId);

        bool CheckFarmSPVersion(string srcFarmId, string destFarmId);

        bool CheckSPVersion(AveSPVersion srcSPVersion, AveSPVersion destSPVersion);

        void RegisterLicense(string farmId);

        void UnregisterLicense(IEnumerable<string> farmIds);

        [OperationContract]
        void DeleteFarm(FarmDto dto);
    }
}
