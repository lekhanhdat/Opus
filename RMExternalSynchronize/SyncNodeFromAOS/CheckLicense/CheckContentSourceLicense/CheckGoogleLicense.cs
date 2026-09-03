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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;

namespace RMSynchronize.SyncNodeFromAOS.CheckLicense.CheckContentSourceLicense;

public  class CheckGoogleLicense : ContentSourceInterface.CheckLicense {
    private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
    
    private readonly RALogger _logger = RALogger.GetInstance(typeof(CheckGoogleLicense));

    public CheckGoogleLicense(ContentSourceInterface.ContentSource contentSource)
    {
        ContentSource = contentSource;
    }
    
    public override List<SourceFlag> GetSourceFlags()
    {
        var googleLicense =
            TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, PaidForProduct.OpusGoogle);
        var gControlGoogleLicense = TenantService.HasInitGControlPlatForm().GetAwaiter().GetResult();

        _logger.Info($"Has Google license is {googleLicense} and Has Google GControl License is {gControlGoogleLicense}");
       
        return googleLicense || gControlGoogleLicense ? [..ContentSource.GetSourceFlags(),SourceFlag.Google] : ContentSource.GetSourceFlags();
    }
}