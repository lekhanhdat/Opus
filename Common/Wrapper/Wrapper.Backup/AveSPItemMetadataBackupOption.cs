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

namespace AvePoint.Wrapper.Backup
{
    public class SPItemMetadataBackupOption
    {
        /// <summary>
        /// 默认构造函数，Include User和Include Group默认为true
        /// </summary>
        public SPItemMetadataBackupOption()
        {
            IncludeUser = true;
            IncludeGroup = true;
        }

        /// <summary>
        /// backup related terms only
        /// </summary>
        public bool BackupRelatedTermsOnly { get; set; }

        /// <summary>
        /// backup related term sets
        /// </summary>
        public bool BackupRelatedTermSets { get; set; }

        /// <summary>
        /// Backup Lookup value GUID
        /// </summary>
        public bool BackupItemTPGUIDofLookupValue { get; set; }

        /// <summary>
        /// 是否包含User Cache，默认值为true
        /// </summary>
        public bool IncludeUser { get; set; }
        /// <summary>
        /// 是否包含Group Cache，默认值为true
        /// </summary>
        public bool IncludeGroup { get; set; }

        /// <summary>
        /// 是否备份Item全部UIVersion信息
        /// </summary>
        public bool IncludeAllUIVersions { get; set; }
    }
}
