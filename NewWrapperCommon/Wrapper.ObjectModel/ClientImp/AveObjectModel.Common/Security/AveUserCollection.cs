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

        public AveUserCollection(IAveRequest request, AveWeb parentweb, string source, string groupName, Dictionary<string, object> userColProperties)
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
            List<Dictionary<string, object>> userPropertiesList = base.DataCache.GetChildren();
            mListData = new List<IAveUser>(userPropertiesList.Count);
            foreach (Dictionary<string, object> userProperties in userPropertiesList)
            {
                AveUser user = new AveUser(mRequest, mParentWeb, mSource, userProperties);
                mListData.Add(user);
            }
        }

        #region IAveUserCollection-members
        
        public IAveUser Add(AveUserCreationInformation userCreationInfo)
        {
            return this.Add(userCreationInfo.LoginName, userCreationInfo.Email, userCreationInfo.Title, string.Empty);
        }

        public void Add(AveUserCreationInformation[] userCreationInfos)
        {
            foreach( AveUserCreationInformation createInfo in userCreationInfos ) 
            {
                this.Add(createInfo);
            }
        }

        public IAveUser Add(string loginName, string email, string name, string notes)
        {
            Dictionary<string, object> userProperties = new Dictionary<string, object>();
            userProperties.Add("LoginName", loginName);
            userProperties.Add("Email", email);
            userProperties.Add("Name", name);
            userProperties.Add("Notes", notes);
            userProperties.Add("ID", -1); //暂时先占位，如果有需求或效率问题，再修改接口，穿ID 进来
            Dictionary<string, object> newUserProp = this.mRequest.AddUser(mParentWeb.ServerRelativeUrl, mSource, mGroupName, userProperties);
            AveUser user = new AveUser(mRequest, this.mParentWeb, mSource, newUserProp);
            mListData.Add(user);
            return user;
        }

        public void Add(AveUser user)
        {
            this.Add(user.LoginName, user.Email, user.Name, user.Notes);
        }

        public IAveUser GetByLoginName(string loginName)
        {
            if (string.IsNullOrEmpty(loginName))
            {
                return null;
            }
            string searchLoginName = loginName;
            if (!loginName.Contains("|") && loginName.Contains(":"))
            {
                int index = loginName.IndexOf(':');
                searchLoginName = loginName.Substring(index + 1);
            }
            IAveUser user = null;
            user = mListData.Find(delegate(IAveUser u)
                {
                    return u.LoginName.Equals(searchLoginName, StringComparison.OrdinalIgnoreCase) ||
                           u.NoPrefixLoginName.Equals(searchLoginName, StringComparison.OrdinalIgnoreCase);
                });
            return user;
        }

        public IAveUser GetByID(int id)
        {
            return mListData.Find(
                delegate(IAveUser user) 
                {
                    return id == user.ID;
                });
        }

        public void Remove(IAveUser user)
        {
            if( mListData.Contains(user) )
            {
                this.mRequest.DeleteUser(mParentWeb.ServerRelativeUrl, mSource, mGroupName, user.LoginName);
                mListData.Remove(user);
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
                IAveUser user = GetByLoginName(loginName);
                if (user == null) 
                {
                    throw new ArgumentException(string.Format("User:{0} does not exist.", loginName));
                }
                return user;
            }
        }

        public void RemoveByID(int id)
        {
            Remove(this.GetByID(id));
        }

        public void AddOrRemoveUserInCache(IAveUser user, bool add)
        {
            if (add)
            {
                mListData.Add(user);
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
                mListData.Remove(user);
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