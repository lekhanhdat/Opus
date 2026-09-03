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
using AvePoint.RA.Contract.Object;
using System.Collections.Generic;

namespace AvePoint.RA.Contract.AAD
{
    public interface IAccountWrapperService
    {
        /// <summary>
        /// Will search the users/groups from Azure AD based on the searchstring
        /// </summary>
        /// <param name="tenantId">refer to customer id in AOS, but not O365 tenant id</param>
        /// <param name="searchString"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        List<AADAccount> SearchAccounts(string tenantId, string searchString, int top = 20, bool onlyIncludeAAdUser = false);

        List<AADAccount> SearchAccounts(string tenantId, string searchString, string appProfileId, int top = 20);
        List<AADAccount> SearchAccounts4FSConnection(string tenantId, string searchString, int top = 20);

        List<AADAccount> GetTeamSiteGroupOwners(string tenantId, string aadId, string office365TenantId);

        List<AADAccount> GetGroupsByAadIds(string tenantId, List<string> groupAadIds, string office365TenantId);
        AADAccount GetGroupsByAadId(string tenantId, string groupAadId);

        AADAccount GetGroupsByIdOrGroupEmail(string tenantId, string groupAadId, string groupEmail);
        AADAccount GetAccountByIdOrUPN(string tenantId, string userId, string userPrincipalName);

        List<AADAccount> GetAccountsByUserEmials(string tenantId, List<string> userEmails, string office365TenantId);

        List<AADAccount> GetAccountsByUserOrGroupEmails(string tenantId, List<string> emails);

        List<AADAccount> GetGroupsByUserId(string tenantId, string userId, string office365TenantId);

        /// <summary>
        /// will get user from Azure AD
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="userIdOrUPN">user id or user principle name in O365</param>
        /// <returns></returns>
        AADAccount GetAccount(string tenantId, string userIdOrUPN);

        AADAccount GetAccount(Cloud.Sdk.Data.AosModern.AppProfileInfo profile, string userIdOrUPN);

        /// <summary>
        /// Register the AAD user/group to AOS
        /// </summary>
        /// <param name="tenantId">refer to customer id in AOS, but not O365 tenant id</param>
        /// <param name="accounts"></param>
        /// <returns></returns>
        List<AADAccount> Regester2AOS(string tenantId, IList<AADAccount> accounts);

        IList<AADAccount> Regester2AOS(string tenantId, string o365TenantId, IList<AADAccount> accounts);
        List<AADAccount> GetAADAccounts(List<AADAccount> accounts, string customerId);
    }
}
