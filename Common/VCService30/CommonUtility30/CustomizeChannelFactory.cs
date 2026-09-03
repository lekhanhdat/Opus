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




namespace AvePoint.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Utility;
    #endregion

    public class CustomizeChannelFactory<T>
    {
        static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static T CreateManagerChannel(string endpointConfigurationName)
        {
            return CreateChannel(endpointConfigurationName, AveEnv.ManagerSchema, AveEnv.ManagerAddress, AveEnv.ManagerPort);
        }

        public static T CreateManagerChannel()
        {
            return CreateManagerChannel("managerAgentControlService");
        }

        public static T CreateLocalChannel(String endpointConfigurationName)
        {
            return CreateLocalChannel(endpointConfigurationName, String.Empty);
        }

        public static T CreateLocalChannel(String endpointConfigurationName, String jobId)
        {
            return CreateChannel(
                endpointConfigurationName,
                AveEnv.AgentSchema,
                AveEnv.AgentAddress,
                AveEnv.AgentPort,
                jobId);
        }

        public static T CreateChannel(
            String endpointConfigurationName,
            String schema,
            String host,
            Int32 port)
        {
            return CreateChannel(endpointConfigurationName, schema, host, port, string.Empty);
        }

        public static T CreateChannel(
            String endpointConfigurationName,
            String schema,
            String host,
            int port,
            String jobId)
        {
            return CreateChannelInDetails(endpointConfigurationName, schema, host, port, jobId).Item1;
        }

        public static System.Tuple<T, IDefaultCommunicationTimeouts> CreateChannelInDetails(
           String endpointConfigurationName,
           String schema,
           String host,
           Int32 port,
           String jobId)
        {
            PermissiveCertificatePolicy.Enact();
            var channelFactory = new ChannelFactory<T>(endpointConfigurationName);
            channelFactory.Credentials.ClientCertificate.Certificate = AvePoint.GCommon.Utility.Cloud.GCommonRoleConfiguration.WCF_Certificate;
            channelFactory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.Custom;
            channelFactory.Credentials.ServiceCertificate.Authentication.CustomCertificateValidator = new CustomX509CertificateValidator();

            UriBuilder uriBuilder;
            if (String.IsNullOrEmpty(jobId))
                uriBuilder = new UriBuilder(schema, host, port, channelFactory.Endpoint.Address.Uri.AbsolutePath);
            else uriBuilder = new UriBuilder(schema, host, port, "/" + jobId + channelFactory.Endpoint.Address.Uri.AbsolutePath);

            channelFactory.Endpoint.Address = new EndpointAddress(uriBuilder.Uri.ToString());
            CustomIdentityVerifer.Plug(channelFactory);

            logger.Info(String.Format("Channel factory created. Endpoint address: {0}", uriBuilder.Uri));
            return new System.Tuple<T, IDefaultCommunicationTimeouts>(channelFactory.CreateChannel(), channelFactory.Endpoint.Binding);
        }

        public static T CreateChannel(String endpointConfigurationName, Uri uri)
        {
            PermissiveCertificatePolicy.Enact();
            ChannelFactory<T> channelFactory = new ChannelFactory<T>(endpointConfigurationName);
            channelFactory.Endpoint.Address = new EndpointAddress(uri);
            CustomIdentityVerifer.Plug(channelFactory);
            logger.Info(string.Format("Channel factory created. Endpoint address: {0}", uri.ToString()));
            return channelFactory.CreateChannel();
        }

        #region Duplex Channel Factory

        public static T CreateDuplexChannel(object callImplementation, string endpointConfigurationName, Uri uri)
        {
            InstanceContext context = new InstanceContext(callImplementation);
            return CreateDuplexChannel(context, endpointConfigurationName, uri);
        }

        public static T CreateDuplexChannel(
            InstanceContext context,
            string endpointConfigurationName,
            string schema, string host, int port, string jobId)
        {
            PermissiveCertificatePolicy.Enact();

            DuplexChannelFactory<T> channelFactory = new DuplexChannelFactory<T>(context, endpointConfigurationName);

            UriBuilder uriBuilder;
            if (string.IsNullOrEmpty(jobId))
            {
                uriBuilder = new UriBuilder(schema, host, port, channelFactory.Endpoint.Address.Uri.AbsolutePath);
            }
            else
            {
                uriBuilder = new UriBuilder(schema, host, port, "/" + jobId + channelFactory.Endpoint.Address.Uri.AbsolutePath);
            }

            channelFactory.Endpoint.Address = new EndpointAddress(uriBuilder.Uri.ToString());
            CustomIdentityVerifer.Plug(channelFactory);
            logger.Info(string.Format("Channel factory created. Endpoint address: {0}", uriBuilder.Uri.ToString()));
            return channelFactory.CreateChannel();
        }

        public static T CreateDuplexChannel(InstanceContext context, string endpointConfigurationName, Uri uri)
        {
            PermissiveCertificatePolicy.Enact();
            DuplexChannelFactory<T> channelFactory = new DuplexChannelFactory<T>(context, endpointConfigurationName);
            channelFactory.Endpoint.Address = new EndpointAddress(uri);
            CustomIdentityVerifer.Plug(channelFactory);
            channelFactory.Faulted += delegate
            {
                logger.Error("The channel factory {0} has faulted", uri.ToString());
            };
            logger.Info(string.Format("Channel factory created. Endpoint address: {0}", uri));
            return channelFactory.CreateChannel();
        }

        #endregion
    }
}