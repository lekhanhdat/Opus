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

using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Core.SPBackup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Backup.Core
{
    internal class SPWebExportWrapper : ISPWebExport
    {
        private readonly IAveWeb web;
        private ISPSiteExport backupSite;
        private ISPWebExport backupWeb;

        public SPWebExportWrapper(IAveWeb web)
        {
            if (web == null)
            {
                throw new ArgumentNullException("web");
            }

            this.web = web;

            Initialize();
        }

        private void Initialize()
        {
            var backupAPI = new SPBackupAPI();
            backupSite = backupAPI.CreateSPSiteExport(web.Site);
            backupWeb = backupAPI.CreateSPWebExport(backupSite, web);
        }

        public void Dispose()
        {
            backupWeb.Dispose();
            backupSite.Dispose();

            backupWeb = null;
            backupSite = null;
        }

        private void EnsureBackupWeb()
        {
            if (backupWeb == null)
            {
                throw new ArgumentNullException("backupWeb");
            }
        }

        public void ExportBaseInfo(IAveBackupStream stream)
        {
            ExportBaseInfo(stream, null);
        }

        public void ExportBaseInfo(IAveBackupStream stream, SetWebInfoAction setWebInfo)
        {
            EnsureBackupWeb();
            backupSite.ExportBaseInfo(stream);
            stream.EndWriteMetadata();
            stream.FlushMetadata(0);
            stream.BeginWriteMetadata();
            if (setWebInfo != null)
            {
                backupWeb.ExportBaseInfo(stream, setWebInfo);
            }
            else
            {
                backupWeb.ExportBaseInfo(stream);
            }
        }

        public void ExportFeatures(IAveBackupStream stream)
        {
            EnsureBackupWeb();
            backupWeb.ExportFeatures(stream);
        }

        public void ExportSettings(IAveBackupStream stream)
        {
            EnsureBackupWeb();
            backupWeb.ExportSettings(stream);
        }

        public void ExportContentTypes(IAveBackupStream stream, SPContentTypeBackupOption backupContentTypeOption)
        {
            EnsureBackupWeb();
            backupWeb.ExportContentTypes(stream, backupContentTypeOption);
        }

        public void ExportEventReceivers(IAveBackupStream stream)
        {
            EnsureBackupWeb();
            backupWeb.ExportEventReceivers(stream);
        }

        public void ExportSearchInfo(IAveBackupStream stream)
        {
            EnsureBackupWeb();
            backupWeb.ExportSearchInfo(stream);
        }

        public void ExportNavigation(IAveBackupStream stream, SPNavigationOption backupNavigationOption)
        {
            EnsureBackupWeb();
            backupWeb.ExportNavigation(stream, backupNavigationOption);
        }

        public void ExportRoles(IAveBackupStream stream)
        {
            EnsureBackupWeb();
            backupWeb.ExportRoles(stream);
        }

        public void ExportSocialInfos(IAveBackupStream stream)
        {
            EnsureBackupWeb();
            backupWeb.ExportSocialInfos(stream);
        }

        public void ExportRoleAssignments(IAveBackupStream stream)
        {
            EnsureBackupWeb();
            backupWeb.ExportRoleAssignments(stream);
        }

        public void ExportRoleAssignments(IAveBackupStream stream, SPRoleAssignmentsBakupOption backupOption)
        {
            EnsureBackupWeb();
            backupWeb.ExportRoleAssignments(stream, backupOption);
        }


        public void ExportFields(IAveBackupStream stream, SPWebFieldBackupOption backupColumnOption)
        {
            EnsureBackupWeb();
            backupWeb.ExportFields(stream, backupColumnOption);
        }


        public void ExportLanguageInfo(IAveBackupStream stream)
        {
            EnsureBackupWeb();
            backupWeb.ExportLanguageInfo(stream);
        }
    }
}
