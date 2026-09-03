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
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Collections;
using AvePoint.Common;
using AvePoint.Wrapper.Resource;
using AvePoint.GCommon.Utility.I18N;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Restore.Core;
using AvePoint.GCommon.Utility;
using System.Web;
using AvePoint.GCommon.Contract.Common;
using Util.MIP;
using Microsoft.Online.SharePoint.TenantManagement;
using AngleSharp.Io;

namespace AvePoint.Wrapper.Restore
{
    [AveCodeReviewAttribute("2012/03/09", "Navy.Li@avepoint.com", "Bingkun.Wang@AvePoint.com",
        new string[] { CodeReviewConstants.CHECK_LIST_ID_CO_1,
                       CodeReviewConstants.CHECK_LIST_ID_CO_6,
                       CodeReviewConstants.CHECK_LIST_ID_FA_4 }, null, true)]
    public class AveSPDoc : RestoreableObject,IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(AveSPDoc));
        private IReport report = new AveWrapperReport();
        public IReport GetReport()
        {
            return report;
        }
        private AveSPFolder mAveSPFolder;
        private AveSPSite mAveParentSite;
        private AveItemHoldRecord mFileHold;
        private List<IAveListItem> mHoldItems;
        private Hashtable mHTMetaInfo;
        public AveSPSite ParentSite
        {
            get { return mAveParentSite; }
        }
        private IAveBackupRestoreQueryService mQueryService;
        private AveSPItem mAveSPItem;
        private AveDocumentInfo mDocumentInfo = new AveDocumentInfo();
        private SharePointDocumentDataProcessor dataProcessor;

        public IAveWeb Web
        {
            get { return AveSPItem.SPWeb; }
            set { AveSPItem.SPWeb = value; }
        }

        public AveSPItem AveSPItem
        {
            get { return mAveSPItem; }
        }

        public bool IsNewCreated
        {
            get { return mDocumentInfo.IsNewCreatedDoc | mDocumentInfo.IsNewCreatedView; }
        }

        public bool NeedChangeItemId
        {
            get { return mDocumentInfo.NeedChangeItemId; }
            set { mDocumentInfo.NeedChangeItemId = value; }
        }

        public IAveFile SPFile
        {
            get { return AveSPItem.SPFile; }
            set { AveSPItem.SPFile = value; }
        }

        public IAveView SPView
        {
            get { return mDocumentInfo.AveItem.View; }
        }

        public AveViewDocInfo AveView
        {
            get { return mDocumentInfo.AveView; }
        }

        public AveSPFolder ParentFolder
        {
            get { return mAveSPFolder; }
        }

        public string Name
        {
            get { return mDocumentInfo.Name; }
        }

        public bool IsCurrentVersion
        {
            get { return mDocumentInfo.IsCurrentVersion; }
        }

        public string Url
        {
            get
            {
                if (string.IsNullOrEmpty(mDocumentInfo.Url))
                {
                    string fileUrl = (mAveSPFolder.ServerRelativeUrl + "/" + mDocumentInfo.Name).Substring(Web.ServerRelativeUrl.Length);
                    mDocumentInfo.Url = Web.Url + fileUrl;
                }
                return mDocumentInfo.Url;
            }
        }

        public string TagUrl
        {
            get
            {
                if (SPFile != null && SPFile.ParentFolder != null && SPFile.ParentFolder.ParentWeb != null && SPFile.ParentFolder.ParentWeb.Url != null)
                    return this.SPFile.ParentFolder.ParentWeb.Url.TrimEnd('/') + "/" + this.SPFile.Url.TrimStart('/');
                else
                    return string.Empty;
            }
        }

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

        public string OwnerLoginName
        {
            get
            {
                return mAveSPItem.OwnerLoginName;
            }
        }

        public Guid OldUniqueId
        {
            get
            {
                return mDocumentInfo.OldUniqueId;
            }
        }

        //[Obsolete("This constructor is only used for unit test")]
        protected AveSPDoc()
        { }

        public AveSPDoc(AveSPFolder aveFolder, string name)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.Constructor"))
            {
#endif
                aveFolder.ParentList.ParentWeb.ReloadWebAndParentInternalForSPRequestTimeout(false);
                mAveSPFolder = aveFolder;
                mDocumentInfo.Name = name;
                int pos = mDocumentInfo.Name.IndexOf(':');
                if (pos >= 0)
                {
                    mDocumentInfo.Name = mDocumentInfo.Name.Substring(0, pos);
                }
                mDocumentInfo.SiteId = aveFolder.ParentList.ParentWeb.ParentSite.SPSite.ID;
                mDocumentInfo.ParentId = aveFolder.Id;
                if (this.ParentFolder.ParentList.ParentWeb.WebInfo != null)
                {
                    mDocumentInfo.SourceWebUrl = this.ParentFolder.ParentList.ParentWeb.WebInfo.Url;
                }
                mQueryService = aveFolder.QueryService;

                //Web = mAveSPFolder.ParentList.ParentWeb.SPWeb;
                mAveParentSite = aveFolder.ParentList.ParentWeb.ParentSite;
                mDocumentInfo.KeepDefaultValue = mAveParentSite.KeepDefaultValue;
                mDocumentInfo.VerifyItemMMSColumnValue = mAveParentSite.VerifyItemMMSColumnValue;
                mAveSPItem = new AveSPItem(mDocumentInfo, AveItemType.Document, mAveSPFolder, mQueryService);//构造时会给Web赋值
                mAveSPItem.IsNewCreatedDoc = mAveSPFolder.IsNewCreated;

#if PerformanceLog
            }
