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
namespace AvePoint.ObjectModel.ClientOM
{
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint.Client;
    using System;
    using System.Collections.Generic;

    public partial class AveClientOM2013Request
    {
        #region private
        private void AssembleUserCustomActionProperties(Dictionary<string, object> userCustonActionProperties, UserCustomAction userCustomAction)
        {
            CopyProperty(userCustonActionProperties, userCustomAction);
            object temp;
            if (userCustonActionProperties.TryGetValue("Scope", out temp))
            {
                userCustonActionProperties["Scope"] = (AveUserCustomActionScope)temp;
            }
            if (userCustonActionProperties.TryGetValue("RegistrationType", out temp))
            {
                userCustonActionProperties["RegistrationType"] = (AveUserCustomActionRegistrationType)temp;
            }
            if (userCustonActionProperties.TryGetValue("Rights", out temp) && temp is BasePermissions)
            {
                AveBasePermissions rights = AveBasePermissions.EmptyMask;
                var basePerms = temp as BasePermissions;
                foreach (int perm in Enum.GetValues(typeof(PermissionKind)))
                {
                    if (basePerms.Has((PermissionKind)Enum.ToObject(typeof(PermissionKind), perm)))
                    {
                        string permissionStr = Enum.GetName(typeof(PermissionKind), perm);
                        rights = (AveBasePermissions)Enum.Parse(typeof(AveBasePermissions), permissionStr) | rights;
                    }
                }
                userCustonActionProperties["Rights"] = rights;
            }
        }

        private UserCustomActionCollection UserCustomActionCollection_Get(AveClientContext context,AveUserCustomActionScope scope, string webUrl,Guid listId)
        {
            switch (scope)
            {
                case AveUserCustomActionScope.Site:
                    return context.Site.UserCustomActions;
                case AveUserCustomActionScope.Web:
                    return context.Site.OpenWeb(webUrl).UserCustomActions;
                case AveUserCustomActionScope.List:
                    return context.Site.OpenWeb(webUrl).Lists.GetById(listId).UserCustomActions;
                case AveUserCustomActionScope.Unknown:
                default:
                    throw new ArgumentException("Invalid UserCustomActionScope " + scope);
            }
        }
        #endregion private
        #region IAveRequest
        public Dictionary<string, object> UserCustomActionCollection_Add(AveUserCustomActionScope scope,string webUrl,Guid listId,string location)
        {
            using (AveClientContext context = CreateContext())
            {
                var collection = UserCustomActionCollection_Get(context, scope, webUrl, listId);
                var action = collection.Add();
                action.Location = location;
                action.Update();
                context.Load(action);
                context.ExecuteQuery();
                var actionProperties = new Dictionary<string, object>();
               AssembleUserCustomActionProperties(actionProperties, action);
                return actionProperties;
            }
        }

        public Dictionary<string, object> UserCustomAction_Update(AveUserCustomActionScope scope, string webUrl, Guid listId,Guid actionId, Dictionary<string, object> changeProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                var collection = UserCustomActionCollection_Get(context, scope, webUrl, listId);
                var action = collection.GetById(actionId);
                AveObjectCopy.UpdateObjectBasicProperties(changeProperties, action);
                action.Update();
                context.Load(action);
                context.ExecuteQuery();
                var actionProperties = new Dictionary<string, object>();
                AssembleUserCustomActionProperties(actionProperties, action);
                return actionProperties;
            }
        }

        public void UserCustomAction_Delete(AveUserCustomActionScope scope, string webUrl, Guid listId, Guid actionId)
        {
            using (AveClientContext context = CreateContext())
            {
                var collection = UserCustomActionCollection_Get(context, scope, webUrl, listId);
                var action = collection.GetById(actionId);
               
                context.Load(action);
                ConditionalScope condition = new ConditionalScope(context, ()=>(action.ServerObjectIsNull.HasValue && !action.ServerObjectIsNull.Value));
                using (condition.StartScope())
                {
                    using (condition.StartIfTrue())
                    {
                        action.DeleteObject();
                    }
                }
                context.ExecuteQuery();
            }
        }

        public void UserCustomActionCollection_Clear(AveUserCustomActionScope scope, string webUrl, Guid listId)
        {
            using (AveClientContext context = CreateContext())
            {
                var collection = UserCustomActionCollection_Get(context, scope, webUrl, listId);
                collection.Clear();
                context.ExecuteQuery();
            }
        }

