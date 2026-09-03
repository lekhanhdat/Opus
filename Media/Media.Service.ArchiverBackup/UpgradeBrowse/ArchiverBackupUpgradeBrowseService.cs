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
    using System.IO;
    using System.Reflection;
    using System.Text;
    using AvePoint.Common;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.Media.Common;
    using AvePoint.Media.Core.Index;
    using Merged18NResources.MediaServiceArchiverBackup;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using Storage;
    using AvePoint.GCommon.Utility;
    #endregion

    #region CodeReview

    [AveCodeReview(
    "2012/2/29",
    "dwxue@avepoint.com",
    "jbli@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_EH_1 },
    "ADO-26066",
    false)]
    #endregion

    public class ArchiverBackupUpgradeBrowseService
        : BrowserServiceBase<ArchiverUpgradeBrowseInfo, ArchiverUpgradeBrowseResult>
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        IXSystem indexLogicalDevice;

        public override void Open(ArchiverUpgradeBrowseInfo browserInfo)
        {
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeBrowseServiceOpenBegin);
            this.indexLogicalDevice = this.StorageDeviceManager.Open(browserInfo.IndexLogicalDevice.GetXRIS(PhysicalDeviceUsage.Index));
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeBrowseServiceOpenSuccessed);
        }

        public override ArchiverUpgradeBrowseResult Browse(ArchiverUpgradeBrowseInfo browserInfo)
        {
            var result = new ArchiverUpgradeBrowseResult();
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeBrowseServiceBrowseBegin);
            result.NodeList = Invoker.CallMethod(this, MethodBase.GetCurrentMethod().Name + browserInfo.TreeNodeLevel.ToString(), browserInfo) as List<TreeNode>;
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeBrowseServiceBrowseSuccessed);
            return result;
        }


        public override void ProcessException(Exception e)
        {
            e = e.InnerException ?? e;
            this.logger.Error(MediaServiceArchiverBackupResource.ArchiverBackupUpgradeBrowserServiceProcessExceptionError, e.ToString());
        }

        public override void Dispose()
        {
            if (indexLogicalDevice != null)
            {
                this.indexLogicalDevice.Close();
            }
        }
    }
}