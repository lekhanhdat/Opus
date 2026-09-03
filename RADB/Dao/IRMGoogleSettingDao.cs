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
using System.Linq.Expressions;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMGoogleSettingDao : IBaseDao<RMGoogleSetting>
    {
        RMGoogleSetting GetSettingInfoByScope(Guid containerId, Guid scopeId, Guid driveId);
        List<RMGoogleSetting> GetAllSettings();
        List<RMGoogleSetting> GetDriveNodeLevelSettings();
        List<RMGoogleSetting> GetRunJobSetting();
        Task SetSettingJobTimeWithContainerIdAsync(Guid containerId, Guid scopeId);
        Task<RMGoogleSetting> GetSettingInfo(Guid containerId, Guid driveId);
        RMGoogleSetting GetParentNode(Expression<Func<RMGoogleSetting, bool>> whereLambda);
        Task AddOrUpdateCustomSettingAsync(RMGoogleTreeNode node, Guid driveId);
        RMGoogleSetting GetSettingInfoByAgentId(string id);
        Task<List<RMGoogleSetting>> GetSettingsByExpression(Expression<Func<RMGoogleSetting, bool>> whereLambda);

        Task DeleteGoogleSettingAsync(Guid id);

        Task CheckNeedRemoveDescendantsSetting(RMGoogleTreeNode settingNode, string nodeProfileIdPath);
        Task UpdateLabelNameSettingAsync(RMDbContext ctx, string uniqueLabelId, string newName);

        public List<string> GetUnSyncableNodeIdsByContainerId(Guid containerId);
        List<RMGoogleSetting> GetSettingInforDrive(Guid containerId);

        Task<List<RMSimpleRule>> GetGoogleDriveMappingRules(Guid driveId);

        Task SaveGoogleSettingMappingRule(RMGoogleTreeNode node);

        Task UpdateEnableRecordManagement(RMGoogleTreeNode node);
    }
}