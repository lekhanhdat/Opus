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
using AvePoint.RA.Contract.Object;
using Cloud.Sdk.Data.Aos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudAos = Cloud.Sdk.Data.AosModern;

namespace AvePoint.RA.Common.Aos.Util
{
    public class AADAccountConverter
    {
        public static List<CloudAos.O365UserInfo> Convert(List<AADAccount> accounts, string inviter)
        {
            return accounts.Select(o => Convert(o, inviter)).ToList();
        }

        private static CloudAos.O365UserInfo Convert(AADAccount o, string inviter)
        {
            var user = new CloudAos.O365UserInfo()
            {
                InviteType = o.InviteType == Contract.Object.AccountType.Group ? CloudAos.InviteType.Group : CloudAos.InviteType.User,
                ObjectId = o.Id,
                Name = o.Mail,
                FirstName = o.GivenName,
                Inviter = inviter,
                //Roles = new List<string> { "id_tenant_RevIM" },
                LastName = o.SurName,
                Email = o.Mail,
            };

            if (o.InviteType == Contract.Object.AccountType.Group)
            {
                if (!string.IsNullOrEmpty(o.Mail))
                {
                    user.Email = o.Mail;
                }
                else
                {
                    user.Email = o.DisplayName;
                }
                user.Name = o.DisplayName;
            }

            return user;
        }

    }
}
