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
using System.Threading;
using AvePoint.StorageOptimization.Schedule.Common;
using System.Xml;
using System.IO;
using Microsoft.Win32;
using AvePoint.Common;
using AvePoint.GCommon;
using System.Collections;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using AvePoint.GCommon.Contract.Server.Job.Object;
using LOGRESOURCE = Merged18NResources.Archive.Archive;
using ADDTAGRESOURCE = Merged18NResources.Archive.ResourceFileForArchiver;
using REPORTRESOURCE = Merged18NResources.Archive.ArchiveForInternationalization;
using SPDisposeCheck;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common.Office;
using AvePoint.GCommon.Contract.CommonFilter;
using System.Data;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using RAArchiverCommon;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.ExplorerSync.Cache;
using AvePoint.Wrapper.Restore;
using AvePoint.RA.Contract.RMReport;
using System.Globalization;

namespace AvePoint.RA.SharePoint.Archiver
{
    class ArchiverKeepData
    {
        #region Private Member
        private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private ScheduleConfiguration mConfig;

        private ReportInfo mKeepDataReportInfo;

        private DateTime mInitialTime = DateTime.MinValue;//用于记录mSite的生存时间

        private IAveSite mSite = null;

        private IAveWeb mWeb = null;

        private IAveList mList = null;

        private string mSiteUrl = string.Empty;

        private Guid mWebID = Guid.Empty;

        private Guid mListID = Guid.Empty;

        private Guid mRecordFeatureId = new Guid("da2e115b-07e4-49d9-bb2c-35e93bb9fca9");

        private readonly static object mLock = new object();

        #endregion

        #region Porperty
        private IAveSite Site
        {
            get
            {
                if (null == mSite)
                {
                    mLog.Info("Init site for ArchiverDeletion.mSiteUrl:{0}.", mSiteUrl);
                    mInitialTime = DateTime.Now;
                    AveObjectModelFactory factory = mConfig.aveObjectModelFactory;
                    mSite = factory.CreateSite(mSiteUrl);
                }
                else if ((string.Compare(mSite.Url, mSiteUrl, StringComparison.OrdinalIgnoreCase) != 0)
                            || mInitialTime.AddHours(23) < DateTime.Now)
                {
                    mLog.Info("Init site for ArchiverDeletion.mSiteUrl:{0}.", mSiteUrl);
                    mSite.Dispose();
                    mInitialTime = DateTime.Now;
                    AveObjectModelFactory factory = mConfig.aveObjectModelFactory;
                    mSite = factory.CreateSite(mSiteUrl);
                }
                return mSite;
            }
        }

        private IAveWeb Web
        {
            get
            {
                if (null == mWeb)
                {
                    mLog.Info("Init web for ArchiverDeletion.webGuid:{0}.", mWebID);
                    mWeb = Site.OpenWeb(mWebID);
                }
                else if (!mWeb.ID.Equals(mWebID))
                {
                    mLog.Info("Init web for ArchiverDeletion.webGuid:{0}.", mWebID);
                    mWeb.Dispose();
                    mWeb = Site.OpenWeb(mWebID);
                }
                return mWeb;
            }
        }

        private IAveList List
        {
            get
            {
                if (Guid.Empty.Equals(mListID))//如果listGuid为空，说明是systemList，则赋值为null
                {
                    mList = null;
                }
                else if (null == mList || !mListID.Equals(mList.ID))
                {
                    mLog.Info("Init list for ArchiverDeletion.listGuid:{0}.", mListID);
                    mList = Web.Lists[mListID];
                }
                return mList;
            }
        }
        private IAveORecords Record
        {
            get
            {
                IAveORecords records = AveObjectModelFactory.CreateObjectModelFactory("", null, AveContextKind.Auto).CreateRecords();
                return records;
            }
        }

        private ExplorerDao explorerDao = null;

        private ExplorerDao ExplorerDao
        {
            get
            {
                if (explorerDao == null)
                {
                    explorerDao = new ExplorerDao(true);
                }
                return explorerDao;
            }
        }
        #endregion

        #region Construct and Init

        public ArchiverKeepData(ScheduleConfiguration config)
        {
            mConfig = config;
            if (mConfig.DeletionIAveSite != null)
            {
                mSite = mConfig.DeletionIAveSite;
                mSiteUrl = mSite.Url;
                mInitialTime = DateTime.Now;
            }
            if (mConfig.DeletionIAveWeb != null)
            {
                mWeb = mConfig.DeletionIAveWeb;
                mWebID = mWeb.ID;
            }
            if (mConfig.DeletionIAveList != null)
            {
                mList = mConfig.DeletionIAveList;
                mListID = mList.ID;
            }
            mKeepDataReportInfo = new ReportInfo(mConfig);
        }

        public void Dispose()
        {
            try
            {
                //Keep Data & Deletion目前都是多线程，且Container对象都是外围传来的，直接外围Dispose
                if (mList != null)
                {
                    //mList = null;
                }
                if (mWeb != null)
                {
                    //mWeb.Dispose();
                    //mWeb = null;
                }
                if (mSite != null)
                {
                    //mSite.Dispose();
                    //mSite = null;
                }
            }
            catch (Exception e)
            {
                mLog.Info("Archiver Keep Data Dispose Error: {0}", e.ToString());
            }

        }
        #endregion

