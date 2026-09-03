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
using AvePoint.RA.Contract.Workflow;
using System;

namespace AvePoint.RA.Workflow.Common
{
    public interface IWFService<T> where T: BaseReviewRequestInfo
    {
        /// <summary>
        /// 启动一个workflow instance
        /// </summary>
        /// <returns>workflow 实例的GUID</returns>
        Guid StartWorkflow(T request, string definitionXamlStr);

        /// <summary>
        /// 取消workflow
        /// </summary>
        void Cancel(T request, string definitionXamlStr);

        /// <summary>
        /// 继续执行workflow
        /// </summary>
        /// <param name="bookmark">下一个可能的bookmark name</param>
        void Resume(T request, string definitionXamlStr, string bookmark = null);
    }
}
