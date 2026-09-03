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
using AvePoint.RA.Contract.Aos;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Services;
using RAGoogle.Extension;
using System.Collections.Generic;
using System.Reflection;
using System.Web;

namespace RAGoogle.API
{
    internal class DirectoryApi : IDisposable
    {
        private DirectoryService _service;
        private readonly RMAosGoogleAppProfile _appInfo;
        private readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        internal DirectoryApi(RMAosGoogleAppProfile app, BaseClientService.Initializer initializer)
        {
            _appInfo = app;
            _service = new DirectoryService(initializer);
        }

        internal async Task<List<Group>> ListGroupsAsync()
        {
            List<Group> groups = [];
            var request = _service.Groups.List();
            request.Domain = _appInfo.DomainName;
            request.Fields = "groups(name,id,description,email)";
            if (!string.IsNullOrEmpty(_appInfo.UserName) && _appInfo.UserName.Length < 40)
            {
                request.QuotaUser = _appInfo.UserName;
            }
            do
            {
                var response = await request.ExecuteExAsync();
                if (response == null)
                {
                    //exception
                }

                if (response != null && response.GroupsValue != null)
                {
                    groups.AddRange(response.GroupsValue);
                }
                request.PageToken = response?.NextPageToken;

            } while (!string.IsNullOrEmpty(request.PageToken));

            return groups;
        }
        public async Task<Group> GetGroupByIdAsync(string groupKey, string fields = null)
        {
            Group group = null;
            var request = _service.Groups.Get(groupKey);
            request.Fields = fields == null ? "name,id,description,email" : fields;
            if (!string.IsNullOrEmpty(_appInfo.UserName) && _appInfo.UserName.Length < 40)
            {
                request.QuotaUser = _appInfo.UserName;
            }
            group = await request.ExecuteExAsync();
            return group;
        }

        internal async Task<List<Member>> ListGroupMembersAsync(string groupKey)
        {
            List<Member> members = [];
            var request = _service.Members.List(groupKey);
            request.Fields = "members(email,id,type,role,status)";
            if (!string.IsNullOrEmpty(_appInfo.UserName) && _appInfo.UserName.Length < 40)
            {
                request.QuotaUser = _appInfo.UserName;
            }
            do
            {
                var response = await request.ExecuteExAsync();
                if (response == null) { }
                if (response != null && response.MembersValue != null)
                {
                    foreach (var member in response.MembersValue)
                    {
                        if (member.Type.Equals("GROUP"))
                        {
                            members.AddRange(await ListGroupMembersAsync(member.Id));
                        }
                        else
                        {
                            members.Add(member);
                        }
                    }
                }
                request.PageToken = response?.NextPageToken;
            } while (!string.IsNullOrEmpty(request.PageToken));
            return members;
        }

        internal async Task<List<Member>> GetGroupFirstUserAsync(string groupKey)
        {
            List<Member> members = [];
            var request = _service.Members.List(groupKey);
            request.IncludeDerivedMembership = true;
            request.MaxResults = 1;
            request.Fields = "members(email,id,type,role,status)";
            if (!string.IsNullOrEmpty(_appInfo.UserName) && _appInfo.UserName.Length < 40)
            {
                request.QuotaUser = _appInfo.UserName;
            }
            var response = await request.ExecuteExAsync();
            if (response != null && response.MembersValue != null)
            {
                foreach (var member in response.MembersValue)
                {
                    if (member.Type.Equals("GROUP"))
                    {
                        members.AddRange(await GetGroupFirstUserAsync(member.Id));
                    }
                    else
                    {
                        members.AddRange(response.MembersValue);
                    }
                }
            }
            return members;
        }
        internal async Task<List<User>> ListUsersAsync()
        {
            List<User> users = [];
            var request = _service.Users.List();
            request.Domain = _appInfo.DomainName;
            request.Fields = "nextPageToken,users(name,primaryEmail,id,archived,suspended)";
            if (!string.IsNullOrEmpty(_appInfo.UserName) && _appInfo.UserName.Length < 40)
            {
                request.QuotaUser = _appInfo.UserName;
            }
            do
            {
                var response = await request.ExecuteExAsync();
                if (response == null)
                {
                    //exception
                }

                if (response != null && response.UsersValue != null)
                {
                    users.AddRange(response.UsersValue);
                }
                request.PageToken = response?.NextPageToken;

            } while (!string.IsNullOrEmpty(request.PageToken));

            return users;
        }
        internal async Task<Users> SearchUsersAsync(string searchKey, int maxResults)
        {
            searchKey = HttpUtility.UrlEncode(searchKey);
            var request = _service.Users.List();
            request.Query = searchKey;
            request.MaxResults = maxResults;
            request.Customer = "my_customer";
            Users users = await request.ExecuteAsync();
            return users;
        }

