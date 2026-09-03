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
    class AveGroupCollection : AveAbstractCommonCollection<IAveGroup>, IAveGroupCollection
    {
        private AveWeb mWeb;
        private IAveRequest mRequest;
        private string mGroupSource;
        private Dictionary<string, int> loginNameIndex;
        private Dictionary<int, IAveGroup> loginIdIndex;

        public AveGroupCollection(AveWeb web, IAveRequest request, string groupSource, Dictionary<string, object> groupColProperties)
        {
            mRequest = request;
            mWeb = web;
            mGroupSource = groupSource;
            base.DataCache.AddPropertyies(groupColProperties);
            InitGroupCollection();
        }

        public AveGroupCollection(AveWeb web, IAveRequest request, List<IAveGroup> groups)
        {
            mRequest = request;
            mWeb = web;
            mListData = groups;

            loginNameIndex = new Dictionary<string, int>(mListData.Count, StringComparer.OrdinalIgnoreCase);
            loginIdIndex = new Dictionary<int, IAveGroup>(mListData.Count);
            foreach (var group in mListData)
            {
                loginNameIndex[group.Name] = group.ID;
                loginIdIndex[group.ID] = group;
            }
        }

        internal void InitGroupCollection()
        {
            var groupList = base.DataCache.GetChildren();
            mListData = new List<IAveGroup>(groupList.Count);
            loginNameIndex = new Dictionary<string, int>(groupList.Count, StringComparer.OrdinalIgnoreCase);
            loginIdIndex = new Dictionary<int, IAveGroup>(groupList.Count);
            foreach (var groupProperties in groupList)
            {
                AveGroup group = new AveGroup(this.mRequest, this.mWeb, groupProperties);
                mListData.Add(group);
                loginNameIndex[group.Name] = group.ID;
                loginIdIndex[group.ID] = group;
            }
        }
        public IAveGroup GetByID(int id)
        {
            lock (loginNameIndex)
            {
                IAveGroup group;
                if (!loginIdIndex.TryGetValue(id, out group))
                {
                    group = null;
                }

                return group;
            }
            //return mListData.Find(
            //    delegate(IAveGroup group)
            //    {
            //        return group.ID.Equals(id);
            //    }
            //    );
        }

        private IAveGroup GetByName(string name)
        {
            lock (loginNameIndex)
            {
                IAveGroup group = null;
                int id;
                if (loginNameIndex.TryGetValue(name, out id))
                {
                    group = loginIdIndex[id];
                }
                return group;
            }
            //return mListData.Find(
            //         delegate(IAveGroup group)
            //         {
            //             return group.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
            //         });
        }

        public IAveGroup this[string name]
        {
            get
            {
                return GetByName(name);
            }
        }
        public IAveGroup Add(AveGroupCreationInformation groupCreationInfo)
        {
            Dictionary<string, object> groupProperties = new Dictionary<string, object>();
            AveGroup group = null;
            if (this[groupCreationInfo.Title]==null)
            {
                groupProperties = this.mRequest.AddGroup(this.mWeb.ServerRelativeUrl, null, null, null, groupCreationInfo.Title, groupCreationInfo.Description, this.mGroupSource);
                group = new AveGroup(this.mRequest, this.mWeb, groupProperties);
                lock (loginNameIndex)
                {
                    mListData.Add(group);
                    loginNameIndex[group.Name] = group.ID;
                    loginIdIndex[group.ID] = group;
                }
            }           
            return group;
        }

        public void Add(List<AveGroupCreationInformation> groupCreationInfos)
        {
            foreach (AveGroupCreationInformation groupCreationInfo in groupCreationInfos)
            {
                Add(groupCreationInfo);
                //Dictionary<string, object> groupProperties = new Dictionary<string, object>();
                //AveGroup group = null;
                //if (this[groupCreationInfo.Title] == null)
                //{
                //    groupProperties = this.mRequest.AddGroup(this.mWeb.ServerRelativeUrl, null, null, null, groupCreationInfo.Title, groupCreationInfo.Description, this.mGroupSource);
                //    group = new AveGroup(this.mRequest, this.mWeb, groupProperties);
                //    mListData.Add(group);
                //}
            }
        }
        public void Add(string name, IAveMember owner, IAveUser defaultUser, string description)
        {
            owner = owner == null ? mWeb.CurrentUser : owner;
            string ownerType = owner is IAveUser ? "User" : "Group";
            Dictionary<string, object> groupProperties = new Dictionary<string, object>();
            AveGroup group = null;
            if (GetByName(name) == null)
            {
                string defaultLoginName = ownerType.Equals("Group") ? mWeb.CurrentUser.LoginName : (defaultUser == null ? null : defaultUser.LoginName);
                groupProperties = this.mRequest.AddGroup(this.mWeb.ServerRelativeUrl, (owner as IAvePrincipal).LoginName, ownerType, defaultLoginName, name, description, this.mGroupSource);
                group = new AveGroup(this.mRequest, this.mWeb, groupProperties);
                //mListData.Add(group);
                lock (loginNameIndex)
                {
                    mListData.Add(group);
                    loginNameIndex[group.Name] = group.ID;
                    loginIdIndex[group.ID] = group;
                }
            }
        }
        public void Remove(IAveGroup group)
        {
            this.mRequest.DeleteGroup(this.mWeb.ServerRelativeUrl, group.ID);
            lock (loginNameIndex)
            {
                mListData.Remove(group);
                loginNameIndex.Remove(group.Name);
                loginIdIndex.Remove(group.ID);
            }
        }
        public void Remove(string name)
        {
            IAveGroup group = this[name];
            this.mRequest.DeleteGroup(this.mWeb.ServerRelativeUrl, group.ID);
            lock (loginNameIndex)
            {
                mListData.Remove(group);
                loginNameIndex.Remove(group.Name);
                loginIdIndex.Remove(group.ID);
            }
        }
        public void Remove(int index)
        {
            IAveGroup group = this[index];
            this.mRequest.DeleteGroup(this.mWeb.ServerRelativeUrl, group.ID);
            lock (loginNameIndex)
            {
                mListData.Remove(group);
                loginNameIndex.Remove(group.Name);
                loginIdIndex.Remove(group.ID);
            }
        }

        public void RemoveByID(int id)
        {
            IAveGroup group = this.GetByID(id);
            this.mRequest.DeleteGroup(this.mWeb.ServerRelativeUrl, id);
            lock (loginNameIndex)
            {
                mListData.Remove(group);
                loginNameIndex.Remove(group.Name);
                loginIdIndex.Remove(group.ID);
            }
        }

        public IAveWeb Web
        {
            get { return mWeb; }
        }

        public IAveGroupCollection GetCollection(string[] names)
        {
            throw new NotImplementedException();
        }

        public IAveGroupCollection GetCollection(int[] groupIds)
        {
            List<IAveGroup> groups = new List<IAveGroup>();
            foreach (IAveGroup group in mListData)
            {
                if (groupIds.Contains(group.ID))
                {
                    groups.Add(group);
                }
            }
            return new AveGroupCollection(mWeb, mRequest, groups);

        }
    }
}
