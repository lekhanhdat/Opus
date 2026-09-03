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
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.ArchiverCommon
{
    public enum ExchangeCacheNodeType
    {
        Group = 0,
        Mailbox = 3,
        Folder = 5,
        Item = 700,
    }
    public enum ExchangeMailboxType
    {
        None = 0,
        PublicFolder = 1,
        User = 2,
        Group = 3,
    }

    public enum SPNodeLevel
    {
        SiteCollection = 1,
        APP = 2,
        Web = 3,
        List = 1000,
        Folder = 1002,
        Item = 10000,
        ItemVersion = 10001,
        Attachment = 20000,
        Document = 50000,
        DocumentVersion = 50001,
        FitParentRule = 70000,
    }

    public enum ActionType
    {
        ArchiverAndRemove = 0,
        ArchiverAndKeepData = 1,
        ExportBeforeArchiver = 2,
        ExportOnly = 3,//need support future.
        KeepDataOnly = 4,
        BackupOnly = 5,
        Move = 6,
        ExportBeforeKeepDataOnly = 7,
        DeleteOnly = 8,
        ExportBeforeDelete = 9,
        ArchchiveToStorage = 10,
        DeleteDocumentToRecyleBinOnly = 11,
        ArchiveByMicrosoft = 12,
    }

    internal enum HoldAndRecordStatusMask
    {
        EditBlockedMask = 1, //只要不允许编辑, 这位值就为1, 包括Hold 和 Block edit and delete
        RecordMask = 0x10, //Record 文件，这位值 就是1 ， 包含Block edit and delete， block delete
        DeleteBlockedMask = 0x100,//只要不允许删除，这位值就为1, 包括 Hold， block edit and delete， block delete
        HoldMask = 0x1000, //Hold 文件，这位值就是 1， 
    }   

    public enum ItemDependencyOption
    {
        NotRestore = 0,
        Overwrite = 1,
        Append = 2,
        SkipConfilctItem = 3,
    }


    [Flags]
    public enum RecordRestrictions
    {
        None,
        BlockDelete,
        BlockEdit
    }

    public enum KeepDataStatus
    {
        Delete = 0,
        TagContent = 1,
        LeaveOnlyStub = 2,
        DeclareRecord = 4,
        Keep = 8,
        LockConversation = 16,
        Vault = 64
    }

    public enum SOVaultDBStatus
    {
        WaitingApprove = 1,
        HasBeenReported = 2,
        Approved = 3,
        Rejected = 4,
        Vaulted = 5,
        Failed = 6,
        Rescan = 7,
    }
    //用来区分数据源
    public enum SOSourceFlag
    {
        None = 0,
        SharePoint = 1,
        GroupMailbox = 2,
        Group = 3,
        PhysicalObject = 4,
        SPLocal = 5,
        OneDrive = 6,//only for records
        Teams = 11,
        GoogleDrive = 12,
    }

    public enum ProcessResult
    {
        Default,
        SkipCurrentNode,
        FitRule,
        FitParentRule,
        SkipListNode,
        CurrentVersionHasApprove,
        Continue
    }

    public enum ProcessType
    {
        NeedProcess,
        NoNeedProcess,
    }
    //public enum JobDetailsStatus
    //{

    //}
    public enum ScheduleProcedure { Scan, Backup, Deletion, Restore, Process, EndUserBackup, VaultScan, VaultExport, TestRun, PhysicalRecords }

    /// <summary>
    /// This is used in Item Backup.
    /// </summary>
    public enum NodeType
    {
        Undefine = 0,
        Root = -2,
        Farm = -1,
        WebApp = 2,
        Site = 100,
        Web = 200,
        App = 201,
        List = 300,
        DocList = 5, //
        Folder = 400,
        MyProfileList = 7, //
    }

    /// <summary>
    /// This is used in check rule
    /// </summary>
    public enum ItemType
    {//对应的值请不要改
        UNKNOW_TYPE = 0,
        DOCUMENT = 1,
        DOCUMENT_VER = 2,
        ITEM_TYPE = 4,
        ITEM_VERSION = 5,
        ATTACHMENT = 6,
        FOLDER_DOCLIB = 101,
        FOLDER_LIST = 102,
        FOLDER_SYSTEM = 109,
        FOLDER_VERSION = 103,
    }
}
