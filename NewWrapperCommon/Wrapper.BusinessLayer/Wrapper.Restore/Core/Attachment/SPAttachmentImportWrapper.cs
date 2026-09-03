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

namespace AvePoint.Wrapper.Restore.Core
{
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Core.SPRestore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    class SPAttachmentImportWrapper : ISPAttachmentImport
    {
        private ISPSiteImport restoreSite;
        private ISPWebImport restoreWeb;
        private ISPListImport restoreList;
        private ISPFolderImport restoreFolder;
        private ISPAttachmentImport restoreAttachment;

        private readonly IAveListItem destListItem;
        private readonly string attachmenInternaltName;

        public SPAttachmentImportWrapper(IAveListItem destListItem, string attachmenInternaltName)
        {
            if (destListItem == null)
            {
                throw new ArgumentNullException("destListItem");
            }

            this.destListItem = destListItem;
            this.attachmenInternaltName = attachmenInternaltName;
        }

        private void Initialize(IAveRestoreStream restoreStream)
        {
            var restoreAPI = new SPRestoreAPI();
            restoreSite = restoreAPI.CreateSPSiteImport(destListItem.ParentList.ParentWeb.Site);
            restoreSite.Restore(restoreStream, new SPSiteRestoreOption()
            {
                RestoreAction = SPContainerRestoreAction.Skip,
            });
            restoreStream.Reset();

            restoreWeb = restoreAPI.CreateSPWebImport(restoreSite, destListItem.ParentList.ParentWeb.ServerRelativeUrl);
            restoreWeb.Restore(restoreStream, new SPWebRestoreOption()
            {
                RestoreAction = SPContainerRestoreAction.None,
            });
            restoreStream.Reset();

            restoreList = restoreAPI.CreateSPListImport(restoreWeb, destListItem.ParentList.Title);
            restoreList.Restore(restoreStream, new SPListRestoreOption()
            {
                RestoreAction = SPContainerRestoreAction.None,
            });
            restoreStream.Reset();

            restoreFolder = restoreAPI.CreateSPFolderImport(restoreList, destListItem.ParentList.RootFolder.ServerRelativeUrl);
            if (!restoreFolder.SPFolder.ServerRelativeUrl.Equals(restoreFolder.SPFolder.ParentList.RootFolder.ServerRelativeUrl))
            {
                restoreFolder.Restore(restoreStream, new SPFolderRestoreOption()
                {
                    RestoreAction = SPFolderRestoreAction.Default,
                });
                restoreStream.Reset();
            } 

            restoreAttachment = restoreAPI.CreateSPAttachmentImport(restoreFolder, attachmenInternaltName);
        }

        public void Dispose()
        {
            restoreSite.Dispose();
            restoreWeb.Dispose();
            restoreList.Dispose();
            restoreFolder.Dispose();
            restoreAttachment.Dispose();

            restoreSite = null;
            restoreWeb = null;
            restoreList = null;
            restoreFolder = null;
            restoreAttachment = null;
        }

        private void EnsureRestoreAttachment()
        {
            if (restoreAttachment == null)
            {
                throw new ArgumentNullException("restoreAttachment");
            }
        }

        public SPFileRestoreReport Restore(IAveRestoreStream restoreStream, SPAttachmentRestoreOption spAttachmentRestoreOption)
        {
            Initialize(restoreStream);
            EnsureRestoreAttachment();
            return restoreAttachment.Restore(restoreStream, spAttachmentRestoreOption);
        }
    }
}
