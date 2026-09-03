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
using AvePoint.Hybrid.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.JPMC
{
    /// <summary>
    /// Token returned for accepted jobs. Callers must report completion using the provided data.
    /// </summary>
    public sealed class JobExecutionHandle
    {
        private JobExecutionHandle(string tenantId, string? jobId, JobType jobType, IReadOnlyCollection<Guid> connectionGroupIds, double reservedWeight)
        {
            TenantId = tenantId;
            JobId = jobId;
            JobType = jobType;
            ConnectionGroupIds = connectionGroupIds;
            ReservedWeight = reservedWeight;
        }

        public string TenantId { get; }

        public string? JobId { get; }

        public JobType JobType { get; }

        public IReadOnlyCollection<Guid> ConnectionGroupIds { get; }

        public double ReservedWeight { get; }

        public static JobExecutionHandle Create(string tenantId, string? jobId, JobType jobType, IReadOnlyCollection<Guid> connectionGroupIds, double reservedWeight)
            => new JobExecutionHandle(tenantId, jobId, jobType, connectionGroupIds, reservedWeight);

    }
}
