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
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Configurations
{
    public enum RMConfigType
    {
        CommonSetting = 1,
        AppSettings = 2,
        StorageSettings = 3,
        DatabaseSettings = 4,
        EnvSettings = 5,
    }


    public class JobCountConfig
    {
        //public int DefaultCountForAllJob = 2;
        public List<TenantConfig> Tenants;
    }

    public class TenantConfig
    {
        public Guid TenantId;
        public List<JobConfig> JobConfigs;
        /// <summary>
        /// used in UniqueId job
        /// </summary>
        public string UniqueIdJobSearchSiteColumnFieldName;
        /// <summary>
        /// used in QuniqueId job
        /// </summary>
        public string UniqueIdJobSearchListColumnFieldName;
    }

    public class JobConfig
    {
        public int JobType;
        public int SubJobCount;
    }

    public class CustomApp
    {
        public Guid TenantId;
        public string AppClientId;
    }

    public class CustomAppConfigs
    {
        public List<CustomApp> CustomApps;
    }

    public class FileExtentionsConfig
    {
        public bool EnableExclusion { get; set; }
        public List<string> FileExtensions { get; set; }
    }

    #region Security Related Config
    /// <summary>
    /// security config中目前只有CSPHeaderConfig，后续有其它安全配置可以加进来
    /// </summary>
    public class SecurityConfig
    {
        public CSPHeaderConfigItems CSPHeaderConfig;
    }

    /// <summary>
    /// 目前只支持FormAction，添加自定义Source
    /// </summary>
    public class CSPHeaderConfigItems
    {
        public List<SourceItem> CustomFormActionsSources;
    }

    public class SourceItem
    {
        /// <summary>
        /// eg: 在xml文件中：<ItemValue>*.sharepointguild.com</ItemValue>
        /// </summary>
        public string ItemValue;
    }
    #endregion
}
