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


namespace AvePoint.GCommon.Utility.ServiceVersion
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using AvePoint.GCommon.Contract.ServiceVersion.Object;

    public class ServiceVersionHelper
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private const string PRODUCT_VERSION_TAG_NAME = "configuration/properties/ProductVersion";
        private const string DISPLAY_VERSION_TAG_NAME = "configuration/properties/DisplayVersion";
        private const string SERVICE_VERSION_FILE_NAME = "ServiceVersion.config";

        public static ServiceVersionInfoDto GetVersion(string versionFolder)
        {
            string versionFile = SecurityUtils.SafeCombinePath(versionFolder,SERVICE_VERSION_FILE_NAME);
            FileStream versionFileStream = File.OpenRead(versionFile);
            XmlDocument reader = new XmlDocument();
            try
            {
                reader.Load(versionFileStream);
                ServiceVersionInfoDto version = new ServiceVersionInfoDto
                {
                    ProductVersion = reader.SelectSingleNode(PRODUCT_VERSION_TAG_NAME).InnerText,
                    DisplayVersion = reader.SelectSingleNode(DISPLAY_VERSION_TAG_NAME).InnerText
                };
                return version;
            }
            catch (Exception e)
            {
                mLog.Error("Can not find the version file at the default location. " + e.ToString());
                return null;
            }
            finally
            {
                versionFileStream.Close();
            }
        }

        public static ServiceVersionInfoDto GetVersion(bool isControl)
        {
            string versionFolder = GetServiceVersionDirectory(isControl);
            return GetVersion(versionFolder);
        }


        private static string GetServiceVersionDirectory(bool isControl)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var webBinPath = Path.Combine(baseDirectory, "bin");
            var serviceVersionPath = Path.Combine(webBinPath, SERVICE_VERSION_FILE_NAME);
            // web role
            if (isControl && Directory.Exists(webBinPath))
            {
                if (!File.Exists(serviceVersionPath))
                {
                    mLog.Error("Web role environment, but service version file not exist");
                }
                return webBinPath;
            }
            // timer & agent worker role
            return baseDirectory;
        }
    }
}
