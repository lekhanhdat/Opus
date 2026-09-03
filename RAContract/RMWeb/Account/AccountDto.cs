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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.Account
{
    public class AccountDto
    {
        public int Id { set; get; }
        public string UserId { set; get; }
        public string UserPrincipalName { get; set; }
        public string DisplayName { get; set; }
        public RMActiveDirectoryObjectType ObjectType { get; set; }
        public RMAccountType AccountType { get; set; }
        public string Email { get; set; }
        public long LastModifiedTime { get; set; }
        public int IsRemoved { get; set; }
        public string AADId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class UserQueryResult
    {
        public List<SecurityUserDto> Users { get; set; }
        public int TotalCount { get; set; }
    }
    [DataContract]
    public class UserQueryParams
    {
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public int PageIndex { get; set; }
        [DataMember]
        public string SearchValue { get; set; }
        [DataMember]
        public string SortBy { get; set; }
        [DataMember]
        public bool IsAscending { get; set; }
    }
}
