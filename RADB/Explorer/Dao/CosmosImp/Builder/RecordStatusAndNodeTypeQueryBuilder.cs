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

namespace AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder
{
    /// <summary>
    /// 需要根据不同的数据源，来确定node type和status的组合filter
    /// </summary>
    public class RecordStatusAndNodeTypeQueryBuilder : IFilterBuilder
    {
        private IObjectArrayFilterBuilder<RMRecordStatus> statusFilterBuilder = new RecordStatusQueryBuilder();
        private IObjectArrayFilterBuilder<int> nodeTypeFilterBuilder = new NodeTypeQueryBuilder();

        public Query Filter(Query query, ExplorerFilterOptionV2 filterOption)
        {
            if (!CanFilter(filterOption)) return query;

            var isQueryElectronic = filterOption.IsQueryElectronic();
            var isQueryPhysical = filterOption.IsQueryPhysical();

            if (isQueryElectronic && isQueryPhysical)
            {
                query.Where(q1 =>
                {
                    q1.Where(q2 =>
                    {
                        FilterElectroic(q2, filterOption);
                        return q2;
                    }).OrWhere(q3 =>
                    {
                        FilterPhysical(q3, filterOption);
                        return q3;
                    });

                    return q1;
                });
            }
            else if (isQueryElectronic)
            {
                FilterElectroic(query, filterOption);
            }
            else if (isQueryPhysical)
            {
                FilterPhysical(query, filterOption);
            }

            return query;
        }

        //如果页面勾选了查询Archived数据并且关系为and，则替换RecordStatus为Archived，即只查询Archived状态的数据
        private void FilterElectroic(Query query, ExplorerFilterOptionV2 filterOption)
        {
            nodeTypeFilterBuilder.Filter(query, filterOption.GetElectronicNodeTypes());
            statusFilterBuilder.Filter(query, filterOption.QueryArchivedData != null ? RecordStatusHelper.GetElectronicStatusWithArchived() : RecordStatusHelper.GetDefaultElectronicStatus());
        }


        private void FilterPhysical(Query query, ExplorerFilterOptionV2 filterOption)
        {
            nodeTypeFilterBuilder.Filter(query, filterOption.GetPhysicalNodeTypes());
            statusFilterBuilder.Filter(query, filterOption.GetPhysicalStatus());
        }

        private bool CanFilter(ExplorerFilterOptionV2 filterOption)
        {
            return filterOption != null && filterOption.SourceFlags != null && filterOption.SourceFlags.Count > 0;
        }

        //private bool IsQueryElectronic(ExplorerFilterOptionV2 filterOption)
        //{
        //    var sourceFlags = new List<SourceFlag> { SourceFlag.SharePoint, SourceFlag.Exchange, SourceFlag.FileSystem };
        //    return filterOption.SourceFlags.Exists(s => sourceFlags.Contains(s)) && GetElectronicNodeTypes(filterOption).Count() > 0;
        //}

        //private bool IsQueryPhysical(ExplorerFilterOptionV2 filterOption)
        //{
        //    return filterOption.SourceFlags.Exists(s => s == SourceFlag.Physical) && GetPhysicalNodeTypes(filterOption).Count() > 0;
        //}

        //private int[] GetElectronicNodeTypes(ExplorerFilterOptionV2 filterOption)
        //{
        //    var nodeTypes = new List<int>();
        //    if (filterOption.SourceFlags.Contains(SourceFlag.SharePoint)) nodeTypes.Add((int)RMNodeLevel.Item);
        //    if (filterOption.SourceFlags.Contains(SourceFlag.Exchange)) nodeTypes.Add((int)NodeLevel.ExchangeOnlineItem);
        //    if (filterOption.SourceFlags.Contains(SourceFlag.FileSystem))
        //    {
        //        nodeTypes.AddRange(new List<int> { (int)RMNodeLevel.FSFolder, (int)RMNodeLevel.FSFile });
        //    }

        //    var withoutNodeTypes = filterOption.WithoutNodeTypes;
        //    if (withoutNodeTypes != null && withoutNodeTypes.Count > 0)
        //    {
        //        foreach (var withoutType in withoutNodeTypes)
        //        {
        //            if (nodeTypes.Contains((int)withoutType))
        //            {
        //                nodeTypes.Remove((int)withoutType);
        //            }
        //        }
        //    }

        //    if (filterOption.NodeTypes != null)
        //    {
        //        var tempNodeTypes = filterOption.NodeTypes.Select(o => (int)o).ToList();
        //        return nodeTypes.Intersect(tempNodeTypes).ToArray();
        //    }

        //    return nodeTypes.ToArray();
        //}

        //private int[] GetPhysicalNodeTypes(ExplorerFilterOptionV2 filterOption)
        //{
        //    var result =  new int[] { (int)RMNodeLevel.PhysicalBox, (int)RMNodeLevel.PhysicalFile, (int)RMNodeLevel.PhysicalRecord };

        //    var withoutNodeTypes = filterOption.WithoutNodeTypes;
        //    if (withoutNodeTypes != null && withoutNodeTypes.Count > 0)
        //    {
        //        var resultList = result.ToList();
        //        foreach (var withoutType in withoutNodeTypes)
        //        {
        //            if (resultList.Contains((int)withoutType))
        //            {
        //                resultList.Remove((int)withoutType);
        //            }
        //        }
        //        result = resultList.ToArray();
        //    }

        //    if (filterOption.NodeTypes != null)
        //    {
        //        var tempNodeTypes = filterOption.NodeTypes.Select(o => (int)o).ToList();
        //        return result.Intersect(tempNodeTypes).ToArray();
        //    }

        //    return result;
        //}

        //private RMRecordStatus[] GetPhysicalStatus(ExplorerFilterOptionV2 filterOption)
        //{
        //    var result= new RMRecordStatus[] { RMRecordStatus.Active, RMRecordStatus.Closed, RMRecordStatus.Destroyed, RMRecordStatus.Missing };
        //    if (filterOption.Status != null)
        //    {
        //        return result.Intersect(filterOption.Status).ToArray();
        //    }

        //    return result;
        //}

        //private RMRecordStatus[] GetElectronicStatus()
        //{
        //    return new RMRecordStatus[] { RMRecordStatus.Active };

        //}
    }
}
