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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Object;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.DocAve
{
    public interface ITeamsSettingTreeService
    {
        List<RMSPTreeNode> LoadFarm();

        void TransChildrenNodeName(RMSPSampleTreeNode node);

        SPTreeMessage Browse(SPTreeNodeDto currentNode);
        List<SPTreeNodeDto> BrowseTeamsTreeNode(SPTreeNodeDto parent);
        Task<List<RemoteSiteCollection>> GetTeamsUnderContainer(string containerSPObjectId, List<string> teamNames, bool browseInherit = false);
        Task<List<RMSPTreeNode>> BrowseAsync(RMSPTreeNode parent, bool needCheckPermission = false,bool browseInherit = false, bool needChannel = false);
        Task<List<RMSPSampleTreeNode>> BrowseSampleTreeAsync(RMSPSampleTreeNode parent, bool needCheckPermission = false, bool needI18N = true, bool loadOrphanedOD = true);
        List<RMSPSampleTreeNode> LoadFarmSampleTree();

        Task<List<RMSPTreeNode>> BrowseDirectSitesByTeamNode(SPTreeNodeDto teamNode);
        void TransChildrenNodeName(SearchSiteCollectionLazyLoadResponse response);
    }
}
