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
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.SourceTreeQuery.Model;
using AvePoint.RA.Service.Services.SourceTreeQuery;
using AvePoint.RA.Web.Common.Filters.SourceTreeNodeFilters;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.SourceTree
{
    [RMApiAuthorize(RMPermissionExtensionMasks.AzureFSAdmin, preferred: false)]
    public class AzureFileShareTreeQueryController : SourceTreeQueryController<AzureFileShareTreeNode>
    {

        private static readonly AzureFileShareTreeQuerier Querier = new AzureFileShareTreeQuerier();

        public override AzureFileShareTreeNode GetRootNode()
        {
            return Querier.GetRootNode();
        }

        [AzureFileShareTreeNodePermissionFilter]
        public override Task<IEnumerable<AzureFileShareTreeNode>> GetChildren([FromBody] AzureFileShareTreeNode node)
        {
            return Querier.GetChildrenContainerAsync(node);
        }

        [AzureFileShareTreeNodePermissionFilter]
        public override Task<IEnumerable<AzureFileShareTreeNode>> GetChildrenWithSettingIcon([FromBody] AzureFileShareTreeNode node)
        {
            return Querier.GetChildrenContainerWithSettingIconAsync(node);
        }

        [AzureFileShareTreeNodePermissionFilter]
        public override PagingSourceTreeNode<AzureFileShareTreeNode> GetPagingChildren([FromBody] PagingSourceTreeNode<AzureFileShareTreeNode> node)
        {
            throw new NotImplementedException();
        }

        [AzureFileShareTreeNodePermissionFilter]
        public override PagingSourceTreeNode<AzureFileShareTreeNode> GetPagingChildrenWithSettingIcon([FromBody] PagingSourceTreeNode<AzureFileShareTreeNode> node)
        {
            throw new NotImplementedException();
        }
    }
}