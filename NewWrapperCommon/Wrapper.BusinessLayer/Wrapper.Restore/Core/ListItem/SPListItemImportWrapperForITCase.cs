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
    /// 封装Restore ListItem，为了只还原一个listItem使用，内部还是使用AveSPListItem的方法
    /// </summary>
    class SPListItemImportWrapperForITCase : ISPListItemImport
    {
        private ISPSiteImport restoreSite;
        private ISPWebImport restoreWeb;
        private ISPListImport restoreList;
        private ISPFolderImport restoreFolder;
        private ISPListItemImport restoreListItem;
        private SPRestoreAPI restoreAPI;

        private readonly IAveList destList;
        private readonly string listItemName;
        private readonly int rowId;

        public SPListItemImportWrapperForITCase(IAveList destList, string listItemName)
            : this(destList, listItemName, -1)
        {

        }

        // 主要给Replicator使用，因为Replicator知道目的端的ItemId
        public SPListItemImportWrapperForITCase(IAveList destList, string listItemName, int rowId)
        {
            if (destList == null)
            {
                throw new ArgumentNullException("destList");
            }

            this.destList = destList;
            this.listItemName = listItemName;
            this.rowId = rowId;
        }

        private void Initialize(IAveRestoreStream restoreStream)
        {
            restoreAPI = new SPRestoreAPI();
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
                ManagedMetadataOption = new SPManagedMetadataRestoreOption()
                {
                    EnableCache = true
                },
                ConfigurationRestoreOption = new SPWebConfigurationRestoreOption()
                {
                    ContentTypeRestoreAction = SPObjectRestoreAction.Restore,
                    ContentTypeRestoreOption = new Restore.AveContentTypeRestoreOption(),
                    FieldRestoreAction = SPObjectRestoreAction.Restore,
                    FieldRestoreOption = new Restore.AveFieldRestoreOption(),
                    IsRestoreWebRegionalSettings = true,
                    RestoreConfiguration = true,
                },
            });
            restoreStream.Reset();
            
            restoreList = restoreAPI.CreateSPListImport(restoreWeb, destList.Title);
            restoreList.Restore(restoreStream, new SPListRestoreOption()
            {
                RestoreAction = SPContainerRestoreAction.None,
                ConfigurationRestoreOption = new SPListConfigurationRestoreOption() 
                { 
                    RestoreConfiguration = true,
                    FieldRestoreOption=new AveFieldRestoreOption(),
                    FieldRestoreAction = SPObjectRestoreAction.Restore ,
                    ContentTypeRestoreOption=new AveContentTypeRestoreOption(),
                    ContentTypeRestoreAction=SPObjectRestoreAction.Restore
                }
            });
            restoreStream.Reset();

            restoreFolder = restoreAPI.CreateSPFolderImport(restoreList, destList.RootFolder.ServerRelativeUrl);
        }

        public void Dispose()
        {
            if (restoreListItem != null)
            {
                restoreListItem.Dispose();
            }
            if (restoreFolder != null)
            {
                restoreFolder.Dispose();
            }
            restoreList.Dispose();
            restoreWeb.Dispose();
            restoreSite.Dispose();

            restoreListItem = null;
            restoreFolder = null;
            restoreList = null;
            restoreWeb = null;
            restoreSite = null;
        }

        private void EnsureRestoreListItem()
        {
            if (restoreListItem == null)
            {
                throw new ArgumentNullException("restoreListItem");
            }
        }

        public SPFileRestoreReport Restore(IAveRestoreStream restoreStream, SPListItemRestoreOption spListItemRestoreOption)
        {
            Initialize(restoreStream);
            SPFileRestoreReport report = new SPFileRestoreReport();
            while (restoreStream.ReadHead() != null)
            {
                if (rowId > 0)
                {
                    restoreListItem = restoreAPI.CreateSPListItemImport(restoreFolder, listItemName, rowId);
                }
                else
                {
                    restoreListItem = restoreAPI.CreateSPListItemImport(restoreFolder, listItemName);
                }
                report = restoreListItem.Restore(restoreStream, spListItemRestoreOption);
                restoreStream.Reset();
            }
            return report;
        }

        public IAveListItem Item
        {
            get { EnsureRestoreListItem(); return restoreListItem.Item; }
        }
    }
}
