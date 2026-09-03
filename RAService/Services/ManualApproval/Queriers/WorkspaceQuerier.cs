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
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.DB.Explorer.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ManualApproval.Queriers
{
    public class WorkspaceQuerier : IFilter
    {
        public ManualApprovalFilterOptions FilterOption => ManualApprovalFilterOptions.Workspace;

        public Task<Expression<Func<ManualApprovalRecord, bool>>> GetCosmosDBFilterExpressionAsync(string value)
        {
            var definition = JsonConvert.DeserializeObject<WorkspaceQueryDefinition>(value);
            if (definition.ContentSource == SourceFlag.OneDrive)
            {
                var workspaceIds = definition.WorkspaceIds;
                return System.Threading.Tasks.Task.FromResult<Expression<Func<ManualApprovalRecord, bool>>>((root) => workspaceIds.Contains(root.ScopeId));
            }

            var expressions = new List<Expression>();
            ParameterExpression parameter = Expression.Parameter(typeof(ManualApprovalRecord), "root");

            var workspacePaths = definition.WorkspacePaths.Select(item => item.EndsWith('/') ? item.ToLower() : item.ToLower() + '/');
            if (definition.ContentSource == SourceFlag.FileSystem)
            {
                workspacePaths = definition.WorkspacePaths.Select(item => item.EndsWith('\\') ? item.ToLower() : item.ToLower() + '\\');
            }

            foreach (var workspacePath in workspacePaths)
            {
                Expression<Func<ManualApprovalRecord, bool>> expression =
                    (root) => root.ManualFullPath.ToLower().StartsWith(workspacePath.ToLower());
                expressions.Add(expression.Body);
            }
            var body = expressions.AsEnumerable().Aggregate(Expression.OrElse);
            return System.Threading.Tasks.Task.FromResult(Expression.Lambda<Func<ManualApprovalRecord, bool>>(body, parameter));
        }

        public class WorkspaceQueryDefinition
        {
            public List<Guid> WorkspaceIds { get; set; } = new();

            public List<string> WorkspacePaths { get; set; } = new();

            public SourceFlag ContentSource { get; set; }
        }
    }
}
