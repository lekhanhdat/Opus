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
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Restore;

namespace AvePoint.Wrapper.Core.SPRestore
{

    /// <summary>
    /// using find type and find value to find special item
    /// find sequence: string -> int -> GUID -> object
    /// if find type is 
    ///     leafName         -> string (findValue)
    ///     RowId            -> int (findIntValue)
    ///     TpGuid          -> GUID (findGuidValue)
    ///     Title            -> string (findValue)
    ///     CustomColumn     -> string -> int -> GUID -> object
    /// </summary>
    public class SPItemFindOption
    {
        /// <summary>
        /// Find Type
        /// </summary>
        public SPItemFindType FindType { get; set; }
        /// <summary>
        /// Find Value
        /// </summary>
        public string FindValue { get; set; }
        /// <summary>
        /// Find Int Value
        /// </summary>
        public int? FindIntValue { get; set; }
        /// <summary>
        /// Find Guid Value
        /// </summary>
        public Guid? FindGuidValue { get; set; }
        /// <summary>
        /// Find Obj value
        /// </summary>
        public object FindObjValue { get; set; }

        /// <summary>
        /// Find column name
        /// </summary>
        public string FindColumnName { get; set; }
    }

    /// <summary>
    /// find type
    /// </summary>
    public enum SPItemFindType
    {
        /// <summary>
        /// File Title
        /// </summary>
        LeafName,
        /// <summary>
        /// List Item Row Id
        /// </summary>
        RowId,
        /// <summary>
        /// TP_GUID
        /// </summary>
        TpGuid,
        /// <summary>
        /// title
        /// </summary>
        Title,
        /// <summary>
        /// special column value
        /// </summary>
        CustomColumn,
    }

    /// <summary>
    /// conflict option
    /// 根据check option来获取对应的restore action.
    /// 
    /// 1. 如果custom conflict handler不为null，则每进行一次检查，都会调用对应的方法，然后根据对应的返回值来决定是否继续
    /// 2. 如果checkOption为空，则检查customConflicthandler是否为空，如果不为空，则根据该函数返回的结果来还原。
    /// 3. 如果没有设置custom conflict handler,和 custom conflict result handler,则遍历CheckOptions，
    ///    如果第一个不冲突，则继续第二个检查，直到结束或者发现冲突为止，然后根据结果来决定选择冲突action还是非冲突action
    /// 4. 如果不需要检查冲突，则设置非冲突action即可，我们使用这个作为default值。
    /// </summary>
    public class SPItemConflictOption<T>
    {
        /// <summary>
        /// a list conflict check option
        /// </summary>
        public IList<SPItemConflictCheckOption> CheckOptions { get; set; }

        /// <summary>
        /// 自定义的conflict handler
        /// </summary>
        public Func<T, SPItemRestoreAction> CustomConflictHandler;

        /// <summary>
        /// custom conflict handler
        /// 传入的参数是检查的option和对应的结果 true: 表示冲突，false表示不冲突
        /// 返回值是: 第一个返回值:true表示继续检查下一个option，false表示不检查；第二个返回值: 对应的action
        /// </summary>
        public Func<SPItemConflictCheckOption, bool, Tuple<bool, SPItemRestoreAction>> CustomConflictResultHandler;

        /// <summary>
        /// non conflict action, default value when no conflict checks
        /// </summary>
        public SPItemRestoreAction NonConflictAction { get; set; }

        /// <summary>
        /// conflict action
        /// </summary>
        public SPItemRestoreAction ConflictAction { get; set; }
    }

    public class SPItemRestoreOption : SPObjectRestoreOption
    {
        /// <summary>
        /// Find Option to find the related item
        /// </summary>
        public SPItemFindOption FindOption { get; set; }

        ///// <summary>
        ///// Restore Action for the conflict check action
        ///// </summary>
        //public SPItemRestoreAction RestoreAction { get; set; }

        ///// <summary>
        ///// Conflict Check Option
        ///// </summary>
        //public SPItemConflictCheckOption ConflictCheckOption { get; set; }

