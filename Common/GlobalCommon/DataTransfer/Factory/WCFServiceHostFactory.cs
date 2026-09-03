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
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Transfer.Data.Service;
using AvePoint.GCommon.Transfer.MQ.Interface;
using AvePoint.GCommon.Transfer.MQ.Service;

namespace AvePoint.GCommon.Transfer.Factory
{
    public class WCFServiceHostFactory
    {
        private static AveLogger mLog = new AveLogger(typeof(WCFServiceHostFactory), false);

        private static List<ServiceHost> mServiceHosts = new List<ServiceHost>();

        public static void Init(string address, int port, string relatedBaseUri, string jobId, WCFServiceHostType type)
        {
            if ((type & WCFServiceHostType.MQ) == WCFServiceHostType.MQ)
            {
                mServiceHosts.Add(CreateServiceHost(typeof(AveMQWCFService), typeof(IMQWCFService), address, port, relatedBaseUri, WCFServiceHostType.MQ.ToString(), jobId));
                mServiceHosts.Add(CreateServiceHost(typeof(AveMQWCFServiceOneWay), typeof(IMQWCFServiceOneWay), address, port, relatedBaseUri, WCFServiceHostType.MQOneWay.ToString(), jobId));
            }

            if ((type & WCFServiceHostType.DataTransfer) == WCFServiceHostType.DataTransfer)
            {
                mServiceHosts.Add(CreateServiceHost(typeof(RelayService), typeof(IRelay), address, port, relatedBaseUri, WCFServiceHostType.DataTransfer.ToString(), jobId));
            }

            if ((type & WCFServiceHostType.FileTransfer) == WCFServiceHostType.FileTransfer)
            {
                mServiceHosts.Add(CreateServiceHost(typeof(FileTransferService), typeof(IFileTransferService), address, port, relatedBaseUri, WCFServiceHostType.FileTransfer.ToString(), jobId));
            }
        }

        public static void StartHosting()
        {
            try
            {
                foreach (ServiceHost sh in mServiceHosts)
                {
                    StringBuilder addresses = new StringBuilder();
                    foreach (ServiceEndpoint endpoint in sh.Description.Endpoints)
                    {
                        addresses.Append(endpoint.Address.Uri.ToString());
                        addresses.Append("\t");
                        addresses.Append(WCFSharedConfiguration.BindingConfigurationToString(endpoint.Binding));
                        addresses.Append("\t");
                    }
                    try
                    {
                        mLog.Info(string.Format("Begin to host service: {0}  Address: {1}", sh.Description.ServiceType.ToString(), addresses.ToString()));
                        sh.Open();
                        mLog.Info(string.Format("Successfully host service: {0} Address: {1}", sh.Description.ServiceType.ToString(), addresses.ToString()));
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = string.Format("Exception occurs when start hosting {0}. Exception details: {1}", sh.Description.ServiceType.ToString(), ex.ToString(), addresses.ToString());
                        //throw exception with error code, the error information is printted  outside.
                        mLog.Error(errorMsg);
                        throw;
                    }

                }
            }
            catch (Exception ex)
            {
                mLog.Error(string.Format("An error occurred while doing service hosting. Exception: {0}", ex.ToString()));
            }
        }

        public static void StopHosting()
        {
            foreach (ServiceHost sh in mServiceHosts)
            {
                try
                {
                    if (sh.State != CommunicationState.Closed)
                    {
                        mLog.Info(string.Format("Begin to close service: {0}", sh.Description.ServiceType.ToString()));
                        sh.Abort();
                        mLog.Info(string.Format("Successfully close service: {0}", sh.Description.ServiceType.ToString()));
                    }
                    else
                    {
                        mLog.Info(string.Format("Service [{0}] is already in Closed state.", sh.Description.ServiceType.ToString()));
                    }
                }
                catch (Exception ex)
                {
                    string errorMsg = string.Format("Exception occurs when stop hosting {0}. Exception details: {1}", sh.Description.ServiceType.ToString(), ex.ToString());
                    mLog.Error(errorMsg);
                }
            }
        }

        public static ServiceHost CreateServiceHost(Type serviceType, Type interfaceType, string address, int port, string relatedBaseUri, string serviceName, string jobId)
        {
            return CreateServiceHost(serviceType, interfaceType, DataTransferConfiguration.UriSchema, address, port, relatedBaseUri, serviceName, jobId);
        }

        public static ServiceHost CreateServiceHost(Type serviceType, Type interfaceType, string schema, string address, int port, string relatedBaseUri, string serviceName, string jobId)
        {
            List<Uri> baseAddresses = new List<Uri>();

            baseAddresses.Add(UriUtility.CreateUri(schema, address, port, relatedBaseUri, serviceName, jobId));

            ServiceHost sh = new ServiceHost(serviceType, baseAddresses.ToArray());

            if (sh.Description.Endpoints.Count == 0)
            {
                //NetTcpBinding binding = new NetTcpBinding();
                //binding.Security.Mode = SecurityMode.None;
                //binding.PortSharingEnabled = true;
                //sh.AddServiceEndpoint(interfaceType, binding, string.Empty);
                sh.AddServiceEndpoint(interfaceType, DataTransferConfiguration.DefaultDataBinding, string.Empty);
                ////********************************************
                ////wdz 临时增加，由于没有配置文件，而默认的值在传输数据的时候小于64k，所以暂时用代码配置，否则传输数据会异常。
                //binding.TransferMode = TransferMode.Buffered;
                //binding.MaxBufferSize = 536870912;
                //binding.MaxReceivedMessageSize = 536870912;
                //binding.ReaderQuotas.MaxStringContentLength = 536870912;
                //binding.ReaderQuotas.MaxArrayLength = 536870912;
                ////********************************************
            }
            //else
            //{
            //    if (!string.IsNullOrEmpty(jobId))
            //    {
            //        foreach (var endpoint in sh.Description.Endpoints)
            //        {
            //            endpoint.Address = new EndpointAddress(UriUtility.CreateUri(schema, address, port, endpoint.Address.Uri.AbsolutePath, jobId));
            //        }
            //    }
            //}

            return sh;
        }
    }

    /// <summary>
    /// 初始化ServiceHost的几个类型
    /// 不能使用int的最高bit位。
    /// </summary>
    [Flags]
    public enum WCFServiceHostType
    {
        DataTransfer = 0x01,
        MQ = 0x02,
        MQOneWay = 0x04,
        FileTransfer,
        ALL = 0x7FFFFFFF,
    }
}
