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
        }

        internal void InitGroupCollection()
        {
            List<Dictionary<string, object>> groupList = base.DataCache.GetChildren();
            mListData = new List<IAveGroup>(groupList.Count);
            foreach (Dictionary<string, object> groupProperties in groupList)
            {
                AveGroup group = new AveGroup(this.mRequest, this.mWeb, groupProperties);
                mListData.Add(group);
            }
        }
        public IAveGroup GetByID(int id)
        {
            return mListData.Find(
                delegate(IAveGroup group)
                {
                    return group.ID.Equals(id);
                });
        }

        private IAveGroup GetByName(string name)
        {
            return mListData.Find(
                     delegate(IAveGroup group)
                     {
                         return group.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
                     });
        }

        public IAveGroup this[string name]
        {
            get
            {
                IAveGroup resultGroup = GetByName(name);
                if (resultGroup == null) 
                {
                    throw new ArgumentException(string.Format("Group {0} does not exist.", name));
                }
                return resultGroup;
            }
        }
        public IAveGroup Add(AveGroupCreationInformation groupCreationInfo)
        {
            Dictionary<string, object> groupProperties = new Dictionary<string, object>();
            AveGroup group = null;
            if (GetByName(groupCreationInfo.Title) == null)
            {
                groupProperties = this.mRequest.AddGroup(this.mWeb.ServerRelativeUrl, null, null, null, groupCreationInfo.Title, groupCreationInfo.Description, this.mGroupSource);
                group = new AveGroup(this.mRequest, this.mWeb, groupProperties);
                mListData.Add(group);
            }
            return group;
        }

        public void Add(List<AveGroupCreationInformation> groupCreationInfos)
        {
            foreach (AveGroupCreationInformation groupCreationInfo in groupCreationInfos)
            {
                Dictionary<string, object> groupProperties = new Dictionary<string, object>();
                AveGroup group = null;
                if (GetByName(groupCreationInfo.Title) == null)
                {
                    groupProperties = this.mRequest.AddGroup(this.mWeb.ServerRelativeUrl, null, null, null, groupCreationInfo.Title, groupCreationInfo.Description, this.mGroupSource);
                    group = new AveGroup(this.mRequest, this.mWeb, groupProperties);
                    mListData.Add(group);
                }
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
                groupProperties = this.mRequest.AddGroup(this.mWeb.ServerRelativeUrl, (owner as IAvePrincipal).LoginName, ownerType, defaultUser == null ? null : defaultUser.LoginName, name, description, this.mGroupSource);
                group = new AveGroup(this.mRequest, this.mWeb, groupProperties);
                mListData.Add(group);
            }
        }
        public void Remove(IAveGroup group)
        {
            this.mRequest.DeleteGroup(this.mWeb.ServerRelativeUrl, group.ID);
            mListData.Remove(group);
        }
        public void Remove(string name)
        {
            IAveGroup group = GetByName(name);
            this.mRequest.DeleteGroup(this.mWeb.ServerRelativeUrl, group.ID);
            mListData.Remove(group);
        }
        public void Remove(int index)
        {
            IAveGroup group = this[index];
            this.mRequest.DeleteGroup(this.mWeb.ServerRelativeUrl, group.ID);
            mListData.Remove(group);
        }

        public void RemoveByID(int id)
        {
            IAveGroup group = this.GetByID(id);
            this.mRequest.DeleteGroup(this.mWeb.ServerRelativeUrl, id);
            mListData.Remove(group);
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
