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
using AvePoint.Media.StorageApi;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Service.Services.Discovery.Salesforce;
using Castle.MicroKernel.Proxy;
using Castle.Windsor;

namespace RADiscoveryUnitTest.SalesforceDiscoveryTests;

public abstract class SalesforceInitializeTest
{
    protected IRMDiscoverySalesforceConfigurationService ConfigurationService;
    protected IRMDiscoverySalesforceDataQueryService DataQueryService;
    protected IRMDiscoverySalesforceJobManagementService JobManagementService;
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
            TenantLocalValue.LogonGroupId = "cabcb612-9046-4079-89af-19fcb279b32a";
            StorageApiConfiguration.Setup();
            ISettingProfileService SettingProfileService = PlatformWindsorManager.GetService<ISettingProfileService>();
            ConfigurationService = PlatformWindsorManager.GetService<IRMDiscoverySalesforceConfigurationService>();
            DataQueryService = PlatformWindsorManager.GetService<IRMDiscoverySalesforceDataQueryService>();
            JobManagementService = new RMDiscoverySalesforceJobManagementService();
            //byte[] communicationKey = SettingProfileService.GetCommunicationEncryptionKey();
            //CspCommunicationWrapper.CommunicationEncryptionKey = communicationKey;
        }
        catch (Exception e)
        {

        }
    }
}