        #region Public Method
        //add for SAAS-24795
        //此方法只是add column，赋值还是需要在keepdata方法中进行
        public void CreateTagColumn(Guid listId, Guid webID, string siteUrl)
        {
            if (listId != Guid.Empty)
            {
                mSiteUrl = siteUrl;
                mWebID = webID;
                mListID = listId;
                try
                {
                    using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.CreateTagColumn"))
                    {
                        List<TagContentInfo> tagContentInfos = mConfig.currentRule.TagContentInfo;
                        System.Globalization.CultureInfo cultureInfo = List.ParentWeb.LanguageCulture;
                        ArrayList allColumn = new ArrayList();
                        string fieldSchema = string.Empty;
                        foreach (TagContentInfo info in tagContentInfos)
                        {
                            string columnName = info.ColumnName;
                            switch (info.Type)
                            {
                                case TagContentInfoType.Text:
                                    fieldSchema = $"<Field Type=\"{AveFieldType.Text.ToString()}\" DisplayName=\"{columnName}\" Name=\"{columnName}\"/>";
                                    break;
                                case TagContentInfoType.Number:
                                    fieldSchema = $"<Field Type=\"{AveFieldType.Number.ToString()}\" DisplayName=\"{columnName}\" Name=\"{columnName}\"/>";
                                    break;
                                case TagContentInfoType.DateTime:
                                    fieldSchema = $"<Field Type=\"{AveFieldType.DateTime.ToString()}\" DisplayName=\"{columnName}\" Name=\"{columnName}\"/>";
                                    break;
                                case TagContentInfoType.Boolean:
                                    fieldSchema = $"<Field Type=\"{AveFieldType.Boolean.ToString()}\" DisplayName=\"{columnName}\" Name=\"{columnName}\"><Default>0</Default></Field>";
                                    break;
                                case TagContentInfoType.Archived:
                                    columnName = ADDTAGRESOURCE.ResourceManager.GetString("StorageOptimization13_SOARAddTagArchived", cultureInfo);
                                    fieldSchema = $"<Field Type=\"{AveFieldType.Boolean.ToString()}\" DisplayName=\"{columnName}\" Name=\"{columnName}\"><Default>0</Default></Field>";
                                    break;
                                case TagContentInfoType.ArchivedBy:
                                    columnName = ADDTAGRESOURCE.ResourceManager.GetString("StorageOptimization13_SOARAddTagArchivedBy", cultureInfo);
                                    fieldSchema = $"<Field Type=\"{AveFieldType.User.ToString()}\" DisplayName=\"{columnName}\" Name=\"{columnName}\"/>";
                                    break;
                                case TagContentInfoType.ArchivedDate:
                                    columnName = ADDTAGRESOURCE.ResourceManager.GetString("StorageOptimization13_SOARAddTagArchivedTime", cultureInfo);
                                    fieldSchema = $"<Field Type=\"{AveFieldType.DateTime.ToString()}\" DisplayName=\"{columnName}\" Name=\"{columnName}\"/>";
                                    break;
                                default:
                                    throw new Exception(String.Format("The type:{0} is not supported when creating tag column in list:{1}, web:{2}, site:{3}", info.Type, listId, webID, siteUrl));
                            }
                            if (!List.Fields.ContainsField(columnName))
                            {
                                List.Fields.AddFieldAsXml(fieldSchema, true, AveAddFieldOptions.AddToAllContentTypes);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    mLog.Error("An error occurred while creating tag column, error:{0}, listId:{1}, webId:{2}, siteUrl:{3}", e, listId, webID, siteUrl);
                }
            }
        }

        //add for RevIM folder rule keepdata
        public void KeepFolderData(Guid itemID, int archiveLevel, string siteUrl, Guid webID, Guid listID, bool isVersion, string folderUrl, bool IsRepeatProcess)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.KeepFolderData"))
            {
                if (itemID.Equals(Guid.Empty))
                {
                    return;
                }
                if (IsRepeatProcess)
                {
                    mLog.Info($"the folder already was processed, will skip keep, id:{itemID}");
                    return;
                }
                mSiteUrl = siteUrl;
                mWebID = webID;
                mListID = listID;
                bool shouldReport = true;
                try
                {
                    AvePoint.GCommon.Contract.StorageOptimization.Object.Rule rule = mConfig.currentRule;
                    if (rule.PolicyLevel == PolicyLevel.Document || rule.PolicyLevel == PolicyLevel.DocumentVersion || rule.PolicyLevel == PolicyLevel.Item || rule.PolicyLevel == PolicyLevel.ItemVersion || rule.PolicyLevel == PolicyLevel.Attachment)
                    {
                        mLog.Info($"{rule.PolicyLevel} level rule will skip process folder");
                        shouldReport = false;
                        return;
                    }
                    int option = rule.KeepDataOption;
                    int keepDataStatus = option;
                    IAveListItem listItem = List.GetItemByUniqueId(itemID);
                    if (rule.KeepDataOption != (int)KeepDataOption.Keep && !isVersion)
                    {
                        if ((option & (int)KeepDataOption.TagContent) == (int)KeepDataOption.TagContent)
                        {
                            keepDataStatus = keepDataStatus | (int)KeepDataOption.TagContent;
                            CreateTagContent(listItem, rule.TagContentInfo);
                        }
                    }
                    //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.KeepData, itemID.ToString(), mConfig.JobId, rule.Id);
                    UpdateFolderDisposalDueDate(listItem.ID, listItem.UniqueId);
                    mLog.Info("Successfully keep folder item data:{0}", listItem.Name);
                }
                catch (Exception e)
                {
                    if (List.BaseTemplate == AveListTemplateType.PictureLibrary &&
                        (folderUrl.EndsWith("/_t", StringComparison.OrdinalIgnoreCase)
                        || folderUrl.EndsWith("/_w", StringComparison.OrdinalIgnoreCase)))
                    {
                        //ADO-164699 Picture Library 上传文件会自动生成相应的image，这类文件不显示在job detail里.
                        mLog.Info("Current List is Picture Library, Folder has keep data, FolderPath: {0}.", folderUrl);
                        shouldReport = false;
                        return;
                    }
                    mLog.Error("An error occurred while keeping folder item data:{0}", e.ToString());
                    //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, itemID, archiveLevel, mKeepDataReportInfo.SubJobId);
                    mKeepDataReportInfo.ExceptionTackle(e.Message, SPNodeLevel.Item.ToString());
                }
                finally
                {
                    if (shouldReport && !isVersion)
                    {
                        mKeepDataReportInfo.AddDeletionReportToUpdateItemStatus((int)CacheNodeType.Folder, "SO_Action_Keep", itemID, 0);
                    }
                }
            }
        }

        //parameter of bool isVersion add for ado - 84721
        public void KeepDocumnetData(Guid docID, int UIVersion, int archiveLevel, int level, string siteUrl, Guid webID, Guid listID, bool isVersion)
        {
            mLog.Info($"Begin KeepDocumnetData.file:{docID}, UIVersion {UIVersion}");
            bool shouldReport = true;
            mSiteUrl = siteUrl;
            mWebID = webID;
            mListID = listID;
            long fileSize = 0;
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.KeepDocumnetData"))
                {
                    string fileUrl = mKeepDataReportInfo.Url;
                    if (isVersion && fileUrl.IndexOf(':') > 0)
                    {
                        fileUrl = fileUrl.Substring(0, fileUrl.LastIndexOf(":", StringComparison.OrdinalIgnoreCase));
                    }
                    IAveFile file = Web.GetFile(docID, fileUrl);
                    #region check out logic
                    if (!file.Exists)
                    {
                        if (IsAutoCheckOutFile(file))
                        {
                            mLog.Info("File is Auto Check Out File. file Url:{0}", fileUrl);
                            mKeepDataReportInfo.SetFailedInfo("StorageOptimization_SOARRecordManagerAutoCheckOutFile");
                            mKeepDataReportInfo.Status = JobDetailsStatus.Failed;
                            //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, docID, archiveLevel, mKeepDataReportInfo.SubJobId);
                            mKeepDataReportInfo.ExceptionTackle("This file is Auto Check Out File.", SPNodeLevel.Document.ToString());
                            return;
                        }
                    }
                    if (file.CheckedOutByUser != null && !isVersion)
                    {
                        //AutoCheckOut file user same as job user
                        if (IsAutoCheckOutFile(file))
                        {
                            mLog.Info("File is Auto Check Out File. file Url:{0}", fileUrl);
                            mKeepDataReportInfo.SetFailedInfo("StorageOptimization_SOARRecordManagerAutoCheckOutFile");
                            mKeepDataReportInfo.Status = JobDetailsStatus.Failed;
                            //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, docID, archiveLevel, mKeepDataReportInfo.SubJobId);
                            mKeepDataReportInfo.ExceptionTackle("This file is Auto Check Out File.", SPNodeLevel.Document.ToString());
                            return;
                        }
                        int checkOutUserId = file.CheckedOutByUser.ID;
                        if (checkOutUserId > 0)
                        {
                            file = LoadCheckOutFile(mWeb, file.Item.UniqueId, checkOutUserId).Item.File;
                        }
                    }
                    //if (!file.Exists)
                    //{
                    //    mLog.Info("File is not exist. file Url:{0}", fileUrl);
                    //    mKeepDataReportInfo.SetFailedInfo("StorageOptimization_SOARRecordManagerFileNotExist");
                    //    mKeepDataReportInfo.Status = JobDetailsStatus.Failed;
                    //    mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, docID, archiveLevel, mKeepDataReportInfo.SubJobId);
                    //    mKeepDataReportInfo.ExceptionTackle("This file is not exist.", SPNodeLevel.Document.ToString());
                    //    return;
                    //}
                    #endregion
                    fileSize = file.Length;
                    if ((mConfig.BackgroundSettings.SkipExtentionName.Exists(f => file.Name.EndsWith(f, StringComparison.OrdinalIgnoreCase)))/* || (file.ParentFolder.Name.Equals("Forms", StringComparison.OrdinalIgnoreCase) && !file.ParentFolder.GetType().IsVisible)*/)
                    {
                        //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Archived, docID, archiveLevel, mConfig.JobId);
                        shouldReport = false;
                        return;
                    }
                    int option = mConfig.currentRule.KeepDataOption;
                    bool undeclared = false;
                    int keepDataStatus = 0;
                    keepDataStatus = option;
                    if (mConfig.currentRule.KeepDataOption != (int)KeepDataOption.Keep)
                    {
                        if ((option & (int)KeepDataOption.TagContent) == (int)KeepDataOption.TagContent)
                        {
                            keepDataStatus = keepDataStatus | (int)KeepDataOption.TagContent;
                            //add for manual declare and rule for add tag
                            if (archiveLevel == (int)SPNodeLevel.Document && (keepDataStatus & (int)KeepDataOption.DeclareRecord) == (int)KeepDataOption.DeclareRecord && mConfig.IsILMode && ArchiverCommonStaticMethod.IsBlockDeleteOnlyRecord(file.Item))
                            {
                                UndeclareItem(docID);
                                file = Web.GetFile(docID, fileUrl);
                                undeclared = true;
                            }
                            //file.ListItemAllFields获取到modified不是UTC,导致update item时，modified时间被修改，在此处对modified重新赋值
                            file.Item["Modified"] = file.TimeLastModified;
                            CreateTagContent(file.Item, mConfig.currentRule.TagContentInfo);
                        }
                        if (undeclared || (option & (int)KeepDataOption.DeclareRecord) == (int)KeepDataOption.DeclareRecord)
                        {
                            if (mConfig.IsOneDriverSite)
                            {
                                mLog.Warn($"Skip declare for onedrive data source");
                                mKeepDataReportInfo.Message = mKeepDataReportInfo.Message + I18NEntity.MultiI18nSeparator + "RM_SO_OneDriveDeclareItem_ErrorMessage";
                                mKeepDataReportInfo.Status = JMJobDetailsCombineUtil.CombineJobDetailStatus(mKeepDataReportInfo.Status, JobDetailsStatus.Skipped);
                            }
                            else
                            {
                                try
                                {
                                    ActiveInPlaceRecordManagementFeature();
                                    keepDataStatus = keepDataStatus | (int)KeepDataOption.DeclareRecord;
                                    if (!isVersion)
                                    {
                                        DeclareItem(docID, file.Item);
                                    }
                                }
                                catch (InvalidOperationException ex)
                                {
                                    mLog.Warn($"In Place Records Management Feature is not installed in this farm, cannot declare document {mKeepDataReportInfo.Url} as a record:{ex.ToString()}");
                                    mKeepDataReportInfo.ExceptionTackle(LOGRESOURCE.StorageOptimization13_SOARArchiverKeepDataInPlaceRecordsManagementFeatureNotInstalled, SPNodeLevel.Document.ToString());
                                }
                            }
                        }
                    }
                    IAveTimeZone webTimeZone = file.Item.Web.RegionalSettings.TimeZone;
                    //if (!isVersion)
                    //{
                    //    mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.KeepData, docID.ToString(), mConfig.JobId, mConfig.currentRule.Id);
                    //}
                    UpdateItemDisposalDueDate(file.Item.ID, docID);
                    mLog.Info($"Successfully keep document file:{docID}, UIVersion {UIVersion}.");
                }
            }
            catch (StorageFactoryException storExp)
            {
                mLog.Error("Error in Connect to StubDB" + storExp.ToString());
                mKeepDataReportInfo.SetFailedInfo("An error occurred while keeping document data");
                //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, docID, archiveLevel, mKeepDataReportInfo.SubJobId);
                mKeepDataReportInfo.ExceptionTackle(storExp.Message, SPNodeLevel.Document.ToString());
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while keeping document data:{0} {1}", mKeepDataReportInfo.Url, e.ToString());
                if (mConfig.IsILMode)
                {
                    //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, docID, archiveLevel, mKeepDataReportInfo.SubJobId);
                }
                if (e.InnerException != null && e.InnerException.Message.Contains("This item cannot be declared a record because it is checked out"))
                {
                    mKeepDataReportInfo.ExceptionTackle("StorageOptimization_SOARCheckOutFileCannotDeclare", SPNodeLevel.Document.ToString());
                }
                else
                {
                    mKeepDataReportInfo.ExceptionTackle(e.Message, SPNodeLevel.Document.ToString());
                }
            }
            finally
            {
                if (shouldReport)
                {
                    mKeepDataReportInfo.Size = fileSize;
                    if (mConfig.IsOneDriverSite)
                    {
                        mKeepDataReportInfo.AddDeletionReportToUpdateItemStatus(isVersion ? (int)CacheNodeType.ItemVersion : (int)CacheNodeType.Item, "RM_JS_RDM_CreateRule_Options_ArchiveAndKeep_TagContent", docID, 0);
                    }
                    else
                    {
                        mKeepDataReportInfo.AddDeletionReportToUpdateItemStatus(isVersion ? (int)CacheNodeType.ItemVersion : (int)CacheNodeType.Item, GetKeepActionKey(mConfig.currentRule.KeepDataOption), docID, 0);
                    }
                    SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(ContractConstants.ITEMSIZEFORLICENSE, mKeepDataReportInfo.Url);
                }
            }
            mLog.Info($"End KeepDocumnetData.file:{docID}, UIVersion {UIVersion}.");
        }

        public void KeepItemData(Guid itemID, int UIVersion, int archiveLevel, int level, string siteUrl, Guid webID, Guid listID, bool isVersion)
        {
            mSiteUrl = siteUrl;
            mWebID = webID;
            mListID = listID;
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.KeepItemData"))
                {
                    AvePoint.GCommon.Contract.StorageOptimization.Object.Rule rule = mConfig.currentRule;
                    int option = rule.KeepDataOption;
                    bool undeclared = false;
                    int keepDataStatus = 0;
                    IAveListItem listItem = List.GetItemByUniqueId(itemID);
                    keepDataStatus = option;
                    if (rule.KeepDataOption != (int)KeepDataOption.Keep && !isVersion)
                    {
                        if ((option & (int)KeepDataOption.TagContent) == (int)KeepDataOption.TagContent)
                        {
                            keepDataStatus = keepDataStatus | (int)KeepDataOption.TagContent;
                            if (archiveLevel == (int)SPNodeLevel.Item && (keepDataStatus & (int)KeepDataOption.DeclareRecord) == (int)KeepDataOption.DeclareRecord && mConfig.IsILMode && ArchiverCommonStaticMethod.IsBlockDeleteOnlyRecord(listItem))
                            {
                                UndeclareItem(itemID);
                                listItem = List.GetItemByUniqueId(itemID);
                                undeclared = true;
                            }
                            CreateTagContent(listItem, rule.TagContentInfo);
                        }
                        if (undeclared || (option & (int)KeepDataOption.DeclareRecord) == (int)KeepDataOption.DeclareRecord)
                        {
                            if (mConfig.IsOneDriverSite)
                            {
                                mLog.Warn($"Skip declare for onedrive data source");
                                mKeepDataReportInfo.Message = mKeepDataReportInfo.Message + I18NEntity.MultiI18nSeparator + "RM_SO_OneDriveDeclareItem_ErrorMessage";
                                mKeepDataReportInfo.Status = JMJobDetailsCombineUtil.CombineJobDetailStatus(mKeepDataReportInfo.Status, JobDetailsStatus.Skipped);
                            }
                            else
                            {
                                try
                                {
                                    ActiveInPlaceRecordManagementFeature();
                                    keepDataStatus = keepDataStatus | (int)KeepDataOption.DeclareRecord;
                                    if (!isVersion)
                                    {
                                        DeclareItem(itemID, listItem);
                                    }
                                    listItem = List.GetItemByUniqueId(itemID);
                                }
                                catch (InvalidOperationException ex)
                                {
                                    mLog.Warn("In Place Records Management Feature is not installed in this farm, cannot declare document as a record:{0}", ex.ToString());
                                    mKeepDataReportInfo.ExceptionTackle(LOGRESOURCE.StorageOptimization13_SOARArchiverKeepDataInPlaceRecordsManagementFeatureNotInstalled, SPNodeLevel.Item.ToString());
                                }
                            }
                        }
                    }
                    IAveTimeZone webTimeZone = listItem.Web.RegionalSettings.TimeZone;
                    //if (!isVersion)
                    //{
                    //    mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.KeepData, itemID.ToString(), mConfig.JobId, rule.Id);
                    //}
                    UpdateItemDisposalDueDate(listItem.ID, itemID);
                    mLog.Info("Successfully keep item data:{0}", listItem.Name);
                }
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while keeping item data:{0}", e.ToString());
                //if (mConfig.isRAMode)
                //{
                //    mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, itemID, archiveLevel, mKeepDataReportInfo.SubJobId);
                //}
                mKeepDataReportInfo.ExceptionTackle(e.Message, SPNodeLevel.Item.ToString());
            }
            finally
            {
                if (mConfig.IsOneDriverSite)
                {
                    mKeepDataReportInfo.AddDeletionReportToUpdateItemStatus(isVersion ? (int)CacheNodeType.ItemVersion : (int)CacheNodeType.Item, "RM_JS_RDM_CreateRule_Options_ArchiveAndKeep_TagContent", itemID, 0);
                }
                else
                {
                    mKeepDataReportInfo.AddDeletionReportToUpdateItemStatus(isVersion ? (int)CacheNodeType.ItemVersion : (int)CacheNodeType.Item, GetKeepActionKey(mConfig.currentRule.KeepDataOption), itemID, 0);
                }
                SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(ContractConstants.ITEMSIZEFORLICENSE, mKeepDataReportInfo.Url);
            }
        }

