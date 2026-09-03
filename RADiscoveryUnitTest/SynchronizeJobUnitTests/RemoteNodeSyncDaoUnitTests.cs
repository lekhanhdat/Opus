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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core.Synchronize.DbManager;
using AvePoint.RA.DB.Dao.SynchronizeDao;
using AvePoint.RA.DB.Dao.SynchronizeDao.Imp;
using RMSynchronize.SyncNodeFromAOS;
using RMSynchronize.SyncNodeFromAOS.ChangeLog;
using RMSynchronize.SyncNodeFromAOS.Executors;

namespace RADiscoveryUnitTest.SynchronizeJobUnitTests;

[TestClass]
public class RemoteNodeSyncDaoUnitTests : SynchronizeJobInitializeTest
{
    public IRemoteNodeSynchronizeDao _remoteNodeSyncDao;
    
    [TestInitialize]
    public void Initialize()
    {
        RMSynchronizeDbManager.UpdateSqliteDbName("test.db");
        _remoteNodeSyncDao = new SqliteRemoteNodeSynchronizeDao();
    }
    
    [TestMethod]
    public async Task CrudForGoogleAndSPO_ShouldBeSuccess()
    {
        RMSyncNodeJobManager.Init("jobId");

        var changeLogger = new RMSyncNodeAzureChangeLogger(true, "jobId");
        var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
        var o365Tenants = await client.TenantManagementService.GetByTypeAsync(Cloud.Sdk.Data.AosModern.PlatformType.Office365);
        var googleTenants = await client.TenantManagementService.GetByTypeAsync(Cloud.Sdk.Data.AosModern.PlatformType.Google);
        List<RMSyncNodeExecutor> syncNodeExecutors = 
                 [
                    new RMSyncSharePointSiteNodeExecutor(client, o365Tenants, changeLogger), 
                    new RMSyncGoogleMyDriveContainerNodeExecutor(client, googleTenants, changeLogger), 
                    new RMSyncGoogleSharedDriveContainerNodeExcutor(client, googleTenants, changeLogger)
                 ];
        await RMSynchronizeDbManager.DownloadDatabaseAsync();

        foreach (var syncNodeExecutor in syncNodeExecutors)
        {
            await syncNodeExecutor.RunAsync();
        }
        
        var fileInfo = new FileInfo(RMSynchronizeDbManager.GetDbPath());
        
        Assert.IsTrue(fileInfo.Length > 32 * 1024);
    }
}