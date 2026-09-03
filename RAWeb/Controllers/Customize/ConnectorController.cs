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
using AvePoint.RA.Contract.CustomizeConnector;
using AvePoint.RA.Contract.CustomizeConnector.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters.CustomizeConnector;
using AvePoint.RA.Web.Common.WIF;
using AvePoint.Wrapper.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Exchange.WebServices.Data;
using RazorEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace AvePoint.RA.Web.Controllers.Customize
{
    [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, preferred: false)]
    public class ConnectorController : BaseApiController
    {

        private IRMCustomizeConnectorService _CustomizeConnectorService;

        private IRMCustomizeConnectorService CustomizeConnectorService => PlatformWindsorManager.GetService(ref _CustomizeConnectorService);

        private IExplorerService _ExplorerService;

        private IExplorerService ExplorerService => PlatformWindsorManager.GetService(ref _ExplorerService);

        [HttpPost]
        [ValidateCustomizeConnectorActionFilter(CustomizeConnectorAction.Add)]
        public async System.Threading.Tasks.Task<CustomizeConnectorActionResult> Add([FromBody] CustomizeConnectorInfo connectorInfo)
        {
            if ((await CustomizeConnectorService.GetSimpleInfoByNameAsync(connectorInfo.Name)) != null)
            {
                return CustomizeConnectorActionResult.Result(ActionResultStatus.Repeat);
            }

            return await CustomizeConnectorService.AddAsync(connectorInfo);
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<IEnumerable<CustomizeConnectorInfo>> GetAll()
        {
            return await CustomizeConnectorService.GetAllAsync();
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<CustomizeConnectorInfo> Get([FromBody] Guid id)
        {
            return await CustomizeConnectorService.GetAsync(id);
        }

        [HttpPost]
        public async System.Threading.Tasks.Task Delete([FromBody] List<Guid> ids)
        {
            await CustomizeConnectorService.DeleteAsync(ids);
        }

        [HttpPost]
        [ValidateCustomizeConnectorActionFilter(CustomizeConnectorAction.Update)]
        public async System.Threading.Tasks.Task<CustomizeConnectorActionResult> Update([FromBody] CustomizeConnectorInfo connectorInfo)
        {
            var repeatNameConnector = await CustomizeConnectorService.GetSimpleInfoByNameAsync(connectorInfo.Name);
            if (repeatNameConnector != null && repeatNameConnector.Id != connectorInfo.Id)
            {
                return CustomizeConnectorActionResult.Result(ActionResultStatus.Repeat);
            }
            return await CustomizeConnectorService.UpdateAsync(connectorInfo);
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<List<CustomizeConnectorNameValue<string>>> ViewItemDetailForExplorerSearch([FromBody] Guid id)
        {
            return await CustomizeConnectorService.ViewItemDetailForExplorerSearchAsync(id);
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> DownloadJsonScheme([FromForm] Guid id)
        {
            var jsonScheme = await CustomizeConnectorService.GenerateJsonSchemeAsync(id);
            var bytes = System.Text.Encoding.UTF8.GetBytes(jsonScheme.Item2);
            var stream = new MemoryStream(bytes);
            return new FileStreamResult(stream, "application/octet-stream")
            {
                FileDownloadName = $"{jsonScheme.Item1}.json"
            };
        }

        [HttpPost]
        public void RunConnectorChangeRuleTimer()
        {
            ExplorerService.RunConnectorTimerJob(JobRunBy.Control);
        }
    }
}
