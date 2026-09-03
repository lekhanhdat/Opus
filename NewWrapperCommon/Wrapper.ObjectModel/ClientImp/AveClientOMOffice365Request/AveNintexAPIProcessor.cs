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
using AvePoint.Nintex.O365API;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using AvePoint.GCommon;
using AvePoint.Office365.Api;

namespace AvePoint.ObjectModel.ClientOM
{
    class AveNintexAPIProcessor
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveNintexAPIProcessor));
        private string siteUrl;
        private IWorkflowAPI nintexWorkflowAPI;
        private IFormAPI mFormAPI;
        private ITokenProvider tokenProvider;
        private ConcurrentDictionary<string, NintexContext> contextCache;
        public AveNintexAPIProcessor(string siteUrl, ITokenProvider tokenProvider, APIMethod apiMethod)
        {
            this.siteUrl = siteUrl;
            nintexWorkflowAPI = NintexAPIFactory.GetWorkflowAPI(apiMethod);
            mFormAPI = NintexAPIFactory.GetFormAPI(apiMethod);
            this.tokenProvider = tokenProvider;
            contextCache = new ConcurrentDictionary<string, NintexContext>(StringComparer.OrdinalIgnoreCase);
        }

        private NintexContext GetNintexContext(string webUrl)
        {
            var tempKey = webUrl.TrimEnd('/');
            if (!contextCache.ContainsKey(tempKey))
            {
                contextCache[tempKey] = new NintexContext();
            }
            return contextCache[tempKey];
        }

        private AuthenticationHeader InitializeAuthenticationHeader(string webUrl, ITokenProvider tokenProvider)
        {
            return new AuthenticationHeader() { WebUrl = webUrl, Cookie = tokenProvider.GetToken(new Uri(webUrl)) };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="publishName"></param>
        /// <param name="webUrl"></param>
        /// <param name="listTitle"></param>
        /// <param name="migrate"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string ImportNintexWorkflow(Stream stream, string publishName, string webUrl, string listTitle, Guid parentListId, bool migrate)
        {
            try
            {

                var nintexContext = GetNintexContext(webUrl);
                AuthenticationHeader authenticationHeader = InitializeAuthenticationHeader(webUrl, tokenProvider);
                string workflowId = migrate ? FindWorkflowByName(authenticationHeader, nintexContext, publishName, parentListId, true) : string.Empty;
                return nintexWorkflowAPI.ImportWorkflow(new CustomerDomain(), authenticationHeader, workflowId, migrate, listTitle, stream, nintexContext);
            }
            catch (AggregateException e)
            {
                mLogger.Error("Import workflow failed.Error:{0}", e);
                if (e.InnerException != null && e.InnerException is NintexHttpException)
                {
                    throw new AveWrapperBaseException(((NintexHttpException)e.InnerException).Response);
                }
                else
                {
                    throw;
                }
            }
        }

        public string ConvertNintexFormJsonObjectToXml(string webUrl, string formJsonData, string fileName)
        {
            var nintexContext = GetNintexContext(webUrl);
            AuthenticationHeader authenticationHeader = InitializeAuthenticationHeader(webUrl, tokenProvider);
            return mFormAPI.ConvertJsonObjectToXml(authenticationHeader, nintexContext, formJsonData, fileName);
        }

        public string PublishNintexWorkflow(string webUrl, Guid workflowDefinitionId)
        {
            try
            {
                var nintexContext = GetNintexContext(webUrl);
                AuthenticationHeader authenticationHeader = InitializeAuthenticationHeader(webUrl, tokenProvider);
                return nintexWorkflowAPI.PublishWorkflow(new CustomerDomain(), authenticationHeader, workflowDefinitionId.ToString(), nintexContext, AssignedUseType.Development);

            }
            catch (AggregateException e)
            {
                mLogger.Error("Publish workflow failed.Error:{0}", e);
                if (e.InnerException != null && e.InnerException is NintexHttpException)
                {
                    throw new AveWrapperBaseException(((NintexHttpException)e.InnerException).Response);
                }
                else
                {
                    throw;
                }
            }
        }
        public string PublishNintexWorkflow(Stream stream, string publishName, string webUrl, string listTitle, Guid parentListId)
        {
            try
            {
                var nintexContext = GetNintexContext(webUrl);
                AuthenticationHeader authenticationHeader = InitializeAuthenticationHeader(webUrl, tokenProvider);
                string workflowId = FindWorkflowByName(authenticationHeader, nintexContext, publishName, parentListId, false);
                if (string.IsNullOrEmpty(workflowId))
                {
                    workflowId = nintexWorkflowAPI.ImportWorkflow(new CustomerDomain(), authenticationHeader, workflowId, true, listTitle, stream, nintexContext);
                }
                else
                {
                    nintexWorkflowAPI.SaveWorkflow(new CustomerDomain(), authenticationHeader, workflowId, stream, nintexContext);
                }
                return nintexWorkflowAPI.PublishWorkflow(new CustomerDomain(), authenticationHeader, workflowId, nintexContext, AssignedUseType.Development);
            }
            catch (AggregateException e)
            {
                mLogger.Error("Publish workflow failed.Error:{0}", e);
                if (e.InnerException != null && e.InnerException is NintexHttpException)
                {
                    throw new AveWrapperBaseException(((NintexHttpException)e.InnerException).Response);
                }
                else
                {
                    throw;
                }
            }
        }

        private string FindWorkflowByName(AuthenticationHeader authenticationHeader, NintexContext nintexContext, string workflowName, Guid parentListId, bool onlySaveWorkflow)
        {
            WorkflowItem[] nintexWorkflows = nintexWorkflowAPI.ListWorkflows(new CustomerDomain(), authenticationHeader, nintexContext);
            foreach (WorkflowItem workflow in nintexWorkflows)
            {
                if (onlySaveWorkflow && workflow.IsPublished)
                {
                    continue;
                }
                if (string.Equals(workflow.Name, workflowName, StringComparison.OrdinalIgnoreCase))
                {
                    if (parentListId != Guid.Empty && workflow.ListId != parentListId)
                    {
                        throw new AveWrapperBaseException(string.Format("There is a same name workflow with different parent list, worklfow name: {0}, source list id: {1}, destination list id: {2}", workflowName, parentListId, workflow.ListId));
                    }

                    return workflow.Id;
                }
            }
            return string.Empty;
        }

        public void SaveNintexForm(string formXml, string webUrl, Guid listId, string contentTypeId)
        {
            AuthenticationHeader authenticationHeader = InitializeAuthenticationHeader(webUrl, tokenProvider);
            mFormAPI.SaveForm(new CustomerDomain(), authenticationHeader, new MemoryStream(Encoding.UTF8.GetBytes(formXml)), listId, contentTypeId, GetNintexContext(webUrl));
        }

        public void PublishNintexForm(string webUrl, Guid listId, string contentTypeId)
        {
            AuthenticationHeader authenticationHeader = InitializeAuthenticationHeader(webUrl, tokenProvider);
            mFormAPI.PublishForm(new CustomerDomain(), authenticationHeader, listId, contentTypeId, GetNintexContext(webUrl));
        }
        public Stream ExportNintexForm(string webUrl, Guid listId, string contentTypeId)
        {
            AuthenticationHeader authenticationHeader = InitializeAuthenticationHeader(webUrl, tokenProvider);
            var stream = mFormAPI.ExportForm(new CustomerDomain(), authenticationHeader, listId, contentTypeId, GetNintexContext(webUrl), FormExportDataType.Xml);
            return stream;
        }
    }
}
