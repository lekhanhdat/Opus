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
namespace AvePoint.RA.Contract.RMWeb.Account
{
    public class RMADAccountDto
    {
        public int Id { get; set; }

        public string LoginName { get; set; }

        public string DisplayName { get; set; }

        public string Domain { get; set; }

        public int DomainId { get; set; }

        public string AccountSID { get; set; }

        public RMAccountType Type { get; set; }

        public RMAccountStatus Status { get; set; }
    }

    public struct RMActiveDirectoryPropertyNames
    {
        public const string DISPLAY_NAME = "displayName";
        public const string SAMACCOUNTNAME = "samaccountname";
        public const string FIRSTNAME = "givenname";
        public const string LASTNAME = "sn";
        public const string MAIL = "mail";
        public const string USER_PRINCIPAL_NAME = "userprincipalname";
    }
}
