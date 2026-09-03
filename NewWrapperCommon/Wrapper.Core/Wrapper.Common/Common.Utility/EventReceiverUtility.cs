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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    /// <summary>
    /// 控制EventReceiver的开启和关闭，Dispose()时会恢复到使用之前的状态,保护现场；
    /// </summary>
    public class AveEventReceiverUtility : IDisposable
    {
        private IAveItemEventReceiver itemEventReceiver;
        private bool? eventReceiverEnabledStatus;
        private bool changed = false;

        /// <summary>
        /// 构造对象，并根据参数初始化EventReceiver(当前状态与要求的状态不一致时会修改;)
        /// </summary>
        /// <param name="EventReceiverEnabled"></param>
        public AveEventReceiverUtility(bool eventReceiverEnable)
        {
            itemEventReceiver = WrapperRuntime.CurrentContext.ModelFactory.CreateItemEventReceiver();
            eventReceiverEnabledStatus = itemEventReceiver.EventFiringEnabled;
            InitEventReceiver(eventReceiverEnable);
        }

        private void InitEventReceiver(bool eventReceiverEnable)
        {
            if (eventReceiverEnabledStatus == null || eventReceiverEnabledStatus.Value != eventReceiverEnable)
            {
                EnsureEventReceiver(eventReceiverEnable);
                changed = true;
            }
        }

        /// <summary>
        /// dispose()会恢复改变之前的状态；
        /// </summary>
        public void Dispose()
        {
            if (changed && eventReceiverEnabledStatus != null)
            {
                EnsureEventReceiver(eventReceiverEnabledStatus.Value);
            }
        }

        /// <summary>
        /// 根据参数开启或关闭EventReceiver;
        /// </summary>
        /// <param name="enable"></param>
        private void EnsureEventReceiver(bool enable)
        {
            if (enable)
            {
                itemEventReceiver.EventFiringEnabled = true;
            }
            else
            {
                itemEventReceiver.EventFiringEnabled = false;
            }
        }
    }
}
