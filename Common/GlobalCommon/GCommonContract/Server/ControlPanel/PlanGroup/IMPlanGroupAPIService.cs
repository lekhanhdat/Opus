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



using AvePoint.GCommon.Contract.Server.ControlPanel.PlanGroup.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.PlanGroup
{
    public interface IMPlanGroupAPIService
    {
        /// <summary>
        /// 通过PlanGroupParaDto创建一个Job
        /// </summary>
        /// <param name="dto">PlanGroupParaDto中的属性除了Job Id外, PlanId, PlanOrder, Category, PlanGroupId都会赋上</param>
        /// <returns>PlanGroupJobResult中需要返回Job Id, 创建出来Job的状态</returns>
        PlanGroupJobResult CreatePlanGroupJob(PlanGroupParaDto dto);

        /// <summary>
        /// 根据PlanGroupParaDto运行之前创建好的Job
        /// </summary>
        /// <param name="dto">PlanGroupParaDto中的属性都会赋上</param>
        void RunPlanGroupJob(PlanGroupParaDto dto);
    }
}
