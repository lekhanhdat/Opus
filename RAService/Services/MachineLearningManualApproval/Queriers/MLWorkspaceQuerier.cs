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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.Service.Services.ManualApproval.Queriers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.MachineLearningManualApproval.Queriers
{
    public class MLWorkspaceQuerier : IFilter
    {
        public ManualApprovalFilterOptions FilterOption => ManualApprovalFilterOptions.MLWorkspace;

        private IRMScopeDao RMScopeDao => PlatformWindsorManager.GetService<IRMScopeDao>();

        public Task<Expression<Func<ManualApprovalRecord, bool>>> GetCosmosDBFilterExpressionAsync(string value)
        {
            var definition = JsonConvert.DeserializeObject<WorkspaceQueryDefinition>(value);
            List<Guid> siteIds = definition.WorkspaceIds;
            if (definition.ContentSource == SourceFlag.Google)
            {
                var workspaceIds = definition.Extentions;
                return System.Threading.Tasks.Task.FromResult<Expression<Func<ManualApprovalRecord, bool>>>((root) => workspaceIds.Contains(root.AveSiteId));
            }
            if (definition.ContentSource == SourceFlag.SharePoint)
            {
                siteIds = RMScopeDao.GetScopeIds(definition.WorkspacePaths);
            }
            return System.Threading.Tasks.Task.FromResult<Expression<Func<ManualApprovalRecord, bool>>>((record) => siteIds.Contains(record.ScopeId));
        }
    }

    public class WorkspaceQueryDefinition
    {
        public List<Guid> WorkspaceIds { get; set; } = new();

        public List<string> WorkspacePaths { get; set; } = new();

        public SourceFlag ContentSource { get; set; }

        public List<string> Extentions { get; set; } = new();
    }
}
