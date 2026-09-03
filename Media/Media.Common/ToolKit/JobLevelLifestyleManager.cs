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




namespace AvePoint.Media.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using Castle.MicroKernel;
    using Castle.MicroKernel.Context;
    using Castle.MicroKernel.Lifestyle;
    #endregion

    public class JobLevelLifestyleManager : AbstractLifestyleManager
    {
        static Dictionary<String, Object> cachedJobData = new Dictionary<String, Object>();

        public override object Resolve(CreationContext context, IReleasePolicy releasePolicy)
        {
            var result = default(Object);
            lock (cachedJobData)
            {
                var jobId = JobIdManager.JobId;
                if (cachedJobData.ContainsKey(jobId))
                {
                    result = cachedJobData[jobId];
                }
                else
                {
                    result = base.Resolve(context, releasePolicy);
                    cachedJobData.Add(jobId, result);
                }
            }
            return result;
        }

        public static void Remove(String jobId)
        {
            lock (cachedJobData)
            {
                if (cachedJobData.ContainsKey(jobId))
                {
                    cachedJobData.Remove(jobId);
                }
            }
        }

        public override void Dispose()
        { }
    }
}
