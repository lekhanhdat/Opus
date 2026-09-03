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


namespace AvePoint.GCommon.Contract.CentralAdmin
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using AvePoint.Adonis.CentralAdmin.Object;
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using AvePoint.GCommon.Contract.CentralAdmin.Object.SecurityCenter;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Action;
    using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
    using AvePoint.GCommon.Contract.Server.Job.Object;
    using AvePoint.GCommon.Contract.Tree.Object;
    using Server.Common;

    #endregion using directives

    /// <summary>
    /// CA 提供给GUI的所有服务
    /// </summary>
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMCentralAdminService
    {
        /// <summary>
        /// Main message handler, the class directly implement the interface has the central control of the message
        /// </summary>
        /// <param name="messageList">the message passed from the GUI</param>
        /// <returns>the handled message of the Agent</returns>
        [OperationContract]
        List<CAMessage> HandleMessage(List<CAMessage> messageList);

        [OperationContract]
        string HandlePlan(CAPlan plan, bool isSchedule);

        #region Plan

        [OperationContract]
        String CreatePlan(CAPlan plan, bool isRunNow);

        [OperationContract]
        CAPlan GetPlan(string planId);

        [OperationContract]
        CAPlan GetPlanByJobID(string jobId);

        [OperationContract]
        IList<CAPlan> GetPlanByName(string planName);

        [OperationContract]
        IList<CAPlan> LoadPlan(int type);

        [OperationContract]
        List<CAPlanModel> LoadAllPlans(int[] types);

        [OperationContract]
        int UpdatePlan(CAPlan plan, bool isRunNow);

        [OperationContract]
        bool CheckPermissionBeforeUpdatePlan(List<ObjectPermissionDto> permissions);

        [OperationContract]
        string CreatePlanForAPI(CAPlan plan, bool isRunNow);

        [OperationContract]
        int DeletePlan(string planID);

        [OperationContract]
        int DeletePlans(String[] planIDs);

        [OperationContract]
        bool CheckProfileNameExist(string profileName, ProfileType type);

        [OperationContract]
        bool CheckPlanNameExist(string planName);

        /// <summary>
        /// 返回Plan的Running状态
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        Dictionary<string, bool> GetPlanRunningStatus(string[] planIDs);

        #endregion Plan

        #region Job

        [OperationContract]
        CAJobDto GetLatestRunningJob(string planId);

        [OperationContract]
        CASearchResultNodeInfosWrapper GetJobResult(string jobID, int pageNum);

        #region dead account

        [OperationContract]
        OnlineDeadAccountResult GetDeadAccountJobResultByJobID(string jobID, string searchText, int from, int to);

        #endregion dead account

        [OperationContract]
        CAAdminSearchResultInfosWrapper GetAdminSearchJobResult(string jobId);

        [OperationContract]
        List<CASearchResultNodeInfo> GetSecuritySearchJobResult(string jobId, int start, int offset);

        [OperationContract]
        List<CASearchResultNodeInfo> GetAllSecuritySearchJobResult(string jobId);

        [OperationContract]
        CAAdminSearchResultInfosWrapper GetAdminSearchAllLowestLevelResult(string jobId);

        #region import configurationfile

        [OperationContract]
        CASecurityImportPermissionsOperation GetImportSCFileJobResultWithPerpage(string jobID, int pageNum, int perPage);

        [OperationContract]
        CAImportResultStatistics GetImportGlobalJobInfo(string jobID);

        #endregion import configurationfile

        #region search web part

        [OperationContract]
        List<WebPartInstanceInfo> GetWebPartsJobResult(string jobID);

        [OperationContract]
        List<WebPartTemplate> GetWebPartTemplateJobResult(string jobID);

        #endregion search web part

        [OperationContract]
        CAJobDto CheckJobStatus(string jobID);

        #endregion Job

        #region Util

        [OperationContract]
        List<SPTreeNodeDto> BrowseRemoteSiteGroups(SPTreeNodeDto registeredFarm);

        [OperationContract]
        List<SPTreeNodeDto> BrowseRemoteSiteCollectionsByIds(string[] ids);

        [OperationContract]
        List<SPTreeNodeDto> BrowseRemoteSiteGroupsByGroupIds(SPTreeNodeDto registeredFarm, List<string> groupIds);

        [OperationContract]
        CASecurityImportPermissionsOperation GetRecordsFromImportFiles(byte[] fileBytes, ReportFileType type);

        [OperationContract]
        CASecurityEditGroupsOperation GetEditGroupRecordsFromImportFiles(byte[] filData, ReportFileType type);

        #endregion Util

        #region download

        [OperationContract]
        string GetSecuritySearchDownloadReportTempFilePath(string jobId, bool isExportForEditing, ReportFileType type);

        [OperationContract]
        string GetAdminSearchDownloadReportTempFilePath(string jobId, ReportFileType type);

        #endregion download

        #region CA Profile

        [OperationContract]
        List<CADocAveNodePolicyDto> GetPagedNodePolicyMapping(string searchKey, List<ColumnOrder> ColumnOrders, int index, int size,bool hasFetch);

        [OperationContract]
        List<CADocAveNodePolicyDto> GetPagedNodePolicyMappingForContainer(string searchKey, List<ColumnOrder> ColumnOrders, int index, int size, bool hasFetch, int indexForContainer);

        [OperationContract]
        int GetNodePolicyMappingTotalCount(string searchKey);

        [OperationContract]
        int GetNodePolicyMappingTotalCountForContainer(string searchKey);

        [OperationContract]
        List<ProfileInheritInfo> GetPolicyIdsBySelectedNodes(List<SPTreeNodeDto> selectedNodes);

        [OperationContract]
        List<AdministratorProfileInfo> GetCAProfilesById(string[] ids);

        [OperationContract]
        List<ScheduleTemplateDto> GetAllAdminProfileSchedule(bool authorised = true);

        [OperationContract]
        string AddAndUpdateAdminProfileSchedule(ScheduleTemplateDto schedule);

        [OperationContract]
        bool ScheduleTemplateOperation(string[] templateIds, ScheduleTemplateAction action);

        [OperationContract]
        List<string> GetNotAppliedScheduleIds(List<string> templateIds);

        /// <summary>
        /// 删除ScheduleTemplate之后需要更新Profile以及Profile对应的Plan
        /// </summary>
        /// <param name="id"></param>
        [OperationContract]
        void DeleteAdminProfileSchedule(string[] ids);

        [OperationContract]
        IList<CAPlan> CheckReuseProfilePlan(AdministratorProfileInfo profileInfo, List<SPTreeNodeDto> applyNodes);

        [OperationContract]
        AdministratorProfileJobOperation GetCAProfileAndRule(AdministratorProfileJobOperation operation);

        /// <summary>
        /// 此方法用于JobCleanService中用于删除PE Job的Data
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        [OperationContract]
        bool DeleteCAPEJobDataByIds(string[] Ids);

        [OperationContract]
        List<AdministratorProfileInfo> GetAllProfiles();
        [OperationContract]
        AdministratorProfileInfo GetPEProfileByName(string profileName);

        [OperationContract]
        bool RemoveProfileFromNode(string siteCollectionUrl);
        #endregion CA Profile

        #region access list

        [OperationContract]
        int DeleteAccessList(List<string> ids);

        [OperationContract]
        AccessListForDisplay GetAllAccessLists();

        [OperationContract]
        string CreateOrUpdateAccessList(AccessListDto accessList);

        #endregion access list

        string RunPlan(string planId);

        int GetSecuritySearchFileResultCount(string jobId);
    }
}