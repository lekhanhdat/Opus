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
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.DB.Model;
using System.Collections.Generic;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMAOSNotificationDao
    {
        void Add(RMAosQueueMessage message);
        void Delete(string id);
        void DeleteAll(string tenantId);
        List<RMAosQueueMessage> GetSyncNodeMessages(string tenantId, List<int> types);
        List<string> GetPendingTenants(List<int> types, long timePeriod);
        RMAosQueueMessage GetSyncAOSSecurityProfileMessage(string tenantId);
        List<RMAosQueueMessage> GetChangeTenantOwnerMessage();
        void Refresh(RMAosQueueMessage message);
    }
}
