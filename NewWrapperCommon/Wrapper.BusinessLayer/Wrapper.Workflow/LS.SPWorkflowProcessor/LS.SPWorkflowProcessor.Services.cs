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
using System.Diagnostics;
using System.IO;
using System.Text;

using LS.SPWorkflowProcessor.Resources;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.Workflow;

namespace LS.SPWorkflowProcessor.Services
{
    public enum ServiceType
    {
        LoggingService,
        PerformanceMonitorService,
        UserMappingService,
        CacheService,
        PostponeActionService,
        LanguageMappingService,
        WFDataFilterService,
        UserInfoProvider
    }

    public class RuntimeService
    {
        public ServiceType RuntimeServiceType
        {
            get;
            set;
        }

        public RuntimeService()
        { }

        public RuntimeService(Dictionary<string, string> param)
        {

        }

        public virtual void Dispose()
        {

        }
    }

    #region Logging Service

    public enum LoggingServiceType
    {
        File,
        SQLLite,
        SQLServer,
        EventLog,
    }

    public class LoggingService : RuntimeService
    {
        private string mCurrentProcess = Process.GetCurrentProcess().ProcessName;
        public string CurrentProcess
        {
            get { return mCurrentProcess; }
        }
        private LogLevel mLoggingLevel = LogLevel.High;

        private ScopeMonitor mScopeMonitor;
        public ScopeMonitor ScopeMonitorTimer
        {
            get
            {
                if (mScopeMonitor == null)
                {
                    mScopeMonitor = new ScopeMonitor();
                    mScopeMonitor.MonitorEnabled = true;
                }
                return mScopeMonitor;
            }
        }


        public LogLevel LoggingLevel
        {
            get { return mLoggingLevel; }
            set { mLoggingLevel = value; }
        }

        internal LoggingService()
        {
            RuntimeServiceType = ServiceType.LoggingService;
        }

        internal LoggingService(Dictionary<string, string> param)
        {
            RuntimeServiceType = ServiceType.LoggingService;
        }

        public virtual void WriteLog(string key, params string[] args)
        {

        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }

    internal class FileLoggingService : LoggingService, IDisposable
    {
        private StreamWriter mLogWriter;

        internal FileLoggingService(Dictionary<string, string> param)
        {
            if (!param.ContainsKey("FileName"))
            {
                throw new MissingMemberException("FileName");
            }

            string fileName = param["FileName"];

            bool append = false;
            if (param.ContainsKey("Append"))
            {
                append = bool.Parse(param["Append"]);
            }

            if (param.ContainsKey("LoggingLevel"))
            {
                LoggingLevel = (LogLevel)int.Parse(param["LoggingLevel"]);
            }

            string logResourceFile = SPWorkflowProcessorRuntime.RootDirectory + @"\data\WFCoreMessage.en-US.xml";
            if (param.ContainsKey("LogResourceFile"))
            {
                if (File.Exists(param["LogResourceFile"]))
                    logResourceFile = param["LogResourceFile"];
            }
            LogResoucesManager.LoadResources(logResourceFile);
            mLogWriter = new StreamWriter(fileName, append);
        }

        public override void WriteLog(string key, params string[] args)
        {
            LogLevel level = LogLevel.Monitorable;
            string message;
            string category;

            if (LogResoucesManager.GetResource(key, out level, out message, out category))
            {
                if ((int)level < (int)LoggingLevel)
                    return;
            }
            else
            {
                LogResoucesManager.GetResource(Logs.ResourceItemMissing, out level, out message, out category);
                if ((int)level < (int)LoggingLevel)
                    return;
                message += key;
            }


            if (key == Logs.MonitorScope || key == Logs.MonitorScopeLeave)
            {
                string param0 = string.Empty;
                if (args != null && args[0] != null)
                    param0 = args[0];
                if (key == Logs.MonitorScope)
                    ScopeMonitorTimer.StartMonitor(param0);
                if (key == Logs.MonitorScopeLeave)
                {
                    Array.Resize<string>(ref args, 2);
                    args[1] = ScopeMonitorTimer.GetCurrentDurationString(param0);
                    ScopeMonitorTimer.RemoveMonitor(param0);
                }
            }



            StringBuilder builder = new StringBuilder();
            builder.Append(DateTime.Now.ToString());
            builder.Append("\t");
            builder.Append(SetEntryLength(CurrentProcess, 60));
            builder.Append("\t");
            builder.Append(SetEntryLength(LogResoucesManager.GetLevelString(level.ToString()), 20));
            builder.Append("\t\t");
            builder.Append(SetEntryLength(category, 30));
            builder.Append("\t\t");
            builder.Append(string.Format(message, args));
            mLogWriter.WriteLine(builder.ToString());
        }

