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
    public interface ISharePointOnPremiseSettingDao : IBaseDao<RMSharePointOnPremiseSetting>
    {
        Task AddOrUpdateGlobalSettingAsync(RMSPTreeNode node);
        Task AddOrUpdateCustomSettingAsync(RMSPTreeNode node, Guid siteId);
        void UpdateBCSColumnName(Guid groupId, string bcsColumnName, string bcsColumnDescription, bool columnRequired = true);
        List<GCommon.Contract.StorageOptimization.Object.UserInfo> GetReocrdOwnersBySettingId(int settingId);
        List<RMSharePointOnPremiseSetting> LoadSharePointSettings(Guid groupId);
        RMSharePointOnPremiseSetting LoadSharePointSetting(Guid scopeId, Guid siteId);
        RMSharePointOnPremiseSetting GetGroupLevelSetting(string groupName, Guid scopeId);
        RMSharePointOnPremiseSetting GetSiteLevelSetting(string fullPath, Guid scopeId);
        bool IsUsingExistingColumnByGroupIds(List<Guid> ids);
        Task AddOrUpdateGlobalSettingUsingExistColumnAsync(RMSPTreeNode node);
        void CheckNeedRemoveDescendantsSetting(RMSPTreeNode node, string profileIdPath);
        Task DeleteSharePointSettingAsync(Guid id, Guid siteId);
        bool CleanSettingJobTime(RMSPTreeNode node);
        RMSharePointOnPremiseSetting GetParentNode(Expression<Func<RMSharePointOnPremiseSetting, bool>> whereLambda);
        RMSharePointOnPremiseSetting GetSettingInfoByAgentGroupId(string id);
        List<RMSharePointOnPremiseSetting> LoadRunJobSetting();//Get Node have not config before.
        List<RMSharePointOnPremiseSetting> LoadAllSetting();
        List<RMSharePointOnPremiseSetting> LoadExcludeSiteCollectionSetting();
        Task<bool> SetSettingJobTimeAsync(Guid scopeId, Guid siteId, bool isFailedColumn, bool isFailedProperty);
        Task<bool> SetSettingJobTimeAsync(Guid scopeId, bool isFailedConfigColumn, bool isFailedConfigProperty);
        List<RMSharePointOnPremiseSetting> GetColumnInfos(string[] ids);
        RMSharePointOnPremiseSetting GetSettingInfoByScope(Guid groupId, Guid siteId, Guid scopeId);
        List<RMSharePointOnPremiseSetting> LoadShowUniqueIdSetting();
        bool ExistShowUniqueIdSetting();
        Dictionary<Guid, bool> GetWebEnableManagementSettingInfo(Guid groupId, Guid siteId);
    }
}
