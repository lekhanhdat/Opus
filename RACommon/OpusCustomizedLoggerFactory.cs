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
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Util;

namespace AvePoint.RA.Common
{
    public class OpusCustomizedLoggerFactory : ICustomizedLoggerFactory
    {
        public ILogger CreateInstance()
        {
            return new OpusUtilLogger();
        }

        public ILogger CreateInstance(Type type)
        {
            return new OpusUtilLogger();
        }

        public ILogger CreateInstance(string loggerName)
        {
            return new OpusUtilLogger();
        }
    }
    public class OpusUtilLogger : ILogger
    {
        private static RALogger mLog = RALogger.GetInstance(typeof(OpusCustomizedLoggerFactory));
        private static readonly OpusUtilLoggerScopeFactory scopeFactory;
        public LoggerScope CreateScope<TState>(TState state)
        {
            return scopeFactory.BeginScope(state);
        }

        public void Debug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            mLog.Debug($"Message : {message}, MemberName : {memberName}, SourceFilePath : {sourceFilePath}, SourceLineNumber : {sourceLineNumber}");
        }

        public void Error(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            mLog.Error($"Message : {message}, MemberName : {memberName}, SourceFilePath : {sourceFilePath}, SourceLineNumber : {sourceLineNumber}");
        }

        public void Info(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            mLog.Info($"Message : {message}, MemberName : {memberName}, SourceFilePath : {sourceFilePath}, SourceLineNumber : {sourceLineNumber}");
        }

        public void Warn(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            mLog.Warn($"Message : {message}, MemberName : {memberName}, SourceFilePath : {sourceFilePath}, SourceLineNumber : {sourceLineNumber}");
        }

        public void Debug(string message, Dictionary<string, object> fields)
        {

        }

        public void Error(string message, Dictionary<string, object> fields)
        {

        }

        public void Info(string message, Dictionary<string, object> fields)
        {

        }

        public void Warn(string message, Dictionary<string, object> fields)
        {

        }
    }
    internal class OpusUtilLoggerScopeFactory
    {
        private readonly ScopeRegistry registry = new ScopeRegistry();

        public LoggerScope BeginScope<TScope>(TScope scope)
            => new LoggerScope(scope, registry);
    }
}
