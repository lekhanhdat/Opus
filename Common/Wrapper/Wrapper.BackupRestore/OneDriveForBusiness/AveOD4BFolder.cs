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
using System.Reflection;
using System.Collections.Generic;

using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using System.IO;
using System.Text;
using System.Linq;

namespace AvePoint.Wrapper.BackupRestore
{
    internal class AveOD4BFolder : AveOD4BBase, IAveBackupRestoreFolder
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private string mServerRelativeUrl = string.Empty;
        private string mWebUrl = string.Empty;
        //Folder.ItemCount API返回值为当前folder下一层的item(folder&item)数量
        private int mItemCount = 0;
        private int mRowId = 0;
        private bool mHasUniqueRoleAssignments = false;
        private const int BUFFERSIZE = 1024 * 1024;
        private Guid mUniqueId = Guid.Empty;
        private List<AveBRFolderInfo> mSubFolders = null;
        private AveOD4BList mParentList = null;
        private DateTime mCreated;
        private DateTime mModified;

        public event ExportFileAction FileMetadataExporting;
        public event ExportFileAction FileMetadataExported;
        public event ExportFileAction FileContentExporting;
        public event ExportFileAction FileContentExported;
        public event ExportFileAction AddingReport;
        public event ExportFileAction FilteringOutFile;

        public string SeverRelativeUrl
        {
            get { return this.mServerRelativeUrl; }
        }

        public string WebUrl
        {
            get { return this.mWebUrl; }
        }

        public string Name { get; private set; }

        public int Id
        {
            get { return this.mRowId; }
        }
        public Guid UniqueId
        {
            get { return this.mUniqueId; }
        }

        public Guid ParentListId
        {
            get { return this.mParentList.Id; }
        }

        internal int VersionCount { get; private set; }
        internal int Author { get; set; }
        internal int Editor { get; set; }
        internal int UIVersion { get; set; }
        internal string HTMLFileType { get; set; }
        internal IStreamConvertor StreamConvertor { get; private set; }

        internal AveOD4BFolder(string webUrl, string folderUrl, AveOD4BList parentList, AveBRFolderInfo folderInfo = null)
            : base(parentList.Controller)
        {
            this.mWebUrl = webUrl;
            this.mServerRelativeUrl = folderUrl;
            this.mParentList = parentList;
            if (folderInfo != null)
            {
                this.mRowId = folderInfo.Id;
                this.mUniqueId = folderInfo.UniqueId;
                this.mItemCount = folderInfo.ItemCount;
                this.Name = folderInfo.Name;
                this.mSubFolders = folderInfo.SubFolders;
                this.mHasUniqueRoleAssignments = folderInfo.HasUniqueRoleAssignments;
                this.mCreated = folderInfo.Created;
                this.mModified = folderInfo.Modified;
                this.UIVersion = folderInfo.UIVersion;
                this.Author = folderInfo.Author;
                this.Editor = folderInfo.Editor;
                this.HTMLFileType = folderInfo.HTMLFileType;
            }
        }

        protected override string Level
        {
            get
            {
                return "Folder";
            }
        }
        public void SetVersionCount(int versionCount)
        {
            VersionCount = versionCount;
        }

        public void SetStreamConvertor(IStreamConvertor streamConvertor)
        {
            StreamConvertor = streamConvertor;
        }

        protected override void EnsureExportMethods()
        {
            ExportMethods[BackupOption.BasicInfo] = ExportBasicInfo;
            ExportMethods[BackupOption.RoleAssignment] = ExportRoleAssignment;
        }

        private ProcessResult ExportBasicInfo(IAveBackupStream stream)
        {
            ProcessResult result = new ProcessResult();

            //var values = GetColumnValues(this.mParentList.Columns);
            ExportFolderBasicInfo(stream);
            ExportFolderColumnValues(stream);
            //ExportColumnValues(stream, values);

            return result;
        }

        private void ExportFolderBasicInfo(IAveBackupStream stream)
        {
            Dictionary<string, object> docInfo = new Dictionary<string, object>();
            docInfo["Id"] = this.mUniqueId;
            //[pending]
            docInfo["UIVersion"] = 512;//baseItemInfo.Version;
            docInfo["DoclibRowId"] = this.mRowId;
            docInfo["IsCurrentVersion"] = true;
            docInfo["TimeCreated"] = this.mCreated;
            docInfo["TimeLastModified"] = this.mModified;

            stream.WriteMetadata(AveMetadataType.DocProperty, docInfo);
        }

