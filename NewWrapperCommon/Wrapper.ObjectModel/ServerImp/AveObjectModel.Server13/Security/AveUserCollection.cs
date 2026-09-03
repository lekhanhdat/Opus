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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using System.Reflection;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.Server13
{
    class AveUserCollection : AveAbstractCommonCollection<IAveUser>, IAveUserCollection
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private SPUserCollection mUsers;
        private AveWeb mWeb;
        private AveGroup mWithinGroup;

        public AveUserCollection(AveWeb web, SPUserCollection users)
            : base(users)
        {
            mWeb = web;
            mUsers = users;
        }

        #region IAveUserCollection Members

        public IAveUser Add(AveUserCreationInformation userCreationInfo)
        {
            mUsers.Add(userCreationInfo.LoginName, userCreationInfo.Email, userCreationInfo.Title, null);
            SPUser user = mUsers[userCreationInfo.LoginName];
            if (user == null)
            {
                return null;
            }
            return new AveUser(mWeb, user);
        }

        public void Add(AveUserCreationInformation[] userCreationInfos)
        {
            SPUserInfo[] userInfos = new SPUserInfo[userCreationInfos.Length];
            for (int i = 0; i < userInfos.Length; i++)
            {
                userInfos[i].Email = userCreationInfos[i].Email;
                userInfos[i].LoginName = userCreationInfos[i].LoginName;
                userInfos[i].Name = userCreationInfos[i].Title;
                userInfos[i].Notes = userCreationInfos[i].Notes;
            }
            mUsers.AddCollection(userInfos);
        }

        public IAveUser GetByLoginName(string loginName)
        {
            foreach (SPUser user in mUsers)
            {
                if (string.Equals(loginName, user.LoginName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return new AveUser(mWeb, user);
                }
            }
            return null;
        }

        public void Remove(IAveUser user)
        {
            mUsers.Remove(user.LoginName);
        }

        public IAveUser this[string name]
        {
            get
            {
                return new AveUser(mWeb, mUsers[name]);
            }
        }

        public IAveUser Add(string loginName, string email, string name, string notes)
        {
            mUsers.Add(loginName, email, name, notes);
            return new AveUser(mWeb, mUsers[loginName]);
        }

        public void AddCollection(SPUserInfo[] addUsersInfo, IEnumerable<SPUser> addUsers)
        {
            try
            {
                AveAssemblyUtility.InvokeMethod(mUsers, mUsers.GetType(), "AddCollection", new Type[] { typeof(SPUserInfo[]), typeof(IEnumerable<SPUser>) }, new object[] { addUsersInfo, addUsers }, true);
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, ServerAPIResource.AddUsrToCollectionError, e.ToString());
                if (addUsersInfo != null)
                {
                    this.mUsers.AddCollection(addUsersInfo);
                }
                if (addUsers != null)
                {
                    List<SPUserInfo> userInfos = new List<SPUserInfo>();
                    foreach (SPUser user in addUsers)
                    {
                        SPUserInfo info = new SPUserInfo();
                        info.Name = user.Name;
                        info.LoginName = user.LoginName;
                        info.Email = user.Email;
                        info.Notes = user.Notes;
                        userInfos.Add(info);
                    }
                    this.mUsers.AddCollection(userInfos.ToArray());
                }
            }

        }

        public IAveUser GetByID(int id)
        {
            return new AveUser(mWeb, mUsers.GetByID(id));
        }

        public void Remove(string loginName)
        {
            mUsers.Remove(loginName);
        }

        public override IAveUser this[int index]
        {
            get
            {
                return new AveUser(mWeb, mUsers[index]);
            }
        }

        public void RemoveByID(int id)
        {
            mUsers.RemoveByID(id);
        }

        public IAveWeb Web
        {
            get
            {
                return mWeb;
            }
        }

        internal SPUserCollection Users
        {
            get
            {
                return mUsers;
            }
        }

        public IAveGroup WithinGroup
        {
            get
            {
                if (mWithinGroup == null)
                {
                    SPGroup group = (SPGroup)AveAssemblyUtility.GetPropertyValue(mUsers, "WithinGroup");
                    if (group != null)
                    {
                        mWithinGroup = new AveGroup(mWeb, group);
                    }
                }
                return mWithinGroup;
            }
        }

        public void AddOrRemoveUserInCache(IAveUser user, bool add)
        {
            throw new NotImplementedException();
        }

        #endregion

        protected override object CreatElementInstance(object t)
        {
            return new AveUser(mWeb, t as SPUser);
        }

        public override int Count
        {
            get { return mUsers.Count; }
        }
    }
}
