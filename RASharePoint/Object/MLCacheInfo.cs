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
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.EnforceRetention;
using AvePoint.RA.SharePoint.ExplorerSync.Modes;
using AvePoint.RA.RACommonUtility.UniqueId;

namespace AvePoint.RA.SharePoint.Object
{

    public class AutoSmartCacheItemInfo
    { }

    public class SPAutoSmartCacheItemInfo : AutoSmartCacheItemInfo
    {
        public IAveList AveList { get; set; }
        public IAveListItem AveItem { get; set; }
        public IAveTaxonomyField AveTaxField { get; set; }
        public IAveORecords Records { get; set; }
        public RMSharePointSetting Setting { get; set; }
        public ConfigSiteSetting ConfigSiteSetting { get; set; }
        public SPOLabelUtility LabelUtility { get; set; }
        public Guid RemoteSiteId { get; set; }
    }

    public class OneDriveAutoSmartCacheItemInfo : AutoSmartCacheItemInfo
    {
        public IAveList AveList { get; set; }
        public IAveListItem AveItem { get; set; }
        public Guid ParentId { get; set; }
        public SyncItemRuleInfo ParentItemRule { get; set; }
        public List<string> ExcludePath { get; set; }
        public RMOneDriveSetting SpSetting { get; set; }
        public UniqueIdUtil IdUtil { get; set; }

        
    }

}
