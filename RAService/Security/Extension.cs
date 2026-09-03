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
using AvePoint.GCommon.Contract.CloudAppAdmin.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.RADataBroker.Common;
using Cloud.Sdk.Data.AosModern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Security
{
    public static class RMAosLicenseInfoExtension
    {
        public static bool ArchiverLicenseExpired(this RMAosLicenseInfo license)
        {
            var archiverLicense = license.RelatedProductLicenses?.FirstOrDefault(o => o.ProductType == RelatedProductType.CloudArchiving);
            return archiverLicense == null || archiverLicense.LicenseExpired;
        }

        public static bool IsOnlySOLicense(this RMAosLicenseInfo license)
        {
            var result = false;
            var hasOpusILLicense = license.AdditionalProduct.HasFlag(PaidForProduct.OpusIL);
            var hasOpusSOLicense = license.AdditionalProduct.HasFlag(PaidForProduct.OpusSO);
            var hasOpusGoogleLicense = license.AdditionalProduct.HasFlag(PaidForProduct.OpusGoogle);
            if (!hasOpusGoogleLicense && !hasOpusILLicense && hasOpusSOLicense)
            {
                result = true;
            }
            return result;
        }

    }
}

