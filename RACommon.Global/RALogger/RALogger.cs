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




namespace AvePoint.RA.CommonUtil
{
    #region using directives
    using AvePoint.RA.Contract.Services;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using log4net;
    using log4net.Repository.Hierarchy;
    using AvePoint.RA.Common.Cache;
    using System.Runtime.InteropServices.ComTypes;

    //using namespace AvePoint.AOSReporting.CommonUtility.Cryptography;
    #endregion

    /// <summary>
    /// 日志类,在AveLogger基础上修改以支持log上传到cloud.
    /// </summary>
    public class RALogger : IRALogger
    {
        public static string ConfigFile { get; set; }
        public static bool DisableLogCacheMode { get; set; } = true;

        #region --Static fields--
        private static string jobId;
        private static LogType logType = LogType.ServiceLog;
        private static string jobTenantId;
        private static string loggingPrefix;
        private static string loggingPostfix;
        private static IRALogUploader logUploader;
        #endregion

        #region --Fields--
        /// <summary>
        /// Global级别的，如果Global给disable了，那对象内部的也无效。
        /// </summary>
        /// <summary>
        /// Current对象级别的。
        /// </summary>
        private bool checkSensitiveKeywordInContext = true;
        protected IAveLoggerImp loggerImp;
        protected bool log2File = true;
        #endregion

        #region --Constructors--
        public RALogger(IAveLoggerImp imp, bool checkSensitiveKeyword)
        {
            this.loggerImp = imp;
            this.checkSensitiveKeywordInContext = checkSensitiveKeyword;
        }
       
        public RALogger(Type type, bool checkSensitiveKeyword)
            : this(new AveLoggerImp(type), checkSensitiveKeyword)
        {
        }

        public RALogger(IAveLoggerImp imp)
            : this(imp, true)
        {
        }

        public RALogger(Type type)
            : this(type, true)
        {
        }
        #endregion

        #region --public static properties--
        public static string JobId
        {
            get { return jobId;}
        }
        public static string JobTenantId
        {
            get { return jobTenantId; }
        }
        public static LogType LogType
        {
            get { return logType; }
        }
        
        public static RollingMode RollingStyle
        {
            get
            {
                Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
                Logger rootLogger = hierarchy.Root;
                AveSeparativeLogAppender appender = rootLogger.GetAppender("AveSeparativeLogAppender") as AveSeparativeLogAppender;

                if (appender == null)
                {
                    return default(RollingMode);
                }
                return appender.RollingStyle;
            }
            set
            {
                Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
                Logger rootLogger = hierarchy.Root;
                AveSeparativeLogAppender appender = rootLogger.GetAppender("AveSeparativeLogAppender") as AveSeparativeLogAppender;

                if (appender == null)
                {
                    return;
                }
                appender.RollingStyle = value;
                appender.ActivateOptions();
            }
        }

        /// <summary>
        /// 根据Type获取logger实例。
        /// <example>
        /// AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        /// </example>
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>logger实例</returns>
        public static RALogger GetInstance(Type type)
        {
            return new RALogger(type);
        }
        #endregion

        #region --public static method--
        
        public static void SeparateLogToTenant(string tenantId, string jobIdArgs)
        {
            jobId = jobIdArgs;
            jobTenantId = tenantId;
            if (string.IsNullOrEmpty(jobIdArgs) || string.IsNullOrEmpty(tenantId))
            {
                return;
            }
            if (!string.IsNullOrEmpty(jobIdArgs))
            {
                logType = LogType.JobLog;
                CallContext.SetData("LogType", LogType.JobLog);
            }

            CallContext.SetData("ThreadJobId", jobId);

            CallContext.SetData("TenantIdentity", tenantId);

           
        }

        public static void WaitForAllLogsFlush()
        {
            AveLoggerImp.WaitForAllLogsFlush();
        }

        public static void SetCustomizedLogPrefix(string logPrefix)
        {
            loggingPrefix = logPrefix;
        }
        public static void SetCustomizedLogPostfix(string logPostfix)
        {
            loggingPostfix = logPostfix;
        }

        #region --Log Uploader--
        public static void SetUploader(IRALogUploader uploader)
        {
            logUploader = uploader;
        }

        public static void FinallyUpload(string tenantAccountName, string jobId)
        {
            logUploader?.FinallyUpload(tenantAccountName, jobId);
        }

        public static void UploadCurrentLog()
        {
            logUploader?.UploadCurrentLog();
        }

        public static void UploadLog(string fileName)
        {
            logUploader?.UploadLog(fileName);
        }
        #endregion
        #endregion

        #region --public properties--
        public AveLogLevel CurrentLogLevel { get { return loggerImp.CurrentLogLevel; } }
        public bool IsErrorEnabled { get { return loggerImp.IsErrorEnabled; } }
        public bool IsWarnEnabled { get { return loggerImp.IsWarnEnabled; } }
        public bool IsInfoEnabled { get { return loggerImp.IsInfoEnabled; } }
        public bool IsDebugEnabled { get { return loggerImp.IsDebugEnabled; } }

        #endregion

        #region --public log method--

        #region --Debug methods--

