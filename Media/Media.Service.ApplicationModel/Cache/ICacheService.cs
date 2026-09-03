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




namespace AvePoint.Media.Service
{
    #region using directives
    using System;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.Media.Core.IO;
    using AvePoint.Media.Service.DomainModel;
    using Storage;

    #endregion

    public interface ICacheService
    {
        IXSystem CacheSystem { get; set; }

        void Open(CacheSettingDto cacheSetting, Boolean isDirectSystem, Boolean isBackup = default(Boolean));

        /// <summary>
        /// 将用于还原的数据块下载到cache，以解决磁带中交叉读取数据块导致的效率损失
        /// </summary>
        /// <param name="fileType">meta or content</param>
        /// <param name="highName">file path</param>
        /// <param name="lowName">file name</param>
        void DownloadDataFromDevice(IXConverter converter, IXSystem logicalDevice, FileType fileType, String highName, String lowName);

        void Clear(String dataVolume, String jobId, Int32 preFixNum);

        void Close();
    }
}