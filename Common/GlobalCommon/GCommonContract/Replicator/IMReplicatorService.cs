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
using AvePoint.GCommon.Contract.Replicator.Object;
using AvePoint.GCommon.Contract.Replicator.Object.Message;
using AvePoint.GCommon.Contract.Replicator.Object.OperationResults;
using AvePoint.GCommon.Contract.Replicator.Object.ProfileContents;
using AvePoint.GCommon.Contract.Replicator.Object.Settings;
using AvePoint.GCommon.Contract.Replicator.Object.ViewModels;
using AvePoint.GCommon.Contract.Replicator.Settings;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Action;
using AvePoint.Adonis.Replicator.Contract.Settings;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;

namespace AvePoint.GCommon.Contract.Replicator
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMReplicatorService
    {
        #region For GUI

        [OperationContract]
        PlanOperationResult SavePlan(ReplicatorPlan plan);

        [OperationContract]
        PlanOperationResult UpdatePlan(ReplicatorPlan plan);

        [OperationContract]
        PlanOperationResult DeletePlans(IEnumerable<string> planIds);

        [OperationContract]
        PlanOperationResult LoadPlan(string id);

        /// <summary>
        /// 获得所有Plan的Summary信息
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        PlanOperationResult LoadPlanSummaries();

        [OperationContract]
        PlanOperationResult LoadRealTimePlans();

        [OperationContract]
        PlanOperationResult LoadExistedPlanGroups();

        //[OperationContract]
        //PlanOperationResult UpdatePlanSummary(ReplicatorPlan plan);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">planId</param>
        /// <param name="runOption"></param>
        /// <returns></returns>
        [OperationContract]
        PlanOperationResult Run(string id, ReplicatorRunSetting runSettings);

        [OperationContract]
        PlanOperationResult LoadContainRealTimeMappingsPlan(string id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">planId</param>
        /// <param name="runOption"></param>
        /// <returns></returns>
        [OperationContract]
        PlanOperationResult RunRunNowJob(string id, ReplicatorRunSetting runSettings);

        /// <summary>
        /// 从给定PlanId中获取正在运行的PlanId
        /// </summary>
        /// <param name="planIds"></param>
        /// <returns></returns>
        [OperationContract]
        PlanOperationResult GetRunningPlans(IEnumerable<string> planIds);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="farmIds"></param>
        /// <returns></returns>
        [OperationContract]
        PlanOperationResult LoadAgentGroupsByFarmId(IEnumerable<string> farmIds);

        [OperationContract]
        PlanOperationResult LoadAllAgentGroups();

        [OperationContract]
        PlanOperationResult LoadAllFarms();

        [OperationContract]
        PlanOperationResult LoadPlanSummariesByFarmId(string farmId);

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Empty</returns>
        [OperationContract]
        ProfileOperationResult SaveProfile(ProfileDto profile);

        [OperationContract]
        ProfileOperationResult DoesNameExist(string profileName, ProfileType type);

        [OperationContract]
        ProfileOperationResult TestByteLevelPath(ProfileDto profile, bool createIfFolderNotExists);

        [OperationContract]
        ProfileOperationResult SaveByteLevel(ProfileDto profile, bool createIfFolderNotExists);

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Profile</returns>
        [OperationContract]
        ProfileOperationResult LoadProfileById(string id);

        /// <summary>
        /// 根据Type获得所有Profile的Name和Id信息，返回结果存在ProfileOperationResult中的Profiles中
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Profiles 里边只含Name Id属性</returns>
        [OperationContract]
        ProfileOperationResult LoadProfileSummariesByType(ProfileType type);

        /// <summary>
        /// 根据Type获得所有Profile的Name和Id信息，返回结果存在ProfileOperationResult中的ProfilesByType中
        /// </summary>
        /// <param name="types"></param>
        /// <returns>ProfilesByType</returns>
        [OperationContract]
        ProfileOperationResult LoadProfileSummariesByTypes(IEnumerable<ProfileType> types);

        /// <summary>
        /// 根据Type获取所有Profile的详细信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Profiles 含所有属性</returns>
        [OperationContract]
        ProfileOperationResult LoadProfilesByType(ProfileType type);

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Empty</returns>
        [OperationContract]
        ProfileOperationResult UpdateProfile(ProfileDto profile);

        /// <summary>
        /// 会判断Profile是否使用
        /// </summary>
        /// <returns>Empty</returns>
        [OperationContract]
        ProfileOperationResult DeleteProfiles(IEnumerable<string> profileIds, ProfileType profileType);

        [OperationContract]
        ProfileUsageConditionResult GetProfileUsageCondition(string profileId, ProfileType type);

        [OperationContract]
        ProfileUsageConditionResult GetRunningPlanReferenceTheSpecifiedProfile(string profileId, ProfileType type);

        /// <summary>
        /// 获取所有Farm的信息，提供给ConfigDB使用
        /// </summary>
        /// <returns>FarmConfigDBInfos</returns>
        [OperationContract]
        ProfileOperationResult GetAllFarmsForConfigDBSetting();

        [OperationContract]
        ProfileOperationResult GetAllFarmsForConfigDBSettingOnSystem();

        /// <summary>
        /// 
        /// </summary>
        /// <returns>FarmByteLevelInfos</returns>
        [OperationContract]
        ProfileOperationResult GetAllFarmsForByteLevelSetting();

        [OperationContract]
        ProfileOperationResult GetDefaultConfigDB(string farmId);

        [OperationContract]
        ProfileOperationResult TestConfigDB(ReplicatorConfigDBContent configDB, string farmId, string agentGroupId);

        [OperationContract]
        ProfileOperationResult SetAsDefault(string profileId, ProfileType type);
    
        [OperationContract]
        MappingOperationResult RegisterRealTimeMappings(List<string> planIds, bool register);

        /// <summary>
        /// Add to queue的时候判断mapping是否重复
        /// </summary>
        [OperationContract]
        MappingOperationResult ValidateMapping(ReplicatorMappingBase mapping);

        /// <summary>
        /// 获取给定plan下对应的所有mapping的状态
        /// </summary>
        /// <param name="planId">plan的ID</param>
        /// <returns>取其中的MappingsStatuses属性</returns>
        [OperationContract]
        MappingOperationResult LoadMappingStatuses(string planId);

        /// <summary>
        /// 在 Online Replicate 所有Plan的Mappings中得到与此节点相关的Mappings
        /// </summary>
        /// <param name="url">节点的Url</param>
        /// <returns></returns>
        [OperationContract]
        MappingOperationResult GetOtherRelativeMappings(string url);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mappingIds"></param>
        /// <returns>SubJobs</returns>
        [OperationContract]
        JobOperationResult GetSubJobInfos(IEnumerable<string> mappingIds);

        [OperationContract]
        ScheduleTimeOperationResult GetScheduleJobsForCalendarView(IEnumerable<ReplicatorSchedule> schedules, DateTime start, DateTime end);

        [OperationContract]
        JobOperationResult GetSubJobSummaries(string jobId);

        [OperationContract]
        JobOperationResult GetSubJobSummariesBySubJobId(string subJobId);

        [OperationContract]
        JobOperationResult GetSubJobDetail(string subJobId, JobReportDetailStatus[] statusFilter, int from, int to);
        
        [OperationContract]
        JobOperationResult SearchSubJobDetail(string subJobId, SubJobDetailSearchDto searchDto);

        [OperationContract]
        JobOperationResult GetAllSubJobsByMappingId(string mappingId);

        [OperationContract]
        JobOperationResult GetSubJobReports(IEnumerable<string> subJobIds);

        /// <summary>
        /// 根据每个subJobId和它的setting来rollback
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        [OperationContract]
        JobOperationResult Rollback(Dictionary<string, ReplicatorRollbackSetting> settings);

        [OperationContract]
        ScheduleTimeOperationResult ValidateSchedule(ReplicatorSchedule schedule);

        /// <summary>
        /// 判断replicator cache DB是否被多个farm使用
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        [OperationContract]
        ProfileOperationResult IsCacheDBUsedByMultiFarms(string profileId);

        [OperationContract]
        PlanOperationResult ImportPlan(ReplicatorPlanInfoModel models, ReplicatorImportPlanSetting setting);

        [OperationContract]
        PlanOperationResult AnalyzeOnlinePlanModel(byte[] fileData);

        [OperationContract]
        PlanOperationResult GetNonMappingPlans();

        [OperationContract]
        ProfileOperationResult GetRunningPlansWithGivenFarm(string farmId);

        [OperationContract]
        MappingOperationResult GetMappingTypeBySubJobId(string subJobId);

        [OperationContract]
        PlanOperationResult GetCurrentUserRole();

        [OperationContract]
        PlanOperationResult UpdateSharedSiteCollections(ReplicatorPlan replicatorPlan, List<string> newSiteCollectionIds);

        [OperationContract]
        PlanOperationResult TestUpdatePlan(ReplicatorPlan plan);

        #endregion

        #region For Agent

        [OperationContract]
        AgentOperationResult GetMappingsById(IEnumerable<string> ids);

        [OperationContract]
        AgentOperationResult GetAllMappings();

        [OperationContract]
        AgentOperationResult UpdateJobProgress(string jobId, string mappingId, MappingProgressMessage progress);

        [OperationContract]
        AgentOperationResult UpdateMappingReport(string jobId, string mappingId, ReplicatorJobReport jobReport);

        [OperationContract]
        AgentOperationResult UpdateJobReport(string jobId, ReplicatorReportSummary summary);

        [OperationContract]
        AgentOperationResult GetAllAgentLicenseInfoes();
        #endregion

        #region Replication Details
        [OperationContract]
        DashboardRecordOperationResult GetRecordsByFilter(ReplicatorDetailQueryCondition filter, bool searchAuto = false);

        [OperationContract]
        DashboardRecordOperationResult GetDownloadFilePathByDownloadType(ReplicatorDetailQueryCondition filter, List<ReplicationDetailColumnType> displayColumn, ReportFileType type, bool searchAuto = false);

        #endregion
    }
}
