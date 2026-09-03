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

namespace AvePoint.GCommon.Utility
{
    public abstract class  Startable:IStartable
    {
        /// <summary>
        /// 在start开始时触发的事件
        /// </summary>
        public event System.EventHandler OnStarting;
        /// <summary>
        /// 在start完成后触发的事件
        /// </summary>
        public event System.EventHandler OnStarted;
        /// <summary>
        /// 在stop开始时触发的事件
        /// </summary>
        public event System.EventHandler OnStopping;
        /// <summary>
        /// 在stop完成后触发的事件
        /// </summary>
        public event System.EventHandler OnStopped;

        public void Start()
        {
            if (OnStarting != null)
            {
                OnStarting(this, EventArgs.Empty);
            }

            InternalStart();

            if (OnStarted != null)
            {
                OnStarted(this, EventArgs.Empty);
            }
        }

        public void Stop()
        {
            if (OnStopping != null)
            {
                OnStopping(this, EventArgs.Empty);
            }

            InternalStop();

            if (OnStopped != null)
            {
                OnStopped(this, EventArgs.Empty);
            }
        }

        public abstract void InternalStart();
        public abstract void InternalStop();

    }
}
