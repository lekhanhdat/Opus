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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.SyncNode;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.TenantUpgrade;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Services.Tenant.Upgrade;
using Cloud.Sdk.AosModern;
using Cloud.Sdk.Data.AosModern;
using RazorEngine.Compilation.ImpromptuInterface.Dynamic;
using RMSynchronize.SyncNodeFromAOS.ChangeLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Core.Synchronize.DbContext;
using AvePoint.RA.DB.Core.Synchronize.DbContext.Base;
using AvePoint.RA.DB.Dao.GoogleSyncNodeDao;

namespace RMSynchronize.SyncNodeFromAOS.Executors
{
    public abstract class RMSyncNodeExecutor
    {

        protected static readonly string s_tenantId = TenantLocalValue.LogonGroupId;

        protected static readonly IRMSyncNodeDao s_syncNodeDao = PlatformWindsorManager.GetService<IRMSyncNodeDao>();
        
        protected static readonly IRMGoogleSyncNodeDao s_syncGoogleNodeDao = PlatformWindsorManager.GetService<IRMGoogleSyncNodeDao>();

        protected readonly RALogger _logger;

        protected readonly AosModernApiTenantClient _tenantClient;

        protected readonly List<TenantConnectionInfo> _tenantConnectionInfoes;

        protected readonly RMSyncNodeAzureChangeLogger _changeLogger;

        protected abstract NodeLevel RecordContainerNodeLevel { get; }

        public abstract SourceFlag ContentSource { get; }

        public abstract RemoteNodeType AosNodeType { get; }
        
        private readonly IRemoteNodeEvent _remoteNodeEvent;

        public RMSyncNodeExecutor(AosModernApiTenantClient tenantClient, List<TenantConnectionInfo> tenantConnectionInfoes, RMSyncNodeAzureChangeLogger changeLogger)
        {
            _logger = RALogger.GetInstance(GetType());
            _tenantClient = tenantClient;
            _tenantConnectionInfoes = tenantConnectionInfoes;
            _changeLogger = changeLogger;
            _remoteNodeEvent = new RemoteNodeEvents();
            s_syncNodeDao.InjectRemoteNodeSynchronizeEvent(_remoteNodeEvent);
            s_syncGoogleNodeDao.InjectRemoteNodeSynchronizeEvent(_remoteNodeEvent);
        }

        protected abstract Task SyncNodeAsync(RMContainerInfoAdaption containerInfo);

        protected abstract Task UpgradeNodeAsync(RMContainerInfoAdaption containerInfo);

        protected abstract Task DeleteContainers(IEnumerable<RMContainerInfoAdaption> containerInfoes);

        protected abstract Task AddContainers(IEnumerable<RMContainerInfoAdaption> containerInfoes);

        protected abstract Task UpdateContainers(IEnumerable<RMContainerInfoAdaption> containerInfoes);

        protected abstract Task<IEnumerable<RMContainerInfoAdaption>> GetRecordContainers();

