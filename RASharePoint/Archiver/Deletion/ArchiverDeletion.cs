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



using Aspose.Pdf;
using Aspose.Words.Saving;
using AvePoint.Common;
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.TransientFault;
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.Media.Service.DomainModel;
using AvePoint.PhysicalCore.SQL;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Core.Synchronize.DbManager;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.RAPhysical.Disposal;
using AvePoint.RA.SharePoint.Archiver.Common;
using AvePoint.RA.SharePoint.Archiver.Common.ApprovalService;
using AvePoint.RA.SharePoint.Archiver.Common.Manual;
using AvePoint.RA.SharePoint.Archiver.Scan.DiscorverScan.AOSP;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common.JobExecutionProcess;
using AvePoint.RA.SharePoint.Common.JobExecutionProgress;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.Object;
using AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan;
using AvePoint.StorageOptimization.Schedule.Common;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Common.Utility;
using AvePoint.Wrapper.Common.Office;
using DataOrchestration.Tag.Sdk.Service.CloudRecords.Contract;
using DocumentFormat.OpenXml.Math;
using HSMAzureCommon;
using HSMCommon;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.ComplianceFoundation.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using RAArchiverCommon;
using RAArchiverCommon.DestructionCache;
using RAArchiverCommon.TeamsController;
using SPDisposeCheck;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml;
using static AvePoint.RA.Common.Utils.SimpleLocker;
using ADDTAGRESOURCE = Merged18NResources.Archive.ResourceFileForArchiver;
using JobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus;
using ListItemComplianceInfo = Microsoft.SharePoint.Client.ListItemComplianceInfo;
using LOGRESOURCE = Merged18NResources.Archive.Archive;
using REPORTRESOURCE = Merged18NResources.Archive.ArchiveForInternationalization;

namespace AvePoint.RA.SharePoint.Archiver
{
    [AvePoint.GCommon.Contract.CodeReview.AveCodeReview(
       "2012/2/24",
       "ruiheng.liu@AvePoint.com",
       "Dong.xie@AvePoint.com",
       new string[]
        {
            CodeReviewConstants.CHECK_LIST_ID_SOCKET_1,
            CodeReviewConstants.CHECK_LIST_ID_SECURITY_1,
            CodeReviewConstants.CHECK_LIST_ID_SECURITY_2,
            CodeReviewConstants.CHECK_LIST_ID_EH_1,
            CodeReviewConstants.CHECK_LIST_ID_EH_2,
            CodeReviewConstants.CHECK_LIST_ID_DB_1,
            CodeReviewConstants.CHECK_LIST_ID_FA_1,
            CodeReviewConstants.CHECK_LIST_ID_FA_10,
            CodeReviewConstants.CHECK_LIST_ID_STREAM_1,
            CodeReviewConstants.CHECK_LIST_ID_HC_1,
            CodeReviewConstants.CHECK_LIST_ID_HC_2,
            CodeReviewConstants.CHECK_LIST_ID_THREAD_1,
            CodeReviewConstants.CHECK_LIST_ID_THREAD_2,
        },
       "ADO-25950",
       true
       )]

    [AvePoint.GCommon.Contract.CodeReview.AveCodeReview(
       "2012/8/7",
       "ruiheng.liu@AvePoint.com",
       "yanlong.gu@AvePoint.com",
       new string[]
        {
            CodeReviewConstants.CHECK_LIST_ID_SOCKET_1,
            CodeReviewConstants.CHECK_LIST_ID_SECURITY_1,
            CodeReviewConstants.CHECK_LIST_ID_SECURITY_2,
            CodeReviewConstants.CHECK_LIST_ID_EH_1,
            CodeReviewConstants.CHECK_LIST_ID_EH_2,
            CodeReviewConstants.CHECK_LIST_ID_DB_1,
            CodeReviewConstants.CHECK_LIST_ID_FA_1,
            CodeReviewConstants.CHECK_LIST_ID_FA_10,
            CodeReviewConstants.CHECK_LIST_ID_STREAM_1,
            CodeReviewConstants.CHECK_LIST_ID_HC_1,
            CodeReviewConstants.CHECK_LIST_ID_HC_2,
            CodeReviewConstants.CHECK_LIST_ID_THREAD_1,
            CodeReviewConstants.CHECK_LIST_ID_THREAD_2,
            CodeReviewConstants.CHECK_LIST_ID_LOG_1,
            CodeReviewConstants.CHECK_LIST_ID_LOG_2,
            CodeReviewConstants.CHECK_LIST_ID_LOG_3,
            CodeReviewConstants.CHECK_LIST_ID_LOG_4,
        },
       "ADO-44684",
       true
       )]
    [AvePoint.GCommon.Contract.CodeReview.AveCodeReview(
    "2012/11/2",
    "yanlong.gu@AvePoint.com",
    "dongliang.liu@AvePoint.com",
    new string[]
            {
                CodeReviewConstants.CHECK_LIST_ID_FA_1,
                CodeReviewConstants.CHECK_LIST_ID_FA_10,
                CodeReviewConstants.CHECK_LIST_ID_LOG_1,
                CodeReviewConstants.CHECK_LIST_ID_LOG_2,
                CodeReviewConstants.CHECK_LIST_ID_LOG_3,
                CodeReviewConstants.CHECK_LIST_ID_LOG_4,
            },
    "ADO-53910",
    false
    )]
    internal class ArchiverDeletion : IDisposable
    {
        #region Private Member
        private static AveLogger mLog = AveLogger.GetInstance(typeof(ArchiverDeletion));
        private IRelativeDataArchiverService RelativeDataArchiverAction => PlatformWindsorManager.GetService<IRelativeDataArchiverService>();
        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private IRMArchiverSettingsService RMArchiverSettingsService => PlatformWindsorManager.GetService<IRMArchiverSettingsService>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private IRuleManagerService mRuleManagerService;
        public IRuleManagerService RuleManagerService
        {
            get
            {
                if (mRuleManagerService == null)
                {
                    mRuleManagerService = (IRuleManagerService)PlatformWindsorManager.GetService(typeof(IRuleManagerService));
                }
                return mRuleManagerService;
            }
        }
        private ITenantService mTenantService;
        protected ITenantService TenantService
        {
            get
            {
                if (mTenantService == null)
                {
                    mTenantService = (ITenantService)PlatformWindsorManager.GetService(typeof(ITenantService));
                }
                return mTenantService;
            }
        }
        private DeletionNode node;
        public ScheduleConfiguration mConfig;
        private ReportInfo mReportInfo;
        private bool mIsCSDTenant;
        private ArchiverKeepData keepData;
        private XmlElement mHeaderInfo = null;
        private List<string> checkForCreateTagList = new List<string>();
        private Dictionary<Guid, IAveListItem> checkForLoadList = new Dictionary<Guid, IAveListItem>();//删除attachment时，用来判断当前listitem是否已经load
        private List<Guid> checkContentTypeList = new List<Guid>();//用来记录当前list是否已经包含了ct Link a document
        private static readonly object createFolderLock = new object();//link a stub多线程创建文件夹lock
        private static readonly object attachmentCacheLock = new object();
        #region SPObject
        private DateTime mInitialTime = DateTime.MinValue;//用于记录mSite的生存时间
        private IAveSite mSite = null;
        private IAveWeb mWeb = null;
        private IAveList mList = null;
        private long versionSize = 0;
        private byte[] linkFileContent;
        #endregion
        private delegate void DeleteMethod();
        private Dictionary<char, DeleteMethod> mMethodDic = null;
        private delegate void KeepDataMethod();
        private Dictionary<char, KeepDataMethod> mMethodDicForKeepData = null;

        private List<Guid> mEnableVersionList = new List<Guid>();
        private bool mBackupDeleteLowLevelStatus = true;//用于记录是否含有备份失败的Version。以决定以上级别是否删除
        private string preCheckSiteUrl = string.Empty;
        private string preCheckWebUrl = string.Empty;
        private string activeFeatureSCUrl = string.Empty;
        private Wrapper.Backup.AveSPFolder backupStubBackupAveSPCurrentFolder = null;
        private Wrapper.Restore.AveSPFolder backupStubRestoreAveSPCurrentFolder = null;
        private bool stubHasUniqueRoleAssignments = false;
        private HSMConnector HSMConnectorInstance = null;
        private Char delimiter = (Char)0x12;
        private readonly List<PendingDocumentDeletion> pendingDocumentDeletions = new List<PendingDocumentDeletion>();
        private Microsoft.SharePoint.Client.ClientContext pendingDocumentDeleteContext;
        private Microsoft.SharePoint.Client.Web pendingDocumentDeleteWeb;
        private bool _isNonStubFileRemain; // only update one time is enough to validate if need delete SC
        //private string _firstFoundNonStubFileLocation = string.Empty;
        #endregion Private Member

        #region Property
        private IAveSite Site
        {
            get
            {
                string mSiteUrl = mHeaderInfo.GetAttribute(KeyWord.SiteUrl);
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
                Guid webGuid = new Guid(mHeaderInfo.GetAttribute(KeyWord.WebId));
                if (null == mWeb)
                {
                    mLog.Info("Init web for ArchiverDeletion.webGuid:{0}.", webGuid);
                    mWeb = Site.OpenWeb(webGuid);
                }
                else if (!mWeb.ID.Equals(webGuid))
                {
                    mLog.Info("Init web for ArchiverDeletion.webGuid:{0}.", webGuid);
                    mWeb.Dispose();
                    mWeb = Site.OpenWeb(webGuid);
                }
                return mWeb;
            }
        }

