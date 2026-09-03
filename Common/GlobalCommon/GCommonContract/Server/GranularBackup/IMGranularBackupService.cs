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




namespace AvePoint.GCommon.Contract.Server.GranularBackup
{
    #region == using directives ==
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.GranularBackup.Object;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
    using AvePoint.GCommon.Contract.Server.GranularBackup.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.GCommon.Contract.DeploymentManager.Object;

    #endregion

    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMGranularBackupService
    {
        #region == Granular backup plan 增删改查==
        [OperationContract]
        string CreatePlan(GranularBackupPlanDto plan);

        /// <summary> Update backup plan info。 </summary>
        [OperationContract]
        BackupPlanOperationResult UpdatePlan(GranularBackupPlanDto plan);

        [OperationContract]
        GranularBackupPlanDto GetQuickBackupDefaultSetting();

        [OperationContract]
        string SaveQuickBackupDefaultSetting(GranularBackupPlanDto plan);

        [OperationContract]
        double[] HandleStatisticsMessage(SPTreeNodeDto tree);

        /// <summary> Docave6.0GA接口 </summary>
        [OperationContract]
        bool CheckPlanNameExist(string planId, string planName, string farmName, int[] planCategory);

        /// <summary> Docave 6.1接口 </summary>
        [OperationContract]
        bool CheckPlanNameHasExisted(string planId, string planName, int[] planCategory);

        /// <summary>获取PlanType是Backup PlanBuilder的plan. </summary>
        /// <returns></returns>
        [OperationContract]
        List<SimpleDataDto> GetBackupPlansForShow();

        [OperationContract]
        GranularBackupPlanDto GetBackupPlanById(string planId);

        [OperationContract]
        BackupPlanOperationResult DeleteBackupPlans(List<string> planIds);

        /// <summary> 根据planId,检查back plan是否在running job.</summary>
        /// <param name="planId"></param>
        /// <returns></returns>
        [OperationContract]
        bool ExistRunningJob(string planId);

        /// <summary> 创建一个IntervalType为OnlyOnce的schedule，来跑Backup job或Test run job。 </summary>
        /// <param name="jobParams"></param>
        /// <returns></returns>
        [OperationContract]
        string RunOnceBackup(ItemRunJobParams jobParams);

        /// <summary>
        /// 对shared的plan，更新 新加的sitecollecion权限 给相应的用户
        /// </summary>
        /// <param name="plan"></param>
        /// <param name="siteCollectionIds"></param>
        [OperationContract]
        void UpdateSharedPlanPermission(PlanDto plan, List<string> siteCollectionIds);

        [OperationContract]
        List<string> PruningBackupDataByJobIdsFromGui(List<string> jobIds, bool deleteJob);
        #endregion

        #region == Agent Group ==
        /// <summary> According to the farmId, find the agent groups </summary>
        /// <param name="farmId"></param>
        /// <returns></returns>
        [OperationContract]
        List<ServiceGroupDto> GetAgentGroupByFarmId(string farmId);
        #endregion

        #region ==Schedule Scheme ==
        [OperationContract]
        List<ScheduleSchemeDto> GetAllScheduleSchemeInfos();

        [OperationContract]
        string CreateScheduleScheme(ScheduleSchemeDto schemeDto);

        [OperationContract]
        bool CheckSchemeNameHasExisted(string schemeName);

        [OperationContract]
        BackupPlanOperationResult BatchDeleteSchemeByIds(List<string> schemeIds);

        [OperationContract]
        string UpdateScheduleSchemeContent(ScheduleSchemeDto schemeDto);

        [OperationContract]
        Dictionary<string, List<ScheduleDto>> GetAllScheme();

        [OperationContract]
        List<ScheduleJobMonitorResultDto> GetScheduleJobsForCalendarView(List<ScheduleDto> schedules, List<DateTime> times);
        #endregion ==

        #region == Storage Policy ==
        /// <summary> 获取Storage policy的FreeSpace和TotalSpace. </summary>
        /// <param name="dto"></param>
        [OperationContract]
        StoragePolicyDto GetStoragePolicyFreeSpaceById(string storagePolicyId);
        #endregion

        #region ==Backup GUI Get Init Data==
        /// <summary> 根据ItemBackupRequsetType参数，获取初始化页面元数据。 </summary>
        /// <param name="type"></param>
        /// <param name="farmId">当type == ItemBackupRequsetType.AgentGroup时，farmId不能为空。</param>
        /// <returns></returns>
        [OperationContract]
        ItemBackupResponse GetBackupInitializedDataForGUI(ItemBackupRequsetType requestType, string farmId);

        //根据farm的Ids获取webApp信息
        [OperationContract]
        Dictionary<string, List<NameAndIdDto>> GetWebAppDtoByFarmId(List<string> ids);

        //返回结果中第一个放的是SC总数，第二个放备份过的SC的数量
        [OperationContract]
        List<int> GetSiteCollectionCount(List<string> farmIds, List<NameAndIdDto> webAppNames);

        [OperationContract]
        Dictionary<DateTimeOffset, double> GetScheduleJobsByTimeRange(long startTime, long endTime, string timezoneId);

        [OperationContract]
        List<GranularBackupJobDto> GetBackupJobsByTime(long startTime, long endTime, string timezoneId);

        [OperationContract]
        Dictionary<int, int> GetJobStatesCount(DashBoardParamDto param);

        [OperationContract]
        List<ScheduleDto> GetSchedules(DashBoardParamDto param);
        #endregion

        [OperationContract]
        int GetSpVersionFromSiteMasterByPlanId(String planId);
    }
}
