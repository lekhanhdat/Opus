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
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.AzureFileShare;
using AvePoint.RA.Contract.AzureFileShare.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.AzureFileShare.Api;
using AvePoint.RA.Service.Services.AzureFileShare.AuditHandler;
using AvePoint.RA.Service.Services.AzureFileShare.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.RACommonUtility.Encryption;

namespace AvePoint.RA.Service.Services.AzureFileShare
{
    [Audit]
    public class RMAzureFileShareConnectionService : RMServiceBase, IRMAzureFileShareConnectionService
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RMAzureFileShareConnectionService));

        private IRMAzureFileShareConnectionDao  AzureFileShareConnectionDao => PlatformWindsorManager.GetService<IRMAzureFileShareConnectionDao>();

        public IRMAzureFileShareConnectionGroupDao AzureFileShareConnectionGroupDao => PlatformWindsorManager.GetService<IRMAzureFileShareConnectionGroupDao>();

        private IGeneralSettingService  GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService >();

        private Task<GeneralSettingModel> Gsl => GeneralSettingService.GetGeneralSettingAsync();

        public async Task<AzureFileShareConnectionItem> GetAsync(string name)
        {
            try
            {
                var connection = AzureFileShareConnectionDao.Get(name);
                return AzureFileShareConnectionConverter.Convert(connection, await Gsl);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get azure file share connection item [{name}]. Error: {e}");
                return null;
            }
        }

        public async Task<AzureFileShareConnectionItem> GetAsync(Guid id)
        {
            try
            {
                var connection = AzureFileShareConnectionDao.Get(id);
                return AzureFileShareConnectionConverter.Convert(connection, await Gsl);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while get azure file share connection item [{id}]. Error: {e}");
                return null;
            }
        }

        public async Task<List<AzureFileShareConnectionItem>> GetAllAsync(bool needSecret = false)
        {
            try
            {
                List<RMAzureFileShareConnection> connections;
                if (needSecret)
                {
                    connections = AzureFileShareConnectionDao.GetAll();
                }
                else
                {
                    connections = AzureFileShareConnectionDao.GetAllWithoutSecret();
                }
                return AzureFileShareConnectionConverter.Convert(connections, await Gsl);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get azure file share connections. Error: {e}");
                return new List<AzureFileShareConnectionItem>();
            }
        }

        public async Task<List<AzureFileShareConnectionItem>> GetAllAsync(List<Guid> ids)
        {
            try
            {
                var connections = AzureFileShareConnectionDao.GetAll(ids);
                return AzureFileShareConnectionConverter.Convert(connections, await Gsl);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get azure file share connections: [{string.Join(", ", ids)}]. Error: {e}");
                return new List<AzureFileShareConnectionItem>();
            }
        }

        public async Task<List<AzureFileShareConnectionItem>> GetAllByConnectionGroupAsync(Guid connectionGroupId)
        {
            try
            {
                var connections = AzureFileShareConnectionDao.GetAllByConnectionGroup(connectionGroupId);
                return AzureFileShareConnectionConverter.Convert(connections, await Gsl);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while get azure file share connections by group [{connectionGroupId}]. Error: {e}");
                return new List<AzureFileShareConnectionItem>();
            }
        }

        public async Task<List<AzureFileShareConnectionItem>> GetAllWithoutRelatedConnectionGroupAsync()
        {
            try
            {
                var connections = AzureFileShareConnectionDao.GetAllWithoutRelatedConnectionGroup();
                return AzureFileShareConnectionConverter.Convert(connections, await Gsl);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while get azure file share connection without related connection group. Error: {e}");
                return new List<AzureFileShareConnectionItem>();
            }
        }

        public bool Has(Guid id)
        {
            try
            {
                return AzureFileShareConnectionDao.Has(id);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while check connection: [{id}] has in record. Error: {e}");
                return false;
            }

        }

        public bool Remove(Guid id)
        {
            try
            {
                return AzureFileShareConnectionDao.Remove(id);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while remove azure file share connection [{id}]. Error: {e}");
                return false;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.AzureFileShareDeleteConnection, AfterHandler = typeof(AzureFileShareConnectionAfterAuditHandler), BeforeHandler = typeof(AzureFileShareConnectionBeforeAuditHandler))]
        public async Task<bool> RemoveAsync(List<Guid> ids)
        {
            try
            {
                return await AzureFileShareConnectionDao.RemoveAsync(ids);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while remove azure file share connections [{string.Join(", ", ids)}]. Error: {e}");
                return false;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.AzureFileShareCreateConnection, AfterHandler = typeof(AzureFileShareConnectionAfterAuditHandler), BeforeHandler = typeof(AzureFileShareConnectionBeforeAuditHandler))]
        public async Task<bool> CreateAsync(AzureFileShareConnectionItem connectionItem)
        {
            try
            {
                if (connectionItem == null)
                {
                    Logger.Warn($"The parameter [connectionItem] is null, can't invoke create action.");
                    return false;
                }

                var connection = AzureFileShareConnectionConverter.Convert(connectionItem, await Gsl);

                if (connection.ConnectionGroupId != Guid.Empty && !AzureFileShareConnectionGroupDao.Has(connection.ConnectionGroupId))
                {
                    Logger.Warn($"Can't find connection will related group [{connection.ConnectionGroupId}].");
                    return false;
                }

                connection.Created = DateTime.UtcNow.Ticks;
                connection.Modified = DateTime.UtcNow.Ticks;
                connection.CreatedBy = TenantLocalValue.LogonUserId;
                connection.ModifiedBy = TenantLocalValue.LogonUserId;
                return AzureFileShareConnectionDao.Add(connection);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while create azure file share connection. Error: {e}");
                return false;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.AzureFileShareEditConnection, AfterHandler = typeof(AzureFileShareConnectionAfterAuditHandler), BeforeHandler = typeof(AzureFileShareConnectionBeforeAuditHandler))]
        public async Task<bool> ModifyAsync(AzureFileShareConnectionItem connectionItem)
        {
            try
            {
                if (connectionItem == null)
                {
                    Logger.Warn($"The parameter [connectionItem] is null, can't invoke modify action.");
                    return false;
                }

                var connection = AzureFileShareConnectionConverter.Convert(connectionItem, await Gsl);

                if (connection.ConnectionGroupId != Guid.Empty && !AzureFileShareConnectionGroupDao.Has(connection.ConnectionGroupId))
                {
                    Logger.Warn($"Can't find connection will related group [{connection.ConnectionGroupId}].");
                    return false;
                }

                var conn = AzureFileShareConnectionDao.Get(connection.Id);
                connection.Created = conn.Created;
                connection.CreatedBy = conn.CreatedBy;
                connection.Modified = DateTime.UtcNow.Ticks;
                connection.ModifiedBy = TenantLocalValue.LogonUserId;
                return AzureFileShareConnectionDao.Modify(connection);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while modify azure file share connection: [{connectionItem?.Id}]. Error: {e}");
                return false;
            }
        }

        public bool Validate(AzureFileShareConnectionItem connectionItem)
        {
            try
            {
                var connectionInfo = AzureFileShareConnectionConverter.ConvertToConnectionInfo(connectionItem);
                var apiContext = new AzureFileShareApiContext(connectionInfo);
                return apiContext.ValidateConnection();
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while validate connection info. Error: {e}");
                return false;
            }
        }
    }
}
