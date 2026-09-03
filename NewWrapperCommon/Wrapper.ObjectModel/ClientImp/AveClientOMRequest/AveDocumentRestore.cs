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


using AveClientRequest.Common;
using AvePoint.GCommon;
using AvePoint.ObjectModel.WebService;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.Client;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Application;
using Microsoft.SharePoint.Client.Utilities;
using Microsoft.SharePoint.Client.WebParts;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using ClientFile = Microsoft.SharePoint.Client.File;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveDocumentRestore : IDisposable
    {
        private AveLogger mLog = AveLogger.GetInstance(typeof(AveDocumentRestore));
        protected Site mSite;
        protected Web mParentWeb;
        protected List mParentList;
        protected string mParentFolderUrl;
        protected Folder mParentFolder;
        protected ListItem mListItem;
        protected int mRowId;
        protected string mName;
        protected int mVersion;
        protected bool mIsView;
        protected bool mIsGhostedPage;
        protected bool mHasStream;
        protected bool mOverWrite;
        protected bool mIsOriginalCheckOut;
        protected bool mListVersionSettingChanged = false;
        protected string mViewUrl;
        protected string mFileRelativeUrl;
        protected string mCheckInComment;
        protected AveRestoreOption mRestoreOption;
        protected Stream mFileStream;
        protected AveListItemRestore mItemRestore;
        protected ClientContext mContext;
        protected int mModerationStatus;
        protected object mObj;
        protected string mServerVersion;
        protected int BigFileSize = 1 * 1024 * 1024;
        protected bool mIsWelcomePageChanged = false;
        protected bool mHasPreCurrentVersion;
        protected bool mIsNewCreated;
        protected List<string> mSpecialFileList = new List<string>() { ".master", ".evtx", ".cs" };
        protected bool mMOVE_ITEM_TO_CONFLICT_FOLDER;
        protected bool mMOVE_SOURCE_TO_CONFLICT_FOLDER;
        protected bool mOverwriteByLastModifiedTime;
        protected string mListRootFolderServerRelativeUrl;
        protected AveClientOMRequest mRequest;
        protected bool mIsSystemFile;
        protected IReport mReport;
        private bool mIsCurrentMethodRetried = false;
        private IAveWeb mAveWebCache;
        AveDocumentInfo mDocInfo;

        /// <summary>
        /// 为unittest添加构造函数
        /// </summary>
        public AveDocumentRestore() { }

        public AveDocumentRestore(AveClientOMRequest request, Site site, object obj, AveClientContext conText, string serverVersion, IReport report)
        {
            mSite = site;
            mContext = conText;
            mObj = obj;
            mServerVersion = serverVersion;
            mRequest = request;
            mReport = report;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Doclib is a part of Keys")]
        protected virtual void PrepareRestoreContext(AveDocumentInfo docInfo, Stream fileStream)
        {
            mDocInfo = docInfo;
            Dictionary<string, object> data = docInfo.DocData;
            mParentWeb = mContext.Site.OpenWeb(data["WebUrl"] as string);
            mAveWebCache = data.ContainsKey("AveWebObject") ? (IAveWeb)data["AveWebObject"] : null;
            mParentFolderUrl = data["FolderUrl"] as string;
            mParentFolder = GetFolderByAPI(data["FolderUrl"] as string);
            mRowId = data.ContainsKey("DoclibRowId") ? Convert.ToInt32(data["DoclibRowId"]) : -1;
            mRestoreOption = (AveRestoreOption)data["RestoreOption"];
            mName = data["Title"] as string;
            mVersion = Convert.ToInt32(data["UIVersion"]);
            mFileRelativeUrl = mParentFolderUrl.TrimEnd('/') + "/" + mName;
            mIsView = data.ContainsKey("IsView") ? Convert.ToBoolean(data["IsView"]) : false;
            mIsGhostedPage = data.ContainsKey("IsGhostedPage") ? Convert.ToBoolean(data["IsGhostedPage"]) : false;
            mHasStream = data.ContainsKey("HasStream") ? Convert.ToBoolean(data["HasStream"]) : false;
            mOverWrite = data.ContainsKey("DeleteItem") ? Convert.ToBoolean(data["DeleteItem"]) : false;
            mIsOriginalCheckOut = data.ContainsKey("IsOriginalCheckOut") ? Convert.ToBoolean(data["IsOriginalCheckOut"]) : false;
            mCheckInComment = data.ContainsKey("CheckInComment") ? data["CheckInComment"] as string : string.Empty;
            mModerationStatus = data.ContainsKey("_ModerationStatus") ? Convert.ToInt32(data["_ModerationStatus"]) : -1;
            mHasPreCurrentVersion = data.ContainsKey("HasPreCurrentVersion") ? Convert.ToBoolean(data["HasPreCurrentVersion"]) : false;
            mMOVE_ITEM_TO_CONFLICT_FOLDER = data.ContainsKey("MOVE_ITEM_TO_CONFLICT_FOLDER") ? Convert.ToBoolean(data["MOVE_ITEM_TO_CONFLICT_FOLDER"]) : false;
            mMOVE_SOURCE_TO_CONFLICT_FOLDER = data.ContainsKey("MOVE_SOURCE_TO_CONFLICT_FOLDER") ? Convert.ToBoolean(data["MOVE_SOURCE_TO_CONFLICT_FOLDER"]) : false;//用于destination win的还原.
            mListRootFolderServerRelativeUrl = data.ContainsKey("ListRootFolderServerRelativeUrl") ? data["ListRootFolderServerRelativeUrl"] as string : string.Empty;
            mIsSystemFile = mRowId <= 0;
            mFileStream = fileStream;
            mParentList = null;
            Guid listId;
            if (TryGetListId(data, out listId))
            {
                if (!listId.Equals(Guid.Empty))
                {
                    mParentList = mParentWeb.Lists.GetById(listId);
                }
            }
            mOverwriteByLastModifiedTime = data.ContainsKey("OverwriteByLastModifiedTime") ? Convert.ToBoolean(data["OverwriteByLastModifiedTime"]) : false;
            mItemRestore = mParentList != null && mRowId > 0 ? new AveListItemRestore(mRequest, mSite, mParentWeb, mParentList, mRowId, mModerationStatus, mContext, mObj) : null;
            LoadWebInfo();
        }

        protected bool TryGetListId(Dictionary<string, object> data, out Guid listId)
        {
            listId = Guid.Empty;
            object idObj = null;
            if (data != null && data.TryGetValue("ListId", out idObj) && idObj != null)
            {
                try
                {
                    listId = new Guid(idObj as string);
                    return true;
                }
                catch (Exception e)
                {
                    this.mLog.Debug("An error occurred while trying to get list id.Error:{0}", e);
                }
            }
            return false;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "property key")]
        public virtual Dictionary<string, object> RestoreDocument(AveDocumentInfo info, Stream fileStream)
        {
            PrepareRestoreContext(info, fileStream);
            Dictionary<string, object> restoreResult = null;
            try
            {
                IAveWeb web = info.DocData.ContainsKey("AveWebObject") ? (IAveWeb)info.DocData["AveWebObject"] : null;
                if (web != null && info.OriginalRowId <= 0)
                {
                    if (info.Name.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
                    {
                        var parentFolder = web.GetFolder(info.ParentFolderRelativeUrl);
                        if (parentFolder != null &&
                            parentFolder.Exists &&
                            parentFolder.Properties.ContainsKey("_ipfs_infopathenabled") &&
                            ((string)parentFolder.Properties["_ipfs_infopathenabled"]).Equals("True", StringComparison.OrdinalIgnoreCase))
                        {
                            //InfoPath sharepoint list view的WebPart不能还原出来，需要使用web service的模拟InfoPath的publish创建出来。skip view的还原。
                            restoreResult = new Dictionary<string, object>();
                            return restoreResult;
                        }
                    }
                }
                if (mIsView)
                {
                    restoreResult = RestoreView(info.DocData["ViewInformation"] as List<Dictionary<string, object>>);
                }
                else
                {
                    //处理conflict folder
                    if (mMOVE_ITEM_TO_CONFLICT_FOLDER)
                    {
                        MoveToConflictFolder();
                    }
                    restoreResult = RestoreGenericFile(info.DocData, info.FieldsInfo.Fields);
                    //处理destination win 的conflict 转移。
                    if (mMOVE_SOURCE_TO_CONFLICT_FOLDER)
                    {
                        MoveToConflictFolder();
                    }
                }
                if (info.DocData.ContainsKey("SolutionStatus") && (int)info.DocData["SolutionStatus"] == 1)
                {
                    int id = 0;
                    if (!restoreResult.ContainsKey("RowId"))
                    {
                        id = (int)restoreResult["RowId"];
                        this.mRequest.OperateSolution("ACT", mContext.Url, mAveWebCache.ServerRelativeUrl, id);
                    }
                }
            }
            catch (Exception ex)
            {
                if (restoreResult == null)
                {
                    restoreResult = new Dictionary<string, object>();
                }
                restoreResult["Exception"] = string.Format("Restore document:{0}\\{1} failed:{2}.\r\n", mParentFolderUrl, mName, ex.ToString());
                restoreResult["ExceptionMessage"] = ex.Message;
            }

            return restoreResult;
        }

        protected virtual Dictionary<string, object> RestoreView(List<Dictionary<string, object>> viewInfoList)
        {
            if (mParentList != null)
            {
                LoadViews();
            }
            Dictionary<string, object> restoreResult = new Dictionary<string, object>();
            try
            {
                foreach (Dictionary<string, object> viewInfo in viewInfoList)
                {
                    bool personalView = viewInfo.ContainsKey("PersonalView") ? (bool)viewInfo["PersonalView"] : false;
                    if (personalView) //OFFICE 365 do not support personal view restore.
                    {
                        restoreResult["SkipViewItem"] = true;
                        restoreResult["SkipViewMessage"] = "Skip personal view restore.";
                        return restoreResult;
                    }
                    ViewType viewType = (ViewType)Enum.Parse(typeof(ViewType), viewInfo["ViewType"].ToString());
                    string leafName = viewInfo["LeafName"] as string;
                    string title = viewInfo["Title"] as string;
                    bool setAsDefaultView = viewInfo.ContainsKey("SetAsDefaultView") ? (bool)viewInfo["SetAsDefaultView"] : false;
                    #region Check View exists
                    View view = null;
                    if (mParentList != null)
                    {
                        view = GetViewByUrl(mParentList.Views, leafName);
                        if (view == null)
                        {
                            view = GetViewByTitle(mParentList.Views, title);
                        }
                    }
                    #endregion

                    #region Check Conflict
                    if (view != null && mParentList != null)
                    {
                        if (!view.ViewType.Equals(viewType.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            mParentList.GetView(view.Id).DeleteObject();
                            view = null;
                        }
                        else if (!mOverWrite && !mDocInfo.IsNewCreated)
                        {
                            restoreResult["SkipViewItem"] = true;
                            restoreResult["SkipViewMessage"] = "Skip view restore, when conflict.";
                            if (viewInfo.ContainsKey("Id"))
                            {
                                mDocInfo.MappingManager.SiteMappingManager.AddViewGuidMapping((Guid)viewInfo["Id"], view.Id);
                                mDocInfo.AveView.Views[(Guid)viewInfo["Id"]] = view.Id;
                                //viewIdMapping[(Guid)viewInfo["Id"]] = view.Id;
                            }
                            return restoreResult;
                        }
                    }
                    #endregion

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
                        //view = mParentList.Views.Add(viewName, new StringCollection(), "", 100, true, false, viewType, (bool)viewInfo["PersonalView"]);
                        ViewCreationInformation creationInformation = new ViewCreationInformation();
                        creationInformation.Title = title;//ADO-71023，view title 可以含有 . leafName则没有
                        creationInformation.Paged = true;
                        creationInformation.Query = string.Empty;
                        creationInformation.RowLimit = 100;
                        creationInformation.SetAsDefaultView = false;
                        creationInformation.ViewTypeKind = viewType;
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
                        mRequest.AssembleViewProperties(viewProp, view, mAveWebCache.ServerRelativeUrl);
                        restoreResult["View"] = viewProp;

                        mDocInfo.RestoringItem.IsNewItem = mDocInfo.IsNewCreated = true;
                        #endregion
                    }

                    #region UpdateProperties
                    bool changed = false;

                    if (view != null && !view.Title.Equals(title))
                    {
                        view.Title = title;
                        changed = true;
                    }
                    if (view != null && view.DefaultView != setAsDefaultView)
                    {
                        view.DefaultView = setAsDefaultView;
                        changed = true;
                    }

                    if (changed)
                    {
                        view.Update();
                    }
                    #endregion

                    if (viewInfo.ContainsKey("Id"))
                    {
                        mDocInfo.MappingManager.SiteMappingManager.AddViewGuidMapping((Guid)viewInfo["Id"], view.Id);
                        mDocInfo.AveView.Views[(Guid)viewInfo["Id"]] = view.Id;
                    }
                    if (personalView && mParentList != null)
                    {
                        restoreResult["ViewUrl"] = view.ServerRelativeUrl.Substring(mAveWebCache.ServerRelativeUrl.TrimEnd('/').Length + 1);
                    }
                    else if (view != null)
                    {
                        restoreResult["ViewUrl"] = view.ServerRelativeUrl.Substring(mAveWebCache.ServerRelativeUrl.TrimEnd('/').Length + 1);
                    }
                    mFileRelativeUrl = view == null ? mFileRelativeUrl : view.ServerRelativeUrl;//can't find view with url,reset file server relative url.
                }
                restoreResult["RestoreSuccessfully"] = true;

                try
                {
                    ClientFile file = mParentWeb.GetFileByServerRelativeUrl(mFileRelativeUrl);

                    Dictionary<string, object> fileProp = new Dictionary<string, object>();
                    string listName = string.Empty;
                    if (mParentList != null)
                    {
                        listName = mParentList.Title;
                        fileProp["ListName"] = listName;
                    }
                    mContext.Load(file);
                    //if (!string.IsNullOrEmpty(listName))
                    //{
                    //    mContext.Load(file, f => f.ListItemAllFields);
                    //}
                    if (mDocInfo.IsNewCreated || mOverWrite)
                    {
                        RestoreWebParts(file);
                    }
                    mContext.ExecuteQuery();

                    if (mHasStream && file != null)
                    {
                        //Microsoft.SharePoint.Client.File.SaveBinaryDirect(mContext, file.ServerRelativeUrl, mFileStream, true);
                        mRequest.SaveBinary(file.ServerRelativeUrl, mFileStream, null, true, AveClientOMRequest.SaveBinaryCheckMode.Overwrite, mContext, mObj);
                    }
                    if (file != null)
                    {
                        fileProp["Exists"] = true;
                        mRequest.AssembleFileProperties(fileProp, file, mAveWebCache.ServerRelativeUrl, null);
                        restoreResult["File"] = fileProp;
                    }
                }
                catch (Exception ex)
                {
                    mLog.Debug(AveClientOMRequestResource.RestoreViewError, mFileRelativeUrl, ex.ToString());
                }
            }
            catch (Exception ex)
            {
                restoreResult["RestoreSuccessfully"] = false;
                restoreResult["Exception"] = string.Format("Restore view under list:{0} failed:{1}.\r\n", mParentList.Title, ex.ToString());
            }

            return restoreResult;
        }

        protected LimitedWebPartManager GetLimitedWebpartManager(ref ClientFile webPartPage)
        {
            LimitedWebPartManager limitedWebPartManager = webPartPage.GetLimitedWebPartManager(PersonalizationScope.Shared);
            mContext.Load(webPartPage);
            mContext.Load(limitedWebPartManager);
            mContext.Load(limitedWebPartManager, manager => manager.WebParts);
            if (mParentList != null)
            {
                mContext.Load(mParentList);
                mContext.Load(mParentList, l => l.Views.IncludeWithDefaultProperties(v => v.ViewFields.SchemaXml));
            }
            mContext.ExecuteQuery();
            return limitedWebPartManager;
        }

        protected void RestoreWebParts(ClientFile webPartPage)
        {
            if (mDocInfo.WebParts != null && mDocInfo.WebParts.Count > 0)
            {
                ListItem webPartPageitem = mParentList != null ? webPartPage.ListItemAllFields : null;
                using (AveWebPartRestore webpartRestore = new AveWebPartRestore(mContext,
                                                                                mAveWebCache,
                                                                                mParentWeb,
                                                                                mParentList,
                                                                                webPartPage,
                                                                                GetLimitedWebpartManager(ref webPartPage),
                                                                                webPartPageitem,
                                                                                mDocInfo.WebPartCache,
                                                                                mReport,
                                                                                mObj))
                {
                    webpartRestore.InternalRestoreWebParts(webpartRestore.GetNeedRestoreWebParts(mDocInfo.WebParts, true));
                }
            }
        }

        protected void RestoreGenericFileWebParts(ClientFile webPartFile)
        {
            if (!mDocInfo.IsCurrentVersion)
            {
                return;
            }
            RestoreWebParts(webPartFile);
        }

        private View GetViewByTitle(ViewCollection views, string title)
        {
            if (views != null)
            {
                foreach (View view in views)
                {
                    if (string.Equals(title, view.Title, StringComparison.OrdinalIgnoreCase))
                    {
                        return view;
                    }
                }
            }
            return null;
        }

        private View GetViewByUrl(ViewCollection views, string leafName)
        {
            if (views == null)
            {
                return null;
            }
            foreach (View tempView in views)
            {
                if (tempView.ServerRelativeUrl.EndsWith("/" + leafName.TrimStart('/'), StringComparison.OrdinalIgnoreCase))
                {
                    return tempView;
                }
            }
            return null;
        }

        protected virtual Dictionary<string, object> RestoreGhostPage(Dictionary<string, object> docData, Dictionary<string, object> userData)
        {
            Dictionary<string, object> restoreResult = new Dictionary<string, object>();
            ClientFile file = mParentWeb.GetFileByServerRelativeUrl(mFileRelativeUrl);

            bool exist = LoadFileInfo(file);

            if (!exist)
            {
                if (mParentList != null)
                {
                    //file = (SPFile)AveAssemblyUtility.InvokeMethod(mParentFolder.Files, typeof(SPFileCollection), "AddGhosted", new object[] { docData["SetupPath"], mFileRelativeUrl, true });
                }
                else
                {
                    file = mParentWeb.GetFileByServerRelativeUrl(mFileRelativeUrl);
                }
            }

            if (mVersion == file.UIVersion)
            {
                RestoreFileProperties(file, docData["Properties"] as Dictionary<string, string>);
                if (mItemRestore != null)
                {
                    mItemRestore.UpdateListItem(ref mListItem, userData, ListItemUpdateMethodKind.CustomSystemUpdate, false);
                }
            }
            restoreResult["RestoreStatus"] = true;
            return restoreResult;
        }

        protected virtual Dictionary<string, object> RestoreGenericFile(Dictionary<string, object> docData, Dictionary<string, object> userData)
        {
            Dictionary<string, object> docRestoreResult = new Dictionary<string, object>();
            Guid oldId = Guid.Empty;
            int oldRowId = 0;
            mIsNewCreated = docData.ContainsKey("IsNewCreated") ? Convert.ToBoolean(docData["IsNewCreated"].ToString()) : false;

            #region Handle Conflict
            // file = mParentWeb.GetFileByServerRelativeUrl(mFileRelativeUrl);
            ClientFile file = new Microsoft.SharePoint.Client.File(mContext, new ObjectPathMethod(mContext, mParentWeb.Path, "GetFileByServerRelativeUrl", new object[] { mFileRelativeUrl }));
            bool exist = LoadFileInfo(file);
            if (exist)
            {
                docRestoreResult["ConflictWithDocument"] = true;
            }
            if (NeedSkipByLastModifiedTime(file, docData, exist, ref docRestoreResult))
            {
                return docRestoreResult;
            }
            if (exist)
            {
                if (SkipRestoreTopicFile(mName, mAveWebCache, mParentList))
                {
                    Dictionary<string, object> restoreResult = new Dictionary<string, object>();
                    restoreResult["SkipTopicFile"] = true;
                    restoreResult["RestoreStatus"] = true;
                    return restoreResult;
                }
            }
            TryDeleteFile(file, docData, ref oldRowId, ref exist);
            docRestoreResult["OverWriteAllVersion"] = true;
            #endregion

            bool needReload = false;
            int result = CreateANewFileOrVersion(ref file, docData, userData, exist, ref needReload);
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
                        bool needLoadFile = UpdateModeration(userData);
                        //ADO-157015 对于1.0 UpdateModeration方法后直接UpdateModified ExecuteQuery时会导致Version Conflict Exception，
                        //因此需要在 UpdateModified 之前先load 下file.ListItemAllFields属性
                        if (mVersion == 512)
                        {
                            ReloadFileListItemAllFields(file);
                        }

                        needLoadFile |= UpdateModifiedForMajorVersion(file, userData);

                        docRestoreResult["IsNewCreated"] = true;
                        if (needLoadFile)
                        {
                            this.LoadFileInfo(file);
                        }
                    }
                }

                if (mParentList != null)
                {
                    if (mListVersionSettingChanged)
                    {
                        var listVersionSetting = new Dictionary<string, object>();
                        listVersionSetting["EnableVersioning"] = mParentList.EnableVersioning;
                        listVersionSetting["EnableMinorVersions"] = mParentList.EnableMinorVersions;
                        listVersionSetting["EnableModeration"] = mParentList.EnableModeration;
                        listVersionSetting["ForceCheckout"] = mParentList.ForceCheckout;
                        docRestoreResult["ListVersionSetting"] = listVersionSetting;
                    }
                    if (mListItem != null && mListItem.FieldValues.Count > 0)
                    {
                        docRestoreResult["NewId"] = mListItem["UniqueId"];
                        docRestoreResult["NewRowId"] = mListItem.Id;
                    }
                }
                if (mOverWrite && mItemRestore == null && mHasStream && !mIsNewCreated)
                {
                    mRequest.SaveBinary(mFileRelativeUrl, mFileStream, null, true, AvePoint.ObjectModel.ClientOM.AveClientOMRequest.SaveBinaryCheckMode.Overwrite, mContext, mObj);
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
                mRequest.AssembleFileProperties(fileProperties, file, mAveWebCache.ServerRelativeUrl, mListItem);
                docRestoreResult["File"] = fileProperties;
            }
            return docRestoreResult;
        }


        private void ReloadFileListItemAllFields(ClientFile file)
        {
            try
            {
                mContext.Load(file.ListItemAllFields);
                mContext.ExecuteQuery();
            }
            catch (Exception e)
            {
                mLog.Info("An error occurred while loading ListItemAllFields, Error: {0}", e);
            }
        }
        private bool UpdateModifiedForMajorVersion(ClientFile file, Dictionary<string, object> userData)
        {
            if (mVersion % 512 != 0)
            {//10 模拟的情况下， 小Version的Modified信息无法keep
                return false;
            }
            
            if (this.mParentList.EnableVersioning)
            {
                this.mParentList.EnableVersioning = false;
                this.mParentList.EnableModeration = false;
                this.mListVersionSettingChanged = true;
                this.mParentList.Update();
            }
            var dateTime = (DateTime)userData["Modified"];
            if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }
            file.ListItemAllFields["Modified"] = dateTime;
            file.ListItemAllFields.Update();
            return true;
        }

        public virtual bool SkipRestoreTopicFile(string fileName, IAveWeb mParentWeb, List mParentList)
        {
            return false;
        }

        protected virtual bool IsPagesInMasterPage()
        {
            //If overwrite, pages in master page gallery which has no content should be overwrite.
            if (mParentList.BaseTemplate != 116) { return false; }
            return mDocInfo.Name.Equals("PeopleSearchResults.aspx") || mDocInfo.Name.Equals("SearchResults.aspx");
        }

        private void TryDeleteFile(ClientFile file, Dictionary<string, object> docData, ref int oldRowId, ref bool exist)
        {
            string reportingGalleryTemplateId = docData.ContainsKey("_reportinggallerytemplateid") ? docData["_reportinggallerytemplateid"] as string : null;
            if (exist && mOverWrite && !(mParentList != null && !string.IsNullOrEmpty(reportingGalleryTemplateId) && mParentList.Id.ToString().Equals(reportingGalleryTemplateId, StringComparison.OrdinalIgnoreCase)))
            {
                if (mParentList != null &&
                    (mHasStream ||
                     mParentList.BaseTemplate == (int)ListTemplateType.WebPageLibrary ||
                     mParentList.BaseTemplate == 850 ||//publishing pages library
                     IsPagesInMasterPage()))
                {
                    if (!mIsSystemFile)
                    {
                        try
                        {
                            if (mListItem != null)
                            {
                                oldRowId = mListItem.Id;
                            }
                            string fileUrl = this.GetRelativeUrl(file.ServerRelativeUrl);
                            if (string.Equals(mAveWebCache.RootFolder.WelcomePage, fileUrl, StringComparison.OrdinalIgnoreCase))
                            {
                                mParentWeb.RootFolder.WelcomePage = string.Empty;
                                mParentWeb.RootFolder.Update();
                                mIsWelcomePageChanged = true;
                            }
                            ExceptionHandlingScope ehScope = new ExceptionHandlingScope(mContext);
                            using (ehScope.StartScope())
                            {
                                using (ehScope.StartTry())
                                {
                                    if (file.CheckOutType != CheckOutType.None)
                                    {
                                        if (true)//file.Versions.Count != 1)//should load versions from the LoadFile
                                        {
                                            file.UndoCheckOut();
                                        }
                                    }
                                    file.DeleteObject();
                                }
                                using (ehScope.StartCatch())
                                {
                                }
                            }
                            exist = false;
                        }
                        catch (Exception ex)
                        {
                            mLog.Warn("Delete file :{0} failed.Error Message:{1}", mFileRelativeUrl, ex.ToString());
                        }
                    }
                }
            }
        }

        private bool NeedSkipByLastModifiedTime(ClientFile file, Dictionary<string, object> docData, bool exist, ref Dictionary<string, object> docRestoreResult)
        {
            if (!exist || !mOverwriteByLastModifiedTime || !docData.ContainsKey("BiggestVersionModified"))
            {
                return false;
            }
            DateTime destModified;
            if (mListItem != null && mListItem.FieldValues.ContainsKey("Modified"))
            {
                destModified = (DateTime)mListItem["Modified"];
            }
            else
            {
                destModified = file.TimeLastModified;
            }
            if ((DateTime)docData["BiggestVersionModified"] > destModified)
            {
                return false;
            }
            //Overwrite document by LastModifiedTime.
            docRestoreResult["SkippedByLastModifiedTime"] = true;
            docRestoreResult["RestoreStatus"] = true;
            Dictionary<string, object> fileProperties = new Dictionary<string, object>();
            mRequest.AssembleFileProperties(fileProperties, file, mAveWebCache.ServerRelativeUrl, mListItem);
            docRestoreResult["File"] = fileProperties;
            return true;
        }


        private bool UpdateModeration(Dictionary<string, object> userData)
        {
            DateTime originalModified = DateTime.MinValue;
            string moderationComments = string.Empty;

            if (NeedWebServiceUpdate(userData, ref originalModified, ref moderationComments))
            {
                ResetModerationStatus(mParentList);
                string webAppName = AveUrlUtility.GetServerUrl(mContext.Url);
                Dictionary<string, object> needKeepData = new Dictionary<string, object>();
                needKeepData["ModerationStatus"] = mModerationStatus;
                needKeepData["Modified"] = originalModified;
                needKeepData["ModerationComments"] = moderationComments;
                AveWebServiceRequest.UpdateListItems(webAppName, mAveWebCache.ServerRelativeUrl, mParentList.Title, mListItem.Id, mListItem.FieldValues["FileRef"].ToString(), mObj, needKeepData);
                return true;
            }
            return false;
        }

        private void ResetModerationStatus(List parentList)
        {
            if (parentList.EnableModeration && parentList.EnableMinorVersions && mModerationStatus == 2)
            {
                mModerationStatus = 3;
            }
        }

        private bool NeedWebServiceUpdate(Dictionary<string, object> userData, ref DateTime originalModified, ref string moderationComments) //还原Document时，checkout，checkin增加version会造成ModerationStatus，Modified，
        {
            originalModified = userData.ContainsKey("Modified") ? (DateTime)userData["Modified"] : DateTime.Now;
            moderationComments = userData.ContainsKey("_ModerationComments") ? userData["_ModerationComments"].ToString() : string.Empty;

            return (mListItem.FieldValues.ContainsKey("_ModerationStatus") && !mListItem.FieldValues["_ModerationStatus"].Equals(mModerationStatus)) ||//if ModerationStatus equal.
                //(mListItem.FieldValues.ContainsKey("Modified") && !mListItem.FieldValues["Modified"].Equals(originalModified)) ||//if modified equal.
                   (mListItem.FieldValues.ContainsKey("_ModerationComments") && mListItem.FieldValues["_ModerationComments"] != null && !mListItem.FieldValues["_ModerationComments"].Equals(moderationComments));
        }

        public virtual void LoadWebInfo()
        {
            //mContext.Load(mParentWeb);
            //mContext.Load(mParentWeb.RootFolder);
            //mContext.Load(mParentWeb, w => w.AllProperties);
            if (mParentList != null)
            {
                mContext.Load(mParentList);

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
                    mContext.Load(mParentFolder, f => f.Name);
                    mContext.Load(mParentFolder, f => f.ParentFolder);
                    mContext.Load(mParentFolder, f => f.ServerRelativeUrl);
                    mContext.Load(mParentFolder, f => f.ContentTypeOrder);
                    mContext.Load(mParentFolder, f => f.Files);
                    mContext.Load(mParentFolder, f => f.Folders);
                }
            }
            //mContext.ExecuteQuery();//这个地方如果不执行，LoadFileInfo（）时，ExecuteQuery（）如果file不存在，这些属性就都取不到了，造成还原failed；
        }

        public virtual void LoadViews()
        {
            mContext.Load(mParentList, l => l.Views);
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

        private bool LoadFileInfo(ClientFile file)
        {
            try
            {
                ConditionalScope conditionScope = new ConditionalScope(mContext, () => file.Exists, true);
                ListItemCollection listItems = null;
                using (conditionScope.StartScope())
                {
                    using (conditionScope.StartIfTrue())
                    {
                        if (mParentList != null)
                        {
                            CamlQuery camelQueyr = new CamlQuery();
                            camelQueyr.ViewXml = string.Format("<View><Query><Where><Eq><FieldRef Name=\"FileRef\"/><Value Type=\"Lookup\">{0}</Value></Eq></And></Where></Query></View>", mFileRelativeUrl);
                            camelQueyr.FolderServerRelativeUrl = mParentFolderUrl;
                            listItems = mParentList.GetItems(camelQueyr);
                            mContext.Load(listItems, items => items.IncludeWithDefaultProperties(item => item.HasUniqueRoleAssignments));
                        }
                        mContext.Load(file);
                    }
                }

                mContext.ExecuteQuery();

                bool exist = conditionScope.TestResult.HasValue && conditionScope.TestResult.Value;
                mListItem = (exist && listItems != null && listItems.Count == 1) ? listItems[0] : null;

                return exist;
            }
            catch (WebException ex)
            {
                mLog.Debug(AveClientOMRequestResource.LoadFileInfoError, mFileRelativeUrl, ex.ToString());
                int interval = WrapperConfiguration.BPOS_S.ClientRequestRetryInterval;
                if (!AveExceptionHelper.IsConnectionException(ex) && !AveExceptionHelper.IsHTTP429Error(ex, ref interval) || mIsCurrentMethodRetried)
                {
                    return false;
                }
                Thread.Sleep(interval);
                mIsCurrentMethodRetried = true;
                mLog.Debug("Retry Method:{0}", "LoadFileInfo");
                return AveEventHelper.Retry(delegate
                {
                    LoadWebInfo();
                    return LoadFileInfo(file);
                });
            }
            catch (Exception e)
            {
                mLog.Debug(AveClientOMRequestResource.LoadFileInfoError, mFileRelativeUrl, e.ToString());
                return false;
            }
            finally
            {
                mIsCurrentMethodRetried = false;
            }
        }

        private static void RestoreFileProperties(ClientFile file, Dictionary<string, string> properties)
        {
            bool needUpdate = false;

            foreach (KeyValuePair<string, string> keyValue in properties)
            {
                //file.Properties[keyValue.Key] = keyValue.Value;
                needUpdate = true;
            }

            if (needUpdate)
            {
                //file.Update();
            }
        }

        private bool IsSystemJpgFile(string parentFolderUrl, string fileName)
        {
            //Slide Library's hidden folder "_t" and ths files in it don't need to be restored.
            if (this.mParentList != null && this.mParentList.BaseTemplate == 2100
                && parentFolderUrl.EndsWith("_t", StringComparison.OrdinalIgnoreCase)
                && fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        private static bool CheckPathUnderFormsFolder(string url, string listRootFolerServerRelativeUrl)
        {
            bool isIn = false;

            if (url.StartsWith(listRootFolerServerRelativeUrl + "/Forms", StringComparison.OrdinalIgnoreCase))
            {
                isIn = true;
            }

            return isIn;
        }

        private int CreateANewFileOrVersion(ref ClientFile file, Dictionary<string, object> documentInfo, Dictionary<string, object> documentProperties, bool exist, ref bool needReload)
        {
            int compareResult = 1;
            int ratings = -1;
            if (documentProperties.ContainsKey("CurrentUserRatings"))
            {
                ratings = Convert.ToInt32(documentProperties["CurrentUserRatings"]);
                documentProperties.Remove("CurrentUserRatings");
            }
            if (exist)
            {
                ClientFile tempFile = file;
                if (tempFile.UIVersion > mVersion || (!mIsNewCreated && !mOverWrite))
                {
                    compareResult = 0;// TODO
                }
                else if (tempFile.UIVersion == mVersion)
                {
                    if (tempFile.UIVersion % 512 > 0)
                    {
                        if (mParentList != null && mParentList.EnableMinorVersions == false)
                        {
                            mParentList.EnableMinorVersions = true;
                            mParentList.Update();
                        }
                        if (tempFile.Level == FileLevel.Checkout && !mIsOriginalCheckOut)
                        {
                            tempFile.CheckIn(documentInfo["CheckInComment"] as string, CheckinType.MinorCheckIn);
                        }
                    }
                    //Replicator incremental restore version. Update the dest file.
                    if (!mOverWrite && mRestoreOption == AveRestoreOption.OverWrite)
                    {
                        if (!mIsOriginalCheckOut && tempFile.Level == FileLevel.Checkout)
                        {
                            int level = 255;
                            IncreaseVersion(tempFile, documentProperties, tempFile.UIVersion % 512 == 0, tempFile.UIVersion == mVersion, true, ref level);
                            needReload = true;
                        }
                    }
                    if (mOverWrite)//Skip restoring the stream of System Page in Basic Search Site
                    {
                        bool change = false;
                        bool enableVersion = false;
                        bool enableMinorVersion = false;
                        if (tempFile.UIVersion % 512 > 0)
                        {
                            CheckoutFile(tempFile, mIsSystemFile || tempFile.CheckOutType != CheckOutType.None);
                        }
                        else if (mParentList != null)
                        {
                            enableVersion = mParentList.EnableVersioning;
                            enableMinorVersion = mParentList.EnableMinorVersions;
                            if (mVersion % 512 == 0 && enableVersion)
                            {
                                mParentList.EnableVersioning = false;
                                mParentList.Update();
                                change = true;
                            }
                        }
                        if ((mHasStream || mFileStream.Length > 0) && !this.mAveWebCache.Template.Equals("SRCHCENTERLITE#0", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                tempFile = this.AddFile(mFileRelativeUrl, mFileStream, null, true, AveClientOMRequest.SaveBinaryCheckMode.Overwrite, mContext,  mObj);
                                if (this.mParentList != null)
                                {
                                    ConditionalScope conditionScope = new ConditionalScope(mContext, () => tempFile.ListItemAllFields != null, true);
                                    using (conditionScope.StartScope())
                                    {
                                        using (conditionScope.StartIfTrue())
                                        {
                                            AveListItemRestore.SetFieldValues(tempFile.ListItemAllFields, documentProperties);
                                            SetPropertiesForXSN();
                                            tempFile.ListItemAllFields.Update();
                                        }
                                    }
                                }
                                if (!LoadFileInfo(tempFile)) //Master page in use is not allowed to edit.
                                {
                                    tempFile = mParentWeb.GetFileByServerRelativeUrl(mFileRelativeUrl);
                                    LoadFileInfo(tempFile);
                                }
                            }
                            catch (Exception ex)
                            {
                                mLog.Warn("Add file stream failed.File Url:{0}, Message:{1}", mFileRelativeUrl, ex.ToString());
                            }
                        }
                        RestoreGenericFileWebParts(tempFile);
                        if (tempFile.UIVersion % 512 > 0)
                        {
                            CheckinFile(tempFile, CheckinType.OverwriteCheckIn, mIsOriginalCheckOut);
                        }
                        if (change)
                        {
                            if (enableVersion)
                            {
                                mParentList.EnableVersioning = true;
                                if (enableMinorVersion)
                                {
                                    mParentList.EnableMinorVersions = true;
                                }
                                mParentList.Update();
                                mContext.Load(mParentList, l => l.EnableVersioning, l => l.EnableMinorVersions);
                                mContext.ExecuteQuery();
                            }
                        }
                    }
                    compareResult = 2;
                }
                else
                {
                    tempFile = CreateNewVersion(tempFile, documentInfo, documentProperties, exist, ref needReload);
                    compareResult = 1;
                }
                file = tempFile;
            }
            else
            {
                file = CreateNewVersion(file, documentInfo, documentProperties, exist, ref needReload);
                compareResult = 1;
            }
            if (string.Compare(mServerVersion, "15.", StringComparison.OrdinalIgnoreCase) < 0 && ratings != -1)
            {
                using (AveWebServiceRequest webServiceRequest = new AveWebServiceRequest(mRequest.Url, mRequest.mUserAccountInfo, mObj, mServerVersion))
                {
                    string itemUrl = mRequest.Url.Substring(0, mRequest.Url.IndexOf(mDocInfo.DocData["WebUrl"] as string, StringComparison.OrdinalIgnoreCase)) + mFileRelativeUrl;
                    mContext.Load(mSite, site => site.Id);
                    mContext.Load(mParentWeb, web => web.Id);
                    mContext.ExecuteQuery();
                    webServiceRequest.SetListItemRatings(itemUrl, mName, ratings, mSite.Id, mParentWeb.Id);
                }
            }
            return compareResult;
        }

        private ClientFile CreateNewVersion(ClientFile file, Dictionary<string, object> documentInfo, Dictionary<string, object> documentProperties, bool exist, ref bool needReload)
        {
            AveItemUIVersion itemUIVersion = new AveItemUIVersion(mVersion);
            List<string> versionLabels = new List<string>();

            if (!exist)
            {
                bool needCheckout = mIsOriginalCheckOut && (mVersion == 1 || mVersion == 512);
                file = AddFileWithStream(documentInfo, documentProperties, itemUIVersion, needCheckout);
                versionLabels.Add(file.UIVersionLabel);
            }
            int currentMajor = file.MajorVersion;
            int currentMinor = file.MinorVersion;
            int level = (int)file.Level;
            CreateNewMajorVersion(file, itemUIVersion, documentProperties, versionLabels, ref level, ref currentMajor, ref currentMinor, ref needReload);
            CreateNewMinorVersion(file, itemUIVersion, documentProperties, versionLabels, ref level, ref currentMajor, ref currentMinor, ref needReload);
            DeleteUnnecessaryVersion(file, itemUIVersion, versionLabels, ref needReload);
            return file;
        }

        private void CreateNewMajorVersion(ClientFile file, AveItemUIVersion itemUIVersion, Dictionary<string, object> documentProperties, List<string> versionLabels, ref int level, ref int currentMajor, ref int currentMinor, ref bool needReload)
        {
            bool change = false;
            bool isCurrentRestoredVersion = false;
            bool update = true;
            while (currentMajor < itemUIVersion.MajorVersion)
            {
                lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ListSettingLocker"))
                {
                    if (mParentList != null && !change)
                    {
                        PrepareListSettingsBeforeIncreaseFileVersion(true);
                        mContext.Load(mParentList, list => list.EnableVersioning, list => list.EnableMinorVersions);
                        mContext.ExecuteQuery();
                    }
                }
                isCurrentRestoredVersion = ++currentMajor == itemUIVersion.MajorVersion && itemUIVersion.MinorVersion == 0;
                update = isCurrentRestoredVersion && (mIsOriginalCheckOut || mVersion % 512 > 0);
                file = IncreaseVersion(file, documentProperties, true, isCurrentRestoredVersion, update, ref level);
                change = true;
                versionLabels.Add(currentMajor.ToString() + ".0");
                needReload = true;
            }
            if (isCurrentRestoredVersion && !update)
            {
                UpdateDocumentAndRestoreWebparts(file, documentProperties);
            }
            if (change)
            {
                currentMinor = 0;
            }
            else
            {
                currentMinor = file.MinorVersion;
            }
        }

        private void CreateNewMinorVersion(ClientFile file, AveItemUIVersion itemUIVersion, Dictionary<string, object> documentProperties, List<string> versionLabels, ref int level, ref int currentMajor, ref int currentMinor, ref bool needReload)
        {
            bool change = false;
            bool isCurrentRestoredVersion = false;
            while (currentMinor < itemUIVersion.MinorVersion)
            {
                lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ListSettingLocker"))
                {
                    if (mParentList != null && !change)
                    {
                        PrepareListSettingsBeforeIncreaseFileVersion(false);
                        mContext.Load(mParentList, list => list.EnableVersioning, list => list.EnableMinorVersions);
                        mContext.ExecuteQuery();
                    }
                }
                isCurrentRestoredVersion = ++currentMinor == itemUIVersion.MinorVersion;
                file = IncreaseVersion(file, documentProperties, false, isCurrentRestoredVersion, isCurrentRestoredVersion, ref level);
                versionLabels.Add(currentMajor.ToString() + "." + currentMinor.ToString());
                needReload = true;
                change = true;
            }
        }

        private void DeleteUnnecessaryVersion(ClientFile file, AveItemUIVersion itemUIVersion, List<string> versionLabels, ref bool needReload)
        {
            for (int i = 0; i < versionLabels.Count - 1; i++)
            {
                try
                {
                    if (versionLabels[i] == itemUIVersion.MajorVersion.ToString() + ".0" ||
                        (mIsOriginalCheckOut && i == versionLabels.Count - 2))      //For checkout version, keep the previous version
                    {
                        continue;
                    }
                    file.Versions.DeleteByLabel(versionLabels[i]);
                    needReload = true;
                }
                catch (Exception ex)
                {
                    mLog.Warn("Delete versions:{0} failed.Error Message:{1}", versionLabels[i], ex.ToString());
                }
            }
        }
        private bool IsWebPartPage(Dictionary<string, object> documentProperties)
        {
            if ((documentProperties.ContainsKey("HTML_x0020_File_x0020_Type") && documentProperties["HTML_x0020_File_x0020_Type"] != null &&
                documentProperties["HTML_x0020_File_x0020_Type"].Equals("SharePoint.WebPartPage.Document")))
            {
                return true;
            }
            return false;
        }
        private ClientFile AddFileWithStream(Dictionary<string, object> documentInfo, Dictionary<string, object> documentProperties, AveItemUIVersion uiVersion, bool needForceCheckout)
        {
            ClientFile file = null;
            bool listUpdate = false;
            bool forceCheckout = false;
            if (mParentList != null)
            {
                forceCheckout = mParentList.ForceCheckout;
            }
            bool needAddFile = true;
            lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ListSettingLocker"))
            {
                if (mParentList != null)
                {
                    PrepareListSettingsBeforeAddFile(uiVersion, needForceCheckout, ref listUpdate);
                    if (!IsWebPartPage(documentProperties))//WebPartPage can not be added with API.
                    {
                        HandleWikiPage(ref file, ref needAddFile);
                    }
                    mContext.Load(mParentList, list => list.ForceCheckout, list => list.EnableVersioning, list => list.EnableMinorVersions);
                    mContext.ExecuteQuery();
                }
            }
            bool isCurrentRestoredVersion = false;
            HandleDocumentStream(ref file, GetFileStreamUpdateOption(needAddFile, ref isCurrentRestoredVersion));
            if (isCurrentRestoredVersion)
            {
                if (mVersion % 512 > 0)
                {
                    CheckoutFile(file, mIsSystemFile);
                }
                //this.LoadFileInfo(file);//Get listItem from the new file.                
                UpdateDocumentAndRestoreWebparts(file, documentProperties);
                //UpdateDocumentProperties(file, documentProperties);
                //RestoreGenericFileWebParts(file);
                if (mVersion % 512 > 0)
                {
                    CheckinFile(file, GetCheckinType(), needForceCheckout);
                }
            }
            if (listUpdate)
            {
                mParentList.ForceCheckout = forceCheckout;
                mParentList.Update();
                mContext.Load(mParentList, list => list.EnableVersioning, list => list.EnableMinorVersions);
            }
            if (!this.LoadFileInfo(file))//reload file after being updated
            {
                this.LoadFileInfo(file);
            }

            return file;
        }

        private bool NeedSkipCheckout()
        {
            return CheckPathUnderFormsFolder(mFileRelativeUrl, mListRootFolderServerRelativeUrl) ||
                   IsSystemJpgFile(mParentFolderUrl, mFileRelativeUrl);
        }

        private FileStreamUpdateOption GetFileStreamUpdateOption(bool needAddFile, ref bool isCurrentRestoredVersion)
        {
            FileStreamUpdateOption fileStreamUpdateOption = new FileStreamUpdateOption()
            {
                NeedUpdateStream = needAddFile,
                FileStreamUpdateType = default(FileStreamUpdateType)
            };
            if (mParentList == null)
            {
                isCurrentRestoredVersion = true;
                fileStreamUpdateOption.FileStreamUpdateType = FileStreamUpdateType.Custom;
                return fileStreamUpdateOption;
            }
            if (mVersion == 1 ||
                (mVersion == 512 && (!mParentList.EnableVersioning || !mParentList.EnableMinorVersions)))
            {
                isCurrentRestoredVersion = true;
                fileStreamUpdateOption.FileStreamUpdateType = FileStreamUpdateType.Custom;
                return fileStreamUpdateOption;
            }
            return fileStreamUpdateOption;
        }

        private void HandleWikiPage(ref ClientFile file, ref bool needAddFile)
        {
            if (!mHasStream && mParentList != null && mParentList.BaseTemplate == (int)ListTemplateType.WebPageLibrary)
            {
                //file = mParentFolder.Files.AddTemplateFile(mFileRelativeUrl, TemplateFileType.WikiPage);
                file = mParentList.RootFolder.Files.AddTemplateFile(mFileRelativeUrl, TemplateFileType.WikiPage);
                if (mIsWelcomePageChanged)
                {
                    string fileUrl = this.GetRelativeUrl(mFileRelativeUrl);
                    mIsWelcomePageChanged = false;
                    mParentWeb.RootFolder.WelcomePage = fileUrl;
                    mParentWeb.RootFolder.Update();
                }
                needAddFile = false;
            }
        }

        private void HandleDocumentStream(ref ClientFile file, FileStreamUpdateOption fileStreamUpdateOption)
        {
            if (!fileStreamUpdateOption.NeedUpdateStream)
            {
                return;
            }
            //string etagNew;                
            //file.SaveBinary(mFileStream, false, true, null, null, null, out etagNew);
            //Microsoft.SharePoint.Client.File.SaveBinaryDirect(mContext, file.ServerRelativeUrl, mFileStream, true);
            Stream fileStream = fileStreamUpdateOption.FileStreamUpdateType == default(FileStreamUpdateType) ? new MemoryStream() : mFileStream;
            file = this.AddFile(mFileRelativeUrl, fileStream, null, true, AveClientOMRequest.SaveBinaryCheckMode.Overwrite, mContext, mObj);
            if (mContext.HasPendingRequest)
            {
                this.mContext.ExecuteQuery();
            }
            this.mContext.Load(file);
            this.mContext.ExecuteQuery();
        }

        private CheckinType GetCheckinType()
        {
            CheckinType checkinType = CheckinType.OverwriteCheckIn;
            if (mParentList != null && !mParentList.ForceCheckout)
            {
                return checkinType;
            }
            if (mVersion < 512 || mParentList.EnableMinorVersions)
            {
                checkinType = CheckinType.MinorCheckIn;
            }
            else
            {
                checkinType = CheckinType.MajorCheckIn;
            }
            return checkinType;
        }

        private void CheckinFile(ClientFile file, CheckinType checkinType, bool needCheckout)
        {
            if (needCheckout)
            {
                return;
            }
            ConditionalScope checkinScope = new ConditionalScope(mContext, () => file.Level == FileLevel.Checkout, true);
            using (checkinScope.StartScope())
            {
                using (checkinScope.StartIfTrue())
                {
                    file.CheckIn(mCheckInComment, checkinType);
                }
            }
        }

        private void CheckoutFile(ClientFile file, bool needSkipCheckout = false)
        {
            if (needSkipCheckout)
            {
                return;
            }
            ConditionalScope checkoutFileCondition = new ConditionalScope(mContext, () => file.Level != FileLevel.Checkout, true);
            using (checkoutFileCondition.StartScope())
            {
                using (checkoutFileCondition.StartIfTrue())
                {
                    file.CheckOut();
                }
            }
        }

        private void UpdateDocumentAndRestoreWebparts(ClientFile file, Dictionary<string, object> documentProperties)
        {
            SetPropertiesForXSN();
            if (mItemRestore == null || mIsSystemFile)
            {
                return;
            }
            bool change = false;
            bool enableVersion = false;
            bool enableMinorVersion = false;
            if (mParentList != null)
            {
                enableVersion = mParentList.EnableVersioning;
                enableMinorVersion = mParentList.EnableMinorVersions;
                if (mVersion % 512 == 0 && enableVersion)
                {
                    mParentList.EnableVersioning = false;
                    mParentList.Update();
                    change = true;
                }
            }
            ActionBeforeSetValue(file.ListItemAllFields, documentProperties);
            AveListItemRestore.SetFieldValues(file.ListItemAllFields, documentProperties);
            SetUserDataJunctionFieldValues(file);
            file.ListItemAllFields.Update();
            RestoreGenericFileWebParts(file);
            ActionAfterSetValue(file.ListItemAllFields, documentProperties);
            if (change && mParentList != null)
            {
                if (enableVersion)
                {
                    mParentList.EnableVersioning = true;
                    if (enableMinorVersion)
                    {
                        mParentList.EnableMinorVersions = true;
                    }
                    mParentList.Update();
                    mContext.Load(mParentList, l => l.EnableVersioning, l => l.EnableMinorVersions);
                }
            }
        }

        #region for xsn document
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ipfs_streamhash:Document metadata")]
        protected void SetPropertiesForXSN()
        {
            if (mDocInfo.Name.EndsWith(".xsn", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(mDocInfo.XSNStreamHashValue))
            {
                var properties = new Dictionary<string, object>();
                var values = new Dictionary<string, object>();
                values.Add("ipfs_streamhash", mDocInfo.XSNStreamHashValue);
                properties.Add("ChangedMetaInfo", values);
                mRequest.UpdateFile(mDocInfo.ParentWebRelativeUrl, mDocInfo.ParentListTitle, mDocInfo.ServerRelativeUrl, properties);
            }
        }
        #endregion

        private void ActionBeforeSetValue(ListItem item, Dictionary<string, object> documentProperties)
        {
            SetEditorReadOnly(true);
            UpdateEditor(item, documentProperties);
            SetEditorReadOnly(false);
            SetModerationStatus(documentProperties);
        }

        protected virtual void UpdateEditor(ListItem item, Dictionary<string, object> documentProperties)
        {
            if (!documentProperties.ContainsKey("Editor"))
            {
                return;
            }
            item["Editor"] = documentProperties["Editor"];
            item["Author"] = documentProperties["Author"];
            item.Update();
            documentProperties.Remove("Editor");
        }

        private void SetModerationStatus(Dictionary<string, object> documentProperties)
        {
            if (!mParentList.EnableModeration)
            {
                documentProperties["_ModerationStatus"] = mModerationStatus;
            }
        }

        private void ActionAfterSetValue(ListItem item, Dictionary<string, object> documentProperties)
        {
            SetEditorReadOnly(true);
        }

        protected virtual void SetEditorReadOnly(bool readOnly)//Keep SP2010 editor field value.
        {
            if (mParentList == null)
            {
                return;
            }
            Field editorField = mParentList.Fields.GetById(AveBuiltInFieldId.Editor);
            editorField.ReadOnlyField = readOnly;
            editorField.Update();
        }

        protected void SetUserDataJunctionFieldValues(ClientFile file)
        {
            if (mDocInfo.FieldsInfo.MultiLookupFields == null)
            {
                return;
            }
            foreach (KeyValuePair<string, object> fieldInfo in mDocInfo.FieldsInfo.MultiLookupFields)
            {
                file.ListItemAllFields[fieldInfo.Key] = fieldInfo.Value.ToString();
            }
        }

        private void PrepareListSettingsBeforeAddFile(AveItemUIVersion uiVersion, bool needForceCheckout, ref bool listUpdate)
        {
            if (mParentList != null)
            {
                if (needForceCheckout && !mParentList.ForceCheckout)
                {
                    mParentList.ForceCheckout = true;
                    mParentList.Update();
                    listUpdate = true;
                }
                else if (!needForceCheckout && mParentList.ForceCheckout)
                {
                    mParentList.ForceCheckout = false;
                    mParentList.Update();
                    listUpdate = true;
                }
            }
            if (uiVersion.UIVersion < 512)
            {
                if (mParentList != null && !mParentList.EnableMinorVersions)
                {
                    mParentList.EnableVersioning = true;
                    mParentList.EnableMinorVersions = true;
                    mParentList.Update();
                }
            }
        }

        private void PrepareListSettingsBeforeIncreaseFileVersion(bool increaseMajorVersion)
        {
            if (mParentList == null)
            {
                return;
            }
            bool enableVersioning = mParentList.EnableVersioning;
            bool enableMinorVersions = mParentList.EnableMinorVersions;
            if (!enableVersioning)
            {
                mParentList.EnableVersioning = true;
                mListVersionSettingChanged = true;
            }
            if (!increaseMajorVersion && !enableMinorVersions)
            {
                mParentList.EnableMinorVersions = true;
                mListVersionSettingChanged = true;
            }
            else if (increaseMajorVersion && enableMinorVersions)// && (mHasPreCurrentVersion || isCheckout))
            {
                mParentList.EnableMinorVersions = false;
                mListVersionSettingChanged = true;
            }
            if (mListVersionSettingChanged)
            {
                mParentList.Update();
            }
        }

        protected virtual ClientFile AddFile(string serverRelativeUrl, System.IO.Stream stream, string etag, bool overwriteIfExists, AveClientOMRequest.SaveBinaryCheckMode checkMode, ClientRuntimeContext context, object obj)
        {
            if (mContext.HasPendingRequest)
            {
                mContext.ExecuteQuery();
            }
            string fileType = Path.GetExtension(mFileRelativeUrl);
            if (mSpecialFileList.Contains(fileType))
            {
                string webAppName = AveUrlUtility.GetServerUrl(mContext.Url);
                AveWebServiceRequest.SaveSpecialBinary(webAppName, mAveWebCache.ServerRelativeUrl, mObj, mFileRelativeUrl, stream, mServerVersion);
            }
            else
            {
                mRequest.SaveBinary(mFileRelativeUrl, stream, null, true, checkMode, mContext, mObj);
            }

            //ClientObjectData objData = AveAssemblyUtility.GetPropertyValue(mParentWeb, "ObjectData") as ClientObjectData;
            //objData.MethodReturnObjects.Clear();

            ClientFile file = mParentWeb.GetFileByServerRelativeUrl(mFileRelativeUrl);
            if (mIsWelcomePageChanged)
            {
                string fileUrl = this.GetRelativeUrl(serverRelativeUrl);
                mIsWelcomePageChanged = false;
                mParentWeb.RootFolder.WelcomePage = fileUrl;
                mParentWeb.RootFolder.Update();
            }

            return file;
        }
        private ClientFile IncreaseVersion(ClientFile file, Dictionary<string, object> documentProperties, bool increaseMajorVersion, bool isCurrentRestoredVersion, bool needUpdateDocument, ref int level)
        {
            bool isCheckout = mIsOriginalCheckOut && isCurrentRestoredVersion;
            //lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ListSettingLocker"))
            //{
            //    PrepareListSettingsBeforeIncreaseFileVersion(increaseMajorVersion);
            //    CheckoutFile(file, level == (int)FileLevel.Checkout || NeedSkipCheckout());
            //    if (mParentList != null)
            //    {
            //        mContext.Load(mParentList, list => list.EnableMinorVersions);
            //    }
            //    mContext.ExecuteQuery();
            //}
            CheckoutFile(file, level == (int)FileLevel.Checkout || NeedSkipCheckout());
            if (isCurrentRestoredVersion)
            {
                FileStreamUpdateOption fileStreamUpdateOption = new FileStreamUpdateOption()
                {
                    NeedUpdateStream = mHasStream || mParentList != null && mParentList.BaseTemplate == 850,
                    FileStreamUpdateType = FileStreamUpdateType.Custom
                };
                HandleDocumentStream(ref file, fileStreamUpdateOption);
                if (needUpdateDocument)
                {
                    UpdateDocumentAndRestoreWebparts(file, documentProperties);
                }
                //UpdateDocumentProperties(file, documentProperties);
                //RestoreGenericFileWebParts(file);
            }
            CheckinFile(file, increaseMajorVersion ? CheckinType.MajorCheckIn : CheckinType.MinorCheckIn, isCheckout);
            if (!isCheckout)
            {
                level = 2;
            }

            return file;
        }

        protected string GetRelativeUrl(string fileUrl)
        {
            string fileRelativeUrl = string.Empty;
            if (mFileRelativeUrl.StartsWith(mAveWebCache.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                fileRelativeUrl = mFileRelativeUrl.Substring(mAveWebCache.ServerRelativeUrl.TrimEnd('/').Length + 1);
            }
            else
            {
                fileRelativeUrl = fileUrl;
            }
            return fileRelativeUrl;
        }

        public virtual void Dispose()
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
        protected virtual void MoveToConflictFolder()
        {
            try
            {
                ClientFile file = GetFileByAPI(mFileRelativeUrl);
                bool exist = LoadFileInfo(file);//获取list setting属性需要执行context，把loadfile拿到前边一并获取，减少通信；
                if (!mParentList.ServerTemplateCanCreateFolders)
                {
                    return;
                }
                if (!mParentList.EnableFolderCreation)
                {
                    mParentList.EnableFolderCreation = true;
                    mParentList.Update();
                }
                if (!exist)
                {
                    return;
                }
                Dictionary<string, object> needKeepFields = new Dictionary<string, object>();
                needKeepFields.Add("Modified", file.TimeLastModified);
                needKeepFields.Add("Created", file.TimeCreated);
                needKeepFields.Add("Author", file.Author);
                needKeepFields.Add("Editor", file.ModifiedBy);
                string conflictFolderUrl = mParentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME;
                Microsoft.SharePoint.Client.Folder folder = mParentWeb.GetFolderByServerRelativeUrl(conflictFolderUrl);
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
                    if (mParentList != null && mParentList.BaseType != BaseType.DocumentLibrary)
                    {
                        ListItemCreationInformation creationInformation = new ListItemCreationInformation();
                        creationInformation.FolderUrl = conflictFolderUrl;
                        creationInformation.UnderlyingObjectType = FileSystemObjectType.Folder;
                        creationInformation.LeafName = AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME;
                        ListItem listItem = AddItemByAPI(mParentList, creationInformation);
                        listItem["Title"] = AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME;
                        listItem.Update();
                        folder = GetFolderByAPI(conflictFolderUrl);
                        mContext.Load(listItem);
                        mContext.Load(folder);
                        mContext.ExecuteQuery();
                    }
                    else
                    {
                        //if (mParentList != null && mParentList.BaseTemplate == 2100)     老逻辑，去掉
                        //{
                        //    string webApp = AveUrlUtility.GetServerUrl(mContext.Url);
                        //    AveWebServiceRequest.AddSlideFolder(webApp, mAveWebCache.ServerRelativeUrl, mParentList.Title, mParentFolder.ServerRelativeUrl, AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME, mObj);
                        //    folder = GetFolderByAPI(conflictFolderUrl);
                        //}
                        //else
                        //{
                            folder = AddFolderByAPI(mParentFolder.Folders,AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME);
                        //}
                        mContext.Load(folder);
                        mContext.ExecuteQuery();
                    }
                }
                #endregion
                string moveFileTitle = AveSPUtility.GetConflictNewName(file.Name, file.TimeLastModified);
                string moveFileUrl = conflictFolderUrl + "/" + moveFileTitle;
                //file.CopyTo(moveFileUrl, false);
                MoveToByAPI(file,moveFileUrl, MoveOperations.None);
                mContext.Load(file);
                mContext.Load(mListItem);
                mContext.ExecuteQuery();
                if (mListItem != null)
                {
                    mItemRestore.UpdateListItem(ref mListItem, needKeepFields, ListItemUpdateMethodKind.SystemUpdate, false);
                }
                ClientObjectData objData = AveAssemblyUtility.GetPropertyValue(mParentWeb, "ObjectData") as ClientObjectData;
                objData.MethodReturnObjects.Clear();
            }
            catch (Exception ex)
            {
                mLog.Error("Move item:{0} to Conflict folder failed,error:{1}", (mParentFolderUrl + "/" + mName), ex.ToString());
            }
        }

        protected virtual ClientFile AddFileByAPI(FileCollection files, FileCreationInformation createInfo)
        {
            return files.Add(createInfo);
        }

        protected virtual Folder AddFolderByAPI(FolderCollection folders, string url)
        {
            return folders.Add(url);
        }

        protected virtual Folder GetFolderByAPI(string url)
        {
            return mParentWeb.GetFolderByServerRelativeUrl(url);
        }

        protected virtual ClientFile GetFileByAPI(string url)
        {
            return mParentWeb.GetFileByServerRelativeUrl(url);
        }

        protected virtual void MoveToByAPI(ClientFile file, string url, MoveOperations option)
        {
            file.MoveTo(url, option);
        }

        protected virtual ListItem AddItemByAPI(List list, ListItemCreationInformation creationInformation)
        {
            return mParentList.AddItem(creationInformation);
        }
    }

    public struct FileStreamUpdateOption
    {
        internal bool NeedUpdateStream;
        internal FileStreamUpdateType FileStreamUpdateType;
    }

    internal enum FileStreamUpdateType
    {
        Empty = 0,
        Custom = 1
    }
}
