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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.Common
{
    public class AuthenticationHandler
    {
        private string url;
        private AveBPOSAccountInfo accountInfo;
        //change for Cloud Records On-Premise.
        private Dictionary<AuthenticationModeOption, IAuthenticationProvider> defaultProviders = new Dictionary<AuthenticationModeOption, IAuthenticationProvider>
        {
            { AuthenticationModeOption.Windows, new WindowsAuthenticationProvider() },
            //{ AuthenticationModeOption.Forms, new FormAuthenticationProvider() },
            //{ AuthenticationModeOption.Claims, new ClaimsAuthenticationProvider() },
            { AuthenticationModeOption.ADFS,new ADFSAuthenticationProvider() },
            //{ AuthenticationModeOption.Online, new OnlineAuthenticationProvider() },
            //{ AuthenticationModeOption.OnlineGraphToken, new OnlineGraphAuthenticationProvider()},
        };

        static AuthenticationHandler()
        {
            InitProviders();
        }

        //ExtentionMethod by configuration file
        private static void InitProviders()
        {
        }

        public AuthenticationHandler(string url, AveBPOSAccountInfo accountInfo)
        {
            this.url = url;
            this.accountInfo = accountInfo;
        }

        public AuthenticationResult GetAuthenticationResult(AuthenticationModeOption[] options)
        {
            foreach (var option in options)
            {
                if(defaultProviders.ContainsKey(option))
                {
                    var result = defaultProviders[option].Login(this.url, this.accountInfo);
                    if (result.Status == AutheStatus.Successful)
                    {
                        return result;
                    }
                }
            }
            return new AuthenticationResult(AutheStatus.Skip, AveAuthenticationMode.None);
        }
    }
}
