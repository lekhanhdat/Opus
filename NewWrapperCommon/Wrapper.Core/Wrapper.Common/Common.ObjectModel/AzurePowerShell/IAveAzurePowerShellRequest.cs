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
    public interface IAveAzurePowerShellRequest:IDisposable
    {
        Dictionary<string, object> GetSecurityGroups();

        IAveO365User GetUser(string userPrincipalName);

        IAveO365Group GetGroup(string groupName, string email);

        Guid GetGroupObjectIdByName(string groupName);

        Dictionary<string, object> GetUsersFromSecurityGroup(string groupName);
        /// <summary>
        /// 此接口只给check role用。 TODO: Browser里只是临时改动。将来需要修改和control的接口，把int值返回给control，由control来判断当前user的role.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        int GetUserRole(string userName);
        List<Dictionary<string, object>> GetOffice365Domains();

        string GetOffice365Domain();

        bool IsSmallBusinessSubscription();

        bool IsGlobalAdmin(string currentUserName);

        string GetOffice365AdminSiteCollectionUrl();

        void Dispose();
        List<Dictionary<string, object>> GetOffice365UserDetailsForUserSeat();
    }
}
