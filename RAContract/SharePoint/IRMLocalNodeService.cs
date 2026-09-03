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
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.SharePoint.OnPrem;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.SharePoint
{
    public interface IRMLocalNodeService
    {
        void BatchAdd(IEnumerable<OnPremiseSPLocalNode> nodes);

        int DeleteNodesByIDs(IEnumerable<string> ids);

        int UpdateNodes(IEnumerable<OnPremiseSPLocalNode> nodes);

        List<SPTreeNodeDto> GetAllNodes();

        Task<List<OnPremiseSPLocalNode>> GetPageNodesByParentIdAsync(int pageIndex, int total, string parentId);

        RMSiteCollection GetLocalSiteCollectionById(string id);

        Task<List<RMSiteCollection>> GetLocalSiteCollectionsByIdListAsync(List<string> ids);

        Task<List<RMSiteCollection>> GetAllLocalSiteCollectionsAsync();

        bool IsLocalSiteCollectionExistByUrl(string url);

        RMSiteCollection GetLocalSiteCollectionByUrl(string url);

        Task<List<RMSiteCollection>> GetLocalSiteCollectionsByWebAppIdAsync(string webappId);

        Task<List<RMWebApplication>> GetAllLocalWebApplicationsAsync();

        RMWebApplication GetLocalWebApplicationById(string id);
        Task<List<RMSiteCollection>> GetLocalSiteCollectionsByFarmIdAsync(string farmId);

        bool LocalNodesIsSync();
    }
}
