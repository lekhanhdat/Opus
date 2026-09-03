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
using AvePoint.RA.Contract;
using AvePoint.Wrapper.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.ArchiverCommon
{
    public class ArchiveApproveReport
    {
        public string PartitionKey { get; set; }

        public string EntityRowKey { get; set; }

        public string NodeId { get; set; }


        public string LeafName { get; set; }


        public string FullPath { get; set; }


        public string ParentId { get; set; }

        public string ScanJobID { get; set; }
        public string SortTicks { set; get; }//use for ReadFrom db method
        public long ScanTime { get; set; }

        public long ArchivedTime { get; set; }

        public long LastModifiedTime { get; set; }


        public int UIVersion { get; set; }


        public int LibRowId { get; set; }


        public int NodeType { get; set; }


        public int SPNodeLevel { get; set; }


        public int CacheNodeType { get; set; }


        public int ArchiveLevel { get; set; }


        public SOApproveDBStatus Status { get; set; }

        public bool IsRepeatProcess { get; set; }

        public bool IsCheckOption { get; set; }

        public byte Level { get; set; }


        public string RuleId { get; set; }

        public string RuleName { get; set; }

        public string RuleArchiverAction { get; set; }
        #region Google
        public string TermId { get; set; }
        #endregion
        public bool DoDelete { get; set; }

        public List<int> ItemIDs { get; set; }

        public bool IsAppData { get; set; }

        public string AppDataName { get; set; }

        #region for Archiver TestRun
        public long DocumentSize { set; get; }

        public long Created { set; get; }

        public string CreatedBy { set; get; }

        public long Modified { set; get; }

        public string ModifiedBy { set; get; }

        public bool ActionTaken { set; get; }

        public string SiteUrl { get; set; }

        public Guid WebID { get; set; }

        public Guid ListID { get; set; }

        #endregion

        public string Metadata { get; set; }

        public string JsonMeta { get; set; }//存放数据源的Json

        public ManifestDocumentSnapshot ManifestDocumentSnapshot { get; set; }

        //可以考虑当做插入DB 时候的属性，也可以考虑Init Job 的时候在schedule config 里面放一个全局 sourcetype
        //因为操作DB 有读和写两个操作，读的时候是没有Detail 对象的，所以有个全局对象控制Source 类型会统一 一些
        public int SourceFlag { get; set; }

        public string SiteTitle { get; set; }

        public int HasRelatedDocument { get; set; }//记录是否存在RelatedRecord ， >1 表示存在，0 表示不存在

        public int DeleteRelatedRecords { get; set; }//标记是否在删除文件的同时，删除RelatedRecord. 1 means delated related record, 0 means skip

        public string RelatedRecordInfo { get; set; }

        #region MailBox Object
        public Guid MailBoxID { get; set; }
        public string MailSubject { get; set; }
        public string MailFrom { get; set; }
        public string MailBoxAddress { get; set; }
        public int MailboxType { get; set; }
        public long DateTimeReceived { get; set; }
        public long DateTimeCreated { get; set; }
        public string MailDisplayPath { get; set; }
        #endregion
        public IAveListItem ItemObject { get; set; }
        public string StubInfo { get; set; }
        public string StubId { get; set; }
        public bool ShouldAddDetails { get; set; }
        public string Author { get; set; }
        public string Editor { get; set; }

        public int AuthorId { get; set; }

        public int EditorId { get; set; }

        public bool IsArchiveBy365 { get; set; }

        public bool IsRelativeDataJob { get; set; }
    }

    public class RecordsOneDriveExplorerCache
    {
        public Guid ListId { get; set; }
        public string ItemName { get; set; }
        public Guid NodeId { get; set; }
        public string TermName { get; set; }
        public Guid TermId { get; set; }
    }

    internal sealed class ApproveDBPartitionKeyComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            string tempx = x;
            string tempy = y;
            if (!x.EndsWith("Manual"))
            {
                tempx = string.Format("{0}{1}", x, "Manual");
            }
            if (!y.EndsWith("Manual"))
            {
                tempy = string.Format("{0}{1}", y, "Manual");
            }
            return string.Equals(tempx, tempy, StringComparison.CurrentCultureIgnoreCase);
        }

        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
    }

    public class MetadataCacheInfo
    {
        public string TextFieldName { get; set; }
        public Guid TextFieldId { get; set; }
        public Dictionary<string, string> TermValueMapping { get; set; }

        public MetadataCacheInfo()
        {
            TermValueMapping = new Dictionary<string, string>();
        }
    }

    public class TagInfoCollection
    {
        public object Value = string.Empty;
        public string Key = string.Empty;
    }

    public class ManifestDocumentSnapshot
    {
        public ManifestSiteInfo Site { get; set; }

        public ManifestListInfo List { get; set; }

        public ManifestFolderInfo Folder { get; set; }

        public string DocumentAccessUrl { get; set; }

        public string DocumentServerRelativeUrl { get; set; }

        public bool IsSystemFile { get; set; }

        public bool HasUniqueRoleAssignments { get; set; }

        public bool IsVersion { get; set; }

        public long DocumentSize { get; set; }

        public string FileTitle { get; set; }

        public Dictionary<string, object> ColumnValues { get; set; }

        public IList<TagInfoCollection> TagInfoOverrides { get; set; }

        public IList<ManifestMetadataEntry> MetadataEntries { get; set; }

        public string ContentFilePath { get; set; }

        public byte[] ContentBytes { get; set; }

        public string RecordsRelatedValue { get; set; }

        public bool EnableHsm { get; set; } = true;

        public string PathMd5 { get; set; }

        public string FileServerRelativeUrl { get; set; }

        public string StubId { get; set; }

        public string ScopeUrl { get; set; }

        public DateTime? CreatedTime { get; set; }

        public DateTime? ModifiedTime { get; set; }

        public string Author { get; set; }

        public string AuthorString { get; set; } = string.Empty;

        public string Editor { get; set; }

        public string EditorString { get; set; } = string.Empty;

        public int AuthorId { get; set; }

        public int EditorId { get; set; }

        public bool SkipRelatedRecords { get; set; }

        // Azure blob lazy download support
        public string ContentBlobPrefix { get; set; }
        public string StorageContainerName { get; set; }
        public string StorageConnectionString { get; set; }

        public string ContentStorageConnectionString { get; set; }
        public string Version { get; set; }
        public long TotalSize { get; set; }
    }

    public sealed class ManifestMetadataEntry
    {
        public string Type { get; set; }

        public object? Data { get; set; }
    }

    public sealed class ManifestSiteInfo
    {
        public string Url { get; set; }

        public string ServerRelativeUrl { get; set; }
    }

    public sealed class ManifestFolderInfo
    {
        public string ServerRelativeUrl { get; set; }

        public string Path { get; set; }
    }

    public sealed class ManifestListInfo
    {
        public Guid Id { get; set; }

        public int BaseTemplate { get; set; }

        public bool Hidden { get; set; }

        public bool IsCatalog { get; set; }
    }

    public class CacheItemDto
    {
        public IAveListItem CacheItem { get; set; }

        public int ArchiverLevel { get; set; }

        public AveListTemplateType BaseTemplate { get; set; }

        public string Url { get; set; }
    }

    public class BackupIAveListItemCacheDto
    {
        public IAveListItem CacheItem { get; set; }

        public long CacheTime { get; set; }

        public string ItemId { get; set; }

    }

    internal class KeyWord
    {
        internal static string TYPE = "type";
        internal static string PATH = "path";
        internal static string HEADERTYPE = "fileHeaderType";
        internal static string TIME = "archivedTime";
        internal static string ID = "spId";
        internal static string RowId = "rowId";
        internal static string LEVEL = "level";
        internal static string VERSION = "UIVersion";
        internal static string WEBAPP = "webApp";
        internal static string PROFILE = "isMyProfileList";
        internal static string NODEGUID = "nodeGuid";
        internal static string SYSTEMFILE = "isSystemFile";
        internal static string BACKUPTYPE = "backupType";
        internal static string SiteUrl = "siteUrl";
        internal static string WebId = "webId";
        internal static string ListId = "listId";
        internal static string ISVERSION = "isVersion";
        internal static string MYLEVEL = "myLevel";
        internal static string SIZE = "size";
        internal static string URL = "url";
        internal static string RULENAME = "ruleName";
        internal static string SUBJOBID = "subJobId";
        internal static string MEDIANAME = "mediaName";
        internal static string FULLPATH = "fullPath";//for Error page ,give a FullPath
        internal static string scopeId = "scopeId";
        internal static string isInheritPermission = "isInheritPermission";
        internal static string permissions = "permissions";
        internal static string CompatibilityLevel = "compatibilityLevel";  //SAAS-10848 在创建SiteCollection的时候，需要用到这几个属性。
        internal static string LCID = "lcid";
        internal static string Owner = "owner";
        internal static string Template = "template";
        internal static string Title = "title";
        internal static string AppDataName = "appDataName";
        internal static string IsAppData = "isAppData";
        internal static string ParentId = "parentId"; //SAAS-23014 删除attachment时，获取所属listItem时，此属性为判断条件
        internal static string EndUserJobId = "endUserJobId"; //SAAS-32843 删除Related Document 过程使用
        internal static string DoDelete = "DoDelete";
        internal static string DeleteRelatedRecords = "DeleteRelatedRecords";
        internal static string HasUniqueRoleAssignments = "HasUniqueRoleAssignments";

        internal static string Created = "Created";
        internal static string Modified = "Modified";
        internal static string Author = "Author";
        internal static string Editor = "Editor";
        internal static string StubInfo = "stubInfo";
        #region Exchange Mailbox
        internal static string Name = "name";
        internal static string ParentFullPath = "parentFullPath";
        internal static string NodeType = "nodeType";
        internal static string DataType = "dataType";
        #endregion
    }

    public static class ArchiverErrorMessage
    {
        public static string BCSFieldNotFoundString = "StorageOptimization_SOARRecordManagerEXOListBCSNotExist";

        public static string TermNotExistString = "StorageOptimization_SOARRecordManagerEXOSourceTermNotExist";

        public static string NotUnderTermScopeString = "StorageOptimization_SOARRecordManagerEXONotInSameTermScope";

        public static string TermSettingNotFoundString = "StorageOptimization_SOARRecordManagerEXOTermSettingNotFound";
    }
}
