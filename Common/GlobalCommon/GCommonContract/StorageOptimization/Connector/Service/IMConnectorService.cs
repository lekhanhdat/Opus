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
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.SolutionManager.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Connector.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Connector.Object.OperationResult;
using AvePoint.GCommon.Contract.StorageOptimization.Connector.Object.Settings;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Connector.Service
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMConnectorService
    {
        [OperationContract]
        GetInheritedPathResult GetInheritedPath(SPTreeNodeDto listNode);

        [OperationContract]
        AgentOperationResult SaveSiteSetting(SPTreeNodeDto listNode, PhysicalDeviceDto physicalDeviceDto);

        [OperationContract]
        AgentOperationResult ConfigConnectorList(SPTreeNodeDto listNode, ListOperation operation);

        [OperationContract]
        CheckLicenseResult CheckLicenseStatus(ServiceDto agentInfo);

        [OperationContract]
        RuleOperationResult LoadManagedPath(SPTreeNodeDto checkedNode);

        /// <summary>
        /// 获取自身或最近的父节点的配置信息
        /// </summary>
        /// <param name="checkedNode"></param>
        /// <returns></returns>
        [OperationContract]
        RuleOperationResult LoadConnectorInfo(SPTreeNodeDto checkedNode);

        [OperationContract]
        RuleOperationResult SaveConnectorInfo(SPTreeNodeDto checkedNode,
            IEnumerable<SPTreeNodeDto> listNodes,
            SyncSettingsInfoDto syncSettings,
            List<ConnectorPathAndListSettingDto> pathAndListSettings);

        [OperationContract]
        RuleOperationResult RemoveStorageInfo(SPTreeNodeDto checkedNode);

        [OperationContract]
        RuleOperationResult SavePathInfo(RuleOperationType operationType, SPTreeNodeDto checkedNode, IEnumerable<SPTreeNodeDto> listNodes, List<ConnectorPathAndListSettingDto> pathAndListSettings);

        [OperationContract]
        RuleOperationResult SaveSyncSettings(SyncSettingsInfoDto syncSettings, SPTreeNodeDto checkedNode);

        [OperationContract]
        RuleOperationResult CheckDevice(IEnumerable<PhysicalDeviceDto> physicalDevices, SPTreeNodeDto checkedNode, SPTreeNodeDto parent, string farmId);

        [OperationContract]
        RuleOperationResult GetAllSupportedTemplates();

        [OperationContract]
        bool IsInstalledStubDB(SPTreeNodeDto node);

        [OperationContract]
        BlobProviderType GetBlobProviderTypeByNode(SPTreeNodeDto node);

        //[OperationContract]
        //List<string> LoadAllConnectedListNodeIds(string farmId, NodeLevel childNodeLevel);

        [OperationContract]
        SettingOperationResult LoadSetting(string id);

        [OperationContract]
        SettingOperationResult SaveSetting(ProfileDto profile);

        [OperationContract]
        SettingOperationResult UpdataSetting(ProfileDto profile);

        [OperationContract]
        SettingOperationResult DeleteSetting(MappingType type, string id);

        [OperationContract]
        SettingOperationResult DeleteSettings(MappingType type, IEnumerable<String> ids);

        [OperationContract]
        SettingOperationResult LoadAllSettingsByType(MappingType type);

        [OperationContract]
        SettingOperationResult LoadAllSettingSummariesByType(MappingType type);

        [OperationContract]
        SettingOperationResult LoadAllSettingSummaries();

        [OperationContract]
        bool IsProfileInUseByType(MappingType type, IEnumerable<string> profileIds);

        [OperationContract]
        FeatureOperationResult FeatureProcess(SPTreeNodeDto node, ConnectorLibType libraryType, FeatureOperation operation);

        [OperationContract]
        List<MapStoragePathDto> LoadStorageInfo(SPTreeNodeDto checkedNode);

        [OperationContract]
        SettingOperationResult CheckSchedule(ScheduleDto schedule);

        [OperationContract]
        RuleOperationResult RemoveSyncSetting(SPTreeNodeDto checkedNode);

        [OperationContract]
        List<SPRoleDefinitionDto> GetSPRoleDefinitions(SPTreeNodeDto webNode);

        [OperationContract]
        bool IsSolutionExist(RequestMessage message);

        /// <summary>
        /// whether deployed conncetor solution or not; and is assembly in GAC or not
        /// </summary>
        /// <param name="checkedNodes"></param>
        /// <returns></returns>
        [OperationContract]
        DataManagerOperationResult CheckSolutionAndAssembly(List<SPTreeNodeDto> checkedNodes);

        [OperationContract]
        bool IsAssemblyInGAC(RequestMessage message);
    }
}


