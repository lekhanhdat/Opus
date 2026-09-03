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
    /// 封装BackupFile，为了只备份一个文件使用，内部还是使用AveSPDocument的方法
    /// </summary>
    class SPFileExportWrapper : ISPFileExport
    {
        private readonly IAveFile file;
        private ISPSiteExport backupSite;
        private ISPWebExport backupWeb;
        private ISPListExport backupList;
        private ISPFileExport backupFile;
        private int version;

        public SPFileExportWrapper(IAveFile file, int version)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }
            if (version <= 0)
            {
                throw new ArgumentOutOfRangeException("version");
            }

            this.file = file;
            this.version = version;
            Initialize();
        }

        private void Initialize()
        {
            var backupAPI = new SPBackupAPI();
            backupSite = backupAPI.CreateSPSiteExport(file.ParentFolder.ParentWeb.Site);
            backupWeb = backupAPI.CreateSPWebExport(backupSite, file.ParentFolder.ParentWeb);
            backupList = backupAPI.CreateSPListExport(backupWeb, file.ParentFolder.ParentList);
            backupFile = backupAPI.CreateSPFileExport(backupList, file, this.version);
        }

        public void Dispose()
        {
            backupFile.Dispose();
            backupList.Dispose();
            backupWeb.Dispose();
            backupSite.Dispose();

            backupFile = null;
            backupList = null;
            backupWeb = null;
            backupSite = null;
        }

        private void EnsureBackupFile()
        {
            if (backupFile == null)
            {
                throw new ArgumentNullException("backupFile");
            }
        }

        public void ExportMetadata(IAveBackupStream stream, SPItemMetadataBackupOption backupOption)
        {
            EnsureBackupFile();
            backupFile.ExportMetadata(stream, backupOption);
        }

        public void ExportRoleAssignments(IAveBackupStream stream)
        {
            EnsureBackupFile();
            backupFile.ExportRoleAssignments(stream);
        }

        public void ExportRoleAssignments(IAveBackupStream stream, SPRoleAssignmentsBakupOption backupOption)
        {
            throw new NotImplementedException();
        }

        public void ExportContent(IAveBackupStream stream)
        {
            EnsureBackupFile();
            backupFile.ExportContent(stream);
        }

        public void ExportContent(IAveBackupStream stream, bool forceBackup)
        {
            EnsureBackupFile();
            backupFile.ExportContent(stream, forceBackup);
        }

        public void ExportAlerts(IAveBackupStream stream)
        {
            EnsureBackupFile();
            backupFile.ExportAlerts(stream);
        }

        public void ExportSocialInfos(IAveBackupStream stream)
        {
            EnsureBackupFile();
            backupFile.ExportSocialInfos(stream);
        }
    }
}
