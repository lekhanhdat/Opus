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
using AvePoint.RA.Contract.Discovery.Model.PlanProfile;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.Plan;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.PlanProfile
{
    public interface IRMDiscoveryPlanSiteMappingDao
    {
        Task<List<string>> GetNodeIdsByPlanProfileIdAsync(int planProfileId);
        Task DeleteByPlanProfileIdAsync(int planProfileId);
        Task DeleteByPlanProfileIdsAsync(List<int> planProfileIds);
        Task InsertMappingsAsync(int planProfileId, List<string> nodeIds, RMDiscoveryPlanSiteType siteType);
        Task UpdateMappingsAsync(int planProfileId, List<SiteMappingRequest> siteMappings);
        Task<int> GetSiteMappingTypeAsync(int planProfileId);
        Task<int> GetTotalMappingSitesAsync(int planProfileId);
        Task<List<string>> GetNodeIdsByProfileId(int planProfileId);
    }
}