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
using log4net.Appender;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace AvePoint.RA.CommonUtil
{
    public class ErrorAlertAppender : AppenderSkeleton
    {
        private static readonly int TimerInerval = 30 * 1000;
        private static readonly List<string> logs = new List<string>();

        static ErrorAlertAppender()
        {
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (loggingEvent.Level == Level.Error)
            {
                var msg = RenderLoggingEvent(loggingEvent);
                lock (logs)
                {
                    logs.Add(msg);
                }
            }
        }

        private static void UploadDataCallback(object o)
        {
            try
            {
                string text = null;
                lock (logs)
                {
                    if (logs.Count > 0)
                    {
                        var sb = new StringBuilder();
                        logs.ForEach(l => sb.AppendLine(l));
                        text = sb.ToString();
                        logs.Clear();
                    }
                }
                if (text != null)
                {
                    //var client = RAStorageUtil.GetBlobClient(RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.LOG_STORAGE_CONNECTION_STRING]);
                    //var container = client.GetContainerReference(RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.LOG_CONTAINER_NAME]);
                    ////var blobName = $"Errors/{DateTime.UtcNow.ToString("MM_dd_HH")}_{Environment.MachineName}.log";
                    //var blobName = $"Errors/{DateTime.UtcNow.ToString("MM_dd_HH")}_{System.Net.Dns.GetHostName()}.log";
                    //var blob = container.GetAppendBlobReference(blobName);
                    //if (!blob.Exists())
                    //{
                    //    blob.UploadText(text);
                    //}
                    //else
                    //{
                    //    blob.AppendText(text);
                    //}
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Error occurred while uploading data {0}", e);
            }
        }
    }
}
