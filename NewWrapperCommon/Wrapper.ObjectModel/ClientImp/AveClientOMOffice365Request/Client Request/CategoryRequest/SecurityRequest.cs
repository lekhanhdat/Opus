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
using AveClientRequest.Common;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Utilities;
using System;
using System.Collections.Generic;

namespace AvePoint.ObjectModel.ClientOM
{
    public partial class AveClientOMOffice365Request : AveClientOM2019Request
    {

        [ReplaceByAPI]
        public override Dictionary<string, object> GetGroups(string webRelativeUrl, string groupColSource, string loginName)
        {
            switch (groupColSource)
            {
                case "web.siteGroups":
                    return base.GetGroups(webRelativeUrl, groupColSource, loginName);
                case "web.groups":
                    return GetWebGroups(webRelativeUrl);
                case "user.groups":
                    return GetUserGroups(webRelativeUrl, loginName);
            }
            throw new NotSupportedException();
        }

        private Dictionary<string, object> GetUserGroups(string webRelativeUrl, string loginName)
        {
            Dictionary<string, object> groups = new Dictionary<string, object>();
            List<Dictionary<string, object>> groupList = new List<Dictionary<string, object>>();
            groups.Add(AveObjectModelConstant.ChildrenProperties, groupList);

            using (AveClientContext context = CreateContext())
            {

                Web web = context.Site.OpenWeb(webRelativeUrl);

                var user = web.SiteUsers.GetByLoginName(loginName);
                context.Load(user.Groups, gs => gs.IncludeWithDefaultProperties(g => g.Owner.Id, g => g.Owner.PrincipalType));
                context.ExecuteQuery();

                foreach (Group group in user.Groups)
                {
                    Dictionary<string, object> groupProp = GetGroupProperties(this.mSiteTrimObj, context, group, true);
                    groupList.Add(groupProp);
                }
            }

            return groups;
        }


        private Dictionary<string, object> GetWebGroups(string webRelativeUrl)
        {
            Dictionary<string, object> groups = new Dictionary<string, object>();
            List<Dictionary<string, object>> groupList = new List<Dictionary<string, object>>();
            groups.Add(AveObjectModelConstant.ChildrenProperties, groupList);

            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webRelativeUrl);
                context.Load(web, a => a.HasUniqueRoleAssignments,
                    a => a.RoleAssignments.Groups.IncludeWithDefaultProperties(g => g.Owner.Id, g => g.Owner.PrincipalType));
                context.ExecuteQuery();

                if (web.HasUniqueRoleAssignments)
                {
                    foreach (Group group in web.RoleAssignments.Groups)
                    {
                        Dictionary<string, object> groupProp = GetGroupProperties(this.mSiteTrimObj, context, group, true);
                        groupList.Add(groupProp);
                    }
                }
            }

