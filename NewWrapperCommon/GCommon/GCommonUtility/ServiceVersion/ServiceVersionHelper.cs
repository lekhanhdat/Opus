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
using System.Text;
using System.IO;
using System.Xml;
using AvePoint.GCommon.Contract.Server.ControlPanel.UpdateManager.Object;
using Microsoft.Win32;

namespace AvePoint.GCommon.Utility.ServiceVersion
{
    public class ServiceVersionHelper
    {
        private static AveLogger mLog = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string PRODUCT_VERSION_TAG_NAME = "configuration/properties/ProductVersion";
        private const string DISPLAY_VERSION_TAG_NAME = "configuration/properties/DisplayVersion";
        private const string DEPLOY_ID_TAG_NAME = "configuration/properties/DeployId";
        private const string PROPERTIES_TAG_NAME = "configuration/properties";
        private const string DEPLOY_ID = "DeployId";

        public static AvePoint.GCommon.Contract.ServiceVersion.Object.ServiceVersionInfoDto GetVersion(bool isControl,  
            string productType = ServiceVersionConstains.DOCAVE_REG_ROOT_FOLDER,
            string regKeyType = ServiceVersionConstains.REG_AGENT_KEY)
        {
            string controlVersionFilePath = GetControlVersionFilePath();
            string versionDir = isControl ? controlVersionFilePath : AppDomain.CurrentDomain.BaseDirectory;
            string versionFile = Path.Combine(versionDir, "ServiceVersion.config");
            try
            {
                if (!File.Exists(versionFile))
                {
                    string installFolder = GetVersionFilePath(productType, regKeyType);
                    versionFile = Path.Combine(System.IO.Path.Combine(installFolder, "bin"), "ServiceVersion.config");
                }
                return GetVersionInfo(versionFile);
            }
            catch (Exception e)
            {
                mLog.Error("Can not find the version file at the default location. " + e.ToString());
                return new Contract.ServiceVersion.Object.ServiceVersionInfoDto
                {
                    DisplayVersion = "DocAve 6",
                    ProductVersion = string.Empty,
                };
            }
        }
        
        /// <summary>
        /// Read install path from Registry.
        /// 
        /// REG_AGENT_KEY = "Path";
        /// REG_MANAGER_KEY = "InstallPath";
        /// REG_SHELL_KEY = "Shell";
        /// REG_ROOT_FOLDER = "SOFTWARE\\AvePoint\\DocAve6";
        /// </summary>
        /// <param name="regKeyType">AvePoint.GCommon.Utility.ServiceVersion.ServiceVersionConstains</param>
        /// <param name="fileinBin">Default: true</param>
        /// <returns></returns>
        public static AvePoint.GCommon.Contract.ServiceVersion.Object.ServiceVersionInfoDto GetVersion(string productType, string regKeyType, bool fileinBin = true)
        {
            string installFolder = GetVersionFilePath(productType, regKeyType);

            string versionFile = string.Empty;
            if (fileinBin)
            {
                versionFile = System.IO.Path.Combine(System.IO.Path.Combine(installFolder, "bin"), "ServiceVersion.config");
            }
            else
            {
                versionFile = System.IO.Path.Combine(installFolder, "ServiceVersion.config");
            }
            return GetVersionInfo(versionFile);
        }

        private static AvePoint.GCommon.Contract.ServiceVersion.Object.ServiceVersionInfoDto GetVersionInfo(string versionFile)
        {
            FileStream versionFileStream = null;
            System.Xml.XmlDocument reader = null;
            try
            {
                versionFileStream = System.IO.File.OpenRead(versionFile);
                reader = new System.Xml.XmlDocument();
                reader.Load(versionFileStream);
                XmlNode deployIdNode = reader.SelectSingleNode(DEPLOY_ID_TAG_NAME);
                AvePoint.GCommon.Contract.ServiceVersion.Object.ServiceVersionInfoDto version = new AvePoint.GCommon.Contract.ServiceVersion.Object.ServiceVersionInfoDto()
                {
                    ProductVersion = reader.SelectSingleNode(PRODUCT_VERSION_TAG_NAME).InnerText,
                    DisplayVersion = reader.SelectSingleNode(DISPLAY_VERSION_TAG_NAME).InnerText,
                    DeployId = deployIdNode == null ? string.Empty : deployIdNode.InnerText
                };
                return version;
            }
            catch (Exception e)
            {
                mLog.Error("Can not find the version file at the default location. " + e.ToString());
                return new Contract.ServiceVersion.Object.ServiceVersionInfoDto
                {
                    DisplayVersion = "DocAve 6",
                    ProductVersion = string.Empty,
                };
            }
            finally
            {
                if (versionFileStream != null)
                {
                    versionFileStream.Dispose();
                }
            }
        }

