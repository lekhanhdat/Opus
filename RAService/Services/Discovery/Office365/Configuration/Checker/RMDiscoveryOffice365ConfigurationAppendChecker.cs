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
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Configuration.Checker
{
    public class RMDiscoveryOffice365ConfigurationAppendChecker
    {
        private readonly IRMDiscoveryOffice365NodeDao _nodeDao = new RMDiscoveryOffice365NodeDao();

        private readonly List<Guid> _specifyContainerIds;

        public RMDiscoveryOffice365ConfigurationAppendChecker(List<Guid> specifyContainerIds)
        {
            _specifyContainerIds = specifyContainerIds;
        }

        public async Task<(bool Succeed, string Message)> CheckAsync()
        {
            if (!await CheckHasEnoughAppsAsync())
            {
                return (false, "RM_JM_AppProfile_NotFoundError");
            }

            return (true, string.Empty);
        }

        private async Task<bool> CheckHasEnoughAppsAsync()
        {
            var supportNodeLevels = new HashSet<NodeLevel>
            {
                NodeLevel.O365GroupSites,
                NodeLevel.SiteCollection,
                NodeLevel.SkyDrivePro
            };

            var o365TenantIds = (await _nodeDao.GetOpusO365TenantIdsByContainerAsync(supportNodeLevels.ToList(), _specifyContainerIds.ToArray())).ToHashSet();
            var avaliableApps = RMAosApiClient.GetAllProfiles(TenantLocalValue.LogonGroupId);
            var avaliableAppO365TenantIds = avaliableApps.Select(item => new Guid(item.TenantId)).ToHashSet();

            var intersectedO365TenantIds = o365TenantIds.Intersect(avaliableAppO365TenantIds).ToHashSet();
            return intersectedO365TenantIds.Count == o365TenantIds.Count;
        }
    }
}
