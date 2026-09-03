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
using Cloud.Sdk.Data.AosModern;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Aos
{
    public class RMAosLicenseInfo
    {
        public bool Enable { get; set; }

        public LicenseType Type { get; set; }

        public PaidForModule AdditionalDataSource { get; set; }

        public PaidForProduct AdditionalProduct { get; set; }

        public bool EnableAutoClassification { get; set; }

        public List<RMAosRelatedProductLicense> RelatedProductLicenses { get; set; } = new();

        public SOStorageLicenseInfo StorageLicenseInfo { get; set; }

        public OpusDiscoveryLicenseInfo DiscoveryLicenseInfo { get; set; }
        
        public OpusDiscoveryLicenseInfo SalesforceDiscoveryLicenseInfo { get; set; }
        public OpusDiscoveryLicenseInfo GoogleROTDiscoveryLicenseInfo { get; set; }
        public OpusDiscoveryLicenseInfo FSDiscoveryLicenseInfo { get; set; }
    }

    public class RMAosRelatedProductLicense
    {
        public RelatedProductType ProductType { get; set; }
        public bool LicenseExpired { get; set; }
        public bool Byos { get; set; }
    }

    public enum RelatedProductType
    {
        None,
        CloudArchiving
    }

    public class SOStorageLicenseInfo
    {
        //判断是否为Byos
        public bool Byos { get; set; }

        //目前SO只有UnlimitedUser 设置了Storage 限制, UnlimitedStorage 设置了user seat 限制
        public SaleType SaleType { get; set; }

        //每个user seat购买多少Size，对于SO来说，实际Storage size 计算时需要再默认加1，和cloud archive逻辑一致
        public int CustomerSize { get; set; }

        //如果是UnlimitedUser时，代表买了多少TB storage，如果是UnlimitedStorage，代表这买了多少user seat
        public int UserSeat { get; set; }

        public bool EnableContentSearch { get; set; }
    }

    public class OpusDiscoveryLicenseInfo
    {
        //GB
        public int TenantTotalSize { get; set; }

        public int FrequencyPerYear { get; set; }
    }
}
