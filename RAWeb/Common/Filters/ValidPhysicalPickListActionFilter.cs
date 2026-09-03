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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidPhysicalPickListActionFilter : BaseActionFilter
    {
        //private RALogger logger = RALogger.GetInstance(typeof(ValidPhysicalPickListActionFilter));
        public const string PICK_LIST_ACTION_FOR_LOAN = "Loan";
        public const string PICK_LIST_ACTION_FOR_DESTRUCTION = "Destruction";
        private string action;
        public ValidPhysicalPickListActionFilter()
        {

        }
        public ValidPhysicalPickListActionFilter(string type)
        {
            action = type;
        }

        protected override Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            Object parmObj = actionContext.ActionArguments.Values.FirstOrDefault();
            if (parmObj is CompleteActionParam completeActionParam)
            {
                if (!completeActionParam.IsSelectAll)
                {
                    ExplorerDao explorerDao = new();
                    var records = explorerDao.QueryAll(r => completeActionParam.SelectedItemIds != null && completeActionParam.SelectedItemIds.Contains(r.Id));

                    Func<Record, bool> predicate = action switch
                    {
                        PICK_LIST_ACTION_FOR_LOAN => r => r.LoanPickStatus != (int)PickStatusType.Pendding,
                        PICK_LIST_ACTION_FOR_DESTRUCTION => r => r.DestructionPickStatus != (int)PickStatusType.Pendding,
                        _ => throw new ArgumentOutOfRangeException($"Not expected action type {action}")
                    };

                    if (records.Any(predicate))
                    {
                        actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                        return Task.CompletedTask; ;
                    }

                    if (completeActionParam.IsContainerLevel && !records.Any(r => r.NodeType == (int)RMNodeType.PhyBox))
                    {
                        actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                        return Task.CompletedTask;
                    }

                    if (!completeActionParam.IsContainerLevel && records.Any(r => r.NodeType == (int)RMNodeType.PhyBox))
                    {
                        actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                        return Task.CompletedTask;
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}