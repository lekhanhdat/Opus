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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.AzureFileShare;
using AvePoint.RA.Contract.AzureFileShare.Model;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.AzureFileShare
{
    [RMApiAuthorize(RMPermissionExtensionMasks.AzureFSAdmin, preferred: false)]
    public class AzureFileShareConnectionController : BaseApiController
    {
        private IRMAzureFileShareConnectionGroupService _AzureFileShareConnectionGroupService;
        private IRMAzureFileShareConnectionGroupService AzureFileShareConnectionGroupService => PlatformWindsorManager.GetService(ref _AzureFileShareConnectionGroupService);
        private IRMAzureFileShareConnectionService _AzureFileShareConnectionService;
        private IRMAzureFileShareConnectionService AzureFileShareConnectionService => PlatformWindsorManager.GetService(ref _AzureFileShareConnectionService);

        [HttpPost]
        public Task<List<AzureFileShareConnectionGroupItem>> GetGroups()
        {
            return AzureFileShareConnectionGroupService.GetAllAsync();
        }

        [HttpPost]
        public Task<bool> DeleteGroups([FromBody] List<Guid> ids)
        {
            return AzureFileShareConnectionGroupService.RemoveAsync(ids);
        }

        [HttpPost]
        public async Task<AzureFileShareResponse<bool>> UpsertGroup([FromBody] AzureFileShareConnectionGroupItem group)
        {
            var existGroup = await AzureFileShareConnectionGroupService.GetAsync(group.Name);
            if(existGroup != null && existGroup.Id != group.Id)
            {
                return AzureFileShareResponse<bool>.Failed(AzureFileShareResponseErrorType.RepeatName, false);
            }

            if(group.Id != Guid.Empty && AzureFileShareConnectionGroupService.Has(group.Id))
            {
                var modifyRes = await AzureFileShareConnectionGroupService.ModifyAsync(group);
                return AzureFileShareResponse<bool>.Generate(modifyRes, modifyRes);
            }

            var createRes = await AzureFileShareConnectionGroupService.CreateAsync(group);
            return AzureFileShareResponse<bool>.Generate(createRes, createRes);
        }

        [HttpPost]
        public Task<List<AzureFileShareConnectionItem>> GetConnections()
        {
            return AzureFileShareConnectionService.GetAllAsync();
        }

        [HttpPost]
        public Task<bool> DeleteConnections([FromBody] List<Guid> ids)
        {
            return AzureFileShareConnectionService.RemoveAsync(ids);
        }

        [HttpPost]
        public Task<List<AzureFileShareConnectionItem>> GetConnectionsWithoutRelatedGroup()
        {
            return AzureFileShareConnectionService.GetAllWithoutRelatedConnectionGroupAsync();
        }

        [HttpPost]
        public async Task<AzureFileShareResponse<bool>> UpsertConnection([FromBody] AzureFileShareConnectionItem connection)
        {
            var existConn = await AzureFileShareConnectionService.GetAsync(connection.Name);
            if (existConn != null && existConn.Id != connection.Id)
            {
                return AzureFileShareResponse<bool>.Failed(AzureFileShareResponseErrorType.RepeatName, false);
            }

            if (await IsDuplicateConnection(connection))
            {
                return AzureFileShareResponse<bool>.Failed(AzureFileShareResponseErrorType.ValidateError, false);
            }

            if (!AzureFileShareConnectionService.Validate(connection))
            {
                return AzureFileShareResponse<bool>.Failed(AzureFileShareResponseErrorType.ValidateError, false);
            }

            if (connection.Id != Guid.Empty && AzureFileShareConnectionService.Has(connection.Id))
            {
                var modifyRes = await AzureFileShareConnectionService.ModifyAsync(connection);
                return AzureFileShareResponse<bool>.Generate(modifyRes, modifyRes);
            }

            var createResult = await AzureFileShareConnectionService.CreateAsync(connection);
            return AzureFileShareResponse<bool>.Generate(createResult, createResult);
        }

        [HttpPost]
        public async Task<bool> ValidateConnectionInfo([FromBody] AzureFileShareConnectionItem connection)
        {
            if (await IsDuplicateConnection(connection))
            {
                return false;
            }
            return AzureFileShareConnectionService.Validate(connection);
        }

        private async Task<bool> IsDuplicateConnection(AzureFileShareConnectionItem connection)
        {
            var allConnections = await AzureFileShareConnectionService.GetAllAsync();
            return allConnections.Any(c => c.Id != connection.Id
                && c.FileShareName == connection.FileShareName
                && c.AccessEndPoint == connection.AccessEndPoint);
        }
    }
}