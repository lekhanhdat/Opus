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
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Meetings;
using Microsoft.SharePoint.Upgrade;
using Microsoft.SharePoint.Utilities;
using SPDisposeCheck;
using Microsoft.SharePoint.Workflow;
using AvePoint.Wrapper.Restore;
using System.IO;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveList : AveSecurableObject, IAveList, IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveList));

        private const string RowOrdinal = "RowOrdinal";
        private const string ColName = "ColName";
        private static readonly Dictionary<string, string> SKIP_FIELD_MAP = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> NEED_FIELD_MAP = new Dictionary<string, string>();

        private SPList mList;
        private AveWeb m_ParentWeb;
        private AveSite mSite;
        private AveFolder mRootFolder;
        private AveEventReceiverDefinitionCollection mEventReceivers;
        internal AveViewCollection mViews;     //状态不一致
        private AveListDataSource mDataSource;
        private AveView mDefaultView;
        private AveUser mAuthor;
        private AveAudit mAudit;
        private AveAlertTemplate mAlertTemplate;
        private AveAlertTemplate mSmsAlertTemplate;
        private AveWorkflowAssociationCollection mWorkflowAssociations;
        private AveListCollection mLists;
        private AveFieldIndexCollection mFieldIndexes;
        private AveFormCollection mForms;
        private AveListItemCollection mFolders;
        private AveContentTypeCollection mContentTypes;
        private AveFieldCollection mFields;
        private Dictionary<string, AveColumn> mFieldMap;
        private Dictionary<Guid, AveColumn> mIdFieldMap;
        private bool mIsFieldMapLoaded;
        private Guid mPreviousUID;
        //默认通过update或checkout增加item version的情况下，增加version的field value会是前一个version的，NeedSetNullFields存储需要设置为null的field。
        public List<string> NeedSetNullFields;
        private AveSOIntegrationUtility mSOIntegrationUtil;
        internal AveSOIntegrationUtility SOIntegrationUtil
        {
            get
            {
                if (mSOIntegrationUtil == null)
                {
                    mSOIntegrationUtil = new AveSOIntegrationUtility(mSite, this);
                    // AveSOIntegrationUtility构造函数中会反射执行connector中的ConnectorItemRestore逻辑，该逻辑中会new一个新的web，如果是in place还原，会删除document所在list中的部分event receiver。由于删除之后会做update操作，导致wrapper逻辑中的list失效，之后再update wrapper逻辑中的list会产生save conflict错误。
                    if (this.mList != null && this.IsConnectorList != null && this.isConnectorList.Value)
                    {
                        this.m_ParentWeb.ReloadWeb();
                        this.Reload();
                    }
                }
                return mSOIntegrationUtil;
            }
        }

        static AveList()
        {
            SKIP_FIELD_MAP["tp_ID"] = "#tp_ID";
            SKIP_FIELD_MAP["tp_ListId"] = "#tp_ListId";
            SKIP_FIELD_MAP["tp_SiteId"] = "#tp_SiteId";
            SKIP_FIELD_MAP["tp_RowOrdinal"] = "#tp_RowOrdinal";
            SKIP_FIELD_MAP["tp_Version"] = "#tp_Version";
            SKIP_FIELD_MAP["tp_Ordering"] = "#tp_Ordering";
            SKIP_FIELD_MAP["tp_ThreadIndex"] = "#tp_ThreadIndex";
            SKIP_FIELD_MAP["tp_HasAttachment"] = "#tp_HasAttachment";
            SKIP_FIELD_MAP["tp_ModerationStatus"] = "#tp_ModerationStatus";
            SKIP_FIELD_MAP["tp_IsCurrent"] = "#tp_IsCurrent";
            SKIP_FIELD_MAP["tp_ItemOrder"] = "#tp_ItemOrder";
            SKIP_FIELD_MAP["tp_InstanceID"] = "#tp_InstanceID";
            SKIP_FIELD_MAP["tp_GUID"] = "#tp_GUID";
            SKIP_FIELD_MAP["tp_CopySource"] = "#tp_CopySource";
            SKIP_FIELD_MAP["tp_HasCopyDestinations"] = "#tp_HasCopyDestinations";
            SKIP_FIELD_MAP["tp_AuditFlags"] = "#tp_AuditFlags";
            SKIP_FIELD_MAP["tp_InheritAuditFlags"] = "#tp_InheritAuditFlags";
            SKIP_FIELD_MAP["tp_Size"] = "#tp_Size";
            SKIP_FIELD_MAP["tp_WorkflowVersion"] = "#tp_WorkflowVersion";
            SKIP_FIELD_MAP["tp_WorkflowInstanceID"] = "#tp_WorkflowInstanceID";
            SKIP_FIELD_MAP["tp_ParentId"] = "#tp_ParentId";
            SKIP_FIELD_MAP["tp_DocId"] = "#tp_DocId";
            SKIP_FIELD_MAP["tp_DeleteTransactionId"] = "#tp_DeleteTransactionId";
            SKIP_FIELD_MAP["tp_ContentTypeId"] = "#tp_ContentTypeId";
            //SKIP_FIELD_MAP["uniqueidentifier1"] = "#uniqueidentifier1";
            SKIP_FIELD_MAP["tp_Level"] = "#tp_Level";
            SKIP_FIELD_MAP["tp_IsCurrentVersion"] = "#tp_IsCurrentVersion";
            SKIP_FIELD_MAP["tp_UIVersion"] = "#tp_UIVersion";
            SKIP_FIELD_MAP["tp_CalculatedVersion"] = "#tp_CalculatedVersion";
            SKIP_FIELD_MAP["tp_UIVersionString"] = "#tp_UIVersionString";
            SKIP_FIELD_MAP["tp_DraftOwnerId"] = "#tp_DraftOwnerId";

            NEED_FIELD_MAP["tp_Editor"] = "Editor";
            NEED_FIELD_MAP["tp_Author"] = "Author";
            NEED_FIELD_MAP["tp_Modified"] = "Modified";
            NEED_FIELD_MAP["tp_Created"] = "Created";
        }

        public AveList(AveListCollection lists, SPList list)
            : base(list)
        {
            mLists = lists;
            mList = list;
            m_ParentWeb = lists.Web as AveWeb;
            mSite = lists.Web.Site as AveSite;
        }

        internal SPList List
        {
            get
            {
                return mList;
            }
        }

        #region IAveList Members

        public bool AllowMultiResponses
        {
            get { return mList.AllowMultiResponses; }
            set { mList.AllowMultiResponses = value; }
        }

        public int Version
        {
            get { return mList.Version; }
        }

        public string ColNameCollection
        {
            get;
            set;
        }

        public bool AllowContentTypes
        {
            get { return mList.AllowContentTypes; }
        }

        public AveListTemplateType BaseTemplate
        {
            get { return (AveListTemplateType)mList.BaseTemplate; }
        }

        public AveBaseType BaseType
        {
            get { return (AveBaseType)mList.BaseType; }
        }

        public DateTime Created
        {
            get { return mList.Created; }
        }

        public IAveContentTypeCollection ContentTypes
        {
            get
            {
                if (mContentTypes == null)
                {
                    try
                    {
                        mContentTypes = new AveContentTypeCollection(this, mList.ContentTypes);
                    }
                    catch (Exception e)
                    {
                        logger.Debug("Failed to get the list content types, reload and try again. Exception: {0}", e);
                        this.Reload();
                        mContentTypes = new AveContentTypeCollection(this, mList.ContentTypes);
                    }
                }
                return mContentTypes;
            }
        }

        public bool ContentTypesEnabled
        {
            get
            {
                return mList.ContentTypesEnabled;
            }
            set
            {
                mList.ContentTypesEnabled = value;
            }
        }

        public Guid DefaultContentApprovalWorkflowId
        {
            get
            {
                return mList.DefaultContentApprovalWorkflowId;
            }
            set
            {
                mList.DefaultContentApprovalWorkflowId = value;
            }
        }

        public string DefaultDisplayFormUrl
        {
            get
            {
                return mList.DefaultDisplayFormUrl;
            }
            set
            {
                mList.DefaultDisplayFormUrl = value;
            }
        }

        public string DefaultEditFormUrl
        {
            get
            {
                return mList.DefaultEditFormUrl;
            }
            set
            {
                mList.DefaultEditFormUrl = value;
            }
        }

        public string DefaultNewFormUrl
        {
            get
            {
                return mList.DefaultNewFormUrl;
            }
            set
            {
                mList.DefaultNewFormUrl = value;
            }
        }

        public string DefaultViewUrl
        {
            get { return mList.DefaultViewUrl; }
        }

        public IAveView DefaultView
        {
            get
            {
                if (mDefaultView == null)
                {
                    SPView view = mList.DefaultView;
                    if (view != null)
                    {
                        mDefaultView = new AveView(this, view);
                    }
                }
                return mDefaultView;
            }
        }

        public string Description
        {
            get
            {
                return mList.Description;
            }
            set
            {
                mList.Description = value;
            }
        }

        public AveDraftVisibilityType DraftVersionVisibility
        {
            get
            {
                return (AveDraftVisibilityType)mList.DraftVersionVisibility;
            }
            set
            {
                mList.DraftVersionVisibility = (DraftVisibilityType)value;
            }
        }

        public string Direction
        {
            get
            {
                return mList.Direction;
            }
            set
            {
                mList.Direction = value;
            }
        }

        public bool EnableAttachments
        {
            get
            {
                return mList.EnableAttachments;
            }
            set
            {
                mList.EnableAttachments = value;
            }
        }

        public bool EnableFolderCreation
        {
            get
            {
                return mList.EnableFolderCreation;
            }
            set
            {
                mList.EnableFolderCreation = value;
            }
        }

        public bool EnableMinorVersions
        {
            get
            {
                return mList.EnableMinorVersions;
            }
            set
            {
                mList.EnableMinorVersions = value;
            }
        }

        public bool EnableModeration
        {
            get
            {
                return mList.EnableModeration;
            }
            set
            {
                mList.EnableModeration = value;
            }
        }

        public bool EnableVersioning
        {
            get
            {
                return mList.EnableVersioning;
            }
            set
            {
                mList.EnableVersioning = value;
            }
        }

        public string EventSinkClass
        {
            get
            {
                return mList.EventSinkClass;
            }
            set
            {
                mList.EventSinkClass = value;
            }
        }

        public string EventSinkData
        {
            get
            {
                return mList.EventSinkData;
            }
            set
            {
                mList.EventSinkData = value;
            }
        }

        public IAveFieldCollection Fields
        {
            get
            {
                if (mFields == null)
                {
                    mFields = new AveFieldCollection(this.ParentWeb as AveWeb, mList.Fields);
                }
                else
                {
                    var spFileds = mList.Fields;
                    if (!object.ReferenceEquals(mFields.FieldCollection, spFileds))
                    {
                        mFields = new AveFieldCollection(this.ParentWeb as AveWeb, spFileds);
                    }
                }
                return mFields;
            }
        }

        public IAveFieldIndexCollection FieldIndexes
        {
            get
            {
                if (mFieldIndexes == null)
                {
                    mFieldIndexes = new AveFieldIndexCollection(mList.FieldIndexes);
                }
                return mFieldIndexes;
            }
        }

        public bool ForceCheckout
        {
            get
            {
                return mList.ForceCheckout;
            }
            set
            {
                mList.ForceCheckout = value;
            }
        }

        public bool HasExternalDataSource
        {
            get { return mList.HasExternalDataSource; }
        }

        public bool Hidden
        {
            get
            {
                return mList.Hidden;
            }
            set
            {
                mList.Hidden = value;
            }
        }

        public Guid Id
        {
            get { return mList.ID; }
        }

        public string ImageUrl
        {
            get
            {
                return mList.ImageUrl;
            }
            set
            {
                mList.ImageUrl = value;
            }
        }

        public bool IsApplicationList
        {
            get
            {
                return mList.IsApplicationList;
            }
            set
            {
                mList.IsApplicationList = value;
            }
        }

        public bool IsCatalog
        {
            get { return false; }
        }

        public bool IsSiteAssetsLibrary
        {
            get { return mList.IsSiteAssetsLibrary; }
            set { mList.IsSiteAssetsLibrary = value; }
        }

        //Items在file或者folder添加后，也跟着添加一个item，否则状态不一致
        public IAveListItemCollection Items
        {
            get
            {
                SPQuery query = new SPQuery();
                query.ViewAttributes = "Scope=\"Recursive\"";
                return this.GetItems(new AveQuery(this, query));
            }
        }

        public int ItemCount
        {
            get { return mList.ItemCount; }
        }

        public DateTime LastItemDeletedDate
        {
            get { return mList.LastItemDeletedDate; }
        }

        public DateTime LastItemModifiedDate
        {
            get { return mList.LastItemModifiedDate; }
        }

        public bool MultipleDataList
        {
            get
            {
                return mList.MultipleDataList;
            }
            set
            {
                mList.MultipleDataList = value;
            }
        }

        public bool NoCrawl
        {
            get
            {
                return mList.NoCrawl;
            }
            set
            {
                mList.NoCrawl = value;
            }
        }

        public bool OnQuickLaunch
        {
            get
            {
                return mList.OnQuickLaunch;
            }
            set
            {
                mList.OnQuickLaunch = value;
            }
        }

        public IAveWeb ParentWeb
        {
            get
            {
                if (AveSPUtility.StsCompareStrings(this.mLists.Web.ServerRelativeUrl, this.ParentWebUrl))
                {
                    return mLists.Web;
                }
                if (m_ParentWeb == null)
                {
                    m_ParentWeb = mLists.Web.Site.OpenWeb(this.ParentWebUrl) as AveWeb;
                }
                return m_ParentWeb;

            }
        }

        public string ParentWebUrl
        {
            get { return mList.ParentWebUrl; }
        }

        public IAveFolder RootFolder
        {
            get
            {
                if (mRootFolder == null)
                {
                    mRootFolder = new AveFolder(this.ParentWeb as AveWeb, mList.RootFolder);
                }
                return mRootFolder;
            }
        }

        public string SchemaXml
        {
            get { return mList.SchemaXml; }
        }

        public bool ServerTemplateCanCreateFolders
        {
            get { return mList.ServerTemplateCanCreateFolders; }
        }

        public Guid TemplateFeatureId
        {
            get { return mList.TemplateFeatureId; }
        }

        public string Title
        {
            get
            {
                return mList.Title;
            }
            set
            {
                mList.Title = value;
            }
        }

        public string ValidationFormula
        {
            get
            {
                return mList.ValidationFormula;
            }
            set
            {
                mList.ValidationFormula = value;
            }
        }

        public string ValidationMessage
        {
            get
            {
                return mList.ValidationMessage;
            }
            set
            {
                mList.ValidationMessage = value;
            }
        }

        public bool? IsConnectorList
        {
            get
            {
                if (isConnectorList == null)
                {
                    isConnectorList = (bool)AveAssemblyUtility.InvokeMethod(WrapperRuntime.CurrentContext.ModelFactory.CreateConnectorInegration(), "IsConnectorLibrary", this.ID, this.RootFolder.Properties);
                }
                return isConnectorList;
            }
            set { isConnectorList = value; }
        }

        public bool IsOneDriveLibrary
        {
            get
            {
                return false;
            }
        }

        public IAveListItem AddItem(AveItemCreationInformation itemCreationInfo)
        {
            string rootFolderUrl = AveAssemblyUtility.GetPropertyValue(this.mList, "RootFolderUrl") as string;
            string leafName = null;
            SPFileSystemObjectType file = SPFileSystemObjectType.File;
            if (itemCreationInfo != null)
            {
                if (itemCreationInfo.FolderUrl != null)
                {
                    rootFolderUrl = itemCreationInfo.FolderUrl;
                }
                file = (SPFileSystemObjectType)itemCreationInfo.UnderlyingObjectType;
                leafName = itemCreationInfo.LeafName;
            }

            return new AveListItem(this.Items as AveListItemCollection, mList.AddItem(SPResourcePath.FromDecodedUrl(rootFolderUrl), file, SPResourcePath.FromDecodedUrl(leafName)));
        }

        public IAveListItem GetItemById(int id)
        {
            SPListItem listItem = mList.GetItemById(id);
            if (listItem == null)
            {
                return null;
            }
            return new AveListItem(this.Items as AveListItemCollection, listItem);
        }

        public IAveListItem GetItemById(string id)
        {
            return GetItemById(int.Parse(id));
        }

        public IAveListItem GetItemByGuid(Guid tp_Guid)
        {
            int itemId = mSite.QueryService.GetTpIdByTpGuid(tp_Guid, mList.ID);
            return GetItemById(itemId);
        }

        [SPDisposeCheckIgnore(SPDisposeCheckID._170, "The dispose cleanup is handled automatically by the SharePoint framework.")]
        public IAveListItemCollection GetItems(AveCamlQuery camlQuery)
        {
            SPQuery query = new SPQuery();
            query.Folder = ((AveFolder)this.ParentWeb.GetFolder(camlQuery.FolderServerRelativeUrl)).Folder;
            query.DatesInUtc = camlQuery.DatesInUtc;
            if (camlQuery.ListItemCollectionPosition != null)
            {
                query.ListItemCollectionPosition = new SPListItemCollectionPosition(camlQuery.ListItemCollectionPosition.PagingInfo);
            }
            query.ViewXml = camlQuery.ViewXml;
            return new AveListItemCollection(this, mList.GetItems(query));
        }

        public void Update()
        {
            mList.Update();
        }

        public IAveListItemCollection GetItems(IAveQuery query)
        {
            return new AveListItemCollection(this, mList.GetItems((query as AveQuery).spQuery));
        }

        public IAveListItem GetItemByUniqueId(Guid uniqueId)
        {
            SPListItem listItem = mList.GetItemByUniqueId(uniqueId);
            if (listItem == null)
            {
                return null;
            }
            return new AveListItem(this.Items as AveListItemCollection, listItem);
        }

        public bool AllowDeletion
        {
            get
            {
                return mList.AllowDeletion;
            }
            set
            {
                mList.AllowDeletion = value;
            }
        }

        public IAveUser Author
        {
            get
            {
                if (mAuthor == null)
                {
                    mAuthor = new AveUser(this.ParentWeb as AveWeb, mList.Author);
                }
                return mAuthor;
            }
        }

        public IAveAudit Audit
        {
            get
            {
                if (mAudit == null)
                {
                    mAudit = new AveAudit(mList.Audit);
                }
                return mAudit;
            }
        }

        public AveBasePermissions AnonymousPermMask64
        {
            get
            {
                return (AveBasePermissions)mList.AnonymousPermMask64;
            }
            set
            {
                mList.AnonymousPermMask64 = (SPBasePermissions)value;
            }
        }

        public AveDefaultItemOpen DefaultItemOpen
        {
            get
            {
                return (AveDefaultItemOpen)mList.DefaultItemOpen;
            }
            set
            {
                mList.DefaultItemOpen = (DefaultItemOpen)value;
            }
        }

        public bool DefaultItemOpenUseListSetting
        {
            get { return mList.DefaultItemOpenUseListSetting; }
            set { mList.DefaultItemOpenUseListSetting = value; }
        }

        public bool DisableGridEditing
        {
            get
            {
                return mList.DisableGridEditing;
            }
            set
            {
                mList.DisableGridEditing = value;
            }
        }

        public string EmailAlias
        {
            get
            {
                return mList.EmailAlias;
            }
            set
            {
                mList.EmailAlias = value;
            }
        }

        public bool EnableAssignToEmail
        {
            get { return mList.EnableAssignToEmail; }
            set { mList.EnableAssignToEmail = value; }
        }

        public bool EnforceDataValidation
        {
            get { return mList.EnforceDataValidation; }
            set { mList.EnforceDataValidation = value; }
        }

        public bool EnableDeployingList
        {
            get { return mList.EnableDeployingList; }
            set { mList.EnableDeployingList = value; }
        }

        public bool EnableDeployWithDependentList
        {
            get { return mList.EnableDeployWithDependentList; }
            set { mList.EnableDeployWithDependentList = value; ; }
        }

        public bool EnablePeopleSelector
        {
            get { return mList.EnablePeopleSelector; }
            set { mList.EnablePeopleSelector = value; }
        }

        public bool EnableResourceSelector
        {
            get { return mList.EnableResourceSelector; }
            set { mList.EnableResourceSelector = value; }
        }

        public bool EnableSchemaCaching
        {
            get { return mList.EnableSchemaCaching; }
            set { mList.EnableSchemaCaching = value; }
        }

        public bool EnableSyndication
        {
            get { return mList.EnableSyndication; }
            set { mList.EnableSyndication = value; }
        }

        public bool EnableThrottling
        {
            get { return mList.EnableThrottling; }
            set { mList.EnableThrottling = value; }
        }

        public bool ExcludeFromOfflineClient
        {
            get { return mList.ExcludeFromOfflineClient; }
            set { mList.ExcludeFromOfflineClient = value; }
        }

        public IAveEventReceiverDefinitionCollection EventReceivers
        {
            get
            {
                if (mEventReceivers == null)
                {
                    mEventReceivers = new AveEventReceiverDefinitionCollection(mList.EventReceivers);
                }
                return mEventReceivers;
            }
        }

        public string EventSinkAssembly
        {
            get { return mList.EventSinkAssembly; }
            set { mList.EventSinkAssembly = value; }
        }

        #region get all the ListInfo from DB instead of API

        public AveListInfo GetListInfoByNative()
        {
            return mSite.QueryService.GetListInfo(this);
        }

        #endregion

        public AveListInfo GetListInfo()
        {
            AveListInfo info = GetListInfoByNative();

            if (mList.TemplateFeatureId.Equals(new Guid("00bfea71-de22-43b2-a848-c05709900100")))
            {
                info.ListSchema = mList.SchemaXml;
            }

            return info;
            //AveListInfo listInfo = new AveListInfo();


            //if (mList == null)//when {System Folder}, the list is null
            //{
            //    listInfo.Title = AveConstants.SYSTEM_FOLDER;
            //    return listInfo;
            //}
            //try
            //{
            //    listInfo.BaseTemplate = (int)mList.BaseTemplate;
            //    listInfo.TemplateFeatureId = mList.TemplateFeatureId;
            //    listInfo.BaseType = (int)mList.BaseType;
            //    listInfo.Title = mList.Title;
            //    listInfo.Description = mList.Description;
            //    listInfo.Id = mList.ID;
            //    string url = mList.RootFolder.ServerRelativeUrl.Substring(ParentWeb.RootFolder.ServerRelativeUrl.Length).Trim('/');
            //    listInfo.Url = ParentWeb.Url.TrimEnd('/') + "/" + url;
            //    listInfo.ServerRelativeUrl = mList.RootFolder.ServerRelativeUrl;
            //    if (mList.BaseTemplate == SPListTemplateType.ExternalList)
            //    {
            //        listInfo.DataSourceXml = (string)AveAssemblyUtility.InvokeMethod(mList.DataSource, mList.DataSource.GetType(), "ToXml", null);
            //    }

            //    listInfo.RootWebOnly = mList.RootWebOnly;

            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}
            //return listInfo;
        }

        public string GetPropertiesXmlForUncustomizedViews()
        {
            return mList.GetPropertiesXmlForUncustomizedViews();
        }

        public bool IrmEnabled
        {
            get { return mList.IrmEnabled; }
            set { mList.IrmEnabled = value; }
        }

        public bool IrmExpire
        {
            get { return mList.IrmExpire; }
            set { mList.IrmExpire = value; }
        }

        public bool IrmReject
        {
            get { return mList.IrmReject; }
            set { mList.IrmReject = value; }
        }

        public int MajorWithMinorVersionsLimit
        {
            get
            {
                return mList.MajorWithMinorVersionsLimit;
            }
            set
            {
                mList.MajorWithMinorVersionsLimit = value;
            }
        }

        public int MajorVersionLimit
        {
            get
            {
                return mList.MajorVersionLimit;
            }
            set
            {
                mList.MajorVersionLimit = value;
            }
        }

        public bool NavigateForFormsPages
        {
            get
            {
                return mList.NavigateForFormsPages;
            }
            set
            {
                mList.NavigateForFormsPages = value;
            }
        }

        public int ReadSecurity
        {
            get
            {
                return mList.ReadSecurity;
            }
            set
            {
                mList.ReadSecurity = value;
            }
        }

        public string SendToLocationName
        {
            get
            {
                return mList.SendToLocationName;
            }
            set
            {
                mList.SendToLocationName = value;
            }
        }

        public string SendToLocationUrl
        {
            get
            {
                return mList.SendToLocationUrl;
            }
            set
            {
                mList.SendToLocationUrl = value;
            }
        }

        public IAveViewCollection Views
        {
            get
            {
                if (mViews == null || mViews.IsDirty)
                {
                    mViews = new AveViewCollection(this, mList.Views);
                }
                return mViews;
            }
        }

        public int WriteSecurity
        {
            get
            {
                return mList.WriteSecurity;
            }
            set
            {
                mList.WriteSecurity = value;
            }
        }

        public IAveListItem AddItem(string folderUrl, AveFileSystemObjectType underlyingObjectType)
        {
            return new AveListItem(this.Items as AveListItemCollection, mList.AddItem(folderUrl, (SPFileSystemObjectType)underlyingObjectType));
        }

        public IAveListItem AddItem(string folderUrl, AveFileSystemObjectType underlyingObjectType, string leafName)
        {
            return new AveListItem(this.Items as AveListItemCollection, mList.AddItem(SPResourcePath.FromDecodedUrl(folderUrl), (SPFileSystemObjectType)underlyingObjectType, SPResourcePath.FromDecodedUrl(leafName)));
        }

        public void Delete()
        {
            mList.Delete();
        }

        public void EnsureRssSettings()
        {
            mList.EnsureRssSettings();
        }

        public bool AllowRssFeeds
        {
            get { return mList.AllowRssFeeds; }
        }

        public bool RootWebOnly
        {
            get
            {
                return mList.RootWebOnly;
            }
            set
            {
                mList.RootWebOnly = value;
            }
        }

        public IAveAlertTemplate AlertTemplate
        {
            get
            {
                if (mAlertTemplate == null)
                {
                    SPAlertTemplate alertTemplate = mList.AlertTemplate;
                    if (alertTemplate != null)
                    {
                        mAlertTemplate = new AveAlertTemplate(alertTemplate);
                    }
                }
                return mAlertTemplate;
            }
            set
            {
                mAlertTemplate = value as AveAlertTemplate;
                if (mAlertTemplate != null)
                {
                    mList.AlertTemplate = mAlertTemplate.AlertTemplate;
                }
                else
                {
                    mList.AlertTemplate = null;
                }
            }
        }

        public IAveAlertTemplate SmsAlertTemplate
        {
            get
            {
                if (mSmsAlertTemplate == null)
                {
                    SPAlertTemplate alertTemplate = mList.SmsAlertTemplate;
                    if (alertTemplate != null)
                    {
                        mSmsAlertTemplate = new AveAlertTemplate(alertTemplate);
                    }
                }
                return mSmsAlertTemplate;
            }
            set
            {
                mSmsAlertTemplate = value as AveAlertTemplate;
                if (mSmsAlertTemplate != null)
                {
                    mList.SmsAlertTemplate = mSmsAlertTemplate.AlertTemplate;
                }
                else
                {
                    mList.SmsAlertTemplate = null;
                }
            }
        }

        public IAveWorkflowAssociationCollection WorkflowAssociations
        {
            get
            {
                //if (mWorkflowAssociations == null)
                //{
                //mWorkflowAssociations = new AveWorkflowAssociationCollection(mList.WorkflowAssociations);
                //}
                mWorkflowAssociations = new AveWorkflowAssociationCollection(this, mList.WorkflowAssociations);
                return mWorkflowAssociations;
            }
        }

        public IAveFormCollection Forms
        {
            get
            {
                if (mForms == null || mForms.IsDirty)
                {
                    mForms = new AveFormCollection(mList.Forms);
                }
                return mForms;
            }
        }

        public List<AveRoleAssignmentInfo> GetRoleAssignments(string siteid, string scopeId)
        {
            return mSite.QueryService.GetListRoleAssignments(siteid, scopeId);
        }

        public string GetListViewSchema(Guid siteId, Guid listId)
        {
            return mSite.QueryService.GetListViewSchema(siteId, listId);
        }

        public IAveListDataSource DataSource
        {
            get
            {
                if (mDataSource == null)
                {
                    SPListDataSource listDataSource = mList.DataSource;
                    if (listDataSource != null)
                    {
                        mDataSource = new AveListDataSource(listDataSource);
                    }
                }
                return mDataSource;
            }
        }

        #endregion

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property")]
        public IAveListItemCollection Folders
        {
            get
            {
                if (mFolders == null)
                {
                    if (!this.HasExternalDataSource)
                    {
                        SPQuery query = new SPQuery();
                        query.ViewAttributes = "Scope=\"RecursiveAll\"";
                        query.Query = "<Where><Eq><FieldRef Name=\"FSObjType\" /><Value Type=\"Integer\">1</Value></Eq></Where>";
                        mFolders = this.GetItems(new AveQuery(this, query)) as AveListItemCollection;
                    }
                }
                return mFolders;
            }
        }

        internal void LoadFieldMap()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.LoadFieldMap"))
            {

                if (!mIsFieldMapLoaded)
                {
                    mFieldMap = new Dictionary<string, AveColumn>();
                    mIdFieldMap = new Dictionary<Guid, AveColumn>();
                    string listFieldSchemal = mSite.QueryService.GetFields(mSite.ID, ParentWeb.ID, mList.ID);
                    string listViewFieldsSchema = mSite.QueryService.GetViewFields(mList.ParentWeb.Site.ID, mList.ID);
                    Load(listFieldSchemal, listViewFieldsSchema, mFieldMap, mIdFieldMap);
                    mIsFieldMapLoaded = true;
                }

            }

        }

        protected void Load(string fieldSchema, string viewFields, Dictionary<string, AveColumn> fieldMap, Dictionary<Guid, AveColumn> idFieldMap)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.Load"))
            {

                fieldMap.Clear();
                StringBuilder SBColName = new StringBuilder();
                if (idFieldMap != null)
                {
                    idFieldMap.Clear();
                }
                XmlDocument mXDoc = new XmlDocument();
                HashSet<string> displayFields = GetDisplayFields(viewFields);
                List<string> fields = GetFieldsFromSchema(fieldSchema);
                foreach (string field in fields)
                {
                    mXDoc.InnerXml = field;
                    XmlElement xmlFirstChild = (XmlElement)mXDoc.FirstChild;
                    if (xmlFirstChild.HasAttribute(ColName))
                    {
                        string column = xmlFirstChild.Attributes[ColName].Value;
                        SBColName.Append(",").Append(column);
                        string row;
                        if (xmlFirstChild.HasAttribute(RowOrdinal))
                        {
                            row = xmlFirstChild.Attributes[RowOrdinal].Value;
                        }
                        else
                        {
                            row = "0";
                        }
                        string name = xmlFirstChild.Attributes["Name"].Value;
                        bool isDisplayColumn = displayFields.Contains(name);

                        AveColumn aveField = new AveColumn(name, column, isDisplayColumn);
                        fieldMap[row + column] = aveField;
                        if (idFieldMap != null && xmlFirstChild.HasAttribute("ID"))
                        {
                            idFieldMap[new Guid(xmlFirstChild.Attributes["ID"].Value)] = aveField;
                        }
                        // the other column of the list field
                        uint i = 2;
                        while (true)
                        {
                            string colName = ColName + i;
                            if (!xmlFirstChild.HasAttribute(colName))
                            {
                                break;
                            }
                            string rowOrdinal = RowOrdinal + i;
                            if (xmlFirstChild.HasAttribute(rowOrdinal))
                            {
                                row = xmlFirstChild.Attributes[rowOrdinal].Value;
                            }
                            else
                            {
                                row = "0";
                            }
                            column = xmlFirstChild.Attributes[colName].Value;
                            SBColName.Append(",").Append(column);
                            fieldMap[row + column] = new AveColumn(name + AveConstants.FIELD_SEPARATOR + i, column, false);
                            i++;
                        }
                    }
                }
                ColNameCollection = SBColName.ToString();

            }

        }

        private static HashSet<string> GetDisplayFields(string viewFieldsSchema)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.GetDisplayFields"))
            {

                HashSet<string> displayFields = new HashSet<string>();
                if (viewFieldsSchema == null)
                {
                    return displayFields;
                }
                displayFields.Add("Title");
                displayFields.Add("Created");
                displayFields.Add("Modified");

                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml("<AveViewFields>" + viewFieldsSchema + "</AveViewFields>");
                foreach (XmlNode node in xDoc.GetElementsByTagName("FieldRef"))
                {
                    string name = node.Attributes["Name"].Value;
                    if (!displayFields.Contains(name))
                    {
                        displayFields.Add(name);
                    }
                }

                return displayFields;

            }

        }

        protected List<string> GetFieldsFromSchema(string fieldSchema)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.GetFieldsFromSchema"))
            {

                List<string> fields = new List<string>();
                XmlDocument xDoc = new XmlDocument();

                xDoc.InnerXml = fieldSchema;
                foreach (XmlNode node in xDoc.FirstChild.ChildNodes)
                {
                    fields.Add(node.OuterXml);
                }

                return fields;

            }

        }

        //only used for item user data
        public void ReplaceFieldNames(Dictionary<string, object> oldData, Dictionary<string, object> newData, byte rowOrdinal)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.ReplaceFieldNames"))
            {

                AveColumn field;
                string name;
                foreach (KeyValuePair<string, object> pair in oldData)
                {
                    if (SKIP_FIELD_MAP.TryGetValue(pair.Key, out name))
                    {
                        //有多行UserData记录的情况下，第一行记录是正确的。
                        if (!newData.ContainsKey(name))
                        {
                            newData[name] = pair.Value;
                        }
                        continue;
                    }
                    if (NEED_FIELD_MAP.TryGetValue(pair.Key, out name))
                    {
                        if (rowOrdinal.ToString().Equals("0"))
                        {
                            newData[name] = pair.Value;
                        }
                        continue;
                    }
                    name = rowOrdinal + pair.Key;
                    if (mFieldMap.TryGetValue(name, out field))
                    {
                        newData[field.BackupName] = pair.Value;
                    }
                    else if (rowOrdinal > 0)
                    {
                        continue;
                    }
                    else
                    {
                        newData[AveConstants.FIELD_SEPARATOR + pair.Key] = pair.Value;
                    }
                }

            }

        }

        public override IAveSecurableObjectImpl SecurableObjectImpl
        {
            get { return new AveSecurableObjectImpl(this.ParentWeb as AveWeb, AveAssemblyUtility.GetPropertyValue(mList, "SecurableObjectImpl")); }
        }

        public IAveListCollection Lists
        {
            get { return mLists; }
        }

        #region IAveList Members

        public Guid ID
        {
            get { return mList.ID; }
        }

        public bool ExcludeFromTemplate
        {
            get
            {
                return mList.ExcludeFromTemplate;
            }
        }

        public bool IsThrottled
        {
            get
            {
                return mList.IsThrottled;
            }
        }

        public bool Ordered
        {
            get
            {
                return mList.Ordered;
            }
            set
            {
                mList.Ordered = value;
            }
        }

        public bool ShowUser
        {
            get
            {
                return mList.ShowUser;
            }
            set
            {
                mList.ShowUser = value;
            }
        }

        public bool IsSchedulingEventOnList()
        {
            Type type = typeof(Microsoft.SharePoint.Publishing.Internal.ScheduledItemEventReceiver);
            string strB = type.Assembly.FullName.ToString();
            string fullName = type.FullName;
            SPEventReceiverType itemUpdating = SPEventReceiverType.ItemUpdating;
            SPEventReceiverType itemAdded = SPEventReceiverType.ItemAdded;
            SPEventReceiverDefinitionCollection eventReceivers = mList.EventReceivers;
            bool flag = false;
            bool flag2 = false;
            foreach (SPEventReceiverDefinition definition in eventReceivers)
            {
                if ((string.Compare(definition.Assembly, strB, StringComparison.Ordinal) != 0) || (string.Compare(definition.Class, fullName, StringComparison.Ordinal) != 0))
                {
                    continue;
                }
                if (definition.Type == itemUpdating)
                {
                    flag = true;
                }
                else if (definition.Type == itemAdded)
                {
                    flag2 = true;
                }
                if (flag && flag2)
                {
                    break;
                }
            }
            return (flag && flag2);
        }

        public IAveListItem AddItem()
        {
            return new AveListItem(this.Items as AveListItemCollection, mList.AddItem());
        }

        public IAveListItem GetItemByIdSelectedFields(int id, params string[] fields)
        {
            SPListItem listItem = mList.GetItemByIdSelectedFields(id, fields);
            if (listItem != null)
            {
                return new AveListItem(this.Items as AveListItemCollection, listItem);
            }
            return null;
        }

        #endregion

        public IAveView GetView(Guid viewGuid)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.GetView"))
            {

                if (viewGuid != Guid.Empty)
                {
                    return this.Views[viewGuid];
                }
                return this.Views.DefaultView;

            }

        }

        public AveListSettingInfo GetListSettings()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.GetListSettings"))
            {

                var listSettingInfo = new AveListSettingInfo();
                mSite.QueryService.GetListSettingInfoByNative(mSite.ID, ParentWeb.ID, ID, listSettingInfo);
                AveSPListUtility.AssemblyListAllSettingInfo(this, listSettingInfo);
                return listSettingInfo;

            }

        }

        //use this function to analyze the RootFolderInfo's MetaInfo and store it in Hashtable

        private bool isEnableMetaPublishing()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.isEnableMetaPublishing"))
            {

                bool isEnable = false;

                for (int i = mList.EventReceivers.Count - 1; i >= 0; i--)
                {
                    SPEventReceiverDefinition definition = mList.EventReceivers[i];
                    if (((definition.Name == AveListMetaDateSettingInfo.AddedName) && (definition.Type == SPEventReceiverType.ItemAdded)) && (definition.Assembly == AveListMetaDateSettingInfo.AssembleName))
                    {
                        isEnable = true;
                    }
                    else if (((definition.Name == AveListMetaDateSettingInfo.UpdateName) && (definition.Type == SPEventReceiverType.ItemUpdated)) && (definition.Assembly == AveListMetaDateSettingInfo.AssembleName))
                    {
                        isEnable = true;
                    }
                }

                return isEnable;

            }

        }

        private bool isSchedulingEventOnList()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.isSchedulingEventOnList"))
            {

                Type type = typeof(Microsoft.SharePoint.Publishing.Internal.ScheduledItemEventReceiver);
                string strB = type.Assembly.FullName.ToString();
                string fullName = type.FullName;
                SPEventReceiverType itemUpdating = SPEventReceiverType.ItemUpdating;
                SPEventReceiverType itemAdded = SPEventReceiverType.ItemAdded;
                SPEventReceiverDefinitionCollection eventReceivers = mList.EventReceivers;
                bool flag = false;
                bool flag2 = false;
                foreach (SPEventReceiverDefinition definition in eventReceivers)
                {
                    if ((string.Compare(definition.Assembly, strB, StringComparison.Ordinal) != 0) || (string.Compare(definition.Class, fullName, StringComparison.Ordinal) != 0))
                    {
                        continue;
                    }
                    if (definition.Type == itemUpdating)
                    {
                        flag = true;
                    }
                    else if (definition.Type == itemAdded)
                    {
                        flag2 = true;
                    }
                    if (flag && flag2)
                    {
                        break;
                    }
                }
                return (flag && flag2);

            }

        }

        private bool IsServerTemplateCanCreateFolders(ulong flags, SPListTemplateType BaseTemplate, SPWeb ParentWeb, SPBaseType BaseType)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.IsServerTemplateCanCreateFolders"))
            {

                if ((((((BaseTemplate == SPListTemplateType.WebTemplateCatalog) ||
                    (BaseTemplate == SPListTemplateType.WebPartCatalog)) ||
                    ((BaseTemplate == SPListTemplateType.ListTemplateCatalog) ||
                    (BaseTemplate == SPListTemplateType.SolutionCatalog))) ||
                    (((BaseTemplate == SPListTemplateType.ThemeCatalog) ||
                    (BaseTemplate == SPListTemplateType.UserInformation)) ||
                    (BaseTemplate == SPListTemplateType.Survey))) ||
                    (SPMeeting.IsMeetingWorkspaceWeb(ParentWeb) &&
                    (BaseType != SPBaseType.DocumentLibrary))) || Ave2010ListFlags.HasExternalDataSource(flags))
                {
                    return false;
                }
                SPListTemplateCollection listTemplates = ParentWeb.ListTemplates;
                foreach (SPListTemplate template in listTemplates)
                {
                    if (template.Type == BaseTemplate)
                    {
                        return template.AllowsFolderCreation;
                    }
                }
                return true;

            }

        }

        private void PreRestoreListItem(AveListItemInfo info, AveItem aveItem, Dictionary<string, object> data, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.PreRestoreListItem"))
            {

                if (data.ContainsKey("DoclibRowId"))
                {
                    if (info.SettingInfo.KEEP_ITEM_TPGUID)
                    {
                        aveItem.CheckConflictState(info.RestoringItem, info.SiteId, info.ParentId, info.tp_Guid);
                    }
                    else if (this.mList != null && mList.BaseTemplate == SPListTemplateType.DiscussionBoard && userData.ContainsKey("MessageId"))
                    {
                        //只有是list是DiscussionBoard的时候而且是reply有messageId 才应该用特殊的判断冲突
                        string messageId = userData["MessageId"].ToString();
                        Guid messgageIdFieldId = SPBuiltInFieldId.MessageId;
                        string messgageIdFieldColName = AveAssemblyUtility.GetPropertyValue(mList.Fields[messgageIdFieldId], "ColName").ToString();
                        aveItem.CheckConflictStateForDiscussionReply(info.RestoringItem, info.SiteId, info.ParentId, messageId, messgageIdFieldColName);
                    }
                    else
                    {
                        //mParentFolder.RestoringItem.CheckConflictState(mSqlConn, mSiteId, mParentId);
                        aveItem.CheckConflictStateForListItem(info.RestoringItem, info.SiteId);
                    }
                }
                if (info.RestoringItem.ConflilctFromRecycleBin)
                {
                    //只有在Check for Conflicts in Destination Recycle Bin选择yes，冲突处理选择skip的条件下不会清空回收站
                    if (!(info.RestoringItem.IsIncludingRecycleBinData && info.RestoreOption == AveRestoreMode.Default))
                    {
                        if (info.SettingInfo.KEEP_ITEM_TPGUID)
                        {
                            mSite.QueryService.RemoveListItemInRecycleBin(this.ParentWeb.Site, info.ParentId, info.tp_Guid);
                        }
                        else
                        {
                            mSite.QueryService.RemoveItemInRecycleBin(this.ParentWeb.Site, info.ParentId, info.Name);
                        }
                    }
                }
                if (info.RestoringItem.ConflictWithDocument)
                {
                    try
                    {
                        if (userData.ContainsKey("Modified") && aveItem.SkipIfSameModifiedTime(info, userData["Modified"]))
                        {
                            info.RestoringItem.NeedSkipped = true;
                            throw new AveRestoreException(AveRestoreResult.SkipTheSameItem, AveRestoreResult.SkipTheSameItem.ToString());
                        }
                        if (data.ContainsKey("BiggestVersionModified") && !aveItem.OverwriteByModifiedTime(info, data["BiggestVersionModified"], null))
                        {
                            info.RestoringItem.NeedSkipped = true;
                            //return AveRestoreResult.Omit;
                            throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                        }
                    }
                    catch (AveRestoreException)
                    {
                        throw;
                    }
                    try
                    {
                        int tempRowId = info.OriginalRowId;
                        //只给Replicator使用，因为Replicator之前有discover逻辑，不需要在有Verify的逻辑
                        if (info.SettingInfo.NewItemWithOutVerifyConflict)
                        {
                            tempRowId = 0;
                        }//只给Replicator使用，因为Replicator知道目的端的RowId
                        else if (info.SettingInfo.IncreaceVerionWithRowId && info.RowId > 0)
                        {
                            tempRowId = info.RowId;
                        }
                        else
                        {
                            if (info.SettingInfo.KEEP_ITEM_TPGUID)
                            {
                                tempRowId = mSite.QueryService.GetTpIdByTpGuid(mSite.ID, info.tp_Guid, mList.ID);
                            }
                            else
                            {
                                tempRowId = info.MappingManager.SiteMappingManager.GetMappingItemId(info.ListId, tempRowId, tempRowId);
                            }
                        }

                        if (tempRowId > 0)
                        {
                            aveItem.mSPListItem = mList.GetItemById(tempRowId);
                            //同步重新赋值
                            aveItem.ListItem = new AveListItem(aveItem.mList, aveItem.mSPListItem);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetRowIdError, e.ToString());
                    }

                    if (aveItem.mSPListItem != null)
                    {
                        if (aveItem.mSPListItem.FileSystemObjectType == SPFileSystemObjectType.Folder)
                        {
                            SPFolder spFolder = null;
                            spFolder = aveItem.mSPListItem.Folder;
                            if (spFolder != null)
                            {
                                throw new ItemTypeConflictException(info.OriginalRowId, spFolder.ServerRelativeUrl);
                            }
                            throw new ItemTypeConflictException(info.OriginalRowId, aveItem.mSPListItem.Url);
                        }
                        //TODO: IF overwrite.
                        if (info.SettingInfo.DELETE_ITEM)
                        {
                            try
                            {
                                if (!IsReportingMetadataList() && mList.BaseTemplate != SPListTemplateType.Meetings)
                                {
                                    bool movedSuccess = false;
                                    if (info.SettingInfo.MOVE_ITEM_TO_CONFLICT_FOLDER)
                                    {
                                        movedSuccess = aveItem.MoveToConflictFolder(aveItem.mSPList, aveItem.mParentFolder, aveItem.mSPListItem, true);
                                    }
                                    if (!movedSuccess && !IsWorkflowTask(aveItem))//如果Workflow Task外围没有进行过滤，还原时不能删除目的端，以免造成破坏
                                    {
                                        aveItem.UnLockItem(aveItem.mSPListItem);
                                        aveItem.mSPListItem.Delete();
                                        aveItem.mSPListItem = null;
                                    }
                                    else
                                    {
                                        aveItem.mSPListItem = null;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                if (aveItem.mSPListItem != null)
                                {
                                    logger.Log(AveLogLevel.WARN, ServerAPIResource.ItemCannotBeDelete, aveItem.mSPListItem.Title, e);
                                    if (aveItem.mSPListItem.ParentList.BaseTemplate == SPListTemplateType.Events && mSite.QueryService.IsItemExist(aveItem.mSPListItem.ParentList.ID, aveItem.mSPListItem.ID, aveItem.mSPListItem.ParentList.ParentWeb.Site.ID)) //删掉了
                                    {
                                        aveItem.mSPListItem = null;
                                    }
                                }
                            }
                        }
                    }
                }
                info.RestoringItem.TargetTable = info.RestoringItem.GetTargetTable(info.OriginalVersion, info.IsVersion);
                if (info.RestoringItem.TargetTable == RestoreTargetTable.None)
                {
                    //return AveRestoreResult.Omit;
                    throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                }

            }

        }

        private AveRestoreResult RealRestoreListItem(AveListItemInfo info, AveItem aveItem, Dictionary<string, object> data, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.RealRestoreListItem"))
            {

                AveRestoreResult result = AveRestoreResult.Normal;
                //#region only for WebDataBase Template
                //if (aveItem.mList.IsACCSRVSystemList())
                //{
                //    mParentFolder.RestoringItem.NeedSkipped = true;
                //    return Int32.MinValue;
                //}
                //#endregion
                result = AddListItem(info, aveItem, data, userData);
                result = UpdateListItemVersion(info, aveItem, data, userData);
                return result;

            }

        }

        private AveRestoreResult AddListItem(AveListItemInfo info, AveItem aveItem, Dictionary<string, object> data, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.AddListItem"))
            {

                bool listItemHasUpdated = false;

                if (aveItem.mSPListItem == null)
                {
                    //Below we need to set the content type once item created, otherwise Update() will give this item default value to the default content type.(Doc-59767)
                    SPContentTypeId itemContentTypeId = SPContentTypeId.Empty;
                    try
                    {
                        if (info.FieldsInfo.Fields.ContainsKey("ContentType"))
                        {
                            itemContentTypeId = ((info.FieldsInfo.Fields["ContentType"] as AveFieldValueInfo).ColValue as AveContentTypeId).ContentTypeId;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetContentTypeIdError, e.ToString());
                    }

                    if (mList.BaseTemplate == SPListTemplateType.DiscussionBoard
                        && userData.ContainsKey("#ThreadIndexParentId")
                        && (int)userData["#ThreadIndexParentId"] > 0)
                    {
                        SPListItem item = aveItem.mParentFolder.Item;
                        int parentId = (int)userData["#ThreadIndexParentId"];
                        try
                        {
                            int newId = info.MappingManager.SiteMappingManager.GetMappingItemId(info.ListId, parentId);
                            item = mList.GetItemById(newId);
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetItemByIdError, e.ToString());
                        }
                        if (item == null)
                        {
                            item = aveItem.mParentFolder.Item;
                        }
                        aveItem.mSPListItem = SPUtility.CreateNewDiscussionReply(item);
                        //同步重新赋值
                        aveItem.ListItem = new AveListItem(aveItem.mList, aveItem.mSPListItem);
                    }
                    else if (mList.BaseTemplate == SPListTemplateType.MeetingUser)
                    {
                        aveItem.mSPListItem = mList.AddItem(aveItem.mParentFolder.ServerRelativeUrl, SPFileSystemObjectType.File);
                        //同步重新赋值
                        aveItem.ListItem = new AveListItem(aveItem.mList, aveItem.mSPListItem);
                        if (userData.ContainsKey("Status"))
                        {
                            aveItem.mSPListItem["Status"] = userData["Status"].ToString();
                        }
                        if (userData.ContainsKey("Attendance"))
                        {
                            aveItem.mSPListItem["Attendance"] = userData["Attendance"].ToString();
                        }
                        if (userData.ContainsKey("Title"))
                        {
                            AveAssemblyUtility.SetFieldValue(aveItem.mSPListItem, "m_strNewBaseName", userData["Title"].ToString());
                        }
                    }
                    else if (mList.BaseTemplate == SPListTemplateType.Meetings)
                    {
                        aveItem.mSPListItem = RestoreMeetingSeriesItem(info, userData, ref listItemHasUpdated);
                        //同步重新赋值
                        aveItem.ListItem = new AveListItem(aveItem.mList, aveItem.mSPListItem);
                        if (aveItem.mSPListItem == null)
                        {
                            //return AveRestoreResult.Failed;
                            throw new AveRestoreException(AveRestoreResult.Failed, AveRestoreResult.Failed.ToString());
                        }
                    }
                    else
                    {
                        //aveItem.mSPListItem = mList.Items.Add(aveItem.mParentFolder.ServerRelativeUrl, SPFileSystemObjectType.File);
                        aveItem.mSPListItem = mList.AddItem(aveItem.mParentFolder.ServerRelativeUrl, SPFileSystemObjectType.File);
                        //同步重新赋值
                        aveItem.ListItem = new AveListItem(aveItem.mList, aveItem.mSPListItem);
                        //Here we need to set the content type once item created, otherwise Update() will give this item default value to the default content type.(Doc-59767)
                        SPContentTypeId emptyContentTypeId = SPContentTypeId.Empty;
                        if (!SPContentTypeId.Equals(itemContentTypeId, emptyContentTypeId))
                        {
                            aveItem.mSPListItem["ContentTypeId"] = itemContentTypeId;
                        }
                        //end
                    }

                    try
                    {
                        #region Discussion Reply
                        //新添加一个reply会修改Subject的Last Updated，这update之前获得Parent的Last Updated值
                        bool isDiscussionReply = false;
                        DateTime time = DateTime.MinValue;
                        SPListItem parentItem = null;
                        try
                        {
                            if (mList.BaseTemplate == SPListTemplateType.DiscussionBoard
                                && userData.ContainsKey("#ThreadIndexParentId")
                                && (int)userData["#ThreadIndexParentId"] > 0)
                            {
                                parentItem = aveItem.mParentFolder.Item;
                                try
                                {
                                    parentItem = mList.GetItemById(parentItem.ID);
                                }
                                catch (Exception e)
                                {
                                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetListIemFaild, e.ToString());
                                }
                                isDiscussionReply = true;
                                time = (DateTime)parentItem[SPBuiltInFieldId.DiscussionLastUpdated];
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetItemDisTimeError, e.ToString());
                            isDiscussionReply = false;
                        }
                        #endregion
                        // InstanceID 属性需要更新，但在updateField时更新不成功，所以放在这里更新
                        if (userData.ContainsKey("#tp_InstanceID"))
                        {
                            aveItem.mSPListItem["InstanceID"] = userData["#tp_InstanceID"];
                        }
                        if (AveWebDatabaseSite.IsWebDatabaseWeb(aveItem.mWeb.Web))
                        {
                            AveWebDatabaseSite.AppendRequiredFieldsForNewItem(aveItem.mSPListItem, data, userData);
                        }
                        //新建一个item时，在update之前set "GUID"，可以将其更新,不是所有的listitem都有这个guid的属性
                        if (!listItemHasUpdated && userData.ContainsKey("#tp_GUID") && aveItem.mSPListItem.Fields.ContainsField("GUID"))
                        {
                            aveItem.mSPListItem["GUID"] = info.tp_Guid;//userData["#tp_GUID"];
                        }
                        //新建一个item时，在update之前用SetIDForMigration设置itemId，可以将itemId更新
                        if (!listItemHasUpdated && mSite.QueryService.CheckItemIdAvailable(mSite.ID, mList.ID, info.OriginalRowId) && info.NeedChangeItemId)
                        {
                            MigrateItemId(aveItem, info);
                        }
                        else
                        {
                            aveItem.mSPListItem.Update();
                        }

                        //survey list response的fields没有GUID ，因此做特殊处理
                        if (mList.BaseTemplate == SPListTemplateType.Survey)
                        {
                            mSite.QueryService.UpdateItemGuid(info.tp_Guid, aveItem.mSPListItem.UniqueId, aveItem.mParentFolder.UniqueId, mSite.ID, (bool)userData["#tp_IsCurrentVersion"], (byte)aveItem.mSPListItem.Level, (int)userData["#tp_CalculatedVersion"]);
                        }
                        #region Discussion Reply
                        try
                        {
                            //如果是Reply，将Parent Subject的Last Updated值改回去
                            if (isDiscussionReply && parentItem != null)
                            {
                                parentItem[SPBuiltInFieldId.DiscussionLastUpdated] = time;
                                parentItem.SystemUpdate(false);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.DEBUG, ServerAPIResource.UpdateItemError, e);
                        }
                        #endregion
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Cannot add list item: {0} in the list: {1}, details: {2}", info.Name, mList.Title, e.ToString());
                    }
                    //使用API更新itemID，在新建item时，使用SetIDForMigration方法，设置ID值
                    //if (aveItem.mSPListItem.ID != info.OriginalRowId)
                    //{
                    //    int returnCode = mSite.DBService.ChangeItemId(info.SiteId, aveItem.mSPListItem.UniqueId, mList.RootFolder.UniqueId, 1, aveItem.mSPListItem.ID, info.OriginalRowId);
                    //    if (returnCode == 0)
                    //    {
                    //        aveItem.mSPListItem = mList.GetItemById(info.OriginalRowId);
                    //    }
                    //}
                    info.IsNewCreated = true;
                }

                //TODO: check the SPListItem int ID(map)
                aveItem.InitBySPListItem(aveItem.mSPListItem);

                mSite.QueryService.UpdateAllDocsPropertyByNative(info, info.DTimeCreated, info.DTimeLastModified, info.OriginalVersion);

                return AveRestoreResult.Normal;

            }

        }

        /// <summary>
        /// 在某些情况下更改了Id然后update，API会出错，暂时没有找到具体原因以及好的解决方案，添加try catch
        /// 如果update失败，revert相关逻辑，不进行Id替换
        /// </summary>
        /// <param name="aveItem"></param>
        /// <param name="info"></param>
        private void MigrateItemId(AveItem aveItem, AveListItemInfo info)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.MigrateItemId"))
            {

                int itemIdNewCreated = aveItem.mSPListItem.ID;
                AveAssemblyUtility.InvokeMethod(aveItem.mSPListItem, "SetIDForMigration", new Type[] { typeof(int) }, new object[] { info.OriginalRowId });
                try
                {
                    aveItem.mSPListItem.Update();
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.ListItemUpdateError, e);
                    AveAssemblyUtility.InvokeMethod(aveItem.mSPListItem, "SetIDForMigration", new Type[] { typeof(int) }, new object[] { itemIdNewCreated });
                    aveItem.mSPListItem.Update();
                }
                mSite.QueryService.ChangeNextItemId(info.OriginalRowId, mSite.ID, mList.ID);

            }

        }

        private AveRestoreResult UpdateListItemVersion(AveListItemInfo info, AveItem aveItem, Dictionary<string, object> data, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.UpdateListItemVersion"))
            {

                AveRestoreResult result = AveRestoreResult.Normal;
                if (info.OriginalVersion < info.Version)
                {
                    //originalVersion < destVersion
                    //Insert version
                    if (!mSite.QueryService.CreateVersionByNative(info, info.OriginalVersion, info.RestoringItem))
                    {
                        //return AveRestoreResult.Failed;
                        throw new AveRestoreException(AveRestoreResult.Failed, AveRestoreResult.Failed.ToString());
                    }
                    info.FieldsInfo.Fields = aveItem.ConvertToFieldWithNativeName(info.FieldsInfo.Fields);
                    //用数据库增加version，有些field需要添加进来
                    if (data.ContainsKey("DraftOwnerId"))
                    {
                        data["DraftOwnerId"] = info.DraftOwnerId;
                        info.FieldsInfo.Fields.Add("tp_DraftOwnerId", info.DraftOwnerId);
                    }
                    if (userData.ContainsKey("#tp_IsCurrentVersion"))
                    {
                        info.FieldsInfo.Fields.Add("tp_IsCurrentVersion", userData["#tp_IsCurrentVersion"]);
                    }
                    info.FieldsInfo.Fields.Add("tp_ModerationStatus", info.ModerationStatus);

                    //插入Version，如果目的端已经存在，这时候不去修改Level的值，否则会导致结构乱套
                    //如果是100表示这个记录是我们自己插入的
                    int originalLevel = info.OriginalLevel;
                    byte level = mSite.QueryService.GetLevel(info, info.OriginalVersion);
                    if (level != 100)
                    {
                        originalLevel = level;
                    }

                    info.FieldsInfo.Fields.Add("tp_Level", originalLevel);
                    mSite.QueryService.UpdateVersionByNative(info, info.RestoringItem, data, info.FieldsInfo.Fields, info.OriginalVersion);
                    info.Level = originalLevel;
                    result = AveRestoreResult.ResoreLessVersion;
                }
                else if (info.OriginalVersion == info.Version)
                {
                    // originalVersion == destVersion
                    SPModerationStatusType moderationType = (SPModerationStatusType)info.ModerationStatus;
                    if (aveItem.mSPListItem.ParentList.EnableModeration && aveItem.mSPListItem.ModerationInformation != null && aveItem.mSPListItem.ModerationInformation.Status != moderationType)
                    {
                        if (moderationType == SPModerationStatusType.Approved)
                        {
                            try
                            {
                                aveItem.mSPListItem.ModerationInformation.Status = moderationType;
                                aveItem.mSPListItem.ModerationInformation.Comment = info.ModerationComments;
                                aveItem.mSPListItem.Update();//TO DO Proformance
                                info.Level = (byte)aveItem.mSPListItem.Level;
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.UpdateItemError, e);
                                Reload();
                                aveItem.mSPListItem = mList.GetItemById(aveItem.mSPListItem.ID);
                                //同步重新赋值
                                aveItem.ListItem = new AveListItem(aveItem.mList, aveItem.mSPListItem);
                                info.NeedUpdateStatusByNative = true;
                            }
                        }
                        else
                        {
                            info.NeedUpdateStatusByNative = true;
                        }
                    }
                    aveItem.UpdateFields(info.FieldsInfo.Fields, info, false, false);
                    if (aveItem.mSPListItem.Level != (SPFileLevel)info.OriginalLevel || (aveItem.mSPListItem.Level == SPFileLevel.Draft))
                    {
                        mSite.QueryService.ChangeLevelByNative(info, aveItem.ListItem, info.OriginalVersion, info.OriginalLevel, info.DraftOwnerId);
                        info.Level = info.OriginalLevel;
                    }
                    result = AveRestoreResult.RestoreEqualVersion;
                    //TODO: update system property
                }
                else
                {
                    // originalVersion > destVersion
                    aveItem.CreateItemVersion(info.OriginalVersion, info.IsNewCreated);
                    if (aveItem.mSPListItem.ModerationInformation != null && aveItem.mSPListItem.ModerationInformation.Status != (SPModerationStatusType)info.ModerationStatus)
                    {
                        try
                        {
                            aveItem.mSPListItem.ModerationInformation.Status = (SPModerationStatusType)info.ModerationStatus;
                            aveItem.mSPListItem.Update();
                            info.Level = (byte)aveItem.mSPListItem.Level;
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.DEBUG, ServerAPIResource.UpdateItemError, e);
                            aveItem.mSPListItem = mList.GetItemById(aveItem.mSPListItem.ID);
                            //同步重新赋值
                            aveItem.ListItem = new AveListItem(aveItem.mList, aveItem.mSPListItem);
                        }
                    }
                    aveItem.UpdateFields(info.FieldsInfo.Fields, info, false, false);

                    info.IsNewCreated = true;
                    if (aveItem.mSPListItem.Level != (SPFileLevel)info.OriginalLevel)
                    {
                        mSite.QueryService.ChangeLevelByNative(info, aveItem.ListItem, info.OriginalVersion, info.OriginalLevel, info.DraftOwnerId);
                        info.Level = info.OriginalLevel;
                        //mSPListItem = mAveSPList.SPList.GetItemById(mSPListItem.ID);
                    }
                    //TODO: update system property
                    result = AveRestoreResult.RestoreBiggerVersion;
                }
                return result;

            }

        }

        [Obsolete("Use AveListItemSerializer instead")]
        private void PostRestoreListItem(AveListItemInfo info, AveItem aveItem)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.PostRestoreListItem"))
            {

                // for survey response
                if (mList.BaseTemplate == SPListTemplateType.Survey)
                {
                    if (info.OriginalLevel == 255 && info.CheckoutUserId > 0)
                    {
                        mSite.QueryService.ChangeCheckoutUserID(info, aveItem.mSPListItem.UniqueId, info.CheckoutUserId);
                    }
                }
                if (info.NeedUpdateStatusByNative)
                {
                    //mSite.QueryService.ChangeModerationStatusByNative(info, aveItem.ListItem.UniqueId, info.ModerationStatus);
                }
                //使用API更新TPGUID，在新建item时将TPGUID更新进去
                //if (info.IsNewCreated && info.SettingInfo.KEEP_ITEM_TPGUID)
                //{
                //    mSite.DBService.ChangeItemTPGuidByNative(info, info.SiteId, info.ParentId, aveItem.mSPListItem.UniqueId, info.tp_Guid);
                //}

            }

        }

        [Obsolete("Use AveListItemSerializer instead")]
        public AveRestoreResult RestoreListItem(AveListItemInfo info, Dictionary<string, object> data, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.RestoreListItem"))
            {

                AveItem aveItem = info.AveItem as AveItem;
                AveRestoreResult result = AveRestoreResult.Normal;
                PreRestoreListItem(info, aveItem, data, userData);
                result = RealRestoreListItem(info, aveItem, data, userData);
                PostRestoreListItem(info, aveItem);
                return result;

            }

        }
        [SuppressMessage("FxCopCustomRules", "C100003:DoNotUseSpecificSPMethod")]
        private SPListItem RestoreMeetingSeriesItem(AveListItemInfo info, Dictionary<string, object> userData, ref bool listItemHasUpdated)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.RestoreMeetingSeriesItem"))
            {

                try
                {
                    SPListItem listItem = null;
                    try
                    {
                        listItem = mList.GetItemById((int)userData["#tp_ID"]);
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetListIemFaild, e.ToString());
                    }
                    if (listItem == null)
                    {
                        listItem = mList.AddItem();
                    }
                    if (userData.ContainsKey("Title"))
                    {
                        listItem["Title"] = userData["Title"];
                    }
                    int eventType = 0;
                    if (userData.ContainsKey("EventType"))
                    {
                        eventType = (int)userData["EventType"];
                        //update will fail if we don't assign recurrenceID field when eventtype is 0,please see ADO-5026 for more detail
                        if (eventType == 0)
                        {
                            listItem["EventType"] = 2;
                            listItem["RecurrenceID"] = DateTime.Now;
                        }
                        else
                        {
                            listItem["EventType"] = eventType;
                        }
                    }
                    int timeZoneId = -1;
                    SPTimeZone timeZone = null;
                    DateTime eventDate = DateTime.MinValue;
                    DateTime endDate = DateTime.MinValue;
                    int duration = 0;
                    if (userData.ContainsKey("TimeZone"))
                    {
                        timeZoneId = (int)userData["TimeZone"];
                        listItem["TimeZone"] = timeZoneId;
                    }
                    else if (userData.ContainsKey("UID") && (eventType == 2 || eventType == 3))
                    {
                        foreach (SPListItem tItem in mList.Items)
                        {
                            if (tItem["UID"] != null && (Guid)userData["UID"] == new Guid(tItem["UID"].ToString())
                                && (int)tItem["EventType"] == 1
                                && tItem["TimeZone"] != null)
                            {
                                timeZoneId = (int)tItem["TimeZone"];
                                if (tItem["Duration"] != null)
                                {
                                    duration = (int)tItem["Duration"];
                                }
                                break;
                            }
                        }
                    }
                    if (AveListMappingManager.TimeZoneDic == null)
                    {
                        AveListMappingManager.TimeZoneDic = new Dictionary<int, IAveTimeZone>();
                        foreach (SPTimeZone tz in SPRegionalSettings.GlobalTimeZones)
                        {
                            AveListMappingManager.TimeZoneDic.Add(tz.ID, new AveTimeZone(tz));
                        }
                    }
                    if (timeZoneId == 0)
                    {
                        timeZoneId = 93;
                    }
                    if (AveListMappingManager.TimeZoneDic.ContainsKey(timeZoneId))
                    {
                        timeZone = (AveListMappingManager.TimeZoneDic[timeZoneId] as AveTimeZone).TimeZone;
                    }
                    else
                    {
                        SPUser agentAccount = listItem.ParentList.ParentWeb.Site.RootWeb.CurrentUser;
                        if (agentAccount != null && agentAccount.RegionalSettings != null)
                        {
                            timeZone = agentAccount.RegionalSettings.TimeZone;
                        }
                        else
                        {
                            timeZone = listItem.ParentList.ParentWeb.RegionalSettings.TimeZone;
                        }
                    }
                    if (userData.ContainsKey("EventDate"))
                    {
                        eventDate = timeZone.UTCToLocalTime(Convert.ToDateTime(userData["EventDate"], System.Globalization.DateTimeFormatInfo.InvariantInfo));
                        listItem["EventDate"] = eventDate;
                    }
                    if (userData.ContainsKey("Duration"))
                    {
                        duration = (int)userData["Duration"];
                        listItem["Duration"] = duration;
                    }

                    if (userData.ContainsKey("EndDate"))
                    {
                        endDate = Convert.ToDateTime(userData["EndDate"], System.Globalization.DateTimeFormatInfo.InvariantInfo);
                        //MaxDateTime
                        if (endDate.Year == 9999 && eventDate != DateTime.MinValue && duration != 0)
                        {
                            TimeSpan tsEventDate = eventDate.TimeOfDay;
                            TimeSpan tsDuration = TimeSpan.FromSeconds(duration);
                            endDate = endDate.Date.Add(tsEventDate).Add(tsDuration);
                        }
                        else
                        {
                            endDate = timeZone.UTCToLocalTime(endDate);
                        }
                        listItem["EndDate"] = endDate;
                    }
                    else if (eventType == 3 && eventDate != DateTime.MinValue && duration != 0)
                    {
                        TimeSpan tsDuration = TimeSpan.FromSeconds(duration);
                        endDate = eventDate.Add(tsDuration);
                        listItem["EndDate"] = endDate;
                    }
                    if (userData.ContainsKey("RecurrenceID"))
                    {
                        DateTime recurrenceID = mList.ParentWeb.RegionalSettings.TimeZone.UTCToLocalTime(Convert.ToDateTime(userData["RecurrenceID"], System.Globalization.DateTimeFormatInfo.InvariantInfo));
                        listItem["RecurrenceID"] = recurrenceID;
                    }

                    if (userData.ContainsKey("UID"))
                    {
                        listItem["UID"] = userData["UID"];
                        mPreviousUID = (Guid)userData["UID"];
                    }
                    else
                    {
                        listItem["UID"] = mPreviousUID;
                    }
                    if (userData.ContainsKey("Location"))
                    {
                        listItem["Location"] = userData["Location"];
                    }
                    if (userData.ContainsKey("RecurrenceData"))
                    {
                        listItem["RecurrenceData"] = userData["RecurrenceData"];
                    }
                    if (userData.ContainsKey("fAllDayEvent"))
                    {
                        listItem["fAllDayEvent"] = userData["fAllDayEvent"];
                    }
                    if (userData.ContainsKey("fRecurrence"))
                    {
                        listItem["fRecurrence"] = userData["fRecurrence"];
                    }
                    if (userData.ContainsKey("RRule"))
                    {
                        listItem["RRule"] = userData["RRule"];
                    }
                    if (userData.ContainsKey("ExRule"))
                    {
                        listItem["ExRule"] = userData["ExRule"];
                    }
                    if (userData.ContainsKey("SuppressUntil"))
                    {
                        listItem["SuppressUntil"] = mList.ParentWeb.RegionalSettings.TimeZone.UTCToLocalTime(Convert.ToDateTime(userData["SuppressUntil"], System.Globalization.DateTimeFormatInfo.InvariantInfo));
                    }
                    if (userData.ContainsKey("IsOrphaned"))
                    {
                        //DOC-67486，在此处设置listItem["IsOrphaned"]=true或者不设置该值，都会导致listItem.Update抛出异常
                        //所以在此处设置listItem["IsOrphaned"] = false，如果是true在之后更新field的时候会更新正确。
                        //listItem["IsOrphaned"] = userData["IsOrphaned"];
                        listItem["IsOrphaned"] = false;
                    }
                    if (userData.ContainsKey("IsException"))
                    {
                        listItem["IsException"] = userData["IsException"];
                    }
                    if (userData.ContainsKey("IsDetached"))
                    {
                        listItem["IsDetached"] = userData["IsDetached"];
                    }
                    if (userData.ContainsKey("Sequence"))
                    {
                        listItem["Sequence"] = userData["Sequence"];
                    }
                    if (userData.ContainsKey("DTStamp"))
                    {
                        listItem["DTStamp"] = mList.ParentWeb.RegionalSettings.TimeZone.UTCToLocalTime(Convert.ToDateTime(userData["DTStamp"], System.Globalization.DateTimeFormatInfo.InvariantInfo));
                    }
                    if (userData.ContainsKey("#tp_InstanceID"))
                    {
                        listItem["InstanceID"] = userData["#tp_InstanceID"];
                    }
                    if (userData.ContainsKey("EventUID"))
                    {
                        ProcessEventUidForMeeting(info, userData);
                        listItem["EventUID"] = userData["EventUID"];
                    }
                    else
                    {
                        //listItem["EventUID"] = null;
                    }
                    if (userData.ContainsKey("Organizer"))
                    {
                        listItem["Organizer"] = info.Extension.PrincipalId;
                    }
                    if (userData.ContainsKey("EventUrl") && userData.ContainsKey("EventUrl#2"))
                    {
                        SPFieldUrlValue tValue = new SPFieldUrlValue();
                        tValue.Description = userData["EventUrl#2"].ToString();
                        tValue.Url = info.Extension.FieldUrlValue;
                        listItem["EventUrl"] = tValue;
                    }
                    //新建一个item时，在update之前set "GUID"，可以将其更新,不是所有的listitem都有这个guid的属性
                    if (userData.ContainsKey("#tp_GUID") && listItem.Fields.ContainsField("GUID"))
                    {
                        listItem["GUID"] = info.tp_Guid;//userData["#tp_GUID"];
                    }
                    //新建一个item时，在update之前用SetIDForMigration设置itemId，可以将itemId更新
                    if (mSite.QueryService.CheckItemIdAvailable(mSite.ID, mList.ID, info.OriginalRowId) && info.NeedChangeItemId)
                    {
                        AveAssemblyUtility.InvokeMethod(listItem, "SetIDForMigration", new Type[] { typeof(int) }, new object[] { info.OriginalRowId });
                        mSite.QueryService.ChangeNextItemId(info.OriginalRowId, mSite.ID, mList.ID);
                    }
                    listItem.ParentList.ParentWeb.Site.WebApplication.FormDigestSettings.Enabled = false;
                    listItem.Update();
                    listItemHasUpdated = true;
                    return listItem;
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.ListItemUpdateError, e);
                }
                return null;

            }

        }

        /// <summary>
        /// 将meetingserials的listItem的EventUID中的ListID装换为目的端的ListID
        /// </summary>
        /// <param name="data"></param>
        private void ProcessEventUidForMeeting(AveListItemInfo info, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.ProcessEventUidForMeeting"))
            {

                if (userData.ContainsKey("EventType") && userData["EventType"].ToString().Equals("1"))
                {
                    try
                    {
                        if (userData.ContainsKey("EventUrl"))
                        {
                            string sourceUrl = userData["EventUrl"].ToString();
                            string webUrl = info.Extension.DestUrl.Substring(0, info.Extension.DestUrl.LastIndexOf('/'));
                            webUrl = webUrl.Substring(0, webUrl.LastIndexOf('/'));
                            using (SPWeb web = mList.ParentWeb.Site.OpenWeb(webUrl))
                            {
                                SPList list = web.GetList(info.Extension.DestUrl);
                                string ListID = list.ID.ToString();
                                if (userData.ContainsKey("EventUID"))
                                {
                                    string EventUID = userData["EventUID"].ToString();
                                    string sourceUID = EventUID.Substring(EventUID.IndexOf('{') + 1, 36);
                                    userData["EventUID"] = EventUID.Replace(sourceUID, ListID);
                                    if (info.FieldsInfo.Fields.ContainsKey("EventUID"))
                                    {
                                        (info.FieldsInfo.Fields["EventUID"] as AveFieldValueInfo).ColValue = userData["EventUID"];
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.ReplaceListIdForMeetingSerialsFailed, e);
                    }
                }

            }

        }

        /// <summary>
        /// 判断这个list是否是ReporttingTemplate,使用web上property以及list的title来判断
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public bool IsReportTemplateList()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.IsReportTemplateList"))
            {

                bool isReportTemplateList = false;
                try
                {
                    SPList list = mList;
                    SPWeb web = m_ParentWeb.Web;
                    if (web.Properties.ContainsKey("_reportinggallerytemplateid"))
                    {
                        string Guid = web.Properties["_reportinggallerytemplateid"];
                        if (web.Properties["_reportinggallerytemplateid"].Equals(list.ID.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            isReportTemplateList = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.IsReportingTemplateFailed, e);
                }
                return isReportTemplateList;

            }

        }
        internal bool IsVariationLabelsList()
        {
            return m_ParentWeb.VariationLabelListId == this.ID;
        }
        public bool IsRelationshipsList()
        {
            return m_ParentWeb.RelationshipsListId == this.ID;
        }
        internal bool IsReportingMetadataList()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.IsReportingMetadataList"))
            {

                bool isReportMetadataList = false;
                try
                {
                    SPList list = mList;
                    SPWeb web = m_ParentWeb.Web;
                    if (web.Properties.ContainsKey("_reportinggallerymetadataid"))
                    {
                        string Guid = web.Properties["_reportinggallerymetadataid"];
                        if (web.Properties["_reportinggallerymetadataid"].Equals(list.ID.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            isReportMetadataList = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.IsReportingMetadataFailed, e);
                }
                return isReportMetadataList;

            }

        }

        public IAveRelatedFieldCollection GetRelatedFields()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.GetRelatedFields"))
            {

                return new AveRelatedFieldCollection(this, mList.GetRelatedFields());

            }

        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property.")]
        private static string GetFileFilter(string filter)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.GetFileFilter"))
            {

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(filter);
                foreach (XmlNode node in doc.GetElementsByTagName("property"))
                {
                    string value = node.Attributes["value"].Value;
                    string name = node.Attributes["name"].Value;
                    if (name.Equals("filterpath", StringComparison.OrdinalIgnoreCase))
                    {
                        return value.Trim('/');
                    }
                }
                return "";

            }

        }

        public Guid Recycle()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.Recycle"))
            {

                return mList.Recycle();

            }

        }

        /// <summary>
        /// Add for Web Database System List
        /// </summary>
        /// <returns></returns>
        public bool IsACCSRVSystemList()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.IsACCSRVSystemList"))
            {

                bool isMSysASOSystemList = false;
                bool isMacroSystemList = false;
                SPWeb web = this.m_ParentWeb.Web;
                SPList list = mList;
                if (web != null && web.WebTemplate.Equals("ACCSRV", StringComparison.OrdinalIgnoreCase))
                {
                    #region IsMSysASOSystemList
                    if (web.AllProperties.ContainsKey("___MSysASOId"))
                    {
                        isMSysASOSystemList = list.ID.Equals(new Guid((string)web.AllProperties["___MSysASOId"]));
                    }
                    else
                    {
                        isMSysASOSystemList = list.Title.Equals("MSysASO", StringComparison.OrdinalIgnoreCase);
                    }
                    #endregion
                    #region IsMacroSystemList
                    if (!isMSysASOSystemList)
                        isMacroSystemList = list.Title.Equals("Macro", StringComparison.OrdinalIgnoreCase);
                    #endregion
                }
                return isMSysASOSystemList || isMacroSystemList;

            }

        }

        [Obsolete]
        public IAveWorkflowAssociation AddWorkflowAssociation(IAveWorkflowAssociation association)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.AddWorkflowAssociation"))
            {

                //return new AveWorkflowAssociation(mList.AddWorkflowAssociation((association as AveWorkflowAssociation).WorkflowAssociation));
                SPWorkflowAssociation tempAssociation = mList.AddWorkflowAssociation((association as AveWorkflowAssociation).WorkflowAssociation);
                return new AveWorkflowAssociation(this.WorkflowAssociations, tempAssociation);

            }

        }

        [Obsolete]
        public void UpdateWorkflowAssociation(IAveWorkflowAssociation workflowAssociation)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.UpdateWorkflowAssociation"))
            {

                mList.UpdateWorkflowAssociation((workflowAssociation as AveWorkflowAssociation).WorkflowAssociation);

            }

        }


        public ulong Flags
        {
            get
            {
                return (ulong)AveAssemblyUtility.GetPropertyValue(mList, "Flags");
            }
        }

        Dictionary<string, Dictionary<string, string>> clientLocationBasedDefaults = null;
        public Dictionary<string, Dictionary<string, string>> ClientLocationBasedDefaults
        {
            get
            {
                if (clientLocationBasedDefaults == null)
                {
                    clientLocationBasedDefaults = new Dictionary<string, Dictionary<string, string>>();
                    IAveFile spFile = this.ParentWeb.GetFile(mList.RootFolder.ServerRelativeUrl + "/Forms/client_LocationBasedDefaults.html");
                    if (spFile != null && spFile.Exists)
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(new UTF8Encoding().GetString(spFile.OpenBinary()));
                        foreach (XmlNode node in doc.DocumentElement.SelectNodes("a"))
                        {
                            var columnValueMapping = new Dictionary<string, string>();
                            foreach (XmlNode field in node.ChildNodes)
                            {
                                if (field.Name.Equals("DefaultValue"))
                                {
                                    columnValueMapping[field.Attributes["FieldName"].Value] = field.InnerText;
                                }
                            }
                            clientLocationBasedDefaults[System.Web.HttpUtility.UrlDecode(node.Attributes["href"].Value)] = columnValueMapping;
                        }
                    }
                    clientLocationBasedDefaults = SortClientLocationBasedDefaults(clientLocationBasedDefaults);
                }
                return clientLocationBasedDefaults;
            }
            set
            {
                this.clientLocationBasedDefaults = value;
            }
        }

        private Dictionary<string, Dictionary<string, string>> SortClientLocationBasedDefaults(Dictionary<string, Dictionary<string, string>> originalDic)
        {
            List<string> keys = new List<string>(originalDic.Keys);
            keys.Sort((left, right) =>
            {
                return right.Length - left.Length;
            });
            var SortedDic = new Dictionary<string, Dictionary<string, string>>();
            foreach (var key in keys)
            {
                SortedDic.Add(string.Format("{0}/", key), originalDic[key]);
            }
            return SortedDic;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (mAlertTemplate != null)
            {
                mAlertTemplate.Dispose();
                mAlertTemplate = null;
            }
            if (mSOIntegrationUtil != null)
            {
                mSOIntegrationUtil.Dispose();
                mSOIntegrationUtil = null;
            }
        }

        #endregion

        public virtual void Reload()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.Reload"))
            {

                if (mList != null)
                {
                    SPList tempList = m_ParentWeb.Web.Lists.GetList(mList.ID, true);
                    //有些情况，当执行某些API时list和web的关系断开，list.update并没有反馈到web，导致web上list是"旧"的。
                    if (mList.Version > tempList.Version)
                    {
                        m_ParentWeb.ReloadWeb();
                        mList = m_ParentWeb.Web.Lists.GetList(mList.ID, true);
                    }
                    else
                    {
                        mList = tempList;
                    }
                    //try
                    //{
                    //    mList.Update();
                    //}
                    //catch (Exception e)
                    //{
                    //    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.ListUpdateError, mList.Title, e);
                    //    this.m_ParentWeb.ReloadWeb();
                    //    mList = (this.m_ParentWeb.Lists[mList.ID] as AveList).List;
                    //}
                    base.Reload(mList);
                    mContentTypes = new AveContentTypeCollection(this, mList.ContentTypes);
                    mFields = new AveFieldCollection(this.ParentWeb as AveWeb, mList.Fields);
                    mLists = this.m_ParentWeb.Lists as AveListCollection;
                    mViews = new AveViewCollection(this, mList.Views);
                    mDefaultView = null;
                    mRootFolder = null;
                    mAuthor = null;
                    clientLocationBasedDefaults = null;
                    mEventReceivers = null;
                }

            }

        }
        public void ReloadFields()
        {
            mFields = new AveFieldCollection(this.ParentWeb as AveWeb, mList.Fields);
        }
        private bool IsWorkflowTask(AveItem item)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.IsWorkflowTask"))
            {

                bool isWorkflowInstance = false;
                if (this.BaseTemplate == AveListTemplateType.Tasks && item != null && item.mSPListItem.ContentType != null)
                {
                    string contentTypeId = item.mSPListItem.ContentTypeId.ToString();
                    if ((!string.IsNullOrEmpty(contentTypeId)) && contentTypeId.StartsWith("0x010801", StringComparison.OrdinalIgnoreCase))
                    {
                        isWorkflowInstance = true;
                    }
                }
                return isWorkflowInstance;

            }

        }

        #region For performance

        int maxListItemRowId = -1;
        private bool? isConnectorList;

        internal int MaxListItemRowId
        {
            get
            {
                //-1代表没有初始化过，GetMaxListItemRowId的默认返回值为0
                if (maxListItemRowId == -1)
                {
                    maxListItemRowId = mSite.QueryService.GetMaxListItemRowId(mSite.ID, Id);
                }
                return maxListItemRowId;
            }
            set { maxListItemRowId = value; }
        }

        #endregion

        public void SetWorkflowsAssociated(bool bWorkflowsAssociated)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.SetWorkflowsAssociated"))
            {

                AveAssemblyUtility.InvokeMethod(mList, "SetWorkflowsAssociated", bWorkflowsAssociated);

            }

        }

        public void UpdateListRssSetting(Dictionary<string, object> updateProp)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.UpdateListRssSetting"))
            {

                if (updateProp.ContainsKey("AllowRss"))
                {
                    this.EnableSyndication = Convert.ToBoolean(updateProp["AllowRss"].ToString());
                    this.Update();
                }
                if (updateProp.ContainsKey("LimitDescriptionLength") && Convert.ToBoolean(updateProp["LimitDescriptionLength"].ToString()))
                    this.RootFolder.Properties["vti_rss_LimitDescriptionLength"] = 1;
                else
                    this.RootFolder.Properties["vti_rss_LimitDescriptionLength"] = 0;

                if (updateProp.ContainsKey("ChannelTitle"))
                {
                    this.RootFolder.Properties["vti_rss_ChannelTitle"] = updateProp["ChannelTitle"].ToString();
                }
                if (updateProp.ContainsKey("ChannelDescription"))
                {
                    this.RootFolder.Properties["vti_rss_ChannelDescription"] = updateProp["ChannelDescription"].ToString();
                }
                if (updateProp.ContainsKey("ChannelImageUrl"))
                {
                    this.RootFolder.Properties["vti_rss_ChannelImageUrl"] = updateProp["ChannelImageUrl"].ToString();
                }
                if (updateProp.ContainsKey("ItemLimit"))
                {
                    this.RootFolder.Properties["vti_rss_ItemLimit"] = updateProp["ItemLimit"].ToString();
                }
                if (updateProp.ContainsKey("DayLimit"))
                {
                    this.RootFolder.Properties["vti_rss_DayLimit"] = updateProp["DayLimit"].ToString();
                }
                if (this.BaseType == AveBaseType.DocumentLibrary)
                {
                    if (updateProp.ContainsKey("DocumentAsEnclosure") && Convert.ToBoolean(updateProp["DocumentAsEnclosure"].ToString()))
                        this.RootFolder.Properties["vti_rss_DocumentAsEnclosure"] = 1;
                    else
                        this.RootFolder.Properties["vti_rss_DocumentAsEnclosure"] = 0;
                    if (updateProp.ContainsKey("DocumentAsLink") && Convert.ToBoolean(updateProp["DocumentAsLink"].ToString()))
                        this.RootFolder.Properties["vti_rss_DocumentAsLink"] = 1;
                    else
                        this.RootFolder.Properties["vti_rss_DocumentAsLink"] = 0;
                }
                this.RootFolder.Update();

            }

        }

        public IAveFolder GetFolder(string serverRelativeUrl)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.GetFolder"))
            {

                return this.ParentWeb.GetFolder(serverRelativeUrl);

            }

        }
        [Obsolete("not use any more")]
        public void GetViews(Dictionary<Guid, List<AveViewInfo>> viewCache)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.GetViews"))
            {

                this.mSite.QueryService.GetViews(viewCache, this.mSite.ID, this.Id, this.DefaultView.ID);

            }

        }

        public bool RequestAccessEnabled
        {
            get
            {
                return mList.RequestAccessEnabled;
            }
            set
            {
                mList.RequestAccessEnabled = value;
            }
        }

        public Collection<IAveSPListItemInfo> GetItemsWithUniquePermissions()
        {
            Collection<IAveSPListItemInfo> listItemInfos = new Collection<IAveSPListItemInfo>();
            foreach (SPListItemInfo info in mList.GetItemsWithUniquePermissions())
            {
                listItemInfos.Add(new AveSPListItemInfo(info));
            }
            return listItemInfos;
        }
        public List<int> GetItemsByColumnValue(string columnDisplayName, string value)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveList.GetItemsByColumnValue"))
            {

                if (Fields.ContainsField(columnDisplayName))
                {
                    List<int> itemRowIds = new List<int>();
                    AveField field = mFields[columnDisplayName] as AveField;
                    if (field.Type == AveFieldType.Text)
                    {
                        itemRowIds = mSite.QueryService.GetItemsByColumnValue(mSite.ID, this.Id, field.ColName, value);
                    }
                    else
                    {
                        itemRowIds = null;
                    }
                    return itemRowIds;
                }
                return null;

            }

        }

        public void CleanListData()
        {
            AveAssemblyUtility.InvokeMethod(SPManager.Instance, "Invalidate", new Type[] { }, null);
            //Todo
        }

        public bool CheckItemIsExist(int rowId)
        {
            bool isExist = false;

            IAveListItem item = null;
            if ((item = this.GetItemById(rowId)) != null)
            {
                isExist = true;
            }

            return isExist;
        }

        public bool CheckItemIsExist(string rowId, Guid itemId, string parentFolderServerRelativeUrl = null)
        {
            return CheckItemIsExist(int.Parse(rowId));
        }

        public void UpdateListCreated(DateTime created)
        {
        }

        public void UpdateListModifyInfo(Dictionary<string, object> modifyInfoDictionary)
        {
            if (mSite.NativeApiPermission != WrapperNativeApiPermission.FullControl)
            {
                logger.Log(AveLogLevel.WARN, "Can not update list basic info because of permission issue. Web Url:{0}. List Title:{1}", this.ParentWeb == null ? "" : this.ParentWeb.Url, this.Title);
                return;
            }
            this.mSite.QueryService.UpdateListInfoByNative(this.mSite.ID, this.ParentWeb.ID, this.ID, modifyInfoDictionary);
        }

        public bool CheckIfHasAlertsOfSpecificConditions(int? itemId, AveEventType eventType, int userId, AveAlertFrequency frequency)
        {
            return this.mSite.QueryService.HasAlertsOfSpecificConditions(this.mSite.ID, this.Id, itemId, eventType, userId, frequency);
        }

        public void RestoreListRatingSetting(AveListSettingInfo info)
        {
            if (info.AllowRatingSetting != null && info.AllowRatingSetting.IsAvailable)//BPOS-D Publishing为null，会抛空引用；
            {
                string experience = string.Empty;
                var publishing = new AvePublishing(mSite);
                bool allowListRatingSetting = info.AllowRatingSetting.Value;
                Guid averageRatings = AveEnv.IsMoss ? publishing.AverageRatings : Guid.Empty;
                Guid ratingsCount = AveEnv.IsMoss ? publishing.RatingsCount : Guid.Empty;
                Guid likesCount = AveEnv.IsMoss ? publishing.LikesCount : Guid.Empty;
                bool destAllow = this.Fields.Contains(averageRatings) && this.Fields.Contains(ratingsCount) && this.Fields.Contains(likesCount) && !string.IsNullOrEmpty(mList.RootFolder.Properties["Ratings_VotingExperience"] as string);
                ProcessListRattingSetting(allowListRatingSetting, destAllow, publishing, info);
            }
            #region unused now
            //if (info.AllowRatingSetting != null && info.AllowRatingSetting.IsAvailable)
            //{
            //    Type type = AveAssemblyUtility.GetType("Microsoft.SharePoint.Portal, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c", "Microsoft.SharePoint.Portal.ReputationHelper");

            //    if (info.AllowRatingSetting.Value)
            //    {
            //        string experience = AveAssemblyUtility.InvokeStaticMethod(type, "GetExperience", new object[] { mList, false }).ToString();
            //        string newExperience = Enum.GetName(typeof(AveRatingSettingType), info.RatingSettingType.Value);
            //        if (!newExperience.Equals(experience, StringComparison.OrdinalIgnoreCase))
            //        {
            //            if (!string.IsNullOrEmpty(experience))
            //            {
            //                AveAssemblyUtility.InvokeStaticMethod(type, "SwitchReputation", new object[] { mList, newExperience, experience });
            //            }
            //            else
            //            {
            //                AveAssemblyUtility.InvokeStaticMethod(type, "EnableReputation", new object[] { mList, experience });
            //            }
            //            mList.Update();
            //        }
            //    }
            //    else
            //    {
            //        AveAssemblyUtility.InvokeStaticMethod(type, "DisableReputation", new object[] { mList });
            //        mList.Update();
            //    }
            //}
            #endregion
        }
        /// <summary>
        /// add for process list ratting setting
        /// </summary>
        /// <param name="sourceEnable"></param>
        /// <param name="destEnable"></param>
        public void ProcessListRattingSetting(bool sourceEnable, bool destEnable, AvePublishing publishing, AveListSettingInfo info)
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
                            IAveFieldCollection fields = this.Fields;
                            IAveFieldCollection availableFields = this.ParentWeb.AvailableFields;

                            Guid ratings = (AveEnv.IsSharePoint2016 || AveEnv.IsSharePoint2013 || AveEnv.IsSharePoint2010 || AveEnv.IsSharePoint2007) ? publishing.Ratings : Guid.Empty;
                            Guid ratingsCount = (AveEnv.IsSharePoint2016 || AveEnv.IsSharePoint2013 || AveEnv.IsSharePoint2010 || AveEnv.IsSharePoint2007) ? publishing.RatingsCount : Guid.Empty;
                            Guid likesCount = (AveEnv.IsSharePoint2016 || AveEnv.IsSharePoint2013 || AveEnv.IsSharePoint2010 || AveEnv.IsSharePoint2007) ? publishing.LikesCount : Guid.Empty;

                            Guid averageRatings = (AveEnv.IsSharePoint2016 || AveEnv.IsSharePoint2013 || AveEnv.IsSharePoint2010 || AveEnv.IsSharePoint2007) ? publishing.AverageRatings : Guid.Empty;
                            Guid likedBy = (AveEnv.IsSharePoint2016 || AveEnv.IsSharePoint2013 || AveEnv.IsSharePoint2010 || AveEnv.IsSharePoint2007) ? publishing.LikedBy : Guid.Empty;
                            Guid ratedBy = (AveEnv.IsSharePoint2016 || AveEnv.IsSharePoint2013 || AveEnv.IsSharePoint2010 || AveEnv.IsSharePoint2007) ? publishing.RatedBy : Guid.Empty;

                            if (!fields.Contains(ratings) && availableFields.Contains(ratings))
                            {
                                IAveField field1 = availableFields[ratings];
                                this.Fields.AddFieldAsXml(field1.SchemaXmlWithResourceTokens, false, AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddToNoContentType);
                            }
                            if (!fields.Contains(ratingsCount) && availableFields.Contains(ratingsCount))
                            {
                                IAveField field2 = availableFields[ratingsCount];
                                this.Fields.AddFieldAsXml(field2.SchemaXmlWithResourceTokens, false, AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddToNoContentType);
                            }
                            if (!fields.Contains(likesCount) && availableFields.Contains(likesCount))
                            {
                                IAveField field3 = availableFields[likesCount];
                                this.Fields.AddFieldAsXml(field3.SchemaXmlWithResourceTokens, false, AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddToNoContentType);
                            }

                            if (!fields.Contains(averageRatings))
                            {
                                IAveField field4 = availableFields[averageRatings];
                                this.Fields.AddFieldAsXml(field4.SchemaXmlWithResourceTokens, false, AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddToNoContentType);
                            }
                            if (!fields.Contains(likedBy))
                            {
                                IAveField field5 = availableFields[likedBy];
                                this.Fields.AddFieldAsXml(field5.SchemaXmlWithResourceTokens, false, AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddToNoContentType);
                            }
                            if (!fields.Contains(ratedBy))
                            {
                                IAveField field6 = availableFields[ratedBy];
                                this.Fields.AddFieldAsXml(field6.SchemaXmlWithResourceTokens, false, AveAddFieldOptions.AddFieldInternalNameHint | AveAddFieldOptions.AddToNoContentType);
                            }

                            mList.ParentWeb.AllowUnsafeUpdates = true;

                            string experience = (info.RatingSettingType != null) ? Enum.GetName(typeof(AveRatingSettingType), info.RatingSettingType.Value) : string.Empty;
                            //10 TO 13, 10side has no "Experience", but if enable rating, we must set a default value, if not, the enable rating option will not effect.
                            if (string.IsNullOrEmpty(experience))
                            {
                                mList.RootFolder.Properties["Ratings_VotingExperience"] = Enum.GetName(typeof(AveRatingSettingType), AveRatingSettingType.Ratings);
                            }
                            else
                            {
                                mList.RootFolder.Properties["Ratings_VotingExperience"] = experience;
                            }
                            mList.RootFolder.Update();
                            mList.Update();
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
                    //reportor.AddDetail(new AveWrapperReportDto(mSPList.Title, mSPList.Title, AveReportObjectType.ListRattingSetting, AveStatus.Failed, string.Format("Process List Rating setting Error.\n error message:{0}", e.Message)));
                    logger.Log(AveLogLevel.WARN, string.Format("Process List Rating Setting Error.\n Error message: {0}", e));
                    //mLog.Warn("Process List Rating setting Error. Error:{0}", e.ToString());
                }

            }

        }

        #region add for SP2013
        public int SearchVersion
        {
            get { return (int)AveAssemblyUtility.GetPropertyValue(mList, "SearchVersion"); }
            set { AveAssemblyUtility.SetPropertyValue(mList, "SearchVersion", value); }
        }

        public IAveInformationRightsManagementSettings InformationRightsManagementSettings
        {
            get
            {
                return new AveInformationRightsManagementSettings(mList.InformationRightsManagementSettings);
            }
        }
        #endregion

        #region Add to operate Change Log

        public IAveChangeCollection GetChanges()
        {
            return new AveChangeCollection(mList.GetChanges());
        }

        public IAveChangeCollection GetChanges(IAveChangeQuery query)
        {
            return new AveChangeCollection(mList.GetChanges((query as AveChangeQuery).ChangeQuery));
        }

        public IAveChangeCollection GetChanges(IAveChangeToken changeToken)
        {
            return new AveChangeCollection(mList.GetChanges((changeToken as AveChangeToken).ChangeToken));
        }

        public IAveChangeCollection GetChanges(IAveChangeToken changeToken, IAveChangeToken changeTokenEnd)
        {
            SPChangeToken ct1 = (changeToken as AveChangeToken).ChangeToken;
            SPChangeToken ct2 = (changeTokenEnd as AveChangeToken).ChangeToken;
            return new AveChangeCollection(mList.GetChanges(ct1, ct2));
        }

        #endregion

        #region User Resource
        public IAveUserResource TitleResource
        {
            get { return new AveUserResource(List.TitleResource); }
        }

        public IAveUserResource DescriptionResource
        {
            get { return new AveUserResource(List.DescriptionResource); }
        }
        #endregion

        public bool AllowEveryoneViewItems
        {
            get
            {
                return mList.AllowEveryoneViewItems;
            }
            set
            {
                mList.AllowEveryoneViewItems = value;
            }
        }

        public AveBrowserFileHandling BrowserFileHandling
        {
            get
            {
                return (AveBrowserFileHandling)mList.BrowserFileHandling;
            }
            set
            {
                mList.BrowserFileHandling = (SPBrowserFileHandling)value;
            }
        }

        public AveCalculationOptions CalculationOptions
        {
            get
            {
                return (AveCalculationOptions)mList.CalculationOptions;
            }
            set
            {
                mList.CalculationOptions = (SPCalculationOptions)value;
            }
        }

        public bool CanReceiveEmail
        {
            get { return mList.CanReceiveEmail; }
        }

        public AveBasePermissions EffectiveBasePermissions
        {
            get { return (AveBasePermissions)mList.EffectiveBasePermissions; }
        }

        public AveBasePermissions EffectiveFolderPermissions
        {
            get { return (AveBasePermissions)mList.EffectiveFolderPermissions; }
        }

        public bool ForceDefaultContentType
        {
            get
            {
                return mList.ForceDefaultContentType;
            }
            set
            {
                mList.ForceDefaultContentType = value;
            }
        }

        public string MobileDefaultDisplayFormUrl
        {
            get { return mList.MobileDefaultDisplayFormUrl; }
        }

        public string MobileDefaultEditFormUrl
        {
            get { return mList.MobileDefaultEditFormUrl; }
        }

        public string MobileDefaultNewFormUrl
        {
            get { return mList.MobileDefaultNewFormUrl; }
        }

        public IAveView MobileDefaultView
        {
            get
            {
                if (mList.MobileDefaultView != null)
                {
                    return new AveView(this, mList.MobileDefaultView);
                }

                return null;
            }
        }

        public string MobileDefaultViewUrl
        {
            get { return mList.MobileDefaultViewUrl; }
        }

        public bool UseFormsForDisplay
        {
            get
            {
                return mList.UseFormsForDisplay;
            }
            set
            {
                mList.UseFormsForDisplay = value;
            }
        }

        public bool RestrictedTemplateList
        {
            get { return mList.RestrictedTemplateList; }
        }


        public bool IsExceedListViewLookupThreshold
        {
            get { return false; }
        }

        public bool EnableManagedIndexes
        {
            get
            {
                return mList.EnableManagedIndexes;
            }

            set
            {
                mList.EnableManagedIndexes = value;
            }
        }

        public IAveUserCustomActionCollection UserCustomActions
        {
            get
            {
                return new AveUserCustomActionCollection(List.UserCustomActions);
            }
        }

        public AveListExperience ListExperienceOptions
        {
            get
            {
                return (AveListExperience)this.List.ListExperienceOptions;
            }

            set
            {
                this.List.ListExperienceOptions = (SPListExperience)value;
            }
        }

        public bool CrawlNonDefaultViews {
            get { return mList.CrawlNonDefaultViews; }
            set { mList.CrawlNonDefaultViews = value; }
        }


        public AveComplianceTagInfo ComplianceTag
        {
            get { return null; }
            set {  }
        }

        public void PublicSharepointInfoPathList(IAveFile templateFile, int lcid, string listId, string contentTypeId)
        {
            // local do nothing
        }

        public void SaveNintexForm(string formXml, string contentTypeId)
        {
            throw new NotSupportedException();
        }
        public void PublishNintexForm(string contentTypeId)
        {
            throw new NotSupportedException();
        }

        public Stream ExportNintexForm(string contentTypeId)
        {
            throw new NotImplementedException();
        }

        public WorkflowStartOptionCache BackupWorkflowStartOption(string url, Guid webId, Guid listId)
        {
            return null;
        }

        public void RestoreWOrkflowStartOption(string url, Guid webId, Guid listId, WorkflowStartOptionCache cache)
        {

        }

        public IAveListItemCollection GetItemsForRecords(AveCamlQuery camlQuery)
        {
            throw new NotImplementedException();
        }
    }
}
