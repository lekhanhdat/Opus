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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Common;
using System.Reflection;
//using Microsoft.SharePoint;
using LOGRESOURCE = Merged18NResources.Archive.Archive;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.Wrapper.Common;
//using AvePoint.Adonis.StorageOptimization.Common.Object;
//using AvePoint.Wrapper.Contract;
using AvePoint.Wrapper.Discovery;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Discover;
using AvePoint.RA.Contract;
using AvePoint.RA.SharePoint.Archiver.Scan.Implement;

namespace AvePoint.RA.SharePoint.Archiver
{
    [AvePoint.GCommon.Contract.CodeReview.AveCodeReview(
   "2012/6/5",
   "ruiheng.liu@AvePoint.com",
   "yanlong.gu@AvePoint.com",
   new string[]
   {
        CodeReviewConstants.CHECK_LIST_ID_SOCKET_1,
        CodeReviewConstants.CHECK_LIST_ID_SECURITY_1,
        CodeReviewConstants.CHECK_LIST_ID_SECURITY_2,
        CodeReviewConstants.CHECK_LIST_ID_EH_1,
        CodeReviewConstants.CHECK_LIST_ID_EH_2,
        CodeReviewConstants.CHECK_LIST_ID_DB_1,
        CodeReviewConstants.CHECK_LIST_ID_FA_1,
        CodeReviewConstants.CHECK_LIST_ID_FA_10,
        CodeReviewConstants.CHECK_LIST_ID_STREAM_1,
        CodeReviewConstants.CHECK_LIST_ID_HC_1,
        CodeReviewConstants.CHECK_LIST_ID_HC_2,
        CodeReviewConstants.CHECK_LIST_ID_THREAD_1,
        CodeReviewConstants.CHECK_LIST_ID_THREAD_2,
   },
   "ADO-33396",
   true
   )]
    [AvePoint.GCommon.Contract.CodeReview.AveCodeReview(
      "2012/8/7",
      "ruiheng.liu@AvePoint.com",
      "yanlong.gu@AvePoint.com",
      new string[]
       {
            CodeReviewConstants.CHECK_LIST_ID_SOCKET_1,
            CodeReviewConstants.CHECK_LIST_ID_SECURITY_1,
            CodeReviewConstants.CHECK_LIST_ID_SECURITY_2,
            CodeReviewConstants.CHECK_LIST_ID_EH_1,
            CodeReviewConstants.CHECK_LIST_ID_EH_2,
            CodeReviewConstants.CHECK_LIST_ID_DB_1,
            CodeReviewConstants.CHECK_LIST_ID_FA_1,
            CodeReviewConstants.CHECK_LIST_ID_FA_10,
            CodeReviewConstants.CHECK_LIST_ID_STREAM_1,
            CodeReviewConstants.CHECK_LIST_ID_HC_1,
            CodeReviewConstants.CHECK_LIST_ID_HC_2,
            CodeReviewConstants.CHECK_LIST_ID_THREAD_1,
            CodeReviewConstants.CHECK_LIST_ID_THREAD_2,
            CodeReviewConstants.CHECK_LIST_ID_LOG_1,
            CodeReviewConstants.CHECK_LIST_ID_LOG_2,
            CodeReviewConstants.CHECK_LIST_ID_LOG_3,
            CodeReviewConstants.CHECK_LIST_ID_LOG_4,
       },
      "ADO-44684",
      true
      )]
    [AvePoint.GCommon.Contract.CodeReview.AveCodeReview(
    "2012/11/2",
    "yanlong.gu@AvePoint.com",
    "dongliang.liu@AvePoint.com",
    new string[]
           {
                CodeReviewConstants.CHECK_LIST_ID_FA_1,
                CodeReviewConstants.CHECK_LIST_ID_FA_10,
                CodeReviewConstants.CHECK_LIST_ID_LOG_1,
                CodeReviewConstants.CHECK_LIST_ID_LOG_2,
                CodeReviewConstants.CHECK_LIST_ID_LOG_3,
                CodeReviewConstants.CHECK_LIST_ID_LOG_4,
           },
    "ADO-53910",
    true
    )]
    [AvePoint.GCommon.Contract.CodeReview.AveCodeReview(
      "2013/5/10",
      "yanlong.gu@AvePoint.com",
      "dongliang.liu@AvePoint.com",
      new string[]
       {
            CodeReviewConstants.CHECK_LIST_ID_EH_1,
            CodeReviewConstants.CHECK_LIST_ID_EH_2,
            CodeReviewConstants.CHECK_LIST_ID_FA_1,
            CodeReviewConstants.CHECK_LIST_ID_FA_10,
            CodeReviewConstants.CHECK_LIST_ID_LOG_1,
            CodeReviewConstants.CHECK_LIST_ID_LOG_2,
            CodeReviewConstants.CHECK_LIST_ID_LOG_3,
            CodeReviewConstants.CHECK_LIST_ID_LOG_4,
       },
      "ADO-72680",
      false
      )]
    [AvePoint.GCommon.Contract.CodeReview.AveCodeReview(
    "2013/10/11",
    "dong.xie@AvePoint.com",
    "dongliang.liu@AvePoint.com",
    new string[]
           {
                CodeReviewConstants.CHECK_LIST_ID_FA_1,
                CodeReviewConstants.CHECK_LIST_ID_FA_10,
                CodeReviewConstants.CHECK_LIST_ID_LOG_1,
                CodeReviewConstants.CHECK_LIST_ID_LOG_2,
                CodeReviewConstants.CHECK_LIST_ID_LOG_3,
                CodeReviewConstants.CHECK_LIST_ID_LOG_4,
           },
    "ADO-92003",
    true
    )]
    class ScanDiscovrerNodeWorker : DiscoverNodeWorkerBase
    {
        #region Private fields
       // private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Properties

        #endregion

        #region Public Methods
        public ScanDiscovrerNodeWorker(ScanJobSettings jobSettings, ScheduleConfiguration paraConfig, IBackwardDependencyNodeCache<object> dependencyObjs, bool justEstimateListCount)
            :base(jobSettings, paraConfig, dependencyObjs)
        {
        }

        #endregion
    }
}
