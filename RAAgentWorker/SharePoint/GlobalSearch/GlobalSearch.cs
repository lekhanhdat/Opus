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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.FileSystem;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.SharePoint.GlobalSearch.Action;
using RAFileSystem.SharePoint.GlobalSearch.Discover;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.GCommon;

namespace AvePoint.RA.SharePoint.GlobalSearch
{
    public class GlobalSearch : IScheduleJobWorker
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string mJobId;
        private AvePoint.RA.Contract.Global.Explorer.GlobalSearchAction mAction;
        private object mActionExtension;
        private bool mHasError = false;
        public void Bind(string msg)
        {
            var jobMessage = SerializerHelper.DeserializeByDataContractSerializer<AvePoint.RA.Contract.Global.JobMessage.GlobalSearchActionJobMessage>(msg);
            mJobId = jobMessage.JobId;
            mAction = jobMessage.Action;
            mActionExtension = jobMessage.ActionExtension;
        }

        public void Run()
        {
            IGlobalSearchAction action = null;
            try
            {
                action = GetGlobalSearchAction(mAction);
                GlobalSearchDiscover discover = new GlobalSearchDiscover(mJobId, mAction);
                discover.Run();
                ProcessDataInBatch(discover, action, mActionExtension);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while running global search action job. Error:{0}", e.ToString());
                mHasError = true;
            }

            try
            {
                JobContext.Current.Cleanup();
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while cleaning up. Error:" + e.ToString());
            }

            int failedCount = action.GetFailedCount();
            int successCount = action.GetSuccessCount();
            if (failedCount > 0 && successCount > 0)
            {
                HybridApiClient.Instance.UpdateJobState(JobContext.Current.JobId, (int)JobStatus.FinishWithException, "");
            }
            else if ((failedCount > 0 && successCount == 0) || mHasError)
            {
                HybridApiClient.Instance.UpdateJobState(JobContext.Current.JobId, (int)JobStatus.Failed, "");
            }
            else
            {
                HybridApiClient.Instance.UpdateJobState(JobContext.Current.JobId, (int)JobStatus.Finished, "");
            }
        }

        private void ProcessDataInBatch(GlobalSearchDiscover discover, IGlobalSearchAction action, object actionExtenstion)
        {
            while (true)
            {
                if (GlobalSearchCache.Instance.DiscoverCache.Count >= 100)
                {
                    var data = GlobalSearchCache.Instance.DiscoverCache.Take(100).ToList();
                    logger.Info($"Start to process {data.Count} items.");
                    action.DoAction(data, actionExtenstion, mJobId);
                }
                else
                {
                    if (discover.DiscoverFinish)
                    {
                        var data = GlobalSearchCache.Instance.DiscoverCache.TakeAll().ToList();
                        if (data.Count > 0)
                        {

                            logger.Info($"Start to process {data.Count} items.");
                            action.DoAction(data, actionExtenstion, mJobId);
                        }
                        break;
                    }
                    else
                    {
                        Thread.Sleep(5000);
                    }
                }
            }
        }


        private IGlobalSearchAction GetGlobalSearchAction(AvePoint.RA.Contract.Global.Explorer.GlobalSearchAction action)
        {
            switch (action)
            {
                case AvePoint.RA.Contract.Global.Explorer.GlobalSearchAction.DeclareRecords:
                case AvePoint.RA.Contract.Global.Explorer.GlobalSearchAction.UnDeclareRecords:
                    return new DeclareAction(action);
                //case AvePoint.RA.Contract.Global.Explorer.GlobalSearchAction.MoveTo:
                //    return new MoveAction();
                case AvePoint.RA.Contract.Global.Explorer.GlobalSearchAction.Reclassify:
                    return new ReclassifyAction();
                //case AvePoint.RA.Contract.Global.Explorer.GlobalSearchAction.AccessControl:
                //    return new AccessControlAction();
                default:
                    return null;
            }
        }
    }
}
