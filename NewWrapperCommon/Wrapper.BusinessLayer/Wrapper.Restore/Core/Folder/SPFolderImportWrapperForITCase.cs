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
using AvePoint.Wrapper.Core.SPRestore;

namespace AvePoint.Wrapper.Restore.Core
{
    /// <summary>
    /// 封装Restore Folder，为了只还原一个folder使用，内部还是使用AveSPListItem的方法
    /// </summary>
    class SPFolderImportWrapperForITCase : ISPFolderImport
    {
        private ISPSiteImport restoreSite;
        private ISPWebImport restoreWeb;
        private ISPListImport restoreList;
        private ISPFolderImport restoreFolder;

        private readonly IAveList destList;
        private readonly string parentFolderRelativeUrl;
        private readonly string folderRelatedUrl;

        public SPFolderImportWrapperForITCase(IAveList destList, string parentFolderRelativeUrl, string folderRelatedUrl)
        {
            if (destList == null)
            {
                throw new ArgumentNullException("destList");
            }

            this.destList = destList;
            this.folderRelatedUrl = folderRelatedUrl;
            this.parentFolderRelativeUrl = parentFolderRelativeUrl;

            //Initialize();
        }

        private void Initialize(IAveRestoreStream restoreStream)
        {
            var restoreAPI = new SPRestoreAPI();
            restoreSite = restoreAPI.CreateSPSiteImport(destList.ParentWeb.Site);
            restoreSite.Restore(restoreStream, new SPSiteRestoreOption()
            {
                RestoreAction = SPContainerRestoreAction.Skip,
            });
            restoreStream.Reset();
            
            restoreWeb = restoreAPI.CreateSPWebImport(restoreSite, destList.ParentWeb.ServerRelativeUrl);
            restoreWeb.Restore(restoreStream, new SPWebRestoreOption()
            {
                RestoreAction = SPContainerRestoreAction.None,
            });
            restoreStream.Reset();
            
            restoreList = restoreAPI.CreateSPListImport(restoreWeb, destList.Title);
            restoreList.Restore(restoreStream, new SPListRestoreOption()
            {
                RestoreAction = SPContainerRestoreAction.None,
            });
            restoreStream.Reset();

            restoreFolder = restoreAPI.CreateSPFolderImport(restoreList, parentFolderRelativeUrl, folderRelatedUrl);
        }

        public void Dispose()
        {
            restoreFolder.Dispose();
            restoreList.Dispose();
            restoreWeb.Dispose();
            restoreSite.Dispose();

            restoreFolder = null;
            restoreList = null;
            restoreWeb = null;
            restoreSite = null;
        }

        private void EnsureRestoreFolder()
        {
            if (restoreFolder == null)
            {
                throw new ArgumentNullException("restoreFolder");
            }
        }

        public IAveFolder SPFolder { get { return restoreFolder.SPFolder; } }

        public SPFileRestoreReport Restore(IAveRestoreStream restoreStream, SPFolderRestoreOption spFolderRestoreOption)
        {
            Initialize(restoreStream);
            
            return restoreFolder.Restore(restoreStream, spFolderRestoreOption);
        }
    }
}
