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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class PhysicalAuditWorker
    {
        public static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        public static Dictionary<string, DB.Model.RMAccount> AccountDic = [];

        public static async Task<string> BuildPhysicalActionAuditAsync(string actionAudit, PhysicalActionType actionType, bool isNew)
        {
            var auditList = new List<PhysicalAudit>();

            if (!AccountDic.TryGetValue(TenantLocalValue.LogonUserEmail, out var account))
            {
                account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                AccountDic.TryAdd(TenantLocalValue.LogonUserEmail, account);
            }

            var physicalAuditInfo = new PhysicalAudit()
            {
                ActionTime = DateTime.UtcNow.Ticks,
                ActionType = actionType,
                ActionUser = account.DisplayName,
            };

            if (!isNew)
            {
                if (!string.IsNullOrEmpty(actionAudit))
                {
                    auditList = JsonConvert.DeserializeObject<List<PhysicalAudit>>(actionAudit);
                }
            }

            if (auditList.Count == 20)
            {
                auditList.RemoveAt(0);
            }

            auditList.Add(physicalAuditInfo);
            return JsonConvert.SerializeObject(auditList);
        }
    }
}
