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


namespace ExchangeUtility.Graph.PowerShellRestAPI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    //using AvePoint.Application.Configuration.Utils;
    using ExchangeBackupUtility.Graph.PowerShellRestAPI;
    using ExchangeCommonWrapper;
    using Util.MSAzure;
    using Util.Outlook;
    using Microsoft.Exchange.WebServices.Data;

    using AvePoint.RA.CommonUtil;
    using Newtonsoft.Json;
    using Polly;
    using Mailbox = Util.Outlook.Mailbox;
    using Task = System.Threading.Tasks.Task;

    public class OutlookService
    {
        protected static RALogger logger = RALogger.GetInstance(typeof(OutlookService));
        private readonly OutlookClient client;
        private string tenantId;
        private Func<string> GetAccessToken;
        private string pfPrimaySmtpAddress;
        private Dictionary<string, string> pfMailboxDic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public OutlookService(IAuthObject authObject)
        {
            var tokenProvider = authObject as AOSTokenAuthObjectV2;
            ArgumentNullException.ThrowIfNull(tokenProvider);
            GetAccessToken = tokenProvider.GetAccessToken;
            tenantId = tokenProvider.TenantId;
            client = new OutlookClient(GetUserAgent(), authObject.Environment);
        }

        public OutlookService(string tenantId, string token, AzureEnvironment env)
        {
            this.tenantId = tenantId;
            this.GetAccessToken = () => token;
            client = new OutlookClient(GetUserAgent(), env);
        }

        private string GetUserAgent()
        {
            var version = ExchangeGlobalConfig.ProductVersion;
            var userAgent = string.Format("ISV|AvePoint|CloudRecords/{0}", version.IsNullOrWhiteSpace() ? "1.0" : version);
            logger.Debug($"User Agent: {userAgent}. version: {version}");
            return userAgent;
            //studo::
        //    return !string.IsNullOrEmpty(OEMSettingConfigUtil.UserAgent)
        //         ? OEMSettingConfigUtil.UserAgent
        //         : "Mozilla/5.0 (Windows NT; Windows NT 10.0; en-US) WindowsPowerShell/5.1.22000.1335";
        }

        #region Basic method

        public class QueryResult : QueryResult<Hashtable> { }

        public class QueryResult<T>
        {
            [JsonExtensionData()]
            public IDictionary<string, object> Members { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "value", Required = Required.Default)]
            public List<T> Values;
        }

        private async Task<T> QueryAsync<T>(string cmdletName, Dictionary<string, object> parameters, OutlookHeader header = null)
        {
            return await QueryAsync<T>(cmdletName, parameters, () => header);
        }

        private async Task<T> QueryAsync<T>(string cmdletName, Dictionary<string, object> parameters, Func<OutlookHeader> header)
        {
            var command = new CommandRequest()
            {
                CmdletInput = new CmdletInput()
                {
                    CmdletName = cmdletName,
                    Parameters = parameters
                }
            };
            return await InvokeWithRetryAsync(delegate
            {
                return client.GetService<IOutlookService<T>>(GetAccessToken(), tenantId).InvokeCommandAsync(command, header?.Invoke());
            });
        }

        /// <summary>
        /// Unified exception handling logic
        /// </summary>
        private async Task<T> InvokeWithRetryAsync<T>(Func<Task<T>> func)
        {
            return await Policy.Handle<OutlookException>(e =>
            {
                logger.Warn($"Failed to call command, details: {e}. StatusCode: {e.StatusCode}. ");
                switch (e.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        return true;
                    case HttpStatusCode.InternalServerError:
                        if (e.InnerException?.Message?.Contains("is not found in the local forest. Please connect to the right") ?? false)
                        {
                            HandleNotFoundInLocalForestException(e.InnerException.Message);
                            return true;
                        }
                        return false;
                    case HttpStatusCode.BadRequest:
                        {
                            if (e.Message?.Contains("Cmdlet needs proxy.", StringComparison.OrdinalIgnoreCase) ?? false)
                            {
                                SetPFPrimarySmtpAddressAsync().ExecuteAsyncTask();
                                return true;
                            }
                            return false;
                        }
                    default:
                        return false;
                }
            }).RetryAsync(1)
            .ExecuteAsync(func);
        }

        private void HandleNotFoundInLocalForestException(string errorMessage)
        {
            var exception = JsonConvert.DeserializeObject<EXOPSException>(errorMessage);
            var contentMailboxId = exception.Error.Details.FirstOrDefault()?.Target;
            SetPFPrimarySmtpAddressAsync(contentMailboxId).ExecuteAsyncTask();
        }

        private bool IsKnownError(OutlookException exception)
        {
            if (!string.IsNullOrEmpty(exception.InnerException?.Message))
            {
                if (exception.InnerException.Message.Contains("ProxyAddressExistsException", StringComparison.OrdinalIgnoreCase)
                    || exception.InnerException.Message.Contains("NotAcceptedDomainException", StringComparison.OrdinalIgnoreCase)
                    || exception.InnerException.Message.Contains("The role assigned to application", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return exception.Message?.Contains("ManagementObjectNotFoundException") ?? false;
        }
        #endregion

        #region public method
        public async Task<PublicFolderMetadata> GetPublicFolderMetadataAsync(string identity)
        {
            logger.Info("Get public folder[{0}].", identity);
            var queryResult = await QueryAsync<QueryResult>("Get-PublicFolder", new Dictionary<string, object> { { "Identity", identity } },
                () => new OutlookHeader("X-AnchorMailbox", pfPrimaySmtpAddress));
            var info = queryResult?.Values?.FirstOrDefault();
            if (info == null)
            {
                logger.Warn("Failed to get public folder {0}.", identity);
                return null;
            }
            var publicFolderInfo = new PublicFolderMetadata() { General = new General(), Limits = new Limits() };
            publicFolderInfo.MailEnabled = Convert.ToBoolean(info["MailEnabled"]?.ToString());
            publicFolderInfo.Identity = info["Identity"]?.ToString();
            publicFolderInfo.General.Name = info["Name"]?.ToString();
            publicFolderInfo.General.PerUserReadStateEnabled = Convert.ToBoolean(info["PerUserReadStateEnabled"]?.ToString());
            publicFolderInfo.Limits.IssueWarningQuota = info["IssueWarningQuota"]?.ToString();
            publicFolderInfo.Limits.ProhibitPostQuota = info["ProhibitPostQuota"]?.ToString();
            publicFolderInfo.Limits.MaxItemSize = info["MaxItemSize"]?.ToString();
            publicFolderInfo.Limits.RetainDeletedItemsFor = info["RetainDeletedItemsFor"]?.ToString();
            publicFolderInfo.Limits.AgeLimit = info["AgeLimit"]?.ToString();
            logger.Info("Public Folder[{0}] name: {1}, mailEnabled: {2}", identity, publicFolderInfo.General.Name, publicFolderInfo.MailEnabled);
            return publicFolderInfo;
        }

        public async Task<PublicFolderMetadata> GetMailPublicFolderMetadataAsync(string identity)
        {
            logger.Info("Get mail public folder[{0}].", identity);

            var publicFolderInfo = new PublicFolderMetadata()
            {
                GeneralMailProperties = new GeneralMailProperties(),
                EmailAddress = new PFEmailAddress(),
                DeliveryOptions = new DeliveryOptions(),
                MailFlowSettings = new MailFlowSettings()
            };
            var queryResult = await QueryAsync<QueryResult<MailPublicFolder>>("Get-MailPublicFolder", new Dictionary<string, object> { { "Identity", identity } },
                () => new OutlookHeader("X-AnchorMailbox", pfPrimaySmtpAddress));
            var info = queryResult?.Values?.FirstOrDefault();

            if (info == null)
            {
                logger.Warn("Failed to get mail public folder {0}.", identity);
                return null;
            }
            publicFolderInfo.GeneralMailProperties.Alias = info.Alias;
            publicFolderInfo.GeneralMailProperties.DisplayName = info.DisplayName;
            publicFolderInfo.GeneralMailProperties.Id = info.Id;
            publicFolderInfo.GeneralMailProperties.HiddenFromAddressListsEnabled = info.HiddenFromAddressListsEnabled;
            publicFolderInfo.GeneralMailProperties.CustomAttribute1 = info.CustomAttribute1;
            publicFolderInfo.GeneralMailProperties.CustomAttribute2 = info.CustomAttribute2;
            publicFolderInfo.GeneralMailProperties.CustomAttribute3 = info.CustomAttribute3;
            publicFolderInfo.GeneralMailProperties.CustomAttribute4 = info.CustomAttribute4;
            publicFolderInfo.GeneralMailProperties.CustomAttribute5 = info.CustomAttribute5;
            publicFolderInfo.GeneralMailProperties.CustomAttribute6 = info.CustomAttribute6;
            publicFolderInfo.GeneralMailProperties.CustomAttribute7 = info.CustomAttribute7;
            publicFolderInfo.GeneralMailProperties.CustomAttribute8 = info.CustomAttribute8;
            publicFolderInfo.GeneralMailProperties.CustomAttribute9 = info.CustomAttribute9;
            publicFolderInfo.GeneralMailProperties.CustomAttribute10 = info.CustomAttribute10;
            publicFolderInfo.GeneralMailProperties.CustomAttribute11 = info.CustomAttribute11;
            publicFolderInfo.GeneralMailProperties.CustomAttribute12 = info.CustomAttribute12;
            publicFolderInfo.GeneralMailProperties.CustomAttribute13 = info.CustomAttribute13;
            publicFolderInfo.GeneralMailProperties.CustomAttribute14 = info.CustomAttribute14;
            publicFolderInfo.GeneralMailProperties.CustomAttribute15 = info.CustomAttribute15;
            publicFolderInfo.EmailAddress.EmailAddresses = info.EmailAddresses;
            publicFolderInfo.DeliveryOptions.GrantSendOnBehalfTo = info.GrantSendOnBehalfTo;
            publicFolderInfo.DeliveryOptions.ForwardingAddress = info.ForwardingAddress;
            publicFolderInfo.DeliveryOptions.DeliverToMailboxAndForward = info.DeliverToMailboxAndForward;
            publicFolderInfo.MailFlowSettings.MaxSendSize = info.MaxSendSize;
            publicFolderInfo.MailFlowSettings.MaxReceiveSize = info.MaxReceiveSize;
            publicFolderInfo.MailFlowSettings.AcceptMessagesOnlyFrom = info.AcceptMessagesOnlyFrom;
            publicFolderInfo.MailFlowSettings.AcceptMessagesOnlyFromDLMembers = info.AcceptMessagesOnlyFromDLMembers;
            publicFolderInfo.MailFlowSettings.RequireSenderAuthenticationEnabled = info.RequireSenderAuthenticationEnabled;
            publicFolderInfo.MailFlowSettings.RejectMessagesFrom = info.RejectMessagesFrom;
            publicFolderInfo.MailFlowSettings.RejectMessagesFromDLMembers = info.RejectMessagesFromDLMembers;
            return publicFolderInfo;
        }

        public async Task<string[]> GetPublicFolderPermissionMetadataAsync(string alias)
        {
            logger.Info("Get public folder [{0}] send as permission.");
            var trustees = new List<string>();
            var result = await QueryAsync<QueryResult>("Get-RecipientPermission", new Dictionary<string, object> { ["Identity"] = alias });
            if (result == null || result.Values == null)
            {
                logger.Warn("Failed to get public folder {0} send as permission.", alias);
                return Array.Empty<string>();
            }

            foreach (var info in result.Values)
            {
                var trustee = info["Trustee"]?.ToString();
                if (string.IsNullOrEmpty(trustee))
                {
                    logger.Info("Public Folder[{0}] trustee is null.", alias);
                    continue;
                }
                else
                {
                    trustees.Add(trustee.ToString());
                }
            }
            return trustees.ToArray();
        }

        public async Task SetPublicFolderStatusAsync(string identity, PublicFolderMetadata pfMetadata)
        {
            logger.Info("Set public folder [{0}] status: {1}.", identity, pfMetadata.MailEnabled);
            if (pfMetadata.MailEnabled)
            {
                await EnableMailPublicFolderAsync(identity);
            }
        }

        public async Task EnableMailPublicFolderAsync(string identity)
        {
            await QueryAsync<QueryResult>("Enable-MailPublicFolder",
                          new Dictionary<string, object>
                          {
                              ["Identity"] = identity,
                              ["Confirm"] = false
                          },
                          () => new OutlookHeader("X-AnchorMailbox", pfPrimaySmtpAddress));
        }

        public async Task SetPublicFolderMetadataAsync(string identity, PublicFolderMetadata pfMetadata)
        {
            logger.Info("Set public folder[{0}].", identity);
            var commandParameters = new Dictionary<string, object>
            {
                ["Identity"] = identity,
                ["Name"] = pfMetadata.General.Name,
                ["PerUserReadStateEnabled"] = pfMetadata.General.PerUserReadStateEnabled,
                ["IssueWarningQuota"] = pfMetadata.Limits.IssueWarningQuota,
                ["ProhibitPostQuota"] = pfMetadata.Limits.ProhibitPostQuota,
                ["MaxItemSize"] = pfMetadata.Limits.MaxItemSize,
                ["RetainDeletedItemsFor"] = pfMetadata.Limits.RetainDeletedItemsFor,
                ["AgeLimit"] = pfMetadata.Limits.AgeLimit
            };
            if (!pfMetadata.MailEnabled)
                commandParameters.Add("MailEnabled", pfMetadata.MailEnabled);
            await QueryAsync<QueryResult>("Set-PublicFolder", commandParameters,
                () => new OutlookHeader("X-AnchorMailbox", pfPrimaySmtpAddress));
        }

        public async Task SetMailPublicFolderMetadataAsync(string identity, PublicFolderMetadata pfMetadata)
        {
            logger.Info("Set mail public folder[{0}].", identity);
            await QueryAsync<QueryResult>(
                  "Set-MailPublicFolder",
                  new Dictionary<string, object>
                  {
                      ["Identity"] = identity,
                      ["Alias"] = pfMetadata.GeneralMailProperties.Alias,
                      ["DisplayName"] = pfMetadata.GeneralMailProperties.DisplayName,
                      ["HiddenFromAddressListsEnabled"] = pfMetadata.GeneralMailProperties.HiddenFromAddressListsEnabled,
                      ["CustomAttribute1"] = pfMetadata.GeneralMailProperties.CustomAttribute1,
                      ["CustomAttribute2"] = pfMetadata.GeneralMailProperties.CustomAttribute2,
                      ["CustomAttribute3"] = pfMetadata.GeneralMailProperties.CustomAttribute3,
                      ["CustomAttribute4"] = pfMetadata.GeneralMailProperties.CustomAttribute4,
                      ["CustomAttribute5"] = pfMetadata.GeneralMailProperties.CustomAttribute5,
                      ["CustomAttribute6"] = pfMetadata.GeneralMailProperties.CustomAttribute6,
                      ["CustomAttribute7"] = pfMetadata.GeneralMailProperties.CustomAttribute7,
                      ["CustomAttribute8"] = pfMetadata.GeneralMailProperties.CustomAttribute8,
                      ["CustomAttribute9"] = pfMetadata.GeneralMailProperties.CustomAttribute9,
                      ["CustomAttribute10"] = pfMetadata.GeneralMailProperties.CustomAttribute10,
                      ["CustomAttribute11"] = pfMetadata.GeneralMailProperties.CustomAttribute11,
                      ["CustomAttribute12"] = pfMetadata.GeneralMailProperties.CustomAttribute12,
                      ["CustomAttribute13"] = pfMetadata.GeneralMailProperties.CustomAttribute13,
                      ["CustomAttribute14"] = pfMetadata.GeneralMailProperties.CustomAttribute14,
                      ["CustomAttribute15"] = pfMetadata.GeneralMailProperties.CustomAttribute15,
                      ["EmailAddresses"] = pfMetadata.EmailAddress.EmailAddresses,
                      ["GrantSendOnBehalfTo"] = pfMetadata.DeliveryOptions.GrantSendOnBehalfTo,
                      ["ForwardingAddress"] = pfMetadata.DeliveryOptions.ForwardingAddress,
                      ["DeliverToMailboxAndForward"] = pfMetadata.DeliveryOptions.DeliverToMailboxAndForward,
                      ["MaxSendSize"] = pfMetadata.MailFlowSettings.MaxSendSize,
                      ["MaxReceiveSize"] = pfMetadata.MailFlowSettings.MaxReceiveSize,
                      ["AcceptMessagesOnlyFrom"] = pfMetadata.MailFlowSettings.AcceptMessagesOnlyFrom,
                      ["AcceptMessagesOnlyFromDLMembers"] = pfMetadata.MailFlowSettings.AcceptMessagesOnlyFromDLMembers,
                      ["RequireSenderAuthenticationEnabled"] = pfMetadata.MailFlowSettings.RequireSenderAuthenticationEnabled,
                      ["RejectMessagesFrom"] = pfMetadata.MailFlowSettings.RejectMessagesFrom,
                      ["RejectMessagesFromDLMembers"] = pfMetadata.MailFlowSettings.RejectMessagesFromDLMembers
                  },
                  () => new OutlookHeader("X-AnchorMailbox", pfPrimaySmtpAddress));
        }

        public async Task SetPublicFolderPermissionMetadataAsync(PublicFolderMetadata pfMetadata)
        {
            var count = pfMetadata.DeliveryOptions.Trustees.Length;
            logger.Info("Set public folder [{0}] permission, the id is {1}, the trustees count is {2}, the trustees are {3}.",
                pfMetadata.GeneralMailProperties.Alias, pfMetadata.GeneralMailProperties.Id, count, string.Join(".", pfMetadata.DeliveryOptions.Trustees));
            if (count == 0)
            {
                return;
            }
            var watch = Stopwatch.StartNew();
            int index = 0;
            var identity = pfMetadata.GeneralMailProperties.Id ?? pfMetadata.GeneralMailProperties.Alias;
            foreach (var trustee in pfMetadata.DeliveryOptions.Trustees)
            {
                ++index;
                await AddRecipientPermissionAsync<QueryResult>(identity, trustee);
                if (index < count)
                    Thread.Sleep(20000);//A certain interval is required, otherwise the previous result will be overwritten
            }
            watch.Stop();
            logger.Info($"Set recipient permission finished. count: {count}, time: {watch.Elapsed}.");
        }

        public async Task<T> AddRecipientPermissionAsync<T>(string identity, string trustee)
        {
            return await QueryAsync<T>("Add-RecipientPermission",
                  new Dictionary<string, object>
                  {
                      ["Identity"] = identity,
                      ["AccessRights"] = "SendAs",
                      ["Confirm"] = false,
                      ["Trustee"] = trustee
                  });
        }

        public async Task AddPublicFolderClientPermissionAsync(string identity, string userName, FolderPermissionLevel permission)
        {
            await QueryAsync<QueryResult>("Add-PublicFolderClientPermission",
                       new Dictionary<string, object>
                       {
                           ["Identity"] = identity,
                           ["User"] = userName,
                           ["AccessRights"] = permission.ToString()
                       },
                       () => new OutlookHeader("X-AnchorMailbox", pfPrimaySmtpAddress));
        }

        public async Task<Dictionary<string, string>> GetPFPrimarySmtpAddressAsync(string identity)
        {
            var mailboxs = await QueryAsync<QueryResult<PFMailbox>>("Get-Mailbox",
                 new Dictionary<string, object>
                 {
                     ["PublicFolder"] = true,
                     ["Identity"] = identity,
                 });
            return mailboxs.Values.ToDictionary(m => m.ExchangeGuid, m => m.PrimarySmtpAddress);
        }

        public async Task<PSBaseObject> GetOrganizationConfigAsync()
        {
            var configs = await QueryAsync<QueryResult<PSBaseObject>>("Get-OrganizationConfig", new());
            return configs.Values.FirstOrDefault();
        }

        public async Task SetPFPrimarySmtpAddressAsync(string rootPFMailboxId = null)
        {
            if (string.IsNullOrEmpty(rootPFMailboxId))
            {
                var config = await GetOrganizationConfigAsync();
                rootPFMailboxId = config.AdditionalData.TryGetValue("RootPublicFolderMailbox", out var mailboxId) ? mailboxId?.ToString() : null;
            }
            if (rootPFMailboxId != null && pfMailboxDic.TryGetValue(rootPFMailboxId, out var address))
            {
                this.pfPrimaySmtpAddress = address;
            }
            else
            {
                var mailboxs = await GetPFPrimarySmtpAddressAsync(rootPFMailboxId);
                pfMailboxDic.AddRange(mailboxs, true);
                this.pfPrimaySmtpAddress = mailboxs.Values.FirstOrDefault();
            }
        }

        #endregion

        #region EX GROUP
        public async Task<bool> SetUnifiedGroupAddressAsync(string identity, string emailAddress)
        {
            bool flag = true;
            try
            {
                logger.Info("Start to set office 365 group. Identity: {0}. SmtpAddress: {1}. ", identity, emailAddress);
                await QueryAsync<QueryResult>("Set-UnifiedGroup",
                           new Dictionary<string, object>
                           {
                               ["Identity"] = identity,
                               ["PrimarySmtpAddress"] = emailAddress
                           });
            }
            catch (OutlookException saex) when (IsKnownError(saex))
            {
                logger.Error("Failed to set group address throughout cmdlet. Reason:{0}", saex);
                flag = false;
            }
            catch (Exception ex)
            {
                logger.Error("SetUnifiedGroupAddressAsync. Failed to set group address throughout cmdlet. Reason:{0}", ex);
                flag = false;
            }
            return flag;
        }
        #endregion

        #region EXO & REST API cmdlets
        public async Task<Mailbox> GetEXOMailboxAsync(string identity, OutlookQuery query = null)
        {
            try
            {
                return await InvokeWithRetryAsync(delegate
                {
                    return client.GetService<IOutlookMailboxService>(GetAccessToken(), tenantId).GetAsync(identity, query);
                });
            }
            catch (Exception e)
            {
                logger.Error($"Failed to get the mailbox: {identity}, Error: {e}");
                return null;
            }
        }

        public async Task<PagedResult<RecipientPermissionInfo>> GetEXORecipientPermissionAsync(string identity)
        {
            return await InvokeWithRetryAsync(delegate
            {
                return client.GetService<IOutlookRecipientService>(GetAccessToken(), tenantId).GetRecipientPermissionsAsync(identity);
            });
        }

        public async Task<PagedResult<T>> GetDataNextPageAsync<T>(string paramQuery)
        {
            return await InvokeWithRetryAsync(delegate
            {
                return (client.GetService<IOutlookRecipientService>(GetAccessToken(), tenantId) as IOutlookPageQueryService<T>).GetDataNextPageAsync(paramQuery);
            });
        }
        #endregion
    }
}