        ///// <summary>
        ///// Handle item conflict option
        ///// </summary>
        //public SPItemConflictHandleOption ConflictHandleOption { get; set; }

        /// <summary>
        /// 控制都还原Item关联的那些Metadata Info
        /// </summary>
        public SPItemRestoreConfiguration RestoreConfiguration { get; set; }

        /// <summary>
        /// Metadata Restore Option
        /// </summary>
        public SPItemMetadataRestoreOption MetadataRestoreOption { get; set; }

        /// <summary>
        /// RoleAssignments Restore Option
        /// </summary>
        public SPRoleAssignmentsRestoreOption RoleAssignmentsRestoreOption { get; set; }

        /// <summary>
        /// workflow restore option
        /// </summary>
        public SPWorkflowRestoreOption WorkflowRestoreOption { get; set; }

        /// <summary>
        /// Filter User Info
        /// </summary>
        public Action<AvePoint.Wrapper.Common.AveUserList> FilterUserInfo { get; set; }

        /// <summary>
        /// Filter Group Info
        /// </summary>
        public Action<AvePoint.Wrapper.Common.AveGroupList> FilterGroupInfo { get; set; }
    }

    public class SPItemRestoreConfiguration
    {
        //public SPObjectRestoreAction ItemBasic { get; set; }

        public SPObjectRestoreAction Security { get; set; }

        public SPObjectRestoreAction Alerts { get; set; }

        public SPObjectRestoreAction SocialTag { get; set; }

        public SPObjectRestoreAction SocialComment { get; set; }

        public SPObjectRestoreAction DocumentTagging { get; set; }

        public SPObjectRestoreAction WorkflowInstance { get; set; }

        public SPObjectRestoreAction WorkflowSchedule { get; set; }
    }

    public class SPFileRestoreOption : SPItemRestoreOption
    {
        /// <summary>
        /// 处理源端的备份信息
        /// </summary>
        public Func<SPBackupDto.SPDocumentMetadataDto, bool> ProcessFileMetadataDto { get; set; }

        /// <summary>
        /// Conflict Option
        /// </summary>
        public SPItemConflictOption<IAveFile> ConflictOption { get; set; }
                        
        /// <summary>
        /// 控制还原GhostPage Content 和 Path的选项
        /// </summary>
        public RestoreGhostPageOption GhostPageOption { get; set; }

        /// <summary>
        /// Connector inplace restore时, 如果List被整体删除，Blob数据是永久保存的，还原时为了避免数据多份，需要Overwrite掉
        /// </summary>
        public bool OverWriteBlob { get; set; }

    }
    
    public class SPListItemRestoreOption : SPItemRestoreOption
    {
        /// <summary>
        /// 处理源端的备份信息
        /// </summary>
        public Func<SPBackupDto.SPListItemMetadataDto, bool> ProcessListItemMetadataDto;

        /// <summary>
        /// Conflict Option
        /// </summary>
        public SPItemConflictOption<IAveListItem> ConflictOption { get; set; }

        /// <summary>
        /// If the restore is for achiver
        /// </summary>
        public bool ArchiverRestoreMicroFeed { get; set; }
    }

    public class SPFolderRestoreOption : SPItemRestoreOption
    {
        /// <summary>
        /// 处理Folder Basic Info
        /// </summary>
        public Action<Dictionary<string, object>> ProcessBasicInfoAction { get; set; }

        /// <summary>
        /// Restore Action
        /// </summary>
        public SPFolderRestoreAction RestoreAction { get; set; }

        /// <summary>
        /// Delete Action
        /// </summary>
        public Action FolderDeleted { get; set; }

    }

    public class SPAttachmentRestoreOption : SPObjectRestoreOption
    {
        private Func<SPBackupDto.SPAttachmentMetadataDto, bool> processAttachmentFunc;
        public Func<SPBackupDto.SPAttachmentMetadataDto, bool> ProcessAttachmentFunc
        {
            get { return processAttachmentFunc; }
            set { processAttachmentFunc = value; }
        }

