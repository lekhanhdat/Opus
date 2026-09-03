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

    internal class AveSiteUsersSerializer : IAveUsersSerializer
    {
        private AveSite m_Site;

        public AveSiteUsersSerializer(AveSite site)
        {
            m_Site = site;
        }

        public List<AveUserInfo> GetObjectData(bool allAvailableUser)
        {
            List<AveUserInfo> userInfoList = new List<AveUserInfo>();
            foreach (AveUser user in m_Site.RootWeb.SiteUsers)
            {
                AveUserInfo userInfo = new AveUserInfo();
                userInfo.ID = user.ID;
                /*if ((!user.IsSiteAdmin) && (!allAvailableUser) && (!CheckUserHasPermission(userInfo.ID)))
                {
                    //check user permission if user is not site admini and do not get none permission user
                    continue;
                }*/
                userInfo.Email = user.Email;
                userInfo.Login = user.LoginName;
                userInfo.Title = user.Name;
                userInfo.SiteAdmin = user.IsSiteAdmin;
                userInfo.Notes = user.Notes;
                userInfo.DomainGroup = user.IsDomainGroup;
                userInfoList.Add(userInfo);
            }
            return userInfoList;
        }

        #region filter user has no permission
/*        private bool CheckUserHasPermission(int userId)
        {
            foreach(IAveRoleAssignment roleAssignment in m_Site.RootWeb.RoleAssignments)
            {
                if (roleAssignment.Member.ID.Equals(userId))
                {
                    return true;
                }
            }
            return false;
        }*/
        #endregion

        public List<AveUserInfo> GetObjectData()
        {
            throw new NotImplementedException();
        }

        public object SetObjectData(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
