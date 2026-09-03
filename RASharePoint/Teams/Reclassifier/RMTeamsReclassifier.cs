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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using System;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.RMExplorer.RMReclassifier;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Common;

namespace AvePoint.RA.SharePoint.Teams.Reclassifier
{
    public class RMTeamsReclassifier : RMSPReclassifier
    {
        private readonly ITeamsSettingDao TeamsSettingDao = PlatformWindsorManager.GetService<ITeamsSettingDao>();
        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        public RMTeamsReclassifier(ChangeTermDto dto) : base(dto, SourceFlag.Teams)
        {
        }

        protected override string GetBCSColumn(RemoteSiteCollection site)
        {
            var teamNode = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(site.TeamId).Item1;
            var groupLevelSetting = TeamsSettingDao.GetSettingInfoByScope(new Guid(teamNode.parentId), Guid.Empty, Guid.Empty, new Guid(teamNode.parentId));
            var columnName = groupLevelSetting.IsUsingExistColumnName ? groupLevelSetting.ExistColumnName : groupLevelSetting.ColumnName;
            return columnName;
        }
    }
}
