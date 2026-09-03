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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
using AvePoint.GCommon.Contract.PlatformRecovery.Object;
using AvePoint.GCommon.Contract.ReportCenter.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper;
using AvePoint.GCommon.Contract.Server.ControlPanel.LanguageMapping.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.PlatformRecovery
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMPlatformRecoveryService
    {
        #region Api

        /// <summary>加载tree(Api)</summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        AveTreeMessage BrowseTree(AveTreeMessage request);

        #endregion Api

        #region backup

        /// <summary>保存快速备份的plan(GUI该功能不再使用)</summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        [OperationContract]
        string SaveQuickBackupDefaultSetting(PRBackupPlanDto plan);

        /// <summary>读取快速备份的plan(GUI该功能不再使用)</summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        [OperationContract]
        PRBackupPlanDto LoadQuickBackupDefaultSetting();

        /// <summary>创建plan(GUI)</summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        [OperationContract]
        string CreatePlan(PRBackupPlanDto plan);

        /// <summary>Save as plan(GUI)</summary>
        /// <param name="planDto"></param>
        /// <returns></returns>
        [OperationContract]
        string SaveAsPlan(PRBackupPlanDto planDto);

        /// <summary>修改备份plan(GUI未使用)</summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        [OperationContract]
        void UpdateBackupPlan(PRBackupPlanDto plan);

        /// <summary>校验是否选择重复tree节点</summary>
        /// <param name="plan"></param>
        [OperationContract]
        void CheckSameDBForGUI(PRBackupPlanDto plan);

        /// <summary>得到Quickbackup默认设置信息(GUI该功能不再使用)</summary>
        /// <returns></returns>
        [OperationContract]
        PRBackupPlanDto GetQuickBackupDefaultSetting();

        /// <summary>得到PolicyData数据列表(GUI)</summary>
        /// <returns></returns>
        [OperationContract]
        List<StoragePolicyDto> GetPolicyDataCollection(PRStoragePolicyGUIMessge message);

        /// <summary>得到树静态信息(GUI-已移至界面处理)</summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        [OperationContract]
        double[] HandleStatisticsMessage(PRTreeNodeDto tree);

        /// <summary>得到所有StagingPolicy数据列表(GUI)</summary>
        /// <returns></returns>
        [OperationContract]
        List<PRStagingPolicyDto> GetPRStagingPolicyCollection();

        /// <summary>按照id得到StagingPolicy对象(GUI)</summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        PRStagingPolicyDto GetPRStagingPolicy(String id);

        /// <summary>创建StagingPolicy对象(GUI)</summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [OperationContract]
        PRStagingPolicyDto CreatePRStagingPolicy(PRStagingPolicyDto dto);

        /// <summary>修改StagingPolicy对象(GUI)</summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [OperationContract]
        PRStagingPolicyDto UpdatePRStagingPolicy(PRStagingPolicyDto dto);

        /// <summary> 删除StagingPolicy对象(GUI)</summary>
        /// <param name="id"></param>
        [OperationContract]
        void DeletePRStagingPolicy(string id);

        /// <summary>校验StagingPolicy中的信息(GUI未使用)</summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [OperationContract]
        OldErrorCode TestIsNameExist(PRStagingPolicyDto dto, bool isCreate);

        /// <summary>按类型得到所有agent列表(GUI)</summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [OperationContract]
        List<ServiceDto> GetAllAgentsByType(String type);

        /// <summary>按agent的id得到sql server的名称列表(GUI)</summary>
        /// <param name="agentId"></param>
        /// <returns></returns>
        [OperationContract]
        List<PRStagingInstanceInfo> LoadInstanceName(ServiceDto serviceDto);

        /// <summary>获取Storage policy的FreeSpace和TotalSpace信息. (GUI) </summary>
        /// <param name="dto"></param>
        [OperationContract]
        StoragePolicyDto GetStoragePolicyFreeSpaceById(string storagePolicyId);

        /// <summary>按照id得到plan对象（GUI）</summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        PRBackupPlanDto GetPlanById(string id);

        /// <summary>得到所有plan列表（GUI）</summary>
        /// <returns></returns>
        [OperationContract]
        List<PRBackupPlanDto> GetAllPlan();

        /// <summary>得到显示的plan的简易数据对象列表（GUI未使用）</summary>
        /// <returns></returns>
        [OperationContract]
        List<SimpleDataDto> GetAllPlanForShow();

        /// <summary>按照id删除plan对象（GUI）</summary>
        /// <param name="id"></param>
        [OperationContract]
        void DeletePlanById(string id);

        /// <summary>plan列表（GUI）</summary>
        /// <param name="list"></param>
        [OperationContract]
        void DeletePlanList(List<PRBackupPlanDto> list);

        /// <summary>修改plan对象（GUI）</summary>
        /// <param name="plan"></param>
        [OperationContract]
        void UpdatePlan(PRBackupPlanDto plan);

        /// <summary>校验VDB（GUI未使用）</summary>
        /// <param name="policy"></param>
        [OperationContract]
        bool CheckVDBByMediaService(StoragePolicyDto policy);

        /// <summary>获得SnapPolicy列表（GUI未使用）</summary>
        /// <returns></returns>
        [OperationContract]
        List<object> GetSnapPolicyList();

        /// <summary>校验plan名称是否重复(GUI) </summary>
        /// <param name="planDto"></param>
        /// <returns></returns>
        [OperationContract]
        bool CheckPlanNameExist(PRBackupPlanDto planDto);

        /// <summary>校验Plan 是否有Job 正在Running(GUI)</summary>
        /// <param name="planId">要校验的Plan</param>
        /// <returns></returns>
        [OperationContract]
        bool CheckPlanRunningJob(string planId);

        /// <summary>校验Plan 是否有Job 正在Running或Waiting(GUI)</summary>
        /// <param name="planId">要校验的Plan</param>
        /// <returns></returns>
        [OperationContract]
        PRCheckRunningWaitingPlanMessage CheckPlanRunningJobOrWaitingJob(string planId);

        /// <summary>校验Plan 是否有Job 正在Running(GUI)</summary>
        /// <param name="planIds">要校验的Plan Id 列表</param>
        /// <returns></returns>
        [OperationContract]
        List<string> CheckPlansRunningJob(List<string> planIds);

        /// <summary>校验Plan 是否有Job 正在Running或Waiting(GUI)</summary>
        /// <param name="planIds">要校验的Plan Id 列表</param>
        /// <returns></returns>
        [OperationContract]
        PRCheckRunningWaitingPlanMessage CheckPlansRunningOrWaitingJob(List<string> planIds);

        /// <summary> 创建一个only once类型的schedule(GUI) </summary>
        /// <param name="planId"></param>
        /// <returns></returns>
        [OperationContract]
        string RunOnceBackup(PRBackupPlanDto paraPlan, PRScheduleAdvanceOption prScheduleAdvanceOption);

        /// <summary>得到plan列表在planManager界面显示(GUI[planManager界面加载时调用,只返回需要显示的数据])</summary>
        /// <returns></returns>
        [OperationContract]
        List<PRBackupPlanDto> GetAllSimplePlanForShow();

        /// <summary>按plan template获得plan列表(GUI[planBuilder界面选择templatePlan时调用]未使用)</summary>
        /// <param name="PlanTemplateStr"></param>
        /// <returns></returns>
        [OperationContract]
        List<PRBackupPlanDto> GetSavedPlanForPlanTemplate(bool isPlanTemplate);

        [OperationContract]
        List<PRBackupPlanDto> GetPlanTemplate(PRBackupGUIMessage message);

        /// <summary>按id得到plan不包含tree信息(GUI[templatePlan选择后点击next按钮调用该方法]未使用) </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        PRBackupPlanDto GetPlanByIdWithoutTree(string id);

        /// <summary>planBuilder模块CalendarView时,schedule的创建(GUI)</summary>
        /// <param name="schedules"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        [OperationContract]
        List<ScheduleJobMonitorResultDto> GetScheduleJobsForCalendarView(List<ScheduleDto> schedules, List<DateTime> times);

        /// <summary>获得备份时间（Media）</summary>
        /// <param name="jobId"></param>
        [OperationContract]
        long GetBackupTime(string jobId);

        /// <summary>修改job表的index_status属性（Media）</summary>
        /// <param name="id"></param>
        /// <param name="indexStatus"></param>
        [OperationContract]
        void UpdateIndexStatusForPRJob(string id, int indexStatus);

        /// <summary>按条件获得VssSnapShotSetDto(Agent)</summary>
        /// <param name="agentId">agentID</param>
        /// <param name="jobId">无改条件时设置null</param>
        /// <returns></returns>
        [OperationContract]
        List<PRVssSnapShotSetDto> GetPRVssSnapShotSetDtoList(string agentId, string jobId);

        /// <summary>按id修改job的Performance属性。(Media)</summary>
        /// <param name="jobId"></param>
        /// <param name="performance"></param>
        [OperationContract]
        void UpdateJobPerformance(string jobId, string performance);

        /// <summary>按类型修改job的扩展状态（Agent and media）</summary>
        /// <param name="jobId"></param>
        /// <param name="status"></param>
        /// <param name="type"></param>
        [OperationContract]
        void UpdateJobExtendStatus(string jobId, JobExtendStatus status, ExtendStatusType type);

        /// <summary>发送消息到maintenance（GUI）</summary>
        /// <param name="job"></param>
        /// <param name="IsIndex"></param>
        /// <param name="IsMapping"></param>
        /// <param name="IsCopy"></param>
        /// <param name="stagingDto"></param>
        [OperationContract]
        void SendMessageToMaintenance(PRBackupJobDto job, bool IsIndex, bool IsMapping, bool IsCopy,
            PRStagingPolicyDto stagingDto, NotificationDto emailSetting);

        /// <summary>按照LogicDeviceId得到所有PRSiteMasterIndexDto(storagePolicy统计时使用)</summary>
        /// <param name="logicDeviceId">logicDeviceID</param>
        /// <returns></returns>
        [OperationContract]
        List<PRSiteMasterIndexDto> GetIndexInfoByLogicDeviceId(string logicDeviceId);

        /// <summary>按照logicalDevice获得index数据对象(RC)</summary>
        /// <param name="logicalDevice"></param>
        /// <returns></returns>
        [OperationContract]
        List<DiskSpacePlanDefinition> GetDiskSpacePlanDefinitions(DiskSpaceLogicalDeviceDefinition logicalDevice);

        /// <summary>批量删除staging(GUI)</summary>
        /// <param name="list"></param>
        [OperationContract]
        void DeletePRStagingPolicyList(List<PRStagingPolicyDto> list);

        /// <summary>按照id得到job</summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [OperationContract]
        PRBackupJobDto GetBackupJob(string jobId);

        /// <summary>校验备份时间有效性</summary>
        /// <param name="schedule"></param>
        [OperationContract]
        List<InvalidScheduleTime> CheckScheduleTimeForGUI(ScheduleDto schedule);

        /// <summary>判断是否支持VDI</summary>
        /// <returns></returns>
        [OperationContract]
        bool IsSupportVDI();

        /// <summary>校验邮件服务是否设置</summary>
        /// <returns></returns>
        [OperationContract]
        bool HasUseableNotificationSetting();

        #endregion backup

        #region restore

        [OperationContract]
        PRRestorePlanGUIMessge GetBackupPlanMessge(PRRestorePlanGUIMessge message);

        [OperationContract]
        PRRestorePlanDto GetPRRestorePlan(string id);

        /// <summary>获取指定时间段内backup data的记录(GUI未使用)</summary>
        /// <param name="timeStampType"></param>
        /// <returns></returns>
        [OperationContract]
        List<PRRestoreRecordDto> GetBackupDataRecords(long startTime, long endTime);

        /// <summary>获取所有backup data的farm name和对应的plan name(GUI)</summary>
        /// <returns></returns>
        [OperationContract]
        PRRestoreGUIMessge GetBackupDataFilterConditions(PRRestoreGUIMessge guiMessage);

        /// <summary>通过filter 条件search backup data 记录(GUI)</summary>
        /// <param name="filterDto"></param>
        /// <returns></returns>
        [OperationContract]
        List<PRRestoreRecordDto> SearchBackupDataRecords(PRRestoreDataFilter filterDto);

        /// <summary>创建job (GUI)</summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [OperationContract]
        string CreateRestorePlan(PRRestorePlanDto dto);

        /// <summary>DBtree还原outplace，获得选中节点信息(GUI未使用)</summary>
        /// <param name="srcTreeDto"></param>
        /// <returns></returns>
        [OperationContract]
        List<PRTreeNodeDto> GetSelectedNodeList(PRTreeNodeDto srcTreeDto);

        /// <summary>获得所有farm列表(GUI)</summary>
        /// <param name="farmId">源端tree第一个farm节点的ID</param>
        /// <returns></returns>
        [OperationContract]
        List<ServiceDto> GetAllDestination(string farmId);

        /// <summary>按farmName获得所有farm列表(GUI)</summary>
        /// <param name="farmName"></param>
        /// <returns></returns>
        [OperationContract]
        List<ServiceDto> GetAllDestinationByFarmName(string farmName);

        /// <summary>获得所有WebApplication列表(GUI[往agent发送信息]未使用)</summary>
        /// <param name="node">源端tree（需要取选中的WebApplication加载到控件）</param>
        /// <returns></returns>
        [OperationContract]
        List<string> GetWebApplicationList(PRTreeNodeDto node);

        /// <summary>获得outOfPlace节点的edit信息(GUI[往agent发送信息])</summary>
        /// <param name="node">选中节点</param>
        /// <param name="farmID">farmID</param>
        /// <returns></returns>
        [OperationContract]
        PRTreeNodeDto GetOutOfPlaceNodeEditInfo(PRTreeNodeDto node, ServiceDto serviceDto);

        /// <summary>当编辑OOP的dbServer后,调用获得对应的Database and log file mapping数据(GUI)</summary>
        /// <param name="serviceDto"></param>
        /// <param name="dbServer"></param>
        /// <returns></returns>
        [OperationContract]
        PRDBOOPInfo GetDBLocationInfo(ServiceDto serviceDto, string dbServer);

        /// <summary>获得outOfPlace信息(GUI)</summary>
        /// <param name="serviceDto"></param>
        /// <returns></returns>
        [OperationContract]
        PROOPRestoreBrowserContract GetOutOfPlaceInfo(ServiceDto serviceDto);

        /// <summary>按照jobid获得HostFullName属性(未使用)</summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [OperationContract]
        string GetHostFullNameByJobIdForMeida(string jobId);

        /// <summary>获得agentGroup列表(item 还原)</summary>
        /// <param name="farmId"></param>
        /// <returns></returns>
        [OperationContract]
        List<ServiceGroupDto> GetAgentGroupList(PRRestoreGUIMessge restoreMessage);

        /// <summary>得到staging信息(item 还原,未使用)</summary>
        /// <returns></returns>
        [OperationContract]
        List<PRStagingPolicyDto> GetPRStagingPolicyCollectionForRestore();

        /// <summary>得到语言设置(item 还原)</summary>
        /// <returns></returns>
        [OperationContract]
        LanguageMapping GetLanguageMapping();

        /// <summary>得到语言设置(item 还原)</summary>
        /// <returns></returns>
        [OperationContract]
        List<NameAndIdDto> GetAllLanguageMappingSettings();

        /// <summary>按照id获得Language对象</summary>
        /// <param name="mappingId"></param>
        /// <returns></returns>
        [OperationContract]
        LanguageMappingDto GetLanguageMappingSettingById(string mappingId);

        [OperationContract]
        List<NameAndIdDto> GetAllUserMappingSettings();

        [OperationContract]
        List<NameAndIdDto> GetAllDomainMappingSettings();

        /// <summary>得到action信息列表(item 还原)</summary>
        /// <param name="srcTree"></param>
        /// <param name="desTree"></param>
        /// <returns></returns>
        [OperationContract]
        List<ActionOption> GetActionInfoList(SPTreeNodeDto srcTree, SPTreeNodeDto desTree);

        /// <summary>获取ITEM还原overview界面统计信息</summary>
        /// <param name="srcTree">设置源端sptree</param>
        /// <param name="Guid">prtree选中的db节点的indexGuid</param>
        /// <param name="jobID">备份jobID</param>
        /// <returns></returns>
        [OperationContract]
        Dictionary<SPObjectLevel, PRRestoreStatistics> GetRestoreStatistics(SPTreeNodeDto srcTree, string guid, string jobID);

        /// <summary>测试staging的sqlServe账号是否有效</summary>
        /// <param name="serviceDto"></param>
        /// <param name="instanceName"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [OperationContract]
        bool TestSqlServerAcount(ServiceDto serviceDto, string instanceName, string name, string password);

        /// <summary>测试staging对应的空间大小</summary>
        /// <param name="serviceDto"></param>
        /// <param name="stagingPolicy"></param>
        /// <returns></returns>
        [OperationContract]
        PRStagingPolicyDto TestSpaceForStaging(ServiceDto serviceDto, PRStagingPolicyDto stagingPolicy);

        /// <summary>OOP注册帐号方法</summary>
        /// <param name="serviceDto"></param>
        /// <param name="configInfo"></param>
        /// <returns></returns>
        [Obsolete]
        [OperationContract]
        AgentConfigResult RegistAccount(ServiceDto serviceDto, AgentConfigInfo configInfo);

        /// <summary>OOP注册帐号方法</summary>
        [OperationContract]
        bool RegisterManagedAccount(ServiceDto agentDto, PRRegisterManagedAccountContract contract);

        [OperationContract]
        List<ServiceDto> GetAllAgents(string srcAgentID);

        /// <summary>获得PRBackupCatalogDto对象（Agent）</summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        [OperationContract]
        PRBackupCatalogDto GetCatalogDtoForAgent(string jobID);

        /// <summary>批量获得PRBackupCatalogDto对象</summary>
        /// <param name="jobIDList"></param>
        /// <returns></returns>
        [OperationContract]
        List<PRBackupCatalogDto> GetCatalogDtoListForAgent(List<string> jobIDList);
        /// <summary>给agent获得media处文件信息</summary>
        /// <param name="jobID"></param>
        /// <param name="fullPathList"></param>
        /// <returns></returns>
        [OperationContract]
        List<FullPathInfo> GetFilePathInfoFromMediaByJobID(string jobID, List<string> fullPathList);

        /// <summary>获得备份时WFE server环境中安装的程序对象（Media）</summary>
        /// <param name="jobID"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        [OperationContract]
        List<PRWFEObjectInfoDto> GetFEWList(string jobID, PRTreeNodeDto node);

        /// <summary>获得当前WFE server环境中安装的程序对象（Agent）</summary>
        /// <param name="jobID"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        [OperationContract]
        List<PRWFEObjectInfoDto> GetNewFEWList(string jobID, PRTreeNodeDto node);

        /// <summary>
        /// 当前job是否custom action
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        PRCustomActionInfo VerifyDataCustomAction(string jobId);

        /// <summary>验证加密信息是否存在(日历界面点击next时调用)</summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [OperationContract]
        PRGUIMessge VerifyRestoreData(PRGUIMessge message);

        #endregion restore

        #region Schedule Scheme

        [OperationContract]
        Dictionary<ScheduleSchemeDto, List<ScheduleDto>> GetAllScheme();

        [OperationContract]
        string CreateScheduleScheme(ScheduleSchemeDto SchemeDto);

        [OperationContract]
        bool CheckSchemeNameExist(string SchemeName);

        [OperationContract]
        void DeleteSchemeByName(string SchemeName);

        [OperationContract]
        void RenameSchemeName(string id, string NewName);

        [OperationContract]
        void AddScheduleInScheme(string SchemeName, ScheduleDto ScheduleDto);

        [OperationContract]
        void UpdateSchedule(ScheduleDto ScheduleDto);

        [OperationContract]
        void DeleteScheduleInScheme(ScheduleDto ScheduleDto);

        #endregion Schedule Scheme

        #region SiteMasterIndex

        /// <summary>用来创建SiteMasterIndex(Media)</summary>
        /// <param name="siteMasterIndex"></param>
        /// <param name="subInfo"></param>
        [OperationContract]
        void CreateSiteMasterIndexInfo(PRSiteMasterIndexDto siteMasterIndex);

        /// <summary>更新SiteMasterIndex(Media)</summary>
        /// <param name="siteMasterIndex"></param>
        [OperationContract]
        void UpdateSiteMasterIndexInfo(PRSiteMasterIndexDto siteMasterIndex);

        /// <summary>删除SiteMasterIndex(Media)</summary>
        /// <param name="id"></param>
        [OperationContract]
        void DeleteSiteMasterIndexInfo(string id);

        /// <summary>按照id得到PRSiteMasterIndexDto(Media)</summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        PRSiteMasterIndexDto GetSiteMasterIndexInfoById(string id);

        /// <summary>插入/修改PRSiteMasterIndexDto属性(Media)</summary>
        /// <param name="dto"></param>
        [OperationContract]
        MasterIndexReturnInfoDto InsertOrUpdatePlatformSiteInfo(PRSiteMasterIndexDto dto);

        /// <summary>获得最近一次fullBack类型的job(Media)</summary>
        /// <param name="planId"></param>
        /// <returns></returns>
        [OperationContract]
        string GetLatestFBJobId(String planId);

        /// <summary>获得当前job所在cycle中备份时间大于当前job的jobid列表(AGENT)</summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [OperationContract]
        List<string> GetAllIndexAfterBackupTime(string jobId);

        #endregion SiteMasterIndex

        #region Hardware Provider

        [OperationContract]
        void SaveVssProvider(PRVssProviderDto provider);

        [OperationContract]
        void SaveVssSnapshotSet(PRVssSnapShotSetDto set);

        [OperationContract]
        List<object> DeleteSnapshotSet(Guid snapshotSetId);

        [OperationContract]
        List<PRVssSnapShotSetDto> GetVssSnapshtoSet(string agentName, string jobId);

        [OperationContract]
        PlatformBackupRequest GetStoragePolicyDtoForAgent(string jobId, DataSecurity dataSecurity, CompressionType compressionType);

        [OperationContract]
        ServiceDto GetMediaServiceDtoForAgent(string jobId);

        [OperationContract]
        PRVssSnapShotSetDto GetVssSnapshotSetById(Guid snapshotSetID);

        [OperationContract]
        List<PRVssSnapShotSetDto> GetVssSnapshtoSetByPlan(string planId);

        #endregion Hardware Provider

        #region retention

        [OperationContract]
        void SetJobRetentionResult(PRJobRetentionMessage msg, string id);

        [OperationContract]
        void UpdateDeadLine(TimeSpan timeSpan, string id);

        #endregion retention

        #region Keep Live
        /// <summary>keep live方法(for GUI)</summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [OperationContract]
        PRKeepLiveGUIMessage KeepLiveMethod(PRKeepLiveGUIMessage message);
        /// <summary>设置返回结果(for Client)</summary>
        /// <param name="msg"></param>
        /// <param name="id"></param>
        [OperationContract]
        void SetKeepLiveResult(PRMessage msg, string id);
        /// <summary>设置结束时间(for Client)</summary>
        /// <param name="timeSpan"></param>
        /// <param name="id"></param>
        [OperationContract]
        void UpdateKeepLiveDeadLine(TimeSpan timeSpan, string id);
        #endregion Keep Live

        #region stop job

        /// <summary>更改job状态（Agent）</summary>
        /// <param name="jobId">job id</param>
        /// <param name="state">job状态</param>
        [OperationContract]
        void UpdateJobStateForStop(string jobId, int state);

        #endregion stop job

        #region get cycle data for agent

        [OperationContract]
        Dictionary<string, List<string>> GetCycleJobIds(List<string> jobIDs);

        [OperationContract]
        Dictionary<string, PlatformBackupRequest> GetMediaConfigMapByJobIds(List<string> jobIDs);

        #endregion get cycle data for agent

        #region PRSN Backup

        /// <summary>获得平台版本</summary>
        /// <returns></returns>
        [OperationContract]
        PlatformType GetServerVersion();

        [OperationContract]
        PRPlatformType GetPlatformType();

        /// <summary></summary>
        /// <param name="paraPlan"></param>
        /// <param name="prSNBackupInfoDto"></param>
        /// <param name="isTestRun">是否为testRun</param>
        /// <returns></returns>
        [OperationContract]
        string RunOnceBackupForPRSN(PRBackupPlanDto paraPlan, PRSNBackupInfoDto prSNBackupInfoDto, bool isTestRun);

        /// <summary>获得当前用户run now的设置信息</summary>
        /// <returns></returns>
        [OperationContract]
        PRSNBackupInfoDto GetRunOnceSettingByLoginUser();

        [OperationContract]
        List<PRStagingPolicyDto> GetPRStagingPolicyCollectionForPRSN();

        /// <summary>获得所有的staging</summary>
        /// <returns></returns>
        [OperationContract]
        List<PRStagingPolicyDto> GetPRStagingPolicyCollectionComboBoxForPRSN();

        /// <summary>获得farm对应的staging</summary>
        /// <param name="prStagingPolicyGUIMessge"></param>
        /// <returns></returns>
        [OperationContract]
        List<PRStagingPolicyDto> GetPRStagingPolicyCollectionComboBoxForPRSNByFarmName(PRGUIMessge prStagingPolicyGUIMessge);

        [OperationContract]
        PRStagingPolicyDto GetPRStagingPolicyForPRSN(string id);

        [OperationContract]
        void DeletePRStagingPolicyForPRSN(string id);

        [OperationContract]
        void DeletePRStagingPolicyListForPRSN(List<PRStagingPolicyDto> list);

        [OperationContract]
        OldErrorCode TestIsNameExistForPRSNVerifyIndex(PRStagingPolicyDto dto, bool isCreate);

        /// <summary>为PRSN创建staging(VerifyAndIndex)</summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [OperationContract]
        PRStagingPolicyDto CreatePRStagingPolicyForPRSN(PRStagingPolicyDto dto);

        /// <summary>为PRSN修改staging(VerifyAndIndex)</summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [OperationContract]
        PRStagingPolicyDto UpdatePRStagingPolicyForPRSN(PRStagingPolicyDto dto);

        [OperationContract]
        void SendMessageToMaintenanceForPRSN(PRBackupJobDto job, PRSNMaintenanceOptionDto maintenance, NotificationDto emailSetting);

        #endregion PRSN Backup

        #region PRSN Restore

        /// <summary>获得stagingPolicy信息(smsp)</summary>
        /// <returns></returns>
        [OperationContract]
        List<PRStagingPolicyDto> GetPRStagingPolicyCollectionForRestoreForPRSN();

        [OperationContract]
        /// <summary>按FarmName过滤StagingPolicy列表</summary>
        /// <param name="prStagingPolicyGUIMessge"></param>
        /// <returns></returns>
        List<PRStagingPolicyDto> GetPRStagingPolicyCollectionForRestoreForPRSNByFarmName(PRGUIMessge prStagingPolicyGUIMessge);

        #endregion PRSN Restore

        #region PRSN Data Management
        /// <summary>按平台获得storage policy列表</summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [OperationContract]
        List<StoragePolicyDto> GetAllStoragePolicyForDataManagement(PRDataManagerMessage message);

        /// <summary>获得StoragePolicy</summary>
        /// <returns></returns>
        [OperationContract]
        List<StoragePolicyDto> GetAllStoragePolicyForPRSN();

        #endregion PRSN Data Management

        #region 获取Shared路径给blob使用

        [OperationContract]
        string GetInstallSharedPath();

        #endregion 获取Shared路径给blob使用

        #region Data Management
        /// <summary>数据导入点击finish按钮时进行的处理</summary>
        /// <param name="dto"></param>
        [OperationContract]
        string UpgradeImportData(PlatformUpgradeParam paramDto, string notificationId);

        #endregion Data Management

        #region Notification

        /// <summary>获得邮箱信息列表</summary>
        /// <returns></returns>
        [OperationContract]
        List<ProfileDto> GetNotificationList();

        #endregion Notification

        #region Farm Rebuild

        [OperationContract]
        List<PRRestoreRecordDto> GetBackupJobWithConfigDB(PRRestoreDataFilter filterDto);

        [OperationContract]
        PRMessage HandleFarmRebuildAction(PRMessage request);

        [OperationContract]
        PRMessage CreateFarmRebuildPlanAndJob(PRRestorePlanDto plan);

        [OperationContract]
        void HandleCancelAction(PRMessage request);

        #endregion Farm Rebuild

        #region Restore From Alternate Location

        [OperationContract]
        PRMessage HandleSNRestoreFromAlternateLocationAction(PRMessage request);

        [OperationContract]
        PRMessage CreateRestoreFromAlternateLocationPlanAndJob(PRRestorePlanDto plan);

        [OperationContract]
        void HandleRestoreFromAlternateLocationCancel(PRMessage request);

        #endregion Restore From Alternate Location

        #region Migratuion

        [OperationContract]
        string RunMigration(SMSPMigrationRunDto migrationDto, bool isDatabase, string notificationId);

        [OperationContract]
        List<FarmDto> GetAllFarms();

        [OperationContract]
        PRSNMigrationBrowseMessage GetDBFromSelectedAgent(FarmDto farm, ServiceDto agent, bool IsDatabaseBrowser);

        [OperationContract]
        List<ServiceDto> GetAllAgentsByFarm(string farmId, bool isDatabase);

        #endregion Migratuion

        #region plan group

        [OperationContract]
        List<NameAndIdDto> GetAllPlanGroups();

        #endregion plan group

        #region security profile

        [OperationContract]
        List<NameAndIdDto> GetAllSecurityProfiles();

        #endregion security profile

        #region SMSP ScriptProfile(command and operation)

        /// <summary>按照profile类型获得PRSNCommandOperationDto对象列表</summary>
        /// <returns></returns>
        [OperationContract]
        List<PRSNCommandOperationDto> GetPRSNCommandOperationCollectionForDisplay();

        /// <summary>按照profile类型获得PRSNScriptProfileDto对象列表</summary>
        /// <param name="operationType">profile类型</param>
        /// <returns></returns>
        [OperationContract]
        List<PRSNCommandOperationDto> GetPRSNCommandOperationCollection(ScriptOperationType operationType);

        /// <summary>为PRSN按id获得CommandWithOperation</summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        PRSNCommandOperationDto GetPRSNCommandOperation(string id);

        /// <summary>创建PRSNScriptProfileDto</summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [OperationContract]
        PRSNCommandOperationDto CreatePRSNCommandOperationDto(PRSNCommandOperationDto dto);

        /// <summary>为PRSN修改CommandWithOperation</summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [OperationContract]
        PRSNCommandOperationDto UpdatePRSNCommandOperation(PRSNCommandOperationDto dto);

        /// <summary>为PRSN按id删除CommandWithOperation</summary>
        /// <param name="id"></param>
        [OperationContract]
        void DeletePRSNCommandOperation(string id);

        /// <summary>批量删除CommandOperation</summary>
        /// <param name="idList"></param>
        [OperationContract]
        void DeletePRSNCommandOperationList(List<PRSNCommandOperationDto> idList);

        /// <summary>为PRSN校验CommandWithOperation的名称是否重复</summary>
        /// <param name="dto"></param>
        /// <param name="isCreate"></param>
        /// <returns></returns>
        [OperationContract]
        bool TestPRSNCommandOperationIsNameExist(PRSNCommandOperationDto dto, bool isCreate);

        /// <summary>获得所有PR agent,TODO需要确认状态</summary>
        /// <returns></returns>
        [OperationContract]
        List<ServiceDto> GetAllPROperationAgents();

        #endregion SMSP ScriptProfile(command and operation)

        #region Advance search

        [OperationContract]
        SPTreeNodeDto HandleSearchBrowseMessage(PRAdvanceSearchGUIMessge message);

        #endregion Advance search

        #region Get Wrapper

        [OperationContract]
        DataEncryptionInfoWrapper GetDataEncryptionInfoWrapper(string profileGuid, string protectionKeyGUIDGuid);

        #endregion Get Wrapper

        #region Backup history
        [OperationContract]
        void InsertBackupHistory(PlatformBackupHistoryDto backupHistory);
        [OperationContract]
        void InsertMultipleBackupHistory(List<PlatformBackupHistoryDto> backupHistoryList);

        [OperationContract]
        List<PlatformBackupHistoryDto> GetBackupHistoryLaterThan(string dbserver, string dbname, DateTime backupStartTime);
        [OperationContract]
        List<PlatformBackupHistoryDto> GetDocumentBackupHistoryByPlan(string planId);
        #endregion
    }
}