        private void ExportFolderColumnValues(IAveBackupStream stream)
        {
            Dictionary<string, object> userData = new Dictionary<string, object>();
            userData["Author"] = this.Author;
            userData["Editor"] = this.Editor;
            userData["Modified"] = this.mModified;
            userData["Created"] = this.mCreated;
            userData["#tp_Level"] = 1;
            userData["HTML_x0020_File_x0020_Type"] = this.HTMLFileType;
            //userData["#tp_ContentTypeId"] = info.ContentTypeId;
            stream.WriteMetadata(AveMetadataType.DocData, userData);
        }

        private void ExportFileBasicInfo(IAveBackupStream stream, AveBRItemInfo info)
        {
            Dictionary<string, object> docInfo = new Dictionary<string, object>();

            docInfo["Id"] = info.UniqueId;
            docInfo["UIVersion"] = info.UIVersion;
            docInfo["DoclibRowId"] = info.Id;
            docInfo["IsCurrentVersion"] = info.IsCurrent;
            docInfo["Level"] = info.Level;
            docInfo["TimeCreated"] = info.Created;
            docInfo["TimeLastModified"] = info.Modified;
            docInfo["HasStream"] = info.Length > 0 ? 1 : 0;
            docInfo["LeafName"] = info.Name;
            docInfo["IsCheckOut"] = info.Level == 255;
            docInfo["HasUniqueRoleAssignments"] = info.HasUniqueRoleAssignments;
            docInfo["BiggestVersionModified"] = info.BiggestVersionModified;
            //[DEBUG]
            StringBuilder builder = new StringBuilder();
            foreach (var kv in docInfo)
            {
                builder.AppendFormat("[Key:{0},Value:{1}]", kv.Key, kv.Value == null ? "NULL" : kv.Value.ToString());
            }
            mLog.Info("Item:{0} Basic Info:{1}", info.ServerRelativeUrl, builder.ToString());
            stream.WriteMetadata(AveMetadataType.DocProperty, docInfo);
        }

        private void ExportFileRoleAssignments(IAveBackupStream stream, AveBRItemInfo info)
        {
            List<AveRoleAssignmentInfo> roleInfos = new List<AveRoleAssignmentInfo>();
            if (info.HasUniqueRoleAssignments)
            {
                string name;
                foreach (var role in info.RoleAssignments)
                {
                    var oldInfo = InfoConverter<AveRoleAssignmentInfo>.ConvertToCommonInfo(role);
                    if (Controller.GlobalCache.TryGetRoleDefintionNameById(role.RoleId, out name))
                    {
                        oldInfo.RoleName = name;
                        roleInfos.Add(oldInfo);
                    }
                    else
                    {
                        mLog.Info("Cannot get role name of Id:{0}", role.RoleId.ToString());
                        continue;
                    }
                }
            }
            //[DEBUG]
            mLog.Info("Item {0} HasUniqueRoleAssignments = {1}", info.ServerRelativeUrl, info.HasUniqueRoleAssignments.ToString());
            stream.WriteMetadata(AveMetadataType.RoleAssignment, roleInfos);
        }

        private void ExportFileColumnValues(IAveBackupStream stream, AveBRItemInfo info)
        {
            Dictionary<string, object> userData = new Dictionary<string, object>();

            userData["Author"] = info.Author;
            userData["Editor"] = info.Editor;
            userData["Modified"] = info.Modified;
            userData["Created"] = info.Created;
            //userData["#tp_ContentTypeId"] = info.ContentTypeId;
            userData["#tp_ModerationStatus"] = info.ModerationStatus;
            userData["_ModerationComments"] = info.ModerationComments;
            if (info.CustomColumns != null)
            {
                foreach (var kv in info.CustomColumns)
                {
                    if (this.mParentList.CustomColumns.Contains(kv.Key))
                    {
                        userData[kv.Key] = kv.Value;
                    }
                }
            }
            stream.WriteMetadata(AveMetadataType.DocData, userData);
        }

        private ProcessResult ExportRoleAssignment(IAveBackupStream stream)
        {
            ProcessResult result = new ProcessResult();
            if (this.mHasUniqueRoleAssignments)
            {
                var forderRoleAssignmentInfo = this.mController.GetFolderRoleAssignmentsInfo(this.mWebUrl, this.mServerRelativeUrl);
                if (forderRoleAssignmentInfo == null)
                {
                    throw new Exception("Failed to backup folder role assignment info");
                }
                if (forderRoleAssignmentInfo.Count > 0)
                {
                    List<AveRoleAssignmentInfo> infos = new List<AveRoleAssignmentInfo>();
                    forderRoleAssignmentInfo.ForEach(roleAssignmentInfo =>
                    {
                        infos.Add(InfoConverter<AveRoleAssignmentInfo>.ConvertToCommonInfo(roleAssignmentInfo));
                    });
                    stream.WriteMetadata(AveMetadataType.RoleAssignment, infos);
                }
            }
            return result;
        }

        protected override void FillCacheData(ProcessResult result)
        {

        }

        protected override void AddFakeData(ProcessResult result)
        {

        }

