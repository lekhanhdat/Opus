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
using System.ServiceModel.Activation;
using System.Configuration;
using System.ServiceModel;
using System.IO;
using System.Threading;
using System.ServiceModel.Description;
using AvePoint.GCommon;

namespace AvePoint.Common
{
    public class CustomizeServiceHostFactory
    {
        public static ServiceHost CreateServiceHost(Type serviceType)
        {
            return CreateServiceHost(serviceType, string.Empty);
        }

        public static ServiceHost CreateServiceHost(Type serviceType, string jobId)
        {
            string relatedBaseUri = ConfigurationManager.AppSettings["relatedBaseUri"];
            if (relatedBaseUri == null)
            {
                relatedBaseUri = string.Empty;
            }
            return CreateServiceHost(serviceType, AveEnv.AgentSchema, AveEnv.AgentAddress, AveEnv.AgentPort, relatedBaseUri, jobId);
        }

        public static ServiceHost CreateServiceHost(Type serviceType, string schema, string address, int port)
        {
            string relatedBaseUri = ConfigurationManager.AppSettings["relatedBaseUri"];
            if (relatedBaseUri == null)
            {
                relatedBaseUri = string.Empty;
            }
            return CreateServiceHost(serviceType, schema, address, port, relatedBaseUri, string.Empty);
        }

        public static ServiceHost CreateServiceHost(Type serviceType, string relatedBaseUri, string jobId)
        {
            return CreateServiceHost(serviceType, AveEnv.AgentSchema, AveEnv.AgentAddress, AveEnv.AgentPort, relatedBaseUri, jobId);
        }

        public static ServiceHost CreateServiceHost(Type serviceType, string schema, string address, int port, string relatedBaseUri, string jobId)
        {
#if DEBUG
            while (File.Exists("C:\\debugServiceHostFactory"))
            {
                Thread.Sleep(2000);
            }
#endif
            List<Uri> baseAddresses = new List<Uri>();
            UriBuilder ub = new UriBuilder();
            ub.Scheme = schema;
            ub.Host = address;
            ub.Port = port;
            if (string.IsNullOrEmpty(jobId))
            {
                ub.Path = relatedBaseUri;
            }
            else
            {
                ub.Path = jobId + "/" + relatedBaseUri;
            }
            baseAddresses.Add(ub.Uri);

            ServiceHost sh = new ServiceHost(serviceType, baseAddresses.ToArray());
            
            //string configurationFile = Path.Combine(AveEnv.AgentBinFolder, AgentConstants.AgentConfigurationFileName.AgentConfigFile_WcfConfigurationsConfig);
            //WCFSharedConfiguration.SetConfigurationFile(configurationFile);
            //WCFSharedConfiguration.LoadSharedConfigureations();

            //foreach (ServiceEndpoint endPoint in sh.Description.Endpoints)
            //{
            //    WCFSharedConfiguration.ApplyEndpointBindingConfigurations(endPoint);
            //}
            //WCFSharedConfiguration.ApplyServiceHostBehaviorConfigurations(sh);

            return sh;
        }
    }
}
