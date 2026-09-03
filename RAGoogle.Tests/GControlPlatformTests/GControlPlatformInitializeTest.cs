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
using System.Reflection;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using Castle.Windsor;

namespace RAGoogleTests.GControlPlatformTests;

public abstract class GControlPlatformInitializeTest
{
    protected GControlPlatformInitializeTest()
    {
        try
        {
            RALogger.ConfigFile = "TimerLog4net.config";
            RMGlobalConfiguration.Init();
            string installPath = AppDomain.CurrentDomain.BaseDirectory;
            var logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
            WindsorContainer windsorContainer = new ();
            windsorContainer.Install(Castle.Windsor.Installer.Configuration.FromXmlFile(
                Path.Combine(installPath, "Castle/ServiceCastle.config")));
            PlatformWindsorManager.SetUp(windsorContainer);
            AosApiUtility.Init(RMCertificateHelper.GetCertificate(RMCertNames.AvePointRecords));
            TenantLocalValue.LogonGroupId = "478c3962-2325-4d32-beb0-f2c5c78acc3a";
            TenantLocalValue.LogonUserId = "f53c80c8-6929-459b-bef0-1d1ec1a4867b";
        }
        catch (Exception e)
        {

        }
    }
}