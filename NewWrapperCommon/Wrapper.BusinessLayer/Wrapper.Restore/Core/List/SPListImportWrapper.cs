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
    /// 封装Restore List，为了只还原一个list使用，内部还是使用AveSPList的方法
    /// </summary>
    class SPListImportWrapper : ISPListImport
    {
        private ISPListImport restoreList;
        private ISPSiteImport restoreSite;
        private ISPWebImport restoreWeb;

        private readonly IAveWeb destWeb;
        private readonly string listTitle;

        public SPListImportWrapper(IAveWeb destWeb, string listTitle)
        {
            if (destWeb == null)
            {
                throw new ArgumentNullException("destWeb");
            }

            this.destWeb = destWeb;
            this.listTitle = listTitle;

            Initialize();
        }

        private void Initialize()
        {
            var restoreAPI = new SPRestoreAPI();
            restoreSite = restoreAPI.CreateSPSiteImport(destWeb.Site);
            restoreWeb = restoreAPI.CreateSPWebImport(restoreSite, destWeb.ServerRelativeUrl);
            restoreList = restoreAPI.CreateSPListImport(restoreWeb, listTitle);
        }

        public void Dispose()
        {
            restoreList.Dispose();
            restoreWeb.Dispose();
            restoreSite.Dispose();

            restoreList = null;
            restoreWeb = null;
            restoreSite = null;
        }

        private void EnsureRestoreList()
        {
            if (restoreList == null)
            {
                throw new ArgumentNullException("restoreList");
            }
        }

        public SPFileRestoreReport Restore(IAveRestoreStream restoreStream, SPListRestoreOption spListRestoreOption)
        {
            EnsureRestoreList();
            restoreSite.Restore(restoreStream, new SPSiteRestoreOption()
            {
                RestoreAction = SPContainerRestoreAction.Skip,
            });
            restoreStream.Reset();
            restoreWeb.Restore(restoreStream,new SPWebRestoreOption()
            {
                RestoreAction = SPContainerRestoreAction.None,
            });
            restoreStream.Reset();
            return restoreList.Restore(restoreStream, spListRestoreOption);
        }

        public ISPSiteImport ParentSite
        {
            get { return restoreSite; }
        }

        public IAveList SPList
        {
            get { return restoreList.SPList; }
        }


        public void Restore(IAveRestoreStream restoreStream, SPListRestoreOption spListRestoreOption, ISPListImportProfiler profiler)
        {
            throw new NotImplementedException();
        }

        public Wrapper.Core.SPRestore.Mapping.IFieldMapping FieldMapping { get; set; }

        public Wrapper.Core.SPRestore.Mapping.IContentTypeMapping ContentTypeMapping { get; set; }

    }
}