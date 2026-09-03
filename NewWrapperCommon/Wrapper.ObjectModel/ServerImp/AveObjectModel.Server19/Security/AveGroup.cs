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
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Utilities;
using AvePoint.Common;
using System.Collections.Generic;

namespace AvePoint.ObjectModel.Server19
{
    class AveGroup : AvePrincipal, IAveGroup
    {
        /// <summary>
        /// 请不要给这个赋值，是为了防止重复新建对象使用的
        /// </summary>
        private static SPUserInfo[] emptyUserInfos = new SPUserInfo[0];
        private SPGroup mGroup;
        private AveUserCollection mUsers;
        private AveMember mOwner;

        public AveGroup(AveWeb web, SPGroup group)
            : base(web, group)
        {
            mGroup = group;
        }

        internal SPGroup Group
        {
            get
            {
                return mGroup;
            }
        }

        #region IAveGroup Members

        public string Description
        {
            get
            {
                return mGroup.Description;
            }
            set
            {
                mGroup.Description = value;
            }
        }

        public string OwnerTitle
        {
            get
            {
                return (mGroup.Owner as SPPrincipal).Name;
            }
        }

        public void Update()
        {
            mGroup.Update();
        }

        public IAveUserCollection Users
        {
            get
            {
                if (mUsers == null)
                {
                    mUsers = new AveUserCollection(mWeb, mGroup.Users);
                }
                return mUsers;
            }
        }

        public void AddUser(IAveUser user)
        {
            AveUser tempUser = (AveUser)user;
            if (tempUser != null && tempUser.Principal != null && tempUser.Principal.ID > 0)
            {
                List<SPUser> users = new List<SPUser>();
                users.Add(tempUser.User);
                ((AveUserCollection)this.Users).AddCollection(emptyUserInfos, users);
            }
            else
            {
                this.AddUser(user.LoginName, user.Email, user.Name, user.Notes);
            }
        }

        public void AddUser(string loginName, string email, string name, string notes)
        {
            this.Users.Add(loginName, email, name, notes);
        }

        public bool AllowMembersEditMembership
        {
            get
            {
                return mGroup.AllowMembersEditMembership;
            }
            set
            {
                mGroup.AllowMembersEditMembership = value;
            }
        }

        public bool AllowRequestToJoinLeave
        {
            get
            {
                return mGroup.AllowRequestToJoinLeave;
            }
            set
            {
                mGroup.AllowRequestToJoinLeave = value;
            }
        }

        public bool AutoAcceptRequestToJoinLeave
        {
            get
            {
                return mGroup.AutoAcceptRequestToJoinLeave;
            }
            set
            {
                mGroup.AutoAcceptRequestToJoinLeave = value;
            }
        }

        public string DistributionGroupAlias
        {
            get
            {
                return mGroup.DistributionGroupAlias;
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mGroup, "DistributionGroupAlias", value);
            }
        }

        public string DistributionGroupErrorMessage
        {
            get
            {
                return mGroup.DistributionGroupErrorMessage;
            }
            set
            {
                AveAssemblyUtility.SetPropertyValue(mGroup, "DistributionGroupErrorMessage", value);
            }
        }

        public bool OnlyAllowMembersViewMembership
        {
            get
            {
                return mGroup.OnlyAllowMembersViewMembership;
            }
            set
            {
                mGroup.OnlyAllowMembersViewMembership = value;
            }
        }

        public IAveMember Owner
        {
            get
            {
                SPMember groupOwner = mGroup.Owner;
                return AveMember.InitMember(mWeb, groupOwner);
            }
            set
            {
                if (value != null)
                {
                    mGroup.Owner = (value as AveMember).Member;
                }
                else
                {
                    mGroup.Owner = null;
                }
            }
        }

        public string RequestToJoinLeaveEmailSetting
        {
            get
            {
                return mGroup.RequestToJoinLeaveEmailSetting;
            }
            set
            {
                mGroup.RequestToJoinLeaveEmailSetting = value;
            }
        }

        public override string LoginName
        {
            get
            {
                return mGroup.LoginName;
            }
        }

        public override string Name
        {
            get
            {
                return mGroup.Name;
            }
            set
            {
                mGroup.Name = value;
            }
        }

        public override AvePrincipalType PrincipalType
        {
            get
            {
                return (AvePrincipalType)(SPPrincipalType)(AveAssemblyUtility.GetPropertyValue(mGroup, "PrincipalType"));
            }
        }

        public void RemoveUser(IAveUser user)
        {
            this.Users.RemoveByID(user.ID);
        }

        public string DistributionGroupEmail
        {
            get
            {
                return mGroup.DistributionGroupEmail;
            }
        }

        public void CreateDistributionGroup(string dlAlias)
        {
            mGroup.CreateDistributionGroup(dlAlias);
        }

        public void DeleteDistributionGroup()
        {
            mGroup.DeleteDistributionGroup();
        }
        #endregion
    }
}
