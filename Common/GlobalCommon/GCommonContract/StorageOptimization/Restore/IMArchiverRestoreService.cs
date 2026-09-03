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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Restore
{
    /// <summary>
    /// Archiver Restore
    /// </summary>
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMArchiverRestoreService 
    {
        /// <summary>
        /// tree: 通过GetSelectedEntireTree获取的Tree节点， 返回值：长度固定为9的int类型数组
        /// </summary>
        [OperationContract]
        Dictionary<SPObjectLevel, MediaRestoreStatistics> GetNodeCalculateSummary(SOPlan plan);
        /// <summary>
        /// plan: 前台收集的需要存储的信息， 返回值：planId.
        /// </summary>
        [OperationContract]
        SOReturnMessage SaveRestorePlan(SOPlan plan);

        /// <summary>
        /// 验证out place的路径是否合理
        /// </summary>
        [OperationContract]
        List<string> ValidateFSDestPathInfo(PhysicalDeviceDto dto);

        /// <summary>
        /// GUI点击AdvanceSearch按钮时调用的方法,  返回值：过滤以后重组的tree.
        /// </summary>
        [OperationContract]
        SPTreeNodeDto GetSearchTreeResult(BackupDataSearchContract searchContract);

        /// <summary>
        /// 获取当前节点下的crawl profile信息.
        /// </summary>
        /// <param name="treeList"></param>
        /// <returns></returns>
        List<StoragePolicyDto> GetCrawlProfileList(List<SPTreeNodeDto> treeList);

        /// <summary>
        /// GUI分页查询
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        //[OperationContract]
        //FullTextIndexQueryResults Query(FullTextIndexQuery query);

        /// <summary>
        /// 验证Security Profile是否存在，不存在则返回错误信息。
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        [OperationContract]
        string ValidateSecurityProfile(List<SPTreeNodeDto> nodes);

        /// <summary>
        /// 用来保存crawl index export plan信息.
        /// </summary>
        /// <param name="crawlIndexExportPlan"></param>
        /// <returns></returns>
        [OperationContract] 
        SOReturnMessage SaveCrawlIndexExportPlan(SOPlan crawlIndexExportPlan);

        /// <summary>
        /// 检查是否应用过crawl profile.
        /// </summary>
        /// <returns></returns>
        [OperationContract] 
        bool IsAppliedCrawlPorfile();

        [OperationContract]
        List<RestoreSecurityInfoWrapper> GetRestoreSecurityProfileList(List<string> subJobIdList);
        [OperationContract]
        SOMessageType SaveEndUserRestoreSetting(EndUserRestoreSetting setting);

        [OperationContract]
        EndUserRestoreSetting GetEndUserRestoreSetting();
    }
}