        internal async Task<Groups> SearchGroupsAsync(string searchKey, string searchCondition, int maxResults)
        {
            searchKey = HttpUtility.UrlEncode(searchKey);
            var request = _service.Groups.List();
            request.Query = $"{searchCondition}:{searchKey}*";
            request.MaxResults = maxResults;
            request.Customer = "my_customer";
            Groups groups = await request.ExecuteAsync();
            return groups;
        }

        internal async Task<Groups> SearchEqualGroupsAsync(string searchKey, string searchCondition, int maxResults)
        {
            var request = _service.Groups.List();
            request.Query = $"{searchCondition}={searchKey}";
            request.MaxResults = maxResults;
            request.Customer = "my_customer";
            Groups groups = await request.ExecuteAsync();
            return groups;
        }

        internal async Task<(List<Member>, string)> ListGroupMembersPageAsync(string groupKey, string nextPageToken, int pageSize)
        {
            List<Member> members = [];
            var request = _service.Members.List(groupKey);
            request.PageToken = nextPageToken;
            request.MaxResults = pageSize;
            if (!string.IsNullOrEmpty(_appInfo.UserName) && _appInfo.UserName.Length < 40)
            {
                request.QuotaUser = _appInfo.UserName;
            }
            var response = await request.ExecuteExAsync();
            if (response != null && response.MembersValue != null)
            {
                members.AddRange(response.MembersValue);
            }
            return (members, response?.NextPageToken);
        }
        internal async Task<Dictionary<string, int>> GetGroupMermbersCountAsync(List<string> userEmails)
        {
            Dictionary<string, int> results = [];
            await userEmails.ForEachAsync(async userEmail =>
            {
                try
                {
                    var request = _service.Members.List(userEmail);
                    var members = await request.ExecuteAsync();
                    results.Add(userEmail, members.MembersValue.Count);
                }
                catch (Exception ex)
                {
                    logger.Error($"[DirectoryApi.GetGroupMermbersCountAsync] An eror occur while getting group member count: {ex}");
                }
            });
            return results;
        }
        public async Task<List<Group>> GetDirectGroupsAsync(string userEmail)
        {
            var request = _service.Groups.List();
            request.UserKey = userEmail;
            var groups = new List<Group>();

            do
            {
                var response = await request.ExecuteAsync();
                if (response.GroupsValue != null)
                {
                    groups.AddRange(response.GroupsValue);
                }
                request.PageToken = response.NextPageToken;
            } while (!string.IsNullOrEmpty(request.PageToken));

            return groups;
        }
        public async Task<User> GetUserByIdAsync(string userKey, string fileds = null)
        {
            var request = _service.Users.Get(userKey);
            request.Fields = fileds == null ? "name,primaryEmail,id,archived,suspended,thumbnailPhotoUrl,creationTime" : fileds;
            if (!string.IsNullOrEmpty(_appInfo.UserName) && _appInfo.UserName.Length < 40)
            {
                request.QuotaUser = _appInfo.UserName;
            }
            var user = await request.ExecuteExAsync();
            return user;
        }
        
        public async Task<Members> GetUsersInGroupByGroupIdAsync(string userKey, string fileds = null)
        {
            var request = _service.Members.List(userKey);
            request.Fields = fileds == null ? "id,email" : fileds;
            if (!string.IsNullOrEmpty(_appInfo.UserName) && _appInfo.UserName.Length < 40)
            {
                request.QuotaUser = _appInfo.UserName;
            }
            var members = await request.ExecuteExAsync();
            return members;
        }
        internal async Task<Byte[]> GetUserPhotoThumbnail(string thumnbnailUrl)
        {
            using (HttpResponseMessage response = await _service.HttpClient.GetAsync(thumnbnailUrl))
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
        }
        internal async Task<List<Domains>> ListDomainsAsync()
        {
            var request = _service.Domains.List("my_customer");
            if (!string.IsNullOrEmpty(_appInfo.UserName) && _appInfo.UserName.Length < 40)
            {
                request.QuotaUser = _appInfo.UserName;
            }
            var response = await request.ExecuteExAsync();
            if (response == null) { }
            return (List<Domains>)response?.Domains;
        }
        internal async Task<Domains> GetDomainsAsync(string domainName)
        {
            var request = _service.Domains.Get(_appInfo.UserName, domainName);
            if (!string.IsNullOrEmpty(_appInfo.UserName) && _appInfo.UserName.Length < 40)
            {
                request.QuotaUser = _appInfo.UserName;
            }
            var domains = await request.ExecuteExAsync();
            return domains;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _service?.Dispose();
            _service = null;
        }

        ~DirectoryApi()
        {
            Dispose(false);
        }
    }
}
