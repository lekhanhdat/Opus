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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Contract;
using AvePoint.Wrapper.Core.Common;
using AvePoint.Wrapper.Core.SPRestore;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using LS.SPWorkflowProcessor;
using AvePoint.Wrapper.Core.SPRestore.Mapping;
using AvePoint.Wrapper.Mapping;
using System.Text.RegularExpressions;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    public class AveSPList : RestoreableObject, AvePoint.Wrapper.Restore.IAveSPList, IDisposable
    {
        protected static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        //List Title
        private string mName;
        private AveSPWeb mAveSPWeb;
        private IAveBackupRestoreQueryService mQueryService;
        private AveListInfo mListInfo;
        private AveListSettingInfo mListSettingInfo;
        private bool mIsNewCreated = false;
        private bool mNeedContinue = true;
        private IAveList mSPList;
        private IAveListItemSerializer mListItemSerializer;
        private Guid mId;
        private string mUrl;
        private string mOldDefaultViewUrl;
        private string mRootFolderPath;
        private RestoringDto mRestringFolder;
        private bool mStopAlerts = true;
        private List<Guid> mListAlertIDs = new List<Guid>();
        private AveSPListFieldCollection mFields;
        private AveSPListContentTypeCollection mContentTypes;
        private Guid mOldId = Guid.Empty;
        private bool mIsTaxonomyList = false;
        private bool mRequestAccessEnabled = true;
        private string mWelComePage = string.Empty;
        private string mDocumentTemplateUrl = null;
        private string mRssViewFieldXml = string.Empty;
        public string mCacheVersionItems = string.Empty;
        protected IReport reportor = new AveWrapperReport();
        public int mMeetingSeriesDuration = -1;
        private readonly List<int> SpecialListTemplateIdsUnderPersonalSite = new List<int>() { 113, 116, 121, 123, 124, 175 };
        internal Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<int, Guid>>>>> lookupItemUniqueIdCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<int, Guid>>>>>(); // webid,listid,lookup column id, item tp_guid , item rowid  item uniqueid

        public IReport GetReport()
        {
            return reportor;
        }
        private bool? hasUniqueField = null;
        private AveSPSite mAveParentSite;
        private AveListSecurity mSecurity;
        private IAveListItem mPreItem;
        private bool mRestoreRssView = false;
        private Dictionary<Guid, AveRelationshipDeleteBehavior> mRelatedFieldBehavior = new Dictionary<Guid, AveRelationshipDeleteBehavior>();
        private Dictionary<Guid, AveRelationshipDeleteBehavior> mDestFieldBehavior = new Dictionary<Guid, AveRelationshipDeleteBehavior>();
        private List<Dictionary<string, object>> mAlertInfos = new List<Dictionary<string, object>>();
        private List<String> mNeedUpdateToDefaultView = new List<string>();
        private List<String> mNeedUpdateToDefaultContentType = new List<string>();
        private Dictionary<Guid, AveViewInfo> mNeedUpdateSpotlightViews = new Dictionary<Guid, AveViewInfo>();

        /* 当List中有Calculate Column并且使用TODAY() formula的时候:
             1.添加对应的这个Column
             2.在这个List下添加Item
          这两种情况SharePoint API都会让List和Web之间的关联关系丢失，从而使后面的还原操作出现问题
          因此在这两种情况的时候需要reload
        */
        internal bool containsTODAY = false;
        //当containsTODAY 为true的时候，还原List下的第一个Item的时候，会出问题，但是调用一次List.Upate就会好用，
        //因为无法使用List Version来判断是什么地方导致对象不一致，所以没有找到根本原因，因此先用reload
        internal bool firstTime = true;


        //用于标识Fields是还原还是Load的。
        internal bool IsLoadFieldXml = true;
        //当把MicroFeed当成普通Item还原的时候，需要user是System Account
        private bool needsElevation = false;
        //不要随便使用elevatedSite 和 elevatedWeb 两个对象，容易出现对象不一致的情况
        private IAveSite elevatedSite = null;
        private IAveWeb elevatedWeb = null;

        private static Dictionary<Guid, AveBaseType> mTemplateToBaseType
        {
            get
            {
                return new Dictionary<Guid, AveBaseType>()
                {
                    {new Guid("00BFEA71-D1CE-42de-9C63-A44004CE0104"),AveBaseType.GenericList} ,
                    {new Guid("F979E4DC-1852-4F26-AB92-D1B2A190AFC9"),AveBaseType.DocumentLibrary},
                    {new Guid("26676156-91A0-49F7-87AA-37B1D5F0C4D0"),AveBaseType.DocumentLibrary},
                    {new Guid("065C78BE-5231-477e-A972-14177CC5B3C7"),AveBaseType.GenericList },
                    {new Guid("239650e3-ee0b-44a0-a22a-48292402b8d8"),AveBaseType.GenericList},
                    {new Guid("a568770a-50ba-4052-ab48-37d8029b3f47"),AveBaseType.GenericList},
                    {new Guid("00BFEA71-7E6D-4186-9BA8-C047AC750105"),AveBaseType.GenericList},
                    {new Guid("00BFEA71-DE22-43B2-A848-C05709900100"),AveBaseType.GenericList},
                    {new Guid("00BFEA71-DBD7-4F72-B8CB-DA7AC0440130"),AveBaseType.DocumentLibrary},
                    {new Guid("00BFEA71-F381-423D-B9D1-DA7A54C50110"),AveBaseType.DocumentLibrary},
                    {new Guid("00BFEA71-6A49-43FA-B535-D15C05500108"),AveBaseType.GenericList},
                    {new Guid("00BFEA71-E717-4E80-AA17-D0C71B360101"),AveBaseType.DocumentLibrary},
                    {new Guid("00BFEA71-EC85-4903-972D-EBE475780106"),AveBaseType.GenericList},
                    {new Guid("00BFEA71-9549-43f8-B978-E47E54A10600"),AveBaseType.GenericList},
                    {new Guid("58160a6b-4396-4d6e-867c-65381fb5fbc9"),AveBaseType.GenericList},
                    {new Guid("08386d3d-7cc0-486b-a730-3b4cfe1b5509"),AveBaseType.GenericList},
                    {new Guid("00BFEA71-513D-4CA0-96C2-6A47775C0119"),AveBaseType.GenericList},
                    {new Guid("00BFEA71-3A1D-41D3-A0EE-651D11570120 "),AveBaseType.GenericList},
                    {new Guid("071DE60D-4B02-4076-B001-B456E93146FE"),AveBaseType.DocumentLibrary},
                    {new Guid("9ad4c2d4-443b-4a94-8534-49a23f20ba3c"),AveBaseType.GenericList},
                    {new Guid("1C6A572C-1B58-49ab-B5DB-75CAF50692E6"),AveBaseType.GenericList},
                    {new Guid("A0E5A010-1329-49d4-9E09-F280CDBED37D"),AveBaseType.DocumentLibrary},
                    {new Guid("00BFEA71-5932-4F9C-AD71-1557E5751100"),AveBaseType.Issue},
                    {new Guid("6E53DD27-98F2-4AE5-85A0-E9A8EF4AA6DF"),AveBaseType.DocumentLibrary},
                    {new Guid("00BFEA71-2062-426C-90BF-714C59600103"),AveBaseType.GenericList},
                    {new Guid("00BFEA71-F600-43F6-A895-40C0DE7B0117"),AveBaseType.DocumentLibrary},
                    {new Guid("00BFEA71-52D4-45B3-B544-B1C71B620109"),AveBaseType.DocumentLibrary},
                    {new Guid("5D220570-DF17-405e-B42D-994237D60EBF"),AveBaseType.DocumentLibrary},
                    {new Guid("481333E1-A246-4d89-AFAB-D18C6FE344CE"),AveBaseType.GenericList},
                    {new Guid("2510D73F-7109-4ccc-8A1C-314894DEEB3A"),AveBaseType.DocumentLibrary},
                    {new Guid("636287a7-7f62-4a6e-9fcc-081f4672cbf8"),AveBaseType.GenericList},
                    {new Guid("00BFEA71-EB8A-40B1-80C7-506BE7590102"),AveBaseType.Survey},
                    {new Guid("00BFEA71-A83E-497E-9BA0-7A5C597D0107"),AveBaseType.GenericList},
                    {new Guid("d5191a77-fa2d-4801-9baf-9f4205c9e9d2"),AveBaseType.GenericList},
                    {new Guid("29D85C25-170C-4df9-A641-12DB0B9D4130"),AveBaseType.DocumentLibrary},
                    {new Guid("00BFEA71-C796-4402-9F2F-0EB9A6E71B18"),AveBaseType.DocumentLibrary},
                    {new Guid("d7670c9c-1c29-4f44-8691-584001968a74"),AveBaseType.GenericList},
                    {new Guid("9c2ef9dc-f733-432e-be1c-2e79957ea27b"),AveBaseType.GenericList},
                    {new Guid("00BFEA71-4EA5-48D4-A4AD-305CF7030140"),AveBaseType.GenericList},
                    {new Guid("00BFEA71-2D77-4A75-9FCA-76516689E21A"),AveBaseType.GenericList},
                    {new Guid("00BFEA71-1E1D-4562-B56A-F05371BB0115"),AveBaseType.DocumentLibrary},
                    {new Guid("60d1e34f-0eb3-4e56-9049-85daabfec68c"),AveBaseType.GenericList} // project server Issues
                };
            }
        }

        private static List<int> uniqueListTemplates
        {
            get
            {
                return new List<int>
                {
                        {(int)AveListTemplateType.MeetingUser},
                        {(int)AveListTemplateType.Categories},
                        {(int)AveListTemplateType.Posts},
                        {(int)AveListTemplateType.Comments},
                        {(int)AveListTemplateType.MicroFeed},
                        {850},//850 refer to Pages Library
                        {10102}, // 10102 refer to converted form
                        {200}, //200 refer to Meeting Series
                        {212}, //212 refter to WorkSpace Pages
                        {402}, //402 refer to Resources
                        {403}, //403 refer to WhereAbouts
                        {404}, //404 refer to Phone Call Memo
                        {124}, //124 refer to DesignCatalog
                };
            }
        }

        private static List<int> catalogTemplates
        {
            get
            {
                return new List<int>
                {
                        {(int)AveListTemplateType.ListTemplateCatalog},
                        {(int)AveListTemplateType.WebTemplateCatalog},
                        {(int)AveListTemplateType.SolutionCatalog},
                        {(int)AveListTemplateType.ThemeCatalog},
                        {(int)AveListTemplateType.UserInformation},
                        {(int)AveListTemplateType.WebPartCatalog},
                        {(int)AveListTemplateType.MasterPageCatalog}
                };
            }
        }

        public bool MoveConnectorSetting { get; set; }

        private Dictionary<string, List<AveExtendMasterPageInfo>> tempMasterSettings = new Dictionary<string, List<AveExtendMasterPageInfo>>();
        public Dictionary<string, List<AveExtendMasterPageInfo>> TempMasterSettings
        {
            get { return tempMasterSettings; }
        }

        //sp2013 MicroFeed item "RefRoot" and "RefReply" contani data need replace in list postAction.
        internal List<int> PostMicroFeedItem = new List<int>();

        public bool RestoreRssView
        {
            get { return mRestoreRssView; }
            set { mRestoreRssView = value; }
        }
        public AveSPSite ParentSite
        {
            get
            {
                return mAveParentSite;
            }
        }
        public bool HasUniqueField
        {
            get
            {
                if (!hasUniqueField.HasValue)
                {
                    hasUniqueField = mSPList == null ? false : mSPList.Fields.Any<IAveField>(field => field.EnforceUniqueValues);
                    hasUniqueField = (AveFields.needUpdateUniqueValueFields == null ? false : AveFields.needUpdateUniqueValueFields.Count > 0) | hasUniqueField;
                }
                return hasUniqueField.Value;
            }
        }
        public AveObjectSecurity Security
        {
            get
            {
                if (mSecurity == null)
                {
                    mSecurity = new AveListSecurity(this);
                }
                return mSecurity;
            }
        }

        internal List<Guid> ListAlertIDs
        {
            get { return mListAlertIDs; }
        }

        public IAveListItem PreItem
        {
            get
            {
                return mPreItem;
            }
            set
            {
                mPreItem = value;
            }
        }

        public string Url
        {
            get
            {
                return mUrl;
            }
        }

        public string CacheVersionItems
        {
            get { return mCacheVersionItems; }
            set { mCacheVersionItems = value; }
        }

        public bool NeedContinue
        {
            get { return this.mNeedContinue; }
            set { this.mNeedContinue = value; }
        }

        public Exception SkipException { get; private set; }

        public string WelComePage
        {
            get
            {
                return mWelComePage;
            }
        }

        public bool IsTaxonomyList
        {
            get { return mIsTaxonomyList; }
        }

        public Guid OldId
        {
            get { return mOldId; }
        }

        public AveSPListFieldCollection AveFields
        {
            get
            {
                if (mFields == null)
                {
                    mFields = new AveSPListFieldCollection(this);
                }
                return mFields;
            }
        }

        public AveSPListContentTypeCollection AveContentTypes
        {
            get
            {
                if (mContentTypes == null)
                {
                    mContentTypes = new AveSPListContentTypeCollection(this);
                }
                return mContentTypes;
            }
        }

        private bool? mIsSchedulingOnList = null;

        internal bool IsSchedulingOnList
        {
            get
            {
                if (mIsSchedulingOnList == null && this.ParentSite.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel)
                {
                    try
                    {
                        IAveScheduledItem scheduleItem = this.ParentSite.ObjectModelFactory.CreateScheduledItem();
                        if (SPList != null)
                        {
                            mIsSchedulingOnList = scheduleItem.GetIsSchedulingEventRegisteredOnList(SPList);
                        }
                        else
                        {
                            mIsSchedulingOnList = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Info("An exception occurred while get list scheduling status. exception:{0}", ex.ToString());
                        mIsSchedulingOnList = false;
                    }
                }
                return mIsSchedulingOnList.HasValue ? mIsSchedulingOnList.Value : false;
            }
        }

        internal IAveListItemSerializer ListItemSerializer
        {
            get
            {
                if (mListItemSerializer == null)
                {
                    mListItemSerializer = mAveParentSite.ObjectModelFactory.CreateListItemSerializer(mAveParentSite.SPSite, mAveSPWeb.SPWeb, mSPList);
                }
                return mListItemSerializer;
            }
        }

        public IAveList SPList
        {
            get { return mSPList; }
        }
        private bool hasInitialAveList = false;
        public IAveList AveList
        {
            get
            {
                if (mAveSPWeb.SPWeb == null)
                {
                    mSPList = null;
                }
                else if (mSPList == null && !hasInitialAveList)
                {
                    try
                    {
                        mSPList = mAveSPWeb.SPWeb.GetListByName(mName, true);
                    }
                    catch (Exception e)
                    {
                        log.Debug(string.Format("Get AveList error.Exception:{0}", e.ToString()));
                    }
                    finally
                    {
                        hasInitialAveList = true;
                    }
                }
                return mSPList;
            }
        }
        public AveSPWeb ParentWeb
        {
            get { return mAveSPWeb; }
        }
        public AveListInfo ListInfo
        {
            get { return mListInfo; }
        }

        public AveListSettingInfo ListSettingInfo
        {
            get { return mListSettingInfo; }
            set { mListSettingInfo = value; }
        }

        public bool IsNewCreated
        {
            get { return mIsNewCreated; }
        }
        public IAveBackupRestoreQueryService QueryService
        {
            get { return mQueryService; }
        }

        public IAveFolder RootFolder
        {
            get
            {
                if (IsSystemList)
                {
                    return mAveSPWeb.SPWeb.RootFolder;
                }
                return mSPList != null ? mSPList.RootFolder : null;
            }
        }

        public RestoringDto RestoringFolder
        {
            get { return mRestringFolder; }
        }

        public string RootFolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(mRootFolderPath))
                {
                    try
                    {
                        if (IsSystemList)
                        {
                            mRootFolderPath = mAveSPWeb.SPWeb.ServerRelativeUrl;
                        }
                        else
                        {
                            mRootFolderPath = mSPList.RootFolder.ServerRelativeUrl;
                        }
                        mRootFolderPath = mRootFolderPath.Trim('/');
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while getting root folder path. Name:{0}\n error message:{1}", mName, e));
                        //mLog.Warn(e, "An error occurred while getting root folder path. Name:{0}", mName);
                    }
                }
                return mRootFolderPath;
            }
        }

        public bool IsSystemList
        {
            get { return mId == Guid.Empty && string.Compare(mName, AveConstants.SYSTEM_FOLDER, StringComparison.OrdinalIgnoreCase) == 0; }
        }

        public bool StopAlerts
        {
            get { return mStopAlerts; }
            set { mStopAlerts = value; }
        }
        public bool KeepDefaultValue
        {
            get { return mAveSPWeb.ParentSite.KeepDefaultValue; }
        }

        public bool AutoDeclareRecord
        {
            get { return mAutoDeclareRecord.HasValue ? mAutoDeclareRecord.Value : false; }
        }

        public List<string> NeedUpdateToDefaultView
        {
            get { return mNeedUpdateToDefaultView; }
        }

        public List<string> NeedUpdateToDefaultContentType
        {
            get { return mNeedUpdateToDefaultContentType; }
        }

        public AveSPList(AveSPWeb _AveWeb, string _name)
        {
            mAveSPWeb = _AveWeb;
            mAveParentSite = mAveSPWeb.ParentSite;
            mName = mAveSPWeb.ListMapping.GetMappingTitleBeforeAdd(_name);
            mName = mAveSPWeb.ParentSite.GetNameByLanguageMapping(mName, AveLanguageMappingType.ListMapping);
            mQueryService = mAveSPWeb.QueryService;
            mIsNewCreated = mAveSPWeb.IsNewCreated;
            mFields = new AveSPListFieldCollection(this);
            mContentTypes = new AveSPListContentTypeCollection(this);
            mRestringFolder = new RestoringDto();
        }

        /// <summary>
        /// Add for SPM Function (HSM)
        /// </summary>
        /// <param name="_AveWeb"></param>
        /// <param name="_name"></param>
        /// <param name="info"></param>
        /// <param name="fields"></param>
        public AveSPList(AveSPWeb _AveWeb, string _name, AveListInfo info, AveSPListFieldCollection fields = null)
            : this(_AveWeb, _name)
        {
            this.mListInfo = info;
            if (fields != null)
            {
                this.mFields = fields;
            }
        }

        /// <summary>
        /// only used for wrapper test purpose
        /// </summary>
        /// <param name="aveList"></param>
        /// <param name="restoreStream"></param>
        /// <param name="folderRelativeUrl"></param>
        /// <param name="isRestoreFolder"></param>
        public AveSPList(AveSPWeb web, IAveRestoreStream restoreStream, AveSecurityMapping securityMapping, AveCommonRestoreConfiguraion restoreConfig, string title)
        {
            mAveSPWeb = web;
            mAveParentSite = web.ParentSite;
            mSPList = mAveSPWeb.SPWeb.Lists.GetByTitle(title);
            mAveParentSite = web.ParentSite;
            mRestringFolder = new RestoringDto();
        }

        //public AveSPList(AveSPWeb web, IAveRestoreStream restoreStream)
        //{
        //    mAveParentSite = web.ParentSite;
        //    mAveSPWeb = web;
        //    mRestoreStream = restoreStream;
        //    mAveSPFolder = new AveSPFolder(this, mRestoreStream);
        //}

        //public AveSPList(AveSPWeb web, IAveRestoreStream restoreStream, AveSecurityMapping securityMapping, AveCommonRestoreConfiguraion restoreConfig)
        //    : this(web, restoreStream)
        //{
        //    mSecurityMapping = securityMapping;
        //    mRestoreConfig = restoreConfig;
        //    mAveParentSite = web.ParentSite;
        //}

        //public AveSPList(AveSPWeb web, IAveRestoreStream restoreStream, AveSecurityMapping securityMapping, AveCommonRestoreConfiguraion restoreConfig, string title)
        //    : this(web, restoreStream, securityMapping, restoreConfig)
        //{
        //    mSPList = mAveSPWeb.SPWeb.Lists.GetByTitle(title);
        //    mAveParentSite = web.ParentSite;
        //}
        public void BackupListWorkflowSetting()
        {
            using (new AvePerformanceScope("Restore.AveSPList.BackupListWorkflowSetting"))
            {
                BackupWorkflowStartOption();
            }
        }

        /// <summary>
        /// Decode specail characters in path from media: ('%1' to '%'; '%2' to '\')
        /// </summary>
        public void DecodeNameForSpecialChar()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.DecodeNameForSpecialChar"))
            {

                if (!string.IsNullOrEmpty(mName))
                {
                    mName = AvePoint.GCommon.AveConverter.DecodeSpecialChar(mName);
                    mName = mAveSPWeb.ParentSite.GetNameByLanguageMapping(mName, AveLanguageMappingType.ListMapping);
                }

            }

        }
        internal void AddNeedUpdateSpotlightViews(Guid desViewId, AveViewInfo viewInfo)
        {
            lock (mNeedUpdateSpotlightViews)
            {
                mNeedUpdateSpotlightViews[desViewId] = viewInfo;
            }
        }
        public void RestoreListProperty(AveListSettingInfo listSettingInfo, bool RestoreListOnQuickLaunch = true)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.RestoreListProperty"))
            {

                try
                {
                    mListSettingInfo = listSettingInfo;
                    base.IsSettingRestored = true;
                    if (this.ParentWeb.ParentSite.SPSite.IsOnlineSite && listSettingInfo.ComplianceTag != null && listSettingInfo.ComplianceTag.IsAvailable)
                    {
                        var webID = this.ParentWeb.SPWeb.ID;
                        var listID = this.SPList.ID;
                        if (!this.ParentWeb.ParentSite.UnRestoreListComplianceTagProperties.ContainsKey(webID))
                        {
                            this.ParentWeb.ParentSite.UnRestoreListComplianceTagProperties.Add(webID, new Dictionary<Guid, AveComplianceTagInfo>());
                        }
                        this.ParentWeb.ParentSite.UnRestoreListComplianceTagProperties[webID][listID] = ListSettingInfo.ComplianceTag.Value;
                    }
                    if (listSettingInfo.DefaultView != null && listSettingInfo.DefaultView.IsAvailable)
                    {
                        mOldDefaultViewUrl = listSettingInfo.DefaultView.Value;
                    }
                    if (listSettingInfo.Description != null && listSettingInfo.Description.IsAvailable)
                    {
                        mSPList.Description = listSettingInfo.Description.Value != null ? listSettingInfo.Description.Value : "";
                    }
                    //mSPList.AllowContentTypes = mListInfo.AllowContentTypes;
                    if (listSettingInfo.AllowDeletion != null && listSettingInfo.AllowDeletion.IsAvailable)
                    {
                        mSPList.AllowDeletion = listSettingInfo.AllowDeletion.Value;
                    }
                    //mSPList.AllowRssFeeds = mListInfo.AllowRssFeads;
                    //mSPList.ApplicationList = listSettingInfo.ApplicationList;
                    //mSPList.AutoSaveEnabled = listSettingInfo.AutoSaveEnabled;
                    //mSPList.BaseTemplate = mListInfo.BaseTemplate;
                    //mSPList.BaseType = mListInfo.BaseType;

                    if (listSettingInfo.DefaultItemOpen != null && listSettingInfo.DefaultItemOpen.IsAvailable)
                    {
                        if (listSettingInfo.DefaultItemOpen.Value == 0)
                        {
                            mSPList.DefaultItemOpenUseListSetting = false;
                        }
                        else if (listSettingInfo.DefaultItemOpen.Value == 1)
                        {
                            mSPList.DefaultItemOpen = AveDefaultItemOpen.Browser;
                        }
                        else
                        {
                            mSPList.DefaultItemOpen = AveDefaultItemOpen.PreferClient;
                        }
                    }
                    if (listSettingInfo.ListExperienceOptions != null && listSettingInfo.ListExperienceOptions.IsAvailable)
                    {
                        mSPList.ListExperienceOptions = (AveListExperience)listSettingInfo.ListExperienceOptions.Value;

                    }
                    if (listSettingInfo.EnableAssignToEmail != null && listSettingInfo.EnableAssignToEmail.IsAvailable)
                    {
                        if (this.ParentSite.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel && listSettingInfo.EnableAssignToEmail.Value)
                        {
                            mSPList.EnableAssignToEmail = false;
                            this.ParentSite.MappingManager.SiteMappingManager.AddAssignToEmailSettingmapping(this.ParentWeb.SPWeb.ID, this.mSPList.ID);
                        }
                        else
                        {
                            mSPList.EnableAssignToEmail = listSettingInfo.EnableAssignToEmail.Value;
                        }
                    }
                    if (listSettingInfo.EnableAttachments != null && listSettingInfo.EnableAttachments.IsAvailable
                        && (mSPList.BaseType != AveBaseType.DocumentLibrary) && (mSPList.BaseType != AveBaseType.Survey))
                    {
                        mSPList.EnableAttachments = listSettingInfo.EnableAttachments.Value;
                    }
                    if (listSettingInfo.EnableDeployingList != null && listSettingInfo.EnableDeployingList.IsAvailable)
                    {
                        mSPList.EnableDeployingList = listSettingInfo.EnableDeployingList.Value;  // have not found which should replace it
                    }
                    if (listSettingInfo.EnableDeployWithDependentList != null && listSettingInfo.EnableDeployWithDependentList.IsAvailable)
                    {
                        mSPList.EnableDeployWithDependentList = listSettingInfo.EnableDeployWithDependentList.Value;
                    }
                    if (listSettingInfo.EnableFolderCreation != null && listSettingInfo.EnableFolderCreation.IsAvailable && mSPList.ServerTemplateCanCreateFolders)
                    {
                        mSPList.EnableFolderCreation = listSettingInfo.EnableFolderCreation.Value;
                    }
                    if (listSettingInfo.RequestAccessEnabled != null && listSettingInfo.RequestAccessEnabled.IsAvailable)
                    {
                        mRequestAccessEnabled = listSettingInfo.RequestAccessEnabled.Value;
                    }
                    if (ListSettingInfo.CrawlNonDefaultViews != null && ListSettingInfo.CrawlNonDefaultViews.IsAvailable)
                    {
                        mSPList.CrawlNonDefaultViews = ListSettingInfo.CrawlNonDefaultViews.Value;
                    }

                    if (mSPList.BaseType == AveBaseType.DocumentLibrary)
                    {
                        if (listSettingInfo.EnableMinorVersions != null && listSettingInfo.EnableMinorVersions.IsAvailable)
                        {
                            mSPList.EnableMinorVersions = listSettingInfo.EnableMinorVersions.Value;
                        }
                        if (listSettingInfo.EventSinkAssembly != null && listSettingInfo.EventSinkAssembly.IsAvailable)
                        {
                            mSPList.EventSinkAssembly = listSettingInfo.EventSinkAssembly.Value;
                        }
                    }

                    if (listSettingInfo.EnableModeration != null && listSettingInfo.EnableModeration.IsAvailable)
                    {
                        mSPList.EnableModeration = listSettingInfo.EnableModeration.Value;
                    }
                    if (listSettingInfo.EnablePeopleSelector != null && listSettingInfo.EnablePeopleSelector.IsAvailable)
                    {
                        mSPList.EnablePeopleSelector = listSettingInfo.EnablePeopleSelector.Value;
                    }
                    if (listSettingInfo.EnableResourceSelector != null && listSettingInfo.EnableResourceSelector.IsAvailable)
                    {
                        EnsureListResourceSelector(listSettingInfo);
                    }
                    if (listSettingInfo.EnableSchemaCaching != null && listSettingInfo.EnableSchemaCaching.IsAvailable)
                    {
                        mSPList.EnableSchemaCaching = listSettingInfo.EnableSchemaCaching.Value;
                    }
                    if (listSettingInfo.EnableSyndication != null && listSettingInfo.EnableSyndication.IsAvailable)
                    {
                        mSPList.EnableSyndication = listSettingInfo.EnableSyndication.Value;
                    }
                    if (listSettingInfo.EnableThrottling != null && listSettingInfo.EnableThrottling.IsAvailable && listSettingInfo.EnableThrottling.Value != mSPList.EnableThrottling) // sp api update this value using sql, so check it first will be more efficent
                    {
                        mSPList.EnableThrottling = listSettingInfo.EnableThrottling.Value;
                    }
                    //Some list's enableThrottling will reset content type, so we set content type setting here.
                    if (listSettingInfo.ContentTypesEnabled != null && listSettingInfo.ContentTypesEnabled.IsAvailable && mSPList.AllowContentTypes)
                    {
                        mSPList.ContentTypesEnabled = listSettingInfo.ContentTypesEnabled.Value;
                    }
                    if (listSettingInfo.EnableVersioning != null && listSettingInfo.EnableVersioning.IsAvailable && mSPList.BaseType != AveBaseType.Survey)
                    {
                        mSPList.EnableVersioning = listSettingInfo.EnableVersioning.Value;
                    }
                    if (listSettingInfo.AllowMultiResponses != null && listSettingInfo.AllowMultiResponses.IsAvailable && mSPList.BaseType == AveBaseType.Survey)
                    {
                        mSPList.AllowMultiResponses = listSettingInfo.AllowMultiResponses.Value;
                    }
                    if (listSettingInfo.DocumentTemplateUrl != null && listSettingInfo.DocumentTemplateUrl.IsAvailable && mSPList.BaseType == AveBaseType.DocumentLibrary)
                    {
                        mDocumentTemplateUrl = listSettingInfo.DocumentTemplateUrl.Value;
                    }

                    if (listSettingInfo.EnforceDataValidation != null && listSettingInfo.EnforceDataValidation.IsAvailable)
                    {
                        //mSPList.EnforceDataValidation = listSettingInfo.EnforceDataValidation.Value;
                        //ADO-73805,该属性影响list下item的还原，需要在ListPostAction中更新
                        mEnforceDataValidation = listSettingInfo.EnforceDataValidation.Value;
                    }
                    if (listSettingInfo.ExcludeFromOfflineClient != null && listSettingInfo.ExcludeFromOfflineClient.IsAvailable)
                    {
                        mSPList.ExcludeFromOfflineClient = listSettingInfo.ExcludeFromOfflineClient.Value;
                    }
                    //mSPList.ExcludeFromTemplate = mListInfo.ExcludeFromTemplate;
                    if (listSettingInfo.ShowUser != null && listSettingInfo.ShowUser.IsAvailable)
                    {
                        mSPList.ShowUser = listSettingInfo.ShowUser.Value;
                    }

                    if (listSettingInfo.ForceCheckout != null && listSettingInfo.ForceCheckout.IsAvailable)
                    {
                        if (listSettingInfo.ForceCheckout.Value)
                        {
                            if (!mSPList.HasExternalDataSource && mSPList.BaseType == AveBaseType.DocumentLibrary)
                            {
                                mSPList.ForceCheckout = listSettingInfo.ForceCheckout.Value;
                            }
                        }
                        else
                        {
                            mSPList.ForceCheckout = listSettingInfo.ForceCheckout.Value;
                        }
                    }

                    if (listSettingInfo.ValidationFormula != null && listSettingInfo.ValidationFormula.IsAvailable)
                    {
                        mValidationFormula = listSettingInfo.ValidationFormula.Value;
                    }

                    if (listSettingInfo.ValidationMessage != null && listSettingInfo.ValidationMessage.IsAvailable
                        && listSettingInfo.ValidationMessage.Value != null && listSettingInfo.ValidationMessage.Value.Length <= 0x400L)
                    {
                        mValidationMessage = listSettingInfo.ValidationMessage.Value;
                    }
                    //mSPList.HasUniqueRoleAssignments = mListInfo.HasUniqueRoleAssigntments;
                    if (listSettingInfo.Hidden != null && listSettingInfo.Hidden.IsAvailable)
                    {
                        mSPList.Hidden = listSettingInfo.Hidden.Value;
                    }
                    if (listSettingInfo.IrmEnabled != null && listSettingInfo.IrmEnabled.IsAvailable)
                    {
                        mSPList.IrmEnabled = listSettingInfo.IrmEnabled.Value;
                    }
                    if (listSettingInfo.IrmExpire != null && listSettingInfo.IrmExpire.IsAvailable)
                    {
                        mSPList.IrmExpire = listSettingInfo.IrmExpire.Value;
                    }
                    if (listSettingInfo.IrmReject != null && listSettingInfo.IrmReject.IsAvailable)
                    {
                        mSPList.IrmReject = listSettingInfo.IrmReject.Value;
                    }
                    //mSPList.IsThrottled = mListInfo.IsThrottled;  
                    if (listSettingInfo.OnQuickLaunch != null && listSettingInfo.OnQuickLaunch.IsAvailable && RestoreListOnQuickLaunch)
                    {
                        mSPList.OnQuickLaunch = listSettingInfo.OnQuickLaunch.Value;
                    }
                    if (listSettingInfo.WorkflowsAssociated != null && listSettingInfo.WorkflowsAssociated.IsAvailable && listSettingInfo.WorkflowsAssociated.Value)
                    {
                        mSPList.SetWorkflowsAssociated(listSettingInfo.WorkflowsAssociated.Value);
                    }

                    if (listSettingInfo.NoCrawl != null && listSettingInfo.NoCrawl.IsAvailable)
                    {
                        if (listSettingInfo.NoCrawl.Value)
                        {
                            mSPList.NoCrawl = listSettingInfo.NoCrawl.Value;
                        }
                        else if (!mSPList.HasExternalDataSource)
                        {
                            mSPList.NoCrawl = false;
                        }
                    }

                    if (listSettingInfo.MultipleDataList != null && listSettingInfo.MultipleDataList.IsAvailable
                        && mSPList.ParentWeb.WebTemplate != null && mSPList.ParentWeb.WebTemplate.StartsWith(AveWrapperConstants.WebTemplateMWS, StringComparison.OrdinalIgnoreCase))
                    { //If destination web is not a meeting web, the attribute "MultipleDataList" shouldn't be true.
                        mSPList.MultipleDataList = listSettingInfo.MultipleDataList.Value;

                        //如果新创建的list MultipleDataList为true，设置MultipleDataList = false时，可能会多出folder。
                        if (mIsNewCreated && listSettingInfo.MultipleDataList.Value == false)
                        {
                            try
                            {
                                for (int i = mSPList.RootFolder.SubFolders.Count - 1; i >= 0; i--)
                                {
                                    if (mSPList.RootFolder.SubFolders[i].Item != null)
                                    {
                                        mSPList.RootFolder.SubFolders[i].Delete();
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                reportor.AddDetail(new AveWrapperReportDto(mSPList.Title, mSPList.Title, AveReportObjectType.ListProperty, AveStatus.Failed, AveReportResource.Wrapper_Report_ClearMultipleDataListDefaultFoldersError, e.Message));
                                log.Warn(string.Format("An error occurred while clear MultipleDataList default folders. error:{0}", e.ToString()), e);
                            }
                        }
                    }
                    //mSPList.Ordered = mListInfo.Ordered;
                    //mSPList.TemplateFeatureId = mListInfo.TemplateFeatureId;
                    //mSPList.Title = mListInfo.Title;
                    //mSPList.ServerTemplateCanCreateFolders = mListInfo.ServerTemplateCanCreateFolders;
                    if (listSettingInfo.ReadSecurity != null && listSettingInfo.ReadSecurity.IsAvailable
                        && (listSettingInfo.ReadSecurity.Value == 1 || listSettingInfo.ReadSecurity.Value == 2))
                    {
                        mSPList.ReadSecurity = listSettingInfo.ReadSecurity.Value;
                    }

                    if (listSettingInfo.WriteSecurity != null && listSettingInfo.WriteSecurity.IsAvailable
                        && (listSettingInfo.WriteSecurity.Value == 1 || listSettingInfo.WriteSecurity.Value == 2 || listSettingInfo.WriteSecurity.Value == 4))
                    {
                        mSPList.WriteSecurity = listSettingInfo.WriteSecurity.Value;
                    }

                    if (listSettingInfo.DraftVersionVisibility != null && listSettingInfo.DraftVersionVisibility.IsAvailable)
                    {
                        AveDraftVisibilityType temType = (AveDraftVisibilityType)listSettingInfo.DraftVersionVisibility.Value;
                        if (temType == AveDraftVisibilityType.Approver || temType == AveDraftVisibilityType.Author || temType == AveDraftVisibilityType.Reader)
                        {
                            mSPList.DraftVersionVisibility = (AveDraftVisibilityType)listSettingInfo.DraftVersionVisibility.Value;
                        }
                    }

                    //此属性在界面上无法设置，其影响list 的Flags 的还原，所以注释掉
                    //if (listSettingInfo.ThumbnailSize > 0 && mSPList is IAveDocumentLibrary)
                    //{
                    //    IAveDocumentLibrary spDocLibrary = (IAveDocumentLibrary)mSPList;
                    //    spDocLibrary.ThumbnailsEnabled = true;
                    //    spDocLibrary.ThumbnailSize = listSettingInfo.ThumbnailSize.Value;
                    //}
                    //if (listSettingInfo.EmailAlias != null && listSettingInfo.EmailAlias.IsAvailable && !string.IsNullOrEmpty(listSettingInfo.EmailAlias.Value))
                    //{
                    //    mSPList.EmailAlias = listSettingInfo.EmailAlias.Value;
                    //}
                    if (listSettingInfo.SendToLocation != null && listSettingInfo.SendToLocation.IsAvailable && !string.IsNullOrEmpty(listSettingInfo.SendToLocation.Value))
                    {
                        int temIndex = listSettingInfo.SendToLocation.Value.IndexOf('|');
                        mSPList.SendToLocationName = temIndex > 0 ? listSettingInfo.SendToLocation.Value.Substring(0, temIndex) : listSettingInfo.SendToLocation.Value;
                        mSPList.SendToLocationUrl = temIndex > 0 ? listSettingInfo.SendToLocation.Value.Substring(temIndex + 1) : string.Empty;
                        if (!string.IsNullOrEmpty(mSPList.SendToLocationUrl))
                        {
                            mSPList.SendToLocationUrl = AveReplaceProcessor.UrlReplace(mSPList.SendToLocationUrl, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveSPWeb.ParentSite.SourceSiteInfo, mAveSPWeb.ParentSite.ServerRelativeUrl);
                        }
                    }

                    if (ListSettingInfo.ImageUrl != null && ListSettingInfo.ImageUrl.IsAvailable && !string.IsNullOrEmpty(listSettingInfo.ImageUrl.Value))
                    {
                        mSPList.ImageUrl = AveReplaceProcessor.UrlReplace(ListSettingInfo.ImageUrl.Value, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveSPWeb.ParentSite.SourceSiteInfo, mAveSPWeb.ParentSite.ServerRelativeUrl);
                    }
                    //修改下面两个if的判断条件，当Value.HasValue为false(即源端的值为空)时也进行处理
                    //源端为空，目的端有值的时候无法更新数据，故去掉listSettingInfo.MaxMajorVersionCount.Value > 0的判断
                    if ((mSPList.EnableMinorVersions || mSPList.EnableModeration) && listSettingInfo.MaxMajorwithMinorVersionCount != null && listSettingInfo.MaxMajorwithMinorVersionCount.IsAvailable
                        && (!listSettingInfo.MaxMajorwithMinorVersionCount.Value.HasValue || listSettingInfo.MaxMajorwithMinorVersionCount.Value < 0xc350))
                    {
                        //mSPList.MajorWithMinorVersionsLimit = listSettingInfo.MaxMajorwithMinorVersionCount.Value.HasValue ? listSettingInfo.MaxMajorwithMinorVersionCount.Value.Value : AveAllListsTableColumnValue.MaxMajorwithMinorVersionCount;
                        mMajorWithMinorVersionsLimit = listSettingInfo.MaxMajorwithMinorVersionCount.Value.HasValue ? listSettingInfo.MaxMajorwithMinorVersionCount.Value.Value : AveAllListsTableColumnValue.MaxMajorwithMinorVersionCount;
                        mSPList.MajorWithMinorVersionsLimit = 0;
                    }

                    if (this.ParentWeb.SPWeb.AppInstanceId == Guid.Empty)
                    {
                        if (mSPList.EnableVersioning && listSettingInfo.MaxMajorVersionCount != null && listSettingInfo.MaxMajorVersionCount.IsAvailable
                            && (!listSettingInfo.MaxMajorVersionCount.Value.HasValue || listSettingInfo.MaxMajorVersionCount.Value < 0xc350))
                        {
                            //mSPList.MajorVersionLimit = listSettingInfo.MaxMajorVersionCount.Value.HasValue ? listSettingInfo.MaxMajorVersionCount.Value.Value : AveAllListsTableColumnValue.MaxMajorVersionCount;
                            mMajorVersionLimit = listSettingInfo.MaxMajorVersionCount.Value.HasValue ? listSettingInfo.MaxMajorVersionCount.Value.Value : AveAllListsTableColumnValue.MaxMajorVersionCount; ;
                            mSPList.MajorVersionLimit = 0;
                        }
                    }
                    if (listSettingInfo.AuditFlags != null && listSettingInfo.AuditFlags.IsAvailable && listSettingInfo.AuditFlags.Value > 0)
                    {
                        //TODO: update this flag.
                        //mSPList.Audit.AuditFlags = SPAuditMaskType.All;
                    }
                    if (listSettingInfo.SendToLocationName != null && listSettingInfo.SendToLocationName.IsAvailable && listSettingInfo.SendToLocationName.Value != null)
                    {
                        mSPList.SendToLocationName = listSettingInfo.SendToLocationName.Value;
                    }
                    //if (listSettingInfo.SendToLocationUrl != null && listSettingInfo.SendToLocationUrl.IsAvailable && listSettingInfo.SendToLocationUrl.Value != null)
                    //{
                    //    mSPList.SendToLocationUrl = listSettingInfo.SendToLocationUrl.Value;
                    //}
                    try
                    {
                        if (listSettingInfo.IsSiteAssetsLibrary != null && listSettingInfo.IsSiteAssetsLibrary.IsAvailable)
                        {
                            mSPList.IsSiteAssetsLibrary = listSettingInfo.IsSiteAssetsLibrary.Value;
                        }
                    }
                    catch (AveSecurityTrimingException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, "list property can not restore successfully. ListTitle:{0},Exception:{1}.", mSPList.Title, e.ToString());
                        //mLog.Warn("list property can not restore succussfully. ListTitle:{0},Exception:{1}.", mSPList.Title, e.ToString());
                    }

                    if (listSettingInfo.RssViewField != null && listSettingInfo.RssViewField.IsAvailable)
                    {
                        mRssViewFieldXml = listSettingInfo.RssViewField.Value;
                    }
                    if (listSettingInfo.DisableGridEditing != null && listSettingInfo.DisableGridEditing.IsAvailable)
                    {
                        mSPList.DisableGridEditing = listSettingInfo.DisableGridEditing.Value;
                    }
                    if (listSettingInfo.NavigateForFormsPages != null && listSettingInfo.NavigateForFormsPages.IsAvailable)
                    {
                        mSPList.NavigateForFormsPages = listSettingInfo.NavigateForFormsPages.Value;
                    }
                    if (ListSettingInfo.EnableManagedIndexes != null && ListSettingInfo.EnableManagedIndexes.IsAvailable)
                    {
                        mSPList.EnableManagedIndexes = ListSettingInfo.EnableManagedIndexes.Value;
                    }
                    if (mSPList.HasUniqueRoleAssignments && listSettingInfo.AnonymousPermMask64 != null && listSettingInfo.AnonymousPermMask64.IsAvailable)
                    {
                        if (mSPList.AnonymousPermMask64 != (AveBasePermissions)listSettingInfo.AnonymousPermMask64.Value)
                        {
                            mSPList.AnonymousPermMask64 = (AveBasePermissions)listSettingInfo.AnonymousPermMask64.Value;
                        }
                    }

                    try
                    {
                        //if (listSettingInfo.AllowRatingSetting != null && listSettingInfo.AllowRatingSetting.IsAvailable && mAveParentSite.Publishing != null)//BPOS-D Publishing为null，会抛空引用；
                        //{
                        //    bool allowListRatingSetting = listSettingInfo.AllowRatingSetting.Value;
                        //    Guid averageRatings = AveSPEnv.IsMoss ? mAveParentSite.Publishing.AverageRatings : Guid.Empty;
                        //    Guid ratingsCount = AveSPEnv.IsMoss ? mAveParentSite.Publishing.RatingsCount : Guid.Empty;
                        //    bool destAllow = mSPList.Fields.Contains(averageRatings) && mSPList.Fields.Contains(ratingsCount);
                        //    ProcessListRattingSetting(allowListRatingSetting, destAllow);
                        //}
                        UpdateListRating(listSettingInfo);
                    }
                    catch (AveSecurityTrimingException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        log.Warn("AverageRatings constructor is not supported in BPOS mode.", e);
                    }

                    #region DOC-75090 ADO-15128 Audience,EnterPriseKeyWords,Ratting这三个setting，如果目的端开启，不能随便关闭，否则可能导致目的端数据出现问题
                    // 5.x在manual的时候会保持一致，考虑到manual两端的一致性，这些属性都是跟column相关，也就没有必要单独处理
                    //try
                    //{
                    //    bool destEnableAudience = mSPList.Fields.Contains(new Guid("61cbb965-1e04-4273-b658-eedaa662f48d"));
                    //    if (listSettingInfo.EnableAudienceSetting != null && listSettingInfo.EnableAudienceSetting.IsAvailable && !listSettingInfo.EnableAudienceSetting.Value && destEnableAudience)
                    //    {
                    //        mSPList.Fields.Delete("Target Audiences");
                    //    }
                    //}
                    //catch
                    //{ }
                    //try
                    //{
                    //    bool destEnterPriseKeyWords = mSPList.Fields.Contains(new Guid("23F27201-BEE3-471e-B2E7-B64FD8B7CA38"));
                    //    if (listSettingInfo.EnterPriseKeyWordsEnable != null && listSettingInfo.EnterPriseKeyWordsEnable.IsAvailable && !listSettingInfo.EnterPriseKeyWordsEnable.Value && destEnterPriseKeyWords)
                    //    {
                    //        try
                    //        {
                    //            ProcessEnterPriseKeyWordsSetting();
                    //        }
                    //        catch { }
                    //    }
                    //}
                    //catch
                    //{ }
                    #endregion

                    //if (listSettingInfo.EnableMetaPublish != null && listSettingInfo.EnableMetaPublish.IsAvailable)
                    //{
                    //    bool sourceEnableMetaPublishing = listSettingInfo.EnableMetaPublish.Value;
                    //    if (sourceEnableMetaPublishing == false)
                    //    {
                    //        //delete the eventreceiver if secondary exist
                    //        ProcessListMetaPublishing();
                    //    }
                    //}

                    if (listSettingInfo.ScheduledItemSetting != null && listSettingInfo.ScheduledItemSetting.IsAvailable && AveEnv.IsMoss)
                    {
                        SetScheduledItemSetting(listSettingInfo.ScheduledItemSetting.Value);
                    }
                    SetTitleAndDescriptionResource(mSPList, listSettingInfo);
                    mSPList.Update();
                    if (listSettingInfo.EmailAlias != null && listSettingInfo.EmailAlias.IsAvailable && !string.IsNullOrEmpty(listSettingInfo.EmailAlias.Value))
                    {
                        try
                        {
                            if (!string.Equals(listSettingInfo.EmailAlias.Value, mSPList.EmailAlias, StringComparison.OrdinalIgnoreCase))
                            {
                                mSPList.EmailAlias = listSettingInfo.EmailAlias.Value;
                                mSPList.Update();
                            }
                        }
                        catch (Exception ex)
                        {
                            reportor.AddDetail(new AveWrapperReportDto(mSPList.Title, mSPList.Title, AveReportObjectType.ListProperty, AveStatus.Failed, AveReportResource.Wrapper_Report_ListPropertyRestoreFailed, mSPList.Title, ex.Message));
                            log.Log(AveLogLevel.WARN, "list property can not restore successfully. ListTitle:{0},Exception:{1}.", mSPList.Title, ex.ToString());
                        }
                    }

                    #region restore list setting: Enterprise Metadata and Keywords Settings
                    if (AveEnv.IsMoss && mAveParentSite.ObjectModelFactory.ContextKind != AveContextKind.Server07ObjectModel)
                    {
                        var metaFieldSettings = mAveParentSite.ObjectModelFactory.CreateMetadataListFieldSettings(mSPList);
                        if (listSettingInfo.EnableMetaPublish != null && listSettingInfo.EnableMetaPublish.IsAvailable)
                        {
                            metaFieldSettings.EnableMetadataPromotion = listSettingInfo.EnableMetaPublish.Value;
                        }
                        if (listSettingInfo.EnterPriseKeyWordsEnable != null && listSettingInfo.EnterPriseKeyWordsEnable.IsAvailable)
                        {
                            metaFieldSettings.EnableKeywordsField = listSettingInfo.EnterPriseKeyWordsEnable.Value;
                        }
                        metaFieldSettings.Update();
                    }
                    #endregion

                    UpdateModifyInfo(listSettingInfo);

                    //list的属性中有会影响到column的属性，在还原完属性后需要reload下list，保证column的属性是最新的
                    this.ReloadList();
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, string.Format("Error occurred when Restore list Properties, list title: {0}.\n error message:{1}", mSPList.Title, ex));
                    reportor.AddDetail(new AveWrapperReportDto(listSettingInfo.Title.Value, listSettingInfo.Title.Value, AveReportObjectType.ListProperty, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionToUpdateListSetting, ex.Message));
                }
                catch (Exception ex)
                {
                    reportor.AddDetail(new AveWrapperReportDto(listSettingInfo.Title.Value, listSettingInfo.Title.Value, AveReportObjectType.ListProperty, AveStatus.Failed, AveReportResource.Wrapper_Report_RestoreListPropertyError, mSPList.Title, ex.Message));
                    log.Log(AveLogLevel.WARN, string.Format("Error occurred when Restore list Properties, list title: {0}.\n error message:{1}", mSPList.Title, ex));
                    //mLog.Warn("Error happenned when Restore list Properites, list title: {0}. Reason: {1}", mSPList.Title, ex.ToString());
                }

            }

        }

        private void UpdateModifyInfo(AveListSettingInfo listSettingInfo)
        {
            Dictionary<string, object> listColumns = new Dictionary<string, object>();
            //保持list的last modified time 一致
            if (listSettingInfo.LastModifiedTime != null && listSettingInfo.LastModifiedTime.IsAvailable && listSettingInfo.LastModifiedTime.Value != DateTime.MinValue)
            {
                //相关log会在UpdateListModifyInfo方法里面输出，此处不需要在添加WARN log
                if (mAveParentSite.SPSite.NativeApiPermission == WrapperNativeApiPermission.FullControl)
                {
                    mAveParentSite.AddUnRestoreListLastModifiedTime(mSPList.ID, listSettingInfo.LastModifiedTime.Value);
                }
            }

            //保持list的创建时间一致
            if (listSettingInfo.Created != null && listSettingInfo.Created.IsAvailable && listSettingInfo.Created.Value != DateTime.MinValue)
            {
                //this.SPList.UpdateListCreated(listSettingInfo.Created.Value);
                listColumns.Add("tp_Created", listSettingInfo.Created.Value);
            }

            //保持list的Author 一致

            if (listSettingInfo.Author != null && listSettingInfo.Author.IsAvailable && listSettingInfo.Author.Value > 0)
            {
                int destAuthorId = 0;
                IAvePrincipal principal = mAveSPWeb.ParentSite.SPMembers.FindMember(listSettingInfo.Author.Value.GetValueOrDefault(), true);
                try
                {
                    //ADO-76919 can not get author
                    if (mSPList != null && mSPList.Author != null)
                    {
                        destAuthorId = mSPList.Author.ID;
                    }
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.DEBUG, "Failed to get author Id of list. List title: {0}. Error:{1}", mSPList.Title, ex.ToString());
                }
                if (principal != null && principal.PrincipalType == AvePrincipalType.User && !principal.ID.Equals(destAuthorId))
                {
                    listColumns.Add("tp_Author", principal.ID);
                }
            }
            this.AveList.UpdateListModifyInfo(listColumns);
        }

        private void SetTitleAndDescriptionResource(IAveList list, AveListSettingInfo settingInfo)
        {
            //to do:是否需要还原list title resource
            if (settingInfo.TitleResource != null && settingInfo.TitleResource.IsAvailable)
            {
                list.TitleResource.SetUserResource(list, settingInfo.TitleResource.Value);
            }
            if (settingInfo.DescriptionResource != null && settingInfo.DescriptionResource.IsAvailable)
            {
                list.DescriptionResource.SetUserResource(list, settingInfo.DescriptionResource.Value);
            }
        }

        private void SetScheduledItemSetting(bool isScheduledItemSetting)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.SetScheduledItemSetting"))
            {

                try
                {
                    if (this.SPList != null && this.SPList.DefaultView != null) //If default view is null, below two functions will throw out "Null Exception"
                    {
                        IAveScheduledItem scheduledItem = null;
                        scheduledItem = mAveParentSite.ObjectModelFactory.CreateScheduledItem();
                        if (scheduledItem != null)
                        {
                            if (isScheduledItemSetting)
                            {
                                scheduledItem.RegisterSchedulingEventOnList(this.SPList);
                            }
                            else
                            {
                                scheduledItem.DisableSchedulingOnList(this.SPList);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    reportor.AddDetail(new AveWrapperReportDto("Scheduled Setting", mSPList.Title, AveReportObjectType.ListProperty, AveStatus.Failed, AveReportResource.Wrapper_Report_SetScheduledItemSettingError, ex.Message));
                    log.Info("An error happened while SetScheduledItemSetting. Exception: " + ex.ToString());
                }

            }

        }

        public void AddDefaultViewUrl(string destDefaultUrl)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.AddDefaultViewUrl"))
            {

                if (!string.IsNullOrEmpty(mOldDefaultViewUrl))
                {
                    string s = this.ParentWeb.SPWeb.Url;
                    string s1 = this.SPList.ParentWebUrl;
                    string destUrl = this.ParentWeb.SPWeb.Url.Substring(0, this.ParentWeb.SPWeb.Url.Length - this.SPList.ParentWebUrl.Length) + destDefaultUrl;
                    mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddListDefaultViewMapping(mOldDefaultViewUrl, destUrl);
                }

            }

        }

        /// <summary>
        /// add for process list ratting setting
        /// </summary>
        /// <param name="sourceEnable"></param>
        /// <param name="destEnable"></param>
        public void ProcessListRattingSetting(bool sourceEnable, bool destEnable)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.ProcessListRattingSetting"))
            {

                try
                {
                    if (sourceEnable == destEnable)
                    {
                        return;
                    }
                    else
                    {
                        if (sourceEnable)
                        {
                            IAveFieldCollection fields = mSPList.Fields;
                            IAveFieldCollection availableFields = mSPList.ParentWeb.AvailableFields;
                            Guid averageRatings = (AveEnv.IsSharePoint2010 || AveEnv.IsSharePoint2007) ? mAveParentSite.Publishing.AverageRatings : Guid.Empty;
                            Guid ratingsCount = (AveEnv.IsSharePoint2010 || AveEnv.IsSharePoint2007) ? mAveParentSite.Publishing.RatingsCount : Guid.Empty;
                            if (!fields.Contains(averageRatings))
                            {
                                IAveField field = availableFields[averageRatings];
                                mSPList.Fields.AddFieldAsXml(field.SchemaXmlWithResourceTokens, true, AveAddFieldOptions.AddFieldToDefaultView | AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddToNoContentType);
                            }
                            if (!fields.Contains(ratingsCount) && availableFields.Contains(ratingsCount))
                            {
                                IAveField field2 = availableFields[ratingsCount];
                                mSPList.Fields.AddFieldAsXml(field2.SchemaXmlWithResourceTokens, false, AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddToNoContentType);
                            }
                        }
                        else
                        {
                            //DOC-75090 ADO-15128 Audience,EnterPriseKeyWords,Ratting这三个setting，如果目的端开启，不能随便关闭，否则可能导致目的端数据出现问题
                            //Guid averageRatings = (AveEnv.IsSharePoint2010 || AveEnv.IsSharePoint2007) ? mAveParentSite.Publishing.AverageRatings : Guid.Empty;
                            //Guid ratingsCount = (AveEnv.IsSharePoint2010 || AveEnv.IsSharePoint2007) ? mAveParentSite.Publishing.RatingsCount : Guid.Empty;
                            //IAveField fieldById = GetFieldById(averageRatings, mSPList.Fields);
                            //if (fieldById != null)
                            //{
                            //    mSPList.Fields.Delete(fieldById.InternalName);
                            //}
                            //IAveField field2 = GetFieldById(ratingsCount, mSPList.Fields);
                            //if (field2 != null)
                            //{
                            //    mSPList.Fields.Delete(field2.InternalName);
                            //}
                        }
                    }
                }
                catch (Exception e)
                {
                    reportor.AddDetail(new AveWrapperReportDto("List Rate Setting", mSPList.Title, AveReportObjectType.ListProperty, AveStatus.Failed, AveReportResource.Wrapper_Report_ProcessListRatingSettingError, e.Message));
                    log.Log(AveLogLevel.WARN, string.Format("Process List Rating setting Error.\n error message:{0}", e));
                    //mLog.Warn("Process List Rating setting Error. Error:{0}", e.ToString());
                }

            }

        }

        /// <summary>
        /// 如果源端没有选择该setting的话，目的端也要将其删除
        /// </summary>
        private void ProcessListMetaPublishing()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.ProcessListMetaPublishing"))
            {

                for (int i = this.mSPList.EventReceivers.Count - 1; i >= 0; i--)
                {
                    IAveEventReceiverDefinition definition = this.mSPList.EventReceivers[i];
                    if (((definition.Name == AveListMetaDateSettingInfo.AddedName) && (definition.Type == AveEventReceiverType.ItemAdded)) && (definition.Assembly == AveListMetaDateSettingInfo.AssembleName))
                    {
                        definition.Delete();
                    }
                    else if (((definition.Name == AveListMetaDateSettingInfo.UpdateName) && (definition.Type == AveEventReceiverType.ItemUpdated)) && (definition.Assembly == AveListMetaDateSettingInfo.AssembleName))
                    {
                        definition.Delete();
                    }
                }

            }

        }

        private void ProcessEnterpriseKeyWordsSetting()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.ProcessEnterpriseKeyWordsSetting"))
            {

                string fileInternalName = string.Empty;
                foreach (IAveField field in mSPList.Fields)
                {
                    if (field.ID.Equals(new Guid("23F27201-BEE3-471e-B2E7-B64FD8B7CA38")))
                    {
                        fileInternalName = field.InternalName;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(fileInternalName))
                {
                    mSPList.Fields.Delete(fileInternalName);
                }

            }

        }

        private IAveField GetFieldById(Guid id, IAveFieldCollection fieldColl)
        {
            try
            {
                return fieldColl[id];
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public void RestoreDocumentTemplateUrl()
        {
            if (base.IsSettingRestored)
            {

                using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.RestoreDocumentTemplateUrl"))
                {

                    if (mDocumentTemplateUrl != null && (mSPList is IAveDocumentLibrary))
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(mDocumentTemplateUrl))
                            {
                                IAveDocumentLibrary docLib = mSPList as IAveDocumentLibrary;
                                docLib.DocumentTemplateUrl = string.Empty;
                                docLib.UpdateSPDocumentLibrary();
                                docLib.Update();
                                return;
                            }
                            string templateUrl = string.Empty;
                            mDocumentTemplateUrl = mDocumentTemplateUrl.ToLower(CultureInfo.InvariantCulture);
                            if (mDocumentTemplateUrl.Contains("/forms/"))
                            {
                                IAveDocumentLibrary docLib = mSPList as IAveDocumentLibrary;
                                templateUrl = mDocumentTemplateUrl.Substring(mDocumentTemplateUrl.LastIndexOf("/forms/", StringComparison.OrdinalIgnoreCase));
                                templateUrl = mSPList.RootFolder.ServerRelativeUrl + templateUrl;
                                docLib.DocumentTemplateUrl = templateUrl;
                                docLib.UpdateSPDocumentLibrary();
                                docLib.Update();
                            }
                        }
                        catch (AveSecurityTrimingException ex)
                        {
                            log.Log(AveLogLevel.WARN, WrapperRestoreResource.UpdateDocumentTemplateUrlFailed, mSPList.Title, mDocumentTemplateUrl, ex);
                            //qlluo: Post action do not support report, remove it.
                            //reportor.AddDetail(new AveWrapperReportDto("DocumentTemplateUrl", "DocumentTemplateUrl", AveReportObjectType.DocumentTemplateUrl, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreDocumentTemplateUrl + ex.Message));

                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, WrapperRestoreResource.UpdateDocumentTemplateUrlFailed, mSPList.Title, mDocumentTemplateUrl, e);
                            //mLog.Warn("An error occurred while update DocumentTemplateUrl. list title:{0}, source DocumentTemplateUrl:{1}", mSPList.Title, mDocumentTemplateUrl);
                        }
                    }


                }

            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property")]
        internal void RestoreDocumentsFromDropOffZone(bool enableRoute)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.RestoreDocumentsFromDropOffZone"))
            {
                try
                {
                    if (this.SPList == null || (this.SPList != null && this.SPList.ParentWeb != null && this.SPList.ParentWeb.Site != null && (this.SPList.ParentWeb.Site.APIType == AveAPIType.BPOS_D || this.SPList.ParentWeb.Site.APIType == AveAPIType.BPOS_S)))
                    {
                        return;
                    }
                    if (mAveSPWeb.SPWeb.Features[new Guid("7AD5272A-2694-4349-953E-EA5EF290E97C")] == null)
                    {
                        return;
                    }
                    string url = WrapperRuntime.CurrentContext.ModelFactory.Utility.GetLocalizedString("$Resources:dlccore,DropOffZone_ListFolder;", null, mAveSPWeb.SPWeb.Language);
                    if (!this.SPList.RootFolder.Url.EndsWith(url, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                    IAveWeb web = mAveSPWeb.SPWeb;
                    IAveList list = this.SPList;

                    try
                    {
                        AveEcmDocumentRouting.UpdateDropOffLibContentType(web);
                    }
                    catch (Exception ctEx)
                    {
                        log.Warn("An error occurred while updating drop off library's content type. Error: {0}", ctEx.ToString());
                    }
                    if (!enableRoute)
                    {
                        return;
                    }
                    Dictionary<int, int> itemsInDropOffLibrary;
                    if (mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.GetValueFromItemIdMapping(list.ID, out itemsInDropOffLibrary))
                    {
                        foreach (int itemId in itemsInDropOffLibrary.Values)
                        {
                            try
                            {
                                IAveListItem item = null;
                                int checkId;
                                Guid fileId;
                                if (mAveParentSite.ObjectModelFactory.CreateAveItem(mSPList.ParentWeb, mSPList).IsCheckOutFile(mAveSPWeb.ParentSite.SPSite.ID, this.SPList.ID, itemId, out checkId, out fileId) && checkId != mAveSPWeb.SPWeb.CurrentUser.ID)
                                {
                                    IAveUser user = null;
                                    try
                                    {
                                        user = mAveSPWeb.SPWeb.SiteUsers.GetByID(checkId);
                                        web = mAveSPWeb.ParentSite.GetCheckoutWeb(mAveSPWeb.SPWeb, web.SiteUsers.GetByID(checkId), fileId);
                                        list = web.Lists[this.SPList.ID];
                                        //这个file在之后会被checkin，所以不需要在之后改check out user id
                                        mAveSPWeb.ParentSite.CheckOutFileId = Guid.Empty;
                                        mAveSPWeb.ParentSite.CheckOutUser = -1;
                                    }
                                    catch (Exception e)
                                    {
                                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetCheckOutUserFailed, e.ToString());
                                        //这个file在之后会被checkin，所以不需要在之后改check out user id
                                        mAveParentSite.ObjectModelFactory.CreateAveItem(mSPList.ParentWeb, mSPList).ChangeCheckoutUserID(web.Site.ID, fileId, web.CurrentUser.ID);
                                        //mAveSPWeb.ParentSite.CheckOutFileId = fileId;
                                        //mAveSPWeb.ParentSite.CheckOutUser = checkId;
                                    }
                                }
                                item = list.GetItemById(itemId);
                                string destination;
                                if ((bool)AveEcmDocumentRouting.RouteFileToFinalDestination(item.Web, null, item.Web.CurrentUser, item.File, out destination))
                                {
                                    log.Info("The file {0} is routed to {1}.", item.File.Url, destination.ToString());
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Warn("An error occurred while routing document {0} to destination. Error: {0}", itemId, ex.ToString());
                            }
                        }
                    }
                }
                catch (Exception ee)
                {
                    log.Warn("An error occurred while routing document to destination. Error: {0}", ee.ToString());
                }

            }

        }

        public bool IsConfictWithRecycle(string name, Guid WebId, IAveBackupRestoreQueryService queryService)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.IsConflictWithRecycle"))
            {
                if (!mAveParentSite.ObjectModelFactory.IsSPInstalled)
                {
                    return false;
                }
                if (queryService != null)
                {
                    return queryService.IsConflictWithRecycle(name, mAveParentSite.SPSite.ID, WebId);
                }
                return false;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ACTs is a part of xml")]
        public void RestoreListRootFolder()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.RestoreListRootFolder"))
            {

                var restoredProperties = new List<string>();
                if (mListSettingInfo != null && mListSettingInfo.RootFolderInfo != null && mListSettingInfo.RootFolderInfo.IsAvailable
                    && mListSettingInfo.RootFolderInfo.Value != null)// && mSPList.RootFolder.Properties != null)
                {
                    bool needUpdate = false;
                    if (mListSettingInfo.RootFolderInfo.Value.WelcomePageUrl != null)
                    {
                        mSPList.RootFolder.WelcomePage = mListSettingInfo.RootFolderInfo.Value.WelcomePageUrl;
                        needUpdate = true;
                    }


                    try
                    {
                        if (mListSettingInfo.RootFolderInfo.Value.MetaInfoDic != null)
                        {
                            if (mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.ContainsKey("vti_welcomepage"))
                            {
                                mWelComePage = mListSettingInfo.RootFolderInfo.Value.MetaInfoDic["vti_welcomepage"].ToString();
                                restoredProperties.Add("vti_welcomepage");
                            }
                            if (mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.ContainsKey("AVEFILESHARE"))
                            {
                                mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.Remove("AVEFILESHARE");
                                log.Debug("Remve property AVEFILESHARE successfully.");
                            }
                            //string metaNavSettingString = null;
                            //if ((metaNavSettingString = mListSettingInfo.RootFolderInfo.MetaInfoDic.ContainsKey("client_MOSS_MetadataNavigationSettings") ? mListSettingInfo.RootFolderInfo.MetaInfoDic["client_MOSS_MetadataNavigationSettings"].ToString() : null) != null)
                            //{
                            //    XmlDocument viewSettingXml = new XmlDocument();
                            //    viewSettingXml.LoadXml(metaNavSettingString);
                            //    if (viewSettingXml.GetElementsByTagName("ViewSettings").Count > 0)
                            //    {
                            //        mHiddenViewCache = viewSettingXml.GetElementsByTagName("ViewSettings")[0].OuterXml;
                            //    }

                            //    string destMetaString = mSPList.RootFolder.Properties.ContainsKey("client_MOSS_MetadataNavigationSettings") ? mSPList.RootFolder.Properties["client_MOSS_MetadataNavigationSettings"].ToString() : null;
                            //    mSPList.RootFolder.Properties["client_MOSS_MetadataNavigationSettings"] = MetaNavSetting.GetDestMetaNavSettingString(metaNavSettingString, mSPList.Fields, destMetaString);
                            //}

                            bool ConnectorSettingCannotRestore = false;
                            if (this.ParentSite.SPContextKind == AveContextKind.ClientObjectModel
                                && (this.ParentSite.SPSite.SPVersion.StartsWith("14.", StringComparison.Ordinal)
                                || (mListSettingInfo.EnableSyndication != null
                                && mListSettingInfo.EnableSyndication.IsAvailable == true
                                && !mSPList.EnableSyndication.Equals(mListSettingInfo.EnableSyndication.Value))))
                            {
                                UpdateListRssSetting();
                            }

                            if (mSPList.RootFolder.Properties != null)
                            {
                                needUpdate = true;

                                string[] keyNames = new string[] { "vti_rss_DocumentAsLink", "vti_rss_ChannelTitle", "vti_rss_ChannelDescription",
                                                       "vti_rss_DayLimit", "vti_rss_ItemLimit", "vti_rss_LimitDescriptionLength",
                                                        "vti_rss_DocumentAsEnclosure","vti_rss_ChannelImageUrl"};
                                restoredProperties.AddRange(keyNames);
                                foreach (string key in keyNames)
                                {
                                    if (mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.ContainsKey(key))
                                    {
                                        if ("vti_rss_ChannelImageUrl".Equals(key, StringComparison.OrdinalIgnoreCase))
                                        {
                                            mSPList.RootFolder.Properties[key] = AveReplaceProcessor.UrlReplace(
                                                mListSettingInfo.RootFolderInfo.Value.MetaInfoDic[key].ToString(), ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveSPWeb.ParentSite.SourceSiteInfo, mAveSPWeb.ParentSite.ServerRelativeUrl);
                                        }
                                        else
                                        {
                                            mSPList.RootFolder.Properties[key] = mListSettingInfo.RootFolderInfo.Value.MetaInfoDic[key];
                                        }
                                    }
                                    else
                                    {
                                        if (mSPList.RootFolder.Properties.ContainsKey(key))
                                        {
                                            mSPList.RootFolder.Properties.Remove(key);
                                        }
                                    }
                                }
                                #region Add for FNS list
                                const string FSNSettings = "FSNSettings";
                                restoredProperties.Add(FSNSettings);
                                if (mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.ContainsKey(FSNSettings))
                                {
                                    string fsnValue = mListSettingInfo.RootFolderInfo.Value.MetaInfoDic[FSNSettings].ToString().Replace(@"\\", @"\").Replace("\\r\\n", "");
                                    mSPList.RootFolder.Properties[FSNSettings] = fsnValue;
                                }
                                #endregion

                                #region --add for connector list root folder properties, if dest is connector library do not restore, if the lib is new created, restore it
                                string[] connectorKeyNames = new string[] { "ConnectorStorageSetting", "ConnectorRoleSetting", "ConnectorFolderStubID" };
                                restoredProperties.AddRange(connectorKeyNames);
                                //if (mSPList.TemplateFeatureId == new Guid(AveWrapperConstants.AVEFSDLFEATRUEID) || mSPList.TemplateFeatureId == new Guid(AveWrapperConstants.AVEVDLFEATRUEID))
                                //{
                                //    isConnectorLibrary = true;
                                if (connectorKeyNames.Any(key => mSPList.RootFolder.Properties.ContainsKey(key)))
                                {
                                    ConnectorSettingCannotRestore = true;
                                }
                                //}

                                if (MoveConnectorSetting && !ConnectorSettingCannotRestore)
                                {
                                    foreach (string key in connectorKeyNames)
                                    {
                                        if (mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.ContainsKey(key))
                                        {
                                            //ADO-124475,ADO-120506:修正了media、content等类型Library的UserName等有'\'的属性在备份还原过后，因转义问题造成的备份错误。          
                                            if (key.Equals("ConnectorStorageSetting", StringComparison.OrdinalIgnoreCase))
                                            {
                                                String tempString = mListSettingInfo.RootFolderInfo.Value.MetaInfoDic[key].ToString();
                                                tempString = tempString.Replace(@"\\", @"\");
                                                mListSettingInfo.RootFolderInfo.Value.MetaInfoDic[key] = tempString;
                                            }
                                            mSPList.RootFolder.Properties[key] = mListSettingInfo.RootFolderInfo.Value.MetaInfoDic[key];
                                        }
                                    }
                                }
                                #endregion
                                //还原label的Publishing site template
                                var specialProperties = new string[] { "SourceVarRootWebTemplatePropertyName", "ecm_AllowManualDeclaration","ecm_IPRListUseListSpecific","ecm_ListReadyForIPR"
                        ,"ecm_ListFieldsReadyForIPR","ecm_AutoDeclareRecords","EnableAutoSpawnPropertyName","AutoSpawnStopAfterDeletePropertyName","UpdateWebPartsPropertyName"
                        ,"CopyResourcesPropertyName", "SendNotificationEmailPropertyName", "vti_listname","vti_listtitle","TranslateFields","UseListPolicy"
                        ,"SystemUsesListPolicy","dlc_listHasExpirationPolicy","client_LocationBasedMetadataDefaults_file"};
                                restoredProperties.AddRange(specialProperties);
                                foreach (var property in specialProperties)
                                {
                                    if (mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.ContainsKey(property))
                                    {
                                        if (string.Equals(property, "client_LocationBasedMetadataDefaults_file", StringComparison.OrdinalIgnoreCase))
                                        {
                                            string fileUrl = mListSettingInfo.RootFolderInfo.Value.MetaInfoDic[property].ToString();
                                            mSPList.RootFolder.Properties[property] = AveReplaceProcessor.UrlReplace(fileUrl, ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveSPWeb.ParentSite.SourceSiteInfo, mAveSPWeb.ParentSite.ServerRelativeUrl);
                                            continue;
                                        }
                                        mSPList.RootFolder.Properties[property] = mListSettingInfo.RootFolderInfo.Value.MetaInfoDic[property];
                                    }
                                }
                                //ecm_AutoDeclareRecords必须先设置为false，不然对还原item有影响，在list postAcion中还原。
                                if (mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.ContainsKey("ecm_AutoDeclareRecords"))
                                {
                                    mAutoDeclareRecord = Boolean.Parse(mListSettingInfo.RootFolderInfo.Value.MetaInfoDic["ecm_AutoDeclareRecords"].ToString());
                                    mSPList.RootFolder.Properties["ecm_AutoDeclareRecords"] = "False";
                                    restoredProperties.Add("ecm_AutoDeclareRecords");
                                }
                                if (mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.ContainsKey("ContentTypes_Mapping"))
                                {
                                    if (mSPList.RootFolder.Properties["ContentTypes_Mapping"] == null)
                                    {
                                        mSPList.RootFolder.Properties["ContentTypes_Mapping"] = mListSettingInfo.RootFolderInfo.Value.MetaInfoDic["ContentTypes_Mapping"];
                                        restoredProperties.Add("ContentTypes_Mapping");
                                    }
                                }
                                //Add to support List settings->Information Rights Management settings
                                if (mSPList.IrmEnabled)
                                {
                                    string[] keyNamesIRM = new string[] { "vti_irm_IrmOffline", "vti_irm_IrmVBA", "vti_irm_IrmDescription",
                                                       "vti_irm_IrmExpireDate", "vti_irm_IrmOfflineDays", "vti_irm_IrmTitle",
                                                        "vti_irm_IrmPrint" };
                                    restoredProperties.AddRange(keyNamesIRM);
                                    foreach (string key in keyNamesIRM)
                                    {
                                        if (mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.ContainsKey(key))
                                        {
                                            mSPList.RootFolder.Properties[key] = mListSettingInfo.RootFolderInfo.Value.MetaInfoDic[key];
                                        }
                                        else
                                        {
                                            if (mSPList.RootFolder.Properties.ContainsKey(key))
                                            {
                                                mSPList.RootFolder.Properties.Remove(key);
                                            }
                                        }
                                    }
                                }


                                foreach (string key in mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.Keys)
                                {
                                    if (String.IsNullOrEmpty(key))
                                        continue;
                                    if (key.StartsWith("LastModifiedACTs_", StringComparison.Ordinal) || key.StartsWith("LastModifiedSFs_", StringComparison.Ordinal))
                                    {
                                        mSPList.RootFolder.Properties[key] = mListSettingInfo.RootFolderInfo.Value.MetaInfoDic[key];
                                        restoredProperties.Add(key);
                                    }
                                }

                                restoredProperties.Sort(StringComparer.Ordinal);
                                foreach (string pro in mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.Keys)
                                {
                                    //ADO-206533 针对特殊数据过滤
                                    if (string.Equals("PowerAppFormProperties", pro, StringComparison.Ordinal))
                                    {
                                        continue;
                                    }
                                    if (restoredProperties.BinarySearch(pro, StringComparer.Ordinal) < 0)
                                    {
                                        DateTime listDateTime = new DateTime();
                                        if (DateTime.TryParse(mListSettingInfo.RootFolderInfo.Value.MetaInfoDic[pro].ToString(), out listDateTime))
                                        {
                                            //DateTime.TryParese()有时会把时间格式转为Local，所以需要调用ToUTC方法
                                            mSPList.RootFolder.Properties[pro] = listDateTime.ToUniversalTime();
                                        }
                                        else
                                        {
                                            mSPList.RootFolder.Properties[pro] = mListSettingInfo.RootFolderInfo.Value.MetaInfoDic[pro];
                                        }
                                    }
                                }
                            }
                        }
                        if (needUpdate)
                        {
                            mSPList.RootFolder.Update();
                        }
                        //if (isConnectorLibrary && !ConnectorSettingCannotRestore)
                        //{
                        //    Assembly ass = Assembly.LoadFile(Path.Combine(AveEnv.AgentBinFolder, "DocAve.SP2010.Connector.DataCenter.dll"));
                        //    Invoker.AddTypeSearchAssembly(ass);

                        //    IContentNativeRestore mNativeRestore = Invoker.CreateNewInstance("AvePoint.FileShare.ConnectorItemRestoreWorker") as IContentNativeRestore;

                        //    if (mNativeRestore != null)
                        //    {
                        //        Invoker.CallMethod(mNativeRestore, "RewriteCfgFile", new object[1] { mSPList });
                        //    }
                        //}
                    }
                    catch (AveSecurityTrimingException ex)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while restoring list root Folder. ListId:{0}, ListTitle:{1}\n error message:{2}", mSPList.ID, mSPList.Title, ex));
                        reportor.AddDetail(new AveWrapperReportDto(mSPList.Title, mSPList.Title, AveReportObjectType.ListProperty, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionToUpdateListRootFolder, ex.Message));
                    }
                    catch (Exception e)
                    {
                        reportor.AddDetail(new AveWrapperReportDto(mSPList.Title, mSPList.Title, AveReportObjectType.ListProperty, AveStatus.Failed, AveReportResource.Wrapper_Report_RestoreListRootFolderError, mSPList.ID, mSPList.Title, e.Message));
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while restoring list root Folder. ListId:{0}, ListTitle:{1}\n error message:{2}", mSPList.ID, mSPList.Title, e));
                        //mLog.Warn(e, "An error occurred while restoring list root Folder. ListId:{0}, ListTitle:{1}", mSPList.ID, mSPList.Title);
                    }
                }

            }

        }
        private void UpdateListRssSetting()
        {
            Dictionary<string, object> RssSettings = new Dictionary<string, object>();
            if (mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.ContainsKey("vti_rss_DocumentAsEnclosure") && mListSettingInfo.RootFolderInfo.Value.MetaInfoDic["vti_rss_DocumentAsEnclosure"] != null)
            {
                RssSettings["DocumentAsEnclosure"] = int.Parse(mListSettingInfo.RootFolderInfo.Value.MetaInfoDic["vti_rss_DocumentAsEnclosure"].ToString()) == 1;
            }
            if (mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.ContainsKey("vti_rss_DocumentAsLink") && mListSettingInfo.RootFolderInfo.Value.MetaInfoDic["vti_rss_DocumentAsLink"] != null)
            {
                RssSettings["DocumentAsLink"] = int.Parse(mListSettingInfo.RootFolderInfo.Value.MetaInfoDic["vti_rss_DocumentAsLink"].ToString()) == 1;
            }
            if (mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.ContainsKey("vti_rss_ItemLimit"))
            {
                RssSettings["ItemLimit"] = mListSettingInfo.RootFolderInfo.Value.MetaInfoDic["vti_rss_ItemLimit"];
            }
            if (mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.ContainsKey("vti_rss_DayLimit"))
            {
                RssSettings["DayLimit"] = mListSettingInfo.RootFolderInfo.Value.MetaInfoDic["vti_rss_DayLimit"];
            }
            if (mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.ContainsKey("vti_rss_LimitDescriptionLength") && mListSettingInfo.RootFolderInfo.Value.MetaInfoDic["vti_rss_LimitDescriptionLength"] != null)
            {
                RssSettings["LimitDescriptionLength"] = int.Parse(mListSettingInfo.RootFolderInfo.Value.MetaInfoDic["vti_rss_LimitDescriptionLength"].ToString()) == 1;
            }
            if (mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.ContainsKey("vti_rss_ChannelTitle"))
            {
                RssSettings["ChannelTitle"] = mListSettingInfo.RootFolderInfo.Value.MetaInfoDic["vti_rss_ChannelTitle"];
            }
            if (mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.ContainsKey("vti_rss_ChannelDescription"))
            {
                RssSettings["ChannelDescription"] = mListSettingInfo.RootFolderInfo.Value.MetaInfoDic["vti_rss_ChannelDescription"];
            }
            if (mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.ContainsKey("vti_rss_ChannelImageUrl"))
            {
                RssSettings["ChannelImageUrl"] = AveReplaceProcessor.UrlReplace(
                            mListSettingInfo.RootFolderInfo.Value.MetaInfoDic["vti_rss_ChannelImageUrl"].ToString(), ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveSPWeb.ParentSite.SourceSiteInfo, mAveSPWeb.ParentSite.ServerRelativeUrl);
            }
            RssSettings["AllowRss"] = mListSettingInfo.EnableSyndication == null ? false : mListSettingInfo.EnableSyndication.Value;
            if (RssSettings.Count > 0)
            {
                //RssSettings["AllowRss"] = mListSettingInfo.AllowRssFeads.Value;
                //RssSettings["ChannelTitle"] = ParentWeb.SPWeb.Title + ":" + mSPList.Title;
                //RssSettings["ChannelDescription"] = "RSS feed for the " + mSPList.Title + " list.";
                //RssSettings["ChannelImageUrl"] = mSPList.ParentWebUrl + "_layouts/images/siteIcon.png";
                mSPList.UpdateListRssSetting(RssSettings);
            }
        }
        public void ReloadList()
        {
            ReloadList(mId);
        }

        private void ReloadList(Guid listid)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.ReloadList"))
            {

                mSPList.Reload();
                try
                {
                    this.mListItemSerializer = null;
                    if (!this.ParentWeb.SPWeb.AllowUnsafeUpdates)
                    {
                        this.ParentWeb.SPWeb.AllowUnsafeUpdates = true;
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.ReloadListError, e.ToString());
                }

            }

        }

        private IAveListTemplate GetSPListTemplateByFeatureId(IAveWeb web, Guid featureId, int type)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.GetSPListTemplateByFeatureId"))
            {
                IAveListTemplate listTemplate = null;

                try
                {
                    bool isBuildInTemplate = Enum.IsDefined(typeof(AveListTemplateType), type);

                    foreach (IAveListTemplate t in web.ListTemplates)
                    {
                        if (t.Type_Client == type)
                        {
                            if (!isBuildInTemplate || (t.FeatureId == featureId))
                            {
                                listTemplate = t;
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while finding list template id. WebUrl:{0}, ListFeatureId:{1}\n error message:{2}", web.Url, featureId, e));
                    //mLog.Warn(e, "An error occurred while finding list template id. WebUrl:{0}, ListFeatureId:{1}", web.Url, featureId);
                }
                return listTemplate;
            }
        }

        private IAveListTemplate GetSPListTemplateByType(IAveWeb web, int type)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.GetSPListTemplateByFeatureId"))
            {
                IAveListTemplate listTemplate = null;
                try
                {
                    foreach (IAveListTemplate t in web.ListTemplates)
                    {
                        if (t.Type_Client == type)
                        {
                            listTemplate = t;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while finding list template id. WebUrl:{0}, ListFeatureId:{1}\n error message:{2}", web.Url, Guid.Empty, e));
                    //mLog.Warn(e, "An error occurred while finding list template id. WebUrl:{0}, ListFeatureId:{1}", web.Url, featureId);
                }
                return listTemplate;
            }
        }

        /// <summary>
        /// 获取custom list template
        /// </summary>
        /// <param name="web"></param>
        /// <param name="featureId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private IAveListTemplate GetSPListTemplateByCustomListTemplateName(IAveWeb web, string customListTemplateName)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.GetSPListTemplateByCustomListTemplateName"))
            {

                IAveListTemplate listTemplate = null;
                try
                {
                    IAveListTemplateCollection tc = web.Site.GetCustomListTemplates(web);
                    foreach (IAveListTemplate t in tc)
                    {
                        if (t.InternalName.LastIndexOf('.') >= 0 && customListTemplateName.Equals(t.InternalName.Substring(0, t.InternalName.LastIndexOf('.')), StringComparison.OrdinalIgnoreCase))
                        {
                            listTemplate = t;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while finding custom list template. WebUrl:{0}, ListTemplateName:{1}\n error message:{2}", web.Url, customListTemplateName, e));
                    //mLog.Warn(e, "An error occurred while finding list template id. WebUrl:{0}, ListFeatureId:{1}", web.Url, featureId);
                }
                return listTemplate;
            }

        }

        private int CreateList(string title)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.CreateList"))
            {

                Guid featureId = mListInfo.TemplateFeatureId;
                string description = mListInfo.Description;
                int listTemplate = mListInfo.BaseTemplate;
                int listBaseType = mListInfo.BaseType;
                Guid id = Guid.Empty;
                int dstListTemplate = listTemplate;
                //获取custom template mapping
                TemplateKeyInfo templateInfo = new TemplateKeyInfo(TemplateMappingLevel.List, "", listTemplate.ToString());
                string mappingTemplate = ParentSite.TemplateMapping.GetMappingTemplateBeforeAdd(templateInfo);

                //list template mapping 是否成功
                bool listTemplateMappingSuccessfully = false;
                //是否是CustomTemplate的mapping
                bool useCustomTemplate = false;
                #region ADO-195833 优先获取Custom template
                IAveListTemplate template = null;
                if (!mappingTemplate.Equals(((AveListTemplateType)listTemplate).ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    template = GetSPListTemplateByCustomListTemplateName(mAveSPWeb.SPWeb, mappingTemplate);
                    if (template == null && int.TryParse(mappingTemplate, out dstListTemplate))
                    {
                        // to do set template
                        listTemplate = dstListTemplate;
                        listTemplateMappingSuccessfully = true;
                    }
                    else
                    {
                        useCustomTemplate = true;
                    }
                }
                #endregion

                string url = mListInfo.ServerRelativeUrl;
                url = AveReplaceProcessor.UrlReplace(url, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.WebUrlMapping, new ReplaceOption(true), mAveSPWeb.ParentSite.SourceSiteInfo, mAveSPWeb.ParentSite.ServerRelativeUrl);
                if (url.StartsWith(mAveSPWeb.SPWeb.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                {
                    url = url.Substring(mAveSPWeb.SPWeb.ServerRelativeUrl.Length);
                }
                url = url.TrimStart('/');
                if (mListInfo.BaseTemplate == (int)AveListTemplateType.ExternalList)
                {
                    try
                    {
                        id = mAveSPWeb.SPWeb.Lists.Add(title, description, url, mListInfo.DataSourceXml);
                    }
                    catch (Exception ex)//同SiteCollection下出现异常 ADO-19168
                    {
                        string accessExceptionMessage = "Access denied. You do not have permission to perform this action or access this resource.";
                        string remoteErrorMessage = "The remote server returned an error: (401) Unauthorized.";
                        if (ex.Message.Contains(accessExceptionMessage) || ex.Message.Contains(remoteErrorMessage)
                            || (ex.InnerException != null && (ex.InnerException.Message.Contains(accessExceptionMessage) || ex.InnerException.Message.Contains(remoteErrorMessage))))
                        {
                            throw new AveSecurityTrimingException(ex.Message, ex);
                        }
                        log.Warn("Create list with error:  " + "  " + url + "   " + title + "  " + ex.ToString());
                        int index = url.IndexOf("/", StringComparison.OrdinalIgnoreCase);
                        url = url.Substring(0, index + 1) + title;
                        for (int i = 1; i < 1000; i++)
                        {
                            try
                            {
                                id = mAveSPWeb.SPWeb.Lists.Add(title, description, url, mListInfo.DataSourceXml);
                                break;
                            }
                            catch (Exception e)//目的端根据title拼出的Url被占用，参照SharePoint，在Title后面不断加1
                            {
                                if (e.Message.Contains(accessExceptionMessage) || e.Message.Contains(remoteErrorMessage)
                                    || (ex.InnerException != null && (ex.InnerException.Message.Contains(accessExceptionMessage) || ex.InnerException.Message.Contains(remoteErrorMessage))))
                                {
                                    throw new AveSecurityTrimingException(e.Message, e);
                                }
                                log.Warn("Create list with error:  " + "  " + url + "   " + title + "  " + e.ToString());
                                url = url + i.ToString();
                                if (i == 999)
                                {
                                    throw;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (!useCustomTemplate && featureId != Guid.Empty)
                    {
                        mAveSPWeb.ReloadWeb();
                        template = listTemplateMappingSuccessfully ? GetSPListTemplateByType(mAveSPWeb.SPWeb, listTemplate) : GetSPListTemplateByFeatureId(mAveSPWeb.SPWeb, featureId, listTemplate);
                        if (this.mRestoreOption.mAveListRestoreOption.VerifyListTemplateFeature)
                        {
                            IAveFeatureDefinition definition = null;
                            try
                            {
                                if (mAveSPWeb.ParentSite.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel &&
                                    mAveSPWeb.SPWeb.Site.WebApplication != null && mAveSPWeb.SPWeb.Site.WebApplication.Farm != null &&
                                    mAveSPWeb.SPWeb.Site.WebApplication.Farm.FeatureDefinitions != null)
                                {
                                    //definition = mAveSPWeb.SPWeb.Site.WebApplication.Farm.FeatureDefinitions[featureId];
                                    definition = GetFeatureDefination(featureId);
                                }

                                if (definition != null)
                                {
                                    log.Debug("The scope of feature:{0} with id:{1} is :{2}", definition.DisplayName, definition.ID, definition.Scope);
                                    if (definition.Scope == AveFeatureScope.Site)
                                    {
                                        if (mAveSPWeb.SPWeb.Site.Features[featureId] == null)
                                        {
                                            //#region 加入Feature多重依赖的处理。
                                            //AddDependenceFeature(definition);
                                            //#endregion
                                            mAveSPWeb.SPWeb.Site.Features.Add(featureId, false);
                                            mAveSPWeb.SPWeb.Site.Update();
                                        }
                                    }
                                    else
                                    {
                                        if (mAveSPWeb.SPWeb.Features[featureId] == null)
                                        {
                                            //#region 加入Feature多重依赖的处理。
                                            //AddDependenceFeature(definition);
                                            //#endregion

                                            // 加入Publishing Feature
                                            if (featureId.Equals(new Guid("22A9EF51-737B-4ff2-9346-694633FE4416")))
                                            {
                                                if (mAveSPWeb.SPWeb.Site.Features[AveSP2010FeatureDefinitions.PublishingSite] == null)
                                                {
                                                    mAveSPWeb.SPWeb.Site.Features.Add(AveSP2010FeatureDefinitions.PublishingSite, true);
                                                    // 在加入了site的feature之后，需要重新load一下，保证缓存中的数据和真实数据一致。
                                                    mAveSPWeb.SPWeb.Site.ReloadSite();
                                                    mAveSPWeb.SPWeb.ReloadWeb();
                                                }

                                                // Publishing feature(22a9ef51-737b-4ff2-9346-694633fe4416)是一个hidden feature。
                                                // 该feature是在开启WebPublishing feature的时候自动开启的。
                                                // 所以这里不开启Publishing feature，直接开启WebPublishing featur(94C94CA6-B32F-4da9-A9E3-1F3D343D7ECB)即可。
                                                if (mAveSPWeb.SPWeb.Features[AveSP2010FeatureDefinitions.PublishingWeb] == null)
                                                {
                                                    mAveSPWeb.SPWeb.Features.Add(AveSP2010FeatureDefinitions.PublishingWeb, true);
                                                }
                                            }
                                            else
                                            {
                                                mAveSPWeb.SPWeb.Features.Add(featureId, true);
                                            }
                                        }
                                    }
                                }
                                else if (mAveSPWeb.ParentSite.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel)
                                {
                                    if (mAveSPWeb.SPWeb.Features[featureId] == null)
                                    {
                                        mAveSPWeb.SPWeb.Features.Add(featureId, true);
                                        mAveSPWeb.SPWeb.Update();
                                    }
                                }
                                else if (mAveSPWeb.ParentSite.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
                                {
                                    bool needReloadWeb = false;
                                    try
                                    {
                                        if (featureId.Equals(new Guid("22A9EF51-737B-4ff2-9346-694633FE4416")))
                                        {
                                            if (mAveSPWeb.SPWeb.Site.Features[AveSP2010FeatureDefinitions.PublishingSite] == null)
                                            {
                                                mAveSPWeb.SPWeb.Site.Features.Add(AveSP2010FeatureDefinitions.PublishingSite, true);
                                                // 在加入了site的feature之后，需要重新load一下，保证缓存中的数据和真实数据一致。
                                                mAveSPWeb.SPWeb.Site.ReloadSite();
                                                needReloadWeb = true;
                                            }

                                            // Publishing feature(22a9ef51-737b-4ff2-9346-694633fe4416)是一个hidden feature。
                                            // 该feature是在开启WebPublishing feature的时候自动开启的。
                                            // 所以这里不开启Publishing feature，直接开启WebPublishing featur(94C94CA6-B32F-4da9-A9E3-1F3D343D7ECB)即可。
                                            if (mAveSPWeb.SPWeb.Features[AveSP2010FeatureDefinitions.PublishingWeb] == null)
                                            {
                                                mAveSPWeb.SPWeb.Features.Add(AveSP2010FeatureDefinitions.PublishingWeb, true);
                                                needReloadWeb = true;
                                            }
                                        }
                                        else
                                        {
                                            if (mAveSPWeb.SPWeb.Features[featureId] == null)
                                            {
                                                mAveSPWeb.SPWeb.Features.Add(featureId, true);
                                                needReloadWeb = true;
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        log.Warn("An error occurred while creating list by active feature.FeatureId:{0},Error:{1}", featureId, e);
                                    }
                                    if (needReloadWeb)
                                    {
                                        mAveSPWeb.SPWeb.ReloadWeb();
                                    }
                                }
                                //开启feature之后，有的feature会创建list后面就不用再创建了。
                                if (!CheckListTemplateNeedCreated())
                                {
                                    return 0;
                                }
                            }
                            catch (Exception ex)
                            {
                                string message = string.Empty;
                                if (definition == null)
                                {
                                    message = string.Format("Activate feature by id:{0} failed:{1}", mListInfo.TemplateFeatureId, ex.ToString());
                                }
                                else
                                {
                                    message = string.Format("Activate feature:{0} with id:{1} and scope:{2} failed:{3}", definition.DisplayName, mListInfo.TemplateFeatureId, definition.Scope, ex.ToString());
                                }
                                log.Warn(message);
                                throw;//激活相关feature 失败，不还原list。
                            }
                            try
                            {
                                mSPList = mAveSPWeb.SPWeb.GetListByName(mName, true);
                                id = mSPList.ID;
                            }
                            catch (Exception ex)
                            {   //开启相关feature后仍未产生需要的list，继续后面的create list逻辑。
                                log.Debug("Create List:{0} by activate feature:{1} failed:{2}", mName, mListInfo.TemplateFeatureId, ex);
                            }
                        }
                    }
                    if (mSPList == null)
                    {
                        if (template != null)
                        {
                            try
                            {
                                string associatedFeatureId = template.FeatureId.ToString();
                                //由于存在mapping所以不能使用 listTemplate 要使用template.Type_Client
                                id = mAveSPWeb.SPWeb.Lists.Add(title, description, url, associatedFeatureId, template.Type_Client, null, AveQuickLaunchOptions.Off);
                            }
                            catch (Exception e)
                            {
                                string accessExceptionMessage = "Access denied. You do not have permission to perform this action or access this resource.";
                                string remoteErrorMessage = "The remote server returned an error: (401) Unauthorized.";
                                if (e.Message.Contains(accessExceptionMessage) || e.Message.Contains(remoteErrorMessage)
                                    || (e.InnerException != null && (e.InnerException.Message.Contains(accessExceptionMessage) || e.InnerException.Message.Contains(remoteErrorMessage))))
                                {
                                    throw new AveSecurityTrimingException(e.Message, e);
                                }
                                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.CreateListError, e.ToString());
                                id = mAveSPWeb.SPWeb.Lists.Add(title, description, template);
                            }
                        }
                        else
                        {
                            try
                            {
                                AveListTemplateType refListTemplate = (AveListTemplateType)listTemplate;
                                object associatedFeatureId = mAveParentSite.ObjectModelFactory.CreateLegacyListTemplate().LookupAssociatedFeatureId(ref refListTemplate);
                                if (associatedFeatureId != null)
                                {
                                    id = mAveSPWeb.SPWeb.Lists.Add(title, description, url, associatedFeatureId.ToString(), (int)listTemplate, null, AveQuickLaunchOptions.Off);
                                }
                                else
                                {
                                    //This is only to restore my task of SP2013 for now.
                                    //This method is based on the implement of CreateListCore method in Microsoft.Office.Server.WorkManagement.DataModel
                                    if (!string.IsNullOrEmpty(ListInfo.ListSchema))
                                    {
                                        XmlDocument xDoc = new XmlDocument();
                                        xDoc.LoadXml(ListInfo.ListSchema);
                                        XmlNode node = xDoc.SelectSingleNode("/List/Fields");
                                        StringBuilder schema = new StringBuilder();
                                        schema.Append(string.Format(CultureInfo.InvariantCulture, "<List BaseType=\"Text\"><MetaData><Fields>{0}</Fields></MetaData></List>", node.InnerXml));
                                        id = mAveSPWeb.SPWeb.Lists.Add(title, description, url, featureId.ToString(), listTemplate, listTemplate.ToString(), schema.ToString(), AveQuickLaunchOptions.Off);
                                    }
                                    else
                                    {
                                        id = mAveSPWeb.SPWeb.Lists.Add(title, description, url, featureId.ToString(), (int)listTemplate, null, AveQuickLaunchOptions.Off);
                                    }
                                }
                                if (listTemplate == 160)
                                {
                                    mAveSPWeb.RestoreAccessRequestProperties(id, mListInfo.ServerRelativeUrl);
                                    UpdateAccessRequestsListDefaultSetting(mAveSPWeb.SPWeb.Lists[id]);
                                }
                            }
                            catch (Exception e)
                            {
                                string accessExceptionMessage = "Access denied. You do not have permission to perform this action or access this resource.";
                                string remoteErrorMessage = "The remote server returned an error: (401) Unauthorized.";
                                if (e.Message.Contains(accessExceptionMessage) || e.Message.Contains(remoteErrorMessage)
                                    || (e.InnerException != null && (e.InnerException.Message.Contains(accessExceptionMessage) || e.InnerException.Message.Contains(remoteErrorMessage))))
                                {
                                    throw new AveSecurityTrimingException(e.Message, e);
                                }
                                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.CreateListError, e.ToString());
                                id = mAveSPWeb.SPWeb.Lists.Add(title, description, (AveListTemplateType)listTemplate);
                            }
                        }
                    }
                }
                mIsNewCreated = true;
                mSPList = mAveSPWeb.SPWeb.Lists[id];
                mId = id;
                return 0;

            }

        }

        /// <summary>
        /// ADO-160937 AcceRequests比较特殊，默认是hidden的，而我们正常创建出来的Hidden为false，在
        /// 不还原property的情况下会导致AccessRequests list可见
        /// </summary>
        /// <param name="accessRequestsList"></param>
        private void UpdateAccessRequestsListDefaultSetting(IAveList accessRequestsList)
        {
            try
            {
                accessRequestsList.Hidden = true;
                accessRequestsList.ReadSecurity = 1;
                accessRequestsList.WriteSecurity = 1;
                accessRequestsList.RequestAccessEnabled = false;
                accessRequestsList.NoCrawl = true;
                accessRequestsList.DisableGridEditing = true;
                accessRequestsList.Update();
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while update access requests list default settings. List url: {0}, error: {1}", accessRequestsList.RootFolder.ServerRelativeUrl, e);
            }
        }
        /// <summary>
        /// 如果要加入的Feature还有依赖的Feature,需要先打开依赖的Feature。
        /// </summary>
        /// <param name="featureDefine"></param>
        /// <param name="recursionLevel">递归的level数，防止循环依赖产生死锁。默认最多递归5次</param>
        private void AddDependenceFeature(IAveFeatureDefinition featureDefine, int recursionLevel = 0)
        {
            /*
             * publishing feature:22a9ef51-737b-4ff2-9346-694633fe4416 依赖于
             * site collection Publishing Infrastructure feature:f6924d36-2fa8-4f0b-b16d-06b7250180fa,
             * 由于两者之间的依赖在微软提供的配置文件里面没有给出，所以这里特殊处理。
             */
            if (featureDefine != null)
            {
                if (featureDefine.FeatureId.Equals(new Guid("22a9ef51-737b-4ff2-9346-694633fe4416")))
                {
                    if (mAveSPWeb.SPWeb.Site.Features[AveSP2010FeatureDefinitions.PublishingSite] == null)
                    {
                        mAveSPWeb.SPWeb.Site.Features.Add(AveSP2010FeatureDefinitions.PublishingSite, true);
                    }
                }
                //var dependencyFeatures = featureDefine.ActivationDependencies;
                //if (dependencyFeatures != null && dependencyFeatures.Count > 0)
                //{
                //    foreach (var dependencyFeature in dependencyFeatures)
                //    {
                //        var dependeceId = dependencyFeature.FeatureId;
                //        var dependencyFeatureDefine = GetFeatureDefination(dependeceId);
                //        // 递归检查是否还有依赖Feature。
                //        if (++recursionLevel <= 5)
                //        {
                //            AddDependenceFeature(dependencyFeatureDefine, recursionLevel);
                //            if (mAveSPWeb.SPWeb.Site.Features[dependeceId] == null)
                //            {
                //                mAveSPWeb.SPWeb.Site.Features.Add(dependeceId, true);
                //            }
                //        }
                //        else
                //        {
                //            throw new Exception("Dependency features can only have 5 layers.")
                //        }
                //    }
                //}
            }
        }

        private IAveFeatureDefinition GetFeatureDefination(Guid id)
        {
            IAveFeatureDefinition definition;
            definition = mAveSPWeb.SPWeb.Site.FeatureDefinitions[id];
            if (definition == null)
            {
                definition = mAveSPWeb.SPWeb.Site.WebApplication.Farm.FeatureDefinitions[id];
            }
            return definition;
        }

        public void RestoreListSelf(AveListInfo listInfo)
        {
            RestoreListSelf(listInfo, ListRestoreOption.Title);
        }

        /// <summary>
        /// 这个函数主要是为了load或者创建基本的list所需要的，如果需要还原setting，请到restore property函数中。
        /// </summary>
        /// <param name="listInfo"></param>
        public void RestoreListSelf(AveListInfo listInfo, ListRestoreOption option, bool allowRestoreToSameList = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.RestoreListSelf"))
            {

                mAveSPWeb.ReloadWebAndParentInternalForSPRequestTimeout(false);
                mOldId = listInfo.Id;

                if (string.Compare(mName, "{System Folder}", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    mListInfo = listInfo;
                    //need do test if some information has to be restored.
                    //maybe need not.
                    return;
                }
                if (!mAveSPWeb.SPWeb.IsRootWeb && listInfo.RootWebOnly)
                {
                    //if this list is root web only and the customer retore it to subSite
                    //we just skip this list
                    NeedContinue = false;
                    return;
                }

                if (WorkflowRelatedListNeedSkipped(listInfo.BaseTemplate))
                {
                    if (ParentSite.AveSite.IsOnlineSite)
                    {
                        NeedContinue = false;
                        return;
                    }
                    else
                    {
                        if (!listInfo.ServerRelativeUrl.EndsWith("/NintexTemplates", StringComparison.OrdinalIgnoreCase) && !listInfo.ServerRelativeUrl.EndsWith("/NintexSnippets", StringComparison.OrdinalIgnoreCase))
                        {
                            NeedContinue = false;
                            return;
                        }
                    }
                }
                // skip nintex form library
                if (listInfo.IsNintexFormLibrary)
                {
                    NeedContinue = false;
                    return;
                }

                if (CheckNeedSkipListWhileOffice365DenyAddAndCustomizePages(listInfo))
                {
                    NeedContinue = false;
                    SkipException = new AveWrapperSkipException(AveInternalResourceKey.Wrapper_Exception_Restore_SkipListWhenDenyAddAndCustomizePagesStatus, listInfo.Title, ParentWeb.Name);
                    return;
                }

                mListInfo = listInfo;
                if (ParentSite.MappingManager.ListMappingManager.ListTemplateMapping.ContainsKey(listInfo.BaseTemplate))
                {
                    listInfo.BaseTemplate = ParentSite.MappingManager.ListMappingManager.ListTemplateMapping[listInfo.BaseTemplate];
                    mListInfo.BaseTemplate = listInfo.BaseTemplate;
                }
                try
                {
                    mSPList = FindList(option, listInfo, allowRestoreToSameList);
                    if (mSPList == null)
                    {
                        mName = GetAvailableListTitle(mName);
                        throw new ArgumentException("List does not exist: " + mName);
                    }
                    CheckListTemplateConflict(listInfo.BaseType, listInfo.TemplateFeatureId, mSPList);

                    mId = mSPList.ID;
                }
                catch (ArgumentException)
                {
                    //check if the list is conflict with the dest's RecycleBinData.
                    //if it is really confict, we will skip this list
                    if (this.mRestringFolder.IsIncludingRecycleBinData && this.mRestoreOption.CheckRestoreOption(AveRestoreMode.Default))
                    {
                        bool isConflict = IsConfictWithRecycle(mName, ParentWeb.SPWeb.ID, mQueryService);
                        if (isConflict)
                        {
                            NeedContinue = false;
                            SkipException = new GCommon.Utility.Exceptions.SharePoint.DuplicatedObjectInRecycleBinException(GCommon.Utility.I18N.ContextValues.SharePoint.ObjectType.List);
                            return;
                        }
                    }
                    if (CheckListTemplateNeedCreated())
                    {
                        CreateList(mName);
                    }
                    else
                    {
                        log.Info("skip create list: {0}, due to the template: {1} can't be duplicated in the site", mName, mListInfo.BaseTemplate);
                    }
                }
                if (Guid.Equals(mListInfo.Id, mAveSPWeb.TaxonomyHiddenList))
                {
                    IAveWeb rootWeb = mAveSPWeb.ParentSite.SPSite.RootWeb;
                    rootWeb.ReloadWeb();
                    if (mSPList != null)
                    {
                        mSPList.Reload();
                    }
                    if (rootWeb.Properties != null)
                    {
                        if (rootWeb.Properties.ContainsKey("TaxonomyHiddenList"))
                        {
                            rootWeb.Properties["TaxonomyHiddenList"] = mSPList.ID.ToString();
                        }
                        else
                        {
                            rootWeb.Properties.Add("TaxonomyHiddenList", mSPList.ID.ToString());
                        }
                        rootWeb.Properties.Update();
                    }
                    mIsTaxonomyList = true;
                }
                if (this.ParentSite.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel && this.ParentSite.SPSite.CompatibilityLevel == 15
                    && mSPList.RootFolder.ServerRelativeUrl.EndsWith("/Lists/PublishedFeed", StringComparison.OrdinalIgnoreCase))
                {
                    this.needsElevation = true;
                }
                try
                {
                    //反插还原document时，不会走到RestoreListRootFolder方法，所以在这里也先把ecm_AutoDeclareRecords置成false，在post action更新回来。
                    string listAutoDeclareRecords = mSPList.RootFolder.Properties.ContainsKey("ecm_AutoDeclareRecords") ?
                        mSPList.RootFolder.Properties["ecm_AutoDeclareRecords"] as string : null;
                    if (string.Equals(listAutoDeclareRecords, "True", StringComparison.OrdinalIgnoreCase))
                    {
                        mAutoDeclareRecord = true;
                        mSPList.RootFolder.Properties["ecm_AutoDeclareRecords"] = "False";
                        mSPList.RootFolder.Update();
                    }
                    string webRootFolderUrl = mAveSPWeb.SPWeb.RootFolder.ServerRelativeUrl;
                    string listRootFolderUrl = mSPList.RootFolder.ServerRelativeUrl;
                    mUrl = mAveSPWeb.SPWeb.Url.TrimEnd('/') + "/" + listRootFolderUrl.Substring(webRootFolderUrl.Length).Trim('/');
                    if (!string.Equals(mListInfo.Url, mUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddAbsoluteUrlMapping(mListInfo.Url, mUrl);
                    }
                    if (mListInfo.ServerRelativeUrl != null && !mListInfo.ServerRelativeUrl.Equals(listRootFolderUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddListUrlMapping(mListInfo.ServerRelativeUrl, listRootFolderUrl);
                    }
                    mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddListIdMapping(mListInfo.Id, mSPList.ID);
                    mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddListTitleMapping(mAveSPWeb.SPWeb.ID, mListInfo.Title, mSPList.Title);
                    if (this.needsElevation)
                    {
                        EnsureElevatedObject();
                    }
                    //AveFields.LoadExistLookupFields();
                }
                catch (AveSecurityTrimingException)
                {
                    throw;// new AveSecurityTrimingException(ex.Message, ex);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while getting list root folder url. ListId:{0}, ListTitle:{1}\n error message:{2}", mId, mName, e));
                    //mLog.Warn(e, "An error occurred while getting list root folder url. ListId:{0}, ListTitle:{1}", mId, mName);
                }

            }

        }

        private void EnsureElevatedObject()
        {
            try
            {
                this.elevatedSite = this.ParentSite.ObjectModelFactory.CreateElevatedSite(this.ParentSite.SPSite.Url);
                this.elevatedWeb = this.elevatedSite.OpenWeb(this.ParentWeb.SPWeb.ID);
                this.mSPList = this.elevatedWeb.Lists[mSPList.ID];
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, "Failed to elevate SharePoint object. SPWeb Url:{0}. Error:{1}", this.mAveSPWeb.Url, ex.ToString());
            }
        }

        private IAveList FindList(ListRestoreOption option, AveListInfo listInfo, bool allowRestoreToSameList)
        {
            IAveList list = null;
            if ((option & ListRestoreOption.Title) == ListRestoreOption.Title)
            {
                list = FindListByTitle(mName);
                if (list != null)
                {
                    log.Log(AveLogLevel.DEBUG, "Find list by title:{0}", mName);
                }
            }
            if (list == null && (option & ListRestoreOption.Url) == ListRestoreOption.Url)
            {
                string url = AveReplaceProcessor.UrlReplace(listInfo.ServerRelativeUrl, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.WebUrlMapping,
                new ReplaceOption(true), mAveSPWeb.ParentSite.SourceSiteInfo, mAveSPWeb.ParentSite.ServerRelativeUrl);
                list = FindListByUrl(url);
                //todo need update list title.
                if (list != null)
                {
                    log.Log(AveLogLevel.DEBUG, "Find list by url:{0}", url);
                }
            }
            if (list == null && listInfo.BaseTemplate == (int)AveListTemplateType.CallTrack && mAveSPWeb.WebInfo.WebTemplate.Equals("SGS#0"))
            {
                //special for Phone Memo list under Group Work Site 
                list = mAveSPWeb.SPWeb.Lists.GetById(mOldId);
                list.Title = mName;
                list.Update();
            }
            // avoid to find the same list
            if (list != null && !allowRestoreToSameList && mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.ListIdMappingContainsValue(list.ID))
            {
                list = null;
            }
            return list;
        }

        private IAveList FindListByUrl(string url)
        {
            IAveList list = null;
            if (url.StartsWith(mAveSPWeb.SPWeb.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    list = mAveSPWeb.SPWeb.GetList(url);
                    //if (list != null && !string.Equals(mSPList.Title, mName, StringComparison.OrdinalIgnoreCase))
                    //{
                    //    //list.Title = GetAvailableListTitle(mName);
                    //    list.Update();
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.UpdateListFailed, e.ToString());
                    list = null;
                }
            }
            return list;
        }

        private IAveList FindListByTitle(string title)
        {
            return mAveSPWeb.SPWeb.Lists.TryGetList(title);
        }

        private string GetAvailableListTitle(string title)
        {
            int count = 1;
            string newTitle = title;
            while (count <= 500)
            {
                if (FindListByTitle(newTitle) == null)
                {
                    return newTitle;
                }
                newTitle = string.Format("{0}{1}", title, count);
                count++;
            }
            throw new Exception();
        }

        /// <summary>
        ///meeting user 类型的list，一个meeting site只能有一个实例。创建不出第二个这样的list。
        //Blog中的Categoris和Posts类型只能有一个实例, 但是Fast Search Center类型的站点中可以创建多个Posts类型的list (Tabs List)
        //开启publishing feature的Pages
        /// </summary>
        /// <param name="exist"></param>
        /// <returns></returns>
        private bool CheckListTemplateNeedCreated()
        {
            bool needCreate = true;
            if (mListInfo.BaseTemplate == (int)AveListTemplateType.Posts && mAveSPWeb.SPWeb.WebTemplate != "BLOG")
            {
                return needCreate;
            }

            bool isSearch = string.Equals(mAveSPWeb.SPWeb.WebTemplate, "SRCHCEN", StringComparison.OrdinalIgnoreCase) & mListInfo.BaseTemplate == 301;

            if (uniqueListTemplates.Contains(mListInfo.BaseTemplate) && !isSearch)
            {
                mAveSPWeb.SPWeb.Lists.FirstOrDefault(list =>
                {
                    if (list.BaseTemplate == (AveListTemplateType)mListInfo.BaseTemplate)
                    {
                        mSPList = list;
                        mId = mSPList.ID;
                        mSPList.Title = mName;
                        mSPList.Update();
                        needCreate = false;
                        return true;
                    }
                    return false;
                });
            }
            else if (catalogTemplates.Contains(mListInfo.BaseTemplate))
            {
                mSPList = mAveSPWeb.SPWeb.GetCatalog((AveListTemplateType)mListInfo.BaseTemplate);
                if (mSPList != null)
                {
                    mId = mSPList.ID;
                    needCreate = false;
                }
            }

            if (needCreate == true && mListInfo.BaseTemplate == (int)AveListTemplateType.MicroFeed && mListInfo.TemplateFeatureId.Equals(AveSP2013FeatureDefinitions.MySiteMicroBlog))
            {
                if (mAveSPWeb.SPWeb.Features[mListInfo.TemplateFeatureId] == null)
                {
                    try
                    {
                        //通过反插激活feature来自动创建MicroFeed list
                        mAveSPWeb.SPWeb.Features.Add(mListInfo.TemplateFeatureId, true);
                        // 在加入了web的feature之后，需要重新load一下，保证缓存中的数据和真实数据一致。
                        mAveSPWeb.SPWeb.ReloadWeb();
                        mAveSPWeb.SPWeb.Lists.FirstOrDefault(list =>
                        {
                            if (list.BaseTemplate == (AveListTemplateType)mListInfo.BaseTemplate)
                            {
                                mSPList = list;
                                mId = mSPList.ID;
                                mSPList.Title = mName;
                                mSPList.Update();
                                needCreate = false;
                                return true;
                            }
                            return false;
                        });
                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred while activating feature.FeatureId: {0},Error: {1}", mListInfo.TemplateFeatureId, e);
                    }
                }
            }
            return needCreate;
        }


        private void CheckListTemplateConflict(int baseType, Guid templateFeatureId, IAveList list)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.CheckListTemplateConflict"))
            {
                if (baseType == 0)
                {
                    if ((int)list.BaseTemplate == 1301 && templateFeatureId.ToString().Equals("29d85c25-170c-4df9-a641-12db0b9d4130", StringComparison.OrdinalIgnoreCase))
                    {
                        //For Translators List whose BaseType is GenericList but in Dictionary mTemplateToBaseType is DocumentLibrary.
                        return;
                    }
                    else if (mTemplateToBaseType.ContainsKey(templateFeatureId))
                    {
                        if (list.BaseType != mTemplateToBaseType[templateFeatureId])
                        {
                            throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_DifferentBaseTypeList);
                        }
                    }
                }
                else
                {
                    if ((int)list.BaseType != baseType)
                    {
                        throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_DifferentBaseTypeList);
                    }
                }

            }

        }
        /// <summary>
        /// Skip this three list associated with workflow,we will handle it when restore workflow
        /// </summary>
        /// <param name="baseTemplate"></param>
        /// <returns></returns>
        private bool WorkflowRelatedListNeedSkipped(int baseTemplate)
        {
            if (baseTemplate == (int)AveListTemplateType.NoCodePublic ||
                baseTemplate == (int)AveListTemplateType.NoCodeWorkflows ||
                baseTemplate == (int)AveListTemplateType.WorkflowHistory ||
                baseTemplate == (int)AveListTemplateType.NintexWrokflow ||
                baseTemplate == (int)AveListTemplateType.WFSVC)
            {
                return true;
            }
            return false;
        }

        private bool CheckNeedSkipListWhileOffice365DenyAddAndCustomizePages(AveListInfo listInfo)
        {
            if (ParentSite.AveSite.IsOnlineSite && SpecialListTemplateIdsUnderPersonalSite.Contains(listInfo.BaseTemplate))
            {
                if (mAveSPWeb.SPWeb.Site.DenyAddAndCustomizePagesStatus)
                {
                    return true;
                }
            }
            return false;
        }

        public string ServerRelativeUrl
        {
            get { return string.Empty; }
        }

        public string Name
        {
            get { return mName; }
            set { mName = value; }
        }

        public void RestoreUnRestoreWebPart(IReport report)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.RestoreUnRestoreWebPart"))
            {

                Dictionary<Guid, Dictionary<string, List<object>>> fileWebParts = null;
                if (mOldId == Guid.Empty)
                {
                    return;
                }
                if (mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.GetValueFromUnRestoreWebPartInfo(mOldId, out fileWebParts))
                {
                    try
                    {
                        foreach (KeyValuePair<Guid, Dictionary<string, List<object>>> pair in fileWebParts)
                        {
                            IAveWeb web = null;
                            try
                            {
                                Guid webId = pair.Key;
                                if (webId == mAveSPWeb.SPWeb.ID)
                                {
                                    web = mAveSPWeb.SPWeb;
                                }
                                else
                                {
                                    web = mAveSPWeb.ParentSite.SPSite.OpenWeb(webId);
                                }

                                foreach (KeyValuePair<string, List<object>> filePair in pair.Value)
                                {
                                    AveSPDoc spDoc = new AveSPDoc(mAveSPWeb.ParentSite);
                                    try
                                    {
                                        IAveFile file = web.GetFile(filePair.Key);
                                        int userId = -1;
                                        if (file.CheckOutType != AveCheckOutType.None && (mAveParentSite.QueryService != null && mAveParentSite.QueryService.IsCheckOutFile(mAveParentSite.SPSite.ID, file.UniqueId, ref userId) && userId != mAveSPWeb.SPWeb.CurrentUser.ID))
                                        {
                                            IAveUser checkOutUser = null;
                                            try
                                            {
                                                checkOutUser = mAveSPWeb.SPWeb.SiteUsers.GetByID(userId);
                                            }
                                            catch (Exception e)
                                            {
                                                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetUserByIdError, e.ToString());
                                            }
                                            if (checkOutUser != null)
                                            {
                                                file = (mAveParentSite.GetCheckoutWeb(mAveParentSite.SPSite.ID, mAveSPWeb.SPWeb, checkOutUser, file.UniqueId).GetFile(file.UniqueId));
                                            }

                                        }
                                        spDoc.SPFile = file;
                                        spDoc.SPWeb = file.Web;
                                        spDoc.SetRestoreOption(RestoreOption);
                                        spDoc.ParentList = this;
                                        //当我们在进行List Pose Action来还原没有还原的ListWebPart时，不应该删除已经存在的Web Part
                                        spDoc.RestoreWebPart(filePair.Value, false);
                                    }
                                    catch (AveSecurityTrimingException)
                                    {
                                        throw;
                                    }
                                    catch (Exception fe)
                                    {
                                        log.Log(AveLogLevel.WARN, string.Format("An error occurred when restore un-restored webpart in file, file id: {0}.\n error message:{1}", filePair.Key, fe));
                                        //mLog.Warn("An error occurred when restore un-restored webpart in file, file id: {0}. Reason: {1}.", filePair.Key.ToString(), fe.ToString());
                                    }
                                    finally
                                    {
                                        report.AddDetails(spDoc.GetReport().GetDetails());
                                    }
                                }
                            }
                            catch (AveSecurityTrimingException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                log.Log(AveLogLevel.WARN, string.Format("An error occurred when restore un-restored webpart.\n error message:{0}", ex));
                                //mLog.Warn("An error occurred when restore un-restored webpart. Reason: {0}.", ex.ToString());
                            }
                            finally
                            {
                                if (web != null && web.ID != mAveSPWeb.SPWeb.ID)
                                {
                                    web.Dispose();
                                }
                            }
                        }
                    }
                    catch (AveSecurityTrimingException ex)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred when restore un-restored webpart.\n error message:{0}", ex));
                        report.AddDetail(new AveWrapperWebpartReportDto("WebPart", "WebPart", null, string.Empty, string.Empty, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionToRestoreWebPart, ex.Message));
                    }
                }

            }

        }
        /// <summary>
        /// 详细请参考AveListSettingFlags类
        /// 1           ListSettingBackup       (0-No,      1-Yes)
        /// 2           IsListSettingChanged    (0-No,      1-Yes)
        /// 4           EnableVersions          (0-Disable, 1-Enable)
        /// 8           EnableMinorVersions     (0-Disable, 1-Enable)
        /// 16          EnableModeration        (0-Disable, 1-Enable)
        /// 32          ForceCheckout           (0-Disable, 1-Enable)
        /// </summary>
        private long mListSettingFlag;
        private AveDraftVisibilityType draftVisibilityType = AveDraftVisibilityType.None;
        private string mValidationFormula = null;
        private string mValidationMessage = null;
        private bool? mEnforceDataValidation = null;
        private bool? mAutoDeclareRecord = null;
        private bool mModifiedFieldChanged = false;
        private int mMajorVersionLimit = -1;
        private int mMajorWithMinorVersionsLimit = -1;


        public void SetListSettingFlags(int value)
        {
            mListSettingFlag |= value;
        }
        private bool IsColColumn(string colName)
        {
            //添加对column 类型的判断，SP对类型的数量是有限制的，可以通过SP的数据库查询，当前没发现超过数据的情况，因此没有添加对于超过限制的判断，如果有问题，需要添加检查类型数量的逻辑
            List<string> allcols = new List<string> { "nvarchar", "ntext", "sql_variant", "int", "float", "datetime", "bit", "uniqueidentifier" };
            Regex reg = new Regex("^(nvarchar|ntext|sql_variant|int|float|datetime|bit|uniqueidentifier)[0-9]*$");
            return reg.IsMatch(colName);
        }
        public List<string> SetNeedSetNullFields(AveBaseItemInfo itemInfo, string folderServerRelativeUrl)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.SetNeedSetNullFields"))
            {
                Dictionary<string, object> fieldValues = itemInfo.FieldsInfo.Fields;
                List<string> needSetNullFields = new List<string>();
                folderServerRelativeUrl = string.Format("{0}/", folderServerRelativeUrl.TrimEnd('/'));
                //ExternalList 没有ColName，会抛异常
                if (mSPList != null && mSPList.BaseTemplate != AveListTemplateType.ExternalList && (int)this.mSPList.BaseTemplate != 160)
                {
                    IAveFieldCollection fieldCollection = mSPList.Fields;
                    bool isCollecterList = mSPList.IsConnectorList.HasValue ? mSPList.IsConnectorList.Value : false;
                    foreach (IAveField field in fieldCollection)
                    {
                        try
                        {
                            object obj = field.ColName;
                            if (obj != null
                                //ADO-129426 item的SetNeedSetNullFields逻辑中，过滤BaseType是Facilities类型的column，在还column的过程中，
                                //如果将这个column设为null，在update的时候会报System.Exception: Field or property "Facilities" does not exist.的错。
                                && !string.Equals(field.TypeAsString, "Facilities", StringComparison.OrdinalIgnoreCase)
                                //ADO-89825 App Store Site中，特殊field AppMetadataLocale不能设置为null。
                                && !field.ID.Equals(new Guid("{14c6cd06-7417-42c1-a051-89e455fd1090}"))//Exclude from ADO-89825
                                                                                                       //Do need set Connector URL  column value to null.
                                && !(isCollecterList && field.InternalName.Equals("URL", StringComparison.OrdinalIgnoreCase))
                                && !(itemInfo.ItemType == AveItemType.Folder && field.InternalName.Equals("Folder", StringComparison.OrdinalIgnoreCase) && mAveParentSite.SPContextKind == AveContextKind.ClientObjectModel))
                            {
                                string colName = obj.ToString();
                                if (IsColColumn(colName) && IsSupportToSetNull(field.InternalName))
                                {
                                    if (field.Type == AveFieldType.WorkflowStatus
                                        //ADO-181176,ADO-179023 如果lookup column关联的list尚未还原，给这个column设置为null，ClientAPI Update时会抛出System.Exception: Field or property "Entity" does not exist.的错。
                                        || (this.mAveParentSite.SPContextKind == AveContextKind.ClientObjectModel && field.Type == AveFieldType.Lookup))
                                    {
                                        continue;
                                    }

                                    //ADO-219805 针对Folder -> DocumentSet 的ContentType mapping的特殊处理，
                                    //HTML_x0020_File_x0020_Type 在DocumentSet的情况下值是Sharepoint.DocumentSet，在Folder的情况下值是null
                                    //HTML_x0020_File_x0020_Type 值是null的情况下 做Folder 显示，值是Sharepoint.DocumentSet的情况下 做DocumentSet 显示
                                    if (itemInfo.ItemType == AveItemType.Folder
                                        && string.Equals("HTML_x0020_File_x0020_Type", field.InternalName, StringComparison.OrdinalIgnoreCase)
                                        && itemInfo.FieldsInfo.Fields.ContainsKey("ContentType")
                                        && (((AveFieldValueInfo)itemInfo.FieldsInfo.Fields["ContentType"]).ColValue is IAveContentTypeId)
                                        && AveSPDocumentSet.IsDocumentSet((IAveContentTypeId)((AveFieldValueInfo)itemInfo.FieldsInfo.Fields["ContentType"]).ColValue)
                                        && !itemInfo.FieldsInfo.Fields.ContainsKey("HTML_x0020_File_x0020_Type"))
                                    {
                                        itemInfo.FieldsInfo.Fields["HTML_x0020_File_x0020_Type"] = new AveFieldValueInfo() { ColName = "HTML_x0020_File_x0020_Type", ColValue = "Sharepoint.DocumentSet" };
                                    }

                                    if (KeepDefaultValue)
                                    {
                                        var value = mSPList.ClientLocationBasedDefaults.FirstOrDefault(p => (folderServerRelativeUrl.StartsWith(p.Key, StringComparison.OrdinalIgnoreCase))
                                            && p.Value.ContainsKey(field.InternalName));
                                        if (value.Value != null)
                                        {
                                            var isTaxonomyField = field is IAveTaxonomyField;
                                            fieldValues[field.InternalName] = new AveFieldValueInfo()
                                            {
                                                ColValue = isTaxonomyField ? AveFieldHelper.GetTaxonomyFieldValue(value.Value[field.InternalName])
                                                : value.Value[field.InternalName]
                                            };
                                            if (isTaxonomyField)
                                            {
                                                if (!AveFields.TaxonomyFields.Contains(field.InternalName))
                                                {
                                                    AveFields.TaxonomyFields.Add(field.InternalName);
                                                }
                                            }
                                            continue;
                                        }
                                        else if (!String.IsNullOrEmpty(field.DefaultValue) || !String.IsNullOrEmpty(field.DefaultFormula))
                                        {
                                            //[ADO-8099]keep default value
                                            var colValue = fieldValues.ContainsKey(field.InternalName)
                                                ? (fieldValues[field.InternalName] as AveFieldValueInfo).ColValue
                                                : null;
                                            if (colValue == null || ((colValue as string) != null && (colValue as string).Equals(string.Empty, StringComparison.OrdinalIgnoreCase)))
                                            {
                                                //mms column需要加到缓存中,否则后续逻辑无法处理
                                                if (field is IAveTaxonomyField)
                                                {
                                                    if (!AveFields.TaxonomyFields.Contains(field.InternalName))
                                                    {
                                                        AveFields.TaxonomyFields.Add(field.InternalName);
                                                    }
                                                }
                                                fieldValues[field.InternalName] = new AveFieldValueInfo() { ColValue = AveFieldHelper.GetFieldDefaultValues(field) };
                                            }
                                            continue;
                                        }
                                    }
                                    needSetNullFields.Add(field.InternalName);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, "An error occurred while SetNeedSetNullFields. error:{0}", e.ToString());
                        }
                    }
                }
                return needSetNullFields;

            }

        }

        public bool IsReportingMetadataList()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.IsReportingMetadataList"))
            {

                bool isReportMetadataList = false;
                try
                {
                    IAveList list = mSPList;
                    IAveWeb web = mAveSPWeb.SPWeb;
                    if (web.Properties != null)
                    {
                        if (web.Properties.ContainsKey("_reportinggallerymetadataid"))
                        {
                            if (string.Equals(web.Properties["_reportinggallerymetadataid"].ToString(), list.ID.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                isReportMetadataList = true;
                            }
                        }
                    }
                    else
                    {
                        if (web.AllProperties.ContainsKey("_reportinggallerymetadataid"))
                        {
                            if (string.Equals(web.AllProperties["_reportinggallerymetadataid"].ToString(), list.ID.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                isReportMetadataList = true;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Warn("charge whether the list is reporting metadata error,Exception:{0}", e.ToString());
                }
                return isReportMetadataList;

            }

        }

        public bool IsSupportToSetNull(string internalName)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.IsSupportToSetNull"))
            {

                bool isSupportToSetNull = true;
                try
                {
                    if ((string.Equals(internalName, "_dlc_Reporting_TemplateId", StringComparison.Ordinal)
                        || string.Equals(internalName, "_dlc_Reporting_QueryAssembly", StringComparison.Ordinal)
                        || string.Equals(internalName, "_dlc_Reporting_InjectionAssembly", StringComparison.Ordinal)
                        || string.Equals(internalName, "_dlc_Reporting_InjectionClass", StringComparison.Ordinal)
                        || string.Equals(internalName, "_dlc_Reporting_IconUrl", StringComparison.Ordinal)
                        || string.Equals(internalName, "_dlc_Reporting_HttpContentType", StringComparison.Ordinal)
                        && IsReportingMetadataList()))
                    {
                        isSupportToSetNull = false;
                    }
                }
                catch (Exception e)
                {
                    log.Warn("charge whether the list field is '_dlc_Reporting_TemplateId', Exception:{0}", e.ToString());
                }
                return isSupportToSetNull;

            }

        }
        //public void SetContentTypeMapping(AveContentTypeMapping contentTypeMapping)
        //{
        //    mContentTypes.ContentTypeMapping = contentTypeMapping;
        //}
        public void ReloadAll()
        {
            try
            {
                ParentWeb.ParentSite.ReloadSite();
                ParentWeb.ReloadWeb();
                ReloadList();
            }
            catch (Exception ex)
            {
                log.Warn("An error occur while reload site web list ,exception:{0}", ex.ToString());
            }
        }
        /// <summary>
        /// Before Restoring Item, backup list setting
        /// </summary>
        /// <param name="needChange">Only For SPM. ADO-103465</param>
        public void BackupListSetting(bool needChange = false)
        {
            using (new AvePerformanceScope("Restore.AveSPList.BackupListSetting"))
            {
                lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("FieldLock"))
                {
                    AveFields.BackupValidationFields();
                    AveFields.BackupFieldsDefaultValue();
                    //由于web.alerts API获取有误，改用还原时候stopAlert
                    AveSPAlert.StopListAlerts(this);
                    if (mSPList == null || (mListSettingFlag & AveListSettingFlags.LIST_SETTING_BACKUP) != 0)
                    {
                        return;
                    }

                    try
                    {
                        //[DOC-70534] 需要将list的RelatedField的DeleteBehavior置为None 
                        IAveRelatedFieldCollection fields = mSPList.GetRelatedFields();
                        if (fields != null)
                        {
                            foreach (IAveRelatedField relatedField in fields)
                            {
                                if (relatedField.RelationshipDeleteBehavior != AveRelationshipDeleteBehavior.None)
                                {
                                    try
                                    {
                                        IAveList tempList = ParentWeb.SPWeb.Lists[relatedField.ListId];
                                        IAveFieldLookup lookupField = tempList.Fields[relatedField.FieldId] as IAveFieldLookup;
                                        if (lookupField != null)
                                        {
                                            mRelatedFieldBehavior[relatedField.FieldId] = relatedField.RelationshipDeleteBehavior;
                                            lookupField.RelationshipDeleteBehavior = AveRelationshipDeleteBehavior.None;
                                            lookupField.Update();
                                        }
                                    }
                                    catch (AveSecurityTrimingException e)
                                    {
                                        //Contribute 权限
                                        log.Log(AveLogLevel.WARN, "Error occurred when set lookup field to none ." + e.ToString());
                                    }
                                    catch (Exception e)
                                    {
                                        log.Log(AveLogLevel.WARN, "Error occurred when set lookup field to none ." + e.ToString());
                                    }
                                }
                            }
                            foreach (IAveField field in mSPList.Fields)
                            {
                                try
                                {
                                    if (field is IAveFieldLookup)
                                    {
                                        IAveFieldLookup lookupField = field as IAveFieldLookup;
                                        if (lookupField.RelationshipDeleteBehavior != AveRelationshipDeleteBehavior.None)
                                        {
                                            mDestFieldBehavior[field.ID] = lookupField.RelationshipDeleteBehavior;
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    log.Log(AveLogLevel.WARN, "Error occurred when set destination lookup field to none ." + e.ToString());
                                }
                            }
                            foreach (Guid fieldId in mDestFieldBehavior.Keys)
                            {
                                if (mSPList.Fields.Contains(fieldId))
                                {
                                    IAveFieldLookup lookupField = mSPList.Fields[fieldId] as IAveFieldLookup;
                                    lookupField.RelationshipDeleteBehavior = AveRelationshipDeleteBehavior.None;
                                    lookupField.Update();
                                }
                            }
                        }
                    }
                    catch (AveSecurityTrimingException)
                    {
                        //Contribute 权限
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.BackupListSettingFailed, e);
                    }
                    this.ListItemSerializer.BeforeSetObjectData();
                }
                lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ListSettingLocker"))
                {
                    //ReloadList();
                    mListSettingFlag |= AveListSettingFlags.LIST_SETTING_BACKUP;

                    if (mSPList.EnableVersioning)
                    {
                        mListSettingFlag |= AveListSettingFlags.LIST_ENABLE_VERSIONS;
                    }

                    if (mSPList.EnableMinorVersions)
                    {
                        mListSettingFlag |= AveListSettingFlags.LIST_ENABLE_MINOR_VERSIONS;
                    }

                    if (mSPList.EnableModeration)
                    {
                        mListSettingFlag |= AveListSettingFlags.LIST_ENABLE_MODERATION;
                    }

                    if (mSPList.ForceCheckout)
                    {
                        mListSettingFlag |= AveListSettingFlags.LIST_FORCE_CHECK_OUT;
                    }

                    if (mSPList.AllowMultiResponses)
                    {
                        mListSettingFlag |= AveListFlags.ALLOWMULTIPLE_RESPONSES_LIST;
                    }

                    this.draftVisibilityType = mSPList.DraftVersionVisibility;

                    bool changed = false;

                    //if (mSPList.ForceCheckout)
                    //{
                    //    mSPList.ForceCheckout = false;
                    //    changed = true;
                    //}
                    if (needChange)
                    {
                        if (!mSPList.EnableModeration && !mSPList.HasExternalDataSource && mSPList.BaseTemplate != AveListTemplateType.Survey)
                        {
                            mSPList.EnableModeration = true;
                            changed = true;
                        }

                        if (!mSPList.EnableModeration && !mSPList.HasExternalDataSource && mSPList.BaseTemplate != AveListTemplateType.DataSources &&
                            mSPList.BaseTemplate != AveListTemplateType.Survey && mSPList.BaseTemplate != AveListTemplateType.ImagesLibrary && mSPList.BaseTemplate != AveListTemplateType.UserInformation && mSPList.BaseTemplate != AveListTemplateType.Posts
                            && mSPList.BaseTemplate != AveListTemplateType.DataConnectionLibrary
                            && !(mSPList.ParentWeb.WebTemplate == "MPS" && (mSPList.BaseTemplate == AveListTemplateType.Meetings || mSPList.BaseTemplate == AveListTemplateType.HomePageLibrary)))
                        {
                            mSPList.EnableModeration = true;
                            changed = true;
                        }

                        if (!mSPList.EnableVersioning
                            && mSPList.BaseTemplate != AveListTemplateType.Survey
                            && mSPList.BaseTemplate != AveListTemplateType.ExternalList
                            && mSPList.BaseTemplate != AveListTemplateType.Meetings
                            && mSPList.BaseTemplate != AveListTemplateType.UserInformation)
                        {
                            mSPList.EnableVersioning = true;
                            changed = true;
                        }
                    }

                    if (mSPList.EnforceDataValidation)
                    {
                        mEnforceDataValidation = mSPList.EnforceDataValidation;
                        mSPList.EnforceDataValidation = false;
                        changed = true;
                    }
                    if (mSPList.BaseTemplate == AveListTemplateType.Survey && !mSPList.AllowMultiResponses)
                    {
                        mSPList.AllowMultiResponses = true;
                        changed = true;
                    }
                    if (mSPList.EnableAssignToEmail)
                    {
                        mSPList.EnableAssignToEmail = false;
                        changed = true;
                        this.ParentSite.MappingManager.SiteMappingManager.AddNeedEnableSendEmailList(this.ParentWeb.SPWeb.ID, this.mSPList.ID);
                    }

                    if (!changed)
                    {
                        return;
                    }
                    try
                    {
                        mSPList.Update();
                        try
                        {
                            if (RestoringDto.GetIsReplicator())
                            {
                                this.ReloadList();//此处调用ContentTypes.Update()方法的时候在13环境下会在EventCache表中产生记录，导致Replicator认为ContentType作了改变而进行了转移，在此为其改为Reload方式
                            }
                            else
                            {
                                ////List对象更新后mContentTypes 也需要更新下以保证其version和当前list version一致，否则contentType的Update会抛异常
                                if (mSPList.AllowContentTypes) //否则会出现异常，需要添加判断过滤下。
                                {
                                    mSPList.ContentTypes.Update();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.UpdateContentTypeError, e.ToString());
                        }
                        SetListSettingFlags(AveListSettingFlags.LIST_SETTING_CHANGED);
                    }
                    catch (AveSecurityTrimingException e)
                    {
                        //Contribute 权限
                        log.Log(AveLogLevel.WARN, string.Format("Can not update this list setting. ListTitle:{0}\n error message:{1}", mSPList.Title, e));
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("Can not update this list setting. ListTitle:{0}\n error message:{1}", mSPList.Title, e));
                        //mLog.Warn(e, "Can not update this list setting. ListTitle:{0}", mSPList.Title);
                    }
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are xml element name. ")]
        public void RestoreListRootFolderProperties()
        {
            //for 2013 project site's task list.Restore rootfolder properties after restoring items.
            if (this.mAveParentSite.SPSite.APIType == AveAPIType.BPOS_S)
            {
                if (this.RootFolder != null && this.RootFolder.Properties != null && this.RootFolder.Properties.ContainsKey("Timeline_Timeline"))
                {
                    try
                    {
                        bool needUpdate = false;
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(this.RootFolder.Properties["Timeline_Timeline"].ToString());
                        foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                        {
                            if (node.Name.Equals("tskSet") || node.Name.Equals("mlSet"))
                            {
                                foreach (XmlElement childNode in node.ChildElements())
                                {
                                    if (childNode.HasAttribute("uid") && !string.IsNullOrEmpty(childNode.Attributes["uid"].Value))
                                    {
                                        int origionalItemId = 0;
                                        if (int.TryParse(childNode.Attributes["uid"].Value, out origionalItemId))
                                        {
                                            int tempAttributesValue = ParentSite.MappingManager.SiteMappingManager.GetMappingItemId(mSPList.ID, origionalItemId);
                                            if (tempAttributesValue != -1)
                                            {
                                                childNode.Attributes["uid"].Value = tempAttributesValue.ToString();
                                                needUpdate = !string.Equals(childNode.Attributes["uid"].Value, origionalItemId.ToString(), StringComparison.OrdinalIgnoreCase);
                                            }
                                            else//对应Item没还，将uid置0，否则出现脏数据。
                                            {
                                                childNode.Attributes["uid"].Value = "0";
                                                needUpdate = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (needUpdate)
                        {
                            this.RootFolder.Properties["Timeline_Timeline"] = doc.OuterXml;
                            this.RootFolder.Update();
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Debug("Post restore list root folder failed.Title:{0},Web:{1},Error Message:{2}", SPList == null ? "" : SPList.Title, this.ParentWeb.ServerRelativeUrl, ex);
                    }
                }
            }
        }

        public void RestoreListSetting()
        {
            using (new AvePerformanceScope("Restore.AveSPList.RestoreListSetting"))
            {
                try
                {
                    //每Restore一个Document,可能会更改List的Setting。Request底层记录List Setting,然后在还原完当前Document时把记录的List setting更新到AveList中，作为真正的List setting。
                    //而当多线程Restore时,Request里记录的List setting很可能就不是List的真实Setting了。
                    //具体可见: AveListMemento 以及Client List的Reload方法。
                    if (this.ParentSite.SPContextKind == AveContextKind.ClientObjectModel && mSPList != null)
                    {
                        mSPList.Reload();
                    }
                    this.ListItemSerializer.AfterSetObjectData();
                    try
                    {
                        if (mSPList != null)
                        {
                            foreach (IAveRelatedField field in mSPList.GetRelatedFields())
                            {
                                if (mRelatedFieldBehavior.ContainsKey(field.FieldId))
                                {
                                    try
                                    {
                                        IAveList tempList = ParentWeb.SPWeb.Lists[field.ListId];
                                        IAveFieldLookup lookupField = tempList.Fields[field.FieldId] as IAveFieldLookup;
                                        if (lookupField != null)
                                        {
                                            lookupField.RelationshipDeleteBehavior = mRelatedFieldBehavior[field.FieldId];
                                            lookupField.Update();
                                        }
                                    }
                                    catch (AveSecurityTrimingException)
                                    {
                                        throw;
                                    }
                                    catch (Exception e)
                                    {
                                        log.Log(AveLogLevel.WARN, "Error occurred when restore list's related field behavior " + e.ToString());
                                    }
                                }
                            }
                            foreach (Guid fieldId in mDestFieldBehavior.Keys)
                            {
                                try
                                {
                                    if (mSPList.Fields.Contains(fieldId))
                                    {
                                        IAveFieldLookup lookupField = mSPList.Fields[fieldId] as IAveFieldLookup;
                                        lookupField.RelationshipDeleteBehavior = mDestFieldBehavior[fieldId];
                                        lookupField.Update();
                                    }
                                }
                                catch (AveSecurityTrimingException)
                                {
                                    throw;
                                }
                                catch (Exception e)
                                {
                                    log.Log(AveLogLevel.WARN, "Error occurred when set destination lookup field to none ." + e.ToString());
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
                        log.Log(AveLogLevel.WARN, WrapperRestoreResource.RestoreListSettingFailed, e);
                    }

                    if (mSPList != null)
                    {
                        bool listSettingChanged = false;
                        bool hasTryReloadWeb = false;//
                        while (true)
                        {
                            if (mValidationFormula != null)
                            {
                                var fieldDisplayNameMapping = this.AveFields.FieldMapping.EnumFieldDisplayNameMapping().ToDictionary(pair => pair.Key, pair => pair.Value);
                                foreach (var pair in fieldDisplayNameMapping)
                                {
                                    if (mValidationFormula.Contains(pair.Key) && !pair.Key.Equals(pair.Value, StringComparison.Ordinal))
                                    {
                                        mValidationFormula = mValidationFormula.Replace(pair.Key, pair.Value);
                                    }
                                }
                                mSPList.ValidationFormula = mValidationFormula;
                                listSettingChanged = true;
                            }
                            if (mValidationMessage != null)
                            {
                                mSPList.ValidationMessage = mValidationMessage;
                                listSettingChanged = true;
                            }
                            if (mEnforceDataValidation != null)
                            {
                                mSPList.EnforceDataValidation = (Boolean)mEnforceDataValidation;
                                listSettingChanged = true;
                            }
                            if (mAutoDeclareRecord != null && mAutoDeclareRecord.Value)
                            {
                                mSPList.RootFolder.Properties["ecm_AutoDeclareRecords"] = mAutoDeclareRecord.ToString();
                                mSPList.RootFolder.Update();
                            }
                            #region parse list setting
                            RestoreListVersionSetting(ref listSettingChanged);
                            if (mSPList.HasUniqueRoleAssignments && mSPList.RequestAccessEnabled != mRequestAccessEnabled)
                            {
                                mSPList.RequestAccessEnabled = mRequestAccessEnabled;
                                listSettingChanged = true;
                            }

                            #endregion

                            if (listSettingChanged)
                            {
                                try
                                {
                                    mSPList.Update();
                                    mListSettingFlag = AveListSettingFlags.LIST_SETTING_NULL;
                                }
                                catch (AveSecurityTrimingException)
                                {
                                    throw;
                                }
                                catch (Exception e)
                                {
                                    if (!hasTryReloadWeb)
                                    {
                                        mAveSPWeb.ReloadWeb();
                                        hasTryReloadWeb = true;
                                        ReloadList();
                                        continue;
                                    }
                                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while resetting list setting. ListTitle:{0}\n error message:{1}", this.mSPList.Title, e));
                                    //mLog.Warn(e, "An error occurred while reseting list setting. ListTitle:{0}", mSPList.Title);
                                }
                            }
                            break;
                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    //qlluo: Post action do not support report, remove it.
                    //reportor.AddDetail(new AveWrapperReportDto("ListSetting", "ListSetting", AveReportObjectType.ListSetting, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreListSetting + ex.Message));
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while resetting list setting. ListTitle:{0}\n error message:{1}", this.mSPList.Title, ex));
                }
            }
        }

        private bool RestoreListVersionSetting(ref bool listSettingChanged)
        {
            if ((mListSettingFlag & AveListSettingFlags.LIST_SETTING_CHANGED) != 0)
            {
                bool enableVersioning = (mListSettingFlag & AveListSettingFlags.LIST_ENABLE_VERSIONS) != 0;
                if (mSPList.EnableVersioning != enableVersioning)
                {
                    mSPList.EnableVersioning = enableVersioning;
                    listSettingChanged = true;
                }

                bool enableMinorVersions = (mListSettingFlag & AveListSettingFlags.LIST_ENABLE_MINOR_VERSIONS) != 0;
                if (mSPList.EnableMinorVersions != enableMinorVersions)
                {
                    mSPList.EnableMinorVersions = enableMinorVersions;
                    listSettingChanged = true;
                }

                bool enableModeration = (mListSettingFlag & AveListSettingFlags.LIST_ENABLE_MODERATION) != 0;
                if (mSPList.EnableModeration != enableModeration)
                {
                    mSPList.EnableModeration = enableModeration;
                    listSettingChanged = true;
                }

                bool forceCheckOut = (mListSettingFlag & AveListSettingFlags.LIST_FORCE_CHECK_OUT) != 0;
                if (mSPList.ForceCheckout != forceCheckOut)
                {
                    mSPList.ForceCheckout = forceCheckOut;
                    listSettingChanged = true;
                }

                bool allowMultiResponses = (mListSettingFlag & AveListFlags.ALLOWMULTIPLE_RESPONSES_LIST) != 0;
                if (mSPList.AllowMultiResponses != allowMultiResponses)
                {
                    mSPList.AllowMultiResponses = allowMultiResponses;
                    listSettingChanged = true;
                }
            }
            #region 由于SP16API发生变化, 这两个setting必须放在其他Version setting之前更新。
            if (mMajorVersionLimit > 0)
            {
                mSPList.MajorVersionLimit = mMajorVersionLimit;
                listSettingChanged = true;
            }
            if (mMajorWithMinorVersionsLimit > 0)
            {
                mSPList.MajorWithMinorVersionsLimit = mMajorWithMinorVersionsLimit;
                listSettingChanged = true;
            }
            #endregion

            if (this.draftVisibilityType != AveDraftVisibilityType.None && this.draftVisibilityType != mSPList.DraftVersionVisibility)
            {
                mSPList.DraftVersionVisibility = this.draftVisibilityType;
                listSettingChanged = true;
            }
            return listSettingChanged;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Templateist")]
        public void Update_ReportTemplateistWebProperties()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.Update_ReportTemplateistWebProperties"))
            {
                var value = Guid.Empty;
                if (mAveSPWeb.SPWeb.Properties != null && mAveSPWeb.SPWeb.Properties.ContainsKey("_reportinggallerytemplateid") && mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.GetValueFromListIdMapping(new Guid(mAveSPWeb.SPWeb.Properties["_reportinggallerytemplateid"]), out value))
                {
                    if (value == mSPList.ID)
                    {
                        mAveSPWeb.SPWeb.Properties["_reportinggallerytemplateid"] = mSPList.ID.ToString();
                        mAveSPWeb.SPWeb.Properties.Update();
                    }
                }
                else if (mAveSPWeb.SPWeb.AllProperties.ContainsKey("_reportinggallerytemplateid") && mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.GetValueFromListIdMapping(new Guid(mAveSPWeb.SPWeb.AllProperties["_reportinggallerytemplateid"].ToString()), out value))
                {
                    if (value == mSPList.ID)
                    {
                        mAveSPWeb.SPWeb.AllProperties["_reportinggallerytemplateid"] = mSPList.ID.ToString();
                        mAveSPWeb.SPWeb.Update();
                    }
                }

            }

        }

        public void RestoreMetadataNavigationSettings()
        {
            if (base.IsSettingRestored)
            {

                using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.RestoreMetadataNavigationSettings"))
                {

                    if (mListSettingInfo != null && mListSettingInfo.RootFolderInfo != null && mListSettingInfo.RootFolderInfo.IsAvailable
                        && mListSettingInfo.RootFolderInfo.Value != null && mListSettingInfo.RootFolderInfo.Value.MetaInfoDic != null)
                    {
                        if (mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.ContainsKey("client_MOSS_MetadataNavigationSettings"))
                        {
                            try
                            {
                                string metadataNavigationSettings = mListSettingInfo.RootFolderInfo.Value.MetaInfoDic["client_MOSS_MetadataNavigationSettings"].ToString();
                                XmlDocument xDoc = new XmlDocument();
                                xDoc.InnerXml = metadataNavigationSettings;
                                List<XmlNode> deleteNodes = new List<XmlNode>();
                                #region reset view info
                                XmlNodeList nodes = xDoc.GetElementsByTagName("View");
                                foreach (XmlNode node in nodes)
                                {
                                    if (node.Attributes["ViewId"] != null)
                                    {
                                        IAveView view = null;
                                        Guid desViewId = Guid.Empty;
                                        try
                                        {
                                            Guid sourceViewId = new Guid(node.Attributes["ViewId"].Value);
                                            if (this.ParentSite.MappingManager.SiteMappingManager.GetViewGuidMappingValue(sourceViewId, out desViewId))
                                            {
                                                view = mSPList.Views[desViewId];
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetListViewByNameError, e.ToString());
                                        }
                                        if (view != null)
                                        {
                                            node.Attributes["ViewId"].Value = desViewId.ToString();
                                            if (node.Attributes["CachedName"] != null)
                                            {
                                                node.Attributes["CachedName"].Value = view.Title.ToString();
                                            }
                                            if (node.Attributes["CachedUrl"] != null)
                                            {
                                                node.Attributes["CachedUrl"].Value = view.Url;
                                            }
                                        }
                                        else
                                        {
                                            deleteNodes.Add(node);
                                        }
                                    }
                                }
                                #endregion
                                foreach (XmlNode node1 in xDoc.ChildNodes)
                                {
                                    if (node1.Name == "MetadataNavigationSettings")
                                    {
                                        foreach (XmlNode node2 in node1.ChildNodes)
                                        {
                                            if (node2.Name == "NavigationHierarchies")
                                            {
                                                foreach (XmlNode node3 in node2.ChildNodes)
                                                {
                                                    if (node3.Name == "FolderHierarchy")
                                                    {
                                                        foreach (XmlNode node4 in node3.ChildNodes)
                                                        {
                                                            if (!ResetFolderViewSetting(node4))
                                                            {
                                                                deleteNodes.Add(node4);
                                                            }
                                                        }
                                                    }
                                                    else if (node3.Name == "MetadataField")
                                                    {
                                                        //deleteNodes.Add(node3);
                                                        //continue;
                                                        string fieldType = string.Empty;
                                                        if (!ResetMetadataField(node3, out fieldType))
                                                        {
                                                            deleteNodes.Add(node3);
                                                        }
                                                        else
                                                        {
                                                            if (fieldType == "ContentTypeId")
                                                            {
                                                                foreach (XmlNode node4 in node3.ChildNodes)
                                                                {
                                                                    if (!ResetContentTypeViewSetting(node4))
                                                                    {
                                                                        deleteNodes.Add(node4);
                                                                    }
                                                                }
                                                            }
                                                            else if (fieldType == "TaxonomyFieldType")
                                                            {
                                                                foreach (XmlNode node4 in node3.ChildNodes)
                                                                {
                                                                    if (!ResetTaxonomyFieldViewSetting(node4))
                                                                    {
                                                                        deleteNodes.Add(node4);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else if (node2.Name == "KeyFilters")
                                            {
                                                foreach (XmlNode node3 in node2.ChildNodes)
                                                {
                                                    if (node3.Name == "MetadataField")
                                                    {
                                                        string fieldType = string.Empty;
                                                        if (!ResetMetadataField(node3, out fieldType))
                                                        {
                                                            node2.RemoveChild(node3);
                                                        }
                                                    }
                                                }
                                            }
                                            else if (node2.Name == "ManagedIndices")
                                            {
                                                foreach (XmlNode node3 in node2.ChildNodes)
                                                {
                                                    if (node3.Name == "ManagedIndex")
                                                    {
                                                        if (!ResetManagedIndex(node3))
                                                        {
                                                            deleteNodes.Add(node3);
                                                        }
                                                    }
                                                }
                                            }
                                            else if (node2.Name == "ViewSettings")
                                            {
                                                //nothing to do
                                            }
                                        }
                                    }
                                }
                                foreach (XmlNode node in deleteNodes)
                                {
                                    XmlNode pNode = node.ParentNode;
                                    pNode.RemoveChild(node);
                                }
                                if (AveEnv.IsMoss && this.mAveParentSite.SPContextKind != AveContextKind.ClientObjectModel)
                                {
                                    try
                                    {
                                        ResetMetaDataNavigationSetting(xDoc.InnerXml);
                                        mSPList.RootFolder.Reload();
                                    }
                                    catch (AveSecurityTrimingException)
                                    {
                                        throw;
                                    }
                                    catch (Exception e)
                                    {
                                        log.Log(AveLogLevel.WARN, "An error occurred while SetMetadataNavigationSettings.list:{0}, error:{1}", mSPList.Title, e.ToString());
                                        mSPList.RootFolder.Properties["client_MOSS_MetadataNavigationSettings"] = xDoc.InnerXml;
                                        mSPList.RootFolder.Update();
                                    }
                                }
                                else
                                {
                                    if (mSPList.RootFolder.Properties != null)
                                    {
                                        mSPList.RootFolder.Properties["client_MOSS_MetadataNavigationSettings"] = xDoc.InnerXml;
                                        mSPList.RootFolder.Update();
                                    }
                                }
                            }
                            catch (AveSecurityTrimingException ex)
                            {
                                log.Log(AveLogLevel.WARN, "An error occurred while RestoreMetadataNavigationSettings. error:{0}", ex.ToString());
                                //qlluo: Post action do not support report, remove it.
                                //reportor.AddDetail(new AveWrapperReportDto("MetadataNavigationSettings", "MetadataNavigationSettings", AveReportObjectType.MetadataNavigationSettings, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreNavigationSettings + ex.Message));
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.WARN, "An error occurred while RestoreMetadataNavigationSettings. error:{0}", e.ToString());
                                //mLog.Warn("An error occurred while RestoreMetadataNavigationSettings. error:{0}", e.ToString());
                            }
                        }
                    }

                }

            }
        }

        private bool ResetMetaDataNavigationSetting(string innerXml)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.ResetMetaDataNavigationSetting"))
            {

                bool suc = false;
                IAveOMetadataNavigationSettings setting = mAveParentSite.ObjectModelFactory.CreateMetadataNavigationSettings(innerXml);
                setting.SetMetadataNavigationSettings(mSPList, setting);
                suc = true;
                return suc;

            }

        }

        private bool ResetMetadataField(XmlNode node, out string fieldType)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.ResetMetadataField"))
            {

                fieldType = string.Empty;
                if (node.Attributes["CachedName"] != null)
                {
                    string fieldInternalName = node.Attributes["CachedName"].Value;
                    IAveField field = null;
                    try
                    {
                        field = mSPList.Fields.GetFieldByInternalName(fieldInternalName);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFieldError, e.ToString());
                    }
                    if (field != null)
                    {
                        if (node.Attributes["FieldID"] != null)
                        {
                            node.Attributes["FieldID"].Value = field.ID.ToString();
                        }
                        if (node.Attributes["FieldType"] != null)
                        {
                            if (field.TypeAsString == "Computed")
                            {
                                node.Attributes["FieldType"].Value = "ContentTypeId";
                            }
                            else
                            {
                                node.Attributes["FieldType"].Value = field.TypeAsString;
                            }
                            fieldType = node.Attributes["FieldType"].Value;
                        }
                        //if (node.Attributes["CachedDisplayName"] != null)
                        //{
                        //    node.Attributes["CachedDisplayName"].Value = field.InternalName;
                        //}
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;

            }

        }

        private bool ResetManagedIndex(XmlNode node)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.ResetManagedIndex"))
            {

                if (node.Attributes["IndexFieldName"] != null)
                {
                    string fieldName = node.Attributes["IndexFieldName"].Value;
                    IAveField field = null;
                    try
                    {
                        field = mSPList.Fields.GetField(fieldName);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFieldError, e.ToString());
                    }
                    if (field != null)
                    {
                        if (node.Attributes["IndexFieldID"] != null)
                        {
                            node.Attributes["IndexFieldID"].Value = field.ID.ToString();
                        }
                        if (node.Attributes["IndexFieldNameSecondary"] != null)
                        {
                            try
                            {
                                fieldName = node.Attributes["IndexFieldNameSecondary"].Value;
                                field = mSPList.Fields.GetField(fieldName);
                                if (node.Attributes["IndexFieldIDSecondary"] != null)
                                {
                                    node.Attributes["IndexFieldIDSecondary"].Value = field.ID.ToString();
                                }
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetAttributeFailed, e.ToString());
                                return false;
                            }
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;

            }

        }

        private bool ResetFolderViewSetting(XmlNode node)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.ResetFolderViewSetting"))
            {

                if (node.Name == "ViewSettings")
                {
                    if (node.Attributes["FolderId"] != null)
                    {
                        int folderId = 0;
                        int.TryParse(node.Attributes["FolderId"].Value, out folderId);
                        if (folderId > 0)
                        {
                            int newFolderId = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.GetMappingItemId(mId, folderId);
                            if (newFolderId != -1)
                            {
                                node.Attributes["FolderId"].Value = newFolderId.ToString();
                                if (node.Attributes["UniqueNodeId"] != null)
                                {
                                    IAveListItem item = mSPList.GetItemById(newFolderId);
                                    node.Attributes["UniqueNodeId"].Value = item.UniqueId.ToString();
                                }
                            }
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return false;

            }

        }

        //目前此方法只还原contentType根节点的view setting，即node.Attributes["UniqueNodeId"].Value=""的情况，
        //原因是在调试还原其下节点的view setting时，导致目的端list的Per-location view settings设置页面打开出错。 
        private bool ResetContentTypeViewSetting(XmlNode node)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.ResetContentTypeViewSetting"))
            {

                if (node.Name == "ViewSettings")
                {
                    if (node.Attributes["UniqueNodeId"] != null)
                    {
                        string contentTypeId = node.Attributes["UniqueNodeId"].Value;
                        if (string.IsNullOrEmpty(contentTypeId))
                        {
                            return true;
                        }
                        if (ParentSite.MappingManager.ListMappingManager.ListLevelCTIdMapping.ContainsKey(contentTypeId))
                        {
                            string newContentTyopeId = ParentSite.MappingManager.ListMappingManager.ListLevelCTIdMapping[contentTypeId].ToString();
                            node.Attributes["UniqueNodeId"].Value = newContentTyopeId;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return false;

            }

        }

        //目前此方法只还原metadata field根节点的view setting，即node.Attributes["UniqueNodeId"].Value=""的情况
        private bool ResetTaxonomyFieldViewSetting(XmlNode node)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.ResetTaxonomyFieldViewSetting"))
            {

                //wait to do
                if (node.Name == "ViewSettings")
                {
                    if (node.Attributes["UniqueNodeId"] != null && string.IsNullOrEmpty(node.Attributes["UniqueNodeId"].Value))
                    {
                        return true;
                    }
                }
                return false;

            }

        }
        //alert 处理统一放在AveSPAlert类中处理
        //private void StopListAlerts()
        //{
        //    if (!StopAlerts || mSPList == null)
        //    {
        //        return;
        //    }
        //    try
        //    {
        //        Guid listId = mSPList.ID;
        //        List<Guid> tmpAlertIds = new List<Guid>();
        //        IAveAlertCollection webAlerts = mAveSPWeb.SPWeb.Alerts;
        //        if (webAlerts != null)
        //        {
        //            foreach (IAveAlert alert in mAveSPWeb.SPWeb.Alerts)
        //            {
        //                if (alert.ListID != null && alert.Status == AveAlertStatus.On && alert.ListID == listId)
        //                {
        //                    tmpAlertIds.Add(alert.ID);
        //                }
        //            }
        //            foreach (Guid alertId in tmpAlertIds)
        //            {
        //                IAveAlert alert = mAveSPWeb.SPWeb.Alerts[alertId];
        //                alert.Status = AveAlertStatus.Off;
        //                alert.Update(false);
        //            }
        //            if (mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.NeedEnableAlerts.ContainsKey(mAveSPWeb.SPWeb.ID))
        //            {
        //                mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.NeedEnableAlerts[mAveSPWeb.SPWeb.ID].AddRange(tmpAlertIds);
        //            }
        //            else
        //            {
        //                mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.NeedEnableAlerts.Add(mAveSPWeb.SPWeb.ID, tmpAlertIds);
        //            }
        //            //mListAlertIDs.AddRange(tmpAlertIds);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Warn("Stop List Alert exception: " + ex.ToString());
        //    }
        //}
        //private void EnableListAlerts()
        //{
        //    if (StopAlerts && mListAlertIDs.Count > 0)
        //    {
        //        try
        //        {
        //            if (this.ParentSite.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel)
        //            {
        //                string sqlCmd = "delete from eventcache where SiteId = @SiteId and WebId = @WebId and ListId = @ListId and EventData is not null and ACL is not null";
        //                mSqlConn.ClearParameters();
        //                mSqlConn.AddParameter("@SiteId", mAveSPWeb.SPWeb.Site.ID);
        //                mSqlConn.AddParameter("@WebId", mAveSPWeb.SPWeb.ID);
        //                mSqlConn.AddParameter("@ListId", mId);
        //                mSqlConn.ExecuteNonQuery(sqlCmd);
        //            }
        //            mAveSPWeb.ReloadWeb();
        //            foreach (Guid alertId in mListAlertIDs)
        //            {
        //                try
        //                {
        //                    IAveAlert alert = mAveSPWeb.SPWeb.Alerts[alertId];
        //                    alert.Status = AveAlertStatus.On;
        //                    SetAlertViewID(mAveSPWeb.SPWeb.Alerts[alertId]);//DOC-65976
        //                    alert.Update(false);
        //                }
        //                catch
        //                { }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            mLog.Warn("Enable List Alert exception: " + ex.ToString());
        //        }
        //    }
        //}

        //private void SetAlertViewID(IAveAlert alert)
        //{
        //    try
        //    {
        //        if (alert.Properties != null && alert.Properties.ContainsKey("viewid"))
        //        {
        //            Guid viewID = new Guid(alert.Properties["viewid"]);
        //            if (mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.ViewGuidMapping.ContainsKey(viewID))
        //            {
        //                alert.Properties["viewid"] = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.ViewGuidMapping[viewID].ToString();
        //                alert.Properties.Update();
        //                alert.Update();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLog.Error("Restore ViewID of Alert.Error:" + ex.ToString());
        //    }
        //}

        //after create Categories List, we need delete the first 3 items for MySite. DOC-52153 
        public void DeleteItemsForCategory()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.DeleteItemsForCategory"))
            {

                if (SPList != null && SPList.BaseTemplate == AveListTemplateType.Categories && SPList.BaseType == AveBaseType.GenericList
                    && mAveSPWeb.ParentSite.SPSite.RootWeb.WebTemplate.StartsWith("SPSPERS", StringComparison.OrdinalIgnoreCase))
                {
                    bool needUpdate = false;
                    for (int i = 1; i <= 3; i++)
                    {
                        IAveListItem item = SPList.GetItemById(i);
                        if (item != null)
                        {
                            needUpdate = true;
                            item.Delete();
                        }
                    }
                    if (needUpdate)
                    {
                        SPList.Update();
                    }
                }

            }

        }


        public void EnableListVersioning(AveVersionMode versionMode)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.EnableListVersioning"))
            {

                bool changed = false;
                switch (versionMode)
                {
                    case AveVersionMode.None:
                        if (mSPList.EnableVersioning)
                        {
                            mSPList.EnableVersioning = false;
                            changed = true;
                        }
                        break;
                    case AveVersionMode.MajorVersion:
                        if (!mSPList.EnableVersioning || mSPList.EnableMinorVersions)
                        {
                            mSPList.EnableVersioning = true;
                            mSPList.EnableMinorVersions = false;
                            changed = true;
                        }
                        break;
                    case AveVersionMode.MinorVersion:
                        if (!mSPList.EnableVersioning || !mSPList.EnableMinorVersions)
                        {
                            mSPList.EnableVersioning = true;
                            mSPList.EnableMinorVersions = true;
                            changed = true;
                        }
                        break;
                    default:
                        break;
                }
                if (changed)
                {
                    mSPList.Update();
                }

            }

        }

        internal void UpdateMicroFeedItem()
        {
            if (PostMicroFeedItem == null || PostMicroFeedItem.Count == 0)
            {
                return;
            }
            try
            {
                foreach (int itemOriginalId in PostMicroFeedItem)
                {
                    int itemId = ParentSite.MappingManager.SiteMappingManager.GetMappingItemId(mSPList.ID, itemOriginalId);
                    if (itemId != -1)
                    {
                        var postItem = mSPList.GetItemById(itemId);
                        var refRoot = postItem["RefRoot"];
                        bool needUpdate = false;
                        if (refRoot != null)
                        {
                            refRoot = ReplaceMFPXml(refRoot.ToString());
                            postItem["RefRoot"] = refRoot;
                            needUpdate = true;
                        }
                        var refReply = postItem["RefReply"];
                        if (refReply != null)
                        {
                            refReply = ReplaceMFPXml(refReply.ToString());
                            postItem["RefReply"] = refReply;
                            needUpdate = true;
                        }
                        if (needUpdate)
                        {
                            postItem.SystemUpdate(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, "An exception occurred while do update MicroFeed Item. exception:{0}", ex.ToString());
            }
        }

        internal static void PostUpdateSocialItems(List<int> socialItems, IAveList parentList, IAveWeb parentWeb, AveSiteMappingManager siteMappingManager)
        {
            if (socialItems == null || socialItems.Count == 0 || parentList == null || parentWeb == null || siteMappingManager == null)
            {
                return;
            }

            foreach (int itemOriginalId in socialItems)
            {

                int itemId = siteMappingManager.GetMappingItemId(parentList.ID, itemOriginalId);
                try
                {
                    if (itemId != -1)
                    {
                        log.Debug("Post update social item WebId:{0},ListId:{1},itemId:{2}", parentWeb.ID,
                            parentList.ID, itemId);
                        var socialItem = parentList.GetItemById(itemId);
                        bool needUpdate = false;
                        //siteMappingManager.SourceSiteInfo.Id是66添加的，为了兼容老数据，需要做下处理
                        if (siteMappingManager.SourceSiteInfo.Id != Guid.Empty)
                        {
                            if (SetGuidValueByMapping(new Dictionary<Guid, Guid> { { siteMappingManager.SourceSiteInfo.Id, parentWeb.Site.ID } }, socialItem, "SiteId"))
                            {
                                needUpdate = true;
                            }
                        }
                        else
                        {
                            //兼容老数据,如果是原端site内的url就做替换
                            string url = Convert.ToString(socialItem["Url"]);
                            if (!string.IsNullOrEmpty(url) && url.StartsWith(siteMappingManager.SourceSiteInfo.Url, StringComparison.OrdinalIgnoreCase))
                            {
                                socialItem["SiteId"] = parentWeb.Site.ID;
                                needUpdate = true;
                            }
                        }
                        if (SetGuidValueByMapping(siteMappingManager.WebIDMapping, socialItem, "WebId"))
                        {
                            needUpdate = true;
                        }
                        if (SetGuidValueByMapping(siteMappingManager.WebIDMapping, socialItem, "ListId"))
                        {
                            needUpdate = true;
                        }
                        if (SetTextValueByReplaceUrl(siteMappingManager, socialItem, "Url"))
                        {
                            needUpdate = true;
                        }
                        if (needUpdate)
                        {
                            socialItem.SystemUpdate(false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do update MicroFeed Item {0}. exception:{1}", itemId, ex);
                }
            }
        }

        /// <summary>
        /// 根据mapping替换Guid field value
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="item"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private static bool SetGuidValueByMapping(IDictionary<Guid, Guid> mapping, IAveListItem item, string fieldName)
        {
            bool changed = false;
            try
            {
                Guid originalId = (Guid)item[fieldName];
                Guid newId;
                if (mapping.TryGetValue(originalId, out newId))
                {
                    item[fieldName] = newId;
                    changed = true;
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while getting mapped guid,fieldName:{0},ItemId:{1},Error:{2}", fieldName, item.ID, e);
            }
            return changed;
        }

        /// <summary>
        /// 替换内部绝对url
        /// </summary>
        /// <param name="mappingManager"></param>
        /// <param name="item"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private static bool SetTextValueByReplaceUrl(AveSiteMappingManager mappingManager, IAveListItem item, string fieldName)
        {
            bool changed = false;
            try
            {
                string url = Convert.ToString(item[fieldName]);
                //只替换内部url
                if (!string.IsNullOrEmpty(url) && url.StartsWith(mappingManager.SourceSiteInfo.Url, StringComparison.OrdinalIgnoreCase))
                {
                    string newUrl = AveReplaceProcessor.UrlReplace(url, mappingManager.SiteManagedMappings, new ReplaceOption(true, true, true), mappingManager.SourceSiteInfo, mappingManager.DestSiteInfo.ServerRelativeUrl);
                    item["Url"] = newUrl;
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while getting mapped guid,fieldName:{0},ItemId:{1},Error:{2}", fieldName, item.ID, e);
            }
            return changed;
        }

        private string ReplaceMFPXml(string xml)
        {
            if (xml.StartsWith("<MFP", StringComparison.OrdinalIgnoreCase))
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(xml);
                if (xDoc.GetElementsByTagName("c").Count == 1)
                {
                    XmlNode cNode = xDoc.GetElementsByTagName("c")[0];
                    int refRootItemId = Int32.Parse(cNode.InnerText);
                    int tempRootItemId = ParentSite.MappingManager.SiteMappingManager.GetMappingItemId(mSPList.ID, refRootItemId);
                    if (tempRootItemId != -1)
                    {
                        refRootItemId = ParentSite.MappingManager.SiteMappingManager.GetMappingItemId(mSPList.ID, refRootItemId);
                        cNode.InnerText = refRootItemId.ToString();
                    }
                    var refRootItem = mSPList.GetItemById(refRootItemId);
                    if (xDoc.GetElementsByTagName("d").Count == 1)
                    {
                        xDoc.GetElementsByTagName("d")[0].InnerText = refRootItem.UniqueId.ToString();
                    }
                    if (xDoc.GetElementsByTagName("e").Count == 1)
                    {
                        xDoc.GetElementsByTagName("e")[0].InnerText = refRootItem["RootPostID"].ToString();
                    }
                    if (xDoc.GetElementsByTagName("f").Count == 1)
                    {
                        xDoc.GetElementsByTagName("f")[0].InnerText = refRootItem["RootPostUniqueID"].ToString();
                    }
                    if (xDoc.GetElementsByTagName("p").Count == 1)
                    {
                        xDoc.GetElementsByTagName("p")[0].InnerText = refRootItem["RootPostOwnerID"].ToString();
                    }
                }
                return xDoc.OuterXml;
            }
            return xml;
        }

        #region IDisposable Members

        internal bool isPosted = false;

        public void Dispose()
        {
            new AveSPListPostAction(this).Excute();

            if (mFields != null)
            {
                mFields.Dispose();
            }
            if (mSecurity != null)
            {
                mSecurity.Dispose();
            }
            if (mContentTypes != null)
            {
                mContentTypes.Dispose();
            }
            if (reportor != null)
            {
                reportor.Dispose();
            }
            if (mSPList != null)
            {
                mSPList.CleanListData();
            }
            if (this.elevatedWeb != null)
            {
                elevatedWeb.Dispose();
                elevatedWeb = null;
            }
            if (this.elevatedSite != null)
            {
                elevatedSite.Dispose();
                elevatedSite = null;
            }
            if (this.needsElevation)
            {
                this.ParentWeb.ReloadWeb();
                this.needsElevation = false;
            }
            if (lookupItemUniqueIdCache != null && lookupItemUniqueIdCache.Count > 0)
            {
                lookupItemUniqueIdCache = new Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, Dictionary<int, Guid>>>>>();
            }
        }

        #endregion

        [Obsolete("restore rss view by common restore view logic")]
        internal void RestoreListRssViewField()
        {
            using (new AvePerformanceScope("Restore.AveSPList.RestoreListRssViewField"))
            {
                //已经还原过或者ClientObjectModel不走此方法
                if (RestoreRssView || mAveParentSite.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
                {
                    return;//rss view 已经在webpart上进行过还原处理，就不必在此处重复处理
                }
                IAveView rssView = null;
                try
                {
                    if (mSPList != null)
                    {
                        //mSPList.EnsureRssSettings();//ensure rsssetting
                        if (!AveSPListUtility.IsViewExist(mSPList, "RssView"))
                        {
                            AveSPListUtility.EnsureRssView(mSPList);
                        }
                        rssView = mSPList.Views["RssView"];
                        if (!rssView.Hidden)
                        {
                            rssView.Hidden = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetRssViewFailed, e);
                }
                if (rssView == null)//源端为默认或者目的端为默认的情况下我们不会处理
                    return;
                IAveViewFieldCollection fieldCollection = rssView.ViewFields;//restore destination view field
                rssView.ViewFields.RemoveAll();
                try
                {
                    if (!string.IsNullOrEmpty(mRssViewFieldXml))
                    {
                        mRssViewFieldXml = "<Field>" + mRssViewFieldXml + "</Field>";
                        XmlDocument rssDoc = new XmlDocument();
                        rssDoc.PreserveWhitespace = true;
                        rssDoc.LoadXml(mRssViewFieldXml);
                        IAveFieldCollection fieldC = mSPList.Fields;
                        foreach (XmlNode fieldNode in rssDoc.GetElementsByTagName("FieldRef"))
                        {
                            XmlElement node = fieldNode as XmlElement;
                            string fieldName = node.GetAttribute("Name");
                            string realFieldName = ParentWeb.ParentSite.GetNameByLanguageMapping(fieldName, AveLanguageMappingType.FieldMapping);
                            try
                            {
                                foreach (IAveField field in fieldC)
                                {
                                    if (field.InternalName.Equals(realFieldName))
                                    {
                                        rssView.ViewFields.Add(field);
                                        break;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.AddRssViewFieldError, e);
                            }
                        }
                    }
                    rssView.Update();
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, "Restore Rss View Field Failed . list title:{0}, Exception:{1}", mSPList.Title, ex.ToString());
                    //qlluo: Post action do not support report, remove it.
                    //reportor.AddDetail(new AveWrapperReportDto("ListRssViewField", "ListRssViewField", AveReportObjectType.ListRssViewField, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreListRssViewField + ex.Message));
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "Restore Rss View Field Failed . list title:{0}, Exception:{1}", mSPList.Title, e.ToString());
                    rssView.ViewFields.RemoveAll();//delete all failed field in view
                    foreach (string fieldStr in fieldCollection)
                    {
                        rssView.ViewFields.Add(fieldStr);//roll back
                    }
                    rssView.Update();
                }
            }
        }

        public void UpdateDefaultValue()
        {
            using (new AvePerformanceScope("Restore.AveSPList.UpdateDefaultValue"))
            {
                if (this.SPList == null)
                {
                    return;
                }
                try
                {
                    IAveFile spFile = SPList.ParentWeb.GetFile(SPList.RootFolder.ServerRelativeUrl + "/Forms/client_LocationBasedDefaults.html");
                    if (spFile != null && spFile.Exists)
                    {
                        IAveFolder spFolder = null;
                        XmlDocument xDoc = new XmlDocument();
                        xDoc.PreserveWhitespace = true;
                        bool changed = false;
                        string oldListRelativeUrl = mListInfo.ServerRelativeUrl;
                        string startStr = SPList.RootFolder.ServerRelativeUrl;
                        string oldLinkUrl = String.Empty;
                        oldListRelativeUrl = AveHttpUtility.UrlPathEncode(oldListRelativeUrl, true, false);
                        startStr = AveHttpUtility.UrlPathEncode(startStr, true, false);
                        xDoc.LoadXml(new UTF8Encoding().GetString(spFile.OpenBinary()));
                        foreach (XmlNode node in xDoc.DocumentElement.SelectNodes("a"))
                        {
                            XmlElement temp = (XmlElement)node;
                            try
                            {
                                oldLinkUrl = temp.GetAttribute("href");
                                if (!oldLinkUrl.StartsWith(startStr, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (oldLinkUrl.StartsWith(oldListRelativeUrl, StringComparison.OrdinalIgnoreCase))
                                    {
                                        temp.SetAttribute("href", startStr + oldLinkUrl.Substring(oldListRelativeUrl.Length));
                                        changed = true;
                                    }
                                }
                                spFolder = SPList.ParentWeb.GetFolder(temp.GetAttribute("href"));
                                if (spFolder == null || !spFolder.Exists)
                                {
                                    xDoc.RemoveChild(node);
                                    changed = true;
                                }
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetAttributeFailed, e);
                            }
                            foreach (XmlNode field in node.ChildNodes)
                            {
                                if (field.Name.Equals("DefaultValue"))
                                {
                                    string mappingValue = AveFields.FieldMapping.GetMappingRestoredFieldInternalName(field.Attributes["FieldName"].Value);
                                    if (!String.IsNullOrEmpty(mappingValue))
                                    {
                                        field.Attributes["FieldName"].Value = mappingValue;
                                        changed = true;
                                    }

                                    try
                                    {
                                        string fieldValue = field.InnerText;
                                        IAveField mField = SPList.Fields.GetFieldByInternalName(field.Attributes["FieldName"].Value);
                                        if (mField != null && (mField.TypeAsString.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) || mField.TypeAsString.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase)))
                                        {
                                            bool valueSettingChange = false;
                                            string[] valuenum = fieldValue.Split('#');
                                            if (valuenum.Length > 1)
                                            {
                                                for (int i = 0; i < valuenum.Length; i++)
                                                {
                                                    Guid termId = Guid.Empty;
                                                    string valueString1 = valuenum[i];
                                                    if (valueString1.Contains("|"))
                                                    {
                                                        if (valueString1.EndsWith(";", StringComparison.Ordinal))
                                                        {
                                                            valueString1 = valueString1.Remove(valueString1.Length - 1);
                                                        }
                                                        string[] temps = valueString1.Split('|');
                                                        if (temps.Length == 2)
                                                        {
                                                            termId = new Guid(temps[1]);
                                                        }
                                                        if (termId != Guid.Empty && ParentSite.MetadataService != null && ParentSite.MetadataService.TermIdMapping.ContainsKey(termId))
                                                        {
                                                            fieldValue = fieldValue.Replace(termId.ToString(), ParentSite.MetadataService.TermIdMapping[termId].ToString());
                                                            field.InnerText = fieldValue;
                                                            changed = true;
                                                            valueSettingChange = true;
                                                            continue;
                                                        }
                                                    }
                                                }
                                            }
                                            if (!valueSettingChange)
                                            {
                                                log.Log(AveLogLevel.DEBUG, "No metadata column default value setting has been changed. Field name:{0}. Value:{1}", mField.InternalName, fieldValue);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Log(AveLogLevel.WARN, "Replace metadata column default value failed. Error:{0}", ex.ToString());
                                    }
                                }
                            }
                        }
                        if (changed)
                        {
                            spFile.SaveBinary(Encoding.UTF8.GetBytes(xDoc.OuterXml));
                            spFile.Update();
                        }

                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("Update the file: {0} failed in ListPostAction. Exception: {1}", SPList.RootFolder.ServerRelativeUrl + "/Forms/client_LocationBasedDefaults.html", ex);
                    //qlluo: Post action do not support report, remove it.
                    //reportor.AddDetail(new AveWrapperReportDto("ListDefaultValue", "ListDefaultValue", AveReportObjectType.ListDefaultValue, AveStatus.Skipped, WrapperReportResource.Wrapper_Report_NoPermissionToRestoreListDefaultView + ex.Message));
                }
                catch (Exception ex)
                {
                    log.Warn("Update the file: {0} failed in ListPostAction. Exception: {1}", SPList.RootFolder.ServerRelativeUrl + "/Forms/client_LocationBasedDefaults.html", ex);
                }
            }
        }

        public void UpdateDefaultView()
        {
            try
            {
                if (mNeedUpdateToDefaultView != null && mNeedUpdateToDefaultView.Count > 0)
                {
                    SPList.ParentWeb.ReloadWeb();
                    SPList.Reload();
                    foreach (String fieldName in mNeedUpdateToDefaultView)
                    {
                        this.SPList.DefaultView.ViewFields.Add(fieldName);
                    }
                    this.SPList.DefaultView.Update();
                }
            }
            catch (Exception ex)
            {
                log.Warn("Update default view: {0} with exception in ListPostAction. Exception: {1}", this.SPList.DefaultView.Title, ex);
            }
        }
        public void UpdateSpotlightViews()
        {
            if (!this.ParentSite.SPSite.IsOnlineSite && this.ParentSite.SPContextKind != AveContextKind.Server19ObjectModel)
            {
                return;
            }
            foreach (var viewInfo in mNeedUpdateSpotlightViews)
            {
                var spotlightString = GetOneViewSpotlightString(viewInfo.Value);
                if (!string.IsNullOrEmpty(spotlightString))
                {
                    try
                    {
                        var view = this.mSPList.Views[viewInfo.Key];
                        view.ListViewXml = string.Format("<SpotlightInfo>{0}</SpotlightInfo>", spotlightString);
                        view.Update();
                    }
                    catch (Exception e)
                    {
                        log.Error("Update view spotlight failed. View: {0}, Error: {1}", viewInfo.Value.Title, e);
                    }
                }
            }
        }
        private string GetOneViewSpotlightString(AveViewInfo viewInfo)
        {
            try
            {
                XmlDocument xd = new XmlDocument();
                xd.LoadXml(viewInfo.ListViewXml);
                XmlNode spotlightInfoNode = xd.SelectSingleNode("View/SpotlightInfo");
                if (!string.IsNullOrEmpty(spotlightInfoNode.InnerText))
                {
                    var spotLightString = spotlightInfoNode.InnerText;
                    var spotlightUnits = new List<string>();
                    foreach (var folderUnit in spotLightString.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        StringBuilder newSpotlightString = new StringBuilder();
                        var itemsUnit = folderUnit.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                        if (itemsUnit.Length != 2)
                        {
                            log.Warn("Invald format: {0}", folderUnit);
                            continue;
                        }
                        var parentFolderId = GetSpotlightItemId(itemsUnit[0], viewInfo);
                        if (parentFolderId == -1)
                        {
                            log.Warn("Can not found base folder. String: {0}", folderUnit);
                            continue;
                        }
                        foreach (var itemRowid in itemsUnit[1].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            var id = GetSpotlightItemId(itemRowid, viewInfo);
                            if (id > 0)
                            {
                                if (newSpotlightString.Length == 0)
                                {
                                    newSpotlightString.Append(string.Format("{0}=", parentFolderId));
                                }
                                newSpotlightString.Append(string.Format("{0};", id));
                            }
                        }
                        if (newSpotlightString.Length > 0)
                        {
                            newSpotlightString.Length -= 1;
                            spotlightUnits.Add(newSpotlightString.ToString());
                        }
                    }
                    if (spotlightUnits.Count > 0)
                    {
                        return string.Format("|{0}|", string.Join("|", spotlightUnits.ToArray()));
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("Get spotlight failed. View Title: {0}, Error: {1}", viewInfo.Title, e);
            }
            return string.Empty;
        }
        private int GetSpotlightItemId(string rowId, AveViewInfo viewInfo)
        {
            int id;
            if (int.TryParse(rowId, out id))
            {
                if (id == 0)
                {
                    return 0;
                }
                else
                {
                    try
                    {
                        var newId = this.ParentSite.MappingManager.SiteMappingManager.GetMappingItemId(this.SPList.ID, id);
                        if (newId == -1 && viewInfo.MappingForSpotlight.ContainsKey(id))
                        {
                            var mapping = viewInfo.MappingForSpotlight[id];
                            var sourceUrl = mapping[1];
                            var siteMappingManager = ParentSite.MappingManager.SiteMappingManager;
                            var newUrl = AveReplaceProcessor.UrlReplace(sourceUrl,
                                siteMappingManager.SiteManagedMappings,
                                new ReplaceOption(true, true),
                                siteMappingManager.SourceSiteInfo,
                                siteMappingManager.DestSiteInfo.ServerRelativeUrl);
                            switch ((AveFileSystemObjectType)int.Parse(mapping[0]))
                            {
                                case AveFileSystemObjectType.File:
                                    var file = ParentWeb.SPWeb.GetFile(newUrl);
                                    if (file.Exists)
                                    {
                                        return file.Item.ID;
                                    }
                                    break;
                                case AveFileSystemObjectType.Folder:
                                    var folder = ParentWeb.SPWeb.GetFolder(newUrl);
                                    if (folder.Exists)
                                    {
                                        return folder.Item.ID;
                                    }
                                    break;
                            }
                        }
                        else if (newId > 0)
                        {
                            return newId;
                        }
                        else
                        {
                            log.Warn("This item not exist in source environment. Item Id: {0}", id);
                            return -1;
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error("Get item for spotlight failed. Id: {0}, Error: {1}", rowId, e);
                    }
                }
            }
            return -1;
        }

        internal void RestoreDocumentSetMetaInfo()
        {
            using (new AvePerformanceScope("Restore.AveSPList.RestoreDocumentSetMetaInfo"))
            {
                try
                {
                    Dictionary<int, int> documentIdInDocumentSet = new Dictionary<int, int>();
                    foreach (var keyValuePair in ParentSite.MappingManager.ListMappingManager.DocumentSetGuidMetaInfoMapping)
                    {
                        if (documentIdInDocumentSet.Count > 0)
                        {
                            documentIdInDocumentSet.Clear();
                        }
                        var documentSet = mSPList.Folders[keyValuePair.Key];

                        XmlDocument snapShot = new XmlDocument();
                        snapShot.PreserveWhitespace = true;//sharepoint use xmlreader to analyze this, which won't ignore white space node.ADO-8150
                        snapShot.LoadXml(keyValuePair.Value.Replace("\\r\\n", "\r\n").Replace(@"\\", @"\"));

                        foreach (XmlElement tmp in snapShot.SelectNodes("/SnapshotCollection/Items/Item").OfType<XmlElement>())
                        {
                            try
                            {
                                string url = tmp.GetAttribute("Url");
                                int oldId = int.Parse(tmp.GetAttribute("Id"));
                                var file = documentSet.ParentList.ParentWeb.GetFile(documentSet.Folder.ServerRelativeUrl + "/" + url);
                                if (file.Exists)
                                {
                                    if (!documentIdInDocumentSet.ContainsValue(file.Item.ID))
                                    {
                                        tmp.SetAttribute("Id", file.Item.ID.ToString());
                                        tmp.SetAttribute("Guid", file.UniqueId.ToString());
                                    }

                                    documentIdInDocumentSet[oldId] = file.Item.ID;
                                }
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetFileMetaInfoFailed, e);
                            }
                        }

                        foreach (XmlElement snapshot in snapShot.SelectNodes("/SnapshotCollection/Snapshots/Snapshot").OfType<XmlElement>())
                        {
                            try
                            {
                                foreach (XmlElement item in snapshot.SelectNodes("SnapshotItems/SnapshotItem").OfType<XmlElement>())
                                {
                                    int oldId = int.Parse(item.GetAttribute("Id"));
                                    if (documentIdInDocumentSet.ContainsKey(oldId))
                                    {
                                        item.SetAttribute("Id", documentIdInDocumentSet[oldId].ToString());
                                    }
                                    else
                                    {
                                        //
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetFileMetaInfoFailed, e);
                            }
                        }
                        documentSet.Properties["snapshots"] = snapShot.OuterXml;
                        documentSet.SystemUpdate(false);
                    }
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "Failed to restore document set MetaInfo. List Title:{0}. Error:{1}", mSPList.Title, ex.ToString());
                }
            }
        }

        internal void RemoveTempMasterPage()
        {
            try
            {
                if (tempMasterSettings.Count > 0)
                {
                    foreach (var temp in tempMasterSettings.Keys)
                    {
                        string tempFileUrl = temp;
                        IAveFile tempFile = this.ParentWeb.SPWeb.GetFile(tempFileUrl);
                        if (!tempFile.Exists)
                        {
                            continue;
                        }
                        List<AveExtendMasterPageInfo> settingInfos = tempMasterSettings[tempFileUrl];
                        foreach (var setting in settingInfos)
                        {
                            using (IAveWeb web = this.ParentSite.SPSite.OpenWeb(setting.CurrentWebId))
                            {
                                this.ParentSite.Publishing.SetWebMasterPageInfo(setting, web, web.AlternateCssUrl, !mAveParentSite.NotRestoreWebCss);
                            }
                        }
                        try
                        {
                            tempFile.Delete();
                        }
                        catch (Exception ex)
                        {
                            ParentSite.TempMasterPages.Add(string.Format("{0}:{1}", this.ParentWeb.SPWeb.ID.ToString(), tempFileUrl));
                            log.Log(AveLogLevel.WARN, "cannot delete temp master page :{0}, exception:{1}", tempFile.Url, ex.ToString()); ;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, "an exception occurred while do remove temp master page. exception:{0}", ex.ToString());
            }
        }

        /// <summary>
        /// ADO-9496
        /// Calendar list的ResourceSelector属性开启之后，还需要将list下的name为Facilities的column指向站点唯一的Facility类型的list
        /// 需要认证下其他的list是否有该属性为true的情况，还要看下Facility是否是唯一的
        /// 注意field的关系可以在field还原中处理，目前wrapper对xml为Facilities，API类型为Invalid类型的field没有处理导致了这个问题
        /// 该field在该case上可以作为lookup类型field处理，测试没有问题。所以等到field结构完善后，可以去掉该部分field的赋值。
        /// </summary>
        /// <param name="listSettingInfo"></param>
        private void EnsureListResourceSelector(AveListSettingInfo listSettingInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.EnsureListResourceSelector"))
            {

                mSPList.EnableResourceSelector = listSettingInfo.EnableResourceSelector.Value;

                #region 支持Fecilities类型field还原之后可以将这部分代码去掉
                if (listSettingInfo.EnableResourceSelector.Value)
                {
                    //set lookup field value as 
                    IAveList resourceList = null;
                    foreach (IAveList list in mSPList.ParentWeb.Lists)
                    {
                        if (list.BaseTemplate == AveListTemplateType.Facility)
                        {
                            resourceList = list;
                            break;
                        }
                    }
                    if (resourceList != null)
                    {
                        try
                        {
                            IAveFieldLookup fieldByInternalName = mSPList.Fields.GetFieldByInternalName("Facilities", false) as IAveFieldLookup;
                            if (fieldByInternalName != null)
                            {
                                fieldByInternalName.LookupList = resourceList.ID.ToString("B").ToUpper(CultureInfo.InvariantCulture);
                                fieldByInternalName.Update();
                            }
                            else
                            {
                                log.Info("Can not get relative field in current list. list title:{0}, filed internal name:{1}", mSPList.Title, "Facilities");
                            }
                        }
                        catch (Exception e)
                        {
                            log.Error("An error occurred while setting listResourceSelector attribute, list title:{0}, error information:{1}", mSPList.ID, e.ToString());
                        }
                    }

                }
                #endregion

            }

        }

        public void RestoreAlerts(List<Dictionary<string, object>> alertInfos, bool isSchedAlert)
        {
            foreach (Dictionary<string, object> sAlertInfo in alertInfos)
            {
                AveSPAlert alert = AveSPAlert.CreateInstance(this);
                alert.RestoreAlert(sAlertInfo, isSchedAlert);
            }
        }

        public void RestoreUnRestoreAlerts()
        {
            if (mAlertInfos.Count > 0)
            {
                foreach (Dictionary<string, object> alertInfo in mAlertInfos)
                {
                    AveSPAlert alert = AveSPAlert.CreateInstance(this);
                    alert.RestoreAlert(alertInfo, true);
                }
                mAlertInfos.Clear();
            }
        }

        //由于365 API limitation，SandBox feature放到ListPostAction里激活。
        public void ActiveSandBoxFeature()
        {
            try
            {
                if (this.AveList != null && this.AveList.BaseTemplate == AveListTemplateType.SolutionCatalog && this.ParentSite.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
                {
                    foreach (Guid featureId in this.mAveSPWeb.ActivatedWebFeatureIDs)
                    {
                        try
                        {
                            //ADO-180177:在覆盖还原Solution Gallery中solution时，关联的web feature会被重新置为deactive状态，365 Features集合需Reload。
                            mAveSPWeb.SPWeb.ReloadFeatures();
                            if (mAveSPWeb.SPWeb.Features[featureId] == null)
                            {
                                mAveSPWeb.SPWeb.Features.Add(featureId, false, AveFeatureDefinitionScope.Site);
                            }
                        }
                        catch (Exception e)
                        {
                            log.Warn("Failed to active this feature. Id: {0}, Error: {1}", featureId, e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn("Failed to active site features. Error: {0}", e);
            }
        }

        public void UpdateTaskEventReceiverSynchronous(bool isSync)
        {
            if (!mAveParentSite.DisableTaskSynchronous)
            {
                log.Info("Option DisableTaskSynchronous is true,so will return");
                return;
            }
            if ((int)this.SPList.BaseTemplate != 171)
            {
                log.Info("The list base template is :{0},not task list,so will return", this.SPList.BaseTemplate);
                return;
            }
            IAveEventReceiverDefinitionCollection eventRecivers = this.SPList.EventReceivers;
            if (eventRecivers == null)
            {
                log.Info("Event receiver is null,so will return,please check work management service is active.");
                return;
            }
            log.Info("update list event receiver  synchronous to:{0}", isSync);
            Dictionary<Guid, int> updateInfo = new Dictionary<Guid, int>();
            if (isSync)
            {
                foreach (IAveEventReceiverDefinition eventone in eventRecivers)
                {
                    if (eventone.Class.Equals("Microsoft.SharePoint.Portal.TaskListNewsFeedEventReceiver", StringComparison.OrdinalIgnoreCase))
                    {
                        if (eventone.Name.Equals("TaskListItemAdded", StringComparison.OrdinalIgnoreCase) && eventone.Synchronization != 1)
                        {
                            log.Info("Event receiver TaskListItemAdded will Synchronous");
                            updateInfo.Add(eventone.ID, 1);
                        }
                        if (eventone.Name.Equals("TaskListItemUpdated", StringComparison.OrdinalIgnoreCase) && eventone.Synchronization != 1)
                        {
                            log.Info("Event receiver TaskListItemUpdated will Synchronous");
                            updateInfo.Add(eventone.ID, 1);
                        }
                    }
                }
            }
            else
            {
                foreach (IAveEventReceiverDefinition eventone in eventRecivers)
                {
                    if (eventone.Class.Equals("Microsoft.SharePoint.Portal.TaskListNewsFeedEventReceiver", StringComparison.OrdinalIgnoreCase))
                    {
                        if (eventone.Name.Equals("TaskListItemAdded", StringComparison.OrdinalIgnoreCase) && eventone.Synchronization != 2)
                        {
                            log.Info("Event receiver TaskListItemAdded will Asynchronous");
                            updateInfo.Add(eventone.ID, 2);
                        }
                        if (eventone.Name.Equals("TaskListItemUpdated", StringComparison.OrdinalIgnoreCase) && eventone.Synchronization != 2)
                        {
                            log.Info("Event receiver TaskListItemUpdated will Asynchronous");
                            updateInfo.Add(eventone.ID, 2);
                        }
                    }
                }
            }
            foreach (KeyValuePair<Guid, int> item in updateInfo)
            {
                try
                {
                    this.SPList.EventReceivers[item.Key].Synchronization = item.Value;
                    this.SPList.EventReceivers[item.Key].Update();
                    log.Info("Event receiver:{0} change sync to:{1} is successful", this.SPList.EventReceivers[item.Key].Name, this.SPList.EventReceivers[item.Key].Synchronization);
                }
                catch (Exception ex)
                {
                    log.Warn("An error occur while updating event receiver sync state,exception:{0}", ex.ToString());
                }
            }

        }
        private void UpdateListRating(AveListSettingInfo listSettingInfo)
        {
            if (mAveParentSite.SPContextKind != AveContextKind.ClientObjectModel)
            {
                mSPList.RestoreListRatingSetting(listSettingInfo);
            }
            else
            {
                if (string.Compare(mAveParentSite.SPSite.SPVersion, "15.", StringComparison.OrdinalIgnoreCase) > 0)//.StartsWith("15.", StringComparison.OrdinalIgnoreCase))//sp13
                {
                    if (!mSPList.ParentWeb.AvailableFields.Contains(new Guid("5a14d1ab-1513-48c7-97b3-657a5ba6c742")))
                    {
                        //ADO-54826 sharepoint抛找不到5a14d1ab-1513-48c7-97b3-657a5ba6c742field的异常
                        throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Restore_NotFindSpecificField);
                    }

                    IAveReputationHelper reputationHelper = mAveParentSite.ObjectModelFactory.CreateReputationHelper();
                    if (listSettingInfo.AllowRatingSetting.Value)
                    {
                        string experience = reputationHelper.GetExperience(mSPList, false);
                        string newExperience = (listSettingInfo.RatingSettingType != null) ? Enum.GetName(typeof(AveRatingSettingType), listSettingInfo.RatingSettingType.Value) : string.Empty;
                        if (newExperience != experience)
                        {
                            if (!string.IsNullOrEmpty(experience))
                            {
                                reputationHelper.SwitchReputation(mSPList, newExperience, experience);
                            }
                            else
                            {
                                reputationHelper.EnableReputation(mSPList, newExperience, false);
                            }
                            mSPList.Update();
                        }
                    }
                    else
                    {
                        reputationHelper.DisableReputation(mSPList);
                        mSPList.Update();
                    }
                }
                else//sp10
                {
                    //ADO-128116 先开启setting会多出rating column，如果对应的column应用mapping，再去新建，那么由于create column keep id，会导致冲突
                    //if (listSettingInfo.AllowRatingSetting.Value)
                    //{
                    //    ParentSite.ObjectModelFactory.CreateRatingsSettingsPage().EnableRatings(mSPList, false);
                    //}
                    //else
                    //{
                    //    ParentSite.ObjectModelFactory.CreateRatingsSettingsPage().DisableRatings(mSPList);
                    //}
                }
            }
        }

        public void UpdateDiscussionLikedCount()
        {
            if (this.SPList.BaseTemplate == AveListTemplateType.DiscussionBoard)
            {
                foreach (var folder in this.RootFolder.Folders)
                {
                    if (folder.Item != null)
                    {
                        AveCamlQuery query = new AveCamlQuery();
                        query.FolderServerRelativeUrl = folder.ServerRelativeUrl;

                        IAveListItemCollection items = this.SPList.GetItems(query);
                        int likedCount = 0;
                        if (GetItemPropertyWithoutException(folder.Item, "LikesCount") != null)
                        {
                            int.TryParse(GetItemPropertyWithoutException(folder.Item, "LikesCount").ToString(), out likedCount);
                        }
                        foreach (IAveListItem item in items)
                        {
                            int itemLikeCount = 0;

                            if (GetItemPropertyWithoutException(item, "LikesCount") != null && int.TryParse(GetItemPropertyWithoutException(item, "LikesCount").ToString(), out itemLikeCount))
                            {
                                likedCount += itemLikeCount;
                            }
                        }
                        folder.Item["DescendantLikesCount"] = likedCount;
                        folder.Item.Update();
                    }
                }
            }
        }

        private object GetItemPropertyWithoutException(IAveListItem item, string propertyName)
        {
            try
            {
                return item[propertyName];
            }
            catch (Exception e)
            {
                log.Debug(string.Format("Failed to get the property: {0}, exception: {1}", propertyName, e));
                return null;
            }
        }

        public void RestoreUserCustomActions(List<AveUserCustomActionInfo> customActions)
        {
            AveSPUserCustomActionCollection restoreUserCustomActions = new AveSPListUserCustomActionCollection(this);
            restoreUserCustomActions.Restore(customActions);
        }

        private void BackupWorkflowStartOption()
        {
            try
            {
                if (ParentSite.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel && SPList != null)
                {
                    log.Info("Being to backup workflow start option for list {0}", SPList.Title);
                    var cache = SPList.BackupWorkflowStartOption(this.ParentWeb.SPWeb.Url, this.ParentWeb.SPWeb.ID, this.SPList.ID);
                    this.ParentSite.WorkflowCache.AddCache(this.ParentWeb.SPWeb.ID, this.SPList.ID, cache);
                    log.Debug("Finish backup workflow start option for list {0}", SPList.Title);
                    //if (cache.HasData())
                    //{
                    //    ReloadList();
                    //}
                }
                else
                {
                    log.Debug("SPList is null, this should be the web root folder, or destination is not client mode");
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while backup list workflow start option setting.Error:{0}", e);
            }
        }

        public void RestoreWorkflowStartOption()
        {
            try
            {
                if (ParentSite.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel && SPList != null)
                {

                    WorkflowStartOptionCache cache;
                    if (ParentSite.WorkflowCache.TryGetListCache(SPList.ParentWeb.ID, SPList.ID, out cache))
                    {
                        log.Info("Being to restore workflow start option for list {0}", SPList.Title);
                        SPList.RestoreWOrkflowStartOption(ParentWeb.SPWeb.Url, ParentWeb.SPWeb.ID, SPList.ID, cache);
                        log.Debug("Finish restore workflow start option for list {0}", SPList.Title);
                    }
                    else
                    {
                        log.Info("Cache for list {0} not exist.", SPList.Title);
                    }

                }
                else
                {
                    log.Debug("SPList is null, this should be the web root folder, or destination is not client mode");
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while backup list workflow start option setting.Error:{0}", e);
            }
        }

        #region IAveSPList Members


        IAveSPContentTypeCollection IAveSPList.AveContentTypes
        {
            get { return mContentTypes; }
        }

        IAveSPFieldCollection IAveSPList.AveFields
        {
            get { return mFields; }
        }

        IAveSPSite IAveSPList.ParentSite
        {
            get { return mAveParentSite; }
        }

        IAveSPWeb IAveSPList.ParentWeb
        {
            get { return mAveSPWeb; }
        }

        IAveObjectSecurity IAveSPList.Security
        {
            get { return mSecurity; }
        }

        #endregion
    }

    //internal class AveSPListV1 : AveSPList, ISPListImport
    //{
    //    private AveSPWebV1 parentWeb;

    //    private ListSourceInfo listCacheInfo = new ListSourceInfo();

    //    internal AveSPWebV1 ParentSPWebV1 { get { return parentWeb; } }

    //    public AveSPListV1(AveSPWebV1 restoreWeb, string listTitle)
    //        : base(restoreWeb, listTitle)
    //    {
    //        this.parentWeb = restoreWeb;
    //    }

    //    /// <summary>
    //    /// Restore List
    //    /// 
    //    /// 这个是新加的接口,外围请暂时不要调用
    //    /// </summary>
    //    /// <param name="restoreStream"></param>
    //    /// <param name="spListRestoreOption"></param>
    //    /// <returns></returns>
    //    public SPFileRestoreReport Restore(IAveRestoreStream restoreStream, SPListRestoreOption spListRestoreOption)
    //    {
    //        var profiler = new AvePoint.Wrapper.Restore.Core.DefaultRestoreListProfiler();

    //        this.Restore(restoreStream, spListRestoreOption, profiler);

    //        return profiler.GenerateReport();
    //    }

    //    /// <summary>
    //    /// Get action according to metadata
    //    /// </summary>
    //    /// <param name="metadataType"></param>
    //    /// <returns></returns>
    //    private Action<IAveRestoreStream, SPListRestoreOption, AveMetadata, ISPListImportProfiler> GetAction(AveMetadataType metadataType)
    //    {
    //        Action<IAveRestoreStream, SPListRestoreOption, AveMetadata, ISPListImportProfiler> action = null;

    //        switch (metadataType)
    //        {
    //            case AveMetadataType.ListBasicInfo:
    //                action = RestoreListBasicInfo;
    //                break;
    //            case AveMetadataType.ListProperty:
    //                action = RestoreListProperty;
    //                break;
    //            case AveMetadataType.ListField:
    //                action = RestoreListField;
    //                break;
    //            case AveMetadataType.ListContentType:
    //                action = RestoreListContentType;
    //                break;
    //            case AveMetadataType.ListEventReceiver:
    //                action = RestoreListEventReceiver;
    //                break;
    //            case AveMetadataType.ListCTWorkflowAssociation:
    //                action = RestoreListCTWorkflowAssociation;
    //                break;
    //            case AveMetadataType.ListWorkflowAssociation:
    //                action = RestoreListWorkflowAssociation;
    //                break;
    //            case AveMetadataType.RoleAssignment:
    //                action = RestoreListRoleAssignment;
    //                break;
    //            case AveMetadataType.UserCache:
    //                action = RestoreListUserCache;
    //                break;
    //            case AveMetadataType.GroupCache:
    //                action = RestoreListGroupCache;
    //                break;
    //            case AveMetadataType.RoleAssignmentsDto:
    //                action = RestoreListRoleAssignmentsDto;
    //                break;
    //            case AveMetadataType.RoleAssignmentInheritStatus:
    //                action = RestoreListRoleAssignmentInheritStatus;
    //                break;
    //            case AveMetadataType.DocImmedSubscriptions:
    //                action = RestoreListDocImmedSubscriptions;
    //                break;
    //            case AveMetadataType.DocSchedSubscriptions:
    //                action = RestoreListDocSchedSubscriptions;
    //                break;
    //            case AveMetadataType.SocialTag:
    //                action = RestoreListSocialTag;
    //                break;
    //            case AveMetadataType.SocialComment:
    //                action = RestoreListSocialComment;
    //                break;
    //            case AveMetadataType.MetadataService:
    //                action = RestoreListMetadataService;
    //                break;
    //        }

    //        return action;
    //    }

    //    private void EnsureConfigurationOption(SPListRestoreOption option)
    //    {
    //        if (option.ConfigurationRestoreOption == null)
    //        {
    //            throw new ArgumentNullException("option.ConfigurationRestoreOption");
    //        }
    //    }

    //    private void EnsureSecurityOption(SPListRestoreOption option)
    //    {
    //        if (option.SecurityRestoreOption == null)
    //        {
    //            throw new ArgumentNullException("option.SecurityRestoreOption");
    //        }
    //    }

    //    private void EnsureWFAssociationOption(SPListRestoreOption option)
    //    {
    //        if (option.WorkflowRestoreOption == null || option.WorkflowRestoreOption.AssociationRestoreOption == null)
    //        {
    //            throw new ArgumentNullException("option.WorkflowRestoreOption.AssociationRestoreOption");
    //        }
    //    }

    //    private void EnsureMMSOption(SPListRestoreOption option)
    //    {
    //        if (option.ManagedMetadataOption == null)
    //        {
    //            throw new ArgumentNullException("option.ManagedMetadataOption");
    //        }
    //    }

    //    private void RestoreListBasicInfo(IAveRestoreStream restoreStream, SPListRestoreOption option, AveMetadata metadata, ISPListImportProfiler profiler)
    //    {
    //        try
    //        {
    //            var listBaseInfo = metadata.GetMetadata<AveListInfo>();

    //            if (option.BeforeBasicInfoAction != null)
    //            {
    //                option.BeforeBasicInfoAction(listBaseInfo);
    //            }

    //            listCacheInfo.SetAveListInfoValue(listBaseInfo);

    //            if (option.RestoreAction == SPContainerRestoreAction.Replace)
    //            {
    //                var list = ParentWeb.AveWeb.GetListByName(this.Name, false);

    //                if (list != null)
    //                {
    //                    if (!list.IsCatalog && list.AllowDeletion)
    //                    {
    //                        if (option.ListDeleted != null)
    //                        {
    //                            option.ListDeleted();
    //                        }
    //                        list.Delete();
    //                    }
    //                }

    //                this.RestoreOption.SetRequestOption(false, false, (int)AveRestoreMode.Replace);
    //            }
    //            else
    //            {
    //                this.RestoreOption.SetRequestOption(false, false, (int)AveRestoreMode.Default);
    //            }

    //            if (option.ConflictCheckOption == SPListConflictCheckOption.CheckRecycleBin)
    //            {
    //                this.RestoringFolder.IsIncludingRecycleBinData = true;
    //            }

    //            this.RestoreOption.mAveListRestoreOption.VerifyListTemplateFeature = option.VerifyListTemplateFeature;

    //            this.RestoreListSelf(listBaseInfo, option.FindOption.ToListRestoreOption(), !option.AvoidToRestoreSameList);

    //            //report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            if (option.AfterBasicInfoAction != null)
    //            {
    //                var afterInfo = new AveListRestoreBasicInfo();
    //                afterInfo.Status = RestoreObjectResult.Exist;
    //                if (IsNewCreated)
    //                {
    //                    afterInfo.Status = RestoreObjectResult.NewCreated;
    //                }
    //                else if (!NeedContinue)
    //                {
    //                    afterInfo.Status = RestoreObjectResult.Skipped;
    //                }
    //                option.AfterBasicInfoAction(afterInfo);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            //report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Failed, ex.Message);
    //            log.Error("Restore list basic info for {0} failed:{1}", Name, ex);
    //            throw;
    //        }
    //    }

    //    private void RestoreListProperty(IAveRestoreStream restoreStream, SPListRestoreOption option, AveMetadata metadata, ISPListImportProfiler profiler)
    //    {
    //        EnsureConfigurationOption(option);

    //        var listsetting = metadata.GetMetadata<AveListSettingInfo>();

    //        if (NeedRestore(option) && option.ConfigurationRestoreOption.RestoreConfiguration)
    //        {
    //            this.MoveConnectorSetting = option.ConfigurationRestoreOption.RestoreConnectorSettings;

    //            this.RestoreListProperty(listsetting);
    //            this.RestoreListRootFolder();
    //            //report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            //report.Details.AnalyzeReport(GetReport());
    //        }
    //        else
    //        {
    //            this.ListSettingInfo = listsetting;
    //            //report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //        }
    //    }

    //    private bool NeedRestore(SPListRestoreOption option)
    //    {
    //        return IsNewCreated || option.RestoreAction != SPContainerRestoreAction.Skip;
    //    }

    //    private void RestoreListField(IAveRestoreStream restoreStream, SPListRestoreOption option, AveMetadata metadata, ISPListImportProfiler profiler)
    //    {
    //        EnsureConfigurationOption(option);

    //        if ((NeedRestore(option) && option.ConfigurationRestoreOption.RestoreConfiguration) || option.ConfigurationRestoreOption.FieldRestoreAction == SPObjectRestoreAction.Cache)
    //        {
    //            var fieldXml = string.Empty;

    //            if (option.ConfigurationRestoreOption.FieldRestoreAction != SPObjectRestoreAction.Skip)
    //            {
    //                fieldXml = metadata.GetMetadata<string>();
    //                if (option.ConfigurationRestoreOption.ProcessFieldAction != null)
    //                {
    //                    fieldXml = option.ConfigurationRestoreOption.ProcessFieldAction(fieldXml);
    //                }
    //            }

    //            //Convert Mapping
    //            if (FieldMapping != null)
    //            {
    //                var ctMetaData = restoreStream.TryReadMetadata(AveMetadataType.ListContentType);
    //                if (ctMetaData != null)
    //                {
    //                    AveContentTypeCollectionInfo listCTCollectionInfo = ctMetaData.GetMetadata<AveContentTypeCollectionInfo>();
    //                    var mappingListInfo = new AveMappingSourceSPListInfo(listCacheInfo.ToAveListInfo(), parentWeb.MappingWebInfo, listCTCollectionInfo);
    //                    var costomMapping = FieldMapping.ToIAveFieldMapping(mappingListInfo);
    //                    if (costomMapping != null)
    //                    {
    //                        this.AveFields.FieldMapping.SetCustomMapping(costomMapping);
    //                    }
    //                }
    //            }

    //            switch (option.ConfigurationRestoreOption.FieldRestoreAction)
    //            {
    //                case SPObjectRestoreAction.Restore:
    //                    this.AveFields.RestoreFields(fieldXml, option.ConfigurationRestoreOption.FieldRestoreOption);
    //                    //report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                    //report.Details.AnalyzeReport(this.AveFields.GetReport());
    //                    break;
    //                case SPObjectRestoreAction.Cache:
    //                    this.AveFields.LoadFields(fieldXml);
    //                    //report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //                    break;
    //                default:
    //                    //report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //                    break;
    //            }
    //        }
    //        else
    //        {
    //            //report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //        }
    //    }

    //    private void RestoreListContentType(IAveRestoreStream restoreStream, SPListRestoreOption option, AveMetadata metadata, ISPListImportProfiler profiler)
    //    {
    //        EnsureConfigurationOption(option);

    //        if ((NeedRestore(option) && option.ConfigurationRestoreOption.RestoreConfiguration) || option.ConfigurationRestoreOption.ContentTypeRestoreAction == SPObjectRestoreAction.Cache)
    //        {
    //            AveContentTypeCollectionInfo info = null;

    //            if (option.ConfigurationRestoreOption.ContentTypeRestoreAction != SPObjectRestoreAction.Skip)
    //            {
    //                info = metadata.GetMetadata<AveContentTypeCollectionInfo>();

    //                if (option.ConfigurationRestoreOption.ProcessContentTypeAction != null)
    //                {
    //                    option.ConfigurationRestoreOption.ProcessContentTypeAction(info);
    //                }

    //                //Convert Mapping
    //                if (ContentTypeMapping != null)
    //                {
    //                    var mappingListInfo = new AveMappingSourceSPListInfo(listCacheInfo.ToAveListInfo(), parentWeb.MappingWebInfo, info);
    //                    var customMapping = ContentTypeMapping.ToIAveContentTypeMapping(mappingListInfo);
    //                    if (customMapping != null)
    //                    {
    //                        this.AveContentTypes.ContentTypeMapping.SetCustomMapping(customMapping);
    //                    }
    //                }
    //            }

    //            switch (option.ConfigurationRestoreOption.ContentTypeRestoreAction)
    //            {
    //                case SPObjectRestoreAction.Restore:
    //                    this.AveContentTypes.RestoreContentTypes(info, option.ConfigurationRestoreOption.ContentTypeNameMapping, option.ConfigurationRestoreOption.ContentTypeRestoreOption);
    //                    //report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                    //report.Details.AnalyzeReport(this.AveContentTypes.GetReport());                        
    //                    break;
    //                case SPObjectRestoreAction.Cache:
    //                    this.AveContentTypes.LoadContentTypes(info);
    //                    //report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //                    break;
    //                default:
    //                    //report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //                    break;
    //            }
    //        }
    //        else
    //        {
    //            //report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //        }
    //    }

    //    private void RestoreListEventReceiver(IAveRestoreStream restoreStream, SPListRestoreOption option, AveMetadata metadata, ISPListImportProfiler profiler)
    //    {
    //        EnsureConfigurationOption(option);

    //        if (NeedRestore(option) && option.ConfigurationRestoreOption.RestoreConfiguration)
    //        {
    //            var eventReceivers = metadata.GetMetadata<List<AveEventReceiverInfo>>();
    //            using (var aveEventReceivers = AveSPEventReceiver.CreateInstance(this))
    //            {
    //                aveEventReceivers.RestoreEventReceivers(eventReceivers);
    //                //report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                //report.Details.AnalyzeReport(aveEventReceivers.GetReport());
    //            }
    //        }
    //        else
    //        {
    //            //report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Skipped);
    //        }
    //    }

    //    private void RestoreListCTWorkflowAssociation(IAveRestoreStream restoreStream, SPListRestoreOption option, AveMetadata metadata, ISPListImportProfiler profiler)
    //    {
    //        EnsureWFAssociationOption(option);

    //        if (NeedRestore(option) && option.WorkflowRestoreOption.AssociationRestoreOption.RestoreAction != SPObjectRestoreAction.Skip)
    //        {
    //            var ctWFInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
    //            var ctWFResolution = WFConflictResolution.Instance;
    //            ctWFResolution.AssociationOption = (WFAssociationConflictResolutionOption)option.WorkflowRestoreOption.AssociationRestoreOption.ConflictResolutionOption;
    //            SPWorkflowProcessorRuntime.ProcessAssociation = true;
    //            SPWorkflowProcessorRuntime.TemplateFileConflictRules = (TemplateFileConflictRulesEnum)option.WorkflowRestoreOption.AssociationRestoreOption.TemplateFileConflictRules;

    //            var associationRestored = false;

    //            foreach (var unit in ctWFInfo)
    //            {
    //                if (option.WorkflowRestoreOption.AssociationRestoreOption.RestoreAction == SPObjectRestoreAction.Restore)
    //                {
    //                    string contentTypeId;
    //                    if ((contentTypeId = AveContentTypes.ContentTypeMapping.GetMappingRestoredContentTypeId(unit.CTId)) != null)
    //                    {
    //                        var ct = this.SPList.ContentTypes[ParentSite.ObjectModelFactory.CreateContentTypeId(contentTypeId)];
    //                        if (ct != null)
    //                        {
    //                            unit.CTName = ct.Name;
    //                        }
    //                        else
    //                        {
    //                            ct = this.SPList.ContentTypes[unit.CTName];
    //                        }
    //                        ctWFResolution.RestoreAssociationData(unit, this, ct);
    //                        associationRestored = true;
    //                    }
    //                }
    //                else
    //                {
    //                    ctWFResolution.CacheAssociationData(unit);
    //                }
    //            }
    //            if (associationRestored) { this.ReloadList(); }
    //            //using (var workflowReport = ctWFResolution.GetReport())
    //            //{
    //            //    report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            //    report.Details.AnalyzeReport(workflowReport);
    //            //}
    //        }
    //    }

    //    private void RestoreListWorkflowAssociation(IAveRestoreStream restoreStream, SPListRestoreOption option, AveMetadata metadata, ISPListImportProfiler profiler)
    //    {
    //        EnsureWFAssociationOption(option);

    //        if (NeedRestore(option) && option.WorkflowRestoreOption.AssociationRestoreOption.RestoreAction != SPObjectRestoreAction.Skip)
    //        {
    //            var wfInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
    //            var wfResolution = WFConflictResolution.Instance;
    //            wfResolution.AssociationOption = (WFAssociationConflictResolutionOption)option.WorkflowRestoreOption.AssociationRestoreOption.ConflictResolutionOption;
    //            SPWorkflowProcessorRuntime.ProcessAssociation = true;
    //            SPWorkflowProcessorRuntime.TemplateFileConflictRules = (TemplateFileConflictRulesEnum)option.WorkflowRestoreOption.AssociationRestoreOption.TemplateFileConflictRules;
    //            var associationRestored = false;
    //            foreach (var unit in wfInfo)
    //            {
    //                if (option.WorkflowRestoreOption.AssociationRestoreOption.RestoreAction == SPObjectRestoreAction.Restore)
    //                {
    //                    wfResolution.RestoreAssociationData(unit, this);
    //                    associationRestored = true;
    //                }
    //                else
    //                {
    //                    wfResolution.CacheAssociationData(unit);
    //                }
    //            }
    //            if (associationRestored) { this.ReloadList(); }
    //            //using (var workflowReport = wfResolution.GetReport())
    //            //{
    //            //    report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //            //    report.Details.AnalyzeReport(workflowReport);
    //            //}
    //        }
    //    }

    //    private void RestoreListRoleAssignment(IAveRestoreStream restoreStream, SPListRestoreOption option, AveMetadata metadata, ISPListImportProfiler profiler)
    //    {
    //        EnsureSecurityOption(option);

    //        if (NeedRestore(option) && option.SecurityRestoreOption.RestoreSecurity)
    //        {
    //            var roleAssignments = metadata.GetMetadata<List<AveRoleAssignmentInfo>>();

    //            using (var security = AveObjectSecurity.CreateInstance(this))
    //            {
    //                if (option.SecurityRestoreOption.RoleAssignmentsRestoreOption.FilterRoleAssignments != null)
    //                {
    //                    roleAssignments = option.SecurityRestoreOption.RoleAssignmentsRestoreOption.FilterRoleAssignments(roleAssignments);
    //                }

    //                if (this.ListSettingInfo != null && this.ListSettingInfo.HasUniqueRoleAssigntments != null && this.ListSettingInfo.HasUniqueRoleAssigntments.IsAvailable)
    //                {
    //                    security.SourceHasUniqueRoleAssignment = this.ListSettingInfo.HasUniqueRoleAssigntments.Value;
    //                }
    //                security.RestoreRoleAssignments(roleAssignments, option.SecurityRestoreOption.RoleAssignmentsRestoreOption.ToSecurityRestoreOption());

    //                //report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                //report.Details.AnalyzeReport(security.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreListGroupCache(IAveRestoreStream restoreStream, SPListRestoreOption option, AveMetadata metadata, ISPListImportProfiler profiler)
    //    {
    //        EnsureSecurityOption(option);
    //        if (NeedRestore(option))
    //        {
    //            ParentSPWebV1.ParentSPSiteV1.RestoreSiteGroups(option.SecurityRestoreOption, metadata, profiler);
    //        }
    //    }

    //    private void RestoreListUserCache(IAveRestoreStream restoreStream, SPListRestoreOption option, AveMetadata metadata, ISPListImportProfiler profiler)
    //    {
    //        EnsureSecurityOption(option);
    //        if (NeedRestore(option))
    //        {
    //            ParentSPWebV1.ParentSPSiteV1.RestoreSiteUsers(option.SecurityRestoreOption, metadata, profiler);
    //        }
    //    }

    //    private void RestoreListRoleAssignmentInheritStatus(IAveRestoreStream restoreStream, SPListRestoreOption option, AveMetadata metadata, ISPListImportProfiler profiler)
    //    {
    //        EnsureSecurityOption(option);
    //        if (NeedRestore(option) && option.SecurityRestoreOption.RestoreSecurity)
    //        {
    //            var inheritStatus = metadata.GetMetadata<bool>();

    //            using (var security = AveObjectSecurity.CreateInstance(this))
    //            {
    //                if (option.SecurityRestoreOption.RoleAssignmentsRestoreOption.RestoreInheritance)
    //                {
    //                    security.SourceHasUniqueRoleAssignment = inheritStatus;
    //                }

    //                security.RestoreRoleAssignments(null, option.SecurityRestoreOption.RoleAssignmentsRestoreOption.ToSecurityRestoreOption());

    //                //report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                //report.Details.AnalyzeReport(security.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreListRoleAssignmentsDto(IAveRestoreStream restoreStream, SPListRestoreOption option, AveMetadata metadata, ISPListImportProfiler profiler)
    //    {
    //        EnsureSecurityOption(option);
    //        if (NeedRestore(option) && option.SecurityRestoreOption.RestoreSecurity)
    //        {
    //            var roleAssignments = metadata.GetMetadata<AvePoint.Wrapper.Core.SPBackupDto.SPRoleAssignmentsDto>();

    //            using (var security = AveObjectSecurity.CreateInstance(this))
    //            {
    //                if (option.SecurityRestoreOption.RoleAssignmentsRestoreOption.FilterRoleAssignments != null)
    //                {
    //                    roleAssignments.RoleAssignmentInfos = option.SecurityRestoreOption.RoleAssignmentsRestoreOption.FilterRoleAssignments(roleAssignments.RoleAssignmentInfos);
    //                }

    //                if (option.SecurityRestoreOption.RoleAssignmentsRestoreOption.RestoreInheritance)
    //                {
    //                    security.SourceHasUniqueRoleAssignment = !roleAssignments.IsInherit;
    //                }

    //                security.ParentSite.RestoreUser(roleAssignments.UserCache);
    //                security.ParentSite.RestoreGroup(roleAssignments.GroupCache);

    //                security.RestoreRoleAssignments(roleAssignments.RoleAssignmentInfos, option.SecurityRestoreOption.RoleAssignmentsRestoreOption.ToSecurityRestoreOption());

    //                //report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                //report.Details.AnalyzeReport(security.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreListMetadataService(IAveRestoreStream restoreStream, SPListRestoreOption option, AveMetadata metadata, ISPListImportProfiler profiler)
    //    {
    //        EnsureMMSOption(option);

    //        if (NeedRestore(option) && option.ManagedMetadataOption.RestoreType != SPManagedMetadataRestoreType.Restore)
    //        {
    //            throw new NotSupportedException(option.ManagedMetadataOption.RestoreType.ToString());
    //        }

    //        ParentSPWebV1.ParentSPSiteV1.RestoreMetadataService(option.ManagedMetadataOption, metadata, profiler);
    //    }

    //    private void RestoreListSocialComment(IAveRestoreStream restoreStream, SPListRestoreOption option, AveMetadata metadata, ISPListImportProfiler profiler)
    //    {
    //        EnsureConfigurationOption(option);

    //        if (NeedRestore(option) && option.ConfigurationRestoreOption.RestoreConfiguration)
    //        {
    //            using (var socialComment = new AveSPSocialComment(this.Url, ParentSite))
    //            {
    //                var socialComments = metadata.GetMetadata<List<AveSocialCommentInfo>>();

    //                socialComment.Restore(socialComments);

    //                //report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                //report.Details.AnalyzeReport(socialComment.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreListSocialTag(IAveRestoreStream restoreStream, SPListRestoreOption option, AveMetadata metadata, ISPListImportProfiler profiler)
    //    {
    //        EnsureConfigurationOption(option);

    //        if (NeedRestore(option) && option.ConfigurationRestoreOption.RestoreConfiguration)
    //        {
    //            using (var socialTag = new AveSPSocialTag(this.Url, ParentSite))
    //            {
    //                var socialTags = metadata.GetMetadata<List<AveSocialTagInfo>>();

    //                socialTag.Restore(socialTags);

    //                //report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                //report.Details.AnalyzeReport(socialTag.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreListDocSchedSubscriptions(IAveRestoreStream restoreStream, SPListRestoreOption option, AveMetadata metadata, ISPListImportProfiler profiler)
    //    {
    //        EnsureConfigurationOption(option);

    //        if (NeedRestore(option) && option.ConfigurationRestoreOption.RestoreConfiguration)
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
    //                //report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                //report.Details.AnalyzeReport(alert.GetReport());
    //            }
    //        }
    //    }

    //    private void RestoreListDocImmedSubscriptions(IAveRestoreStream restoreStream, SPListRestoreOption option, AveMetadata metadata, ISPListImportProfiler profiler)
    //    {
    //        EnsureConfigurationOption(option);

    //        if (NeedRestore(option) && option.ConfigurationRestoreOption.RestoreConfiguration)
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
    //                //report.Details = new MetadataRestoreDetails(WrapperRestoreStatus.Successful);
    //                //report.Details.AnalyzeReport(alert.GetReport());
    //            }
    //        }
    //    }


    //    public void Restore(IAveRestoreStream restoreStream, SPListRestoreOption spListRestoreOption, ISPListImportProfiler profiler)
    //    {
    //        if (restoreStream == null)
    //        {
    //            throw new ArgumentNullException("restoreStream");
    //        }

    //        if (spListRestoreOption == null)
    //        {
    //            throw new ArgumentNullException("spListRestoreOption");
    //        }


    //        AveMetadata metadata = null;

    //        while ((metadata = restoreStream.ReadMetadata()) != null)
    //        {
    //            if (!NeedContinue)
    //            {
    //                log.Error("This list need to skip. List Title:{0}.", this.listCacheInfo.Title);
    //                return;
    //            }

    //            var action = GetAction(metadata.MetadataType);

    //            if (action != null)
    //            {
    //                var report = new MetadataRestoreReport(metadata.MetadataType);
    //                using (WrapperStopwatch.CreateInstance(spListRestoreOption.IncludePerformanceDetails, report.AddTimeUsage))
    //                {
    //                    action(restoreStream, spListRestoreOption, metadata, profiler);
    //                }
    //            }
    //            else
    //            {
    //                log.Error("There is no action for {0}, please submit a request for this type.", metadata.MetadataType);
    //            }
    //        }

    //    }

    //    public IFieldMapping FieldMapping { get; set; }

    //    public IContentTypeMapping ContentTypeMapping { get; set; }
    //}

    /// <summary>
    /// 缓存备份信息 basic info，setting info等
    /// </summary>
    internal class ListSourceInfo
    {
        //AveWebInfo
        public string Title { get; set; }
        public int BaseTemplate { get; set; }

        internal void SetAveListInfoValue(AveListInfo listBaseInfo)
        {
            this.Title = listBaseInfo.Title;
            this.BaseTemplate = listBaseInfo.BaseTemplate;
        }

        internal AveListInfo ToAveListInfo()
        {
            return new AveListInfo()
            {
                Title = this.Title,
                BaseTemplate = this.BaseTemplate
            };
        }
    }

}
