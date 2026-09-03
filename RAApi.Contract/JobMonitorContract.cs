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
using AvePoint.Api.Contract;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DocAveOnline.WebApi.Contracts
{
    #region Monitor Contract
    public class JobMoudleParam
    {
        public List<PlanCategory> Categories { get; set; }

        public List<string> JobIds { get; set; }

        public JobMonitorCommandType JobMonitorCommandType { set; get; }
        /// <summary>
        /// key 字段名, Value Match的value
        /// </summary>
        public Dictionary<string, List<string>> Filter { get; set; }
        /// <summary>
        /// 分页用的开始的记录数据
        /// </summary>
        public int Start { get; set; }

        /// <summary>
        /// 显示的长度
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 查询Job的类型
        /// </summary>
        public List<int> TypeList { get; set; }

        /// <summary>
        /// 按升序查询还是按降序查询
        /// </summary>
        public List<OrderColumn> Sorts { get; set; }

    }

    public class JobMoudleResult
    {
        public int TotalLength { get; set; }

        public List<Job> SelectionValues { get; set; }
        /// <summary>
        /// 保存当前查询的Job数据
        /// </summary> 
        public List<Job> Values { get; set; }
    }
    #endregion

    #region Detail Contract

    public class JobDetailParam
    {
        public string Id { get; set; }
        public int JobState { set; get; }
        public int JobType { set; get; }
        public int JobCategory { set; get; }
        public string PlanId { set; get; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public string CommonSearch { get; set; }
        public string TimeZoneId { get; set; }
        public TimeZoneType ZoneType { get; set; }
        public int[] States { get; set; }
        public int[] EntityTypes { get; set; }
    }
    public class JobDetailResults
    {

        public List<JobSummaryItem> SummaryItems { set; get; }

        public List<SOJobDetailDto> Values { get; set; }

        public int TotalLength { get; set; }

        public int JobType { get; set; }
    }

    public class JobDetail : BaseContract
    {

        public List<JobSummaryItem> SummaryItems { set; get; }

        public List<SOJobDetailDto> Values { get; set; }

        public int TotalLength { get; set; }

        public int JobType { get; set; }
    }

    public class JobDetailDto
    {
        public long ID { get; set; }

        public long Date { get; set; }
        /// <summary>
        /// 前台转换时间所用。
        /// 标明job default Timezone
        /// </summary>
        public string TimeZoneId { get; set; }

        public string Type { get; set; }

        /// <summary>
        /// 用来标识URL object name，这样可以方便用户快速定位object
        /// </summary>
        public string Title { get; set; }

        public string SrcURL { get; set; }

        public string DestURL { get; set; }

        public string SrcAgentHost { get; set; }

        public string DestAgentHost { get; set; }

        public string MediaHost { get; set; }

        public string PhysicalDevice { get; set; }

        public string Size { get; set; }

        public string Status { get; set; }

        public string Comment { get; set; }

        public string Option { get; set; }
    }
    public class SOJobDetailDto
    {
        public int EntityType { get; set; }
        //public long ID { get; set; }   //属性名叫ID 会被自动识别成Entity

        public long Date { get; set; }
        /// <summary>
        /// 前台转换时间所用。
        /// 标明job default Timezone
        /// </summary>
        public string TimeZoneId { get; set; }

        public string Type { get; set; }

        /// <summary>
        /// 用来标识URL object name，这样可以方便用户快速定位object
        /// </summary>
        public string Title { get; set; }

        public string SrcURL { get; set; }

        public string DestURL { get; set; }

        public string SrcAgentHost { get; set; }

        public string DestAgentHost { get; set; }

        public string MediaHost { get; set; }

        public string PhysicalDevice { get; set; }

        public string Size { get; set; }

        public string Status { get; set; }

        public string Comment { get; set; }

        public string Option { get; set; }

        public string Farm { get; set; }

        public string RuleName { get; set; }

        public string DataOperation { get; set; }

        public string Action { get; set; }

        public string LogicalDevice { get; set; }

        public string MoveDataTo { get; set; }

        public string FileName { get; set; }

        public string TargetFolder { get; set; }

        public string SourceFolder { get; set; }
    }

    public enum JobReportDetailStatus : int
    {
        [EnumMember]
        Success = 0,
        [EnumMember]
        Failed = 1,
        [EnumMember]
        Skipped = 2,
        [EnumMember]
        Filtered = 3,
    }
    public enum TimeZoneType
    {
        Default = 0,
        Local = 1
    }

    #endregion

    #region Job Summary
    public class JobSummaryItem
    {
        public string Title { set; get; }
        public List<KeyAndValue> SummaryRow { set; get; }
    }
    public class KeyAndValue
    {
        public string Key { set; get; }
        public string Value { set; get; }
    }
    #endregion

    public class OrderColumn
    {
        /// <summary>
        /// 需要排序属性的属性名字
        /// </summary>
        public string PropName { get; set; }

        /// <summary>
        /// Order < 0 stands for desc
        /// Order > 0 stands for asc
        /// </summary>
        public int Order { get; set; }


        private List<JobParam> jobParamList;
        /// <summary>
        /// 查询条件用到的job list.
        /// </summary>
        public List<JobParam> JobParamList
        {
            get
            {
                if (jobParamList == null)
                {
                    jobParamList = new List<JobParam>();
                }
                return jobParamList;
            }
            private set
            {
                jobParamList = value;
            }
        }
    }

    public class Job
    {
        public string Id { set; get; }
        public int Type { get; set; }
        public long StartTime { get; set; }
        public long FinishTime { set; get; }
        public double Progress { set; get; }
        public int State { set; get; }
        public string Detail { set; get; }
        public int PlanType { get; set; }
        public int Category { get; set; }
        public string UserName { get; set; }
        public string PlanName { get; set; }
        public string SrcAgentName { get; set; }
        public string DestAgentName { get; set; }
        public string PlanId { get; set; }
        public string Dependency { get; set; }
        public string RevIMKey { get; set; }
        public string Scope { get; set; }
    }

    public class Jobs : BaseContract
    {
        public List<Job> Values { get; set; }
    }


    /// <summary>
    /// 操作数据时所使用的dto ,例如查询，删除job
    /// </summary> 
    public class JobParam
    {
        public string JobId { get; set; }

        /// <summary>
        /// 0 not remove data
        /// 1 remove data
        /// </summary> 
        public int RemoveData { get; set; }

        /// <summary>
        /// 当前Job 是否被选中
        /// </summary> 
        public bool IsChecked { get; set; }
    }
    [DataContract]
    public class JobDto
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public string PlanName { get; set; }
        [DataMember]
        public Module Module { get; set; }
        [DataMember]
        public JobStatus Status { get; set; }
        [DataMember]
        public DateTime StartTime { get; set; }
        [DataMember]
        public DateTime? FinishTime { get; set; }
        [DataMember]
        public double Progress { get; set; }
        [DataMember]
        public RemoveNodeType NodeType { get; set; }
        [DataMember]
        public JobStatistics Statistics { get; set; }
    }
    [DataContract]
    public class JobResult : BaseContract
    {
        [DataMember]
        public List<JobDto> Jobs { get; set; }
        [DataMember]
        public long LimitSize { get; set; } //GB
    }
    [DataContract]
    public class JobStatistics
    {
        [DataMember]
        public int Successful { get; set; }
        [DataMember]
        public int Failed { get; set; }
        [DataMember]
        public int Skipped { get; set; }
        [DataMember]
        public int TotalCount { get; set; }
    }

    public enum JobMonitorCommandType
    {
        /// <summary>
        /// 根据条件获得job列表
        /// </summary>
        GetJobValues = 0,
        /// <summary>
        /// 根据JobId获取JobId的更新信息（保留暂时不做）
        /// </summary>
        GetJobStates = 1,
        /// <summary>
        /// 删除Job的操作状态
        /// </summary>
        DeleteJobValues = 2,
        /// <summary>
        /// 操作Ribbon的操作状态
        /// </summary>
        OptionRibbon = 3,
        /// <summary>
        /// 获得Job详细信息
        /// </summary>
        GetJobDetail = 4,
        /// <summary>
        /// 执行job的Pause操作
        /// </summary>
        PauseJobAction = 6,
        /// <summary>
        /// 执行job的Resume操作
        /// </summary>
        ResumeJobAction = 7,
        /// <summary>
        /// 执行job的Stop操作
        /// </summary>
        StopJobAction = 8,
        /// <summary>
        /// 执行job的Start操作
        /// </summary>
        StartJobAction = 9,
        /// <summary>
        /// 执行获取视图操作
        /// </summary>
        GetView = 10,
        /// <summary>
        /// 执行删除job的一些相关内容，此操作不会删除Job和Job Detail等相关信息。
        /// </summary>
        DeleteJobContent = 11,
        /// <summary>
        /// 执行rollback的相关操作
        /// </summary>
        Rollback = 12,
        /// <summary>
        /// 执行Index的相关操作
        /// </summary>
        Index = 13,
        /// <summary>
        /// 执行Restart的相关操作
        /// </summary>
        Restart = 14,
        /// <summary>
        /// 执行Mapping的相关操作
        /// </summary>
        Mapping = 15,
        /// <summary>
        /// 执行CopySnapShot的相关操作
        /// </summary>
        CopySnapShot = 16,
        /// <summary>
        /// 执行Dead Account Deletion相关操作
        /// </summary>
        DeadAccountDeletion = 17,
        /// <summary>
        /// 执行Search Result相关操作
        /// </summary>
        SearchResult = 18,
        /// <summary>
        /// 执行Rollback Changes相关操作
        /// </summary>
        RollbackChanges = 19,
        ChangeStatus = 20,
        BreakpointResume = 21,
        DeleteSourceContents = 22,
        Remove = 23,
        Promote = 24,
    }
    [DataContract]
    public enum Module
    {
        //None = 0,
        [EnumMember]
        GranularBackup = PlanCategory.GranularBackup,
        [EnumMember]
        Administrator = PlanCategory.CentralAdmin,
        [EnumMember]
        ContentManager = PlanCategory.ContentManager,
        [EnumMember]
        GranularRestore = PlanCategory.GranularRestore,
        [EnumMember]
        Replicator = PlanCategory.Replicator,
        [EnumMember]
        PlatformBackup = PlanCategory.PlatformRecoveryBackup,
        [EnumMember]
        PlatformRestore = PlanCategory.PlatformRecoveryRestore,
        [EnumMember]
        ConvertStubToContent = PlanCategory.ConvertStubToContent,
        [EnumMember]
        ScheduledStorageManager = PlanCategory.ExtenderScheduled,
        [EnumMember]
        //StorageOptimizationConfig = 10,
        OrphanBLOBRetention = PlanCategory.StubRetention,
        [EnumMember]
        //CASTSAdmSchedule = 12,
        //Auditor = 13,
        DeploymentManager = PlanCategory.DeploymentManager,
        [EnumMember]
        ReportCenter = PlanCategory.ReportCenter,
        [EnumMember]
        Archiver = PlanCategory.Archiver,
        [EnumMember]
        Connector = PlanCategory.Connector,
        [EnumMember]
        ArchiverRestore = PlanCategory.ArchiverRestore,
        [EnumMember]
        LogManager = PlanCategory.LogManager,
        [EnumMember]
        ArchiverRetention = PlanCategory.ArchiverRetention,
        [EnumMember]
        JobPruning = PlanCategory.JobPruning,
        [EnumMember]
        SharePointMigration = PlanCategory.SPMigration07To10,
        [EnumMember]
        PlatformMaintenanceManager = PlanCategory.PlatformRecoveryMaintenance,
        //LicenseManager = 25,
        //LanguageTranslater = 26,
        //AutomaticDownloadPatch = 27,
        //AutomaticNotifyNewPatch = 28
    }
    [DataContract]
    public enum JobStatus
    {
        [EnumMember]
        Waiting = -1,
        [EnumMember]
        InProgress = 0,
        [EnumMember]
        Started = 1,
        [EnumMember]
        Finished = 2,
        [EnumMember]
        Failed = 3,
        [EnumMember]
        Stopped = 4,
        [EnumMember]
        Paused = 5,
        [EnumMember]
        Skipped = 6,
        [EnumMember]
        FinishedWithException = 7,
        [EnumMember]
        Pending = 8,
        [EnumMember]
        Stopping = 9,
        [EnumMember]
        Pausing = 10
    }
    [DataContract]
    public class ArchiverExportJobDetailInfo : BaseContract
    {
        [DataMember]
        public string SiteCollectionURL { get; set; }
        [DataMember]
        public List<ArchiverExportJobDetail> Details { get; set; }
    }
    [DataContract]
    public class ArchiverExportJobDetail
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string FullPath { get; set; }
        [DataMember]
        public JobReportDetailStatus Status { get; set; }
        [DataMember]
        public string Version { get; set; }
        [DataMember]
        public string DestinationPath { get; set; }
    }

    public class ScheduleJobQueueResult : BaseContract { 
        public List<ScheduleJobQueueDto> ScheduleJobQueues { get; set; }
    }

    public class ScheduleJobQueueDto
    {
        public string Key { get; set; }
        public ScheduleJobQueueState State { get; set; }
    }

    public enum ScheduleJobQueueState
    {
        UnDefined = -1,
        Waiting = 0,
        Ready = 1,
        Running = 2,
        Finished = 3,
        NeedSkipSincePlan = 4,
        NeedSkipSinceDuplicate = 5,
        NeedFailedSinceTimeOut = 6,
        Skipping = 7,
        Failing = 8,
        NeedFailedSincePlanGroup = 9,
        NeedSkippedSincePlanGroup = 10,
        ManuallyInsert = 11,
        NeedStopSincePlanAutoStopIB = 12,
        NeedSkippedSinceRetension = 13
    }
}

