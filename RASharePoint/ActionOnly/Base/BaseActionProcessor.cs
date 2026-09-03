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
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.Report;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Records.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.ActionOnly.Base
{
    public abstract class BaseActionProcessor
    {
        protected static readonly IAveLogger logger = AveLogger.GetInstance(typeof(BaseActionProcessor));
        //protected IProgressService ProgressService = null;
        //protected IReportService ReportService = null;
        //protected RecordsActionOnlyJobMessage JobMessage;
        protected DateTime RunJobUTCTime;
        protected bool JobHasErrorNode = false;
        protected bool ActionUseMultiThreads = false;
        protected int ItemsPerTask = 200;
        protected readonly object LockObj = new object();
        public List<RuleNodeContract> ExcludeNodes;//TODO
        public List<Rule> AllRecordsRule;//TODO

        protected static IRMReportManager ReportManager
        {
            get
            {
                return ReportMangerFactory.Instance.ReportManager;
            }
        }
        public BaseActionProcessor()
        { }
        public BaseActionProcessor(List<Rule> recordsRule)
        {
            //JobContext.Current.JobMessage = message;
            //JobContext.Current.Init(message);
            //ProgressService = JobContext.Current.ProgressManager.Create();
            //ReportService = JobContext.Current.ReportManager.Create();
            //JobMessage = message;
            AllRecordsRule = recordsRule;
            RunJobUTCTime = DateTime.UtcNow;
            //try
            //{
            //    if (Int32.TryParse(Util.GetAppSettingValue("ActionThreads"), out ItemsPerTask))
            //    {
            //        ActionUseMultiThreads = true;
            //    }
            //}
            //catch (Exception e)
            //{
            //    logger.Info($"Get thread settings error {e.ToString()}");
            //}
        }
        public virtual bool Run()
        {
            return JobHasErrorNode;
        }



    }
}
