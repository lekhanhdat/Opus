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
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.Client;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.Wrapper.Common;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.IO;
using AvePoint.Wrapper.Resource.Common;

namespace AvePoint.ObjectModel.Common
{
    [AveCodeReviewAttribute("2012/03/09", "Navy.Li@avepoint.com", "Bingkun.Wang@AvePoint.com",
        new[]
        {
            CodeReviewConstants.CHECK_LIST_ID_CO_1,
            CodeReviewConstants.CHECK_LIST_ID_CO_6,
            CodeReviewConstants.CHECK_LIST_ID_FA_4
        }, null, true)]
    internal class AveList : AveSecurableObject, IAveList
    {
        private static readonly AveLogger mLogger = AveLogger.GetInstance(typeof(AveList));

        public static List<string> IgnoreFields = new List<string>();
        public static int DefaultLCID = 1033;

        #region add to keep item's LastModifiedTime property

        private readonly object privateLock = new object();
        private readonly object privateLockWorkflowAssociations = new object();
        private readonly object privateLockContentTypes = new object();
        private readonly object privateLockFields = new object();
        private int listSettingFlag;
        private List<AveContentType> mChangedContentTypes;
        private bool mModifiedFieldChanged = false;
        private bool mModifiedFieldHidden;
        private Dictionary<AveContentType, bool> mModifiedFieldLinkHidden;
        private Dictionary<AveContentType, bool> mModifiedFieldLinkReadOnly;
        private bool mModifiedFieldReadOnly;

        #endregion

        private readonly bool isOneDriveLibrary;
        private readonly object loadListItemGuidAndRowIdMappingLock = new object();
        private readonly object mLoadFieldLock = new object();
        private readonly AveWeb mParentWeb;
        private AveUserResource mTitleResource;
        private AveUserResource mDescriptionResource;
        private object privateLockTitleResource = new object();
        private object privateLockDescriptionResource = new object();
        private bool mIsNeedLoadFieldsInitialized;
        private Dictionary<string, int> mItemIdMapping;
        private Dictionary<string, int> mItemUniqueIdAndRowIdMapping;
        private Dictionary<string, string> mNeedLoadFields;
        private IAveRequest mRequest;
        private bool? mIsExceedListViewLookupThreshold;
        private object mIsExceedListViewLookupThresholdLock = new object();
        private static HashSet<Guid> BuiltInLookupColumn = new HashSet<Guid>();
        private bool? isRssViewExist;
        private AveComplianceTagInfo mComplianceTag;
        private bool RssViewExist
        {
            get
            {
                if (!isRssViewExist.HasValue)
                {
                    isRssViewExist = AveSPListUtility.IsViewExist(this, "RssView");
                }
                return isRssViewExist.Value;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint Property")]
        static AveList()
        {
            //IgnoreFields.Add("_ModerationComments");
            //IgnoreFields.Add("Body");
            //IgnoreFields.Add("PostCategory");
            //IgnoreFields.Add("PublishedDate");
            //IgnoreFields.Add("Keywords");
            IgnoreFields.Add("ImageWidth");
            //IgnoreFields.Add("_Comments");
            //IgnoreFields.Add("AlternateThumbnailUrl");
            //IgnoreFields.Add("wic_System_Copyright");
            IgnoreFields.Add("MediaLengthInSeconds");
            IgnoreFields.Add("Modified");
            //IgnoreFields.Add("Editor");
            IgnoreFields.Add("_CopySource");
            IgnoreFields.Add("CheckoutUser");
            //IgnoreFields.Add("HTML_x0020_File_x0020_Type");
            IgnoreFields.Add("_SourceUrl");
            IgnoreFields.Add("_SharedFileIndex");
            IgnoreFields.Add("TemplateUrl");
            IgnoreFields.Add("xd_ProgID");
            IgnoreFields.Add("xd_Signature");
            IgnoreFields.Add("_HasCopyDestinations");
            IgnoreFields.Add("owshiddenversion");
            IgnoreFields.Add("InstanceID");
            IgnoreFields.Add("Order");
            IgnoreFields.Add("WorkflowVersion");
            //IgnoreFields.Add("WorkflowInstanceID");

            BuiltInLookupColumn.Add(new Guid("1982e408-0f94-4149-8349-16f301d89134"));  // InternalName:PreviouslyAssignedTo
            BuiltInLookupColumn.Add(new Guid("3881510a-4e4a-4ee8-b102-8ee8e2d0dd4b"));  // InternalName:CheckoutUser
            BuiltInLookupColumn.Add(new Guid("94f89715-e097-4e8b-ba79-ea02aa8b7adb"));  // InternalName:FileRef
            BuiltInLookupColumn.Add(new Guid("7111aa1b-e7ae-4b69-acaf-db669b76e03a"));  // InternalName:V4CallTo
            BuiltInLookupColumn.Add(new Guid("c5c4b81c-f1d9-4b43-a6a2-090df32ebb68"));  // InternalName:ProgId
            BuiltInLookupColumn.Add(new Guid("960ff01f-2b6d-4f1b-9c3f-e19ad8927341"));  // InternalName:FolderChildCount
            BuiltInLookupColumn.Add(new Guid("dddd2420-b270-4735-93b5-92b713d0944d"));  // InternalName:ScopeId
            BuiltInLookupColumn.Add(new Guid("6bfaba20-36bf-44b5-a1b2-eb6346d49716"));  // InternalName:AppAuthor
            BuiltInLookupColumn.Add(new Guid("875fab27-6e95-463b-a4a6-82544f1027fb"));  // InternalName:RelatedIssues
            BuiltInLookupColumn.Add(new Guid("53101f38-dd2e-458c-b245-0c236cc13d1a"));  // InternalName:AssignedTo
            BuiltInLookupColumn.Add(new Guid("774eab3a-855f-4a34-99da-69dc21043bec"));  // InternalName:ParentLeafName
            BuiltInLookupColumn.Add(new Guid("38bea83b-350a-1a6e-f34a-93a6af31338b"));  // InternalName:PostCategory
            BuiltInLookupColumn.Add(new Guid("30bb605f-5bae-48fe-b4e3-1f81d9772af9"));  // InternalName:FSObjType
            BuiltInLookupColumn.Add(new Guid("58014f77-5463-437b-ab67-eec79532da67"));  // InternalName:_CheckinComment
            BuiltInLookupColumn.Add(new Guid("b4fa187b-eb65-478e-8bc6-93b0da320f03"));  // InternalName:ResolvedBy
            BuiltInLookupColumn.Add(new Guid("b824e17e-a1b3-426e-aecf-f0184d900485"));  // InternalName:ItemChildCount
            BuiltInLookupColumn.Add(new Guid("7f15088c-1448-41c7-a125-18a3a90ce543"));  // InternalName:LastReplyBy
            BuiltInLookupColumn.Add(new Guid("50d8f08c-8e99-4948-97bf-2be41fa34a0d"));  // InternalName:TaskGroup
            BuiltInLookupColumn.Add(new Guid("687c7f94-686a-42d3-9b67-2782eac4b4f8"));  // InternalName:MetaInfo
            BuiltInLookupColumn.Add(new Guid("c3a92d97-2b77-4a25-9698-3ab54874bc6f"));  // InternalName:Predecessors
            BuiltInLookupColumn.Add(new Guid("f0218b98-d0d6-4fc1-b15b-aabeb89f32a9"));  // InternalName:DiscussionTitleLookup
            BuiltInLookupColumn.Add(new Guid("e0f298a5-7e3e-4895-9ff8-90d88ec4526d"));  // InternalName:V4SendTo
            BuiltInLookupColumn.Add(new Guid("8137f7ad-9170-4c1d-a17b-4ca7f557bc88"));  // InternalName:ParticipantsPicker
            BuiltInLookupColumn.Add(new Guid("fd447db5-3908-4b47-8f8c-a5895ed0aa6a"));  // InternalName:ParentID
            BuiltInLookupColumn.Add(new Guid("4a389cb9-54dd-4287-a71a-90ff362028bc"));  // InternalName:VirusStatus
            BuiltInLookupColumn.Add(new Guid("078b9dba-eb8c-4ec5-bfdd-8d220a3fcc5d"));  // InternalName:MyEditor
            BuiltInLookupColumn.Add(new Guid("8fca95c0-9b7d-456f-8dae-b41ee2728b85"));  // InternalName:File_x0020_Size
            BuiltInLookupColumn.Add(new Guid("173f76c8-aebd-446a-9bc9-769a2bd2c18f"));  // InternalName:Last_x0020_Modified
            BuiltInLookupColumn.Add(new Guid("ff90fecb-7f46-44f5-9698-db44a81b2a8b"));  // InternalName:ParentItemEditor
            BuiltInLookupColumn.Add(new Guid("4b7403de-8d94-43e8-9f0f-137a3e298126"));  // InternalName:UniqueId
            BuiltInLookupColumn.Add(new Guid("a4e7b3e1-1b0a-4ffa-8426-c94d4cb8cc57"));  // InternalName:Facilities
            BuiltInLookupColumn.Add(new Guid("e08400f3-c779-4ed2-a18c-ab7f34caa318"));  // InternalName:AppEditor
            BuiltInLookupColumn.Add(new Guid("56605df6-8fa1-47e4-a04c-5b384d59609f"));  // InternalName:FileDirRef
            BuiltInLookupColumn.Add(new Guid("423874f8-c300-4bfb-b7a1-42e2159e3b19"));  // InternalName:SortBehavior
            BuiltInLookupColumn.Add(new Guid("bc1a8efb-0f4c-49f8-a38f-7fe22af3d3e0"));  // InternalName:ParentVersionString
            BuiltInLookupColumn.Add(new Guid("211a8cfc-93b7-4173-9254-0bfe2d1643da"));  // InternalName:UserName
            BuiltInLookupColumn.Add(new Guid("8ffccefe-998b-4896-a6df-32d566f69141"));  // InternalName:ShortestThreadIndexIdLookup
            BuiltInLookupColumn.Add(new Guid("998b5cff-4a35-47a7-92f3-3914aa6aa4a2"));  // InternalName:Created_x0020_Date
            BuiltInLookupColumn.Add(new Guid("4d64b067-08c3-43dc-a87b-8b8e01673313"));  // InternalName:RatedBy

            BuiltInLookupColumn.Add(new Guid("94f89715-e097-4e8b-ba79-ea02aa8b7adb")); //URL Path
            BuiltInLookupColumn.Add(new Guid("56605df6-8fa1-47e4-a04c-5b384d59609f")); // Path
            BuiltInLookupColumn.Add(new Guid("173f76c8-aebd-446a-9bc9-769a2bd2c18f")); //modified
            BuiltInLookupColumn.Add(new Guid("998b5cff-4a35-47a7-92f3-3914aa6aa4a2")); //created
            BuiltInLookupColumn.Add(new Guid("8fca95c0-9b7d-456f-8dae-b41ee2728b85")); //file Size
            BuiltInLookupColumn.Add(new Guid("30bb605f-5bae-48fe-b4e3-1f81d9772af9")); //item Type
            BuiltInLookupColumn.Add(new Guid("423874f8-c300-4bfb-b7a1-42e2159e3b19")); //sort type
            BuiltInLookupColumn.Add(new Guid("4b7403de-8d94-43e8-9f0f-137a3e298126")); // unique id
            BuiltInLookupColumn.Add(new Guid("c5c4b81c-f1d9-4b43-a6a2-090df32ebb68")); // progid
            BuiltInLookupColumn.Add(new Guid("dddd2420-b270-4735-93b5-92b713d0944d")); // scope id
            BuiltInLookupColumn.Add(new Guid("4a389cb9-54dd-4287-a71a-90ff362028bc")); // virus status
            BuiltInLookupColumn.Add(new Guid("687c7f94-686a-42d3-9b67-2782eac4b4f8")); // property bag
            BuiltInLookupColumn.Add(new Guid("94f89715-e097-4e8b-ba79-ea02aa8b7adb")); //URL Path
            BuiltInLookupColumn.Add(new Guid("56605df6-8fa1-47e4-a04c-5b384d59609f")); // Path
            BuiltInLookupColumn.Add(new Guid("173f76c8-aebd-446a-9bc9-769a2bd2c18f")); //modified
            BuiltInLookupColumn.Add(new Guid("998b5cff-4a35-47a7-92f3-3914aa6aa4a2")); //created
            BuiltInLookupColumn.Add(new Guid("8fca95c0-9b7d-456f-8dae-b41ee2728b85")); //file Size
            BuiltInLookupColumn.Add(new Guid("30bb605f-5bae-48fe-b4e3-1f81d9772af9")); //item Type
            BuiltInLookupColumn.Add(new Guid("423874f8-c300-4bfb-b7a1-42e2159e3b19")); //sort type
            BuiltInLookupColumn.Add(new Guid("4b7403de-8d94-43e8-9f0f-137a3e298126")); // unique id
            BuiltInLookupColumn.Add(new Guid("c5c4b81c-f1d9-4b43-a6a2-090df32ebb68")); // progid
            BuiltInLookupColumn.Add(new Guid("dddd2420-b270-4735-93b5-92b713d0944d")); // scope id
            BuiltInLookupColumn.Add(new Guid("4a389cb9-54dd-4287-a71a-90ff362028bc")); // virus status
            BuiltInLookupColumn.Add(new Guid("687c7f94-686a-42d3-9b67-2782eac4b4f8")); // property bag
            BuiltInLookupColumn.Add(new Guid("94f89715-e097-4e8b-ba79-ea02aa8b7adb")); //URL Path
            BuiltInLookupColumn.Add(new Guid("56605df6-8fa1-47e4-a04c-5b384d59609f")); // Path
            BuiltInLookupColumn.Add(new Guid("173f76c8-aebd-446a-9bc9-769a2bd2c18f")); //modified
            BuiltInLookupColumn.Add(new Guid("998b5cff-4a35-47a7-92f3-3914aa6aa4a2")); //created
            BuiltInLookupColumn.Add(new Guid("8fca95c0-9b7d-456f-8dae-b41ee2728b85")); //file Size
            BuiltInLookupColumn.Add(new Guid("30bb605f-5bae-48fe-b4e3-1f81d9772af9")); //item Type
            BuiltInLookupColumn.Add(new Guid("423874f8-c300-4bfb-b7a1-42e2159e3b19")); //sort type
            BuiltInLookupColumn.Add(new Guid("4b7403de-8d94-43e8-9f0f-137a3e298126")); // unique id
            BuiltInLookupColumn.Add(new Guid("c5c4b81c-f1d9-4b43-a6a2-090df32ebb68")); // progid
            BuiltInLookupColumn.Add(new Guid("dddd2420-b270-4735-93b5-92b713d0944d")); // scope id
            BuiltInLookupColumn.Add(new Guid("4a389cb9-54dd-4287-a71a-90ff362028bc")); // virus status
            BuiltInLookupColumn.Add(new Guid("687c7f94-686a-42d3-9b67-2782eac4b4f8")); // property bag

            BuiltInLookupColumn.Add(new Guid("a7b731a3-1df1-4d74-a5c6-e2efba617ae2")); // CheckedOutUserId
            BuiltInLookupColumn.Add(new Guid("cfaabd0f-bdbd-4bc2-b375-1e779e2cad08")); // IsCheckedoutToLocal
            BuiltInLookupColumn.Add(new Guid("6d2c4fde-3605-428e-a236-ce5f3dc2b4d4")); // SyncClientId
            BuiltInLookupColumn.Add(new Guid("9d4adc35-7cc8-498c-8424-ee5fd541e43a")); // CheckedOutTitle
            BuiltInLookupColumn.Add(new Guid("8e69e8e8-df8a-45dc-858a-1b806dde24c0")); // DocConcurrencyNumber
            BuiltInLookupColumn.Add(new Guid("3b653cee-df6b-4cd4-b66d-ad5ce875b25e")); // ParentUniqueId
            BuiltInLookupColumn.Add(new Guid("692b7133-d1d1-4a01-b604-2987724f86c3")); // StreamHash
            BuiltInLookupColumn.Add(new Guid("f3b0adf9-c1a2-4b02-920d-943fba4b3611")); // TaxCatchAll 
            BuiltInLookupColumn.Add(new Guid("1df5e554-ec7e-46a6-901d-d85a3881cb18")); // Author 
            BuiltInLookupColumn.Add(new Guid("d31655d1-1d5b-4511-95a1-7a09e9b75bf2")); // Editor 
            BuiltInLookupColumn.Add(new Guid("786099e5-d20a-4232-86e5-cfc3d6face96")); // Restricted 
            BuiltInLookupColumn.Add(new Guid("14ee99cd-bed9-474a-bf99-8f753fbad6b4")); // OriginatorId 
            BuiltInLookupColumn.Add(new Guid("0b16648a-daff-47d4-9fda-c6038b75ed27")); // NoExecute 
            BuiltInLookupColumn.Add(new Guid("d48268e5-c65d-486c-bbf1-874cf986d7d3")); // ContentVersion 
            BuiltInLookupColumn.Add(new Guid("ccc1037f-f65e-434a-868e-8c98af31fe29")); // _ComplianceFlags 
            BuiltInLookupColumn.Add(new Guid("d4b6480a-4bed-4094-9a52-30181ea38f1d")); // _ComplianceTag 
            BuiltInLookupColumn.Add(new Guid("92be610e-ddbb-49f4-b3b1-5c2bc768df8f")); // _ComplianceTagWrittenTime 
            BuiltInLookupColumn.Add(new Guid("418d7676-2d6f-42cf-a16a-e43d2971252a")); // _ComplianceTagUserId 
            BuiltInLookupColumn.Add(new Guid("4df6bfaf-f887-424e-8ea3-fd050113e7a9")); // SMTotalSize 
            BuiltInLookupColumn.Add(new Guid("d340fca5-f503-4baa-bae9-90f1447ebff6")); // SMLastModifiedDate 
            BuiltInLookupColumn.Add(new Guid("1faa4902-9115-44b9-bba7-791441ca1d6f")); // SMTotalFileStreamSize 
            BuiltInLookupColumn.Add(new Guid("a261b12a-8ca2-47fa-a117-05861d637c7e")); // SMTotalFileCount 
        }

        public AveList(IAveRequest request, AveWeb web, Dictionary<string, object> listProp)
            : base(request)
        {
            SearchVersion = 0;
            mRequest = request;
            mParentWeb = web;
            listProp["ParentWeb"] = web;
            base.DataCache.AddPropertyies(listProp);
            IAveFolder folder = RootFolder;
            isOneDriveLibrary = TemplateFeatureId == new Guid("e9c0ff81-d821-4771-8b4c-246aa7e5e9eb");
        }

        internal IAveRequest Request
        {
            get { return mRequest; }
        }

        public List<string> NeedSetNullFields { get; set; }

        public bool AllowEveryoneViewItems { get; set; }

        public AveBrowserFileHandling BrowserFileHandling { get; set; }

        public AveCalculationOptions CalculationOptions { get; set; }

        public bool CanReceiveEmail
        {
            get { return false; }
        }

        public AveBasePermissions EffectiveBasePermissions
        {
            get { return base.DataCache.GetProperty<AveBasePermissions>("EffectiveBasePermissions"); }
        }

        public AveBasePermissions EffectiveFolderPermissions
        {
            get { return base.DataCache.GetProperty<AveBasePermissions>("EffectiveFolderPermissions"); }
        }

        public bool ForceDefaultContentType { get; set; }

        public string MobileDefaultDisplayFormUrl
        {
            get { return null; }
        }

        public string MobileDefaultEditFormUrl
        {
            get { return null; }
        }

        public string MobileDefaultNewFormUrl
        {
            get { return null; }
        }

        public IAveView MobileDefaultView
        {
            get { return null; }
        }

        public string MobileDefaultViewUrl
        {
            get { return null; }
        }

        public bool UseFormsForDisplay { get; set; }

        public bool RestrictedTemplateList
        {
            get { return false; }
        }

        public bool? IsConnectorList { get; set; }

        public bool IsOneDriveLibrary
        {
            get { return isOneDriveLibrary; }
        }

        public IAveListDataSource DataSource
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("AveDataSource"))
                {
                    Dictionary<string, object> ds = base.DataCache.GetProperty<Dictionary<string, object>>("DataSource" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveListDataSource dataSource = new AveListDataSource(ds);
                    base.DataCache.PropertiesCache["AveDataSource"] = dataSource;
                    return dataSource;
                }
                return base.DataCache.GetProperty<IAveListDataSource>("AveDataSource");
            }
        }

