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

namespace AvePoint.GCommon.Contract.CloudAppAdmin
{
    using AvePoint.GCommon.Contract.CloudAppAdmin.Message;
    using AvePoint.GCommon.Contract.CloudAppAdmin.Object;
    using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
    using System.Collections.Generic;

    public interface IMCloudAppAdminService
    {
        SearchUserMessage SearchUsers(SearchUserMessage message);

        SearchGroupMessage SearchGroups(SearchGroupMessage message);

        OperationResult UpdateUsers(UpdateUserMessage message);

        OperationResult UpdateGroups(UpdateGroupMessage message);

        OperationResult AssignUserApplication(AssignUserApplicationMessage message);

        OperationResult AssignUserLicense(AssignUserLicenseMessage message);

        OperationResult AssignGroupApplication(AssignGroupApplicationMessage message);

        OperationResult AssignGroupLicense(AssignGroupLicenseMessage message);

        OperationResult ManageUserMailBoxAccess(ManageUserEmailAccessMessage message);

        List<ADApplication> LoadApplications(string tenantId);

        List<ADLicense> LoadLicenses(string tenantId, bool isFilter);
        FilterDefaultValueMessage LoadFilterDefaultValue(string tenantId);
        OperationResult ManageUserGroup(ManageUserGroupMessage message);

        OperationResult SaveProfile(ProfileMessage message);

        List<SimpleProfileDto> LoadProfiles(ProfileType type, string tenantId);

        List<SimpleProfileDto> LoadConflictProfiles(string tenantId, List<string> peProfileIds);

        ProfileMessage LoadProfileDetail(SimpleProfileDto dto, string tenantId, bool isRealTime);

        ProfileMessage LoadPEResultProfileDetail(string peProfileId, string tenantId, bool isWhatIfResult);

        OperationResult DeleteProfiles(List<string> profileIds);

        LoadUserDetailMessage LoadUserDetails(LoadUserDetailMessage message);

        List<ADGroup> LoadGroupDetails(string tenantId, List<ADGroup> groupList, CAALoadDetailType type);

        BrowseADMessage BrowseADDetails(BrowseADMessage message);

        List<ADUser> LoadUsersFromRecycleBin(SearchUserMessage message);

        OperationResult TestCredentialValid(ProfileMessage message);

        Dictionary<string, string> GetTenantNames();

        List<TempUserInfo> LoadTempUsers(string tenantId);

        OperationResult EditTempUsers(List<TempUserInfo> tempUsers);

        bool CheckTempUserForGAO(string tenantId, List<string> usernames);

        void ExportFileToLocation(CAAExportParameter parameters);

        List<string> LoadApplicationNames(string tenantId);

        List<string> LoadLicenseNames(string tenantId);
        List<ADUser> RefreshUsersetAndSaveToProfile(string tenantId, string profileId);
        List<ADGroup> RefreshGroupsetAndSaveToProfile(string tenantId, string profileId);
        OperationResult BatchAddUsers(BatchUpdateUserMessage message);

        OperationResult BatchAddGroups(BatchUpdateGroupMessage message);

        CAAOperationResultMessage GetOperationResult(string sessionId);

        OperationResult DeleteDBRecord(string sessionId);

        bool CheckIsO365AD(string tenantId);
        OperationResult SaveAndRunPENow(ProfileMessage message);
        OperationResult RunPE(string profileId);
        OperationResult RunWhatIfPE(string profileId);
        OperationResult RunWhatIfReport(ProfileMessage message);
        OperationResult RunPEConflict(List<string> profileIDs);
        OperationResult DeletePEProfiles(List<string> profileIDs);
        ProfileMessage LoadProfileDetailForMultiple(List<SimpleProfileDto> dtos, string tenantId, bool isRealTime);
        ProfileMessage LoadProfileDetailWithKeepAlive(string sessionId, SimpleProfileDto dto, string tenantId, bool isRealTime);

        void ClearTenantCache(string tenantId);

        List<ADExtensionProperty> LoadCustomFields(string tenantId);
    }
}