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


using System.Collections.Generic;
using AvePoint.GCommon.Contract.PlatformRecovery.Object;
using AvePoint.GCommon.Contract.Server.Common;

namespace AvePoint.GCommon.Contract.Server.Retention
{
    public class RetentionRequest
    {
        public int JobType { get; set; }
        public string UserName { get; set; }
        public string PlanName { get; set; }
        public string TriggerJobId { get; set; }
        public string RetentionJobId { get; set; }
        public ObjectInfoDto ObjectInfo { get; set; }
        /// <summary>
        /// Tenant User filter data
        /// </summary>
        public string BackupPlanId { get; set; }
        public PRPlatformType PlatformType { get; set; }
        public string NotificationProfileId { get; set; }
        public List<string> StoragePolicyIds { get; set; }
        public RetentionActionType ActionType { get; set; }
    }
}
