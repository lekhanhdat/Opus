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
using AvePoint.Wrapper.Restore;
using AvePoint.Wrapper.Restore.Core;

namespace AvePoint.Wrapper.Core.SPRestore
{
    /// <summary>
    /// SP Restore API
    /// </summary>
    internal class SPRestoreAPI : ISPRestoreAPI
    {
        private readonly Version currentVersion = new Version(1, 0, 0, 0);

        public Version CurrentVersion { get { return currentVersion; } }

        #region Site Collection
        /// <summary>
        /// 根据已经存在site来创建restore对象
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public ISPSiteImport CreateSPSiteImport(IAveSite site)
        {
            return new AveSPSiteV1(site.Url, site.Url, site.SPMode == WrapperSPMode.Server ? AveContextKind.ServerObjectModel : AveContextKind.ClientObjectModel, site.UserAccountInfo);
        }

        /// <summary>
        /// 根据已经存在的site来创建restore对象
        /// </summary>
        /// <param name="url"></param>
        /// <param name="accountInfo"></param>
        /// <returns></returns>
        public ISPSiteImport CreateSPSiteImport(string url, O365AccountInfo accountInfo)
        {
            if(accountInfo == null)
            {
                throw new ArgumentNullException("accountInfo");
            }

            return new AveSPSiteV1(url, url, AveContextKind.ClientObjectModel, new AveBPOSAccountInfo() { Domain = accountInfo.Domain, Password = accountInfo.Password, UserName = accountInfo.UserName });
        }

        /// <summary>
        /// 一般用于manu input，目的端不存在的情况下，进行创建
        /// </summary>
        /// <param name="webApplicationUrl"></param>
        /// <param name="url">site collection的完整url</param>
        /// <returns></returns>
        public ISPSiteImport CreateSPSiteImport(string webApplicationUrl, string url)
        {
            return new AveSPSiteV1(url, webApplicationUrl, null, AveContextKind.ServerObjectModel, null);
        }

        #endregion

        #region Web
        public ISPWebImport CreateIndependentSPWebImport(ISPSiteImport restoreSite, string url)
        {
            throw new NotImplementedException();//不需要实现，这个以前code不支持。
        }

        public ISPWebImport CreateSPWebImport(ISPSiteImport restoreSite, string url)
        {
            return new AveSPWebV1((AveSPSiteV1) restoreSite, url);
        }

        public ISPWebImport CreateSPWebImport(IAveSite site, string url)
        {
            return new SPWebImportWrapperForITCase(site, url, this);
        }

        #endregion

        #region List

        public ISPListImport CreateSPListImport(ISPWebImport restoreWeb, string listTitle)
        {
            return new AveSPListV1((AveSPWebV1)restoreWeb, listTitle);
        }

        /// <summary>
        /// 封装BackupList，为了只备份一个list使用，内部还是使用AveSPList的方法
        /// </summary>
        /// <param name="destWeb"></param>
        /// <param name="listTitle"></param>
        /// <returns></returns>
        public ISPListImport CreateSPListImport(IAveWeb web, string listTitle)
        {
            return new SPListImportWrapper(web, listTitle);
        }
        #endregion

        #region folder
        /// <summary>
        /// For ITCase Only
        /// </summary>
        /// <param name="restoreList"></param>
        /// <param name="parentFolderRelativeUrl"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public ISPFolderImport CreateSPFolderImport(IAveList restoreList, string parentFolderRelativeUrl, string folderName)
        {
            return new SPFolderImportWrapperForITCase(restoreList, parentFolderRelativeUrl, folderName);
        }

        public ISPFolderImport CreateSPFolderImport(ISPListImport restoreList, string parentFolderRelativeUrl, string folderName)
        {
            return new AveSPFolderV1((AveSPListV1)restoreList, parentFolderRelativeUrl, folderName);
        }

        public ISPFolderImport CreateSPFolderImport(ISPListImport restoreList, string folderRelativeUrl)
        {
            var list = (AveSPListV1)restoreList;
            if (!string.IsNullOrEmpty(folderRelativeUrl)
                && !list.RootFolder.ServerRelativeUrl.Equals(folderRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                var index = folderRelativeUrl.LastIndexOf('/');

                if (index > 0)
                {
                    return CreateSPFolderImport(restoreList, folderRelativeUrl.Substring(0, index), folderRelativeUrl.Substring(index + 1));
                }
                else
                {
                    throw new ArgumentException("folderRelativeUrl");
                }
            }
            else
            {
                return new AveSPFolderV1(list, "{System Folder}");
            }
        }

        public ISPFolderImport CreateSPFolderImport(ISPFolderImport restoreFolder, string folderName)
        {
            return new AveSPFolderV1((AveSPFolderV1)restoreFolder, folderName);
        }

        #endregion

        #region file

        public ISPFileImport CreateSPFileImport(IAveList list, IAveFolder rootFolder, string fileName)
        {
            return new SPFileImportWrapperForITCase(list, rootFolder, fileName);
        }

        public ISPFileImport CreateSPFileImport(IAveList list, IAveFolder rootFolder, string fileName, int rowId)
        {
            return new SPFileImportWrapperForITCase(list, rootFolder, fileName, rowId);
        }
        

        public ISPFileImport CreateSPFileImport(ISPFolderImport restoreFolder, string fileName, int rowId)
        {
            throw new NotImplementedException();
            //return new AveSPDoc((AveSPFolder)restoreFolder, fileName, rowId);
        }
        
        #endregion

        #region list item

        /// <summary>
        /// 主要给Replicator使用，因为Replicator知道目的端的ItemId
        /// </summary>
        /// <param name="destList"></param>
        /// <param name="listItemName"></param>
        /// <param name="rowId"></param>
        /// <returns></returns>
        public ISPListItemImport CreateSPListItemImport(IAveList list, string listItemName, int rowId)
        {
            return new SPListItemImportWrapperForITCase(list, listItemName, rowId);
        }

        public ISPListItemImport CreateSPListItemImport(IAveList list, string listItemName)
        {
            return new SPListItemImportWrapperForITCase(list, listItemName);
        }

        public ISPListItemImport CreateSPListItemImport(ISPFolderImport restoreFolder, string listItemName)
        {
            return new AveSPListItemV1((AveSPFolder) restoreFolder, listItemName);
        }

        public ISPListItemImport CreateSPListItemImport(ISPFolderImport restoreFolder, string listItemName, int rowId)
        {
            return new AveSPListItemV1((AveSPFolder) restoreFolder, listItemName, rowId);
        }

        #endregion

        #region Attachment
        public ISPAttachmentImport CreateSPAttachmentImport(ISPFolderImport restoreFolder, string name)
        {
            return new AveSPAttachment((AveSPFolder)restoreFolder, name);
        }

        public ISPAttachmentImport CreateSPAttachmentImport(IAveListItem listItem, string attachmenInternaltName)
        {
            return new SPAttachmentImportWrapper(listItem, attachmenInternaltName);
        }
        #endregion
    }
}
