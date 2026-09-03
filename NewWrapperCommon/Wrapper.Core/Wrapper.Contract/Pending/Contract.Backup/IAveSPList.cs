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
using AvePoint.Wrapper.Core.SPBackup;

namespace AvePoint.Wrapper.Backup
{
    public interface IAveSPList : IDisposable
    {
        IAveSPSite AveSPSite { get; }
        IAveSPWeb ParentWeb { get; }
        IAveList SPList { get; }
        bool IsSystemList { get; }
        bool IsWorkflowHistoryList { get; }
        bool NeedExportExcel { get; set; }
        string ExcelPath { get; set; }
        bool BackupLookUpDisplayValue { get; set; }
        bool BackupItemTPGUIDofLookupValue { get; set; }
        /// <summary>
        /// 此option专用于需要在正常还原中需要使用File LeafName去查找Lookup对象，只对Document Library生效，外围模块请谨慎使用。
        /// </summary>
        bool BackupItemLeafNameOfLookupValue { get; set; }
        Dictionary<int, object> SocialThreadCache { get; set; }
        /// <summary>
        /// 此option专用于需要在正常还原中需要使用Column Value进行还原的情况，外围模块请谨慎使用。
        /// </summary>
        bool BackupItemLookupDisplayValueForRestore { get; set; }

        event Func<IAveField, bool> OnAddColumnToExcelFile;

        #region Properties
        Guid Id { get; }
        string Title { get; }
        string ServerRelativeUrl { get; }
        string Path { get; }

        Guid ScopeId { get; }
        #endregion

        void ExportBaseInfo(IAveBackupStream output);

        /// <summary>PR Item is virtual site</summary>
        void ExportBaseInfo(IAveBackupStream output, string url);

        /// <param name="includeAuthor">是否先备份List的Author，避免还原的时候不存在导致找不到User</param>
        void ExportSettings(IAveBackupStream output, bool includeAuthor = true);

        /// <param name="includeGroup">是否先备份User Field的Selection Group，避免还原的时候不存在导致找不到Group</param>
        void ExportFields(IAveBackupStream output, bool includeGroup = true, AveBackupOption backupColumnOption = null);

        void ExportContentTypes(IAveBackupStream output, AveBackupOption backupContentTypeOption = null);
        void ExportEventReceivers(IAveBackupStream output);
        void ExportSocialTags(IAveBackupStream output);
        void ExportSocialComments(IAveBackupStream output);

        /// <param name="includeUser">是否先备份Alert的User，避免还原的时候不存在导致找不到User</param>
        void ExportAlerts(IAveBackupStream output, bool includeUser = true);

        /// <summary>只有Raplicator需要，别的模块User不需要单独控制，会跟Permission走</summary>
        void ExportUsers(IAveBackupStream output);

        /// <summary>只有Raplicator需要，别的模块Group不需要单独控制，会跟Permission走</summary>
        void ExportGroups(IAveBackupStream output);

        /// <param name="includeUserAndGroup">是否先备份相关的User和Group，避免还原的时候不存在</param>
        void ExportRoleAssignments(IAveBackupStream output, bool includeUserAndGroup = true);

        void ExportWorkflows(IAveBackupStream stream, SPListWorkflowAssociationBackupOption option);

        void ExportFullTextIndex(IAveBackupStream output, Dictionary<string, object> customFieldValues);

        void ExportUserCustomActions(IAveBackupStream output);

        List<AveEventReceiverInfo> GetEventReceivers();
        List<AveUserInfo> GetUsers();
        List<AveGroupInfo> GetGroups();

        void ExportPolicy(IAveBackupStream output);

    }

    public class SPListWorkflowAssociationBackupOption
    {
        public SPListWorkflowAssociationBackupOption()
        {
            ExportListAssociation = true;
            ExportContentTypeAssociation = true;
            //BackupWorkflowAssocationToExportedFile = true;
        }

        public bool ExportListAssociation { get; set; }

        public bool ExportContentTypeAssociation { get; set; }

        public string NWConfigDBConnectionString { get; set; }

        public string NWContentDBConnectionString { get; set; }

        public Func<AveWorkflowAssociationInfo, bool> FilterFunc { get; set; }
        public bool BackupWorkflowAssocationToExportedFile { get; set; }
    }
}