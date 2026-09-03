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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ReportCenter.Model;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using RAReportCenter.Model.ReportNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAReportCenter.TermUsageReport.Scanner
{
    //public class SharePointOnlineTermUsageReportScanner : TermUsageReportScanner<SharePointOnlineNeedReportNode>
    //{

    //    public SharePointOnlineTermUsageReportScanner(TermUsageReportModel reportInfo) : base(reportInfo) { }

    //    private readonly SharePointOnlineTreeQuerier querier = new SharePointOnlineTreeQuerier();

    //    protected override SourceFlag Source => SourceFlag.SharePoint;

    //    protected override IEnumerable<SharePointOnlineNeedReportNode> ExpandHasChildrenNeedReportNodes(IEnumerable<SharePointOnlineNeedReportNode> needReportNodes)
    //    {
    //        var result = new List<SharePointOnlineNeedReportNode>(needReportNodes);
    //        foreach (var node in needReportNodes)
    //        {
    //            if (node.Level == NodeLevel.WebApplication)
    //            {
    //                var siteCollections = querier.GetChildrenContainer(new RASourceTreeQuery.Model.SharePointOnlineTreeNode
    //                {
    //                    Level = NodeLevel.WebApplication,
    //                    Id = node.Id
    //                });

    //                var nodes = siteCollections.ToList().ConvertAll(item => new SharePointOnlineNeedReportNode
    //                {
    //                    Id = item.Id,
    //                    ContainerId = item.ContainerId,
    //                    LeafName = item.LeafName,
    //                    FullPath = item.FullPath,
    //                    Level = item.Level,
    //                });

    //                result.AddRange(nodes);
    //            }
    //        }
    //        return result;
    //    }

    //    protected override List<ExplorerSearchOptionV3> GetNeedReportNodeQueryOptions(SharePointOnlineNeedReportNode reportNode)
    //    {
    //        return new List<ExplorerSearchOptionV3>();
    //    }

    //    protected override string GetReportDataFullPath(BaseRecordDto data)
    //    {
    //        return data.FullPath;
    //    }

    //    protected override RMReportObjectLevel GetReportDataObjectLvel(BaseRecordDto data)
    //    {
    //        return RMReportObjectLevel.Document;
    //    }

    //    protected override string GetReportLevelI18NKeyByNodeLevel(NodeLevel level)
    //    {
    //        return "aaa";
    //    }

    //    protected override IEnumerable<SharePointOnlineNeedReportNode> RemoveNoNeedReportNodes(IEnumerable<SharePointOnlineNeedReportNode> needReportNodes)
    //    {
    //        return needReportNodes.Where(item => item.Level != NodeLevel.WebApplication).ToList();
    //    }
    //}
}
