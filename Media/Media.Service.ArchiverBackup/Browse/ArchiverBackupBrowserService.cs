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




namespace AvePoint.Media.Service.ArchiverBackup
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AvePoint.Common;
    using AvePoint.GCommon;
    using AvePoint.Media.Common;
    using Merged18NResources.MediaServiceArchiverBackup;
    using AvePoint.Media.Service.DomainModel;
    using Storage;
    using global::Media.Common.ClassicStorageApi;

    #endregion using directives

    public class ArchiverBackupBrowserService
        : BrowserServiceBase<ArchiverBrowseInfo, ArchiverBrowseResult>
        , IBrowserService
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        IXSystem indexLogicalDevice;

        public IIndexService<ArchiverIndexServiceOpenParameter> IndexService { get; set; }

        public IArchiverBrowserIndexService ArchiverBrowserIndexService { get; set; }

        public override void Open(ArchiverBrowseInfo browserInfo)
        {
            this.indexLogicalDevice = XFactoryCommon.InstanceLibrary(new List<string>() { browserInfo.IndexLogicalDevice.ConnectionString });
            this.indexLogicalDevice.Open();
            var openParam = new ArchiverIndexServiceOpenParameter(browserInfo, null, indexLogicalDevice);
            this.IndexService.Open(openParam);
        }

        public override ArchiverBrowseResult Browse(ArchiverBrowseInfo browserInfo)
        {
            var result = new ArchiverBrowseResult();
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupBrowserServiceArchiverBrowseResultBegin, browserInfo.Path);
            result.NodeList = Invoker.CallMethod(this, MethodBase.GetCurrentMethod().Name + browserInfo.Level.ToString(), browserInfo) as List<TreeNode>;
            result.TotalCounts = this.GetSubItemsCount(browserInfo);
            return result;
        }

        public override void ProcessException(Exception e)
        {
            e = e.InnerException ?? e;
            this.logger.Error(MediaServiceArchiverBackupResource.ArchiverBackupBrowserServiceProcessExceptionError, e.ToString());
        }

        public override void Dispose()
        {
            if (IndexService != null)
            {
                this.IndexService.Close();
            }
            if (indexLogicalDevice != null)
            {
                this.indexLogicalDevice.Close();
            }
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupBrowserServiceDisposeEnd);
        }









        private Int32 GetSubItemsCount(ArchiverBrowseInfo browseInfo)
        {
            var itemsCount = default(Int32);
            if (browseInfo.Level == TreeNodeLevel.Items && browseInfo.OffSet == 0)
            {
                var parentIndexInfo = new ArchiverIndexInfo(browseInfo);
                itemsCount = (Int32)this.ArchiverBrowserIndexService.GetSubItemsCount(parentIndexInfo);
            }
            return itemsCount;
        }
    }
}