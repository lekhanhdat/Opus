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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.Services.ManualApproval.Queriers;

public class GControlReviewerQuerier : IFilter
{
    private readonly IRALogger _logger = new RALogger(typeof(GControlReviewerQuerier));
    private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
    public ManualApprovalFilterOptions FilterOption => ManualApprovalFilterOptions.GControlReviewer;
    public async Task<Expression<Func<ManualApprovalRecord, bool>>> GetCosmosDBFilterExpressionAsync(string value)
    {
        _logger.Info($"GetCosmosDBFilterExpressionAsync: Filtering by GControlReviewer with value '{value}'.");
        var filterReviewer = JsonConvert.DeserializeObject<List<int>>(value);
        var accounts = await AccountDao.GetUserWithRemovedByIds(filterReviewer);
        var userPrincipalNames = accounts.Select(item => item.UserPrincipalName);
        accounts = AccountDao.GetUserWithRemovedByPrincipalNames(userPrincipalNames);

        var expressions = new List<Expression>();
        ParameterExpression parameter = Expression.Parameter(typeof(ManualApprovalRecord), "root");

        foreach(var reviewer in accounts)
        {
            Expression<Func<ManualApprovalRecord, bool>> expression =
                (root) => root.GControlCurrentApproverId == reviewer.AADId || Enumerable.Contains(root.GControlManualReviewers, reviewer.Id);
            expressions.Add(expression.Body);
        }
        var body = expressions.AsEnumerable().Aggregate(Expression.OrElse);
        return Expression.Lambda<Func<ManualApprovalRecord, bool>>(body, parameter);
    }
}