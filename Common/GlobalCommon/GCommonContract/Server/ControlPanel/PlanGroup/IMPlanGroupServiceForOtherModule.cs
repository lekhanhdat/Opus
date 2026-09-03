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
using AvePoint.GCommon.Contract.Server.ControlPanel.PlanGroup.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.PlanGroup
{
    public interface IMPlanGroupServiceForOtherModule
    {
        /// <summary>
        /// 用于load已经创建的PlanGroup
        /// </summary>
        /// <returns>PlanGroupDtoForOtherModule中PlanGroupName和PlanGroupId的值会赋上</returns>
        List<PlanGroupDtoForOtherModule> LoadExistedPlanGroups();

        /// <summary>
        /// 用于创建plan时，将plan添加到选中的plangroup中
        /// </summary>
        /// <param name="dto">PlanGroupDtoForOtherModule中的属性PlanId，Category，PlanGroupIds需要赋上值</param>
        /// <returns>返回添加的状态</returns>
        PlanGroupResult AddPlanToSelectedPlanGroups(PlanGroupDtoForOtherModule dto);

        /// <summary>
        /// 删除包含在plangroup中的plan时使用
        /// </summary>
        /// <param name="dtos">PlanGroupDtoForOtherModule中的属性PlanId，Category，PlanGroupIds需要赋上值</param>
        /// <returns>返回删除的状态</returns>
        PlanGroupResult RemovePlansFromPlanGroups(List<PlanGroupDtoForOtherModule> dtos);

        /// <summary>
        /// 更新plan时使用(该方法需要在各个模块调用update plan方法之前被调用)
        /// </summary>
        /// <param name="dto">PlanGroupDtoForOtherModule中的属性PlanId，Category，PlanGroupIds需要赋上值</param>
        /// <returns></returns>
        PlanGroupResult UpdatePlanOfPlanGroup(PlanGroupDtoForOtherModule dto);
    }
}
