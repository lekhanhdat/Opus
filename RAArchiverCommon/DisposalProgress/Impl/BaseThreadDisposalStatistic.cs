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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAArchiverCommon.DisposalProgress.Impl
{
    public abstract class BaseThreadDisposalStatistic : BaseDisposalStatistic
    {
        protected AveTenantThread _executeThread;

        protected Action _executeThreadAction;

        protected void ActionWithLockAndCatch(Action action, string errorMessage)
        {
            lock (_lockObject)
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    _mLog.Error(errorMessage + $",e:{e}");
                }
            }
        }

        public override void Dispose()
        {

        }

        public override void Init(DisposalStaticInitObject initObject)
        {
            throw new NotImplementedException();
        }

        public override ThreadState GetStatisticState()
        {
            throw new NotImplementedException();
        }

        public override bool AlreadyInit()
        {
            throw new NotImplementedException();
        }

        public override void StartStatistic()
        {
            if(_statisticState != ThreadState.Unstarted)
            {
                _mLog.Error($"Thread statis is {_statisticState}, unable start statistic");
                return;
            }
            if (!_alreadyInit)
            {
                _mLog.Error($"Not init, unable start statistic");
                return;
            }

            ActionWithLockAndCatch(() =>
            {
                if (_executeThread == null)
                {
                    if (_executeThreadAction != null)
                    {
                        _executeThread = new AveTenantThread(new ThreadStart(_executeThreadAction));
                        _executeThread.IsBackground = true;
                        _executeThread.Start();
                    }
                    else
                    {
                        _mLog.Error("already have execute thread running");
                    }
                }
                _statisticState = ThreadState.Running;
            }, "Fail Start Static");
        }

        public override void PrepareEndStatistic()
        {
            throw new NotImplementedException();
        }

        public override void WaitEndStatistic()
        {
            throw new NotImplementedException();
        }
    }
}