        public override void Dispose()
        {
            if (mLogWriter != null)
            {
                mLogWriter.Close();
                mLogWriter.Dispose();
            }
        }

        private string SetEntryLength(string entry, int len)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(entry);
            for (int i = 0; i < len - entry.Length; i++)
                builder.Append(" ");
            return builder.ToString();
        }
    }

    internal class AveWorkflowLoggingServicePrimary : LoggingService, IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        internal AveWorkflowLoggingServicePrimary(Dictionary<string, string> param)
        {
            if (param.ContainsKey("LoggingLevel"))
            {
                LoggingLevel = (LogLevel)int.Parse(param["LoggingLevel"]);
            }

            string logResourceFile = SPWorkflowProcessorRuntime.RootDirectory + @"\data\WFCoreMessage.en-US.xml";
            if (param.ContainsKey("LogResourceFile"))
            {
                if (File.Exists(param["LogResourceFile"]))
                    logResourceFile = param["LogResourceFile"];
            }
            LogResoucesManager.LoadResources(logResourceFile);
        }

        public override void WriteLog(string key, params string[] args)
        {
            LogLevel level = LogLevel.Monitorable;
            string message;
            string category;

            if (LogResoucesManager.GetResource(key, out level, out message, out category))
            {
                if ((int)level < (int)LoggingLevel)
                    return;
            }
            else
            {
                LogResoucesManager.GetResource(Logs.ResourceItemMissing, out level, out message, out category);
                if ((int)level < (int)LoggingLevel)
                    return;
                message += key;
            }


            if (key == Logs.MonitorScope || key == Logs.MonitorScopeLeave)
            {
                string param0 = string.Empty;
                if (args != null && args[0] != null)
                    param0 = args[0];
                if (key == Logs.MonitorScope)
                    ScopeMonitorTimer.StartMonitor(param0);
                if (key == Logs.MonitorScopeLeave)
                {
                    Array.Resize<string>(ref args, 2);
                    args[1] = ScopeMonitorTimer.GetCurrentDurationString(param0);
                    ScopeMonitorTimer.RemoveMonitor(param0);
                }
            }



            StringBuilder builder = new StringBuilder();
            //builder.Append(SetEntryLength(CurrentProcess, 60));
            //builder.Append("\t");$
            //builder.Append(SetEntryLength(LogResoucesManager.GetLevelString(level.ToString()), 20));
            //builder.Append("\t\t");
            builder.Append(level.ToString());
            builder.Append("-");
            //builder.Append(SetEntryLength(category, 30));
            //builder.Append("\t\t");
            builder.Append(category);
            builder.Append("-");
            builder.Append(string.Format(message, args));
            logger.Log(ConvertLogLevel(level),builder.ToString());
        }

        private AveLogLevel ConvertLogLevel(LogLevel originalLevel) 
        {
            switch (originalLevel) 
            {
                case LogLevel.Critical:
                case LogLevel.Exception:
                    return AveLogLevel.ERROR;
                case LogLevel.High:
                    return AveLogLevel.WARN;
                case LogLevel.Medium:
                    return AveLogLevel.INFO;
                case LogLevel.Monitorable:
                    return AveLogLevel.DEBUG;
            }
            return AveLogLevel.INFO;
        }

        public override void Dispose()
        {
        }

