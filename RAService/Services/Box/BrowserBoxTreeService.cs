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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.BoxBrowser;
using AvePoint.RA.RACommonUtility.Browser;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Box
{
    public class BrowserBoxTreeService : IBrowserBoxTreeService
    {
        private static RALogger logger = new RALogger(typeof(BrowserBoxTreeService));

        public Task<RABoxBrowserContract> BBrowserTreeByPager(RABoxBrowserContract contract)
        {
            try
            {
                return BoxBrowserClient.BBrowserTreeByPager(contract);
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured when retrieved box browser tree by selected node and pager, node ID: [{contract.Id}], Error: {ex}");
                return Task.FromResult<RABoxBrowserContract>(null);
            }
        }

        public Task<IEnumerable<RABoxBrowserContract>> GetChildrenWithSettingIcon(RABoxBrowserContract contract)
        {
            try
            {
                return BoxBrowserClient.GetChildrenWithSettingIcon(contract);
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured when retrieved children nodes by selected node, node ID: [{contract.Id}], Error: {ex}");
                return Task.FromResult<IEnumerable<RABoxBrowserContract>>(null);
            }
        }

        public Task<RABoxBrowserContract> GetRootNode()
        {
            try
            {
                return BoxBrowserClient.GetRootNode();
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured when retrieved root node, Error: {ex}");
                return Task.FromResult<RABoxBrowserContract>(null);
            }
        }
    }
}
