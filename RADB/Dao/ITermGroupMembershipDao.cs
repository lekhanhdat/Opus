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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface ITermGroupMembershipDao : IBaseDao<RMTermGroupMembership>
    {
        void DeleteTermGroupInfo(Guid termGroupId, Guid termStoreId);
        List<RMTermGroupMembership> GetTermGroupInfoById(Guid termGroupId);
        bool ExistTermGroupInfo(Guid termGroupId, Guid termStoreId);
        bool ExistTermGroupInfo(Guid termGroupId, string googleTenant);
        RMTermGroupMembership GetTermGroupInfo(Guid termGroupId, Guid termStoreId);
        List<RMTermGroupMembership> GetTermGroupsByAgentGroupId(string id);
        void AddTermGroupInfo(Guid termGroupId, string url, string displayName, string termStoreName, Guid termStoreId, string agentGroupId, SiteType siteType);
        Task AddGoogleTenantTermGroup(Guid termGroupId, string url, string displayName, string termStoreName, Guid termStoreId, string agentGroupId, SiteType siteType);
        Task UpdateTermGroupInfoAsync(int id, Guid termGroupId, string url, string displayName, string termStoreName, Guid termStoreId, string agentGroupId, SiteType siteType);
        List<RMTermGroupMembership> GetOtherGroupsByAgentGroupIdAndTermGroupId(string id, Guid termGroupId);
        List<RMTermGroupMembership> GetAllTermGroupMembership();
        List<Guid> GetTermStoreIdsByTermGroupId(Guid termGroupId, SiteType siteType);
        List<RMTermGroupMembership> GetTermGroupMembershipByTermGroupId(Guid termGroupId, SiteType siteType);
        Dictionary<Guid, List<Guid>> GetTermStoreAndGroupIdMapping();
        List<string> GetAllSpecifiedSites(SiteType siteType);
        Task<Dictionary<string,string>> GetGoogleTenantsExisted(List<string> googleTenants, Guid termGroupId);        
        Task DeleteGoogleTenantsByTermGroupId(Guid termGroupId);
        Task DeleteGoogleTenantsByTermGroupIdAndSiteUrl(List<string> googleTenants, Guid termGroupId);
        Task<List<RMTermGroupMembership>> GetGoogleTermGroupMemberships();
        Task AddGoogleTenantInTermGroupMembership(RMTermGroupMembership termGrMembership);
        Task<List<string>> GetTermGroupsBySiteUrlGroupIds(List<string> siteUrls);
        Task<IEnumerable<RMTermGroupMembership>> LoadByPager(int pageIndex, int pageSize);
        Task<long> MultiGeoInsertTermGroupMembershipTableAsync(IEnumerable<RMTermGroupMembership> termGroupMemberships);
        Task<long> MultiGeoDeleteAllTermGroupMembershipAsync();
    }
}
