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
using AvePoint.Wrapper.Common;
using System.Xml;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon;
namespace AvePoint.ObjectModel.Common
{
    public static class WellknownUris
    {
        public const string MICROSOFT_SCHEMAS_OFFICE = "http://schemas.microsoft.com/office";
        public const string MICROSOFT_SCHEMAS_WEBPART_V2 = "http://schemas.microsoft.com/WebPart/v2";
        public const string MICROSOFT_SCHEMAS_WEBPART_V3 = "http://schemas.microsoft.com/WebPart/v3";
        public const string MICROSOFT_SCHEMAS_SHRAEPOINT = "http://schemas.microsoft.com/sharepoint/";
        public const string MICROSOFT_SCHEMAS_OFFICE_PROJECT_WS = "http://schemas.microsoft.com/office/project/server/webservices";
    }
    [AveCodeReview("2012/03/09", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CO_11, CodeReviewConstants.CHECK_LIST_ID_CS_2 }, null, true)]
    class AveLimitedWebPartCollection : AveAbstractCommonCollection<IAveWebPart>, IAveLimitedWebPartCollection
    {
        private AveLimitedWebPartManager mLimitedWebPartManager;
        private IAveRequest mRequest;
        //private List<AveWebPart> mWebPartCol;
        private AveWeb mWeb;
        static private AveLogger mLogger = AveLogger.GetInstance(typeof(AveLimitedWebPartCollection));
        public AveLimitedWebPartCollection(AveLimitedWebPartManager limitedWebPartManager, IAveRequest request, Dictionary<string, object> webpartColProperties, AveWeb web)
        {
            mLimitedWebPartManager = limitedWebPartManager;
            mRequest = request;
            mWeb = web;
            base.DataCache.AddPropertyies(webpartColProperties);
            InitLimitedWebPartCollection();
        }

        internal void InitLimitedWebPartCollection()
        {
            var webpartPropertiesList = base.DataCache.GetChildren();
            //mWebPartCol = new List<AveWebPart>(webpartPropertiesList.Count);
            mListData = new List<IAveWebPart>(webpartPropertiesList.Count);
            foreach (var webpartProperties in webpartPropertiesList)
            {
                AveWebPart webpart = CreateWebPartBasedOnType(mRequest, webpartProperties);
                //mWebPartCol.Add(webpart);
                mListData.Add(webpart);
            }
        }
        /// <summary>
        /// 根据webpart的类型创建对象
        /// </summary>
        /// <param name="request"></param>
        /// <param name="webpartProperties"></param>
        /// <returns></returns>
        private AveWebPart CreateWebPartBasedOnType(IAveRequest request, IDictionary<string, object> webpartProperties)
        {
            AveWebPart webpart = null;
            try
            {
                if (webpartProperties.ContainsKey("DefinitionXml"))
                {
                    string typeName = GetWebPartTypeName(webpartProperties["DefinitionXml"] as string);
                    string listTitle = string.Empty;

                    string title = webpartProperties.ContainsKey("Title") ? webpartProperties["Title"].ToString() : string.Empty;
                    Guid listId = webpartProperties.ContainsKey("ListId") ? (Guid)webpartProperties["ListId"] : Guid.Empty;
                    if (string.IsNullOrEmpty(title) && listId != Guid.Empty)
                    {
                        IAveList list = mWeb.Lists.GetById(listId);
                        if (list != null)
                        {
                            webpartProperties["Title"] = list.Title;
                        }
                    }

                    switch (typeName)
                    {
                        case "Microsoft.SharePoint.WebPartPages.XsltListViewWebPart":
                            webpart = new AveXsltListViewWebPart(request, mWeb, webpartProperties);
                            try
                            {
                                listTitle = mWeb.Lists[webpart.BaseInfo.ListId].Title;
                            }
                            catch (Exception ex)
                            {
                                mLogger.Warn("Get List:{0} failed.Error Message:{1}", webpart.BaseInfo.ListTitle, ex.ToString());
                            }
                            (webpart as AveXsltListViewWebPart).Init(listTitle, mWeb.ID, mWeb.ServerRelativeUrl.TrimStart(new char[] { '/' }));
                            break;
                        case "Microsoft.SharePoint.WebPartPages.ListFormWebPart":
                            webpart = new AveListFormWebPart(request, mWeb, webpartProperties);
                            (webpart as AveListFormWebPart).Init(mWeb.ID, mWeb.ServerRelativeUrl.TrimStart(new char[] { '/' }));
                            break;
                        case "Microsoft.SharePoint.WebPartPages.ListViewWebPart":
                            webpart = new AveListViewWebPart(request, mWeb, webpartProperties);
                            try
                            {
                                if (webpart.BaseInfo.ListId != Guid.Empty)
                                {
                                    listTitle = mWeb.Lists[webpart.BaseInfo.ListId].Title;
                                }
                            }
                            catch (Exception ex)
                            {
                                mLogger.Warn("Get List:{0} failed.Error Message:{1}", webpart.BaseInfo.ListTitle, ex.ToString());
                            }
                            (webpart as AveListViewWebPart).Init(listTitle, mWeb.ID, mWeb.ServerRelativeUrl.TrimStart(new char[] { '/' }));
                            break;
                        default:
                            webpart = new AveWebPart(request, mWeb, webpartProperties);
                            webpart.Init();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Create webpart base on type failed.Error Message:{0}", ex.ToString());
            }

            return webpart;
        }

        /// <summary>
        /// 获取webpart是哪种类型的
        /// </summary>
        /// <param name="definitionXml"></param>
        /// <returns></returns>
        private string GetWebPartTypeName(string definitionXml)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(definitionXml);
            string webPartNameSpace = string.Empty;
            string typeName = string.Empty;
            //根据webpart的version采用不同的处理办法
            if (xDoc.OuterXml.Contains(WellknownUris.MICROSOFT_SCHEMAS_WEBPART_V2))
            {
                webPartNameSpace = WellknownUris.MICROSOFT_SCHEMAS_WEBPART_V2;
            }
            else if (xDoc.OuterXml.Contains(WellknownUris.MICROSOFT_SCHEMAS_WEBPART_V3))
            {
                webPartNameSpace = WellknownUris.MICROSOFT_SCHEMAS_WEBPART_V3;
            }
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xDoc.NameTable);
            nsmgr.AddNamespace("WebPart", webPartNameSpace);
            if (xDoc.OuterXml.Contains(WellknownUris.MICROSOFT_SCHEMAS_WEBPART_V2))
            {
                typeName = xDoc.DocumentElement.SelectSingleNode("WebPart:TypeName", nsmgr).InnerText;
            }
            else if (xDoc.OuterXml.Contains(WellknownUris.MICROSOFT_SCHEMAS_WEBPART_V3))
            {
                string temp = xDoc.DocumentElement.SelectSingleNode("//WebPart:type", nsmgr).Attributes["name"].Value;
                string[] split = temp.Split(new char[] { ',' }, 2);
                typeName = split[0].Trim();

            }
            return typeName;
        }
        //internal List<AveWebPart> WebParts
        //{
        //    get
        //    {
        //        return mWebPartCol;
        //    }
        //}

        #region IAveLimitedWebPartCollection Members

        public new IAveWebPart this[int index]
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

    }
}
