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
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.JobMonitor
{
    [DataContract]
    public class JMDetailsQuery
    {
        [DataMember]
        public string JobID { get; set; }
        [DataMember]
        public int JobType { get; set; }
        [DataMember]
        public string SearchValue { get; set; }
        [DataMember]
        public string[] SearcheKeys { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public int CurrentPage { get; set; }
        [DataMember]
        public JobDetailsStatus[] StatusFilters { get; set; }
        [DataMember]
        public int[] EntityTypeFilters { get; set; }
        [DataMember]
        public ActionTab[] ActionTabFilters { get; set; }
        [DataMember]
        public string[] ArchiverActionFilters { get; set; }

        // For sub job details
        [DataMember]
        public JobStatus[] SubJobStatusFilters { get; set; }
    }
}
