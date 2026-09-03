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
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Project.Server.Library;
using Microsoft365.Authentication;
using System.Net;
using System.Xml;
using AvePoint.GCommon;

namespace AvePoint.ObjectModel.PSI
{
    internal class AvePSIClient<TServiceContract> : ClientBase<TServiceContract>
        where TServiceContract : class
    {

        public TServiceContract ServiceChannel
        {
            get
            {
                return Channel;
            }
        }
         
        #region Constructors
        public AvePSIClient(Uri serviceUri, String authCookie)
            : base(
            new ServiceEndpoint(ContractDescription.GetContract(typeof(TServiceContract)))
            {
                Binding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport),
                Address = new EndpointAddress(new Uri(serviceUri, "_vti_bin/psi/projectserver.svc"))
            })
        {
            (base.Endpoint.Binding as BasicHttpsBinding).MaxReceivedMessageSize = Int32.MaxValue;
            (base.Endpoint.Binding as BasicHttpsBinding).MaxBufferSize = Int32.MaxValue;
            base.Endpoint.EndpointBehaviors.Add(new CookieBehavior(authCookie));
        }

        public AvePSIClient(string siteUrl, ITokenProvider tokenProvider)
            : base(
            new ServiceEndpoint(ContractDescription.GetContract(typeof(TServiceContract)))
            {
                Binding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport),
                Address = new EndpointAddress(new Uri(siteUrl.TrimEnd('/') + "/_vti_bin/psi/projectserver.svc"))
            })
        {
            (base.Endpoint.Binding as BasicHttpsBinding).MaxReceivedMessageSize = Int32.MaxValue;
            (base.Endpoint.Binding as BasicHttpsBinding).MaxBufferSize = Int32.MaxValue;
            base.Endpoint.EndpointBehaviors.Add(new CookieBehavior(tokenProvider, new Uri(siteUrl)));
        }

        #endregion Constructors
    }

    public class CookieBehavior : IEndpointBehavior
    {
        private string cookie;
        private ITokenProvider mTokenProvider;
        private Uri mSiteUrl;

        public CookieBehavior(string cookie)
        {
            this.cookie = cookie;
        }

        public CookieBehavior(ITokenProvider tokenProvider, Uri siteUrl)
        {
            mTokenProvider = tokenProvider;
            mSiteUrl = siteUrl;
        }

        public void AddBindingParameters(ServiceEndpoint serviceEndpoint,
            BindingParameterCollection bindingParameters)
        { }

        public void ApplyClientBehavior(ServiceEndpoint serviceEndpoint,
            System.ServiceModel.Dispatcher.ClientRuntime behavior)
        {
            behavior.ClientMessageInspectors.Add(new CookieMessageInspector(mTokenProvider, mSiteUrl));
        }

        public void ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint,
            System.ServiceModel.Dispatcher
            .EndpointDispatcher endpointDispatcher)
        { }

        public void Validate(ServiceEndpoint serviceEndpoint) { }
    }

    public class CookieMessageInspector : IClientMessageInspector
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(CookieMessageInspector));
        private string cookie;
        private ITokenProvider mTokenProvider;
        private Uri mSiteUrl;

        public CookieMessageInspector(string cookie)
        {
            this.cookie = cookie;
        }

        public CookieMessageInspector(ITokenProvider tokenProvider, Uri siteUrl)
        {
            mSiteUrl = siteUrl;
            mTokenProvider = tokenProvider;
        }

        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            if (reply.IsFault)
            {
                MessageBuffer buffer = reply.CreateBufferedCopy(int.MaxValue);
                reply = buffer.CreateMessage(); //这里需要做一份copy，不然外围截获异常会提示：This message cannot support the operation because it has been read
                MessageFault fault = MessageFault.CreateFault(buffer.CreateMessage(), int.MaxValue);
                FaultException fe = FaultException.CreateFault(fault);
                string error;
                GetPSClientError(fe, out error);
                if (!string.IsNullOrEmpty(error))
                {
                    mLogger.Warn("fault exception happened, error message:{0}", error);
                }
            }
        }

        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request,
            System.ServiceModel.IClientChannel channel)
        {
            HttpRequestMessageProperty httpRequestMessage;
            object httpRequestMessageObject;
            if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name
                , out httpRequestMessageObject))
            {
                httpRequestMessage = httpRequestMessageObject
                    as HttpRequestMessageProperty;
                if (string.IsNullOrEmpty(httpRequestMessage.Headers["Cookie"]))
                {
                    //httpRequestMessage.Headers["Cookie"] = cookie;
                    if (mTokenProvider.TokenType == TokenType.Bearer)
                    {
                        httpRequestMessage.Headers[HttpRequestHeader.Authorization] = mTokenProvider.GetToken(mSiteUrl);
                    }
                    else if (mTokenProvider.TokenType == TokenType.IDCLR)
                    {
                        httpRequestMessage.Headers[HttpRequestHeader.Cookie] = mTokenProvider.GetToken(mSiteUrl);
                    }
                }
            }
            else
            {
                httpRequestMessage = new HttpRequestMessageProperty();
                //httpRequestMessage.Headers.Add("Cookie", cookie);
                if (mTokenProvider.TokenType == TokenType.Bearer)
                {
                    httpRequestMessage.Headers[HttpRequestHeader.Authorization] = mTokenProvider.GetToken(mSiteUrl);
                }
                else if (mTokenProvider.TokenType == TokenType.IDCLR)
                {
                    httpRequestMessage.Headers[HttpRequestHeader.Cookie] = mTokenProvider.GetToken(mSiteUrl);
                }
                request.Properties.Add(HttpRequestMessageProperty.Name
                    , httpRequestMessage);
            }

            return null;
        }

        private PSClientError GetPSClientError(FaultException e, out string errOut)
        {
            string PREFIX = e.Message;
            errOut = string.Empty;
            PSClientError psClientError = null;

            //if (e == null)
            //{
            //    errOut = PREFIX + "Null parameter (FaultException e) passed in.";
            //    psClientError = null;
            //}
            //else
            {
                var messageFault = e.CreateMessageFault();

                if (messageFault.HasDetail)
                {
                    using (var xmlReader = messageFault.GetReaderAtDetailContents())
                    {
                        var xml = new XmlDocument();
                        xml.Load(xmlReader);
                        var serverExecutionFault = xml["ServerExecutionFault"];
                        if (serverExecutionFault != null)
                        {
                            var exceptionDetails = serverExecutionFault["ExceptionDetails"];
                            if (exceptionDetails != null)
                            {
                                try
                                {
                                    errOut = exceptionDetails.InnerXml + "\r\n";
                                    psClientError = new PSClientError(exceptionDetails.InnerXml);
                                }
                                catch (InvalidOperationException ex)
                                {
                                    errOut = PREFIX + "Unable to convert fault exception info ";
                                    errOut += "a valid Project Server error message. Message: \n\t";
                                    errOut += ex.Message;
                                    psClientError = null;
                                }
                            }
                            else
                            {
                                errOut = PREFIX
                                    + "The FaultException e is a ServerExecutionFault, "
                                    + "but does not have ExceptionDetails.";
                            }
                        }
                        else
                        {
                            errOut = xml.InnerText;
                        }
                    }
                }
                else // No detail in the MessageFault.
                {
                    errOut = PREFIX + "The FaultException e does not have any detail.";
                }
            }
            errOut += "\r\n" + e.ToString() + "\r\n";
            return psClientError;
        }
    }
}
