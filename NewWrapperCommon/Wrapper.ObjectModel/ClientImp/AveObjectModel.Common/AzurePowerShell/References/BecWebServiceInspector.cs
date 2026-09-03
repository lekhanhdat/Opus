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

using Microsoft.Online.Administration.Automation;
using System;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace AvePoint.Wrapper.Common
{
    public class BecWebServiceInspector : IClientMessageInspector
    {
        private string token;

        public BecWebServiceInspector(string token)
        {
            this.token = token;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState) { }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "becwebservice is part of request header.")]
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            UserIdentityHeader header = new UserIdentityHeader
            {
                LiveToken = token,
            };
            MessageHeader header2 = MessageHeader.CreateHeader("UserIdentityHeader", "http://provisioning.microsoftonline.com/", header);
            request.Headers.Add(header2);            	

            ClientVersionHeader header3 = new ClientVersionHeader
            {
                ClientId = new Guid("{50AFCE61-C917-435b-8C6D-60AA5A8B8AA7}"),
                Version = "1.1.0.0"
            };
            request.Headers.Add(MessageHeader.CreateHeader("ClientVersionHeader", "http://provisioning.microsoftonline.com/", header3));

            ContractVersionHeader contractVersionHeader = new ContractVersionHeader();
            contractVersionHeader.BecVersion = Microsoft.Online.Administration.Version.Version16;
            request.Headers.Add(MessageHeader.CreateHeader("ContractVersionHeader", "http://provisioning.microsoftonline.com/", contractVersionHeader));

            request.Headers.Add(MessageHeader.CreateHeader("TrackingHeader", "http://becwebservice.microsoftonline.com/", Guid.NewGuid()));
            return null;
        }
    }
}