        private IAveList List
        {
            get
            {
                Guid listGuid = new Guid(mHeaderInfo.GetAttribute(KeyWord.ListId));
                if (Guid.Empty.Equals(listGuid))//如果listGuid为空，说明是systemList，则赋值为null
                {
                    if (mList != null)
                    {
                        StartEnableVersionList();
                    }
                    mLog.Info($"Current item is in systemList or not in any list. Url: {mReportInfo.Url}");
                    mList = null;
                }
                else if (null == mList || !listGuid.Equals(mList.ID))
                {
                    if (mList != null)
                    {
                        StartEnableVersionList();
                    }
                    mLog.Info("Init list for ArchiverDeletion.listGuid:{0}.", listGuid);
                    mList = Web.Lists[listGuid];
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

        private List<string> designLists = new List<string>();

        #endregion

        #region Construct and Init
        //多线程Deletion每个线程都会实例化构造方法，需要注意变量赋值
        public ArchiverDeletion(ScheduleConfiguration config)
        {
            mConfig = config;
            if (mConfig.DeletionIAveSite != null)
            {
                mSite = mConfig.DeletionIAveSite;
                mInitialTime = DateTime.Now;
            }
            if (mConfig.DeletionIAveWeb != null)
            {
                mWeb = mConfig.DeletionIAveWeb;
            }
            if (mConfig.DeletionIAveList != null)
            {
                mList = mConfig.DeletionIAveList;
            }
            if (backupStubBackupAveSPCurrentFolder == null)
            {
                backupStubBackupAveSPCurrentFolder = mConfig.StubBackupAveSPRootFolder;
            }
            designLists = WebUtil.GetDesignLists(TenantService.IsCSDTenant());
            backupStubRestoreAveSPCurrentFolder = mConfig.StubRestoreAveSPRootFolder;
            mReportInfo = new ReportInfo(mConfig);
            keepData = new ArchiverKeepData(mConfig);
            mIsCSDTenant = TenantService.IsCSDTenant();
            InitKeepDataMethodDic();
            InitDeleteMethodDic();
        }
        private HSMConnector HSMConnector
        {
            get
            {
                if (HSMConnectorInstance == null)
                {
                    HSMConnectorInstance = HSMConnector.GetInstance(mConfig);
                }
                return HSMConnectorInstance;
            }
        }

        private void InitDeleteMethodDic()
        {
            mMethodDic = new Dictionary<char, DeleteMethod>();
            mMethodDic.Add(AveConstants.TYPE_SITE, DeleteSite);
            mMethodDic.Add(AveConstants.TYPE_WEB, DeleteWeb);
            mMethodDic.Add(AveConstants.TYPE_LIST, DeleteList);
            mMethodDic.Add(AveConstants.TYPE_FOLDER, DeleteFolder);//add for RevIM folder rule
            mMethodDic.Add(AveConstants.TYPE_LISTITEM, DeleteListItem);
            mMethodDic.Add(AveConstants.TYPE_LISTITEMVERSION, DeleteListItemVersion);
            mMethodDic.Add(AveConstants.TYPE_DOCUMENT, DeleteDocument);
            mMethodDic.Add(AveConstants.TYPE_VERSION, DeleteDocumentVersion);
            mMethodDic.Add(AveConstants.TYPE_ATTACHMENTS, DeleteAttachment);
        }

        private void InitKeepDataMethodDic()
        {
            mMethodDicForKeepData = new Dictionary<char, KeepDataMethod>();
            mMethodDicForKeepData.Add(AveConstants.TYPE_FOLDER, KeepFolderData);//add for RevIM folder rule keepdata
            mMethodDicForKeepData.Add(AveConstants.TYPE_DOCUMENT, KeepDocumnetData);
            mMethodDicForKeepData.Add(AveConstants.TYPE_VERSION, KeepDocumnetData);
            mMethodDicForKeepData.Add(AveConstants.TYPE_LISTITEM, KeepItemData);
            mMethodDicForKeepData.Add(AveConstants.TYPE_LISTITEMVERSION, KeepItemData);
            mMethodDicForKeepData.Add(AveConstants.TYPE_ATTACHMENTS, KeepAttachmentData);
        }
        public void Dispose()
        {
            try
            {
                FlushPendingDocumentDeletions();
                DisposePendingDocumentDeleteContext();
                //Keep Data & Deletion目前都是多线程，且Container对象都是外围传来的，直接外围Dispose
                if (keepData != null)
                {
                    //keepData.Dispose();
                    //keepData = null;
                }
                if (mList != null)
                {
                    StartEnableVersionList();
                    //here the mEnableVersionList count must be 0.
                    mLog.Info("The EnableVersionList Count is {0}", mEnableVersionList.Count);
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
                mLog.Info("Archiver Deletion Dispose Error: {0}", e.ToString());
            }
        }
        #endregion

        #region Public Method
        //add for SAAS-24795
        public void CreateTagColumn(DeletionNode node)
        {
            mHeaderInfo = node.HeaderInfo;
            string siteUrl = mHeaderInfo.GetAttribute(KeyWord.SiteUrl);
            Guid webID = new Guid(mHeaderInfo.GetAttribute(KeyWord.WebId));
            Guid listID = new Guid(mHeaderInfo.GetAttribute(KeyWord.ListId));
            string value = String.Concat(listID.ToString(), "|", webID.ToString(), "|", siteUrl);
            if (!checkForCreateTagList.Contains(value))
            {
                mLog.Info("Add Column to List.SiteUrl:{0}.ListId:{1}.", siteUrl, listID);
                keepData.CreateTagColumn(listID, webID, siteUrl);
                checkForCreateTagList.Add(value);
            }
        }

        public void ActiveInPlaceRecordsFeature(DeletionNode node)
        {
            mHeaderInfo = node.HeaderInfo;
            string siteUrl = mHeaderInfo.GetAttribute(KeyWord.SiteUrl);
            Guid webID = new Guid(mHeaderInfo.GetAttribute(KeyWord.WebId));
            Guid listID = new Guid(mHeaderInfo.GetAttribute(KeyWord.ListId));
            if (!activeFeatureSCUrl.Equals(siteUrl, StringComparison.OrdinalIgnoreCase))
            {
                mLog.Info("ActiveInPlaceRecordsFeature.SiteUrl:{0}.", siteUrl);
                keepData.ActiveInPlaceRecordManagementFeature(listID, webID, siteUrl);
                activeFeatureSCUrl = siteUrl;
            }
        }

        public void CreateLinkDocumentCT(DeletionNode node)
        {
            mHeaderInfo = node.HeaderInfo;
            Guid listID = new Guid(mHeaderInfo.GetAttribute(KeyWord.ListId));
            if (!checkContentTypeList.Contains(listID) && Convert.ToBoolean(mHeaderInfo.GetAttribute(KeyWord.ISVERSION)) == false)//不包含才需要创建
            {
                Guid docId = new Guid(mHeaderInfo.Attributes[KeyWord.ID].Value);
                string realFileUrl = node.FullPath;
                IAveFile file = Web.GetFile(docId, realFileUrl);
                IAveContentType findLinkContentType = file.Item.ParentList.ContentTypes["Link to a Document"];//获取parentList的ct
                IAveContentType linkContentTypeFromWeb = Site.RootWeb.ContentTypes["Link to a Document"];//获取rootWeb的ct
                try
                {
                    if (findLinkContentType == null)
                    {
                        if (!file.ParentFolder.ParentList.ContentTypesEnabled)
                        {
                            mLog.Info("Need update allow content type.");
                            file.ParentFolder.ParentList.ContentTypesEnabled = true;
                        }
                        if (linkContentTypeFromWeb != null)
                        {
                            //rename content name，通过ID Find,如果目的端有多个同类型不同名的CT，默认取出哪个还原哪个。 REC-1778
                            IEnumerable<IAveContentType> ctCollection = file.Item.ParentList.ContentTypes.Where(ct => ct.Parent.ID.ToString() == linkContentTypeFromWeb.ID.ToString());
                            if (ctCollection.Count() == 0)
                            {
                                mLog.Info("Need add content type 'link to a document' from web.");
                                file.ParentFolder.ParentList.ContentTypes.AddExistingContentType(linkContentTypeFromWeb);
                                List.Update();
                                checkContentTypeList.Add(listID);//将创建过content type的listid添加到list中，下次不再创建
                            }
                        }
                    }
                    else //第一条记录进来并且list之前已经创建过，直接将listid添加到List中
                    {
                        checkContentTypeList.Add(listID);//将创建过content type的listid添加到list中，下次不再创建
                    }
                }
                catch (Exception e)
                {
                    mLog.Warn("Some error occur when create link a document content type, file:{0}, listID: {1} ,error: {2}", realFileUrl, listID, e.ToString());
                }
            }
        }
        public void HandleResponseMessage(DeletionNode node)//string msg)
        {
            try
            {
                FlushPendingDocumentDeletionsIfNeeded(node);
                this.node = node;
                mLog.Info("start to process deletion for node:{0}.", node.SPId);
                mHeaderInfo = node.HeaderInfo; //(XmlElement)xDoc.GetElementsByTagName("FileHeader")[0];
                char type = node.ObjectType;
                //add check , if  type is Document rule  skip all the version  and Deletion the Document and Version one time
                if ((mConfig.currentRule.PolicyLevel != PolicyLevel.DocumentVersion && mConfig.currentRule.PolicyLevel != PolicyLevel.ItemVersion)
                    && (type == AveConstants.TYPE_VERSION || type == AveConstants.TYPE_LISTITEMVERSION))
                {
                    mLog.Info("Current node {0} is VERSION and skip delete.", node.SPId);
                    versionSize += long.Parse(mHeaderInfo.GetAttribute(KeyWord.SIZE));
                    return;
                }
                #region Post Action for folder/list.
                if (type == AveConstants.TYPE_LIST)
                {
                    mLog.Info("Begin List Post Action.Url:{0}.", node.FullPath);
                    this.ListPostActionDeletion();
                    mLog.Info("End List Post Action.Url:{0}.", node.FullPath);
                }
                if (type == AveConstants.TYPE_FOLDER)
                {
                    mLog.Info("Begin Folder Post Action.Url:{0}.", node.FullPath);
                    this.FolderPostActionDeletion();
                    mLog.Info("End Folder Post Action.Url:{0}.", node.FullPath);
                }
                #endregion
                bool deleteWithNoBackup = mConfig.actionType == ActionType.DeleteOnly || mConfig.actionType == ActionType.ExportBeforeDelete || mConfig.actionType == ActionType.DeleteDocumentToRecyleBinOnly;
                bool isLinkToDucument = LinkFileCommon.IsLeaveStubRule(mConfig.currentRule);
                if (type == AveConstants.TYPE_DOCUMENT && isLinkToDucument && !mConfig.currentRule.IsLeaveStubRemoveMetadata && !deleteWithNoBackup)
                {
                    bool successful = true;
                    //remove stub if job not to delete original file
                    Action rollbackStubOnFailure = () =>
                    {
                        try
                        {
                            var dbNodeRollback = HSMConnector.DBForHSMStub.GetRecord(node.SPId);
                            if (dbNodeRollback != null && !string.IsNullOrEmpty(dbNodeRollback.FileUrl))
                            {
                                string stubPathRollback = dbNodeRollback.FileUrl + "." + LinkFileCommon.GetStubFileNameSuffix(mConfig);
                                try
                                {
                                    var stubFileRollback = mList.GetFileByPath(stubPathRollback);
                                    if (stubFileRollback != null)
                                    {
                                        stubFileRollback.Delete();
                                        mLog.Info($"[Rollbackstub] Deleted stub because original file not deleted. SPId:{node.SPId} Stub:{stubPathRollback}");
                                    }
                                }
                                catch (Exception ex2)
                                {
                                    mLog.Warn($"[Rollbackstub] Failed to delete stub. SPId:{node.SPId} Path:{stubPathRollback} Ex:{ex2.Message}");
                                }
                            }
                        }
                        catch (Exception ex1)
                        {
                            mLog.Warn($"[Rollbackstub] Cannot get stub record. SPId:{node.SPId} Ex:{ex1.Message}");
                        }
                    };
                    //需要先post action再skip，顺序不可修改，否则remove empty folder和特殊task等类型case不好用
                    if (mHeaderInfo.GetAttribute("DoDelete").Equals("False", StringComparison.OrdinalIgnoreCase))
                    {
                        mLog.Info("LinkDocument:Current node {0} DoDelete is False and skip delete.", node.SPId);
                        HSMConnector.DBForHSMStub.UpdateRecordStatus(node.SPId, (int)StubExportStauts.Failed);
                        rollbackStubOnFailure();
                        return;
                    }
                    else if (string.IsNullOrEmpty(mHeaderInfo.GetAttribute("DoDelete")))
                    {
                        mLog.Info("LinkDocument:Current node {0} does not have DoDelete node and skip delete.", node.SPId);
                        HSMConnector.DBForHSMStub.UpdateRecordStatus(node.SPId, (int)StubExportStauts.Failed);
                        rollbackStubOnFailure();
                        return;
                    }
                    else if (mHeaderInfo.GetAttribute("DoDelete").Equals("true", StringComparison.OrdinalIgnoreCase)
                         /*&& mConfig.currentRule != null && mConfig.currentRule.PolicyLevel == PolicyLevel.Document*/
                         && mConfig.FailedVersionFileIds.Contains(node.SPId))
                    {
                        mLog.Info("LinkDocument:Current node {0} DoDelete is true but it has version backup failed so skip delete.", node.FullPath);
                        HSMConnector.DBForHSMStub.UpdateRecordStatus(node.SPId, (int)StubExportStauts.Failed);
                        rollbackStubOnFailure();
                        JobExecutionProgressStatisticExecutor.Instance.DecreaseTotalMatchedRuleFiles();
                        return;
                    }
                    else
                    {
                        if (mHeaderInfo.Attributes[KeyWord.URL] != null && !string.IsNullOrEmpty(mHeaderInfo.Attributes[KeyWord.URL].Value))
                        {
                            string fileName = mHeaderInfo.Attributes[KeyWord.URL].Value.Substring(mHeaderInfo.Attributes[KeyWord.URL].Value.IndexOf("\\") + 1);
                            if (!WrapperConfiguration.IsSkipCheckSystemFile && mConfig.BackgroundSettings.SkipExtentionName.Exists(f => fileName.EndsWith(f, StringComparison.OrdinalIgnoreCase)))
                            {
                                successful = false;
                                rollbackStubOnFailure();
                            }
                        }

                        //判断是否备份成功，如果备份失败，则返回false，不进行删除
                        XmlElement headerExtraInfo = (XmlElement)node.Document.GetElementsByTagName("HeaderExtraAttribute")[0];
                        if (headerExtraInfo != null && !headerExtraInfo.GetAttribute("status").Equals("Complete", StringComparison.OrdinalIgnoreCase))
                        {
                            mLog.Info("Current node {0} status not equals Complete and skip delete.", node.SPId);
                            successful = false;
                            rollbackStubOnFailure();
                        }

                    }
                    using (AvePerformanceScope pc = new AvePerformanceScope("DBForHSMStub.UpdateRecordStatus"))
                    {
                        int affectedRowsCount;
                        if (successful)
                        {
                            affectedRowsCount = HSMConnector.DBForHSMStub.UpdateRecordStatusToVerified(node.SPId);
                        }
                        else
                        {
                            affectedRowsCount = HSMConnector.DBForHSMStub.UpdateRecordStatus(node.SPId, (int)StubExportStauts.Failed);
                            rollbackStubOnFailure();
                        }

                        if (affectedRowsCount == 0)
                        {
                            var dbNode = HSMConnector.DBForHSMStub.GetRecord(node.SPId);

                            if (dbNode == null)
                            {
                                mLog.Warn($"Cannot find the record in the db. SPId {node.SPId}");
                                //说明没有拼包，或者拼包失败，没有写入DB，用老逻辑兼容
                                if (mHeaderInfo.HasAttribute(KeyWord.HasUniqueRoleAssignments) && mHeaderInfo.GetAttribute(KeyWord.HasUniqueRoleAssignments).Equals("true", StringComparison.OrdinalIgnoreCase))
                                {
                                    stubHasUniqueRoleAssignments = true;
                                }
                                mReportInfo.GetBasicInfo(mHeaderInfo);
                                mMethodDic[type]();
                            }
                        }
                        else
                        {
                            try
                            {
                                var dbNode = HSMConnector.DBForHSMStub.GetRecord(node.SPId);
                                if (dbNode != null)
                                {
                                    if (dbNode.IsManifestStub)
                                    {
                                        mLog.Info($"Skip SharePoint item lookup for manifest stub deletion. SPId:{node.SPId}, RowId:{dbNode.RowID}, FileUrl:{dbNode.FileUrl}");
                                    }
                                    else
                                    {
                                        IAveListItem item = null;
                                        try
                                        {
                                            item = mList.GetItemById(dbNode.RowID);
                                        }
                                        catch (Exception e)
                                        {
                                            mLog.Warn($"fail by id get,by name get will, ex:{e}");
                                            item = mList.GetFileByPath(dbNode.FileUrl + '.' + LinkFileCommon.GetStubFileNameSuffix(mConfig));
                                        }
                                        DeleteRelatedObjectForDeleteOnlyAction(item.UniqueId, item, mConfig.currentRule.RelatedRecordOption == RelatedRecordOption.Both ? 1 : 0);
                                    }
                                }
                            }
                            catch(Exception e)
                            {
                                mLog.Error($"Fail process relative data in delete phase,error:{e}");
                            }
                        }
                    }
                }
                else
                {
                    //需要先post action再skip，顺序不可修改，否则remove empty folder和特殊task等类型case不好用
                    if (mHeaderInfo.GetAttribute("DoDelete").Equals("False", StringComparison.OrdinalIgnoreCase))
                    {
                        mLog.Info("Current node {0} DoDelete is False and skip delete.", node.SPId);
                        return;
                    }
                    else if (string.IsNullOrEmpty(mHeaderInfo.GetAttribute("DoDelete")))
                    {
                        mLog.Info("Current node {0} does not have DoDelete node and skip delete.", node.SPId);
                        return;
                    }
                    else if (mHeaderInfo.GetAttribute("DoDelete").Equals("true", StringComparison.OrdinalIgnoreCase)
                                && mHeaderInfo.HasAttribute("rowId") && mConfig.FailedObjectIds.Contains(mHeaderInfo.GetAttribute("rowId")))
                    {
                        mLog.Info($"Current node: {mHeaderInfo.GetAttribute(KeyWord.URL)}.NodeID:{mHeaderInfo.GetAttribute("LibRowId")}. DoDelete is true but it has sub level backup failed so skip delete.");
                        JobExecutionProgressStatisticExecutor.Instance.DecreaseTotalMatchedRuleFiles();
                        return;
                    }
                    else if (mHeaderInfo.GetAttribute("DoDelete").Equals("true", StringComparison.OrdinalIgnoreCase)
                         /*&& mConfig.currentRule != null && mConfig.currentRule.PolicyLevel == PolicyLevel.Document*/
                         && mConfig.FailedVersionFileIds.Contains(node.SPId))
                    {
                        mLog.Info("Current node {0} DoDelete is true but it has version backup failed so skip delete.", node.FullPath);
                        JobExecutionProgressStatisticExecutor.Instance.DecreaseTotalMatchedRuleFiles();
                        return;
                    }
                    //判断是否备份成功，如果备份失败，则返回false，不进行删除
                    XmlElement headerExtraInfo = (XmlElement)node.Document.GetElementsByTagName("HeaderExtraAttribute")[0];
                    if (headerExtraInfo != null && !headerExtraInfo.GetAttribute("status").Equals("Complete", StringComparison.OrdinalIgnoreCase))
                    {
                        mLog.Info("Current node {0} status not equals Complete and skip delete.", node.SPId);
                        if (type == AveConstants.TYPE_VERSION || type == AveConstants.TYPE_LISTITEMVERSION)
                        {
                            mBackupDeleteLowLevelStatus = false;
                        }
                        return;
                    }
                    if (mMethodDic.ContainsKey(type)
                        &&
                        (mConfig.currentRule.KeepDataOption == (int)KeepDataOption.Delete
                        || (mConfig.currentRule.KeepDataOption & (int)KeepDataOption.LinkDocument) == (int)KeepDataOption.LinkDocument
                        || (mConfig.currentRule.KeepDataOption & (int)KeepDataOption.NotBackup) == (int)KeepDataOption.NotBackup
                        || (mConfig.currentRule.KeepDataOption & (int)KeepDataOption.Archive) == (int)KeepDataOption.Archive
                        || (mConfig.currentRule.KeepDataOption & (int)KeepDataOption.ArchiveAndLeaveStub)== (int)KeepDataOption.ArchiveAndLeaveStub
                        || (mConfig.currentRule.KeepDataOption & (int)KeepDataOption.ArchiveBackupAndRemove)== (int)KeepDataOption.ArchiveBackupAndRemove
                        || (mConfig.currentRule.KeepDataOption & (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub)== (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub
                        || (mConfig.currentRule.KeepDataOption & (int)KeepDataOption.DeleteOnly) == (int)KeepDataOption.DeleteOnly
                        || (mConfig.currentRule.KeepDataOption & (int)KeepDataOption.IsEnableRemoveRetentionLabel) == (int)KeepDataOption.IsEnableRemoveRetentionLabel
                        ))
                    {
                        if (mHeaderInfo.HasAttribute(KeyWord.HasUniqueRoleAssignments) && mHeaderInfo.GetAttribute(KeyWord.HasUniqueRoleAssignments).Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            stubHasUniqueRoleAssignments = true;
                        }
                        mReportInfo.GetBasicInfo(mHeaderInfo);
                        mMethodDic[type]();
                    }
                    //keep data only(backup only, don't select tag and declare option).
                    else if (mMethodDicForKeepData.ContainsKey(type) && mConfig.currentRule.KeepDataOption == (int)KeepDataOption.Keep)
                    {
                        mLog.Info("Current rule KeepDataOption is only Keep so skip it.FullPath:{0}.KeepDataOption:{1}", node.SPId, mConfig.currentRule.KeepDataOption);
                        //if (type != AveConstants.TYPE_VERSION)//Version会随着Item的更新而更新
                        //{
                        //    mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Archived, new Guid(mHeaderInfo.Attributes[KeyWord.ID].Value), GetArchiveLevel(), mConfig.JobId);
                        //}
                        return;
                    }
                    else if (mMethodDicForKeepData.ContainsKey(type))
                    {
                        mReportInfo.GetBasicInfo(mHeaderInfo);
                        mMethodDicForKeepData[type]();
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Error(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionHandleResponseMessageerror, ex.ToString());
                //mConfig.JobReportDto.HasErrorNode = true;
            }
        }


        public void HandleResponseDocumentVersionRuleMessage(List<DeletionNode> nodes)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveDeletion.HandleResponseDocumentVersionRuleMessage"))
            {
                IAveFile currentFile = null;
                List<int> currentFileNeedDeleteVersions = new List<int>();
                foreach (var node in nodes)
                {
                    try
                    {
                        this.node = node;
                        mHeaderInfo = node.HeaderInfo;
                        mReportInfo.GetBasicInfo(mHeaderInfo);
                        int uiVersion = int.Parse(mHeaderInfo.Attributes[KeyWord.VERSION].Value);
                        Guid fileId = new Guid(mHeaderInfo.Attributes[KeyWord.ID].Value);
                        char type = node.ObjectType;
                        mLog.Info($"start to process deletion for Document Version node:{node.SPId}.Version:{uiVersion}.");
                        if (mHeaderInfo.GetAttribute("DoDelete").Equals("False", StringComparison.OrdinalIgnoreCase))
                        {
                            mLog.Info($"Current node {node.SPId}.Version:{uiVersion} DoDelete is False and skip delete.");
                            continue;
                        }
                        else if (string.IsNullOrEmpty(mHeaderInfo.GetAttribute("DoDelete")))
                        {
                            mLog.Info($"Current node {node.SPId}.Version:{uiVersion} does not have DoDelete node and skip delete.");
                            continue;
                        }
                        
                        if (type == AveConstants.TYPE_VERSION)
                        {
                            if (currentFile == null)
                            {
                                string fileUrl = mReportInfo.Url.Substring(0, mReportInfo.Url.LastIndexOf(":", StringComparison.OrdinalIgnoreCase));
                                if (mConfig.BackgroundSettings.SkipExtentionName.Exists(f => fileUrl.EndsWith(f, StringComparison.OrdinalIgnoreCase)))
                                {
                                    mLog.Warn("Can not delete this document version,because document version it may be config keep file or system file.FileUrl: {0}.", fileUrl);
                                    JobExecutionProgressStatisticExecutor.Instance.DecreaseTotalMatchedRuleFiles();
                                    continue;
                                }
                                currentFile = Web.GetFile(fileId, fileUrl);
                            }
                            currentFileNeedDeleteVersions.Add(uiVersion);
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Error(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionHandleResponseMessageerror, ex.ToString());
                        //mConfig.ProgressDto.HasErrorNode = true;
                    }
                }
                if (currentFile != null)
                {
                    List<int> failedDeleteVersionIds = new List<int>();
                    Dictionary<int, VersionInfo> needDeleteVersionIds = new Dictionary<int, VersionInfo>();
                    var fileVersions = currentFile.Versions;
                    foreach (int versionNumber in currentFileNeedDeleteVersions)
                    {
                        var version = fileVersions.Where(x => x.ID == versionNumber).FirstOrDefault();
                        if(version != null)
                        {
                            needDeleteVersionIds.Add(version.ID, new VersionInfo() { VersionLabel = version.VersionLabel, Size = version.Size });
                        }
                    }
                    if (needDeleteVersionIds.Count > 0)
                    {
                        ListItemComplianceInfo complianceInfo = null;
                        bool needRestoreComplianceTag = false;
                        try
                        {
                            GetComplianceTagIfEnableRemove(currentFile.Item, out complianceInfo);
                            if (!mConfig.currentRule.LockRecordBeforeDestroy &&
                                complianceInfo != null && complianceInfo.TagPolicyHold && !complianceInfo.TagPolicyRecord &&
                                IsRecordTypeComplianceTag(Site, complianceInfo.ComplianceTag))
                            {
                                mReportInfo.Status = JobDetailsStatus.Skipped;
                                mReportInfo.Message = "StorageOptimization_Skip_Unlock_Status_Item";
                                foreach(var versionObj in needDeleteVersionIds)
                                {
                                    mLog.Info($"skip Delete current unlock status version :{versionObj.Key}.File Id:{currentFile.UniqueId}");
                                    mReportInfo.AddDeletionVersionReport((int)CacheNodeType.ItemVersion, GetExecuteActionForJobDetail(), currentFile.ServerRelativeUrl + ":" + versionObj.Value.VersionLabel);
                                }
                                return;
                            }
                            object ItemHoldRecordStatus = null;
                            bool isRecord = ScheduleConfiguration.CheckisRecord(currentFile.Item);
                            if (isRecord && mConfig.currentRule.DeleteRecords)
                            {
                                Record.UndeclareItemAsRecord(currentFile.Item);
                                ItemHoldRecordStatus = currentFile.Item.FieldValues["_vti_ItemHoldRecordStatus"];
                                currentFile.Item.FieldValues["_vti_ItemHoldRecordStatus"] = null;
                            }
                            DeleteComplianceTagIfEnableRemove(currentFile.Item, complianceInfo, out needRestoreComplianceTag);
                            //批量删除完全在Wrapper底层Client API中做，保证只实例化一次File以及File.Versions.
                            if (mConfig.currentRule.DeleteToRecycleBin)
                            {
                                var ids = needDeleteVersionIds.Select(i => i.Key).ToList();
                                foreach (var id in ids)
                                {
                                    try
                                    {
                                        fileVersions.RecycleByID(id);
                                    }
                                    catch (Exception e)
                                    {
                                        failedDeleteVersionIds.Add(id);
                                    }
                                }
                            }
                            else
                            {
                                failedDeleteVersionIds = fileVersions.DeleteByIDs(needDeleteVersionIds.Select(i => i.Key).ToList());
                            }
                            foreach (var versionObj in needDeleteVersionIds)
                            {
                                if (failedDeleteVersionIds.Contains(versionObj.Key))
                                {
                                    continue;
                                }
                                //mReportInfo.AddDeleteOnlyVersionReport("ItemVersion", GetExecuteActionForJobDetail(), versionId.Value.VersionLabel, versionId.Value.Size);
                                SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(versionObj.Value.Size, mConfig.GetNodeFullPath(currentFile.ServerRelativeUrl + ":" + versionObj.Value.VersionLabel));
                                mReportInfo.Size = versionObj.Value.Size;
                                mReportInfo.AddDeletionVersionReport((int)CacheNodeType.ItemVersion, GetExecuteActionForJobDetail(), currentFile.ServerRelativeUrl + ":" + versionObj.Value.VersionLabel);
                            }
                            if (failedDeleteVersionIds.Count > 0)
                            {
                                //Version删除Failed后，重新Get一次最新的File，确保获取SP存在的最新Versions
                                currentFile = Web.GetFile(currentFile.UniqueId, currentFile.ServerRelativeUrl);
                                foreach (var versionId in failedDeleteVersionIds)
                                {
                                    try
                                    {
                                        //由于Version是批量删除的，一次批量删除可能失败一部分数据，需要ensure当前这批数据的version在SP中是否存在
                                        //a.不存在，证明批量删除成功，添加report即可
                                        //b.存在，证明批量删除失败，one by one删除
                                        if (currentFile.Versions.Where(v => v.ID == versionId).Count() == 0)
                                        {
                                            //mReportInfo.AddDeleteOnlyVersionReport("ItemVersion", GetExecuteActionForJobDetail(), needDeleteVersionIds[versionId].VersionLabel, needDeleteVersionIds[versionId].Size);
                                            mReportInfo.AddDeletionVersionReport((int)CacheNodeType.ItemVersion, GetExecuteActionForJobDetail(), currentFile.ServerRelativeUrl + ":" + needDeleteVersionIds[versionId].VersionLabel);
                                        }
                                        else
                                        {
                                            if (mConfig.currentRule.DeleteToRecycleBin)
                                            {
                                                currentFile.Versions.RecycleByID(versionId);
                                            }
                                            else
                                            {
                                                currentFile.Versions.DeleteByID(versionId);
                                            }
                                            mLog.Info($"Success Delete current failed version one by one:{versionId}.File Id:{currentFile.UniqueId}.");
                                            //mReportInfo.AddDeleteOnlyVersionReport("ItemVersion", GetExecuteActionForJobDetail(), needDeleteVersionIds[versionId].VersionLabel, needDeleteVersionIds[versionId].Size);
                                            mReportInfo.AddDeletionVersionReport((int)CacheNodeType.ItemVersion, GetExecuteActionForJobDetail(), currentFile.ServerRelativeUrl + ":" + needDeleteVersionIds[versionId].VersionLabel);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        mLog.Error($"Failed Delete current failed version one by one:{versionId}.File Id:{currentFile.UniqueId}.Exception:{ex.ToString()}");
                                        string errorMessage = ex.InnerException != null && ex.InnerException.Message != null ? ex.InnerException.Message : ex.Message;
                                        if (errorMessage != null && errorMessage.Contains("This item cannot be updated because it is locked as read-only"))
                                        {
                                            mReportInfo.SetFailedInfo("StorageOptimization13_SOARDeleteOfficeLockFile");
                                        }
                                        else if (errorMessage != null && errorMessage.Contains("The label that's applied to this item prevents it from being edited or deleted. Check the item's label for more details"))
                                        {
                                            mReportInfo.SetFailedInfo("StorageOptimization_SOARRecordManagerLabelDocumentDeleteFailed");
                                        }
                                        else
                                        {
                                            mReportInfo.SetFailedInfo(errorMessage);
                                        }
                                        mConfig.ProgressDto.HasErrorNode = true;
                                        //mReportInfo.AddDeleteOnlyVersionReport("ItemVersion", GetExecuteActionForJobDetail(), needDeleteVersionIds[versionId].VersionLabel, 0);
                                        mReportInfo.AddDeletionVersionReport((int)CacheNodeType.ItemVersion, GetExecuteActionForJobDetail(), currentFile.ServerRelativeUrl + ":" + needDeleteVersionIds[versionId].VersionLabel);
                                    }
                                }
                            }
                            SetComplianceTagIfEnableRemove(currentFile.Item, complianceInfo);
                            needRestoreComplianceTag = false;
                            if (isRecord && mConfig.currentRule.DeleteRecords)
                            {
                                Record.DeclareItemAsRecord(currentFile.Item);
                                currentFile.Item.FieldValues["_vti_ItemHoldRecordStatus"] = ItemHoldRecordStatus;
                            }
                        }
                        catch (Exception e)
                        {
                            if (needRestoreComplianceTag)
                            {
                                SetComplianceTagIfEnableRemove(currentFile?.Item, complianceInfo);
                            }                            
                            mLog.Error($"Have an Exception when Delete version in batches. Exception:{e}.");
                        }
                    }
                }
            }
        }


        #endregion

        #region Delete
        [SuppressMessage("FxCopCustomRules", "C100003:DoNotUseSpecificSPMethod")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Archiver Deletion")]
        private void DeleteSite()
        {
            int archiveLevel = GetArchiveLevel();
            Guid siteID = Site.ID;
            string errorDetails = string.Empty;
            if (mConfig.ArchiveJobSplitedDBInfo.IsUseSplitedDB && !mConfig.ArchiveJobSplitedDBInfo.IsLatestSplitedDB)
            {
                mLog.Info($"the site run in virtual job and not is latest virtual job, will skip delete and delete in latest virtual job, site id:{siteID}");
                return;
            }
            try
            {
                mLog.Info("Current Site:{0} Template is:{1}.DenyAddAndCustomizePages is:{2}.", Site.Url, Site.RootWeb.Template, Site.DenyAddAndCustomizePagesStatus);
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.DeleteSite"))
                {
                    if(mConfig?.currentRule?.PolicyLevel == PolicyLevel.SiteCollection && mConfig.IsTeams)
                    {
                        mLog.Warn(string.Format("Teams data source run sc level rule, sould not delete SC ,SiteCollection Url is : {0}", Site.Url));
                        mConfig.JobReportDto.HasErrorNode = true;
                        mReportInfo.Status = JobDetailsStatus.Failed;
                        mReportInfo.Message = "StorageOptimization_SOTeamsSourceScLevelRuleArchiverDeleteSiteError";
                        RevertDenyAddAndCustomizePagesStatus();
                        return;
                    }
                    if (Site.HasHolds)
                    {
                        mLog.Warn("StorageOptimization_SOARArchiverDeletionDeleteHoldSiteWeb");
                        mReportInfo.SetFailedInfo("StorageOptimization_SOARArchiverDeletionDeleteHoldSiteWeb");
                        RevertDenyAddAndCustomizePagesStatus();
                        if (RMRemoteNodeDao.CheckIsOrphanedOD(siteID.ToString()))
                        {
                            RMArchiverSettingsService.DisableSCArchiverManageMent(siteID);
                            mLog.Warn("StorageOptimization_SOARArchiverDeletionDisableOrphanedODArchiveManagement");
                            mReportInfo.SetFailedInfo("StorageOptimization_SOARArchiverDeletionDisableOrphanedODArchiveManagement");
                        }
                        return;
                    }
                    if ((mConfig.currentRule.KeepDataOption & (int)KeepDataOption.NotBackup) != (int)KeepDataOption.NotBackup)
                    {
                        mLog.Info("Check all list allow delete");
                        if (!ChecklistAllowDeletion(Site.RootWeb.Lists, ref errorDetails) && !CheckOnlyStubFileRemain4SC())
                        {
                            mLog.Warn("Delete Site Failed , Site have some list can not delete . Site Url: {0}", Site.Url);
                            mReportInfo.SetFailedInfo(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteSiteList, errorDetails);  //SAAS-13414 站点无法删除时添加错误信息。
                            mConfig.JobReportDto.HasErrorNode = true;
                            RevertDenyAddAndCustomizePagesStatus();
                            //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, siteID, archiveLevel, mReportInfo.SubJobId);
                            return;
                        }

                        mLog.Info("Check all web allow delete");
                        if (!CheckWebAllowDelete(Site) && !CheckOnlyStubFileRemain4SC())
                        {
                            mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteSiteWeb);
                            mReportInfo.SetFailedInfo(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteSiteWeb);
                            RevertDenyAddAndCustomizePagesStatus();
                            //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, siteID, archiveLevel, mReportInfo.SubJobId);
                            return;
                        }

                        mLog.Info("Check all list allow delete and all web allow delete success.");
                    }
                    if (mConfig.JobReportDto.HasErrorNode)
                    {
                        mLog.Info($"Skip to delete site because there are error nodes.");
                        mReportInfo.Status = JobDetailsStatus.Skipped;
                        RevertDenyAddAndCustomizePagesStatus();
                        return;
                    }

                    string siteUrl = Site.Url.ToString();
                    char[] specialCharacter = { '&', '^' };
                    int siteUrlContainsSpecial = siteUrl.IndexOfAny(specialCharacter);
                    var siteRecord = GetSiteRecord(Site);
                    bool isForceFitTeamsLevelRule = !string.IsNullOrEmpty(mConfig.ForceFitTeamsRuleID);
                    if (isForceFitTeamsLevelRule)
                    {
                        SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(mReportInfo.Size, mReportInfo.Url);
                        if (Site.RootWeb.WebTemplate.Equals("TEAMCHANNEL", StringComparison.OrdinalIgnoreCase))
                        {
                            mLog.Info($"The channel site is disposal successful: {siteUrl}");
                            TeamsDisposalState.AddDisposalSuccessfulChannelSite(siteUrl);
                            return;
                        }
                        else if (Site.RootWeb.WebTemplate.Equals("GROUP", StringComparison.OrdinalIgnoreCase))
                        {
                            mLog.Info($"The group site is disposal successful: {siteUrl}");
                            TeamsDisposalState.IsGroupSiteDisposalSuccessful = true;
                            return;
                        }
                        else
                        {
                            mLog.Warn("SiteCollection WebTemplate is not TEAMCHANNEL or GROUP , Do not Delete ,SiteCollection Url is : " + siteUrl);
                            mConfig.JobReportDto.HasErrorNode = true;
                            //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, siteID, archiveLevel, mReportInfo.SubJobId);
                            mReportInfo.Status = JobDetailsStatus.Failed;
                            mReportInfo.Message = "StorageOptimization_SOARArchiverDeleteGroupSiteError";
                            RevertDenyAddAndCustomizePagesStatus();
                            return;
                        }
                    }
                    else if (Site.RootWeb.WebTemplate.Equals("GROUP", StringComparison.OrdinalIgnoreCase))
                    {
                        mLog.Warn(string.Format("SiteCollection WebTemplate is GROUP , Do not Delete ,SiteCollection Url is : {0}", siteUrl));
                        mConfig.JobReportDto.HasErrorNode = true;
                        //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, siteID, archiveLevel, mReportInfo.SubJobId);
                        mReportInfo.Status = JobDetailsStatus.Failed;
                        mReportInfo.Message = "StorageOptimization_SOARArchiverDeleteGroupSiteError";
                        RevertDenyAddAndCustomizePagesStatus();
                        return;
                    }
                    else if (Site.RootWeb.WebTemplate.Equals("TEAMCHANNEL", StringComparison.OrdinalIgnoreCase))
                    {
                        mLog.Warn($"SiteCollection WebTemplate is: {Site.RootWeb.WebTemplate} (Private/Shared Channel Site), Do not Delete, SiteCollection Url is: {siteUrl}");
                        mConfig.JobReportDto.HasErrorNode = true;
                        mReportInfo.Status = JobDetailsStatus.Failed;
                        mReportInfo.Message = "StorageOptimization_SOTeamsSourceScLevelRuleArchiverDeleteSiteError";
                        RevertDenyAddAndCustomizePagesStatus();
                        return;
                    }
                    else if (siteUrlContainsSpecial >= 0)
                    {
                        mLog.Warn(string.Format("SiteCollection URL contains special character , Do not Delete ,SiteCollection Url is : {0}", siteUrl));
                        mConfig.JobReportDto.HasErrorNode = true;
                        //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, siteID, archiveLevel, mReportInfo.SubJobId);
                        mReportInfo.Status = JobDetailsStatus.Failed;
                        mReportInfo.Message = "StorageOptimization_SOARArchiverDeletionDeleteSiteHaveSpecialCharacter";
                        RevertDenyAddAndCustomizePagesStatus();
                        return;
                    }
                    else
                    {

                        string mAdminUrl = AveUrlUtility.GetSPOAdminUrlBySiteUrl(mConfig.aveObjectModelFactory.AccountInfo, siteUrl);
                        mLog.Info("O365 Admin Url is : {0}.", mAdminUrl);
                        IAveTenant aveTenant = null;
                        AveRetryPolicy.DefaultProgressive.ExecuteAction(() =>
                        {
                            aveTenant = mConfig.aveObjectModelFactory.CreateTenant(mAdminUrl);
                        });
                        var geoLocationInfo = aveTenant?.GetTenantGeoLocationinfo();
                        if (geoLocationInfo != null && geoLocationInfo.Count > 1)
                        {
                            foreach (var location in geoLocationInfo)
                            {
                                if (siteUrl.StartsWith(location.RootSiteUrl) || siteUrl.StartsWith(location.MySiteHostUrl))
                                {
                                    mAdminUrl = location.TenantAdminUrl;
                                    mLog.Info($"GetTenantGeoLocationinfo.O365 Admin New Url is : {mAdminUrl}.SiteUrl:{siteUrl}.");
                                    AveRetryPolicy.DefaultProgressive.ExecuteAction(() =>
                                    {
                                        aveTenant = mConfig.aveObjectModelFactory.CreateTenant(mAdminUrl);
                                    });
                                }
                            }
                        }

                        Site?.DeleteSCTermGroup();
                        //aveTenant?.DeleteSite(siteUrl);

                        aveTenant.RemoveSite(siteUrl);
                        mLog.Info($"Delete site to RecycleBin success.");
                        try
                        {
                            if (mConfig.NeedDeleteSCPermanently() && IsSiteExistInRecycleBin(aveTenant, siteUrl))
                            {
                                aveTenant.RemoveDeletedSite(siteUrl);
                                mLog.Info($"Remove site from RecycleBin success.");
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Error($"Remove from RecycleBin site {siteUrl} Error {e}");
                            throw;
                        }
                        RMKeyValueDao.DeleteByKey(RMSynchronizeDbManager.LastSyncTimeKey);
                        SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(mReportInfo.Size, mReportInfo.Url);
                        ArchiverJobManagementService archiverJobManagementService = new ArchiverJobManagementService();
                        new AveTaskRetryHelper(5, true).ExecuteWithRetryMechanism(() =>
                        {
                            //add retry logic due to AOS API not stable.
                            archiverJobManagementService.UpdateSiteCollectionAfterAchiveredAsync(siteUrl, true, mConfig.TenantGroupId, mConfig.JobId).Wait();

                            try
                            {
                                RecordsDBOperation.DeleteSiteInRecords(siteUrl);
                            }
                            catch (Exception e)
                            {
                                mLog.Warn("An error occurred while deleting site in records. Error:{0}", e.ToString());
                            }

                        });
                    }

                    UpdateExploreDB(siteID, 2, siteRecord);
                    //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Archived, siteID, archiveLevel, mReportInfo.SubJobId);
                    mLog.Info(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteSitesucceed, siteUrl);
                }
            }
            catch (Exception ex)
            {
                RevertDenyAddAndCustomizePagesStatus();
                mLog.Error(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteSite, ex.ToString());
                if (ex?.InnerException?.GetType() == typeof(Microsoft.SharePoint.Client.ServerUnauthorizedAccessException))
                {
                    mReportInfo.ExceptionTackle("RM_JM_Details_Failed_AccessDenied", SPNodeLevel.SiteCollection.ToString());
                }
                else
                {
                    //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, siteID, archiveLevel, mReportInfo.SubJobId);
                    mReportInfo.ExceptionTackle(ex.InnerException.Message, SPNodeLevel.SiteCollection.ToString());
                }
                ApprovedDatasSqliteHelper.UpdateStatus(siteID, (int)ProcessedStatus.Failed);
            }
            finally
            {
                mReportInfo.AddDeletionReport((int)CacheNodeType.SiteCollection, GetExecuteActionForJobDetail());
            }
        }

        private bool IsSiteExistInRecycleBin(IAveTenant tenant, string siteUrl)
        {
            var exist = false;
            try
            {
                var properties = tenant.GetDeletedSitePropertiesByUrl(siteUrl);
                exist = true;
                mLog.Info($"Site exists in recycle bin.");
            }
            catch (Exception e)
            {
                mLog.Error("Check site in recycle bin failed {0}.Error:{1}", siteUrl, e);
            }
            return exist;
        }

        // only has stub file remain and no other issue => allow to delete site collection
        private bool CheckOnlyStubFileRemain4SC()
        {
            if (mConfig.currentRule.PolicyLevel != PolicyLevel.Teams && mConfig.currentRule.PolicyLevel != PolicyLevel.SiteCollection)
            {
                // not log to not spam for lower level rule
                return false;
            }

            if (mConfig.JobReportDto.HasErrorNode)
            {
                mLog.Warn($"IsOnlyStubFileRemain. Current Site:{Site.Url} cannot be deleted because the job has error nodes. Skip check remain file");
                return false;
            }

            if (_isNonStubFileRemain)
            {
                mLog.Info($"IsOnlyStubFileRemain. Current Site:{Site.Url} cannot be deleted because there are normal (non-stub) files remaining.");
                return false;
            }

            mLog.Info($"IsOnlyStubFileRemain. Current Site:{Site.Url} only has stub file remain, allow to delete site collection.");
            return true;
        }

        private bool IsChannelSiteDefaultLib(IAveList list)
        {
            try
            {
                if (GetListUrl(list).Equals("Shared Documents", StringComparison.OrdinalIgnoreCase)
                && list.BaseTemplate == AveListTemplateType.DocumentLibrary
                && Site.RootWeb.WebTemplate.Equals("TEAMCHANNEL", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception e)
            {
                mLog.Error($"Fail check Is Channel Site Default Lib, error:{e}");
                return false;
            }
            
        }

        private string GetListUrl(IAveList discoverList)
        {
            return discoverList.RootFolder.ServerRelativeUrl.Split("/").LastOrDefault();
        }

        private Record GetSiteRecord(IAveSite site)
        {
            var recId = GetRecordId(site.ID, site.ID);
            return new Record()
            {
                Id = recId,
                RecordsId = string.Empty,
                LeafName = site.RootWeb.Title,
                FullPath = site.RootWeb.Url,
                ScopeId = site.ID,
                DirPath = site.RootWeb.Url,
                NodeId = site.ID,
                // = site.ID.ToString(),
                CollectTime = DateTime.UtcNow.Ticks,
                TimeCreated = site.RootWeb.Created.Ticks,
                CreateDate = int.Parse(site.RootWeb.Created.ToString("yyyyMMdd")),
                NodeType = 100,
                //TermId = itemRule.TermInfo.UniqueId,
                //TermName = itemRule.TermInfo.Name,
                SourceFlag = (int)SourceFlag.SharePoint,
                // DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(itemRule.DisposalAction),
                // RuleId = itemRule.Rule != null ? new Guid(itemRule.Rule.Id) : Guid.Empty,
                // RuleLevel = itemRule.Rule != null ? (int)itemRule.Rule.PolicyLevel : 0,
                HoldStatus = false,
                RecordStatus = 2,
            };
        }

        public Guid GetRecordId(Guid scopeId, Guid nodeId)
        {
            return ToMd5(scopeId.ToString().ToLowerInvariant() + nodeId.ToString().ToLowerInvariant());
        }
        public Guid ToMd5(string source)
        {
            return HashCodeHelper.StringHash(source);
        }

        [SuppressMessage("FxCopCustomRules", "C100003:DoNotUseSpecificSPMethod", MessageId = "Microsoft.SharePoint.SPWeb.get_Lists")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Archiver Deletion")]
        private void DeleteWeb()
        {
            int archiveLevel = GetArchiveLevel();
            bool shouldReport = true;
            Guid webID = Web.ID;
            string errorDetail = string.Empty;
            if (mConfig.ArchiveJobSplitedDBInfo.IsUseSplitedDB && !mConfig.ArchiveJobSplitedDBInfo.IsLatestSplitedDB)
            {
                mLog.Info($"the web run in virtual job and not is latest virtual job, will skip delete and delete in latest virtual job, web id:{webID}");
                return;
            }
            try
            {
                mLog.Info("Current Web Url:{0}. Web ID:{1}.", Web.ServerRelativeUrl, webID);
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.DeleteWeb"))
                {
                    if (Web.IsRootWeb)
                    {
                        mLog.Info("Current web is RootWeb.WebID:{0}.", webID);
                        shouldReport = false;
                        return;
                    }
                    if (Web.IsAppWeb)
                    {
                        mLog.Info("Current web is AppWeb.WebID:{0}.", webID);
                        shouldReport = false;
                        return;
                    }
                    if ((mConfig.currentRule.KeepDataOption & (int)KeepDataOption.NotBackup) != (int)KeepDataOption.NotBackup)
                    {
                        if (!ChecklistAllowDeletion(Web.Lists, ref errorDetail))
                        {
                            if (CheckOnlyStubFileRemain4SC())
                            {
                                // skip instead of fail, if only stub file remain to allow to delete site collection later
                                mLog.Info($"Current Web:{Web.ServerRelativeUrl} cannot be deleted because there are lists that cannot be deleted, but only stub files remain. Set Skip instead of fail delete web to allow site collection deletion later.");
                                mReportInfo.SetReportStatus(JobDetailsStatus.Skipped, LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteSiteList);
                                return;
                            }

                            var errorMessage = string.IsNullOrEmpty(errorDetail) 
                                ? LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteSiteList 
                                : LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteWebList;

                            //modify for SAAS-24910对summary comment信息国际化
                            mReportInfo.SetFailedInfo(errorMessage, errorDetail);

                            mConfig.JobReportDto.HasErrorNode = true;
                            //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, webID, archiveLevel, mReportInfo.SubJobId);
                            mLog.Warn("This site some list should not be deleted");
                            return;
                        }
                    }
                    //delete
                    string webName = Web.Name.ToString();

                    Web.Delete();
                    SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(mReportInfo.Size, mConfig.GetNodeFullPath(mReportInfo.Url));
                    UpdateExploreDB(webID, 2);
                    //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Archived, webID, archiveLevel, mReportInfo.SubJobId);
                    mLog.Info(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteWebsucceed, webName);
                }
            }
            catch (ServerException ex)
            {
                mLog.Error("ServerException." + LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteWeb, ex);
                if (IsSkipSubWebRemainError(ex))
                {
                    return;
                }
                mReportInfo.ExceptionTackle(ex.InnerException.Message, SPNodeLevel.Web.ToString());
                ApprovedDatasSqliteHelper.UpdateStatus(webID, (int)ProcessedStatus.Failed);
            }
            catch (Exception ex)
            {
                mLog.Error(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteWeb, ex);
                //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, webID, archiveLevel, mReportInfo.SubJobId);
                if (ex.InnerException is ServerException serverException && IsSkipSubWebRemainError(serverException))
                {
                    return;
                }
                mReportInfo.ExceptionTackle(ex.InnerException.Message, SPNodeLevel.Web.ToString());
                ApprovedDatasSqliteHelper.UpdateStatus(webID, (int)ProcessedStatus.Failed);
            }
            finally
            {
                if (shouldReport)
                {
                    mReportInfo.AddDeletionReport((int)CacheNodeType.Web, GetExecuteActionForJobDetail());
                }
            }
        }

        private bool IsSkipSubWebRemainError(ServerException ex)
        {
            if (ex == null) return false;
            if (ex.ServerErrorCode == AveSPErrorCode.V_CANT_DELETE_SERVICE_WITH_SUBWEBS_15)
            {
                // need to check for the sub site still exist
                if (CheckOnlyStubFileRemain4SC() && !CheckSubWebAllowDelete(Web))
                {
                    mLog.Info($"Current Web:{Web.ServerRelativeUrl} cannot be deleted but only stub files remain. Set Skip instead of fail delete web to allow site collection deletion later.");
                    mReportInfo.SetReportStatus(JobDetailsStatus.Skipped, ex.Message);
                    return true;
                }

                mLog.Info($"Current Web:{Web.ServerRelativeUrl} cannot be deleted. Set Fail delete web.");
                return false;
            }

            mLog.Info($"Current Web:{Web.ServerRelativeUrl} cannot be deleted because of ServerException. Errorcode: {ex.ServerErrorCode}, Ex:{ex}. Set Fail delete web.");
            return false;
        }

        private bool CheckSubWebAllowDelete(IAveWeb subsite)
        {
            var hasAnySubWeb = false;
            if (subsite.Webs.Count > 0)
            {
                mLog.Info($"CheckSubWebAllowDelete.Current web has subwebs:{subsite.Webs.Count} in web: {subsite.ServerRelativeUrl}.");
                foreach (IAveWeb web in subsite.Webs)
                {
                    if (!web.IsAppWeb)
                    {
                        mLog.Info($"CheckSubWebAllowDelete.Current web is:{web.ServerRelativeUrl} in web: {subsite.ServerRelativeUrl}.");
                        hasAnySubWeb = true;
                        break;
                    }

                    mLog.Info($"CheckSubWebAllowDelete.Current web is appweb:{web.ServerRelativeUrl} in web: {subsite.ServerRelativeUrl}.");
                }
                return !hasAnySubWeb;
            }

            mLog.Info($"CheckSubWebAllowDelete.Current web has no subwebs in web: {subsite.ServerRelativeUrl}.");
            return true;
        }

        [SuppressMessage("FxCopCustomRules", "C100003:DoNotUseSpecificSPMethod", MessageId = "Microsoft.SharePoint.SPList.get_Items")]
        private void DeleteList()
        {
            int archiveLevel = GetArchiveLevel();
            bool shouldReport = true;
            string listTitle = string.Empty;
            Guid listID = Guid.Empty;
            if (mConfig.ArchiveJobSplitedDBInfo.IsUseSplitedDB && !mConfig.ArchiveJobSplitedDBInfo.IsLatestSplitedDB)
            {
                mLog.Info($"the list run in virtual job and not is latest virtual job, will skip delete and delete in latest virtual job, list id:{List?.ID}");
                return;
            }
            if (null == List)
            {
                shouldReport = false;
                return;
            }
            else
            {
                listTitle = List.Title;
                listID = List.ID;
            }
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.DeleteList"))
                {
                    mLog.Info("Current List BaseTemplate is:{0}.BaseType:{1}.List title:{2}.List Url:{3}.", List.BaseTemplate.ToString(), List.BaseType.ToString(), List.Title, List.DefaultViewUrl);
                    if (!ChecklistAllowDeletion(List, listID, archiveLevel))
                    {
                        return;
                    }
                    //SAAS-26520 删除workflow
                    foreach (var ct in List.ContentTypes)
                    {
                        var asso = ct.WorkflowAssociations;
                        while (asso.Count > 0)
                        {
                            asso.Remove(asso[0]);
                        }
                    }
                    List.Delete();
                    SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(mReportInfo.Size, mConfig.GetNodeFullPath(mReportInfo.Url));
                    RMArchiverSettingsService.DeleteArchiverSetting(listID, new Guid(mConfig.AveSiteId));
                    //If list has beed Delete ,we do not need to Change EnabelVersioning back
                    if (mEnableVersionList.Contains(listID))
                    {
                        mEnableVersionList.Remove(listID);
                    }
                    mLog.Info(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteListsucceed, node.FullPath);
                    UpdateExploreDB(listID, 2);
                    //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Archived, listID, archiveLevel, mReportInfo.SubJobId);
                }
            }
            catch (Exception ex)
            {
                mLog.Error(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteList, node.FullPath, ex.ToString());
                //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, listID, archiveLevel, mReportInfo.SubJobId);
                mReportInfo.ExceptionTackle(ex.InnerException.Message, SPNodeLevel.List.ToString(), List);
                ApprovedDatasSqliteHelper.UpdateStatus(listID, (int)ProcessedStatus.Failed);
            }
            finally
            {
                if (shouldReport)
                {
                    mReportInfo.AddDeletionReport((int)CacheNodeType.List, GetExecuteActionForJobDetail());
                }
            }
        }

        #region add for RevIM folder rule
        /// <summary>
        /// Archiver Remove & End User Container Level Rule 不删除folder，只有folder level rule才删除folder.
        /// </summary>
        private void DeleteFolder()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.DeleteFolder"))
            {
                if (mConfig.currentRule != null && (
                        mConfig.currentRule.PolicyLevel == PolicyLevel.Teams
                     || mConfig.currentRule.PolicyLevel == PolicyLevel.SiteCollection
                     || mConfig.currentRule.PolicyLevel == PolicyLevel.Site
                     || mConfig.currentRule.PolicyLevel == PolicyLevel.List
                     || mConfig.currentRule.PolicyLevel == PolicyLevel.Library))
                {
                    mLog.Info("Current rule is container level rule and don't delete folder.");
                    return;
                }
                if (mConfig.ArchiveJobSplitedDBInfo.IsUseSplitedDB && !mConfig.ArchiveJobSplitedDBInfo.IsLatestSplitedDB)
                {
                    mLog.Info($"the folder run in virtual job and not is latest virtual job, will skip delete and delete in latest virtual job, folder id:{mHeaderInfo?.Attributes[KeyWord.ID]?.Value}");
                    return;
                }
                bool shouldReport = true;
                int archiveLevel = GetArchiveLevel();
                Guid folderId = Guid.Empty;
                string folderName = string.Empty;
                IAveListItem folderItem = null;
                try
                {
                    if (List == null)
                    {
                        mLog.Info($"Current folder is in system list or not in anylist. Url: {mReportInfo.Url} ");
                        mReportInfo.Status = JobDetailsStatus.Skipped;
                        return;
                    }

                    folderId = new Guid(mHeaderInfo.Attributes[KeyWord.ID].Value);
                    try
                    {
                        folderItem = List.GetItemByUniqueId(folderId);
                    }
                    catch (Exception e)
                    {
                        //GetListItemByUniqueId 中抛出的错误固定，为了减少影响，所以使用字符串判定
                        if (e.InnerException != null && e.InnerException.Message.Contains("Item does not exist"))
                        {
                            mLog.Info("Cannot found folder ID: {0}.", folderId);
                        }
                        else
                        {
                            throw;
                        }
                    }
                    DestructionReport destructionReport = null;

                    if (folderItem != null)
                    {
                        GetComplianceTagIfEnableRemove(folderItem, out ListItemComplianceInfo complianceInfo);
                        destructionReport = GetDestructionReportBySource(folderId, folderItem, destructionReport);
                        IAveFolder aveFolder = List.GetFolder(mHeaderInfo.Attributes[KeyWord.URL].Value);
                        //aveFolder.Exists
                        if (((mConfig.currentRule.KeepDataOption & (int)KeepDataOption.NotBackup) != (int)KeepDataOption.NotBackup) && (aveFolder.ItemCount > 0 || aveFolder.SubFolders.Count > 0))
                        {
                            mLog.Debug($"List properties of folder {mReportInfo.Url}. Hidden: {List.Hidden} IsCatalog: {List.IsCatalog}, BaseTemplate: {List.BaseTemplate}");

                            if (List.BaseTemplate == AveListTemplateType.DesignCatalog
                            || List.BaseTemplate == AveListTemplateType.MasterPageCatalog
                            || List.BaseTemplate == AveListTemplateType.WebPageLibrary
                            || List.BaseTemplate == AveListTemplateType.ThemeCatalog
                            || List.BaseTemplate == AveListTemplateType.WebTemplateExtensionsList // RECO-29989: skip delete folder contain system files in "wte" List
                            || List.Hidden || List.IsCatalog)
                            {
                                mLog.Info($"This folder may contain system file and is in system list so skip delete. {mReportInfo.Url}");
                                mReportInfo.Status = JobDetailsStatus.Skipped;
                                return;
                            }

                            mReportInfo.Status = JobDetailsStatus.Failed;
                            mReportInfo.ExceptionTackle("StorageOptimization_SOARFolderHasSubFolderOrSubItemCannotDelete", SPNodeLevel.Folder.ToString());
                            //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, folderId, archiveLevel, mReportInfo.SubJobId);
                            mLog.Error("Can't delete folder. Current Folder:{0} has SubFolder:{1} or SubItem:{2}.", folderItem.Name, aveFolder.SubFolders.Count, aveFolder.ItemCount);
                            if (aveFolder.Files != null && aveFolder.Files.Count > 0)
                            {
                                foreach (var aveFile in aveFolder.Files)
                                {
                                    mLog.Error("Can't delete folder. Current Folder:{0} has files:{1}.", folderItem.Name, aveFile.ServerRelativeUrl);
                                }
                            }
                            return;
                        }

                        if (SpCommonUtility.IsTeamChannelFolder(folderItem))
                        {
                            throw new Exception("StorageOptimization_SOTeamsSystemFolderDeleteFailed");
                        }

                        DeleteComplianceTagIfEnableRemove(folderItem, complianceInfo, out bool needRestoreComplianceTag);
                        folderName = folderItem.Name;
                        folderItem.Delete();
                        RMArchiverSettingsService.DeleteArchiverSetting(folderId, new Guid(mConfig.AveSiteId));
                        AddToDestructionCache(destructionReport);
                        mLog.Info($"Delete folder action type is: {mConfig.actionType}");
                        using (PerformanceScope scope = new("AddAllDestructionReportForFolder"))
                        {
                            if ((mConfig.currentRule.KeepDataOption & (int)KeepDataOption.NotBackup) == (int)KeepDataOption.NotBackup)
                            {
                                AddAllDestructionReportForFolder(folderItem.UniqueId.ToString());
                            }
                        }
                    }
                    else
                    {
                        mLog.Info("Cannot found folder, will skip. Item Name: {0}, ID: {1}.", folderName, folderId);
                        mReportInfo.Status = JobDetailsStatus.Skipped;
                    }
                    UpdateExploreDB(folderId, 2);
                    SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(mReportInfo.Size, mConfig.GetNodeFullPath(mReportInfo.Url));
                    mLog.Info("Delete folder item success. Item Name: {0}, ID: {1}.", folderName, folderId);
                    //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Archived, folderId, archiveLevel, mReportInfo.SubJobId);
                }
                catch (Exception ex)
                {
                    ApprovedDatasSqliteHelper.UpdateStatus(folderId, (int)ProcessedStatus.Failed);
                    if (List.BaseTemplate == AveListTemplateType.PictureLibrary &&
                        (mHeaderInfo.Attributes[KeyWord.URL].Value.EndsWith("/_t", StringComparison.OrdinalIgnoreCase)
                        || mHeaderInfo.Attributes[KeyWord.URL].Value.EndsWith("/_w", StringComparison.OrdinalIgnoreCase)))
                    {
                        //ADO-164699 Picture Library 上传文件会自动生成相应的image，这类文件不显示在job detail里.
                        mLog.Info("Current List is Picture Library,Folder has deleted.");
                        shouldReport = false;
                        return;
                    }
                    mReportInfo.Status = JobDetailsStatus.Failed;
                    if ((ex.InnerException?.Message?.Contains("Folder cannot be deleted because it contains items which are either on hold or declared as records")).GetValueOrDefault())
                    {
                        mReportInfo.ExceptionTackle("StorageOptimization_SOARFolderHasHoldOrDeclareCannotDelete", SPNodeLevel.Folder.ToString());
                    }
                    else
                    {
                        mReportInfo.ExceptionTackle(ex.Message, SPNodeLevel.Folder.ToString());
                    }
                    if (ex.Message != null && ex.Message.Contains("This item cannot be updated because it is locked as read-only"))
                    {
                        mReportInfo.SetFailedInfo("StorageOptimization13_SOARDeleteOfficeLockFile");
                        mLog.Warn("[ArchiverDeletion][DeleteFolder]This item cannot be updated because it is locked as read-only.");
                        mConfig.JobReportDto.HasErrorNode = true;
                    }
                    else if (ex.Message != null && ex.Message.Contains("The label that's applied to this item prevents it from being edited or deleted. Check the item's label for more details"))
                    {
                        mReportInfo.SetFailedInfo("StorageOptimization_SOARRecordManagerLabelDocumentDeleteFailed");
                        mLog.Warn("[ArchiverDeletion][DeleteFolder]The label that's applied to this item prevents it from being edited or deleted. Check the item's label for more details.");
                        mConfig.JobReportDto.HasErrorNode = true;
                    }
                    //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, folderId, archiveLevel, mReportInfo.SubJobId);
                    mLog.Error("An error occur while delete folder. Folder ID:{0},Folder Name:{2}, Message:{1}.", folderId, ex.ToString(), folderName);
                }
                finally
                {
                    if (shouldReport)
                    {
                        mReportInfo.AddDeletionReport((int)CacheNodeType.Folder, GetExecuteActionForJobDetail());
                    }
                }
            }
        }

        private DestructionReport GetDestructionReportBySource(Guid folderId, IAveListItem folderItem, DestructionReport destructionReport)
        {
            if (mConfig.IsOneDriverSite)
            {
                var record = GetRecordInExplorerDao(folderId);
                if (record != null)
                {
                    destructionReport = GetOnedriveDestructionReport(folderItem, record.TermName, record.RecordsId);
                }
            }
            else
            {
                destructionReport = GetDestructionReport(folderItem);
            }

            return destructionReport;
        }

        private void AddAllDestructionReportForFolder(string folderNodeId)
        {
            var folderOrFiles = ScanDBOperationFactory.GetScanDBOperation(mConfig).SelectItemsByParentWithJsonMeta(mConfig.currentRule.Id, folderNodeId);
            mLog.Info($"When not backup rule, build destrunction report from scan db. Get [{folderNodeId}] sub folders or files count is: {folderOrFiles.Count}");
            foreach (var ff in folderOrFiles)
            {
                AddToDestructionCache(GetDestructionReport(ff));
                if (ff.CacheNodeType >= (int)CacheNodeType.Folder && ff.CacheNodeType < (int)CacheNodeType.Item)
                {
                    AddAllDestructionReportForFolder(ff.NodeId);
                }
            }
        }

        #endregion

        [SuppressMessage("FxCopCustomRules", "C100003:DoNotUseSpecificSPMethod")]
        private void DeleteListItem()
        {
            int archiveLevel = GetArchiveLevel();
            Guid ListItemId = Guid.Empty;
            int ListItemRowId = -1;
            bool readyForReport = true;
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.DeleteListItem"))
                {
                    ListItemId = new Guid(mHeaderInfo.Attributes[KeyWord.ID].Value);
                    ListItemRowId = Convert.ToInt32(mHeaderInfo.Attributes[KeyWord.RowId].Value);
                    IAveListItem listItem = null;
                    string listItemFullPath = string.Empty;
                    try
                    {
                        if (List.BaseTemplate == AveListTemplateType.DesignCatalog
                            || List.BaseTemplate == AveListTemplateType.MasterPageCatalog
                            || List.BaseTemplate == AveListTemplateType.WebPageLibrary
                            || List.BaseTemplate == AveListTemplateType.ThemeCatalog
                            || List.Hidden || List.IsCatalog)
                        {
                            mLog.Info($"Skip delete system file {mReportInfo.Url}");
                            mReportInfo.Status = JobDetailsStatus.Skipped;
                            return;
                        }
                        if (ListItemRowId >= 0)
                        {
                            listItem = List.GetItemById(ListItemRowId);//List.GetItemByUniqueId(ListItemId);
                        }
                        else
                        {
                            listItem = List.GetItemByUniqueId(ListItemId);
                        }
                        if (CheckItemHasModifiedAfterBackup(mHeaderInfo, listItem))
                        {
                            mReportInfo.Status = JobDetailsStatus.Skipped;
                            mReportInfo.Message = "StorageOptimization_DeleteItemSkip_Modified";
                            return;
                        }
                        listItemFullPath = listItem.ParentList.ParentWeb.Url.TrimEnd('/') + "/" + listItem.Url.TrimStart('/');
                        mLog.Info("Begin delete list item.ItemName:{0}.FullPath:{1}.RowId:{2}.Id:{3}.", listItem.Title, listItemFullPath, ListItemRowId, ListItemId);
                        if (List.BaseTemplate == AveListTemplateType.TasksWithTimelineAndHierarchy)
                        {
                            try
                            {
                                IEnumerable<IAveListItem> items = from subItem in listItem.ParentList.Items where subItem["ParentID"] != null && subItem["ParentID"].ToString().Substring(0, subItem["ParentID"].ToString().IndexOf(';')).Equals(listItem.ID.ToString(), StringComparison.OrdinalIgnoreCase) select subItem;
                                if (items.Count<IAveListItem>() > 0)
                                {
                                    CacheItemDto cacheItemdto = new CacheItemDto();
                                    cacheItemdto.ArchiverLevel = archiveLevel;
                                    cacheItemdto.CacheItem = listItem;
                                    cacheItemdto.BaseTemplate = AveListTemplateType.TasksWithTimelineAndHierarchy;
                                    cacheItemdto.Url = mReportInfo.Url;
                                    mConfig.TasksCacheItemDtoCollection.Add(cacheItemdto);
                                    readyForReport = false;
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                mLog.Error(string.Format("This is some exceptions of deleting task {0}.Error:{1}", listItem.Title, ex.ToString()));
                            }
                        }
                    }
                    catch (Exception exce)
                    {
                        mLog.Warn("Can Not Get File:{0} By Current User: {1}", node.FullPath, exce.ToString());
                    }
                    if (mConfig.CheckItemIsRecordsHold(ListItemId))
                    {
                        mLog.Warn("Item is RecordsHold. Item Name: {0}.", listItem.Name);
                        //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, ListItemId, archiveLevel, mReportInfo.SubJobId);
                        mReportInfo.Status = JobDetailsStatus.Skipped;
                        mReportInfo.Message = "StorageOptimization_EXOExploreHoldFile";
                        return;
                    }
                    if (listItem == null && mConfig.Action == ArchiverAction.RELATIVEDATA_ARCHIVER_BACKUP_JOB_REQUEST)
                    {
                        mLog.Info("Current listItem is null in RELATIVEDATA_ARCHIVER_BACKUP_JOB_REQUEST DeleteListItem.ItemName:{0}.", mHeaderInfo.Attributes[KeyWord.FULLPATH].Value);
                        mReportInfo.Status = JobDetailsStatus.Skipped;
                    }
                    DateTime itemModifyTime = (DateTime)listItem["Modified"];
                    if (!CheckItemModifyTime(itemModifyTime))
                    {
                        mLog.Info(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteListItemModified);
                        mReportInfo.SetFailedInfo(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteListItemModified);
                        //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, listItem.UniqueId, archiveLevel, mReportInfo.SubJobId);
                        return;
                    }
                    if (mBackupDeleteLowLevelStatus == false)//判断是否有备份失败的version
                    {
                        mLog.Info(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteListItemItemVersion);
                        mReportInfo.SetFailedInfo(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteListItemItemVersion);
                        //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, ListItemId, archiveLevel, mReportInfo.SubJobId);
                        return;
                    }
                    if (!CheckRelatedObjectStatus(listItem.Name, ListItemId, archiveLevel, listItem))
                    {
                        return;
                    }
                    if (DeleteRelatedObjectForDeleteOnlyAction(ListItemId, listItem, mConfig.currentRule.RelatedRecordOption == RelatedRecordOption.Both ? 1 : 0))
                    {
                        mLog.Info("Has related ojbect delete failed.");
                        mReportInfo.SetFailedInfo("StorageOptimization13_SOARRelatedRecordDeleteFailed");
                        //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, listItem.UniqueId, archiveLevel, mReportInfo.SubJobId);
                        return;
                    }
                    //delete
                    if (listItem != null)
                    {
                        string name = listItem.Name;
                        ListItemComplianceInfo complianceInfo = null;
                        bool needRestoreComplianceTag = false;
                        try
                        {
                            GetComplianceTagIfEnableRemove(listItem, out complianceInfo);
                            if (!mConfig.currentRule.LockRecordBeforeDestroy &&
                                complianceInfo != null && complianceInfo.TagPolicyHold && !complianceInfo.TagPolicyRecord &&
                                IsRecordTypeComplianceTag(Site, complianceInfo.ComplianceTag))
                            {
                                mLog.Info("skip Delete current unlock status item. Item Name: {0}.", listItem.Name);
                                mReportInfo.Status = JobDetailsStatus.Skipped;
                                mReportInfo.Message = "StorageOptimization_Skip_Unlock_Status_Item";
                                return;
                            }
                            if (ScheduleConfiguration.CheckisRecord(listItem) && mConfig.currentRule.DeleteRecords)
                            {
                                Record.UndeclareItemAsRecord(listItem);
                            }
                            DeleteComplianceTagIfEnableRemove(listItem, complianceInfo, out needRestoreComplianceTag);
                            RemoveRelatedRelationship(listItem, listItemFullPath);
                            AveTaskRetryHelper retryHelper = new AveTaskRetryHelper(5, true);//new KeyValuePair<string, string>("ServerException", "HRESULT: 0x80131904")
                            CaculateItemSize(listItem);
                            retryHelper.ExecuteWithRetryMechanism(() =>
                            {
                                listItem.Delete();
                                needRestoreComplianceTag = false;
                                //DeleteEmptyFolder(aveFolder);
                            });
                        }
                        catch (Exception spex)
                        {
                            if (ScheduleConfiguration.CheckisRecord(listItem) && mConfig.currentRule.DeleteRecords
                                || (mConfig.IsILMode && ArchiverCommonStaticMethod.IsBlockDeleteOnlyRecord(listItem)))
                            {
                                mLog.Info("Office 365 Begin CheckItemIsRecord");
                                if (ArchiverCommonStaticMethod.CheckIsRecordOnly(listItem))
                                {
                                    mLog.Info("This Item is Declare Item. Item Name:{0}", listItem.Name);
                                    bool itemChange = false;
                                    if (listItem.Attachments.Count != 0)
                                    {
                                        foreach (IAveAttachment attachment in listItem.Attachments)
                                        {
                                            if (mConfig.cacheRecordAttachments.ContainsKey(ListItemId))
                                            {
                                                if (!mConfig.cacheRecordAttachments[ListItemId].Contains(attachment.FileName))
                                                {
                                                    mLog.Warn("This Item is Office 365 Declare Item,but attachment has modified,Item Name is {0}.", listItem.Name);
                                                    itemChange = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    if (!itemChange)
                                    {
                                        Record.UndeclareItemAsRecord(listItem);
                                        listItem.Delete();
                                        needRestoreComplianceTag = false;
                                        UpdateExploreDB(ListItemId, 2);
                                        //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Archived, ListItemId, archiveLevel, mReportInfo.SubJobId);
                                        return;
                                    }
                                    else
                                    {
                                        mLog.Warn("This Item is Declare Only Item,but attachment has modified,ItemName:{0}", listItem.Name);
                                        mConfig.JobReportDto.HasErrorNode = true;
                                        //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, ListItemId, archiveLevel, mReportInfo.SubJobId);
                                        mReportInfo.SetFailedInfo("This Item is Declare Only Item,but attachment has modified");//SOArchiverInternationalString.SOARSOItemAttachmentNameOrCountChanged);
                                        return;
                                    }
                                }
                                else
                                {
                                    mLog.Warn("This Item is Declare And Hold Item.ItemName:{0}", listItem.Name);
                                    mConfig.JobReportDto.HasErrorNode = true;
                                    //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, ListItemId, archiveLevel, mReportInfo.SubJobId);
                                    mReportInfo.SetFailedInfo("This Item is Declare And Hold Item");//SOArchiverInternationalString.StorageOptimization13_SOARSOFileIsDeclareRecordAndHold);
                                    return;
                                }
                            }

                            if (spex.InnerException != null && spex.InnerException.Message.Contains("The label that's applied to this item prevents it from being edited or deleted"))
                            {
                                mLog.Info("Current item label file.FileName:{0}.", listItem.UniqueId);
                                if (mConfig.IsILMode
                                    && listItem.Fields.ContainsField("Retention label")
                                    && LabelAppliedByRecords(listItem["Retention label"].ToString()))
                                {
                                    mLog.Info("Current item is label file and Records remove label and delete.FileName:{0}.", listItem.UniqueId);
                                    //listItem.SetComplianceTag(string.Empty, false, false, false, false, false);
                                    listItem.SetComplianceTagOnBulkItems(string.Empty);
                                    if (ListItemRowId >= 0)
                                    {
                                        listItem = List.GetItemById(ListItemRowId);//List.GetItemByUniqueId(ListItemId);
                                    }
                                    else
                                    {
                                        listItem = List.GetItemByUniqueId(ListItemId);
                                    }

                                    listItem.Delete();
                                    needRestoreComplianceTag = false;
                                    mLog.Info("Delete label item success.File name:{0}", listItem.UniqueId);
                                    UpdateExploreDB(ListItemId, 2);
                                    //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Archived, docId, archiveLevel, mReportInfo.SubJobId);
                                    return;
                                }
                                else
                                {
                                    mReportInfo.SetFailedInfo("StorageOptimization_SOARRecordManagerLabelDocumentDeleteFailed");
                                    mConfig.JobReportDto.HasErrorNode = true;
                                    //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, ListItemId, archiveLevel, mReportInfo.SubJobId);
                                    return;
                                }
                            }

                            if (spex.Message != null && spex.Message.Contains("This item cannot be updated because it is locked as read-only"))
                            {
                                mReportInfo.SetFailedInfo("StorageOptimization13_SOARDeleteOfficeLockFile");
                                mLog.Warn("[ArchvierDeletion][DeleteListItem]This item cannot be updated because it is locked as read-only.");
                                mConfig.JobReportDto.HasErrorNode = true;
                            }
                            else if (spex.Message != null && spex.Message.Contains("The label that's applied to this item prevents it from being edited or deleted. Check the item's label for more details"))
                            {
                                mReportInfo.SetFailedInfo("StorageOptimization_SOARRecordManagerLabelDocumentDeleteFailed");
                                mLog.Warn("[ArchvierDeletion][DeleteListItem]The label that's applied to this item prevents it from being edited or deleted. Check the item's label for more details.");
                                mConfig.JobReportDto.HasErrorNode = true;
                            }
                            mLog.Warn("List Item {0}.FullPath:{1}.Delete Error:{2}.", node.FullPath, listItemFullPath, spex.ToString());
                        }
                        finally
                        {
                            if (needRestoreComplianceTag)
                            {
                                SetComplianceTagIfEnableRemove(listItem, complianceInfo);
                            }
                        }
                        mLog.Info(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteListItemsucceed, " ");
                        UpdateExploreDB(ListItemId, 2);
                        //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Archived, ListItemId, archiveLevel, mReportInfo.SubJobId);
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Error("delete list item {0} failed:{1}", node.FullPath, ex);
                ApprovedDatasSqliteHelper.UpdateStatus(ListItemId, (int)ProcessedStatus.Failed);
                //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, ListItemId, archiveLevel, mReportInfo.SubJobId);
                mReportInfo.ExceptionTackle(ex.Message, SPNodeLevel.Item.ToString());
            }
            finally
            {
                mBackupDeleteLowLevelStatus = true;//reset
                if (readyForReport)
                {
                    if (mConfig.Action == ArchiverAction.RELATIVEDATA_ARCHIVER_BACKUP_JOB_REQUEST)
                    {
                        mReportInfo.AddRelativeDataArchiverReport(SPNodeLevel.Item);
                    }
                    else
                    {
                        mReportInfo.AddDeletionReportToUpdateItemStatus((int)CacheNodeType.Item, GetExecuteActionForJobDetail(), ListItemId, 0, versionSize);
                    }
                }
                versionSize = 0;
            }
        }
        private void CaculateItemSize(IAveListItem listItem)
        {
            int versionCount = 1;
            if (listItem.Versions != null && listItem.Versions.Count > 0)
            {
                versionCount = listItem.Versions.Count;
            }
            SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(ContractConstants.ITEMSIZEFORLICENSE * versionCount, mConfig.GetNodeFullPath(mReportInfo.Url));
        }
        private bool LabelAppliedByRecords(string labelName)
        {
            if (string.IsNullOrWhiteSpace(labelName))
            {
                return false;
            }

            if (mConfig.IsOneDriverSite)
            {
                return RecordsDBOperation.RMEXOLabels.Any(x => x.LabelName.Equals(labelName, StringComparison.OrdinalIgnoreCase) && x.Status == 1 && x.Type == 2);
            }
            //else if (mConfig.IsTeams)
            //{
            //    return RecordsDBOperation.RMEXOLabels.Any(x => x.LabelName.Equals(labelName, StringComparison.OrdinalIgnoreCase) && x.Status == 1 && x.Type == 2); type ?
            //}
            else
            {
                return RecordsDBOperation.RMEXOLabels.Any(x => x.LabelName.Equals(labelName, StringComparison.OrdinalIgnoreCase) && x.Status == 1 && x.Type == 1);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100003:DoNotUseSpecificSPMethod")]
        private void DeleteListItemVersion()
        {
            int archiveLevel = GetArchiveLevel();
            Guid ListItemID = Guid.Empty;
            bool shouldReport = true;
            int uiVersion = 0;
            try
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.DeleteListItemVersion"))
                {
                    ListItemID = new Guid(mHeaderInfo.Attributes[KeyWord.ID].Value);
                    IAveListItem listItem = List.GetItemByUniqueId(ListItemID);
                    uiVersion = Convert.ToInt32(mHeaderInfo.Attributes[KeyWord.VERSION].Value);
                    IAveListItemVersion version = listItem.Versions.GetVersionFromID(uiVersion);
                    if (version != null)
                    {
                        if (version.IsCurrentVersion)
                        {
                            shouldReport = false;
                            return;
                        }
                        if (!CheckItemModifyTime(version.Created))
                        {
                            mReportInfo.SetFailedInfo(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteListItemVersionModified);
                            return;
                        }
                        if (mConfig.CheckItemIsRecordsHold(ListItemID))
                        {
                            mLog.Warn($"Item is RecordsHold. Item Name: {listItem.ID}, item version is {uiVersion}.");
                            //mConfig.soArchiverQueryWorkerForDel.UpdateArchiveDeletionStatus(SOApproveDBStatus.Failed, ListItemID, archiveLevel, uiVersion, mReportInfo.SubJobId);
                            mReportInfo.Status = JobDetailsStatus.Skipped;
                            mReportInfo.Message = "StorageOptimization_EXOExploreHoldFile";
                            return;
                        }
                        GetComplianceTagIfEnableRemove(listItem, out ListItemComplianceInfo complianceInfo);
                        if (!mConfig.currentRule.LockRecordBeforeDestroy &&
                            complianceInfo != null && complianceInfo.TagPolicyHold && !complianceInfo.TagPolicyRecord &&
                            IsRecordTypeComplianceTag(Site, complianceInfo.ComplianceTag))
                        {
                            mLog.Info("skip Delete current unlock status item. Item Name: {0}.", listItem.Name);
                            mReportInfo.Status = JobDetailsStatus.Skipped;
                            mReportInfo.Message = "StorageOptimization_Skip_Unlock_Status_Item";
                            return;
                        }
                        DeleteComplianceTagIfEnableRemove(listItem, complianceInfo, out bool needRestoreComplianceTag);
                        try
                        {
                            SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(ContractConstants.ITEMSIZEFORLICENSE, mConfig.GetNodeFullPath(mReportInfo.Url));
                            version.Delete();
                            //mConfig.soArchiverQueryWorkerForDel.UpdateArchiveDeletionStatus(SOApproveDBStatus.Archived, ListItemID, archiveLevel, uiVersion, mReportInfo.SubJobId);
                            SetComplianceTagIfEnableRemove(listItem, complianceInfo);
                            needRestoreComplianceTag = false;
                            mLog.Info("delete version {0} of {1}", uiVersion, ListItemID);
                        }
                        catch (Exception ex)
                        {
                            mLog.Error("Delete version {0} of {1} check record status failed:{2}", uiVersion, ListItemID, ex);
                            if(needRestoreComplianceTag)
                            {
                                SetComplianceTagIfEnableRemove(listItem, complianceInfo);
                            }
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                mBackupDeleteLowLevelStatus = false;
                mLog.Error("delete version {0} of {1} failed:{2}", uiVersion, node.FullPath, ex);
                //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, ListItemID, archiveLevel, mReportInfo.SubJobId);
                mReportInfo.ExceptionTackle(ex.Message, SPNodeLevel.ItemVersion.ToString());
                if (ex.Message != null && ex.Message.Contains("This item cannot be updated because it is locked as read-only"))
                {
                    mReportInfo.SetFailedInfo("StorageOptimization13_SOARDeleteOfficeLockFile");
                    mLog.Warn("[ArchiverDeletion][DeleteListItemVersion]This item cannot be updated because it is locked as read-only.");
                    mConfig.JobReportDto.HasErrorNode = true;
                }
                else if (ex.Message != null && ex.Message.Contains("The label that's applied to this item prevents it from being edited or deleted. Check the item's label for more details"))
                {
                    mReportInfo.SetFailedInfo("StorageOptimization_SOARRecordManagerLabelDocumentDeleteFailed");
                    mLog.Warn("[ArchiverDeletion][DeleteListItemVersion]The label that's applied to this item prevents it from being edited or deleted. Check the item's label for more details.");
                    mConfig.JobReportDto.HasErrorNode = true;
                }
            }
            finally
            {
                if (shouldReport)
                {
                    if (mConfig.Action == ArchiverAction.RELATIVEDATA_ARCHIVER_BACKUP_JOB_REQUEST)
                    {
                        mReportInfo.AddRelativeDataArchiverReport(SPNodeLevel.ItemVersion);
                    }
                    else
                    {
                        mReportInfo.AddDeletionReport((int)CacheNodeType.ItemVersion, GetExecuteActionForJobDetail());
                    }
                }
            }
        }

        private bool GetComplianceTagIfEnableRemove(IAveListItem listItem, out ListItemComplianceInfo complianceInfo)
        {
            try
            {
                complianceInfo = null;
                string retentionLabel = listItem.GetComplianceTagName();
                if (!string.IsNullOrWhiteSpace(retentionLabel) &&
                    (WrapperConfiguration.EnableRemoveRetentionLabel ||
                    mConfig.currentRule.IncludeDeleteRecordLabel ||
                    (mConfig.currentRule.KeepDataOption & (int)KeepDataOption.IsEnableRemoveRetentionLabel) == (int)KeepDataOption.IsEnableRemoveRetentionLabel))
                {
                    var nowComplianceInfo = listItem.GetComplianceInfo(false);
                    if(string.IsNullOrWhiteSpace(nowComplianceInfo?.ComplianceTag))
                    {
                        return false;
                    }
                    complianceInfo = new ListItemComplianceInfo()
                    {
                        ComplianceTag = nowComplianceInfo.ComplianceTag,
                        TagPolicyHold = nowComplianceInfo.TagPolicyHold,
                        TagPolicyEventBased = nowComplianceInfo.TagPolicyEventBased,
                        TagPolicyRecord = nowComplianceInfo.TagPolicyRecord
                    };
                    return true;
                }
                return false;
            }
            catch(Exception e)
            {
                mLog.Error($"fail get complianceTag, item:{listItem.Url},Exception:{e}");
                throw;
            }
        }

        private void DeleteComplianceTagIfEnableRemove(IAveListItem listItem, ListItemComplianceInfo complianceInfo, out bool deletedComplianceTag)
        {
            deletedComplianceTag = false;
            try
            {
                if (!string.IsNullOrWhiteSpace(complianceInfo?.ComplianceTag))
                {
                    if (mConfig.currentRule.IncludeDeleteRecordLabel)
                    {
                        var isRecordTypeLabel = IsRecordTypeComplianceTag(Site, complianceInfo.ComplianceTag);
                        if (isRecordTypeLabel)
                        {
                            mLog.Info("Current label is record label and the rule is include the record label");
                            if (mConfig.currentRule.LockRecordBeforeDestroy &&
                                !complianceInfo.TagPolicyRecord && complianceInfo.TagPolicyHold && isRecordTypeLabel)
                            {
                                mLog.Info("Current status of label is unlocked. Locking the record before removing the label");
                                listItem.LockRecordItem();

                                var refreshedComplianceInfo = listItem.GetComplianceInfo(false);
                                if (refreshedComplianceInfo == null || !refreshedComplianceInfo.TagPolicyRecord)
                                {
                                    throw new InvalidOperationException("The record lock state could not be verified before retention label removal");
                                }
                                complianceInfo.TagPolicyRecord = true;
                            }
                            listItem.SetComplianceTagOnBulkItems("");
                            deletedComplianceTag = true;
                        }
                    }
                    if((WrapperConfiguration.EnableRemoveRetentionLabel ||
                        (mConfig.currentRule.KeepDataOption & (int)KeepDataOption.IsEnableRemoveRetentionLabel) == (int)KeepDataOption.IsEnableRemoveRetentionLabel))
                    {
                        if (!IsRecordTypeComplianceTag(Site, complianceInfo.ComplianceTag))
                        {
                            mLog.Info("Current label is not record label and the rule is enable remove retention label");
                            listItem.SetComplianceTagOnBulkItems("");
                            deletedComplianceTag = true;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                mLog.Error($"Fail delete retention label,error message:{ex.Message},error:{ex}");
                throw;
            }

        }

        protected bool IsRecordTypeComplianceTag(IAveSite site, string complianceTagName)
        {
            try
            {
                if (mConfig.SharePointRetentionLabel == null)
                {
                    mConfig.InitRetentionLabelCollections(site);
                }
                if (mConfig.SharePointRetentionLabel.TryGetValue(complianceTagName, out AveComplianceTagInfo info))
                {
                    if (info.BlockDelete && info.BlockEdit)
                    {
                        return true;
                    }
                }
                else
                {
                    mLog.Warn($"Unable get complianceTag info from site avaliable compliance tags by tag name:{complianceTagName}, site url:{site.Url}");
                }
                return false;
            }
            catch (Exception ex)
            {
                mLog.Error($"Fail get complianceTag info from site avaliable compliance tags by tag name:{complianceTagName}, site url:{site.Url}, ex:{ex}");
                throw;
            }
        }

        private bool SetComplianceTagIfEnableRemove(IAveListItem listItem, ListItemComplianceInfo complianceInfo)
        {
            bool isDeclaredRecord = false;
            
            if (!string.IsNullOrWhiteSpace(complianceInfo?.ComplianceTag) &&
                (WrapperConfiguration.EnableRemoveRetentionLabel ||
                mConfig.currentRule.IncludeDeleteRecordLabel ||
                (mConfig.currentRule.KeepDataOption & (int)KeepDataOption.IsEnableRemoveRetentionLabel) == (int)KeepDataOption.IsEnableRemoveRetentionLabel))
            {
                try
                {
                    if (listItem.IsRecord() && mConfig.currentRule.IncludeDeleteRecordLabel)
                    {
                        Record.UndeclareItemAsRecord(listItem);
                        isDeclaredRecord = true;
                    }
                    listItem.SetComplianceTagOnBulkItems(complianceInfo.ComplianceTag);
                    if (mConfig.SharePointRetentionLabel == null)
                    {
                        mConfig.InitRetentionLabelCollections(Site);
                    }
                    if (mConfig.SharePointRetentionLabel.TryGetValue(complianceInfo.ComplianceTag, out AveComplianceTagInfo aveComplianceTagInfo))
                    {
                        if (aveComplianceTagInfo.UnlockedAsDefault && complianceInfo.TagPolicyHold && complianceInfo.TagPolicyRecord && IsRecordTypeComplianceTag(Site,complianceInfo.ComplianceTag))
                        {
                            listItem.LockRecordItem();
                        }
                    }
                    else
                    {
                        mLog.Warn($"can not get compliance init lock status, compliane name :{complianceInfo.ComplianceTag}");
                    }
                    if (isDeclaredRecord)
                    {
                        Record.DeclareItemAsRecord(listItem);
                    }
                    return true;
                }
                catch(Exception ex)
                {
                    mLog.Error($"Fail set retention label,label:{complianceInfo.ComplianceTag},error message:{ex.Message},error:{ex}");
                    throw;
                }
            }
            return false;
        }

        private void FlushPendingDocumentDeletionsIfNeeded(DeletionNode currentNode)
        {
            if (pendingDocumentDeletions.Count == 0)
            {
                return;
            }

            if (currentNode == null || currentNode.ObjectType != AveConstants.TYPE_DOCUMENT || !CanQueueDocumentDeletion(currentNode))
            {
                FlushPendingDocumentDeletions();
                return;
            }

            var pending = pendingDocumentDeletions[0];
            bool sameScope = string.Equals(pending.SiteUrl, currentNode.HeaderInfo.GetAttribute(KeyWord.SiteUrl), StringComparison.OrdinalIgnoreCase)
                && string.Equals(pending.WebId.ToString(), currentNode.HeaderInfo.GetAttribute(KeyWord.WebId), StringComparison.OrdinalIgnoreCase)
                && string.Equals(pending.ListId.ToString(), currentNode.HeaderInfo.GetAttribute(KeyWord.ListId), StringComparison.OrdinalIgnoreCase);

            if (!sameScope)
            {
                FlushPendingDocumentDeletions();
            }
        }

        private bool TryQueuePendingDocumentDeletion(IAveFile file, Guid docId, string realFileUrl, long shouldDeleteObjectTotalSize, string md5, DestructionReport destructionReport, bool useRecordUpdate)
        {
            if (!CanQueueDocumentDeletion(node) || !EnsurePendingDocumentDeleteContext())
            {
                if (mConfig.EnableDeleteDocumentBatchOptimization)
                {
                    mLog.Info($"DeleteDocumentBatchOptimization skip queue and fallback to original delete. FileId:{docId}. Url:{realFileUrl}.");
                }
                return false;
            }

            if (!TryCreatePendingDocumentDeletion(file, docId, realFileUrl, shouldDeleteObjectTotalSize, md5, destructionReport, useRecordUpdate, out PendingDocumentDeletion pendingDeletion))
            {
                return false;
            }

            pendingDocumentDeletions.Add(pendingDeletion);
            mLog.Info($"DeleteDocumentBatchOptimization queue document. FileId:{docId}. PendingCount:{pendingDocumentDeletions.Count}. BatchSize:{mConfig.DeleteDocumentBatchOptimizationBatchSize}. Url:{realFileUrl}.");
            if (pendingDocumentDeletions.Count >= mConfig.DeleteDocumentBatchOptimizationBatchSize)
            {
                mLog.Info($"DeleteDocumentBatchOptimization flush pending documents by batch size. PendingCount:{pendingDocumentDeletions.Count}. BatchSize:{mConfig.DeleteDocumentBatchOptimizationBatchSize}.");
                FlushPendingDocumentDeletions();
            }

            return true;
        }

        private bool CanQueueDocumentDeletion(DeletionNode currentNode)
        {
            if (!mConfig.EnableDeleteDocumentBatchOptimization)
            {
                return false;
            }

            if (currentNode == null || currentNode.ObjectType != AveConstants.TYPE_DOCUMENT)
            {
                return false;
            }

            if (mConfig.Action == ArchiverAction.RELATIVEDATA_ARCHIVER_BACKUP_JOB_REQUEST)
            {
                return false;
            }

            if ((mConfig.currentRule.KeepDataOption & (int)KeepDataOption.KeepLatestVersion) == (int)KeepDataOption.KeepLatestVersion)
            {
                return false;
            }

            if (LinkFileCommon.IsLeaveStubRule(mConfig.currentRule))
            {
                return false;
            }

            return true;
        }

        private bool TryCreatePendingDocumentDeletion(IAveFile file, Guid docId, string realFileUrl, long shouldDeleteObjectTotalSize, string md5, DestructionReport destructionReport, bool useRecordUpdate, out PendingDocumentDeletion pendingDeletion)
        {
            pendingDeletion = null;

            if (pendingDocumentDeleteWeb == null)
            {
                return false;
            }

            var csomFile = pendingDocumentDeleteWeb.GetFileByServerRelativePath(Microsoft.SharePoint.Client.ResourcePath.FromDecodedUrl(realFileUrl));
            if (mConfig.currentRule.DeleteToRecycleBin)
            {
                csomFile.Recycle();
            }
            else
            {
                csomFile.DeleteObject();
            }

            pendingDeletion = new PendingDocumentDeletion
            {
                SiteUrl = mHeaderInfo.GetAttribute(KeyWord.SiteUrl),
                WebId = new Guid(mHeaderInfo.GetAttribute(KeyWord.WebId)),
                ListId = new Guid(mHeaderInfo.GetAttribute(KeyWord.ListId)),
                DocId = docId,
                FileUniqueId = file.UniqueId,
                FileServerRelativeUrl = realFileUrl,
                ReportUrl = mReportInfo.Url,
                SubJobId = mReportInfo.SubJobId,
                RuleName = mReportInfo.RuleName,
                MediaName = mReportInfo.MediaName,
                ShouldDeleteObjectTotalSize = shouldDeleteObjectTotalSize,
                Md5 = md5,
                DestructionReport = destructionReport,
                UseRecordUpdate = useRecordUpdate,
            };

            return true;
        }

        private bool EnsurePendingDocumentDeleteContext()
        {
            if (pendingDocumentDeleteContext != null && pendingDocumentDeleteWeb != null)
            {
                return true;
            }

            pendingDocumentDeleteContext = Web.GetClientContext() as Microsoft.SharePoint.Client.ClientContext;
            if (pendingDocumentDeleteContext == null)
            {
                mLog.Warn($"DeleteDocumentBatchOptimization can not get ClientContext from current web. Url:{mReportInfo?.Url}.");
                return false;
            }

            pendingDocumentDeleteWeb = pendingDocumentDeleteContext.Site.OpenWeb(Web.ServerRelativeUrl);
            return true;
        }

        private void FlushPendingDocumentDeletions()
        {
            if (pendingDocumentDeletions.Count == 0)
            {
                return;
            }

            var batch = new List<PendingDocumentDeletion>(pendingDocumentDeletions);
            pendingDocumentDeletions.Clear();
            mLog.Info($"DeleteDocumentBatchOptimization execute batch delete. Count:{batch.Count}. BatchSize:{mConfig.DeleteDocumentBatchOptimizationBatchSize}.");

            try
            {
                pendingDocumentDeleteContext?.ExecuteQuery();
                mLog.Info($"DeleteDocumentBatchOptimization execute batch delete success. Count:{batch.Count}.");
                foreach (var pending in batch)
                {
                    FinalizePendingDocumentDeletion(pending);
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"Batch document deletion failed. Count:{batch.Count}. Fallback to single delete. Message:{ex}");
                foreach (var pending in batch)
                {
                    FallbackPendingDocumentDeletion(pending);
                }
            }
            finally
            {
                DisposePendingDocumentDeleteContext();
            }
        }

        private void FinalizePendingDocumentDeletion(PendingDocumentDeletion pending)
        {
            if (pending.UseRecordUpdate)
            {
                var record = GetRecordInExplorerDao(pending.DocId);
                UpdateRecordInExploreDB(record, pending.DocId, mConfig.actionType == ActionType.ArchchiveToStorage ? 8 : 2, pathMd5: pending.Md5);
                AddToDestructionCache(pending.DestructionReport);
            }
            else
            {
                UpdateExploreDB(pending.DocId, mConfig.actionType == ActionType.ArchchiveToStorage ? 8 : 2, pathMd5: pending.Md5);
            }

            var report = new ReportInfo(mConfig)
            {
                Url = pending.ReportUrl,
                SubJobId = pending.SubJobId,
                RuleName = pending.RuleName,
                MediaName = pending.MediaName,
                Size = pending.ShouldDeleteObjectTotalSize,
                Status = JobDetailsStatus.Successful,
                Message = string.Empty
            };

            report.AddDeletionReportToUpdateItemStatus((int)CacheNodeType.Item, GetExecuteActionForJobDetail(), pending.FileUniqueId, pending.ShouldDeleteObjectTotalSize);
            mConfig.ProgressDto?.UpdateProgress();
        }

        private void FallbackPendingDocumentDeletion(PendingDocumentDeletion pending)
        {
            try
            {
                var fallbackFile = Web.GetFile(pending.DocId, pending.FileServerRelativeUrl);
                if (mConfig.currentRule.DeleteToRecycleBin)
                {
                    fallbackFile.Recycle();
                }
                else
                {
                    fallbackFile.Delete();
                }

                FinalizePendingDocumentDeletion(pending);
            }
            catch (Exception ex)
            {
                ApprovedDatasSqliteHelper.UpdateStatus(pending.DocId, (int)ProcessedStatus.Failed);
                mLog.Error($"Fallback document deletion failed. File:{pending.FileServerRelativeUrl}. Message:{ex}");

                var report = new ReportInfo(mConfig)
                {
                    Url = pending.ReportUrl,
                    SubJobId = pending.SubJobId,
                    RuleName = pending.RuleName,
                    MediaName = pending.MediaName,
                    Size = pending.ShouldDeleteObjectTotalSize,
                    Status = JobDetailsStatus.Failed,
                    Message = ex.Message
                };

                report.ExceptionTackle(ex.Message, SPNodeLevel.Document.ToString());
                report.AddDeletionReportToUpdateItemStatus((int)CacheNodeType.Item, GetExecuteActionForJobDetail(), pending.FileUniqueId, pending.ShouldDeleteObjectTotalSize);
                mConfig.ProgressDto?.UpdateProgress();
            }
        }

        private void DisposePendingDocumentDeleteContext()
        {
            pendingDocumentDeleteWeb = null;
            pendingDocumentDeleteContext?.Dispose();
            pendingDocumentDeleteContext = null;
        }

        [SuppressMessage("FxCopCustomRules", "C100003:DoNotUseSpecificSPMethod")]
        public void DeleteDocument()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.DeleteDocument"))
            {
                Guid fileUniqueId = Guid.Empty;
                bool shouldReport = true;
                Guid docId = Guid.Empty;
                long shouldDeleteObjectTotalSize = 0;
                int archiveLevel = GetArchiveLevel();
                try
                {
                    if (mHeaderInfo.Attributes[KeyWord.SYSTEMFILE] != null)
                    {
                        var isSystemFileStr = mHeaderInfo.Attributes[KeyWord.SYSTEMFILE].Value;
                        if (isSystemFileStr.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                        {
                            mLog.Info($"Skip delete SYSTEMFILE: {mReportInfo.Url}");
                            mReportInfo.Status = JobDetailsStatus.Skipped;
                            return;
                        }
                    }


                    if (mConfig?.RMDiscoveryOptimizationSetting?.MS365DataType == (int)MS365DataType.Phl)
                    {
                        if (List.BaseTemplate != AveListTemplateType.PreservationHoldLibrary
                        || (List.Title != "Preservation Hold Library" && List?.RootFolder?.Url?.EndsWith("PreservationHoldLibrary", StringComparison.OrdinalIgnoreCase) != true))
                        {
                            mLog.Info($"Skip delete un phl list file {mReportInfo.Url}");
                            mReportInfo.Status = JobDetailsStatus.Skipped;
                            return;
                        }
                    }
                    else if (List.BaseTemplate == AveListTemplateType.DesignCatalog
                        || List.BaseTemplate == AveListTemplateType.MasterPageCatalog
                        || (List.BaseTemplate == AveListTemplateType.WebPageLibrary && !(mIsCSDTenant && DataCenterUtil.Is21V()))
                        || List.BaseTemplate == AveListTemplateType.ThemeCatalog
                        || List.Hidden || List.IsCatalog)
                    {
                        mLog.Info($"Skip delete system file {mReportInfo.Url}");
                        mReportInfo.Status = JobDetailsStatus.Skipped;
                        return;
                    }

                    docId = new Guid(mHeaderInfo.Attributes[KeyWord.ID].Value);
                    string realFileUrl = mReportInfo.Url;
                    mLog.Info("Begin delete document.Document id:{0}.", docId);
                    if (mConfig.currentRule != null
                        && (mConfig.currentRule.KeepDataOption & (int)KeepDataOption.DeleteOnly) == (int)KeepDataOption.DeleteOnly
                        && !string.IsNullOrEmpty(mConfig.siteUrlSchemeAndHost)
                        && realFileUrl.StartsWith(mConfig.siteUrlSchemeAndHost))
                    {
                        mLog.Info($"Current file url is fullurl and get file ServerRelativeUrl.FileId:{docId}.");
                        realFileUrl = realFileUrl.Substring(mConfig.siteUrlSchemeAndHost.TrimEnd('/').Length);
                        if (!realFileUrl.StartsWith("/"))
                        {
                            realFileUrl = "/" + realFileUrl;
                            mLog.Warn($"Current file realFileUrl not StartsWith slash and add slash.FileId:{docId}.realFileUrl:{realFileUrl}.");
                        }
                    }
                    IAveFile file = Web.GetFile(docId, realFileUrl);
                    shouldDeleteObjectTotalSize = GetFileTotalSize(file);
                    fileUniqueId = file.UniqueId;
                    #region file not exist
                    if (!file.Exists)
                    {
                        mLog.Info($"Current file doesn't exist.FileID:{docId}.File ServerRelativeUrl:{realFileUrl}. File FullURL:{mReportInfo.Url}.");
                        try
                        {
                            if (List.BaseTemplate == AveListTemplateType.PictureLibrary)
                            {
                                try
                                {
                                    string fileName = mHeaderInfo.Attributes[KeyWord.URL].Value.Substring(mHeaderInfo.Attributes[KeyWord.URL].Value.IndexOf("\\") + 1);
                                    string folderUrl = realFileUrl.Substring(0, realFileUrl.IndexOf(fileName) - 1);
                                    if ((folderUrl.EndsWith("/_t", StringComparison.OrdinalIgnoreCase) || folderUrl.EndsWith("/_w", StringComparison.OrdinalIgnoreCase)))
                                    {
                                        //ADO-164699 Picture Library 上传文件会自动生成相应的image，这类文件不显示在job detail里.
                                        mLog.Info("Current List is Picture Library,File has deleted.");
                                        shouldReport = false;
                                    }
                                    return;
                                }
                                catch (Exception ue)
                                {
                                    mLog.Info($"Init url analyse failed {ue}");
                                }
                            }
                            if (file.InDocumentLibrary)
                            {
                                if (!TakeOverCheckOutFile(Web, file, docId))
                                {
                                    if (mConfig.Action == ArchiverAction.RELATIVEDATA_ARCHIVER_BACKUP_JOB_REQUEST)
                                    {
                                        mLog.Info("Current file doesn't exist in SharePoint while RELATIVEDATA_ARCHIVER_BACKUP_JOB_REQUEST DeleteDocument.File Id:{0}.", docId);
                                        mReportInfo.Status = JobDetailsStatus.Skipped;
                                    }
                                    else if (mConfig.IsILMode && (LinkFileCommon.IsLeaveStubRule(mConfig.currentRule)))
                                    {
                                        mLog.Info("Current file not exists and KeepDataOption is leave stub.");
                                        //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, docId, archiveLevel, mReportInfo.SubJobId);
                                        mReportInfo.Status = JobDetailsStatus.Skipped;
                                        mReportInfo.Message = "RM_JM_GlobalSearch_CannotFindExchangeItem";
                                    }
                                    else
                                    {
                                        mLog.Info($"Current file not exists and skip report.File:{realFileUrl}.");
                                        shouldReport = false; 
                                    }
                                    return;
                                }
                                else
                                {
                                    mLog.Info($"Current file not exists and skip report.File:{realFileUrl}.");
                                    shouldReport = false;
                                }
                            }
                        }
                        catch (FileNotFoundException ex)
                        {
                            mLog.Info(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteDocumentdelete, ex.ToString());
                            if (mConfig.Action == ArchiverAction.RELATIVEDATA_ARCHIVER_BACKUP_JOB_REQUEST)
                            {
                                mReportInfo.Status = JobDetailsStatus.Skipped;
                            }
                            return;
                        }
                    }
                    #endregion
                    if (CheckDocHasModifiedAfterBackup(mHeaderInfo, file))
                    {
                        mReportInfo.Status = JobDetailsStatus.Skipped;
                        mReportInfo.Message = "StorageOptimization_DeleteItemSkip_Modified";
                        return;
                    }
                    if (mConfig.CheckItemIsRecordsHold(docId))
                    {
                        mLog.Warn("File is RecordsHold. File Name: {0}.", file.UniqueId);
                        //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, docId, archiveLevel, mReportInfo.SubJobId);
                        mReportInfo.Status = JobDetailsStatus.Skipped;
                        mReportInfo.Message = "StorageOptimization_EXOExploreHoldFile";
                        //mConfig.ProgressDto.HasErrorNode = true;
                        return;
                    }
                    //ADO-181461 SharePoint自带Folder，Folder.Item对象为空.
                    if (mConfig.BackgroundSettings.SkipExtentionName.Exists(f => file.Name.EndsWith(f, StringComparison.OrdinalIgnoreCase)))
                    {
                        mLog.Warn("Can not delete this document,because document it may be config keep file or system file.FileUrl: {0}.", realFileUrl);
                        //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Archived, docId, archiveLevel, mReportInfo.SubJobId);
                        TagDiscoverOptimizationData(file.UniqueId.ToString());
                        shouldReport = false;
                        JobExecutionProgressStatisticExecutor.Instance.DecreaseTotalMatchedRuleFiles();
                        return;
                    }
                    if (file.Item != null && !CheckItemModifyTime((DateTime)file.TimeLastModified))
                    {
                        mLog.Warn("Can not delete this document,because document last modify time has changed. File Id: {0}.Modified:{1}.TimeLastModified:{2}.", file.UniqueId, file.Item["Modified"].ToString(), file.TimeLastModified.ToString());
                        mReportInfo.Status = JobDetailsStatus.Skipped;
                        mReportInfo.Message = "StorageOptimization_DeleteItemSkip_Modified";
                        mReportInfo.ExceptionTackle(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteDocumentModified, SPNodeLevel.Document.ToString());
                        //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, docId, archiveLevel, mReportInfo.SubJobId);
                        return;
                    }
                    if (!mBackupDeleteLowLevelStatus)//判断是否有备份失败的version
                    {
                        mLog.Warn("Can not delete this document,because archive document version has failed. File Id: {0}.", file.UniqueId);
                        mReportInfo.SetFailedInfo(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteDocumentversionfailed);
                        //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, docId, archiveLevel, mReportInfo.SubJobId);
                        return;
                    }
                    if (!CheckRelatedObjectStatus(file.Name, docId, archiveLevel, file.Item))
                    {
                        return;
                    }
                    if (DeleteRelatedObjectForDeleteOnlyAction(docId, file.Item, mConfig.currentRule.RelatedRecordOption == RelatedRecordOption.Both ? 1 : 0))
                    {
                        mLog.Info("Has related ojbect delete failed.");
                        mReportInfo.SetFailedInfo("StorageOptimization13_SOARRelatedRecordDeleteFailed");
                        //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, docId, archiveLevel, mReportInfo.SubJobId);
                        mConfig.JobReportDto.HasErrorNode = true;
                        return;
                    }
                    string md5 = string.Empty;
                    ListItemComplianceInfo complianceInfo = null;
                    bool needRestoreComplianceTag = false;
                    try
                    {
                        GetComplianceTagIfEnableRemove(file.Item, out complianceInfo);
                        if (!mConfig.currentRule.LockRecordBeforeDestroy &&
                            complianceInfo != null && complianceInfo.TagPolicyHold && !complianceInfo.TagPolicyRecord &&
                            IsRecordTypeComplianceTag(Site, complianceInfo.ComplianceTag))
                        {
                            mLog.Info("skip Delete current unlock status item. Item Name: {0}.", file.Item.Name);
                            mReportInfo.Status = JobDetailsStatus.Skipped;
                            mReportInfo.Message = "StorageOptimization_Skip_Unlock_Status_Item";
                            return;
                        }
                        if (!mConfig.currentRule.IsLeaveStubRemoveMetadata
                            && LinkFileCommon.IsLeaveStubRule(mConfig.currentRule))
                        {
                            LinkDocumentPackagingAsync(file, docId, archiveLevel, mReportInfo.Size + versionSize).Wait();
                            shouldReport = false;
                            return;
                        }
                        else
                        {
                            //理论上，OPUS目前走不到使用备份还原方式创建stub
                            LinkDocumentAsync(file, docId, archiveLevel).Wait();
                        }
                        object ItemHoldRecordStatus = null;
                        bool isRecord = ScheduleConfiguration.CheckisRecord(file.Item);
                        if (isRecord && mConfig.currentRule.DeleteRecords)
                        {
                            mLog.Info("start to try undeclare data");
                            Record.UndeclareItemAsRecord(file.Item);
                            ItemHoldRecordStatus = file.Item.FieldValues["_vti_ItemHoldRecordStatus"];
                            mLog.Info($"start to try undeclare data,ItemHoldRecordStatus:{ItemHoldRecordStatus?.ToString()}");
                            file.Item.FieldValues["_vti_ItemHoldRecordStatus"] = null;
                        }
                        DeleteComplianceTagIfEnableRemove(file.Item, complianceInfo, out needRestoreComplianceTag);
                        RemoveRelatedRelationship(file.Item, file.ServerRelativeUrl);
                        CheckRetentionlabel(file);
                        if (mConfig.IsILMode && mConfig.actionType == ActionType.ArchchiveToStorage)
                        {
                            var parentFolder = GetCurrentAveBackupFolder(file.ParentFolder);
                            md5 = LinkFileCommon.GetDocumnetPathMD5(file.Web.Site.Url, parentFolder.Path, file.Name);
                        }
                        string fileId = file.UniqueId.ToString();
                        if ((mConfig.currentRule.KeepDataOption & (int)KeepDataOption.DeleteOnly) == (int)KeepDataOption.DeleteOnly)
                        {
                            //Delete Data from SharePoint
                            if (!((mConfig.currentRule.KeepDataOption & (int)KeepDataOption.KeepLatestVersion) == (int)KeepDataOption.KeepLatestVersion))
                            {
                                mReportInfo.Size = shouldDeleteObjectTotalSize;
                                
                                SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(GetFileTotalSize(file), mReportInfo.Url);
                                if (TryQueuePendingDocumentDeletion(file, docId, realFileUrl, shouldDeleteObjectTotalSize, md5, null, false))
                                {
                                    needRestoreComplianceTag = false;
                                    shouldReport = false;
                                    return;
                                }

                                if (mConfig.currentRule.DeleteToRecycleBin)
                                {
                                    file.Recycle();
                                }
                                else
                                {
                                    file.Delete();
                                }
                                needRestoreComplianceTag = false;
                                mLog.Info("DeleteOnly.Delete Document Success.File Id:{0}", file.UniqueId);
                            }
                            //Delete All Versions from SharePoint
                            else if ((mConfig.currentRule.KeepDataOption & (int)KeepDataOption.KeepLatestVersion) == (int)KeepDataOption.KeepLatestVersion && mConfig.currentRule.KeepLatestMajorAndMinorVersion == 0)
                            {
                                mLog.Info($"Before Delete Document All Version.File Id:{file.UniqueId}.Version Count:{file.Versions.Count}.File Total Size:{shouldDeleteObjectTotalSize}.Version Number:{GetFileVersionString(file.Versions)}.");
                                SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(GetFileVersionsTotalRealSize(file), mReportInfo.Url);
                                if (mConfig.currentRule.DeleteToRecycleBin)
                                {
                                    file.RecycleVersionsByIds(GetFileVersionIds(file.Versions));
                                }
                                else
                                {
                                    file.DeleteAllVersion();
                                }
                                file = Web.GetFile(docId, realFileUrl);
                                shouldDeleteObjectTotalSize = shouldDeleteObjectTotalSize - GetFileTotalSize(file);
                                mReportInfo.Size = shouldDeleteObjectTotalSize;
                                SetComplianceTagIfEnableRemove(file.Item, complianceInfo);
                                needRestoreComplianceTag = false;
                                if (isRecord && mConfig.currentRule.DeleteRecords)
                                {
                                    Record.DeclareItemAsRecord(file.Item);
                                    file.Item.FieldValues["_vti_ItemHoldRecordStatus"] = ItemHoldRecordStatus;
                                }
                                mLog.Info($"Delete Document All Version Success.File Id:{file.UniqueId}.Remain Version Count:{file.Versions.Count}.Remain File Total Size:{GetFileTotalSize(file)}.Delete Total Size:{shouldDeleteObjectTotalSize}.Remain Version Number:{GetFileVersionString(file.Versions)}.");
                            }
                            //Keep Latest xxx version in SharePoint
                            else
                            {
                                KeepXLatestMajorAndMinorVersionAndDeleteOthersVersion(file);
                                shouldReport = false;
                                SetComplianceTagIfEnableRemove(file.Item, complianceInfo);
                                needRestoreComplianceTag = false;
                                if (isRecord && mConfig.currentRule.DeleteRecords)
                                {
                                    Record.DeclareItemAsRecord(file.Item);
                                    file.Item.FieldValues["_vti_ItemHoldRecordStatus"] = ItemHoldRecordStatus;
                                }
                            }
                            mLog.Info("DeleteOnly.Delete Document Success.File name:{0}", file.UniqueId);
                            DeleteEmptyFolder(file);
                            UpdateExploreDB(docId, mConfig.actionType == ActionType.ArchchiveToStorage ? 8 : 2, pathMd5: md5);
                        }
                        else
                        {
                            var record = GetRecordInExplorerDao(docId);
                            DestructionReport destructionReport = null;
                            if (mConfig.IsOneDriverSite && record != null)
                            {
                                destructionReport = GetOnedriveDestructionReport(file.Item, record.TermName, record.RecordsId);
                            }
                            else
                            {
                                destructionReport = GetDestructionReport(file.Item);
                            }
                            if (!mConfig.IsRelativeDataJob || SOArchiverJobInfoStatistics.Instance.IsDeleteArchiveAction)
                            {
                                long size = GetFileTotalSize(file);
                                if (SOArchiverJobInfoStatistics.Instance.IsDeleteArchiveAction)
                                {
                                    mLog.Info($"this is relativeData data and is delete only action,Delete Document Success,is relativeData job:{mConfig.IsRelativeDataJob},is delete only:{SOArchiverJobInfoStatistics.Instance.IsDeleteArchiveAction},size:{size}");
                                }
                                SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(size, mConfig.GetNodeFullPath(mReportInfo.Url), GenerateDiscoveryOptimizationFileReport(file));
                            }
                            mReportInfo.Size = shouldDeleteObjectTotalSize;
                            if ((mConfig.currentRule.KeepDataOption & (int)KeepDataOption.NotBackup) == (int)KeepDataOption.NotBackup && mConfig.currentRule.DeleteToRecycleBin)
                            {
                                if (TryQueuePendingDocumentDeletion(file, docId, realFileUrl, shouldDeleteObjectTotalSize, md5, destructionReport, true))
                                {
                                    needRestoreComplianceTag = false;
                                    shouldReport = false;
                                    return;
                                }

                                file.Recycle();
                            }
                            else
                            {
                                if (TryQueuePendingDocumentDeletion(file, docId, realFileUrl, shouldDeleteObjectTotalSize, md5, destructionReport, true))
                                {
                                    needRestoreComplianceTag = false;
                                    shouldReport = false;
                                    return;
                                }

                                file.Delete();
                            }
                            needRestoreComplianceTag = false;
                            mLog.Info("Delete Document Success.File name:{0}", file.UniqueId);
                            DeleteEmptyFolder(file);
                            UpdateRecordInExploreDB(record, docId, mConfig.actionType == ActionType.ArchchiveToStorage ? 8 : 2, pathMd5: md5);
                            AddToDestructionCache(destructionReport);
                        }
                        TagDiscoverOptimizationData(fileId);
                        //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Archived, docId, archiveLevel, mReportInfo.SubJobId);
                    }
                    catch (RALeaveStubException ex)
                    {
                        ApprovedDatasSqliteHelper.UpdateStatus(docId, (int)ProcessedStatus.Failed);
                        mLog.Warn($"An error occur while leave stub {file.ServerRelativeUrl}. Message:{ex.ToString()}.");
                        throw;
                    }
                    catch (StubNameConflictException snce)
                    {
                        ApprovedDatasSqliteHelper.UpdateStatus(docId, (int)ProcessedStatus.Failed);
                        mLog.Warn($"An error occur while leave stub {file.ServerRelativeUrl}. StubNameConflictException Message:{snce.ToString()}.");
                        throw;
                    }
                    catch (Exception ex)
                    {
                        mLog.Info("File Delete Error: {0} error message: {1}", file.ServerRelativeUrl, ex.ToString());
                        ApprovedDatasSqliteHelper.UpdateStatus(docId, (int)ProcessedStatus.Failed);
                        if (ex.Message != null && ex.Message.Contains("This library contains items that have been modified or deleted but must remain available due to eDiscovery holds. Items cannot be modified or removed"))
                        {
                            mReportInfo.SetFailedInfo("StorageOptimization_SOPhlLibHoldDocumentDeleteFailed");
                            mLog.Warn("[ArchiverDeletion][DeleteDocument]This library contains items that have been modified or deleted but must remain available due to eDiscovery holds. Items cannot be modified or removed.");
                            mConfig.JobReportDto.HasErrorNode = true;
                            return;
                        }

                        IAveListItem listItem = List.GetItemByUniqueId(docId);
                        IAveFolder rootFolder = Web.RootFolder;
                        #region delete declare document for office 365
                        if (ScheduleConfiguration.CheckisRecord(listItem) && mConfig.currentRule.DeleteRecords
                            || (mConfig.IsILMode && ArchiverCommonStaticMethod.IsBlockDeleteOnlyRecord(listItem)))
                        {
                            mLog.Info("Office365 Begin CheckFileIsRecord");
                            if (!ArchiverCommonStaticMethod.CheckIsHoldOnly(listItem))
                            {
                                mLog.Info("This File is Declare File.UniqueId:{0}", file.UniqueId);
                                Record.UndeclareItemAsRecord(listItem);
                                //undeclare file need reload file.
                                file = Web.GetFile(docId, realFileUrl);
                                var record = GetRecordInExplorerDao(docId);
                                DestructionReport destructionReport = null;
                                if (mConfig.IsOneDriverSite && record != null)
                                {
                                    destructionReport = GetOnedriveDestructionReport(file.Item, record.TermName, record.RecordsId);
                                }
                                else
                                {
                                    destructionReport = GetDestructionReport(file.Item);
                                }
                                file.Delete();
                                needRestoreComplianceTag = false;
                                UpdateRecordInExploreDB(record, docId, mConfig.actionType == ActionType.ArchchiveToStorage ? 8 : 2, pathMd5: md5);
                                AddToDestructionCache(destructionReport);
                                //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Archived, docId, archiveLevel, mReportInfo.SubJobId);
                                return;
                            }
                            else
                            {
                                mLog.Warn("This File is Declare And Hold File.UniqueId:{0}", file.UniqueId);
                                mConfig.JobReportDto.HasErrorNode = true;
                                //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, docId, archiveLevel, mReportInfo.SubJobId);
                                mReportInfo.SetFailedInfo("Hold File");//to do
                                return;
                            }
                        }
                        #endregion
                        #region check out/lock/label file.
                        if (ex.InnerException != null
                            && (ex.InnerException.Message.Contains("is checked out for editing by")
                            || ex.InnerException.Message.Contains("est extrait pour modification par")//法语
                            || ex.InnerException.Message.Contains("został wyewidencjonowany do edycji przez użytkownika")//波兰语
                            || ex.InnerException.Message.Contains("извлечен для редактирования пользователем")//俄语
                            || ex.InnerException.Message.Contains("Foi feito o check-out para edição do arquivo") || ex.InnerException.Message.Contains("Foi dada saída ao ficheiro")//葡萄牙语
                            || ex.InnerException.Message.Contains("zur Bearbeitung ausgecheckt")//德语
                            || file.CheckOutType != AveCheckOutType.None
                            ))
                        {
                            mLog.Info($"Current file is check out file and Records check in and delete.FileName:{file.Name}.checkOutType:{file.CheckOutType}");
                            file.CheckIn("");
                            var record = GetRecordInExplorerDao(docId);
                            DestructionReport destructionReport = null;
                            if (mConfig.IsOneDriverSite && record != null)
                            {
                                destructionReport = GetOnedriveDestructionReport(file.Item, record.TermName, record.RecordsId);
                            }
                            else
                            {
                                destructionReport = GetDestructionReport(file.Item);
                            }
                            file.Delete();
                            needRestoreComplianceTag = false;
                            mLog.Info("Delete check out document success.File name:{0}", file.Name);
                            UpdateRecordInExploreDB(record, docId, mConfig.actionType == ActionType.ArchchiveToStorage ? 8 : 2, pathMd5: md5);
                            AddToDestructionCache(destructionReport);
                            //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Archived, docId, archiveLevel, mReportInfo.SubJobId);
                            return;
                        }
                        #region Item cannot be deleted while on hold
                        if (ex.InnerException != null
                            && (ex.InnerException.Message.Contains("Item cannot be deleted while on hold.")))
                        {
                            mLog.Info("Current file is is hold file.FileName:{0}. will retry.", file.Name);
                            Thread.Sleep(500);
                            file = Web.GetFile(docId, realFileUrl);
                            var record = GetRecordInExplorerDao(docId);
                            DestructionReport destructionReport = null;
                            if (mConfig.IsOneDriverSite && record != null)
                            {
                                destructionReport = GetOnedriveDestructionReport(file.Item, record.TermName, record.RecordsId);
                            }
                            else
                            {
                                destructionReport = GetDestructionReport(file.Item);
                            }
                            file.Delete();
                            needRestoreComplianceTag = false;
                            mLog.Info("Delete hold file success.File name:{0}", file.Name);
                            UpdateRecordInExploreDB(record, docId, mConfig.actionType == ActionType.ArchchiveToStorage ? 8 : 2, pathMd5: md5);
                            AddToDestructionCache(destructionReport);
                            return;
                        }
                        #endregion
                        if (ex.InnerException != null
                            && (ex.InnerException.Message.Contains("is locked for shared use by")
                            || ex.InnerException.Message.Contains("Impossible de modifier les propriétés d'un document lorsqu'il est extrait et modifié hors connexion")
                            || ex.InnerException.Message.Contains("est verrouillé pour une utilisation partagée par")//法语
                            || ex.InnerException.Message.Contains("на редагування або заблокував його для редагування") || ex.InnerException.Message.Contains("заблоковано для спільного використання")//乌克兰语
                            || ex.InnerException.Message.Contains("bloqueado para su edición") || ex.InnerException.Message.Contains("ha bloqueado el archivo")))//西班牙语
                        {
                            mLog.Info("Current file is is locked file.FileName:{0}.", file.Name);
                            mReportInfo.SetFailedInfo("StorageOptimization13_SOARDeleteOfficeLockFile");
                            mConfig.JobReportDto.HasErrorNode = true;
                            //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, docId, archiveLevel, mReportInfo.SubJobId);
                            return;
                        }

                        if (ex.InnerException != null
                            && (ex.InnerException.Message.Contains("The label that's applied to this item prevents it from being edited or deleted")
                            || ex.InnerException.Message.Contains("Die auf dieses Element angewendete Bezeichnung verhindert, dass es bearbeitet oder gelöscht werden kann.")))//德语
                        {
                            mLog.Info("Current file is is label file.FileName:{0}.", file.Name);
                            if (mConfig.IsILMode
                                && file.Item.Fields.ContainsField("_ComplianceTag")
                                && RecordsDBOperation.RMEXOLabels.Where(
                                    x => x.LabelName == file.Item["_ComplianceTag"].ToString()
                                    && x.Status == 1 && x.Type == 1).FirstOrDefault() != null)
                            {
                                mLog.Info("Current file is label file and Records remove label and delete.FileName:{0}.", file.Name);
                                //file.Item.SetComplianceTag(string.Empty, false, false, false, false);
                                file.Item.SetComplianceTagOnBulkItems(string.Empty);
                                file = Web.GetFile(docId, realFileUrl);
                                var record = GetRecordInExplorerDao(docId);
                                DestructionReport destructionReport = null;
                                if (mConfig.IsOneDriverSite && record != null)
                                {
                                    destructionReport = GetOnedriveDestructionReport(file.Item, record.TermName, record.RecordsId);
                                }
                                else
                                {
                                    destructionReport = GetDestructionReport(file.Item);
                                }
                                file.Delete();
                                needRestoreComplianceTag = false;
                                mLog.Info("Delete label file success.File name:{0}", file.Name);
                                UpdateRecordInExploreDB(record, docId, mConfig.actionType == ActionType.ArchchiveToStorage ? 8 : 2, pathMd5: md5);
                                AddToDestructionCache(destructionReport);
                                //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Archived, docId, archiveLevel, mReportInfo.SubJobId);
                                return;
                            }
                            else
                            {
                                mReportInfo.SetFailedInfo("StorageOptimization_SOARRecordManagerLabelDocumentDeleteFailed");
                                mLog.Warn("[ArchiverDeletion][DeleteDocument]Delete label file failed.");
                                mConfig.JobReportDto.HasErrorNode = true;
                                //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, docId, archiveLevel, mReportInfo.SubJobId);
                                return;
                            }
                        }

                        if (rootFolder.WelcomePage.Equals(listItem.Url, StringComparison.OrdinalIgnoreCase))
                        {
                            DeleteRootFolderFile(Web, docId);
                            //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Archived, docId, archiveLevel, mReportInfo.SubJobId);
                            return;
                        }
                        #endregion
                        #region Error Code: 6404
                        string errorMessage = ex.ToString();
                        if (errorMessage.Contains("Code d'erreur : 6404")//法语
                            || errorMessage.Contains(": 6404")
                            || errorMessage.Contains("Error Code: 6404")
                            || errorMessage.Contains("Код ошибки: 6404")//俄语
                            || errorMessage.Contains("Impossible de supprimer le fichier")
                            || errorMessage.Contains("Cannot remove file"))
                        {
                            mLog.Info("Delete file has 6404 error and need reget file.File url:{0}.", realFileUrl);
                            AveTaskRetryHelper retryHelper = new AveTaskRetryHelper(3, true);
                            DestructionReport destructionReport = null;
                            var record = GetRecordInExplorerDao(docId);
                            retryHelper.ExecuteWithRetryMechanism(() =>
                            {
                                file = mConfig.aveObjectModelFactory.CreateSite(Site.Url).OpenWeb(Web.ID).GetFile(docId, realFileUrl);
                                if (file.Exists)
                                {
                                    if (mConfig.IsOneDriverSite && record != null)
                                    {
                                        destructionReport = GetOnedriveDestructionReport(file.Item, record.TermName, record.RecordsId);
                                    }
                                    else
                                    {
                                        destructionReport = GetDestructionReport(file.Item);
                                    }
                                    file.Delete();
                                    needRestoreComplianceTag = false;
                                }
                                else
                                {
                                    mLog.Info("Current file:{0} doesn't exist when Error Code: 6404.", realFileUrl);
                                }
                            });
                            mLog.Info("Delete 6404 error file success.File name:{0}", file.Name);
                            UpdateRecordInExploreDB(record, docId, mConfig.actionType == ActionType.ArchchiveToStorage ? 8 : 2, pathMd5: md5);
                            AddToDestructionCache(destructionReport);
                            //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Archived, docId, archiveLevel, mReportInfo.SubJobId);
                            return;
                        }
                        #endregion
                        if (ex.Message != null && ex.Message.Contains("This item cannot be updated because it is locked as read-only"))
                        {
                            mReportInfo.SetFailedInfo("StorageOptimization13_SOARDeleteOfficeLockFile");
                            mLog.Warn("[ArchiverDeletion][DeleteDocument]This item cannot be updated because it is locked as read-only.");
                            mConfig.JobReportDto.HasErrorNode = true;
                            return;
                        }
                        else if (ex.Message != null && ex.Message.Contains("The label that's applied to this item prevents it from being edited or deleted. Check the item's label for more details"))
                        {
                            mReportInfo.SetFailedInfo("StorageOptimization_SOARRecordManagerLabelDocumentDeleteFailed");
                            mLog.Warn("[ArchiverDeletion][DeleteDocument]The label that's applied to this item prevents it from being edited or deleted. Check the item's label for more details.");
                            mConfig.JobReportDto.HasErrorNode = true;
                            return;
                        }

                        throw;
                    }
                    finally
                    {
                        if (needRestoreComplianceTag)
                        {
                            SetComplianceTagIfEnableRemove(file?.Item, complianceInfo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ApprovedDatasSqliteHelper.UpdateStatus(docId, (int)ProcessedStatus.Failed);
                    mLog.Error(" An error occurred while deleting document:{0}.Message:{1}.", mReportInfo.Url, ex.ToString());
                    //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, docId, archiveLevel, mReportInfo.SubJobId);
                    // need to validate again for the checkIn in the child exception
                    if (ex.Message != null && ex.Message.Contains("This item cannot be updated because it is locked as read-only"))
                    {
                        mReportInfo.SetFailedInfo("StorageOptimization13_SOARDeleteOfficeLockFile");
                        mLog.Warn("[ArchiverDeletion][DeleteDocument]This item cannot be updated because it is locked as read-only.");
                        mConfig.JobReportDto.HasErrorNode = true;
                        return;
                    }
                    mReportInfo.ExceptionTackle(ex.Message, SPNodeLevel.Document.ToString());
                }
                finally
                {
                    mBackupDeleteLowLevelStatus = true;//reset backup version status
                    if (shouldReport)
                    {
                        if (mConfig.Action == ArchiverAction.RELATIVEDATA_ARCHIVER_BACKUP_JOB_REQUEST)
                        {
                            mReportInfo.AddRelativeDataArchiverReport(SPNodeLevel.Item, (mConfig.currentRule.KeepDataOption & (int)KeepDataOption.LinkDocument) == (int)KeepDataOption.LinkDocument);
                        }
                        else
                        {
                            mReportInfo.AddDeletionReportToUpdateItemStatus((int)CacheNodeType.Item, GetExecuteActionForJobDetail(), fileUniqueId, shouldDeleteObjectTotalSize);
                        }
                        versionSize = 0;
                    }
                    mConfig.ProgressDto?.UpdateProgress();
                }
            }
        }
        private bool isAOSPJob(JobType jobtype) => jobtype == JobType.AOSPRestore ||
                                                   jobtype == JobType.DiscoveryAOSPJob ||
                                                   jobtype == JobType.DiscoveryAOSPOptimization ||
                                                   jobtype == JobType.DiscoveryAOSPOptimizationCalculate;
        private bool CheckDocHasModifiedAfterBackup(XmlElement mHeaderInfo, IAveFile file)
        {
            try
            {
                DateTime modifiedTime = (DateTime)file.Item.FieldValues["Modified"];
                long archiverModifiedTime = Convert.ToInt64(mHeaderInfo.Attributes["Modified"].Value);
                if (archiverModifiedTime > 0 && archiverModifiedTime < modifiedTime.Ticks)
                {
                    mLog.Warn($"current doc has modifed,can not deleted it,archiver modified time:{archiverModifiedTime},modfied time:{modifiedTime.Ticks}");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                mLog.Error($"CheckDocHasModifiedAfterBackup failed, error:{ex}");
                return true;
            }
        }
        private bool CheckItemHasModifiedAfterBackup(XmlElement mHeaderInfo, IAveListItem item)
        {
            try
            {
                DateTime modifiedTime = (DateTime)item.FieldValues["Modified"];
                long archiverModifiedTime = Convert.ToInt64(mHeaderInfo.Attributes["Modified"].Value);
                long modifiedTimeToleranceTicks = TimeSpan.FromSeconds(5).Ticks;
                if (archiverModifiedTime > 0 && archiverModifiedTime + modifiedTimeToleranceTicks < modifiedTime.Ticks)
                {
                    mLog.Warn($"current item has modifed,can not deleted it,archiver modified time:{archiverModifiedTime},modfied time:{modifiedTime.Ticks}");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                mLog.Error($"CheckItemHasModifiedAfterBackup failed, error:{ex}");
                return true;
            }
        }
        private void TagDiscoverOptimizationData(string fileId)
        {
            if (mConfig.IsDiscoverOptimization && mConfig.currentRule.PolicyLevel == PolicyLevel.Document)
            {
                try
                {
                    if (isAOSPJob(mConfig.jobtype))
                    {
                        mLog.Info("This is AOSP job, use AOSP Scanner tag.");
                        DiscoveryAOSPScanner.Instance.TagAsArchivedAsync(fileId).GetAwaiter().GetResult();
                    }
                    else
                    {
                        DiscoverScanner.TagAsArchivedAsync(fileId).GetAwaiter().GetResult();
                    }
                }
                catch (Exception e)
                {
                    mLog.Error($"TagAsArchived file failed,file url:{mReportInfo.Url},will retry:10s,error:{e}");
                    Thread.Sleep(1000 * 10);
                    try
                    {
                        if (isAOSPJob(mConfig.jobtype))
                        {
                            mLog.Info("This is AOSP job, use AOSP Scanner retag.");
                            DiscoveryAOSPScanner.Instance.TagAsArchivedAsync(fileId).GetAwaiter().GetResult();
                        }
                        else
                        {
                            DiscoverScanner.TagAsArchivedAsync(fileId).GetAwaiter().GetResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Error($"retry TagAsArchived file failed,file url:{mReportInfo.Url},error:{ex}");
                    }
                }
            }
        }
        private RMDiscoveryOptimizationFileReport GenerateDiscoveryOptimizationFileReport(IAveFile file)
        {
            RMDiscoveryOptimizationFileReport report = new RMDiscoveryOptimizationFileReport();
            try
            {
                report.AuthorID = file.Author.ID;
                report.AuthorEmail = file.Author.Email;
                report.ModifiedID = file.ModifiedBy.ID;
                report.ModifiedEmail = file.ModifiedBy.Email;
                report.CreateTime = file.TimeCreated.Year.ToString() + file.TimeCreated.Month.ToString("00");
                report.ModifiedTime = file.TimeLastModified.Year.ToString() + file.TimeLastModified.Month.ToString("00");
                report.VersionCount = file.Versions.Count;
            }
            catch (Exception e)
            {
                mLog.Error($"there is some thing wrong when generat delete report,error:{e.ToString()}");
            }
            return report;
        }
        private string GetFileVersionString(IAveFileVersionCollection versions)
        {
            string fileVersionString = string.Empty;
            if (versions != null)
            {
                try
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var version in versions)
                    {
                        stringBuilder.Append(version.ID + ";");
                    }
                    fileVersionString = stringBuilder.ToString();
                }
                catch (Exception ex)
                {
                    mLog.Warn($"GetFileVersionString Exception.Message:{ex}.");
                }
            }
            return fileVersionString;
        }
        private List<int> GetFileVersionIds(IAveFileVersionCollection versions)
        {
            List<int> versionIds = new List<int>();
            if (versions != null)
            {
                try
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var version in versions)
                    {
                        if (!version.IsCurrentVersion)
                        {
                            versionIds.Add(version.ID);
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLog.Warn($"GetFileVersionIds Exception.Message:{ex}.");
                }
            }
            return versionIds;
        }
        private void KeepXLatestMajorAndMinorVersionAndDeleteOthersVersion(IAveFile file)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.KeepXLatestMajorAndMinorVersionAndDeleteOthersVersion"))
            {
                int currentVersion = file.UIVersion;
                Dictionary<int, VersionInfo> needDeleteVersionIds = new Dictionary<int, VersionInfo>();
                long shouldDeleteVersionTotalSize = 0;
                foreach (IAveFileVersion tmpVersion in file.Versions)
                {
                    if (CheckCurrentVersionShouldDelete(file, tmpVersion.ID))
                    {
                        //tmpVersion.Recycle();
                        SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(tmpVersion.Size, mReportInfo.Url);
                        mLog.Info($"Need Delete current version:{tmpVersion.ID}.File Id:{file.UniqueId}.");
                        needDeleteVersionIds.Add(tmpVersion.ID, new VersionInfo() { VersionLabel = tmpVersion.VersionLabel, Size = tmpVersion.Size });
                        shouldDeleteVersionTotalSize = shouldDeleteVersionTotalSize + tmpVersion.Size;
                    }
                    else
                    {
                        mLog.Info($"Skip Delete current version:{tmpVersion.ID}.File Id:{file.UniqueId}.");
                        JobExecutionProgressStatisticExecutor.Instance.DecreaseTotalMatchedRuleFiles();
                    }
                }
                List<int> failedDeleteVersionIds = new List<int>();
                try
                {
                    //批量删除完全在Wrapper底层Client API中做，保证只实例化一次File以及File.Versions.
                    if (mConfig.currentRule.DeleteToRecycleBin)
                    {
                        var ids = needDeleteVersionIds.Select(i => i.Key).ToList();
                        foreach (var id in ids)
                        {
                            try
                            {
                                file.Versions.RecycleByID(id);
                            }
                            catch (Exception e)
                            {
                                failedDeleteVersionIds.Add(id);
                            }
                        }
                    }
                    else
                    {
                        failedDeleteVersionIds = file.Versions.DeleteByIDs(needDeleteVersionIds.Select(i => i.Key).ToList());
                    }
                    foreach (var versionId in needDeleteVersionIds)
                    {
                        if (failedDeleteVersionIds.Contains(versionId.Key))
                        {
                            continue;
                        }
                        mReportInfo.AddDeleteOnlyVersionReport((int)CacheNodeType.ItemVersion, GetExecuteActionForJobDetail(), versionId.Value.VersionLabel, versionId.Value.Size);
                    }
                    if (failedDeleteVersionIds.Count > 0)
                    {
                        //Version删除Failed后，重新Get一次最新的File，确保获取SP存在的最新Versions
                        file = Web.GetFile(file.UniqueId, file.ServerRelativeUrl);
                        foreach (var versionId in failedDeleteVersionIds)
                        {
                            try
                            {
                                //由于Version是批量删除的，一次批量删除可能失败一部分数据，需要ensure当前这批数据的version在SP中是否存在
                                //a.不存在，证明批量删除成功，添加report即可
                                //b.存在，证明批量删除失败，one by one删除
                                if (file.Versions.Where(v => v.ID == versionId).Count() == 0)
                                {
                                    mReportInfo.AddDeleteOnlyVersionReport((int)CacheNodeType.ItemVersion, GetExecuteActionForJobDetail(), needDeleteVersionIds[versionId].VersionLabel, needDeleteVersionIds[versionId].Size);
                                }
                                else
                                {
                                    if (mConfig.currentRule.DeleteToRecycleBin)
                                    {
                                        file.Versions.RecycleByID(versionId);
                                    }
                                    else
                                    {
                                        file.Versions.DeleteByID(versionId);
                                    }
                                    mLog.Info($"Success Delete current failed version one by one:{versionId}.File Id:{file.UniqueId}.");
                                    mReportInfo.AddDeleteOnlyVersionReport((int)CacheNodeType.ItemVersion, GetExecuteActionForJobDetail(), needDeleteVersionIds[versionId].VersionLabel, needDeleteVersionIds[versionId].Size);
                                }
                            }
                            catch (Exception ex)
                            {
                                mLog.Error($"Failed Delete current failed version one by one:{versionId}.File Id:{file.UniqueId}.Exception:{ex.ToString()}");
                                string errorMessage = ex.InnerException != null && ex.InnerException.Message != null ? ex.InnerException.Message : ex.Message;
                                mConfig.ProgressDto.HasErrorNode = true;
                                mReportInfo.SetFailedInfo(errorMessage);
                                mReportInfo.AddDeleteOnlyVersionReport((int)CacheNodeType.ItemVersion, GetExecuteActionForJobDetail(), needDeleteVersionIds[versionId].VersionLabel, 0);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    mLog.Error($"Have an Exception when Delete version in batches. Exception:{e}.");
                }
                mReportInfo.UpdateItemStatusForDeleteOnlyVersion(file.UniqueId, shouldDeleteVersionTotalSize);
            }
        }

        private bool CheckCurrentVersionShouldDelete(IAveFile file, int currentUIVersion)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.CheckCurrentVersionShouldDelete"))
            {
                int versionSequenceNo = 0;
                int majorVersionSequenceNo = 0;
                int minorOfMajorSequenceNo = 0;
                bool isMajorVersion = false;
                bool isLastMajorVersion = false;
                //Current Version不处理.
                //Current Version/ Publish Major Version，IsCurrentVersion都是true.
                bool isCurrentVersion = false;
                if ((currentUIVersion / 512) == (file.Versions[0].ID / 512))
                {
                    isLastMajorVersion = true;
                }
                IAveFileVersion version = null;
                foreach (IAveFileVersion tmpVersion in file.Versions.OrderByDescending(v => v.ID))
                {
                    isMajorVersion = tmpVersion.ID % 512 == 0;
                    if (tmpVersion.ID == currentUIVersion)
                    {
                        version = tmpVersion;
                        isCurrentVersion = tmpVersion.IsCurrentVersion;
                        break;
                    }
                    if (tmpVersion.IsCurrentVersion)
                    {
                        continue;
                    }
                    if ((tmpVersion.ID / 512 == currentUIVersion / 512) && !isMajorVersion)
                    {
                        ++minorOfMajorSequenceNo;
                    }
                    ++versionSequenceNo;
                    majorVersionSequenceNo += isMajorVersion ? 1 : 0;
                }
                if (version == null)
                {
                    mLog.Info($"Version is null when CheckCurrentVersionShouldDelete.currentUIVersion:{currentUIVersion}.");
                    return false;
                }
                else if (isCurrentVersion)
                {
                    mLog.Info($"Version isCurrentVersion when CheckCurrentVersionShouldDelete.currentUIVersion:{currentUIVersion}.");
                    return false;
                }
                //case PolicyCondition.MajorAndMintorVersions:

                int leaveLastVersionCount = mConfig.currentRule.KeepLatestMajorAndMinorVersion;
                return versionSequenceNo >= leaveLastVersionCount;
            }
        }
        private long GetFileTotalSize(IAveFile aveFile)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.GetFileTotalSize"))
            {
                long fileTotalSize = 0;
                if (aveFile != null && aveFile.Item != null && aveFile.Item.Fields != null && aveFile.Item.FieldValues.ContainsKey("SMTotalSize"))
                {
                    try
                    {
                        string totalSize = aveFile.Item.FieldValues["SMTotalSize"].ToString();
                        if (totalSize.IndexOf(";#") != -1)
                        {
                            totalSize = totalSize.Substring(0, totalSize.IndexOf(";#"));
                        }
                        fileTotalSize = Convert.ToInt64(totalSize);
                    }
                    catch (Exception ex)
                    {
                        mLog.Info($"Can't get file total size.FileId:{aveFile.UniqueId}.Message:{ex}.");
                        fileTotalSize = aveFile.Length;
                    }
                }
                return fileTotalSize;
            }
        }
        private long GetFileVersionsTotalRealSize(IAveFile aveFile,bool includeCurrentVersion=false)
        {
            try
            {
                long result = 0;
                if (aveFile.Versions.Count == 0)
                {
                    return 0;
                }
                else
                {
                    foreach (var version in aveFile.Versions)
                    {
                        if (!includeCurrentVersion)
                        {
                            if (version.IsCurrentVersion)
                            {
                                continue;
                            }
                        }
                        result += version.Size;
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                mLog.Warn($"some thing went wrong went get file versions size,error:{e.ToString()}");
                return 0;
            }
        }
        private void AddToDestructionCache(DestructionReport destructionReport)
        {
            if (destructionReport == null)
            {
                return;
            }
            DestructionFactory.GetInstance(mConfig.SiteCollectionID.ToString(), mConfig.JobId).InsertValueToDB(new List<DestructionReport>() { destructionReport });
        }

        private DestructionReport GetDestructionReport(IAveListItem item)
        {
            try
            {
                DestructionReport destructionReport = new DestructionReport()
                {
                    NodeId = item.UniqueId.ToString(),
                    ArchivedTime = DateTime.UtcNow.Ticks,
                    RuleID = new Guid(mConfig.currentRule.Id),
                    SortTicks = Snowflake.Instance().GetTicks().ToString(),
                    JsonMeta = GetJsonMeta(item),
                    ListId = item.ParentList.ID,
                    FullPath = item.Url,
                    ActionType = (int)mConfig.actionType
                };
                return destructionReport;
            }
            catch (Exception e)
            {
                mLog.Warn($"Error occurred while generating destruction report. Item Id:{item.UniqueId} Error:{e.ToString()}");
                return null;
            }
        }

        private DestructionReport GetDestructionReport(ArchiveApproveReport item)
        {
            try
            {
                DestructionReport destructionReport = new DestructionReport()
                {
                    NodeId = item.NodeId,
                    ArchivedTime = DateTime.UtcNow.Ticks,
                    RuleID = new Guid(mConfig.currentRule.Id),
                    SortTicks = Snowflake.Instance().GetTicks().ToString(),
                    JsonMeta = item.JsonMeta,
                    ListId = item.ListID,
                    FullPath = item.FullPath,
                    ActionType = (int)mConfig.actionType
                };
                return destructionReport;
            }
            catch (Exception e)
            {
                mLog.Warn($"Error occurred while generating destruction report. Item Id:{item.NodeId} Error:{e}");
                return null;
            }
        }

        private DestructionReport GetOnedriveDestructionReport(IAveListItem item, string onedriveItemTermName, string recordsId)
        {
            try
            {
                DestructionReport destructionReport = new DestructionReport()
                {
                    NodeId = item.UniqueId.ToString(),
                    ArchivedTime = DateTime.UtcNow.Ticks,
                    RuleID = new Guid(mConfig.currentRule.Id),
                    SortTicks = Snowflake.Instance().GetTicks().ToString(),
                    JsonMeta = GetOnedriveJsonMeta(item, onedriveItemTermName, recordsId),
                    ListId = item.ParentList.ID,
                    FullPath = item.Url,
                    ActionType = (int)mConfig.actionType
                };
                return destructionReport;
            }
            catch (Exception e)
            {
                mLog.Warn($"Error occurred while generating destruction report. Item Id:{item.UniqueId} Error:{e.ToString()}");
                return null;
            }
        }

        private string GetItemExtension(string objectName, IAveListItem aveItem)
        {
            var result = string.Empty;
            if (aveItem.ParentList.BaseType == AveBaseType.DocumentLibrary)
            {
                var ext = System.IO.Path.GetExtension(objectName);
                result = ext.IndexOf(".") >= 0 ? ext.Substring(1) : "RM_RDM_RecordDetails_DataType_FileNull";
            }
            else
            {
                result = "RM_RDM_RecordDetails_DataType_SPItem";
            }
            return result;
        }

        private string GetJsonMeta(IAveListItem item)
        {
            ArchiverSharePointDto dto = new ArchiverSharePointDto()
            {
                LeafName = item.Name,
                Path = item.Url,
                ArchivedTime = DateTime.UtcNow,
                Metadata = GetMetaData(item),
                ScopeID = item.ParentList.ID,
                SPNodeLevel = item.FileSystemObjectType == AveFileSystemObjectType.Folder ? (int)NodeLevel.Folder : 505, //ConvertToArchiveApproveReport -- ItemType
                CreatedTime = item.FieldValues.ContainsKey(SPColumnConstants.SP_Created) ? item.GetUTCDateWithTimeZone(SPColumnConstants.SP_Created).Ticks : 0,
                CDLastModifiedTime = item.FieldValues.ContainsKey(SPColumnConstants.Modified) ? item.GetUTCDateWithTimeZone(SPColumnConstants.Modified).Ticks : 0,
                FileType = GetItemExtension(item.Name, item),
            };
            string recordsId = string.Empty;
            if (item.FieldValues.ContainsKey(SPColumnConstants.DocumentId))
            {
                recordsId = item.FieldValues[SPColumnConstants.DocumentId]?.ToString();
            }
            else if (item.FieldValues.ContainsKey(RcordsBuiltInColumn.UNIQUEID_NAME))
            {
                recordsId = item.FieldValues[RcordsBuiltInColumn.UNIQUEID_NAME]?.ToString();
            }
            dto.RecordsId = recordsId;
            return JsonConvert.SerializeObject(dto);
        }

        private string GetOnedriveJsonMeta(IAveListItem item, string onedriveItemTermName, string recordsId)
        {
            ArchiverSharePointDto dto = new ArchiverSharePointDto()
            {
                LeafName = item.Name,
                Path = item.Url,
                ArchivedTime = DateTime.UtcNow,
                Metadata = GetMetaData(item),
                ScopeID = item.ParentList.ID,
                OnedriveTermName = onedriveItemTermName,
                CreatedTime = item.FieldValues.ContainsKey(SPColumnConstants.SP_Created) ? item.GetUTCDateWithTimeZone(SPColumnConstants.SP_Created).Ticks : 0,
                CDLastModifiedTime = item.FieldValues.ContainsKey(SPColumnConstants.Modified) ? item.GetUTCDateWithTimeZone(SPColumnConstants.Modified).Ticks : 0,
                FileType = GetItemExtension(item.Name, item),
                RecordsId = recordsId,
            };
            return JsonConvert.SerializeObject(dto);
        }

        public string GetMetaData(IAveListItem item)
        {
            Hashtable columns = this.GetItemColumns(item, BackgroundSettings.GetInstance().RADisplayColumns, mConfig.IsILMode, mConfig.BCSColumnName);
            if (columns != null && columns.Count > 0)
            {
                XmlDocument doc = new XmlDocument();
                XmlElement xe = doc.CreateElement("MetaData");
                foreach (var column in columns.Keys)
                {
                    XmlElement colXe = doc.CreateElement("Column");
                    colXe.SetAttribute("Name", column.ToString());
                    string value = columns[column].ToString();
                    if (value.Contains(delimiter))
                    {
                        string[] values = value.Split(delimiter);
                        colXe.SetAttribute("Value", values[0].ToString());
                        colXe.SetAttribute("ExtendValue", values[1].ToString());
                    }
                    else
                    {
                        colXe.SetAttribute("Value", columns[column].ToString());
                    }
                    xe.AppendChild(colXe);
                }
                return xe.OuterXml;
            }
            return null;
        }
        public Hashtable GetItemColumns(IAveListItem item, List<string> fieldNames, bool isRAJob, string bcsColumnName)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.GetItemColumns"))
            {
                Hashtable columnCollectionOfDisplayName = new Hashtable(StringComparer.OrdinalIgnoreCase);
                if (item != null)
                {
                    #region get RA BCSColumn
                    if (isRAJob)
                    {
                        try
                        {
                            IAveField field = null;
                            if (!string.IsNullOrEmpty(bcsColumnName))
                            {
                                field = item.Fields.GetField(bcsColumnName);
                            }
                            //如果为空，就取BCS Column
                            if (field == null)
                            {
                                string BCSColumnID = "20f84bba906045b4af568ee102a52dcb";
                                field = item.Fields.GetFieldById(new Guid(BCSColumnID), false);
                            }
                            if (field.Type == AveFieldType.Invalid)
                            {
                                var fileObj = item[field.ID];
                                if (fileObj.GetType() != typeof(string))
                                {
                                    var dic = ((Dictionary<string, object>)item[field.ID]);
                                    var termName = dic["Label"].ToString();
                                    var termId = new Guid(dic["TermGuid"].ToString());
                                    columnCollectionOfDisplayName[bcsColumnName] = field.GetFieldValueAsText(item[field.ID]) + delimiter.ToString() + termName + "|" + termId;
                                }
                                else
                                {
                                    columnCollectionOfDisplayName[bcsColumnName] = field.GetFieldValueAsText(item[field.ID]) + delimiter.ToString() + item[field.ID];
                                }
                            }
                            else
                            {
                                mLog.Info("BCSColumnID exist but column type is not Invalid.Field Type:{0}.", field.Type.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            mLog.Info($"Can not get RA BCS Column property when get item columns.bcsColumnName:{bcsColumnName}.Message:{ex.ToString()}.");
                        }
                    }
                    #endregion
                    foreach (var fieldName in fieldNames)
                    {
                        bool isGetColumnByInternalName = false;
                        IAveField field = null;
                        try
                        {
                            if (fieldName.Equals("Content Type", StringComparison.OrdinalIgnoreCase) || fieldName.Equals("ContentType", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    columnCollectionOfDisplayName[fieldName.ToLower(CultureInfo.CurrentCulture)] = item.ContentType.Name;
                                }
                                catch (Exception ex)
                                {
                                    mLog.Info("Can not get content type property when get item columns.Message:{0}.", ex.Message);
                                }
                                continue;
                            }
                            field = item.Fields[fieldName];
                        }
                        catch (Exception e)
                        {
                            try
                            {
                                field = item.Fields.GetFieldByInternalName(fieldName);
                                isGetColumnByInternalName = true;
                            }
                            catch (Exception ex)
                            {
                                mLog.Info("Can not get field by internal name when get item columns.FieldName:{0}.Message:{1}.", fieldName, ex.Message);
                                columnCollectionOfDisplayName[fieldName.ToLower(CultureInfo.CurrentCulture)] = string.Empty;
                                continue;
                            }
                        }
                        try
                        {
                            string fieldTitle = field.Title.ToLower(CultureInfo.CurrentCulture);//RA Need Lower
                            string fieldInternalName = field.InternalName.ToLower(CultureInfo.InvariantCulture);
                            if (field.Hidden)
                            {
                                mLog.Info("Current field is hidden, field Name:{0}.", fieldTitle);
                                continue;
                            }
                            if (item[field.ID] == null)
                            {
                                if (field.Type != AveFieldType.Number && field.Type != AveFieldType.DateTime && field.Type != AveFieldType.Boolean && field.Type != AveFieldType.Integer)
                                {//text match * need this.        
                                    columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = string.Empty;
                                }
                                continue;
                            }
                            switch (field.Type)
                            {
                                //在rule判断时，会判断数据类型。
                                case AveFieldType.Boolean:
                                case AveFieldType.Number:
                                    columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = item[field.ID];
                                    break;
                                case AveFieldType.Counter:
                                    columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = Convert.ToDouble(item[field.ID]);
                                    break;
                                case AveFieldType.DateTime:
                                    columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = ToUniversalTimeWithTimeZone((DateTime)item[field.ID], item.Web);
                                    break;
                                case AveFieldType.User:
                                    var value = item[field.ID];
                                    var stringVlue = value as string;
                                    if (stringVlue != null)
                                    {
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = stringVlue.Substring(stringVlue.IndexOf('#') + 1);
                                    }
                                    else if (value is IEnumerable)
                                    {
                                        StringBuilder users = new StringBuilder();
                                        foreach (var userinfo in (value as IEnumerable))
                                        {
                                            var user = userinfo.ToString();
                                            users.Append(user.Substring(user.IndexOf('#') + 1));
                                            users.Append(';');
                                        }
                                        users.Length = Math.Max(0, users.Length - 1);
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = users.ToString();
                                    }
                                    else
                                    {
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = value;
                                    }
                                    break;
                                case AveFieldType.Lookup:
                                    var lookupValue = item[field.ID];
                                    var realValue = lookupValue as IAveFieldLookupValue;
                                    if (realValue != null)
                                    {
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = realValue.LookupValue;
                                    }
                                    else if (string.Equals(field.TypeAsString, "Lookup", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(field.TypeAsString, "LookupMulti", StringComparison.OrdinalIgnoreCase))
                                    {
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = field.GetFieldValueAsText(lookupValue);
                                    }
                                    else
                                    {
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = lookupValue;
                                    }
                                    break;
                                case AveFieldType.Invalid:
                                    if (string.Equals(field.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(field.TypeAsString, "TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                                    {
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = field.GetFieldValueAsText(item[field.ID]) + delimiter.ToString() + item[field.ID].ToString();
                                    }
                                    else
                                    {
                                        columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = item[field.ID];
                                    }
                                    break;
                                case AveFieldType.ModStat:
                                    columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = field.GetFieldValueAsText(item[field.ID]);
                                    break;
                                default:
                                    columnCollectionOfDisplayName[isGetColumnByInternalName ? fieldInternalName : fieldTitle] = field.GetFieldValueAsText(item[field.ID]);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            mLog.Info(string.Format("Get the metadata of item error.Field Name:{0}.Exception:{1}", field.Title, ex.Message));
                        }
                    }
                }
                return columnCollectionOfDisplayName;
            }
        }
        private DateTime ToUniversalTimeWithTimeZone(DateTime datetime, IAveWeb web)
        {
            if (datetime.Kind != DateTimeKind.Utc)
            {
                datetime = web.RegionalSettings.TimeZone.LocalTimeToUTC(datetime);
            }
            return datetime;
        }
        private void CheckRetentionlabel(IAveFile file)
        {
            try
            {
                if (mConfig.IsILMode
                    && file.Item.Fields.ContainsField("_ComplianceTag")
                    && LabelAppliedByRecords(file.Item["_ComplianceTag"].ToString()))
                {
                    mLog.Info("Current file is label file and Records remove label and delete.FileName:{0}.", file.UniqueId);
                    //file.Item.SetComplianceTag(string.Empty, false, false, false, false, false);
                    file.Item.SetComplianceTagOnBulkItems(string.Empty);
                    file = Web.GetFile(file.UniqueId, file.ServerRelativeUrl);
                    mLog.Info("Delete label file success.File name:{0}", file.UniqueId);
                }
            }
            catch (Exception e)
            {
                mLog.Warn("Delete Retention label.File name:{0} error :{1}", file.Name, e.ToString());
            }
        }

        private string GetExecuteActionForJobDetail()
        {
            string actionType = string.Empty;
            if ((mConfig.currentRule.KeepDataOption & (int)KeepDataOption.LinkDocument) == (int)KeepDataOption.LinkDocument)
            {
                actionType = "SO_Action_LevelStub";
            }
            else if ((mConfig.currentRule.KeepDataOption & (int)KeepDataOption.ArchiveAndLeaveStub) == (int)KeepDataOption.ArchiveAndLeaveStub)
            {
                actionType = "SO_Action_LevelStub";
            }
            else if ((mConfig.currentRule.KeepDataOption & (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub) == (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub)
            {
                actionType = "SO_Action_LevelStub";
            }
            else if ((mConfig.currentRule.KeepDataOption & (int)KeepDataOption.Archive) == (int)KeepDataOption.Archive)
            {
                actionType = "SO_Action_Destroy";
            }
            else if (mConfig.currentRule != null && ((mConfig.currentRule.KeepDataOption & (int)KeepDataOption.NotBackup) == (int)KeepDataOption.NotBackup))
            {
                actionType = "SO_Action_Destroy";
            }
            else
            {
                actionType = "SO_Action_Destroy";
            }
            return actionType;
        }

        private void RemoveRelatedRelationship(IAveListItem aveListItem, string itemUrl)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.RemoveRelatedRelationship"))
            {
                if (mConfig.IsILMode)
                {
                    try
                    {
                        //移除related 关系
                        var utility = new RelatedRecordsUtility();
                        var relatedInfos = utility.GetRelatedProperties(aveListItem);
                        foreach (var relatedInfo in relatedInfos ?? new())
                        {
                            utility.RemoveRelateColumnValue(relatedInfo, Site, itemUrl, aveListItem.UniqueId, "");
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Info("An error occur when remove related relationship.ItemUrl:{0}.Message:{1}.", itemUrl, ex.ToString());
                    }
                }
            }
        }

        private void DeleteEmptyFolder(IAveFile file)
        {
            //mConfig.currentRule.IsDeleteParentFolder = true;
            if (mConfig.currentRule.IsDeleteParentFolder)
            {
                IAveFolder aveFolder = null;
                //From CI,some file get parent folder failed and add retry.
                new AveTaskRetryHelper(5, true).ExecuteWithRetryMechanism(() =>
                {
                    aveFolder = file.ParentFolder;
                });
                if (aveFolder != null)
                {
                    if (aveFolder.UniqueId == aveFolder.ParentList.RootFolder.UniqueId)
                    {
                        mLog.Info("Current folder is root folder and skip delete empty folder.");
                    }
                    else
                    {
                        mConfig.AddDeleteFolderCache(aveFolder.UniqueId);
                    }
                }
            }
        }

        private bool CheckRelatedObjectStatus(string objectName, Guid objectGuid, int objectLevel, IAveListItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverDeletion.CheckRelatedObjectStatus"))
            {
                bool needDeleteSourceFile = true;
                #region 检查Related Record 是否成功删除的逻辑
                if (mConfig.IsILMode && mHeaderInfo.HasAttribute(KeyWord.RelativeDataJobId))
                {
                    mLog.Info("Begin check Related Record State.ObjectName:{0}.", objectName);
                    var jobIds = mHeaderInfo.Attributes[KeyWord.RelativeDataJobId].Value;
                    if (!string.IsNullOrEmpty(jobIds))
                    {
                        var jobIdList = jobIds.Split(';').ToList();
                        mLog.Info("Current Object has Related Record Job,ID Collection:{0}.", jobIds);
                        //Add retry times
                        List<string> needRemoveJobID = new List<string>();
                        int retryCount = 180;
                        while (retryCount > 0)
                        {
                            mLog.Info("needRemoveJobID count is:{0}.", needRemoveJobID.Count);
                            foreach (string jobId in needRemoveJobID)
                            {
                                jobIdList.Remove(jobId);
                            }
                            needRemoveJobID.Clear();
                            if (jobIdList.Count == 0)
                            {
                                if (retryCount == 180)
                                {
                                    mLog.Info("Current Object has related but no end user job ID.");
                                    needDeleteSourceFile = false;
                                }
                                mLog.Info("jobIdList count is 0.");
                                break;
                            }
                            foreach (string jobId in jobIdList)
                            {
                                if (!string.IsNullOrEmpty(jobId))
                                {
                                    var jobState = LoadJobStatus(jobId);
                                    if ((jobState == JobStatus.InProgress || jobState == JobStatus.Wait))
                                    {
                                        mLog.Info(string.Format("Job id is : {0}, status is : {1},continue current foreach.", jobId, jobState.ToString()));
                                        continue;
                                    }
                                    else if (jobState == JobStatus.Finished)
                                    {
                                        mLog.Info(string.Format("Delete related item job : {0} is finished.", jobId));
                                        //Add Finish report for related item in current job detail and summary, to do later
                                    }
                                    else
                                    {
                                        needDeleteSourceFile = false;
                                        //有一个删除related document job 失败，就return，不删除这个document,先打出一些必要的log，然后return
                                        if (jobState == JobStatus.Failed || jobState == JobStatus.FinishWithException || jobState == JobStatus.Skipped || jobState == JobStatus.Stopped)
                                        {
                                            mLog.Warn("The related item deleted failed, skip delete the current Object,JobID:{0}, JobState:{1}.", jobId, jobState.ToString());
                                            mReportInfo.ExceptionTackle("Failed to delete related Object", SPNodeLevel.Item.ToString());
                                        }
                                        else if (jobState == JobStatus.InProgress || jobState == JobStatus.Wait)
                                        {
                                            mLog.Warn(string.Format("The related item deleted job still {0}, skip delete the current Object,JobID:{1}.", jobState.ToString(), jobId));
                                            mReportInfo.ExceptionTackle("Failed to delete related Object", SPNodeLevel.Item.ToString());
                                        }
                                    }
                                    needRemoveJobID.Add(jobId);
                                    RelativeDataJobReortOperation jobReport = new RelativeDataJobReortOperation(jobId);
                                    List<JobDetail> jobDetails = jobReport.GetReports();
                                    foreach (JobDetail jobdetail in jobDetails)
                                    {
                                        JobDetailsStatus status = JobDetailsStatus.Successful;
                                        switch (jobdetail.Status)
                                        {
                                            case 0:
                                                status = JobDetailsStatus.Successful;
                                                if (jobdetail.Remark12 == "Backup")
                                                {
                                                    jobdetail.Message = "StorageOptimization13_SOARRelatedRecordBackupSuccess";
                                                }
                                                else if (jobdetail.Remark12 == "Export")
                                                {
                                                    jobdetail.Message = "StorageOptimization13_SOARRelatedRecordExportSuccess";
                                                }
                                                else
                                                {
                                                    jobdetail.Message = "StorageOptimization13_SOARRelatedRecordDeleteSuccess";
                                                }
                                                break;
                                            case 1:
                                                status = JobDetailsStatus.Failed;
                                                if (jobdetail.Remark12 == "Backup")
                                                {
                                                    jobdetail.Message = "StorageOptimization13_SOARRelatedRecordBackupFailed";
                                                }
                                                else if (jobdetail.Remark12 == "Export")
                                                {
                                                    jobdetail.Message = "StorageOptimization13_SOARRelatedRecordExportFailed";
                                                }
                                                else
                                                {
                                                    jobdetail.Message = "StorageOptimization13_SOARRelatedRecordDeleteFailed";
                                                }
                                                break;
                                            case 2:
                                                status = JobDetailsStatus.Skipped;
                                                if (jobdetail.Remark12 == "Backup")
                                                {
                                                    jobdetail.Message = "StorageOptimization13_SOARRelatedRecordBackupSkipped";
                                                }
                                                else if (jobdetail.Remark12 == "Export")
                                                {
                                                    jobdetail.Message = "StorageOptimization13_SOARRelatedRecordExportSkipped";
                                                }
                                                else
                                                {
                                                    jobdetail.Message = "StorageOptimization13_SOARRelatedRecordDeleteSkipped";
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                        if (jobdetail.Remark12 == "Backup")
                                        {
                                            JobExecutionProcessStatisticExecutor.Instance.CalculateArchiveSummary(null, jobdetail.Size, Convert.ToInt32(Enum.Parse(typeof(CacheNodeType), jobdetail.Type)), status);
                                            mConfig.JobReportDto.AddReport(jobdetail.SrcURL, jobdetail.Size, status, Convert.ToInt32(Enum.Parse(typeof(CacheNodeType), jobdetail.Type)), mConfig.JobId, "", "", jobdetail.Message);
                                        }
                                        else if (jobdetail.Remark12 == "Export")
                                        {
                                            mConfig.JobReportDto.AddVaultReport(jobdetail.SrcURL, jobdetail.Size, status, Convert.ToInt32(Enum.Parse(typeof(CacheNodeType), jobdetail.Type)), mConfig.JobId, "", "", jobdetail.Message);
                                        }
                                        else
                                        {
                                            JobExecutionProcessStatisticExecutor.Instance.CalculateSuccessDeleteAndStubSummary(status, "", jobdetail.Remark12, Convert.ToInt32(Enum.Parse(typeof(CacheNodeType), jobdetail.Type)));
                                            mConfig.JobReportDto.AddDeletionReport(jobdetail.SrcURL, jobdetail.Size, status, Convert.ToInt32(Enum.Parse(typeof(CacheNodeType), jobdetail.Type)), mConfig.JobId, "", "", jobdetail.Remark12, jobdetail.Message);
                                        }
                                    }
                                }
                                else
                                {
                                    needRemoveJobID.Add(jobId);
                                    needDeleteSourceFile = false;
                                    mLog.Info("Current Job ID is null,continue current foreach.");
                                    continue;
                                }
                            }
                            Thread.Sleep(10 * 1000);
                            retryCount--;
                        }
                        if (!needDeleteSourceFile)
                        {
                            mLog.Info("Current file has related Object doesn't delete.ObjectName:{0}.", objectName);
                            mReportInfo.Status = JobDetailsStatus.Failed;
                            mReportInfo.Message = "StorageOptimization13_SOARRelatedRecordsSourceObjectDeleteFailed";
                            //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, objectGuid, objectLevel, mReportInfo.SubJobId);
                            return false;
                        }
                    }
                    else
                    {
                        mLog.Info("Current File RelativeDataJobId is null.ObjectName:{0}.", objectName);
                        if (item.Fields.ContainsFieldWithStaticName("RecordsRelated"))
                        {
                            var metadata = item["RecordsRelated"];
                            if (metadata != null && !string.IsNullOrEmpty(metadata.ToString()))
                            {
                                RMRelatedItemInfo physicalRelated = new RelatedRecordsUtility().GetRelatedProperties(item).Where(r => r.SourceFlag == (int)ArchiverCommon.SOSourceFlag.PhysicalObject).FirstOrDefault();
                                if (physicalRelated != null)
                                {
                                    mLog.Info("Related column value has physicalRelated and delete current Object.");
                                }
                                else
                                {
                                    //mReportInfo.Status = JobDetailsStatus.Failed;
                                    //mConfig.ProgressDto.HasErrorNode = true;
                                    //mReportInfo.Message = "StorageOptimization13_SOARRelatedRecordsSourceObjectDeleteFailed";
                                    mLog.Info("Related column value is not null.RecordsRelated:{0}.", metadata.ToString());
                                    //return false;
                                }
                            }
                            else
                            {
                                mLog.Info("Related column value is null and delete current Object.");
                            }
                        }
                    }
                }
                return needDeleteSourceFile;
                #endregion
            }
        }

        private bool DeleteRelatedObjectForDeleteOnlyAction(Guid nodeId, IAveListItem item, int deleteRelatedRecords)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverDeletion.DeleteRelatedObjectForDeleteOnlyAction"))
            {
                if (mConfig.currentRule.IsManualApproval)
                {
                    Guid recordID = ScheduleConfiguration.GetRecordId(mSite.ID, nodeId);
                    if (mConfig.IsILMode && mConfig.ExplorerDao != null)
                    {
                        var record = mConfig.exploreDBSPRecords.Where(x => x.ScopeId == mSite.ID && x.Id == recordID).FirstOrDefault();
                        if (record == null && mConfig.exploreDBSPRecords.Count >= 10000)
                        {
                            record = mConfig.ExplorerDao.ReadById(mSite.ID, recordID);
                        }
                        if (record?.ManualIsRelatedRecords ?? false)
                        {
                            deleteRelatedRecords = record.ManualRelatedRecordsAction;
                        }
                    }
                }

                bool hasErrorRelated = false;
                if (mConfig.IsILMode && !mConfig.IsRelativeDataJob && deleteRelatedRecords == 1 && !mConfig.CheckItemIsRecordsHold(nodeId))
                {
                    if (item.Fields.ContainsFieldWithStaticName("RecordsRelated"))
                    {
                        var metadata = item["RecordsRelated"];
                        if (metadata != null && !string.IsNullOrEmpty(metadata.ToString()))
                        {
                            mLog.Info("Begin DisposeRelatedItemsForDeleteOnly.Url:{0}.RelatedInfo:{1}.", item.Url, metadata.ToString());
                            //hasErrorRelated = DisposalRelatedItemUtility.DisposeRelatedItemsForDeleteOnly(mConfig, metadata.ToString(), mConfig.currentRule, SendJobDetail);
                            var relatedString = FilterScanExistingItems(metadata.ToString());
                            if (!string.IsNullOrWhiteSpace(relatedString))
                            {
                                if (mConfig.IsOneDriverSite)
                                {
                                    hasErrorRelated = !RelativeDataArchiverAction.DeleteRelatedData(RuleManagerService.ConvertToOneDriveRule(mConfig.currentRule), nodeId, relatedString, (int)SourceFlag.OneDrive, true, mConfig.JobId);
                                }
                                else if (mConfig.IsTeams)
                                {
                                    hasErrorRelated = !RelativeDataArchiverAction.DeleteRelatedData(mConfig.currentRule, nodeId, relatedString, (int)SourceFlag.Teams, true, mConfig.JobId);
                                }
                                else
                                {
                                    hasErrorRelated = !RelativeDataArchiverAction.DeleteRelatedData(mConfig.currentRule, nodeId, relatedString, (int)SourceFlag.SharePoint, true, mConfig.JobId);
                                }
                            }
                        }
                        else
                        {
                            mLog.Info("Related column value is null");
                        }
                    }
                }
                return hasErrorRelated;
            }
        }

        private string FilterScanExistingItems(string relatedString)
        {
            string resultString = relatedString;
            var relatedItems = RelatedRecordsUtility.GetRelatedProperties(relatedString);
            var spoDataIds = relatedItems.Where(r => r.SourceFlag == (int)SourceFlag.SharePoint || r.SourceFlag == (int)SourceFlag.All).Select(r => r.id).ToList();
            if (spoDataIds.IsNotNullOrEmpty())
            {
                var existingIds = ScanDataCache.Instance.GetScanExistingIds(spoDataIds);
                if (existingIds.IsNotNullOrEmpty())
                {
                    mLog.Info($"Current related item fit rule in this job so skip it.NodeIds:{string.Join(",", existingIds)}.");
                    resultString = RelatedRecordsUtility.GetRelatedString(relatedItems.Where(r => !existingIds.Contains(r.id)).ToList());
                }
            }
            return resultString;
        }



        #region Leave Stub Method
        private async System.Threading.Tasks.Task LinkDocumentAsync(IAveFile file, Guid docId, int archiveLevel)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverDeletion.LinkDocument"))
            {
                bool isLinkToDucument = LinkFileCommon.IsLeaveStubRule(mConfig.currentRule);
                string folderPath = string.Empty;
                string filePath = string.Empty;
                string desUrl = string.Empty;
                if (isLinkToDucument)
                {
                    desUrl = this.GetDestUrlByFile(file);
                    LinkFileCommon.RemoveArchiveStub(file, mConfig);
                    //mLog.Info("Current file is LinkDocument.FileUrl:{0}.FolderUrl:{1}.", file.ServerRelativeUrl, desUrl);
                    mLog.Info($"Current file is LinkDocument. docId {docId}");
                    if (!mConfig.currentRule.IsLeaveStubRemoveMetadata)
                    {
                        try
                        {
                            //file.Name, file.ServerRelativeUrl, mConfig.currentRule.Name, DateTime.Now.ToString()
                            var psc = await LinkFileCommon.SetStubContentValue(file, mConfig);
                            //linkFileContent = GetFileContent(file.ParentFolder.ParentWeb.LanguageCulture, psc);//linkContentType.ID.ToString(), "/Style Library/revimdisposalstub.aspx", Site.Url);
                            folderPath = Path.Combine(AveEnv.AgentJobFolder, mConfig.JobId);
                            //RECO-213 多线程link document时，两个文件名字相同，生成的dat相同，需要区分开,否则会出现IOException
                            filePath = Path.Combine(folderPath, /*file.ServerRelativeUrl.Replace("/", "_").Replace(@"\", "_")*/ Guid.NewGuid().ToString() + ".dat");
                            #region init temp file
                            try
                            {
                                lock (createFolderLock)
                                {
                                    if (!Directory.Exists(folderPath))
                                    {
                                        Directory.CreateDirectory(folderPath);
                                        mLog.Info("Create Folder : {0}.", folderPath);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                if (Directory.Exists(folderPath))
                                {
                                    mLog.Info("the folder is exist, folderPath: {0}.", folderPath);
                                }
                                else
                                {
                                    mLog.Error("Can not create temp folder : {0}. Reason: {1}.", folderPath, ex.ToString());
                                    throw;
                                }
                            }
                            #endregion

                            using (AvePerformanceScope pc1 = new AvePerformanceScope("ArchiverDeletion.LinkDocument.ExportDocument"))
                            {
                                using (RecordManagerFileSender fileSender = new RecordManagerFileSender(filePath))
                                {
                                    using (IAveBackupStream exportStream = new WrapperBackupStreamV1(new FileSendWrapper(fileSender)))
                                    {
                                        SPStubDocExport exportor = null;
                                        AveSPFolder parentFolder = null;
                                        if ((mConfig.currentRule.KeepDataOption & (int)KeepDataOption.NotBackup) == (int)KeepDataOption.NotBackup)
                                        {
                                            parentFolder = mConfig.GetCurrentAveBackupFolder(file.ParentFolder);
                                            if (psc.ReCenterLinkSelected || mConfig.currentRule.LeaveStubType == LeaveStubType.Link)
                                            {
                                                psc.SetValue(StubDynamicValueType.ReCenterLink, GetReCenterRestoreLink(file, parentFolder));
                                            }
                                            linkFileContent = LinkFileCommon.GetFileContent(file.ParentFolder.ParentWeb.LanguageCulture, psc);//linkContentType.ID.ToString(), "/Style Library/revimdisposalstub.aspx", Site.Url);
                                            exportor = new SPStubDocExport(parentFolder, file, linkFileContent);
                                        }
                                        else
                                        {
                                            parentFolder = GetCurrentAveBackupFolder(file.ParentFolder);
                                            if (psc.ReCenterLinkSelected || mConfig.currentRule.LeaveStubType == LeaveStubType.Link)
                                            {
                                                psc.SetValue(StubDynamicValueType.ReCenterLink, GetReCenterRestoreLink(file, parentFolder));
                                            }
                                            linkFileContent = LinkFileCommon.GetFileContent(file.ParentFolder.ParentWeb.LanguageCulture, psc);//linkContentType.ID.ToString(), "/Style Library/revimdisposalstub.aspx", Site.Url);
                                            exportor = new SPStubDocExport(parentFolder, file, linkFileContent);
                                        }
                                        exportor.ExportSPFile(exportStream);
                                    }
                                }
                            }
                            this.LeaveDocumentLinkFile(file, desUrl, filePath);
                        }
                        catch (Exception e)
                        {
                            mLog.Warn("Some errors occur when export file, file name {0}, error detail {1}.", file.Name, e.ToString());
                            throw new RALeaveStubException(e.Message);
                        }
                    }
                    else
                    {
                        await this.LeaveDocumentLinkFileWithoutMetadataAsync(file, desUrl);
                    }
                }
            }
        }

        private async System.Threading.Tasks.Task LinkDocumentPackagingAsync(IAveFile file, Guid docId, int archiveLevel, long reportSize)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverDeletion.LinkDocument"))
            {
                string folderPath = string.Empty;
                string filePath = string.Empty;
                string desUrl = string.Empty;
                string md5 = string.Empty;
                //不去删除旧Stub文件，由Migration Notification去做实现这个行为
                //LinkFileCommon.RemoveArchiveStub(file, mConfig);
                //linkFileContent = GetFileContent(file.ParentFolder.ParentWeb.LanguageCulture);//linkContentType.ID.ToString(), "/Style Library/revimdisposalstub.aspx", Site.Url);
                folderPath = Path.Combine(AveEnv.AgentJobFolder, mConfig.JobId);
                //RECO-213 多线程link document时，两个文件名字相同，生成的dat相同，需要区分开,否则会出现IOException
                filePath = Path.Combine(folderPath, /*file.ServerRelativeUrl.Replace("/", "_").Replace(@"\", "_")*/ Guid.NewGuid().ToString() + ".dat");
                desUrl = this.GetDestUrlByFile(file);
                //mLog.Info("Current file is LinkDocument.FileUrl:{0}.FolderUrl:{1}.", file.ServerRelativeUrl, desUrl);
                try
                {
                    #region init temp file
                    try
                    {
                        lock (createFolderLock)
                        {
                            if (!Directory.Exists(folderPath))
                            {
                                Directory.CreateDirectory(folderPath);
                                mLog.Info("Create Folder : {0}.", folderPath);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Directory.Exists(folderPath))
                        {
                            mLog.Info("the folder is exist, folderPath: {0}.", folderPath);
                        }
                        else
                        {
                            mLog.Error("Can not create temp folder : {0}. Reason: {1}.", folderPath, ex.ToString());
                            throw;
                        }
                    }
                    #endregion
                    var psc = await LinkFileCommon.SetStubContentValue(file, mConfig);
                    using (AvePerformanceScope pc1 = new AvePerformanceScope("ArchiverDeletion.LinkDocument.ExportDocument"))
                    {
                        using (RecordManagerFileSender fileSender = new RecordManagerFileSender(filePath))
                        {
                            using (IAveBackupStream exportStream = new WrapperBackupStreamV1(new FileSendWrapper(fileSender)))
                            {
                                SPStubDocExport exportor = null;
                                AveSPFolder parentFolder = null;
                                if ((mConfig.currentRule.KeepDataOption & (int)KeepDataOption.NotBackup) == (int)KeepDataOption.NotBackup)
                                {
                                    parentFolder = mConfig.GetCurrentAveBackupFolder(file.ParentFolder);
                                    md5 = LinkFileCommon.GetDocumnetPathMD5(file.Web.Site.Url, parentFolder.Path, file.Name);
                                    if (psc.ReCenterLinkSelected || mConfig.currentRule.LeaveStubType == LeaveStubType.Link)
                                    {
                                        string restoreUrl = GetReCenterRestoreLink(file, md5);
                                        psc.SetValue(StubDynamicValueType.ReCenterLink, restoreUrl);
                                    }
                                    linkFileContent = LinkFileCommon.GetFileContent(file.ParentFolder.ParentWeb.LanguageCulture, psc);
                                    exportor = new SPStubDocExport(parentFolder, file, linkFileContent);
                                }
                                else
                                {
                                    parentFolder = GetCurrentAveBackupFolder(file.ParentFolder);
                                    md5 = LinkFileCommon.GetDocumnetPathMD5(file.Web.Site.Url, parentFolder.Path, file.Name);
                                    if (psc.ReCenterLinkSelected || mConfig.currentRule.LeaveStubType == LeaveStubType.Link)
                                    {
                                        string restoreUrl = GetReCenterRestoreLink(file, md5);
                                        psc.SetValue(StubDynamicValueType.ReCenterLink, restoreUrl);
                                    }
                                    linkFileContent = LinkFileCommon.GetFileContent(file.ParentFolder.ParentWeb.LanguageCulture, psc);
                                    exportor = new SPStubDocExport(parentFolder, file, linkFileContent);
                                }
                                exportor.ExportSPFile(exportStream);
                            }
                        }
                    }
                    try
                    {
                        var PackageApi = CreateLinkFileByPackage.GetInstance(mConfig);
                        PackageApi.ResetList(file.ParentFolder.ParentList);
                        PackageApi.ProcessDocument(file, desUrl, filePath, reportSize, psc, md5);
                        PackageApi.SplitPackage(false);
                    }
                    finally
                    {
                        DeleteTempFile(new List<string>() { filePath });
                    }
                }
                catch (Exception e)
                {
                    mLog.Warn("Some errors occur when export file, file name {0}, error detail {1}.", file.Name, e.ToString());
                    throw new RALeaveStubException(e.Message);
                }
            }
        }

        private string GetDestUrlByFile(IAveFile file)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverDeletion.GetDestUrlByFile"))
            {
                string desUrl = string.Empty;
                string[] destUrls = file.ServerRelativeUrl.Split('/');
                string[] webdestUrls = file.Web.Site.Url.Split('/');
                for (int i = 0; i < 3; i++)
                {
                    desUrl += webdestUrls[i] + "/";
                }
                for (int i = 1; i < destUrls.Length - 1; i++)
                {
                    desUrl += destUrls[i] + "/";
                }
                return desUrl;
            }
        }

        private string GetReCenterRestoreLink(IAveFile file, AveSPFolder parentFolder)
        {
            string md5 = LinkFileCommon.GetDocumnetPathMD5(file.Web.Site.Url, parentFolder.Path, file.Name);
            string userId = string.Empty;
            if (mConfig.IsOneDriverSite)
            {
                userId = ArchiverCommonStaticMethod.GetADUserID(file.Web.Site.Owner.Email, mConfig.aveObjectModelFactory.AccountInfo);
            }
            StubLinkDetails stubLinkDetails = new StubLinkDetails(mConfig.aveObjectModelFactory.AccountInfo.TenantId, file.Web.Site.Url, file.ServerRelativeUrl, md5, mConfig.CurrentIndexJobID, userId, mConfig.currentRule.LeaveStubType);
            stubLinkDetails.StubProductSource = WrapperConfiguration.IsAOSPLeaveStub ? StubProductSource.AOSP : StubProductSource.Opus;
            var reCenterUrl = ArchiverCommonStaticMethod.GetReCenterHost(mConfig.TenantGroupId);
            if (string.IsNullOrEmpty(reCenterUrl))
            {
                throw new RALeaveStubException("Can not get ReCenter host.");
            }

            return string.Format($"{reCenterUrl.TrimEnd('/')}/?archiver={new StubLinkProcessor(mConfig).ConvertToString(stubLinkDetails)}");
        }

        private string GetReCenterRestoreLink(IAveFile file, string md5)
        {
            string userId = string.Empty;
            if (mConfig.IsOneDriverSite)
            {
                userId = ArchiverCommonStaticMethod.GetADUserID(file.Web.Site.Owner.Email, mConfig.aveObjectModelFactory.AccountInfo);
            }
            StubLinkDetails stubLinkDetails = new StubLinkDetails(mConfig.aveObjectModelFactory.AccountInfo.TenantId, file.Web.Site.Url, file.ServerRelativeUrl, md5, mConfig.CurrentIndexJobID, userId, mConfig.currentRule.LeaveStubType);
            stubLinkDetails.StubProductSource = WrapperConfiguration.IsAOSPLeaveStub ? StubProductSource.AOSP : StubProductSource.Opus;
            var reCenterUrl = ArchiverCommonStaticMethod.GetReCenterHost(mConfig.TenantGroupId);
            if (string.IsNullOrEmpty(reCenterUrl))
            {
                throw new RALeaveStubException("Can not get ReCenter host.");
            }

            return string.Format($"{reCenterUrl.TrimEnd('/')}/?archiver={new StubLinkProcessor(mConfig).ConvertToString(stubLinkDetails)}");
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "revimdisposalstub")]
        private void LeaveDocumentLinkFile(IAveFile file, string desUrl, string filePath)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverDeletion.LeaveDocumentLinkFile"))
            {
                string newLeafName = GetStubFileName(file);
                string serverRelativeUrl = file.ServerRelativeUrl + LinkFileCommon.GetStubFileNameSuffixWithDot(mConfig);
                IAveFile newfile = null;
                try
                {
                    using (AvePerformanceScope performanceRestore = new AvePerformanceScope("ArchiverDeletion.LeaveDocumentLinkFile.Restore"))
                    {
                        using (RecordManagerFileReceiver fileReceiver = new RecordManagerFileReceiver(filePath))
                        {
                            using (IAveRestoreStream importStream = new WrapperRestoreStreamV1(new FileReceiverWrapper(fileReceiver)))
                            {
                                string listUrl = List.ParentWeb.Url + "/" + List.RootFolder.Url;
                                string subFolderUrl = desUrl.Substring(listUrl.Length).Trim('/');
                                Wrapper.Restore.AveSPFolder aveSPFolder = null;
                                if ((mConfig.currentRule.KeepDataOption & (int)KeepDataOption.NotBackup) == (int)KeepDataOption.NotBackup)
                                {
                                    aveSPFolder = mConfig.GetStubRestoreAveCurrentFolder(subFolderUrl, file.ParentFolder.UniqueId);
                                }
                                else
                                {
                                    aveSPFolder = GetStubRestoreAveCurrentFolder(subFolderUrl, file.ParentFolder.UniqueId);
                                }
                                using (SPStubDocImport importor = new SPStubDocImport(aveSPFolder, mConfig.GetStubIAveORecords(), newLeafName, desUrl))
                                {
                                    importor.ImportAveSPDoc(importStream, mConfig, file);
                                }
                            }
                        }
                    }
                    newfile = GetCreateLinkFile(serverRelativeUrl);
                    IAveListItem newItem = newfile.Item;
                    //REC-2432 Host Header Site Collection通过IAveFile GetFile(string serverRelativeUrl);方式获取不到IAveListItem对象.
                    if (newItem == null)
                    {
                        mLog.Info("Current IAveListItem is null and will ReGet IAveListItem by List GetItemByUniqueId.");
                        newItem = List.GetItemByUniqueId(newfile.UniqueId);
                        mLog.Info("ReGet IAveListItem successful by List GetItemByUniqueId. IAveListItem is null:{0}.", newItem == null);
                    }

                    try
                    {
                        if (LinkFileCommon.IsDeclareLinkFile(mConfig))
                        {
                            DeclareItem(newItem);
                        }
                    }
                    catch (Exception exc)
                    {
                        mLog.Warn("Declare Item has some error, detail: {0}.", exc.ToString());
                        throw;
                    }
                }
                catch (Exception e)
                {
                    mLog.Warn("Some error occur when create leave a stub item ,file name {0}, error detail: {1}.", file.Name, e.ToString());
                    newfile = GetCreateLinkFile(serverRelativeUrl);
                    if (newfile != null && newfile.Exists)
                    {
                        var exItem = List.GetItemByUniqueId(newfile.UniqueId);
                        if (ScheduleConfiguration.CheckisRecord(exItem))
                        {
                            mLog.Info("Current wrong stub file is declare file and need undeclare.FileName:{0}.", newLeafName);
                            UndeclareItem(newfile.Item.ID);
                            exItem = List.GetItemByUniqueId(newfile.UniqueId);
                            mLog.Info("Current wrong stub file is declare file and undeclare success.FileName:{0}.", newLeafName);
                        }
                        exItem.Delete();
                        mLog.Info("Current wrong stub file is delete success.FileName:{0}.", newLeafName);
                    }
                    else
                    {
                        mLog.Info("Current wrong stub file object is null and skip delete.FileName:{0}.", newLeafName);
                    }
                    throw new RALeaveStubException(e.Message, e);
                }
                finally
                {
                    mLog.Info("End to Restore.SourceFileUrl:{0}.DesListUrl:{1}.", file.UniqueId, "");
                    DeleteTempFile(new List<string>() { filePath });
                }
            }
        }

        private async System.Threading.Tasks.Task LeaveDocumentLinkFileWithoutMetadataAsync(IAveFile file, string desUrl)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverDeletion.LeaveDocumentLinkFileWithoutMetadata"))
            {
                string newLeafName = GetStubFileName(file);
                try
                {
                    //mLog.Info("Current stub does not keep metadata and begin to add file to SP. StubUrl:{0}.", desUrl + newLeafName);

                    var psc = await LinkFileCommon.SetStubContentValue(file, mConfig);
                    var parentFolder = GetCurrentAveBackupFolder(file.ParentFolder);
                    if (psc.ReCenterLinkSelected)
                    {
                        psc.SetValue(StubDynamicValueType.ReCenterLink, GetReCenterRestoreLink(file, parentFolder));
                    }
                    IAveFile stubFile = file.ParentFolder.Files.Add(new AveFileCreationInformation() { Url = newLeafName, Content = LinkFileCommon.GetFileContent(file.ParentFolder.ParentWeb.LanguageCulture, psc), Overwrite = true });
                    if (stubHasUniqueRoleAssignments)//file.Item.HasUniqueRoleAssignments 不好用
                    {
                        mLog.Info("Current stub HasUniqueRoleAssignments and begin to BreakRoleInheritance & add RoleAssignments. StubUrl:{0}.", file.UniqueId);
                        stubFile.Item.BreakRoleInheritance(false);
                        foreach (var roleAssignment in file.Item.RoleAssignments)
                        {
                            try
                            {
                                stubFile.Item.RoleAssignments.Add(roleAssignment);
                            }
                            catch (Exception e)
                            {
                                mLog.Warn("Some error occur when restoring RoleAssignments.", e.ToString());
                            }
                        }
                    }
                    IAveListItem newItem = stubFile.Item;
                    try
                    {
                        LinkFileCommon.SetLinkFieldValue(newItem, mConfig);
                    }
                    catch (Exception e)
                    {
                        mLog.Warn($"set item  {file.Name} field value has some error, detail: {e.ToString()}.");
                        throw;
                    }
                    try
                    {
                        if (LinkFileCommon.IsDeclareLinkFile(mConfig))
                        {
                            DeclareItem(newItem);
                        }
                    }
                    catch (Exception exc)
                    {
                        mLog.Warn($"Declare Item {file.Name} has some error, detail: {exc.ToString()}.");
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    string serverRelativeUrl = file.ServerRelativeUrl + LinkFileCommon.GetStubFileNameSuffixWithDot(mConfig);
                    mLog.Warn("Some error occur when create leave a stub item ,file name {0}, error detail: {1}.", file.Name, ex.ToString());
                    var newfile = GetCreateLinkFile(serverRelativeUrl);
                    if (newfile != null && newfile.Exists)
                    {
                        var exItem = List.GetItemByUniqueId(newfile.UniqueId);
                        if (ScheduleConfiguration.CheckisRecord(exItem))
                        {
                            mLog.Info("Current wrong stub file is declare file and need undeclare.FileName:{0}.", newLeafName);
                            UndeclareItem(newfile.Item.ID);
                            exItem = List.GetItemByUniqueId(newfile.UniqueId);
                            mLog.Info("Current wrong stub file is declare file and undeclare success.FileName:{0}.", newLeafName);
                        }
                        exItem.Delete();
                        mLog.Info("Current wrong stub file is delete success.FileName:{0}.", newLeafName);
                    }
                    else
                    {
                        mLog.Info("Current wrong stub file object is null and skip delete.FileName:{0}.", newLeafName);
                    }
                    throw;
                }
            }
        }

        private string GetStubFileName(IAveFile file)
        {
            string stubFileName = string.Empty;
            if (mConfig.IsILMode)
            {
                stubFileName = file.Name + ".aspx";
            }
            else
            {
                if (mConfig.currentRule.LeaveStubType == LeaveStubType.Aspx)
                {
                    stubFileName = file.Name + ".aspx";
                }
                else if (mConfig.currentRule.LeaveStubType == LeaveStubType.Html)
                {
                    stubFileName = file.Name + ".html";
                }
                else if (mConfig.currentRule.LeaveStubType == LeaveStubType.Link)
                {
                    stubFileName = file.Name + ".url";
                }
                else
                {
                    stubFileName = file.Name + ".txt";
                }
            }
            return stubFileName;
        }

        private IAveFile GetCreateLinkFile(string serverRelativeUrl)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverDeletion.GetCreateLinkFile"))
            {
                IAveFile linkFile = Web.GetFile(serverRelativeUrl);
                if (linkFile.UniqueId == Guid.Empty || linkFile.Item == null)
                {
                    mLog.Info("File UniqueId is Guid empty and reGet file.");
                    try
                    {
                        //Office 365 Root Site Collection need send serverRelativeUrl.RECO-1278
                        linkFile = Web.GetFile(System.Web.HttpUtility.UrlDecode(serverRelativeUrl));
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("Can't reGet file UniqueId.Message:{0}.", ex.ToString());
                    }
                }
                return linkFile;
            }
        }

        public void UndeclareItem(int itemID)
        {
            try
            {
                IAveListItem listItem = List.GetItemById(itemID);
                Record.UndeclareItemAsRecord(listItem);
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while converting the record:{0}", e.ToString());
                throw;
            }
        }

        private void DeclareItem(IAveListItem listItem)
        {
            try
            {
                mLog.Info("Begin Declare item:{0}.", listItem.ID);
                if (mConfig.IsILMode)
                {
                    mConfig.EnsureBlockEditAndDelete(Site);
                }
                using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverDeletion.DeclareItemByIAveListItem"))
                {
                    if (mConfig.IsILMode)
                    {
                        var isRecord = ArchiverCommonStaticMethod.CheckisRecord(listItem);
                        if (isRecord)
                        {
                            //add option to check declared records option.
                            if (ArchiverCommonStaticMethod.IsBlockDeleteOnlyRecord(listItem))
                            {
                                mLog.Info("Current status is not declared reocrd block edit and delete need declared again {0}", listItem.ID);
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
                        if (!ArchiverCommonStaticMethod.CheckisRecord(listItem))
                        {
                            Record.DeclareItemAsRecord(listItem);
                        }
                    }
                    mLog.Info("Declare item Successfully:{0}", listItem.ID);
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

        private void DeleteTempFile(List<string> files)
        {
            foreach (string fileFullPath in files)
            {
                try
                {
                    System.IO.File.Delete(fileFullPath);
                    mLog.Info("Delete Temp file Successful.");
                }
                catch (Exception ex)
                {
                    mLog.Warn("Error in Delete Temp File: {0}, Error: {1}", fileFullPath, ex.ToString());
                }
            }
        }

        private void UpdateExploreDB(Guid nodeID, int updateStatus, Record addRecord = null, string pathMd5 = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.UpdateExploreDB"))
            {
                Guid recordID = ScheduleConfiguration.GetRecordId(mSite.ID, nodeID);
                if (mConfig.IsILMode && mConfig.ExplorerDao != null)
                {
                    try
                    {
                        Record record = null;
                        if (mConfig.IsRelativeDataJob)
                        {
                            record = mConfig.ExplorerDao.ReadById(mSite.ID, recordID);
                        }
                        else
                        {
                            record = mConfig.exploreDBSPRecords.Where(x => x.ScopeId == mSite.ID && x.Id == recordID).FirstOrDefault();
                            if (record == null && mConfig.exploreDBSPRecords.Count >= 10000)
                            {
                                record = mConfig.ExplorerDao.ReadById(mSite.ID, recordID);
                            }
                        }
                        if (record != null)
                        {
                            if (mConfig.currentRule.IsManualApproval)
                            {
                                AddManualHistory(record);
                                if (updateStatus == (int)RMRecordStatus.Archived && record.RecordStatus == (int)RMRecordStatus.ManualPreSync)
                                {
                                    //unsync data,keep status when archiver
                                    mConfig.ExplorerDao.UpdateRecordStatusAndDestroyedTime4Manual(mSite.ID, recordID, (int)RMRecordStatus.ManualPreSync);
                                }
                                else
                                {
                                    mConfig.ExplorerDao.UpdateRecordStatusAndDestroyedTime4Manual(mSite.ID, recordID, updateStatus);
                                }
                                //mConfig.ExplorerDao.UpdateRecordStatusAndDestroyedTime4Manual(mSite.ID, recordID, updateStatus);
                            }
                            else
                            {
                                mConfig.ExplorerDao.UpdateRecordStatusAndDestroyedTime(mSite.ID, recordID, updateStatus);
                            }
                            mLog.Info("Update Record Status successful");
                            if (updateStatus == 8)
                            {
                                ArchiverIndexDto indexDto = null; ;
                                try
                                {
                                    indexDto = Convert2ArchiverIndexDto(mConfig.GetArchiverIndex(pathMd5));
                                }
                                catch (Exception e)
                                {
                                    mLog.Error($"Error occurred while getting archiver index. Node id:{nodeID} Error:{e.ToString()}");
                                }
                                //ecord.AppendMetaInfoForArchiverIndex(SerializerHelper.SerializeByDataContractJsonSerializer(indexDto));
                                mConfig.ExplorerDao.AddArchivedRelatedColumn(mSite.ID, recordID, pathMd5, mConfig.CurrentIndexJobID, indexDto != null ? SerializerHelper.SerializeByDataContractJsonSerializer(indexDto) : null);
                                mLog.Info("Update archived custom column successful");
                            }
                        }
                        else
                        {
                            mLog.Info("Current object:{0} doesn't exist in explore.", recordID);
                            if (addRecord != null)
                            {
                                //RECO-9707 if site collection doesn't exist in explorer, add it
                                addRecord.DestroyedTime = DateTime.UtcNow.Ticks;
                                mConfig.ExplorerDao.Add(addRecord);
                                mLog.Info("Add object:{0} in explorer.", addRecord.NodeId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("Update Record Status Failed.Message:{0}.", ex.ToString());
                    }
                }
            }
        }

        private void UpdateRecordInExploreDB(Record record, Guid nodeID, int updateStatus, string pathMd5 = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.UpdateExploreDB"))
            {
                Guid recordID = ScheduleConfiguration.GetRecordId(mSite.ID, nodeID);
                if (mConfig.IsILMode && mConfig.ExplorerDao != null)
                {
                    try
                    {
                        if (record != null)
                        {
                            if (mConfig.currentRule.IsManualApproval)
                            {
                                if (mConfig.AutoApprovalManualRule)
                                {
                                    record.ManualApprovedStatus = (int)Contract.SOApproveDBStatus.Cancelled;
                                }
                                AddManualHistory(record);
                                if (mConfig.AutoApprovalManualRule)
                                {
                                    mConfig.ExplorerDao.UpdateRecordStatusToCancel(mSite.ID, recordID,
                                        (updateStatus == (int)RMRecordStatus.Archived && record.RecordStatus == (int)RMRecordStatus.ManualPreSync) ? (int)RMRecordStatus.ManualPreSync : updateStatus);
                                }
                                else if (updateStatus == (int)RMRecordStatus.Archived && record.RecordStatus == (int)RMRecordStatus.ManualPreSync)
                                {
                                    //unsync data,keep status when archiver
                                    mConfig.ExplorerDao.UpdateRecordStatusAndDestroyedTime4Manual(mSite.ID, recordID, (int)RMRecordStatus.ManualPreSync);
                                }
                                else
                                {
                                    mConfig.ExplorerDao.UpdateRecordStatusAndDestroyedTime4Manual(mSite.ID, recordID, updateStatus);
                                }
                                //mConfig.ExplorerDao.UpdateRecordStatusAndDestroyedTime4Manual(mSite.ID, recordID, updateStatus);
                                DeletedParentWhenDeleteRecord(record);
                            }
                            else
                            {
                                mConfig.ExplorerDao.UpdateRecordStatusAndDestroyedTime(mSite.ID, recordID, updateStatus);
                            }
                            mLog.Info("Update Record Status successful");
                            if (updateStatus == 8)
                            {
                                ArchiverIndexDto indexDto = null; ;
                                try
                                {
                                    indexDto = Convert2ArchiverIndexDto(mConfig.GetArchiverIndex(pathMd5));
                                }
                                catch (Exception e)
                                {
                                    mLog.Error($"Error occurred while getting archiver index. Node id:{nodeID} Error:{e.ToString()}");
                                }
                                //ecord.AppendMetaInfoForArchiverIndex(SerializerHelper.SerializeByDataContractJsonSerializer(indexDto));
                                mConfig.ExplorerDao.AddArchivedRelatedColumn(mSite.ID, recordID, pathMd5, mConfig.CurrentIndexJobID, indexDto != null ? SerializerHelper.SerializeByDataContractJsonSerializer(indexDto) : null);
                                mLog.Info("Update archived custom column successful");
                            }
                        }
                        else
                        {
                            mLog.Info("Current object:{0} doesn't exist in explore.", recordID);
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("Update Record Status Failed.Message:{0}.", ex.ToString());
                    }
                }
            }
        }

        private void DeletedParentWhenDeleteRecord(Record record)
        {
            if (record.SourceFlag == (int)SourceFlag.OneDrive && record.ParentId != Guid.Empty)
            {
                try
                {
                    //Delete List
                    if(record.ParentId == record.ListId)
                    {
                        var existItemUnderList = mConfig.ExplorerDao.Exist(
                        r => r.ParentId == record.ParentId
                        && r.Id != record.Id
                        && r.IsManualSynced
                        && (r.ManualInternalApprovedStatus == (int)Contract.SOApproveDBStatus.WaitingApprove || r.ManualInternalApprovedStatus == (int)Contract.SOApproveDBStatus.WorkflowInProgress)
                        );

                        var existFolderUnderList = mConfig.ExplorerDao.Exist(
                        r => r.ParentId == record.ParentId
                        && r.Id != record.Id
                        && r.IsManualSynced
                        && r.NodeType == (int)NodeLevel.Folder
                        );
                        if(!existItemUnderList && !existFolderUnderList)
                        {
                            var listItem = mConfig.ExplorerDao.GetFirstOrDefault(r => r.NodeId == record.ParentId);
                            listItem.IsManualSynced = false;
                            mConfig.ExplorerDao.Upsert(listItem);
                            mLog.Info($"Delete reocrd [{record.Id}] parent list [{listItem.Id}] success");
                        }
                        return;
                    }

                    //Delete Folder
                    var existItemUnderParent = mConfig.ExplorerDao.Exist(
                        r => r.ParentId == record.ParentId 
                        && r.Id != record.Id 
                        && r.IsManualSynced 
                        && (r.ManualInternalApprovedStatus == (int)Contract.SOApproveDBStatus.WaitingApprove || r.ManualInternalApprovedStatus == (int)Contract.SOApproveDBStatus.WorkflowInProgress)
                        );
                    if (!existItemUnderParent)
                    {
                        var parentItem = mConfig.ExplorerDao.GetFirstOrDefault(r => r.NodeId == record.ParentId);
                        parentItem.IsManualSynced = false;
                        mConfig.ExplorerDao.Upsert(parentItem);
                        mLog.Info($"Delete reocrd [{record.Id}] parent folder [{parentItem.Id}] success");
                        DeletedParentWhenDeleteRecord(parentItem);
                    }
                }
                catch (Exception e)
                {
                    mLog.Info($"Delete reocrd [{record.Id}] parent folder failed, error: {e}");
                }
            }
        }

        private Record GetRecordInExplorerDao(Guid nodeID)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverDeletion.GetRecordInExplorerDao"))
            {
                Guid recordID = ScheduleConfiguration.GetRecordId(mSite.ID, nodeID);
                if (mConfig.IsILMode && mConfig.ExplorerDao != null)
                {
                    try
                    {
                        Record record = null;
                        if (mConfig.IsRelativeDataJob)
                        {
                            record = mConfig.ExplorerDao.ReadById(mSite.ID, recordID);
                        }
                        else
                        {
                            record = mConfig.exploreDBSPRecords.Where(x => x.ScopeId == mSite.ID && x.Id == recordID).FirstOrDefault();
                            if (record == null && mConfig.exploreDBSPRecords.Count >= 10000)
                            {
                                record = mConfig.ExplorerDao.ReadById(mSite.ID, recordID);
                            }
                        }
                        return record;
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("Update Record Status Failed.Message:{0}.", ex.ToString());
                        return null;
                    }
                }
                else
                { return null; }
            }
        }

        private void AddManualHistory(Record record)
        {
            ManualUtil manualUtil = new ManualUtil(mConfig);
            manualUtil.AddManualHistory(record);
        }
        public ArchiverIndexDto Convert2ArchiverIndexDto(ArchiverBasicIndex index)
        {
            ArchiverIndexDto dto = new ArchiverIndexDto()
            {
                ContentDataFileNumber = index.ContentDataFileNumber,
                ContentDataHeaderOffset = index.ContentDataHeaderOffset,
                ContentDataOffset = index.ContentDataOffset,
                ContentLength = index.ContentLength,
                ContentOffset = index.ContentOffset,
                ContentPageSize = index.ContentPageSize,
                DataFileLength = index.DataFileLength,
                DataFileNumber = index.DataFileNumber,
                DataFileOffset = index.DataFileOffset,
                Flag = index.Flag,
                JobId = index.JobId,
                MetaDataHeaderOffset = index.MetaDataHeaderOffset,
                Version = index.Version
            };
            return dto;
        }


        #region Backup + Stub获取Backup/Restore 当前层folder(不适用于Stub Only)

        //实例化到Subfolder,始终使用RootFolder对象去Get SubFolder
        private Wrapper.Restore.AveSPFolder GetStubRestoreAveCurrentFolder(string subFolderUrl, Guid parentFolderId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveDeletion.GetStubRestoreAveCurrentFolder"))
            {
                if (!backupStubRestoreAveSPCurrentFolder.Id.Equals(parentFolderId))
                {
                    mLog.Info("Deletion GetStubRestoreAveCurrentFolder StubSubFolderUrl:{0}.", subFolderUrl);
                    backupStubRestoreAveSPCurrentFolder = GetRestoreSubAveSPFolder(mConfig.StubRestoreAveSPRootFolder, subFolderUrl);
                    return backupStubRestoreAveSPCurrentFolder;
                }
                else
                {
                    return backupStubRestoreAveSPCurrentFolder;
                }
            }
        }

        private Wrapper.Restore.AveSPFolder GetRestoreSubAveSPFolder(Wrapper.Restore.AveSPFolder parentFolder, string destFolderUrl)
        {
            if (string.IsNullOrEmpty(destFolderUrl))
            {
                return parentFolder;
            }
            if (!destFolderUrl.Contains("/"))
            {
                Wrapper.Restore.AveSPFolder subFolder = new Wrapper.Restore.AveSPFolder(parentFolder, destFolderUrl);
                subFolder.InitSPFolder();
                return subFolder;
            }
            int pos = destFolderUrl.IndexOf("/");
            if (pos > -1)
            {
                string subDest = destFolderUrl.Substring(0, pos);
                string subLastDest = destFolderUrl.Substring(pos + 1);
                Wrapper.Restore.AveSPFolder subFolder = new Wrapper.Restore.AveSPFolder(parentFolder, subDest);
                subFolder.InitSPFolder();
                return this.GetRestoreSubAveSPFolder(subFolder, subLastDest);
            }
            return parentFolder;
        }

        /// <summary>
        /// 每个List缓存Root Folder，然后每个SubFolder单独获取，如果是同一个Subfolder则不需要重新实例化
        /// </summary>
        private Wrapper.Backup.AveSPFolder GetCurrentAveBackupFolder(IAveFolder folder)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.ReGetStubRestoreAveSPFolder"))
            {
                Wrapper.Backup.AveSPFolder result;
                if (folder.UniqueId != backupStubBackupAveSPCurrentFolder.Id)
                {
                    mLog.Info("Deletion GetCurrentAveBackupFolder Current folder :{0} doesn't match StubBackupAveSPCurrentFolder:{1} that need get new folder.", folder.ServerRelativeUrl, backupStubBackupAveSPCurrentFolder.ServerRelativeUrl);
                    result = new Wrapper.Backup.AveSPFolder(GetCurrentAveBackupFolderByRootFolder(folder.ParentFolder), folder.Name, folder.UniqueId, folder.ID, 512/*folder.Item.Versions[0].VersionId*/);//version多时有效率问题，默认赋值1.0
                    backupStubBackupAveSPCurrentFolder = result;
                }
                else
                {
                    result = backupStubBackupAveSPCurrentFolder;
                }
                return result;
            }
        }

        private Wrapper.Backup.AveSPFolder GetCurrentAveBackupFolderByRootFolder(IAveFolder folder)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.GetCurrentAveBackupFolderByRootFolder"))
            {
                Wrapper.Backup.AveSPFolder result;
                if (string.IsNullOrEmpty(folder.ServerRelativeUrl))
                {
                    mLog.Error("folder ServerRelativeUrl is empty.");
                    throw new RALeaveStubException("File Not Found.");
                }
                if (folder.UniqueId != mConfig.StubBackupAveSPRootFolder.Id)
                {
                    mLog.Info("Deletion GetCurrentAveBackupFolderByRootFolder Current folder :{0} doesn't match StubBackupAveSPRootFolder:{1} that need get new folder.", folder.ServerRelativeUrl, mConfig.StubBackupAveSPRootFolder.ServerRelativeUrl);
                    result = new Wrapper.Backup.AveSPFolder(GetCurrentAveBackupFolderByRootFolder(folder.ParentFolder), folder.Name, folder.UniqueId, folder.ID, 512/*folder.Item.Versions[0].VersionId*/);//version多时有效率问题，默认赋值1.0
                }
                else
                {
                    result = mConfig.StubBackupAveSPRootFolder;
                }
                return result;
            }
        }

        #endregion

        #endregion

        //add for RevIM folder rule keepdata
        private void KeepFolderData()
        {
            keepData.SetReportInfo(mReportInfo.Url, mReportInfo.MediaName, mReportInfo.RuleName, mReportInfo.SubJobId, mReportInfo.Size);
            Guid itemID = new Guid(mHeaderInfo.Attributes[KeyWord.ID].Value);
            int archiveLevel = GetArchiveLevel();
            string siteUrl = mHeaderInfo.GetAttribute(KeyWord.SiteUrl);
            Guid webID = new Guid(mHeaderInfo.GetAttribute(KeyWord.WebId));
            Guid listID = new Guid(mHeaderInfo.GetAttribute(KeyWord.ListId));
            bool.TryParse(mHeaderInfo.GetAttribute(KeyWord.IsRepeatProcess), out bool IsRepeatProcess);
            //create tag column
            string value = String.Concat(listID.ToString(), "|", webID.ToString(), "|", siteUrl);
            if (!checkForCreateTagList.Contains(value))//如果存在不创建
            {
                keepData.CreateTagColumn(listID, webID, siteUrl);
                checkForCreateTagList.Add(value);
            }
            keepData.KeepFolderData(itemID, archiveLevel, siteUrl, webID, listID, false, mHeaderInfo.Attributes[KeyWord.URL].Value, IsRepeatProcess);
        }

        private void KeepDocumnetData()
        {
            //Folder Rule, skip add tag for document
            if (mConfig.currentRule != null
                 && mConfig.currentRule.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.Folder
                 && mConfig.currentRule.Name.Equals(mReportInfo.RuleName))
            {
                mLog.Info(string.Format("Skip document: {0} in keep folder action.", mReportInfo.Url));
                return;
            }

            keepData.SetReportInfo(mReportInfo.Url, mReportInfo.MediaName, mReportInfo.RuleName, mReportInfo.SubJobId, mReportInfo.Size);
            Guid docID = new Guid(mHeaderInfo.Attributes[KeyWord.ID].Value);
            int UIVersion = Convert.ToInt32(mHeaderInfo.Attributes[KeyWord.VERSION].Value);
            int level = Convert.ToInt32(mHeaderInfo.Attributes[KeyWord.LEVEL].Value);
            int archiveLevel = GetArchiveLevel();
            bool isVersion = Convert.ToBoolean(mHeaderInfo.Attributes[KeyWord.ISVERSION].Value);
            string siteUrl = mHeaderInfo.GetAttribute(KeyWord.SiteUrl);
            Guid webID = new Guid(mHeaderInfo.GetAttribute(KeyWord.WebId));
            Guid listID = new Guid(mHeaderInfo.GetAttribute(KeyWord.ListId));
            keepData.KeepDocumnetData(docID, UIVersion, archiveLevel, level, siteUrl, webID, listID, isVersion);
        }
        private void KeepItemData()
        {
            //Folder Rule, skip add tag for document
            if (mConfig.currentRule != null
                 && mConfig.currentRule.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.Folder
                 && mConfig.currentRule.Name.Equals(mReportInfo.RuleName))
            {
                mLog.Info(string.Format("Skip document: {0} in keep folder action.", mReportInfo.Url));
                return;
            }
            keepData.SetReportInfo(mReportInfo.Url, mReportInfo.MediaName, mReportInfo.RuleName, mReportInfo.SubJobId, mReportInfo.Size);
            Guid itemID = new Guid(mHeaderInfo.Attributes[KeyWord.ID].Value);
            int UIVersion = Convert.ToInt32(mHeaderInfo.Attributes[KeyWord.VERSION].Value);
            int level = Convert.ToInt32(mHeaderInfo.Attributes[KeyWord.LEVEL].Value);
            int archiveLevel = GetArchiveLevel();
            bool isVersion = Convert.ToBoolean(mHeaderInfo.Attributes[KeyWord.ISVERSION].Value);
            string leafName = Convert.ToString(mHeaderInfo.Attributes[KeyWord.PATH].Value);
            string siteUrl = mHeaderInfo.GetAttribute(KeyWord.SiteUrl);
            Guid webID = new Guid(mHeaderInfo.GetAttribute(KeyWord.WebId));
            Guid listID = new Guid(mHeaderInfo.GetAttribute(KeyWord.ListId));
            keepData.KeepItemData(itemID, UIVersion, archiveLevel, level, siteUrl, webID, listID, isVersion);

        }
        private void KeepAttachmentData()
        {
            keepData.SetReportInfo(mReportInfo.Url, mReportInfo.MediaName, mReportInfo.RuleName, mReportInfo.SubJobId, mReportInfo.Size);
            Guid attachmentID = new Guid(mHeaderInfo.Attributes[KeyWord.ID].Value);
            int UIVersion = Convert.ToInt32(mHeaderInfo.Attributes[KeyWord.VERSION].Value);
            int level = Convert.ToInt32(mHeaderInfo.Attributes[KeyWord.LEVEL].Value);
            int archiveLevel = GetArchiveLevel();
            string leafName = Convert.ToString(mHeaderInfo.Attributes[KeyWord.PATH].Value);
            string listOrFolderPath = mHeaderInfo.GetAttribute(KeyWord.URL);
            IAveListItem item = GetListItemForAttachment(leafName, listOrFolderPath);
            keepData.KeepAttachmentData(attachmentID, item, level, archiveLevel, UIVersion, Site.Url, leafName, Web.ID, List.ID, false);
        }
        private IAveListItem GetListItemForAttachment(string attName, string listOrFolderPath)
        {
            IAveListItem listItem = null;
            try
            {
                if (attName.Contains("_.000") && !attName.StartsWith(":", StringComparison.OrdinalIgnoreCase))
                {
                    int itemNum = Convert.ToInt32(attName.Substring(0, attName.IndexOf("_", StringComparison.OrdinalIgnoreCase)));
                    listItem = List.GetItemById(itemNum);
                }
                else
                {
                    string folderName = listOrFolderPath.Substring(listOrFolderPath.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1);
                    var parentFolder = from folder in List.Folders.OfType<IAveListItem>()
                                       where folder.Name == folderName
                                       select folder;//831
                    if (parentFolder.ToList<IAveListItem>().Count > 0)
                    {
                        listItem = parentFolder.ToList<IAveListItem>()[0];
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Info("Item does not exist: {0}", ex.ToString());
            }
            return listItem;
        }

        [SuppressMessage("FxCopCustomRules", "C100003:DoNotUseSpecificSPMethod")]
        private void DeleteDocumentVersion()
        {
            bool shouldReport = true;
            Guid fileId = Guid.Empty;
            int archiveLevel = GetArchiveLevel();
            int uiVersion = 0;
            try
            {
                uiVersion = int.Parse(mHeaderInfo.Attributes[KeyWord.VERSION].Value);
                fileId = new Guid(mHeaderInfo.Attributes[KeyWord.ID].Value);
                string fileUrl = mReportInfo.Url.Substring(0, mReportInfo.Url.LastIndexOf(":", StringComparison.OrdinalIgnoreCase));
                IAveFile currentFile = Web.GetFile(fileId, fileUrl);
                if (!currentFile.Exists)
                {
                    try
                    {
                        if (currentFile.InDocumentLibrary)
                        {
                            if (!TakeOverCheckOutFile(Web, currentFile, fileId))
                            {
                                if (mConfig.Action == ArchiverAction.RELATIVEDATA_ARCHIVER_BACKUP_JOB_REQUEST)
                                {
                                    mReportInfo.AddRelativeDataArchiverReport(SPNodeLevel.ItemVersion);
                                }
                                return;
                            };
                        }
                    }
                    catch (FileNotFoundException ex)
                    {
                        mLog.Info(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteDocumentVersiondelete, ex.ToString());
                        //mConfig.soArchiverQueryWorkerForDel.UpdateArchiveDeletionStatus(SOApproveDBStatus.Failed, fileId, archiveLevel, uiVersion, mReportInfo.SubJobId);
                        mBackupDeleteLowLevelStatus = false;
                        if (mConfig.Action == ArchiverAction.RELATIVEDATA_ARCHIVER_BACKUP_JOB_REQUEST)
                        {
                            mReportInfo.AddRelativeDataArchiverReport(SPNodeLevel.ItemVersion);
                        }
                        return;
                    }
                }
                if (mConfig.BackgroundSettings.SkipExtentionName.Exists(f => currentFile.Name.EndsWith(f, StringComparison.OrdinalIgnoreCase)))
                {
                    mLog.Info("Skip delete file version: {0} of file: {1} due to extension name is in skip list.", uiVersion, currentFile.Name);
                    shouldReport = false;
                    JobExecutionProgressStatisticExecutor.Instance.DecreaseTotalMatchedRuleFiles();
                    return;
                }
                IAveFileVersion fileVersion = null;
                new AveTaskRetryHelper(5, true).ExecuteWithRetryMechanism(() =>
                {
                    //add retry logic due to some customer get version error.
                    fileVersion = currentFile.Versions.GetVersionFromID(uiVersion);
                });
                if (fileVersion == null || fileVersion.IsCurrentVersion)
                {
                    shouldReport = false;
                    return;
                }
                if (mConfig.CheckItemIsRecordsHold(fileId))
                {
                    mLog.Warn("Document version is RecordsHold. document id: {0}.", currentFile.UniqueId);
                    //mConfig.soArchiverQueryWorkerForDel.UpdateArchiveDeletionStatus(SOApproveDBStatus.Failed, fileId, archiveLevel, uiVersion, mReportInfo.SubJobId);
                    mReportInfo.Status = JobDetailsStatus.Skipped;
                    mReportInfo.Message = "StorageOptimization_EXOExploreHoldFile";
                    return;
                }
                if (!CheckItemModifyTime(currentFile.TimeLastModified))
                {
                    mReportInfo.SetFailedInfo(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteDocumentVersionModified);
                    //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, fileId, archiveLevel, mReportInfo.SubJobId);
                    mBackupDeleteLowLevelStatus = false;
                    return;
                }
                //delete
                ListItemComplianceInfo complianceInfo = null;
                bool needRestoreComplianceTag = false;
                try
                {
                    GetComplianceTagIfEnableRemove(currentFile.Item, out complianceInfo);
                    if (!mConfig.currentRule.LockRecordBeforeDestroy &&
                        complianceInfo != null && complianceInfo.TagPolicyHold && !complianceInfo.TagPolicyRecord &&
                        IsRecordTypeComplianceTag(Site, complianceInfo.ComplianceTag))
                    {
                        mLog.Info("skip Delete current unlock status item. Item Name: {0}.", currentFile.Item.Name);
                        mReportInfo.Status = JobDetailsStatus.Skipped;
                        mReportInfo.Message = "StorageOptimization_Skip_Unlock_Status_Item";
                        return;
                    }
                    DeleteComplianceTagIfEnableRemove(currentFile.Item, complianceInfo, out needRestoreComplianceTag);
                    SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(fileVersion.Size, mConfig.GetNodeFullPath(mReportInfo.Url));
                    fileVersion.Delete();
                    SetComplianceTagIfEnableRemove(currentFile.Item, complianceInfo);
                    needRestoreComplianceTag = false;
                    //mConfig.soArchiverQueryWorkerForDel.UpdateArchiveDeletionStatus(SOApproveDBStatus.Archived, fileId, archiveLevel, uiVersion, mReportInfo.SubJobId);
                    mLog.Info("delete document version:{0} of {1}", uiVersion, fileId);
                }
                catch (Exception e)
                {
                    mLog.Warn("File Version Delete Error: {0} version: {1} error message: {2}", node.FullPath, currentFile.UIVersion, e.ToString());
                    if(needRestoreComplianceTag)
                    {
                        SetComplianceTagIfEnableRemove(currentFile?.Item, complianceInfo);
                    }
                    mReportInfo.ExceptionTackle(e.Message, SPNodeLevel.DocumentVersion.ToString());
                }
            }
            catch (Exception e)
            {
                mLog.Error("delete file version:{0} of file:{1} failed:{2}", uiVersion, node.FullPath, e);
                mBackupDeleteLowLevelStatus = false;
                //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, fileId, archiveLevel, mReportInfo.SubJobId);
                mReportInfo.ExceptionTackle(e.Message, SPNodeLevel.DocumentVersion.ToString());

                if (e.Message != null && e.Message.Contains("This item cannot be updated because it is locked as read-only"))
                {
                    mReportInfo.SetFailedInfo("StorageOptimization13_SOARDeleteOfficeLockFile");
                    mLog.Warn("[ArchiverDeletion][DeleteDocumentVersion]This item cannot be updated because it is locked as read-only.");
                    mConfig.JobReportDto.HasErrorNode = true;
                }
                else if (e.Message != null && e.Message.Contains("The label that's applied to this item prevents it from being edited or deleted. Check the item's label for more details"))
                {
                    mReportInfo.SetFailedInfo("StorageOptimization_SOARRecordManagerLabelDocumentDeleteFailed");
                    mLog.Warn("[ArchiverDeletion][DeleteDocumentVersion]The label that's applied to this item prevents it from being edited or deleted. Check the item's label for more details.");
                    mConfig.JobReportDto.HasErrorNode = true;
                }
            }
            finally
            {
                if (shouldReport)
                {
                    if (mConfig.Action == ArchiverAction.RELATIVEDATA_ARCHIVER_BACKUP_JOB_REQUEST)
                    {
                        mReportInfo.AddRelativeDataArchiverReport(SPNodeLevel.ItemVersion, mConfig.currentRule.KeepDataOption == (int)KeepDataOption.LinkDocument);
                    }
                    else
                    {
                        mReportInfo.AddDeletionReport((int)CacheNodeType.ItemVersion, "SO_Action_Delete");
                    }
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100003:DoNotUseSpecificSPMethod")]
        private void DeleteAttachment()
        {
            Guid attachmentId = new Guid(mHeaderInfo.Attributes[KeyWord.ID].Value);
            string attName = mHeaderInfo.GetAttribute(KeyWord.PATH);
            int archiveLevel = GetArchiveLevel();
            try
            {
                string listOrFolderPath = mHeaderInfo.GetAttribute(KeyWord.URL);
                RealDeleteAttachment(attName, listOrFolderPath);
                //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Archived, attachmentId, archiveLevel, mReportInfo.SubJobId);
            }
            catch (Exception ex)
            {
                mBackupDeleteLowLevelStatus = false;
                //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, attachmentId, archiveLevel, mReportInfo.SubJobId);
                mLog.Error(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteAttachment, attName, ex.ToString());
                mReportInfo.ExceptionTackle(ex.Message, "Item");
                if(ex.Message != null && ex.Message.Contains("This item cannot be updated because it is locked as read-only"))
                {
                    mReportInfo.SetFailedInfo("StorageOptimization13_SOARDeleteOfficeLockFile");
                    mLog.Warn("[ArchiverDeletion][DeleteAttachment]This item cannot be updated because it is locked as read-only.");
                    mConfig.JobReportDto.HasErrorNode = true;
                }
                else if(ex.Message != null && ex.Message.Contains("The label that's applied to this item prevents it from being edited or deleted. Check the item's label for more details"))
                {
                    mReportInfo.SetFailedInfo("StorageOptimization_SOARRecordManagerLabelDocumentDeleteFailed");
                    mLog.Warn("[ArchiverDeletion][DeleteAttachment]The label that's applied to this item prevents it from being edited or deleted. Check the item's label for more details.");
                    mConfig.JobReportDto.HasErrorNode = true;
                }
            }
            finally
            {
                if (mConfig.Action == ArchiverAction.RELATIVEDATA_ARCHIVER_BACKUP_JOB_REQUEST)
                {
                    mReportInfo.AddRelativeDataArchiverReport(SPNodeLevel.Attachment);
                }
                else
                {
                    mReportInfo.AddDeletionReportToUpdateItemStatus((int)CacheNodeType.Attachment, "SO_Action_Delete", attachmentId, 0);
                    SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(mReportInfo.Size, mConfig.GetNodeFullPath(mReportInfo.Url));
                }
            }
        }

        private void RealDeleteAttachment(string attName, string listOrFolderPath)
        {
            IAveListItem listItem = null;
            //Micro Feed没有Attachment Rule，并且Micro Feed Attachment与Post/Reply一同删除，部分情况此处删除会有权限问题
            if (List.BaseTemplate == AveListTemplateType.MicroFeed)
            {
                mLog.Info(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteAttachmentsucceed, " ");
                return;
            }
            #region Get ListItem
            try
            {
                if (attName.Contains("_.000") && !attName.StartsWith(":", StringComparison.OrdinalIgnoreCase))
                {
                    Guid parentId = new Guid(mHeaderInfo.Attributes[KeyWord.ParentId].Value);
                    //已经load过不需要再load，直接从字典去获取想要的listItem
                    if (checkForLoadList.ContainsKey(parentId))
                    {
                        listItem = checkForLoadList[parentId];
                    }
                    else
                    {
                        int itemNum = Convert.ToInt32(attName.Substring(0, attName.IndexOf("_", StringComparison.OrdinalIgnoreCase)));
                        listItem = List.GetItemById(itemNum);
                        checkForLoadList[listItem.UniqueId] = listItem;//将已经load的listItem添加到字典
                    }
                }
                else
                {
                    string folderName = listOrFolderPath.Substring(listOrFolderPath.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1);
                    var parentFolder = from folder in List.Folders.OfType<IAveListItem>()
                                       where folder.Name == folderName
                                       select folder;//831
                    if (parentFolder.ToList<IAveListItem>().Count > 0)
                        listItem = parentFolder.ToList<IAveListItem>()[0];
                    else
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Info("Item does not exist: {0}", ex.ToString());
                return;
            }
            #endregion
            ListItemComplianceInfo complianceInfo = null;
            bool needRestoreComplianceTag = false;
            try
            {
                GetComplianceTagIfEnableRemove(listItem, out complianceInfo);
                if (!mConfig.currentRule.LockRecordBeforeDestroy &&
                    complianceInfo != null && complianceInfo.TagPolicyHold && !complianceInfo.TagPolicyRecord &&
                    IsRecordTypeComplianceTag(Site, complianceInfo.ComplianceTag))
                {
                    mLog.Info("skip Delete current attchment in unlock status item. Item Name: {0}.", listItem.Name);
                    mReportInfo.Status = JobDetailsStatus.Skipped;
                    mReportInfo.Message = "StorageOptimization_Skip_Unlock_Status_Item";
                    return;
                }
                DeleteComplianceTagIfEnableRemove(listItem, complianceInfo, out needRestoreComplianceTag);
                DateTime itemModifyTime = (DateTime)listItem["Modified"];   //SAAS-11014 获取在删除Attachment之前的listItem的Modified。
                attName = attName.Substring(attName.IndexOf(":", StringComparison.OrdinalIgnoreCase) + 1);
                //ADO-123176   Office365 的Attachment 删除会涨Version，外围添加逻辑控制，先关掉List 的Version  Add by Dong Xie 2014.5.16
                if (List.EnableVersioning) 
                { 
                    StopEnableVersionList();
                }
                listItem.Attachments.Delete(attName);
                listItem["Modified"] = itemModifyTime;   //SAAS-11014 保持在删除Attachment之后的Modified不改变。
                listItem.SystemUpdate();
                if (SetComplianceTagIfEnableRemove(listItem, complianceInfo))
                {
                    needRestoreComplianceTag = false;
                    listItem.FieldValues[SPColumnConstants.SP_ComplianceTag] = complianceInfo?.ComplianceTag;
                }
            }
            catch (Exception e)
            {
                #region delete office 365 declare item which have attachment
                if (ScheduleConfiguration.CheckisRecord(listItem))
                {
                    mLog.Info("Office365 Begin CheckItemIsRecord");
                    if (ArchiverCommonStaticMethod.CheckIsRecordOnly(listItem))
                    {
                        mLog.Info("This attachment is belong to Office 365 Declare Item,Item Name is {0} ,attachment Name is {1}.", listItem.Name, attName);
                        lock (attachmentCacheLock)
                        {
                            if (!mConfig.cacheRecordAttachments.ContainsKey(listItem.UniqueId))
                            {
                                List<string> attachmentNames = new List<string>();
                                attachmentNames.Add(attName);
                                mConfig.cacheRecordAttachments.Add(listItem.UniqueId, attachmentNames);
                            }
                            else
                            {
                                List<string> attachmentNames = mConfig.cacheRecordAttachments[listItem.UniqueId];
                                if (!attachmentNames.Contains(attName))
                                {
                                    attachmentNames.Add(attName);
                                }
                                mConfig.cacheRecordAttachments[listItem.UniqueId] = attachmentNames;
                            }
                        }
                        return;
                    }
                    else
                    {
                        mLog.Warn("This attachment is belong to Office 365 Declare And Hold Item,Item Name is {0} ,attachment Name is {1}.", listItem.Name, attName);
                    }
                }
                else
                {
                    mLog.Warn("Item Attachment {0} Delete Error: {1}", attName, e.ToString());
                }
                #endregion

                throw;
            }
            finally
            {
                if (needRestoreComplianceTag)
                {
                    SetComplianceTagIfEnableRemove(listItem, complianceInfo);
                }
            }
        }


        private void DeleteRootFolderFile(IAveWeb web, Guid docId)
        {
            IAveFile file = web.GetFile(docId, mReportInfo.Url);
            IAveFolder rootFolder = web.RootFolder;
            string welcomePage = rootFolder.WelcomePage;
            ListItemComplianceInfo complianceInfo = null;
            bool needRestoreComplianceTag = false;
            try
            {
                GetComplianceTagIfEnableRemove(file.Item, out complianceInfo);
                if (!mConfig.currentRule.LockRecordBeforeDestroy &&
                    complianceInfo != null && complianceInfo.TagPolicyHold && !complianceInfo.TagPolicyRecord &&
                    IsRecordTypeComplianceTag(Site, complianceInfo.ComplianceTag))
                {
                    mLog.Info("skip Delete current unlock status item. Item Name: {0}.", file.Item.Name);
                    mReportInfo.Status = JobDetailsStatus.Skipped;
                    mReportInfo.Message = "StorageOptimization_Skip_Unlock_Status_Item";
                    return;
                }
                DeleteComplianceTagIfEnableRemove(file.Item, complianceInfo, out needRestoreComplianceTag);
                rootFolder.WelcomePage = "";
                rootFolder.Update();
                file.Delete();
                needRestoreComplianceTag = false;
                mLog.Info("delete root folder file:{0}", mReportInfo.Url);
                return;
            }
            catch (Exception e)
            {
                mLog.Error(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteDocumentOccurred, mHeaderInfo.OuterXml, e.ToString());
                if (needRestoreComplianceTag)
                {
                    SetComplianceTagIfEnableRemove(file?.Item, complianceInfo);
                }

                mReportInfo.ExceptionTackle(e.Message, "Item");
                if (e.Message != null && e.Message.Contains("This item cannot be updated because it is locked as read-only"))
                {
                    mReportInfo.SetFailedInfo("StorageOptimization13_SOARDeleteOfficeLockFile");
                    mLog.Warn("[ArchiverDeletion][DeleteRootFolderFile]This item cannot be updated because it is locked as read-only.");
                    mConfig.JobReportDto.HasErrorNode = true;
                }
                else if (e.Message != null && e.Message.Contains("The label that's applied to this item prevents it from being edited or deleted. Check the item's label for more details"))
                {
                    mReportInfo.SetFailedInfo("StorageOptimization_SOARRecordManagerLabelDocumentDeleteFailed");
                    mLog.Warn("[ArchiverDeletion][DeleteRootFolderFile]The label that's applied to this item prevents it from being edited or deleted. Check the item's label for more details.");
                    mConfig.JobReportDto.HasErrorNode = true;
                }
            }
            finally
            {
                rootFolder.WelcomePage = welcomePage;
                rootFolder.Update();//还原welcomePage属性
            }

        }
        #endregion

        #region Method

        private int GetArchiveLevel()
        {
            int archiveLevel = 0;
            if (mHeaderInfo.HasAttribute(KeyWord.MYLEVEL))
            {
                archiveLevel = Convert.ToInt32(mHeaderInfo.GetAttribute(KeyWord.MYLEVEL));
            }
            return archiveLevel;
        }

        /// <summary>
        /// Only add for Office365 Delete Attachment 
        /// </summary>
        private void StopEnableVersionList()
        {
            if (List.EnableVersioning)
            {
                List.EnableVersioning = false;
                List.Update();
                mEnableVersionList.Add(List.ID);
            }
        }


        private void StartEnableVersionList()
        {
            try
            {
                if (mEnableVersionList.Contains(mList.ID))
                {
                    mList.EnableVersioning = true;
                    mList.Update();
                    mEnableVersionList.Remove(mList.ID);
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("Start Enable List Version failed" + ex.ToString());
            }
        }

        /// <summary>
        /// 用于获取系统用户上传，并被非系统用户checkout的document
        /// </summary>
        /// <param name="web"></param>
        /// <param name="fileId"></param>
        /// <param name="checkOutUserId"></param>
        /// <returns></returns>
       /* private IAveFile LoadCheckOutFile(IAveWeb web, Guid fileId, int checkOutUserId)
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
        }*/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="docId"></param>
        /// <returns></returns>
        private bool TakeOverCheckOutFile(IAveWeb web, IAveFile file, Guid docId)
        {
            try
            {
                IAveDocumentLibrary docList = List as IAveDocumentLibrary;
                IList<IAveCheckedOutFile> checkOutFiles = docList.CheckedOutFiles;
                int count = 0;
                foreach (IAveCheckedOutFile cofile in checkOutFiles)
                {
                    if (cofile.LeafName.Equals(file.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        cofile.TakeOverCheckOut();
                        file = web.GetFile(docId, mReportInfo.Url);
                        if (!file.Exists)
                        {
                            return false;
                        }
                        break;
                    }
                    count++;
                }
                if (count >= checkOutFiles.Count)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                mLog.Info("Take Over Check Out File Error: {0}", e.ToString());
                return false;
            }
            return true;
        }
        private bool ChecklistAllowDeletion(IAveList list, Guid listId, int archiveLevel)
        {
            bool listAllowDeletion = true;
            if (!List.AllowDeletion)
            {
                mLog.Info(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteListCannotDelete);
                if (mConfig.currentRule != null && mConfig.currentRule.PolicyLevel == PolicyLevel.List)
                {
                    mReportInfo.SetReportStatus(JobDetailsStatus.Failed, LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteListCannotDelete);
                    mLog.Warn("[ArchiverDeletion][ChecklistAllowDeletion]List acnnot delete.");
                    mConfig.JobReportDto.HasErrorNode = true;
                }
                else
                {
                    mReportInfo.SetReportStatus(JobDetailsStatus.Skipped, LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteListCannotDelete);
                }
                //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, List.ID, archiveLevel, mReportInfo.SubJobId);
                return false;
            }

            if(IsChannelSiteDefaultLib(List))
            {
                mReportInfo.SetReportStatus(JobDetailsStatus.Skipped, LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteListCannotDelete);
                return false;
            }

            if ((mConfig.currentRule.KeepDataOption & (int)KeepDataOption.NotBackup) != (int)KeepDataOption.NotBackup)
            {
                //We must Reload List Here ,Because O365 List cache the Items(tmpList.Items) ,No matter We Delete the items or Not.
                IAveList tmpList = Web.GetList(listId);
                foreach (IAveListItem listItem in tmpList.Items)
                {
                    mLog.Info("Current list has list item.List title:{0}.listItem:{1}.", tmpList.Title, listItem.Name);
                    bool needSkip = false;
                    bool isFolder = false;
                    AveObjectModelFactory factory = mConfig.aveObjectModelFactory;
                    var workflowTaskContentTypeId = factory?.CreateContentTypeId(AveBuiltInContentTypeId.WorkflowTask);
                    if (!mConfig.BackgroundSettings.SkipExtentionName.Exists(f => listItem.Name.EndsWith(f, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (listItem.ContentType != null && listItem.ContentType.ID != null && workflowTaskContentTypeId != null && listItem.ContentType.ID.IsChildOf(workflowTaskContentTypeId))
                        {
                            mLog.Info("Current list has list item and listItem ContentType is WorkflowTask.List title:{0}.listItem:{1}.listItemPath:{2}.", tmpList.Title, listItem.Name, listItem.Url);
                        }
                        else
                        {
                            //不是aspx/js/css 输出文件信息，用来诊断
                            needSkip = true;
                            mLog.Warn($"Current list:{tmpList.Title} has list item:{listItem.Name} URL:{listItem.Url} can not be deleted.");
                        }
                    }
                    else
                    {
                        //aspx/js/css 输出文件信息，用来诊断
                        if (List.BaseTemplate == AveListTemplateType.WebPageLibrary && mConfig.currentRule != null && (
                            mConfig.currentRule.PolicyLevel == PolicyLevel.Teams 
                            || mConfig.currentRule.PolicyLevel == PolicyLevel.SiteCollection
                            ))
                        {
                            needSkip = true;
                        }
                        mLog.Info($"Current list:{tmpList.Title} has system item:{listItem.Name} URL:{listItem.Url}.");
                    }
                    var folderContentTypeId = factory?.CreateContentTypeId("0x0120");
                    if (listItem.ContentType != null && listItem.ContentType.ID != null && folderContentTypeId != null && listItem.ContentType.ID.IsChildOf(folderContentTypeId))
                    {
                        mLog.Info("Current list has list item and listItem ContentType is 0x0120.List title:{0}.listItem:{1}.listItemPath:{2}.", tmpList.Title, listItem.Name, listItem.Url);
                        isFolder = true;
                    }
                    if (needSkip && !isFolder)
                    {
                        listAllowDeletion = false;
                        mLog.Info("Current list has list item and need skip this list.List title:{0}.listItem:{1}.listItemPath:{2}.listItemCreated:{3}.", tmpList.Title, listItem.Name, listItem.Url, GetListItemCreated(listItem));
                    }
                }
            }
            if (listAllowDeletion == false)
            {
                //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, List.ID, archiveLevel, mReportInfo.SubJobId);
                mReportInfo.SetReportStatus(JobDetailsStatus.Skipped, LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteListItemCannotDelete);   //SAAS-13093 设置为skip状态
            }
            return listAllowDeletion;
        }

        // skipValidateList true if already passed NeedValidateListForSiteDeletion check, no need to check again
        private bool UpdateNonStubFileRemainFlag(IAveList tmpList, IAveListItem listItem, List<int> lockStubIds, bool skipValidateList = false)
        {
            if (mConfig.currentRule.PolicyLevel != PolicyLevel.Teams && mConfig.currentRule.PolicyLevel != PolicyLevel.SiteCollection)
                return false;
            if (_isNonStubFileRemain)
                return false;

            if (!skipValidateList && !NeedValidateListForSiteDeletion(tmpList)) // no need to check this list if it will be skip validate when deleting site anyway
            {
                mLog.Debug($"Skip checking item because list:{tmpList.Title} does not need validation for site deletion.");
                return false;
            }

            try
            {
                if (!LinkFileCommon.IsStubFileType(listItem.Name))  // if file has stub suffix
                {
                    mLog.Info($"Current list:{tmpList.Title} has list item:{listItem.Name.LogBase64()} which is not a stub file.");
                    _isNonStubFileRemain = true;
                    return true;
                }

                var linkFileValue = tmpList.Fields.ContainsField(LinkFileCommon.LinkFileFieldName)
                    ? listItem.Properties[LinkFileCommon.LinkFileFieldName]?.ToString()
                    : null;
                if (!string.IsNullOrEmpty(linkFileValue))
                {
                    mLog.Info($"Current list:{tmpList.Title} has list item:{listItem.Name.LogBase64()} which is an OPUS stub file by LinkFile field validation.");
                }
                else
                {
                    string fileContent = string.Empty;
                    if (listItem.File != null)
                    {
                        using (Stream fileStream = listItem.File.OpenBinaryStream())
                        {
                            using (StreamReader reader = new StreamReader(fileStream))
                            {
                                fileContent = reader.ReadToEnd();
                            }
                        }
                    }
                    var isOpusStubContent = !string.IsNullOrEmpty(mConfig.ReCenterURL)
                        && !string.IsNullOrEmpty(fileContent)
                        && fileContent.IndexOf(mConfig.ReCenterURL, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!isOpusStubContent)
                    {
                        mLog.Info($"Current list:{tmpList.Title} has list item:{listItem.Name.LogBase64()} which is not an OPUS stub file by content validation.");
                        _isNonStubFileRemain = true;
                        return true;
                    }
                    mLog.Info($"Current list:{tmpList.Title} has list item:{listItem.Name.LogBase64()} which is an OPUS stub file by content validation.");
                }

                var tagName = listItem.GetComplianceTagName();
                if (string.IsNullOrWhiteSpace(tagName))
                {
                    return false;
                }
                HandleRetentionLabelOnStub(tmpList, listItem, lockStubIds);
                mLog.Info($"Current list:{tmpList.Title} has listItem id: {listItem.ID}, path:{listItem.FullPath()} which is a stub file. linkFileValue: {linkFileValue}, label need to remove: {tagName}");
            }
            catch (Exception e)
            {
                mLog.Error($"Failed to check if list item is a stub file. List:{tmpList.Title} Item:{listItem.Name} URL:{listItem.Url}. Exception:{e}");
                _isNonStubFileRemain = true;
                return true;
            }

            return false;
        }

        private void HandleRetentionLabelOnStub(IAveList tmpList, IAveListItem listItem, List<int> lockStubIds)
        {
            var complianceInfo = listItem.GetComplianceInfo();
            var isRecordTypeLabel = IsRecordTypeComplianceTag(Site, complianceInfo.ComplianceTag);

            if (!string.IsNullOrWhiteSpace(complianceInfo?.ComplianceTag))
            {
                if (isRecordTypeLabel)
                {
                    if (!complianceInfo.TagPolicyRecord && complianceInfo.TagPolicyHold && isRecordTypeLabel)
                    {
                        // Current status of label is unlock status. So start lock label before remove the current label
                        listItem.LockRecordItem();
                    }
                    //listItem.SetComplianceTagOnBulkItems("");
                    lockStubIds.Add(listItem.ID);
                }
                else
                {
                    // Current label is not record label and need to remove it
                    //listItem.SetComplianceTagOnBulkItems("");
                    lockStubIds.Add(listItem.ID);
                }
            }

            if (lockStubIds.Count >= 20)
            {
                mLog.Info($"lock stub ids found reach limit, start processing.");
                ClearRetentionLabelsOnStubs(tmpList, lockStubIds); // clear retention labels on stub files in batch of 20 before continuing
            }
        }

        // if found any non-stub file remain, just stop. Process further is meaningless
        private void ClearRetentionLabelsOnStubs(IAveList list, List<int> itemIds)
        {
            if (itemIds.IsNullOrEmpty())
            {
                return;
            }

            if (mConfig.currentRule.PolicyLevel != PolicyLevel.Teams && mConfig.currentRule.PolicyLevel != PolicyLevel.SiteCollection)
                return;
            if (_isNonStubFileRemain)
            {
                itemIds.Clear();
                mLog.Info($"Found non-stub files remain, so skip clearing retention labels on stub files.");
                return;
            }

            try
            {
                mLog.Info($"Start clearing retention labels on stub files. List:{list.Title}, ItemIds:{string.Join(",", itemIds)}.");
                list.SetComplianceTagOnBulkItems(itemIds, "");
            }
            catch (Exception e)
            {
                mLog.Error($"Failed to clear retention labels on stub files. List:{list.Title} ItemIds:{string.Join(",", itemIds)}. Exception:{e}");
                foreach (var itemId in itemIds)
                {
                    try
                    {
                        list.SetComplianceTagOnBulkItems([itemId],"");
                    }
                    catch (Exception ex)
                    {
                        mLog.Error($"Failed to clear retention label on stub file. List:{list.Title} ItemId:{itemId}. Exception:{ex}");
                    }
                }   
            }

            mLog.Info($"Finished clearing retention labels on stub files. List:{list.Title}, itemCount: {itemIds.Count}.");
            itemIds.Clear();
        }

        private bool NeedValidateListForSiteDeletion(IAveList list, bool needLog = false)
        {
            if (SharepointUtil.CheckIsDesignList(list))
            {
                if (needLog)
                    mLog.Info("Current list is design list.List title:{0}.List Url:{1}.", list.Title, list.DefaultViewUrl);
                return false;
            }

            if (((list.Hidden
                || list.Title.Equals("{System Folder}", StringComparison.OrdinalIgnoreCase))
                || (!list.AllowDeletion
                    && !mConfig.BackgroundSettings.ListTemplateTable.Contains((int)list.BaseTemplate))
                || list.IsSiteAssetsLibrary))
            {
                if (needLog)
                    //system list
                    mLog.Info("Current list is system list.List title:{0}.List Url:{1}.", list.Title, list.DefaultViewUrl);
                return false;
            }

            if (list.BaseTemplate == AveListTemplateType.DesignCatalog
                || list.BaseTemplate == AveListTemplateType.MasterPageCatalog
                || list.BaseTemplate == AveListTemplateType.WebPageLibrary
                || list.BaseTemplate == AveListTemplateType.ThemeCatalog
                || list.Hidden || list.IsCatalog)
            {
                if (needLog)
                    //system list
                    mLog.Info("Current list is system list or Catalog.List title:{0}.List Url:{1}.", list.Title, list.DefaultViewUrl);
                return false;
            }

            return true;
        }


        /// <summary>
        /// return  "true" mains only has system list  return "false" mains normal list
        /// </summary>
        /// <param name="lists"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100003:DoNotUseSpecificSPMethod")]
        private bool ChecklistAllowDeletion(IAveListCollection lists, ref string errorDetail)
        {
            bool onlySystemList = true;
            foreach (IAveList list in lists)
            {
                var isCurrentListAllowDeletion = true;
                List<int> lockStubIds = [];
                if (IsChannelSiteDefaultLib(list))
                {
                    mLog.Info("Current list is channel site default lib.List title:{0}.List Url:{1}.", list.Title, list.DefaultViewUrl);
                    continue;
                }

                if (!NeedValidateListForSiteDeletion(list, true))
                {
                    continue;
                }
                
                if (!list.AllowDeletion && list.ItemCount == 0)
                {
                    //some system list which itself can't delete 
                    mLog.Info("Current list is not allow deletion.List title:{0}.List Url:{1}.", list.Title, list.DefaultViewUrl);
                    continue;
                }
                else if (!list.AllowDeletion && list.BaseTemplate.Equals(AveListTemplateType.MicroFeed))
                {
                    //DefinitionId不为1是Mirco Feed List第二个Folder中的Item，直接删除即可。
                    mLog.Info("Current list is not allow deletion.List title:{0}.BaseTemplate is MicroFeed.List Url:{1}.", list.Title, list.DefaultViewUrl);
                    foreach (IAveListItem listItem in list.Items)
                    {
                        mLog.Info("Current list is not allow deletion.List title:{0}.BaseTemplate is MicroFeed.List Url:{1}.listItem Name:{2}.", list.Title, list.DefaultViewUrl, listItem.Name);
                        if (listItem.Fields.ContainsField("DefinitionId") && listItem["DefinitionId"] != null && listItem["DefinitionId"].ToString().Equals("1"))
                        {
                            mLog.Info("Current list is not allow deletion.List title:{0}.BaseTemplate is MicroFeed.DefinitionId is 1.listItem Name:{1}.", list.Title, listItem.Name);
                            onlySystemList = false;
                        }
                    }
                    continue;
                }
                else if (!list.AllowDeletion && list.ItemCount > 0)
                {
                    mLog.Info("Current list is not allow deletion and ItemCount > 0.List Url:{0}.List title:{1}.ItemCount:{2}.", list.DefaultViewUrl, list.Title, list.ItemCount);
                    //为Office 365 添加处理，office365 API  list.Items 返回的对象，包含了Folder ，导致判断了Folder 的name 是不是我们不删除的，这样就会使得此处认为是List 下有文件没删，Container级别不删 Add by Dong Xie
                    List<Guid> folderID = new List<Guid>();
                    try
                    {
                        foreach (IAveListItem listFolder in list.Folders)
                        {
                            folderID.Add(listFolder.UniqueId);
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Info($"ChecklistAllowDeletion.Failed get folders.Meesage:{ex}.");
                    }
                    //declare record list
                    foreach (IAveListItem listItem in list.Items)
                    {
                        mLog.Info($"Current list is not allow deletion and ItemCount > 0.List title:{list.Title}.ItemName:{listItem.Name}.URL:{listItem.Url}.ID:{listItem.ID}.UniqueId:{listItem.UniqueId}.FileSystemObjectType:{listItem.FileSystemObjectType}.");
                        if (folderID.Contains(listItem.UniqueId) || listItem.FileSystemObjectType == AveFileSystemObjectType.Folder)
                        {
                            continue;
                        }
                        if (ScheduleConfiguration.CheckisRecord(listItem) || !mConfig.BackgroundSettings.SkipExtentionName.Exists(f => listItem.Name.EndsWith(f, StringComparison.OrdinalIgnoreCase)))
                        {
                            mLog.Info("Current list is not allow deletion and ItemCount > 0 and item is record or not keep document.List title:{0}.ItemName:{1}.", list.Title, listItem.Name);
                            onlySystemList = false;
                            isCurrentListAllowDeletion = false;
                        }

                        // additionally check for not allow deletion list
                        if (!isCurrentListAllowDeletion)
                        {
                            UpdateNonStubFileRemainFlag(list, listItem, lockStubIds, true);
                        }
                    }

                    ClearRetentionLabelsOnStubs(list, lockStubIds);
                }
                else
                {
                    //List 允许删除且Item Count = 0的情况有两种：
                    //1.Archiver Job过程中新创建的List
                    //2.有些系统List，虽然已经删除，但是有些会随着Deletion操作再次触发创建出来
                    //以上两种只要List下没有数据都直接删除List
                    if (list.ItemCount == 0)
                    {
                        mLog.Info("Current list ItemCount is 0.List title:{0}.List Url:{1}.", list.Title, list.DefaultViewUrl);
                    }
                    else
                    {
                        //此条Log是为了验证list.ItemCount和list.Items.Count不一致的问题.
                        mLog.Info("Current list ItemCount > 0.List title:{0}.List Url:{1}.list.Items Count:{2}.", list.Title, list.DefaultViewUrl, list.Items.Count);
                        foreach (IAveListItem listItem in list.Items)
                        {
                            mLog.Info("Current list ItemCount > 0 and allow deletion.List title:{0}.listItem Name:{1}.List Url:{2}.", list.Title, listItem.Name, list.DefaultViewUrl);
                            if (!mConfig.BackgroundSettings.SkipExtentionName.Exists(f => listItem.Name.EndsWith(f, StringComparison.OrdinalIgnoreCase)))
                            {
                                mLog.Info("Current list ItemCount > 0 and allow deletion and not keep document.List title:{0}.listItem Name:{1}.listItem Created:{2}.", list.Title, listItem.Name, GetListItemCreated(listItem));
                                onlySystemList = false;
                                isCurrentListAllowDeletion = false;
                            }

                            // additionally check for found file in nonSystemList
                            if (!isCurrentListAllowDeletion)
                            {
                                UpdateNonStubFileRemainFlag(list, listItem, lockStubIds, true);
                            }
                        }

                        ClearRetentionLabelsOnStubs(list, lockStubIds);
                    }
                }
            }
            return onlySystemList;
        }

        /// <summary>
        /// 避免Listitem获取不到Created属性出异常
        /// </summary>
        /// <param name="listItem"></param>
        /// <returns></returns>
        private string GetListItemCreated(IAveListItem listItem)
        {
            string listItemCreated = string.Empty;
            try
            {
                if (listItem != null)
                {
                    listItemCreated = listItem["Created"].ToString();
                }
            }
            catch (Exception ex)
            {
                mLog.Info("Can't GetListItemCreated.Message:{0}.", ex.Message);
            }
            return listItemCreated;
        }

        private bool CheckWebAllowDelete(IAveSite site)
        {
            if (site.AllWebs.Count > 1)
            {
                foreach (IAveWeb web in site.AllWebs)
                {
                    if (!web.IsAppWeb && !web.IsRootWeb)
                    {
                        mLog.Info("CheckWebAllowDelete.Current web is:{0}.", web.ServerRelativeUrl);
                        return false;
                    }
                }
            }
            return true;
        }
        private bool CheckSiteIsHold(IAveSite site)
        {
            Hashtable allProperties = site?.RootWeb?.AllProperties;
            if(allProperties != null && allProperties.ContainsKey("allwebholds") && allProperties["allwebholds"] != null)
            {
                string[] holds = allProperties["allwebholds"].ToString().Split(';');
                foreach (string hold in holds)
                {
                    String stringHoldEndTime = hold.Split(',').Last();
                    if (DateTime.TryParse(stringHoldEndTime, out DateTime holdEndTime) && DateTime.UtcNow < holdEndTime)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private bool CheckItemModifyTime(DateTime modifyTime)
        {
            DateTime archiveTime = DateTime.MinValue;
            //返回比较结果，并对mItemTimeCompareRet
            string archiveTimeStr = mHeaderInfo.GetAttribute(KeyWord.TIME);
            if (archiveTimeStr.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                archiveTime = new DateTime(long.Parse(archiveTimeStr));
                mLog.Info("DBArchivedTime is:{0},ObjectModifyTime is:{1}.", archiveTime, modifyTime);
                return !(modifyTime.CompareTo(archiveTime) > 0);
            }
        }

        private JobStatus LoadJobStatus(string jobId)
        {
            JobStatus state = JobStatus.Wait;
            try
            {
                var relativeDataJobState = mConfig.JobReportDto.GetRelativeDataJobState(jobId);
                if (string.IsNullOrEmpty(relativeDataJobState) || "NotBackup".Equals(relativeDataJobState, StringComparison.OrdinalIgnoreCase))
                {
                    mLog.Info("End User Job State is null or NotBackup.");
                    return state;
                }
                state = (JobStatus)Enum.Parse(typeof(JobStatus), relativeDataJobState);
            }
            catch (Exception ex)
            {
                mLog.Info("Can not in load job status, reason : " + ex.ToString());
            }
            return state;
        }

        public void PreDeleteSiteCollection(DeletionNode node)
        {
            mHeaderInfo = node.HeaderInfo;
            if (preCheckSiteUrl != Site.Url)
            {
                UpdateDenyAddAndCustomizePagesStatus();
                preCheckSiteUrl = Site.Url;
            }
            if (preCheckWebUrl != Web.Url)
            {
                DisableAllLookupEnforceRelationship();
                preCheckWebUrl = Web.Url;
            }
        }

        private void UpdateDenyAddAndCustomizePagesStatus()
        {
            try
            {
                if (Site.DenyAddAndCustomizePagesStatus)
                {
                    mConfig.denyAddAndCustomizePagesStatus = Site.DenyAddAndCustomizePagesStatus.ToString();
                    mLog.Info("Current Site:{0} DenyAddAndCustomizePagesStatus is is true and need set value to false.", Site.Url);
                    Site.DenyAddAndCustomizePagesStatus = false;
                }
            }
            catch (Exception ex)
            {
                mLog.Info("UpdateDenyAddAndCustomizePagesStatus failed.Message:{0}.", ex.ToString());
            }
        }

        /// <summary>
        /// Failed Delete Site need revert status.
        /// </summary>
        private void RevertDenyAddAndCustomizePagesStatus()
        {
            try
            {
                if (!string.IsNullOrEmpty(mConfig.denyAddAndCustomizePagesStatus))
                {
                    Site.DenyAddAndCustomizePagesStatus = Convert.ToBoolean(mConfig.denyAddAndCustomizePagesStatus);
                    mLog.Info("Current Site:{0} DenyAddAndCustomizePagesStatus need revert status.", Site.Url);
                }
            }
            catch (Exception ex)
            {
                mLog.Info("RevertDenyAddAndCustomizePagesStatus failed.Message:{0}.", ex.ToString());
            }
        }

        private void DisableAllLookupEnforceRelationship()
        {
            try
            {
                mLog.Info("Begin DisableAllLookupEnforceRelationship.");
                foreach (IAveList list in Web.Lists)
                {
                    if (list.Hidden || list.Title.Equals("{System Folder}", StringComparison.OrdinalIgnoreCase))
                    {
                        mLog.Info("Current list is system list.List title:{0}.List Url:{1}. when DisableAllLookupEnforceRelationship.", list.Title, list.DefaultViewUrl);
                        continue;
                    }
                    List<IAveField> lookupFields = list.Fields.Where(x => x.Type == AveFieldType.Lookup && !x.Hidden && !x.ReadOnlyField && !AveBuiltInFieldId.Contains(x.ID)).ToList();
                    foreach (IAveField field in lookupFields)
                    {
                        IAveFieldLookup aveFieldLookup = field as IAveFieldLookup;
                        if (aveFieldLookup.IsRelationship && aveFieldLookup.RelationshipDeleteBehavior != AveRelationshipDeleteBehavior.None)
                        {
                            mLog.Info("Current list is contains lookup column and IsRelationship is true.List title:{0}.List Url:{1}.field StaticName:{2}.", list.Title, list.DefaultViewUrl, field.StaticName);
                            aveFieldLookup.IsRelationship = false;
                            aveFieldLookup.RelationshipDeleteBehavior = AveRelationshipDeleteBehavior.None;
                            aveFieldLookup.Update();
                        }
                    }
                }
                mLog.Info("End DisableAllLookupEnforceRelationship.");
            }
            catch (Exception ex)
            {
                mLog.Info("DisableAllLookupEnforceRelationship failed.Message:{0}.", ex.ToString());
            }
        }

        private void ListPostActionDeletion()
        {
            try
            {
                if (mConfig.TasksCacheItemDtoCollection.Count != 0)
                {
                    mLog.Info("Archiver begin Post deletion for Tasks item.");
                    int count = 0;
                    while (true)
                    {
                        if (mConfig.TasksCacheItemDtoCollection.Count > 1 && count < 100)
                        {
                            count++;
                            foreach (CacheItemDto cacheItemDto in mConfig.TasksCacheItemDtoCollection)
                            {
                                IAveListItem cacheItem = cacheItemDto.CacheItem;
                                mLog.Info(string.Format("begin delete Tasks item {0}, count :{1}.", cacheItem.Title, count));
                                DeleteListItem(cacheItemDto);
                            }
                            foreach (CacheItemDto cacheItemDto in mConfig.NeedDeleteTasksCacheItemDtoCollection)
                            {
                                mConfig.TasksCacheItemDtoCollection.Remove(cacheItemDto);
                            }
                        }
                        else
                        {
                            foreach (CacheItemDto cacheItemDto in mConfig.TasksCacheItemDtoCollection)
                            {
                                IAveListItem cacheItem = cacheItemDto.CacheItem;
                                mLog.Info(string.Format("begin delete Tasks item {0}.", cacheItem.Title));
                                DeleteListItem(cacheItemDto, true);
                                mLog.Info("end delete Tasks item. ");
                            }
                            break;
                        }
                    }
                    mConfig.TasksCacheItemDtoCollection.Clear();
                    mConfig.NeedDeleteTasksCacheItemDtoCollection.Clear();
                    mLog.Info("Archiver end Post deletion for Tasks item.");
                }
            }
            catch (Exception ex)
            {
                mLog.Info("An error occur while PostActionDeletion:", ex.ToString());
            }
        }

        private void DeleteListItem(CacheItemDto cacheItemDto, bool isExceedThreshold = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("SP2013ArchiveBackUp.DeleteListItem"))
            {
                int archiveLevel = cacheItemDto.ArchiverLevel;
                IAveListItem listItem = cacheItemDto.CacheItem;
                Guid ListItemId = listItem.UniqueId;
                bool readyForReport = true;
                try
                {
                    if (cacheItemDto.BaseTemplate == AveListTemplateType.TasksWithTimelineAndHierarchy)
                    {
                        try
                        {
                            IAveList list = listItem.ParentList;
                            //Web.ReloadWeb();
                            //list = Web.GetList(listItem.ParentList.RootFolder.ServerRelativeUrl);
                            IEnumerable<IAveListItem> items = from subItem in list.Items where subItem["ParentID"] != null && subItem["ParentID"].ToString().Substring(0, subItem["ParentID"].ToString().IndexOf(';')).Equals(listItem.ID.ToString(), StringComparison.OrdinalIgnoreCase) select subItem;
                            if (items.Count<IAveListItem>() > 0)
                            {
                                if (isExceedThreshold)
                                {
                                    //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, ListItemId, archiveLevel, mReportInfo.SubJobId);
                                    mReportInfo.ExceptionTackle(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteTaskItemWithSubTask, SPNodeLevel.Item.ToString());
                                    mLog.Warn(string.Format("Can not delete task {0},because it has subtask.", listItem.Title));
                                }
                                else
                                {
                                    readyForReport = false;
                                }
                                return;
                            }
                            else
                            {
                                mConfig.NeedDeleteTasksCacheItemDtoCollection.Add(cacheItemDto);
                            }
                        }
                        catch (Exception ex)
                        {
                            mLog.Error(string.Format("This is some exceptions of deleting tasks item {0}.Error:{1}", listItem.Title, ex.ToString()));
                        }
                    }
                    //delete
                    if (listItem != null)
                    {
                        string name = listItem.Name;
                        ListItemComplianceInfo complianceInfo = null;
                        bool needRestoreComplianceTag = false;
                        try
                        {
                            GetComplianceTagIfEnableRemove(listItem, out complianceInfo);
                            if (!mConfig.currentRule.LockRecordBeforeDestroy &&
                                complianceInfo != null && complianceInfo.TagPolicyHold && !complianceInfo.TagPolicyRecord &&
                                IsRecordTypeComplianceTag(Site, complianceInfo.ComplianceTag))
                            {
                                mLog.Info("skip Delete current unlock status item. Item Name: {0}.", listItem.Name);
                                mReportInfo.Status = JobDetailsStatus.Skipped;
                                mReportInfo.Message = "StorageOptimization_Skip_Unlock_Status_Item";
                                return;
                            }
                            if (ScheduleConfiguration.CheckisRecord(listItem) && mConfig.currentRule.DeleteRecords)
                            {
                                Record.UndeclareItemAsRecord(listItem);
                            }
                            DeleteComplianceTagIfEnableRemove(listItem, complianceInfo, out needRestoreComplianceTag);                            
                            CaculateItemSize(listItem);
                            listItem.Delete();
                            needRestoreComplianceTag = false;
                        }
                        catch (Exception spex)
                        {
                            if (needRestoreComplianceTag)
                            {
                                SetComplianceTagIfEnableRemove(listItem, complianceInfo);
                            }
                            mLog.Warn("List Item {0} Delete Error: {1}", name, spex.ToString());
                            throw;
                        }
                        mLog.Info(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteListItemsucceed, name);
                        //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Archived, ListItemId, archiveLevel, mReportInfo.SubJobId);
                        mReportInfo.Status = JobDetailsStatus.Successful;
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error(LOGRESOURCE.StorageOptimization13_SOARArchiverDeletionDeleteListItemCreateliststub, ex.ToString());
                    //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, ListItemId, archiveLevel, mReportInfo.SubJobId);
                    mReportInfo.ExceptionTackle(ex.ToString(), SPNodeLevel.Item.ToString());
                    if (ex.Message != null && ex.Message.Contains("This item cannot be updated because it is locked as read-only"))
                    {
                        mReportInfo.SetFailedInfo("StorageOptimization13_SOARDeleteOfficeLockFile");
                        mLog.Warn("[ArchiverDeletion][DeletelistAllowDeletion]This item cannot be updated because it is locked as read-only.");
                        mConfig.JobReportDto.HasErrorNode = true;
                    }
                    else if (ex.Message != null && ex.Message.Contains("The label that's applied to this item prevents it from being edited or deleted. Check the item's label for more details"))
                    {
                        mReportInfo.SetFailedInfo("StorageOptimization_SOARRecordManagerLabelDocumentDeleteFailed");
                        mLog.Warn("[ArchiverDeletion][DeletelistAllowDeletion]The label that's applied to this item prevents it from being edited or deleted. Check the item's label for more details.");
                        mConfig.JobReportDto.HasErrorNode = true;
                    }
                }
                finally
                {
                    mReportInfo.Url = cacheItemDto.Url;
                    if (readyForReport)
                    {
                        mReportInfo.AddDeletionReportToUpdateItemStatus((int)CacheNodeType.Item, "SO_Action_Delete", ListItemId, 0, versionSize);
                    }
                }
            }
        }

        private void FolderPostActionDeletion()
        {
            Guid folderId = new Guid(mHeaderInfo.Attributes[KeyWord.ID].Value);
            if (mConfig.GetAllDeleteFolderCacheDto().Contains(folderId))
            {
                IAveFolder aveFolder = List.Folders[folderId].Folder;
                if (aveFolder.Files.Count == 0 && aveFolder.SubFolders.Count == 0)
                {
                    GetComplianceTagIfEnableRemove(aveFolder.Item, out ListItemComplianceInfo complianceInfo);
                    DeleteComplianceTagIfEnableRemove(aveFolder.Item, complianceInfo, out bool deletedTag);
                    string folderUrl = aveFolder.ServerRelativeUrl;
                    aveFolder.Delete();
                    mConfig.RemoveDeleteFolderCache(folderId);
                    JobExecutionProcessStatisticExecutor.Instance.CalculateSuccessDeleteAndStubSummary(JobDetailsStatus.Successful, mConfig.currentRule.Id, "SO_Action_Delete", (int)CacheNodeType.Folder);
                    mConfig.JobReportDto.AddDeletionReport(mConfig.GetNodeFullPath(folderUrl), 0, JobDetailsStatus.Successful, (int)CacheNodeType.Folder, mConfig.JobId, mConfig.currentRule.Name, "", "SO_Action_Delete", "StorageOptimization_SOARSODeletionParentFolder");
                    mLog.Info("Delete Parent Folder :{0} success.", folderUrl);
                }
                else
                {
                    mLog.Info("Current folder :{0} not empty.Files Count:{1}.SubFolders Count:{2}.", aveFolder.ServerRelativeUrl, aveFolder.Files.Count, aveFolder.SubFolders.Count);
                }
            }
        }
        #endregion
    }

    public class ReportInfo
    {
        #region Property
        public JobDetailsStatus Status { get; set; }
        public string Message { get; set; }
        public string Url { get; set; }
        public string SubJobId { get; set; }
        public string RuleName { get; set; }
        public string MediaName { get; set; }
        public long Size { get; set; }
        #endregion Property

        #region Private Member
        private ScheduleConfiguration mConfig;
        private string mErrorDetail = string.Empty;
        private CGDBReader dbReader = null;
        #endregion

        public ReportInfo(ScheduleConfiguration Configuration)
        {
            mConfig = Configuration;
            if (Configuration.ArchiverExtendSetting != null && Configuration.ArchiverExtendSetting.IsCGDiscovery)
            {
                dbReader = CGDBReader.GetInstance(mConfig.ArchiverExtendSetting, mConfig.SiteCollectionID.ToString(), mConfig.SiteCollectionUrl);
            }
        }

        private void Init()
        {
            Url = string.Empty;
            MediaName = string.Empty;
            RuleName = string.Empty;
            SubJobId = string.Empty;
            Size = 0;
            Status = JobDetailsStatus.Successful;
            Message = string.Empty;
        }

        #region Public Method
        public void GetBasicInfo(XmlElement stubInfo)
        {
            Init();
            Url = stubInfo.GetAttribute(KeyWord.URL).Replace("\\", "/");
            MediaName = stubInfo.GetAttribute(KeyWord.MEDIANAME);
            RuleName = stubInfo.GetAttribute(KeyWord.RULENAME);
            SubJobId = stubInfo.GetAttribute(KeyWord.SUBJOBID);
            Size = long.Parse(stubInfo.GetAttribute(KeyWord.SIZE));
        }

        public void AddDeletionReport(int nodeLevel, string keepData, long size = 0)
        {
            JobExecutionProcessStatisticExecutor.Instance.CalculateSuccessDeleteAndStubSummary(Status, mConfig?.currentRule?.Id, keepData, nodeLevel);
            //SAAS-13414 使report可以传入参数。
            mConfig.JobReportDto.AddDeletionReport(nodeLevel == (int)CacheNodeType.SiteCollection ? Url : mConfig.GetNodeFullPath(Url),
                                                     Size + size,
                                                     Status,
                                                     nodeLevel,
                                                     SubJobId,
                                                     RuleName,
                                                     MediaName,
                                                     keepData,
                                                     Message,
                                                     mErrorDetail);
        }

        public void AddDeletionVersionReport(int nodeLevel, string keepData, string path, long size = 0)
        {
            //SAAS-13414 使report可以传入参数。
            JobExecutionProcessStatisticExecutor.Instance.CalculateSuccessDeleteAndStubSummary(Status, mConfig?.currentRule.Id, keepData, nodeLevel);
            mConfig.JobReportDto.AddDeletionReport(mConfig.GetNodeFullPath(path),
                                                     Size + size,
                                                     Status,
                                                     nodeLevel,
                                                     SubJobId,
                                                     RuleName,
                                                     MediaName,
                                                     keepData,
                                                     Message,
                                                     mErrorDetail);
        }

        public void AddDeletionReportToUpdateItemStatus(int nodeLevel, string keepData, Guid ItemId, long shouldDeleteObjectTotalSize, long size = 0)
        {
            AddDeletionReport(nodeLevel, keepData, size);
            var archiverExtendSetting = mConfig.ArchiverExtendSetting;
            if (archiverExtendSetting != null && archiverExtendSetting.IsCGDiscovery)
            {
                dbReader.UpdateStatusAndArchiveSize(mConfig.SiteCollectionID.ToString(), ItemId, ConvertToBackupRestoreStatus(Status), shouldDeleteObjectTotalSize, mConfig.ArchiverUNCTime);
            }
        }
        public void AddDeleteOnlyVersionReport(int nodeLevel, string keepData, string versionLabel, long size = 0)
        {
            JobExecutionProcessStatisticExecutor.Instance.CalculateSuccessDeleteAndStubSummary(Status, mConfig?.currentRule?.Id, keepData, nodeLevel);
            string verisonUrl = mConfig.GetNodeFullPath(Url) + ":" + versionLabel;
            mConfig.JobReportDto.AddDeletionReport(verisonUrl,
                                                    size,
                                                    Status,
                                                    nodeLevel,
                                                    SubJobId,
                                                    RuleName,
                                                    MediaName,
                                                    keepData,
                                                    Message,
                                                    mErrorDetail);
        }
        public void UpdateItemStatusForDeleteOnlyVersion(Guid ItemId, long fileTotalSize = 0)
        {
            var archiverExtendSetting = mConfig.ArchiverExtendSetting;
            var tempStatus = ConvertToBackupRestoreStatus(Status);
            if (archiverExtendSetting != null && archiverExtendSetting.IsCGDiscovery)
            {
                if (tempStatus == BackupRestoreStatus.Failed || tempStatus == BackupRestoreStatus.Skipped || fileTotalSize == 0)
                {
                    dbReader.UpdateStatus(mConfig.SiteCollectionID.ToString(), ItemId, ConvertToBackupRestoreStatus(Status));
                }
                else if (tempStatus == BackupRestoreStatus.Succeed && fileTotalSize != 0)
                {
                    dbReader.UpdateStatusAndArchiveSize(mConfig.SiteCollectionID.ToString(), ItemId, ConvertToBackupRestoreStatus(Status), fileTotalSize, mConfig.ArchiverUNCTime);
                }
            }
        }
        private BackupRestoreStatus ConvertToBackupRestoreStatus(JobDetailsStatus status)
        {
            switch (status)
            {
                case JobDetailsStatus.Successful:
                    return BackupRestoreStatus.Succeed;
                case JobDetailsStatus.Failed:
                    return BackupRestoreStatus.Failed;
                case JobDetailsStatus.Skipped:
                    return BackupRestoreStatus.Skipped;
                default:
                    return BackupRestoreStatus.UnKnown;
            }
        }
        public void AddRelativeDataArchiverReport(SPNodeLevel nodeLevel, bool isLeaveLinkAction = false)
        {
            //JobDetail detail = new JobDetail()
            //{
            //    SubJobId = mConfig.JobId,
            //    Type = nodeLevel,
            //    SrcURL = mConfig.GetNodeFullPath(Url),
            //    Size = Size,
            //    Status = (int)Status,
            //    Remark12 = isLeaveLinkAction ? "LeaveLinkInSharePoint" : "Delete",
            //    Message = Message
            //};
            string fileName = Url.Replace('\\', '/').Split('/').Last();
            AddRelativeDataDetail(Size, fileName, mConfig.GetNodeFullPath(Url), (int)nodeLevel, Status, isLeaveLinkAction ? "SO_Action_LevelStub" : "SO_Action_Delete", Message);
            if (Status == JobDetailsStatus.Failed)
            {
                throw new Exception(Message);
            }
        }

        private void AddRelativeDataDetail(long size, string name, string fullPath, int type, JobDetailsStatus status, string action, string comment)
        {
            if (mConfig.RelativeDataJobSourceFlag == (int)SourceFlag.Physical)
            {
                //RM_JS_JM_Related_DeleteRelatedFailed                
                SendPhysicalJobDetail(name, fullPath, PhysicalDisposalActionType.Disposal, String.Empty, ArchiverTypeConvert.ConvertNodeLevelToI18n(type), status, comment);
            }
            else
            {
                SendSPJobDetail(size, fullPath, type, status, action, comment);
            }
        }

        public void SendPhysicalJobDetail(string name, string originPath, PhysicalDisposalActionType action, string destinationPath, string ItemType, JobDetailsStatus status, string comment = "")
        {
            ReportMangerFactory.Instance.ReportManager.SendJobDetail(new JMPhysicalDisposalJobDetails()
            {
                ObjectName = name,
                FullPath = originPath,
                ActionType = GetI18NActionType(action),
                DestinationPath = destinationPath,
                ItemType = ItemType,
                Status = status,
                Comment = comment
            });
        }
        private string GetI18NActionType(PhysicalDisposalActionType action)
        {
            string result = string.Empty;
            switch (action)
            {
                case PhysicalDisposalActionType.Pending:
                    result = "RM_JMD_PD_DisposalAction_Pending";
                    break;
                case PhysicalDisposalActionType.Disposal:
                    result = "RM_JMD_PD_DisposalAction_Dispose";
                    break;
                case PhysicalDisposalActionType.Move:
                    result = "RM_JMD_PD_DisposalAction_Move";
                    break;
                default:
                    result = action.ToString();
                    break;
            }
            return result;
        }
        public void SendSPJobDetail(long nodeSize, string originPath, int cacheNodeType, JobDetailsStatus status, string action, string comment = "")
        {
            JMArchiverActionJobDetails mArchiverActionJobDetails = new JMArchiverActionJobDetails();
            mArchiverActionJobDetails.SourceLocation = originPath;
            mArchiverActionJobDetails.Size = nodeSize.ToString();
            mArchiverActionJobDetails.RuleName = mConfig.currentRule.Name;
            mArchiverActionJobDetails.Status = status;
            mArchiverActionJobDetails.Level = ArchiverTypeConvert.ConvertNodeLevelToI18n(cacheNodeType);
            mArchiverActionJobDetails.ActionTab = (int)ActionTab.Action;
            mArchiverActionJobDetails.Action = action;
            mArchiverActionJobDetails.FinishTime = DateTime.UtcNow.Ticks;
            mArchiverActionJobDetails.Comment = comment;
            ReportMangerFactory.Instance.ReportManager.SendJobDetail(mArchiverActionJobDetails);
            JobExecutionProgressStatisticExecutor.Instance.IncreaseOtherActions();
        }

        public void ExceptionTackle(string errormessage, string DeletionCommons, IAveList list = null)
        {
            if (!mConfig.IsRelativeDataJob)
            {
                mConfig.JobReportDto.AddDeletionCommons(DeletionCommons);
                Message = errormessage;
                if (errormessage != null && 
                    (errormessage.Contains("Item does not exist. It may have been deleted by another user.")
                    || errormessage.Contains("Cannot delete Site Notebook")
                    || errormessage.Contains("Désolé... Nous ne pouvons pas supprimer le bloc-notes du site")))
                {
                    Status = JobDetailsStatus.Skipped;
                }
                else if (DeletionCommons.Equals(SPNodeLevel.Folder.ToString()) 
                    && errormessage != null 
                    && (errormessage.Equals("StorageOptimization_SOTeamsSystemFolderDeleteFailed") 
                    || errormessage.Contains("To delete this folder, go to the channel in Microsoft Teams")))
                {
                    Status = JobDetailsStatus.Skipped;
                    Message = "StorageOptimization_SOTeamsSystemFolderDeleteFailed";
                }
                else if ((mConfig.jobtype == JobType.SpecifySitesArchiverBackup || mConfig.jobtype == JobType.SpecifyTeamsArchiverBackup)
                    && list != null
                    && SharepointUtil.CheckIsDesignList(list))
                {
                    Status = JobDetailsStatus.Skipped;
                }
                else
                {
                    Status = JobDetailsStatus.Failed;
                    mConfig.JobReportDto.HasErrorNode = true;
                }
            }
            // if (mConfig.IsrelativeDataJob)
            // {
            //     Status = JobDetailsStatus.Failed;
            // }
        }

        public void SetFailedInfo(string failedInfo, string errorDetail = default(string))
        {
            Status = JobDetailsStatus.Failed;
            Message = failedInfo;
            mErrorDetail = errorDetail;
        }
        public void SetReportStatus(JobDetailsStatus status, string failedInfo = default(string))
        {
            Status = status;
            Message = failedInfo;
        }
        #endregion
    }

    /// <summary>
    /// Delete Only Keep Version
    /// </summary>
    public class VersionInfo
    {
        public long Size { get; set; }
        public string VersionLabel { get; set; }
    }

    internal class PendingDocumentDeletion
    {
        public string SiteUrl { get; set; }
        public Guid WebId { get; set; }
        public Guid ListId { get; set; }
        public Guid DocId { get; set; }
        public Guid FileUniqueId { get; set; }
        public string FileServerRelativeUrl { get; set; }
        public string ReportUrl { get; set; }
        public string SubJobId { get; set; }
        public string RuleName { get; set; }
        public string MediaName { get; set; }
        public long ShouldDeleteObjectTotalSize { get; set; }
        public string Md5 { get; set; }
        public DestructionReport DestructionReport { get; set; }
        public bool UseRecordUpdate { get; set; }
    }
}
