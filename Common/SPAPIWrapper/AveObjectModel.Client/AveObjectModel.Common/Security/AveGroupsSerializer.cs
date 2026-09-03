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
    using AvePoint.GCommon;
    using System.Reflection;
    #endregion

    internal class AveGroupsSerializer : IAveGroupsSerializer
    {
        public static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private AveWeb m_Web;

        public AveGroupsSerializer(AveWeb web)
        {
            m_Web = web;
        }

        public List<AveGroupInfo> GetObjectData(bool allGroups)
        {
            List<AveGroupInfo> groupsInfo = null;
            IAveGroupCollection groups = allGroups ? m_Web.SiteGroups : m_Web.Groups;
            if (groups.Count > 0)
            {
                groupsInfo = new List<AveGroupInfo>(groups.Count);
            }
            foreach (IAveGroup group in groups)
            {
                try
                {
                    AveGroupInfo groupInfo = new AveGroupInfo();
                    groupInfo.Description = group.Description;
                    groupInfo.ID = group.ID;
                    if (!allGroups && (!CheckGroupHasPermission(groupInfo.ID)))
                    {
                        continue;
                    }
                    groupInfo.Title = group.Name;
                    //添加诊断log,判断空引用的地方SAAS-28834
                    if (null == group.Owner)
                    {
                        mLog.Warn("The group's owner is null,{0}", group.Name);
                    }
                    else
                    {
                        groupInfo.Owner = group.Owner.ID;
                        groupInfo.OwnerIsUser = group.Owner is IAveUser;
                    }
                    //SAAS-8191 增加Group Settings中的四个属性
                    groupInfo.AllowMembersEditMembership = group.AllowMembersEditMembership;
                    groupInfo.AllowRequestToJoinLeave = group.AllowRequestToJoinLeave;
                    groupInfo.AutoAcceptRequestToJoinLeave = group.AutoAcceptRequestToJoinLeave;
                    groupInfo.OnlyAllowMembersViewMembership = group.OnlyAllowMembersViewMembership;
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
                    ArgumentCheck.CheckNotNull(groupsInfo);
                    groupsInfo?.Add(groupInfo);
                }
                catch (Exception e)
                {
                    //SAAS-37894
                    mLog.Error($"An error occured when Get one group:{group.Name} or it's users failed, error:{e.Message}, stack trace:{e.StackTrace}");
                }
            }
            return groupsInfo;
        }

        #region filter group has no permission
        private bool CheckGroupHasPermission(int groupId)
        {
            foreach (IAveRoleAssignment roleAssignment in m_Web.RoleAssignments)
            {
                if (roleAssignment.Member.ID.Equals(groupId))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        public List<AveGroupInfo> GetObjectData()
        {
            throw new NotImplementedException();
        }

        public object SetObjectData(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
