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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.PlanGroup.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.PlanGroup
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMPlanGroupService
    {
        [OperationContract]
        List<PlanDtoForPlanGroup> GetPlansByPlanCategory(PlanCategory planCategory);

        [OperationContract]
        List<PlanDtoForPlanGroup> GetPlansByModule(Modules module);

        [OperationContract]
        List<PlanDtoForPlanGroup> GetPlansByGroupId(string planGroupId);

        [OperationContract]
        PlanGroupDto Create(PlanGroupDto dto);

        [OperationContract]
        List<PlanGroupDto> GetPlanGroups();

        [OperationContract]
        PlanGroupDto GetPlanGroup(string planGroupId);

        [OperationContract]
        PlanGroupDto GetPlanGroupWithPlans(string planGroupId);

        [OperationContract]
        List<PlanGroupDto> Delete(List<PlanGroupDto> items);

        [OperationContract]
        bool ValidatePlanGroupName(PlanGroupDto planGroup);

        [OperationContract]
        string Update(PlanGroupDto dto);

        [OperationContract]
        void RunPlanGroup(PlanGroupDto dto);

        [OperationContract]
        void RunSchedulePlanGroup(PlanGroupDto dto, PGScheduleType type = PGScheduleType.None, bool needSkip = false);

        [OperationContract]
        PlanGroupDto GetRunnablePlanGroup(string planGroupId, long startTime);

        [OperationContract]
        void DoOperationsAfterJobComplete(BaseJobDto info);

        [OperationContract]
        List<PlanDtoForPlanGroup> GetPlansByGroupOrderInfos(List<PlanOrderInfos> PlanOrderInfos);

        [OperationContract]
        string IsDoEidt(string id);

        //[OperationContract]
        //string isDoDelete(List<string> ids);

        [OperationContract]
        List<ScheduleJobMonitorResultDto> GetScheduleJobsForCalendarView(List<ScheduleDto> schedules, List<DateTime> times);

        [OperationContract]
        List<PlanDtoForPlanGroup> GetPlansByPlanIds(List<PlanDtoForPlanGroup> dtos);

        [OperationContract]
        List<PlanDtoForPlanGroup> GetPlanListByGroupId(string groupId);
    }
}
