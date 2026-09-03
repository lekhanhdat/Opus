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
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.Records.Core.Utilities.Extensions;
using Newtonsoft.Json;
using SqlKata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
{
    public class GoogleTreeFilterQueryBuilder : IFilterBuilder, IAdvancedQueryBuilder
    {
        private IFilterBuilder groupQueryBuilder = new ContainerIdQueryBuilder();
        private GoogleTreeFilterDriveQueryBuilder _driveQueryBuilder = new GoogleTreeFilterDriveQueryBuilder();
        private ParentIdQueryBuilder _parentIdQueryBuilder = new ParentIdQueryBuilder();
        public Query Filter(Query query, ExplorerFilterOptionV2 filterOption)
        {
            if (!CanFilter(filterOption)) return query;

            var isQueryContainerId = IsQueryByContainerId(filterOption);
            var isQueryDriveId = IsQueryByDriveId(filterOption);
            //var isQueryFolderId = IsQueryByFolderId(filterOption);

            //if (isQueryContainerId && isQueryDriveId && isQueryFolderId)
            if (isQueryContainerId && isQueryDriveId)
                {
                query.Where(allTreeQ =>
                {
                    allTreeQ.Where(containerQ =>
                    {
                        FilterContainerId(containerQ, filterOption);
                        return containerQ;
                    }).OrWhere(driveQ =>
                    {
                        FilterDriveId(driveQ, filterOption);
                        return driveQ;
                    //}).OrWhere(folderQ =>
                    //{
                    //    FilterFolderId(folderQ, filterOption);
                    //    return folderQ;
                    });
                    return allTreeQ;
                });
            }
            else if (isQueryContainerId)
            {
                FilterContainerId(query, filterOption);
            }
            else if (isQueryDriveId)
            {
                FilterDriveId(query, filterOption);
            }
            //else if (isQueryFolderId)
            //{
            //    FilterFolderId(query, filterOption);
            //}
            return query;
        }
        #region Precheck query
        private bool CanFilter(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption != null && filterOption.SPNodes != null && filterOption.SPNodes.Count > 0;
        }
        private bool IsQueryByContainerId(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption != null && filterOption.SPNodes != null && filterOption.SPNodes.Any(n => n.Level == (int)NodeLevel.GoogleSharedDriveContainer || n.Level == (int)NodeLevel.GoogleMyDriveContainer);
        }
        private bool IsQueryByDriveId(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption != null && filterOption.SPNodes != null && filterOption.SPNodes.Any(n => n.Level == (int)NodeLevel.GoogleSharedDrive || n.Level == (int)NodeLevel.GoogleMyDrive);
        }
        private bool IsQueryByFolderId(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption != null && filterOption.SPNodes != null && filterOption.SPNodes.Any(n => n.Level == (int)NodeLevel.GoogleFolder);
        }

        #endregion

        #region Query filter
        private void FilterContainerId(Query query, ExplorerFilterOptionV2 filterOption)
        {
            groupQueryBuilder.Filter(query, new ExplorerFilterOptionV2 { ContainerIds = filterOption.SPNodes.Where(n => n.Level == (int)NodeLevel.GoogleMyDriveContainer || n.Level == (int)NodeLevel.GoogleSharedDriveContainer).Select(n => n.Id).ToList() });
        }

        private void FilterDriveId(Query query, ExplorerFilterOptionV2 filterOption)
        {
            _driveQueryBuilder.Filter(query, filterOption);
        }
        private void FilterFolderId(Query query, ExplorerFilterOptionV2 filterOption)
        {
            _parentIdQueryBuilder.Filter(query, new ExplorerFilterOptionV2 { ParentIds = filterOption.SPNodes.Where(n => n.Level == (int)NodeLevel.GoogleFolder).Select(n => $"{n.DriveId}/{n.Id}".ToMd5()).ToList()});
        }
        #endregion

        #region Advanced search  
        public Query Build(Query query, ExplorerQueryColumn column, string objJson, ExplorerSearchColumnOperationLogic columnOperationLogic, ExplorerSearchKeyOperationLogic keyOperationLogic = ExplorerSearchKeyOperationLogic.AND)
        {
            if (!string.Equals(GetColumnId(), column.Id, System.StringComparison.OrdinalIgnoreCase)) return query;

            var filterOption = Convert2SearchOptionV2(objJson);
            return Filter(query, filterOption);
        }

        protected string GetColumnId()
        {
            return Contract.TemplateManagement.QueryCloumnIds.GoogleLocation;
        }
        protected ExplorerFilterOptionV2 Convert2SearchOptionV2(string objJson)
        {
            return new ExplorerFilterOptionV2() { SPNodes = JsonConvert.DeserializeObject<List<SPFilterNode>>(objJson) };
        }
        #endregion
    }
}
