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





namespace AvePoint.GCommon
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Security.Principal;

    #endregion

    /// <summary>
    /// <example>下面的例子说明了如何使用这个类
    /// <code>
    /// using(AveAppPoolExecuter appPool=new AveAppPoolExecuter())
    /// {
    ///     ......do something under app pool account......
    /// }
    /// </code>
    /// </example>
    /// </summary>
    public class AveAppPoolExecuter : IDisposable
    {
        WindowsIdentity origIden = null;

        /// <summary>
        /// 构造函数，当一个对象new成功之后，identity已经变成了app pool user,需要手动或using自动调用Dispose返回
        /// </summary>
        public AveAppPoolExecuter()
        {
            this.origIden = WindowsIdentity.GetCurrent();
            Win32Native.RevertToSelf();
        }

        public void Dispose()
        {
            if (origIden != null)
            {
                //origIden.Impersonate();
                origIden = null;
            }
        }
    }
}