        protected virtual void OnMetadataExporting(ExportFileEventArgs args)
        {
            FireEvents(FileMetadataExporting, args);
        }

        protected virtual void OnMetadataExported(ExportFileEventArgs args)
        {
            FireEvents(FileMetadataExported, args);
        }

        protected virtual void OnContentExporting(ExportFileEventArgs args)
        {
            FireEvents(FileContentExporting, args);
        }

        protected virtual void OnContentExported(ExportFileEventArgs args)
        {
            FireEvents(FileContentExported, args);
        }

        protected virtual void OnAddingReport(ExportFileEventArgs args)
        {
            FireEvents(AddingReport, args);
        }

        protected virtual void OnFilteringOutFile(ExportFileEventArgs args)
        {
            FireEvents(FilteringOutFile, args);
        }

        private void FireEvents(ExportFileAction processAction, ExportFileEventArgs args)
        {
            if (processAction != null)
            {
                Array.ForEach<Delegate>(processAction.GetInvocationList(),
                action =>
                {
                    try
                    {
                        action.DynamicInvoke(this, args);
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("Errors occurred while invoking events, Error:{0} ", e);
                    }
                });
            }
        }

        private ExportFileEventArgs BuildEventArgs(string webUrl, string serverRelativeUrl, int rowId, Guid uniqueId, string name, long contentLength, int failedCount, Dictionary<string, object> userData, ProcessResult result)
        {
            return new ExportFileEventArgs
            {
                WebUrl = webUrl,
                ServerRelativeUrl = serverRelativeUrl,
                RowId = rowId,
                UniqueId = uniqueId,
                Name = name,
                ContentLength = contentLength,
                FailedCount = failedCount,
                UserData = userData,
                Result = result
            };
        }

        public List<ProcessResult> ExportFiles(IAveBackupStream stream, BackupOption options)
        {
            List<ProcessResult> results = new List<ProcessResult>();
            if (!ValidItemCount())
            {
                mLog.Info("No items in folder: {0}", this.mServerRelativeUrl);
                return results;
            }

            ProcessorConfig config = new ProcessorConfig
            {
                Name = this.Name,
                WebUrl = this.mWebUrl,
                FolderUrl = mServerRelativeUrl,
                TempFileSieThreshold = 20L * 1024 * 1024 * 1024,
                ProcessorType = ProcessorType.Unordered,
            };

            using (var processor = new AveDataProcessor(config, Controller))
            {
                processor.FailProcessor += FailProcessor;
                processor.StartProcess(QueryAndFilterFiles());

                foreach (var info in processor.Results)
                {
                    DecryptFile(info);

                    string tempName = info.Name;
                    if (!info.IsCurrent)
                    {
                        tempName = string.Format("{0}:{1}", info.Name, info.VersionLabel);
                    }
                    Dictionary<string, object> userData = SetUserData(info);
                    var args = BuildEventArgs(this.mWebUrl, info.ServerRelativeUrl, info.Id, info.UniqueId, tempName, info.Length, info.FailedCount, userData, info.Result);

                    OnMetadataExporting(args);
                    ExportMetadata(stream, info);
                    OnMetadataExported(args);

                    OnContentExporting(args);
                    ExportFileContent(stream, info);
                    OnContentExported(args);

                    OnAddingReport(args);
                    processor.ReleaseFileUsage(args.ContentLength);
                    results.Add(info.Result);
                    mLog.Info("Finished backing up item. Name:{0}. Version:{1}", info.Name, info.UIVersion.ToString());
                }
            }

            return results;
        }

        private void DecryptFile(AveBRItemInfo info)
        {
            if (StreamConvertor != null && mParentList.IrmEnabled)
            {
                try
                {
                    info.Content = StreamConvertor.Process(info.Content, info.Name);
                }
                catch (Exception ex)
                {
                    info.Content = null;
                    info.Length = 0L;
                    info.FailedCount++;
                    info.Result.SetFailed(ex);
                }
            }
        }

        private Dictionary<string, object> SetUserData(AveBRItemInfo info)
        {
            Dictionary<string, object> userData = new Dictionary<string, object>();
            string loginName;
            if (Controller.GlobalCache.TryGetUsername(info.Author, out loginName))
            {
                userData["Author"] = loginName;
            }
            if (Controller.GlobalCache.TryGetUsername(info.Editor, out loginName))
            {
                userData["Editor"] = loginName;
            }
            userData["Created"] = info.Created;
            return userData;
        }

        private void FailProcessor(object sender, ProcessorFailedEventArgs e)
        {
            var itemInfo = new AveBRItemInfo()
            {
                Name = "[Discovery Logic]",
                ServerRelativeUrl = Path.Combine(mServerRelativeUrl, "[Discovery Logic]")
            };
            itemInfo.Result.SetFailed(e.Exception);
            FailorSkipItem(itemInfo);
        }

