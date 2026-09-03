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

using System.Collections.Generic;

namespace AvePoint.Wrapper.Core.SPRestore
{
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Restore;
    using System;

    /// <summary>
    /// restore list option
    /// </summary>
    public class SPListRestoreOption : SPObjectRestoreOption
    {
        /// <summary>
        /// Restore Action
        /// </summary>
        public SPContainerRestoreAction RestoreAction { get; set; }

        /// <summary>
        /// Conflict Check Option
        /// </summary>
        public SPListConflictCheckOption ConflictCheckOption { get; set; }

        /// <summary>
        /// Find option
        /// </summary>
        public SPListFindOption FindOption { get; set; }

        /// <summary>
        /// 避免还原到同一个list
        /// </summary>
        public bool AvoidToRestoreSameList { get; set; }

        /// <summary>
        /// Verify list template before creating the list.
        /// </summary>
        public bool VerifyListTemplateFeature { get; set; }

        /// <summary>
        /// Process Basic Info Action, 还原List Basic Info前的Action，便于外围修改或者特殊逻辑处理
        /// </summary>
        public Action<AvePoint.Wrapper.Common.AveListInfo> BeforeBasicInfoAction { get; set; }

        /// <summary>
        /// List Deleted Event
        /// </summary>
        public Action ListDeleted { get; set; }

        /// <summary>
        /// 还原List Basic Info 后的Action，让外围获取某些必须信息和修改相关Options
        /// </summary>
        public Action<AveListRestoreBasicInfo> AfterBasicInfoAction { get; set; }


        /// <summary>
        /// Configuration Restore Option
        /// </summary>
        public SPListConfigurationRestoreOption ConfigurationRestoreOption { get; set; }

        /// <summary>
        /// Security Restore Option
        /// </summary>
        public SPSecurityRestoreOption SecurityRestoreOption { get; set; }

        /// <summary>
        /// 控制ManagedMetadata的还原，主要是针对Term信息的选项。
        /// </summary>
        public SPManagedMetadataRestoreOption ManagedMetadataOption { get; set; }

        /// <summary>
        /// 还原Workflow的选项。
        /// </summary>
        public SPWorkflowRestoreOption WorkflowRestoreOption { get; set; }
    }

    /// <summary>
    /// Find option for list
    /// </summary>
    [Flags]
    public enum SPListFindOption : int
    {
        /// <summary>
        /// Title
        /// </summary>
        Title = 1,
        /// <summary>
        /// Url
        /// </summary>
        Url = 2,
        /// <summary>
        /// Title and url
        /// </summary>
        TitleAndUrl = 4,
    }

    /// <summary>
    /// Conflict Check Option
    /// </summary>
    public enum SPListConflictCheckOption : int
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
    public class SPListConfigurationRestoreOption
    {
        /// <summary>
        /// 控制是否需要还原Configuration,包括setting， feature等
        /// </summary>
        public bool RestoreConfiguration { get; set; }

        /// <summary>
        /// 还原Connector setting
        /// </summary>
        public bool RestoreConnectorSettings { get; set; }

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
        /// 还原ContentType的选项。
        /// </summary>
        public AveContentTypeRestoreOption ContentTypeRestoreOption { get; set; }

        /// <summary>
        /// Process Content Type Action
        /// </summary>
        public Action<AvePoint.Wrapper.Common.AveContentTypeCollectionInfo> ProcessContentTypeAction { get; set; }

        /// <summary>
        /// Content Type的display Name映射
        /// 在执行还原后被赋值
        /// </summary>
        public Dictionary<string, string> ContentTypeNameMapping { get; set; }
    }
}

