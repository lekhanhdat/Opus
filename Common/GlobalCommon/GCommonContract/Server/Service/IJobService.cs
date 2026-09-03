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



using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using System.Collections.Generic;
namespace AvePoint.GCommon.Contract.Server.Service
{
    public interface IJobService
    {
        ///// <summary>
        ///// 创建Job
        ///// 为Job Creator创建full Permission
        ///// 为plan owner创建full Permission
        ///// 为plan owner所在Group的Owner和Power User创建full Permission
        ///// </summary>
        ///// <param name="plan">执行Job的Plan</param>
        ///// <param name="job"></param>
        ///// <returns>jobId</returns>
        //string CreateJob(PlanDto plan, BaseJobDto job);

        /// <summary>
        /// 删除Job，清空Job上的Permissions
        /// </summary>
        /// <param name="jobId"></param>
        void DeleteJob(string jobId);

        List<BaseJobDto> GetAllJobs(long finishTime);
    }
}
