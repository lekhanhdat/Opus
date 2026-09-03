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
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.Box.Model;
using AvePoint.RA.Contract.Connections;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.Box.AuditHandler;
using AvePoint.RA.Service.Services.Box.Converters;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Box
{
    [Audit]
    public class RMBoxConnectionGroupService : RMServiceBase, IRMBoxConnectionGroupService
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RMBoxConnectionGroupService));
        private IRMBoxConnectionGroupDao BoxConnectionGroupDao => PlatformWindsorManager.GetService<IRMBoxConnectionGroupDao>();

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.BoxCreateGroup, AfterHandler = typeof(BoxConnectionGroupAfterAuditHandler), BeforeHandler = typeof(BoxConnectionGroupBeforeAuditHandler))]
        public async Task<bool> CreateAsync(BoxConnectionGroupItem connectionGroupItem)
        {
            if (connectionGroupItem is null)
            {
                throw new ArgumentNullException(nameof(connectionGroupItem), $"The connection group is null or empty. Unable to remove any box connection group.");
            }

            var existGroup = BoxConnectionGroupDao.GetByName(connectionGroupItem.Name);
            if (existGroup != null && existGroup.Id != connectionGroupItem.Id)
            {
                throw new ManageConnectionException(ConnectionResponseErrorType.NameExists);
            }

            try
            {
                var connectionGroup = BoxConnectionGroupConverter.ConvertToEntity(connectionGroupItem);
                connectionGroup.Created = connectionGroup.Modified = DateTime.UtcNow.Ticks;
                connectionGroup.CreatedBy = connectionGroup.ModifiedBy = TenantLocalValue.LogonUserId;

                return BoxConnectionGroupDao.Add(connectionGroup);
            }
            catch (InvalidOperationException mcex)
            {
                throw new ManageConnectionException(mcex.Message);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while creating the Box connection group. Error: {e}");
                return false;
            }
        }

        public async Task<IEnumerable<BoxConnectionGroupItem>> GetAllAsync()
        {
            try
            {
                var groups = BoxConnectionGroupDao.GetAll();
                if (groups == null || !groups.Any())
                {
                    Logger.Warn($"No box connection groups were retrieved from the database.");
                    return new List<BoxConnectionGroupItem>();
                }

                return groups.ConvertAll(item => item.ConvertToItem());
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while retrieving box connection groups. Error: {e}");
                return new List<BoxConnectionGroupItem>();
            }
        }

        public async Task<IEnumerable<BoxConnectionGroupItem>> GetAllByIdsAsync(List<Guid> ids)
        {
            if (ids == null || !ids.Any())
            {
                Logger.Warn($"The parameter [{ids}] is null or empty, unable to retrieve box connection groups.");
                return new List<BoxConnectionGroupItem>();
            }

            try
            {
                var groups = BoxConnectionGroupDao.GetByIds(ids);
                if (groups == null || !groups.Any())                                                                                         
                {
                    Logger.Warn($"No box connection groups were retrieved from the database for IDs: [{string.Join(", ", ids)}].");
                    return new List<BoxConnectionGroupItem>();
                }
                return groups.ConvertAll(item => item.ConvertToItem());
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while retrieving box connection groups [{string.Join(", ", ids)}]. Error: {e}");
                return new List<BoxConnectionGroupItem>();
            }
        }

        public async Task<BoxConnectionGroupItem> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                Logger.Warn($"The parameter [{id}] is empty, unable to retrieve box connection group.");
                return null;
            }

            try
            {
                var group = BoxConnectionGroupDao.GetById(id);
                if (group is null)
                {
                    Logger.Warn($"No box connection group with ID [{id}] was found.");
                    return null;
                }
                return BoxConnectionGroupConverter.ConvertToItem(group);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while retrieving box connection group [{id}]. Error: {e}");
                return null;
            }
        }

        public async Task<BoxConnectionGroupItem> GetByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Logger.Warn($"The parameter [{name}] is null or empty, unable to retrieve box connection group.");
                return null;
            }

            try
            {
                var group = BoxConnectionGroupDao.GetByName(name);
                if (group is null)
                {
                    Logger.Warn($"No box connection group with name [{name}] was found.");
                    return null;
                }

                return BoxConnectionGroupConverter.ConvertToItem(group);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while retrieving box connection group [{name}]. Error: {e}");
                return null;
            }
        }

        public bool Exists(Guid id)
        {
            if (id == Guid.Empty)
            {
                Logger.Warn($"The parameter [{id}] is empty, unable to check if box connection group exists.");
                return false;
            }

            try
            {
                return BoxConnectionGroupDao.Exists(id);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while checking if box connection group [{id}] exists. Error: {e}");
                return false;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.BoxEditGroup, AfterHandler = typeof(BoxConnectionGroupAfterAuditHandler), BeforeHandler = typeof(BoxConnectionGroupBeforeAuditHandler))]
        public async Task<bool> ModifyAsync(BoxConnectionGroupItem connectionGroupItem)
        {
            if (connectionGroupItem is null)
            {
                throw new ArgumentNullException(nameof(connectionGroupItem), $"The connection group is null or empty. Unable to modify any box connection group.");
            }

            var existGroup = BoxConnectionGroupDao.GetByName(connectionGroupItem.Name);
            if (existGroup != null && existGroup.Id != connectionGroupItem.Id)
            {
                throw new ManageConnectionException(ConnectionResponseErrorType.NameExists);
            }

            var connectionGroup = BoxConnectionGroupConverter.ConvertToEntity(connectionGroupItem);
            existGroup = BoxConnectionGroupDao.GetById(connectionGroupItem.Id);
            if (existGroup == null)
            {
                throw new ManageConnectionException(ConnectionResponseErrorType.Unknown, "The current connection group doesn't exist.");
            }
            try
            {
                connectionGroup.Created = existGroup.Created;
                connectionGroup.CreatedBy = existGroup.CreatedBy;
                connectionGroup.Modified = DateTime.UtcNow.Ticks;
                connectionGroup.ModifiedBy = TenantLocalValue.LogonUserId;

                return BoxConnectionGroupDao.Modify(connectionGroup);
            }
            catch (InvalidOperationException mcex)
            {
                throw new ManageConnectionException(mcex.Message);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while  a Box connection group. Error: {e}");
                return false;
            }

        }

        public bool RemoveById(Guid id)
        {
            if (id == Guid.Empty)
            {
                Logger.Warn($"The parameter [{id}] is empty. Unable to remove the box connection group.");
                return false;
            }

            try
            {
                return BoxConnectionGroupDao.RemoveById(id);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to remove the box connection group with ID [{id}]. Error: {e}");
                return false;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.BoxDeleteGroup, AfterHandler = typeof(BoxConnectionGroupAfterAuditHandler), BeforeHandler = typeof(BoxConnectionGroupBeforeAuditHandler))]
        public async Task<bool> RemoveAsync(List<Guid> ids)
        {
            if (ids == null || !ids.Any())
            {
                throw new ArgumentNullException(nameof(ids), $"The parameter {nameof(ids)} is null or empty. Unable to remove any box connection group.");
            }

            try
            {
                return await BoxConnectionGroupDao.RemoveByIdsAsync(ids);
            }
            catch (Exception e)
            {
                var idList = string.Join(", ", ids);
                Logger.Error($"Failed to remove box connection groups [{idList}]. Error: {e}");
                return false;
            }
        }
    }
}

