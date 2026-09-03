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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using LS.SPWorkflowProcessor.Services;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Globalization;
using System.Data.SqlClient;
using AvePoint.Common;
using System.Security.Cryptography;
using System.Text;
using AvePoint.Wrapper.Resource.Workflow;

namespace LS.SPWorkflowProcessor
{
    [Flags]
    public enum TemplateFileConflictRulesEnum :byte
    {
        KeepTarget = 0,
        KeepSource = 1,
    }

    public class SPWorkflowProcessorRuntime
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static List<RuntimeService> mServices;
        public static string RootDirectory { get; private set; }

        private static InstanceProcessOption backupInstanceOption;

        public static InstanceProcessOption BackupInstanceOption
        {
            get 
            {
                if (backupInstanceOption == null)
                {
                    backupInstanceOption = new InstanceProcessOption();
                }
                return backupInstanceOption;
            }
        }

        /// <summary>
        /// 目前是DPM专用属性，控制在同一个web下是否允许在不同的list下添加同名的SPD/Nintex workflow。
        /// 正常情况下，在同一个web下是不可以存在同名的SPD/Nintex workflow。
        /// </summary>
        public static bool IsAllowDuplicateSPDAndNintexInSameWeb
        {
            get;
            set;
        }

        private static bool processAssociation = false;
        internal static bool ProcessAssociation
        {
            get { return processAssociation; }
            set 
            {
                LoadConfigurationBeforeChanged();
                processAssociation = value;
            }
        }

        private static bool processInstance = false;
        internal static bool ProcessInstance
        {
            get { return processInstance; }
            set 
            {
                LoadConfigurationBeforeChanged();
                processInstance = value;
            }
        }

        private static bool processMarkOnlyWorkflow = true;
        public static bool ProcessMarkOnlyWorkflow
        {
            get { return processMarkOnlyWorkflow; }
            set
            {
                LoadConfigurationBeforeChanged();
                processMarkOnlyWorkflow = value; 
            }
        }

        private static bool updateCurrentVersion = false;
        public static bool UpdateCurrentVersion
        {
            get { return updateCurrentVersion; }
            set
            {
                LoadConfigurationBeforeChanged(); 
                updateCurrentVersion = value; 
            }
        }

        private static bool performanceMonitorIsOn = false;
        public static bool PerformanceMonitorIsOn
        {
            get { return performanceMonitorIsOn; }
            set 
            { 
                LoadConfigurationBeforeChanged();
                performanceMonitorIsOn = value;
            }
        }

        private static bool restoreHistoryOnly = true;
        public static bool RestoreHistoryOnly
        {
            get { return restoreHistoryOnly; }
            set
            {
                LoadConfigurationBeforeChanged(); 
                restoreHistoryOnly = value; 
            }
        }

        private static bool skipRunningInstance = false;
        internal static bool SkipRunningInstance
        {
            get { return skipRunningInstance; }
            set
            {
                LoadConfigurationBeforeChanged(); 
                skipRunningInstance = value; 
            }
        }

        private static bool restoreCurrentVersionOnly = false;
        /// <summary>
        /// true：只转移current version的template file
        /// </summary>
        public static bool RestoreCurrentVersion
        {
            get { return restoreCurrentVersionOnly; }
            set
            {
                LoadConfigurationBeforeChanged(); 
                restoreCurrentVersionOnly = value;
            }
        }

        private static bool restartRunningInstance = false;
        internal static bool RestartRunningInstance
        {
            get { return restartRunningInstance; }
            set 
            { 
                LoadConfigurationBeforeChanged();
                restartRunningInstance = value;
            }
        }

        private static bool findTaskItemByOriginalUniqueIdOnly = true;
        public static bool FindTaskItemByOriginalUniqueIdOnly
        {
            get 
            {
                return findTaskItemByOriginalUniqueIdOnly;
            }
            set 
            {
                LoadConfigurationBeforeChanged();
                findTaskItemByOriginalUniqueIdOnly = value;
            }
        }

        /// <summary>
        /// 当原端site users中不存在此user时,是否强制还原此user
        /// 如果强制还原,当user是dead account,使用placeHolder的情况下可能会导致ensure进来一个title和placeHolder一样,login是原端login的user
        /// 默认值暂时设置为true,保证还原workflow过程中restore user逻辑与以前一致
        /// add in DocAve 6.7
        /// </summary>
        public static bool ForceEnsureUsersInWorkflow { get; set; }

        private static int pauseTimeAfterCancelWorkflow = 8;
        public static int PauseTimeAfterCancelWorkflow 
        {
            get { return pauseTimeAfterCancelWorkflow; }
            set { pauseTimeAfterCancelWorkflow = value; }
        }

        private static int pauseTimeBeforeRestartWorkflow = 1;
        public static int PauseTimeBeforeRestartWorkflow
        {
            get { return pauseTimeBeforeRestartWorkflow; }
            set
            {
                LoadConfigurationBeforeChanged(); 
                pauseTimeBeforeRestartWorkflow = value; 
            }
        }

        private static bool getSP2013WorkflowDefinitionForPR = false;
        public static bool GetSP2013WorkflowDefinitionForPR
        {
            get { return getSP2013WorkflowDefinitionForPR; }
            set { getSP2013WorkflowDefinitionForPR = value; }
        }

        /// <summary>
        /// 缓存当前job中还原过的reusable workflow template id
        /// 内部有并发控制，不需要在调用的地方额外加锁
        /// </summary>
        private static ReusableWorkflowTemplateBaseIdCache restoredWorkflowTemplateIdCache = new ReusableWorkflowTemplateBaseIdCache();
        public static ReusableWorkflowTemplateBaseIdCache RestoredWorkflowTemplateIdCache
        {
            get { return restoredWorkflowTemplateIdCache; }
            set { restoredWorkflowTemplateIdCache = value; }
        }

        #region nintex workflow

        private static int nintexWorkflowMaxBackupInstanceProgressCount = 5000;
        //备份nintex workflow instance progress的最大数量，默认值为0，不备份
        internal static int NintexWorkflowMaxBackupInstanceProgressCount
        {
            get { return nintexWorkflowMaxBackupInstanceProgressCount; }
            set
            {
                LoadConfigurationBeforeChanged();
                nintexWorkflowMaxBackupInstanceProgressCount = value; 
            }
        }

