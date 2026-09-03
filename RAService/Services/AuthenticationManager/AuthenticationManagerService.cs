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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common.Cryptography;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.Authentication;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Services.AuthenticationManager.AuditHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;

namespace AvePoint.RA.Service.AuthenticationManager
{
    [Audit]
    public class AuthenticationManagerService : IAuthenticationManagerService
    {
        private AveLogger logger = AveLogger.GetInstance(typeof(AuthenticationManagerService));

        public IAuthenticationModeDao AuthenticationDao { get; set; }
        //public IADDomainDao AdDomainDao { get; set; }
        //public IAccountDao AccountDao { get; set; }

        //private ADAuthentication ADProvider = new ADAuthentication();


        #region Authentication Mode Operation

        [RACodeReview("Allen Yin")]
        public List<RMAuthenticationDto> GetAuthenticationModes(bool onlyEnableMode, bool containDomains, bool onlyEnableDomain)
        {
            List<RMAuthenticationDto> results = null;
            try
            {
                var modes = AuthenticationDao.GetAuthenticationModes(onlyEnableMode);
                results = modes.Select(am => ConvertToAuthenticationInfo(am, containDomains, onlyEnableDomain)).ToList();
            }
            catch (Exception ex)
            {
                logger.Error("Get authentication modes failed! Error message: {0}.", ex.ToString());
            }
            return results;
        }

