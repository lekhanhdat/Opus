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

namespace AvePoint.ObjectModel.Server13
{
    class AveAuditQuery:IAveAuditQuery
    {
        private SPAuditQuery mAuditQuery;

        public AveAuditQuery(SPAuditQuery spAuditQuery)
        {
            mAuditQuery=spAuditQuery;
        }

        public AveAuditQuery(IAveSite site)
        {
            SPSite tempSite = (site == null) ? null : (site as AveSite).Site;
            mAuditQuery = new SPAuditQuery(tempSite);
        }

        internal SPAuditQuery AuditQuery
        {
            get
            {
                return mAuditQuery;
            }
        }

        public bool? HasMoreItems
        {
            get { return mAuditQuery.HasMoreItems; }
        }

        public uint RowLimit
        {
            get
            {
                return mAuditQuery.RowLimit;
            }
            set
            {
                mAuditQuery.RowLimit=value;
            }
        }

        public void AddEventRestriction(AveAuditEventType eventId)
        {
            mAuditQuery.AddEventRestriction((SPAuditEventType)eventId);
        }

        public void RestrictToList(IAveList list)
        {
            SPList tempList=(list==null)?null:(list as AveList).List;
            mAuditQuery.RestrictToList(tempList);
        }

        public void RestrictToListItem(IAveListItem listItem)
        {
            SPListItem tempListItem=(listItem==null)?null:(listItem as AveListItem).ListItem;
            mAuditQuery.RestrictToListItem(tempListItem);
        }

        public void RestrictToUser(int userId)
        {
            mAuditQuery.RestrictToUser(userId);
        }

        public void SetRangeEnd(DateTime end)
        {
            mAuditQuery.SetRangeEnd(end);
        }

        public void SetRangeStart(DateTime start)
        {
            mAuditQuery.SetRangeStart(start);
        }
    }
}
