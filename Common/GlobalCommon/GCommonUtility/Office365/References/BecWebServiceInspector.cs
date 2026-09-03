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
namespace AvePoint.Common.Office365
{
    using Microsoft.Online.Administration.Automation;
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using Version = Microsoft.Online.Administration.Version;

    internal class BecWebServiceInspector : IClientMessageInspector
    {
        private String token;

        public BecWebServiceInspector(String token)
        {
            this.token = token;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState) { }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var header = new UserIdentityHeader {LiveToken = token};
            var header2 = MessageHeader.CreateHeader("UserIdentityHeader", "http://provisioning.microsoftonline.com/", header);
            request.Headers.Add(header2);

            var header3 = new ClientVersionHeader
            {
                ClientId = new Guid("{50AFCE61-C917-435b-8C6D-60AA5A8B8AA7}"),
                Version = "1.1.0.0"
            };
            request.Headers.Add(MessageHeader.CreateHeader("ClientVersionHeader", "http://provisioning.microsoftonline.com/", header3));

            var contractVersionHeader = new ContractVersionHeader {BecVersion = Version.Version16};
            request.Headers.Add(MessageHeader.CreateHeader("ContractVersionHeader", "http://provisioning.microsoftonline.com/", contractVersionHeader));
            request.Headers.Add(MessageHeader.CreateHeader("TrackingHeader", "http://becwebservice.microsoftonline.com/", Guid.NewGuid()));
            return null;
        }
    }
}
