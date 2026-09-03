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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.RMWeb.Authentication;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.Service.Security.Buildin
{
    public class BuildinAuthentication
    {
        private IAccountDao accountDao;

        protected IAccountDao AccountDao
        {
            get
            {
                if (accountDao == null)
                {
                    accountDao = (IAccountDao)PlatformWindsorManager.GetService(typeof(IAccountDao));
                }
                return accountDao;
            }
        }

        public RMIdentity AuthenticateCredential(LocalSystemCredential model)
        {
            RMIdentity identity = new RMIdentity();
            identity.Name = "admin";
            identity.DisplayName = I18NEntity.GetString("RM_JS_Common_LocalAdminName");
            identity.AuthenticationType = RMAuthenticationTypes.LocalSystem.ToString();
            if (!model.UserName.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                identity.IsAuthenticated = false;
            }
            else if (AccountDao.VerifyAdminPassword(model.Password))
            {
                var admin = AccountDao.GetSuperAdmin();
                identity.AccountType = RMAccountType.Local;
                identity.AccountId = admin.Id;
                identity.RegisterEmail = model.RegistedEmail;
                identity.TenantGroupId = model.TenantGroupId;
                identity.IsAuthenticated = true;
            }
            else
            {
                identity.IsAuthenticated = false;
            }
            
            return identity;
        }
    }
}
