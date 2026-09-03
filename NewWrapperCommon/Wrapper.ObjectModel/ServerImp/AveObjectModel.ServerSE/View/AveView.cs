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
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveView : IAveView
    {
        protected static AveLogger logger = AveLogger.GetInstance(typeof(AveView));

        private SPView mView;
        private AveContentTypeId mContentTypeId;
        private AveViewFieldCollection mViewFields;
        private AveViewCollection mViews;
        private SPList mSPList;
        private AveList mList;
        public AveView(AveList list, SPView view)
        {
            mList = list;
            mSPList = list.List;
            mViews = list.Views as AveViewCollection;
            mView = view;
        }
        public AveView(AveList list)
        {
            //关闭和开启list的enable moderation设置时，list.views发生了变化，但是API中没有及时更新，需要reload
            list.ParentWeb.ReloadWeb();
            list.Reload();
            mList = list;
            mSPList = list.List;
        }

        internal SPView View
        {
            get
            {
                return mView;
            }
        }

        #region IAveView Members

        public void DeleteObject()
        {
            mView.Views.Delete(mView.ID);
        }

        public void Update()
        {
            mView.Update();
        }

        public string Aggregations
        {
            get
            {
                return mView.Aggregations;
            }
            set
            {
                mView.Aggregations = value;
            }
        }

        public string AggregationsStatus
        {
            get
            {
                return mView.AggregationsStatus;
            }
            set
            {
                mView.AggregationsStatus = value;
            }
        }

        public string BaseViewId
        {
            get { return mView.BaseViewID; }
        }

        public uint Flag
        {
            get
            {
                return (UInt32)AveAssemblyUtility.GetPropertyValue(mView, "Flags");
            }
        }

        public IAveContentTypeId ContentTypeId
        {
            get
            {
                if (mContentTypeId == null)
                {
                    mContentTypeId = new AveContentTypeId(mView.ContentTypeId);
                }
                return mContentTypeId;
            }
            set
            {
                mContentTypeId = value as AveContentTypeId;
                if (mContentTypeId != null)
                {
                    mView.ContentTypeId = mContentTypeId.ContentTypeId;
                }
                else
                {
                    mView.ContentTypeId = SPContentTypeId.Empty;
                }
            }
        }

        public bool DefaultView
        {
            get
            {
                return mView.DefaultView;
            }
            set
            {
                mView.DefaultView = value;
            }
        }

        public bool DefaultViewForContentType
        {
            get
            {
                return mView.DefaultViewForContentType;
            }
            set
            {
                mView.DefaultViewForContentType = value;
            }
        }

        public bool EditorModified
        {
            get
            {
                return mView.EditorModified;
            }
            set
            {
                mView.EditorModified = value;
            }
        }

        public string Formats
        {
            get
            {
                return mView.Formats;
            }
            set
            {
                mView.Formats = value;
            }
        }

        public bool Hidden
        {
            get
            {
                return mView.Hidden;
            }
            set
            {
                mView.Hidden = value;
            }
        }

        public string HtmlSchemaXml
        {
            get { return mView.HtmlSchemaXml; }
        }

        public Guid ID
        {
            get { return mView.ID; }
        }

        public string ImageUrl
        {
            get { return mView.ImageUrl; }
        }

        public bool IncludeRootFolder
        {
            get
            {
                return mView.IncludeRootFolder;
            }
            set
            {
                mView.IncludeRootFolder = value;
            }
        }

        public string Method
        {
            get
            {
                return mView.Method;
            }
            set
            {
                mView.Method = value;
            }
        }

        public bool MobileDefaultView
        {
            get
            {
                return mView.MobileDefaultView;
            }
            set
            {
                mView.MobileDefaultView = value;
            }
        }

        public bool MobileView
        {
            get
            {
                return mView.MobileView;
            }
            set
            {
                mView.MobileView = value;
            }
        }

        public string ModerationType
        {
            get { return mView.ModerationType; }
        }

        public bool OrderedView
        {
            get { return mView.OrderedView; }
        }

        public bool Paged
        {
            get
            {
                return mView.Paged;
            }
            set
            {
                mView.Paged = value;
            }
        }

        public bool PersonalView
        {
            get { return mView.PersonalView; }
        }

        public bool ReadOnlyView
        {
            get { return mView.ReadOnlyView; }
        }

        public bool RequiresClientIntegration
        {
            get { return mView.RequiresClientIntegration; }
        }

        public uint RowLimit
        {
            get
            {
                return mView.RowLimit;
            }
            set
            {
                mView.RowLimit = value;
            }
        }

        public AveViewScope Scope
        {
            get
            {
                return (AveViewScope)mView.Scope;
            }
            set
            {
                mView.Scope = (SPViewScope)value;
            }
        }

        public string ServerRelativeUrl
        {
            get { return mView.ServerRelativeUrl; }
        }

        public string StyleId
        {
            get { return mView.StyleID; }
        }

        public bool Threaded
        {
            get { return mView.Threaded; }
        }

        public string Title
        {
            get
            {
                return mView.Title;
            }
            set
            {
                mView.Title = value;
            }
        }

        public string Toolbar
        {
            get
            {
                return mView.Toolbar;
            }
            set
            {
                mView.Toolbar = value;
            }
        }

        public string ToolbarTemplateName
        {
            get { return mView.ToolbarTemplateName; }
        }

        public string Url
        {
            get { return mView.Url; }
        }

        public string ViewData
        {
            get
            {
                return mView.ViewData;
            }
            set
            {
                mView.ViewData = value;
            }
        }

        public IAveViewFieldCollection ViewFields
        {
            get
            {
                if (mViewFields == null)
                {
                    mViewFields = new AveViewFieldCollection(mView.ViewFields);
                }
                return mViewFields;
            }
        }

        public string ViewJoins
        {
            get
            {
                return mView.Joins;
            }
            set
            {
                mView.Joins = value;
            }
        }

        public string ViewProjectedFields
        {
            get
            {
                return mView.ProjectedFields;
            }
            set
            {
                mView.ProjectedFields = value;
            }
        }

        public string Query
        {
            get
            {
                return mView.Query;
            }
            set
            {
                mView.Query = value;
            }
        }

        public string Type
        {
            get { return mView.Type; }
        }

        public IAveList ParentList
        {
            get
            {
                return mViews.List;
            }
        }

        public string RowLimitExceeded
        {
            get
            {
                return mView.RowLimitExceeded;
            }
            set
            {
                mView.RowLimitExceeded = value;
            }
        }

        public string GroupByFooter
        {
            get
            {
                return mView.GroupByFooter;
            }
            set
            {
                mView.GroupByFooter = value;
            }
        }

        public string GroupByHeader
        {
            get
            {
                return mView.GroupByHeader;
            }
            set
            {
                mView.GroupByHeader = value;
            }
        }

        public string OpenApplicationExtension
        {
            get
            {
                return mView.OpenApplicationExtension;
            }
            set
            {
                mView.OpenApplicationExtension = value;
            }
        }

        public string ViewBody
        {
            get
            {
                return mView.ViewBody;
            }
            set
            {
                mView.ViewBody = value;
            }
        }

        public string ViewEmpty
        {
            get
            {
                return mView.ViewEmpty;
            }
            set
            {
                mView.ViewEmpty = value;
            }
        }

        public string ViewFooter
        {
            get
            {
                return mView.ViewFooter;
            }
            set
            {
                mView.ViewFooter = value;
            }
        }

        public string ViewHeader
        {
            get
            {
                return mView.ViewHeader;
            }
            set
            {
                mView.ViewHeader = value;
            }
        }

        public string ParameterBindings
        {
            get
            {
                return mView.ParameterBindings;
            }
            set
            {
                mView.ParameterBindings = value;
            }
        }

        public string Joins
        {
            get
            {
                return mView.Joins;
            }
            set
            {
                mView.Joins = value;
            }
        }

        public string InlineEdit
        {
            get
            {
                return mView.InlineEdit;
            }
            set
            {
                mView.InlineEdit = value;
            }
        }

        public string XslLink
        {
            get
            {
                return mView.XslLink;
            }
            set
            {
                mView.XslLink = value;
            }
        }

        public string Xsl
        {
            get
            {
                return mView.Xsl;
            }
            set
            {
                mView.Xsl = value;
            }
        }

        public void ApplyStyle(IAveViewStyle viewStyles)
        {
            mView.ApplyStyle((viewStyles as AveViewStyle).ViewStyle);
        }

        public string CalendarSettings
        {
            get
            {
                return mView.CalendarSettings;
            }
            set
            {
                mView.CalendarSettings = value;
            }
        }

        public SPView RestoreView(AveDocumentInfo info, string title, string leafName, int? userID, bool isPersonal, int viewType, Guid viewId, bool? isDefaultView, bool isMobileView, bool isDefaultMobileView,bool hidden)
        {
            SPView view = null;
            SPList personalList = null;
            try
            {
                if (isPersonal)
                {
                    //只有使用对应的user才能得到PersonalView，这里SPSite对象直接Close了，后面SPView对象只能读取不能更新。
                    IAveUser user = mList.ParentWeb.SiteUsers.GetByID(userID.Value);
                    IAveWeb web;
                    try
                    {
                        web = mList.ParentWeb.Site.GetCheckoutWeb(mList.ParentWeb.Site.ID, mList.ParentWeb, user, Guid.Empty, false);
                    }
                    catch(Exception e)
                    {
                        var ex = new AveUnauthorizedAccessException(string.Format("User:{0} don't have enough permission to site:{1}.", user.LoginName, mList.ParentWeb.Url),user.LoginName,e);
                        logger.Warn("Restore personal view failed. View name: {0}, Reason: {1}.", title, ex);
                        throw ex;
                    }
                    personalList = (web as AveWeb).Web.Lists[mSPList.ID];
                    view = personalList.Views[title];
                    if (!view.Url.EndsWith("/" + leafName, StringComparison.OrdinalIgnoreCase)) //同名不同url不应该覆盖
                    {
                        view = null;
                    }
                }
                else
                {
                    string tempLeafName = leafName;
                    if (!tempLeafName.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        tempLeafName = "/" + leafName;
                    }
                    foreach (SPView tempView in mSPList.Views)
                    {
                        if (tempView.Url.EndsWith(tempLeafName, StringComparison.OrdinalIgnoreCase))
                        {
                            view = tempView;
                            break;
                        }
                    }
                    if (info.FindViewByTitle && view == null)
                    {
                        foreach (SPView tempView in mSPList.Views)
                        {
                            if (tempView.Title.Equals(title, StringComparison.Ordinal))
                            {
                                view = tempView;
                                break;
                            }
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetRestoreViewError, e.ToString());
            }
            AveViewType enumViewType = GetViewType(viewType);
            string deletedViewUrl = string.Empty;
            if (view != null)
            {
                //TODO:Check view conflict
                info.RestoringItem.ConflictType = ConflictType.Document;
                if (info.RestoringItem.OverWrite && (!view.Type.Equals(enumViewType.ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    deletedViewUrl = view.ServerRelativeUrl;
                    view.Views.Delete(view.ID);//view.Views.Delete(view.Id);
                    view = null;
                }
            }
            if (view != null)
            {
                info.AveView.ViewUrl = view.Url;
            }
            if (view == null)
            {
                #region 有时候我们会还原出空的WebPart的View页面，需要把错误的页面给Recycle，然后再还原
                try
                {
                    if (!isPersonal)
                    {
                        deletedViewUrl = RecycleWebPartView(deletedViewUrl, leafName);
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("Verify the view:{0} with leaf name: {1} and url: {2} under list: {3} failed: {4}.", title, leafName, deletedViewUrl, mSPList.RootFolder.Url, ex.ToString());
                }
                #endregion

                if (isPersonal)
                {
                    string viewName = title;
                    try
                    {
                        view = personalList.Views.Add(viewName, new System.Collections.Specialized.StringCollection(), "", 100, true, false, (Microsoft.SharePoint.SPViewCollection.SPViewType)enumViewType, true);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        SPUser user = (mList.ParentWeb as AveWeb).Web.SiteUsers.GetByID(userID.Value);
                        logger.Log(AveLogLevel.WARN, "User: {0} has no enough permission to create a personal view, will try to promote permission as site Administrator temporary to create a personal view. Exception: {1}", user.LoginName, ex.ToString());
                        user.IsSiteAdmin = true;
                        user.Update();
                        try
                        {
                            using (SPSite personalSite = new SPSite(mList.ParentWeb.Site.ID, user.UserToken))
                            {
                                using (SPWeb personalWeb = personalSite.OpenWeb(mList.ParentWeb.ID))
                                {
                                    personalList = personalWeb.Lists[mSPList.ID];
                                    view = personalList.Views.Add(viewName, new System.Collections.Specialized.StringCollection(), "", 100, true, false, (Microsoft.SharePoint.SPViewCollection.SPViewType)enumViewType, true);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        finally
                        {
                            user.IsSiteAdmin = false;
                            user.Update();
                            logger.Log(AveLogLevel.INFO, "Change temp creates personal view permission back success for user: {0}.", user.LoginName);
                        }
                    }
                }
                else
                {
                    string viewName = leafName.Substring(0, leafName.LastIndexOf('.'));
                    view = mSPList.Views.Add(viewName, new System.Collections.Specialized.StringCollection(), "", 100, true, false, (Microsoft.SharePoint.SPViewCollection.SPViewType)enumViewType, false);
                }
                info.IsNewCreated = true;
                // ADO-148155在add view之后要reload一下web，否则还原webpart的时候在list上找不到view。
                info.AveItem.Web.ReloadWeb();
            }
            if (info.RestoringItem.OverWrite || info.IsNewCreated)
            {
                UpdateViewProperty(view, title, isDefaultView, isMobileView, isDefaultMobileView,hidden);
            }

            //[ADO-17975]使用SharePoint Designer，可以把List Check Out，导致View文件被Check Out。由于Wrapper不支持多Version View文件的还原，修改代码防止抛出异常导致Job CWE。
            //info.AveView.Views.Add(viewId, view.ID);
            info.AveView.Views[viewId] = view.ID;

            if (isPersonal)
            {
                //如果是新建的PersonalView，mViewUrl可能为null，在下面通过正常的方法给值
                if (string.IsNullOrEmpty(info.AveView.ViewUrl))
                {
                    info.AveView.ViewUrl = personalList.Views[view.ID].Url;
                }
            }
            else
            {
                info.AveView.ViewUrl = view.Url;
            }

            return view;
        }

        private string RecycleWebPartView(string deletedViewUrl, string leafName)
        {
            SPFile tempFile = null;
            if (string.IsNullOrEmpty(deletedViewUrl))
            {
                deletedViewUrl = string.Format("{0}/Forms/{1}", mSPList.RootFolder.ServerRelativeUrl, leafName);
                tempFile = mSPList.ParentWeb.GetFile(SPResourcePath.FromDecodedUrl(deletedViewUrl));
                if (!tempFile.Exists)
                {
                    deletedViewUrl = string.Format("{0}/{1}", mSPList.RootFolder.ServerRelativeUrl, leafName);
                    tempFile = mSPList.ParentWeb.GetFile(SPResourcePath.FromDecodedUrl(deletedViewUrl));
                }
            }
            else
            {
                tempFile = mSPList.ParentWeb.GetFile(SPResourcePath.FromDecodedUrl(deletedViewUrl));
            }

            if (tempFile.Exists && (tempFile.Item == null))
            {
                int webPartCount = 0;
                using (Microsoft.SharePoint.WebPartPages.SPLimitedWebPartManager manager = tempFile.GetLimitedWebPartManager(System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared))
                {
                    webPartCount = manager.WebParts.Count;
                    if (manager.Web != null)
                    {
                        manager.Web.Dispose();
                    }
                }

                if (webPartCount <= 0)
                {
                    tempFile.Recycle();
                    tempFile.Update();
                }
            }
            return deletedViewUrl;
        }

        private void UpdateViewProperty(SPView view, string title, bool? isDefaultView, bool isMobileView, bool isDefaultMobileView,bool hidden)
        {
            bool change = false;
            if (!view.Title.Equals(title))
            {
                view.Title = title;
                change = true;
            }
            if (isDefaultView.HasValue && view.DefaultView != isDefaultView.Value)
            {
                if (isDefaultView.Value && mSPList.DefaultView == null)
                {
                    if (mSPList.Views.Count > 0)
                    {
                        SPView tempView = mSPList.Views[0];
                        tempView.DefaultView = true;
                        tempView.Update();
                    }
                }
                view.DefaultView = isDefaultView.Value;
                change = true;
            }
            if (view.MobileView != isMobileView)
            {
                view.MobileView = isMobileView;
                change = true;
            }
            if (view.MobileDefaultView != isDefaultMobileView)
            {
                view.MobileDefaultView = isDefaultMobileView;
                change = true;
            }
            if (view.Hidden != hidden)
            {
                view.Hidden = hidden;
                change = true;
            }
            if (change)
            {
                view.Update();
            }
        }

        private AveViewType GetViewType(int viewType)
        {
            AveViewType enumViewType;
            if ((viewType & 0x4000000) == 0x4000000)
            {
                enumViewType = AveViewType.Gantt;
            }
            else if ((viewType & 0x80000) == 0x80000)
            {
                enumViewType = AveViewType.Calendar;
            }
            else if ((viewType & 0x20000) == 0x20000)
            {
                enumViewType = AveViewType.Chart;
            }
            else if ((viewType & 0x800) == 0x800)
            {
                enumViewType = AveViewType.Grid;
            }
            else if ((viewType & 0x1) == 0x1)
            {
                enumViewType = AveViewType.Html;
            }
            else
                enumViewType = AveViewType.None;
            return enumViewType;
        }

        #endregion


        public string CssStyleSheet
        {
            get { return mView.CssStyleSheet; }
        }

        public uint MobileItemLimit
        {
            get
            {
                return mView.MobileItemLimit;
            }
            set
            {
                mView.MobileItemLimit = value;
            }
        }

        public string MobileSimpleViewField
        {
            get
            {
                return mView.MobileSimpleViewField;
            }
            set
            {
                mView.MobileSimpleViewField = value;
            }
        }

        public Uri MobileUrl
        {
            get { return mView.MobileUrl; }
        }

        public string ProjectedFields
        {
            get
            {
                return mView.ProjectedFields;
            }
            set
            {
                mView.ProjectedFields = value;
            }
        }

        public string PropertiesXml
        {
            get { return mView.PropertiesXml; }
        }

        public bool RecurrenceRowset
        {
            get { return mView.RecurrenceRowset; }
        }

        public bool TabularView
        {
            get
            {
                return mView.TabularView;
            }
            set
            {
                mView.TabularView = value;
            }
        }

        public string ToolbarType
        {
            get { return mView.ToolbarType; }
        }

        public AveFileLevel Level
        {
            get { return (AveFileLevel)mView.Level; }
        }


        public IAveUserResource TitleResource
        {
            get
            {
                return mView.TitleResource == null ? null : new AveUserResource(mView.TitleResource);
            }
        }

        public string ListViewXml
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                var xd = new System.Xml.XmlDocument();
                xd.LoadXml(mView.GetViewXml());
                var spotlightInfoNode = xd.SelectSingleNode("View/SpotlightInfo");
                if (spotlightInfoNode != null)
                {
                    spotlightInfoNode.ParentNode.RemoveChild(spotlightInfoNode);
                }
                var parentNode = xd.SelectSingleNode("View");
                var newNode = xd.CreateDocumentFragment();
                newNode.InnerXml = value;
                parentNode.AppendChild(newNode);
                mView.SetViewXml(xd.OuterXml);
            }
        }
    }
}
