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
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Web.Common;
using Newtonsoft.Json;
using AvePoint.RA.Web.Common.WIF;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Common;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.PRM
{
    [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser, preferred: false)]
    public class PhysicalRequestApiController : BaseApiController
    {
        private IPhysicalReqeustService _PysicalReqeustService;
        private IPhysicalReqeustService PysicalReqeustService => PlatformWindsorManager.GetService(ref _PysicalReqeustService);
        private ITemplateManagementService _TemplateManagementService;
        private ITemplateManagementService TemplateManagementService => PlatformWindsorManager.GetService(ref _TemplateManagementService);
        private IExplorerService _ExplorerService;
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService(ref _ExplorerService);


        [HttpGet]
        [ValidPhysicalExplorerActionFilter("GetRequest")]
        public async Task<PhysicalRequestDto> GetRequest(int id)
        {
            var req = await PysicalReqeustService.GetRequestAsync(id);
            var fileInfos = req?.PhysicalFileInfos;
            if (req?.Type == PhysicalRequestType.Creation)
            {
                if (fileInfos != null)
                {
                    foreach (var fileInfo in fileInfos)
                    {
                        fileInfo.Template = await TemplateManagementService.LoadTemplateDtoAsync(fileInfo.TemplateId);
                        await ExplorerService.ConvertDateTimeColumnValueTimeZoneAsync(fileInfo);
                    }
                }
                else if (req?.PhysicalFileInfo != null)
                {
                    var fileInfo = req?.PhysicalFileInfo;
                    fileInfo.Template = await TemplateManagementService.LoadTemplateDtoAsync(fileInfo.TemplateId);
                    await ExplorerService.ConvertDateTimeColumnValueTimeZoneAsync(fileInfo);
                }
            }
            return req;
        }

        [HttpPost]
        public Task<PhysicalRequestResult> Query([FromBody] PhysicalRequestParam param)
        {
            return PysicalReqeustService.QueryAsync(param);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public Task<PhysicalRequestResult> Approve([FromBody] PhysicalRequestParam param)
        {
            return PysicalReqeustService.ApproveAsync(param);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public List<PhysicalObjectDto> GetLoanFolderByBoxIds([FromBody] List<Guid> guids)
        {
            return PysicalReqeustService.GetLoanFolderByBoxIds(guids);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public Task<PhysicalRequestResult> Reject([FromBody] PhysicalRequestParam param)
        {
            return PysicalReqeustService.RejectAsync(param);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser)]
        public Task<PhysicalRequestResult> CancelRequest([FromBody] PhysicalRequestParam param)
        {
            return PysicalReqeustService.CancelRequestAsync(param);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public Task<PhysicalRequestResult> Modify([FromBody] PhysicalRequestDto dto)
        {           
            return PysicalReqeustService.UpdateAsync(dto);
        }

        
        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser, RMSubPermissionMasks.PhysicalBoxCreationRequest, DiffPermissionJoinType = DB.SecurityTrimming.Model.PermissionJoinType.And)]
        [ValidPhysicalExplorerActionFilter("ValidateNewRequest")]
        [HttpPost]
        public Task<PhysicalRequestResult> NewBoxRequest([FromBody] PhysicalRequestDto dto)
        {
            return PysicalReqeustService.CreateAsync(dto);
        }

        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser, RMSubPermissionMasks.PhysicalFolderCreationRequest, DiffPermissionJoinType = DB.SecurityTrimming.Model.PermissionJoinType.And)]
        [ValidPhysicalExplorerActionFilter("ValidateNewRequest")]
        [HttpPost]
        public Task<PhysicalRequestResult> NewFolderRequest([FromBody] PhysicalRequestDto dto)
        {
            return PysicalReqeustService.CreateAsync(dto);
        }

        [HttpPost]
        [ValidPhysicalExplorerActionFilter("ValidateNewRequest")]
        public Task<PhysicalRequestResult> NewRecordRequest([FromBody] PhysicalRequestDto dto)
        {
            return PysicalReqeustService.CreateAsync(dto);
        }

        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser, RMSubPermissionMasks.PhysicalFolderLoanRequest, DiffPermissionJoinType = DB.SecurityTrimming.Model.PermissionJoinType.And)]
        [ValidPhysicalExplorerActionFilter("ValidateLoanRequest")]
        [HttpPost]
        public Task<PhysicalRequestResult> LoanRequest([FromBody] LoanRequestDto dto)
        {
            return PysicalReqeustService.LoanRequestAsync(dto);
        }

        [RMApiAuthorize(RMPermissionMasks.PhysicalEndUser, RMSubPermissionMasks.PhysicalMoveRequest, DiffPermissionJoinType = DB.SecurityTrimming.Model.PermissionJoinType.And)]
        [ValidPhysicalExplorerActionFilter("ValidateMoveRequest")]
        [HttpPost]
        public Task<PhysicalRequestResult> MoveRequest([FromBody] MoveRequestDto dto)
        {
            return PysicalReqeustService.MoveRequestAsync(dto);
        }

        [HttpPost]
        public bool CheckItemOnHold([FromBody] List<Guid> ids)
        {
            bool rs = PysicalReqeustService.CheckItemOnHold(ids);
            return rs;
        }
        /*        private async Task AssembleTemplateAsync(List<PhysicalRequestDto> requests)
                {
                    if (requests != null && requests.Count > 0)
                    {
                        var creationList = requests.Where(r => r.Type == PhysicalRequestType.Creation).ToList();
                        await creationList.ConvertAllAsync(async r => 
                        {
                            if (r.PhysicalFileInfo != null)
                            {
                                r.PhysicalFileInfo.Template = await TemplateManagementService.LoadTemplateDtoAsync(r.PhysicalFileInfo.TemplateId);
                            }
                            return r;
                        });
                    }
                }*/


        [HttpPost]
        public string GetFilterInfo()
        {
            return JsonConvert.SerializeObject(PysicalReqeustService.GetFilterDataSource());
        }

        [HttpPost]
        public bool CheckItemOnLoan([FromBody] PhysicalRequestParam param)
        {
            return PysicalReqeustService.CheckItemsOnLoan(param);
        }
    }
}