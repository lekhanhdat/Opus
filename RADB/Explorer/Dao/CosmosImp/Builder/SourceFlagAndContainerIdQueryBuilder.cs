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
using AvePoint.RA.Contract.RMWeb.Explorer;
using SqlKata;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
{
    public class SourceFlagAndContainerIdQueryBuilder : IFilterBuilder
    {
        private IFilterBuilder containerIdQueryBuilder = new ContainerIdQueryBuilder();
        private SourceFlagQueryBuilder sourceFlagQueryBuilder = new SourceFlagQueryBuilder();
        public Query Filter(Query query, ExplorerFilterOptionV2 filterOption)
        {
            if (!CanFilter(filterOption)) return query;

            var isQueryContainerId = IsQueryContainerId(filterOption);
            var isQueryNonContainerIdSourceFlag = IsQueryNonContainerIdSourceFlag(filterOption);
            if (isQueryContainerId && isQueryNonContainerIdSourceFlag) //container id only valid for non EXO or SP sources
            {
                query.Where(q1 =>
                {
                    q1.Where(q2 =>
                    {
                        FilterNonContainerSourceFlag(q2, filterOption);
                        return q2;
                    }).OrWhere(q3 =>
                    {
                        FilterContainerId(q3, filterOption);
                        return q3;
                    });

                    return q1;
                });
            }
            else if (isQueryContainerId)
            {
                FilterContainerId(query, filterOption);
                //query.WhereArrayContainV2(GetFilterValue(filterOption), GetFilterColumnName().FormatColumnName());
            }
            else 
            {
                sourceFlagQueryBuilder.Filter(query, filterOption);
            }


            return query;
        }

        private bool IsQueryContainerId(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption.ContainerIds != null;
        }

        private bool IsQueryNonContainerIdSourceFlag(ExplorerFilterOptionV2 filterOption)
        {
            var sources = filterOption.SourceFlags.Except(GetDefaultContainerIdSource());
            return sources.Any();
        }

        //private List<SourceFlag> GetContainerIdSource(ExplorerFilterOptionV2 filterOption)
        //{
        //    return filterOption.SourceFlags.Intersect(GetDefaultContainerIdSource()).ToList();
        //}

        private List<SourceFlag> GetDefaultContainerIdSource()
        {
            //return new List<SourceFlag> { SourceFlag.Exchange, SourceFlag.SharePoint };
            return SourceFlagHelper.GetDefaultContainerIdSource();
        }

        private void FilterNonContainerSourceFlag(Query query, ExplorerFilterOptionV2 filterOption)
        {
            var sources = filterOption.SourceFlags.Except(GetDefaultContainerIdSource());
            sourceFlagQueryBuilder.Filter(query, sources.ToArray());
            //query.WhereArrayContainV2(sources, GetFilterColumnName().FormatColumnName());
        }

        private void FilterContainerId(Query query, ExplorerFilterOptionV2 filterOption)
        {
            containerIdQueryBuilder.Filter(query, filterOption);
        }

        private bool CanFilter(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption != null && filterOption.SourceFlags != null;
        }

        //private string GetFilterColumnName()
        //{
        //    return CosmosConst.C_SourceFlag;
        //}

        //private object GetFilterValue(ExplorerFilterOptionV2 filterOption)
        //{
        //    return filterOption.SourceFlags;
        //}
    }
}
