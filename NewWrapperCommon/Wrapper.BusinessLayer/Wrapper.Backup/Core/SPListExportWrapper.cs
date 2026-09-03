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
    /// 封装BackupList，为了只备份一个list使用，内部还是使用AveSPList的方法
    /// </summary>
    internal class SPListExportWrapper : ISPListExport, IDisposable
    {
        private readonly IAveList list;
        private ISPSiteExport backupSite;
        private ISPWebExport backupWeb;
        private ISPListExport backupList;

        public SPListExportWrapper(IAveList list)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            this.list = list;

            Initialize();
        }

        private void Initialize()
        {
            var backupAPI = new SPBackupAPI();
            backupSite = backupAPI.CreateSPSiteExport(list.ParentWeb.Site);
            backupWeb = backupAPI.CreateSPWebExport(backupSite, list.ParentWeb);
            backupList = backupAPI.CreateSPListExport(backupWeb, list);
        }

        public void Dispose()
        {
            backupList.Dispose();
            backupWeb.Dispose();
            backupSite.Dispose();

            backupList = null;
            backupWeb = null;
            backupSite = null;
        }

        private void EnsureBackupList()
        {
            if (backupList == null)
            {
                throw new ArgumentNullException("backupList");
            }
        }

        public void ExportRoleAssignments(IAveBackupStream stream)
        {
            EnsureBackupList();
            backupList.ExportRoleAssignments(stream);
        }

        public void ExportRoleAssignments(IAveBackupStream stream, SPRoleAssignmentsBakupOption backupOption)
        {
            EnsureBackupList();
            backupList.ExportRoleAssignments(stream, backupOption);
        }

        public void ExportAlerts(IAveBackupStream stream)
        {
            EnsureBackupList();
            backupList.ExportAlerts(stream);
        }

        public void ExportSocialInfos(IAveBackupStream stream)
        {
            EnsureBackupList();
            backupList.ExportSocialInfos(stream);
        }

        public void ExportBaseInfo(IAveBackupStream stream)
        {
            ExportBaseInfo(stream, null);
        }

        public void ExportBaseInfo(IAveBackupStream stream, SetListInfoAction setListInfo)
        {
            EnsureBackupList();
            backupSite.ExportBaseInfo(stream);
            stream.EndWriteMetadata();
            stream.FlushMetadata(0);
            stream.BeginWriteMetadata();
            backupWeb.ExportBaseInfo(stream);
            stream.EndWriteMetadata();
            stream.FlushMetadata(0);
            stream.BeginWriteMetadata();
            if (setListInfo != null)
            {
                backupList.ExportBaseInfo(stream, setListInfo);
            }
            else
            {
                backupList.ExportBaseInfo(stream);
            }
        }

        public void ExportContentTypes(IAveBackupStream stream, SPContentTypeBackupOption backupContentTypeOption)
        {
            EnsureBackupList();
            backupList.ExportContentTypes(stream, backupContentTypeOption);
        }

        public void ExportEventReceivers(IAveBackupStream stream)
        {
            EnsureBackupList();
            backupList.ExportEventReceivers(stream);
        }

        public void ExportFullTextIndex(IAveBackupStream stream, Dictionary<string, object> customFieldValues)
        {
            EnsureBackupList();
            backupList.ExportFullTextIndex(stream, customFieldValues);
        }

        public void ExportFields(IAveBackupStream stream, SPListFieldBackupOption backupOption)
        {
            EnsureBackupList();
            backupList.ExportFields(stream, backupOption);
        }

        public void ExportSettings(IAveBackupStream stream, bool includeAuthor = true)
        {
            EnsureBackupList();
            backupList.ExportSettings(stream, includeAuthor);
        }
    }
}