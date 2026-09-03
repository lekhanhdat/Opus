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

namespace AvePoint.GCommon.Utility.Exceptions
{
    /// <summary>
    /// 在AccountMapping中找不到Id对应的记录
    /// </summary>
    [Serializable]
    public class AccountNotFoundException : AveException
    {
        public AccountNotFoundException(string id)
            : base(id)
        {
        }
    }

    /// <summary>
    /// 在Group中找不到GroupId对应的记录
    /// </summary>
    [Serializable]
    public class GroupNotFoundException : AveException
    {
        public GroupNotFoundException(string groupId)
            : base(groupId)
        {
        }
    }

    /// <summary>
    /// 在Plan中找不到PlanId对应的记录
    /// </summary>
    [Serializable]
    public class PlanNotFoundException : AveException
    {
        public PlanNotFoundException(string planId)
            : base(planId)
        {
        }
    }

    /// <summary>
    /// 在OjbectInfo中找不到ObjectId对应的记录
    /// </summary>
    [Serializable]
    public class ObjectNotFoundException : AveException
    {
        public ObjectNotFoundException(string objectId)
            : base(objectId)
        {
        }
    }

    /// <summary>
    /// User不在Group中
    /// </summary>
    [Serializable]
    public class UserNotInGroupException : AveException
    {
        public UserNotInGroupException(string userId)
            : base(string.Format("UserId {0}", userId))
        {
        }

        public UserNotInGroupException(string userId, string groupId)
            : base(string.Format("UserId {0}, GroupId {1}", userId, groupId))
        {
        }
    }

    /// <summary>
    /// Database中数据出现一致性问题
    /// </summary>
    [Serializable]
    public class InvalidDataException : AveException
    {
        public InvalidDataException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// 在User对应的Role
    /// </summary>
    [Serializable]
    public class UserRoleNotFoundException : AveException
    {
        public UserRoleNotFoundException(string userId)
            : base(userId)
        {
        }
    }
}
