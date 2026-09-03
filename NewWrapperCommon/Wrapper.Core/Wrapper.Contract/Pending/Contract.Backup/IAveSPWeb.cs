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
    public interface IAveSPWeb : IDisposable
    {
        IAveWeb SPWeb { get; }
        IAveSPSite ParentSite { get; }
        string Name { get; }

        void ExportBaseInfo(IAveBackupStream output);
        /// <summary>PR Item is virtual site</summary>
        void ExportBaseInfo(IAveBackupStream output, string url);
        void ExportFeatures(IAveBackupStream output);
        void ExportSettings(IAveBackupStream output);
        void ExportSettings(IAveBackupStream output, AveBackupOption option);
        void ExportLanguageInfo(IAveBackupStream output);
        void ExportFields(IAveBackupStream output, AveBackupOption backupColumnOption = null);
        void ExportFields(IAveBackupStream output, List<string> filterFields);
        void ExportContentTypes(IAveBackupStream output, List<string> filterContentTypes = null);
        void ExportContentTypes(IAveBackupStream output, AveBackupOption backupContentTypeOption);
        void ExportEventReceivers(IAveBackupStream output);
        void ExportSearchInfo(IAveBackupStream output);
        void ExportSocialTags(IAveBackupStream output);
        void ExportSocialComments(IAveBackupStream output);
        //Added to backup social feeds
        void ExportSocialFeeds(IAveBackupStream output);
        void ExportNavigation(IAveBackupStream output, bool backupInheritedNavNodes = true, bool needFullUrl = false, string srcWebAppUrl = null);
        void ExportUsers(IAveBackupStream output, bool includeUsersWithoutSecurity = false);
        void ExportGroups(IAveBackupStream output, bool includeGroupsWithoutSecurity = false);
        void ExportRoles(IAveBackupStream output);
        void ExportRoleAssignments(IAveBackupStream output);

        void ExportWorkflows(IAveBackupStream stream, SPWebWorkflowAssociationBackupOption option);

        void ExportUserCustomActions(IAveBackupStream output);

        void ExportFullTextIndex(IAveBackupStream output, Dictionary<string, object> customFieldValues);
        void ExportPolicy(IAveBackupStream output);

        List<AveEventReceiverInfo> GetEventReceivers();
        AveFeatureInfoBox GetFeatures();
        List<AveRoleInfo> GetRoles();
        List<AveUserInfo> GetUsers(bool includeUsersWithoutSecurity = true);
        List<AveGroupInfo> GetGroupsWithAllMembers(bool includeUsersWithoutSecurity = true);

        Dictionary<int, object> GetMicroFeedCache();
        Dictionary<int, object> GetSocialThreadCache();
    }

    public class SPWebWorkflowAssociationBackupOption
    {
        public SPWebWorkflowAssociationBackupOption()
        {
            ExportWebAssociation = true;
            ExportContentTypeAssociation = true;
            ExportInstance = true;
            //BackupWorkflowAssocationToExportedFile = true;
        }

        public bool ExportWebAssociation { get; set; }

        public bool ExportContentTypeAssociation { get; set; }

        public bool ExportInstance { get; set; }

        public string NWContentDBConnectionString { set; get; }

        public string NWConfigDBConnectionString { get; set; }

        public List<string> ContentTypeFilter { get; set; }

        public Func<AveWorkflowAssociationInfo, bool> FilterFunc { get; set; }

        public bool BackupWorkflowAssocationToExportedFile { get; set; }

        public Func<AveReusableWorkflowTemplateInfo, bool> TemplateFilterFunc { get; set; }
    }
}
