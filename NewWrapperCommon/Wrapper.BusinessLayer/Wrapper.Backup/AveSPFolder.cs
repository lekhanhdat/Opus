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
using AvePoint.Common;
using AvePoint.Wrapper.Core.SPBackup;
using AvePoint.Wrapper.Core.SPBackupDto;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPFolder : IAveEnableCache, AvePoint.Wrapper.Backup.IAveSPFolder, ISPFolderExport
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveSPFolder));
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

        public List<Dictionary<string, object>> ImmedSubscriptionsCache = null;
        public List<Dictionary<string, object>> SchedSubscriptionsCache = null;

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

        public string Url
        {
            get
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFolder.TagUrl"))
                {
                    string tagUrl = string.Empty;//add sp1 estimate
                    if (CheckFolderStyle())
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

        /// <summary>
        /// SP2013 List中Folder Url的格式为http://hostheader/sites/team/lists/test/DispForm.aspx?ID=2
        /// System Folder下的Folder需要特别注意
        /// </summary>
        /// <returns></returns>
        private bool CheckFolderStyle()
        {
            if (mAveParentSite.SPContextKind.IsServerMode13Upper())
            {
                return mAveList.SPList == null || (mAveList.SPList != null && mAveList.SPList.BaseType == AveBaseType.DocumentLibrary);
            }

            return (mQueryService != null && (mQueryService as IAveConnectorQueryService).IsSP2010SP1(mAveParentSite.SPSite.ID))
                     || (mAveList.SPList != null && mAveList.SPList.BaseType == AveBaseType.DocumentLibrary);
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
                Init(-1, aveList.ScopeId);
            }
        }

        /// <summary>
        /// through folder
        /// </summary>
        /// <param name="aveList"></param>
        /// <param name="folder"></param>
        internal AveSPFolder(AveSPList aveList, IAveFolder folder)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFolder.Constructor"))
            {
                mAveList = aveList;
                mSender = aveList.Sender;
                mId = folder.UniqueId;
                var rowId = -1;
                if (folder.Item != null)
                {
                    rowId = folder.Item.ID;
                    mVersion = folder.Item.Versions[0].VersionId;
                }

                Init(rowId, Guid.Empty);
                mSPFolder = folder;
                mQueryService = aveList.QueryService;
                if (folder.ServerRelativeUrl.Length == aveList.ServerRelativeUrl.Length)
                {
                    mPath = aveList.Path;
                }
                else
                {
                    mPath = aveList.Path + "\\" +
                            folder.ServerRelativeUrl.Substring(aveList.ServerRelativeUrl.Length + 1);
                }
                mServerRelativeUrl = folder.ServerRelativeUrl;
                mAveParentSite = aveList.ParentSite;
                mTimeLastModified = DateTime.MinValue;
            }
        }

        public AveSPFolder(AveSPFolder aveFolder, string name, Guid id, int rowId, int version)
            : this(aveFolder, name, id, rowId, version, DateTime.MinValue)
        {
        }
        public AveSPFolder(AveSPFolder aveFolder, string name, Guid id, int rowId, int version, DateTime currentVersionModified)
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
                Init(rowId, Guid.Empty);
                mTimeLastModified = currentVersionModified;

                if (mAveList.ParentSite.SPContextKind == AveContextKind.ClientObjectModel)
                {
                    if (mAveSPItem.SPListItem == null)
                    {
                        mSPFolder = null;
                    }
                    else if (mAveList.SPList != null)
                    {
                        string urlTag = mAveList.ServerRelativeUrl.Substring(0, mAveList.ServerRelativeUrl.Length - mAveList.SPList.RootFolder.Url.Length);
                        string folderServerRelativeUrl = '/' + urlTag.Trim('/') + "/" + this.mAveSPItem.SPListItem.Url.Trim('/');
                        if (folderServerRelativeUrl.StartsWith("//", StringComparison.OrdinalIgnoreCase))
                        {
                            folderServerRelativeUrl = folderServerRelativeUrl.Substring(1);
                        }
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

        private void Init(int rowId, Guid ScopeId)
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
            Guid folderId = TryGetFolderId(folder);
            mAveSPItem = new AveSPItem(mId, rowId, mVersion, mServerRelativeUrl, AveItemType.Folder, folderId,
                mAveList.ParentWeb.ParentSite.SPSite.ID, mAveList,
                mSender, mQueryService, mAveList.Fields, mAveList.SolutionStatus, folder);
            if (!ScopeId.Equals(Guid.Empty))
            {
                mAveSPItem.ScopeId = ScopeId;
            }
            //if (rowId != -1)
            //{
            //    mAveSPItem.ParentId = mParentFolder.Id;
            //}
        }
        //ADO-132097, 某些客户环境中folder.UniqueId会出错, 增加容错处理, 使用folder.Item.UniqueId在取一遍。
        private Guid TryGetFolderId(IAveFolder folder)
        {
            if (folder == null)
            {
                return Guid.Empty;
            }
            try
            {
                return folder.UniqueId;
            }
            catch (Exception ex)
            {
                if (folder.Item != null)
                {
                    logger.Warn("Failed to get folder.UniqueId, use folder.Item.UniqueId instead, error:{0}", ex);
                    return folder.Item.UniqueId;
                }
                else
                {
                    logger.Error("Failed to get folderId from folder.Item.UniqueId, folder.Item is null.");
                    return Guid.Empty;
                }
                
            }
        }

        public string ServerRelativeUrl
        {
            get { return mServerRelativeUrl; }
            set { mServerRelativeUrl = value; }
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
                aveSPItem.HasStream = ((int)docInfoDic["HasStream"] == 1);
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

        public void SetItemProperty(IAveSPItem aveSPItem, Dictionary<string, object> docInfoDic)
        {
            this.SetItemProperty(aveSPItem as AveSPItem, docInfoDic);
        }

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

        public int UserDataJunctionCacheMaxRow
        {
            get
            {
                var folderWithCache = this.SPFolder as IAveEnableCache;
                if (folderWithCache != null)
                {
                    return folderWithCache.UserDataJunctionCacheMaxRow;
                }
                return 0;
            }
            set
            {
                var folderWithCache = this.SPFolder as IAveEnableCache;
                if (folderWithCache != null)
                {
                    folderWithCache.UserDataJunctionCacheMaxRow = value;
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


        private string GetFolderUrlForNoteBoardWebPart()
        {
            string result = string.Empty;
            if (this.Url.Equals(string.Empty))
            {
                string webAppUrl = AveList.ParentWeb.SPWeb.Url.Substring(0, AveList.ParentWeb.SPWeb.Url.IndexOf(AveList.ParentWeb.SPWeb.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase));
                string folderUrl = this.ServerRelativeUrl.StartsWith("/", StringComparison.Ordinal) ? this.ServerRelativeUrl : "/" + this.ServerRelativeUrl;
                result = webAppUrl + folderUrl;
            }
            else
            {
                result = this.Url;
            }
            return result;
        }

        #region IAveSPFolder Members

        IAveSPItem IAveSPFolder.AveItem
        {
            get { return mAveSPItem; }
        }

        IAveSPList IAveSPFolder.ParentList
        {
            get { return mAveList; }
        }

        IAveSPFolder IAveSPFolder.ParentFolder
        {
            get { return mParentFolder; }
        }

        IAveSPSite IAveSPFolder.AveSPSite
        {
            get { return mAveParentSite; }
        }

        public void ExportDocInfo(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPFolder.ExportDocInfo"))
            {
                var docInfo = GetDocInfo();
                if (docInfo != null)
                {
                    output.WriteMetadata(AveMetadataType.DocProperty, docInfo);
                }
            }
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

        public void ExportSocialTags(IAveBackupStream output)
        {
            if (this.ParentSite.SPContextKind.IsServerMode10Upper())
            {
                if (AveEnv.IsMoss && AveItem.ScopeUrl != null)
                {//ScopeUrl为空时，还原tag时会误将其作为rootsite的tag进行备份（参考doc80662）
                    string absoluteUrl = GetFolderUrlForNoteBoardWebPart();
                    var tag = new AveSPSocialTag(absoluteUrl, this.ParentSite);
                    tag.Export(output);
                }
            }
        }

        public void ExportSocialComments(IAveBackupStream output)
        {
            if (this.ParentSite.SPContextKind.IsServerMode10Upper())
            {
                if (AveEnv.IsMoss && AveItem.ScopeUrl != null)
                {
                    string absoluteUrl = GetFolderUrlForNoteBoardWebPart();
                    var comment = new AveSPSocialComment(absoluteUrl, this.ParentSite);
                    comment.Export(output);
                }
            }
        }

        public void ExportToExcel()
        {
            if (!string.IsNullOrEmpty(mServerRelativeUrl) && this.AveList.NeedExportExcel && this.AveList.SPList != null && !this.AveList.SPList.Hidden)
            {
                mAveSPItem.ExportDataToExcel(mServerRelativeUrl.TrimStart('/'));
            }
        }

        /// <param name="includeUser">是否先备份Alert的User，避免还原的时候不存在导致找不到User</param>
        public void ExportAlerts(IAveBackupStream output, bool includeUser = true, bool onlyAvaiableUser = false)
        { 
            if (includeUser)
            {
                AveItem.CacheUserFromAlert(this);
                if (onlyAvaiableUser)
                {
                    AveItem.ExportUnavailableUserInCache(output);
                }
                else
                {
                    AveItem.ExportUserCache(output);
                }
            }
            AveSPAlert mFolderAlert = AveSPAlert.CreateInstance(this);
            mFolderAlert.Export(output);
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

        #endregion

        public void ExportMetadata(IAveBackupStream stream, SPItemMetadataBackupOption backupOption)
        {
            var metadata = new SPFolderMetadataDto();

            #region backup ItemMetadata
            if (this.mAveSPItem.RowId > 0)
            {
                var userData = mAveSPItem.GetUserDataInfoWithDependence(backupOption);
                metadata.UserDataInfo = userData.Item1;
                metadata.MetadataInfo = userData.Item2;

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

        public void ExportRoleAssignments(IAveBackupStream stream)
        {
            ExportRoleAssignments(stream, new SPRoleAssignmentsBakupOption()
            {
                IncludeUsers = true,
                IncludeGroups = true,
                IncludeInheritedRoleAssignments = false,
            });
        }

        public void ExportRoleAssignments(IAveBackupStream stream, SPRoleAssignmentsBakupOption backupOption)
        {
            if (backupOption == null)
            {
                throw new ArgumentNullException("backupOption");
            }
            mAveSPItem.ExportRoleAssignments(stream, backupOption.IncludeUsers, backupOption.IncludeGroups);
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
            mAveSPItem.ExportSocialInfos(stream, Url);
        }
    }
}