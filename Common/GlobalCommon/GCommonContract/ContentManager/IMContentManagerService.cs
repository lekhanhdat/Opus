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


namespace AvePoint.GCommon.Contract.ContentManager
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using AvePoint.Common;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.ContentManager.Object;
    using AvePoint.GCommon.Contract.Migration.Object.OperationResults;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Server.Common.ExportLocation.Object;
    using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
    using AvePoint.GCommon.Contract.Tree.Object;

    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMContentManagerService
    {
        #region Plan 的 增删改查

        /// <summary>
        /// 创建plan
        /// </summary>
        /// <param name="plan"></param>
        /// <returns>planID</returns>
        [OperationContract]
        String CreatePlan(CMPlan plan);

        /// <summary>
        /// 根据planID获得一个plan对象
        /// </summary>
        /// <param name="planId"></param>
        /// <returns></returns>
        [OperationContract]
        CMPlan GetPlan(string planId);

        /// <summary>
        /// 根据planType，load plan
        /// </summary>
        /// <param name="types"></param>
        /// <returns>从数据库中load出的plan集合</returns>
        [OperationContract]
        IList<SimpleDataDto> LoadPlans(int[] types);


        /// <summary>
        /// 根据planName/planType，load plan
        /// </summary>
        /// <param name="types"></param>
        /// <returns>从数据库中load出的plan</returns>
        [OperationContract]
        IList<CMPlan> LoadPlansByName(string name, int[] types);

        /// <summary>
        /// 更新plan
        /// </summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        [OperationContract]
        int UpdatePlan(CMPlan plan);

        /// <summary>
        /// 删除plan
        /// </summary>
        /// <param name="planIDs"></param>
        /// <returns>删除plan的记录数</returns>
        [OperationContract]
        CommonDetailInfoDto DeletePlans(List<CMPlan> plans);

        /// <summary>
        /// Used for GAO
        /// </summary>
        /// <param name="planId"></param>
        /// <returns></returns>
        [OperationContract]
        void DeletePlan(string planId);

        #endregion

        [OperationContract]
        PlanGroupLoadResult GetAllPlanGroups();

        #region 业务方法

        /// <summary>
        /// run job 方法 
        /// 页面点击run方法
        /// </summary>
        /// <param name="plan"></param>
        /// <returns>string : job id</returns>
        [OperationContract]
        int RunOnce(CMPlan plan);

        /// <summary>
        /// 查看相同PlanType下是否存在同名Plan
        /// </summary>
        /// <param name="planName"></param>
        /// <param name="planType"></param>
        /// <returns>true plan名字已经存在</returns>
        [OperationContract]
        bool CheckPlanNameExist(string planName);

        /// <summary>
        /// 获取默认的Content Manager Default Settings 页面的设置
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        CMDefaultSettings GetDefaultSettings();

        /// <summary>
        /// 修改Content Manager Default Settings
        /// 此方法同下面的方法功能一样，只为区分copy和move，为了audit中可以正常获取到当前修改的究竟为哪个
        /// </summary>
        /// <param name="defaultSettings"></param>
        /// <returns></returns>
        [OperationContract]
        int ModifyMoveDefaultSettings(CMDefaultSettings defaultSettings);

        [OperationContract]
        int ModifyCopyDefaultSettings(CMDefaultSettings defaultSettings);

        /// <summary>
        /// 处理GUI刚进入页面后发给server的Get请求
        /// </summary>
        /// <param name="requestType"></param>
        /// <returns></returns>
        [OperationContract]
        CMResponse GetInitData(CMRequestType requestType);

        [OperationContract]
        void RollbackFromContentManager(CMRestoreInfo restoreInfo);

        #endregion
        [OperationContract]
        IList<ExportLocationDto> GetLocationByFarmId(string farmId);
        [OperationContract]
        AveTreeMessage GetNodeItems(AveTreeMessage message);

        /// <summary>
        ///  初始化Preview tree
        ///  需要把源端tree和目的端的tree发给agent，他们根据升降级规则返回一颗preview tree
        ///  源端tree带虚拟节点，目的端tree不带
        /// </summary>
        /// <param name="dto">preview 功能所有的参数</param>
        /// <returns></returns>
        [OperationContract]
        SPTreeNodeDto InitPreviewTree(PreviewTreeParamDto dto);

        [OperationContract]
        List<SPTreeNodeDto> BrowsePreviewTree(FarmDto farm, SPTreeNodeDto node, ActionType action);

        /// <summary> 
        /// check schedule 开始时间，如果比当前utc时间早则不能保存plan检测开始时间
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        bool CheckScheduleStartTime(long startTime, string timeZoneId);

        /// <summary>
        /// 检测name ,schedule
        /// </summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        [OperationContract]
        PlanValidateResult ValidatePlanInfo(CMPlan plan);

        /// <summary>
        /// 获取running的plan集合
        /// </summary>
        /// <param name="planIds"></param>
        /// <returns></returns>
        [OperationContract]
        List<CMPlan> GetRunningPlans(List<CMPlan> planList);

        /// <summary>
        /// check当前plan是否有running的job
        /// </summary>
        /// <param name="planId"></param>
        /// <returns></returns>
        [OperationContract]
        bool CheckRunningPlan(string planId);

        /// <summary>
        /// GetJobStatesCount
        /// For Landing Page
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [OperationContract]
        Dictionary<JobState, int> GetJobStatesCount(CMDashBoardParamDto param);

        /// <summary>
        /// GetSchedules
        /// For Landing Page
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [OperationContract]
        List<ScheduleDto> GetSchedules(CMDashBoardParamDto param);

        /// <summary>
        /// 获取所有Export，Import的Plan
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<CMDashBoardLocationDto> GetAllExportLocations();

        [OperationContract]
        void EditPermission(PlanDto plan, List<string> siteCollectionIds);

        [OperationContract]
        int CheckPlanNeedShareSiteCollections(CMPlan plan, List<string> newSiteCollectionIds);
    }
}
