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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.Client;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.Common
{
    class AveItem : AveClientObject, IAveItem,IDisposable
    {
        private AveWeb mWeb;
        private AveList mList;
        private AveFolder mFolder;
        private AveListItem mListItem;
        private IAveRequest mRequest;
        private AveBaseItemInfo mBasicItemInfo;
        private AveFile mFile;
        private AveView mView;
        //since there are some performance issue when get AveListItem's Versions, we cache it if cannot find better method        
        [ThreadStatic]
        private static AveListItem CurrentListItemCaches = null;
        static AveLogger mLogger = AveLogger.GetInstance(typeof(AveItem));
        private IReport mReport;
        public IReport Report
        {
            get
            {
                if (mReport == null)
                {
                    mReport = new AveWrapperReport();
                }
                return mReport;
            }
        }
        public void SetReport(IReport report)
        {
            mReport = report;
        }
        
        private List<Dictionary<string, object>> UserDataJunction;

        public string OwnerLoginName
        {
            get { throw new NotImplementedException(); }
        }

        public AveItem(IAveSite site)
        {
            mRequest = (site as AveSite).Request;
        }

        public AveItem(IAveWeb web, IAveList list) : this(web.Site)
        {
            mWeb = web as AveWeb;
            mList = list as AveList;
            mRequest = (web.Site as AveSite).Request;
        }
        
        public AveItem(AveBaseItemInfo BasicItemInfo,IAveFolder folder)
        {
            mBasicItemInfo = BasicItemInfo;
            mFolder = folder as AveFolder;
            mList = mFolder.ParentList as AveList;
            mWeb = mList.ParentWeb as AveWeb;
            mRequest = (mWeb.Site as AveSite).Request;
        }

        public AveItem(AveBaseItemInfo BasicItemInfo, IAveFolder folder, IAveList list)
        {
            mBasicItemInfo = BasicItemInfo;
            mFolder = folder as AveFolder;
            mList = list as AveList;
            mWeb = mList.ParentWeb as AveWeb;
            mRequest = (mWeb.Site as AveSite).Request; 
        }

        public AveItem(AveBaseItemInfo BasicItemInfo, IAveFolder folder, IAveWeb web, IAveList list)
        {
            mBasicItemInfo = BasicItemInfo;
            mFolder = folder as AveFolder;
            mList = list as AveList;
            mWeb = web as AveWeb;
            mRequest = (mWeb.Site as AveSite).Request;      
            try
            {
                if (mBasicItemInfo.RowId > 0)
                {                    
                    if (AveItem.CurrentListItemCaches != null)
                    {
                        AveListItem tempListItem = AveItem.CurrentListItemCaches;
                        if (tempListItem.UniqueId.Equals(mBasicItemInfo.GUID))
                        {
                            mListItem = tempListItem;
                        }
                        else
                        {
                            mListItem = mList.GetItemById(mBasicItemInfo.RowId) as AveListItem;                           
                            AveItem.CurrentListItemCaches = mListItem;
                        }
                    }
                    else
                    {
                        mListItem = mList.GetItemById(mBasicItemInfo.RowId) as AveListItem;
                        AveItem.CurrentListItemCaches = mListItem;
                    }
                }
            }
            catch(Exception ex)
            {
                mLogger.Warn("AveItem:{0} constructor failed.Error Message:{1}.",BasicItemInfo.ServerRelativeUrl,ex.ToString());
            }
            try
            {
                if (mListItem != null && mListItem.File != null)
                {
                    mFile = mListItem.File as AveFile;
                }
                else if (mBasicItemInfo.ItemType == AveItemType.Document && !string.IsNullOrEmpty(mBasicItemInfo.ServerRelativeUrl))
                {
                    mFile = mWeb.GetFile(mBasicItemInfo.ServerRelativeUrl) as AveFile;
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("AveItem:{0} constructor failed.Error Message:{1}.", BasicItemInfo.ServerRelativeUrl, ex.ToString());
            }
        }

        internal AveList List
        {
            get
            {
                return mList;
            }
        }

        #region IAveItem Members

        public Dictionary<string, object> GetDocInfo(AveBaseItemInfo baseItemInfo)
        {
            if (baseItemInfo == null || (baseItemInfo.RowId <= 0 && string.IsNullOrEmpty(mBasicItemInfo.ServerRelativeUrl)))            
            {
                return null;
            }
            Dictionary<string, object> docInfo = new Dictionary<string,object>();
            if (baseItemInfo is AveListItemInfo)
            {
                if (mList != null && baseItemInfo.RowId > 0)
                {
                    AveListItem listItem = null;
                    if (this.mListItem != null && this.mListItem.ID == baseItemInfo.RowId)
                    {
                        listItem = this.mListItem;
                    }
                    else
                    {
                        listItem = mList.GetItemById(baseItemInfo.RowId) as AveListItem;
                    }
                    listItem.GetDocInfo(baseItemInfo, docInfo);
                }
            }
            else if (baseItemInfo is AveFileInfo)
            {
                if (string.IsNullOrEmpty(mBasicItemInfo.ServerRelativeUrl))
                {
                    return null;
                }
                AveFile file = null;
                if (this.mFile != null && this.mFile.ServerRelativeUrl.Equals(baseItemInfo.ServerRelativeUrl))
                {
                    file = this.mFile;
                }
                else
                {
                    file = mWeb.GetFile(baseItemInfo.ServerRelativeUrl) as AveFile;
                }
                file.GetDocInfo(baseItemInfo, docInfo);
            }
            else if (baseItemInfo is AveFolderInfo)
            {
                AveFolder folder = mWeb.GetFolder(baseItemInfo.ServerRelativeUrl) as AveFolder;
                folder.GetDocInfo(baseItemInfo, docInfo);
            }
            return docInfo;
        }

        public Dictionary<string, object> GetAttachmentInfo(AveBaseItemInfo baseItemInfo)
        {
            if (baseItemInfo == null || string.IsNullOrEmpty(baseItemInfo.ServerRelativeUrl))
            {
                return null;
            }
            Dictionary<string, object> attachmentInfo = new Dictionary<string, object>();
            AveFile file= mWeb.GetFile(baseItemInfo.ServerRelativeUrl) as AveFile;
            attachmentInfo.Add("Title", file.Name);
            attachmentInfo.Add("Created", file.TimeCreated);
            attachmentInfo.Add("Modified", file.TimeLastModified);
            return attachmentInfo;
        }

        public Dictionary<string, object> GetUserData(AveBaseItemInfo baseItemInfo)
        {
            if (baseItemInfo == null || (baseItemInfo.RowId <= 0 && string.IsNullOrEmpty(mBasicItemInfo.ServerRelativeUrl)))           
            {
                return null;
            }
            AveListItem listItem = null;
            if (this.mListItem != null && this.mListItem.ID == baseItemInfo.RowId)
            {
                listItem = this.mListItem;
            }
            else
            {
                if (mList != null && baseItemInfo.RowId >0)
                {
                    listItem = mList.GetItemById(baseItemInfo.RowId) as AveListItem;
                }
            }
            if (listItem != null)
            {//Generally, document & item will run this case
                return listItem.GetUserData(baseItemInfo, ref this.UserDataJunction);
            }           
            return null;
        }

        public List<Dictionary<string, object>> GetItemUserData(AveBaseItemInfo baseItemInfo)
        {
            throw new NotImplementedException();
        }

        public int GetParnetIdByThreadIndex(Guid listId, byte[] threadIndex)
        {
            throw new NotImplementedException();
        }

        public int GetInternalVersion(Guid itemId, int version)
        {
            throw new NotImplementedException();
        }

        public int GetDDocFlag(AveBaseItemInfo info)
        {
            throw new NotImplementedException();
        }

        public byte[] GetRbsIdByNative(AveBaseItemInfo info)
        {
            return null;
        }

        public List<AveRBSStubInfo13> GetRbsIdListByNative(AveBaseItemInfo info)
        {
            return null;
        }

        public AveStorageInfo GetStorageInfo(AveBaseItemInfo itemInfo, byte[] rbsId, bool IsBackupLinkForArchivedData, string activeProviderName)
        {
            throw new NotImplementedException();
        }

        public AveStubDataType GetEBSDataType(AveBaseItemInfo info)
        {
            throw new NotImplementedException();
        }

        public AveStubDataType GetRBSDataType(AveBaseItemInfo itemInfo, byte[] rbsId)
        {
            throw new NotImplementedException();
        }

        public AveStubDataType GetRBSDataType(byte[] RBSBlobId, byte[] rbsId)
        {
            throw new NotImplementedException();
        }

        public string GetStubInfoByNative(AveBaseItemInfo info)
        {
            throw new NotImplementedException();
        }

        public AveStubDataType GetStubDataType(AveBaseItemInfo info, byte[] rbsId)
        {
            throw new NotImplementedException();
        }
        internal void CheckConflictStateForVariationLabels(RestoringDto restoringDto, string labelName, bool isSource)
        {
            var query = new AveCamlQuery()
            {
                ViewXml = string.Format("<View><Query><Where><Eq><FieldRef Name='Title' /><Value Type='Text'>{0}</Value></Eq></Where></Query></View>", labelName),
            };
            var items = List.GetItems(query);
            if (items.Count > 0)
            {
                restoringDto.ConflictType = ConflictType.Document;
                restoringDto.ConflictRowId = items[0].ID;
                if (restoringDto.OverWrite && isSource != IsSourceLabel(items[0]))
                {
                    var errorMsg = isSource ?
                        string.Format("Cannot overwrite source label to target label, label name:{0}", labelName) :
                        string.Format("Cannot overwrite target label to source label, label name:{0}", labelName);
                    throw new InvalidOperationException(errorMsg);
                }
            }
            else
            {
                restoringDto.ConflictType = ConflictType.None;
                restoringDto.ConflictRowId = -1;
            }
        }

        private static bool IsSourceLabel(IAveListItem item)
        {
            bool isSource = false;
            try
            {
                isSource = (bool)item["Is_x0020_Source"];
            }
            catch (ArgumentException)
            { }
            return isSource;
        }

        internal void CheckConflictStateForRelationshipsList(RestoringDto restoringDto, string objectID)
        {
            var query = new AveCamlQuery()
            {
                ViewXml = string.Format("<View><Query><Where><Eq><FieldRef Name='ObjectID' /><Value Type='URL'>{0}</Value></Eq></Where></Query></View>", objectID),
            };
            var items = List.GetItems(query);
            if (items.Count > 0)
            {
                restoringDto.ConflictType = ConflictType.Document;
                restoringDto.ConflictRowId = items[0].ID;
            }
            else
            {
                restoringDto.ConflictType = ConflictType.None;
                restoringDto.ConflictRowId = -1;
            }
        }
        public List<Dictionary<string, object>> GetUserDataJunction(AveBaseItemInfo baseItemInfo)
        {
            using (new AvePerformanceScope("AvePoint.ObjectModel.Common.AveItem.GetUserDataJunction"))
            {
                if (this.UserDataJunction != null && this.UserDataJunction.Count > 0)
                {
                    return this.UserDataJunction;
                }
                if (baseItemInfo == null || baseItemInfo.RowId <= 0)
                {
                    return null;
                }
                AveListItem listItem = null;
                if (this.mListItem != null && this.mListItem.ID == baseItemInfo.RowId)
                {
                    listItem = this.mListItem;
                }
                else
                {
                    if (mList != null && baseItemInfo.RowId > 0)
                    {
                        try
                        {
                            listItem = mList.GetItemById(baseItemInfo.RowId) as AveListItem;
                        }
                        catch (Exception ex)
                        {
                            mLogger.Warn("Get Item:{0} by Id failed.Error Message:{1}.", baseItemInfo.ServerRelativeUrl, ex.ToString());
                        }
                    }
                }
                if (listItem == null)
                {
                    return null;
                }
                IAveListItemVersion itemVersion = listItem.Versions.GetVersionFromID(baseItemInfo.Version);
                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                Dictionary<string, object> dataCache = null;
                if (itemVersion != null)
                {
                    foreach (IAveField field in itemVersion.Fields)
                    {
                        try
                        {
                            dataCache = null;
                            object fieldValue = itemVersion[field.InternalName];
                            if (fieldValue == null)
                            {
                                continue;
                            }
                            if (field is IAveFieldUser)
                            {
                                IAveFieldUser userField = field as IAveFieldUser;
                                if (userField.AllowMultipleValues)
                                {
                                    List<string> value = (List<string>)fieldValue;
                                    if (value == null) continue;
                                    foreach (string temp in value)
                                    {
                                        if (temp.IndexOf(';') > 0)
                                        {
                                            int tempValue = 0;
                                            bool result = int.TryParse(temp.Substring(0, temp.IndexOf(';')), out tempValue);
                                            if (result)
                                            {
                                                dataCache = new Dictionary<string, object>();
                                                dataCache["tp_FieldId"] = userField.ID;
                                                dataCache["tp_Id"] = tempValue;
                                                dataCache["tp_UIVersion"] = baseItemInfo.Version;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (field is IAveFieldLookup)
                            {
                                IAveFieldLookup lookupField = field as IAveFieldLookup;
                                if (lookupField.AllowMultipleValues)
                                {
                                    List<string> value = (List<string>)fieldValue;
                                    if (value == null) continue;
                                    foreach (string temp in value)
                                    {
                                        if (temp.IndexOf(';') > 0)
                                        {
                                            int tempValue = 0;
                                            bool result = int.TryParse(temp.Substring(0, temp.IndexOf(';')), out tempValue);
                                            if (result)
                                            {
                                                dataCache = new Dictionary<string, object>();
                                                dataCache["tp_FieldId"] = lookupField.ID;
                                                dataCache["tp_Id"] = tempValue;
                                                dataCache["tp_UIVersion"] = baseItemInfo.Version;
                                            }
                                        }
                                    }
                                }
                            }

                            else if (field.TypeAsString == "TaxonomyFieldTypeMulti")
                            {
                                //to do 
                            }
                            if (dataCache != null)
                            {
                                data.Add(dataCache);
                            }
                        }
                        catch (Exception e)
                        {
                            object obj = itemVersion[field.Title];
                            mLogger.Debug(AveObjectModel_CommonResource.GetUserDataJunctionError, field.Title, obj == null ? string.Empty : obj.ToString(), this.mBasicItemInfo.ServerRelativeUrl, e.ToString());
                            //mLog.Warn("FieldName:{0}. FieldValue:{1}", field.Title, itemVersion[field.Title].ToString());
                        }
                    }
                }
                return data;
            }
        }

        #endregion

        public Dictionary<string, object> GetDocInfo(AveBaseItemInfo baseItemInfo, Dictionary<string, object> currentVersionDocData)
        {
            using (new AvePerformanceScope("AvePoint.ObjectModel.Common.AveItem.GetDocInfo"))
            {
                Dictionary<string, object> dataCache = new Dictionary<string, object>();
                if (currentVersionDocData != null && currentVersionDocData.Count > 0)
                {
                    foreach (KeyValuePair<string, object> ele in currentVersionDocData)
                    {
                        dataCache.Add(ele.Key, ele.Value);
                    }
                    return dataCache;
                }
                if (baseItemInfo == null || (baseItemInfo.RowId <= 0 && string.IsNullOrEmpty(baseItemInfo.ServerRelativeUrl)))
                {
                    return dataCache;
                }
                AveListItem listItem = null;
                if (baseItemInfo.RowId > 0)
                {
                    if (this.mListItem != null && this.mListItem.ID == baseItemInfo.RowId)
                    {
                        listItem = this.mListItem;
                    }
                    else
                    {
                        if (mList != null && baseItemInfo.RowId > 0)
                        {
                            try
                            {
                                listItem = mList.GetItemById(baseItemInfo.RowId) as AveListItem;
                            }
                            catch (Exception ex)
                            {
                                mLogger.Warn("Get Item:{0} by Id failed.Error Message:{1}.", baseItemInfo.ServerRelativeUrl, ex.ToString());
                            }
                        }

                    }
                }
                if (baseItemInfo.RowId > 0)
                {
                    if (listItem != null)
                    {
                        //Generally, document & item will run this case
                        listItem.GetDocInfo(baseItemInfo, dataCache);
                    }
                    else
                    {
                        throw new FileNotFoundException(ServerAPIResource.FileNotFoundException);
                    }
                }
                else if (baseItemInfo.ItemType == AveItemType.Document)
                {
                    if (string.IsNullOrEmpty(mBasicItemInfo.ServerRelativeUrl))
                    {
                        return null;
                    }
                    AveFile file = null;
                    if (this.mFile != null && !string.IsNullOrEmpty(this.mFile.ServerRelativeUrl) && this.mFile.ServerRelativeUrl.Equals(baseItemInfo.ServerRelativeUrl))
                    {
                        file = this.mFile;
                    }
                    else
                    {
                        file = mWeb.GetFile(baseItemInfo.ServerRelativeUrl) as AveFile;
                    }
                    file.GetDocInfo(baseItemInfo, dataCache);
                }
                else if (baseItemInfo.ItemType == AveItemType.Folder)
                {
                    AveFolder folder = mWeb.GetFolder(baseItemInfo.ServerRelativeUrl) as AveFolder;
                    dataCache = folder.GetDocInfo(baseItemInfo, dataCache);
                }
                return dataCache;
            }
        }

        public int GetInternalVersion(AveBaseItemInfo info)
        {
            return 0;
        }

        public int GetDocFlag(AveBaseItemInfo info)
        {
            return 0;
        }


        public int GetCheckOutUserId(AveBaseItemInfo info)
        {
            throw new NotImplementedException();
        }

        public List<int> GetDocVersions(AveBaseItemInfo baseItemInfo)
        {
            using (new AvePerformanceScope("AvePoint.ObjectModel.Common.AveItem.GetDocVersions"))
            {
                List<int> AllVersions = null;
                AveListItem listItem = null;
                if (mList == null || baseItemInfo == null || baseItemInfo.RowId <= 0)
                {
                    return null;
                }
                //Dictionary<string, object> docInfo = null;
                if (this.mListItem != null && this.mListItem.ID == baseItemInfo.RowId)
                {
                    listItem = this.mListItem;
                }
                else
                {
                    if (mList != null && baseItemInfo.RowId > 0)
                    {
                        listItem = mList.GetItemById(baseItemInfo.RowId) as AveListItem;
                    }
                }
                if (listItem != null)
                {//Generally, document & item will run this case
                    var versions = listItem.Versions.Select<IAveListItemVersion, int>(v => { return v.VersionId; });
                    AllVersions = versions.ToList<int>();
                }
                else if (baseItemInfo.ItemType == AveItemType.Document)
                {//only attachment will run this case
                    AveFile file = null;
                    if (this.mFile != null && this.mFile.ServerRelativeUrl.Equals(baseItemInfo.ServerRelativeUrl))
                    {
                        file = this.mFile;
                    }
                    else
                    {
                        file = mWeb.GetFile(baseItemInfo.ServerRelativeUrl) as AveFile;
                    }
                    var versions = file.Versions.Select<IAveFileVersion, int>(v => { return v.ID; });
                    AllVersions = versions.ToList<int>();
                }
                return AllVersions;
            }
        }
        #region IAveItem Members


        public int GetAttachmentSize(AveBaseItemInfo info)
        {
            return 0;
        }

        int? IAveItem.GetInternalVersion(AveBaseItemInfo info)
        {
            return 0;
        }

        public IAveFile File
        {
            get
            {
                if (mFile == null && !string.IsNullOrEmpty(mBasicItemInfo.ServerRelativeUrl))
                {
                    mFile = mWeb.GetFile(mBasicItemInfo.ServerRelativeUrl) as AveFile;
                }
                return mFile; 
            }
            set
            {
                mFile = value as AveFile;
            }
        }

        public IAveListItem ListItem
        {
            get
            {
                if (mListItem == null && mBasicItemInfo.RowId > 0)
                {
                    mListItem = mList.GetItemById(mBasicItemInfo.RowId) as AveListItem;
                }
                return mListItem;
            }
            set
            {
                mListItem = value as AveListItem;
            }
        }

        public Dictionary<string, string> GetItemViewFields(AveBaseItemInfo info, Dictionary<string, object> tempUserData, IAveListItem listItem)
        {
            throw new NotImplementedException();
        }

        public int ChangeItemId(Guid siteId, Guid id, Guid rootFolderId, int itemType, int fromId, int toId)
        {
            return -1;
        }

        public void UpdateAllDocsPropertyByNative(AveBaseItemInfo mBaseItemInfo, DateTime timeCreated, DateTime timeLastModified, int version)
        {
            throw new NotImplementedException();
        }

        public bool CreateVersionByNative(AveBaseItemInfo mBaseItemInfo, int version, RestoringDto restoringDto)
        {
            throw new NotImplementedException();
        }

        public void InitBySPListItem(IAveListItem listItem)
        {
            
        }

        public void UpdateFields(Dictionary<string, object> fieldData, AveBaseItemInfo info)
        {
            UpdateFields(fieldData, info, false);
        }

        public IAveFile LoadCheckOutFile(IAveWeb web, Guid fileId, IAveUser user)
        {
            throw new NotImplementedException();
        }

        public IAveFile GetFile(string name)
        {
            string folderPath = mFolder.ServerRelativeUrl.TrimEnd('/') + "/" + name;
            return mWeb.GetFile(folderPath);
        }

        public IAveFile GetFile()
        {
            return this.File == null ?
                    this.mWeb.GetFile(mBasicItemInfo.ServerRelativeUrl) : this.File;
        }

        public IAveFolder Folder
        {
            get
            {
                return this.mFolder;
            }
            set
            {
                this.mFolder = value as AveFolder;
            }
        }

        #endregion

        public void AddFields(IAveListItem spListItem, Dictionary<string, object> fieldMap, AveBaseItemInfo info)
        {
            throw new NotImplementedException();
        }

        #region IAveItem Members


        public IAveWeb Web
        {
            get
            {
                return mWeb;
            }
            set
            {
                mWeb = value as AveWeb;
            }
        }

        #endregion


        public int GetCurrentUIVersion(Guid siteId, IAveListItem item)
        {
            return (int)item["_UIVersion"];
        }

        public void InsertIntoAllUserDatajunction(IAveListItem item, Guid fieldId, Guid sourceListId, int id, int ordinal, int version)
        {
            throw new NotImplementedException();
        }

        public void UpdateColumnByNative(Guid siteId, IAveListItem item, int version, int rowOrdinal, string colName, object colValue)
        {
            throw new NotImplementedException();
        }


        public IAveView View
        {
            get
            {
                return mView;
            }
            set
            {
                mView = value as AveView;
            }
        }

        public Dictionary<string, object> GetItemCurrentVersionDocData(AveBaseItemInfo itemInfo)
        {
            return GetDocInfo(itemInfo, null);
        }

        public List<AveRoleAssignmentInfo> GetItemRoleAssignments(Guid siteId, Guid scopeId)
        {
            if (mListItem != null)
            {
                return mListItem.RoleAssignments.GetRoleAssignments(siteId);
            }
            return null;
        }

        #region IAveItem Members
        
        #endregion

        #region IAveItem Members


        public bool IsCheckOutFile(Guid siteId, Guid listId, int fileId, out int checkId, out Guid id)
        {
            throw new NotImplementedException();
        }

        public void ChangeCheckoutUserID(Guid siteId, Guid uniqueID, int newUserID)
        {
            throw new NotImplementedException();
        }


        public bool MoveToConflictFolder(IAveList parentList, IAveFolder parentFolder, IAveListItem listItem, bool isSourceWin)
        {
            return true;
        }

        #endregion


        void IAveItem.ReloadFile()
        {

        }

        void IAveItem.ReloadFile(bool fakeDeletedUser)
        {
            
        }

        public void UpdateFields(Dictionary<string, object> fieldMap, AveBaseItemInfo info, bool ThrowWhenUpdateFailed)
        {
            try
            {
                //UserInfoList's item can not update ContentType
                if (this.List != null && this.List.BaseTemplate == AveListTemplateType.UserInformation && fieldMap != null && fieldMap.ContainsKey("ContentType"))
                {
                    fieldMap.Remove("ContentType");
                }
                mListItem.DataCache.ChangedProperties.Add("ChangedFieldValues", fieldMap);
                mListItem.SystemUpdate();
                if (this.ListItem != null && this.ListItem.File != null && this.ListItem.File.ServerRelativeUrl.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    object title = string.Empty;
                    if (fieldMap.TryGetValue("Title", out title))
                    {
                        mListItem["Title"] = title;
                        mListItem.SystemUpdate();
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn(string.Format("Failed to update fields.Message:{0}.", ex.ToString()));
                if (ThrowWhenUpdateFailed)
                {
                    throw;
                }
            }
        }

        public List<AveTermStoreInfo> GetRelatedMetadataInfo(List<AveTaxFieldInfo> infos, AveBackupOption backupOption)
        {
            AveMetaDataServiceSerializer serializer = this.Web.Site.MetaDataServiceSerializer as AveMetaDataServiceSerializer;
            return serializer.GetRelatedMetadataInfo(this.Web.Site, infos, backupOption);
        }
        public List<AveTermStoreInfo> GetTermPropertyWebPartMetadataInfo(List<string> termPropertyWebPartInfos, AveBackupOption backupColumnOption)
        {
            AveMetaDataServiceSerializer serializer = this.Web.Site.MetaDataServiceSerializer as AveMetaDataServiceSerializer;
            return serializer.GetTermPropertyWebPartMetadataInfo(this.Web.Site, termPropertyWebPartInfos, backupColumnOption);
        }

        public bool SetTaxCatchAllValue(IAveListItem item, IAveFieldCollection fields)
        {
            return false;
        }

        public void ChangeDocdataByNative(Dictionary<string, object> docData)
        {
            
        }

        public void ChangeUserdataByNative(Dictionary<string, object> userData)
        {
            
        }

        public void Dispose()
        {
            
        }

        public Dictionary<string, object> GetListItemInfo(AveBaseItemInfo baseItemInfo, Dictionary<string, object> currentVersionDocData)
        {
            return GetDocInfo(baseItemInfo, currentVersionDocData);
        }

    }
}
