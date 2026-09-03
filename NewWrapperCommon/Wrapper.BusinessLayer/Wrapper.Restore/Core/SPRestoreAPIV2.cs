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
    /// V2是重新Restore代码，这样如果开启新逻辑，只需要配置配置文件即可。
    /// </summary>
    internal class SPRestoreAPIV2 : ISPRestoreAPI
    {
        private readonly Version currentVersion = new Version(2, 0, 0, 0);

        public Version CurrentVersion { get { return currentVersion; } }

        #region Site Collection
        /// <summary>
        /// Create Site Import According to IAveSite instance
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public ISPSiteImport CreateSPSiteImport(IAveSite site)
        {
            if(site.SPMode == WrapperSPMode.O365)
            {
                var account = new O365AccountInfo();
                account.Domain = site.UserAccountInfo.Domain;
                account.Password = site.UserAccountInfo.Password;
                account.UserName = site.UserAccountInfo.UserName;

                return CreateSPSiteImport(site.Url, account);
            }

            return CreateSPSiteImport(site.WebApplication.GetResponseUri(AveUrlZone.Default).AbsoluteUri, site.Url);
        }

        /// <summary>
        /// 根据已经存在的site来创建restore对象
        /// </summary>
        /// <param name="url"></param>
        /// <param name="accountInfo"></param>
        /// <returns></returns>
        public ISPSiteImport CreateSPSiteImport(string url, O365AccountInfo accountInfo)
        {
            return new SPSiteImport(url, accountInfo);
        }

        /// <summary>
        /// 一般用于manu input，目的端不存在的情况下，进行创建
        /// </summary>
        /// <param name="webApplicationUrl"></param>
        /// <param name="url">site collection的完整url</param>
        /// <returns></returns>
        public ISPSiteImport CreateSPSiteImport(string webApplicationUrl, string url)
        {
            return new SPSiteImport(webApplicationUrl, url);
        }
        #endregion

        #region Web
        public ISPWebImport CreateIndependentSPWebImport(ISPSiteImport restoreSite, string siteRelativeURL)
        {
            return new SPWebImportWrapperMT((SPSiteImport)restoreSite, siteRelativeURL, this);
        }
        public ISPWebImport CreateSPWebImport(ISPSiteImport restoreSite, string siteRelativeURL)
        {
            return new SPWebImport((SPSiteImport)restoreSite, siteRelativeURL);
        }

        public ISPWebImport CreateSPWebImport(IAveSite site, string siteRelativeURL)
        {
            return new SPWebImportWrapperForITCase(site, siteRelativeURL, this);
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
            if (!string.IsNullOrEmpty(folderRelativeUrl))
            {
                var index = folderRelativeUrl.LastIndexOf('/');

                if (index > 0)
                {
                    return CreateSPFolderImport(restoreList, folderRelativeUrl.Substring(0, index), folderRelativeUrl.Substring(index + 1));
                }
                else
                {
                    return CreateSPFolderImport(restoreList, string.Empty, folderRelativeUrl);
                }
            }
            throw new ArgumentNullException("folderRelativeUrl");
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
