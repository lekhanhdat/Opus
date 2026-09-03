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
using AvePoint.RA.Contract.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IPermissionManagementService
    {
        Task<RAReturnMessage> SaveLocationPermissionAsync(ScopePermissionDto dto);
        ScopePermissionDto ConvertToScopePermissionDto(ScopePermissionSimpleDto simpleDto);
        List<string> GetlocationPathsCanBeViewed(List<int> userAndGroupIds);
        Task<List<AOSUserDto>> GetUsersWithPermissionAsync(string scopeId);
        List<int> GetUserIdsWithPermission(string scopeId);
        List<int> GetScopePermissionIds(List<int> accountAndGroupIds);
        List<int> GetExcludeScopePermissionIds(string scopeId, List<int> accountAndGroupIds);
        List<int> GetIncludeScopePermissionIds(string scopeId, List<int> accountAndGroupIds);
        int GetScopePermissionId(string scopeId);
        bool HasCurrentScopePermission(string scopePath, List<int> accountAndGroupIds);
        Dictionary<string, bool> GetScopeBreakInherMapping(List<string> scopeIds);
        string RealRunSetPermissionJob(JobRunBy JobRunType, string param);
        Task<UsersAndBreakInheritStatus> GetBreakOrInheritPermissionAsync(string scopeId, bool includeSelf);
        string GetScopeIdFullPath(string scopeId);
        ScopePermissionDto ConvertToScopePermissionDto(PhysicalObjectDto obj);
        Task<RAReturnMessage> SyncADUsersAsync(List<AOSUserDto> users);
        Task<RAReturnMessage> SavePermissionForNewPhysicalAsync(ScopePermissionDto dto, PhysicalObjectDto obj);
        void DeletePermissionInfo(string scopeId);
        string GetScopeIdFullPath(PhysicalObjectDto node);
        int GetScopePermissionId(string scopeIdPath, bool includeSelf);
        RAReturnMessage RunSetPermissionJob(ScopePermissionJobContextDto dto);
        List<int> GetExcludeScopePermissionIdsForSearch(string scopePath, List<int> accountAndGroupIds);
        List<int> GetIncludeScopePermissionIdsForSearch(string scopePath, List<int> accountAndGroupIds);
    }
}
