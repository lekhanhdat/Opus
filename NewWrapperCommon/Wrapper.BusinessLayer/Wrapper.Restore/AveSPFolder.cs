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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Core.Common;
using AvePoint.Wrapper.Core.SPBackupDto;
using AvePoint.Wrapper.Core.SPRestore;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using LS.SPWorkflowProcessor;
using System.Xml;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    public class AveSPFolder : AveSPItem, AvePoint.Wrapper.Restore.IAveSPFolder
    {
        private IAveFolder mSPFolder;
        private CurrentRestoreDocStatus currentDocStatus;
        protected AveFolderInfo mFolderInfo
        {
            get
            {
                return this.mBaseItemInfo as AveFolderInfo;
            }
            set
            {
                this.mBaseItemInfo = value;
            }
        }
        private RestoringDto mRestoringItem = null;
        private string mServerRelativeUrl;
        private bool mHasMoveUp;

        private bool isRestoreConnectorFolderProperties = false;
        public bool IsRestoreConnectorFolderProperties
        {
            set { isRestoreConnectorFolderProperties = value; }
            get { return isRestoreConnectorFolderProperties; }
        }

        public IAveFolder SPFolder
        {
            set
            {
                mSPFolder = value;
            }
            get
            {
                return mSPFolder;
            }
        }
        public CurrentRestoreDocStatus CurrentDocStatus
        {
            get { return currentDocStatus; }
            set { currentDocStatus = value; }
        }
        public RestoringDto RestoringItem { get { return mFolderInfo.RestoringItem; } }
        public string ServerRelativeUrl
        {
            get { return mServerRelativeUrl; }
            set { mServerRelativeUrl = value; }
        }
        public bool HasMoveUp
        {
            get { return mHasMoveUp; }
        }

        #region Obsolete field&property
        [Obsolete("no use now, will remove later")]
        private AveItemSecurity mSecurity;
        [Obsolete("no use now, will remove later")]
        private string mUrl;
        [Obsolete("no use now, will remove later")]
        private string mSrcUrl;
        [Obsolete("no use now, will remove later")]
        private long mSize;
        [Obsolete("no use now, will remove later")]
        private Dictionary<string, Dictionary<Guid, Guid>> mWebPartMapping = new Dictionary<string, Dictionary<Guid, Guid>>();
        [Obsolete("no use now, will remove later")]
        internal Dictionary<string, Dictionary<Guid, Guid>> WebPartMapping
        {
            get { return mWebPartMapping; }
        }
        [Obsolete("already inherit from AveSPItem, will remove later")]
        public AveSPItem AveSPItem
        {
            get { return this; }
        }
        [Obsolete("use mAveSPItem instead, will remove later")]
        private AveSPItem mEnsureCTFieldItem;//只为ensure field和contenttype使用
        [Obsolete("use AveSPItem instead, will remove later")]
        public AveSPItem EnsureCTFieldItem
        {
            get { return mEnsureCTFieldItem; }
        }
        //public AveSPItem AveItem
        //{
        //    get
        //    {
        //        return mAveSPItem;
        //    }
        //}
        [Obsolete("no use now, will remove later")]
        public string Url
        {
            get { return mUrl; }
        }
        [Obsolete("no use now, will remove later")]
        public string SrcUrl
        {
            get { return mSrcUrl; }
        }
        [Obsolete("no use now, will remove later")]
        public long Size
        {
            get { return mSize; }
        }

        /// <summary>
        /// 注意在不同的SP版本Folder TagUrl是不一样的,而且需要区分List Folder,Library Folder和Web Folder
        /// </summary>
        [Obsolete("no use now, will remove later")]
        public string TagUrl
        {
            get
            {
                string tagUrl = string.Empty;
                if (CheckFolderStyle())
                {
                    tagUrl = mSPFolder.ParentWeb.Url.TrimStart('/') + "/" + mSPFolder.Url.Trim('/');
                }
                else
                {
                    if (mAveSPList.SPList != null && !string.IsNullOrEmpty(mAveSPList.SPList.DefaultDisplayFormUrl))
                    {
                        string fileUrl = mAveSPList.SPList.DefaultDisplayFormUrl.TrimStart('/').Substring(mAveSPList.ParentWeb.SPWeb.ServerRelativeUrl.TrimStart('/').Length).TrimStart('/');
                        tagUrl = mAveSPList.ParentWeb.SPWeb.Url.TrimEnd('/') + "/" + fileUrl + "?ID=" + this.RowId;
                    }
                }
                return tagUrl;
            }
        }

        /// <summary>
        /// SP2013 List中Folder Url的格式为http://hostheader/sites/team/lists/test/DispForm.aspx?ID=2
        /// System Folder下的Folder需要特别注意
        /// </summary>
        /// <returns></returns>
        private bool CheckFolderStyle()
        {
            if (mAveParentSite.SPContextKind.IsServerMode13Upper())
            {
                return mAveSPList.SPList == null || (mAveSPList.SPList != null && mAveSPList.SPList.BaseType == AveBaseType.DocumentLibrary);
            }

            return (mQueryService != null && (mQueryService as IAveConnectorQueryService).IsSP2010SP1(mAveParentSite.SPSite.ID))
                     || (mAveSPList.SPList != null && mAveSPList.SPList.BaseType == AveBaseType.DocumentLibrary);
        }

        #endregion

        [Obsolete("This constructor is only used for unit test")]
        public AveSPFolder()
        {
        }

        //only for list root folder, we need to push it to stack
        public AveSPFolder(AveSPList aveList, string name)
            : base(aveList, null)
        {
            mFolderInfo = new AveFolderInfo();
            mSPFolder = aveList.RootFolder;
            mFolderInfo.GUID = mSPFolder.UniqueId;
            mFolderInfo.ItemType = AveItemType.Folder;
            mServerRelativeUrl = mSPFolder.ServerRelativeUrl;
            if (mServerRelativeUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                mServerRelativeUrl = mServerRelativeUrl.TrimEnd('/');
            }
            int pos = name.IndexOf(':');
            if (pos >= 0)
            {
                name = name.Substring(0, pos);
            }
            mFolderInfo.SiteId = aveList.ParentSite.SPSite.ID;
            mFolderInfo.Name = name;
            mFolderInfo.ParentId = aveList.RootFolder.UniqueId;
            mFolderInfo.RestoringItem = new RestoringDto();
            mFolderInfo.IsNewCreated = mAveSPList.IsNewCreated;
            mFolderInfo.IsRestoreConnectorFolderProperties = IsRestoreConnectorFolderProperties;
            ResetRestoringDtoThreadStaticProperties();
        }

        public AveSPFolder(AveSPFolder aveFolder, string name)
            : base(AveItemType.Folder, aveFolder, name)
        {
            //mAveSPItem = new AveSPItem(mFolderInfo, AveItemType.Folder, aveFolder, aveFolder.QueryService, name);
            mQueryService = aveFolder.QueryService;
            mAveParentSite = aveFolder.ParentSite;
            mAveSPList = aveFolder.ParentList;
            mParentFolder = aveFolder;

            mFolderInfo.ServerRelativeUrl = aveFolder.ServerRelativeUrl.TrimEnd('/') + "/" + this.AveSPItem.Name.TrimStart('/'); ;
            if (mFolderInfo.ServerRelativeUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                mFolderInfo.ServerRelativeUrl = mFolderInfo.ServerRelativeUrl.TrimEnd('/');
            }
            mFolderInfo.RestoringItem = new RestoringDto();
            mFolderInfo.IsNewCreated = mAveSPList.IsNewCreated;
            mServerRelativeUrl = mFolderInfo.ServerRelativeUrl;
            mEnsureCTFieldItem = new AveSPItem(mParentFolder);
            ResetRestoringDtoThreadStaticProperties();
        }

        /// <summary>
        /// only used for wrapper test purpose
        /// </summary>
        /// <param name="aveList"></param>
        /// <param name="restoreStream"></param>
        /// <param name="folderRelativeUrl"></param>
        /// <param name="isRestoreFolder"></param>
        public AveSPFolder(AveSPList aveList, IAveRestoreStream restoreStream, string folderRelativeUrl, bool isRestoreFolder)
            : base(aveList, restoreStream)
        {
            mFolderInfo = new AveFolderInfo();
            mAveSPList = aveList;
            //mAveSPWeb = aveList.ParentWeb;
            mQueryService = aveList.QueryService;
            mAveParentSite = aveList.ParentSite;
            //mAveSPItem = new AveSPItem(aveList, restoreStream);
            mSPFolder = aveList.ParentWeb.SPWeb.GetFolder(folderRelativeUrl);
            mServerRelativeUrl = mSPFolder.ServerRelativeUrl;
            mFolderInfo.GUID = mSPFolder.UniqueId;
            mFolderInfo.ParentId = mSPFolder.ParentFolder.UniqueId;
            mFolderInfo.SiteId = mAveSPList.ParentWeb.ParentSite.SPSite.ID;
            mFolderInfo.ItemType = AveItemType.Folder;
            mFolderInfo.ServerRelativeUrl = folderRelativeUrl;
            if (mFolderInfo.ServerRelativeUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                mFolderInfo.ServerRelativeUrl = mFolderInfo.ServerRelativeUrl.TrimEnd('/');
            }
            mFolderInfo.IsNewCreated = mAveSPList.IsNewCreated;
            mFolderInfo.RestoringItem = new RestoringDto();
            ResetRestoringDtoThreadStaticProperties();
        }

        public string ResetAvailableName()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFolder.ResetAvailableName"))
            {

                try
                {
                    IAveFolder folder = GetFolder(mParentFolder.SPFolder, mFolderInfo.Name);
                    if (folder == null)
                    {
                        return mFolderInfo.Name;
                    }
                    string extension = string.Empty;
                    string prevName = mFolderInfo.Name;
                    int pos = mFolderInfo.Name.LastIndexOf('.');
                    if (pos > 0)
                    {
                        extension = mFolderInfo.Name.Substring(pos, mFolderInfo.Name.Length - pos);
                        prevName = mFolderInfo.Name.Substring(0, pos);
                    }
                    for (int i = 1; i <= 1000; ++i)
                    {
                        StringBuilder temp = new StringBuilder(prevName);
                        temp.Append("_");
                        temp.Append(i.ToString());
                        temp.Append(extension);
                        folder = GetFolder(mParentFolder.SPFolder, temp.ToString());
                        if (folder == null)
                        {
                            mFolderInfo.Name = temp.ToString();
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Warn("Reset folder AvailableName Error: " + e.ToString());
                }
                return mFolderInfo.Name;

            }

        }
        /// <summary>
        /// 检查文件夹名是否冲突，如果冲突重新命名
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="needIncluded">检查文件夹名是否从本身开始</param>
        /// <returns></returns>
        /// [Obsolete("no use now, will remove later")]
        public string ResetAvailableName(string oldName, bool needIncluded)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFolder.ResetAvailableName"))
            {

                try
                {
                    string newName = string.Empty;
                    string extension = string.Empty;
                    string prevName = oldName;
                    int pos = oldName.LastIndexOf('.');
                    if (pos > 0)
                    {
                        extension = oldName.Substring(pos, oldName.Length - pos);
                        prevName = oldName.Substring(0, pos);
                    }

                    IAveFolder folder = null;
                    if (needIncluded)
                    {
                        folder = GetFolder(mParentFolder.SPFolder, oldName);
                        if (folder == null || !folder.Exists)
                        {
                            mFolderInfo.Name = oldName;
                            return mFolderInfo.Name;
                        }
                    }

                    for (int i = 0; i <= 1000; ++i)
                    {
                        StringBuilder temp = new StringBuilder(prevName);
                        temp.Append("_");
                        temp.Append(i.ToString());
                        temp.Append(extension);
                        folder = GetFolder(mParentFolder.SPFolder, temp.ToString());
                        if (folder == null)
                        {
                            mFolderInfo.Name = temp.ToString();
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Warn("Reset folder AvailableName Error: " + e.ToString());
                }
                return mFolderInfo.Name;

            }

        }

        public void ReloadFolder(bool force = true)
        {
            if (mSPFolder != null)
            {
                mSPFolder.Reload(force);
            }
            if (mParentFolder != null)
            {
                mParentFolder.ReloadFolder(force);
            }
        }
        /// <summary>
        /// just init spfolder object, no need to restore, only for replicator.
        /// </summary>
        public void InitSPFolder()
        {
            InitSPFolder(false);
        }

        public void ResetParentFolder(int maxUrlLength, bool needResetName)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFolder.ResetParentFolder"))
            {

                try
                {
                    while (mParentFolder.SPFolder.Url.Length + mParentFolder.SPFolder.ParentWeb.ServerRelativeUrl.Length + mFolderInfo.Name.Length + 1 > maxUrlLength && !mParentFolder.SPFolder.Url.Equals(mAveSPList.SPList.RootFolder.Url))
                    {
                        mParentFolder.SPFolder = mParentFolder.SPFolder.ParentFolder;
                        mHasMoveUp = true;
                    }
                    mFolderInfo.ParentId = mParentFolder.SPFolder.UniqueId;
                    if (mParentFolder.SPFolder.Url.Equals(mAveSPList.SPList.RootFolder.Url))
                    {
                        mServerRelativeUrl = mParentFolder.SPFolder.ServerRelativeUrl + "/" + mFolderInfo.Name;
                        if (needResetName)
                        {
                            mFolderInfo.Name = mParentFolder.SPFolder.Name;
                            mServerRelativeUrl = mParentFolder.SPFolder.ServerRelativeUrl;
                        }
                    }
                    else
                    {
                        mServerRelativeUrl = mParentFolder.SPFolder.ServerRelativeUrl + "/" + mFolderInfo.Name;
                        if (needResetName)
                        {
                            mFolderInfo.Name = mParentFolder.SPFolder.Name;
                            mServerRelativeUrl = mParentFolder.SPFolder.ServerRelativeUrl + "/" + mFolderInfo.Name;
                        }
                    }
                    if (mServerRelativeUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        mServerRelativeUrl = mServerRelativeUrl.TrimEnd('/');
                    }
                    //if (mAveSPItem != null)
                    {
                        this.ParentFolder.SPFolder = mParentFolder.SPFolder;
                    }
                }
                catch (Exception ex)
                {
                    log.Warn("An error occurred while Resetting Parent Folder ." + ex.ToString());
                }

            }

        }

        public void ResetParentFolder(bool moveUptoRootFolder, bool moveUptoHighLevelFolder, bool needResetName)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFolder.ResetParentFolder"))
            {

                try
                {
                    if (moveUptoRootFolder)
                    {
                        mParentFolder.SPFolder = mAveSPList.SPList.RootFolder;
                        mFolderInfo.ParentId = mParentFolder.SPFolder.UniqueId;
                        mServerRelativeUrl = mAveSPList.SPList.RootFolder.ServerRelativeUrl + "/" + mFolderInfo.Name;
                        if (needResetName)
                        {
                            mFolderInfo.Name = mParentFolder.SPFolder.Name;
                            mServerRelativeUrl = mAveSPList.SPList.RootFolder.ServerRelativeUrl;
                        }
                        mHasMoveUp = true;
                    }
                    else if (moveUptoHighLevelFolder)
                    {
                        mParentFolder.SPFolder = mParentFolder.SPFolder.ParentFolder;
                        mFolderInfo.ParentId = mParentFolder.SPFolder.UniqueId;
                        mServerRelativeUrl = mParentFolder.SPFolder.ServerRelativeUrl + "/" + mFolderInfo.Name;
                        if (needResetName)
                        {
                            mFolderInfo.Name = mParentFolder.SPFolder.Name;
                            mServerRelativeUrl = mParentFolder.SPFolder.ServerRelativeUrl;
                        }
                        mHasMoveUp = true;
                    }
                    //if (mAveSPItem != null)
                    {
                        this.ParentFolder.SPFolder = mParentFolder.SPFolder;
                    }
                    if (mServerRelativeUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        mServerRelativeUrl = mServerRelativeUrl.TrimEnd('/');
                    }
                }
                catch (Exception ex)
                {
                    log.Warn("An error occurred while Resetting Parent Folder ." + ex.ToString());
                }

            }

        }

        public void ResetParentFolder(AveSPFolder parentFolder)
        {
            mParentFolder = parentFolder;
            mFolderInfo.ParentId = parentFolder.Id;
            mServerRelativeUrl = mParentFolder.SPFolder.ServerRelativeUrl + "/" + mFolderInfo.Name;
            //if (mAveSPItem != null)
            //{
            //    mAveSPItem = new AveSPItem(mFolderInfo, AveItemType.Folder, mParentFolder, mQueryService);
            //}
            mAveItem = mAveParentSite.ObjectModelFactory.CreateAveItem(mBaseItemInfo, mAveSPList.RootFolder, mParentWeb.SPWeb, mAveSPList.SPList);
            mBaseItemInfo.AveItem = mAveItem;
            mHasMoveUp = true;
        }

        public void InitSPFolder(bool tryCreate)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFolder.InitSPFolder"))
            {

                mSPFolder = GetFolder(mParentFolder.SPFolder, mFolderInfo.Name);
                if (tryCreate && (mSPFolder == null || mSPFolder.Exists == false))
                {
                    try
                    {
                        IAveListItem item = null;

                        /************************************************
                        Slibe Library需要关闭Event Receiver。Content Library和Media Library需要打开
                        *************************************************/
                        bool eventReceiverEnabled = mAveSPList.SPList.TemplateFeatureId == new Guid(AveWrapperConstants.AVEFSDLFEATRUEID)
                                                    || mAveSPList.SPList.TemplateFeatureId == new Guid(AveWrapperConstants.AVEVDLFEATRUEID);

                        //使用using方法来处理EventReceiver的使用和重置逻辑；
                        using (AveEventReceiverUtility eventReceiver = new AveEventReceiverUtility(eventReceiverEnabled))
                        {
                            item = mAveSPList.SPList.Items.Add(mParentFolder.SPFolder.ServerRelativeUrl, AveFileSystemObjectType.Folder, mFolderInfo.Name);
                            item["Title"] = mFolderInfo.Name;
                            item.SystemUpdate(false);
                            mSPFolder = mAveSPList.ParentWeb.AveWeb.GetFolder(item.Folder.ServerRelativeUrl);
                            mFolderInfo.GUID = mSPFolder.UniqueId;
                            this.IsNewCreated = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(string.Format("Can not create folder:{0}. Reason:{1})", mFolderInfo.Name, ex.ToString()));
                    }
                }

            }

        }

        private IAveFolder GetFolder(IAveFolder parentFolder, string name)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFolder.GetFolder"))
            {

                IAveFolder folder = null;
                try
                {
                    if (!string.IsNullOrEmpty(parentFolder.ServerRelativeUrl))
                    {
                        //[ADO-53892]In order to determine the conflict folder, we must reload folder object to get the new created folder
                        parentFolder = mAveSPList.ParentWeb.SPWeb.GetFolder(parentFolder.ServerRelativeUrl);
                    }
                    if (name.Equals("{System Folder}"))
                    {
                        folder = parentFolder;
                        mFolderInfo.GUID = folder.UniqueId;
                    }
                    else
                    {
                        //system folder的parentfolder的ServerRelativeUrl后面有 "/"
                        folder = parentFolder.ParentWeb.GetFolder(parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + name);
                        //folder = parentFolder.SubFolders[name];
                        mFolderInfo.GUID = folder.UniqueId;
                    }
                }
                //  catch(ArgumentException)
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFolderByNameError, e.ToString());
                    if (!string.IsNullOrEmpty(mFolderInfo.ServerRelativeUrl))
                    {
                        try
                        {
                            folder = mAveSPList.ParentWeb.SPWeb.GetFolder(mFolderInfo.ServerRelativeUrl);
                            if (folder != null && folder.Exists)
                            {
                                mFolderInfo.GUID = folder.UniqueId;
                            }
                            else
                            {
                                folder = null;
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFolderByNameError, ex.ToString());
                            //no exist
                            folder = null;
                        }
                    }
                }

                if (folder == null || !folder.Exists)
                {
                    return null;
                }

                return folder;

            }

        }

        internal override bool ShouldRestoreItem(AveItemFieldCollectionInfo fieldColInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFolder.ShouldRestoreItem"))
            {

                if (this.TryGetItem(fieldColInfo, this.SPListItem))
                {
                    AveRestoreMode restoreMode = this.RestoreOption.mAveRestoreMode;

                    if (restoreMode == AveRestoreMode.OverWrite)
                    {
                        return true;
                    }
                    else if (restoreMode == AveRestoreMode.Append)
                    {
                        AveItemFieldInfo itemFieldInfo = fieldColInfo.GetUniqueItemFieldInfoByDisplayName("BaseName");
                        return false;
                    }
                    else if (restoreMode == AveRestoreMode.OverWriteByModifiedTime)
                    {
                        AveItemFieldInfo fieldInfo = fieldColInfo.GetUniqueItemFieldInfoByDisplayName("ModifyDate");
                        if ((DateTime)fieldInfo.Value > (DateTime)this.SPListItem["ModifyDate"])
                        {
                            return true;
                        }
                        return false;
                    }
                    return false;
                }
                else
                {
                    return true;
                }

            }

        }

        public AveRestoreResult RestoreSelf(
               Dictionary<string, object> allDocData,
               Dictionary<string, object> allUserData)
        {
            return RestoreSelf(allDocData, allUserData, new List<Dictionary<string, object>>());
        }

        public AveRestoreResult RestoreSelf(
              Dictionary<string, object> allDocData,
              Dictionary<string, object> allUserData,
              List<Dictionary<string, object>> allDataJunction)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFolder.RestoreSelf"))
            {

                AveRestoreResult result = AveRestoreResult.Normal;
                try
                {
                    ProcessPreCondition(allDocData, allUserData, allDataJunction);
                    //TODOLMM  if SPList is null, will thow exception,  (LMM, Please think more about System List--xhyou)
                    ProcessVerifyItem();
                    mParentFolder.SPFolder.SubFolders.SetReport(report);
                    if (this.ParentList.firstTime && this.ParentList.containsTODAY)
                    {
                        this.ParentWeb.ReloadWeb();
                        this.ParentList.ReloadList();
                        this.ParentList.firstTime = false;
                    }
                    result = mParentFolder.SPFolder.SubFolders.RestoreFolder(mFolderInfo, allDocData, allUserData);
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("An Error occurred while restore folder. {0}" + mFolderInfo.Name, ex);
                    throw;
                }
                catch (AveRestoreException ex)
                {
                    result = ex.Result;
                }
                finally
                {
                    // EnableModeration 被改变，需要重新还原listsetting
                    if (mFolderInfo.SettingInfo.LIST_SETTING_CHANGED)
                    {
                        ParentList.SetListSettingFlags(AveListSettingFlags.LIST_SETTING_CHANGED);
                    }
                    if (ParentList.containsTODAY)
                    {
                        this.ParentWeb.ReloadWeb();
                        this.ParentList.ReloadList();
                    }
                }
                //AveRestoreResult result = mAveSPList.SPList.RestoreFolder(mFolderInfo, allDocData, allUserData);
                ProcessPostCondition(result, allDocData, allUserData);
                return result;
            }
        }

        internal void ProcessPostCondition(AveRestoreResult result, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData)
        {
            if (mFolderInfo.SettingInfo.LIST_SETTING_CHANGED)
            {
                ParentFolder.ParentList.SetListSettingFlags(AveListSettingFlags.LIST_SETTING_CHANGED);
            }
            if (result != AveRestoreResult.Failed && result != AveRestoreResult.Omit && mFolderInfo.RowId > 0)
            {
                mAveSPList.AveFields.ResetNotUpdateLookupFieldValue(mFolderInfo.RowId);
                mAveSPList.AveFields.ResetNintexFormDataFieldValue(mFolderInfo.RowId);
                mAveSPList.AveFields.ResetNotUpdateUrlFieldValue(mFolderInfo.RowId);
                this.AddItemMapping(mFolderInfo.OriginalRowId);
            }
            mSPFolder = mFolderInfo.AveItem.Folder;

            //if current list is a discussion board, we should reload web and reload the SPFolder object to get the correct last updated time.
            if (mAveSPList != null && mAveSPList.SPList != null && mAveSPList.SPList.BaseTemplate == AveListTemplateType.DiscussionBoard)
            {
                mParentWeb.ReloadWeb();
                ReloadFolder();
            }
        }

        /// <summary>
        /// 在该方法中处理AveSPFolder需要单独处理的DocData相关设置，AveSPItem(AveSPDoc/AveSPItem/AveSPFolder)共有的DocData处理在AveSPItem对应的ProcessPreDocDataCondtion中进行设置
        /// </summary>
        /// <param name="allDocData"></param>
        internal override void ProcessPreDocDataCondition(Dictionary<string, object> allDocData)
        {
            base.ProcessPreDocDataCondition(allDocData);
        }

        /// <summary>
        /// 在该方法中处理AveSPFolder需要单独处理的UserData相关设置，
        /// AveSPItem(AveSPDoc/AveSPItem/AveSPFolder)共有的UserData处理在AveSPItem对应的ProcessPreUserDataCondtion中进行设置
        /// </summary>
        /// <param name="allUserData"></param>
        internal override void ProcessPreUserDataCondition(Dictionary<string, object> allUserData)
        {
            //ADO-91965
            //当这个Folder是Manual Input或者是目的端已存在(Name和源端不同)的情况下，需要把UD表中的Title保持和目的端一致
            //但是DiscussionBoard中的Item(其实就是Folder)，需要把Title还原到目的端，因为SP界面上显示的是Title，而非Name
            //对于不是当前version的folder则title不进行修改，否则非当前version的title始终都还不回去
            //if (this.mBaseItemInfo.IsCurrentVersion && allUserData.ContainsKey("Title") && !string.IsNullOrEmpty(Name) && mParentFolder.ParentList.SPList.BaseTemplate != AveListTemplateType.DiscussionBoard)
            //{
            //    allUserData["Title"] = Name;
            //}
            base.ProcessPreUserDataCondition(allUserData);
        }

        internal override void ProcessPreUserAndJunctionDataCondition(Dictionary<string, object> allUserData, List<Dictionary<string, object>> junctionData)
        {
            base.ProcessPreUserAndJunctionDataCondition(allUserData, junctionData);
        }

        /// <summary>
        /// 在该方法中处理AveSPFolder需要单独处理的Setting设置(和allDocData，allUserData无关)，AveSPItem(AveSPDoc/AveSPItem/AveSPFolder)共有的setting设置在AveSPItem对应的ProcessPreSettingCondition中进行设置
        /// </summary>
        internal override void ProcessPreSettingCondition()
        {
            base.ProcessPreSettingCondition();
            RestoreOption.mAveItemRestoreOption.DELETE_ITEM = mAveSPList.RestoringFolder.Init(mFolderInfo.Name, CheckRestoreOption(IsNewCreated, AveRestoreMode.OverWrite), RestoreOption.mAveItemRestoreOption.DELETE_ITEM);
            mFolderInfo.SettingInfo.DELETE_ITEM = RestoreOption.mAveItemRestoreOption.DELETE_ITEM;
            mFolderInfo.RestoringItem = mAveSPList.RestoringFolder;
            mFolderInfo.ParentListName = mAveSPList.Name;
            mFolderInfo.ParentListIsSystem = mAveSPList.IsSystemList;
            mFolderInfo.ListRootFolderId = mAveSPList.RootFolder.UniqueId;
            mFolderInfo.IsOverWrite = CheckRestoreOption(mFolderInfo.IsNewCreated, AveRestoreMode.OverWrite);
        }

        /// <summary>
        /// 在该方法中处理AveSPFolder需要单独处理的MetaInfo(包括UnVersionedMetaInfo)，AveSPItem(AveSPDoc/AveSPItem/AveSPFolder)共有的MetaInfo设置在AveSPItem对应的ProcessPreMetaInfoCondtion中进行设置
        /// </summary>
        /// <param name="allDocData"></param>
        internal override void ProcessPreMetaInfoCondition(Dictionary<string, object> allDocData)
        {
            base.ProcessPreMetaInfoCondition(allDocData);
            ProcessDocumentSetCuptureVersionData(allDocData);
            ProcessContentTypeOrder(allDocData);
        }
        private void ProcessContentTypeOrder(Dictionary<string, object> allDocData)
        {
            try
            {
                // 针对O365 处理document set content type order
                if (ParentSite.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel || !allDocData.ContainsKey("MetaInfo"))
                {
                    return;
                }
                var metaInfoDic = AveCompressedUtility.GetMetaInfoDictionary((byte[])allDocData["MetaInfo"]);
                if (metaInfoDic.ContainsKey("vti_contenttypeorder") && metaInfoDic["vti_contenttypeorder"] != null && ParentList.SPList != null)
                {
                    string[] ctIds = metaInfoDic["vti_contenttypeorder"].Split(',');
                    int count = ctIds.Length;
                    List<IAveContentType> tempContentTypes = new List<IAveContentType>();
                    foreach (string id in ctIds)
                    {
                        IAveContentType destinationContentType = null;
                        //content type has been migrated in job, find it by ct id .
                        if (ParentSite.MappingManager.ListMappingManager.ListLevelCTIdMapping.ContainsKey(id))
                        {
                            destinationContentType = ParentList.SPList.ContentTypes[ParentSite.MappingManager.ListMappingManager.ListLevelCTIdMapping[id]];
                            if (destinationContentType != null)
                            {
                                tempContentTypes.Add(destinationContentType);
                            }
                        }
                        //content type does not been migrated in this job ,find it by name.
                        if (destinationContentType == null && ParentList.AveContentTypes.ContentTypeCache.ContainsKey(id))
                        {
                            AveContentTypeInfo ctInfo = ParentList.AveContentTypes.ContentTypeCache[id];
                            string mappingName = ParentList.AveContentTypes.ContentTypeMapping.GetContentTypeNameMappingFromGui(ctInfo.Name);

                            string ctNameForFind = string.IsNullOrEmpty(mappingName) || string.Equals(mappingName, ctInfo.Name, StringComparison.OrdinalIgnoreCase) ? ctInfo.Name : mappingName;
                            destinationContentType = ParentList.SPList.ContentTypes[ctNameForFind];
                            if (destinationContentType != null)
                            {
                                tempContentTypes.Add(destinationContentType);
                            }
                        }
                        if (destinationContentType == null)
                        {
                            log.Warn("Can not find any content type for id: {0}.", id);
                            return;
                        }
                    }

                    StringBuilder tempString = new StringBuilder();
                    tempContentTypes.ForEach(ct => { tempString.AppendFormat("{0},", ct.ID.ToString()); });
                    tempString.Length--;
                    allDocData["vti_contenttypeorder"] = tempString.ToString();
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "An error occourred while processing content type order. Error:{0}", e);
            }
        }

        #region ProcessDocumentSetCuptureVersionData
        private void ProcessDocumentSetCuptureVersionData(Dictionary<string, object> allDocData)
        {
            try
            {
                if (!allDocData.ContainsKey("MetaInfo"))
                {
                    return;
                }
                var metaInfoDic = AveCompressedUtility.GetMetaInfoDictionary((byte[])allDocData["MetaInfo"]);
                if (metaInfoDic.ContainsKey("snapshots"))
                {
                    var snapshots = metaInfoDic["snapshots"];
                    XmlDocument snapShot = new XmlDocument();
                    snapShot.PreserveWhitespace = true;//sharepoint use xmlreader to analyze this, which won't ignore white space node.ADO-8150
                    snapShot.LoadXml(snapshots);

                    foreach (XmlElement fieldElement in snapShot.SelectNodes("/SnapshotCollection/Snapshots/Snapshot/Fields/Field").OfType<XmlElement>())
                    {
                        try
                        {
                            string oldValue = fieldElement.GetAttribute("Id");
                            if (!string.IsNullOrEmpty(oldValue))
                            {
                                var oldFiedlId = new Guid(oldValue);
                                Guid newValue = ParentList.AveFields.FieldMapping.GetMappingRestoredFieldId(oldFiedlId);
                                if (newValue != Guid.Empty)
                                {
                                    fieldElement.SetAttribute("Id", newValue.ToString());
                                    var mappedValue = GetMappedCaptureColumnValue(oldFiedlId, newValue, fieldElement.InnerText);
                                    if(!string.IsNullOrEmpty(mappedValue))
                                    {
                                        fieldElement.InnerText = mappedValue;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetFileMetaInfoFailed, e);
                        }
                    }
                    allDocData["snapshots"] = snapShot.OuterXml;
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, "An error occourred while dealing with capture version.Error:{0}", e);
            }
        }

        public string GetMappedCaptureColumnValue(Guid sourceFieldId, Guid destFieldId, string captureColumnValue)
        {
            var sourceFieldInfo = ParentList.AveFields.XmlFields.Values.FirstOrDefault(f => f.ID == sourceFieldId);
            if (sourceFieldInfo != null)
            {
                //暂时没有必要处理MMS Column值，再ItemMetadata 中被过滤掉即可。
                //if (sourceFieldInfo.TypeAsString == "TaxonomyFieldType")
                //{
                //    var taxonomyField = ParentList.AveFields.SourceTextTaxonomyDic.FirstOrDefault(f => string.Equals(f.Value, sourceFieldInfo.FieldInternalName, StringComparison.OrdinalIgnoreCase));
                //    if (default(KeyValuePair<string, string>).Equals(taxonomyField))
                //    {
                //        sourceFieldInfo = ParentList.AveFields.XmlFields[taxonomyField.Key];
                //    }
                //}
                ItemMetadata itemData = new ItemMetadata(this, mBaseItemInfo.OriginalVersion, mBaseItemInfo.OriginalRowId, new Dictionary<string, object>() { { sourceFieldInfo.FieldInternalName, GetValueFormatForMapping(sourceFieldInfo, captureColumnValue) }, { "#tp_ID", RowId } }, null);
                var fieldValues = itemData.ProcessItemMetadata(IsMergeToFolder);
                if (fieldValues != null)
                {
                    //ADO-199949 HyperLink 由于Address和description，导致有两个filedValues，使用最后的一个
                    var fkv = fieldValues.LastOrDefault(kv => kv.Value.Id == destFieldId);
                    if (fkv.Value != null && !string.IsNullOrEmpty(fkv.Value.ColValue.ToString()))
                    {
                        return ConvertMappedValueToDocumentSetCaptureFormat(sourceFieldInfo, captureColumnValue, fkv.Value.ColValue.ToString(), destFieldId);
                    }
                }
            }
            return string.Empty;
        }

        private string ConvertMappedValueToDocumentSetCaptureFormat(AveXmlField sourceFieldInfo, string oldValue, string mappedValue, Guid destFieldId)
        {
            switch (sourceFieldInfo.Type)
            {
                case AveFieldType.Lookup:
                    if (!sourceFieldInfo.AllowMultipleValues)
                    {
                        int mappedLookupId = -1;
                        if (Int32.TryParse(mappedValue, out mappedLookupId))
                        {
                            var destField = this.ParentList.SPList.Fields.GetById(destFieldId) as IAveFieldLookup;
                            if (destField != null)
                            {
                                using (var lookupWeb = this.ParentSite.SPSite.OpenWeb(destField.LookupWebId))
                                {
                                    var lookupList = lookupWeb.GetList(new Guid(destField.LookupList));
                                    var lookupField = lookupList.Fields.GetFieldByInternalName(destField.LookupField);
                                    var item = lookupList.GetItemById(mappedLookupId);
                                    var displayValue = item[lookupField.InternalName].ToString();
                                    return string.Format("{0};#{1}", mappedLookupId, displayValue);
                                }
                            }
                        }
                    }
                    break;
                case AveFieldType.User:
                    if (!sourceFieldInfo.AllowMultipleValues)
                    {
                        return FormatUserCaptureInfo(mappedValue);
                    }
                    else
                    {
                        StringBuilder result = new StringBuilder();
                        var temp = mappedValue.Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                        for(int i = 0; i < temp.Length - 1; i += 2)
                        {
                            if (!string.IsNullOrEmpty(temp[i]))
                            {
                                result.AppendFormat("{0};#", FormatUserCaptureInfo(temp[i]));
                            }
                        }
                        if (result.Length >= 2 && result.ToString().EndsWith(";#"))
                        {
                            result.Length -= 2;
                        }
                        return result.ToString();
                    }
                case AveFieldType.URL:
                    if(!string.IsNullOrEmpty(oldValue) && oldValue.Contains(','))
                    {
                        return string.Format("{0},{1}", mappedValue, oldValue.Split(',')[1]);
                    }
                    break;
                default:
                    return mappedValue;
            }
            return mappedValue;
        }

        private string FormatUserCaptureInfo(string mappedValue)
        {
            int mappedUserId = -1;
            if (Int32.TryParse(mappedValue, out mappedUserId))
            {
                var mappedUserName = this.ParentSite.SPSite.RootWeb.SiteUsers.GetByID(mappedUserId).Name;
                return string.Format("{0};#{1}", mappedUserId, mappedUserName);
            }
            else
            {
                log.Warn("Failed get mapped user info, mapped user id: {0}", mappedValue);
                return mappedValue;
            }
        }

        private object GetValueFormatForMapping(AveXmlField sourceFieldInfo, string captureColumnValue)
        {
            switch (sourceFieldInfo.Type)
            {
                case AveFieldType.Lookup:
                    if (!sourceFieldInfo.AllowMultipleValues)
                    {
                        var index1 = captureColumnValue.IndexOf('#');
                        if (index1 > -1)
                        {
                            var temp = captureColumnValue.Split('#');
                            return string.Format("{0}{1}", temp[0], temp[1]);
                        }
                    }
                    else
                    {
                        return GetCaptureMultiValueForMapping(captureColumnValue);
                    }
                    break;
                case AveFieldType.User:
                    if (!sourceFieldInfo.AllowMultipleValues)
                    {
                        var index2 = captureColumnValue.IndexOf(';');
                        if (index2 > -1)
                        {
                            var temp = captureColumnValue.Split(';');
                            return temp[0];
                        }
                    }
                    else
                    {
                        return GetCaptureMultiValueForMapping(captureColumnValue);
                    }
                    break;
                case AveFieldType.URL:
                    if(!string.IsNullOrEmpty(captureColumnValue) && captureColumnValue.Contains(','))
                    {
                        return captureColumnValue.Split(',')[0];
                    }
                    break;
                default:
                    return captureColumnValue;
            }
            return captureColumnValue;
        }

        private Dictionary<int, string> GetCaptureMultiValueForMapping(string captureColumnValue)
        {
            var temp = captureColumnValue.Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
            var result = new Dictionary<int, string>();
            for (int i = 0; i < temp.Length - 1; i += 2)
            {
                result.Add(Int32.Parse(temp[i]), temp[i + 1]);
            }
            return result;
        }
        #endregion

        internal void ProcessPreCondition(Dictionary<string, object> allDocData, Dictionary<string, object> allUserData, List<Dictionary<string, object>> allJunctionData)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFolder.ProcessPreCondition"))
            {
                //move this new AveSPItem process to AveFolder constructor method.
                //mAveSPItem = new AveSPItem(mFolderInfo, AveItemType.Folder, mParentFolder, mQueryService);
                this.SetRestoreOption(mRestoreOption);
                ProcessPreDocDataCondition(allDocData);
                ProcessPreSettingCondition();
                ProcessPreUserAndJunctionDataCondition(allUserData, allJunctionData);

            }
        }

        #region Obsolete method
        [Obsolete("no use now, will remove later")]
        private Guid GetFolderIdByName(string name)
        {
            return mQueryService.GetFolderIdByName(name, mFolderInfo.SiteId, mFolderInfo.ParentId);
        }
        [Obsolete("no use now, will remove later")]
        private void CreateFolder(AveItemFieldCollectionInfo itemFieldColInfo)
        {
            IAveList aveList = this.ParentList.SPList;
            AveItemCreationInformation aici = new AveItemCreationInformation();
            aici.FolderUrl = mParentFolder.ServerRelativeUrl;
            aici.UnderlyingObjectType = AveFileSystemObjectType.Folder;
            this.SPListItem = aveList.AddItem(aici);
            this.RestoreItemProperty(itemFieldColInfo, aveList, this.SPListItem);
        }
        [Obsolete("no use now, will remove later")]
        private void AddFields(IAveListItem item, Dictionary<string, object> fieldMap)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFolder.AddFields"))
            {

                if (fieldMap == null)
                {
                    return;
                }
                foreach (KeyValuePair<string, object> field in fieldMap)
                {
                    try
                    {
                        if (field.Value is DateTime)
                        {
                            item[field.Key] = item.ParentList.ParentWeb.RegionalSettings.TimeZone.UTCToLocalTime(Convert.ToDateTime(field.Value, System.Globalization.DateTimeFormatInfo.InvariantInfo));
                        }
                        else
                        {
                            item[field.Key] = field.Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while add field. field key:{0}, field value:{1}, folder name:{2}\n error message:{3}", field.Key, field.Value, mFolderInfo.Name, ex));
                        //mLog.Warn(ex, "An error occurred while updating the field '{0}' of value '{1}' for item '{2}'", field.Key, field.Value, info.Name);
                    }
                }

            }

        }

        [Obsolete("no use now, will remove later")]
        public void GetOrCreateFolder()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFolder.GetOrCreateFolder"))
            {

                //mSPFolder = mParentFolder.mSPFolder.SubFolders[info.Name];
                mSPFolder = GetFolder(mSPFolder, mFolderInfo.Name);
                if (mSPFolder != null)
                {
                    mFolderInfo.IsNewCreated = false;
                }
                else
                {
                    try
                    {
                        IAveListItem tmpItem = this.mParentFolder.mAveSPList.SPList.AddItem(mParentFolder.mSPFolder.ServerRelativeUrl, AveFileSystemObjectType.Folder, mFolderInfo.Name);
                        tmpItem["Title"] = mFolderInfo.Name;
                        tmpItem.Update();
                        mSPFolder = tmpItem.Folder;
                        mFolderInfo.GUID = mSPFolder.UniqueId;
                        mFolderInfo.IsNewCreated = true;
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, "An error occurred while creating new folder :{0} Exception:{1}", mFolderInfo.Name, e.ToString());
                    }
                }


            }

        }
        #endregion

        #region IAveSPFolder Members
        IAveSPItem IAveSPFolder.EnsureCTFieldItem
        {
            get { return mEnsureCTFieldItem; }
        }

        public void ResetParentFolder(IAveSPFolder parentFolder)
        {
            ResetParentFolder(parentFolder as AveSPFolder);
        }

        #endregion

        /// <summary>
        /// Pre restore metadata which is used for the folder and restore document itself.
        /// </summary>
        /// <param name="option"></param>
        /// <param name="folderMetadataDto"></param>
        protected void RestoreFolderMetadataDto(SPFolderRestoreOption option, SPFolderMetadataDto folderMetadataDto)
        {
            this.mAveParentSite.RestoreUser(folderMetadataDto.UserCache);
            this.mAveParentSite.RestoreGroup(folderMetadataDto.GroupCache);
            this.mAveParentSite.RestoreMetadataInfo(folderMetadataDto.MetadataInfo);
            this.VerifyItemMetadataDependency(folderMetadataDto.UserDataInfo, option.MetadataRestoreOption);
            this.RestoreSelf(folderMetadataDto.DocInfo_Old, folderMetadataDto.UserDataInfo);
            this.RestoreLookupFieldGuidValue(folderMetadataDto.ItemTPGUIDofLookupValue);
        }

        internal void ResetRestoringDtoThreadStaticProperties()
        {
            if (RestoringItem != null)
            {
                RestoringItem.ResetThreadStaticProperties();
            }
        }
    }

    //internal class AveSPFolderV1 : AveSPFolder, ISPFolderImport
    //{
    //    private readonly AveSPFolderV1 parentSPFolderV1;
    //    private readonly AveSPListV1 parentSPListV1;

    //    public AveSPFolderV1(AveSPFolderV1 aveSPFolderV1, string folderName)
    //        : base(aveSPFolderV1, folderName)
    //    {
    //        this.parentSPFolderV1 = aveSPFolderV1;
    //        this.parentSPListV1 = aveSPFolderV1.parentSPListV1;
    //    }

    //    public AveSPFolderV1(AveSPListV1 aveSPListV1, string folderName)
    //        : base(aveSPListV1, folderName)
    //    {
    //        this.parentSPListV1 = aveSPListV1;
    //    }

    //    public AveSPFolderV1(AveSPListV1 aveSPListV1, string parentFolderRelativeUrl, string folderName)
    //        : base(new AveSPFolder(aveSPListV1, null, parentFolderRelativeUrl, false), folderName)
    //    {

    //    }

    //    //public AveSPFolderV1(AveSPListV1 aveSPListV1) :
    //    //    base(aveSPListV1, "{System Folder}")
    //    //{
    //    //    this.parentSPListV1 = aveSPListV1;
    //    //}

    //    /// <summary>
    //    /// Restore folder
    //    /// 
    //    /// 这个是新加的接口,外围请暂时不要调用
    //    /// </summary>
    //    /// <param name="restoreStream"></param>
    //    /// <param name="spFolderRestoreOption"></param>
    //    /// <returns></returns>
    //    public SPFileRestoreReport Restore(IAveRestoreStream restoreStream, SPFolderRestoreOption spFolderRestoreOption)
    //    {
    //        if (restoreStream == null)
    //        {
    //            throw new ArgumentNullException("restoreStream");
    //        }

    //        if (spFolderRestoreOption == null)
    //        {
    //            throw new ArgumentNullException("spFolderRestoreOption");
    //        }

    //        var restoreReport = new SPFileRestoreReport();

    //        using (WrapperStopwatch.CreateInstance(spFolderRestoreOption.IncludePerformanceDetails, restoreReport.UpdateTimeUsage))
    //        {
    //            this.PreRestore(restoreStream, spFolderRestoreOption.FilterUserInfo, spFolderRestoreOption.FilterGroupInfo);

    //            AveMetadata metadata = null;

    //            while ((metadata = restoreStream.ReadMetadata()) != null)
    //            {
    //                var action = GetAction(metadata.MetadataType);

    //                if (action != null)
    //                {
    //                    var metadataRestoreReport = new MetadataRestoreReport(metadata.MetadataType);
    //                    using (WrapperStopwatch.CreateInstance(spFolderRestoreOption.IncludePerformanceDetails, metadataRestoreReport.AddTimeUsage))
    //                    {
    //                        action(restoreStream, spFolderRestoreOption, metadata, metadataRestoreReport);
    //                    }

    //                    restoreReport.Add(metadata.MetadataType, metadataRestoreReport);
    //                }
    //                else
    //                {
    //                    log.Error("There is no action for {0}, please submit a request for this type.", metadata.MetadataType);
    //                }
    //            }
    //        }

    //        return restoreReport;
    //    }

    //    private Action<IAveRestoreStream, SPFolderRestoreOption, AveMetadata, MetadataRestoreReport> GetAction(AveMetadataType metadataType)
    //    {
    //        Action<IAveRestoreStream, SPFolderRestoreOption, AveMetadata, MetadataRestoreReport> action = null;

    //        switch (metadataType)
    //        {
    //            case AveMetadataType.DocProperty:
    //                action = RestoreFolderDocProperty;
    //                break;
    //            case AveMetadataType.ItemMetadataDto:
    //                action = RestoreFolderItemMetadataDto;
    //                break;
    //            case AveMetadataType.DocDataJunction:
    //                action = RestoreFolderDocDataJunction;
    //                break;
    //            case AveMetadataType.LookupFieldGuidValue:
    //                action = RestoreFolderLookupFieldGuidValue;
    //                break;
    //            case AveMetadataType.RoleAssignment:
    //                action = RestoreFolderRoleAssignment;
    //                break;
    //            //case AveMetadataType.UserCache:
    //            //    action = RestoreFolderUserCache;
    //            //    break;
    //            //case AveMetadataType.GroupCache:
    //            //    action = RestoreFolderGroupCache;
    //            //    break;
    //            case AveMetadataType.RoleAssignmentsDto:
    //                action = RestoreFolderRoleAssignmentsDto;
    //                break;
    //            case AveMetadataType.RoleAssignmentInheritStatus:
    //                action = RestoreFolderRoleAssignmentInheritStatus;
    //                break;
    //            case AveMetadataType.DocImmedSubscriptions:
    //                action = RestoreFolderDocImmedSubscriptions;
    //                break;
    //            case AveMetadataType.DocSchedSubscriptions:
    //                action = RestoreFolderDocSchedSubscriptions;
    //                break;
    //            case AveMetadataType.SocialTag:
    //                action = RestoreFolderSocialTag;
    //                break;
    //            case AveMetadataType.SocialComment:
    //                action = RestoreFolderSocialComment;
    //                break;
    //            case AveMetadataType.DocumentTagging:
    //                action = RestoreFolderDocumentTagging;
    //                break;
    //            case AveMetadataType.MetadataService:
    //                action = RestoreListMetadataService;
    //                break;
    //            case AveMetadataType.WorkflowInstance:
    //                action = RestoreFolderWorkflowInstance;
    //                break;
    //            case AveMetadataType.WorkflowSchedule:
    //                action = RestoreFolderWorkflowSchedule;
    //                break;
    //        }

    //        return action;
    //    }

    //    private void EnsureWFInstanceOption(SPFolderRestoreOption option)
    //    {
    //        if (option.WorkflowRestoreOption == null || option.WorkflowRestoreOption.InstanceRestoreOption == null)
    //        {
    //            throw new ArgumentNullException("option.WorkflowRestoreOption.InstanceRestoreOption");
    //        }
    //    }

    //    private void RestoreFolderDocProperty(IAveRestoreStream restoreStream, SPFolderRestoreOption option, AveMetadata metadata, MetadataRestoreReport report)
    //    {
    //        var result = AveRestoreResult.Normal;
    //        try
    //        {
    //            this.mFolderInfo.VerifyItemMMSColumnValue = option.MetadataRestoreOption.VerifyDependency;
    //            this.mFolderInfo.KeepDefaultValue = option.MetadataRestoreOption.KeepColumnDefaultValue;
    //            this.mFolderInfo.KeepDestItemRowId = option.MetadataRestoreOption.KeepUniqueIdAndRowId;
    //            this.mFolderInfo.IsRestoreConnectorFolderProperties = option.MetadataRestoreOption.IsRestoreConnectorFolderProperties;

    //            var docData = metadata.GetMetadata<Dictionary<string, object>>();
    //            Dictionary<string, object> userData = null;

    //            if (option != null) { option.ProcessBasicInfoAction(docData); }

    //            var userDataMetadata = restoreStream.TryReadMetadata(AveMetadataType.DocData);

    //            if (userDataMetadata != null) { userData = userDataMetadata.GetMetadata<Dictionary<string, object>>(); }

    //            if (option.MetadataRestoreOption != null)
    //            {
    //                if (option.MetadataRestoreOption.VerifyDependency && userData != null)
    //                {
    //                    VerifyItemMetadataDependency(userData, option.MetadataRestoreOption);
    //                }
    //            }

    //            if (option.RestoreAction == SPFolderRestoreAction.Replace && userData != null)
    //            {
    //                var folder = this.parentSPListV1.ParentWeb.SPWeb.GetFolder(this.ServerRelativeUrl);

    //                if (folder.Exists)
    //                {
    //                    folder.Delete();
    //                    if (option.FolderDeleted != null) { option.FolderDeleted(); }
    //                }
    //            }

    //            this.RestoreOption.mAveRestoreMode = AveRestoreMode.Default;

    //            result = this.RestoreSelf(docData, userData);
    //            report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            report.Details.AnalyzeReport(this.GetReport());
    //        }
    //        catch (Exception ex)
    //        {
    //            report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Failed, ex.ToString());
    //            report.Details.AnalyzeReport(this.GetReport());
    //            throw;
    //        }
    //    }

    //    private void RestoreFolderItemMetadataDto(IAveRestoreStream restoreStream, SPFolderRestoreOption option, AveMetadata metadata, MetadataRestoreReport report)
    //    {
    //        var restoreDetails = new MetadataRestoreDetails();

    //        var folderMetadataDto = metadata.GetMetadata<SPFolderMetadataDto>();

    //        //TODO option需要控制下
    //        RestoreFolderMetadataDto(option, folderMetadataDto);

    //        report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //        report.Details.AnalyzeReport(this.GetReport());
    //    }

    //    private void RestoreFolderDocDataJunction(IAveRestoreStream restoreStream, SPFolderRestoreOption option, AveMetadata metadata, MetadataRestoreReport report)
    //    {
    //        if (this.IsNewCreated || option.RestoreAction == SPFolderRestoreAction.Default)
    //        {
    //            this.RestoreDataJunction(metadata.GetMetadata<List<Dictionary<string, object>>>());
    //            report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            report.Details.AnalyzeReport(this.GetReport());
    //        }
    //    }

    //    private void RestoreFolderLookupFieldGuidValue(IAveRestoreStream restoreStream, SPFolderRestoreOption option, AveMetadata metadata, MetadataRestoreReport report)
    //    {
    //        if (this.IsNewCreated || option.RestoreAction == SPFolderRestoreAction.Default)
    //        {
    //            var lookupFieldGuidValue = metadata.GetMetadata<Dictionary<string, string>>();
    //            this.RestoreLookupFieldGuidValue(lookupFieldGuidValue);
    //        }
    //    }

    //    private void RestoreFolderRoleAssignment(IAveRestoreStream restoreStream, SPFolderRestoreOption option, AveMetadata metadata, MetadataRestoreReport report)
    //    {
    //        if (option.RoleAssignmentsRestoreOption != null)
    //        {
    //            var roleAssignments = metadata.GetMetadata<List<AveRoleAssignmentInfo>>();

    //            using (var security = AveObjectSecurity.CreateInstance(this))
    //            {
    //                if (option.RoleAssignmentsRestoreOption.FilterRoleAssignments != null)
    //                {
    //                    roleAssignments = option.RoleAssignmentsRestoreOption.FilterRoleAssignments(roleAssignments);
    //                }

    //                security.RestoreRoleAssignments(roleAssignments, option.RoleAssignmentsRestoreOption.ToSecurityRestoreOption());
    //                security.SourceHasUniqueRoleAssignment = this.HasUniqueRoleAssignments;
    //                report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                report.Details.AnalyzeReport(security.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreFolderRoleAssignmentsDto(IAveRestoreStream restoreStream, SPFolderRestoreOption option, AveMetadata metadata, MetadataRestoreReport report)
    //    {
    //        if (option.RoleAssignmentsRestoreOption != null)
    //        {
    //            var roleAssignments = metadata.GetMetadata<AvePoint.Wrapper.Core.SPBackupDto.SPRoleAssignmentsDto>();

    //            using (var security = AveObjectSecurity.CreateInstance(this))
    //            {
    //                if (option.RoleAssignmentsRestoreOption.FilterRoleAssignments != null)
    //                {
    //                    roleAssignments.RoleAssignmentInfos = option.RoleAssignmentsRestoreOption.FilterRoleAssignments(roleAssignments.RoleAssignmentInfos);
    //                }

    //                if (option.RoleAssignmentsRestoreOption.RestoreInheritance)
    //                {
    //                    security.SourceHasUniqueRoleAssignment = !roleAssignments.IsInherit;
    //                }

    //                security.ParentSite.RestoreUser(roleAssignments.UserCache);
    //                security.ParentSite.RestoreGroup(roleAssignments.GroupCache);

    //                security.RestoreRoleAssignments(roleAssignments.RoleAssignmentInfos, option.RoleAssignmentsRestoreOption.ToSecurityRestoreOption());

    //                report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                report.Details.AnalyzeReport(security.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreFolderRoleAssignmentInheritStatus(IAveRestoreStream restoreStream, SPFolderRestoreOption option, AveMetadata metadata, MetadataRestoreReport report)
    //    {
    //        if (option.RoleAssignmentsRestoreOption != null && option.RoleAssignmentsRestoreOption.RestoreInheritance)
    //        {
    //            var inheritStatus = metadata.GetMetadata<bool>();

    //            using (var security = AveObjectSecurity.CreateInstance(this))
    //            {
    //                if (option.RoleAssignmentsRestoreOption.RestoreInheritance)
    //                {
    //                    security.SourceHasUniqueRoleAssignment = inheritStatus;
    //                }

    //                security.RestoreRoleAssignments(null, option.RoleAssignmentsRestoreOption.ToSecurityRestoreOption());

    //                report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                report.Details.AnalyzeReport(security.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreFolderDocImmedSubscriptions(IAveRestoreStream restoreStream, SPFolderRestoreOption option, AveMetadata metadata, MetadataRestoreReport report)
    //    {
    //        if (option.RestoreAction == SPFolderRestoreAction.Default || option.RestoreAction == SPFolderRestoreAction.Replace)
    //        {
    //            using (var alert = AveSPAlert.CreateInstance(this))
    //            {
    //                var data = metadata.GetMetadata<List<Dictionary<string, object>>>();

    //                if (data != null && data.Count > 0)
    //                {
    //                    foreach (var val in data)
    //                    {
    //                        alert.RestoreAlert(val, false);
    //                    }
    //                }
    //                report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                report.Details.AnalyzeReport(alert.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreFolderDocSchedSubscriptions(IAveRestoreStream restoreStream, SPFolderRestoreOption option, AveMetadata metadata, MetadataRestoreReport report)
    //    {
    //        if (option.RestoreAction == SPFolderRestoreAction.Default || option.RestoreAction == SPFolderRestoreAction.Replace)
    //        {
    //            using (var alert = AveSPAlert.CreateInstance(this))
    //            {
    //                var data = metadata.GetMetadata<List<Dictionary<string, object>>>();

    //                if (data != null && data.Count > 0)
    //                {
    //                    foreach (var val in data)
    //                    {
    //                        alert.RestoreAlert(val, true);
    //                    }
    //                }
    //                report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                report.Details.AnalyzeReport(alert.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreFolderSocialTag(IAveRestoreStream restoreStream, SPFolderRestoreOption option, AveMetadata metadata, MetadataRestoreReport report)
    //    {
    //        if ((this.IsNewCreated || option.RestoreAction == SPFolderRestoreAction.Default) && AvePoint.Common.AveEnv.IsMoss)
    //        {
    //            using (var socialComment = new AveSPSocialComment(this.TagUrl, ParentSite))
    //            {
    //                var socialComments = metadata.GetMetadata<List<AveSocialCommentInfo>>();

    //                socialComment.Restore(socialComments);

    //                report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                report.Details.AnalyzeReport(socialComment.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreFolderSocialComment(IAveRestoreStream restoreStream, SPFolderRestoreOption option, AveMetadata metadata, MetadataRestoreReport report)
    //    {
    //        if ((this.IsNewCreated || option.RestoreAction == SPFolderRestoreAction.Default) && AvePoint.Common.AveEnv.IsMoss)
    //        {
    //            using (var socialTag = new AveSPSocialTag(this.TagUrl, ParentSite))
    //            {
    //                var socialTags = metadata.GetMetadata<List<AveSocialTagInfo>>();

    //                socialTag.Restore(socialTags);

    //                report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                report.Details.AnalyzeReport(socialTag.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreFolderDocumentTagging(IAveRestoreStream restoreStream, SPFolderRestoreOption option, AveMetadata metadata, MetadataRestoreReport report)
    //    {
    //        if ((this.IsNewCreated || option.RestoreAction == SPFolderRestoreAction.Default) && AvePoint.Common.AveEnv.IsMoss)
    //        {
    //            using (var documentTagging = new AveDocumentTagging(this.TagUrl, ParentSite))
    //            {
    //                var tags = metadata.GetMetadata<List<AveDocumentTaggingInfo>>();

    //                documentTagging.Restore(tags);
    //                report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                report.Details.AnalyzeReport(documentTagging.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreListMetadataService(IAveRestoreStream restoreStream, SPFolderRestoreOption option, AveMetadata metadata, MetadataRestoreReport report)
    //    {
    //        var mmsData = metadata.GetMetadata<List<AveTermStoreInfo>>();
    //        this.mAveParentSite.RestoreMetadataInfo(mmsData);
    //    }

    //    private void RestoreFolderWorkflowInstance(IAveRestoreStream restoreStream, SPFolderRestoreOption option, AveMetadata metadata, MetadataRestoreReport report)
    //    {
    //        EnsureWFInstanceOption(option);

    //        if (option.WorkflowRestoreOption.InstanceRestoreOption.RestoreInstance)
    //        {
    //            var wfInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
    //            var wfResolution = WFConflictResolution.Instance;
    //            wfResolution.InstanceOption = (WFInstanceConflictResolutionOption)option.WorkflowRestoreOption.InstanceRestoreOption.ConflictResolutionOption;
    //            option.WorkflowRestoreOption.InstanceRestoreOption.ToWFInstanceSetting();

    //            foreach (var unit in wfInfo)
    //            {
    //                wfResolution.RestoreInstanceData(unit, this);
    //            }
    //            if (wfInfo.Count > 0) { this.parentSPListV1.ReloadList(); }
    //            using (var workflowReport = wfResolution.GetReport())
    //            {
    //                report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                report.Details.AnalyzeReport(workflowReport);
    //            }
    //        }
    //    }

    //    private void RestoreFolderWorkflowSchedule(IAveRestoreStream restoreStream, SPFolderRestoreOption option, AveMetadata metadata, MetadataRestoreReport report)
    //    {
    //        EnsureWFInstanceOption(option);

    //        if (option.WorkflowRestoreOption.InstanceRestoreOption.RestoreInstance)
    //        {
    //            var wfInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
    //            var wfResolution = WFConflictResolution.Instance;

    //            foreach (var unit in wfInfo)
    //            {
    //                wfResolution.RestoreScheduleData(unit, this.SPListItem);
    //            }
    //            using (var workflowReport = wfResolution.GetReport())
    //            {
    //                report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                report.Details.AnalyzeReport(workflowReport);
    //            }
    //        }
    //    }
    //}
}
