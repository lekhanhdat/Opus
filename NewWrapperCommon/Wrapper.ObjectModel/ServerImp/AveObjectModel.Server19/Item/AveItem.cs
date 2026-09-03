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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.ObjectModel.Server19.List;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using Microsoft.Office.DocumentManagement;
using Microsoft.Office.RecordsManagement.Holds;
using Microsoft.Office.RecordsManagement.RecordsRepository;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using AvePoint.GCommon.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Reflection;
using AvePoint.ObjectModel.Server19.NonPublicAPI;
using AvePoint.Wrapper.Restore;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.Report;

namespace AvePoint.ObjectModel.Server19
{
    class AveItem : IAveItem, IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveItem));
        private IReport mReport;
        public SharePointDocumentDataProcessor dataProcessor;
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
        internal AveWeb mWeb;
        internal AveList mList;
        internal AveSite mSite;
        //下面四个对象是与外围Wrapper的接口
        private AveListItem mListItem;
        private AveFile mFile;
        // only for system folder
        private AveFolder mFolder;
        public AveBaseItemInfo info;
        //下面四个对象为内部对象，统一维护在还原item/file/folder用到的SP对象
        internal SPFolder mParentFolder;
        internal SPFile mSPFile;
        internal SPListItem mSPListItem;
        internal SPList mSPList
        {
            get
            {
                if (mList == null)
                {
                    return null;
                }
                return mList.List;
            }
        }
        internal AveFolder mAveParentFolder;//为parent folder的reload提供使用的对象

        private Dictionary<string, object> userdataObjects = new Dictionary<string, object>();//记录更新userdata中的column
        private Dictionary<string, object> docdataObjects = new Dictionary<string, object>();//记录更新userdata中的column
        private readonly bool hasFullControlPermission;

        internal bool IsWelcomePage { set; get; }

        internal int mAveItemRestoreResult;
        private AveStorageInfo13 mStorageInfo;

        private string mOwnerLoginName = null;

        public string OwnerLoginName
        {
            get { return mOwnerLoginName; }
        }

        public string setupPath = null;

        public AveItem(IAveSite site)
        {
            mSite = site as AveSite;
            hasFullControlPermission = mSite == null ? false : mSite.NativeApiPermission == WrapperNativeApiPermission.FullControl;
        }

        public AveItem(IAveWeb web, IAveList list)
        {
            mWeb = web as AveWeb;
            mSite = web.Site as AveSite;
            mList = list as AveList;
            hasFullControlPermission = mSite == null ? false : mSite.NativeApiPermission == WrapperNativeApiPermission.FullControl;
        }

        public AveItem(AveBaseItemInfo info, AveList list)
        {
            this.info = info;
            mList = list;
            //mSPList = mList.List;
            mWeb = list.ParentWeb as AveWeb;
            mSite = mWeb.Site as AveSite;
            hasFullControlPermission = mSite == null ? false : mSite.NativeApiPermission == WrapperNativeApiPermission.FullControl;
            if (list != null)
            {
                mParentFolder = list.List.ParentWeb.GetFolder(info.ParentId);
            }
        }

        public AveItem(AveBaseItemInfo info, IAveFolder folder, IAveWeb web, IAveList list)
        {
            this.info = info;
            mParentFolder = (folder as AveFolder).Folder;  //need to do test for system folder //TODOLMM
            mAveParentFolder = folder as AveFolder;
            mList = list as AveList;
            folder.ParentList = mList;
            mWeb = web as AveWeb;
            mSite = web.Site as AveSite;
            hasFullControlPermission = mSite == null ? false : mSite.NativeApiPermission == WrapperNativeApiPermission.FullControl;
            //if (mList != null)
            //{
            //    mSPList = mList.List;
            //}
        }

        #region IAveItem Members

        public void SetReport(IReport report)
        {
            mReport = report;
        }

        /// <summary>
        /// 获取Document的信息
        /// </summary>
        /// <param name="itemInfo"></param>
        /// <param name="currentVersionDocData"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetDocInfo(AveBaseItemInfo itemInfo, Dictionary<string, object> currentVersionDocData)
        {
            var dataCache = GetItemInfo(itemInfo, currentVersionDocData);
            if (dataCache.Count <= 0)
            {
                dataCache = GetDocVersionInfo(itemInfo, currentVersionDocData);
            }

            if (itemInfo.ItemType == AveItemType.Document && !dataCache.ContainsKey("CustomizedPageStatus") && !string.IsNullOrEmpty(itemInfo.ServerRelativeUrl))
            {
                try
                {
                    var file = mWeb.GetFile(itemInfo.ServerRelativeUrl);
                    if (file.Exists)
                    {
                        dataCache["CustomizedPageStatus"] = file.CustomizedPageStatus;
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("Failed to backup CustomizedPageStatus, url: {0}, exception: {1}", itemInfo.ServerRelativeUrl, ex);
                }
            }
            return ProcessItemInfoData(itemInfo, currentVersionDocData, dataCache);
        }

        /// <summary>
        /// 获取ListItem的信息
        /// </summary>
        /// <param name="itemInfo"></param>
        /// <param name="currentVersionDocData"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetListItemInfo(AveBaseItemInfo itemInfo, Dictionary<string, object> currentVersionDocData)
        {
            var dataCache = GetItemInfo(itemInfo, currentVersionDocData);
            if (dataCache.Count <= 0)
            {
                dataCache = GetListItemVersionInfo(itemInfo, currentVersionDocData);
            }
            return ProcessItemInfoData(itemInfo, currentVersionDocData, dataCache);
        }

        private Dictionary<string, object> ProcessItemInfoData(AveBaseItemInfo itemInfo, Dictionary<string, object> currentVersionDocData, Dictionary<string, object> dataCache)
        {
            //Oliver:如果dataCache count为零，说明数据库中已经不存在这个Item了，应该以异常的形式report出去
            //对于RootFolder暂时去掉这个判断，RootFolder的RowId为-1
            if (dataCache.Count == 0 && itemInfo.RowId != -1)
            {
                throw new FileNotFoundException(ServerAPIResource.FileNotFoundException);
            }
            //有效率问题，改为用ScopeId判断
            if (currentVersionDocData != null && currentVersionDocData.ContainsKey("HasUniqueRoleAssignments") && !dataCache.ContainsKey("HasUniqueRoleAssignments"))
            {
                dataCache.Add("HasUniqueRoleAssignments", currentVersionDocData["HasUniqueRoleAssignments"]);
            }

            if (dataCache.ContainsKey("InternalVersion"))
            {
                itemInfo.InternalVersion = (int)dataCache["InternalVersion"];
            }
            // set IsVersion in dic

            if (!dataCache.ContainsKey("IsUserDocVersion"))
            {
                dataCache["IsUserDocVersion"] = false;
            }
            return dataCache;
        }

        private Dictionary<string, object> GetItemInfo(AveBaseItemInfo itemInfo, Dictionary<string, object> currentVersionItemData)
        {
            var dataCache = mAveParentFolder.GetDocDataFromCache(itemInfo) ?? new Dictionary<string, object>();
            if (dataCache.Count == 0)
            {
                mSite.QueryService.GetDocInfo(itemInfo, dataCache);
                //TODOREMOVE
#if DEBUG
                if (mAveParentFolder.EnableCache &&
                    (itemInfo.ItemType == AveItemType.Document || itemInfo.ItemType == AveItemType.ListItem) && itemInfo.RowId > 0 &&
                    dataCache.Count > 0)
                {
                    throw new Exception("DataCache failed");
                }
#endif
            }

            if (dataCache.Count > 0)
            {
                itemInfo.IsVersion = Convert.ToInt32(dataCache["IsCurrentVersion"]) != 1;
                itemInfo.IsCurrentVersion = !itemInfo.IsVersion;
                itemInfo.PageVersion = false;
                itemInfo.Level = (byte)dataCache["Level"];
                itemInfo.ScopeId = (Guid)dataCache["ScopeId"];
                itemInfo.ScopeUrl = string.Format("{0}/{1}", (string)dataCache["DirName"], (string)dataCache["LeafName"]);
                itemInfo.ScopeUrl = itemInfo.ScopeUrl.TrimStart('/');
                if (dataCache.ContainsKey("DocFlags"))
                {
                    itemInfo.DocFlag = (int)dataCache["DocFlags"];
                }
                if (dataCache.ContainsKey("HasStream"))
                {
                    itemInfo.HasStream = ((int)dataCache["HasStream"] == 1);
                }
                if (dataCache.ContainsKey("Size"))
                {
                    itemInfo.DocumentSize = (int)dataCache["Size"];
                }
            }
            return dataCache;
        }

        private Dictionary<string, object> GetDocVersionInfo(AveBaseItemInfo itemInfo, Dictionary<string, object> currentVersionItemData)
        {
            var dataCache = mAveParentFolder.GetVersionDataFromCache(itemInfo) ?? new Dictionary<string, object>();
            if (dataCache.Count == 0)
            {
                mSite.QueryService.GetVersionInfo(itemInfo, dataCache);
                //TODOREMOVE
#if DEBUG
                if (mAveParentFolder.EnableCache &&
                    (itemInfo.ItemType == AveItemType.Document || itemInfo.ItemType == AveItemType.ListItem) && itemInfo.RowId > 0 &&
                 dataCache.Count > 0)
                {
                    throw new Exception("DataCache failed");
                }
#endif
            }
            if (dataCache.Count > 0)
            {
                itemInfo.IsVersion = true;
                itemInfo.IsCurrentVersion = false;
                itemInfo.PageVersion = true;
                dataCache["IsUserDocVersion"] = itemInfo.IsVersion;
                itemInfo.Level = (byte)dataCache["Level"];
                if (dataCache.ContainsKey("DocFlags"))
                {
                    itemInfo.DocFlag = (int)dataCache["DocFlags"];
                }
                if (dataCache.ContainsKey("Size"))
                {
                    itemInfo.DocumentSize = (int)dataCache["Size"];
                    int internalVersion = dataCache.ContainsKey("InternalVersion") ? (int)dataCache["InternalVersion"] : 0;
                    itemInfo.HasStream = mSite.QueryService.GetDocHasStream(itemInfo, internalVersion);

                    if (!dataCache.ContainsKey("HasStream"))
                    {
                        dataCache.Add("HasStream", itemInfo.HasStream ? 1 : 0);
                    }
                    else
                    {
                        dataCache["HasStream"] = itemInfo.HasStream ? 1 : 0;
                    }
                }
                if (currentVersionItemData != null)
                {
                    if (currentVersionItemData.ContainsKey("SetupPath") && currentVersionItemData["SetupPath"] != null)
                    {
                        dataCache["SetupPath"] = currentVersionItemData["SetupPath"];
                    }
                    if (currentVersionItemData.ContainsKey("ScopeId") && currentVersionItemData["ScopeId"] != null)
                    {
                        itemInfo.ScopeId = (Guid)currentVersionItemData["ScopeId"];
                    }
                }
                dataCache["DoclibRowId"] = itemInfo.RowId;
            }
            return dataCache;
        }

        private Dictionary<string, object> GetListItemVersionInfo(AveBaseItemInfo itemInfo, Dictionary<string, object> currentVersionItemData)
        {
            var dataCache = mAveParentFolder.GetVersionDataFromCache(itemInfo) ?? new Dictionary<string, object>();
            if (dataCache.Count == 0)
            {
                mSite.QueryService.GetListItemVersionInfo(itemInfo, dataCache);
                //TODOREMOVE
#if DEBUG
                if (mAveParentFolder.EnableCache &&
                    (itemInfo.ItemType == AveItemType.Document || itemInfo.ItemType == AveItemType.ListItem) && itemInfo.RowId > 0 &&
                 dataCache.Count > 0)
                {
                    throw new Exception("DataCache failed");
                }
#endif
            }
            if (dataCache.Count > 0)
            {
                object isCurrentVersion = null;
                if (dataCache.TryGetValue("IsCurrentVersion", out isCurrentVersion) && isCurrentVersion != null)
                {
                    itemInfo.IsVersion = Convert.ToInt32(isCurrentVersion) != 1;
                }
                else
                {
                    itemInfo.IsVersion = true;
                }
                itemInfo.IsCurrentVersion = !itemInfo.IsVersion;
                itemInfo.PageVersion = true;
                if (dataCache.ContainsKey("Level") && dataCache["Level"] != null)
                {
                    itemInfo.Level = (byte)dataCache["Level"];
                }
                itemInfo.DocumentSize = 0;
                dataCache["IsUserDocVersion"] = itemInfo.IsVersion;
                if (dataCache.ContainsKey("DocFlags"))
                {
                    itemInfo.DocFlag = (int)dataCache["DocFlags"];
                }
                if (currentVersionItemData != null)
                {
                    if (currentVersionItemData.ContainsKey("SetupPath") && currentVersionItemData["SetupPath"] != null)
                    {
                        dataCache["SetupPath"] = currentVersionItemData["SetupPath"];
                    }
                    if (currentVersionItemData.ContainsKey("ScopeId") && currentVersionItemData["ScopeId"] != null)
                    {
                        itemInfo.ScopeId = (Guid)currentVersionItemData["ScopeId"];
                    }
                }
                dataCache["DoclibRowId"] = itemInfo.RowId;
            }
            return dataCache;
        }

        public Dictionary<string, object> GetAttachmentInfo(AveBaseItemInfo info)
        {
            return mSite.QueryService.GetAttachmentInfo(info);
        }

        public int GetParnetIdByThreadIndex(Guid siteId, Guid listId, byte[] threadIndex)
        {
            return mSite.QueryService.GetParentIdByThreadIndex(siteId, listId, threadIndex);
        }

        public Dictionary<string, object> GetUserData(AveBaseItemInfo baseItemInfo)
        {
            if (mList != null)
            {
                mList.LoadFieldMap();
            }
            Dictionary<string, object> userData = new Dictionary<string, object>();
            List<Dictionary<string, object>> rowDataList = null;
            rowDataList = mAveParentFolder.GetUserDataFromCache(baseItemInfo);
            if (rowDataList == null || rowDataList.Count == 0)
            {
                rowDataList = mSite.QueryService.GetUserData(baseItemInfo, mList.ColNameCollection);
                //TODOREMOVE
#if DEBUG
                if (mAveParentFolder.EnableCache &&
                    (baseItemInfo.ItemType == AveItemType.Document || baseItemInfo.ItemType == AveItemType.ListItem) && baseItemInfo.RowId > 0 &&
                    (rowDataList != null && rowDataList.Count != 0))
                {
                    throw new Exception("DataCache failed");
                }
#endif
            }

            foreach (Dictionary<string, object> rowData in rowDataList)
            {
                byte rowOrdinal = (byte)rowData["tp_RowOrdinal"];
                userData.Remove("#tp_RowOrdinal");
                if (mList != null)
                {
                    mList.ReplaceFieldNames(rowData, userData, rowOrdinal);
                }
            }

            if (userData.ContainsKey("#tp_ThreadIndex"))
            {
                byte[] threadIndex = (byte[])userData["#tp_ThreadIndex"];
                while (threadIndex.Length > 5)
                {
                    byte[] temp = new byte[threadIndex.Length - 5];
                    Array.Copy(threadIndex, temp, threadIndex.Length - 5);
                    int parentId = mSite.QueryService.GetParentIdByThreadIndex(baseItemInfo.SiteId, mList.Id, temp);
                    if ((parentId == 0) && (threadIndex.Length != 22))
                    {
                        threadIndex = temp;
                        continue;
                    }
                    if (threadIndex.Length >= 22)
                    {
                        userData.Add("#ThreadIndexParentId", parentId);
                        break;
                    }
                }
            }

            if (userData.ContainsKey("SolutionId") && baseItemInfo.MappingManager.SiteMappingManager.SolutionStatus != null)
            {
                Guid solutionId = new Guid(userData["SolutionId"].ToString());
                if (baseItemInfo.MappingManager.SiteMappingManager.SolutionStatus.ContainsKey(solutionId))
                {
                    userData.Add("#SolutionStatus", baseItemInfo.MappingManager.SiteMappingManager.SolutionStatus[solutionId]);
                }
                else
                {
                    userData.Add("#SolutionStatus", 0);
                }
            }

            if (mList != null && (int)mList.BaseTemplate == 160)
            {
                List<int> status = mSite.QueryService.GetItemsByColumnValue(mSite.ID, mList.ID, "int4", "0");
                if (!status.Contains((int)userData["#tp_ID"]))
                {
                    userData["#tp_isARLListItemTerminated"] = true;
                }
            }
            if (mList != null && mList.IsRelationshipsList())
            {
                AssemblyVariationLabelName(userData);
            }
            return userData;
        }
        /// <summary>
        /// 在备份Relationships List Item的时候通过Label Unique Id反找到Label Name
        /// </summary>
        /// <param name="userData"></param>
        private void AssemblyVariationLabelName(Dictionary<string, object> userData)
        {
            if (userData.ContainsKey("Label"))
            {
                var labelId = (Guid)userData["Label"];
                if (labelId != Guid.Empty)
                {
                    var labelName = mSite.GetVariationLabelName(labelId);
                    userData["Label"] = string.Format("{0};{1}", labelId.ToString(), labelName);
                }
            }
        }
        public List<AveTermStoreInfo> GetRelatedMetadataInfo(List<AveTaxFieldInfo> taxFieldInfos, AveBackupOption backupColumnOption)
        {
            AveMetaDataServiceSerializer serializer = mSite.MetaDataServiceSerializer as AveMetaDataServiceSerializer;
            return serializer.GetRelatedMetadataInfo(mSite, taxFieldInfos, backupColumnOption);
        }
        public List<AveTermStoreInfo> GetTermPropertyWebPartMetadataInfo(List<string> termPropertyWebPartInfos, AveBackupOption backupColumnOption)
        {
            AveMetaDataServiceSerializer serializer = mSite.MetaDataServiceSerializer as AveMetaDataServiceSerializer;
            return serializer.GetTermPropertyWebPartMetadataInfo(mSite, termPropertyWebPartInfos, backupColumnOption);
        }
        public List<Dictionary<string, object>> GetUserDataJunction(AveBaseItemInfo baseItemInfo)
        {
            if (mAveParentFolder.EnableCache &&
                (baseItemInfo.ItemType == AveItemType.Document || baseItemInfo.ItemType == AveItemType.ListItem) && baseItemInfo.RowId > 0)
            {
                return mAveParentFolder.GetUserDataJunctionFromCache(baseItemInfo);
            }
            else
            {
                return mSite.QueryService.GetUserDataJunction(baseItemInfo);
            }
        }

        public int? GetInternalVersion(AveBaseItemInfo itemInfo)
        {
            return mSite.QueryService.GetInternalVersion(itemInfo);
        }

        public int GetDocFlag(AveBaseItemInfo info)
        {
            try
            {
                return mSite.QueryService.GetDocFlag(info);
            }
            //ParentId == Guie.Empty, 对于Attachment会抛异常, 目前DocFlag仅用于判断EBS Stub, SP2016不支持, 故暂时返回0;
            catch (ArgumentNullException) { return 0; }
        }

        public byte[] GetRbsIdByNative(AveBaseItemInfo info)
        {
            return mSite.QueryService.GetRbsIdByNative(info);
        }

        public List<AveRBSStubInfo13> GetRbsIdListByNative(AveBaseItemInfo info)
        {
            return mSite.QueryService.GetRbsIdListByNative(info);
        }

        public string GetStubInfoByNative(AveBaseItemInfo info)
        {
            return mSite.QueryService.GetStubInfoByNative(info.SiteId, info.GUID, null == info.InternalVersion ? GetInternalVersion(info).Value : info.InternalVersion.Value);
        }

        public int GetCheckOutUserId(AveBaseItemInfo info)
        {
            return mSite.QueryService.GetCheckOutUserId(info);
        }

        public List<int> GetDocVersions(AveBaseItemInfo info)
        {
            var versionList = this.mAveParentFolder.GetDocVersionsFromCache(info);
            if (versionList == null || versionList.Count == 0)
            {
                versionList = mSite.QueryService.GetDocVersions(info);
                //TODOREMOVE
#if DEBUG
                if (mAveParentFolder.EnableCache &&
                    (info.ItemType == AveItemType.Document || info.ItemType == AveItemType.ListItem) && info.RowId > 0 &&
                    versionList.Count > 0)
                {
                    throw new Exception("DataCache failed");
                }
#endif
            }
            return versionList;
        }

        public int GetAttachmentSize(AveBaseItemInfo info)
        {
            return mSite.QueryService.GetAttachmentSize(info);
        }

        public Dictionary<string, string> GetItemViewFields(AveBaseItemInfo info, Dictionary<string, object> tempUserData, IAveListItem listItem)
        {
            Dictionary<string, string> vFields = new Dictionary<string, string>();
            Dictionary<string, object> nameToColname = new Dictionary<string, object>();
            //Dictionary<string, object> nameToColname = mFields.GetNameToColNameMapping();
            //TODO
            IAveFieldCollection curSPFields = listItem.ParentList.Fields;

            if (listItem.ParentList.DefaultView != null && tempUserData != null)
            {
                foreach (string iName in listItem.ParentList.DefaultView.ViewFields)
                {
                    try
                    {
                        object obj = null;
                        string temp = curSPFields.GetFieldByInternalName(iName).Title;
                        if (nameToColname.TryGetValue(temp, out obj))
                        {
                            string uN = obj.ToString();
                            object uV = null;
                            if (uN.Equals("tp_UIVersionString", StringComparison.OrdinalIgnoreCase))
                            {
                                uN = "tp_UIVersion";
                                if (tempUserData.TryGetValue(uN, out uV))
                                {
                                    string ver = "Version";
                                    if (uV is int)
                                    {
                                        int v = (int)uV;
                                        if (v % 512 == 0)
                                        {
                                            v = v / 512;
                                            vFields.Add(ver, v.ToString() + ".0");
                                        }
                                        else
                                        {
                                            int r = v % 512;
                                            v = v / 512;
                                            vFields.Add(ver, v.ToString() + "." + r.ToString());
                                        }
                                    }
                                }
                            }
                            else if (tempUserData.TryGetValue(uN, out uV))
                            {
                                if (string.IsNullOrEmpty(uV.ToString()))
                                {
                                    continue;
                                }
                                if (!vFields.ContainsKey(temp))
                                {
                                    if (temp.Equals("Created By") || temp.Equals("Modified By"))
                                    {
                                        try
                                        {

                                            vFields.Add(temp, listItem.ParentList.ParentWeb.SiteUsers.GetByID((int)uV).LoginName);
                                        }
                                        catch (Exception e)
                                        {
                                            logger.Log(AveLogLevel.DEBUG, ServerAPIResource.AddFieldError, e.ToString());
                                        }
                                    }
                                    else
                                    {
                                        vFields.Add(temp, listItem.ParentList.Fields.GetFieldByInternalName(iName).GetFieldValueAsText(uV));
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetItemFieldError, e.ToString());
                        continue;
                    }
                }
            }
            return vFields;
        }

        #endregion

        internal bool HasFullControlPermission
        {
            get
            {
                return hasFullControlPermission;
            }
        }

        /// <summary>
        /// 这个函数的主要目的是为保证Version是正确的，不保证还原document的field。不过需要保证调用这个之前List的ForceCheckOut属性是false
        /// add the out ref paramter filecurrentversion is for stub restore, if the file is new created the value will be -1 and if the file
        /// is already exist then the value will be the currentversion 
        /// </summary>
        /// <returns>
        /// 0 current version is bigger than original version, this case cannot be handled by API.
        /// 1 current version less original version,create a version equal original version
        /// 2 current version equal original version
        /// </returns>
        public int CreateANewFileOrVersion(IAveRestoreStream receiver, SPWeb web, SPList list, SPFolder folder,
            string fileName, int uiVersion, bool isCheckOut, string checkInComment, RestoringDto restoringDto, bool isGhostPage, string setupPath, List<SPListItem> holdItems, Hashtable HTMetaInfo, AveDocumentInfo docInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.CreateANewFileOrVersion"))
            {

                int compareResult = 1;
                SPFile file = null;
                //web下的RootFolder.ServerRelativeUrl结尾包括了"/"
                string folderServerRelativeUrl = folder.ServerRelativeUrl;
                if (!info.SettingInfo.DELETE_ITEM && info.IsCheckOut)
                {
                    file = LoadCheckOutFile(web, folder, fileName);
                }
                else
                {
                    try
                    {
                        if (folderServerRelativeUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                        {
                            file = web.GetFile(SPResourcePath.FromDecodedUrl(folderServerRelativeUrl + fileName));
                        }
                        else
                        {
                            file = web.GetFile(SPResourcePath.FromDecodedUrl(folderServerRelativeUrl + "/" + fileName));
                        }
                    }
                    catch (Exception e)//todo
                    {
                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetFileFaild, e.ToString());
                        file = null;
                    }
                }
                if (file != null && file.Exists)
                {
                    //fileVersion = file.UIVersion;
                    if (file.UIVersion > uiVersion)
                    {
                        if (!docInfo.IsThumbnails)
                        {
                            logger.Warn("Failed to restore only the historical versions  {0}. To restore historical versions in SharePoint 2016, select the desired versions along with the current version, and then perform the restore job.", fileName);
                            throw new AveWrapperSkipException(AveInternalResourceKey.Wrapper_Exception_Server16_FaileRestoreHistoricalVersions);//需要国际化
                                                                                                                                                //throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                        }
                        #region Delete for SP2013 by Austin
                        //InitBySPFile(file);
                        //if (!mSite.QueryService.CreateVersionByNative(info, uiVersion, restoringDto))
                        //{
                        //    InitBySPFile(file);
                        //    return AveRestoreResult.Omit;
                        //}
                        //compareResult = 0;
                        #endregion
                    }
                    else if (file.UIVersion < uiVersion)
                    {
                        if (!restoringDto.IsNewItem)
                        {
                            //return AveRestoreResult.Omit;
                            restoringDto.NeedSkipped = true;
                            throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                        }
                        //ADO-110334 对需要在checkout version上append version的情况，需要先将目的端本身文件checkin。
                        if (file.Level == SPFileLevel.Checkout)
                        {
                            try
                            {
                                file.CheckIn(string.Empty);
                                file = web.GetFile(file.UniqueId);
                            }
                            catch (Exception ex)
                            {
                                logger.Warn("An exception occurred while check in the file: {0}, exception: {1}", file.Name, ex.ToString());
                            }
                        }
                        file = CreateNewVersion(receiver, folder, file, fileName, false, uiVersion, isCheckOut, checkInComment, isGhostPage, setupPath, holdItems, HTMetaInfo, docInfo);
                        info.IsNewCreated = true;
                        compareResult = 1;
                    }
                    else
                    {
                        if (file.Level == SPFileLevel.Checkout && !isCheckOut)
                        {
                            RestoreWebPart(info as AveDocumentInfo, new AveFile(mWeb, file));
                            using (AvePerformanceScope scope = new AvePerformanceScope("SP.AveItem.CreateANewFileOrVersion.CheckIn"))
                            {
                                if (file.UIVersion % 512 > 0)
                                {
                                    file.CheckIn(checkInComment, SPCheckinType.MinorCheckIn);
                                }
                                else
                                {
                                    file.CheckIn(checkInComment);
                                }
                            }
                        }
                        if (info.HasStream && (!file.InDocumentLibrary || file.Item == null))
                        {
                            //TODO 这样每个这样的文件都会走两次SaveBinary 正常文件是没有问题
                            using (Stream stream = new AveSPFileStream(receiver))
                            {
                                try
                                {
                                    file.SaveBinary(file.OpenBinary());
                                }
                                catch (Exception ex)
                                {
                                    logger.Info("An exception occurred while trying to save binary, file: {0}, exception: {1}", file.Name, ex.ToString());
                                }
                                Stream newStream = ReplaceStreamBeforeAdd(stream, folder, null, fileName, info.IsCurrentVersion);
                                using (AvePerformanceScope scope = new AvePerformanceScope("SP.AveItem.CreateANewFileOrVersion.SaveBinary"))
                                {
                                    file.SaveBinaryExtension(newStream);
                                }
                            }
                        }
                        compareResult = 2;
                    }
                }
                else
                {
                    file = CreateNewVersion(receiver, folder, file, fileName, true, uiVersion, isCheckOut, checkInComment, isGhostPage, setupPath, holdItems, HTMetaInfo, docInfo);
                    info.IsNewCreated = true;
                    compareResult = 1;
                }
                InitBySPFile(file);
                return compareResult;

            }

        }

        /// <summary>
        ///ADO-142924：无法保证数据中一定同时存在以下四个field value,因此分别处理
        /// </summary>
        /// <param name="fieldMap"></param>
        /// <returns>
        /// true:存在对应属性，可以继续更新之后的Modify by field
        /// false:不存在对应属性，不需要继续更新之后的Modify by field
        /// </returns>
        private bool TryUpdateItemBasicInfo(Dictionary<string, object> fieldMap)
        {
            if (this.HasFullControlPermission)
            {
                bool needUpdate = false;
                needUpdate |= TrySetUserData("tp_Editor", "Editor", fieldMap);
                needUpdate |= TrySetUserData("tp_Author", "Author", fieldMap);
                needUpdate |= TrySetUserData("tp_Modified", "Modified", fieldMap);
                needUpdate |= TrySetUserData("tp_Created", "Created", fieldMap);
                if (needUpdate)
                {
                    UpdateDataByNative(false, true, true);
                    //ADO-186792 large items and indexed column in (createed,modified,editor,author) . The method will update the "NameValuePaire" table
                    //if (mSPListItem.ParentList.IsThrottled)     此方法会修改时间，暂时去掉。
                    //{
                    //    mSPListItem.SystemUpdate(false);
                    //}
                    return true;
                }
            }
            else
            {
                DateTime modifiedTime = default(DateTime);
                if (TrySetSpecialProperty(fieldMap, ref modifiedTime))
                {
                    bool bPrseverItemVersion = true;
                    bool bNoVersion = true;
                    if (info is AveDocumentInfo)
                    {
                        //Approve状态的信息可以通过InternalUpdate进行更新，这样可以减少一个update
                        if (info.ModerationStatus == 0 && mSPListItem.ModerationInformation != null)
                        {
                            mSPListItem.ModerationInformation.Comment = info.ModerationComments;
                        }
                    }
                    else
                    {
                        // 对于ListItem & Folder, Moderation Status和4个特殊的column一起更新，
                        //否则只能keep住一个，Moderation Status或者4个特殊的column
                        bNoVersion = false;
                        if (mSPListItem.ModerationInformation != null)
                        {
                            mSPListItem.ModerationInformation.Comment = info.ModerationComments;
                            mSPListItem.ModerationInformation.Status = (SPModerationStatusType)info.ModerationStatus;
                            //当同时更新Moderation和4个特殊的column的时候，bPrseverItemVersion需要设置成false，否则会删除第一个Publish version
                            //如果不更新Moderation，只更新4个特殊的column，需要设置成true，否则会涨version
                            bPrseverItemVersion = false;
                        }
                    }
                    if (!AveItem.AveItemSystemUpdate(mSPListItem, false, bPrseverItemVersion, info.ModerationStatus == 0, bNoVersion)
                        || (modifiedTime != default(DateTime) && ((DateTime)mSPListItem[SPBuiltInFieldId.Modified]).Millisecond != modifiedTime.Millisecond))
                    {
                        logger.Log(AveLogLevel.WARN, "Use SystemUpdate method to update item basic info which may not keep some column values. Item Url:{0}", mSPListItem.Url);
                        mSPListItem.SystemUpdate(false);
                    }
                    return true;
                }
            }
            return false;
        }

        private bool TrySetSpecialProperty(Dictionary<string, object> fieldMap, ref DateTime modifiedTime)
        {
            bool needUpate = false;
            int authorId;
            if (TryGetUserData<int>("Author", fieldMap, out authorId))
            {
                SPUser author = mWeb.Web.SiteUsers.GetByID(authorId);
                mSPListItem[SPBuiltInFieldId.Author] = author;
                needUpate = true;
            }


            int editorId;
            if (TryGetUserData<int>("Editor", fieldMap, out editorId))
            {
                SPUser editor = mWeb.Web.SiteUsers.GetByID(editorId);
                mSPListItem[SPBuiltInFieldId.Editor] = editor;
                needUpate = true;
            }

            if (TryGetUserData<DateTime>("Modified", fieldMap, out modifiedTime))
            {
                modifiedTime = mWeb.Web.RegionalSettings.TimeZone.UTCToLocalTime(modifiedTime);
                mSPListItem[SPBuiltInFieldId.Modified] = modifiedTime;
                needUpate = true;
            }

            DateTime createdTime;
            if (TryGetUserData<DateTime>("Created", fieldMap, out createdTime))
            {
                createdTime = mWeb.Web.RegionalSettings.TimeZone.UTCToLocalTime(createdTime);
                mSPListItem[SPBuiltInFieldId.Created] = createdTime;
                needUpate = true;
            }
            return needUpate;
        }

        private bool TryGetUserData<T>(string fieldMapKey, Dictionary<string, object> fieldMap, out T valueData)
        {
            object fieldValue;
            if (fieldMap.TryGetValue(fieldMapKey, out fieldValue))
            {
                valueData = (T)(((AveFieldValueInfo)fieldValue).ColValue);
                return true;
            }
            valueData = default(T);
            return false;
        }

        //// <summary>
        /// 使用反射的方式调用Internal API进行更新4个特殊的column并且保持version不变
        /// </summary>
        /// <param name="bNoVersion">to update document, please use true, or use false</param>
        /// <param name="bPrseverItemVersion">to update document, please use true, or use false</param>
        /// <param name="bPublish">if want to save to a publish version, set to true, otherwise set to false</param>
        /// <returns>更新成功返回true，调用方法出错或者是方法被改变则返回false</returns>
        internal static bool AveItemSystemUpdate(SPListItem item, bool bSystem, bool bPrseverItemVersion, bool bPublish, bool bNoVersion)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("Server.AveItem.AveItemSystemUpdate"))
            {

                //internal void UpdateInternal(bool bSystem, bool bPreserveItemVersion, Guid newGuidOnAdd, bool bMigration, 
                //bool bPublish, bool bNoVersion, bool bCheckOut, bool bCheckin, bool suppressAfterEvents, string filename, 
                //bool bPreserveItemUIVersion)
                try
                {
                    var method = typeof(SPListItem).GetMethod("UpdateInternal", BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.Public
                        , null, new Type[]{typeof(bool),typeof(bool),typeof(Guid),typeof(bool),typeof(bool),typeof(bool),
                            typeof(bool), typeof(bool),typeof(bool), typeof(string), typeof(bool), typeof(bool)}, null);

                    //Verify if the method UpdateInternal has changed. if changed, return false. Error Log
                    if (method == null)
                    {
                        logger.Log(AveLogLevel.ERROR, "Internal update method has changed.");
                        return false;
                    }

                    method.Invoke(item, new object[] {bSystem, bPrseverItemVersion, Guid.Empty, true,
                            bPublish , bNoVersion, false, false, false, null, bPrseverItemVersion, false});
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.WARN, "Failed to internal update item. Error: {0}. Internal exception: {1}", ex, ex.InnerException == null ? "" : ex.InnerException.ToString());
                    return false;
                }
                return true;

            }
        }

        internal void AveItemNativeSystemUpdate(int authorId, int editorId, DateTime modifiedTime, DateTime createdTime)
        {
            mSite.QueryService.UpdateSpecialPropertyByNative(editorId.ToString(), authorId.ToString(), modifiedTime, createdTime, this.info);
        }

        private bool EnableVersioning(SPList list)
        {
            if (list == null)
            {
                return false;
            }
            return (list.EnableVersioning || list.EnableMinorVersions);
        }

        // 递归调用了，应该是没有逻辑走到。但是因为调用源是IAveSPItem定义的接口，所以不确定外围是否有使用，为了不影响build，暂时不删除这个方式。
        [Obsolete("no use now, will remove later")]
        public IAveFile LoadCheckOutFile(IAveWeb web, Guid fileId, IAveUser user)
        {
            //IAveFile tempFile = LoadCheckOutFile(web, fileId, user);
            //return tempFile;
            throw new NotImplementedException();
        }

        [Obsolete("Not use any more, will remove it later")]
        public SPFile LoadCheckOutFileByNative(SPWeb web, string folderServerRelativeUrl, string fileName)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.LoadCheckOutFileByNative"))
            {

                int currentUserID = mWeb.Web.CurrentUser.ID;
                mSite.QueryService.ChangeCheckoutUserID(info, info.CheckOutFileUniqueID, currentUserID);
                SPFile file = null;
                bool exist = false;
                if (folderServerRelativeUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    file = web.GetFile(SPResourcePath.FromDecodedUrl(folderServerRelativeUrl + fileName));
                    exist = file.Exists;
                }
                else
                {
                    file = web.GetFile(SPResourcePath.FromDecodedUrl(folderServerRelativeUrl + "/" + fileName));
                    exist = file.Exists;
                }
                if (info.CheckoutUserId > 0)
                {
                    mSite.QueryService.ChangeCheckoutUserID(info, info.CheckOutFileUniqueID, info.CheckoutUserId);
                }
                return file;

            }

        }

        #region ADO-132466.使用Site.GetCheckoutWeb其中传web参数的方法来获取的CheckoutWeb，当user对web有权限，但是list没有权限的时候会导致Web.GetFile获取不到相应的file。由于这个方法没有人调用，所以不修改，直接注释掉。
        //        public SPFile LoadCheckOutFile(SPWeb web, Guid fileId, SPUser user)
        //        {
        //
        //            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.LoadCheckOutFile"))
        //            {
        //
        //                SPUserToken userToken = user.UserToken;
        //                SPWeb curWeb = mSite.GetCheckoutWeb(mSite.ID, web, user, fileId);
        //                return curWeb.GetFile(fileId);
        //
        //            }
        //
        //        }
        #endregion

        internal SPListItem LoadCheckOutListItem(SPList list, Guid fileId, SPUser user)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.LoadCheckOutFile_1"))
            {

                //SPUser user = web.SiteUsers.GetByID(checkOutUserId);
                Guid webId = list.ParentWeb.ID;
                Guid siteId = list.ParentWeb.Site.ID;
                SPListItem item = null;
                SPWeb curWeb = mSite.GetCheckoutWeb(siteId, list.ParentWeb, list, user, fileId, false);
                list = curWeb.Lists[list.ID];
                item = list.GetItemByUniqueId(fileId);
                return item;

            }

        }

        public SPFile LoadCheckOutFile(SPWeb spWeb, SPFolder folder, string fileName)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.LoadCheckOutFile_2"))
            {

                var fileServerRelativeUrl = string.Format("{0}/{1}", folder.ServerRelativeUrl.TrimEnd('/'), fileName);
                var file = spWeb.GetFile(SPResourcePath.FromDecodedUrl(fileServerRelativeUrl));
                var checkoutFile = LoadCheckoutFile(spWeb, file, true, fileServerRelativeUrl);
                if (checkoutFile != null)
                {
                    return checkoutFile;
                }
                return file;

            }

        }

        //Delete middle versions
        [SuppressMessage("FxCopCustomRules", "C100003:DoNotUseSpecificSPMethod", Justification = "There is no SPFileVersionCollection class")]
        internal void DeleteMiddleVersions(List<int> versionLabels)
        {
            for (int i = 0; i < versionLabels.Count - 1; i++)
            {
                try
                {
                    SPListItemVersion version = mSPListItem.Versions.GetVersionFromID(versionLabels[i]);
                    if (version != null && version.IsCurrentVersion == false)
                    {
                        version.Delete();
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("An error occurred while deleting temporary version '{0}' for an item: {1}, details: {2}", versionLabels[i], info.Name, e.ToString());
                }
            }
        }

        //ListItem使用API方式更新ModerationStatus，如果出错，则使用Native更新。
        internal void UpdateListItemModerationStatus(AveListItemInfo info)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.UpdateListItemModerationStatus"))
            {

                try
                {
                    if (info == null || ListItem == null || mList == null)
                    {
                        return;
                    }
                    if (!mList.EnableModeration && (info.ModerationStatus == 0 && info.OriginalVersion % 512 == 0 || info.ModerationStatus == 3 && info.OriginalVersion % 512 != 0))
                    {
                        return;
                    }
                    else
                    {
                        if (!mList.EnableModeration)
                        {
                            mList.EnableModeration = true;
                            mList.Update();
                            info.SettingInfo.LIST_SETTING_CHANGED = true;
                        }
                    }

                    //当Agent Account没有权限的时候，Moderation信息会和4个特殊的column一起更新
                    if (!this.HasFullControlPermission)
                    {
                        return;
                    }

                    SPModerationStatusType moderationType = (SPModerationStatusType)info.ModerationStatus;
                    if (mSPListItem.ModerationInformation.Status != moderationType)
                    {
                        try
                        {
                            mSPListItem.ModerationInformation.Status = moderationType;
                            mSPListItem.ModerationInformation.Comment = info.ModerationComments;
                            mSPListItem.SystemUpdate(false);
                            info.Level = (byte)mSPListItem.Level;
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.DEBUG, ServerAPIResource.UpdateItemError, e);
                            mList.Reload();
                            mSPListItem = mList.List.GetItemById(mSPListItem.ID);
                            //同步重新赋值
                            ListItem = new AveListItem(mList, mSPListItem);
                            info.Level = (byte)mSPListItem.Level;
                            info.NeedUpdateStatusByNative = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.WARN, "Failed to restore item moderation status info. List title: {0}, Item RowId: {1}. Error: {2}", mList.Title, mSPListItem.ID, ex.ToString());
                }

            }

        }

        private SPFile CreateNewVersion(IAveRestoreStream receiver, SPFolder folder, SPFile file, string fileName, bool addNewFile, int uiVersion, bool isCheckOut, string checkInComment, bool isGhostPage, string setupPath, List<SPListItem> holdItems, Hashtable HTMetaInfo, AveDocumentInfo docInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.CreateNewVersion"))
            {

                AveItemVersionNumber vn = new AveItemVersionNumber(uiVersion);
                bool isMajorVersion = vn.UIVersion % 512 == 0;
                List<int> versionLabels = new List<int>();
                bool isFirstTime = true;
                //BackupParentListSetting();

                if (!string.IsNullOrEmpty(setupPath))
                {
                    this.setupPath = setupPath;
                }

                if (addNewFile)
                {
                    if (isGhostPage)
                    {
                        if (isCheckOut && (uiVersion == 1 || uiVersion == 512))
                        {
                            file = AddGhostedPage(fileName, setupPath, receiver, true, docInfo.GhostPageOption, docInfo);
                            //当Agent Account没有Full Control的时候，暂时先对只有一个Checkout Version的GhostPage不做处理
                            if (!this.hasFullControlPermission)
                            {
                                info.CheckoutUserId = -1;
                            }
                        }
                        else
                        {
                            file = AddGhostedPage(fileName, setupPath, receiver, false, docInfo.GhostPageOption, docInfo);
                        }
                    }
                    else
                    {
                        if (isCheckOut && (uiVersion == 1 || uiVersion == 512))
                        {
                            //ADO-108825 保持目的端删除文件itemId和UniqueId的方法无法创建出一个checkout的文件，故对需要创建出checkout文件的情况不keep目的端的itemId和UniqueId。
                            if ((info.DestItemRowId > 0) || (info.DestItemUniqueId != Guid.Empty))
                            {
                                info.DestItemRowId = 0;
                                info.DestItemUniqueId = Guid.Empty;
                            }
                            file = AddFileWithStream(receiver, folder, fileName, isMajorVersion, true, holdItems, HTMetaInfo, docInfo);
                        }
                        else
                        {
                            file = AddFileWithStream(receiver, folder, fileName, isMajorVersion, false, holdItems, HTMetaInfo, docInfo);
                            //处理第一个version 有checkin comment的情况，使用API更新checkin comment
                            if (!string.IsNullOrEmpty(checkInComment)
                                && (uiVersion == 1 || (uiVersion == 512 && mSPList == null ? true : !mSPList.EnableMinorVersions)))
                            {
                                file.CheckOut();
                                file.CheckIn(checkInComment, SPCheckinType.OverwriteCheckIn);
                            }
                        }
                    }
                    versionLabels.Add(file.UIVersion);
                    isFirstTime = false;
                }

                #region Increase Version for Large Version
                if (info.MaxVersionDiff > 0 && uiVersion - file.UIVersion >= 512 * info.MaxVersionDiff)
                {
                    if (!this.HasFullControlPermission)
                    {
                        logger.Log(AveLogLevel.WARN, "Agent account does not have enough permission to increase large version");
                    }
                    else
                    {
                        if (file.Level == SPFileLevel.Checkout)
                        {
                            file.CheckIn("", SPCheckinType.MajorCheckIn);
                        }
                        else
                        {
                            file.CheckOut();
                            file.CheckIn("", SPCheckinType.MajorCheckIn);
                        }
                        InitBySPFile(file);
                        int tempVersion = uiVersion - 512;
                        Guid parentFolderId = Guid.Empty;
                        if (mParentFolder != null)
                        {
                            parentFolderId = mParentFolder.UniqueId;
                        }
                        mSite.QueryService.IncreaseVersionByNative(uiVersion, mSite.ID, file.UniqueId, file.UIVersion, file.Item.ID, parentFolderId);
                        ReloadFile();
                        file = mFile.File;
                        versionLabels.Add(tempVersion);
                    }
                }
                #endregion

                //increase version to uiversion
                int currentMajor = file.MajorVersion;
                int currnetMinor = file.MinorVersion;


                while (currentMajor < vn.MajorVersion)
                {
                    file = IncreaseVersion(receiver, true, folder, file, isCheckOut, checkInComment, isFirstTime, holdItems, HTMetaInfo, docInfo);
                    currentMajor++;
                    versionLabels.Add(file.UIVersion);
                    isFirstTime = false;
                }

                currnetMinor = file.MinorVersion;
                while (vn.MinorVersion > currnetMinor)
                {
                    file = IncreaseVersion(receiver, false, folder, file, isCheckOut, checkInComment, isFirstTime, holdItems, HTMetaInfo, docInfo);
                    currnetMinor++;
                    versionLabels.Add(file.UIVersion);
                    isFirstTime = false;
                }

                //delete middle versions
                int deleteVersionCount = versionLabels.Count - 1;
                if (isCheckOut && deleteVersionCount > 0)
                {//如果当前Version是Checkout Version，它前一个Version必须保留，SP中也是这么限制的。
                    deleteVersionCount = versionLabels.Count - 2;
                }
                using (AvePerformanceScope scope = new AvePerformanceScope("SP.AveItem.CreateNewVersion.DeleteByID"))
                {
                    try
                    {
                        for (int i = 0; i < deleteVersionCount; i++)
                        {
                            //mSite.QueryService.DeleteVersionByNative(info, file.UniqueId, versionLabels[i]);
                            file.Versions.DeleteByID(versionLabels[i]);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Info("Delete version failed. Error: {0}", e);
                    }
                }

                return file;

            }

        }

        private SPFile AddFileWithStream(IAveRestoreStream receiver, SPFolder folder, string fileName, bool isMajorVersion, bool needForceCheckout, List<SPListItem> holdItems, Hashtable HTMetaInfo)
        {
            return AddFileWithStream(receiver, folder, fileName, isMajorVersion, needForceCheckout, holdItems, HTMetaInfo, null);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint setup path.")]
        private SPFile AddFileWithStream(IAveRestoreStream receiver, SPFolder folder, string fileName, bool isMajorVersion, bool needForceCheckout, List<SPListItem> holdItems, Hashtable HTMetaInfo, AveDocumentInfo docInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.AddFileWithStream"))
            {

                SPListTemplateType listType = new SPListTemplateType();
                if (mSPList != null)
                {
                    listType = mSPList.BaseTemplate;
                }
                SPFile file = null;

                UpdateListSettings(mSPList, info, isMajorVersion, needForceCheckout);
                if (info.SettingInfo.LIST_SETTING_CHANGED)
                {
                    if (folder.ParentFolder.ParentListId == Guid.Empty)
                    {//Reload Root Folder, 否则List的Update对RootFolder不起作用
                        folder = mSPList.RootFolder;
                    }
                }

                if (!this.hasFullControlPermission && needForceCheckout)
                {
                    //这个方法和下面AddFile的内容几乎一样，修改时候需要注意，以后需要重构
                    file = AddRealCheckoutFile(listType, folder, fileName, receiver, HTMetaInfo, docInfo);
                    info.CheckoutUserId = -1;
                }
                else
                {
                    if (listType == SPListTemplateType.WebPageLibrary && (!info.HasStream || (!string.IsNullOrEmpty(this.setupPath) && this.setupPath.Equals(@"DocumentTemplates\wkpstd.aspx", StringComparison.OrdinalIgnoreCase))))
                    {
                        file = SPUtility.CreateNewWikiPage(mSPList, folder.Url + "/" + fileName);
                        info.IsNewCreated = true;
                    }
                    else if (listType == SPListTemplateType.MasterPageCatalog && !info.HasStream)
                    {
                        file = CreateMasterPage(folder, fileName);
                    }
                    else if (info.HasStream)
                    {
                        file = AddFileWithStream(receiver, folder, fileName, HTMetaInfo, docInfo);
                    }
                    else
                    {
                        using (AvePerformanceScope scope = new AvePerformanceScope("SP.AveItem.AddFileWiteStream.Add->Empty"))
                        {
                            file = folder.Files.Add(SPResourcePath.FromDecodedUrl(fileName), new byte[] { }, new SPFileCollectionAddParameters { Overwrite = true });
                        }
                        info.IsNewCreated = true;
                    }
                }

                try
                {
                    if (holdItems.Count > 0)
                    {
                        UnLockItem(holdItems, file.Item);
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("After add file, unlock item: {0} error: {1}", fileName, e.ToString());
                }
                //exist new file with checkout level, but needForceCheckout is false
                if (!needForceCheckout && file.Level == SPFileLevel.Checkout)
                {
                    RestoreWebPart(info as AveDocumentInfo, new AveFile(mWeb, file));
                    file.CheckIn("");
                }


                return file;

            }

        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint setup path.")]
        private SPFile AddRealCheckoutFile(SPListTemplateType listType, SPFolder folder, string fileName, IAveRestoreStream receiver, Hashtable HTMetaInfo, AveDocumentInfo docInfo)
        {
            SPList list = null;
            SPFolder checkoutFolder = null;
            if (folder.ParentWeb.CurrentUser != null && folder.ParentWeb.CurrentUser.ID != info.CheckoutUserId)
            {
                SPUser user = folder.ParentWeb.SiteUsers.GetByID(info.CheckoutUserId);
                SPWeb checkoutWeb = mSite.GetCheckoutWeb(folder.ParentWeb, mSPList, ref user, Guid.Empty);
                list = checkoutWeb.Lists[mSPList.ID];
                checkoutFolder = checkoutWeb.GetFolder(SPResourcePath.FromDecodedUrl(folder.ServerRelativeUrl));
            }
            else
            {
                list = mSPList;
                checkoutFolder = folder;
            }

            SPFile file = null;
            if (listType == SPListTemplateType.WebPageLibrary && (!info.HasStream || (!string.IsNullOrEmpty(this.setupPath) && this.setupPath.Equals(@"DocumentTemplates\wkpstd.aspx", StringComparison.OrdinalIgnoreCase))))
            {
                file = SPUtility.CreateNewWikiPage(list, folder.Url + "/" + fileName);
                info.IsNewCreated = true;
            }
            else if (listType == SPListTemplateType.MasterPageCatalog && !info.HasStream)
            {
                file = CreateMasterPage(checkoutFolder, fileName);
            }
            else if (info.HasStream)
            {
                file = AddFileWithStream(receiver, checkoutFolder, fileName, HTMetaInfo, docInfo);
            }
            else
            {
                using (AvePerformanceScope scope = new AvePerformanceScope("SP.AveItem.AddRealCheckoutFile.Add->Empty"))
                {
                    file = checkoutFolder.Files.Add(SPResourcePath.FromDecodedUrl(fileName), new byte[] { },new SPFileCollectionAddParameters { Overwrite = true });
                }
                info.IsNewCreated = true;
            }
            return file;
        }

        private void UpdateListSettings(SPList list, AveBaseItemInfo itemInfo, bool isMajorVersion, bool needForceCheckout)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.UpdateListSettings"))
            {

                if (list == null || list.BaseTemplate == SPListTemplateType.Survey ||
                    list.BaseTemplate == SPListTemplateType.ExternalList)
                {
                    return;
                }
                bool needUpdate = false;

                if (needForceCheckout && !list.ForceCheckout)
                {
                    list.ForceCheckout = true;
                    needUpdate = true;
                }
                //approved的major version和draft的minor version不需要开启EnableModeration即可创建出来。
                if ((itemInfo.ModerationStatus != 0 && isMajorVersion || itemInfo.ModerationStatus != 3 && !isMajorVersion) && !list.EnableModeration)
                {
                    list.EnableModeration = true;
                    needUpdate = true;
                }
                if (!isMajorVersion)
                {
                    //add version smaller than 1.0,should switch on MinorVersion.
                    if (!list.EnableMinorVersions)
                    {
                        list.EnableVersioning = true;
                        list.EnableMinorVersions = true;
                        needUpdate = true;
                    }
                }
                else
                {
                    if (!list.EnableVersioning && info.OriginalVersion != 512)
                    {
                        list.EnableVersioning = true;
                        needUpdate = true;
                    }
                    if (needForceCheckout ||
                        (info is AveDocumentInfo && (info as AveDocumentInfo).IsOrignialCheckOut)) //ADO-155235 在还原CheckOut的大version的时候，需要把小version关闭，不然CheckOut之后增加的是小version。
                    {
                        if (list.EnableMinorVersions)
                        {
                            list.EnableMinorVersions = false;
                            needUpdate = true;
                        }
                    }
                }

                if (needUpdate)
                {
                    info.SettingInfo.LIST_SETTING_CHANGED = true;
                    list.Update();
                }

            }

        }

        private SPFile CreateMasterPage(SPFolder folder, string fileName)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.CreateMasterPage"))
            {

                SPFile file = null;
                try
                {
                    SPFile file2 = folder.ParentWeb.GetFile("_catalogs/masterpage/PageLayoutTemplate.aspx");

                    if (!file2.Exists)
                    {
                        string setupPath = @"Features\PublishingResources\PageLayoutTemplate.aspx";
                        string path = folder.ServerRelativeUrl + "/PageLayoutTemplate.aspx";
                        object[] paramObjs = new object[] { setupPath, path, true };
                        file2 = AveAssemblyUtility.InvokeMethod(folder.Files, "AddGhosted", paramObjs) as SPFile;
                        info.IsNewCreated = true;
                    }
                    string strNewUrl = folder.Url + "/" + fileName;
                    if (!strNewUrl.Equals("_catalogs/masterpage/PageLayoutTemplate.aspx", StringComparison.OrdinalIgnoreCase))
                    {
                        file2.CopyTo(strNewUrl, false);
                        info.IsNewCreated = true;
                    }
                    file = folder.ParentWeb.GetFile(SPResourcePath.FromDecodedUrl(strNewUrl));
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.MasterPageCreateFailed, fileName, e);
                }
                return file;

            }

        }

        private SPFile AddFileWithStream(IAveRestoreStream receiver, SPFolder folder, string fileName, Hashtable HTMetaInfo)
        {
            return AddFileWithStream(receiver, folder, fileName, HTMetaInfo, null);
        }

        private SPFile AddFileWithStream(IAveRestoreStream receiver, SPFolder folder, string fileName, Hashtable HTMetaInfo, AveDocumentInfo docInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.AddFileWithStream_1"))
            {

                Stream stream = null;
                SPFile file = null;
                if (info.HasStream)
                {
                    stream = new AveSPFileStream(receiver);
                }
                //SP2013中所有对File Content的操作都必须在Add 之前完成
                stream = ReplaceStreamBeforeAdd(stream, folder, null, fileName, info.IsCurrentVersion);
                if (docInfo != null && docInfo.IsLinkFile && this.mList != null && this.mList.IsConnectorList.HasValue && mList.IsConnectorList.Value)
                {
                    var id = mList.SOIntegrationUtil.RestoreLinkFile(fileName, folder.UniqueId, Guid.Empty, stream, true);
                    file = (mWeb.GetFile(id) as AveFile).File;
                }
                else
                {
                    if (docInfo != null && docInfo.HasStream)
                    {
                        docInfo.ServerRelativeUrl = folder.ServerRelativeUrl.TrimEnd('/') + "/" + fileName;
                        AveSPDocContentReplacer replacer = new AveSPDocContentReplacer(mSite, stream, docInfo);
                        stream = replacer.ReplaceWebPartContent();
                    }
                    if (info.IsStubData)
                    {
                        file = this.mList.SOIntegrationUtil.AddStubFileWithStream(folder, fileName, stream);
                        long bsn = mList.SOIntegrationUtil.QueryService.GetMaxRbs(file.Web.Site.ID, file.UniqueId);
                        if (bsn == -1)
                        {
                            throw new Exception("There is not a BSN");
                        }
                        this.mList.SOIntegrationUtil.UpdateStubSize((int)file.Level, file.ParentFolder.UniqueId, file.UniqueId, file.Web.Site.ID, (int)mStorageInfo.Size, bsn);
                        RestoreStubDBInfo();
                        //RestoreConnectorStub(info.GUID, info.OriginalVersion, 1);
                    }
                    else if (info.DestItemRowId > 0 && info.DestItemUniqueId != Guid.Empty)
                    {
                        file = CreateOrUpdateFileAndItem(stream, fileName);
                    }
                    else
                    {
                        //通过在Hashtable中添加column value来达到新创建文件keep tp_GUID的逻辑
                        if (!this.HasFullControlPermission && info.UserData.ContainsKey("#tp_GUID"))
                        {
                            HTMetaInfo["GUID"] = info.UserData["#tp_GUID"].ToString();
                            info.UserData.Remove("#tp_GUID");
                        }
                        if (HTMetaInfo.Count != 0)
                        {
                            using (AvePerformanceScope scope = new AvePerformanceScope("SP.AveItem.AddFileWithStream_1.Add->Hashtable"))
                            {
                                file = folder.Files.AddExtension(fileName, stream, HTMetaInfo, true);
                            }
                        }
                        else
                        {
                            using (AvePerformanceScope scope = new AvePerformanceScope("SP.AveItem.AddFileWithStream_1.Add"))
                            {
                                file = folder.Files.AddExtension(fileName, stream, true);
                            }
                        }
                    }
                }
                if (stream != null)
                {
                    stream.Dispose();
                }

                info.IsNewCreated = true;
                return file;

            }

        }

        /// <summary>
        /// 请慎重调用这个方法，这个方法能够添加两个RowId相同的File，导致界面上，两个文件的Column Value等关联关系丢失
        /// 当使用该方法添加两个UniqueId相同的File的时候，添加第二个是抛出Save Conflict错误
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private SPFile CreateOrUpdateFileAndItem(Stream stream, string fileName)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.CreateOrUpdateFileAndItem"))
            {

                object request = AveAssemblyUtility.GetPropertyValue(mWeb.Web, "Request");
                string webUrl = mWeb.Url;
                string webRelativeUrl = (mParentFolder.Url.TrimEnd('/') + "/" + fileName).TrimStart('/');
                SPFileStream fileStream = new SPFileStream(mWeb.Web, 1024);
                byte[] buffer = new byte[102400];
                int count = stream.Read(buffer, 0, buffer.Length);
                while (count > 0)
                {
                    fileStream.Write(buffer, 0, count);
                    count = stream.Read(buffer, 0, buffer.Length);
                }
                object lockBytes = AveAssemblyUtility.GetPropertyValue(fileStream, "LockBytes");
                int length = (int)fileStream.Length;
                UInt32 num = 0;
                string str3 = "";
                string listId = mParentFolder.ParentListId.ToString("B").ToUpper(System.Globalization.CultureInfo.InvariantCulture);
                int rowId = info.DestItemRowId;
                Guid uniqueId = info.DestItemUniqueId;
                object[] pars = new object[] { webUrl, webRelativeUrl, uniqueId, lockBytes, length, 0, null, null, 0, 0, 1, (object)0, (object)0, null, "", rowId, listId, 0, num, str3 };
                AveAssemblyUtility.InvokeMethod(request, "CreateOrUpdateFileAndItem", pars);
                SPFile file = mWeb.Web.GetFile(SPResourcePath.FromDecodedUrl(webRelativeUrl));
                fileStream.Close();
                return file;

            }

        }

        [Obsolete("Use AddGhostedPage(string name, string setupPath, IAveRestoreStream receiver, bool needForceCheckout, int ghostPageOption) instead")]
        [SuppressMessage("FxCopCustomRules", "C100003: DoNotUseSpecificSPMethod", Justification = "do not check exception handle")]
        public SPFile AddGhostedPage(string name, string setupPath, bool needForceCheckout)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.AddGhostedPage"))
            {

                //restore the file that has no stream, like web part page, wiki page...
                //The HasStream value is 0, and the SetupPath is not NULL.
                //Using the internal method 'AddGhosted' of SPFileCollection to add these files.
                string path = string.Format("{0}/{1}", mParentFolder.ServerRelativeUrl.TrimEnd('/'), name);
                if (string.IsNullOrEmpty(setupPath))
                {
                    throw new AveWarningException(AveInternalResourceKey.Wrapper_Exception_Server_NotFindSetupPathForGhostedPage, string.Empty, path);
                }
                try
                {
                    SPFile file = null;
                    try
                    {
                        file = mWeb.Web.GetFile(SPResourcePath.FromDecodedUrl(path));
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetFileFaild, e);
                    }
                    finally
                    {
                        if (file != null && !file.Exists)
                        {
                            file = CreateAGhostedPage(name, setupPath, needForceCheckout, path);
                        }
                    }
                    InitBySPFile(file);
                    return file;
                }
                catch (Exception e)
                {
                    throw new AveWarningException(e, AveInternalResourceKey.Wrapper_Exception_Server_NotFindSetupPathForGhostedPage, setupPath, path);
                }

            }

        }

        [SuppressMessage("FxCopCustomRules", "C100003: DoNotUseSpecificSPMethod", Justification = "do not check exception handle")]
        public SPFile AddGhostedPage(string name, string setupPath, IAveRestoreStream receiver, bool needForceCheckout, int ghostPageOption, AveDocumentInfo docInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.AddGhostedPage"))
            {

                //restore the file that has no stream, like web part page, wiki page...
                //The HasStream value is 0, and the SetupPath is not NULL.
                //Using the internal method 'AddGhosted' of SPFileCollection to add these files.
                string path = string.Format("{0}/{1}", mParentFolder.ServerRelativeUrl.TrimEnd('/'), name);
                if (string.IsNullOrEmpty(setupPath))
                {
                    throw new AveWarningException(AveInternalResourceKey.Wrapper_Exception_Server_NotFindSetupPathForGhostedPage, string.Empty, path);
                }
                try
                {
                    SPFile file = null;
                    try
                    {
                        file = mWeb.Web.GetFile(SPResourcePath.FromDecodedUrl(path));
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetFileFaild, e);
                    }
                    finally
                    {
                        if (file != null && !file.Exists)
                        {
                            file = CreateAGhostedPage(name, setupPath, needForceCheckout, path);
                            info.IsNewCreated = true;
                        }
                    }
                    if (docInfo.IsNewCreated || docInfo.IsOverWrite)
                    {
                        SetGhostedPageContent(file, receiver, ghostPageOption, docInfo); //For Restore UnGostedPage Content.
                        InitBySPFile(file);
                        return file;
                    }
                    else
                    {
                        docInfo.RestoringItem.NeedSkipped = true;
                        throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                    }
                }
                catch (AveRestoreException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveWarningException(e, AveInternalResourceKey.Wrapper_Exception_Server_NotFindSetupPathForGhostedPage, setupPath, path);
                }

            }

        }

        private void SetGhostedPageContent(SPFile file, IAveRestoreStream receiver, int ghostPageOption)
        {
            SetGhostedPageContent(file, receiver, ghostPageOption, null);
        }

        private void SetGhostedPageContent(SPFile file, IAveRestoreStream receiver, int ghostPageOption, AveDocumentInfo docInfo)
        {
            if (info.HasStream && file.UIVersion == info.OriginalVersion
            && ghostPageOption == 3/* && AveRestoreGhostPageOption.KeepStreamAndPath*/)
            {
                if (receiver == null)
                {
                    return;
                }
                Stream stream = new AveSPFileStream(receiver);
                if (docInfo != null && docInfo.HasStream && (docInfo.Name.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase)
                            || docInfo.Name.EndsWith(".master", StringComparison.OrdinalIgnoreCase)))
                {
                    //因为还没有调用InitBySPFile方法，docInfo.ServerRelativeUrl属性还没有被初始化，但是在ReplaceWebPartContent()方法中有可能使用
                    docInfo.ServerRelativeUrl = file.ServerRelativeUrl;
                    AveSPDocContentReplacer replacer = new AveSPDocContentReplacer(mSite, stream, docInfo);
                    stream = replacer.ReplaceWebPartContent();
                }
                if (mSPList != null)
                {
                    SPList list = mSPList;
                    bool updateModeration = false;
                    if (file.Level == SPFileLevel.Published)
                    {
                        if (list.EnableMinorVersions || list.EnableModeration)
                        {
                            using (AvePerformanceScope scope = new AvePerformanceScope("SP.AveItem.SetGhostedPageContent.EnableModeration->false"))
                            {
                                updateModeration = list.EnableModeration;
                                list.EnableMinorVersions = false;
                                list.EnableModeration = false;
                                info.SettingInfo.LIST_SETTING_CHANGED = true;
                                list.Update();
                                //ado-60027, 由于62 release时间较为紧急，暂时用这个方式来解决问题，需要在63里找下root cause，换个方法来解决
                                //mWeb.ReloadWeb();
                                //file= mWeb.Web.GetFile(file.Url);
                            }
                        }
                    }
                    file.CheckOut();
                    try
                    {
                        file.SaveBinaryExtension(stream);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Update ghost page content error {0}", ex.Message);
                    }
                    file.CheckIn("", SPCheckinType.OverwriteCheckIn);
                    if (updateModeration)
                    {
                        using (AvePerformanceScope scope = new AvePerformanceScope("SP.AveItem.SetGhostedPageContent.EnableModeration->true"))
                        {
                            list.EnableModeration = true;
                            list.Update();
                        }
                    }
                }
                else
                {
                    file.SaveBinaryExtension(stream);
                }
            }
            else if (file.CustomizedPageStatus == SPCustomizedPageStatus.Customized
                && ghostPageOption == 2/* && AveRestoreGhostPageOption.KeepPathOnly*/)
            {
                using (AvePerformanceScope scope = new AvePerformanceScope("SP.AveItem.SetGhostedPageContent.RevertContentStream"))
                {
                    file.RevertContentStream();
                }
            }
        }

        private SPFile CreateAGhostedPage(string name, string setupPath, bool needForceCheckout, string path)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.CreateAGhostedPage"))
            {

                SPFile file = null;
                if (mList != null)
                {
                    UpdateListSettings(mSPList, info, info.OriginalVersion % 512 == 0, needForceCheckout);
                    try
                    {
                        file = (SPFile)AveAssemblyUtility.InvokeMethod(
                         mParentFolder.Files, typeof(SPFileCollection), "AddGhosted", new object[] { setupPath, path, false });
                    }
                    catch (Exception ex) //we can not find the template in 15 hive, so we will try it in 14 hive. Maybe dirty data will be created.
                    {
                        logger.Info(ex.Message);
                        IAveCommonRequest request = mWeb.Request;
                        object[] args = new object[] { setupPath, (byte)14, 0, mList.ID, Guid.NewGuid(), 0, 0, null, path, mWeb.GetWebRelativeUrlFromUrl(path), true, false, true, null };
                        Type[] paramTypes = new Type[] { args[0].GetType(), args[1].GetType(), args[2].GetType(), Type.GetType("System.Guid&"), Type.GetType("System.Guid&"), args[5].GetType(), args[6].GetType(), typeof(string), args[8].GetType(), args[9].GetType(), args[10].GetType(), args[11].GetType(), args[12].GetType(), Type.GetType("System.Object") };
                        request.SetGhostedFile(args, paramTypes);
                        Guid docId = (Guid)args[4];
                        file = mWeb.Web.GetFile(docId);

                    }

                    //需要重新获取file，否则在之后使用Checkout再Checkin时抛出异常。
                    file = mWeb.Web.GetFile(file.UniqueId);
                    if (!needForceCheckout && file.Level == SPFileLevel.Checkout)
                    {
                        RestoreWebPart(info as AveDocumentInfo, new AveFile(mWeb, file));
                        file.CheckIn("");
                    }
                }
                else
                {
                    IAveCommonRequest request = mWeb.Request;
                    object[] args = new object[] { setupPath, (byte)mWeb.Site.CompatibilityLevel, 0, Guid.Empty, Guid.NewGuid(), 0, 0, string.Empty, path, name, false, false, true, new object() ,0};
                    Type[] paramTypes = new Type[] { args[0].GetType(), args[1].GetType(), args[2].GetType(), Type.GetType("System.Guid&"), Type.GetType("System.Guid&"), args[5].GetType(), args[6].GetType(), args[7].GetType(), args[8].GetType(), args[9].GetType(), args[10].GetType(), args[11].GetType(), args[12].GetType(), args[13].GetType(), args[14].GetType() };
                    request.SetGhostedFile(args, paramTypes);
                    Guid docId = (Guid)args[4];
                    file = mWeb.Web.GetFile(docId);
                }
                return file;

            }

        }

        #region Init Self

        public void InitBySPFile(SPFile spFile)
        {
            InitBySPFile(spFile, true);
        }

        public void InitBySPFile(SPFile spFile, bool needReloadFile)
        {
            mSPFile = spFile;
            mFile = new AveFile(mWeb, spFile);
            info.Name = spFile.Name;
            info.GUID = spFile.UniqueId;
            info.Version = spFile.UIVersion;
            info.Level = (byte)spFile.Level;
            info.ServerRelativeUrl = spFile.ServerRelativeUrl;
            info.WebId = mWeb.ID;
            if (!(info.GUID == Guid.Empty && string.Compare(info.Name, AveConstants.SYSTEM_FOLDER, StringComparison.OrdinalIgnoreCase) == 0)) //IsSystemList
            {
                try
                {
                    if (mList != null && mList.List != null)
                    {
                        if (spFile.Item != null && spFile.UniqueId != spFile.Item.UniqueId)
                        {//在日语环境下，文件名如果只有半角和全角的区别时，file.Item会取错item
                            mSPListItem = mSPList.GetItemByUniqueId(spFile.UniqueId);
                        }
                        else
                        {
                            mSPListItem = spFile.Item;
                        }
                        if (mSPListItem != null)
                        {
                            info.RowId = mSPListItem.ID;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetItemFromFileError, ex.ToString());
                }
                if (needReloadFile)
                {
                    if (mList != null)
                    {
                        try
                        {
                            if (info.RowId > 0)
                            {
                                mSPListItem = mList.List.GetItemById(info.RowId);

                                SPFile file = mSPListItem.File;
                                if (file.CheckOutType != Microsoft.SharePoint.SPFile.SPCheckOutType.None && file.Level != SPFileLevel.Checkout)
                                {
                                    mSPListItem = LoadCheckOutListItem(mList.List, file.UniqueId, file.CheckedOutByUser);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Warn("An error occurred while getting ListItem from InitBySPFile. file name: {0}, error: {1}", info.Name, e.ToString());
                        }
                    }
                }
            }
            //info.InternalVersion = mSite.QueryService.GetInternalVersion(info, false, spFile.UniqueId, info.Version);
        }

        public void InitBySPListItem(SPListItem spListItem)
        {
            int version = mSite.QueryService.GetCurrentUIVersion(info.SiteId, info.ParentId, spListItem.UniqueId);
            InitBySPListItem(spListItem, version);
        }

        private void InitBySPListItem(SPListItem spListItem, int version)
        {
            mSPListItem = spListItem;
            mListItem = new AveListItem(this.mList, mSPListItem);
            if (spListItem != null)
            {
                try
                {
                    //Folder使用Name而不是Title, 无论Lib还是list。
                    if (spListItem.Folder != null && spListItem.Folder.Exists)
                    {
                        info.Name = spListItem.Folder.Name;
                    }
                    else
                    {
                        info.Name = spListItem.Title;
                    }
                }
                catch (ArgumentException e)
                {
                    logger.Debug("Error in Init SPListItem.{0}", e);
                    info.Name = spListItem.Name; // add this when title is null
                }
            }
            mSPFile = mSPListItem.File;
            using (var scope = new AvePerformanceScope("Object.Server.AveItem.InitBySPListItem.GetUniqueId"))
            {
                info.GUID = spListItem.UniqueId;
            }
            info.RowId = spListItem.ID;
            info.Level = (byte)spListItem.Level;
            //mScopeId = spListItem.RoleAssignments.ID;
            info.ServerRelativeUrl = spListItem.Web.ServerRelativeUrl.TrimEnd('/') + "/" + spListItem.Url;
            info.ScopeUrl = info.ServerRelativeUrl.Substring(1);
            info.Version = version;
            if (info.Version == 0)
            {
                throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Server_NoVersionId, info.Name);
            }
        }

        /// <summary>
        /// 当新添加ListItem的时候，需要更新之前缓存的Item Name，如果Keep不住源端RowId，LeafName会变化
        /// </summary>
        /// <param name="newLeafName"></param>
        internal void RefreshCacheName(int rowId)
        {
            info.RestoringItem.ReSetItemName(rowId.ToString() + "_.000");
            //判断冲突的时候会使用到mList.MaxListItemRowId，因此也需要更新
            if (rowId > mList.MaxListItemRowId)
            {
                mList.MaxListItemRowId = rowId;
            }
        }

        public void ReInitBySPListItemLevel(SPListItem spListItem)
        {
            info.Level = (byte)spListItem.Level;
        }

        #endregion

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used in SharePoint MetaInfo.")]
        public void RestoreMetaInfo(SPFile file, byte[] bts)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.RestoreMetaInfo"))
            {

                try
                {
                    List<string> needRestore = new List<string> { "ipfs_listform", "ipfs_streamhash" };
                    if (file.ParentFolder.ParentListId == Guid.Empty || file.Item == null)
                    {
                        needRestore.Add("ContentTypeId");
                    }
                    bool needUpdate = false;
                    foreach (string key in needRestore)
                    {
                        if (info.MetaInfoDic.ContainsKey(key))
                        {
                            file.Properties[key] = info.MetaInfoDic[key];
                            needUpdate = true;
                        }
                    }
                    if (needUpdate)
                    {
                        file.Update();
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("An error occurred while restoring file MetaInfo. File name: {0}, Details: {1}", file.Name, e.ToString());
                }

            }

        }

        //处理替换PublishingPageContent中publishingReusableFragmentIdSection 指向ReusableContentList item的Link
        public void ReplaceReusableContentLink(XmlDocument xDoc)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.ReplaceReusableContentLink"))
            {

                string _ReusableContentListId = (string)mParentFolder.ParentWeb.AllProperties["_ReusableContentListId"];
                string __PagesListId = (string)mParentFolder.ParentWeb.AllProperties["__PagesListId"];
                try
                {
                    if (!string.IsNullOrEmpty(_ReusableContentListId) && !string.IsNullOrEmpty(__PagesListId))
                    {
                        Guid reusableContentListId = new Guid(_ReusableContentListId);
                        Guid pagesListId = new Guid(__PagesListId);
                        if (mSPList.ID == pagesListId && info.MappingManager.SiteMappingManager.ContainsKeyForItemIdMapping(reusableContentListId))
                        {
                            foreach (XmlNode node in xDoc.GetElementsByTagName("div"))
                            {
                                if (node.Attributes["id"] != null && node.Attributes["id"].Value == "__publishingReusableFragmentIdSection")
                                {
                                    foreach (XmlNode node1 in node.ChildNodes)
                                    {
                                        if (node1.Name == "a")
                                        {
                                            string tValue = node1.Attributes["href"].Value;
                                            if (tValue.EndsWith("_.000", StringComparison.OrdinalIgnoreCase))
                                            {
                                                string tValue1 = tValue.Substring(tValue.LastIndexOf('/') + 1);
                                                tValue1 = tValue1.Substring(0, tValue1.IndexOf('_'));
                                                int originalId = Int32.Parse(tValue1);
                                                int desId = info.MappingManager.SiteMappingManager.GetMappingItemId(reusableContentListId, originalId);
                                                if (desId != -1)
                                                    node1.Attributes["href"].Value = tValue.Substring(0, tValue.LastIndexOf('/') + 1) + desId + "_.000";
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.WARN, "An error occurred while ReplaceReusableContentLink.error:{0}", e.ToString());
                    logger.Warn("An error occurred while Replace Reusable Content Link: {0}", e.ToString());
                }

            }

        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used in SharePoint MetaInfo")]
        public void RestoreMetaInfo(SPListItem item, byte[] bts)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.RestoreMetaInfo_1"))
            {

                try
                {
                    Dictionary<string, string> metaInfoDic = GetMetaInfoDictionary(bts);
                    string[] needRestore = new string[] { "ipfs_streamhash", "vti_Snapshots" };
                    bool needUpdate = false;
                    foreach (string key in needRestore)
                    {
                        if (metaInfoDic.ContainsKey(key))
                        {
                            item.Properties[key] = metaInfoDic[key];
                            needUpdate = true;
                        }
                    }
                    if (metaInfoDic.ContainsKey("PublishingPageLayout"))
                    {
                        item.Properties["PublishingPageLayout"] = AveReplaceProcessor.UrlReplace(metaInfoDic["PublishingPageLayout"].ToString(), info.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), info.MappingManager.SiteMappingManager.SourceSiteInfo, info.MappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl);
                        needUpdate = true;
                    }
                    if (metaInfoDic.ContainsKey("RedirectURL"))
                    {
                        //此处item["RedirectURL"] 影响item.Properties["RedirectURL"]，如果是给item.Properties["RedirectURL"]赋值,出现赋值无效的问题。
                        item["RedirectURL"] = AveReplaceProcessor.UrlReplace(metaInfoDic["RedirectURL"].ToString(), info.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), info.MappingManager.SiteMappingManager.SourceSiteInfo, info.MappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl);
                        //item.Properties["RedirectURL"] = AveReplaceProcessor.UrlReplace(metaInfoDic["RedirectURL"].ToString(), ParentFolder.ParentList.ParentWeb.ParentSite.SiteManagedMappings, new ReplaceOption(true, true));
                    }
                    if (metaInfoDic.ContainsKey("Content Archived"))
                    {
                        item["Content Archived"] = (object)false;
                        needUpdate = true;
                    }
                    if (metaInfoDic.ContainsKey("_CopySource"))
                    {
                        item.Properties["_CopySource"] = AveReplaceProcessor.UrlReplace(metaInfoDic["_CopySource"], info.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), info.MappingManager.SiteMappingManager.SourceSiteInfo, info.MappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl);
                        needUpdate = true;
                    }
                    string[] needRestore1 = new string[] { "PublishingPageContent", "SummaryLinks", "PublishingPageImage" };
                    AveSiteMappingManager siteMappingManager = info.MappingManager.SiteMappingManager;
                    foreach (string key in needRestore1)
                    {
                        if (metaInfoDic.ContainsKey(key))
                        {

                            string value = metaInfoDic[key];
                            try
                            {
                                XmlDocument fieldDoc = new XmlDocument();
                                fieldDoc.PreserveWhitespace = true;
                                value = value.Replace(@"\r\n", "\r\n");
                                fieldDoc.InnerXml = "<ReplaceXmlLinks>" + value + "</ReplaceXmlLinks>";
                                if (key == "PublishingPageContent")
                                {
                                    ReplaceReusableContentLink(fieldDoc);
                                }
                                foreach (XmlNode node in fieldDoc.GetElementsByTagName("a"))
                                {
                                    //CI-34011
                                    if (node.Attributes["href"] != null)
                                    {
                                        node.Attributes["href"].Value = AveReplaceProcessor.UrlReplace(node.Attributes["href"].Value, siteMappingManager.SiteManagedMappings, new ReplaceOption(true), siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
                                    }
                                }
                                foreach (XmlNode node in fieldDoc.GetElementsByTagName("img"))
                                {
                                    node.Attributes["src"].Value = AveReplaceProcessor.UrlReplace(node.Attributes["src"].Value, siteMappingManager.SiteManagedMappings, new ReplaceOption(true), siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
                                }
                                value = fieldDoc.FirstChild.InnerXml;
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.SetXmlAttError, e.ToString());
                                value = AveReplaceProcessor.ReplaceStringLinks(value, info.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true), info.MappingManager.SiteMappingManager.SourceSiteInfo, info.MappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl);
                            }
                            item.Properties[key] = value;
                            needUpdate = true;
                        }
                    }
                    if (needUpdate)
                    {
                        item.SystemUpdate(false);
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("An error occurred while restoring item MetaInfo. Item name: {0}, details: {1}", item.Name, e.ToString());
                }

            }

        }

        private Dictionary<string, string> GetMetaInfoDictionary(byte[] bts)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.GetMetaInfoDictionary"))
            {

                string metaInfo = string.Empty;
                if (AveCompressedUtility.IsTCompressedBytes(bts))
                {
                    metaInfo = AveCompressedUtility.GetTCompressedString(bts);
                }
                else
                {
                    metaInfo = Encoding.UTF8.GetString(bts);
                }
                return AveCompressedUtility.GetMetaInfoDictionary(metaInfo);

            }

        }

        /// <summary>
        /// 把冲突的item移到冲突文件夹下，并且修改其name和tp_guid
        /// </summary>
        /// <param name="parentFolder"></param>
        /// <param name="listItem"></param>
        /// <param name="mSqlConn"></param>
        /// <returns>是否成功</returns>
        [SuppressMessage("FxCopCustomRules", "C100003:DoNotUseSpecificSPMethod", MessageId = "Microsoft.SharePoint.SPList.get_Items", Justification = "Do not refuse SharePoint API")]
        public bool MoveToConflictFolder(SPList parentList, SPFolder parentFolder, SPListItem listItem, bool isSourceWin)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.MoveToConflictFolder"))
            {

                if (!parentList.ServerTemplateCanCreateFolders)
                {
                    return false;
                }
                if (parentList.EnableFolderCreation == false)
                {
                    parentList.EnableFolderCreation = true;
                }

                SPFolder conflictFolder = null;
                try
                {
                    conflictFolder = parentFolder.SubFolders[AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME];
                }
                catch (ArgumentException)
                {
                    if (parentList.BaseType == SPBaseType.DocumentLibrary)
                    {
                        conflictFolder = parentFolder.SubFolders.Add(AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME);
                    }
                    else
                    {
                        SPListItem item = parentList.Items.Add(parentFolder.ServerRelativeUrl, SPFileSystemObjectType.Folder, AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME);
                        item["Title"] = AveWrapperConstants.REPLICATOR_CONFLICT_FOLDER_NAME;//13 RTM API in generic list folder name is not update to sharepoint, need set again
                        item.SystemUpdate(false);
                        conflictFolder = item.Folder;
                        mAveParentFolder.Reload();
                    }
                }
                DateTime lastModified = DateTime.MinValue;

                if (listItem.File != null)
                {
                    lastModified = listItem.File.TimeLastModified;
                }
                else
                {
                    lastModified = mSite.QueryService.GetLastModifiedByNative(mSite.ID, parentList.ID, listItem.ID, true);
                }
                lastModified = parentList.ParentWeb.RegionalSettings.TimeZone.UTCToLocalTime(lastModified);
                if (parentList.BaseType == SPBaseType.DocumentLibrary)
                {
                    return MoveDocumentToConflictFolder(listItem.File, parentFolder, conflictFolder, lastModified, isSourceWin);
                }
                else
                {
                    return MoveListItemToConflictFolder(parentList, listItem, parentFolder, conflictFolder, lastModified, isSourceWin);
                }

            }

        }

        private string GetTitleColNameFromSchemaXml(SPListItem listItem, SPList parentList)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.GetTitleColNameFromSchemaXml"))
            {

                SPField Titlefiled = null;
                string TitleColName = string.Empty;
                try
                {
                    Titlefiled = listItem.Fields["Title"];
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.LoadXml(Titlefiled.SchemaXml);
                    XmlElement firstNode = (XmlElement)xDoc.FirstChild;
                    if (firstNode.HasAttribute("ColName"))
                    {
                        TitleColName = firstNode.GetAttribute("ColName");
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.XmlFormatInvalid,
                        Titlefiled == null ? string.Empty : Titlefiled.SchemaXml, e.ToString());
                    if (parentList.ParentWeb.Language != 1033)
                    {
                        //not English in destination
                        XmlDocument listFieldsDoc = new XmlDocument();
                        listFieldsDoc.LoadXml(parentList.SchemaXml);
                        foreach (XmlNode fieldNod in listFieldsDoc.GetElementsByTagName("Field"))
                        {
                            XmlElement fieldEle = fieldNod as XmlElement;
                            if (fieldEle.GetAttribute("Name").Equals("Title") && fieldEle.GetAttribute("Name").Equals("Title"))
                            {
                                if (fieldEle.HasAttribute("ColName"))
                                {
                                    TitleColName = fieldEle.GetAttribute("ColName");
                                    break;
                                }
                            }
                        }
                    }
                }

                return TitleColName;

            }

        }

        /// <summary>
        /// 使用API移动pic image slide library下的文件到冲突文件夹下
        /// 注意需要keep current version的modify和modify by
        /// 因为上层已经考虑到了checkout的情况，所以这里可以不用去切换checkout user
        /// </summary>
        /// <returns></returns>
        private bool MoveDocumentToConflictFolder(SPFile currentFile, SPFolder parentFolder, SPFolder conflictFolder, DateTime lastModifyTime, bool isSourceWin)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.MoveThumbnailToConflictFolder"))
            {

                try
                {
                    AveBasicItemInfo basicItemInfo = new AveBasicItemInfo();
                    try
                    {
                        currentFile = LoadCheckOutFile(parentFolder.ParentWeb, parentFolder, currentFile.Name);
                    }
                    catch (Exception ex)
                    {
                        logger.Log(AveLogLevel.WARN, "Failed to load checkout file while moving document to conflict folder. Document Url: {0}. Error: {1}", currentFile.Url, ex.ToString());
                        return false;
                    }
                    Guid itemId = currentFile.UniqueId;
                    int uiversion = currentFile.UIVersion;
                    int level = (int)currentFile.Level;

                    if (mSite.QueryService.QueryBasicInfoForThumbNail(mSite.ID, parentFolder.UniqueId, itemId, uiversion, level, basicItemInfo))
                    {
                        string moveFileTitle = isSourceWin ? AveSPUtility.GetConflictNewName(currentFile.Name, lastModifyTime) : currentFile.Name;
                        string moveFileUrl = conflictFolder.ServerRelativeUrl + "/" + moveFileTitle;
                        currentFile.MoveTo(moveFileUrl);
                        if (mSite.NativeApiPermission == WrapperNativeApiPermission.FullControl)
                        {
                            mSite.QueryService.UpdateBasicInfoForThumbNail(mSite.ID, conflictFolder.UniqueId, itemId, uiversion, level, basicItemInfo);
                        }
                        else
                        {
                            basicItemInfo.Tp_modify = currentFile.Item.ParentList.ParentWeb.RegionalSettings.TimeZone.UTCToLocalTime(basicItemInfo.Tp_modify);
                            basicItemInfo.Tp_create = currentFile.Item.ParentList.ParentWeb.RegionalSettings.TimeZone.UTCToLocalTime(basicItemInfo.Tp_create);
                            currentFile.Item[SPBuiltInFieldId.Modified] = basicItemInfo.Tp_modify;
                            currentFile.Item[SPBuiltInFieldId.Created] = basicItemInfo.Tp_create;
                            currentFile.Item[SPBuiltInFieldId.Editor] = currentFile.Item.ParentList.ParentWeb.SiteUsers.GetByID(basicItemInfo.Editor);
                            currentFile.Item[SPBuiltInFieldId.Author] = currentFile.Item.ParentList.ParentWeb.SiteUsers.GetByID(basicItemInfo.Author);
                            if (!AveItem.AveItemSystemUpdate(currentFile.Item, false, true, info.Level == 1, true))
                            {
                                logger.Log(AveLogLevel.WARN, "Failed to internal update file basic info while moving document to conflict folder. Document url:{0}", currentFile.Url);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Can not find the document information.");
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, "There is an error occurred while move the file: {0}. Detail: {1}", currentFile.Name, e.ToString());
                    return false;
                }
                return true;

            }

        }

        private bool MoveListItemToConflictFolder(SPList parentList, SPListItem listItem, SPFolder parentFolder, SPFolder conflictFolder, DateTime lastModified, bool isSourceWin)
        {
            try
            {
                AveBasicItemInfo basicItemInfo = new AveBasicItemInfo();
                Guid itemId = listItem.UniqueId;
                SPFile file = parentList.ParentWeb.GetFile(SPResourcePath.FromDecodedUrl(listItem.Url));
                //string titleColName = GetTitleColNameFromSchemaXml(listItem, parentList);
                //MoveListItemToConflictFolderByNative()对update titleColName处理有问题，并且如果file不存在就不用move，无需对file进行判断
                //if (!file.Exists)
                //{
                //    if (mSite.NativeApiPermission == WrapperNativeApiPermission.FullControl)
                //    {
                //        return mSite.QueryService.MoveListItemToConflictFolderByNative(titleColName, parentFolder.ServerRelativeUrl, conflictFolder.Name, conflictFolder.ParentListId, conflictFolder.UniqueId, listItem.ID, listItem.UniqueId, lastModified, listItem.Web.Site.ID);
                //    }
                //    else
                //    {
                //        logger.Log(AveLogLevel.WARN, "Failed to move list item to conflict folder because of permission issue. Item Url: {0}", listItem.Url);
                //    }
                //}
                int uiversion = file.UIVersion;
                int level = (int)listItem.Level;
                if (mSite.QueryService.QueryBasicInfoForThumbNail(mSite.ID, parentFolder.UniqueId, itemId, uiversion, level, basicItemInfo))
                {
                    string itemName = isSourceWin ? string.Format("{0}({1})", listItem.Title, AveDateTimeUtility.ConvertToType008(lastModified)) : listItem.Title;
                    listItem["Title"] = itemName;
                    listItem.SystemUpdate(false);
                    object status = null;
                    if (parentList.EnableModeration)
                    {
                        status = listItem["_ModerationStatus"];
                    }
                    file.MoveTo(string.Format("{0}/{1}/{2}", parentFolder.ServerRelativeUrl, conflictFolder.Name, itemName));
                    if (parentList.EnableModeration && listItem["_ModerationStatus"] != status)
                    {
                        listItem["_ModerationStatus"] = status;
                        listItem.SystemUpdate(false);
                    }
                    if (mSite.NativeApiPermission == WrapperNativeApiPermission.FullControl)
                    {
                        mSite.QueryService.UpdateBasicInfoForThumbNail(mSite.ID, conflictFolder.UniqueId, itemId, uiversion, level, basicItemInfo);
                    }
                    else
                    {
                        basicItemInfo.Tp_modify = parentList.ParentWeb.RegionalSettings.TimeZone.UTCToLocalTime(basicItemInfo.Tp_modify);
                        basicItemInfo.Tp_create = parentList.ParentWeb.RegionalSettings.TimeZone.UTCToLocalTime(basicItemInfo.Tp_create);
                        listItem[SPBuiltInFieldId.Modified] = basicItemInfo.Tp_modify;
                        listItem[SPBuiltInFieldId.Created] = basicItemInfo.Tp_create;
                        listItem[SPBuiltInFieldId.Editor] = parentList.ParentWeb.SiteUsers.GetByID(basicItemInfo.Editor);
                        listItem[SPBuiltInFieldId.Author] = parentList.ParentWeb.SiteUsers.GetByID(basicItemInfo.Author);
                        if (!AveItem.AveItemSystemUpdate(listItem, false, false, level == 1, false))
                        {
                            logger.Log(AveLogLevel.WARN, "Failed to internal update item basic info while moving list item to conflict folder. Item url:{0}", listItem.Url);
                        }
                    }
                    return true;
                }
                else
                {
                    throw new Exception("Can not find the list item information.");
                }
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.DEBUG, "There is an error occurred while moving the list item: {0}. Detail: {1}", listItem.Url, e.ToString());
                return false;
            }
        }

        /// <summary>
        /// 解锁过程会重新load item\file对象，所以要把新的对象返回
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public SPListItem UnLockItem(SPListItem item)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.UnLockItem"))
            {

                if ((AveEnv.IsMoss))
                {
                    if (AveEnv.IsPublishing)
                    {
                        return UnlockItemByNonWssAPI(item);
                    }
                }
                return item;

            }

        }

        private SPListItem UnlockItemByNonWssAPI(SPListItem item)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.UnlockItemByNonWssAPI"))
            {

                if (Records.IsLocked(item))
                {
                    Records.UndeclareItemAsRecord(item);
                    item = mSPList.GetItemById(item.ID);
                }
                //if (item.File != null && item.Level == SPFileLevel.Checkout)
                //{
                //    Records.UnlockItem(item, item.Name);
                //    item = mSPList.GetItemById(item.ID);
                //}
                if (item.Properties.ContainsKey("_vti_ItemHoldRecordStatus") && !item.Properties["_vti_ItemHoldRecordStatus"].ToString().Equals("0"))
                {
                    //清空_vti_ItemHoldRecordStatus属性值，不然在删除的时候可能删不了
                    item.Properties["_vti_ItemHoldRecordStatus"] = "0";
                    item.SystemUpdate(false);
                }
                return item;

            }

        }

        public void UnLockItem(List<SPListItem> holdIds, SPListItem item)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.UnLockItem_1"))
            {

                if (Hold.IsItemOnHold(item))
                {
                    foreach (SPListItem holdId in holdIds)
                    {
                        Hold.RemoveHold(item, holdId, string.Empty);
                    }
                }
                if (Records.IsLocked(item))
                {
                    Records.UndeclareItemAsRecord(item);
                }
                if (item.File != null && item.Level == SPFileLevel.Checkout)
                {
                    Records.UnlockItem(item, item.Name);
                }

            }

        }

        private SPFile IncreaseVersion(IAveRestoreStream receiver, bool increaseMajorVersion, SPFolder folder, SPFile file, bool isCheckOut, string checkinComment, bool restoreContent, List<SPListItem> holdItems, Hashtable HTMetaInfo)
        {
            return IncreaseVersion(receiver, increaseMajorVersion, folder, file, isCheckOut, checkinComment, restoreContent, holdItems, HTMetaInfo, null);
        }

        private SPFile IncreaseVersion(IAveRestoreStream receiver, bool increaseMajorVersion, SPFolder folder, SPFile file, bool isCheckOut, string checkinComment, bool restoreContent, List<SPListItem> holdItems, Hashtable HTMetaInfo, AveDocumentInfo docInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.IncreaseVersion"))
            {
                if (increaseMajorVersion && file.Level == SPFileLevel.Checkout)
                {
                    file.CheckIn(checkinComment, SPCheckinType.MajorCheckIn);
                }
                UpdateListSettings(mSPList, info, increaseMajorVersion, false);

                Stream stream = null;
                if (info.HasStream)
                {
                    stream = new AveSPFileStream(receiver);
                }

                try
                {
                    if (file.Item != null)
                    {
                        SPListItem tempItem = null;
                        if (file.UniqueId != file.Item.UniqueId)
                        {//在日语环境下，文件名如果只有半角和全角的区别时，file.Item会取错item
                            tempItem = mSPList.GetItemByUniqueId(file.UniqueId);
                        }
                        else
                        {
                            tempItem = file.Item;
                        }
                        SPListItem unLockedItem = UnLockItem(tempItem);
                        if (unLockedItem != null)
                        {
                            file = unLockedItem.File;
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.UnlockItemError, e.ToString());
                }

                SPFolder checkoutFolder = null;
                if (file.Level != SPFileLevel.Checkout)
                {
                    if (!this.HasFullControlPermission && isCheckOut)
                    {
                        if (Web.CurrentUser.ID != info.CheckoutUserId)
                        {
                            var checkoutUser = mWeb.Web.SiteUsers.GetByID(info.CheckoutUserId);
                            var checkoutWeb = mSite.GetCheckoutWeb(mWeb.Web, mList.List, ref checkoutUser, file.UniqueId);
                            file = checkoutWeb.GetFile(SPResourcePath.FromDecodedUrl(file.ServerRelativeUrl));
                            checkoutFolder = checkoutWeb.GetFolder(folder.UniqueId);
                        }
                        info.CheckoutUserId = -1;
                    }
                    file.CheckOut();
                }
                if (restoreContent && info.HasStream)
                {
                    try
                    {
                        if (HTMetaInfo.Count != 0)
                        {
                            using (AvePerformanceScope scope = new AvePerformanceScope("SP.AveItem.IncreaseVersion.Add->Hashtable"))
                            {
                                file = checkoutFolder != null ? checkoutFolder.Files.AddExtension(file.Name, stream, HTMetaInfo, true)
                                    : folder.Files.AddExtension(file.Name, stream, HTMetaInfo, true);
                            }
                        }
                        else
                        {
                            if (docInfo != null && docInfo.IsLinkFile && this.mList != null && this.mList.IsConnectorList.HasValue && mList.IsConnectorList.Value)
                            {
                                mList.SOIntegrationUtil.RestoreLinkFile(file.Name, folder.UniqueId, file.UniqueId, stream, true);
                                file = (mWeb.GetFile(file.UniqueId) as AveFile).File;
                            }
                            else if (info.IsStubData)
                            {
                                this.mList.SOIntegrationUtil.UpdateSOFileStream(file, stream, true, false);
                                long bsn = mList.SOIntegrationUtil.QueryService.GetMaxRbs(file.Web.Site.ID, file.UniqueId);
                                if (bsn == -1)
                                {
                                    throw new Exception("There is not a BSN");
                                }
                                this.mList.SOIntegrationUtil.UpdateStubSize((int)file.Level, file.ParentFolder.UniqueId, file.UniqueId, file.Web.Site.ID, (int)mStorageInfo.Size, bsn);
                                RestoreStubDBInfo();
                                //RestoreConnectorStub(info.GUID, info.OriginalVersion, 1);
                            }
                            else
                            {
                                string etage;
                                stream = ReplaceStreamBeforeAdd(stream, folder, file, file.Name, info.IsCurrentVersion);
                                //To replace the URL in the content.
                                if (docInfo != null && docInfo.HasStream)
                                {
                                    docInfo.ServerRelativeUrl = file.ServerRelativeUrl;
                                    AveSPDocContentReplacer replacer = new AveSPDocContentReplacer(mSite, stream, docInfo);
                                    stream = replacer.ReplaceWebPartContent();
                                }
                                using (AvePerformanceScope scope = new AvePerformanceScope("SP.AveItem.IncreaseVersion.SaveBinary"))
                                {
                                    file.SaveBinaryExtension(stream, false, true, null, null, null, out etage);
                                }
                            }
                        }
                    }
                    catch (SPException e)
                    {
                        logger.Log(AveLogLevel.DEBUG, "Restore version content. {0}", e);
                        file.UndoCheckOut();
                        throw;
                    }
                }

                try
                {
                    if (holdItems.Count > 0)
                    {
                        SPListItem tempItem = null;
                        if (file.UniqueId != file.Item.UniqueId)
                        {//在日语环境下，文件名如果只有半角和全角的区别时，file.Item会取错item
                            tempItem = mSPList.GetItemByUniqueId(file.UniqueId);
                        }
                        else
                        {
                            tempItem = file.Item;
                        }
                        UnLockItem(holdItems, tempItem);
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("After add file, unlock item: {0} error: {1} ", file.Name, e.ToString());
                }

                if (!isCheckOut)
                {
                    if (increaseMajorVersion && mSPList != null)
                    {
                        //如果是major version，并且moderation status不是approved的，需要关闭minor version。
                        if (mSPList.EnableMinorVersions && info.ModerationStatus != 0)
                        {
                            info.SettingInfo.LIST_SETTING_CHANGED = true;
                            using (AvePerformanceScope scope = new AvePerformanceScope("SP.AveItem.IncreaseVersion.EnableMinorVersions->false"))
                            {
                                mSPList.EnableMinorVersions = false;
                                mSPList.Update();
                            }
                        }
                        RestoreWebPart(info as AveDocumentInfo, new AveFile(mWeb, file));
                        using (AvePerformanceScope scope = new AvePerformanceScope("SP.AveItem.IncreaseVersion.CheckIn->Major"))
                        {
                            file.CheckIn(checkinComment, SPCheckinType.MajorCheckIn);
                        }
                        if (mSPList.EnableModeration && mSPList.EnableMinorVersions && file.Level != SPFileLevel.Published)
                        {
                            using (AvePerformanceScope scope = new AvePerformanceScope("SP.AveItem.IncreaseVersion.Approve"))
                            {
                                file.Approve(string.Empty);
                            }
                        }
                    }
                    else
                    {
                        if (mSPList == null)
                        {
                            logger.Warn("List is null when increase version.");
                        }
                        RestoreWebPart(info as AveDocumentInfo, new AveFile(mWeb, file));
                        using (AvePerformanceScope scope = new AvePerformanceScope("SP.AveItem.IncreaseVersion.CheckIn->Minor"))
                        {
                            file.CheckIn(checkinComment, SPCheckinType.MinorCheckIn);
                        }
                    }
                }
                return file;

            }


        }

        private Stream ReplaceStreamBeforeAdd(Stream stream, SPFolder folder, SPFile file, string filename, bool isCurrentVersion)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.ReplaceStreamBeforeAdd"))
            {

                Stream result = stream;
                try
                {
                    /*对于web rootfolder下面的文件，不能直接使用file.Item因为会出现异常，所以增加了file.ParentFolder.ParentListId是否为Guid.Empty的判断
                 */
                    if (file != null && !Guid.Empty.Equals(file.ParentFolder.ParentListId) && file.Item != null)
                    {
                        info.RowId = file.Item.ID;
                    }
                    ReplaceStreamByExtension(folder, file, filename, isCurrentVersion, ref result);
                    ReplaceStreamByFileName(filename, ref result);
                }
                catch (Exception e)
                {
                    logger.Warn("An error occurred while replace stream before add, file name: {0}, is current version: {1}, error: {2}", filename, isCurrentVersion, e);
                }
                return result;

            }

        }

        private bool ReplaceStreamByExtension(SPFolder folder, SPFile file, string filename, bool isCurrentVersion, ref Stream stream)
        {
            if (ReplaceStreamForReportServiceFile(folder, file, filename, isCurrentVersion, ref stream)) return true;
            //Add replace stream logic for other fuction here
            //if (ReplaceStreamForFunc1()) return true;
            //if (ReplaceStreamForFunc2()) return true;
            return false;
        }

        private bool ReplaceStreamForReportServiceFile(SPFolder folder, SPFile file, string filename, bool isCurrentVersion, ref Stream stream)
        {
            var ctIdBytes = this.info.UserData.TryGetValue("#tp_ContentTypeId") as byte[];
            if (ctIdBytes == null) return false;

            switch (ReportServiceUtil.GetReportFileType(filename, new AveContentTypeId(ctIdBytes)))
            {
                case ReportFileType.RSDS:
                    stream = AveReportingService.ReplaceDataSourceStream(stream, this);
                    return true;
                case ReportFileType.RDL:
                    stream = AveReportingService.ReplaceReportStream(stream, this);
                    return true;
                case ReportFileType.PPSDC:
                    stream = PPSDataSource.ReplaceStreamBeforeAddFile(stream, folder, file, info, isCurrentVersion);
                    return true;
                case ReportFileType.None:
                default:
                    return false;
            }
        }
        private bool ReplaceStreamByFileName(string filename, ref Stream stream)
        {
            if (filename.Equals("client_LocationBasedDefaults.html", StringComparison.OrdinalIgnoreCase) && AveSPServerUtility.IsOrInSystemFormsFolder(mParentFolder))
            {
                if (mList != null)
                {
                    this.mList.ClientLocationBasedDefaults = null;
                }
                stream = ListSettingStreamReplaceProcessor.ReplaceListColumnDefaultValueStream(stream, mList, info);
                return true;
            }
            else if (filename.Equals("RetentionPolicy.Xml", StringComparison.OrdinalIgnoreCase) && AveSPServerUtility.IsOrInSystemFormsFolder(mParentFolder))
            {
                stream = ListSettingStreamReplaceProcessor.ReplaceListRetentionStream(stream, info);
                return true;
            }
            else if (filename.Equals("Nintex_AutoStartRules.xml", StringComparison.OrdinalIgnoreCase)
                     && (AveSPServerUtility.IsOrInSystemFormsFolder(mParentFolder) || AveSPServerUtility.IsOrInListRootFolder(mParentFolder)))
            {
                stream = ListSettingStreamReplaceProcessor.ReplaceNintexAutoStartRulesStream(stream, info);
                return true;
            }
            return false;
        }
        private bool StubThumbnailsLib()
        {
            return null != mList && mList.BaseTemplate == AveListTemplateType.PictureLibrary;
        }

        /// <summary>
        /// Create folder version in library
        /// </summary>
        /// <param name="originalVersion"></param>
        /// <param name="deleteBaseVersion"></param>

        [SuppressMessage("FxCopCustomRules", "C100003:DoNotUseSpecificSPMethod")]
        private void CreateFolderVersion(int originalVersion, bool deleteBaseVersion)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.CreateFolderVersion"))
            {

                if (mSPListItem == null)
                {
                    return;
                }
                UnLockItem(mSPListItem);
                List<int> versionLabels = new List<int>();
                if (deleteBaseVersion)
                {
                    versionLabels.Add(info.Version);
                }

                IncreaseDocumentLibraryFolderMajorVersion(originalVersion, versionLabels);
                IncreaseDocumentLibraryFolderMinorVersion(originalVersion, versionLabels);
                //delete middle versions
                DeleteMiddleVersions(versionLabels);
                // 涨version可能会导致level改变，如果不让info.Level的值update正确，不然最后update modified等属性的时候使用level当db query条件的时候，query不到数据。
                info.Level = (int)mSPListItem.Level;

            }

        }
        private bool ChangeModerationSettingWhenIncreaseVersion()
        {
            bool needUpdateList = false;
            if (info.ModerationStatus == (int)SPModerationStatusType.Approved && mSPList.EnableModeration)
            {
                mSPList.EnableModeration = false;
                needUpdateList = true;
            }
            else if (info.ModerationStatus != (int)SPModerationStatusType.Approved && !mSPList.EnableModeration)
            {
                mSPList.EnableModeration = true;
                needUpdateList = true;
            }
            return needUpdateList;
        }

        private void IncreaseDocumentLibraryFolderMajorVersion(int aimVersion, List<int> versionLabels)
        {
            int aimMajorVersion = aimVersion / 512 * 512;
            if (aimMajorVersion <= info.Version)
            {
                return;
            }

            #region list setting for update major version
            try
            {
                bool needUpdateListSettings = false;
                if (mSPList.EnableVersioning == false)
                {
                    mSPList.EnableVersioning = true;
                    needUpdateListSettings = true;
                }
                if (mSPList.EnableMinorVersions)
                {
                    mSPList.EnableMinorVersions = false;
                    needUpdateListSettings = true;
                }

                needUpdateListSettings |= ChangeModerationSettingWhenIncreaseVersion();

                if (needUpdateListSettings)
                {
                    info.SettingInfo.LIST_SETTING_CHANGED = true;
                    mSPList.Update();
                }
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.ListModerationEnableFailed, mSPList.Title, e);
                return;
            }
            #endregion

            //try
            //{
            //    //通过Reflector查看SharePoint页面源代码，在Approval之前需要删除workflow
            //    mWeb.AllowUnsafeUpdates = true;
            //    foreach (SPWorkflow workflow in mSPListItem.Workflows)
            //    {
            //        if (workflow.ParentAssociation.Id == mSPListItem.ParentList.DefaultContentApprovalWorkflowId)
            //        {
            //            SPWorkflowManager.CancelWorkflow(workflow);
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.DeleteWFError, e.ToString());
            //}

            int preVersion = info.Version;
            int updateTime = 0;
            while (info.Version < aimMajorVersion)
            {
                mSPListItem.Update();

                info.Version = mSite.QueryService.GetCurrentUIVersion(info.SiteId, info.ParentId, mSPListItem.UniqueId);
                #region 这段retry逻辑不知道是什么case用的，先留着，考虑以后去掉。
                if (preVersion == info.Version)
                {
                    updateTime++;
                    if (updateTime > 5)
                    {
                        return;
                    }
                    continue;
                }
                else
                {
                    updateTime = 0;
                }
                preVersion = info.Version;
                #endregion
                versionLabels.Add(info.Version);
            }
        }

        private void IncreaseDocumentLibraryFolderMinorVersion(int aimVersion, List<int> versionLabels)
        {
            if (aimVersion <= info.Version)
            {
                return;
            }
            #region list setting for update minor version
            bool needUpdateList = false;
            if (mSPList.EnableMinorVersions == false)
            {
                mSPList.EnableMinorVersions = true;
                needUpdateList = true;
            }

            needUpdateList |= ChangeModerationSettingWhenIncreaseVersion();

            if (needUpdateList)
            {
                info.SettingInfo.LIST_SETTING_CHANGED = true;
                mSPList.Update();
            }

            #endregion

            int preVersion = info.Version;
            int updateTime = 0;
            while (info.Version < aimVersion)
            {
                mSPListItem.Update();
                info.Version = mSite.QueryService.GetCurrentUIVersion(info.SiteId, info.ParentId, mSPListItem.UniqueId);
                #region 这段retry逻辑不知道是什么case用的，先留着，考虑以后去掉。
                if (preVersion == info.Version)
                {
                    updateTime++;
                    if (updateTime > 5)
                    {
                        return;
                    }
                    continue;
                }
                else
                {
                    updateTime = 0;
                }
                preVersion = info.Version;
                #endregion
                versionLabels.Add(info.Version);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100003:DoNotUseSpecificSPMethod")]
        public void CreateItemVersion(int originalVersion, bool deleteBaseVersion)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.CreateItemVersion"))
            {

                if (mSPListItem == null)
                {
                    return;
                }
                #region For replicator realtime to restore large version item
                if (info.MaxVersionDiff > 0 && originalVersion - info.Version >= 512 * info.MaxVersionDiff)
                {
                    if (!this.HasFullControlPermission)
                    {
                        logger.Log(AveLogLevel.WARN, "Agent account does not have enough permission to increase large version");
                    }
                    else
                    {
                        List<int> deletedVersion = new List<int>();
                        if (deleteBaseVersion)
                        {
                            deletedVersion.Add(info.Version);
                        }
                        Guid parentFolderId = Guid.Empty;
                        if (mParentFolder != null)
                        {
                            parentFolderId = mParentFolder.UniqueId;
                        }
                        mSite.QueryService.IncreaseVersionByNative(originalVersion, mSite.ID, mSPListItem.UniqueId, info.Version, mSPListItem.ID, parentFolderId);
                        mSPListItem = mList.List.GetItemById(mSPListItem.ID);
                        DeleteMiddleVersions(deletedVersion);
                        InitBySPListItem(mSPListItem);
                        return;
                    }
                }
                #endregion
                //list下folder可以通过操作folder对应的listItem.Update增长version，library下folder比较特殊，需要在folder是approve状态时Update才会增长version
                if (mSPList.BaseType == SPBaseType.DocumentLibrary)
                {
                    CreateFolderVersion(originalVersion, deleteBaseVersion);
                    return;
                }
                UpdateListSettings(mList.List, info, originalVersion % 512 == 0, false);
                UnLockItem(mSPListItem);
                bool isDenied = false;
                if (mList.EnableModeration && mSPListItem.ModerationInformation != null && mSPListItem.ModerationInformation.Status == SPModerationStatusType.Denied)
                {
                    mSPListItem.ModerationInformation.Status = SPModerationStatusType.Approved;
                    mSPListItem.Update();
                    isDenied = true;
                }
                List<int> versionLabels = new List<int>();
                if (deleteBaseVersion)
                {
                    versionLabels.Add(info.Version);
                }
                int preVersion = -1;
                while (originalVersion > info.Version)
                {
                    mSPListItem.Update();
                    try
                    {
                        //version和workflow 同时存在的时候,调用update()方法会造成mSPListItem的部分属性出现null的情况，因此再赋值。
                        info.Version = mSite.QueryService.GetCurrentUIVersion(info.SiteId, info.ParentId, mSPListItem.UniqueId);
                    }
                    catch (NullReferenceException)
                    {
                        mSPListItem = mSPList.GetItemById(mSPListItem.ID);
                        mListItem = new AveListItem(mList, mSPListItem);
                        info.Version = mSite.QueryService.GetCurrentUIVersion(info.SiteId, info.ParentId, mSPListItem.UniqueId);
                    }
                    //info.Version = mSite.QueryService.GetCurrentUIVersion(info.SiteId, info.ParentId, mSPListItem.UniqueId);
                    if (preVersion == info.Version)
                    {
                        //InitBySPListItem(mSPListItem);
                        logger.Warn("Cannot increase item version by calling update for item: {0}", mSPListItem.Title);
                        break;
                    }
                    preVersion = info.Version;
                    versionLabels.Add(info.Version);
                }
                if (isDenied)
                {
                    SPFile file = mSPListItem.Web.GetFile(SPResourcePath.FromDecodedUrl(mSPListItem.Url));
                    file.TakeOffline();
                }
                //delete middle versions
                DeleteMiddleVersions(versionLabels);
                InitBySPListItem(mSPListItem);

            }

        }

        #region For Update Fields

        public void UpdateFields(Dictionary<string, object> fieldMap, AveBaseItemInfo info)
        {
            UpdateFields(fieldMap, info, false, true);
        }

        //给 Non-SP Migration使用，他们会在Migration Wrapper中调用Wrapper Server方法
        public void UpdateFields(Dictionary<string, object> fieldMap, AveBaseItemInfo info, bool throwWhenUpdateFailed)
        {
            UpdateFields(fieldMap, info, throwWhenUpdateFailed, true);
        }

        /// <summary>
        /// 更新item field
        /// </summary>
        /// <param name="fieldMap"></param>
        /// <param name="info"></param>
        /// <param name="ThrowWhenUpdateFailed"></param>
        /// <param name="needUpdate">是否更新item basic info, 使用wrapper进行还原因为会在最后调用一次更新，所以这个地方不需要进行更新</param>
        public void UpdateFields(Dictionary<string, object> fieldMap, AveBaseItemInfo info, bool ThrowWhenUpdateFailed, bool needUpdate)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.UpdateFields"))
            {

                string url = mSPListItem.Url;
                if (!UpdateFieldsInternal(mSPListItem, info, fieldMap, ThrowWhenUpdateFailed))
                {
                    try
                    {
                        if (fieldMap != null)
                        {
                            using (new AvePerformanceScope("SP.AveItem.UpdateFields.UpdateFields->Single"))
                            {
                                foreach (var field in fieldMap)
                                {
                                    try
                                    {
                                        mSPListItem = mSPListItem.ParentList.GetItemById(mSPListItem.ID);
                                        Dictionary<string, object> tmpFieldMap = new Dictionary<string, object>();
                                        tmpFieldMap.Add(field.Key, field.Value);
                                        AddFields(mSPListItem, tmpFieldMap, info);
                                        mSPListItem.SystemUpdate(false);
                                    }
                                    catch (UnauthorizedAccessException e1)
                                    {
                                        if ((int)mSPList.BaseTemplate == 544)
                                        {
                                            logger.Warn(string.Format("An error occurred while updating the item in Micro Feed list. \r\n ERROR: {0}", e1.ToString()));
                                        }
                                        else
                                        {
                                            logger.Log(AveLogLevel.INFO, WrapperReportResource.Wrapper_Report_CannotUpdateColumValueError, mSPListItem.Url, field.Key, field.Value, e1.ToString());
                                            Report.AddDetail(new AveWrapperReportDto(mSPListItem.Name, mSPListItem.Title, AveReportObjectType.UpdateField, AveStatus.Failed, AveReportResource.Wrapper_Report_CannotUpdateColumValueError, mSPListItem.Url, field.Key, field.Value, e1.Message));
                                        }
                                    }
                                    catch (Exception e2)
                                    {
                                        logger.Log(AveLogLevel.INFO, WrapperReportResource.Wrapper_Report_CannotUpdateColumValueError, mSPListItem.Url, field.Key, field.Value, e2.ToString());
                                        Report.AddDetail(new AveWrapperReportDto(mSPListItem.Name, mSPListItem.Title, AveReportObjectType.UpdateField, AveStatus.Failed, AveReportResource.Wrapper_Report_CannotUpdateColumValueError, mSPListItem.Url, field.Key, field.Value, e2.Message));
                                    }
                                }
                            }
                        }
                    }
                    catch (UnauthorizedAccessException e3)
                    {
                        if ((int)mSPList.BaseTemplate == 544)
                        {
                            logger.Warn(string.Format("{0}", e3.ToString()));
                        }
                        else
                        {
                            logger.Warn("An error occurred while updating an item fields. Url: {0}, Id: {1}, Details: {2}", mSPListItem.Url, mSPListItem.UniqueId, e3.ToString());
                        }
                    }
                    catch (Exception e4)
                    {
                        logger.Warn("An error occurred while updating an item fields. Url: {0}, Id: {1}, Details: {2}", mSPListItem.Url, mSPListItem.UniqueId, e4.ToString());
                    }
                }
                //新上传的xml格式文件，第一次update不能把title正确更新，需要再次更新一次才能更新进去，maybe sharepoint's bug.
                try
                {
                    if (url != null && url.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        if (fieldMap.ContainsKey("Title") && fieldMap["Title"] is AveFieldValueInfo)
                        {
                            string title = (string)(fieldMap["Title"] as AveFieldValueInfo).ColValue;
                            if (!string.Equals(title, (string)mSPListItem["Title"]))
                            {
                                mSPListItem["Title"] = title;
                                using (new AvePerformanceScope("SP.AveItem.UpdateFields.SystemUpdate->Title"))
                                {
                                    mSPListItem.SystemUpdate(false);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.ConnotGetItemUrl, e.ToString());
                }
                if (info is AveDocumentInfo)
                {
                    RestoreWebPart(info as AveDocumentInfo);
                    ReloadItem();
                }

                //Document在有些情况下，用API也不能正确更新Editor，Author，Modified和Created这几个属性，再次也需要用SQL更新
                // if (mType != AveItemType.Document)
                {
                    if (fieldMap.Count > 0 && mSPListItem != null && mSPListItem.ParentList != null)
                    {
                        try
                        {
                            //不能保证在还原过程中一致keep相关信息，因此在最后统一更新即可，此处不再需要单独更新
                            if (TryUpdateItemBasicInfo(fieldMap))
                            {

                                //不需要ReloadItem，暂时后面没有使用这两个Column Value值
                                if (fieldMap.ContainsKey("Modified_x0020_By") && fieldMap.ContainsKey("Created_x0020_By"))
                                {
                                    UpdateModifiedBy(((AveFieldValueInfo)fieldMap["Modified_x0020_By"]).ColValue.ToString(), ((AveFieldValueInfo)fieldMap["Created_x0020_By"]).ColValue.ToString());
                                }
                                //wrapper中使用，默认值为false，不需要进行更新
                                if (needUpdate)
                                {
                                    UpdateDataByNative(false, true);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Warn("An error occurred while updating Editor and Author. Url: {0}, Id: {1}, Details: {2}", url, mSPListItem.UniqueId, e);
                        }
                    }
                }

                // UpdateFileModerationStatus需要在最后一次用internalUpdate把editor更新上去(TryUpdateItemBasicInfo)之后再调用，否则editor会被更新成agent account，之后再使用internalUpdate也无法正确更新editor。
                if (info is AveDocumentInfo)
                {
                    UpdateFileModerationStatus(info as AveDocumentInfo);
                }

                try
                {
                    mOwnerLoginName = fieldMap.ContainsKey("Author") ? (mAveParentFolder.ParentWeb as AveWeb).GetSiteUserById(Convert.ToInt32((fieldMap["Author"] as AveFieldValueInfo).ColValue)) : null;
                }
                catch (Exception e)
                {
                    logger.Warn("An error occurred while getting the owner. Url: {0}, Id: {1}, Details: {2}", mSPListItem.Url, mSPListItem.UniqueId, e.ToString()); ;
                }

            }

        }

        private bool TrySetUserData(string columnName, string fieldMapKey, Dictionary<string, object> fieldMap)
        {
            bool setSucceed = false; ;
            object fieldValue;
            if (fieldMap.TryGetValue(fieldMapKey, out fieldValue))
            {
                SetUserData(columnName, ((AveFieldValueInfo)fieldValue).ColValue);
                setSucceed = true;
            }
            return setSucceed;
        }

        private void ReloadItem()
        {
            if (mSPListItem == null || mSPListItem.ParentList == null)
            {
                return;
            }
            try
            {
                mSPListItem = mSPListItem.ParentList.GetItemById(mSPListItem.ID);//TO DO Performance
            }
            catch (Exception ex)
            {
                if (mSPListItem.ParentList.BaseTemplate == SPListTemplateType.Survey)
                {
                    logger.Debug("Failed to reload survey item. Error:{0}", ex.ToString());
                    mSPListItem = LoadCheckoutListItem(mSPListItem.Web, mSPListItem.ParentList, mSPListItem.ID);
                }
                else
                {
                    throw;
                }
            }
            //同步重新赋值
            mListItem = new AveListItem(this.mList, mSPListItem);
        }

        // 这个方法需要在最后一次用internalUpdate把editor更新上去之后再调用，否则editor会被更新成agent account，之后再使用internalUpdate也无法正确更新editor。
        private void UpdateFileModerationStatus(AveDocumentInfo info)
        {
            try
            {
                if (!mList.EnableModeration && (info.ModerationStatus == 0 && info.OriginalVersion % 512 == 0 || info.ModerationStatus == 3 && info.OriginalVersion % 512 != 0))
                {
                    return;
                }
                else
                {
                    if (!mList.EnableModeration)
                    {
                        using (new AvePerformanceScope("SP.AveItem.UpdateFileModerationStatus.EnableModeration->true"))
                        {
                            mList.EnableModeration = true;
                            mList.Update();
                            info.SettingInfo.LIST_SETTING_CHANGED = true;
                        }
                    }
                }
                using (new AvePerformanceScope("SP.AveItem.UpdateFileModerationStatus"))
                {
                    SPModerationStatusType moderationType = (SPModerationStatusType)info.ModerationStatus;
                    if (mSPListItem.ModerationInformation.Status != moderationType && mSPListItem.Level != SPFileLevel.Checkout)
                    {
                        //如果Agent Account对DB有DBO权限的时候，使用Native方式更新一下，否则，不用更新相关属性，Approved状态会调用API更改
                        if (this.HasFullControlPermission && moderationType != SPModerationStatusType.Approved)
                        {
                            info.NeedUpdateStatusByNative = true;
                            return;
                        }

                        try
                        {
                            mSPListItem.ModerationInformation.Status = moderationType;
                            mSPListItem.ModerationInformation.Comment = info.ModerationComments;
                            if (AveItemSystemUpdate(mSPListItem, true, true, moderationType == SPModerationStatusType.Approved, true))
                            {
                                mSPListItem = mList.List.GetItemById(mSPListItem.ID);
                                ListItem = new AveListItem(mList, mSPListItem);
                            }
                            info.Level = (byte)mSPListItem.Level;
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.DEBUG, ServerAPIResource.UpdateItemError, e);
                            mList.Reload();
                            mSPListItem = mList.List.GetItemById(mSPListItem.ID);
                            //同步重新赋值
                            ListItem = new AveListItem(mList, mSPListItem);
                            info.Level = (byte)mSPListItem.Level;
                            info.NeedUpdateStatusByNative = true;
                        }
                        mSPFile = mSPListItem.File;
                        mFile = new AveFile(mWeb, mSPFile);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "Failed to update document moderation status. Error:{0}", ex.ToString());
            }
        }

        //Use UpdateFileModerationStatus(AveDocumentInfo info) instead.
        private void UpdateModerationInfomation(AveDocumentInfo info)
        {
            SPModerationStatusType moderationType = (SPModerationStatusType)info.ModerationStatus;
            SPListItem item = mSPListItem;
            if (item.ModerationInformation != null)
            {
                if (item.ModerationInformation.Status != moderationType && item.Level != SPFileLevel.Checkout)
                {
                    if (moderationType == SPModerationStatusType.Approved)
                    {
                        try
                        {
                            item.ModerationInformation.Status = moderationType;
                            item.ModerationInformation.Comment = info.ModerationComments;
                            //经调试2010RC在此处用item.Update()不长version，用item.SystemUpdate(false)会导致还原后面version时出现问题。
                            item.Update();
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.DEBUG, ServerAPIResource.UpdateItemError, e);
                            ReloadFile();
                            mSPListItem = mSPFile.Item;
                            //同步重新赋值
                            mListItem = new AveListItem(mList, mSPListItem);
                            info.NeedUpdateStatusByNative = true;
                        }
                    }
                    else
                    {
                        info.NeedUpdateStatusByNative = true;
                    }
                }
                // Modified the Doc-55545: if upload file first then open list approve feature, then checkout file, status will be pending but draftownerId is null.
                else if (!info.NeedUpdateStatusByNative && info.DraftOwnerId == -1 && info.IsOrignialCheckOut && info.ModerationType == AveModerationStatusType.Pending && item.ModerationInformation.Status == SPModerationStatusType.Pending)
                {
                    info.NeedUpdateStatusByNative = true;
                    //mLog.Debug("Item in source site is created before list approve feature activated");
                }
                try
                {
                    info.Level = (int)item.Level;
                    if (mSPFile.Level != item.Level)
                    {
                        mSPFile = item.File;
                        mFile = new AveFile(mWeb, mSPFile);
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.UpdateItemLevelError, e.ToString());
                    ReloadFile();
                    mSPListItem = mSPFile.Item;
                    //同步重新赋值
                    mListItem = new AveListItem(mList, mSPListItem);
                    info.Level = (int)mSPListItem.Level;
                }
            }
        }

        internal void RestoreWebPart(AveDocumentInfo info)
        {
            RestoreWebPart(info, this.File);
        }

        internal void RestoreWebPart(AveDocumentInfo info, IAveFile file)
        {
            if (info.WebParts == null || info.WebParts.Count == 0 || info.WebPartRestored)
            {
                return;
            }
            //if (!this.HasFullControlPermission)
            //{
            //    info.WebParts = FilterPersonalWebPart(info.WebParts);
            //    if (info.WebParts.Count == 0)
            //    {
            //        return;
            //    }
            //}
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.RestoreWebPart"))
            {
                if (mList != null && mList.ForceCheckout == true && file.Level != AveFileLevel.Checkout)
                {
                    mList.ForceCheckout = false;
                    info.SettingInfo.LIST_SETTING_CHANGED = true;
                    using (AvePerformanceScope scope = new AvePerformanceScope("SP.AveItem.RestoreWebPart.ForceCheckout->false"))
                    {
                        mList.Update();
                    }
                }
                bool changed = false;
                bool enableModeration = false;
                bool enableMinorVersions = false;
                if (mList != null)
                {
                    enableModeration = mList.EnableModeration;
                    enableMinorVersions = mList.EnableMinorVersions;
                }
                if (mList != null && file.Item != null && file.Item.ModerationInformation != null && file.Item.ModerationInformation.Status == AveModerationStatusType.Approved)
                {
                    if (mList.EnableModeration)
                    {
                        mList.EnableModeration = false;
                        changed = true;
                    }
                    if (changed)
                    {
                        using (AvePerformanceScope scope = new AvePerformanceScope("SP.AveItem.RestoreWebPart.EnableModeration->false"))
                        {
                            mList.Update();
                        }
                    }
                }
                //已经published的major version文件，如果开启EnableMinorVersions=true，还原web part会产生新的小version。
                if (mList != null && file.Item != null && file.Level == AveFileLevel.Published)
                {
                    if (mList.EnableMinorVersions && file.UIVersion % 512 == 0)
                    {
                        mList.EnableMinorVersions = false;
                        changed = true;
                    }
                    if (changed)
                    {
                        using (AvePerformanceScope scope = new AvePerformanceScope("SP.AveItem.RestoreWebPart.EnableMinorVersions->false"))
                        {
                            mList.Update();
                        }
                    }
                }
                using (IAveLimitedWebPartManager webPartManager = file.GetLimitedWebPartManager(System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared))
                {
                    webPartManager.Cache = info.WebPartCache;
                    webPartManager.SetRestoreReport(Report);
                    webPartManager.RestoreWebParts(info.WebParts, true);
                }
                if (changed)
                {
                    mList.EnableModeration = enableModeration;
                    mList.EnableMinorVersions = enableMinorVersions;
                    using (AvePerformanceScope scope = new AvePerformanceScope("SP.AveItem.RestoreWebPart.ListUpdate"))
                    {
                        mList.Update();
                    }
                }
                info.WebPartRestored = true;
            }
        }

        private List<AveWebPartBaseInfo> FilterPersonalWebPart(List<AveWebPartBaseInfo> allWebParts)
        {
            List<AveWebPartBaseInfo> webParts = new List<AveWebPartBaseInfo>();
            foreach (var webPartInfo in allWebParts)
            {
                try
                {
                    if (webPartInfo.UserID > 0 || webPartInfo.Personalization != null)
                    {
                        logger.Log(AveLogLevel.WARN, "Skip restoring personal webpart because of permission issue. Page Url:{0}. WebPartTypeId:{1}, WebPart Class Name:{2}, WebPart Assembly Name:{3}", this.File.ServerRelativeUrl, webPartInfo.WebPartTypeId, webPartInfo.Class, webPartInfo.Assembly);
                        continue;
                    }
                    webParts.Add(webPartInfo);
                }
                catch (Exception ex)
                {
                    logger.Log(AveLogLevel.WARN, "An error occurred while filtering personal web part. Message:{0}", ex.ToString());
                    continue;
                }
            }
            return webParts;
        }

        /// <summary>
        /// 还原folder Moderation Infomation；
        /// </summary>
        /// <param name="info"></param>
        internal void UpdateFolderModerationStatus(AveFolderInfo info)
        {
            try
            {
                if (!mList.EnableModeration && (info.ModerationStatus == 0 && info.OriginalVersion % 512 == 0 || info.ModerationStatus == 3 && info.OriginalVersion % 512 != 0))
                {
                    return;
                }
                else
                {
                    if (!mList.EnableModeration)
                    {
                        mList.EnableModeration = true;
                        mList.Update();
                        info.SettingInfo.LIST_SETTING_CHANGED = true;
                    }
                }
                //当Agent Account没有权限的时候，Moderation信息会和4个特殊的column一起更新
                if (!this.HasFullControlPermission)
                {
                    return;
                }

                if (mSPListItem.ModerationInformation != null && (mSPListItem.ModerationInformation.Status != (SPModerationStatusType)info.ModerationStatus || (mSPListItem.ModerationInformation.Status == 0 && mSPListItem.Level != SPFileLevel.Published)))
                {
                    if (mSPListItem.ModerationInformation.Status == SPModerationStatusType.Approved && (SPModerationStatusType)info.ModerationStatus == SPModerationStatusType.Pending
                        && mSPList.BaseType == SPBaseType.DocumentLibrary && mSPList.EnableMinorVersions)
                    {
                        logger.Log(AveLogLevel.DEBUG, "Change list setting for Pending moderation status. Item Url:{0}", mSPListItem.Url);
                        mSPList.EnableMinorVersions = false;
                        info.SettingInfo.LIST_SETTING_CHANGED = true;
                        mSPList.Update();
                    }

                    try
                    {
                        var moderationType = (SPModerationStatusType)info.ModerationStatus;
                        if (mSPListItem.ModerationInformation.Status != moderationType)
                        {
                            mSPListItem.ModerationInformation.Status = moderationType;
                            mSPListItem.ModerationInformation.Comment = info.ModerationComments;
                            if (AveItemSystemUpdate(mSPListItem, true, true, moderationType == SPModerationStatusType.Approved, true))
                            {
                                mSPListItem = mList.List.GetItemById(mSPListItem.ID);
                                ListItem = new AveListItem(mList, mSPListItem);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.UpdateItemError, e);
                        mList.Reload();
                        mSPListItem = mList.List.GetItemById(mSPListItem.ID);
                        //同步重新赋值
                        ListItem = new AveListItem(mList, mSPListItem);
                        info.NeedUpdateStatusByNative = true;
                    }
                    finally
                    {
                        info.Level = (byte)mSPListItem.Level;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.FolderModerationInfoUpdateFailed, e);
            }
        }


        private void UpdateModifiedBy(string modifiedBy, string createdBy)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.UpdateModifiedBy"))
            {

                SPField fieldModified = mSPListItem.ParentList.Fields.GetField("Modified_x0020_By");
                string colNameModified = fieldModified.GetProperty("ColName");

                SPField fieldCreated = mSPListItem.ParentList.Fields.GetField("Created_x0020_By");
                string colNameCreated = fieldCreated.GetProperty("ColName");
                SetUserData(colNameModified, modifiedBy);
                SetUserData(colNameCreated, createdBy);
                //mSite.QueryService.UpdateModifiedBy(modifiedBy, createdBy, colNameModified, colNameCreated, info);

            }

        }

        public void UpdateSpecialPropertyByNative(string editor, string author, DateTime modified, DateTime created, AveBaseItemInfo info)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.UpdateSpecialPropertyByNative"))
            {

                mSite.QueryService.UpdateSpecialPropertyByNative(editor, author, modified, created, info);

            }

        }

        private void SetClientLocationBasedDefaults(SPListItem spListItem)
        {
            string folderName = string.Empty;
            if (this.mAveParentFolder.ServerRelativeUrl[this.mAveParentFolder.ServerRelativeUrl.Length - 1].Equals('/'))
            {
                folderName = this.mAveParentFolder.ServerRelativeUrl;
            }
            else
            {
                folderName = string.Format("{0}/", this.mAveParentFolder.ServerRelativeUrl);
            }
            foreach (var baseDefaults in this.mList.ClientLocationBasedDefaults)
            {
                if (folderName.StartsWith(baseDefaults.Key, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var defaults in baseDefaults.Value)
                    {
                        try
                        {
                            spListItem[defaults.Key] = defaults.Value;
                        }
                        catch (Exception e)
                        {
                            logger.Warn("An error occurred while set default value. key:{0}, defaultValue:{1}, ex:{2}.", defaults.Key, defaults.Value, e);
                        }

                    }
                    break;
                }
            }
        }

        private bool UpdateFieldsInternal(SPListItem spListItem, AveBaseItemInfo info, Dictionary<string, object> fieldMap, bool ThrowWhenUpdateFailed)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.UpdateFieldsInternal"))
            {

                if (fieldMap == null || fieldMap.Count == 0 || spListItem == null)
                {
                    return true;
                }
                AveItemEventReceiver receiver = new AveItemEventReceiver();
                bool isChangeEventReceiver = false;
                try
                {
                    SetModernField(spListItem, info, fieldMap);
                    SetFieldValueToNull(spListItem, info.NeedSetNullFields);
                    if (fieldMap.ContainsKey("PublishingPageContent"))
                    {
                        try
                        {
                            string value = (info.FieldsInfo.Fields["PublishingPageContent"] as AveFieldValueInfo).ColValue.ToString();
                            XmlDocument xDoc = new XmlDocument();
                            xDoc.PreserveWhitespace = true;
                            xDoc.InnerXml = "<ReplaceXmlLinks>" + value + "</ReplaceXmlLinks>";
                            ReplaceReusableContentLink(xDoc);
                            AveFieldValueInfo fieldInfo = new AveFieldValueInfo(string.Empty, xDoc.FirstChild.InnerXml, 0);
                            fieldMap["PublishingPageContent"] = fieldInfo;
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.DEBUG, ServerAPIResource.SetXmlAttError, e.ToString());
                        }
                    }

                    //pages下的Redirect的page，开启pages的approval后，设置page的RedirectURL，经过approval之后"RedirectURL"这个field的值变成null了，但是在metaInfo中存在
                    //maybe a bug of SP?
                    if (spListItem.Properties["RedirectURL"] != null && !fieldMap.ContainsKey("RedirectURL"))
                    {
                        AveFieldValueInfo fieldInfo = new AveFieldValueInfo(string.Empty, spListItem.Properties["RedirectURL"], 0);
                        fieldMap.Add("RedirectURL", fieldInfo);
                    }
                    if (WrapperRuntime.CurrentContext.IsMoss)
                    {
                        try
                        {
                            if (fieldMap.ContainsKey("RoutingConditions"))
                            {
                                AveFieldValueInfo fieldValueInfo = info.FieldsInfo.Fields["RoutingConditions"] as AveFieldValueInfo;
                                if (fieldValueInfo != null)
                                {
                                    var value = fieldValueInfo.ColValue;
                                    if (value is string)
                                    {
                                        fieldValueInfo.ColValue = ReplaceTermIdInConditions(value as string, info.FieldsInfo.TermIdMapping);
                                    }
                                }
                            }
                            AddTaxonomyFields(spListItem, info);
                        }
                        catch (Exception e)
                        {
                            logger.Warn("An error occurred while AddTaxonomyFields. Error: {0}", e.ToString());
                        }
                    }
                    if (info.FieldsInfo.FieldsInMetaInfo != null && info.FieldsInfo.FieldsInMetaInfo.Count > 0)
                    {
                        foreach (string key in info.FieldsInfo.FieldsInMetaInfo.Keys)
                        {
                            if (!info.FieldsInfo.Fields.ContainsKey(key))
                            {
                                fieldMap.Add(key, info.FieldsInfo.FieldsInMetaInfo[key]);
                            }
                        }
                    }
                    if ((this.mAveItemRestoreResult == 2) && fieldMap.ContainsKey("PublishingPageLayout"))//bug:21879,对带有version的checoutPage在执行checkin小version后使其filed value=null有效化，同时content中PublishingPageLayout置为null，避免原端数据对field value造成影响。
                    {
                        using (new AvePerformanceScope("SP.AveItem.UpdateFieldsInternal.SystemUpdate->PublishingPageLayout"))
                        {
                            spListItem.SystemUpdate(false);
                        }
                    }

                    if (AveEnv.IsMoss)
                    {
                        RestoreDocumentIdFieldForDocumentIService(spListItem, fieldMap);
                    }

                    AddFields(spListItem, fieldMap, info);
                    AddMultiLookupFields(spListItem, info);
                    //list下添加schedlue类型的contenttype就可以设置datatime以及all day event属性，这时也需要进行处理
                    if (spListItem.ParentList.BaseTemplate == SPListTemplateType.Meetings || spListItem.ParentList.BaseTemplate == SPListTemplateType.Events || fieldMap.ContainsKey("fAllDayEvent"))
                    {
                        ResetEventFields(spListItem, fieldMap, info);
                    }


                    //ADO-117290:infopath所创建的document在还原document的user data的时候已经将TemplateUrl更新，并且会同步到content中，所以不需要再次替换
                    //ModifiedSpecialFieldsInContentTypeForNewItem(fieldMap, spListItem, false, info);
                    if (receiver != null && receiver.EventFiringDisabled && (int)spListItem.ParentList.BaseTemplate == 171 && info.EnableEventReceiver)
                    {
                        receiver.EventFiringEnabled = true;
                        isChangeEventReceiver = true;
                    }
                    try
                    {
                        using (new AvePerformanceScope("SP.AveItem.UpdateFieldsInternal.SystemUpdate->EventFiringDisabled:" + receiver.EventFiringDisabled))
                        {
                            spListItem.SystemUpdate(false);//TO DO Performance
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.UpdateItemError, e);
                        //ADO-114301 使用sharepoint API执行 file.Approve ->file.Checkout -> file.Item.SystemUpdate 过程，在调用SystemUpdate时会抛异常，使用Update正常。
                        if (spListItem.Level == SPFileLevel.Checkout)
                        {
                            using (new AvePerformanceScope("SP.AveItem.UpdateFieldsInternal.Update->Exception"))
                            {
                                spListItem.Update();
                            }
                        }
                        else
                        {
                            using (new AvePerformanceScope("SP.AveItem.UpdateFieldsInternal.SystemUpdate->Exception"))
                            {
                                spListItem.SystemUpdate(false);
                            }
                        }
                    }
                    return true;
                }
                catch (Exception e)
                {
                    logger.Warn("An error occurred while updating an item fields. Url: {0}, Id: {1}, Details: {2}", spListItem.Url, spListItem.UniqueId, e.ToString());
                    if (ThrowWhenUpdateFailed)
                    {
                        throw;
                    }
                    return false;
                }
                finally
                {
                    if (isChangeEventReceiver && receiver != null && (!receiver.EventFiringDisabled))
                    {
                        receiver.EventFiringEnabled = false;
                        isChangeEventReceiver = false;
                    }
                }

            }

        }

        private void SetModernField(SPListItem spListItem, AveBaseItemInfo info, Dictionary<string, object> fieldMap)
        {
            if (spListItem.File != null && info.MappingManager != null) //小migration 没有mapping
            {
                dataProcessor = new SharePointDocumentDataProcessor(this.File, info.MappingManager.SiteMappingManager, info.SourceSiteInfo);
                dataProcessor.ProcessUserData(fieldMap);
            }
            
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "listidx")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "msft")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "hier")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "docid")]
        private void RestoreDocumentIdFieldForDocumentIService(SPListItem item, Dictionary<string, object> fieldMap)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.RestoreDocumentIdFieldForDocumentIService"))
            {

                try
                {
                    if (this.mSite.Site.Features[new Guid("B50E3104-6812-424f-A011-CC90E6327318")] == null
                       || info.AveItem.ListItem.ContentType == null
                       || (!AveSPDocumentSet.IsDocumentSet(info.AveItem.ListItem.ContentType.ID)
                            && !info.AveItem.ListItem.ContentType.ID.IsChildOf(new AveContentTypeId(AveBuiltInContentTypeId.Document))))
                    {
                        return;
                    }

                    const string DocIdField1 = "_dlc_DocId";
                    const string DocIdField2 = "_dlc_DocIdUrl";
                    const string DocIdField3 = "_dlc_DocIdUrl#2";

                    if (WrapperConfiguration.KeepDocumentIdValue)
                    {
                        //If keep source documentId,不需要在这里对DocumentId进行Assignee操作
                        return;
                    }

                    //如果不keep 原端DocumentId,需要对目的端item进行Assignee DocumentId操作
                    AssinDocId(item);
                    //For builtin document id provider, AssignDocId will add or modify the [docid_msft_hier_listidx] property of root web(on Object item.Web.Site.RootWeb)
                    //SPWeb cached in mWeb may out of date, add logic to reload web for this case. 
                    if (mWeb.IsRootWeb)
                    {
                        if (!string.Equals(TryGetWebProperty("docid_msft_hier_listidx", mWeb.Web), TryGetWebProperty("docid_msft_hier_listidx", item.Web.Site.RootWeb), StringComparison.OrdinalIgnoreCase))
                        {
                            mWeb.ReloadWeb();
                        }
                    }
                    //由于不需要keep原端DocumentId,将对应的field从fieldMap中移除
                    if (fieldMap.ContainsKey(DocIdField1)) fieldMap.Remove(DocIdField1);
                    if (fieldMap.ContainsKey(DocIdField2)) fieldMap.Remove(DocIdField2);
                    if (fieldMap.ContainsKey(DocIdField3)) fieldMap.Remove(DocIdField3);


                    //if (!fieldMap.ContainsKey(DocIdField1))
                    //{

                    //    //DocumentId feature为开启状态时，并且原端没开启对应feature时，需要给对应的file的DocumentId赋值
                    //    //在DocAve中添加file时，由于EventReceiver是Disable的，所以添加file时不会自动赋值，需要在这里进行处理
                    //    AssinDocId(item);
                    //    //For builtin document id provider, AssignDocId will add or modify the [docid_msft_hier_listidx] property of root web(on Object item.Web.Site.RootWeb)
                    //    //SPWeb cached in mWeb may out of date, add logic to reload web for this case. 
                    //    if (mWeb.IsRootWeb)
                    //    {
                    //        if (!string.Equals(TryGetWebProperty("docid_msft_hier_listidx", mWeb.Web), TryGetWebProperty("docid_msft_hier_listidx", item.Web.Site.RootWeb), StringComparison.OrdinalIgnoreCase))
                    //        {
                    //            mWeb.ReloadWeb();
                    //        }
                    //    }
                    //    return;
                    //}

                    //if (!WrapperConfiguration.KeepDocumentIdValue)
                    //{
                    //    AssinDocId(item);
                    //    //For builtin document id provider, AssignDocId will add or modify the [docid_msft_hier_listidx] property of root web(on Object item.Web.Site.RootWeb)
                    //    //SPWeb cached in mWeb may out of date, add logic to reload web for this case. 
                    //    if (mWeb.IsRootWeb)
                    //    {
                    //        if (!string.Equals(TryGetWebProperty("docid_msft_hier_listidx", mWeb.Web), TryGetWebProperty("docid_msft_hier_listidx", item.Web.Site.RootWeb), StringComparison.OrdinalIgnoreCase))
                    //        {
                    //            mWeb.ReloadWeb();
                    //        }
                    //    }
                    //    if (fieldMap.ContainsKey(DocIdField1)) fieldMap.Remove(DocIdField1);
                    //    if (fieldMap.ContainsKey(DocIdField2)) fieldMap.Remove(DocIdField2);
                    //    if (fieldMap.ContainsKey(DocIdField3)) fieldMap.Remove(DocIdField3);
                    //}
                }
                catch (Exception ex)
                {
                    logger.Warn("An error occurred while assigning doc id to file {0}. Exception: {1}", item.Url, ex);
                }

            }

        }

        private string TryGetWebProperty(string propertyName, SPWeb web)
        {
            string property = string.Empty;
            if (web.Properties.ContainsKey(propertyName))
            {
                property = web.Properties[propertyName] as string;
            }
            return property;
        }

        //Only for Wss 4.0 enviroment
        private static void AssinDocId(SPListItem item)
        {
            AveItemEventReceiver eventReceiver = new AveItemEventReceiver();
            if (!eventReceiver.EventFiringEnabled)
            {
                AveAssemblyUtility.InvokeStaticMethod(typeof(DocumentId), "AssignDocId", item);
            }
        }


        private void ModifiedSpecialFieldsInContentTypeForNewItem(Dictionary<string, object> fieldMap, SPListItem spListItem, bool needUpdateImmed, AveBaseItemInfo info)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.ModifiedSpecialFieldsInContentTypeForNewItem"))
            {

                SPList spList = spListItem.ParentList;
                //Below we need to set the content type once item created, otherwise Update() will give this item default value to the default content type.(Doc-59767)
                SPContentType itemContentType = null;
                SPContentTypeId itemContentTypeId = SPContentTypeId.Empty;
                try
                {
                    if (fieldMap != null && fieldMap.ContainsKey("ContentType") && (info.FieldsInfo.Fields != null && info.FieldsInfo.Fields.Keys.Contains("ContentType")))
                    {
                        itemContentTypeId = ((info.FieldsInfo.Fields["ContentType"] as AveFieldValueInfo).ColValue as AveContentTypeId).ContentTypeId;
                        itemContentType = spList.ContentTypes[itemContentTypeId];
                        spListItem["ContentTypeId"] = itemContentTypeId;
                        if (itemContentType.Fields.ContainsFieldWithStaticName("TemplateUrl")) //Here templateUrl is for info path content type
                        {
                            SPField sf = itemContentType.Fields.GetFieldByInternalName("TemplateUrl");
                            if (spListItem["TemplateUrl"] != null)
                            {
                                //using replaceprocessor instead of self write method  
                                //ADO-44153 http://10.2.30.30:1000  http://10.2.30.30:1000/sites/tt case can not handler
                                //url has been replace in prepare logic
                                string url = sf.GetFieldValueAsText(spListItem["TemplateUrl"]);
                                //if (!url.StartsWith(info.MappingManager.SiteMappingManager.SourceSiteInfo.WebAppUrl, StringComparison.OrdinalIgnoreCase))
                                //{
                                //    string hostheader = AveReplaceProcessor.GetHostHeader(info.MappingManager.SiteMappingManager.SourceSiteInfo.WebAppUrl);
                                //    string zoneUrl = AveReplaceProcessor.GetHostHeader(url);
                                //    url = url.Replace(zoneUrl, hostheader);
                                //}
                                //Dictionary<string, string> tempUrlMapping = info.MappingManager.SiteMappingManager.AbsoluteUrlMapping;


                                url = AveReplaceProcessor.UrlReplace(url, info.MappingManager.SiteMappingManager.SiteManagedMappings, new ReplaceOption(true, true), info.MappingManager.SiteMappingManager.SourceSiteInfo, info.MappingManager.SiteMappingManager.DestSiteInfo.ServerRelativeUrl);
                                //for (int i = tempUrlMapping.Count - 1; i >= 0; i--)
                                //{

                                //    if (url.Contains(tempUrlMapping.ElementAt(i).Key))
                                //    {
                                //        url = url.Replace(tempUrlMapping.ElementAt(i).Key, tempUrlMapping.ElementAt(i).Value);
                                //        break;
                                //    }
                                //}
                                spListItem["TemplateUrl"] = url;
                            }
                            else
                            {
                                spListItem["TemplateUrl"] = itemContentType.DocumentTemplateUrl;
                            }
                        }
                        if (needUpdateImmed)
                        {
                            spListItem.SystemUpdate(false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //mLog.Warn(string.Format("There is an error when update content type of item: {0}.", item.Url), ex);
                    logger.Warn("There is an error when update content type of item: {0}, details: {1}", spListItem.Url, ex.ToString());
                }

            }

        }
        [SuppressMessage("FxCopCustomRules", "C100003:DoNotUseSpecificSPMethod", MessageId = "Microsoft.SharePoint.SPList.get_Items", Justification = "Do not refuse SharePoint API")]
        private void ResetEventFields(SPListItem listItem, Dictionary<string, object> fieldMapping, AveBaseItemInfo info)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.ResetEventFields"))
            {

                int eventType = 0;
                if (fieldMapping.ContainsKey("EventType"))
                {
                    eventType = Convert.ToInt32(((AveFieldValueInfo)fieldMapping["EventType"]).ColValue);
                    listItem["EventType"] = eventType;
                }
                SPTimeZone timeZone = null;
                DateTime eventDate = DateTime.MinValue;
                DateTime endDate = DateTime.MinValue;
                bool allDayEvent = false;
                int duration = -1;
                SPUser agentAccount = listItem.ParentList.ParentWeb.CurrentUser;
                timeZone = (agentAccount != null && agentAccount.RegionalSettings != null) ? agentAccount.RegionalSettings.TimeZone : listItem.ParentList.ParentWeb.RegionalSettings.TimeZone;
                //ADO-136459:源端TimeZone value应与目的端相同，否则如果源端目的端时区若不同，那么还原后的时间就会与源端有差异
                object fieldValueTimeZoneId;
                if (fieldMapping.TryGetValue("TimeZone", out fieldValueTimeZoneId))
                {
                    listItem["TimeZone"] = Convert.ToInt32(((AveFieldValueInfo)fieldValueTimeZoneId).ColValue);
                }
                else if (fieldMapping.ContainsKey("UID") && (eventType == 2 || eventType == 3))
                {
                    foreach (var tItem in listItem.ParentList.Items.Cast<SPListItem>().Where(tItem => fieldMapping["UID"] is AveFieldValueInfo))
                    {
                        Guid filedUid;
                        if (!Guid.TryParse((fieldMapping["UID"] as AveFieldValueInfo).ColValue.ToString(), out filedUid))
                        {
                            logger.Warn("This is not a guid. String: {0}", (fieldMapping["UID"] as AveFieldValueInfo).ColValue);
                            continue;
                        }
                        if (tItem["UID"] == null || filedUid != new Guid(tItem["UID"].ToString()) ||
                            (int)tItem["EventType"] != 1 || tItem["Duration"] == null)
                            continue;
                        int.TryParse(tItem["Duration"].ToString(), out duration);
                        break;
                    }
                }

                //当fAllDayEvent为true时，EventDate和EventDate忽略时区。
                if (fieldMapping.ContainsKey("fAllDayEvent"))
                {
                    allDayEvent = Convert.ToBoolean(((AveFieldValueInfo)fieldMapping["fAllDayEvent"]).ColValue);
                }
                if (fieldMapping.ContainsKey("EventDate"))
                {
                    if (allDayEvent)
                    {
                        eventDate = Convert.ToDateTime(((AveFieldValueInfo)fieldMapping["EventDate"]).ColValue, System.Globalization.DateTimeFormatInfo.InvariantInfo);
                    }
                    else
                    {
                        eventDate = timeZone.UTCToLocalTime(Convert.ToDateTime(((AveFieldValueInfo)fieldMapping["EventDate"]).ColValue, System.Globalization.DateTimeFormatInfo.InvariantInfo));
                    }
                    listItem["EventDate"] = eventDate;
                }
                if (fieldMapping.ContainsKey("Duration"))
                {
                    duration = Convert.ToInt32(((AveFieldValueInfo)fieldMapping["Duration"]).ColValue);
                    listItem["Duration"] = duration;
                }

                if (fieldMapping.ContainsKey("EndDate"))
                {
                    endDate = Convert.ToDateTime(((AveFieldValueInfo)fieldMapping["EndDate"]).ColValue, System.Globalization.DateTimeFormatInfo.InvariantInfo);
                    //MaxDateTime
                    if (endDate.Year == 9999 && eventDate != DateTime.MinValue && duration >= 0)
                    {
                        TimeSpan tsEventDate = eventDate.TimeOfDay;
                        TimeSpan tsDuration = TimeSpan.FromSeconds(duration);
                        endDate = endDate.Date.Add(tsEventDate).Add(tsDuration);
                    }
                    else
                    {
                        if (!allDayEvent)
                        {
                            endDate = timeZone.UTCToLocalTime(endDate);
                        }
                    }
                    listItem["EndDate"] = endDate;
                }
                else if (eventType == 3 && eventDate != DateTime.MinValue && duration >= 0)
                {
                    TimeSpan tsDuration = TimeSpan.FromSeconds(duration);
                    endDate = eventDate.Add(tsDuration);
                    listItem["EndDate"] = endDate;
                }
                if (fieldMapping.ContainsKey("StartDate"))
                {
                    if (allDayEvent)
                    {
                        listItem["StartDate"] = Convert.ToDateTime(((AveFieldValueInfo)fieldMapping["StartDate"]).ColValue, System.Globalization.DateTimeFormatInfo.InvariantInfo);
                    }
                }

            }

        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private void AddFields(SPListItem spListItem, Dictionary<string, object> fieldMap, AveBaseItemInfo info)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.AddFields"))
            {

                if (fieldMap == null || spListItem == null)
                {
                    return;
                }
                string urlKey = string.Empty;
                foreach (KeyValuePair<string, object> field in fieldMap)
                {
                    try
                    {
                        AveFieldValueInfo fieldValue = (AveFieldValueInfo)field.Value;
                        //当没有FullControl权限的时候，当成普通Column赋值，但是AppEditor无法Keep
                        if (this.HasFullControlPermission &&
                            (field.Key.Equals("AppAuthor", StringComparison.OrdinalIgnoreCase) || field.Key.Equals("AppEditor", StringComparison.OrdinalIgnoreCase)))
                        {
                            SetUserData("tp_" + field.Key, fieldValue.ColValue);
                            continue;
                        }
                        if (field.Key.Equals("ContentType"))
                        {
                            SPContentTypeId itemContentTypeId = (fieldValue.ColValue as AveContentTypeId).ContentTypeId;
                            spListItem["ContentTypeId"] = itemContentTypeId;
                            continue;  //update CT by ContentTypeId
                        }
                        if (field.Key.Equals("PPSMA_ObjectXML"))
                        {
                            AvePerformancePointCache.AddToProcessInPostAction(info);
                        }
                        if (field.Key.Equals("File_x0020_Type", StringComparison.OrdinalIgnoreCase) && fieldValue.ColValue.ToString().StartsWith("arc_", StringComparison.OrdinalIgnoreCase) && !info.IsStubData)
                        {
                            spListItem[field.Key] = fieldValue.ColValue.ToString().Substring(4);
                            continue;//for DOC-52047, If backup is archived file, restore it as real data.
                        }
                        if (!info.IsStubData && field.Key.Equals("Content_x0020_Archived"))
                        {
                            spListItem[field.Key] = false;//for DOC-56959, If backup is archived file, restore it as real data.
                            continue;
                        }
                        if (field.Key.Equals("WikiField"))
                        {
                            //internal void SetValue(string strName, object value, SPField field, bool protectFields, bool skipValidation)
                            //使用spListItem[field.Key]给WikiField赋值是，实际上调用了SetValue("WikiField", field.Value, null, false, false)
                            //skipValidation为false表示，赋值还需要做数据验证，可能实际上赋上的值不是field.Value。参考 DOC-54634 
                            //此处使用反射调用SetValue方法，将skipValidation设置为true，跳过了数据验证的过程。
                            Type[] argsTypes = new Type[] { typeof(string), typeof(object), typeof(SPField), typeof(bool), typeof(bool) };
                            object[] args = new object[] { "WikiField", fieldValue.ColValue, null, false, true };
                            AveAssemblyUtility.InvokeMethod(spListItem, spListItem.GetType(), "SetValue", argsTypes, args);
                            continue;
                        }
                        if (field.Key.Equals("_HasCopyDestinations", StringComparison.OrdinalIgnoreCase))
                        {
                            spListItem.Properties["vti_HasCopyDests"] = fieldValue.ColValue;
                            continue;
                        }
                        if (fieldValue.ColValue is DateTime)
                        {
                            spListItem[field.Key] = spListItem.ParentList.ParentWeb.RegionalSettings.TimeZone.UTCToLocalTime(Convert.ToDateTime(fieldValue.ColValue, System.Globalization.DateTimeFormatInfo.InvariantInfo));
                        }
                        else if (fieldValue.FieldType == AveFieldType.URL)
                        {
                            SPFieldUrlValue tempUrlValue = null;
                            string currentKey = field.Key;
                            if (field.Key.EndsWith("#2", StringComparison.OrdinalIgnoreCase))
                            {
                                currentKey = field.Key.Remove(field.Key.IndexOf("#2", StringComparison.OrdinalIgnoreCase));
                                if (fieldMap.ContainsKey(currentKey))
                                {
                                    tempUrlValue = ((AveFieldValueInfo)fieldMap[currentKey]).ColValue as SPFieldUrlValue;
                                    if (tempUrlValue != null)
                                    {
                                        tempUrlValue.Description = fieldValue.ColValue.ToString();
                                    }
                                    else
                                    {
                                        tempUrlValue = new SPFieldUrlValue();
                                        tempUrlValue.Description = fieldValue.ColValue.ToString();
                                        fieldValue.ColValue = tempUrlValue;
                                        continue;
                                    }
                                }
                                else
                                {
                                    //Log.warn
                                    continue;
                                }
                            }
                            else
                            {
                                if (fieldMap.ContainsKey(currentKey + "#2"))
                                {
                                    tempUrlValue = ((AveFieldValueInfo)fieldMap[currentKey + "#2"]).ColValue as SPFieldUrlValue;
                                    if (tempUrlValue != null)
                                    {
                                        tempUrlValue.Url = fieldValue.ColValue.ToString();
                                        //[ADO-55856]migration 07-13 Barcode url we need replace doc id to item id
                                        if (field.Key.Equals("_dlc_BarcodePreview", StringComparison.OrdinalIgnoreCase))
                                        {
                                            tempUrlValue.Url = ReplaceBarcodeUrl(spListItem.ID, tempUrlValue.Url);
                                        }
                                    }
                                    else
                                    {
                                        tempUrlValue = new SPFieldUrlValue();
                                        tempUrlValue.Url = fieldValue.ColValue.ToString();
                                        fieldValue.ColValue = tempUrlValue;
                                        continue;
                                    }
                                }
                                else
                                {
                                    tempUrlValue = new SPFieldUrlValue();
                                    tempUrlValue.Url = fieldValue.ColValue.ToString();
                                    tempUrlValue.Description = fieldValue.ColValue.ToString();
                                }
                            }
                            spListItem[currentKey] = tempUrlValue;
                        }
                        else
                        {
                            spListItem[field.Key] = fieldValue.ColValue;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("An error occurred while updating the field '{0}' of value '{1}' for item '{2}', details: {3}", field.Key, field.Value, spListItem.Url, ex.ToString());
                    }
                }

            }

        }

        private void AddMultiLookupFields(SPListItem spListItem, AveBaseItemInfo info)
        {
            if (info.FieldsInfo.MultiLookupFields != null && info.FieldsInfo.MultiLookupFields.Count > 0)
            {
                foreach (string fieldName in info.FieldsInfo.MultiLookupFields.Keys)
                {
                    object tempValue = info.FieldsInfo.MultiLookupFields[fieldName];
                    if (tempValue is AveFieldLookupValueCollection)
                    {
                        spListItem[fieldName] = (tempValue as AveFieldLookupValueCollection).FieldLookupValues;
                    }
                    else if (tempValue is AveFieldUserValueCollection)
                    {
                        spListItem[fieldName] = (tempValue as AveFieldUserValueCollection).FieldUserValueCollection;
                    }
                    else
                    {
                        logger.Log(AveLogLevel.WARN, "The value is not multi lookup column value. FieldId: {0}, value type: {1}", fieldName, tempValue.GetType().Name);
                    }
                }
            }
        }

        private void AddTaxonomyFields(SPListItem spListItem, AveBaseItemInfo info)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.AddTaxonomyFields"))
            {

                if (info.FieldsInfo.TaxonomyFieldsInMapping != null && info.FieldsInfo.TaxonomyFieldsInMapping.Count > 0)
                {
                    IAveListItem item = info.AveItem.ListItem;
                    if (item != null)
                    {
                        AveTaxonomyFieldUtility.SetFieldValue(item, info.FieldsInfo.TaxonomyFieldsInMapping, info.IsForceAddTerm, -1, info.FieldsInfo.TermIdMapping, info.FieldsInfo.MergedTermIdMapping);
                        SetTaxCatchAllValue(item, item.ParentList.Fields);
                    }
                }

            }

        }

        public bool SetTaxCatchAllValue(IAveListItem item, IAveFieldCollection fields)
        {
            bool needUpdate = false;
            Dictionary<int, AveTaxonomyFieldValue> taxCatchAllValue = new Dictionary<int, AveTaxonomyFieldValue>();
            foreach (IAveField field in fields)
            {
                AveTaxonomyField taxField = field as AveTaxonomyField;
                if (taxField != null)
                {
                    object obj = item[taxField.ID];
                    if (obj == null)
                    {
                        continue;
                    }
                    if (obj is AveTaxonomyFieldValue)
                    {
                        AveTaxonomyFieldValue value = obj as AveTaxonomyFieldValue;
                        int num = value.WssId;
                        if (num > 0 && !taxCatchAllValue.ContainsKey(num))
                        {
                            taxCatchAllValue[num] = value;
                        }
                    }
                    else if (obj is AveTaxonomyFieldValueCollection)
                    {
                        foreach (AveTaxonomyFieldValue t in obj as AveTaxonomyFieldValueCollection)
                        {
                            AveTaxonomyFieldValue value = t as AveTaxonomyFieldValue;
                            int num = value.WssId;
                            if (num > 0 && !taxCatchAllValue.ContainsKey(num))
                            {
                                taxCatchAllValue[num] = value;
                            }
                        }
                    }
                }
            }
            if (taxCatchAllValue.Count > 0)
            {
                AveFieldLookupValueCollection lookupValue = new AveFieldLookupValueCollection();
                string taxCatchAllProperty = string.Empty;
                foreach (int num in taxCatchAllValue.Keys)
                {
                    AveFieldLookupValue temp1 = new AveFieldLookupValue(num, taxCatchAllValue[num].Label + "|" + taxCatchAllValue[num].TermGuid);
                    lookupValue.Add(temp1);
                    if (taxCatchAllProperty == string.Empty)
                    {
                        taxCatchAllProperty = taxCatchAllValue[num].WssId + ";#" + taxCatchAllValue[num].Label + "|" + taxCatchAllValue[num].TermGuid;
                    }
                    else
                    {
                        taxCatchAllProperty = taxCatchAllProperty + ";#" + taxCatchAllValue[num].WssId + ";#" + taxCatchAllValue[num].Label + "|" + taxCatchAllValue[num].TermGuid;
                    }
                }
                item["TaxCatchAll"] = lookupValue;
                if (item.ParentList.BaseType == AveBaseType.DocumentLibrary)
                {
                    item.Properties["TaxCatchAll"] = taxCatchAllProperty;
                }
                needUpdate = true;
            }
            return needUpdate;
        }

        public void SetDocData(string column, object value)
        {
            docdataObjects[column] = value;
        }
        public void SetUserData(string column, object value)
        {
            userdataObjects[column] = value;
        }

        /// <summary>
        /// info 对象中SiteId,ParentId,Level,UnVersionedMetaInfo,Name需要初始化
        /// 更新完之后会清空DocData缓存数据
        /// </summary>
        /// <param name="docData"></param>
        private void ChangeDocdataByNative()
        {
            if (this.docdataObjects.Count == 0)
            {
                return;
            }

            this.mSite.QueryService.ChangeDocdataByNative(info, this.info.GUID, this.docdataObjects);
            this.docdataObjects.Clear();
        }

        /// <summary>
        /// info 对象中SiteId,ParentId,Level,UnVersionedMetaInfo,Name需要初始化
        /// 更新完之后会清空UserData缓存数据
        /// </summary>
        /// <param name="docData"></param>
        private void ChangeUserdataByNative()
        {
            if (this.userdataObjects.Count == 0)
            {
                return;
            }

            this.mSite.QueryService.ChangeUserdataByNative(info, this.info.GUID, this.userdataObjects);
            this.userdataObjects.Clear();
        }

        /// <summary>
        /// 对Item相关属性进行Native方式更新
        /// 如果更新了AllUserData数据，请考虑是否需要reload
        /// </summary>
        /// <param name="changeDocData"></param>
        /// <param name="changeUserData"></param>
        /// <param name="reloadItem">是否需要ReloadItem，一般来说，更改AllUserData记录的时候需要，但是有的情况，更新完之后不在使用这个Item的时候，就不需要Reload</param>
        public void UpdateDataByNative(bool changeDocData, bool changeUserData, bool reloadItem = true)
        {
            if ((!changeDocData && !changeUserData)
                || (this.docdataObjects.Count == 0 && this.userdataObjects.Count == 0)
                || (changeDocData && this.docdataObjects.Count == 0 && !changeUserData)
                || (changeUserData && this.userdataObjects.Count == 0 && !changeDocData))
            {
                return;
            }

            if (this.HasFullControlPermission)
            {
                if (changeDocData)
                {
                    ChangeDocdataByNative();
                }
                if (changeUserData)
                {
                    ChangeUserdataByNative();
                }
                if (reloadItem)
                {
                    ReloadItem();
                }
            }
            else
            {
                //Output all properties
                StringBuilder builder = new StringBuilder("Not update all properties because of permission issue.");
                //由于Key为数据库表的Column Name，在Log中输出比较不好，暂时先注释
                //builder.AppendLine("");
                //foreach (var pro in docdataObjects)
                //{
                //    builder.AppendLine(string.Format("Key: {0}, Value: {1}", pro.Key, pro.Value == null ? "Null" : pro.Value.ToString()));
                //}
                //foreach (var pro in userdataObjects)
                //{
                //    builder.AppendLine(string.Format("Key: {0}, Value: {1}", pro.Key, pro.Value == null ? "Null" : pro.Value.ToString()));
                //}
                logger.Log(AveLogLevel.WARN, builder.ToString());
            }

        }

        //在开启content organizer的site中，添加Content Organizer Rules设置的条件可以是对metadata column，此时在conditions中有和termId相关的属性，需要替换成目的端的termId
        private string ReplaceTermIdInConditions(string conditions, Dictionary<Guid, Guid> termIdMapping)
        {
            if (termIdMapping == null)
            {
                return conditions;
            }
            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(conditions);
                XmlNodeList nodes = xDoc.GetElementsByTagName("Condition");
                bool changed = false;
                foreach (XmlElement n in nodes.OfType<XmlElement>())
                {
                    string originalValue = n.GetAttribute("Value");
                    string newValue = originalValue;
                    if (originalValue.Contains(";#") && originalValue.Contains("|"))
                    {
                        string[] tempArray = originalValue.Split(new string[] { ";#", "|" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string s in tempArray)
                        {
                            if (s.Length == 36)
                            {
                                try
                                {
                                    Guid id = new Guid(s);
                                    if (termIdMapping.ContainsKey(id))
                                    {
                                        newValue = newValue.Replace(id.ToString(), termIdMapping[id].ToString());
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Info("The value is not GUID format. Value: {0}, exception: {1}", s, ex.ToString());
                                }
                            }
                        }
                        if (!string.Equals(originalValue, newValue, StringComparison.OrdinalIgnoreCase))
                        {
                            n.SetAttribute("Value", newValue);
                            changed = true;
                        }
                    }
                }
                if (changed)
                {
                    conditions = xDoc.OuterXml;
                }
                return conditions;
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.INFO, "An exception occurred while load routing conditions, conditions: {0}, exception: {1}", conditions, ex.ToString());
                return conditions;
            }
        }

        private void SetFieldValueToNull(SPListItem spListItem, List<string> fields)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.SetFieldValueToNull"))
            {

                if (fields != null)
                {
                    try
                    {
                        foreach (string fieldName in fields)
                        {
                            if (spListItem[fieldName] != null)
                            {
                                spListItem[fieldName] = null;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn(ServerAPIResource.SetFieldValueNullError, e);
                    }
                }

            }

        }

        public void RestoreContentByNative(IAveRestoreStream receiver)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.RestoreContentByNative"))
            {

                using (AveSPFileStream fs = new AveSPFileStream(receiver))
                {
                    if (info.IsStubData)
                    {
                        mList.SOIntegrationUtil.UpdateSOFileStream(mSPFile, fs);
                        long bsn = mList.SOIntegrationUtil.QueryService.GetMaxRbs(mSPFile.Web.Site.ID, mSPFile.UniqueId);
                        if (bsn == -1)
                        {
                            throw new Exception("There is not a BSN");
                        }
                        mList.SOIntegrationUtil.UpdateStubSize((int)mSPFile.Level, mSPFile.ParentFolder.UniqueId, mSPFile.UniqueId, mSPFile.Web.Site.ID, (int)info.DocumentSize, bsn);
                    }
                    else
                    {
                        try
                        {
                            if (mSPList != null && EnableVersioning(mSPList))
                            {
                                mSPList.EnableVersioning = false;
                                mSPList.EnableMinorVersions = false;
                                info.SettingInfo.LIST_SETTING_CHANGED = true;
                                mSPList.Update();
                            }
                            mSPFile.SaveBinaryExtension(fs);
                        }
                        catch (Exception ex)
                        {
                            logger.Log(AveLogLevel.WARN, "An error occurred while restoring content. File Url: {0}. Error: {1}", mSPFile.Url, ex.ToString());
                        }
                    }
                }

            }

        }

        #endregion

        internal void CheckConflictState(RestoringDto restoringDto, Guid siteId, Guid parentId, Guid tp_Guid)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.CheckConflictState"))
            {

                //只给Replicator使用，因为Replicator之前有discover逻辑，不需要在有Verify的逻辑
                if (info.SettingInfo.NewItemWithOutVerifyConflict)
                {
                    restoringDto.ConflictType = ConflictType.None;
                    restoringDto.IsNewItem = true;
                    return;
                }//只给Replicator使用，因为Replicator知道目的端的RowId
                else if (info.SettingInfo.IncreaceVerionWithRowId)
                {
                    restoringDto.ConflictType = ConflictType.Document;
                    return;
                }

                mSite.QueryService.CheckConflictInfo(siteId, mList.ID, parentId, tp_Guid, restoringDto);
                SetItemStatusByConflictType(info);

            }

        }

        [SuppressMessage("Microsoft.Globalization", "CA1309:UseOrdinalStringCompariso", Justification = "请务必使用StringComparison.InvariantCultureIgnoreCase，因为对应的SQL Order By是语言相关的")]
        public void CheckConflictState(RestoringDto restoringDto, Guid siteId, Guid parentId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.CheckConflictState_1"))
            {

                //只给Replicator使用，因为Replicator之前有discover逻辑，不需要在有Verify的逻辑
                if (info.SettingInfo.NewItemWithOutVerifyConflict)
                {
                    restoringDto.IsNewItem = true;
                    return;
                }//只给Replicator使用，因为Replicator知道目的端的RowId
                else if (info.SettingInfo.IncreaceVerionWithRowId)
                {
                    restoringDto.ConflictType = ConflictType.Document;
                    return;
                }

                //请务必使用StringComparison.InvariantCultureIgnoreCase，因为对应的SQL Order By是语言相关的
                if (string.Compare(restoringDto.NameMapping, mAveParentFolder.MaxSubLeafName, StringComparison.InvariantCultureIgnoreCase) > 0)
                {//当前doc/folder leaf Name最大，不可能冲突，后续还原List Item比这个还大（源端排序），也不可能冲突，从而达到节省效率的目的。
                    restoringDto.IsNewItem = true;
                    mAveParentFolder.MaxSubLeafName = restoringDto.NameMapping;
                    restoringDto.ConflictType = ConflictType.None;
                    return;
                }
                mSite.QueryService.CheckConflictInfo(siteId, parentId, restoringDto);
                SetItemStatusByConflictType(info);

            }

        }

        /// <summary>
        /// check 冲突类型后的处理,Item,Document,Folder都走这一个方法
        /// </summary>
        /// <param name="baseItemInfo"></param>
        private static void SetItemStatusByConflictType(AveBaseItemInfo baseItemInfo)
        {
            //不冲突的情况IsNewItem置成True
            //只要不是skip的情况,recycle bin冲突都应该remove recycle bin data,然后IsNewItem置成True
            if (baseItemInfo.RestoringItem.ConflictType == ConflictType.None
                || (baseItemInfo.RestoringItem.ConflictType == ConflictType.RecycleBin && baseItemInfo.RestoreOption != AveRestoreMode.Default))
            {
                baseItemInfo.RestoringItem.IsNewItem = true;
            }
        }

        /// <summary>
        /// 当restore Discussion的reply时需要特殊的判断冲突，需要用parentId和messageId来判断，messageId的ColumnName可能不确定，需要一个参数表示
        /// </summary>
        /// <param name="sqlConn"></param>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="messageId"></param>
        /// <param name="fieldColumn"></param>
        internal void CheckConflictStateForDiscussionReply(RestoringDto restoringDto, Guid siteId, Guid parentId, string messageId, string fieldColumn)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.CheckConflictStateForDiscussionReply"))
            {

                mSite.QueryService.CheckConflictInfoBySpecialColumn(siteId, parentId, messageId, fieldColumn, restoringDto);
                SetItemStatusByConflictType(info);

            }

        }

        internal void CheckConflictStateForAccessRequest(RestoringDto restoringDto, int requestedByUserId, string fullUrl)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.CheckConflictStateForAccessRequest"))
            {

                SPQuery query = new SPQuery();
                string serverRelativeUrl = mWeb.Web.GetServerRelativeUrlFromUrl(fullUrl);
                //查询条件：在AccessRequestList中Status为Pending状态，请求者与原端一致，所请求的内容与原端Url一致的Item
                query.Query = "<Where><And><And><Eq><FieldRef Name='Status' /><Value Type='Integer'>0</Value></Eq><Eq><FieldRef Name='RequestedByUserId' /><Value Type='Integer'>" + requestedByUserId + "</Value></Eq></And><Eq><FieldRef Name='RequestedObjectUrl' /><Value Type='Url'>" + serverRelativeUrl + "</Value></Eq></And></Where>";
                SPListItemCollection items = this.mList.List.GetItems(query);
                try
                {
                    if (items != null && items.Count > 0)
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
                catch (Exception e)
                {
                    logger.Warn("An error occurred while restoring a ListItem in Access Request list. Error: {0}", e.ToString());
                }

            }

        }

        internal void CheckConflictStateForCommunityMember(RestoringDto restoringDto, Guid siteId, Guid parentId, int memberId, string fieldColumn)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.CheckConflictStateForCommunityContributor"))
            {

                mSite.QueryService.CheckConflictInfoBySpecialColumn(siteId, parentId, memberId, fieldColumn, restoringDto);
                SetItemStatusByConflictType(info);

            }

        }

        internal void CheckConflictStateForComposedLooksItems(AveListItemInfo itemInfo, Guid siteId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.CheckConflictStateForListItem"))
            {

                RestoringDto restoringDto = itemInfo.RestoringItem;
                Dictionary<string, object> userData = itemInfo.UserData;
                if (userData.ContainsKey("Title"))
                {
                    string title = userData["Title"].ToString();

                    mSite.QueryService.CheckConflictInfoForListItem(siteId, mList.Id, title, restoringDto);
                    SetItemStatusByConflictType(info);
                }

            }

        }
        [SuppressMessage("Microsoft.Globalization", "CA1309:UseOrdinalStringCompariso", Justification = "请务必使用StringComparison.InvariantCultureIgnoreCase，因为对应的SQL Order By是语言相关的")]
        internal void CheckConflictStateForListItem(RestoringDto restoringDto, Guid siteId)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.CheckConflictStateForListItem"))
            {

                if (restoringDto.NameMapping.Contains("_.000"))
                {
                    int rowId = 0;
                    string strRowId = restoringDto.NameMapping.Substring(0, restoringDto.NameMapping.IndexOf("_.000", StringComparison.OrdinalIgnoreCase));
                    if (int.TryParse(strRowId, out rowId))
                    {
                        //当前Item rowId最大，不可能冲突，后续还原List Item比这个还大（源端排序），也不可能冲突，从而达到节省效率的目的。
                        if (rowId > mList.MaxListItemRowId)
                        {
                            restoringDto.IsNewItem = true;
                            mList.MaxListItemRowId = rowId;
                            restoringDto.ConflictType = ConflictType.None;
                            return;
                        }
                    }
                }

                mSite.QueryService.CheckConflictInfoForListItem(siteId, mList.Id, restoringDto);
                SetItemStatusByConflictType(info);

            }

        }
        internal void CheckConflictStateForVariationLabels(RestoringDto restoringDto, string labelName, bool isSource)
        {
            var query = new SPQuery()
            {
                Query = string.Format("<Where><Eq><FieldRef Name='Title' /><Value Type='Text'>{0}</Value></Eq></Where>", labelName),
            };
            var items = this.mList.List.GetItems(query);
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

        private static bool IsSourceLabel(SPListItem item)
        {
            bool isSource = false;
            try
            {
                isSource = (bool)item["Is_x0020_Source"];
            }
            catch (ArgumentException e)
            {
                logger.Log(AveLogLevel.DEBUG, "An error occourred while getting item property IsSourceLabel. {0}", e);
            }
            return isSource;
        }

        internal void CheckConflictStateForRelationshipsList(RestoringDto restoringDto, string objectID)
        {
            var query = new SPQuery()
            {
                Query = string.Format("<Where><Eq><FieldRef Name='ObjectID' /><Value Type='URL'>{0}</Value></Eq></Where>", objectID),
            };
            var items = this.mList.List.GetItems(query);
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
        internal void CheckConflictByFieldValue(AveListItemInfo info, string fieldInternalName, string fieldType, object fieldValue)
        {
            int itemRowId = GetItemRowIdByFieldValue(fieldInternalName, fieldType, fieldValue);
            if (itemRowId > -1)
            {
                info.RowId = itemRowId;
                info.RestoringItem.ConflictType = ConflictType.Document;
            }
        }

        internal int GetItemRowIdByFieldValue(string fieldInternalName, string fieldType, object fieldValue)
        {
            string fieldValueStr = fieldValue.ToString();
            AveFieldValueInfo fieldValueInfo = fieldValue as AveFieldValueInfo;
            if (fieldValueInfo != null)
            {
                fieldValueStr = fieldValueInfo.ColValue.ToString();
            }
            AveQuery query = new AveQuery();
            query.Query = string.Format("<Where><Eq><FieldRef Name='{0}'/><Value Type='{1}'>{2}</Value></Eq></Where>", fieldInternalName, fieldType, fieldValueStr);
            query.ViewFields = "<FieldRef Name='ID'/>";
            int itemRowId = -1;
            var items = mList.GetItems(query);
            if (items != null && items.Count > 0)
            {
                itemRowId = items[0].ID;
            }
            return itemRowId;
        }

        //private void SetConflictInfo(RestoringDto restoringDto, SqlDataReader dr)
        //{
        //    if (!dr.IsDBNull(1) && !dr.IsDBNull(2))
        //    {
        //        int rowId = dr.GetInt32(1);
        //        int level = dr.GetByte(2);
        //        int uiVersion = dr.GetInt32(3);
        //        if (level == 1)
        //        {
        //            restoringDto.PublishingUIVersion = uiVersion;
        //        }
        //        else if (level == 2)
        //        {
        //            restoringDto.DraftUIVersion = uiVersion;
        //        }
        //    }
        //    else
        //    {
        //        restoringDto.PublishingUIVersion = dr.GetInt32(3);
        //    }
        //}

        internal SPFolder GetFolder(SPFolder parentFolder, AveBaseItemInfo info)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.GetFolder"))
            {

                SPFolder folder = null;
                Guid mId = Guid.Empty;
                try
                {
                    if (info.Name.Equals("{System Folder}"))
                    {
                        folder = parentFolder;
                        mId = folder.UniqueId;
                    }
                    else
                    {
                        //system folder的parentfolder的ServerRelativeUrl后面有 "/"
                        folder = parentFolder.ParentWeb.GetFolder(SPResourcePath.FromDecodedUrl(parentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + info.Name));
                        //folder = parentFolder.SubFolders[info.Name];
                        mId = folder.UniqueId;
                    }
                }
                //  catch(ArgumentException)
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetFolderError, e.ToString());
                    mId = mSite.QueryService.GetFolderIdByName(info);
                    if (mId != Guid.Empty)
                    {
                        try
                        {
                            folder = (mWeb as AveWeb).Web.GetFolder(mId);
                        }
                        catch (Exception ex)
                        {
                            logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetFolderError, ex.ToString());
                            //no exist
                        }
                    }
                }
                if (info.ItemType == AveItemType.Folder)
                {
                    info.GUID = mId;
                }
                else
                {
                    info.ParentId = mId;
                }
                if (folder == null || !folder.Exists)
                {
                    return null;
                }
                return folder;

            }

        }

        public bool OverwriteByModifiedTime(AveBaseItemInfo info, object objSourceDateTime, object objLevel)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.OverwriteByModifiedTime"))
            {

                if (info == null)
                {
                    return true;
                }
                if (!info.SettingInfo.OverWriteByModifiedTime)
                {
                    return true;
                }
                if (objSourceDateTime != null)
                {
                    DateTime sourceDateTime;
                    if (objSourceDateTime is DateTime)
                    {
                        sourceDateTime = (DateTime)objSourceDateTime;
                    }
                    else
                    {
                        if (!DateTime.TryParse(objSourceDateTime.ToString(), out sourceDateTime))
                        {
                            return true;
                        }
                    }
                    //ado-38584 如果listitem与冲突的listitem的parent folder所在的层次结构不一样，则parentid需要在checkconflict时查出。
                    Guid parentId = Guid.Empty;
                    if (info.RestoringItem != null && info.RestoringItem.ConflictItemParentFolerGuid != Guid.Empty)
                    {
                        parentId = info.RestoringItem.ConflictItemParentFolerGuid;
                    }
                    else
                    {
                        parentId = info.ParentId;
                    }
                    DateTime destDateTime = mSite.QueryService.GetLastModifiedByNative(info.SiteId, info.ListId, info.RestoringItem.ConflictRowId, false);
                    if (sourceDateTime <= destDateTime)
                    {
                        if (objLevel != null)
                        {
                            if (sourceDateTime == destDateTime && objLevel.ToString() == "255" && !info.IsCheckOut)
                            {
                                return true;//ADO-17063
                            }
                        }
                        return false;
                    }
                }
                //DateTime sourceDateTime;
                //DateTime destDateTime = mSite.QueryService.GetLastModified(info.SiteId, info.ParentId, info.RestoringItem.ConflictRowId);
                //if (DateTime.TryParse(objSourceDateTime.ToString(), out sourceDateTime) &&
                //    sourceDateTime <= destDateTime)
                //{
                //    if (objLevel != null)
                //    {
                //        if (sourceDateTime == destDateTime && objLevel.ToString() == "255" && !info.IsCheckOut)
                //        {
                //            return true;//ADO-17063
                //        }
                //    }
                //    return false;
                //}
                return true;

            }

        }

        public bool SkipIfSameModifiedTime(AveBaseItemInfo info, object objSourceDateTime)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.SkipIfSameModifiedTime"))
            {

                if (info == null)
                {
                    return false;
                }
                if (!info.SettingInfo.SKIP_IF_SAME_MODIFIEDTIME || info.SettingInfo.DELETE_ITEM)
                {
                    return false;
                }
                if (objSourceDateTime != null)
                {
                    DateTime sourceDateTime;
                    if (objSourceDateTime is DateTime)
                    {
                        sourceDateTime = (DateTime)objSourceDateTime;
                    }
                    else
                    {
                        if (!DateTime.TryParse(objSourceDateTime.ToString(), out sourceDateTime))
                        {
                            return false;
                        }
                    }
                    if (sourceDateTime == mSite.QueryService.GetVersionModified(info.SiteId, info.ParentId, info.RestoringItem.ConflictRowId, info.OriginalVersion))
                    {
                        return true;
                    }
                }
                return false;

            }

        }

        #region IAveItem Members

        /// <summary>
        /// 当调用AveItem(IAveSite site)构造函数时，Web 值返回null，需要调用Web的set方法
        /// </summary>
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

        public IAveFile File
        {
            get
            {
                if (mFile == null)
                {
                    if (mSPFile != null)
                    {
                        mFile = new AveFile(mWeb, mSPFile);
                    }
                }
                return mFile;
            }
            set
            {
                //调用set方法，File属性返回值不发生改变。Set方法的作用需要考虑
                if (value != null)
                {
                    mFile = value as AveFile;
                    mSPFile = mFile.File;
                }
                else
                {
                    mSPFile = null;
                }
            }
        }

        public IAveListItem ListItem
        {
            get
            {
                if (mListItem == null)
                {
                    if (mSPListItem != null)
                    {
                        mListItem = new AveListItem(mList, mSPListItem);
                    }
                    else
                    {
                        try
                        {
                            if (mList != null && info.RowId > 0)
                            {
                                mListItem = mList.GetItemById(info.RowId) as AveListItem;
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetListIemFaild, e.ToString());
                            try
                            {
                                mListItem = AveSPUtility.LoadCheckOutFile(Web, info.GUID, GetCheckOutUserId(info), AveObjectModelFactory.CreateObjectModelFactory(null, null)).Item as AveListItem;
                            }
                            catch (Exception ex)
                            {
                                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetListIemFaild, ex.ToString());
                            }
                        }
                    }
                }
                return mListItem;
            }
            set
            {
                mListItem = value as AveListItem;
                if (mListItem != null)
                {
                    mSPListItem = mListItem.ListItem;
                }
                else
                {
                    mSPListItem = null;
                }
            }
        }

        public int ChangeItemId(Guid siteId, Guid id, Guid rootFolderId, int itemType, int fromId, int toId)
        {
            return mSite.QueryService.ChangeItemId(siteId, id, rootFolderId, itemType, fromId, toId);
        }

        public void UpdateAllDocsPropertyByNative(AveBaseItemInfo mBaseItemInfo, DateTime timeCreated, DateTime timeLastModified, int version)
        {
            mSite.QueryService.UpdateAllDocsPropertyByNative(mBaseItemInfo, timeCreated, timeLastModified, version);
        }

        public bool CreateVersionByNative(AveBaseItemInfo mBaseItemInfo, int version, RestoringDto restoringDto)
        {
            return mSite.QueryService.CreateVersionByNative(info, version, restoringDto);
        }

        public IAveFile GetFile(string name)
        {
            string folderPath = mParentFolder.ServerRelativeUrl.TrimEnd('/') + "/" + name;
            return mWeb.GetFile(folderPath);
        }

        public IAveFile GetFile()
        {
            return mWeb.GetFile(this.info.GUID);
        }

        public void InitBySPListItem(IAveListItem listItem)
        {
            InitBySPListItem((listItem as AveListItem).ListItem);
        }

        public Dictionary<string, object> ConvertToFieldWithNativeName(Dictionary<string, object> dic)
        {
            Dictionary<string, object> fields = new Dictionary<string, object>();
            foreach (string key in dic.Keys)
            {
                if (dic[key] is AveFieldValueInfo)
                {
                    AveFieldValueInfo field = (AveFieldValueInfo)dic[key];
                    if (field != null && !string.IsNullOrEmpty(field.ColName))
                    {
                        if (field.ColName.StartsWith("tp_", StringComparison.OrdinalIgnoreCase))
                        {
                            fields.Add(field.ColName, field.ColValue);
                        }
                        else
                        {
                            KeyValuePair<byte, object> rowValue = new KeyValuePair<byte, object>((byte)field.RowOrdinal, field.ColValue);
                            fields.Add(field.ColName + "#" + field.RowOrdinal, rowValue);
                        }
                    }
                }
            }
            return fields;
        }

        public IAveFolder Folder
        {
            get
            {
                if (mFolder == null)
                {
                    if (mListItem == null)
                    {
                        return null;
                    }
                    return mListItem.Folder;
                }
                else
                {
                    return mFolder;
                }
            }
            set
            {
                if (value != null)
                {
                    if (value.Item == null)
                    {
                        //system folder
                        mFolder = value as AveFolder;
                    }
                    else
                    {
                        mListItem = value.Item as AveListItem;
                        mSPListItem = mListItem.ListItem;
                    }
                }
                else
                {
                    mFolder = null;
                }
            }
        }


        public void ReloadFile()
        {
            ReloadFile(false);
        }

        public void ReloadFile(bool fakeDeletedUser)
        {
            var checkoutFile = LoadCheckoutFile(mWeb.Web, mSPFile, fakeDeletedUser, mSPFile.ServerRelativeUrl);
            if (checkoutFile != null)
            {
                mSPFile = checkoutFile;
                mFile = new AveFile(mWeb, mSPFile);
            }
            else
            {
                mSPFile = (mWeb as AveWeb).Web.GetFile(mSPFile.UniqueId);
                if (mSPFile != null)
                {
                    mFile = new AveFile(mWeb, mSPFile);
                }
            }
        }

        /// <summary>
        /// 当前user对list没有权限的时候，可以取到SPFile，但是不可以对其进行操作。Exists属性是false，可以取到UniqueId、parent的id和自己的serverRelativeUrl。
        /// </summary>
        /// <param name="spWeb"></param>
        /// <param name="spFile"></param>
        /// <param name="fakeDeletedUser"></param>
        /// <returns></returns>
        private SPFile LoadCheckoutFile(SPWeb spWeb, SPFile spFile, bool fakeDeletedUser, string fileServerRelativeUrl)
        {
            int userId = -1;
            bool isCheckOutFile = spFile.Exists ? mSite.QueryService.IsCheckOutFile(mSite.ID, spFile.UniqueId, ref userId) : mSite.QueryService.IsCheckOutFile(mSite.ID, fileServerRelativeUrl, ref userId);
            if (isCheckOutFile && userId != mWeb.CurrentUser.ID)
            {
                SPUser user = null;
                SPFile file = null;
                try
                {
                    user = spWeb.SiteUsers.GetByID(userId);
                    SPList spList = null;
                    try
                    {
                        spList = spWeb.Lists[spFile.ParentFolder.ParentListId];
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Failed to get parent list of file {0}. Exception: {1}.", spFile.ServerRelativeUrl, ex);
                    }
                    file = (Web.Site as AveSite).GetCheckoutWeb(Web.Site.ID, spWeb, spList, user, spFile.UniqueId, false).GetFile(spFile.UniqueId);
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetFileFaild, e.ToString());
                    if (fakeDeletedUser)
                    { //file checkout user doesn't exist in the site users list.
                        mSite.QueryService.ChangeCheckoutUserID(mSite.ID, spFile.UniqueId, spWeb.CurrentUser.ID);
                        file = spWeb.GetFile(spFile.UniqueId);
                        mSite.CheckOutFileId = spFile.UniqueId;
                        mSite.CheckOutUser = userId;
                    }
                }
                return file;
            }
            return null;
        }

        /// <summary>
        /// load item level=255即checkout item，例如survey listitem；
        /// </summary>
        /// <param name="web"></param>
        /// <param name="list"></param>
        /// <param name="rowId"></param>
        /// <returns></returns>
        public SPListItem LoadCheckoutListItem(SPWeb web, SPList list, int rowId)
        {
            int userId = -1;
            Guid guid = Guid.Empty;
            if (mSite.QueryService.IsCheckOutFile(mSite.ID, mList.Id, rowId, out userId, out guid) && userId != mWeb.CurrentUser.ID)
            {
                SPUser user = null;
                SPListItem item = null;
                try
                {
                    user = web.SiteUsers.GetByID(userId);
                    item = mSite.GetCheckoutWeb(mSite.ID, web, list, user, guid, false).GetList(mList.List.DefaultViewUrl).GetItemById(info.RowId);
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetFileFaild, e.ToString());
                }
                return item;
            }
            return null;
        }

        public IAveFile ReloadFile(Guid fileID)
        {
            return Folder.ParentList.ParentWeb.GetFile(fileID);
        }

        /// <summary>
        /// Miguration缩略图的问题，07sharepoint缩略图只有512一个version，还原到10环境，缩略图无法checkout，checkin产生version
        /// </summary>
        public void RestoreMigStubThumbnails(IAveRestoreStream receiver, SPWeb web, SPFolder folder,
            string fileName, bool isCheckOut)
        {
            SPFile file = null;
            //web下的RootFolder.ServerRelativeUrl结尾包括了"/"
            string folderServerRelativeUrl = folder.ServerRelativeUrl;

            file = LoadCheckOutFile(web, folder, fileName);

            if (file.Exists)
            {
                InitBySPFile(file);
                info.InternalVersion = file.UIVersion;
                info.RestoreVersion = file.UIVersion;
                RestoreContentByNative(receiver);
            }
        }

        public void SetAttachmentInfo(Guid id, int internalVersion)
        {
            info.Version = 512;
            info.GUID = id;
            info.Level = 1;
            info.InternalVersion = internalVersion;
        }

        public void AddFields(IAveListItem spListItem, Dictionary<string, object> fieldMap, AveBaseItemInfo info)
        {
            AddFields((spListItem as AveListItem).ListItem, fieldMap, info);
        }

        public void InsertIntoAllUserDatajunction(IAveListItem item, Guid fieldId, Guid sourceListId, int id, int ordinal, int version)
        {
            mSite.QueryService.InsertIntoAllUserDataJunction(item, fieldId, sourceListId, id, ordinal, version);
        }

        public void UpdateColumnByNative(Guid siteId, IAveListItem item, int version, int rowOrdinal, string colName, object colValue)
        {
            mSite.QueryService.UpdateColumnByNative(siteId, item, version, rowOrdinal, colName, colValue);
        }

        [Obsolete("Please use  GetCurrentUIVersion(Guid siteId, Guid parentId, IAveListItem item) for proformance")]
        public int GetCurrentUIVersion(Guid siteId, IAveListItem item)
        {
            return mSite.QueryService.GetCurrentUIVersion(mSite.ID, item.UniqueId);
        }

        public int GetCurrentUIVersion(Guid siteId, Guid parentId, IAveListItem item)
        {
            return mSite.QueryService.GetCurrentUIVersion(mSite.ID, parentId, item.UniqueId);
        }

        public Dictionary<string, object> GetItemCurrentVersionDocData(AveBaseItemInfo baseItemInfo)
        {
            var dataCache = this.mAveParentFolder.GetCurrentVersionDocDataFromCache(baseItemInfo) ?? new Dictionary<string, object>();
            if (dataCache.Count == 0)
            {
                dataCache = mSite.QueryService.GetCurrentVersionDocInfo(mSite.ID, baseItemInfo.ParentId, baseItemInfo.GUID);
                //TODOREMOVE
#if DEBUG
                if (mAveParentFolder.EnableCache &&
                                    (baseItemInfo.ItemType == AveItemType.Document || baseItemInfo.ItemType == AveItemType.ListItem) && baseItemInfo.RowId > 0 &&
                                    dataCache.Count > 0)
                {
                    throw new Exception("DataCache failed");
                }
#endif
            }
            return dataCache;

        }

        public IAveView View
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public List<AveRoleAssignmentInfo> GetItemRoleAssignments(Guid siteId, Guid scopeId)
        {
            return mSite.QueryService.GetObjectRoleAssignments(siteId, scopeId);
        }


        public bool IsCheckOutFile(Guid siteId, Guid listId, int fileId, out int checkId, out Guid id)
        {
            return mSite.QueryService.IsCheckOutFile(siteId, listId, fileId, out checkId, out id);
        }

        public void ChangeCheckoutUserID(Guid siteId, Guid uniqueID, int newUserID)
        {
            mSite.QueryService.ChangeCheckoutUserID(siteId, uniqueID, newUserID);
        }

        public bool MoveToConflictFolder(IAveList parentList, IAveFolder parentFolder, IAveListItem listItem, bool isSourceWin)
        {
            return MoveToConflictFolder((parentList as AveList).List, (parentFolder as AveFolder).Folder, (listItem as AveListItem).ListItem, isSourceWin);

        }

        public AveSOIntegrationUtility GetSOIntegrationUtilForRestore(IAveRestoreStream receiver)
        {
            if (mList == null)
            {
                return null;
            }
            AveMetadata metadata = null != receiver ? receiver.TryReadMetadata(AveMetadataType.DocStorageInfo) : null;
            mStorageInfo = null != metadata ? metadata.GetMetadata<AveStorageInfo13>() : new AveStorageInfo13();
            mList.SOIntegrationUtil.StorageInfo = mStorageInfo;
            return mList.SOIntegrationUtil;
        }

        private string ReplaceBarcodeUrl(int itemID, string Url)
        {
            string pattern = @"doc=[A-Fa-f0-9]{8}(-[A-Fa-f0-9]{4}){3}-[A-Fa-f0-9]{12}";
            string replace = string.Format("ID={0}", itemID.ToString());
            if (Regex.IsMatch(Url, pattern, RegexOptions.IgnoreCase))
            {
                Url = Regex.Replace(Url, pattern, replace);
            }
            return Url;
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {

        }

        #endregion

        internal void RestoreStubDBInfo()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.RestoreStubDBInfo"))
            {

                try
                {

                    foreach (var info in mList.SOIntegrationUtil.StorageInfo.ShredStubInfoList)
                    {
                        if (!string.IsNullOrEmpty(info.StubDBInfoBase64s))
                        {
                            try
                            {
                                mList.SOIntegrationUtil.RestoreStubDBInfo(info.StubDBInfoBase64s);
                            }
                            catch (Exception e)
                            {
                                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.RestoreDBinfoError, e.ToString());
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.RestoreDBinfoError, e.ToString());
                }

            }

        }

        internal void RestoreItemConnectorInfo(IAveRestoreStream receiver, AveDocumentInfo docInfo, int uiVersion, int result)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.RestoreItemConnectorInfo"))
            {

                try
                {
                    //LinkFile不需要再转换成stub.
                    if (!docInfo.IsLinkFile && mList.IsConnectorList == true)
                    {
                        mList.SOIntegrationUtil.RestoreConnectorItem(ListItem, uiVersion, result, info.RestoringItem.OverWriteBlob);
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.RestoreConninfoError, e.ToString());
                }

            }

        }

        internal void RestoreFolderConnectorInfo()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.RestoreFolderConnectorInfo"))
            {

                try
                {
                    if (mList.IsConnectorList == true)
                    {
                        GetSOIntegrationUtilForRestore(null);
                        mList.SOIntegrationUtil.RestoreConnectorItem(Folder.UniqueId, info.RestoringItem.OverWriteBlob);
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.RestoreFolderConninfoError, e.ToString());
                }

            }

        }

        internal bool IsItemHasAlerts(SPListItem item)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.IsItemHasAlerts"))
            {

                if (this.mSPList == null || item == null || this.mSPList.ID == Guid.Empty)
                {
                    return false;
                }
                return mSite.QueryService.ItemHasAlerts(mSite.ID, this.mSPList.ID, item.ID);

            }

        }

        internal void RestoreConnectorStub(Guid newItemId, int uiVersion, int result)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveItem.RestoreConnectorStubInfo"))
            {

                if (mList.IsConnectorList == true)
                {
                    mList.SOIntegrationUtil.RestoreConnectorStub(newItemId, result, uiVersion);
                }

            }

        }

        internal void UpdateAlldocsPropertiesByNative(AveBaseItemInfo info, AveItem aveItem)
        {
            //ADO-165256 default value can not update to cotnent database.
            if (info.DTimeCreated != default(DateTime))
            {
                aveItem.SetDocData("TimeCreated", info.DTimeCreated);
            }
            if (info.DTimeLastModified != default(DateTime))
            {
                aveItem.SetDocData("TimeLastModified", info.DTimeLastModified);
            }

            if (info.UnVersionedMetaInfo == null)
            {
                aveItem.SetDocData("UnVersionedMetaInfo", DBNull.Value);
                aveItem.SetDocData("UnVersionedMetaInfoSize", DBNull.Value);
                aveItem.SetDocData("UnVersionedMetaInfoVersion", DBNull.Value);
            }
            else
            {
                aveItem.SetDocData("UnVersionedMetaInfo", info.UnVersionedMetaInfo);
                aveItem.SetDocData("UnVersionedMetaInfoSize", info.UnVersionedMetaInfo.LongLength);
                aveItem.SetDocData("UnVersionedMetaInfoVersion", info.UnVersionedMetaInfoVersion);
            }
        }
    }
}