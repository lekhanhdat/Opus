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
using AvePoint.RA.Contract.Google.GControlPlatform;
using AvePoint.RA.Contract.Google.NexusGovernance;
using AvePoint.RA.Contract.Tenant;
using Castle.MicroKernel.Proxy;
using Castle.Windsor;
using Cloud.Sdk.Nexus.Governance;

namespace RADiscoveryUnitTest.GControlPlatformTests;

public abstract class GControlPlatformInitializeTest
{
    protected IGControlPlatformApprovalProcessService GControlPlatformApprovalProcessService;
    protected IGControlPlatformTaskService GControlPlatformTaskService;
    protected IGControlPlatformEmailTemplateService GControlPlatformEmailTemplateService;
    protected IGControlPlatformJobService GControlPlatformJobService;
    protected IGControlPlatformEmailService GControlPlatformEmailService;
    protected INexusGovernancePersonalSettingService NexusGovernancePersonalSettingService;

    [TestInitialize]
    public void Init()
    {
        try
        {
            RALogger.ConfigFile = "TimerLog4net.config";
            RMGlobalConfiguration.Init();
            string installPath = AppDomain.CurrentDomain.BaseDirectory;
            var logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
            WindsorContainer windsorContainer = new WindsorContainer();
            windsorContainer.Install(Castle.Windsor.Installer.Configuration.FromXmlFile(
                Path.Combine(installPath, "Castle/ServiceCastle.config")));
            var selector = windsorContainer.Resolve<IModelInterceptorsSelector>("AvePoint.RA.Common.Audit.AuditInterceptorSelector");
            windsorContainer.Kernel.ProxyFactory.AddInterceptorSelector(selector);
            PlatformWindsorManager.SetUp(windsorContainer);
            AosApiUtility.Init(RMCertificateHelper.GetCertificate(RMCertNames.AvePointRecords));
            TenantLocalValue.LogonGroupId = "9c32d55c-8bdd-4598-be95-b7284217a0ff";
            TenantLocalValue.LogonUserId = "a18229f9-7b3c-4042-88b3-428b80436d68";
            StorageApiConfiguration.Setup();
            GControlPlatformApprovalProcessService = PlatformWindsorManager.GetService<IGControlPlatformApprovalProcessService>();
            GControlPlatformTaskService = PlatformWindsorManager.GetService<IGControlPlatformTaskService>();
            GControlPlatformEmailTemplateService = PlatformWindsorManager.GetService<IGControlPlatformEmailTemplateService>();
            GControlPlatformJobService = PlatformWindsorManager.GetService<IGControlPlatformJobService>();
            GControlPlatformEmailService = PlatformWindsorManager.GetService<IGControlPlatformEmailService>();
            NexusGovernancePersonalSettingService = PlatformWindsorManager.GetService<INexusGovernancePersonalSettingService>();

        }
        catch (Exception e)
        {

        }
    }
}