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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using Cloud.Sdk.Data.Aos.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Service.Services.Tenant
{
    public class RMRemoteO365AccountService : RMServiceBase, IRMRemoteO365AccountService
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMRemoteO365AccountService));
        private IRMServiceAccountDao ServiceAccountDao => PlatformWindsorManager.GetService<IRMServiceAccountDao>();
        private ITenantInfoDao TenantInfoDao => PlatformWindsorManager.GetService<ITenantInfoDao>();


        #region Initialize all ServiceAccounts
        public void SyncAllServiceAccountsFromAOS()
        {
            string tenantGroupId = TenantLocalValue.LogonGroupId;
            try
            {
                var serviceAccounts = RMAosApiClient.GetServiceAccounts(tenantGroupId);
                logger.Info("Begin to sync service accounts. Group id is {0}", tenantGroupId);
                if (serviceAccounts == null || serviceAccounts.Count == 0)
                {
                    logger.Info("This is no service accounts to sync.");
                    return;
                }
                serviceAccounts = serviceAccounts.Distinct(new ServiceAccountComparer()).ToList();
                List<O365ServiceAccountDto> o365ServiceAccounts = ConvertToO365ServiceAccountDtoes(serviceAccounts);
                CreateServiceAccount(o365ServiceAccounts);
                TenantInfoDao.UpdateSyncSAState(tenantGroupId, RMInitNodeState.Synced);
            }
            catch (Exception ex)
            {
                TenantInfoDao.UpdateSyncSAState(tenantGroupId, RMInitNodeState.SyncFailed);
                logger.Error("Failed to sync all service accounts. Exception is {0}.", ex.ToString());
            }
        }

        private List<O365ServiceAccountDto> ConvertToO365ServiceAccountDtoes(List<Cloud.Sdk.Data.AosModern.ServiceAccount> serviceAccounts)
        {
            var o365ServiceAccounts = new List<O365ServiceAccountDto>();
            foreach (var serviceAccount in serviceAccounts)
            {

                var adminUrl = RMAosApiClient.GetO365TenantInfoByIdAsync(serviceAccount.TenantId).GetAwaiter().GetResult().AdminUrl;

                o365ServiceAccounts.Add(new O365ServiceAccountDto()
                {
                    Id = HashCodeHelper.ToMD5HashCode(serviceAccount.UserName),
                    UserName = serviceAccount.UserName,
                    Password = string.Empty,
                    TenantId = serviceAccount.TenantId,
                    TenantName = serviceAccount.DomainName,
                    AdminUrl = adminUrl,
                });
            }
            return o365ServiceAccounts;
        }

        private class ServiceAccountComparer : IEqualityComparer<Cloud.Sdk.Data.AosModern.ServiceAccount>
        {
            public bool Equals(Cloud.Sdk.Data.AosModern.ServiceAccount x, Cloud.Sdk.Data.AosModern.ServiceAccount y)
            {
                if (object.ReferenceEquals(x, y)) return true;
                if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null))
                {
                    return false;
                }
                return x.UserName.Equals(y.UserName, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(Cloud.Sdk.Data.AosModern.ServiceAccount obj)
            {
                return obj.GetHashCode();
            }
        }
        #endregion

        public void CreateServiceAccount(O365ServiceAccountDto account)
        {
            ServiceAccountDao.CreateServiceAccount(account);
        }

        public void UpdateServiceAccount(O365ServiceAccountDto account)
        {
            ServiceAccountDao.UpdateServiceAccount(account);
        }

        public void CreateServiceAccount(List<O365ServiceAccountDto> accounts)
        {
            ServiceAccountDao.CreateServiceAccount(accounts);
        }

        public void UpdateServiceAccount(List<O365ServiceAccountDto> accounts)
        {
            ServiceAccountDao.UpdateServiceAccount(accounts);
        }

        public void CreateOrUpdateServiceAccount(List<O365ServiceAccountDto> accounts)
        {
            ServiceAccountDao.CreateOrUpdateServiceAccount(accounts);
        }

        public List<O365ServiceAccountDto> GetServiceAccounts(List<string> ids)
        {
            return ServiceAccountDao.GetServiceAccounts(ids);
        }

        public O365ServiceAccountDto GetServiceAccountByUser(string userName)
        {
            return ServiceAccountDao.GetServiceAccountByUser(userName);
        }

        public bool CheckServiceAccountExisted(string userName)
        {
            return ServiceAccountDao.CheckServiceAccountExisted(userName);
        }

        public void UpdateServiceAccountPass(string userName, string password)
        {
            ServiceAccountDao.UpdateServiceAccountPass(userName, password);
        }

        public List<string> GetAllUserNames()
        {
            return ServiceAccountDao.GetAllUserNames();
        }

        public List<O365ServiceAccountDto> GetAllAccounts()
        {
            return ServiceAccountDao.GetAllAccounts();
        }

        public bool UpdateUserNameAndPasswordById(string id, string userName, string password)
        {
            return ServiceAccountDao.UpdateUserNameAndPasswordById(id, userName, password);
        }
    }
}