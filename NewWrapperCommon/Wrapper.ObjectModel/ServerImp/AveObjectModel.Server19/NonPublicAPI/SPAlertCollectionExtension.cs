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

namespace AvePoint.ObjectModel.Server19.NonPublicAPI
{
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint;
    using System;
    using System.Reflection;
    using Delegate_SPAlertCollection_Add = Func<Microsoft.SharePoint.SPAlertCollection, string, System.Guid, uint, int, Microsoft.SharePoint.SPAlertFrequency, System.DateTime, Microsoft.SharePoint.SPAlertStatus,
        Microsoft.SharePoint.SPUser, string, int, string, string, string, bool, Microsoft.SharePoint.SPAlertDeliveryChannels, System.Guid, System.Guid>;

    [NonPublicAPI("Microsoft.SharePoint.SPAlertCollection")]
    static class SPAlertCollectionExtension
    {
        private static readonly Type TypeOfSPAlertCollection = typeof(SPAlertCollection);

        private static Delegate_SPAlertCollection_Add GetDelegate_Add()
        {
            var types = new Type[] { typeof(string), typeof(Guid), typeof(uint), typeof(int), typeof(SPAlertFrequency), typeof(DateTime),
                typeof(SPAlertStatus), typeof(SPUser), typeof(string), typeof(int), typeof(string),typeof(string), typeof(string),
                typeof(bool), typeof(SPAlertDeliveryChannels), typeof(Guid) };

            return TypeOfSPAlertCollection.GetMethod<Delegate_SPAlertCollection_Add>(nameof(Add), BindingFlags.Instance | BindingFlags.NonPublic, null, types, null);
        }

        public static Guid Add(this SPAlertCollection alertCollection, string alertTitle, Guid guidKey, uint uintKey, int eventType, SPAlertFrequency alertFrequency, DateTime alertTime,
            SPAlertStatus status, SPUser recipient, string bstrItemDocUrl, int alertTypeAndScopeBits, string strAlertTemplateName, string filter,
            string bstrProperties, bool bSendMail, SPAlertDeliveryChannels deliveryChannels, Guid alertId)
        {
            return GetDelegate_Add()(alertCollection, alertTitle, guidKey, uintKey, eventType, alertFrequency, alertTime, status, recipient,
                bstrItemDocUrl, alertTypeAndScopeBits, strAlertTemplateName, filter, bstrProperties, bSendMail, deliveryChannels, alertId);
        }
    }


}
