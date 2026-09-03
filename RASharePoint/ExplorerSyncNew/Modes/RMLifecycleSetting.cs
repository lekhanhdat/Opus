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
using System;
using System.Collections.Generic;
using System.Linq;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.Common;

namespace AvePoint.RA.SharePoint.ExplorerSyncNew.Modes
{
    public class RMLifecycleSetting
    {
        public Guid ScopeId { set; get; }
        public Guid SiteGroupId { set; get; }
        public Guid TeamsId { set; get; }
        public Guid SiteId { set; get; }
        public Guid WebId { set; get; }
        public Guid ListId { set; get; }
        public Guid FolderId { set; get; }
        public bool IsInheritParentTerm { get; set; }
        public bool IsChangedInheritParentTerm { get; set; }
        public bool EnableLifecycleManagementForSharePointLists { get; set; } = true;
        public string FullPath { get; set; }

        public static RMLifecycleSetting FromSharePointSetting(RMSharePointSetting spSetting)
        {
            if (spSetting == null) return null;
            var node = SPCommonUtility.DeserializeTreeNodeInfo(spSetting.NodeInfo);
            return new RMLifecycleSetting
            {
                ScopeId = spSetting.ScopeId,
                SiteGroupId = spSetting.SiteGroupId,
                TeamsId = Guid.Empty,
                SiteId = spSetting.SiteId,
                WebId = spSetting.WebId,
                ListId = spSetting.ListId,
                FolderId = spSetting.FolderId,
                IsInheritParentTerm = spSetting.IsInheritParentTerm,
                EnableLifecycleManagementForSharePointLists = node?.EnableLifecycleManagementForSharePointLists ?? true,
                FullPath = spSetting.FullPath,
            };
        }

        public static List<RMLifecycleSetting> FromSharePointSetting(List<RMSharePointSetting> spSettings)
        {
            if (spSettings == null || spSettings.Count == 0) return new List<RMLifecycleSetting>();
            return spSettings.Select(spSetting => FromSharePointSetting(spSetting)).ToList();
        }

        public static RMLifecycleSetting FromTeamsSetting(RMTeamsSetting teamsSetting)
        {
            if (teamsSetting == null) return null;
            var node = SPCommonUtility.DeserializeTreeNodeInfo(teamsSetting.NodeInfo);
            return new RMLifecycleSetting
            {
                ScopeId = teamsSetting.ScopeId,
                SiteGroupId = teamsSetting.TeamsGroupId,
                TeamsId = teamsSetting.TeamsId,
                SiteId = teamsSetting.SiteId,
                WebId = teamsSetting.WebId,
                ListId = teamsSetting.ListId,
                FolderId = teamsSetting.FolderId,
                IsInheritParentTerm = teamsSetting.IsInheritParentTerm,
                EnableLifecycleManagementForSharePointLists = node?.EnableLifecycleManagementForSharePointLists ?? true,
                FullPath = teamsSetting.FullPath,
            };
        }

        public static List<RMLifecycleSetting> FromTeamsSetting(List<RMTeamsSetting> teamsSettings)
        {
            if (teamsSettings == null || teamsSettings.Count == 0) return new List<RMLifecycleSetting>();
            return teamsSettings.Select(teamsSetting => FromTeamsSetting(teamsSetting)).ToList();
        }
    }
}