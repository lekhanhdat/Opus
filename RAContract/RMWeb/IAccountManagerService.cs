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
using AvePoint.RA.Contract.RMWeb.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IAccountManagerService
    {
        string GetSuperAdminName();

        bool ValidateLocalAccountPassword(string password);

        bool ChangeLocalAccountPassword(string newPassword, out RMOperatingAccountError errorType);

        bool AddADAccounts(ref List<RMADAccountDto> accounts);

        bool DeleteADAccount(int id);

        bool DeleteADAccounts(List<int> ids);

        /// <param name="pageIndex">第几页，从1开始</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="totalRecord">总记录数</param>
        /// <returns></returns>
        List<RMADAccountDto> GetAccounts(int pageIndex, int pageSize, out int totalRecord);

        List<RMADAccountDto> GetAccounts(List<int> ids);

        List<RMADAccountDto> SearchAccountSuggestion(string key, int perDomainCount);

        RMADAccountDto SearchAccountSuggestion(string name);

        RMADAccountDto SearchSingleAccount(string fullName);

        string GetAdminSecurityToken(string password);
        RMADAccountDto GetADAcountByLoginName(string loginName);
    }
}
