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
using AvePoint.GCommon.Contract.CodeReview;
using System.IO;
using System.Xml;
using System.Collections;
using AvePoint.Wrapper.Core.Common;
using AvePoint.Wrapper.Core.SPBackupDto;
using AvePoint.Wrapper.Core.SPRestore;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AvePoint.GCommon.Utility;
using LS.SPWorkflowProcessor;
using System.Text.RegularExpressions;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    [AveCodeReviewAttribute("2012/03/09", "Navy.Li@avepoint.com", "Bingkun.Wang@AvePoint.com",
        new string[] { CodeReviewConstants.CHECK_LIST_ID_CO_1, 
                       CodeReviewConstants.CHECK_LIST_ID_CO_6, 
                       CodeReviewConstants.CHECK_LIST_ID_FA_4 }, null, true)]
    public class AveSPDoc : AveSPItem, AvePoint.Wrapper.Restore.IAveSPDoc
    {
        private AveItemHoldRecord mFileHold;
        private List<IAveListItem> mHoldItems;
        private Hashtable mHTMetaInfo;
        private IAveRestoreStream mReceiver;
        protected AveDocumentInfo mDocumentInfo
        {
            get
            {
                return mBaseItemInfo as AveDocumentInfo;
            }
            set
            {
                this.mBaseItemInfo = value;
            }
        }

        public IAveFile SPFile
        {
            get
            {
                if (mAveItem != null)
                {
                    return mAveItem.File;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                mAveItem.File = value;
            }
        }
        public AveViewDocInfo AveView
        {
            get { return mDocumentInfo.AveView; }
        }
        public bool IsView 
        {
            get { return mDocumentInfo.IsView; }
        }
        public IAveRestoreStream Receiver
        {
            get { return this.mReceiver; }
            set { this.mReceiver = value; }
        }
        public bool IsNewCreateView
        {
            get { return mDocumentInfo.IsView && mDocumentInfo.IsNewCreated; }
        }
        public bool HasStream
        {
            get { return mBaseItemInfo.HasStream; }
            set { this.mBaseItemInfo.HasStream = value; }
        }
        public string TagUrl
        {
            get
            {
                if (SPFile != null && SPFile.ParentFolder != null && SPFile.ParentFolder.ParentWeb != null && SPFile.ParentFolder.ParentWeb.Url != null)
                {
                    if (!string.IsNullOrEmpty(this.SPFile.Url))
                    {
                        return this.SPFile.ParentFolder.ParentWeb.Url.TrimEnd('/') + "/" + this.SPFile.Url.TrimStart('/');
                    }
                    else {
                        string fileUrl=(mParentFolder.ServerRelativeUrl + "/" + mDocumentInfo.Name).Substring(SPWeb.ServerRelativeUrl.Length);
                        return this.SPFile.ParentFolder.ParentWeb.Url.TrimEnd('/') + fileUrl;
                    }
                }
                else if (SPFile == null)
                {
                    if (string.IsNullOrEmpty(mDocumentInfo.Url))
                    {
                        string fileUrl = (mParentFolder.ServerRelativeUrl + "/" + mDocumentInfo.Name).Substring(SPWeb.ServerRelativeUrl.Length);
                        mDocumentInfo.Url = SPWeb.Url + fileUrl;
                    }
                    return mDocumentInfo.Url;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public bool SetVerifyPageLayoutOption
        {
            set { mRestoreOption.mAveItemRestoreOption.VerifyPageLayout = value; mDocumentInfo.VerifyPageLayout = value; }
        }

        public bool FindViewByTitle
        {
            get { return mDocumentInfo.FindViewByTitle; }
            set { mDocumentInfo.FindViewByTitle = value; }
        }

        #region Obsolete field&property
        [Obsolete("use SPWeb instead, will remove later")]
        public IAveWeb Web
        {
            get { return this.SPWeb; }
            set { this.SPWeb = value; }
        }
        [Obsolete("wrapper already change current class inherit AveSPItem, will remove AveSPItem property later")]
        public AveSPItem AveSPItem
        {
            get { return this; }
        }
        [Obsolete("no use now, will remove later")]
        public bool NeedChangeItemId
        {
            get { return mDocumentInfo.NeedChangeItemId; }
            set { mDocumentInfo.NeedChangeItemId = value; }
        }
        [Obsolete("no use now, will remove later")]
        public IAveView SPView
        {
            get { return mDocumentInfo.AveItem.View; }
        }
        [Obsolete("no use now, will remove later")]
        public bool IsCurrentVersion
        {
            get { return mDocumentInfo.IsCurrentVersion; }
        }
        [Obsolete("no use now, will remove later")]
        public string Url
        {
            get
            {
                if (string.IsNullOrEmpty(mDocumentInfo.Url))
                {
                    string fileUrl = (mParentFolder.ServerRelativeUrl + "/" + mDocumentInfo.Name).Substring(SPWeb.ServerRelativeUrl.Length);
                    mDocumentInfo.Url = SPWeb.Url + fileUrl;
                }
                return mDocumentInfo.Url;
            }
        }
        [Obsolete("no use now, will remove later")]
        public bool? ConflictWithDocument
        {
            get
            {
                if (mDocumentInfo.RestoringItem == null)
                {
                    return null;
                }
                if (mDocumentInfo.IsView)
                {
                    return mDocumentInfo.RestoringItem.ConflictWithDocument;
                }
                //Overwrite the whole item
                if (mDocumentInfo.RestoringItem.OverwriteAllVersion)
                {
                    return true;
                }
                //Do not conflict
                if (!mDocumentInfo.RestoringItem.ConflictWithDocument)
                {
                    return false;
                }
                return !IsNewCreated;
            }
        }
        [Obsolete("no use now, will remove later")]
        public long Size
        {
            get { return 0; }
        }
        #endregion


        [Obsolete("This constructor is only used for unit test")]
        public AveSPDoc()
        { }

        public AveSPDoc(AveSPFolder aveFolder, string name)
            : base(AveItemType.Document, aveFolder, name)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.Constructor"))
            {

                if (this.ParentFolder.ParentList.ParentWeb.WebInfo != null)
                {
                    mDocumentInfo.SourceWebUrl = this.ParentFolder.ParentList.ParentWeb.WebInfo.Url;
                }
                if (this.ParentFolder.ParentList.SPList != null && this.ParentFolder.ParentList.SPList.BaseTemplate == AveListTemplateType.MasterPageCatalog)
                {
                    mDocumentInfo.ParentLibraryIsMasterPageGallery = true;
                }                
                this.IsNewCreated = mParentFolder.IsNewCreated;
            }
        }

        public AveSPDoc(AveSPFolder aveFolder, string name, int rowId)
            :this(aveFolder, name)
        {
            if (rowId <= 0)
            {
                throw new ArgumentOutOfRangeException("rowId");
            }
            this.mDocumentInfo.RowId = rowId;
        }

        /// <summary>
        /// this constructor is used for postAction, only for internal use, will change it access level to internal
        /// </summary>
        /// <param name="aveSite"></param>
        public AveSPDoc(AveSPSite aveSite)
            : base(aveSite)
        {
            mBaseItemInfo = new AveDocumentInfo();
        }

        public void ResetParentFolder(int maxUrlLength)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.ResetParentFolder"))
            {

                try
                {
                    while (mParentFolder.SPFolder.Url.Length + mParentFolder.SPFolder.ParentWeb.ServerRelativeUrl.Length + mDocumentInfo.Name.Length + 1 > maxUrlLength && !mParentFolder.SPFolder.Url.Equals(mParentFolder.ParentList.SPList.RootFolder.Url))
                    {
                        mParentFolder.SPFolder = mParentFolder.SPFolder.ParentFolder;
                        mDocumentInfo.HasMoveUp = true;
                    }
                    mDocumentInfo.ParentId = mParentFolder.SPFolder.UniqueId;
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

        public void ResetParentFolder(bool moveUptoRootFolder, bool moveUptoHighLevelFolder)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.ResetParentFolder"))
            {

                try
                {
                    if (moveUptoRootFolder)
                    {
                        mParentFolder.SPFolder = mParentFolder.ParentList.SPList.RootFolder;//.SPFolder.ParentWeb.Lists[mAveSPFolder.SPFolder.ParentListId].RootFolder;
                        mDocumentInfo.ParentId = mParentFolder.SPFolder.UniqueId;
                        //if (mAveSPItem != null)
                        {
                            this.ParentFolder.SPFolder = mParentFolder.SPFolder;
                        }
                        mDocumentInfo.HasMoveUp = true;
                    }
                    else if (moveUptoHighLevelFolder)
                    {
                        mParentFolder.SPFolder = mParentFolder.SPFolder.ParentFolder;
                        mDocumentInfo.ParentId = mParentFolder.SPFolder.UniqueId;
                        //if (mAveSPItem != null)
                        {
                            this.ParentFolder.SPFolder = mParentFolder.SPFolder;
                        }
                        mDocumentInfo.HasMoveUp = true;
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
            mDocumentInfo.ParentId = parentFolder.Id;
            //if (mAveSPItem != null)
            //{
            //    mAveSPItem = new AveSPItem(mDocumentInfo, AveItemType.Document, mAveSPFolder, mQueryService);
            //}
            mDocumentInfo.HasMoveUp = true;
        }

        public string ResetAvailableName()
        {
            return ResetAvailableName(DateTime.MinValue,false);
        }
        public string ResetAvailableName(DateTime timeLastModified, bool isLinkFile)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.ResetAvailableName"))
            {

                string newFileName = mDocumentInfo.Name;
                try
                {
                    DateTime destTimeLastModified = DateTime.MinValue;
                    if (!CheckFileExist(mDocumentInfo.Name, ref destTimeLastModified))
                    {
                        return newFileName;
                    }

                    if (!RestoreOption.mAveItemRestoreOption.SKIP_IF_SAME_MODIFIEDTIME || destTimeLastModified == DateTime.MinValue || destTimeLastModified != timeLastModified)
                    {
                        string extension = string.Empty;
                        string prevName = mDocumentInfo.Name;
                        int pos = mDocumentInfo.Name.LastIndexOf('.');
                        if (pos > 0 && isLinkFile)
                        {
                            var realPos = mDocumentInfo.Name.LastIndexOf('.', pos - 1);
                            if (realPos > 0)
                            {
                                pos = realPos;
                            }
                        }
                        if (pos > 0)
                        {
                            extension = mDocumentInfo.Name.Substring(pos, mDocumentInfo.Name.Length - pos);
                            prevName = mDocumentInfo.Name.Substring(0, pos);
                        }
                        for (int i = 1; i <= 1000; ++i)
                        {
                            StringBuilder temp = new StringBuilder(prevName);
                            temp.Append("_");
                            temp.Append(i.ToString());
                            temp.Append(extension);

                            if (!CheckFileExist(temp.ToString(), ref destTimeLastModified))
                            {
                                newFileName = temp.ToString();
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("ResetAvailableName error. \n error message:{0}", e));
                    //mLog.Warn("ResetAvailableName Error: " + e.ToString());
                }
                return newFileName;
            }
        }
        public string ResetAvailableName(DateTime timeLastModified)
        {
            return ResetAvailableName(timeLastModified, false);
        }
        private bool CheckFileExist(string fileName, ref DateTime lastModifiedTime)
        {
            var fileServerRelativeUrl = string.Format("{0}/{1}", this.ParentFolder.ServerRelativeUrl.TrimEnd('/'), fileName);
            IAveFile file = this.ParentWeb.SPWeb.GetCheckoutFile(fileServerRelativeUrl);
            if (file != null && file.Exists)
            {
                lastModifiedTime = (file.Item == null) ? file.TimeLastModified : ((DateTime)file.Item["Modified"]).ToUniversalTime();
                return true;
            }
            //if (this.mAveParentSite.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel && this.mParentFolder.RestoringItem.IsIncludingRecycleBinData)
            //{
            //    RestoringDto dto = new RestoringDto();
            //    dto.NameMapping = fileName;
            //    mQueryService.CheckConflictInfo(mDocumentInfo.SiteId, mDocumentInfo.ParentId, dto);
            //    if (dto.ConflictType == ConflictType.RecycleBin)
            //    {
            //        return true;
            //    }
            //}
            //[ADO-126223]注释此处用于解决skip+Append+考虑recycle bin的时候还原Document不删除回收站且File Name变化还原的现象。
            return false;

        }

        public bool NeedAppendNewVersion(DateTime timeLastModified)
        {
            bool needAppendNewVersion = false;
            try
            {
                IAveFile file = this.GetFile(mDocumentInfo.Name);
                if (file == null || !file.Exists)
                {
                    return false;
                }
                DateTime destTimeLastModified = (file.Item == null) ?
                    file.TimeLastModified : ((DateTime)file.Item["Modified"]).ToUniversalTime();
                if (!RestoreOption.mAveItemRestoreOption.SKIP_IF_SAME_MODIFIEDTIME || destTimeLastModified != timeLastModified)
                {
                    needAppendNewVersion = true;
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, string.Format("Set NeedAppendNewVersion Error.\n error message:{0}", e));
            }
            return needAppendNewVersion;
        }

        /// <summary>
        /// 检查文件名是否冲突，如果冲突重新命名
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="needIncluded">检查文件名是否从本身开始</param>
        /// <returns></returns>
        public string ResetAvailableName(string oldName, bool needIncluded)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.ResetAvailableName"))
            {

                string newFileName = string.Empty;
                IAveFile file = null;
                try
                {
                    if (needIncluded)
                    {
                        var serverRelativeUrl = string.Format("{0}/{1}", this.ParentFolder.ServerRelativeUrl.TrimEnd('/'), oldName);
                        // 还需要判断下是否是用其他user check out的file。
                        file = this.ParentWeb.SPWeb.GetCheckoutFile(serverRelativeUrl);
                        if (file == null || !file.Exists)
                        {
                            newFileName = oldName;
                            return newFileName;
                        }
                    }

                    string extension = string.Empty;
                    string prevName = oldName;
                    int pos = oldName.LastIndexOf('.');
                    if (pos > 0)
                    {
                        extension = oldName.Substring(pos, oldName.Length - pos);
                        prevName = oldName.Substring(0, pos);
                    }
                    for (int i = 1; i <= 1000; ++i)
                    {
                        StringBuilder temp = new StringBuilder(prevName);
                        temp.Append("_");
                        temp.Append(i.ToString());
                        temp.Append(extension);
                        try
                        {
                            file = this.ParentWeb.SPWeb.GetCheckoutFile(string.Format("{0}/{1}", this.ParentFolder.ServerRelativeUrl.TrimEnd('/'), temp.ToString()));
                            if (!file.Exists)
                            {
                                newFileName = file.Name;
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFileByNameError, e.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("ResetAvailableName error. \n error message:{0}", e));
                    //mLog.Warn("ResetAvailableName Error: " + e.ToString());
                }
                return newFileName;

            }

        }

        /// <summary>
        /// 在该方法中处理AveSPDoc需要单独处理的DocData相关设置，AveSPItem(AveSPDoc/AveSPItem/AveSPFolder)共有的DocData处理在AveSPItem对应的ProcessPreDocDataCondtion中进行设置
        /// </summary>
        /// <param name="allDocData"></param>
        internal override void ProcessPreDocDataCondition(Dictionary<string, object> allDocData)
        {
            base.ProcessPreDocDataCondition(allDocData);
            mDocumentInfo.CheckinComment = string.Empty;
            if (allDocData.ContainsKey("CheckinComment"))
            {
                mDocumentInfo.CheckinComment = (string)allDocData["CheckinComment"];
            }
            if(allDocData.ContainsKey("IsLinkFile"))
            {
                mDocumentInfo.IsLinkFile = (bool)allDocData["IsLinkFile"];
             
            }
            mDocumentInfo.OrignialID = allDocData.ContainsKey("Id") ? (Guid)allDocData["Id"] : Guid.Empty;
        }

        /// <summary>
        /// 在该方法中处理AveSPDoc需要单独处理的UserData相关设置，AveSPItem(AveSPDoc/AveSPItem/AveSPFolder)共有的UserData处理在AveSPItem对应的ProcessPreUserDataCondtion中进行设置
        /// </summary>
        /// <param name="allUserData"></param>
        [Obsolete("Use ProcessPreUserAndJunctionDataCondition() instead")]
        internal override void ProcessPreUserDataCondition(Dictionary<string, object> allUserData)
        {
            base.ProcessPreUserDataCondition(allUserData);
            if (allUserData.Keys.Contains("SolutionId"))
            {
                mDocumentInfo.SolutionId = new Guid(allUserData["SolutionId"].ToString());
                if (mAveSPList.SPList != null && mAveSPList.SPList.BaseTemplate == AveListTemplateType.SolutionCatalog)
                {
                    mDocumentInfo.ActivatedWebFeatureIDs = mAveSPList.ParentWeb.ActivatedWebFeatureIDs;
                }
            }
            //O365会将CheckinCommont当做是Column Value来备份，存放在allUserData中
            if (allUserData.ContainsKey("#tp_CheckinComment"))
            {
                mDocumentInfo.CheckinComment = (string)allUserData["#tp_CheckinComment"];
            }
            if (allUserData.ContainsKey("#tp_CopySource"))
            {
                string copysource = allUserData["#tp_CopySource"].ToString();
                mDocumentInfo.CopySource = AveReplaceProcessor.UrlReplace(copysource, this.mAveParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
            }
            if (allUserData.ContainsKey("#tp_HasCopyDestinations"))
            {
                bool hascopy = false;
                if (bool.TryParse(allUserData["#tp_HasCopyDestinations"].ToString(), out hascopy))
                {
                    mDocumentInfo.HasCopyDestinations = hascopy;
                }
            }
        }

        internal override void ProcessPreUserAndJunctionDataCondition(Dictionary<string, object> allUserData, List<Dictionary<string, object>> junctionData)
        {
            base.ProcessPreUserAndJunctionDataCondition(allUserData, junctionData);
            if (allUserData.Keys.Contains("SolutionId"))
            {
                mDocumentInfo.SolutionId = new Guid(allUserData["SolutionId"].ToString());
                if (mAveSPList.SPList != null && mAveSPList.SPList.BaseTemplate == AveListTemplateType.SolutionCatalog)
                {
                    mDocumentInfo.ActivatedWebFeatureIDs = mAveSPList.ParentWeb.ActivatedWebFeatureIDs;
                }
            }
            //O365会将CheckinCommont当做是Column Value来备份，存放在allUserData中
            if (allUserData.ContainsKey("#tp_CheckinComment"))
            {
                mDocumentInfo.CheckinComment = (string)allUserData["#tp_CheckinComment"];
            }
            if (allUserData.ContainsKey("#tp_CopySource"))
            {
                string copysource = allUserData["#tp_CopySource"].ToString();
                mDocumentInfo.CopySource = AveReplaceProcessor.UrlReplace(copysource, this.mAveParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
            }
            if (allUserData.ContainsKey("#tp_HasCopyDestinations"))
            {
                bool hascopy = false;
                if (bool.TryParse(allUserData["#tp_HasCopyDestinations"].ToString(), out hascopy))
                {
                    mDocumentInfo.HasCopyDestinations = hascopy;
                }
            }
        }

        /// <summary>
        /// 在该方法中处理AveSPDoc需要单独处理的Setting设置(和allDocData，allUserData无关)，AveSPItem(AveSPDoc/AveSPItem/AveSPFolder)共有的setting设置在AveSPItem对应的ProcessPreSettingCondition中进行设置
        /// </summary>
        internal override void ProcessPreSettingCondition()
        {
            base.ProcessPreSettingCondition();
            RestoreOption.mAveItemRestoreOption.DELETE_ITEM = mParentFolder.RestoringItem.Init(mDocumentInfo.Name, CheckRestoreOption(IsNewCreated, AveRestoreMode.OverWrite), RestoreOption.mAveItemRestoreOption.DELETE_ITEM);
            mDocumentInfo.SettingInfo.DELETE_ITEM = RestoreOption.mAveItemRestoreOption.DELETE_ITEM;
            mDocumentInfo.SettingInfo.IsProcessSolutionStatus = RestoreOption.mAveItemRestoreOption.IsProcessSolutionStatus;
            mDocumentInfo.SettingInfo.MIG_STUB_PIC_THUMBNAILS = mRestoreOption.mAveStorgeOption.MIG_STUB_PIC_THUMBNAILS;
            mDocumentInfo.SettingInfo.SKIP_IF_SAME_MODIFIEDTIME = mRestoreOption.mAveItemRestoreOption.SKIP_IF_SAME_MODIFIEDTIME;
            mDocumentInfo.SettingInfo.OverWriteByModifiedTime = CheckRestoreOption(AveRestoreMode.OverWriteByModifiedTime);
            mDocumentInfo.RestoringItem = mParentFolder.RestoringItem;
            mDocumentInfo.IsOrignialCheckOut = (mDocumentInfo.OriginalLevel == 255);
            if (mParentFolder.CurrentDocStatus == null)
            {
                mParentFolder.CurrentDocStatus = new CurrentRestoreDocStatus();
            }
            if (mDocumentInfo.Name != mParentFolder.CurrentDocStatus.Name)
            {
                mParentFolder.CurrentDocStatus.Name = mDocumentInfo.Name;
                mParentFolder.CurrentDocStatus.HasPreCurrentVersion = false;
                mDocumentInfo.HasPreCurrentVersion = false;
            }
            mParentFolder.CurrentDocStatus.Status = mDocumentInfo.ModerationStatus;
            mParentFolder.CurrentDocStatus.UIVersion = mDocumentInfo.OriginalVersion;
            if (this.Receiver != null)
            {
                mBaseItemInfo.DocumentSize = this.Receiver.ContentLength;
            }
            mBaseItemInfo.HasStream = this.HasStream;
        }

        /// <summary>
        /// 在该方法中处理AveSPDoc需要单独处理的MetaInfo(包括UnVersionedMetaInfo)，AveSPItem(AveSPDoc/AveSPItem/AveSPFolder)共有的MetaInfo设置在AveSPItem对应的ProcessPreMetaInfoCondtion中进行设置
        /// </summary>
        /// <param name="allDocData"></param>
        internal override void ProcessPreMetaInfoCondition(Dictionary<string, object> allDocData)
        {
            base.ProcessPreMetaInfoCondition(allDocData);
            if (allDocData.ContainsKey("MetaInfo"))
            {
                byte[] bts = (byte[])allDocData["MetaInfo"];
                string metaInfo = string.Empty;
                try
                {
                    metaInfo = AveCompressedUtility.GetTCompressedString(bts);
                    mDocumentInfo.MetaInfoDic = AveCompressedUtility.GetMetaInfoDictionary(metaInfo);

                    string sourceCTId;
                    IAveContentTypeId dstCTId;
                    if (this.ParentList != null && this.ParentList.SPList != null
                        && mDocumentInfo.MetaInfoDic.TryGetValue("ContentTypeId", out sourceCTId)
                        && this.ParentSite.MappingManager.ListMappingManager.ListLevelCTIdMapping.TryGetValue(sourceCTId, out dstCTId))
                    {
                        mDocumentInfo.MetaInfoDic["ContentTypeId"] = dstCTId.ToString();
                    }
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetMetaInfoFailed, ex);
                }
            }
            if (allDocData.ContainsKey("UnVersionedMetaInfo"))
            {
                byte[] bts = (byte[])allDocData["UnVersionedMetaInfo"];
                string unversionmetaInfo = string.Empty;
                try
                {
                    if (AveCompressedUtility.IsTCompressedBytes(bts))
                    {
                        unversionmetaInfo = AveCompressedUtility.GetTCompressedString(bts);
                        Dictionary<Dictionary<string, string>, string> unversionedmetaInfodic = AveCompressedUtility.GetMetaInfoDictionaryWithSeparator(unversionmetaInfo);
                        string meta = ReplaceUnVersionedMetaInfo(unversionedmetaInfodic);
                        bts = AveCompressedUtility.GetTCompressedBytes(meta);
                    }
                    else
                    {
                        unversionmetaInfo = Encoding.UTF8.GetString(bts);
                        Dictionary<Dictionary<string, string>, string> unversionedmetaInfodic = AveCompressedUtility.GetMetaInfoDictionaryWithSeparator(unversionmetaInfo);
                        string meta = ReplaceUnVersionedMetaInfo(unversionedmetaInfodic);
                        bts = Encoding.UTF8.GetBytes(meta);
                    }
                    mDocumentInfo.UnVersionedMetaInfo = bts;
                    mDocumentInfo.UnVersionedMetaInfoVersion = allDocData.ContainsKey("UnVersionedMetaInfoVersion") ? int.Parse(allDocData["UnVersionedMetaInfoVersion"].ToString()) : 0;
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetMetaInfoFailed, ex);
                }
            }
        }

        /// <summary>
        /// 在该方法中处理系统文件
        /// </summary>
        /// <returns>如果发生了Stream替换或者还原，就不需要继续Restore了</returns>
        private bool ProcessPreSystemFileCondition()
        {
            if (mParentFolder.ParentList.SPList != null && AveSPUtility.IsOrInSystemFormsFolder(mParentFolder.SPFolder))
            {
                if (Name.Equals("client_LocationBasedDefaults.html", StringComparison.OrdinalIgnoreCase))
                {
                    SPFile = GetFile();
                    if (SPFile != null)
                    {
                        if (SPFile.Exists)
                        {
                            this.MergeSouAndDesDefaultValueWithStream(
                                mParentFolder.ParentList.SPList, SPFile, mParentFolder.ParentList, CheckRestoreOption(IsNewCreated, AveRestoreMode.OverWrite));
                        }
                        else
                        {
                            IAveFolder folder = mParentFolder.SPFolder;
                            string path = folder.ServerRelativeUrl.TrimEnd('/') + "/" + Name;
                            folder.ParentWeb.GetFolder(folder.ServerRelativeUrl.TrimEnd('/')).Files.Add(path, AveTemplateFileType.FormPage);
                            SPFile = folder.ParentWeb.GetFile(path);
                            this.CreateSouAndDesDefaultValueWithStream(
                                mParentFolder.ParentList.SPList, SPFile, mParentFolder.ParentList, CheckRestoreOption(IsNewCreated, AveRestoreMode.OverWrite));
                        }
                    }
                    return false;
                }
                else if (Name.Equals("RetentionPolicy.Xml", StringComparison.OrdinalIgnoreCase))
                {
                    SPFile = GetFile();
                    if (SPFile != null && SPFile.Exists)
                    {
                        return this.OverWriteRetionStream(mParentFolder.ParentList.SPList,
                                                                        SPFile,
                                                                        mParentFolder.ParentList,
                                                                        CheckRestoreOption(IsNewCreated, AveRestoreMode.OverWrite));
                    }
                }
            }
            return true;
        }

        /// <returns>是否需要继续Restore</returns>
        internal bool ProcessPreCondition(Dictionary<string, object> allDocData, Dictionary<string, object> allUserData, List<Dictionary<string, object>> junctionData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.ProcessPreCondition"))
            {

                this.SetRestoreOption(mRestoreOption);
                RestoringDataValidation(allDocData, allUserData);
                ProcessPreDocDataCondition(allDocData);
                ProcessPreSettingCondition();
                ProcessViewInfo(allDocData);
                ProcessGhostInfo(allDocData);
                ProcessPreUserAndJunctionDataCondition(allUserData, junctionData);
                #region conflict resolution
                if (mRestoreOption.CheckRestoreOption(AveRestoreMode.AppendANewVersion) && !mDocumentInfo.IsView && !mDocumentInfo.IsGhostPage && !mDocumentInfo.Needskip)
                {
                    mDocumentInfo.RestoringItem.IsNewItem = true;
                    mDocumentInfo.IsVersion = false;
                }
                #endregion

                #region Hold and Declare

                mHTMetaInfo = new Hashtable();

                if ((allUserData.ContainsKey("_vti_ItemHoldRecordStatus")) && (!allUserData["_vti_ItemHoldRecordStatus"].ToString().Equals("0")) && (allDocData.ContainsKey("MetaInfo")))
                {
                    byte[] dateMetaInfo = (byte[])allDocData["MetaInfo"];
                    try
                    {
                        mFileHold = this.GetHoldRecord(mHTMetaInfo, dateMetaInfo, allUserData);
                        if (mFileHold != null)
                        {
                            mHoldItems = mAveParentSite.GetHoldItemID(mFileHold.HoldsProperty);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.WARN, "An error occurred while getting hold and declared record information. Error Message:{0} ", ex);
                    }
                }
                #endregion

                return ProcessPreSystemFileCondition();

            }

        }


        //CI-31912 不还原AllUserData表中没有数据的document以及document version
        private void RestoringDataValidation(Dictionary<string, object> allDocData, Dictionary<string, object> allUserData)
        {
            if (mParentFolder.ParentList.SPList != null && allDocData.ContainsKey("DoclibRowId") && (int)allDocData["DoclibRowId"] > 0)
            {
                if (allUserData == null || allUserData.Count == 0)
                {
                    log.Info("Skip restoring current document as it's user data is invalid.");
                    throw new AveWrapperSkipException(AveInternalResourceKey.Wrapper_Exception_Restore_SkipRestoreDocumentWithInvalidUserData);
                }
            }
        }
        internal void ProcessWebPartCondtion(IList<AveWebPartBaseInfo> webParts)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.ProcessWebPartCondition"))
            {
                if (webParts == null)
                {
                    return;
                }
                if (mAveParentSite.SPContextKind != AveContextKind.ClientObjectModel)
                {
                    this.mAveParentSite.MappingManager.SiteMappingManager.LoadWebPartIDMapping(mAveParentSite.SPSite);
                }
                AveSPWebPartManager manager = new AveSPWebPartManager(this);   
                foreach (AveWebPartBaseInfo webPartInfo in webParts)
                {
                    if (webPartInfo.UserID > 0)
                    {
                        this.ParentSite.SPMembers.FindMember(webPartInfo.UserID, true);
                    }
                }
                mDocumentInfo.WebPartCache = manager.GetWebPartCache();
                if (!mParentWeb.ParentSite.SPSite.IsOnlineSite && !mDocumentInfo.IsView &&
                     string.Compare(ParentSite.MappingManager.SiteMappingManager.SourceSiteInfo.SPVersion, ParentSite.MappingManager.SiteMappingManager.DestSiteInfo.SPVersion, StringComparison.Ordinal) > 0)
                {
                    bool isOnlineToLocal13 = WrapperConfiguration.RestoreWebPartFromOnlineToLocal && mParentWeb.ParentSite.SourceSiteInfo.IsOnline && mParentWeb.ParentSite.SPContextKind == AveContextKind.Server13ObjectModel;
                    if (!isOnlineToLocal13)
                    {
                        AveWebPartAssemblyFilter webpartFilter = new AveWebPartAssemblyFilter(this.TagUrl, mParentWeb.ParentSite.SPSite.SPVersion);
                        webParts = webpartFilter.FilterWebParts(webParts);
                        report.AddDetails(webpartFilter.FilteredWebParts);
                    }
                }
                mDocumentInfo.WebParts = webParts.ToList();
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "vti_copydestinations is a key")]
        private string ReplaceUnVersionedMetaInfo(Dictionary<Dictionary<string, string>, string> metainfodic)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.ReplaceUnVersionedMetaInfo"))
            {

                string metainfo = string.Empty;
                try
                {

                    XmlDocument xdoc = new XmlDocument();
                    xdoc.PreserveWhitespace = true;
                    foreach (KeyValuePair<Dictionary<string, string>, string> item in metainfodic)
                    {
                        if (item.Key.Count == 1)
                        {
                            foreach (KeyValuePair<string, string> inneritem in item.Key)
                            {
                                if (inneritem.Key.Equals("vti_copydestinations", StringComparison.OrdinalIgnoreCase))
                                {
                                    string value = inneritem.Value;
                                    xdoc.LoadXml(value);
                                    foreach (XmlNode node in xdoc.DocumentElement.ChildNodes)
                                    {
                                        XmlElement xe = (XmlElement)node;
                                        if (xe.HasAttribute("Url"))
                                        {
                                            string url = xe.GetAttribute("Url");
                                            url = AveReplaceProcessor.UrlReplace(url, this.mAveParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
                                            xe.SetAttribute("Url", url);
                                        }
                                        if (xe.HasAttribute("CreatedBy"))
                                        {
                                            int createdby = int.Parse(xe.GetAttribute("CreatedBy"));
                                            IAvePrincipal user = this.mAveParentSite.SPMembers.FindMember(createdby, true);
                                            if (user != null )
                                            {
                                                xe.SetAttribute("CreatedBy", user.ID.ToString());
                                            }
                                        }
                                        if (xe.HasAttribute("ModifiedBy"))
                                        {
                                            int modifiby = int.Parse(xe.GetAttribute("ModifiedBy"));
                                            IAvePrincipal user = this.mAveParentSite.SPMembers.FindMember(modifiby, true);
                                            if (user != null )
                                            {
                                                xe.SetAttribute("ModifiedBy", user.ID.ToString());
                                            }
                                        }

                                    }
                                    metainfo += inneritem.Key + item.Value + xdoc.OuterXml + "\r\n";
                                    xdoc.RemoveAll();
                                }
                                else
                                {
                                    metainfo += inneritem.Key + item.Value + inneritem.Value + "\r\n";
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.ReplaceMetaInfoFailed, e);
                }
                return metainfo.TrimEnd('\n').TrimEnd('\r');


            }

        }

        private IAveFile GetFile()
        {
            IAveFolder folder = mParentFolder.SPFolder;
            string folderPath = folder.ServerRelativeUrl.TrimEnd('/') + "/" + Name;
            return folder.ParentWeb.GetFile(folderPath);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint view name")]
        public void ProcessViewInfo(Dictionary<string, object> allDocData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.ProcessViewInfo"))
            {

                mDocumentInfo.Needskip = false;
                mDocumentInfo.IsOverWrite = CheckRestoreOption(AveRestoreMode.OverWrite);
                mDocumentInfo.IsView = allDocData.ContainsKey("IsViewPage") ? (bool)allDocData["IsViewPage"] : false;
                if (mDocumentInfo.IsView)
                {
                    int i = 0;
                    while (allDocData.ContainsKey("ViewID" + i))
                    {
                        try
                        {
                            AveViewInfo viewInfo = new AveViewInfo();
                            viewInfo.IsPersonal = (bool)allDocData["IsPersonal" + i];


                            viewInfo.ViewType = AveSPView.GetViewType(allDocData["ViewType" + i]);
                            viewInfo.Id = (Guid)allDocData["ViewID" + i];

                            //view title 走language mapping
                            string viewTitle = (string)allDocData["ViewTitle" + i];
                            //空 title,或者备份的是resource id（local备份），则不需要走mapping
                            if (string.IsNullOrEmpty((viewTitle)) || viewTitle.StartsWith("$Resources", StringComparison.Ordinal))
                            {
                                viewInfo.Title = viewTitle;
                            }
                            else
                            {
                                //365备份的是当前语言的title，需要根据目的端web语言走mapping
                                viewInfo.Title = ParentSite.GetNameByLanguageMapping(viewTitle, AveLanguageMappingType.ViewTitleMapping);

                            }

                            viewInfo.LeafName = (string)allDocData["LeafName"];
                            if (allDocData.ContainsKey("IsDefaultView" + i))//DocAve 5420没有备份IsDefaultView
                            {
                                viewInfo.IsDefaultView = (bool)allDocData["IsDefaultView" + i];
                            }
                            if (allDocData.ContainsKey("IsMobileView" + i))
                            {
                                viewInfo.IsMobileView = (bool)allDocData["IsMobileView" + i];
                            }
                            if (allDocData.ContainsKey("IsDefaultMobileView" + i))
                            {
                                viewInfo.IsDefaultMobileView = (bool)allDocData["IsDefaultMobileView" + i];
                            }
                            if(allDocData.ContainsKey("ListViewXml" + i))
                            {
                                viewInfo.ListViewXml = (string)allDocData["ListViewXml" + i];
                            }
                            if (allDocData.ContainsKey("MappingForSpotlight" + i))
                            {
                                viewInfo.MappingForSpotlight = (Dictionary<int, List<string>>)allDocData["MappingForSpotlight" + i];
                            }
                            viewInfo.UserID = -1;
                            if (viewInfo.IsPersonal && allDocData.ContainsKey("UserID" + i))
                            {
                                viewInfo.UserID = (int)allDocData["UserID" + i];
                                int userID = mAveParentSite.SPMembers.FindMemberId(viewInfo.UserID.Value, true, false);
                                if (userID <= 0)
                                {
                                    string msg = string.Format("Can not find user associated with personal view. ViewTitle: {0}. UserID: {1}", viewInfo.Title, viewInfo.UserID);
                                    log.Log(AveLogLevel.WARN, msg);
                                }
                                viewInfo.UserID = userID;
                            }
                            byte BaseViewId = 0;
                            if (allDocData.ContainsKey("BaseViewId" + i))
                            {
                                BaseViewId = (byte)allDocData["BaseViewId" + i];
                            }
                            if (allDocData.ContainsKey("Hidden" + i))
                            {
                                viewInfo.Hidden = (bool)allDocData["Hidden" + i];
                            }
                            if (viewInfo.LeafName == "WebFldr.aspx" && BaseViewId == 3)
                            {
                                mDocumentInfo.Needskip = true;
                                //return;
                            }
                            mDocumentInfo.AveView.Vinfos.Add(viewInfo);

                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, string.Format("An error occurred while process page view. \n error message:{0}", e));
                        }
                        i++;
                    }
                    if (CheckRestoreOption(AveRestoreMode.OverWrite))
                    {
                        mDocumentInfo.IsNewCreated = true;
                    }
                }

            }

            if (mDocumentInfo.IsView && mDocumentInfo.Needskip)
            {//Explorer View 不需要还原
                mDocumentInfo.RestoringItem.NeedSkipped = true;
                //mLog.Warn("source baseViewID is not in destination view baseViewIDs. view title:Explorer View");
                //return AveRestoreResult.Omit;
                throw new AveRestoreException(AveRestoreResult.SkipRestoreItemMetaData, AveRestoreResult.SkipRestoreItemMetaData.ToString());
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint view name")]
        public void ProcessGhostInfo(Dictionary<string, object> allDocData)
        {
            #region Check the document is ghost page or not
            mDocumentInfo.DocFlag = allDocData.ContainsKey("DocFlags") ? (int)allDocData["DocFlags"] : 0;
            mDocumentInfo.OriginalPageStatus = allDocData.ContainsKey("CustomizedPageStatus") ? (AveCustomizedPageStatus)allDocData["CustomizedPageStatus"] : AveCustomizedPageStatus.None;
            if (allDocData.ContainsKey("SetupPath"))
            {
                mDocumentInfo.SetupPath = (string)allDocData["SetupPath"];
            }
            if (!mDocumentInfo.HasStream && mDocumentInfo.SetupPath != null
                || (mDocumentInfo.HasStream && mDocumentInfo.SetupPath != null && (mAveParentSite.SaveBinaryForGhostPage == AveRestoreGhostPageOption.KeepPathOnly || mAveParentSite.SaveBinaryForGhostPage == AveRestoreGhostPageOption.KeepStreamAndPath)))
            {
                if (mParentFolder.ParentList.SPList != null)
                {
                    if (!AveDocFlags.IsMustBeUnGostedWhenUndirtiedDoc(mDocumentInfo.DocFlag) || (!AveDocFlags.IsUngostedDoc(mDocumentInfo.DocFlag)))
                    {
                        mDocumentInfo.IsGhostPage = true;
                    }
                    if (mParentFolder.ParentList.SPList.BaseTemplate == AveListTemplateType.WebPageLibrary
                        && mDocumentInfo.SetupPath.Equals(@"DocumentTemplates\wkpstd.aspx", StringComparison.OrdinalIgnoreCase))
                    {
                        mDocumentInfo.IsGhostPage = false;
                    }
                    if (mParentFolder.ParentList.SPList.BaseTemplate == AveListTemplateType.MasterPageCatalog
                        && mDocumentInfo.SetupPath.Equals(@"Features\PublishingResources\PageLayoutTemplate.aspx", StringComparison.OrdinalIgnoreCase))
                    {
                        mDocumentInfo.IsGhostPage = false;
                    }
                }
                else if (mParentFolder.ParentList.SPList == null)
                {
                    if (!AveDocFlags.IsMustBeUnGostedWhenUndirtiedDoc(mDocumentInfo.DocFlag) || (!AveDocFlags.IsUngostedDoc(mDocumentInfo.DocFlag)))
                    {
                        mDocumentInfo.IsGhostPage = true;
                    }
                }
            }
            if (mDocumentInfo.IsGhostPage)
            {
                //mDocumentInfo.FieldsInfo.Fields = mAveSPFolder.ParentList.AveFields.GetFieldValues(-1, mDocumentInfo.OriginalVersion, allUserData);                
                AveSPWeb parentWeb = mParentFolder.ParentList.ParentWeb;
                string name = mDocumentInfo.Name;
                string setupPath = mDocumentInfo.SetupPath;
                if (mAveParentSite.AveLanguageProcesser != null && (!mAveParentSite.AveLanguageProcesser.LanguageRexSame()))
                {
                    AveLanguageProcesser languageProcesser = mAveParentSite.AveLanguageProcesser;
                    this.ProcessGhostPageNameAndPath(parentWeb.WebSrcLanguageId, parentWeb.SPWeb.Language, ref name, ref setupPath);
                }
                else if (parentWeb.WebSrcLanguageId != parentWeb.SPWeb.Language && mParentFolder.ParentList.SPList != null
                        && mParentFolder.ParentList.SPList.BaseTemplate == AveListTemplateType.MasterPageCatalog)
                {
                    //make sure the the pagelayout in createpage.aspx in pages list is ok when language mapping is not setup
                    this.ProcessGhostPageNameAndPath(parentWeb.WebSrcLanguageId, parentWeb.SPWeb.Language, ref name, ref setupPath);
                }
                mDocumentInfo.Name = name;
                mDocumentInfo.SetupPath = setupPath;
                mDocumentInfo.GhostPageOption = (int)mAveParentSite.SaveBinaryForGhostPage;
            }
            #endregion
        }

        public AveRestoreResult RestoreSelf(Dictionary<string, object> allDocData, Dictionary<string, object> allUserData)
        {
            return RestoreSelf(allDocData, allUserData, null, null);
        }

        public AveRestoreResult RestoreSelf(Dictionary<string, object> allDocData, Dictionary<string, object> allUserData, List<Dictionary<string, object>> junctionData, List<AveWebPartBaseInfo> webParts)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.RestoreSelf"))
            {

                AveRestoreResult result = AveRestoreResult.Normal;
                try
                {
                    if (ProcessPreCondition(allDocData, allUserData, junctionData))
                    {
                        try
                        {
                            loaded = true;
                            ProcessWebPartCondtion(webParts);
                            ProcessVerifyItem();
                            mParentFolder.SPFolder.Files.DocumentSerializer.SetReport(report);
                            if (this.ParentList.firstTime && this.ParentList.containsTODAY)
                            {
                                this.ParentWeb.ReloadWeb();
                                this.ParentList.ReloadList();
                                this.ParentList.firstTime = false;
                            }
                            try
                            {
                                result = mParentFolder.SPFolder.Files.DocumentSerializer.SetObjectData(mDocumentInfo, this.Receiver, allDocData, allUserData, mHoldItems, mHTMetaInfo);
                            }
                            finally
                            {
                                //设置成Master Page的page，还原时删除不掉。底层处理删除不掉时，将原page进行rename，然后继续还原page，在当前List的PostAction中再将rename的page进行删除。
                                mParentFolder.ReloadFolder(false);//还原View时可能会导致Web Reload，导致外面的Folder对象无效。
                                if (mDocumentInfo.AveItem != null && mDocumentInfo.AveItem.File != null && mDocumentInfo.AveItem.File.Exists
                                    && mDocumentInfo.WebParts != null && mDocumentInfo.WebParts.Count > 0)
                                {
                                    string webAppUrl = AveUrlUtility.GetServerUrl(mParentWeb.ParentSite.SPSite.Url);
                                    string pageUrl = webAppUrl + mDocumentInfo.AveItem.File.ServerRelativeUrl;
                                    report.UpdateWebpartInfo(pageUrl, mDocumentInfo.AveItem.File.UniqueId, ParentSite.SPSite.SPMode == WrapperSPMode.Server ? ParentSite.MappingManager.SiteMappingManager : null);
                                }
                            }
                            if (mDocumentInfo.TempMasterSettings != null && !string.IsNullOrEmpty(mDocumentInfo.TempFileUrl))
                            {
                                mParentFolder.ParentList.TempMasterSettings[mDocumentInfo.TempFileUrl] = mDocumentInfo.TempMasterSettings;
                            }
                        }
                        catch (AveRestoreException ex)
                        {
                            result = ex.Result;
                        }
                        finally
                        {
                            //ADO-125610,当file还原失败抛出异常时，不应该影响list的setting
                            if (mDocumentInfo.SettingInfo.LIST_SETTING_CHANGED)
                            {
                                ParentFolder.ParentList.SetListSettingFlags(AveListSettingFlags.LIST_SETTING_CHANGED);
                            }
                            if (ParentList.containsTODAY)
                            {
                                this.ParentWeb.ReloadWeb();
                                this.ParentList.ReloadList();
                            }
                        }
                        ProcessCabinet();
                        ProcessPostCondition(result, allDocData, allUserData);                        
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("You don't have permission to restore this item. {0}" + mDocumentInfo.Name, ex);
                    result = AveRestoreResult.Failed;
                    throw;
                }
                catch (AveRestoreException ex)
                {
                    result = ex.Result;
                }
                return result;

            }

        }

        private void ProcessCabinet()
        {
            if (this.Name.EndsWith(".xsn", StringComparison.OrdinalIgnoreCase) && this.mDocumentInfo.IsVersion == false && this.SPFile != null)
            {
                //this.ParentSite.MappingManager.SiteMappingManager.UnReplaceGuidAndUrlInfoPathCache.Add(this.mDocumentInfo.GUID.ToString() + "," + this.mDocumentInfo.WebId.ToString() +"," + this.mDocumentInfo.Version.ToString());                
                this.ParentSite.MappingManager.SiteMappingManager.UnReplaceGuidAndUrlInfoPathCache.Add(this.SPFile.ServerRelativeUrl + "," + this.ParentWeb.SPWeb.ID);
            }
        }

        internal override void ProcessVerifyItem()
        {
            if (mRestoreOption.mAveItemRestoreOption.VerifyPageLayout)
            {
                if (mDocumentInfo.FieldsInfo.Fields.ContainsKey("PublishingPageLayout"))
                {
                    bool hasFound = false;
                    string url = string.Empty;
                    try
                    {
                        url = (mDocumentInfo.FieldsInfo.Fields["PublishingPageLayout"] as AveFieldValueInfo).ColValue.ToString();
                        //只验证绝对url 是不是目的站点的。
                        url = AveReplaceProcessorV2.UrlDecode(url);
                        if (AveReplaceProcessorV2.IsAbsoluteUrl(url) && !(url.StartsWith(SPWeb.Site.Url,StringComparison.OrdinalIgnoreCase) && url[SPWeb.Site.Url.Length] == '/'))
                        {
                            hasFound = true;
                        }
                        //已经在 ProcessPreCondition(allDocData, allUserData, junctionData) 做过 URL 替换
                        IAveFile layoutFile = SPWeb.Site.RootWeb.GetFile(url);
                        if (layoutFile != null && layoutFile.Exists)
                        {
                            hasFound = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.WARN, "Can not get the page layout. Error:{0}", ex.ToString());
                    }
                    if (!hasFound)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("Can not find the page layout that the file uses. File name:{0}. PageLayout Url:{1}", mDocumentInfo.Name, url));
                        throw new AveVerifyPageLayoutNotFoundException(AveInternalResourceKey.Wrapper_Exception_Restore_VerifyFilePageLayoutFailed);
                    }
                }
            }
            if (NeedSkipDocument())
            {
                throw new AveWrapperSkipException(AveInternalResourceKey.Wrapper_Exception_Restore_Office365Environmental);
            }
            if(NeedSkipNintexFormFile(this.mDocumentInfo.ParentFolderRelativeUrl, this.mDocumentInfo.Name))
            {
                throw new AveRestoreException(AveRestoreResult.Omit, "Omit");
            }
            base.ProcessVerifyItem();
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharedWithMe,groupdocviewpage,mydocviewpage is a page name")]
        private bool NeedSkipDocument()
        {
            bool needSkip = false;
            try
            {
                if (!mParentWeb.ParentSite.SPSite.IsOnlineSite &&
                    mParentWeb.ParentSite.SourceSiteInfo != null && mParentWeb.ParentSite.SourceSiteInfo.SPVersion != null)
                {
                    if ((!string.IsNullOrEmpty(this.mDocumentInfo.SetupPath) && (this.mDocumentInfo.SetupPath.Equals(@"features\mysitedocumentlibrary\groupdocviewpage.aspx", StringComparison.OrdinalIgnoreCase) || this.mDocumentInfo.SetupPath.Equals(@"features\mysitedocumentlibrary\mydocviewpage.aspx", StringComparison.OrdinalIgnoreCase)))
                                    && (!string.IsNullOrEmpty(this.mDocumentInfo.Name) && (this.mDocumentInfo.Name.Equals("SharedWithMe.aspx", StringComparison.OrdinalIgnoreCase) || this.mDocumentInfo.Name.Equals("SharedWithGroup.aspx", StringComparison.OrdinalIgnoreCase) || this.mDocumentInfo.Name.Equals("All.aspx", StringComparison.OrdinalIgnoreCase))))
                    {
                        return true;
                    }
                    if (this.mDocumentInfo.IsView) //After ProcessViewInfo();
                    {
                        return needSkip;
                    }
                    Version sourceVersion = new Version(mParentWeb.ParentSite.SourceSiteInfo.SPVersion);
                    Version targetVersion = new Version(mParentWeb.ParentSite.SPSite.SPVersion);
                    if (sourceVersion.Major < 16 || sourceVersion.Major <= targetVersion.Major)
                    {//此问题是解决local到local，源端version比目的端高的时候，一些文件被skip了的情况，以后sharepoint出新版本时候需要注意这个地方
                        return needSkip;
                    }
                    //Online-Local, 16-15, skip restoring pages.
                    bool isOnlineToLocal13 = WrapperConfiguration.RestoreWebPartFromOnlineToLocal&& mParentWeb.ParentSite.SourceSiteInfo.IsOnline && mParentWeb.ParentSite.SPContextKind == AveContextKind.Server13ObjectModel;
                    var isASPX = mDocumentInfo.Name.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase);
                    needSkip = isOnlineToLocal13 ? needSkip : isASPX;
                    if (mParentFolder.ParentList.SPList != null)
                    {
                        //这里就是为了过滤掉有webpart 的view
                        switch (mParentFolder.ParentList.SPList.BaseTemplate)
                        {
                            case AveListTemplateType.MasterPageCatalog:
                                needSkip = true;
                                break;
                            case AveListTemplateType.GenericList:
                                needSkip = mParentFolder.ServerRelativeUrl.StartsWith(mParentFolder.ParentList.SPList.RootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase)
                                    && isASPX && !isOnlineToLocal13;
                                break;
                            case AveListTemplateType.DocumentLibrary:
                                needSkip = mParentFolder.ServerRelativeUrl.StartsWith(mParentFolder.ParentList.SPList.RootFolder.ServerRelativeUrl + "/Forms", StringComparison.OrdinalIgnoreCase)
                                    && isASPX && !isOnlineToLocal13;
                                break;
                            default:
                                break;
                        }
                    }
                }
                else if (mDocumentInfo.IsView)
                {
                    return needSkip;
                }
                else if (IsNintexFormRelatedFiles())
                {
                    return needSkip;
                }
                return needSkip;
            }
            catch (Exception ex)
            {
                log.Debug(string.Format("Can not recognize the document. Message:{0}", ex.ToString()));
                return false;
            }
        }

        // ADO-194300：过滤list和library下的NFForm.xml文件，这个文件会在还原nintex form publish的时候生成出来，如果还原这个文件的话会覆盖一些我们替换的属性。
        private bool NeedSkipNintexFormFile(string parentFolderRelativeUrl, string leafName)
        {
            if (!string.IsNullOrEmpty(parentFolderRelativeUrl))
            {
                if (Regex.IsMatch(parentFolderRelativeUrl, "/Lists/.*/item", RegexOptions.IgnoreCase)
                    || Regex.IsMatch(parentFolderRelativeUrl, "/Forms/Document", RegexOptions.IgnoreCase))
                {
                    if (string.Equals(leafName, "NFForm.xml", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsNintexFormRelatedFiles()
        {
            bool isNintexFormRelatedFiles = false;
            if (mParentWeb.ParentSite.SPSite.IsOnlineSite && mParentFolder.ParentList.Name.Equals("NintexFormXml", StringComparison.OrdinalIgnoreCase))
            {
                isNintexFormRelatedFiles = true;
            }
            if (this.mDocumentInfo.Name.Equals("NFForm.xml", StringComparison.OrdinalIgnoreCase))
            {
                foreach (IAveContentType contentType in mParentFolder.ParentList.SPList.ContentTypes)
                {
                    if (mParentFolder.ParentList.SPList.BaseType == AveBaseType.DocumentLibrary
                        && mParentFolder.ServerRelativeUrl.TrimStart('/').Equals(string.Format("{0}/{1}/{2}", mParentFolder.ParentList.RootFolderPath, "Forms", contentType.Name), StringComparison.OrdinalIgnoreCase))
                    {
                        isNintexFormRelatedFiles = true;
                        break;
                    }
                    else if (mParentFolder.ParentList.SPList.BaseType == AveBaseType.GenericList
                            && mParentFolder.ServerRelativeUrl.TrimStart('/').Equals(string.Format("{0}/{1}", mParentFolder.ParentList.RootFolderPath, contentType.Name), StringComparison.OrdinalIgnoreCase))
                    {
                        isNintexFormRelatedFiles = true;
                        break;
                    }
                }
            }

            return isNintexFormRelatedFiles;
        }

        internal void ProcessPostCondition(AveRestoreResult result, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData)
        {
            if (mDocumentInfo.IsView)
            {
                var siteMappingManager = this.ParentSite.MappingManager.SiteMappingManager;
                var listMappingManager = this.ParentSite.MappingManager.ListMappingManager;

                mParentFolder.ParentList.RestoreRssView = mDocumentInfo.AveView.RestoreRssView;

                foreach (KeyValuePair<Guid, Guid> pair in mDocumentInfo.AveView.Views)
                {
                    if (!listMappingManager.ListViewMapping.ContainsKey(pair.Key))
                    {
                        listMappingManager.ListViewMapping.Add(pair.Key, pair.Value);
                    }
                    if (mParentFolder.ParentList.SPList != null)
                    {
                        siteMappingManager.AddListViewMapping(mParentFolder.ParentList.SPList.ID, pair.Key, pair.Value);
                    }
                }

                if (result > 0)
                {
                    foreach (var info in mDocumentInfo.AveView.Vinfos)
                    {
                        if (info.MappingForSpotlight != null
                            && info.MappingForSpotlight.Count > 0
                            && listMappingManager.ListViewMapping.ContainsKey(info.Id))
                        {
                            this.ParentList.AddNeedUpdateSpotlightViews(listMappingManager.ListViewMapping[info.Id], info);
                        }
                    }
                }
                return;
            }

            if (allUserData.ContainsKey("_vti_ItemHoldRecordStatus") && !allUserData["_vti_ItemHoldRecordStatus"].ToString().Equals("0"))
            {
                if (SPFile != null && SPFile.Web != null && mFileHold != null)
                {
                    mAveParentSite.AddUnRestoreFileHoldRecordInfo(SPFile.Web.ID, SPFile.ServerRelativeUrl, mFileHold);
                }
            }

            if (result > 0 && mDocumentInfo.OriginalRowId > 0)
            {
                mParentFolder.ParentList.AveFields.ResetNotUpdateLookupFieldValue(mDocumentInfo.RowId);
                mParentFolder.ParentList.AveFields.ResetNintexFormDataFieldValue(mDocumentInfo.RowId);
                mParentFolder.ParentList.AveFields.ResetNotUpdateUrlFieldValue(mDocumentInfo.RowId);
                this.ResetRelatedItemsFieldValue(mDocumentInfo.RowId);
                base.ProcessPostJunctionDataCondition(mBaseItemInfo.FieldsInfo.NeedPostRestoreMultiLookupFields);
                this.AddItemMapping(mDocumentInfo.OriginalRowId);

                if (!mDocumentInfo.IsVersion && mDocumentInfo.OriginalLevel == 1 && mDocumentInfo.ModerationStatus == 0)
                {
                    mParentFolder.CurrentDocStatus.HasPreCurrentVersion = true;
                    mDocumentInfo.HasPreCurrentVersion = true;
                }
                if (this.SPListItem != null && this.ParentFolder.ParentList.SPList != null && (int)this.ParentFolder.ParentList.SPList.BaseTemplate == 850)
                {
                    if (mDocumentInfo.OrignialID != Guid.Empty)
                    {
                        this.ParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.WebMappingManager.AddPageItemSDGuidMapping(mDocumentInfo.OrignialID, this.SPListItem.UniqueId);
                    }
                    if (mDocumentInfo.OldDocId != Guid.Empty)
                    {
                        this.ParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.WebMappingManager.AddPageItemONGuidMapping(mDocumentInfo.OldDocId, this.SPListItem.UniqueId);
                    }
                }
            }
            #region schedule item
            try
            {
                if (this.ParentList.IsSchedulingOnList)
                {
                    if (SPFile != null && SPFile.Item != null && SPFile.Item.Fields[AveFieldId.StartDate] != null && SPFile.Item.Fields[AveFieldId.ExpiryDate] != null && mDocumentInfo.IsCurrentVersion)
                    {
                        if (!mDocumentInfo.IsCheckOut)
                        {
                            this.ParentSite.MappingManager.SiteMappingManager.AddScheduleItemCacheMapping(this.ParentWeb.SPWeb.ID, SPFile.UniqueId);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn("An exception occurred while add schedule item:{0}, exception :{1}", this.ScopeUrl, e.ToString());
            }
            #endregion

            #region Durable Link
            if (result > 0 && (this.ParentSite.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel
                || this.ParentSite.ObjectModelFactory.ContextKind >= AveContextKind.Server16ObjectModel)
                && mDocumentInfo.OrignialID != Guid.Empty && SPFile != null && !string.IsNullOrEmpty(SPFile.LinkingUrl))
            {
                this.ParentSite.MappingManager.SiteMappingManager.AddDurableLinkMapping(mDocumentInfo.OrignialID, SPFile.LinkingUrl);
            }
            #endregion
        }

        public bool CheckIfOnlyDoDiscard()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.CheckIfOnlyDoDiscard"))
            {

                if (RestoreOption.mAveItemRestoreOption.DISCARD_ITEM_ONLY)
                {
                    try
                    {
                        IAveFile spFile = this.GetFile(mDocumentInfo.Name);
                        if (spFile.CheckOutType != AveCheckOutType.None)
                        {
                            spFile.UndoCheckOut();
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while the SPFile UndoCheckOut.file name:{0}\n error message:{1}", mDocumentInfo.Name, ex));
                    }
                    return true;
                }
                return false;


            }

        }

        /// <param name="clearAllBeforeRestore">当我们在进行List Pose Action来还原没有还原的ListWebPart时，不应该删除已经存在的Web Part</param>
        public void RestoreWebPart(IList webPartList, bool clearAllBeforeRestore)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.WebPart"))
            {
                IAveList temList = null;
                bool status = false;
                bool enableVersioning = true;
                bool enableModeration = true;
                bool enableMinorVersions = true;
                bool forceCheckOut = true;
                AveDraftVisibilityType draftVersionVisibility = AveDraftVisibilityType.Approver;
                AveSPWebPartManager manager = null;
                try
                {
                    if (mAveParentSite.SPContextKind != AveContextKind.ClientObjectModel)
                    {
                        this.mAveParentSite.MappingManager.SiteMappingManager.LoadWebPartIDMapping(mAveParentSite.SPSite);
                    }
                    manager = new AveSPWebPartManager(this);
                    //关闭spFile 所在list的approve和version，避免在还原webpart时，可能多出一个version。
                    if (mAveParentSite.SPContextKind != AveContextKind.ClientObjectModel)
                    {
                        if (SPFile != null)
                        {
                            try
                            {
                                if (this.SPFile.Item != null)
                                {
                                    if (this.ParentFolder != null && this.ParentFolder.ParentList != null && this.ParentFolder.ParentList.SPList != null)
                                    {
                                        temList = this.ParentFolder.ParentList.SPList;
                                    }
                                    else
                                    {
                                        temList = SPFile.Item.ParentList;
                                    }
                                }
                                if (temList != null)
                                {
                                    enableMinorVersions = temList.EnableMinorVersions;
                                    enableVersioning = temList.EnableVersioning;
                                    enableModeration = temList.EnableModeration;
                                    draftVersionVisibility = temList.DraftVersionVisibility;
                                    forceCheckOut = temList.ForceCheckout;
                                    if (enableVersioning || enableModeration || forceCheckOut || (draftVersionVisibility != AveDraftVisibilityType.Reader))
                                    {
                                        if (temList.EnableVersioning && temList.BaseTemplate != AveListTemplateType.Survey)
                                        {
                                            temList.EnableVersioning = false;
                                            status = true;
                                        }
                                        if (temList.EnableModeration && !temList.HasExternalDataSource && temList.BaseTemplate != AveListTemplateType.Survey)
                                        {
                                            temList.EnableModeration = false;
                                            status = true;
                                        }
                                        if (!temList.HasExternalDataSource && temList.ForceCheckout)
                                        {
                                            temList.ForceCheckout = false;
                                            status = true;
                                        }
                                        if (temList.DraftVersionVisibility != AveDraftVisibilityType.Reader)
                                        {
                                            temList.DraftVersionVisibility = AveDraftVisibilityType.Reader;
                                            status = true;
                                        }
                                        if (status)
                                        {
                                            temList.Update();
                                        }
                                    }
                                }
                            }
                            catch (AveSecurityTrimingException)
                            {
                                throw;
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFileByNameError, e.ToString());
                                //有些spfile取不到list，在此会抛出异常，跳过即可
                            }
                        }
                    }
                    manager.Restore(webPartList, RestoreOption, clearAllBeforeRestore);
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.RealRestoreWebPartFailed, ex.ToString());
                    report.AddDetail(new AveWrapperWebpartReportDto("WebPart", "WebPart", null, string.Empty, string.Empty, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionToRestoreWebPart , ex.Message));
                }
                finally
                {
                    if (SPFile != null && SPFile.Exists && webPartList != null && webPartList.Count > 0)
                    {
                        string webAppUrl = AveUrlUtility.GetServerUrl(mAveParentSite.SPSite.Url);
                        string pageUrl = webAppUrl + SPFile.ServerRelativeUrl;
                        report.UpdateWebpartInfo(pageUrl, SPFile.UniqueId, ParentSite.SPSite.SPMode == WrapperSPMode.Server ? ParentSite.MappingManager.SiteMappingManager : null);
                    }
                    if (manager != null && manager.NeedReloadList)
                    {
                        if (mAveSPList != null && mAveSPList.AveList != null)
                        {
                            mAveSPList.AveList.Reload();
                        }
                        if (mParentFolder != null && mParentFolder.SPFolder != null)
                        {
                            mParentFolder.SPFolder.Reload(false);
                        }
                    }
                    if (status && temList != null)
                    {
                        try
                        {
                            if (temList.EnableVersioning != enableVersioning)
                            {
                                temList.EnableVersioning = enableVersioning;
                            }
                            if (temList.EnableModeration != enableModeration)
                            {
                                temList.EnableModeration = enableModeration;
                            }
                            if (temList.EnableMinorVersions != enableMinorVersions)
                            {
                                temList.EnableMinorVersions = enableMinorVersions;
                            }
                            if (temList.ForceCheckout != forceCheckOut)
                            {
                                temList.ForceCheckout = forceCheckOut;
                            }
                            if (temList.DraftVersionVisibility != draftVersionVisibility)
                            {
                                temList.DraftVersionVisibility = draftVersionVisibility;
                            }
                            temList.Update();
                        }
                        catch (AveSecurityTrimingException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.UpdateListFailed, e.ToString());
                            //存在一些hidden list，不能update setting。
                        }
                    }
                    //else
                    //{
                    //    AveSPWebPart webpartRestore = new AveSPWebPart(this, null);
                    //    webpartRestore.RestoreWebPartV2(webPartList as List<AveWebPartBaseInfo>);
                    //}
                    if (manager != null)
                    {
                        manager.Dispose();
                        manager = null;
                    }
                }

            }


        }

        public void RestoreAlert(Dictionary<string, object> data, bool isSchedAlert)
        {
            AveSPDocAlert mDocAlert = new AveSPDocAlert(this);
            mDocAlert.RestoreAlert(data, isSchedAlert);
        }

        public void RestoreSPComments(List<AveCommentInfo> comments,bool overwrite)
        {
            if (this.Item != null)
            {
                try
                {
                    var storage = mAveParentSite.ObjectModelFactory.CreateSPCommentStorage(this.ParentSite.SPSite);
                    if (storage != null)
                    {
                        var destinationComments = storage.GetComments(this.Item);
                        if (destinationComments.Count > 0)
                        {
                            if (overwrite)
                            {
                                storage.DeleteComments(this.Item);
                            }
                            else
                            {
                                return;
                            }
                        }
                        foreach (var comment in comments)
                        {
                            try
                            {
                                comment.OwnerInfo = mAveParentSite.SPMembers.GetMappingUserLogin(comment.OwnerInfo);
                                var resultComment = storage.AddComment(this.Item, comment);
                                if (comment.ReplyCount > 0)
                                {
                                    foreach (var reply in comment.Replies)
                                    {
                                        try
                                        {
                                            reply.OwnerInfo = mAveParentSite.SPMembers.GetMappingUserLogin(reply.OwnerInfo);
                                            reply.Parent = resultComment;
                                            storage.AddComment(this.Item, reply);
                                        }
                                        catch (Exception e)
                                        {
                                            log.Warn("An error occurred while restoreing SPComment reply. text:{0},Error:{1}", reply.Text, e);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Warn("An error occurred while restoreing one SPComment . text:{0},Error:{1}", comment.Text, ex);
                            }
                        }
                    }
                }
                catch (Exception ex2)
                {
                    log.Warn("An error occurred while restoreing all SPComments .Error:{0}", ex2);
                }
            }
           
        }

        public void CreateSouAndDesDefaultValueWithStream(IAveList list, IAveFile spFile, AveSPList aveSPList, bool overWrite)
        {
            if (HasStream)
            {
                Stream stream;
                XmlElement element;
                XmlDocument xSou = new XmlDocument();
                xSou.PreserveWhitespace = true;
                string linkUrl;
                string startStr = AveHttpUtility.UrlPathEncode(list.RootFolder.ServerRelativeUrl, true, false);
                string listRelativeUrl = AveHttpUtility.UrlPathEncode(aveSPList.ListInfo.ServerRelativeUrl, true, false);
                try
                {
                    stream = new AveSPFileStream(mReceiver);
                    xSou.Load(stream);
                    foreach (XmlNode node in xSou.DocumentElement.SelectNodes("a"))
                    {
                        element = (XmlElement)node;
                        linkUrl = element.GetAttribute("href");
                        if (!linkUrl.StartsWith(startStr, StringComparison.OrdinalIgnoreCase))
                        {
                            if (linkUrl.StartsWith(listRelativeUrl, StringComparison.OrdinalIgnoreCase))
                            {
                                element.SetAttribute("href", startStr + linkUrl.Substring(listRelativeUrl.Length));
                            }
                        }
                    }
                    spFile.SaveBinary(Encoding.UTF8.GetBytes(xSou.OuterXml));
                    spFile.Update();
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.MergeFileError, list.RootFolder.ServerRelativeUrl + "/Forms/client_LocationBasedDefaults.html", e.ToString());
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "MergeSouAndDesDefaultValueWithStream is method name. ")]
        public void MergeSouAndDesDefaultValueWithStream(IAveList list, IAveFile spFile, AveSPList aveSPList, bool overWrite)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPItem.MergeSouAndDesDefaultValueWithStream"))
            {

                if (HasStream)
                {
                    XmlDocument xDes = new XmlDocument();
                    XmlDocument xSou = new XmlDocument();
                    xDes.PreserveWhitespace = true;
                    xSou.PreserveWhitespace = true;
                    XmlElement newChild = null;
                    XmlElement temp = null;
                    Stream stream = null;
                    bool existNode = false;
                    bool valueConflict = false;
                    string startStr = list.RootFolder.ServerRelativeUrl;
                    string oldLinkUrl = String.Empty;
                    string oldListRelativeUrl = aveSPList.ListInfo.ServerRelativeUrl;

                    oldListRelativeUrl = AveHttpUtility.UrlPathEncode(oldListRelativeUrl, true, false);
                    startStr = AveHttpUtility.UrlPathEncode(startStr, true, false);

                    Dictionary<string, AveXmlField> xmlFields = aveSPList.AveFields.XmlFields;
                    List<XmlNode> needAddList = new List<XmlNode>();
                    try
                    {
                        stream = new AveSPFileStream(mReceiver);
                        xSou.Load(stream);
                        xDes.LoadXml(new UTF8Encoding().GetString(spFile.OpenBinary()));
                        foreach (XmlNode node in xSou.DocumentElement.SelectNodes("a"))
                        {
                            temp = (XmlElement)node;
                            oldLinkUrl = temp.GetAttribute("href");
                            if (!oldLinkUrl.StartsWith(startStr, StringComparison.OrdinalIgnoreCase))
                            {
                                if (oldLinkUrl.StartsWith(oldListRelativeUrl, StringComparison.OrdinalIgnoreCase))
                                {
                                    temp.SetAttribute("href", startStr + oldLinkUrl.Substring(oldListRelativeUrl.Length));
                                }
                            }
                        }
                        foreach (XmlNode souNode in xSou.DocumentElement.SelectNodes("a"))
                        {
                            existNode = false;
                            XmlElement tempSou = (XmlElement)souNode;
                            foreach (XmlNode desNode in xDes.DocumentElement.SelectNodes("a"))
                            {
                                XmlElement tempDes = (XmlElement)desNode;
                                if (tempSou.GetAttribute("href").Equals(tempDes.GetAttribute("href"), StringComparison.OrdinalIgnoreCase))
                                {
                                    foreach (XmlNode addNode in tempSou.SelectNodes("DefaultValue"))
                                    {
                                        valueConflict = false;
                                        XmlElement tempAdd = (XmlElement)addNode;

                                        if (xmlFields.ContainsKey(tempAdd.GetAttribute("FieldName")))
                                        {
                                            xmlFields.Remove(tempAdd.GetAttribute("FieldName"));
                                        }

                                        string mappingValue = aveSPList.AveFields.FieldMapping.GetMappingRestoredFieldInternalName(tempAdd.GetAttribute("FieldName"));
                                        string fieldInternalName = string.IsNullOrEmpty(mappingValue) ?
                                            tempAdd.GetAttribute("FieldName") : mappingValue;

                                        foreach (XmlNode node in tempDes.SelectNodes("DefaultValue"))
                                        {
                                            temp = (XmlElement)node;                                       
                                            if (temp.GetAttribute("FieldName").Equals(fieldInternalName))
                                            {
                                                if (overWrite)
                                                {
                                                    temp.InnerText = tempAdd.InnerText;
                                                }
                                                valueConflict = true;
                                                break;
                                            }
                                        }
                                        if (!valueConflict)
                                        {
                                            newChild = tempDes.FirstChild.OwnerDocument.CreateElement("DefaultValue");
                                            newChild.SetAttribute("FieldName", fieldInternalName);
                                            newChild.InnerText = tempAdd.InnerText;
                                            tempDes.AppendChild((XmlNode)newChild);
                                        }
                                    }

                                    if (overWrite)
                                    {
                                        foreach (XmlNode node in tempDes.SelectNodes("DefaultValue"))
                                        {
                                            temp = (XmlElement)node;
                                            if (xmlFields.ContainsKey(temp.GetAttribute("FieldName")))
                                            {
                                                desNode.RemoveChild(node);
                                            }
                                        }
                                    }

                                    existNode = true;
                                    break;
                                }
                            }
                            if (!existNode)
                            {
                                needAddList.Add(souNode);
                            }
                        }
                        foreach (XmlNode newNode in needAddList)
                        {
                            try
                            {
                                temp = (XmlElement)newNode;
                                newChild = xDes.CreateElement("a");
                                newChild.SetAttribute("href", temp.GetAttribute("href"));
                                newChild.InnerXml = temp.InnerXml;
                                xDes.DocumentElement.AppendChild((XmlNode)newChild);
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.MergeFileValuesFailed, e);
                            }
                        }
                        spFile.SaveBinary(Encoding.UTF8.GetBytes(xDes.OuterXml));
                        spFile.Update();
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.MergeFileError, list.RootFolder.ServerRelativeUrl, e.ToString());
                    }
                }

            }

        }

        /// <summary>
        /// 更新原端的list Retention xml stream流用于目的端还原
        /// </summary>
        /// <returns>如果发生了Stream替换，就不需要继续Restore了</returns>
        public bool OverWriteRetionStream(IAveList list, IAveFile spFile, AveSPList aveSPList, bool overWrite)
        {
            try
            {
                if (overWrite)
                {
                    Stream stream = null;
                    stream = new AveSPFileStream(mReceiver);
                    XmlDocument retentionXml = new XmlDocument();
                    retentionXml.Load(stream);
                    XmlElement retentionEle = (XmlElement)retentionXml.DocumentElement.FirstChild;
                    retentionEle.SetAttribute("href", list.RootFolder.ServerRelativeUrl);
                    spFile.SaveBinary(Encoding.UTF8.GetBytes(retentionXml.OuterXml));
                    spFile.Update();
                    return false;
                }

            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, WrapperRestoreResource.OverWriteRetionStreamFailed, e);
            }
            return true;
        }

        /// <summary>
        /// added for renaming name and setuppath for ghosted page if user choosen language mapping
        /// </summary>
        internal void ProcessGhostPageNameAndPath(uint sLanguageId, uint dLanguageId, ref string name, ref string setupPath)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPItem.ProcessGhostPageNameAndPath"))
            {

                try
                {
                    string sLanguageString = sLanguageId.ToString();
                    string dLanguageString = dLanguageId.ToString();
                    if (setupPath.StartsWith(sLanguageString, StringComparison.OrdinalIgnoreCase))
                    {
                        setupPath = dLanguageString + setupPath.Substring(sLanguageString.Length);
                    }
                    string sourceShortName = AveLanguageProcesser.CultureIdNameMapping[sLanguageId];
                    string destShortName = AveLanguageProcesser.CultureIdNameMapping[dLanguageId];

                    if (setupPath.ToLower(CultureInfo.InvariantCulture).Contains(sourceShortName.ToLower(CultureInfo.InvariantCulture)))
                    {
                        setupPath = setupPath.ToLower(CultureInfo.InvariantCulture).Replace(sourceShortName.ToLower(CultureInfo.InvariantCulture), destShortName);
                    }
                    if (setupPath.Contains(sLanguageString))
                    {
                        setupPath = setupPath.Replace(sLanguageString, dLanguageString);
                    }
                }
                catch (Exception e)
                {
                    log.Warn("An Error occurred while setting ghost page name and setupPath,GhostPage name:{0},SetupPath:{1},Exception:{2}", name, setupPath, e.ToString());
                }

            }

        }

        internal IAveFile GetFile(string name)
        {
            return mAveItem.GetFile(name);
        }
        internal void ReloadFile(bool fakeDeletedUser = false)
        {
            mAveItem.ReloadFile(fakeDeletedUser);
        }
        public void SetStream(IAveRestoreStream stream)
        {
            mReceiver = stream;
        }

        #region Obsolete method
        [Obsolete("no use now, will remove later")]
        private void CheckIfPossibleDoDiscard()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.CheckIfPossibleDoDiscard"))
            {

                if (RestoreOption.mAveItemRestoreOption.DISCARD_ITEM_POSSIBLE)
                {
                    try
                    {
                        IAveFile spFile = this.GetFile(mDocumentInfo.Name);
                        if (spFile.CheckOutType != AveCheckOutType.None)
                        {
                            spFile.UndoCheckOut();
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while the SPFile UndoCheckOut. file name:{0}\n error message:{1}", mDocumentInfo.Name, ex));
                    }
                }

            }

        }


        [Obsolete("no use now, will remove later")]
        public bool DestinationExist()
        {
            try
            {
                IAveFile file = this.GetFile(mDocumentInfo.Name);
                if (file != null)
                {
                    return file.Exists;
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, string.Format("Get File error. \n error message:{0}", ex));
            }
            return false;
        }
        #endregion

        #region IAveSPDoc Members

        public void CreateSouAndDesDefaultValueWithStream(IAveList list, IAveFile spFile, IAveSPList aveSPList, bool overWrite)
        {
            CreateSouAndDesDefaultValueWithStream(list, spFile, aveSPList as AveSPList, overWrite);
        }

        public void MergeSouAndDesDefaultValueWithStream(IAveList list, IAveFile spFile, IAveSPList aveSPList, bool overWrite)
        {
            MergeSouAndDesDefaultValueWithStream(list, spFile, aveSPList as AveSPList, overWrite);
        }

        public void OverWriteRetionStream(IAveList list, IAveFile spFile, IAveSPList aveSPList, bool overWrite)
        {
            OverWriteRetionStream(list, spFile, aveSPList as AveSPList, overWrite);
        }

        public void ResetParentFolder(IAveSPFolder parentFolder)
        {
            ResetParentFolder(parentFolder as AveSPFolder);
        }

        #endregion

        /// <summary>
        /// Restore Document Metadata Dto
        /// </summary>
        /// <param name="fileRestoreOption"></param>
        /// <param name="documentMetadataDto"></param>
        /// <param name="restoreStream"></param>
        /// <returns></returns>
        //internal MetadataRestoreDetails RestoreDocumentMetadataDto(IAveRestoreStream restoreStream, SPFileRestoreOption fileRestoreOption, SPDocumentMetadataDto documentMetadataDto)
        //{
        //    var restoreDetails = new MetadataRestoreDetails();

        //    this.SetStream(restoreStream);
        //    this.mAveParentSite.RestoreUser(documentMetadataDto.UserCache);
        //    this.mAveParentSite.RestoreGroup(documentMetadataDto.GroupCache);
        //    this.mAveParentSite.RestoreMetadataInfo(documentMetadataDto.MetadataInfo);
        //    this.VerifyItemMetadataDependency(documentMetadataDto, fileRestoreOption);
        //    var result = this.RestoreDocument(documentMetadataDto, fileRestoreOption);

        //    restoreDetails.Status = ConvertToRestoreStatus(result);

        //    return restoreDetails;
        //}

        private static WrapperRestoreStatus ConvertToRestoreStatus(AveRestoreResult restoreResult)
        {
            WrapperRestoreStatus status = WrapperRestoreStatus.None;
            switch (restoreResult)
            {
                case AveRestoreResult.Failed:
                    status = WrapperRestoreStatus.Failed;
                    break;
                case AveRestoreResult.SkipItemUniqueFieldConflict:
                case AveRestoreResult.SkipRecycleBinData:
                case AveRestoreResult.SkipRestoreItemMetaData:
                case AveRestoreResult.SkipTheSameItem:
                    status = WrapperRestoreStatus.Skipped;
                    break;
                default:
                    status = WrapperRestoreStatus.Successful;
                    break;
            }

            return status;
        }

        /// <summary>
        /// Restore Document
        /// </summary>
        /// <param name="documentMetadataDto"></param>
        /// <param name="restoreOption"></param>
        //private AveRestoreResult RestoreDocument(SPDocumentMetadataDto documentMetadataDto, SPFileRestoreOption restoreOption)
        //{
        //    /*
        //     * 2100是slide library，这个library必须关闭才好用
        //     */
        //    using (new AveEventReceiverUtility(mAveSPList != null && mAveSPList.SPList != null && (int)mAveSPList.SPList.BaseTemplate == 2100))
        //    {
        //        restoreOption.ToAveRestoreOption(this.mRestoreOption);
        //        var restoreResult = this.RestoreSelf(documentMetadataDto.DocInfo_Old, documentMetadataDto.UserDataInfo,
        //                                             documentMetadataDto.DocDataJunction, documentMetadataDto.WebParts);
        //        this.RestoreLookupFieldGuidValue(documentMetadataDto.ItemTPGUIDofLookupValue);

        //        return restoreResult;
        //    }
        //}

        public IAveFile File
        {
            get
            {
                EnsureSPFile();
                if (mAveItem != null)
                {
                    return mAveItem.File;
                }
                return null;
            }
            internal set
            {
                mAveItem.File = value;
            }
        }

        public IAveListItem Item
        {
            get
            {
                EnsureSPFile();
                if (mAveItem != null && mAveItem.File != null)
                {
                    return mAveItem.File.Item;
                }

                return null;
            }
        }

        /// <summary>
        /// create restore file according to ave sp doc
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        //internal static ISPFileImport CreateInstance(ISPFolderImport folder, string name)
        //{
        //    return new AveSPDocV1((AveSPFolder)folder, name);
        //}


        private bool loaded = false;

        /// <summary>
        /// Ensure SPFile
        /// </summary>
        /// <returns></returns>
        internal void EnsureSPFile()
        {
            if (!loaded)
            {
                mAveItem.File =
                    ParentWeb.AveWeb.GetCheckoutFile(Path.Combine(ParentFolder.ServerRelativeUrl, mBaseItemInfo.Name));
                    // load 主要是为了单独还原 role assignment以及workflow instance使用。
                mAveItem.ListItem = mAveItem.File.Item;
                loaded = true;
            }
        }
    }

    //internal class AveSPDocV1 : AveSPDoc, ISPFileImport
    //{
    //    private bool alertRestored = false;

    //    internal AveSPDocV1(AveSPFolder aveFolder, string name)
    //        : base(aveFolder, name) { }

    //    internal AveSPDocV1(AveSPFolder aveFolder, string name, int rowId)
    //        : base(aveFolder, name, rowId) { }

    //    internal AveSPDocV1(AveSPSite aveSite)
    //        : base(aveSite) { }

    //    /// <summary>
    //    /// Restore文件
    //    /// 
    //    /// 这个是新加的接口,外围暂时请不要调用
    //    /// </summary>
    //    /// <param name="restoreStream"></param>
    //    /// <param name="spFileRestoreOption"></param>
    //    /// <returns></returns>
    //    public SPFileRestoreReport Restore(IAveRestoreStream restoreStream, SPFileRestoreOption spFileRestoreOption)
    //    {
    //        if (restoreStream == null)
    //        {
    //            throw new ArgumentNullException("restoreStream");
    //        }
    //        if (spFileRestoreOption == null)
    //        {
    //            throw new ArgumentNullException("spFileRestoreOption");
    //        }

    //        SPFileRestoreReport restoreReport = new SPFileRestoreReport();
    //        using (WrapperStopwatch.CreateInstance(spFileRestoreOption.IncludePerformanceDetails, restoreReport.UpdateTimeUsage))
    //        {
    //            this.PreRestore(restoreStream, spFileRestoreOption.FilterUserInfo, spFileRestoreOption.FilterGroupInfo);
    //            AveMetadata metadata = null;

    //            //TODO find

    //            while ((metadata = restoreStream.ReadMetadata()) != null)
    //            {
    //                var action = GetAction(metadata.MetadataType);
    //                if (action != null)
    //                {
    //                    var metadataRestoreReport = new MetadataRestoreReport(metadata.MetadataType);
    //                    using (WrapperStopwatch.CreateInstance(spFileRestoreOption.IncludePerformanceDetails, metadataRestoreReport.AddTimeUsage))
    //                    {
    //                        action(restoreStream, spFileRestoreOption, metadata, metadataRestoreReport);
    //                    }
    //                    restoreReport.Add(metadata.MetadataType, metadataRestoreReport);
    //                }
    //                else
    //                {
    //                    WrapperLogger.Instance.WriteToLogWithResourceKey(WrapperLogger.Level.Error, "TODO:{0}", metadata.MetadataType.ToString());
    //                }
    //            }
    //        }
    //        return restoreReport;
    //    }
        
    //    private Action<IAveRestoreStream, SPFileRestoreOption, AveMetadata, MetadataRestoreReport> GetAction(AveMetadataType metadataType)
    //    {
    //        Action<IAveRestoreStream, SPFileRestoreOption, AveMetadata, MetadataRestoreReport> action = null;
    //        switch (metadataType)
    //        {
    //            case AveMetadataType.DocProperty:
    //            case AveMetadataType.ItemMetadataDto:
    //                action = RestoreItemMetadata;
    //                break;
    //            case AveMetadataType.RoleAssignment:
    //                action = RestoreRoleAssignments;
    //                break;
    //            case AveMetadataType.RoleAssignmentsDto:
    //                action = RestoreRoleAssignmentsDto;
    //                break;
    //            //case AveMetadataType.RoleAssignmentInheritStatus:
    //            //    break;
    //            case AveMetadataType.DocImmedSubscriptions:
    //            case AveMetadataType.DocSchedSubscriptions:
    //                action = RestoreItemAlert;
    //                break;
    //            case AveMetadataType.AlertsDto:
    //                action = RestoreAlertsDto;
    //                break;
    //            case AveMetadataType.SocialTag:
    //                action = RestoreSocialTag;
    //                break;
    //            case AveMetadataType.SocialComment:
    //                action = RestoreSocialComment;
    //                break;
    //            case AveMetadataType.DocumentTagging:
    //                action = RestoreDocumentTag;
    //                break;
    //            case AveMetadataType.WorkflowInstance:
    //                action = RestoreWorkflowInstance;
    //                break;
    //            case AveMetadataType.WorkflowSchedule:
    //                action = RestoreWorkflowSchedule;
    //                break;
    //        }
    //        return action;
    //    }

    //    private void RestoreRoleAssignmentsDto(IAveRestoreStream restoreStream, SPFileRestoreOption restoreOption, AveMetadata metadata, MetadataRestoreReport restoreReport)
    //    {
    //        var roleAssignments = metadata.GetMetadata<AvePoint.Wrapper.Core.SPBackupDto.SPRoleAssignmentsDto>();

    //        if (restoreOption.RoleAssignmentsRestoreOption != null)
    //        {
    //            using (var security = AveObjectSecurity.CreateInstance(this))
    //            {
    //                if (restoreOption.RoleAssignmentsRestoreOption.FilterRoleAssignments != null)
    //                {
    //                    roleAssignments.RoleAssignmentInfos = restoreOption.RoleAssignmentsRestoreOption.FilterRoleAssignments(roleAssignments.RoleAssignmentInfos);
    //                }

    //                if (restoreOption.RoleAssignmentsRestoreOption.RestoreInheritance)
    //                {
    //                    security.SourceHasUniqueRoleAssignment = !roleAssignments.IsInherit;
    //                }

    //                security.ParentSite.RestoreUser(roleAssignments.UserCache);
    //                security.ParentSite.RestoreGroup(roleAssignments.GroupCache);

    //                security.RestoreRoleAssignments(roleAssignments.RoleAssignmentInfos, restoreOption.RoleAssignmentsRestoreOption.ToSecurityRestoreOption());
    //                restoreReport.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                restoreReport.Details.AnalyzeReport(security.GetReport());
    //            }
    //        }
    //    }

    //    private void EnsureItem(int rowId)
    //    {
    //        if (this.SPListItem == null && rowId > 0)
    //        {
    //            this.SPListItem = this.ParentList.SPList.GetItemById(rowId);
    //        }
    //    }

    //    private SPDocumentMetadataDto GetSPDocumentMetadataDto(IAveRestoreStream stream, AveMetadata metadata)
    //    {
    //        SPDocumentMetadataDto docDto = null;
    //        switch (metadata.MetadataType)
    //        {
    //            case AveMetadataType.DocProperty:
    //                docDto = new SPDocumentMetadataDto
    //                             {
    //                                 DocInfo_Old = metadata.GetMetadata<Dictionary<string, object>>(),
    //                                 WebParts = stream.GetMetadataObj<List<AveWebPartBaseInfo>>(AveMetadataType.DocWebPart),
    //                                 MetadataInfo = stream.GetMetadataObj<List<AveTermStoreInfo>>(AveMetadataType.MetadataService),
    //                                 UserDataInfo = stream.GetMetadataObj<Dictionary<string, object>>(AveMetadataType.DocData),
    //                                 DocDataJunction = stream.GetMetadataObj<List<Dictionary<string, object>>>(AveMetadataType.DocDataJunction),
    //                                 ItemTPGUIDofLookupValue = stream.GetMetadataObj<Dictionary<string, string>>(AveMetadataType.LookupFieldGuidValue),
    //                                 ItemUIVersionNums = stream.GetMetadataObj<List<int>>(AveMetadataType.DocVersions)
    //                             };
    //                docDto.IsView = ConvertViewProperty(docDto.DocInfo_Old);
    //                break;
    //            case AveMetadataType.ItemMetadataDto:
    //                docDto = metadata.GetMetadata<SPDocumentMetadataDto>();
    //                break;
    //            default:
    //                WrapperLogger.Instance.WriteToLogWithResourceKey(WrapperLogger.Level.Error, "Invalid MetadataType to get SPDocumentMetadataDto. MetadataType:{0}", metadata.MetadataType.ToString());
    //                break;
    //        }
    //        return docDto;
    //    }

    //    private bool ConvertViewProperty(Dictionary<string, object> docInfo_Old)
    //    {
    //        try
    //        {
    //            if (docInfo_Old != null && docInfo_Old.ContainsKey("IsViewPage"))
    //            {
    //                return (bool)docInfo_Old["IsViewPage"];
    //            }
    //            return false;
    //        }
    //        catch (Exception ex)
    //        {
    //            WrapperLogger.Instance.WriteToLogWithResourceKey(WrapperLogger.Level.Verbose, "Failed to convert view property. Error:{0}", ex.ToString());
    //            return false;
    //        }
    //    }

    //    /// <summary>
    //    /// 处理备份数据，如果外围有特殊处理，也在此执行
    //    /// </summary>
    //    /// <param name="restoreStream"></param>
    //    /// <param name="metadata"></param>
    //    /// <param name="option"></param>
    //    /// <returns> </returns>
    //    private bool ProcessSourceMetadataInfo(SPDocumentMetadataDto metaDto, SPFileRestoreOption option)
    //    {
    //        if (metaDto == null)
    //        {
    //            throw new ArgumentNullException("metaDto");
    //        }
    //        if (option == null)
    //        {
    //            throw new ArgumentNullException("option");
    //        }

    //        this.mDocumentInfo.VerifyItemMMSColumnValue = option.MetadataRestoreOption.VerifyDependency;
    //        this.mDocumentInfo.KeepDefaultValue = option.MetadataRestoreOption.KeepColumnDefaultValue;
    //        this.mDocumentInfo.KeepDestItemRowId = option.MetadataRestoreOption.KeepUniqueIdAndRowId;
    //        this.mDocumentInfo.GhostPageOption = (int)option.GhostPageOption;

    //        bool result = AveDelegateExecutor.SafeExecuteFunc(option.ProcessFileMetadataDto, metaDto);

    //        if (metaDto.DocInfo_Old != null && metaDto.DocInfo_Old.ContainsKey("LeafName") && !metaDto.DocInfo_Old["LeafName"].ToString().Equals(this.mBaseItemInfo.Name, StringComparison.OrdinalIgnoreCase))
    //        {
    //            this.mBaseItemInfo.Name = metaDto.DocInfo_Old["LeafName"].ToString();
    //        }
    //        return result;
    //    }
        
    //    private void RestoreItemMetadata(IAveRestoreStream restoreStream, SPFileRestoreOption restoreOption, AveMetadata metadata, MetadataRestoreReport restoreReport)
    //    {
    //        var metadataDto = GetSPDocumentMetadataDto(restoreStream, metadata);
    //        //if (TryGetItem(restoreOption, metadataDto))
    //        //{
    //        //    HandleConflict(restoreOption);
    //        //    this.mBaseItemInfo.NewCodeRestore = true;
    //        //}
    //        var conflictAction = ProcessConflictCheck(restoreOption, metadataDto);
    //        ProcessConflictResultAction(restoreOption, conflictAction, metadataDto);
    //        this.mBaseItemInfo.NewCodeRestore = true;
    //        ProcessSourceMetadataInfo(metadataDto, restoreOption);
            
    //        restoreReport.Details = this.RestoreDocumentMetadataDto(restoreStream, restoreOption, metadataDto);
    //    }

    //    /// <summary>
    //    /// conflict的选项
    //    /// </summary>
    //    /// <param name="option"></param>
    //    /// <param name="sourceData"></param>
    //    /// <returns></returns>
    //    private SPItemRestoreAction ProcessConflictCheck(SPFileRestoreOption option, SPItemMetadataDto sourceData)
    //    {
    //        if(option.ConflictOption == null)
    //        {
    //            throw new ArgumentNullException("ConflictOption");
    //        }

    //        var restoreAction = option.ConflictOption.NonConflictAction;

    //        if(option.ConflictOption.CheckOptions != null && option.ConflictOption.CheckOptions.Count > 0)
    //        {
    //            foreach(var item in option.ConflictOption.CheckOptions)
    //            {
    //                var conflict = ProcessConflictCheckOption(item, sourceData);

    //                if(option.ConflictOption.CustomConflictResultHandler != null)
    //                {
    //                    var processResult = option.ConflictOption.CustomConflictResultHandler(item, conflict);

    //                    if(!processResult.Item1)
    //                    {
    //                        restoreAction = processResult.Item2;
    //                        break;
    //                    }
    //                }
    //                else if (conflict)
    //                {
    //                    restoreAction = option.ConflictOption.ConflictAction;
    //                    break;
    //                }
    //            }
    //        }
    //        else if(option.ConflictOption.CustomConflictHandler != null)
    //        {
    //            restoreAction = option.ConflictOption.CustomConflictHandler(this.File);
    //        }

    //        return restoreAction;
    //    }

    //    private void ProcessConflictResultAction(SPFileRestoreOption option, SPItemRestoreAction action, SPDocumentMetadataDto metaDto)
    //    {
    //        switch (action)
    //        {
    //            case SPItemRestoreAction.Skip:
    //                throw new AveRestoreException(AveRestoreResult.SkipTheSameItem, "Skip");
    //            case SPItemRestoreAction.DiscardCheckOut:
    //                this.File.UndoCheckOut();
    //                throw new AveRestoreException(AveRestoreResult.Omit, "Omit");
    //            case SPItemRestoreAction.Overwrite:
    //                DeleteFileForOverwriteRestore(option);
    //                break;
    //            case SPItemRestoreAction.NewVersion:
    //                ParentFolder.RestoringItem.ResetNewItemValues(true, mBaseItemInfo.Name, mBaseItemInfo.Name);
    //                ProcessAppendNewVersionAction(metaDto, option);
    //                break;
    //            case SPItemRestoreAction.Default:
    //                ParentFolder.RestoringItem.ResetNewItemValues(true, mBaseItemInfo.Name, mBaseItemInfo.Name);
    //                break;
    //        }
    //    }

    //    /// <summary>
    //    /// Wrapper默认处理NewVersion选项Action的相关Metadata处理。在目的端Version基础上，根据原端增长Version。
    //    /// </summary>
    //    private void ProcessAppendNewVersionAction(SPDocumentMetadataDto metaDto, SPFileRestoreOption option)
    //    {
    //        if (option.ProcessFileMetadataDto == null && Item != null)
    //        {
    //            if (metaDto.DocInfo_Old != null && metaDto.DocInfo_Old.ContainsKey("UIVersion"))
    //            {
    //                var sourceVersion = (int)metaDto.DocInfo_Old["UIVersion"];
    //                var destVersion = File.UIVersion;
    //                if (sourceVersion % 512 != 0)
    //                {
    //                    if (destVersion % 512 == 511)
    //                    {
    //                        metaDto.DocInfo_Old["UIVersion"] = destVersion + 2;
    //                    }
    //                    else
    //                    {
    //                        metaDto.DocInfo_Old["UIVersion"] = ++destVersion;
    //                    }
    //                }
    //                else
    //                {
    //                    metaDto.DocInfo_Old["UIVersion"] = destVersion - destVersion % 512 + 512;
    //                }
    //            }
    //            else
    //            {
    //                throw new ArgumentException("Can't find version info from metadata");
    //            }
    //        }
    //    }


    //    /// <summary>
    //    /// Built-in check的机制
    //    /// </summary>
    //    /// <param name="option"></param>
    //    /// <param name="sourceData"></param>
    //    /// <returns></returns>
    //    protected bool ProcessConflictCheckOption(SPItemConflictCheckOption option, SPItemMetadataDto sourceData)
    //    {
    //        var conflict = false;

    //        switch(option)
    //        {
    //            case SPItemConflictCheckOption.CheckExist:
    //                conflict = (this.File != null && this.File.Exists);
    //                break;
    //            case SPItemConflictCheckOption.CheckModifiedTime:
    //                {
    //                    var file = this.File;
    //                    if (file != null && file.Exists)
    //                    {
    //                        if (file.Item == null)
    //                        {
    //                            conflict = !file.TimeLastModified.Equals(((DateTime)sourceData.DocInfo_Old["TimeLastModified"]).ToUniversalTime());
    //                        }
    //                        else
    //                        {
    //                            conflict = !file.Item[AveBuiltInFieldId.Modified].Equals(((DateTime)sourceData.UserDataInfo["#tp_Modified"]).ToUniversalTime());
    //                        }
    //                    }
    //                    else
    //                    {
    //                        conflict = true;
    //                    }
    //                }
    //                break;
    //            case SPItemConflictCheckOption.CheckNewChanged:
    //                {
    //                    var file = this.File;
    //                    if (file != null && file.Exists)
    //                    {
    //                        if (file.Item == null)
    //                        {
    //                            conflict = file.TimeLastModified < ((DateTime)(sourceData.DocInfo_Old["TimeLastModified"])).ToUniversalTime();
    //                        }
    //                        else
    //                        {
    //                            conflict = ((DateTime)file.Item[AveBuiltInFieldId.Modified]) < ((DateTime)(sourceData.UserDataInfo["#tp_Modified"])).ToUniversalTime();
    //                        }
    //                    }
    //                }
    //                break;
    //            case SPItemConflictCheckOption.CheckRecycleBin:
    //                {
    //                    if (this.mAveParentSite.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel)
    //                    {
    //                        var dto = new RestoringDto();
    //                        dto.NameMapping = this.Name;
    //                        mQueryService.CheckConflictInfo(mDocumentInfo.SiteId, mDocumentInfo.ParentId, dto);
    //                        if (dto.ConflictType == ConflictType.RecycleBin)
    //                        {
    //                            conflict = true;
    //                        }
    //                    }
    //                }
    //                break;
    //            case SPItemConflictCheckOption.CheckVersionNumber:
    //                {
    //                    var file = this.File;
    //                    if (file != null && file.Exists)
    //                    {
    //                        if (file.Item == null)
    //                        {
    //                            conflict = file.UIVersion > (int)(sourceData.DocInfo_Old["UIVersion"]);
    //                        }
    //                        else
    //                        {
    //                            conflict = file.UIVersion > ((int)(sourceData.UserDataInfo["#tp_UIVersion"]));
    //                        }
    //                    }
    //                }
    //                break;
    //            default:
    //                break;
    //        }

    //        return conflict;
    //    }

    //    //private bool TryGetItem(SPFileRestoreOption option, SPItemMetadataDto sourceData)
    //    //{
    //    //    if (option.ConflictCheckOption == SPItemConflictCheckOption.None)
    //    //    {
    //    //        //reasonMsg = "No need to check conflict.";
    //    //        return this.File != null && this.File.Exists;
    //    //    }
    //    //    //请务必使用StringComparison.InvariantCultureIgnoreCase，因为对应的SQL Order By是语言相关的
    //    //    if (string.Compare(this.mBaseItemInfo.Name, this.ParentFolder.MaxSubLeafName, StringComparison.InvariantCultureIgnoreCase) > 0)
    //    //    {//当前doc/folder leaf Name最大，不可能冲突，后续还原List Item比这个还大（源端排序），也不可能冲突，从而达到节省效率的目的。
    //    //        this.ParentFolder.MaxSubLeafName = this.mBaseItemInfo.Name;
    //    //        return false;
    //    //    }

    //    //    //TODO LeafName & Checkout File
    //    //    this.File = this.ParentFolder.SPFolder.Files.First(file => file.Name.Equals(mBaseItemInfo.Name, StringComparison.OrdinalIgnoreCase));
    //    //    bool exist = this.File != null && this.File.Exists;

    //    //    if (!exist && option.ConflictCheckOption == SPItemConflictCheckOption.CheckExist)
    //    //    {
    //    //        throw new FileNotFoundException(string.Format("File:{0} not found in destination.", mBaseItemInfo.Name));
    //    //    }

    //    //    return exist;
    //    //}

    //    ///// <summary>
    //    ///// 冲突处理完成后，需要设置一个标志位，跳过老实现的冲突处理
    //    ///// </summary>
    //    ///// <param name="option"></param>
    //    //private void HandleConflict(SPFileRestoreOption option)
    //    //{
    //    //    SPItemRestoreAction action = GetItemRestoreAction(option);

    //    //    switch (action)
    //    //    {
    //    //        case SPItemRestoreAction.Skip:
    //    //            //TODO Log
    //    //            throw new AveRestoreException(AveRestoreResult.SkipTheSameItem, "Skip");
    //    //        case SPItemRestoreAction.DiscardCheckOut:
    //    //            //TODO Log
    //    //            this.File.UndoCheckOut();
    //    //            throw new AveRestoreException(AveRestoreResult.Omit, "Omit");
    //    //        case SPItemRestoreAction.Overwrite:
    //    //            DeleteFileForOverwriteRestore(option);
    //    //            break;
    //    //        case SPItemRestoreAction.Default:
    //    //            break;
    //    //    }
    //    //}

    //    //private SPItemRestoreAction GetItemRestoreAction(SPFileRestoreOption option)
    //    //{
    //    //    SPItemRestoreAction action = SPItemRestoreAction.Skip;

    //    //    switch (option.ConflictHandleOption)
    //    //    {
    //    //        case SPItemConflictHandleOption.Skip:
    //    //            action = SPItemRestoreAction.Skip;
    //    //            break;
    //    //        case SPItemConflictHandleOption.Custom:
    //    //            if (option.ConflictHandleFunc == null)
    //    //            {
    //    //                //TODO Log;
    //    //                action = SPItemRestoreAction.Skip;
    //    //            }
    //    //            else
    //    //            {
    //    //                action = option.ConflictHandleFunc(this.File);
    //    //            }
    //    //            break;
    //    //        case SPItemConflictHandleOption.Overwrite:
    //    //            action = SPItemRestoreAction.Overwrite;
    //    //            break;
    //    //        default:
    //    //            action = SPItemRestoreAction.Default;
    //    //            break;
    //    //    }

    //    //    return action;
    //    //}

    //    private void DeleteFileForOverwriteRestore(SPFileRestoreOption option)
    //    {
    //        if (ShouldDelete())
    //        {
    //            try
    //            {
    //                HandleWelcomePageSetting();
    //                HandleWorkflowInstance();
    //                if (option.MetadataRestoreOption.KeepUniqueIdAndRowId)
    //                {
    //                    //TODO
    //                }

    //                if (File.CheckOutStatus != AveCheckOutStatus.None)
    //                {
    //                    File.CheckIn(String.Empty);
    //                }
    //                File.Delete();
    //                HandleItemAlerts();
    //            }
    //            catch (Exception ex)
    //            {
    //                WrapperLogger.Instance.WriteToLogWithResourceKey(WrapperLogger.Level.Verbose, ServerAPIResource.PreRestoreDocError, ex.ToString());
    //                ProcessMasterPage();
    //            }
    //        }
    //    }

    //    private bool ShouldDelete()
    //    {
    //        //不是缩略图
    //        //不在Forms文件夹下  "/Forms/"   --SPList.Forms.SchemaXml 时间比较长
    //        //List不是ReportTemplateList
    //        return true;
    //    }

    //    private void HandleWelcomePageSetting()
    //    { 
    //    }

    //    private void HandleItemAlerts()
    //    {
    //    }

    //    private void HandleWorkflowInstance()
    //    { 
    //    }

    //    private void ProcessMasterPage()
    //    {
    //    }

    //    private void BackUpMasterPageSetting()
    //    { }

    //    private void RestoreRoleAssignments(IAveRestoreStream restoreStream, SPFileRestoreOption restoreOption, AveMetadata metadata, MetadataRestoreReport restoreReport)
    //    {
    //        if (restoreOption.RestoreConfiguration.Security == SPObjectRestoreAction.Skip)
    //        {
    //            //log
    //            return;
    //        }
    //        if (restoreOption.RoleAssignmentsRestoreOption != null)
    //        {
    //            var roleAssignments = metadata.GetMetadata<List<AveRoleAssignmentInfo>>();
    //            using (var security = AveObjectSecurity.CreateInstance(this))
    //            {
    //                if (restoreOption.RoleAssignmentsRestoreOption.FilterRoleAssignments != null)
    //                {
    //                    roleAssignments = restoreOption.RoleAssignmentsRestoreOption.FilterRoleAssignments(roleAssignments);
    //                }

    //                security.RestoreRoleAssignments(roleAssignments, restoreOption.RoleAssignmentsRestoreOption.ToSecurityRestoreOption());
    //                //security.SourceHasUniqueRoleAssignment = this.HasUniqueRoleAssignments;
    //                restoreReport.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                restoreReport.Details.AnalyzeReport(security.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreDocumentTag(IAveRestoreStream restoreStream, SPFileRestoreOption restoreOption, AveMetadata metadata, MetadataRestoreReport restoreReport)
    //    {
    //        if (restoreOption.RestoreConfiguration.DocumentTagging == SPObjectRestoreAction.Skip)
    //        {
    //            //log
    //            return;
    //        }
    //        using (AveDocumentTagging docTag = new AveDocumentTagging(this.TagUrl, this.mAveParentSite))
    //        {
    //            docTag.Restore(metadata.GetMetadata<List<AveDocumentTaggingInfo>>());
    //            restoreReport.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            restoreReport.Details.AnalyzeReport(docTag.GetReport());
    //        }
    //    }

    //    private void RestoreSocialTag(IAveRestoreStream restoreStream, SPFileRestoreOption restoreOption, AveMetadata metadata, MetadataRestoreReport restoreReport)
    //    {
    //        if (restoreOption.RestoreConfiguration.SocialTag == SPObjectRestoreAction.Skip)
    //        {
    //            //log
    //            return;
    //        }
    //        using (AveSPSocialTag socialTags = new AveSPSocialTag(this.TagUrl, this.mAveParentSite))
    //        {
    //            socialTags.Restore(metadata.GetMetadata<List<AveSocialTagInfo>>());
    //            restoreReport.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            restoreReport.Details.AnalyzeReport(socialTags.GetReport());
    //        }
    //    }

    //    private void RestoreSocialComment(IAveRestoreStream restoreStream, SPFileRestoreOption restoreOption, AveMetadata metadata, MetadataRestoreReport restoreReport)
    //    {
    //        if (restoreOption.RestoreConfiguration.SocialComment == SPObjectRestoreAction.Skip)
    //        {
    //            //log
    //            return;
    //        }
    //        using (AveSPSocialComment socialComment = new AveSPSocialComment(this.TagUrl, this.mAveParentSite))
    //        {
    //            socialComment.Restore(metadata.GetMetadata<List<AveSocialCommentInfo>>());
    //            restoreReport.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            restoreReport.Details.AnalyzeReport(socialComment.GetReport());
    //        }           
    //    }

    //    private Tuple<List<Dictionary<string, object>>, List<Dictionary<string, object>>> GetAllAlertsInfo(IAveRestoreStream restoreStream, AveMetadata metadata)
    //    {
    //        List<Dictionary<string, object>> immAlerts = new List<Dictionary<string, object>>();
    //        List<Dictionary<string, object>> schedAlerts = new List<Dictionary<string, object>>();
    //        try
    //        {
    //            if (metadata.MetadataType == AveMetadataType.DocImmedSubscriptions)
    //            {
    //                immAlerts = metadata.GetMetadata<List<Dictionary<string, object>>>();
    //                schedAlerts = restoreStream.GetMetadataObj<List<Dictionary<string, object>>>(AveMetadataType.DocSchedSubscriptions);
    //            }
    //            else if (metadata.MetadataType == AveMetadataType.DocSchedSubscriptions)
    //            {
    //                schedAlerts = metadata.GetMetadata<List<Dictionary<string, object>>>();
    //                immAlerts = restoreStream.GetMetadataObj<List<Dictionary<string, object>>>(AveMetadataType.DocImmedSubscriptions);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            WrapperLogger.Instance.WriteToLogWithResourceKey(WrapperLogger.Level.Error, "Failed to get alert info. Error:{0}", ex.ToString());
    //        }
    //        return new Tuple<List<Dictionary<string, object>>, List<Dictionary<string, object>>>(immAlerts, schedAlerts);
    //    }

    //    private void RestoreAlertsDto(IAveRestoreStream restoreStream, SPFileRestoreOption restoreOption, AveMetadata metadata, MetadataRestoreReport restoreReport)
    //    {
    //        if (restoreOption.RestoreConfiguration.DocumentTagging == SPObjectRestoreAction.Skip)
    //        {
    //            return;
    //        }

    //        var alerts = metadata.GetMetadata<SPAlertsDto>();
    //        bool restoreImmed = alerts.ImmedSubscriptions != null && alerts.ImmedSubscriptions.Count > 0;
    //        bool restoreSched = alerts.SchedSubscriptions != null && alerts.SchedSubscriptions.Count > 0;
    //        if (restoreImmed || restoreSched)
    //        {
    //            this.ParentSite.RestoreUser(alerts.UserCache);
    //            using (AveSPAlert alert = AveSPAlert.CreateInstance(this))
    //            {
    //                if (restoreImmed)
    //                {
    //                    alert.RestoreAlerts(alerts.ImmedSubscriptions, false);
    //                }
    //                if (restoreSched)
    //                {
    //                    alert.RestoreAlerts(alerts.SchedSubscriptions, true);
    //                }
    //                restoreReport.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                restoreReport.Details.AnalyzeReport(alert.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreItemAlert(IAveRestoreStream restoreStream, SPFileRestoreOption restoreOption, AveMetadata metadata, MetadataRestoreReport restoreReport)
    //    {
    //        if (alertRestored || restoreOption.RestoreConfiguration.DocumentTagging == SPObjectRestoreAction.Skip)
    //        {
    //            if (alertRestored)
    //            {
    //                return;
    //            }
    //            //log
    //            return;
    //        }

    //        var alertInfos = GetAllAlertsInfo(restoreStream, metadata);
    //        if (alertInfos.Item1.Count == 0 && alertInfos.Item2.Count == 0)
    //        {
    //            return;
    //        }
    //        using (AveSPAlert alert = AveSPAlert.CreateInstance(this))
    //        {
    //            alert.RestoreAlerts(alertInfos.Item1, false);
    //            alert.RestoreAlerts(alertInfos.Item2, true);
    //            restoreReport.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            restoreReport.Details.AnalyzeReport(alert.GetReport());
    //        }
    //        alertRestored = true;
    //    }

    //    private void RestoreWorkflowInstance(IAveRestoreStream restoreStream, SPFileRestoreOption restoreOption, AveMetadata metadata, MetadataRestoreReport restoreReport)
    //    {
    //        if (restoreOption.RestoreConfiguration.WorkflowInstance == SPObjectRestoreAction.Skip)
    //        {
    //            //log
    //            return;
    //        }
    //        if (restoreOption.WorkflowRestoreOption.InstanceRestoreOption.NeedCheckRestoreOption && !this.CheckRestoreOption(this.IsNewCreated, AveRestoreMode.OverWrite))
    //        {
    //            return;
    //        }
    //        var wfInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
    //        WFConflictResolution wfResolution = WFConflictResolution.Instance;
    //        wfResolution.InstanceOption = WFInstanceConflictResolutionOption.OverwriteByModifiedTime;
    //        foreach (var unit in wfInfo)
    //        {
    //            wfResolution.RestoreInstanceData(unit, this);
    //        }
    //        if (wfInfo.Count > 0)//由于对象不一致，导致在还原workflow instance时list.update（UpdateListSettings）出错，现在增加list的reload操作，重新获取一下list对象
    //        {
    //            this.ParentFolder.ParentList.ReloadList();
    //        }
    //    }
    //    private void RestoreWorkflowSchedule(IAveRestoreStream restoreStream, SPFileRestoreOption restoreOption, AveMetadata metadata, MetadataRestoreReport restoreReport)
    //    {
    //        if (restoreOption.RestoreConfiguration.WorkflowSchedule == SPObjectRestoreAction.Skip)
    //        {
    //            //log
    //            return;
    //        }
    //        if (this.CheckRestoreOption(this.IsNewCreated, AveRestoreMode.OverWrite))
    //        {
    //            var wfInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
    //            IWFConflictResolution wfResolution = WFConflictResolution.Instance;
    //            foreach (var unit in wfInfo)
    //            {
    //                wfResolution.RestoreScheduleData(unit, this.SPListItem);
    //            }
    //        }
    //    }

    //}

    /// <summary>
    /// Document Restore Healper
    /// </summary>
    //static class AveDocumentRestoreHelper
    //{
    //    /// <summary>
    //    /// 临时使用
    //    /// </summary>
    //    internal sealed class AveSPDocRestoreDto
    //    {
    //        public AveSPDoc SPDocument;
    //        public IAveRestoreStream RestoreStream;// { get;set}
    //        public SPFileRestoreOption SPFileRestoreOption;
    //        public AveMetadata Metadata;
    //    }

    //    private static readonly AveLogger logger = AveLogger.GetInstance(typeof (AveDocumentRestoreHelper));

    //    private static readonly Dictionary<AveMetadataType, RestoreAction<AveSPDocRestoreDto, MetadataRestoreReport>> restoreActions = new Dictionary<AveMetadataType, RestoreAction<AveSPDocRestoreDto, MetadataRestoreReport>>
    //            {
    //                {AveMetadataType.DocProperty, RestoreDocProperty}, //contains AveMetadataType.LookupFieldGuidValue
    //                {AveMetadataType.RoleAssignment, RestoreRoleAssignments},
    //                {AveMetadataType.RoleAssignmentInheritStatus, RestoreInheritance},
    //                {AveMetadataType.GroupCache, RestoreGroupCache},
    //                {AveMetadataType.UserCache, RestoreUserCache},
    //                {AveMetadataType.DocumentTagging, RestoreDocumentTagging},
    //                {AveMetadataType.DocImmedSubscriptions, RestoreDocImmedSubscriptions},
    //                {AveMetadataType.DocSchedSubscriptions, RestoreDocSchedSubscriptions},
    //                {AveMetadataType.WorkflowInstance, RestoreWorkflowInstance},
    //                {AveMetadataType.WorkflowSchedule, RestoreWorkflowSchedule},
    //                {AveMetadataType.SocialTag, RestoreSocialTag},
    //                {AveMetadataType.SocialComment, RestoreSocialComment},
    //                {AveMetadataType.ItemMetadataDto, RestoreDocumentMetadataDto},
    //                {AveMetadataType.RoleAssignmentsDto, RestoreRoleAssignmentsDto},
    //                {AveMetadataType.AlertsDto, RestoreAlertsDto},
    //                {AveMetadataType.SocialDto, RestoreSocialDto},
    //                {AveMetadataType.WorkflowDto, RestoreWorkflowDto}
    //            };

    //    private static MetadataRestoreReport RestoreWorkflowDto(AveSPDocRestoreDto restoreDto)
    //    {
    //        throw new NotImplementedException();
    //        //return AveWorkflowRestoreHelper.RestoreWorkflowDto(
    //        //    restoreDto.SPFileRestoreOption.IncludePerformanceDetails, restoreDto.Metadata, restoreDto.SPDocument.SPListItem,
    //        //    restoreDto.SPDocument.ParentList, restoreDto.SPFileRestoreOption.WorkflowRestoreOption);
    //    }

    //    private static MetadataRestoreReport RestoreWorkflowSchedule(AveSPDocRestoreDto restoreDto)
    //    {
    //        throw new NotImplementedException();
    //        //return AveWorkflowRestoreHelper.RestoreWorkflowSchedule(
    //        //        restoreDto.SPFileRestoreOption.IncludePerformanceDetails, restoreDto.Metadata, restoreDto.SPDocument.SPListItem);
    //    }

    //    private static MetadataRestoreReport RestoreWorkflowInstance(AveSPDocRestoreDto restoreDto)
    //    {
    //        throw new NotImplementedException();
    //        //return AveWorkflowRestoreHelper.RestoreWorkflowInstance(
    //        //        restoreDto.SPFileRestoreOption.IncludePerformanceDetails, restoreDto.Metadata, restoreDto.SPDocument.SPListItem,
    //        //        restoreDto.SPDocument.ParentList, restoreDto.SPFileRestoreOption.WorkflowRestoreOption);
    //    }

    //    private static MetadataRestoreReport RestoreSocialDto(AveSPDocRestoreDto restoreDto)
    //    {
    //        return AveSocialRestoreHelper.RestoreSocialoDto(restoreDto.SPFileRestoreOption.IncludePerformanceDetails,
    //                                                        restoreDto.SPDocument.ParentSite, restoreDto.Metadata, restoreDto.SPDocument.TagUrl);
    //    }

    //    private static MetadataRestoreReport RestoreAlertsDto(AveSPDocRestoreDto restoreDto)
    //    {
    //        using (var alert = AveSPAlert.CreateInstance(restoreDto.SPDocument))
    //        {
    //            return AveAlertRestoreHelper.RestoreAlertDto(restoreDto.SPFileRestoreOption.IncludePerformanceDetails, alert,
    //                                                  restoreDto.SPDocument.ParentSite, restoreDto.Metadata);
    //        }
    //    }

    //    private static MetadataRestoreReport RestoreSocialComment(AveSPDocRestoreDto restoreDto)
    //    {
    //        return AveSocialRestoreHelper.RestoreSocialComment(restoreDto.SPFileRestoreOption.IncludePerformanceDetails,
    //                                                        restoreDto.SPDocument.ParentSite, restoreDto.Metadata, restoreDto.SPDocument.TagUrl);
    //    }

    //    private static MetadataRestoreReport RestoreSocialTag(AveSPDocRestoreDto restoreDto)
    //    {
    //        return AveSocialRestoreHelper.RestoreSocialTag(restoreDto.SPFileRestoreOption.IncludePerformanceDetails,
    //                                                        restoreDto.SPDocument.ParentSite, restoreDto.Metadata, restoreDto.SPDocument.TagUrl);
    //    }

    //    private static MetadataRestoreReport RestoreDocSchedSubscriptions(AveSPDocRestoreDto restoreDto)
    //    {
    //        using (var alert = AveSPAlert.CreateInstance(restoreDto.SPDocument))
    //        {
    //            return AveAlertRestoreHelper.RestoreDocSchedSubscriptions(restoreDto.SPFileRestoreOption.IncludePerformanceDetails, alert,
    //                                                  restoreDto.Metadata);
    //        }
    //    }

    //    private static MetadataRestoreReport RestoreDocImmedSubscriptions(AveSPDocRestoreDto restoreDto)
    //    {
    //        using (var alert = AveSPAlert.CreateInstance(restoreDto.SPDocument))
    //        {
    //            return AveAlertRestoreHelper.RestoreDocImmedSubscriptions(restoreDto.SPFileRestoreOption.IncludePerformanceDetails, alert,
    //                                                  restoreDto.Metadata);
    //        }
    //    }

    //    private static MetadataRestoreReport RestoreDocumentTagging(AveSPDocRestoreDto restoreDto)
    //    {
    //        return AveSocialRestoreHelper.RestoreDocumentTag(restoreDto.SPFileRestoreOption.IncludePerformanceDetails,
    //                                                        restoreDto.SPDocument.ParentSite, restoreDto.Metadata, restoreDto.SPDocument.TagUrl);
    //    }


    //    /// <summary>
    //    /// Restore Group Cache
    //    /// </summary>
    //    /// <param name="restoreDto"></param>
    //    /// <returns></returns>
    //    private static MetadataRestoreReport RestoreGroupCache(AveSPDocRestoreDto restoreDto)
    //    {
    //        return AveSecurityRestoreHelper.RestoreGroupCache(restoreDto.SPFileRestoreOption.IncludePerformanceDetails,
    //                                                          restoreDto.SPDocument.ParentSite, restoreDto.Metadata);
    //    }

    //    /// <summary>
    //    /// Restore User Cache
    //    /// </summary>
    //    /// <param name="restoreDto"></param>
    //    /// <returns></returns>
    //    private static MetadataRestoreReport RestoreUserCache(AveSPDocRestoreDto restoreDto)
    //    {
    //        return AveSecurityRestoreHelper.RestoreUserCache(restoreDto.SPFileRestoreOption.IncludePerformanceDetails,
    //                                                         restoreDto.SPDocument.ParentSite, restoreDto.Metadata);
    //    }

    //    /// <summary>
    //    /// Restore Inheritance
    //    /// </summary>
    //    /// <param name="restoreDto"></param>
    //    /// <returns></returns>
    //    private static MetadataRestoreReport RestoreInheritance(AveSPDocRestoreDto restoreDto)
    //    {
    //        using (var security = AveObjectSecurity.CreateInstance(restoreDto.SPDocument))
    //        {
    //            return AveSecurityRestoreHelper.RestoreInheritance(
    //                    restoreDto.SPFileRestoreOption.IncludePerformanceDetails,
    //                    restoreDto.Metadata, security, restoreDto.SPFileRestoreOption.RoleAssignmentsRestoreOption);
    //        }
    //    }

    //    /// <summary>
    //    /// Restore role assignments
    //    /// </summary>
    //    /// <param name="restoreDto"></param>
    //    /// <returns></returns>
    //    private static MetadataRestoreReport RestoreRoleAssignments(AveSPDocRestoreDto restoreDto)
    //    {
    //        using (var security = AveObjectSecurity.CreateInstance(restoreDto.SPDocument))
    //        {
    //            return AveSecurityRestoreHelper.RestoreRoleAssignments(
    //                    restoreDto.SPFileRestoreOption.IncludePerformanceDetails,
    //                    restoreDto.Metadata, security, restoreDto.SPFileRestoreOption.RoleAssignmentsRestoreOption);
    //        }
    //    }

    //    /// <summary>
    //    /// Restore Role Assignments
    //    /// </summary>
    //    /// <param name="restoreDto"></param>
    //    /// <returns></returns>
    //    private static MetadataRestoreReport RestoreRoleAssignmentsDto(AveSPDocRestoreDto restoreDto)
    //    {
    //        using (var security = AveObjectSecurity.CreateInstance(restoreDto.SPDocument))
    //        {
    //            return AveSecurityRestoreHelper.RestoreRoleAssignmentsDto(
    //                    restoreDto.SPFileRestoreOption.IncludePerformanceDetails,
    //                    restoreDto.Metadata, security, restoreDto.SPFileRestoreOption.RoleAssignmentsRestoreOption);
    //        }
    //    }

    //    /// <summary>
    //    /// Restore Doc Property
    //    /// </summary>
    //    /// <param name="restoreDto"></param>
    //    /// <returns></returns>
    //    private static MetadataRestoreReport RestoreDocProperty(AveSPDocRestoreDto restoreDto)
    //    {
    //        return RestoreActionExecutor.ExecuteAction(AveMetadataType.ItemMetadataDto,
    //                                                       restoreDto.SPFileRestoreOption.IncludePerformanceDetails,
    //                                                       () =>
    //                                                       {
    //                                                           var documentMetadataDto = new SPDocumentMetadataDto
    //                                                           {
    //                                                               DocInfo_Old = restoreDto.Metadata.GetMetadata<Dictionary<string, object>>(),
    //                                                               WebParts = restoreDto.RestoreStream.GetMetadataObj<List<AveWebPartBaseInfo>>(AveMetadataType.DocWebPart),
    //                                                               MetadataInfo = restoreDto.RestoreStream.GetMetadataObj<List<AveTermStoreInfo>>(AveMetadataType.MetadataService),
    //                                                               UserDataInfo = restoreDto.RestoreStream.GetMetadataObj<Dictionary<string, object>>(AveMetadataType.DocData),
    //                                                               DocDataJunction = restoreDto.RestoreStream.GetMetadataObj<List<Dictionary<string, object>>>(AveMetadataType.DocDataJunction),
    //                                                               ItemTPGUIDofLookupValue = restoreDto.RestoreStream.GetMetadataObj<Dictionary<string, string>>(AveMetadataType.LookupFieldGuidValue)
    //                                                           };

    //                                                           return restoreDto.SPDocument.RestoreDocumentMetadataDto(restoreDto.RestoreStream, restoreDto.SPFileRestoreOption, documentMetadataDto);
    //                                                       });
    //    }

    //    /// <summary>
    //    /// Restore Doc Metadata
    //    /// </summary>
    //    /// <param name="restoreDto"></param>
    //    /// <returns></returns>
    //    private static MetadataRestoreReport RestoreDocumentMetadataDto(AveSPDocRestoreDto restoreDto)
    //    {
    //        return RestoreActionExecutor.ExecuteAction(AveMetadataType.ItemMetadataDto,
    //                                                   restoreDto.SPFileRestoreOption.IncludePerformanceDetails,
    //                                                   () =>
    //                                                   {
    //                                                       var documentMetadataDto =
    //                                                           restoreDto.Metadata
    //                                                                     .GetMetadata<SPDocumentMetadataDto>();

    //                                                       return
    //                                                           restoreDto.SPDocument.RestoreDocumentMetadataDto(
    //                                                               restoreDto.RestoreStream,
    //                                                               restoreDto.SPFileRestoreOption,
    //                                                               documentMetadataDto);
    //                                                   });
    //    }

    //    /// <summary>
    //    /// Handle Metadata
    //    /// </summary>
    //    /// <param name="docRestoreDto"></param>
    //    /// <returns></returns>
    //    internal static MetadataRestoreReport HandleMetadata(AveSPDocRestoreDto docRestoreDto)
    //    {
    //        RestoreAction<AveSPDocRestoreDto, MetadataRestoreReport> restoreAction = null;

    //        if (restoreActions.TryGetValue(docRestoreDto.Metadata.MetadataType, out restoreAction))
    //        {
    //            return restoreAction(docRestoreDto);
    //        }
    //        else
    //        {
    //            logger.Error("Cannot handle this type:{0}", docRestoreDto.Metadata.MetadataType);
    //            //TODO 以后需要处理这个
    //        }

    //        return null;
    //    }
    //}
}