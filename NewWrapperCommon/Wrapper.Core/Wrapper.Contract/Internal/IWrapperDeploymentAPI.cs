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

using AvePoint.Wrapper.Core.Common;
using AvePoint.Wrapper.Core.Internal.Restore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Core.Internal
{
    /// <summary>
    /// Deployment API for export and import
    /// </summary>
    public interface IWrapperDeploymentAPI
    {
        /// <summary>
        /// 是否支持该环境
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        bool Support(WrapperSPMode mode, Version version);

        /// <summary>
        /// Site Import这个用于url已经存在。
        /// </summary>
        /// <param name="url"></param>
        /// <param name="accountInfo"></param>
        /// <param name="manager"></param>
        /// <returns></returns>
        ISiteImport CreateSiteImport(string url, Common.O365AccountInfo accountInfo, IImportObjectManager manager);

        /// <summary>
        /// 用于O365
        /// </summary>
        /// <param name="authentication"></param>
        /// <param name="manager"></param>
        /// <returns></returns>
        ISiteImport CreateSiteImport(IO365Authentication authentication, IImportObjectManager manager);

        /// <summary>
        /// Site Import，这个用于local，并且是manu input
        /// </summary>
        /// <param name="webApplicationUrl"></param>
        /// <param name="url"></param>
        /// <param name="manager"></param>
        /// <returns></returns>
        ISiteImport CreateSiteImport(string webApplicationUrl, string url, IImportObjectManager manager);

        /// <summary>
        /// File Import这个用户还原文件
        /// </summary>
        /// <param name="listImport"></param>
        /// <param name="folderUrl"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        IFileImport CreateFileImport(IListImport listImport, string folderUrl, string fileName);

        /// <summary>
        /// diable firing the sp event receiver
        /// </summary>
        bool SPEventReceiverFiringDisabled { get; set; }

        /// <summary>
        /// 初始化一些global内容，比如找assembly等控制
        /// </summary>
        void Initialize();

        /// <summary>
        /// Create Web Import
        /// </summary>
        /// <param name="siteImport"></param>
        /// <param name="siteRelativeURL">暂时只支持SiteRelativeURL，如“http://w03aio-02x64:1000/sites/sub1/sub2”，root web url是'',sub1 是“sub1”，sub2 是“sub1/sub2”,与Title没有关系</param>
        /// <returns></returns>
        IWebImport CreateWebImport(ISiteImport siteImport, string siteRelativeURL);
    }
}
