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

namespace AvePoint.Wrapper.Common
{
    public class AveSecurityObject
    {
        public const int ScopeChangeId = -2;
        public const int RoleChangeId = -1;
        public SecurityType ObjectType { get; set; }
        public ChangeType ChangeType { get; set; }

        public int PrincipleId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public DateTime EventTime { get; set; }
        public Guid ScopeId { get; set; }
    }

    public class AveSiteMemberObject
    {
        public int PrincipleId { get; set; }
        public bool IsGroup { get; set; }
        public bool IsUser { get; set; }
        public string Title { get; set; }
        public string Login { get; set; }
        public bool IsDomainGroup { get; set; }
        public DateTime EventTime { get; set; }
        public ChangeType ChangeType { get; set; }
        public Dictionary<int, AveSiteMemberObject> AddedMemberIds { get; set; } //if add user to group, mean user ids
        public Dictionary<int, AveSiteMemberObject> DeletedMemberIds { get; set; } //if remove user from group, mean user ids
    }

    public enum SecurityType
    {
        None = 0,
        Role,
        Assignment,
        Scope,
    }
}
