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
using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Browser.Browser.Box;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.BoxBrowser;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers
{
    [Route("api/boxbrowser/[action]")]
    public class BoxBrowserController : RAWebApiBase
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(BoxBrowserController));

        [HttpPost]
        public async Task<RABoxBrowserContract> GetRootNode()
        {
            try
            {
                return BoxBrowser.GetRootNode();
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while attempting to browse box tree. Error: {e}");
                return null;
            }
        }

        [HttpPost]
        public async Task<IEnumerable<RABoxBrowserContract>> GetChildrenWithSettingIcon([FromBody] RABoxBrowserContract contract)
        {
            try
            {
                return await BoxBrowser.GetChildrenWithSettingIcon(contract);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while attempting to browse box tree. Error: {e}");
                return null;
            }
        }

        [HttpPost]
        public async Task<RABoxBrowserContract> BBrowserTreeByPager([FromBody] RABoxBrowserContract contract)
        {
            try
            {
                return await BoxBrowser.BBrowserTreeByPager(contract);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while attempting to browse box tree. Error: {e}");
                return null;
            }
        }
    }
}
