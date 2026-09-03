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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Box.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using Box.V2;
using Box.V2.Auth;
using Box.V2.Config;
using Box.V2.Exceptions;
using Box.V2.JWTAuth;
using Box.V2.Models;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace RABox
{
    public class BoxClientContext : IDisposable
    {
        private static RALogger logger = new RALogger(typeof(BoxClientContext));
        private IRMBoxConnectionDao BoxConnectionDao => PlatformWindsorManager.GetService<IRMBoxConnectionDao>();

        private readonly List<string> UserFields = new List<string>() { BoxUser.FieldName, BoxUser.FieldRole, BoxUser.FieldEnterprise, BoxUser.FieldLogin, BoxUser.FieldStatus, BoxUser.FieldCreatedAt, BoxUser.FieldModifiedAt };
        private readonly List<string> ItemFields = BoxUtility.ItemFields;
        private readonly List<string> ItemBasicFields = BoxUtility.ItemBasicFields;
        private const int Limit = 1000;
        private readonly int MaxRetryCount = 10;
        private readonly int RetryInterval = 30000;

        private string currentUserId;
        private BoxUser currentUser;
        private BoxClient boxClient;
        private BoxClient adminClient;
        private OAuthSession OAuthSession;
        private readonly IBoxConfig boxConfig;
        private readonly BoxJWTAuth boxJwtAuth;

        private HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

        private List<BoxUser> UserCache = new List<BoxUser>();
        private Dictionary<string, bool> externalUserCache = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, BoxCollection<BoxGroupMembership>> groupInfoCache = new Dictionary<string, BoxCollection<BoxGroupMembership>>(StringComparer.OrdinalIgnoreCase);

        public bool IsAdminToken { get; set; }
        public string TokenUserId { get; set; }
        public BoxConnectionItem ConnectionInfo { get; private set; }

        public BoxClientContext(BoxConnectionItem boxConnectionContent)
        {
            ConnectionInfo = boxConnectionContent;

            boxConfig = InitializeBoxConfig();
            boxJwtAuth = boxConfig != null ? new BoxJWTAuth(boxConfig) : null;

            InitializeAuthentication().GetAwaiter().GetResult();
        }

        private async Task InitializeAuthentication()
        {
            try
            {
                if (ConnectionInfo.AuthenticationType == BoxAuthenticationType.UserAuth)
                {
                    await InitializeUserAuthAsync();
                }
                else
                {
                    await InitializeServerAuthAsync();
                }
                Validate();
            }
            catch (Exception ex)
            {
                logger.Warn($"Error occurred while initializing Box client. Ex: {ex}.");
                throw;
            }
        }

        private IBoxConfig InitializeBoxConfig()
        {
            if (ConnectionInfo.AuthenticationType == BoxAuthenticationType.ServerAuth)
            {
                var jwtConfigFileString = Encoding.UTF8.GetString(ConnectionInfo.JsonFileContent);
                return BoxConfig.CreateFromJsonString(jwtConfigFileString);
            }
            else
            {
                return new BoxConfigBuilder(ConnectionInfo.ClientId, ConnectionInfo.ClientSecret).Build();
            }
        }

        private async Task InitializeServerAuthAsync()
        {
            logger.Info("Begin to initialize Box client with server authentication.");
            var accessToken = await boxJwtAuth.AdminTokenAsync();
            adminClient = boxJwtAuth.AdminClient(accessToken);
            ConnectionInfo.EnterpriseId = boxConfig.EnterpriseId;
            logger.Info("Finished initializing Box client with server authentication.");
        }

        private async Task InitializeUserAuthAsync()
        {
            logger.Info($"Begin to initialize Box client with user authentication. Redirect URL: {ConnectionInfo.RedirectUrl}");

            var authCode = ConnectionInfo.Code;
            boxClient = new BoxClient(boxConfig);

            if (authCode == null)
            {
                await HandleExistingAuthAsync();
            }
            else
            {
                var oAuthSession = await GetOAuthSessionAsync(authCode);
                var isValidClient = await ValidateClientCredentialsAsync(oAuthSession.AccessToken, ConnectionInfo.EmailAddress);

                if (!isValidClient)
                {
                    throw new InvalidOperationException("Invalid ClientId, ClientSecret, or EmailAddress.");
                }

                adminClient = new BoxClient(boxConfig, oAuthSession);
                ConnectionInfo.EnterpriseId = ConnectionInfo.EnterpriseId;
                ConnectionInfo.AccessToken = oAuthSession.AccessToken;
                ConnectionInfo.RefreshToken = oAuthSession.RefreshToken;
            }
        }

        private async Task<bool> ValidateClientCredentialsAsync(string accessToken, string emailAddress)
        {
            var userInfo = await GetUserInfoAsync(accessToken);

            if (userInfo == null || userInfo.Login != emailAddress)
            {
                return false;
            }

            var enterpriseId = userInfo.Enterprise?.Id ?? throw new InvalidOperationException("Failed to retrieve enterprise ID.");

            return ConnectionInfo.EnterpriseId == enterpriseId;
        }

        private async Task<BoxUser> GetUserInfoAsync(string accessToken)
        {
            //using (var client = new HttpClient())
            //{
                using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.box.com/2.0/users/me?fields=login,enterprise");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                using var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException("Failed to retrieve user info.");
                }
                var userInfo = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<BoxUser>(userInfo);
            //}
        }

        private async Task<OAuthSession> GetOAuthSessionAsync(string authorizationCode)
        {
            try
            {
            var authSession = await boxClient.Auth.AuthenticateAsync(authorizationCode);

            return new OAuthSession(authSession.AccessToken, authSession.RefreshToken, authSession.ExpiresIn, "bearer");
        }
            catch (Exception ex)
            {
                if (ex is BoxAPIException be && be.StatusCode == HttpStatusCode.BadRequest && be.Message.Contains("invalid_grant") && be.Message.Contains("The authorization code has expired"))
                {
                    throw new Exception("AuthorizationCodeExpired");
                }

                throw;
            }
        }

        private OAuthSession RefreshAccessTokenAsync(RMBoxConnection boxConnection)
        {
            IBoxConfig newBoxConfig = boxConfig ?? new BoxConfigBuilder(ConnectionInfo.ClientId, ConnectionInfo.ClientSecret).Build();
            if (adminClient == null)
            {
                OAuthSession = new OAuthSession(boxConnection.AccessToken, boxConnection.RefreshToken, 3600, "bearer");
                adminClient = new BoxClient(newBoxConfig, OAuthSession);
            }

            var newAuthSession = Retry(() => adminClient.Auth.RefreshAccessTokenAsync(boxConnection.RefreshToken)).Result;
            if (newAuthSession == null)
            {
                throw new InvalidOperationException("Failed to refresh the OAuth session.");
            }
            return newAuthSession;
        }

        private async Task<bool> TokenIsExpiredAsync(string accessToken)
        {
            //using (var client = new HttpClient())
            //{

            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.box.com/2.0/users/me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                using var response = await client.SendAsync(request);
                return !response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking token: {ex.Message}");
                return true;
            }

            //}
        }

        public async Task HandleExistingAuthAsync()
        {
            var boxConnection = BoxConnectionDao.GetById(ConnectionInfo.Id);
            if (string.IsNullOrEmpty(boxConnection.AccessToken) || await TokenIsExpiredAsync(boxConnection.AccessToken))
            {
                if (string.IsNullOrEmpty(boxConnection.RefreshToken))
                {
                    throw new InvalidOperationException("Both AccessToken and RefreshToken are empty.");
                }

                var newAuthSession = RefreshAccessTokenAsync(boxConnection);
                if (newAuthSession == null)
                {
                    throw new InvalidOperationException("Failed to refresh the OAuth session.");
                }
                OAuthSession = new OAuthSession(newAuthSession.AccessToken, newAuthSession.RefreshToken, 3600, "bearer");

                boxConnection.AccessToken = newAuthSession.AccessToken;
                boxConnection.RefreshToken = newAuthSession.RefreshToken;
                BoxConnectionDao.Modify(boxConnection);

                adminClient = new BoxClient(boxConfig, OAuthSession);
            }
            else
            {
                OAuthSession = new OAuthSession(boxConnection.AccessToken, boxConnection.RefreshToken, 3600, "bearer");
                adminClient = new BoxClient(boxConfig, OAuthSession);
            }
        }

        private void Validate()
        {
            currentUser = Retry(() => adminClient.UsersManager.GetCurrentUserInformationAsync(UserFields).Result);
            if (string.IsNullOrEmpty(ConnectionInfo.EnterpriseId) && currentUser.Enterprise != null)
            {
                ConnectionInfo.EnterpriseId = currentUser.Enterprise.Id;
            }
            IsAdminToken = currentUser.Role.Equals("admin");
            TokenUserId = currentUser.Id;
        }

        public void AsUser(string id, bool isPrescan = false)
        {
            var isTokenUser = id == TokenUserId;
            if (currentUser != null && currentUser.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
            {
                currentUserId = id;
                boxClient = adminClient;
                return;
            }
            if ((isPrescan || isTokenUser) && ConnectionInfo.AuthenticationType == BoxAuthenticationType.UserAuth)
            {
                currentUserId = id;
            }
            if (!string.IsNullOrEmpty(TokenUserId) && !TokenUserId.Equals(currentUserId, StringComparison.OrdinalIgnoreCase))
            {
                if (ConnectionInfo.AuthenticationType == BoxAuthenticationType.ServerAuth)
                {
                    string accessToken = Retry(() => boxJwtAuth.UserTokenAsync(id).Result);
                    boxClient = boxJwtAuth.UserClient(accessToken, id);
                }
                else
                {
                    boxClient = new BoxClient(boxConfig, OAuthSession, id);
                }
            }
            else
            {
                boxClient = adminClient;
            }

            currentUserId = id;
            currentUser = null;
        }

        public BoxUser GetUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (UserCache.Count == 0)
            {
                GetAllUsers();
            }

            BoxUser user = UserCache.FirstOrDefault(u => u.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (user != null)
            {
                return user;
            }

            user = Retry(() => boxClient.UsersManager.GetUserInformationAsync(id, UserFields).Result);
            return user;
        }

        public BoxUser GetUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            if (UserCache.Count == 0)
            {
                GetAllUsers();
            }

            BoxUser user = UserCache.FirstOrDefault(u => u.Login.Equals(email, StringComparison.InvariantCultureIgnoreCase));
            return user;
        }

        public BoxUser GetCurrentUser()
        {
            if (currentUser != null)
            {
                return currentUser;
            }

            BoxUser user = Retry(() => boxClient.UsersManager.GetCurrentUserInformationAsync(UserFields).Result);
            if (user != null)
            {
                currentUser = user;
            }
            return user;
        }

        public bool IsExternalUser(string id)
        {
            if (externalUserCache.ContainsKey(id))
            {
                return externalUserCache[id];
            }
            BoxUser user = GetUser(id);
            if (user == null)
            {
                logger.Info($"Get box user is null, user id :{id}");
                externalUserCache[id] = true;
                return true;
            }
            if (user.Enterprise == null)
            {
                logger.Info($"User is external, without enterprise, user id:{id}");
                externalUserCache[id] = true;
                return true;
            }
            if (!user.Enterprise.Id.Equals(ConnectionInfo.EnterpriseId, StringComparison.OrdinalIgnoreCase))
            {
                logger.Info($"User id:{id}, is external user , current enterprise id :{ConnectionInfo.EnterpriseId}, user enterprise id :{user.Enterprise.Id}");
                externalUserCache[id] = true;
                return true;
            }
            externalUserCache[id] = false;
            return false;
        }

        public List<BoxUser> GetAllUsers()
        {
            if (UserCache.Count > 0)
            {
                return UserCache;
            }

            logger.Info("Start to get all users.");
            BoxCollection<BoxUser> bc = Retry(() => adminClient.UsersManager.GetEnterpriseUsersAsync(null, 0, Limit, UserFields).Result);
            int count = bc.Entries.Count;
            List<BoxUser> users = bc.Entries;
            while (bc.TotalCount > count)
            {
                bc = Retry(() => adminClient.UsersManager.GetEnterpriseUsersAsync(null, (uint)count, Limit, UserFields).Result);
                users.AddRange(bc.Entries);
                count += bc.Entries.Count;
            }
            UserCache = users;
            logger.Info($"Finish to get all users, count:{users.Count}.");
            return users;
        }

        public BoxCollection<BoxGroupMembership> GetGroupMembers(string groupId)
        {
            if (groupInfoCache.ContainsKey(groupId))
            {
                return groupInfoCache[groupId];
            }
            else
            {
                return Retry(() => boxClient.GroupsManager.GetAllGroupMembershipsForGroupAsync(groupId, null, null, null/*UserFields*/, false).Result);
            }
        }

        public BoxCollectionMarkerBasedV2<BoxCollaboration> GetFileCollaborations(string id)
        {
            return Retry(() => boxClient.FilesManager.GetCollaborationsCollectionAsync(id).Result);
        }

        public BoxCollection<BoxCollaboration> GetFolderCollaborations(string id)
        {
            return Retry(() => boxClient.FoldersManager.GetCollaborationsAsync(id).Result);
        }

        public bool FileExist(string id)
        {
            BoxFile file = Retry(() => boxClient.FilesManager.GetInformationAsync(id).Result);
            return file != null;
        }

        public bool FolderExist(string id)
        {
            BoxFolder folder = Retry(() => boxClient.FoldersManager.GetInformationAsync(id).Result);
            return folder != null;
        }

        public bool WebLinkExist(string id)
        {
            BoxWebLink webLink = Retry(() => boxClient.WebLinksManager.GetWebLinkAsync(id).Result);
            return webLink != null;
        }

        public BoxFile GetFile(string id)
        {
            BoxFile file = Retry(() => boxClient.FilesManager.GetInformationAsync(id, ItemFields).Result);
            return file;
        }

        public BoxFile GetTrashedFile(string id)
        {
            BoxFile file = Retry(() => boxClient.FilesManager.GetTrashedAsync(id, ItemFields).Result);
            return file;
        }

        public BoxFolder GetFolder(string id)
        {
            BoxFolder folder = Retry(() => boxClient.FoldersManager.GetInformationAsync(id, ItemFields).Result);
            return folder;
        }

        public BoxCollection<BoxItem> GetFolderItems(string id, int limit, int offset = 0)
        {
            BoxCollection<BoxItem> items = Retry(() => boxClient.FoldersManager.GetFolderItemsAsync(id, limit, offset).Result);
            return items;
        }

        public BoxWebLink GetWebLink(string id)
        {
            BoxWebLink webLink = Retry(() => boxClient.WebLinksManager.GetWebLinkAsync(id).Result);
            return webLink;
        }

        public Stream GetFileStream(string id, string versionId)
        {
            Stream stream = Retry(() => boxClient.FilesManager.DownloadAsync(id, versionId).Result);
            if (stream == null)
            {
                throw new Exception($"BoxNotFound:{id}");
            }
            return stream;
        }

        public Stream GetWebLinkStream(string id)
        {
            BoxWebLink webLink = Retry(() => boxClient.WebLinksManager.GetWebLinkAsync(id).Result);
            if (webLink == null)
            {
                throw new Exception($"BoxNotFound:{id}");
            }
            var resultUrl = "[InternetShortcut]\nURL=" + webLink.Url;
            var resultBytes = Encoding.UTF8.GetBytes(resultUrl);
            var deCompressedStream = new MemoryStream(resultBytes);
            return deCompressedStream;
        }

        public List<BoxItem> GetSubFiles(string id)
        {
            List<BoxItem> files = GetSubItems(id);
            return files.Where(b => b.Type.Equals("file", StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<BoxItem> GetSubFolders(string id)
        {
            List<BoxItem> folderItems = GetSubItems(id);

            return folderItems.Where(b => b.Type.Equals("folder", StringComparison.OrdinalIgnoreCase)).ToList();
            }

        /// <summary>
        ///  get all items in a folder. 
        ///  Only file and folder type for now
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryBasicProperty"></param>
        /// <returns></returns>
        public List<BoxItem> GetSubItems(string id, bool queryBasicProperty = false)
        {
            var itemQueryCondition = queryBasicProperty ? ItemBasicFields : ItemFields;
            BoxCollection<BoxItem> bc = Retry(() => boxClient.FoldersManager.GetFolderItemsAsync(id, Limit, 0, itemQueryCondition, sort: "name", direction: BoxSortDirection.ASC).Result);
            if (bc == null)
            {
                return null;
            }
            int count = bc.Entries.Count;
            List<BoxItem> items = bc.Entries;
            int retry = 1;
            while (bc.TotalCount > count)
            {
                bc = Retry(() => boxClient.FoldersManager.GetFolderItemsAsync(id, Limit, count, itemQueryCondition, sort: "name", direction: BoxSortDirection.ASC).Result);
                items.AddRange(bc.Entries);
                count = count + bc.Entries.Count;
                if (bc.Entries.Count == 0)
                {
                    if (retry > 3)
                    {
                        logger.Warn($"Retry for 3 times, there may be dirty data under this folder. TotalCount:{bc.TotalCount}, currentGetCount:{count}.");
                        break;
                    }
                    else
                    {
                        logger.Warn($"Can't get more items. TotalCount:{bc.TotalCount}, currentGetCount:{count}, retryTimes:{retry}.");
                        retry += 1;
                    }
                }
            }
            return items.Where(i => i.OwnedBy.Id.Equals(currentUserId, StringComparison.OrdinalIgnoreCase) && (i.Type == BoxType.file.ToString() || i.Type == BoxType.folder.ToString())).ToList();
        }

        public List<BoxItem> GetTrashedItems(bool queryBasicProperty = false)
        {
            var itemQueryCondition = queryBasicProperty ? ItemBasicFields : ItemFields;
            BoxCollection<BoxItem> bc = Retry(() => boxClient.FoldersManager.GetTrashItemsAsync(limit: Limit, offset: 0, fields: ItemFields).Result);
            if (bc == null)
            {
                return null;
            }
            int count = bc.Entries.Count;
            List<BoxItem> items = bc.Entries;
            int retry = 1;
            while (bc.TotalCount > count)
            {
                bc = Retry(() => boxClient.FoldersManager.GetTrashItemsAsync(limit: Limit, offset: count, fields: ItemFields).Result);
                items.AddRange(bc.Entries);
                count = count + bc.Entries.Count;
                if (bc.Entries.Count == 0)
                {
                    if (retry > 3)
                    {
                        logger.Warn($"Retry for 3 times, there may be dirty data under trash. TotalCount:{bc.TotalCount}, currentGetCount:{count}.");
                        break;
                    }
                    else
                    {
                        logger.Warn($"Can't get more items. TotalCount:{bc.TotalCount}, currentGetCount:{count}, retryTimes:{retry}.");
                        retry += 1;
                    }
                }
            }
            return items.Where(i => i.OwnedBy.Id.Equals(currentUserId, StringComparison.OrdinalIgnoreCase) && (i.Type == BoxType.file.ToString() || i.Type == BoxType.folder.ToString())).ToList();
        }

        public List<BoxEnterpriseEvent> GetModifiedSubItems(string scanFolderId, ref string nextStreamPosition)
        {
            var changedItems = new List<BoxEnterpriseEvent>();
            int retry = 1;
            string lastStreamPosition = nextStreamPosition;
            do
            {
                var eventCollection = Retry(() => boxClient.EventsManager.UserEventsAsync(streamType: UserEventsStreamType.changes, streamPosition: lastStreamPosition).Result);

                if (eventCollection == null || eventCollection.Entries.Count == 0)
                {
                    if (retry > 3)
                    {
                        logger.Warn($"Retry for 3 times, but no events retrieved. StreamPosition:{lastStreamPosition}.");
                        break;
                    }
                    else
                    {
                        logger.Warn($"No events retrieved. StreamPosition:{lastStreamPosition}, retryTimes:{retry}.");
                        retry += 1;
                    }
                }
                else
                {
                    var modifyTypeEvents = eventCollection.Entries
                        .Where(e => BoxUtility.ModifiedEventTypes.Contains(e.EventType)).ToList();

                    foreach (var boxEvent in modifyTypeEvents)
                    {
                        int index = changedItems.FindIndex(item => item.Source.Id == boxEvent.Source.Id);

                        if (index != -1)
                        {
                            if (boxEvent.EventType == BoxUtility.TrashedEventType || 
                                (boxEvent.Source is BoxItem boxItem && (boxItem.PathCollection.Entries.Any(s => s.Id.Equals(scanFolderId)) || (scanFolderId != BoxUtility.BoxRootFolderId && boxItem.Id == scanFolderId))))
                            {
                            changedItems[index] = boxEvent;
                        }
                        }
                        else
                        {
                            if (boxEvent.Source is BoxItem boxItem && boxItem.OwnedBy.Id.Equals(currentUserId, StringComparison.OrdinalIgnoreCase) &&
                                (boxItem.Type == BoxType.file.ToString() || boxItem.Type == BoxType.folder.ToString()))
                            {
                            if (boxEvent.EventType == BoxUtility.TrashedEventType)
                            {
                                changedItems.Add(boxEvent);
                            }
                                else if (boxItem.PathCollection.Entries.Any(s => s.Id.Equals(scanFolderId)) ||
                                        (scanFolderId != BoxUtility.BoxRootFolderId && boxItem.Id == scanFolderId))
                            {
                                changedItems.Add(boxEvent);
                            }
                        }
                    }
                    }
                    retry = 1;
                }
                lastStreamPosition = eventCollection.NextStreamPosition;

            } while (changedItems != null && changedItems.Count > 0);
            nextStreamPosition = lastStreamPosition;

            return changedItems;
        }

        public string InitStreamPosition()
        {
            return Retry(() => boxClient.EventsManager.UserEventsAsync(streamType: UserEventsStreamType.changes, streamPosition: "now").Result).NextStreamPosition;
        }

        public bool DeleteFile(string id)
        {
            return Retry(() => boxClient.FilesManager.DeleteAsync(id).Result.ToString()).ToBoolean(false);
        }

        public bool PurgeTrashedFile(string id)
        {
            return Retry(() => boxClient.FilesManager.PurgeTrashedAsync(id).Result.ToString()).ToBoolean(false);
        }

        public BoxFile RestoreFile(string id)
        {
            var requestParams = new BoxFileRequest()
            {
                Id = id,
                Parent = new BoxRequestEntity()
                {
                    // File will be placed in this folder if original location no longer exists
                    Id = BoxUtility.BoxRootFolderId
        }
            };
            return Retry(() => boxClient.FilesManager.RestoreTrashedAsync(requestParams, ItemFields).Result);

        }

        public void Dispose()
        {
            UserCache = null;
            UserCache = new List<BoxUser>();
        }

        private T Retry<T>(Func<T> func) where T : class
        {
            int counter = 0;
            while (true)
            {
                try
                {
                    counter += 1;
                    return func();
                }
                catch (Exception ex)
                {
                    logger.Error($"Execute Box api error, function name:{func?.Method.Name}, retry count:{counter} error message:{ex}");
                    if (counter > MaxRetryCount)
                    {
                        logger.Error($"Retry failed many times. Retry count:{counter}, ex:{ex}");
                        throw;
                    }

                    if (ex is BoxSessionInvalidatedException || ex.InnerException is BoxSessionInvalidatedException)
                    {
                        logger.Info("Invalid box login info.");
                        //TODO: UpdateAccessToken();
                        HandleExistingAuthAsync().GetAwaiter().GetResult();
                        continue;
                    }

                    if (ex is BoxAPIException || ex.InnerException is BoxAPIException)
                    {
                        BoxAPIException be = ex is BoxAPIException ? (BoxAPIException)ex : ex.InnerException as BoxAPIException;
                        if (be != null && be.Message != null)
                        {
                            logger.Error($"Box Exception RequestID:{be.Error.RequestId}.");
                        }
                        switch (be?.StatusCode)
                        {
                            case HttpStatusCode.BadRequest:
                                logger.Error($"Bad request. Ex:{be.Message}");
                                if (be.Message.Contains("password_reset_required"))
                                {
                                    throw new Exception("UserNeedToResetPassword");
                                }
                                break;
                            case HttpStatusCode.Unauthorized:
                                logger.Error($"Unauthorized. Ex:{be.Message}");
                                
                                HandleExistingAuthAsync().GetAwaiter().GetResult();
                                continue;
                            case HttpStatusCode.NotFound:
                                logger.Debug(be.Message);
                                if (be.Message.Contains("not_trashed"))
                                {
                                    throw new Exception("BoxItemIsNotTrashed");
                                }
                                if (be.Message.Contains("trashed"))
                                {
                                    throw new Exception("BoxItemIsTrashed");
                                }
                                throw new Exception("BoxItemNotFound");
                            case HttpStatusCode.Forbidden:
                                if (be.Message.Contains("user_email_confirmation_required"))
                                {
                                    throw new Exception("UserNeedToCompleteEmailConfirmation");
                                    //User needs to complete email confirmation
                                }
                                if (be.Message.Contains("access_denied_item_locked"))
                                {
                                    throw new Exception("BoxItemLocked");
                                }
                                logger.Error($"Don't have enough permission. Ex:{be.Message}");
                                if (be.Error != null)
                                {
                                    logger.Error($"Don't have enough permission. RequestID:{be.Error.RequestId}. ErrorCode:{be.Error.Code}. Message:{be.Error.Message}. Status:{be.Error.Status}. ErrorName:{be.Error.Name}.");
                                }
                                else
                                {
                                    logger.Error("The current error object is null.");
                                }
                                break;
                            case HttpStatusCode.RequestTimeout:
                            case HttpStatusCode.InternalServerError:
                            case HttpStatusCode.BadGateway:
                            case HttpStatusCode.ServiceUnavailable:
                                logger.Info($"Box error status code:{be.StatusCode}. Retry after {RetryInterval} ms. Retry count: {counter}.");
                                Thread.Sleep(RetryInterval);
                                continue;
                            default:
                                if (be != null && (int)be.StatusCode == 429)
                                {
                                    logger.Info($"Box 429 error. Retry after {RetryInterval} ms. Retry count: {counter}.");
                                    Thread.Sleep(RetryInterval);
                                    continue;
                                }
                                logger.Error($"Box exception error status code:{be?.StatusCode}, ex:{be?.Message}.");
                                break;
                        }
                    }

                    if (ex is WebException || ex.InnerException is WebException)
                    {
                        if (IsHTTP429Error(ex))
                        {
                            logger.Info($"Because of 429 error. Retry after {RetryInterval} ms. Retry count: {counter}");
                            Thread.Sleep(RetryInterval);
                            continue;
                        }
                        var webException = ex as WebException;
                        if (webException.Status == WebExceptionStatus.ProtocolError
                            && webException.Response != null
                            && webException.Response is HttpWebResponse resp)
                        {
                            if (resp.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                logger.Debug($"Unauthorized. Retry count: {counter}.");
                                //UpdateAccessToken();
                                HandleExistingAuthAsync().GetAwaiter().GetResult();
                                continue;
                            }
                            else if (resp.StatusCode == HttpStatusCode.InternalServerError ||
                                resp.StatusCode == HttpStatusCode.RequestTimeout ||
                                resp.StatusCode == HttpStatusCode.ServiceUnavailable ||
                                resp.StatusCode == HttpStatusCode.BadGateway)
                            {
                                logger.Info($"Error status code:{resp.StatusCode}. Retry after {RetryInterval} ms. Retry count: {counter}");
                                Thread.Sleep(RetryInterval);
                                continue;
                            }
                            else
                            {
                                string body = string.Empty;
                                using (Stream respStream = resp.GetResponseStream())
                                {
                                    using (StreamReader sr = new StreamReader(respStream))
                                    {
                                        body = sr.ReadToEnd();
                                    }
                                }
                                logger.Error($"Request failed, ex:{ex}, response body:{body}.");
                                throw;
                            }
                        }
                        else if (webException.Status == WebExceptionStatus.ConnectionClosed ||
                                 webException.Status == WebExceptionStatus.ConnectFailure ||
                                 webException.Status == WebExceptionStatus.NameResolutionFailure ||
                                 webException.Status == WebExceptionStatus.Timeout)
                        {
                            logger.Info($"Error status:{webException.Status}. Retry after {RetryInterval} ms. Retry count: {counter}");
                            Thread.Sleep(RetryInterval);
                            continue;
                        }
                    }

                    if (ex is AggregateException)
                    {
                        AggregateException aex = ex as AggregateException;
                        if (aex.InnerExceptions != null && aex.InnerExceptions.Any(e => e is TimeoutException || e is TaskCanceledException))
                        {
                            logger.Warn("Request timeout or task canceled. Reset HttpClient");
                            //boxClient.ResetHttpClient();
                            continue;
                        }
                    }

                    logger.Error($"Execute request failed: {ex}");
                    throw;
                }
            }
        }

        private bool IsHTTP429Error(Exception e)
        {
            if (e is WebException)
            {
                var webException = e as WebException;
                HttpWebResponse response = webException.Response as HttpWebResponse;
                if (response != null && ((int)response.StatusCode == 429 || (int)response.StatusCode == 503))
                {
                    return true;
                }
                if (webException.Message != null && webException.Message.Contains("The remote server returned an error: (429)"))
                {
                    return true;
                }
            }
            if (e.InnerException != null)
            {
                return IsHTTP429Error(e.InnerException);
            }
            return false;
        }
    }
}
