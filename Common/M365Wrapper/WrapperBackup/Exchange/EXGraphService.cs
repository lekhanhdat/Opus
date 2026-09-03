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
//using AvePoint.RA.Common;
//using AvePoint.RA.CommonUtil;
//using Cloud.Sdk.Data.AosModern;
//using ExchangeUtility.Graph;
//using Google.Apis.PeopleService.v1.Data;
//using Google.Cloud.AIPlatform.V1;
//using Microsoft.Exchange.WebServices.Data;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;
//using Util.Graph;
//using Util.Graph.Beta;
//using Util.MSAzure;

//namespace M365.Wrapper.Backup.Exchange
//{
//    public class EXGraphService
//    {

//        private  RALogger logger = RALogger.GetInstance(typeof(ExchangeMailbox));
//        private readonly Object lockObj = new Object();
//        private readonly GraphClient client;

//        private TokenResult tokenResult;
//        private String token => GetToken();

//        private String userId;

//        private String primaryMailboxId;

//        public EXGraphService
//            (
//            GraphClient client,
//            String userId,
//            String primaryMailboxId)
//        {
//            this.client = client;
//            this.userId = userId;
//            this.primaryMailboxId = primaryMailboxId;
//            //archiveMailbox.InPlaceArchiveMailboxId;   
//            //this.credential = exService.Credential;
//            this.client = new GraphClient("UserAgent", AzureEnvironment.Worldwide);
//        }

//        private String GetToken()
//        {
//            lock (lockObj)
//            {
//                if (tokenResult == null || tokenResult.ExpiresOn < DateTimeOffset.UtcNow.AddMinutes(5))
//                {
//                    ////var response = this.credential.GetToken(TokenResourceType.Graph).Result;
//                    ////if (response.Result != OperationResult.Success)
//                    ////    throw new OperationResponseException(response);
//                    ////tokenResult = response.DataObject;
//                }

//                return tokenResult.AccessToken;
//            }
//        }

//        #region User
//        public virtual async Task<User> GetUser(String identity, GraphQuery query)
//        {
//            try
//            {
              
//                    var service = client.GetService<IGraphUserService>(token);
//                    return await service.GetAsync(identity, query);
                
//            }
//            catch (Exception e)
//            {
//                logger.Error($"Failed to get the o365 user: {identity}, Error: {e}");
//                if (token.IsNullOrEmpty())
//                    throw;
//                return null;
//            }
//        }

//        String EncodeGraph(String source)
//        {
//            if (source.IsNullOrEmpty())
//                return source;

//            return WebUtility.UrlEncode(source).Replace(@"%27", "''");
//        }

//        public virtual async Task<User> GetUser(String emailAddress, Boolean isThrowException = false)
//        {
//            try
//            {
//                var encodeMail = EncodeGraph(emailAddress);
//                var query = new GraphQuery()
//                {
//                    Top = 1,
//                    Select = "id,displayName,userPrincipalName,mail",
//                    Filter = $"proxyAddresses/any(c:c eq 'SMTP:{encodeMail}')  OR mail eq '{encodeMail}' OR userPrincipalName eq '{encodeMail}'"
//                };
//                return await this.SeachUser(query);
//            }
//            catch (Exception e)
//            {
//                logger.Error($"Failed to get user by email: {emailAddress}, Error: {e}");
//                if (isThrowException)
//                    throw;

//                return null;
//            }
//        }

//        public virtual async Task<User> GetInactiveUser(String emailAddress)
//        {
//            try
//            {
//                var encodeMail = EncodeGraph(emailAddress);
//                var query = new GraphQuery()
//                {
//                    Top = 1,
//                    Select = "id,displayName,userPrincipalName,mail",
//                    Filter = $"proxyAddresses/any(c:c eq 'SMTP:{encodeMail}')  OR mail eq '{encodeMail}' OR userPrincipalName eq '{encodeMail}'"
//                };

//                var service = client.GetService<IGraphListDeletedItemsService<User>>(token);
//                var result = await service.GetDeletedUsersAsync(query);
//                return result.Value.FirstOrDefault();
//            }
//            catch (Exception e)
//            {
//                logger.Error($"Failed to get inactive user by email: {emailAddress}, Error: {e}");
//                throw;
//            }
//        }

//        public virtual async Task<User> GetInactiveUserById(String id)
//        {
//            try
//            {
//                var query = new GraphQuery()
//                {
//                    Top = 1,
//                    Select = "proxyAddresses,mail,userPrincipalName",
//                };

//                var service = client.GetService<IGraphListDeletedItemsService<User>>(token);
//                return await service.GetAsync(id, query);
//            }
//            catch (Exception e)
//            {
//                logger.Error($"Failed to get inactive user by email: {id}, Error: {e}");
//                if (token.IsNullOrEmpty())
//                    throw;
//                return null;
//            }
//        }

//        public virtual async IAsyncEnumerable<User> GetUsersByUPNPrefix(String upnPrefix)
//        {
//            var users = AsyncEnumerable.Empty<User>();
//            try
//            {
//                var query = new GraphQuery()
//                {
//                    Top = 1,
//                    Select = "id,proxyAddresses,userPrincipalName",
//                    Filter = $"startswith(userPrincipalName,'{upnPrefix}') or proxyAddresses/any(c:startswith(c, 'smtp:{upnPrefix}'))"
//                };
//                users = this.GetUsers(query);
//            }
//            catch (Exception e)
//            {
//                logger.Error($"Failed get users by upn prefix: {upnPrefix}, error: {e}");
//                throw;
//            }
//            await foreach (var user in users)
//                yield return user;
//        }

