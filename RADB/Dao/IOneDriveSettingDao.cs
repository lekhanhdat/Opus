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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IOneDriveSettingDao : IBaseDao<RMOneDriveSetting>
    {
        Task AddOrUpdateGlobalSettingAsync(RMSPTreeNode node);
        Task AddOrUpdateCustomSettingAsync(RMSPTreeNode node, Guid siteId);
        List<RMOneDriveSetting> LoadOneDriveSettings(Guid groupId);
        RMOneDriveSetting LoadOneDriveSetting(Guid scopeId, Guid siteId);
        Task DeleteOneDriveSettingAsync(Guid id, Guid siteId);
        bool CleanSettingJobTime(RMSPTreeNode node);
        RMOneDriveSetting GetParentNode(Expression<Func<RMOneDriveSetting, bool>> whereLambda);
        List<RMOneDriveSetting> GetFolderSettingUnderList(Guid listId, Guid siteId);
        List<RMOneDriveSetting> GetSettingsByIds(string[] ids);
        List<RMOneDriveSetting> LoadAllSetting();

        List<GCommon.Contract.StorageOptimization.Object.UserInfo> GetReocrdOwnersBySettingId(int settingId);

        List<RMOneDriveSetting> GetAllGroupSettings();
        RMOneDriveSetting GetSettingInfoByScope(Guid groupId, Guid siteId, Guid scopeId);
        RMOneDriveSetting GetSettingInfoByAgentGroupId(string id);
        List<RMOneDriveSetting> LoadOneDriveSettingsUnderSite(Guid siteId);
        void CheckNeedRemoveDescendantsSetting(RMSPTreeNode node, string profileIdPath);
        Task SetSettingJobTimeAsync(Guid scopeId, Guid siteId);
        string GetMetadataColumn(Guid nodeId);
        bool GetSettingEnableInfoByScope(Guid groupId, Guid siteId, Guid scopeId);
        List<RMOneDriveSetting> GetDescendantsDisableNodes(RMSPTreeNode node);
        List<RMOneDriveSetting> LoadShowUniqueIdSetting();
        List<RMOneDriveSetting> LoadGroupSetting(bool isRecheckRule = true);
        RMOneDriveSetting LoadOneSiteSettingEnableManualApprovalFirst();

        bool ExistShowUniqueIdSetting();
    }
}
