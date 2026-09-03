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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Discovery.Job
{
    public enum RMDiscoveryJobVersion
    {
        //2024-02
        V1 = 0,
        //2024-08
        V2 = 1,
        // 2024-10
        V3 = 2,
        // 2025-02
        V4 = 3,
        // 2026-06
        V5 = 4,
    }

    public static class RMDiscoveryJobVersionExtension
    {
        public static JobType ToOffice365JobType(this RMDiscoveryJobVersion version)
        {
            return version switch
            {
                RMDiscoveryJobVersion.V5 => JobType.DiscoveryJobV5,
                RMDiscoveryJobVersion.V4 => JobType.DiscoveryJobV4,
                RMDiscoveryJobVersion.V3 => JobType.DiscoveryJobV3,
                _ => JobType.DiscoveryJobV2
            };
        }

        public static RMDiscoveryJobStatus ToOffice365ProfileJobInitStatus(this RMDiscoveryJobVersion version)
        {
            return version switch
            {
                RMDiscoveryJobVersion.V5 => RMDiscoveryJobStatus.Waiting,
                RMDiscoveryJobVersion.V4 => RMDiscoveryJobStatus.Waiting,
                RMDiscoveryJobVersion.V3 => RMDiscoveryJobStatus.Waiting,
                _ => RMDiscoveryJobStatus.None
            };
        }

        public static RMDiscoveryJobVersion ToOffice365AppendJobVersion(this RMDiscoveryJobVersion version)
        {
            return version switch
            {
                RMDiscoveryJobVersion.V5 => RMDiscoveryJobVersion.V5,
                RMDiscoveryJobVersion.V4 => RMDiscoveryJobVersion.V4,
                RMDiscoveryJobVersion.V3 => RMDiscoveryJobVersion.V3,
                _ => RMDiscoveryJobVersion.V2
            };
        }

        public static RMDiscoveryJobVersion ToOffice365RetryVersion(this RMDiscoveryJobVersion version)
        {
            return version switch
            {
                RMDiscoveryJobVersion.V5 => RMDiscoveryJobVersion.V5,
                RMDiscoveryJobVersion.V4 => RMDiscoveryJobVersion.V4,
                RMDiscoveryJobVersion.V3 => RMDiscoveryJobVersion.V3,
                _ => RMDiscoveryJobVersion.V2
            };
        }

        public static bool IsOffice365NewVersion(this RMDiscoveryJobVersion version)
        {
            return version == RMDiscoveryJobVersion.V5 || version == RMDiscoveryJobVersion.V4 || version == RMDiscoveryJobVersion.V3;
        }
    }
}
