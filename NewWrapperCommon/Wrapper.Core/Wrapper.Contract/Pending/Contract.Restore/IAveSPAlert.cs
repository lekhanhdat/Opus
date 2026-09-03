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
namespace AvePoint.Wrapper.Restore
{
     public interface IAveSPAlert
    {
        void Disposed();
        AvePoint.Wrapper.Common.AveAlertDeliveryChannels GetDeliveryChannel(int mask);
        AvePoint.Wrapper.Common.AveEventType GetEventType(int mask);
        AvePoint.Wrapper.Common.AveAlertFrequency GetFrequency(int mask);
        AvePoint.Wrapper.Common.IReport GetReport();
        void RestoreAlert(System.Collections.Generic.Dictionary<string, object> data, bool isSchedAlert);
        void RestoreAlerts(System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>> iAlertInfoList, bool isSchedAlert);
        void UpdatePrivateProperties(AvePoint.Wrapper.Common.IAveAlert alert, System.Collections.Generic.Dictionary<string, object> data);
        void UpdateSchedProperties(AvePoint.Wrapper.Common.IAveAlert alert, System.Collections.Generic.Dictionary<string, object> data);
        void UpdateSharedProperties(AvePoint.Wrapper.Common.IAveAlert alert, System.Collections.Generic.Dictionary<string, object> data, IAveSPWeb aveWeb);
    }
}
