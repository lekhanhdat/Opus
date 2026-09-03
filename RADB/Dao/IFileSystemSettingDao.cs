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
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IFileSystemSettingDao : IBaseDao<RMFileSystemSetting>
    {
        //List<RMSharePointSetting> LoadRunJobSetting();//Get Node have not config before.
        Task AddOrUpdateFSSettingAsync(RMFileSystemSetting fsSetting);
        Task AddOrUpdateFSSettingAsync(RMFSTreeNode node, Guid connGId);
        RMFileSystemSetting LoadFSSetting(Guid scpoeId, Guid connGId);
        List<Guid> ValidateEnableRecordManagementNodes(List<Guid> nodeIds);
        Task<bool> IsFSEnableRecordManagement(Guid scpoeId);
        Task<bool> IsFullPathConnectionExist(RMFSTreeNode sNode);
        Task<List<string>> AllDisabledRecordManagementPath();
        RMFileSystemSetting GetSettingByConnGroupId(Guid connGroupId);

        Task DeleteFileSystemSettingAsync(Guid id, Guid connGid);

        Task DeleteFSWithSubFolderSettingAsync(List<Guid> ids);

        List<string> GetBreakNodeIds(string parentId);

        Task DeacitveDescendantsSettingAsync(RMFSTreeNode node, Guid connGId);
        RMFileSystemSetting LoadInheritSetting(Guid nodeId, Guid connGId, ref Guid parentId);

        string GetTreeNodeInfoByScheduleId(ScheduleType type, string scheduleId);

        bool ResetApplyExistingOption(Guid scopeId);

        bool ResetApplyClassCodeExistingOption(Guid scopeId);

        List<RMFileSystemSetting> LoadAllSetting();

        IEnumerable<RMFileSystemSetting> LoadAllSettingByGroupIds(IEnumerable<Guid> groupIds);

        List<GCommon.Contract.StorageOptimization.Object.UserInfo> GetReocrdOwnersBySettingId(int settingId);

        List<RMFileSystemSetting> LoadAllSettingsUnderGroup(Guid groupId);

        List<RMFileSystemSetting> LoadAllSettingsUnderConnection(string connectionPath);

        List<RMFileSystemSetting> LoadAllConnectionSettingsUnderGroup(Guid groupId, IEnumerable<string> connectionPaths);

        List<RMFileSystemSetting> LoadAllSettingsByConnectionGroupIdAndConnectionPath(Guid groupId, string connectionPath);

        List<RMFileSystemSetting> GetAllDeactiveUnderGroup(Guid groupId);
        List<RecordOwnerGroupDto> GetRecordOwners(List<Guid> scopeIds);
        bool IsDeactivedNode(string profileId);
        List<string> GetAllDeactiveId();

        List<string> GetAllDisableRecordManagementPath(Guid groupId);
        List<KeyValuePair<string, bool>> GetAllNodeRCCSettings(Guid groupId);
        bool IsConnGroupEnableDownloadRCC(Guid groupId);
        List<KeyValuePair<string, bool>> GetAllDeactivePath(Guid groupId);
        bool IsConnGroupActive(Guid groupId);
        Task BatchUpdateClassCodeAsync(List<RMFileSystemSetting> settings, Guid classCodeId, string classCode, string countryCode, RetentionScheduleType retentionScheduleType, long startDate, bool applyExistDocument);
        Guid GetTermSetIdFromScopeId(Guid scopeId);
        Task UpdateRecordManagementStatus(Guid scopeId, int enableRecordManagement);
        List<RMFileSystemSetting> LoadAllSettingsByScopeIds(List<Guid> scopeIds);
        Task RemoveDescendantsSettingAsync(RMFSTreeNode node, string profileIdPath);

        //test
        Task<List<string>> GetAllDisableRecordManagementPathAsync(Guid groupId);
        Task<RMFileSystemSetting> LoadFSSettingAsync(Guid scpoeId, Guid connGId);
        Task<List<KeyValuePair<string, bool>>> GetAllNodeRCCSettingsAsync(Guid groupId);
        Task<bool> IsConnGroupEnableDownloadRCCAsync(Guid groupId);
        Task<List<KeyValuePair<string, bool>>> GetAllDeactivePathAsync(Guid groupId);
    }
}
