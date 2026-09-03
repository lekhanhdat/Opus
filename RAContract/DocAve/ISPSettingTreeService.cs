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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Object;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.DocAve
{
    /// <summary>
    /// 从DocAve 6获取Tree节点信息
    /// </summary>
    public interface ISPSettingTreeService
    {
        List<RMSPTreeNode> LoadFarm();
        SPTreeMessage Browse(SPTreeNodeDto currentNode, RMBrowseTreeNodeSourceType type);
        Task<List<RMSPTreeNode>> BrowseAsync(RMSPTreeNode parent, bool needCheckPermission = false, RMBrowseTreeNodeSourceType type = RMBrowseTreeNodeSourceType.SharepointOnline);
        Task<bool> ValidateGlobalStorageSettingAsync();
        object GetPhysicalInfos();

        List<RMSPSampleTreeNode> LoadFarmSampleTree();
        Task<List<RMSPSampleTreeNode>> BrowseSampleTreeAsync(RMSPSampleTreeNode parent, bool needCheckPermission = false, RMBrowseTreeNodeSourceType type = RMBrowseTreeNodeSourceType.SharepointOnline, bool needI18N = true, bool loadOrphanedOD = true);
        List<SPTreeNodeDto> BrowseSPTreeNode(SPTreeNodeDto parent, RMBrowseTreeNodeSourceType type = RMBrowseTreeNodeSourceType.SharepointOnline);
        /// <summary>
        /// Browse for GUI 
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        Task<List<RMSampleEXOTreeNode>> BrowseSampleExchangeTreeAsync(RMSampleEXOTreeNode parent, bool needCheckPermission = false);
        List<ExchangeOnlineTreeNodeDto> BrowseExchangeTreeNode(ExchangeOnlineTreeNodeDto parent);
        List<RMEXOTreeNode> BrowseExchangeTree(RMEXOTreeNode parent);
        List<RMSampleEXOTreeNode> LoadExchangeRoot();

        void TransChildrenNodeName(RMSPSampleTreeNode node);
    }
}
