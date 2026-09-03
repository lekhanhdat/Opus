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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ManualApproval.Queriers
{
    public class ReviewerQuerier : IFilterWithHistory
    {

        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        public ManualApprovalFilterOptions FilterOption => ManualApprovalFilterOptions.Reviewer;

        public async Task<Expression<Func<ManualApprovalRecord, bool>>> GetCosmosDBFilterExpressionAsync(string value)
        {
            var filterReviewer = JsonConvert.DeserializeObject<List<int>>(value);
            var accounts = await AccountDao.GetUserWithRemovedByIds(filterReviewer);
            var userPrincipalNames = accounts.Select(item => item.UserPrincipalName);
            accounts = AccountDao.GetUserWithRemovedByPrincipalNames(userPrincipalNames);
            filterReviewer = accounts.Select(item => item.Id).ToList();

            var expressions = new List<Expression>();
            ParameterExpression parameter = Expression.Parameter(typeof(ManualApprovalRecord), "root");

            foreach(var reviewr in filterReviewer)
            {
                Expression<Func<ManualApprovalRecord, bool>> expression =
                    (root) => Enumerable.Contains(root.ManualReviewer, reviewr);
                expressions.Add(expression.Body);
            }
            var body = expressions.AsEnumerable().Aggregate(Expression.OrElse);
            return Expression.Lambda<Func<ManualApprovalRecord, bool>>(body, parameter);
        }

        public async Task<ManualApprovalSqlDefintion> GetHistorySqlDefinitionAsync(string value)
        {
            var sqls = new List<string>();
            var parameters = new List<SqlParameter>();
            var filterReviewer = JsonConvert.DeserializeObject<List<int>>(value);

            for(var i = 0; i < filterReviewer.Count; i++)
            {
                var review = filterReviewer[i];
                var paramName = "@Reviewr"+review;
                var paramValue = $"|{review}|";
                var innerSql = $"EscalateTo LIKE '%'+{paramName}+'%'";
                sqls.Add(innerSql);
                parameters.Add(new SqlParameter(paramName, paramValue));
            }

            var sql = "(" + string.Join(" OR ", sqls) +")";
            return new ManualApprovalSqlDefintion
            {
                Sql = sql,
                Parameter = parameters
            };
        }
    }
}
