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

namespace AvePoint.RA.Contract.Audit.JPMC
{
    public class FSAuditRecord
    {
        public Guid Id { get; init; }

        public int AuditType { get; init; }
        public string AuditTypeStr { get; init; }
        public int AuditLevel { get; init; }

        public string Content { get; init; }

        public long ActionTimeUtc { get; init; }

        public string UserName { get; init; }

        public string ClientIP { get; init; }
       
        public string FormattedTime { get; set; }
        
        public int Status { get; init; }
       
        public string StatusStr { get; set; }
        
        public string ObjectName { get; init; }

        public string ConnectionGroupId { get; set; }
        
        public string ConnectionId { get; set; }
        
        public string ItemId { get; set; }
        
        public string CurrentPath { get; set; }

        public string PreviousPath { get; set; }
    }
}
