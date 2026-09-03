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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Browser.Model;
using AvePoint.RA.Common;
using AvePoint.RA.Common.SharePointBrowser;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Browser.Browser.SPO
{
    public abstract class SPOBaseBrowser
    {

        protected static readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        protected static readonly IRMRemoteNodeService RemoteNodeService = PlatformWindsorManager.GetService<IRMRemoteNodeService>();

        protected SPOBaseBrowser() { }

        protected abstract Task<BrowseResult> BrwoseAsync(SPTreeNodeDto node);

        public static async Task<SPTreeMessage> BrowseAsync(AveTreeMessage request, BrowserType browserType)
        {
            var message = request as SPTreeMessage;
            var response = new SPTreeMessage();
            SPOBaseBrowser instance = new SPOBposChildrenBrowser(browserType);
            Logger.Info($"Start browse {browserType} tree, level: {message.Node.Level}.");
            var result = await instance.BrwoseAsync(message.Node);
            instance.SetNodesProperties(result.Children as List<SPTreeNodeDto>, message.Node);
            if(result.Children != null)
            {
                (result.Children as List<SPTreeNodeDto>).Sort((node1, node2) => string.Compare(node1.Name, node2.Name, StringComparison.CurrentCulture));
            }
            response.Node = message.Node;
            instance.SetTreeCredentialPasswordEmpty(response.Node);
            response.NodeList = result.Children as List<SPTreeNodeDto>;
            response.PageInfo = result.PageInfo;
            response.ChildrenCount = result.ChildrenCount;
            response.HasNextPage = result.HasNextPage;
            response.HasError = result.HasError;
            response.Message = result.ErrorMessage;
            Logger.Info($"End browse sharepoint tree, level: {message.Node.Level}, children count: {response.ChildrenCount}.");
            return response;
        }

        private void SetTreeCredentialPasswordEmpty(SPTreeNodeDto node)
        {
            if (node != null)
            {
                if (node.Level == NodeLevel.SiteCollection)
                {
                    if (node?.NodeExtension?.BposInfo?.UserAccountInfo != null)
                    {
                        node.NodeExtension.BposInfo.UserAccountInfo.Password = string.Empty;
                    }
                }
                else if (node.Level > NodeLevel.SiteCollection)
                {
                    SetTreeCredentialPasswordEmpty(node.Parent);
                }
            }
        }

        private void SetNodesProperties(IList<SPTreeNodeDto> children, SPTreeNodeDto currentNode)
        {
            if (children != null)
            {
                foreach (SPTreeNodeDto child in children)
                {
                    child.SPType = SPType.BPOS;
                    if (child.Level != NodeLevel.ItemVersion && child.Level != NodeLevel.AppData)
                    {
                        child.CanChildrenBeLoaded = true;
                    }
                    if (currentNode != null)
                    {
                        child.SPVersion = currentNode.SPVersion;
                    }
                }
            }
        }
    }
}
