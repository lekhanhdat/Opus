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
using AvePoint.ObjectModel.WebService;
using System.Collections;

namespace AvePoint.ObjectModel.Common
{
    class AveUserCollection : AveAbstractCommonCollection<IAveUser>, IAveUserCollection
    {
        private IAveRequest mRequest;
        private AveWeb mParentWeb;
        private string mSource;
        private string mGroupName;
        private Dictionary<string, int> loginNameIndex;
        private Dictionary<int, AveUser> loginIdIndex;
        private Dictionary<string, int> domainGroupNameIndex;
        private Dictionary<string, int> noPrefixArchiverLoginNameIndex;
        private readonly object loginNameIndexLock = new object();
        private readonly object noPrefixArchiverLoginNameIndexLock = new object();
        private readonly object domainGroupNameIndexLock = new object();

        public AveUserCollection(IAveRequest request, AveWeb parentweb, string source, string groupName, IDictionary<string, object> userColProperties)
        {
            mRequest = request;
            mParentWeb = parentweb;
            mSource = source;
            mGroupName = groupName;
            base.DataCache.AddPropertyies(userColProperties);
            InitUserCollection();
        }

        internal void InitUserCollection()
        {
            var userPropertiesList = base.DataCache.GetChildren();
            mListData = new List<IAveUser>(userPropertiesList.Count);
            loginNameIndex = new Dictionary<string, int>(userPropertiesList.Count, StringComparer.OrdinalIgnoreCase);
            noPrefixArchiverLoginNameIndex = new Dictionary<string, int>(userPropertiesList.Count, StringComparer.OrdinalIgnoreCase);
            loginIdIndex = new Dictionary<int, AveUser>(userPropertiesList.Count);
            domainGroupNameIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var userProperties in userPropertiesList)
            {
                AveUser user = new AveUser(mRequest, mParentWeb, mSource, userProperties);
                mListData.Add(user);
                loginNameIndex[user.LoginName] = user.ID;
                noPrefixArchiverLoginNameIndex[user.NoPrefixLoginNameForArchiver] = user.ID;
                loginIdIndex[user.ID] = user;
                if (user.PrincipalType == AvePrincipalType.SecurityGroup && !string.IsNullOrEmpty(user.Name))
                {
                    domainGroupNameIndex[user.Name] = user.ID;
                }
            }
        }

        #region IAveUserCollection-members

        public IAveUser Add(AveUserCreationInformation userCreationInfo)
        {
            return this.Add(userCreationInfo.LoginName, userCreationInfo.Email, userCreationInfo.Title, string.Empty,-1);
        }

        public void Add(AveUserCreationInformation[] userCreationInfos)
        {
            foreach (AveUserCreationInformation createInfo in userCreationInfos)
            {
                this.Add(createInfo);
            }
        }

        public IAveUser AddUser(IAveUser user)
        {
            return Add(user.LoginName, user.Email, user.Name, user.Notes,user.ID);
        }

        public IAveUser Add(string loginName, string email, string name, string notes)
        {
            return Add(loginName, email, name, notes,-1);
        }

        private IAveUser Add(string loginName, string email, string name, string notes,int id)
        {
            Dictionary<string, object> userProperties = new Dictionary<string, object>
            {
                { "LoginName", loginName },
                { "Email", email },
                { "Name", name },
                { "Notes", notes },
                { "ID", id }
            };
            Dictionary<string, object> newUserProp = this.mRequest.AddUser(mParentWeb.ServerRelativeUrl, mSource, mGroupName, userProperties);
            AveUser user = new AveUser(mRequest, this.mParentWeb, mSource, newUserProp);
            lock (loginNameIndexLock)
            {
                mListData.Add(user);
                loginNameIndex[user.LoginName] = user.ID;
                noPrefixArchiverLoginNameIndex[user.NoPrefixLoginNameForArchiver] = user.ID;
                loginIdIndex[user.ID] = user;
                if (user.PrincipalType == AvePrincipalType.SecurityGroup && !string.IsNullOrEmpty(user.Name))
                {
                    domainGroupNameIndex[user.Name] = user.ID;
                }
            }
            return user;
        }

        public IAveUser GetByLoginName(string loginName)
        {
            if (!string.IsNullOrEmpty(loginName))
            {
                lock (loginNameIndexLock)
                {
                    IAveUser user = null;
                    int id;
                    if (loginNameIndex.TryGetValue(loginName, out id))
                    {
                        user = loginIdIndex[id];
                    }
                    //user = mListData.Find(delegate(IAveUser u)
                    //    {
                    //        return u.LoginName.Equals(loginName, StringComparison.OrdinalIgnoreCase);
                    //    });
                    //if (user == null)
                    //{
                    //    user = mListData.Find(delegate(IAveUser u)
                    //    {
                    //        return u.Name.Equals(loginName, StringComparison.OrdinalIgnoreCase);
                    //    });
                    //}
                    return user;
                }
            }

            return null;
        }

        public IAveUser GetByEmail(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                lock (loginNameIndexLock)
                {
                    IAveUser user = null;
                    int id;
                    foreach(var userDic in loginNameIndex)
                    {
                        if (userDic.Key.IndexOf(email, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            id = userDic.Value;
                            user = loginIdIndex[id];
                        }
                    }
                    return user;
                }
            }
            return null;
        }

