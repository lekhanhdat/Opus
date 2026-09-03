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



using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Administration;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveWindowsAuthenticationProvider : AveAuthenticationProvider, IAveWindowsAuthenticationProvider
    {
        private SPWindowsAuthenticationProvider mWindowsAuthenticationProvider;

        public AveWindowsAuthenticationProvider(SPWindowsAuthenticationProvider windowsAuthenticationProvider)
            : base(windowsAuthenticationProvider)
        {
            mWindowsAuthenticationProvider = windowsAuthenticationProvider;
        }

        public AveWindowsAuthenticationProvider()
            : this(new SPWindowsAuthenticationProvider())
        { }

        public bool DisableKerberos
        {
            get
            {
                return mWindowsAuthenticationProvider.DisableKerberos;
            }
            set
            {
                mWindowsAuthenticationProvider.DisableKerberos = value;
            }
        }

        public bool UseBasicAuthentication
        {
            get
            {
                return mWindowsAuthenticationProvider.UseBasicAuthentication;
            }
            set
            {
                mWindowsAuthenticationProvider.UseBasicAuthentication = value;
            }
        }

        public bool UseWindowsIntegratedAuthentication
        {
            get
            {
                return mWindowsAuthenticationProvider.UseWindowsIntegratedAuthentication;
            }
            set
            {
                mWindowsAuthenticationProvider.UseWindowsIntegratedAuthentication = value;
            }
        }
    }
}
