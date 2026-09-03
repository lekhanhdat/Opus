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

namespace AvePoint.GCommon.Utility
{
    public class TokenParam
    {
        /// <summary>
        /// AOS tenant Id:Tenant group Id
        /// </summary>
        public string CustomerId { get; set; }
        /// <summary>
        /// Office 365 tenant Id
        /// </summary>
        public string TenantId { get; set; }
        /// <summary>
        /// 1. service account Id/service account user name in AOS 
        /// 2. AuthenticationProfile-Id
        /// </summary>
        public string Identity { get; set; }
        /// <summary>
        /// SharePoint site collection Ur
        /// </summary>
        public string SiteUrl { get; set; }
        /// <summary>
        /// Get SharePoint IDCRL or Bearer token
        /// </summary>
        public SharePointTokenType SpTokenType { get; set; }
        /// <summary>
        /// Get token for ADAL or MSAL
        /// </summary>
        public TokenMethod TokenMethod { get; set; }
        public AvePoint.GCommon.Contract.CentralAdmin.Object.AppType AppType { get; set; }

        public string Resource { get; set; }

        //exo client id
        public string ClientId { get; set; }
        public override string ToString()
        {
            return $"CustomerId:{CustomerId}, TenantId:{TenantId}, Identity:{Identity}, SiteUrl:{SiteUrl}, TokenType:{SpTokenType}, TokenMethod:{AvePoint.GCommon.Utility.TokenMethod.MSAL}, AppType:{AppType}";
        }
    }

    
}
