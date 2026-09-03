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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Query.AOSP.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Discovery.Job
{
    public class RMDiscoveryAOSPOptimizeDataSettingDto
    {
        public int ArchiveDataType { get; set; }
        public int DataType { get; set; }
        public string O365TenantId { get; set; }
        public StorageDeviceUIDto SelectedStorage { get; set; }
        public RMDiscoveryAOSPNodeQueryParameter NodeQueryParameter { get; set; }
        public RMDiscoveryAOSPSizeRangeQueryParameter SizeRangeQueryParameter { get; set; }
        public RMDiscoveryAOSPWithoutDateQueryParameter WithoutDateQueryParameter { get; set; }
        public RMDiscoveryAOSPFileExtensionQueryParameter FileExtensionQueryParameter { get; set; }
        public ScheduleParameter ScheduleParameter { get; set; }
        public ProcessActionParameter ProcessActionParameter { get; set; }
        public List<RMDiscoveryRuleDefinition> ROTRule { get; set; }
        public List<RMDiscoveryRuleDefinition> InactiveRule { get; set; }
    }
}
