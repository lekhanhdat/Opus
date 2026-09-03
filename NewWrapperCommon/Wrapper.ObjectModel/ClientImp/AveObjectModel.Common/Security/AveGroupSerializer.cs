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



namespace AvePoint.ObjectModel.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common;
    #endregion

    internal class AveGroupSerializer : IAveGroupSerializer
    {
        private AveSite m_Site;

        public AveGroupSerializer(AveSite site)
        {
            m_Site = site;
        }

        public AveGroupInfo GetObjectData(int principalId)
        {
            foreach (AveGroup group in m_Site.RootWeb.SiteGroups)
            {
                if (group.ID == principalId)
                {
                    AveGroupInfo groupInfo = new AveGroupInfo();
                    groupInfo.Description = group.Description;
                    groupInfo.ID = group.ID;
                    groupInfo.Title = group.Name;
                    groupInfo.OnlyAllowMembersViewMembership = group.OnlyAllowMembersViewMembership;
                    groupInfo.AllowMembersEditMembership = group.AllowMembersEditMembership;
                    groupInfo.AllowRequestToJoinLeave = group.AllowRequestToJoinLeave;
                    groupInfo.AutoAcceptRequestToJoinLeave = group.AutoAcceptRequestToJoinLeave;
                    groupInfo.Owner = group.Owner.ID;
                    groupInfo.OwnerIsUser = group.Owner is IAveUser;
                    foreach (IAveUser user in group.Users)
                    {
                        AveUserInfo userInfo = new AveUserInfo();
                        userInfo.Email = user.Email;
                        userInfo.ID = user.ID;
                        userInfo.Login = user.LoginName;
                        userInfo.Title = user.Name;
                        userInfo.DomainGroup = user.IsDomainGroup;
                        groupInfo.Members.Add(userInfo);
                        groupInfo.Memberships.Add(userInfo.ID);
                    }
                    return groupInfo;
                }
            }
            return null;
        }

        public AveGroupInfo GetObjectData()
        {
            throw new NotImplementedException();
        }

        public object SetObjectData(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
