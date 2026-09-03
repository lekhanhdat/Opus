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
using AvePoint.RA.Contract.SourceTreeQuery.Model;
using AvePoint.RA.Web.Common;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.SourceTree
{

    public abstract class SourceTreeQueryController<T> : BaseApiController where T : SourceTreeNode, IParentSourceTreeNode<T>, new()
    {

        [HttpPost]
        public abstract T GetRootNode();

        [HttpPost]
        public abstract Task<IEnumerable<T>> GetChildren([FromBody] T node);

        [HttpPost]
        public abstract PagingSourceTreeNode<T> GetPagingChildren([FromBody] PagingSourceTreeNode<T> node);

        [HttpPost]
        public abstract Task<IEnumerable<T>> GetChildrenWithSettingIcon([FromBody] T node);
        
        [HttpPost]
        public abstract PagingSourceTreeNode<T> GetPagingChildrenWithSettingIcon([FromBody] PagingSourceTreeNode<T> node);

    }
}