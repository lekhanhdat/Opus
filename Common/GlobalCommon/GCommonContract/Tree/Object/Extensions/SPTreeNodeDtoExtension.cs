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
using System;

namespace AvePoint.GCommon.Contract.Tree.Object
{
    public static class SPTreeNodeDtoExtension
    {
        //public static Boolean IsVirtualNode(this SPTreeNodeDto treeNode)
        //{
        //    return treeNode.Level == NodeLevel.Lists ||
        //        treeNode.Level == NodeLevel.RootFolder ||
        //        treeNode.Level == NodeLevel.Items ||
        //        treeNode.Level == NodeLevel.Folders ||
        //        treeNode.Level == NodeLevel.Apps ||
        //        treeNode.Level == NodeLevel.Sites;
        //}

        public static SPTreeNodeDto GetSiteCollectionNode(this SPTreeNodeDto curnode)
        {
            var node = curnode;
            while (node.Level != NodeLevel.SiteCollection)
            {
                node = node.Parent;
            }
            return node;
        }

        public static SPTreeNodeDto GetGroupNode(this SPTreeNodeDto node)
        {
            while (node.Level != NodeLevel.WebApplication)
            {
                node = node.Parent;
            }
            return node;
        }

        public static SPTreeNodeDto GetTeamsNode(this SPTreeNodeDto node)
        {
            while (node != null && node.Level != NodeLevel.Office365GroupEntire)
            {
                node = node.Parent;
            }
            return node;
        }
    }
}
