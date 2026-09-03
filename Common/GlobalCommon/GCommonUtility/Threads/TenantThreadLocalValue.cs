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

using System.Security.Principal;
using System.Threading;
using AvePoint.GCommon.Contract.Server.Common;

namespace AvePoint.GCommon.Utility
{
    public class TenantThreadLocalValue
    {
        public static string LogonGroupId
        {
            get { return _logonGroupId.Value; }
            set { _logonGroupId.Value = value; }
        }

        public static string LogonUserId
        {
            get { return _logonUserId.Value; }
            set { _logonUserId.Value = value; }
        }

        public static string LogonUserName
        {
            get { return _logonUserName.Value; }
            set { _logonUserName.Value = value; }
        }

        public static string CurrentCulture
        {
            get { return _currentCulture.Value; }
            set { _currentCulture.Value = value; }
        }

        public static IPrincipal CurrentPrincipal
        {
            get { return _currentPrincipal.Value; }
            set { _currentPrincipal.Value = value; }
        }

        public static TenantGroupDBInfoDto PEGroupDBInfo
        {
            get { return _peGroupDBInfo.Value; }
            set { _peGroupDBInfo.Value = value; }
        }

        private static AsyncLocal<string> _logonGroupId = new AsyncLocal<string>();

        private static AsyncLocal<string> _logonUserId = new AsyncLocal<string>();

        private static AsyncLocal<string> _logonUserName = new AsyncLocal<string>();

        private static AsyncLocal<string> _currentCulture = new AsyncLocal<string>();

        private static AsyncLocal<IPrincipal> _currentPrincipal = new AsyncLocal<IPrincipal>();

        private static AsyncLocal<TenantGroupDBInfoDto> _peGroupDBInfo = new AsyncLocal<TenantGroupDBInfoDto>();
    }
}