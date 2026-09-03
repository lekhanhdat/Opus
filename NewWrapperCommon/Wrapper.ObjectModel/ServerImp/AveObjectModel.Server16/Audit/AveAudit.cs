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

namespace AvePoint.ObjectModel.Server16
{
    class AveAudit : IAveAudit
    {
        private SPAudit mAudit;

        public AveAudit(SPAudit spAudit)
        {
            mAudit = spAudit;
        }

        #region IAveAudit Members

        public AveAuditMaskType AuditFlags
        {
            get
            {
                return (AveAuditMaskType)mAudit.AuditFlags;
            }
            set
            {
                mAudit.AuditFlags = (SPAuditMaskType)value;
            }
        }

        public bool UseAuditFlagCache
        {
            get
            {
                return mAudit.UseAuditFlagCache;
            }
            set
            {
                mAudit.UseAuditFlagCache = value;
            }
        }

        public void Update()
        {
            mAudit.Update();
        }

        public void TrimAuditLog(DateTime deleteEndDate)
        {
            mAudit.TrimAuditLog(deleteEndDate);
        }

        #endregion

        public IAveAuditEntryCollection GetEntries(IAveAuditQuery query)
        {
            SPAuditQuery tempQuery = (query == null) ? null : (query as AveAuditQuery).AuditQuery;
            return new AveAuditEntryCollection(mAudit.GetEntries(tempQuery));
        }

        public IAveAuditEntryCollection GetEntries()
        {
            return new AveAuditEntryCollection(mAudit.GetEntries());
        }




        public AveAuditMaskType EffectiveAuditMask
        {
            get { return (AveAuditMaskType)mAudit.EffectiveAuditMask; }
		}

        public bool WriteAuditEventUnlimitedData(AveAuditEventType eventId, string eventSource, string xmlData)
        {
            return mAudit.WriteAuditEventUnlimitedData((SPAuditEventType)eventId, eventSource, xmlData);
        }

        public bool WriteAuditEventUnlimitedData(string eventName, string eventSource, string xmlData)
        {
            return mAudit.WriteAuditEventUnlimitedData(eventName, eventSource, xmlData);
        }
    }
}
