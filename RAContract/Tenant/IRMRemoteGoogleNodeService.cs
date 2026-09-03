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
using System.Threading.Tasks;
using AvePoint.RA.Contract.Object;
using AvePoint.GCommon.Contract.Tree;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.RA.Contract.Tenant;

public interface IRMRemoteGoogleNodeService
{
    Task<RMSampleGoogleTreeNode> GetContainersAsync(RMSampleGoogleTreeNode node, bool checkPermission);
    
    Task<RMSampleGoogleTreeNode> GetContainersForRuleAsync(RMSampleGoogleTreeNode node, bool checkPermission, params NodeLevel[] nodeLevels);

    Task<RMSampleGoogleTreeNode> GetDrivesAsync(RMSampleGoogleTreeNode node, bool checkPermission);
    
    Task<RMSampleGoogleTreeNode> GetDrivesForRuleAsync(RMSampleGoogleTreeNode node, bool checkPermission);

    Task<RMSampleGoogleTreeNode> GetContainersForSearchAsync(RMSampleGoogleTreeNode node, bool checkPermission);

    System.Threading.Tasks.Task LoadGoogleSettingIconAsync(List<RMSampleGoogleTreeNode> nodes);

    List<RMSampleGoogleTreeNode> LoadGoogleDriveRoot();

    Task<List<RMGoogleTreeNode>> BrowserRMTreeAsync(RMGoogleTreeNode parent, bool needCheckPermission = false);
    List<GoogleDriveTreeNodeDto> BrowserTreeAsync(GoogleDriveTreeNodeDto parent);

    Task<RMSampleGoogleTreeNode> GetRemoteNodeByDriveIdAsync(string id);

    #region Google One
    System.Threading.Tasks.Task LoadGoogleSampleSettingsAsync(List<RMSampleGoogleTreeNode> nodes);
    Task<RMSampleGoogleTreeNode> BrowseGoogleNodesByPagerAsync(RMSampleGoogleTreeNode node, bool checkPermission);
    #endregion
}