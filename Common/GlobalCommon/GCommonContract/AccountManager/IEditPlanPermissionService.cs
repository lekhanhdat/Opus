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


using System.Collections.Generic;
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.Server.Common;

namespace AvePoint.GCommon.Contract.AccountManager
{
    public interface IEditPlanPermissionService
    {
        /// <summary>
        /// 编辑plan时，更新siteCollection节点的权限
        /// </summary>
        /// <param name="plan">编辑的plan</param>
        /// <param name="siteCollectionIds">编辑plan中的siteCollection节点</param>
        void EditPermission(PlanDto plan, List<string> siteCollectionIds);
        /// <summary>
        /// 编辑plan时判断是否需要提示share siteCollection
        /// </summary>
        /// <param name="plan">编辑的plan</param>
        /// <param name="siteCollectionIds">plan中选中新加的siteCollection节点</param>
        /// <returns></returns>
        EditPlanCheckResult CheckPlanNeedShareSiteCollection(PlanDto plan, List<string> siteCollectionIds);
    }
}