        public void KeepAttachmentData(Guid attID, IAveListItem listitem, int level, int archiveLevel, int uiVersion, string siteUrl, string leafName, Guid webID, Guid listID, bool isCheckOption)
        {
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.KeepAttachmentData"))
                {
                    AvePoint.GCommon.Contract.StorageOptimization.Object.Rule rule = mConfig.currentRule;
                    int option = rule.KeepDataOption;
                    //int keepDataStatus = isCheckOption ? mConfig.soArchiverQueryWorkerForDel.GetKeepDataStatus(attID) | option : option;
                    IAveTimeZone webTimeZone = listitem.Web.RegionalSettings.TimeZone;
                    //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.KeepData, attID.ToString(), mConfig.JobId, rule.Id);
                    mLog.Info("Successfully keep attachment data:{0}", listitem.Name);
                }
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while keeping attachment data:{0}", e.ToString());
                //if (mConfig.isRAMode)
                //{
                //    mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, attID, archiveLevel, mKeepDataReportInfo.SubJobId);
                //}
                mKeepDataReportInfo.ExceptionTackle(e.Message, SPNodeLevel.Attachment.ToString());
            }
            finally
            {
                mKeepDataReportInfo.AddDeletionReportToUpdateItemStatus((int)CacheNodeType.Attachment, "SO_Action_Keep", attID, 0);
            }
        }
        public void KeepAttachmentData(Guid attID, Guid itemID, int level, int archiveLevel, int uiVersion, string siteUrl, string leafName, Guid webID, Guid listID, bool isCheckOption)
        {
            bool shouldReport = true;
            mSiteUrl = siteUrl;
            mWebID = webID;
            mListID = listID;
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.KeepAttachmentData"))
                {
                    AvePoint.GCommon.Contract.StorageOptimization.Object.Rule rule = mConfig.currentRule;
                    int option = rule.KeepDataOption;
                    IAveListItem listItem = List.GetItemByUniqueId(itemID);
                    //int keepDataStatus = isCheckOption ? mConfig.soArchiverQueryWorkerForDel.GetKeepDataStatus(itemID) | option : option;
                    int keepDataStatus = option;
                    IAveTimeZone webTimeZone = listItem.Web.RegionalSettings.TimeZone;
                    //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.KeepData, attID.ToString(), mConfig.JobId, rule.Id);
                    mLog.Info("Successfully keep attachment data:{0}", leafName);
                }
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while keeping attachment data:{0}", e.ToString());
                //if (mConfig.isRAMode)
                //{
                //    mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, itemID, archiveLevel, mKeepDataReportInfo.SubJobId);
                //}
                mKeepDataReportInfo.ExceptionTackle(e.Message, SPNodeLevel.Attachment.ToString());
            }
            finally
            {
                if (shouldReport)
                {
                    mKeepDataReportInfo.AddDeletionReportToUpdateItemStatus((int)CacheNodeType.Attachment, "SO_Action_Keep", attID, 0);
                }
            }
        }
        public void SetReportInfo(string url, string mediaName, string ruleName, string subJobID, long size)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement reportInfo = doc.CreateElement("reportInfo");
            reportInfo.SetAttribute(KeyWord.URL, url);
            reportInfo.SetAttribute(KeyWord.MEDIANAME, mediaName);
            reportInfo.SetAttribute(KeyWord.RULENAME, ruleName);
            reportInfo.SetAttribute(KeyWord.SUBJOBID, subJobID);
            reportInfo.SetAttribute(KeyWord.SIZE, size.ToString());
            mKeepDataReportInfo.GetBasicInfo(reportInfo);
        }
        #endregion

        #region Private Method

        private string GetKeepActionKey(int keepDataOption)
        {
            if (mConfig.IsSupportRecordLabel &&
                ((keepDataOption & (int)KeepDataOption.Keep) == (int)KeepDataOption.Keep
                || keepDataOption == 17 || keepDataOption == 20 || keepDataOption == 21))
            {
                return "RM_JS_RDM_CreateRule_Options_TagOrLock";
            }
            return "SO_Action_Keep";
        }
        private void CreateTagContent(IAveListItem item, List<TagContentInfo> tagContentInfos)
        {
            string columnName = string.Empty;
            object value = null;
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.CreateTagContent1"))
                {
                    System.Globalization.CultureInfo cultureInfo = item.Web.LanguageCulture;
                    ArrayList allColumn = new ArrayList();
                    var noLabelTagContentInfos = tagContentInfos.Where(t => t.Type != TagContentInfoType.RetentionLabel);
                    foreach (TagContentInfo info in noLabelTagContentInfos)
                    {
                        //AveFieldType type = new AveFieldType();
                        columnName = info.ColumnName;
                        value = info.Value;
                        switch (info.Type)
                        {
                            case TagContentInfoType.Text:
                            case TagContentInfoType.Number:
                                break;
                            case TagContentInfoType.DateTime:
                                value = info.DateTime;
                                break;
                            case TagContentInfoType.Boolean:
                                if (info.Value.Equals("yes", StringComparison.OrdinalIgnoreCase))
                                {
                                    value = true;
                                }
                                else if (info.Value.Equals("no", StringComparison.OrdinalIgnoreCase))
                                {
                                    value = false;
                                }
                                else
                                {
                                    throw new Exception(string.Concat("The value of YES/NO column info is invalid,the value is:", info.Value));
                                }
                                break;
                            case TagContentInfoType.Archived:
                                //columnName = "Archived (Yes/No column)";
                                columnName = ADDTAGRESOURCE.ResourceManager.GetString("StorageOptimization13_SOARAddTagArchived", cultureInfo);
                                value = true;
                                break;
                            case TagContentInfoType.ArchivedBy:
                                //columnName = "Archived By";
                                columnName = ADDTAGRESOURCE.ResourceManager.GetString("StorageOptimization13_SOARAddTagArchivedBy", cultureInfo);
                                value = GetArchivedByUser(mConfig);
                                break;
                            case TagContentInfoType.ArchivedDate:
                                //columnName = "Archived Time";
                                columnName = ADDTAGRESOURCE.ResourceManager.GetString("StorageOptimization13_SOARAddTagArchivedTime", cultureInfo);
                                value = GetArchivedTime(mConfig).ToLocalTime();
                                break;
                            default:
                                break;
                        }
                        item[columnName] = value;
                    }
                    if (noLabelTagContentInfos.Count() > 0)
                    {
                        item.SystemUpdate();
                    }
                    mLog.Info("Document tag success.item id:{0},itme guid {1}.noLabelTagContentInfos:{2}.", item.ID, item.UniqueId, noLabelTagContentInfos.Count());
                    var tagInfo = tagContentInfos.Where(t => t.Type == TagContentInfoType.RetentionLabel).FirstOrDefault();
                    if (tagInfo != null)
                    {
                        if(mConfig.IsSupportRecordLabel && tagInfo.Option == (int)RetentionLabelOptions.GetFromGeneralSetting)
                        {
                            mLog.Info("tag label get from general setting");
                            tagInfo.Value = mConfig.GeneralRetentionLabel;
                            if (string.IsNullOrEmpty(tagInfo.Value))
                            {
                                throw new Exception("StorageOptimization_SOARRecordLabelDoesNotSetValue");
                            }
                        }
                        mLog.Info("tag label value:{0}.", tagInfo.Value);
                        AveComplianceTagInfo info = null;
                        if (mConfig.SharePointRetentionLabel == null)
                        {
                            mConfig.InitRetentionLabelCollections(item.Web.Site);
                        }
                        if (mConfig.SharePointRetentionLabel.TryGetValue(tagInfo.Value, out info))
                        {
                            if (mConfig.IsSupportRecordLabel && tagInfo.Option == (int)RetentionLabelOptions.GetFromGeneralSetting)
                            {
                                mLog.Info("check current label in general setting is record label");
                                if(!(info.BlockDelete && info.BlockEdit))
                                {
                                    mLog.Error($"Current label is not record label {tagInfo.Value}");
                                    throw new Exception("StorageOptimization_SOARCurrentLabelIsNotRecordLabel");
                                }
                            }
                            //item.SetComplianceTag(info.TagName, info.BlockDelete, info.BlockEdit, info.IsEventTag, info.SuperLock, info.UnlockedAsDefault);
                            if (mConfig.IsSupportRecordLabel && tagInfo.Option == (int)RetentionLabelOptions.GetFromGeneralSetting)
                            {
                                item.SetComplianceTag(info.TagName, true, true, false, false);
                            }
                            else
                            {
                                item.SetComplianceTagOnBulkItems(info.TagName);
                            }
                        }
                        else
                        {
                            mLog.Error($"Cannot get label : {tagInfo.Value} in current site collection.");
                            throw new Exception("StorageOptimization_SOARTagCannotGetLabelByName");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while creating tag content, exception:{0}, columnName:{1}, value:{2},url:{3}", e, columnName, value, item.Url);
                throw;
            }
        }

        private string GetArchivedByUser(ScheduleConfiguration config)
        {
            foreach (TagInfoCollection tag in config.tagInfoCollection)
            {
                if (tag.Key.Equals("ArchiveBy", StringComparison.OrdinalIgnoreCase))
                {
                    var user = Web.CurrentUser;
                    string result = user.ID.ToString() + ";#" + user.Name;
                    return result;
                }
            }
            return string.Empty;
        }

        private DateTime GetArchivedTime(ScheduleConfiguration config)
        {
            foreach (TagInfoCollection tag in config.tagInfoCollection)
            {
                if (tag.Key.Equals("ArchiveTime", StringComparison.OrdinalIgnoreCase))
                {
                    return (DateTime)tag.Value;
                }
            }
            return DateTime.MinValue;
        }

        private void UpdateFolderDisposalDueDate(int itemId, Guid itemGuid)
        {
            #region records Update ExploreDB DisposalDueDate.
            Guid recordID = ScheduleConfiguration.GetRecordId(mConfig.SiteCollectionID, itemGuid);
            if (mConfig.IsILMode && ExplorerDao.ReadById(mConfig.SiteCollectionID, recordID) != null)
            {
                using (AvePerformanceScope pc1 = new AvePerformanceScope("ArchiveBackUp.KeepFolderData.CheckDueDate"))
                {
                    try
                    {
                        GCommon.Contract.StorageOptimization.Object.Rule rs = null;
                        int ruleID = 0;
                        long dueDisposalTime = 0;
                        //int disposalAction = 0;
                        Dictionary<int, GCommon.Contract.StorageOptimization.Object.Rule> tempRule = new Dictionary<int, GCommon.Contract.StorageOptimization.Object.Rule>() { };
                        foreach (var archiverRule in mConfig.RuleCollection)
                        {
                            //if (mConfig.currentRule.Id == archiverRule.Key)
                            //{
                            //    break;
                            //}
                            tempRule.Add(ruleID++, archiverRule.Value);
                        }
                        //get new listItem check rule.
                        IAveListItem listItem = List.GetItemById(itemId);
                        RecordsCalculateDisposalDueDate ruleManagement = new RecordsCalculateDisposalDueDate(new RuleCollection() { Rules = tempRule });
                        rs = ruleManagement.CheckFolderCriteria(listItem.Folder);
                        if (rs != null && rs.Name != mConfig.currentRule.Name)
                        {
                            mLog.Info("Current item meet rule and Next Job,rule name:{0}.", rs.Name);
                            //disposalAction = rs.KeepDataOption;
                            dueDisposalTime = DueDateUtil.NextJob;//to do next
                        }
                        //else逻辑为：符合当前rule或不符合其它rule
                        else
                        {
                            rs = ruleManagement.GetDueDisposalRule(listItem, ref dueDisposalTime, rs != null ? rs.Order : -1);
                            mLog.Info("Current item not meet rule,dueDisposalTime:{0}.", dueDisposalTime);
                        }
                        if (rs == null)
                        {
                            mLog.Info("No rule matched for current folder item.");
                            return;
                        }
                        ExplorerDao.UpdateRecordDisposalDueDate(mConfig.SiteCollectionID, recordID, dueDisposalTime, rs.Id, (int)rs.PolicyLevel);
                        mLog.Info("Update RMManagedRecords table successful");
                    }
                    catch (Exception ex)
                    {
                        mLog.Info("Update RMManagedRecords table Failed.Message:{0}.", ex.ToString());
                    }
                }
            }
            #endregion
        }

        private void UpdateItemDisposalDueDate(int itemId, Guid itemGuid)
        {
            try
            {
                #region records Update ExploreDB DisposalDueDate.
                Guid recordID = ScheduleConfiguration.GetRecordId(mConfig.SiteCollectionID, itemGuid);
                if (mConfig.IsILMode && ExplorerDao.ReadById(mConfig.SiteCollectionID, recordID) != null)
                {
                    using (AvePerformanceScope pc1 = new AvePerformanceScope("SP2013ArchiveBackUp.KeepItemData.CheckDueDate"))
                    {
                        GCommon.Contract.StorageOptimization.Object.Rule rs = null;
                        int ruleID = 0;
                        long dueDisposalTime = 0;
                        //int disposalAction = 0;
                        //Dictionary<int, GCommon.Contract.StorageOptimization.Object.Rule> tempRule = new Dictionary<int, GCommon.Contract.StorageOptimization.Object.Rule>() { };
                        //foreach (var archiverRule in mConfig.RuleCollection)
                        //{
                        //    tempRule.Add(ruleID++, archiverRule.Value);
                        //}

                        //get new listItem to check rule.
                        IAveListItem listItem = List.GetItemById(itemId);
                        Guid termUniqueId = GetTermInfo(listItem, listItem.Fields);
                        RuleCollection ruleCollection = BuildRuleManagementByTerm(termUniqueId);
                        RecordsCalculateDisposalDueDate ruleManagement = new RecordsCalculateDisposalDueDate(ruleCollection);

                        if (ruleCollection == null)
                        {
                            Dictionary<int, GCommon.Contract.StorageOptimization.Object.Rule> tempRule = new Dictionary<int, GCommon.Contract.StorageOptimization.Object.Rule>() { };
                            foreach (var archiverRule in mConfig.RuleCollection)
                            {
                                tempRule.Add(ruleID++, archiverRule.Value);
                            }
                            ruleManagement = new RecordsCalculateDisposalDueDate(new RuleCollection() { Rules = tempRule });
                        }
                        rs = ruleManagement.CheckItemCriteria(itemGuid, listItem);
                        if (rs != null && rs.Name != mConfig.currentRule.Name)
                        {
                            mLog.Info("Current item meet rule and Next Job,rule name:{0}.", rs.Name);
                            //disposalAction = rs.KeepDataOption;
                            dueDisposalTime = DueDateUtil.NextJob;//to do next
                        }
                        //else逻辑为：符合当前rule或不符合其它rule
                        else
                        {
                            rs = ruleManagement.GetDueDisposalRule(listItem, ref dueDisposalTime, rs != null ? rs.Order : -1);
                            mLog.Info("Current item not meet rule,dueDisposalTime:{0}.", dueDisposalTime);
                        }
                        if (rs == null)
                        {
                            mLog.Info("No rule matched for current folder item.");
                            return;
                        }
                        ExplorerDao.UpdateRecordDisposalDueDate(mConfig.SiteCollectionID, recordID, dueDisposalTime, rs.Id, (int)rs.PolicyLevel);
                        mLog.Info("Update RMManagedRecords table successful");
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                mLog.Error("Update RMManagedRecords table Failed.Message:{0}.", ex.ToString());
                throw new Exception("StorageOptimization_Exception_FailUpdateCosmosDB");
            }
        }

        internal Guid GetTermInfo(IAveListItem item, IAveFieldCollection fields)
        {
            Guid termUniqueId = Guid.Empty;
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverKeepData.GetTermInfo"))
            {
                string bcsColumnName = ScanDataCache.Instance.SiteLevelCache.BCSColumnInternalName;
                if (string.IsNullOrWhiteSpace(bcsColumnName) && !string.IsNullOrWhiteSpace(ScanDataCache.Instance.SiteLevelCache.BCSColumnDisplayName))
                {
                    bcsColumnName = ScanDataCache.Instance.SiteLevelCache.BCSColumnDisplayName;
                }

                if (fields.ContainsField(bcsColumnName))
                {
                    var termObj = item[bcsColumnName];
                    if (termObj != null && !string.IsNullOrEmpty(termObj.ToString()))
                    {
                        var valueString = termObj.ToString().Split('|');
                        if (valueString.Length > 1)
                        {
                            termUniqueId = new Guid(valueString[1]);
                        }
                        else
                        {
                            mLog.Info($"{item.Url} invalid term format:{valueString}");
                        }
                    }
                    else
                    {
                        mLog.Info($"GetTermInfo:{item.Url} contains BCSColumnInternalName:{bcsColumnName} but column value IsNullOrEmpty.");
                        var itemTaxonomyColumns = GetItemTaxonomyColumns(item, fields);
                        if (itemTaxonomyColumns.ContainsKey(bcsColumnName))
                        {
                            var termId = itemTaxonomyColumns[bcsColumnName].ToString();
                            mLog.Info($"The term uniqueId is {termId}");
                            termUniqueId = new Guid(termId);
                        }
                    }
                }
                else
                {
                    mLog.Info($"GetTermInfo:{item.Url} does not contains BCSColumnInternalName:{ScanDataCache.Instance.SiteLevelCache.BCSColumnInternalName}.");
                }
                return termUniqueId;
            }
        }

        private RuleCollection BuildRuleManagementByTerm(Guid termUniqueId)
        {
            RuleCollection ruleCollections = null;
            RMRuleItemCollection rules = null;
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverKeepData.BuildRuleManagementByTerm"))
            {
                if (ScanDataCache.Instance.TermRuleMapping.TryGetValue(termUniqueId, out rules))
                {
                    ruleCollections = RebuldSPRules(rules);
                    if (ruleCollections.Rules.Count == 0)
                    {
                        mLog.Info($"No SP rules realted to the term {termUniqueId}");
                    }
                    
                }
                else
                {
                    mLog.Info($"BuildRuleManagementByTerm.TermRuleMapping does not contains Term UniqueId:{termUniqueId}.");
                }
                return ruleCollections;
            }
        }

        internal RuleCollection RebuldSPRules(RMRuleItemCollection rules)
        {
            RuleCollection newRuleCol = new RuleCollection();
            Dictionary<int, GCommon.Contract.StorageOptimization.Object.Rule> newRules = new Dictionary<int, GCommon.Contract.StorageOptimization.Object.Rule>();
            int reOrder = 0;
            foreach (var order in rules.CommonRules.Rules.Keys)
            {
                if (rules.CommonRules.Rules[order].PolicyLevel != PolicyLevel.None && rules.CommonRules.Rules[order].SOFilters != null && rules.CommonRules.Rules[order].SOFilters.Count > 0)
                {
                    reOrder++;
                    var rule = rules.CommonRules.Rules[order];
                    //var DAUtil = new DAUtil();
                    //DAUtil.AddMoveToFilter(rule);
                    //var newRule = ruleAssembler.ConvertToSPRule(rule);
                    newRules.Add(order, rule);
                }
            }
            newRuleCol.Rules = newRules;
            return newRuleCol;
        }

        private Hashtable GetItemTaxonomyColumns(IAveListItem item, IAveFieldCollection fields)
        {
            Hashtable columnCollectionOfInternalName = new Hashtable(StringComparer.OrdinalIgnoreCase);
            if (item != null)
            {
                foreach (var field in fields)
                {
                    try
                    {
                        string fieldTitle = field.Title.ToLower(CultureInfo.CurrentCulture);
                        switch (field.Type)
                        {
                            case AveFieldType.Invalid:
                                if (string.Equals(field.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(field.TypeAsString, "TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                                {
                                    IAveTaxonomyField taxnomyField = field as IAveTaxonomyField;

                                    string internalName = field?.StaticName;
                                    mLog.Info($"The field internalName is {internalName}");

                                    //Get Term Path Method
                                    //RECO-11440
                                    object fieldValue = null;
                                    try
                                    {
                                        fieldValue = item[field.ID];
                                    }
                                    catch (Exception ie)
                                    {
                                        mLog.Warn(ie.ToString());
                                    }
                                    if (fieldValue == null)
                                    {
                                        string textFieldName = null;
                                        //Sometimes the TaxonomyField column has no value, and its associated hidden field needs to be used to get the value.
                                        try
                                        {
                                            if (string.Equals(field.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase))
                                            {
                                                textFieldName = item.Fields.GetById((field as IAveTaxonomyField).TextField).InternalName;
                                                mLog.Info("Will get field value by TextField, textFieldName is :{0}", textFieldName);
                                                fieldValue = item[textFieldName];
                                            }
                                            else if (string.Equals(field.TypeAsString, "TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                                            {
                                                //Since Record does not fully support multi-value TaxonomyFieldType, special handling is currently skipped.
                                                mLog.Warn("Skip special handling for TaxonomyFieldTypeMulti data.");
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            mLog.Warn("get TaxonomyField column associated hidden column error: {0}", e.ToString());
                                        }
                                        if (fieldValue == null)
                                        {
                                            continue;
                                        }

                                    }
                                    if (!string.IsNullOrEmpty(internalName))
                                    {
                                        columnCollectionOfInternalName[internalName] = Trim(GetFieldTermIdValue(fieldValue));
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Error(string.Format("Get the taxnomy metadata of item error.Field Name:{0} Field.ID:{1}.Exception:{2}", field.Title, field.ID, ex));
                    }
                }
            }
            return columnCollectionOfInternalName;
        }

        private string GetFieldTermIdValue(object value)
        {
            try
            {
                if (value is Dictionary<string, object> || value.GetType().ToString() == "System.Collections.Generic.Dictionary`2[System.String,System.Object]")
                {
                    try
                    {
                        var dic = ((Dictionary<string, object>)value);
                        if (dic != null && dic.ContainsKey("TermGuid"))
                        {
                            var termId = new Guid(dic["TermGuid"].ToString());
                            return termId.ToString();
                        }
                        else
                        {
                            mLog.Warn("Current FieldTermIdValue:{0} is null or does not ContainsKey TermGuid.", value.ToString());
                            return string.Empty;
                        }
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("Get Taxnomy Filed Value by Dictionary Error, {0}", e.ToString());
                    }
                }
                else if (value is IAveTaxonomyFieldValue)
                {
                    var taxValue = value as IAveTaxonomyFieldValue;
                    var termId = new Guid(taxValue.TermGuid);
                    return termId.ToString();
                }
                else if (!(value is string))
                {
                    mLog.Info("Get Taxnomy Filed Value Error, the value is :{0}", value.ToString());
                }
            }
            catch (Exception e)
            {
                mLog.Warn("Get Taxnomy Filed Value:{0} Error:{1}.", value == null ? string.Empty : value.ToString(), e.ToString());
            }
            string stringValue = value as string;
            if (!string.IsNullOrEmpty(stringValue))
            {
                string[] values = stringValue.Split(';');
                foreach (string key in values)
                {
                    var index = key.IndexOf('|');
                    if (index == 0)
                    {
                        continue;
                    }
                    if (index < 0)
                    {
                        continue;
                    }
                    else
                    {
                        return key.Substring(index + 1);
                    }
                }
            }
            else
            {
                mLog.Warn("Current FieldTermIdValue IsNullOrEmpty.");
                return string.Empty;
            }
            return string.Empty;
        }

        private string Trim(string str, params char[] trimchars)
        {
            return string.IsNullOrEmpty(str) ? str : str.Trim(trimchars);
        }

        /*private void DeclareItem(Guid itemID, string itemName)
        {
            try
            {
                mLog.Info("Begin Declare item:{0}.", itemID);
                if (mConfig.isRAMode)
                {
                    mConfig.EnsureBlockEditAndDelete(Site);
                }
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.DeclareItem"))
                {
                    IAveListItem listItem = List.GetItemByUniqueId(itemID);
                    if (mConfig.isRAMode)
                    {
                        var isRecord = ScheduleConfiguration.CheckisRecord(listItem);
                        if (isRecord)
                        {
                            //add option to check declared records option.
                            if (ArchiverCommonStaticMethod.IsBlockDeleteOnlyRecord(listItem))
                            {
                                mLog.Info("Current status is not declared reocrd block edit and delete need declared again {0}", listItem.Url);
                                Record.UndeclareItemAsRecord(listItem);
                                Record.DeclareItemAsRecord(listItem);
                            }
                        }
                        else
                        {
                            Record.DeclareItemAsRecord(listItem);
                        }
                    }
                    else
                    {
                        if (!ScheduleConfiguration.CheckisRecord(listItem))
                        {
                            Record.DeclareItemAsRecord(listItem);
                        }
                    }
                    mLog.Info("Declare item Successfully:{0}", itemID);
                }
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while declaring item, Item Name:{0}, Error Message:{1}", itemName, e.ToString());
                throw;
            }
            finally
            {
            }
        }*/

        private void DeclareItem(Guid itemID, IAveListItem listItem)
        {
            try
            {
                mLog.Info("Begin Declare item:{0}.", itemID);
                if (mConfig.IsILMode)
                {
                    mConfig.EnsureBlockEditAndDelete(Site);
                }
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.DeclareItemByIAveListItem"))
                {
                    if (mConfig.IsILMode)
                    {
                        var isRecord = ScheduleConfiguration.CheckisRecord(listItem);
                        if (isRecord)
                        {
                            //add option to check declared records option.
                            if (ArchiverCommonStaticMethod.IsBlockDeleteOnlyRecord(listItem))
                            {
                                mLog.Info("Current status is not declared reocrd block edit and delete need declared again {0}", listItem.Url);
                                Record.UndeclareItemAsRecord(listItem);
                                Record.DeclareItemAsRecord(listItem);
                            }
                        }
                        else
                        {
                            Record.DeclareItemAsRecord(listItem);
                        }
                    }
                    else
                    {
                        if (!ScheduleConfiguration.CheckisRecord(listItem))
                        {
                            Record.DeclareItemAsRecord(listItem);
                        }
                    }
                    mLog.Info("Declare item Successfully:{0}", itemID);
                }
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while declaring item, Item Name:{0}, Error Message:{1}", listItem.Url, e.ToString());
                throw;
            }
            finally
            {
            }
        }

        private void ActiveInPlaceRecordManagementFeature()
        {
            try
            {
                lock (mLock)
                {
                    if (!mConfig.HasActiveInPlaceRecordManagementFeature)
                    {
                        mLog.Info("Begin ActiveInPlaceRecordManagementFeature.Url:{0}.", Site.Url);
                        if (Site.Features[mRecordFeatureId] == null)
                        {
                            Site.Features.Add(mRecordFeatureId, true);
                            mSite = null;
                            ArchiverCommonStaticMethod.UpdateSiteRecordDeclarationSettings(Site, ScheduleConfiguration.BlockDeleteEdit);
                            mSite = null;
                        }
                        mConfig.HasActiveInPlaceRecordManagementFeature = true;
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                mLog.Warn("In Place Records Management Feature is not installed in this farm, cannot declare document as a record:{0}", ex.ToString());
                throw;
            }
            catch (Exception ex)
            {
                mLog.Warn("Activate In Place Records Management feature error:{0}", ex.ToString());
            }
        }

        public void ActiveInPlaceRecordManagementFeature(Guid listId, Guid webID, string siteUrl)
        {
            mSiteUrl = siteUrl;
            mWebID = webID;
            mListID = listId;
            try
            {
                lock (mLock)
                {
                    mLog.Info("Begin ActiveInPlaceRecordManagementFeature.Url:{0}.", Site.Url);
                    if (Site.Features[mRecordFeatureId] == null)
                    {
                        Site.Features.Add(mRecordFeatureId, true);
                        mSite = null;
                        ArchiverCommonStaticMethod.UpdateSiteRecordDeclarationSettings(Site, ScheduleConfiguration.BlockDeleteEdit);
                        mSite = null;
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                mLog.Warn("In Place Records Management Feature is not installed in this farm, cannot declare document as a record:{0}", ex.ToString());
                throw;
            }
            catch (Exception ex)
            {
                mLog.Warn("Activate In Place Records Management feature error:{0}", ex.ToString());
            }
        }

        private void UndeclareItem(Guid itemID)
        {
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.UndeclareItem"))
                {
                    IAveListItem listItem = List.GetItemByUniqueId(itemID);

                    Record.UndeclareItemAsRecord(listItem);
                }
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while converting the record:{0}", e.ToString());
                throw;
            }
        }

        private IAveFile LoadCheckOutFile(IAveWeb web, Guid fileId, int checkOutUserId)
        {
            IAveUser user = web.SiteUsers.GetByID(checkOutUserId);
            IAveUserToken userToken = user.UserToken;
            Guid webId = web.ID;
            Guid siteId = web.Site.ID;
            IAveFile file = null;
            AveObjectModelFactory factory = mConfig.aveObjectModelFactory;
            using (IAveSite site = factory.CreateSite(siteId, userToken))
            {
                using (IAveWeb curWeb = site.OpenWeb(webId))
                {
                    file = curWeb.GetFile(fileId);
                }
            }
            return file;
        }

        private bool IsAutoCheckOutFile(IAveFile file)
        {
            IAveDocumentLibrary docList = file.ParentFolder.ParentList as IAveDocumentLibrary;
            IList<IAveCheckedOutFile> checkOutFiles = docList.CheckedOutFiles;
            bool isCheckOutFile = false;
            foreach (IAveCheckedOutFile cofile in checkOutFiles)
            {
                if (cofile.LeafName.Equals(file.Name, StringComparison.OrdinalIgnoreCase))
                {
                    isCheckOutFile = true;
                    break;
                }
            }
            return isCheckOutFile;
        }
        #endregion
    }
}
