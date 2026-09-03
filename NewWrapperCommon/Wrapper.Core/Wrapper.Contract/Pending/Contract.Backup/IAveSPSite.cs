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
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    public interface IAveSPSite : IDisposable
    {
        IAveSite SPSite { get; }
        AveObjectModelFactory ObjectModelFactory { get; }

        void SetLanguageMappingProcesser(AveLanguageProcesser processer);
        //AveUserInfo GetUserInfo(int userId);
        //object GetPrincipalInfo(int principalId);
        string GetScopeUrlByScopeId(Guid scopeId);
        int GetCheckOutUserId(AveBaseItemInfo itemInfo);

        void ExportBaseInfo(IAveBackupStream output);

        /// <summary>
        /// PR Item is virtual site
        /// </summary>
        void ExportBaseInfo(IAveBackupStream output, string url, string webappUrl, bool isHostHeader, string webTemplate= null);
        void ExportFeatures(IAveBackupStream output);
        void ExportSettings(IAveBackupStream output);
        void ExportSearchInfo(IAveBackupStream output);
        void ExportLanguageInfo(IAveBackupStream output);

        /// <summary>
        /// includeUsersWithoutSecurity == true 对应 AveSiteUserQueryOption.AllUsers
        /// includeUsersWithoutSecurity == false 对应 AveSiteUserQueryOption.OnlyHaveSecurityUsers
        /// </summary>
        /// <param name="output"></param>
        /// <param name="includeUsersWithoutSecurity"></param>
        [Obsolete("Replace with ExportUsers(IAveBackupStream output, AveUserBackupOption option)")]
        void ExportUsers(IAveBackupStream output, bool includeUsersWithoutSecurity = true);

        void ExportUsers(IAveBackupStream output, AveUserBackupOption option);

        void ExportGroups(IAveBackupStream output, bool includeGroupsWithoutSecurity = true);
        void ExportUserProfiles(IAveBackupStream output, bool allUsers);
        void ExportAudience(IAveBackupStream output);
        void ExportManagedMetadata(IAveBackupStream output, bool includeGlobalTermGroup = true, bool enableCache = false);
        void ExportFullTextIndex(IAveBackupStream output, Dictionary<string, object> customFieldValues);
        void ExportVariationSetting(IAveBackupStream output);
        void ExportSEOSetting(IAveBackupStream output);
        void ExportUserCustomActions(IAveBackupStream output);
        AveFeatureInfoBox GetFeatures();

        /// <summary>
        /// includeUsersWithoutSecurity == true 对应 AveSiteUserQueryOption.AllUsers
        /// includeUsersWithoutSecurity == false 对应 AveSiteUserQueryOption.OnlyHaveSecurityUsers
        /// </summary>
        /// <param name="includeUsersWithoutSecurity"></param>
        /// <returns></returns>
        [Obsolete("Replace with GetUsers(AveUserBackupOption option)")]
        List<AveUserInfo> GetUsers(bool includeUsersWithoutSecurity = true);
        
        List<AveUserInfo> GetUsers(AveUserBackupOption option);

        List<AveGroupInfo> GetGroupsWithAllMembers(bool includeUsersWithoutSecurity = true);
        void SetBackupOption(AveBackupOption option);
    }
}
