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
using AvePoint.Wrapper.Core.Common;

namespace AvePoint.Wrapper.Core.SPRestore
{
    /// <summary>
    /// Restore API for site, web, list, list item, folder, file, attachment
    /// </summary>
    public interface ISPRestoreAPI
    {
        #region Site Collection
        /// <summary>
        /// Call this method when using IAveSite Object.
        /// </summary>
        /// <param name="site">The site object</param>
        /// <returns></returns>
        ISPSiteImport CreateSPSiteImport(IAveSite site);

        /// <summary>
        /// O365，不支持manu input
        /// 
        /// Call this method when the site is O365 site.
        /// </summary>
        /// <param name="url">O365 site address</param>
        /// <param name="accountInfo">O365 user accounts</param>
        /// <returns>return ISPSiteImport Interface</returns>
        ISPSiteImport CreateSPSiteImport(string url, O365AccountInfo accountInfo);

        /// <summary>
        /// Server Side，支持manu input
        /// 
        /// Call this method when the site is on-premise 
        /// </summary>
        /// <param name="webApplicationUrl">The web application url</param>
        /// <param name="url">the site collection url</param>
        /// <returns>
        /// return ISPSiteImport Interface
        /// </returns>
        ISPSiteImport CreateSPSiteImport(string webApplicationUrl, string url);
        #endregion

        #region Web
        /// <summary>
        /// Create an independent SPWebImport for restore
        /// </summary>
        /// <param name="restoreSite"></param>
        /// <param name="url">A string that contains either the server-relative or site-relative URL of the site. A server-relative URL begins with a forward slash ("/"), while a site-relative URL does not begin with a forward slash.</param>
        /// <returns></returns>
        ISPWebImport CreateIndependentSPWebImport(ISPSiteImport restoreSite, string url);
        /// <summary>
        /// Create SPRestore Web for restore
        /// </summary>
        /// <param name="restoreSite"></param>
        /// <param name="url">A string that contains either the server-relative or site-relative URL of the site. A server-relative URL begins with a forward slash ("/"), while a site-relative URL does not begin with a forward slash.</param>
        /// <returns></returns>
        ISPWebImport CreateSPWebImport(ISPSiteImport restoreSite, string url);

        /// <summary>
        /// 给wrapper使用
        /// </summary>
        /// <param name="restoreSite"></param>
        /// <param name="url">A string that contains either the server-relative or site-relative URL of the site. A server-relative URL begins with a forward slash ("/"), while a site-relative URL does not begin with a forward slash.</param
        /// <returns></returns>
        ISPWebImport CreateSPWebImport(IAveSite site, string url);
        #endregion

        #region List
        /// <summary>
        /// Create SP Restore List
        /// </summary>
        /// <param name="restoreWeb"></param>
        /// <param name="listTitle"></param>
        /// <returns></returns>
        ISPListImport CreateSPListImport(ISPWebImport restoreWeb, string listTitle);

        /// <summary>
        /// 封装BackupList，为了只备份一个list使用，内部还是使用AveSPList的方法
        /// </summary>
        /// <param name="destWeb"></param>
        /// <param name="listTitle"></param>
        /// <returns></returns>
        ISPListImport CreateSPListImport(IAveWeb web, string listTitle);
        #endregion

        #region File
         /// <summary>
        /// Create SP Restore File
        /// </summary>
        /// <param name="destList"></param>
        /// <param name="rootFolder"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        ISPFileImport CreateSPFileImport(IAveList list, IAveFolder rootFolder, string fileName);

        /// <summary>
        /// Create SP Restore File by Id
        /// </summary>
        /// <param name="destList"></param>
        /// <param name="rootFolder"></param>
        /// <param name="fileName"></param>
        /// <param name="rowId"></param>
        /// <returns></returns>
        ISPFileImport CreateSPFileImport(IAveList list, IAveFolder rootFolder, string fileName, int rowId);

