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
using AvePoint.RA.Browser.Browser.Box;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Service.Services.RMFileSystemSettings;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.Box
{
    [RMApiAuthorize(RMPermissionExtensionMasks.BoxAdmin, preferred: false)]
    public class BoxTreeQueryController : BaseApiController
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(BoxTreeQueryController));

        private IBrowserBoxTreeService BrowserBoxTreeService => PlatformWindsorManager.GetService<IBrowserBoxTreeService>();

        [HttpPost]
        public async Task<BoxTreeNode> GetRootNode()
        {
            try
            {
                var rootNode = await BrowserBoxTreeService.GetRootNode();
                return BoxBrowser.ConvertToBoxTreeNode(rootNode);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while attempting to browse box tree. Error: {e}");
                return null;
            }
        }

        [HttpPost]
        public async Task<IEnumerable<BoxTreeNode>> GetChildrenWithSettingIcon([FromBody] BoxTreeNode node)
        {
            try
            {
                var contract = BoxBrowser.ConvertToBoxBrowserContract(node);
                var children = await BrowserBoxTreeService.GetChildrenWithSettingIcon(contract);
                return children.ConvertAll(child => BoxBrowser.ConvertToBoxTreeNode(child));
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while attempting to browse box tree. Error: {e}");
                return null;
            }
        }

        [HttpPost]
        public Task<IEnumerable<BoxTreeNode>> GetPagingChildrenWithSettingIcon([FromBody] BoxTreeNode contract)
        {
            throw new NotImplementedException();
        }

        #region Report center
        [HttpPost]
        public async Task<BoxTreeNode> BBrowserTreeByPager([FromBody] BoxTreeNode curNode)
        {
            string name = string.Empty;
            try
            {
                var contract = BoxBrowser.ConvertToBoxBrowserContract(curNode);
                curNode = BoxBrowser.ConvertToBoxTreeNode(await BrowserBoxTreeService.BBrowserTreeByPager(contract));
            }
            catch(Exception e)
            {
                Logger.Error("An error occurred when browser node.NodeName:[{0}] Error:{1}", name, e.ToString());
            }
            return curNode;
        }

        #endregion
    }
}
