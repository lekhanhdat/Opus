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

namespace AutoInstallationCommon.Utility
{
    #region using directives

    #endregion

    /// <summary>
    ///     日志类。该类可以将log写入文件或windows eventlog。
    /// </summary>
    public class AveLogger : IAveLogger
    {
        private static string loggingPrefix;
        private static string loggingPostfix;
        private readonly IAveLoggerImp loggerImp;

        public AveLogger(IAveLoggerImp imp)
        {
            loggerImp = imp;
        }

        public AveLogger(Type type)
        {
            loggerImp = new AveLoggerImp(type);
        }

        /// <summary>
        ///     根据Type获取logger实例。
        ///     <example>
        ///         AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        ///     </example>
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>logger实例</returns>
        public static AveLogger GetInstance(Type type)
        {
            return new AveLogger(type);
        }

        /// <summary>
        ///     设置log的前缀， eg:[agent version=5.6.0.0]	[CIID=20101026114717058#-1#Demo#33]
        /// </summary>
        /// <param name="logPrefix">log prefix to set</param>
        public static void SetCustomizedLogPrefix(string logPrefix)
        {
            loggingPrefix = logPrefix;
        }

        /// <summary>
        ///     设置log的后缀， eg: 5.6.0.0 实际上一个消息作为前缀还是后缀没有什么特别的，一回事，完全看怎么使用。
        /// </summary>
        /// <param name="logPostfix">log postfix to set</param>
        public static void SetCustomizedLogPostfix(string logPostfix)
        {
            loggingPostfix = logPostfix;
        }

        private string GetFinalMessage(string formatStr, params object[] args)
        {
            var finalMsg = string.Empty;
            if (args.Length == 0)
                finalMsg = formatStr; //兼容原来的 (string msg) 函数
            else if (args.Length == 1 && formatStr.IndexOf("{0}", StringComparison.OrdinalIgnoreCase) == -1)
                finalMsg = string.Format("{0}\t{1}", formatStr, args[0]); //兼容原来的 (string msg，Exception e) 函数
            else
                finalMsg = string.Format(formatStr, args); //兼容原来的 (string formatStr, params object[] args) 函数

            if (!string.IsNullOrEmpty(loggingPrefix)) finalMsg = loggingPrefix + "    " + finalMsg;
            if (!string.IsNullOrEmpty(loggingPostfix)) finalMsg = finalMsg + "    " + loggingPostfix;
            return finalMsg;
        }

        private void WriteEntry(string msg, AveLogLevel level, int eventId, ushort taskCategory, string eventSource)
        {
            loggerImp.WriteEntry(msg, level, eventId, taskCategory, eventSource);
        }

        #region --public properties--

        public AveLogLevel CurrentLogLevel => loggerImp.CurrentLogLevel;
        public bool IsErrorEnabled => loggerImp.IsErrorEnabled;
        public bool IsWarnEnabled => loggerImp.IsWarnEnabled;
        public bool IsInfoEnabled => loggerImp.IsInfoEnabled;
        public bool IsDebugEnabled => loggerImp.IsDebugEnabled;

        #endregion

        #region --public log method--

        #region --Debug methods--

        /// <summary>
        ///     写debug level的日志，要注意formatStr和args的匹配
        /// </summary>
        /// <param name="formatStr">用来格式化后面参数的字符串</param>
        /// <param name="args">可变个数的参数</param>
        public void Debug(string formatStr, params object[] args)
        {
            Debug(0, formatStr, args);
        }

        /// <summary>
        ///     写debug level的日志，要注意formatStr和args的匹配
        /// </summary>
        /// <param name="formatStr">用来格式化后面参数的字符串</param>
        /// <param name="eventId"> event id</param>
        /// <param name="args">可变个数的参数</param>
        public void Debug(int eventId, string formatStr, params object[] args)
        {
            Debug(0, eventId, formatStr, args);
        }

        public void Debug(ushort taskCategory, int eventId, string formatStr, params object[] args)
        {
            Debug(string.Empty, taskCategory, eventId, formatStr, args);
        }

        public void Debug(string eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args)
        {
            try
            {
                if (!IsDebugEnabled) return;

                var finalMsg = GetFinalMessage(formatStr, args);
                WriteEntry(finalMsg, AveLogLevel.DEBUG, eventId, taskCategory, eventSource);
            }
            catch
            {
            }
        }

        #endregion

        #region --Info methods--

        /// <summary>
        ///     写info level的日志，要注意formatStr和args的匹配
        /// </summary>
        /// <param name="formatStr">用来格式化后面参数的字符串</param>
        /// <param name="args">可变个数的参数</param>
        public void Info(string formatStr, params object[] args)
        {
            Info(0, formatStr, args);
        }