        private void ExportMetadata(IAveBackupStream stream, AveBRItemInfo info)
        {
            ExportFileBasicInfo(stream, info);
            ExportFileColumnValues(stream, info);
            ExportFileRoleAssignments(stream, info);
        }

        private void ExportFileContent(IAveBackupStream stream, AveBRItemInfo info)
        {
            Stream content = info.Content;
            if (content != null)
            {
                long readContentCost = 0;
                long writeContentCost = 0;
                try
                {
                    System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                    byte[] buffer = new byte[BUFFERSIZE];
                    int length;
                    //stream.FlushMetadata((int)content.Length);
                    long readSize = 0;
                    while (readSize < content.Length)
                    {
                        watch.Start();
                        length = content.Read(buffer, 0, buffer.Length);
                        watch.Stop();
                        readContentCost += watch.ElapsedMilliseconds;
                        if (length == 0)
                        {
                            break;
                        }
                        readSize += length;
                        watch.Restart();
                        stream.WriteContent(buffer, 0, length);
                        watch.Stop();
                        writeContentCost += watch.ElapsedMilliseconds;
                        watch.Reset();
                    }
                }
                finally
                {
                    mLog.Info("Total read content time:{0} Milliseconds", readContentCost.ToString());
                    mLog.Info("Total write content time:{0} Milliseconds", writeContentCost.ToString());
                    content.Dispose();
                }
            }
            else
            {
                //stream.FlushMetadata(0);
            }
        }

        private bool ValidItemCount()
        {
            if (mSubFolders == null) return mItemCount > 0;

            return mItemCount > mSubFolders.Count;
        }

        private IEnumerable<AveBRItemInfo> QueryAndFilterFiles()
        {
            List<IItemFilter<AveBRItemInfo>> filters = new List<IItemFilter<AveBRItemInfo>>();
            filters.Add(new ItemStatusFilterOut(FailorSkipItem, FailorSkipItem));

            var lazyItems = GetFiles(mWebUrl, mParentList.FullUrl, mServerRelativeUrl, mParentList.IncludeItemVersions, false, true, mParentList.Columns);

            foreach (var item in lazyItems)
            {
                var filterOut = false;
                foreach (var filterAction in filters)
                {
                    if (filterAction.FilterOut(item))
                    {
                        filterOut = true;
                        break;
                    }
                }

                if (!filterOut)
                {
                    if (item.Versions != null && item.Versions.Count > 0)
                    {
                        if (VersionCount > 0 && VersionCount < item.Versions.Count)
                        {
                            item.Versions.RemoveRange(0, item.Versions.Count - VersionCount);
                        }
                        foreach (var version in item.Versions)
                        {
                            yield return version;
                        }

                        item.Versions = null;
                    }

                    yield return item;
                }
            }
        }

        protected virtual IEnumerable<AveBRItemInfo> GetFiles(string webUrl, string listUrl, string parentFolderUrl, bool includeVersions, bool includeVersionMetadata, bool includeSecurity, List<string> columns)
        {
            return mController.QueryLazyFiles(mWebUrl, mParentList.FullUrl, mServerRelativeUrl, includeVersions, includeVersionMetadata, includeSecurity, mParentList.Columns);
        }

        private void FailorSkipItem(AveBRItemInfo itemInfo)
        {
            string reportUrl = string.IsNullOrEmpty(itemInfo.ServerRelativeUrl) ? this.mServerRelativeUrl : itemInfo.ServerRelativeUrl;
            var args = BuildEventArgs(this.mWebUrl, reportUrl, itemInfo.Id, itemInfo.UniqueId, "[Discovery Logic]", itemInfo.Length, itemInfo.FailedCount, null, itemInfo.Result);
            OnFilteringOutFile(args);
        }

        public List<IAveBackupRestoreFolder> GetSubFolders()
        {
            List<IAveBackupRestoreFolder> subFolders = new List<IAveBackupRestoreFolder>();
            if (this.mSubFolders != null && this.mSubFolders.Count > 0)
            {
                foreach (var folder in this.mSubFolders)
                {
                    subFolders.Add(CreateSubFolder(this.WebUrl, folder.ServerRelativeUrl, this.mParentList, folder));
                }
            }
            return subFolders;
        }

        protected virtual IAveBackupRestoreFolder CreateSubFolder(string webUrl, string folderServerRelativeUrl, AveOD4BList parentList, AveBRFolderInfo folderInfo)
        {
            return new AveOD4BFolder(this.WebUrl, folderServerRelativeUrl, this.mParentList, folderInfo);
        }

        protected override List<AveBRChangeObject> GetChangedObjects()
        {
            throw new NotImplementedException();
        }
    }
}