        /// <summary>
        /// Create SP Restore File
        /// </summary>
        /// <param name="restoreFolder"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        ISPFileImport CreateSPFileImport(ISPFolderImport restoreFolder, string fileName);
        /// <summary>
        /// Create SP Restore File according to File， 优先使用rowid来find，如果找不到再使用fileName。
        /// </summary>
        /// <param name="restoreFolder"></param>
        /// <param name="fileName"></param>
        /// <param name="rowId"></param>
        /// <returns></returns>
        ISPFileImport CreateSPFileImport(ISPFolderImport restoreFolder, string fileName, int rowId);
        /// <summary>
        /// Create SP Restore File according to folder url and file name
        /// </summary>
        /// <param name="restoreList"></param>
        /// <param name="folderUrl"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        ISPFileImport CreateSPFileImport(ISPListImport restoreList, string folderUrl, string fileName);
        #endregion

        #region ListItem
        /// <summary>
        /// Only for Wrapper ITCase 
        /// </summary>
        /// <param name="destList"></param>
        /// <param name="listItemName"></param>
        /// <param name="rowId">主要给Replicator使用，因为Replicator知道目的端的ItemId</param>
        /// <returns></returns>
        ISPListItemImport CreateSPListItemImport(IAveList list, string listItemName, int rowId = 0);

        /// <summary>
        /// Create SPListItemImport
        /// </summary>
        /// <param name="restoreFolder"></param>
        /// <param name="listItemName"></param>
        /// <param name="rowId">主要给Replicator使用，因为Replicator知道目的端的ItemId</param>
        /// <returns></returns>
        ISPListItemImport CreateSPListItemImport(ISPFolderImport restoreFolder, string listItemName, int rowId = 0);
        #endregion

        #region Folder
        /// <summary>
        /// Create SPRestore Folder according to restore list, parent folder and folder url
        /// </summary>
        /// <param name="restoreList"></param>
        /// <param name="parentFolderRelativeUrl">Parent Folder ServerRelativeUrl</param>
        /// <param name="folderName">folder name</param>
        /// <returns></returns>
        ISPFolderImport CreateSPFolderImport(ISPListImport restoreList, string parentFolderRelativeUrl, string folderName);

        /// <summary>
        /// For Wrapper ITCase:内部使用
        /// </summary>
        /// <param name="restoreList"></param>
        /// <param name="parentFolderRelativeUrl"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        ISPFolderImport CreateSPFolderImport(IAveList restoreList, string parentFolderRelativeUrl, string folderName);


        /// <summary>
        /// Create SPRestore Folder according to restore list and folderRelativeUrl
        /// 这个一般用于跨folder还原
        /// </summary>
        /// <param name="restoreList"></param>
        /// <param name="folderRelativeUrl">Folder RelativeUrl: String.Empty to create RootFolder</param>
        /// <returns></returns>
        ISPFolderImport CreateSPFolderImport(ISPListImport restoreList, string folderRelativeUrl);

        /// <summary>
        /// Create SPRestore Folder according to restore folder and folder name
        /// </summary>
        /// <param name="restoreFolder">Parent Folder</param>
        /// <param name="folderName">Folder Name</param>
        /// <returns></returns>
        ISPFolderImport CreateSPFolderImport(ISPFolderImport restoreFolder, string folderName);
        #endregion

        #region Attachment
        /// <summary>
        /// Create SP Restore Attachment
        /// 
        /// wrapper内部使用
        /// </summary>
        /// <param name="list"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        ISPAttachmentImport CreateSPAttachmentImport(IAveListItem listItem, string name);

        /// <summary>
        /// Create SP Restore Attachment
        /// </summary>
        /// <param name="restoreFolder"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        ISPAttachmentImport CreateSPAttachmentImport(ISPFolderImport restoreFolder, string name);
        #endregion

        /// <summary>
        /// Current Version
        /// </summary>
        Version CurrentVersion { get; }
    }
}
