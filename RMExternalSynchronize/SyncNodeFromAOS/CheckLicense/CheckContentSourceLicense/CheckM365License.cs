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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMSynchronize.SyncNodeFromAOS.CheckLicense.CheckContentSourceLicense
{
    internal class CheckM365License : ContentSourceInterface.CheckLicense
    {
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private readonly RALogger _logger = RALogger.GetInstance(typeof(CheckM365License));

        public CheckM365License (ContentSourceInterface.ContentSource contentSource)
        {
            ContentSource = contentSource;
        }
        public override List<SourceFlag> GetSourceFlags()
        {
            var mOpusILLicense = TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, PaidForProduct.OpusIL);
            _logger.Info($"Has M365 license is OpusIL: {mOpusILLicense}");
            if (mOpusILLicense)
            {
                return mOpusILLicense ? [.. ContentSource.GetSourceFlags(), SourceFlag.SharePoint, SourceFlag.OneDrive, SourceFlag.Exchange , SourceFlag.Teams] : ContentSource.GetSourceFlags();
            }

            var mOpusSOLicense = TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, PaidForProduct.OpusSO);
            _logger.Info($"Has M365 license is OpusSO: {mOpusSOLicense}");
            try
            {
                if (!mOpusSOLicense && !TenantService.IsNewOpusTenant())
                {
                    var licenseInfo = RMAosApiClient.GetLicenseInfo(TenantLocalValue.LogonGroupId).GetAwaiter().GetResult();
                    _logger.Info($"Get license info from AOS: {licenseInfo.AdditionalProduct}");
                    mOpusSOLicense = licenseInfo.AdditionalProduct.HasFlag(PaidForProduct.OpusSO);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Get license info from AOS failed: {ex}");
            }
            if (mOpusSOLicense)
            {
                return mOpusSOLicense ? [.. ContentSource.GetSourceFlags(), SourceFlag.SharePoint, SourceFlag.OneDrive , SourceFlag.Teams] : ContentSource.GetSourceFlags();
            }

            var mOpusDiscoveryLicense = TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, PaidForProduct.OpusDiscovery);
            _logger.Info($"Has M365 license is OpusDiscovery: {mOpusDiscoveryLicense}");
            if (mOpusDiscoveryLicense)
            {
                return mOpusDiscoveryLicense ? [.. ContentSource.GetSourceFlags(), SourceFlag.SharePoint, SourceFlag.OneDrive , SourceFlag.Teams] : ContentSource.GetSourceFlags();
            }
            return ContentSource.GetSourceFlags();
        }
    }
}
