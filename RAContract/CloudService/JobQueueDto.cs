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
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.CloudService
{
    public class JobQueueDto
    {
        public string MessageId { get; set; }
        public JobType JobType { get; set; }
        public string Parameters { get; set; }
        public JobRunBy JobRunType { get; set; }
        public string TenantGroupId { get; set; }
        public string JobRunByUser { get; set; }
        public string PartnerUser { get; set; }
        public long CreatedTime { get; set; }
        public string Extension { get; set; }
        public string ClientIP { get; set; }
        public ProductType ProductType { get; set; }
        public JobPriority JobPriority { get; set; }
        public long UpdateTime { get; set; }
    }
}
