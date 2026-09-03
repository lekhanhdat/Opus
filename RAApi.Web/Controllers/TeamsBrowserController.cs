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
using System;
using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Api.Web.Filters;
using AvePoint.RA.Browser.Browser.SPO;
using AvePoint.RA.Common.SharePointBrowser;
using AvePoint.RA.CommonUtil;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AvePoint.RA.Api.Web.Controllers
{
    [Route("api/teamsbrowser/[action]")]
    [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridInernalScope)]
    public class TeamsBrowserController : RAWebApiBase
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(TeamsBrowserController));

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        [HttpPost]
        public async System.Threading.Tasks.Task<string> Browser([FromBody] RABrowserContract contract)
        {
            try
            {
                var message = JsonConvert.DeserializeObject<SPTreeMessage>(contract.Message);
                var res = await SPOBaseBrowser.BrowseAsync(message, contract.Type);
                return JsonConvert.SerializeObject(res, SerializerSettings);
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while browser tree. Error: {e}");
                return null;
            }
        }
    }
}
