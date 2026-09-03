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
using AvePoint.RA.Contract.ReportCenter;
using AvePoint.RA.Contract.ReportCenter.Model;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;


namespace AvePoint.RA.Web.Controllers.ReportCenter
{
    [RMApiAuthorize(RMPermissionMasks.ReportCenterEnduser, RMReportPermissionMasks.CreationAndDestructionEnduser, preferred: false)]
    public class CreateAndDestryoedReportController : BaseApiController
    {
        private ICreateAndDestryoedReportService _CreateAndDestryoedReportService;
        private ICreateAndDestryoedReportService CreateAndDestryoedReportService => PlatformWindsorManager.GetService(ref _CreateAndDestryoedReportService);

        [HttpPost]
        public async Task<bool> Create([FromBody] CreateAndDestryoedReportModel reportInfo)
        {
            return await CreateAndDestryoedReportService.Create(reportInfo);
        }

        [HttpPost]
        public async Task<bool> Edit([FromBody] CreateAndDestryoedReportModel reportInfo)
        {
            return await CreateAndDestryoedReportService.Edit(reportInfo);
        }

        [HttpPost]
        public async Task<CreateAndDestryoedReportModel> Get([FromBody] int id)
        {
            return await CreateAndDestryoedReportService.Get(id);
        }

        [HttpPost]
        public bool Delete([FromBody] int id)
        {
            return true;
        }

        [HttpPost]
        public bool GenerateReportJob([FromBody] int id)
        {
            return CreateAndDestryoedReportService.GenerateReportJob(id);
        }
    }
}