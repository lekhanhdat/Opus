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

namespace AvePoint.RA.Contract.TenantUpgrade
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RMUpgradeConfigAttribute : Attribute
    {
        /// <summary>
        /// [Require]
        /// Upgrade feature
        /// </summary>
        public RMUpgradeFeature Feature { get; }

        /// <summary>
        /// [Require]
        /// Upgrade release/hotfix version
        /// </summary>
        public RMUpgradeVersion Version { get; }

        /// <summary>
        /// [Require]
        /// Upgrade logic execution mode.
        /// </summary>
        public RMUpgradeExecutionMode ExecutionMode { get; }

        /// <summary>
        /// [Option]
        /// Does need to be retry if unsuccessful,
        /// Default true.
        /// </summary>
        public bool UnsuccessfulNeedRetry { get; set; } = true;

        /// <summary>
        /// [Option]
        /// Count of retries in case of failure or exception,
        /// Default 3.
        /// </summary>
        public int RetryTimes { get; set; } = 3;

        public RMUpgradeConfigAttribute(RMUpgradeFeature feature, RMUpgradeVersion version, RMUpgradeExecutionMode executionMode)
        {
            Feature = feature;
            Version = version;
            ExecutionMode = executionMode;
        }
    }
}
