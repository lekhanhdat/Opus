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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;

namespace AvePoint.RA.Service.Services.Dashboard.Query
{
    public class TeamsDashboardQuery : DashboardQueryable
    {
        public override SourceFlag Flag => SourceFlag.Teams;
        private IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        public override long GetApplySettingCount(SecurityUserPermissionsDto permission)
        {
            if (permission.IsAdmin)
            {
                return DashboardDao.GetTeamsSettingCount();
            }
            var containerIds = permission.ScopePermissionInfo.Find(item => item.DataSourceType == Flag)?.ScopeIds;
            return DashboardDao.GetTeamsSettingCount(containerIds);
        }

        public override long GetSourceActiveCount(SecurityUserPermissionsDto permission)
        {
            if (permission.IsAdmin)
            {
                return DashboardDao.GetSourceActiveCount(Flag);
            }
            var containerIds = permission.ScopePermissionInfo.Find(item => item.DataSourceType == Flag)?.ScopeIds.ConvertAll(item => item.ToString());
            var siteUrlUnderTeams = GetChannelSiteUnderTeams(containerIds);
            return DashboardDao.GetSourceActiveCount(Flag, containerIds, siteUrlUnderTeams);
        }

        public override long GetSourceDestroyedCount(SecurityUserPermissionsDto permission)
        {
            if (permission.IsAdmin)
            {
                return DashboardDao.GetSourceDestroyedCount(Flag);
            }
            var containerIds = permission.ScopePermissionInfo.Find(item => item.DataSourceType == Flag)?.ScopeIds.ConvertAll(item => item.ToString());
            var siteUrlUnderTeams = GetChannelSiteUnderTeams(containerIds);
            return DashboardDao.GetSourceDestroyedCount(Flag, containerIds, siteUrlUnderTeams);
        }

        public override long GetSourceArchivedCount(SecurityUserPermissionsDto permission)
        {
            if (permission.IsAdmin)
            {
                return DashboardDao.GetSourceArchivedCount(Flag);
            }
            var containerIds = permission.ScopePermissionInfo.Find(item => item.DataSourceType == Flag)?.ScopeIds.ConvertAll(item => item.ToString());
            var siteUrlUnderTeams = GetChannelSiteUnderTeams(containerIds);
            return DashboardDao.GetSourceArchivedCount(Flag, containerIds, siteUrlUnderTeams);
        }

        private List<string> GetChannelSiteUnderTeams(List<string> containerIds)
        {
            var siteUrlUnderTeams = new List<string>();
            var teamsIds = RemoteNodeDao.GetTeamsIdByContainerId(containerIds);
            var dicNodes = RemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsIds(teamsIds, true);
            foreach (var node in dicNodes)
            {
                siteUrlUnderTeams.AddRange(node.Value?.Select(_ => _.url) ?? new List<string>());
            }
            return siteUrlUnderTeams;
        }
    }
}