        public IAveListItemCollection Folders
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Folders"))
                {
                    AveCamlQuery query = AveCamlQuery.CreateAllFoldersQuery();
                    IAveListItemCollection listItemsCollection = null;
                    if (BaseTemplate != AveListTemplateType.ExternalList)
                    {
                        listItemsCollection = GetItems(query);
                    }
                    base.DataCache.PropertiesCache["Folders"] = listItemsCollection;
                    return listItemsCollection;
                }
                return base.DataCache.GetProperty<IAveListItemCollection>("Folders");
            }
        }

        public IAveListCollection Lists
        {
            get { return ParentWeb.Lists; }
        }

        public bool ExcludeFromTemplate
        {
            get { return base.DataCache.GetProperty<bool>("ExcludeFromTemplate"); }
        }

        public bool IsThrottled
        {
            get { return base.DataCache.GetProperty<bool>("IsThrottled"); }
        }

        public bool Ordered
        {
            get { return base.DataCache.GetProperty<bool>("Ordered"); }
            set
            {
                if (!Ordered.Equals(value))
                {
                    base.DataCache.AddChangedProperty("Ordered", value);
                }
            }
        }

        public bool ShowUser
        {
            get { return base.DataCache.GetProperty<bool>("ShowUser"); }
            set { base.DataCache.AddChangedProperty("ShowUser", value); }
        }

        public bool IsSchedulingEventOnList()
        {
            return false;
        }

        public IAveListItem AddItem()
        {
            throw new NotImplementedException();
        }
        [Obsolete("not use any more")]
        public void GetViews(ref Dictionary<string, List<AveViewInfo>> viewCache)
        {
            viewCache.Clear();
            foreach (IAveView view in Views)
            {
                string url = view.ServerRelativeUrl.Trim('/');
                if (!viewCache.ContainsKey(url))
                {
                    viewCache.Add(url, new List<AveViewInfo>());
                }
                List<AveViewInfo> views = viewCache[url];
                AveViewInfo viewInfo = new AveViewInfo();
                viewInfo.Id = view.ID;
                viewInfo.Title = view.Title;
                viewInfo.IsDefaultView = view.DefaultView;
                viewInfo.IsPersonal = view.PersonalView;
                viewInfo.ViewType = AveViewInfo.GetViewType(view.Type);
                viewInfo.ListViewXml = view.ListViewXml;
                views.Add(viewInfo);
            }
        }

        public IAveView GetView(Guid viewGuid)
        {
            if (viewGuid != Guid.Empty)
            {
                return this.Views[viewGuid];
            }
            return null;
        }

        public AveListSettingInfo GetListSettings()
        {
            AveListSettingInfo listSettingInfo = new AveListSettingInfo();
            GetListProperties();
            if (base.DataCache.IsPropertyAvailable("Title"))
            {
                listSettingInfo.Title = Title;
            }
            if (base.DataCache.IsPropertyAvailable("Created"))
            {
                listSettingInfo.Created = Created;
            }
            if (base.DataCache.IsPropertyAvailable("Description"))
            {
                listSettingInfo.Description = Description;
            }
            listSettingInfo.RootFolderInfo = new AveListRootFolderInfo();
            listSettingInfo.RootFolderInfo.Value.MetaInfoDic = RootFolder.Properties;
            listSettingInfo.RootFolderInfo.Value.WelcomePageUrl = RootFolder.WelcomePage;
            if (RssViewExist)
            {
                IAveView rssView = Views["RssView"];
                listSettingInfo.RssViewField = rssView.ViewFields.SchemaXml;
            }
            else
            {
                listSettingInfo.RssViewField = "";
            }
            if (base.DataCache.IsPropertyAvailable("MajorWithMinorVersionsLimit"))
            {
                listSettingInfo.MaxMajorwithMinorVersionCount = MajorWithMinorVersionsLimit;
            }
            if (base.DataCache.IsPropertyAvailable("MajorVersionLimit"))
            {
                listSettingInfo.MaxMajorVersionCount = MajorVersionLimit;
            }
            if (base.DataCache.IsPropertyAvailable("DefaultViewUrl"))
            {
                try
                {
                    listSettingInfo.DefaultView = ParentWeb.Url.Substring(0, ParentWeb.Url.Length - (ParentWebUrl.Length > 1 ? ParentWebUrl.Length : 0)) + DefaultViewUrl;
                }
                catch (Exception e)
                {
                    mLogger.Warn("An error occurred when getting list default view:{0}. ID:{1}. Reason:{2}.", Title, ID, e);
                }
            }
            if (AveSPEnv.IsMoss)
            {
                listSettingInfo.AllowRatingSetting = Fields.Contains(AveFieldId.AverageRatings) && Fields.Contains(AveFieldId.RatingsCount);
                if (RootFolder.Properties != null && RootFolder.Properties.ContainsKey("Ratings_VotingExperience") && !string.IsNullOrEmpty(RootFolder.Properties["Ratings_VotingExperience"].ToString()))
                {
                    string experience = listSettingInfo.RootFolderInfo.Value.MetaInfoDic["Ratings_VotingExperience"].ToString();
                    listSettingInfo.RatingSettingType = (int)Enum.Parse(typeof(AveRatingSettingType), experience, true);
                }
                else
                {
                    listSettingInfo.RatingSettingType = (int)AveRatingSettingType.None;
                }
            }
            if (base.DataCache.IsPropertyAvailable("EventSinkAssembly"))
            {
                listSettingInfo.EventSinkAssembly = EventSinkAssembly;
            }
            if (ID.ToString().Equals(mParentWeb.TaxonomyList))
            {
                listSettingInfo.IsTaxonomyHiddenList = true;
            }
            if (base.DataCache.IsPropertyAvailable("AllowContentTypes"))
            {
                listSettingInfo.AllowContentTypes = AllowContentTypes;
            }
            if (base.DataCache.IsPropertyAvailable("AllowDeletion"))
            {
                listSettingInfo.AllowDeletion = AllowDeletion;
            }
            if (base.DataCache.IsPropertyAvailable("ShowUser"))
            {
                listSettingInfo.ShowUser = ShowUser;
            }
            if (base.DataCache.IsPropertyAvailable("AllowMultiResponses"))
            {
                listSettingInfo.AllowMultiResponses = AllowMultiResponses;
            }
            if (base.DataCache.IsPropertyAvailable("EnableFolderCreation"))
            {
                listSettingInfo.EnableFolderCreation = EnableFolderCreation;
            }
            if (base.DataCache.IsPropertyAvailable("EnableModeration"))
            {
                listSettingInfo.EnableModeration = EnableModeration;
            }
            if (base.DataCache.IsPropertyAvailable("IrmEnabled"))
            {
                listSettingInfo.IrmEnabled = IrmEnabled;
            }
            if (base.DataCache.IsPropertyAvailable("IrmExpire"))
            {
                listSettingInfo.IrmExpire = IrmExpire;
            }
            if (base.DataCache.IsPropertyAvailable("IrmReject"))
            {
                listSettingInfo.IrmReject = IrmReject;
            }
            if (base.DataCache.IsPropertyAvailable("EnableVersioning"))
            {
                listSettingInfo.EnableVersioning = EnableVersioning;
            }
            if (base.DataCache.IsPropertyAvailable("Ordered"))
            {
                listSettingInfo.Ordered = Ordered;
            }
            if (base.DataCache.IsPropertyAvailable("ContentTypesEnabled"))
            {
                listSettingInfo.ContentTypesEnabled = ContentTypesEnabled;
            }
            if (base.DataCache.IsPropertyAvailable("EnableAssignToEmail"))
            {
                listSettingInfo.EnableAssignToEmail = EnableAssignToEmail;
            }
            if (base.DataCache.IsPropertyAvailable("EnableDeployWithDependentList"))
            {
                listSettingInfo.EnableDeployWithDependentList = EnableDeployWithDependentList;
            }
            if (base.DataCache.IsPropertyAvailable("EnableDeployingList"))
            {
                listSettingInfo.EnableDeployingList = EnableDeployingList;
            }
            if (base.DataCache.IsPropertyAvailable("EnablePeopleSelector"))
            {
                listSettingInfo.EnablePeopleSelector = EnablePeopleSelector;
            }
            if (base.DataCache.IsPropertyAvailable("EnableResourceSelector"))
            {
                listSettingInfo.EnableResourceSelector = EnableResourceSelector;
            }
            if (base.DataCache.IsPropertyAvailable("EnableSchemaCaching"))
            {
                listSettingInfo.EnableSchemaCaching = EnableSchemaCaching;
            }
            if (base.DataCache.IsPropertyAvailable("EnforceDataValidation"))
            {
                listSettingInfo.EnforceDataValidation = EnforceDataValidation;
            }
            if (base.DataCache.IsPropertyAvailable("EnableSyndication"))
            {
                listSettingInfo.EnableSyndication = EnableSyndication;
            }
            if (base.DataCache.IsPropertyAvailable("ExcludeFromTemplate"))
            {
                listSettingInfo.ExcludeFromTemplate = ExcludeFromTemplate;
            }
            if (base.DataCache.IsPropertyAvailable("Hidden"))
            {
                listSettingInfo.Hidden = Hidden;
            }
            if (base.DataCache.IsPropertyAvailable("MultipleDataList"))
            {
                listSettingInfo.MultipleDataList = MultipleDataList;
            }
            if (base.DataCache.IsPropertyAvailable("CrawlNonDefaultViews"))
            {
                listSettingInfo.CrawlNonDefaultViews = CrawlNonDefaultViews;
            }
            if (base.DataCache.IsPropertyAvailable("NoCrawl"))
            {
                listSettingInfo.NoCrawl = NoCrawl;
            }
            if (base.DataCache.IsPropertyAvailable("EnableAttachments"))
            {
                listSettingInfo.EnableAttachments = EnableAttachments;
            }
            if (base.DataCache.IsPropertyAvailable("EnableMinorVersions"))
            {
                listSettingInfo.EnableMinorVersions = EnableMinorVersions;
            }
            if (base.DataCache.IsPropertyAvailable("ForceCheckout"))
            {
                listSettingInfo.ForceCheckout = ForceCheckout;
            }

            if (base.DataCache.IsPropertyAvailable("DraftVersionVisibility"))
            {
                listSettingInfo.DraftVersionVisibility = (int)DraftVersionVisibility;
            }
            if (base.DataCache.IsPropertyAvailable("AllowRssFeeds"))
            {
                listSettingInfo.AllowRssFeads = AllowRssFeeds;
            }
            if (base.DataCache.IsPropertyAvailable("EnableThrottling"))
            {
                listSettingInfo.EnableThrottling = EnableThrottling;
            }
            if (base.DataCache.IsPropertyAvailable("IsThrottled"))
            {
                listSettingInfo.IsThrottled = IsThrottled;
            }

            if (base.DataCache.IsPropertyAvailable("HasUniqueRoleAssignments"))
            {
                listSettingInfo.HasUniqueRoleAssigntments = HasUniqueRoleAssignments;
            }
            if (base.DataCache.IsPropertyAvailable("OnQuickLaunch"))
            {
                listSettingInfo.OnQuickLaunch = OnQuickLaunch;
            }

            if (base.DataCache.IsPropertyAvailable("ValidationFormula"))
            {
                listSettingInfo.ValidationFormula = ValidationFormula;
            }
            if (base.DataCache.IsPropertyAvailable("ValidationMessage"))
            {
                listSettingInfo.ValidationMessage = ValidationMessage;
            }

            if (base.DataCache.IsPropertyAvailable("IsSiteAssetsLibrary"))
            {
                listSettingInfo.IsSiteAssetsLibrary = IsSiteAssetsLibrary;
            }

            if (base.DataCache.IsPropertyAvailable("RequestAccessEnabled"))
            {
                listSettingInfo.RequestAccessEnabled = RequestAccessEnabled;
            }

            if (base.DataCache.IsPropertyAvailable("MetadataListFieldSettings"))
            {
                Dictionary<string, object> tempSettings = base.DataCache.GetProperty<Dictionary<string, object>>("MetadataListFieldSettings");
                listSettingInfo.EnableMetaPublish = tempSettings.ContainsKey("EnableMetadataPromotion") ? (bool)tempSettings["EnableMetadataPromotion"] : false;
                listSettingInfo.EnterPriseKeyWordsEnable = tempSettings.ContainsKey("EnableKeywordsField") ? (bool)tempSettings["EnableKeywordsField"] : false;
            }

            #region advanced Settings

            // 0 , UseListSetting
            // 1 , Browser
            // 2 , PreferClient
            if (base.DataCache.IsPropertyAvailable("DefaultItemOpen"))
            {
                listSettingInfo.DefaultItemOpen = (int)DefaultItemOpen;
                if (base.DataCache.IsPropertyAvailable("DefaultItemOpenUseListSetting") && !DefaultItemOpenUseListSetting)
                {
                    listSettingInfo.DefaultItemOpen = 0;
                }
                else
                {
                    switch (DefaultItemOpen)
                    {
                        case AveDefaultItemOpen.Browser:
                            listSettingInfo.DefaultItemOpen = 1;
                            break;
                        case AveDefaultItemOpen.PreferClient:
                            listSettingInfo.DefaultItemOpen = 2;
                            break;
                        default:
                            break;
                    }
                }
            }
            if (base.DataCache.IsPropertyAvailable("ExcludeFromOfflineClient"))
            {
                listSettingInfo.ExcludeFromOfflineClient = ExcludeFromOfflineClient;
            }
            if (base.DataCache.IsPropertyAvailable("SendToLocationName") && base.DataCache.IsPropertyAvailable("SendToLocationUrl"))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(SendToLocationName);
                sb.Append("|");
                sb.Append(SendToLocationUrl);
                listSettingInfo.SendToLocation = sb.ToString();
                listSettingInfo.SendToLocationName = SendToLocationName;
                listSettingInfo.SendToLocationUrl = SendToLocationUrl;
            }
            if (this is IAveDocumentLibrary)
            {
                //ADO-36670 Client 没有Load SchemaXMl故原来取不出来，this.DocumentTemplateUrl赋值错误。
                String schemaXml = SchemaXml; //this.DataCache.GetProperty<String>("SchemaXml");
                if (!string.IsNullOrEmpty(schemaXml))
                {
                    string search = "<List DocTemplateUrl=\"";
                    int starts = schemaXml.IndexOf(search, StringComparison.OrdinalIgnoreCase);
                    if (starts != -1)
                    {
                        int length = search.Length;
                        schemaXml = schemaXml.Substring(starts + length);
                        int ends = schemaXml.IndexOf("\"", StringComparison.OrdinalIgnoreCase);
                        if (ends != -1)
                        {
                            DocumentTemplateUrl = schemaXml.Substring(0, ends);
                            listSettingInfo.DocumentTemplateUrl = DocumentTemplateUrl;
                        }
                    }
                }
            }
            if (base.DataCache.IsPropertyAvailable("DisableGridEditing"))
            {
                listSettingInfo.DisableGridEditing = DisableGridEditing;
            }
            if (base.DataCache.IsPropertyAvailable("NavigateForFormsPages"))
            {
                listSettingInfo.NavigateForFormsPages = NavigateForFormsPages;
            }
            if (base.DataCache.IsPropertyAvailable("EnableManagedIndexes"))
            {
                listSettingInfo.EnableManagedIndexes = EnableManagedIndexes;
            }
            if (base.DataCache.IsPropertyAvailable("ReadSecurity"))
            {
                listSettingInfo.ReadSecurity = ReadSecurity;
            }
            if (base.DataCache.IsPropertyAvailable("WriteSecurity"))
            {
                listSettingInfo.WriteSecurity = WriteSecurity;
            }

            if (base.DataCache.IsPropertyAvailable("ListExperienceOptions"))
            {
                listSettingInfo.ListExperienceOptions = (int)ListExperienceOptions;
            }

            #endregion

            return listSettingInfo;
        }

        public AveRestoreResult RestoreListItem(AveListItemInfo info, Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            SetTaxonomyField(info, -1, true, info.FieldsInfo.TermIdMapping, info.FieldsInfo.MergedTermIdMapping);
            Dictionary<string, object> docData = AssembleBaseItemInfo(info, this);
            docData["ListTemplate"] = (int)BaseTemplate;
            docData["ListEnableModeration"] = EnableModeration;
            docData["ListEnableVersioning"] = EnableVersioning;
            Dictionary<string, object> fields = AveList.ConvertFieldValuesToString(info.FieldsInfo.Fields);
            if (!fields.ContainsKey("Modified"))
            {
                fields.Add("Modified", info.DTimeLastModified);
            }

            if (BaseTemplate == AveListTemplateType.DiscussionBoard)
            {
                if (data.ContainsKey("DiscussionTopic"))
                {
                    docData["DiscussionTopic"] = data["DiscussionTopic"];
                }
                if (data.ContainsKey("ParentThreadId"))
                {
                    docData["ParentThreadId"] = data["ParentThreadId"];
                }
            }

            if (BaseTemplate == AveListTemplateType.Meetings)
            {
                AssemblyMeetingItemInfo(info, userData, docData);
            }
            fields.Add("NeedSetNullFields", info.NeedSetNullFields);
            if (docData.ContainsKey("ListId") && docData["ListId"] != null)
            {
                var oldId = new Guid(docData["ListId"] as string);
                Guid value = Guid.Empty;
                if (info.MappingManager.SiteMappingManager.GetValueFromListIdMapping(oldId, out value))
                {
                    docData["ListId"] = value.ToString();
                }
                else
                {
                    docData["ListId"] = oldId.ToString();
                }
            }
            Dictionary<string, object> restoreResult = mRequest.RestoreListItem(docData, fields, null);
            info.IsNewCreated = restoreResult.ContainsKey("IsNewCreated") ? (bool)restoreResult["IsNewCreated"] : false;

            if (!(Boolean)restoreResult["RestoreStatus"])
            {
                throw new AveRestoreException(AveRestoreResult.Failed, restoreResult["Exception"] as string);
            }
            AveListItem item = new AveListItem(mRequest, mParentWeb, this, restoreResult["Item"] as Dictionary<string, object>, false);
            info.AveItem.ListItem = item;
            info.RowId = item.ID;
            return AveRestoreResult.Normal;
        }

        public IAveRelatedFieldCollection GetRelatedFields()
        {
            Dictionary<string, object> relatedFieldProperties = mRequest.GetRelatedFields(mParentWeb.ServerRelativeUrl, Title, ID);
            AveRelatedFieldCollection relatedFieldCollection = new AveRelatedFieldCollection(mRequest, this, relatedFieldProperties);
            return relatedFieldCollection;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property")]
        public Dictionary<Guid, Guid> GetAlerts(string url, int itemId, AveSPAlertHostType hostType)
        {
            Dictionary<Guid, Guid> listAlerts = new Dictionary<Guid, Guid>();
            Dictionary<string, object> webAlerts = mRequest.GetAlerts(mParentWeb.ServerRelativeUrl);
            if (webAlerts.Count > 0)
            {
                List<Dictionary<string, object>> alerts = webAlerts[AveObjectModelConstant.ChildrenProperties] as List<Dictionary<string, object>>;
                foreach (Dictionary<string, object> alert in alerts)
                {
                    switch (hostType)
                    {
                        case AveSPAlertHostType.List:
                        case AveSPAlertHostType.Folder:
                            if (alert["ListID"].Equals(ID) && !alert.ContainsKey("ItemID"))
                            {
                                Dictionary<string, object> alertProperties = alert["Properties" + AveObjectModelConstant.ObjectPropertySuffix] as Dictionary<string, object>;
                                if (alertProperties != null && alertProperties.ContainsKey("alertoldid"))
                                {
                                    listAlerts.Add(new Guid(alertProperties["alertoldid"].ToString()), new Guid(alert["ID"].ToString()));
                                }
                            }
                            break;
                        case AveSPAlertHostType.Doc:
                        case AveSPAlertHostType.Item:
                            if (alert["ListID"].Equals(ID) && alert["ItemID"].Equals(itemId))
                            {
                                Dictionary<string, object> alertProperties = alert["Properties" + AveObjectModelConstant.ObjectPropertySuffix] as Dictionary<string, object>;
                                if (alertProperties != null && alertProperties.ContainsKey("alertoldid"))
                                {
                                    listAlerts.Add(new Guid(alertProperties["alertoldid"].ToString()), new Guid(alert["ID"].ToString()));
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            return listAlerts;
        }

        public Guid Recycle()
        {
            return mRequest.RecycleList(ParentWeb.ServerRelativeUrl, Title, ID);
        }

        public bool IsACCSRVSystemList()
        {
            bool isMSysASOSystemList = false;
            bool isMacroSystemList = false;
            IAveWeb web = ParentWeb;
            if (web != null && web.WebTemplate != null && web.WebTemplate.Equals("ACCSRV", StringComparison.OrdinalIgnoreCase))
            {
                #region IsMSysASOSystemList

                if (web.AllProperties.ContainsKey("___MSysASOId"))
                {
                    isMSysASOSystemList = ID.Equals(new Guid((string)web.AllProperties["___MSysASOId"]));
                }
                else
                {
                    isMSysASOSystemList = Title.Equals("MSysASO", StringComparison.OrdinalIgnoreCase);
                }

                #endregion

                #region IsMacroSystemList

                if (!isMSysASOSystemList)
                    isMacroSystemList = Title.Equals("Macro", StringComparison.OrdinalIgnoreCase);

                #endregion
            }
            return isMSysASOSystemList || isMacroSystemList;
        }

        public void UpdateWorkflowAssociation(IAveWorkflowAssociation association)
        {
            ((AveWorkflowAssociation)association).Update();
        }

        public IAveWorkflowAssociation AddWorkflowAssociation(IAveWorkflowAssociation association)
        {
            Dictionary<string, object> props = mRequest.CreateListAssociation(ParentWeb.ServerRelativeUrl, ID, "web.workflowTemplates", association);
            AveWorkflowAssociation newWFAssociation = new AveWorkflowAssociation(ParentWeb, this, string.Empty, props);
            if (base.DataCache.IsPropertyAvailable("WorkflowAssociations"))
            {
                (WorkflowAssociations as AveWorkflowAssociationCollection).ListData.Add(newWFAssociation);
            }
            return newWFAssociation;
        }

        public ulong Flags
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Flags"))
                {
                    GetPropertiesFromSchemaXml();
                }
                return base.DataCache.GetProperty<ulong>("Flags");
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
                    IAveFile spFile = this.ParentWeb.GetFile(this.RootFolder.ServerRelativeUrl + "/Forms/client_LocationBasedDefaults.html");
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

        public IAveAudit Audit
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// 当ChangedProperties.Count == 0 时，只有ContentType，Column和WorkflowAssociations被Reload，本身Setting没有重新获取。
        /// </summary>
        public void Reload()
        {
            this.Update();
            InvalidCollections();
        }

        public void ReloadFields()
        {
            Dictionary<string, object> listFields = mRequest.GetFields(this.ParentWeb.ServerRelativeUrl, this.DefaultViewUrl, this.Title, this.ID, "list.fields", null);
            AveFieldCollection fieldCollection = new AveFieldCollection(mParentWeb, this, "list.fields", listFields);
            DataCache.PropertiesCache["Fields"] = fieldCollection;
        }
        private void InvalidCollections()
        {
            lock (privateLockContentTypes)
            {
                if (this.DataCache.IsPropertyAvailable("ContentTypes"))
                {
                    this.DataCache.GetProperty<AveContentTypeCollection>("ContentTypes").IsCollectionDirty = true;
                }
            }
            lock (privateLockFields)
            {
                if (this.DataCache.IsPropertyAvailable("Fields"))
                {
                    this.DataCache.GetProperty<AveFieldCollection>("Fields").IsCollectionDirty = true;
                }
            }
            lock (privateLockWorkflowAssociations)
            {
                if (this.DataCache.IsPropertyAvailable("WorkflowAssociations"))
                {
                    this.DataCache.GetProperty<AveWorkflowAssociationCollection>("WorkflowAssociations").IsDirty = true;
                }
            }
        }

        public void SetWorkflowsAssociated(bool bWorkflowsAssociated)
        {
            //throw new NotImplementedException();
        }

        public IAveListItem GetItemByIdSelectedFields(int id, params string[] fields)
        {
            throw new NotImplementedException();
        }

        public void UpdateListRssSetting(Dictionary<string, object> updateProp)
        {
            mRequest.UpdateListRssSetting(ParentWebUrl, ID, updateProp);
        }

        public void GetViews(Dictionary<Guid, List<AveViewInfo>> viewCache)
        {
            viewCache.Clear();
            foreach (IAveView iView in Views)
            {
                AveView view = iView as AveView;
                if (view != null && !string.IsNullOrEmpty(view.Title))
                {
                    Guid PageUrlID = view.PageUrlID;
                    if (!viewCache.ContainsKey(PageUrlID))
                    {
                        viewCache.Add(PageUrlID, new List<AveViewInfo>());
                    }
                    List<AveViewInfo> views = viewCache[PageUrlID];
                    AveViewInfo viewInfo = new AveViewInfo();
                    viewInfo.Id = view.ID;
                    viewInfo.Title = view.Title;
                    viewInfo.IsDefaultView = view.DefaultView;
                    viewInfo.IsPersonal = view.PersonalView;
                    viewInfo.ViewType = AveViewInfo.GetViewType(view.Type);
                    viewInfo.Hidden = view.Hidden;
                    viewInfo.ListViewXml = view.ListViewXml;
                    views.Add(viewInfo);
                }
            }
        }

        public bool RequestAccessEnabled
        {
            get { return base.DataCache.GetProperty<bool>("RequestAccessEnabled"); }
            set
            {
                if (!RequestAccessEnabled.Equals(value))
                {
                    base.DataCache.AddChangedProperty("RequestAccessEnabled", value);
                }
            }
        }

        public AveComplianceTagInfo ComplianceTag
        {
            get
            {
                if (mParentWeb.Site.IsOnlineSite && mComplianceTag == null)
                {
                    mComplianceTag = (mRequest ).GetListComplianceTagProperties(this.ParentWeb.ServerRelativeUrl, this.RootFolder.ServerRelativeUrl);
                }
                return mComplianceTag;
            }
            set
            {
                if (value != null)
                {
                    if (mRequest.Type == AveClientRequestType.AveClientOMOffice365Request)
                    {
                        var newProperties = (mRequest ).UpdateListComplianceTagProperties(this.ParentWeb.ServerRelativeUrl, this.RootFolder.ServerRelativeUrl, value);
                        if(newProperties != null)
                        {
                            mComplianceTag = newProperties;
                        }
                    }
                }
            }
        }

        public override IAveSecurableObjectImpl SecurableObjectImpl
        {
            get
            {
                if (HasUniqueRoleAssignments)
                {
                    return new AveSecurableObjectImpl(ID, RoleAssignments);
                }
                return ParentWeb.SecurableObjectImpl;
            }
        }

        public Collection<IAveSPListItemInfo> GetItemsWithUniquePermissions()
        {
            throw new NotImplementedException();
        }

        public List<int> GetItemsByColumnValue(string columnDisplayName, string value)
        {
            List<int> listIds = new List<int>();
            if (Fields.ContainsField(columnDisplayName))
            {
                IAveField field = Fields[columnDisplayName];
                if (field.Type == AveFieldType.Text)
                {
                    string internalName = field.InternalName;
                    AveCamlQuery camlQuery = new AveCamlQuery();
                    camlQuery.FolderServerRelativeUrl = RootFolder.ServerRelativeUrl;
                    string query = string.Format("<View><Query><Where><Eq><FieldRef Name='{0}'/><Value Type='Text'>{1}</Value></Eq></Where></Query></View>", internalName, value);
                    camlQuery.ViewXml = query;
                    IAveListItemCollection items = GetItems(camlQuery);
                    foreach (IAveListItem item in items)
                    {
                        if (!listIds.Contains(item.ID))
                        {
                            listIds.Add(item.ID);
                        }
                    }
                    return listIds;
                }
            }
            return null;
        }

        public int Version
        {
            get { throw new NotImplementedException(); }
        }

        public void CleanListData()
        {
            AveClientCacheHandler.CleanSchemaXml(mParentWeb.CacheHandlerId,ParentWeb.ID.ToString(), ID.ToString());
            CleanCollectionData();
        }

        public bool CheckItemIsExist(int rowId)
        {
            return false;
        }

        public bool CheckItemIsExist(string rowId, Guid itemId, string parentFolderServerRelativeUrl = null)
        {
            IAveListItem item = (Items as AveListItemCollection).GetItemByGuid(itemId, parentFolderServerRelativeUrl);
            if (item == null)
            {
                throw new Exception("Item not find.");
            }
            return true;
        }

        public void UpdateListCreated(DateTime created)
        {
            //Client 不需要实现
        }

        public void UpdateListModifyInfo(Dictionary<string, object> modifyInfoDictionary)
        {
            //Client 暂时不实现，如果需要实现可以考虑支持可以支持的部分
        }

        public bool CheckIfHasAlertsOfSpecificConditions(int? itemId, AveEventType eventType, int userId, AveAlertFrequency frequency)
        {
            return false;
        }

        public void RestoreListRatingSetting(AveListSettingInfo info)
        {
        }

        #region add for SP2013

        public int SearchVersion { get; set; }

        public IAveInformationRightsManagementSettings InformationRightsManagementSettings
        {
            get
            {
                //只有13有此属性，10模拟返回null
                if (mRequest.Type == AveClientRequestType.AveClientOM2013Request || mRequest.Type == AveClientRequestType.AveClientOMOffice365Request)
                {
                    if (base.DataCache.IsPropertyNotLoaded("InformationRightsManagementSettings"))
                    {
                        Dictionary<string, object> settings = (mRequest ).GetListInformationRightsManagementSettings(ParentWebUrl, ID);
                        base.DataCache.PropertiesCache["InformationRightsManagementSettings"] = new AveInformationRightsManagementSettings(mRequest, this, settings);
                    }
                    return base.DataCache.GetProperty<IAveInformationRightsManagementSettings>("InformationRightsManagementSettings");
                }
                return null;
            }
        }

        #endregion

        #region Add to operate Change Log ** We will implement this in SP2013 first **

        public IAveChangeCollection GetChanges()
        {
            throw new NotImplementedException();
        }

        public IAveChangeCollection GetChanges(IAveChangeQuery query)
        {
            Dictionary<string, object> changeCollectionDic = mRequest.GetListChangesByQuery(mParentWeb.ServerRelativeUrl, ID, (query as AveChangeQuery).DataCache.PropertiesCache);
            return new AveChangeCollection(changeCollectionDic);
        }

        public IAveChangeCollection GetChanges(IAveChangeToken changeToken)
        {
            throw new NotImplementedException();
        }

        public IAveChangeCollection GetChanges(IAveChangeToken changeToken, IAveChangeToken changeTokenEnd)
        {
            throw new NotImplementedException();
        }

        #endregion


        public IAveUserResource TitleResource
        {
            get
            {
                if (!mParentWeb.Site.IsOnlineSite)
                {
                    return null;
                }
                lock (privateLockTitleResource)
                {
                    if(mTitleResource == null)
                    {
                        mTitleResource = new AveListUserResource(this, AveUserResourceConstants.TITLE_RESOUCE, this.DataCache);
                    }
                    return mTitleResource;
                }
            }
        }

        public IAveUserResource DescriptionResource
        {
            get
            {
                if (!mParentWeb.Site.IsOnlineSite)
                {
                    return null;
                }
                lock (privateLockDescriptionResource)
                {
                    if (mDescriptionResource == null)
                    {
                        mDescriptionResource = new AveListUserResource(this, AveUserResourceConstants.DESCRIPTION_RESOUCE, this.DataCache);
                    }
                    return mDescriptionResource;
                }
            }
        }


        internal override void InitRoleAssignmentProperties(Dictionary<string, object> roleAssignmentProperties)
        {
            roleAssignmentProperties[AveObjectModelConstant.WebServerRelativeUrl] = mParentWeb.ServerRelativeUrl;
            roleAssignmentProperties[AveObjectModelConstant.ListTitle] = Title;
        }

        internal override Dictionary<string, object> AddRoleAssignment(Dictionary<string, object> roleAssignmentProperties)
        {
            return mRequest.AddRoleAssignment(mParentWeb.ServerRelativeUrl, DefaultViewUrl, Title, ID, -1, roleAssignmentProperties, "list.roleAssignments");
        }

        internal override Dictionary<string, object> UpdateRoleAssignment(int principalId, Dictionary<string, object> roleAssignmentProperties)
        {
            return mRequest.UpdateRoleAssignment(mParentWeb.ServerRelativeUrl, DefaultViewUrl, Title, ID, -1, principalId, roleAssignmentProperties, "list.roleAssignments");
        }

        internal void InvalidFields()
        {
            base.DataCache.PropertiesCache.Remove("Fields");
        }

        public List<string> SetNeedSetNullFields(bool keepDefaultValue, Dictionary<string, object> fields)
        {
            List<string> needSetNullFields = new List<string>();
            string[] AllCols =
            {
                "nvarchar1", "nvarchar2", "nvarchar3", "nvarchar4", "nvarchar5", "nvarchar6", "nvarchar7", "nvarchar8",
                "ntext1", "ntext2", "ntext3", "ntext4", "sql_variant1", "nvarchar9", "nvarchar10", "nvarchar11", "nvarchar12", "nvarchar13",
                "nvarchar14", "nvarchar15", "nvarchar16", "ntext5", "ntext6", "ntext7", "ntext8", "sql_variant2", "nvarchar17", "nvarchar18",
                "nvarchar19", "nvarchar20", "nvarchar21", "nvarchar22", "nvarchar23", "nvarchar24", "ntext9", "ntext10", "ntext11", "ntext12",
                "sql_variant3", "nvarchar25", "nvarchar26", "nvarchar27", "nvarchar28", "nvarchar29", "nvarchar30", "nvarchar31", "nvarchar32",
                "ntext13", "ntext14", "ntext15", "ntext16", "sql_variant4", "nvarchar33", "nvarchar34", "nvarchar35", "nvarchar36", "nvarchar37",
                "nvarchar38", "nvarchar39", "nvarchar40", "ntext17", "ntext18", "ntext19", "ntext20", "sql_variant5", "nvarchar41", "nvarchar42",
                "nvarchar43", "nvarchar44", "nvarchar45", "nvarchar46", "nvarchar47", "nvarchar48", "ntext21", "ntext22", "ntext23", "ntext24",
                "sql_variant6", "nvarchar49", "nvarchar50", "nvarchar51", "nvarchar52", "nvarchar53", "nvarchar54", "nvarchar55", "nvarchar56",
                "ntext25", "ntext26", "ntext27", "ntext28", "sql_variant7", "nvarchar57", "nvarchar58", "nvarchar59", "nvarchar60", "nvarchar61",
                "nvarchar62", "nvarchar63", "nvarchar64", "ntext29", "ntext30", "ntext31", "ntext32", "sql_variant8", "int1", "int2", "int3", "int4",
                "int5", "int6", "int7", "int8", "int9", "int10", "int11", "int12", "int13", "int14", "int15", "int16", "float1", "float2", "float3", "float4",
                "float5", "float6", "float7", "float8", "float9", "float10", "float11", "float12", "datetime1", "datetime2", "datetime3", "datetime4",
                "datetime5", "datetime6", "datetime7", "datetime8", "bit1", "bit2", "bit3", "bit4", "bit5", "bit6", "bit7", "bit8", "bit9", "bit10", "bit11",
                "bit12", "bit13", "bit14", "bit15", "bit16", "uniqueidentifier1"
            };

            IAveFieldCollection fieldCollection = Fields;
            //if (fields.ContainsKey("ContentType"))
            //{
            //    string contentTypeId = fields["ContentType"].ToString();
            //    IAveContentType contentType = this.ContentTypes.GetById(contentTypeId);
            //    if (contentType != null)
            //    {
            //        fieldCollection = contentType.Fields;
            //    }
            //    else
            //    {
            //        fieldCollection = this.Fields;
            //    }
            //}
            //else
            //{
            //    fieldCollection = this.Fields;
            //}

            foreach (IAveField field in fieldCollection)
            {
                try
                {
                    if (field.Type == AveFieldType.WorkflowStatus)
                    {
                        continue;
                    }
                    object obj = field.ColName;
                    if (obj != null)
                    {
                        string colName = obj.ToString();
                        if (AllCols.Contains(colName) && !field.Required)
                        {
                            if ((!String.IsNullOrEmpty(field.DefaultValue) || !String.IsNullOrEmpty(field.DefaultFormula)) && keepDefaultValue)
                            {
                                continue;
                            }
                            if (IsUnCompletedLookupField(field as IAveFieldLookup))
                            {
                                continue;
                            }
                            if (NoNeedSetNull(field))
                            {
                                continue;
                            }
                            needSetNullFields.Add(field.InternalName);
                        }
                    }
                }
                catch (Exception e)
                {
                    mLogger.Debug(AveObjectModel_CommonResource.SetNeedSetNullFieldsError, Title, mParentWeb.Url, e.ToString());
                    //mLog.Log(AveLogLevel.WARN,"An error occurred while SetNeedSetNullFields. error:{0}", e.ToString());
                }
            }
            return needSetNullFields;
        }

        private bool NoNeedSetNull(IAveField field)
        {
            return field.Hidden || field.TypeAsString.Equals("Facilities", StringComparison.OrdinalIgnoreCase) ||
                   field.ID.Equals(new Guid("{14c6cd06-7417-42c1-a051-89e455fd1090}")); //app catalog library field.
        }

        //sometimes lookupfield is restored in postaction, should set null for this field
        private bool IsUnCompletedLookupField(IAveFieldLookup field)
        {
            return field != null && string.IsNullOrEmpty(field.LookupList);
        }

        public void SetTaxonomyField(AveBaseItemInfo info, int LCID, bool ForceAddTerm, Dictionary<Guid, Guid> termIdMapping, Dictionary<Guid, List<Guid>> mergedTermIdMapping)
        {
            List<Dictionary<string, object>> needUpdateTaonoxyFields = new List<Dictionary<string, object>>();
            Dictionary<string, string> taxonomyField = info.FieldsInfo.TaxonomyFieldsInMapping;
            if (WrapperRuntime.CurrentContext.IsMoss && taxonomyField != null && taxonomyField.Count > 0)
            {
                try
                {
                    foreach (string fieldName in taxonomyField.Keys)
                    {
                        IAveField field = Fields.GetField(fieldName);
                        IAveTaxonomyField tField = field as IAveTaxonomyField;
                        IAveTaxonomySession session = (mParentWeb.Site).AveSPTaxonomySession;
                        IAveTermStore termStore = AveTaxonomyFieldUtility.GetTermStore(field, session, ref LCID);
                        if (termStore == null)
                        {
                            continue;
                        }
                        IAveTermSet termSet = null;
                        if (tField.TermSetId != Guid.Empty && termStore != null)
                        {
                            termSet = termStore.GetTermSet(tField.TermSetId);
                        }
                        IAveTerm endTerm = null;
                        if (tField.AnchorId != Guid.Empty && termSet != null)
                        {
                            endTerm = termSet.GetTerm(tField.AnchorId);
                        }

                        bool submit = false;
                        HashSet<String> termNames = new HashSet<string>(taxonomyField[fieldName].Split(';'), StringComparer.OrdinalIgnoreCase);
                        string[] termHiberarchy = null;
                        List<IAveTerm> terms = new List<IAveTerm>();
                        foreach (string termName in termNames)
                        {
                            IAveTerm term = null;
                            termHiberarchy = null;
                            string tName = termName.StartsWith("#", StringComparison.Ordinal) ? termName.Substring(1) : termName;
                            if (string.IsNullOrEmpty(tName))
                            {
                                continue;
                            }
                            try
                            {
                                if (tName.Contains("|"))
                                {
                                    try
                                    {
                                        Guid tTermId = Guid.Empty;
                                        string[] temp = tName.Split('|');
                                        if (temp.Length == 2)
                                        {
                                            tName = temp[0];
                                            tTermId = new Guid(temp[1]);
                                            if (termIdMapping != null && termIdMapping.ContainsKey(tTermId))
                                            {
                                                tTermId = termIdMapping[tTermId];
                                            }
                                            else if (mergedTermIdMapping != null)
                                            {
                                                foreach (var pair in mergedTermIdMapping)
                                                {
                                                    if (pair.Value.Contains(tTermId))
                                                    {
                                                        tTermId = pair.Key;
                                                        break;
                                                    }
                                                }
                                            }
                                            if (termSet != null)
                                            {
                                                term = termSet.GetTerm(tTermId);
                                                //添加判断，如果是EnterpriseKeyWord类型的Field，当KeyWord值引用TermStore上Term时，关联到正确的TermStore上。
                                                if (term == null && tField.ID.Equals(new Guid("23F27201-BEE3-471e-B2E7-B64FD8B7CA38")))
                                                {
                                                    foreach (IAveTermStore tStore in session.TermStores)
                                                    {
                                                        if (term == null)
                                                        {
                                                            term = tStore.GetTerm(tTermId);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                term = termStore.GetTerm(tTermId);
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        mLogger.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermByIdError, e.ToString());
                                    }
                                }
                                //'<'表示term的层次关系。
                                else if (tName.Contains("<"))
                                {
                                    termHiberarchy = tName.Split('<');
                                    bool needContinue = false;
                                    term = AveTaxonomyFieldUtility.FindTermForColumnMapping(session, tField, termSet, endTerm, tName, ref needContinue);
                                    if (needContinue)
                                    {
                                        continue;
                                    }
                                }
                                if (term == null && termSet != null)
                                {
                                    try
                                    {
                                        if (endTerm == null)
                                        {
                                            term = termSet.Terms[NormalizeName(tName)];
                                        }
                                        else
                                        {
                                            term = endTerm.Terms[NormalizeName(tName)];
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        mLogger.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCGetTermByNameError, e.ToString());
                                        //DOC-78396 使用此方法刷新对象
                                        IAveTermCollection ts = termSet.GetTerms(NormalizeName(tName).Trim(), true);
                                        if (endTerm == null)
                                        {
                                            term = termSet.Terms[NormalizeName(tName)];
                                        }
                                        else
                                        {
                                            term = endTerm.Terms[NormalizeName(tName)];
                                        }
                                    }
                                }
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                if (ForceAddTerm && termSet != null && !String.IsNullOrEmpty(tName))
                                {
                                    term = AveTaxonomyFieldUtility.CreateNotExistTerm(termSet, endTerm, tName, LCID, ref submit);
                                    submit = true;
                                    mLogger.Debug("Force Add Term. Term Name:{0}", tName);
                                }
                            }
                            if (term != null)
                            {
                                terms.Add(term);
                                //如果field不允许多值，没有必要找多个term了。
                                if (!tField.AllowMultipleValues)
                                {
                                    break;
                                }
                            }
                        }
                        if (submit)
                        {
                            try
                            {
                                termStore.CommitAll();
                                submit = false;
                            }
                            catch (Exception e)
                            {
                                mLogger.Log(AveLogLevel.DEBUG, WrapperCommonResource.AWCClearTermError, e.ToString());
                            }
                        }
                        Dictionary<string, object> taxonomyfield = SetTaxonomyValueToItem(tField, terms, LCID, fieldName);
                        if (taxonomyfield != null)
                        {
                            needUpdateTaonoxyFields.Add(taxonomyfield);
                        }
                    }
                    info.FieldsInfo.Fields.Add("TaxonomyFields", needUpdateTaonoxyFields);
                }
                catch (NotImplementedException ex)
                {
                    mLogger.Warn("Taxonomy Field is not support.Error Message:{0}.", ex.ToString());
                }
            }
        }

        private string NormalizeName(string termName)
        {
            if (termName == null)
            {
                return null;
            }
            Regex trimSpacesRegex = new Regex(@"\s+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            char tempChar = (char)0xff06;
            return trimSpacesRegex.Replace(termName, " ").Replace('&', tempChar);
        }

        private Dictionary<string, object> SetTaxonomyValueToItem(IAveTaxonomyField tField, List<IAveTerm> terms, int LCID, string fieldName)
        {
            Dictionary<string, object> taxonomyfield = null;
            if (tField.AllowMultipleValues)
            {
                //IAveTaxonomyFieldValueCollection taxValueCollection = tField.TaxonomyFieldValueCollection;
                taxonomyfield = new Dictionary<string, object>();
                taxonomyfield.Add("FieldName", fieldName);
                taxonomyfield.Add("TextField", Fields.GetFieldById(tField.TextField, false).InternalName);
                List<string> mutipleText = new List<string>();
                foreach (IAveTerm tTerm in terms)
                {
                    if (tTerm != null)
                    {
                        int effectiveLcid = LCID;
                        string text = " " + tTerm.GetDefaultLabel(effectiveLcid) + "|" + tTerm.ID; //Add a space to avoid exception, when Taxonomy string started with '#'.
                        mutipleText.Add(text);
                        //IAveTaxonomyFieldValue value2 = tField.TaxonomyFieldValue;
                        //value2.PopulateFromLabelGuidPair(text);
                        //taxValueCollection.Add(value2);
                    }
                }
                taxonomyfield.Add("Text", mutipleText);
                taxonomyfield["AllowMultipleValues"] = tField.AllowMultipleValues;
            }
            else
            {
                if (terms.Count > 0)
                {
                    int effectiveLcid = LCID;
                    string text = terms[0].GetDefaultLabel(effectiveLcid) + "|" + terms[0].ID;
                    taxonomyfield = new Dictionary<string, object>();
                    taxonomyfield.Add("FieldName", fieldName);
                    taxonomyfield.Add("TextField", Fields.GetFieldById(tField.TextField, false).InternalName);
                    taxonomyfield.Add("Text", text);
                    taxonomyfield["AllowMultipleValues"] = tField.AllowMultipleValues;
                }
            }
            return taxonomyfield;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Doclib is a part of keys")]
        public static Dictionary<string, object> AssembleBaseItemInfo(AveBaseItemInfo info, IAveList aveList)
        {
            Dictionary<string, object> docData = new Dictionary<string, object>();
            docData["FolderUrl"] = info.ParentFolderRelativeUrl;
            docData["WebUrl"] = info.ParentWebRelativeUrl;
            docData["ListTitle"] = info.ParentListTitle;
            docData["ListId"] = info.ListId.ToString();
            docData["RestoreOption"] = info.RestoreOption;
            docData["DoclibRowId"] = info.OriginalRowId;
            docData["UIVersion"] = info.OriginalVersion;
            docData["Level"] = info.OriginalLevel;
            docData["DraftOwnerId"] = info.DraftOwnerId;
            docData["_ModerationStatus"] = info.ModerationStatus;
            docData["ModerationComments"] = info.ModerationComments;
            docData["CheckOutUserId"] = info.CheckoutUserId;
            docData["DeleteItem"] = info.SettingInfo.DELETE_ITEM;
            docData["Title"] = info.Name;
            docData["Size"] = info.DocumentSize;
            docData["HasStream"] = info.HasStream;
            docData["ServerRelativeUrl"] = info.ServerRelativeUrl;
            docData["HasPreCurrentVersion"] = info.HasPreCurrentVersion;
            docData["Id"] = info.GUID;
            docData["SKIP_IF_SAME_MODIFIEDTIME"] = info.SettingInfo.SKIP_IF_SAME_MODIFIEDTIME;
            docData["MOVE_ITEM_TO_CONFLICT_FOLDER"] = info.SettingInfo.MOVE_ITEM_TO_CONFLICT_FOLDER && info.SettingInfo.DELETE_ITEM; //将MOVE_ITEM_TO_CONFLICT_FOLDER属性封装一下，restore时需要；
            docData["MOVE_SOURCE_TO_CONFLICT_FOLDER"] = info.SettingInfo.MOVE_SOURCE_TO_CONFLICT_FOLDER && info.SettingInfo.DELETE_ITEM;
            docData["OverwriteByLastModifiedTime"] = info.SettingInfo.OverWriteByModifiedTime;
            if (info.DocData != null && info.DocData.ContainsKey("BiggestVersionModified"))
            {
                docData["BiggestVersionModified"] = info.DocData["BiggestVersionModified"];
            }
            if (info.DocData != null && info.DocData.ContainsKey("IsSystemFile"))
            {
                docData["IsSystemFile"] = info.DocData["IsSystemFile"];
            }
            if (info.MappingManager != null)
            {
                docData["DestRowId"] = info.MappingManager.SiteMappingManager.GetMappingItemId(info.ListId, info.OriginalRowId);
            }
            AveListItemInfo itemInfo = info as AveListItemInfo;
            if (itemInfo != null)
            {
                docData["GUID"] = itemInfo.tp_Guid;
            }
            if (info.RestoringItem != null)
            {
                docData["IsNewCreated"] = info.RestoringItem.IsNewItem;
            }
            if (aveList != null)
            {
                docData["ListRootFolderServerRelativeUrl"] = aveList.RootFolder.ServerRelativeUrl;
            }
            if (info.SettingInfo.CheckItemByFieldValue)
            {
                docData["MatchItemFieldDisplayName"] = info.SettingInfo.MatchItemFieldDisplayName;
            }
            return docData;
        }

        public static void RemoveDocumentId(Dictionary<string, object> fieldValues)
        {
            fieldValues.Remove("_dlc_DocId");
            fieldValues.Remove("_dlc_DocIdUrl");
            fieldValues.Remove("_dlc_DocIdUrl#2");
        }

        public static Dictionary<string, object> ConvertFieldValuesToString(Dictionary<string, object> fieldValues)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();

            //不是ContentTypeId类型的ContentType，Item Update的时候会抛异常，在此删除这种ContentType避免后面的异常
            if (fieldValues.ContainsKey("ContentType"))
            {
                AveFieldValueInfo fieldInfo = fieldValues["ContentType"] as AveFieldValueInfo;
                AveContentTypeId itemContentTypeId = fieldInfo.ColValue as AveContentTypeId;
                if (itemContentTypeId == null)
                {
                    fieldValues.Remove("ContentType");
                }
            }

            foreach (KeyValuePair<string, object> kv in fieldValues)
            {
                AveFieldValueInfo fieldInfo = kv.Value as AveFieldValueInfo;
                if (fieldInfo != null && fieldInfo.ColValue != null && !fieldInfo.ColValue.GetType().IsAssignableFrom(typeof(IAveTaxonomyFieldValue)) && !fieldInfo.ColValue.GetType().IsAssignableFrom(typeof(IAveTaxonomyFieldValueCollection)))
                {
                    if (fieldInfo.FieldType == AveFieldType.URL)
                    {
                        AveFieldUrlValue tempUrlValue = null;
                        string currentKey = kv.Key;
                        if (kv.Key.EndsWith("#2", StringComparison.OrdinalIgnoreCase))
                        {
                            currentKey = kv.Key.Remove(kv.Key.IndexOf("#2", StringComparison.OrdinalIgnoreCase));
                            if (fieldValues.ContainsKey(currentKey))
                            {
                                tempUrlValue = ((AveFieldValueInfo)fieldValues[currentKey]).ColValue as AveFieldUrlValue;
                                if (tempUrlValue != null)
                                {
                                    tempUrlValue.Description = fieldInfo.ColValue.ToString();
                                }
                                else
                                {
                                    tempUrlValue = new AveFieldUrlValue();
                                    tempUrlValue.Description = fieldInfo.ColValue.ToString();
                                    fieldInfo.ColValue = tempUrlValue;
                                    continue;
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (fieldValues.ContainsKey(currentKey + "#2"))
                            {
                                tempUrlValue = ((AveFieldValueInfo)fieldValues[currentKey + "#2"]).ColValue as AveFieldUrlValue;
                                if (tempUrlValue != null)
                                {
                                    tempUrlValue.Url = fieldInfo.ColValue.ToString();
                                }
                                else
                                {
                                    tempUrlValue = new AveFieldUrlValue();
                                    tempUrlValue.Url = fieldInfo.ColValue.ToString();
                                    fieldInfo.ColValue = tempUrlValue;
                                    continue;
                                }
                            }
                            else
                            {
                                tempUrlValue = new AveFieldUrlValue();
                                tempUrlValue.Url = fieldInfo.ColValue.ToString();
                                tempUrlValue.Description = fieldInfo.ColValue.ToString();
                            }
                        }
                        dic[currentKey] = tempUrlValue.ToString();
                    }
                    else if (!IsBasicType(fieldInfo))
                    {
                        dic[kv.Key] = fieldInfo.ColValue.ToString();
                    }
                    else
                    {
                        dic[kv.Key] = fieldInfo.ColValue;
                    }
                }
            }
            return dic;
        }

        /// <summary>
        /// 基本类型不需要tostring, 对于DateTime和浮点类型, 德语环境有问题, 因此只处理了这两类。
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        private static bool IsBasicType(AveFieldValueInfo fieldInfo)
        {
            return fieldInfo.ColValue.GetType() == typeof(DateTime)
                   || fieldInfo.ColValue.GetType() == typeof(float)
                   || fieldInfo.ColValue.GetType() == typeof(double);
        }

        internal void AssemblyMeetingItemInfo(AveListItemInfo itemInfo, Dictionary<string, object> userData, Dictionary<string, object> docData)
        {
            if (userData.ContainsKey("Title"))
            {
                docData["Title"] = userData["Title"];
            }
            int eventType = 0;
            if (userData.ContainsKey("EventType"))
            {
                eventType = (int)userData["EventType"];
                docData["EventType"] = eventType;
            }
            if (userData.ContainsKey("TimeZone"))
            {
                docData["TimeZone"] = (int)userData["TimeZone"];
            }
            else if (userData.ContainsKey("UID") && (eventType == 2 || eventType == 3))
            {
                docData["UID"] = userData["UID"];
            }
            if (userData.ContainsKey("EventDate"))
            {
                docData["EventDate"] = userData["EventDate"];
            }
            if (userData.ContainsKey("Duration"))
            {
                docData["Duration"] = (int)userData["Duration"];
            }
            if (userData.ContainsKey("EndDate"))
            {
                docData["EndDate"] = userData["EndDate"];
            }
            if (userData.ContainsKey("RecurrenceID"))
            {
                docData["RecurrenceID"] = userData["RecurrenceID"];
            }
            if (userData.ContainsKey("UID"))
            {
                docData["UID"] = userData["UID"];
            }
            if (userData.ContainsKey("Location"))
            {
                docData["Location"] = userData["Location"];
            }
            if (userData.ContainsKey("RecurrenceData"))
            {
                docData["RecurrenceData"] = userData["RecurrenceData"];
            }
            if (userData.ContainsKey("fAllDayEvent"))
            {
                docData["fAllDayEvent"] = userData["fAllDayEvent"];
            }
            if (userData.ContainsKey("fRecurrence"))
            {
                docData["fRecurrence"] = userData["fRecurrence"];
            }
            if (userData.ContainsKey("RRule"))
            {
                docData["RRule"] = userData["RRule"];
            }
            if (userData.ContainsKey("ExRule"))
            {
                docData["ExRule"] = userData["ExRule"];
            }
            if (userData.ContainsKey("SuppressUntil"))
            {
                docData["SuppressUntil"] = userData["SuppressUntil"];
            }
            if (userData.ContainsKey("IsOrphaned"))
            {
                //DOC-67486，在此处设置listItem["IsOrphaned"]=true或者不设置该值，都会导致listItem.Update抛出异常
                //所以在此处设置listItem["IsOrphaned"] = false，如果是true在之后更新field的时候会更新正确。
                //listItem["IsOrphaned"] = userData["IsOrphaned"];
                docData["IsOrphaned"] = false;
            }
            if (userData.ContainsKey("IsException"))
            {
                docData["IsException"] = userData["IsException"];
            }
            if (userData.ContainsKey("IsDetached"))
            {
                docData["IsDetached"] = userData["IsDetached"];
            }
            if (userData.ContainsKey("Sequence"))
            {
                docData["Sequence"] = userData["Sequence"];
            }
            if (userData.ContainsKey("DTStamp"))
            {
                docData["DTStamp"] = userData["DTStamp"];
            }
            if (userData.ContainsKey("#tp_InstanceID"))
            {
                docData["InstanceID"] = userData["#tp_InstanceID"];
            }
            if (itemInfo != null)
            {
                if (userData.ContainsKey("EventUID"))
                {
                    docData["EventUID"] = userData["EventUID"];
                    string[] idparts = userData["EventUID"].ToString().Split(':');
                    var value = Guid.Empty;
                    if (idparts.Length == 5 && itemInfo.MappingManager.SiteMappingManager.GetValueFromListIdMapping(new Guid(idparts[2]), out value))
                    {
                        docData["EventUID"] = userData["EventUID"].ToString().Replace(idparts[2], value.ToString("B"));
                    }
                }
                if (userData.ContainsKey("Organizer"))
                {
                    if (itemInfo != null)
                    {
                        docData["Organizer"] = itemInfo.Extension.PrincipalId;
                    }
                }
                if (userData.ContainsKey("EventUrl") && userData.ContainsKey("EventUrl#2"))
                {
                    docData["EventUrl"] = userData["EventUrl"];
                    docData["EventUrl#2"] = userData["EventUrl#2"];
                    docData["FieldUrlValue"] = itemInfo.Extension.FieldUrlValue;
                }
            }
        }

        private void GetPropertiesFromSchemaXml()
        {
            XmlDocument tempDocument = null;
            Dictionary<string, object> properties = new Dictionary<string, object>();
            Dictionary<string, string> propertiesPair = new Dictionary<string, string>();
            propertiesPair["Flags"] = "ulong";
            foreach (KeyValuePair<string, string> tempProperty in propertiesPair)
            {
                if (tempDocument == null)
                {
                    tempDocument = new XmlDocument();
                    tempDocument.LoadXml(SchemaXml);
                }
                XmlElement rootNode = tempDocument.DocumentElement;
                if (rootNode.HasAttribute("Flags") && !string.IsNullOrEmpty(rootNode.GetAttribute("Flags")))
                {
                    properties[tempProperty.Key] = GetValueFromType(tempProperty.Value, rootNode.GetAttribute("Flags"));
                }
            }
            if (properties.Count > 0)
            {
                base.DataCache.AddPropertyies(properties);
            }
        }

        private object GetValueFromType(string type, string strValue)
        {
            object value = null;
            try
            {
                switch (type)
                {
                    case "ulong":
                        value = Convert.ToUInt64(strValue);
                        break;
                    case "boolean":
                        value = Convert.ToBoolean(strValue);
                        break;
                    case "string":
                    default:
                        value = strValue;
                        break;
                }
            }
            catch (Exception ex)
            {
                mLogger.Debug(string.Format("Can not convert to certain type.Type:{0},value:{1},Messages:{2}", type, strValue, ex));
            }
            return value;
        }

        private void GetListAdvancedSettingProperties()
        {
            if ((listSettingFlag & (int)ListRequestSettingFlag.AdvancedSetting) != 0)
            {
                return;
            }
            base.DataCache.AddPropertyies(mRequest.GetListAdvancedSettingProperties(ParentWebUrl, ID));
            listSettingFlag |= (int)ListRequestSettingFlag.AdvancedSetting;
        }

        private void GetListVersionLimited()
        {
            if ((listSettingFlag & (int)ListRequestSettingFlag.ListVersion) != 0)
            {
                return;
            }
            base.DataCache.AddPropertyies(mRequest.GetListVersionLimited(ParentWebUrl, ID));
            listSettingFlag |= (int)ListRequestSettingFlag.ListVersion;
        }

        private void GetListGeneralSettings()
        {
            if ((listSettingFlag & (int)ListRequestSettingFlag.General) != 0)
            {
                return;
            }
            if (BaseType.Equals(AveBaseType.Survey) || BaseTemplate.Equals(AveListTemplateType.Events)) //Get Survey and Calendar list general setting.
            {
                base.DataCache.AddPropertyies(mRequest.GetListGeneralProperties(ParentWebUrl, ID));
            }
            listSettingFlag |= (int)ListRequestSettingFlag.General;
        }

        private void GetListEditViewSettingProperties()
        {
            if ((listSettingFlag & (int)ListRequestSettingFlag.EditViewSetting) != 0)
            {
                return;
            }
            if (DefaultView != null)
            {
                base.DataCache.AddPropertyies(mRequest.GetListEditViewSettingProperties(ParentWebUrl, Title, ID, DefaultView.ID));
            }
            listSettingFlag |= (int)ListRequestSettingFlag.EditViewSetting;
        }

        private void GetMetadataNavigationSettings()
        {
            if ((listSettingFlag & (int)ListRequestSettingFlag.MetadataNavigation) != 0)
            {
                return;
            }
            base.DataCache.AddPropertyies(mRequest.GetMetadataNavigationSettings(ParentWebUrl, ID, Title));
            listSettingFlag |= (int)ListRequestSettingFlag.MetadataNavigation;
        }

        private void GetPerLocationViewSettings()
        {
            if ((listSettingFlag & (int)ListRequestSettingFlag.PerLocationView) != 0)
            {
                return;
            }
            base.DataCache.AddPropertyies(mRequest.GetPerLocationViewSettings(ParentWebUrl, ID));
            listSettingFlag |= (int)ListRequestSettingFlag.PerLocationView;
        }

        private void GetListAccessRequestsSettingProperties()
        {
            if ((listSettingFlag & (int)ListRequestSettingFlag.AccessRequests) != 0)
            {
                return;
            }
            base.DataCache.AddPropertyies(mRequest.GetListAccessRequestsSettingProperties(ParentWebUrl, ID));
            listSettingFlag |= (int)ListRequestSettingFlag.AccessRequests;
        }

        private void GetListRssProperties()
        {
            if ((listSettingFlag & (int)ListRequestSettingFlag.Rss) != 0 || !RssViewExist)
            {
                return;
            }
            base.DataCache.AddPropertyies(mRequest.GetListRssProperties(ParentWebUrl, ID));
            listSettingFlag |= (int)ListRequestSettingFlag.Rss;
        }

        private void GetListProperties()
        {
            if (ParentWeb.AppInstanceId != Guid.Empty)
            {
                return;
            }
            GetListAdvancedSettingProperties();
            GetListVersionLimited();
            GetListGeneralSettings();
            GetListEditViewSettingProperties();
            GetMetadataNavigationSettings();
            GetPerLocationViewSettings();
            GetListAccessRequestsSettingProperties();
            GetListRssProperties();
            base.DataCache.PropertiesCache["MetadataListFieldSettings"] = mRequest.GetMetadataListFieldSettings(ParentWebUrl, Title, ID);
        }

        private void CleanCollectionData()
        {
            DataCache.RemoveProperty("Fields");
            DataCache.RemoveProperty("ContentTypes");
            DataCache.RemoveProperty("Views");
        }

        #region IAveList Members

        internal Dictionary<string, string> NeedLoadFields
        {
            get
            {
                if (mNeedLoadFields == null || !mIsNeedLoadFieldsInitialized)
                {
                    lock (mLoadFieldLock)
                    {
                        if (mNeedLoadFields == null || !mIsNeedLoadFieldsInitialized)
                        {
                            mNeedLoadFields = new Dictionary<string, string>();
                            IAveField tempField;
                            if ((tempField = Fields.GetFieldById(AveBuiltInFieldId.Author, false)) != null)
                            {
                                mNeedLoadFields.Add(tempField.InternalName, tempField.TypeAsString);
                            }
                            foreach (AveField field in Fields)
                            {
                                if (!(string.IsNullOrEmpty(field.ColName) || AveList.IgnoreFields.Contains(field.InternalName)) && !field.InternalName.Equals("Created_x0020_By"))
                                {
                                    mNeedLoadFields[field.InternalName] = field.TypeAsString;
                                }
                                else if (field.InternalName.Equals("_CheckinComment"))
                                {
                                    mNeedLoadFields[field.InternalName] = field.TypeAsString;
                                }
                            }
                            mIsNeedLoadFieldsInitialized = true;
                        }
                    }
                }
                return mNeedLoadFields;
            }
            set { mNeedLoadFields = value; }
        }

        public Dictionary<string, int> ListItemGuidAndRowIdMappings
        {
            get
            {
                lock (loadListItemGuidAndRowIdMappingLock)
                {
                    if (mItemIdMapping == null)
                    {
                        InitItemsIdMapping();
                    }
                    return mItemIdMapping;
                }
            }
        }

        protected void InitItemsIdMapping()
        {
            lock (loadListItemGuidAndRowIdMappingLock)
            {
                var FieldNameList = new List<string> { AveFieldNameCollection.Guid_Field, AveFieldNameCollection.UniqueId_Field };
                var mappingCollection = (mRequest ).GetListItemGuidAndRowIdMappingsInLargeList(ParentWebUrl, RootFolder.ServerRelativeUrl, ID, FieldNameList);
                if (mappingCollection != null)
                {
                    mItemIdMapping = mappingCollection[AveFieldNameCollection.Guid_Field];
                    mItemUniqueIdAndRowIdMapping = mappingCollection[AveFieldNameCollection.UniqueId_Field];
                }
                else
                {
                    mItemIdMapping = new Dictionary<string, int>();
                    mItemUniqueIdAndRowIdMapping = new Dictionary<string, int>();
                }
            }
        }

        public Dictionary<string, int> ListItemUniqueIdAndRowIdMappings
        {
            get
            {
                lock (loadListItemGuidAndRowIdMappingLock)
                {
                    if (mItemUniqueIdAndRowIdMapping == null)
                    {
                        InitItemsIdMapping();
                    }
                    return mItemUniqueIdAndRowIdMapping;
                }
            }
        }

        public string DocumentTemplateUrl
        {
            get { return base.DataCache.GetProperty<string>("DocumentTemplateUrl"); }
            set
            {
                if (!string.Equals(DocumentTemplateUrl, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("DocumentTemplateUrl", value);
                }
            }
        }

        public bool AllowDeletion
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("AllowDeletion"))
                {
                    string schema = SchemaXml;
                    if (!string.IsNullOrEmpty(schema))
                    {
                        XmlDocument xDoc = new XmlDocument();
                        xDoc.LoadXml(schema);
                        if (xDoc.DocumentElement.HasAttribute("AllowDeletion"))
                        {
                            base.DataCache.PropertiesCache["AllowDeletion"] = Convert.ToBoolean(xDoc.DocumentElement.GetAttribute("AllowDeletion"));
                            return base.DataCache.GetProperty<bool>("AllowDeletion");
                        }
                    }
                    base.DataCache.PropertiesCache["AllowDeletion"] = default(bool);
                }
                return base.DataCache.GetProperty<bool>("AllowDeletion");
            }
            set
            {
                if (!AllowDeletion.Equals(value))
                {
                    base.DataCache.AddChangedProperty("AllowDeletion", value);
                }
            }
        }

        public bool AllowRssFeeds
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("AllowRssFeeds"))
                {
                    GetListRssProperties();
                    if (base.DataCache.IsPropertyAvailable("RootFolderRssProperties"))
                    {
                        (RootFolder as AveFolder).DataCache.PropertiesCache["Properties" + AveObjectModelConstant.ObjectPropertySuffix] = base.DataCache.GetProperty<Hashtable>("RootFolderRssProperties");
                    }
                }
                return base.DataCache.GetProperty<bool>("AllowRssFeeds");
            }
        }

        public bool AllowMultiResponses
        {
            get { return base.DataCache.GetProperty<bool>("AllowMultiResponses"); }
            set
            {
                if (!AllowMultiResponses.Equals(value))
                {
                    base.DataCache.AddChangedProperty("AllowMultiResponses", value);
                }
            }
        }

        public bool AllowContentTypes
        {
            get { return base.DataCache.GetProperty<bool>("AllowContentTypes"); }
        }

        public IAveUser Author
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Author"))
                {
                    string loginName = base.DataCache.GetProperty<string>("Author" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveUser author = ParentWeb.SiteUsers.GetByLoginName(loginName) as AveUser;
                    base.DataCache.PropertiesCache["Author"] = author;
                }
                return base.DataCache.GetProperty<IAveUser>("Author");
            }
        }

        public AveBasePermissions AnonymousPermMask64
        {
            get { return base.DataCache.GetProperty<AveBasePermissions>("AnonymousPermMask64"); }
            set { base.DataCache.AddChangedProperty("AnonymousPermMask64", value); }
        }

        public AveListTemplateType BaseTemplate
        {
            get { return base.DataCache.GetProperty<AveListTemplateType>("BaseTemplate"); }
        }

        public AveBaseType BaseType
        {
            get { return base.DataCache.GetProperty<AveBaseType>("BaseType"); }
        }

        public DateTime Created
        {
            get { return base.DataCache.GetProperty<DateTime>("Created"); }
        }

        public IAveContentTypeCollection ContentTypes
        {
            get
            {
                AveContentTypeCollection contentTypeCollection = null;
                lock (privateLockContentTypes)
                {
                    if (base.DataCache.IsPropertyNotLoaded("ContentTypes"))
                    {
                        contentTypeCollection = new AveContentTypeCollection(ParentWeb, this, "list.contentTypes");
                        base.DataCache.PropertiesCache["ContentTypes"] = contentTypeCollection;
                    }
                    else
                    {
                        contentTypeCollection = base.DataCache.GetProperty<AveContentTypeCollection>("ContentTypes");
                        if (contentTypeCollection.IsCollectionDirty)
                        {
                            contentTypeCollection.UpdateCollectionInternally();
                        }
                    }
                }
                return contentTypeCollection;
            }
        }

        public bool ContentTypesEnabled
        {
            get { return base.DataCache.GetProperty<bool>("ContentTypesEnabled"); }
            set
            {
                if (!ContentTypesEnabled.Equals(value))
                {
                    base.DataCache.AddChangedProperty("ContentTypesEnabled", value);
                }
            }
        }

        public Guid DefaultContentApprovalWorkflowId
        {
            get { return base.DataCache.GetProperty<Guid>("DefaultContentApprovalWorkflowId"); }
            set
            {
                if (!DefaultContentApprovalWorkflowId.Equals(value))
                {
                    base.DataCache.AddChangedProperty("DefaultContentApprovalWorkflowId", value);
                }
            }
        }

        public string DefaultDisplayFormUrl
        {
            get { return base.DataCache.GetProperty<string>("DefaultDisplayFormUrl"); }
            set
            {
                if (!string.Equals(DefaultDisplayFormUrl, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("DefaultDisplayFormUrl", value);
                }
            }
        }

        public string DefaultEditFormUrl
        {
            get { return base.DataCache.GetProperty<string>("DefaultEditFormUrl"); }
            set
            {
                if (!string.Equals(DefaultEditFormUrl, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("DefaultEditFormUrl", value);
                }
            }
        }

        public AveDefaultItemOpen DefaultItemOpen
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("DefaultItemOpen"))
                {
                    GetListAdvancedSettingProperties();
                }
                return base.DataCache.GetProperty<AveDefaultItemOpen>("DefaultItemOpen");
            }
            set { base.DataCache.AddChangedProperty("DefaultItemOpen", (int)value); }
        }

        public bool DefaultItemOpenUseListSetting
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("DefaultItemOpenUseListSetting"))
                {
                    GetListAdvancedSettingProperties();
                }
                return base.DataCache.GetProperty<bool>("DefaultItemOpenUseListSetting");
            }
            set
            {
                if (!DefaultItemOpenUseListSetting.Equals(value))
                {
                    base.DataCache.AddChangedProperty("DefaultItemOpenUseListSetting", value);
                }
            }
        }

        public string DefaultNewFormUrl
        {
            get { return base.DataCache.GetProperty<string>("DefaultNewFormUrl"); }
            set
            {
                if (!string.Equals(DefaultNewFormUrl, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("DefaultNewFormUrl", value);
                }
            }
        }

        public string DefaultViewUrl
        {
            get { return base.DataCache.GetProperty<string>("DefaultViewUrl"); }
        }

        public string Description
        {
            get { return base.DataCache.GetProperty<string>("Description"); }
            set
            {
                if (!string.Equals(Description, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("Description", value);
                }
            }
        }

        public AveDraftVisibilityType DraftVersionVisibility
        {
            get { return (AveDraftVisibilityType)base.DataCache.GetProperty<int>("DraftVersionVisibility"); }
            set { base.DataCache.AddChangedProperty("DraftVersionVisibility", (int)value); }
        }

        public string Direction
        {
            get { return base.DataCache.GetProperty<string>("Direction"); }
            set
            {
                if (!string.Equals(Direction, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("Direction", value);
                }
            }
        }

        public bool DisableGridEditing
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("DisableGridEditing"))
                {
                    GetListAdvancedSettingProperties();
                }
                return base.DataCache.GetProperty<bool>("DisableGridEditing");
            }
            set
            {
                if (!DisableGridEditing.Equals(value))
                {
                    base.DataCache.AddChangedProperty("DisableGridEditing", value);
                }
            }
        }

        public string EmailAlias
        {
            get { return base.DataCache.GetProperty<string>("EmailAlias"); }
            set
            {
                if (!string.Equals(EmailAlias, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("EmailAlias", value);
                }
            }
        }

        public bool EnableAssignToEmail
        {
            get { return base.DataCache.GetProperty<bool>("EnableAssignToEmail"); }
            set
            {
                if (!EnableAssignToEmail.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableAssignToEmail", value);
                }
            }
        }

        public bool EnableAttachments
        {
            get { return base.DataCache.GetProperty<bool>("EnableAttachments"); }
            set
            {
                if (!EnableAttachments.Equals(value) || EnableAttachments)
                {
                    base.DataCache.AddChangedProperty("EnableAttachments", value);
                }
            }
        }

        public bool EnforceDataValidation
        {
            get { return base.DataCache.GetProperty<bool>("EnforceDataValidation"); }
            set
            {
                if (!EnforceDataValidation.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnforceDataValidation", value);
                }
            }
        }

        public bool EnableDeployingList
        {
            get { return base.DataCache.GetProperty<bool>("EnableDeployingList"); }
            set
            {
                if (!EnableDeployingList.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableDeployingList", value);
                }
            }
        }

        public bool EnableDeployWithDependentList
        {
            get { return base.DataCache.GetProperty<bool>("EnableDeployWithDependentList"); }
            set
            {
                if (!EnableDeployWithDependentList.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableDeployWithDependentList", value);
                }
            }
        }

        public bool EnableFolderCreation
        {
            get { return base.DataCache.GetProperty<bool>("EnableFolderCreation"); }
            set
            {
                if (!EnableFolderCreation.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableFolderCreation", value);
                }
            }
        }

        public bool EnableMinorVersions
        {
            get { return base.DataCache.GetProperty<bool>("EnableMinorVersions"); }
            set
            {
                if (!EnableMinorVersions.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableMinorVersions", value);
                }
            }
        }

        public bool EnableModeration
        {
            get { return base.DataCache.GetProperty<bool>("EnableModeration"); }
            set
            {
                if (!EnableModeration.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableModeration", value);
                }
            }
        }

        public bool EnablePeopleSelector
        {
            get { return base.DataCache.GetProperty<bool>("EnablePeopleSelector"); }
            set
            {
                if (!EnablePeopleSelector.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnablePeopleSelector", value);
                }
            }
        }

        public bool EnableResourceSelector
        {
            get { return base.DataCache.GetProperty<bool>("EnableResourceSelector"); }
            set
            {
                if (!EnableResourceSelector.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableResourceSelector", value);
                }
            }
        }

        public bool EnableSchemaCaching
        {
            get { return base.DataCache.GetProperty<bool>("EnableSchemaCaching"); }
            set
            {
                if (!EnableSchemaCaching.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableSchemaCaching", value);
                }
            }
        }

        public bool EnableSyndication
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("EnableSyndication"))
                {
                    GetListRssProperties();
                    if (base.DataCache.IsPropertyAvailable("RootFolderRssProperties"))
                    {
                        (RootFolder as AveFolder).DataCache.PropertiesCache["Properties" + AveObjectModelConstant.ObjectPropertySuffix] = base.DataCache.GetProperty<Hashtable>("RootFolderRssProperties");
                    }
                }
                return base.DataCache.GetProperty<bool>("EnableSyndication");
            }
            set
            {
                if (!EnableSyndication.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableSyndication", value);
                }
            }
        }

        public bool EnableThrottling
        {
            get { return base.DataCache.GetProperty<bool>("EnableThrottling"); }
            set
            {
                if (!EnableThrottling.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableThrottling", value);
                }
            }
        }

        public bool EnableVersioning
        {
            get { return base.DataCache.GetProperty<bool>("EnableVersioning"); }
            set
            {
                if (!EnableVersioning.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableVersioning", value);
                }
            }
        }

        public bool ExcludeFromOfflineClient
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("ExcludeFromOfflineClient"))
                {
                    GetListAdvancedSettingProperties();
                }
                return base.DataCache.GetProperty<bool>("ExcludeFromOfflineClient");
            }
            set
            {
                if (!ExcludeFromOfflineClient.Equals(value))
                {
                    base.DataCache.AddChangedProperty("ExcludeFromOfflineClient", value);
                }
            }
        }

        public IAveEventReceiverDefinitionCollection EventReceivers
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("EventReceivers"))
                {
                    Dictionary<string, object> eventReceiversProperties = mRequest.GetEventReceiverDefinitions(ParentWeb.ServerRelativeUrl, DefaultViewUrl, Title, ID, "list.eventReceivers");
                    AveEventReceiverDefinitionCollection eventReceiverDefinitionCol = null;
                    if (eventReceiversProperties != null)
                    {
                        eventReceiverDefinitionCol = new AveEventReceiverDefinitionCollection(mParentWeb, this, mRequest, "list.eventReceivers", eventReceiversProperties);
                    }
                    base.DataCache.PropertiesCache["EventReceivers"] = eventReceiverDefinitionCol;
                    return eventReceiverDefinitionCol;
                }
                return base.DataCache.GetProperty<IAveEventReceiverDefinitionCollection>("EventReceivers");
            }
        }

        public string EventSinkAssembly
        {
            get { return base.DataCache.GetProperty<string>("EventSinkAssembly"); }
            set
            {
                if (!string.Equals(EventSinkAssembly, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("EventSinkAssembly", value);
                }
            }
        }

        public IAveFieldCollection Fields
        {
            get
            {
                AveFieldCollection fields = null;
                lock (privateLockFields)
                {
                    if (base.DataCache.IsPropertyNotLoaded("Fields"))
                    {
                        fields = new AveFieldCollection(mParentWeb, this, "list.fields", null);
                        base.DataCache.PropertiesCache["Fields"] = fields;
                    }
                    else
                    {
                        fields = base.DataCache.GetProperty<AveFieldCollection>("Fields");
                        if (fields.IsCollectionDirty)
                        {
                            fields.UpdateCollectionInternally();
                        }
                    }
                }
                return fields;
            }
        }

        public bool ForceCheckout
        {
            get { return base.DataCache.GetProperty<bool>("ForceCheckout"); }
            set
            {
                if (!ForceCheckout.Equals(value))
                {
                    base.DataCache.AddChangedProperty("ForceCheckout", value);
                }
            }
        }

        public string GetPropertiesXmlForUncustomizedViews()
        {
            return base.DataCache.GetProperty<string>("GetPropertiesXmlForUnCustomizedViews");
        }

        public bool HasExternalDataSource
        {
            get { return base.DataCache.GetProperty<bool>("HasExternalDataSource"); }
        }

        public bool Hidden
        {
            get { return base.DataCache.GetProperty<bool>("Hidden"); }
            set
            {
                if (!Hidden.Equals(value))
                {
                    base.DataCache.AddChangedProperty("Hidden", value);
                }
            }
        }

        public Guid ID
        {
            get { return base.DataCache.GetProperty<Guid>("Id"); }
        }

        public string ImageUrl
        {
            get { return base.DataCache.GetProperty<string>("ImageUrl"); }
            set
            {
                if (!string.Equals(ImageUrl,value))
                {
                    base.DataCache.AddChangedProperty("ImageUrl", value);
                }
            }
        }

        public bool IsApplicationList
        {
            get { return base.DataCache.GetProperty<bool>("IsApplicationList"); }
            set
            {
                if (!IsApplicationList.Equals(value))
                {
                    base.DataCache.AddChangedProperty("IsApplicationList", value);
                }
            }
        }

        public bool IsCatalog
        {
            get { return base.DataCache.GetProperty<bool>("IsCatalog"); }
        }

        public bool IsSiteAssetsLibrary
        {
            get { return base.DataCache.GetProperty<bool>("IsSiteAssetsLibrary"); }
            set
            {
                if (!IsSiteAssetsLibrary.Equals(value))
                {
                    base.DataCache.AddChangedProperty("IsSiteAssetsLibrary", value);
                }
            }
        }

        public bool IrmEnabled
        {
            get { return base.DataCache.GetProperty<bool>("IrmEnabled"); }
            set
            {
                if (!IrmEnabled.Equals(value))
                {
                    base.DataCache.AddChangedProperty("IrmEnabled", value);
                }
            }
        }

        public bool IrmExpire
        {
            get { return base.DataCache.GetProperty<bool>("IrmExpire"); }
            set
            {
                if (!IrmExpire.Equals(value))
                {
                    base.DataCache.AddChangedProperty("IrmExpire", value);
                }
            }
        }

        public bool IrmReject
        {
            get { return base.DataCache.GetProperty<bool>("IrmReject"); }
            set
            {
                if (!IrmReject.Equals(value))
                {
                    base.DataCache.AddChangedProperty("IrmReject", value);
                }
            }
        }

        public IAveListItemCollection Items
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Items"))
                {
                    AveCamlQuery query = AveCamlQuery.CreateAllItemsQuery(true);
                    IAveListItemCollection listItemsCollection = null;
                    if (BaseTemplate == AveListTemplateType.ExternalList)
                    {
                        //external list doesn't have complete fields of sp list,
                        //can not use any normal query option
                        query.ViewXml = null;
                        query.QueryXml = null;
                        query.QueryOptionXml = null;
                        query.ViewFieldsXml = null;
                        query.FolderServerRelativeUrl = null;
                        listItemsCollection = GetItems(query);
                    }
                    else
                    {
                        listItemsCollection = GetItems(query);
                    }
                    base.DataCache.PropertiesCache["Items"] = listItemsCollection;
                    return listItemsCollection;
                }
                return base.DataCache.GetProperty<IAveListItemCollection>("Items");
            }
        }

        public int ItemCount
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("ItemCount"))
                {
                    base.DataCache.PropertiesCache["ItemCount"] = (Items == null) ? 0 : Items.Count;
                }
                return base.DataCache.GetProperty<int>("ItemCount");
            }
        }

        public DateTime LastItemDeletedDate
        {
            get { return base.DataCache.GetProperty<DateTime>("LastItemDeletedDate"); }
        }

        public DateTime LastItemModifiedDate
        {
            get { return base.DataCache.GetProperty<DateTime>("LastItemModifiedDate"); }
        }

        public int MajorWithMinorVersionsLimit
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("MajorWithMinorVersionsLimit"))
                {
                    GetListVersionLimited();
                }
                return base.DataCache.GetProperty<int>("MajorWithMinorVersionsLimit");
            }
            set
            {
                if (value > 0 && value <= 50000 && !MajorWithMinorVersionsLimit.Equals(value))
                {
                    base.DataCache.AddChangedProperty("MajorWithMinorVersionsLimit", value);
                }
            }
        }

        public int MajorVersionLimit
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("MajorVersionLimit"))
                {
                    GetListVersionLimited();
                }
                return base.DataCache.GetProperty<int>("MajorVersionLimit");
            }
            set
            {
                if (value == 0)
                {
                    base.DataCache.AddChangedProperty("MajorVersionLimit", 50000);
                }
                if (value > 0 && value <= 50000 && !MajorVersionLimit.Equals(value))
                {
                    base.DataCache.AddChangedProperty("MajorVersionLimit", value);
                }
            }
        }

        public bool MultipleDataList
        {
            get { return base.DataCache.GetProperty<bool>("MultipleDataList"); }
            set
            {
                if (!MultipleDataList.Equals(value))
                {
                    base.DataCache.AddChangedProperty("MultipleDataList", value);
                }
            }
        }

        public bool NavigateForFormsPages
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("NavigateForFormsPages"))
                {
                    GetListAdvancedSettingProperties();
                }
                return base.DataCache.GetProperty<bool>("NavigateForFormsPages");
            }
            set
            {
                if (!NavigateForFormsPages.Equals(value))
                {
                    base.DataCache.AddChangedProperty("NavigateForFormsPages", value);
                }
            }
        }

        public bool EnableManagedIndexes
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("EnableManagedIndexes"))
                {
                    GetListAdvancedSettingProperties();
                }
                return base.DataCache.GetProperty<bool>("EnableManagedIndexes");
            }
            set
            {
                if (!EnableManagedIndexes.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableManagedIndexes", value);
                }
            }
        }

        public bool NoCrawl
        {
            get { return base.DataCache.GetProperty<bool>("NoCrawl"); }
            set
            {
                if (!NoCrawl.Equals(value))
                {
                    base.DataCache.AddChangedProperty("NoCrawl", value);
                }
            }
        }

        public bool OnQuickLaunch
        {
            get { return base.DataCache.GetProperty<bool>("OnQuickLaunch"); }
            set
            {
                if (!OnQuickLaunch.Equals(value))
                {
                    base.DataCache.AddChangedProperty("OnQuickLaunch", value);
                }
            }
        }

        public IAveWeb ParentWeb
        {
            get { return base.DataCache.GetProperty<IAveWeb>("ParentWeb"); }
        }

        public string ParentWebUrl
        {
            get { return ParentWeb.ServerRelativeUrl; }
        }

        public int ReadSecurity
        {
            get { return base.DataCache.GetProperty<int>("ReadSecurity"); }
            set
            {
                if (!ReadSecurity.Equals(value))
                {
                    base.DataCache.AddChangedProperty("ReadSecurity", value);
                }
            }
        }

        public IAveFolder RootFolder
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("RootFolder"))
                {
                    Dictionary<string, object> folderProperties = base.DataCache.GetProperty<Dictionary<string, object>>("RootFolder" + AveObjectModelConstant.ObjectPropertySuffix);

                    #region 在此处加载Rss Properties会造成效率问题，且Rss Properties是通过模拟Http request支持的，对于这类属性可以不支持，所以这个属性暂时去掉不支持。

                    //try
                    //{
                    //    base.DataCache.AddPropertyies(mRequest.GetListRssProperties(this.ParentWebUrl, this.ID));
                    //}
                    //catch (AveSecurityTrimingException ex)
                    //{
                    //    mLogger.Warn("An error occurred while get list rssproperties.listid: {0}", this.ID, ex);
                    //    //throw ex;
                    //    //contribute level没有权限取得ListRssProperty
                    //}

                    #endregion

                    if (base.DataCache.IsPropertyAvailable("RootFolderRssProperties"))
                    {
                        folderProperties["Properties"] = base.DataCache.GetProperty<Hashtable>("RootFolderRssProperties");
                    }
                    AveFolder rootFolder = new AveFolder(mRequest, mParentWeb, this, null, folderProperties);
                    base.DataCache.PropertiesCache["RootFolder"] = rootFolder;
                }
                return base.DataCache.GetProperty<IAveFolder>("RootFolder");
            }
        }

        public bool RootWebOnly
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("RootWebOnly"))
                {
                    base.DataCache.PropertiesCache["RootWebOnly"] = (Flags & 0x4000L) != 0L;
                }
                return base.DataCache.GetProperty<bool>("RootWebOnly");
            }
            set
            {
                if (!RootWebOnly.Equals(value))
                {
                    base.DataCache.AddChangedProperty("RootWebOnly", value);
                }
            }
        }

        public string SchemaXml
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("SchemaXml"))
                {
                    string schemal = mRequest.GetListSchemalXml(ParentWebUrl, ID, Title);
                    AveClientCacheHandler.WriteSchemaXml(schemal, mParentWeb.CacheHandlerId, mParentWeb.ID.ToString(), ID.ToString(), ID.ToString(), SchemaType.List);
                }
                return AveClientCacheHandler.GetSchemaXml(mParentWeb.CacheHandlerId, mParentWeb.ID.ToString(), ID.ToString(), ID.ToString(), SchemaType.List);
            }
        }

        public string SendToLocationName
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("SendToLocationName"))
                {
                    GetListAdvancedSettingProperties();
                }
                return base.DataCache.GetProperty<string>("SendToLocationName");
            }
            set
            {
                if (!string.Equals(SendToLocationName, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("SendToLocationName", value);
                }
            }
        }

        public string SendToLocationUrl
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("SendToLocationUrl"))
                {
                    GetListAdvancedSettingProperties();
                }
                return base.DataCache.GetProperty<string>("SendToLocationUrl");
            }
            set
            {
                if (!string.Equals(SendToLocationUrl, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("SendToLocationUrl", value);
                }
            }
        }

        public bool ServerTemplateCanCreateFolders
        {
            get { return base.DataCache.GetProperty<bool>("ServerTemplateCanCreateFolders"); }
        }

        public Guid TemplateFeatureId
        {
            get { return base.DataCache.GetProperty<Guid>("TemplateFeatureId"); }
        }

        public string Title
        {
            get { return base.DataCache.GetProperty<string>("Title"); }
            set
            {
                if (!string.Equals(Title, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("Title", value);
                }
            }
        }

        public string ValidationFormula
        {
            get { return base.DataCache.GetProperty<string>("ValidationFormula"); }
            set
            {
                if (!string.Equals(ValidationFormula, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("ValidationFormula", value);
                }
            }
        }

        public string ValidationMessage
        {
            get { return base.DataCache.GetProperty<string>("ValidationMessage"); }
            set
            {
                if (!string.Equals(ValidationMessage, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("ValidationMessage", value);
                }
            }
        }

        public IAveViewCollection Views
        {
            get
            {
                lock (privateLock)
                {
                    if (base.DataCache.IsPropertyNotLoaded("Views"))
                    {
                        Dictionary<string, object> viewsDic = mRequest.GetViews(mParentWeb.ServerRelativeUrl, Title, ID);
                        AveViewCollection views = new AveViewCollection(this, mRequest, viewsDic);
                        base.DataCache.PropertiesCache["Views"] = views;
                    }
                    return base.DataCache.GetProperty<IAveViewCollection>("Views");
                }
            }
        }

        public int WriteSecurity
        {
            get { return base.DataCache.GetProperty<int>("WriteSecurity"); }
            set
            {
                if (!WriteSecurity.Equals(value))
                {
                    base.DataCache.AddChangedProperty("WriteSecurity", value);
                }
            }
        }

        public IAveAlertTemplate AlertTemplate
        {
            get { return base.DataCache.GetProperty<IAveAlertTemplate>("AlertTemplate"); }
            set { base.DataCache.AddChangedProperty("AlertTemplate", value); }
        }

        public IAveView DefaultView
        {
            get
            {
                IAveViewCollection vs = Views;
                if (vs.Count > 0)
                {
                    foreach (IAveView view in vs)
                    {
                        if (view.DefaultView)
                        {
                            return view;
                        }
                    }
                    return null;
                }
                return null;
            }
        }

        public void EnsureRssSettings()
        {
            throw new NotImplementedException();
        }

        public string EventSinkClass
        {
            get { return base.DataCache.GetProperty<string>("EventSinkClass"); }
            set
            {
                if (!string.Equals(EventSinkClass, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("EventSinkClass", value);
                }
            }
        }

        public string EventSinkData
        {
            get { return base.DataCache.GetProperty<string>("EventSinkData"); }
            set
            {
                if (!string.Equals(EventSinkData, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("EventSinkData", value);
                }
            }
        }

        public IAveFieldIndexCollection FieldIndexes
        {
            get { return base.DataCache.GetProperty<IAveFieldIndexCollection>("FieldIndexes"); }
        }

        public IAveFormCollection Forms
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Forms"))
                {
                    Dictionary<string, object> formsPro = mRequest.GetForms(ParentWeb.ServerRelativeUrl, Title, ID);
                    AveFormCollection forms = new AveFormCollection(formsPro);
                    base.DataCache.PropertiesCache["Forms"] = forms;
                }
                return base.DataCache.GetProperty<IAveFormCollection>("Forms");
            }
        }

        public IAveAlertTemplate SmsAlertTemplate
        {
            get { return base.DataCache.GetProperty<IAveAlertTemplate>("SmsAlertTemplate"); }
            set { base.DataCache.AddChangedProperty("SmsAlertTemplate", value); }
        }

        public IAveWorkflowAssociationCollection WorkflowAssociations
        {
            get
            {
                lock (privateLockWorkflowAssociations)
                {
                    if (base.DataCache.IsPropertyNotLoaded("WorkflowAssociations"))
                    {
                        //Dictionary<string, object> workflowsPro = mRequest.GetWorkflowAssociations(this.ParentWeb.ServerRelativeUrl, this.Title, this.ID, "list.workflow", null);
                        AveWorkflowAssociationCollection workflows = new AveWorkflowAssociationCollection(ParentWeb, this, null, "list.workflow");
                        base.DataCache.PropertiesCache["WorkflowAssociations"] = workflows;
                    }
                    else if (base.DataCache.GetProperty<AveWorkflowAssociationCollection>("WorkflowAssociations").IsDirty)
                    {
                        base.DataCache.GetProperty<AveWorkflowAssociationCollection>("WorkflowAssociations").UpdateCollectionInternally();
                    }
                    return base.DataCache.GetProperty<IAveWorkflowAssociationCollection>("WorkflowAssociations");
                }
            }
        }

        public IAveListItem AddItem(string folderUrl, AveFileSystemObjectType underlyingObjectType)
        {
            return AddItem(folderUrl, underlyingObjectType, default(string));
        }

        public IAveListItem AddItem(string folderUrl, AveFileSystemObjectType underlyingObjectType, string leafName)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic["folderUrl"] = folderUrl;
            dic["FileSystemObjectType"] = underlyingObjectType;
            dic["leafName"] = leafName;
            var newItem = new AveListItem(mRequest, ParentWeb, this, dic, true);
            if (base.DataCache.IsPropertyAvailable("Items"))
            {
                (Items as AveListItemCollection).ListData.Add(newItem);
            }
            return newItem;
        }

        public IAveListItem AddItem(AveItemCreationInformation itemCreationInfo)
        {
            return AddItem(itemCreationInfo.FolderUrl, itemCreationInfo.UnderlyingObjectType, itemCreationInfo.LeafName);
        }

        public void Delete()
        {
            mRequest.DeleteList(ParentWeb.ServerRelativeUrl, Title, ID);
            (ParentWeb.Lists as AveListCollection).ListData.Remove(this);
        }

        public IAveListItem GetItemById(int id)
        {
            Dictionary<string, object> itemPro = mRequest.GetItem(ParentWeb.ServerRelativeUrl, Title, ID, id, default(Guid));
            return new AveListItem(mRequest, ParentWeb, this, itemPro, false);
        }

        public IAveListItem GetItemByGuid(Guid tp_Guid)
        {
            //TODO: Replace the method with Items.GetItemByGuid
            Dictionary<string, object> itemProperties = mRequest.GetItemByGuid(ParentWeb.ID, ID, tp_Guid);
            return new AveListItem(mRequest, ParentWeb, this, itemProperties, false);
        }

        public IAveListItem GetItemById(string id)
        {
            return GetItemById(int.Parse(id));
        }

        /// <summary>
        /// Get sub/root folder in current list
        /// </summary>
        /// <param name="serverRelativeUrl">folder ServeRelativUrl</param>
        /// <returns></returns>
        public IAveFolder GetFolder(string serverRelativeUrl)
        {
            Dictionary<string, object> folderProperties = null;
            folderProperties = mRequest.GetFolder(ParentWeb.ServerRelativeUrl, Title, ID, serverRelativeUrl);
            return new AveFolder(mRequest, ParentWeb, this, null, folderProperties);
        }

        public IAveListItemCollection GetItems(AveCamlQuery camlQuery)
        {
            Dictionary<string, object> items = mRequest.GetItems(mParentWeb.ServerRelativeUrl, Title, ID, camlQuery.ToStringArray());
            AveListItemCollection listItemsCollection = new AveListItemCollection(mRequest, ParentWeb, this, false, items);
            return listItemsCollection;
        }

        public IAveListItemCollection GetItemsForRecords(AveCamlQuery camlQuery)
        {
            Dictionary<string, object> items = mRequest.GetItemsForRecords(mParentWeb.ServerRelativeUrl, Title, ID, camlQuery.ToStringArray());
            AveListItemCollection listItemsCollection = new AveListItemCollection(mRequest, ParentWeb, this, false, items);
            return listItemsCollection;
        }

        public IAveListItemCollection GetItems(IAveQuery query)
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            base.DataCache.AddChangedProperty("ListType", (int)BaseTemplate);
            string title = base.DataCache.PropertiesCache["Title"].ToString();
            Dictionary<string, object> updateProperties = mRequest.UpdateList(ParentWeb.ServerRelativeUrl, title, ID, base.DataCache.ChangedProperties);
            if (updateProperties.ContainsKey("SchemaXml"))
            {
                AveClientCacheHandler.WriteSchemaXml(updateProperties["SchemaXml"].ToString(), mParentWeb.CacheHandlerId, mParentWeb.ID.ToString(), ID.ToString(), ID.ToString(), SchemaType.List);
                updateProperties.Remove("SchemaXml");
            }
            base.DataCache.UpdateProperties(updateProperties);
        }

        public IAveListItem GetItemByUniqueId(Guid uniqueId)
        {
            Dictionary<string, object> itemPro = mRequest.GetItemByUniqueId(this.ParentWeb.ID, this.ID, uniqueId);
            return itemPro != null ? new AveListItem(mRequest, this.ParentWeb, this, itemPro, false) : null;
        }

        public AveListInfo GetListInfo()
        {
            AveListInfo listInfo = new AveListInfo();
            listInfo.Id = ID;
            listInfo.Title = Title;
            listInfo.BaseTemplate = (int)BaseTemplate;
            listInfo.TemplateFeatureId = TemplateFeatureId;
            listInfo.BaseType = (int)BaseType;
            listInfo.Description = Description;
            string url = RootFolder.ServerRelativeUrl.Substring(ParentWeb.RootFolder.ServerRelativeUrl.Length).Trim('/');
            listInfo.Url = ParentWeb.Url.TrimEnd('/') + "/" + url;
            listInfo.ServerRelativeUrl = RootFolder.ServerRelativeUrl;
            if (BaseTemplate == AveListTemplateType.ExternalList)
            {
                listInfo.DataSourceXml = (string)AveAssemblyUtility.InvokeMethod(DataSource, DataSource.GetType(), "ToXml", null);
            }
            listInfo.RootWebOnly = RootWebOnly;
            return listInfo;
        }

        public string GetListViewSchema(Guid siteId, Guid listId)
        {
            IAveView defaultView = DefaultView;
            if (defaultView != null)
            {
                return defaultView.ViewFields.SchemaXml;
            }
            return string.Empty;
        }

        public void UpdateListSetting(AveListSettingInfo listSettingInfo)
        {
            if (listSettingInfo.Description.IsAvailable)
            {
                Description = listSettingInfo.Description.Value != null ? listSettingInfo.Description.Value : "";
            }
            if (listSettingInfo.DefaultItemOpen.IsAvailable)
            {
                if (listSettingInfo.DefaultItemOpen.Value == 0)
                {
                    DefaultItemOpenUseListSetting = false;
                }
                else if (listSettingInfo.DefaultItemOpen.Value == 1)
                {
                    DefaultItemOpen = AveDefaultItemOpen.Browser;
                }
                else
                {
                    DefaultItemOpen = AveDefaultItemOpen.PreferClient;
                }
            }
            if ((BaseType != AveBaseType.DocumentLibrary) && (BaseType != AveBaseType.Survey)
                && listSettingInfo.EnableAttachments.IsAvailable && (listSettingInfo.EnableAttachments != null))
            {
                EnableAttachments = listSettingInfo.EnableAttachments.Value;
            }
            if (ServerTemplateCanCreateFolders && listSettingInfo.EnableFolderCreation.IsAvailable && listSettingInfo.EnableFolderCreation != null)
            {
                EnableFolderCreation = listSettingInfo.EnableFolderCreation.Value;
            }
            if (BaseType == AveBaseType.DocumentLibrary && listSettingInfo.EnableMinorVersions != null)
            {
                if (listSettingInfo.EnableMinorVersions.IsAvailable)
                {
                    EnableMinorVersions = listSettingInfo.EnableMinorVersions.Value;
                }
                if (listSettingInfo.EventSinkAssembly.IsAvailable)
                {
                    EventSinkAssembly = listSettingInfo.EventSinkAssembly.Value;
                }
            }
            if (BaseType != AveBaseType.Survey && listSettingInfo.EnableVersioning.IsAvailable && listSettingInfo.EnableVersioning != null)
            {
                EnableVersioning = listSettingInfo.EnableVersioning.Value;
            }
            if (BaseType == AveBaseType.Survey && listSettingInfo.AllowMultiResponses.IsAvailable && listSettingInfo.AllowMultiResponses != null)
            {
                AllowMultiResponses = listSettingInfo.AllowMultiResponses.Value;
            }

            if (listSettingInfo.ForceCheckout.IsAvailable)
            {
                if (listSettingInfo.ForceCheckout != null)
                {
                    if (!HasExternalDataSource && BaseTemplate == AveListTemplateType.DocumentLibrary)
                    {
                        ForceCheckout = listSettingInfo.ForceCheckout.Value;
                    }
                }
                else
                {
                    ForceCheckout = listSettingInfo.ForceCheckout.Value;
                }
            }

            if (listSettingInfo.ValidationMessage.IsAvailable && listSettingInfo.ValidationMessage.Value != null && listSettingInfo.ValidationMessage.Value.Length <= 0x400L)
            {
                ValidationMessage = listSettingInfo.ValidationMessage.Value;
            }
            else if (!HasExternalDataSource)
            {
                NoCrawl = false;
            }

            if (listSettingInfo.ReadSecurity.IsAvailable && listSettingInfo.ReadSecurity != null)
            {
                if (listSettingInfo.ReadSecurity.Value == 1 || listSettingInfo.ReadSecurity.Value == 2)
                {
                    ReadSecurity = listSettingInfo.ReadSecurity.Value;
                }
            }
            if (listSettingInfo.WriteSecurity.IsAvailable && listSettingInfo.WriteSecurity != null)
            {
                if (listSettingInfo.WriteSecurity.Value == 1 || listSettingInfo.WriteSecurity.Value == 2 || listSettingInfo.WriteSecurity.Value == 4)
                {
                    WriteSecurity = listSettingInfo.WriteSecurity.Value;
                }
            }

            if (listSettingInfo.DraftVersionVisibility.IsAvailable)
            {
                AveDraftVisibilityType temType = (AveDraftVisibilityType)listSettingInfo.DraftVersionVisibility.Value;
                if (temType == AveDraftVisibilityType.Approver || temType == AveDraftVisibilityType.Author || temType == AveDraftVisibilityType.Reader)
                {
                    DraftVersionVisibility = (AveDraftVisibilityType)listSettingInfo.DraftVersionVisibility.Value;
                }
            }

            if (listSettingInfo.ThumbnailSize.IsAvailable && listSettingInfo.ThumbnailSize.Value > 0 && this is IAveDocumentLibrary)
            {
                IAveDocumentLibrary spDocLibrary = (IAveDocumentLibrary)this;
                spDocLibrary.ThumbnailsEnabled = true;
                spDocLibrary.ThumbnailSize = listSettingInfo.ThumbnailSize.Value.Value;
            }
            if (listSettingInfo.SendToLocation.IsAvailable && !string.IsNullOrEmpty(listSettingInfo.SendToLocation.Value))
            {
                int temIndex = listSettingInfo.SendToLocation.Value.IndexOf('|');
                SendToLocationName = temIndex > 0 ? listSettingInfo.SendToLocation.Value.Substring(0, temIndex) : listSettingInfo.SendToLocation.Value;
                SendToLocationUrl = temIndex > 0 ? listSettingInfo.SendToLocation.Value.Substring(temIndex + 1) : string.Empty;
            }
            if ((EnableMinorVersions || EnableModeration) && listSettingInfo.MaxMajorwithMinorVersionCount.IsAvailable &&
                listSettingInfo.MaxMajorwithMinorVersionCount.Value > 0 && listSettingInfo.MaxMajorwithMinorVersionCount.Value < 0xc350)
            {
                MajorWithMinorVersionsLimit = listSettingInfo.MaxMajorwithMinorVersionCount.Value.HasValue ? listSettingInfo.MaxMajorwithMinorVersionCount.Value.Value : AveAllListsTableColumnValue.MaxMajorwithMinorVersionCount;
            }
            if (EnableVersioning && listSettingInfo.MaxMajorVersionCount.IsAvailable
                && listSettingInfo.MaxMajorVersionCount.Value > 0 && listSettingInfo.MaxMajorVersionCount.Value < 0xc350)
            {
                MajorVersionLimit = listSettingInfo.MaxMajorVersionCount.Value.HasValue ? listSettingInfo.MaxMajorVersionCount.Value.Value : AveAllListsTableColumnValue.MaxMajorVersionCount;
            }
            if (HasUniqueRoleAssignments && listSettingInfo.AnonymousPermMask64.IsAvailable)
            {
                if (AnonymousPermMask64 != (AveBasePermissions)listSettingInfo.AnonymousPermMask64.Value)
                {
                    AnonymousPermMask64 = (AveBasePermissions)listSettingInfo.AnonymousPermMask64.Value;
                }
            }
            string[] dProp =
            {
                "AllowDeletion", "EnableAssignToEmail", "EnableDeployingList", "EnableDeployWithDependentList", "EnforceDataValidation",
                "ExcludeFromOfflineClient", "IrmEnabled", "IrmExpire", "IrmReject", "EnablePeopleSelector", "EnableResourceSelector", "EnableSchemaCaching", "EnableSyndication",
                "EnableThrottling", "DisableGridEditing", "NavigateForFormsPages", "EmailAlias", "SendToLocationName", "SendToLocationUrl"
            };
            string[] sProp = { "Hidden", "OnQuickLaunch", "MultipleDataList", "EnableModeration", "ContentTypesEnabled", "NoCrawl", "ValidationFormula" };
            CopyObjectAve(this, listSettingInfo, sProp, dProp);
            Update();
        }

        #endregion

        #region IAveSecurableObject Members

        public override IAveRoleAssignmentCollection RoleAssignments
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("RoleAssignments"))
                {
                    if (!HasUniqueRoleAssignments)
                    {
                        return ParentWeb.RoleAssignments;
                    }
                    Dictionary<string, object> roleAssignmentsProperties = mRequest.GetRoleAssignments(mParentWeb.ServerRelativeUrl, DefaultViewUrl, Title, ID, -1, "list.roleAssignments");
                    AveRoleAssignmentCollection roleAssignments = new AveRoleAssignmentCollection(this, mRequest, ParentWeb.Site as AveSite, ParentWeb as AveWeb, this, -1, "list.roleAssignments", roleAssignmentsProperties);
                    base.DataCache.PropertiesCache["RoleAssignments"] = roleAssignments;
                    return roleAssignments;
                }
                return base.DataCache.GetProperty<IAveRoleAssignmentCollection>("RoleAssignments");
            }
        }

        protected override IAveRoleAssignmentCollection InternalBreakRoleInheritance(bool copyRoleAssignments, bool clearSubscopes)
        {
            Dictionary<string, object> roleAssignmentsProperties = mRequest.BreakRoleInheritance(mParentWeb.ServerRelativeUrl, DefaultViewUrl, Title, ID, -1, copyRoleAssignments, clearSubscopes, "list.roleAssignments");
            AveRoleAssignmentCollection roleAssignmentCol = new AveRoleAssignmentCollection(this, mRequest, ParentWeb.Site as AveSite, ParentWeb as AveWeb, this, -1, "list.roleAssignments", roleAssignmentsProperties);
            return roleAssignmentCol;
        }

        protected override IAveRoleAssignmentCollection InternalResetRoleInheritance()
        {
            Dictionary<string, object> roleAssignmentsProperties = mRequest.ResetRoleInheritance(mParentWeb.ServerRelativeUrl, DefaultViewUrl, Title, ID, -1, "list.roleAssignments");
            AveRoleAssignmentCollection roleAssignmentCol = new AveRoleAssignmentCollection(this, mRequest, ParentWeb.Site as AveSite, ParentWeb as AveWeb, this, -1, "list.roleAssignments", roleAssignmentsProperties);
            return roleAssignmentCol;
        }

        public override void RemoveRoleAssignment(int principalId)
        {
            if (RoleAssignments.GetByPrincipalId(principalId) != null)
            {
                mRequest.DeleteRoleAssignment(mParentWeb.ServerRelativeUrl, DefaultViewUrl, Title, ID, -1, principalId, "list.roleAssignments");
            }
        }

        #endregion

        public bool IsExceedListViewLookupThreshold
        {
            get
            {
                if (mIsExceedListViewLookupThreshold == null)
                {
                    lock (mIsExceedListViewLookupThresholdLock)
                    {
                        if (mIsExceedListViewLookupThreshold == null)
                        {
                            int lookupFieldCount = 0;
                            foreach (IAveField field in this.Fields)
                            {
                                if (BuiltInLookupColumn.Contains(field.ID))
                                {
                                    continue;
                                }
                                IAveFieldLookup lookupField = field as IAveFieldLookup;
                                if ((lookupField != null && !lookupField.IsDependentLookup)
                                    || field.Type == AveFieldType.WorkflowStatus)
                                {
                                    lookupFieldCount++;
                                }
                            }
                            mIsExceedListViewLookupThreshold = lookupFieldCount >= 12;
                        }
                    }
                }
                return mIsExceedListViewLookupThreshold.Value;
            }
        }

        public IAveUserCustomActionCollection UserCustomActions
        {
            get
            {
                return null;
            }
        }

        public AveListExperience ListExperienceOptions
        {
            get
            {
                return base.DataCache.GetProperty<AveListExperience>("ListExperienceOptions");
            }

            set
            {
                if (ListExperienceOptions != value)
                {
                    base.DataCache.AddChangedProperty("ListExperienceOptions", value);
                }
            }
        }

        public void PublicSharepointInfoPathList(IAveFile templateFile, int lcid, string listId, string contentTypeId)
        {
            mRequest.PublishSharepointList(this.ParentWeb.ServerRelativeUrl, templateFile as IAveFile, lcid, listId, contentTypeId);
        }

        internal bool IsVariationLabelsList()
        {
            return mParentWeb.VariationLabelListId == ID;
        }
        public bool IsRelationshipsList()
        {
            return mParentWeb.RelationshipsListId == ID;
        }

        public void SaveNintexForm(string formXml, string contentTypeId)
        {
            if (this.mParentWeb.Site.IsOnlineSite)
            {
                (mRequest ).SaveNintexForm(formXml, this.mParentWeb.Url, ID, contentTypeId);
            }
            else
            {
                throw new NotSupportedException("only support Online Nintex Workflow.");
            }
        }
        public void PublishNintexForm(string contentTypeId)
        {
            if (this.mParentWeb.Site.IsOnlineSite)
            {
                (mRequest ).PublishNintexForm(this.mParentWeb.Url, ID, contentTypeId);
            }
            else
            {
                throw new NotSupportedException("only support Online Nintex Workflow.");
            }
        }
        public Stream ExportNintexForm(string contentTypeId)
        {
            if (this.mParentWeb.Site.IsOnlineSite)
            {
                return (mRequest ).ExportNintexForm(this.mParentWeb.Url, ID, contentTypeId);
            }
            else
            {
                throw new NotSupportedException("only support Online Nintex Workflow.");
            }
        }

        public WorkflowStartOptionCache BackupWorkflowStartOption(string url, Guid webId, Guid listId)
        {
            return mRequest.BackupWorkflowStartOption(url, webId, listId);
        }

        public void RestoreWOrkflowStartOption(string url, Guid webId, Guid listId, WorkflowStartOptionCache cache)
        {
           mRequest.RestoreWorkflowStartOption(url, webId, listId, cache);
        }

        public bool CrawlNonDefaultViews
        {
            get
            {
                return base.DataCache.GetProperty<bool>("CrawlNonDefaultViews");
            }
            set
            {
                if (!CrawlNonDefaultViews.Equals(value))
                {
                    base.DataCache.AddChangedProperty("CrawlNonDefaultViews", value);
                }
            }
        }

    }

    [Flags]
    internal enum ListRequestSettingFlag
    {
        None = 0,
        AdvancedSetting = 1,
        ListVersion = 2,
        General = 4,
        EditViewSetting = 8,
        MetadataNavigation = 16,
        PerLocationView = 32,
        AccessRequests = 64,
        Rss = 128
    }
}