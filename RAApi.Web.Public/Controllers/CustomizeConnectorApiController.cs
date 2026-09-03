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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.Common.SharePointBrowser;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.CustomizeConnector;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.RACommonUtility.Browser;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [Route("api/connector/[action]")]
    [TypeFilter(typeof(APIRateLimitFilter))]
    [APIScopeFilter(ContractConstants.RecordsPublicScope)]
    [RMConnectorApiPerformanceLogger]
    public class CustomizeConnectorApiController : RAWebApiBase
    {

        private IRMCustomizeConnectorApiService _CustomizeConnectorApiService;

        private IRMCustomizeConnectorApiService CustomizeConnectorApiService => PlatformWindsorManager.GetService(ref _CustomizeConnectorApiService);

        [HttpPost]
        public object SubmitRecords([FromBody] object connectorDataObj)
        {
            return CustomizeConnectorApiService.InsertData(connectorDataObj).GetAwaiter().GetResult();
        }

        [HttpPost]
        public object GetDueRecords([FromBody] object queryInfo)
        {
            return CustomizeConnectorApiService.GetData(queryInfo).GetAwaiter().GetResult();
        }

        [HttpPost]
        public object DisposeRecords([FromBody] object disposalInfo)
        {
            return CustomizeConnectorApiService.DisposeRecords(disposalInfo).GetAwaiter().GetResult();
        }
    }
}
