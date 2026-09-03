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
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.DB.Model;
using System.Data.SqlClient;
using static AvePoint.RA.DB.Dao.GoogleSyncNodeDao.RMGoogleRemoteNodeDao;

namespace AvePoint.RA.DB.Dao.GoogleSyncNodeDao.Contract;

public interface IRMGoogleRemoteNodeDao
{
    Task<List<RMRemoteNode>> GetAllGoogleRemoteNodes();
    Task<RMSampleGoogleTreeNode> GetContainersWithoutCheckPermissionAsync(RMSampleGoogleTreeNode node);
    
    Task<RMSampleGoogleTreeNode> GetContainersWithoutCheckPermissionForRuleAsync(RMSampleGoogleTreeNode node, params NodeLevel[] nodeLevels);

    Task<RMSampleGoogleTreeNode> GetContainersWithCheckPermissionForRuleAsync(RMSampleGoogleTreeNode node, params NodeLevel[] nodeLevels);
    
    Task<RMSampleGoogleTreeNode> GetContainersWithCheckPermissionAsync(RMSampleGoogleTreeNode node);

    Task<RMSampleGoogleTreeNode> GetDrivesWithoutCheckPermissionAsync(RMSampleGoogleTreeNode node);
    
    Task<RMSampleGoogleTreeNode> GetDrivesWithoutCheckPermissionForRuleAsync(RMSampleGoogleTreeNode node);

    Task<RMSampleGoogleTreeNode> GetDrivesWithCheckPermissionAsync(RMSampleGoogleTreeNode node);
    
    Task<RMSampleGoogleTreeNode> GetDrivesWithCheckPermissionForRuleAsync(RMSampleGoogleTreeNode node);

    Task<RMSampleGoogleTreeNode> GetContainersForSearchAsync(RMSampleGoogleTreeNode node, bool checkPermission);

    Task<List<RMRemoteNode>> QueryGoogleNodesForSearchAsync(RMSampleGoogleTreeNode node, (string sql, List<SqlParameter> parameters) queryTuple);

    RMSampleGoogleTreeNode GetGoogleContainerById(string id);

    RMSampleGoogleTreeNode GetGoogleDriveById(string id);
    
    List<RMSampleGoogleTreeNode> GetGoogleDrives(IEnumerable<string> ids);
    
    List<RMSampleGoogleTreeNode> GetGoogleContainers(IEnumerable<string> ids);

    List<RMSampleGoogleTreeNode> GetAllGoogleContainers();

    List<RMSampleGoogleTreeNode> GetGoogleDrivesByParentId(string parentId);

    List<string> GetGoogleTenantIdsUnderContainer(string parentId);

    List<string> GetGoogleTenantIdsUnderNodes(List<string> nodeIds, NodeLevelExpressionType expType);

    Task<List<string>> GetGoogleTenantIdsUnderContainers(List<string> containerIds);

    RMGoogleSetting LoadGoogleSetting(Guid id, Guid driveId);

    List<RMSimpleRule> GetMappingRules(Guid containerId, Guid driveId);

    List<string> GetContainerNames(List<string> nodeIds);

    Task<List<string>> GetPermissionContainerIdsAsync();
}