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




namespace AvePoint.GCommon.Contract
{
    #region using directives
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public class CoreServiceEndpointInfo
    {
        /// <summary>
        /// This property must be set
        /// </summary>
        [DataMember]
        public String HostOrIpAddress { get; set; }

        /// <summary>
        /// This property must be set
        /// </summary>
        [DataMember]
        public Int32 Port { get; set; }

        [DataMember]
        public String Scheme { get; set; }

        /// <summary>
        /// This property is to use as a specified IOC container key
        /// </summary>
        [DataMember]
        public String RemotingTypeKey { get; set; }
        /// <summary>
        /// This property must be set
        /// </summary>
        [DataMember]
        public String EndpointConfigurationName { get; set; }

        public override string ToString()
        {
            return String.Format("{0}://{1}:{2}",Scheme,HostOrIpAddress,Port);
        }
    }
}
