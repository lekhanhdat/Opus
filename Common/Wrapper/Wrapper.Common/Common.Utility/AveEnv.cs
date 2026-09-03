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





using System.IO;
using AvePoint.Common;
using System.Text;
using Microsoft.Win32;
using System.Net;
using System.Xml;
using System.Reflection;
using AvePoint.GCommon;
using System;
using AvePoint.Wrapper.Resource;
using System.Collections.Generic;
using Util;
using AvePoint.GCommon.Utility;

namespace AvePoint.Wrapper.Common
{
    public class AveSPEnv
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public const string UNKNOWN_STRING = "Unknown";
        public const string AVE_SP14_REGISTER_PATH = @"software\Microsoft\shared Tools\web Server Extensions\14.0";
        public const string AVE_SP14_DLL_PATH = @"Microsoft Shared\Web Server Extensions\14\ISAPI";
        private static bool loadIsMoss = false;
        private static string mProductInfo = "DocAve";  //TODO: perpare for change
        private static string mSPVersion = "5.2.2.0";    //TODO: prepare for change
        private static int mSslMode = -1;
        private static string mVssVersion = UNKNOWN_STRING;
        private static string mMossVersion = UNKNOWN_STRING;
        private static bool mIsMoss = false;
        private static string mInstallPath = UNKNOWN_STRING;
        private static List<string> mossFileInfo = new List<string>() { "Microsoft.Office.Server.dll", "Microsoft.Office.Server.UserProfiles.dll", "Microsoft.SharePoint.Taxonomy.dll" };

        private static string mMossConfigPath = "AvePoint.Wrapper.Common.MossConfig.xml";

        public static string SPVersion
        {
            get
            {
                string dataDir = Path.Combine(AveEnv.AgentRootFolder, "data");
                using (StreamReader sr = new StreamReader(Path.Combine(dataDir, "version.txt"), Encoding.UTF8))
                {
                    mSPVersion = sr.ReadLine()?.Trim();
                }
                return mSPVersion;
            }
        }

        public static string ProductInfo
        {
            get
            {
                return mProductInfo;
            }
        }

        public static string SPInstallPath
        {
            get
            {
                if (UNKNOWN_STRING == mInstallPath)
                {
                    LoadSPInstallPath();
                }
                return mInstallPath;
            }
        }

        public static bool IsMoss
        {
            get
            {
                if (!loadIsMoss)
                {
                    if (IsServerMode)
                    {
                        LoadSPVersion();
                    }
                    else
                    {
                        LoadMoss();
                    }
                }
                return mIsMoss;
            }
        }

        public static bool IsServerMode { private get; set; }

        public static bool IsPublishing
        {
            get
            {
                //TODO implement this
                if (!loadIsMoss)
                {
                    LoadSPVersion();
                }
                return mIsMoss;
            }
        }

        public static int SslMode
        {
            get
            {
                if (-1 == mSslMode)
                {
                    LoadSPVersion();
                }
                return mSslMode;
            }
        }
        private static string GetAssemblyVersion(string assemblyPath)
        {
            string version = UNKNOWN_STRING;
            assemblyPath = SecurityUtils.SafeCombinePath(assemblyPath);
            if (File.Exists(assemblyPath))
            {
                AssemblyName anm = AssemblyName.GetAssemblyName(assemblyPath);
                version = anm.Version.ToString();
            }
            return version;
        }
        private static void LoadSPVersion()
        {
            try
            {
                string isApiDir = SecurityUtils.SafeCombinePath(System.Environment.GetEnvironmentVariable("CommonProgramFiles"), AVE_SP14_DLL_PATH);
                string vssFile = SecurityUtils.SafeCombinePath(isApiDir, "Microsoft.SharePoint.dll");
                if (File.Exists(vssFile))
                {
                    mVssVersion = GetAssemblyVersion(vssFile);
                }
                if (!mIsMoss)
                {
                    bool isAllInclude = true;
                    foreach (string mossFile in mossFileInfo)//有的环境安装Offcie的软件，也会带Microsoft.Office.Server.dll，因此添加多个判断；
                    {
                        string file = SecurityUtils.SafeCombinePath(isApiDir, mossFile);
                        if (!File.Exists(file))
                        {
                            isAllInclude = false;
                            break;
                        }
                    }
                    mIsMoss = isAllInclude;
                }
                if (mIsMoss)
                {
                    string mossFile = SecurityUtils.SafeCombinePath(isApiDir, "Microsoft.SharePoint.Server.dll");
                    if (File.Exists(mossFile))
                    {
                        mMossVersion = GetAssemblyVersion(mossFile);
                    }
                }
                loadIsMoss = true;
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, WrapperCommonResource.AWCLoadSPVersionXMLError, ex.ToString());
            }
        }

        private static void LoadSPInstallPath()
        {
            RegistryKey rootKey = null;
            RegistryKey key = null;
            try
            {
                rootKey = Registry.LocalMachine;
                key = rootKey.OpenSubKey(AVE_SP14_REGISTER_PATH);
                string location = key.GetValue("Location")?.ToString();
                string disk = location.Substring(0, location.IndexOf(":\\", StringComparison.OrdinalIgnoreCase));
                string path = location.Substring(location.IndexOf(":\\", StringComparison.OrdinalIgnoreCase) + 1);
                IPHostEntry hostEntry = Dns.GetHostEntry(AveEnv.AgentAddress);
                string mIP = UNKNOWN_STRING;
                if (hostEntry.AddressList.Length > 0)
                {
                    mIP = hostEntry.AddressList[0].ToString();
                }
                mInstallPath = "\\\\" + mIP + "\\" + disk + "$" + path + "TEMPLATE";
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCLoadInstallPahtError, e.ToString());
                mInstallPath = UNKNOWN_STRING;
            }
            finally
            {
                if (rootKey != null)
                    rootKey.Close();
                if (key != null)
                    key.Close();
            }
        }

        private static void LoadMoss()
        {
            try
            {
                XmlDocument mXmlDocument = new XmlDocument();
                using (StreamReader sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(mMossConfigPath)))
                {
                    mXmlDocument.LoadXml(sr.ReadToEnd());
                }
                XmlNode rootNode = mXmlDocument.DocumentElement.FirstChild;
                if (rootNode.Name.Equals("Moss"))
                {
                    mIsMoss = XmlConvert.ToBoolean(rootNode.Attributes["isMoss"].Value.ToString());
                }
                loadIsMoss = true;
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, WrapperCommonResource.AWCLoadMossXMLError, ex.ToString());
            }
        }
    }
}
