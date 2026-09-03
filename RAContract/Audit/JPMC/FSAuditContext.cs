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
using AvePoint.RA.Contract.RMWeb.Audit;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.Contract.Audit.JPMC
{
    public sealed class FSAuditContext
    {
        public FSAuditType AuditType { get; set; }
        public FSAuditLevel AuditLevel { get; set; }
        public FSAuditExecutedBy ExecutedBy { get; set; }
        public AuditStatus Status { get; set; }
        public Guid ConnectionGroupId { get; set; }
        public Guid ConnectionId { get; set; }
        public Guid ItemId { get; set; }
        public string CurrentPath { get; set; }
        public string PreviousPath { get; set; }
        public List<FSAuditModifiedContent> ModifiedContents { get; set; } = new();
        public string UserName { get; set; }
        public string ErrorMessage { get; set; }
        public string ClientIP { get; set; }
        public long ActionTimeUtc { get; set; }
        public string ObjectName { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();

        public static FSAuditContext GetNewContext(FSAuditType auditType, FSAuditLevel auditLevel)
        {
            return new FSAuditContext
            {
                AuditType = auditType,
                AuditLevel = auditLevel,
                Status = AuditStatus.Successful,
                ActionTimeUtc = DateTime.UtcNow.Ticks
            };
        }

        public void SetMetadata(string key, object value)
        {
            Metadata[key] = value;
        }

        public FSAuditContext AddModifiedContent(string targetSetting, string oldValue, string newValue)
        {
            ModifiedContents.Add(new FSAuditModifiedContent
            {
                Id = Guid.NewGuid(),
                TargetSetting = targetSetting,
                OldValue = oldValue,
                NewValue = newValue
            });
            return this;
        }
    }

    public sealed class FSAuditModifiedContent
    {
        public Guid Id { get; set; }
        public string TargetSetting { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}