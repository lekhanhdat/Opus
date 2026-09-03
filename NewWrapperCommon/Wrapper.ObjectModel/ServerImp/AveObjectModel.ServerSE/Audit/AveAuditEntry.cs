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



using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using System;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveAuditEntry : IAveAuditEntry
    {
        private SPAuditEntry mAuditEntry;

        public AveAuditEntry(SPAuditEntry auditEntry)
        {
            mAuditEntry = auditEntry;
        }

        public string DocLocation
        {
            get { return mAuditEntry.DocLocation; }
        }

        public AveAuditEventType Event
        {
            get { return (AveAuditEventType)mAuditEntry.Event; }
        }

        public string EventData
        {
            get { return mAuditEntry.EventData; }
        }

        public string EventName
        {
            get { return mAuditEntry.EventName; }
        }

        public AveAuditEventSource EventSource
        {
            get { return (AveAuditEventSource)mAuditEntry.EventSource; }
        }

        public Guid ItemId
        {
            get { return mAuditEntry.ItemId; }
        }

        public AveAuditItemType ItemType
        {
            get { return (AveAuditItemType)mAuditEntry.ItemType; }
        }

        public AveAuditLocationType LocationType
        {
            get { return (AveAuditLocationType)mAuditEntry.LocationType; }
        }

        public string MachineIP
        {
            get { return mAuditEntry.MachineIP; }
        }

        public string MachineName
        {
            get { return mAuditEntry.MachineName; }
        }

        public DateTime Occurred
        {
            get { return mAuditEntry.Occurred; }
        }

        public Guid SiteId
        {
            get { return mAuditEntry.SiteId; }
        }

        public string SourceName
        {
            get { return mAuditEntry.SourceName; }
        }

        public int UserId
        {
            get { return mAuditEntry.UserId; }
        }
    }
}
