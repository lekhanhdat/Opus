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
using LS.SPWorkflowProcessor;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Resource;
using System.Text.RegularExpressions;

namespace AvePoint.Wrapper.Restore
{
    [AveCodeReviewAttribute("2012/03/09", "Navy.Li@avepoint.com", "Bingkun.Wang@AvePoint.com",
        new string[] { CodeReviewConstants.CHECK_LIST_ID_CO_1, 
                       CodeReviewConstants.CHECK_LIST_ID_CO_6, 
                       CodeReviewConstants.CHECK_LIST_ID_FA_4 }, null, true)]
    [AveCodeReview("2012/04/20", "Yuzhi.Jiang@AvePoint.com", "Yongqiang.Zhou@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CS_1, CodeReviewConstants.CHECK_LIST_ID_CS_2 }, null, true)]

    public class AveSPList : RestoreableObject<AveSPList>,IDisposable
    {
        //private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        //List Title
        private string mName;
        private string mLanguageMappingName;
        private AveSPWeb mAveSPWeb;
        private IAveBackupRestoreQueryService mQueryService;
        private AveListInfo mListInfo;
        private AveListSettingInfo mListSettingInfo;
        private bool mIsNewCreated = false;
        private bool mNeedContinue = true;
        private AveSkipType mSkipType = AveSkipType.Unknown;
        private IAveList mSPList;
        private IAveListItemSerializer mListItemSerializer;
        private Guid mId;
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
        private AveDraftVisibilityType mDraftVersionVisibility = AveDraftVisibilityType.Reader;
        protected IReport reportor = new AveWrapperReport();
        public IReport GetReport()
        {
            return reportor;
        }

        private AveSPSite mAveParentSite;
        private AveListSecurity mSecurity;
        private IAveListItem mPreItem;
        private string mSrcUrl;
        private String mUrl;
        private long mSize;
        private bool mRestoreRssView = false;


        private IList<AveSolutionInfo> mSandboxSolutions = new List<AveSolutionInfo>();
        private AveFeatureInfoBox mSiteFeatures = new AveFeatureInfoBox() { Scope = AveFeatureScope.Site };
        private AveFeatureInfoBox mWebFeatures = new AveFeatureInfoBox() { Scope = AveFeatureScope.Web };
        private Dictionary<string, object> mDefValue = null;
        private bool? isCommunitySiteDiscussionList = null;
        private List<string> invalidLookupList = new List<string>();
        internal bool IsCommunitySiteDiscussionList
        {
            get
            {
                if (isCommunitySiteDiscussionList == null)
                {
                    if (ParentWeb.SPWeb.Features[new Guid("961D6A9C-4388-4CF2-9733-38EE8C89AFD4")] != null)
                    {
                        if (mSPList != null && mSPList.BaseTemplate == AveListTemplateType.DiscussionBoard)
                        {
                            if (mSPList.EventReceivers != null)
                            {
                                foreach (IAveEventReceiverDefinition def in mSPList.EventReceivers)
                                {
                                    if (string.Equals(def.Class, "Microsoft.SharePoint.Portal.CommunityEventReceiver", StringComparison.OrdinalIgnoreCase))
                                    {
                                        isCommunitySiteDiscussionList = true;
                                        ParentWeb.CommunitySiteDiscussionsListTitle = mSPList.Title;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (isCommunitySiteDiscussionList == null)
                    {
                        isCommunitySiteDiscussionList = false;
                    }
                }
                return isCommunitySiteDiscussionList.Value;
            }
        }

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
                    {new Guid("f9ce21f8-f437-4f7e-8bc6-946378c850f0"),AveBaseType.GenericList},
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
                        {(int)AveListTemplateType.MaintenanceLogLibrary},//SAAS-23998
                        {(int)AveListTemplateType.Social},
                        {850},//850 refer to Pages Library
                        {10102}, // 10102 refer to converted form
                        {125},//125 refer to appdata list
                        {1101}, //https://jira.avepoint.net/browse/SAAS-13571 , pwsrisks, feature Id 448e1394-5e76-44b4-9e1c-169b7a389a1b
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
                        {(int)AveListTemplateType.MasterPageCatalog},
                        {(int)AveListTemplateType.DesignCatalog},
                        {(int)AveListTemplateType.AppDataCatalog},
                        {(int)AveListTemplateType.WebTemplateExtensionsList}, //[SAAS-41190]Reload web and find 'Web Template Extension list' again.
                };
            }
        }

        public bool MoveConnectorSetting { get; set; }

        private Dictionary<string, List<AveExtendMasterPageInfo>> tempMasterSettings = new Dictionary<string, List<AveExtendMasterPageInfo>>();
        public Dictionary<string, List<AveExtendMasterPageInfo>> TempMasterSettings
        {
            get { return tempMasterSettings; }
        }

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

        public void RegisterSandboxSolution(int rowId, Guid solutionId, IList<Guid> dependencySolutions)
        {
            lock (mSandboxSolutions)
            {
                mSandboxSolutions.Add(new AveSolutionInfo() { RowId = rowId, Id = solutionId, Dependencies = dependencySolutions });
            }
        }

        public void RegisterSandboxFeatures(Guid featureId, AveFeatureScope scope, List<Guid> dependencyFeatures)
        {
            AveFeatureInfoBox featureInfobox = scope == AveFeatureScope.Site ? mSiteFeatures : mWebFeatures;
            lock (featureInfobox)
            {
                featureInfobox.FeatureList.Add(new AveFeatureInfo()
                    {
                        Id = featureId,
                        Scope = scope,
                        Dependencies = dependencyFeatures
                    });
            }
        }

        public Dictionary<string, object> DefaultValues
        {
            get
            {
                if (mDefValue == null)
                {
                    GetDefaultValue();
                }
                return mDefValue;
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

        //存储记录list上的Taxonomy Field和它的TextField的名字对应关系。
        private List<string> mTaxonomyFields;
        public List<string> TaxonomyFields
        {
            get
            {
                if (mTaxonomyFields == null)
                {
                    mTaxonomyFields = AveTaxonomyField.GetListTaxonomyFields(mSPList);
                }
                return mTaxonomyFields;
            }
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

        public string SrcUrl
        {
            get
            {
                return mSrcUrl;
            }
        }

        public string Url
        {
            get
            {
                return mUrl;
            }
        }

        public long Size
        {
            get
            {
                return mSize;
            }
        }

        public bool NeedContinue
        {
            get { return this.mNeedContinue; }
            set { this.mNeedContinue = value; }
        }

        public AveSkipType SkipType
        {
            get { return this.mSkipType; }
        }

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

        public Guid Id
        {
            get { return mId; }
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
            set { mIsNewCreated = value; }
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
                    return mAveSPWeb.SPWeb != null ? mAveSPWeb.SPWeb.RootFolder : null;
                }
                else
                {
                    return mSPList != null ? mSPList.RootFolder : null;
                }
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

        string[] ForceClassicModeByListTemplateIDs { get; set; } = new string[] { "116", "850", "115", "140" };

        public bool IsSpecialList
        {
            get
            {
                return mListInfo != null &&
                    (ForceClassicModeByListTemplateIDs.Contains(mListInfo.BaseTemplate.ToString())
                    || mListInfo.ServerRelativeUrl.Contains("Style Library", StringComparison.OrdinalIgnoreCase));
            }
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
            get { return mAutoDeclareRecord; }
        }

        public static bool IsHighSpeedMode { get; set; }

        public AveSPList(AveSPWeb _AveWeb, string _name)
        {
            mAveSPWeb = _AveWeb;
            mAveParentSite = mAveSPWeb.ParentSite;
            mName = mAveSPWeb.ParentSite.GetNameByLanguageMapping(_name, AveLanguageMappingType.ListMapping);
            if (!string.Equals(mName, _name, StringComparison.OrdinalIgnoreCase))
            {
                log.Info("Mapping list title from {0} to {1} in Init AveSPList.", _name, mName);
                mLanguageMappingName = mName;
            }
            mQueryService = mAveSPWeb.QueryService;
            mIsNewCreated = mAveSPWeb.Name != "." ? mAveSPWeb.IsNewCreated : false;
            mFields = new AveSPListFieldCollection(this);
            mContentTypes = new AveSPListContentTypeCollection(this);
            mRestringFolder = new RestoringDto();
        }

        public AveSPList(AveSPWeb _AveWeb, string _name, bool selectToList)
            : this(_AveWeb, _name)
        {
            if (selectToList) //如果勾选了list或list以下级别则list level不做language mapping
            {
                mName = _name;
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
        public void GetListSelf(AveListInfo sourceListInfo)
        {
            mListInfo = sourceListInfo;//mSPList.GetListInfo();
            mOldId = sourceListInfo.Id;
            CheckNeedSkipList(sourceListInfo);

            mSPList = mAveSPWeb.SPWeb.Lists.GetListByName(mName, false);
            log.Info("Get list by name: {0}, result: {1}", mName, mSPList != null ? "success" : "failed");
            Common.ArgumentCheck.CheckNotNull(mSPList);
            mId = mSPList.ID;
            string webRootFolderUrl = mAveSPWeb.SPWeb.RootFolder.ServerRelativeUrl;
            string listRootFolderUrl = mSPList?.RootFolder.ServerRelativeUrl;
            mUrl = mAveSPWeb.SPWeb.Url.TrimEnd('/') + "/" + listRootFolderUrl?.Substring(webRootFolderUrl.Length).Trim('/');
            this.AveFields.LoadExistLookupFields();

            log.Info($"List Url: {mUrl}, source url: {sourceListInfo.Url}");
        }
        /// <summary>
        /// Decode specail characters in path from media: ('%1' to '%'; '%2' to '\')
        /// </summary>
        public void DecodeNameForSpecialChar()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.DecodeNameForSpecialChar"))
            {
#endif
                if (!string.IsNullOrEmpty(mName))
                {
                    mName = AvePoint.GCommon.AveConverter.DecodeSpecialChar(mName);
                    mName = mAveSPWeb.ParentSite.GetNameByLanguageMapping(mName, AveLanguageMappingType.ListMapping);
                }
#if PerformanceLog
            }
#endif
        }

        public void RestoreListProperty(AveListSettingInfo listSettingInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.RestoreListProperty"))
            {
#endif
                try
                {
                    mListSettingInfo = listSettingInfo;
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
                        mSPList.DefaultItemOpen = (AveDefaultItemOpen)Enum.ToObject(typeof(AveDefaultItemOpen), listSettingInfo.DefaultItemOpen.Value);
                        if (listSettingInfo.DefaultItemOpenUseListSetting != null && listSettingInfo.DefaultItemOpenUseListSetting.IsAvailable)
                        {
                            mSPList.DefaultItemOpenUseListSetting = listSettingInfo.DefaultItemOpenUseListSetting.Value;
                        }
                    }

                    if (listSettingInfo.ListExperience != null && listSettingInfo.ListExperience.IsAvailable)
                    {
                        mSPList.ListExperience = (AveListExperience)Enum.ToObject(typeof(AveListExperience), listSettingInfo.ListExperience.Value);
                    }
                    if (listSettingInfo.EnableManagedIndexes != null && listSettingInfo.EnableManagedIndexes.IsAvailable)
                    {
                        mSPList.EnableManagedIndexes = listSettingInfo.EnableManagedIndexes.Value;
                    }
                    //Change this update to Site Post Action
                    if (listSettingInfo.EnableAssignToEmail != null && listSettingInfo.EnableAssignToEmail.IsAvailable)
                    {
                        if (mAveSPWeb != null && mAveSPWeb.ParentSite != null && mAveSPWeb.ParentSite.IsListIncludeEnableAssignEmail(mSPList))
                        {
                            var settingInfo = mAveSPWeb.ParentSite.GetOrCreateEndRestoreListSettingsInfo(mSPList);
                            settingInfo.SourceEnableAssignToEmail = listSettingInfo.EnableAssignToEmail.Value;
                            log.Info($"RestoreListProperty_EndRestoreListSettingsMapping:[EnableAssignToEmail-true]");
                        }
                        else
                        {
                            log.Info($"RestoreListProperty_EndRestoreListSettingsMapping:[EnableAssignToEmail-false]");
                        }
                        //   mSPList.EnableAssignToEmail = listSettingInfo.EnableAssignToEmail.Value;
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

                    if (mSPList.BaseType == AveBaseType.DocumentLibrary)
                    {
                        if (listSettingInfo.EnableMinorVersions != null && listSettingInfo.EnableMinorVersions.IsAvailable)
                        {
                            //SAAS-21870 目的端开启EnableVersioning时MajorVersionLimit值会自动变成500，导致源端MajorVersionLimit值为空时MajorVersionLimit属性冲突判断无效。
                            if (listSettingInfo.EnableVersioning.Value && !mSPList.EnableVersioning)
                            {
                                mSPList.MajorVersionLimit = 500;
                            }
                            //SAAS-21870 目的端开启MinorVersion时MajorWithMinorVersionsLimit值会自动变成1，导致源端MajorWithMinorVersionsLimit值为空时MajorWithMinorVersionsLimit属性冲突判断无效。
                            if (!mSPList.EnableMinorVersions && listSettingInfo.EnableMinorVersions.Value)
                            {
                                mSPList.MajorWithMinorVersionsLimit = 1;
                            }
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
                    if (listSettingInfo.CrawlNonDefaultViews != null && listSettingInfo.CrawlNonDefaultViews.IsAvailable)
                    {
                        mSPList.CrawlNonDefaultViews = listSettingInfo.CrawlNonDefaultViews.Value;
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
                        mSPList.EnforceDataValidation = listSettingInfo.EnforceDataValidation.Value;
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
                        // SAAS-25938 原端list 引用的column 在还原setting时 有可能还没有还原该column，故先备份formula的值 并在 PostAction中还原
                        mValidationFormula = listSettingInfo.ValidationFormula.Value;
                        mListValidationSettingFlag |= AveListSettingFlags.LIST_VALIDATION_FORMULA;
                    }

                    if (listSettingInfo.ValidationMessage != null && listSettingInfo.ValidationMessage.IsAvailable
                        && listSettingInfo.ValidationMessage.Value != null && listSettingInfo.ValidationMessage.Value.Length <= 0x400L)
                    {
                        mValidationMessage = listSettingInfo.ValidationMessage.Value;
                        mListValidationSettingFlag |= AveListSettingFlags.LIST_VALIDATION_MESSAGE;
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
                    if (listSettingInfo.OnQuickLaunch != null && listSettingInfo.OnQuickLaunch.IsAvailable)
                    {
                        mSPList.OnQuickLaunch = listSettingInfo.OnQuickLaunch.Value;
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
                        && mSPList.ParentWeb.WebTemplate != null && mSPList.ParentWeb.WebTemplate.StartsWith(AveWrapperConstants.mWebTemplateMWS, StringComparison.OrdinalIgnoreCase))
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
                                reportor.AddDetail(new AveWrapperReportDto(mSPList.Title, mSPList.Title, AveReportObjectType.ListProperty, AveStatus.Failed, string.Format("An error occurred while clear MultipleDataList default folders. error:{0}", e.Message)));
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
                    //修改下面两个if的判断条件，当Value.HasValue为false(即源端的值为空)时也进行处理
                    //源端为空，目的端有值的时候无法更新数据，故去掉listSettingInfo.MaxMajorVersionCount.Value > 0的判断
                    if ((mSPList.EnableMinorVersions || mSPList.EnableModeration) && listSettingInfo.MaxMajorwithMinorVersionCount != null && listSettingInfo.MaxMajorwithMinorVersionCount.IsAvailable
                        && (!listSettingInfo.MaxMajorwithMinorVersionCount.Value.HasValue || listSettingInfo.MaxMajorwithMinorVersionCount.Value < 0xc351))
                    {
                        mSPList.MajorWithMinorVersionsLimit = listSettingInfo.MaxMajorwithMinorVersionCount.Value.HasValue ? listSettingInfo.MaxMajorwithMinorVersionCount.Value.Value : AveAllListsTableColumnValue.MaxMajorwithMinorVersionCount;
                    }

                    if (this.ParentWeb.SPWeb.AppInstanceId == Guid.Empty)
                    {
                        if (mSPList.EnableVersioning && listSettingInfo.MaxMajorVersionCount != null && listSettingInfo.MaxMajorVersionCount.IsAvailable
                            && (!listSettingInfo.MaxMajorVersionCount.Value.HasValue || listSettingInfo.MaxMajorVersionCount.Value < 0xc351))
                        {
                            mSPList.MajorVersionLimit = listSettingInfo.MaxMajorVersionCount.Value.HasValue ? listSettingInfo.MaxMajorVersionCount.Value.Value : AveAllListsTableColumnValue.MaxMajorVersionCount;
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
                    if (mSPList.HasUniqueRoleAssignments && listSettingInfo.AnonymousPermMask64 != null && listSettingInfo.AnonymousPermMask64.IsAvailable)
                    {
                        if (mSPList.AnonymousPermMask64 != (AveBasePermissions)listSettingInfo.AnonymousPermMask64.Value)
                        {
                            mSPList.AnonymousPermMask64 = (AveBasePermissions)listSettingInfo.AnonymousPermMask64.Value;
                        }
                    }

                    RestoreListRattingSetting(listSettingInfo);

                    #region
                    //DOC-75090 ADO-15128 Audience,EnterPriseKeyWords,Ratting这三个setting，如果目的端开启，不能随便关闭，否则可能导致目的端数据出现问题
                    try
                    {
                        bool destEnableAudience = mSPList.Fields.Contains(AveFieldId.AudienceTargeting);
                        if (listSettingInfo.EnableAudienceSetting != null && listSettingInfo.EnableAudienceSetting.IsAvailable && listSettingInfo.EnableAudienceSetting.Value && !destEnableAudience)
                        {
                            mSPList.SetAudienceTargetting(true);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error("Restore {0} Audience targeting settings.due to:{1}", Name, e.ToString());
                    }
                    #endregion

                    if (listSettingInfo.EnableKeywordsField != null && listSettingInfo.EnableKeywordsField.IsAvailable && listSettingInfo.EnableKeywordsField.Value ||
                        listSettingInfo.EnableMetadataPromotion != null && listSettingInfo.EnableMetadataPromotion.IsAvailable && listSettingInfo.EnableMetadataPromotion.Value)
                    {
                        try
                        {
                            ProcessEnterPriseKeyWordsSetting(listSettingInfo);
                        }
                        catch (Exception e)
                        {
                            log.Debug($"Process Enterprise keywords setting {e.ToString()}");
                        }

                    }

                    if (listSettingInfo.EnableMetaPublish != null && listSettingInfo.EnableMetaPublish.IsAvailable)
                    {
                        bool sourceEnableMetaPublishing = listSettingInfo.EnableMetaPublish.Value;
                        if (sourceEnableMetaPublishing == false)
                        {
                            //delete the eventreceiver if secondary exist
                            PorcessListMetaPublishing();
                        }
                    }

                    if (listSettingInfo.ScheduledItemSetting != null && listSettingInfo.ScheduledItemSetting.IsAvailable && AveSPEnv.IsMoss)
                    {
                        SetScheduledItemSetting(listSettingInfo.ScheduledItemSetting.Value);
                    }

                    SetTitleAndDescriptionResource(mSPList, listSettingInfo);
                    mSPList.Update();
                    if (listSettingInfo.EmailAlias != null && listSettingInfo.EmailAlias.IsAvailable && !string.IsNullOrEmpty(listSettingInfo.EmailAlias.Value))
                    {
                        mSPList.EmailAlias = listSettingInfo.EmailAlias.Value;
                        try
                        {
                            mSPList.EmailAlias = listSettingInfo.EmailAlias.Value;
                            mSPList.Update();
                        }
                        catch (Exception ex)
                        {
                            reportor.AddDetail(new AveWrapperReportDto(mSPList.Title, mSPList.Title, AveReportObjectType.ListProperty, AveStatus.Failed, string.Format("list property can not restore successfully. ListTitle:{0},Exception:{1}.", mSPList.Title, ex.ToString())));
                            log.Log(AveLogLevel.WARN, "list property can not restore successfully. ListTitle:{0},Exception:{1}.", mSPList.Title, ex.ToString());
                        }
                    }
                    //保持list的创建时间一致
                    if (listSettingInfo.Created != null && listSettingInfo.Created.IsAvailable && listSettingInfo.Created.Value != DateTime.MinValue)
                    {
                        UpdateListCreatedByNative(listSettingInfo.Created.Value);
                    }
                    //保持list的last modified time 一致
                    if (listSettingInfo.LastModifiedTime != null && listSettingInfo.LastModifiedTime.IsAvailable && listSettingInfo.LastModifiedTime.Value != DateTime.MinValue)
                    {
                        mAveParentSite.AddUnRestoreListLastModifiedTime(mSPList.ID, listSettingInfo.LastModifiedTime.Value);
                    }
                    //保持list的Author 一致
                    if (listSettingInfo.Author != null && listSettingInfo.Author.IsAvailable && listSettingInfo.Author.Value > 0)
                    {
                        IAvePrincipal principal = mAveSPWeb.ParentSite.SPMembers.FindMember(listSettingInfo.Author.Value.GetValueOrDefault(), true);
                        if (principal != null && principal.PrincipalType == AvePrincipalType.User && !principal.ID.Equals(mSPList.Author.ID))
                        {
                            UpdateListAuthorByNative(mSPList.ParentWeb.ID, mSPList.ID, principal.ID);
                        }
                    }

                    //list的属性中有会影响到column的属性，在还原完属性后需要reload下list，保证column的属性是最新的
                    this.ReloadList();
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, string.Format("Error occurred when Restore list Properties, list title: {0}.\n error message:{1}", mSPList.Title, ex));
                    reportor.AddDetail(new AveWrapperReportDto(listSettingInfo.Title.Value, listSettingInfo.Title.Value, AveReportObjectType.ListProperty, AveStatus.Skipped, "You don't have permission to update List Setting. " + ex.Message));
                }
                catch (Exception ex)
                {
                    reportor.AddDetail(new AveWrapperReportDto(listSettingInfo.Title.Value, listSettingInfo.Title.Value, AveReportObjectType.ListProperty, AveStatus.Failed, string.Format("Error occurred when Restore list Properties, list title: {0}.\n error message:{1}", mSPList.Title, ex.Message)));
                    log.Log(AveLogLevel.WARN, string.Format("Error occurred when Restore list Properties, list title: {0}.\n error message:{1}", mSPList.Title, ex));
                    //mLog.Warn("Error happenned when Restore list Properites, list title: {0}. Reason: {1}", mSPList.Title, ex.ToString());
                }
#if PerformanceLog
            }
#endif
        }

        private void SetTitleAndDescriptionResource(IAveList list, AveListSettingInfo listSettingInfo)
        {
            //to do:是否需要还原list title resource
            if (listSettingInfo.TitleResource != null && listSettingInfo.TitleResource.IsAvailable)
            {
                list.TitleResource.SetUserResource(list.ParentWeb, listSettingInfo.TitleResource.Value, !this.IsNewCreated);
                list.TitleResource.Update();
            }
            if (listSettingInfo.DescriptionResource != null && listSettingInfo.DescriptionResource.IsAvailable)
            {
                list.DescriptionResource.SetUserResource(list.ParentWeb, listSettingInfo.DescriptionResource.Value, !this.IsNewCreated);
                list.DescriptionResource.Update();
            }
        }

        private void SetScheduledItemSetting(bool isScheduledItemSetting)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.SetScheduledItemSetting"))
            {
#endif
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
                    reportor.AddDetail(new AveWrapperReportDto(mSPList.Title, mSPList.Title, AveReportObjectType.ListScheduledItemSetting, AveStatus.Failed, string.Format("An error happened while SetScheduledItemSetting. Exception:{0}", ex.Message)));
                    log.Info("An error happened while SetScheduledItemSetting. Exception: " + ex.ToString());
                }
#if PerformanceLog
            }
#endif
        }

        private void UpdateListCreatedByNative(DateTime created)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.UpdateListCreatedByNative"))
            {
#endif

                if (this.SPList == null)
                {
                    return;
                }
                try
                {
                    this.SPList.UpdateListCreated(created);
                }
                catch (Exception e)
                {
                    reportor.AddDetail(new AveWrapperReportDto(mSPList.Title, mSPList.Title, AveReportObjectType.ListCreatedBy, AveStatus.Failed, string.Format(WrapperRestoreResource.UpdateListFailed, e.Message)));
                    log.Log(AveLogLevel.INFO, WrapperRestoreResource.UpdateListFailed, e);
                }
#if PerformanceLog
            }
#endif
        }

        private void UpdateListAuthorByNative(Guid webId, Guid listId, int author)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.UpdateListAuthorByNative"))
            {
#endif
                if (mAveSPWeb.ParentSite.SPContextKind == AveContextKind.ClientObjectModel)
                {
                    return;
                }
                try
                {
                    mQueryService.UpdateListAuthorByNative(webId, listId, author);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.INFO, WrapperRestoreResource.UpdateListFailed, e);
                }
#if PerformanceLog
            }
#endif
        }

        public void AddDefaultViewUrl(string destDefaultUrl)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.AddDefaultViewUrl"))
            {
#endif
                if (!string.IsNullOrEmpty(mOldDefaultViewUrl))
                {
                    string s = this.ParentWeb.SPWeb.Url;
                    string s1 = this.SPList.ParentWebUrl;
                    string destUrl = this.ParentWeb.SPWeb.Url.Substring(0, this.ParentWeb.SPWeb.Url.Length - this.SPList.ParentWebUrl.Length) + destDefaultUrl;
                    mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddListDefaultViewMapping(mOldDefaultViewUrl, destUrl);
                }
#if PerformanceLog
            }
#endif
        }

        public void RestoreListRattingSetting(AveListSettingInfo listSettingInfo)
        {
            try
            {
                if (listSettingInfo.AllowRatingSetting != null && listSettingInfo.AllowRatingSetting.IsAvailable && mAveParentSite.Publishing != null)//BPOS-D Publishing为null，会抛空引用；
                {
                    bool allowListRatingSetting = listSettingInfo.AllowRatingSetting.Value;
                    Guid averageRatings = AveSPEnv.IsMoss ? mAveParentSite.Publishing.AverageRatings : Guid.Empty;
                    Guid ratingsCount = AveSPEnv.IsMoss ? mAveParentSite.Publishing.RatingsCount : Guid.Empty;
                    bool destAllow = mSPList.Fields.Contains(averageRatings) && mSPList.Fields.Contains(ratingsCount);
                    if (!allowListRatingSetting || allowListRatingSetting == destAllow)
                    {
                        return;
                    }
                    AveRatingSettingType ratingExperience = AveRatingSettingType.Likes;
                    if (listSettingInfo.RatingExperience != null && listSettingInfo.RatingExperience.IsAvailable)
                    {
                        ratingExperience = (AveRatingSettingType)Enum.Parse(typeof(AveRatingSettingType), listSettingInfo.RatingExperience.Value);
                    }
                    mSPList.SetRatingSettings(true, ratingExperience);
                }
            }
            catch (AveSecurityTrimingException)
            {
                throw;
            }
            catch (Exception e)
            {
                log.Error("Restore list rating setting failed.due to", e.ToString());
            }

        }

        /// <summary>
        /// add for process list rating setting
        /// </summary>
        /// <param name="sourceEnable"></param>
        /// <param name="destEnable"></param>
        public void ProcessListRattingSetting(bool sourceEnable, bool destEnable)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.ProcessListRatingSetting"))
            {
#endif
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
                            if (averageRatings != Guid.Empty && !fields.Contains(averageRatings))
                            {
                                IAveField field = availableFields[averageRatings];
                                mSPList.Fields.AddFieldAsXml(field.SchemaXmlWithResourceTokens, true, AveAddFieldOptions.AddFieldToDefaultView | AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddToAllContentTypes);
                            }
                            if (ratingsCount != Guid.Empty && !fields.Contains(ratingsCount) && availableFields.Contains(ratingsCount))
                            {
                                IAveField field2 = availableFields[ratingsCount];
                                mSPList.Fields.AddFieldAsXml(field2.SchemaXmlWithResourceTokens, false, AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddToAllContentTypes);
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
                    reportor.AddDetail(new AveWrapperReportDto(mSPList.Title, mSPList.Title, AveReportObjectType.ListRatingSetting, AveStatus.Failed, string.Format("Process List Rating setting Error.\n error message:{0}", e.Message)));
                    log.Log(AveLogLevel.WARN, string.Format("Process List Rating setting Error.\n error message:{0}", e));
                    //mLog.Warn("Process List Rating setting Error. Error:{0}", e.ToString());
                }
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// 如果源端没有选择该setting的话，目的端也要将其删除
        /// </summary>
        private void PorcessListMetaPublishing()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.PorcessListMetaPublishing"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }

        private void ProcessEnterPriseKeyWordsSetting(AveListSettingInfo listSettingInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.ProcessEnterPriseKeyWordsSetting"))
            {
#endif
                IAveMetadataListFieldSettings metadataListFieldSettings = this.ParentSite.ObjectModelFactory.CreateMetadataListFieldSettings(this.SPList);
                if (listSettingInfo.EnableKeywordsField != null)
                {
                    metadataListFieldSettings.EnableKeywordsField = listSettingInfo.EnableKeywordsField.Value;
                }
                if (listSettingInfo.EnableMetadataPromotion != null)
                {
                    metadataListFieldSettings.EnableMetadataPromotion = listSettingInfo.EnableMetadataPromotion.Value;
                }
                metadataListFieldSettings.Update();
#if PerformanceLog
            }
#endif
        }

        /*private IAveField GetFieldById(Guid id, IAveFieldCollection fieldColl)
        {
            try
            {
                return fieldColl[id];
            }
            catch (ArgumentException)
            {
                return null;
            }
        }*/

        public void RestoreWelComePage()
        {
            try
            {
                //if (!String.IsNullOrEmpty(mWelComePage))
                //{
                //    SPFile destPage = mSPList.ParentWeb.GetFile(mSPList.RootFolder.ServerRelativeUrl + "\\" + mWelComePage);
                //    if (destPage.Exists)
                //    {
                //        mSPList.RootFolder.Properties["vti_welcomepage"] = mWelComePage;
                //        mSPList.RootFolder.Update();

                //        Hashtable temHs = (Hashtable)destPage.Item.Properties.Clone();

                //        foreach (string key in temHs.Keys)
                //        {
                //            destPage.Item.Properties[key] = temHs[key];
                //        }
                //        destPage.Item.Update();
                //        mSPList.Update();


                //    }
                //    mWelComePage = null;
                //}
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, string.Format("An error occurred while restoring list welcomePage. ListId:{0}, ListTitle:{1}\n error message:{2}", mSPList == null ? Guid.Empty : mSPList.ID,
                    mSPList == null ? string.Empty : mSPList.Title, e));
                //mLog.Warn(e, "An error occurred while restoring list welComePage. ListId:{0}, ListTitle:{1}",
                //     mSPList == null ? Guid.Empty : mSPList.ID,
                //    mSPList == null || mSPList == null ? string.Empty : mSPList.Title);
            }
        }

        public void RestoreDocumentTemplateUrl()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.RestoreDocumentTemplateUrl"))
            {
#endif
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
                        mDocumentTemplateUrl = mDocumentTemplateUrl.ToLower();
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
                        reportor.AddDetail(new AveWrapperReportDto("DocumentTemplateUrl", "DocumentTemplateUrl", AveReportObjectType.DocumentTemplateUrl, AveStatus.Skipped, "You don't have permission to restore document template url. " + ex.Message));

                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, WrapperRestoreResource.UpdateDocumentTemplateUrlFailed, mSPList.Title, mDocumentTemplateUrl, e);
                        //mLog.Warn("An error occurred while update DocumentTemplateUrl. list title:{0}, source DocumentTemplateUrl:{1}", mSPList.Title, mDocumentTemplateUrl);
                    }
                }

#if PerformanceLog
            }
#endif
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property")]
        internal void RestoreDocumentsFromDropOffZone(bool enableRoute)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.RestoreDocumentsFromDropOffZone"))
            {
#endif
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
                    if (mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.ItemIdMapping.TryGetValue(list.ID, out itemsInDropOffLibrary))
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
#if PerformanceLog
            }
#endif
        }

        public bool IsConfictWithRecycle(string name, Guid WebId, IAveBackupRestoreQueryService queryService)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.IsConfictWithRecycle"))
            {
#endif
                if (!mAveParentSite.ObjectModelFactory.IsSPInstalled)
                {
                    return false;
                }
                if (queryService == null) //目的端为Office 365时防止抛空引用，job completed with exception，这里暂时控制一下。
                {
                    return false;
                }
                else
                {
                    return queryService.IsConfictWithRecycle(name, WebId);
                }
#if PerformanceLog
            }
#endif
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ACTs is a part of xml")]
        public void RestoreListRootFolder()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.RestoreListRootFolder"))
            {
#endif
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
                            if (mSPList.RootFolder.Properties != null)
                            {
                                needUpdate = true;

                                string[] keyNames = new string[] { "vti_rss_DocumentAsLink", "vti_rss_ChannelTitle", "vti_rss_ChannelDescription",
                                                       "vti_rss_DayLimit", "vti_rss_ItemLimit", "vti_rss_LimitDescriptionLength", 
                                                        "vti_rss_DocumentAsEnclosure","vti_rss_ChannelImageUrl" };
                                restoredProperties.AddRange(keyNames);
                                foreach (string key in keyNames)
                                {
                                    if (mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.ContainsKey(key))
                                    {
                                        if ("vti_rss_ChannelImageUrl".Equals(key, StringComparison.OrdinalIgnoreCase))
                                        {
                                            var newUrl = AveReplaceProcessor.UrlReplace(
                                                mListSettingInfo.RootFolderInfo.Value.MetaInfoDic[key].ToString(), ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveSPWeb.ParentSite.SourceSiteInfo, mAveSPWeb.ParentSite.ServerRelativeUrl);
                                            if(newUrl.Contains('?'))//替换Url后面的参数，目前没发现此Url存在ListId或者Item RowId，暂不放在PostAction里处理。TODO：AveReplaceProcessor.SuffixReplace方法应该重构到UrlRepace方法里。
                                            {
                                                bool needReplaceLast = false;
                                                newUrl = AveReplaceProcessor.SuffixReplace(newUrl, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager,
                                                    mAveParentSite.SourceSiteInfo, mAveParentSite.ServerRelativeUrl, ref needReplaceLast);
                                                mSPList.RootFolder.Properties[key] = newUrl;
                                            }
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
                                        mSPList.RootFolder.Properties[property] = mListSettingInfo.RootFolderInfo.Value.MetaInfoDic[property];
                                    }
                                }
                                //ecm_AutoDeclareRecords必须先设置为false，不然对还原item有影响，在list postAcion中还原。
                                if (mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.ContainsKey("ecm_AutoDeclareRecords"))
                                {
                                    mAutoDeclareRecord = Boolean.Parse(mListSettingInfo.RootFolderInfo.Value.MetaInfoDic["ecm_AutoDeclareRecords"].ToString());
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
                                //url of the page save the field defualt vale setting  
                                if (mListSettingInfo.RootFolderInfo.Value.MetaInfoDic.ContainsKey("client_LocationBasedMetadataDefaults_file"))
                                {
                                    mSPList.RootFolder.Properties["client_LocationBasedMetadataDefaults_file"] = AveReplaceProcessor.UrlReplace(
                                             mListSettingInfo.RootFolderInfo.Value.MetaInfoDic["client_LocationBasedMetadataDefaults_file"].ToString(), ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), mAveSPWeb.ParentSite.SourceSiteInfo, mAveSPWeb.ParentSite.ServerRelativeUrl);
                                    restoredProperties.Add("client_LocationBasedMetadataDefaults_file");
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
                                    //SAAS-37206 针对特殊数据过滤
                                    if (string.Equals("PowerAppFormProperties", pro, StringComparison.Ordinal))
                                    {
                                        log.Info($"Skip current list root folder PowerAppFormProperties restore.");
                                        continue;
                                    }
                                    if (restoredProperties.BinarySearch(pro, StringComparer.Ordinal) < 0)
                                    {
                                        DateTime listDateTime = new DateTime();
                                        if (DateTime.TryParse(mListSettingInfo.RootFolderInfo.Value.MetaInfoDic[pro].ToString(), out listDateTime))
                                        {
                                            mSPList.RootFolder.Properties[pro] = listDateTime;
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
                    catch (AveExceedStorageLimitException)
                    {
                        throw;
                    }
                    catch (AveSecurityTrimingException ex)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while restoring list root Folder. ListId:{0}, ListTitle:{1}\n error message:{2}", mSPList.ID, mSPList.Title, ex));
                        reportor.AddDetail(new AveWrapperReportDto(mSPList.Title, mSPList.Title, AveReportObjectType.ListRootFolder, AveStatus.Skipped, "You don't have permission to update List RootFolder. " + ex.Message));
                    }
                    catch (Exception e)
                    {
                        reportor.AddDetail(new AveWrapperReportDto(mSPList.Title, mSPList.Title, AveReportObjectType.ListRootFolder, AveStatus.Failed, string.Format("An error occurred while restoring list root Folder. ListId:{0}, ListTitle:{1}\n error message:{2}", mSPList.ID, mSPList.Title, e)));
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while restoring list root Folder. ListId:{0}, ListTitle:{1}\n error message:{2}", mSPList.ID, mSPList.Title, e));
                        //mLog.Warn(e, "An error occurred while restoring list root Folder. ListId:{0}, ListTitle:{1}", mSPList.ID, mSPList.Title);
                    }
                }
#if PerformanceLog
            }
#endif
        }

        public void ReloadList()
        {
            ReloadList(mId);
        }

        private void ReloadList(Guid listid)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.ReloadList"))
            {
#endif
                try
                {
                    if (mSPList != null)
                    {
                        mSPList.Reload();
                        //if (!this.ParentWeb.SPWeb.AllowUnsafeUpdates)
                        //{
                        //    this.ParentWeb.SPWeb.AllowUnsafeUpdates = true;
                        //}
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.ReloadListError, e.ToString());
                }
#if PerformanceLog
            }
#endif
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
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.GetSPListTemplateByCustomListTemplateName"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }

        private int CreateList(string title)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.CreateList"))
            {
#endif
                Guid featureId = mListInfo.TemplateFeatureId;
                string description = string.Empty;
                int listTemplate = mListInfo.BaseTemplate;
                int listBaseType = mListInfo.BaseType;
                Guid id = Guid.Empty;
                int dstListTemplate = listTemplate;
                //获取custom template mapping
                TemplateKeyInfo templateInfo = new TemplateKeyInfo(TemplateMappingLevel.List, "", listTemplate.ToString());
                string mappingTemplate = ParentSite.TemplateMapping.GetMappingTemplateBeforeAdd(templateInfo);
                //是否是CustomTemplate的mapping
                bool useCustomTemplate = false;
                if (int.TryParse(mappingTemplate, out dstListTemplate))
                {
                    mappingTemplate = ((AveListTemplateType)dstListTemplate).ToString();
                }
                else
                {
                    useCustomTemplate = true;
                }
                if (!mappingTemplate.Equals(((AveListTemplateType)listTemplate).ToString(), StringComparison.OrdinalIgnoreCase) && !useCustomTemplate)
                {
                    // to do set template
                    listTemplate = dstListTemplate;
                }
                string url = mListInfo.ServerRelativeUrl;
                url = AveReplaceProcessor.UrlReplace(url, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.WebUrlMapping, new ReplaceOption(true), mAveSPWeb.ParentSite.SourceSiteInfo, mAveSPWeb.ParentSite.ServerRelativeUrl);
                //AOSBR-602
                //if (IsListUrlBeingUsed(url))
                //{
                //   url = null;
                //}
                if (url.StartsWith(mAveSPWeb.SPWeb.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                {
                    url = url.Substring(mAveSPWeb.SPWeb.ServerRelativeUrl.Length);
                }
                //if (url != null)
                //{
                url = url.TrimStart('/');
                //}

                if (mListInfo.BaseTemplate == (int)AveListTemplateType.ExternalList)
                {
                    try
                    {
                        id = mAveSPWeb.SPWeb.Lists.Add(title, description, url, mListInfo.DataSourceXml);
                    }
                    catch (Exception ex)//同SiteCollection下出现异常 ADO-19168
                    {
                        if (ex.Message.Contains("Access denied. You do not have permission to perform this action or access this resource.") || ex.Message.Contains("The remote server returned an error: (401) Unauthorized.") || ex.InnerException.Message.Contains("Access denied. You do not have permission to perform this action or access this resource.") || ex.InnerException.Message.Contains("The remote server returned an error: (401) Unauthorized."))
                        {
                            log.Warn("Create list failed,due to {0}", ex.ToString());
                            throw new AveSecurityTrimingException(WrapperReportResourceKey.Wrapper_AccessDenied.ToString(), AvePoint.Wrapper.Resource.WrapperRestoreReportResource.Wrapper_AccessDenied);
                        }
                        log.Warn("Create list with error:  " + "  " + url + "   " + title + "  " + ex.ToString());
                        throw;
                        //int index = url.IndexOf("/", StringComparison.OrdinalIgnoreCase);
                        //url = url.Substring(0, index + 1) + title;
                        //for (int i = 1; i < 1000; i++)
                        //{
                        //    try
                        //    {
                        //        id = mAveSPWeb.SPWeb.Lists.Add(title, description, url, mListInfo.DataSourceXml);
                        //        break;
                        //    }
                        //    catch (Exception e)//目的端根据title拼出的Url被占用，参照SharePoint，在Title后面不断加1
                        //    {
                        //        if (e.Message.Contains("Access denied. You do not have permission to perform this action or access this resource.") || e.Message.Contains("The remote server returned an error: (401) Unauthorized.") || e.InnerException.Message.Contains("Access denied. You do not have permission to perform this action or access this resource.") || e.InnerException.Message.Contains("The remote server returned an error: (401) Unauthorized."))
                        //        {
                        //            log.Warn("Create list failed,due to {0}", e.ToString());
                        //            throw new AveSecurityTrimingException(WrapperReportResourceKey.Wrapper_AccessDenied.ToString(), AvePoint.Wrapper.Resource.WrapperRestoreReportResource.Wrapper_AccessDenied);
                        //        }
                        //        log.Warn("Create list with error:  " + "  " + url + "   " + title + "  " + e.ToString());
                        //        url = url + i.ToString();
                        //        if (i == 999)
                        //        {
                        //            throw;
                        //        }
                        //    }
                        //}
                    }
                }
                else
                {
                    IAveListTemplate template = null;
                    if (useCustomTemplate)
                    {
                        template = GetSPListTemplateByCustomListTemplateName(mAveSPWeb.SPWeb, mappingTemplate);
                    }
                    else if (featureId != Guid.Empty)
                    {
                        //mAveSPWeb.ReloadWeb(); improve performance
                        //template = GetSPListTemplateByFeatureId(mAveSPWeb.SPWeb, featureId, listTemplate);
                        if (this.mRestoreOption.mAveListRestoreOption.VerifyListTemplateFeature)
                        {
                            IAveFeatureDefinition definition = null;
                            try
                            {
                                if (mAveSPWeb.ParentSite.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel &&
                                    mAveSPWeb.SPWeb.Site.WebApplication != null && mAveSPWeb.SPWeb.Site.WebApplication.Farm != null &&
                                    mAveSPWeb.SPWeb.Site.WebApplication.Farm.FeatureDefinitions != null)
                                {
                                    definition = mAveSPWeb.SPWeb.Site.WebApplication.Farm.FeatureDefinitions[featureId];
                                }

                                if (definition != null)
                                {
                                    log.Debug("The scope of feature:{0} with id:{1} is :{2}", definition.DisplayName, definition.ID, definition.Scope);
                                    if (definition.Scope == AveFeatureScope.Site)
                                    {
                                        if (mAveSPWeb.SPWeb.Site.Features[featureId] == null)
                                        {
                                            mAveSPWeb.SPWeb.Site.Features.Add(featureId, false);
                                            mAveSPWeb.SPWeb.Site.Update();
                                        }
                                    }
                                    else
                                    {
                                        if (mAveSPWeb.SPWeb.Features[featureId] == null)
                                        {
                                            mAveSPWeb.SPWeb.Features.Add(featureId, false);
                                            mAveSPWeb.SPWeb.Update();
                                        }
                                    }
                                }
                                else if (mAveSPWeb.ParentSite.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel)
                                {
                                    if (mAveSPWeb.SPWeb.Features[featureId] == null)
                                    {
                                        mAveSPWeb.SPWeb.Features.Add(featureId, false);
                                        mAveSPWeb.SPWeb.Update();
                                    }
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
                                log.Warn("Create List:{0} by activate feature:{1} failed:{2}", mName, mListInfo.TemplateFeatureId, ex.ToString());
                            }
                        }
                    }
                    if (mSPList == null)
                    {
                        if (template != null)
                        {
                            string associatedFeatureId = string.Empty;
                            try
                            {
                                associatedFeatureId = template.FeatureId.ToString();
                                //由于存在mapping所以不能使用 listTemplate 要使用template.Type_Client
                                id = mAveSPWeb.SPWeb.Lists.Add(title, description, url, associatedFeatureId, template.Type_Client, null, AveQuickLaunchOptions.Off);
                            }
                            catch (SPListExistException existException)
                            {
                                log.Log(AveLogLevel.WARN, WrapperRestoreResource.CreateListError, existException.ToString());
                                throw;
                            }
                            catch (SPUniqueListInstanceException uniqueException)
                            {
                                log.Log(AveLogLevel.WARN, WrapperRestoreResource.CreateListError, uniqueException.ToString());
                                throw;
                            }
                            catch (Exception e)
                            {
                                if (e.Message.Contains("Access denied. You do not have permission to perform this action or access this resource.") || e.Message.Contains("The remote server returned an error: (401) Unauthorized.") || e.InnerException.Message.Contains("Access denied. You do not have permission to perform this action or access this resource.") || e.InnerException.Message.Contains("The remote server returned an error: (401) Unauthorized."))
                                {
                                    log.Warn("Create list failed,due to {0}", e.ToString());
                                    throw new AveSecurityTrimingException(WrapperReportResourceKey.Wrapper_AccessDenied.ToString(), AvePoint.Wrapper.Resource.WrapperRestoreReportResource.Wrapper_AccessDenied);
                                }
                                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.CreateListError, e.ToString());
                                id = mAveSPWeb.SPWeb.Lists.Add(title, description, template, associatedFeatureId);
                            }
                        }
                        else
                        {
                            try
                            {
                                if (featureId != Guid.Empty)
                                {
                                    id = mAveSPWeb.SPWeb.Lists.Add(title, description, url, featureId.ToString(), (int)listTemplate, null, AveQuickLaunchOptions.Off);
                                }
                                else
                                {
                                    //使用源端的template来创建
                                    AveListTemplateType refListTemplate = (AveListTemplateType)listTemplate;
                                    object associatedFeatureId = mAveParentSite.ObjectModelFactory.CreateLegacyListTemplate().LookupAssociatedFeatureId(ref refListTemplate);
                                    if (associatedFeatureId != null)
                                    {
                                        id = mAveSPWeb.SPWeb.Lists.Add(title, description, url, associatedFeatureId.ToString(), (int)listTemplate, null, AveQuickLaunchOptions.Off);
                                    }
                                    else
                                    {
                                        id = mAveSPWeb.SPWeb.Lists.Add(title, description, url, featureId.ToString(), (int)listTemplate, null, AveQuickLaunchOptions.Off);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                if (
                                    e.Message.Contains(
                                        "Access denied. You do not have permission to perform this action or access this resource.") ||
                                    e.Message.Contains("The remote server returned an error: (401) Unauthorized.") ||
                                    (e.InnerException!=null&&e.InnerException.Message.Contains(
                                        "Access denied. You do not have permission to perform this action or access this resource.")) ||
                                    (e.InnerException != null && e.InnerException.Message.Contains(
                                        "The remote server returned an error: (401) Unauthorized.")))
                                {
                                    log.Warn("create list failed,due to {0}", e.ToString());
                                    throw new AveSecurityTrimingException(WrapperReportResourceKey.Wrapper_AccessDenied.ToString(), AvePoint.Wrapper.Resource.WrapperRestoreReportResource.Wrapper_AccessDenied);
                                }
                                log.Log(AveLogLevel.WARN, WrapperRestoreResource.CreateListError, e.ToString());
                                id = mAveSPWeb.SPWeb.Lists.Add(title, description, (AveListTemplateType)listTemplate);
                            }
                        }
                    }
                }
                mIsNewCreated = true;
                mSPList = mAveSPWeb.SPWeb.Lists[id];
                mId = id;
                return 0;
#if PerformanceLog
            }
#endif
        }

        private void CheckListConflict(AveListInfo listInfo)
        {
            try
            {
                bool findListByTitle = false;
                bool findListByUrl = false;
                string listTitle = mName;
                StringBuilder logBuilder = new StringBuilder();
                logBuilder.AppendFormat("[SAAS-30604]Check list conflict result:");
                logBuilder.AppendLine(string.Format("SourceListInfo:[{0}][{1}][{2}][ActualListTitleUsed:{3}]", listInfo.Title, listInfo.Url, listInfo.BaseTemplate, mName));
                var titleList = FindListByTitle(listTitle, mLanguageMappingName);
                if (titleList != null)
                {
                    logBuilder.AppendLine("");
                    logBuilder.AppendFormat("Find list by title [{0}] success.FoundListInformation:[{1}][{2}][{3}]", mName, titleList.Title, titleList.RootFolder.Url, titleList.BaseTemplate);
                    findListByTitle = true;
                }
                else
                {
                    logBuilder.AppendLine("");
                    logBuilder.AppendFormat($"List with title [{mName}][{mLanguageMappingName}] was not found in destination.");
                }
                string url = AveReplaceProcessor.UrlReplace(listInfo.ServerRelativeUrl, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.WebUrlMapping,
                new ReplaceOption(true), mAveSPWeb.ParentSite.SourceSiteInfo, mAveSPWeb.ParentSite.ServerRelativeUrl);
                var urlList = FindListByUrl(url);
                if (urlList != null)
                {
                    logBuilder.AppendLine("");
                    logBuilder.AppendFormat("Find list by url {0} success.FoundListInformation:[{1}][{2}][{3}]", url, urlList.Title, urlList.RootFolder.Url, urlList.BaseTemplate);
                    findListByUrl = true;
                }
                else
                {
                    logBuilder.AppendLine("");
                    logBuilder.AppendFormat("List with url [{0}] was not found in destination.", url);
                }
                logBuilder.AppendLine(string.Format("Find list result.FindListByTitle:{0},FindListByUrl:{1}", findListByTitle, findListByUrl));
                bool needOutputAllListInformation = false;
                if (findListByUrl)
                {
                    if (!findListByTitle)
                    {
                        needOutputAllListInformation = true;
                        //url找到了，但是title没找到，很可能是有问题，需要把所
                    }
                    else
                    {
                        //都找到了，正常case
                    }
                }
                if (needOutputAllListInformation)
                {
                    mAveSPWeb.OutputAllListInformation(logBuilder);
                    //reload一下，看看能不能解决掉
                    mAveSPWeb.SPWeb.ReloadWeb();
                }
                log.Info(logBuilder.ToString());
            }
            catch (Exception e)
            {
                log.Warn("Check list conflict has error.SourceListInfo:[{0}][{1}][{2}][ActualListTitleUsed:{3}],Error:{4}", listInfo.Title, listInfo.Url, listInfo.BaseTemplate, mName, e);
            }
        }

        public void RestoreListSelf(AveListInfo listInfo, bool allowRestoreToSameList, ListRestoreOption findListOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.RestoreListSelf"))
            {
#endif
            mAveSPWeb.ReloadWebAndParentInternalForSPRequestTimeout(false);
            mOldId = listInfo.Id;

            if (string.Compare(mName, "{System Folder}", StringComparison.OrdinalIgnoreCase) == 0)
            {
                mListInfo = listInfo;
                //need do test if some information has to be restored.
                //maybe need not.
                return;
            }

            //用于判断该list是否为O365Group的Sitepages，是否开启了Custom Script
            CheckNeedSkipList(listInfo);
            mListInfo = listInfo;
            if (ParentSite.MappingManager.ListMappingManager.ListTemplateMapping.ContainsKey(listInfo.BaseTemplate))
            {
                listInfo.BaseTemplate = ParentSite.MappingManager.ListMappingManager.ListTemplateMapping[listInfo.BaseTemplate];
                mListInfo.BaseTemplate = listInfo.BaseTemplate;
            }
            try
            {
                if (listInfo.BaseTemplate == 1310) //PreservationHoldLibrary, this library may auto-created when restoring other list/library, need to reload the web to refresh the lists collection
                {
                    System.Text.StringBuilder logBuilder = new System.Text.StringBuilder();
                    logBuilder.AppendLine("[SAAS-35097] Output all lists before reloading web.");
                    mAveSPWeb.OutputAllListInformation(logBuilder);
                    mAveSPWeb.ReloadWeb();
                    logBuilder.AppendLine("[SAAS-35097] Output all lists after reloading web.");
                    mAveSPWeb.OutputAllListInformation(logBuilder);
                    log.Info(logBuilder.ToString());
                }
                CheckListConflict(listInfo);
                mSPList = FindList(findListOption, listInfo, allowRestoreToSameList);
                if (SPList == null && (IsCatalogTemplate(listInfo) || IsUniqueListTemplate(listInfo)))
                {
                    ReloadWebAndOutputInformation();
                    mSPList = FindList(findListOption, listInfo, allowRestoreToSameList);
                }
                if (mSPList == null)
                {
                    mName = GetAvailableListTitle(mName);
                    throw new ArgumentException("ListDoesNotExist: " + mName);
                }
                CheckListTemplateConflict(listInfo.BaseType, listInfo.BaseTemplate, listInfo.TemplateFeatureId, mSPList);

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
                        return;
                    }
                }
                if (CheckListTemplateNeedCreated())
                {
                    try
                    {
                        CreateList(mName);
                    }
                    catch (SPListExistException ex)
                    {
                        log.Warn("create list failed:{0}", ex);
                        //For error:SAAS-38268   A list, survey, discussion board, or document library with the specified title already exists in this Web site.  Please choose another title.
                        if (ex.ServerErrorCode == -2130575342)
                        {
                            ReloadWebListsAndFindCurrent(listInfo, allowRestoreToSameList, findListOption);
                        }
                        //mAveSPWeb.SPWeb.Lists.GetByTitle(mListInfo.Title);//TODO Long
                        if (ParentWeb.WebInfo.LCID != ParentWeb.SPWeb.Language)
                        {
                            var lcidName = new CultureInfo((int)ParentWeb.SPWeb.Language).Name;
                            mAveSPWeb.SPWeb.LoadListTitleResource(lcidName);
                            foreach (var list in mAveSPWeb.SPWeb.Lists)
                            {
                                var titleForLCID = list.TitleResource.GetValueForUICulture(lcidName);
                                if (mName.Equals(titleForLCID, StringComparison.OrdinalIgnoreCase))
                                {
                                    mSPList = list;
                                    break;
                                }
                            }
                        }

                        if (mSPList == null)
                        {
                            throw;
                        }
                    }
                    catch (SPUniqueListInstanceException ex)
                    {
                        log.Warn("create list failed:{0}", ex);
                        mAveSPWeb.SPWeb.Lists.FirstOrDefault(list =>
                        {
                            if (list.BaseTemplate == (AveListTemplateType)mListInfo.BaseTemplate)
                            {
                                mSPList = list;
                                mId = mSPList.ID;
                                mSPList.Title = mName;
                                mSPList.Update();
                                return true;
                            }
                            return false;
                        });

                        if (mSPList == null)
                        {
                            throw;
                        }
                    }
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
                        rootWeb.Properties["TaxonomyHiddenList"] = mSPList?.ID.ToString();
                    }
                    else
                    {
                        rootWeb.Properties.Add("TaxonomyHiddenList", mSPList?.ID.ToString());
                    }
                    rootWeb.Properties.Update();
                }
                mIsTaxonomyList = true;
            }
            if (listInfo.IsCommunitySiteDiscussionList)
            {
                isCommunitySiteDiscussionList = true;
                ParentWeb.CommunitySiteDiscussionsListTitle = mName;
            }
            try
            {
                string webRootFolderUrl = mAveSPWeb.SPWeb.RootFolder.ServerRelativeUrl;
                Common.ArgumentCheck.CheckNotNull(mSPList);
                string listRootFolderUrl = mSPList?.RootFolder.ServerRelativeUrl;
                ArgumentCheck.CheckNotNull(listRootFolderUrl);
                mUrl = mAveSPWeb.SPWeb.Url.TrimEnd('/') + "/" + listRootFolderUrl?.Substring(webRootFolderUrl.Length).Trim('/');
                mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddAbsoluteUrlMapping(mListInfo.Url, mUrl);
                mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddListUrlMapping(mListInfo.ServerRelativeUrl, listRootFolderUrl);
                mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddListIdMapping(mListInfo.Id, mSPList.ID);
                    mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.AddListTitleMapping(mAveSPWeb.SPWeb.ID, mListInfo.Title, mSPList.Title);
                log.Info("AddListIDMapping when RESTORE LIST SELF listname:{0}", mName);
                this.AveFields.LoadExistLookupFields();
                //for those lookup fields whose lookup list is his parent list, update its list property to the source list, in case the parent list is skipped and the mapping is not corrected
                if (mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.LookupFieldCache.ContainsKey(mSPList.ID))
                {
                    foreach (KeyValuePair<Guid, AveLookupObject> lookupFieldCache in mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.LookupFieldCache[mSPList.ID])
                    {
                        if (lookupFieldCache.Value.List.Contains(lookupFieldCache.Value.ListId.ToString()))      //lookup list is the list itself
                        {
                            lookupFieldCache.Value.List = lookupFieldCache.Value.List.Replace(lookupFieldCache.Value.ListId.ToString(), mListInfo.Id.ToString());
                        }
                    }
                }
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
#if PerformanceLog
            }
#endif
        }

        private bool IsUniqueListTemplate(AveListInfo listInfo)
        {
            return listInfo == null ? false : uniqueListTemplates.Contains(listInfo.BaseTemplate);
        }

        private bool IsCatalogTemplate(AveListInfo listInfo)
        {
            return listInfo == null ? false : catalogTemplates.Contains(listInfo.BaseTemplate);
        }

        private void ReloadWebListsAndFindCurrent(AveListInfo listInfo, bool allowRestoreToSameList, ListRestoreOption findListOption)
        {
            try
            {
                log.Info("[SAAS-38268]Try to reload web lists and find current.");
                ReloadWebAndOutputInformation();
                CheckListConflict(listInfo);
                mSPList = FindList(findListOption, listInfo, allowRestoreToSameList);
                if (mSPList == null)
                {
                    mName = GetAvailableListTitle(mName);
                    log.Warn($"[SAAS-38268]After reloading web lists, this list still does not exist:{mName}. ");
                    return;
                }
                else
                {
                    log.Info($"[SAAS-38268]After reloading web lists, this list:{mName} can be found.");
                }
                CheckListTemplateConflict(listInfo.BaseType, listInfo.BaseTemplate, listInfo.TemplateFeatureId, mSPList);
                mId = mSPList.ID;
            }
            catch (Exception e)
            {
                log.Error("An error occured when Reload Web Lists And Find Current, error:{0}", e);
            }
        }

        private void ReloadWebAndOutputInformation()
        {
            System.Text.StringBuilder logBuilder = new System.Text.StringBuilder();
            logBuilder.AppendLine("[SAAS-38268] Output all lists before reloading web.");
            mAveSPWeb.OutputAllListInformation(logBuilder);
            mAveSPWeb.ReloadWeb();
            logBuilder.AppendLine("[SAAS-38268] Output all lists after reloading web.");
            mAveSPWeb.OutputAllListInformation(logBuilder);
            log.Info(logBuilder.ToString());
        }
        /// <summary>
        /// 这个函数主要是为了load或者创建基本的list所需要的，如果需要还原setting，请到restore property函数中。
        /// </summary>
        /// <param name="listInfo"></param>
        public void RestoreListSelf(AveListInfo listInfo, bool allowRestoreToSameList = false)
        {
            RestoreListSelf(listInfo, allowRestoreToSameList,(ListRestoreOption)WrapperConfiguration.ListRestoreOption);
        }

        /*private bool IsListUrlBeingUsed(string url)
        {
            return FindListByUrl(url) != null;
        }
*/
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

        private IAveList FindList(ListRestoreOption option, AveListInfo listInfo, bool allowRestoreToSameList)
        {
            if (Guid.Equals(mListInfo.Id, mAveSPWeb.TaxonomyHiddenList))
            {
                return mAveSPWeb.ParentSite.SPSite.RootWeb.Lists["TaxonomyHiddenList"];
            }

            IAveList list = null;
            if ((option & ListRestoreOption.Title) == ListRestoreOption.Title)
            {
                list = FindListByTitle(mName, mLanguageMappingName);
                if (list != null)
                {
                    log.Log(AveLogLevel.INFO, "Find list by title:{0}", list.Title);
                }
                else
                {
                    log.Info("Find list by title failed:{0}", mName);
                }
            }
            if (list == null && (option & ListRestoreOption.Url) == ListRestoreOption.Url)
            {
                string url = listInfo.IsOopRestoreList? listInfo.ServerRelativeUrl : AveReplaceProcessor.UrlReplace(listInfo.ServerRelativeUrl, mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.WebUrlMapping,
                new ReplaceOption(true), mAveSPWeb.ParentSite.SourceSiteInfo, mAveSPWeb.ParentSite.ServerRelativeUrl);
                list = FindListByUrl(url);
                //todo need update list title.
                if (list != null)
                {
                    log.Log(AveLogLevel.INFO, "Find list by url: {0}", url);
                }
                else
                {
                    log.Log(AveLogLevel.INFO, "Find list by url failed: {0}", url);
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
            if (list != null && !allowRestoreToSameList && mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.ListIdMapping.ContainsValue(list.ID))
            {
                log.Info("List is not Null but ListIDMapping contains its ID list:{0}  ID:{1}", mName, list.ID);
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

        private IAveList FindListByTitle(string title, string mappingTitle)
        {
            var list = FindListByTitle(title);
            if (list == null && !string.IsNullOrEmpty(mLanguageMappingName) && title != mappingTitle)
            {
                list = FindListByTitle(mappingTitle);
            }
            return list;
        }

        /// <summary>
        ///meeting user 类型的list，一个meeting site只能有一个实例。创建不出第二个这样的list。
        //Blog中的Categoris和Posts类型只能有一个实例
        //开启publishing feature的Pages
        /// </summary>
        /// <param name="exist"></param>
        /// <returns></returns>
        private bool CheckListTemplateNeedCreated()
        {
            bool needCreate = true;
            if (uniqueListTemplates.Contains(mListInfo.BaseTemplate))
            {
                if (mListInfo.BaseTemplate == (int)AveListTemplateType.Posts && mListInfo.TemplateFeatureId == Guid.Empty)
                {
                    //Basic Search Center类型的web下的Tablist的BaseTamplate与Posts的BaseTamplate相同，但TemplateFeatureId为Empty。
                    return true;
                }
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
            return needCreate;
        }

        /// <summary>
        ///  SAAS-34829 Modfication:
        ///  2 rules:
        ///  #1 DocumentLibrary <-> Document Library
        ///  #2 List(GenericList, Unused, DiscussionBoard, Survey, Issue) <-> List(GenericList, Unused, DiscussionBoard, Survey, Issue)
        ///  Currently Control side is following the rule, however, agent side will only allow Issue(5) to Custom List(0)
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="baseTemplate"></param>
        /// <param name="templateFeatureId"></param>
        /// <param name="list"></param>
        private void CheckListTemplateConflict(int baseType, int baseTemplate, Guid templateFeatureId, IAveList list)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.CheckListTemplateConflict"))
            {
#endif
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
                            throw new Exception(WrapperRestoreReportResource.Wrapper_ListTemplateConflict);
                        }
                    }
                }
                else if (baseType == 5)
                {
                    if ((int)list.BaseType != 0 && (int)list.BaseType != 5)
                    {
                        throw new Exception(WrapperRestoreReportResource.Wrapper_ListTemplateConflict);
                    }
                }
                else
                {
                    if ((int)list.BaseType != baseType && (int)list.BaseTemplate != baseTemplate)
                    {
                        throw new Exception(WrapperRestoreReportResource.Wrapper_ListTemplateConflict);
                    }
                }
#if PerformanceLog
            }
#endif
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
                NeedContinue = false;
                this.mSkipType = AveSkipType.WorkflowRelatedList;
                return true;
            }
            return false;
        }

        private bool SpecialListTemplateNeedSkipped(int baseTemplate)
        {
            if (mAveSPWeb.ParentSite.ObjectModelFactory.ContextKind == AveContextKind.ClientObjectModel)
            {
                if ( (baseTemplate == 544 && !AveSPList.IsHighSpeedMode) ||  //SP2013 MicroFeed, this list could not be created, once "Site Feed" feature is activated, the list will appear in subsite ||
                     baseTemplate == 125 //SP2013 AppData, this list could not be created
                    || baseTemplate == 3300 //SP2016 SharingLinks, it's used for document guest link, no need to restore
                    || baseTemplate == 3415 //_catalogs/wte
                    || baseTemplate == (int)AveListTemplateType.AccessRequest //SAAS-37760   Skip this list template
                     || baseTemplate == (int)AveListTemplateType.ItemReferenceCollection
                      || baseTemplate == (int)AveListTemplateType.ItemReferenceReference
                       || baseTemplate == (int)AveListTemplateType.ItemReferenceReferenceCollection) //Reference list,column is special, some columns do have title, not support now
                {
                    NeedContinue = false;
                    this.mSkipType = AveSkipType.SpecialTemplateList;
                    return true;
                }
            }
            return false;
        }

        //SAAS-12467 ProjectPolicyItemList生成无用数据导致eDiscovery Sets List不可用。
        private bool ProjectPolicyItemListNeedSkipped(string listUrl)
        {
            if (mAveSPWeb.SPWeb.IsRootWeb
                && string.Equals(mAveSPWeb.SPWeb.WebTemplate, "EDISC", StringComparison.OrdinalIgnoreCase)
                && listUrl.EndsWith("/ProjectPolicyItemList", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
        public void GetSPListByOption(AveListInfo listInfo, ListRestoreOption findListOption)
        {
            mListInfo = listInfo;
            mSPList = FindList(findListOption, listInfo, true);
        }
        //skip sepcial lists while office 365 custom script Enabled 
        private bool SkipSpecialListsWhileOffice365CustomScriptDisabled(int baseTemplate)
        {
            if (WrapperConfiguration.WrapperConfigurationForBPOS.SpecialListTemplateIdsUnderPersonalSite.Contains(baseTemplate))
            {
                if (mAveSPWeb.SPWeb.Site.DenyAddAndCustomizePagesStatus)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 检查是否应该skip掉当前list
        /// </summary>
        /// <param name="listInfo"></param>
        private void CheckNeedSkipList(AveListInfo listInfo)
        {
            if (!mAveSPWeb.SPWeb.IsRootWeb && listInfo.RootWebOnly)
            {
                //if this list is root web only and the customer retore it to subSite
                //we just skip this list
                log.Warn(string.Format("Skip restore the list. due to the list is rootweb only.List Title:{0}", mName));
                this.mSkipType = AveSkipType.RootWebOnlyList;
                NeedContinue = false;
                return;
            }
            if (listInfo.BaseTemplate == (int)AveListTemplateType.SiteCollectionAppCatalog && !mAveParentSite.SPSite.EnableSiteAppCatalog)
            {
                log.Warn("The current list:{0} under web:{1} is skipped because en EnableSiteAppCatalog", listInfo.Title, ParentWeb.Name);
                NeedContinue = false;
                throw new Exception("RM_RS_SkipRestoreSiteAppCatalogBecauseUnEnable");
            }
            //判断O365的custom script是否关闭了，是否是O365的sitepages
            if (SkipSpecialListsWhileOffice365CustomScriptDisabled(listInfo.BaseTemplate))
            {
                log.Warn("The current list:{0} under web:{1} is skipped because of the special template", listInfo.Title, ParentWeb.Name);
                NeedContinue = false;
                throw new SkipException(WrapperReportResourceKey.Wrapper_SkipSpecialListsWhileOffice365CustomScriptDisabled.ToString(), WrapperRestoreReportResource.Wrapper_SkipSpecialListsWhileOffice365CustomScriptDisabled, listInfo.Title, ParentWeb.Name);
            }
            //SAAS-22529：判断该list是否为O365Group的SitePages，如果是，则跳过还原此list
            //if (string.Equals(mAveSPWeb.SPWeb.Site.RootWeb.WebTemplate, "GROUP", StringComparison.OrdinalIgnoreCase) &&
            //    listInfo.BaseTemplate == (int)AveListTemplateType.WebPageLibrary &&
            //    mAveSPWeb.SPWeb.Site.DenyAddAndCustomizePagesStatus)
            //{
            //    log.Warn("The current list:{0} under web:{1} is skipped because of it is O365 Group SitePages and it can not be edited", listInfo.Title, ParentWeb.Name);
            //    NeedContinue = false;
            //    throw new SkipException(WrapperReportResourceKey.Wrapper_SkipO365SpecialList.ToString(), WrapperRestoreReportResource.Wrapper_SkipO365SpecialList, listInfo.Title);
            //}

            if (WorkflowRelatedListNeedSkipped(listInfo.BaseTemplate)
                    || SpecialListTemplateNeedSkipped(listInfo.BaseTemplate)
                    || ProjectPolicyItemListNeedSkipped(listInfo.ServerRelativeUrl))
            {
                throw new SkipException(WrapperReportResourceKey.Wrapper_SkipSpecialList.ToString(), WrapperRestoreReportResource.Wrapper_SkipSpecialList);
            }
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

        public void RestoreUnRestoreWebPart()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.RestoreUnRestoreWebPart"))
            {
#endif
                Dictionary<Guid, Dictionary<string, List<object>>> fileWebParts = null;
                //Dictionary<string, List<AveWebPartBaseInfo>> fileWebPartsByListTitle = null;
                //string listTitle = mName;
                //if (mAveSPWeb.ListTitleMapping.ContainsKey(listTitle))
                //{
                //    listTitle = mAveSPWeb.ListTitleMapping[listTitle];
                //}
                //if (mAveSPWeb.UnRestoreWebPartCache.TryGetValue(listTitle, out fileWebPartsByListTitle))
                //{
                //    IAveWeb web = mAveSPWeb.SPWeb;
                //    try
                //    {
                //        foreach (string fileUrl in fileWebPartsByListTitle.Keys)
                //        {
                //            IAveFile file = web.GetFile(fileUrl, false);
                //            AveSPDoc spDoc = new AveSPDoc(mAveSPWeb.ParentSite);
                //            spDoc.Web = web;
                //            spDoc.SPFile = file;
                //            spDoc.RestoreWebPart(fileWebPartsByListTitle[fileUrl], false);

                //            if (mAveSPWeb.ParentSite.WebPartMapping.ContainsKey(fileUrl))
                //            {
                //                mAveSPWeb.ParentSite.WebPartMapping.Remove(fileUrl);
                //            }
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        mLog.Log(AveLogLevel.WARN, string.Format("An error occurred when restore un-restored webpart.\n error message:{0}", ex));
                //        //mLog.Warn("An error occurred when restore un-restored webpart. Reason: {0}.", ex.ToString());
                //    }
                //}
                if (mOldId == Guid.Empty)
                {
                    return;
                }
                if (mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.UnRestoreWebPartCache.TryGetValue(mOldId, out fileWebParts))
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
                                    try
                                    {
                                        IAveFile file = web.GetFile(filePair.Key);
                                        AveSPDoc spDoc = new AveSPDoc(mAveSPWeb.ParentSite);
                                        int userId = -1;
                                        if (file.CheckOutType != AveCheckOutType.None || (mAveParentSite.QueryService != null && mAveParentSite.QueryService.IsCheckOutFile(mAveParentSite.SPSite.ID, file.UniqueId, ref userId) && userId != mAveSPWeb.SPWeb.CurrentUser.ID))
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
                                        spDoc.AveSPItem.SPFile = file;
                                        spDoc.Web = file.Web;
                                        spDoc.SPFile = file;
                                        spDoc.SetRestoreOption(RestoreOption);
                                        spDoc.AveSPItem.ParentList = this;
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
                        reportor.AddDetail(new AveWrapperReportDto("WebPart", "WebPart", AveReportObjectType.WebPart, AveStatus.Skipped, "You don't have permission to restore WebPart. " + ex.Message));
                    }

                    mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.UnRestoreWebPartCache.Remove(mOldId);
                }
#if PerformanceLog
            }
#endif
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
        private long mListVersionSettingFlag;
        private long mListValidationSettingFlag;
        private string mValidationFormula = null;
        private string mValidationMessage = null;
        private bool mAutoDeclareRecord = false;


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
        public List<string> SetNeedSetNullFieldsEx(List<string> fieldValues)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.SetNeedSetNullFields"))
            {

                List<string> needSetNullFields = new List<string>();
                string[] AllCols = new string[] {"nvarchar1" ,"nvarchar2" ,"nvarchar3" ,"nvarchar4" ,"nvarchar5" ,"nvarchar6" ,"nvarchar7" ,"nvarchar8" ,
                "ntext1" ,"ntext2" ,"ntext3" ,"ntext4" ,"sql_variant1","nvarchar9" ,"nvarchar10" ,"nvarchar11" ,"nvarchar12" ,"nvarchar13" ,
                "nvarchar14" ,"nvarchar15" ,"nvarchar16" ,"ntext5" ,"ntext6" ,"ntext7" ,"ntext8" ,"sql_variant2","nvarchar17" ,"nvarchar18" ,
                "nvarchar19" ,"nvarchar20" ,"nvarchar21" ,"nvarchar22" ,"nvarchar23" ,"nvarchar24" ,"ntext9" ,"ntext10" ,"ntext11" ,"ntext12" ,
                "sql_variant3","nvarchar25" ,"nvarchar26" ,"nvarchar27" ,"nvarchar28" ,"nvarchar29" ,"nvarchar30" ,"nvarchar31" ,"nvarchar32" ,
                "ntext13" ,"ntext14" ,"ntext15" ,"ntext16" ,"sql_variant4","nvarchar33" ,"nvarchar34" ,"nvarchar35" ,"nvarchar36" ,"nvarchar37" ,
                "nvarchar38" ,"nvarchar39" ,"nvarchar40" ,"ntext17" ,"ntext18" ,"ntext19" ,"ntext20" ,"sql_variant5","nvarchar41" ,"nvarchar42" ,
                "nvarchar43" ,"nvarchar44" ,"nvarchar45" ,"nvarchar46" ,"nvarchar47" ,"nvarchar48" ,"ntext21" ,"ntext22" ,"ntext23" ,"ntext24" ,
                "sql_variant6","nvarchar49" ,"nvarchar50" ,"nvarchar51" , "nvarchar52" ,"nvarchar53" ,"nvarchar54" ,"nvarchar55" ,"nvarchar56" ,
                "ntext25" ,"ntext26" ,"ntext27" ,"ntext28" ,"sql_variant7","nvarchar57" ,"nvarchar58" ,"nvarchar59" ,"nvarchar60" ,"nvarchar61" ,
                "nvarchar62" ,"nvarchar63" ,"nvarchar64" ,"ntext29" ,"ntext30" ,"ntext31" ,"ntext32" ,"sql_variant8","int1","int2","int3","int4",
                "int5","int6","int7","int8","int9","int10","int11","int12","int13","int14","int15","int16","float1","float2","float3","float4",
                "float5","float6","float7","float8","float9","float10","float11","float12", "datetime1","datetime2","datetime3","datetime4",
                "datetime5","datetime6","datetime7","datetime8","bit1","bit2","bit3","bit4","bit5","bit6","bit7","bit8","bit9","bit10","bit11",
                "bit12","bit13","bit14","bit15","bit16","uniqueidentifier1"};

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
                                && !field.ID.Equals(new Guid("{14c6cd06-7417-42c1-a051-89e455fd1090}")))
                            {
                                string colName = obj.ToString();
                                if (IsColColumn(colName) && IsSupportToSetNull(field.InternalName))
                                {
                                    if (field.Type == AveFieldType.WorkflowStatus || fieldValues.Exists(name => name.Equals(field.InternalName, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        continue;
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
        public List<string> SetNeedSetNullFields(Dictionary<string, object> fieldValues)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.SetNeedSetNullFields"))
            {
#endif
                List<string> needSetNullFields = new List<string>();
                string[] AllCols = new string[]	{"nvarchar1" ,"nvarchar2" ,"nvarchar3" ,"nvarchar4" ,"nvarchar5" ,"nvarchar6" ,"nvarchar7" ,"nvarchar8" ,
                "ntext1" ,"ntext2" ,"ntext3" ,"ntext4" ,"sql_variant1","nvarchar9" ,"nvarchar10" ,"nvarchar11" ,"nvarchar12" ,"nvarchar13" ,
	            "nvarchar14" ,"nvarchar15" ,"nvarchar16" ,"ntext5" ,"ntext6" ,"ntext7" ,"ntext8" ,"sql_variant2","nvarchar17" ,"nvarchar18" ,
                "nvarchar19" ,"nvarchar20" ,"nvarchar21" ,"nvarchar22" ,"nvarchar23" ,"nvarchar24" ,"ntext9" ,"ntext10" ,"ntext11" ,"ntext12" ,
                "sql_variant3","nvarchar25" ,"nvarchar26" ,"nvarchar27" ,"nvarchar28" ,"nvarchar29" ,"nvarchar30" ,"nvarchar31" ,"nvarchar32" ,
                "ntext13" ,"ntext14" ,"ntext15" ,"ntext16" ,"sql_variant4","nvarchar33" ,"nvarchar34" ,"nvarchar35" ,"nvarchar36" ,"nvarchar37" ,
                "nvarchar38" ,"nvarchar39" ,"nvarchar40" ,"ntext17" ,"ntext18" ,"ntext19" ,"ntext20" ,"sql_variant5","nvarchar41" ,"nvarchar42" ,
                "nvarchar43" ,"nvarchar44" ,"nvarchar45" ,"nvarchar46" ,"nvarchar47" ,"nvarchar48" ,"ntext21" ,"ntext22" ,"ntext23" ,"ntext24" ,
                "sql_variant6","nvarchar49" ,"nvarchar50" ,"nvarchar51" , "nvarchar52" ,"nvarchar53" ,"nvarchar54" ,"nvarchar55" ,"nvarchar56" ,
                "ntext25" ,"ntext26" ,"ntext27" ,"ntext28" ,"sql_variant7","nvarchar57" ,"nvarchar58" ,"nvarchar59" ,"nvarchar60" ,"nvarchar61" ,
                "nvarchar62" ,"nvarchar63" ,"nvarchar64" ,"ntext29" ,"ntext30" ,"ntext31" ,"ntext32" ,"sql_variant8","int1","int2","int3","int4",
                "int5","int6","int7","int8","int9","int10","int11","int12","int13","int14","int15","int16","float1","float2","float3","float4",
                "float5","float6","float7","float8","float9","float10","float11","float12", "datetime1","datetime2","datetime3","datetime4",
                "datetime5","datetime6","datetime7","datetime8","bit1","bit2","bit3","bit4","bit5","bit6","bit7","bit8","bit9","bit10","bit11",
                "bit12","bit13","bit14","bit15","bit16","uniqueidentifier1"};

                //ExternalList 没有ColName，会抛异常
                if (mSPList != null && mSPList.BaseTemplate != AveListTemplateType.ExternalList)
                {
                    foreach (IAveField field in mSPList.Fields)
                    {
                        try
                        {
                            object obj = field.ColName;
                            if (obj != null &&
                                (!(field is IAveFieldLookup) || field is IAveTaxonomyField || field is IAveFieldUserValue))// exclude lookup field, or item will fail to update when the dependent list is not restored
                            {
                                string colName = obj.ToString();
                                if (AllCols.Contains(colName) && IsSupportToSetNull(field.InternalName))
                                {
                                    if (field.Type == AveFieldType.WorkflowStatus)
                                    {
                                        continue;
                                    }
                                    //check for SP13 Community Site
                                    if (mSPList.ParentWeb.Site.CompatibilityLevel == 15 && mSPList.ParentWeb.WebTemplateId == 62)
                                    {
                                        if (IsSpecialColumnInCommunitySite(field.InternalName))
                                        {
                                            continue;
                                        }
                                    }
                                    if ((!String.IsNullOrEmpty(field.DefaultValue) || !String.IsNullOrEmpty(field.DefaultFormula)) && KeepDefaultValue)
                                    {
                                        //[ADO-8099]keep default value
                                        if (!string.IsNullOrEmpty(field.DefaultValue) && !fieldValues.ContainsKey(field.InternalName))
                                        {
                                            fieldValues[field.InternalName] = new AveFieldValueInfo() { ColValue = AveFieldHelper.GetFieldDefaultValues(field) };
                                        }
                                        continue;
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
#if PerformanceLog
            }
#endif
        }

        private bool IsSpecialColumnInCommunitySite(string internalName)
        {
            if ((int)SPList.BaseTemplate == 880)  //Community Members
            {
                if (string.Equals(internalName, "NumberOfBestResponses", StringComparison.Ordinal)
                    || string.Equals(internalName, "NumberOfDiscussions", StringComparison.Ordinal)
                    || string.Equals(internalName, "NumberOfReplies", StringComparison.Ordinal)
                    || string.Equals(internalName, "ReputationScore", StringComparison.Ordinal))
                {
                    return true;
                }
            }
            else if ((int)SPList.BaseTemplate == 500)  //Category
            {
                if (string.Equals(internalName, "ReplyCount", StringComparison.Ordinal)
                    || string.Equals(internalName, "TopicCount", StringComparison.Ordinal)
                    || string.Equals(internalName, "LastPostBy", StringComparison.Ordinal)
                    || string.Equals(internalName, "LastPostDate", StringComparison.Ordinal))
                {
                    return true;
                }
            }
            else if (IsCommunitySiteDiscussionList)
            {
                if (string.Equals(internalName, "Popularity", StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }


        public bool IsReportingMetadataList()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.IsReportingMetadataList"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }

        public bool IsSupportToSetNull(string internalName)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.IsSupportToSetNull"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }
        //public void SetContentTypeMapping(AveContentTypeMapping contentTypeMapping)
        //{
        //    mContentTypes.ContentTypeMapping = contentTypeMapping;
        //}

        public void BackupListSetting()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.BackupListSetting"))
            {
#endif
                //lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("FieldLock"))
                lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ListSettingLocker"))
                {
                    //AveFields.BackupValidationFields();
                    //AveFields.BackupFieldsDefaultValue();
                    //由于web.alerts API获取有误，改用还原时候stopAlert
                    AveSPAlert.StopListAlerts(this);
                    if (mSPList == null || (mListSettingFlag & AveListSettingFlags.LIST_SETTING_BACKUP) != 0)
                    {
                        return;
                    }

                    if (this.ListItemSerializer.BeforeSetObjectData())
                    {
                        ReloadList();
                    }
                    //}
                    //lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ListSettingLocker"))
                    //{

                    mListSettingFlag |= AveListSettingFlags.LIST_SETTING_BACKUP;

                    if (mSPList.EnableVersioning)
                    {
                        mListSettingFlag |= AveListSettingFlags.LIST_ENABLE_VERSIONS;
                    }

                    if (mSPList.EnableMinorVersions)
                    {
                        mListSettingFlag |= AveListSettingFlags.LIST_ENABLE_MINOR_VERSIONS;
                    }

                    if (mSPList.ForceCheckout)
                    {
                        mListSettingFlag |= AveListSettingFlags.LIST_FORCE_CHECK_OUT;
                    }

                    if (mSPList.AllowMultiResponses)
                    {
                        mListSettingFlag |= AveListFlags.ALLOWMULTIPLE_RESPONSES_LIST;
                    }

                    bool changed = false;


                    //if (mSPList.ForceCheckout)
                    //{
                    //    mSPList.ForceCheckout = false;
                    //    changed = true;
                    //}

                    if (!string.IsNullOrEmpty(mSPList.ValidationFormula))
                    {
                        bool isBackupValidationFormula = (mListValidationSettingFlag & AveListSettingFlags.LIST_VALIDATION_FORMULA) != 0;
                        if (!isBackupValidationFormula)
                        {
                            mValidationFormula = mSPList.ValidationFormula;
                            mListValidationSettingFlag |= AveListSettingFlags.LIST_VALIDATION_FORMULA;
                        }
                        mSPList.ValidationFormula = string.Empty;
                        changed = true;
                    }

                    if (mSPList.BaseType == AveBaseType.DocumentLibrary && mSPList.EnableModeration)
                    {
                        mSPList.EnableModeration = false;
                        mDraftVersionVisibility = mSPList.DraftVersionVisibility;
                        changed = true;
                        mListSettingFlag |= AveListSettingFlags.LIST_SETTING_CHANGED;
                        mListSettingFlag |= AveListSettingFlags.LIST_ENABLE_MODERATION;
                        log.Info("CustomLog_BackupListSetting()-list content approval key:[{0}],mListSettingFlag:[{1}],mDraftVersionVisibility:[{2}]", mSPList.EnableModeration,mListSettingFlag,mDraftVersionVisibility);
                    }

                    if (mAveSPWeb != null && mAveSPWeb.ParentSite != null && mAveSPWeb.ParentSite.IsListIncludeEnableAssignEmail(mSPList))
                    {
                        var settingInfo = mAveSPWeb.ParentSite.GetOrCreateEndRestoreListSettingsInfo(mSPList);
                        settingInfo.TargetEnableAssignToEmail = mSPList.EnableAssignToEmail;
                        if (mSPList.EnableAssignToEmail)
                        {
                            mSPList.EnableAssignToEmail = false;
                            changed = true;
                            log.Info("CustomLog_BackupListSetting()-list EnableAssignToEmail:[{0}]", mSPList.EnableAssignToEmail);
                        }
                    }

                    if (!string.IsNullOrEmpty(mSPList.ValidationMessage))
                    {
                        bool isBackupValidationMessage = (mListValidationSettingFlag & AveListSettingFlags.LIST_VALIDATION_MESSAGE) != 0;
                        if (!isBackupValidationMessage)
                        {
                            mValidationMessage = mSPList.ValidationMessage;
                            mListValidationSettingFlag |= AveListSettingFlags.LIST_VALIDATION_MESSAGE;
                        }
                        mSPList.ValidationMessage = string.Empty;
                        changed = true;
                    }
                    if (mSPList.BaseTemplate == AveListTemplateType.Survey && !mSPList.AllowMultiResponses)
                    {
                        mSPList.AllowMultiResponses = true;
                        changed = true;
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
                            //List对象更新后mContentTypes 也需要更新下以保证其version和当前list version一致，否则contentType的Update会抛异常
                            mSPList.ContentTypes.Update();
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

#if PerformanceLog
            }
#endif
        }

        public void DisableListVersionSettings()
        {
            lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ListSettingLocker"))
            {
                if (mSPList != null)
                {
                    bool changed = false;
                    if (mSPList.BaseTemplate == AveListTemplateType.Posts)
                    {
                        mSPList.EnableAttachments = true;
                        changed = true;
                    }
                    if (mSPList.EnableVersioning)
                    {
                        mListVersionSettingFlag |= AveListSettingFlags.LIST_ENABLE_VERSIONS;
                        mSPList.EnableVersioning = false;
                        changed = true;
                    }
                    if (mSPList.EnableMinorVersions)
                    {
                        mListVersionSettingFlag |= AveListSettingFlags.LIST_ENABLE_MINOR_VERSIONS;
                        mSPList.EnableMinorVersions = false;
                        changed = true;
                    }
                    if (changed)
                    {
                        mSPList.Update();
                    }
                }
            }
        }

        public void RevertListVersionSettings()
        {
            if (mSPList != null)
            {
                bool changed = false;
                if ((mListVersionSettingFlag & AveListSettingFlags.LIST_ENABLE_VERSIONS) != 0)
                {
                    mSPList.EnableVersioning = true;
                    changed = true;
                }
                if ((mListVersionSettingFlag & AveListSettingFlags.LIST_ENABLE_MINOR_VERSIONS) != 0)
                {
                    mSPList.EnableMinorVersions = true;
                    changed = true;
                }
                if (changed)
                {
                    mSPList.Update();
                }
            }
        }

        public void RestoreListRootFolderProperties()
        {
            //for 2013 project site's task list.Restore rootfolder properties after restoring items.
            if (this.mAveParentSite.SPSite.APIType == AveAPIType.BPOS_S)
            {
                try
                {

                    if (this.RootFolder != null && this.RootFolder.Properties != null)
                    {
                        foreach (string property in this.RootFolder.Properties.Keys)
                        {
                            if (property.StartsWith("Timeline_", StringComparison.OrdinalIgnoreCase))
                            {
                                log.Info("Update time line property:{0}", property);
                                bool needUpdate = false;
                                XmlDocument doc = new XmlDocument();
                                doc.LoadXml(this.RootFolder.Properties[property].ToString());
                                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                                {
                                    if (node.Name.Equals("tskSet") || node.Name.Equals("mlSet"))
                                    {
                                        foreach (XmlElement childNode in node.ChildNodes)
                                        {
                                            if (childNode.HasAttribute("uid") && !string.IsNullOrEmpty(childNode.Attributes["uid"].Value))
                                            {
                                                int origionalItemId = 0;
                                                if (int.TryParse(childNode.Attributes["uid"].Value, out origionalItemId) &&
                                                    ParentSite.MappingManager.SiteMappingManager.ItemIdMapping.ContainsKey(mSPList.ID)
                                                    && ParentSite.MappingManager.SiteMappingManager.ItemIdMapping[mSPList.ID].ContainsKey(origionalItemId))
                                                {
                                                    childNode.Attributes["uid"].Value = ParentSite.MappingManager.SiteMappingManager.ItemIdMapping[mSPList.ID][origionalItemId].ToString();
                                                    if (!string.Equals(childNode.Attributes["uid"].Value, origionalItemId.ToString(), StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        needUpdate = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (needUpdate)
                                {
                                    this.RootFolder.Properties[property] = doc.OuterXml;
                                    this.RootFolder.Update();
                                }
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Debug("Post restore list root folder failed.Title:{0},Web:{1},Error Message:{3}", this.SPList.Title, this.ParentWeb.ServerRelativeUrl, ex.ToString());
                }
            }
        }

        public void RestoreListSetting()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.RestoreListSetting"))
            {
#endif
                //if (mAveParentSite.SPContextKind == AveContextKind.ClientObjectModel && mModifiedFieldChanged)
                //{
                //    AveFields.EnableModifiedField(true);
                //}
                try
                {
                    //AveFields.RestoreValidationFields();
                    //AveFields.RestoreFieldDefaultValue();
                    if (this.ListItemSerializer.AfterSetObjectData())
                    {
                        ReloadList();
                    }
                    //EnableListAlerts();

                    if (mSPList != null)
                    {
                        bool listSettingChanged = false;
                        bool isBackupValidationFormula = (mListValidationSettingFlag & AveListSettingFlags.LIST_VALIDATION_FORMULA) != 0;
                        if (isBackupValidationFormula && !string.IsNullOrEmpty(mValidationFormula))
                        {
                            var fieldDisplayNameMapping = this.AveFields.FieldMapping.EnumFieldDisplayNameMapping().ToDictionary(pair => pair.Key, pair => pair.Value);
                            mSPList.ValidationFormula = ValidationFormulaParser.Parse(mValidationFormula, fieldDisplayNameMapping);
                            listSettingChanged = true;
                        }
                        bool isBackupValidationMessage = (mListValidationSettingFlag & AveListSettingFlags.LIST_VALIDATION_MESSAGE) != 0;
                        if (isBackupValidationMessage && !string.IsNullOrEmpty(mValidationMessage))
                        {
                            mSPList.ValidationMessage = mValidationMessage;
                            listSettingChanged = true;
                        }
                        #region parse list setting
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
                                mSPList.DraftVersionVisibility = mDraftVersionVisibility;
                                listSettingChanged = true;
                            }

                            log.Info("CustomLog_List Post Action RestoreListSetting()-list content approval key:[{0}],mListSettingFlag:[{1}],mDraftVersionVisibility:[{2}],  mSPList.EnableModeration != enableModeration:[{3}]", mSPList.EnableModeration, mListSettingFlag, mDraftVersionVisibility, mSPList.EnableModeration != enableModeration);

                            bool forceCheckOut = (mListSettingFlag & AveListSettingFlags.LIST_FORCE_CHECK_OUT) != 0;
                            if (forceCheckOut && mSPList.ForceCheckout != forceCheckOut)
                            {
                                mSPList.ForceCheckout = forceCheckOut;
                                listSettingChanged = true;
                            }



                            if (mAutoDeclareRecord)
                            {
                                mSPList.RootFolder.Properties["ecm_AutoDeclareRecords"] = "True";
                                mSPList.RootFolder.Update();
                            }

                            bool allowMultiResponses = (mListSettingFlag & AveListFlags.ALLOWMULTIPLE_RESPONSES_LIST) != 0;
                            if (/*allowMultiResponses && */mSPList.AllowMultiResponses != allowMultiResponses)
                            {
                                mSPList.AllowMultiResponses = allowMultiResponses;
                                listSettingChanged = true;
                            }
                        }
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
                                mListSettingFlag = AveListSettingFlags.LIST_SETTING_NULL;
                                mSPList.Update();
                            }
                            catch (AveSecurityTrimingException)
                            {
                                throw;
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.WARN, string.Format("An error occurred while resetting list setting. ListTitle:{0}\n error message:{1}", mSPList.Title, e));
                                //mLog.Warn(e, "An error occurred while reseting list setting. ListTitle:{0}", mSPList.Title);
                            }
                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    reportor.AddDetail(new AveWrapperReportDto("ListSetting", "ListSetting", AveReportObjectType.ListSetting, AveStatus.Skipped, "You don't have permission to restore List Setting. " + ex.Message));
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while resetting list setting. ListTitle:{0}\n error message:{1}", mSPList.Title, ex));
                }
#if PerformanceLog
            }
#endif
        }
        public void RestoreListComplianceInfo()
        {
            if (mSPList != null && ListSettingInfo != null)
            {
                try
                {
                    if (ListSettingInfo.ComplianceTagInfo != null && ListSettingInfo.ComplianceTagInfo.IsAvailable)
                    {
                        mSPList.SetListComplianceTag(ListSettingInfo.ComplianceTagInfo.Value);
                    }
                    else // set label to None, will overwirte applied lable in target side.
                    {
                        mSPList.SetListComplianceTag(new AveComplianceTagInfo() { TagName = "", BlockEdit = false, BlockDelete = false });
                    }
                }
                catch (Exception e)
                {
                    log.Warn("restore list compliance info failed, title:{0}, error:{0}", mSPList.Title, e);
                }
            }
        }

        public void Update_ReportTemplateistWebProperties()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.Update_ReportTemplateistWebProperties"))
            {
#endif
                if (mAveSPWeb.SPWeb.Properties != null && mAveSPWeb.SPWeb.Properties.ContainsKey("_reportinggallerytemplateid") && mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.ListIdMapping.ContainsKey(new Guid(mAveSPWeb.SPWeb.Properties["_reportinggallerytemplateid"])))
                {
                    if (mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.ListIdMapping[new Guid(mAveSPWeb.SPWeb.Properties["_reportinggallerytemplateid"])] == mSPList.ID)
                    {
                        mAveSPWeb.SPWeb.Properties["_reportinggallerytemplateid"] = mSPList.ID.ToString();
                        mAveSPWeb.SPWeb.Properties.Update();
                    }
                }
                else if (mAveSPWeb.SPWeb.AllProperties.ContainsKey("_reportinggallerytemplateid") && mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.ListIdMapping.ContainsKey(new Guid(mAveSPWeb.SPWeb.AllProperties["_reportinggallerytemplateid"].ToString())))
                {
                    if (mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.ListIdMapping[new Guid(mAveSPWeb.SPWeb.AllProperties["_reportinggallerytemplateid"].ToString())] == mSPList.ID)
                    {
                        mAveSPWeb.SPWeb.AllProperties["_reportinggallerytemplateid"] = mSPList.ID.ToString();
                        mAveSPWeb.SPWeb.Update();
                    }
                }
#if PerformanceLog
            }
#endif
        }

        public void RestoreMetadataNavigationSettings()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.RestoreMetadataNavigationSettings"))
            {
#endif
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
                                if (node.Attributes["CachedName"] != null)
                                {
                                    string viewName = node.Attributes["CachedName"].Value;
                                    IAveView view = null;
                                    try
                                    {
                                        view = mSPList.Views[viewName];
                                    }
                                    catch (Exception e)
                                    {
                                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetListViewByNameError, e.ToString());
                                    }
                                    if (view != null)
                                    {
                                        if (node.Attributes["ViewId"] != null)
                                        {
                                            node.Attributes["ViewId"].Value = view.ID.ToString();
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
                            if (AveSPEnv.IsMoss && this.mAveParentSite.SPContextKind != AveContextKind.ClientObjectModel)
                            {
                                try
                                {
                                    ResetMetaDataNavegationSetting(xDoc.InnerXml);
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
                            reportor.AddDetail(new AveWrapperReportDto("MetadataNavigationSettings", "MetadataNavigationSettings", AveReportObjectType.MetadataNavigationSettings, AveStatus.Skipped, "You don't have permission to restore metadata navigation settings. " + ex.Message));
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, "An error occurred while RestoreMetadataNavigationSettings. error:{0}", e.ToString());
                            //mLog.Warn("An error occurred while RestoreMetadataNavigationSettings. error:{0}", e.ToString());
                        }
                    }
                }
#if PerformanceLog
            }
#endif
        }

        private bool ResetMetaDataNavegationSetting(string innerXml)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.ResetMetaDataNavegationSetting"))
            {
#endif
                bool suc = false;
                IAveOMetadataNavigationSettings setting = mAveParentSite.ObjectModelFactory.CreateMetadataNavigationSettings(innerXml);
                setting.SetMetadataNavigationSettings(mSPList, setting);
                suc = true;
                return suc;
#if PerformanceLog
            }
#endif
        }

        private bool ResetMetadataField(XmlNode node, out string fieldType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.ResetMetadataField"))
            {
#endif
                fieldType = string.Empty;
                if (node.Attributes["CachedName"] != null)
                {
                    string fieldDisplayName = node.Attributes["CachedName"].Value;
                    IAveField field = null;
                    try
                    {
                        field = mSPList.Fields.GetField(fieldDisplayName);
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
                        if (node.Attributes["CachedDisplayName"] != null)
                        {
                            node.Attributes["CachedDisplayName"].Value = field.InternalName;
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
#if PerformanceLog
            }
#endif
        }

        private bool ResetManagedIndex(XmlNode node)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.ResetManagedIndex"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }

        private bool ResetFolderViewSetting(XmlNode node)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.ResetFolderViewSetting"))
            {
#endif
                if (node.Name == "ViewSettings")
                {
                    if (node.Attributes["FolderId"] != null)
                    {
                        int folderId = 0;
                        int.TryParse(node.Attributes["FolderId"].Value, out folderId);
                        if (folderId > 0)
                        {
                            if (mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.ItemIdMapping[mId] != null && mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.ItemIdMapping[mId].ContainsKey(folderId))
                            {
                                int newFolderId = mAveSPWeb.ParentSite.MappingManager.SiteMappingManager.ItemIdMapping[mId][folderId];
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
#if PerformanceLog
            }
#endif
        }

        //目前此方法只还原contentType根节点的view setting，即node.Attributes["UniqueNodeId"].Value=""的情况，
        //原因是在调试还原其下节点的view setting时，导致目的端list的Per-location view settings设置页面打开出错。 
        private bool ResetContentTypeViewSetting(XmlNode node)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.ResetContentTypeViewSetting"))
            {
#endif
                if (node.Name == "ViewSettings")
                {
                    if (node.Attributes["UniqueNodeId"] != null)
                    {
                        string contentTypeId = node.Attributes["UniqueNodeId"].Value.ToString();
                        if (ParentSite.MappingManager.ListMappingManager.ListLevelCTIdMapping != null && ParentSite.MappingManager.ListMappingManager.ListLevelCTIdMapping.ContainsKey(contentTypeId))
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
                    if (node.Attributes["UniqueNodeId"] != null && string.IsNullOrEmpty(node.Attributes["UniqueNodeId"].Value))
                    {
                        return true;
                    }
                }
                return false;
#if PerformanceLog
            }
#endif
        }

        //目前此方法只还原metadata field根节点的view setting，即node.Attributes["UniqueNodeId"].Value=""的情况
        private bool ResetTaxonomyFieldViewSetting(XmlNode node)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.ResetTaxonomyFieldViewSetting"))
            {
#endif
                //wait to do
                if (node.Name == "ViewSettings")
                {
                    if (node.Attributes["UniqueNodeId"] != null && string.IsNullOrEmpty(node.Attributes["UniqueNodeId"].Value))
                    {
                        return true;
                    }
                }
                return false;
#if PerformanceLog
            }
#endif
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
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.DeleteItemsForCategory"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }

        public void EnableListVersioning(AveVersionMode versionMode)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.EnableListVersioning"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }

        public void RestoreList()
        {

        }

        public void RestoreListSelf1(AveListInfo listInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.RestoreListSelf1"))
            {
#endif
                mSPList = mAveSPWeb.SPWeb.Lists.GetByTitle(listInfo.Title);
                mIsRestored = true;
                if (mSPList == null)
                {
                    mSPList = CreateNewList(listInfo);
                }
                else
                {
                    UpdateExistedList(listInfo);
                }
                mUrl = mAveParentSite.ApplicationName + mSPList.RootFolder.ServerRelativeUrl;
                mSrcUrl = listInfo.SrcUrl;
                mSize = listInfo.Size;
#if PerformanceLog
            }
#endif
        }

        protected IAveList CreateNewList(AveListInfo listInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.CreateNewList"))
            {
#endif
                AveListCreationInformation alci = new AveListCreationInformation();
                alci.Title = listInfo.Title;
                //alci.Url = listInfo.Url;
                alci.Description = listInfo.Description;
                //alci.DocumentTemplateType = listInfo.DocTemplateType;
                alci.TemplateType = listInfo.BaseTemplate;
                alci.QuickLaunchOption = (AveQuickLaunchOptions)listInfo.QuickLaunchOptions;
                IAveList list = mAveSPWeb.SPWeb.Lists.Add(alci);
                if (listInfo.BaseTemplate == (int)AveListTemplateType.Survey)
                {
                    list.AllowMultiResponses = true;
                }
                return list;
#if PerformanceLog
            }
#endif
        }

        protected void UpdateExistedList(AveListInfo listInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.UpdateExistedList"))
            {
#endif
                mSPList.Title = listInfo.Title;
                if (listInfo.Description != null)
                {
                    mSPList.Description = listInfo.Description;
                }
                if (listInfo.BaseTemplate == (int)AveListTemplateType.Survey)
                {
                    mSPList.AllowMultiResponses = true;
                }
                mSPList.Update();
#if PerformanceLog
            }
#endif
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (this.SPList != null)
            {
                this.SPList.CleanListData();
            }
            if (mFields != null)
            {
                if (mFields.XmlFields != null)
                {
                    mFields.XmlFields.Clear();
                }
                mFields = null;
            }
            if (mListItemSerializer != null)
            {
                mListItemSerializer = null; //SAAS-21766 将该属性设为空，释放所占空间
            }

            if(mContentTypes != null)
            {
                mContentTypes.Dispose();
            }

            if(reportor != null)
            {
                reportor.Dispose();
            }
        }

        #endregion

        internal void RestoreListRssViewField()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.RestoreListRssViewField"))
            {
#endif
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
                        ArgumentNullException.ThrowIfNull(fieldC);
                        foreach (XmlNode fieldNode in rssDoc.GetElementsByTagName("FieldRef"))
                        {
                            XmlElement node = fieldNode as XmlElement;
                            string fieldName = node.GetAttribute("Name");
                            string realFieldName = ParentWeb.ParentSite.GetNameByLanguageMapping(fieldName, AveLanguageMappingType.FieldMapping);
                            try
                            {
                                ArgumentCheck.CheckNotNull(fieldC);
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
                    reportor.AddDetail(new AveWrapperReportDto("ListRssViewField", "ListRssViewField", AveReportObjectType.ListRssViewField, AveStatus.Skipped, "You don't have permission to restore ListRssViewField. " + ex.Message));
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
#if PerformanceLog
            }
#endif
        }

        public void UpdateDefaultValue()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.UpdateDefaultValue"))
            {
#endif
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
                        xDoc.LoadXml(new UTF8Encoding().GetString(spFile.OpenBinary()).EncodeAmpersandInHref());
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
                    reportor.AddDetail(new AveWrapperReportDto("ListDefaultValue", "ListDefaultValue", AveReportObjectType.ListDefaultValue, AveStatus.Skipped, "You don't have permission to restore List Default View. " + ex.Message));
                }
                catch (Exception ex)
                {
                    log.Warn("Update the file: {0} failed in ListPostAction. Exception: {1}", SPList.RootFolder.ServerRelativeUrl + "/Forms/client_LocationBasedDefaults.html", ex);
                }
#if PerformanceLog
            }
#endif
        }

        internal void RestoreSolutionStatus()
        {
            if (this.SPList != null && this.SPList.BaseTemplate == AveListTemplateType.SolutionCatalog)
            {
                this.SPList.RestoreSolutionStatus(mSandboxSolutions);
            }
        }

        internal void RestoreSolutionFeatures()
        {
            if (this.SPList != null && this.SPList.BaseTemplate == AveListTemplateType.SolutionCatalog)
            {
                RemoveNonActivatedFeatures(mSiteFeatures, this.mAveParentSite.SourceFeatures);
                using (AveSPFeature featureManager = new AveSPFeature(this.ParentSite))
                {
                    featureManager.Restore(mSiteFeatures);
                }

                RemoveNonActivatedFeatures(mWebFeatures, this.mAveSPWeb.SourceFeatures);
                using (AveSPFeature featureManager = new AveSPFeature(this.ParentWeb))
                {
                    featureManager.Restore(mWebFeatures);
                }
            }
        }

        private void RemoveNonActivatedFeatures(AveFeatureInfoBox solutionFeatures, AveFeatureInfoBox activeFeatures)
        {
            if (activeFeatures != null)
            {
                solutionFeatures.FeatureList = solutionFeatures.FeatureList.Intersect(activeFeatures.FeatureList, new AveFeatureInfoComparer()).ToList<AveFeatureInfo>();
            }
        }

        internal void RestoreFieldIndexes()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.RestoreFieldIndexes"))
            {
#endif
                AveFields.RestoreListFieldIndexes();
#if PerformanceLog
            }
#endif
        }

        internal void RestoreDocumentSetMetaInfo()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.RestoreDocumentSetMetaInfo"))
            {
#endif
                Dictionary<int, int> documentIdInDocumentSet = new Dictionary<int, int>();
                foreach (var keyValuePair in ParentSite.MappingManager.ListMappingManager.DocumentSetGuidMetaInfoMapping)
                {
                    var documentSet = mSPList.Folders[keyValuePair.Key];

                    XmlDocument snapShot = new XmlDocument();
                    snapShot.PreserveWhitespace = true;//sharepoint use xmlreader to analyze this, which won't ignore white space node.ADO-8150
                    snapShot.LoadXml(keyValuePair.Value["snapshots"].ToString().Replace("\\r\\n", "\r\n").Replace(@"\\", @"\"));

                    foreach (XmlElement tmp in snapShot.SelectNodes("/SnapshotCollection/Items/Item"))
                    {
                        try
                        {
                            string url = tmp.GetAttribute("Url");
                            IAveFile file = TryGetFile(documentSet.Folder, url);
                            if (file != null && file.Exists)
                            {
                                int oldId = int.Parse(tmp.GetAttribute("Id"));
                                tmp.SetAttribute("Id", file.Item.ID.ToString());
                                tmp.SetAttribute("Guid", file.UniqueId.ToString());
                                documentIdInDocumentSet.Add(oldId, file.Item.ID);
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetFileMetaInfoFailed, e);
                        }
                    }

                    foreach (XmlElement snapshot in snapShot.SelectNodes("/SnapshotCollection/Snapshots/Snapshot"))
                    {
                        try
                        {
                            foreach (XmlElement item in snapshot.SelectNodes("SnapshotItems/SnapshotItem"))
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
                            foreach (XmlElement fieldElement in snapshot.SelectNodes("Fields/Field"))
                            {
                                string oldValue = fieldElement.GetAttribute("Id");
                                if (!string.IsNullOrEmpty(oldValue))
                                {
                                    Guid newValue = AveFields.FieldMapping.GetMappingRestoredFieldId(new Guid(oldValue));
                                    if (newValue != Guid.Empty)
                                    {
                                        fieldElement.SetAttribute("Id", newValue.ToString());
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetFileMetaInfoFailed, e);
                        }
                    }
                    documentSet.Folder.Properties["snapshots"] = snapShot.OuterXml;
                    documentSet.Folder.Update();
                    documentIdInDocumentSet.Clear();
                }
                foreach (var keyValuePair in ParentSite.MappingManager.ListMappingManager.DocumentSetGuidMetaInfoMapping)
                {
                    var documentSet = mSPList.Folders[keyValuePair.Key];
                    documentSet["Editor"] = keyValuePair.Value["Editor"];
                    documentSet["Modified"] = keyValuePair.Value["Modified"];
                    documentSet.SystemUpdate();
                }

#if PerformanceLog
            }
#endif
        }

        internal void RestoreWorkflowStartOptions()
        {
            try
            {
                WFConflictResolution wfResolution = WFConflictResolution.Instance;
                wfResolution.UpdateWorkflowStartOptions(this);
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "An error occured when restore workflow startOptions error: {0}", e);
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
                                this.ParentSite.Publishing.SetWebMasterPageInfo(setting, web, web.AlternateCssUrl);
                            }
                        }
                        try
                        {
                            tempFile.Delete();
                        }
                        catch (Exception ex)
                        {
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
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPList.EnsureListResourceSelector"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }

        private IAveFile TryGetFile(IAveFolder parentFolder, string url)
        {
            IAveFile file = null;
            try
            {
                file = parentFolder.Files[url];
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFileFromURLError, ex);
            }
            if (file == null || !file.Exists)
            {
                file = parentFolder.ParentWeb.GetFile(parentFolder.ServerRelativeUrl + "/" + url);
            }
            return file;
        }
        public void AddInvalidLookupListTitle(string internalName)
        {
            lock (invalidLookupList)
            {
                if (!invalidLookupList.Contains(internalName))
                {
                    invalidLookupList.Add(internalName);
                }
            }
        }
        public bool IsLookupListValid(string listName)
        {
            lock (invalidLookupList)
            {
                return invalidLookupList.Contains(listName);
            }
        }

        private void GetDefaultValue()
        {
            mDefValue = new Dictionary<string, object>();

            //if (SPList != null && SPList.RootFolder != null && !string.IsNullOrEmpty(SPList.RootFolder.ServerRelativeUrl) && SPList.BaseTemplate == AveListTemplateType.DocumentLibrary)
            if (SPList != null && SPList.BaseType == AveBaseType.DocumentLibrary)
            {
                IAveFile spFile = SPList.ParentWeb.GetFile(SPList.RootFolder.ServerRelativeUrl + "/Forms/client_LocationBasedDefaults.html");
                if (spFile != null && spFile.Exists)
                {
                    XmlDocument xDoc = new XmlDocument();
                    try
                    {
                        xDoc.LoadXml(new UTF8Encoding().GetString(spFile.OpenBinary()).EncodeAmpersandInHref());
                        log.Info($"Success get default value in AveSPList and DetectTheEncoding.");
                    }
                    catch (Exception ex)
                    {
                        log.Info($"Can not get default value in AveSPList and DetectTheEncoding.Message:{ex.ToString()}.");
                        xDoc.LoadXml(DetectTheEncoding(spFile.OpenBinary()).EncodeAmpersandInHref());
                        log.Info($"Success get DetectTheEncoding value in AveSPList.");
                    }
                    foreach (XmlNode node in xDoc.DocumentElement.SelectNodes("a"))
                    {
                        foreach (XmlNode field in node.ChildNodes)
                        {
                            if (field.Name.Equals("DefaultValue"))
                            {
                                string fieldName = field.Attributes["FieldName"].Value;
                                object objDefValue;
                                if (!mDefValue.TryGetValue(fieldName, out objDefValue))
                                {
                                    objDefValue = new Dictionary<string, object>();
                                }
                                Dictionary<string, object> defValue = objDefValue as Dictionary<string, object>;
                                string key = ((XmlElement)node).GetAttribute("href");
                                IAveField aveField = SPList.Fields.GetFieldByInternalName(fieldName, false);
                                if (aveField != null)
                                {
                                    if (aveField.Type == AveFieldType.DateTime)
                                    {
                                        if (field.InnerText.Equals("[today]", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            defValue[key] = DateTime.Today;
                                        }
                                        else
                                        {
                                            defValue[key] = DateTime.Parse(field.InnerText);
                                        }
                                    }
                                    else
                                    {
                                        defValue[key] = field.InnerText;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private string DetectTheEncoding(byte[] bytes)
        {
            Encoding encoding = null;
            String text = null;
            // Test UTF8 with BOM. This check can easily be copied and adapted
            // to detect many other encodings that use BOMs.
            UTF8Encoding encUtf8Bom = new UTF8Encoding(true, true);
            Byte[] preamble = encUtf8Bom.GetPreamble();
            Int32 prLen = preamble.Length;
            if (bytes.Length >= prLen && preamble.SequenceEqual(bytes.Take(prLen)))
            {
                // UTF8 BOM found; use encUtf8Bom to decode.
                try
                {
                    log.Info($"Use UTF8 BOM encoding.");
                    // Seems that despite being an encoding with preamble,
                    // it doesn't actually skip said preamble when decoding...
                    text = encUtf8Bom.GetString(bytes, prLen, bytes.Length - prLen);
                    encoding = encUtf8Bom;
                }
                catch (Exception ex)
                {
                    // Confirmed as not UTF-8!
                    log.Error($"Failed UTF8 BOM encoding.Message:{ex.ToString()}.");
                }
            }
            // fall back to default ANSI encoding.
            if (encoding == null)
            {
                log.Info($"Begin detect by default ANSI encoding.");
                encoding = Encoding.GetEncoding(1252);
                text = encoding.GetString(bytes);
                log.Info($"Success detect by default ANSI encoding.Encoding text:{text}.");
            }
            return text;
        }

        internal void ReorderListFields(List<string> mappedSourceFields)
        {
            if (mSPList != null)
            {
                mSPList.ReorderListFields(mappedSourceFields);
            }
        }

        public void InitSqliteCacheInfo(string jobId, int aveListSqliteCacheTypes)
        {
            log.Info($"Init sqlite cache info for list. list title:{mSPList.Title}, Id: {mSPList.ID} jobId:{jobId}, aveListSqliteCacheTypes:{aveListSqliteCacheTypes}");
            mSPList.InitSqliteCacheInfo(jobId, aveListSqliteCacheTypes);
        }
    }

    public enum ListRestoreOption
    {
        Title = 1,
        Url = 2,
        TitleAndUrl = 3,
    }

    class ValidationFormulaParser
    {
        static AveLogger mLogger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static readonly char[] operators = new char[] { '=', '<', '>', '&', '^', '+', '-', '*', '/' };
        static readonly char[] separators = new char[] { ',', '(', ')', ' ' };

        static public string Parse(string formula, Dictionary<string, string> fieldDisplayNameMapping)
        {
            if (string.IsNullOrEmpty(formula) || fieldDisplayNameMapping.Count == 0)
            {
                return formula;
            }

            string fieldDisplayNameMapped = string.Empty;
            string fieldDisplayName = string.Empty;

            bool isKeyWord = false;
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < formula.Length; i++)
            {
                char ch = formula[i];
                if (operators.Contains(ch) || separators.Contains(ch))
                {
                    builder.Append(ch);
                }
                else if ('['.Equals(ch))
                {// column，需要做mapping
                    fieldDisplayName = GetFieldDisplayName(formula, ref i, ']');
                    if (!string.IsNullOrEmpty(fieldDisplayName))
                    {
                        fieldDisplayNameMapped = GetFieldDisplayNameByMapping(fieldDisplayName, fieldDisplayNameMapping);

                        builder.Append(ch).Append(fieldDisplayNameMapped).Append(']');
                    }
                    else
                    {
                        builder.Append(ch);
                    }
                }
                else if ('"'.Equals(ch))
                {// 双引号之中不会包含column，不需要做mapping
                    var content = GetFieldDisplayName(formula, ref i, '"');
                    if (!string.IsNullOrEmpty(content))
                    {
                        builder.Append(ch).Append(fieldDisplayName).Append('"');
                    }
                    else
                    {
                        builder.Append(ch);
                    }
                }
                else
                {
                    fieldDisplayName = GetFieldDisplayName(formula, ref i, out isKeyWord);
                    if (!isKeyWord && !string.IsNullOrEmpty(fieldDisplayName) && !Validator.IsNumber(fieldDisplayName))
                    {// column，需要做mapping, 数字的column会在 '['分支处理
                        fieldDisplayNameMapped = GetFieldDisplayNameByMapping(fieldDisplayName, fieldDisplayNameMapping);
                        builder.Append(fieldDisplayNameMapped);
                    }
                    else
                    {// 关键字
                        builder.Append(fieldDisplayName);
                    }
                }
            }

            var result = builder.ToString();
            mLogger.Info("formula:[{0}] => [{1}]", formula, result);
            return result;
        }

        private static string GetFieldDisplayNameByMapping(string fieldDisplayName, Dictionary<string, string> fieldDisplayNameMapping)
        {
            string result = string.Empty;
            if (!fieldDisplayNameMapping.TryGetValue(fieldDisplayName.Trim(), out result))
            {
                result = fieldDisplayName;
            }

            return result;
        }

        private static string GetFieldDisplayName(string array, ref int index, out bool isKeyWord)
        {
            StringBuilder fieldBuilder = new StringBuilder();
            char ch = array[index];

            while (!operators.Contains(ch) && !separators.Contains(ch))
            {
                fieldBuilder.Append(ch);
                index++;
                if (index < array.Length)
                {
                    ch = array[index];
                }
                else
                {
                    break;
                }
            }
            index--;
            isKeyWord = IsKeyWord(ch);

            return fieldBuilder.ToString();
        }

        private static bool IsKeyWord(char ch)
        {
            return '('.Equals(ch);
        }

        private static string GetFieldDisplayName(string formula, ref int startIndex, char separator)
        {
            string result = string.Empty;
            int index = formula.IndexOf(separator, startIndex + 1);
            if (index > startIndex)
            {
                var length = index - startIndex - 1;
                result = formula.Substring(startIndex + 1, length);
                startIndex = index;
            }

            return result;
        }
    }
}