        [RACodeReview("Allen Yin")]
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AuthenticationManagement, Action = AuditAction.EnableAuthenticationMode,
            BeforeHandler = typeof(AuthenticationBeforeAuditHandler), AfterHandler = typeof(AuthenticationAfterAuditHandler))]
        public bool EnableAuthenticationMode(int id)
        {
            try
            {
                return AuthenticationDao.ChangeAuthenticationModeStatus(id, true);
            }
            catch (Exception ex)
            {
                logger.Error("Enable authentication mode failed! Mode id: {0}, Error message: {1}.", id, ex.ToString());
            }
            return false;
        }

        [RACodeReview("Allen Yin")]
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AuthenticationManagement, Action = AuditAction.DisableAuthenticationMode,
            BeforeHandler = typeof(AuthenticationBeforeAuditHandler), AfterHandler = typeof(AuthenticationAfterAuditHandler))]
        public bool DisableAuthenticationMode(int id)
        {
            try
            {
                return AuthenticationDao.ChangeAuthenticationModeStatus(id, false);
            }
            catch (Exception ex)
            {
                logger.Error("Disable authentication mode failed! Mode id: {0}, Error message: {1}.", id, ex.ToString());
            }
            return false;
        }

        [RACodeReview("Allen Yin")]
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AuthenticationManagement, Action = AuditAction.SetDefaultAuthenticationMode,
            BeforeHandler = typeof(AuthenticationBeforeAuditHandler), AfterHandler = typeof(AuthenticationAfterAuditHandler))]
        public bool SetDefaultAuthenticationMode(int id)
        {
            try
            {
                return AuthenticationDao.SetDefaultAuthenticationMode(id);
            }
            catch (Exception ex)
            {
                logger.Error("Set as default authentication mode failed! Mode id: {0}, Error message: {1}.", id, ex.ToString());
            }
            return false;
        }

        [RACodeReview("Allen Yin")]
        public RMAuthenticationDto GetDefaultAuthenticationMode()
        {
            var defMode = AuthenticationDao.Find(m => m.IsDefault);
            return ConvertToAuthenticationInfo(defMode, false, false);
        }

        [RACodeReview("Allen Yin")]
        public RMAuthenticationDto GetAuthenticationModeById(int id)
        {
            var mode = AuthenticationDao.Find(m => m.Id == id);
            if (mode == null)
            {
                return null;
            }
            else
            {
                return ConvertToAuthenticationInfo(mode, false, false);
            }
        }

        private RMAuthenticationDto ConvertToAuthenticationInfo(RMAuthenticationMode mode, bool containDomains, bool onlyEnableDomain)
        {
            RMAuthenticationDto info = new RMAuthenticationDto();
            info.Id = mode.Id;
            info.Enable = mode.Enable;
            info.IsDefault = mode.IsDefault;
            info.Type = mode.Type;
            info.Name = RMSecurityUtil.GetAuthenticationMode(mode.Type);
            if (containDomains)
            {
                info.Domains = GetADDomains(onlyEnableDomain);
            }
            return info;
        }
        #endregion


        #region AD Domain Operation

        [RACodeReview("Allen Yin")]
        public RMDomainDto GetADDomain(int id, bool needPassword = false)
        {
            var domain = AdDomainDao.Find(d => d.Id == id);
            if (domain != null)
            {
                return RMSecurityUtil.ConvertToDomainDto(domain, needPassword);
            }
            return null;
        }

        [RACodeReview("Allen Yin")]
        public List<RMDomainDto> GetADDomains(List<int> ids)
        {
            List<RMDomainDto> results = null;
            try
            {
                var domains = AdDomainDao.FindList(d => ids.Contains(d.Id));
                results = domains.Select(dm => RMSecurityUtil.ConvertToDomainDto(dm)).ToList();
            }
            catch (Exception ex)
            {
                logger.Error("Get ad domains failed! Error message: {0}.", ex.ToString());
            }
            return results;
        }

        [RACodeReview("Allen Yin")]
        public List<RMDomainDto> GetADDomains(bool onlyEnableDomain)
        {
            List<RMDomainDto> results = null;
            try
            {
                var domains = AdDomainDao.GetADDomains(onlyEnableDomain);
                return domains.Select(dm => RMSecurityUtil.ConvertToDomainDto(dm)).ToList();
            }
            catch (Exception ex)
            {
                logger.Error("Get ad domains failed! Error message: {0}.", ex.ToString());
            }
            return results;
        }

        [RACodeReview("Allen Yin")]
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AuthenticationManagement, Action = AuditAction.AddADDomain,
            BeforeHandler = typeof(AuthenticationBeforeAuditHandler), AfterHandler = typeof(AuthenticationAfterAuditHandler))]
        public RMDomainDto AddADDomain(RMDomainDto info, out RMOperatingDomainError errorType)
        {
            errorType = RMOperatingDomainError.None;
            ADAuthentication adProvider = new ADAuthentication();
            if (adProvider.DomainValidationTest(ref info))
            {
                if (AdDomainDao.Exist(d => d.DomainName.Equals(info.DomainName, StringComparison.OrdinalIgnoreCase)))
                {
                    errorType = RMOperatingDomainError.DomainIsExist;
                    return null;
                }
                var domain = AdDomainDao.Create(RMSecurityUtil.ConvertToDBDomain(info));
                return RMSecurityUtil.ConvertToDomainDto(domain);
            }
            else
            {
                errorType = RMOperatingDomainError.UnableConnectDomain;
                return null;
            }
        }

        [RACodeReview("Allen Yin")]
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AuthenticationManagement, Action = AuditAction.DeleteADDomain,
            BeforeHandler = typeof(AuthenticationBeforeAuditHandler), AfterHandler = typeof(AuthenticationAfterAuditHandler))]
        public bool DeleteADDomain(int id)
        {
            try
            {
                return AdDomainDao.DeleteDomainById(id);
            }
            catch (Exception ex)
            {
                logger.Error("Delete ad domain failed! Domain id: {0}, Error message: {1}.", id, ex.ToString());
            }
            return false;
        }

        [RACodeReview("Allen Yin")]
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AuthenticationManagement, Action = AuditAction.DeleteADDomain,
            BeforeHandler = typeof(AuthenticationBeforeAuditHandler), AfterHandler = typeof(AuthenticationAfterAuditHandler))]
        public bool DeleteADDomain(List<int> ids)
        {
            try
            {
                return AdDomainDao.DeleteDomainByIds(ids);
            }
            catch (Exception ex)
            {
                StringBuilder idsString = new StringBuilder("[");
                var total = ids.Count;
                if (ids != null && total > 0)
                {
                    idsString.AppendFormat("{0}", ids[0]);
                }
                
                for (int i=1; i < total; i++)
                {
                    idsString.AppendFormat(",{0}", ids[i]);
                }
                
                logger.Error("Delete ad domains failed! Domain ids: {0}, Error message: {1}.", idsString.ToString(), ex.ToString());
            }
            return false;
        }

        [RACodeReview("Allen Yin")]
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AuthenticationManagement, Action = AuditAction.EnableADDomain,
            BeforeHandler = typeof(AuthenticationBeforeAuditHandler), AfterHandler = typeof(AuthenticationAfterAuditHandler))]
        public bool UpdateADDomainStatus(int id, bool status)
        {
            RMADDomain domain = AdDomainDao.Find(i => i.Id == id);
            if (domain.Enable != status)
            {
                domain.Enable = status;
                return AdDomainDao.Update(domain, d => d.Enable);
            }
            else
            {
                return true;
            }
        }

        [RACodeReview("Allen Yin")]
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AuthenticationManagement, Action = AuditAction.EnableADDomain,
            BeforeHandler = typeof(AuthenticationBeforeAuditHandler), AfterHandler = typeof(AuthenticationAfterAuditHandler))]
        public bool UpdateADDomainStatus(List<int> ids, bool status)
        {
            List<RMADDomain> updateDomains = new List<RMADDomain>();
            List<RMADDomain> domains = AdDomainDao.FindList(i => ids.Contains(i.Id));

            foreach (var domain in domains)
            {
                if (domain.Enable != status)
                {
                    domain.Enable = status;
                    updateDomains.Add(domain);
                }
            }

            if (updateDomains.Count > 0)
            {
                return AdDomainDao.BatchUpdate(updateDomains, d => d.Enable) > 0;
            }
            else
            {
                return true;
            }
        }

        [RACodeReview("Allen Yin")]
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AuthenticationManagement, Action = AuditAction.EditADDomain,
            BeforeHandler = typeof(AuthenticationBeforeAuditHandler), AfterHandler = typeof(AuthenticationAfterAuditHandler))]
        public bool UpdateADDomainUserInfo(int id, string userName, string password)
        {
            bool result = false;
            try
            {
                var dbEncrypt = DatabaseEncryptionFactory.CreateDatabaseEncryption();
                SecureString sPw = DatabaseEncryptionHelper.ConvertSecurityString(password);
                var domain = AdDomainDao.Find(d => d.Id == id);
                if (domain != null && ADProvider.DomainValidationTest(domain.RealName, userName, password))
                {
                    domain.UserName = userName;
                    domain.Password = dbEncrypt.EncryptPasswordDtoToXmlString(sPw);
                    result = AdDomainDao.Update(domain);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Update ad domain info failed! Domain id: {0}, Error message: {1}.", id, ex.ToString());
            }
            return result;
        }

        #endregion

    }
}