        /// <summary>
        /// 写debug level的日志，要注意formatStr和args的匹配
        /// </summary>
        /// <param name="formatStr">用来格式化后面参数的字符串</param>
        /// <param name="args">可变个数的参数</param>
        public void Debug(string formatStr, params object[] args)
        {
            try
            {
                if (!IsDebugEnabled) return;

                WriteEntry(GetFinalMessage(formatStr, args), AveLogLevel.DEBUG, 0, 0);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }
        }

        //        [Obsolete]
        //        public void Debug(int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Debug(ushort taskCategory, int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Debug(EventSources eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }

        #endregion

        #region --Info methods--

        /// <summary>
        /// 写info level的日志，要注意formatStr和args的匹配
        /// </summary>
        /// <param name="formatStr">用来格式化后面参数的字符串</param>
        /// <param name="args">可变个数的参数</param>
        public void Info(string formatStr, params object[] args)
        {
            try
            {
                if (!IsInfoEnabled) return;

                WriteEntry(GetFinalMessage(formatStr, args), AveLogLevel.INFO, 0, 0);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }
        }

        //        [Obsolete]
        //        public void Info(int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Info(ushort taskCategory, int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Info(EventSources eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Info(EventSources eventSource, ushort taskCategory, AveEventMessage eventMessage)
        //        {
        //
        //        }

        #endregion

        #region --Warn methods--

        /// <summary>
        /// 写warn level的日志，要注意formatStr和args的匹配
        /// </summary>
        /// <param name="formatStr">用来格式化后面参数的字符串</param>
        /// <param name="args">可变个数的参数</param>
        public void Warn(string formatStr, params object[] args)
        {
            try
            {
                if (!IsWarnEnabled) return;

                WriteEntry(GetFinalMessage(formatStr, args), AveLogLevel.WARN, 0, 0);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }
        }

        //        [Obsolete]
        //        public void Warn(int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Warn(ushort taskCategory, int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Warn(EventSources eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }

        #endregion

        #region --Error methods--

        /// <summary>
        /// 写error level的日志，要注意formatStr和args的匹配
        /// </summary>
        /// <param name="formatStr">用来格式化后面参数的字符串</param>
        /// <param name="args">可变个数的参数</param>
        public void Error(string formatStr, params object[] args)
        {
            try
            {
                if (!IsErrorEnabled) return;

                WriteEntry(GetFinalMessage(formatStr, args), AveLogLevel.ERROR, 0, 0);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }
        }

        //        [Obsolete]
        //        public void Error(int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Error(ushort taskCategory, int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Error(EventSources eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Error(EventSources eventSource, ushort taskCategory, int eventId, int errorCode, string formatStr, params object[] args)
        //        {
        //
        //        }

        #endregion

        #region --Log methods--

        public void Log(AveLogLevel aveLogLevel, string formatStr, params object[] args)
        {
            try
            {
                if (CurrentLogLevel > aveLogLevel) return;

                WriteEntry(GetFinalMessage(formatStr, args), aveLogLevel, 0, 0);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.ToString());
            }
        }

        //        [Obsolete]
        //        public void Log(AveLogLevel aveLogLevel, int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Log(AveLogLevel aveLogLevel, ushort taskCategory, int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }
        //
        //        [Obsolete]
        //        public void Log(AveLogLevel aveLogLevel, EventSources eventSource, ushort taskCategory, int eventId, string formatStr, params object[] args)
        //        {
        //
        //        }

        //[Obsolete]
        //public void Log(EventSources eventSource, ushort taskCategory, AveEventMessage eventMessage, AveErrorCodeException exception)
        //{
        //}

        #endregion
        #endregion

        protected string GetFinalMessage(string formatStr, params object[] args)
        {
            string finalMsg = string.Empty;
            if (args.Length == 0)
            {
                finalMsg = formatStr; //兼容原来的 (string msg) 函数
            }
            else if (args.Length == 1 && formatStr.IndexOf("{0}", StringComparison.OrdinalIgnoreCase) == -1)
            {
                finalMsg = string.Format("{0}\t{1}", formatStr, args[0]);//兼容原来的 (string msg，Exception e) 函数
            }
            else
            {
                finalMsg = string.Format(formatStr, args);//兼容原来的 (string formatStr, params object[] args) 函数
            }

            if (!string.IsNullOrEmpty(loggingPrefix))
            {
                finalMsg = loggingPrefix + "    " + finalMsg;
            }
            if (!string.IsNullOrEmpty(loggingPostfix))
            {
                finalMsg = finalMsg + "    " + loggingPostfix;
            }
            if (log2File)//GA+ online will skip this becasue he will call trace log later
            {
                Trace.WriteLine(finalMsg);
            }
            return finalMsg;
        }

        private void WriteEntry(string msg, AveLogLevel level, int eventId, ushort taskCategory, Exception e = null)
        {
            loggerImp.WriteEntry(msg, level, eventId, taskCategory, "", e);
        }

        //DO NOT DELETE THIS CODE UNLESS WE NO LONGER REGUIRE ASSEMBLY 
        //private void DummyFunctionToMakeSureReferencesGetCopy_DO_NOT_DELETE_THIS_CODE()
        //{
        //    var dummyType = typeof(System.Configuration.ConfigurationManager);
        //    Console.WriteLine(dummyType.FullName);
        //}
    }



    public enum LogType
    {
        None = 0,
        JobLog = 1,
        ServiceLog = 2,
        Common = 3,
    }
}