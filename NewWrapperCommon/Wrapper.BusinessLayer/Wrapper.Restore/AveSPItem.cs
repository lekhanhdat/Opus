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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Linq;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Core.SPBackupDto;
using AvePoint.Wrapper.Core.SPRestore;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.Wrapper.Restore
{
    [AveCodeReviewAttribute("2012/03/09", "Navy.Li@avepoint.com", "Bingkun.Wang@AvePoint.com",
        new string[] { CodeReviewConstants.CHECK_LIST_ID_CO_1, 
                       CodeReviewConstants.CHECK_LIST_ID_CO_6, 
                       CodeReviewConstants.CHECK_LIST_ID_FA_4 }, null, true)]
    public class AveSPItem : RestoreableObject, IAveSPItem, IDisposable, IReportable
    {
        private bool hasExcludedUrl = false;
        protected static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected AveSPSite mAveParentSite;
        protected AveSPWeb mParentWeb;
        protected AveSPList mAveSPList;
        protected IAveItem mAveItem = null;
        protected IAveBackupRestoreQueryService mQueryService;
        protected AveSPFolder mParentFolder;
        protected AveBaseItemInfo mBaseItemInfo = new AveBaseItemInfo();
        protected IAveTimeZone mAveTimeZone;
        protected IReport report = new AveWrapperReport();
        protected bool mIsMergeToFolder;
        public AveRelatedItemsInfo relatedItemsInfo;
        public IReport GetReport()
        {
            return report;
        }
        public AveSPSite ParentSite
        {
            get
            {
                return mAveParentSite;
            }
        }
        public AveSPWeb ParentWeb
        {
            get
            {
                return mParentWeb;
            }
        }
        public AveSPList ParentList
        {
            get
            {
                return mAveSPList;
            }
            set
            {
                mAveSPList = value;
            }
        }
        public AveSPFolder ParentFolder
        {
            get { return mParentFolder; }
        }
        public IAveWeb SPWeb
        {
            get
            {
                return mAveItem != null ? mAveItem.Web : null;
            }
            set
            {
                mAveItem.Web = value;
            }
        }
        public IAveListItem SPListItem
        {
            get
            {
                if (mAveItem != null)
                {
                    return mAveItem.ListItem;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                mAveItem.ListItem = value;
            }
        }
        public IAveBackupRestoreQueryService QueryService
        {
            get { return this.mQueryService; }
            set { this.mQueryService = value; }
        }
        public string Name
        {
            get { return mBaseItemInfo.Name; }
            set { mBaseItemInfo.Name = value; }
        }
        public string ScopeUrl
        {
            get { return mBaseItemInfo.ScopeUrl; }
            set { mBaseItemInfo.ScopeUrl = value; }
        }
        public Guid SiteId
        {
            get { return mBaseItemInfo.SiteId; }
            set { mBaseItemInfo.SiteId = value; }
        }
        public int Version
        {
            get { return mBaseItemInfo.Version; }
            set { mBaseItemInfo.Version = value; }
        }
        public int Level
        {
            get { return mBaseItemInfo.Level; }
            set { mBaseItemInfo.Level = value; }
        }
        public int RowId
        {
            get { return mBaseItemInfo.RowId; }
            set { mBaseItemInfo.RowId = value; }
        }
        public Guid Id
        {
            get { return mBaseItemInfo.GUID; }
            set { mBaseItemInfo.GUID = value; }
        }

        public bool IsNewCreated
        {
            get
            {
                return mBaseItemInfo.IsNewCreated;
            }
            set
            {
                mBaseItemInfo.IsNewCreated = value;
            }
        }
        /// <summary>
        ///For CM Import Folder
        /// </summary>
        public bool IsMergeToFolder
        {
            get
            {
                return mIsMergeToFolder;
            }
            set
            {
                mIsMergeToFolder = value;
            }
        }

        #region Obsolete field&property
        [Obsolete("use IsNewCreated instead, will remove later")]
        public bool IsNewCreatedDoc
        {
            get
            {
                return mBaseItemInfo.IsNewCreated;
            }
            set { mBaseItemInfo.IsNewCreated = value; }
        }
        [Obsolete("no use now, will remove later")]
        private AveObjectModelFactory mOMFactory;
        [Obsolete("no use now, will remove later")]
        private AveStorage mStorage;
        #region add for stub property
        [Obsolete("no use now, will remove later")]
        private bool mIsStubData = false;
        [Obsolete("no use now, will remove later")]
        private int mPicHeight;
        [Obsolete("no use now, will remove later")]
        private int mPicWidth;
        #endregion
        [Obsolete("no use now, will remove later")]
        private Dictionary<string, object> fieldsInMetaInfo = null;
        [Obsolete("no use now, will remove later")]
        private int mOriginalModerationStatus;
        [Obsolete("no use now, will remove later")]
        public AveStorage AveStorage
        {
            get { return mStorage; }
        }
        [Obsolete("no use now, will remove later")]
        public int RestoreVersion
        {
            get { return mBaseItemInfo.RestoreVersion; }
            set { mBaseItemInfo.RestoreVersion = value; }
        }
        [Obsolete("no use now, will remove later")]
        public bool IsStubData
        {
            get { return mIsStubData; }
        }
        [Obsolete("no use now, will remove later")]
        public Guid ScopeId
        {
            get { return mBaseItemInfo.ScopeId; }
            set { mBaseItemInfo.ScopeId = value; }
        }
        [Obsolete("no use now, will remove later")]
        public bool HasUniqueRoleAssignments
        {
            get { return mBaseItemInfo.HasUniqueRoleAssignments; }
        }
        [Obsolete("no use now, will remove later")]
        public int? InternalVersion
        {
            get { return mBaseItemInfo.InternalVersion; }
            set { mBaseItemInfo.InternalVersion = value; }
        }
        [Obsolete("no use now, will remove later")]
        public int OriginalModerationStatus
        {
            get { return mOriginalModerationStatus; }
            set { mOriginalModerationStatus = value; }
        }
        [Obsolete("no use now, will remove later")]
        private int mOldRowId;
        [Obsolete("no use now, will remove later")]
        public int OldRowId
        {
            get { return mOldRowId; }
        }

        [Obsolete("no use now, will remove later")]
        public bool IsCheckOut
        {
            get { return mBaseItemInfo.IsCheckOut; }
            set { mBaseItemInfo.IsCheckOut = value; }
        }
        [Obsolete("no use now, will remove later")]
        public bool IsVersion
        {
            get { return mBaseItemInfo.IsVersion; }
            set { mBaseItemInfo.IsVersion = value; }
        }
        [Obsolete("no use now, will remove later")]
        public string OwnerLoginName
        {
            get { return mAveItem.OwnerLoginName; }
        }
        #endregion


        [Obsolete("This constructor is only used for unit test")]
        internal AveSPItem()
        { }

        public AveSPItem(AveSPList aveSPList, IAveRestoreStream aveRestoreStream)
        {
            mAveParentSite = aveSPList.ParentSite;
            mParentWeb = aveSPList.ParentWeb;
            mAveSPList = aveSPList;
            mQueryService = aveSPList.QueryService;
            try
            {
                if (mAveSPList.ParentWeb.SPWeb.RegionalSettings != null)
                {
                    mAveTimeZone = mAveSPList.ParentWeb.SPWeb.RegionalSettings.TimeZone;
                }
            }
            catch (Exception ex)
            {
                //RegionalSettings is not available in Contributes Permissions in BPOS
                log.Log(AveLogLevel.INFO, "cannot get regional setting TimeZone, exception:{0}", ex.ToString());
            }
            mAveItem = mAveParentSite.ObjectModelFactory.CreateAveItem(mBaseItemInfo, mAveSPList.RootFolder, mParentWeb.SPWeb, mAveSPList.SPList);
            mBaseItemInfo.AveItem = mAveItem;
        }

        public AveSPItem(AveItemType type, AveSPFolder parentFolder, string name)
        {
            if (parentFolder.ParentList.ParentWeb.ReloadWebAndParentInternalForSPRequestTimeout(false))
            {
                if (!parentFolder.ParentList.IsSystemList)
                {
                    parentFolder.ParentList.ReloadList();
                    parentFolder.ReloadFolder(false);
                }
            }
            mAveParentSite = parentFolder.ParentSite;
            mParentWeb = parentFolder.ParentWeb;
            mAveSPList = parentFolder.ParentList;
            mParentFolder = parentFolder;
            mQueryService = parentFolder.QueryService;
            try
            {
                if (mAveSPList.ParentWeb.SPWeb.RegionalSettings != null)
                {
                    mAveTimeZone = mAveSPList.ParentWeb.SPWeb.RegionalSettings.TimeZone;
                }
            }
            catch (Exception ex)
            {
                //RegionalSettings is not available in Contributes Permissions in BPOS
                log.Log(AveLogLevel.INFO, "cannot get regional setting TimeZone, exception:{0}", ex.ToString());
            }
            switch (type)
            {
                case AveItemType.Document:
                    mBaseItemInfo = new AveDocumentInfo();
                    break;
                case AveItemType.ListItem:
                    mBaseItemInfo = new AveListItemInfo();
                    break;
                case AveItemType.Folder:
                    if (mAveSPList.SPList != null && mAveSPList.SPList.Title.Equals("NintexSnippets", StringComparison.OrdinalIgnoreCase) && mAveSPList.SPList.BaseTemplate == AveListTemplateType.NintexWrokflow)
                    {
                        int userId = -1;
                        if (int.TryParse(name, out userId) && userId > 0)
                        {
                            var newUser = mAveParentSite.SPMembers.FindMember(userId, true, false);
                            if (newUser != null && newUser.ID > 0)
                            {
                                name = newUser.ID.ToString();
                            }
                        }
                    }
                    mBaseItemInfo = new AveFolderInfo();
                    break;
                case AveItemType.Attachement:
                    mBaseItemInfo = new AveAttachmentInfo();
                    break;
                default:
                    mBaseItemInfo = new AveBaseItemInfo();
                    break;
            }
            mBaseItemInfo.Name = name;
            if (!string.IsNullOrEmpty(name))
            {
                int pos = name.IndexOf(':');
                if (pos >= 0)
                {
                    mBaseItemInfo.Name = name.Substring(0, pos);
                }
            }
            mBaseItemInfo.ItemType = type;
            InitBaseItemInfo();

            mAveItem = parentFolder.ParentSite.ObjectModelFactory.CreateAveItem(mBaseItemInfo, parentFolder.SPFolder, mAveSPList.ParentWeb.SPWeb, mAveSPList.SPList);
            mBaseItemInfo.AveItem = mAveItem;
        }

        private void InitBaseItemInfo()
        {
            if (mAveSPList.SPList != null)
            {
                mBaseItemInfo.ListId = mAveSPList.SPList.ID;
            }
            mBaseItemInfo.SiteId = mAveParentSite.SPSite.ID;
            mBaseItemInfo.ParentId = mParentFolder.Id;
            mBaseItemInfo.MappingManager = mAveParentSite.MappingManager;
            mBaseItemInfo.GetUserFromMapping = mAveParentSite.SPMembers.GetMappingUserLogin;
            mBaseItemInfo.KeepDefaultValue = mAveParentSite.KeepDefaultValue;
            mBaseItemInfo.VerifyItemMMSColumnValue = mAveParentSite.VerifyItemMMSColumnValue;
            mBaseItemInfo.KeepDestItemRowId = mAveParentSite.KeepDestItemRowId;
        }

        /// <summary>
        /// this constructor is now used for postAction, only for internal use, will change it access level to internal
        /// </summary>
        /// <param name="aveSite"></param>
        public AveSPItem(AveSPSite aveSite)
        {
            mQueryService = aveSite.QueryService;
            mAveParentSite = aveSite;
            mAveItem = mAveParentSite.ObjectModelFactory.CreateAveItem(mAveParentSite.SPSite);
        }

        public void AddItemMapping(int rowId)
        {
            //RestoreSocialListItem的方法应该放在Server层，不能在Wrapper层就restore完成，这样很多需要还原之后初始化的变量都没有初始化，很容易在后面引起问题
            //SF先暂时过滤SocialList Item    6.7重写相关逻辑
            if (mAveSPList.SPList != null && (int)mAveSPList.SPList.BaseTemplate != 550)//SocialList
            {
                mAveParentSite.MappingManager.SiteMappingManager.AddItemIdMapping(mParentFolder.ParentList.SPList.ID, rowId, mAveItem.ListItem.ID);
            }
        }

        public int GetCurrentUIVersion(Guid siteId, IAveListItem item)
        {
            return mAveItem.GetCurrentUIVersion(siteId, item);
        }

        private IAveFieldLookupValueCollection RestoreDataJunctionForLookupField(Dictionary<int, string> value, int originalVersion, IAveField field, ref bool allFind, AveXmlField xmlField)
        {
            IAveFieldLookupValueCollection lookupCol = null;
            var siteManager = mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager;
            AveLookupObject obj;
            if (siteManager.TryGetValueFromLookupFieldMapping(mParentFolder.ParentList.SPList.ID, field.ID, out obj))
            {
                try
                {
                    Guid oldListId = new Guid(obj.SourceListId);
                    var valueId = Guid.Empty;
                    if (siteManager.GetValueFromListIdMapping(oldListId, out valueId))
                    {
                        var valueInfo = new AveSourceFieldValueInfo();
                        var isSettingCol = false;
                        valueInfo.SourceFieldInfo = new AveSourceFieldInfo
                        {
                            SourceDisplayName = xmlField.Title,
                            SourceInternalName = xmlField.FieldInternalName,
                            SourceType = xmlField.Type
                        };
                        foreach (KeyValuePair<int, string> lookupValue in value)
                        {
                            //对于replicator的increment job,有可能还原了list,但是不需要还原它下面的item
                            if (siteManager.ContainsKeyForItemIdMapping(valueId))
                            {
                                allFind = false;
                                break;
                            }

                            string mappingValue = lookupValue.Key.ToString();
                            //add for custom mapping
                            if (xmlField.CustomFieldInfo != null && !string.IsNullOrEmpty(lookupValue.Value))
                            {
                                var mappingValueObject = mAveSPList.AveFields.CustomMappingValue(lookupValue.Value, field, xmlField.CustomFieldInfo, valueInfo, ref isSettingCol);
                                if (mappingValueObject == null)
                                {
                                    continue;
                                }
                                mappingValue = mappingValueObject.ToString();
                            }
                            int itemId;
                            if (!int.TryParse(mappingValue, out itemId))
                            {
                                allFind = false;
                                continue;
                            }
                            int tempListItemId = siteManager.GetMappingItemId(valueId, itemId);
                            if (tempListItemId != -1)
                            {
                                if (lookupCol == null)
                                {
                                    lookupCol = mAveParentSite.ObjectModelFactory.CreateFieldLookupValueCollection();
                                }
                                lookupCol.Add(mAveParentSite.ObjectModelFactory.CreateFieldLookupValue(tempListItemId, "Title"));
                                continue;
                            }
                            else
                            {
                                allFind = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        allFind = false;
                    }
                    if (!allFind)
                    {
                        if (xmlField != null && xmlField.CustomFieldInfo != null)
                        {
                            this.mAveParentSite.xmlFieldCache[field.ID] = xmlField;
                        }
                    }
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore dataJunction. name:{0}\n error message:{1}", field.ID, e));
                    //mLog.Warn(e, "An error occurred while restore datajunction. Name:{0}", de.Key);
                }
            }
            else
            {
                if (xmlField.TypeAsString.Equals("UserMulti", StringComparison.OrdinalIgnoreCase))
                {
                    var valueInfo = new AveSourceFieldValueInfo();
                    var isSettingCol = false;
                    valueInfo.SourceFieldInfo = new AveSourceFieldInfo
                    {
                        SourceDisplayName = xmlField.Title,
                        SourceInternalName = xmlField.FieldInternalName,
                        SourceType = xmlField.Type
                    };
                    foreach (KeyValuePair<int, string> itemId in value)
                    {
                        object mappingValue = itemId.Key;
                        mappingValue = mAveSPList.AveFields.CustomMappingValue(mappingValue, field, xmlField.CustomFieldInfo, valueInfo, ref isSettingCol);
                        if (mappingValue != null && !string.IsNullOrEmpty(mappingValue.ToString()))
                        {
                            if (lookupCol == null)
                            {
                                lookupCol = mAveParentSite.ObjectModelFactory.CreateFieldLookupValueCollection();
                            }
                            lookupCol.Add(mAveParentSite.ObjectModelFactory.CreateFieldLookupValue(Convert.ToInt32(mappingValue), "Title"));
                        }
                    }

                }
                else
                {
                    Guid listId = new Guid(((IAveFieldLookup)field).LookupList);
                    if (!listId.Equals(Guid.Empty) && value.Keys.Count != 0)
                    {
                        foreach (int itemId in value.Keys)
                        {
                            int destItemId = siteManager.GetMappingItemId(listId, itemId);
                            //如果lista在listb之前还原,就有可能找到正确的lookup关系
                            if (destItemId != -1)
                            {
                                //value = itemId.ToString();
                                //newdata[spField.InternalName] = value;
                                if (lookupCol == null)
                                {
                                    lookupCol = mAveParentSite.ObjectModelFactory.CreateFieldLookupValueCollection();
                                }
                                lookupCol.Add(mAveParentSite.ObjectModelFactory.CreateFieldLookupValue(destItemId, "Title"));
                            }
                            //如果没有找到正确的对应关系,将将其加入到PostAction中,需要注意此时lookupID传的是listId,而不是obj.List,所以在PostAction中还原的时候需要稍加处理
                            else
                            {
                                if (!String.IsNullOrEmpty(itemId.ToString()))
                                {
                                    ArrayList list1 = new ArrayList();
                                    list1.Add(Convert.ToInt32(itemId));
                                    siteManager.AddNotUpdateLookupFieldValue(listId, mParentFolder.ParentList.ParentWeb.SPWeb.ID, mParentFolder.ParentList.SPList.ID, RowId, Version, field.ID, new ArrayList(value.Keys));
                                }
                            }
                        }
                    }
                }
            }
            return lookupCol;
            //if (allFind && lookupCol != null)
            //{
            //    if (originalVersion < Version || Level == 255)
            //    {
            //        List<int> values = new List<int>();
            //        foreach (IAveFieldLookupValue lookupValue in lookupCol)
            //        {
            //            values.Add(lookupValue.LookupId);
            //        }
            //        Guid sourceListId = ParentFolder.ParentList.SPList.ID;
            //        if (Level == 255)
            //        {
            //            RemoveDatajunctionByNative(mAveItem.ListItem, field.ID, sourceListId, originalVersion);
            //        }
            //        CreateDatajunctionByNative(mAveItem.ListItem, field.ID, sourceListId, originalVersion, values);
            //    }
            //    else
            //    {
            //        mAveItem.ListItem[field.ID] = lookupCol;
            //        needUpdate = true;
            //    }
            //}
            //else if (!allFind)
            //{
            //    AveLookupObject obj = siteManager.LookupFieldCache[mParentFolder.ParentList.SPList.ID][field.ID];

            //    //mParentFolder.ParentList.ParentWeb.ParentSite.AddNotUpdateLookupFieldValue(mParentFolder.ParentList.ParentWeb.SPWeb.ID, mParentFolder.ParentList.SPList.ID, mRowId, originalVersion, fieldId, list);

            //    siteManager.AddNotUpdateLookupFieldValue(new Guid(obj.SourceListId), mParentFolder.ParentList.ParentWeb.SPWeb.ID, mParentFolder.ParentList.SPList.ID, mBaseItemInfo.RowId, originalVersion, field.ID, new ArrayList(value.Keys));
            //}
        }

        private IAveFieldUserValueCollection RestoreDataJunctionForUserField(Dictionary<int, string> value, int originalVersion, IAveField field, Guid sourceFieldId)
        {
            IAveFieldUserValueCollection userValueCol = null;
            AveXmlField xmlField = mAveSPList.AveFields.GetXmlFieldBySourceFieldId(sourceFieldId);
            AveSourceFieldInfo fieldInfo = new AveSourceFieldInfo
            {
                SourceDisplayName = xmlField.Title,
                SourceInternalName = xmlField.FieldInternalName,
                SourceType = xmlField.Type
            };
            AveSourceFieldValueInfo fieldValueInfo = new AveSourceFieldValueInfo
            {
                SourceFieldInfo = fieldInfo
            };
            if (xmlField != null)
            {
                foreach (KeyValuePair<int, string> kv in value)
                {
                    string itemValue = kv.Key.ToString();
                    string realValue = itemValue;
                    IAvePrincipal principal = null;
                    if (xmlField.CustomFieldInfo != null)
                    {
                        if (!String.IsNullOrEmpty(kv.Value))
                        {
                            fieldValueInfo.SourceValue = kv.Value;
                            realValue = kv.Value;
                        }
                        else if (xmlField.Type == AveFieldType.User)
                        {
                            string userTitle = string.Empty;
                            string loginName = string.Empty;
                            loginName = mAveSPList.AveFields.GetLoginNameFromId(itemValue, ref userTitle);
                            if (loginName.StartsWith("i:0#.w|", StringComparison.OrdinalIgnoreCase))
                            {
                                loginName = loginName.Substring("i:0#.w|".Length);
                            }
                            fieldValueInfo.SourceValue = loginName;
                            realValue = loginName;
                        }
                        string mappedValue = mAveSPList.AveFields.FieldMapping.GetMappingValue(fieldValueInfo);
                        if (!String.IsNullOrEmpty(mappedValue))
                        {
                            realValue = mappedValue;
                        }
                        principal = mParentFolder.ParentList.ParentWeb.ParentSite.SPMembers.GetOrAddUser(realValue);
                        if (principal == null)
                        {
                            try
                            {
                                principal = mParentFolder.ParentList.ParentWeb.ParentSite.SPSite.RootWeb.SiteGroups[realValue];
                            }
                            catch (Exception ex)
                            {
                                log.Warn("Can not find the user or group.name is:{0}.Error:{1}", realValue, ex.ToString());
                            }
                        }
                    }
                    else
                    {
                        principal = mParentFolder.ParentList.ParentWeb.ParentSite.SPMembers.FindMember(kv.Key,true,true);
                        if (principal == null)//ADO-110131源端user被删除，导致获取不到user
                        {
                            object member = mParentFolder.ParentList.ParentWeb.ParentSite.SPMembers.UserAndDomainMapping.GetUserMapping(kv.Key);
                            if (member is AveSPMemberInfo)
                            {
                                userValueCol = AddUserFieldValue(userValueCol, ((AveSPMemberInfo)member).NewId, ((AveSPMemberInfo)member).AccountName);
                                if (!(field as IAveFieldUser).AllowMultipleValues)
                                {//对于目的端是单值的，获取到一个值后即可
                                    break;
                                }
                            }
                        }
                    }
                    if (principal != null)
                    {
                        userValueCol = AddUserFieldValue(userValueCol, principal.ID, principal.Name);
                        if (!(field as IAveFieldUser).AllowMultipleValues)
                        {//对于目的端是单值的，获取到一个值后即可
                            break;
                        }
                    }
                }
            }
            return userValueCol;
        }

        private IAveFieldUserValueCollection AddUserFieldValue(IAveFieldUserValueCollection userValueCol, int id, string loginName)
        {
            if (userValueCol == null)
            {
                userValueCol = mAveParentSite.ObjectModelFactory.CreateFieldUserValueCollection();
            }
            userValueCol.Add(mAveParentSite.ObjectModelFactory.CreateFieldUserValue(mParentFolder.ParentList.ParentWeb.SPWeb, id, loginName));
            return userValueCol;
        }

        private AveFieldValueInfo RestoreDataJunctionForOtherField(Dictionary<int, string> value, int originalVersion, IAveField field, Guid sourceFieldId)
        {
            string finalValue = "";
            AveXmlField xmlField = mAveSPList.AveFields.GetXmlFieldBySourceFieldId(sourceFieldId);
            AveSourceFieldInfo fieldInfo = new AveSourceFieldInfo
            {
                SourceDisplayName = xmlField.Title,
                SourceInternalName = xmlField.FieldInternalName,
                SourceType = xmlField.Type
            };
            AveSourceFieldValueInfo fieldValueInfo = new AveSourceFieldValueInfo
            {
                SourceFieldInfo = fieldInfo
            };
            if (xmlField != null)
            {
                string splictChare = string.Empty;
                List<string> tempValueCache = new List<string>();
                foreach (KeyValuePair<int, string> kv in value)
                {
                    string itemValue = kv.Key.ToString();
                    string realValue = itemValue;
                    if (!String.IsNullOrEmpty(kv.Value))
                    {
                        fieldValueInfo.SourceValue = kv.Value;
                        realValue = kv.Value;
                    }
                    else if (xmlField.Type == AveFieldType.User)
                    {
                        string userTitle = string.Empty;
                        string loginName = string.Empty;
                        loginName = mAveSPList.AveFields.GetLoginNameFromId(itemValue, ref userTitle);
                        if (loginName.StartsWith("i:0#.w|", StringComparison.OrdinalIgnoreCase))
                        {
                            loginName = loginName.Substring("i:0#.w|".Length);
                        }
                        fieldValueInfo.SourceValue = loginName;
                        realValue = userTitle;
                    }
                    string mappedValue = mAveSPList.AveFields.FieldMapping.GetMappingValue(fieldValueInfo);
                    //ADO-122034 此处处理逻辑用于应对多值ADGroup类型的Mapping值问题。当Mapping源端值为ADGroup且验证方式为Clame验证时，
                    //会出现loginName与userTitle(realValue)不相同的问题。由于loginName的值是数据库内的id串类型。在Mapping中不能找到相关映射
                    //导致mappedValue为Empty(注意：不是null)。所以，当mappedValue为Empty时使用realValue重新进行尝试，如果得到了正确的Mapping
                    //值，则可以认为这个值就是我们需要显示的Column Value。
                    if (String.IsNullOrEmpty(mappedValue))
                    {
                        fieldValueInfo.SourceValue = realValue;
                        mappedValue = mAveSPList.AveFields.FieldMapping.GetMappingValue(fieldValueInfo);
                    }
                    if (!String.IsNullOrEmpty(mappedValue))
                    {
                        realValue = mappedValue;
                    }
                    if (field.TypeAsString.Equals("Choice", StringComparison.OrdinalIgnoreCase) || field.TypeAsString.Equals("MultiChoice", StringComparison.OrdinalIgnoreCase))
                    {
                        if (realValue == null || tempValueCache.Contains(realValue))
                        {
                            continue;
                        }
                        tempValueCache.Add(realValue);
                        if (string.IsNullOrEmpty(splictChare))
                        {
                            splictChare = ";#";
                        }
                        IAveFieldMultiChoice choiceField = field as IAveFieldMultiChoice;
                        if (choiceField.Choices.Contains(realValue) || choiceField.FillInChoice)
                        {
                            finalValue = finalValue + realValue + ";#";
                        }
                    }
                    else if (field.TypeAsString.Equals("Text", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(splictChare))
                        {
                            splictChare = ";";
                        }
                        finalValue = finalValue + realValue + ";";
                    }
                    else if (field.TypeAsString.Equals("Note", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(splictChare))
                        {
                            splictChare = "\n";
                        }
                        finalValue = finalValue + realValue + "\n";
                    }
                }
                if (finalValue.Length > 0)
                {
                    finalValue = finalValue.Substring(0, finalValue.Length - splictChare.Length);
                }
            }

            var aveField = new AveFieldValueInfo { ColValue = finalValue, ColName = field.ColName, FieldType = field.Type, RowOrdinal = field.RowOrdinal };
            return aveField;
        }

        private AveFieldValueInfo RestoreDataJunctionForUniqueValueField(Dictionary<int, string> value, int originalVersion, IAveField field, Guid sourceFieldId)
        {
            string finalValue = "";
            AveXmlField xmlField = mAveSPList.AveFields.GetXmlFieldBySourceFieldId(sourceFieldId);
            AveSourceFieldInfo fieldInfo = new AveSourceFieldInfo
            {
                SourceDisplayName = xmlField.Title,
                SourceInternalName = xmlField.FieldInternalName,
                SourceType = xmlField.Type
            };
            AveSourceFieldValueInfo fieldValueInfo = new AveSourceFieldValueInfo
            {
                SourceFieldInfo = fieldInfo
            };
            if (xmlField != null)
            {
                string splictChare = string.Empty;
                foreach (KeyValuePair<int, string> kv in value)
                {
                    string itemValue = kv.Key.ToString();
                    if (!String.IsNullOrEmpty(kv.Value))
                    {
                        fieldValueInfo.SourceValue = kv.Value;
                    }
                    else if (xmlField.Type == AveFieldType.User)
                    {
                        string userTitle = string.Empty;
                        string loginName = string.Empty;
                        loginName = mAveSPList.AveFields.GetLoginNameFromId(itemValue, ref userTitle);
                        if (loginName.StartsWith("i:0#.w|", StringComparison.OrdinalIgnoreCase))
                        {
                            loginName = loginName.Substring("i:0#.w|".Length);
                        }
                        fieldValueInfo.SourceValue = loginName;
                    }
                    string mappedValue = mAveSPList.AveFields.FieldMapping.GetMappingValue(fieldValueInfo);
                    if (!String.IsNullOrEmpty(mappedValue))
                    {
                        if (field.BaseTypeString.Equals("Boolean", StringComparison.OrdinalIgnoreCase) && !mappedValue.Equals("true", StringComparison.OrdinalIgnoreCase) && !mappedValue.Equals("false", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        finalValue = mappedValue;
                        break;
                    }
                }
            }
            var aveField = new AveFieldValueInfo { ColValue = finalValue, ColName = field.ColName, FieldType = field.Type, RowOrdinal = field.RowOrdinal };
            return aveField;
        }

        private Dictionary<string, string> RestoreDataJunctionForMetadataField(Dictionary<int, string> value, int originalVersion, IAveField field, Guid sourceFieldId)
        {
            SetForceAddTerm(true);
            var taxonomyFieldsInMapping = new Dictionary<string, string>();
            if (WrapperRuntime.CurrentContext.IsMoss)
            {
                string needUpdateValues = string.Empty;
                AveXmlField xmlField = mAveSPList.AveFields.GetXmlFieldBySourceFieldId(sourceFieldId);
                AveSourceFieldInfo fieldInfo = new AveSourceFieldInfo
                {
                    SourceDisplayName = xmlField.Title,
                    SourceInternalName = xmlField.FieldInternalName,
                    SourceType = xmlField.Type
                };
                AveSourceFieldValueInfo fieldValueInfo = new AveSourceFieldValueInfo
                {
                    SourceFieldInfo = fieldInfo
                };
                foreach (KeyValuePair<int, string> kv in value)
                {
                    string itemValue = kv.Key.ToString();
                    if (!String.IsNullOrEmpty(kv.Value))
                    {
                        fieldValueInfo.SourceValue = kv.Value;
                        itemValue = kv.Value;
                    }
                    else if (xmlField.Type == AveFieldType.User)
                    {
                        string userTitle = string.Empty;
                        string loginName = string.Empty;
                        loginName = mAveSPList.AveFields.GetLoginNameFromId(itemValue, ref userTitle);
                        if (loginName.StartsWith("i:0#.w|", StringComparison.OrdinalIgnoreCase))
                        {
                            loginName = loginName.Substring("i:0#.w|".Length);
                        }
                        fieldValueInfo.SourceValue = loginName;
                        itemValue = userTitle;
                    }
                    string mappedValue = mAveSPList.AveFields.FieldMapping.GetMappingValue(fieldValueInfo);
                    if (!String.IsNullOrEmpty(mappedValue))
                    {
                        itemValue = mappedValue;
                    }
                    needUpdateValues = needUpdateValues + itemValue + ";";
                }
                taxonomyFieldsInMapping.Add(field.InternalName, needUpdateValues);
            }
            return taxonomyFieldsInMapping;
        }

        protected bool CheckNeedToRestoreDataJunction(List<Dictionary<string, object>> junctionData, ref int originalVersion)
        {
            if (this.ParentSite.SPSite.NativeApiPermission == WrapperNativeApiPermission.FullControl)
            {
                return true;
            }
            if (Level == 255)
            {
                log.Log(AveLogLevel.WARN, "Skip to restore checked out version lookup value of the item because of lack of permission.");
                return false;
            }
            try
            {
                var node = junctionData.Find(dic => dic.ContainsKey("tp_UIVersion") && Convert.ToInt32(dic["tp_UIVersion"]) > 0);
                originalVersion = Convert.ToInt32(node["tp_UIVersion"]);
                if (originalVersion < this.Version)
                {
                    log.Log(AveLogLevel.WARN, "Skip to restore historical version lookup value of the item because of lack of permission.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, "Can not find source item version info. Error: {0}", ex.ToString());
                return false;
            }
            return true;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1302:DoNotHardcodeLocaleSpecificStrings", Justification = "SendTo is a string")]
        public void RestoreDataJunction(List<Dictionary<string, object>> junctionData)
        {
            if (junctionData == null || junctionData.Count == 0)
            {
                return;
            }
            int originalVersion = 0;
            if (!CheckNeedToRestoreDataJunction(junctionData, ref originalVersion))
            {
                return;
            }
            using (new AvePerformanceScope("Restore.AveSPItem.RestoreDataJunction"))
            {
                try
                {
                    ItemMetadata itemData = new ItemMetadata(this, mBaseItemInfo.OriginalVersion, mBaseItemInfo.OriginalRowId, new Dictionary<string, object>(), junctionData);
                    Dictionary<string, AveFieldValueInfo> itemValues = itemData.ProcessItemMetadata(IsMergeToFolder);
                    bool needUpdate = false;
                    foreach (var item in itemValues)
                    {
                        mAveItem.ListItem[item.Key] = item.Value.ColValue;
                        needUpdate = true;
                    }
                    if (needUpdate)
                    {
                        AveSPItem.SystemUpdate(mAveItem.ListItem);
                        mAveSPList.AveList.Reload();
                        this.ParentFolder.SPFolder.Reload(false);
                    }
                    this.ParentList.AveFields.ResetNotUpdateLookupFieldValue(this.RowId);
                    this.ParentList.AveFields.ResetNintexFormDataFieldValue(this.RowId);
                    this.ParentList.AveFields.ResetNotUpdateUrlFieldValue(this.RowId);
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore dataJunction. name:{0}\n error message:{1}", ex));
                    report.AddDetail(new AveWrapperReportDto("", "", AveReportObjectType.DataJunctions, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionToRestoreDataJunctions, ex.Message));
                }
            }
        }

        public void RemoveDatajunctionByNative(IAveListItem item, Guid fieldId, Guid sourceListId, int version)
        {
            mQueryService.RemoveDataJunctionByNative(item, fieldId, sourceListId, version);
        }

        public void CreateDatajunctionByNative(IAveListItem item, Guid fieldId, Guid sourceListId, int version, List<int> values)
        {
            int ordinal = 0;
            foreach (int value in values)
            {
                InsertIntoAllUserDatajunction(item, fieldId, sourceListId, value, ordinal, version);
                ordinal++;
            }
        }

        public void UpdateColumnByNative(Guid siteId, IAveListItem item, int version, int rowOrdinal, string colName, object colValue)
        {
            mAveItem.UpdateColumnByNative(siteId, item, version, rowOrdinal, colName, colValue);
        }

        public void InsertIntoAllUserDatajunction(IAveListItem item, Guid fieldId, Guid sourceListId, int id, int ordinal, int version)
        {
            mAveItem.InsertIntoAllUserDatajunction(item, fieldId, sourceListId, id, ordinal, version);
        }

        //Reload SPRequest SystemUpdate.
        public static void SystemUpdate(IAveListItem item)
        {
            //释放SPWeb对象的SPRequest对象
            item.ParentList.ParentWeb.InvalidateRequest();
            //调用SPWeb的InitializeSPRequest会重新获取SPRequest对象
            item.ParentList.ParentWeb.InitializeSPRequest();
            item.SystemUpdate();
        }

        #region For BPOS
        public void RestoreItemProperty(AveItemFieldCollectionInfo fieldCollection, IAveList list, IAveListItem item)
        {
            RestoreItemProperty(fieldCollection, list, item, false);
        }
        public void RestoreItemProperty(AveItemFieldCollectionInfo fieldCollection, IAveList list, IAveListItem item, bool overwriteVersion)
        {
            ConvertNameToMappedName(fieldCollection);

            string staticName = string.Empty;
            foreach (AveItemFieldInfo itemField in fieldCollection.ItemFields)
            {
                if (string.IsNullOrEmpty(itemField.StaticName))
                {
                    IAveField field = list.Fields.GetByInfo(itemField.DisplayName, itemField.Type);
                    if (field == null)
                    {
                        continue;
                    }
                    staticName = field.StaticName;
                }
                else
                {
                    staticName = itemField.StaticName;
                }
                if (!string.IsNullOrEmpty(itemField.Type) && itemField.Type.StartsWith(AveFieldType.User.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    string userValue = GetUserContent(list, itemField.Value);
                    if (!string.IsNullOrEmpty(userValue))
                    {
                        item[staticName] = userValue;
                    }
                }
                else if (itemField.Type == "DateTime")
                {
                    if (mAveTimeZone != null)
                    {
                        item[staticName] = mAveTimeZone.UTCToLocalTime((DateTime)itemField.Value);
                    }
                }
                else
                {
                    item[staticName] = itemField.Value;
                }
            }
            if (overwriteVersion)
            {
                item.UpdateOverwriteVersion();
            }
            else
            {
                item.Update();
            }
        }
        internal bool TryGetItem(AveItemFieldCollectionInfo fieldColInfo, IAveListItem aveItem)
        {
            IAveListItemCollection listItems = mAveSPList.SPList.Items;
            foreach (IAveListItem listItem in listItems)
            {
                if (string.Equals(listItem[mRestoreConfig.CheckUniqueField], fieldColInfo.UniqueId))
                {
                    aveItem = listItem;
                    return true;
                }
            }
            aveItem = null;
            return false;
        }
        private void ConvertNameToMappedName(AveItemFieldCollectionInfo fieldInfoCol)
        {
            //Dictionary<string, string> fieldNameMapping = mAveSPList.AveFields.FieldDisplayNameMapping;

            foreach (AveItemFieldInfo fieldInfo in fieldInfoCol.ItemFields)
            {
                if (fieldInfo.DisplayName != null)
                {
                    string mappedFieldName = mAveSPList.AveFields.FieldMapping.GetMappingRestoredFieldDisplayName(fieldInfo.DisplayName + fieldInfo.Type);
                    //if (fieldNameMapping.TryGetValue(fieldInfo.DisplayName + fieldInfo.Type, out mappedFieldName))
                    if (String.IsNullOrEmpty(mappedFieldName))
                    {
                        fieldInfo.DisplayName = mappedFieldName;
                    }
                }
            }
        }
        internal virtual bool ShouldRestoreItem(AveItemFieldCollectionInfo fieldColInfo)
        {
            if (TryGetItem(fieldColInfo, mAveItem.ListItem))
            {
                AveRestoreMode restoreMode = RestoreOption.mAveRestoreMode;

                if (restoreMode == AveRestoreMode.OverWrite || restoreMode == AveRestoreMode.Append)
                {
                    return true;
                }
                else if (restoreMode == AveRestoreMode.OverWriteByModifiedTime)
                {
                    AveItemFieldInfo fieldInfo = fieldColInfo.GetUniqueItemFieldInfoByDisplayName("ModifyDate");
                    if ((DateTime)fieldInfo.Value > (DateTime)mAveItem.ListItem["ModifyDate"])
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            }
            else
            {
                return true;
            }
        }
        private string GetUserContent(IAveList list, object names)
        {
            StringBuilder builder = new StringBuilder();
            string[] userNames = ((string)names).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string logonName = string.Empty;
            foreach (string displayName in userNames)
            {
                try
                {
                    if (displayName.EndsWith("*****group", StringComparison.OrdinalIgnoreCase))
                    {
                        logonName = displayName.Substring(0, displayName.Length - 10);
                        IAveGroup group = list.ParentWeb.SiteGroups[logonName];
                        if (group == null)
                        {
                            group = list.ParentWeb.SiteGroups.Add(new AveGroupCreationInformation() { Title = logonName, Description = "" });
                        }
                        builder.AppendFormat("{0};#{1};#", group.ID, group.Name);
                    }
                    else
                    {
                        //logonName = mSecurityMapping.GetDomainUser(displayName);
                        IAveUser user = list.ParentWeb.EnsureAvailableUser(logonName);
                        builder.AppendFormat("{0};#{1};#", user.ID, user.LoginName);
                    }
                }
                catch (Exception ex)
                {
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreUserFailedEventMessage(logonName, ex));
                }
            }
            return builder.ToString().TrimEnd(new char[] { ';', '#' }).Trim();
        }
        public void RestoreComplianceTag(AveItemComplianceTagInfo complianceTagInfo)
        {
            if (this.ParentSite.SPSite.IsOnlineSite)
            {
                try
                {
                    complianceTagInfo.ComplianceUserLoginName = string.Empty;
                    if (complianceTagInfo.ComplianceTagUserId > 0)
                    {
                        var result = ParentSite.SPMembers.FindMemberIdAndLoginNameFromUserMapping(complianceTagInfo.ComplianceTagUserId);
                        if (result != null)
                        {
                            complianceTagInfo.ComplianceUserLoginName = result.Item2;
                        }
                    }
                    this.SPListItem.SetComplianceTag(complianceTagInfo);
                }
                catch(Exception e)
                {
                    log.Error("Failed to restore compliance tag to item. Web id: {0}, list id: {1}, row id: {2}. Exception: {3}", this.ParentWeb.SPWeb.ID, this.ParentList.SPList.ID, this.SPListItem.ID, e);
                }
            }
        }
        #endregion

        //DOC-70322 for replicator,用于replicator的increment job能够正确还原lookup field的value.
        [Obsolete("如果备份了LookupFieldGuid，在PrepareUserDataAndDataJunction的时候已经处理，不需要单独调用此方法")]
        public void RestoreLookupFieldGuidValue(Dictionary<string, string> lookupFieldGuidValue)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPItem.RestoreLookupFieldGuidValue"))
            {

                IAveList list = mParentFolder.ParentList.SPList;
                if (list == null || lookupFieldGuidValue == null)
                {
                    return;
                }
                bool needUpdateItem = false;
                try
                {
                    foreach (KeyValuePair<string, string> pair in lookupFieldGuidValue)
                    {
                        string name = pair.Key;
                        string mappingName = mParentFolder.ParentList.AveFields.FieldMapping.GetMappingRestoredFieldInternalName(name);
                        if (!String.IsNullOrEmpty(mappingName))
                        {
                            name = mappingName;
                        }
                        IAveField field = list.Fields.GetFieldByInternalName(name);

                        if (!field.BaseTypeString.Equals("Lookup", StringComparison.OrdinalIgnoreCase)
                           && !field.BaseTypeString.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase))
                        {
                            log.Info("This field is not lookupField. field name:{0}, field displayName:{1}.", field.InternalName, field.Title);
                            continue;
                        }

                        if (field.InternalName.Equals("TaxCatchAll"))
                        {
                            continue;
                        }
                        IAveFieldLookup lookupField = field as IAveFieldLookup;
                        if (lookupField.LookupList == null)//说明Column对应的LookupList在目的端没有找到
                        {
                            continue;
                        }
                        Guid lookupListId = new Guid(lookupField.LookupList);
                        string value = pair.Value;
                        if (!lookupField.AllowMultipleValues && value.IndexOf(';') < 0)
                        {
                            int sourceRowId = Int32.Parse(value.ToString().Substring(0, value.ToString().IndexOf('#')));
                            Guid guid = new Guid(value.ToString().Substring(value.ToString().IndexOf('#') + 1));
                            int rowId = GetLookupIdByGUID(lookupField.LookupWebId, lookupListId, guid);
                            if (rowId > 0)
                            {
                                SPListItem[name] = rowId;
                                mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.AddItemIdMapping(lookupListId, sourceRowId, rowId);
                                needUpdateItem = true;
                            }
                        }
                        else if (lookupField.AllowMultipleValues)
                        {
                            string[] values = value.Split(';');
                            IAveFieldLookupValueCollection lookupCol = null;
                            foreach (string temp in values)
                            {
                                if (string.IsNullOrEmpty(temp))
                                {
                                    continue;
                                }
                                int sourceRowId = Int32.Parse(temp.ToString().Substring(0, temp.ToString().IndexOf('#')));
                                Guid guid = new Guid(temp.ToString().Substring(temp.ToString().IndexOf('#') + 1));
                                int rowId = GetLookupIdByGUID(lookupField.LookupWebId, lookupListId, guid);
                                if (rowId > 0)
                                {
                                    if (lookupCol == null)
                                    {
                                        lookupCol = mAveParentSite.ObjectModelFactory.CreateFieldLookupValueCollection();
                                    }
                                    IAveFieldLookupValue lookupValue = mAveParentSite.ObjectModelFactory.CreateFieldLookupValue(rowId, "Title");
                                    lookupValue.LookupId = rowId;
                                    lookupCol.Add(lookupValue);
                                    mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.AddItemIdMapping(lookupListId, sourceRowId, rowId);
                                }
                            }
                            if (lookupCol != null)
                            {
                                SPListItem[name] = lookupCol;
                                needUpdateItem = true;
                            }
                        }
                    }
                    if (needUpdateItem)
                    {
                        SystemUpdate(SPListItem);
                    }
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while RestoreLookupFieldGuidValue. error:{0}.", e.ToString());
                }


            }

        }

        [Obsolete]
        public int GetLookupIdByGUID(Guid lookupListId, Guid tpGuid)
        {
            return GetLookupIdByGUID(Guid.Empty, lookupListId, tpGuid);
        }

        public int GetLookupIdByGUID(Guid lookupWebId, Guid lookupListId, Guid tpGuid)
        {
            if (mQueryService != null)
            {
                return mQueryService.GetLookupIdByGUID(mAveParentSite.SPSite.ID, lookupListId, tpGuid);
            }
            else if (lookupWebId != Guid.Empty)
            {
                if (mAveSPList != null && mAveSPList.AveList != null && mAveSPList.AveList.ID == lookupListId)
                {
                    log.Debug("The item lookup column value will be restored in post action.Because it relate itself.");
                    return -1;
                }
                int itemId = mAveParentSite.GetLookupItemIdAndGuid(lookupWebId, lookupListId, tpGuid);
                if (itemId <= 0)
                {
                    log.Debug("Can not find the Lookup Item RowId by Item TPGuid. LookupWebId: {0}, LookupListId: {1}, ItemTPGuid: {2}", lookupWebId, lookupListId, tpGuid);
                }
                return itemId;
            }
            else
            {
                return -1;
            }
        }

        public int GetLookupIdByFieldDisplayNameAndFieldValue(Guid lookupWebId, Guid lookupListId, String lookupColumnDisplayName, String itemLookupColumnDisplayValue)
        {
            if (lookupWebId != Guid.Empty)
            {
                int itemId = mAveParentSite.GetLookupItemIdByDisplayValue(lookupWebId, lookupListId, lookupColumnDisplayName, itemLookupColumnDisplayValue);
                if (itemId == -1)
                {
                    log.Debug("Can not find the Lookup Item RowId by lookup column display name and column value. LookupWebId: {0}, LookupListId: {1}, FieldDisplayName: {2}, FieldValue: {3}", lookupWebId, lookupListId, lookupColumnDisplayName, itemLookupColumnDisplayValue);
                }
                return itemId;
            }
            else
            {
                return -1;
            }
        }

        public void InitBySPListItem(IAveListItem listItem)
        {
            mAveItem.InitBySPListItem(listItem);
        }

        //这个方法是为了更新Fieldmapping中关于TaxonomyField的信息，同时得到info.FieldsInfo.TermIdMapping为还原这种类型的值做准备。
        internal void GetTaxonomyTermIdMapping(Dictionary<string, object> fieldMapping, AveBaseItemInfo info)
        {
            using (new AvePerformanceScope("Restore.AveSPItem.GetTaxonomyTermIdMapping"))
            {
                var dic = new Dictionary<string, string>();
                foreach (string taxonomyField in mParentFolder.ParentList.AveFields.TaxonomyFields)
                {
                    if (fieldMapping.ContainsKey(taxonomyField))
                    {
                        var fieldValue = fieldMapping[taxonomyField] as AveFieldValueInfo;
                        if (fieldValue.ColValue is string)
                        {
                            dic.Add(taxonomyField, fieldValue.ColValue.ToString());
                        }
                        else
                        {
                            log.Info("This Taxonomy Field value Type is not string. value:{0}", fieldMapping[taxonomyField].ToString());
                        }
                        fieldMapping.Remove(taxonomyField);
                    }
                }
                info.FieldsInfo.TaxonomyFieldsInMapping = dic;
                if (mParentFolder.ParentList.ParentWeb.ParentSite.MetadataService != null)
                {
                    info.FieldsInfo.TermIdMapping = mParentFolder.ParentList.ParentWeb.ParentSite.MetadataService.TermIdMapping;
                    info.FieldsInfo.MergedTermIdMapping = mParentFolder.ParentList.ParentWeb.ParentSite.MetadataService.MergedTermIdMapping;

                }

                if (dic.Count > 0 && !info.IsVersion)
                {
                    var infos = new StringBuilder();
                    infos.AppendLine("Item Source TaxonomyField Value:");
                    foreach (KeyValuePair<string, string> pair in dic)
                    {
                        infos.AppendLine(pair.Key + " : " + pair.Value);
                    }
                    log.Debug(infos.ToString());
                }
            }
        }

        /// <summary>
        /// 在该方法中处理和allDocData相关的设置
        /// </summary>
        /// <param name="allDocData"></param>
        internal virtual void ProcessPreDocDataCondition(Dictionary<string, object> allDocData)
        {
            mBaseItemInfo.IsVersion = allDocData.ContainsKey("IsUserDocVersion") ? (bool)allDocData["IsUserDocVersion"] : false;
            mBaseItemInfo.HasStream = allDocData.ContainsKey("HasStream") ? ((int)allDocData["HasStream"]) == 1 : false;
            mBaseItemInfo.OriginalVersion = allDocData.ContainsKey("UIVersion") ? (int)allDocData["UIVersion"] : -1;
            mBaseItemInfo.OriginalLevel = allDocData.ContainsKey("Level") ? Convert.ToByte(allDocData["Level"]) : (byte)0;
            mBaseItemInfo.DTimeCreated = allDocData.ContainsKey("TimeCreated") ? (DateTime)allDocData["TimeCreated"] : DateTime.MinValue;
            if (allDocData.ContainsKey("TimeLastModified") && allDocData["TimeLastModified"] != null)
            {
                mBaseItemInfo.DTimeLastModified = (DateTime)allDocData["TimeLastModified"];
            }
            //else if (allDocData.ContainsKey("BiggestVersionModified") && allDocData["BiggestVersionModified"] != null)
            //{
            //    mBaseItemInfo.DTimeLastModified = (DateTime)allDocData["BiggestVersionModified"];
            //}
            else
            {
                mBaseItemInfo.DTimeLastModified = DateTime.MinValue;
            }
            mBaseItemInfo.GUID = allDocData.ContainsKey("Id") ? new Guid(allDocData["Id"].ToString()) : Guid.Empty;
            mBaseItemInfo.DraftOwnerId = allDocData.ContainsKey("DraftOwnerId") ? (int)allDocData["DraftOwnerId"] : -1;
            //TODO: Replace DraftOwnerId with Editor when DraftOwnerId is null, Need to test.
            //if (mBaseItemInfo.DraftOwnerId == -1 && allDocData.ContainsKey("Editor"))
            //{
            //    mBaseItemInfo.DraftOwnerId = Convert.ToInt32(allDocData["Editor"]);
            //}
            if (mBaseItemInfo.DraftOwnerId > 0)
            {
                mBaseItemInfo.DraftOwnerId = mAveSPList.ParentWeb.ParentSite.SPMembers.FindMemberId(mBaseItemInfo.DraftOwnerId);
            }
            mBaseItemInfo.CheckoutUserId = -1;
            if (allDocData.ContainsKey("CheckoutUserId"))
            {
                mBaseItemInfo.CheckoutUserId = Convert.ToInt32(allDocData["CheckoutUserId"]);
                mBaseItemInfo.CheckoutUserId = mAveSPList.ParentWeb.ParentSite.SPMembers.FindMemberId(mBaseItemInfo.CheckoutUserId);
            }
            mBaseItemInfo.OriginalRowId = -1;
            if (allDocData.ContainsKey("DoclibRowId"))
            {
                mBaseItemInfo.OriginalRowId = Convert.ToInt32(allDocData["DoclibRowId"]);
            }
            if (allDocData.ContainsKey("IsCurrentVersion"))
            {
                mBaseItemInfo.IsCurrentVersion = Convert.ToBoolean(allDocData["IsCurrentVersion"]);
            }
            if (allDocData.ContainsKey("HasUniqueRoleAssignments"))
            {
                mBaseItemInfo.HasUniqueRoleAssignments = (bool)allDocData["HasUniqueRoleAssignments"];
                allDocData.Remove("HasUniqueRoleAssignments");
            }
            mBaseItemInfo.DocData = allDocData;
            ProcessPreMetaInfoCondition(allDocData);
        }

        internal virtual void ProcessPreUserAndJunctionDataCondition(Dictionary<string, object> allUserData, List<Dictionary<string, object>> junctionData)
        {
            mBaseItemInfo.ModerationStatus = 0;
            if (mBaseItemInfo.DTimeLastModified == DateTime.MinValue && allUserData.ContainsKey("Modified"))
            {
                mBaseItemInfo.DTimeLastModified = (DateTime)allUserData["Modified"];
            }
            if (allUserData.ContainsKey("#tp_ModerationStatus"))
            {
                mBaseItemInfo.ModerationStatus = Convert.ToInt32(allUserData["#tp_ModerationStatus"]);
            }
            mBaseItemInfo.ModerationComments = string.Empty;
            if (allUserData.ContainsKey("_ModerationComments"))
            {
                mBaseItemInfo.ModerationComments = (string)allUserData["_ModerationComments"];
            }
            if (!hasExcludedUrl)
            {
                AveReplaceProcessor.excludedUrls = null;
            }
            ItemMetadata itemData = new ItemMetadata(this, mBaseItemInfo.OriginalVersion, mBaseItemInfo.OriginalRowId, allUserData, junctionData);
            mBaseItemInfo.FieldsInfo.Fields = itemData.ProcessItemMetadata(IsMergeToFolder).ToDictionary(pair => pair.Key, pair => (object)pair.Value);
            if (mParentFolder.ParentList.AveFields.NeedForceCreateTerm)
            {
                SetForceAddTerm(true);
            }

            mBaseItemInfo.NeedSetNullFields = mParentFolder.ParentList.SetNeedSetNullFields(mBaseItemInfo, mParentFolder.ServerRelativeUrl);
            GetTaxonomyTermIdMapping(mBaseItemInfo.FieldsInfo.Fields, mBaseItemInfo);
            mBaseItemInfo.UserData = allUserData;

            string fieldDisplayName = this.ParentList.AveFields.FieldMapping.GetMappingRestoredFieldDisplayName(mRestoreOption.mAveItemRestoreOption.MatchItemFieldDisplayValue);
            if (string.IsNullOrEmpty(fieldDisplayName))
            {
                fieldDisplayName = mRestoreOption.mAveItemRestoreOption.MatchItemFieldDisplayValue;
            }
            mBaseItemInfo.SettingInfo.MatchItemFieldDisplayName = fieldDisplayName;
            mBaseItemInfo.SettingInfo.CheckItemByFieldValue = NeedCheckItemByFieldValue(mBaseItemInfo, mRestoreOption.mAveItemRestoreOption.MatchItemFieldDisplayValue);

            ResolveUniqueFieldConflict();
        }

        private bool NeedCheckItemByFieldValue(AveBaseItemInfo info, string fieldDisplayName)
        {
            IAveField field = null;
            try
            {
                if (!string.IsNullOrEmpty(fieldDisplayName) && ParentList.SPList != null)
                {
                    field = ParentList.SPList.Fields[fieldDisplayName];
                }
            }
            catch (Exception e)
            {
                log.Debug("Can not get the list field: '{0}' in the list: '{1}'.Error Message: {2}", fieldDisplayName, ParentList.SPList.Title, e.ToString());
            }
            if (field != null)
            {
                if (info.FieldsInfo.Fields.ContainsKey(field.InternalName) || info.UserData.ContainsKey(field.InternalName))
                {
                    return true;
                }
                else
                {
                    log.Debug("Can not get the list field value in the list .Field: '{0}', List: '{1}'.", fieldDisplayName, ParentList.SPList.Title);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 在该方法中处理和allUserData相关的设置
        /// </summary>
        /// <param name="allUserData"></param>
        [Obsolete("Use ProcessPreUserAndJunctionDataCondition() instead")]
        internal virtual void ProcessPreUserDataCondition(Dictionary<string, object> allUserData)
        {
            mBaseItemInfo.ModerationStatus = 0;
            if (mBaseItemInfo.DTimeLastModified == DateTime.MinValue && allUserData.ContainsKey("Modified"))
            {
                mBaseItemInfo.DTimeLastModified = (DateTime)allUserData["Modified"];
            }
            if (allUserData.ContainsKey("#tp_ModerationStatus"))
            {
                mBaseItemInfo.ModerationStatus = Convert.ToInt32(allUserData["#tp_ModerationStatus"]);
            }
            mBaseItemInfo.ModerationComments = string.Empty;
            if (allUserData.ContainsKey("_ModerationComments"))
            {
                mBaseItemInfo.ModerationComments = (string)allUserData["_ModerationComments"];
            }
            mBaseItemInfo.FieldsInfo.Fields = mParentFolder.ParentList.AveFields.GetFieldValues(mBaseItemInfo.Name, mBaseItemInfo.OriginalRowId, mBaseItemInfo.OriginalVersion, allUserData, true, this.IsMergeToFolder);

            if (mParentFolder.ParentList.AveFields.NeedForceCreateTerm)
            {
                SetForceAddTerm(true);
            }

            mBaseItemInfo.NeedSetNullFields = mParentFolder.ParentList.SetNeedSetNullFields(mBaseItemInfo, mParentFolder.ServerRelativeUrl);
            GetTaxonomyTermIdMapping(mBaseItemInfo.FieldsInfo.Fields, mBaseItemInfo);
            mBaseItemInfo.UserData = allUserData;
            ResolveUniqueFieldConflict();
        }

        internal virtual void ProcessPostJunctionDataCondition(List<Dictionary<string, object>> junctionData)
        {
            if (mBaseItemInfo.FieldsInfo.NeedPostRestoreMultiLookupFields == null || mBaseItemInfo.FieldsInfo.NeedPostRestoreMultiLookupFields.Count <= 0)
            {
                return;
            }
            foreach (Dictionary<string, object> fieldInfo in junctionData)
            {
                mParentWeb.ParentSite.MappingManager.SiteMappingManager.AddNotUpdateLookupFieldValue((Guid)fieldInfo["LookupListId"],
                    mParentFolder.ParentList.ParentWeb.SPWeb.ID,
                    mParentFolder.ParentList.SPList.ID,
                    RowId,
                    Version <= 0 ? (int)fieldInfo["ItemVersion"] : Version,
                    (Guid)fieldInfo["FieldId"],
                    fieldInfo["FieldValue"]);
            }
        }

        internal virtual void ProcessPreJunctionDataCondition(List<Dictionary<string, object>> junctionData)
        {
            if (junctionData == null)
            {
                return;
            }

            using (new AvePerformanceScope("Restore.AveSPItem.ProcessPreJunctionDataCondition"))
            {

                Dictionary<string, object> multiLookupFields = new Dictionary<string, object>();
                List<Dictionary<string, object>> needPostMultiFields = new List<Dictionary<string, object>>();
                try
                {
                    int originalVersion = 0;
                    Dictionary<Guid, Dictionary<int, string>> fieldValues = new Dictionary<Guid, Dictionary<int, string>>();//<FieldId,<ItemId,DisplayValue>>
                    foreach (Dictionary<string, object> dic in junctionData)
                    {
                        if (originalVersion == 0)
                        {
                            originalVersion = (int)dic["tp_UIVersion"];
                        }
                        Guid fieldId = (Guid)dic["tp_FieldId"];
                        int id = (int)dic["tp_Id"];
                        if (!fieldValues.ContainsKey(fieldId))
                        {
                            fieldValues.Add(fieldId, new Dictionary<int, string>());
                        }
                        string displayValue = string.Empty;
                        if (dic.ContainsKey("DisplayValue"))
                        {
                            displayValue = dic["DisplayValue"].ToString();
                        }
                        fieldValues[fieldId].Add(id, displayValue);
                    }
                    bool needUpdate = false;

                    foreach (KeyValuePair<Guid, Dictionary<int, string>> kv in fieldValues)
                    {
                        try
                        {
                            Guid sourceFieldId = kv.Key;
                            AveXmlField xmlField = mAveSPList.AveFields.GetXmlFieldBySourceFieldId(sourceFieldId);
                            if (!(xmlField.TypeAsString.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase) || xmlField.TypeAsString.Equals("UserMulti", StringComparison.OrdinalIgnoreCase)))
                                continue;
                            Dictionary<int, string> value = kv.Value;
                            IAveField field = null;
                            try
                            {
                                field = mParentFolder.ParentList.AveFields.GetFieldById(mParentFolder.ParentList.AveFields.FieldMapping.GetMappingRestoredFieldId(sourceFieldId));
                            }
                            catch (Exception ex)
                            {
                                log.Debug("Restore data junction ensure field failed.Exception:{0}", ex.ToString());
                            }
                            if (field == null)
                            {
                                continue;
                            }
                            if (field is IAveFieldUser)
                            {
                                IAveFieldUserValueCollection userValueCol = RestoreDataJunctionForUserField(value, originalVersion, field, sourceFieldId);
                                if (userValueCol != null)
                                {
                                    multiLookupFields[field.InternalName] = userValueCol;
                                }
                            }
                            else if (field.TypeAsString == "TaxonomyFieldType" || field.TypeAsString == "TaxonomyFieldTypeMulti")
                            {
                                var taxonomyFieldsInMapping = RestoreDataJunctionForMetadataField(value, originalVersion, field, sourceFieldId);
                                if (taxonomyFieldsInMapping.Count > 0)
                                {
                                    foreach (KeyValuePair<string, string> mapping in taxonomyFieldsInMapping)
                                    {
                                        this.mBaseItemInfo.FieldsInfo.TaxonomyFieldsInMapping[mapping.Key] = mapping.Value;
                                    }
                                }
                            }
                            else if (field.TypeAsString.Equals("Choice", StringComparison.OrdinalIgnoreCase) || field.TypeAsString.Equals("MultiChoice", StringComparison.OrdinalIgnoreCase)
                                || field.TypeAsString.Equals("Text", StringComparison.OrdinalIgnoreCase) || field.TypeAsString.Equals("Note", StringComparison.OrdinalIgnoreCase))
                            {
                                var fieldValue = RestoreDataJunctionForOtherField(value, originalVersion, field, sourceFieldId);
                                if (fieldValue != null)
                                {
                                    this.mBaseItemInfo.FieldsInfo.Fields.Add(field.InternalName, fieldValue);
                                }
                            }
                            else if (field is IAveFieldLookup)
                            {
                                bool allFind = true;
                                IAveFieldLookupValueCollection lookupCol = RestoreDataJunctionForLookupField(value, originalVersion, field, ref allFind, xmlField);
                                var siteManager = mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager;
                                if (allFind && lookupCol != null)
                                {
                                    multiLookupFields[field.InternalName] = lookupCol;
                                }
                                else if (!allFind)
                                {
                                    AveLookupObject obj;
                                    siteManager.TryGetValueFromLookupFieldMapping(mParentFolder.ParentList.SPList.ID, field.ID, out obj);
                                    AddNeedPostJunction(needPostMultiFields,
                                                        new Guid(obj.SourceListId),
                                                        mParentFolder.ParentList.ParentWeb.SPWeb.ID,
                                                        mParentFolder.ParentList.SPList.ID,
                                                        mBaseItemInfo.RowId,
                                                        originalVersion,
                                                        field.ID,
                                                        new ArrayList(value.Keys));
                                }
                            }
                            else if (field.BaseTypeString.Equals("Number", StringComparison.OrdinalIgnoreCase)
                                || field.BaseTypeString.Equals("Currency", StringComparison.OrdinalIgnoreCase)
                                || field.BaseTypeString.Equals("DateTime", StringComparison.OrdinalIgnoreCase)
                                || field.BaseTypeString.Equals("Boolean", StringComparison.OrdinalIgnoreCase))
                            {
                                var fieldValue = RestoreDataJunctionForUniqueValueField(value, originalVersion, field, sourceFieldId);
                                if (fieldValue != null)
                                {
                                    this.mBaseItemInfo.FieldsInfo.Fields.Add(field.InternalName, fieldValue);
                                }
                            }
                        }
                        catch (AveSecurityTrimingException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.ERROR, string.Format("An error occurred while restore dataJunction. name:{0}\n error message:{1}", kv.Key, e));
                        }
                    }
                    if (multiLookupFields.Count > 0)
                    {
                        mBaseItemInfo.FieldsInfo.MultiLookupFields = multiLookupFields;
                    }
                    if (needPostMultiFields.Count > 0)
                    {
                        mBaseItemInfo.FieldsInfo.NeedPostRestoreMultiLookupFields = needPostMultiFields;
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore dataJunction. name:{0}\n error message:{1}", ex));
                    report.AddDetail(new AveWrapperReportDto("", "", AveReportObjectType.DataJunctions, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionToRestoreDataJunctions, ex.Message));
                }

            }

        }

        public void CacheNintexFormDataForPostAction(string formData)
        {
            mBaseItemInfo.FieldsInfo.NintexFormDataForPostAction = formData;
        }

        private void AddNeedPostJunction(List<Dictionary<string, object>> fieldValues, Guid lookupListId, Guid webId, Guid listId, int rowId, int version, Guid fieldId, ArrayList valueList)
        {
            Dictionary<string, object> fieldValueInfo = new Dictionary<string, object>();
            fieldValueInfo["LookupListId"] = lookupListId;
            //fieldValueInfo["WebId"] = webId;
            //fieldValueInfo["ListId"] = listId;
            //fieldValueInfo["ItemRowId"] = rowId;
            fieldValueInfo["ItemVersion"] = version;
            fieldValueInfo["FieldId"] = fieldId;
            fieldValueInfo["FieldValue"] = valueList;
            fieldValues.Add(fieldValueInfo);
        }

        private Dictionary<IAveField, object> GetItemUniqueFields()
        {
            Dictionary<IAveField, object> UniqueFields = new Dictionary<IAveField, object>();

            foreach (string internalName in mAveSPList.AveFields.needUpdateUniqueValueFields)
            {
                try
                {
                    if (!string.IsNullOrEmpty(internalName))
                    {
                        string tmp = internalName;
                        string mappingValue = mAveSPList.AveFields.FieldMapping.GetMappingRestoredFieldInternalName(internalName);
                        if (!string.IsNullOrEmpty(mappingValue))
                        {
                            tmp = mappingValue;
                        }
                        if (mBaseItemInfo.FieldsInfo.Fields.ContainsKey(tmp))
                        {
                            object value = mBaseItemInfo.FieldsInfo.Fields[tmp];
                            IAveField field = mAveSPList.SPList.Fields.GetFieldByInternalName(tmp);
                            if (field != null && value != null)
                            {
                                UniqueFields[field] = value;
                            }
                        }
                        if (mBaseItemInfo.FieldsInfo.TaxonomyFieldsInMapping.ContainsKey(tmp))
                        {
                            string colValue = mBaseItemInfo.FieldsInfo.TaxonomyFieldsInMapping[tmp];
                            IAveField field = mAveSPList.SPList.Fields.GetFieldByInternalName(tmp);
                            if (field != null && colValue != null)
                            {
                                AveFieldValueInfo value = new AveFieldValueInfo();
                                colValue = colValue.Split('|')[0];
                                value.ColValue = colValue;
                                UniqueFields[field] = value;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "Can not get the field. InternalName:{0}. Error:{1}", internalName, ex.ToString());
                }
            }

            foreach (KeyValuePair<string, object> pair in mBaseItemInfo.FieldsInfo.Fields)
            {
                try
                {
                    // 过滤Url类型的Column的Description列
                    if (pair.Key.Contains("#"))
                    {
                        continue;
                    }
                    IAveField field = mAveSPList.SPList.Fields.GetFieldByInternalName(pair.Key);
                    if (field != null && field.EnforceUniqueValues && pair.Value != null)
                    {
                        UniqueFields[field] = pair.Value;
                    }
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "Can not get the field. InternalName:{0}. Error:{1}", pair.Key, ex.ToString());
                }
            }
            return UniqueFields;
        }

        /// <summary>
        /// 根据unique field判断是否冲突，如果unique field一样并且不是一个Item则认为冲突
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private bool CheckUniqueFieldConflict(out IAveListItemCollection items)
        {
            items = null;
            Dictionary<IAveField, object> UniqueFields = GetItemUniqueFields();
            int itemsCount = 0;
            if (UniqueFields.Count > 0)
            {
                try
                {
                    AveCamlQuery query = GetCamlQueryByFields(UniqueFields, CamlQueryFindOption.Or);
                    items = mAveSPList.SPList.GetItems(query);
                    if (items == null)
                    {
                        return false;
                    }
                    //ADO-81193 设置Column Mapping时，items.Count可能会抛错，将其放到try catch里
                    itemsCount = items.Count - FilterNotNecessaryItem(items);
                }
                catch (Exception ex)
                {
                    log.Warn("Check list item which has unique field conflict error.Message:{0}", ex.ToString());
                    return false;
                }
            }
            return itemsCount > 0;
        }

        /// <summary>
        /// ADO-83256,当item是自己本身时，不需要认为是冲突的。
        /// </summary>
        /// <param name="items"></param>
        private int FilterNotNecessaryItem(IAveListItemCollection items)
        {
            int notNecessaryItemCount = 0;
            foreach (var item in items)
            {
                if (IsSameItem(item))
                {
                    notNecessaryItemCount++;
                    continue;
                }
            }
            return notNecessaryItemCount;
        }

        private bool IsSameItem(IAveListItem item)
        {
            bool isSame = false;
            if (ParentList.SPList.BaseType != AveBaseType.DocumentLibrary)
            {
                if (ParentSite.SPContextKind == AveContextKind.ClientObjectModel)
                {
                    //365 keep tp_guid,use tp_guid check conflict
                    isSame = IsSameItemByGuidField(item, "GUID", "#tp_GUID",mBaseItemInfo.UserData);
                    //for HSM IB, use unique id check conflict
                    if (!isSame && RestoreOption.mAveItemRestoreOption.CheckConflictByUniqueId)
                    {
                        isSame = IsSameItemByGuidField(item, "UniqueId", "Id", mBaseItemInfo.DocData);
                    }
                }
                else if (RestoreOption.mAveItemRestoreOption.KEEP_ITEM_TPGUID)
                {
                    //for RP local, use tp_Guid check conflict
                    isSame = IsSameItemByGuidField(item, "GUID", "#tp_GUID", mBaseItemInfo.UserData);
                }
                else
                {
                    //默认用row id check conflict
                    isSame = IsSameItemById(item);
                }
            }
            else
            {
                if (item.Name.Equals(mBaseItemInfo.Name, StringComparison.OrdinalIgnoreCase))
                {
                    isSame = true;
                }
            }
            return isSame;
        }

        private static bool IsSameItemByGuidField(IAveListItem item, string fieldName, string fieldNameInDictionary,IDictionary<string,object> data)
        {
            var isSame = false;
            object fieldValue;
            object fieldValueInBackupData;
            //当前local item["GUID"].ToString()是"{GUID}"形式，365是"GUID"形式，new成Guid类型进行比较。        
            if (item.FieldValues.TryGetValue(fieldName, out fieldValue) && fieldValue != null && data.TryGetValue(fieldNameInDictionary,out fieldValueInBackupData))
            {
                if (new Guid(fieldValue.ToString()) == new Guid(fieldValueInBackupData.ToString()))
                {
                    isSame = true;
                }
            }
            return isSame;
        }

        private bool IsSameItemById(IAveListItem item)
        {
            var isSame = false;
            var listItemTempId = GetListItemIdByName(mBaseItemInfo.Name);
            if (!string.IsNullOrEmpty(listItemTempId))
            {
                if (Convert.ToInt32(listItemTempId) == item.ID)
                {
                    isSame = true;
                }
            }
            return isSame;
        }

        public string GetListItemIdByName(string itemName)
        {
            int tempIndex = itemName.LastIndexOf("_.000", StringComparison.OrdinalIgnoreCase);
            return tempIndex > 0 ? itemName.Substring(0, tempIndex) : null;
        }

        private AveCamlQuery GetCamlQueryByFields(Dictionary<IAveField, object> fields, CamlQueryFindOption findOption)
        {
            AveCamlQuery query = new AveCamlQuery();
            StringBuilder viewXmlStringBuilder = new StringBuilder();
            viewXmlStringBuilder.Append("<View><Query><Where>");
            if (fields.Count > 1)
            {
                viewXmlStringBuilder.Append("<" + findOption.ToString() + ">");
            }
            foreach (KeyValuePair<IAveField, object> pair in fields)
            {
                AveFieldValueInfo fieldValue = pair.Value as AveFieldValueInfo;
                if (fieldValue != null)
                {
                    string colValue = fieldValue.ColValue.ToString();
                    string value = colValue.Contains(";#") ? colValue.Substring(colValue.IndexOf(";#", StringComparison.Ordinal) + 2) : colValue;
                    string fieldRef = string.Format("<Eq><FieldRef Name='{0}'/><Value Type='Text'>{1}</Value></Eq>", pair.Key.InternalName, value);
                    viewXmlStringBuilder.Append(fieldRef);
                }
            }
            if (fields.Count > 1)
            {
                viewXmlStringBuilder.Append("</" + findOption.ToString() + ">");
            }
            viewXmlStringBuilder.Append("</Where></Query></View>");
            query.ViewXml = viewXmlStringBuilder.ToString();
            query.FolderServerRelativeUrl = this.ParentFolder.ServerRelativeUrl;
            return query;
        }
        private void ResolveUniqueFieldConflict()
        {
            if (WrapperConfiguration.UniqueFieldSolution == UniqueFieldSolution.Continue || mAveSPList == null || !mAveSPList.HasUniqueField || !this.mBaseItemInfo.IsCurrentVersion)//continue
            {
                return;
            }
            IAveListItemCollection items = null;
            if (CheckUniqueFieldConflict(out items))
            {
                if (WrapperConfiguration.UniqueFieldSolution == UniqueFieldSolution.Skip) //skip
                {
                    log.Log(AveLogLevel.WARN, "Skip to restore item because of unique field value conflict. Item Name:{0}, Item RowId:{1}", mBaseItemInfo.Name, mBaseItemInfo.OriginalRowId);
                    throw new AveRestoreException(AveRestoreResult.SkipItemUniqueFieldConflict, AveInternalResourceKey.Wrapper_Exception_Restore_ItemHasForcedUniqueField, mBaseItemInfo.Name);
                }
                else if (WrapperConfiguration.UniqueFieldSolution == UniqueFieldSolution.Overwrite) //overwrite
                {
                    for (int i = items.Count - 1; i >= 0; i--)
                    {
                        try
                        {
                            if (IsSameItem(items[i]))
                            {
                                continue;
                            }
                            items[i].Delete();
                        }
                        catch (Exception ex)
                        {
                            log.Warn("Delete item which has unique field failed,continue restoring it.Message:{0}", ex.ToString());
                            //365还原中不能关闭eventreceivers，导致不能delete和update带有uniquefield的item
                            if (this.ParentSite.SPContextKind == AveContextKind.ClientObjectModel && items[i].ID == 1 && items[i].Url.StartsWith("DeviceChannels",StringComparison.OrdinalIgnoreCase))
                            {
                                log.Warn("Skip to delete the item {0}, because it has a field that requires a unique value.", mBaseItemInfo.Name);
                                throw new AveRestoreException(AveRestoreResult.SkipItemUniqueFieldConflict, "Cannot delete the item has a field that requires a unique value.");
                            }
                        }

                    }
                }
            }
        }
        /// <summary>
        /// 在该方法中处理和Setting相关的设置(和allDocData，allUserData无关的)
        /// </summary>
        internal virtual void ProcessPreSettingCondition()
        {
            mBaseItemInfo.SettingInfo.CheckConflictByUniqueId = RestoreOption.mAveItemRestoreOption.CheckConflictByUniqueId;
            mBaseItemInfo.SettingInfo.KEEP_ITEM_TPGUID = RestoreOption.mAveItemRestoreOption.KEEP_ITEM_TPGUID;
            mBaseItemInfo.SettingInfo.MOVE_ITEM_TO_CONFLICT_FOLDER = RestoreOption.mAveItemRestoreOption.MOVE_ITEM_TO_CONFLICT_FOLDER;
            mBaseItemInfo.SettingInfo.MOVE_SOURCE_TO_CONFLICT_FOLDER = RestoreOption.mAveItemRestoreOption.MOVE_SOURCE_ITEM_TO_FOLDER;
            mBaseItemInfo.SettingInfo.DESTSTUB_CONTENT = RestoreOption.mAveStorgeOption.DESTSTUB_CONTENT;
            mBaseItemInfo.SettingInfo.NewItemWithOutVerifyConflict = RestoreOption.mAveItemRestoreOption.NewItemWithOutVerifyConflict;
            mBaseItemInfo.SettingInfo.IncreaceVerionWithRowId = RestoreOption.mAveItemRestoreOption.IncreaceVerionWithRowId;
            if (this.ParentList.SPList != null)//System Folder
            {
                mBaseItemInfo.ParentListTitle = this.ParentList.SPList.Title;
            }
            mBaseItemInfo.RestoreOption = mRestoreOption.mAveRestoreMode;
            mBaseItemInfo.SourceSiteInfo = mAveParentSite.SourceSiteInfo;
            mBaseItemInfo.ParentSiteServerRelativeUrl = mAveParentSite.ServerRelativeUrl;
            mBaseItemInfo.ParentWebRelativeUrl = this.ParentList.ParentWeb.SPWeb.ServerRelativeUrl;
            mBaseItemInfo.ParentFolderRelativeUrl = this.ParentFolder.SPFolder.ServerRelativeUrl;
        }

        /// <summary>
        /// 在该方法中处理MetaInfo相关的设置(MetaInfo属于allDocData)
        /// </summary>
        /// <param name="allDocData"></param>
        internal virtual void ProcessPreMetaInfoCondition(Dictionary<string, object> allDocData)
        {
        }

        internal virtual void ProcessVerifyItem()
        {
            if (mBaseItemInfo.VerifyItemMMSColumnValue)
            {
                if (this.ParentSite.MetadataService == null)
                {
                    this.ParentSite.MetadataService = new AveMetadataService(this.ParentSite);
                }
                //保证item的MetadataColumn的term能够存在或还原成功，才能允许继续restore item
                if (this.ParentFolder.ParentList.SPList != null && mBaseItemInfo.FieldsInfo.TaxonomyFieldsInMapping != null && !this.ParentSite.MetadataService.VerifyMetadataColumnValue(this.ParentFolder.ParentList.SPList, mBaseItemInfo.FieldsInfo.TaxonomyFieldsInMapping, mBaseItemInfo.FieldsInfo.TermIdMapping, mBaseItemInfo.FieldsInfo.MergedTermIdMapping))
                {
                    log.Log(AveLogLevel.WARN, string.Format("VerifyMetadataColumnValue failed, shouldn't restore document:{0}", mBaseItemInfo.Name));
                    throw new AveVerifyItemMetadataValueNotFoundException(AveInternalResourceKey.Wrapper_Exception_Restore_VerifyItemMetadataValueNotFound);
                }
            }
        }

        public bool MoveToConflictFolder(IAveList parentList, IAveFolder parentFolder, IAveListItem listItem, bool isSourceWin)
        {
            return mAveItem.MoveToConflictFolder(parentList, parentFolder, listItem, isSourceWin);
        }

        public AveItemHoldRecord GetHoldRecord(Hashtable metaInfos, byte[] dataMetaInfo, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPItem.GetHoldRecord"))
            {

                try
                {
                    string metaInfoString = null;
                    if (AveCompressedUtility.IsTCompressedBytes(dataMetaInfo))
                    {
                        metaInfoString = AveCompressedUtility.GetTCompressedString(dataMetaInfo);
                    }
                    else
                    {
                        metaInfoString = Encoding.UTF8.GetString(dataMetaInfo);
                    }

                    var fileHoldValue = AveCompressedUtility.ModifyMetaInfoString(metaInfos, metaInfoString);

                    return new AveItemHoldRecord()
                    {
                        ItemHoldRecordStatus = userData["_vti_ItemHoldRecordStatus"].ToString(),
                        ItemDeclaredRecord = userData.ContainsKey("_vti_ItemDeclaredRecord") ? userData["_vti_ItemDeclaredRecord"].ToString() : null,
                        IconOverlay = userData.ContainsKey("IconOverlay") ? userData["IconOverlay"].ToString() : null,
                        ItemLockHolders = fileHoldValue["ecm_ItemLockHolders"],
                        ItemDeleteBlockHolders = fileHoldValue["ecm_ItemDeleteBlockHolders"],
                        HoldsProperty = fileHoldValue["_dlc_Holds_Property"],
                        RecordRestrictions = fileHoldValue["ecm_RecordRestrictions"],
                        IsHold = fileHoldValue["_dlc_Holds_Property"] != null ? true : false,
                        IsRecord = userData.ContainsKey("_vti_ItemDeclaredRecord") ? true : false
                    };
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.INFO, "Get the lock and declared record metainfo. Error:{0}.", ex);
                }
                return null;

            }

        }

        public bool EnsureItemSchemaDependency(Dictionary<string, object> userData, List<Dictionary<string, object>> junctionData, bool skipItemWhenNotFound, bool skipItemWhenConflict, AveContentTypeRestoreOption ctRestoreOption, AveFieldRestoreOption fieldRestoreOption, bool throwException)
        {
            #region Restore item content type and fields
            using (new AvePerformanceScope("Restore.AveSPItem.EnsureItemSchemaDependency"))
            {
                try
                {
                    Exception ex = null;
                    lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("FieldLock"))
                    {
                        try
                        {
                            ParentList.AveFields.EnsureFields(userData, junctionData, skipItemWhenNotFound, skipItemWhenConflict, fieldRestoreOption);
                        }
                        catch (Exception excep)
                        {
                            log.Log(AveLogLevel.WARN, "Failed to ensure fields, error:{0}", excep);
                            ex = excep;
                        }
                    }
                    EnsureItemContentTypeDependency(userData, skipItemWhenNotFound, skipItemWhenConflict, ctRestoreOption);
                    if (ex != null)
                    {
                        throw ex;
                    }
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, "Exception threw when try to ensure the item schema dependency. Exception info: {0}", e.ToString());
                    if (throwException)
                    {
                        throw;
                    }
                    else
                    {
                        return false;
                    }
                }
                finally
                {
                    if (ParentList.SPList != null && ParentSite.MappingManager.SiteMappingManager.ListEnsureFields.ContainsKey(ParentList.SPList.ID)
                            && ParentSite.MappingManager.SiteMappingManager.ListEnsureFields[ParentList.SPList.ID].Count != 0)
                    {
                        ParentList.ReloadList();
                    }
                }
            }
            #endregion
            return true;
        }

        public void EnsureItemContentTypeDependency(Dictionary<string, object> userData, bool skipItemWhenNotFound, bool skipItemWhenConflict, AveContentTypeRestoreOption ctRestoreOption)
        {
            string contentTypeIdStr = string.Empty;
            if (userData.ContainsKey("#tp_ContentTypeId"))
            {
                contentTypeIdStr = AveConvert.ConvertByteToContentTypeId(mAveParentSite.ObjectModelFactory, (byte[])userData["#tp_ContentTypeId"]).ToString();
            }
            lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ContentTypeLock"))
            {
                ParentList.AveContentTypes.EnsureContentType(contentTypeIdStr, ctRestoreOption, skipItemWhenNotFound, skipItemWhenConflict, ParentList.AveFields.HasCreateFieldWhenEnsureFields);
            }
        }

        public bool EnsureItemSchemaDependency(Dictionary<string, object> userData, bool skipItemWhenNotFound, bool skipItemWhenConflict, AveContentTypeRestoreOption ctRestoreOption, AveFieldRestoreOption fieldRestoreOption)
        {
            using (new AvePerformanceScope("Restore.AveSPItem.EnsureItemSchemaDependency_1"))
            {
                return EnsureItemSchemaDependency(userData, skipItemWhenNotFound, skipItemWhenConflict, ctRestoreOption, fieldRestoreOption, true);
            }
        }

        public bool EnsureItemSchemaDependency(Dictionary<string, object> userData, bool skipItemWhenNotFound, bool skipItemWhenConflict, AveContentTypeRestoreOption ctRestoreOption, AveFieldRestoreOption fieldRestoreOption, bool throwException)
        {
            using (new AvePerformanceScope("Restore.AveSPItem.EnsureItemSchemaDependency_2"))
            {
                return EnsureItemSchemaDependency(userData, null, skipItemWhenNotFound, skipItemWhenConflict, ctRestoreOption, fieldRestoreOption, throwException);
            }
        }

        public void ResetName(string newName)
        {
            mBaseItemInfo.Name = newName;
        }

        public void SetForceAddTerm(bool isForceAdd)
        {
            mBaseItemInfo.IsForceAddTerm = isForceAdd;
        }


        public void SetExcludeUrls(ICollection<string> urls)
        {
            this.hasExcludedUrl = true;
            AveReplaceProcessor.excludedUrls = urls;
        }

        public void SetMaxVersionDiff(int maxVersionDiff)
        {
            mBaseItemInfo.MaxVersionDiff = maxVersionDiff;
        }

        public void SetEnableEventReceiver(bool isEnable)
        {
            mBaseItemInfo.EnableEventReceiver = isEnable;
        }
        #region Obsolete method
        /// <summary>
        /// 为folder 反差ct和column使用，只是构建一个list空壳
        /// </summary>
        /// <param name="parentFolder"></param>
        [Obsolete("only used for AveSPFolder.EnsureCTFieldItem, may remove later")]
        public AveSPItem(AveSPFolder parentFolder)
        {
            mAveSPList = parentFolder.ParentList;
            mAveParentSite = parentFolder.ParentList.ParentWeb.ParentSite;
        }

        [Obsolete("no use now, will remove later")]
        public void PostAction()
        {
            //ResetParentListSetting();
        }

        [Obsolete("no use now, will remove later")]
        public void AddFields(IAveListItem spListItem, Dictionary<string, object> fieldMap, AveBaseItemInfo info)
        {
            mAveItem.AddFields(spListItem, fieldMap, info);
        }
        [Obsolete("no use now, will remove later")]
        public void AddFields(Dictionary<string, object> fieldMap)
        {
            AddFields(mAveItem.ListItem, fieldMap, mBaseItemInfo);
        }
        //public void ResetContentToFileShare()
        //{
        //    AveSPItemNativeInfo docInfo = new AveSPItemNativeInfo(mBaseItemInfo.SiteId, mParentFolder.ParentList.ParentWeb.SPWeb.ID, mBaseItemInfo.GUID, mBaseItemInfo.InternalVersion, mBaseItemInfo.Level, 0, mAveItem.File, null);
        //    mStorage.ConvertDBToFileSystem(docInfo);
        //}
        [Obsolete("no use now, will remove later")]
        public void SetPicProperty(int width, int heigth)
        {
            mPicWidth = width;
            mPicHeight = heigth;
        }
        /// <summary>
        /// Return type:
        /// 0, No confiction (there is not document in alldocs table)
        /// 1, Confilict with RecycleBin
        /// 2, Confilict with current document
        /// 3, Confilict with both current document and RecycleBin
        /// </summary>
        /// <param name="sqlConn"></param>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        //[Obsolete("no use now, will remove later")]
        //public int IsConflict(AveSqlConnection sqlConn, Guid siteId, Guid parentId, string name)
        //{

        //    using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPItem.IsConflict"))
        //    {

        //        //if (!AveEnvironment.IsSPInstalled) { return 0; } 
        //        sqlConn.ClearParameters();
        //        sqlConn.AddParameter("@SiteId", siteId);
        //        sqlConn.AddParameter("@ParentId", parentId);
        //        sqlConn.AddParameter("@LeafName", name);

        //        string cmdText = "SELECT DeleteTransactionId FROM AllDocs WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND LeafName=@LeafName";
        //        int conflictType = 0;
        //        using (SqlDataReader dr = sqlConn.ExecuteReader(cmdText))
        //        {
        //            while (dr.Read())
        //            {
        //                conflictType |= 2;
        //                break;
        //            }
        //        }

        //        cmdText = "SELECT DeleteTransactionId FROM AllDocs WHERE SiteId=@SiteId AND DeleteTransactionId<>0x AND ParentId=@ParentId AND LeafName=@LeafName";
        //        using (SqlDataReader dr = sqlConn.ExecuteReader(cmdText))
        //        {
        //            while (dr.Read())
        //            {
        //                conflictType |= 1;
        //                break;
        //            }
        //        }

        //        return conflictType;

        //    }

        //}
        /// <summary>
        /// get conflict type by tp_guid, only for a ListItem
        /// </summary>
        /// <param name="sqlConn"></param>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="tp_Guid"></param>
        /// <returns>Return type:
        /// 0, No conflict (there is not document in alldocs table)
        /// 1, conflict with RecycleBin
        /// 2, conflict with current item
        /// 3, conflict with both current item and RecycleBin</returns>
        //        [Obsolete("no use now, will remove later")]
        //        public static int IsListItemConflict(AveSqlConnection sqlConn, Guid siteId, Guid parentId, Guid tp_Guid)
        //        {

        //            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPItem.IsListItemConflict"))
        //            {

        //                //if (!AveEnvironment.IsSPInstalled) { return 0; } 
        //                sqlConn.ClearParameters();
        //                sqlConn.AddParameter("@tp_SiteId", siteId);
        //                sqlConn.AddParameter("@tp_ParentId", parentId);
        //                sqlConn.AddParameter("@tp_Guid", tp_Guid);

        //                const string cmdText = @"SELECT Distinct tp_DeleteTransactionId from AllUserData 
        //                                        WHERE tp_SiteId=@tp_SiteId and tp_ParentId=@tp_ParentId 
        //                                        and tp_GUID=@tp_Guid and tp_IsCurrentVersion=1;";
        //                int conflictType = 0;
        //                using (SqlDataReader dr = sqlConn.ExecuteReader(cmdText))
        //                {
        //                    while (dr.Read())
        //                    {
        //                        byte[] transactionId = dr.GetSqlBinary(0).Value;
        //                        if (transactionId.Length > 0)
        //                        {
        //                            conflictType |= 1;
        //                        }
        //                        else
        //                        {
        //                            conflictType |= 2;
        //                        }
        //                    }
        //                }
        //                return conflictType;

        //            }

        //        }
        //        [Obsolete("no use now, will remove later")]
        //        public bool CreateVersionByNative(int version, RestoringDto restoringDto)
        //        {
        //            if (!mAveParentSite.ObjectModelFactory.IsSPInstalled) { return false; }
        //            return mAveItem.CreateVersionByNative(mBaseItemInfo, version, restoringDto);

        //        }

        [Obsolete("no use now, will remove later")]
        public void InitFieldsInMetaInfo(Dictionary<string, string> metaInfoDic)
        {
            if (metaInfoDic != null)
            {
                fieldsInMetaInfo = ParentFolder.ParentList.AveFields.GetFieldValuesInMetaInfo(-1, mBaseItemInfo.Version, metaInfoDic, mAveSPList.ParentWeb.SPWeb.ID, mBaseItemInfo.ListId);
                mBaseItemInfo.FieldsInfo.FieldsInMetaInfo = fieldsInMetaInfo;
            }
        }
        [Obsolete("no use now, will remove later")]
        public IAveFile LoadCheckOutFile(IAveWeb mSPWeb, Guid fileId, IAveUser iAveUser)
        {
            return mAveItem.LoadCheckOutFile(mSPWeb, fileId, iAveUser);
        }
        #endregion

        #region IAveSPItem Members


        IAveSPFolder IAveSPItem.ParentFolder
        {
            get { return mParentFolder; }
        }

        IAveSPList IAveSPItem.ParentList
        {
            //get { return mParentFolder.ParentList; }
            get { return mAveSPList; }
        }

        IAveSPSite IAveSPItem.ParentSite
        {
            get { return mAveParentSite; }
        }

        IAveSPWeb IAveSPItem.ParentWeb
        {
            get { return mParentWeb; }
        }

        #endregion
        public void Dispose()
        {
            report.Dispose();
        }

        /// <summary>
        /// Verify Item Metadata Dependency
        /// </summary>
        /// <param name="documentMetadataDto"></param>
        /// <param name="spFileRestoreOption"></param>
        internal void VerifyItemMetadataDependency(SPDocumentMetadataDto documentMetadataDto, SPFileRestoreOption spFileRestoreOption)
        {
            if (spFileRestoreOption == null)
            {
                throw new ArgumentNullException("spFileRestoreOption");
            }

            if (documentMetadataDto == null)
            {
                throw new ArgumentNullException("documentMetadataDto");
            }

            VerifyItemMetadataDependency(documentMetadataDto.UserDataInfo, spFileRestoreOption.MetadataRestoreOption);

        }

        /// <summary>
        /// Verify Item Metadata Dependency
        /// </summary>
        /// <param name="listItemMetadataDto"></param>
        /// <param name="spListItemRestoreOption"></param>
        internal void VerifyItemMetadataDependency(SPListItemMetadataDto listItemMetadataDto, SPListItemRestoreOption spListItemRestoreOption)
        {
            if (spListItemRestoreOption == null)
            {
                throw new ArgumentNullException("spListItemRestoreOption");
            }

            if (listItemMetadataDto == null)
            {
                throw new ArgumentNullException("listItemMetadataDto");
            }

            VerifyItemMetadataDependency(listItemMetadataDto.UserDataInfo, spListItemRestoreOption.MetadataRestoreOption);
        }

        protected void VerifyItemMetadataDependency(Dictionary<string, object> userDataInfo,
                                                  SPItemMetadataRestoreOption metadataRestoreOption)
        {
            if (metadataRestoreOption != null && metadataRestoreOption.VerifyDependency)
            {
                if (!ParentList.IsSystemList)
                {
                    EnsureItemSchemaDependency(userDataInfo,
                                               metadataRestoreOption.DependencyNotFoundAction ==
                                               SPItemMetadataDependencyNotFoundAction.SkipItem,
                                               metadataRestoreOption.DependencyConflictAction ==
                                               SPItemMetadataDependencyConflictAction.SkipItem,
                                               metadataRestoreOption.ContentTypeRestoreOption,
                                               metadataRestoreOption.FieldRestoreOption);
                }
            }
        }

        /// <summary>
        /// Prepare before restore
        /// </summary>
        internal void PreRestore(IAveRestoreStream restoreStream, Action<AveUserList> filterUser, Action<AveGroupList> filterGroup)
        {
            this.mAveSPList.BackupListSetting();

            var userCaches = restoreStream.TryReadMetadataList(AveMetadataType.UserCache);
            var groupCaches = restoreStream.TryReadMetadataList(AveMetadataType.GroupCache);

            if (userCaches != null)
            {
                if (filterUser != null)
                {
                    userCaches.ForEach(metadata =>
                        {
                            var userInfo = metadata.GetMetadata<AveUserList>();
                            filterUser(userInfo);
                            ParentSite.RestoreUser(userInfo);
                        });
                }
                else
                {
                    userCaches.ForEach(metadata => ParentSite.RestoreUser(metadata.GetMetadata<AveUserList>()));
                }
            }
            if (groupCaches != null)
            {
                if (filterGroup != null)
                {
                    groupCaches.ForEach(metadata =>
                    {
                        var groupInfo = metadata.GetMetadata<AveGroupList>();
                        filterGroup(groupInfo);
                        ParentSite.RestoreGroup(groupInfo);
                    });
                }
                else
                {
                    groupCaches.ForEach(metadata => ParentSite.RestoreGroup(metadata.GetMetadata<AveGroupList>()));
                }
            }
        }

        public void ResetRelatedItemsFieldValue(int docRowId)
        {
            if (relatedItemsInfo != null)
            {
                this.ParentSite.MappingManager.SiteMappingManager.AddRelatedItemsFieldValue(relatedItemsInfo.WebId, relatedItemsInfo.ListId, docRowId, relatedItemsInfo.Version, relatedItemsInfo.Schema);
            }
        }
    }
}

public enum CamlQueryFindOption
{
    Or,
    And
}