        public Dictionary<string,object> UserCustomActionCollection_Load(AveUserCustomActionScope scope, string webUrl, Guid listId)
        {
            var children = new List<IDictionary<string, object>>();
            using (AveClientContext context = CreateContext())
            {
                var collection = UserCustomActionCollection_Get(context, scope, webUrl, listId);
                context.Load(collection);
                context.ExecuteQuery();
                foreach (var action in collection)
                {
                    Dictionary<string, object> properties = new Dictionary<string, object>();
                    AssembleUserCustomActionProperties(properties, action);
                    children.Add(properties);
                }
            }
            var result = new Dictionary<string, object>();
            result.AddChildren(children);
            return result;
        }
        #endregion  IAveRequest
        #region Obsolete methods
        [Obsolete]
        public Dictionary<string, object> AddUserCustomAction(string webServerRelativeUrl, string location)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                UserCustomAction newUserCustomAction = web.UserCustomActions.Add();
                newUserCustomAction.Location = location;
                newUserCustomAction.Update();
                context.Load(newUserCustomAction);
                context.ExecuteQuery();
                Dictionary<string, object> userCustomActionProp = new Dictionary<string, object>();
                AssembleUserCustomActionProperties(userCustomActionProp, newUserCustomAction);
                return userCustomActionProp;
            }
        }

        [Obsolete]
        public void DeleteUserCustomAction(string webServerRelativeUrl, Guid userCustomActionId)
        {
            using (AveClientContext context = CreateContext())
            {
                UserCustomAction userCustomAction = FindUserCustomAction(webServerRelativeUrl, userCustomActionId, context);
                if (userCustomAction != null)
                {
                    userCustomAction.DeleteObject();
                    context.ExecuteQuery();
                }
            }
        }
        [Obsolete]
        public void UserCustomActionsClear(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                web.UserCustomActions.Clear();
                context.ExecuteQuery();
            }
        }
        [Obsolete]
        private UserCustomAction FindUserCustomAction(string webServerRelativeUrl, Guid userCustomActionId, ClientContext context)
        {
            Web web = context.Site.OpenWeb(webServerRelativeUrl);
            UserCustomAction userCustomAction = null;
            try
            {
                userCustomAction = web.UserCustomActions.GetById(userCustomActionId);
                context.Load(userCustomAction);
                context.ExecuteQuery();
            }
            catch (Exception ex)
            {
                mLogger.Debug("An error occurred while finding userCustomAction.Message:{0}.", ex.ToString());
                userCustomAction = null;
            }
            return userCustomAction;
        }

        [Obsolete]
        public Dictionary<string, object> UpdateUserCustomAction(string webServerRelativeUrl, Guid userCustomActionId, Dictionary<string, object> userCustomActionProperties)
        {
            using (AveClientContext context = CreateContext())
            {
                UserCustomAction userCustomAction = FindUserCustomAction(webServerRelativeUrl, userCustomActionId, context);
                if (userCustomAction != null && userCustomActionProperties.Count > 0)
                {
                    AveObjectCopy.UpdateObjectBasicProperties(userCustomActionProperties, userCustomAction);
                    userCustomAction.Update();
                    context.Load(userCustomAction);
                    context.ExecuteQuery();
                    Dictionary<string, object> userCustomActionProp = new Dictionary<string, object>();
                    AssembleUserCustomActionProperties(userCustomActionProp, userCustomAction);
                    return userCustomActionProp;
                }
                else
                {
                    return null;
                }
            }
        }

        [Obsolete]
        public Dictionary<string, object> GetUserCustomActions(string webServerRelativeUrl)
        {
            using (AveClientContext context = CreateContext())
            {
                Dictionary<string, object> actionsProperties = new Dictionary<string, object>();
                var actionList = new List<IDictionary<string, object>>();
                Web web = context.Site.OpenWeb(webServerRelativeUrl);
                context.Load(web, w => w.UserCustomActions);
                context.ExecuteQuery();
                foreach (UserCustomAction userCustomAction in web.UserCustomActions)
                {
                    Dictionary<string, object> actionProperty = new Dictionary<string, object>();
                    AssembleUserCustomActionProperties(actionProperty, userCustomAction);
                    actionList.Add(actionProperty);
                }
                actionsProperties.AddChildren(actionList);
                return actionsProperties;
            }
        }
        #endregion Obsolete methods
    }
}
