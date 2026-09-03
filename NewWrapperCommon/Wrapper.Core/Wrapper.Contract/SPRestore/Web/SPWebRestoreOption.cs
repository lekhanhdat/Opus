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

using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Restore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Core.SPRestore
{
    public class SPWebRestoreOption : SPObjectRestoreOption
    {
        /// <summary>
        /// 控制还原内容。
        /// </summary>
        public SPContainerRestoreAction RestoreAction { get; set; }

        /// <summary>
        /// Conflict Check Option
        /// </summary>
        public SPWebConflictCheckOption ConflictCheckOption { get; set; }



        /// <summary>
        /// Web Deleted Event
        /// </summary>
        public Action WebDeleted { get; set; }

        /// <summary>
        /// 还原Web Basic Info前的Action，便于外围修改或者特殊逻辑处理
        /// </summary>
        public Action<AveWebInfo> BeforeBasicInfoAction { get; set; }

        /// <summary>
        /// 还原Web Basic Info 后的Action，让外围获取某些必须信息和修改相关Options
        /// </summary>
        public Action<AveWebRestoreBasicInfo> AfterBasicInfoAction { get; set; }



        /// <summary>
        /// Configuration Restore Option
        /// </summary>
        public SPWebConfigurationRestoreOption ConfigurationRestoreOption { get; set; }

        /// <summary>
        /// 包含特定的User、Group和roleAssignments，还有对应的还原选项。
        /// </summary>
        public SPWebSecurityRestoreOption SecurityRestoreOption { get; set; }

        /// <summary>
        /// 控制ManagedMetadata的还原，主要是针对Term信息的选项。
        /// </summary>
        public SPManagedMetadataRestoreOption ManagedMetadataOption { get; set; }

        /// <summary>
        /// 还原Workflow的选项。
        /// </summary>
        public SPWorkflowRestoreOption WorkflowRestoreOption { get; set; }     

        /// <summary>
        /// 还原Navigation 的option
        /// </summary>
        public SPNavigationRestoreOption NavigationRestoreOption { get; set; }
    }


    /// <summary>
    /// Conflict Check Option
    /// </summary>
    public enum SPWebConflictCheckOption : int
    {
        /// <summary>
        /// 不进行检查，直接根据action进行还原
        /// </summary>
        None = 0,

        /// <summary>
        /// 检查Recycle Bin，如果有数据，则认为冲突
        /// </summary>
        CheckRecycleBin = 1,
    }

    /// <summary>
    /// Web Configuration Restore option
    /// </summary>
    public class SPWebConfigurationRestoreOption
    {
        /// <summary>
        /// 控制是否需要还原Configuration,包括setting， feature等
        /// </summary>
        public bool RestoreConfiguration { get; set; }

        /// <summary>
        /// 是否还原regional settings
        /// </summary>
        public bool IsRestoreWebRegionalSettings { get; set; }

        /// <summary>
        /// 控制Fields的逻辑
        /// </summary>
        public SPObjectRestoreAction FieldRestoreAction { get; set; }

        /// <summary>
        /// Process Field Action
        /// </summary>
        public Func<string, string> ProcessFieldAction { get; set; }

        /// <summary>
        /// 还原Field的选项。
        /// </summary>
        public AveFieldRestoreOption FieldRestoreOption { get; set; }

        /// <summary>
        /// 控制ContentType的逻辑
        /// </summary>
        public SPObjectRestoreAction ContentTypeRestoreAction { get; set; }

        /// <summary>
        /// Process Content Type Action
        /// </summary>
        public Action<AvePoint.Wrapper.Common.AveContentTypeCollectionInfo> ProcessContentTypeAction { get; set; }

        /// <summary>
        /// 还原ContentType的选项。
        /// </summary>
        public AveContentTypeRestoreOption ContentTypeRestoreOption { get; set; }

        /// <summary>
        /// Content Type的display Name映射
        /// 在执行还原后被赋值
        /// </summary>
        public Dictionary<string, string> ContentTypeNameMapping { get; set; }
    }

    /// <summary>
    /// Web中users、groups和roleAssignments的还原选项。
    /// </summary>
    public class SPWebSecurityRestoreOption : SPSecurityRestoreOption
    {
        /// <summary>
        /// 控制是否还原Permission Level
        /// </summary>
        public bool RestorePermissionLevel { get; set; }
    }

    /// <summary>
    /// 还原Navigation node的选项
    /// </summary>
    public class SPNavigationRestoreOption
    {
        /// <summary>
        /// 控制是否还原navigation node。
        /// </summary>
        public bool NeedRestoreNavigation { get; set; }
        
        /// <summary>
        /// 控制是否保留那些指向无效节点的navigation node。
        /// </summary>
        public bool ForceKeepInvalidNode { get; set; }
        
        /// <summary>
        /// 目的端是root web，并且源端web的navigation setting是share时，控制是否将源端navigation强制还原到目的端root web上。
        /// 如果强制还原，可能会出现无效的navigation node。
        /// </summary>
        public bool IsMoveInheritNavigationNode { get; set; }

    }
}
