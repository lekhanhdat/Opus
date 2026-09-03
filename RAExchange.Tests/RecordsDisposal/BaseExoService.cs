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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.RAExchange.Authorization;
using Castle.Windsor;
using ExchangeBackupUtility;
using ExchangeUtility;

namespace RAExchange.Tests.RecordsDisposal;

public abstract class BaseExoService
{
    protected IRMMailboxService MailboxService;
    protected BposInfo BposInfo;
    protected string Address = "dnlukj01@4y4q6l.onmicrosoft.com";
    
    protected BaseExoService()
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
            TenantLocalValue.LogonGroupId = "cabcb612-9046-4079-89af-19fcb279b32a";
            TenantLocalValue.LogonUserId = "f53c80c8-6929-459b-bef0-1d1ec1a4867b";
            MailboxService = PlatformWindsorManager.GetService<IRMMailboxService>();
            var keyValueDao = new RMKeyValueDao();
            var supportGraphApi = keyValueDao.GetValueByKeyAsync("EXOJOB_USING_GRAPH_API").Result;

            BposInfo = MailboxService.GetBPOSInfoByExchangeNode(new ExchangeOnlineTreeNodeDto()
                { ID = "0ce81853-0c66-4e46-a2ea-1ebb80e8f07c", UsingModernApp = bool.TryParse(supportGraphApi, out var flag) && flag});
        }
        catch (Exception e)
        {

        }
    }
}
