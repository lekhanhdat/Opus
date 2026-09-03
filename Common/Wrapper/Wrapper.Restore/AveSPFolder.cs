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
using AvePoint.GCommon;
using System.Data.SqlClient;
using System.Xml;
using AvePoint.Common;
using AvePoint.Wrapper.Common.Common.Utility;
using AvePoint.Wrapper.Resource;
using AvePoint.GCommon.Contract.CodeReview;


namespace AvePoint.Wrapper.Restore
{
    [AveCodeReview("2012/03/06", "qwhu@avepoint.com", "", new string[] { CodeReviewConstants.CHECK_LIST_ID_CS_1 }, "ADO-26504", true)]
    public class AveSPFolder : RestoreableObject,IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(AveSPFolder));

        private AveSPList mAveSPList;
        private IAveBackupRestoreQueryService mQueryService;
        private AveSPFolder mParentFolder;
        private IAveFolder mSPFolder;
        private string mServerRelativeUrl;

        private AveSPItem mAveSPItem;
        private AveSPItem mEnsureCTFieldItem;//只为ensure field和contenttype使用

        [ThreadStatic]
        private static CurrentRestoreDocStatus currentDocStatus;

        private bool mHasMoveUp;
        private AveSPWeb mAveSPWeb;
        private string mUrl;
        private string mSrcUrl;
        private long mSize;
        private Dictionary<string, object> documentSetMetaInfo;
        private IReport report = new AveWrapperReport();

        private AveFolderInfo mFolderInfo = new AveFolderInfo();

        private AveSPSite mAveParentSite;
        private Dictionary<string, Dictionary<Guid, Guid>> mWebPartMapping = new Dictionary<string, Dictionary<Guid, Guid>>();

        private bool? mIsWithinFormFolder = null;
        public bool? IsWithinFormFolder
        {
            get { return mIsWithinFormFolder; }
            set { mIsWithinFormFolder = value; }
        }

        public IReport GetReport()
        {
            return report;
        }

        internal Dictionary<string, Dictionary<Guid, Guid>> WebPartMapping
        {
            get { return mWebPartMapping; }
        }

        public AveSPSite ParentSite
        {
            get { return mAveParentSite; }
        }

        [ThreadStatic]
        private static RestoringDto mRestoringItem;
        public RestoringDto RestoringItem
        {
            get
            {
                if (mRestoringItem == null)
                {
                    mRestoringItem = new RestoringDto();
                }
                return mRestoringItem;
            }
            set { mRestoringItem = value; }
        }

        public Guid Id
        {
            get { return mFolderInfo.GUID; }
            set { mFolderInfo.GUID = value; }
        }

        public string Name
        {
            get { return mFolderInfo.Name; }
            set { mFolderInfo.Name = value; }
        }

        public bool IsNewCreated
        {
            get { return mFolderInfo.IsNewCreated; }
            set { mFolderInfo.IsNewCreated = value; }
        }
        public bool HasMoveUp
        {
            get { return mHasMoveUp; }
        }
        public AveSPItem AveSPItem
        {
            get { return mAveSPItem; }
        }

        public AveSPItem EnsureCTFieldItem
        {
            get { return mEnsureCTFieldItem; }
        }

        public AveSPList ParentList
        {
            get { return mAveSPList; }
        }

        public AveSPFolder ParentFolder
        {
            get { return mParentFolder; }
        }

        public CurrentRestoreDocStatus CurrentDocStatus
        {
            get { return currentDocStatus; }
            set { currentDocStatus = value; }
        }

        public IAveBackupRestoreQueryService QueryService
        {
            get { return mQueryService; }
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

        //public AveSPItem AveItem
        //{
        //    get
        //    {
        //        return mAveSPItem;
        //    }
        //}


        public string Url
        {
            get { return mUrl; }
        }

        public string SrcUrl
        {
            get { return mSrcUrl; }
        }

        public long Size
        {
            get { return mSize; }
        }

        public string OwnerLoginName
        {
            get { return mAveSPItem == null ? null : mAveSPItem.OwnerLoginName; }
        }

        public AveSPFolder()
        {
        }

        public string TagUrl
        {
            get
            {
                if ((mQueryService as IAveConnectorQueryService).IsSP2010SP1(mAveParentSite.SPSite.ID))//sp1
                {
                    return mSPFolder.ParentWeb.Url.TrimStart('/') + "/" + mSPFolder.Url.Trim('/');
                }
                else
                {
                    if (mAveSPList.SPList != null && mAveSPList.SPList.BaseType != AveBaseType.DocumentLibrary)
                    {
                        if (!string.IsNullOrEmpty(mAveSPList.SPList.DefaultDisplayFormUrl))
                        {
                            string fileUrl = mAveSPList.SPList.DefaultDisplayFormUrl.TrimStart('/').Substring(mAveSPList.ParentWeb.SPWeb.ServerRelativeUrl.TrimStart('/').Length).TrimStart('/');
                            return mAveSPList.ParentWeb.SPWeb.Url.TrimEnd('/') + "/" + fileUrl + "?ID=" + mAveSPItem.RowId;
                        }
                        return string.Empty;//return AveSPUtility.GetServerUrl(mAveParentSite.SPSite) + "/" + mAveSPItem.ScopeUrl;
                    }
                    else
                    {
                        return mSPFolder.ParentWeb.Url.TrimStart('/') + "/" + mSPFolder.Url.Trim('/');
                    }
                }
            }
        }

        //only for list root folder, we need to push it to stack
        public AveSPFolder(AveSPList aveList, string name)
        {
            mAveSPList = aveList;
            mAveParentSite = aveList.ParentSite;
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
            mQueryService = mAveSPList.QueryService;
            mFolderInfo.SiteId = mAveSPList.ParentWeb.ParentSite.SPSite.ID;
            mFolderInfo.Name = name;
            mFolderInfo.ParentId = aveList.RootFolder.UniqueId;
            //mAveSPItem = new AveSPItem(info, AveItemType.Folder, mParentFolder, mSqlConn);
            mRestoringItem = new RestoringDto();
            mFolderInfo.IsNewCreated = mAveSPList.IsNewCreated;
            mFolderInfo.KeepDefaultValue = mAveParentSite.KeepDefaultValue;
            mFolderInfo.VerifyItemMMSColumnValue = mAveParentSite.VerifyItemMMSColumnValue;
        }

        public AveSPFolder(AveSPFolder aveFolder, string name)
        {
            aveFolder.ParentList.ParentWeb.ReloadWebAndParentInternalForSPRequestTimeout(false);
            mParentFolder = aveFolder;
            mAveParentSite = aveFolder.ParentSite;
            mAveSPList = aveFolder.ParentList;

            int pos = name.IndexOf(':');
            if (pos >= 0)
            {
                name = name.Substring(0, pos);
            }
            mQueryService = mAveSPList.QueryService;
            mFolderInfo.SiteId = mAveSPList.ParentWeb.ParentSite.SPSite.ID;
            mFolderInfo.ParentId = aveFolder.Id;
            mFolderInfo.Name = name;
            mFolderInfo.ItemType = AveItemType.Folder;
            mFolderInfo.ServerRelativeUrl = aveFolder.ServerRelativeUrl.TrimEnd('/') + "/" + name.TrimStart('/'); ;
            if (mFolderInfo.ServerRelativeUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                mFolderInfo.ServerRelativeUrl = mFolderInfo.ServerRelativeUrl.TrimEnd('/');
            }
            //mAveSPItem = new AveSPItem(info, AveItemType.Folder, mParentFolder, mSqlConn);
            mRestoringItem = new RestoringDto();
            mFolderInfo.IsNewCreated = mAveSPList.IsNewCreated;
            mServerRelativeUrl = mFolderInfo.ServerRelativeUrl;
            mFolderInfo.KeepDefaultValue = mAveParentSite.KeepDefaultValue;
            mFolderInfo.VerifyItemMMSColumnValue = mAveParentSite.VerifyItemMMSColumnValue;
            //mAveSPItem = new AveSPItem(mFolderInfo, AveItemType.Folder, mParentFolder, mQueryService);
            mEnsureCTFieldItem = new AveSPItem(mParentFolder);

        }

        /// <summary>
        /// only used for wrapper test purpose
        /// </summary>
        /// <param name="aveList"></param>
        /// <param name="restoreStream"></param>
        /// <param name="folderRelativeUrl"></param>
        /// <param name="isRestoreFolder"></param>
        public AveSPFolder(AveSPList aveList, IAveRestoreStream restoreStream, string folderRelativeUrl, bool isRestoreFolder)
        {
            mAveSPList = aveList;
            mAveSPWeb = aveList.ParentWeb;
            mQueryService = aveList.QueryService;
            mAveParentSite = aveList.ParentSite;
            mAveSPItem = new AveSPItem(aveList, restoreStream);
            mSPFolder = mAveSPWeb.SPWeb.GetFolder(folderRelativeUrl);
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
            mFolderInfo.KeepDefaultValue = mAveParentSite.KeepDefaultValue;
            mFolderInfo.VerifyItemMMSColumnValue = mAveParentSite.VerifyItemMMSColumnValue;
            mRestoringItem = new RestoringDto();
        }

        public string ResetAvailableName()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFolder.ResetAvailableName"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }
        /// <summary>
        /// 检查文件夹名是否冲突，如果冲突重新命名
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="needIncluded">检查文件夹名是否从本身开始</param>
        /// <returns></returns>
        public string ResetAvailableName(string oldName, bool needIncluded)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFolder.ResetAvailableName"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }
        public void ResetName(string newName)
        {
            mFolderInfo.Name = newName;
        }

        public void Reload()
        {
            if (mSPFolder != null)
            {
                IAveFolder spFolder = GetFolder(mSPFolder.ParentFolder, mSPFolder.Name);
                if (spFolder == null)
                {
                    log.Log(AveLogLevel.WARN, string.Format("Cannot find folder:{0}", mSPFolder.ServerRelativeUrl));
                    //mLog.Warn("Cannot find folder '{0}'", mSPFolder.ServerRelativeUrl);
                }
                else
                {
                    mSPFolder = spFolder;
                }
            }
        }
        public void ReloadFolder()
        {
            if (mSPFolder != null)
            {
                try
                {
                    IAveFolder spFolder = mAveSPList.ParentWeb.SPWeb.GetFolder(mSPFolder.ServerRelativeUrl);
                    if (spFolder == null)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("Cannot find folder:{0}", mSPFolder.ServerRelativeUrl));
                        //mLog.Warn("Cannot find folder '{0}'", mSPFolder.ServerRelativeUrl);
                    }
                    else
                    {
                        mSPFolder = spFolder;
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.ReloadFolderError, e);
                }
            }
        }
        /// <summary>
        /// just init spfolder object, no need to restore, only for replicator.
        /// </summary>
        public void InitSPFolder()
        {
            InitSPFolder(false);
        }

        public void InitSPItemForHighSpeed()
        {
            mAveSPItem = new AveSPItem(mFolderInfo, AveItemType.Folder, mParentFolder, mQueryService);
        }
        public void ResetParentFolder(int maxUrlLength, bool needResetName)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFolder.ResetParentFolder"))
            {
#endif
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
                    if (mAveSPItem != null)
                    {
                        mAveSPItem.ParentFolder.SPFolder = mParentFolder.SPFolder;
                    }
                }
                catch (Exception ex)
                {
                    log.Warn("An error occurred while Resetting Parent Folder ." + ex.ToString());
                }
