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
    /// 封装Restore File，为了只还原一个文件使用，内部还是使用AveSPDocument的方法
    /// </summary>
    class SPFileImportWrapperForITCase : ISPFileImport
    {
        private ISPSiteImport restoreSite;
        private ISPWebImport restoreWeb;
        private ISPListImport restoreList;
        private ISPFolderImport restoreFolder;
        private ISPFileImport restoreFile;
        private static SPRestoreAPI restoreAPI = new SPRestoreAPI();

        private readonly IAveList destList;
        private readonly IAveFolder parentFolder;
        private readonly string fileName;
        private readonly int rowId;

        public SPFileImportWrapperForITCase(IAveList destList, IAveFolder parentFolder, string fileName)
            : this(destList, parentFolder, fileName, -1)
        {
        }

        public SPFileImportWrapperForITCase(IAveList destList, IAveFolder parentFolder, string fileName, int rowId)
        {
            if (destList == null)
            {
                throw new ArgumentNullException("destList");
            }

            if (parentFolder == null)
            {
                throw new ArgumentNullException("rootFolder");
            }

            this.destList = destList;
            this.parentFolder = parentFolder;
            this.fileName = fileName;
            this.rowId = rowId;
        }

        private void Initialize(IAveRestoreStream restoreStream)
        {
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

            restoreFolder = restoreAPI.CreateSPFolderImport(restoreList, parentFolder.ServerRelativeUrl);
        }

        public void Dispose()
        {
            if (restoreFile != null)
            {
                restoreFile.Dispose();
            }
            if (restoreFolder != null)
            {
                restoreFolder.Dispose();
            }
            restoreList.Dispose();
            restoreWeb.Dispose();
            restoreSite.Dispose();

            restoreFile = null;
            restoreFolder = null;
            restoreList = null;
            restoreWeb = null;
            restoreSite = null;
        }

        private void EnsureRestoreFile()
        {
            if (restoreFile == null)
            {
                throw new ArgumentNullException("restoreFile");
            }
        }

        public IAveFile File
        {
            get 
            {
                EnsureRestoreFile();
                return restoreFile.File; 
            }
        }

        public SPFileRestoreReport Restore(IAveRestoreStream restoreStream, SPFileRestoreOption spFileRestoreOption)
        {
            Initialize(restoreStream);
            SPFileRestoreReport report = new SPFileRestoreReport();
            while (restoreStream.ReadHead() != null)
            {
                if (rowId > 0)
                {
                    restoreFile = restoreAPI.CreateSPFileImport(restoreFolder, fileName, rowId);
                }
                else
                {
                    restoreFile = restoreAPI.CreateSPFileImport(restoreFolder, fileName);
                }
                report = restoreFile.Restore(restoreStream, spFileRestoreOption);
                restoreStream.Reset();
            }
            return report;
        }

        public IAveListItem Item
        {
            get { EnsureRestoreFile(); return restoreFile.Item; }
        }
    }
}
