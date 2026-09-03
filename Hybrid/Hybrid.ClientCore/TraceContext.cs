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
namespace AvePoint.Hybrid.ClientCore
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;

    public class TraceContext
    {
        //remove System.Diagnostics.DiagnosticSource and add back later.
        //private Activity seflActivity;

        public string TraceId { get; private set; }
        public string ActivityId { get; set; }
        public string RequestId { get; }
        public string InterfaceName { get; }
        public string MethodName { get; }
        public bool IsRetry { get; set; }

        internal Stopwatch Stopwatch { get; }


        public TraceContext(MethodInfo method)
            : this(method.DeclaringType.FullName, method.Name)
        {

        }

        public TraceContext(Type type, [CallerMemberName] string member = "")
           : this(type.FullName, member)
        {

        }

        public TraceContext(string interfaceName, string methodName)
        {

            InterfaceName = interfaceName;
            MethodName = methodName;
            RequestId = Guid.NewGuid().ToString();
            TraceId = Guid.NewGuid().ToString();

            //var activity = Activity.Current;
            //if (activity == null)
            //{
            //    seflActivity = new Activity($"{interfaceName}.{methodName}");
            //    seflActivity.Start();
            //    activity = seflActivity;
            //}
            //TraceId = activity.TraceId.ToString();
            //ActivityId = activity.Id;

            Stopwatch = Stopwatch.StartNew();

        }

        ~TraceContext()
        {
            //seflActivity?.Dispose();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{InterfaceName}.{MethodName}\nRequestId: {RequestId}");
            if (!string.IsNullOrEmpty(TraceId))
            {
                sb.AppendLine($"TraceId:{TraceId}");
            }
            if (!string.IsNullOrEmpty(ActivityId))
            {
                sb.AppendLine($"ActivityId:{ActivityId}");
            }
            if (IsRetry)
            {
                sb.AppendLine("RetryInvoke");
            }
            return sb.ToString();
        }

        public string ToFinalString()
        {
            return $"{ToString()}\nDuration: {Stopwatch.ElapsedMilliseconds}ms";
        }

        public KeyValuePair<string, string>[] GetLoggingScope()
        {
            return new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>(CloudSdkLoggingFields.Interface,InterfaceName),
                new KeyValuePair<string, string>(CloudSdkLoggingFields.Method,MethodName),
                new KeyValuePair<string, string>(CloudSdkLoggingFields.Duration,$"{Stopwatch.ElapsedMilliseconds}"),
                new KeyValuePair<string, string>(CloudSdkLoggingFields.TraceId,TraceId),
                new KeyValuePair<string, string>(CloudSdkLoggingFields.ActivityId,ActivityId),
            };
        }
    }

}