        [ThreadStatic]
        private static SqlConnection mNintexConfigDBConnection = null;
        [ThreadStatic]
        public static bool HasSetNintexConfigDBConnection = false;
        public static SqlConnection NintexConfigDBConnection
        {
            get
            {
                return mNintexConfigDBConnection;
            }
            set
            {
                mNintexConfigDBConnection = value;
                HasSetNintexConfigDBConnection = true;
            }
        }

        [ThreadStatic]
        private static SqlConnection mNintexContentDBConnection = null;
        [ThreadStatic]
        public static bool HasSetNintexContentDBConnection = false;
        public static SqlConnection NintexContentDBConnection 
        {
            get 
            {
                return mNintexContentDBConnection;
            }
            set 
            {
                mNintexContentDBConnection = value;
                HasSetNintexContentDBConnection = true;
            }
        }
        public static void CloseNinexDBConnection()
        {
            if (mNintexConfigDBConnection != null)
            {
                mNintexConfigDBConnection.Dispose();
                mNintexConfigDBConnection = null;
                HasSetNintexConfigDBConnection = false;
            }
            if (mNintexContentDBConnection != null)
            {
                mNintexContentDBConnection.Dispose();
                mNintexContentDBConnection = null;
                HasSetNintexContentDBConnection = false;
            }
        }

