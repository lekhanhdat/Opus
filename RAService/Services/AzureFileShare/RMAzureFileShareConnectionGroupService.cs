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
using AvePoint.RA.Service.Services.AzureFileShare.AuditHandler;
using AvePoint.RA.Service.Services.AzureFileShare.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.AzureFileShare
{
    [Audit]
    public class RMAzureFileShareConnectionGroupService : RMServiceBase, IRMAzureFileShareConnectionGroupService
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RMAzureFileShareConnectionGroupService));

        private IRMAzureFileShareConnectionGroupDao AzureFileShareConnectionGroupDao => PlatformWindsorManager.GetService<IRMAzureFileShareConnectionGroupDao>();

        private IGeneralSettingService  GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private Task<GeneralSettingModel> Gsl => GeneralSettingService.GetGeneralSettingAsync();

        public async Task<AzureFileShareConnectionGroupItem> GetAsync(string name)
        {
            try
            {
                var group = AzureFileShareConnectionGroupDao.Get(name);
                return AzureFileShareConnectionGroupConverter.Convert(group, await Gsl);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get azure file share connection group [{name}]. Error: {e}");
                return null;
            }
        }

        public async Task<AzureFileShareConnectionGroupItem> GetAsync(Guid id)
        {
            try
            {
                var group = AzureFileShareConnectionGroupDao.Get(id);
                return AzureFileShareConnectionGroupConverter.Convert(group, await Gsl);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get azure file share connection group [{id}]. Error: {e}");
                return null;
            }
        }

        public async Task<List<AzureFileShareConnectionGroupItem>> GetAllAsync()
        {
            try
            {
                var groups = AzureFileShareConnectionGroupDao.GetAll();
                return AzureFileShareConnectionGroupConverter.Convert(groups, await Gsl);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get azure file share connections. Error: {e}");
                return new List<AzureFileShareConnectionGroupItem>();
            }
        }

        public async Task<List<AzureFileShareConnectionGroupItem>> GetAllAsync(List<Guid> ids)
        {
            try
            {
                var groups = AzureFileShareConnectionGroupDao.GetAll(ids);
                return AzureFileShareConnectionGroupConverter.Convert(groups, await Gsl);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get azure file share connections. Error: {e}");
                return new List<AzureFileShareConnectionGroupItem>();
            }
        }

        public bool Has(Guid id)
        {
            try
            {
                return AzureFileShareConnectionGroupDao.Has(id);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while check group: [{id}] has in record. Error: {e}");
                return false;
            }
        }

        public bool Remove(Guid id)
        {
            try
            {
                return AzureFileShareConnectionGroupDao.Remove(id);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while remove azure file share connection group [{id}]. Error: {e}");
                return false;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.AzureFileShareDeleteGroup, AfterHandler = typeof(AzureFileShareConnectionGroupAfterAuditHandler), BeforeHandler = typeof(AzureFileShareConnectionGroupBeforeAuditHandler))]
        public async Task<bool> RemoveAsync(List<Guid> ids)
        {
            try
            {
                return await AzureFileShareConnectionGroupDao.RemoveAsync(ids);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while remove azure file share connection groups [{string.Join(", ", ids)}]. Error: {e}");
                return false;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.AzureFileShareCreateGroup, AfterHandler = typeof(AzureFileShareConnectionGroupAfterAuditHandler), BeforeHandler = typeof(AzureFileShareConnectionGroupBeforeAuditHandler))]
        public async Task<bool> CreateAsync(AzureFileShareConnectionGroupItem connectionGroupItem)
        {
            try
            {
                if (connectionGroupItem == null)
                {
                    Logger.Warn($"The parameter [connnectionGroupItem] is null, can't invoke create action.");
                    return false;
                }

                var connectionGroup = AzureFileShareConnectionGroupConverter.Convert(connectionGroupItem, await Gsl);
                connectionGroup.Created = DateTime.UtcNow.Ticks;
                connectionGroup.Modified = DateTime.UtcNow.Ticks;
                connectionGroup.CreatedBy = TenantLocalValue.LogonUserId;
                connectionGroup.ModifiedBy = TenantLocalValue.LogonUserId;
                return AzureFileShareConnectionGroupDao.Add(connectionGroup);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while create azure file share connection group. Error: {e}");
                return false;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.AzureFileShareEditGroup, AfterHandler = typeof(AzureFileShareConnectionGroupAfterAuditHandler), BeforeHandler = typeof(AzureFileShareConnectionGroupBeforeAuditHandler))]
        public async Task<bool> ModifyAsync(AzureFileShareConnectionGroupItem connectionGroupItem)
        {
            try
            {
                if (connectionGroupItem == null)
                {
                    Logger.Warn($"The parameter [connnectionGroupItem] is null, can't invoke modify action.");
                    return false;
                }

                var connectionGroup = AzureFileShareConnectionGroupConverter.Convert(connectionGroupItem, await Gsl);
                var group = AzureFileShareConnectionGroupDao.Get(connectionGroup.Id);
                connectionGroup.Created = group.Created;
                connectionGroup.CreatedBy = group.CreatedBy;
                connectionGroup.Modified = DateTime.UtcNow.Ticks;
                connectionGroup.ModifiedBy = TenantLocalValue.LogonUserId;
                return AzureFileShareConnectionGroupDao.Modify(connectionGroup);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while modify azure file share connection group. Error: {e}");
                return false;
            }
        }
    }
}