        /// <summary>
        /// 由于Control和TimerService走的是一套逻辑，所以如果启动的是TimerService的话，需要找本层目录
        /// </summary>
        /// <returns>control 或 timer service的配置文件的目录</returns>
        private static string GetControlVersionFilePath()
        {
            string controlVersionFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
            if (!Directory.Exists(controlVersionFilePath))
            {
                controlVersionFilePath = Directory.GetParent(controlVersionFilePath).FullName;
            }
            return controlVersionFilePath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyType"></param>
        /// <param name="productType">ServiceVersionConstains DOCAVE_REG_ROOT_FOLDER or NETAPP_REG_ROOT_FOLDER or IBM_REG_ROOT_FOLDER</param>
        /// <returns></returns>
        private static string GetVersionFilePath(string productType,string keyType)
        {
            RegistryKey reg = Registry.LocalMachine.OpenSubKey(productType);
            string path = reg.GetValue(keyType).ToString();
            return path;
        }

        /// <summary>
        /// 为NetApp和NetApp_IBM获取ServiceVersion.config文件中的DeployId
        /// </summary>
        /// <returns></returns>
        public static ViewDeployIDDto GetDeployId()
        {
            ViewDeployIDDto result = new ViewDeployIDDto();
            string controlVersionFilePath = GetControlVersionFilePath();
            string versionFile = Path.Combine(controlVersionFilePath, "ServiceVersion.config");
            if (!File.Exists(versionFile))
            {
                result.HasError = true;
                result.Error = new Exception(string.Format("Can not find the version file at the location: {0}", versionFile));
                mLog.Error(string.Format("Can not find the version file at the location: {0}", versionFile));
            }
            else
            {
                try
                {
                    result.DeployID = GetDeployIdByConfig(versionFile);
                    result.HasError = false;
                }
                catch (Exception e)
                {
                    mLog.Error("Error when get the deploy id from the ServiceVersion.config: " + e.ToString());
                    result.DeployID = null;
                    result.HasError = true;
                    result.Error = e;
                }
            }
            return result;
        }

        private static string GetDeployIdByConfig(string versionFile)
        {
            string result = string.Empty;
            FileStream versionFileStream = null;
            System.Xml.XmlDocument reader = null;
            try
            {
                versionFileStream = File.OpenRead(versionFile);
                reader = new System.Xml.XmlDocument();
                reader.Load(versionFileStream);
                result = reader.SelectSingleNode(DEPLOY_ID_TAG_NAME).InnerText;
            }
            finally
            {
                if (versionFileStream != null)
                {
                    versionFileStream.Dispose();
                }
            }
            return result;
        }

        public static bool CreateOrUpdateDeployId(string deployId)
        {
            bool result = false;
            string controlVersionFilePath = GetControlVersionFilePath();
            string versionFile = Path.Combine(controlVersionFilePath, "ServiceVersion.config");
            if (!File.Exists(versionFile))
            {
                mLog.Error(string.Format("Can not find the version file at the location: {0}", versionFile));
            }
            else
            {
                XmlDocument reader = null;
                try
                {
                    reader = new XmlDocument();
                    reader.Load(versionFile);
                    XmlNode propertiesNode = reader.SelectSingleNode(PROPERTIES_TAG_NAME);
                    XmlNode deployIdNode = reader.SelectSingleNode(DEPLOY_ID_TAG_NAME);
                    if (propertiesNode != null)
                    {
                        if (deployIdNode == null)
                        {
                            XmlElement deployIdEle = reader.CreateElement(DEPLOY_ID);
                            deployIdEle.InnerText = deployId;
                            propertiesNode.AppendChild(deployIdEle);
                        }
                        else
                        {
                            deployIdNode.InnerText = deployId;
                        }
                        reader.Save(versionFile);
                        mLog.Info("add node deployId in ServiceVersion.config failed.");
                        result = true;
                    }
                    else
                    {
                        mLog.Error("cannot find the node properties in ServiceVersion.config, add node deployId failed.");
                    }
                }
                catch (Exception e)
                {
                    mLog.Error("error when add node deployId in ServiceVersion.config: " + e);
                }
            }
            return result;
        }
    }

    public class ServiceVersionConstains
    {
        
        public const string REG_AGENT_KEY = "Path";
        public const string REG_MANAGER_KEY = "InstallPath";
        public const string REG_SHELL_KEY = "Shell";
        public const string DOCAVE_REG_ROOT_FOLDER = "SOFTWARE\\AvePoint\\DocAve6";
        public const string NETAPP_REG_ROOT_FOLDER = "SOFTWARE\\Network Appliance\\SnapManager for SharePoint 7";
        public const string IBM_REG_ROOT_FOLDER = "SOFTWARE\\IBM\\SnapManager for SharePoint 7";

    }
}
