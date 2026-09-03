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
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.CustomizeConnector.Model;
using AvePoint.RA.Contract.CustomizeConnector.Model.Api;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.CustomizeConnector.ColumnManager
{
    public class PeopleOrGroupConnectorColumn : IConnectorColumn
    {
        public Contract.TemplateManagement.ColumnType Type => Contract.TemplateManagement.ColumnType.PeopleOrGroup;

        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();

        private IAccountWrapperService AccountWrapperService => PlatformWindsorManager.GetService<IAccountWrapperService>();

        private readonly ConcurrentDictionary<string, AOSUserDto> UserAndGroups = new();

        public PeopleOrGroupConnectorColumn()
        {
            var accounts = UserService.GetManagementUsersForAosDtoAsync().Result;
            foreach(var account in accounts)
            {
                if(string.IsNullOrEmpty(account.UserPrincipalName))
                {
                    account.UserPrincipalName = account.UserName;
                }
              
                account.UserPrincipalName = account.UserPrincipalName.ToLower();

                UserAndGroups.TryAdd(account.UserPrincipalName, account);
            }
        }

        public bool DefinitionValidate(CustomizeConnectorColumnInfo columnInfo)
        {
            return true;
        }

        public async Task<(bool, CustomColumn)> TryConvertToCustomColumnAsync(CustomizeConnectorColumnInfo columnInfo, object valueJson)
        {
            CustomColumn customColumn = null;
            if(valueJson is not List<object> emailObjs || emailObjs.Count == 0)
            {
                return (false, customColumn);
            }

            var emails = emailObjs.ConvertAll(item => item.ToString().ToLower());
            var aosAccounts = new List<AOSUserDto>();
            foreach(var email in emails)
            {
                if(UserAndGroups.TryGetValue(email, out var value))
                {
                    aosAccounts.Add(value);
                }
            }

            customColumn = new CustomColumn
            {
                Users = aosAccounts
            };

            return (true, customColumn);
        }

        public async Task<CustomizeConnectorDataValidateResult> ValueValidateAsync(CustomizeConnectorColumnInfo columnInfo, object valueJson)
        {
            if (columnInfo.IsRequired && string.IsNullOrEmpty(valueJson?.ToString()))
            {
                return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_IsRequired"), columnInfo.InternalName));
            }

            if (!columnInfo.IsRequired && string.IsNullOrEmpty(valueJson?.ToString()))
            {
                return CustomizeConnectorDataValidateResult.Validated();
            }

            if(valueJson is not List<object> emailObjs)
            {
                return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_IsIllegal"), columnInfo.InternalName));
            }

            var emails = emailObjs.ConvertAll(item => item.ToString());
            var unValidEmails = emails.Where(item => !IsValidEmail(item));
            if(unValidEmails.Any())
            {
                return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_IsValidEmail"), columnInfo.InternalName, string.Join(", ", unValidEmails)));
            }

            var needQueryEmails = emails.Where(item => !UserAndGroups.ContainsKey(item.ToLower())).ToList();
            if(needQueryEmails.Count == 0)
            {
                return CustomizeConnectorDataValidateResult.Validated();
            }

            var accounts = AccountWrapperService.GetAccountsByUserOrGroupEmails(TenantLocalValue.LogonGroupId, needQueryEmails);
            if(accounts.Count == 0)
            {
                return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_IsExistEmail"), columnInfo.InternalName, string.Join(", ", needQueryEmails)));
            }
            var syncRes = await UserService.SyncUsersAsync(TenantLocalValue.LogonGroupId, accounts);
            if(!syncRes)
            {
                return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_SyncEmailFailed"), columnInfo.InternalName, string.Join(", ", needQueryEmails)));
            }

            var aosAccountDtos = accounts.ConvertAll(item => AADAccount.Convert2AOSUserDto(item));
            foreach(var aosAccount in aosAccountDtos)
            {
                if (string.IsNullOrEmpty(aosAccount.UserPrincipalName))
                {
                    aosAccount.UserPrincipalName = aosAccount.UserName;
                }
                aosAccount.UserPrincipalName = aosAccount.UserPrincipalName.ToLower();
                UserAndGroups.TryAdd(aosAccount.UserPrincipalName, aosAccount);
            }

            needQueryEmails = needQueryEmails.Where(item => !aosAccountDtos.Exists(i => i.UserPrincipalName.Equals(item, StringComparison.OrdinalIgnoreCase))).ToList();
            if(needQueryEmails.Count > 0)
            {
                return CustomizeConnectorDataValidateResult.UnValidated(string.Format(I18NEntity.GetString("RM_Connector_Validate_IsExistEmail"), columnInfo.InternalName, string.Join(", ", needQueryEmails)));
            }

            return CustomizeConnectorDataValidateResult.Validated();
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                static string DomainMapper(Match match)
                {
                    var idn = new IdnMapping();
                    string domainName = idn.GetAscii(match.Groups[2].Value);
                    return match.Groups[1].Value + domainName;
                }
            }
            catch
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public async Task<CustomizeConnectorNameValue<string>> ConvertToNameValueAsync(CustomizeConnectorColumnInfo columnInfo, Dictionary<string, CustomColumn> customColumnDic, bool forDisplay = true)
        {
            var res = new CustomizeConnectorNameValue<string>
            {
                Name = columnInfo.Name,
                Value = "",
            };

            if (customColumnDic.TryGetValue(columnInfo.Id.ToString(), out var customColumn))
            {
                var users = customColumn.Users.Select(item => item.DisplayName).ToList();
                res.Value = string.Join("; ", users);
            }

            return res;
        }

        public bool TryConvertToRulePolicy(CustomizeConnectorColumnInfo columnInfo, Dictionary<string, CustomColumn> customColumnDic, out object value)
        {
            if (!customColumnDic.TryGetValue(columnInfo.Id.ToString(), out var customColumn))
            {
                value = null;
                return false;
            }

            value = customColumn.Users;
            return true;
        }
    }
}
