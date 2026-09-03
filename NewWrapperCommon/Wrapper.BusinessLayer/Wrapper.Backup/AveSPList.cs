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
using AvePoint.Wrapper.Core.SPBackupDto;
using AvePoint.Wrapper.Mapping;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Common;
using AvePoint.Wrapper.Core.SPBackup;
using AvePoint.Wrapper.Common.Office;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml;
using AvePoint.Wrapper.Resource.Backup;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPList : AvePoint.Wrapper.Backup.IAveSPList, IDisposable, ISPListExport
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly object lockObject = new object();
        private FMExcelOpenXml mFMExcel;
        private Dictionary<int, ExportExcelData> mExportExcelDatas;
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
        public event Func<IAveField, bool> OnAddColumnToExcelFile;
        protected bool OnAddColumnToExcelFileSafe(IAveField field)
        {
            if (OnAddColumnToExcelFile != null)
            {
                return OnAddColumnToExcelFile(field);
            }
            return true;
        }
        private AveListDataCache mDataCache = new AveListDataCache();
        private Dictionary<Guid, List<AveViewInfo>> mViewCache = null;//= new Dictionary<Guid, List<AveViewInfo>>();

        //private Dictionary<string, AveFieldType> mDisplayColumns = null;
        private Dictionary<string, AveSPField> mDisplayColumns = null;
        private object mDisplayColumnsInitLock = new object();

        private Dictionary<Guid, int> mSolutionStatus = null;
        private string mExcelPath = string.Empty;
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

        private IAveSOIntegrationUtility mSOIntegrationUtil;
        private Dictionary<string, FieldInfoForExcel> mFieldNameTypeDic;
        private Dictionary<string, string> mImportExcelHeaders;
        /// <summary>
        /// ContentManager 源端365有多线程备份的逻辑，这样判断是否为view时，会有多个thread操作mViewCache，导致mViewCache这个集合出现问题
        /// </summary>
        private object locker = new object();
        //private object mLockForMultiCMBackup = new object();

        private bool backupLookUpDisplayValue = false;
        private bool backupItemTPGUIDofLookupValue = false;
        private bool backupItemLeafNameOfLookupValue = false;
        private bool backupItemLookupDisplayValueForRestore = false;


        //<ListId,<ColumnInternalName,<ItemId,ColumnValue>>>
        private Dictionary<string, Dictionary<string, Dictionary<string, string>>> lookUpListItemIDAndValues = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
        public bool BackupLookUpDisplayValue
        {
            get { return backupLookUpDisplayValue; }
            set { backupLookUpDisplayValue = value; }
        }

        public bool BackupItemTPGUIDofLookupValue
        {
            get { return backupItemTPGUIDofLookupValue; }
            set { backupItemTPGUIDofLookupValue = value; }
        }

        public bool BackupItemLeafNameOfLookupValue
        {
            get { return backupItemLeafNameOfLookupValue; }
            set { backupItemLeafNameOfLookupValue = value; }
        }

        public bool BackupItemLookupDisplayValueForRestore
        {
            get { return backupItemLookupDisplayValueForRestore; }
            set { backupItemLookupDisplayValueForRestore = value; }
        }
        internal Dictionary<int, ExportExcelData> ExportExcelDatas
        {
            get
            {
                if (mExportExcelDatas == null)
                {
                    mExportExcelDatas = new Dictionary<int, ExportExcelData>();
                }
                return mExportExcelDatas;
            }
        }
        private Dictionary<string, AveLookupFieldInfo> lookupFields = null;
        private List<string> userFields = null;
        private Dictionary<string, AveTaxFieldInfo> taxonomyFields = null;
        public Dictionary<string, AveLookupFieldInfo> LookupFields
        {
            get
            {
                lock (locker)
                {
                    if (lookupFields == null)
                    {
                        EnsureListFieldCache();
                    }
                    return lookupFields;
                }
            }
        }

        private Dictionary<int, object> mSocialThreadCache = null;
        public Dictionary<int, object> SocialThreadCache
        {
            get
            {
                return mSocialThreadCache;
            }
            set
            {
                mSocialThreadCache = value;
            }
        }

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

        private void EnsureListFieldCache()
        {
            lookupFields = new Dictionary<string, AveLookupFieldInfo>();
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
                        else if (field.Type == AveFieldType.Lookup || (field.Type == AveFieldType.Invalid && field.TypeAsString.Equals("Facilities", StringComparison.OrdinalIgnoreCase)))
                        {
                            IAveFieldLookup lookupField = field as IAveFieldLookup;
                            AddLookupFieldToCache(lookupField);
                        }
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

        private void AddLookupFieldToCache(IAveFieldLookup lookupField)
        {
            IAveWeb web = mAveSPWeb.SPWeb;
            try
            {
                AveLookupFieldInfo fieldInfo = new AveLookupFieldInfo();
                fieldInfo.Id = lookupField.ID;
                fieldInfo.LookupWeb = lookupField.LookupWebId;
                if (String.IsNullOrEmpty(lookupField.LookupList) || !AveTypeHelper.IsGuid(lookupField.LookupList))
                {
                    return;
                }
                fieldInfo.LookupList = new Guid(lookupField.LookupList);
                fieldInfo.LookupField = lookupField.LookupField;
                if (mAveSPWeb.SPWeb.ID != lookupField.LookupWebId)
                {
                    web = mAveParentSite.SPSite.OpenWeb(lookupField.LookupWebId);
                }
                IAveList lookupList = web.Lists[new Guid(lookupField.LookupList)];
                fieldInfo.LookupColumnRowNameForQuery = lookupList.Fields.GetFieldByInternalName(lookupField.LookupField).ColName;
                fieldInfo.LookupColumnDisplayName = lookupField.InternalName;
                lookupFields[lookupField.InternalName] = fieldInfo;
            }
            finally
            {
                if (web.ID != mAveSPWeb.SPWeb.ID)
                {
                    web.Dispose();
                    web = null;
                }
            }
        }

        private void AddUserFieldToCache(IAveFieldUser userField)
        {
            userFields.Add(userField.InternalName);
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

        public AveSPSite ParentSite
        {
            get { return mAveParentSite; }
        }

        public AveIndexCache AveIndexCache
        {
            get
            {
                lock (locker)
                {
                    if (this.indexCache == null)
                    {
                        this.indexCache = new AveIndexCache(this);
                    }
                    return this.indexCache;
                }
            }
        }

        public AveColumnCache CustomColumnCache
        {
            get { return this.AveIndexCache.CustomColumnCache; }
            set { this.AveIndexCache.CustomColumnCache = value; }
        }
        public bool HasUniqueRoleAssignments
        {
            get { return mSPList.HasUniqueRoleAssignments; }
        }

        public AveSPWeb ParentWeb
        {
            get { return mAveSPWeb; }
        }

        public AveSPListFieldCollection Fields
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

        private AveSPListContentTypes CachedContentTypes
        {
            get
            {
                try
                {
                    InitContentTypes(new AveSPListContentTypeCollection(this));
                }
                catch (Exception e)
                {
                    log.Warn("Init list ContentTypes error,list title:{0}, exception:{1}.", mTitle, e.ToString());
                }

                return mCachedContentTypes;
            }
        }

        public Dictionary<Guid, List<AveViewInfo>> ViewCache
        {
            get
            {
                //把锁加在这里，第一个thread进来时会把mViewCache 初始化完毕，这样后面的thread既有不会把初始化一半的mViewCache return 
                //也不会把再走InitViewCache()将mViewCache重新new
                lock (locker)
                {
                    if (mViewCache == null)
                    {
                        InitViewCache();
                    }
                    return mViewCache;
                }
            }
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
                    mSOIntegrationUtil = this.ParentSite.ObjectModelFactory.CreateSOIntegrationUtility(this.ParentSite.SPSite, this.SPList);
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
                    lock (mDisplayColumnsInitLock)
                    {
                        if (mDisplayColumns == null)
                        {
                            Fields.Load(true);
                            mDisplayColumns = GetColumns(FullTextIndexLevel.IncludeDefaultViewColumns);
                        }
                    }
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

        public AveSPList(AveSPWeb _AveWeb, Guid _id, string _title)
            : this(_AveWeb, _id, _title, false)
        {
        }

        public AveSPList(AveSPWeb _AveWeb, Guid _id, string _title, bool getFullSchema)
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
                mPath = mAveSPWeb.Name + "\\" + AvePoint.GCommon.AveConverter.EncodeSpecialChar(mTitle);
                if (mId == Guid.Empty && string.Compare(AveConstants.SYSTEM_FOLDER, mTitle, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    mIsSystemList = true;
                    mScopeId = _AveWeb.ScopeId;
                    return;
                }
                InitSPList(_id, _title);
                mScopeId = mSPList.RoleAssignments.ID;
                mFields = null;
                mCachedContentTypes = null;
                //mFields = new AveSPListFieldCollection(this);
                //try
                //{
                //    InitContentTypes(new AveSPListContentTypeCollection(this));
                //}
                //catch (Exception e)
                //{
                //    log.Warn("Init list ContentTypes error,list title:{0}, exception:{1}.", mTitle, e.ToString());
                //}
                //GetListViews();
            }
        }

        public AveSPList(Wrapper.Core.SPBackup.ISPWebExport backupWeb, IAveList list)
            : this((AveSPWeb)backupWeb, list.ID, list.Title)
        {
        }

        internal void InitSPList(Guid _id, string _title)
        {
            try
            {
                mSPList = mAveSPWeb.SPWeb.Lists.GetById(_id);
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
            if (mSPList.BaseTemplate == AveListTemplateType.SolutionCatalog)
            {
                mSolutionStatus = new Dictionary<Guid, int>();
                if (mSPList.ParentWeb.Site.Solutions != null)
                {
                    foreach (IAveUserSolution solution in mSPList.ParentWeb.Site.Solutions)
                    {
                        mSolutionStatus.Add(solution.SolutionId, (int)solution.Status);
                    }
                }
            }
        }

        /// <summary>
        /// Init View Cache
        /// </summary>
        internal void InitViewCache()
        {
            if (mViewCache == null)
            {
                try
                {
                    mViewCache = new Dictionary<Guid, List<AveViewInfo>>();
                    if (mSPList != null)
                    {
                        if (mQueryService != null)
                        {
                            if (mSPList.DefaultView != null)
                            {
                                mQueryService.GetViews(mViewCache, this.mAveParentSite.SPSite.ID, this.Id, SPList.DefaultView.ID);
                            }
                            else
                            {
                                mQueryService.GetViews(mViewCache, this.mAveParentSite.SPSite.ID, this.Id, Guid.Empty);
                            }
                        }
                        //for O365
                        else
                        {
                            mSPList.GetViews(mViewCache);
                        }
                        GenerateMappingForSpotLight(mViewCache.Values.ToList());
                    }
                    if (mViewCache.Count == 0)
                    {
                        log.Log(AveLogLevel.WARN, "Can not get list views. List Title:{0}", mSPList == null ? string.Empty : mSPList.Title);
                    }
                }
                catch (Exception e)
                {
                    log.Warn("Get List views error,List title:{0}, exception:{1}.", mSPList.Title, e.ToString());
                }
            }
        }
        /// <summary>
        /// For efficiency reasons, get views spot light together.
        /// </summary>
        /// <param name="viewCache"></param>
        private void GenerateMappingForSpotLight(List<List<AveViewInfo>> viewCache)
        {
            //List<string> -> Type, LeafName
            var mappings = new Dictionary<int, List<string>>();
            foreach (var viewInfos in viewCache)
            {
                foreach (var viewInfo in viewInfos)
                {
                    if (string.IsNullOrEmpty(viewInfo.ListViewXml))
                    {
                        continue;
                    }
                    XmlDocument xd = new XmlDocument();
                    try
                    {
                        // spot light format: 
                        // |folderId=itemId;itemId;itemId|folderId=itemId;|
                        var spotLightChar = new char[] { '|', ';', '=' };
                        List<int> itemIds = new List<int>();
                        xd.LoadXml(viewInfo.ListViewXml);
                        XmlNode spotlightInfoNode = xd.SelectSingleNode("View/SpotlightInfo");
                        if (spotlightInfoNode != null && (!string.IsNullOrEmpty(spotlightInfoNode.InnerText)))
                        {
                            foreach (var id in spotlightInfoNode.InnerText.Split(spotLightChar, StringSplitOptions.RemoveEmptyEntries))
                            {
                                int itemId;
                                if (int.TryParse(id, out itemId) && itemId > 0)
                                {
                                    try
                                    {
                                        if (mappings.ContainsKey(itemId))
                                        {
                                            var spotLightInfo = mappings[itemId];
                                            if (spotLightInfo == null)
                                            {
                                                continue;
                                            }
                                            viewInfo.MappingForSpotlight[itemId] = spotLightInfo;
                                        }
                                        else
                                        {
                                            var item = mSPList.GetItemById(itemId);
                                            var spotLightInfo = new List<string>
                                            {
                                                ((int)item.FileSystemObjectType).ToString(),
                                                item.FileSystemObjectType == AveFileSystemObjectType.File?
                                                    item.File.ServerRelativeUrl:item.Folder.ServerRelativeUrl
                                            };
                                            mappings[itemId] = spotLightInfo;
                                            viewInfo.MappingForSpotlight[itemId] = spotLightInfo;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        mappings[itemId] = null;
                                        log.Warn("Get item by Id failed. Id: {0}. Error: {1}", itemId, e);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Warn("Generate mapping for spot light failed. View: {0}, Error: {1}", viewInfo.ListViewXml, e);
                    }
                }
            }
        }

        internal void SetFieldNameTypeDicValue()
        {
            ImportExcelHeaders.Add("Path", string.Empty);
            List<string> keys = new List<string>();
            foreach (IAveField field in this.SPList.Fields)
            {
                if (NeedExportToExcel(keys, field) && OnAddColumnToExcelFileSafe(field))
                {
                    GetFieldInfo(field, keys);
                }
            }
        }

        private bool NeedExportToExcel(List<string> keys, IAveField field)
        {
            return !field.ReadOnlyField
                && !field.Title.Equals("ID")
                && !field.InternalName.Equals("ContentType", StringComparison.OrdinalIgnoreCase)
                && !field.Hidden
                && field.InternalName != null
                && !keys.Contains(field.Title);
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
            if (!ImportExcelHeaders.ContainsKey(field.Title + ":=" + FieldInternalTypeAndGuiTypeMapping.GetGuiTypeByInternalType(field.TypeAsString)))
            {
                keys.Add(field.Title);
                ImportExcelHeaders.Add(field.Title + ":=" + FieldInternalTypeAndGuiTypeMapping.GetGuiTypeByInternalType(field.TypeAsString), string.Empty);
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

        public void CachePrincipalFromPermission(int value)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.CachePrincipalFromPermission"))
            {
                if (RoleAssignmentCache == null)
                {
                    AveRoleAssignments roleAssignments = AveRoleAssignments.CreateInstance(this);
                    RoleAssignmentCache = roleAssignments.GetRoleAssignments();
                }
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
                        if (!mDataCache.PrincipalIdAlreadyExists(principalId))
                        {
                            var user = mAveSPWeb.ParentSite.DataCache.GetUserInfo(principalId);
                            if (user != null && (value & 1) != 0)
                            {
                                mDataCache.AddToCache(principalId, user);
                                continue;
                            }
                            var group = mAveSPWeb.ParentSite.DataCache.GetGroupInfo(principalId);
                            if (group != null && (value & 2) != 0)
                            {
                                mDataCache.AddToCache(principalId, group);
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
                        if (!mDataCache.PrincipalIdAlreadyExists(userId))
                        {
                            AveUserInfo userInfo = mAveSPWeb.ParentSite.DataCache.GetUserInfo(userId);
                            if (userInfo == null)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperBackupResource.AuthorTypeError, userId);
                                return;
                            }
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
                if (!mDataCache.PrincipalIdAlreadyExists(userId))
                {
                    AveUserInfo userInfo = mAveSPWeb.ParentSite.DataCache.GetUserInfo(userId);
                    if (userInfo == null)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperBackupResource.UserTypeError, userId);
                        return;
                    }
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
                            if (userField.SelectionGroup != 0)
                            {
                                AveGroupInfo groupInfo = (AveGroupInfo)mAveSPWeb.ParentSite.DataCache.GetGroupInfo(userField.SelectionGroup);
                                if (groupInfo != null)
                                {
                                    mDataCache.AddToCache(userField.SelectionGroup, groupInfo);
                                }
                            }
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
            output.WriteMetadata(AveMetadataType.UserCache, mDataCache.GetUsersForExport());
        }

        public void ExportGroupCache(IAveBackupStream output)
        {
            output.WriteMetadata(AveMetadataType.GroupCache, mDataCache.GetGroupsForExport());
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
        /// <summary>
        /// 获取list field,可返回Default view Display Column，非隐藏的column，所有column。
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        internal Dictionary<string, AveSPField> GetColumns(FullTextIndexLevel level)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.GetColumns"))
            {
                switch (level)
                {
                    case FullTextIndexLevel.IncludeDefaultViewColumns:
                        return Fields.FieldMap.Values.Where(field => field.IsDisplayColumn).ToDictionary(field => field.BackupName, field => field);
                    case FullTextIndexLevel.IncludeAllVisiableColumns:
                        return Fields.IdFieldMap.Values.Where(field => (field.IsDisplayColumn || !field.IsHidden)).ToDictionary(field => field.BackupName, field => field);
                    //已弃用，原来是指所有DB中有值的column
                    case FullTextIndexLevel.IncludeAllColumns:
                        return Fields.FieldMap.Values.ToDictionary(field => field.BackupName, field => field);
                    case FullTextIndexLevel.IncludeAllColumnsAndSystemColumns:
                        return Fields.IdFieldMap.Values.Distinct().ToDictionary(field => field.BackupName, field => field);
                    default:
                        return Fields.IdFieldMap.Values.Distinct().ToDictionary(field => field.BackupName, field => field);
                }
            }
        }

        internal void TryGetContentType(byte[] contentTypeId, out string contentTypeName)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.TryGetContentType"))
            {
                contentTypeName = string.Empty;
                var contentTypes = CachedContentTypes;
                if (contentTypes != null)
                {
                    contentTypes.TryGet(contentTypeId, out contentTypeName);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseItemInfo"></param>
        /// <param name="item"></param>
        /// <param name="grandfatherId">parentFolder.ParentFolder.UniqueId</param>
        internal void EnsureRestoringItemCurrentVersionDocData(AveBaseItemInfo baseItemInfo, IAveItem item, Guid grandfatherId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.EnsureRestoringItemCurrentVersionDocData"))
            {
                if (baseItemInfo.RowId > 0)
                {
                    //try
                    //{
                    try
                    {
                        if (AveSPItem.RestoringItemParentCurrentVersionDocData == null || baseItemInfo.ParentId != (Guid)AveSPItem.RestoringItemParentCurrentVersionDocData["Id"])
                        {
                            AveBaseItemInfo parentItemInfo = new AveBaseItemInfo() { SiteId = baseItemInfo.SiteId, ParentId = grandfatherId, GUID = baseItemInfo.ParentId, ServerRelativeUrl = baseItemInfo.ParentFolderRelativeUrl, ItemType = AveItemType.Folder };
                            AveSPItem.RestoringItemParentCurrentVersionDocData = item.GetItemCurrentVersionDocData(parentItemInfo);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, WrapperBackupResource.AWBEnsureItemVisionInfo, e.ToString());
                    }
                    try
                    {
                        if (AveSPItem.RestoringItemCurrentVersionDocData == null || AveSPItem.RestoringItemCurrentVersionDocData.Count == 0 || baseItemInfo.GUID != (Guid)AveSPItem.RestoringItemCurrentVersionDocData["Id"] || (baseItemInfo.GUID == (Guid)AveSPItem.RestoringItemCurrentVersionDocData["Id"] && baseItemInfo.Version != (int)AveSPItem.RestoringItemCurrentVersionDocData["UIVersion"]))
                        {
                            AveSPItem.RestoringItemCurrentVersionDocData = item.GetItemCurrentVersionDocData(baseItemInfo);
                            if (!AveSPItem.RestoringItemCurrentVersionDocData.ContainsKey("HasUniqueRoleAssignments") && AveSPItem.RestoringItemCurrentVersionDocData.ContainsKey("ScopeId"))
                            {
                                AveSPItem.RestoringItemCurrentVersionDocData["HasUniqueRoleAssignments"] = !Guid.Equals(AveSPItem.RestoringItemParentCurrentVersionDocData["ScopeId"], AveSPItem.RestoringItemCurrentVersionDocData["ScopeId"]);
                            }
                            //由于ScopId信息初始化在DocInfo，但是如果只备份permission的时候，无法知道scopId信息，所以在初始化的时候给scopeId赋值
                            if (baseItemInfo.ScopeId == Guid.Empty && AveSPItem.RestoringItemCurrentVersionDocData.ContainsKey("ScopeId"))
                            {
                                baseItemInfo.ScopeId = (Guid)AveSPItem.RestoringItemCurrentVersionDocData["ScopeId"];
                            }
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
                    this.mTitle = this.ServerRelativeUrl.Trim('/').Replace(@"/", "_");
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
                                string fieldName = string.Empty;
                                if (key.StartsWith("#tp_", StringComparison.Ordinal))
                                {
                                    fieldName = key.Substring(3);
                                }
                                else
                                {
                                    fieldName = key.TrimStart('#');
                                }
                                object fieldValue = listItemData[key];
                                string metadataFieldName;
                                if (metadataFields.TryGetValue(key, out metadataFieldName))
                                {
                                    fieldName = metadataFieldName;
                                }
                                FieldInfoForExcel fieldInfo;
                                if (FieldNameTypeDic.TryGetValue(fieldName, out fieldInfo))
                                {
                                    string fieldTypeAsString = fieldInfo.TypeAsString;
                                    if (fieldTypeAsString.Equals("User", StringComparison.OrdinalIgnoreCase))
                                    {
                                        int userId;
                                        if (Int32.TryParse(fieldValue.ToString(), out userId))
                                        {
                                            AveUserInfo userInfo = this.mAveParentSite.DataCache.UserCache.GetUserInfo(userId);
                                            if (userInfo != null)
                                            {
                                                fieldValue = userInfo.Login;
                                            }
                                            else
                                            {
                                                fieldValue = this.SPList.ParentWeb.Users.GetByID(userId).LoginName;
                                            }
                                        }
                                    }
                                    if (fieldTypeAsString.Equals("UserMulti", StringComparison.OrdinalIgnoreCase) && listItemData[key] != null)
                                    {
                                        var userFieldValue = fieldValue as IAveFieldUserValueCollection;
                                        StringBuilder tempFieldValue = new StringBuilder();
                                        if (userFieldValue != null)
                                        {
                                            foreach (var v in userFieldValue)
                                            {
                                                tempFieldValue.Append(v.User.LoginName);
                                                tempFieldValue.Append(";");
                                            }
                                        }
                                        else//O365是String格式。
                                        {
                                            var usersString = fieldValue.ToString();
                                            var users = usersString.Split(new string[] { ";#" }, StringSplitOptions.None).ToList();
                                            for (int index = 0; index < users.Count; index++)
                                            {
                                                int userId;
                                                if (index % 2 == 0 && Int32.TryParse(users[index], out userId))
                                                {
                                                    AveUserInfo userInfo = this.mAveParentSite.DataCache.UserCache.GetUserInfo(userId);
                                                    if (userInfo != null)
                                                    {
                                                        tempFieldValue.Append(userInfo.Login);
                                                        tempFieldValue.Append(";");
                                                    }
                                                    else
                                                    {
                                                        var user = this.SPList.ParentWeb.Users.GetByID(userId);
                                                        if (user != null)
                                                        {
                                                            tempFieldValue.Append(this.SPList.ParentWeb.Users.GetByID(userId).LoginName);
                                                            tempFieldValue.Append(";");
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if (tempFieldValue.Length > 0)
                                        {
                                            tempFieldValue.Length--;
                                        }
                                        itemData.Add(fieldInfo.TitleAndGuiType, tempFieldValue.ToString());
                                        continue;
                                    }
                                    if (fieldTypeAsString.Equals("Lookup", StringComparison.OrdinalIgnoreCase))
                                    {
                                        var lookupListAndField = mLookupFieldListId[fieldInfo.Title];
                                        Guid lookupListId = new Guid(lookupListAndField.Key);
                                        IAveList lookupList;
                                        //365多线程要加锁
                                        lock (lockObject)
                                        {
                                            if (!mLookupListDic.TryGetValue(lookupListId, out lookupList))
                                            {
                                                lookupList = this.SPList.Lists.GetById(lookupListId);
                                                mLookupListDic.Add(lookupListId, lookupList);
                                            }
                                        }
                                        //local fieldValue只有Id,365是Id;displayVlue形式
                                        int lookupListItemId;
                                        int index = fieldValue.ToString().IndexOf(';');
                                        if (index == 0)
                                        {
                                            continue;
                                        }
                                        if (index > 0)
                                        {
                                            lookupListItemId = Convert.ToInt32(fieldValue.ToString().Substring(0, index));
                                        }
                                        else
                                        {
                                            lookupListItemId = Convert.ToInt32(fieldValue);
                                        }
                                        IAveListItem lookupListItem = lookupList.GetItemById(lookupListItemId);
                                        itemData.Add(fieldInfo.TitleAndGuiType, lookupListItem[lookupListAndField.Value].ToString());
                                        continue;
                                    }
                                    if (fieldTypeAsString.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase) && listItemData[key] != null)
                                    {
                                        //local fieldValue是IAveFieldLookupValueCollection类型，365是string类型
                                        StringBuilder tempFieldValue = new StringBuilder();
                                        if (fieldValue is IAveFieldLookupValueCollection)
                                        {
                                            var lookupFieldValue = fieldValue as IAveFieldLookupValueCollection;
                                            foreach (var v in lookupFieldValue)
                                            {
                                                tempFieldValue.Append(v.LookupValue);
                                                tempFieldValue.Append(";");
                                            }
                                        }
                                        else if (fieldValue is String)
                                        {
                                            var lookupFieldValue = fieldValue as String;
                                            //lookupFieldValue格式为“ID;#DisplayValue;#ID;#DisplayValue;”,我们要取Split后的偶数项，DisplayValue为empty的也要保留
                                            string[] splitedValues = lookupFieldValue.Split(new string[] { ";#" }, StringSplitOptions.None);
                                            for (int i = 1; i < splitedValues.Length; i = i + 2)
                                            {
                                                tempFieldValue.Append(splitedValues[i]);
                                                tempFieldValue.Append(";");
                                            }
                                        }
                                        if (tempFieldValue.Length > 0)
                                        {
                                            tempFieldValue.Length--;
                                        }
                                        itemData.Add(fieldInfo.TitleAndGuiType, tempFieldValue.ToString());
                                        continue;
                                    }
                                    itemData.Add(fieldInfo.TitleAndGuiType, fieldValue.ToString());
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

        /// <summary>
        /// ADO-16710,对于not board webpart，list url与正常的不一样，在web app后面多一个/
        /// </summary>
        /// <returns></returns>
        private string GetListUrlForNoteBoardWebPart()
        {
            string result = string.Empty;
            string webAppUrl = string.Empty;
            if (ParentWeb.SPWeb.ServerRelativeUrl.Equals("/"))
            {
                webAppUrl = ParentWeb.SPWeb.Url;
            }
            else
            {
                webAppUrl = ParentWeb.SPWeb.Url.Substring(0, ParentWeb.SPWeb.Url.IndexOf(ParentWeb.SPWeb.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase));
            }
            string listUrl = this.ServerRelativeUrl.StartsWith("/", StringComparison.Ordinal) ? "/" + this.ServerRelativeUrl : "//" + this.ServerRelativeUrl;
            result = webAppUrl + listUrl;
            return result;
        }


        private Dictionary<string, string> GetLookupItemIdAndDisplayValueBySPQuery(AveLookupFieldInfo fieldInfo, bool needSkipIfNotInLibrary)
        {
            Dictionary<string, string> itemIdAndValues = null;
            IAveWeb web = null;
            try
            {
                if (this.ParentWeb.SPWeb.ID == fieldInfo.LookupWeb)
                {
                    web = this.ParentWeb.SPWeb;
                }
                else
                {
                    web = this.ParentSite.SPSite.OpenWeb(fieldInfo.LookupWeb);
                }
                var lookupList = web.Lists.GetListById(fieldInfo.LookupList, true);
                //只会备份Document Library下Item LeafName
                if (lookupList.BaseType != AveBaseType.DocumentLibrary && needSkipIfNotInLibrary)
                {
                    log.Debug("BackupItemLeafNameOfLookupValue option is only used for DocumentLibrary, current lookupList type:{0}, lookupList ID:{1}", lookupList.BaseType, lookupList.ID);
                    return null;
                }
                var query = this.ParentSite.ObjectModelFactory.CreateQuery();
                query.ViewAttributes = "Scope='RecursiveAll'";
                IAveListItemCollection items;
                itemIdAndValues = new Dictionary<string, string>();
                do
                {
                    items = lookupList.GetItems(query);
                    foreach (var item in items)
                    {
                        var fieldValue = item[fieldInfo.LookupField];
                        if (fieldValue != null)
                        {
                            itemIdAndValues[item.ID.ToString()] = fieldValue.ToString();
                        }
                    }
                    query.ListItemCollectionPosition = items.ListItemCollectionPosition;
                } while (items.ListItemCollectionPosition != null);
                return itemIdAndValues;
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while getting lookup item id and display value. FieldInfo:{0}. Error: {1}", fieldInfo, e);
            }
            finally
            {
                if (web != null && this.ParentWeb.SPWeb.ID != fieldInfo.LookupWeb)
                {
                    web.Dispose();
                    web = null;
                }
            }
            return itemIdAndValues;
        }

        private Dictionary<string, string> GetLookupItemIdAndDisplayValueByAPI(AveLookupFieldInfo fieldInfo, bool needSkipIfNotInLibrary)
        {
            Dictionary<string, string> itemIdAndValues = null;
            IAveWeb web = null;
            try
            {
                if (this.ParentWeb.SPWeb.ID == fieldInfo.LookupWeb)
                {
                    web = this.ParentWeb.SPWeb;
                }
                else
                {
                    web = this.ParentSite.SPSite.OpenWeb(fieldInfo.LookupWeb);
                }
                var list = web.GetList(fieldInfo.LookupList);
                itemIdAndValues = new Dictionary<string, string>();
                if (list.BaseType != AveBaseType.DocumentLibrary && needSkipIfNotInLibrary)
                {
                    log.Debug("BackupItemLeafNameOfLookupValue option is only used for DocumentLibrary, current lookupList type:{0}, lookupList ID:{1}", list.BaseType, list.ID);
                    return null;
                }
                foreach (var item in list.Items)
                {
                    var fieldValue = item[fieldInfo.LookupField];
                    if (fieldValue != null)
                    {
                        itemIdAndValues[item.ID.ToString()] = fieldValue.ToString();
                    }
                }
                foreach (var folder in list.Folders)
                {
                    var fieldValue = folder[fieldInfo.LookupField];
                    if (fieldValue != null)
                    {
                        itemIdAndValues[folder.ID.ToString()] = fieldValue.ToString();
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occurred while getting lookup item id and display value. FieldInfo:{0}. Error: {1}", fieldInfo, e);
            }
            finally
            {
                if (web != null && this.ParentWeb.SPWeb.ID != fieldInfo.LookupWeb)
                {
                    web.Dispose();
                    web = null;
                }
            }
            return itemIdAndValues;
        }

        /// <summary>
        /// 只会备份Document Library下Item LeafName
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        internal string GetLookupItemLeafNameByItemId(string itemId, AveLookupFieldInfo fieldInfo)
        {
            string orginalField = fieldInfo.LookupField;
            try
            {
                fieldInfo.LookupField = "FileLeafRef";
                string leafName = GetLookupDisplayValuebyItemId(itemId, fieldInfo, true);
                return leafName;
            }
            finally
            {
                fieldInfo.LookupField = orginalField;
            }
        }
        /// <summary>
        /// 获取Lookup field的Display value.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="fieldInfo"></param>
        /// <param name="needSkipIfNotInLibrary">当被lookup list是非Library时,是否获取Display value. 目前只是备份Leafname时赋值为true</param>
        /// <returns></returns>
        public string GetLookupDisplayValuebyItemId(string itemId, AveLookupFieldInfo fieldInfo, bool needSkipIfNotInLibrary = false)
        {
            try
            {
                if (!lookUpListItemIDAndValues.ContainsKey(fieldInfo.LookupList.ToString()) || !lookUpListItemIDAndValues[fieldInfo.LookupList.ToString()].ContainsKey(fieldInfo.LookupField))
                {
                    Dictionary<string, string> itemIdAndValues = null;
                    //Client SPQuery未实现，需要通过API方式获取
                    //ADO-147382 computed类型column比较特殊LookupColumnRowNameForQuery为null，无法通过QueryService来获取对应的value
                    if (this.ParentSite.ObjectModelFactory.ContextKind != AveContextKind.ClientObjectModel &&
                        !String.IsNullOrEmpty(fieldInfo.LookupColumnRowNameForQuery))
                    {
                        itemIdAndValues = GetLookupItemIdAndDisplayValueBySPQuery(fieldInfo, needSkipIfNotInLibrary);
                    }
                    else
                    {
                        itemIdAndValues = GetLookupItemIdAndDisplayValueByAPI(fieldInfo, needSkipIfNotInLibrary);
                    }
                    if (itemIdAndValues == null)
                    {
                        return String.Empty;
                    }
                    if (!lookUpListItemIDAndValues.ContainsKey(fieldInfo.LookupList.ToString()))
                    {
                        Dictionary<string, Dictionary<string, string>> tempDic = new Dictionary<string, Dictionary<string, string>>();
                        tempDic.Add(fieldInfo.LookupField, itemIdAndValues);
                        lookUpListItemIDAndValues[fieldInfo.LookupList.ToString()] = tempDic;
                    }
                    else
                    {
                        lookUpListItemIDAndValues[fieldInfo.LookupList.ToString()][fieldInfo.LookupField] = itemIdAndValues;
                    }
                }
                return lookUpListItemIDAndValues[fieldInfo.LookupList.ToString()][fieldInfo.LookupField][itemId];
            }
            catch(Exception ex)
            {
                log.Warn("Failed to get lookup display value by item id, id: {0}, exception: {1}", itemId, ex);
                return string.Empty;
            }
        }

        public void Dispose()
        {
            foreach (var exportExcelData in ExportExcelDatas)
            {
                ExportItemDataToExcel(exportExcelData.Value.UserData, exportExcelData.Value.Path);
            }
            if (mFMExcel != null)
            {
                mFMExcel.Dispose();
            }
            if (this.SPList != null)
            {
                this.SPList.CleanListData();
                this.mSPList = null;
            }
        }

        #region IAveSPList Members

        IAveSPSite IAveSPList.AveSPSite
        {
            get { return mAveParentSite; }
        }

        IAveSPWeb IAveSPList.ParentWeb
        {
            get { return mAveSPWeb; }
        }

        public IAveList SPList
        {
            get { return mSPList; }
        }

        public bool IsSystemList
        {
            get { return mIsSystemList; }
        }

        public bool IsWorkflowHistoryList
        {
            get
            {
                bool isWorkflowHistory = false;
                if (mSPList != null && mSPList.BaseTemplate == AveListTemplateType.WorkflowHistory)
                {
                    isWorkflowHistory = true;
                }
                return isWorkflowHistory;
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
                //默认为empty，外围需要赋值
                mExcelPath = value;
                //Export应用metadata ,list非隐藏时导出excel
                if (mNeedCreateExcel && !mSPList.Hidden)
                {
                    CreateExcelOrSheetIfNeed();
                }
            }
        }

        #region Properties
        public string Title
        {
            get { return mTitle; }
        }

        public string Path
        {
            get { return mPath; }
        }

        public string ServerRelativeUrl
        {
            get { return mIsSystemList ? mAveSPWeb.SPWeb.ServerRelativeUrl : mSPList.RootFolder.ServerRelativeUrl; }
        }

        public Guid Id
        {
            get { return mId; }
        }

        public Guid ScopeId
        {
            get { return mScopeId; }
        }
        #endregion

        public void ExportBaseInfo(IAveBackupStream output)
        {
            var listInfo = new AveSPListInfo(this);
            listInfo.Export(output);
        }

        /// <summary>在setListInfo里面修改ListInfo的数据</summary>
        public void ExportBaseInfo(IAveBackupStream stream, SetListInfoAction setListInfo)
        {
            var listInfo = new AveSPListInfo(this);
            var result = listInfo.GetListInfo();
            if (setListInfo != null)
            {
                setListInfo(result);
            }
            stream.WriteMetadata(AveMetadataType.ListBasicInfo, result);
        }

        /// <summary>PR Item is virtual site</summary>
        public void ExportBaseInfo(IAveBackupStream output, string url)
        {
            var listInfo = new AveSPListInfo(this);
            var result = listInfo.GetListInfo();
            result.Url = url;
            output.WriteMetadata(AveMetadataType.ListBasicInfo, result);
        }
        /// <param name="includeAuthor">是否先备份List的Author，避免还原的时候不存在导致找不到User</param>
        public void ExportSettings(IAveBackupStream output, bool includeAuthor = true)
        {
            if (includeAuthor)
            {
                this.CacheUserFromAuthor();
                this.ExportUserCache(output);
            }
            var listSettingInfo = new AveSPListSettingInfo(this);
            listSettingInfo.Export(output);
        }

        /// <param name="includeGroup">是否先备份User Field的Selection Group，避免还原的时候不存在导致找不到Group</param>
        public void ExportFields(IAveBackupStream output, bool includeGroup = true, AveBackupOption backupColumnOption = null)
        {
            if (includeGroup)
            {
                this.CacheGroupsFromUserField();
                this.ExportGroupCache(output);
            }
            AveSPFieldCollection fields = AveSPFieldCollection.CreateInstance(this);
            if (backupColumnOption == null)
            {
                backupColumnOption = new AveBackupOption();
            }
            fields.Export(output, backupColumnOption);
        }

        public void ExportFields(IAveBackupStream stream, SPListFieldBackupOption backupColumnOption)
        {
            if (backupColumnOption.IncludeGroups)
            {
                this.CacheGroupsFromUserField();
                this.ExportGroupCache(stream);
            }
            AveSPFieldCollection fields = AveSPFieldCollection.CreateInstance(this);
            var backupOption = new AveBackupOption()
            {
                BackupRelatedTermSets = backupColumnOption.BackupRelatedTermSets,
                BackupRelatedTermsOnly = backupColumnOption.BackupRelatedTermsOnly,
                BeforeExportFieldsAction = backupColumnOption.BeforeExportFieldsAction
            };
            fields.Export(stream, backupOption);
        }
        private void ExportContentTypeNintexFormXml(List<AveContentTypeInfo> contentTypes)
        {
            using (AveNintexForm nf = AveNintexForm.CreateNintexForm(this))
            {
                foreach (var ct in contentTypes)
                {
                    try
                    {
                        ct.NintexFormXmls = nf.ExportNintexForm(ct.Id, ct.XmlDocuments);
                        if (ct.NintexFormXmls != null && ct.NintexFormXmls.Count > 0)
                        {
                            log.Info("Success to backup nintex form in content type:{0} of list:{1}", ct.Id, this.Title);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error(WrapperBackupResource.BackupNintexFormFailed, ct.Id, this.Id, e);
                    }
                }
            }
        }

        private bool NeedBackupNintexForm(AveBackupOption backupContentTypeOption)
        {
            var tempContextKind = this.ParentSite.ObjectModelFactory.ContextKind;

            if (tempContextKind >= AveContextKind.ServerObjectModel ||
                this.ParentSite.SPSite.IsOnlineSite)
            {
                if (backupContentTypeOption == null)
                {
                    return true;
                }
                return backupContentTypeOption.BackupNintexForm;
            }
            return false;
        }

        public void ExportContentTypes(IAveBackupStream output, AveBackupOption backupContentTypeOption = null)
        {
            var contentTypes = AveSPContentTypeCollection.CreateInstance(this);
            var result = contentTypes.GetContentTypeCollectionInfoObj();
            if (backupContentTypeOption != null && backupContentTypeOption.BeforeExportContentTypesAction != null)
            {
                backupContentTypeOption.BeforeExportContentTypesAction(result);
            }

            if (NeedBackupNintexForm(backupContentTypeOption))
            {
                ExportContentTypeNintexFormXml(result.ContentTypes);
            }
            output.WriteMetadata(AveMetadataType.ListContentType.ToString(), result);
        }

        public void ExportContentTypes(IAveBackupStream stream, SPContentTypeBackupOption backupContentTypeOption)
        {
            var backupOption = new AveBackupOption()
            {
                BeforeExportContentTypesAction = backupContentTypeOption.BeforeExportConentTypesAction
            };
            this.ExportContentTypes(stream, backupOption);
        }

        public void ExportEventReceivers(IAveBackupStream output)
        {
            var events = AveSPEventReceiver.CreateInstance(this);
            events.Export(output); ;
        }

        public void ExportSocialTags(IAveBackupStream output)
        {
            if (this.ParentSite.SPContextKind.IsServerMode10Upper())
            {
                if (AveEnv.IsMoss)
                {
                    string listUrl = GetListUrlForNoteBoardWebPart();
                    var tag = new AveSPSocialTag(listUrl + "/", this.ParentSite);
                    tag.Export(output);
                }
            }
        }

        public void ExportSocialComments(IAveBackupStream output)
        {
            if (this.ParentSite.SPContextKind.IsServerMode10Upper())
            {
                if (AveEnv.IsMoss)
                {
                    string listUrl = GetListUrlForNoteBoardWebPart();
                    var comment = new AveSPSocialComment(listUrl + "/", this.ParentSite);
                    comment.Export(output);
                }
            }
        }


        /// <param name="includeUser">是否先备份Alert的User，避免还原的时候不存在导致找不到User</param>
        public void ExportAlerts(IAveBackupStream output, bool includeUser = true)
        {
            if (includeUser)
            {
                this.CacheUserFromAlert();
                this.ExportUserCache(output);
            }
            AveSPAlert mListAlert = AveSPAlert.CreateInstance(this);
            mListAlert.Export(output);
        }

        /// <summary>只有Raplicator需要，别的模块User不需要单独控制，会跟Permission走</summary>
        public void ExportUsers(IAveBackupStream output)
        {
            this.CachePrincipalFromPermission(3);
            this.ExportUserCache(output);
        }

        /// <summary>只有Raplicator需要，别的模块Group不需要单独控制，会跟Permission走</summary>
        public void ExportGroups(IAveBackupStream output)
        {
            this.CachePrincipalFromPermission(3);
            this.ExportGroupCache(output);
        }

        /// <param name="includeUserAndGroup">是否先备份相关的User和Group，避免还原的时候不存在</param>
        public void ExportRoleAssignments(IAveBackupStream output, bool includeUserAndGroup = true)
        {
            if (includeUserAndGroup)
            {
                this.CachePrincipalFromPermission(3);
                this.ExportUserCache(output);
                this.ExportGroupCache(output);
            }
            var roleAssignments = AveRoleAssignments.CreateInstance(this);
            roleAssignments.Export(output);
        }

        public void ExportWorkflows(IAveBackupStream stream, SPListWorkflowAssociationBackupOption option)
        {
            AveWorkflow workflow = new AveWorkflow() { ForceBackupAssoiciation = true, BackupWorkflowAssocationToExportedFile = option.BackupWorkflowAssocationToExportedFile };
            if (!string.IsNullOrEmpty(option.NWContentDBConnectionString))
            {
                workflow.SetNWDBConnectionString(option.NWContentDBConnectionString);
            }

            if (!string.IsNullOrEmpty(option.NWConfigDBConnectionString))
            {
                workflow.SetNWConfigDBConnectionString(option.NWConfigDBConnectionString);
            }

            if (option.ExportListAssociation)
            {
                workflow.ExportListWFAssociation(stream, this, option.FilterFunc);
            }
            if (option.ExportContentTypeAssociation)
            {
                workflow.ExportListContentTypeWFAssociation(stream, this, option.FilterFunc);
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


        public List<AveEventReceiverInfo> GetEventReceivers()
        {
            var events = AveSPEventReceiver.CreateInstance(this);
            return events.GetReceivers();
        }

        public List<AveUserInfo> GetUsers()
        {
            this.CachePrincipalFromPermission(3);
            return mDataCache.GetUsersForExport().Users;
        }

        public List<AveGroupInfo> GetGroups()
        {
            this.CachePrincipalFromPermission(3);
            return mDataCache.GetGroupsForExport().Groups;
        }
        #endregion

        public void ExportRoleAssignments(IAveBackupStream stream)
        {
            ExportRoleAssignments(stream, true);
        }

        public void ExportRoleAssignments(IAveBackupStream stream, SPRoleAssignmentsBakupOption backupOption)
        {
            //todo:oliver 重复代码
            SPRoleAssignmentsDto roleAssignmentsDto = new SPRoleAssignmentsDto();

            if (backupOption.IncludeInheritedRoleAssignments || HasUniqueRoleAssignments)
            {
                using (var roleAssignments = AveRoleAssignments.CreateInstance(this))
                {
                    roleAssignmentsDto = roleAssignments.GetRoleAssignmentsDto(backupOption.IncludeUsers, backupOption.IncludeGroups);
                }
            }
            roleAssignmentsDto.IsInherit = !HasUniqueRoleAssignments;

            stream.WriteMetadata(AveMetadataType.RoleAssignmentsDto, roleAssignmentsDto);
        }

        public void ExportAlerts(IAveBackupStream stream)
        {
            var alert = AveSPAlert.CreateInstance(this);
            var alertsDto = alert.GetAlertsDto();

            if (alertsDto != null)
            {
                stream.WriteMetadata(AveMetadataType.AlertsDto, alertsDto);
            }
        }

        public void ExportSocialInfos(IAveBackupStream stream)
        {
            if (this.ParentSite.ObjectModelFactory.ContextKind.IsServerMode10Upper())
            {
                var socialDto = new SPSocialDto();

                string listUrl = GetListUrlForNoteBoardWebPart();
                socialDto.Comments = new AveSPSocialComment(listUrl + "/", this.ParentSite).GetSocialComments();
                socialDto.Tags = new AveSPSocialTag(listUrl + "/", this.ParentSite).GetSocialTags();

                if ((socialDto.Comments != null && socialDto.Comments.Count > 0) ||
                    (socialDto.Tags != null && socialDto.Tags.Count > 0))
                {
                    stream.WriteMetadata(AveMetadataType.SocialDto, socialDto);
                }
            }
        }

        public void ExportPolicy(IAveBackupStream output)
        {
            var policy = new AveSPPolicy(this.ParentSite, this.ParentWeb, this);
            policy.Export(output);
        }

        //add for PRItemRestore to backup social info.
        public Dictionary<int, object> ConvertItemstoThreadsInfo(List<IAveSPItem> items)
        {
            Dictionary<int, object> result = new Dictionary<int, object>();
            List<AveSocialFeedInfo> feedInfos = ConvertItemToPostInfo(items);
            foreach (AveSocialFeedInfo info in feedInfos)
            {
                int id = 0;
                if (int.TryParse(info.Id, out id))
                {
                    result.Add(id, info);
                }
            }
            return result;
        }

        //add for PRItemRestore to backup social info.
        private List<AveSocialFeedInfo> ConvertItemToPostInfo(List<IAveSPItem> items)
        {
            List<AveSocialFeedInfo> feedInfos = new List<AveSocialFeedInfo>();
            List<AveSocialFeedPostInfoForPR> infoList = new List<AveSocialFeedPostInfoForPR>();
            foreach (var item in items)
            {
                AveSocialFeedPostInfoForPR infoForPR = new AveSocialFeedPostInfoForPR();
                infoForPR.Info = new AveSocialFeedPostInfo();
                infoForPR.Info.Attachment = new AveSocialAttachmentInfo();

                infoForPR.Info.Attributes = (AveOSocialPostAttributes)Enum.Parse(typeof(AveOSocialPostAttributes), item.SPListItem["Attributes"].ToString());
                //info.AuthorIndex = item.Author.ID;
                infoForPR.Info.CreatedTime = DateTime.Parse(item.SPListItem["Created"].ToString());
                infoForPR.Info.Id = item.RowId.ToString();
                foreach (string name in Regex.Split(item.SPListItem["LikedBy"].ToString(), ";#"))
                {
                    int temp = 0;
                    if (int.TryParse(name, out temp))
                    {
                        infoForPR.Info.Likers.Add(this.mAveSPWeb.SPWeb.SiteUsers.GetByID(temp).NoPrefixLoginName);
                    }
                }
                infoForPR.Info.ModifiedTime = DateTime.Parse(item.SPListItem["Modified"].ToString());
                //info.Overlays
                infoForPR.Info.PostType = (AveOSocialPostType)Enum.Parse(typeof(AveOSocialPostType), item.SPListItem["ContentType"].ToString());
                infoForPR.Info.PreferredImageUri = new Uri(item.SPListItem["MediaLinkURI"].ToString());
                infoForPR.Info.Source = new AveSocialLink();
                infoForPR.Info.Source.Text = item.SPListItem["PostSource"].ToString();
                infoForPR.Info.Source.Uri = new Uri(item.SPListItem["Content"].ToString());
                infoForPR.Info.Text = item.SPListItem["Content"].ToString();
                infoForPR.Info.Attachment.AttachmentKind = AveOSocialAttachmentKind.Image;
                infoForPR.Info.Attachment.Content = this.ParentWeb.SPWeb.GetFile(item.SPListItem["MediaLinkURI"].ToString()).OpenBinary(AveOpenBinaryOptions.SkipVirusScan);

                infoForPR.ReplyCount = int.Parse(item.SPListItem["ReplyCount"].ToString());
                infoList.Add(infoForPR);
            }


            IEnumerable<IGrouping<string, AveSocialFeedPostInfoForPR>> postGroup = infoList.GroupBy<AveSocialFeedPostInfoForPR, string>(n => n.Info.Id);
            foreach (var group in postGroup)
            {
                foreach (AveSocialFeedPostInfoForPR post in group)
                {
                    AveSocialFeedInfo feedinfo = new AveSocialFeedInfo();

                    if (post.Info.PostType == AveOSocialPostType.Root)
                    {
                        feedinfo.Id = post.Info.Id;
                        feedinfo.RootPost = post.Info;
                        //feedinfo.TotalReplyCount = post.
                    }
                    else
                    {
                        feedinfo.Replies.Add(post.Info);
                    }
                    feedInfos.Add(feedinfo);
                }
            }
            return feedInfos;
        }

        public void ExportUserCustomActions(IAveBackupStream output)
        {
            AveSPUserCustomActionCollection spUserCustomActionCollection = new AveSPListUserCustomActionCollection(this);
            output.WriteMetadata(AveMetadataType.ListUserCustomAction, spUserCustomActionCollection.GetUserCustomActionInfos());
        }
    }

    class ExportExcelData
    {
        public string Path { get; set; }
        public Dictionary<string, object> UserData { get; set; }
        public int Version { get; set; }
    }
}
