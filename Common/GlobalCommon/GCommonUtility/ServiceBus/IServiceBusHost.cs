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
namespace AvePoint.GCommon.Utility.ServiceBus
{
    using AvePoint.GCommon.Contract.AvePointService;
    using AvePoint.GCommon.Utility.AvePointService;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Public to each module to start the service bus message receiver host
    /// </summary>
    public interface IServiceBusHost
    {
        /// <summary>
        /// Prepare to receive message from service bus
        /// </summary>
        void StartHost();

        /// <summary>
        /// Stop all working thread when all working thread is complete
        /// </summary>
        void SafeStopHost();
    }

    /// <summary>
    /// Get specified service bus host
    /// </summary>
    public class ServiceBusReceiverFactory
    {
        /// <summary>
        /// Initial queue or relay instance based on parameter relayType
        /// </summary>
        /// <param name="connectionDto">Need call "SbConnectionDtoUtility.GetConnectionDto" method to get</param>
        /// <param name="threadMaxCount">Only used in queue service</param>
        /// <param name="relayType">Which type need to use</param>
        /// <param name="asyncFunc">Used for queue and Relay async event</param>
        /// <param name="syncFunc">Used for relay sync event</param>
        /// <returns>IServiceBusHost instance</returns>
        public static IServiceBusHost CreateServiceBusEventReceiver(int threadMaxCount, RelayType relayType, string dataCenter, Action<AresRemoteEventProperty> asyncFunc, Func<AresRemoteEventProperty, AresRemoteEventResultDto> syncFunc)
        {
            if (relayType == RelayType.Queue)
            {
                return new QueueServiceHost(threadMaxCount, dataCenter, asyncFunc);
            }
            else
            {
                return new RelayServiceHost(asyncFunc, syncFunc);
            }
        }
    }
}
