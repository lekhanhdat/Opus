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
using System.Threading.Tasks;

namespace AvePoint.Wrapper.Common
{
    public interface IAveObjectSharingInformationUser
    {
        string CustomRoleNames { get; }
        string Department { get; }
        string Email { get; }
        bool HasEditPermission { get; }
        bool HasViewPermission { get; }
        int Id { get; }
        bool IsDomainGroup { get; }
        bool IsSiteAdmin { get; }
        string JobTitle { get; }
        string LoginName { get; }
        string Name { get; }
        string Picture { get; }
        IAvePrincipal Principal { get; }
        string SipAddress { get; }
        IAveUser User { get; } 
    }
}