#if PerformanceLog
            }
#endif
        }

        public void ResetParentFolder(bool moveUptoRootFolder, bool moveUptoHighLevelFolder, bool needResetName)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFolder.ResetParentFolder"))
            {
#endif
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
                    if (mAveSPItem != null)
                    {
                        mAveSPItem.ParentFolder.SPFolder = mParentFolder.SPFolder;
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
#if PerformanceLog
            }
#endif
        }

        public void ResetParentFolder(AveSPFolder parentFolder)
        {
            mParentFolder = parentFolder;
            mFolderInfo.ParentId = parentFolder.Id;
            mServerRelativeUrl = mParentFolder.SPFolder.ServerRelativeUrl + "/" + mFolderInfo.Name;
            if (mAveSPItem != null)
            {
                mAveSPItem = new AveSPItem(mFolderInfo, AveItemType.Folder, mParentFolder, mQueryService);
            }
            mHasMoveUp = true;
        }

        public void InitSPFolder(bool tryCreate)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFolder.InitSPFolder"))
            {
#endif
                if (mSPFolder == null)
                {
                    mSPFolder = GetFolder(mParentFolder.SPFolder, mFolderInfo.Name);
                }
                if (tryCreate && (mSPFolder == null || mSPFolder.Exists == false) && mAveSPList.SPList != null)
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
                            log.Info($"InitSPFolder.Begin create folder.URL:{mSPFolder.ServerRelativeUrl}.");
                            //item = mAveSPList.SPList.Items.Add(mParentFolder.SPFolder.ServerRelativeUrl, AveFileSystemObjectType.Folder, mFolderInfo.Name);
                            item = mAveSPList.SPList.AddItemUsingPath(mParentFolder.SPFolder.ServerRelativeUrl, AveFileSystemObjectType.Folder, mFolderInfo.Name);
                            item["Title"] = mFolderInfo.Name;
                            item.SystemUpdate(false);
                            mSPFolder = item.Folder;
                            log.Info("Create folder in InitSPFolder.Url:{0}", mSPFolder.ServerRelativeUrl);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(string.Format("Can not create folder:{0}. Reason:{1})", mFolderInfo.Name, ex.ToString()));
                    }
                }
