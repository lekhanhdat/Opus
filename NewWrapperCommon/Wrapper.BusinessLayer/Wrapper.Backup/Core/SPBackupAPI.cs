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
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Backup.Core;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Core.SPBackup
{
    class SPBackupAPI : ISPBackupAPI
    {
        public ISPSiteExport CreateSPSiteExport(IAveSite sourceSite)
        {
            return new AveSPSite(sourceSite);
        }

        public ISPWebExport CreateSPWebExport(ISPSiteExport backupSite, IAveWeb web)
        {
            return new AveSPWeb(backupSite, web);
        }

        public ISPListExport CreateSPListExport(ISPWebExport backupWeb, IAveList list)
        {
            return new AveSPList(backupWeb, list);
        }

        public ISPFileExport CreateSPFileExport(ISPListExport backupList, IAveFile file, int version)
        {
            return new AveSPDoc(backupList, file, version);
        }

        public ISPItemExport CreateSPListItemExport(ISPListExport backupList, IAveListItem listItem, int version)
        {
            return new AveSPListItem(new AveSPFolder((AveSPList)backupList, listItem.ParentList.RootFolder), listItem.Name, listItem.UniqueId, listItem.ID, version);
        }

        public ISPItemExport CreateSPListItemExport(ISPFolderExport parentFolder, string name, Guid uniqueId, int rowId, int version)
        {
            return new AveSPListItem((AveSPFolder)parentFolder, name, uniqueId, rowId, version);
        }

        public ISPFolderExport CreateSPFolderExport(ISPListExport backupList, Guid folderId)
        {
            var parentFolder = (backupList as AveSPList).SPList.ParentWeb.GetFolder(folderId);
            return new AveSPFolder((AveSPList)backupList, parentFolder);
        }
        //TODO
        public ISPFolderExport CreateSPFolderExport(ISPListExport backupList, IAveFolder folder, int version)
        {
            if (folder.ParentFolder == null)
            {
                return new AveSPFolder((AveSPList)backupList);
            }
            else
            {
                return null;
                //var parentFolder = (backupList as AveSPList).SPList.ParentWeb.GetFolder(folder.ParentFolder.UniqueId);
                //return new AveSPFolder(parentFolder, folder.Name, folder.UniqueId, folder.Item.ID, version);
            }
        }

        public ISPFolderExport CreateSPFolderExport(ISPFolderExport parentFolder, IAveFolder folder, int version)
        {
            return new AveSPFolder((AveSPFolder)parentFolder, folder.Name, folder.UniqueId, folder.Item.ID, version);
        }

        public ISPFolderExport CreateSPFolderExport(ISPFolderExport parentFolder, string leafName, Guid guid, int rowId, int version)
        {
            return new AveSPFolder((AveSPFolder)parentFolder, leafName, guid, rowId, version);
        }

        public ISPAttachmentExport CreateSPAttachmentExport(ISPFolderExport backupFolder, IAveAttachment attachment)
        {
            return new AveSPAttachment((AveSPFolder)backupFolder, attachment.ROWID, attachment.FileName, attachment.ServerRelativeUrl);
        }

        #region For Wrapper ITCase
        public ISPWebExport CreateSPWebExport(IAveWeb web)
        {
            return new SPWebExportWrapper(web);
        }

        public ISPListExport CreateSPListExport(IAveList list)
        {
            return new SPListExportWrapper(list);
        }
        public ISPFileExport CreateSPFileExport(IAveFile file, int version)
        {
            return new SPFileExportWrapper(file, version);
        }
        public ISPItemExport CreateSPListItemExport(IAveListItem listItem, int version)
        {
            return new SPListItemExportWrapper(listItem, version);
        }
        public ISPAttachmentExport CreateSPAttachmentExport(IAveListItem listItem, IAveAttachment attachment)
        {
            return new SPAttachmentExportWrapper(listItem, attachment);
        }
        #endregion

        public ISPFolderExport CreateSPFolderExport(ISPListExport backupList, Guid folderId, string serverRelativeUrl, int rowID)
        {
            var parentFolder = (backupList as AveSPList).SPList.ParentWeb.GetFolder(folderId, rowID, serverRelativeUrl);
            return new AveSPFolder((AveSPList)backupList, parentFolder);
        }
    }
}
