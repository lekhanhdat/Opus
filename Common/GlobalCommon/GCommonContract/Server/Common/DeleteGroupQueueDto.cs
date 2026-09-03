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

namespace AvePoint.GCommon.Contract.Server.Common
{
    public class DeleteGroupQueueDto
    {
        public string Id { get; set; }
        public string TenantGroupId { get; set; }
        public DeleteGroupStatus Status { get; set; }
        public long StartTime { get; set; }
        public int RetryCount { get; set; }
        public DeleteGroupTaskStatus TaskStatus { get; set; }

        public override string ToString()
        {
            return string.Format("DeleteGroupQueueDto[Id {0}, TenantGroupId {1}, Status {2}, RetryCount {3}, TaskStatus {4}]", Id, TenantGroupId, (int)Status, RetryCount, (int)TaskStatus);
        }
    }

    public enum DeleteGroupStatus
    {
        Waiting = 0,
        Running = 1,
        Finished = 2,
        Failed = 3
    }

    [Flags]
    public enum DeleteGroupTaskStatus
    {
        None = 0,
        DeleteComplete = 1,
        MoveComplete = 2,
        DeleteControlComplete = 4,
        DeleteGroupInfoComplete = 8
    }
}
