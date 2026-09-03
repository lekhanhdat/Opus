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
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using AvePoint.Wrapper.Common;
using System.IO;
using AvePoint.Common;
using System.Xml;
using AvePoint.GCommon;

namespace AvePoint.ObjectModel.Server13
{
    class AveCustomWebPartUpdaterUtility
    {
        protected static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public static SpecialWebPartUpdater GetWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveWebPart)
        {
            if (webPart == null)
            {
                return null;
            }
            string webPartTypeName = webPart.GetType().Name;
            var customWebPartUpdaterSettings = AveCustomWebPartUpdaterSetting.GetInstance().CustomWebPartUpdaterSettings;
            if (customWebPartUpdaterSettings.ContainsKey(webPartTypeName))
            {
                var value = customWebPartUpdaterSettings[webPartTypeName];
                if (value is AveCustomWebPartSettingInfo)
                {
                    AveCustomWebPartUpdater updater = new AveCustomWebPartUpdater(webPart, aveWebPart);
                    updater.Init(value as AveCustomWebPartSettingInfo);
                    return updater;
                }
                else if (value is string)
                {
                    lock (customWebPartUpdaterSettings)
                    {
                        Assembly customScriptAssembly = CompileCustomScript(value as string);
                        if (customScriptAssembly != null)
                        {
                            customWebPartUpdaterSettings[webPartTypeName] = customScriptAssembly;
                            Type customType = GetCustomWebPartUpdaterTypeFromAssembly("Ave" + webPartTypeName + "Updater", customScriptAssembly);
                            if (customType != null)
                            {
                                return customType.GetConstructor(new Type[] { typeof(System.Web.UI.WebControls.WebParts.WebPart), typeof(AveWebPart) }).Invoke(new object[] { webPart, aveWebPart }) as SpecialWebPartUpdater;
                            }
                            else
                            {
                                logger.Log(AveLogLevel.WARN, "Cannot get the special updater for {0}", webPartTypeName);
                            }
                        }
                        else
                        {
                            customWebPartUpdaterSettings.Remove(webPartTypeName);
                            logger.Log(AveLogLevel.WARN, "Cannot compile custom script for {0}", webPartTypeName);
                            return null;
                        }
                    }
                }
                else if (value is Assembly)
                {
                    Type customType = GetCustomWebPartUpdaterTypeFromAssembly("Ave" + webPartTypeName + "Updater", value as Assembly);
                    if (customType != null)
                    {
                        return customType.GetConstructor(new Type[] { typeof(System.Web.UI.WebControls.WebParts.WebPart), typeof(AveWebPart) }).Invoke(new object[] { webPart, aveWebPart }) as SpecialWebPartUpdater;
                    }
                    else
                    {
                        logger.Log(AveLogLevel.WARN, "Cannot get the special updater for {0} by the assembly.", webPartTypeName);
                    }
                }
            }
            return null;
        }

        private static Assembly CompileCustomScript(string customScript)
        {
            CSharpCodeProvider objCSharpCodePrivoder = new CSharpCodeProvider();
            CompilerParameters objCompilerParameters = new CompilerParameters();

            objCompilerParameters.ReferencedAssemblies.Add("System.dll");
            objCompilerParameters.ReferencedAssemblies.Add("System.Web.dll");
            objCompilerParameters.ReferencedAssemblies.Add("System.Xml.dll");
            //objCompilerParameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");
            objCompilerParameters.ReferencedAssemblies.Add(typeof(System.Xml.Linq.Extensions).Assembly.Location);
            //objCompilerParameters.ReferencedAssemblies.Add("AgentCommonWrapperCommon.dll");
            objCompilerParameters.ReferencedAssemblies.Add(typeof(AvePoint.Wrapper.Common.AveWebPartBaseInfo).Assembly.Location);
            //objCompilerParameters.ReferencedAssemblies.Add("Microsoft.SharePoint.dll");
            objCompilerParameters.ReferencedAssemblies.Add(typeof(Microsoft.SharePoint.SPSite).Assembly.Location);
            //objCompilerParameters.ReferencedAssemblies.Add(SP2010WrapperServer.dll);
            objCompilerParameters.ReferencedAssemblies.Add(typeof(AvePoint.ObjectModel.Server13.SpecialWebPartUpdater).Assembly.Location);
            if (AveSPEnv.IsMoss)
            {
                //objCompilerParameters.ReferencedAssemblies.Add("Microsoft.SharePoint.Publishing.dll");
                AddPublishingContentTypeId(objCompilerParameters);
            }
            objCompilerParameters.GenerateExecutable = false;
            objCompilerParameters.GenerateInMemory = true;

            CompilerResults cr = objCSharpCodePrivoder.CompileAssemblyFromSource(objCompilerParameters, new string[] { customScript });
            if (cr.Errors.HasErrors)
            {
                return null;
            }
            else
            {
                return cr.CompiledAssembly;
            }
        }

