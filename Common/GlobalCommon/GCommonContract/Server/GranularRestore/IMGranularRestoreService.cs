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




namespace AvePoint.GCommon.Contract.Server.GranularRestore
{
    #region == using directives ==
    using System.Collections.Generic;
    using System.ServiceModel;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
    using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
    using AvePoint.GCommon.Contract.Server.GranularRestore.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.GCommon.Contract.DeploymentManager.Object;


    #endregion ==

    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMGranularRestoreService
    {
        /// <summary> 获取指定时间段内backup data的记录,不包括Failed job备份数据. </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [OperationContract]
        BackupDataCollectionDto GetBackupDataRecords(long startTime, long endTime);


        /// <summary> 获取默认时间段和默认BackupType的job记录 </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="backupType"></param>
        /// <returns></returns>
        [OperationContract]
        BackupDataCollectionDto GetDefaultDataRecords(long startTime, long endTime, AvePoint.GCommon.Contract.GranularBackup.Object.BackupType backupType);

        /// <summary>
        /// 获取所有backup data的farm name和对应的plan name
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        Dictionary<string, List<SimpleDataDto>> GetBackupDataFilterConditions();

        /// <summary>
        /// 通过filter 条件search backup data 记录
        /// </summary>
        /// <param name="filterDto"></param>
        /// <returns></returns>
        [OperationContract]
        List<BackupDataRecordDto> SearchBackupDataRecords(BackupDataFilterDto filterDto);

        /// <summary> 创建Restore plan记录。</summary>
        /// <param name="plan"></param>
        /// <returns>plan id</returns>
        [OperationContract]
        string CreatePlan(GranularRestorePlanDto plan);

        [OperationContract]
        GranularRestorePlanDto GetRestorePlan(string planId);

        /// <summary>返回 Advance Search 后Resotre tree。</summary>
        /// <param name="searchContract"></param>
        /// <returns></returns>
        [OperationContract]
        SPTreeNodeDto HandleSearchBrowseMessage(BackupDataSearchContract searchContract);

        /// <summary> 根据Backup jobId获得Statistics数据。</summary>
        /// <param name="backupJobId"></param>
        /// <returns></returns>
        [OperationContract]
        Dictionary<SPObjectLevel, MediaRestoreStatistics> GetRestoreStatistics(RestoreStatisticsContract request);

        /// <summary> 验证用户输入的FS path合法性。 </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [OperationContract]
        List<string> ValidateFSDestPathInfo(PhysicalDeviceDto dto, bool isCreateFolder = true);

        /// <summary> 根据ItemRequsetType类型参数, 获取页面初始化元数据。</summary>
        /// <param name="type"></param>
        /// <param name="farmId"></param>
        /// <returns></returns>
        [OperationContract]
        ItemRestoreResponse GetRestoreInitializedDataForGUI(ItemRequsetType type, string farmId);

        [OperationContract]
        List<BackupDataRecordDto> GetBackupNodeJobDetails(List<string> jobIds);

        [OperationContract]
        bool CheckAttachRestore(GRPreviewInitMessage msg);

        /// <summary>根据JobId取到SecurityProfileInfo </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [OperationContract]
        List<RestoreSecurityProfileInfo> GetSecurityProfileInfoByJobId(string jobId);

        /// <summary> 验证CustomAction操作 </summary>
        [OperationContract]
        RestoreCustomActionInfo VerifyCustomActionByJobId(string jobId);

        /// <summary>
        /// LandingPage中获取不同状态Job备份的数量
        /// </summary>
        [OperationContract]
        List<GranularRestoreJobDto> GetRestoreJobsByTime(long startTime, long endTime, string timezoneId);

        [OperationContract]
        Dictionary<int, int> GetJobStatesCount(DashBoardParamDto param);

        [OperationContract]
        GranularLocateTreeDto GetSearchResultBySearchJobId(GranularLocateJobDto locateJob);
    }
}
