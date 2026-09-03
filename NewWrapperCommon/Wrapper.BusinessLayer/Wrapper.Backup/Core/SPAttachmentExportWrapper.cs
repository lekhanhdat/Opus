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

namespace AvePoint.Wrapper.Backup.Core
{
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Core.SPBackup;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// 封装BackupAttachment，为了只备份一个attachment使用，内部还是使用AveSPAttachment的方法。wrapper内部使用。
    /// </summary>
    class SPAttachmentExportWrapper : ISPAttachmentExport
    {
        private readonly IAveAttachment attachment;
        private readonly IAveListItem listItem;
        private ISPSiteExport backupSite;
        private ISPWebExport backupWeb;
        private ISPListExport backupList;
        private ISPFolderExport backupFolder;
        private ISPAttachmentExport backupAttachment;

        public SPAttachmentExportWrapper(IAveListItem listItem, IAveAttachment attachment)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException("attachment");
            }

            this.attachment = attachment;
            this.listItem = listItem;

            Initialize();
        }

        private void Initialize()
        {
            var backupAPI = new SPBackupAPI();
            backupSite = backupAPI.CreateSPSiteExport(listItem.Web.Site);
            backupWeb = backupAPI.CreateSPWebExport(backupSite, listItem.Web);
            backupList = backupAPI.CreateSPListExport(backupWeb, listItem.ParentList);
            backupFolder = backupAPI.CreateSPFolderExport(backupList, attachment.GetParentId());
            backupAttachment = backupAPI.CreateSPAttachmentExport(backupFolder, attachment);
        }

        public void Dispose()
        {
            backupAttachment.Dispose();

            backupAttachment = null;
        }

        private void EnsureBackupAttachment()
        {
            if (backupAttachment == null)
            {
                throw new ArgumentNullException("backupAttachment");
            }
        }

        public void ExportContent(IAveBackupStream stream)
        {
            EnsureBackupAttachment();
            backupAttachment.ExportContent(stream);
        }

        public void ExportDocInfo(IAveBackupStream stream)
        {
            EnsureBackupAttachment();
            backupAttachment.ExportDocInfo(stream);
        }

        public void ExportStorgeInfo(IAveBackupStream stream)
        {
            EnsureBackupAttachment();
            backupAttachment.ExportStorgeInfo(stream);
        }
    }
}
