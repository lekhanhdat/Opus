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
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Encryption;
using AvePoint.RA.Service.Services.Box.AuditHandler;
using AvePoint.RA.Service.Services.Box.Converters;
using Newtonsoft.Json;
using RABox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Box
{
    [Audit]
    public class RMBoxConnectionService : RMServiceBase, IRMBoxConnectionService
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RMBoxConnectionService));
        private IRMBoxConnectionDao BoxConnectionDao => PlatformWindsorManager.GetService<IRMBoxConnectionDao>();
        public IRMBoxConnectionGroupDao BoxConnectionGroupDao => PlatformWindsorManager.GetService<IRMBoxConnectionGroupDao>();
        private static RMAesEncryptorWrapper AesEncryptorWrapper => new();

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.BoxCreateConnection, AfterHandler = typeof(BoxConnectionAfterAuditHandler), BeforeHandler = typeof(BoxConnectionBeforeAuditHandler))]
        public async Task<bool> CreateAsync(BoxConnectionItem connectionItem)
        {
            if (connectionItem is null)
            {
                throw new ArgumentNullException(nameof(connectionItem));
            }

            var existConn = BoxConnectionDao.GetByName(connectionItem.Name);
            if (existConn != null && existConn.Id != connectionItem.Id)
            {
                throw new ManageConnectionException(ConnectionResponseErrorType.NameExists);
            }

            try
            {
                var currentTime = DateTime.UtcNow.Ticks;
                var currentUserID = TenantLocalValue.LogonUserId;
                var connection = BoxConnectionConverter.ConvertToEntity(connectionItem);
                connection.Created = currentTime;
                connection.Modified = currentTime;
                connection.CreatedBy = currentUserID;
                connection.ModifiedBy = currentUserID;
                connection.ConnectionGroupId = Guid.Empty;
                return BoxConnectionDao.Add(connection);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while creating the Box connection. Error: {e}");
                return false;
            }
        }

        public async Task<IEnumerable<BoxConnectionItem>> GetAllAsync()
        {
            try
            {
                var connections = BoxConnectionDao.GetAll();

                if (connections == null || !connections.Any())
                {
                    Logger.Warn($"No box connections were retrieved from the database.");
                    return new List<BoxConnectionItem>();
                }

                return BoxConnectionConverter.Convert(connections);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while retrieving box connections. Error: {e}");
                return new List<BoxConnectionItem>();
            }
        }

        public async Task<IEnumerable<BoxConnectionItem>> GetAllByIdsAsync(List<Guid> ids)
        {
            if (ids == null || !ids.Any())
            {
                Logger.Warn($"The parameter [{ids}] is null or empty, unable to retrieve box connection groups.");
                return new List<BoxConnectionItem>();
            }

            try
            {
                var connections = BoxConnectionDao.GetAllByIds(ids);

                if (connections is null)
                {
                    Logger.Warn($"No box connections were retrieved for IDs: [{string.Join(", ", ids)}].");
                    return new List<BoxConnectionItem>();
                }
                return BoxConnectionConverter.Convert(connections);
            }
            catch (Exception e)
            {
                var idList = string.Join(", ", ids);
                Logger.Error($"An error occurred while retrieving box connections: [{idList}]. Error: {e}");
                return new List<BoxConnectionItem>();
            }
        }

        public async Task<IEnumerable<BoxConnectionItem>> GetAllByConnectionGroupAsync(Guid connectionGroupId)
        {
            if (connectionGroupId == Guid.Empty)
            {
                Logger.Warn("The parameter connectionGroupId is empty.");
                return new List<BoxConnectionItem>();
            }

            try
            {
                var connections = BoxConnectionDao.GetAllByConnectionGroup(connectionGroupId);

                if (connections is null)
                {
                    Logger.Warn($"No box connections were retrieved from the database for group ID: [{connectionGroupId}].");
                    return new List<BoxConnectionItem>();
                }
                return BoxConnectionConverter.Convert(connections);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while retrieving box connections by group [{connectionGroupId}]. Error: {e}");
                return new List<BoxConnectionItem>();
            }
        }

        public async Task<IEnumerable<BoxConnectionItem>> GetAllWithoutRelatedConnectionGroupAsync()
        {
            try
            {
                var connections = BoxConnectionDao.GetAllWithoutRelatedConnectionGroup();

                if (connections is null)
                {
                    Logger.Warn($"No box connections without related connection group were retrieved from the database.");
                    return new List<BoxConnectionItem>();
                }
                return BoxConnectionConverter.Convert(connections);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while retrieving box connections without related connection group. Error: {e}");
                return new List<BoxConnectionItem>();
            }
        }


        public async Task<BoxConnectionItem> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                Logger.Warn($"The parameter id is empty.");
                return null;
            }
            try
            {
                var connection = BoxConnectionDao.GetById(id);

                if (connection is null)
                {
                    Logger.Warn($"No box connection item with ID [{id}] was found.");
                    return null;
                }
                return BoxConnectionConverter.ConvertToItem(connection);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while retrieving box connection item [{id}]. Error: {e}");
                return null;
            }
        }

        public async Task<BoxConnectionItem> GetByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Logger.Warn($"The parameter name is null or empty.");
                return null;
            }

            try
            {
                var connection = BoxConnectionDao.GetByName(name);

                if (connection is null)
                {
                    Logger.Warn($"No box connection item with name [{name}] was found.");
                    return null;
                }
                return BoxConnectionConverter.ConvertToItem(connection);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while retrieving box connection item [{name}]. Error: {e}");
                return null;
            }
        }

        public bool Exists(Guid id)
        {
            if (id == Guid.Empty)
            {
                Logger.Warn($"The parameter id is empty.");
                return false;
            }

            try
            {
                return BoxConnectionDao.Exists(id);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while checking if connection [{id}] exists in record. Error: {e}");
                return false;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.BoxEditConnection, AfterHandler = typeof(BoxConnectionAfterAuditHandler), BeforeHandler = typeof(BoxConnectionBeforeAuditHandler))]
        public async Task<bool> ModifyAsync(BoxConnectionItem connectionItem)
        {
            if (connectionItem is null)
            {
                throw new ArgumentNullException(nameof(connectionItem));
            }

            var existConn = BoxConnectionDao.GetByName(connectionItem.Name);
            if (existConn != null && existConn.Id != connectionItem.Id)
            {
                throw new ManageConnectionException(ConnectionResponseErrorType.NameExists);
            }

            existConn = BoxConnectionDao.GetById(connectionItem.Id);
            if (existConn == null)
            {
                throw new ManageConnectionException(ConnectionResponseErrorType.Unknown, "The current connection doesn't exist.");
            }

            if (connectionItem.ConnectionGroupId != Guid.Empty && !BoxConnectionGroupDao.Exists(connectionItem.ConnectionGroupId))
            {
                throw new ManageConnectionException(ConnectionResponseErrorType.Unknown, "The connection group doesn't exist.");
            }

            UpdateConnectionFields(existConn, connectionItem);

            try
            {
                return BoxConnectionDao.Modify(existConn);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while modifying box connection. Error: {e}");
                return false;
            }
        }

        private void UpdateConnectionFields(RMBoxConnection existConn, BoxConnectionItem connectionItem)
        {
            existConn.Name = connectionItem.Name;
            existConn.Description = connectionItem.Description;
            existConn.AuthenticationType = (int)connectionItem.AuthenticationType;
            existConn.Modified = DateTime.UtcNow.Ticks;
            existConn.ModifiedBy = TenantLocalValue.LogonUserId;

            existConn.EnterpriseId = connectionItem.EnterpriseId;
            existConn.ClientId = connectionItem.ClientId;
            existConn.EmailAddress = connectionItem.EmailAddress;
            existConn.JsonFileName = connectionItem.JsonFileName;
            existConn.RedirectUrl = connectionItem.RedirectUrl;
            existConn.AccessToken = connectionItem.AccessToken;
            existConn.RefreshToken = connectionItem.RefreshToken;

            if (connectionItem.AuthenticationType == BoxAuthenticationType.UserAuth)
            {
                HandleUserAuthFields(existConn, connectionItem);
            }
            else if (connectionItem.AuthenticationType == BoxAuthenticationType.ServerAuth)
            {
                HandleServerAuthFields(existConn, connectionItem);
            }
        }

        private void HandleUserAuthFields(RMBoxConnection existConn, BoxConnectionItem connectionItem)
        {
            if (connectionItem.ClientSecret.IsNullOrEmpty())
            {
                if (existConn.ClientSecret.IsNullOrEmpty())
                {
                    throw new ManageConnectionException(ConnectionResponseErrorType.ValidationError, "The client secret cannot be empty");
                }
                connectionItem.ClientSecret = AesEncryptorWrapper.Decrypt(existConn.ClientSecret);
                connectionItem.JsonFileContent = existConn.JsonFileContent;
            }
            else
            {
                existConn.ClientSecret = AesEncryptorWrapper.Encrypt(connectionItem.ClientSecret);
                existConn.JsonFileContent = connectionItem.JsonFileContent;
            }
        }

        private void HandleServerAuthFields(RMBoxConnection existConn, BoxConnectionItem connectionItem)
        {
            if (connectionItem.JsonFileContent.IsNullOrEmpty())
            {
                if (existConn.JsonFileContent.IsNullOrEmpty())
                {
                    throw new ManageConnectionException(ConnectionResponseErrorType.ValidationError, "The JSON file content cannot be empty");
                }
                connectionItem.JsonFileContent = AesEncryptorWrapper.Decrypt(existConn.JsonFileContent);
                connectionItem.ClientSecret = existConn.ClientSecret;
            }
            else
            {
                existConn.JsonFileContent = AesEncryptorWrapper.Encrypt(connectionItem.JsonFileContent);
                existConn.ClientSecret = connectionItem.ClientSecret;
            }
        }

        public bool RemoveById(Guid id)
        {
            if (id == Guid.Empty)
            {
                Logger.Warn($"The parameter id is empty.");
                return false;
            }

            try
            {
                return BoxConnectionDao.RemoveById(id);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while removing box connection with ID [{id}]. Error: {e}");
                return false;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.BoxDeleteConnection, AfterHandler = typeof(BoxConnectionAfterAuditHandler), BeforeHandler = typeof(BoxConnectionBeforeAuditHandler))]
        public async Task<bool> RemoveAsync(List<Guid> ids)
        {
            if (ids == null || !ids.Any())
            {
                throw new ArgumentNullException(nameof(ids), $"The parameter {nameof(ids)} is null or empty.");
            }

            try
            {
                return await BoxConnectionDao.RemoveAsync(ids);
            }
            catch (Exception e)
            {
                var idList = string.Join(", ", ids);
                Logger.Error($"An error occurred while removing box connections [{idList}]. Error: {e}");
                return false;
            }
        }

        private bool IsValidJsonFileContent(byte[] bytes)
        {
            try
            {
                var content = Encoding.UTF8.GetString(bytes);
                return JsonConvert.DeserializeObject<object>(content) != null;
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while attempting to convert JSON file content. Error: {ex}");
                return false;
            }
        }

        public bool Validate(BoxConnectionItem connectionItem)
        {
            if (connectionItem == null)
            {
                throw new ArgumentNullException(nameof(connectionItem));
            }

            if (IsServerAuthWithInvalidJson(connectionItem))
            {
                throw new ManageConnectionException(ConnectionResponseErrorType.JsonFileInvalid);
            }

            return IsValidConnection(connectionItem);
        }

        private bool IsServerAuthWithInvalidJson(BoxConnectionItem connectionItem)
        {
            return connectionItem.AuthenticationType == BoxAuthenticationType.ServerAuth
                && connectionItem.JsonFileContent != null
                && !IsValidJsonFileContent(connectionItem.JsonFileContent);
        }

        private bool IsValidConnection(BoxConnectionItem connectionItem)
        {
            try
            {
                if (connectionItem.Id == Guid.Empty)
                {
                    return ValidateConnectionWithoutId(connectionItem);
                }
                if ((connectionItem.AuthenticationType == BoxAuthenticationType.ServerAuth && connectionItem.JsonFileContent != null) || (connectionItem.AuthenticationType == BoxAuthenticationType.UserAuth))
                {
                    return ValidateConnectionWithoutId(connectionItem);
                }
                return true;
            }
            catch (ManageConnectionException e)
            {
                Logger.Error($"An error occurred while validating connection info for connection [{connectionItem.Name}]. Error: {e}");
                throw;
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while validating connection info for connection [{connectionItem.Name}]. Error: {e}");
                return false;
            }
        }

        private bool ValidateConnectionWithoutId(BoxConnectionItem connectionItem)
        {
            try
            {
                var clientContext = new BoxClientContext(connectionItem);
                if (DoesEnterpriseIdExist(connectionItem))
                {
                    throw new ManageConnectionException(ConnectionResponseErrorType.EnterpriseIdExists);
                }
                return !string.IsNullOrEmpty(clientContext.TokenUserId);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("AuthorizationCodeExpired"))
                {
                    throw new ManageConnectionException(ConnectionResponseErrorType.AuthorizationCodeExpired);
                }
                throw;
            }
            
        }

        private bool DoesEnterpriseIdExist(BoxConnectionItem connectionItem)
        {
            return BoxConnectionDao.ExistsByEnterpriseId(connectionItem.EnterpriseId, connectionItem.Id);
        }
    }
}