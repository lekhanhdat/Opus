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
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPFolder : IAveEnableCache
    {
        private Guid mId;
        private int mVersion;
        private string mPath;
        private AveSPList mAveList;
        private AveSPFolder mParentFolder;
        private IAveBackupStream mSender;
        private IAveBackupRestoreQueryService mQueryService;
        private AveSPSite mAveParentSite;
        private string mServerRelativeUrl;
        private IAveFolder mSPFolder;
        private DateTime mTimeLastModified;

        public IAveFolder SPFolder
        {
            get { return mSPFolder; }
            //set; //{ mSPFolder = value; }
        }

        public AveSPSite ParentSite
        {
            get { return mAveParentSite; }
        }

        private AveSPItem mAveSPItem;

        public IAveBackupStream Sender
        {
            get { return mSender; }
        }

        public IAveBackupRestoreQueryService QueryService
        {
            get { return mQueryService; }
        }

        public AveSPFolder AveParentFolder
        {
            get { return mParentFolder; }
        }

        public AveSPList AveList
        {
            get { return mAveList; }
        }

        public Guid Id
        {
            get { return mId; }
        }

        public string Path
        {
            get { return mPath; }
        }

        public AveSPItem AveItem
        {
            get { return mAveSPItem; }
        }

        public bool IsVersion
        {
            get { return mAveSPItem.IsVersion; }
        }

        public string TagUrl
        {
            get
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFolder.TagUrl"))
                {
                    string tagUrl = string.Empty;//add sp1 estimate
                    if ((mQueryService != null && (mQueryService as IAveConnectorQueryService).IsSP2010SP1(mAveParentSite.SPSite.ID))
                     || (mAveList.SPList != null && mAveList.SPList.BaseType == AveBaseType.DocumentLibrary))
                    {
                        int root = this.ParentSite.SPSite.Url.IndexOf("/", 8, StringComparison.OrdinalIgnoreCase);
                        if (root > 0)
                        {
                            tagUrl = this.ParentSite.SPSite.Url.Substring(0, root + 1) + this.AveItem.ScopeUrl;
                        }
                        else
                        {
                            tagUrl = this.ParentSite.SPSite.Url + "/" + this.AveItem.ScopeUrl;
                        }
                    }
                    else
                    {
                        if (mAveList.SPList != null && !string.IsNullOrEmpty(mAveList.SPList.DefaultDisplayFormUrl))
                        {
                            string fileUrl = mAveList.SPList.DefaultDisplayFormUrl.TrimStart('/').Substring(mAveList.ParentWeb.SPWeb.ServerRelativeUrl.TrimStart('/').Length).TrimStart('/');
                            tagUrl = mAveList.ParentWeb.SPWeb.Url.TrimEnd('/') + "/" + fileUrl + "?ID=" + mAveSPItem.RowId;
                        }
                    }
                    return tagUrl;
                }
            }
        }

        public AveSPFolder(AveSPList aveList)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFolder.Constructor"))
            {
                mAveList = aveList;
                mSender = aveList.Sender;
                mPath = aveList.Path;
                mQueryService = aveList.QueryService;
                mAveParentSite = aveList.ParentWeb.ParentSite;
                //mId = aveList.SPList.RootFolder.UniqueId;
                mVersion = 512;

                if (mAveList.SPList != null)
                {
                    mSPFolder = mAveList.SPList.RootFolder;
                }
                else if (mAveList.ParentWeb.SPWeb != null)
                {
                    mSPFolder = mAveList.ParentWeb.SPWeb.RootFolder;
                }

                if (mSPFolder != null)
                {
                    mServerRelativeUrl = mSPFolder.ServerRelativeUrl;
                    mId = mSPFolder.UniqueId;
                }
                Init(-1, aveList.ScopeId, null);
            }
        }

        public AveSPFolder(AveSPFolder aveFolder, string name, Guid id, int rowId, int version)
            : this(aveFolder, name, id, rowId, version, null)
        { }

        public AveSPFolder(AveSPFolder aveFolder, string name, Guid id, int rowId, int version, IAveListItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFolder.Constructor"))
            {
                mAveList = aveFolder.AveList;
                mSender = mAveList.Sender;
                mParentFolder = aveFolder;
                mId = id;
                mVersion = version;
                mQueryService = aveFolder.QueryService;
                mPath = aveFolder.Path + "\\" + name;
                mServerRelativeUrl = aveFolder.ServerRelativeUrl.TrimEnd('/') + "/" + name;
                mAveParentSite = aveFolder.ParentSite;
                Init(rowId, Guid.Empty, item);

                if (mAveList.ParentSite.SPContextKind == AveContextKind.ClientObjectModel)
                {
                    if (mAveSPItem.SPListItem == null)
                    {
                        mSPFolder = null;
                    }
                    else if (mAveList.SPList != null)
                    {
                        string urlTag = mAveList.ServerRelativeUrl.Substring(0, mAveList.ServerRelativeUrl.Length - mAveList.SPList.RootFolder.Url.Length);
                        string folderServerRelativeUrl = (urlTag.Equals("/") ? "" : '/' + urlTag.Trim('/')) + "/" + this.mAveSPItem.SPListItem.Url.Trim('/');
                        mSPFolder = mAveList.SPList.GetFolder(folderServerRelativeUrl);
                    }
                    if (mSPFolder == null)
                    {
                        if (mAveList.SPList != null)
                        {
                            mSPFolder = mAveList.SPList.GetFolder(mServerRelativeUrl);
                        }
                        else
                        {
                            mSPFolder = mAveList.ParentWeb.SPWeb.GetFolder(mServerRelativeUrl);
                        }
                    }
                }
                else
                {
                    mSPFolder = mAveList.ParentWeb.SPWeb.GetFolder(mId);
                }
            }
        }

        private void Init(int rowId, Guid ScopeId, IAveListItem item)
        {
            IAveFolder folder = null;
            if (mParentFolder != null)
            {
                folder = mParentFolder.SPFolder;
            }
            else if (mSPFolder != null && mSPFolder.Exists)
            {
                folder = mSPFolder.ParentFolder;
            }
            mAveSPItem = new AveSPItem(mId, rowId, mVersion, mServerRelativeUrl, AveItemType.Folder, folder == null ? Guid.Empty : folder.UniqueId,
                mAveList.ParentWeb.ParentSite.SPSite.ID, mAveList,
                mSender, mQueryService, mAveList.Fields, mAveList.SolutionStatus, item, folder);
            if (!ScopeId.Equals(Guid.Empty))
            {
                mAveSPItem.ScopeId = ScopeId;
            }
            //if (rowId != -1)
            //{
            //    mAveSPItem.ParentId = mParentFolder.Id;
            //}
        }

        public string ServerRelativeUrl
        {
            get { return mServerRelativeUrl; }
        }

        public void SetItemProperty(AveSPItem aveSPItem, Dictionary<string, object> docInfoDic)
        {
            aveSPItem.IsVersion = false;

            if (docInfoDic.ContainsKey("Level"))
            {
                aveSPItem.Level = (byte)docInfoDic["Level"];//int 类型为什么要转化成Byte
            }

            if (docInfoDic.ContainsKey("ScopeId"))
            {
                aveSPItem.ScopeId = (Guid)docInfoDic["ScopeId"];
            }

            if (docInfoDic.ContainsKey("DirName") && docInfoDic.ContainsKey("LeafName"))
            {
                aveSPItem.ScopeUrl = (string)docInfoDic["DirName"] + "/" + (string)docInfoDic["LeafName"];
                aveSPItem.ScopeUrl = aveSPItem.ScopeUrl.TrimStart('/');
            }

            if (docInfoDic.ContainsKey("DocFlags"))
            {
                aveSPItem.DocFlag = (int)docInfoDic["DocFlags"];
            }
            if (docInfoDic.ContainsKey("HasStream"))
            {
                aveSPItem.HasStream = (Convert.ToInt32(docInfoDic["HasStream"]) == 1);
            }

            aveSPItem.IsVersion = true;
            aveSPItem.Level = (byte)docInfoDic["Level"];
            if (docInfoDic.ContainsKey("Size"))
            {
                aveSPItem.HasStream = ((int)docInfoDic["Size"] > 0);
            }
            docInfoDic["DoclibRowId"] = aveSPItem.RowId;

            if (docInfoDic.ContainsKey("InternalVersion"))
            {
                aveSPItem.InternalVersion = (int)docInfoDic["InternalVersion"];
            }
        }

        public void SetFolderLastModified(DateTime lastModifiedTime)
        {
            this.mTimeLastModified = lastModifiedTime;
        }

        public void CachePrincipalFromMetadata()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFolder.CachePrincipalFromMetadata"))
            {
                mAveSPItem.CachePrincipalFromMetadata();
            }
        }

        public void CachePrincipalFromPermission(int value)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFolder.CachePrincipalFromPermission"))
            {
                mAveSPItem.CachePrincipalFromPermission(value);
            }
        }

        public void CachePrincipalOfTargetAudience()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFolder.CachePrincipalOfTargetAudience"))
            {
                mAveSPItem.CachePrincipalOfTargetAudience();
            }
        }

        public void ExportUserCache(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFolder.ExportUserCache"))
            {
                output.WriteMetadata(AveMetadataType.UserCache, mAveSPItem.DataCache.UserList);
            }
        }

        public string ExportUserCache()
        {
            return AveConvert.ConvertAveObjToAveXml(AveMetadataType.UserCache.ToString(), mAveSPItem.DataCache.UserList);
        }

        public void ExportGroupCache(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFolder.ExportGroupCache"))
            {
                output.WriteMetadata(AveMetadataType.GroupCache, mAveSPItem.DataCache.GroupList);
            }
        }

        public string ExportGroupCache()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFolder.ExportGroupCache"))
            {
                return AveConvert.ConvertAveObjToAveXml(AveMetadataType.GroupCache.ToString(), mAveSPItem.DataCache.GroupList);
            }
        }

        public void ExportDocInfo(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFolder.ExportDocInfo"))
            {
                Dictionary<string, object> docInfo = mAveSPItem.GetDocInfo();
                if (docInfo != null)
                {
                    if (this.mTimeLastModified != DateTime.MinValue)
                    {
                        docInfo["TimeLastModified"] = this.mTimeLastModified;
                    }
                    output.WriteMetadata(AveMetadataType.DocProperty, docInfo);
                }
            }
        }

        public string ExportDocInfo()
        {
            string xml = string.Empty;
            Dictionary<string, object> docInfo = mAveSPItem.GetDocInfo();
            if (docInfo != null)
            {
                xml = AveConvert.ConvertAveObjToAveXml(AveMetadataType.DocProperty.ToString(), docInfo);
            }
            return xml;
        }

        public void ExportUserDataInfo(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFolder.ExportDocInfo"))
            {
                Dictionary<string, object> userData = mAveSPItem.UserDataCache;
                if (userData == null)
                {
                    mAveSPItem.UserDataCache = mAveSPItem.GetUserData();
                    userData = mAveSPItem.UserDataCache;
                }
                if (userData != null)
                {
                    if (!string.IsNullOrEmpty(mServerRelativeUrl) && mParentFolder.AveList.NeedExportExcel && !mParentFolder.AveList.SPList.Hidden)
                    {
                        mAveSPItem.ExportDataToExcel(mServerRelativeUrl.TrimStart('/'));
                    }
                    output.WriteMetadata(AveMetadataType.DocData, userData);
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "DataJunctionInfo is a part of common method name")]
        public void ExportDataJunctionInfo(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFolder.ExportDataJunctionInfo"))
            {
                List<Dictionary<string, object>> dataCache = mAveSPItem.GetUserDataJunction();
                if (dataCache != null)
                {
                    output.WriteMetadata(AveMetadataType.DocDataJunction, dataCache);
                }
            }
        }

        public bool HasUniqueRoleAssignments
        {
            get
            {
                return mAveSPItem.HasUniqueRoleAssignments;
            }
        }

        public void ExportUnavailableUserInCache(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFolder.ExportUnavailableUserInCache"))
            {
                mAveSPItem.ExportUnavailableUserInCache(output);
            }
        }

        /// <summary>
        /// 使用这个方法cache userdatajunction里面的user信息
        /// </summary>
        public void CachePrincipalFromDatajunction()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFolder.CachePrincipalFromDataJunction"))
            {
                mAveSPItem.CachePrincipalFromDatajunction();
            }
        }

        public Dictionary<string, object> GetAllColumnValues(ColumnsLevel columnsLevel = ColumnsLevel.None)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFolder.GetAllColumnValues"))
            {
                return mAveSPItem.GetAllColumnValues(columnsLevel);
            }
        }

        public void ExportFullTextIndex(IAveBackupStream output, Dictionary<string, object> customFieldValues)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFolder.ExportFullTextIndex"))
            {
                var index = new FullTextIndex() 
                {
                    TimeZoneInfoID = AveList.ParentWeb.TimeZoneInfoId,                    
                };
                if (customFieldValues != null)
                {
                    index.SetCustomColumnValues(customFieldValues);
                }
                output.WriteMetadata(AveMetadataType.FullTextIndex, index);
            }
        }

        #region add for RevIM
        public void ExportMetadata(IAveBackupStream stream, SPItemMetadataBackupOption backupOption)
        {
            var metadata = new AveSPFolderMetadataDto();

            #region backup ItemMetadata
            if (this.mAveSPItem.RowId > 0)
            {
                var userData = mAveSPItem.GetUserDataInfoWithDependence(backupOption);
                metadata.UserDataInfo = userData.ItemA;
                metadata.MetadataInfo = userData.ItemB;

                metadata.DocDataJunction = mAveSPItem.GetUserDataJunctionCache(true);
                //output.WriteMetadata(AveMetadataType.DocDataJunction, dataCache);

                if (backupOption != null && backupOption.BackupItemTPGUIDofLookupValue)
                {
                    metadata.ItemTPGUIDofLookupValue = mAveSPItem.GetLookupFieldGuidValue();
                }
            }
            #endregion

            metadata.DocInfo_Old = GetDocInfo();
            if (backupOption != null)
            {
                if (backupOption.IncludeUser)
                {
                    metadata.UserCache = mAveSPItem.GetUserCache(false);
                }
                if (backupOption.IncludeGroup)
                {
                    metadata.GroupCache = mAveSPItem.GetGroupCache();
                }
            }

            stream.WriteMetadata(AveMetadataType.ItemMetadataDto, metadata);
        }

        private Dictionary<string, object> GetDocInfo()
        {
            Dictionary<string, object> docInfo = mAveSPItem.GetDocInfo();
            if (docInfo != null)
            {
                if (this.mTimeLastModified != DateTime.MinValue)
                {
                    docInfo["TimeLastModified"] = this.mTimeLastModified;
                }
            }
            return docInfo;
        }
        #endregion

        #region IAveEnableCache Member

        /// <summary>
        /// 目前只有Server10支持cache
        /// </summary>
        public bool EnableCache
        {
            get
            {
                var folderWithCache = this.SPFolder as IAveEnableCache;
                if (folderWithCache != null)
                {
                    return folderWithCache.EnableCache;
                }
                return false;
            }
            set
            {
                var folderWithCache = this.SPFolder as IAveEnableCache;
                if (folderWithCache != null)
                {
                    folderWithCache.EnableCache = value;
                }
            }
        }

        public void Dispose()
        {
            var folderWithCache = this.SPFolder as IAveEnableCache;
            if (folderWithCache != null)
            {
                folderWithCache.Dispose();
            }
        }

        #endregion IAveEnableCache Member
    }
}