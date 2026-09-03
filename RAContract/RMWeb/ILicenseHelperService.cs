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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RoleAssignments;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface ILicenseHelperService
    {
        string CustomerId { get; }
        Task<bool> IsNewOpus(bool checkTenantExist = false, bool useCache = true);
        Task<bool> UpdateLicense(bool isInit, bool disableSO = false, bool isMigrationJob = false);
        Task<RAReturnMessage> ValidateLicense();

        bool IsEnableDeleteRestoreDataFeature();

        Task<bool> IsAvePointStorage();
        Task<bool> IsCloudArchivingByos();

        Task<bool> IsEnableMaestroAI();
        Task<bool> ForceEnableSO();
        bool HasOpusILLicense { get; }
        bool HasOpusSOLicense { get; }

        bool HasOpusDiscoveryLicense { get; }
        bool HasOpusGoogleLicense { get; }
        bool HasOpusSalesforceDiscoveryLicense { get; }
        bool HasOpusGoogleROTDiscoveryLicense { get; }
        bool HasOpusFileSystemDiscoveryLicense { get; }
        bool HasOpusSPILOrSOLicense { get; }
        bool HasGoogleControlLicense { get; }
        Task<long> GetUpgradeOpusTime();
        bool CheckAdditionalDataSource(PaidForModule module);
    }

}
