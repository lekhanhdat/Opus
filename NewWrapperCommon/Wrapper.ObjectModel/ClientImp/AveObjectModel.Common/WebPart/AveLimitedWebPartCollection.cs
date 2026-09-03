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
using System.Web.UI.WebControls.WebParts;
using AvePoint.Wrapper.Common;
using System.Xml;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon;
namespace AvePoint.ObjectModel.Common
{
    [AveCodeReview("2012/03/09", "yuzhi.jiang@avepoint.com", "jin.zhang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CO_11, CodeReviewConstants.CHECK_LIST_ID_CS_2 }, null, true)]
    class AveLimitedWebPartCollection : AveAbstractCommonCollection<IAveWebPart>, IAveLimitedWebPartCollection
    {
        private AveLimitedWebPartManager mLimitedWebPartManager;
        private IAveRequest mRequest;
        private List<AveWebPart> mWebPartCol;
        private AveWeb mWeb;
        static private AveLogger mLogger=AveLogger.GetInstance(typeof(AveLimitedWebPartCollection));
        public AveLimitedWebPartCollection(AveLimitedWebPartManager limitedWebPartManager, IAveRequest request, Dictionary<string, object> webpartColProperties,AveWeb web)
        {
            mLimitedWebPartManager = limitedWebPartManager;
            mRequest = request;
            mWeb = web;
            base.DataCache.AddPropertyies(webpartColProperties);
            InitLimitedWebPartCollection();
        }

        internal void InitLimitedWebPartCollection()
        {
            List<Dictionary<string, object>> webpartPropertiesList = base.DataCache.GetChildren();
            mWebPartCol = new List<AveWebPart>(webpartPropertiesList.Count);

            foreach(Dictionary<string, object> webpartProperties in webpartPropertiesList)
            {
                AveWebPart webpart = CreateWebPartBasedOnType(mRequest, webpartProperties);
                mWebPartCol.Add(webpart);
            }
        }
        /// <summary>
        /// 根据webpart的类型创建对象
        /// </summary>
        /// <param name="request"></param>
        /// <param name="webpartProperties"></param>
        /// <returns></returns>
        private AveWebPart CreateWebPartBasedOnType(IAveRequest request, Dictionary<string, object> webpartProperties)
        {
            AveWebPart webpart = null;
            try
            {
                if (webpartProperties.ContainsKey("DefinitionXml"))
                {
                    string typeName = GetWebPartTypeName(webpartProperties["DefinitionXml"] as string);
                    string listTitle = string.Empty;

                    switch (typeName)
                    {
                        case "Microsoft.SharePoint.WebPartPages.XsltListViewWebPart":
                            webpart = new AveXsltListViewWebPart(request, webpartProperties);
                            try
                            {
                                listTitle = mWeb.Lists[webpart.BaseInfo.ListId].Title;
                            }
                            catch (Exception ex)
                            {
                                mLogger.Warn("Get List:{0} failed.Error Message:{1}.", webpart.BaseInfo.ListTitle, ex.ToString());
                            }
                            (webpart as AveXsltListViewWebPart).Init(listTitle, mWeb.ID, mWeb.ServerRelativeUrl.TrimStart(new char[] { '/' }));
                            break;
                        case "Microsoft.SharePoint.WebPartPages.ListFormWebPart":
                            webpart = new AveListFormWebPart(request, webpartProperties);
                            (webpart as AveListFormWebPart).Init(mWeb.ID, mWeb.ServerRelativeUrl.TrimStart(new char[] { '/' }));
                            break;
                        case "Microsoft.SharePoint.WebPartPages.ListViewWebPart":
                            webpart = new AveListViewWebPart(request, webpartProperties);
                            try
                            {
                                listTitle = mWeb.Lists[webpart.BaseInfo.ListId].Title;
                            }
                            catch (Exception ex)
                            {
                                mLogger.Warn("Get List:{0} failed.Error Message:{1}.", webpart.BaseInfo.ListTitle, ex.ToString());
                            }
                            (webpart as AveListViewWebPart).Init(listTitle, mWeb.ID, mWeb.ServerRelativeUrl.TrimStart(new char[] { '/' }));
                            break;
                        case "Microsoft.SharePoint.Portal.WebControls.SiteFeedWebPart":
                            webpart = new AveSiteFeedWebPart(request, webpartProperties);
                            webpart.Init();
                            break;
                        case "Microsoft.SharePoint.WebPartPages.GettingStartedWebPart":
                            webpart = new AveGettingStartedWebPart(request, webpartProperties);
                            webpart.Init();
                            break;
                        case "Microsoft.SharePoint.Portal.WebControls.OWACalendarPart":
                        case "Microsoft.SharePoint.Portal.WebControls.OWAContactsPart":
                        case "Microsoft.SharePoint.Portal.WebControls.OWAInboxPart":
                        case "Microsoft.SharePoint.Portal.WebControls.OWAPart":
                        case "Microsoft.SharePoint.Portal.WebControls.OWATasksPart":
                            webpart = new AveOWAWebPart(request, webpartProperties);
                            webpart.Init();
                            break;
                        case "Microsoft.Office.Excel.WebUI.ExcelWebRenderer":
                            webpart = new AveExcelWebAccessWebPart(request, webpartProperties);
                            webpart.Init();
                            break;
                        case "Microsoft.SharePoint.Portal.WebControls.SiteDocuments":
                            webpart = new AveSiteAggregatorWebPart(request, webpartProperties);
                            webpart.Init();
                            break;
                        case "Microsoft.SharePoint.WebPartPages.ImageWebPart":
                            webpart = new AveImageViewWebPart(request, webpartProperties);
                            webpart.Init();
                            break;
                        case "Microsoft.SharePoint.WebPartPages.ContentEditorWebPart":
                            webpart = new AveContentEditorWebPart(request, webpartProperties);
                            webpart.Init();
                            break;
                        case "Microsoft.SharePoint.WebPartPages.BlogAdminWebPart":
                        case "Microsoft.SharePoint.WebPartPages.BlogMonthQuickLaunch":
                            webpart = new AveBlogWebPart(request, webpartProperties);
                            webpart.Init();
                            break;
                        case "Microsoft.Office.Server.Search.WebControls.SearchBoxScriptWebPart":
                            webpart = new AveSearchWebPart(request, webpartProperties);
                            webpart.Init();
                            break;
                        case "Microsoft.SharePoint.Publishing.WebControls.ContentByQueryWebPart":
                            webpart = new AveContentByQueryWebPart(request, webpartProperties);
                            webpart.Init();
                            break;
                        case "Microsoft.Office.Server.WebControls.DocIdSearchWebPart":
                            webpart = new AveDocIdSearchWebPart(request, webpartProperties);
                            webpart.Init();
                            break;
                        case "Microsoft.SharePoint.WebPartPages.MembersWebPart":
                            webpart = new AveMembersWebPart(request, webpartProperties);
                            webpart.Init();
                            break;
                        case "Microsoft.SharePoint.Portal.WebControls.SPSlicerChoicesWebPart":
                        case "Microsoft.SharePoint.Portal.WebControls.ApplyFiltersWebPart":
                            webpart = new AveFilterWebPart(request, webpartProperties);
                            webpart.Init();
                            break;
                        case "Microsoft.SharePoint.Portal.WebControls.DashboardWebPart":
                        case "Microsoft.SharePoint.Portal.WebControls.CommunityJoinWebPart":
                        case "Microsoft.SharePoint.Portal.WebControls.CommunityAdminWebPart":
                            webpart = new AveCommunityWebPart(request, webpartProperties);
                            webpart.Init();
                            break;
                        case "Microsoft.SharePoint.Portal.WebControls.ProjectSummaryWebPart":
                            webpart = new AveProjectSummaryWebPart(request, webpartProperties);
                            webpart.Init();
                            break;
                        case "Microsoft.SharePoint.WebPartPages.PageViewerWebPart":
                            webpart = new AvePageViewerWebPart(request, webpartProperties);
                            (webpart as AvePageViewerWebPart).Init();
                            break;
                        case "Microsoft.SharePoint.WebPartPages.SPTimelineWebPart":
                            webpart = new AveSPTimelineWebPart(request, webpartProperties);
                            (webpart as AveSPTimelineWebPart).Init();
                            break;
                        case "Microsoft.SharePoint.WebPartPages.PictureLibrarySlideshowWebPart":
                            webpart = new AvePictureLibrarySlideshowWebPart(request, webpartProperties);
                            (webpart as AvePictureLibrarySlideshowWebPart).Init();
                            break;
                        case "Microsoft.SharePoint.Taxonomy.TermProperty":
                            webpart = new AveTermPropertyWebPart(request, webpartProperties);
                            (webpart as AveTermPropertyWebPart).Init();
                            break;
                        case "Microsoft.SharePoint.Portal.WebControls.BlogView":
                        case "Microsoft.SharePoint.WebPartPages.DataFormWebPart":
                            webpart = new AveBlogViewWebPart(request, webpartProperties);
                            try
                            {
                                listTitle = mWeb.Lists[webpart.BaseInfo.ListId].Title;
                            }
                            catch (Exception ex)
                            {
                                mLogger.Warn("Get List:{0} failed.Error Message:{1}.", webpart.BaseInfo.ListId, ex);
                            }
                            (webpart as AveBlogViewWebPart).Init(listTitle);
                            break;
                        default:
                            webpart = new AveWebPart(request, webpartProperties);
                            webpart.Init();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Create webpart base on type failed.Error Message:{0}.", ex.ToString());
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
            if (xDoc.OuterXml.Contains("http://schemas.microsoft.com/WebPart/v2"))
            {
                webPartNameSpace = "http://schemas.microsoft.com/WebPart/v2";
            }
            else if (xDoc.OuterXml.Contains("http://schemas.microsoft.com/WebPart/v3"))
            {
                webPartNameSpace = "http://schemas.microsoft.com/WebPart/v3";
            }
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xDoc.NameTable);
            nsmgr.AddNamespace("WebPart", webPartNameSpace);
            if (xDoc.OuterXml.Contains("http://schemas.microsoft.com/WebPart/v2"))
            {
                typeName = xDoc.DocumentElement.SelectSingleNode("WebPart:TypeName", nsmgr).InnerText; 
            }
            else if (xDoc.OuterXml.Contains("http://schemas.microsoft.com/WebPart/v3"))
            {
                string temp = xDoc.DocumentElement.SelectSingleNode("//WebPart:type", nsmgr).Attributes["name"].Value;
                string[] split = temp.Split(new char[] { ',' }, 2);
                typeName = split[0].Trim();
            }
            return typeName;
        }
        internal List<AveWebPart> WebParts
        {
            get
            {
                return mWebPartCol;
            }
        }

        #region IAveLimitedWebPartCollection Members

        public new IAveWebPart this[int index]
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
       
        #region IEnumerable<WebPart> Members

        IEnumerator<IAveWebPart> IEnumerable<IAveWebPart>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