            return groups;
        }


        [ReplaceByAPI]
        public override Dictionary<string, object> GetUsers(string webRelativeUrl, string groupName, string userColSource)
        {
            switch (userColSource)
            {
                case "web.users":
                    return GetUsers(webRelativeUrl);
                case "web.allUsers":
                case "web.siteUsers":
                    return GetSiteUsers(webRelativeUrl);
                case "group.users":
                    return GetGroupUsers(webRelativeUrl, groupName);
                default:
                    throw new Exception("unsupported source:" + userColSource);
            }
            //return base.GetUsers(webRelativeUrl, groupName, userColSource);

        }

        private Dictionary<string, object> GetGroupUsers(string webRelativeUrl, string groupName)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webRelativeUrl);
                var group = web.SiteGroups.GetByName(groupName);
                context.Load(group.Users, a => a.IncludeWithDefaultProperties());
                context.ExecuteQuery();

                return ConvertUserCollection(group.Users);
            }
        }

        private Dictionary<string, object> GetSiteUsers(string webRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webRelativeUrl);
                context.Load(web.SiteUsers, a => a.IncludeWithDefaultProperties());
                context.ExecuteQuery();

                return ConvertUserCollection(web.SiteUsers);
            }
        }

        private Dictionary<string, object> ConvertUserCollection(UserCollection users)
        {
            var userCollectionProperties = new Dictionary<string, object>();
            var userPropertiesList = new List<Dictionary<string, object>>();
            userCollectionProperties.Add(AveObjectModelConstant.ChildrenProperties, userPropertiesList);

            foreach (var user in users)
            {
                userPropertiesList.Add(ConvertUser(user));
            }

            return userCollectionProperties;
        }
        /// <summary>
        /// Web.Users
        /// </summary>
        /// <param name="webRelativeUrl"></param>
        /// <returns></returns>
        private Dictionary<string, object> GetUsers(string webRelativeUrl)
        {
            var userCollectionProperties = new Dictionary<string, object>();
            var userPropertiesList = new List<Dictionary<string, object>>();
            userCollectionProperties.Add(AveObjectModelConstant.ChildrenProperties, userPropertiesList);

            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webRelativeUrl);
                context.Load(web, a => a.HasUniqueRoleAssignments,
                    a => a.RoleAssignments.Include(r => r.Member),
                    a => a.RoleAssignments.Groups.Include(g => g.Users.IncludeWithDefaultProperties()));
                context.ExecuteQuery();

                if (web.HasUniqueRoleAssignments)
                {
                    var userId = new HashSet<int>();
                    foreach (var role in web.RoleAssignments)
                    {
                        var user = role.Member as User;

                        if (user != null)
                        {
                            userId.Add(user.Id);
                            userPropertiesList.Add(ConvertUser(user));
                        }
                    }

                    foreach (var group in web.RoleAssignments.Groups)
                    {
                        foreach (var user in group.Users)
                        {
                            if (!userId.Contains(user.Id))
                            {
                                userId.Add(user.Id);
                                userPropertiesList.Add(ConvertUser(user));
                            }
                        }
                    }
                }
            }

            return userCollectionProperties;
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetUser(int id)
        {
            return base.GetUser(id);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetUser(string userEmail)
        {
            return base.GetUser(userEmail);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetEnsureUser(string webServerRelativeUrl, string loginName)
        {
            return base.GetEnsureUser(webServerRelativeUrl, loginName);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetRoleDefinitions(string webServerRelativeUrl)
        {
            return base.GetRoleDefinitions(webServerRelativeUrl);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> GetRoleAssignments(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, int itemId, string roleAssignmentsSource)
        {
            return base.GetRoleAssignments(webServerRelativeUrl, listServerRealtiveUrl, listTitle, listId, itemId, roleAssignmentsSource);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> ResolvePrincipal(string webServerRelativeUrl, string input, int scopes, int sources, bool inputIsEmailOnly, bool ignoreDomainDiff)
        {
            return base.ResolvePrincipal(webServerRelativeUrl, input, scopes, sources, inputIsEmailOnly, ignoreDomainDiff);
        }
        [KeepOriginalWithAPI]
        public override Dictionary<string, object> SearchPrincipals(string webServerRelativeUrl, string input, int scopes, int sources, int maxCount)
        {
            return base.SearchPrincipals(webServerRelativeUrl, input, scopes, sources, maxCount);
        }

        [ReplaceByAPI]
        public override Dictionary<string, object> AddUser(string webServerRelativeUrl, string source, string groupName, Dictionary<string, object> userProp)
        {
            string userName = userProp["Name"] as string;
            string userLoginName = userProp["LoginName"] as string;
            string userEmail = userProp["Email"] as string;
            string userNotes = userProp["Notes"] as string;
            int userId = (int)userProp["ID"];
            var userCreationInfo = new UserCreationInformation()
            {
                Email = userEmail,
                LoginName = userLoginName,
                Title = userName,
            };

            using (var context = CreateContext())
            {
                User user = null;

                switch (source)
                {
                    case "group.users":
                        user = AddGroupUser(context, groupName, userName, userLoginName, userEmail, userId);
                        break;
                    case "web.allUsers":
                    case "web.users":
                    case "web.siteUsers":
                        user = context.Web.SiteUsers.Add(userCreationInfo);
                        break;
                    case "web.siteAdministrators":
                        user = context.Web.SiteUsers.Add(userCreationInfo);
                        user.IsSiteAdmin = true;
                        user.Update();
                        break;
                    default:
                        break;
                }

                if (user != null)
                {
                    context.Load(user);
                    context.ExecuteQuery();

                    return ConvertUser(user);
                }
            }
            return new Dictionary<string, object>();
        }

        private Dictionary<string, object> ConvertUser(User user)
        {
            Dictionary<string, object> userProperties = new Dictionary<string, object>();
            CopyProperty(userProperties, user);

            if (user.IsPropertyAvailable("UserId")
                && user.UserId != null
                && (!string.IsNullOrEmpty(user.UserId.NameId))
                && user.UserId.NameId.StartsWith("S-", StringComparison.OrdinalIgnoreCase))
            {
                userProperties.Add("SID", user.UserId.NameId);
            }

            userProperties["IsDomainGroup"] = user.PrincipalType != PrincipalType.User;
            userProperties["Name"] = user.Title;

            return userProperties;
        }

        private User AddGroupUser(ClientContext context, string groupName, string userName, string userLoginName, string userEmail, int userId)
        {
            User user;
            if (userId > 0)
            {
                mLogger.Info("Add user {0} to group {1} by user id.", userId, groupName);
                var tempUser = context.Web.SiteUsers.GetById(userId);
                user = context.Web.SiteGroups.GetByName(groupName).Users.AddUser(tempUser);
            }
            else
            {
                mLogger.Info("Add user {0}|{1}|{2} to group {3} by user info.", userName, userLoginName, userEmail, groupName);
                var userCreationInfo = new UserCreationInformation()
                {
                    Email = userEmail,
                    LoginName = userLoginName,
                    Title = userName,
                };
                user = context.Web.SiteGroups.GetByName(groupName).Users.Add(userCreationInfo);
            }
            return user;
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> AddRoleDefinition(string webServerRelativeUrl, Dictionary<string, object> roleDefinitionProperties)
        {
            return base.AddRoleDefinition(webServerRelativeUrl, roleDefinitionProperties);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> AddRoleAssignment(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, int itemId, Dictionary<string, object> roleAssignmentProperties, string roleAssignmentsSource)
        {
            return base.AddRoleAssignment(webServerRelativeUrl, listServerRealtiveUrl, listTitle, listId, itemId, roleAssignmentProperties, roleAssignmentsSource);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> AddGroup(string webRelativeUrl, string ownerName, string ownerType, string defaultUserName, string groupName, string description, string groupSource)
        {
            return base.AddGroup(webRelativeUrl, ownerName, ownerType, defaultUserName, groupName, description, groupSource);
        }

        [KeepOriginalWithAPI]
        public override void DeleteGroup(string webServerRelativeUrl, int id)
        {
            base.DeleteGroup(webServerRelativeUrl, id);
        }

        [KeepOriginalWithAPI]
        public override void DeleteRoleAssignment(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, int itemId, int principalId, string source)
        {
            base.DeleteRoleAssignment(webServerRelativeUrl, listServerRelativeUrl, listTitle, listId, itemId, principalId, source);
        }

        [KeepOriginalWithAPI]
        public override void DeleteRoleDefinition(string webServerRelativeUrl, string roleDefintionName)
        {
            base.DeleteRoleDefinition(webServerRelativeUrl, roleDefintionName);
        }

        [KeepOriginalWithAPI]
        public override void DeleteUser(string webServerRelativeUrl, string source, string groupName, string loginName)
        {
            base.DeleteUser(webServerRelativeUrl, source, groupName, loginName);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> UpdateGroup(string webServerRelativeUrl, int id, Dictionary<string, object> groupProperties)
        {
            return base.UpdateGroup(webServerRelativeUrl, id, groupProperties);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> UpdateRoleAssignment(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, int itemId, int principalId, Dictionary<string, object> needUpdateRoleAssignmentProperties, string roleAssignmentsSource)
        {
            return base.UpdateRoleAssignment(webServerRelativeUrl, listServerRelativeUrl, listTitle, listId, itemId, principalId, needUpdateRoleAssignmentProperties, roleAssignmentsSource);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> UpdateRoleDefinition(string webServerRelativeUrl, int id, Dictionary<string, object> needUpdateRoledefinitionProperties)
        {
            return base.UpdateRoleDefinition(webServerRelativeUrl, id, needUpdateRoledefinitionProperties);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> BreakRoleInheritance(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, int itemId, bool copyRoleAssignments, bool clearSubscopes, string roleAssignmentsSource)
        {
            return base.BreakRoleInheritance(webServerRelativeUrl, listServerRelativeUrl, listTitle, listId, itemId, copyRoleAssignments, clearSubscopes, roleAssignmentsSource);
        }

        [KeepOriginalWithAPI]
        public override Dictionary<string, object> ResetRoleInheritance(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, int itemId, string roleAssignmentsSource)
        {
            return base.ResetRoleInheritance(webServerRelativeUrl, listServerRelativeUrl, listTitle, listId, itemId, roleAssignmentsSource);
        }

        [NoAPI("Cannot find User.Notes property")]
        public override Dictionary<string, object> UpdateUser(string webServerRelativeUrl, string loginName, string name, string userColSource, Dictionary<string, object> userProp)
        {
            using (var context = CreateContext())
            {
                var user = context.Web.SiteUsers.GetByLoginName(loginName);
                bool changed = false;
                foreach (KeyValuePair<string, object> pair in userProp)
                {
                    switch (pair.Key)
                    {
                        case "Email":
                            user.Email = userProp["Email"] as string;
                            changed = true;
                            break;
                        case "Name":
                            user.Title = userProp["Name"] as string;
                            changed = true;
                            break;
                        case "Notes":
                            //user. = userProp["Notes"] as string;
                            //need to update the list item field, no need to keep for this.
                            break;
                        case "IsSiteAdmin":
                            user.IsSiteAdmin = Convert.ToBoolean(pair.Value);
                            changed = true;
                            break;
                        default:
                            break;
                    }
                }

                if (changed)
                {
                    user.Update();
                }

                context.Load(user);
                context.ExecuteQuery();
                return ConvertUser(user);
            }

        }
    }
}