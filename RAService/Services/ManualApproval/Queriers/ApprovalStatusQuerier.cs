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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ManualApproval.Queriers
{
    public class ApprovalStatusQuerier : IFilterWithHistory, ISorter, IDefaultValue
    {
        public ManualApprovalOrderOptions OrderOption => ManualApprovalOrderOptions.ApprovalStatus;

        public ManualApprovalFilterOptions FilterOption => ManualApprovalFilterOptions.ApprovalStatus;

        public ManualApprovalDefaultOptions DefaultValueOption => ManualApprovalDefaultOptions.ApprovalStatus;

        public async Task<Expression<Func<ManualApprovalRecord, bool>>> GetCosmosDBFilterExpressionAsync(string value)
        {
            var filterApprovalStatus = JsonConvert.DeserializeObject<List<int>>(value);
            return (root) => filterApprovalStatus.Contains(root.ManualApprovedStatus);
        }

        public System.Linq.Expressions.Expression<Func<ManualApprovalRecord, dynamic>> GetCosmosDBOrderExpression()
        {
            return (root) => root.ManualApprovedStatus;
        }

        public async Task<object> GetDefaultValueAsync()
        {
            return new List<SOApproveDBStatus>
            {
                //SOApproveDBStatus.WaitingApprove,
                SOApproveDBStatus.Approved,
                SOApproveDBStatus.Rejected,
            };
        }

        public async Task<ManualApprovalSqlDefintion> GetHistorySqlDefinitionAsync(string value)
        {
            var filterApprovalStatus = JsonConvert.DeserializeObject<List<int>>(value);
            var sql = $"ApprovedStatus IN {DatabaseUtility.BuildInClause(filterApprovalStatus)}";
            var result = new ManualApprovalSqlDefintion
            {
                Sql = sql,
            };

            return result;
        }
    }
}
