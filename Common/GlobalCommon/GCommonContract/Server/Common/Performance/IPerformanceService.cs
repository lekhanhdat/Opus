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




namespace AvePoint.GCommon.Contract.Server.Common.Performance
{
    #region using directives
    using System.Collections.Generic;
    using System.ServiceModel;
    using AvePoint.GCommon.Contract.AgentService.Object;
    using AvePoint.GCommon.Contract.ReportCenter.Object;
    #endregion

    [ServiceContract]
    public interface IPerformanceService
    {
        [OperationContract]
        CpuMemoryUsage GetCpuAndMemoryDetail();

        [OperationContract]
        List<ProcessDetails> GetProcessDetails();

        [OperationContract]
        DTopologyServer GetSystemInfo();

        [OperationContract]
        DNetworkConnection GetNetworkInfo(DNetworkConnection networkConnection);

        [OperationContract]
        double GetNetworkUsage();

        [OperationContract]
        AveLoadBalanceInfo GetLoadBalanceInfo();

        [OperationContract]
        List<AvePoint.GCommon.Contract.Gateway.Object.DiskInfoDto> GetLocalDiskInfo();
    }
}
