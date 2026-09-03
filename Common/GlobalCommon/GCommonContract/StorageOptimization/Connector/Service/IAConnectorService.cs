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
using AvePoint.GCommon.Contract.StorageOptimization.Connector.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Connector.Service
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IAConnectorService
    {
        [OperationContract]
        IDictionary<string, ResultMessageType> ValidatePaths(List<ConnectorSettingDto> paths);
        [OperationContract]
        IDictionary<SPTreeNodeDto, ConnectorResult> SaveListSettings(List<ConnectorSettingDto> connectorSettings);
        [OperationContract]
        ConnectorResult RemoveListSetting(SPTreeNodeDto listNode);
        [OperationContract]
        GetListSettingResult GetListSetting(SPTreeNodeDto listNode);
        [OperationContract]
        Dictionary<Guid, ListStorageSettingDto> GetListStorageSettings(List<SPTreeNodeDto> listNodes);

        /// <summary>
        /// Save the mapping settings before SaveGlobalSetting, it should be broadcast to all reliable agents.
        /// </summary>
        [OperationContract]
        ConnectorResult SaveSyncSetting(SyncSetting syncSetting);
        [OperationContract]
        ConnectorResult DeleteSyncSetting(SPTreeNodeDto node);

        [OperationContract]
        List<SPFeatureStatus> FeatureProcess(FeatureMessage message);
        [OperationContract]
        List<SPSolutionStatus> GetSolutionStatus(ConnectorLibType cnctLibType);

        [OperationContract]
        List<SPRoleDefinitionDto> GetSPRoleDefinitions(SPTreeNodeDto webNode);
    }
}
