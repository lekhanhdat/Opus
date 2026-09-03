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



namespace AvePoint.Hybrid.AgentService
{
    using AvePoint.GCommon;
    using AvePoint.RA.CommonUtil;
    using Castle.Windsor;
    using Castle.Windsor.Configuration.Interpreters;
    using Castle.Windsor.Installer;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    internal sealed class AgentIocComponentManager
    {
        AvePoint.GCommon.AveLogger logger = new AvePoint.GCommon.AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string applicationPath = string.Empty;
        private readonly IWindsorContainer windsorContainer;

        internal AgentIocComponentManager(IWindsorContainer windsorContainer)
        {
            this.windsorContainer = windsorContainer;
            applicationPath = Path.Combine(AveEnv.AgentBinFolder, "app");
        }

        internal void LoadComponents()
        {
            var allFiles = GetAllConfigFiles(new DirectoryInfo(applicationPath));
            allFiles.Insert(0, Path.Combine(AveEnv.AgentBinFolder, "CloudAgentService.exe.config"));
            logger.Info("agent service path : " + Path.Combine("CloudAgentService.exe.config"));
            allFiles.ForEach(AddComponentsByFile);
        }

        private void AddComponentsByFile(string configFile)
        {
            try
            {
                var configResource = new AgentIocConfigurationResource(configFile);
                if (configResource.ContainsCastleSection())
                {
                    var installer = new ConfigurationInstaller(
                        new XmlInterpreter(configResource));

                    windsorContainer.Install(installer);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while loading the ioc config file, Exception: {0}", ex.ToString());
            }
        }

        private List<string> GetAllConfigFiles(DirectoryInfo directoryInfo)
        {
            var files = new List<string>();
            //if (directoryInfo.Exists)
            //{
            //    Array.ForEach(directoryInfo.GetFiles(), f => files.Add(f.FullName));
            //    Array.ForEach(directoryInfo.GetDirectories(), d => files.AddRange(GetAllConfigFiles(d)));
            //}
            return files;
        }
    }
}
