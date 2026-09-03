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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Core.SPBackup
{
    #region common backup options
    public class SPWorkflowAsscioationBackupOption
    {
        public string ContentDBconnectionString { get; set; }
        public string ConfigDBconnectionString { get; set; }
        public bool ExportAssoiciation { get; set; }
        public bool ExportContentTypeAssoiciation { get; set; }
        public Func<AveWorkflowAssociationInfo, bool> FilterFunc { get; set; }
    }

    public class SPFieldBackupOption
    {
        public bool BackupRelatedTermSets { get; set; }
        public bool BackupRelatedTermsOnly { get; set; }
        public Action<AveFieldCollectionInfo> BeforeExportFieldsAction { get; set; }
    }

    public class SPContentTypeBackupOption
    {
        public Action<AveContentTypeCollectionInfo> BeforeExportConentTypesAction { get; set; }
    }
    #endregion

    #region web
    public class SPWebWorkflowBackupOption : SPWorkflowAsscioationBackupOption
    {
        public bool ExportInstance { get; set; }
        
    }

    public class SPWebFieldBackupOption : SPFieldBackupOption
    {
        // 不备份的fields, string: field的Name属性
        //public List<string> filterFields { get; set; }
    }

    public class SPNavigationOption
    {
        public bool BackupInheritedNavNodes { get; set; }
        /// <summary>
        /// 是否需要备份NavigationNode指向的Url的完全路径，CM会用到
        /// </summary>
        public bool NeedFullUrl { get; set; }
        /// <summary>
        /// 替换NavigationNode的WebApplication的Url，PRItem会用到
        /// </summary>
        public string SrcWebAppUrl { get; set; }
    }
    #endregion

    #region list
    public class SPListFieldBackupOption : SPFieldBackupOption
    {
        public bool IncludeGroups { get; set; }
    }
    #endregion

    #region site
    /// <summary>
    /// 备份SiteCollection的Managed metadata的选项。
    /// </summary>
    public class SPSiteManagedMetadataBackupOption
    {
        /// <summary>
        /// 是否要备份Global的Term Group。
        /// </summary>
        public bool IncludeGlobalTermGroup { get; set; }
        /// <summary>是否使用cache中的数据。
        /// 推荐使用false，老的接口默认使用false。使用从缓存中获取需要创建线程去check缓存，效率较低。
        /// false：一般流程，从系统获取数据。
        /// true：从缓存读取数据，如果没有再走一般流程从系统获取。
        /// </summary>
        public bool EnableCache { get; set; }
    }

    #endregion
}
