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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{

    public interface IRMScopeRoleAssignmentDao: IBaseDao<RMScopeRoleAssignment>
    {
        
        void CreateOrUpdateScopePermission(int groupId, Dictionary<int, List<Guid>> scopePermissions);

        void AddScopePermission(int groupId, List<Guid> scopeIds, SourceFlag source);

        void RemoveAllPermisionsByDataSource(int groupId, List<int> dataSource);
        //void RemoveScopePermission(int groupId, List<Guid> scopeids);
        List<Guid> GetAllContainersByGroupDataSource(List<int> groupIds, int dataSource);
        Task<Dictionary<int,List<Guid>>> GetAllContainersByUsersAsync(List<string> user);
        List<Guid> GetContainersByUsers(List<string> users, SourceFlag sourceType = SourceFlag.All);
        List<int> GetSourceFlagsByUser(List<string> users);
        bool HavePermissionOnContainerId(Guid containerId, List<string> user);
        //validate user have permision on all containers
        bool ValidateContainerIdPermission(List<string> containerIds, List<string> user);
        IList<SourceScopeId> QueryAllScopes();

        /// <summary>
        /// remove containers from all of the groups
        /// </summary>
        /// <param name="scopeIds"></param>
        /// <returns></returns>
        int RemoveContainers(List<Guid> scopeIds);
        void RemoveContainers(List<RMScopeRoleAssignment> scopeRoleAssignments);
        List<int> GetAllGroupsByContainerId(List<Guid> containerIds, int dataSource);
        Dictionary<Guid, IGrouping<Guid, RMScopeRoleAssignment>> GetAllScopeRoleByContainerId(List<Guid> containerIds, int dataSource);
        List<RMScopeRoleAssignment> GetAllScopeRoleByContainerIds(List<Guid> containerIds);
        List<string> GetUserIdsByScopeIds(List<Guid> scopeIds, int dataSource);

    }
}
