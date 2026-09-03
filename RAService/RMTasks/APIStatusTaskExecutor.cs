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
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Task;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.RMTasks
{
    public class APIStatusTaskExecutor : ITaskExecutor
    {



        public System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            //string apiUrl = string.Empty;
            //try
            //{
            //    apiUrl = RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.DAO_CONTROL_SERVICE_ADDRESS];
            //    if (apiUrl.Contains("/odata"))
            //    {
            //        apiUrl = apiUrl.Substring(0, apiUrl.IndexOf("/odata") + 1) + healthApi;
            //        var status = SendRequest("GET", apiUrl, "");
            //        logger.Info($"DAO API status is { status }, {apiUrl}.");
            //    }
            //    else
            //    {
            //        logger.Warn($"Invalid DAO API url:{apiUrl}.");
            //    }

            //}
            //catch (Exception ex)
            //{
            //    logger.Error($"dao api status not avaliable, {apiUrl}.ERROR:{ex.ToString()}");
            //}
            return System.Threading.Tasks.Task.CompletedTask;
        }

    }
}
