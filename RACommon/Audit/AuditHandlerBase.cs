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
using AvePoint.RA.Contract.RMWeb.Audit;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Audit
{
    /// <summary>
    /// have reviewed by allen yin
    /// </summary>
    public interface IBeforeAuditHandler
    {
        /// <summary>
        /// before handler重载此方法，在此方法中需要获取old value，存于RMAuditInfo的ModifyContent中
        /// </summary>
        /// <param name="info">返回给外围的auditInfo</param>
        /// <param name="model">当多个方法共用一个handler的时候，利用此参数进行区分</param>
        /// <param name="category">当多个方法共用一个handler的时候，利用此参数进行区分</param>
        /// <param name="action">当多个方法共用一个handler的时候，利用此参数进行区分</param>
        /// <param name="args">被代理的方法的参数</param>
        /// <param name="target">被代理的方法所在对象实例</param>
        Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args , object target);

    }

    /// <summary>
    /// have reviewed by allen yin
    /// </summary>
    public interface IAfterAuditHandler
    {
        /// <summary>
        /// after handler重载此方法，需要在此方法中，返回一个RMAuditInfo，如果参数中info不为null，要直接在此参数基础上添加信息，并返回此实例
        /// 在 after handler中需要判断一下info中的E是否为null，如果E不为null，说明之前的方法有异常抛出
        /// </summary>
        /// <param name="info">before handler中返回的info</param>
        /// <param name="model"></param>
        /// <param name="category"></param>
        /// <param name="action"></param>
        /// <param name="args">被代理的方法的参数</param>
        /// <param name="target">被代理的方法所在对象实例</param>
        /// <param name="returnValue">被代理方法的返回值</param>
        /// <returns></returns>
        Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue);

    }

}
