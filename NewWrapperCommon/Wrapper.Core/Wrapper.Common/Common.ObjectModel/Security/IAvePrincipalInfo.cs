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
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public interface IAvePrincipalInfo
    {
        string Department { get; }
        string DisplayName { get; }
        string JobTitle { get; }
        string Email { get; }
        string LoginName { get; }
        string Mobile { get; }
        AvePrincipalType PrincipalType { get; }
        int PrincipalID { get; }
    }

    public enum AvePrincipalSource
    {
        // Summary:
        //     Do not specify a source.
        None = 0,
        //
        // Summary:
        //     Use the user information list as the source.
        UserInfoList = 1,
        //
        // Summary:
        //     Use Windows as the source.
        Windows = 2,
        //
        // Summary:
        //     Use the membership provider as the source.
        MembershipProvider = 4,
        //
        // Summary:
        //     Use the role provider as the source
        RoleProvider = 8,
        //
        // Summary:
        //     Use all sources.
        All = 15,
    }

}