        private Func<IAveAttachment, SPItemRestoreAction> attachmentConflictHandleFunc;
        public Func<IAveAttachment, SPItemRestoreAction> AttachmentConflictHandlFunc
        {
            get { return attachmentConflictHandleFunc; }
            set { attachmentConflictHandleFunc = value; }
        }
    }
    
    public sealed class SPItemMetadataRestoreOption
    {
        /// <summary>
        /// 是否需要验证page layout是否存在
        /// </summary>
        public bool VerifyPageLayout { get; set; }  

        /// <summary>
        /// 是否需要验证关联的column或者content type，包括MMS column
        /// </summary>
        public bool VerifyDependency { get; set; }

        /// <summary>
        /// 如果dependency不存在怎么处理。
        /// </summary>
        public SPItemMetadataDependencyNotFoundAction DependencyNotFoundAction { get; set; }

        /// <summary>
        /// 如果dependency冲突了怎么处理。
        /// </summary>
        public SPItemMetadataDependencyConflictAction DependencyConflictAction { get; set; }

        /// <summary>
        /// Content Type Restore Option
        /// </summary>
        public AveContentTypeRestoreOption ContentTypeRestoreOption { get; set; }

        /// <summary>
        /// Field Restore Option
        /// </summary>
        public AveFieldRestoreOption FieldRestoreOption { get; set; }

        /// <summary>
        /// 是否keep tp_GUID属性
        /// </summary>
        public bool KeepTP_GUID { get; set; }

        /// <summary>
        /// 是否需要keep删除之前的unique id或者row id。
        /// </summary>
        public bool KeepUniqueIdAndRowId { get; set; }

        /// <summary>
        /// 当create item时，是否需要保留目的端column的default value
        /// </summary>
        public bool KeepColumnDefaultValue { get; set; }

        /// <summary>
        /// 还原Item MMS column value的时候，如果Term在目的端不存在，是否要Force Add
        /// </summary>
        public bool IsForceAddTerm { get; set; }

        /// <summary>
        /// 使用源端的Lookup Value（不进行目的端Lookup Item RowId查找）
        /// </summary>
        public bool UseSourceLookupValue { get; set; }

        /// <summary>
        /// For folder, whether to restore SO property on folder
        /// </summary>
        public bool IsRestoreConnectorFolderProperties { get; set; }
    }

    /// <summary>
    /// folder restore Action
    /// </summary>
    [Flags]
    public enum SPFolderRestoreAction : int
    {
        /// <summary>
        /// None，如果目的端不存在，则不还原
        /// </summary>
        None = 0,
        /// <summary>
        /// 如果目的端不存在，则创建；否则不创建，并且不还原folder属性。
        /// </summary>
        CreateIfNotFound,
        /// <summary>
        /// 如果目的端不存在，则创建；否则不创建，并且还原folder属性,相当于merge功能。
        /// </summary>
        Default,
        /// <summary>
        /// 如果目的端存在，则先删除再还原
        /// </summary>
        Replace,
    }

    /// <summary>
    /// Item Metadata Dependency Not Found Action
    /// 目前只有两种方式，要么item不还原，要么就创建出对应的dependency
    /// </summary>
    [Flags]
    public enum SPItemMetadataDependencyNotFoundAction
    {
        /// <summary>
        /// Skip Item when dependency doesn't exist.
        /// </summary>
        SkipItem,
        /// <summary>
        /// Create dependency
        /// </summary>
        RestoreDependency,
    }

    /// <summary>
    /// Item Metadata Dependency Conflict Action
    /// </summary>
    [Flags]
    public enum SPItemMetadataDependencyConflictAction
    {
        /// <summary>
        /// Skip item when dependency conflict
        /// </summary>
        SkipItem,

        /// <summary>
        /// 根据后续的restore option来还原dependency
        /// </summary>
        RestoreDependency,
    }

    public enum RestoreGhostPageOption
    {
        /// <summary>
        /// default value
        /// </summary>
        NoAction = 0,
        KeepStreamOnly = 1,
        KeepPathOnly = 2,
        KeepStreamAndPath = 3,
    }
}
