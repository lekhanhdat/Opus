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
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using Merged18NResources.MediaServiceArchiverBackup;
    using AvePoint.Media.Service.DomainModel;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Storage;
    #endregion

    #region CodeReview

    [AveCodeReview(
    "2012/3/21",
    "dwxue@avepoint.com",
    "yjhuo@avepoint.com",
    new string[] { },
    null,
    true)]
    #endregion

    public class EndUserArchiverBrowserService
        : BrowserServiceBase<EndUserBrowseInfo, EndUserBrowseResult>
        , IBrowserService
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        IXSystem indexLogicalDevice;

        public IIndexService<ArchiverIndexServiceOpenParameter> IndexService { get; set; }
        public IEndUserBrowserIndexService BrowserIndexService { get; set; }

        public override void Open(EndUserBrowseInfo browserInfo)
        {
            this.logger.Info(MediaServiceArchiverBackupResource.EndUserArchiverBrowserServiceOpenInfo);
            this.indexLogicalDevice = this.StorageDeviceManager.Open(browserInfo.IndexLogicalDevice.ToXRIS());
            this.IndexService.Open(new ArchiverIndexServiceOpenParameter(browserInfo, this.indexLogicalDevice));
        }

        public override EndUserBrowseResult Browse(EndUserBrowseInfo browserInfo)
        {
            this.logger.Info(MediaServiceArchiverBackupResource.EndUserArchiverBrowserServiceBrowseStartView, browserInfo.PathMD5);
            var result = new EndUserBrowseResult();
            var currentIndex = this.BrowserIndexService.GetCurrentIndex(browserInfo.PathMD5);
            var currentNode = new EndUserTreeNode(currentIndex);
            if (!browserInfo.BrowseFoldersOnly)
                this.AddParentNode(currentNode);
            this.logger.Info(MediaServiceArchiverBackupResource.EndUserArchiverBrowserServiceBrowseInfo, currentNode.Url);
            var childIndexList = this.BrowserIndexService.GetChildIndexList(new ArchiverIndexInfo(browserInfo));
            if (browserInfo.BrowseFoldersOnly)
                childIndexList = childIndexList.FindAll(index => index.Type.EqualsIgnoreCase("F"));
            if (currentIndex.Type.EqualsIgnoreCase("W") && currentIndex.Name.EqualsIgnoreCase("."))
                childIndexList.AddRange(this.BrowserIndexService.GetChildIndexList(currentIndex.ParentPathMD5));
            if (browserInfo.Length != 0)
            {
                var childCount = this.BrowserIndexService.GetChildCount(browserInfo.PathMD5);
                if (childCount > browserInfo.OffSet + browserInfo.Length)
                    currentNode.HasNextPage = true;
            }
            var childNodeList = new List<EndUserTreeNode>();
            childIndexList.ForEach(index => { childNodeList.Add(new EndUserTreeNode(index, currentNode.Url)); });
            currentNode.ChildNodes = childNodeList;
            result.Node = currentNode;
            this.logger.Info(MediaServiceArchiverBackupResource.EndUserArchiverBrowserServiceBrowseChildrenCount, result.Node.ChildNodes.Count);
            return result;
        }

        public override void ProcessException(Exception e)
        {
            e = e.InnerException ?? e;
            this.logger.Error(MediaServiceArchiverBackupResource.EndUserArchiverBrowserServiceProcessExceptionError, e.ToString());
        }

        public override void Dispose()
        {
            if (IndexService != null)
                this.IndexService.Close();
            this.StorageDeviceManager.Close(this.indexLogicalDevice);
            this.logger.Info(MediaServiceArchiverBackupResource.EndUserArchiverBrowserServiceDisposeFinish);
        }

        void AddParentNode(EndUserTreeNode currentNode)
        {
            var currentIndex = this.BrowserIndexService.GetCurrentIndex(currentNode.NodeMd5Value);
            if (!currentIndex.Type.EqualsIgnoreCase("E"))
            {
                var parentNode = new EndUserTreeNode();
                var parentIndex = this.BrowserIndexService.GetParentIndex(currentNode.NodeMd5Value);
                var position = parentIndex.Name.Contains("\\") ? parentIndex.Name.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) : parentIndex.Name.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                var tempName = parentIndex.Name.Substring(position + 1);
                parentNode.Level = parentIndex.Type.ToNodeLevelByMediaDataTypeString().ToString().ToEnum<TreeNodeLevel>();
                parentNode.Name = AveConverter.DecodeSpecialChar(tempName);
                parentNode.NodeMd5Value = parentIndex.PathMD5;
                this.AddParentNode(parentNode);
                currentNode.ParentNode = parentNode;
                if (currentIndex.Type.EqualsIgnoreCase("W") && parentNode.Url.Contains("\\"))
                    currentNode.Url = parentNode.Url + "/" + currentNode.Name;
                else currentNode.Url = parentNode.Url + "\\" + currentNode.Name;
                this.logger.Info(MediaServiceArchiverBackupResource.EndUserArchiverBrowserServiceAddParentNodeUrl, currentNode.Url);
            }
            else
                currentNode.Url = currentIndex.Name;
        }
    }
}