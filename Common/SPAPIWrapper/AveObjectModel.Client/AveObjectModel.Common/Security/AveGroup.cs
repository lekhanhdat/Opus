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
using System.Text;
using AvePoint.Wrapper.Common;
namespace AvePoint.ObjectModel.Common
{
    class AveGroup : AvePrincipal, IAveGroup
    {
        private IAveRequest mRequest;
        private AveWeb mWeb;

        public AveGroup(IAveRequest request, AveWeb web, IDictionary<string, object> groupProperties)
        {
            mWeb = web;
            mRequest = request;
            groupProperties["ParentWeb"] = web;
            base.DataCache.AddPropertyies(groupProperties);
        }

        public bool IsHiddenInUI
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsHiddenInUI");
            }
        }

        public bool AllowMembersEditMembership
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowMembersEditMembership");
            }
            set
            {
                base.DataCache.AddChangedProperty("AllowMembersEditMembership", value);
            }
        }
        public bool AllowRequestToJoinLeave
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowRequestToJoinLeave");
            }
            set
            {
                base.DataCache.AddChangedProperty("AllowRequestToJoinLeave", value);
            }
        }
        public bool AutoAcceptRequestToJoinLeave
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AutoAcceptRequestToJoinLeave");
            }
            set
            {
                base.DataCache.AddChangedProperty("AutoAcceptRequestToJoinLeave", value);
            }
        }
        public String Description
        {
            get
            {
                return base.DataCache.GetProperty<string>("Description");
            }
            set
            {
                base.DataCache.AddChangedProperty("Description", value);
            }
        }
        public string DistributionGroupAlias
        {
            get
            {
                return base.DataCache.GetProperty<string>("DistributionGroupAlias");
            }
            set
            {
                base.DataCache.AddChangedProperty("DistributionGroupAlias", value);
            }
        }
        public string DistributionGroupErrorMessage
        {
            get
            {
                return base.DataCache.GetProperty<string>("DistributionGroupErrorMessage");
            }
            set
            {
                base.DataCache.AddChangedProperty("DistributionGroupErrorMessage", value);
            }
        }
        public bool OnlyAllowMembersViewMembership
        {
            get
            {
                return base.DataCache.GetProperty<bool>("OnlyAllowMembersViewMembership");
            }
            set
            {
                base.DataCache.AddChangedProperty("OnlyAllowMembersViewMembership", value);
            }
        }
        public IAveMember Owner
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Owner") && base.DataCache.IsPropertyAvailable("OwnerId"))
                {
                    int memberId = base.DataCache.GetProperty<int>("OwnerId");
                    string type = base.DataCache.GetProperty<string>("OwnerType");
                    if (!string.IsNullOrEmpty(type) && (type.Equals("group") || type.Equals("SharePointGroup")))
                    {
                        base.DataCache.AddProperty("Owner",mWeb.SiteGroups.GetByID(memberId));
                    }
                    else //if (type.Equals("user"))
                    {
                        base.DataCache.AddProperty("Owner",mWeb.SiteUsers.GetByID(memberId));
                    }
                }
                return base.DataCache.GetProperty<IAveMember>("Owner");
            }
            set
            {
                base.DataCache.AddProperty("Owner",value);
                base.DataCache.AddChangedProperty("OwnerId", value.ID);
                base.DataCache.AddChangedProperty("OwnerType", value is IAveUser ? "user" : "group");
                base.DataCache.AddChangedProperty("OwnerLoginName", (value as IAvePrincipal).LoginName);
            }
        }
        public String OwnerTitle
        {
            get
            {
                return base.DataCache.GetProperty<string>("OwnerTitle");
            }
        }
        public string RequestToJoinLeaveEmailSetting
        {
            get
            {
                return base.DataCache.GetProperty<string>("RequestToJoinLeaveEmailSetting");
            }
            set
            {
                base.DataCache.AddChangedProperty("RequestToJoinLeaveEmailSetting", value);
            }
        }

        public void Update()
        {
            if (base.DataCache.ChangedProperties.Count > 0)
            {
                if (base.DataCache.ChangedProperties.ContainsKey("Name"))
                {
                    base.DataCache.ChangedProperties["Title"] = base.DataCache.ChangedProperties["Name"].ToString();//Group没有Name只有Title属性
                }
                Dictionary<string, object> groupProperties = this.mRequest.UpdateGroup(this.mWeb.ServerRelativeUrl, this.ID, base.DataCache.ChangedProperties);
                base.DataCache.UpdateProperties(groupProperties);
            }
        }
        public IAveUserCollection Users
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Users"))
                {
                    IDictionary<string, object> userCollectionProperties = new Dictionary<string, object>();
                    if (base.DataCache.IsPropertyAvailable("Users" + AveObjectModelConstant.ObjectPropertySuffix))
                    {
                        var usersProperties = new List<IDictionary<string, object>>();
                        var usersObj = DataCache.GetPropertyWithoutChange<List<string>>("Users" + AveObjectModelConstant.ObjectPropertySuffix);
                        foreach (string loginName in usersObj)
                        {
                            IDictionary<string, object> userProperties = new Dictionary<string, object>();
                            IAveUser gropUser = mWeb.SiteUsers[loginName];
                            if (gropUser != null) //SAAS-202
                            {
                                userProperties = (gropUser as AveUser).DataCache.GetPropertyCache();
                                usersProperties.Add(userProperties);
                            }
                        }
                        userCollectionProperties.AddChildren(usersProperties);
                    }
                    else
                    {
                        userCollectionProperties = this.mRequest.GetUsers(mWeb.ServerRelativeUrl, this.Name, "group.users");
                    }

                    AveUserCollection users = new AveUserCollection(this.mRequest, this.mWeb, "group.users", this.Name, userCollectionProperties);
                    base.DataCache.AddProperty("Users",users);
                    return users;
                }
                return base.DataCache.GetProperty<IAveUserCollection>("Users");
            }
        }

        public void AddUser(IAveUser user)
        {
            IAveUser tempUser = this.Users.GetByLoginName(user.LoginName);
            if (tempUser == null)
            {
                this.Users.AddUser(user);
                (user as AveUser).DataCache.RemoveProperty("Groups");
            }
        }

        public void RemoveUser(IAveUser user)
        {
            this.Users.RemoveByID(user.ID);
        }

        public string DistributionGroupEmail
        {
            get { throw new NotImplementedException(); }
        }
    }
}
