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

namespace AvePoint.RA.Contract.RMWeb.JobMonitor
{
    public class JMDetailsResult
    {
        public bool IsDeleted { get; set; }

        public int TotalNumber { get; set; }

        public bool Success { get; set; } = true;

        public IEnumerable<JMJobDetails> Details { get; set; }
        public JMProgressStatisticDetails JobProgressDetails { get; set; }

    }

    public class JMAOSPDetailsResult
    {
        public bool IsDeleted { get; set; }

        public int TotalNumber { get; set; }

        public bool Success { get; set; } = true;

        public List<JMAOSPRestoreActionJobDetailes> Details { get; set; }
    }

    public class HSMArchvierJobDetailsResult
    {
        public bool IsDeleted { get; set; }

        public int TotalNumber { get; set; }

        public bool Success { get; set; } = true;

        public List<JMHSMArchiverJobDetailes> Details { get; set; }
    }

    public class JMDetailsResultForApi
    {
        public bool IsDeleted { get; set; }

        public int TotalNumber { get; set; }

        public bool Success { get; set; } = true;

        public List<JMRestoreActionJobDetailes> Details { get; set; }
    }
    public class JMProgressStatisticDetails
    {
        public string ProcessedSites { get; set; } = string.Empty;
        public string ProcessedFiles { get; set; } = string.Empty;
        public string ProcessedSize { get; set; } = string.Empty;
        public string EstimatedFinishTime { get; set; } = string.Empty;
        public string LastUpdateTime { get; set; } = string.Empty;
        public bool IsNewJob { get; set; }
    }
}
