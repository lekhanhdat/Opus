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
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.BackupRestore
{
    internal class AveOD4BList : AveOD4BBase, IAveBackupRestoreList
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private bool mFailed = false;
        private AveBRListInfo mInternalInfo;
        //Identity of parent web
        private string mWebUrl = string.Empty;
        //Identity of one AveOD4BList
        private string mFullUrl = string.Empty;
        //Whether inherites permission from its parent
        private bool mHasUniqueRoleAssignments = false;

        private int mRootFolderItemCount;

        private string mRootFolderServerRelativeUrl = string.Empty;

        private Dictionary<string, object> mInformationRightsManagementDic = null;

        //Columns that need to be backup its value of one item
        List<string> mColumns = new List<string>();
        //editable columns
        List<string> mCustomColumns = new List<string>();

        #region Properties
        public string WebUrl
        {
            get { return this.mWebUrl; }
        }

        public string FullUrl
        {
            get { return this.mFullUrl; }
        }

        public string RootFolderServerRelativeUrl
        {
            get { return this.mRootFolderServerRelativeUrl; }
        }

        public Guid Id
        {
            get
            {
                VerifyCacheData("ListBasic");
                return this.mInternalInfo.Id;
            }
        }

        public string Title
        {
            get
            {
                VerifyCacheData("ListBasic");
                return this.mInternalInfo.Title;
            }
        }

        //Total items count(not include folder count)
        public int ItemCount
        {
            get
            {
                VerifyCacheData("ListBasic");
                return this.mInternalInfo.ItemCount;
            }
        }

        public bool IrmEnabled
        {
            get
            {
                VerifyCacheData("ListBasic");
                return this.mInternalInfo.IrmEnabled;
            }
        }

        public bool EnableVersioning
        {
            get
            {
                VerifyCacheData("ListBasic");
                return this.mInternalInfo.EnableVersioning;
            }
            set
            {
                VerifyCacheData("ListBasic");
                this.mInternalInfo.EnableVersioning = value;
            }
        }
        public bool EnableMinorVersions
        {
            get
            {
                VerifyCacheData("ListBasic");
                return this.mInternalInfo.EnableMinorVersions;
            }
            set
            {
                VerifyCacheData("ListBasic");
                this.mInternalInfo.EnableMinorVersions = value;
            }
        }
        public bool EnableModeration
        {
            get
            {
                VerifyCacheData("ListBasic");
                return this.mInternalInfo.EnableModeration;
            }
            set
            {
                VerifyCacheData("ListBasic");
                this.mInternalInfo.EnableModeration = value;
            }
        }

        internal List<string> Columns
        {
            get { return this.mColumns; }
        }

        internal List<string> CustomColumns
        {
            get { return this.mCustomColumns; }
        }

        internal bool IncludeItemVersions { get; private set; }

        internal bool IncludeSystemUpdate { get; private set; }

        internal Dictionary<string, int> FailedItems { get; private set; }
        #endregion

        public AveOD4BList(AveOD4BRequestController controller, string webUrl, string url)
            : base(controller)
        {
            this.mWebUrl = webUrl;
            this.mFullUrl = url;
            try
            {
                VerifyCacheData("ListBasic");
                this.mHasUniqueRoleAssignments = this.mInternalInfo.HasUniqueRoleAssignments;
                this.mRootFolderItemCount = this.mInternalInfo.RootFolderItemCount;
                this.mRootFolderServerRelativeUrl = this.mInternalInfo.RootFolderServerRelativeUrl;
            }
            catch (Exception ex)
            {
                mLog.Warn("Failed to init list basic info. Error:{0}", ex);
                this.mFailed = true;
            }

            //if (this.mItemCount > 0)
            //{
            InitColumns();
            //}
        }

        private void InitColumns()
        {
            if (this.mFailed) return;

            //add hidden or readonly fields
            this.mColumns.Add("File_x0020_Size");
            this.mColumns.Add("Editor");
            this.mColumns.Add("Author");
            this.mColumns.Add("FileRef");
            this.mColumns.Add("Created");
            this.mColumns.Add("Modified");
            this.mColumns.Add("_UIVersionString");
            this.mColumns.Add("_UIVersion");
            this.mColumns.Add("_Level");
            this.mColumns.Add("ContentTypeId");
            this.mColumns.Add("FileLeafRef");
            this.mColumns.Add("FileDirRef");
            this.mColumns.Add("ID");
            this.mColumns.Add("UniqueId");
            this.mColumns.Add("FSObjType");
            this.mColumns.Add("HTML_x0020_File_x0020_Type");
            if (this.EnableModeration)
            {
                this.mColumns.Add("_ModerationStatus");
                this.mColumns.Add("_ModerationComments");
            }

            var columns = mController.GetListEditableFields(this.mWebUrl, this.mFullUrl);
            foreach (var column in columns)
            {
                //如果有一个column internal name是Folder(case sensitive)的话,需要过滤, 否则在获取column value的时候Client API会抛错. 
                //而且不能给folder的column value赋值, API同样会抛错
                if (string.Equals("Folder", column, StringComparison.Ordinal))
                {
                    mLog.Warn("Skip the special column:[Folder]");
                    continue;
                }
                this.mCustomColumns.Add(column);
                if (this.mColumns.Exists(str => string.Equals(str, column, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }
                this.mColumns.Add(column);
            }
            StringBuilder builder = new StringBuilder();
            foreach (var column in this.mColumns)
            {
                builder.AppendFormat("[{0}] ", column);
            }
            mLog.Info("Columns of List {0}: {1}", this.mFullUrl, builder.ToString());
        }

        public void IncludeItemVersion(bool yes)
        {
            this.IncludeItemVersions = yes;
        }

        public void IncludeSystemUpdateOption(bool yes)
        {
            this.IncludeSystemUpdate = yes;
        }

        public void SetFailedItems(Dictionary<string, int> failedItems)
        {
            this.FailedItems = failedItems;
        }

        public void DisableInformationRightsManagementSettings()
        {
            if (this.mInternalInfo.IrmEnabled)
            {
                this.mInformationRightsManagementDic = GetInformationRightsManagementSettingsInfo();
                if (this.mInformationRightsManagementDic != null)
                {
                    this.mController.InformationRightsManagementSettingsReset(this.mWebUrl, this.mFullUrl);
                }
            }
        }

        public void EnableInformationRightsManagementSettings()
        {
            if (this.mInternalInfo.IrmEnabled && this.mInformationRightsManagementDic != null)
            {
                this.mController.InformationRightsManagementSettingsUpdate(this.mWebUrl, this.mFullUrl, this.mInformationRightsManagementDic);
            }
        }

        public IAveBackupRestoreFolder GetRootFolder()
        {
            //delete all items in list when running inc job
            if (ItemCount == 0 && mBackupAll)
            {
                return new AveOD4BFolder(this.mWebUrl, this.mRootFolderServerRelativeUrl, this, null);
            }
            else
            {
                AveBRFolderInfo rootFolder = new AveBRFolderInfo();
                rootFolder.ItemCount = mRootFolderItemCount;
                rootFolder.ServerRelativeUrl = this.mRootFolderServerRelativeUrl;
                rootFolder.Name = "{System Folder}";
                rootFolder.SubFolders = BuildFolderTree(GetAllFolders());
                if (mBackupAll)
                {
                    return new AveOD4BFolder(this.mWebUrl, this.mRootFolderServerRelativeUrl, this, rootFolder);
                }
                else
                {
                    return new AveOD4BCacheFolder(this.mWebUrl, this.mRootFolderServerRelativeUrl, this, ChangedItems, rootFolder);
                }
            }
        }

        protected override List<AveBRChangeObject> GetChangedObjects()
        {
            return Controller.GetListChangedItems(this.WebUrl, this.mRootFolderServerRelativeUrl, Id, mBackupStartTime, mBackupEndTime, this.IncludeItemVersions, false, true, this.IncludeSystemUpdate, this.Columns, this.FailedItems);
        }

        private List<AveBRFolderInfo> GetAllFolders()
        {
            return mController.GetAllFoldersInList(this.WebUrl, this.mRootFolderServerRelativeUrl, this.mColumns, ItemCount > 5000);
        }

        /// <summary>
        /// 先将所有的folder进行分层
        /// 从层次最深的folder开始逐个找parent
        /// </summary>
        /// <param name="folders"></param>
        /// <returns></returns>
        private List<AveBRFolderInfo> BuildFolderTree(List<AveBRFolderInfo> folders)
        {
            mLog.Info("There are totally {0} folders in List {1}", folders.Count.ToString(), this.mRootFolderServerRelativeUrl);
            if (folders.Count == 0)
            {
                return new List<AveBRFolderInfo>();
            }

            List<AveBRFolderInfo> folderTree = new List<AveBRFolderInfo>();
            List<int> levels = new List<int>();
            Dictionary<int, List<AveBRFolderInfo>> levelFolders = new Dictionary<int, List<AveBRFolderInfo>>();
            foreach (var folder in folders)
            {
                int level = folder.ServerRelativeUrl.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Length;
                List<AveBRFolderInfo> sameLevelFolders = null;
                if (levelFolders.TryGetValue(level, out sameLevelFolders))
                {
                    sameLevelFolders.Add(folder);
                }
                else
                {
                    sameLevelFolders = new List<AveBRFolderInfo>();
                    sameLevelFolders.Add(folder);
                    levelFolders[level] = sameLevelFolders;
                    levels.Add(level);
                }
            }

            levels.Sort();
            for (int index = levels.Count - 1; index >= 0; index--)
            {
                foreach (var subfolder in levelFolders[levels[index]])
                {
                    //如果子folder没有变化的话，就不append到folder tree上
                    if (TrimFolderTree(subfolder))
                    {
                        continue;
                    }
                    //index == 0 -> 遍历到最顶层的folder，没有parent folder
                    if (index == 0)
                    {
                        folderTree.Add(subfolder);
                    }
                    else
                    {
                        AppendToParent(levelFolders[levels[index - 1]], subfolder, this.RootFolderServerRelativeUrl);
                    }
                }
            }

            return folderTree;
        }

        private void AppendToParent(List<AveBRFolderInfo> nomineedParents, AveBRFolderInfo subFolder, string rootFolderServerRelativeUrl)
        {
            foreach (var parent in nomineedParents)
            {
                if (subFolder.ServerRelativeUrl.StartsWith(parent.ServerRelativeUrl + "/", StringComparison.OrdinalIgnoreCase))
                {
                    if (parent.SubFolders == null)
                    {
                        parent.SubFolders = new List<AveBRFolderInfo>();
                    }
                    parent.SubFolders.Add(subFolder);
                    return;
                }
            }

            string url = subFolder.ServerRelativeUrl.Replace(rootFolderServerRelativeUrl, "").TrimStart('/');
            if (url.StartsWith("Forms/", StringComparison.OrdinalIgnoreCase))
            {
                mLog.Info("Skip the system folder. Sub folder Url: {0}", subFolder.ServerRelativeUrl);
                return;
            }

            StringBuilder builder = new StringBuilder();
            foreach (var parent in nomineedParents)
            {
                builder.AppendFormat("[{0}] ", parent.ServerRelativeUrl);
            }
            mLog.Info("Parent Folder Urls: {0}", builder.ToString());

            throw new Exception(string.Format("Cannot find parent folder. Folder Url:{0}", subFolder.ServerRelativeUrl));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <returns>true if folder no need add to folder tree</returns>
        private bool TrimFolderTree(AveBRFolderInfo folder)
        {
            if (mBackupAll)
            {
                return false;
            }

            return !CheckChangesExistInFolder(folder);
        }
        //[pending]set change type to folder object
        private bool CheckChangesExistInFolder(AveBRFolderInfo folder)
        {
            return ChangedItems.Exists(item =>
                (item.Exception == null && item.ServerRelativeUrl.StartsWith(folder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase) ||
                ((item.ChangeType == 4 || item.ChangeType == 1) && folder.ServerRelativeUrl.StartsWith(item.ServerRelativeUrl) && string.Equals(item.ItemType, "1", StringComparison.OrdinalIgnoreCase)))
             );
        }

        protected override string Level
        {
            get
            {
                return "List";
            }
        }

        protected override void EnsureExportMethods()
        {
            ExportMethods[BackupOption.BasicInfo] = ExportBasicInfo;
            ExportMethods[BackupOption.RoleAssignment] = ExportRoleAssignment;
        }

        private ProcessResult ExportBasicInfo(IAveBackupStream stream)
        {
            ProcessResult result = new ProcessResult();
            VerifyCacheData("ListBasic");

            stream.WriteMetadata(AveMetadataType.ListBasicInfo, InfoConverter<AveListInfo>.ConvertToCommonInfo(this.mInternalInfo));
            stream.WriteMetadata(AveMetadataType.ListProperty, InfoConverter<AveListSettingInfo>.ConvertToCommonInfo(this.mInternalInfo));
            return result;
        }

        private ProcessResult ExportRoleAssignment(IAveBackupStream stream)
        {
            ProcessResult result = new ProcessResult();
            if (this.mHasUniqueRoleAssignments)
            {
                var listRoleAssignmentInfo = this.mController.GetListRoleAssignmentsInfo(this.mWebUrl, this.mFullUrl);
                if (listRoleAssignmentInfo == null)
                {
                    throw new Exception("Failed to backup list role assignment info");
                }
                if (listRoleAssignmentInfo.Count > 0)
                {
                    List<AveRoleAssignmentInfo> infos = new List<AveRoleAssignmentInfo>();
                    listRoleAssignmentInfo.ForEach(roleAssignmentInfo =>
                    {
                        infos.Add(InfoConverter<AveRoleAssignmentInfo>.ConvertToCommonInfo(roleAssignmentInfo));
                    });
                    stream.WriteMetadata(AveMetadataType.RoleAssignment, infos);
                }
            }
            return result;
        }

        private Dictionary<string, object> GetInformationRightsManagementSettingsInfo()
        {
            Dictionary<string, object> informationRightsManagementDic = null;
            TimeSpan timeSpan = this.mInternalInfo.DocumentLibraryProtectionExpireDate.AddDays(1.0) - DateTime.UtcNow;
            if (timeSpan.Ticks > 0)
            {
                informationRightsManagementDic = new Dictionary<string, object>();
                informationRightsManagementDic.Add("IrmExpire", this.mInternalInfo.IrmExpire);
                informationRightsManagementDic.Add("IrmReject", this.mInternalInfo.IrmReject);
                informationRightsManagementDic.Add("PolicyTitle", this.mInternalInfo.PolicyTitle);
                informationRightsManagementDic.Add("PolicyDescription", this.mInternalInfo.PolicyDescription);
                informationRightsManagementDic.Add("DocumentLibraryProtectionExpireDate", this.mInternalInfo.DocumentLibraryProtectionExpireDate);
                informationRightsManagementDic.Add("DisableDocumentBrowserView", this.mInternalInfo.DisableDocumentBrowserView);
                informationRightsManagementDic.Add("EnableGroupProtection", this.mInternalInfo.EnableGroupProtection);
                informationRightsManagementDic.Add("GroupName", this.mInternalInfo.GroupName);
                informationRightsManagementDic.Add("AllowPrint", this.mInternalInfo.AllowPrint);
                informationRightsManagementDic.Add("AllowScript", this.mInternalInfo.AllowScript);
                informationRightsManagementDic.Add("AllowWriteCopy", this.mInternalInfo.AllowWriteCopy);
                informationRightsManagementDic.Add("EnableDocumentAccessExpire", this.mInternalInfo.EnableDocumentAccessExpire);
                informationRightsManagementDic.Add("DocumentAccessExpireDays", this.mInternalInfo.DocumentAccessExpireDays);
                informationRightsManagementDic.Add("EnableLicenseCacheExpire", this.mInternalInfo.EnableLicenseCacheExpire);
                informationRightsManagementDic.Add("LicenseCacheExpireDays", this.mInternalInfo.LicenseCacheExpireDays);
            }
            return informationRightsManagementDic;
        }

        protected override void FillCacheData(ProcessResult result)
        {
            mLog.Info("Begin to fill {0} cache data", this.mFullUrl);
            var info = base.mController.BatchGetOD4BListInfo(this.mWebUrl, this.mFullUrl);

            foreach (var kv in info)
            {
                if (string.Equals(kv.Key, "ListBasic", StringComparison.OrdinalIgnoreCase))
                {
                    this.mInternalInfo = (AveBRListInfo)kv.Value;
                    this.mInternalInfo.Url = this.mFullUrl;
                }
                base.mInternCache.Add(kv.Key, new CacheItem() { Value = kv.Value, Result = result });
            };
        }

        protected override void AddFakeData(ProcessResult result)
        {
            base.mInternCache.Add("ListBasic", new CacheItem() { Value = null, Result = result });
        }
    }
}
