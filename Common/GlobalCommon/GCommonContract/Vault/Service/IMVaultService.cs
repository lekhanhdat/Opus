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



using System.Collections.Generic;
using System.ServiceModel;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Contract.Vault.Object;

namespace AvePoint.GCommon.Contract.Vault.Service
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMVaultService
    {
        #region Processing Pool
        [OperationContract]
        ProPoolOperationResult CreateProcessingPool(ProcessingPoolDto pool);

        [OperationContract]
        ProcessingPoolDto GetProcessingPool(string Id);

        [OperationContract]
        ProPoolOperationResult Deletes(List<string> ids);

        [OperationContract]
        ProPoolOperationResult Delete(string id);

        [OperationContract]
        List<ProcessingPoolDto> GetAllProcessingPool();

        [OperationContract]
        ProPoolOperationResult UpdateProcessingPool(ProcessingPoolDto pool);

        [OperationContract]
        IList<FarmDto> GetFarmFromAgent();

        [OperationContract]
        List<ServiceGroupDto> GetAllAgentGroup(string farmId);

        [OperationContract]
        List<ProcessingPoolDto> GetAllProcessingPoolsByFarm(string farmId);

        [OperationContract]
        ProPoolOperationResult IsUsedByPlan(List<string> poolIds);
        #endregion

        #region Profile Manager
        [OperationContract]
        List<VaultProfileDto> GetProfileByFarmId(string farmId);

        [OperationContract]
        ProfileOperationResult CreateProfile(VaultProfileDto profile);

        [OperationContract]
        VaultProfileDto GetProfileByName(string profileName);

        [OperationContract]
        List<VaultProfileDto> GetAllProfile();

        [OperationContract]
        VaultProfileDto GetProfileById(string profileId);

        [OperationContract]
        ProfileOperationResult EditProfile(VaultProfileDto profile);

        [OperationContract]
        SOReturnMessage DeleteProfile(List<VaultProfileDto> profiles);

        [OperationContract]
        ProfileOperationResult ProfileIsUsed(List<string> profileIds);
        #endregion

        #region == Common ==

        /// <summary>
        /// 根据PhysicalDeviceType.FS=0取得net share类型的logical device.
        /// </summary>
        [OperationContract]
        List<LogicalDeviceDto> GetAllLogicalDevices();

        /// <summary>
        /// 
        /// </summary>
        [OperationContract]
        List<StoragePolicyDto> GetAllStoragePolicyByRetentionType(int dataType, StoragePolicyType retentionType);

        [OperationContract]
        List<StoragePolicyDto> GetAllStoragePolicy();
        #endregion

        #region Node Setting Info 
        [OperationContract]
        VaultNodeInfoResponse GetNodeSettingInfo(SPTreeNodeDto node);

        /// <summary>
        /// 建立节点与Profile的关系
        /// </summary>
        /// <param name="vaultNodeInfoRequest">
        /// 成功之后，需要返回给GUI当前节点的状态，有可能父节点有改动，状态与GUI当时状态不一致，需要后台处理
        /// </param>
        /// <returns></returns>
        [OperationContract]
        VaultNodeInfoResponse Apply(VaultNodeInfoRequest vaultNodeInfoRequest);

        [OperationContract]
        VaultNodeInfoResponse CmdletApply(CmdletRequest cmdletRequest); 

        /// <summary>
        /// 建立节点与Profile的关系
        /// </summary>
        /// <param name="vaultNodeInfoRequest"></param>
        /// <returns></returns>
        [OperationContract]
        VaultNodeInfoResponse Retract(SPTreeNodeDto node);

        /// <summary>
        /// 删除节点与Profile的关系
        /// </summary>
        /// <param name="vaultNodeInfoRequest">
        /// 操作成功之后，需要返回给GUI当前节点的状态，有可能父节点有改动，状态与GUI当时状态不一致，需要后台处理
        /// </param>
        /// <returns></returns>
        [OperationContract]
        VaultNodeInfoResponse Remove(SPTreeNodeDto node);

        /// <summary>
        /// 继承
        /// </summary>
        /// <param name="vaultNodeInfoRequest">
        /// 操作成功之后，需要返回给GUI当前节点的状态，有可能父节点有改动，状态与GUI当时状态不一致，需要后台处理
        /// </param>
        /// <returns></returns>
        [OperationContract]
        VaultNodeInfoResponse Inherit(SPTreeNodeDto node);

        /// <summary>
        /// 打破继承
        /// </summary>
        /// <param name="vaultNodeInfoRequest">
        /// 操作成功之后，需要返回给GUI当前节点的状态，有可能父节点有改动，状态与GUI当时状态不一致，需要后台处理
        /// </param>
        /// <returns></returns>
        [OperationContract]
        VaultNodeInfoResponse StopInherit(SPTreeNodeDto node);

        /// <summary>
        /// 对应GUI页面点击RunNow
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        [OperationContract]
        VaultNodeInfoResponse RunNow(VaultNodeInfoRequest node);
        #endregion

        //#region 提供给agent
        //[OperationContract]
        //bool ScanFileTransfer(TransferMessage message);
        //#endregion
    }
}
