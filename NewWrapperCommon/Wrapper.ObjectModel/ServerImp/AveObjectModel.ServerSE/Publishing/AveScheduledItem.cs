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



using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Publishing;
using System;
using Microsoft.SharePoint;


namespace AvePoint.ObjectModel.ServerSE
{
    class AveScheduledItem : IAveScheduledItem
    {
        private ScheduledItem mScheduledItem;

        public AveScheduledItem(ScheduledItem scheduledItem)
        {
            mScheduledItem = scheduledItem;
        }

        /// <summary>
        /// Construct for calling static member;
        /// </summary>
        public AveScheduledItem()
        { }

        #region IAveScheduledItem Members

        public bool IsScheduledItem(IAveListItem sourceListItem)
        {
            return false;
        }

        public IAveScheduledItem GetScheduledItem(IAveListItem sourceListItem)
        {
            return null;
        }

        public void SetScheduledItemStatus(IAveListItem listItem)
        {
        }

        public void DisableSchedulingOnList(IAveList list)
        {
            AveAssemblyUtility.InvokeStaticMethod(typeof(ScheduledItem), "DisableSchedulingOnList", new Type[] { typeof(SPList) }, new object[] { (list as AveList).List });
        }

        public void RegisterSchedulingEventOnList(IAveList list)
        {
            AveAssemblyUtility.InvokeStaticMethod(typeof(ScheduledItem), "RegisterSchedulingEventOnList", new Type[] { typeof(SPList) }, new object[] { (list as AveList).List });
        }

        public bool GetCanListPropertiesSupportScheduling(IAveList list)
        {
            return (bool)AveAssemblyUtility.InvokeStaticMethod(typeof(ScheduledItem), "GetCanListPropertiesSupportScheduling", new Type[] { typeof(SPList) }, new object[] { (list as AveList).List });
        }

        public bool GetIsSchedulingEventRegisteredOnList(IAveList list)
        {
            return (bool)AveAssemblyUtility.InvokeStaticMethod(typeof(ScheduledItem), "GetIsSchedulingEventRegisteredOnList", new Type[] { typeof(SPList) }, new object[] { (list as AveList).List });
        }

        #endregion
    }
}