        /// <summary>
        /// only used for cloud archiver.
        /// </summary>
        public IAveUser GetByUPNName(string upn)
        {
            if (!string.IsNullOrEmpty(upn))
            {
                lock (noPrefixArchiverLoginNameIndexLock)
                {
                    IAveUser user = null;
                    int id;
                    foreach (var userDic in noPrefixArchiverLoginNameIndex)
                    {
                        if (userDic.Key.EqualIgnoreCase(upn))
                        {
                            id = userDic.Value;
                            user = loginIdIndex[id];
                        }
                    }
                    return user;
                }
            }
            else
            {
                return null;
            }
        }

        public IAveUser GetDomainGroupByName(string groupName)
        {
            if (!string.IsNullOrEmpty(groupName))
            {
                lock (domainGroupNameIndexLock)
                {
                    int id;
                    if (domainGroupNameIndex.TryGetValue(groupName, out id))
                    {
                        return loginIdIndex[id];
                    }
                }
            }
            return null;
        }

        public IAveUser GetByID(int id)
        {
            lock (loginNameIndexLock)
            {
                AveUser user;
                if (!loginIdIndex.TryGetValue(id, out user))
                {
                    user = null;
                }

                return user;
            }
            //return mListData.Find(
            //    delegate(IAveUser user) 
            //    {
            //        return id == user.ID;
            //    });
        }

        public void Remove(IAveUser user)
        {
            if (mListData.Contains(user))
            {
                this.mRequest.DeleteUser(mParentWeb.ServerRelativeUrl, mSource, mGroupName, user.LoginName);
                lock (loginNameIndexLock)
                {
                    mListData.Remove(user);
                    loginIdIndex.Remove(user.ID);
                    loginNameIndex.Remove(user.LoginName);
                    noPrefixArchiverLoginNameIndex.Remove(user.NoPrefixLoginNameForArchiver);
                    if (user.PrincipalType == AvePrincipalType.SecurityGroup && !string.IsNullOrEmpty(user.Name))
                    {
                        domainGroupNameIndex.Remove(user.Name);
                    }
                }
            }
        }

        public void Remove(List<IAveUser> users)
        {
            List<string> loginNames = new List<string>();
            foreach (IAveUser user in users)
            {
                loginNames.Add(user.LoginName);
            }
            this.mRequest.DeleteUsers(mParentWeb.ServerRelativeUrl, mSource, mGroupName, loginNames);
            foreach (IAveUser user in users)
            {
                if (mListData.Contains(user))
                {
                    lock (loginNameIndexLock)
                    {
                        mListData.Remove(user);
                        loginIdIndex.Remove(user.ID);
                        loginNameIndex.Remove(user.LoginName);
                        noPrefixArchiverLoginNameIndex.Remove(user.NoPrefixLoginNameForArchiver);
                        if (user.PrincipalType == AvePrincipalType.SecurityGroup && !string.IsNullOrEmpty(user.Name))
                        {
                            domainGroupNameIndex.Remove(user.Name);
                        }
                    }
                }
            }
        }

        public void Remove(string loginName)
        {
            Remove(this.GetByLoginName(loginName));
        }

        public IAveUser this[string loginName]
        {
            get
            {
                return GetByLoginName(loginName);
            }
        }

        public void RemoveByID(int id)
        {
            Remove(this.GetByID(id));
        }

        public void RemoveByIDs(List<int> ids)
        {
            List<IAveUser> needRemovedUsers = new List<IAveUser>();
            foreach (int id in ids)
            {
                needRemovedUsers.Add(this.GetByID(id));
            }
            Remove(needRemovedUsers);
        }

        public void AddOrRemoveUserInCache(IAveUser user, bool add)
        {
            if (add)
            {
                if (!mListData.Contains(user))//防止陷入死循环
                {
                    lock (loginNameIndexLock)
                    {
                        mListData.Add(user);
                        loginIdIndex[user.ID] = user as AveUser;
                        loginNameIndex[user.LoginName] = user.ID;
                        noPrefixArchiverLoginNameIndex[user.NoPrefixLoginNameForArchiver] = user.ID;
                        //使用PrincipalType判断是否为Domaingroup，因为GetEnsureUser方法返回值中不包含Domaingroup属性，new的user实例Domaingroup属性默认为false。
                        if (user.PrincipalType == AvePrincipalType.SecurityGroup && !string.IsNullOrEmpty(user.Name))
                        {
                            domainGroupNameIndex[user.Name] = user.ID;
                        }
                    }
                }
            }
            else
            {
                this.RemoveInCache(user.LoginName);
            }
        }
        private void RemoveInCache(string loginName)
        {
            IAveUser user = this.GetByLoginName(loginName);
            if (user != null)
            {
                lock (loginNameIndexLock)
                {
                    mListData.Remove(user);
                    loginIdIndex.Remove(user.ID);
                    loginNameIndex.Remove(user.LoginName);
                    noPrefixArchiverLoginNameIndex.Remove(user.NoPrefixLoginNameForArchiver);
                    if (user.PrincipalType == AvePrincipalType.SecurityGroup && !string.IsNullOrEmpty(user.Name))
                    {
                        domainGroupNameIndex.Remove(user.Name);
                    }
                }
            }
        }


        #endregion



        public IAveWeb Web
        {
            get { throw new NotImplementedException(); }
        }

        public IAveGroup WithinGroup
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}