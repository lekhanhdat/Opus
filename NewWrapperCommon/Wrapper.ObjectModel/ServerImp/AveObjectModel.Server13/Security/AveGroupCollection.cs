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



using System.Collections.Generic;
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server13
{
    class AveGroupCollection : AveAbstractCommonCollection<IAveGroup>, IAveGroupCollection
    {
        private SPGroupCollection mGroups;
        private AveWeb mWeb;

        public AveGroupCollection(AveWeb web, SPGroupCollection groups)
            : base(groups)
        {
            mWeb = web;
            mGroups = groups;
        }

        #region IAveGroupCollection Members

        public IAveGroup GetByID(int id)
        {
            return new AveGroup(mWeb, mGroups.GetByID(id));
        }

        public IAveGroup this[string name]
        {
            get
            {
                return new AveGroup(mWeb, mGroups[name]);
            }
        }

        public IAveGroup Add(AveGroupCreationInformation groupCreationInfo)
        {
            string title = groupCreationInfo.Title;
            string description = groupCreationInfo.Description;
            SPMember currentUser = mGroups.Web.CurrentUser;
            mGroups.Add(title, currentUser, null, description);
            return this[title];
        }

        public void Add(List<AveGroupCreationInformation> groupCreationInfos)
        {
            foreach (AveGroupCreationInformation aveGroupInfo in groupCreationInfos)
            {
                this.Add(aveGroupInfo);
            }
        }

        public void Remove(IAveGroup group)
        {
            mGroups.Remove(group.Name);
        }

        public void Add(string name, IAveUser owner, IAveUser defaultUser, string description)
        {
            mGroups.Add(name, (owner as AveUser).User, (defaultUser as AveUser).User, description);
        }

        public void Add(string name, IAveMember owner, IAveUser defaultUser, string description)
        {
            SPMember member = null;
            SPUser user = null;
            if (owner is AveMember)
            {
                AveMember aveMember = owner as AveMember;
                if (aveMember != null)
                {
                    member = aveMember.Member;
                }
            }
            else if (owner is AveGroup)
            {
                AveGroup aveGroup = owner as AveGroup;
                if (aveGroup != null)
                {
                    member = (aveGroup as AveGroup).Group;
                }
            }
            else if (owner is AveUser)
            {
                AveUser aveUser = owner as AveUser;
                if (aveUser != null)
                {
                    member = aveUser.User;
                }
            }
            if (defaultUser is AveUser)
            {
                AveUser aveUser = defaultUser as AveUser;
                if (aveUser != null)
                {
                    user = aveUser.User;
                }
            }
            mGroups.Add(name, member, user, description);
        }

        public void Remove(string name)
        {
            mGroups.Remove(name);
        }

        public void Remove(int index)
        {
            mGroups.Remove(index);
        }

        public override IAveGroup this[int index]
        {
            get
            {
                return new AveGroup(mWeb, mGroups[index]);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveGroup(mWeb, t as SPGroup);
        }

        public override int Count
        {
            get { return mGroups.Count; }
        }

        public void RemoveByID(int id)
        {
            mGroups.RemoveByID(id);
        }

        public IAveWeb Web
        {
            get
            {
                return mWeb;
            }
        }
        #endregion

        public IAveGroupCollection GetCollection(string[] names)
        {
            return new AveGroupCollection(mWeb, mGroups.GetCollection(names));
        }

        public IAveGroupCollection GetCollection(int[] groupIds)
        {
            return new AveGroupCollection(mWeb, mGroups.GetCollection(groupIds));
        }
    }
}
