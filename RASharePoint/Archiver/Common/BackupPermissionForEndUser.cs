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
using System.Reflection;
using System.Text;
using AvePoint.GCommon;
using AvePoint.StorageOptimization.Schedule.Common;
using AvePoint.Wrapper.Backup;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
//using AvePoint.Adonis.StorageOptimization.Archiver.Object;

namespace AvePoint.RA.SharePoint.Archiver
{
    [AvePoint.GCommon.Contract.CodeReview.AveCodeReview(
    "2013/1/23",
    "yanlong.gu@AvePoint.com",
    "dongliang.liu@AvePoint.com",
    new string[]
    {
        CodeReviewConstants.CHECK_LIST_ID_EH_1,
        CodeReviewConstants.CHECK_LIST_ID_EH_2,
        CodeReviewConstants.CHECK_LIST_ID_DB_1,
        CodeReviewConstants.CHECK_LIST_ID_FA_1,
        CodeReviewConstants.CHECK_LIST_ID_FA_10,
        CodeReviewConstants.CHECK_LIST_ID_STREAM_1,
        CodeReviewConstants.CHECK_LIST_ID_HC_1,
        CodeReviewConstants.CHECK_LIST_ID_HC_2,
        CodeReviewConstants.CHECK_LIST_ID_THREAD_1,
        CodeReviewConstants.CHECK_LIST_ID_THREAD_2,
        CodeReviewConstants.CHECK_LIST_ID_LOG_1,
        CodeReviewConstants.CHECK_LIST_ID_LOG_2,
        CodeReviewConstants.CHECK_LIST_ID_LOG_3,
        CodeReviewConstants.CHECK_LIST_ID_LOG_4,
    },
    "ADO-60251",
    false
    )]
    internal class BackupPermissionForEndUser
    {
        private static readonly AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private long pemMask = 1856436900591;         //need set default Contribute perMask

        private List<int> roles;

        private Guid cacheScopeId;

        private const long systemAccount = 1073741823;

        /// <summary>
        /// you must call this function first
        /// </summary>
        /// <param name="customerPerMask"></param>
        private void SetPemMask(long customerPerMask)
        {
            pemMask = customerPerMask;
        }

        public BackupPermissionForEndUser(List<PermissionLevel> permissionLevels)
        {
            try
            {
                if (permissionLevels != null)
                {
                    long SpecialPermMask = 0;
                    foreach (PermissionLevel mPermissionLevel in permissionLevels)
                    {
                        SpecialPermMask |= mPermissionLevel.PermissionID;
                    }
                    SetPemMask(SpecialPermMask);
                }
            }
            catch (Exception e)
            {
                mLog.Warn("An error occurred while backing up permission for end user:{0}", e.ToString());
                throw;
            }
        }

        public void GetWebRoles(AveSPWeb aveWeb)
        {
            roles = new AveRoles(aveWeb).GetRoles().Where(info => (info.PermMask != null && CheckPemMask((long)info.PermMask))).Select(info => info.RoleId).ToList();
            //aveWeb.GetRoles().Where(info => (info.PermMask != null && CheckPemMask((long)info.PermMask))).Select(info => info.RoleId).ToList();
        }

        /// <summary>
        /// The node have unique permission should call this function
        /// </summary>
        /// <param name="roleAssignmentInfo"></param>
        /// <returns></returns>
        public EndUserPermission GetEndUserPermssion(IAveRoleAssignmentCollection roleAssignments)
        {
            try
            {
                if (pemMask == 0)
                {
                    throw new Exception("You must call SetPerMask for set customer PerMask.");
                }
                if (cacheScopeId == roleAssignments.ID)
                {
                    return new EndUserPermission() { isInheritPermission = true, users = new List<string>() };//scopeId = roleAssignments.ID, 
                }

                EndUserPermission permission = new EndUserPermission();
                permission.isInheritPermission = false;
                //permission.scopeId = roleAssignments.ID;

                foreach (var roleAssignment in roleAssignments)
                {
                    foreach (var role in roleAssignment.RoleDefinitionBindings)
                    {
                        if ((roleAssignment.Member.ID == systemAccount) || roles.Contains(role.ID))
                        {
                            if (roleAssignment.Member is IAveUser)
                            {
                                permission.AddUser(roleAssignment.Member.LoginName);
                            }
                            else
                            {
                                var group = roleAssignment.Member as IAveGroup;
                                foreach (var user in group.Users)
                                {
                                    permission.AddUser(user.LoginName);
                                }
                            }
                        }
                    }
                }
                cacheScopeId = roleAssignments.ID;
                return permission;
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while getting end user permission:{0}", e.ToString());
                throw;
            }
        }

        private bool CheckPemMask(long itemPemMask)
        {
            return ((itemPemMask & pemMask) == pemMask);
        }
    }

    internal class EndUserPermission
    {
        //public Guid scopeId { get; set; }  //SAAS-13814 ScopeId由RoleAssignments的Id获得，但是目前Client API不支持该属性，该属性在Archiver的Header中，暂时没有用到，所以注释掉

        public bool isInheritPermission { get; set; }

        public List<string> users = new List<string>();

        public void AddUser(string user)
        {
            if (!users.Contains(user))
                users.Add(user);
        }

        public string GetUserString()
        {
            if (users.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(";");
            foreach (string user in users)
            {
                sb.Append(user);
                sb.Append(";");
            }
            return sb.ToString();
        }
    }
}
