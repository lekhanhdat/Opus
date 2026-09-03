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
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.Authentication;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Security.ActiveDirectory;
using AvePoint.RA.Service.Services.AccountManager.AuditHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.RA.Service.AccountManager
{
    [Audit]
    public class AccountManagerService : IAccountManagerService
    {
        public IAuthenticationManagerService AuthMgrService { get; set; }

        public IAccountDao AccountDao { get; set; }

        public ADAuthentication ADProvider = new ADAuthentication();

        #region AD Account operations
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AccountManagement, Action = AuditAction.AddADAccount,
            BeforeHandler = typeof(AccountManagementBeforeAuditHandler), AfterHandler = typeof(AccountManagementAfterAuditHandler))]
        public bool AddADAccounts(ref List<RMADAccountDto> accounts)
        {
            bool result = false;
            if (ADProvider.AccountsValidationTest(ref accounts))
            {
                var availableAccounts = accounts
                    .Where(a => a.Status == RMAccountStatus.Available)
                    .Select(a => RMSecurityUtil.ConvertToDBAccount(a))
                    .ToList();
                int total;
                if ((total = availableAccounts.Count) > 0)
                {
                    result = AccountDao.BatchCreate(availableAccounts) == total;
                }
                else
                {
                    result = true;
                }
            }
            return result;
        }

        [RACodeReview("Allen Yin")]
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AccountManagement, Action = AuditAction.DeleteAccount,
            BeforeHandler = typeof(AccountManagementBeforeAuditHandler), AfterHandler = typeof(AccountManagementAfterAuditHandler))]
        public bool DeleteADAccount(int id)
        {
            return AccountDao.DeleteByKey(id);
        }

        [RACodeReview("Allen Yin")]
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AccountManagement, Action = AuditAction.DeleteAccount,
            BeforeHandler = typeof(AccountManagementBeforeAuditHandler), AfterHandler = typeof(AccountManagementAfterAuditHandler))]
        public bool DeleteADAccounts(List<int> ids)
        {
            if (ids != null && ids.Count > 0)
            {
                return AccountDao.BatchDelete(a => ids.Contains(a.Id)) > 0;
            }
            return false;
        }

        [RACodeReview("Allen Yin")]
        public List<RMADAccountDto> GetAccounts(int pageIndex, int pageSize, out int totalRecord)
        {
            List<RMADAccountDto> accounts = new List<RMADAccountDto>();
            //Get All domains registered in RA here
            var domains = AuthMgrService.GetADDomains(false);
            var list = AccountDao.GetAccounts(pageIndex, pageSize, out totalRecord);
            foreach (var item in list)
            {
                RMADAccountDto account = RMSecurityUtil.ConvertToAccountDto(item);
                if (account.Type == RMAccountType.Local)
                {
                    account.Status = RMAccountStatus.Active;
                }
                else
                {
                    RMDomainDto domain = domains.FirstOrDefault(d => d.Id == item.DomainId);
                    if (domain == null)
                    {
                        account.Status = RMAccountStatus.Delete;
                    }
                    else
                    {
                        account.Domain = domain.DomainName;
                        account.Status = domain.Enable ? RMAccountStatus.Active : RMAccountStatus.Deactive;
                    }
                }
                accounts.Add(account);
            }

            return accounts;
        }

        public List<RMADAccountDto> GetAccounts(List<int> ids)
        {
            if (ids != null && ids.Count > 0)
            {
                return AccountDao.FindList(a => ids.Contains(a.Id))
                    .Select(a => RMSecurityUtil.ConvertToAccountDto(a)).ToList();
            }
            return null;
        }

        [RACodeReview("Allen Yin")]
        public List<RMADAccountDto> SearchAccountSuggestion(string key, int perDomainCount)
        {
            return ADProvider.SearchAccounts(key, perDomainCount);
        }

        public RMADAccountDto SearchAccountSuggestion(string name)
        {
            return ADProvider.SearchSingleAccount(name);
        }

        /// <summary>
        /// 精确匹配一个AD User
        /// </summary>
        /// <param name="fullName">
        /// e.g: jt\administrator
        /// administrator
        /// administrator@jt.com
        /// </param>
        /// <returns></returns>
        [RACodeReview("Allen Yin")]
        public RMADAccountDto SearchSingleAccount(string fullName)
        {
            return ADProvider.SearchSingleAccountByFullName(fullName);
        }
        #endregion

        #region Local Account operations
        public string GetSuperAdminName()
        {
            var admin = AccountDao.GetSuperAdmin();
            if (admin != null)
            {
                return admin.DisplayName;
            }
            return string.Empty;
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AccountManagement, Action = AuditAction.EditLocalAccount,
            BeforeHandler = typeof(AccountManagementBeforeAuditHandler), AfterHandler = typeof(AccountManagementAfterAuditHandler))]
        public bool ChangeLocalAccountPassword(string newPassword, out RMOperatingAccountError errorType)
        {
            if (AccountDao.VerifyAdminPassword(newPassword))
            {
                errorType = RMOperatingAccountError.SamePassword;
            }
            else if (AccountDao.SaveAdminPassword(newPassword))
            {
                errorType = RMOperatingAccountError.None;
                return true;
            }
            else
            {
                errorType = RMOperatingAccountError.SavePasswordFailed;
            }

            return false;
        }

        public bool ValidateLocalAccountPassword(string password)
        {
            return AccountDao.VerifyAdminPassword(password);
        }
        #endregion

        #region Token
        /// <summary>
        /// 为实现AD Account的Security Token 准备
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public RMADAccountDto GetADAcountByLoginName(string loginName)
        { 
                if (loginName.Contains(@"\"))
                {
                    //直接根据Name获取Account
                    RMAccount dAccount = AccountDao.GetAdAccountByLoginName(loginName);
                    return RMSecurityUtil.ConvertToAccountDto(dAccount);
                }
                else if (loginName.Contains("@"))
                {
                    string[] name = loginName.Split('@');
                    string account = name[0];
                    string domain = name[1];
                    //组装Name 获取Account
                    RMAccount dAccount = AccountDao.GetAdAccountByLoginName(domain +"\\"+account);
                    return RMSecurityUtil.ConvertToAccountDto(dAccount);
                } 
            return null;
        }
        /// <summary>
        /// 暂时不用
        /// </summary>
        /// <returns></returns>
        public RMAccount RefreshAdminSecurityToken()
        {
            var admin = AccountDao.GetSuperAdmin();
            if (admin != null)
            {
                string temp = admin.LoginName +":"+ DateTime.UtcNow.Ticks.ToString();
                admin.SecurityToken = AveProtectedData.ProtectWithBase64(Encoding.UTF8.GetBytes(temp));
                AccountDao.Update(admin);
            }
            return admin;
        }
        /// <summary>
        /// 用之前要验证密码
        /// </summary>
        /// <returns></returns>
        public string GetAdminSecurityToken(string password)
        {
            var admin = AccountDao.GetSuperAdmin();
            if (admin != null)
            {
                if (admin.SecurityToken == null)
                {
                    string temp = admin.LoginName + ":" + DateTime.UtcNow.Ticks.ToString();
                    admin.SecurityToken = AveProtectedData.ProtectWithBase64(Encoding.UTF8.GetBytes(temp));
                    AccountDao.Update(admin);
                }
                return admin.SecurityToken;
            }
            return null;
        }
        #endregion
    }
}
