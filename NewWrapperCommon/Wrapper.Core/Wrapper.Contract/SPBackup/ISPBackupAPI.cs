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
using AvePoint.Wrapper.Backup;

namespace AvePoint.Wrapper.Core.SPBackup
{
    /// <summary>
    /// Backup API
    /// </summary>
    public interface ISPBackupAPI
    {
        /// <summary>
        /// Create SPBackupSite according to site
        /// </summary>
        /// <param name="sourceSite"></param>
        /// <returns></returns>
        ISPSiteExport CreateSPSiteExport(IAveSite site);
        /// <summary>
        /// Create SPBackupWeb according to backup site and web
        /// </summary>
        /// <param name="backupSite"></param>
        /// <param name="web"></param>
        /// <returns></returns>
        ISPWebExport CreateSPWebExport(ISPSiteExport backupSite, IAveWeb web);
        /// <summary>
        /// Create SPBackupList according to backup web and list
        /// </summary>
        /// <param name="backupWeb"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        ISPListExport CreateSPListExport(ISPWebExport backupWeb, IAveList list);
        /// <summary>
        /// Create SPBackupFolder according to backup list and folder
        /// </summary>
        /// <param name="backupList"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        ISPFolderExport CreateSPFolderExport(ISPListExport backupList, IAveFolder folder, int version);
        /// <summary>
        /// Create SPBackupFolder according to backup list and folder guid
        /// ITCase use this constructor to create attachment parent folder.
        /// </summary>
        /// <param name="backupList"></param>
        /// <param name="folderId"></param>
        /// <returns></returns>
        ISPFolderExport CreateSPFolderExport(ISPListExport backupList, Guid folderId);

        /// <summary>
        /// Create SPBackupFolder according to backup list and folder Guid or [folder Url and folder RowID](O365)
        /// ITCase use this constructor to create attachment parent folder.
        /// </summary>
        /// <param name="backupList"></param>
        /// <param name="folderId">-1 means that folder is not a listitem.</param>
        /// <returns></returns>
        ISPFolderExport CreateSPFolderExport(ISPListExport backupList, Guid folderId, string serverRelativeUrl, int rowID);


        /// <summary>
        /// Create SPBackupFolder based on parent folder, leaf name, guid, rowid and version
        /// </summary>
        /// <param name="parentFolder"></param>
        /// <param name="leafName"></param>
        /// <param name="guid"></param>
        /// <param name="rowId"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        ISPFolderExport CreateSPFolderExport(ISPFolderExport parentFolder, string leafName, Guid guid, int rowId, int version);
        /// <summary>
        /// Create SPBackupFile according to backup list and file
        /// </summary>
        /// <param name="backupList"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        ISPFileExport CreateSPFileExport(ISPListExport backupList, IAveFile file, int version);
        /// <summary>
        /// Create SPBackupFile according to backup list and listItem
        /// </summary>
        /// <param name="backupList"></param>
        /// <param name="listItem"></param>
        /// <returns></returns>
        ISPItemExport CreateSPListItemExport(ISPListExport backupList, IAveListItem listItem, int version);
        /// <summary>
        /// Create SPBackupAttachment according to backup list and attachment
        /// wrapper内部使用
        /// </summary>
        /// <param name="listItem"></param>
        /// <param name="attachment"></param>
        /// <returns></returns>
        ISPAttachmentExport CreateSPAttachmentExport(IAveListItem listItem, IAveAttachment attachment);
        /// <summary>
        /// Create SPBackupAttachment according to backup list and attachment
        /// </summary>
        /// <param name="backupList"></param>
        /// <param name="attachment"></param>
        /// <returns></returns>
        ISPAttachmentExport CreateSPAttachmentExport(ISPFolderExport backupFolder, IAveAttachment attachment);

        #region For Wrapper ITCase
        /// <summary>
        /// 封装BackupWeb，为了只备份一个Web使用，内部还是使用AveSPWeb的方法
        /// For Wrapper ITCase
        /// </summary>
        /// <param name="web"></param>
        /// <returns></returns>
        ISPWebExport CreateSPWebExport(IAveWeb web);
        /// <summary>
        /// 封装BackupList，为了只备份一个list使用，内部还是使用AveSPList的方法
        /// For Wrapper ITCase
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        ISPListExport CreateSPListExport(IAveList list);
        /// <summary>
        /// 封装BackupFolder，为了只备份一个folder version使用，内部还是使用AveSPFolder的方法
        /// For Wrapper ITCase
        /// </summary>
        /// <param name="parentFolder"></param>
        /// <param name="folder"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        ISPFolderExport CreateSPFolderExport(ISPFolderExport parentFolder, IAveFolder folder, int version);
        /// <summary>
        /// 封装BackupFile，为了只备份一个file version使用，内部还是使用AveSPDoc的方法
        /// For Wrapper ITCase
        /// </summary>
        /// <param name="backupList"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        ISPFileExport CreateSPFileExport(IAveFile file, int version);
        /// <summary>
        /// 封装BackupListItem，为了只备份一个listitem version使用，内部还是使用AveSPListItem的方法
        /// For Wrapper ITCase
        /// </summary>
        /// <param name="backupList"></param>
        /// <param name="listItem"></param>
        /// <returns></returns>
        ISPItemExport CreateSPListItemExport(IAveListItem listItem, int version);
        #endregion
    }
}