//        public virtual async Task<User> SeachUser(GraphQuery query)
//        {
//            try
//            {
//                var service = client.GetService<IGraphUserService>(token);
//                var result = await service.GetDataAsync(query);
//                return result.Value.FirstOrDefault();
//            }
//            catch (Exception e)
//            {
//                logger.Error($"Failed to search the o365 user: {query.Filter}, Error: {e}");
//                return null;
//            }
//        }

//        public virtual async IAsyncEnumerable<User> GetUsers(GraphQuery query = null)
//        {
//            var users = new List<User>();
//            try
//            {
//                var service = client.GetService<IGraphUserService>(token);
//                var results = await service.GetDataAsync(query);
//                results.Expand(a => users.AddRange(a), service);
//            }
//            catch (Exception e)
//            {
//                logger.Error($"Failed to get the o365 users, Error: {e}");
//            }
//            foreach (var user in users)
//                yield return user;
//        }

//        #endregion

//        public virtual async Task<ExchangeSettings> GetExchangeSettings(String mail)
//        {
//            try
//            {
//                var service = client.GetService<IGraphExchangeSettingsService>(token);
//                return await service.GetMailboxExchangeSettingAsync(mail);
//            }
//            catch (Exception e)
//            {
//                logger.Error($"Failed to get the exchange settings, Mailbox: {mail}, Error: {e}");
//                throw;
//            }
//        }

//        public virtual async Task<Message> GetMessage(String messageId, GraphQuery query)
//        {
//            var service = client.GetService<IGraphMessageService>(token);
//            return await service.GetMessageAsync(userId, messageId, query);
//        }

//        public virtual async Task<String> GetMessageMIMEContent(String messageId)
//        {
//            var service = client.GetService<IGraphMessageService>(token);
//            return await service.GetMessageMIMEContentAsync(userId, messageId);
//        }

//        /// <summary>
//        ///  Get the folder collection directly under the root folder of the user's mailbox.   
//        /// </summary>
//        /// <param name="query"></param>
//        /// <returns></returns>
//        public virtual async IAsyncEnumerable<MailboxFolder> GetMailboxFolders(GraphQuery query = null, Action<Exception> onErrorOccurred = null)
//        {
//            var folders = new List<MailboxFolder>();
//            try
//            {
//                var service = client.GetService<IGraphMailboxFolderService>(token);
//                var result = await service.GetFoldersAsync(primaryMailboxId, query);
//                result.Expand(a => folders.AddRange(a), service);
//            }
//            catch (Exception e)
//            {
//                logger.Error($"Failed to get the mailbox folders, Mailbox: {primaryMailboxId}, Error: {e}");
//                onErrorOccurred?.Invoke(e);
//            }
//            foreach (var folder in folders)
//                yield return folder;
//        }
//        public virtual async Task<MailboxFolder> GetMailboxFolder(String id, GraphQuery query = null)
//        {
//            try
//            {

//                var service = client.GetService<IGraphMailboxFolderService>(token);
//                return await service.GetFolderAsync(primaryMailboxId, id, query);

//            }
//            catch (Exception e)
//            {
//                logger.Error($"Failed to get the mailbox folder, Mailbox: {primaryMailboxId}, FolderId: {id}, Error: {e}");
//                throw;
//            }
//        }

//        public virtual async IAsyncEnumerable<MailboxFolder> GetMailboxFolderDelta(GraphQuery query = null)
//        {
//            var folders = new List<MailboxFolder>();
//            try
//            {

//                var service = client.GetService<IGraphMailboxFolderService>(token);
//                var result = await service.GetFolderDeltaAsync(primaryMailboxId, query);
//                folders.AddRange(result.Value);

//            }
//            catch (Exception e)
//            {
//                logger.Error($"Failed to get the mailbox folder delta, Mailbox: {primaryMailboxId}, Error: {e}");
//            }
//            foreach (var folder in folders)
//                yield return folder;
//        }

//        public virtual async IAsyncEnumerable<MailboxFolder> GetChildFolders(
//           String id,
//           GraphQuery query = null,
//           Action<Exception> onErrorOccurred = null)
//        {
//            var folders = new List<MailboxFolder>();
//            try
//            {
//                var service = client.GetService<IGraphMailboxFolderService>(token);
//                var result = await service.GetChildFoldersAsync(primaryMailboxId, id, query);
//                result.Expand(a => folders.AddRange(a), service);
//            }
//            catch (Exception e)
//            {
//                logger.Error($"Failed to get the child folders. Mailbox: {primaryMailboxId}, FolderId: {id}, Error: {e}");
//                onErrorOccurred?.Invoke(e);
//            }
//            foreach (var folder in folders)
//                yield return folder;
//        }

//        public virtual async Task<List<ExportItemResponse>> ExportItems(List<String> itemIds)
//        {
//            var items = new List<ExportItemResponse>();

//            var service = client.GetService<IGraphMailboxService>(token);
//            var request = new ExportItemsRequest()
//            {
//                ItemIds = itemIds
//            };
//            var result = await service.ExportItemsAsync(primaryMailboxId, request);
//            items.AddRange(result.Value);

//            return items;
//        }

//    }
//}
