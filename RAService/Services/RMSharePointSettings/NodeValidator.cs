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
using RazorEngine.Compilation.ImpromptuInterface.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RMSharePointSettings
{
    public class ValidateNodeInfo
    {
        public Guid ScopeId { get; set; }
        public Guid GroupId { get; set; }
        public bool IsValid { get; set; }
    }

    public static class ValidateNodeInfoExtension
    {
        public static bool NodeExistingInCache(this ValidateNodeInfo node, List<ValidateNodeInfo> cachedNodes)
        {
            return cachedNodes.Any(n => n.ScopeId == node.ScopeId && n.GroupId == node.GroupId);
        }

        public static bool NodeIsValid(this ValidateNodeInfo node, List<ValidateNodeInfo> cachedNodes)
        {
            return cachedNodes.First(n => n.ScopeId == node.ScopeId && n.GroupId == node.GroupId).IsValid;
        }

        public static void AddNode2Cache(this ValidateNodeInfo node, List<ValidateNodeInfo> cachedNodes)
        {
            cachedNodes.Add(node);
        }
    }


}
