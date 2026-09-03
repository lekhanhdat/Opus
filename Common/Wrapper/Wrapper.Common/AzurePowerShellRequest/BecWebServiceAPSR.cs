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
namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Threading;
    using GCommon;
    using GCommon.Contract.SharePointBrowser.Object;
    using Microsoft.Online.Administration;
    using Microsoft.Online.Administration.Automation;

    class BecWebServiceAPSR : IAPSR
    {
        private static AveLogger logger = new AveLogger(typeof(BecWebServiceAPSR));

        private IProvisioningWebService proxy;
        private BecWebServiceInstance becWebServiceInstance;
        private IAPSTokenManager tokenManager;
        private ChannelFactory<IProvisioningWebService> channelFactory;

        public BecWebServiceAPSR(BecWebServiceInstance becWebServiceInstance, IAPSTokenManager tokenManager)
        {
            this.becWebServiceInstance = becWebServiceInstance;
            this.tokenManager = tokenManager;
            InitProxy(new string[0], 0);
        }

        private void InitProxy(IList<string> retryUrls, int retriedUrlIndex)
        {
            try
            {
                Dispose();

                WSHttpBinding binding = new WSHttpBinding(SecurityMode.Transport, false) { MaxReceivedMessageSize = 0x7fffffff };
                binding.ReceiveTimeout = new TimeSpan(0, 5, 0);
                binding.SendTimeout = new TimeSpan(0, 5, 0);
                binding.OpenTimeout = new TimeSpan(0, 5, 0);
                string siteUrl = retryUrls.Count == 0 ? becWebServiceInstance.GetBecWebServiceUri() : retryUrls[retriedUrlIndex];
                EndpointAddress endpoint = new EndpointAddress(siteUrl);
                channelFactory = new ChannelFactory<IProvisioningWebService>(binding, endpoint);

                var becWebServiceInspector = new BecWebServiceInspectorV1(tokenManager);
                var becWebServiceCustomBehavior = new BecWebServiceCustomBehavior(becWebServiceInspector);
                channelFactory.Endpoint.Behaviors.Add(becWebServiceCustomBehavior);
                proxy = channelFactory.CreateChannel();
            }
            catch (Exception e1)
            {
                logger.Warn("Failed to initial BecWebServiceAPSR, error detail : {0}", e1.ToString());
            }
        }

        private TResult WrapperRequest<TIn, TResult>(Func<TIn, TResult> func, TIn parameter)
        {
            var retryUrls = new List<string>();
            var retriedUrlIndex = 0;
            var hasThrottling = false;
            do
            {
                try
                {
                    hasThrottling = false;
                    return func(parameter);
                }
                catch (FaultException<ThrottlingException> faultException)
                {
                    logger.Warn("Call method:{0} with argument:{1} failed:{2}", func.GetInvocationList()[0].Method.Name, parameter, faultException);
                    hasThrottling = true;
                    Thread.Sleep(faultException.Detail.RetryWaitPeriod);
                }
                catch (FaultException<BindingRedirectionException> e)
                {
                    var currentDelegate = func.GetInvocationList()[0];
                    logger.Warn("Call method:{0} with argument:{1} failed:{2}", currentDelegate.Method.Name, parameter, e);
                    if (retryUrls.Count == 0)
                    {
                        retryUrls = e.Detail.Locations;
                    }
                    else
                    {
                        retriedUrlIndex++;
                    }
                    InitProxy(retryUrls, retriedUrlIndex);
                    //func = (Func<TIn, TResult>)Delegate.CreateDelegate(typeof(Func<TIn, TResult>), proxy, currentDelegate.Method);
                    var newFunc = (Func<IProvisioningWebService, TIn, TResult>)Delegate.CreateDelegate(typeof(Func<IProvisioningWebService, TIn, TResult>), currentDelegate.Method);
                    func = item => newFunc(proxy, item);
                }
                catch (Exception ex)
                {
                    logger.Error("Call method:{0} with argument:{1} failed:{2}", func.GetInvocationList()[0].Method.Name, parameter, ex);
                    break;
                }
            } while (retriedUrlIndex < retryUrls.Count || hasThrottling);

            return default(TResult);
        }

        private List<Group> GetGroups(GroupSearchDefinition search)
        {
            List<Group> groups = null;

            using (OperationContextScope contextScope = new OperationContextScope(proxy as IContextChannel))
            {
                ListGroupsRequest groupsRequest = new ListGroupsRequest();
                groupsRequest.BecVersion = Microsoft.Online.Administration.Version.Version16;
                groupsRequest.GroupSearchDefinition = search;

                ListGroupsResponse response = WrapperRequest(proxy.ListGroups, groupsRequest);
                if (response != null)
                {
                    groups = new List<Group>();
                    var currentGroups = response.ReturnValue.Results;
                    if (currentGroups != null && currentGroups.Count > 0)
                    {
                        groups.AddRange(currentGroups);
                    }

                    if (!response.ReturnValue.IsLastPage)
                    {
                        var listContext = response.ReturnValue.ListContext;
                        while (true)
                        {
                            var nextGroupRequest = new NavigateGroupResultsRequest();
                            nextGroupRequest.BecVersion = Microsoft.Online.Administration.Version.Version16;
                            nextGroupRequest.PageToNavigate = Page.Next;
                            nextGroupRequest.ListContext = listContext;

                            var nextGroupResult = WrapperRequest(proxy.NavigateGroupResults, nextGroupRequest);

                            if (nextGroupResult != null)
                            {
                                groups.AddRange(nextGroupResult.ReturnValue.Results);
                                listContext = nextGroupResult.ReturnValue.ListContext;
                                if (nextGroupResult.ReturnValue.IsLastPage)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }


                return groups;
            }
        }

        private List<User> GetUsers(UserSearchDefinition search)
        {
            List<User> users = null;

            using (OperationContextScope contextScope = new OperationContextScope(proxy as IContextChannel))
            {
                ListUsersRequest request = new ListUsersRequest();
                request.BecVersion = Microsoft.Online.Administration.Version.Version16;
                request.UserSearchDefinition = search;
                var response = WrapperRequest(proxy.ListUsers, request);

                if (response != null)
                {
                    users = new List<User>();

                    if (response.ReturnValue.Results != null)
                    {
                        users.AddRange(response.ReturnValue.Results);
                    }

                    if (!response.ReturnValue.IsLastPage)
                    {
                        logger.Info("Get the first page, the count of users is {0}, the account:{1}", response.ReturnValue.Results.Count, tokenManager);

                        var listContext = response.ReturnValue.ListContext;
                        while (true)
                        {
                            NavigateUserResultsRequest navigateUserRequest = new NavigateUserResultsRequest();
                            navigateUserRequest.BecVersion = Microsoft.Online.Administration.Version.Version16;
                            navigateUserRequest.PageToNavigate = Page.Next;
                            navigateUserRequest.ListContext = listContext;
                            var navigateUserResponse = WrapperRequest(proxy.NavigateUserResults, navigateUserRequest);
                            if (navigateUserResponse != null)
                            {
                                logger.Info("Get the next page, the count of users is {0}, the account:{1}", navigateUserResponse.ReturnValue.Results.Count, tokenManager);
                                users.AddRange(navigateUserResponse.ReturnValue.Results);
                                listContext = navigateUserResponse.ReturnValue.ListContext;
                                if (navigateUserResponse.ReturnValue.IsLastPage)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return users;
        }

        public List<Domain> GetDomains()
        {
            using (OperationContextScope contextScope = new OperationContextScope(proxy as IContextChannel))
            {
                var request = new ListDomainsRequest();
                request.SearchFilter = new DomainSearchFilter();
                request.BecVersion = Microsoft.Online.Administration.Version.Version16;
                var response = WrapperRequest(proxy.ListDomains, request);
                if (response != null)
                {
                    return response.ReturnValue;
                }
                return null;
            }
        }
        /// <summary>
        /// 由于能够创建DisplayName一样的Group
        /// 对于Office 365 Group需要匹配Email
        /// 由于Security Name同名的情况比较少，并且Office365中创建同名的Security Group会建议name为unique，暂时只通过DisplayName来判断。
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public Group GetGroup(string groupName, string email = null)
        {
            var groupSearchDefinition = new GroupSearchDefinition();
            groupSearchDefinition.PageSize = 500;
            groupSearchDefinition.SortDirection = SortDirection.Ascending;
            groupSearchDefinition.SortField = SortField.None;
            groupSearchDefinition.SearchString = groupName;

            var groups = GetGroups(groupSearchDefinition);
            if (groups != null)
            {
                foreach (var group in groups)
                {
                    if (groupName.Equals(group.DisplayName, StringComparison.Ordinal))
                    {
                        if (!string.IsNullOrEmpty(email) && !email.Equals(group.EmailAddress, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        return group;
                    }
                }
            }

            return null;
        }

        public List<GroupMember> GetGroupMembers(string groupName)
        {
            var group = GetGroup(groupName);
            if (group == null || group.ObjectId == Guid.Empty)
            {
                logger.Warn("The group:{0} is not found.", groupName);
                return null;
            }

            List<GroupMember> results = null;

            using (OperationContextScope contextScope = new OperationContextScope(proxy as IContextChannel))
            {
                var groupMemberRequest = new ListGroupMembersRequest();
                groupMemberRequest.BecVersion = Microsoft.Online.Administration.Version.Version16;
                groupMemberRequest.GroupMemberSearchDefinition = new GroupMemberSearchDefinition();
                groupMemberRequest.GroupMemberSearchDefinition.PageSize = 500;
                groupMemberRequest.GroupMemberSearchDefinition.SortDirection = SortDirection.Ascending;
                groupMemberRequest.GroupMemberSearchDefinition.SortField = SortField.None;
                groupMemberRequest.GroupMemberSearchDefinition.GroupObjectId = group.ObjectId.Value;

                var groupResponse = WrapperRequest(proxy.ListGroupMembers, groupMemberRequest);

                if (groupResponse != null)
                {
                    results = new List<GroupMember>();
                    if (groupResponse.ReturnValue.Results != null)
                    {
                        results.AddRange(groupResponse.ReturnValue.Results);
                    }

                    if (!groupResponse.ReturnValue.IsLastPage)
                    {
                        var listContext = groupResponse.ReturnValue.ListContext;
                        while (true)
                        {
                            var navigateMemberRequest = new NavigateGroupMemberResultsRequest();
                            navigateMemberRequest.BecVersion = Microsoft.Online.Administration.Version.Version16;
                            navigateMemberRequest.PageToNavigate = Page.Next;
                            navigateMemberRequest.ListContext = listContext;
                            var navigateMemberResponse = WrapperRequest(proxy.NavigateGroupMemberResults, navigateMemberRequest);
                            if (navigateMemberResponse != null)
                            {
                                results.AddRange(navigateMemberResponse.ReturnValue.Results);
                                listContext = navigateMemberResponse.ReturnValue.ListContext;
                                if (navigateMemberResponse.ReturnValue.IsLastPage)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return results;
        }

        public List<Group> GetGroups()
        {
            var groupSearchDefinition = new GroupSearchDefinition();
            groupSearchDefinition.PageSize = 500;
            groupSearchDefinition.SortDirection = SortDirection.Ascending;
            groupSearchDefinition.SortField = SortField.None;

            return GetGroups(groupSearchDefinition);
        }

        public List<Subscription> GetSubscriptions()
        {
            Request request = new Request();
            request.BecVersion = Microsoft.Online.Administration.Version.Version16;
            var response = WrapperRequest(proxy.ListSubscriptions, request);
            if (response != null)
            {
                return response.ReturnValue;
            }
            return null;
        }

        public User GetUser(string userPrincipalName)
        {
            var userByUpnRequest = new GetUserByUpnRequest();
            userByUpnRequest.UserPrincipalName = userPrincipalName;
            var response = WrapperRequest(proxy.GetUserByUpn, userByUpnRequest);

            if (response != null)
            {
                return response.ReturnValue;
            }

            return null;
        }

        public List<Role> GetUserRoles(string userPrincipalName)
        {
            var user = GetUser(userPrincipalName);

            if (user.ObjectId != Guid.Empty)
            {
                ListRolesForUserRequest rolesForUserRequest = new ListRolesForUserRequest();
                rolesForUserRequest.ObjectId = user.ObjectId.Value;
                var response = WrapperRequest(proxy.ListRolesForUser, rolesForUserRequest);

                if (response != null)
                {
                    return response.ReturnValue;
                }
            }

            return null;
        }

        public List<User> GetUsers()
        {
            var search = new UserSearchDefinition();
            search.PageSize = 500;
            search.SortDirection = SortDirection.Ascending;
            search.SortField = SortField.None;

            return GetUsers(search);
        }

        public void Dispose()
        {
            if (channelFactory != null)
            {
                channelFactory.Close();
                channelFactory = null;
            }
        }
    }
}
