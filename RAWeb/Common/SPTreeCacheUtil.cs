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
using AvePoint.RA.Contract.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AvePoint.RA.Web;
using AvePoint.RA.Web.Extentions.Util;

namespace AvePoint.RA.Web.Common
{
    public class SPTreeCacheUtil
    {
        public const string FarmNodeKey = "Farm";
        public static RMSPTreeNode GetNodeById(string id, RAModule module)
        {
            RMSPTreeNode node = null;

            var session = HttpContextExtensions.CurrentHttpContext().Session;
            var key = module+ "_" + id;
            node = session.GetObject<RMSPTreeNode>(key);

            return node;
        }
        public static void CacheNode(RMSPTreeNode node, RAModule module)
        {
            if (node!=null && !string.IsNullOrEmpty(node.SPObjectId))
            {
                var session = HttpContextExtensions.CurrentHttpContext().Session;

                string key = module + "_" + node.SPObjectId;
                session.SetObject(key, node);

                if (node.Level == -1)
                {
                    key = module + "_" + FarmNodeKey;
                    session.SetObject(key, node);
                    node.Children = null;
                }
            }
        }
    }
    public enum RAModule
    {
        DisposalActivityManagement=0,
        SharePointSettings,
        ReportCenter,
        Common
    }
}