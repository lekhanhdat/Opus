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
using AvePoint.Wrapper.Common;
using System.IO;
using System.Net;
using System.Xml;
using Microsoft365.SharePoint.CSOM.Extension;
using Microsoft365.SharePoint.Extension;

namespace AvePoint.ObjectModel.ClientOM
{
    public partial class AveClientOM2013Request
    {
        public const string AddFeatureByRestApiBaseUrl = "{0}/_api/{1}/features/add('{2}')";

        public static string MakeWebFullUrl(string webAppUrl, string webServerRelativeUrl)
        {
            var baseUri = new Uri(webAppUrl.TrimEnd('/') + "/");
            var webFullUri = new Uri(baseUri, webServerRelativeUrl.TrimStart('/'));
            string webFullUrl = webFullUri.AbsoluteUri.TrimEnd('/');
            return webFullUrl;
        }
        public static string ConvertAveFeatureScopeString(string featureScope)
        {
            if (string.Equals(featureScope, "site.features", StringComparison.OrdinalIgnoreCase))
            {
                return "site";
            }
            return "web";
        }
       
        public Dictionary<string, object> AddFeatureByRestApi(string webAppUrl,string webServerRelativeUrl,Guid featureId,bool force,string scope)
        {
            var webFullUrl = MakeWebFullUrl(webAppUrl,webServerRelativeUrl);
            var scopeString = ConvertAveFeatureScopeString(scope);
            string requestUrl = string.Format(AddFeatureByRestApiBaseUrl, webFullUrl, scopeString, featureId);
            ReliableHttpWebRequest request = ReliableHttpWebRequest.CreateRequest(requestUrl, ChangeTokenProvider, GetTenantIdAndDefaultAppIdFunc);
            
            request.Method = "POST";
            request.ContentLength = 0;
            request.Timeout = 600000;
            request.ReadWriteTimeout = 1800000;
            request.SetTokenProvider(webFullUrl, tokenProvider, true);
            using (HttpWebResponse result = request.GetResponse() as HttpWebResponse)
            {
                if (result != null)
                {
                    if (result.StatusCode != HttpStatusCode.OK)
                    {
                        string errorMessage = string.Format("Add feature by rest api failed.Request Url:{0},StatusCode:{1}", request.RequestUri, result.StatusCode);
                        mLogger.Error(errorMessage);
                        throw new WebException(errorMessage);
                    }
                    
                    using (var responseStream = result.GetResponseStream())
                    {
                        string content = new StreamReader(responseStream).ReadToEnd();
                        try
                        {
                            var doc = new XmlDocument();
                            doc.LoadXml(content);
                            XmlNamespaceManager manager = new XmlNamespaceManager(doc.NameTable);
                            manager.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");
                            var definitionNode = doc.SelectSingleNode("//d:DefinitionId", manager);
                            if (definitionNode != null)
                            {
                                return new Dictionary<string, object>
                                {
                                    {"DefinitionId",featureId},
                                    {"Definition" + AveObjectModelConstant.ObjectPropertySuffix,new Dictionary<string,object>()}
                                };
                            }
                            throw new WebException(string.Format("Response invalid.No definition id scope,FeatureId:{0}.Content:{1}",featureId,content));
                        }
                        catch (Exception e)
                        {

                            mLogger.Warn("Active feature by rest api failed.FeatureId:{0},ResponseContent:{1},Error:{2}",featureId,content,e);
                            throw new WebException(string.Format("Response invalid.No definition id scope,FeatureId:{0}.Content:{1}", featureId, content));
                        }
                    }
                }
                else
                {
                    string errorMessage = string.Format("Add feature by rest api failed.Request Url:{0},Result is null.", request.RequestUri);
                    mLogger.Error(errorMessage);
                    throw new WebException(errorMessage);
                }
            }
        }
    }
}
