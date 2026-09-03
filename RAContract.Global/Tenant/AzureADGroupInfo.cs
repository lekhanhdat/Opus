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
using System.Collections.Generic;

namespace AvePoint.RA.Contract.Tenant
{
    public class AzureADGroupInfo
    {
        public string ObjectId { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public int IdentityType { get; set; }

        public bool IsActive { get; set; }

        public List<PostRole> Roles { get; set; }

        public string DomainName { get; set; }

        public string ParentGroupId { get; set; }

        public List<string> ParentGroupIds { get; set; }
    }

    public class PostRole
    {
        public string ApplicationName { get; set; }

        public string Url { get; set; }

        public bool IsAcceptedLicenseAgreement { get; set; }

        public int UserType { get; set; }
    }
}
