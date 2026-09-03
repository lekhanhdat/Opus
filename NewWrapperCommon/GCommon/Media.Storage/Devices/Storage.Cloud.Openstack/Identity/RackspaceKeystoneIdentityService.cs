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
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.Media.Storage.Cloud.OpenStack
{
    class RackspaceKeystoneIdentityService : KeystoneIdentityService
    {

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "rax")]
        protected override void InitIdentityInfo()
        {
            Hashtable authInfoTable = new Hashtable();
            authInfoTable.Add("auth.RAX-KSKEY:apiKeyCredentials.username", openParameter.UserName);
            authInfoTable.Add("auth.RAX-KSKEY:apiKeyCredentials.apiKey", openParameter.Password);
            if (!string.IsNullOrEmpty(openParameter.TenantName))
            {
                authInfoTable.Add("auth.tenantName", openParameter.TenantName);
            }
            if (!string.IsNullOrEmpty(openParameter.TenantId))
            {
                authInfoTable.Add("auth.tenantId", openParameter.TenantId);
            }
            string authRequestString = JsonConvertor.GenJsonString(authInfoTable);
            openStackIdentityInfo.AuthRequestString = authRequestString;
            openStackIdentityInfo.AuthenticationURL = openParameter.AuthenticationURL;

            openStackIdentityInfo.RegionJosnPath = "access.user.RAX-AUTH:defaultRegion".Split(new char[] { '.' });
            openStackIdentityInfo.CDNEndpointType = "rax:object-cdn";
        }
    }
}
