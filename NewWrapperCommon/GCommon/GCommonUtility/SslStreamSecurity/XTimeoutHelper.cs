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

namespace AvePoint.GCommon.Utility.SslStreamSecurity
{
    internal class XTimeoutHelper
    {
        internal static TimeSpan MaxWait = TimeSpan.FromMilliseconds(2147483647.0);

        private DateTime deadline;

        private bool deadlineSet;

        private TimeSpan originalTimeout;

        internal XTimeoutHelper(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw XDiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, XSR.GetString("SFxTimeoutOutOfRange0")));
            }
            if (XTimeoutHelper.IsTooLarge(timeout))
            {
                timeout = XTimeoutHelper.MaxWait;
            }
            this.originalTimeout = timeout;
            this.deadline = DateTime.MaxValue;
            this.deadlineSet = (timeout == TimeSpan.MaxValue);
        }

        internal static TimeSpan Infinite
        {
            get
            {
                return TimeSpan.MaxValue;
            }
        }

        public static bool IsTooLarge(TimeSpan timeout)
        {
            return timeout > XTimeoutHelper.MaxWait && timeout != XTimeoutHelper.Infinite;
        }

        public TimeSpan RemainingTime()
        {
            return this.RemainingTimeExpireZero();
        }

        public TimeSpan RemainingTimeExpireZero()
        {
            if (!this.deadlineSet)
            {
                this.SetDeadline();
                return this.originalTimeout;
            }
            if (this.deadline == DateTime.MaxValue)
            {
                return TimeSpan.MaxValue;
            }
            TimeSpan timeSpan = this.deadline - DateTime.UtcNow;
            if (timeSpan <= TimeSpan.Zero)
            {
                return TimeSpan.Zero;
            }
            return timeSpan;
        }

        internal void SetDeadline()
        {
            this.deadline = DateTime.UtcNow + this.originalTimeout;
            this.deadlineSet = true;
        }


    }
}
