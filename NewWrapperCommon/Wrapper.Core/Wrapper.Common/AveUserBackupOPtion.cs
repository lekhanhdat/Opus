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
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public class AveUserBackupOption
    {
        private AveSiteUsersQueryOption userQueryOption;

        /// <summary>
        /// This Option Only For Site Users
        /// </summary>
        public AveSiteUsersQueryOption UserQueryOption
        {
            get { return userQueryOption; }
            set { userQueryOption = value; }
        }

        /// <summary>
        /// This Option Only For Web Users
        /// 现在认为 web上的user 应该只备份有权限的user ，但是改动影响比较大，本次修改只改动site，待下次修改时看看能不能
        /// 去掉这个option，只备份有权限的user
        /// </summary>
        public bool IncludeUsersWithoutSecurity
        {
            get;
            set;
        }
    }

    /// <summary>
    /// This Option Only For Site Users
    /// </summary>
    public enum AveSiteUsersQueryOption
    {
        /// <summary>
        /// All users, include deactive users and delete users
        /// 对应 includeUsersWithoutSecurity== ture
        /// </summary>
        AllUsers,
        /// <summary>
        /// only active users
        /// </summary>
        AllAvailableUsers,
        /// <summary>
        /// only have Security users
        /// 对应includeUsersWithoutSecurity == false
        /// </summary>
        OnlyHaveSecurityUsers,

    }
}
