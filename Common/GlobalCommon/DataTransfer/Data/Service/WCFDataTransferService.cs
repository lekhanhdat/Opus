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




using System.ServiceModel;
using AvePoint.GCommon.Transfer.Data.Interface;
using AvePoint.GCommon.Transfer.Factory;

namespace AvePoint.GCommon.Transfer.Data.Service
{
    /// <summary>
    /// 封装WCF服务的构建启动和关闭的细节
    /// 提供给上层使用的工具类。
    /// </summary>
    public class WCFDataTransferService
    {
        private ServiceHost mRelayServiceHost;

        public WCFDataTransferService(string AgentAddress, int port, string relatedBaseUri, string jobId)
        {
            mRelayServiceHost = WCFServiceHostFactory.CreateServiceHost(typeof(RelayService), typeof(IRelay), AgentAddress, port, relatedBaseUri, WCFServiceHostType.DataTransfer.ToString(), jobId);
        }

        #region IDataTransferService Members

        public void Open()
        {
            if (mRelayServiceHost.State == CommunicationState.Created)
            {
                mRelayServiceHost.Open();
            }
        }

        public void Close()
        {
            if (mRelayServiceHost.State != CommunicationState.Closed)
            {
                mRelayServiceHost.Close();
            }
        }

        #endregion
    }
}
