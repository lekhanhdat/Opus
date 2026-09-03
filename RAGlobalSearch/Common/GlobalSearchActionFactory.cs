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
using AvePoint.RA.Contract.RMWeb.Explorer;
using RAGlobalSearch.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGlobalSearch.Common
{
    public static class GlobalSearchActionFactory
    {
        public static IGlobalSearchAction GetGlobalSearchAction(GlobalSearchAction action)
        {
            switch (action)
            {
                case GlobalSearchAction.DeclareRecords:
                case GlobalSearchAction.UnDeclareRecords:
                case GlobalSearchAction.AddRecordLabel:
                case GlobalSearchAction.RemoveRecordLabel:
                    return new DeclareAction(action);
                case GlobalSearchAction.MoveTo:
                    return new MoveAction();
                case GlobalSearchAction.Reclassify:
                    return new ReclassifyAction();
                case GlobalSearchAction.AccessControl:
                    return new AccessControlAction();
                case GlobalSearchAction.PhysicalBulkUpdate:
                    return new PhysicalBulkUpdateAction();
                default:
                    return null;
            }
        }
    }
}
