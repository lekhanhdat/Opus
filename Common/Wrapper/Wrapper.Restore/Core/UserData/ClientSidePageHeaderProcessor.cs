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
namespace AvePoint.Wrapper.Restore.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AngleSharp.Dom;
    using AngleSharp.Html.Dom;
    using AngleSharp.Html.Parser;
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Common.Core;
    using Microsoft.SharePoint.Client;
    using Newtonsoft.Json.Linq;
    using OfficeDevPnP.Core.Pages;

    internal class ClientSidePageHeaderProcessor : IUserDataProcessor
    {
        private AveSPDoc _document;
        private AveSPSite _site;
        private IAveListItem aveListItem;
        private IAveFile aveFile;
        private static AveLogger logger = AveLogger.GetInstance(typeof(ClientSidePageHeaderProcessor));

        public Guid Id { get { return new Guid("cb7ca965-91f0-4336-a5df-13d91d955091"); } }

        public ClientSidePageHeaderProcessor() { }

        public ClientSidePageHeaderProcessor(AveSPDoc document)
        {
            if (document != null)
            {
                this._document = document;
                _site = this._document.ParentSite;
            }
        }

        public ClientSidePageHeaderProcessor(PostActionContract contract)
        {
            if (contract != null && contract.PostSender != null && contract.PostSender is AveSPDoc)
            {
                this._document = contract.PostSender as AveSPDoc;
                _site = this._document.ParentSite;
            }
        }

        public PostActionContract GeneratePostActionContract()
        {
            return new PostActionContract()
            {
                Id = Id,
                PostSender = _document
            };
        }

        public bool Process(Dictionary<string, object> userData)
        {
            return Process(userData, (k, v) => userData[k] = v);
        }

        public bool Process(IAveListItem listItem)
        {
            this.aveListItem = listItem;
            return Process(listItem.FieldValues, (k, v) => listItem[k] = v);
        }

        public bool Process(Dictionary<string, object> userData, Action<string, object> updater)
        {
            object clientSideApplicationId;
            if (userData != null &&
                userData.TryGetValue(ClientSidePage.ClientSideApplicationId, out clientSideApplicationId) &&
                clientSideApplicationId != null &&
                ClientSidePage.SitePagesFeatureId.Equals(clientSideApplicationId.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                Object columnValueAsObj = string.Empty;
                //ModernPage-LayoutWebpartsContent
                try
                {
                    bool needPostAction = false;
                    if (userData.TryGetValue(ClientSidePage.PageLayoutContentField, out columnValueAsObj) && columnValueAsObj != null)
                    {
                        var pageLayoutContentField = columnValueAsObj.ToString();
                        if (ProcessLayoutWebpartsContent(pageLayoutContentField, updater))
                        {
                            needPostAction = true;
                            //证明已经postaction完成
                            if (this.aveListItem!=null) {
                                return true;
                            }
                        }
                    }
                    return !needPostAction;
                }
                catch (Exception ex)
                {
                    logger.Error("Fail to replace LayoutWebpartsContent[{0}],Error:[{1}],StackTrace:[{2}]", columnValueAsObj.ToString(), ex.Message, ex.StackTrace);
                }
            }
            return true;
        }

        #region private method

        /// <summary>
        /// PageLayoutContentField 逻辑处理
        /// </summary>
        /// <param name="userData"></param>
        /// <returns>true:Need post action</returns>
        private bool ProcessLayoutWebpartsContent(string pageLayoutContentField, Action<string, object> updater)
        {
            bool isPost = aveListItem != null;
            bool needPost = false;
            try
            {
                this.aveFile = GetDestinationFileByPageHeader(pageLayoutContentField);
                var context = GetClientContext();
                if ((this.aveFile==null || this.aveFile.UniqueId==Guid.Empty) && !isPost)
                {
                    //如果文件是空，并且不是postaction都返回true;
                    needPost = true;
                    return needPost;
                }
                ProcessLayoutWebpartsContent(context, isPost, ref pageLayoutContentField,updater);
                logger.Info("Complete to replace LayoutWebpartsContent:[{0}]...", pageLayoutContentField);
                needPost = true;
            }
            catch (Exception ex)
            {
                logger.Error("Replace ClientSidePageHeader[{0}] Failed,ErrorMessage:[{1}],StackTrace:[{2}]", pageLayoutContentField, ex.Message, ex.StackTrace);
            }
            return needPost;
        }

        private void ProcessLayoutWebpartsContent(object context, bool isPost,ref string pageLayoutContentField, Action<string, object> updater)
        {
            ClientSidePageHeader pageHeader = default(ClientSidePageHeader);
            //bool needPost = false;
            if (context != null && context is ClientContext)
            {
                using (ClientContext ctx = context as ClientContext)
                {
                    pageHeader = new ClientSidePageHeader(ctx, ClientSidePageHeaderType.Default, null);
                    pageHeader.FromHtml(pageLayoutContentField);
                    //needPost |= !ProcessImageSource(pageHeader, isPost);
                    ProcessImageSource(pageHeader, isPost);
                    ProcessAuthorByLine(pageHeader);
                    ProcessAuthors(pageHeader);
                    var pageTitle = GetClientSidePageTitle(pageLayoutContentField);
                    if (String.IsNullOrEmpty(pageTitle))
                    {
                        throw new NullReferenceException($"pageTitle:[{pageTitle}] is null.please check ClientSidePageHeader:[{pageLayoutContentField}]");
                    }
                    pageLayoutContentField = pageHeader.ToHtml(pageTitle).Replace("{", "&#123;").Replace("}", "&#125;").Replace("&amp;", "&");
                    updater(ClientSidePage.PageLayoutContentField, pageLayoutContentField);
                }
            }
            //return needPost;
        }

        private object GetClientContext()
        {
            return this._document.Web.GetClientContext();
        }

        private IAveFile GetDestinationFileByPageHeader(string pageLayoutContentField)
        {
            ClientSidePageHeader pageHeader = FromClientSidePageHeader(pageLayoutContentField);
            if (String.IsNullOrWhiteSpace(pageLayoutContentField) || String.IsNullOrWhiteSpace(pageHeader.ImageServerRelativeUrl)) return null;
            var oldUrl = pageHeader.ImageServerRelativeUrl.StartsWith("//") ? pageHeader.ImageServerRelativeUrl.Substring(1) : pageHeader.ImageServerRelativeUrl;
            string desImageServerRelativeUrl = AveReplaceProcessor.UrlReplace(oldUrl, _site.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), _site.SourceSiteInfo, _site.ServerRelativeUrl);
            return _document.ParentFolder.ParentList.ParentWeb.SPWeb.GetFile(desImageServerRelativeUrl);
        }

        private ClientSidePageHeader FromClientSidePageHeader(string pageLayoutContentField)
        {
            ClientSidePageHeader pageHeader = GetClientSidePageHeader();
            if (!String.IsNullOrWhiteSpace(pageLayoutContentField))
            {
                pageHeader.FromHtml(pageLayoutContentField);
            }
            return pageHeader;
        }

        private ClientSidePageHeader GetClientSidePageHeader(ClientContext clientContext = null, string imageServerRelativeUrl = "", ClientSidePageHeaderType pageHeaderType = ClientSidePageHeaderType.Default)
        {
            return new ClientSidePageHeader(clientContext, pageHeaderType, imageServerRelativeUrl);
        }

        private void ProcessAuthors(ClientSidePageHeader pageHeader)
        {
            string authors = pageHeader.Authors;
            string newAuthors = ReplaceUsers(authors);
            pageHeader.Authors = newAuthors;
        }

        private void ProcessAuthorByLine(ClientSidePageHeader pageHeader)
        {
            var users = new JArray();
            var originalAuthor = JArray.Parse(pageHeader.AuthorByLine);
            foreach (var user in originalAuthor)
            {
                var newUser = GetMappedUser(user.ToString());
                if (newUser != null)
                {
                    users.Add(newUser.LoginName);
                }
                else
                {
                    users.Add(user.ToString());
                }
            }
            pageHeader.AuthorByLine = users.ToString();
        }

        private bool ProcessImageSource(ClientSidePageHeader pageHeader, bool isPost)
        {
            if (!string.IsNullOrEmpty(pageHeader.ImageServerRelativeUrl)
                && (isPost || this.aveFile.Exists))
            {
                var oldUrl = pageHeader.ImageServerRelativeUrl.StartsWith("//") ? pageHeader.ImageServerRelativeUrl.Substring(1) : pageHeader.ImageServerRelativeUrl;
                pageHeader.ImageServerRelativeUrl = AveReplaceProcessor.UrlReplace(oldUrl, _site.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), _site.SourceSiteInfo, _site.ServerRelativeUrl);
                return true;
            }
           return false;
        }

        private string ReplaceUsers(string authors)
        {
            JArray jAuthors = JArray.Parse(authors);
            foreach (var jAuthor in jAuthors)
            {
                var user = GetMappedUser(jAuthor["id"].ToString());
                if (user != null && user.ID > 0)
                {
                    jAuthor["id"] = user.LoginName;
                    jAuthor["upn"] = user.Email;
                    jAuthor["name"] = user.Name;
                }
            }
            var newAuthors = jAuthors.ToString();
            return newAuthors;
        }

        private IAveUser GetMappedUser(string login)
        {
            IAveUser aveUser = null;
            if (_site != null)
            {
                aveUser = _site.SPMembers.GetOrAddUser(login);
            }
            return aveUser;
        }

        private string GetClientSidePageTitle(string desLayoutWebpartsContent)
        {
            HtmlParserOptions option = default(HtmlParserOptions);
            IHtmlDocument htmlDocument = new HtmlParser(option).ParseDocument(desLayoutWebpartsContent);
            IElement spControldata = (from m in (IEnumerable<IElement>)htmlDocument.All
                                      where m.HasAttribute("data-sp-controldata")
                                      select m).FirstOrDefault();
            if (spControldata != null)
            {
                JObject jSpControldata = JObject.Parse(System.Net.WebUtility.HtmlDecode(spControldata.GetAttribute("data-sp-controldata")));
                if (jSpControldata != null)
                {
                    var jProperties = jSpControldata["properties"];
                    if (jProperties != null && jProperties["title"] != null)
                    {
                        return jProperties["title"].ToString();
                    }
                }
            }
            return null;
        }
        #endregion
    }
}

