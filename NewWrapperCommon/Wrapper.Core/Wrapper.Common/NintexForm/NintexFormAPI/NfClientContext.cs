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
/* NFClientContext.cs

   Copyright (c) 2014 - Nintex. All Rights Reserved.  
   This code released under the terms of the  
   Microsoft Reciprocal License (MS-RL,  http://opensource.org/licenses/MS-RL.html.)
   
*/

using System;
using System.Net;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace AvePoint.Wrapper.Common
{
    /// <summary>
    /// Manages the client context for the Nintex Forms service endpoints.
    /// </summary>
    /// <remarks>This class manages the bindings, credentials, behaviors, and other information used 
    /// by the WCF service proxy to interact with the Nintex Forms service endpoints.</remarks>
    public class NfClientContext : IDisposable
    {
        // The WCF service proxy.
        private FormsWcfServiceProxy mProxy;

        /// <summary>
        /// The URL for the SharePoint site.
        /// </summary>
        public readonly string WebUrl;

        /// <summary>
        /// The credentials to use when invoking service operations.
        /// </summary>
        public readonly NetworkCredential Credentials;

        /// <summary>
        /// Creates a new instance, specifying the URL, credentials, and version used 
        /// for the SharePoint site.
        /// </summary>
        /// <param name="webUrl">The URL for the SharePoint site.</param>
        /// <param name="credential">The credentials to use when invoking service operations.</param>
        /// <param name="contextKind">The version of the Nintex Forms service endpoint.</param>
        /// <remarks>If this constructor is invoked, a binding and form digest are generated 
        /// and used for requests to the service endpoint.</remarks>
        public NfClientContext(string webUrl, NetworkCredential credential, AveContextKind contextKind)
        {
            // Validate parameters
            if (String.IsNullOrEmpty(webUrl))
            {
                throw new ArgumentNullException("webUrl",
                    "The webUrl parameter cannot be set to a null value or empty string.");
            }
            //if (credential == null)
            //{
            //    throw new ArgumentNullException("credential", 
            //        "The credential parameter cannot be set to a null value.");
            //}

            // Create instance
            mProxy = new FormsWcfServiceProxy(GetBinding(new Uri(webUrl).Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)), new EndpointAddress(webUrl));
            var clientCredentials = mProxy.ClientCredentials;
            if (clientCredentials != null)
            {
                if (credential != null)
                    clientCredentials.Windows.ClientCredential = credential;
                clientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.None;
            }
            mProxy.Endpoint.Behaviors.Add(new FormsHeaderBehavior(FormDigestFactory.Current.CreateFormDigestInstance(contextKind, webUrl, credential).FormDigestValue));
            WebUrl = webUrl;
            Credentials = credential;
        }

        /// <summary>
        /// Creates a new instance, specifying the URL, binding, and form digest used for 
        /// the Nintex Forms service endpoint.
        /// </summary>
        /// <param name="webUrl">The URL for the SharePoint site.</param>
        /// <param name="binding">The binding to use.</param>
        /// <param name="formDigestValue">The form digest value to use.</param>
        /// <remarks>If this constructor is invoked, the specified binding and form digest 
        /// are used for requests to the service endpoint. The credential property is not set.</remarks>
        public NfClientContext(string webUrl, Binding binding, string formDigestValue)
        {
            // Validate parameters
            if (String.IsNullOrEmpty(webUrl))
            {
                throw new ArgumentNullException("webUrl",
                    "The webUrl parameter cannot be set to a null value or empty string.");
            }
            if (binding == null)
            {
                throw new ArgumentNullException("binding",
                    "The binding parameter cannot be set to a null value.");
            }
            if (String.IsNullOrEmpty(formDigestValue))
            {
                throw new ArgumentNullException("formDigestValue",
                    "The formDigestValue parameter cannot be set to a null value or empty string.");
            }

            // Create instance
            mProxy = new FormsWcfServiceProxy(binding, new EndpointAddress(webUrl));
            mProxy.Endpoint.Behaviors.Add(new FormsHeaderBehavior(formDigestValue));
            WebUrl = webUrl;
        }

        /// <summary>
        /// Generates a binding to use for requests to the Nintex Forms service endpoint.
        /// </summary>
        /// <returns>A <see cref="System.ServiceModel.Channels.Binding" /> used for requests to the service endpoint.</returns>
        /// <remarks>The <see cref="System.ServiceModel.Channels.Binding" /> contains encoding and transport binding elements, and sets the timeouts for opening, closing, and sending requests to two hours.</remarks>
        public static Binding GetBinding(bool isHttpsBinding)
        {
            // Create and configure a new WebMessageEncodingBindingElement binding element.
            var webMessageEncodingBindingElement = new WebMessageEncodingBindingElement { MessageVersion = MessageVersion.None };
            webMessageEncodingBindingElement.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;

            // Create and configure a new HttpTransportBindingElement binding element.

            var transport = new HttpTransportBindingElement
            {
                MaxBufferPoolSize = 524288,
                MaxBufferSize = 65536000,
                MaxReceivedMessageSize = 65536000,
                ManualAddressing = true,
                AuthenticationScheme = AuthenticationSchemes.Ntlm
            };

            // https need HttpsTransportBindingElement
            if (isHttpsBinding)
            {
                transport = new HttpsTransportBindingElement
                {
                    MaxBufferPoolSize = 524288,
                    MaxBufferSize = 65536000,
                    MaxReceivedMessageSize = 65536000,
                    ManualAddressing = true,
                    AuthenticationScheme = AuthenticationSchemes.Ntlm
                };
            }
            // Create a new custom binding based on the new binding elements.
            var binding = new CustomBinding(webMessageEncodingBindingElement, transport)
            {
                CloseTimeout = new TimeSpan(2, 0, 0),
                OpenTimeout = new TimeSpan(2, 0, 0),
                SendTimeout = new TimeSpan(2, 0, 0)
            };

            return binding;
        }

        /// <summary>
        /// Retrieves the XML form definition for the specified content type and SharePoint list.
        /// </summary>
        /// <param name="listId">The unique identifier for the SharePoint list.</param>
        /// <param name="contentTypeId">The content type identifier.</param>
        /// <returns>If successful, a string containing the XML form definition for the 
        /// specified content type and SharePoint list; otherwise, an empty string ("").</returns>
        /// <remarks>The method wraps the GetFormXml method of the WCF service proxy, 
        /// for the GetFormXml service endpoint.</remarks>
        public string GetFormXml(string listId, string contentTypeId)
        {
            // Validate parameters
            if (String.IsNullOrEmpty(listId))
            {
                throw new ArgumentNullException("listId", "The listId parameter cannot be set to a null value or empty string.");
            }

            return mProxy.GetFormXml(listId, contentTypeId);
        }

        /// <summary>
        /// Deletes the form for the specified content type and SharePoint list.
        /// </summary>
        /// <param name="listId">The unique identifier for the SharePoint list.</param>
        /// <param name="contentTypeId">The content type identifier.</param>
        /// <returns>If successful, a string containing the default view URL of the 
        /// specified SharePoint list; otherwise, an empty string ("").</returns>
        /// <remarks>The method wraps the DeleteForm method of the WCF service proxy, 
        /// for the DeleteForm service endpoint.</remarks>
        /// list id should format {list id}, example:{d437c72b-6818-4dc3-9ba2-669c4d25cb88}
        public string DeleteForm(string listId, string contentTypeId)
        {
            // Validate parameters
            if (String.IsNullOrEmpty(listId))
            {
                throw new ArgumentNullException("listId", "The listId parameter cannot be set to a null value or empty string.");
            }

            return mProxy.DeleteForm(listId, contentTypeId);
        }

        /// <summary>
        /// Publishes the XML form definition for the specified content type and SharePoint list.
        /// </summary>
        /// <param name="listId">The unique identifier for the SharePoint list.</param>
        /// <param name="contentTypeId">The content type identifier.</param>
        /// <param name="formXml">The XML form definition to be published.</param>
        /// <returns>If successful, a <see cref="NintexFormsClient.FormSaveInfo" /> object 
        /// containing the URL and version for the published form; otherwise, a null reference.</returns>
        /// <remarks>The method wraps the PublishForm method of the WCF service proxy, 
        /// for the PublishFormXml service endpoint.</remarks>
        public FormSaveInfo PublishForm(string listId, string contentTypeId, string formXml)
        {
            // Validate parameters
            if (String.IsNullOrEmpty(listId))
            {
                throw new ArgumentNullException("listId", "The listId parameter cannot be set to a null value or empty string.");
            }
            if (String.IsNullOrEmpty(formXml))
            {
                throw new ArgumentNullException("formXml", "The formXml parameter cannot be set to a null value or empty string.");
            }

            return mProxy.PublishFormXml(listId, contentTypeId, formXml);
        }

        public void Dispose()
        {
            if (mProxy != null)
            {
                mProxy.Close();
            }
        }
    }
}


