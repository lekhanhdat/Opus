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
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMSecurityGroupMembershipDao : IBaseDao<RMSecurityGroupMembership>
    {
        /// <summary>
        /// To Do contrim how to show users edit or remove users.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name=""></param>
        List<RMSecurityGroupMembership> CreateOrUpdateGroupMemberShips(int groupId, List<string> userIds);

        ///// <summary>
        ///// remove all securtiy memberships when remove group.
        ///// </summary>
        ///// <param name="groupId"></param>
        //void DeleteGroupMemberShips(int groupId);

        //get all permission masks ,user belongs to different groups.
        List<long> GetAllRolesByUser(List<string> userIds);

        /// <summary>
        /// add users to securtiy memberships if doesn't exist.
        /// </summary>
        void AddUsersToGroupMemberShips(int groupId, List<string> userIds);

        /// <summary>
        /// add user to securtiy memberships if current user doesn't have this groupId(permission).
        /// </summary>
        void AddUserToGroupMemberShips(int groupId, string userId);

        /// <summary>
        /// Judge if the user is in the group
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool IsUserInGroup(int groupId, string userId);

        /// <summary>
        /// Update all the same UserIDs to the specified groupId(permission).
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        void AddOrUpdateAllSameUserToGroupMemberShips(int groupId, string userId);
        void RemoveUserGroupMemeberships(int groupId, string userId);//For sync user from AOS,remove it when change user role is AOS.
        /// <summary>
        /// Get all permission groups id contains specified user.
        /// </summary> 
        /// <returns></returns>
        List<int> GetAllGroupIds(List<string> userAndGroupIds);

        void AddOrUpdateUserToGroupMemberShips(int groupId, List<string> userIds);
        /// <summary>
        /// get all sub permission masks
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        List<long> GetSubPermissionMasksByUser(List<string> userIds);
        List<PermissionMask> GetAllPermissoinsByUser(List<string> userIds);
        List<bool> GetAllGroupStatusByUser(List<string> userIds);
    }
}
