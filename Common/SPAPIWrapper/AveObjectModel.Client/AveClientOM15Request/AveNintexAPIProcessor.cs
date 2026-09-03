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
using System.Text;
using System.Threading.Tasks;
using AvePoint.Nintex.O365API;
using System.Collections.Concurrent;
using System.Net;
using System.Xml;
using System.Security;
using AvePoint.GCommon;
using System.IO;
using Microsoft365.Authentication;

namespace AvePoint.ObjectModel.ClientOM
{
    class AveNintexAPIProcessor
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveNintexAPIProcessor));
        private IWorkflowAPI nintexWorkflowAPI;
        private IFormAPI mFormAPI;
        private ITokenProvider tokenProvider;
        private ConcurrentDictionary<string, NintexContext> contextCache;
        private string siteUrl;

        public AveNintexAPIProcessor(string siteUrl, ITokenProvider tokenProvider, APIMethod apiMethod)
        {
            this.siteUrl = siteUrl;
            nintexWorkflowAPI = NintexAPIFactory.GetWorkflowAPI(apiMethod);
            mFormAPI = NintexAPIFactory.GetFormAPI(apiMethod);
            this.tokenProvider = tokenProvider;
            contextCache = new ConcurrentDictionary<string, NintexContext>(StringComparer.OrdinalIgnoreCase);
        }

        protected void NintexOperationRun(string actionName,Action run)
        {
            try
            {
                run();
            }
            catch (NintexHttpException exception)
            {
                mLogger.Error("Nintex operation {0} throw an NintexHttpException.Detail:RequestUri:{1},StatusCode:{2},StatusDescription:{3},ResponseText:{4}",
                    actionName,
                    exception.Url,
                    exception.StatusCode,
                    exception.StatusDescription,
                    exception.Response);
                throw;
            }
        }

        public void PublishNintexWorkflow(string webUrl, string workflowId, string workflowRestrictToScope)
        {
            NintexOperationRun("PublishNintexWorkflow",
                delegate
                {
                    var nintexContext = GetNintexContext(webUrl, workflowRestrictToScope);
                    //mLogger.Info("current weburl :{0}", webUrl);
                    var authenticationHeader = InitializeAuthenticationHeader(webUrl, tokenProvider);
                    //mLogger.Info("authenticationHeader weburl:{0}", authenticationHeader.WebUrl);
                    nintexWorkflowAPI.PublishWorkflow(new CustomerDomain(), authenticationHeader, workflowId, nintexContext, AssignedUseType.Production);
                });
        }

        public void SaveNintexForm(string formXml, string webUrl, Guid listId, string contentTypeId)
        {
            NintexOperationRun("SaveNintexForm",
               delegate
               {
                   AuthenticationHeader authenticationHeader = InitializeAuthenticationHeader(webUrl, tokenProvider);
                   mFormAPI.SaveForm(new CustomerDomain(), authenticationHeader, new MemoryStream(Encoding.UTF8.GetBytes(formXml)), listId, contentTypeId, GetNintexContext(webUrl));
               });  
        }

        public void PublishNintexForm(string webUrl, Guid listId, string contentTypeId)
        {
            NintexOperationRun("PublishNintexForm",
             delegate
             {
                 AuthenticationHeader authenticationHeader = InitializeAuthenticationHeader(webUrl, tokenProvider);
                 mFormAPI.PublishForm(new CustomerDomain(), authenticationHeader, listId, contentTypeId, GetNintexContext(webUrl));
             });
            
        }
        public Stream ExportNintexForm(string webUrl, Guid listId, string contentTypeId)
        {
            AuthenticationHeader authenticationHeader = InitializeAuthenticationHeader(webUrl, tokenProvider);
            var stream = mFormAPI.ExportForm(new CustomerDomain(), authenticationHeader, listId, contentTypeId, GetNintexContext(webUrl), FormExportDataType.Xml);
            return stream;
        }
        private NintexContext GetNintexContext(string webUrl, string workflowRestrictToScope = null)
        {
            var tempKey = string.IsNullOrEmpty(workflowRestrictToScope) ? webUrl.TrimEnd('/') : workflowRestrictToScope;
            if (!contextCache.ContainsKey(tempKey))
            {
                contextCache[tempKey] = new NintexContext();
            }
            return contextCache[tempKey];
        }

        private AuthenticationHeader InitializeAuthenticationHeader(string webUrl, ITokenProvider tokenProvider)
        {
            //string cookie = string.Empty;
            //if (cookies.GetCookies(new Uri(this.siteUrl))["SPOIDCRL"] != null)
            //{
            //    cookie = cookies.GetCookies(new Uri(this.siteUrl))["SPOIDCRL"].ToString();
            //}
            //else if (cookies.GetCookies(new Uri(this.siteUrl))["FedAuth"] != null)
            //{
            //    cookie = cookies.GetCookies(new Uri(this.siteUrl))["FedAuth"].ToString();
            //}
            return new AuthenticationHeader() { WebUrl = webUrl, Cookie = new List<string> { tokenProvider.GetToken(new Uri(webUrl)) } };
        }
    }
}
