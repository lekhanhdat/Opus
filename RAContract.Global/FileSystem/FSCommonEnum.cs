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
namespace AvePoint.RA.Contract.FileSystem
{
    public enum FSJobType
    {
        None = 0,
        UserFullJob = 1,
        RematchRuleFullJob = 2,
        IncrementalJob = 3
    }
    public enum TermConflictOption
    {
        None = 0,
        Skip = 1,
        Overwrite = 2
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

    //public enum KeepDataOption
    //{
    //    Delete = 0,
    //    TagContent = 1,
    //    LeaveOnlyStub = 2,
    //    DeclareRecord = 4,
    //    LockConversation = 8,
    //    Keep = 16,
    //    Remove = 32
    //}
    public enum SOApproveDBStatus
    {
        None = 0,
        WaitingApprove = 1,
        HasBeenReported = 2,
        Approved = 3,
        Rejected = 4,
        Archived = 5,
        Failed = 6,
        Rescan = 7,
        KeepData = 8,
        CheckOption = 9,
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
        SharePoint = 1,
        GroupMailbox = 2,
        Group = 3,
        PhysicalObject = 4,
        OnPremSP = 5,
    }

    public enum RuleAction
    {
        None = 0,
        ArchiveAndRemove = 1,
        ArchiveAndKeep = 2,
        MoveAndDeclare = 3,
        ExportOnly = 4
    }

    public enum DetailTab
    {
        Deletion,
        MoveTo
    }

    public enum DetailType
    {
        Folder,
        Document
    }

    public enum BackupRestoreStatus
    {
        Succeed = 0, //成功
        Failed = 1, //失败
        Skipped = 2, //由于parent失败，而跳过
        UnKnown = 3, //没有返回结果
        UnProcess = 99, //for wpp CG Scan only.
    }

    public enum DetailAction
    {
        None = 0,
        Scan = 1,
        ArchiveAndMove = 2,
        UpdateManual = 3,
        Destroy = 4,
    }
}
