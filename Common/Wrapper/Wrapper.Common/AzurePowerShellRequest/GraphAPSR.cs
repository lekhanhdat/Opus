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
    using System.Linq;
    using System.Collections.Generic;
    using Microsoft.Online.Administration;
    using Graph = Microsoft.Azure.ActiveDirectory.GraphClient;

    class GraphAPSR : IAPSR
    {
        private MFATokenManager tokenManager;
        private Graph.ActiveDirectoryClient client;

        public GraphAPSR(MFATokenManager tokenManager)
        {
            this.tokenManager = tokenManager;
            this.client = new Graph.ActiveDirectoryClient(new Uri("https://graph.windows.net/myorganization"), async () => tokenManager.Token);
        }

        private List<UserLicense> Convert(IList<Graph.AssignedLicense> licenses)
        {
            List<UserLicense> newLicenses = null;

            if(licenses != null && licenses.Count > 0)
            {
                newLicenses = new List<UserLicense>();

                foreach (var license in licenses)
                {
                    newLicenses.Add(new UserLicense() { AccountSkuId = license.SkuId.Value.ToString() });
                }
            }

            return newLicenses;
        }

        private User Convert(Graph.IUser user)
        {
            var newUser = new User()
            {
                AlternateEmailAddresses = new List<string>(user.OtherMails),
                BlockCredential = !user.AccountEnabled,
                City = user.City,
                Country = user.Country,
                Department = user.Department,
                DisplayName = user.DisplayName,
                Fax = user.FacsimileTelephoneNumber,
                FirstName = user.GivenName,
                ImmutableId = user.ImmutableId,
                IsLicensed = user.AssignedPlans.Count > 0,
                LastDirSyncTime = user.LastDirSyncTime,
                LastName = user.Surname,
                Licenses = Convert(user.AssignedLicenses),
                LiveId = null,
                MobilePhone = user.Mobile,
                ObjectId = new Guid(user.ObjectId),
                Office = user.PhysicalDeliveryOfficeName,
                PhoneNumber = user.TelephoneNumber,
                PostalCode = user.PostalCode,
                ProxyAddresses = new List<string>(user.ProxyAddresses),
                PreferredLanguage = user.PreferredLanguage,
                SoftDeletionTimestamp = user.DeletionTimestamp,
                State = user.State,
                StreetAddress = user.StreetAddress,
                Title = user.JobTitle,
                UsageLocation = user.UsageLocation,
                UserPrincipalName = user.UserPrincipalName,
                UserType = (UserType)Enum.Parse(typeof(UserType), user.UserType, true),
            };

            return newUser;
        }

        private Group Convert(Graph.IGroup group)
        {
            var newGroup = new Group()
            {
                DisplayName = group.DisplayName,
                Description = group.Description,
                EmailAddress = group.Mail,
                LastDirSyncTime = group.LastDirSyncTime,
                ProxyAddresses = group.ProxyAddresses == null ? null : new List<string>(group.ProxyAddresses),
            };

            if (!string.IsNullOrEmpty(group.ObjectId))
            {
                newGroup.ObjectId = new Guid(group.ObjectId);
            }

            if (group.SecurityEnabled == true)
            {
                if (group.MailEnabled == true)
                {
                    newGroup.GroupType = GroupType.MailEnabledSecurity;
                }
                else
                {
                    newGroup.GroupType = GroupType.Security;
                }
            }
            else if(group.MailEnabled == true)
            {
                newGroup.GroupType = GroupType.DistributionList;
            }
            else
            {
                newGroup.GroupType = GroupType.Security;
            }

            return newGroup;
        }

        public List<Domain> GetDomains()
        {
            List<Domain> domains = null;
            var pageCollection = client.TenantDetails.ExecuteAsync().Result;

            if (pageCollection != null)
            {
                domains = new List<Domain>();
                do
                {
                    foreach (var detail in pageCollection.CurrentPage)
                    {
                        foreach (var domain in detail.VerifiedDomains)
                        {
                            domains.Add(new Domain()
                            {
                                Name = domain.Name,
                                Capabilities = (DomainCapabilities)Enum.Parse(typeof(DomainCapabilities), domain.Capabilities, true),
                                IsDefault = domain.@default,
                                IsInitial = domain.Initial,
                                Status = DomainStatus.Verified,
                                Authentication = (DomainAuthenticationType)Enum.Parse(typeof(DomainAuthenticationType), domain.Type, true),
                            });
                        }
                    }
                    if (pageCollection.MorePagesAvailable)
                    {
                        pageCollection = pageCollection.GetNextPageAsync().Result;
                    }
                    else
                    {
                        break;
                    }
                }
                while (pageCollection != null);
            }

            return domains;
        }

        public Group GetGroup(string groupName, string email = null)
        {
            var groups = client.Groups.Where(g => g.DisplayName.Equals(groupName, StringComparison.OrdinalIgnoreCase)).ExecuteAsync().Result;

            if (groups != null && groups.CurrentPage.Count > 0)
            {
                return Convert(groups.CurrentPage.First());
            }
            return null;
        }

        public List<GroupMember> GetGroupMembers(string groupName)
        {
            var groups = client.Groups.Where(g => g.DisplayName.Equals(groupName, StringComparison.OrdinalIgnoreCase)).Expand(g => g.Members).ExecuteAsync().Result;
            List<GroupMember> members = null;

            if (groups != null && groups.CurrentPage.Count > 0)
            {
                var group = groups.CurrentPage.First();
                var pageCollection = group.Members;
                members = new List<GroupMember>();

                do
                {
                    foreach (var member in pageCollection.CurrentPage)
                    {
                        var user = member as Graph.User;

                        if (user != null)
                        {
                            members.Add(new GroupMember()
                            {
                                DisplayName = user.DisplayName,
                                EmailAddress = user.Mail,
                                IsLicensed = user.AssignedLicenses.Count > 0,
                                LastDirSyncTime = user.LastDirSyncTime,
                                ObjectId = new Guid(user.ObjectId),
                                GroupMemberType = GroupMemberType.User,
                            });
                        }
                        else if(member is Graph.Contact)
                        {
                            var contact = (Graph.Contact)member;
                            members.Add(new GroupMember()
                            {
                                DisplayName = contact.DisplayName,
                                EmailAddress = contact.Mail,
                                LastDirSyncTime = contact.LastDirSyncTime,
                                ObjectId = new Guid(contact.ObjectId),
                                GroupMemberType = GroupMemberType.Contact,
                            });
                        }
                        else if (member is Graph.Group)
                        {
                            var mIsGroup = (Graph.Group)member;
                            members.Add(new GroupMember()
                            {
                                DisplayName = mIsGroup.DisplayName,
                                EmailAddress = mIsGroup.Mail,
                                LastDirSyncTime = mIsGroup.LastDirSyncTime,
                                ObjectId = new Guid(mIsGroup.ObjectId),
                                GroupMemberType = GroupMemberType.Group,
                            });
                        }
                        else if (member is Graph.ServicePrincipal)
                        {
                            var sp = (Graph.ServicePrincipal)member;
                            members.Add(new GroupMember()
                            {
                                DisplayName = sp.DisplayName,
                                ObjectId = new Guid(sp.ObjectId),
                                GroupMemberType = GroupMemberType.ServicePrincipal,
                            });
                        }
                        else
                        {
                            members.Add(new GroupMember()
                            {
                                ObjectId = new Guid(member.ObjectId),
                                GroupMemberType = GroupMemberType.Other,
                            });
                        }
                    }
                    if (pageCollection.MorePagesAvailable)
                    {
                        pageCollection = pageCollection.GetNextPageAsync().Result;
                    }
                    else
                    {
                        break;
                    }
                }
                while (pageCollection != null);
            }

            return members;
        }

        public List<Group> GetGroups()
        {
            List<Group> groups = null;

            var pageCollection = client.Groups.ExecuteAsync().Result;

            if (pageCollection != null)
            {
                groups = new List<Group>();

                do
                {
                    foreach (var group in pageCollection.CurrentPage)
                    {
                        groups.Add(Convert(group));
                    }
                    if (pageCollection.MorePagesAvailable)
                    {
                        pageCollection = pageCollection.GetNextPageAsync().Result;
                    }
                    else
                    {
                        break;
                    }
                }
                while (pageCollection != null);
            }

            return groups;
        }

        public List<Subscription> GetSubscriptions()
        {
            List<Subscription> subscriptions = null;

            var pageCollection = client.SubscribedSkus.ExecuteAsync().Result;

            if (pageCollection != null)
            {
                subscriptions = new List<Subscription>();
                do
                {
                    foreach (var sub in pageCollection.CurrentPage)
                    {
                        subscriptions.Add(new Subscription()
                        {
                            ObjectId = new Guid(sub.ObjectId),
                            //OcpSubscriptionId = sub.SkuId,
                            SkuId = sub.SkuId,
                            SkuPartNumber = sub.SkuPartNumber,
                        });
                    }

                    if (pageCollection.MorePagesAvailable)
                    {
                        pageCollection = pageCollection.GetNextPageAsync().Result;
                    }
                    else
                    {
                        break;
                    }
                } while (pageCollection != null);
            }

            return subscriptions;
        }

        public User GetUser(string userPrincipalName)
        {
            var users = client.Users.Where(u => u.UserPrincipalName.Equals(userPrincipalName, StringComparison.OrdinalIgnoreCase)).ExecuteAsync().Result;

            if (users != null && users.CurrentPage.Count > 0)
            {
                return Convert(users.CurrentPage.First());
            }

            return null;
        }

        public List<Role> GetUserRoles(string userPrincipalName)
        {
            List<Role> roles = null;
            var users = client.Users.Where(u => u.UserPrincipalName.Equals(userPrincipalName, StringComparison.OrdinalIgnoreCase)).Expand(u => u.MemberOf).ExecuteAsync().Result;

            if (users != null && users.CurrentPage.Count > 0)
            {
                var user = users.CurrentPage.First();

                var pageCollection = user.MemberOf;

                if (pageCollection != null)
                {
                    roles = new List<Role>();
                    do
                    {
                        foreach (var item in pageCollection.CurrentPage)
                        {
                            var directoryRole = item as Graph.DirectoryRole;

                            if (directoryRole != null)
                            {
                                roles.Add(new Role()
                                {
                                    Description = directoryRole.Description,
                                    IsEnabled = !directoryRole.RoleDisabled,
                                    IsSystem = directoryRole.IsSystem,
                                    Name = directoryRole.DisplayName,
                                    ObjectId = new Guid(directoryRole.RoleTemplateId)
                                });
                            }
                        }

                        if (pageCollection.MorePagesAvailable)
                        {
                            pageCollection = pageCollection.GetNextPageAsync().Result;
                        }
                        else
                        {
                            break;
                        }
                    }
                    while (pageCollection != null);
                }
            }

            return roles;
        }

        public List<User> GetUsers()
        {
            List<User> users = null;

            var pageCollection = client.Users.ExecuteAsync().Result;

            if (pageCollection != null)
            {
                users = new List<User>();
                do
                {
                    foreach (var user in pageCollection.CurrentPage)
                    {
                        users.Add(Convert(user));
                    }

                    if (pageCollection.MorePagesAvailable)
                    {
                        pageCollection = pageCollection.GetNextPageAsync().Result;
                    }
                    else
                    {
                        break;
                    }
                } while (pageCollection != null);
            }

            return users;
        }

        public void Dispose()
        {
            
        }
    }
}
