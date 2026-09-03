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
    /// 封装BackupListItem，为了只备份一个listItem使用，内部还是使用AveSPListItem的方法
    /// </summary>
    class SPListItemExportWrapper : ISPItemExport
    {
        private readonly IAveListItem listItem;
        private ISPSiteExport backupSite;
        private ISPWebExport backupWeb;
        private ISPListExport backupList;
        private ISPItemExport backupListItem;
        private int version;

        public SPListItemExportWrapper(IAveListItem listItem, int version)
        {
            if (listItem == null)
            {
                throw new ArgumentNullException("listItem");
            }
            if (version <= 0)
            {
                throw new ArgumentOutOfRangeException("version");
            }

            this.listItem = listItem;
            this.version = version;
            Initialize();
        }

        private void Initialize()
        {
            var backupAPI = new SPBackupAPI();
            backupSite = backupAPI.CreateSPSiteExport(listItem.Web.Site);
            backupWeb = backupAPI.CreateSPWebExport(backupSite, listItem.Web);
            backupList = backupAPI.CreateSPListExport(backupWeb, listItem.ParentList);
            backupListItem = backupAPI.CreateSPListItemExport(backupList, listItem, this.version);
        }

        public void Dispose()
        {
            backupListItem.Dispose();
            backupList.Dispose();
            backupWeb.Dispose();
            backupSite.Dispose();

            backupListItem = null;
            backupList = null;
            backupWeb = null;
            backupSite = null;
        }

        private void EnsureBackupListItem()
        {
            if (backupListItem == null)
            {
                throw new ArgumentNullException("backupListItem");
            }
        }

        public void ExportMetadata(IAveBackupStream stream, SPItemMetadataBackupOption backupOption)
        {
            EnsureBackupListItem();
            backupListItem.ExportMetadata(stream, backupOption);
        }

        public void ExportRoleAssignments(IAveBackupStream stream)
        {
            EnsureBackupListItem();
            backupListItem.ExportRoleAssignments(stream);
        }

        public void ExportRoleAssignments(IAveBackupStream stream, SPRoleAssignmentsBakupOption backupOption)
        {

            EnsureBackupListItem();
            backupListItem.ExportRoleAssignments(stream, backupOption);
        }

        public void ExportAlerts(IAveBackupStream stream)
        {
            EnsureBackupListItem();
            backupListItem.ExportAlerts(stream);
        }

        public void ExportSocialInfos(IAveBackupStream stream)
        {
            EnsureBackupListItem();
            backupListItem.ExportSocialInfos(stream);
        }
    }
}
