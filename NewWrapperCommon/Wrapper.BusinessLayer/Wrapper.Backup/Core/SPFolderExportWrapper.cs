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
using AvePoint.Wrapper.Core.SPBackup;

namespace AvePoint.Wrapper.Backup.Core
{
    /// <summary>
    /// 封装BackupFolder，为了只备份一个folder使用，内部还是使用AveSPFolder的方法
    /// </summary>
    class SPFolderExportWrapper : ISPFolderExport
    {
        private readonly IAveFolder folder;
        private ISPSiteExport backupSite;
        private ISPWebExport backupWeb;
        private ISPListExport backupList;
        private ISPFolderExport backupFolder;
        private int version;

        public SPFolderExportWrapper(IAveFolder folder, int version)
        {
            if (folder == null)
            {
                throw new ArgumentNullException("folder");
            }

            this.folder = folder;
            this.version = version;
            Initialize();
        }

        private void Initialize()
        {
            var backupAPI = new SPBackupAPI();
            backupSite = backupAPI.CreateSPSiteExport(folder.ParentWeb.Site);
            backupWeb = backupAPI.CreateSPWebExport(backupSite, folder.ParentWeb);
            backupList = backupAPI.CreateSPListExport(backupWeb, folder.ParentList);
            //backupFolder = backupAPI.CreateSPFolderExport(folder.ParentFolder, folder, version);
        }

        public void Dispose()
        {
            backupFolder.Dispose();
            backupList.Dispose();
            backupWeb.Dispose();
            backupSite.Dispose();

            backupFolder = null;
            backupList = null;
            backupWeb = null;
            backupSite = null;
        }

        private void EnsureBackupLFolder()
        {
            if (backupFolder == null)
            {
                throw new ArgumentNullException("backupFolder");
            }
        }

        public void ExportMetadata(IAveBackupStream stream, SPItemMetadataBackupOption backupOption)
        {
            EnsureBackupLFolder();
            backupFolder.ExportMetadata(stream, backupOption);
        }

        public void ExportRoleAssignments(IAveBackupStream stream)
        {
            EnsureBackupLFolder();
            backupFolder.ExportRoleAssignments(stream);
        }

        public void ExportRoleAssignments(IAveBackupStream stream, SPRoleAssignmentsBakupOption backupOption)
        {

            EnsureBackupLFolder();
            backupFolder.ExportRoleAssignments(stream, backupOption);
        }

        public void ExportAlerts(IAveBackupStream stream)
        {
            EnsureBackupLFolder();
            backupFolder.ExportAlerts(stream);
        }

        public void ExportSocialInfos(IAveBackupStream stream)
        {
            EnsureBackupLFolder();
            backupFolder.ExportSocialInfos(stream);
        }
    }
}
