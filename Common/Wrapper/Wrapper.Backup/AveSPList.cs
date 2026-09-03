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

//using Microsoft.SharePoint;
//using Microsoft.SharePoint.Administration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Mapping;
using AvePoint.Wrapper.Resource;
using ArgumentCheck = AvePoint.Wrapper.Common.ArgumentCheck;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPList : IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private FMExcelOpenXml mFMExcel;
        private int mCurrentItemCountPerSheet = 0;
        private int mCurrentSheetCount = 0;
        private int mCurrentExcelFileCount = 1;
        private int mItemCountPerSheet = int.MaxValue;
        private int mSheetCountPerExcel = 10;
        private string mTitle;
        private string mPath;
        private string mRootFolderPath;
        private bool mIsSystemList;
        private Guid mId;
        private Guid mScopeId;
        private AveSPWeb mAveSPWeb;
        private IAveList mSPList;
        private AveSPListFieldCollection mFields;
        private AveSPListContentTypes mCachedContentTypes = null;
        private bool mNeedExportExcel = false;
        private AveListDataCache mDataCache = new AveListDataCache();
        private Dictionary<Guid, List<AveViewInfo>> mViewCache = new Dictionary<Guid, List<AveViewInfo>>();
        private Dictionary<string, AvePAGETYPE> mFormCache = new Dictionary<string, AvePAGETYPE>();
        //private Dictionary<string, AveFieldType> mDisplayColumns = null;
        private Dictionary<string, AveSPField> mDisplayColumns = null;

        private Dictionary<Guid, int> mSolutionStatus = null;
        private string mExcelPath = "C:\\" + "Hi.xlsx";
        internal Dictionary<Guid, IAveList> mLookupListDic = new Dictionary<Guid, IAveList>();
        internal Dictionary<string, string> metadataFields = new Dictionary<string, string>();
        private IAveBackupStream mSender;
        private IAveBackupRestoreQueryService mQueryService;
        private Dictionary<string, KeyValuePair<string, string>> mLookupFieldListId = new Dictionary<string, KeyValuePair<string, string>>();
        public List<AveRoleAssignmentInfo> RoleAssignmentCache = null;
        public List<Dictionary<string, object>> ImmedSubscriptionsCache = null;
        public List<Dictionary<string, object>> SchedSubscriptionsCache = null;
        private AveSPSite mAveParentSite;
        private bool mNeedCreateExcel = true;
        private AveIndexCache indexCache = null;
        private bool mIrmEnabled = true;
        private Dictionary<string, object> mInformationRightsManagementDic = null;
        private bool mDisableInformationRightsManagement = false;

        private IAveSOIntegrationUtility mSOIntegrationUtil;
        private Dictionary<string, FieldInfoForExcel> mFieldNameTypeDic;
        private Dictionary<string, string> mImportExcelHeaders;

        /// <summary>
        /// ContentManager 源端365有多线程备份的逻辑，这样判断是否为view时，会有多个thread操作mViewCache，导致mViewCache这个集合出现问题
        /// </summary>
        private readonly object locker = new object();

        private Dictionary<string, AveTaxFieldInfo> taxonomyFields = null;
        public Dictionary<string, AveTaxFieldInfo> TaxonomyFields
        {
            get
            {
                lock (locker)
                {
                    if (taxonomyFields == null)
                    {
                        EnsureListFieldCache();
                    }
                    return taxonomyFields;
                }
            }
        }

        //private Dictionary<string, AveLookupFieldInfo> lookupFields = null;
        //public Dictionary<string, AveLookupFieldInfo> LookupFields
        //{
        //    get
        //    {
        //        lock (locker)
        //        {
        //            if (lookupFields == null)
        //            {
        //                EnsureListFieldCache();
        //            }
        //            return lookupFields;
        //        }
        //    }
        //}

        private List<string> userFields = null;
        public List<string> UserFields
        {
            get
            {
                lock (locker)
                {
                    if (userFields == null)
                    {
                        EnsureListFieldCache();
                    }
                    return userFields;
                }
            }
        }

        public Dictionary<string, string> ImportExcelHeaders
        {
            get
            {
                if (mImportExcelHeaders == null)
                {
                    mImportExcelHeaders = new Dictionary<string, string>();
                    if (mFieldNameTypeDic == null)
                    {
                        mFieldNameTypeDic = new Dictionary<string, FieldInfoForExcel>();
                        SetFieldNameTypeDicValue();
                    }
                }
                return mImportExcelHeaders;
            }
        }

        public int ItemCountPerSheet
        {
            set { mItemCountPerSheet = value; }
        }

        public Dictionary<string, FieldInfoForExcel> FieldNameTypeDic
        {
            get
            {
                if (mFieldNameTypeDic == null)
                {
                    mFieldNameTypeDic = new Dictionary<string, FieldInfoForExcel>();
                    SetFieldNameTypeDicValue();
                }
                return mFieldNameTypeDic;
            }
        }

        public bool NeedExportExcel
        {
            get { return mNeedExportExcel; }
            set { mNeedExportExcel = value; }
        }

        public string ExcelPath
        {
            get { return mExcelPath; }
            set
            {
                mExcelPath = value;
                //Export应用metadata ,list非隐藏时导出excel
                if (mNeedCreateExcel && !mSPList.Hidden)
                {
                    CreateExcelOrSheetIfNeed();
                }
            }
        }

        public AveSPSite ParentSite
        {
            get { return mAveParentSite; }
        }

        public AveIndexCache AveIndexCache
        {
            get
            {
                if (this.indexCache == null)
                {
                    this.indexCache = new AveIndexCache(this);
                }
                return this.indexCache;
            }
        }

        public string Title
        {
            get { return mTitle; }
        }

        public string Path
        {
            get { return mPath; }
        }

        public string RootFolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(mRootFolderPath))
                {
                    if (IsSystemList)
                    {
                        mRootFolderPath = mAveSPWeb.SPWeb.RootFolder.ServerRelativeUrl;
                    }
                    else
                    {
                        mRootFolderPath = mSPList.RootFolder.ServerRelativeUrl;
                    }
                    if (mRootFolderPath.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                        mRootFolderPath = mRootFolderPath.Substring(1, mRootFolderPath.Length - 2);
                    else
                        mRootFolderPath = mRootFolderPath.Substring(1);
                }
                return mRootFolderPath;
            }
        }

        public string ServerRelativeUrl
        {
            get { return mIsSystemList ? mAveSPWeb.SPWeb.ServerRelativeUrl : mSPList.RootFolder.ServerRelativeUrl; }
        }

        public bool IsSystemList
        {
            get { return mIsSystemList; }
        }

        public bool HasUniqueRoleAssignments
        {
            get { return mSPList.HasUniqueRoleAssignments; }
        }

        public Guid Id
        {
            get { return mId; }
        }

        public Guid ScopeId
        {
            get { return mScopeId; }
        }

        public AveSPWeb ParentWeb
        {
            get { return mAveSPWeb; }
        }

        public IAveList SPList
        {
            get { return mSPList; }
        }

        public AveSPListFieldCollection Fields
        {
            get { return mFields; }
        }

        public Dictionary<Guid, List<AveViewInfo>> ViewCache
        {
            get { return mViewCache; }
        }

        public Dictionary<string, AvePAGETYPE> FormCache
        {
            get { return mFormCache; }
        }

        public Dictionary<Guid, int> SolutionStatus
        {
            get { return mSolutionStatus; }
        }

        public IAveSOIntegrationUtility SOIntegrationUtil
        {
            get
            {
                if (mSOIntegrationUtil == null)
                {
                    mSOIntegrationUtil = this.ParentSite.ObjectModelFactory.CreateSOIntegrationUtility();
                }
                return mSOIntegrationUtil;
            }
        }

        /// <summary>
        /// For each key-value-pair of the DisplayColumns, value means the current column is a reference of user or not.
        /// </summary>
        public Dictionary<string, AveSPField> DisplayColumns
        {
            get
            {
                if (mDisplayColumns == null)
                {
                    mDisplayColumns = GetColumns(true);
                }
                return mDisplayColumns;
            }
        }

        public IAveBackupStream Sender
        {
            get { return mSender; }
        }

        public IAveBackupRestoreQueryService QueryService
        {
            get { return mQueryService; }
        }

        public bool DisableInformationRightsManagement
        {
            set { mDisableInformationRightsManagement = value; }
            get { return mDisableInformationRightsManagement; }
        }

        public AveSPList(AveSPWeb _AveWeb, Guid _id, string _title)
            : this(_AveWeb, _id, _title, false)
        {
        }

        public AveSPList(AveSPWeb _AveWeb, Guid _id, string _title, string _path)
            : this(_AveWeb, _id, _title, _path, false)
        {
        }

        public AveSPList(AveSPWeb _AveWeb, Guid _id, string _title, bool getFullSchema)
            : this(_AveWeb, _id, _title, _title, getFullSchema)
        {
        }

        public AveSPList(AveSPWeb _AveWeb, Guid _id, string _title, string path, bool getFullSchema)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.Constructor"))
            {
                mAveSPWeb = _AveWeb;
                if (ParentWeb.mReloadWebAndParentForSPRequestTimeout != null)
                {
                    ParentWeb.mReloadWebAndParentForSPRequestTimeout(false);
                }
                mAveParentSite = _AveWeb.ParentSite;
                mSender = mAveSPWeb.Sender;
                mQueryService = mAveSPWeb.QueryService;
                mId = _id;
                mTitle = _title;
                mPath = mAveSPWeb.Name + "\\" + path;
                if (mId == Guid.Empty && string.Compare(AveConstants.SYSTEM_FOLDER, mTitle, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    mIsSystemList = true;
                    mScopeId = _AveWeb.ScopeId;
                    return;
                }
                InitSPList(_id, _title);
                mScopeId = mSPList.RoleAssignments.ID;
                mFields = new AveSPListFieldCollection(this);
                try
                {
                    InitContentTypes(new AveSPListContentTypeCollection(this));
                }
                catch (Exception e)
                {
                    log.Warn("Init list ContentTypes error,list title:{0}, exception:{1}.", mTitle, e.ToString());
                }
                //GetListViews();
            }
        }

        internal void InitSPList(Guid _id, string _title)
        {
            try
            {
                mSPList = mAveSPWeb.SPWeb.Lists.GetById(_id);
                //由于找不到不抛异常，直接返回null
                if (mSPList == null)
                {
                    mSPList = mAveSPWeb.SPWeb.Lists.GetByTitle(_title);
                    if (mSPList == null)
                    {
                        throw new Exception(string.Format("Cannot find the list with id:{0} and title:{1}", _id, _title));
                    }
                    mId = mSPList.ID;
                }
            }
            catch (AveException)//(SPException)
            {
                try
                {
                    mSPList = mAveSPWeb.SPWeb.Lists.GetByTitle(_title);
                    mId = mSPList.ID;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, WrapperBackupResource.AWBConstrucListError, mAveSPWeb.SPWeb.ID, mAveSPWeb.SPWeb.Url, _id, _title, e.ToString());
                    throw;
                }
            }
            try
            {
                if (mSPList.BaseTemplate == AveListTemplateType.SolutionCatalog)
                {
                    mSolutionStatus = new Dictionary<Guid, int>();
                    if (mSPList.ParentWeb.Site.Solutions != null)
                    {
                        foreach (IAveUserSolution solution in mSPList.ParentWeb.Site.Solutions)
                        {
                            if (!mSolutionStatus.ContainsKey(solution.SolutionId))
                            {
                                mSolutionStatus.Add(solution.SolutionId, (int)solution.Status);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn("Get List solution status error,List title:{0}, exception:{1}.", mSPList.Title, e.ToString());
            }
            try
            {
                if (mSPList.DefaultView != null)
                {
                    if (mQueryService != null)
                    {
                        mQueryService.GetViews(mViewCache, this.Id, SPList.DefaultView.ID);
                    }
                    else
                    {
                        mSPList.GetViews(mViewCache);
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn("Get List views error,List title:{0}, exception:{1}.", mSPList.Title, e.ToString());
            }
            try
            {
                if (mSPList != null && mSPList.Forms != null)
                {
                    foreach (IAveForm form in mSPList.Forms)
                    {
                        mFormCache[form.Url] = form.FormType;
                    }
                }
            }
            catch (Exception e)
            {
                ArgumentCheck.CheckNotNull(mSPList);
                log.Warn("Get List forms error, List title: {0}, exception: {1}", mSPList.Title, e.ToString());
            }
        }

        internal void SetFieldNameTypeDicValue()
        {
            mImportExcelHeaders.Add("Path", string.Empty);
            List<string> keys = new List<string>();
            foreach (IAveField field in this.SPList.Fields)
            {
                if (!field.ReadOnlyField && !field.Title.Equals("ID") && !field.Hidden && field.InternalName != null && !keys.Contains(field.Title))
                {
                    GetFieldInfo(field, keys);
                }
            }
        }

        internal void GetFieldInfo(IAveField field, List<string> keys)
        {
            if (field.Type == AveFieldType.Lookup)
            {
                mLookupFieldListId.Add(field.Title, new KeyValuePair<string, string>((field as IAveFieldLookup).LookupList, (field as IAveFieldLookup).LookupField));
            }
            else if (field.TypeAsString.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) || field.TypeAsString.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
            {
                IAveTaxonomyField taxField = field as IAveTaxonomyField;
                IAveField textField = SPList.Fields[taxField.TextField];
                metadataFields[textField.InternalName] = field.InternalName;
            }
            if (!mImportExcelHeaders.ContainsKey(field.Title + ":=" + FieldInternalTypeAndGuiTypeMapping.GetGuiTypeByInternalType(field.TypeAsString)))
            {
                keys.Add(field.Title);
                mImportExcelHeaders.Add(field.Title + ":=" + FieldInternalTypeAndGuiTypeMapping.GetGuiTypeByInternalType(field.TypeAsString), string.Empty);
            }
            if (!mFieldNameTypeDic.ContainsKey(field.InternalName))
            {
                if (!keys.Contains(field.Title))
                {
                    keys.Add(field.Title);
                }
                mFieldNameTypeDic.Add(field.InternalName, new FieldInfoForExcel() { Title = field.Title, TypeAsString = field.TypeAsString, TitleAndGuiType = field.Title + ":=" + FieldInternalTypeAndGuiTypeMapping.GetGuiTypeByInternalType(field.TypeAsString) });
            }
        }

        /// <summary>
        /// Encode list path for special chars: '%' to %1; '\' to '%2' (server cannot load tree if path owns these characters.)
        /// </summary>
        public void EncodePathForSpecialChar()
        {
            if (!string.IsNullOrEmpty(mTitle))
            {
                mPath = mAveSPWeb.Name + "\\" + AvePoint.GCommon.AveConverter.EncodeSpecialChar(mTitle);
            }
        }

        public void CachePrincipalFromPermission(int value)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.CachePrincipalFromPermission"))
            {
                AveRoleAssignments roleAssignments = AveRoleAssignments.CreateInstance(this);
                RoleAssignmentCache = roleAssignments.GetRoleAssignments();
                //exist list like user information list has UniqueRoleAssignments，but RoleAssignmentCache is null
                if (RoleAssignmentCache == null)
                {
                    return;
                }
                for (int i = 0; i < RoleAssignmentCache.Count; ++i)
                {
                    try
                    {
                        int principalId = RoleAssignmentCache[i].PrincipalId;
                        if (!mDataCache.principalIdAlreadyExists(principalId))
                        {
                            object obj = mAveSPWeb.ParentSite.DataCache.GetPrincipalInfo(principalId);

                            if (obj is AveUserInfo && (value & 1) != 0)
                            {
                                mDataCache.AddToCache(principalId, (AveUserInfo)obj);
                            }
                            else if (obj is AveGroupInfo && (value & 2) != 0)
                            {
                                mDataCache.AddToCache(principalId, (AveGroupInfo)obj);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while cache principal from permission. list id:{0}.list title:{1} \n error message:{2}", this.Id, this.Title, e));
                    }
                }
            }
        }

        public void CacheUserFromAuthor()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.CacheUserFromAuthor"))
            {
                try
                {
                    if (SPList != null && SPList.Author != null)
                    {
                        int userId = SPList.Author.ID;
                        if (!mDataCache.principalIdAlreadyExists(userId))
                        {
                            AveUserInfo userInfo = (AveUserInfo)mAveSPWeb.ParentSite.DataCache.GetPrincipalInfo(userId);
                            mDataCache.AddToCache(userId, userInfo);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while cache user from list author. \n error message:{0}", e));
                }
            }
        }

        public void CacheUserFromAlert()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.CacheUserFromAlert"))
            {
                AveSPAlert listAlert = AveSPAlert.CreateInstance(this);
                ImmedSubscriptionsCache = listAlert.GetImmedSubscriptions();
                SchedSubscriptionsCache = listAlert.GetSchedSubscriptions();

                for (int i = 0; i < ImmedSubscriptionsCache.Count; i++)
                {
                    GetUserInfoAndAddToCache(ImmedSubscriptionsCache, i);
                }
                for (int i = 0; i < SchedSubscriptionsCache.Count; i++)
                {
                    GetUserInfoAndAddToCache(SchedSubscriptionsCache, i);
                }
            }
        }

        internal void GetUserInfoAndAddToCache(List<Dictionary<string, object>> Cache, int iterator)
        {
            try
            {
                int userId = int.Parse(Cache[iterator]["UserId"].ToString());
                if (!mDataCache.principalIdAlreadyExists(userId))
                {
                    AveUserInfo userInfo = (AveUserInfo)mAveSPWeb.ParentSite.DataCache.GetPrincipalInfo(userId);
                    mDataCache.AddToCache(userId, userInfo);
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, string.Format("An error occurred while cache user from alert. \n error message:{0}", e));
            }
        }

        public void CacheGroupsFromUserField()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.CacheGroupsFromUserField"))
            {
                try
                {
                    foreach (IAveField mField in mSPList.Fields)
                    {
                        if (mField.Type == AveFieldType.User)
                        {
                            IAveFieldUser userField = mField as IAveFieldUser;
                            AveGroupInfo groupInfo = (AveGroupInfo)mAveSPWeb.ParentSite.DataCache.GetPrincipalInfo(userField.SelectionGroup);
                            mDataCache.AddToCache(userField.SelectionGroup, groupInfo);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while cache group. \n error message:{0}", e));
                }
            }
        }

        public void ExportUserCache(IAveBackupStream output)
        {
            output.WriteMetadata(AveMetadataType.UserCache, mDataCache.UserList);
        }

        public void ExportGroupCache(IAveBackupStream output)
        {
            output.WriteMetadata(AveMetadataType.GroupCache, mDataCache.GroupList);
        }

        public void InitContentTypes(AveSPContentTypeCollection ct)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.Constructor.InitContentTypes"))
            {
                try
                {
                    mCachedContentTypes = ct.ExportAllContentType();
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.DEBUG, "An error occurred when init list content types, Reason:{0}.", ex);
                }
            }
        }

        public void ExportWorkflows(IAveBackupStream output)
        {
            this.ExportWorkflows(output, null);
        }

        public void ExportWorkflows(IAveBackupStream output, Func<AveWorkflowAssociationInfo, bool> filterFunc)
        {
            AveWorkflow workflow = new AveWorkflow();
            LS.SPWorkflowProcessor.SPWorkflowProcessorRuntime.ProcessAssociation = true;
            workflow.ExportListWFAssociation(output, this, filterFunc);
            workflow.ExportListContentTypeWFAssociation(output, this, filterFunc);
        }

        internal Dictionary<string, AveSPField> GetColumns(bool onlyDisplay, bool includeHidden = false, bool isSystem = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.GetColumns"))
            {
                if (!onlyDisplay)
                {
                    if (includeHidden)
                    {
                        if (isSystem)
                        {
                            return mFields.IdFieldMap.Values.Distinct().ToDictionary(field => field.BackupName, field => field);
                        }
                        return mFields.FieldMap.Values.ToDictionary(field => field.BackupName, field => field);
                    }
                    return mFields.FieldMap.Values.Where(field => !field.IsHidden).ToDictionary(field => field.BackupName, field => field);
                }
                return mFields.FieldMap.Values.Where(field => field.IsDisplayColumn).ToDictionary(field => field.BackupName, field => field);
            }
        }

        internal void TryGetContentType(byte[] contentTypeId, out string contentTypeName)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.TryGetContentType"))
            {
                contentTypeName = string.Empty;
                if (mCachedContentTypes != null)
                {
                    mCachedContentTypes.TryGet(contentTypeId, out contentTypeName);
                }
            }
        }

        private void EnsureListFieldCache()
        {
            //lookupFields = new Dictionary<string, AveLookupFieldInfo>();
            userFields = new List<string>();
            taxonomyFields = new Dictionary<string, AveTaxFieldInfo>();
            if (mSPList != null)
            {
                foreach (IAveField field in mSPList.Fields)
                {
                    try
                    {
                        if (field is IAveTaxonomyField)
                        {
                            IAveTaxonomyField tField = field as IAveTaxonomyField;
                            AddTaxonomyFieldToCache(tField);
                        }
                        //else if (field.Type == AveFieldType.Lookup)
                        //{
                        //    IAveFieldLookup lookupField = field as IAveFieldLookup;
                        //    AddLookupFieldToCache(lookupField);
                        //}
                        else if (field.Type == AveFieldType.User)
                        {
                            IAveFieldUser userField = field as IAveFieldUser;
                            AddUserFieldToCache(userField);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.WARN, "An error occurred while ensure list field cache. Field Internal Name:{0}. List Title:{1}. Error:{2}", field.InternalName, mSPList.Title, ex.ToString());
                    }
                }
            }

        }

        private void AddTaxonomyFieldToCache(IAveTaxonomyField tField)
        {
            AveTaxFieldInfo tInfo = new AveTaxFieldInfo();
            tInfo.SspId = tField.SspId;
            tInfo.TermSetId = tField.TermSetId;
            tInfo.TextFieldInternalName = SPList.Fields[tField.TextField].InternalName;
            tInfo.IsKeywordsColumn = tField.ID.Equals(new Guid("23F27201-BEE3-471e-B2E7-B64FD8B7CA38"));
            if (tInfo.SspId == Guid.Empty && !tInfo.IsKeywordsColumn)
            {
                object customProperty = tField.GetCustomProperty("SspId");
                if (customProperty != null)
                {
                    tInfo.SspId = new Guid(customProperty.ToString());
                }
            }
            taxonomyFields[tField.InternalName] = tInfo;
        }

        private void AddUserFieldToCache(IAveFieldUser userField)
        {
            userFields.Add(userField.InternalName);
        }

        internal Dictionary<string, object> EnsureRestoringItemCurrentVersionDocData(AveBaseItemInfo baseItemInfo, IAveItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.EnsureRestoringItemCurrentVersionDocData"))
            {
                if (baseItemInfo.RowId > 0)
                {
                    try
                    {
                        if (AveSPItem.RestoringItemCurrentVersionDocData == null || baseItemInfo.GUID != (Guid)AveSPItem.RestoringItemCurrentVersionDocData["Id"] || (baseItemInfo.GUID == (Guid)AveSPItem.RestoringItemCurrentVersionDocData["Id"] && baseItemInfo.Version != (int)AveSPItem.RestoringItemCurrentVersionDocData["UIVersion"]))
                        {
                            AveSPItem.RestoringItemCurrentVersionDocData = item.GetItemCurrentVersionDocData(baseItemInfo);
                            AveSPItem.RestoringItemCurrentVersionDocData["HasUniqueRoleAssignments"] = AveSPItem.RestoringItemCurrentVersionDocData.ContainsKey("HasUniqueRoleAssignments") && Convert.ToBoolean(AveSPItem.RestoringItemCurrentVersionDocData["HasUniqueRoleAssignments"]);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, WrapperBackupResource.AWBConnotFindItem, baseItemInfo.GUID, e.ToString());
                    }
                }
                else
                {
                    AveSPItem.RestoringItemCurrentVersionDocData = null;
                }
            }

            return AveSPItem.RestoringItemCurrentVersionDocData;
        }


        internal void CreateExcelOrSheetIfNeed()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.CreateExcelOrSheetIfNeed"))
            {
                if (NeedExportExcel && mNeedCreateExcel)
                {
                    if (string.IsNullOrEmpty(ExcelPath))
                    {
                        throw new NoNullAllowedException();
                    }
                    //this.SetExcelImportPath();
                    mFMExcel = new FMExcelOpenXml();
                    this.mTitle = this.RootFolderPath.Replace(@"/", "_");
                    mFMExcel.CreateExcel(ExcelPath + this.mTitle + ".xlsx");
                    mCurrentSheetCount++;
                    try
                    {
                        mFMExcel.WriteHeader(ImportExcelHeaders);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperBackupResource.AWBWriteExcelHeaderError, e.ToString());
                        mFMExcel.WriteHeader(ImportExcelHeaders);
                    }
                    mNeedCreateExcel = false;
                }
                if (mCurrentItemCountPerSheet == mItemCountPerSheet)
                {
                    mCurrentItemCountPerSheet = 0;
                    if (mCurrentSheetCount == mSheetCountPerExcel)
                    {
                        mFMExcel.Dispose();
                        mFMExcel = null;
                        mFMExcel = new FMExcelOpenXml();
                        mCurrentExcelFileCount++;
                        mFMExcel.CreateExcel(ExcelPath + this.mTitle + "_" + mCurrentExcelFileCount + ".xlsx");
                        mCurrentSheetCount = 1;
                    }
                    else
                    {
                        mFMExcel.CreateSheet();
                        mCurrentSheetCount++;
                    }
                    try
                    {
                        mFMExcel.WriteHeader(ImportExcelHeaders);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperBackupResource.AWBWriteExcelHeaderError, e.ToString());
                        mFMExcel.WriteHeader(ImportExcelHeaders);
                    }
                }
            }
        }

        public void ExportItemDataToExcel(Dictionary<string, object> listItemData, string path)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.ExportItemDataToExcel"))
            {
                try
                {
                    if (listItemData == null || !listItemData.ContainsKey("#tp_ID"))
                    {
                        return;
                    }
                    using (AvePerformanceScope scope = new AvePerformanceScope("Backup.ExportItemDataToExcel.CreateSheet"))
                    {
                        CreateExcelOrSheetIfNeed();
                    }
                    mCurrentItemCountPerSheet++;
                    Dictionary<string, string> itemData;
                    itemData = new Dictionary<string, string>();
                    using (AvePerformanceScope scope = new AvePerformanceScope("Backup.ExportItemDataToExcel.SetField"))
                    {
                        itemData.Add("Path", path);
                        itemData.Add("ID:=Counter", listItemData["#tp_ID"].ToString());
                        foreach (string key in listItemData.Keys)
                        {
                            try
                            {
                                string str = string.Empty;
                                if (key.Trim('#').StartsWith("tp_", StringComparison.Ordinal))
                                {
                                    str = key.Substring(key.IndexOf("tp_", StringComparison.Ordinal) + 3);
                                }
                                else
                                {
                                    str = key.TrimStart('#');
                                }
                                string fieldValue = listItemData[key].ToString();
                                //if ((!metadataFields.ContainsKey(key) && !FieldNameTypeDic.ContainsKey(str)) || metadataFields.ContainsValue(key))
                                //{
                                //    continue;
                                //}
                                if (metadataFields.ContainsKey(key))
                                {
                                    itemData.Add(FieldNameTypeDic[metadataFields[key]].TitleAndGuiType, fieldValue);
                                    continue;
                                }
                                if (FieldNameTypeDic.ContainsKey(str))
                                {
                                    if (FieldNameTypeDic[str].TypeAsString.Equals("User"))
                                    {
                                        int iId;
                                        if (Int32.TryParse(fieldValue, out iId))
                                        {
                                            if (this.mAveParentSite.DataCache.UserCache.Contains(iId))
                                            {
                                                fieldValue = this.mAveParentSite.DataCache.UserCache.GetUserInfo(iId).Login;
                                            }
                                            else
                                            {
                                                IAveUser user = this.SPList.ParentWeb.Users.GetByID(iId);
                                                fieldValue = user.LoginName;
                                            }
                                        }
                                    }
                                    if (FieldNameTypeDic[str].TypeAsString.Equals("Lookup"))
                                    {
                                        IAveListItem lookupListItem = null;
                                        if (mLookupListDic.ContainsKey(new Guid(mLookupFieldListId[FieldNameTypeDic[str].Title].Key)))
                                        {
                                            lookupListItem = mLookupListDic[new Guid(mLookupFieldListId[FieldNameTypeDic[str].Title].Key)].Items.GetById(Convert.ToInt32(fieldValue));
                                        }
                                        else
                                        {
                                            IAveList lookupList = this.SPList.Lists.GetById(new Guid(mLookupFieldListId[FieldNameTypeDic[str].Title].Key));
                                            lookupListItem = lookupList.Items.GetById(Convert.ToInt32(fieldValue));
                                            mLookupListDic.Add(new Guid(mLookupFieldListId[FieldNameTypeDic[str].Title].Key), lookupList);
                                        }
                                        itemData.Add(FieldNameTypeDic[str].TitleAndGuiType, lookupListItem[mLookupFieldListId[FieldNameTypeDic[str].Title].Value].ToString());
                                        continue;
                                    }
                                    itemData.Add(FieldNameTypeDic[str].TitleAndGuiType, fieldValue);
                                }
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.ERROR, "Export Item Data To Excel :" + e.ToString());
                            }
                        }

                        string tmpName = "Name";
                        try
                        {
                            tmpName = this.mAveParentSite.LanguageProcessor.GetTitleWithRealName("Name", this.mAveParentSite.SPSite.RootWeb.Language);
                        }
                        catch (Exception ex)
                        {
                            log.Log(AveLogLevel.DEBUG, "Get Title With Name Error:" + ex.ToString());
                        }
                        if (!itemData.ContainsKey(tmpName + ":=File"))
                        {
                            itemData.Add(tmpName + ":=File", path.Substring(path.TrimEnd('/').LastIndexOf('/') + 1));
                        }
                    }
                    using (AvePerformanceScope scope = new AvePerformanceScope("Backup.ExportItemDataToExcel.WriteLine"))
                    {
                        mFMExcel.WriteLine(itemData);
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.ERROR, "Export Item Data To Excel :" + e.ToString());
                }
            }
        }

        public void ExportFullTextIndex(IAveBackupStream output, Dictionary<string, object> customFieldValues)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.ExportFullTextIndex"))
            {
                var index = new FullTextIndex()
                {
                    TimeZoneInfoID = ParentWeb.TimeZoneInfoId,
                };
                if (customFieldValues != null)
                {
                    index.SetCustomColumnValues(customFieldValues);
                }
                output.WriteMetadata(AveMetadataType.FullTextIndex, index);
            }
        }

       /* public void CloseExcelImport()
        {
            if (this.mFMExcel != null)
            {
                this.mFMExcel.Dispose();
            }
        }
*/
        public void CleanListData()
        {
            if (this.SPList != null)
            {
                this.SPList.CleanListData();
                this.mSPList = null;
            }
        }

        public IList<AveViewInfo> GetViews(Guid docId)
        {
            List<AveViewInfo> viewList = null;
            lock (this.ViewCache)
            {
                if (this.ViewCache.ContainsKey(docId))
                {
                    viewList = this.ViewCache[docId];
                    this.ViewCache.Remove(docId);
                }
            }
            return viewList;
        }

        public void BeforeBackupItems()
        {
            DisableInformationRightsManagementSettings();
            //DisableForceCheckOut();
        }

        public void AfterBackupItems()
        {
            EnableInformationRightsManagementSettings();
            //EnableForceCheckOut();
        }

        private void DisableInformationRightsManagementSettings()
        {
            mDisableInformationRightsManagement = WrapperConfiguration.WrapperConfigurationForBPOS.DisableInformationRightsManagement;
            if (SPList != null && SPList.IrmEnabled && mDisableInformationRightsManagement)
            {
                mInformationRightsManagementDic = GetInformationRightsManagementSettingsInfo();
                if (mInformationRightsManagementDic != null)
                {
                    SPList.InformationRightsManagementSettings.Reset();
                    SPList.IrmEnabled = false;
                    SPList.Update();
                    mIrmEnabled = false;
                }
            }
        }

        private void EnableInformationRightsManagementSettings()
        {
            if (!mIrmEnabled && mInformationRightsManagementDic != null)
            {
                SetInformationRightsManagementSettingsInfo(mInformationRightsManagementDic);
                mIrmEnabled = true;
            }
        }

        private Dictionary<string, object> GetInformationRightsManagementSettingsInfo()
        {
            Dictionary<string, object> InformationRightsManagementDic = null;
            IAveInformationRightsManagementSettings InformationRightsManagementSettings = SPList.InformationRightsManagementSettings;
            TimeSpan timeSpan = InformationRightsManagementSettings.DocumentLibraryProtectionExpireDate.AddDays(1.0) - DateTime.UtcNow;
            if (timeSpan.Ticks > 0)
            {
                InformationRightsManagementDic = new Dictionary<string, object>();
                InformationRightsManagementDic.Add("IrmExpire", SPList.IrmExpire);
                InformationRightsManagementDic.Add("IrmReject", SPList.IrmReject);
                InformationRightsManagementDic.Add("PolicyTitle", InformationRightsManagementSettings.PolicyTitle);
                InformationRightsManagementDic.Add("PolicyDescription", InformationRightsManagementSettings.PolicyDescription);
                InformationRightsManagementDic.Add("DocumentLibraryProtectionExpireDate", InformationRightsManagementSettings.DocumentLibraryProtectionExpireDate);
                InformationRightsManagementDic.Add("DisableDocumentBrowserView", InformationRightsManagementSettings.DisableDocumentBrowserView);
                InformationRightsManagementDic.Add("EnableGroupProtection", InformationRightsManagementSettings.EnableGroupProtection);
                InformationRightsManagementDic.Add("GroupName", InformationRightsManagementSettings.GroupName);
                InformationRightsManagementDic.Add("AllowPrint", InformationRightsManagementSettings.AllowPrint);
                InformationRightsManagementDic.Add("AllowScript", InformationRightsManagementSettings.AllowScript);
                InformationRightsManagementDic.Add("AllowWriteCopy", InformationRightsManagementSettings.AllowWriteCopy);
                InformationRightsManagementDic.Add("EnableDocumentAccessExpire", InformationRightsManagementSettings.EnableDocumentAccessExpire);
                InformationRightsManagementDic.Add("DocumentAccessExpireDays", InformationRightsManagementSettings.DocumentAccessExpireDays);
                InformationRightsManagementDic.Add("EnableLicenseCacheExpire", InformationRightsManagementSettings.EnableLicenseCacheExpire);
                InformationRightsManagementDic.Add("LicenseCacheExpireDays", InformationRightsManagementSettings.LicenseCacheExpireDays);
            }
            return InformationRightsManagementDic;
        }

        private void SetInformationRightsManagementSettingsInfo(Dictionary<string, object> InformationRightsManagementDic)
        {
            SPList.IrmEnabled = true;
            SPList.IrmExpire = Convert.ToBoolean(InformationRightsManagementDic["IrmExpire"]);
            SPList.IrmReject = Convert.ToBoolean(InformationRightsManagementDic["IrmReject"]);
            IAveInformationRightsManagementSettings InformationRightsManagementSettings = SPList.InformationRightsManagementSettings;
            InformationRightsManagementSettings.PolicyTitle = InformationRightsManagementDic["PolicyTitle"].ToString();
            InformationRightsManagementSettings.PolicyDescription = InformationRightsManagementDic["PolicyDescription"].ToString();
            InformationRightsManagementSettings.DocumentLibraryProtectionExpireDate = Convert.ToDateTime(InformationRightsManagementDic["DocumentLibraryProtectionExpireDate"]);
            InformationRightsManagementSettings.DisableDocumentBrowserView = Convert.ToBoolean(InformationRightsManagementDic["DisableDocumentBrowserView"]);
            InformationRightsManagementSettings.EnableGroupProtection = Convert.ToBoolean(InformationRightsManagementDic["EnableGroupProtection"]);
            InformationRightsManagementSettings.GroupName = InformationRightsManagementDic["GroupName"].ToString();
            InformationRightsManagementSettings.AllowPrint = Convert.ToBoolean(InformationRightsManagementDic["AllowPrint"]);
            InformationRightsManagementSettings.AllowScript = Convert.ToBoolean(InformationRightsManagementDic["AllowScript"]);
            InformationRightsManagementSettings.AllowWriteCopy = Convert.ToBoolean(InformationRightsManagementDic["AllowWriteCopy"]);
            InformationRightsManagementSettings.EnableDocumentAccessExpire = Convert.ToBoolean(InformationRightsManagementDic["EnableDocumentAccessExpire"]);
            InformationRightsManagementSettings.DocumentAccessExpireDays = Convert.ToInt32(InformationRightsManagementDic["DocumentAccessExpireDays"]);
            InformationRightsManagementSettings.EnableLicenseCacheExpire = Convert.ToBoolean(InformationRightsManagementDic["EnableLicenseCacheExpire"]);
            InformationRightsManagementSettings.LicenseCacheExpireDays = Convert.ToInt32(InformationRightsManagementDic["LicenseCacheExpireDays"]);
            InformationRightsManagementSettings.Update();
            SPList.Update();
        }

        public List<AveEventReceiverInfo> GetEventReceivers()
        {
            var events = AveSPEventReceiver.CreateInstance(this);
            return events.GetReceivers();
        }

        #region add for DPM

        public void ExportContentTypes(IAveBackupStream output, AveBackupOption backupContentTypeOption = null)
        {
            var contentTypes = AveSPContentTypeCollection.CreateInstance(this);
            var result = contentTypes.GetContentTypeCollectionInfoObj();
            if (backupContentTypeOption != null && backupContentTypeOption.BackupNintexForm)
            {
                BackupNintexForm(result);
            }

            if (backupContentTypeOption != null && backupContentTypeOption.BeforeExportContentTypesAction != null)
            {
                backupContentTypeOption.BeforeExportContentTypesAction(result);
            }
            output.WriteMetadata(AveMetadataType.ListContentType.ToString(), result);
        }
        public void ExportUserCustomActions(IAveBackupStream output)
        {
            AveSPUserCustomActionCollection spUserCustomActionCollection = new AveSPListUserCustomActionCollection(this);
            output.WriteMetadata(AveMetadataType.ListUserCustomAction, spUserCustomActionCollection.GetUserCustomActionInfos());
        }
        private void BackupNintexForm(AveContentTypeCollectionInfo contentTypeCollectionInfo )
        {
            var nitexFormProcessor = new AveNintexForm(this.SPList);
            contentTypeCollectionInfo.ContentTypes.ForEach(ctInfo =>
            {
                ctInfo.NintexFormXml = nitexFormProcessor.ExportNintexForm(ctInfo.Id, ctInfo.XmlDocuments);
            });
        }

        public void ExportFields(IAveBackupStream output, bool includeGroup = true, AveBackupOption backupColumnOption = null)
        {
            if (includeGroup)
            {
                this.CacheGroupsFromUserField();
                this.ExportGroupCache(output);
            }
            AveSPFieldCollection fields = AveSPFieldCollection.CreateInstance(this);
            var fieldCollectionInfo = null == backupColumnOption ? fields.GetFieldInfoObj() : fields.GetFieldInfoObj(backupColumnOption);
            if (backupColumnOption != null && backupColumnOption.BeforeExportFieldsAction != null)
            {
                backupColumnOption.BeforeExportFieldsAction(fieldCollectionInfo);
            }
            if (fieldCollectionInfo.RelatedMetadataInfo.Count > 0)
            {
                output.WriteMetadata(AveMetadataType.MetadataService, fieldCollectionInfo.RelatedMetadataInfo);
            }
            output.WriteMetadata(AveMetadataType.ListField, fieldCollectionInfo.AveSchemaXml);
        }
        #endregion


        public void Dispose()
        {
            AfterBackupItems();
            CleanListData();
        }
    }
}