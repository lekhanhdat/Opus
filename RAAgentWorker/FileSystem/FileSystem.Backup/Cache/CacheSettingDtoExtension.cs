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




namespace AvePoint.GCommon.Contract.Server.ControlPanel.Object
{
    #region using directives
    using AvePoint.GCommon.Contract.Storage.Entity;
    using System;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon.Utility.Cryptography;
    #endregion

    ///<Summary>
    /// Extended the CacheSettingDto.
    ///</Summary>
    #region CodeReview
    [AveCodeReview(
    "2011/12/23",
    "yhzhang@avepoint.com",
    "dwxue@avepoint.com",
    new string[] { CodeReviewConstants.CHECK_LIST_ID_SECURITY_1},
    null,
    true)]
    #endregion
    public static class CacheSettingDtoExtension
    {
        public static LogicalDeviceDto ConvertToMediaLogicalDeviceDto(this CacheSettingDto cacheSetting)
        {
            LogicalDeviceDto logicalDevice = new LogicalDeviceDto();
            foreach (PathMap path in cacheSetting.Extension.Path)
            {
                if (string.IsNullOrEmpty(path.DiskInfo.Password))
                {
                    logicalDevice.PhysicalDrives.Add(PhysicalDeviceDto.GenterateFS(path.DiskInfo.Path, path.DiskInfo.UserName, path.DiskInfo.Password));
                }
                else
                {
                    var passWord = CspCommunicationWrapper.UnWrapKey(path.DiskInfo.Password);
                    var hardCodePassWord = CspCrossPlatformExchangeWrapper.WrapKeyToBase64String(passWord);
                    logicalDevice.PhysicalDrives.Add(PhysicalDeviceDto.GenterateFS(path.DiskInfo.Path, path.DiskInfo.UserName, hardCodePassWord));
                }
            }
            return logicalDevice;
        }
    }
}