        public async Task UpgradeAsync()
        {
            _logger.Info($"Start upgrade [{ContentSource} - {AosNodeType}] node.");

            var containers = await SyncContainerAsync();

            try
            {
                foreach (var container in containers)
                {
                    await UpgradeNodeAsync(container);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while upgrade [{ContentSource} - {AosNodeType}] node. Error: {e}");
            }
        }

        public async Task RunAsync()
        {
            try
            {
                _logger.Info($"Start sync [{ContentSource} - {AosNodeType}] node.");

                var containers = await SyncContainerAsync();

                foreach (var container in containers)
                {
                    await SyncNodeAsync(container);
                }

                _logger.Info($"Successful sync [{ContentSource} - {AosNodeType}] node.");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while sync [{ContentSource} - {AosNodeType}] node. Error: {e}");
            }
        }

        private async Task<IEnumerable<RMContainerInfoAdaption>> SyncContainerAsync()
        {
            var aosContainers = (await GetContainersAsync()).ToHashSet();
            ReNameContainers(aosContainers);

            _logger.Debug($"[{ContentSource} - {AosNodeType}] containers count: [{aosContainers.Count}].");

            var recordContainers = await DeleteNotExistsInAosContainersAsync(await GetRecordContainers());

            var needDeleteContainers = recordContainers.Except(aosContainers).ToHashSet();
            _logger.Debug($"[{ContentSource} - {AosNodeType}] need delete containers count: [{needDeleteContainers.Count}].");
            await DeleteContainers(needDeleteContainers);
            RMSyncNodeJobManager.AddSucceedJobDetail(ContentSource, RMSyncNodeAction.Delete, needDeleteContainers);
            await _changeLogger.Record(needDeleteContainers, ContentSource, RMSyncNodeChangeType.Delete);

            var needAddedContainers = aosContainers.Except(recordContainers).ToHashSet();
            _logger.Debug($"[{ContentSource} - {AosNodeType}] need add containers count: [{needDeleteContainers.Count}].");
            await AddContainers(needAddedContainers);
            RMSyncNodeJobManager.AddSucceedJobDetail(ContentSource, RMSyncNodeAction.Add, needAddedContainers);

            var intersectContainers = recordContainers.Intersect(aosContainers).ToHashSet();

            var needUpdateContainers = await intersectContainers.ToAsyncEnumerable().WhereAwait(async intersectContainer =>
            {
                var hasChange = false;
                var existContainer = aosContainers.First(aosContainer => intersectContainer.AosId.Equals(aosContainer.AosId, StringComparison.OrdinalIgnoreCase));

                if(!intersectContainer.Name.Equals(existContainer.Name))
                {
                    _logger.Debug($"[Container [aos id: {existContainer.AosId}] has been changed name.");
                    await _changeLogger.RecordChangeName(intersectContainer, ContentSource, intersectContainer.Name, existContainer.Name);
                    hasChange = true;
                }
                
                intersectContainer.Name = existContainer.Name;
                return hasChange;
            }).ToHashSetAsync();

            _logger.Debug($"[{ContentSource} - {AosNodeType}] need update containers count: [{needUpdateContainers.Count}].");
            await UpdateContainers(needUpdateContainers);
            RMSyncNodeJobManager.AddSucceedJobDetail(ContentSource, RMSyncNodeAction.Update, needUpdateContainers);

            return intersectContainers.Union(needAddedContainers);
        }

        protected virtual async Task<IEnumerable<RMContainerInfoAdaption>> GetContainersAsync()
        {
            var aosContainersResult = await _tenantClient.ContainerService.GetByTypeAsync(AosNodeType);
            var aosContainers = aosContainersResult.ConvertAll(item => new RMContainerInfoAdaption
            {
                Id = item.Id,
                Name = item.Name,
                NodeLevel = RecordContainerNodeLevel,
                AosId = item.Id,
            });

            return aosContainers;
        }

        protected static AvePoint.GCommon.Contract.CentralAdmin.Object.AppType ConvertIdentityTypeToAppType(IdentityProviderType providerType)
        {
            var appType = AvePoint.GCommon.Contract.CentralAdmin.Object.AppType.Office365;
            switch (providerType)
            {
                case Cloud.Sdk.Data.AosModern.IdentityProviderType.CloudRecords:
                    appType = AvePoint.GCommon.Contract.CentralAdmin.Object.AppType.CloudRecords;
                    break;
                case Cloud.Sdk.Data.AosModern.IdentityProviderType.Office365:
                    appType = AvePoint.GCommon.Contract.CentralAdmin.Object.AppType.Office365;
                    break;
                case Cloud.Sdk.Data.AosModern.IdentityProviderType.SharePoint:
                    appType = AvePoint.GCommon.Contract.CentralAdmin.Object.AppType.SharePoint;
                    break;
                case Cloud.Sdk.Data.AosModern.IdentityProviderType.Exchange:
                    appType = AvePoint.GCommon.Contract.CentralAdmin.Object.AppType.Exchange;
                    break;
                case Cloud.Sdk.Data.AosModern.IdentityProviderType.CustomAzureApp:
                    appType = AvePoint.GCommon.Contract.CentralAdmin.Object.AppType.CustomAzureApp;
                    break;
                case Cloud.Sdk.Data.AosModern.IdentityProviderType.CustomDelegateApp:
                    appType = AvePoint.GCommon.Contract.CentralAdmin.Object.AppType.CustomDelegateApp;
                    break;
            }
            return appType;
        }

        protected virtual void ReNameContainers(IEnumerable<RMContainerInfoAdaption> containerInfoes)
        {

        }

        private async Task<HashSet<RMContainerInfoAdaption>> DeleteNotExistsInAosContainersAsync(IEnumerable<RMContainerInfoAdaption> containers)
        {
            var list = containers.ToList();
            var needDeleteContainers = list.Where(item => string.IsNullOrWhiteSpace(item.AosId)).ToList();
            if(needDeleteContainers.Any())
            {
                _logger.Debug($"Not exists in AOS containers count: [{needDeleteContainers.Count}].");
                await DeleteContainers(needDeleteContainers);
            }
            return list.Where(item => !string.IsNullOrWhiteSpace(item.AosId)).ToHashSet();
        }
    }

}
