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
using System.Text;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Core.SPBackup
{
    public delegate void SetSiteBaseInfoAction(AveSiteInfo info);

    public interface ISPSiteExport : IDisposable
    {
        AveLanguageProcesser LanguageProcessor{ get; set; }

        /// <summary>backup Audience</summary>
        void ExportAudience(IAveBackupStream output);

        /// <summary>backup BaseInfo</summary>
        void ExportBaseInfo(IAveBackupStream output);

        /// <summary>backup BaseInfo, 在setSiteBaseInfo里面修改SiteBaseInfo的数据</summary>
        void ExportBaseInfo(IAveBackupStream stream, SetSiteBaseInfoAction setSiteBaseInfo);

        /// <summary>backup Features</summary>
        void ExportFeatures(IAveBackupStream output);

        /// <summary>backup LanguageInfo</summary>
        void ExportLanguageInfo(IAveBackupStream output);

        /// <summary>backup site managed metadata</summary>
        void ExportManagedMetadata(IAveBackupStream output, SPSiteManagedMetadataBackupOption backupOption);

        /// <summary>backup SearchInfo</summary>
        void ExportSearchInfo(IAveBackupStream output);

        /// <summary>backup Site Property</summary>
        void ExportSettings(IAveBackupStream output);

        /// <summary>
        /// backup User Profiles
        /// allUsers == true: 备份所有的user。
        /// allUsers == false： 判断是否是site的ownner，如果是则备份ownner的信息，不是则什么也不备份。
        /// </summary>
        void ExportUserProfiles(IAveBackupStream output, bool allUsers);

        /// <summary>
        /// backup groups and include groups without permission
        /// </summary>
        /// <param name="output"></param>
        void ExportGroups(IAveBackupStream output);

        /// <summary>
        /// backup Groups
        /// includeGroupsWithoutSecurity == true: 备份所有的Group。
        /// includeGroupsWithoutSecurity == false: 备份有具有role的Group
        /// </summary>
        void ExportGroups(IAveBackupStream output, bool includeGroupsWithoutSecurity);

        /// <summary>
        /// backup Users and include users without permission
        /// </summary>
        /// <param name="output"></param>
        void ExportUsers(IAveBackupStream output);

        /// <summary>
        /// backup Users
        /// includeUsersWithoutSecurity == true: 备份所有的Users。
        /// includeUsersWithoutSecurity == false: 备份有具有role的Users。
        /// </summary>
        void ExportUsers(IAveBackupStream output, bool includeUsersWithoutSecurity);
    }
}
