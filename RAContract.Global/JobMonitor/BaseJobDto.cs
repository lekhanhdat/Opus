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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.JobMonitor
{
    public class BaseJobDto
    {
        public string Id { get; set; }

        public string TaskName { get; set; }

        public int JobType { get; set; }

        public string Module { get; set; }

        public long StartTime { get; set; }

        public long EndTime { get; set; }

        public int Status { get; set; }

        public int Progress { get; set; }

        public string Message { get; set; }

        public string ScopeId { get; set; }

        public string ProfileName { get; set; }

        public string Description { get; set; }

        public string Comment { get; set; }

        public int? PlanType { get; set; }

        public string PlanId { get; set; }

        public string PlanName { get; set; }

        public string UserName { get; set; }

        public int? Category { get; set; }

        public int? IsTestRun { get; set; }

        public long LastUpdateTime { get; set; }

        public bool IsMergeRpt { get; set; }

        public string TenantGroupEmail { get; set; }
        public string SiteCollectionUrl { get; set; }
        public string JobtypeString { get; set; }
        public string SiteId { get; set; }
        //add for query job details
        public Dictionary<string, object> AddValues { get; set; }

        public bool NeedQueryFromUploadLocation { get; set; }

        public long SubJobCount { get; set; }
        public JobVersion JobVersion { get; set; }

        public bool IsMainJob { get; set; }
        public bool IsGettingProgress { get; set; }
    }
}
