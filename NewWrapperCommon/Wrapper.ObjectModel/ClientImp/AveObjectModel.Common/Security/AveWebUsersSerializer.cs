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

    internal class AveWebUsersSerializer : IAveUsersSerializer
    {
        private AveWeb m_Web;

        public AveWebUsersSerializer(AveWeb web)
        {
            m_Web = web;
        }

        public List<AveUserInfo> GetObjectData(AveUserBackupOption option)
        {
            List<AveUserInfo> usersInfo = null;
            if (m_Web.HasUniqueRoleAssignments)
            {
                Dictionary<string, IAveUser> userInfoList = new Dictionary<string, IAveUser>();
                if (option.IncludeUsersWithoutSecurity)
                {
                    IAveUserCollection users = m_Web.SiteUsers;
                    foreach (IAveUser user in users)
                    {
                        userInfoList.Add(user.LoginName, user);
                    }
                    usersInfo = GetUserInfo(userInfoList);
                }
                else
                {
                    foreach (IAveUser user in m_Web.Users)
                    {
                        userInfoList.Add(user.LoginName, user);
                    }
                    foreach (IAveGroup group in m_Web.Groups)
                    {
                        foreach (IAveUser user in group.Users)
                        {
                            if (!userInfoList.ContainsKey(user.LoginName))
                            {
                                userInfoList.Add(user.LoginName, user);
                            }
                        }
                    }
                    usersInfo = GetUserInfo(userInfoList);
                }
            }
            return usersInfo;
        }

        private List<AveUserInfo> GetUserInfo(Dictionary<string, IAveUser> users)
        {
            List<AveUserInfo> usersInfoList = new List<AveUserInfo>();
            foreach (KeyValuePair<string, IAveUser> user in users)
            {
                AveUserInfo userInfo = new AveUserInfo();
                userInfo.ID = user.Value.ID;
                userInfo.Email = user.Value.Email;
                userInfo.Login = user.Value.LoginName;
                userInfo.Title = user.Value.Name;
                userInfo.SiteAdmin = user.Value.IsSiteAdmin;
                userInfo.DomainGroup = user.Value.IsDomainGroup;
                usersInfoList.Add(userInfo);
            }
            return usersInfoList;
        }

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
