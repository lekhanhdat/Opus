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
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.Media.StorageApi;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.Tenant;
using Castle.MicroKernel.Proxy;
using Castle.Windsor;

namespace RADiscoveryUnitTest.GoogleServiceUnitTests;

public class GoogleServiceInitializeTest
{
    [TestInitialize]
    public void Init()
    {
        try
        {
            RALogger.ConfigFile = "TimerLog4net.config";
            var logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
            RMGlobalConfiguration.Init();
            string installPath = AppDomain.CurrentDomain.BaseDirectory;
            WindsorContainer windsorContainer = new WindsorContainer();
            windsorContainer.Install(Castle.Windsor.Installer.Configuration.FromXmlFile(
                Path.Combine(installPath, "Castle/ServiceCastle.config")));
            var selector = windsorContainer.Resolve<IModelInterceptorsSelector>("AvePoint.RA.Common.Audit.AuditInterceptorSelector");
            windsorContainer.Kernel.ProxyFactory.AddInterceptorSelector(selector);
            PlatformWindsorManager.SetUp(windsorContainer);
            AosApiUtility.Init(RMCertificateHelper.GetCertificate(RMCertNames.AvePointRecords));
            TenantLocalValue.LogonGroupId = "89838aa6-7023-4bb0-8ca7-a6ed43ed2ebd";
            //TenantLocalValue.LogonGroupId = "f1a44437-8070-4d86-80ed-b4d587cdd3d3";
            //f1a44437-8070-4d86-80ed-b4d587cdd3d3
            StorageApiConfiguration.Setup();
            ISettingProfileService SettingProfileService = PlatformWindsorManager.GetService<ISettingProfileService>();
            byte[] communicationKey = SettingProfileService.GetCommunicationEncryptionKey();
            CspCommunicationWrapper.CommunicationEncryptionKey = communicationKey;
        }
        catch (Exception e)
        {

        }
    }
}