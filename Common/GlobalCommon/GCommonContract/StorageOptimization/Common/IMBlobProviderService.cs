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
using System.ServiceModel;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Common
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMBlobProviderService
    {

        [OperationContract]
        SOReturnMessage UpdateBlobProviderInfo();

        [OperationContract]
        SOReturnMessage SyncFarmBlobStatus(string FarmId);

        [OperationContract]
        SOReturnMessage InstallBlobProviderBinary(BlobProviderBinary blobProviderBinary);

        [OperationContract]
        SOReturnMessage CreateStubDatabase(List<BlobProviderContract> blobProviders, bool isValidate);

        [OperationContract]
        SOReturnMessage MoveStubDatabase(MoveStubDatabaseRequest request);

        [OperationContract]
        SOReturnMessage UpdateContentDataBase(RuleNodeContract ruleNode, RuleNodeType type);

        [OperationContract]
        SOReturnMessage SetupBlobProvider(List<BlobProviderContract> farms);

        [OperationContract]
        void RunRBSSetting(FarmDto farm);

        [OperationContract]
        SOReturnMessage CheckStartTime(Dictionary<string, ScheduleDto> farmSchedules);

        /// <summary>
        /// 获得所有stubdb信息
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        Dictionary<string, StubDatabaseInfo> GetStubDBInfo();

        /// <summary>
        /// For schedule calendar view
        /// </summary>
        /// <param name="schedules"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        [OperationContract]
        List<ScheduleJobMonitorResultDto> GetScheduleJobsForCalendarView(List<ScheduleDto> schedules, List<DateTime> times);
    }
}
