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
using AvePoint.RA.Contract.RoleAssignments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AvePoint.RA.Web.Models.Resource
{
    public class ResourceItem
    {
        public ResourceKeys Key { get; set; }
        public string Value { get; set; }
        public RMPermissionMasks Permission { get; set; }
        public RMSubPermissionMasks SubPermission { get; set; }
        public RMPermissionExtensionMasks PermissionExtension{ get; set; }
        public RMReportPermissionMasks ReportPermission { get; set; }
        public RMSOPermissionMasks SOPermission { get; set; }
        public RMDiscoveryPermissionMasks DiscoveryPermission { get; set; }
        public RMDiscoverySalesforcePermissionMask SalesforceDiscoveryPermission { get; set; }
        public RMDiscoveryGoogleROTPermissionMask GoogleROTDiscoveryPermission { get; set; }
        public RMDiscoveryFileSystemPermissionMask FSDiscoveryPermission { get; set; }
        public bool IsCSDResource { get; set; }
    }


    public class UIResourceItem
    {
        public ResourceKeys Name { get; set; }
        public string Value { get; set; }
    }
}