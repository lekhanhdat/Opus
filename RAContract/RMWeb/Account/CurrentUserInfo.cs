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
using AvePoint.RA.Contract.RMWeb.CP;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.Account
{
    public class CurrentUserInfo
    {
        public string SessionId { get; set; }

        public int SessionOut { get; set; }

        public string AccountId { get; set; }

        //public string UserSID { get; set; }

        public string LoginName { get; set; }

        public string DisplayName { get; set; }

        public RMAccountType AccountType { get; set; }

        public int GerneralSettingId { get; set; }

        public string RegisterEmail { get; set; }
        public string TenantGroupId { get; set; }
        public RMAuthenticationTypes AuthType { get; set; }

        public string PermissionMark { get; set; }

        public string Company { get; set; }

        public string AccountNumber { get; set; }
        public string PartnarUser { get; set; }
        public string PartnarOwner { get; set; }
        
    }
}
