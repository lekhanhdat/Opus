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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao;
using ExchangeUtility.Graph;

namespace AvePoint.RA.RAExchange.Common
{
    public static class EXOGraphApiResolver
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger Logger =
            RALogger.GetInstance(typeof(EXOGraphApiResolver));

        private const string SUPPORT_GRAPH_API = "EXOJOB_USING_GRAPH_API";
        private const string MAILBOX_LIST_KEY = "EXOJOB_GRAPH_MAILBOX_LIST";
        private const int MAX_MAILBOX_COUNT = 5;

        private static bool? _cachedTenantFlag;
        private static HashSet<string> _cachedMailboxList;
        private static readonly object _cacheLock = new object();
        public static bool IsGraphEnabled(IRMKeyValueDao keyValueDao)
        {
            var value = keyValueDao.GetValueByKeyAsync(SUPPORT_GRAPH_API).Result;
            return bool.TryParse(value, out var flag) && flag;
        }

        public static bool ShouldUseGraph(IRMKeyValueDao keyValueDao, string mailboxAddress, string mailboxGuid, ExchangeOnlineTreeNodeDto treeNodeDto)
        {
            EnsureCacheLoaded(keyValueDao);
            Logger.Info($"The scan job is using Graph API: {_cachedTenantFlag}");
            if (IsArchiveMailbox(mailboxAddress, mailboxGuid, treeNodeDto))
            {
                Logger.Info($"Mailbox {mailboxGuid} is an archive mailbox, not using Graph API.");
                return false;
            }
            if (_cachedTenantFlag.Value)
            {
                return true;
            }

            var result = _cachedMailboxList.Contains(mailboxAddress);
            Logger.Info($"Mailbox {mailboxGuid} is using Graph API: {result}");
            return result;
        }

        private static bool IsArchiveMailbox(string mailboxAddress, string mailboxGuid, ExchangeOnlineTreeNodeDto treeNodeDto)
        {
            var mailboxNode = TreeManagement.GetMailboxNode(treeNodeDto);
            var mailboxType = ExchangeMailboxType.User;
            if (mailboxNode.Type == NodeType.EOO365Group)
            {
                mailboxType = ExchangeMailboxType.Group;
            }
            var mailbox = new ExchangeMailbox(mailboxAddress, mailboxType, mailboxGuid);
            return mailbox.IsArchiveMailbox;
        }

        public static async Task<bool> ShouldUseGraphAsync(IRMKeyValueDao keyValueDao, string mailboxAddress, string mailboxGuid)
        {
            await EnsureCacheLoadedAsync(keyValueDao);
            Logger.Info($"The scan job is using Graph API: {_cachedTenantFlag.Value}");

            if (_cachedTenantFlag.Value)
            {
                return true;
            }

            var result = _cachedMailboxList.Contains(mailboxAddress);
            Logger.Info($"Mailbox {mailboxGuid} is using Graph API: {result}");
            return result;
        }

        private static void EnsureCacheLoaded(IRMKeyValueDao keyValueDao)
        {
            if (_cachedTenantFlag.HasValue)
            {
                return;
            }

            lock (_cacheLock)
            {
                if (_cachedTenantFlag.HasValue)
                {
                    return;
                }

                LoadCache(keyValueDao);
            }
        }

        private static async Task EnsureCacheLoadedAsync(IRMKeyValueDao keyValueDao)
        {
            if (_cachedTenantFlag.HasValue)
            {
                return;
            }

            lock (_cacheLock)
            {
                if (_cachedTenantFlag.HasValue)
                {
                    return;
                }

                LoadCacheAsync(keyValueDao).GetAwaiter().GetResult();
            }
        }

        private static void LoadCache(IRMKeyValueDao keyValueDao)
        {
            try
            {
                var tenantValue = keyValueDao.GetValueByKeyAsync(SUPPORT_GRAPH_API).Result;
                _cachedTenantFlag = bool.TryParse(tenantValue, out var flag) && flag;

                var listValue = keyValueDao.GetValueByKeyAsync(MAILBOX_LIST_KEY).Result;
                _cachedMailboxList = ParseMailboxList(listValue);

                Logger.Info($"Graph API config cached - Tenant flag: {_cachedTenantFlag}, Mailbox count: {_cachedMailboxList.Count}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load Graph API config, falling back to EWS. Error: {ex}");
                _cachedTenantFlag = false;
                _cachedMailboxList = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static async Task LoadCacheAsync(IRMKeyValueDao keyValueDao)
        {
            try
            {
                var tenantValue = await keyValueDao.GetValueByKeyAsync(SUPPORT_GRAPH_API);
                _cachedTenantFlag = bool.TryParse(tenantValue, out var flag) && flag;

                var listValue = await keyValueDao.GetValueByKeyAsync(MAILBOX_LIST_KEY);
                _cachedMailboxList = ParseMailboxList(listValue);

                Logger.Info($"Graph API config cached - Tenant flag: {_cachedTenantFlag}, Mailbox count: {_cachedMailboxList.Count}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load Graph API config, falling back to EWS. Error: {ex}");
                _cachedTenantFlag = false;
                _cachedMailboxList = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static HashSet<string> ParseMailboxList(string listValue)
        {
            if (string.IsNullOrWhiteSpace(listValue))
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            var mailboxes = listValue.Split(';').Select(email => email.Trim()).Where(email => !string.IsNullOrWhiteSpace(email)).ToList();

            if (mailboxes.Count > MAX_MAILBOX_COUNT)
            {
                Logger.Warn($"Graph mailbox list exceeds {MAX_MAILBOX_COUNT}. Using first {MAX_MAILBOX_COUNT} entries only.");
                mailboxes = mailboxes.Take(MAX_MAILBOX_COUNT).ToList();
            }

            return new HashSet<string>(mailboxes, StringComparer.OrdinalIgnoreCase);
        }
    }
}