        private string SetEntryLength(string entry, int len)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(entry);
            for (int i = 0; i < len - entry.Length; i++)
                builder.Append(" ");
            return builder.ToString();
        }
    }

    internal class SQLLiteLoggingService : LoggingService
    { }

    public class ScopeMonitor
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private bool mMonitorEnabled = false;
        public bool MonitorEnabled
        {
            get { return mMonitorEnabled; }
            set { mMonitorEnabled = value; }
        }

        private Dictionary<string, LSHighPerformanceTimer> mMonitorCollection;
        public Dictionary<string, LSHighPerformanceTimer> MonitorCollection
        {
            get
            {
                if (mMonitorCollection == null)
                    mMonitorCollection = new Dictionary<string, LSHighPerformanceTimer>();

                return mMonitorCollection;
            }
        }

        public LSHighPerformanceTimer this[string monitor]
        {
            get
            {
                if (MonitorCollection.ContainsKey(monitor))
                    return MonitorCollection[monitor];
                else
                    return null;
            }
        }

        public ScopeMonitor()
        {
        }

        public void Dispose()
        {
            MonitorCollection.Clear();

        }

        public void StartMonitor(string monitor)
        {
            if (mMonitorEnabled)
            {
                LSHighPerformanceTimer timer;
                if (MonitorCollection.ContainsKey(monitor))
                    timer = MonitorCollection[monitor];
                else
                {
                    timer = new LSHighPerformanceTimer();
                    MonitorCollection.Add(monitor, timer);
                }
                if (timer != null)
                    timer.Start();
            }
        }

        public void StopMonitor(string monitor)
        {
            if (mMonitorEnabled && MonitorCollection.ContainsKey(monitor))
                MonitorCollection[monitor].Stop();

        }

        public void RemoveMonitor(string monitor)
        {
            if (mMonitorEnabled && MonitorCollection.ContainsKey(monitor))
            {
                MonitorCollection[monitor].Stop();
                MonitorCollection.Remove(monitor);
            }
        }

        public double GetCurrentDuration(string monitor)
        {
            try
            {
                if (mMonitorEnabled && MonitorCollection.ContainsKey(monitor))
                    return MonitorCollection[monitor].CurrentDuration;
                else
                    return 0;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.GetDurationError, e.ToString());
                return 0;
            }
        }

        public double GetDuration(string monitor)
        {
            try
            {
                if (mMonitorEnabled && MonitorCollection.ContainsKey(monitor))
                    return MonitorCollection[monitor].Duration;
                else
                    return 0;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.GetDurationError, e.ToString());
                return 0;
            }
        }

        public string GetCurrentDurationString(string monitor)
        {
            return GetCurrentDuration(monitor).ToString();
        }

        public string GetDurationString(string monitor)
        {
            return GetDuration(monitor).ToString();
        }

        public void ResetCurrentDuration(string monitor)
        {
            if (mMonitorEnabled && MonitorCollection.ContainsKey(monitor))
            {
                double a = MonitorCollection[monitor].CurrentDuration;
            }
        }

    }
    #endregion

    #region User Mapping Service
    public class UserMappingService : RuntimeService
    {
        public UserMappingService()
        {
            RuntimeServiceType = ServiceType.UserMappingService;
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public virtual IAveUser GetOrCreateUser(string loginName)
        {
            return null;
        }

        public virtual IAvePrincipal GetOrCreateMember(string loginName)
        {
            return null;
        }
    }
    #endregion

    public class CacheService : RuntimeService
    {
        public enum CacheOptionEnum
        {
            None,
            FailedOnly,
            All,
        }

        public CacheService()
        {
            RuntimeServiceType = ServiceType.CacheService;
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public virtual void CacheData(string siteUrl,string siteId, string webId, string listId, string parentId, int itemId, string index, byte[] data)
        {

        }
    }


    public class PostponeActionService : RuntimeService
    {
        public PostponeActionService()
        {
            RuntimeServiceType = ServiceType.PostponeActionService;
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public virtual void Execute(SPWFAssociationProc associationProcessor, SPWFInstanceProc instanceProcessor)
        {
        }
    }


    public class LanguageMappingService : RuntimeService
    {
        public LanguageMappingService()
        {
            RuntimeServiceType = ServiceType.LanguageMappingService;
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public virtual string GetMappedName(LanguageMappingScopeEnum scope, string originalName)
        {
            return originalName;
        }
    }

    public class WFAsyncDataFilter : RuntimeService 
    {
        internal WFAsyncDataFilter() 
        {
            RuntimeServiceType = ServiceType.WFDataFilterService;
        }

        public virtual void Filter(IAveWeb web, Dictionary<Guid, List<Guid>> taskListIdAndInstanceMapping, Dictionary<Guid, List<Guid>> historyListIdAndInstanceMapping) 
        { }

        public override void Dispose()
        {
            base.Dispose();
        }
    }

    public class WFTaskAndHistoryDataFilter : WFAsyncDataFilter, IDisposable 
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        internal WFTaskAndHistoryDataFilter()
        { }

        internal WFTaskAndHistoryDataFilter(Dictionary<string, string> param)
        { }

        public override void Filter(IAveWeb web, Dictionary<Guid, List<Guid>> taskListIdAndInstanceMapping, Dictionary<Guid, List<Guid>> historyListIdAndInstanceMapping) 
        {
            System.Threading.Thread.Sleep(SPWorkflowProcessorRuntime.PauseTimeAfterCancelWorkflow * 1000);
            IAveListItemCollection items = null;
            int queryItemsCount = 0;

            foreach(var taskId in taskListIdAndInstanceMapping.Keys)
            {
                try
                {
                    IAveList taskList = web.Lists.GetById(taskId);
                    if (taskList == null) 
                    {
                        continue;
                    }
                    foreach (var instanceId in taskListIdAndInstanceMapping[taskId])
                    {
                        items = taskList.GetItemBySpecifiedField("WF4InstanceId", instanceId);
                        queryItemsCount = items.Count - 1;
                        for (int i = queryItemsCount; i >= 0; --i)
                        {
                            object guidObj = null;
                            try
                            {
                                guidObj = items[i][SPWorkflowCommon.OriginalUniqueIdFieldName];
                            }
                            catch (Exception ex) 
                            {
                                logger.Log(AveLogLevel.DEBUG, "An error occurred while filtering workflow task data, error message: {0}", ex);
                                Trace.WriteLine(ex.Message);
                            }
                            if (guidObj == null)
                            {
                                items[i].Delete();
                            }
                        }
                    }
                }
                catch (Exception e) 
                {
                    logger.Log(AveLogLevel.INFO, "A error occurred while filter task. Detail:{0}", e.Message);
                }
            }

            foreach (var historyId in historyListIdAndInstanceMapping.Keys) 
            {
                try
                {
                    IAveList historyList = web.Lists.GetById(historyId);
                    if (historyList == null) 
                    {
                        continue;
                    }
                    foreach (var instanceId in historyListIdAndInstanceMapping[historyId])
                    {
                        items = historyList.GetItemBySpecifiedField("WorkflowInstance", instanceId);
                        queryItemsCount = items.Count - 1;
                        for (int i = queryItemsCount; i >= 0; --i)
                        {
                            object guidObj = null;
                            try
                            {
                                guidObj = items[i][SPWorkflowCommon.OriginalUniqueIdFieldName];
                            }
                            catch (Exception ex)
                            {
                                logger.Log(AveLogLevel.DEBUG, "An error occurred while filtering workflow history data, error message: {0}", ex);
                                Trace.WriteLine(ex.Message);
                            }
                            if (guidObj == null)
                            {
                                items[i].Delete();
                            }
                        }
                    }
                }
                catch (Exception e) 
                {
                    logger.Log(AveLogLevel.INFO, "A error occurred while filter history. Detail:{0}", e.Message);
                }
            }

        }
    }

    public class SPWorkflowUserInfo
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
    }

    public class UserInfoProvider : RuntimeService
    {
        internal UserInfoProvider()
        {
            RuntimeServiceType = ServiceType.UserInfoProvider;
        }

        public virtual SPWorkflowUserInfo GetUserInfo()
        {
            return new SPWorkflowUserInfo();
        }
        public virtual string GetUserName()
        {
            return string.Empty;
        }

        public virtual string GetPassword()
        {
            return string.Empty;
        }

        public virtual string GetDomain()
        {
            return string.Empty;
        }

        public virtual Dictionary<string, string> GetNintexWorkflowExternalReplaceDic()
        {
            return null;
        }

        public virtual List<string> GetNintexWorkflowDisableInvaildAction()
        {
            return null;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }

    public class NWAdminUserInfoProvider : UserInfoProvider, IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private string mUserName = string.Empty;
        private string mPassword = string.Empty;
        private string mDomainName = string.Empty;
        private Dictionary<string, string> mNintexWorkflowExternalReplaceDic = new Dictionary<string, string>();
        private List<string> mDisableNWFInvaildWorkflowAction = new List<string>();

        internal NWAdminUserInfoProvider() 
        {
            InitDefaultNintexWorkflowReplaceDic();
        }

        private void InitDefaultNintexWorkflowReplaceDic()
        {
           if(mNintexWorkflowExternalReplaceDic==null)
           {
               mNintexWorkflowExternalReplaceDic = new Dictionary<string, string>();
           }
            mNintexWorkflowExternalReplaceDic.AddEx("Nintex.Workflow.Activities.Adapters.SPCheckOutItemAdapter", "Nintex.Workflow.Activities.Adapters.SPCheckOutItemWithKeyAdapter");
            mNintexWorkflowExternalReplaceDic.AddEx("Nintex.Workflow.Activities.Adapters.SPCopyItemAdapter", "Nintex.Workflow.Activities.Adapters.SPCopyItemWithKeyAdapter");
            mNintexWorkflowExternalReplaceDic.AddEx("Nintex.Workflow.Activities.Adapters.SPUndoCheckOutItemAdapter", "Nintex.Workflow.Activities.Adapters.SPUndoCheckOutItemWithKeyAdapter");
            mNintexWorkflowExternalReplaceDic.AddEx("Nintex.Workflow.Activities.Adapters.SPDeleteItemAdapter", "Nintex.Workflow.Activities.Adapters.SPDeleteItemWithKeyAdapter");
            mNintexWorkflowExternalReplaceDic.AddEx("Nintex.Workflow.Activities.Adapters.SPWaitForAdapter", "Nintex.Workflow.Activities.Adapters.SPWaitForWithKeyAdapter");
            mNintexWorkflowExternalReplaceDic.AddEx("Nintex.Workflow.Activities.Adapters.SPSetFieldAdapter", "Nintex.Workflow.Activities.Adapters.SPSetFieldWithKeyAdapter");
            mNintexWorkflowExternalReplaceDic.AddEx("Nintex.Workflow.Activities.Adapters.SPUpdateItemAdapter", "Nintex.Workflow.Activities.Adapters.SPUpdateItemWithKeyAdapter");

        }
          

        internal NWAdminUserInfoProvider(Dictionary<string, string> param)
        {
            InitDefaultNintexWorkflowReplaceDic();
            foreach (string key in param.Keys)
            {
                if (key.Contains("UserName"))
                {
                    mUserName = param[key];
                }
                else if (key.Contains("Password"))
                {
                    mPassword = param[key];
                }
                else if (key.Contains("Domain"))
                {
                    mDomainName = param[key];
                }
                else if (key.Contains("Action"))
                {
                    mDisableNWFInvaildWorkflowAction.Add(param[key]);
                }
                else
                {
                    mNintexWorkflowExternalReplaceDic.AddEx(key, param[key]);
                }
            }
        }

        public override SPWorkflowUserInfo GetUserInfo()
        {
            return new SPWorkflowUserInfo() { UserName = mUserName, Password = mPassword, Domain = mDomainName };
        }

        public override string GetUserName()
        {
            return mUserName;
        }

        public override string GetPassword()
        {
            return mPassword;
        }

        public override string GetDomain()
        {
            return mDomainName;
        }

        public override Dictionary<string, string> GetNintexWorkflowExternalReplaceDic()
        {
            return mNintexWorkflowExternalReplaceDic;
        }

        public override List<string> GetNintexWorkflowDisableInvaildAction()
        {
            return mDisableNWFInvaildWorkflowAction;
        }

        public override void Dispose()
        {
            base.Dispose();
            if (mDisableNWFInvaildWorkflowAction != null)
            {
                mDisableNWFInvaildWorkflowAction.Clear();
            }
            if (mNintexWorkflowExternalReplaceDic != null)
            {
                mNintexWorkflowExternalReplaceDic.Clear();
            }
        }
    }
}
