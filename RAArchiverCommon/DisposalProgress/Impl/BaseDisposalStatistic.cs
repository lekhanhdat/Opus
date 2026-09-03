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
    public abstract class BaseDisposalStatistic : IDisposalStatistic
    {
        protected static readonly RALogger _mLog = RALogger.GetInstance(typeof(BaseDisposalStatistic));

        protected volatile ThreadState _statisticState = ThreadState.Unstarted;

        protected IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();

        protected string _mainJobId;

        protected string _subJobId;

        protected JobType _jobType;

        protected bool _alreadyInit;

        protected readonly object _lockObject = new object();

        public virtual void Dispose()
        {
            
        }

        public virtual void Init(DisposalStaticInitObject initObject)
        {
            
        }

        public virtual ThreadState GetStatisticState()
        {
            return _statisticState;
        }

        public virtual bool AlreadyInit()
        {
            return _alreadyInit;
        }

        public virtual void StartStatistic()
        {

        }

        public virtual void PrepareEndStatistic()
        {
            
        }

        public virtual void WaitEndStatistic()
        {
            
        }
    }
}
