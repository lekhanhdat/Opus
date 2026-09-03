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
    class AveUser : AvePrincipal, IAveUser
    {
        private IAveRequest mRequest;
        private AveWeb mParentWeb;
        private string mSource;

        public AveUser(IAveRequest request, AveWeb parentWeb, string source, IDictionary<string, object> properties)
        {
            mRequest = request;
            mParentWeb = parentWeb;
            mSource = source;
            properties["ParentWeb"] = parentWeb;
            base.DataCache.AddPropertyies(properties);
        }

        public String Email
        {
            get 
            {
                return base.DataCache.GetProperty<String>("Email");
            }
            set 
            {
                base.DataCache.AddChangedProperty("Email", value);
            }
        }

        public bool IsDomainGroup 
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsDomainGroup");
            }
        }
        public bool IsSiteAdmin 
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsSiteAdmin");
            } 
            set
            {
                var newAdmins = new List<IDictionary<string, object>>();
                StringBuilder oldAdmins = new StringBuilder();
                foreach (IAveUser user in this.mParentWeb.SiteAdministrators)
                {
                    oldAdmins.Append(user.LoginName.ToLower());
                    oldAdmins.Append(",");
                    if (!value && string.Equals(user.LoginName, this.LoginName))
                    {
                        continue;
                    }
                    else
                    {
                        Dictionary<string, object> userProp = new Dictionary<string, object>();
                        userProp["LoginName"] = user.LoginName;
                        userProp["Name"] = user.Name;
                        userProp["ID"] = user.ID;
                        newAdmins.Add(userProp);
                    }
                }
                base.DataCache.AddChangedProperty("IsSiteAdmin", value);
                if (value)
                {
                    Dictionary<string, object> userProp = new Dictionary<string, object>();
                    userProp["LoginName"] = this.LoginName;
                    userProp["Name"] = this.Name;
                    userProp["ID"] = this.ID;
                    newAdmins.Add(userProp);
                }
                base.DataCache.AddChangedProperty("OldAdministrators", oldAdmins.ToString().TrimEnd(','));
                base.DataCache.AddChangedProperty("NewAdministrators", newAdmins);
            }
        }
        public string Notes
        {
            get
            {
                return base.DataCache.GetProperty<string>("Notes");
            }
            set
            {
                base.DataCache.AddChangedProperty("Notes", value);
            }
        }

        public string NoPrefixLoginName
        {
            get
            {
                if (this.LoginName.IndexOf('|') > 0)
                {
                    return this.LoginName.Substring(this.LoginName.IndexOf('|') + 1);
                }
                else
                {
                    return this.LoginName;
                }
            }
        }

        public string NoPrefixLoginNameForArchiver
        {
            get
            {
                if (this.LoginName.LastIndexOf('|') > 0)
                {
                    return this.LoginName.Substring(this.LoginName.LastIndexOf('|') + 1);
                }
                else
                {
                    return this.LoginName;
                }
            }
        }

        public IAveRegionalSettings RegionalSettings
        { 
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("RegionalSettings") && base.DataCache.IsPropertyAvailable("RegionalSettings" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    Dictionary<string, object> regionalSettingsProperties = base.DataCache.GetProperty<Dictionary<string, object>>("RegionalSettings" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveRegionalSettings regionalSettings = new AveRegionalSettings(this, mRequest, regionalSettingsProperties);
                    base.DataCache.AddProperty("RegionalSettings",regionalSettings);
                    return regionalSettings;
                }
                return base.DataCache.GetProperty<IAveRegionalSettings>("RegionalSettings");
            }
            set
            {
                base.DataCache.AddProperty("RegionalSettings",value);
                base.DataCache.AddChangedProperty("RegionalSettingsChangedProperties", (value as AveClientObject).DataCache.ChangedProperties);
            } 
        }
        public object SPUser 
        { 
            get
            {
                return base.DataCache.GetProperty<object>("SPUser");
            }
        }
        public IAveUserToken UserToken 
        {
            get
            {
                return base.DataCache.GetProperty<IAveUserToken>("UserToken");
            }
        }

        public IAveAlertCollection Alerts 
        {
            get
            {
                throw new NotFiniteNumberException();
            }
        }

        public void Update()
        {
            Dictionary<string, object> newProp = this.mRequest.UpdateUser(this.mParentWeb.ServerRelativeUrl, this.LoginName, this.Name, mSource, base.DataCache.ChangedProperties);
            if (base.DataCache.ChangedProperties.ContainsKey("IsSiteAdmin"))
            {
                base.DataCache.UpdateProperties(newProp);
                if (!this.IsSiteAdmin)
                {
                    this.mParentWeb.SiteAdministrators.AddOrRemoveUserInCache(this, false);
                }
                else
                {
                    this.mParentWeb.SiteAdministrators.AddOrRemoveUserInCache(this, true);
                }
            }
            else
            {
                base.DataCache.UpdateProperties(newProp);
            }
        }

        public IAveGroupCollection Groups
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Groups"))
                {
                    Dictionary<string, object> groupCollection = this.mRequest.GetGroups(this.mParentWeb.ServerRelativeUrl, "user.groups", this.LoginName);
                    AveGroupCollection groups = new AveGroupCollection(this.mParentWeb, this.mRequest, "user.groups", groupCollection);
                    base.DataCache.AddProperty("Groups",groups);
                    return groups;
                }
                return base.DataCache.GetProperty<IAveGroupCollection>("Groups");
            }
        }

        public IAveGroupCollection OwnedGroups
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsShareByEmailGuestUser { get { return base.DataCache.GetProperty<bool>("IsShareByEmailGuestUser"); } }
    }
}