#if PerformanceLog
            }
#endif
        }


        private IAveFolder GetFolder(IAveFolder parentFolder, string name)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFolder.GetFolder"))
            {
#endif
                IAveFolder folder = null;
                try
                {
                    if (name.Equals("{System Folder}"))
                    {
                        folder = parentFolder;
                        mFolderInfo.GUID = folder.UniqueId;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(mFolderInfo.ServerRelativeUrl))
                        {
                            try
                            {
                                folder = mAveSPList.ParentWeb.SPWeb.GetFolder(mFolderInfo.ServerRelativeUrl);
                                if (folder != null)
                                {
                                    mFolderInfo.GUID = folder.UniqueId;
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFolderByNameError, ex.ToString());
                                //no exist
                                folder = null;
                            }
                        }
                        //folder = parentFolder.SubFolders[name];
                        //mFolderInfo.GUID = folder.UniqueId;
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
                            if (folder != null)
                            {
                                mFolderInfo.GUID = folder.UniqueId;
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

                return folder;
#if PerformanceLog
            }
#endif
        }



      

        public void RestoreSelf(
               Dictionary<string, object> allDocData,
               Dictionary<string, object> allUserData,
            List<Dictionary<string, object>> dataJunction = null)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFolder.RestoreSelf"))
            {
#endif
                AveRestoreResult result = AveRestoreResult.Normal;
                try
                {
                    ProcessPreCondition(allDocData, allUserData, dataJunction);
                    //TODOLMM  if SPList is null, will thow exception,  (LMM, Please think more about System List--xhyou)
                    if (mFolderInfo.VerifyItemMMSColumnValue)
                    {
                        if (this.ParentSite.MetadataService == null)
                        {
                            this.ParentSite.MetadataService = new AveMetadataService(this.ParentSite);
                        }
                        //保证item的MetadataColumn的term能够存在或还原成功，才能允许继续restore item
                        if (this.ParentFolder.ParentList.SPList != null && mFolderInfo.FieldsInfo.TaxonomyFieldsInMapping != null && mFolderInfo.FieldsInfo.TermIdMapping != null && !this.ParentSite.MetadataService.VerifyMetadataColumnValue(mFolderInfo, this.ParentFolder.ParentList.SPList, mFolderInfo.FieldsInfo.TaxonomyFieldsInMapping, mFolderInfo.FieldsInfo.TermIdMapping, mAveParentSite.ObjectModelFactory))
                        {
                            log.Log(AveLogLevel.WARN, string.Format("VerifyMetadataColumnValue failed, shouldn't restore folder:{0}", mFolderInfo.Name));
                            throw new AveVerifyItemMetadataValueNotFoundException("Verify item metadata column value failed");
                        }
                    }

                    result = mParentFolder.SPFolder.RestoreFolder(mFolderInfo, allDocData, allUserData);
                    mAveSPItem.CacheMutiLookupValue();
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("An Error occurred while restore folder. {0}" + mFolderInfo.Name, ex);
                    report.AddDetail(new AveWrapperReportDto(mFolderInfo.Name, mFolderInfo.Name, AveReportObjectType.Folder, AveStatus.Skipped, "You don't have permission to Add Folder. " + ex.Message));
                    throw ex;
                }
                catch (AveRestoreException ex)
                {
                    result = ex.Result;
                }
                //AveRestoreResult result = mAveSPList.SPList.RestoreFolder(mFolderInfo, allDocData, allUserData);

                if (result != AveRestoreResult.Failed && result != AveRestoreResult.Omit && mFolderInfo.RowId > 0)
                {
                    mAveSPList.AveFields.ResetNotUpdateLookupFieldValue(mFolderInfo.RowId);
                    mAveSPList.AveFields.ResetNintexFormDataFieldValue(mFolderInfo.RowId);
                    mAveSPItem.AddItemMapping(mFolderInfo.OriginalRowId);
                }

                mSPFolder = mFolderInfo.AveItem.Folder;
                if (documentSetMetaInfo != null)
                {
                    mFolderInfo.MappingManager.ListMappingManager.DocumentSetGuidMetaInfoMapping.Add(mSPFolder.UniqueId, documentSetMetaInfo);
                }
                //SAAS-3849       替换notebook 的Unique ID   
                if (allDocData.ContainsKey("Id") && (Guid)allDocData["Id"] != Guid.Empty)
                {
                    Guid sourceItemUniqueId = (Guid)allDocData["Id"];
                    if (!this.mAveParentSite.MappingManager.SiteMappingManager.ItemGuidForReplicatorConflict.ContainsKey(sourceItemUniqueId))
                    {
                        this.mAveParentSite.MappingManager.SiteMappingManager.ItemGuidForReplicatorConflict[sourceItemUniqueId] = (Guid)mSPFolder.UniqueId;
                    }
                }
#if PerformanceLog
            }
#endif
        }

        private void ProcessPreCondition(Dictionary<string, object> allDocData, Dictionary<string, object> allUserData, List<Dictionary<string, object>> dataJunction)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFolder.ProcessPreCondition"))
            {
#endif
                try
                {
                    mAveSPItem = new AveSPItem(mFolderInfo, AveItemType.Folder, mParentFolder, mQueryService);
                    mAveSPItem.SetRestoreOption(mRestoreOption);
                    mAveSPItem.ProcessPreCondition(allDocData, allUserData);
                    RestoreOption.mAveItemRestoreOption.DELETE_ITEM = mAveSPList.RestoringFolder.Init(mFolderInfo.Name, CheckRestoreOption(IsNewCreated, AveRestoreMode.OverWrite), RestoreOption.mAveItemRestoreOption.DELETE_ITEM);
                    mFolderInfo.SettingInfo.DELETE_ITEM = RestoreOption.mAveItemRestoreOption.DELETE_ITEM;
                    mFolderInfo.RestoringItem = mAveSPList.RestoringFolder;
                    mFolderInfo.ParentListName = mAveSPList.Name;
                    mFolderInfo.ParentListIsSystem = mAveSPList.IsSystemList;
                    mFolderInfo.ListRootFolderId = mAveSPList.RootFolder.UniqueId;
                    mFolderInfo.FieldsInfo.Fields = ParentList.AveFields.GetFieldValues(string.Empty, mFolderInfo.OriginalRowId, mFolderInfo.OriginalVersion, allUserData, true);
                    mFolderInfo.FieldsInfo.MultilookupFields = mAveSPItem.GetDataJunction(dataJunction);
                    mAveSPItem.GetTaxonomyTermIdMapping(mFolderInfo.FieldsInfo.Fields, mFolderInfo);
                    mFolderInfo.IsOverWrite = CheckRestoreOption(mFolderInfo.IsNewCreated, AveRestoreMode.OverWrite);
                    //mFolderInfo.NeedSetNullFields = ParentList.SetNeedSetNullFields(mFolderInfo.FieldsInfo.Fields);
                    mFolderInfo.SourceSiteInfo = ParentList.ParentSite.SourceSiteInfo;
                    mFolderInfo.ParentSiteServerRelativeUrl = ParentList.ParentSite.ServerRelativeUrl;
                    if (mAveSPList.IsCommunitySiteDiscussionList)
                    {
                        mFolderInfo.IsInCommunityDiscussion = true;
                    }
                    if (allDocData.ContainsKey("HasUniqueRoleAssignments"))
                    {
                        mFolderInfo.HasUniqueRoleAssignments = (bool)allDocData["HasUniqueRoleAssignments"];
                        allDocData.Remove("HasUniqueRoleAssignments");
                    }
                    if (allDocData.ContainsKey("snapshots"))
                    {
                        documentSetMetaInfo = new Dictionary<string, object>();
                        documentSetMetaInfo["snapshots"] = allDocData["snapshots"].ToString();
                        documentSetMetaInfo["Editor"] = mFolderInfo.FieldsInfo.Fields["Editor"];
                        documentSetMetaInfo["Modified"] = allUserData["Modified"];
                        allDocData.Remove("snapshots");
                    }
                    if (allDocData.ContainsKey("vti_contenttypeorder") && allDocData["vti_contenttypeorder"] != null)  //restore folder contenttype order 替换contenttypeid
                    {
                        try
                        {
                            log.Info("the folder contains contenttypeorder,start to replaced the contenttypeid");
                            string newContentTypeOrder = string.Empty;
                            Dictionary<string, string> contentTypeIdMapping = new Dictionary<string, string>();
                            string[] contentTypeOrder = allDocData["vti_contenttypeorder"].ToString().Split(',');
                            foreach (string order in contentTypeOrder)
                            {
                                if (this.ParentSite.MappingManager.SiteMappingManager.ListContentTypeIdMapping.TryGetValue(this.mAveSPList.RootFolder.ParentListId, out contentTypeIdMapping))
                                {
                                    string newOrder = string.Empty;
                                    if (contentTypeIdMapping.TryGetValue(order, out newOrder))
                                    {
                                        newContentTypeOrder = String.Concat(newContentTypeOrder, "," + newOrder);
                                    }
                                }
                            }
                            newContentTypeOrder = newContentTypeOrder.TrimStart(',');
                            allDocData["vti_contenttypeorder"] = string.IsNullOrEmpty(newContentTypeOrder) ? null : newContentTypeOrder;
                        }
                        catch (Exception e)
                        {
                            log.Error("update contenttypeorder failed.due to {0}.", e);
                        }
                    }
                    if (mAveSPList != null && mAveSPList.IsTaxonomyList)
                    {
                        //return AveRestoreResult.Omit;
                        throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                    }
                }
                catch (AveRestoreException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.PreProcessConditionError, ex);
                }
#if PerformanceLog
            }
#endif
        }

      





        public string ServerRelativeUrl
        {
            get { return mServerRelativeUrl; }
        }

        /*public void GetOrCreateFolder()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPFolder.GetOrCreateFolder"))
            {
#endif
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

#if PerformanceLog
            }
#endif
        }*/

        public void Dispose()
        {
            if(report != null)
            {
                report.Dispose();
            }
        }
    }
}
