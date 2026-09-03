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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.SharePoint.Common;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.SharePoint.EnforceRuleAction
{
    public abstract class BaseActionProcessor
    {
        protected static readonly IAveLogger logger = AveLogger.GetInstance(typeof(BaseActionProcessor));
        protected DateTime RunJobUTCTime;
        protected bool JobHasErrorNode = false;
        protected bool ActionUseMultiThreads = false;
        protected int ThreadCount = 200;
        protected object LockObj = new object();
        public List<RuleNodeContract> ExcludeNodes;//TODO
        protected IProgressService ProgressService { get; set; }
        protected IReportService<JMJobDetails> JobDetailService { get; set; }

        public BaseActionProcessor()
        {
            RunJobUTCTime = DateTime.UtcNow;
            ProgressService = JobContext.Current.mProgressManager.Create();
            JobDetailService = JobContext.Current.JobDetailManager.Create();
        }
        //public BaseActionProcessor(List<Rule> recordsRule)
        //{
        //    AllRecordsRule = recordsRule;
        //    RunJobUTCTime = DateTime.UtcNow;
        //    try
        //    {
        //        if (Int32.TryParse(Util.GetAppSettingValue("ActionThreads"), out ThreadCount))
        //        {
        //            ActionUseMultiThreads = true;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Info($"Get thread settings error {e.ToString()}");
        //    }
        //}
        public virtual bool Run()
        {
            return JobHasErrorNode;
        }



    }
}