        private static void AddPublishingContentTypeId(CompilerParameters objCompilerParameters)
        {
            objCompilerParameters.ReferencedAssemblies.Add(typeof(Microsoft.SharePoint.Publishing.ContentTypeId).Assembly.Location);
        }

        private static Type GetCustomWebPartUpdaterTypeFromAssembly(string className, Assembly assembly)
        {
            foreach (Type t in assembly.GetTypes())
            {
                if (t.Name.Equals(className))
                {
                    return t;
                }
            }
            return null;
        }
    }

    class AveCustomWebPartUpdaterSetting
    {
        protected static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public Dictionary<string, object> CustomWebPartUpdaterSettings { get; set; }
        private static Object syncRoot = new Object();
        private static AveCustomWebPartUpdaterSetting instance;
        private AveCustomWebPartUpdaterSetting()
        {
            CustomWebPartUpdaterSettings = new Dictionary<string, object>();
            LoadCustomWebPartUpdaterSetting();
        }

        public static AveCustomWebPartUpdaterSetting GetInstance()
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                    {
                        instance = new AveCustomWebPartUpdaterSetting();
                    }
                }
            }
            return instance;
        }

        private void LoadCustomWebPartUpdaterSetting()
        {
            try
            {
                string configFilePath = AveEnv.AgentDataFolder + @"\SP2010\WrapperCommon\CustomWebPartUpdaterSetting.xml";
                if (File.Exists(configFilePath))
                {
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(configFilePath);
                    XmlNodeList updaters = xDoc.GetElementsByTagName("CustomWebPartUpdater");
                    foreach (XmlElement updater in updaters)
                    {
                        string webpartName = updater.GetAttribute("name");
                        string type = updater.GetAttribute("type");
                        switch (type)
                        {
                            case "customOperation":
                                LoadCustomOperationSetting(webpartName, updater);
                                break;
                            case "customScript":
                                LoadCustomScriptSetting(webpartName, updater);
                                break;
                            case "customAssembly":
                                LoadCustomAssemblySetting(webpartName, updater);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "An exception occurred while load CustomWebPartUpdater setting. Exception: {0}", ex.ToString());
            }
        }

        private void LoadCustomScriptSetting(string webpartName, XmlElement updater)
        {
            string scriptFilePath = AveEnv.AgentDataFolder + @"\SP2010\WrapperCommon\" + updater.GetAttribute("scriptFileName");
            if (File.Exists(scriptFilePath))
            {
                string customScript = File.ReadAllText(scriptFilePath, Encoding.UTF8);
                CustomWebPartUpdaterSettings.Add(webpartName, customScript);
            }
            else
            {
                logger.Log(AveLogLevel.WARN, "Cannot find the script file for {0}, script file name: {1}", webpartName, scriptFilePath);
            }
        }

        private void LoadCustomOperationSetting(string webpartName, XmlElement updater)
        {
            AveCustomWebPartSettingInfo settingInfo = new AveCustomWebPartSettingInfo();

            foreach (XmlElement node in updater.ChildElements())
            {
                string nodeName = node.Name;
                string propertyName = node.GetAttribute("propertyName");
                switch (nodeName)
                {
                    case "WebIdReplace":
                        settingInfo.ReplaceWebIdProperties.Add(propertyName);
                        break;
                    case "ListIdReplace":
                        settingInfo.ReplaceListIdProperties.Add(propertyName);
                        break;
                    case "UrlReplace":
                        settingInfo.ReplaceUrlProperties.Add(propertyName);
                        break;
                }
            }
            CustomWebPartUpdaterSettings.Add(webpartName, settingInfo);
        }

        private void LoadCustomAssemblySetting(string webpartName, XmlElement updater)
        {
            string assemblyFileName = AveEnv.AgentDataFolder + @"\SP2010\WrapperCommon\" + updater.GetAttribute("AssemblyFileName");
            if (File.Exists(assemblyFileName))
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(assemblyFileName);
                    CustomWebPartUpdaterSettings.Add(webpartName, assembly);
                }
                catch (Exception ex)
                {
                    logger.Warn("Cannot load custom assembly web part assembly. web part name: {0}, exception: {1}", webpartName, ex.ToString());
                }
            }
        }
    }

    class AveCustomWebPartSettingInfo
    {
        public List<string> ReplaceWebIdProperties { get; set; }
        public List<string> ReplaceListIdProperties { get; set; }
        public List<string> ReplaceUrlProperties { get; set; }

        public AveCustomWebPartSettingInfo()
        {
            ReplaceWebIdProperties = new List<string>();
            ReplaceListIdProperties = new List<string>();
            ReplaceUrlProperties = new List<string>();
        }
    }

    class AveCustomWebPartUpdater : SpecialWebPartUpdater
    {

        public List<string> ReplaceWebIdProperties { get; set; }

        public List<string> ReplaceListIdProperties { get; set; }

        public List<string> ReplaceUrlProperties { get; set; }

        public AveCustomWebPartUpdater(System.Web.UI.WebControls.WebParts.WebPart webPart, AveWebPart aveDoc)
            : base(webPart, aveDoc) { }

        public void Init(AveCustomWebPartSettingInfo settingInfo)
        {
            ReplaceWebIdProperties = settingInfo.ReplaceWebIdProperties;
            ReplaceListIdProperties = settingInfo.ReplaceListIdProperties;
            ReplaceUrlProperties = settingInfo.ReplaceUrlProperties;
        }

        public override bool DoUpateAfterAdd(AveWebPartBaseInfo webPartInfo)
        {

            if (ReplaceWebIdProperties != null && ReplaceWebIdProperties.Count > 0)
            {
                foreach (string property in ReplaceWebIdProperties)
                {
                    ReplaceWebId(property);
                }
            }

            if (ReplaceListIdProperties != null && ReplaceListIdProperties.Count > 0)
            {
                foreach (string property in ReplaceListIdProperties)
                {
                    bool result = ReplaceListId(property);
                    if (!result)
                    {
                        //already add to postaction in ReplaceListId.
                        return false;
                    }
                }
            }

            if (ReplaceUrlProperties != null && ReplaceUrlProperties.Count > 0)
            {
                foreach (string property in ReplaceUrlProperties)
                {
                    ReplaceUrl(property);
                }
            }

            return true;
        }

        private bool ReplaceWebId(string propertyName)
        {
            object originalValue = GetWebPartProperty(propertyName);
            if (originalValue != null)
            {
                Guid originalWebId = new Guid(originalValue.ToString());
                Guid mappingWebId;
                if (mAveWebPart.Manager.Cache.SiteMappingManager.WebIDMapping.TryGetValue(originalWebId, out mappingWebId))
                {
                    Guid newWebId = mappingWebId;
                    SetWebPartProperty(propertyName, newWebId);
                }
            }
            return true;
        }

        private bool ReplaceListId(string propertyName)
        {
            object originalValue = GetWebPartProperty(propertyName);
            if (originalValue != null)
            {
                Guid originalListId = new Guid(originalValue.ToString());
                Guid destListId;
                if (mAveWebPart.Manager.Cache.SiteMappingManager.GetValueFromListIdMapping(originalListId,out destListId))
                {
                    SetWebPartProperty(propertyName, destListId);
                }
                else
                {
                    mAveWebPart.AddUnRestoreWebPartInfo(mAveWebPart.Manager.Web.ID, originalListId, mAveWebPart.Manager.File.ServerRelativeUrl, mAveWebPart.StorageKey);
                    return false;
                }
            }
            return true;
        }

        private bool ReplaceUrl(string propertyName)
        {
            object originalValue = GetWebPartProperty(propertyName);
            if (originalValue != null)
            {
                string originalUrl = originalValue.ToString();
                string newUrl = AveReplaceProcessor.UrlReplace(originalUrl, mAveWebPart.Manager.Cache.SiteManagedMappings, new ReplaceOption(true), mAveWebPart.Manager.Cache.SourceSiteInfo, mAveWebPart.Manager.Cache.DestSiteInfo.ServerRelativeUrl);
                SetWebPartProperty(propertyName, newUrl);
            }
            return true;
        }

        private object GetWebPartProperty(string propertyName)
        {
            Type objType = mWebPart.GetType();
            PropertyInfo property = objType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property != null)
            {
                return property.GetValue(mWebPart, null);
            }
            return null;
        }

        private void SetWebPartProperty(string propertyName, object value)
        {
            Type objType = mWebPart.GetType();
            PropertyInfo property = objType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property != null)
            {
                if (value != null && !value.GetType().Equals(property.PropertyType))
                {
                    Type propertyType = property.PropertyType;
                    #region
                    if (propertyType == typeof(int))
                    {
                        value = Convert.ToInt32(value);
                    }
                    else if (propertyType == typeof(string))
                    {
                        value = Convert.ToString(value);
                    }
                    else if (propertyType == typeof(long))
                    {
                        value = Convert.ToInt64(value);
                    }
                    else if (propertyType == typeof(uint))
                    {
                        value = Convert.ToUInt32(value);
                    }
                    else if (propertyType == typeof(bool))
                    {
                        value = Convert.ToBoolean(value);
                    }
                    else if (propertyType == typeof(Guid))
                    {
                        value = new Guid(value.ToString());
                    }
                    else if (propertyType == typeof(short))
                    {
                        value = Convert.ToInt16(value);
                    }
                    else if (propertyType.BaseType.ToString().Equals("System.Enum"))
                    {
                        value = Enum.Parse(propertyType, value.ToString());
                    }
                    else if (propertyType == typeof(System.Xml.XmlElement))
                    {
                        System.Xml.XmlElement realvalue = property.GetValue(mWebPart, null) as System.Xml.XmlElement;
                        if (realvalue != null)
                        {
                            realvalue.InnerText = value.ToString();
                        }
                        value = realvalue;
                    }
                    #endregion
                }
                property.SetValue(mWebPart, value, null);
            }
        }
    }
}
