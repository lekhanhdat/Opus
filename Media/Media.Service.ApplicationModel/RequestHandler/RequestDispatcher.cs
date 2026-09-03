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




namespace AvePoint.Media.Service
{
    using AvePoint.Common;
    #region using directives
    using AvePoint.GCommon.Contract.Media.TCPRequest;
    using AvePoint.GCommon.Network;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Common;
    using System;
    #endregion

    public class RequestDispatcher
        : IAveNetworkEvent
    {
        public IRequestHandlerFactory HandlerFactory { get; set; }

        /// <summary>
        /// As a matter of fact, Media developers should debug this method as the entry point. 
        /// </summary>
        /// <param name="network">underlying connection</param>
        public void AveNetworkAccepted(IAveNetwork network)
        {
            var message = network.ReceiveMessage();
            var request = (MediaTCPRequest)MediaTCPRequestSerializerHelper.DeSerialize(message);
            JobIdManager.JobId = request.JobId;
            IdentityManager.IdentityType = ServiceConstants.IdentityTypeJobId;
            IdentityManager.IdentityContent = request.JobId != null ? request.JobId.Split('_')[0] : null;
            var requestHandler = this.HandlerFactory.GetHandler(request.GetType().FullName);
            requestHandler.HandleRequest(request, network);
            this.HandlerFactory.ReleaseHandler(requestHandler);
        }
    }
}
