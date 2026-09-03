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


using AvePoint.GCommon.Contract.Storage.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace AvePoint.GCommon.Contract.Server.ControlPanel
{
    public interface IStoragePolicyValidateAspect
    {
        /// <summary>
        /// 返回被占用的信息，没被占用的Storage Policy不可以加到返回值中
        /// 如果模块不需要验证此方法请返回null,不可以使用throw Exeption
        /// </summary>
        /// <param name="storagePolicyIds"></param>
        /// <returns></returns>
        List<ValidateResultDto> ValidateUsedStoragePolicy(List<string> storagePolicyIds);
       /// <summary>
        /// 返回被占用的信息，没被占用的Logical Device不可以加到返回值中
        /// 如果模块不需要验证此方法请返回null,不可以使用throw Exeption
       /// </summary>
       /// <param name="logicalDeviceIds"></param>
       /// <returns></returns>
        List<ValidateResultDto> ValidateUsedLogicalDevice(List<string> logicalDeviceIds);
        /// <summary>
        /// 返回被占用的信息，没被占用的Physical Device不可以加到返回值中
        /// 如果模块不需要验证此方法请返回null,不可以使用throw Exeption
        /// </summary>
        /// <param name="physicalDeviceIds"></param>
        /// <returns></returns>
        List<ValidateResultDto> ValidateUsedPhysicalDevice(List<string> physicalDeviceIds);
        /// <summary>
        /// 返回被占用的信息，没被占用的System Profile不可以加到返回值中
        /// 如果模块不需要验证此方法请返回null,不可以使用throw Exeption
        /// </summary>
        /// <param name="physicalDeviceIds"></param>
        /// <returns></returns>
        List<ValidateResultDto> ValidateUsedSystemProfile(List<string> physicalDeviceIds);
    }
}
