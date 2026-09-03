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

using AvePoint.GCommon.Contract.Server.ControlPanel.ManagedAccount.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Aspect
{
    public interface IModifyAccountProfileAspect
    {
        /// <summary>
        /// 通知accountProfile改动
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        CheckResult NotifyAccountUpdate(AccountProfileDto account);

        /// <summary>
        /// 验证accountProfile是否改动是否通过
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        CheckResult ValidateAccountUpdate(AccountProfileDto account);

        /// <summary>
        /// 检查accountProfile是否被占用
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        CheckResult CheckAccountUse(AccountProfileDto account);
    }

    public class CheckResult
    {
        /// <summary>
        /// 被占用的名字（例如planName profileName等）
        /// </summary>
        public List<string> UsedByNames { get; set; }

        public string ModuleName { get; set; }

        public NotifyResult Result { get; set; }
    }

    public enum NotifyResult
    {
        /// <summary>
        ///传入的profile没有被应用
        /// </summary>
        NoUse = 0,
        /// <summary>
        /// 验证成功
        /// </summary>
        Successfull = 1,
        /// <summary>
        /// 验证失败
        /// </summary>
        Failed = 2,
    }
}
