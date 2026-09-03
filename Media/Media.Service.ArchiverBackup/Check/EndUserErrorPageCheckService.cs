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
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using Merged18NResources.MediaServiceArchiverBackup;
    using AvePoint.Media.Service.DomainModel;
    using Storage;

    #endregion using directives

    #region CodeReview

    [AveCodeReview(
    "2012/6/20",
    "dwxue@avepoint.com",
    "yjhuo@avepoint.com",
    new string[] { },
    null,
    true)]

    #endregion CodeReview

    public class EndUserErrorPageCheckService
        : CheckServiceBase<ErrorPageCheckInfo, ErrorPageCheckResult>
        , ICheckService
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        IXSystem indexLogicalDevice;

        public IIndexService<ArchiverIndexServiceOpenParameter> IndexService { get; set; }

        public IErrorPageCheckIndexService CheckIndexService { get; set; }

        public override void Open(ErrorPageCheckInfo checkInfo)
        {
            this.indexLogicalDevice = this.StorageDeviceManager.Open(checkInfo.LogicalDevice.GetXRIS(PhysicalDeviceUsage.Index));
            this.IndexService.Open(new ArchiverIndexServiceOpenParameter(checkInfo, this.indexLogicalDevice));
        }

        public override ErrorPageCheckResult Check(ErrorPageCheckInfo checkInfo)
        {
            var result = new ErrorPageCheckResult();
            this.logger.Info(MediaServiceArchiverBackupResource.EndUserErrorPageCheckServiceCheckStartCheckingUrl, checkInfo.Url);
            //http://win-irerpkpbom7:1200
            if (checkInfo.Url.EqualsIgnoreCase(checkInfo.SiteUrl))
                result.IsArchived = this.CheckIndexService.CheckSiteCollection(checkInfo.SiteUrl);
            //http://win-irerpkpbom7:1200/SitePages/Home.aspx
            else if (checkInfo.Url.StartsWith(checkInfo.SiteUrl, StringComparison.OrdinalIgnoreCase))
            {
                var url = checkInfo.Url;
                //http://win-irerpkpbom7:1200/120004DL/Forms/AllItems.aspx?RootFolder=%2F120004DL%2FFolder001&FolderCTID=0x0120001ACBDA2776D38746896C603FC9540763&View={1FFE0C15-4FAC-4389-A0D5-F4169631E84B}
                if (checkInfo.Url.Contains("?RootFolder="))
                {
                    url = this.ProcessFolderUrl(checkInfo.Url, checkInfo.WebAppUrl);
                    result.IsArchived = this.CheckIndexService.CheckNormalUrl(url);
                }
                //http://win-irerpkpbom7:1200/_layouts/listform.aspx?PageType=4&ListId={CE293122-FE4F-4080-90B3-FA0193C952B6}&ID=1&ContentTypeID=0x01005F5479491157AB4CB6BC98F64A8821D1
                else if (checkInfo.Url.Contains("?PageType="))
                {
                    url = this.ProcessItemUrl(checkInfo.Url);
                    result.IsArchived = this.CheckIndexService.CheckItemUrl(url);
                }
                //http://10.1.16.26:20010/sites/jap/Shared%20Documents/ドキュメン.aspx?PageVersion=512
                else if (checkInfo.Url.Contains("?PageVersion="))
                {
                    url = this.ProcessDocumentUrl(checkInfo.Url);
                    result.IsArchived = this.CheckIndexService.CheckNormalUrl(url);
                }
                else
                    result.IsArchived = this.CheckIndexService.CheckNormalUrl(url);
            }
            this.logger.Info(MediaServiceArchiverBackupResource.EndUserErrorPageCheckServiceCheckFinished, result.IsArchived);
            return result;
        }

        private String ProcessDocumentUrl(String url)
        {
            var version = url.Substring(url.LastIndexOf("?PageVersion=", StringComparison.OrdinalIgnoreCase) + 13);
            var listUrl = url.Remove(url.LastIndexOf("/", StringComparison.OrdinalIgnoreCase));
            var preUrl = listUrl.Remove(listUrl.LastIndexOf("/", StringComparison.OrdinalIgnoreCase));
            var temp = url.Substring(preUrl.Length);
            var postUrl = temp.Remove(temp.LastIndexOf("?PageVersion=", StringComparison.OrdinalIgnoreCase));
            var result = preUrl + "/_vti_history/" + version + postUrl;
            return result;
        }

        private String ProcessItemUrl(String url)
        {
            var temp = url.Substring(url.LastIndexOf("ListId={", StringComparison.OrdinalIgnoreCase));
            var result = temp.Remove(temp.LastIndexOf("&ContentTypeID=", StringComparison.OrdinalIgnoreCase));
            return result;
        }

        private String ProcessFolderUrl(String url, String webAppUrl)
        {
            var start = url.LastIndexOf("?RootFolder=", StringComparison.OrdinalIgnoreCase);
            var end = url.IndexOf("&", StringComparison.OrdinalIgnoreCase);
            var tempUrl = url;
            //http://win-irerpkpbom7:1200/120004DL/Forms/AllItems.aspx?RootFolder=%2F120004DL%2FFolder001&FolderCTID=0x0120001ACBDA2776D38746896C603FC9540763&View={1FFE0C15-4FAC-4389-A0D5-F4169631E84B}
            if (end != -1)
                tempUrl = tempUrl.Remove(end).Substring(start + 13);
            //http://win-irerpkpbom7:1200/120004DL/Forms/AllItems.aspx?RootFolder=%2F120004DL%2FFolder001
            else
                tempUrl = tempUrl.Substring(start + 13);
            var listUrl = url.Substring(0, start);
            //http://win-irerpkpbom7:1200/Lists/120003CL/AllItems.aspx?RootFolder=%2FLists%2F120003CL%2FLists&FolderCTID=0x012000E2E5149895EE9C4BB27C571AD108204D&View={1BC2797D-121C-4850-A150-44374653B008}&InitialTabId=Ribbon%2EListItem&VisibilityContext=WSSTabPersistence
            var resultUrl = webAppUrl + tempUrl;
            return resultUrl;
        }

        public override void ProcessException(Exception e)
        {
            e = e.InnerException ?? e;
            this.logger.Error(MediaServiceArchiverBackupResource.EndUserErrorPageCheckServiceProcessExceptionError, e.ToString());
        }

        public override void Dispose()
        {
            this.IndexService.Close();
            this.StorageDeviceManager.Close(this.indexLogicalDevice);
            this.logger.Info(MediaServiceArchiverBackupResource.EndUserErrorPageCheckServiceDisposeFinished);
        }
    }
}