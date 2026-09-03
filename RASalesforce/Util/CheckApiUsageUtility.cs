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
using RASalesforce.APIs;

namespace RASalesforce.Util;

public class CheckApiUsageUtility
    {
        private static RALogger _logger = RALogger.GetInstance(typeof(CheckApiUsageUtility));

        public static void Start(SalesforceService salesforceService, string jobId)
        {
            _logger.Info("Start Checking Salesforce API Usage ,ID:{0}", jobId);
            AveTenantThread checkThread = new(CheckReachedToLimitExceed)
            {
                IsBackground = true
            };
            checkThread.Start(salesforceService);
        }

        private static void CheckReachedToLimitExceed(object salesforceService)
        {
            int intervalTime = 1000 * 60 * 2;//2 minutes
            var service = (SalesforceService) salesforceService;
            while (true)
            {
                service.RefreshAsync().GetAwaiter().GetResult();
                if (SalesforceAPIHelper.Instance.IsNeedPostPond)
                {
                    _logger.Warn("Salesforce API usage is over 80%, Pausing the job.");
                    if (!SalesforceAPIHelper.Instance.IsPaused)
                    {
                        SalesforceAPIHelper.Instance.Pause();
                        Thread.Sleep(1000 * 60 * 60 * 24); // 24 hours
                        SalesforceAPIHelper.Instance.Resume();
                    }
                }
                else
                {
                    _logger.Info($"Salesforce API usage is {SalesforceAPIHelper.Instance.ApiUsed + SalesforceAPIHelper.Instance.RequestCount} / {SalesforceAPIHelper.Instance.MaxRequest}, {SalesforceAPIHelper.Instance.RequestCount} used by this job.");
                    Thread.Sleep(intervalTime);
                }
            }
        }

    }