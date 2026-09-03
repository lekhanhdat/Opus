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
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.Audit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Service.Services.AccountManager.AuditHandler
{
    public class AccountManagementBeforeAuditHandler : IBeforeAuditHandler
    {
        private AveLogger logger = AveLogger.GetInstance(typeof(AccountManagementBeforeAuditHandler));
        public IAccountManagerService AccountService { get; set; }

        public void Collect(out RMAuditInfo info, int model, int category, int action, object[] args, object target)
        {
            info = new RMAuditInfo();
            try
            {
                info.Module = (AuditModule)model;
                info.Category = (AuditCategory)category;
                info.Action = (AuditAction)action;
                switch (info.Action)
                {
                    case AuditAction.AddADAccount:
                        AddADAccount(ref info, args);
                        break;
                    case AuditAction.EditLocalAccount:
                        info.Object = AccountService.GetSuperAdminName();
                        break;
                    case AuditAction.DeleteAccount:
                        DeleteAccount(ref info, args);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
        }

        private void AddADAccount(ref RMAuditInfo info, object[] args)
        {
            List<RMADAccountDto> accounts = args[0] as List<RMADAccountDto>;
            if (accounts != null)
            {
                int total = accounts.Count();
                if (total > 0)
                {
                    info.Object = accounts[0].LoginName;
                    for (int i = 1; i < total; i++)
                    {
                        info.Object += ";" + accounts[i].LoginName;
                    }
                }
            }
        }

        private void DeleteAccount(ref RMAuditInfo info, object[] args)
        {
            List<int> ids = null;
            if (args[0] is List<int>)     //多选delete
            {
                ids = args[0] as List<int>;
            }
            else    //单选delete
            {
                ids = new List<int>() { (int)args[0] };
            }

            var accounts = AccountService.GetAccounts(ids);
            int total = accounts.Count();
            if (total > 0)
            {
                info.Object = accounts[0].LoginName;
                for (int i = 1; i < total; i++)
                {
                    info.Object += ";" + accounts[i].LoginName;
                }
            }
        }
    }
}
