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
using System.ServiceModel.Channels;
using System.ServiceModel;
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Transfer.Data.Channel;
using AvePoint.GCommon.Transfer.Common;

namespace AvePoint.GCommon.Transfer.Factory
{
    internal class DataChannelFactory
    {
        public static ITransferChannel GetWcfDataChannel(TransferCommunicationSettings communicationSettings)
        {
            if (communicationSettings.Mode == TransferConfigurationLoadMode.Manual)
            {
                if(communicationSettings.UriSchema.Equals("https", StringComparison.OrdinalIgnoreCase))
                {
                    if (!communicationSettings.EnableSsl)
                    {
                        communicationSettings.UriSchema = "http";
                    }
                    communicationSettings.ConfigurationName = DataTransferConstants.HttpTransferEndPointConfigurationName;
                }
                else if(communicationSettings.UriSchema.Equals("http", StringComparison.OrdinalIgnoreCase))
                {
                    if (communicationSettings.EnableSsl)
                    {
                        communicationSettings.UriSchema = "https";
                    }
                    communicationSettings.ConfigurationName = DataTransferConstants.HttpTransferEndPointConfigurationName;
                }
                else if(communicationSettings.UriSchema.Equals("net.tcp", StringComparison.OrdinalIgnoreCase))
                {
                    communicationSettings.ConfigurationName = DataTransferConstants.TransferEndPointConfigurationName;
                }

                return new WCFDataTransferChannel(new WcfChannelFactory<IRelay>(communicationSettings, 
                    WCFServiceHostType.DataTransfer.ToString())); 
            }
            
            return new WCFDataTransferChannel(new WcfChannelFactory<IRelay>(communicationSettings, 
                WCFServiceHostType.DataTransfer.ToString())); 
        }

        //public static ITransferChannel GetWCFDataChannel(string agentAddress, int port, string relatedBaseUri, string jobId)
        //{
        //    return new WCFDataTransferChannel(new WCFChannelFactory<IRelay>(agentAddress, port, relatedBaseUri, WCFServiceHostType.DataTransfer.ToString(), jobId)); 
        //}

        //public static ITransferChannel GetWCFDataChannel(Binding binding, EndpointAddress endpointAddress)
        //{
        //    return new WCFDataTransferChannel(new WCFChannelFactory<IRelay>(binding, endpointAddress));
        //}

        //public static ITransferChannel GetWCFDataChannel(string serviceConfigurationName)
        //{
        //    return new WCFDataTransferChannel(new WCFChannelFactory<IRelay>(serviceConfigurationName));
        //}
        ///// <summary>
        ///// 构造文件传输服务的WCF处理管道
        ///// </summary>
        ///// <param name="agentAddress"></param>
        ///// <param name="port"></param>
        ///// <param name="relatedBaseUri"></param>
        ///// <param name="jobId"></param>
        ///// <returns></returns>
        //public static ITransferChannel GetWCFFileTransferChannel(string agentAddress, int port, string relatedBaseUri, string jobId)
        //{
        //    return new WCFFileTransferChannel(new WCFChannelFactory<IFileTransferService>(agentAddress, port, relatedBaseUri, WCFServiceHostType.FileTransfer.ToString(), jobId)); 
        //}

        public static ITransferChannel GetWcfFileTransferChannel(TransferCommunicationSettings communicationSettings)
        {
            if (communicationSettings.Mode == TransferConfigurationLoadMode.Manual)
            {
                communicationSettings.UriSchema = "net.tcp";
                communicationSettings.ConfigurationName = DataTransferConstants.FileTransferEndPointConfigurationName;
            }

            return new WCFFileTransferChannel(new WcfChannelFactory<IFileTransferService>(communicationSettings, 
                WCFServiceHostType.FileTransfer.ToString())); 
        }

