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
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml;

//namespace AvePoint.GCommon.Utility.Cloud
//{
//    public static class UpdateWCFCertThumbprint
//    {
//        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(GCommonRoleConfiguration));
//        public static void SetCertThumbprintsToConfig()
//        {
//            logger.Info("Set cert thumbprints to config file start");
//            string installFolderPath1 = @"F:\approot\";
//            string installFolderPath2 = @"E:\approot\";
//            List<string> configPathList = new List<string>()
//            {
//                "Config/ControlWCFBehaviors.config",//timer timertask apiweb 
//                "AspNetCore/Config/ControlWCFBehaviors.config",//web
//                "AspNetCore/bin/Config/ControlWCFBehaviors.config",//web

//                "MediaWcfBehaviorsConfigurations.config",//agent realtimeagent apiagent
//                "AgentCommonWCFBehaviors.config"//agent realtimeagent apiagent
//            };
//            List<string> combineConfigPathList = new List<string>();
//            configPathList.ForEach(configPath =>
//            {
//                var path = GetConfigPath(installFolderPath1, installFolderPath2, configPath);
//                if (!string.IsNullOrEmpty(path))
//                {
//                    combineConfigPathList.Add(path);
//                }
//            });
//            var thumbprint = GCommonRoleConfiguration.WCF_Certificate.Thumbprint;
//            logger.Info("WCF thumbprint is {0}", thumbprint);
//            if (combineConfigPathList.Count != 0)
//            {
//                UpdateCertThumbprint(combineConfigPathList, thumbprint);
//            }
//            var MediaIocPropertiesConfigurationsPath = GetConfigPath(installFolderPath1, installFolderPath2, "MediaIocPropertiesConfigurations.config");
//            if (!string.IsNullOrEmpty(MediaIocPropertiesConfigurationsPath))
//            {
//                UpdateCertThumbprint(MediaIocPropertiesConfigurationsPath, thumbprint);//agent realtimeagent apiagent
//            }
//            logger.Info("Set cert thumbprints to config file end");
//        }

//        private static void UpdateCertThumbprint(List<string> configFilePathList, string thumbprint)
//        {
//            foreach (var filePath in configFilePathList)
//            {
//                try
//                {
//                    logger.Info("start update path {0} thumbprint {1}", filePath, thumbprint);
//                    XmlDocument xDoc = new XmlDocument();
//                    xDoc.Load(filePath);
//                    XmlNode clientCertificateNode = xDoc.SelectSingleNode(@"/behaviors/endpointBehaviors/behavior/clientCredentials/clientCertificate");
//                    XmlNode serviceCertificateNode = xDoc.SelectSingleNode(@"/behaviors/serviceBehaviors/behavior/serviceCredentials/serviceCertificate");
//                    clientCertificateNode.Attributes["findValue"].Value = thumbprint;
//                    serviceCertificateNode.Attributes["findValue"].Value = thumbprint;
//                    xDoc.Save(filePath);
//                    logger.Info("update thumbprint in config file sucess");
//                }
//                catch (Exception e)
//                {
//                    logger.Error("Update config file failed.path is {0},exception is {1}", filePath, e.ToString());
//                    throw e;
//                }
//            }
//        }

//        private static string GetConfigPath(string installFolderPath1, string installFolderPath2, string filePath)
//        {
//            var path = Path.GetFullPath(Path.Combine(installFolderPath1, filePath));
//            var path2 = Path.GetFullPath(Path.Combine(installFolderPath2, filePath));
//            if (File.Exists(path))
//            {
//                logger.Info(path);
//                return path;
//            }
//            else if (File.Exists(path2))
//            {
//                logger.Info(path2);
//                return path2;
//            }
//            else
//            {
//                logger.Warn("Dont find file {0} in {1} {2}", filePath, installFolderPath1, installFolderPath2);
//                return string.Empty;
//            }
//        }

//        private static void UpdateCertThumbprint(string configPath, string thumbprint)
//        {
//            try
//            {
//                XmlDocument xDoc = new XmlDocument();
//                xDoc.Load(configPath);
//                XmlNode clientCertificateNode = xDoc.SelectSingleNode(@"/configuration/properties/mediaServerSSLThumbprint");
//                clientCertificateNode.InnerText = thumbprint;
//                xDoc.Save(configPath);
//            }
//            catch (Exception e)
//            {
//                logger.Error("Update config file failed.path is {0},exception is {1}", configPath, e.ToString());
//                throw e;
//            }
//        }
//    }
//}