        public static Dictionary<string, Dictionary<string, string>> CustomOutcomeIDNames = new Dictionary<string, Dictionary<string, string>>();
        public static Dictionary<string, Dictionary<string, string>> CustomOutcomeNameIDs = new Dictionary<string, Dictionary<string, string>>();
        [ThreadStatic]
        private static Nullable<bool> isNintexDllInstalled = null;
        public static bool IsNintexDllInstalled
        {
            get
            {
                if (isNintexDllInstalled == null)
                {
                    try
                    {
                        string assemblyString = "Nintex.Workflow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=913f6bae0ca5ae12";
                        Assembly assembly;
                        if (SPWorkflowProcessorRuntime.AllProcessorParams != null && SPWorkflowProcessorRuntime.AllProcessorParams.ContainsKey("NWAssemblyName"))
                        {
                            assemblyString = SPWorkflowProcessorRuntime.AllProcessorParams["NWAssemblyName"];
                        }
                        assembly = Assembly.Load(assemblyString);
                        SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_IsInstalled, "true");
                        isNintexDllInstalled = true;
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, WrapperWorkflowResource.LoadAssemblyError, e.Message);
                        SPWorkflowProcessorRuntime.Log(Logs.NintexWorkflow_IsInstalled, "false");
                        isNintexDllInstalled = false;
                    }
                }
                return isNintexDllInstalled == null ? false : isNintexDllInstalled.Value;
            }
        }

        public static bool OverwriteNWMessageTemplates
        {
            get;
            set;
        }

        public static bool OverwriteNWContants
        {
            get;
            set;
        }

        private static UserDefiniedActionIdMappingManager udaMappingManager = new UserDefiniedActionIdMappingManager();
        public static UserDefiniedActionIdMappingManager UDAMappingManager
        {
            get { return udaMappingManager; }
        }

        public static List<Guid> NeedRestoreUserDefiniedActionId = new List<Guid>();

        /// <summary>
        /// 多线程还原web可能会有问题，在web post action时调用，但是没有什么用，以后考虑去掉
        /// 
        /// 使用ThreadSafe来代替原始的。
        /// </summary>
        public static ThreadSafeDictionary<int, int> RestoredUDAMapping = new ThreadSafeDictionary<int, int>();

        #endregion


        public static bool Process13ModelWFInstanceByNative
        {
            get;
            set;
        }

        private static bool restoreBuiltinOnly = false;
        public static bool RestoreBuiltinOnly
        {
            get { return restoreBuiltinOnly; }
            set 
            { 
                LoadConfigurationBeforeChanged();
                restoreBuiltinOnly = value;
            }
        }

        private static bool restoreParentAssociationIfNotFound = false;
        internal static bool RestoreParentAssociationIfNotFound
        {
            get { return restoreParentAssociationIfNotFound; }
            set 
            { 
                LoadConfigurationBeforeChanged();
                restoreParentAssociationIfNotFound = value;
            }
        }

        private static bool replaceSpecificForCompatibility = false;
        public static bool ReplaceSpecificForCompatibility
        {
            get { return replaceSpecificForCompatibility; }
            set 
            {
                LoadConfigurationBeforeChanged();
                replaceSpecificForCompatibility = value;
            }
        }

        private static AveObjectModelFactory mFactory = null;

        public static AveObjectModelFactory ObjectModelFactory
        {
            get
            {
                if (mFactory == null)
                {
                    mFactory = WrapperRuntime.CurrentContext.ModelFactory;
                }
                if (mFactory == null)
                {
                   logger.Warn("SPWorkflowProcessorRuntime.ObjectModelFactory is null.");
                }
                return mFactory;
            }
            set 
            {
                mFactory = value;
            }
        }
        [ThreadStatic]
        private static  AveMappingManager mMappingManager = null;
        public static AveMappingManager MappingManager
        {
            get { return mMappingManager; }
            set { mMappingManager = value; }
        }

        private static int cachedAssociationCount = 0;
        public static int CachedAssociationCount
        {
            get { return cachedAssociationCount; }
            set
            {
                LoadConfigurationBeforeChanged(); 
                cachedAssociationCount = value; 
            }
        }

        /// <summary>
        /// 还原workflow template file时，如果modified time不同，keep原端还是目的端template file content
        /// </summary>
        private static TemplateFileConflictRulesEnum templateFileConflictRules = TemplateFileConflictRulesEnum.KeepSource;
        public static TemplateFileConflictRulesEnum TemplateFileConflictRules
        {
            get { return templateFileConflictRules; }
            set
            {
                LoadConfigurationBeforeChanged(); 
                templateFileConflictRules = value;
            }
        }

        /// <summary>
        /// 改成Safe Thread，以后还需要去掉
        /// </summary>
        public static ThreadSafeDictionary<string, string> AllProcessorParams
        {
            get;
            set;
        }

        public static List<ICustomWorkflowAssociationProc> CustomAssociationProcessors
        { get; set; }

        public static List<ICustomWorkflowInstanceProc> CustomInstanceProcessors
        { get; set; }

        /// <summary>
        /// Safe，只有在load configuration的时候才会初始化
        /// </summary>
        public static Dictionary<Guid, SPWorkflowFileContentCustomProc> CustomTemplateContentProcessors
        { get; set; }


        private static void AddProcessorToCollection(XmlElement processor)
        {

            if (processor.Name.Equals("AssociationProcessor"))
            {
                AddProcessorToCollection(processor, null);
            }
            else if (processor.Name.Equals("InstanceProcessor"))
            {
                AddProcessorToCollection(processor, null);
            }
            else if (processor.Name.Equals("Service"))
            {
                AddProcessorToCollection(processor, null);
            }
            else
            {
                Guid templateId = Guid.Empty;
                if (!processor.Name.Equals("Default"))
                    templateId = new Guid(processor.GetAttribute("TemplateId"));

                SPWorkflowFileContentCustomProc customProc = new SPWorkflowFileContentCustomProc();
                foreach (XmlNode node in processor.ChildNodes)
                {
                    if (!(node is XmlElement))
                        continue;

                    XmlElement fileProc = (XmlElement)node;
                    AddProcessorToCollection(fileProc, customProc);
                }
                if (customProc.AspxFileProcessor != null || customProc.ConfigFileProcessor != null || customProc.RulesFileProcessor != null || customProc.XomlFileProcessor != null)
                    CustomTemplateContentProcessors.Add(templateId, customProc);
            }
        }

        private static void AddProcessorToCollection(XmlElement processor, SPWorkflowFileContentCustomProc customFileProc)
        {
            string dir = processor.GetAttribute("Assembly");
            string cls = processor.GetAttribute("Class");
            if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(cls))
                return;

            Assembly predefinedAssem = typeof(SPWFInstanceUnit).Assembly;
            Type procType = predefinedAssem.GetType(cls, false);
            if (procType == null)
            {
                if (dir.StartsWith("[LS]", StringComparison.Ordinal))
                    dir = RootDirectory + dir.Substring(4);
                Assembly assem = Assembly.LoadFrom(dir);
                procType = assem.GetType(cls, false);
            }
            if (procType == null)
                return;

            if (processor.Name.Equals("AssociationProcessor"))
            {
                //ICustomWorkflowAssociationProc
                ICustomWorkflowAssociationProc instance = (ICustomWorkflowAssociationProc)LSInvoker.CreateNewInstance(procType);
                CustomAssociationProcessors.Add(instance);
            }
            else if (processor.Name.Equals("InstanceProcessor"))
            {
                ICustomWorkflowInstanceProc instance = (ICustomWorkflowInstanceProc)LSInvoker.CreateNewInstance(procType);
                CustomInstanceProcessors.Add(instance);
            }
            else if (processor.Name.Equals("Service"))
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                foreach (XmlNode paramNode in processor.ChildNodes)
                {
                    if (paramNode.Name.Equals("Parameter"))
                    {
                        XmlElement paramElement = (XmlElement)paramNode;
                        string key = paramElement.GetAttribute("Name");
                        string value = paramElement.GetAttribute("Value");
                        if (value.StartsWith("[LS]", StringComparison.Ordinal))
                            value = RootDirectory + value.Substring(4);
                        if (!param.ContainsKey(key))
                            param.Add(key, value);
                    }
                }
                AddService(procType,param);
                return;
            }
            else
            {
                SPWorkflowFileContentProc instance = (SPWorkflowFileContentProc)LSInvoker.CreateNewInstance(procType);
                if (processor.Name.Equals("ConfigContentProcessor"))
                {
                    customFileProc.ConfigFileProcessor = instance;
                }
                else if (processor.Name.Equals("XOMLContentProcessor"))
                {
                    customFileProc.XomlFileProcessor = instance;
                }
                else if (processor.Name.Equals("RulesContentProcessor"))
                {
                    customFileProc.RulesFileProcessor = instance;
                }
                else if (processor.Name.Equals("FormContentProcessor"))
                {
                    customFileProc.AspxFileProcessor = instance;
                }
                else if (processor.Name.Equals("XAMLContentProcessor")) 
                {
                    customFileProc.XamlFileProcessor = instance;
                }
            }

            foreach (XmlNode paramNode in processor.ChildNodes)
            {
                if (paramNode.Name.Equals("Parameter"))
                {
                    XmlElement paramElement = (XmlElement)paramNode;
                    string key = paramElement.GetAttribute("Name");
                    string value = paramElement.GetAttribute("Value");
                    if (!AllProcessorParams.ContainsKey(key))
                        AllProcessorParams.Add(key, value);
                }
            }
        }

        private static string currentProcessTempLocation;

        /// <summary>
        /// workflow缓存数据的位置
        /// </summary>
        public static string CurrentProcessTempLocation
        {
            get
            {
                if (string.IsNullOrEmpty(currentProcessTempLocation))
                {
                    currentProcessTempLocation = AveWrapperConstants.WrapperTempFolder;
                    logger.Debug("Init current process workflow cache temp location,{0}.", currentProcessTempLocation);
                }
                return currentProcessTempLocation;
            }
        }

        //需要加锁控制,否则多线程会有问题
        static string mCurrentConfigFile;
        static object mCurrentConfigFileLockObject = new object();

        public static void LoadConfigurationBeforeChanged()
        {
            //在外围对option赋值之前，先去LoadConfiguration，否则会被默认值覆盖
            string mConfigFile = @"AgentCommonSPWorkflowConfiguration.xml";
            string configFilePath = AveEnv.AgentDataFolder + "\\WrapperCommon\\" + mConfigFile;
            SPWorkflowProcessorRuntime.LoadConfiguration(configFilePath, AveEnv.AgentRootFolder);
        }

        public static void LoadConfiguration(string configFile, string rootDirectory)
        {
            lock (mCurrentConfigFileLockObject)
            {
                if (string.Equals(configFile, mCurrentConfigFile))
                {
                    return;
                }
                if (!File.Exists(configFile))
                {
                    return;
                }
                logger.Info("Start loading workflow configuration file :{0}",configFile);
                mCurrentConfigFile = configFile;

                RootDirectory = rootDirectory.TrimEnd('\\');
                AllProcessorParams = new ThreadSafeDictionary<string, string>();
                CustomAssociationProcessors = new List<ICustomWorkflowAssociationProc>();
                CustomInstanceProcessors = new List<ICustomWorkflowInstanceProc>();
                CustomTemplateContentProcessors = new Dictionary<Guid, SPWorkflowFileContentCustomProc>();
                XmlDocument doc = null;
                XmlNode firstNode = null;
                try
                {
                    if (!File.Exists(configFile))
                        return;
                    doc = new XmlDocument();
                    doc.Load(configFile);

                    firstNode = (XmlNode)doc.DocumentElement;
                    if (firstNode.Name != "LS.Workflow")
                    {
                        return;
                    }
                    foreach (XmlNode node in firstNode.ChildNodes)
                    {
                        if (!(node is XmlElement))
                            continue;
                        XmlElement xe = (XmlElement)node;
                        bool changed = false;
                        if (xe.Name == "Configuration")
                        {
                            #region Self
                            //ProcessAssociation,ProcessInstance两个option不应放到配置文件中，先不进行回写，67会考虑去掉
                            if (xe.GetAttribute("ProcessAssociation").Equals("true", StringComparison.OrdinalIgnoreCase))
                            {
                                ProcessAssociation = true;
                            }
                            if (xe.GetAttribute("ProcessInstance").Equals("true", StringComparison.OrdinalIgnoreCase))
                            {
                                ProcessInstance = true;
                            }
                            ProcessMarkOnlyWorkflow = WrapperConfiguration.GetAttributeFromNode(node, "ProcessMarkOnlyWorkflow", true, ref changed);
                            
                            string tempPerformanceMonitorIsOn = WrapperConfiguration.GetAttributeFromNode(node, "PerformanceMonitorIsOn", "off", ref changed);
                            if (string.Equals(tempPerformanceMonitorIsOn, "on", StringComparison.OrdinalIgnoreCase))
                            {
                                PerformanceMonitorIsOn = true;
                            }

                            CachedAssociationCount = WrapperConfiguration.GetAttributeFromNode(node, "CachedAssociationCount", 0, ref changed);
                            PauseTimeBeforeRestartWorkflow = WrapperConfiguration.GetAttributeFromNode(node, "PauseTimeBeforeRestartWorkflow", 1, ref changed);
                            try
                            {
                                string tempTemplateFileConflictRules = WrapperConfiguration.GetAttributeFromNode(node, "TemplateFileConflictRules", "KeepSource", ref changed);
                                TemplateFileConflictRules = (TemplateFileConflictRulesEnum)Enum.Parse(typeof(TemplateFileConflictRulesEnum), tempTemplateFileConflictRules, true);
                            }
                            catch (ArgumentException e)
                            {
                                logger.Log(AveLogLevel.DEBUG, "An argument error occurred while loading configuration, error message: {0}", e);
                            }
                            RestoreHistoryOnly = WrapperConfiguration.GetAttributeFromNode(node, "RestoreHistoryOnly", true, ref changed);
                            SkipRunningInstance = WrapperConfiguration.GetAttributeFromNode(node, "SkipRunningInstance", false, ref changed);
                            RestartRunningInstance = WrapperConfiguration.GetAttributeFromNode(node, "RestartRunningInstance", false, ref changed);
                            RestoreCurrentVersion = WrapperConfiguration.GetAttributeFromNode(node, "RestoreCurrentVersion", false, ref changed);
                            UpdateCurrentVersion = WrapperConfiguration.GetAttributeFromNode(node, "UpdateCurrentVersion", false, ref changed);
                            //RestoreBuiltinOnly
                            RestoreBuiltinOnly = WrapperConfiguration.GetAttributeFromNode(node, "RestoreBuiltinOnly", false, ref changed);
                            //RestoreParentAssociationIfNotFound
                            RestoreParentAssociationIfNotFound = WrapperConfiguration.GetAttributeFromNode(node, "RestoreParentAssociationIfNotFound", false, ref changed);
                            //ReplaceSpecificForCompatibility
                            ReplaceSpecificForCompatibility = WrapperConfiguration.GetAttributeFromNode(node, "ReplaceSpecificForCompatibility", false, ref changed);
                            //FindTaskItemByOriginalUniqueIdOnly
                            FindTaskItemByOriginalUniqueIdOnly = WrapperConfiguration.GetAttributeFromNode(node, "FindTaskItemByOriginalUniqueIdOnly", true, ref changed);
                            ForceEnsureUsersInWorkflow = WrapperConfiguration.GetAttributeFromNode(node, "ForceEnsureUsersInWorkflow", true, ref changed);
                            #endregion

                            #region children node default values
                            const bool backupTaskItem = true;
                            const bool backupHistoryItem = true;
                            #endregion
                            #region load configuration children
                            var nintexNode = WrapperConfiguration.EnsureXmlNode(xe, "Nintex", ref changed);
                            NintexWorkflowMaxBackupInstanceProgressCount = WrapperConfiguration.GetAttributeFromNode(nintexNode, "NintexWorkflowMaxBackupInstanceProgressCount", NintexWorkflowMaxBackupInstanceProgressCount, ref changed);
                            var instanceOptionNode = WrapperConfiguration.EnsureXmlNode(xe, "InstanceOption", ref changed);
                            var processTaskItem = WrapperConfiguration.GetAttributeFromNode(instanceOptionNode, "BackupTaskItem", backupTaskItem, ref changed);
                            BackupInstanceOption.SetTaskOption(processTaskItem);
                            var processHistoryItem = WrapperConfiguration.GetAttributeFromNode(instanceOptionNode, "BackupHistoryItem", backupHistoryItem, ref changed);
                            BackupInstanceOption.SetHistoryOption(processHistoryItem);
                            #endregion
                            
                        } 
                        else if (xe.Name.Equals("CustomAssociationProcessors") || xe.Name.Equals("CustomInstanceProcessors") || xe.Name.Equals("CustomTemplateContentProcessors") || xe.Name.Equals("RuntimeServices"))
                        {//这几个节点动态加载不需要回写
                            foreach (XmlNode node2 in xe.ChildNodes)
                            {
                                if (!(node2 is XmlElement))
                                    continue;
                                AddProcessorToCollection((XmlElement)node2);
                            }
                        }
                        else if (xe.Name.Equals("Mappings", StringComparison.OrdinalIgnoreCase))
                        {
                            LoadMapping(xe);
                        }

                        if (changed)
                        {
                            doc.Save(configFile);
                        }
                    }

                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.ConfigFileLoadError, ex);
                }
                finally
                {
                    if (doc != null)
                        doc.RemoveAll();
                }
            }
        }

        private static void LoadMapping(XmlElement xe)
        {
            try
            {
                foreach (XmlNode node in xe.ChildNodes)
                {
                    if (!(node is XmlElement))
                        continue;
                    XmlElement mappingNode = node as XmlElement;
                    if (mappingNode.Name.Equals("EmailMappings", StringComparison.Ordinal))
                    {
                        foreach (XmlNode emailNode in mappingNode.ChildNodes)
                        {
                            if (!(emailNode is XmlElement))
                                continue;
                            XmlElement email = emailNode as XmlElement;
                            if (email.HasAttribute("sourceEmail") && email.HasAttribute("destinationEmail"))
                            {
                                string src = email.GetAttribute("sourceEmail");
                                string des = email.GetAttribute("destinationEmail");
                                if (!string.IsNullOrEmpty(src) && !string.IsNullOrEmpty(des))
                                {
                                    SPWorkflowCommon.EmailMapping[src.ToLower(CultureInfo.CurrentCulture)] = des;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.RuntimeLoadMappingError, e);
            }
        }

        public static void AddService(RuntimeService service, Dictionary<string, string> param)
        {
            if (mServices == null)
                mServices = new List<RuntimeService>();
            mServices.Add(service);

        }

        public static void AddService(Type type,Dictionary<string,string> param)
        {
            if (!type.IsSubclassOf(typeof(RuntimeService)))
                return;
            RuntimeService instance = (RuntimeService)LSInvoker.CreateNewInstance(type,new Type[]{typeof(Dictionary<string,string>)},new object[]{param});
            
            AddService(instance,param);
        }

        public static void Stop()
        {
            if (mServices == null)
                return;
            foreach (RuntimeService service in mServices)
                service.Dispose();
            mServices.Clear();
        }



        public static void Log(string key, params string[] args)
        {
            if (mServices == null)
                return;

            foreach (RuntimeService service in mServices)
            {
                switch (service.RuntimeServiceType)
                {
                    case ServiceType.LoggingService:
                        ((LoggingService)service).WriteLog(key, args);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 使用nintex workflow的方法export workflow使用的user信息
        /// 只有Migration使用
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        public static SPWorkflowUserInfo GetNWAdminUserInfo()
        {
            SPWorkflowUserInfo result = null;
            if (mServices == null)
            {
                return result;
            }

            foreach (RuntimeService service in mServices)
            {
                switch (service.RuntimeServiceType)
                {
                    case ServiceType.UserInfoProvider:
                        result = ((NWAdminUserInfoProvider)service).GetUserInfo();
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// 获取需要disable的nintex workflow action的信息，以及nintex workflow中需要额外替换的一些信息,
        /// 从配置文件读取，只有Migration使用
        /// </summary>
        /// <param name="externalReplaceDic"></param>
        /// <param name="needDisableAction"></param>
        public static void GetNWFConvertConfigurations(ref Dictionary<string, string> externalReplaceDic, ref List<string> needDisableAction)
        {
            if (mServices == null)
            {
                return;
            }
            foreach (RuntimeService service in mServices)
            {
                switch (service.RuntimeServiceType)
                {
                    case ServiceType.UserInfoProvider:
                        var nWAdminService = service as NWAdminUserInfoProvider;
                        externalReplaceDic = nWAdminService.GetNintexWorkflowExternalReplaceDic();
                        needDisableAction = nWAdminService.GetNintexWorkflowDisableInvaildAction();
                        break;
                    default:
                        break;
                }
            }
        }

        public static IAveUser OnUserMapping(string loginName)
        {
            IAveUser result = null;
            if (mServices == null)
                return null;
            foreach (RuntimeService service in mServices)
            {
                try
                {
                    switch (service.RuntimeServiceType)
                    {
                        case ServiceType.UserMappingService:
                            result = ((UserMappingService)service).GetOrCreateUser(loginName);
                            if (result != null)
                            {
                                return result;
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.ERROR, WrapperWorkflowResource.MappingUserError, e);
                }
            }
            return null;
        }

        public static IAvePrincipal OnMemberMapping(string loginName)
        {
            IAvePrincipal result = null;
            if (mServices == null)
                return null;
            foreach (RuntimeService service in mServices)
            {
                try
                {
                    switch (service.RuntimeServiceType)
                    {
                        case ServiceType.UserMappingService:
                            result = ((UserMappingService)service).GetOrCreateMember(loginName);
                            if (result != null)
                            {
                                logger.Debug("Find the member id:{0} for loginName:{1},Name:{2},Type:{3},ByName:{4}",
                                    result.ID, result.LoginName, result.Name, result.GetType().FullName, loginName);
                                return result;
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.ERROR, WrapperWorkflowResource.MappingUserError, e);
                }
            }

            logger.Debug("Cannot find principal by Name:{0}", loginName);
            return null;
        }

        public static void OnCacheData(string siteUrl,string siteId, string webId, string listId, string parentId, int itemId, string index, byte[] data)
        {
            if (mServices == null)
                return;
            foreach (RuntimeService service in mServices)
            {
                try
                {
                    switch (service.RuntimeServiceType)
                    {
                        case ServiceType.CacheService:
                            ((CacheService)service).CacheData(siteUrl,siteId, webId, listId, parentId, itemId, index, data);
                            return;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.ERROR, WrapperWorkflowResource.CacheDataError, e);
                }
            }
        }

        public static void ExecutePostAction(SPWFAssociationProc associationProcessor, SPWFAssociationProc associationProcessor13Model, SPWFInstanceProc instanceProcessor, SPWFInstanceProc instanceProcessor13Model)
        {
            if (mServices == null)
                return;
            foreach (RuntimeService service in mServices)
            {
                try
                {
                    switch (service.RuntimeServiceType)
                    {
                        case ServiceType.PostponeActionService:
                            ((PostponeActionService)service).Execute(associationProcessor, instanceProcessor);
                            continue;
                        case ServiceType.WFDataFilterService:
                            if (instanceProcessor13Model != null && instanceProcessor13Model.ParentItem != null && instanceProcessor13Model.ParentItem.Web != null)
                            {
                                ((WFTaskAndHistoryDataFilter)service).Filter(instanceProcessor13Model.ParentItem.Web, instanceProcessor13Model.TaskListIdAndInstanceMapping, instanceProcessor13Model.HistoryListIdAndInstanceMapping);
                            }
                            continue;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.ERROR, WrapperWorkflowResource.PostActionError, e);
                }
            }
        }

        public static string OnLanguageMapping(LanguageMappingScopeEnum scope, string originalName)
        {
            if (mServices == null)
                return originalName;
            foreach (RuntimeService service in mServices)
            {
                try
                {
                    switch (service.RuntimeServiceType)
                    {
                        case ServiceType.LanguageMappingService:
                            return ((LanguageMappingService)service).GetMappedName(scope, originalName);
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.ERROR, WrapperWorkflowResource.LanguageMappingError, e);
                }
            }
            return originalName;
        }

        public static byte[] BackupCustomData(Guid siteId, Guid webId)
        {
            try
            {
                if (SPWorkflowProcessorRuntime.IsNintexDllInstalled)
                {
                    Dictionary<string, List<Hashtable>> data = new Dictionary<string, List<Hashtable>>();

                    List<Hashtable> templates = new List<Hashtable>();
                    NintexMessageTemplateHelper messageTemplateHelper = new NintexMessageTemplateHelper(siteId);
                    messageTemplateHelper.BackupMessageTemplates(webId, templates, siteId);

                    data.Add("Nintex.MessageTemplates", templates);


                    List<Hashtable> contants = new List<Hashtable>();
                    NintexWorkflowConstantHelper contantHelper = new NintexWorkflowConstantHelper(siteId);
                    contantHelper.BackupConstants(webId, contants, siteId);
                    contants.ForEach(constant => {
                        if (constant.ContainsKey("Sensitive") && (bool)constant["Sensitive"])
                        {
                            var value = Decrypt(constant["Value"] as string, string.Empty);
                            constant.AddEx("Value", value);
                        }
                    });

                    data.Add("Nintex.WorkflowConstants", contants);

                    List<Hashtable> userDefinedActions = new List<Hashtable>();
                    NintexWorkflowUserDefinedActionHelper userDefinedActionHelper = new NintexWorkflowUserDefinedActionHelper(siteId);
                    userDefinedActionHelper.BackupUserDefinedActions(siteId,webId, userDefinedActions);


                    data.Add("Nintex.UserDefinedActions", userDefinedActions);

                    byte[] backupData= SerializeCustomData(data);

                    return backupData;
                }
                else
                {
                    return null;
                }
            }
            catch(Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.RestoreCustomDataError, ex);
            }

            return null;
        }

        #region Constants
        private static string Decrypt(string cryptedString, string decryptMethod)
        {
            var encryptionKey = Encoding.ASCII.GetBytes("K6El8wgHjwJ5QhZH36p6aJzGQ62ff17S");
            var encryptionIV = Encoding.ASCII.GetBytes("zPTUqtdrldqdrz3y");
            if (string.IsNullOrEmpty(cryptedString)) return string.Empty;
            string str = null;
            try
            {
                return DecryptUsingDes(cryptedString);
            }
            catch
            {
                try
                {
                    str = DecryptStringFromString_Aes(cryptedString, encryptionKey, encryptionIV);
                }
                catch (Exception ex)
                {
                    logger.Warn("An error occurred while decrypting value. {0}", ex);
                }
            }
            return str;
        }

        private static string DecryptUsingDes(string cryptedString)
        {
            var encryptionKeyDES = Encoding.ASCII.GetBytes("itWe7kxn");
            DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
            using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(cryptedString)))
            {
                using (CryptoStream stream2 = new CryptoStream(stream, provider.CreateDecryptor(encryptionKeyDES, encryptionKeyDES), CryptoStreamMode.Read))
                {
                    using (StreamReader reader = new StreamReader(stream2))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        private static string DecryptStringFromString_Aes(string cipherText, byte[] Key, byte[] IV)
        {
            if (cipherText == null || cipherText.Length <= 0) throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0) throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0) throw new ArgumentNullException("IV");
            using (AesCryptoServiceProvider provider = new AesCryptoServiceProvider())
            {
                provider.Key = Key;
                provider.IV = IV;
                ICryptoTransform transform = provider.CreateDecryptor(provider.Key, provider.IV);
                using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(stream2))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
        }
        
        private static string EncryptStringToString_Aes(string plainText)
        {
            if (plainText == null || plainText.Length <= 0)
            {
                throw new System.ArgumentNullException("plainText");
            }
            var encryptionKey = Encoding.ASCII.GetBytes("K6El8wgHjwJ5QhZH36p6aJzGQ62ff17S");
            var encryptionIV = Encoding.ASCII.GetBytes("zPTUqtdrldqdrz3y");
            string result;
            using (AesCryptoServiceProvider aesCryptoServiceProvider = new AesCryptoServiceProvider())
            {
                aesCryptoServiceProvider.Key = encryptionKey;
                aesCryptoServiceProvider.IV = encryptionIV;
                System.Security.Cryptography.ICryptoTransform transform = aesCryptoServiceProvider.CreateEncryptor(aesCryptoServiceProvider.Key, aesCryptoServiceProvider.IV);
                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                {
                    using (System.Security.Cryptography.CryptoStream cryptoStream = new System.Security.Cryptography.CryptoStream(memoryStream, transform, System.Security.Cryptography.CryptoStreamMode.Write))
                    {
                        using (System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(plainText);
                            streamWriter.Flush();
                            cryptoStream.FlushFinalBlock();
                            streamWriter.Flush();
                        }
                        result = System.Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
            return result;
        }


        #endregion




        /// <summary>
        /// compress & serialize Custom Data
        /// </summary>
        /// <returns></returns>
        internal static byte[] SerializeCustomData(Dictionary<string, List<Hashtable>> data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, data);
            byte[] serializedData = LSUtilityOfBytes.LSStreamToBytes(stream);


            using (MemoryStream stream2 = new MemoryStream(serializedData.Length))
            {
                using (GZipStream stream3 = new GZipStream(stream2, CompressionMode.Compress, true))
                {
                    stream3.Write(serializedData, 0, serializedData.Length);
                }
                serializedData = stream2.GetBuffer();
                Array.Resize<byte>(ref serializedData, Convert.ToInt32(stream2.Length));
            }

            stream.Dispose();

            #region Dispose
            foreach (KeyValuePair<string, List<Hashtable>> pair in data)
            {
                foreach (Hashtable ht in pair.Value)
                {
                    ht.Clear();
                }
                pair.Value.Clear();
            }
            data.Clear();
            #endregion

            return serializedData;
        }

        /// <summary>
        /// Decompress & Deserialized Custom Data
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<string, List<Hashtable>> DeserializeCustomData(byte[] serializedData)
        {
            byte[] decompressedData = new byte[0];
            using (MemoryStream tempStream = new MemoryStream(serializedData))
            {
                tempStream.Position = 0L;
                byte[] temp = new byte[4096];
                using (GZipStream gzipStream = new GZipStream(tempStream, CompressionMode.Decompress, true))
                {
                    int readLen;
                    while ((readLen = gzipStream.Read(temp, 0, 4096)) != 0)
                    {
                        LSUtilityOfBytes.LSAppendBytes(ref decompressedData, temp, 0, readLen);
                    }
                }
                temp = null;
            }

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Binder = new WorkflowSerializationBinder();
            MemoryStream stream = new MemoryStream(decompressedData);
            Dictionary<string, List<Hashtable>> data = (Dictionary<string, List<Hashtable>>)formatter.Deserialize(stream);
            stream.Dispose();
            decompressedData = null;
            return data;
        }

        public static void RestoreCustomData(IAveWeb web,byte[] serializedData,bool isPostAction=false)
        {
            Guid siteId=web.Site.ID;
            Guid webId = web.ID;
            Dictionary<string, List<Hashtable>> data = DeserializeCustomData(serializedData);
            //每个类型只会存在一个 List<Hashtable>类型的数据，所以不需要担心key重复
            Dictionary<string, List<Hashtable>> needPostCache = new Dictionary<string, List<Hashtable>> { };

            try
            {

                foreach (KeyValuePair<string, List<Hashtable>> pairs in data)
                {
                    if (pairs.Key.StartsWith("nintex", StringComparison.OrdinalIgnoreCase))
                    {
                        if (SPWorkflowProcessorRuntime.IsNintexDllInstalled)
                        {
                            string tableName = pairs.Key.Split(new char[] { '.' })[1];
                            if (tableName.Equals("MessageTemplates", StringComparison.OrdinalIgnoreCase))
                            {
                                NintexMessageTemplateHelper messageTemplateHelper = new NintexMessageTemplateHelper(siteId);
                                messageTemplateHelper.RestoreMessageTemplates(siteId, webId, pairs.Value);
                                logger.Debug("Successful restore the NintexMessageTemplate");
                            }
                            else if (tableName.Equals("WorkflowConstants", StringComparison.OrdinalIgnoreCase))
                            {
                                NintexWorkflowConstantHelper contantHelper = new NintexWorkflowConstantHelper(siteId);
                                pairs.Value.ForEach(constant => {
                                    if (constant.ContainsKey("Sensitive") && (bool)constant["Sensitive"])
                                    {
                                        var value = EncryptStringToString_Aes(constant["Value"] as string);
                                        constant.AddEx("Value", value);
                                    }
                                });
                                contantHelper.RestoreConstants(siteId, webId, pairs.Value);
                                logger.Debug("Successful restore the NintexWorkflowConstant");
                            }
                            else if (tableName.Equals("UserDefinedActions", StringComparison.OrdinalIgnoreCase))
                            {
                                //user defined actions 里面的listid需要替换，因为在restore web时还没有初始化list id的mapping，
                                //所以将user defined actions放到cache里，然后在post action还原
                                if(isPostAction)
                                {
                                    NintexWorkflowUserDefinedActionHelper userDefinedActionHelper = new NintexWorkflowUserDefinedActionHelper(siteId);
                                    userDefinedActionHelper.RestoreUserDefinedActions(siteId, webId, pairs.Value);
                                }
                                else
                                {
                                    NintexWorkflowUserDefinedActionHelper userDefinedActionHelper = new NintexWorkflowUserDefinedActionHelper(siteId);
                                    userDefinedActionHelper.CacheUserDefinedActions(siteId, webId, pairs.Value);
                                    needPostCache.Add(pairs.Key, pairs.Value);
                                    logger.Debug("Successful add the NintexWorkflowUserDefinedAction to cache.");
                                }
                            }
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, WrapperWorkflowResource.RestoreCustomDataError, ex);
            }
            finally
            {
                if (needPostCache !=null&& needPostCache.Count > 0)
                {
                    logger.Info("Some of custom data has been put into cache, and we will restore it later in the web post action.IsPostAction:{0}",isPostAction);
                    SPWorkflowProcessorRuntime.OnCacheData(web.Site.Url,siteId.ToString(), webId.ToString(), string.Empty, "CustomData", 0, string.Empty, SerializeCustomData(needPostCache));
                }
            }
        }
    }

    /// <summary>
    /// 控制是否处理instance关联的数据
    /// </summary>
    public class InstanceProcessOption
    {
        public bool ProcessTaskItem { get; private set; }
        public bool ProcessHistoryItem { get; private set; }

        public InstanceProcessOption()
        {
            ProcessTaskItem = true;
            ProcessHistoryItem = true;
        }

        public void SetTaskOption(bool processTask)
        {
            SPWorkflowProcessorRuntime.LoadConfigurationBeforeChanged();
            ProcessTaskItem = processTask;
        }

        public void SetHistoryOption(bool processHistory)
        {
            SPWorkflowProcessorRuntime.LoadConfigurationBeforeChanged();
            ProcessHistoryItem = processHistory;
        }
    }

    public class ReusableWorkflowTemplateBaseIdCache
    {
        private object privateLock = new object();

        //siteid , webid, sourceWFid, destinationWFid  虽然我们keep id， 但是re wf 可能会skip。不能让后续还wf 把re wf template 给覆盖，否则目的端id 不匹配，打不开。
        private Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Guid>>> mRestoredReusableWorkflowTemplateCache;

        public ReusableWorkflowTemplateBaseIdCache()
        {
            mRestoredReusableWorkflowTemplateCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Guid>>> { };
        }

        public void Add(Guid siteId, Guid webId, Guid sourceWFId,Guid destWFId)
        {
            lock (privateLock)
            {
                if (!mRestoredReusableWorkflowTemplateCache.ContainsKey(siteId))
                {
                    mRestoredReusableWorkflowTemplateCache.Add(siteId, new Dictionary<Guid, Dictionary<Guid, Guid>> { });
                }
                if (!mRestoredReusableWorkflowTemplateCache[siteId].ContainsKey(webId))
                {
                    mRestoredReusableWorkflowTemplateCache[siteId].Add(webId, new Dictionary<Guid, Guid> { });
                }
                if (!mRestoredReusableWorkflowTemplateCache[siteId][webId].ContainsKey(sourceWFId))
                {
                    mRestoredReusableWorkflowTemplateCache[siteId][webId][sourceWFId] = destWFId;
                } 
            }
        }

        public bool Contains(Guid siteId, Guid webId, Guid templateBaseId)
        {
            lock (privateLock)
            {

                if (mRestoredReusableWorkflowTemplateCache.ContainsKey(siteId)
                    && mRestoredReusableWorkflowTemplateCache[siteId].ContainsKey(webId)
                        && mRestoredReusableWorkflowTemplateCache[siteId][webId].ContainsKey(templateBaseId))
                {
                    return true;
                }
                return false; 
            }
        }

        public void Remove(Guid siteId, Guid webId)
        {
            lock (privateLock)
            {

                if (mRestoredReusableWorkflowTemplateCache.ContainsKey(siteId))
                {
                    if (webId == Guid.Empty)
                    {
                        Remove(siteId);
                    }
                    else if (mRestoredReusableWorkflowTemplateCache[siteId].ContainsKey(webId))
                    {
                        mRestoredReusableWorkflowTemplateCache[siteId][webId].Clear();
                        mRestoredReusableWorkflowTemplateCache[siteId].Remove(webId);
                    }
                }
            }
        }

        public void Remove(Guid siteId)
        {
            lock (privateLock)
            {
                if (mRestoredReusableWorkflowTemplateCache.ContainsKey(siteId))
                {
                    foreach (var value in mRestoredReusableWorkflowTemplateCache[siteId].Values)
                    {
                        value.Clear();
                    }
                    mRestoredReusableWorkflowTemplateCache[siteId] = null;
                    mRestoredReusableWorkflowTemplateCache.Remove(siteId);
                }
            }
        }

        public Guid GetDestinationTemplateId(Guid siteId, Guid webId, Guid templateBaseId)
        {
            lock (privateLock)
            {

                if (mRestoredReusableWorkflowTemplateCache.ContainsKey(siteId)
                    && mRestoredReusableWorkflowTemplateCache[siteId].ContainsKey(webId)
                        && mRestoredReusableWorkflowTemplateCache[siteId][webId].ContainsKey(templateBaseId))
                {
                    return mRestoredReusableWorkflowTemplateCache[siteId][webId][templateBaseId];
                }
                return Guid.Empty;
            }
        }


    }

    public class UserDefiniedActionIdMapping
    {
        private static object obj=new object();
        private ThreadSafeDictionary<Guid, Guid> userDefinedActionIdMapping = new ThreadSafeDictionary<Guid, Guid> { };

        public Guid SiteId;
        public Guid WebId;

        public UserDefiniedActionIdMapping(Guid siteId, Guid webId, Dictionary<Guid, Guid> userDefinedActionId)
        {
            SiteId = siteId;
            WebId = webId;
            foreach (var idMapping in userDefinedActionId)
            {
                userDefinedActionIdMapping.AddEx(idMapping.Key,idMapping.Value);
            }
        }

        public UserDefiniedActionIdMapping(Guid siteId, Guid webId, ThreadSafeDictionary<Guid, Guid> userDefinedActionId)
        {
            SiteId = siteId;
            WebId = webId;
            userDefinedActionIdMapping = userDefinedActionId;
        }

        public void Clear()
        {
            userDefinedActionIdMapping.Clear();
        }

        public void Add(Guid source, Guid destination)
        {
            userDefinedActionIdMapping.AddEx(source, destination);
        }

        public void AddRange(Dictionary<Guid, Guid> idMapping)
        {
            foreach (var pair in idMapping)
            {
                Add(pair.Key, pair.Value);
            }
        }

        public bool TryGetValue(Guid key,out Guid value)
        {
            return userDefinedActionIdMapping.TryGetValue(key, out value);
        }

        public bool ContainsKey(Guid staticId)
        {
            return userDefinedActionIdMapping.ContainsKey(staticId);
        }

    }

    public class UserDefiniedActionIdMappingManager
    {
        private static object obj = new object();

        List<UserDefiniedActionIdMapping> udaMapping = new List<UserDefiniedActionIdMapping> { };

        public UserDefiniedActionIdMappingManager()
        {
            
        }

        public void Clear(Guid siteId, Guid webId)
        {
            lock (obj)
            {
                for (int k = 0; k < udaMapping.Count; k++)
                {
                    if ((siteId == Guid.Empty && webId == Guid.Empty) || (siteId == udaMapping[k].SiteId && webId == udaMapping[k].WebId))
                    {
                        udaMapping[k].Clear();
                    }
                }
            }
        }

        public void Clear()
        {
            Clear(Guid.Empty, Guid.Empty);
        }

        public void Add(UserDefiniedActionIdMapping mapping)
        {
            lock (obj)
            {
                udaMapping.Add(mapping);
            }
        }

        public UserDefiniedActionIdMapping TryGetUDAIDMapping(Guid siteid,Guid webid)
        {
            return udaMapping.Find(mapping => mapping.SiteId == siteid && mapping.WebId == webid);
        }

        public void Add(Guid siteid,Guid webId,Dictionary<Guid,Guid> udaIDMapping)
        {
            lock (obj)
            {
                var idMapping = udaMapping.Find(item => item.SiteId == siteid && item.WebId == webId);
                if (idMapping == null)
                {
                    udaMapping.Add(new UserDefiniedActionIdMapping(siteid, webId, udaIDMapping));
                }
                else
                {
                    idMapping.AddRange(udaIDMapping);
                }
            }
        }

        public void Add(Guid siteid, Guid webId, Guid source, Guid destination)
        {
            Dictionary<Guid, Guid> mapping = new Dictionary<Guid, Guid>();
            mapping.AddEx(source, destination);
            Add(siteid, webId, mapping);
        }
    }

}
