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
using System.IO;
using System.Xml;

namespace RecordsHotfixMaintenanceService
{
    public class RecordsEnv
    {
        static RALogger logger = RALogger.GetInstance(typeof(RecordsEnv));
        static RecordsEnv()
        {
            InitEnv();
        }

        public static string LogFolder { get; private set; }
        public static string AppDomainRootFolder { get; set; }
        public static Version ProductVersion { get; set; }

        public static void InitEnv()
        {
            try
            {
                AppDomainRootFolder = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
                LogFolder = "/logs/";
                ProductVersion = new Version(GetProductVersion());
            }
            catch (Exception ex)
            {
                logger.Error("Init AveEnv failed:{0}", ex.ToString());
            }
        }

        private static string GetProductVersion()
        {
            string strVersion = "1.0.0.0";
            string filePath = Path.Combine(AppDomainRootFolder, "Config/ServiceVersion/ServiceVersion.config");
            logger.Info("ServiceVersion : {0}", filePath);
            try
            {
                FileInfo thisfile = new FileInfo(filePath);
                if (thisfile.Exists)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(filePath);
                    foreach (var node in doc.GetElementsByTagName("version"))
                    {
                        XmlElement xe = (XmlElement)node;
                        strVersion = xe.InnerText;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("get product verison Error:{0}", e.ToString());
            }
            return strVersion;
        }
    }
   
}