        /// <summary>
        ///     写info level的日志，要注意formatStr和args的匹配
        /// </summary>
        /// <param name="formatStr">用来格式化后面参数的字符串</param>
        /// <param name="eventId"> event id</param>
        /// <param name="args">可变个数的参数</param>
        public void Info(int eventId, string formatStr, params object[] args)
        {
            Info(0, eventId, formatStr, args);
        }

        public void Info(ushort taskCatefory, int eventId, string formatStr, params object[] args)
        {
            Info(string.Empty, taskCatefory, eventId, formatStr, args);
        }

        public void Info(string eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args)
        {
            try
            {
                if (!IsInfoEnabled) return;

                var finalMsg = GetFinalMessage(formatStr, args);
                WriteEntry(finalMsg, AveLogLevel.INFO, eventId, taskCategory, eventSource);
            }
            catch
            {
            }
        }

        #endregion

        #region --Warn methods--

        /// <summary>
        ///     写warn level的日志，要注意formatStr和args的匹配
        /// </summary>
        /// <param name="formatStr">用来格式化后面参数的字符串</param>
        /// <param name="args">可变个数的参数</param>
        public void Warn(string formatStr, params object[] args)
        {
            Warn(0, formatStr, args);
        }

        /// <summary>
        ///     写warn level的日志，要注意formatStr和args的匹配
        /// </summary>
        /// <param name="formatStr">用来格式化后面参数的字符串</param>
        /// <param name="eventId"> event id</param>
        /// <param name="args">可变个数的参数</param>
        public void Warn(int eventId, string formatStr, params object[] args)
        {
            Warn(0, eventId, formatStr, args);
        }

        public void Warn(ushort taskCatefory, int eventId, string formatStr, params object[] args)
        {
            Warn(string.Empty, taskCatefory, eventId, formatStr, args);
        }

        public void Warn(string eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args)
        {
            try
            {
                if (!IsWarnEnabled) return;

                var finalMsg = GetFinalMessage(formatStr, args);
                WriteEntry(finalMsg, AveLogLevel.WARN, eventId, taskCategory, eventSource);
            }
            catch
            {
            }
        }

        #endregion

        #region --Error methods--

        /// <summary>
        ///     写error level的日志，要注意formatStr和args的匹配
        /// </summary>
        /// <param name="formatStr">用来格式化后面参数的字符串</param>
        /// <param name="args">可变个数的参数</param>
        public void Error(string formatStr, params object[] args)
        {
            Error(0, formatStr, args);
        }

        /// <summary>
        ///     写error level的日志，要注意formatStr和args的匹配
        /// </summary>
        /// <param name="formatStr">用来格式化后面参数的字符串</param>
        /// <param name="eventId"> event id</param>
        /// <param name="args">可变个数的参数</param>
        public void Error(int eventId, string formatStr, params object[] args)
        {
            Error(0, eventId, formatStr, args);
        }

        public void Error(ushort taskCategory, int eventId, string formatStr, params object[] args)
        {
            Error(string.Empty, taskCategory, eventId, formatStr, args);
        }

        public void Error(string eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args)
        {
            try
            {
                if (!IsErrorEnabled) return;

                var finalMsg = GetFinalMessage(formatStr, args);
                WriteEntry(finalMsg, AveLogLevel.ERROR, eventId, taskCategory, eventSource);
            }
            catch
            {
            }
        }

        #endregion

        #region --Log methods--

        public void Log(AveLogLevel aveLogLevel, string formatStr, params object[] args)
        {
            Log(aveLogLevel, 0, formatStr, args);
        }

        public void Log(AveLogLevel aveLogLevel, int eventId, string formatStr, params object[] args)
        {
            Log(aveLogLevel, 0, eventId, formatStr, args);
        }

        public void Log(AveLogLevel aveLogLevel, ushort taskCategory, int eventId, string formatStr,
            params object[] args)
        {
            Log(aveLogLevel, string.Empty, taskCategory, eventId, formatStr, args);
        }

        public void Log(AveLogLevel aveLogLevel, string eventSource, ushort taskCategory, int eventId, string formatStr,
            params object[] args)
        {
            try
            {
                if (CurrentLogLevel > aveLogLevel) return;

                var finalMsg = GetFinalMessage(formatStr, args);
                WriteEntry(finalMsg, aveLogLevel, eventId, taskCategory, eventSource);
            }
            catch
            {
            }
        }

        #endregion

        #endregion
    }
}