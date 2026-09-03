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

namespace AvePoint.Wrapper.Common.MultiThread
{
    /// <summary>
    /// task
    /// </summary>
    public abstract class BaseTask : IDisposable
    {
        /// <summary>
        /// ????????????
        /// </summary>
        private bool runInfinite = false;
        /// <summary>
        /// sleep time
        /// </summary>
        private int sleepTime = -1;

        /// <summary>
        /// ????????
        /// </summary>
        public bool RunInfinite
        {
            get
            {
                return runInfinite;
            }
            set
            {
                runInfinite = value;
            }
        }
        /// <summary>
        /// sleep time
        /// </summary>
        public int SleepTime
        {
            get
            {
                return sleepTime;
            }
            set
            {
                sleepTime = value;
            }
        }
        /// <summary>
        /// ????????
        /// </summary>
        public virtual void Process()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// task????????????????action
        /// </summary>
        public virtual void CompleteTask()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// task????????????????????action
        /// </summary>
        /// <param name="ex"></param>
        public virtual void CompleteTask(Exception ex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// ??????????????task
        /// </summary>
        /// <param name="ex"></param>
        public virtual void Stop(Exception ex)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Close();
        }

        protected virtual void Close()
        {
        }
    }
}