#endif
        }

        public AveSPDoc(AveSPSite aveSite)
        {
            mAveParentSite = aveSite;
            mDocumentInfo.KeepDefaultValue = mAveParentSite.KeepDefaultValue;
            mDocumentInfo.VerifyItemMMSColumnValue = mAveParentSite.VerifyItemMMSColumnValue;
            mQueryService = aveSite.QueryService;
            mAveSPItem = new AveSPItem(aveSite);

            if (this.ParentFolder != null && this.ParentFolder.ParentList != null && this.ParentFolder.ParentList.ParentWeb.WebInfo != null)
            {
                mDocumentInfo.SourceWebUrl = this.ParentFolder.ParentList.ParentWeb.WebInfo.Url;
            }
        }
        public void ResetParentFolder(int maxUrlLength)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.ResetParentFolder"))
            {
#endif
                try
                {
                    while (mAveSPFolder.SPFolder.Url.Length + mAveSPFolder.SPFolder.ParentWeb.ServerRelativeUrl.Length + mDocumentInfo.Name.Length + 1 > maxUrlLength && !mAveSPFolder.SPFolder.Url.Equals(mAveSPFolder.ParentList.SPList.RootFolder.Url))
                    {
                        mAveSPFolder.SPFolder = mAveSPFolder.SPFolder.ParentFolder;
                        mDocumentInfo.HasMoveUp = true;
                    }
                    mDocumentInfo.ParentId = mAveSPFolder.SPFolder.UniqueId;
                    if (mAveSPItem != null)
                    {
                        mAveSPItem.ParentFolder.SPFolder = mAveSPFolder.SPFolder;
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
        public void ResetParentFolder(bool moveUptoRootFolder, bool moveUptoHighLevelFolder)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.ResetParentFolder"))
            {
#endif
                try
                {
                    if (moveUptoRootFolder)
                    {
                        mAveSPFolder.SPFolder = mAveSPFolder.ParentList.SPList.RootFolder;//.SPFolder.ParentWeb.Lists[mAveSPFolder.SPFolder.ParentListId].RootFolder;
                        mDocumentInfo.ParentId = mAveSPFolder.SPFolder.UniqueId;
                        if (mAveSPItem != null)
                        {
                            mAveSPItem.ParentFolder.SPFolder = mAveSPFolder.SPFolder;
                        }
                        mDocumentInfo.HasMoveUp = true;
                    }
                    else if (moveUptoHighLevelFolder)
                    {
                        mAveSPFolder.SPFolder = mAveSPFolder.SPFolder.ParentFolder;
                        mDocumentInfo.ParentId = mAveSPFolder.SPFolder.UniqueId;
                        if (mAveSPItem != null)
                        {
                            mAveSPItem.ParentFolder.SPFolder = mAveSPFolder.SPFolder;
                        }
                        mDocumentInfo.HasMoveUp = true;
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
            mAveSPFolder = parentFolder;
            mDocumentInfo.ParentId = parentFolder.Id;
            if (mAveSPItem != null)
            {
                mAveSPItem = new AveSPItem(mDocumentInfo, AveItemType.Document, mAveSPFolder, mQueryService);
            }
            mDocumentInfo.HasMoveUp = true;
        }

        public void SetStream(IAveRestoreStream stream)
        {
            mAveSPItem.SetStream(stream);
        }

        public string ResetAvailableName()
        {
            return ResetAvailableName(DateTime.MinValue);
        }

        public string ResetAvailableName(DateTime timeLastModified)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.ResetAvailableName"))
            {
#endif
                string newFileName = mDocumentInfo.Name;
                try
                {
                    IAveFile file = mAveSPItem.GetFile(mDocumentInfo.Name);
                    if (file == null || !file.Exists)
                    {
                        return newFileName;
                    }
                    //SAAS-27786 在2013request中的GetFile方法里，修改了对item["Modified"]的赋值，如果找到更好的方法，可以修改，并减少一次请求
                    DateTime destTimeLastModified = file.Item == null ? file.TimeLastModified : (DateTime)file.Item["Modified"];
                    if (destTimeLastModified != null && destTimeLastModified == timeLastModified)
                    {
                        log.Info("TimeLastModified in destination and source are same, we won't append name. document Name:{0},destTime:{1},sourceTime:{2}", mDocumentInfo.Name, destTimeLastModified.ToString(), timeLastModified.ToString());
                    }
                    else if (!RestoreOption.mAveItemRestoreOption.SKIP_IF_SAME_MODIFIEDTIME || destTimeLastModified != timeLastModified)
                    {
                        string extension = string.Empty;
                        string prevName = mDocumentInfo.Name;
                        int pos = mDocumentInfo.Name.LastIndexOf('.');
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

                            file = mAveSPItem.GetFile(temp.ToString());
                            if (!file.Exists)
                            {
                                newFileName = file.Name;
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

#if PerformanceLog
            }
#endif
        }

        public bool NeedAppendNewVersion(DateTime timeLastModified)
        {
            bool needAppendNewVersion = false;
            try
            {
                IAveFile file = mAveSPItem.GetFile(mDocumentInfo.Name);
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
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.ResetAvailableName"))
            {
#endif
                string newFileName = string.Empty;
                IAveFile file = null;
                try
                {
                    if (needIncluded)
                    {
                        file = mAveSPItem.GetFile(oldName);
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
                    for (int i = 0; i <= 1000; ++i)
                    {
                        StringBuilder temp = new StringBuilder(prevName);
                        temp.Append("_");
                        temp.Append(i.ToString());
                        temp.Append(extension);
                        try
                        {
                            file = mAveSPItem.GetFile(temp.ToString());
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
#if PerformanceLog
            }
#endif
        }
        public void ResetName(string newName)
        {
            mDocumentInfo.Name = newName;
        }

        internal void ProcessWebPartCondtion(IList<AveWebPartBaseInfo> webParts)
        {
            if (webParts == null)
            {
                return;
            }
            if (mAveParentSite.SPContextKind != AveContextKind.ClientObjectModel && this.mAveParentSite.MappingManager.SiteMappingManager.WebPartTypeIDMapping.Count == 0)
            {
                this.mAveParentSite.MappingManager.SiteMappingManager.LoadWebPartIDMapping(mAveParentSite.SPSite);
            }
            AveSPWebPartManager manager = new AveSPWebPartManager(this);
            mDocumentInfo.WebPartCache = manager.GetWebPartCache();
            mDocumentInfo.WebParts = webParts.ToList();
        }

        public bool ProcessPreCondition(Dictionary<string, object> allDocData, Dictionary<string, object> allUserData, List<Dictionary<string, object>> dataJunction)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.ProcessPreCondition"))
            {
#endif
            mAveSPItem.SetRestoreOption(mRestoreOption);
            //SAAS-38248
            SetDocDataToDocInfoWhichNeedToDelete(allDocData);
            mAveSPItem.ProcessPreCondition(allDocData, allUserData);
            RestoreOption.mAveItemRestoreOption.DELETE_ITEM = mAveSPFolder.RestoringItem.Init(mDocumentInfo.Name, CheckRestoreOption(IsNewCreated, AveRestoreMode.OverWrite), RestoreOption.mAveItemRestoreOption.DELETE_ITEM);
            mDocumentInfo.SettingInfo.DELETE_ITEM = RestoreOption.mAveItemRestoreOption.DELETE_ITEM;
            mDocumentInfo.SettingInfo.IsProcessSolutionStatus = RestoreOption.mAveItemRestoreOption.IsProcessSolutionStatus;
            mDocumentInfo.SettingInfo.MIG_STUB_PIC_THUMBNAILS = mRestoreOption.mAveStorgeOption.MIG_STUB_PIC_THUMBNAILS;
            mDocumentInfo.SettingInfo.SKIP_IF_SAME_MODIFIEDTIME = mRestoreOption.mAveItemRestoreOption.SKIP_IF_SAME_MODIFIEDTIME;
            mDocumentInfo.RestoringItem = mAveSPFolder.RestoringItem;
            mDocumentInfo.IsOrignialCheckOut = (mDocumentInfo.OriginalLevel == 255);
            mDocumentInfo.SourceSiteInfo = mAveParentSite.SourceSiteInfo;
            mDocumentInfo.ParentSiteServerRelativeUrl = mAveParentSite.ServerRelativeUrl;
            if (allDocData.ContainsKey("HasUniqueRoleAssignments"))
            {
                mDocumentInfo.HasUniqueRoleAssignments = (bool)allDocData["HasUniqueRoleAssignments"];
                allDocData.Remove("HasUniqueRoleAssignments");
            }

            //retention xml processor has been support as default value column logic
            //if (allDocData.ContainsKey("LeafName") && mAveSPFolder.ParentList.SPList != null)
            //{
            //    if (allDocData["LeafName"].ToString().Equals("RetentionPolicy.Xml", StringComparison.OrdinalIgnoreCase) && !allDocData.ContainsKey("DoclibRowId"))
            //    {
            //        mAveSPFolder.RestoringItem.NeedSkipped = true;
            //        throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
            //    }
            //}

            if (allUserData.Keys.Contains("SolutionId"))
            {
                mDocumentInfo.SolutionId = new Guid(allUserData["SolutionId"].ToString());
            }

            mAveSPItem.HasStream = allDocData.ContainsKey("HasStream") ? (Convert.ToInt32(allDocData["HasStream"])) == 1 : false;

            mDocumentInfo.CheckinComment = string.Empty;
            if (allDocData.ContainsKey("CheckinComment"))
            {
                mDocumentInfo.CheckinComment = (string)allDocData["CheckinComment"];
            }
            else if (allUserData.ContainsKey("#tp_CheckinComment"))
            {
                mDocumentInfo.CheckinComment = (string)allUserData["#tp_CheckinComment"];
            }
            //if (!mAveParentSite.ObjectModelFactory.ContextKind.Equals(AveContextKind.ClientObjectModel))
            //{
            mDocumentInfo.OrignialID = allDocData.ContainsKey("Id") ? (Guid)allDocData["Id"] : Guid.Empty;
            //}
            //else
            //{
            //    mDocumentInfo.OrignialID = Guid.Empty;
            //}

            if (mAveSPFolder.CurrentDocStatus == null)
            {
                mAveSPFolder.CurrentDocStatus = new CurrentRestoreDocStatus();
            }
            if (mDocumentInfo.Name != mAveSPFolder.CurrentDocStatus.Name)
            {
                mAveSPFolder.CurrentDocStatus.Name = mDocumentInfo.Name;
                mAveSPFolder.CurrentDocStatus.HasPreCurrentVersion = false;
                mDocumentInfo.HasPreCurrentVersion = false;
            }
            if (allDocData.ContainsKey("IsCurrentVersion"))
            {
                mDocumentInfo.IsCurrentVersion = Convert.ToBoolean(allDocData["IsCurrentVersion"]);
            }
            mAveSPFolder.CurrentDocStatus.Status = mDocumentInfo.ModerationStatus;
            mAveSPFolder.CurrentDocStatus.UIVersion = mDocumentInfo.OriginalVersion;
            mAveSPItem.OriginalModerationStatus = mDocumentInfo.ModerationStatus;
            if (ParentFolder.ParentList.SPList != null && (ParentFolder.ParentList.SPList.BaseTemplate == AveListTemplateType.PictureLibrary))
            {
                if (allUserData.ContainsKey("ImageWidth") && allUserData.ContainsKey("ImageHeight"))
                {
                    mAveSPItem.SetPicProperty((int)allUserData["ImageWidth"], (int)allUserData["ImageHeight"]);
                }
            }
            ProcessViewInfo(allDocData, allUserData);
            if (mDocumentInfo.IsView && mDocumentInfo.Needskip)
            {//Explorer View 不需要还原
                mDocumentInfo.RestoringItem.NeedSkipped = true;
                //mLog.Warn("source baseViewID is not in destination view baseViewIDs. view title:Explorer View");
                //return AveRestoreResult.Omit;
                throw new AveRestoreException(AveRestoreResult.SkipRestoreItemMetaData, AveRestoreResult.SkipRestoreItemMetaData.ToString());
            }
            //还原时不勾选IncludeListView,skip还原view
            if (mDocumentInfo.IsView && !WrapperConfiguration.WrapperConfigurationForBPOS.IncludeListView)
            {
                mDocumentInfo.RestoringItem.NeedSkipped = true;
                mDocumentInfo.RestoringItem.NeedSkippedKey = WrapperReportResourceKey.Wrapper_SkippedByNotIncludeListView.ToString();
                mDocumentInfo.RestoringItem.NeedSkippedReason = WrapperRestoreReportResource.Wrapper_SkippedByNotIncludeListView;
                throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
            }
            ProcessGhostInfo(allDocData, allUserData);
            if (allDocData.ContainsKey("MetaInfo"))
            {
                byte[] bts = (byte[])allDocData["MetaInfo"];
                string metaInfo = string.Empty;
                try
                {
                    if (AveCompressedUtility.IsTCompressedBytes(bts))
                    {
                        metaInfo = AveCompressedUtility.GetTCompressedString(bts);
                    }
                    else
                    {
                        metaInfo = Encoding.UTF8.GetString(bts);
                    }
                    mDocumentInfo.MetaInfoDic = AveCompressedUtility.GetMetaInfoDictionary(metaInfo);
                    //mAveSPItem.InitFieldsInMetaInfo(metaInfoDic);
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
            if (allUserData.ContainsKey("#tp_CopySource"))
            {
                string copysource = allUserData["#tp_CopySource"].ToString();
                mDocumentInfo.CopySource = AveReplaceProcessor.UrlReplace(copysource, this.mAveParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
            }
            if (allUserData.ContainsKey("#tp_HasCopyDestinations"))
            {
                bool hascopy = false;
                if (bool.TryParse(allUserData["#tp_HasCopyDestinations"].ToString(), out hascopy))
                    mDocumentInfo.HasCopyDestinations = hascopy;
            }
            mDocumentInfo.SettingInfo.OverWriteByModifiedTime = CheckRestoreOption(AveRestoreMode.OverWriteByModifiedTime);
            //mDocumentInfo.FieldsInfo.Fields = mAveSPFolder.ParentList.AveFields.GetFieldValues(mDocumentInfo.Name, -1, mDocumentInfo.OriginalVersion, allUserData, true);

            if (dataProcessor != null)
            {
                dataProcessor.ProcessUserData(allUserData);
            }

            Dictionary<string, object> userData;
            Dictionary<string, object> uniqueValues;
            if (WrapperConfiguration.WrapperConfigurationForBPOS.IsEndUserRestore)
            {
                allUserData.AddRange(RestoreJunctionDataForEndUser(dataJunction));
            }
            mAveSPFolder.ParentList.AveFields.GetFieldValues(mDocumentInfo.Name, mDocumentInfo.OriginalRowId, mDocumentInfo.OriginalVersion, allUserData, true, out userData, out uniqueValues);

            allUserData["#DefaultValues"] = mAveSPFolder.ParentList.DefaultValues;

            mDocumentInfo.FieldsInfo.Fields = userData;
            mDocumentInfo.FieldsInfo.UniqueValueFields = uniqueValues;
            mDocumentInfo.FieldsInfo.MultilookupFields = mAveSPItem.GetDataJunction(dataJunction);
            //mDocumentInfo.NeedSetNullFields = mAveSPFolder.ParentList.SetNeedSetNullFields(mDocumentInfo.FieldsInfo.Fields);
            #region conflict resolution
            if (mRestoreOption.CheckRestoreOption(AveRestoreMode.AppendANewVersion) && !mDocumentInfo.IsView && !mDocumentInfo.IsGhostPage && !mDocumentInfo.Needskip)
            {
                mDocumentInfo.RestoringItem.IsNewItem = true;
                mDocumentInfo.IsVersion = true;
            }
            #endregion
            mAveSPItem.GetTaxonomyTermIdMapping(mDocumentInfo.FieldsInfo.Fields, mDocumentInfo);

            #region -- add for stub data restore
            //if (mAveSPItem.IsStubData && mAveSPItem.AveStorage != null)
            //{
            //    mAveSPItem.AveStorage.UpdateFileStubByNative(mAveParentSite.SPSite.ID, mAveSPItem.SPFile, currentFileVersion, mDocumentInfo.OriginalVersion, mSqlConn, DateType.Stub);
            //}
            //if (mAveSPItem.AveStorage != null && !mAveSPItem.IsStubData && mAveSPItem.AveStorage is AveConnector)
            //{
            //    mAveSPItem.InitBySPListItem(AveSPItem.SPFile.Item);
            //    mAveSPItem.ResetContentToFileShare();
            //}
            #endregion

            #region

            mHTMetaInfo = new Hashtable();

            if ((allUserData.ContainsKey("_vti_ItemHoldRecordStatus")) && (!allUserData["_vti_ItemHoldRecordStatus"].ToString().Equals("0")) && (allDocData.ContainsKey("MetaInfo")))
            {
                byte[] dateMetaInfo = (byte[])allDocData["MetaInfo"];
                try
                {
                    mFileHold = mAveSPItem.GetHoldRecord(mHTMetaInfo, dateMetaInfo, allUserData);
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
            //TrimItemMetaInfo(mHTMetaInfo);
            #endregion
            if (mAveSPFolder.ParentList.SPList != null && AveSPUtility.IsOrInSystemFormsFolder(mAveSPFolder.SPFolder) && Name.Equals("client_LocationBasedDefaults.html", StringComparison.OrdinalIgnoreCase))
            {
                SPFile = GetFile();
                if (SPFile != null)
                {
                    if (SPFile.Exists)
                    {
                        mAveSPItem.MergeSouAndDesDefaultValueWithStream(
                            mAveSPFolder.ParentList.SPList, SPFile, mAveSPFolder.ParentList, CheckRestoreOption(IsNewCreated, AveRestoreMode.OverWrite));
                    }
                    else
                    {
                        IAveFolder folder = mAveSPFolder.SPFolder;
                        string path = folder.ServerRelativeUrl.TrimEnd('/') + "/" + Name;
                        folder.ParentWeb.GetFolder(folder.ServerRelativeUrl.TrimEnd('/')).Files.Add(path, AveTemplateFileType.FormPage);
                        SPFile = folder.ParentWeb.GetFile(path);
                        mAveSPItem.CreateSouAndDesDefaultValueWithStream(
                            mAveSPFolder.ParentList.SPList, SPFile, mAveSPFolder.ParentList, CheckRestoreOption(IsNewCreated, AveRestoreMode.OverWrite));
                    }
                }
                return false;
            }

            if (mAveSPFolder.ParentList.SPList != null && AveSPUtility.IsOrInSystemFormsFolder(mAveSPFolder.SPFolder) && Name.Equals("RetentionPolicy.Xml", StringComparison.OrdinalIgnoreCase))
            {
                SPFile = GetFile();
                if (SPFile != null && SPFile.Exists)
                {
                    mAveSPItem.OverWriteRetionStream(mAveSPFolder.ParentList.SPList,
                                                                    SPFile,
                                                                    mAveSPFolder.ParentList,
                                                                    CheckRestoreOption(IsNewCreated, AveRestoreMode.OverWrite));
                    return false;
                }
            }
            return true;
#if PerformanceLog
            }
#endif
        }

        private Dictionary<string, object> RestoreJunctionDataForEndUser(List<Dictionary<string, object>> junctionData)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            if (junctionData != null && junctionData.Count > 0)
            {
                foreach (var multiValueCol in junctionData)
                {
                    object fieldId;
                    if (multiValueCol.TryGetValue("tp_FieldId", out fieldId))
                    {
                        Guid id = (Guid)fieldId;

                        var field = mAveSPFolder.ParentList.AveFields.XmlFields.FirstOrDefault(kv => kv.Value.ID.Equals(id));

                        if (field.Value != null && (!result.ContainsKey(field.Value.FieldInternalName)))
                        {
                            result.Add(field.Value.FieldInternalName,"");
                        }
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// SAAS-38248
        /// 有一些属性在备份的时候备份出来，获取之后并在还原之前需要立马删除掉，在这个方法处理
        /// </summary>
        /// <param name="allDocData"></param>
        private void SetDocDataToDocInfoWhichNeedToDelete(Dictionary<string, object> allDocData)
        {
            try
            {
                if (allDocData != null && allDocData.Any())
                {
                    if (allDocData.ContainsKey("CommentsDisabled"))
                    {
                        mDocumentInfo.SourceCommentsDisabled = (bool)allDocData["CommentsDisabled"];
                        allDocData.Remove("CommentsDisabled");
                    }
                    if (allDocData.ContainsKey("CommentsDisabledScope"))
                    {
                        mDocumentInfo.SourceCommentsDisabledScope = (int)allDocData["CommentsDisabledScope"];
                        allDocData.Remove("CommentsDisabledScope");
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("An error occured when set docdata to docinfo which need to delete, due to:{0}", e);
            }
        }
        private Stream FixEmbededWebParts(string fileName, Stream fileStream)
        {
            if (!string.IsNullOrEmpty(fileName)
                && (fileName.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase)
                || fileName.EndsWith(".master", StringComparison.OrdinalIgnoreCase)
                || fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
                && this.ParentFolder.ParentList.SPList != null && this.ParentFolder.ParentList.SPList.BaseTemplate == AveListTemplateType.MasterPageCatalog))
            {
                string page = new StreamReader(fileStream, Encoding.UTF8).ReadToEnd();
                HtmlDocument doc = new HtmlDocument();
                if (fileName.EndsWith(".master", StringComparison.OrdinalIgnoreCase))
                {
                    doc.OptionKeepOriginalSelfCloseTagFormat = true;
                    doc.OptionServerCodeHeaderUnderAttributeValue = new System.Text.RegularExpressions.Regex("^<SharePoint:");
                }
                doc.OptionOutputOriginalCase = true;
                doc.LoadHtml(page);
                Stream htmlContent = new MemoryStream((int)fileStream.Length);
                StreamWriter sw = new StreamWriter(htmlContent, Encoding.UTF8);
                var errorMessage = new StringBuilder();
                foreach (var error in doc.ParseErrors)
                {
                    errorMessage.AppendFormat("parse error details:{0}, code:{1}, line:{2}, position:{3}, source:{4}, stream position:{5}\r\n",
                        error.Reason, error.Code, error.Line, error.LinePosition, error.SourceText, error.StreamPosition);
                }

                if (errorMessage.Length > 0)
                {
                    log.Warn("there are some exceptions when parsing the document:{0}, details:{1}", fileName, errorMessage);
                    sw.Write(page);
                }
                else
                {
                    if (FixWebpartBrokenLinks(doc))
                    {
                        FixHtmlBrokenLinks(doc);
                        sw.Write(doc.DocumentNode.OuterHtml);
                    }
                    else if (FixHtmlBrokenLinks(doc))
                    {
                        sw.Write(doc.DocumentNode.OuterHtml);
                    }
                    else
                    {
                        sw.Write(page);
                    }
                }
                sw.Flush();
                htmlContent.Position = 0;
                return htmlContent;
            }
            return fileStream;
        }

        private bool FixWebpartBrokenLinks(HtmlDocument doc)
        {
            var isFixed = false;
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//node()[@__webpartid]");
            if (nodes != null)
            {
                AveSPWebPartManager manager = new AveSPWebPartManager(this);
                AveWebPartCache webpartCache = manager.GetWebPartCache();
                foreach (HtmlNode node in nodes)
                {
                    WebPartBrokenLinkFixer brokenLinkFixer = WebPartBrokenLinkFixerFactory.CreateBrokenLinkFixer(this.Web, webpartCache, node);
                    if (brokenLinkFixer != null)
                    {
                        isFixed |= brokenLinkFixer.FixBrokenLinks(node);
                    }
                }
            }

            return isFixed;
        }

        private bool FixHtmlBrokenLinks(HtmlDocument htmlDoc)
        {
            var isFixed = false;
            HtmlNodeCollection hrefNode = htmlDoc.DocumentNode.SelectNodes("//a | //link");
            HtmlNodeCollection srcNode = htmlDoc.DocumentNode.SelectNodes("//img | //script");
            if (hrefNode != null || srcNode != null)
            {
                if (hrefNode != null)
                {
                    foreach (HtmlNode linkNode in hrefNode)
                    {
                        string hrefLink = linkNode.GetAttributeValue("href", string.Empty);
                        if (!string.IsNullOrEmpty(hrefLink))
                        {
                            string value = AveReplaceProcessor.UrlReplace(hrefLink, this.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl, true);
                            if (string.Equals(AveReplaceProcessor.UrlPathDecode(hrefLink), value, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                            var newUrl = HttpUtility.UrlPathEncode(value);
                            if (!hrefLink.Equals(newUrl, StringComparison.OrdinalIgnoreCase))
                            {
                                linkNode.SetAttributeValue("href", newUrl);
                                isFixed = true;
                            }
                        }
                    }
                }
                if (srcNode != null)
                {
                    foreach (HtmlNode imageNode in srcNode)
                    {
                        string srcLink = imageNode.GetAttributeValue("src", string.Empty);
                        if (!string.IsNullOrEmpty(srcLink))
                        {
                            string value = AveReplaceProcessor.UrlReplace(srcLink, this.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl, true);
                            //SAAS-23284,UrlPathEncode此方法会将url的路径部分的空格转换成"%20",特殊情况下（如：路径中含有<sharepoint..>这种语言时），无法解析这种格式，导致还原的页面无法打开。
                            if (!HttpUtility.UrlDecode(srcLink).Equals(HttpUtility.UrlDecode(value), StringComparison.OrdinalIgnoreCase))
                            {
                                imageNode.SetAttributeValue("src", HttpUtility.UrlPathEncode(value));
                                isFixed = true;
                            }
                        }
                    }
                }
            }

            return isFixed;
        }

        private Stream FixVisioFile(string fileName, Stream fileStream)
        {
            if (!string.IsNullOrEmpty(fileName)
                && (fileName.EndsWith(".vsdx", StringComparison.OrdinalIgnoreCase)))
            {
                AveVisioFileParser visioFile = new AveVisioFileParser(fileStream, mAveParentSite);
                return visioFile.FixBrokenLinks();
            }
            return fileStream;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "vti_copydestinations is a key")]
        private string ReplaceUnVersionedMetaInfo(Dictionary<Dictionary<string, string>, string> metainfodic)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.ReplaceUnVersionedMetaInfo"))
            {
#endif
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
                                            if (user != null && user is AveUserInfo)
                                            {
                                                AveUserInfo createdbyuser = user as AveUserInfo;
                                                xe.SetAttribute("CreatedBy", createdbyuser.ID.ToString());
                                            }
                                        }
                                        if (xe.HasAttribute("ModifiedBy"))
                                        {
                                            int modifiby = int.Parse(xe.GetAttribute("ModifiedBy"));
                                            IAvePrincipal user = this.mAveParentSite.SPMembers.FindMember(modifiby, true);
                                            if (user != null && user is AveUserInfo)
                                            {
                                                AveUserInfo modifibyuser = user as AveUserInfo;
                                                xe.SetAttribute("ModifiedBy", modifibyuser.ID.ToString());
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

#if PerformanceLog
            }
#endif
        }
        //private void TrimItemMetaInfo(Hashtable metaInfo)
        //{
        //    //trim holds metainfo, we will restore these properties using Hold API
        //    string[] holdKeys = new string[] { "_vti_ItemHoldRecordStatus", "ecm_ItemLockHolders", "ecm_ItemDeleteBlockHolders", "_dlc_Holds_Property", "IconOverlay" };
        //    foreach (string key in holdKeys)
        //    {
        //        if (metaInfo.ContainsKey(key))
        //        {
        //            metaInfo.Remove(key);
        //        }
        //    }
        //    if (metaInfo.ContainsKey("vti_stickycachedpluggableparserprops"))
        //    {
        //        string cachedProps = metaInfo["vti_stickycachedpluggableparserprops"] as string;
        //        if (!string.IsNullOrEmpty(cachedProps))
        //        {
        //            StringBuilder trimHoldsInfo = new StringBuilder();
        //            foreach (string prop in cachedProps.Split(' '))
        //            {
        //                if (!holdKeys.Contains(prop, StringComparer.OrdinalIgnoreCase))
        //                {
        //                    trimHoldsInfo.Append(prop).Append(' ');
        //                }
        //            }
        //            metaInfo["vti_stickycachedpluggableparserprops"] = trimHoldsInfo.ToString().TrimEnd(' ');
        //        }
        //    }
        //}

        private IAveFile GetFile()
        {
            IAveFolder folder = mAveSPFolder.SPFolder;
            string folderPath = folder.ServerRelativeUrl.TrimEnd('/') + "/" + Name;
            return folder.ParentWeb.GetFile(folderPath);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint view name")]
        public void ProcessViewInfo(Dictionary<string, object> allDocData, Dictionary<string, object> allUserData)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.ProcessViewInfo"))
            {
#endif
                mDocumentInfo.Needskip = false;
                mDocumentInfo.IsOverWrite = CheckRestoreOption(AveRestoreMode.OverWrite);
                mDocumentInfo.IsView = allDocData.ContainsKey("IsViewPage") ? (bool)allDocData["IsViewPage"] : false;
                try
                {
                    if (allDocData.ContainsKey("DoclibRowId") && Convert.ToInt32(allDocData["DoclibRowId"]) > 0)
                    {
                        mDocumentInfo.IsView = false;
                    }
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while setting isview: " + e.Message + e.StackTrace);
                }
                if (mDocumentInfo.IsView)
                {
                    int i = 0;
                    mDocumentInfo.ListViewMapping = mAveSPFolder.ParentList.ParentSite.MappingManager.ListMappingManager.ListViewMapping;
                    IAveContentTypeMapping listContentTypeMapping = mAveSPFolder.ParentList.AveContentTypes.ContentTypeMapping;
                    while (allDocData.ContainsKey("ViewID" + i))
                    {
                        try
                        {
                            AveViewInfo viewInfo = new AveViewInfo();
                            viewInfo.IsPersonal = (bool)allDocData["IsPersonal" + i];

                            viewInfo.ViewType = AveSPView.GetViewType(allDocData["ViewType" + i]);
                            viewInfo.Id = (Guid)allDocData["ViewID" + i];
                            viewInfo.Title = (string)allDocData["ViewTitle" + i];
                            viewInfo.Title = ParentSite.GetNameByLanguageMapping(viewInfo.Title, AveLanguageMappingType.ViewMapping);
                            viewInfo.LeafName = (string)allDocData["LeafName"];
                            if (allDocData.ContainsKey("IsDefaultView" + i))//DocAve 5420没有备份IsDefaultView
                            {
                                viewInfo.IsDefaultView = (bool)allDocData["IsDefaultView" + i];
                            }
                            if (allDocData.ContainsKey("ViewData" + i))
                            {
                                viewInfo.ViewData = (string)allDocData["ViewData" + i];
                            }
                            if (allDocData.ContainsKey("IsMobileView" + i))
                            {
                                viewInfo.IsMobileView = (bool)allDocData["IsMobileView" + i];
                            }
                            if (allDocData.ContainsKey("IsDefaultMobileView" + i))
                            {
                                viewInfo.IsDefaultMobileView = (bool)allDocData["IsDefaultMobileView" + i];
                            }
                            if (allDocData.ContainsKey("RowLimit" + i))
                            {
                                viewInfo.RowLimit = (uint)allDocData["RowLimit" + i];
                            }
                            viewInfo.UserID = -1;
                            if (viewInfo.IsPersonal && allDocData.ContainsKey("UserID" + i))
                            {
                                viewInfo.UserID = (int)allDocData["UserID" + i];
                                viewInfo.UserID = mAveParentSite.SPMembers.FindMemberId(viewInfo.UserID.Value);
                            }
                            byte BaseViewId = 0;
                            if (allDocData.ContainsKey("BaseViewId" + i))
                            {
                                BaseViewId = (byte)allDocData["BaseViewId" + i];
                                viewInfo.BaseViewId = BaseViewId;
                            }
                            if (allDocData.ContainsKey("Hidden" + i))
                            {
                                viewInfo.Hidden = (bool)allDocData["Hidden" + i];
                            }
                            if (allDocData.ContainsKey("Scope" + i))
                            {
                                viewInfo.Scope = allDocData["Scope" + i].ToString();
                            }
                            if (allDocData.ContainsKey("ListViewXml" + i))
                            {
                                viewInfo.ListViewXml = allDocData["ListViewXml" + i].ToString();
                                object spotlightInfoMapping;
                                if (allDocData.TryGetValue("ViewSpotlightInfoMapping", out spotlightInfoMapping) )
                                {
                                    ReplaceItemIdInViewSpotlight(viewInfo, spotlightInfoMapping.ToString());
                                }
                            }
                            if (allDocData.ContainsKey("ContentTypeId" + i))
                            {
                                var contentTypeId = allDocData["ContentTypeId" + i].ToString();
                                var mappingCTId = listContentTypeMapping.GetMappingRestoredContentTypeId(contentTypeId);
                                if (!string.IsNullOrEmpty(mappingCTId))
                                {
                                    viewInfo.ContentTypeId = mappingCTId;
                                }
                                else
                                {
                                    viewInfo.ContentTypeId = contentTypeId;
                                }
                            }
                            if (viewInfo.LeafName == "WebFldr.aspx" && viewInfo.Title == "Explorer View" && BaseViewId == 3)
                            {
                                mDocumentInfo.Needskip = true;
                                return;
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
                        mDocumentInfo.IsNewCreatedView = true;
                    }
                }
#if PerformanceLog
            }
#endif
        }

        private void ReplaceItemIdInViewSpotlight(AveViewInfo viewInfo, string spotlightInfoMappingStr)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(viewInfo.ListViewXml);
                XmlNode spotlightInfoNode = doc.SelectSingleNode("View/SpotlightInfo");
                if (spotlightInfoNode == null)
                {
                    return;
                }
                Dictionary<int, string> spotlightInfoMapping = SerializerHelper.DeserializeFromBase64String<Dictionary<int, string>>(spotlightInfoMappingStr);
                Dictionary<int, int> itemIdMapping;
                this.ParentSite.MappingManager.SiteMappingManager.ItemIdMapping.TryGetValue(this.ParentFolder.ParentList.SPList.ID, out itemIdMapping);
                // spot light format: 
                // |folderId=itemId;itemId;itemId|folderId=itemId;|
                string spotlightInfoStr = "|";
                int itemId;
                string sourceUrl;
                foreach (string spotlight in spotlightInfoNode.InnerText.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (spotlight.Contains('='))
                    {
                        string folderRowId = spotlight.Substring(0, spotlight.IndexOf("="));
                        string itemRowIds = spotlight.Substring(spotlight.IndexOf("=") + 1);
                        if (folderRowId == "0")
                        {
                            spotlightInfoStr += "0=";
                        }
                        else
                        {
                            if (itemIdMapping != null && itemIdMapping.TryGetValue(int.Parse(folderRowId), out itemId))
                            {
                                spotlightInfoStr += itemId + "=";
                            }
                            //SAAS-29933 if cannot locate the item from the itemId mapping, try to find the item from the spotlightinfo mapping,need to replace the source URL with dest URL then retireve the new item RowId by the dest URL
                            else if (spotlightInfoMapping.TryGetValue(int.Parse(folderRowId), out sourceUrl))
                            {
                                try
                                {
                                    string destUrl = AveReplaceProcessor.UrlReplace(sourceUrl.Substring(1), this.mAveParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
                                    IAveFolder tempFolder = this.mAveSPFolder.ParentList.SPList.ParentWeb.GetFolder(destUrl);
                                    if (tempFolder != null && tempFolder.Item != null)
                                    {
                                        spotlightInfoStr += tempFolder.Item.ID + "=";
                                    }
                                    else
                                    {
                                        log.Warn("Cannot find the folder ID:{0}, URL:{1} in destination side", folderRowId, destUrl);
                                        continue;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.Log(AveLogLevel.WARN, "Error occurred while replcaing folder id in spotlightinfo. SoureRowId:{0},SouceUrl:{1},Ex:{2}", folderRowId, sourceUrl, ex.ToString());
                                    continue;
                                }
                            }
                            else
                            {
                                //SAAS-29933 If the folder cannot be found neither from Idmapping or url mapping, consider it as dirty data, don't restore, otherwise, it may affect the normal data
                                //The exception case may happen if source library contains this kind of dirty data(usually generated by previous migration jobs), should not continue migrating the dirty data to destination side.
                                //spotlightInfoStr += folderRowId + "=";
                                log.Warn("ListViewXml, Cannot mapping desination folder: {0}", folderRowId);
                                continue;
                            }
                        }
                        List<int> tempList = new List<int>();
                        int sourceItemId = 0;
                        foreach (var itemRowId in itemRowIds.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            sourceItemId = int.Parse(itemRowId);
                            if (itemIdMapping != null && itemIdMapping.TryGetValue(sourceItemId, out itemId))
                            {
                                tempList.Add(itemId);
                            }
                            else if (spotlightInfoMapping.TryGetValue(int.Parse(itemRowId), out sourceUrl))
                            {
                                try
                                {
                                    string destUrl = AveReplaceProcessor.UrlReplace(sourceUrl.Substring(1), this.mAveParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl);
                                    if (sourceUrl.StartsWith("F"))
                                    {
                                        IAveFolder tempFolder = this.mAveSPFolder.ParentList.SPList.ParentWeb.GetFolder(destUrl);
                                        if (tempFolder != null && tempFolder.Item != null)
                                        {
                                            tempList.Add(tempFolder.Item.ID);
                                        }
                                    }
                                    else
                                    {
                                        IAveFile tempFile = this.mAveSPFolder.ParentList.SPList.ParentWeb.GetFile(destUrl);
                                        if (tempFile != null && tempFile.Item != null)
                                        {
                                            tempList.Add(tempFile.Item.ID);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.Log(AveLogLevel.WARN, "Error occurred while replcaing item id in spotlightinfo. SoureRowId:{0},SouceUrl:{1},Ex:{2}", itemRowId, sourceUrl, ex.ToString());
                                }
                            }
                            else
                            {
                                //Same as folder, don't restore dirty data to avoid pinning the wrong item to top
                                //tempList.Add(sourceItemId);
                                log.Warn("ListViewXml, Cannot mapping desination item: {0}", folderRowId);
                            }
                        }
                        spotlightInfoStr += string.Join(";", tempList.ToArray()) + "|";
                    }
                }
                spotlightInfoNode.InnerText = spotlightInfoStr;
                viewInfo.ListViewXml = doc.OuterXml;
            }
            catch (Exception ex)
            {
                log.Warn("Error occurred while replacing spotlightinfo in list view:{0}, ex: {1}", viewInfo.ListViewXml, ex.ToString());
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint view name")]
        public void ProcessGhostInfo(Dictionary<string, object> allDocData, Dictionary<string, object> allUserData)
        {
            #region Check the document is ghost page or not
            mDocumentInfo.DocFlag = allDocData.ContainsKey("DocFlags") ? (int)allDocData["DocFlags"] : 0;
            if (allDocData.ContainsKey("SetupPath"))
            {
                mDocumentInfo.SetupPath = (string)allDocData["SetupPath"];
            }
            if (!mDocumentInfo.HasStream && mDocumentInfo.SetupPath != null
                || (mDocumentInfo.HasStream && mDocumentInfo.SetupPath != null && (mAveParentSite.SaveBinaryForGhostPage == AveRestoreGhostPageOption.KeepPathOnly || mAveParentSite.SaveBinaryForGhostPage == AveRestoreGhostPageOption.KeepStreamAndPath)))
            {
                if (mAveSPFolder.ParentList.SPList != null)
                {
                    if (!AveDocFlags.IsMustBeUnGostedWhenUndirtiedDoc(mDocumentInfo.DocFlag) || (!AveDocFlags.IsUngostedDoc(mDocumentInfo.DocFlag)))
                    {
                        mDocumentInfo.IsGhostPage = true;
                    }
                    if (mAveSPFolder.ParentList.SPList.BaseTemplate == AveListTemplateType.WebPageLibrary
                        && mDocumentInfo.SetupPath.Equals(@"DocumentTemplates\wkpstd.aspx", StringComparison.OrdinalIgnoreCase))
                    {
                        mDocumentInfo.IsGhostPage = false;
                    }
                    if (mAveSPFolder.ParentList.SPList.BaseTemplate == AveListTemplateType.MasterPageCatalog
                        && mDocumentInfo.SetupPath.Equals(@"Features\PublishingResources\PageLayoutTemplate.aspx", StringComparison.OrdinalIgnoreCase))
                    {
                        mDocumentInfo.IsGhostPage = false;
                    }
                }
                else if (mAveSPFolder.ParentList.SPList == null)
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
                AveSPWeb parentWeb = mAveSPFolder.ParentList.ParentWeb;
                string name = mDocumentInfo.Name;
                string setupPath = mDocumentInfo.SetupPath;
                if (mAveParentSite.AveLanguageProcesser != null && (!mAveParentSite.AveLanguageProcesser.LanguageRexSame()))
                {
                    AveLanguageProcesser languageProcesser = mAveParentSite.AveLanguageProcesser;
                    mAveSPItem.ProcessGhostPageNameAndPath(parentWeb.WebSrcLanguageId, parentWeb.SPWeb.Language, ref name, ref setupPath);
                }
                else if (parentWeb.WebSrcLanguageId != parentWeb.SPWeb.Language && mAveSPFolder.ParentList.SPList != null
                        && mAveSPFolder.ParentList.SPList.BaseTemplate == AveListTemplateType.MasterPageCatalog)
                {
                    //make sure the the pagelayout in createpage.aspx in pages list is ok when language mapping is not setup
                    mAveSPItem.ProcessGhostPageNameAndPath(parentWeb.WebSrcLanguageId, parentWeb.SPWeb.Language, ref name, ref setupPath);
                }
                mDocumentInfo.Name = name;
                mDocumentInfo.SetupPath = setupPath;
                mDocumentInfo.GhostPageOption = (int)mAveParentSite.SaveBinaryForGhostPage;
            }
            #endregion
        }
        public AveRestoreResult RestoreSelf(Dictionary<string, object> allDocData, Dictionary<string, object> allUserData, List<Dictionary<string, object>> datajunction = null, List<AveWebPartBaseInfo> webParts = null, SensitivityLabelRestoreOption sensitivityLabelRestoreOption = null)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.RestoreSelf"))
            {
#endif
            AveRestoreResult result = AveRestoreResult.Normal;
            string scrFileName = Guid.NewGuid().ToString() + Path.GetExtension(mDocumentInfo.Name);
            string desFileName = Guid.NewGuid().ToString() + Path.GetExtension(mDocumentInfo.Name);
            try
            {
                dataProcessor = new SharePointDocumentDataProcessor(this);
                ProcessWebPartCondtion(webParts);
                if (!ProcessPreCondition(allDocData, allUserData, datajunction))
                {
                    return result;
                }
                if (mDocumentInfo.VerifyItemMMSColumnValue)
                {
                    if (this.ParentSite.MetadataService == null)
                    {
                        this.ParentSite.MetadataService = new AveMetadataService(this.ParentSite);
                    }
                    //保证item的MetadataColumn的term能够存在或还原成功，才能允许继续restore item
                    if (this.ParentFolder.ParentList.SPList != null && mDocumentInfo.FieldsInfo.TaxonomyFieldsInMapping != null && mDocumentInfo.FieldsInfo.TermIdMapping != null && !this.ParentSite.MetadataService.VerifyMetadataColumnValue(mDocumentInfo, this.ParentFolder.ParentList.SPList, mDocumentInfo.FieldsInfo.TaxonomyFieldsInMapping, mDocumentInfo.FieldsInfo.TermIdMapping, mAveParentSite.ObjectModelFactory))
                    {
                        log.Log(AveLogLevel.WARN, string.Format("VerifyMetadataColumnValue failed, shouldn't restore document:{0}", mDocumentInfo.Name));
                        throw new AveVerifyItemMetadataValueNotFoundException("Verify item metadata column value failed");
                    }
                }
                System.IO.Stream content = new AveSPFileStream(mAveSPItem.Receiver);

                if (sensitivityLabelRestoreOption != null && sensitivityLabelRestoreOption.method == SensitivityLabelRestoreMethod.AppProfile)
                {
                    WriteStreamToFile(content, scrFileName);
                    FileInfo scrFileInfo = new FileInfo(Path.Combine(WrapperConfiguration.TempDirectory, scrFileName));
                    FileInfo desFileInfo = new FileInfo(Path.Combine(WrapperConfiguration.TempDirectory, desFileName));
                    var response = sensitivityLabelRestoreOption.Request.RemoveSensitiveLabelAsync(scrFileInfo, desFileInfo).GetAwaiter().GetResult();

                    if (response.OperationResult == LabelOperationResult.Success)
                    {
                        if (desFileInfo.Exists)
                        {
                            content = desFileInfo.Open(FileMode.Open);
                            log.Info("OperationResult is Success and des file exists.");
                        }
                    }
                    else
                    {
                        log.Warn($"OperationResult is {response.OperationResult}.");
                    }
                }

                content = FixEmbededWebParts(mDocumentInfo.Name, content);

                content = FixVisioFile(mDocumentInfo.Name, content);

                if (IsActivitedSandboxSoltuion(allUserData))
                {
                    log.Info("The solution :{0} is activated sandbox solution", mDocumentInfo.Name);
                    RestoreActivitedSolution(allDocData, allUserData, content);
                }
                else
                {
                    if (sensitivityLabelRestoreOption != null && sensitivityLabelRestoreOption.method == SensitivityLabelRestoreMethod.ServiceAccount)
                    {

                        result = mAveSPFolder.SPFolder.DocumentSerializer.SetObjectDataWithRequest(mDocumentInfo, content, allDocData, allUserData, mHoldItems, mHTMetaInfo, sensitivityLabelRestoreOption.Request);
                    }
                    else
                    {
                        result = mAveSPFolder.SPFolder.DocumentSerializer.SetObjectData(mDocumentInfo, content, allDocData, allUserData, mHoldItems, mHTMetaInfo);
                    }
                }
                mAveSPItem.CacheMutiLookupValue();
                ProcessCabinet();
                if (mDocumentInfo.TempMasterSettings != null && !string.IsNullOrEmpty(mDocumentInfo.TempFileUrl))
                {
                    mAveSPFolder.ParentList.TempMasterSettings[mDocumentInfo.TempFileUrl] = mDocumentInfo.TempMasterSettings;
                }
            }
            catch (AveSecurityTrimingException ex)
            {
                log.Warn("You don't have permission to restore this item. {0}" + mDocumentInfo.Name, ex);
                report.AddDetail(new AveWrapperReportDto(mDocumentInfo.Name, mDocumentInfo.Name, AveReportObjectType.CreateItem, AveStatus.Skipped, "You don't have permission to restore this Item. " + ex.Message));
                result = AveRestoreResult.Failed;
                throw;
            }
            catch (AveRestoreException ex)
            {
                result = ex.Result;
            }
            finally 
            {
                try
                {
                    //Delete temp file. 
                    if (File.Exists(Path.Combine(WrapperConfiguration.TempDirectory, scrFileName)))
                    {
                        File.Delete(Path.Combine(WrapperConfiguration.TempDirectory, scrFileName));
                        log.Info($"SensitivityLabelRestoreMethod.SuccessDeleteSourceFile:{Path.Combine(WrapperConfiguration.TempDirectory, scrFileName)}");
                    }
                    if (File.Exists(Path.Combine(WrapperConfiguration.TempDirectory, desFileName)))
                    {
                        File.Delete(Path.Combine(WrapperConfiguration.TempDirectory, desFileName));
                        log.Info($"SensitivityLabelRestoreMethod.SuccessDeleteDesFile:{Path.Combine(WrapperConfiguration.TempDirectory, desFileName)}");
                    }
                }
                catch (Exception ex)
                {
                    log.Warn("Delete sensitivity label temp file {0}", ex.ToString());
                }
            }
            if (mDocumentInfo.SettingInfo.LIST_SETTING_CHANGED)
            {
                ParentFolder.ParentList.SetListSettingFlags(AveListSettingFlags.LIST_SETTING_CHANGED);
            }

            if (result > 0)
            {
                if (allUserData.ContainsKey("_vti_ItemHoldRecordStatus") && !allUserData["_vti_ItemHoldRecordStatus"].ToString().Equals("0"))
                {
                    if (SPFile != null && SPFile.Web != null && mFileHold != null)
                    {
                        mAveParentSite.AddUnRestoreFileHoldRecordInfo(SPFile.Web.ID, SPFile.ServerRelativeUrl, mFileHold);
                    }
                }
                if (mDocumentInfo.IsView)
                {
                    mAveSPFolder.ParentList.RestoreRssView = mDocumentInfo.AveView.RestoreRssView;
                }

                mAveSPFolder.ParentList.AveFields.ResetNotUpdateLookupFieldValue(mDocumentInfo.RowId);
                mAveSPFolder.ParentList.AveFields.ResetNintexFormDataFieldValue(mDocumentInfo.RowId);
                mAveSPItem.AddItemMapping(mDocumentInfo.OriginalRowId);
                if (dataProcessor != null)
                {
                    dataProcessor.RecordPostActions();
                    dataProcessor = null;
                }

                if (!mDocumentInfo.IsVersion && mDocumentInfo.OriginalLevel == 1 && mDocumentInfo.ModerationStatus == 0)
                {
                    mAveSPFolder.CurrentDocStatus.HasPreCurrentVersion = true;
                    mDocumentInfo.HasPreCurrentVersion = true;
                }
                if (mAveSPItem.SPListItem != null && mAveSPItem.ParentFolder.ParentList.SPList != null && (int)mAveSPItem.ParentFolder.ParentList.SPList.BaseTemplate == 850)
                {
                    if (mDocumentInfo.OrignialID != Guid.Empty && !mAveSPItem.ParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.WebMappingManager.PageItemSDGuidMapping.ContainsKey(mDocumentInfo.OrignialID))
                    {
                        mAveSPItem.ParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.WebMappingManager.PageItemSDGuidMapping[mDocumentInfo.OrignialID] = mAveSPItem.SPListItem.UniqueId;
                    }
                    if (mDocumentInfo.OldDocId != Guid.Empty && !mAveSPItem.ParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.WebMappingManager.PageItemONGuidMapping.ContainsKey(mDocumentInfo.OldDocId))
                    {
                        mAveSPItem.ParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.WebMappingManager.PageItemONGuidMapping[mDocumentInfo.OldDocId] = mAveSPItem.SPListItem.UniqueId;
                    }
                }
            }
            return result;
#if PerformanceLog
            }
#endif
        }

        private void WriteStreamToFile(Stream stream,string fileName)
        {
            using (FileStream localStream = new FileStream(Path.Combine(WrapperConfiguration.TempDirectory, fileName), FileMode.Create))
            {
                Int32 readLen = 0;
                //stream.Position = 0;
                var cacheBuffer = new Byte[1024 * 64];
                while ((readLen = stream.Read(cacheBuffer, 0, cacheBuffer.Length)) > 0)
                {
                    localStream.Write(cacheBuffer, 0, readLen);
                }
            }
        }

        private void ProcessCabinet()
        {
            if (this.Name.EndsWith(".xsn", StringComparison.OrdinalIgnoreCase) && this.mDocumentInfo.IsCurrentVersion)
            {
                this.ParentSite.MappingManager.SiteMappingManager.UnReplaceGuidAndUrlInfoPathCache.Add(this.SPFile.ServerRelativeUrl + "," + this.ParentFolder.ParentList.ParentWeb.SPWeb.ID);
            }
        }

        private bool IsActivitedSandboxSoltuion(Dictionary<string, object> alluserdata)
        {
            return mAveSPFolder.ParentList.SPList != null
                && mAveSPFolder.ParentList.SPList.BaseTemplate == AveListTemplateType.SolutionCatalog
                && alluserdata.ContainsKey("#SolutionStatus")
                //&& Convert.ToInt32(alluserdata["#SolutionStatus"]) == 1;
                && alluserdata["#SolutionStatus"] != null && alluserdata["#SolutionStatus"].Equals("1;1");
        }

        private AveRestoreResult RestoreActivitedSolution(Dictionary<string, object> allDocData, Dictionary<string, object> allUserData, Stream content)
        {
            AveRestoreResult result = AveRestoreResult.Normal;
            AveCoordinatedStream tempStream = new AveCoordinatedStream("RestoreActivedSolution",0, true,50);
            AveIOHelper.Copy(content, tempStream);
            tempStream.Position = 0;
            result = mAveSPFolder.SPFolder.Files.DocumentSerializer.SetObjectData(mDocumentInfo, tempStream, allDocData, allUserData, mHoldItems, mHTMetaInfo);
            if (this.AveSPItem.SPListItem != null)
            {
                tempStream.Position = 0;
                RegisterSolutionActivationDependencies(this.AveSPItem.SPListItem.ID, (Guid)allUserData["SolutionId"], tempStream);
                tempStream.ExplictlyClose();
            }
            return result;
        }

        //keep activation dependencies for sandbox solution, and we will restore it later in list post action
        private void RegisterSolutionActivationDependencies(int rowId, Guid solutionId, Stream solutionContent)
        {
            Stream manifestContent = null;
            using (CabinetExtractor cabinetExtractor = new CabinetExtractor())
            {
                manifestContent = cabinetExtractor.Extract(solutionContent, "manifest.xml");
            }
            if (manifestContent == null)
            {
                log.Warn("can't find manifest file in solution content.");
            }

            XmlDocument manifestDoc = new XmlDocument();
            manifestDoc.Load(manifestContent);

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(manifestDoc.NameTable);
            nsMgr.AddNamespace("ns", "http://schemas.microsoft.com/sharepoint/");

            IList<Guid> dependencySolutions = new List<Guid>();
            XmlNode dependenciesNode = manifestDoc.SelectSingleNode("ns:Solution/ns:ActivationDependencies", nsMgr);
            if (dependenciesNode != null)
            {
                foreach (XmlNode node in dependenciesNode.ChildNodes)
                {
                    if (node.Name.Equals("ActivationDependency", StringComparison.OrdinalIgnoreCase) && node.Attributes["SolutionId"] != null)
                    {
                        dependencySolutions.Add(new Guid(node.Attributes["SolutionId"].Value));
                    }
                }
            }
            mAveSPFolder.ParentList.RegisterSandboxSolution(rowId, solutionId, dependencySolutions);

            this.RegisterFeatureActivationDependencies(manifestDoc, nsMgr, solutionContent);
        }

        private void RegisterFeatureActivationDependencies(XmlNode manifestNode, XmlNamespaceManager msMgr, Stream solutionContent)
        {
            try
            {
                XmlNodeList featureManifests = manifestNode.SelectNodes("ns:Solution/ns:FeatureManifests/ns:FeatureManifest", msMgr);
                using (CabinetExtractor cabinetExtractor = new CabinetExtractor())
                {
                    foreach (XmlElement featureManifest in featureManifests)
                    {
                        try
                        {
                            Stream manifestContent = cabinetExtractor.Extract(solutionContent, featureManifest.GetAttribute("Location"));

                            if (manifestContent == null)
                            {
                                log.Warn("can't find feature manifest file in solution content.");
                            }

                            RegisterFeaturesManifestFile(manifestContent);
                        }
                        catch (Exception e)
                        {
                            log.Error("failed to register feature: {0} due to:{1}", featureManifest.OuterXml, e.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("failed to register feature due to: {0}", ex.ToString());
            }
        }

        private void RegisterFeaturesManifestFile(Stream manifestContent)
        {
            XmlDocument featureManifest = new XmlDocument();
            featureManifest.Load(manifestContent);

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(featureManifest.NameTable);
            nsMgr.AddNamespace("ns", "http://schemas.microsoft.com/sharepoint/");

            XmlElement featureElement = featureManifest.DocumentElement;

            XmlNodeList activationDependencies = featureManifest.SelectNodes("ns:Feature/ns:ActivationDependencies/ns:ActivationDependency", nsMgr);
            List<Guid> dependencyFeatures = new List<Guid>();
            foreach (XmlElement elment in activationDependencies)
            {
                dependencyFeatures.Add(new Guid(elment.GetAttribute("FeatureId")));
            }
            mAveSPFolder.ParentList.RegisterSandboxFeatures(new Guid(featureElement.GetAttribute("Id")), (AveFeatureScope)Enum.Parse(typeof(AveFeatureScope), featureElement.GetAttribute("Scope")), dependencyFeatures);
        }

        // TODO:Add User Mapping
        // TODO:make this an the same function in AveSPItem one function

        public bool CheckIfOnlyDoDiscard()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.CheckIfOnlyDoDiscard"))
            {
#endif
                if (RestoreOption.mAveItemRestoreOption.DISCARD_ITEM_ONLY)
                {
                    try
                    {
                        IAveFile spFile = mAveSPItem.GetFile(mDocumentInfo.Name);
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

#if PerformanceLog
            }
#endif
        }

     
        public long Size
        {
            get { return 0; }
        }

        /// <param name="needCheckDelete">当我们在进行List Pose Action来还原没有还原的ListWebPart时，不应该删除已经存在的Web Part</param>
        public void RestoreWebPart(IList webPartList, bool needCheckDelete)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPDoc.WebPart"))
            {
#endif
                AveSPWebPartManager manager = null;
                try
                {
                    if (mAveParentSite.SPContextKind != AveContextKind.ClientObjectModel)
                    {
                        if (this.mAveParentSite.MappingManager.SiteMappingManager.WebPartTypeIDMapping.Count == 0)
                        {
                            this.mAveParentSite.MappingManager.SiteMappingManager.LoadWebPartIDMapping(mAveParentSite.SPSite);
                        }
                        mAveSPItem.ReloadFile(true);
                    }
                    manager = new AveSPWebPartManager(this);
                    //关闭spFile 所在list的approve和version，避免在还原webpart时，可能多出一个version。
                    IAveList temList = null;
                    bool status = false;
                    bool enableVersioning = true;
                    bool enableModeration = true;
                    bool enableMinorVersions = true;
                    bool forceCheckOut = true;
                    AveDraftVisibilityType draftVersionVisibility = AveDraftVisibilityType.Approver;
                    if (mAveParentSite.SPContextKind != AveContextKind.ClientObjectModel)
                    {
                        if (SPFile != null)
                        {
                            try
                            {
                                if (this.SPFile.Item != null)
                                {
                                    if (this.AveSPItem != null && this.AveSPItem.ParentFolder != null && this.AveSPItem.ParentFolder.ParentList != null && this.AveSPItem.ParentFolder.ParentList.SPList != null)
                                    {
                                        temList = this.AveSPItem.ParentFolder.ParentList.SPList;
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
                    manager.Restore(webPartList, RestoreOption, needCheckDelete);
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
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.RealRestoreWebPartFailed, ex.ToString());
                    report.AddDetail(new AveWrapperReportDto("WebPart", "WebPart", AveReportObjectType.WebPart, AveStatus.Skipped, "You don't have permission to restore WebPart. " + ex.Message));
                }
                finally
                {
                    if (manager != null)
                    {
                        manager.Dispose();
                        manager = null;
                    }
                }
#if PerformanceLog
            }
#endif

        }

        public void RestoreAlert(Dictionary<string, object> data, bool isSchedAlert)
        {
            AveSPDocAlert mDocAlert = new AveSPDocAlert(this);
            mDocAlert.RestoreAlert(data, isSchedAlert);
        }






        /*public bool DestinationExist()
        {
            try
            {
                IAveFile file = mAveSPItem.GetFile(mDocumentInfo.Name);
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
        }*/

        public bool CheckIfHasSensitivityLabels()
        {
            try
            {
                IAveFile file = mAveSPItem.GetFile(mDocumentInfo.Name);
                if (file != null && file.Exists && file.Item != null)
                {
                    if (file.Item.FieldValues.ContainsKey("_IpLabelId"))
                    {
                        var slId = file.Item.FieldValues["_IpLabelId"].ToString();
                        if (!string.IsNullOrEmpty(slId))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, string.Format("Get File error. \n error message:{0}", ex));
            }
            return false;
        }

        public void UnlockSensitivityLabelEncryptedFile(string justificationText = "")
        {
            IAveFile file = mAveSPItem.GetFile(mDocumentInfo.Name);
            file.UnlockSensitivityLabelEncryptedFile("");
        }

        public void Dispose()
        {
            if(report != null)
            {
                report.Dispose();
            }
        }
    }

    public class SensitivityLabelRestoreOption
    {
        public SensitivityLabelRestoreMethod method;
        public IAveRequest Request;
    }

    public enum SensitivityLabelRestoreMethod
    {
        None = 1,
        ServiceAccount = 2,
        AppProfile = 3,
    }
}