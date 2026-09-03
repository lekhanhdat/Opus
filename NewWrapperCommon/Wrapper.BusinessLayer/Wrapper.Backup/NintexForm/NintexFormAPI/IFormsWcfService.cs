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
/* FormsWcfServiceProxy.cs

   Copyright (c) 2014 - Nintex. All Rights Reserved.  
   This code released under the terms of the  
   Microsoft Reciprocal License (MS-RL,  http://opensource.org/licenses/MS-RL.html.)
   
*/

using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;

namespace AvePoint.Wrapper.Backup
{
    /// <summary>
    /// Describes the Nintex Forms service operations.
    /// </summary>
    [ServiceContract]
    public interface IFormsWcfService
    {
        /// <summary>
        /// Retrieves the XML form definition for the specified content type and SharePoint list.
        /// </summary>
        /// <param name="listId">The unique identifier for the SharePoint list.</param>
        /// <param name="contentTypeId">The content type identifier.</param>
        /// <returns>If successful, a string containing the XML form definition for the 
        /// specified content type and SharePoint list; otherwise, an empty string ("").</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, UriTemplate = "/_vti_bin/NintexFormsServices/NfRestService.svc/GetFormXml", ResponseFormat = WebMessageFormat.Xml)]
        string GetFormXml(string listId, string contentTypeId);

        /// <summary>
        /// Deletes the form for the specified content type and SharePoint list.
        /// </summary>
        /// <param name="listId">The unique identifier for the SharePoint list.</param>
        /// <param name="contentTypeId">The content type identifier.</param>
        /// <returns>If successful, a string containing the default view URL of the 
        /// specified SharePoint list; otherwise, an empty string ("").</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, UriTemplate = "/_vti_bin/NintexFormsServices/NfRestService.svc/DeleteForm", ResponseFormat = WebMessageFormat.Json)]
        string DeleteForm(string listId, string contentTypeId);

        /// <summary>
        /// Publishes the XML form definition for the specified content type and SharePoint list.
        /// </summary>
        /// <param name="listId">The unique identifier for the SharePoint list.</param>
        /// <param name="contentTypeId">The content type identifier.</param>
        /// <param name="formXml">The XML form definition to be published.</param>
        /// <returns>If successful, a <see cref="NintexFormsClient.FormSaveInfo" /> object 
        /// containing the URL and version for the published form; otherwise, a null reference.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, UriTemplate = "/_vti_bin/NintexFormsServices/NfRestService.svc/PublishFormXml", ResponseFormat = WebMessageFormat.Json)]
        FormSaveInfo PublishFormXml(string listId, string contentTypeId, string formXml);
    }

    /// <summary>
    /// The WCF service proxy for the Nintex Forms service endpoints.
    /// </summary>
    class FormsWcfServiceProxy : ClientBase<IFormsWcfService>, IFormsWcfService
    {
        public FormsWcfServiceProxy(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
        }

        /// <summary>
        /// Retrieves the XML form definition for the specified content type and SharePoint list.
        /// </summary>
        /// <param name="listId">The unique identifier for the SharePoint list.</param>
        /// <param name="contentTypeId">The content type identifier.</param>
        /// <returns>If successful, a string containing the XML form definition for the specified 
        /// content type and SharePoint list; otherwise, an empty string ("").</returns>
        public string GetFormXml(string listId, string contentTypeId)
        {
            return Channel.GetFormXml(listId, contentTypeId);
        }

        /// <summary>
        /// Deletes the form for the specified content type and SharePoint list.
        /// </summary>
        /// <param name="listId">The unique identifier for the SharePoint list.</param>
        /// <param name="contentTypeId">The content type identifier.</param>
        /// <returns>If successful, a string containing the default view URL of the specified 
        /// SharePoint list; otherwise, an empty string ("").</returns>
        public string DeleteForm(string listId, string contentTypeId)
        {
            return Channel.DeleteForm(listId, contentTypeId);
        }

        /// <summary>
        /// Publishes the XML form definition for the specified content type and SharePoint list.
        /// </summary>
        /// <param name="listId">The unique identifier for the SharePoint list.</param>
        /// <param name="contentTypeId">The content type identifier.</param>
        /// <param name="formXml">The XML form definition to be published.</param>
        /// <returns>If successful, a <see cref="NintexFormsClient.FormSaveInfo" /> object containing 
        /// the URL and version for the published form; otherwise, a null reference.</returns>
        public FormSaveInfo PublishFormXml(string listId, string contentTypeId, string formXml)
        {
            return Channel.PublishFormXml(listId, contentTypeId, formXml);
        }
    }
}
