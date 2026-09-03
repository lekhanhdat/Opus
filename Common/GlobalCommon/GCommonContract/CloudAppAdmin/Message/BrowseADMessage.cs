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

namespace AvePoint.GCommon.Contract.CloudAppAdmin.Message
{
    using AvePoint.GCommon.Contract.CloudAppAdmin.Object;
    using System.Collections.Generic;

    public class BrowseADMessage
    {
        public string TenantId { get; set; }
        public List<ADLicense> Licenses { get; set; }
        public List<ADApplication> Applications { get; set; }
        public List<ADRole> Roles { get; set; }
        public List<string> Domains { get; set; }
        public CAALoadDetailType CAALoadDetailType { get; set; }
    }

    //会根据取出的属性增加相应的属性
    public class ADRole
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string TemplateId { get; set; }
        public List<string> MemberObjectIds { get; set; }
        public string DisplayName { get; set; }
    }

    //只为显示使用的 简单Object
    public class SimpleRole
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
    }
}