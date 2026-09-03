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
using AvePoint.GCommon.Contract.Replicator.Object.OperationResults;
using AvePoint.GCommon.Contract.ReportCenter.Common;
using AvePoint.GCommon.Contract.ReportCenter.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.SuperUserConfiguration.Object;

namespace AvePoint.GCommon.Contract.ReportCenter
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMReportCenterService
    {
        [OperationContract]
        ReportCenterMessage HandleMessage(ReportCenterMessage message);

        [OperationContract]
        ReportCenterMessage HandleConfiguration(ReportCenterMessage message);

        [OperationContract]
        ReportCenterMessage HandleExportReport(ReportCenterMessage message);
      
        [OperationContract]
        ScheduleTimeOperationResult GetScheduleJobsForCalendarView(IEnumerable<RCScheduleDto> schedules, DateTime start, DateTime end);

        [OperationContract]
        ScheduleDstTimeValidationResult CheckTimeValid(RCScheduleDto schedule);

        [OperationContract]
        BaseJobDto GetJobByJobId(string jobId);

        //[OperationContract]
        //void DeleteJobById(List<string> jobIds);

        [OperationContract]
        void DeleteJobAndData(RCJobDeletionChart deleteJob);
        [OperationContract]
        void UpdateSharedPlanPermission(PlanDto plan, List<string> siteCollectionIds);

        [OperationContract]
        AnonymousSettingDto HandleGetAnonymouse();
        [OperationContract]
        void HandleSaveAnonymouse(AnonymousSettingDto dto);

        [OperationContract]
        List<SuperUserConfigurationDto> GetAllTenantInfo();

    }
}
