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
using System.IO;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Utilities;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using AvePoint.ObjectModel.WebService;
using AvePoint.Wrapper.Resource.Client;
using AveClientRequest.Common;
using System.Xml;
using System.Net.Sockets;
using System.Net;
using ClientFile = Microsoft.SharePoint.Client.File;
using Microsoft.SharePoint.Client.WebParts;
using AvePoint.Wrapper.Common.Common.Utility;
using AvePoint.Wrapper.Resource;
using System.Threading;
using System.Reflection;
using Microsoft.SharePoint.Client.RecordsRepository;
using Microsoft.SharePoint.Client.CompliancePolicy;
using AvePoint.Common.FilterEngine;
using NVelocity.Util.Introspection;
using Microsoft.SharePoint.Client.ComplianceFoundation.Models;
using Microsoft365.SharePoint.Cache.Restore;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveDocumentRestore : IDisposable
    {
        private AveLogger mLog = AveLogger.GetInstance(typeof(AveDocumentRestore));
        //private static List<string> ContainCodeBloxkSetupPaths = new List<string>() { @"Features\\PortalLayouts\\SearchMain.aspx", @"global\\seattle.master", 
        //@"Features\\SearchCenterFiles\\conversationresults.aspx", @"Features\\PremiumSearchVerticals\\videoresults.aspx", 
        //@"Features\\SearchCenterFiles\\results.aspx", @"Features\\SearchCenterFiles\\peopleresults.aspx", 
        //@"Features\\ReportAndDataSearch\\reportsanddataresults.aspx", @"Features\\PortalLayouts\\SearchResults.aspx",
        //@"Features\\PortalLayouts\\SearchMain.aspx"};
        //private static List<string> mCodeBlockSite = new List<string>() { "SRCHCEN#0", "SRCHCENTERLITE#0" };  
        private readonly static object moveConflictFolderLocker = new object();
        private readonly static object lockObj = new object();
        private Site mSite;
        private Web mParentWeb;
        private string mParentWebUrl;
        private List mParentList;
        private string mParentFolderUrl;
        private ResourcePath mParentFolderPath;
        private Folder mParentFolder;
        private ListItem mListItem;
        private int mRowId;
        private string mName;
        private int mVersion;
        private bool mIsCurrentVersion;
        private bool mIsView;
        private bool mIsGhostedPage;
        private bool mHasStream;
        private bool mOverWrite;
        private bool mIsOriginalCheckOut;

        private string mFileRelativeUrl;
        private ResourcePath mFileRelativePath;
        private string mCheckInComment;
        private string mSetupPath;
        private AveRestoreOption mRestoreOption;
        private Stream mFileStream;
        private Guid mListId;
        private AveListItemRestore mItemRestore;
        private ClientContext mContext;
        private DocumentRestoreInfo ParentInfo;
        private int mModerationStatus;
        private int mLevel;
        //private AuthenticationMode mAuthMode;
        private string mServerVersion;

        private bool mIsWelcomePageChanged = false;
        private bool mHasPreCurrentVersion;
        private bool mIsNewCreated;
        private bool mMOVE_ITEM_TO_CONFLICT_FOLDER;
        private bool mOverwriteByLastModifiedTime;
        private string mListRootFolderServerRelativeUrl;
        private string mLeafName;
        private Dictionary<string, object> mUserData;
        private int mListItemId;
        private AveClientOM2013Request mRequest;
        private IAveWeb mAveWebCache;
        private AveDocumentInfo mDocInfo;
        private CustomizedPageStatus mPageStatus;
        private bool mIsFormPage = false;
        private bool mIsViewPage = false;
        private bool mHasCodeBlock;
        private string mLoadFileError = string.Empty;
        [ThreadStatic]
        private static IList<string> tempVersionLabels;

        private const string ModernArticlePage = "0x0101009D1CB255DA76424F860D91F20E6C4118";
        private const string ContentTypeId = "ContentType";

        private bool? isModernPage;
        private bool IsModernPage
        {
            get
            {
                if (!isModernPage.HasValue)
                {
                    isModernPage = mUserData.ContainsKey(ContentTypeId) && mUserData[ContentTypeId].ToString().StartsWith(ModernArticlePage);
                }
                return isModernPage.Value;
            }
        }

        /// <summary>
        /// 为unittest添加构造函数
        /// </summary>
        public AveDocumentRestore() { }

        public AveDocumentRestore(AveClientOM2013Request request, Site site, ClientContext conText, string serverVersion)
        {
            mRequest = request;
            mSite = site;
            mContext = conText;
            //mAuthMode = mode;
            mServerVersion = serverVersion;
        }

        protected void PrepareRestoreContext(AveDocumentInfo docInfo, Stream fileStream)
        {
            mDocInfo = docInfo;
            Dictionary<string, object> data = docInfo.DocData;
            mParentWeb = mContext.Site.OpenWeb(data["WebUrl"] as string);
            mParentWebUrl = data["WebUrl"] as string;
            mAveWebCache = data.ContainsKey("AveWebObject") ? (IAveWeb)data["AveWebObject"] : null;
            mParentFolderUrl = data["FolderUrl"] as string;
            mParentFolderPath = ResourcePath.FromDecodedUrl(mParentFolderUrl);
            //supprot special characters such as"#,%"
            mParentFolder = mParentWeb.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(data["FolderUrl"] as string));
            mRowId = data.ContainsKey("DoclibRowId") ? Convert.ToInt32(data["DoclibRowId"]) : -1;
            mRestoreOption = (AveRestoreOption)data["RestoreOption"];
            mName = data["Title"] as string;
            mLeafName = mName;
            mVersion = Convert.ToInt32(data["UIVersion"]);
            mIsCurrentVersion = data.ContainsKey("IsCurrentVersion") ? Convert.ToBoolean(data["IsCurrentVersion"]) : false;
            mFileRelativeUrl = mParentFolderUrl.TrimEnd('/') + "/" + mName;
            mFileRelativePath = ResourcePath.FromDecodedUrl(mFileRelativeUrl);
            mIsView = data.ContainsKey("IsView") ? Convert.ToBoolean(data["IsView"]) : false;
            mIsGhostedPage = data.ContainsKey("IsGhostedPage") ? Convert.ToBoolean(data["IsGhostedPage"]) : false;
            mIsFormPage = data.ContainsKey("IsFormPage") ? Convert.ToBoolean(data["IsFormPage"]) : false;
            mIsViewPage = data.ContainsKey("IsViewPage") ? Convert.ToBoolean(data["IsViewPage"]) : false;
            mSetupPath = data.ContainsKey("SetupPath") ? data["SetupPath"] as string : string.Empty;
            mHasStream = data.ContainsKey("HasStream") ? Convert.ToBoolean(data["HasStream"]) : false;
            mOverWrite = data.ContainsKey("DeleteItem") ? Convert.ToBoolean(data["DeleteItem"]) : false;
            mIsOriginalCheckOut = data.ContainsKey("IsOriginalCheckOut") ? Convert.ToBoolean(data["IsOriginalCheckOut"]) : false;
            mCheckInComment = data.ContainsKey("CheckInComment") ? data["CheckInComment"] as string : string.Empty;
            mModerationStatus = data.ContainsKey("_ModerationStatus") ? Convert.ToInt32(data["_ModerationStatus"]) : -1;
            mLevel = data.ContainsKey("Level") ? Convert.ToInt32(data["Level"]) : -1;
            mHasPreCurrentVersion = data.ContainsKey("HasPreCurrentVersion") ? Convert.ToBoolean(data["HasPreCurrentVersion"]) : false;
            mMOVE_ITEM_TO_CONFLICT_FOLDER = data.ContainsKey("MOVE_ITEM_TO_CONFLICT_FOLDER") ? Convert.ToBoolean(data["MOVE_ITEM_TO_CONFLICT_FOLDER"]) : false;
            mListRootFolderServerRelativeUrl = data.ContainsKey("ListRootFolderServerRelativeUrl") ? data["ListRootFolderServerRelativeUrl"] as string : string.Empty;
            mPageStatus = data.ContainsKey("CustomizedPageStatus") ? (CustomizedPageStatus)data["CustomizedPageStatus"] : CustomizedPageStatus.None;
            /*
             * 不能依賴于template
             */
            mHasCodeBlock = IsCodeBlockStatus();//ContainCodeBloxkSetupPaths.Contains(mSetupPath) ;//&& mCodeBlockSite.Contains(ParentInfo.ParentWebInfo.WebTemplate.ToUpper());
            mFileStream = TryProcessContentTypeLinkItemStream(fileStream, SetFieldValuesAction(docInfo));
            mListId = data.ContainsKey("ListId") ? (Guid)data["ListId"] : default;
            mParentList = data.ContainsKey("ListId") ? mParentWeb.Lists.GetById((Guid)data["ListId"]) : null;
            mOverwriteByLastModifiedTime = data.ContainsKey("OverwriteByLastModifiedTime") ? Convert.ToBoolean(data["OverwriteByLastModifiedTime"]) : false;
            mItemRestore = mParentList != null && mRowId > 0 ? new AveListItemRestore(mRequest, mSite, mParentWeb, mParentList, mRowId, mModerationStatus, mContext) : null;
            mUserData = docInfo.FieldsInfo.Fields;
            mUserData.Remove("Modified_x0020_By"); //this column has a priority than editor when they update together, but we want to keep editor.
            LoadWebInfo();
        }
        private Action<Dictionary<string, object>> SetFieldValuesAction(AveDocumentInfo docInfo)
        {
            return (needSetFieldValueDic) => {
                try
                {
                    foreach (var item in needSetFieldValueDic)
                    {
                        if (docInfo.FieldsInfo.Fields.ContainsKey(item.Key))
                        {
                            docInfo.FieldsInfo.Fields[item.Key] = item.Value;
                        }
                    }
                }
                catch (Exception e)
                {
                    mLog.Error("An error occured when generate SetFieldValuesAction due to {0}", e);
                }
            };
        }

        private Stream TryProcessContentTypeLinkItemStream(Stream fileStream, Action<Dictionary<string, object>> action = null)
        {
            try
            {
                lock (lockObj)
                {
                    if (fileStream == null)
                    {
                        throw new InvalidDataException();
                    }
                    if (string.IsNullOrWhiteSpace(mName))
                    {
                        throw new ArgumentNullException(mName);
                    }
                    if (!mName.ToLower().EndsWith(".url".ToLower()))
                    {
                        return fileStream;
                    }
                    //Greater than 5M, this is not content type link steam
                    if (fileStream.Length >= 5 * 1024 * 1024)
                    {
                        mLog.Warn($"This item:{mName} should not be content type link steam.");
                        return fileStream;
                    }

                    if (!WrapperConfiguration.WrapperConfigurationForBPOS.UseTargetReferenceOfLinkContentTypeItem)
                    {
                        mLog.Info($"Current stream don't need to replace, stack trace:{Environment.StackTrace}");
                        return fileStream;
                    }
                    return ReplaceContentTypeLinkItemStream(fileStream, action);
                }
            }
            catch (Exception e)
            {
                mLog.Error("An error occured when get content type link item's stream due to {0}", e);
            }
            return fileStream;
        }

        private Stream ReplaceContentTypeLinkItemStream(Stream fileStream, Action<Dictionary<string, object>> action = null)
        {
            using (var sr = new StreamReader(fileStream))
            {
                StringBuilder sb = ReplaceContentTypeLinkItemStreamInternal(sr, action);
                if (sb != default(StringBuilder) && sb.Length > 0)
                {
                    var result = sb.ToString();
                    mLog.Info($"Current link content type item details:{result}");
                    using (MemoryStream source = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(result)))
                    {
                        MemoryStream target = new MemoryStream();
                        AveIOHelper.Copy(source, target);
                        target.Position = 0;
                        return target;
                    }
                }
                return fileStream;
            }
        }

        private StringBuilder ReplaceContentTypeLinkItemStreamInternal(StreamReader sr, Action<Dictionary<string, object>> action = null)
        {
            StringBuilder sb = new StringBuilder();
            string sourceUrl = "";
            string condition = "";
            while (!string.IsNullOrWhiteSpace(sourceUrl = sr.ReadLineAsync().GetAwaiter().GetResult()))
            {
                if (sourceUrl.ToLower().StartsWith("URL=".ToLower()))
                {
                    condition = $"{condition}SecondLineMatch;";
                    sourceUrl = $"{sourceUrl.ToLower().TrimStart("URL=".ToLower().ToCharArray())}";
                    var targetUrl = AveReplaceProcessor.UrlReplace(sourceUrl, mDocInfo.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mDocInfo.MappingManager.SiteMappingManager.SourceSiteInfo, mDocInfo.MappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl);
                    sb.AppendLine($"URL={targetUrl}");
                    if (action != null)
                    {
                        //Set replaced _ShortcutUrl for redirecting the correct link
                        var dic = new Dictionary<string, object>();
                        dic["_ShortcutUrl"] = targetUrl;
                        action(dic);
                    }
                }
                else if (sourceUrl.ToLower().Contains("[InternetShortcut]".ToLower()))
                {
                    condition = $"{condition}FirstLineMatch;";
                    sb.AppendLine(sourceUrl);
                }
                else
                {
                    break;
                }
            }
            mLog.Info($"Check Is ContentTyepStream, condition:{condition}, stream length:{sb.Length}");
            if (string.Equals(condition, "FirstLineMatch;SecondLineMatch;", StringComparison.OrdinalIgnoreCase) && sb.Length > 0)
            {
                return sb;
            }
            return default(StringBuilder);
        }

        private bool IsCodeBlockStatus() //SAAS-12850  目前无法很好的判断code block，为了保险起见，master page gallery下的非none status文件默认认为是code block
        {
            if (this.ParentInfo.ParentListInfo != null
                && this.ParentInfo.ParentListInfo.BaseTemplate == (int)AveListTemplateType.WebPageLibrary)
            {
                return false;
            }
            return mPageStatus == CustomizedPageStatus.Uncustomized;
        }

        public Dictionary<string, object> RestoreDocument(AveDocumentInfo docInfo, Stream fileStream, DocumentRestoreInfo restoreInfo)
        {
            ParentInfo = restoreInfo;//Set document parent info instead load with Client API.
            PrepareRestoreContext(docInfo, fileStream);
            Dictionary<string, object> restoreResult = null;
            mFileStream = fileStream;
            try
            {
                if(ItemRestoreCache.IsOverWriteFailItem(mListId.ToString(), mFileRelativeUrl))
                {
                    throw new Exception("RM_RS_FailOverwriteItem");
                }
                if (mIsView)
                {
                    restoreResult = RestoreView(docInfo.DocData["ViewInformation"] as List<Dictionary<string, object>>);
                }
                else
                {
                    //处理conflict folder
                    if (mMOVE_ITEM_TO_CONFLICT_FOLDER)
                    {
                        lock (moveConflictFolderLocker)
                        {
                            MoveToConflictFolder();
                        }
                    }
                    restoreResult = RestoreGenericFile(docInfo.DocData, docInfo.FieldsInfo.Fields, docInfo.FieldsInfo.UniqueValueFields);
                }
            }
            catch (Exception ex)/*review-qlluo*/
            {
                if ((ex is ServerException) && ((ex as ServerException).ServerErrorCode == -2130575282 || (ex as ServerException).ServerErrorCode == -2147023080))
                {
                    mLog.Error("Current site may has exceeded the max storage quota limited.");
                    //exceed storage limited
                    throw;
                }
                if (restoreResult == null)
                {
                    restoreResult = new Dictionary<string, object>();
                }
                //SAAS-35368
                if ((ex is ServerException) && ((ex as ServerException).ServerErrorCode == -2130575223))
                {
                    restoreResult["SkippedByCannotEditItem"] = true;
                    return restoreResult;
                }
                if (ex is AveWrapperSkipException)
                {
                    return restoreResult;
                }
                restoreResult["Exception"] = string.Format("Restore document:{0}\\{1} failed:{2}.\r\n", mParentFolderUrl, mName, ex.ToString());
                if (ex != null && !string.IsNullOrEmpty(ex.Message) && ex.Message.Contains("0x80131904"))
                {
                    restoreResult["ExceptionMessage"] = string.Format(WrapperRestoreReportResource.Wrapper_SharePointBusyError, ex.Message);
                }
                else
                {
                    restoreResult["ExceptionMessage"] = ex.Message;
                }
            }

            return restoreResult;
        }

        protected Dictionary<string, object> RestoreView(List<Dictionary<string, object>> viewInfoList)
        {
            //deadlock occurs when restoring view concurrently
            lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ViewLock"))
            {
                if (mParentList != null)
                {
                    LoadViews();
                }
                Dictionary<string, object> restoreResult = new Dictionary<string, object>();
                Dictionary<Guid, Guid> viewIdMapping = new Dictionary<Guid, Guid>();
                restoreResult["ViewIdsMapping"] = viewIdMapping;
                try
                {
                    Dictionary<string, object> viewInfo = viewInfoList.First<Dictionary<string, object>>();
                    View view = null;
                    //ViewCollection.ViewType viewType = (ViewCollection.SPViewType)Enum.Parse(typeof(ViewCollection.SPViewType), viewInfo["ViewTypeKind"].ToString());
                    ViewType viewType = (ViewType)Enum.Parse(typeof(ViewType), viewInfo["ViewType"].ToString());

                    string leafName = viewInfo["LeafName"] as string;
                    string title = viewInfo["Title"] as string;
                    string viewData = viewInfo.ContainsKey("ViewData") ? (string)viewInfo["ViewData"] : null;
                    bool personalView = viewInfo.ContainsKey("PersonalView") ? (bool)viewInfo["PersonalView"] : false;
                    bool setAsDefaultView = viewInfo.ContainsKey("SetAsDefaultView") ? (bool)viewInfo["SetAsDefaultView"] : false;
                    ViewScope scope = (ViewScope)Enum.Parse(typeof(ViewScope), viewInfo.ContainsKey("Scope") ? viewInfo["Scope"].ToString() : "DefaultValue", true);
                    bool hidden = viewInfo.ContainsKey("Hidden") ? (bool)viewInfo["Hidden"] : false;
                    string baseViewId = viewInfo.ContainsKey("BaseViewId") ? Convert.ToString(viewInfo["BaseViewId"]) : "0";
                    uint rowLimit = viewInfo.ContainsKey("RowLimit") ? (uint)viewInfo["RowLimit"] : 0;
                    bool mobileView = viewInfo.ContainsKey("MobileView") ? (bool)viewInfo["MobileView"] : false;
                    bool mobileDefaultView = viewInfo.ContainsKey("MobileDefaultView") ? (bool)viewInfo["MobileDefaultView"] : false;
                    string contentTypeId = viewInfo.ContainsKey("ContentTypeId") ? (string)viewInfo["ContentTypeId"] : string.Empty;
                    string listViewXml = viewInfo.ContainsKey("ListViewXml") ? (string)viewInfo["ListViewXml"] : string.Empty;

                    if (ParentInfo.ParentListInfo.BaseTemplate == 10000 || personalView)  //Related Actions List还原view会抛异常,personalView不做还原；
                    {
                        restoreResult["SkippedByIsPersonalView"] = true;
                        return restoreResult;
                    }
                    #region Check View exists
                    if (mParentList != null)
                    {
                        try
                        {
                            view = GetViewByLeafName(mParentList.Views, leafName, baseViewId);
                        }
                        /*review-qlluo*/
                        catch (Exception ex)
                        {
                            mLog.Warn("Get view{0} failed.Error Message:{1}", view.ServerRelativeUrl, ex.ToString());
                        }
                    }

                    if (view == null && mParentList != null)
                    {
                        foreach (View tempView in mParentList.Views)
                        {
                            if (tempView.ServerRelativeUrl.EndsWith("/" + leafName.TrimStart('/'), StringComparison.OrdinalIgnoreCase))
                            {
                                view = tempView;
                                break;
                            }
                        }
                    }
                    #endregion

                    #region Check Conflict
                    if (view != null && mParentList != null)
                    {
                        if (!view.ViewType.Equals(viewType.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            restoreResult["OldUniqueId"] = view.Id;
                            view.DeleteObject();
                            view = null;
                        }
                        else if (!mOverWrite)
                        {
                            restoreResult["SkipViewItem"] = true;
                            restoreResult["SkipViewMessage"] = "Skip view restore, when conflict.";
                            if (viewInfo.ContainsKey("Id"))
                            {
                                lock (mDocInfo.MappingManager.SiteMappingManager.ViewGuidMapping)
                                {
                                    mDocInfo.MappingManager.SiteMappingManager.ViewGuidMapping[(Guid)viewInfo["Id"]] = view.Id;
                                    mLog.Info($"Add View Id mapping {viewInfo["Id"]} -> {view.Id}");
                                }
                                mDocInfo.AveView.Views[view.Id] = (Guid)viewInfo["Id"];
                                //viewIdMapping[(Guid)viewInfo["Id"]] = view.Id;
                            }
                            return restoreResult;
                        }
                    }
                    #endregion

                    if (view != null)
                    {
                        mFileRelativeUrl = view.ServerRelativeUrl;
                        mFileRelativePath = ResourcePath.FromDecodedUrl(mFileRelativeUrl);
                    }
                    if (view == null && mParentList != null)
                    {
                        #region Add View
                        string viewName = title;

                        if (!personalView)
                        {
                            int index = leafName.LastIndexOf('.');
                            if (index > 0)
                            {
                                viewName = leafName.Substring(0, index);
                            }
                        }
                        ViewCreationInformation creationInformation = new ViewCreationInformation();
                        creationInformation.Title = viewName;
                        creationInformation.Paged = true;
                        creationInformation.Query = string.Empty;
                        creationInformation.RowLimit = 100;
                        creationInformation.SetAsDefaultView = false;
                        creationInformation.ViewTypeKind = viewType == ViewType.Calendar ? ViewType.Calendar | ViewType.Recurrence : viewType;
                        creationInformation.PersonalView = personalView;
                        view = mParentList.Views.Add(creationInformation);
                        if (IsHiddenView(viewName))
                        {
                            view.Hidden = true;
                            view.Update();
                        }
                        mContext.Load(view);
                        mContext.Load(view, v => v.ViewFields);
                        mContext.ExecuteQuery();
                        Dictionary<string, object> viewProp = new Dictionary<string, object>();
                        AveClientOM2013Request.AssembleViewProperties(viewProp, view, ParentInfo.ParentWebInfo.ServerRelativeUrl);
                        restoreResult["View"] = viewProp;

                        restoreResult["IsNewCreated"] = true;
                        mDocInfo.IsNewCreatedView = true;
                        mFileRelativeUrl = view.ServerRelativeUrl;//view url may change since some character in the title will be escaped
                        mFileRelativePath = ResourcePath.FromDecodedUrl(mFileRelativeUrl);
                        #endregion
                    }

                    #region UpdateProperties
                    bool changed = false;

                    if (view != null)
                    {
                        if (!view.Title.Equals(title))
                        {
                            view.Title = title;
                            changed = true;
                        }
                        if (!view.Scope.Equals(scope))
                        {
                            view.Scope = scope;
                            changed = true;
                        }
                        if (!view.RowLimit.Equals(rowLimit))
                        {
                            view.RowLimit = rowLimit;
                            changed = true;
                        }
                        if (!view.MobileDefaultView.Equals(mobileDefaultView))
                        {
                            view.MobileDefaultView = mobileDefaultView;
                            changed = true;
                        }
                        if (!view.MobileView.Equals(mobileView))
                        {
                            view.MobileView = mobileView;
                            changed = true;
                        }
                        if (view.DefaultView != setAsDefaultView)
                        {
                            view.DefaultView = setAsDefaultView;
                            changed = true;
                        }
                        if (view.Hidden != hidden)
                        {
                            view.Hidden = hidden;
                            changed = true;
                        }
                        if (view.ContentTypeId.ToString() != contentTypeId && !string.IsNullOrEmpty(contentTypeId))
                        {
                            ContentTypeId newContentTypeId = new ContentTypeId();
                            Type type = newContentTypeId.GetType();
                            FieldInfo fieldInfo = type.GetField("m_stringValue", BindingFlags.NonPublic | BindingFlags.Instance);
                            fieldInfo.SetValue(newContentTypeId, contentTypeId);
                            view.ContentTypeId = newContentTypeId;
                            changed = true;
                        }
                        if (!string.Equals(view.ListViewXml, listViewXml, StringComparison.OrdinalIgnoreCase))
                        {
                            mLog.Info("ListViewXml info: Source: {0} {2} Destination: {1}", listViewXml, view.ListViewXml, System.Environment.NewLine);
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(listViewXml);
                            view.ListViewXml = doc.FirstChild.InnerXml;
                            changed = true;
                        }
                        if (changed)
                        {
                            view.Update();
                        }
                    }

                    #endregion

                    if (viewInfo.ContainsKey("Id"))
                    {
                        lock (mDocInfo.MappingManager.SiteMappingManager.ViewGuidMapping)
                        {
                            mDocInfo.MappingManager.SiteMappingManager.ViewGuidMapping[(Guid)viewInfo["Id"]] = view.Id;
                            mLog.Info($"Add View Id mapping {viewInfo["Id"]} -> {view.Id}");
                        }
                        mDocInfo.AveView.Views[view.Id] = (Guid)viewInfo["Id"];
                    }
                    if (personalView && mParentList != null)
                    {
                        restoreResult["ViewUrl"] = view.ServerRelativeUrl.Substring(ParentInfo.ParentWebInfo.ServerRelativeUrl.TrimEnd('/').Length + 1);
                    }
                    else if (view != null)
                    {
                        restoreResult["ViewUrl"] = view.ServerRelativeUrl.Substring(ParentInfo.ParentWebInfo.ServerRelativeUrl.TrimEnd('/').Length + 1);
                    }
                    restoreResult["RestoreSuccessfully"] = true;

                    try
                    {
                        ClientFile file = mParentWeb.GetFileByServerRelativePath(mFileRelativePath);

                        Dictionary<string, object> fileProp = new Dictionary<string, object>();
                        string listName = string.Empty;
                        if (ParentInfo.ParentListInfo != null)
                        {
                            fileProp["ListName"] = ParentInfo.ParentListInfo.Title;
                        }
                        if (mDocInfo.IsNewCreatedView || mOverWrite)
                        {
                            RestoreWebParts(file);
                            changed = false;//Restore WebPart中会ExecuteQuery。
                        }
                        //SAAS-4043 restore viewData & List View Xml after restore Webpart
                        if (!string.Equals(view.ViewData, viewData, StringComparison.OrdinalIgnoreCase))
                        {
                            view.ViewData = viewData;
                            changed = true;
                        }

                        if (changed)
                        {
                            view.Update();
                        }
                        mContext.Load(file);
                        mContext.ExecuteQuery();

                        if (mPageStatus == CustomizedPageStatus.Customized && file != null)
                        {
                            FileRestProcessor.AddFileByRestApi(mContext, mRequest.TokenProvider, mParentWeb.Url, mParentFolder.UniqueId, file.ServerRelativeUrl, mFileStream, true);
                        }

                        if (file != null)
                        {
                            fileProp["Exists"] = true;
                            AveClientOM2013Request.AssembleFileProperties(fileProp, file, ParentInfo.ParentWebInfo.ServerRelativeUrl, file.ListItemAllFields != null ? file.ListItemAllFields : null);
                            restoreResult["File"] = fileProp;
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn(AveClientOMRequestResource.RestoreViewError, mFileRelativeUrl, ex.ToString());
                    }
                }
                catch (Exception ex)
                {
                    restoreResult["RestoreSuccessfully"] = false;
                    restoreResult["Exception"] = string.Format("Restore view under list:{0} failed:{1}.\r\n", ParentInfo.ParentListInfo.Title, ex.ToString());
                    restoreResult["ExceptionMessage"] = ex.Message;
                }
                return restoreResult;
            }
        }

        private View GetViewByLeafName(ViewCollection views, string leafName, string baseViewId)
        {
            if (!string.IsNullOrEmpty(leafName) && views != null)
            {
                foreach (View view in views)
                {
                    if (string.Equals(leafName, GetLeafName(view.ServerRelativeUrl), StringComparison.OrdinalIgnoreCase) &&
                        view.BaseViewId == baseViewId)
                    {
                        return view;
                    }
                }
            }
            return null;
        }
        private string GetLeafName(string serverRelativeUrl)
        {
            string leafName = string.Empty;
            int pos = serverRelativeUrl.LastIndexOf('/');
            leafName = serverRelativeUrl.Substring(pos + 1);
            return leafName;
        }

        protected LimitedWebPartManager GetLimitedWebpartManager(ref ClientFile webPartPage)
        {
            LimitedWebPartManager limitedWebPartManager = webPartPage.GetLimitedWebPartManager(PersonalizationScope.Shared);
            mContext.Load(webPartPage);
            mContext.Load(limitedWebPartManager);
            mContext.Load(limitedWebPartManager, manager => manager.WebParts);
            mContext.ExecuteQuery();
            return limitedWebPartManager;
        }

        protected void RestoreWebParts(ClientFile webPartPage)
        {
            if (this.mParentList != null
                && !string.IsNullOrEmpty(this.mFileRelativeUrl)
                && (mFileRelativeUrl.EndsWith("displayifs.aspx")
                || mFileRelativeUrl.EndsWith("newifs.aspx")
                || mFileRelativeUrl.EndsWith("editifs.aspx")))
            {
                return;
            }
            if (mDocInfo.WebParts != null && mDocInfo.WebParts.Count > 0)
            {
                LimitedWebPartManager limitedWebPartManager = GetLimitedWebPartManagerWithRetry(webPartPage);
                using (AveWebPartRestore webpartRestore = new AveWebPartRestore(mRequest,
                                                                                mContext,
                                                                                mAveWebCache,
                                                                                ParentInfo,
                                                                                mParentList,
                                                                                webPartPage,
                                                                                limitedWebPartManager,
                                                                                mListItem,
                                                                                mDocInfo.WebPartCache,
                                                                                mRequest.TokenProvider,
                                                                                mIsViewPage))
                {
                    webpartRestore.InternalRestoreWebParts(webpartRestore.GetNeedRestoreWebParts(mDocInfo.WebParts, true), false);
                }
            }
        }

        private LimitedWebPartManager GetLimitedWebPartManagerWithRetry(ClientFile webPartPage)
        {
            LimitedWebPartManager limitedWebPartManager = null;
            //SAAS-39299 在GetLimitedWebpartManager的时候偶尔出现以下异常，添加retry。
            //The specified program requires a newer version of Windows. (Exception from HRESULT: 0x8007047E)
            AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, new KeyValuePair<string, string>("ServerException", "HRESULT: 0x8007047E"));
            retryHelper.ExecuteWithRetryMechanism(() =>
            {
                limitedWebPartManager = webPartPage.GetLimitedWebPartManager(PersonalizationScope.Shared);
                mContext.Load(webPartPage);
                mContext.Load(limitedWebPartManager);
                mContext.Load(limitedWebPartManager, manager => manager.WebParts);
                mContext.ExecuteQuery();
            });
            return limitedWebPartManager;
        }

        protected Dictionary<string, object> RestoreGenericFile(Dictionary<string, object> docData, Dictionary<string, object> userData, Dictionary<string, object> uniqueValues = null)
        {
            Dictionary<string, object> docRestoreResult = new Dictionary<string, object>();
            Guid oldId = Guid.Empty;
            int oldRowId = 0;
            mIsNewCreated = docData.ContainsKey("IsNewCreated") ? Convert.ToBoolean(docData["IsNewCreated"].ToString()) : false;
            #region Check Conflict
            //handle doucment has unique value
            if (!WrapperConfiguration.WrapperConfigurationForBPOS.IsEndUserRestore && ResolveUniqueFieldConflictDocument(uniqueValues, docRestoreResult))
            {
                return docRestoreResult;
            }
            //support special characters such as "#,%"
            ClientFile file = mParentWeb.GetFileByServerRelativePath(mFileRelativePath);

            bool exist = LoadFileInfo(file);

            #region Check Skip
            if (ResolveConlictByRestoreOption(exist, mIsNewCreated, file, mListItem, mDocInfo, ParentInfo, docRestoreResult))
            {
                return docRestoreResult;
            }
            #endregion

            //handle Overwrite document by LastModifiedTime.
            if (ResolveOverwriteByLastModifiedTimeDocument(exist, file, docData, docRestoreResult))
            {
                return docRestoreResult;
            }
            //handle  Declared document
            if (ResolveDeclaredDocument(exist, file, docRestoreResult))
            {
                return docRestoreResult;
            }

            if (!exist)
            {
                //TODO get the checkout document which has one version.
            }
            System.Collections.Hashtable webProperties = docData.ContainsKey("ParentWebAllProperties") ? docData["ParentWebAllProperties"] as System.Collections.Hashtable : null;
            DeleteConflictFile(docRestoreResult, ref oldRowId, file, ref exist, webProperties);
            #endregion
            if (ResolveTargetGtSourceVersion(docRestoreResult, file, exist))
            {
                return docRestoreResult;
            }
            bool needReload = false;
            int result = CreateANewFileOrVersionForArchiverRestore(ref file, docData, exist, ref needReload);
            if (needReload)
            {
                this.LoadFileInfo(file);
            }
            docRestoreResult["OriginalId"] = docData["Id"];
            docRestoreResult["OriginalRowId"] = mRowId;

            if (result == 0)
            {
                docRestoreResult["RestoreStatus"] = false;
                if (mParentList != null && mListItem != null && mListItem.FieldValues.ContainsKey("UniqueId"))
                {
                    docRestoreResult["NewId"] = mListItem["UniqueId"];
                }
                else
                {
                    docRestoreResult["NewId"] = oldId;
                }
                docRestoreResult["NewRowId"] = oldRowId;
            }
            else if (result == 1 || result == 2)
            {
                if (result == 2 && mRestoreOption != AveRestoreOption.OverWrite)
                {
                    //TODO return;
                }
                else if (mParentList != null && mItemRestore != null)
                {
                    if (mListItem != null && mListItem.FieldValues.Count > 0)
                    {
                        //UpdateModifiedAndModeration(mUserData);
                        docRestoreResult["IsNewCreated"] = true;
                        //if (mListItem.FieldValues.ContainsKey("_ModerationStatus")
                        //    && mModerationStatus != Convert.ToInt32(mListItem.FieldValues["_ModerationStatus"]))
                        //{
                        //    this.LoadFileInfo(file,false);
                        //}
                    }
                }

                if (mParentList != null && mListItem != null && mListItem.FieldValues.Count > 0)
                {
                    docRestoreResult["NewId"] = mListItem["UniqueId"];
                    docRestoreResult["NewRowId"] = mListItem.Id;
                }
            }
            docRestoreResult["RestoreStatus"] = true;
            if (mParentList != null && mListItem != null && mListItem.FieldValues.Count > 0)
            {
                docRestoreResult["RowId"] = mListItem.Id;
            }
            if (!docRestoreResult.ContainsKey("File") && file != null)
            {
                Dictionary<string, object> fileProperties = new Dictionary<string, object>();
                AveClientOM2013Request.AssembleFileProperties(fileProperties, file, ParentInfo.ParentWebInfo.ServerRelativeUrl, mListItem);
                docRestoreResult["File"] = fileProperties;
            }
            return docRestoreResult;
        }
        private bool ResolveTargetGtSourceVersion(Dictionary<string, object> docRestoreResult, ClientFile file, bool exist)
        {
            if (exist)
            {
                if (file.UIVersion > mVersion || (!mIsNewCreated && !mOverWrite))
                {
                    mLog.Warn("The version can't restore due to current version greater than the version to restore or previous version warn't restore, current version:{0}, restore version:{1}, file url:{2}", file.UIVersion, mVersion, mFileRelativeUrl);
                    docRestoreResult["SkippedItemByTargetGtSourceVersion"] = true;
                    docRestoreResult["RestoreStatus"] = true;
                    return true;
                }
            }
            return false;
        }
        private static bool ResolveConlictByRestoreOption(bool exist,bool isNewCreated, ClientFile file, ListItem item, AveDocumentInfo docInfo,DocumentRestoreInfo parentInfo,Dictionary<string,object> docRestoreResult)
        {
            if (exist && !isNewCreated && !docInfo.SettingInfo.DELETE_ITEM && docInfo.RestoreOption == (int)AveRestoreMode.Default)
            {
                if (file.Exists && item != null)
                {
                    Dictionary<string, object> fileProperties = new Dictionary<string, object>();
                    AveClientOM2013Request.AssembleFileProperties(fileProperties, file, parentInfo.ParentWebInfo.ServerRelativeUrl, item);
                    docRestoreResult["File"] = fileProperties;
                }
                docRestoreResult["IsSkipped"] = true;
                docRestoreResult["RestoreStatus"] = true;
                return true;
            }
            return false;
        }

        private void DeleteConflictFile(Dictionary<string, object> docRestoreResult, ref int oldRowId, ClientFile file, ref bool exist, System.Collections.Hashtable webProperties)
        {
            if (exist && mOverWrite && !(mParentList != null && webProperties != null && webProperties.ContainsKey("_reportinggallerytemplateid") && ParentInfo.ParentListInfo.Id.ToString().Equals((webProperties["_reportinggallerytemplateid"] as string), StringComparison.OrdinalIgnoreCase)))
            {
                if (mParentList != null && (mHasStream || ParentInfo.ParentListInfo.BaseTemplate == (int)ListTemplateType.WebPageLibrary))
                {
                    if (mListItem != null && !mHasCodeBlock)
                    {
                        try
                        {
                            oldRowId = mListItem.Id;
                            string fileUrl = this.GetRelativeUrl(file.ServerRelativeUrl);
                            if (string.Equals(ParentInfo.ParentWebInfo.RootFolderWelcomePage, fileUrl, StringComparison.OrdinalIgnoreCase))
                            {
                                mParentWeb.RootFolder.WelcomePage = string.Empty;
                                mParentWeb.RootFolder.Update();
                                mIsWelcomePageChanged = true;
                            }

                            //modify for SAAS-25372
                            //undeclare之后，file属于check in状态，由于只使用一个excute所以此时获取到的checkouttype依然为online
                            //执行undocheckout会出现异常，导致程序走catch逻辑块，所以将delete操作单独放在一个scope中
                            ExceptionHandlingScope deleteScope = new ExceptionHandlingScope(mContext);
                            using (deleteScope.StartScope())
                            {
                                using (deleteScope.StartTry())
                                {
                                    ConditionalScope scope = new ConditionalScope(mContext, () => file.CheckOutType != CheckOutType.None, true);
                                    using (scope.StartScope())
                                    {
                                        using (scope.StartIfTrue())
                                        {
                                            file.UndoCheckOut();
                                        }
                                    }
                                    if (ParentInfo.ParentListInfo.BaseTemplate == 121 && mListItem != null &&
                                        mListItem.FieldValues.ContainsKey("Status") &&
                                        mListItem.FieldValues["Status"] != null)
                                    {
                                        //deactivate solution文件比较危险，如果是active的solution，先跳过overwrite逻辑。
                                        //AveWebServiceRequest.OperateOnSolution("DEA", mContext.Url, ParentInfo.ParentWebInfo.ServerRelativeUrl, mListItem.Id, mObj);
                                    }
                                    else
                                    {
                                        if (mParentList != null && mListItem != null && mListItem.FieldValues.ContainsKey("UniqueId"))
                                        {
                                            docRestoreResult["OldUniqueId"] = mListItem["UniqueId"];
                                        }
                                        file.DeleteObject();
                                    }
                                }
                                using (deleteScope.StartCatch()) { }
                            }
                            exist = false;
                            mContext.ExecuteQuery();
                            if (deleteScope.HasException)
                            {
                                exist = true;
                                throw new Exception(deleteScope.ErrorMessage);
                            }
                        }
                        catch (Exception ex)
                        {
                            mLog.Warn("Delete file :{0} failed.Error Message:{1}", mFileRelativeUrl, ex.ToString());
                            throw;
                        }
                    }
                }
            }
        }

        private bool ResolveUniqueFieldConflictDocument(Dictionary<string, object> uniqueValues, Dictionary<string, object> docRestoreResult)
        {
            if (uniqueValues != null && uniqueValues.Count > 0)
            {
                List<ListItem> items = new List<ListItem>();
                string queryString = BuildQueryString(uniqueValues);
                CamlQuery query = new CamlQuery();
                query.FolderServerRelativePath = mParentFolderPath;
                query.DatesInUtc = true;
                query.ViewXml = queryString;
                ListItemCollection listItems = mParentList.GetItems(query);
                mParentList.Context.Load(listItems);
                mParentList.Context.ExecuteQuery();
                items.AddRange(listItems.Where(IsConflictItem));
                if (items.Count > 0 && WrapperConfiguration.WrapperConfigurationForBPOS.UniqueFieldsResolution.RestorationOption == UniqueFieldRestorationOption.Skip)
                {
                    docRestoreResult["SkippedByHasUniqueValue"] = true;
                    docRestoreResult["RestoreStatus"] = true;
                    return true;
                }
            }
            return false;
        }

        private bool ResolveOverwriteByLastModifiedTimeDocument(bool exist, ClientFile file, Dictionary<string, object> docData, Dictionary<string, object> docRestoreResult)
        {
            if (exist && mOverwriteByLastModifiedTime && docData.ContainsKey("BiggestVersionModified"))
            {
                DateTime destModified;
                if (mListItem != null && mListItem.FieldValues.ContainsKey("Modified"))
                {
                    destModified = (DateTime)mListItem["Modified"];
                }
                else
                {
                    destModified = file.TimeLastModified;
                }
                if ((DateTime)docData["BiggestVersionModified"] <= destModified)
                {
                    docRestoreResult["SkippedByLastModifiedTime"] = true;
                    docRestoreResult["RestoreStatus"] = true;
                    return true;
                }
            }
            return false;
        }

        //declare item as a record.  就不允许删除文件 在添加相同的文件 故跳过此文件
        private bool ResolveDeclaredDocument(bool exist, ClientFile file, Dictionary<string, object> docRestoreResult)
        {
            if (exist && mListItem != null && mListItem.FieldValues.ContainsKey("_vti_ItemHoldRecordStatus") && mListItem.FieldValues["_vti_ItemHoldRecordStatus"] != null)
            {
                if (mListItem.FieldValues["_vti_ItemHoldRecordStatus"].ToString() == "273")
                {
                    //overwrite或者appendNewVersion都需要做undeclare处理(AppendANewVersion只archiver模块用到)
                    if (mDocInfo.RestoreOption == (int)AveRestoreMode.AppendANewVersion || mOverWrite)
                    {
                        mLog.Info("Undeclare Item, URL:{0}", mFileRelativeUrl);
                        Records.UndeclareItemAsRecord(mContext, mListItem);
                    }
                    else
                    {
                        docRestoreResult["SkippedByDeclaredDocument"] = true;
                        docRestoreResult["RestoreStatus"] = true;
                        return true;
                    }

                }

            }
            return false;
        }

        private bool IsConflictItem(ListItem tempItem)
        {
            if (string.Equals(tempItem["FileRef"].ToString(), mFileRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                //Trim items themselves.
                return false;
            }
            return true;
        }




        private string BuildQueryString(Dictionary<string, object> uniqueValues = null)
        {
            //SAAS-10077 当<Or>和<And>同时套用2个以上的查询条件时,构造出来的query在执行时会有异常,所以采用分层嵌套的方式来构造query.
            StringBuilder viewXmlStringBuilder = new StringBuilder();
            viewXmlStringBuilder.Append("<View><Query><Where>");
            int conditionCount = 0;
            for (int i = 1; i < uniqueValues.Count; i++)
            {
                viewXmlStringBuilder.Append("<" + CamlQueryExpression.Or.ToString() + ">");
            }
            foreach (KeyValuePair<string, object> pair in uniqueValues)
            {
                AveFieldValueInfo fieldValue = pair.Value as AveFieldValueInfo;
                if (fieldValue != null)
                {
                    string colValue = fieldValue.ColValue.ToString();
                    string value = colValue.Contains(";#") ? colValue.Substring(colValue.IndexOf(";#", StringComparison.Ordinal) + 2) : colValue;
                    if (fieldValue.FieldType == AveFieldType.Invalid)
                    {
                        fieldValue.FieldType = AveFieldType.Text;
                    }
                    string fieldRef = string.Format("<Eq><FieldRef Name='{0}'/><Value Type='{1}'>{2}</Value></Eq>", pair.Key, fieldValue.FieldType, value);
                    if (conditionCount > 0)
                    {
                        fieldRef = fieldRef + "</" + CamlQueryExpression.Or.ToString() + ">";
                    }
                    conditionCount++;
                    viewXmlStringBuilder.Append(fieldRef);
                }
            }
            viewXmlStringBuilder.Append("</Where></Query></View>");
            return viewXmlStringBuilder.ToString();
        }





        public void LoadWebInfo()
        {
            try
            {
                mContext.Load(mParentWeb);
                mContext.Load(mParentWeb.RootFolder);
                //mContext.Load(mParentWeb, w => w.AllProperties);
                if (mParentList != null)
                {
                    mContext.Load(mParentList, l => l.RootFolder.Properties);

                    //mContext.Load(mParentList, l => l.BaseTemplate, l => l.RootFolder.ServerRelativeUrl);
                    mContext.Load(mParentList, l => l.BaseTemplate);
                }
                ExceptionHandlingScope excepScope = new ExceptionHandlingScope(mContext);
                using (excepScope.StartScope())
                {
                    using (excepScope.StartTry())
                    {
                        mContext.Load(mParentFolder);
                        mContext.Load(mParentFolder, f => f.ParentFolder);
                    }
                    using (excepScope.StartCatch())
                    {
                        mContext.Load(mParentFolder, f => f.UniqueId);
                        mContext.Load(mParentFolder, f => f.Name);
                        mContext.Load(mParentFolder, f => f.ParentFolder);
                        mContext.Load(mParentFolder, f => f.ServerRelativeUrl);
                        mContext.Load(mParentFolder, f => f.ContentTypeOrder);
                        mContext.Load(mParentFolder, f => f.Files);
                        mContext.Load(mParentFolder, f => f.Folders);
                    }
                }
                mContext.ExecuteQuery();//这个地方如果不执行，LoadFileInfo（）时，ExecuteQuery（）如果file不存在，这些属性就都取不到了，造成还原failed；
            }
            catch (Microsoft.SharePoint.Client.ServerException mse)
            {
                mLog.Info($"GetSite failed with ServerException.Message:{mse.Message}." +
                    $"ServerErrorCode:{mse.ServerErrorCode}." +
                    $"ServerErrorDetails:{mse.ServerErrorDetails}." +
                    $"ServerErrorTraceCorrelationId:{mse.ServerErrorTraceCorrelationId}." +
                    $"ServerErrorTypeName:{mse.ServerErrorTypeName}." +
                    $"ServerErrorValue:{mse.ServerErrorValue}." +
                    $"ServerStackTrace:{mse.ServerStackTrace}." +
                    $"Source:{mse.Source}." +
                    $"StackTrace:{mse.StackTrace}.");
                throw;
            }
        }

        public void LoadViews()
        {
            //mContext.Load(mParentList.Views, vs => vs.IncludeWithDefaultProperties(v => v.ViewFields.SchemaXml));
            mContext.Load(mParentList.Views);
            mContext.ExecuteQuery();
        }

        private bool IsHiddenView(string viewName)
        {
            string[] hiddenViewNames = new string[] { "RssView" };
            foreach (string hiddenViewName in hiddenViewNames)
            {
                if (viewName.Equals(hiddenViewName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="loadWithCaml">是否使用camlQuery来load FileInfo,使用CamlQuery Load出的时间是UTc时间，使用API在大量操作时可以防止HRESULT: 0x80131904异常</param>
        //private bool LoadFileInfo(ClientFile file, bool loadWithCaml)
        private bool LoadFileInfo(ClientFile file)
        {
            try
            {
                if (mIsFormPage || !(mOverwriteByLastModifiedTime || mMOVE_ITEM_TO_CONFLICT_FOLDER))
                {
                    return Exists(file);
                }
                else
                {
                    return IsExists(file);
                }
            }
            catch (WebException we)
            {
                ExceptionHandleUtil.HandleWebException(we);
                mLoadFileError = we.Message;
                mLog.Error("failed to load file due to: {0}", we.ToString());
                return false;
            }
            /*review-qlluo*/
            catch (ServerException se)
            {
                if (se.ServerErrorCode == -2130575282)
                {
                    throw;
                }
                if (IsMasterPageFilesThatCannotBeEditedOrRemoved(se, mFileRelativeUrl))
                {
                    throw new AveWrapperSkipException("This file under master page gallery is not editable");
                }
                ExceptionHandleUtil.HandleServerException(se);
                mLoadFileError = se.Message;
                mLog.Error("failed to load file due to: {0}", se.ToString());
                return false;
            }
            catch (Exception e)
            {
                mLoadFileError = e.Message;
                mLog.Error(AveClientOMRequestResource.LoadFileInfoError, mFileRelativeUrl, e.ToString());
                return false;
            }
        }

        public bool IsMasterPageFilesThatCannotBeEditedOrRemoved(Exception e, string fileRelativeUrl)
        {
            string[] masterPageFileExtensions = new string[] { ".master", ".js", ".html", ".css", ".aspx", ".preview" };
            if (e is ServerException && (fileRelativeUrl.Contains("/_catalogs/masterpage") || fileRelativeUrl.Contains("/Style Library/")))  //do not use error message to judge this because it's error message is internationalized
            {
                foreach (string masterPageFileExtension in masterPageFileExtensions)
                {
                    if (fileRelativeUrl.EndsWith(masterPageFileExtension))
                    {
                        return true;
                    }
                }
                return false;
            }
            else if (e.InnerException != null)
            {
                return IsMasterPageFilesThatCannotBeEditedOrRemoved(e.InnerException, fileRelativeUrl);
            }
            return false;
        }

        public bool IsConnectonForciblyClosedExceptioin(Exception te)
        {
            if (te is ServerException && te.Message.Contains("HRESULT: 0x80131904"))
            {
                return true;
            }
            else if (te is SocketException)
            {
                return true;
            }
            else if (te.InnerException != null)
            {
                return IsConnectonForciblyClosedExceptioin(te.InnerException);
            }
            return false;
        }

        public bool IsDNSUnresolvableExceptioin(Exception te)
        {
            if (te is WebException && te.Message.Contains("The remote name could not be resolved"))
            {
                return true;
            }
            else if (te is SocketException)
            {
                return true;
            }
            else if (te.InnerException != null)
            {
                return IsDNSUnresolvableExceptioin(te.InnerException);
            }
            return false;
        }

        /// <summary>
        /// load file是否存在，load出的date time是UTC时间
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private bool IsExists(ClientFile file)
        {
            ListItemCollection listItems = null;
            ListItem resultItem = null;
            //Load item时抛ServerException(HRESULT: 0x80131904)会导致listItem version还原出错及listItem的属性未初始化异常，所以在这里如果捕获到该异常进行Retry，SAAS-630 & SAAS-252
            AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, new KeyValuePair<string, string>("ServerException", "HRESULT: 0x80131904"));
            bool exist = retryHelper.ExecuteWithRetryMechanism((needReloadFile) =>
            {
                if (needReloadFile)
                {
                    //支持特殊符号%，#等
                    file = mParentWeb.GetFileByServerRelativePath(mFileRelativePath);
                }

                ConditionalScope conditionScope = new ConditionalScope(mContext, () => file.Exists, true);
                try
                {
                    using (conditionScope.StartScope())
                    {
                        using (conditionScope.StartIfTrue())
                        {
                            if (mParentList != null)
                            {
                                CamlQuery camelQueyr = new CamlQuery();
                                camelQueyr.DatesInUtc = true;
                                camelQueyr.ViewXml = string.Format("<View><Query><Where><Eq><FieldRef Name=\"FileRef\"/><Value Type=\"Lookup\">{0}</Value></Eq></Where></Query><RowLimit>1</RowLimit></View>", mFileRelativeUrl);
                                camelQueyr.FolderServerRelativePath = mParentFolderPath;
                                listItems = mParentList.GetItems(camelQueyr);
                                mContext.Load(listItems, items => items.IncludeWithDefaultProperties(item => item.HasUniqueRoleAssignments));
                            }
                            mContext.Load(file);
                            mContext.Load(file, f => f.Level);
                            mContext.Load(file, f => f.ComplianceInfo);
                        }
                    }
                    mContext.ExecuteQuery();
                }
                /*review-qlluo*/
                catch (ServerException ex)
                {
                    mLog.Warn("Load File UTC Datetime failed by query ListItem:" + ex.Message);
                    if (ex.ServerErrorCode == -2147024860)
                    {
                        mContext.Load(mParentList);
                        mContext.Load(file);
                        mContext.Load(file, f => f.Level);
                        mContext.Load(file, f => f.ComplianceInfo);
                        mContext.ExecuteQuery();
                        if (mParentList?.ItemCount > 5000)
                        {
                            int index = 0;
                            int totalCount = mParentList.ItemCount;
                            do
                            {
                                CamlQuery camlQuery = new CamlQuery();
                                camlQuery.DatesInUtc = true;
                                camlQuery.ViewXml = string.Format(
                                    "<View Scope='RecursiveAll'>" +
                                    "<Query><Where><And><Gt><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{0}</Value></Gt><Leq><FieldRef Name=\"ID\"/><Value Type=\"Integer\">{1}</Value></Leq></And></Where></Query>" +
                                    "<ViewFields><FieldRef Name='GUID' /><FieldRef Name='ID' /><FieldRef Name='Attachments'/><FieldRef Name='FileDirRef' /></ViewField>" +
                                    "<RowLimit>{2}</RowLimit>" +
                                    "</View>", index, index + 5000, 5000);
                                int lastIndex = index;
                                camlQuery.FolderServerRelativePath = mParentFolderPath;

                                ListItemCollection items = mParentList.GetItems(camlQuery);
                                mContext.Load(items, its => its.Include(it => it["FileRef"], it => it.HasUniqueRoleAssignments));
                                mContext.ExecuteQuery();
                                resultItem = items.Where(it => it["FileRef"].ToString().Equals(mFileRelativeUrl, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                                index = lastIndex + 5000 < index ? index : lastIndex + 5000;
                                totalCount -= items.Count;

                            } while (totalCount > 0 && resultItem == null);
                        }
                    }
                }
                return conditionScope.TestResult.HasValue && conditionScope.TestResult.Value;
            });
            //bool exist = conditionScope.TestResult.HasValue && conditionScope.TestResult.Value;
            mListItem = (exist && listItems != null && listItems.Count == 1) ? listItems[0] : resultItem;
            //mListItem = resultItem;
            if (exist && mListItem != null)
            {
                mListItemId = mListItem.Id;
                mLog.Info($"IsExist method: mListItem is not null. mListItemId is {mListItemId}");
            }
            mLog.Info($"IsExist method: {exist}");
            return exist;
        }

        /// <summary>
        /// load file是否存在，load出的date time不是UTC时间
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private bool Exists(ClientFile file)
        {
            //Load item时抛ServerException(HRESULT: 0x80131904)会导致listItem version还原出错及listItem的属性未初始化异常，所以在这里如果捕获到该异常进行Retry，SAAS-630 & SAAS-252
            //SAAS-14746 还原系统文件有时会抛The file exists. (Exception from HRESULT: 0x80070050)，在此添加Retry。
            AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, new KeyValuePair<string, string>("ServerException", "HRESULT: 0x80131904"), new KeyValuePair<string, string>("ServerException", "HRESULT: 0x80070050"), new KeyValuePair<string, string>("ServerException", "Save Conflict"), new KeyValuePair<string, string>("ServerException", "Please try again"));
            bool exist = retryHelper.ExecuteWithRetryMechanism((needReloadFile) =>
            {
                if (needReloadFile)
                {
                    //支持特殊符号#，%
                    file = mParentWeb.GetFileByServerRelativePath(mFileRelativePath);
                }
                ConditionalScope conditionScope = new ConditionalScope(mContext, () => file.Exists, true);
                using (conditionScope.StartScope())
                {
                    using (conditionScope.StartIfTrue())
                    {
                        if (mParentList != null)
                        {
                            mContext.Load(file.ListItemAllFields);
                            mContext.Load(file.ListItemAllFields, item => item.HasUniqueRoleAssignments);
                        }
                        mContext.Load(file);
                        mContext.Load(file, f => f.Level);
                        mContext.Load(file, f => f.ComplianceInfo);
                    }
                }
                mContext.ExecuteQuery();
                return conditionScope.TestResult.HasValue && conditionScope.TestResult.Value;
            });
            //bool exist = conditionScope.TestResult.HasValue && conditionScope.TestResult.Value;
            mListItem = mParentList != null && file.ListItemAllFields.FieldValues.Count > 0 ? file.ListItemAllFields : null;
            if (exist && mListItem != null)
            {
                mListItemId = mListItem.Id;
                mLog.Info($"Exist method: mListItem is not null. mListItemId is {mListItemId}");
            }
            mLog.Info($"Exist method: {exist}");
            return exist;
        }

        private int CreateANewFileOrVersionForArchiverRestore(ref ClientFile file, Dictionary<string, object> documentInfo, bool exist, ref bool needReload)
        {
            int compareResult = 1;

            if (exist)
            {
                if (file.UIVersion > mVersion || (!mIsNewCreated && !mOverWrite))
                {
                    compareResult = 0;// TODO
                    mLog.Warn("The version can't restore due to current version greater than the version to restore or previous version warn't restore, current version:{0}, restore version:{1}, file url:{2}", file.UIVersion, mVersion, mFileRelativeUrl);
                }
                else if (file.UIVersion == mVersion)
                {
                    if (file.Level == FileLevel.Checkout && !mIsOriginalCheckOut)
                    {
                        if (mListItem != null)  //SAAS-7142 image library中图片的缩略图file没有对应的ListItem
                        {
                            file.CheckIn(documentInfo["CheckInComment"] as string, CheckinType.MinorCheckIn);//
                            mContext.Load(file);
                            mContext.ExecuteQuery();
                        }
                        else
                        {
                            string pageUrl = AveUrlUtility.GetServerUrl(mContext.Url) + mFileRelativeUrl.TrimStart('/');
                            string webUrl = AveUrlUtility.GetServerUrl(mContext.Url) + (documentInfo["WebUrl"] as string).TrimStart('/');
                            mRequest.WebServiceRequestOnline.CheckInFile(webUrl, pageUrl, documentInfo["CheckInComment"] as string, (int)CheckinType.MinorCheckIn);
                        }
                    }
                    file = ProcessSystemFile(file);
                    compareResult = 2;
                }
                else
                {
                    file = CreateNewVersionForArchiverRestore(file, documentInfo, exist, ref needReload);
                    compareResult = 1;
                }
            }
            else
            {
                file = CreateNewVersionForArchiverRestore(file, documentInfo, exist, ref needReload);
                compareResult = 1;
            }

            return compareResult;
        }

        private ClientFile ProcessSystemFile(ClientFile file)
        {
            if (mOverWrite && mItemRestore == null)
            {
                if (mHasStream && IsGhostPageNeedOverwrite(file) && !mHasCodeBlock)
                {
                    DocumentContentProcessor.AddDocument(
                       mContext,
                       mRequest.TokenProvider,
                       mParentWeb.ServerRelativeUrl,
                       mParentFolder,
                       mFileRelativeUrl,
                       mFileStream,
                       true);
                    file = DocumentContentProcessor.LoadFile(mContext, mFileRelativeUrl);
                }
                if (WrapperConfiguration.WrapperConfigurationForBPOS.IncludeFormPageWebpart || !mIsFormPage)
                {
                    RestoreWebParts(file);
                }
            }

            return file;
        }

        //file at source side is changed or source side is not changed but the file in dest is changed, it only works for ghost page
        private bool IsGhostPageNeedOverwrite(ClientFile file)
        {
            return mPageStatus == CustomizedPageStatus.None
                || mPageStatus == CustomizedPageStatus.Customized
                || (mPageStatus == CustomizedPageStatus.Uncustomized && file.CustomizedPageStatus == CustomizedPageStatus.Customized);
        }



        private void DeleteComplianceTag(ClientFile file)
        {
            if (!string.IsNullOrWhiteSpace(file?.ComplianceInfo?.ComplianceTag))
            {
                try
                {
                    if (!file.ComplianceInfo.TagPolicyRecord && file.ComplianceInfo.TagPolicyHold && IsRecordTypeComplianceTag(file.ComplianceInfo.ComplianceTag) && WasOriginallyLocked())
                    {
                        mRequest.LockRecordItem(mParentWebUrl, mParentFolderUrl, mListItemId.ToString());
                    }
                    mRequest.SetComplianceTagOnBulkItems(mContext, mListRootFolderServerRelativeUrl, new List<int> { mListItemId }, "");
                }
                catch (Exception ex)
                {
                    mLog.Error($"Fail delete retention label,error message:{ex.Message},web url:{mParentWebUrl},listUrl:{mListRootFolderServerRelativeUrl},rowId:{mListItemId},error:{ex}");
                }
            }
        }

        protected bool IsRecordTypeComplianceTag(string complianceTagName)
        {
            try
            {
                var sitePropertyContext = SitePropertyCache.GetInstance();
                if (sitePropertyContext.AvaliableComplianceTags == null)
                {
                    sitePropertyContext.InitAvaliableComplianceTags(mContext.Url, mContext);
                }
                var complianceTag = sitePropertyContext.AvaliableComplianceTags.FirstOrDefault(info => info.TagName == complianceTagName);
                if (complianceTag != null)
                {
                    if (complianceTag.BlockDelete && complianceTag.BlockEdit)
                    {
                        return true;
                    }
                }
                else
                {
                    mLog.Warn($"Unable get complianceTag info from site avaliable compliance tags by tag name:{complianceTagName}, site url:{mContext.Url}");
                }
                return false;
            }
            catch (Exception ex)
            {
                mLog.Error($"Fail get complianceTag info from site avaliable compliance tags by tag name:{complianceTagName}, site url:{mContext.Url}, ex:{ex}");
                throw;
            }
        }

        private bool WasOriginallyLocked()
        {
            if (mUserData?.TryGetValue("_vti_ItemHoldRecordStatus", out var status) != true || status == null || !int.TryParse(status.ToString(), out var value))
            {
                return false;
            }
            return IsLocked(value);
        }

        private void SetComplianceTagIfCreateInThisJob(Dictionary<string, object> documentInfo)
        {
            if (ItemRestoreCache.IsNewCreateItem(mListId.ToString(), mListItemId.ToString()) && documentInfo.ContainsKey("ComplianceTag") && !string.IsNullOrWhiteSpace(documentInfo?["ComplianceTag"]?.ToString()))
            {
                try
                {
                    mRequest.SetComplianceTagOnBulkItems(mContext, mListRootFolderServerRelativeUrl, new List<int> { mListItemId}, documentInfo["ComplianceTag"].ToString());
                    if (WasOriginallyLocked() && IsRecordTypeComplianceTag(documentInfo["ComplianceTag"].ToString()))
                    {
                        mRequest.LockRecordItem(mParentWebUrl, mListRootFolderServerRelativeUrl, mListItemId.ToString());
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error($"Fail set retention label,label:{documentInfo["ComplianceTag"]},web url:{mParentWebUrl}, list url:{mListRootFolderServerRelativeUrl}, row id:{mListItemId},error message:{ex.Message},error:{ex}");
                    throw;                
                }
            }
        }

        private ClientFile CreateNewVersionForArchiverRestore(ClientFile file, Dictionary<string, object> documentInfo, bool exist, ref bool needReload)
        {
            AveItemUIVersion itemUIVersion = new AveItemUIVersion(mVersion);
            List<string> versionLabels = new List<string>();
            bool isFirstTime = true;
            if (mParentList != null && !WrapperConfiguration.WrapperConfigurationForBPOS.IsMultiThreadRestore)
            {
                if (mVersion % 512 > 0 && !ParentInfo.ParentListInfo.EnableMinorVersions)
                {
                    mParentList.EnableVersioning = true;
                    mParentList.EnableMinorVersions = true;
                    ParentInfo.ParentListInfo.EnableVersioning = true;
                    ParentInfo.ParentListInfo.EnableMinorVersions = true;
                    mParentList.Update();
                    ParentInfo.ParentListInfo.isListVersionSettingChanged = true;
                    mLog.Info($"CreateNewVersion mVersion % 512 > 0,ParentInfo.ParentListInfo.EnableMinorVersions is false.");
                }
            }
            if (!exist)
            {
                if (mIsOriginalCheckOut && (mVersion == 1 || mVersion == 512))
                {
                    file = AddFileWithStream(documentInfo, itemUIVersion, true);
                }
                else
                {
                    file = AddFileWithStream(documentInfo, itemUIVersion, false);
                }
                //need to consider web root folder fil,list is null
                UnLockCurrentFile(file);
                versionLabels.Add(file.UIVersionLabel);
                isFirstTime = false;
            }

            int currentMajor = file.MajorVersion;
            int currentMinor = file.MinorVersion;

            bool bApprove = false;
            int level = (int)file.Level;
            mListItemId = mListItem != null ? mListItem.Id : -1;

            if (currentMajor < itemUIVersion.MajorVersion && exist)
            {
                if (NeedCreateMajorVersionForArchiverRestore(currentMajor, itemUIVersion.MajorVersion))
                {
                    if (mIsOriginalCheckOut && (itemUIVersion.MajorVersion - currentMajor == 1) && itemUIVersion.MinorVersion == 0)
                    {
                        file = IncreaseVersion(file, documentInfo, true, true, isFirstTime, ref level, ref bApprove);
                    }
                    else
                    {
                        file = IncreaseVersion(file, documentInfo, true, false, isFirstTime, ref level, ref bApprove);
                    }
                    currentMajor++;
                    currentMinor = 0;
                    versionLabels.Add(currentMajor.ToString() + ".0");
                    isFirstTime = false;
                    needReload = true;
                    mLog.Info($"Execute query, currentMajor:{currentMajor}, majorVersion:{itemUIVersion.MajorVersion}");
                    mContext.ExecuteQuery();
                    
                    if(itemUIVersion.MinorVersion != 0)
                    {
                        KeepModifiedAndEditor(true, bApprove, level != (int)FileLevel.Checkout, true);
                    }
                }
                else
                {
                    mLog.Info($"NeedCreateMajorVersionForArchiverRestore:Skip increase version, currentMajor:{currentMajor}, majorVersion:{itemUIVersion.MajorVersion}");
                }
            }
            else
            {
                //第一个version来的时候文件不存在，默认add的是小version，因此不需要再次创建小version
                mLog.Info($"Skip increase version, currentMajor:{currentMajor}, majorVersion:{itemUIVersion.MajorVersion}");
            }

            if (itemUIVersion.MinorVersion != 0 && exist)//有小version 就加
            {
                if (currentMajor == 0 && currentMinor == 1 && !exist)
                {
                    mLog.Info($"Skip Execute query, currentMinor:{currentMinor}, minorVersion:{itemUIVersion.MinorVersion}.file.UIVersion == 1.file does not exist.");
                }
                else
                {
                    if (mIsOriginalCheckOut && (itemUIVersion.MinorVersion - currentMinor == 1))
                    {
                        file = IncreaseVersion(file, documentInfo, false, true, isFirstTime, ref level, ref bApprove);
                    }
                    else
                    {
                        file = IncreaseVersion(file, documentInfo, false, false, isFirstTime, ref level, ref bApprove);
                    }
                    currentMinor++;
                    versionLabels.Add(currentMajor.ToString() + "." + currentMinor.ToString());
                    isFirstTime = false;
                    needReload = true;
                    mLog.Info($"Execute query, currentMinor:{currentMinor}, minorVersion:{itemUIVersion.MinorVersion}");
                    mContext.ExecuteQuery();
                }
            }
            else
            {
                //第一个version来的时候文件不存在，默认add的是小version，因此不需要再次创建小version
                mLog.Info($"Skip increase version, currentMinor:{currentMinor}, minorVersion:{itemUIVersion.MinorVersion}.");
            }
            AddArchiverRestoreVersionMapping(currentMajor, currentMinor);
            if (mListItem != null && mListItem.FieldValues.ContainsKey("_dlc_BarcodeValue") && mListItem.FieldValues["_dlc_BarcodeValue"] != null)
            {
                UpdateBarcodeItemId(needReload);
            }
            if (needReload)// && !bApprove)
            {
                //Save Conflict
                DocumentContentProcessor.RetryWithSaveConflict(() =>
                {
                    mContext.Load(file);
                    mLog.Info("CreateNewVersion needReload KeepModifiedAndEditor.");
                    KeepModifiedAndEditor(false, bApprove, level != (int)FileLevel.Checkout);
                    if (mContext.HasPendingRequest)
                    {
                        try
                        {
                            mContext.ExecuteQuery();
                            mLog.Info("Update document success.");
                        }
                        catch (ServerException e)
                        {
                            if (e.Message.Contains("This item cannot be updated because it is locked as read-only."))
                            {
                                file = mParentWeb.GetFileByServerRelativePath(mFileRelativePath);
                                Records.UndeclareItemAsRecord(mContext, mListItem);
                                KeepModifiedAndEditor(false, bApprove, level != (int)FileLevel.Checkout);
                                mContext.ExecuteQuery();
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                });
            }

            if (needReload && mLevel == (int)FileLevel.Published && level != (int)FileLevel.Published
                && !bApprove
                && ParentInfo.ParentListInfo.EnableMinorVersions
                && (mLeafName.EndsWith(".master", StringComparison.OrdinalIgnoreCase)
                || mLeafName.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase)
                || mLeafName.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
                || mLeafName.EndsWith(".htm", StringComparison.OrdinalIgnoreCase)))
            {
                if (file.Level != FileLevel.Checkout)
                {
                    file.Publish(string.Empty);
                }
            }
            //删除非CurrentVersion中没能删除的Version
            if (mIsCurrentVersion && tempVersionLabels != null)
            {
                for (int i = 0; i <= tempVersionLabels.Count - 1; i++)
                {
                    try
                    {
                        file.Versions.DeleteByLabel(tempVersionLabels[i]);
                        needReload = true;
                        if (i == tempVersionLabels.Count - 1)
                        {
                            mContext.ExecuteQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("Delete versions:{0} failed.Error Message:{1}", tempVersionLabels[i], ex.ToString());
                    }
                }
                tempVersionLabels = null;
            }
            Dictionary<string, ExceptionHandlingScope> ehScopeDictionary = new Dictionary<string, ExceptionHandlingScope>();
            //删除当前还原file的多余的版本
            for (int i = 0; i < versionLabels.Count - 1; i++)
            {
                try
                {
                    if (versionLabels[i] == itemUIVersion.MajorVersion.ToString() + ".0" && file.UIVersionLabel != versionLabels[i])
                    {
                        if (tempVersionLabels == null)
                        {
                            tempVersionLabels = new List<string>();
                        }
                        tempVersionLabels.Add(versionLabels[i]);
                        continue;
                    }
                    if (mIsOriginalCheckOut && i == versionLabels.Count - 2)//对于checkout的version，我们需要keep他之前的version。
                    {
                        continue;
                    }
                    //防止删除版本时发生异常导致后续操作无法执行
                    ExceptionHandlingScope ehScope = new ExceptionHandlingScope(mContext);
                    using (ehScope.StartScope())
                    {
                        using (ehScope.StartTry())
                        {
                            file.Versions.DeleteByLabel(versionLabels[i]);
                        }
                        using (ehScope.StartCatch())
                        {

                        }
                    }
                    ehScopeDictionary[versionLabels[i]] = ehScope;
                    needReload = true;
                    if (i == versionLabels.Count - 2)
                    {
                        mContext.ExecuteQuery();
                        foreach (KeyValuePair<string, ExceptionHandlingScope> deleteVersion in ehScopeDictionary)
                        {
                            if (deleteVersion.Value.Processed && deleteVersion.Value.HasException)
                            {
                                mLog.Warn("Delete versions:{0} failed. Error Message:{1}", deleteVersion.Key, deleteVersion.Value.ErrorMessage);
                            }
                        }
                        ehScopeDictionary.Clear();
                    }
                }
                catch (Exception ex)
                {
                    mLog.Warn("Delete versions:{0} failed.Error Message:{1}", versionLabels[i], ex.ToString());
                }
            }
            SetComplianceTagIfCreateInThisJob(documentInfo);
            return file;
        }

        private bool NeedCreateMajorVersionForArchiverRestore(int currentSPMajor, int restoreFileMajor)
        {
            bool needCreateMajorVersion = false;
            if (WrapperConfiguration.WrapperConfigurationForBPOS.ArchiverRestoreVersionMapping != null && WrapperConfiguration.WrapperConfigurationForBPOS.ArchiverRestoreVersionMapping.Count != 0 && WrapperConfiguration.WrapperConfigurationForBPOS.ArchiverRestoreVersionMapping.Where(v => v.DoclibRowId == mRowId).Count() > 0)
            {
                var previousVersionMapping = WrapperConfiguration.WrapperConfigurationForBPOS.ArchiverRestoreVersionMapping.Where(v => v.DoclibRowId == mRowId).FirstOrDefault();
                if(previousVersionMapping == null)
                {
                    throw new Exception("perviousVersionMapping is null");
                }
                int previousMajorVersion = new AveItemUIVersion(previousVersionMapping.PreviousRestoreFileBackupVersion).MajorVersion;
                if (previousVersionMapping != null && previousMajorVersion == restoreFileMajor)
                {
                    needCreateMajorVersion = false;
                    mLog.Info($"NeedCreateMajorVersionForArchiverRestore fileRowID:{mRowId}, PreviousRestoreFileBackupVersion:{previousMajorVersion} equals restoreFileMajor:{restoreFileMajor}, so skip increase version.");
                }
                else if (previousVersionMapping != null && restoreFileMajor > previousMajorVersion)
                {
                    needCreateMajorVersion = true;
                    mLog.Info($"NeedCreateMajorVersionForArchiverRestore fileRowID:{mRowId}, PreviousRestoreFileBackupVersion:{previousMajorVersion} large than restoreFileMajor:{restoreFileMajor}, so increase version.");
                }
                else if (previousVersionMapping != null && restoreFileMajor < previousMajorVersion)
                {
                    needCreateMajorVersion = false;
                    mLog.Warn($"NeedCreateMajorVersionForArchiverRestore fileRowID:{mRowId}, PreviousRestoreFileBackupVersion:{previousMajorVersion} less than restoreFileMajor:{restoreFileMajor}, so skip increase version.");
                }
            }
            else
            {
                mLog.Warn($"NeedCreateMajorVersionForArchiverRestore fileRowID:{mRowId}, PreviousRestoreFileBackupVersion is null, restoreFileMajor:{restoreFileMajor}, so increase version.");
                needCreateMajorVersion = true;
            }
            return needCreateMajorVersion;
        }

        private void AddArchiverRestoreVersionMapping(int currentMajor, int currentMinor)
        {
            if (mRowId == 0 || WrapperConfiguration.WrapperConfigurationForBPOS.ArchiverRestoreVersionMapping == null)
            {
                return;
            }

            var mappingList = WrapperConfiguration.WrapperConfigurationForBPOS.ArchiverRestoreVersionMapping;
            int combinedVersion = currentMajor * 512 + currentMinor;
            var target = mappingList.FirstOrDefault(v => v.DoclibRowId == mRowId);

            if (target != null)
            {
                target.PreviousRestoreFileBackupVersion = mVersion;
                target.PreviousRestoreFileMappingVersion = combinedVersion;
                mLog.Info($"AddArchiverRestoreVersionMapping update existing mapping, fileRowID:{mRowId}, PreviousRestoreFileBackupVersion:{mVersion}, PreviousRestoreFileMappingVersion:{combinedVersion}.");
            }
            else
            {
                mappingList.Add(new ArchiverRestoreVersionMapping()
                {
                    DoclibRowId = mRowId,
                    PreviousRestoreFileBackupVersion = mVersion,
                    PreviousRestoreFileMappingVersion = combinedVersion,
                });
                mLog.Info($"AddArchiverRestoreVersionMapping add new mapping, fileRowID:{mRowId}, PreviousRestoreFileBackupVersion:{mVersion}, PreviousRestoreFileMappingVersion:{combinedVersion}.");
            }
        }

        private void UnLockCurrentFile(ClientFile file)
        {
            if (mParentList != null)
            {
                if (mParentList.RootFolder.Properties.FieldValues != null && mParentList.RootFolder.Properties.FieldValues.ContainsKey("ecm_AutoDeclareRecords")
                    && mParentList.RootFolder.Properties.FieldValues["ecm_AutoDeclareRecords"] != null && mParentList.RootFolder.Properties.FieldValues["ecm_AutoDeclareRecords"].ToString().Equals("True", StringComparison.OrdinalIgnoreCase))
                {
                    mLog.Info("Begin to process file unlock process");
                    DateTime now = DateTime.Now;
                    LoadFileInfo(file);
                    UnlockFile(file);
                    mLog.Info("Unlock file finished.Url:{0},TimeCost:{1}", mFileRelativeUrl, DateTime.Now - now);
                }
            }
            else
            {
                mLog.Debug("System file,don't have parent list.");
            }
        }

        private ClientFile AddFileWithStream(Dictionary<string, object> documentInfo, AveItemUIVersion uiVersion, bool needForceCheckout)
        {
            ClientFile file = null;

            bool forceCheckout = false;
            bool listUpdate = false;
            mLog.Info($"Add file with stream, params:mFileRelativeUrl:{SensitiveLogExtension.FormatURLInLog(mFileRelativeUrl, mRowId)}, mIsFormPage:{mIsFormPage}, mPageStatus:{mPageStatus}, IsModernPage:{IsModernPage}, mSetupPath:{mSetupPath}");
            if (mParentList != null && !WrapperConfiguration.WrapperConfigurationForBPOS.IsMultiThreadRestore)
            {
                forceCheckout = ParentInfo.ParentListInfo.ForceCheckout;
                PrepareListSettingsBeforeAddFile(uiVersion, needForceCheckout, ref listUpdate);
            }
            if (mIsFormPage && mPageStatus == CustomizedPageStatus.Uncustomized)
            {
                file = mParentFolder.Files.AddTemplateFile(mFileRelativeUrl, TemplateFileType.FormPage);
            }
            else if ((mPageStatus == CustomizedPageStatus.Uncustomized && @"DocumentTemplates\\wkpstd.aspx".Equals(mSetupPath)))
            {
                file = mParentList?.RootFolder.Files.AddTemplateFile(mFileRelativeUrl, TemplateFileType.WikiPage);
            }
            else if (IsModernPage)
            {
                file = mParentList?.RootFolder.Files.AddTemplateFile(mFileRelativeUrl, TemplateFileType.ClientSidePage);
            }
            else
            {
                DocumentContentProcessor.AddDocument(
                    mContext,
                    mRequest.TokenProvider,
                    mParentWeb.ServerRelativeUrl,
                    mParentFolder,
                    mFileRelativeUrl,
                    mFileStream,
                    true);
                file = mParentWeb.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(mFileRelativeUrl));               
            }
            ChangeBackWelcomPage();
            using (new ContextCacheDisableScope(mContext))
            {
                LoadFile(file);
            }
            DeleteComplianceTag(file);
            if (mListItem != null)
            {
                StringBuilder logBuilder = new StringBuilder();
                try
                {
                    FileLevel currentLevel = file.Level;
                    if (ParentInfo.ParentListInfo.EnableVersioning)
                    {
                        ConditionalScope conditionalScope = new ConditionalScope(mContext, () => file.Level != FileLevel.Checkout, true);
                        using (conditionalScope.StartScope())
                        {
                            using (conditionalScope.StartIfTrue())
                            {
                                logBuilder.AppendLine($"Change file level to checkout.");
                                file.CheckOut();
                                currentLevel = FileLevel.Checkout;
                                //saas-5426 由于网络原因导致chout两次后异常导致后续逻辑无法继续
                            }
                        }
                    }

                    int rowId = 0;
                    try
                    {
                        rowId = mListItem.Id;
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("Need reload ListItem.Id.");
                        mContext.Load(mListItem);
                        mContext.Load(mListItem, s => s.Id);
                        mContext.ExecuteQuery();
                        mLog.Warn($"Reloaded ListItem.Id successfully. Id: {mListItem.Id},error: {e}");
                        rowId = mListItem.Id;
                    }

                    mListItem = new ListItem(mParentList.Context, new ObjectPathMethod(mParentList.Context, mParentList.Path, "GetItemById", new object[] { rowId }));
                    AveListItemRestore.SetFieldValues(ref mListItem, mUserData, false, true);

                    if (mDocInfo.WebParts != null && mDocInfo.WebParts.Count > 0)
                    {
                        logBuilder.AppendLine($"Execute query due to DocInfo.WebParts.Count > 0");
                        mContext.Load(mListItem);
                        mContext.ExecuteQuery();
                    }
                    RestoreWebParts(file);
                    if (!needForceCheckout && currentLevel == FileLevel.Checkout)
                    {
                        if (!ParentInfo.ParentListInfo.ForceCheckout)
                        {
                            file.CheckIn(this.mCheckInComment, CheckinType.OverwriteCheckIn);
                        }
                        else
                        {
                            if (mVersion % 512 > 0 && ParentInfo.ParentListInfo.EnableMinorVersions)
                            {
                                file.CheckIn(this.mCheckInComment, CheckinType.MinorCheckIn);
                            }
                            else
                            {
                                file.CheckIn(this.mCheckInComment, CheckinType.MajorCheckIn);
                            }
                        }
                        currentLevel = FileLevel.Draft;
                    }
                    if (listUpdate)
                    {
                        logBuilder.AppendLine($"Change file level to forceCheckout:{forceCheckout}.");
                        mParentList.ForceCheckout = forceCheckout;
                        mParentList.Update();
                        mContext.Load(mParentList, list => list.EnableVersioning, list => list.EnableMinorVersions);
                    }
                    if (mLevel == (int)FileLevel.Published
                        && ParentInfo.ParentListInfo.EnableMinorVersions
                        && (mLeafName.EndsWith(".master", StringComparison.OrdinalIgnoreCase)
                        || mLeafName.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase)
                        || mLeafName.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
                        || mLeafName.EndsWith(".htm", StringComparison.OrdinalIgnoreCase)))
                    {
                        logBuilder.AppendLine($"Execute query due to file is published page && enableMinorVersions:true.");
                        file.Publish(string.Empty);
                        mContext.Load(mListItem);
                        mContext.Load(file);
                        mContext.ExecuteQuery();
                        if (mVersion == 512) // if page version is 1.0 need to keep modified and editor here.
                        {
                            KeepModifiedAndEditor(true, false, currentLevel != FileLevel.Checkout);
                        }
                    }
                    else
                    {
                        mContext.Load(file);
                        mContext.Load(mListItem);
                        KeepModifiedAndEditor(true, false, currentLevel != FileLevel.Checkout);
                        mContext.ExecuteQuery();
                    }
                }
                /*review-qlluo*/
                catch (Exception e)
                {
                    mLog.Error(logBuilder.ToString());
                    if (IsMasterPageFilesThatCannotBeEditedOrRemoved(e, mFileRelativeUrl))
                    {
                        throw new AveWrapperSkipException("This file under master page gallery is not editable");
                    }
                    else if (!string.IsNullOrEmpty(e.Message)
                        && e.Message.Contains("The operation has timed out"))
                    {
                        mLog.Warn($"An error ocurred when AddFileWithStream for this item:{mFileRelativeUrl}, re-get file and re-update file.");
                        file = mParentWeb.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(mFileRelativeUrl));
                        using (new ContextCacheDisableScope(mContext))
                        {
                            LoadFile(file);
                        }
                        //WPP一部分数据check out/check in超时失败，如果check in失败了，先尝试执行undocheckout操作，再继续执行后续还原操作.
                        try
                        {
                            mLog.Warn($"An error ocurred when AddFileWithStream for this item:{mFileRelativeUrl}, file.Level :{file.Level}.Checkout and check in this file.");
                            file.UndoCheckOut();
                            mContext.Load(file);
                            mContext.Load(mListItem);
                            mContext.ExecuteQuery();
                            mLog.Warn($"Finished retry AddFileWithStream for this item:{mFileRelativeUrl}, file.Level :{file.Level}.");
                        }
                        catch (Exception ex)
                        {
                            mLog.Warn($"Failed UndoCheckOut AddFileWithStream for this item:{mFileRelativeUrl}, file.Level :{file.Level}.Message:{ex}.");
                        }
                        try
                        {
                            mLog.Info("AddFileWithStream wtih exception and KeepModifiedAndEditor.");
                            //WPP一部分数据无论是执行check out/check in操作，还是update column value操作，都会出现time out的情况，但是从客户环境测试发现，虽然update超时，但是column value可以正常更新上去.
                            KeepModifiedAndEditor(true, false, false);
                            mContext.Load(file);
                            mContext.Load(mListItem);
                            mContext.ExecuteQuery();
                            mLog.Info("AddFileWithStream wtih exception and finished KeepModifiedAndEditor.");
                        }
                        catch (Exception ex)
                        {
                            mLog.Warn($"Failed KeepModifiedAndEditor AddFileWithStream for this item:{mFileRelativeUrl}, file.Level :{file.Level}.Message:{ex}.");
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    logBuilder.Clear();
                }
            }
            else
            {
                if (WrapperConfiguration.WrapperConfigurationForBPOS.IncludeFormPageWebpart || !mIsFormPage)
                {
                    RestoreWebParts(file);
                }
            }
            ItemRestoreCache.AddNewCreateItem(mListId.ToString(),mListItem?.Id.ToString());
            return file;
        }

        private void LoadFile(ClientFile file)
        {
            if (!this.LoadFileInfo(file))
            {
                if (!string.IsNullOrEmpty(mLoadFileError))
                {
                    throw new Exception(string.Format("An error occurred when restoring the document, load file failed:{0}.", mLoadFileError));
                }
                else
                {
                    throw new Exception("An error occurred when restoring the document, add file failed.");
                }
            }
        }

        private void ChangeBackWelcomPage()
        {
            if (mIsWelcomePageChanged)
            {
                string fileUrl = this.GetRelativeUrl(mFileRelativeUrl);
                mIsWelcomePageChanged = false;
                mParentWeb.RootFolder.WelcomePage = fileUrl;
                mParentWeb.RootFolder.Update();
                mContext.ExecuteQuery();
            }
        }

        private void PrepareListSettingsBeforeAddFile(AveItemUIVersion uiVersion, bool needForceCheckout, ref bool listUpdate)
        {
            if (needForceCheckout && !ParentInfo.ParentListInfo.ForceCheckout)
            {
                mParentList.ForceCheckout = true;
                mParentList.Update();
                listUpdate = true;
            }
        }

        private bool IsPublished(int version)
        {
            return ParentInfo.ParentListInfo.EnableMinorVersions && version % 512 == 0;
        }

        private ClientFile IncreaseVersion(ClientFile file, Dictionary<string, object> documentInfo, bool increaseMajorVersion, bool isCheckout, bool restoreContent, ref int level, ref bool bApprove)
        {
            int rowId = -1;
            if (mListItemId != -1)
            {
                rowId = mListItemId;
            }
            //int rowId = mListItem != null ? mListItem.Id : -1;

            ConditionalScope conditionalScope = new ConditionalScope(mContext, () => file.Level != FileLevel.Checkout, true);
            using (conditionalScope.StartScope())
            {
                ExceptionHandlingScope exceptionScope = new ExceptionHandlingScope(mContext);
                using (exceptionScope.StartScope())
                {
                    using (exceptionScope.StartTry())
                    {
                        file.CheckOut();
                    }
                    using (exceptionScope.StartCatch())
                    {
                        if (exceptionScope.HasException)
                        {
                            mLog.Error($"Fail checkout ,ex:{exceptionScope.ErrorMessage}," +
                                $"ex code :{exceptionScope.ServerErrorCode}, " +
                                $"ServerErrorValue:{exceptionScope.ServerErrorValue}," +
                                $"ServerErrorDetail:{exceptionScope.ServerErrorDetails}");
                        }
                        //saas-5426 由于网络原因导致chout两次后异常导致后续逻辑无法继续
                    }
                }
                level = (int)FileLevel.Checkout;
            }

            if (restoreContent && mHasStream)
            {
                if ((mPageStatus != CustomizedPageStatus.Uncustomized || file.CustomizedPageStatus != CustomizedPageStatus.Uncustomized) && !IsModernPage)
                {
                    DocumentContentProcessor.AddDocument(
                        mContext,
                        mRequest.TokenProvider,
                        mParentWeb.ServerRelativeUrl,
                        mParentFolder,
                        mFileRelativeUrl,
                        mFileStream,
                        true);
                    file =mParentWeb.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl(mFileRelativeUrl));
                }
                mListItem = new ListItem(mParentList.Context, new ObjectPathMethod(mParentList.Context, mParentList.Path, "GetItemById", new object[] { rowId }));
                AveListItemRestore.SetFieldValues(ref mListItem, mUserData, false, true);
                mContext.Load(mListItem);
                if (mDocInfo.WebParts != null)
                {
                    mContext.ExecuteQuery();
                    RestoreWebParts(file);
                }
            }
            if (!isCheckout)
            {
                CheckInFile(increaseMajorVersion, file, documentInfo, ref level, ref bApprove);
            }
            UnLockCurrentFile(file);
            return file;
        }

        private void CheckInFile(bool increaseMajorVersion, ClientFile file, Dictionary<string, object> documentInfo, ref int level, ref bool bApprove)
        {
            if (increaseMajorVersion)
            {
                file.CheckIn(mCheckInComment, CheckinType.MajorCheckIn);
                if (ParentInfo.ParentListInfo.EnableModeration)
                {
                    string moderationComments = documentInfo.ContainsKey("_ModerationComments") && documentInfo["_ModerationComments"] != null ? documentInfo["_ModerationComments"].ToString() : string.Empty;
                    file.Approve(moderationComments);
                    bApprove = true;
                }
                level = 1;
                mLog.Info("{0} version:{1} checkincomment:{2}", documentInfo.ContainsKey("Title") ? SensitiveLogExtension.FormatURLInLog(documentInfo["Title"]?.ToString()) : string.Empty, documentInfo.ContainsKey("UIVersion") ? documentInfo["UIVersion"] : string.Empty, mCheckInComment);
            }
            else
            {
                file.CheckIn(mCheckInComment, CheckinType.MinorCheckIn);
                level = 2;
                mLog.Info("{0} version:{1} checkincomment:{2}", documentInfo.ContainsKey("Title") ? SensitiveLogExtension.FormatURLInLog(documentInfo["Title"]?.ToString()) : string.Empty, documentInfo.ContainsKey("UIVersion") ? documentInfo["UIVersion"] : string.Empty, mCheckInComment);
            }
            try
            {
                mContext.ExecuteQuery();
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(ex.Message) && ex.Message.EndsWith("is not checked out."))
                {
                    mLog.Info($"File already checked in,skip check in step.ex message:{ex.Message}");
                }
                else
                {
                    throw;
                }
            }
        }

        private void UpdateModifiedAndEditor(bool excuteQuery, bool needCheckIn = true)
        {
            try
            {
                ReUpdate(excuteQuery, needCheckIn);
            }
            catch (Microsoft.SharePoint.Client.ServerException se)
            {
                mLog.Error("failed to update item due to server exception: {0}, ServerErrorCode: {1}", se.ToString(), se.ServerErrorCode);
                Thread.Sleep(3000);
                ReUpdate(excuteQuery, needCheckIn);
            }
            catch (Exception e)
            {
                mLog.Error("failed to update item due to: {0}", e.ToString());
            }
        }

        private void ReUpdate(bool excuteQuery, bool needCheckIn = true)
        {
            if (mListItem != null && !IsPublished(mVersion))
            {
                mListItem = new ListItem(mParentList.Context, new ObjectPathMethod(mParentList.Context, mParentList.Path, "GetItemById", new object[] { mListItemId }));
                if (mUserData.ContainsKey("Author"))
                {
                    mListItem["Author"] = mUserData["Author"];
                }
                if (mUserData.ContainsKey("Editor"))
                {
                    mListItem["Editor"] = mUserData["Editor"];
                }
                if (mUserData.ContainsKey("Modified"))
                {
                    mListItem["Modified"] = new DateTime(((DateTime)mUserData["Modified"]).Ticks, DateTimeKind.Utc);
                }
                if (mUserData.ContainsKey("Created"))
                {
                    mListItem["Created"] = new DateTime(((DateTime)mUserData["Created"]).Ticks, DateTimeKind.Utc);
                }
                if (!needCheckIn)
                {
                    mListItem.Update();
                }
                else
                {
                    IList<ListItemFormUpdateValue> values = new List<ListItemFormUpdateValue>();
                    values.Add(new ListItemFormUpdateValue() { FieldName = "FileLeafRef", FieldValue = mLeafName });
                    values = mListItem.ValidateUpdateListItem(values, true, "", true, true, string.Empty);

                    if (mUserData.ContainsKey("Editor"))
                    {
                        mListItem["Editor"] = mUserData["Editor"];
                        if (mUserData.ContainsKey("Modified"))
                        {
                            mListItem["Modified"] = new DateTime(((DateTime)mUserData["Modified"]).Ticks, DateTimeKind.Utc);
                        }
                        mListItem.UpdateOverwriteVersion();
                    }
                }
                if (excuteQuery)
                {
                    mContext.Load(mListItem);
                    mContext.ExecuteQuery();
                }
            }
        }

        private void KeepModifiedAndEditor(bool executeQuery, bool bApprove, bool needCheckIn = false, bool needDisableMinorVersion = false)
        {
            if (mListItem != null)
            {
                try
                {
                    //if (IsPublished(mVersion)) //can not keep modified and editor when enable majorversion 
                    bool disableMinorVersion = IsPublished(mVersion);
                    if (!WrapperConfiguration.WrapperConfigurationForBPOS.IsMultiThreadRestore)
                    {
                        try
                        {
                            ExceptionHandlingScope ehScope = new ExceptionHandlingScope(mContext);
                            using (ehScope.StartScope())
                            {
                                using (ehScope.StartTry())
                                {
                                    if (disableMinorVersion || needDisableMinorVersion)
                                    {
                                        mParentList.EnableMinorVersions = false;
                                        ParentInfo.ParentListInfo.EnableMinorVersions = false;
                                        mParentList.Update();
                                    }
                                    UpdateModifiedAndEditor(false, needCheckIn);
                                }
                                using (ehScope.StartCatch())
                                {
                                }
                                using (ehScope.StartFinally())
                                {
                                    if (disableMinorVersion || needDisableMinorVersion)
                                    {
                                        mParentList.EnableMinorVersions = true;
                                        if (ParentInfo.ParentListInfo.DraftVersionVisibility > 0 && ParentInfo.ParentListInfo.DraftVersionVisibility <= 2)//DraftVisibilityType枚举最大值为2
                                        {
                                            mParentList.DraftVersionVisibility = (DraftVisibilityType)ParentInfo.ParentListInfo.DraftVersionVisibility;
                                        }
                                        else 
                                        {
                                            mLog.Warn($"current DraftVersionVisibility is out of value,value:{ParentInfo.ParentListInfo.DraftVersionVisibility}");
                                        }
                                        ParentInfo.ParentListInfo.EnableMinorVersions = true;
                                        mParentList.Update();
                                    }
                                }
                            }
                            if (executeQuery)
                            {
                                mContext.Load(mListItem);
                                if (mListItem != null && mListItem.FieldValues != null)
                                {
                                    StringBuilder fieldsLog = new StringBuilder("KeepModifiedAndEditor mListItem.FieldValues: ");
                                    foreach (var field in mListItem.FieldValues)
                                    {
                                        fieldsLog.Append($"{field.Key}={(field.Value == null ? "null" : field.Value.ToString())}; ");
                                    }
                                    mLog.Info(fieldsLog.ToString());
                                }
                                mContext.ExecuteQuery();
                            }
                        }
                        catch (Microsoft.SharePoint.Client.ServerException e)
                        {
                            //If the error message indicates that the file is not on the current web, print the relevant information to check
                            if (e.Message.Contains("not in the current Web", StringComparison.OrdinalIgnoreCase))
                            {
                                mLog.Warn($"KeepModifiedAndEditor invalid url diagnose. ContextUrl:{mContext?.Url}, ParentWebServerRelativeUrl:{mParentWeb?.ServerRelativeUrl}, TargetFileRelativeUrl:{mFileRelativeUrl}, ValidateUpdateField:FileLeafRef={mLeafName}, ListItemFileRef:{(mListItem != null && mListItem.FieldValues != null && mListItem.FieldValues.ContainsKey("FileRef") ? mListItem["FileRef"] : null)}");
                            }
                            if (executeQuery && !disableMinorVersion)
                            {
                                mLog.Warn($"Keep modified and editor failed,will sleep 3s. error :{e}");
                                Thread.Sleep(3000);
                                UpdateModifiedAndEditor(true, needCheckIn);
                            }
                            else
                            {
                                mLog.Warn($"Keep modified and editor failed. error :{e}");
                            }
                        }
                    }
                    else if (!bApprove)
                    {
                        UpdateModifiedAndEditor(executeQuery, needCheckIn);
                    }
                }
                catch (Exception e)
                {
                    mLog.Warn("Update modified and editor fialed. error message:{0}", e.ToString());
                    mContext.Load(mListItem);
                    mContext.ExecuteQuery();
                }
            }
        }

        private string GetRelativeUrl(string fileUrl)
        {
            string fileRelativeUrl = string.Empty;
            if (mFileRelativeUrl.StartsWith(ParentInfo.ParentWebInfo.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                fileRelativeUrl = mFileRelativeUrl.Substring(ParentInfo.ParentWebInfo.ServerRelativeUrl.TrimEnd('/').Length + 1);
            }
            else
            {
                fileRelativeUrl = fileUrl;
            }
            return fileRelativeUrl;
        }

        public void Dispose()
        {
            if (mContext.HasPendingRequest)
            {
                AveAssemblyUtility.SetFieldValue(mContext, typeof(ClientRuntimeContext), "m_request", null);
            }
        }

        /// <summary>
        /// 处理将冲突文件添加到冲突文件夹下的逻辑。
        /// 1.先判断file是否存在，不存在直接返回。
        /// 2.之后判断conflict folder是否存在，不存在则建一个conflict folder
        /// 3.使用moveto方法将file移到conflict folder下
        /// 4.keep 一些属性如created time等
        /// 5.清空client api的缓存
        /// </summary>
        private void MoveToConflictFolder()
        {
            try
            {
                ClientFile file = mParentWeb.GetFileByServerRelativePath(mFileRelativePath);
                bool exist = LoadFileInfo(file);//获取list setting属性需要执行context，把loadfile拿到前边一并获取，减少通信；
                if (!ParentInfo.ParentListInfo.ServerTemplateCanCreateFolders)
                {
                    return;
                }
                if (!ParentInfo.ParentListInfo.EnableFolderCreation)
                {
                    mParentList.EnableFolderCreation = true;
                    mParentList.Update();
                }
                if (!exist)
                {
                    return;
                }
                mContext.Load(mParentFolder);
                mContext.ExecuteQuery();
                Dictionary<string, object> needKeepFields = new Dictionary<string, object>();
                needKeepFields.Add("Modified", file.TimeLastModified);
                needKeepFields.Add("Created", file.TimeCreated);
                needKeepFields.Add("Author", file.Author);
                needKeepFields.Add("Editor", file.ModifiedBy);
                string conflictFolderUrl = mParentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME;
                Microsoft.SharePoint.Client.Folder folder = mParentWeb.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(conflictFolderUrl));
                try
                {
                    mContext.Load(folder);
                    mContext.ExecuteQuery();
                }
                catch (Exception e)
                {
                    mLog.Info("Conflict folder not exist,folderUrl:{0},error:{1}", conflictFolderUrl, e.ToString());
                    folder = null;
                }
                #region --AddConflictFolder--
                if (folder == null)
                {
                    if (ParentInfo.ParentListInfo != null && ParentInfo.ParentListInfo.BaseType != (int)BaseType.DocumentLibrary)
                    {
                        ResourcePath conflictFolderPath = ResourcePath.FromDecodedUrl(conflictFolderUrl);
                        ListItemCreationInformationUsingPath creationInfoUsingPath = new ListItemCreationInformationUsingPath();
                        creationInfoUsingPath.FolderPath = conflictFolderPath;
                        creationInfoUsingPath.UnderlyingObjectType = FileSystemObjectType.Folder;
                        creationInfoUsingPath.LeafName = ResourcePath.FromDecodedUrl(AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME);
                        ListItem listItem = mParentList.AddItemUsingPath(creationInfoUsingPath);
                        listItem["Title"] = AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME;
                        listItem.Update();
                        folder = mParentWeb.GetFolderByServerRelativePath(conflictFolderPath);
                        mContext.Load(listItem);
                        mContext.Load(folder);
                        mContext.ExecuteQuery();
                    }
                    else
                    {
                        FolderCollectionAddParameters folderAddParam = new FolderCollectionAddParameters();
                        folderAddParam.Overwrite = mOverWrite;
                        folder = mParentFolder.Folders.AddUsingPath(ResourcePath.FromDecodedUrl(AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME), folderAddParam);
                        mContext.Load(folder);
                        mContext.ExecuteQuery();
                    }
                }
                #endregion
                string moveFileTitle = AveSPUtility.GetConflictNewName(file.Name, file.TimeLastModified);
                string moveFileUrl = conflictFolderUrl + "/" + moveFileTitle;
                //file.CopyTo(moveFileUrl, false);
                file.MoveTo(moveFileUrl, MoveOperations.None);

                mContext.Load(file);
                mContext.ExecuteQuery();
                if (mListItem != null)
                {
                    mItemRestore.UpdateListItem(mListItem, needKeepFields, ListItemUpdateMethodKind.SystemUpdate, false);
                }
                ClientObjectData objData = AveAssemblyUtility.GetPropertyValue(mParentWeb, "ObjectData") as ClientObjectData;
                objData.MethodReturnObjects.Clear();
            }
            catch (Exception ex)
            {
                mLog.Error("Move item:{0} to Conflict folder failed,error:{1}", (mParentFolderUrl + "/" + mName), ex.ToString());
            }
        }

        #region ReplaceBavcodeItemId
        /// <summary>
        /// Replace BarcodeimageUrl Current Item SourceItemId With DestinationItemId
        /// </summary>
        /// <param name="excuteQuery"></param>
        private void ReplaceBavcodeItemId()
        {
            try
            {
                FieldUrlValue barCodeUrl = (FieldUrlValue)mListItem.FieldValues["_dlc_BarcodePreview"];
                string newUrl = ReplaceUrlId(barCodeUrl.Url);
                barCodeUrl.Url = newUrl;
                mListItem = new ListItem(mParentList.Context, new ObjectPathMethod(mParentList.Context, mParentList.Path, "GetItemById", new object[] { mListItemId }));
                mListItem["_dlc_BarcodePreview"] = barCodeUrl;
                IList<ListItemFormUpdateValue> values = new List<ListItemFormUpdateValue>();
                values.Add(new ListItemFormUpdateValue() { FieldName = "FileLeafRef", FieldValue = mLeafName });
                values = mListItem.ValidateUpdateListItem(values, true, "", true, true, string.Empty);
            }
            catch (Exception ex)
            {
                mLog.Warn("replace barcode item id failed.due to {0}", ex.ToString());
            }
        }
        /// <summary>
        /// Update BarcodeimageUrl Current Item SourceItemId With DestinationItemId
        /// </summary>
        private void UpdateBarcodeItemId(bool needExecuteQuery)
        {
            if (needExecuteQuery) mContext.ExecuteQuery();
            if (mListItem != null && mListItem.FieldValues.ContainsKey("_dlc_BarcodePreview"))
            {
                ExceptionHandlingScope ehScope = new ExceptionHandlingScope(mContext);
                bool isPublished = IsPublished(mVersion);
                bool moderationEnabled = ParentInfo.ParentListInfo.EnableModeration;
                using (ehScope.StartScope())
                {
                    using (ehScope.StartTry())
                    {
                        if (isPublished)
                        {
                            mParentList.EnableMinorVersions = false;
                            if (moderationEnabled)
                            {
                                mParentList.EnableModeration = false;
                            }
                            mParentList.Update();
                        }
                        ReplaceBavcodeItemId();
                        mContext.Load(mListItem);
                    }
                    using (ehScope.StartFinally())
                    {
                        if (isPublished)
                        {
                            mParentList.EnableMinorVersions = true;
                            if (moderationEnabled)
                            {
                                mParentList.EnableModeration = true;
                            }
                            mParentList.Update();
                        }
                    }
                }
            }
            mContext.ExecuteQuery();
        }
        private string ReplaceUrlId(string oldUrl)
        {
            if (oldUrl.IndexOf(", Barcode") > -1)
            {
                oldUrl = oldUrl.Substring(0, oldUrl.LastIndexOf(", Barcode"));
            }
            Dictionary<string, string> idDic = new Dictionary<string, string>();
            string tempUrl = oldUrl.Substring(oldUrl.LastIndexOf('?') + 1);
            if (string.IsNullOrEmpty(tempUrl))
            {
                return oldUrl;
            }
            string idUrl = oldUrl.Substring(oldUrl.LastIndexOf('?') + 1);
            string[] ids = idUrl.Split('&');
            foreach (string id in ids)
            {
                string[] kv = id.Split('=');
                if (kv.Length == 2)
                {
                    idDic.Add(kv[0], kv[1]);
                }
            }
            foreach (KeyValuePair<string, string> kvp in idDic)
            {
                try
                {
                    if (kvp.Key.ToString().Equals("ID", StringComparison.OrdinalIgnoreCase))
                    {
                        int oldId = Convert.ToInt32(kvp.Value);
                        idUrl = idUrl.Replace("ID=" + oldId, "ID=" + mListItem.Id);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    mLog.Warn("Replace Current Item ID failed. Error:{0}", ex.Message);
                }
            }
            return oldUrl.Replace(tempUrl, idUrl);
        }
        #endregion

        protected void UnlockFile(ClientFile file)
        {
            try
            {
                if (CheckFileLockStatus(file))
                {
                    Records.UndeclareItemAsRecord(mContext, mListItem);
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Failed to unlock file. {0}", ex);
            }
        }

        protected virtual bool CheckFileLockStatus(ClientFile file)
        {
            bool locked = false;
            try
            {
                if (file.ListItemAllFields.FieldValues.ContainsKey("_vti_ItemHoldRecordStatus"))
                {
                    object status = file.ListItemAllFields["_vti_ItemHoldRecordStatus"];
                    int value = 0;
                    if (status != null && int.TryParse(status.ToString(), out value))
                    {
                        locked = IsLocked(value);
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Log(AveLogLevel.WARN, "Failed to check item lock status. Error:{0}", ex);
            }

            return locked;
        }

        protected bool IsLocked(int value)
        {
            //bool isOnHold = ((long)value & 4096L) != 0L;
            bool isRecord = ((long)value & 16L) != 0L;
            return isRecord;
            //return isOnHold || isRecord;
        }
    }
}