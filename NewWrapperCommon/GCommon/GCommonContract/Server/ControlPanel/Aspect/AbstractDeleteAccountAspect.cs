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



namespace AvePoint.GCommon.Contract.Server.ControlPanel.Aspect
{
    public abstract class AbstractDeleteAccountAspect
    {
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public virtual bool DeleteData(string accountId)
        {
            return true;
        }

        /// <summary>
        /// 删除记录，包括Job记录，Plan记录，及与各个模块相关的Setting
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public virtual bool DeleteRecords(string accountId)
        {
            return true;
        }

        /// <summary>
        /// 删除Control Panel中的Common设置，例如Email Notification Setting及Storage Policy，各个功能不需要重写此方法
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public virtual bool DeleteCommonSettings(string accountId)
        {
            return true;
        }
    }
}
