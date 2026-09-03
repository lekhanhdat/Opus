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

namespace AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup.Object;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup.Object.OperationResult;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
    using AvePoint.GCommon.Contract.Server.GranularBackup.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Tree.Object;

    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMExchangeOnlineBackupService
    {
        [OperationContract]
        string CreatePlan(ExchangeOnlineBackupPlanDto plan);

        [OperationContract]
        BackupPlanOperationResult UpdatePlan(ExchangeOnlineBackupPlanDto plan);

        [OperationContract]
        ExchangeOnlineBackupPlanDto GetBackupPlanById(string planId);

        [OperationContract]
        List<SimpleDataDto> GetBackupPlansForShow();

        [OperationContract]
        List<PlanDto> GetPlansIncludeDeletedByPlanIds(IEnumerable<string> planIds);

        [OperationContract]
        EOBackupPlanOperationResult DeleteBackupPlans(List<string> planIds);

        [OperationContract]
        bool CheckPlanNameHasExisted(string planName, int[] planCategory);

        /// <summary>
        /// 对shared的plan，更新新加的sitecollecion权限给相应的用户
        /// </summary>
        [OperationContract]
        void UpdateSharedPlanPermission(PlanDto plan, List<string> siteCollectionIds);

        [OperationContract]
        string RunOnceBackup(ExchangeOnlineRunJobParams jobParams);

        [OperationContract]
        bool ExistRunningJob(string planId);

        [OperationContract]
        ExchangeOnlineBackupPlanDto GetQuickBackupDefaultSetting();

        [OperationContract]
        string SaveQuickBackupDefaultSetting(ExchangeOnlineBackupPlanDto plan);

        [OperationContract]
        string CreateScheduleScheme(ScheduleSchemeDto schemeDto);

        [OperationContract]
        string UpdateScheduleSchemeContent(ScheduleSchemeDto schemeDto);

        [OperationContract]
        Dictionary<string, List<ScheduleDto>> GetAllScheme();

        [OperationContract]
        List<ScheduleSchemeDto> GetAllScheduleSchemeInfos();

        [OperationContract]
        EOBackupPlanOperationResult BatchDeleteSchemeByIds(List<string> schemeIds);

        [OperationContract]
        bool CheckSchemeNameHasExisted(string schemeName);

        [OperationContract]
        FilterCallBackResult CreateExchangeFilter(ExchangeOnlineBackupFilterDto filter);

        [OperationContract]
        FilterCallBackResult EditExchangeFilter(ExchangeOnlineBackupFilterDto filter);

        [OperationContract]
        FilterCallBackResult GetExchangeFilterById(string id);

        [OperationContract]
        FilterCallBackResult GetAllExchangeFilters();

        [OperationContract]
        FilterCallBackResult GetAllExchangeFiltersWithNameId();

        [OperationContract]
        FilterCallBackResult DeleteExchangeFilter(string filterId);

        [OperationContract]
        FilterCallBackResult DeleteBatchExchangeFilters(IEnumerable<string> filterIds);

        [OperationContract]
        FilterCallBackResult IsFilterNameNameExist(string name, string excludeId);

        [OperationContract]
        ExchangeOnlineBackupResponse GetBackupInitializedDataForGUI(ExchangeOnlineBackupRequsetType requestType);

        [OperationContract]
        StoragePolicyDto GetStoragePolicyFreeSpaceById(string storagePolicyId);

        [OperationContract]
        List<ScheduleJobMonitorResultDto> GetScheduleJobsForCalendarView(List<ScheduleDto> schedules, List<DateTime> times);

        [OperationContract]
        ExchangeOnlineTreeNodeDto FilterTreeBySearchString(ExchangeOnlineTreeNodeDto node, string searchFilter);
    }
}
