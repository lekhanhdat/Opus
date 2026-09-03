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

using AvePoint.Wrapper.Restore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Core.SPRestore
{
    /// <summary>
    /// SiteCollection的还原选项。
    /// </summary>
    public class SPSiteRestoreOption : SPObjectRestoreOption
    {
        /// <summary>
        /// 一般用于指定site创建用户，
        /// 如果给委托让外围修改，我们没办法判断外围是否修改了，并且没办法知道是否需要进行mapping，扩展性太强反而出现问题。
        /// 
        /// 所以暂时先提供参数，如果以后有类似需求再改进。
        /// </summary>
        public string SpecialSiteCreationAccount { get; set; }

        /// <summary>
        /// 标明当创建新site collection之后是否需要删除SP Objects，目前只支持删除默认的groups
        /// </summary>
        public bool CleanDefaultSPObjects { get; set; }

        /// <summary>
        /// 控制SiteCollection还原的数据内容。
        /// </summary>
        public SPContainerRestoreAction RestoreAction { get; set; }

        /// <summary>
        /// Site Deleted Event
        /// </summary>
        public Action SiteDeleted { get; set; }

        /// <summary>
        /// setting還原option
        /// </summary>
        public SPSiteConfigurationRestoreOption ConfigurationRestoreOption { get; set; }

        /// <summary>
        /// 包含特定的User和Group，还有User和Group的还原选项。
        /// </summary>
        public SPSecurityRestoreOption SecurityRestoreOption { get; set; }

        /// <summary>
        /// 控制ManagedMetadata的还原，主要是针对Term信息的选项。
        /// </summary>
        public SPManagedMetadataRestoreOption ManagedMetadataOption { get; set; }

        /// <summary>
        /// 控制UserProfile的还原。
        /// </summary>
        public SPUserProfileRestoreOption UserProfileOption { get; set; }

        /// <summary>
        /// 指定还原site的content db
        /// </summary>
        public Guid ContentDBId { get; set; }

    }

    /// <summary>
    /// site restore Action
    /// </summary>
    [Flags]
    public enum SPContainerRestoreAction : int
    {
        /// <summary>
        /// None，如果目的端不存在，则不还原
        /// </summary>
        None = 0,
        /// <summary>
        /// 如果目的端不存在，则创建；否则不创建。
        /// </summary>
        Skip,
        /// <summary>
        /// 如果目的端不存在，则创建；存在则使用
        /// </summary>
        Overwrite,
        /// <summary>
        /// 如果目的端存在，则先删除再还原
        /// </summary>
        Replace,
    }

    /// <summary>
    /// SiteCollection中user profile的还原选项。
    /// </summary>
    public class SPUserProfileRestoreOption
    {
        /// <summary>
        /// 控制是否还原User Profile
        /// </summary>
        public bool RestoreUserProfile { get; set; }
        /// <summary>需要备份Tag和Comment</summary>
        public bool EnableTagAndComment { get; set; }
        /// <summary>false： 如果UserProfile存在则不restore membership、tag、comment、colleague、details</summary>
        public bool Overwrite { get; set; }
    }

    /// <summary>
    /// Setting的还原选项
    /// </summary>
    public class SPSiteConfigurationRestoreOption
    {
        /// <summary>
        /// 控制是否需要还原Configuration,包括setting， feature等
        /// </summary>
        public bool RestoreConfiguration { get; set; }

        /// <summary>
        /// 控制是否还原Portal Site Connection
        /// </summary>
        public bool RestorePortalSiteConnection { get; set; }

        /// <summary>
        /// overwrite Search info，支持粒度是单个对象的overwrite，如果目的端keyword存在，但是源端不存在，则不会删除该keyword，如果有需求再添加。
        /// </summary>
        public bool OverwriteSearchInfo { get; set; }
    }
}
