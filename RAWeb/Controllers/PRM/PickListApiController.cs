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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.Service.Services.Explorer;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using DocumentFormat.OpenXml.Office.CoverPageProps;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.PRM
{
    [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin, preferred: false)]
    public class PickListApiController : BaseApiController
    {

        private IPickListService _PickListService;
        private IPickListService PickListService => PlatformWindsorManager.GetService(ref _PickListService);
        private IRecordsHistoryService RecordsHistoryService => PlatformWindsorManager.GetService<IRecordsHistoryService>();

        [HttpPost]
        public Task<PickListLoanResultDto> QueryLoanRequest([FromBody] PickListLoanParam param)
        {
            PickListLoanResultDto resultDto = new PickListLoanResultDto();
            resultDto.List = new List<PickListLoanDto>();
            return PickListService.QueryPickLoanListAsync(param);
        }

        [HttpPost]
        public Task<PickListDestructionResultDto> QueryDestruction([FromBody] PickListDestructionParam param)
        {
            PickListDestructionResultDto resultDto = new PickListDestructionResultDto();
            resultDto.List = new List<PickListDestructionDto>();
            return PickListService.QueryPickDestructionListAsync(param);
        }

        [HttpPost]
        [ValidPhysicalPickListActionFilter(ValidPhysicalPickListActionFilter.PICK_LIST_ACTION_FOR_LOAN)]
        public RAReturnMessage LoanRequestCompelte([FromBody] CompleteActionParam param)
        {
            if (param.IsSelectAll || param.IsContainerLevel)
            {
                return PickListService.StartJob(param, PickObjectType.Loan, PickActionType.Complete);
            }
            else
            {
                return PickListService.UpdatePickStatusCompelte(param, PickObjectType.Loan);
            }
        }

        [HttpPost]
        [ValidPhysicalPickListActionFilter(ValidPhysicalPickListActionFilter.PICK_LIST_ACTION_FOR_DESTRUCTION)]
        public RAReturnMessage DestructionCompelte([FromBody] CompleteActionParam param)
        {
            if (param.IsSelectAll || param.IsContainerLevel)
            {
                return PickListService.StartJob(param, PickObjectType.Destruction, PickActionType.Complete);
            }
            else
            {
                return PickListService.UpdatePickStatusCompelte(param, PickObjectType.Destruction);
            }
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public async Task<PhysicalReturnHistoryResponse> GetReturnLoanHistoryData([FromBody] ReturnLoanHistoryParam param)
        {
            return await RecordsHistoryService.GetReturnLoanHistory(param, 1000);
        }

        [HttpPost]
        public RAReturnMessage StartExportLoanJob([FromBody] CompleteActionParam param)
        {
            return PickListService.StartJob(param, PickObjectType.Loan, PickActionType.Export);
        }

        [HttpPost]
        public RAReturnMessage StartExportDestructionJob([FromBody] CompleteActionParam param)
        {
            return PickListService.StartJob(param, PickObjectType.Destruction, PickActionType.Export);
        }

        [HttpPost]
        public RAReturnMessage StartExportReturnHistoryJob([FromBody] CompleteActionParam param)
        {
            return PickListService.StartJob(param, PickObjectType.ReturnHistory, PickActionType.Export);
        }

        #region Pick list move
        [HttpPost]
        public async Task<PickListMoveResultDto> GetMoveRequets([FromBody] PickListMoveParam param)
        {
            return await RecordsHistoryService.GetMoveData(param, 1000);
        }
        [HttpPost]
        public RAReturnMessage StartExportMoveJob([FromBody] PickMoveListParam param)
        {
            return PickListService.StartMoveJob(param, PickActionType.Export);
        }
        #endregion
    }
}
