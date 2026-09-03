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

/*
 * 这个文件里面放restore会用到的一些公用的option
 */
namespace AvePoint.Wrapper.Core.SPRestore
{
    using AvePoint.Wrapper.Restore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

     [Flags]
    public enum SPWFTemplateFileConflictRules :byte
    {
        KeepTarget = 0,
        KeepSource = 1,
    }

    [System.Flags]
    public enum SPWFAssociationConflictResolutionOption : byte
    {
        /// <summary>
        /// association不会被restore
        /// </summary>
        NotOverwrite = 0,
        /// <summary>
        /// 重新命名association，直到不冲突。命名规则是: [backed up association name]_[number]
        /// </summary>
        Append = 1,
        /// <summary>
        /// 如果目的端association上没有workflow instance，则将目的端association删除后再restore；如果有instance，则skip
        /// </summary>
        Overwrite = 2,
        /// <summary>
        /// 无论目的端的association是否有instance，都先将其删除，然后重新restore；
        /// </summary>
        ForceOverwrite = 3,
        /// <summary>
        /// 不会删除目的端association，但会更新目的端association的一些配置属性；
        /// </summary>
        UpdateOverwrite = 4,
        /// <summary>
        /// 这个option是为instance所用。当还原instance时，如果其parent association没有被还原，
        /// 将在这个过程中重新还原parent association。为了使instance能够还原回去，
        /// 要保证其parent association不会被其本身的冲突规则再次skip掉，因此加了这个Option。
        /// </summary>
        ForceUse = 5
    }

    /// <summary>
    /// SP workflow restore option
    /// </summary>
    public class SPWFInstanceRestoreOption
    {
        /// <summary>
        ///check文件是不是新创建，或者冲突选项为Overwrite
        /// </summary>
        public bool NeedCheckRestoreOption { get; set; }
        /// <summary>
        /// Restore Instance
        /// </summary>
        public bool RestoreInstance { get; set; }
        /// <summary>
        /// Restore parent association
        /// </summary>
        public bool RestoreParentAssociationIfNotFound { get; set; }

        /// <summary>
        /// Running workflow restore action
        /// </summary>
        public SPRunningWorkflowRestoreAction RunningWorkflowRestoreAction { get; set; }

        /// <summary>
        /// Conflict Resolution Option
        /// </summary>
        public SPWFInstanceConflictResolutionOption ConflictResolutionOption { get; set; }
    }

    /// <summary>
    /// Running workflow restore action
    /// </summary>
    [Flags]
    public enum SPRunningWorkflowRestoreAction
    {
        /// <summary>
        /// Skip
        /// </summary>
        Skip,
        /// <summary>
        /// Restart
        /// </summary>
        Restart,
        /// <summary>
        /// Keep Running
        /// </summary>
        KeepRunning,
    }

    public class SPWFAssociationRestoreOption
    {
        /// <summary>
        /// Restore workflow association
        /// </summary>
        public SPObjectRestoreAction RestoreAction { get; set; }

        /// <summary>
        /// Association Conflict Resolution Option
        /// </summary>
        public SPWFAssociationConflictResolutionOption ConflictResolutionOption { get; set; }

        /// <summary>
        /// WF template conflict Rules
        /// </summary>
        public SPWFTemplateFileConflictRules TemplateFileConflictRules { get; set; }
    }

    [Flags]
    public enum SPWFInstanceConflictResolutionOption : byte
    {
        NotOverwrite = 0,
        Overwrite = 1,
        OverwriteByModifiedTime = 2
    }


    public class SPWorkflowRestoreOption
    {
        /// <summary>
        /// 还原Workflow association的选项。
        /// </summary>
        public SPWFAssociationRestoreOption AssociationRestoreOption { get; set; }

        /// <summary>
        /// 还原Workflow Instance的选项。
        /// </summary>
        public SPWFInstanceRestoreOption InstanceRestoreOption { get; set; }
    }
}
