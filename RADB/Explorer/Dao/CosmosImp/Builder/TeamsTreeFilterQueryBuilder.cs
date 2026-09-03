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
using System.Collections.Generic;
using System.Linq;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using Newtonsoft.Json;
using SqlKata;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
{
    public class TeamsTreeFilterQueryBuilder : IFilterBuilder, IAdvancedQueryBuilder
    {
        private IFilterBuilder groupQueryBuilder = new ContainerIdQueryBuilder();
        private SPTreeFilterSiteQueryBuilder siteQueryBuilder = new SPTreeFilterSiteQueryBuilder();
        private TeamsTreeFilterTeamQueryBuilder teamsQueryBuilder = new TeamsTreeFilterTeamQueryBuilder();

        public Query Filter(Query query, ExplorerFilterOptionV2 filterOption)
        {
            if (!CanFilter(filterOption)) return query;
            var isQueryContainerId = IsQueryByContainerId(filterOption);
            var isQueryTeamsId = IsQueryByTeamsId(filterOption);
            var isQuerySiteId = IsQueryByTeamsSiteId(filterOption);
            if (isQueryContainerId && isQueryTeamsId && isQuerySiteId)
            {
                query.Where(allTreeQ =>
                {
                    allTreeQ.Where(containerQ =>
                    {
                        FilterContainerId(containerQ, filterOption);
                        return containerQ;
                    }).OrWhere(teamQ =>
                    {
                        FilterTeamsId(teamQ, filterOption);
                        return teamQ;
                    }).OrWhere(siteQ =>
                    {
                        FilterSiteId(siteQ, filterOption);
                        return siteQ;
                    });
                    return allTreeQ;
                });
            }
            else if (isQueryContainerId)
            {
                FilterContainerId(query, filterOption);
            }
            else if (isQueryTeamsId)
            {
                FilterTeamsId(query, filterOption);
            }
            else if (isQuerySiteId)
            {
                FilterSiteId(query, filterOption);
            }
            return query;
        }

        #region Precheck query
        private bool IsQueryByContainerId(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption != null && filterOption.SPNodes != null && filterOption.SPNodes.Any(n => n.Level == (int)NodeLevel.WebApplication);
        }

        private bool IsQueryByTeamsId(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption != null && filterOption.SPNodes != null && filterOption.SPNodes.Any(n => n.Level == (int)NodeLevel.Office365GroupEntire);
        }

        private bool IsQueryByTeamsSiteId(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption != null && filterOption.SPNodes != null && filterOption.SPNodes.Any(n => n.Level == (int)NodeLevel.SiteCollection);
        }
        #endregion

        #region Query filter
        private void FilterContainerId(Query query, ExplorerFilterOptionV2 filterOption)
        {
            groupQueryBuilder.Filter(query, new ExplorerFilterOptionV2 { ContainerIds = filterOption.SPNodes.Where(n => n.Level == (int)NodeLevel.WebApplication).Select(n => n.Id).ToList() });
        }

        private void FilterTeamsId(Query query, ExplorerFilterOptionV2 filterOption)
        {
            teamsQueryBuilder.Filter(query, filterOption);
        }

        private void FilterSiteId(Query query, ExplorerFilterOptionV2 filterOption)
        {
            siteQueryBuilder.Filter(query, filterOption);
        }
        #endregion

        private bool CanFilter(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption != null && filterOption.SPNodes != null && filterOption.SPNodes.Count > 0;
        }

        #region Advanced search  
        public Query Build(Query query, ExplorerQueryColumn column, string objJson, ExplorerSearchColumnOperationLogic columnOperationLogic, ExplorerSearchKeyOperationLogic keyOperationLogic = ExplorerSearchKeyOperationLogic.AND)
        {
            if (!string.Equals(GetColumnId(), column.Id, System.StringComparison.OrdinalIgnoreCase)) return query;

            var filterOption = Convert2SearchOptionV2(column, objJson, columnOperationLogic, keyOperationLogic);
            return Filter(query, filterOption);
        }

        protected string GetColumnId()
        {
            return Contract.TemplateManagement.QueryCloumnIds.TeamsLocation;
        }
        protected ExplorerFilterOptionV2 Convert2SearchOptionV2(ExplorerQueryColumn column, string objJson, ExplorerSearchColumnOperationLogic columnOperationLogic, ExplorerSearchKeyOperationLogic keyOperationLogic)
        {
            return new ExplorerFilterOptionV2() { SPNodes = JsonConvert.DeserializeObject<List<SPFilterNode>>(objJson) };
        }
        #endregion
    }
}
