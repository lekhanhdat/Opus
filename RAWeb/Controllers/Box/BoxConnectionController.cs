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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.Box.Model;
using AvePoint.RA.Contract.Connections;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Service.Services.Box;
using AvePoint.RA.Service.Services.Box.Converters;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using Microsoft365.SharePoint.Rest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.Box
{
    [RMApiAuthorize(RMPermissionExtensionMasks.BoxAdmin, preferred: false)]
    public class BoxConnectionController : BaseApiController
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(BoxConnectionController));

        private IRMBoxConnectionService _boxConnectionService;
        private IRMBoxConnectionService BoxConnectionService => PlatformWindsorManager.GetService(ref _boxConnectionService);

        private IRMBoxConnectionGroupService _boxConnectionGroupService;
        private IRMBoxConnectionGroupService BoxConnectionGroupService => PlatformWindsorManager.GetService(ref _boxConnectionGroupService);

        [HttpPost]
        public async Task<IEnumerable<BoxConnectionGroupViewModel>> GetAllConnectionGroups()
        {
            var groups = await BoxConnectionGroupService.GetAllAsync();
            var result = groups.ConvertAll(item => item.ConvertToViewModel());
            return result;
        }

        [HttpPost]
        public async Task<ConnectionResponse> DeleteConnectionGroups([FromBody] List<Guid> ids)
        {
            try
            {
                var deleteResult = await BoxConnectionGroupService.RemoveAsync(ids);
                return ConnectionResponse.Generate(deleteResult);
            }
            catch (ArgumentNullException aex)
            {
                logger.Warn($"Failed to delete the connection groups. Error: {aex}");
                return ConnectionResponse.Failed(ConnectionResponseErrorType.Unknown, aex.Message);
            }
            catch (Exception ex)
            {
                logger.Warn($"An unknown error occurred while attempting to delete the connection groups. Error:{ex}");
                return ConnectionResponse.Failed(ConnectionResponseErrorType.Unknown, "Unknown error");
            }
        }

        [HttpPost]
        public async Task<ConnectionResponse> AddConnectionGroup([FromBody] BoxConnectionGroupItem connectionGroup)
        {
            try
            {
                var createResult = await BoxConnectionGroupService.CreateAsync(connectionGroup);
                return ConnectionResponse.Generate(createResult);
            }
            catch (ManageConnectionException mcex)
            {
                logger.Warn($"Failed to add the connection group. Error: {mcex}");
                return mcex.Response;
            }
            catch (ArgumentNullException aex)
            {
                logger.Warn($"Failed to add the connection group. Error: {aex}");
                return ConnectionResponse.Failed(ConnectionResponseErrorType.Unknown, aex.Message);
            }
            catch (Exception ex)
            {
                logger.Warn($"An unknown error occurred while attempting to add the connection group. Error:{ex}");
                return ConnectionResponse.Failed(ConnectionResponseErrorType.Unknown, "Unknown error");
            }
        }

        [HttpPost]
        public async Task<ConnectionResponse> UpdateConnectionGroup([FromBody] BoxConnectionGroupItem connectionGroup)
        {
            try
            {
                var updateResult = await BoxConnectionGroupService.ModifyAsync(connectionGroup);
                return ConnectionResponse.Generate(updateResult);
            }   
            catch (ManageConnectionException mcex)
            {
                logger.Warn($"Failed to update the connection group. Error: {mcex}");
                return mcex.Response;
            }
            catch (ArgumentNullException aex)
            {
                logger.Warn($"Failed to update the connection group. Error: {aex}");
                return ConnectionResponse.Failed(ConnectionResponseErrorType.Unknown, aex.Message);
            }
            catch (Exception ex)
            {
                logger.Warn($"An unknown error occurred while attempting to update the connection group. Error:{ex}");
                return ConnectionResponse.Failed(ConnectionResponseErrorType.Unknown, "Unknown error");
            }
        }

        [HttpPost]
        public async Task<IEnumerable<BoxConnectionViewModel>> GetAllConnections()
        {
            var connections = await BoxConnectionService.GetAllAsync();
            var result= connections.ConvertAll(item => item.ConvertToViewModel());
            return result;
        }

        [HttpPost]
        public async Task<ConnectionResponse> DeleteConnections([FromBody] List<Guid> ids)
        {
            try
            {
                var deleteResult = await BoxConnectionService.RemoveAsync(ids);
                return ConnectionResponse.Generate(deleteResult);
            }
            catch (ArgumentNullException aex)
            {
                logger.Warn($"Error: {aex}");
                return ConnectionResponse.Failed(ConnectionResponseErrorType.Unknown, aex.Message);
            }
            catch (Exception ex)    
            {
                var idList = string.Join(", ", ids);
                logger.Warn($"An unknown error occurred while attempting to delete the connections [{idList}]. Error:{ex}");
                return ConnectionResponse.Failed(ConnectionResponseErrorType.Unknown, "Unknown error");
            }
        }

        [HttpPost]
        public async Task<IEnumerable<BoxConnectionViewModel>> GetConnectionsWithoutRelatedGroup()
        {
            var connections = await BoxConnectionService.GetAllWithoutRelatedConnectionGroupAsync();
            return connections.ConvertAll(item => item.ConvertToViewModel());
        }

        [HttpPost]
        public async Task<ConnectionResponse> AddConnection([FromBody] BoxConnectionItem connection)
        {
            try
            {
                if (!BoxConnectionService.Validate(connection))
                {
                    throw new ManageConnectionException(ConnectionResponseErrorType.ValidationError);
                }

                var createResult = await BoxConnectionService.CreateAsync(connection);
                return ConnectionResponse.Generate(createResult);
            }
            catch (ManageConnectionException mcex)
            {
                logger.Warn($"Failed to add the connection. Error: {mcex}");
                return mcex.Response;
            }
            catch (ArgumentNullException aex)
            {
                logger.Warn($"Failed to add the connection. Error: {aex}");
                return ConnectionResponse.Failed(ConnectionResponseErrorType.Unknown, aex.Message);
            }
            catch (Exception ex)
            {
                logger.Warn($"An unknown error occurred while attempting to add the connection. Error:{ex}");
                return ConnectionResponse.Failed(ConnectionResponseErrorType.Unknown, "Unknown error");
            }
        }

        [HttpPost]
        public async Task<ConnectionResponse> UpdateConnection([FromBody] BoxConnectionItem connectionItem)
        {
            try
            {
                if (!BoxConnectionService.Validate(connectionItem))
                {
                    throw new ManageConnectionException(ConnectionResponseErrorType.ValidationError);
                }
                var updateResult = await BoxConnectionService.ModifyAsync(connectionItem);
                return ConnectionResponse.Generate(updateResult);
            }
            catch (ManageConnectionException mcex)
            {
                logger.Warn($"Failed to update the connection. Error: {mcex}");
                return mcex.Response;
            }
            catch (ArgumentNullException aex)
            {
                logger.Warn($"Failed to update the connection. Error: {aex}");
                return ConnectionResponse.Failed(ConnectionResponseErrorType.Unknown, aex.Message);
            }
            catch (Exception ex)
            {
                logger.Warn($"An unknown error occurred while attempting to update the connection. Error:{ex}");
                return ConnectionResponse.Failed(ConnectionResponseErrorType.Unknown, "Unknown error");
            }
        }

    }
}
