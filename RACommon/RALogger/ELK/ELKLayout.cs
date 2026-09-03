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
using log4net.Core;
using log4net.Layout;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace AvePoint.RA.CommonUtil.ELK
{
    public class ELKLayout : LayoutSkeleton
    {

        public ELKLayout()
        {
            IgnoresException = false;
        }

        public override void ActivateOptions()
        {
        }

        public override void Format(TextWriter writer, LoggingEvent loggingEvent)
        {
            var prop = loggingEvent.GetProperties();
            var obj = new
            {
                Time = loggingEvent.TimeStamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Level = loggingEvent.Level.Name,
                Thread = Thread.CurrentThread.ManagedThreadId,
                Logger = loggingEvent.LoggerName,
                Message = loggingEvent.RenderedMessage,
                TenantId = prop["TenantGroup"],
                //LogonUser = prop["TenantUser"],
                TraceId = prop["TraceId"],
            };
            writer.WriteLine(JsonConvert.SerializeObject(obj, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
        }

    }
}