        /// <summary>
        /// 创建进程内处理管道，
        /// 进程内管道主要是提供客户端和服务在同一个进程中的时候进行快速数据的传输，
        /// 而不必进行WCF调用。
        /// </summary>
        /// <returns>返回提供进程内数据支持的传输通道</returns>
        public static ITransferChannel GetInProcessDataChannel()
        {
            return new InProcessChannel();
        }
        /// <summary>
        /// 提供文件系统的传输通道
        /// 当上层数据写入和读去的媒体对应文件系统的时候，提供服务
        /// </summary>
        /// <returns>提供文件系统支持的传输通道</returns>
        public static ITransferChannel GetFileSystemDataChannel()
        {
            return new FileSystemDataChannel();
        }

        /// <summary>
        /// WCF http mode cannel
        /// </summary>
        /// <param name="communicationSettings"></param>
        /// <returns></returns>
        public static ITransferChannel GetStreamModeWcfDataChannel(TransferCommunicationSettings communicationSettings)
        {
            if (communicationSettings.Mode == TransferConfigurationLoadMode.Manual)
            {
                if (communicationSettings.UriSchema.Equals("https", StringComparison.OrdinalIgnoreCase))
                {
                    if (!communicationSettings.EnableSsl)
                    {
                        communicationSettings.UriSchema = "http";
                    }
                    communicationSettings.ConfigurationName = DataTransferConstants.HttpStreamTransferEndPointConfigurationname;
                }
                else if (communicationSettings.UriSchema.Equals("http", StringComparison.OrdinalIgnoreCase))
                {
                    if (communicationSettings.EnableSsl)
                    {
                        communicationSettings.UriSchema = "https";
                    }
                    communicationSettings.ConfigurationName = DataTransferConstants.HttpStreamTransferEndPointConfigurationname;
                }
                else if (communicationSettings.UriSchema.Equals("net.tcp", StringComparison.OrdinalIgnoreCase))
                {
                    communicationSettings.ConfigurationName = DataTransferConstants.StreamTransferEndPointConfigurationName;
                }
            }

            return new WCFHttpModeChannel(new WcfChannelFactory<IStreamRelay>(communicationSettings, 
                WCFServiceHostType.DataTransferStreaming.ToString())); 
        }

        /// <summary>
        /// http mode process level channel
        /// </summary>
        /// <returns></returns>
        public static ITransferChannel GetInProcessHttpModeChannel()
        {
            return new HttpModeInProcessChannel();
        }


        /// <summary>
        /// 根据DataTransferSetting来创建不同的Channel
        /// </summary>
        /// <param name="dataTransferSetting"></param>
        /// <returns></returns>
        public static ITransferChannel GetTransferChannel(DataTransferSetting dataTransferSetting)
        {
            ITransferChannel channel = null;
            switch (dataTransferSetting.TransferChannelMode)
            {
                case TransferChannelMode.WCFIRelay:
                    if (dataTransferSetting.CommunicationSettings.IsStreamMode)
                    {
                        channel = DataChannelFactory.GetStreamModeWcfDataChannel(dataTransferSetting.CommunicationSettings);
                    }
                    else
                    {
                        channel = DataChannelFactory.GetWcfDataChannel(dataTransferSetting.CommunicationSettings);
                    }
                    break;
                case TransferChannelMode.WCFIFileTransfer:
                    channel = DataChannelFactory.GetWcfFileTransferChannel(dataTransferSetting.CommunicationSettings);
                    break;
                case TransferChannelMode.InProcess:
                    if (dataTransferSetting.CommunicationSettings.IsStreamMode)
                    {
                        channel = DataChannelFactory.GetInProcessHttpModeChannel();
                        //channel = DataChannelFactory.GetInProcessDataChannel();
                    }
                    else
                    {
                        channel = DataChannelFactory.GetInProcessDataChannel();
                    }
                    break;
                case TransferChannelMode.FileSystem:
                    channel = DataChannelFactory.GetFileSystemDataChannel();
                    break;
            }

            return channel;
        }
    }
}
